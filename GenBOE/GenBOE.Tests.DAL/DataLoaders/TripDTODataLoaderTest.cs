// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.DataBridge.DTO;
using System.Collections.ObjectModel;
using GenBOE.Dtos;
using IES.Common;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class TripDTODataLoaderTest : MOQLoaderObject
    {
        TripDTODataLoader sut = null;
        [TestInitialize]
        public void Init()
        {
            sut = new TripDTODataLoader();
        }

        [TestMethod]
        public void L_GetTripByTripID()
        {
            int rateID = MiscTravelRate.Id;
            TripDTO trip = new TripDTO();
            trip.TripID = -1;
            trip.Updateable = UpdateType.Upsert;
            trip.UpdateDate = DateTime.Now;
            trip.MiscTravelRateID = rateID;
            trip.DepartureLocationID = this.DepartureLocation.Id;
            trip.DestinationLocationID = this.DestinationLocation.Id;
            trip.PerDiemID = this.PerDiem.Id;
            trip.Fare = 50;
            trip.RTMiles = 50;
            trip.FareUpdatedByUserID = this.Author.UserID;
            trip.DestinationLocationCode = "DCE";
            trip.DepartureLocationCode = "ALB";


            Dictionary<int, int> savedId = sut.SaveTrips(new Collection<TripDTO> { trip });

            trip = sut.GetTripByTripID(savedId[-1]);

            Assert.IsTrue(trip.TripID > 0, "trip id valid");
            Assert.AreEqual(trip.DepartureLocationCode, "ALB");
            Assert.AreEqual(trip.DepartureLocationID, this.DepartureLocation.Id);
            Assert.AreEqual(trip.DestinationLocationID, this.DestinationLocation.Id);
            Assert.AreEqual(trip.PerDiemID, this.PerDiem.Id);
            Assert.AreEqual(trip.Fare, 50);
            Assert.AreEqual(trip.RTMiles, 50);

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetAllTrips()
        {
            int rateID = MiscTravelRate.Id;
            // create a trip 

            TripDTO trip = new TripDTO();
            trip.TripID = -1;
            trip.Updateable = UpdateType.Upsert;
            trip.UpdateDate = DateTime.Now;
            trip.MiscTravelRateID = rateID;
            trip.DepartureLocationID = this.DepartureLocation.Id;
            trip.DestinationLocationID = this.DestinationLocation.Id;
            trip.PerDiemID = this.PerDiem.Id;
            trip.Fare = 60;
            trip.RTMiles = 60;
            trip.FareUpdatedByUserID = this.Author.UserID;


            Dictionary<int, int> idSaved = sut.SaveTrips(new Collection<TripDTO> { trip });

            ICollection<TripDTO> trips = sut.GetAllTrips();

            Assert.IsTrue(trips.Any(), "No trips exist");

            trip = sut.GetTripByTripID(idSaved[-1]);

            Assert.IsTrue(trips.Select(x => x.TripID).Contains(idSaved[-1]));

            this.ResetTestData();
        }

        [TestMethod]
        public void L_SaveTrips()
        {
            int rateID = MiscTravelRate.Id;
            // create a trip 

            TripDTO trip = new TripDTO();
            trip.TripID = -1;
            trip.Updateable = UpdateType.Upsert;
            trip.UpdateDate = DateTime.Now;
            trip.MiscTravelRateID = rateID;
            trip.DepartureLocationID = this.DepartureLocation.Id;
            trip.DestinationLocationID = this.DestinationLocation.Id;
            trip.PerDiemID = this.PerDiem.Id;
            trip.Fare = 70;
            trip.RTMiles = 70;
            trip.FareUpdatedByUserID = this.Author.UserID;


            int beforeSave = sut.GetAllTrips().Count;
            Dictionary<int, int> ids = sut.SaveTrips(new Collection<TripDTO> { trip });
            int afterSave = sut.GetAllTrips().Count;

            Assert.AreEqual(beforeSave + 1, afterSave, "Save didn't work");

            // refresh the trip
            trip = sut.GetTripByTripID(ids[-1]);

            //edit the trip
            trip.RTMiles = 100;
            trip.Updateable = UpdateType.Upsert;
            sut.SaveTrips(new Collection<TripDTO> { trip });

            trip = sut.GetTripByTripID(trip.TripID);
            Assert.AreEqual(100, trip.RTMiles, "rt miles doesn't match");

            this.ResetTestData();
        }


        [TestMethod]
        //Save two trips, one with a brand new per diem and one using the former per diem
        public void L_SaveTwoTrips()
        {
            int rateID = MiscTravelRate.Id;
            int departLocate = this.DepartureLocation.Id;
            // create a trip 
            TripDTO trip = new TripDTO();
            trip.TripID = -1;
            trip.Updateable = UpdateType.Upsert;
            trip.UpdateDate = DateTime.Now;
            trip.MiscTravelRateID = rateID;
            trip.DepartureLocationID = departLocate;
            trip.DestinationLocationID = this.DestinationLocation.Id;
            trip.PerDiemID = this.PerDiem.Id;
            trip.Fare = 20;
            trip.RTMiles = 20;
            trip.FareUpdatedByUserID = this.Author.UserID;
            trip.DepartureLocationCode = "VA";
            trip.DestinationLocationCode = "MD";

            Dictionary<int, int> savedId = sut.SaveTrips(new Collection<TripDTO> { trip });
            trip = sut.GetTripByTripID(savedId[-1]);

            int afterFirstSave = sut.GetAllTrips().Count;

            // create a second trip using an existing per diem but with different rates
            TripDTO trip2 = new TripDTO();
            trip2.TripID = -1;
            trip2.Updateable = UpdateType.Upsert;
            trip2.UpdateDate = DateTime.Now;
            trip2.MiscTravelRateID = rateID;

            trip2.DepartureLocationID = departLocate;
            trip2.DestinationLocationID = this.DestinationLocation.Id;
            trip2.PerDiemID = trip.PerDiemID;
            trip2.Fare = 500;
            trip2.RTMiles = 500;
            trip2.FareUpdatedByUserID = this.Author.UserID;
            trip2.DepartureLocationCode = "PA";
            trip2.DestinationLocationCode = "MD";

            Dictionary<int, int> saved2Id = sut.SaveTrips(new Collection<TripDTO> { trip2 });

            int afterSecondSave = sut.GetAllTrips().Count;
            Assert.AreEqual(afterSecondSave, afterFirstSave + 1, "Save didn't work");

            trip2 = sut.GetTripByTripID(saved2Id[-1]);

            this.ResetTestData();
        }

        [TestMethod]
        public void GetTripUniqueDataUsingLocationNames()
        {
            var rateDL = new MiscTravelRateDTOLoader();
            int rateID = rateDL.GetAll().First().Id;
            // create a trip 

            TripDTO trip = new TripDTO();
            trip.TripID = -1;
            trip.Updateable = UpdateType.Upsert;
            trip.UpdateDate = DateTime.Now;
            trip.MiscTravelRateID = rateID;
            trip.DepartureLocationID = GlobalTestCaseSetup.GlobalDepartureLocationID;
            trip.DestinationLocationID = GlobalTestCaseSetup.GlobalDestLocationID;
            trip.PerDiemID = GlobalTestCaseSetup.GlobalPerDiemID;
            trip.Fare = 80m;
            trip.RTMiles = 80;
            trip.RentalCarRate = 80m;
            trip.FareUpdatedByUserID = GlobalTestCaseSetup.GlobalBOEAuthorID;

            sut.SaveTrips(new Collection<TripDTO> { trip });

            ICollection<TripDTO> trips = sut.GetAllTrips();

            int tripToSaveID = (from t in trips
                                where t.Fare == 80m && t.DestinationLocationID == GlobalTestCaseSetup.GlobalDestLocationID && t.DepartureLocationID == GlobalTestCaseSetup.GlobalDepartureLocationID && t.Fare == 80m
                                select t.TripID).First();

            trip = sut.GetTripByTripID(tripToSaveID);

            var perDiemLoader = new PerDiemDTODataLoader();
            string qualificationToSearch = perDiemLoader.GetByIds(new Collection<int> { GlobalTestCaseSetup.GlobalPerDiemID }).First().Qualification;

            var LocationLoader = new LocationDTODataLoader();
            string destLocation = LocationLoader.GetById(trip.DestinationLocationID).LocationName;
            string departLocation = LocationLoader.GetById(trip.DepartureLocationID).LocationName;

            int possibleTripCount = sut.GetTripByUniqueTripDataUsingLocationNames(rateID, destLocation, departLocation, qualificationToSearch).Count();

            Assert.IsTrue(possibleTripCount > 0, "no possible trips given trip data entered");

            // reset data so that we don't get 2 restore attempts on 1 WS within 1 minute of each other which will cause a db error
            this.ResetTestData();
        }
    }
}
