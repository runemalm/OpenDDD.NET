using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using OpenDDD.Application;
using OpenDDD.Main;
using OpenDDD.NET.Extensions;
using MyBoundedContext.Domain.Model;
using MyBoundedContext.Domain.Model.Site;

namespace MyBoundedContext.Main
{
	public class EnsureSitesTask : IEnsureDataTask
	{
		/*
	     * Create the sites at application startup,
		 * if not already exists.
	     */
		private readonly ILogger _logger;
		private readonly ISiteRepository _siteRepository;

		public EnsureSitesTask(ISiteRepository siteRepository, ILogger<EnsureSitesTask> logger)
		{
			_siteRepository = siteRepository;
			_logger = logger;
		}
		
		public void Execute(ActionId actionId, CancellationToken ct)
		{
			_logger.LogDebug("Executing ensure sites task..");
			
			var expected = new List<Site>
			{
				new Site
				{
					DomainModelVersion = DomainModelVersion.Latest(), 
					SiteId = SiteId.Create(EnumExtensions.GetDescriptionFromEnumValue(SiteId.Strings.Idealista)),
					Name = "Idealista"
				},
				new Site
				{
					DomainModelVersion = DomainModelVersion.Latest(), 
					SiteId = SiteId.Create(EnumExtensions.GetDescriptionFromEnumValue(SiteId.Strings.ThailandProperty)),
					Name = "Property Search"
				}
			};
			
			var existing = _siteRepository.GetAll(actionId, ct);

			foreach (var e in expected)
			{
				if (!existing.Contains(e))
				{
					_siteRepository.Save(e, actionId, ct);
				}
			}
			
			_logger.LogDebug("..task executed.");
		}
	}
}
