using OpenDDD.Domain.Model.BuildingBlocks.Entity;

namespace Domain.Model.Summary
{
    public class SummaryId : EntityId
    {
        public SummaryId(string value) : base(value) { }

        public static SummaryId Create(string value)
        {
            var summaryId = new SummaryId(value);
            summaryId.Validate(nameof(summaryId));
            return summaryId;
        }
    }
}
