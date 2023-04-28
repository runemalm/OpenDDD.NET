using System.Collections.Generic;
using System.Linq;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation;
using Domain.Model.Summary;
using Infrastructure.Ports.Adapters.Http.v1.Model;

namespace Infrastructure.Ports.Adapters.Http.Common.Translation
{
    public class SummaryTranslator : Translator
    {
        public SummaryTranslator()
        {
            
        }

        public Summary FromV1(SummaryV1 summaryV1)
        {
            return new Summary
            {
                SummaryId = SummaryId.Create(summaryV1.SummaryId),
                Value = summaryV1.Value
            };
        }

        public SummaryV1 ToV1(Summary summary)
        {
            return new SummaryV1
            {
                SummaryId = summary.SummaryId.Value,
                Value = summary.Value,
            };
        }

        public IEnumerable<SummaryV1> ToV1(IEnumerable<Summary> summaries)
            => summaries.Select(ToV1);
    }
}
