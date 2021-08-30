// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.ObjectModel;
    using IES.Common;
    using GenBOE.Dtos;

    public class BOEZoneTravelTripsGridModelView : PersistedDataModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BOEZoneTravelTripsGridModelView()
        {
            this.TravelTripID = -1;
            this.GroupID = null;

            this.PerformingOrgID = -1;
            this.PerformingOrgName = string.Empty;

            this.NonZoneResourceID = -1;
            this.NonZoneResourceName = string.Empty;
            this.ResourceIdForExport = string.Empty;
            this.SecondaryResourceIdForDisplay = string.Empty;

            this.Deleted = false;

            this.OriginID = null;
            this.DestinationCity = string.Empty;
            this.DestinationStateID = null;

            this.FromLocation = string.Empty;
            this.ToLocation = string.Empty;

            this.Purpose = string.Empty;
            this.EstTripDate = DateTime.Today;
            this.DateOfEstimate = DateTime.Today;
            this.UpdateDate = DateTime.Today;

            this.NumOfPeople = null;
            this.NumOfDays = null;
            this.NumOfCars = null;
            this.AirfareEst = null;
            this.PerDiemDaily = null;
            this.CarRentalTrans = null;
            this.TravelAgencyFee = null;
            this.MiscOtherCosts = null;

            this.YearOfEstimate = null;

            this.CustomFieldValues = new Collection<CustomFieldSelectionModelView>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tripType">trip data</param>
        public BOEZoneTravelTripsGridModelView(MSTTravelTripType tripType)
            : this()
        {
            if (tripType == null)
            {
                throw new ArgumentNullException(nameof(tripType));
            }

            this.TravelTripID = tripType.Id;
            this.ModeID = tripType.ModeID;
            this.GroupID = tripType.GroupID;
            this.PerformingOrgID = tripType.PerfOrgID;
            this.NonZoneResourceID = tripType.NonZoneResourceID;
            this.Deleted = tripType.Updateable == UpdateType.Deleted;
            this.OriginID = tripType.ZoneOriginID;
            this.DestinationCity = tripType.ZoneDestCity ?? string.Empty;
            this.DestinationStateID = tripType.ZoneDestinationID;
            this.FromLocation = tripType.NonZoneFrom ?? string.Empty;
            this.ToLocation = tripType.NonZoneTo ?? string.Empty;
            this.Purpose = tripType.Purpose;
            this.EstTripDate = tripType.TripDate;
            this.DateOfEstimate = tripType.EstimateDate;
            this.NumOfPeople = tripType.NumOfPeople;
            this.NumOfDays = tripType.NumOfDays;
            this.NumOfCars = tripType.NonZoneNumCars;
            this.AirfareEst = tripType.NonZoneAirfareEstimate;
            this.PerDiemDaily = tripType.NonZonePerDiemDaily;
            this.CarRentalTrans = tripType.NonZoneCarRentalTrans;
            this.TravelAgencyFee = tripType.NonZoneTravelAgencyFee;
            this.MiscOtherCosts = tripType.NonZoneMiscOtherCosts;
            this.UpdateDate = tripType.UpdateDate;
            this.Cost = tripType.Cost;
            this.YearOfEstimate = tripType.EstimateDate.Year;
            this.ClinId = tripType.ClinId;
            this.WbsId = tripType.WbsId;
            this.ResourceIdForExport = tripType.ResourceIdForExport;
            this.SecondaryResourceIdForDisplay = tripType.SecondaryResourceIdForExport;


            if (tripType.CustomFieldValueContainers != null)
            {
                foreach (CustomFieldValueContainer container in tripType.CustomFieldValueContainers)
                {
                    this.CustomFieldValues.Add(new CustomFieldSelectionModelView
                    {
                        CustomFieldID = container.CustomFieldID,
                        CustomFieldValueID = container.CustomFieldValueID,
                        SelectionID = container.ContainerID,
                        UpdateDate = container.UpdateDate,
                        IsOpenEnded = container.IsOpenEnded,
                        OpenEndedValue = container.OpenEndedValue
                    });
                }
            }
        }

        /// <summary>
        /// Get/Set the TravelTripID
        /// </summary>
        public int TravelTripID { get; set; }

        /// <summary>
        /// Get/Set the GroupID
        /// </summary>
        public int? GroupID { get; set; }

        /// <summary>
        /// Get/Set the ModeID
        /// </summary>
        public MSTTravelMode ModeID { get; set; }

        /// <summary>
        /// Get/Set the PerformingOrgID
        /// </summary>
        public int PerformingOrgID { get; set; }
        /// <summary>
        /// Get/Set the PerformingOrgName
        /// </summary>
        public string PerformingOrgName { get; set; }

        /// <summary>
        /// Get/Set the NonZoneResourceID
        /// </summary>
        public int? NonZoneResourceID { get; set; }
        /// <summary>
        /// Get/Set the NonZoneResourceName
        /// </summary>
        public string NonZoneResourceName { get; set; }

        /// <summary>
        /// Get/Set the ResourceIdExport 
        /// </summary>
        public string ResourceIdForExport { get; set; } // TODO: can rename in trip class, used outside propricer

        ///// <summary>
        ///// Get/Set the SecondaryResourceIdForProPricerExport
        ///// </summary>
        public string SecondaryResourceIdForDisplay { get; set; } // TODO: check if needed
        /// <summary>
        /// Get/Set the OriginID for zone
        /// </summary>
        public int? OriginID { get; set; }
        /// <summary>
        /// Get/Set the OriginName for zone
        /// </summary>
        public string OriginName { get; set; }

        /// <summary>
        /// Get/Set the DestinationCity for zone
        /// </summary>
        public string DestinationCity { get; set; }
        /// <summary>
        /// Get/Set the DestinationStateID for zone
        /// </summary>
        public int? DestinationStateID { get; set; }
        /// <summary>
        /// Get/Set the DestinationStateName for zone
        /// </summary>
        public string DestinationStateName { get; set; }

        /// <summary>
        /// Get/Set the Zone for zone
        /// </summary>
        public int Zone { get; set; }

        /// <summary>
        /// Get/Set the Purpose
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        /// Get/Set the Estimated Trip Date
        /// </summary>
        public DateTime EstTripDate { get; set; }

        /// <summary>
        /// Get/Set the Date of Estimate
        /// </summary>
        public DateTime DateOfEstimate { get; set; }

        /// <summary>
        /// Get/Set the number of people
        /// </summary>
        public decimal? NumOfPeople { get; set; }

        /// <summary>
        /// Get/Set the number of days 
        /// </summary>
        public decimal? NumOfDays { get; set; }

        /// <summary>
        /// Get/Set the number of rental cars for non-zone
        /// </summary>
        public decimal? NumOfCars { get; set; }

        /// <summary>
        /// Decimal precision (for string formatting)
        /// </summary>
        public int DecimalPlaces { get; set; }

        /// <summary>
        /// String to be used on the view for number of people
        /// </summary>
        public string NumOfPeopleStr
        {
            get
            {
                return Utilities.FormatStringWithPrecision(this.NumOfPeople ?? 0, this.DecimalPlaces);
            }
        }

        /// <summary>
        /// String to be used on the view for number of days
        /// </summary>
        public string NumOfDaysStr
        {
            get
            {
                return Utilities.FormatStringWithPrecision(this.NumOfDays ?? 0, this.DecimalPlaces);
            }
        }

        /// <summary>
        /// String to be used on the view for number of cars
        /// </summary>
        public string NumOfCarsStr
        {
            get
            {
                string result = Constants.NOT_APPLICABLE;

                if (this.ModeID == MSTTravelMode.NonZoneDomestic || this.ModeID == MSTTravelMode.NonZoneInternational)
                {
                    result = this.NumOfCars.HasValue ? Utilities.FormatStringWithPrecision(this.NumOfCars.Value, this.DecimalPlaces) : string.Empty;
                }

                return result;
            }
        }

        /// <summary>
        /// Get/Set the FromLocation for non-zone
        /// </summary>
        public string FromLocation { get; set; }

        /// <summary>
        /// Get/Set the ToLocation for non-zone
        /// </summary>
        public string ToLocation { get; set; }

        /// <summary>
        /// Get/Set the Airfare Estimate for non-zone
        /// </summary>
        public decimal? AirfareEst { get; set; }

        /// <summary>
        /// Get/Set the Per Diem Daily for non-zone
        /// </summary>
        public decimal? PerDiemDaily { get; set; }

        /// <summary>
        /// Get/Set the Car Rental/Transportation for non-zone
        /// </summary>
        public decimal? CarRentalTrans { get; set; }

        /// <summary>
        /// Get/Set the Travel Agency Fee for non-zone
        /// </summary>
        public decimal? TravelAgencyFee { get; set; }

        /// <summary>
        /// Get/Set the Misc/Other Costs for non-zone domestic
        /// </summary>
        public decimal? MiscOtherCosts { get; set; }

        /// <summary>
        /// Gets/Sets Cost
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Get the name of the departure location for display on the grid
        /// OriginName for Zone, FromLocation for Non-Zone
        /// </summary>
        public string DepartureName
        {
            get
            {
                return (this.ModeID == MSTTravelMode.ZoneNoAirfare || this.ModeID == MSTTravelMode.ZoneAirfare) ? this.OriginName : this.FromLocation;
            }
        }

        /// <summary>
        /// Get the name of the destination location for display on the grid
        /// Destination City and State for Zone, ToLocation for Non-Zone
        /// </summary>
        public string DestinationName
        {
            get
            {
                return (this.ModeID == MSTTravelMode.ZoneNoAirfare || this.ModeID == MSTTravelMode.ZoneAirfare) ? this.DestinationCity + ", " + this.DestinationStateName : this.ToLocation;
            }
        }

        /// <summary>
        /// Get/Set the deleted boolean
        /// </summary>
        public Boolean Deleted { get; set; }

        /// <summary>
        /// Get/Set the boolean for Create Multiple trips
        /// </summary>
        public Boolean CreateMultiple { get; set; }

        /// <summary>
        /// Get/Set the  number of occurences of multiple trips
        /// </summary>
        public int? numOccurrences { get; set; }

        /// <summary>
        /// Year when the estimate was put together
        /// </summary>
        public int? YearOfEstimate { get; set; }

        /// <summary>
        /// Get/Set the interval of multiple trips
        /// </summary>
        public int? interval { get; set; }

        /// <summary>
        /// Get/Set the CustomFieldValues
        /// </summary>
        public Collection<CustomFieldSelectionModelView> CustomFieldValues { get; set; }

        /// <summary>
        /// Multi Clin/Wbs -> Clin Id
        /// </summary>
        public int? ClinId { get; set; }

        /// <summary>
        /// Multi Clin/Wbs -> Clin Text
        /// </summary>
        public string ClinText { get; set; }

        /// <summary>
        /// Multi Clin/Wbs -> Wbs Id
        /// </summary>
        public int? WbsId { get; set; }

        /// <summary>
        /// Multi Clin/Wbs -> Wbs
        /// </summary>
        public string WbsText { get; set; }
    }
}
