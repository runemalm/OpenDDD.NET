using DDD.Application.Settings;
using DDD.Domain.Model.Auth;
using DDD.Domain.Services.Auth;
using DDD.Infrastructure.Ports.Email;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Infrastructure.Services.Persistence;
using DDD.Logging;
using Application.Settings;
using Domain.Model.Forecast;
using Domain.Model.Summary;

namespace Application.Actions
{
	public sealed class ActionDependencies
	{
		public readonly IAuthDomainService AuthDomainService;
		public readonly IDomainPublisher DomainPublisher;
		public readonly IInterchangePublisher InterchangePublisher;
		public readonly IOutbox Outbox;
		public readonly IPersistenceService PersistenceService;
		public readonly ISettings Settings;
		public readonly ILogger Logger;
		public readonly ICredentials Credentials;
		public readonly ICustomSettings CustomSettings;
		public readonly IForecastDomainService ForecastDomainService;
		public readonly IForecastRepository ForecastRepository;
		public readonly ISummaryRepository SummaryRepository;
		public readonly IEmailPort EmailAdapter;
		public readonly IIcForecastTranslator IcForecastTranslator;
		
		public ActionDependencies(
			IAuthDomainService authDomainService,
			IDomainPublisher domainPublisher,
			IInterchangePublisher interchangePublisher,
			IOutbox outbox,
			IPersistenceService persistenceService,
			ISettings settings,
			ILogger logger,
			ICredentials credentials,
			ICustomSettings customSettings,
			IForecastDomainService forecastDomainService,
			IForecastRepository forecastRepository,
			ISummaryRepository summaryRepository,
			IEmailPort emailAdapter,
			IIcForecastTranslator icForecastTranslator)
		{
			AuthDomainService = authDomainService;
			DomainPublisher = domainPublisher;
			InterchangePublisher = interchangePublisher;
			Outbox = outbox;
			PersistenceService = persistenceService;
			Settings = settings;
			Logger = logger;
			Credentials = credentials;
			CustomSettings = customSettings;
			ForecastDomainService = forecastDomainService;
			ForecastRepository = forecastRepository;
			SummaryRepository = summaryRepository;
			EmailAdapter = emailAdapter;
			IcForecastTranslator = icForecastTranslator;
		}
	}
}
