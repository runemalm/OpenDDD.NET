using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace DDD.NETCore.Hooks
{
	public interface IOnBeforePrimaryAdaptersStartedHook
	{
		Task ExecuteAsync(IApplicationBuilder app);
	}
}
