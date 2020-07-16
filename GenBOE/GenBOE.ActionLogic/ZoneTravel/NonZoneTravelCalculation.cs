// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ZoneTravel
{
    using System;
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// NonZone Travel Calculation Class
    /// 
    /// Does calculations for RMS Zone Travel (the non-zone portion)
    /// </summary>
    public class NonZoneTravelCalculation
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public NonZoneTravelCalculation()
        {
            this.AirfareCost = 0;
            this.DailyCarRentalRate = 0;
            this.DailyPerDiemRate = 0;
            this.NumberOfCars = 0;
            this.NumberOfDays = 0;
            this.NumberOfPeople = 0;
            this.TravelAgencyFee = 0;
            this.MiscOtherDailyFee = 0;
            this.IsDomesticTravel = true;
            this.YearOfEstimate = 0;
            this.YearOfTrip = 0;
            this.DecimalPrecision = 0;
            this.AirfareEscalationRates = new Dictionary<int, decimal>();
            this.PerDiemEscalationRates = new Dictionary<int, decimal>();
            this.MiscEscalationRates = new Dictionary<int, decimal>();
        }

        /// <summary>
        /// Ctor w/ all of the values
        /// </summary>
        /// <param name="numberOfDays">Number of days</param>
        /// <param name="numberOfPeople">Number of people</param>
        /// <param name="numberOfCars">Number of cars</param>
        /// <param name="dailyPerDiemRate">Daily Per Diem Rate</param>
        /// <param name="dailyCarRentalRate">Daily Car Rental Rate</param>
        /// <param name="airfareCost">Airfare cost</param>
        /// <param name="airfareEscalationRates">Escalation Rates</param>
        /// <param name="yearOfEstimate">Year of Estimate</param>
        /// <param name="yearOfTrip">Year of Trip</param>
        /// <param name="isDomesticTrip">Is Domestic Travel</param>
        /// <param name="travelAgencyFee">Travel Agency Fee</param>
        /// <param name="miscOtherFee">Misc/Other Fee</param>
        /// <param name="decimalPrecision">Decimal Precision</param>
        public NonZoneTravelCalculation(decimal numberOfDays, decimal numberOfPeople, decimal numberOfCars, decimal dailyPerDiemRate, decimal dailyCarRentalRate, decimal airfareCost, 
            Dictionary<int, decimal> airfareEscalationRates, Dictionary<int, decimal> perDiemEscalationRates, Dictionary<int, decimal> miscEscalationRates,
            int yearOfEstimate, int yearOfTrip, bool isDomesticTrip, decimal travelAgencyFee, decimal miscOtherFee, int decimalPrecision)
        {
            this.AirfareCost = airfareCost;
            this.DailyCarRentalRate = dailyCarRentalRate;
            this.DailyPerDiemRate = dailyPerDiemRate;
            this.NumberOfCars = numberOfCars;
            this.NumberOfDays = numberOfDays;
            this.NumberOfPeople = numberOfPeople;
            this.TravelAgencyFee = travelAgencyFee;
            this.MiscOtherDailyFee = miscOtherFee;
            this.IsDomesticTravel = isDomesticTrip;
            this.YearOfEstimate = yearOfEstimate;
            this.YearOfTrip = yearOfTrip;
            this.DecimalPrecision = decimalPrecision;

            this.AirfareEscalationRates = airfareEscalationRates;
            this.PerDiemEscalationRates = perDiemEscalationRates;
            this.MiscEscalationRates = miscEscalationRates;
        }

        /// <summary>
        /// Decimal precision for:
        ///     # of days
        ///     # of cars
        ///     # of people
        /// </summary>
        public int DecimalPrecision { get; set; }

        #region Inputs

        /// <summary>
        /// Airfare cost
        /// </summary>
        public decimal AirfareCost { get; set; }

        /// <summary>
        /// Daily per-diem rate
        /// </summary>
        public decimal DailyPerDiemRate { get; set; }

        /// <summary>
        /// Daily car rental rate
        /// </summary>
        public decimal DailyCarRentalRate { get; set; }

        /// <summary>
        /// Number of people
        /// </summary>
        public decimal NumberOfPeople { get; set; }

        /// <summary>
        /// Number of days
        /// </summary>
        public decimal NumberOfDays { get; set; }

        /// <summary>
        /// Number of cars
        /// </summary>
        public decimal NumberOfCars { get; set; }

        /// <summary>
        /// Indicates whether the travel is domestic or not
        /// </summary>
        public bool IsDomesticTravel { get; set; }

        #endregion

        #region Escalation Rate Inputs and Calculations

        /// <summary>
        /// Airfare Escalation rates. Format: Key: Year, Value: Escalation rate
        /// </summary>
        public Dictionary<int, decimal> AirfareEscalationRates { get; set; }

        /// <summary>
        /// Per Diem Escalation rates. Format: Key: Year, Value: Escalation rate
        /// </summary>
        public Dictionary<int, decimal> PerDiemEscalationRates { get; set; }

        /// <summary>
        /// Misc Fee Escalation rates. Format: Key: Year, Value: Escalation rate
        /// </summary>
        public Dictionary<int, decimal> MiscEscalationRates { get; set; }

        /// <summary>
        /// Year in which the trips is being estimated (to be used w/ escalation rates)
        /// </summary>
        public int YearOfEstimate { get; set; }

        /// <summary>
        /// Year in which the trip will be taken (to be used w/ escalation rates)
        /// </summary>
        public int YearOfTrip { get; set; }

        /// <summary>
        /// Airfare Escalation rate multiplier:
        /// 
        /// (rate as of the year of trip / rate as of the year of estimate)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public decimal AirfareEscalationRateMultiplier
        {
            get
            {
                if(this.AirfareEscalationRates == null ||
                    !this.AirfareEscalationRates.ContainsKey(this.YearOfEstimate) ||
                    !this.AirfareEscalationRates.ContainsKey(this.YearOfTrip) ||
                    this.AirfareEscalationRates[this.YearOfEstimate] == 0)
                {
                    throw new DivideByZeroException("Escalation rate could not be calculated");
                }

                return this.AirfareEscalationRates[this.YearOfTrip] / this.AirfareEscalationRates[this.YearOfEstimate];
            }
        }

        /// <summary>
        /// Per Diem Escalation rate multiplier:
        /// 
        /// (rate as of the year of trip / rate as of the year of estimate)
        /// </summary>
        private decimal PerDiemEscalationRateMultiplier
        {
            get
            {
                if (this.PerDiemEscalationRates == null ||
                    !this.PerDiemEscalationRates.ContainsKey(this.YearOfEstimate) ||
                    !this.PerDiemEscalationRates.ContainsKey(this.YearOfTrip) ||
                    this.PerDiemEscalationRates[this.YearOfEstimate] == 0)
                {
                    throw new DivideByZeroException("Escalation rate could not be calculated");
                }

                return this.PerDiemEscalationRates[this.YearOfTrip] / this.PerDiemEscalationRates[this.YearOfEstimate];
            }
        }

        /// <summary>
        /// Misc Fees Escalation rate multiplier:
        /// 
        /// (rate as of the year of trip / rate as of the year of estimate)
        /// </summary>
        private decimal MiscEscalationRateMultiplier
        {
            get
            {
                if (this.MiscEscalationRates == null ||
                    !this.MiscEscalationRates.ContainsKey(this.YearOfEstimate) ||
                    !this.MiscEscalationRates.ContainsKey(this.YearOfTrip) ||
                    this.MiscEscalationRates[this.YearOfEstimate] == 0)
                {
                    throw new DivideByZeroException("Escalation rate could not be calculated");
                }

                return this.MiscEscalationRates[this.YearOfTrip] / this.MiscEscalationRates[this.YearOfEstimate];
            }
        }

        #endregion

        #region System Level entered fees

        /// <summary>
        /// System level Travel Agency Fee
        /// </summary>
        public decimal TravelAgencyFee { get; set; }

        /// <summary>
        /// System level Other/Misc Fee
        /// 
        /// For domestic trips only
        /// </summary>
        public decimal MiscOtherDailyFee { get; set; }

        #endregion

        #region Calculated values

        /// <summary>
        /// Total Airfare - NOT ESCALATED
        /// </summary>
        public decimal UnescalatedTotalAirfare
        {
            get
            {
                return this.AirfareCost * Utilities.AdjustPrecision(this.NumberOfPeople, this.DecimalPrecision);
            }
        }

        /// <summary>
        /// Total Per Diem - NOT ESCALATED
        /// 
        /// First and last days count as 1/2 days only => NumberOfDays - 1
        /// </summary>
        public decimal UnescalatedTotalPerDiem
        {
            get
            {
                return this.DailyPerDiemRate 
                    * Utilities.AdjustPrecision(this.NumberOfPeople, this.DecimalPrecision)
                    * Utilities.AdjustPrecision(this.NumberOfDays - 1, this.DecimalPrecision);
            }
        }

        /// <summary>
        /// Total Car Rental - NOT ESCALATED
        /// </summary>
        public decimal UnescalatedTotalCarRental
        {
            get
            {
                return this.DailyCarRentalRate 
                    * Utilities.AdjustPrecision(this.NumberOfCars, this.DecimalPrecision) 
                    * Utilities.AdjustPrecision(this.NumberOfDays, this.DecimalPrecision);
            }
        }

        /// <summary>
        /// Total Other/Misc Daily Fee - NOT ESCALATED
        /// </summary>
        public decimal UnescalatedTotalOtherMiscDailyFee
        {
            get
            {
                return (this.IsDomesticTravel ? 
                    (this.MiscOtherDailyFee * Utilities.AdjustPrecision(this.NumberOfDays, this.DecimalPrecision) * Utilities.AdjustPrecision(this.NumberOfPeople, this.DecimalPrecision)) 
                    : 0);
            }
        }

        /// <summary>
        /// Total Airfare
        /// </summary>
        public decimal TotalAirfare
        {
            get
            {
                return this.AirfareEscalationRateMultiplier * this.UnescalatedTotalAirfare;
            }
        }

        /// <summary>
        /// Total Per Diem
        /// 
        /// First and last days count as 1/2 days only => NumberOfDays - 1
        /// </summary>
        public decimal TotalPerDiem
        {
            get
            {
                return this.PerDiemEscalationRateMultiplier * this.UnescalatedTotalPerDiem;
            }
        }

        /// <summary>
        /// Total Car Rental
        /// </summary>
        public decimal TotalCarRental
        {
            get
            {
                return this.MiscEscalationRateMultiplier * this.UnescalatedTotalCarRental;
            }
        }

        /// <summary>
        /// Total Other/Misc Daily Fee
        /// </summary>
        public decimal TotalOtherMiscDailyFee
        {
            get
            {
                return this.MiscEscalationRateMultiplier * this.UnescalatedTotalOtherMiscDailyFee;
            }
        }

        /// <summary>
        /// Total Travel Agency Fee
        /// </summary>
        public decimal TotalTravelAgencyFee
        {
            get
            {
                return this.TravelAgencyFee * Utilities.AdjustPrecision(this.NumberOfPeople, this.DecimalPrecision);
            }
        }

        /// <summary>
        /// Escalation Dollars
        /// </summary>
        public decimal EscalationDollars
        {
            get
            {
                return Math.Round(this.TotalTripCost - 
                    (this.UnescalatedTotalAirfare + this.UnescalatedTotalCarRental + this.UnescalatedTotalPerDiem + this.UnescalatedTotalOtherMiscDailyFee + this.TotalTravelAgencyFee), 2);
            }
        }

        /// <summary>
        /// Total Trip Cost
        /// </summary>
        public decimal TotalTripCost
        {
            get
            {
                return Math.Round(this.TotalAirfare + this.TotalPerDiem + this.TotalCarRental + this.TotalOtherMiscDailyFee + this.TotalTravelAgencyFee, 2);
            }
        }

        #endregion
    }
}