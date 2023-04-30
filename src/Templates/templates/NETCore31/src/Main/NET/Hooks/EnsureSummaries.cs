using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using Domain.Model.Summary;

namespace Main.NET.Hooks
{
	public class EnsureSummaries
	{
		private readonly ISummaryRepository _summaryRepository;

		public EnsureSummaries(ISummaryRepository summaryRepository)
		{
			_summaryRepository = summaryRepository;
		}
		
		private IEnumerable<string> GetValues()
		{
			if (_summaryRepository == null)
				throw new Exception("The summary repository hasn't been registered.");
			
			// Required summaries
			var values = 
				new List<string>()
				{
					"Freezing",
					"Bracing",
					"Chilly",
					"Cool",
					"Mild",
					"Warm",
					"Balmy",
					"Hot",
					"Sweltering",
					"Scorching"
				};

			return values;
		}

		public void Execute()
		{
			var values = GetValues();

			foreach (var value in values)
			{
				var existing = Task.Run(() => 
					_summaryRepository.GetWithValueAsync(
						value,
						ActionId.BootId(),
						CancellationToken.None)).GetAwaiter().GetResult();

				if (existing == null)
				{
					var summary = 
						Summary.Create(
							SummaryId.Create(_summaryRepository.GetNextIdentity()), 
							value, 
							ActionId.BootId());
					
					_summaryRepository.Save(
						summary,
						ActionId.BootId(), 
						CancellationToken.None);
				}
			}
		}
		
		public async Task ExecuteAsync()
		{
			var values = GetValues();

			foreach (var value in values)
			{
				var existing = await 
					_summaryRepository.GetWithValueAsync(
						value,
						ActionId.BootId(),
						CancellationToken.None);

				if (existing == null)
				{
					var summary = 
						Summary.Create(
							SummaryId.Create(await _summaryRepository.GetNextIdentityAsync()), 
							value, 
							ActionId.BootId());
					
					await _summaryRepository.SaveAsync(
						summary,
						ActionId.BootId(), 
						CancellationToken.None);
				}
			}
		}
	}
}
