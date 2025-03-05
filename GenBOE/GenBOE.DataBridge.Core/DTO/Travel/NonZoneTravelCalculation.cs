// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO.Travel
{
	using System;
	using System.Collections.Generic;
	using IES.Common;
	using IES.Common.Core.Utilities;

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
			AirfareCost = 0;
			DailyCarRentalRate = 0;
			DailyPerDiemRate = 0;
			NumberOfCars = 0;
			NumberOfDays = 0;
			NumberOfPeople = 0;
			TravelAgencyFee = 0;
			MiscOtherDailyFee = 0;
			IsDomesticTravel = true;
			YearOfEstimate = 0;
			YearOfTrip = 0;
			DecimalPrecision = 0;
			AirfareEscalationRates = new Dictionary<int, decimal>();
			PerDiemEscalationRates = new Dictionary<int, decimal>();
			MiscEscalationRates = new Dictionary<int, decimal>();
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
			AirfareCost = airfareCost;
			DailyCarRentalRate = dailyCarRentalRate;
			DailyPerDiemRate = dailyPerDiemRate;
			NumberOfCars = numberOfCars;
			NumberOfDays = numberOfDays;
			NumberOfPeople = numberOfPeople;
			TravelAgencyFee = travelAgencyFee;
			MiscOtherDailyFee = miscOtherFee;
			IsDomesticTravel = isDomesticTrip;
			YearOfEstimate = yearOfEstimate;
			YearOfTrip = yearOfTrip;
			DecimalPrecision = decimalPrecision;

			AirfareEscalationRates = airfareEscalationRates;
			PerDiemEscalationRates = perDiemEscalationRates;
			MiscEscalationRates = miscEscalationRates;
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
				if (AirfareEscalationRates == null ||
					!AirfareEscalationRates.ContainsKey(YearOfEstimate) ||
					!AirfareEscalationRates.ContainsKey(YearOfTrip) ||
					AirfareEscalationRates[YearOfEstimate] == 0)
				{
					throw new DivideByZeroException("Escalation rate could not be calculated");
				}

				return AirfareEscalationRates[YearOfTrip] / AirfareEscalationRates[YearOfEstimate];
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
				if (PerDiemEscalationRates == null ||
					!PerDiemEscalationRates.ContainsKey(YearOfEstimate) ||
					!PerDiemEscalationRates.ContainsKey(YearOfTrip) ||
					PerDiemEscalationRates[YearOfEstimate] == 0)
				{
					throw new DivideByZeroException("Escalation rate could not be calculated");
				}

				return PerDiemEscalationRates[YearOfTrip] / PerDiemEscalationRates[YearOfEstimate];
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
				if (MiscEscalationRates == null ||
					!MiscEscalationRates.ContainsKey(YearOfEstimate) ||
					!MiscEscalationRates.ContainsKey(YearOfTrip) ||
					MiscEscalationRates[YearOfEstimate] == 0)
				{
					throw new DivideByZeroException("Escalation rate could not be calculated");
				}

				return MiscEscalationRates[YearOfTrip] / MiscEscalationRates[YearOfEstimate];
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
				return AirfareCost * CommonUtilities.AdjustPrecision(NumberOfPeople, DecimalPrecision);
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
				return DailyPerDiemRate
					* CommonUtilities.AdjustPrecision(NumberOfPeople, DecimalPrecision)
					* CommonUtilities.AdjustPrecision(NumberOfDays - 1, DecimalPrecision);
			}
		}

		/// <summary>
		/// Total Car Rental - NOT ESCALATED
		/// </summary>
		public decimal UnescalatedTotalCarRental
		{
			get
			{
				return DailyCarRentalRate
					* CommonUtilities.AdjustPrecision(NumberOfCars, DecimalPrecision)
					* CommonUtilities.AdjustPrecision(NumberOfDays, DecimalPrecision);
			}
		}

		/// <summary>
		/// Total Other/Misc Daily Fee - NOT ESCALATED
		/// </summary>
		public decimal UnescalatedTotalOtherMiscDailyFee
		{
			get
			{
				return IsDomesticTravel ?
					MiscOtherDailyFee * CommonUtilities.AdjustPrecision(NumberOfDays, DecimalPrecision) * CommonUtilities.AdjustPrecision(NumberOfPeople, DecimalPrecision)
					: 0;
			}
		}

		/// <summary>
		/// Total Airfare
		/// </summary>
		public decimal TotalAirfare
		{
			get
			{
				return AirfareEscalationRateMultiplier * UnescalatedTotalAirfare;
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
				return PerDiemEscalationRateMultiplier * UnescalatedTotalPerDiem;
			}
		}

		/// <summary>
		/// Total Car Rental
		/// </summary>
		public decimal TotalCarRental
		{
			get
			{
				return MiscEscalationRateMultiplier * UnescalatedTotalCarRental;
			}
		}

		/// <summary>
		/// Total Other/Misc Daily Fee
		/// </summary>
		public decimal TotalOtherMiscDailyFee
		{
			get
			{
				return MiscEscalationRateMultiplier * UnescalatedTotalOtherMiscDailyFee;
			}
		}

		/// <summary>
		/// Total Travel Agency Fee
		/// </summary>
		public decimal TotalTravelAgencyFee
		{
			get
			{
				return TravelAgencyFee * CommonUtilities.AdjustPrecision(NumberOfPeople, DecimalPrecision);
			}
		}

		/// <summary>
		/// Escalation Dollars
		/// </summary>
		public decimal EscalationDollars
		{
			get
			{
				return Math.Round(TotalTripCost -
					(UnescalatedTotalAirfare + UnescalatedTotalCarRental + UnescalatedTotalPerDiem + UnescalatedTotalOtherMiscDailyFee + TotalTravelAgencyFee), 2);
			}
		}

		/// <summary>
		/// Total Trip Cost
		/// </summary>
		public decimal TotalTripCost
		{
			get
			{
				return Math.Round(TotalAirfare + TotalPerDiem + TotalCarRental + TotalOtherMiscDailyFee + TotalTravelAgencyFee, 2);
			}
		}

		#endregion
	}
}