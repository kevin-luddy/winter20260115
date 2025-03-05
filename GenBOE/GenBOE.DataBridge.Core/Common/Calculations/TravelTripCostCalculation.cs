// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Common.Calculations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.DataBridge.Core.DTO;
	using GenBOE.DataBridge.Core.DTO.FullObjects;
	using GenBOE.DataBridge.Core.DTO.Travel;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Enums;

	public class TravelTripCostCalculation
	{
		/// <summary>
		/// Calculate the travel cost for each travel trip (with preloaded data)
		/// </summary>
		/// <param name="inTravelTrip">Travel Trip</param>
		/// <param name="inWorkspace">Full WS</param>
		/// <param name="trip">Trip</param>
		/// <param name="miscRate">Misc Rate</param>
		/// <param name="perDiem">Per Diem</param>
		/// <param name="escalations">Escalations</param>
		/// <returns>Travel Trip Cost Data</returns>
		public virtual TravelTripCostData CalculateTravelCost(TravelTripType inTravelTrip, WorkspaceDTO inWorkspace, TripDTO trip, decimal miscRate, PerDiemDTO perDiem, IReadOnlyCollection<EscalationRatesDTO> escalations)
		{
			if (inTravelTrip == null)
			{
				throw new ArgumentNullException(nameof(inTravelTrip));
			}
			if (inWorkspace == null)
			{
				throw new ArgumentNullException(nameof(inWorkspace));
			}
			if (trip == null)
			{
				throw new ArgumentNullException(nameof(trip));
			}
			if (perDiem == null)
			{
				throw new ArgumentNullException(nameof(perDiem));
			}
			if (escalations == null)
			{
				throw new ArgumentNullException(nameof(escalations));
			}

			decimal costTotal = 0.0m;
			decimal Fare = 0.0m;
			decimal Hotel = 0.0m;
			decimal MieRate = 0.0m;
			decimal RentalCar = 0.0m;
			decimal FareTotal = 0.0m;
			decimal HotelTotal = 0.0m;
			decimal MIETotal = 0.0m;
			decimal RentalCarTotal = 0.0m;
			decimal MiscTotal = 0.0m;
			decimal EscalationRate = 0.0m;
			decimal NumOfCars = 0.0m;
			decimal TotalCostNoEscalation = 0.0m;
			decimal EscalationDollars = 0.0m;

			DateTime DateWSStatusChange = DateTime.MinValue;

			// Get the dates as strings from the config file
			string sMIECostUpdateActivationDate = ConfigurationUtilities.GetAppSetting("MIECostUpdateActivationDate");
			string sMIECostUpdateTripDate = ConfigurationUtilities.GetAppSetting("MIECostUpdateTripDate");

			// Validate as valid dates
			DateTime MIECostUpdateActivationDate;
			bool ActivationDateParse = DateTime.TryParse(sMIECostUpdateActivationDate, out MIECostUpdateActivationDate);
			DateTime MIECostUpdateTripDate;
			bool TripDateParse = DateTime.TryParse(sMIECostUpdateTripDate, out MIECostUpdateTripDate);

			// CanApplyNewFormula indicates that dates retrived from config are valid for comparison not that the new formula should be applied in this trip case
			// MinValue is returned by default so we make sure this is not the case
			bool CanApplyNewFormula = ActivationDateParse && TripDateParse
				&& DateTime.Compare(MIECostUpdateActivationDate.Date, DateTime.MinValue.Date) > 0
				&& DateTime.Compare(MIECostUpdateTripDate.Date, DateTime.MinValue.Date) > 0;

			if (inWorkspace.WorkspaceState == WorkspaceState.Locked
				|| inWorkspace.WorkspaceState == WorkspaceState.Closed
				|| inWorkspace.WorkspaceState == WorkspaceState.Complete)
			{
				// TODO TIW
				//if (inWorkspace.WorkspaceHistory != null)
				//{
				//	DateWSStatusChange = inWorkspace.WorkspaceHistory.Where(x => x.NewValue == inWorkspace.WorkspaceState).OrderByDescending(y => y.Date).First().Date;
				//}
			}

			Hotel = perDiem.HotelRate;
			MieRate = perDiem.MIERate;
			RentalCar = trip.RentalCarRate;
			Fare = trip.Fare;

			if (inTravelTrip.Segment == SegmentType.LS)
			{
				EscalationRate = (from e in escalations
								  where inTravelTrip.TripDate.Year == e.Year
								  select e.LMSIEscalation).FirstOrDefault();
			}
			else
			{

				EscalationRate = (from e in escalations
								  where inTravelTrip.TripDate.Year == e.Year
								  select e.DevEscalation).FirstOrDefault();
			}

			FareTotal = Fare * inTravelTrip.NumOfPeople * inTravelTrip.NumOfTrips;
			HotelTotal = Hotel * (inTravelTrip.NumOfDays - 1) * inTravelTrip.NumOfPeople * inTravelTrip.NumOfTrips;
			NumOfCars = Math.Ceiling(Convert.ToDecimal(inTravelTrip.NumOfPeople / (decimal)2));

			if (CanApplyNewFormula  // if the config dates exist and are valid
				&& DateTime.Compare(DateTime.Now.Date, MIECostUpdateActivationDate.Date) >= 0 // Today is on or after the new formula activation date
					&& (DateTime.Compare(inTravelTrip.TripDate.Date, MIECostUpdateTripDate.Date) >= 0 && inTravelTrip.NumOfDays > 1   // if the current trip date is on or after the trip date in the config file and the trip has multiple days
					&& (inWorkspace.WorkspaceState == WorkspaceState.Initialization || inWorkspace.WorkspaceState == WorkspaceState.Working)    // if the workspace is in init or working
					|| (inWorkspace.WorkspaceState == WorkspaceState.Closed || inWorkspace.WorkspaceState == WorkspaceState.Complete || inWorkspace.WorkspaceState == WorkspaceState.Locked) // if the workspace is in closed, complete or locked
					&& DateTime.Compare(DateWSStatusChange.Date, MIECostUpdateActivationDate.Date) >= 0 // and the date it was set to either state is on or after the activation date from the config file
					&& DateTime.Compare(inTravelTrip.TripDate.Date, MIECostUpdateTripDate.Date) >= 0 && inTravelTrip.NumOfDays > 1) // the current trip date is on or after the trip date in the config file and the trip has multiple days
				)
			{
				// use the formula that limits cost on first and last day to 75%
				MIETotal = MieRate * Convert.ToDecimal(inTravelTrip.NumOfDays - 0.50) * inTravelTrip.NumOfPeople * inTravelTrip.NumOfTrips;
			}
			else
			{
				// use the formula that limits cost on the first day only to 75%
				MIETotal = MieRate * Convert.ToDecimal(inTravelTrip.NumOfDays - 0.25) * inTravelTrip.NumOfPeople * inTravelTrip.NumOfTrips;
			}
			//Note: Needed to add double to NumOfPeople/2 because having just ints produced wrong results like 1/2 would return 0 and it should be 0.5
			RentalCarTotal = RentalCar * NumOfCars * inTravelTrip.NumOfDays * inTravelTrip.NumOfTrips;

			MiscTotal = miscRate * inTravelTrip.NumOfPeople * inTravelTrip.NumOfTrips;

			TotalCostNoEscalation = FareTotal + HotelTotal + MIETotal + RentalCarTotal + MiscTotal;
			EscalationDollars = TotalCostNoEscalation * EscalationRate;
			costTotal = TotalCostNoEscalation + EscalationDollars;


			return new TravelTripCostData(costTotal, FareTotal, HotelTotal, MIETotal, RentalCarTotal, MiscTotal, EscalationRate, NumOfCars, TotalCostNoEscalation, EscalationDollars);
		}
	}

	/// <summary>
	/// Encapsulates the totals and data used for a trip calculation
	/// </summary>
	public class TravelTripCostData
	{
		#region Private members

		private decimal mCostTotal;
		private decimal mFareTotal;
		private decimal mHotelTotal;
		private decimal mMIETotal;
		private decimal mRentalCarTotal;
		private decimal mMiscTotal;
		private decimal mEscalationRate;
		private decimal mNumOfCars;
		private decimal mTotalCostNoEscalation;
		private decimal mEscalationDollars;

		#endregion Private members

		#region Public members

		public TravelTripCostData(decimal CostTotal, decimal FareTotal, decimal HotelTotal, decimal MIETotal, decimal RentalCarTotal, decimal MiscTotal,
			decimal EscalationRate, decimal NumOfCars, decimal TotalCostNoEscalation, decimal EscalationDollars)
		{
			mCostTotal = CostTotal;
			mFareTotal = FareTotal;
			mHotelTotal = HotelTotal;
			mMIETotal = MIETotal;
			mRentalCarTotal = RentalCarTotal;
			mMiscTotal = MiscTotal;
			mEscalationRate = EscalationRate;
			mNumOfCars = NumOfCars;
			mTotalCostNoEscalation = TotalCostNoEscalation;
			mEscalationDollars = EscalationDollars;
		}

		/// <summary>
		/// Get the cost total from the calculation
		/// </summary>
		public decimal CostTotal
		{
			get { return mCostTotal; }
		}


		/// <summary>
		/// Get the fare total from the calculation
		/// </summary>
		public decimal FareTotal
		{
			get { return mFareTotal; }
		}

		/// <summary>
		/// Get the hotel total from the calculation
		/// </summary>
		public decimal HotelTotal
		{
			get { return mHotelTotal; }
		}

		/// <summary>
		/// Get the MIE total from the calculation
		/// </summary>
		public decimal MIETotal
		{
			get { return mMIETotal; }
		}

		/// <summary>
		/// Get the rental car total from the calculation
		/// </summary>
		public decimal RentalCarTotal
		{
			get { return mRentalCarTotal; }
		}

		/// <summary>
		/// Get the misc cost total from the calculation
		/// </summary>
		public decimal MiscTotal
		{
			get { return mMiscTotal; }
		}

		/// <summary>
		/// Get the retrieved excalation rate used in the calculation
		/// </summary>
		public decimal EscalationRate
		{
			get { return mEscalationRate; }
		}

		/// <summary>
		/// Get the number of cars used in the calculation
		/// </summary>
		public decimal NumberOfCars
		{
			get { return mNumOfCars; }
		}

		/// <summary>
		/// Get the total cost without escalation
		/// </summary>
		public decimal TotalCostNoEscalation
		{
			get { return mTotalCostNoEscalation; }
		}

		/// <summary>
		/// Get the escalation dollars
		/// </summary>
		public decimal EscalatinDollars
		{
			get { return mEscalationDollars; }
		}

		#endregion  Public members
	}
}
