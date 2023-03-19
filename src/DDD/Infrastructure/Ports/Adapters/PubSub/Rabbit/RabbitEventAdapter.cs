using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDD.Domain.Model;
using DDD.Domain.Model.BuildingBlocks.Event;
using DDD.Infrastructure.Ports.Adapters.Common.Exceptions;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Infrastructure.Ports.Monitoring;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DDD.Infrastructure.Ports.Adapters.PubSub.Rabbit
{
	public class RabbitEventAdapter : EventAdapter<RabbitSubscription>
	{
		private IConnection _conn;
		private IDictionary<string, IModel> _publisherChannels;

		private readonly string _host;
		private readonly int _port;
		private readonly string _username;
		private readonly string _password;

		private bool _isConnected;
		private bool _isReconnecting;

		private const int PublishConfirmTimeoutSecs = 5;

		public RabbitEventAdapter(
			string topic,
			string client,
			int maxDeliveryRetries,
			string host,
			int port,
			ILogger logger,
			IMonitoringPort monitoringAdapter,
			SerializerSettings serializerSettings,
			string username = "guest",
			string password = "guest") 
			: base(
				topic, 
				client, 
				maxDeliveryRetries,
				logger, 
				monitoringAdapter,
				serializerSettings)
		{
			_host = host;
			_port = port;
			_username = username;
			_password = password;

			_publisherChannels = new Dictionary<string, IModel>();
			_isConnected = false;
			_isReconnecting = false;
		}

		public override async Task StartAsync()
		{
			Connect();
			ConsumeAll();

			IsStarted = true;

			await Task.CompletedTask;
		}

		public override async Task StopAsync()
		{
			IsStopping = true;

			StopConsumingAll();
			StopPublishingAll();

			Disconnect();

			IsStarted = false;
			IsStopping = false;

			await Task.CompletedTask;
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
			if (_conn != null)
			{
				_conn.Close();
				_conn = null;
			}
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

				StopConsumingAll();
				StopPublishingAll();

				var backoffSecs = 5;
				var totalSecs = 0;

				while (true)
				{
					try
					{
						Connect();
						ConsumeAll();

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
			 * We open a channel and use it for the listener to consume 
			 * from the exchange queue.
			 */
			var subscription = GetSubscription(listener);
		
			if (subscription != null)
				throw new RabbitException(
					"Can't subscribe, we have already subscribed for that " +
					"event in this event adapter. Only one subscription " +
					"per event and adapter is allowed.");

			subscription = CreateSubscription(listener);
			
			if (IsStarted && _isConnected)
				Consume(subscription);
			
			AddSubscription(subscription);

			return subscription;
		}
		
		public override async Task UnsubscribeAsync(IEventListener listener)
		{
			var subscription = GetSubscription(listener);

			if (subscription.Channel != null)
				StopConsuming(subscription);

			RemoveSubscription(subscription);
		}
		
		private RabbitSubscription CreateSubscription(IEventListener listener)
		{
			var subscription = new RabbitSubscription(listener);
			return subscription;
		}
		
		private void Consume(RabbitSubscription subscription)
		{
			var channel = OpenListenerChannel(subscription);

			var consumer = new AsyncEventingBasicConsumer(channel);

			consumer.Received += CreateHandler(channel, subscription);

			subscription.ConsumerTag = 
				channel.BasicConsume(
					queue: QueueName(subscription.EventName, subscription.DomainModelVersion),
					autoAck: false,
					consumer: consumer);
		}
		
		private void ConsumeAll()
		{
			foreach (var sub in GetSubscriptions())
				Consume(sub);
		}

		private void StopConsuming(RabbitSubscription subscription)
		{
			if (subscription.ConsumerTag == null || subscription.Channel == null)
				throw new RabbitException(
					"Couldn't stop consuming. Seems like subscription wasn't consuming.");

			try
			{
				subscription.Channel.BasicCancel(subscription.ConsumerTag);
			}
			catch (Exception e)
			{
				
			}
			finally
			{
				subscription.ConsumerTag = null;
				subscription.Channel.Close();
				subscription.Channel = null;
			}
		}

		private void StopConsumingAll()
		{
			foreach (var sub in GetSubscriptions())
				StopConsuming(sub);
		}

		public override async Task FlushAsync(OutboxEvent outboxEvent)
		{
			while (IsStarted && _isReconnecting && !_isConnected)
			{
				_logger.Log(
					$"Can't flush event, waiting for a reconnect before proceeding..", 
					LogLevel.Warning);
				await Task.Delay(2000);
			}

			if (!IsStarted)
				throw new RabbitException("Can't flush event, rabbit event adapter is not started.");

			if (!_isConnected)
				throw new RabbitException("Can't flush event, rabbit event adapter is not connected.");

			// AssertPublishedBeforeFlushAsync(outboxEvent);
			
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
						channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, PublishConfirmTimeoutSecs));
					}
					catch (Exception e)
					{
						_logger.Log(
							$"RabbitMQ might have problems. " +
							$"The message we tried to publish wasn't confirmed by " +
							$"the broker within {PublishConfirmTimeoutSecs} seconds. " +
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

			await base.FlushAsync(outboxEvent);
		}
		
		// Helpers
		
		private AsyncEventHandler<BasicDeliverEventArgs> CreateHandler(IModel channel, Subscription subscription)
		{
			var listener = subscription.Listener;
			
			AsyncEventHandler<BasicDeliverEventArgs> handler =
				async (ch, ea) =>
				{
					try
					{
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

						await listener.Handle(message);
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
		
		public IModel OpenChannel(IEvent theEvent)
			=> OpenChannel(theEvent.Header.Name, theEvent.Header.DomainModelVersion);

		public IModel OpenChannel(string eventName, DomainModelVersion domainModelVersion)
		{
			if (!_isConnected)
				throw new RabbitException(
					"Must be connected before opening a channel.");

			var channel = _conn.CreateModel();

			channel.ExchangeDeclare(
				exchange: TopicForEvent(eventName, domainModelVersion),
				durable: true,
				type: "fanout");

			return channel;
		}
		
		private IModel GetPublisherChannel(OutboxEvent outboxEvent)
		{
			foreach (var kvp in _publisherChannels)
				if (kvp.Key == TopicForEvent(outboxEvent.EventName, outboxEvent.DomainModelVersion))
					return kvp.Value;

			var channel = OpenPublisherChannel(outboxEvent);

			return channel;
		}
		
		private IModel OpenPublisherChannel(OutboxEvent outboxEvent)
			=> OpenPublisherChannel(outboxEvent.EventName, outboxEvent.DomainModelVersion);

		private IModel OpenPublisherChannel(string eventName, DomainModelVersion domainModelVersion)
		{
			var channel = OpenChannel(eventName, domainModelVersion);
			channel.ConfirmSelect();
			_publisherChannels.Add(TopicForEvent(eventName, domainModelVersion), channel);
			return channel;
		}
		
		private void StopPublishingAll()
		{
			foreach (var pair in _publisherChannels)
				pair.Value.Close();

			_publisherChannels = new Dictionary<string, IModel>();
		}
		
		public IModel OpenListenerChannel(RabbitSubscription subscription)
		{
			var channel = OpenChannel(subscription.EventName, subscription.DomainModelVersion);

			channel.QueueDeclare(
				QueueName(subscription.EventName, subscription.DomainModelVersion),
				durable: true, // survive broker restart?
				exclusive: false, // used by one connection and deleted when that closes?
				autoDelete: false); // delete when last subscriber unsubscribes?

			channel.QueueBind(
				queue: QueueName(subscription.EventName, subscription.DomainModelVersion),
				exchange: TopicForEvent(subscription.EventName, subscription.DomainModelVersion),
				routingKey: "");

			subscription.Channel = channel;

			return channel;
		}

		private string QueueName(Subscription subscription)
			=> TopicSubscriptionForEvent(subscription.EventName, subscription.DomainModelVersion);

		private string QueueName(IEvent theEvent)
			=> TopicSubscriptionForEvent(theEvent.Header.Name, theEvent.Header.DomainModelVersion);

		private string QueueName(string eventName, DomainModelVersion domainModelVersion)
			=> TopicSubscriptionForEvent(eventName, domainModelVersion);

		public async override Task AckAsync(IPubSubMessage message)
		{
			if (!(message is RabbitMessage))
			{
				throw new RabbitException(
					"Expected IPubSubMessage to be a RabbitMessage. " +
					"Something must be wrong with the implementation.");
			}
			else
			{
				var rabbitMessage = (RabbitMessage)message;
				rabbitMessage.Channel.BasicAck(rabbitMessage.EventArgs.DeliveryTag, false);
				await Task.Yield();
			}
		}
	}
}
