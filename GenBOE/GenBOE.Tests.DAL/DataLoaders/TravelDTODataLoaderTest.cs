// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TravelDTODataLoaderTest : MOQLoaderObject
    {
        /// <summary>
        /// Create a loader for testing
        /// </summary>
        /// <returns>Test loader</returns>
        private TravelDTODataLoader CreateTestLoader()
        {
            var _travelTripTaskElementCustomFieldValue = new Mock<ITravelTripTaskElementCustomFieldValueXREFLoader>();
            var _travelTripCustomFieldValue = new Mock<ITravelTripCustomFieldValueXREFLoader>();

            return new TravelDTODataLoader(_travelTripTaskElementCustomFieldValue.Object,_travelTripCustomFieldValue.Object);
        }

        [TestMethod]
        public void L_GetTravelCollectionByBoeIds()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            TravelTripType travelTrip = new TravelTripType();
            travelTrip.TravelTripID = -1;
            travelTrip.SystemTripID = this.SystemTrip.TripID;
            travelTrip.BoeID = boeID;
            travelTrip.GroupID = 2;
            travelTrip.NumOfDays = 2;
            travelTrip.NumOfIntervals = 0;
            travelTrip.NumOfOccurences = 0;
            travelTrip.NumOfPeople = 3;
            travelTrip.NumOfTrips = 2;
            travelTrip.PerfOrgID = this.Perforg.Id;
            travelTrip.Segment = IES.Common.SegmentType.SSC;
            travelTrip.TripDate = DateTime.Now;
            travelTrip.Updateable = UpdateType.Upsert;
            travelTrip.UpdateDate = DateTime.Now;

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = "mockTravelTitle";
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";
            travel.TravelTrips = new Collection<TravelTripType> { travelTrip };

            int beforeSaveTravel = sut.GetByBoeIds(new Collection<int> { boeID }).Count;
            sut.SaveTravels(new Collection<TravelDTO> { travel });
            int afterSaveTravel = sut.GetByBoeIds(new Collection<int> { boeID }).Count;
            Assert.IsTrue(afterSaveTravel == beforeSaveTravel + 1, "save didn't work");

            ICollection<TravelDTO> travelDtos = sut.GetByBoeIds(new Collection<int> { boeID });

            Assert.IsTrue(travelDtos.Count == 1, "no travel elements");
            Assert.IsTrue(travelDtos.First().TravelTrips.Count == 1, "no travel trips");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetTravelIDsByWorkspaceID()
        {
            var sut = this.CreateTestLoader();

            // save one travel so there is always at least one in the db
            TravelTripType travelTrip1 = new TravelTripType();
            travelTrip1.TravelTripID = -1;
            travelTrip1.SystemTripID = this.SystemTrip.TripID;
            travelTrip1.BoeID = this.Boe1.Id;
            travelTrip1.GroupID = 2;
            travelTrip1.NumOfDays = 2;
            travelTrip1.NumOfIntervals = 0;
            travelTrip1.NumOfOccurences = 0;
            travelTrip1.NumOfPeople = 3;
            travelTrip1.NumOfTrips = 2;
            travelTrip1.PerfOrgID = this.Perforg.Id;
            travelTrip1.Segment = IES.Common.SegmentType.SSC;
            travelTrip1.TripDate = DateTime.Now;
            travelTrip1.Updateable = UpdateType.Upsert;
            travelTrip1.UpdateDate = DateTime.Now;

            TravelTripType travelTrip2 = new TravelTripType();
            travelTrip2.TravelTripID = -2;
            travelTrip2.SystemTripID = this.SystemTrip.TripID;
            travelTrip2.BoeID = this.Boe2.Id;
            travelTrip2.GroupID = 2;
            travelTrip2.NumOfDays = 2;
            travelTrip2.NumOfIntervals = 0;
            travelTrip2.NumOfOccurences = 0;
            travelTrip2.NumOfPeople = 3;
            travelTrip2.NumOfTrips = 2;
            travelTrip2.PerfOrgID = this.Perforg.Id;
            travelTrip2.Segment = IES.Common.SegmentType.SSC;
            travelTrip2.TripDate = DateTime.Now;
            travelTrip2.Updateable = UpdateType.Upsert;
            travelTrip2.UpdateDate = DateTime.Now;

            TravelDTO travel1 = new TravelDTO();
            travel1.Id = -1;
            travel1.Updateable = UpdateType.Upsert;
            travel1.UpdateDate = DateTime.Now;
            travel1.TaskID = "ETA";
            travel1.TaskTitle = "mockTravelTitle";
            travel1.BoeID = this.Boe1.Id;
            travel1.Description = "mock travel desc";
            travel1.TravelTrips = new Collection<TravelTripType> { travelTrip1 };

            TravelDTO travel2 = new TravelDTO();
            travel2.Id = -2;
            travel2.Updateable = UpdateType.Upsert;
            travel2.UpdateDate = DateTime.Now;
            travel2.TaskID = "ETA";
            travel2.TaskTitle = "mockTravelTitle";
            travel2.BoeID = this.Boe2.Id;
            travel2.Description = "mock travel desc";
            travel2.TravelTrips = new Collection<TravelTripType> { travelTrip2 };

            int beforeSaveTravel = sut.GetByBoeIds(new Collection<int>() { this.Boe1.Id, this.Boe2.Id }).Count;
            Dictionary<int, int> ids = sut.SaveTravels(new Collection<TravelDTO> { travel1, travel2 });
            travel1 = this._travelDL.GetById(ids[-1]);
            travel2 = this._travelDL.GetById(ids[-2]);
            int afterSaveTravel = sut.GetByBoeIds(new Collection<int>() { this.Boe1.Id, this.Boe2.Id }).Count;

            Assert.IsTrue(afterSaveTravel == beforeSaveTravel + 2, "save didn't work");

            ICollection<TravelDTO> travels = sut.GetByWorkspaceId(this.Workspace.Id);

            Assert.IsTrue(travels.Count >= 2, "no travel ids");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetTravelDataByTravelID()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            TravelTripType travelTrip = new TravelTripType();
            travelTrip.TravelTripID = -1;
            travelTrip.SystemTripID = this.SystemTrip.TripID;
            travelTrip.BoeID = boeID;
            travelTrip.GroupID = 2;
            travelTrip.NumOfDays = 2;
            travelTrip.NumOfIntervals = 0;
            travelTrip.NumOfOccurences = 0;
            travelTrip.NumOfPeople = 3;
            travelTrip.NumOfTrips = 2;
            travelTrip.PerfOrgID = this.Perforg.Id;
            travelTrip.Segment = IES.Common.SegmentType.SSC;
            travelTrip.TripDate = DateTime.Now;
            travelTrip.Updateable = UpdateType.Upsert;
            travelTrip.UpdateDate = DateTime.Now;

            string guid = Guid.NewGuid().ToString();

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = guid;
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";
            travel.TravelTrips = new Collection<TravelTripType> { travelTrip };

            int beforeSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            Dictionary<int, int> ids = sut.SaveTravels(new Collection<TravelDTO> { travel });
            int afterSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            travel = this._travelDL.GetById(ids[-1]);
            Assert.IsTrue(afterSaveTravel == beforeSaveTravel + 1, "save didn't work");

            TravelDTO returnedTravel = sut.GetById(travel.Id);

            Assert.IsTrue(returnedTravel.Id > 0, "travel id is valid");
            Assert.IsTrue(returnedTravel.TaskTitle == guid, "task title didn't match");
            Assert.IsTrue(returnedTravel.TravelTrips.Count > 0, "no travel trips found");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetByIds()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            TravelTripType travelTrip = new TravelTripType();
            travelTrip.TravelTripID = -1;
            travelTrip.SystemTripID = this.SystemTrip.TripID;
            travelTrip.BoeID = boeID;
            travelTrip.GroupID = 2;
            travelTrip.NumOfDays = 2;
            travelTrip.NumOfIntervals = 0;
            travelTrip.NumOfOccurences = 0;
            travelTrip.NumOfPeople = 3;
            travelTrip.NumOfTrips = 2;
            travelTrip.PerfOrgID = this.Perforg.Id;
            travelTrip.Segment = IES.Common.SegmentType.SSC;
            travelTrip.TripDate = DateTime.Now;
            travelTrip.Updateable = UpdateType.Upsert;
            travelTrip.UpdateDate = DateTime.Now;

            string guid = Guid.NewGuid().ToString();

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = guid;
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";
            travel.TravelTrips = new Collection<TravelTripType> { travelTrip };

            int beforeSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            Dictionary<int, int> ids = sut.SaveTravels(new Collection<TravelDTO> { travel });
            int afterSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            travel = this._travelDL.GetById(ids[-1]);
            Assert.IsTrue(afterSaveTravel == beforeSaveTravel + 1, "save didn't work");

            ICollection<TravelDTO> returnedTravel = sut.GetByIds(new Collection<int> { travel.Id });

            Assert.IsTrue(returnedTravel.Any());
            Assert.IsTrue(returnedTravel.First().Id > 0, "travel id is valid");
            Assert.IsTrue(returnedTravel.First().TaskTitle == guid, "task title didn't match");
            Assert.IsTrue(returnedTravel.First().TravelTrips.Count > 0, "no travel trips found");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_SaveTravel()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            TravelTripType travelTrip = new TravelTripType();
            travelTrip.TravelTripID = -1;
            travelTrip.SystemTripID = this.SystemTrip.TripID;
            travelTrip.BoeID = boeID;
            travelTrip.GroupID = 2;
            travelTrip.NumOfDays = 2;
            travelTrip.NumOfIntervals = 0;
            travelTrip.NumOfOccurences = 0;
            travelTrip.NumOfPeople = 3;
            travelTrip.NumOfTrips = 2;
            travelTrip.PerfOrgID = this.Perforg.Id;
            travelTrip.Segment = IES.Common.SegmentType.SSC;
            travelTrip.TripDate = DateTime.Now;
            travelTrip.Updateable = UpdateType.Upsert;
            travelTrip.UpdateDate = DateTime.Now;

            string guid = Guid.NewGuid().ToString();

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = guid;
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";
            travel.TravelTrips = new Collection<TravelTripType> { travelTrip };

            int beforeSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            Dictionary<int, int> ids = sut.SaveTravels(new Collection<TravelDTO> { travel });
            int afterSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            travel = this._travelDL.GetById(ids[-1]);
            Assert.IsTrue(afterSaveTravel == beforeSaveTravel + 1, "save didn't work");

            // edit a travel
            TravelDTO travelToEdit = sut.GetByBoeIds(new Collection<int>() { boeID }).First();
            travelToEdit.Updateable = UpdateType.Upsert;
            travelToEdit.TaskTitle = "MOCKTITLEEDIT";

            sut.SaveTravels(new Collection<TravelDTO> { travelToEdit });

            TravelDTO assertTravel = sut.GetById(travelToEdit.Id);
            Assert.IsTrue(assertTravel.TaskTitle == "MOCKTITLEEDIT", "edit didn't work");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_DeleteTravel()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            TravelTripType travelTrip = new TravelTripType();
            travelTrip.TravelTripID = -1;
            travelTrip.SystemTripID = this.SystemTrip.TripID;
            travelTrip.BoeID = boeID;
            travelTrip.GroupID = 2;
            travelTrip.NumOfDays = 2;
            travelTrip.NumOfIntervals = 0;
            travelTrip.NumOfOccurences = 0;
            travelTrip.NumOfPeople = 3;
            travelTrip.NumOfTrips = 2;
            travelTrip.PerfOrgID = this.Perforg.Id;
            travelTrip.Segment = IES.Common.SegmentType.SSC;
            travelTrip.TripDate = DateTime.Now;
            travelTrip.Updateable = UpdateType.Upsert;
            travelTrip.UpdateDate = DateTime.Now;

            string guid = Guid.NewGuid().ToString();

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = guid;
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";
            travel.TravelTrips = new Collection<TravelTripType> { travelTrip };

            Dictionary<int, int> newIds = sut.SaveTravels(new Collection<TravelDTO> { travel });
            travel = sut.GetById(newIds[-1]);

            Assert.IsNotNull(travel, "travel save did not work");

            // edit a travel
            travel.Updateable = UpdateType.Deleted;
            foreach (TravelTripType travetrip in travel.TravelTrips)
            {
                travetrip.Updateable = UpdateType.Deleted;
            }
            travel.TaskTitle = "MOCKTITLEEDIT";

            int beforeSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            sut.SaveTravels(new Collection<TravelDTO> { travel });
            int afterSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            Assert.IsTrue(afterSaveTravel == beforeSaveTravel - 1, "delete didn't work");

            this.ResetTestData();
        }

        /// <summary>
        /// Test Delete All Travel Task elements
        /// </summary>
        [TestMethod]
        public void L_DeleteAllTravel()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            TravelTripType travelTrip = new TravelTripType();
            travelTrip.TravelTripID = -1;
            travelTrip.SystemTripID = this.SystemTrip.TripID;
            travelTrip.BoeID = boeID;
            travelTrip.GroupID = 2;
            travelTrip.NumOfDays = 2;
            travelTrip.NumOfIntervals = 0;
            travelTrip.NumOfOccurences = 0;
            travelTrip.NumOfPeople = 3;
            travelTrip.NumOfTrips = 2;
            travelTrip.PerfOrgID = this.Perforg.Id;
            travelTrip.Segment = IES.Common.SegmentType.SSC;
            travelTrip.TripDate = DateTime.Now;
            travelTrip.Updateable = UpdateType.Upsert;
            travelTrip.UpdateDate = DateTime.Now;

            string guid = Guid.NewGuid().ToString();

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = guid;
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";
            travel.TravelTrips = new Collection<TravelTripType> { travelTrip };

            Dictionary<int, int> newIds = sut.SaveTravels(new Collection<TravelDTO> { travel });
            travel = sut.GetById(newIds[-1]);

            Assert.IsNotNull(travel, "travel save did not work");

            sut.DeleteAllTravelTripTaskElements(boeID);
            int afterDeleteTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;

            Assert.AreEqual(0, afterDeleteTravel, "delete all travels didn't work");

            this.ResetTestData();
        }
        
        /// <summary>
        /// Tests that RTE loading works as expected
        /// </summary>
        [TestMethod]
        public void TestingRteLoadChanges()
        {
            var sut = new TravelDTODataLoader(new TravelTripTaskElementCustomFieldValueXREFLoader(), new TravelTripCustomFieldValueXREFLoader());

            int id = -1;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                id = gbe.TravelTripTaskElements.First(x => x.TravelTaskDescription.Length > 0).TravelTripTaskElementID;
            }

            // A first test:
            // Check the RTE data being correctly loaded/not loaded..
            TravelDTO nonRteLoadedBoe = sut.GetByIds(new List<int>() { id }).First();

            // make sure RTE data was not loaded by default
            Assert.IsFalse(nonRteLoadedBoe.WasDescriptionSet);

            // make sure Description is handled correctly
            nonRteLoadedBoe.Description = "blah blah";
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);

            // load RTE data, make sure it loads correctly, leaving description alone
            sut.LoadRTEFields(new List<TravelDTO>() { nonRteLoadedBoe });
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);
            Assert.IsTrue(nonRteLoadedBoe.Description == "blah blah");


            // A second test:
            // do a manual, non RTE load first, then load RTE data after, then do a full RTE load, and compare the two, to make sure it's all the same
            nonRteLoadedBoe = sut.GetByIds(new List<int>() { id }).First();
            Assert.IsFalse(nonRteLoadedBoe.WasDescriptionSet);
            sut.LoadRTEFields(new List<TravelDTO>() { nonRteLoadedBoe });
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);

            TravelDTO rteLoadedBoe = sut.GetByIds(new List<int>() { id }, true).First();
            Assert.IsTrue(rteLoadedBoe.WasDescriptionSet);

            this.VerifyTravelDtos(nonRteLoadedBoe, rteLoadedBoe);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are left here if we need to do full BOE checks in the future..")]
        private void VerifyTravelCollections(ICollection<TravelDTO> travel1, ICollection<TravelDTO> travel2)
        {
            Assert.AreEqual(travel1.Count, travel2.Count);
            for (int i = 0; i < travel1.Count; i++)
            { 
                this.VerifyTravelDtos(travel1.ElementAt(i), travel2.ElementAt(i));
            }
        }

        private void VerifyTravelDtos(TravelDTO t1, TravelDTO t2)
        {
            Assert.AreEqual(t1.BoeID, t2.BoeID);
            Assert.AreEqual(t1.BOETaskElementOrder, t2.BOETaskElementOrder);
            Assert.AreEqual(t1.EndDate, t2.EndDate);
            Assert.AreEqual(t1.Id, t2.Id);
            Assert.AreEqual(t1.StartDate, t2.StartDate);
            Assert.AreEqual(t1.TaskID, t2.TaskID);
            Assert.AreEqual(t1.TaskTitle, t2.TaskTitle);
            Assert.AreEqual(t1.UpdateDate, t2.UpdateDate);
            Assert.AreEqual(t1.WasDescriptionSet, t2.WasDescriptionSet);

            Assert.AreEqual(t1.CustomFieldValueContainers.Count, t2.CustomFieldValueContainers.Count);
            for (int i = 0; i < t1.CustomFieldValueContainers.Count; i++)
            {
                Assert.AreEqual(t1.CustomFieldValueContainers.ElementAt(i).ContainerID, t2.CustomFieldValueContainers.ElementAt(i).ContainerID);
                Assert.AreEqual(t1.CustomFieldValueContainers.ElementAt(i).CustomFieldValueID, t2.CustomFieldValueContainers.ElementAt(i).CustomFieldValueID);
                Assert.AreEqual(t1.CustomFieldValueContainers.ElementAt(i).UpdateDate, t2.CustomFieldValueContainers.ElementAt(i).UpdateDate);
            }

            if (t1.WasDescriptionSet && t2.WasDescriptionSet)
            {
                Assert.AreEqual(t1.Description, t2.Description);
            }

            Assert.AreEqual(t1.TravelTrips.Count, t2.TravelTrips.Count);
            for (int j = 0; j < t1.TravelTrips.Count; j++)
            {
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).BoeID, t2.TravelTrips.ElementAt(j).BoeID);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).GroupID, t2.TravelTrips.ElementAt(j).GroupID);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).Id, t2.TravelTrips.ElementAt(j).Id);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).LockedDate, t2.TravelTrips.ElementAt(j).LockedDate);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).NumOfDays, t2.TravelTrips.ElementAt(j).NumOfDays);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).NumOfIntervals, t2.TravelTrips.ElementAt(j).NumOfIntervals);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).NumOfOccurences, t2.TravelTrips.ElementAt(j).NumOfOccurences);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).NumOfPeople, t2.TravelTrips.ElementAt(j).NumOfPeople);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).NumOfTrips, t2.TravelTrips.ElementAt(j).NumOfTrips);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).OriginatingTripID, t2.TravelTrips.ElementAt(j).OriginatingTripID);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).PerfOrgID, t2.TravelTrips.ElementAt(j).PerfOrgID);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).Purpose, t2.TravelTrips.ElementAt(j).Purpose);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).Segment, t2.TravelTrips.ElementAt(j).Segment);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).SystemTripID, t2.TravelTrips.ElementAt(j).SystemTripID);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).TravelTripID, t2.TravelTrips.ElementAt(j).TravelTripID);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).TripDate, t2.TravelTrips.ElementAt(j).TripDate);
                Assert.AreEqual(t1.TravelTrips.ElementAt(j).UpdateDate, t2.TravelTrips.ElementAt(j).UpdateDate);

                Assert.AreEqual(t1.TravelTrips.ElementAt(j).CustomFieldValueContainers.Count, t2.TravelTrips.ElementAt(j).CustomFieldValueContainers.Count);
                for (int i = 0; i < t1.CustomFieldValueContainers.Count; i++)
                {
                    Assert.AreEqual(t1.TravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).ContainerID, 
                                    t2.TravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).ContainerID);
                    Assert.AreEqual(t1.TravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).CustomFieldValueID, 
                                    t2.TravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).CustomFieldValueID);
                    Assert.AreEqual(t1.TravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).UpdateDate, 
                                    t2.TravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).UpdateDate);
                }
            }
        }
    }
}