using System;

namespace Interchange.Domain.Model.Forecast
{
	public class IcForecast : IEquatable<IcForecast>
	{
		public string ForecastId { get; set; }
		public DateTime Date { get; set; }
		public int TemperatureC { get; set; }
		public string SummaryId { get; set; }

		public IcForecast()
		{
			
		}

		public IcForecast(string forecastId, DateTime date, int temperatureC, string summaryId)
		{
			ForecastId = forecastId;
			Date = date;
			TemperatureC = temperatureC;
			SummaryId = summaryId;
		}
		
		// Equality

		public bool Equals(IcForecast? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return ForecastId == other.ForecastId && Date.Equals(other.Date) && TemperatureC == other.TemperatureC && SummaryId == other.SummaryId;
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((IcForecast)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ForecastId, Date, TemperatureC, SummaryId);
		}
	}
}
