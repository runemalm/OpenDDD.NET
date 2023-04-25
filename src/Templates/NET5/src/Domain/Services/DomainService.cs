using DDD.Domain.Services.Auth;
using Domain.Model.Forecast;
using Domain.Model.Summary;

namespace Domain.Services
{
	public class DomainService : DDD.Domain.Services.DomainService
	{
		protected readonly IAuthDomainService _authDomainService;
		protected readonly IForecastRepository _forecastRepository;
		protected readonly ISummaryRepository _summaryRepository;

		public DomainService(DomainServiceDependencies deps) 
			: base(deps.Credentials, deps.Settings, deps.Logger)
		{
			_authDomainService = deps.AuthDomainService;
			_forecastRepository = deps.ForecastRepository;
			_summaryRepository = deps.SummaryRepository;
		}
	}
}
