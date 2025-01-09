// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.ZoneTravel
{
    using System;
    using System.Collections.Generic;
    using GenBOE.ActionLogic.ZoneTravel;
	using IES.Common;
	using IES.Common.classes;
	using Microsoft.Practices.Unity;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	[TestClass]
	public class NonZoneTravelCalculationTest
    {
		private Mock<IActiveDirectoryUtilities> activeDirectoryUtilities = new Mock<IActiveDirectoryUtilities>();

		private Dictionary<int, decimal> EscalationRates
        {
            get
            {
                Dictionary<int, decimal> temp = new Dictionary<int, decimal>();
                    temp.Add(2011, 3);
                    temp.Add(2016, 5);
                    temp.Add(2020, 8);

                return temp;
            }
        }

		/// <summary>
		/// Test Initialize
		/// </summary>
		[TestInitialize]
		public void CreateSystem()
		{
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IActiveDirectoryUtilities), activeDirectoryUtilities.Object);
		}

		[TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void NoData_Exception()
        {
            NonZoneTravelCalculation calc = new NonZoneTravelCalculation();

            Assert.IsFalse(calc.TotalTripCost == 0);
        }

        [TestMethod]
        public void NoData()
        {
            NonZoneTravelCalculation calc = new NonZoneTravelCalculation()
            {
                YearOfEstimate = 2016,
                YearOfTrip = 2016,
                AirfareEscalationRates = EscalationRates,
                PerDiemEscalationRates = EscalationRates,
                MiscEscalationRates = EscalationRates
            };

            Assert.AreEqual(0, calc.TotalAirfare);
            Assert.AreEqual(0, calc.TotalCarRental);
            Assert.AreEqual(0, calc.TotalPerDiem);
            Assert.AreEqual(0, calc.TotalTripCost);
        }

        [TestMethod]
        public void FullData_Domestic()
        {
            NonZoneTravelCalculation calc = new NonZoneTravelCalculation()
            {
                AirfareCost = (decimal)100.1,
                DailyCarRentalRate = (decimal)20.32,
                DailyPerDiemRate = (decimal)13.10,
                MiscOtherDailyFee = (decimal)224.01,
                TravelAgencyFee = (decimal)329.11,
                NumberOfCars = (decimal)2.129843209832094,
                NumberOfDays = (decimal)18.11111111,
                NumberOfPeople = (decimal)2.9999,
                YearOfEstimate = 2016,
                YearOfTrip = 2020,
                AirfareEscalationRates = EscalationRates,
                PerDiemEscalationRates = EscalationRates,
                MiscEscalationRates = EscalationRates,
                DecimalPrecision = 0
            };

            Assert.AreEqual((decimal)480.48, calc.TotalAirfare);
            Assert.AreEqual((decimal)1170.432, calc.TotalCarRental);
            Assert.AreEqual((decimal)1068.96, calc.TotalPerDiem);
            Assert.AreEqual((decimal)987.33, Math.Round(calc.TotalTravelAgencyFee,2));
            Assert.AreEqual((decimal)23061.67, calc.TotalTripCost);
        }

        [TestMethod]
        public void FullData_Domestic_WithDecimalPrecision()
        {
            NonZoneTravelCalculation calc = new NonZoneTravelCalculation()
            {
                AirfareCost = (decimal)100.1,
                DailyCarRentalRate = (decimal)20.32,
                DailyPerDiemRate = (decimal)13.10,
                MiscOtherDailyFee = (decimal)224.01,
                TravelAgencyFee = (decimal)329.11,
                NumberOfCars = (decimal)2.82982094,
                NumberOfDays = (decimal)18.5811,
                NumberOfPeople = (decimal)2.9999,
                YearOfEstimate = 2016,
                YearOfTrip = 2020,
                AirfareEscalationRates = EscalationRates,
                PerDiemEscalationRates = EscalationRates,
                MiscEscalationRates = EscalationRates,
                DecimalPrecision = 2
            };

            Assert.AreEqual((decimal)480.48, calc.TotalAirfare);
            Assert.AreEqual((decimal)1709.5264768, calc.TotalCarRental);
            Assert.AreEqual((decimal)1105.4304, calc.TotalPerDiem);
            Assert.AreEqual((decimal)987.33, Math.Round(calc.TotalTravelAgencyFee, 2));
            Assert.AreEqual((decimal)24260.87, calc.TotalTripCost);
        }

        [TestMethod]
        public void FullData_Domestic_WithDecimalPrecision_EscalationDollars()
        {
            NonZoneTravelCalculation calc = new NonZoneTravelCalculation()
            {
                AirfareCost = (decimal)100.1,
                DailyCarRentalRate = (decimal)20.32,
                DailyPerDiemRate = (decimal)13.10,
                MiscOtherDailyFee = (decimal)224.01,
                TravelAgencyFee = (decimal)329.11,
                NumberOfCars = (decimal)2.82982094,
                NumberOfDays = (decimal)18.5811,
                NumberOfPeople = (decimal)2.9999,
                YearOfEstimate = 2016,
                YearOfTrip = 2020,
                AirfareEscalationRates = EscalationRates,
                PerDiemEscalationRates = EscalationRates,
                MiscEscalationRates = EscalationRates,
                DecimalPrecision = 2
            };

            decimal totalCostExpected = (decimal)24260.87;
            decimal unescalatedCosts = calc.TotalTravelAgencyFee + calc.UnescalatedTotalAirfare + calc.UnescalatedTotalPerDiem + calc.UnescalatedTotalCarRental + calc.UnescalatedTotalOtherMiscDailyFee;
            decimal escalationDollars = Math.Round(totalCostExpected - unescalatedCosts, 2);

            Assert.AreEqual((decimal)480.48, calc.TotalAirfare);
            Assert.AreEqual((decimal)1709.5264768, calc.TotalCarRental);
            Assert.AreEqual((decimal)1105.4304, calc.TotalPerDiem);
            Assert.AreEqual((decimal)987.33, Math.Round(calc.TotalTravelAgencyFee, 2));
            Assert.AreEqual(totalCostExpected, calc.TotalTripCost);
            Assert.AreEqual(escalationDollars, calc.EscalationDollars);
        }

        [TestMethod]
        public void FullData_International()
        {
            NonZoneTravelCalculation calc = new NonZoneTravelCalculation()
            {
                AirfareCost = (decimal)100.1,
                DailyCarRentalRate = (decimal)20.32,
                DailyPerDiemRate = (decimal)13.10,
                MiscOtherDailyFee = (decimal)224.01,
                TravelAgencyFee = (decimal)329.11,
                NumberOfCars = 2,
                NumberOfDays = 18,
                NumberOfPeople = 3,
                IsDomesticTravel = false,
                YearOfEstimate = 2011,
                YearOfTrip = 2020,
                AirfareEscalationRates = EscalationRates,
                PerDiemEscalationRates = EscalationRates,
                MiscEscalationRates = EscalationRates
            };

            Assert.AreEqual((decimal)800.8, calc.TotalAirfare);
            Assert.AreEqual((decimal)1950.72, Math.Round(calc.TotalCarRental, 2));
            Assert.AreEqual((decimal)1781.6, calc.TotalPerDiem);
            Assert.AreEqual((decimal)987.33, Math.Round(calc.TotalTravelAgencyFee, 2));
            Assert.AreEqual((decimal)5520.45,calc.TotalTripCost);
        }

        [TestMethod]
        public void FullData_OtherCtor()
        {
            NonZoneTravelCalculation calc = new NonZoneTravelCalculation(18, 3, 2, (decimal)13.10, (decimal)20.32, (decimal)100.1, EscalationRates, EscalationRates, EscalationRates, 2016, 2020, true, (decimal)329.11, (decimal)224.01, 0);

            Assert.AreEqual((decimal)480.48, calc.TotalAirfare);
            Assert.AreEqual((decimal)1170.432, calc.TotalCarRental);
            Assert.AreEqual((decimal)1068.96, calc.TotalPerDiem);
            Assert.AreEqual((decimal)987.33, Math.Round(calc.TotalTravelAgencyFee, 2));
            Assert.AreEqual((decimal)23061.67, calc.TotalTripCost);
        }

        [TestMethod]
        public void NoCarsNoOtherFeesInternational()
        {
            NonZoneTravelCalculation calc = new NonZoneTravelCalculation()
            {
                AirfareCost = (decimal)100.1,
                DailyPerDiemRate = (decimal)13.10,
                TravelAgencyFee = (decimal)329.11,
                NumberOfDays = 18,
                NumberOfPeople = 3,
                MiscOtherDailyFee = 1000,
                IsDomesticTravel = false,
                YearOfEstimate = 2016,
                YearOfTrip = 2016,
                AirfareEscalationRates = EscalationRates,
                PerDiemEscalationRates = EscalationRates,
                MiscEscalationRates = EscalationRates
            };

            Assert.AreEqual((decimal)300.3, calc.TotalAirfare);
            Assert.AreEqual(0, calc.TotalCarRental);
            Assert.AreEqual((decimal)668.1, calc.TotalPerDiem);
            Assert.AreEqual((decimal)987.33, Math.Round(calc.TotalTravelAgencyFee, 2));
            Assert.AreEqual((decimal)1955.73, calc.TotalTripCost);
        }

        [TestMethod]
        public void NoCarsNoFlight()
        {
            NonZoneTravelCalculation calc = new NonZoneTravelCalculation()
            {
                DailyPerDiemRate = (decimal)13.10,
                MiscOtherDailyFee = (decimal)32.21,
                TravelAgencyFee = (decimal)3.11,
                NumberOfDays = 18,
                NumberOfPeople = 3,
                YearOfEstimate = 2011,
                YearOfTrip = 2016,
                AirfareEscalationRates = EscalationRates,
                PerDiemEscalationRates = EscalationRates,
                MiscEscalationRates = EscalationRates
            };

            Assert.AreEqual(0, calc.TotalAirfare);
            Assert.AreEqual(0, calc.TotalCarRental);
            Assert.AreEqual((decimal)1113.5, calc.TotalPerDiem);
            Assert.AreEqual((decimal)9.33, Math.Round(calc.TotalTravelAgencyFee, 2));
            Assert.AreEqual((decimal)4021.73, Math.Round(calc.TotalTripCost, 2));
        }
    }
}