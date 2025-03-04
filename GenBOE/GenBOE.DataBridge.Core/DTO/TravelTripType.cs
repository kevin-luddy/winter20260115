using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using GenBOE.DataBridge.Core.DTO.Common;
using IES.Common;
using IES.Common.Core.Enums;
using IES.Common.Core.Models;

namespace GenBOE.DataBridge.Core.DTO
{
	[Serializable()]
	[ExcludeFromCodeCoverage]
	public class TravelTripType : UpdateableDTO, IBOEMembership
	{
		public TravelTripType()
		{
			TravelTripID = -1;
			NumOfOccurences = 0;
			NumOfIntervals = 0;
			GroupID = null;

			CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();
		}

		/// <summary>
		/// If this is a locked trip, this ID refers to the original trip it came from
		/// </summary>
		public int? OriginatingTripID { get; set; }

		/// <summary>
		/// The date/time the trip was locked
		/// </summary>
		public DateTime? LockedDate { get; set; }

		// this is the PK ID
		public int TravelTripID { get; set; }

		// this is the user suppplied Trip ID , grouping related trips and sorting
		public int? GroupID { get; set; }

		// this is the system Trip ID that this travel trip is tied to
		public int SystemTripID { get; set; }

		public SegmentType Segment { get; set; }
		public int PerfOrgID { get; set; }
		public int BoeID { get; set; }
		public DateTime TripDate { get; set; }
		public string Purpose { get; set; }
		public int NumOfTrips { get; set; }
		public int NumOfPeople { get; set; }
		public int NumOfDays { get; set; }
		public int NumOfOccurences { get; set; }
		public int NumOfIntervals { get; set; }

		/// <summary>
		/// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
		/// </summary>
		internal IEnumerable<CustomFieldValueContainer> CustomFieldValueContainersIEnum { get; set; }

		public Collection<CustomFieldValueContainer> CustomFieldValueContainers { get; set; }
	}
}
