using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Tests.Domain.Model;

namespace OpenDDD.Tests.Integration.Infrastructure.Persistence.EfCore.Configurations
{
    public class TestEntityConfiguration : EfEntityConfigurationBase<TestEntity, Guid>
    {
        public override void Configure(EntityTypeBuilder<TestEntity> builder)
        {
            base.Configure(builder);

            builder.Property<Guid>("TestAggregateRootId").IsRequired();
        }
    }
}
