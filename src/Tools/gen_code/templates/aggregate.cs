using System;
using System.Linq;
using DDD.Domain;
using DDD.Domain.Exceptions;
using DDD.Domain.Validation;

namespace Domain.Model.{{ class_name }}
{
    public class {{ class_name }} : IAggregate, IEquatable<{{ class_name }}>
    {
        public {{ class_name }}Id {{ class_name }}Id { get; set; }
        EntityId IAggregate.Id => {{ class_name }}Id;
        
        {{ property_expressions }}

        // Public

        public static {{ class_name }} Create(
            {{ property_method_args }})
        {
            throw new NotImplementedException();

            var {{ obj_var_name }} =
                new {{ class_name }}()
                {
                    {{ property_init_args }}
                };

            {{ obj_var_name }}.Validate();

            return {{ obj_var_name }};
        }

        // Private

        protected void Validate()
        {
            throw new NotImplementedException();

            var validator = new Validator<{{ class_name }}>(this);

            var errors = validator
                .NotNull(bb => bb.{{ class_name }}Id.Value)
                // .NotNullOrEmpty(bb => bb.Name)
                // .NotNullOrEmpty(bb => bb.Location)
                .Errors()
                .ToList();

            if (errors.Any())
            {
                throw new InvariantException(
                    $"{{ class_name }} is invalid with errors: " +
                    $"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
            }
        }

        // Equality
        
    }
}
