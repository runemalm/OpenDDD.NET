﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Persistence.EfCore.Configurations
{
    public class LineItemConfiguration : EfEntityConfigurationBase<LineItem, Guid>
    {
        public override void Configure(EntityTypeBuilder<LineItem> builder)
        {
            base.Configure(builder);
            
            // Define the OrderId foreign key (shadow property)
            builder.Property<Guid>("OrderId")
                .IsRequired();
        }
    }
}
