using DDD.Application;
using DDD.Domain.Model.BuildingBlocks.Aggregate;
using DDD.Domain.Model.BuildingBlocks.Entity;
using DDD.Domain.Model.Error;
using DDD.Domain.Model.Validation;
using XxxDomainModelVersion = Domain.Model.DomainModelVersion;

namespace Domain.Model.{{ class_name }}
{
    public class {{ class_name }} : Aggregate, IAggregate, IEquatable<{{ class_name }}>
    {
        public {{ class_name }}Id {{ class_name }}Id { get; set; }
        EntityId IAggregate.Id => {{ class_name }}Id;
        
        {{ property_expressions }}

        // Public

        public static async Task<{{ class_name }}> CreateAsync(
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
                throw DomainException.InvariantViolation(
                    $"{{ class_name }} is invalid with errors: " +
                    $"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
            }
        }

        // Equality
        
    }
}
