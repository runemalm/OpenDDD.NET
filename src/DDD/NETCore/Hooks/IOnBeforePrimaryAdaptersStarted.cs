using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace DDD.NETCore.Hooks
{
	public interface IOnBeforePrimaryAdaptersStartedHook
	{
		void Execute(IApplicationBuilder app);
		Task ExecuteAsync(IApplicationBuilder app);
	}
}
