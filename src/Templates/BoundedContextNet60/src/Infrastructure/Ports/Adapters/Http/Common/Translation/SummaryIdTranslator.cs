using DDD.Infrastructure.Ports.Adapters.Common.Translation;
using Domain.Model.Summary;

namespace Infrastructure.Ports.Adapters.Http.Common.Translation;

public class SummaryIdTranslator : Translator
{
	public SummaryIdTranslator()
	{
		            
	}

	public SummaryId FromV1(string summaryIdV1)
	{
		return SummaryId.Create(summaryIdV1);
    }

	public string ToV1(SummaryId summaryId)
	{
		return summaryId.Value;
	}
}
