// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO.Travel
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using GenBOE.DataBridge.Core.DTO.Common;
	using IES.Common;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;

	[Serializable()]
	public class MSTTravelTripType : UpdateableDTO, IBOEMembership
	{
		public MSTTravelTripType()
		{
			Id = -1;
			GroupID = null;
			CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();
		}

		/// <summary>
		/// travel mode from MSTTravelModeLU
		/// </summary>
		public MSTTravelMode ModeID { get; set; }

		/// <summary>
		/// this is the user suppplied Trip ID , identifying related trips
		/// </summary>
		public int? GroupID { get; set; }
		public SegmentType Segment { get; set; }
		public int PerfOrgID { get; set; }
		public int BoeID { get; set; }
		public DateTime TripDate { get; set; }
		public DateTime EstimateDate { get; set; }
		public string Purpose { get; set; }
		public decimal? NumOfPeople { get; set; }
		public decimal? NumOfDays { get; set; }
		public int? ZoneOriginID { get; set; }
		public string ZoneOriginName { get; set; }
		public int? ZoneDestinationID { get; set; }
		public string ZoneDestinationName { get; set; }
		public string ZoneDestCity { get; set; }
		public int? ZoneResourceID { get; set; }
		public int? ZoneDestinationZone { get; set; }
		public string NonZoneFrom { get; set; }
		public string NonZoneTo { get; set; }
		public decimal? NonZoneAirfareEstimate { get; set; }
		public decimal? NonZonePerDiemDaily { get; set; }
		public decimal? NonZoneCarRentalTrans { get; set; }
		public decimal? NonZoneTravelAgencyFee { get; set; }
		public decimal? NonZoneMiscOtherCosts { get; set; }
		public decimal? NonZoneNumCars { get; set; }
		public int? NonZoneResourceID { get; set; }

		/// <summary>
		/// Clin Id (only used if Multi-Clin/Wbs is selected for the BOE
		/// </summary>
		public int? ClinId { get; set; }

		/// <summary>
		/// Wbs Id (only used if Multi-Clin/Wbs is selected for the BOE
		/// </summary>
		public int? WbsId { get; set; }

		/// <summary>
		/// Clin Title (only used if Multi-Clin/Wbs is selected for the BOE)
		/// </summary>
		public string ClinTitle { get; set; }

		/// <summary>
		/// Clin Number (only used if Multi-Clin/Wbs is selected for the BOE)
		/// </summary>
		public string ClinNumber { get; set; }

		/// <summary>
		/// Wbs Title (only used if Multi-Clin/Wbs is selected for the BOE)
		/// </summary>
		public string WbsTitle { get; set; }

		/// <summary>
		/// Wbs Number (only used if Multi-Clin/Wbs is selected for the BOE)
		/// </summary>
		public string WbsNumber { get; set; }

		/// <summary>
		/// Resource id (string) that should be only used by ProPricer when exporting the travel. 
		/// 
		/// Values:
		///    NonZone - this contains the selection made by the user
		///    Zone - this will always contain Per Diem resource code
		/// </summary>
		public string ResourceIdForExport { get; set; }

		/// <summary>
		/// Secondary Resource id (string) that should be only used by ProPricer when exporting the travel. 
		/// 
		/// Values:
		///    NonZone - NULL
		///    Zone - this will either be null if No-Airfare was selected, or it will contain the Airfare resource code if one was selected
		/// </summary>
		public string SecondaryResourceIdForExport { get; set; }

		/// <summary>
		/// Trip cost
		/// </summary>
		public decimal Cost { get; set; }

		public Collection<CustomFieldValueContainer> CustomFieldValueContainers { get; set; }

		/// <summary>
		/// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
		/// </summary>
		internal IEnumerable<CustomFieldValueContainer> CustomFieldValueContainersIEnum { get; set; }

		/// <summary>
		/// Resource Zone Id
		/// </summary>
		internal int? ResourceZoneId { get; set; }

		/// <summary>
		/// Resource Origin Id
		/// </summary>
		internal int? ResourceOriginId { get; set; }
	}
}
