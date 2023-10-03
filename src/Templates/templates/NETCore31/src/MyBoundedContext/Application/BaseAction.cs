using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenDDD.Application;
using OpenDDD.Application.Error;
using OpenDDD.Domain.Model.Error;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Services.Outbox;
using OpenDDD.NET;
using OpenDDD.NET.Extensions;
using OpenDDD.NET.Services.DatabaseConnection;
using MyBoundedContext.Domain.Model.Site;

namespace MyBoundedContext.Application
{
	public abstract class BaseAction<TCommand, TReturns> : OpenDDD.Application.BaseAction<TCommand, TReturns>
		where TCommand : ICommand
	{
		protected readonly IIdealistaPort _idealistaAdapter;
		protected readonly IThailandPropertyPort _thailandPropertyAdapter;
		protected readonly IDateTimeProvider _dateTimeProvider;
		protected readonly IDomainPublisher _domainPublisher;

		public BaseAction(
			IActionDatabaseConnection actionDatabaseConnection, 
			IOutbox outbox,
			IIdealistaPort idealistaAdapter,
			IThailandPropertyPort thailandPropertyAdapter, 
			IDateTimeProvider dateTimeProvider, 
			IDomainPublisher domainPublisher,
			ILogger<BaseAction<TCommand, TReturns>> logger) : base(actionDatabaseConnection, outbox, logger)
		{
			_idealistaAdapter = idealistaAdapter;
			_thailandPropertyAdapter = thailandPropertyAdapter;
			_dateTimeProvider = dateTimeProvider;
			_domainPublisher = domainPublisher;
		}

		protected ISitePort GetSiteAdapterOrThrow(SiteId siteId)
		{
			ISitePort adapter;
			
			if (siteId == SiteId.Create(EnumExtensions.GetDescriptionFromEnumValue(SiteId.Strings.Idealista)))
				adapter = _idealistaAdapter;
			else if (siteId == SiteId.Create(EnumExtensions.GetDescriptionFromEnumValue(SiteId.Strings.ThailandProperty)))
				adapter = _thailandPropertyAdapter;
			else
				throw ApplicationException.InvalidCommand($"There's no adapter for the site with ID: {siteId}.");
			
			return adapter;
		}
		
		protected async Task<Site> GetAggregateOrThrowAsync(SiteId siteId, ISiteRepository repository, ActionId actionId, CancellationToken ct)
		{
			var site = await repository.GetAsync(siteId, actionId, ct);
				
			if (site == null)
				throw DomainException.NotFound("site", siteId);
			
			return site;
		}
	}
}
