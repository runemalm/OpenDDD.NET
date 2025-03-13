using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDDD.Tests.Base.Domain.Model;

namespace OpenDDD.Tests.Integration.Infrastructure.Persistence.EfCore.Configurations
{
    public class TestValueObjectConfiguration : IEntityTypeConfiguration<TestValueObject>
    {
        public void Configure(EntityTypeBuilder<TestValueObject> builder)
        {
            builder.HasNoKey();
        }
    }
}
