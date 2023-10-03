using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace MyBoundedContext.Domain.Model.Site
{
	public interface ISitePort
	{
		string PropertyXpath { get; set; }
		Task<SearchResults.SearchResults> CrawlSearchPageAsync(CancellationToken ct);
		Task<HtmlDocument> GetSearchPageAsync(CancellationToken ct);
		Property.Property ParseProperty(HtmlNode propertyNode);
	}
}
