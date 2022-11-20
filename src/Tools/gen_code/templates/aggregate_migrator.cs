using System.Collections.Generic;
using System.Linq;
using DDD.Domain.Model;
using DDD.Infrastructure.Ports.Repository;
using Domain.Model.{{ aggregate_name }};
// using XxxDomainModelVersion = Domain.Model.DomainModelVersion;

namespace Infrastructure.Ports.Adapters.Migration
{
    public class {{ aggregate_name }}Migrator : IMigrator<{{ aggregate_name }}>
    {
        private readonly DomainModelVersion _latestVersion;
        
        public {{ aggregate_name }}Migrator()
        {
            // _latestVersion = XxxDomainModelVersion.Latest();
        }

        public {{ aggregate_name }} Migrate({{ aggregate_name }} {{ obj_var_name }})
        {
            DomainModelVersion at = {{ obj_var_name }}.DomainModelVersion;

            while (at < _latestVersion)
            {
                var methodName = $"From_v{at.ToString().Replace('.', '_')}";
                var method = GetType().GetMethod(methodName, new [] {typeof({{ aggregate_name }})});
                {{ obj_var_name }} = ({{ aggregate_name }})method.Invoke(this, new object[]{% raw %}{{% endraw %}{{ obj_var_name }}{% raw %}}{% endraw %});
                at = {{ obj_var_name }}.DomainModelVersion;
            }

            return {{ obj_var_name }};
        }
        
        public IEnumerable<{{ aggregate_name }}> Migrate(IEnumerable<{{ aggregate_name }}> {{ obj_var_name }}s)
            => {{ obj_var_name }}s.Select(Migrate);
    }
}
