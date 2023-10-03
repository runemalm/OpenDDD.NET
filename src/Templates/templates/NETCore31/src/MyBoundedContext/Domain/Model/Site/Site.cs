using System;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Domain.Model.AggregateRoot;
using OpenDDD.Domain.Model.Entity;
using OpenDDD.Domain.Model.Event;
using OpenDDD.NET;
using MyBoundedContext.Vertical;
using AppDomainModelVersion = MyBoundedContext.Domain.Model.DomainModelVersion;

namespace MyBoundedContext.Domain.Model.Site
{
    public class Site : AggregateRoot, IAggregateRoot, IEquatable<Site>
    {
        public SiteId SiteId { get; set; }
        EntityId IAggregateRoot.Id => SiteId;
        
        public string Name { get; set; }

        // Public

        public static Site Create(
            SiteId siteId,
            string name,
            ActionId actionId)
        {
            var site =
                new Site
                {
                    DomainModelVersion = AppDomainModelVersion.Latest(),
                    SiteId = siteId,
                    Name = name
                };

            return site;
        }

        public async Task<SearchResults.SearchResults> CrawlSearchPageAsync(ISitePort siteAdapter, IDateTimeProvider dateTimeProvider, IDomainPublisher domainPublisher, ActionId actionId, CancellationToken ct)
        {
            var searchResults = await siteAdapter.CrawlSearchPageAsync(ct);
            
            await domainPublisher.PublishAsync(new CrawlSearchPage.SearchPageCrawled(SiteId, searchResults, dateTimeProvider, actionId));

            return searchResults;
        }

        // Private

        

        // Equality
        
        public bool Equals(Site other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(SiteId, other.SiteId) && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Site)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), SiteId, Name);
        }

        public static bool operator ==(Site left, Site right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Site left, Site right)
        {
            return !Equals(left, right);
        }
    }
}
