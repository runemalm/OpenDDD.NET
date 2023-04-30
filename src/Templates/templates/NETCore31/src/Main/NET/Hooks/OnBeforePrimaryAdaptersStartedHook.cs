using System.Threading.Tasks;
using OpenDDD.NET.Hooks;
using Domain.Model.Summary;
using Microsoft.AspNetCore.Builder;

namespace Main.NET.Hooks
{
	public class OnBeforePrimaryAdaptersStartedHook : IOnBeforePrimaryAdaptersStartedHook
	{
		private readonly ISummaryRepository _summaryRepository;

		public OnBeforePrimaryAdaptersStartedHook(
			ISummaryRepository summaryRepository)
		{
			_summaryRepository = summaryRepository;
		}
		
		public void Execute(IApplicationBuilder app)
		{
			BuildEnsureSummaries(app).Execute();
		}

		public async Task ExecuteAsync(IApplicationBuilder app)
		{
			await BuildEnsureSummaries(app).ExecuteAsync();
		}
		
		private EnsureSummaries BuildEnsureSummaries(IApplicationBuilder app)
		{
			var ensureSummaries = new EnsureSummaries(_summaryRepository);
			return ensureSummaries;
		}
	}
}
