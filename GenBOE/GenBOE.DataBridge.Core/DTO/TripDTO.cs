// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common;
	using IES.Common.Core.Models;

	[Serializable()]
	[ExcludeFromCodeCoverage]
	public class TripDTO : UpdateableDTO
	{
		public TripDTO()
		{
			TripID = -1;
			MiscTravelRateID = -1;
			DepartureLocationID = -1;
			DestinationLocationID = -1;
			PerDiemID = -1;
			TripCount = 0;
			InUse = false;
			LockedRate = false;
			DepartureLocationCode = string.Empty;
			DestinationLocationCode = string.Empty;
		}

		public int TripID { get; set; }

		// Note: This is the ModeID
		public int MiscTravelRateID { get; set; }

		// Departure location can either be an exisiting departure or a brand new one if the user starts to type
		public int DepartureLocationID { get; set; }


		// Desintation location can either be an exisiting departure or a brand new one if the user starts to type
		public int DestinationLocationID { get; set; }

		public int PerDiemID { get; set; }
		public decimal Fare { get; set; }
		public int RTMiles { get; set; }

		public int FareUpdatedByUserID { get; set; }
		public DateTime? FareLastUpdatedDate { get; set; }
		public DateTime? LastUsedDate { get; set; }
		public int TripCount { get; set; }
		public bool InUse { get; set; }
		public bool LockedRate { get; set; }
		public decimal RentalCarRate { get; set; }
		public string DepartureLocationCode { get; set; }
		public string DestinationLocationCode { get; set; }


	}
}
