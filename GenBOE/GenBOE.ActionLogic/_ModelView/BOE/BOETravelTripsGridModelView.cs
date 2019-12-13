using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using IES.Common;
using GenBOE.Dtos;

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// Model view for travel trips
    /// </summary>
    public class BOETravelTripsGridModelView : PersistedDataModelView
    {
        public string DisplayEvent { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BOETravelTripsGridModelView()
        {
            this.PerDiemID = -1;

            this.PerformingOrgID = -1;
            this.PerformingOrgName = String.Empty;

            this.Deleted = false;

            this.TravelTripID = -1;
            this.SystemTripID = -1;
            this.Segment = SegmentType.None;
            this.TripDate = DateTime.Today;
            this.Purpose = string.Empty;
            this.numOfDays = 1;
            this.numOfPeople = 1;
            this.numOfTrips = 1;
            this.GroupID = null;
            this.UpdateDate = DateTime.Today;

            this.CustomFieldValues = new Collection<CustomFieldSelectionModelView>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="theDTO"></param>
        public BOETravelTripsGridModelView(TravelTripType theDTO)
            : this()
        {
            if (theDTO != null)
            {
                this.TravelTripID = theDTO.TravelTripID;
                this.SystemTripID = theDTO.SystemTripID;
                this.Segment = theDTO.Segment;
                this.PerformingOrgID = theDTO.PerfOrgID;
                this.TripDate = theDTO.TripDate;
                this.Purpose = theDTO.Purpose;
                this.numOfDays = theDTO.NumOfDays;
                this.numOfPeople = theDTO.NumOfPeople;
                this.numOfTrips = theDTO.NumOfTrips;
                this.GroupID = theDTO.GroupID;
                this.UpdateDate = theDTO.UpdateDate;

                if (theDTO.CustomFieldValueContainers != null)
                {
                    foreach (CustomFieldValueContainer container in theDTO.CustomFieldValueContainers)
                    {
                        this.CustomFieldValues.Add(new CustomFieldSelectionModelView
                        {
                            CustomFieldValueID = container.CustomFieldValueID,
                            SelectionID = container.ContainerID,
                            UpdateDate = container.UpdateDate,
                            CustomFieldID = container.CustomFieldID,
                            IsOpenEnded = container.IsOpenEnded,
                            OpenEndedValue = container.OpenEndedValue
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Gets/Sets TravelTripID
        /// </summary>
        public int TravelTripID { get; set; }

        /// <summary>
        /// Gets/Sets SystemTripID
        /// </summary>
        public int SystemTripID { get; set; }

        /// <summary>
        /// Gets/Sets GroupID
        /// </summary>
        [RegularExpression(@"[0-9]\d{0,3}", ErrorMessage = "ID must be a number from 1 to 9999.")]
        [Range(1, 9999, ErrorMessage = "ID must be a number from 1 to 9999.")]
        public int? GroupID { get; set; }

        /// <summary>
        /// Gets/Sets Segment
        /// </summary>
        [Required(ErrorMessage = "Segment is required.")]
        public SegmentType Segment { get; set; }

        /// <summary>
        /// Gets/Sets PerformingOrgID
        /// </summary>
        [Required(ErrorMessage = "Performing Org is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Performing Org is invalid.")]
        public int PerformingOrgID { get; set; }

        /// <summary>
        /// Gets/Sets PerformingOrgName
        /// </summary>
        public string PerformingOrgName { get; set; }

        /// <summary>
        /// Gets/Sets ModeID
        /// </summary>
        [Required(ErrorMessage = "Mode is required.")]
        public string Mode { get; set; }
        public int ModeID { get; set; }

        /// <summary>
        /// Gets/Sets DepartureID
        /// </summary>
        public string DepartureName { get; set; }
        [Required(ErrorMessage = "Departure is required.")]
        public int DepartureID { get; set; }

        /// <summary>
        /// Gets/Sets DestinationID
        /// </summary>
        public string DestinationName { get; set; }
        [Required(ErrorMessage = "Destination is required.")]
        public int DestinationID { get; set; }

        /// <summary>
        /// Gets/Sets Purpose
        /// </summary>
        [Required(ErrorMessage = "Purpose is required.")]
        public string Purpose { get; set; }

        /// <summary>
        /// Gets/Sets PerDiemID
        /// </summary>
        public int PerDiemID { get; set; }

        /// <summary>
        /// Gets/Sets TripDate
        /// </summary>
        [Required(ErrorMessage = "Date is required.")]
        public DateTime TripDate { get; set; }

        /// <summary>
        /// Gets/Sets numOfPeople
        /// </summary>
        [Required(ErrorMessage = "# People must be a value from 1 to 999.")]
        [Range(1, 999, ErrorMessage = "# People must be a value from 1 to 999.")]
        public int numOfPeople { get; set; }

        /// <summary>
        /// Gets/Sets numOfDays
        /// </summary>
        [Required(ErrorMessage = "# Days must be a value from 1 to 999.")]
        [Range(1, 999, ErrorMessage = "# Days must be a value from 1 to 999.")]
        public int numOfDays { get; set; }

        /// <summary>
        /// Gets/Sets numOfTrips
        /// </summary>
        [Required(ErrorMessage = "# Trips must be a value from 1 to 999.  Negative values can be entered for credit proposals.")]
        [Range(-999, 999, ErrorMessage = "# Trips must be a value from 1 to 999.  Negative values can be entered for credit proposals.")]
        public int numOfTrips { get; set; }

        /// <summary>
        /// Gets/Sets Cost
        /// </summary>
        public long Cost { get; set; }

        /// <summary>
        /// Gets/Sets Deleted
        /// </summary>
        public Boolean Deleted { get; set; }

        /// <summary>
        /// Gets/Sets numOfOccurrences
        /// </summary>
        [Range(1, 99, ErrorMessage = "# Occurrences must be a value from 1 to 99.")]
        public int? numOfOccurrences { get; set; }

        /// <summary>
        /// Gets/Sets Interval
        /// </summary>
        [Range(1, 99, ErrorMessage = "Interval must be a value from 1 to 99.")]
        public int? Interval { get; set; }

        /// <summary>
        /// Gets/Sets a <see cref="Boolean"/> indicating if the segment help link should be shown
        /// </summary>
        public Boolean ShowSegmentHelpLink { get; set; }

        /// <summary>
        /// Gets/Sets a <see cref="Boolean"/> indicating if the create multiple checkbox is checked
        /// </summary>
        public Boolean CreateMultiple { get; set; }

        /// <summary>
        /// Gets/Sets CustomFieldValues
        /// </summary>
        public Collection<CustomFieldSelectionModelView> CustomFieldValues { get; set; }
    }
}
