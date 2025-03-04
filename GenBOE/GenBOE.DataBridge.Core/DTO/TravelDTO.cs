// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using GenBOE.DataBridge.Core.DTO.Common;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;

	[Serializable()]
	public class TravelDTO : UpdateableDTO, IBOEMembership, IStartEndDates, IDateShiftable
	{
		private string description;

		public TravelDTO()
		{
			Id = -1;
			TaskID = string.Empty;
			TaskTitle = string.Empty;
			TravelTrips = new Collection<TravelTripType>();
			MSTTravelTrips = new Collection<MSTTravelTripType>();
			StartDate = DateTime.MinValue;
			EndDate = DateTime.MinValue;

			CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();

			description = null;
			WasDescriptionSet = false;
			preventRteDbLoad = false;
		}

		/// <summary>
		/// The value of the order in which the task will appear in the boe listing
		/// </summary>
		public int BOETaskElementOrder { get; set; }

		public string TaskID { get; set; }

		public string TaskTitle { get; set; }

		#region RTE Fields

		/// <summary>
		/// Indicates whether the Description field was set (either from user, or via RTE load)
		/// </summary>
		public bool WasDescriptionSet { get; set; }

		/// <summary>
		/// This is used to mark when we make the DB call, to prevent all subsequent calls, as RTE fields were loaded already
		/// </summary>
		private bool preventRteDbLoad { get; set; }

		// The Travel Description
		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				description = value;
				WasDescriptionSet = true;
			}
		}

		#endregion

		public int BoeID { get; set; }

		public ICollection<TravelTripType> TravelTrips { get; set; }

		public ICollection<MSTTravelTripType> MSTTravelTrips { get; set; }


		public DateTime? StartDate { get; set; }

		public DateTime? EndDate { get; set; }

		public Collection<CustomFieldValueContainer> CustomFieldValueContainers { get; set; }

		/// <summary>
		/// Gets the children that can be shifted.
		/// </summary>
		public ICollection<IDateShiftable> Children
		{
			get
			{
				return new List<IDateShiftable>();
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has a spread of values.
		/// </summary>
		public bool HasSpread
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets the date shift level.
		/// </summary>
		public Level DateShiftLevel
		{
			get
			{
				return Level.Travel;
			}
		}

		#region Required Validation Messages

		public const string ONE_TRIP_REQUIRED = "At least one trip is required for a task element";
		public const string DATE_OUT_OF_RANGE = "Date must be between {0} and {1}";
		public const string TRAVEL_TASK_START_DATE_INVALID = "Start Date must be on or after the BOE Start Date: {0}";
		public const string TRAVEL_TASK_END_DATE_INVALID = "End Date must be on or before the BOE End Date: {0}";
		public const string TRAVEL_TASK_SEGMENT_INVALID = "Segment is invalid.";
		public const string TRAVEL_TASK_PERFORG_INVALID = "Performing Org is required.";
		public const string TRIP_START_DATE_INVALID = "Trip Start Date must be on or after {0} Start Date: {1}";
		public const string TRIP_END_DATE_INVALID = "Trip End Date must be on or before {0} End Date: {1}";

		#endregion
	}
}