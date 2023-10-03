using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenDDD.Application;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Ports.Adapters.Http;
using OpenDDD.Infrastructure.Ports.Adapters.Http.Common;
using OpenDDD.Infrastructure.Ports.Adapters.Http.NET;
using OpenDDD.NET;
using OpenDDD.NET.Extensions;
using OpenDDD.NET.Services.DatabaseConnection;
using OpenDDD.NET.Services.Outbox;
using MyBoundedContext.Domain.Model;
using MyBoundedContext.Domain.Model.Property;
using MyBoundedContext.Domain.Model.SearchResults;
using MyBoundedContext.Domain.Model.Site;
using MyBoundedContext.Infrastructure.Ports.Adapters.Http.Dto;
using MyBoundedContext.Tests;
using Xunit;

namespace MyBoundedContext.Vertical
{
	[Route("v1")]
	[Version("v1.0.0")]
	public class CrawlSearchPageController : NetHttpAdapter
	{
		private readonly IAction<CrawlSearchPage.Command, SearchResults> _action;
		private readonly HttpTranslation _httpTranslation;
		
		public CrawlSearchPageController(CrawlSearchPage.Action action, HttpTranslation httpTranslation)
		{
			_action = action;
			_httpTranslation = httpTranslation;
		}

		/// <remarks>
		/// Crawl a site's search page.
		/// </remarks>
		/// <returns>
		/// Returns the search result.
		/// </returns>
		[Public]
		[Section("Crawling")]
		[HttpPost("crawling/crawl-search-page")]
		[Returns(typeof(HttpTranslation.SearchResultsV1))]
		public async Task<IActionResult> CrawlSearchPageEndpoint([FromBody] CrawlSearchPage.Request request, CancellationToken ct)
		{
			var actionId = ActionId.Create();
			var command = _httpTranslation.FromV1(request);
			var result = await _action.ExecuteTrxAsync(command, actionId, ct);
			return Ok(_httpTranslation.ToV1(result));
		}
	}

	public static class CrawlSearchPage
	{
		public class Request : RequestBase
		{
			public string SiteId { get; set; }
		}

		public class Command : CommandBase
		{
			public SiteId SiteId { get; set; }
		}

		public class Action : Application.BaseAction<Command, SearchResults>
		{
			private readonly ISiteRepository _siteRepository;

			public Action(
				IActionDatabaseConnection actionDatabaseConnection,
				IActionOutbox outbox,
				ISiteRepository siteRepository,
				IIdealistaPort idealistaAdapter,
				IThailandPropertyPort thailandPropertyAdapter, 
				IDateTimeProvider dateTimeProvider, 
				IDomainPublisher domainPublisher,
				ILogger<Action> logger) 
				: base(actionDatabaseConnection, outbox, idealistaAdapter, thailandPropertyAdapter, dateTimeProvider, domainPublisher, logger)
			{
				_siteRepository = siteRepository;
			}

			public override async Task<SearchResults> ExecuteAsync(Command command, ActionId actionId, CancellationToken ct)
			{
				var site = await GetAggregateOrThrowAsync(command.SiteId, _siteRepository, actionId, ct);
				
				var siteAdapter = GetSiteAdapterOrThrow(command.SiteId);

				var searchResults = await site.CrawlSearchPageAsync(siteAdapter, _dateTimeProvider, _domainPublisher, actionId, ct);

				await _siteRepository.SaveAsync(site, actionId, ct);

				return searchResults;
			}
		}
		
		public class SearchPageCrawled : DomainEvent
		{
			public SiteId SiteId { get; set; }
			public SearchResults SearchResults { get; set; }

			public SearchPageCrawled()
			{
				
			}

			public SearchPageCrawled(SiteId siteId, SearchResults searchResults, IDateTimeProvider dateTimeProvider, ActionId actionId) 
				: base("SearchPageCrawled", DomainModelVersion.Latest(), "Search", dateTimeProvider, actionId)
			{
				SiteId = siteId;
				SearchResults = searchResults;
			}
		}
		
		[Collection("Group A")]
	    public class CrawlSearchPageTests : BaseActionUnitTests
	    {
		    [Fact]
	        public async Task TestSuccess_Idealista_Result_And_Events()
	        {
	            // Arrange
	            var siteId = SiteId.Create(EnumExtensions.GetDescriptionFromEnumValue(SiteId.Strings.Idealista));
	            var site = Site.Create(siteId, "Test Site", ActionId);
	            await SiteRepository.SaveAsync(site, ActionId, CancellationToken.None);
	            
	            MockAllAdaptersSearchPageResponse = GetHtmlDocument("Vertical/TestFiles/idealista-searchpage1.html");

	            var expected = SearchResults.Create(DateTimeProvider.Now, new List<SearchPage>
	            {
		            SearchPage.Create(new List<Property>()
		            {
			            Property.Create(
				            PropertyId.Create("prop-1"), 
				            "Flat in avenida Manuel Mena Palma, 2, Cortijo Torrequebrada, Benalmádena",
				            @"仅在 6 月至 10 月期间提供数周和两周的度假租赁服务。
整个月 8 月 2300，两周 1500
海滨公寓，可欣赏大海和山脉的壮丽景色。带大露台，非常明亮。它有一张双人床和一张沙发床。
私人城市化，有四个游泳池和绿地，可直接通往海滩和社区停车场。.",
				            Price.Create(800, Currency.Euro),
				            Location.Create("Costa del Sol", 0.0, 0.0),
				            PropertyType.Condo,
				            ActionId),
			            Property.Create(
				            PropertyId.Create("prop-2"), 
				            "Terraced house in La Carolina-Guadalpín, Marbella",
				            "Wonderful townhouse for rent for long term. On the golden mile next to the Piruli of Marbella and the Corte Inglés in the center. Located in a privileged and quiet area surrounded by nature and large gardens. With the possibility of walking to the beach or the center and with all the services just a",
				            Price.Create(2750, Currency.Euro),
				            Location.Create("Costa del Sol", 0.0, 0.0),
				            PropertyType.Condo,
				            ActionId),
			            Property.Create(
				            PropertyId.Create("prop-3"), 
				            "Detached house in avenida de España, 1, Mijas Golf, Mijas",
				            @"VILLA MIJAS GOLF, LONGLET AVAILABLE FROM SEPTEMBER 2023 5 year lease
Living space600 m²
Land space1 400 m²
Number of bedrooms5
Number of bathrooms4
This villa should be seen in real as pictures do not really tell the whole story!
The distribution is as follows:
- First floor: living room, dining",
				            Price.Create(5000, Currency.Euro),
				            Location.Create("Costa del Sol", 0.0, 0.0),
				            PropertyType.Condo,
				            ActionId)
		            })
	            });

	            // Act
	            var command = new Command
	            {
		            SiteId = siteId
	            };

	            var actual = await CrawlSearchPageAction.ExecuteAsync(command, ActionId, CancellationToken.None);

	            // Assert
	            AssertEntity(expected, actual);
	            AssertDomainEventPublished(new SearchPageCrawled(siteId, actual, DateTimeProvider, ActionId));
	        }
	        
	        [Fact]
	        public async Task TestSuccess_Idealista_Http_Response_Ok()
	        {
		        // Arrange
		        var siteId = SiteId.Create(EnumExtensions.GetDescriptionFromEnumValue(SiteId.Strings.Idealista));
		        var site = Site.Create(siteId, "Test Site", ActionId);
		        await SiteRepository.SaveAsync(site, ActionId, CancellationToken.None);
	            
		        MockAllAdaptersSearchPageResponse = GetHtmlDocument("Vertical/TestFiles/idealista-searchpage1.html");

		        // Act
		        var response = 
			        await PostAsync("/v1/crawling/crawl-search-page", new Request
			        {
				        SiteId = siteId.ToString()
			        });

		        // Assert
		        AssertSuccessResponse(response);
	        }

		    [Fact]
	        public async Task TestSuccess_ThailandProperty_Result_And_Events()
	        {
	            // Arrange
	            var siteId = SiteId.Create(EnumExtensions.GetDescriptionFromEnumValue(SiteId.Strings.ThailandProperty));
	            var site = Site.Create(siteId, "Test Site", ActionId);
	            await SiteRepository.SaveAsync(site, ActionId, CancellationToken.None);
	            
	            MockAllAdaptersSearchPageResponse = GetHtmlDocument("Vertical/TestFiles/thailandproperty-searchpage1.html");

	            var expected = SearchResults.Create(DateTimeProvider.Now, new List<SearchPage>
	            {
		            SearchPage.Create(new List<Property>()
		            {
			            Property.Create(
				            PropertyId.Create("prop-1"), 
				            "1 Bedroom Condo for rent at Life One Wireless",
				            "This property is a 35 SqM condo with 1 bedroom and 1 bathroom that is available for sale. It is part of the Life One Wireless project in Lumphini, Bangkok. You can rent this condo long term for ฿28,000 per month. and was completed in Apr 2020",
				            Price.Create(28000, Currency.Baht),
				            Location.Create("Pathum Wan, Bangkok", 0.0, 0.0),
				            PropertyType.Condo,
				            ActionId),
			            Property.Create(
				            PropertyId.Create("prop-2"), 
				            "1 Bedroom Condo for rent at Via 49",
				            "space 46 sqm - 3rd floor - 1 Bedroom 1 Bathroom - Facility fee included - Contract 1 year minimum Location is next to Fuji Supermarket, Starbucks, Villa market, Samitivej Hospital",
				            Price.Create(28000, Currency.Baht),
				            Location.Create("Watthana, Bangkok", 0.0, 0.0),
				            PropertyType.Condo,
				            ActionId),
			            Property.Create(
				            PropertyId.Create("prop-3"), 
				            "2 Bedroom Condo for rent at Regent Royal Place 1",
				            "This property is a 84 SqM condo with 2 bedrooms and 1 bathroom that is available for sale. It is part of the Regent Royal Place 1 project in Lumphini, Bangkok. You can rent this condo long term for ฿25,000 per month. and was completed in Dec 1997",
				            Price.Create(25000, Currency.Baht),
				            Location.Create("Pathum Wan, Bangkok", 0.0, 0.0),
				            PropertyType.Condo,
				            ActionId)
		            })
	            });

	            // Act
	            var command = new Command
	            {
		            SiteId = siteId
	            };

	            var actual = await CrawlSearchPageAction.ExecuteAsync(command, ActionId, CancellationToken.None);

	            // Assert
	            AssertEntity(expected, actual);
	            AssertDomainEventPublished(new SearchPageCrawled(siteId, actual, DateTimeProvider, ActionId));
	        }
	        
	        [Fact]
	        public async Task TestSuccess_ThailandProperty_Http_Response_Ok()
	        {
		        // Arrange
		        var siteId = SiteId.Create(EnumExtensions.GetDescriptionFromEnumValue(SiteId.Strings.ThailandProperty));
		        var site = Site.Create(siteId, "Test Site", ActionId);
		        await SiteRepository.SaveAsync(site, ActionId, CancellationToken.None);
	            
		        MockAllAdaptersSearchPageResponse = GetHtmlDocument("Vertical/TestFiles/thailandproperty-searchpage1.html");

		        // Act
		        var response = 
			        await PostAsync("/v1/crawling/crawl-search-page", new Request
			        {
				        SiteId = siteId.ToString()
			        });

		        // Assert
		        AssertSuccessResponse(response);
	        }
	    }
	}
}
