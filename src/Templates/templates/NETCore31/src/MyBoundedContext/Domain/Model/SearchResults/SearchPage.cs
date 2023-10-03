using System.Collections.Generic;
using OpenDDD.Domain.Model.ValueObject;
using AppDomainModelVersion = MyBoundedContext.Domain.Model.DomainModelVersion;

namespace MyBoundedContext.Domain.Model.SearchResults
{
    public class SearchPage : ValueObject
    {
        public IEnumerable<Property.Property> Properties { get; set; }

        // Public

        public static SearchPage Create(IEnumerable<Property.Property> properties)
        {
            var searchPage =
                new SearchPage
                {
                    Properties = properties
                };

            return searchPage;
        }
    }
}