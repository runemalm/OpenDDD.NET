using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation;
using Domain.Model.{{ bb_name }};
using Infrastructure.Ports.Adapters.Http.vX.Model;

namespace Infrastructure.Ports.Adapters.Http.Common.Translation
{
    public class {{ bb_name }}Translator : Translator
    {
        public {{ bb_name }}Translator()
        {
            
        }

        public {{ bb_name }} FromVX({{ bb_name }}VX {{ obj_var_name }}VX)
        {
            throw new NotImplementedException("Generated code not implemented.");

            // return new {{ bb_name }}
            // {
            //     {{ bb_name }}Id = {{ bb_name }}Id.Create({{ obj_var_name }}VX.{{ bb_name }}Id),
            //     Name = {{ obj_var_name }}VX.Name,
            //     Description = {{ obj_var_name }}VX.Description
            // };
        }

        public {{ bb_name }}VX ToVX({{ bb_name }} {{ obj_var_name }})
        {
            throw new NotImplementedException("Generated code not implemented.");

            // return new {{ bb_name }}VX
            // {
            //     {{ bb_name }}Id = {{ obj_var_name }}.{{ bb_name }}Id.Value,
            //     Name = {{ obj_var_name }}.Name,
            //     Description = {{ obj_var_name }}.Description
            // };
        }

        public IEnumerable<{{ bb_name }}VX> ToVX(IEnumerable<{{ bb_name }}> {{ obj_var_name }}s)
            => {{ obj_var_name }}s.Select(ToVX);
    }
}
