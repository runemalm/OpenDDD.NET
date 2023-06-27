using System.Collections.Generic;
using OpenDDD.Infrastructure.Ports.Adapters.Repository;
using Domain.Model.{{ aggregate_name }};
using ContextDomainModelVersion = Domain.Model.DomainModelVersion;

namespace Infrastructure.Ports.Adapters.Repository.Migration
{
    public class {{ aggregate_name }}Migrator : Migrator<{{ aggregate_name }}>
    {
        public {{ aggregate_name }}Migrator() : base(IamDomainModelVersion.Latest())
        {
            
        }

        public {{ aggregate_name }} FromV1_0_X({{ aggregate_name }} {{ obj_var_name }}V1_0_X)
        {
            var nextVersion = userV1_0_Y;
            
            nextVersion.DomainModelVersion = new ContextDomainModelVersion("1.0.Y");

            nextVersion.SomeNewProperty = null;
            
            return nextVersion;
        }
    }
}
