using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Tests.Base.Domain.Model;

namespace OpenDDD.Tests.Integration.Infrastructure.Persistence.EfCore.Configurations
{
    public class TestAggregateRootConfiguration : EfAggregateRootConfigurationBase<TestAggregateRoot, Guid>
    {
        public override void Configure(EntityTypeBuilder<TestAggregateRoot> builder)
        {
            base.Configure(builder);

            builder.OwnsOne(a => a.Value);

            builder.HasMany(a => a.Entities)
                .WithOne()
                .HasForeignKey("TestAggregateRootId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(a => a.Entities).AutoInclude();
        }
    }
}
