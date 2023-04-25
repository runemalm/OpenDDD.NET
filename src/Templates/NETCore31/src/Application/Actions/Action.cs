using DDD.Application;
using DDD.Application.Settings;
using DDD.Domain.Model.Auth;
using DDD.Infrastructure.Ports.Email;
using DDD.Logging;
using Application.Settings;
using Domain.Model.Forecast;
using Domain.Model.Summary;

namespace Application.Actions
{
	public abstract class Action<TCommand, TReturns> : DDD.Application.Action<TCommand, TReturns>
		where TCommand : ICommand
	{
		protected readonly ISettings _settings;
		protected readonly ILogger _logger;
		protected readonly ICredentials _credentials;
		protected readonly ICustomSettings _customSettings;
		protected readonly IForecastDomainService _forecastDomainService;
		protected readonly IForecastRepository _forecastRepository;
		protected readonly ISummaryRepository _summaryRepository;
		protected readonly IEmailPort _emailAdapter;
		protected readonly IIcForecastTranslator _icForecastTranslator;

		public Action(ActionDependencies deps)
			: base(
				deps.AuthDomainService, 
				deps.DomainPublisher, 
				deps.InterchangePublisher, 
				deps.Outbox, 
				deps.PersistenceService)
		{
			_settings = deps.Settings;
			_logger = deps.Logger;
			_credentials = deps.Credentials;
			_customSettings = deps.CustomSettings;
			_forecastDomainService = deps.ForecastDomainService;
			_forecastRepository = deps.ForecastRepository;
			_summaryRepository = deps.SummaryRepository;
			_emailAdapter = deps.EmailAdapter;
			_icForecastTranslator = deps.IcForecastTranslator;
		}
	}
}
