using System.ComponentModel;
using OpenDDD.Domain.Model.Entity;

namespace MyBoundedContext.Domain.Model.Site
{
    public class SiteId : EntityId
    {
        public enum Strings
        {
            [Description("Idealista")]
            Idealista,
            [Description("ThailandProperty")]
            ThailandProperty
        }

        public SiteId(string value) : base(value) { }

        public static SiteId Create(string value)
        {
            var siteId = new SiteId(value);
            siteId.Validate(nameof(siteId));
            return siteId;
        }
    }
}
