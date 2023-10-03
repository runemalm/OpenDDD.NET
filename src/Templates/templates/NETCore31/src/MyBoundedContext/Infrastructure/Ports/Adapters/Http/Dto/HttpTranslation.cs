using System;
using System.Collections.Generic;
using System.Linq;
using OpenDDD.Infrastructure.Ports.Adapters.Http.Dto;
using MyBoundedContext.Domain.Model.Property;
using MyBoundedContext.Domain.Model.SearchResults;
using MyBoundedContext.Domain.Model.Site;
using MyBoundedContext.Vertical;

namespace MyBoundedContext.Infrastructure.Ports.Adapters.Http.Dto
{
	public class HttpTranslation
	{
		// Request to commands
		
		public CrawlSearchPage.Command FromV1(CrawlSearchPage.Request request)
		{
			return new CrawlSearchPage.Command
			{
				SiteId = SiteId.Create(request.SiteId)
			};
		}
		
		public GetProperties.Command FromV1(GetProperties.Request request)
		{
			return new GetProperties.Command
			{
				
			};
		}
		
		// Entities
		
		public class SearchResultsV1 : BaseDto
		{
			public DateTime SearchedAt { get; set; }
			public IEnumerable<SearchPageV1> Pages { get; set; }
		}

		public SearchResultsV1 ToV1(SearchResults obj)
		{
			if (obj == null) return null;
			var dto = new SearchResultsV1
			{
				SearchedAt = obj.SearchedAt,
				Pages = ToV1(obj.Pages)
			};
			return dto;
		}
		
		public class SearchPageV1 : BaseDto
		{
			public IEnumerable<PropertyV1> Properties { get; set; }
		}
		
		public SearchPageV1 ToV1(SearchPage obj)
		{
			if (obj == null) return null;
			var dto = new SearchPageV1
			{
				Properties = ToV1(obj.Properties)
			};
			return dto;
		}
		
		public IEnumerable<SearchPageV1> ToV1(IEnumerable<SearchPage> objs)
			=> objs?.Select(ToV1);
		
		public class PropertyV1 : BaseDto
		{
			public string Description { get; set; }
		}
		
		public PropertyV1 ToV1(Property obj)
		{
			if (obj == null) return null;
			var dto = new PropertyV1
			{
				Description = obj.Description
			};
			return dto;
		}
		
		public IEnumerable<PropertyV1> ToV1(IEnumerable<Property> objs)
			=> objs?.Select(ToV1);
	}
}
