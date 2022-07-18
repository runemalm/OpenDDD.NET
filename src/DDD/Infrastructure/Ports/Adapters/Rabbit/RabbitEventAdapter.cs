using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDD.Domain;
using DDD.Infrastructure.Ports.Adapters.Rabbit.Exceptions;
using DDD.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DDD.Infrastructure.Ports.Adapters.Rabbit
{
	public class RabbitEventAdapter : EventAdapter
	{
		private IConnection _conn;
		private IDictionary<string, IModel> _publisherChannels = new Dictionary<string, IModel>();
		private IDictionary<string, IModel> _listenerChannels = new Dictionary<string, IModel>();

		private string _host;
		private int _port;
		private string _username;
		private string _password;

		private bool _isConnected = false;
		private bool _isReconnecting = false;

		private const int _publishConfirmTimeoutSecs = 5;

		public RabbitEventAdapter(
			string topic,
			string client,
			bool listenerAcksRequired,
			bool publisherAcksRequired,
			string host,
			int port,
			ILogger logger,
			IMonitoringPort monitoringAdapter,
			string username = "guest",
			string password = "guest") :
			base(topic, client, listenerAcksRequired, publisherAcksRequired, logger, monitoringAdapter)
		{
			_host = host;
			_port = port;
			_username = username;
			_password = password;
		}

		public override async Task StartAsync()
		{
			Connect();

			IsStarted = true;

			await Task.Yield();
		}

		public override async Task StopAsync()
		{
			IsStopping = true;

			CloseListenerChannels();
			ClosePublisherChannels();

			Disconnect();

			IsStarted = false;
			IsStopping = false;

			await Task.Yield();
		}

		protected void Connect()
		{
			ConnectionFactory factory = new ConnectionFactory();

			factory.UserName = _username;
			factory.Password = _password;
			factory.Port = _port;
			factory.HostName = _host;
			factory.DispatchConsumersAsync = true;
			factory.AutomaticRecoveryEnabled = false;
			factory.RequestedHeartbeat = TimeSpan.FromSeconds(30);

			try
			{
				_conn = factory.CreateConnection();
				_conn.ConnectionShutdown +=
					async (ch, ea) =>
						await OnDisconnect(ch, ea);

				_isConnected = true;
			}
			catch (Exception e)
			{
				throw new RabbitException($"Couldn't create a connection, exception: '{e}'", e);
			}
		}

		protected void Disconnect()
		{
			_conn.Close();
			_isConnected = false;
		}

		private async Task OnDisconnect(object sender, ShutdownEventArgs eventArgs)
		{
			/*
			 * We have been disconnected from rabbit.
			 * 
			 * If this wasn't intentional, e.g. we were disconnected due to a 
			 * network segmentation, rabbit restarted, or similar, 
			 * we try to reconnect again (indefinitely).
			 */
			if (IsStarted && !IsStopping)
			{
				_isConnected = false;
				_isReconnecting = true;

				_logger.Log($"Rabbit disconnected, reconnecting..", LogLevel.Error);

				ClosePublisherChannels();
				CloseListenerChannels();

				var backoffSecs = 5;
				var totalSecs = 0;

				while (true)
				{
					try
					{
						Connect();
						OpenListenerChannels();

						_isReconnecting = false;
						_logger.Log($"Rabbit reconnected after {totalSecs} seconds.", LogLevel.Information);

						break;
					}
					catch (Exception e)
					{
						_logger.Log($"Rabbit failed to reconnect after {totalSecs} seconds.", LogLevel.Error, e);
						await Task.Delay(backoffSecs * 1000);
						totalSecs += backoffSecs;
					}
				}
			}
		}

		public override async Task<Subscription> SubscribeAsync(IEventListener listener)
		{
			/*
			 * Subscribe a listener to an event.
			 * 
			 * There's one exchange per event and the context has it's own 
			 * queue on that exchange.
			 * 
			 * We open a channel from which the listener will start consuming 
			 * from the context's queue of the event's exchange.
			 */
			var subscription = await base.SubscribeAsync(listener);

			var channel = OpenListenerChannel(listener);

			ConsumeChannel(channel, subscription);

			return subscription;
		}

		private AsyncEventHandler<BasicDeliverEventArgs> CreateHandler(IModel channel, IEventListener listener)
		{
			AsyncEventHandler<BasicDeliverEventArgs> handler =
				async (ch, ea) =>
				{
					try
					{
						var body = ea.Body.ToArray();

						var message = new RabbitMessage(channel, ea);

						if (ea.Redelivered)
							_logger.Log($"Received re-delivered {listener.ListensTo} " +
								$"({listener.ListensToVersion}) from " +
								$"{_context}.",
								LogLevel.Warning);
						else
							_logger.Log(
								$"Received {listener.ListensTo} " +
								$"({listener.ListensToVersion}) from " +
								$"{_context}.",
								LogLevel.Debug);

						await Handle(message, listener);
					}
					catch (Exception e)
					{
						_logger.Log(
							$"Handling of {listener.ListensTo} " +
							$"({listener.ListensToVersion}) from " +
							$"{_context} failed.",
							LogLevel.Error,
							e);
					}
				};

			return handler;
		}

		public void ConsumeChannel(IModel channel, Subscription subscription)
		{
			var consumer = new AsyncEventingBasicConsumer(channel);

			consumer.Received += CreateHandler(channel, subscription.Listener);

			channel.BasicConsume(
				queue: QueueName(subscription.EventName, subscription.DomainModelVersion),
				autoAck: false,
				consumer: consumer);
		}

		public IModel OpenListenerChannel(IEventListener listener)
			=> OpenListenerChannel(listener.ListensTo, listener.ListensToVersion);

		public IModel OpenListenerChannel(string eventName, IDomainModelVersion domainModelVersion)
		{
			var channel = OpenChannel(eventName, domainModelVersion);

			channel.QueueDeclare(
				QueueName(eventName, domainModelVersion),
				durable: false, // survive broker restart?
				exclusive: false, // used by one connection and deleted when that closes?
				autoDelete: true); // delete when last subscriber unsubscribes?

			channel.QueueBind(
				queue: QueueName(eventName, domainModelVersion),
				exchange: TopicForEvent(eventName, domainModelVersion),
				routingKey: "");

			_listenerChannels.Add($"{eventName}-{domainModelVersion}", channel);

			return channel;
		}

		private IModel OpenPublisherChannel(OutboxEvent outboxEvent)
			=> OpenPublisherChannel(outboxEvent.EventName, outboxEvent.DomainModelVersion);

		private IModel OpenPublisherChannel(string eventName, DomainModelVersion domainModelVersion)
		{
			var channel = OpenChannel(eventName, domainModelVersion);
			channel.ConfirmSelect();
			_publisherChannels.Add($"{eventName}-{domainModelVersion}", channel);
			return channel;
		}

		private IModel GetPublisherChannel(OutboxEvent outboxEvent)
		{
			foreach (var kvp in _publisherChannels)
				if (kvp.Key == $"{outboxEvent.EventName}-{outboxEvent.DomainModelVersion}")
					return kvp.Value;

			var channel = OpenPublisherChannel(outboxEvent);

			return channel;
		}

		public IModel OpenChannel(IEvent theEvent)
			=> OpenChannel(theEvent.EventName, theEvent.DomainModelVersion);

		public IModel OpenChannel(string eventName, IDomainModelVersion domainModelVersion)
		{
			if (!_isConnected)
				throw new RabbitException(
					"Must be connected before opening a channel.");

			var channel = _conn.CreateModel();

			channel.ConfirmSelect();

			channel.ExchangeDeclare(
				exchange: TopicForEvent(eventName, domainModelVersion),
				type: "fanout");

			return channel;
		}

		private void OpenListenerChannels()
		{
			foreach (var subscription in GetSubscriptions())
			{
				var channel =
					OpenListenerChannel(
						subscription.EventName,
						subscription.DomainModelVersion);

				ConsumeChannel(channel, subscription);
			}
		}

		private void CloseListenerChannels()
		{
			foreach (var pair in _listenerChannels)
				pair.Value.Close();

			_listenerChannels = new Dictionary<string, IModel>();
		}

		private void ClosePublisherChannels()
		{
			foreach (var pair in _publisherChannels)
				pair.Value.Close();

			_publisherChannels = new Dictionary<string, IModel>();
		}

		private string QueueName(Subscription subscription)
			=> TopicSubscriptionForEvent(subscription.EventName, subscription.DomainModelVersion);

		private string QueueName(IEvent theEvent)
			=> TopicSubscriptionForEvent(theEvent.EventName, theEvent.DomainModelVersion);

		private string QueueName(string eventName, IDomainModelVersion domainModelVersion)
			=> TopicSubscriptionForEvent(eventName, domainModelVersion);

		public async override Task AckAsync(IPubSubMessage message)
		{
			if (!(message is RabbitMessage))
				throw new RabbitException(
					"Expected IPubSubMessage to be a RabbitMessage. " +
					"Something must be wrong with the implementation.");
			else
			{
				var rabbitMessage = (RabbitMessage)message;
				rabbitMessage.Channel.BasicAck(rabbitMessage.EventArgs.DeliveryTag, false);
				await Task.Yield();
			}
		}

		public override Task FlushAsync(OutboxEvent outboxEvent)
		{
			var message = outboxEvent.JsonPayload;

			var body = Encoding.UTF8.GetBytes(message);

			var channel = GetPublisherChannel(outboxEvent);

			_monitoringAdapter.TrackDependency(
				() =>
				{
					channel.BasicPublish(
						exchange: TopicForEvent(outboxEvent.EventName, outboxEvent.DomainModelVersion),
						routingKey: "",
						basicProperties: null,
						body: body);

					try
					{
						channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, _publishConfirmTimeoutSecs));
					}
					catch (Exception e)
					{
						_logger.Log(
							$"RabbitMQ might have problems. " +
							$"The message we tried to publish wasn't confirmed by " +
							$"the broker within {_publishConfirmTimeoutSecs} seconds. " +
							$"Tried to publish on the " +
							$"'{TopicForEvent(outboxEvent.EventName, outboxEvent.DomainModelVersion)}' " +
							$"exchange. Ignoring the error now but you need to " +
							$"look it up.",
							LogLevel.Error,
							e);
					}

					return true;
				},
				"AMQP",
				"PublishEvent",
				message);

			base.FlushAsync(outboxEvent);

			return Task.CompletedTask;
		}
	}
}
