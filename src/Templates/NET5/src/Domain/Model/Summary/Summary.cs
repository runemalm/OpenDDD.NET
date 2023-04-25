using System;
using System.Linq;
using DDD.Application;
using DDD.Domain.Model.BuildingBlocks.Aggregate;
using DDD.Domain.Model.BuildingBlocks.Entity;
using DDD.Domain.Model.Error;
using DDD.Domain.Model.Validation;
using WeatherDomainModelVersion = Domain.Model.DomainModelVersion;

namespace Domain.Model.Summary
{
    public class Summary : Aggregate, IAggregate, IEquatable<Summary>
    {
        public SummaryId SummaryId { get; set; }
        EntityId IAggregate.Id => SummaryId;
        
        public string Value { get; set; }

        // Public

        public static Summary Create(
            SummaryId summaryId,
            string value,
            ActionId actionId)
        {
            var summary =
                new Summary()
                {
                    DomainModelVersion = WeatherDomainModelVersion.Latest(),
                    SummaryId = summaryId,
                    Value = value
                };

            summary.Validate();

            return summary;
        }

        // Private

        protected void Validate()
        {
            var validator = new Validator<Summary>(this);

            var errors = validator
                .NotNull(bb => bb.SummaryId.Value)
                .NotNullOrEmpty(bb => bb.Value)
                .Errors()
                .ToList();

            if (errors.Any())
            {
                throw DomainException.InvariantViolation(
                    $"Summary is invalid with errors: " +
                    $"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
            }
        }

        // Equality

        public bool Equals(Summary? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && SummaryId.Equals(other.SummaryId) && Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Summary)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), SummaryId, Value);
        }
    }
}
