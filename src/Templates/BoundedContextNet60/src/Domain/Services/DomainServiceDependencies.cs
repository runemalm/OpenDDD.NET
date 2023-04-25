using DDD.Application.Settings;
using DDD.Domain.Model.Auth;
using DDD.Domain.Services.Auth;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Logging;
using Domain.Model.Forecast;

namespace Domain.Services
{
	public sealed class DomainServiceDependencies
	{
		public readonly IDomainPublisher DomainPublisher;
		public readonly IInterchangePublisher InterchangePublisher;
		public readonly ICredentials Credentials;
		public readonly ISettings Settings;
		public readonly ILogger Logger;
		public readonly IAuthDomainService AuthDomainService;
		public readonly IForecastRepository ForecastRepository;

		public DomainServiceDependencies(
			IDomainPublisher domainPublisher,
			IInterchangePublisher interchangePublisher,
			ICredentials credentials,
			ISettings settings,
			ILogger logger,
			IAuthDomainService authDomainService,
			IForecastRepository forecastRepository)
		{
			DomainPublisher = domainPublisher;
			InterchangePublisher = interchangePublisher;
			Credentials = credentials;
			Settings = settings;
			Logger = logger;
			AuthDomainService = authDomainService;
			ForecastRepository = forecastRepository;
		}
	}
}
