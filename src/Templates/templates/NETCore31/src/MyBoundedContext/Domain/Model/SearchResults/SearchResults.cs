using System;
using System.Collections.Generic;
using System.Linq;
using OpenDDD.Domain.Model.ValueObject;
using AppDomainModelVersion = MyBoundedContext.Domain.Model.DomainModelVersion;

namespace MyBoundedContext.Domain.Model.SearchResults
{
    public class SearchResults : ValueObject
    {
        public DateTime SearchedAt { get; set; }
        public IEnumerable<SearchPage> Pages { get; set; }

        // Public

        public static SearchResults Create(DateTime searchedAt, IEnumerable<SearchPage> pages)
        {
            var searchResults =
                new SearchResults
                {
                    SearchedAt = searchedAt,
                    Pages = pages.ToList()
                };

            return searchResults;
        }
    }
}
