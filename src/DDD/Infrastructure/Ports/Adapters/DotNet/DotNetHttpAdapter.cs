using Microsoft.AspNetCore.Mvc;

namespace DDD.Infrastructure.Ports.Adapters.DotNet
{
	[ApiController]
	public class DotNetHttpAdapter : ControllerBase, IHttpPort
	{
		public DotNetHttpAdapter() : base()
		{
			
		}
	}
}
