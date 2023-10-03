using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using OpenDDD.Application.Error;
using OpenDDD.NET;
using MyBoundedContext.Domain.Model.SearchResults;
using MyBoundedContext.Domain.Model.Site;
using MyBoundedContext.Infrastructure.Ports.Adapters.Site;
using Property = MyBoundedContext.Domain.Model.Property.Property;

namespace MyBoundedContext.Infrastructure.Ports.Adapters
{
	public abstract class BaseSiteAdapter : ISitePort
	{
		private readonly SiteId _siteId;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly ILogger _logger;
		private readonly Uri _searchPageUri;
		private readonly IMockedSearchPage _mockedSearchPage;
  
		public BaseSiteAdapter(
			IHttpClientFactory httpClientFactory,
			IDateTimeProvider dateTimeProvider,
			ILogger logger,
			SiteId siteId,
			Uri searchPageUri,
			IMockedSearchPage mockedSearchPage)
		{
			_siteId = siteId;
			_httpClientFactory = httpClientFactory;
			_dateTimeProvider = dateTimeProvider;
			_logger = logger;
			_searchPageUri = searchPageUri;
			_mockedSearchPage = mockedSearchPage;
		}

		// ISitePort
		
		public abstract string PropertyXpath { get; set; }

		public async Task<SearchResults> CrawlSearchPageAsync(CancellationToken ct)
		{
			var searchedAt = _dateTimeProvider.Now;

			HtmlDocument document;

			if (_mockedSearchPage != null)
			{
				document = _mockedSearchPage.GetResponse();
				
				if (document == null)
					throw new DddException("Failed to get mocked response. Are you sure you have set the mocked response in your test setup phase?");
			}
			else
			{
				document = await GetSearchPageAsync(ct);
			}

			var properties = new List<Property>();

			var nodes = document.DocumentNode.SelectNodes(PropertyXpath);
			if (nodes == null || nodes.Count == 0)
			{
				_logger.LogError(
					"No properties returned from the search results page. " +
					"That's very unexpected, you should look into the parsing and make sure the DOM layour " +
					"hasn't changed or something.");
			}
			else
			{
				foreach (HtmlNode propertyNode in nodes)
				{
					var property = ParseProperty(propertyNode);
					properties.Add(property);
				}
			}

			var pages = new List<SearchPage>
			{
				SearchPage.Create(properties)
			};

			var searchResults = SearchResults.Create(searchedAt, pages);

			return searchResults;
		}

		public async Task<HtmlDocument> GetSearchPageAsync(CancellationToken ct)
		{
			HtmlWeb web = new HtmlWeb();
			HtmlDocument document = await web.LoadFromWebAsync(_searchPageUri.ToString(), ct);
			return document;
		}
		
		public abstract Property ParseProperty(HtmlNode propertyNode);

		// Helpers
		
		protected double ParseNumberFromCurrencyString(string input)
		{
			// Define the regular expression pattern to match numeric values
			string pattern = @"[\d,]+(?:\.\d+)?";
        
			// Use regex to find the numeric value in the input string
			Match match = Regex.Match(input, pattern);
        
			if (match.Success)
			{
				// Parse the matched value to double
				string numericValue = match.Value.Replace(",", ""); // Remove commas
				if (double.TryParse(numericValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedValue))
				{
					return parsedValue;
				}
			}

			return 0.0;
		}
		
		protected string RemoveMultipleSpacesAndLineBreaksRegex(string input)
		{
			// Replace multiple spaces and line breaks with a single space
			string pattern = @"\s+";
			string replacement = " ";

			return Regex.Replace(input, pattern, replacement);
		}
        
		protected string RemoveSpacesBeforeCommasRegex(string input)
		{
			// Replace spaces before commas with no space
			string pattern = @"\s+,";
			string replacement = ",";

			return Regex.Replace(input, pattern, replacement);
		}
	}
}
