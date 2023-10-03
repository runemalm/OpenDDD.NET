using System;
using System.Globalization;
using System.Net.Http;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenDDD.NET;
using MyBoundedContext.Domain.Model;
using MyBoundedContext.Domain.Model.Property;
using MyBoundedContext.Domain.Model.Site;
using OpenDDD.NET.Extensions;

namespace MyBoundedContext.Infrastructure.Ports.Adapters.Site.Idealista
{
    public class IdealistaAdapter : BaseSiteAdapter, IIdealistaPort
    {
        public IdealistaAdapter(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IDateTimeProvider dateTimeProvider,
            ILogger<IdealistaAdapter> logger,
            IMockedSearchPage mockedSearchPage = null) : base(
                httpClientFactory,
                dateTimeProvider,
                logger,
                SiteId.Create(EnumExtensions.GetDescriptionFromEnumValue(SiteId.Strings.Idealista)),
                new Uri(configuration.GetSection("Sites:Idealista")["SearchPageUri"]),
                mockedSearchPage
            )
        {
            
        }

        // ISitePort

        public override string PropertyXpath { get; set; } = "//article[@class='item extended-item item-multimedia-container']";

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
        
        // Helpers

        private string ParseTitle(HtmlNode propertyNode)
        {
            var value = propertyNode.SelectSingleNode(".//a[@role='heading']")?.InnerText.Trim();
            
            return value;
        }
        
        private string ParseDescription(HtmlNode propertyNode)
        {
            var value = propertyNode.SelectSingleNode(".//div[@class='item-description description']")?.InnerText.Trim();

            return value;
        }
        
        private Price ParsePrice(HtmlNode propertyNode)
        {
            var value = propertyNode.SelectSingleNode(".//span[@class='item-price h2-simulated']")?.InnerText.Trim();

            double amount = ParseNumberFromCurrencyString(value);
            
            var price = Price.Create(amount, Currency.Euro);

            return price;
        }
        
        private Location ParseLocation(HtmlNode propertyNode)
        {
            var lat = 0.0;
            var lng = 0.0;
            
            var location = Location.Create("Costa del Sol", lat, lng);

            return location;
        }
    }
}
