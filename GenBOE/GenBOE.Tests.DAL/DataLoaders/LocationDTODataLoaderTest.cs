// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using System.Transactions;

    [TestClass]
    public class LocationDTODataLoaderTest
    {
        [TestMethod]
        public void L_GetById()
        {
            var sut = new LocationDTODataLoader();

            // call GetLocation to make sure there is a location in the db
            int LocationID = GlobalTestCaseSetup.GetLocation();

            LocationDTO location = sut.GetById(LocationID);

            Assert.IsTrue(location.Id > 0, "location id isn't valid");
            Assert.IsTrue(location.LocationName.Contains("MOCK"), "Mock location name wasn't found");
        }

        [TestMethod]
        public void L_GetAllLocations()
        {
            var sut = new LocationDTODataLoader();
            // call GetLocation to make sure there is a location in the db
            GlobalTestCaseSetup.GetLocation();

            ICollection<LocationDTO> locations = sut.GetAllLocations();

            Assert.IsTrue(locations.Count > 0, "no locations found");
        }

        [TestMethod]
        public void L_GetLocationName()
        {
            var sut = new LocationDTODataLoader();
            // call GetLocation to make sure there is a location in the db
           int locationID = GlobalTestCaseSetup.GetLocation();

           string name = sut.GetLocationName(locationID);

           Assert.IsTrue(!string.IsNullOrEmpty(name), "name isn't empty");
        }

        [TestMethod]
        public void L_SaveLocation()
        {
            var sut = new LocationDTODataLoader();

            string name = Guid.NewGuid().ToString();
            LocationDTO location = new LocationDTO { Id =- 1, LocationName = "MOCK" + name, LastUpdatedBy = GlobalTestCaseSetup.GlobalBOEAuthorID, Updateable = UpdateType.Upsert };

            using (TransactionScope scope = new TransactionScope())
            {
                int? locationID = sut.Save(location);

                Assert.IsNotNull(locationID);
                Assert.IsTrue(locationID > 0, "location id isn't valid");
                scope.Complete();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void L_SaveLocation_EditNotAllowed()
        {
            var sut = new LocationDTODataLoader();

            string name = Guid.NewGuid().ToString();
            LocationDTO location = new LocationDTO { Id = 2, LocationName = "MOCK" + name, LastUpdatedBy = GlobalTestCaseSetup.GlobalBOEAuthorID, Updateable = UpdateType.Upsert };

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(location);
                scope.Complete();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void L_SaveLocation_DeleteNotAllowed()
        {
            var sut = new LocationDTODataLoader();

            string name = Guid.NewGuid().ToString();
            LocationDTO location = new LocationDTO { Id = 2, LocationName = "MOCK" + name, LastUpdatedBy = GlobalTestCaseSetup.GlobalBOEAuthorID, Updateable = UpdateType.Deleted };

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(location);
                scope.Complete();
            }
        }
    }
}
