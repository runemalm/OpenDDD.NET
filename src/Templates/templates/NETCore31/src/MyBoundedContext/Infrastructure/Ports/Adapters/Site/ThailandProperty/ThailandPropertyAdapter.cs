using System;
using System.Net.Http;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenDDD.NET;
using MyBoundedContext.Domain.Model;
using MyBoundedContext.Domain.Model.Property;
using MyBoundedContext.Domain.Model.Site;
using OpenDDD.NET.Extensions;

namespace MyBoundedContext.Infrastructure.Ports.Adapters.Site.ThailandProperty
{
    public class ThailandPropertyAdapter : BaseSiteAdapter, IThailandPropertyPort
    {
        public ThailandPropertyAdapter(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IDateTimeProvider dateTimeProvider,
            ILogger<ThailandPropertyAdapter> logger,
            IMockedSearchPage mockedSearchPage = null) : base(
                httpClientFactory,
                dateTimeProvider,
                logger,
                SiteId.Create(EnumExtensions.GetDescriptionFromEnumValue(SiteId.Strings.ThailandProperty)),
                new Uri(configuration.GetSection("Sites:ThailandProperty")["SearchPageUri"]),
                mockedSearchPage
            )
        {
            
        }
        
        // ISitePort

        public override string PropertyXpath { get; set; } = "//div[@class='search-list featured']";

        public override Property ParseProperty(HtmlNode propertyNode)
        {
            var property = new Property
            {
                DomainModelVersion = DomainModelVersion.Latest(),
                Title = ParseTitle(propertyNode),
                Description = ParseDescription(propertyNode),
                Price = ParsePrice(propertyNode),
                Location = ParseLocation(propertyNode),
                PropertyType = PropertyType.Condo
            };

            return property;
        }
        
        private string ParseTitle(HtmlNode propertyNode)
        {
            var value = propertyNode.SelectSingleNode(".//h3[@class='name']")?.InnerText.Trim();

            return value;
        }
        
        private string ParseDescription(HtmlNode propertyNode)
        {
            var value = propertyNode.SelectSingleNode(".//div[@class='description-text']")?.InnerText.Trim();

            return value;
        }
        
        private Price ParsePrice(HtmlNode propertyNode)
        {
            var value = propertyNode.SelectSingleNode(".//div[@class='price']")?.InnerText.Trim();

            double amount = ParseNumberFromCurrencyString(value);
            
            var price = Price.Create(amount, Currency.Baht);

            return price;
        }
        
        private Location ParseLocation(HtmlNode propertyNode)
        {
            var value = propertyNode.SelectSingleNode(".//div[@class='location']/small")?.InnerText.Trim();

            value = RemoveMultipleSpacesAndLineBreaksRegex(value);
            value = RemoveSpacesBeforeCommasRegex(value);

            var lat = 0.0;
            var lng = 0.0;
            
            var location = Location.Create(value, lat, lng);

            return location;
        }
    }
}
