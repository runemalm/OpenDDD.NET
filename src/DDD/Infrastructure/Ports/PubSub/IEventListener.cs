﻿using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain;
using DDD.Domain.Model;

namespace DDD.Infrastructure.Ports.PubSub
{
	public interface IEventListener
	{
		public abstract Context Context { get; }
		public abstract string ListensTo { get; }
		public abstract DomainModelVersion ListensToVersion { get; }

		Task StartAsync();
		Task StopAsync();
		Task<bool> React(IPubSubMessage message);
		Task Handle(IPubSubMessage message);
	}
}