// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Common;
    using GenBOE.Dtos;

    [Serializable()]
    public class TravelDTO : UpdateableDTO, IBOEMembership, IStartEndDates, IDateShiftable
    {
        private string description;

        [NonSerialized]
        private static TravelDTODataLoader loader;

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
            
            this.description = null;
            this.WasDescriptionSet = false;
            this.preventRteDbLoad = false;
            if (loader == null) { loader = new TravelDTODataLoader(new TravelTripTaskElementCustomFieldValueXREFLoader(), new TravelTripCustomFieldValueXREFLoader()); }
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
                if (this.Id > 0 && !this.WasDescriptionSet && !this.preventRteDbLoad)
                {
                    this.preventRteDbLoad = true;
                    loader.LoadRTEFields(new List<TravelDTO>() { this });
                }

                return this.description;
            }
            set
            {
                this.description = value;
                this.WasDescriptionSet = true;
            }
        }

        #endregion

        public int BoeID { get; set; }

        /// <summary>
        /// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
        /// </summary>
        internal IEnumerable<TravelTripType> TravelTripsIEnum { get; set; }

        public ICollection<TravelTripType> TravelTrips { get; set; }



        internal IEnumerable<MSTTravelTripType> MSTTravelTripsIEnum { get; set; }

        public ICollection<MSTTravelTripType> MSTTravelTrips { get; set; }


        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        /// <summary>
        /// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
        /// </summary>
        internal IEnumerable<CustomFieldValueContainer> CustomFieldValueContainersIEnum { get; set; }

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