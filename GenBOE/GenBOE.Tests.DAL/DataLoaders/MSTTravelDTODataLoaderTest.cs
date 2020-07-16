// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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
    public class MSTTravelDTODataLoaderTest : MOQLoaderObject
    {
        /// <summary>
        /// Create a loader for testing
        /// </summary>
        /// <returns>Test loader</returns>
        private MSTTravelDTODataLoader CreateTestLoader()
        {
            var _MSTTravelTripTaskElementCustomFieldValue = new Mock<ITravelTripTaskElementCustomFieldValueXREFLoader>();
            var _MSTTravelTripCustomFieldValue = new Mock<ITravelTripCustomFieldValueXREFLoader>();

            return new MSTTravelDTODataLoader(_MSTTravelTripTaskElementCustomFieldValue.Object, _MSTTravelTripCustomFieldValue.Object);
        }

        [TestMethod]
        public void L_GetMSTTravelCollectionByBoeIds()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            MSTTravelTripType MSTTravelTrip = new MSTTravelTripType();
            MSTTravelTrip.Id = -1;
            MSTTravelTrip.BoeID = boeID;
            MSTTravelTrip.GroupID = 2;
            MSTTravelTrip.ModeID = MSTTravelMode.ZoneNoAirfare;
            MSTTravelTrip.NumOfDays = 2;
            MSTTravelTrip.NumOfPeople = 3;
            MSTTravelTrip.PerfOrgID = this.Perforg.Id;
            MSTTravelTrip.Segment = IES.Common.SegmentType.SSC;
            MSTTravelTrip.TripDate = DateTime.Now;
            MSTTravelTrip.Updateable = UpdateType.Upsert;
            MSTTravelTrip.UpdateDate = DateTime.Now;

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = "mockTravelTitle";
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";
            travel.MSTTravelTrips = new Collection<MSTTravelTripType> { MSTTravelTrip };

            int beforeSaveTravel = sut.GetByBoeIds(new Collection<int> { boeID }).Count;
            sut.SaveTravels(new Collection<TravelDTO> { travel });
            int afterSaveTravel = sut.GetByBoeIds(new Collection<int> { boeID }).Count;
            Assert.IsTrue(afterSaveTravel == beforeSaveTravel + 1, "save didn't work");

            ICollection<TravelDTO> travelDtos = sut.GetByBoeIds(new Collection<int> { boeID });

            Assert.IsTrue(travelDtos.Count == 1, "no travel elements");
            Assert.IsTrue(travelDtos.First().MSTTravelTrips.Count == 1, "no travel trips");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetMSTTravelIDsByWorkspaceID()
        {
            var sut = this.CreateTestLoader();

            // save one travel so there is always at least one in the db
            MSTTravelTripType MSTTravelTrip1 = new MSTTravelTripType();
            MSTTravelTrip1.Id = -1;
            MSTTravelTrip1.BoeID = this.Boe1.Id;
            MSTTravelTrip1.ModeID = MSTTravelMode.ZoneNoAirfare;
            MSTTravelTrip1.GroupID = 2;
            MSTTravelTrip1.NumOfDays = 2;
            MSTTravelTrip1.NumOfPeople = 3;
            MSTTravelTrip1.PerfOrgID = this.Perforg.Id;
            MSTTravelTrip1.Segment = IES.Common.SegmentType.SSC;
            MSTTravelTrip1.TripDate = DateTime.Now;
            MSTTravelTrip1.Updateable = UpdateType.Upsert;
            MSTTravelTrip1.UpdateDate = DateTime.Now;

            MSTTravelTripType MSTTravelTrip2 = new MSTTravelTripType();
            MSTTravelTrip2.ModeID = MSTTravelMode.ZoneNoAirfare;
            MSTTravelTrip2.Id = -2;
            MSTTravelTrip2.BoeID = this.Boe2.Id;
            MSTTravelTrip2.GroupID = 2;
            MSTTravelTrip2.NumOfDays = 2;
            MSTTravelTrip2.NumOfPeople = 3;
            MSTTravelTrip2.PerfOrgID = this.Perforg.Id;
            MSTTravelTrip2.Segment = IES.Common.SegmentType.SSC;
            MSTTravelTrip2.TripDate = DateTime.Now;
            MSTTravelTrip2.Updateable = UpdateType.Upsert;
            MSTTravelTrip2.UpdateDate = DateTime.Now;

            TravelDTO travel1 = new TravelDTO();
            travel1.Id = -1;
            travel1.Updateable = UpdateType.Upsert;
            travel1.UpdateDate = DateTime.Now;
            travel1.TaskID = "ETA";
            travel1.TaskTitle = "mockTravelTitle";
            travel1.BoeID = this.Boe1.Id;
            travel1.Description = "mock travel desc";
            travel1.MSTTravelTrips = new Collection<MSTTravelTripType> { MSTTravelTrip1 };

            TravelDTO travel2 = new TravelDTO();
            travel2.Id = -2;
            travel2.Updateable = UpdateType.Upsert;
            travel2.UpdateDate = DateTime.Now;
            travel2.TaskID = "ETA";
            travel2.TaskTitle = "mockTravelTitle";
            travel2.BoeID = this.Boe2.Id;
            travel2.Description = "mock travel desc";
            travel2.MSTTravelTrips = new Collection<MSTTravelTripType> { MSTTravelTrip2 };

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
        public void L_GetMSTTravelDataByTravelID()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            MSTTravelTripType MSTTravelTrip = new MSTTravelTripType();
            MSTTravelTrip.Id = -1;
            MSTTravelTrip.BoeID = boeID;
            MSTTravelTrip.ModeID = MSTTravelMode.ZoneNoAirfare;
            MSTTravelTrip.GroupID = 2;
            MSTTravelTrip.NumOfDays = 2;
            MSTTravelTrip.NumOfPeople = 3;
            MSTTravelTrip.PerfOrgID = this.Perforg.Id;
            MSTTravelTrip.Segment = IES.Common.SegmentType.SSC;
            MSTTravelTrip.TripDate = DateTime.Now;
            MSTTravelTrip.Updateable = UpdateType.Upsert;
            MSTTravelTrip.UpdateDate = DateTime.Now;

            string guid = Guid.NewGuid().ToString();

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = guid;
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";
            travel.MSTTravelTrips = new Collection<MSTTravelTripType> { MSTTravelTrip };

            int beforeSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            Dictionary<int, int> ids = sut.SaveTravels(new Collection<TravelDTO> { travel });
            int afterSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            travel = this._travelDL.GetById(ids[-1]);
            Assert.IsTrue(afterSaveTravel == beforeSaveTravel + 1, "save didn't work");

            TravelDTO returnedTravel = sut.GetById(travel.Id);

            Assert.IsTrue(returnedTravel.Id > 0, "travel id is valid");
            Assert.IsTrue(returnedTravel.TaskTitle == guid, "task title didn't match");
            Assert.IsTrue(returnedTravel.MSTTravelTrips.Count > 0, "no travel trips found");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetMSTByIds()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            MSTTravelTripType MSTTravelTrip = new MSTTravelTripType();
            MSTTravelTrip.Id = -1;
            MSTTravelTrip.ModeID = MSTTravelMode.ZoneNoAirfare;
            MSTTravelTrip.BoeID = boeID;
            MSTTravelTrip.GroupID = 2;

            MSTTravelTrip.NumOfDays = 2;
            MSTTravelTrip.NumOfPeople = 3;
            MSTTravelTrip.PerfOrgID = this.Perforg.Id;
            MSTTravelTrip.Segment = IES.Common.SegmentType.SSC;
            MSTTravelTrip.TripDate = DateTime.Now;
            MSTTravelTrip.Updateable = UpdateType.Upsert;
            MSTTravelTrip.UpdateDate = DateTime.Now;

            string guid = Guid.NewGuid().ToString();

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = guid;
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";
            travel.MSTTravelTrips = new Collection<MSTTravelTripType> { MSTTravelTrip };

            int beforeSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            Dictionary<int, int> ids = sut.SaveTravels(new Collection<TravelDTO> { travel });
            int afterSaveTravel = sut.GetByBoeIds(new Collection<int>() { boeID }).Count;
            travel = this._travelDL.GetById(ids[-1]);
            Assert.IsTrue(afterSaveTravel == beforeSaveTravel + 1, "save didn't work");

            ICollection<TravelDTO> returnedTravel = sut.GetByIds(new Collection<int> { travel.Id });

            Assert.IsTrue(returnedTravel.Any());
            Assert.IsTrue(returnedTravel.First().Id > 0, "travel id is valid");
            Assert.IsTrue(returnedTravel.First().TaskTitle == guid, "task title didn't match");
            Assert.IsTrue(returnedTravel.First().MSTTravelTrips.Count > 0, "no travel trips found");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_SaveMSTTravel()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            MSTTravelTripType MSTTravelTrip = new MSTTravelTripType();
            MSTTravelTrip.Id = -1;
            MSTTravelTrip.BoeID = boeID;
            MSTTravelTrip.ModeID = MSTTravelMode.ZoneNoAirfare;
            MSTTravelTrip.GroupID = 2;
            MSTTravelTrip.NumOfDays = 2;
            MSTTravelTrip.NumOfPeople = 3;
            MSTTravelTrip.EstimateDate = DateTime.Now;
            MSTTravelTrip.PerfOrgID = this.Perforg.Id;
            MSTTravelTrip.Segment = IES.Common.SegmentType.SSC;
            MSTTravelTrip.TripDate = DateTime.Now;
            MSTTravelTrip.Updateable = UpdateType.Upsert;
            MSTTravelTrip.UpdateDate = DateTime.Now;

            string guid = Guid.NewGuid().ToString();

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = guid;
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";

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
        public void L_DeleteMSTTravel()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            MSTTravelTripType MSTTravelTrip = new MSTTravelTripType();
            MSTTravelTrip.Id = -1;
            MSTTravelTrip.ModeID = MSTTravelMode.ZoneNoAirfare;
            MSTTravelTrip.BoeID = boeID;
            MSTTravelTrip.GroupID = 2;
            MSTTravelTrip.NumOfDays = 2;
            MSTTravelTrip.NumOfPeople = 3;
            MSTTravelTrip.PerfOrgID = this.Perforg.Id;
            MSTTravelTrip.Segment = IES.Common.SegmentType.SSC;

            MSTTravelTrip.TripDate = DateTime.Now;
            MSTTravelTrip.Updateable = UpdateType.Upsert;
            MSTTravelTrip.UpdateDate = DateTime.Now;

            string guid = Guid.NewGuid().ToString();

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = guid;
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";
            travel.MSTTravelTrips.Add(MSTTravelTrip);
            Dictionary<int, int> newIds = sut.SaveTravels(new Collection<TravelDTO> { travel });
            travel = sut.GetById(newIds[-1]);

            Assert.IsNotNull(travel, "travel save did not work");

            // edit a travel
            travel.Updateable = UpdateType.Deleted;
            foreach (MSTTravelTripType travetrip in travel.MSTTravelTrips)
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
        /// Test Delete All MST Travel Task Elements
        /// </summary>
        [TestMethod]
        public void L_DeleteAllMSTTravel()
        {
            var sut = this.CreateTestLoader();
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            MSTTravelTripType MSTTravelTrip = new MSTTravelTripType();
            MSTTravelTrip.Id = -1;
            MSTTravelTrip.ModeID = MSTTravelMode.ZoneNoAirfare;
            MSTTravelTrip.BoeID = boeID;
            MSTTravelTrip.GroupID = 2;
            MSTTravelTrip.NumOfDays = 2;
            MSTTravelTrip.NumOfPeople = 3;
            MSTTravelTrip.PerfOrgID = this.Perforg.Id;
            MSTTravelTrip.Segment = IES.Common.SegmentType.SSC;

            MSTTravelTrip.TripDate = DateTime.Now;
            MSTTravelTrip.Updateable = UpdateType.Upsert;
            MSTTravelTrip.UpdateDate = DateTime.Now;

            string guid = Guid.NewGuid().ToString();

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = guid;
            travel.BoeID = boeID;
            travel.Description = "mock travel desc";
            travel.MSTTravelTrips.Add(MSTTravelTrip);
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
        public void TestingMSTRteLoadChanges()
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
                Assert.AreEqual(t1.CustomFieldValueContainers.ElementAt(i).IsOpenEnded, t2.CustomFieldValueContainers.ElementAt(i).IsOpenEnded);
                Assert.AreEqual(t1.CustomFieldValueContainers.ElementAt(i).OpenEndedValue, t2.CustomFieldValueContainers.ElementAt(i).OpenEndedValue);
                Assert.AreEqual(t1.CustomFieldValueContainers.ElementAt(i).CustomFieldID, t2.CustomFieldValueContainers.ElementAt(i).CustomFieldID);
            }

            if (t1.WasDescriptionSet && t2.WasDescriptionSet)
            {
                Assert.AreEqual(t1.Description, t2.Description);
            }

            Assert.AreEqual(t1.MSTTravelTrips.Count, t2.MSTTravelTrips.Count);
            for (int j = 0; j < t1.MSTTravelTrips.Count; j++)
            {
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).BoeID, t2.MSTTravelTrips.ElementAt(j).BoeID);
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).GroupID, t2.MSTTravelTrips.ElementAt(j).GroupID);
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).Id, t2.MSTTravelTrips.ElementAt(j).Id);
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).NumOfDays, t2.MSTTravelTrips.ElementAt(j).NumOfDays);
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).NumOfPeople, t2.MSTTravelTrips.ElementAt(j).NumOfPeople);
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).PerfOrgID, t2.MSTTravelTrips.ElementAt(j).PerfOrgID);
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).Purpose, t2.MSTTravelTrips.ElementAt(j).Purpose);
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).Segment, t2.MSTTravelTrips.ElementAt(j).Segment);
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).Id, t2.MSTTravelTrips.ElementAt(j).Id);
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).TripDate, t2.MSTTravelTrips.ElementAt(j).TripDate);
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).UpdateDate, t2.MSTTravelTrips.ElementAt(j).UpdateDate);
                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).ModeID, t2.MSTTravelTrips.ElementAt(j).ModeID);

                Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.Count, t2.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.Count);
                for (int i = 0; i < t1.CustomFieldValueContainers.Count; i++)
                {
                    Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).ContainerID,
                                    t2.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).ContainerID);
                    Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).CustomFieldValueID,
                                    t2.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).CustomFieldValueID);
                    Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).UpdateDate,
                                    t2.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).UpdateDate);
                    Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).IsOpenEnded,
                                    t2.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).IsOpenEnded);
                    Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).OpenEndedValue,
                                    t2.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).OpenEndedValue);
                    Assert.AreEqual(t1.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).CustomFieldID,
                                    t2.MSTTravelTrips.ElementAt(j).CustomFieldValueContainers.ElementAt(i).CustomFieldID);
                }
            }
        }
    }
}