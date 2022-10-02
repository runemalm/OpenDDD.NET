namespace Infrastructure.Ports.Adapters.Http.v1_0_0.Model
{
	public class Forecast_v1_0_0
	{
		public string ForecastId { get; set; }
		public DateTime Date { get; set; }
		public int TemperatureC { get; set; }
		public int TemperatureF { get; set; }
		public string Summary { get; set; }
	}
}
