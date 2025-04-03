using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.DataBridge.Core.ModelView
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class PerDiemDataModelView
	{
		public PerDiemDataModelView()
		{
			HotelRate = 0.0m;
			MIERate = 0.0m;
			RentalCar = 0.0m;
			PerDiemNotes = string.Empty;
		}
		public decimal HotelRate { get; set; }
		public decimal MIERate { get; set; }
		public decimal RentalCar { get; set; }
		public string PerDiemNotes { get; set; }
	}

	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class FareDataModelView
	{
		public FareDataModelView()
		{
			DepartuareLocationID = 0;
			PerDiemDestination = string.Empty;
			PerDiemLocationID = 0;
			DestinationID = 0;
			Fare = 0.0m;
			RTMIles = 0;

		}
		public int DepartuareLocationID { get; set; }
		public string PerDiemDestination { get; set; }
		public int PerDiemLocationID { get; set; }
		public int DestinationID { get; set; }
		public decimal Fare { get; set; }
		public int RTMIles { get; set; }
	}
}
