namespace Infrastructure.Ports.Adapters.Http.v1.Model
{
	public class ForecastV1
	{
		public string ForecastId { get; set; }
		public DateTime Date { get; set; }
		public int TemperatureC { get; set; }
		public int TemperatureF { get; set; }
		public string SummaryId { get; set; }
	}
}
