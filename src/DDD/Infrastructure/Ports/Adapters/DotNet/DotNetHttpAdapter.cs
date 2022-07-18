using Microsoft.AspNetCore.Mvc;

namespace DDD.Infrastructure.Ports.Adapters.DotNet
{
	public class DotNetHttpAdapter : ControllerBase, IHttpPort
	{
		public DotNetHttpAdapter() : base()
		{
			
		}
	}
}
