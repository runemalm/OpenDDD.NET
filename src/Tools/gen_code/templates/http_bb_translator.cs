using System.Collections.Generic;
using System.Linq;
using DDD.Infrastructure.Ports.Adapters.Common.Translation;
using Domain.Model.{{ bb_name }};
using Infrastructure.Ports.Adapters.Http.vX_X_X.Model;

namespace Infrastructure.Ports.Adapters.Http.Common.Translation
{
    public class {{ bb_name }}Translator : Translator
    {
        public {{ bb_name }}Translator()
        {
            
        }

        public {{ bb_name }} From_vX_X_X({{ bb_name }}_vX_X_X {{ obj_var_name }}_vX_X_X)
        {
            throw new NotImplementedException("Generated code not implemented.");

            // return new {{ bb_name }}
            // {
            //     {{ bb_name }}Id = {{ bb_name }}Id.Create({{ obj_var_name }}_vX_X_X.{{ bb_name }}Id),
            //     Name = {{ obj_var_name }}_vX_X_X.Name,
            //     Description = {{ obj_var_name }}_vX_X_X.Description
            // };
        }

        public {{ bb_name }}_vX_X_X To_vX_X_X({{ bb_name }} {{ obj_var_name }})
        {
            throw new NotImplementedException("Generated code not implemented.");

            // return new {{ bb_name }}_vX_X_X
            // {
            //     {{ bb_name }}Id = {{ obj_var_name }}.{{ bb_name }}Id.Value,
            //     Name = {{ obj_var_name }}.Name,
            //     Description = {{ obj_var_name }}.Description
            // };
        }

        public IEnumerable<{{ bb_name }}_vX_X_X> To_vX_X_X(IEnumerable<{{ bb_name }}> {{ obj_var_name }}s)
            => {{ obj_var_name }}s.Select(To_vX_X_X);
    }
}
