using DDD.Domain.Services.Auth;
using Domain.Model.Forecast;

namespace Domain.Services
{
	public class DomainService : DDD.Domain.Services.DomainService
	{
		protected readonly IAuthDomainService _authDomainService;
		protected readonly IForecastRepository _forecastRepository;

		public DomainService(DomainServiceDependencies deps) 
			: base(deps.Credentials, deps.Settings, deps.Logger)
		{
			_authDomainService = deps.AuthDomainService;
			_forecastRepository = deps.ForecastRepository;
		}
	}
}
