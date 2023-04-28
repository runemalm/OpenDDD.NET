using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace OpenDDD.NET.Hooks
{
	public interface IOnBeforePrimaryAdaptersStartedHook
	{
		void Execute(IApplicationBuilder app);
		Task ExecuteAsync(IApplicationBuilder app);
	}
}
