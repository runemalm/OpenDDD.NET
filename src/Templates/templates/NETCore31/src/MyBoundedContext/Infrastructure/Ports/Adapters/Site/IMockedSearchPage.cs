using HtmlAgilityPack;

namespace MyBoundedContext.Infrastructure.Ports.Adapters.Site
{
    public interface IMockedSearchPage
    {
        HtmlDocument GetResponse();
    }
}
