using Microsoft.Extensions.Options;

namespace DDD.Application.Settings.PowerIam
{
	public class PowerIamSettings : IPowerIamSettings
	{
		public string Url { get; }
		
		public PowerIamSettings() { }

		public PowerIamSettings(IOptions<Options> options)
		{
			var url = options.Value.POWERIAM_URL;
			
			Url = url;
		}
	}
}
