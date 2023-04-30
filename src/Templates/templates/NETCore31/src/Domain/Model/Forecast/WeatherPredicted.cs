using System;
using OpenDDD.Application;
using OpenDDD.Domain.Model.BuildingBlocks.Event;
using Domain.Model.Summary;

namespace Domain.Model.Forecast
{
	public class WeatherPredicted : DomainEvent, IEquatable<WeatherPredicted>
	{
		public ForecastId ForecastId { get; set; }
		public DateTime Date { get; set; }
		public int TemperatureC { get; set; }
		public SummaryId SummaryId { get; set; }

		public WeatherPredicted() : base("WeatherPredicted", DomainModelVersion.Latest(), "Weather", ActionId.Create()) { }

		public WeatherPredicted(Forecast forecast, ActionId actionId) 
			: base("WeatherPredicted", DomainModelVersion.Latest(), "Weather", actionId)
		{
			ForecastId = forecast.ForecastId;
			Date = forecast.Date;
			TemperatureC = forecast.TemperatureC;
			SummaryId = forecast.SummaryId;
		}

		// Equality
		
		public bool Equals(WeatherPredicted? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return base.Equals(other) && ForecastId.Equals(other.ForecastId) && Date.Equals(other.Date) && TemperatureC == other.TemperatureC && SummaryId.Equals(other.SummaryId);
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((WeatherPredicted)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), ForecastId, Date, TemperatureC, SummaryId);
		}
	}
}
