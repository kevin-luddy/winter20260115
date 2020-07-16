// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.NewValidation;
    using IES.Common;
    using IES.Common.classes;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common.Exceptions;

    [TestClass]
    public class BOEZoneTravelControllerLogicTest
    {
        private Mock<IFullObjectFactory> Factory;
        private Mock<IBOELaborControllerLogic> boeLaborControllerLogic;
        private Mock<ITravelDTODataLoader> travelDTODataLoader;
        private Mock<IMSTZoneTravelOriginDTODataLoader> mstZoneTravelOriginDTODataLoader;
        private Mock<IMSTZoneTravelDestinationDTODataLoader> mstZoneTravelDestinationDTODataLoader;
        private Mock<IMSTZoneTravelResourceDTODataLoader> mstZoneTravelResourceDTODataLoader;
        private Mock<IPerformingOrgDTODataLoader> perfOrgLoader;
        private Mock<IResourceDTODataLoader> resourceDTODataLoader;
        private Mock<IGenBOEControllerLogic> genBOEControllerLogic;
        private Mock<IMSTZoneTravelValidator> mstZoneTravelValidator;
        private Mock<RMSZoneTravelRatesFeesDataLoader> zoneTravelRatesFeesLoader;
        private Mock<IRetriever> retriever;
        private Mock<ICommonDataMapper> commonDataMapper;
        private Mock<IPermissionsDTODataLoader> permissionsDTODataLoader;

        private List<WorkspaceRMSEscalationRatesDTO> escalationRates = new List<WorkspaceRMSEscalationRatesDTO>
        {
                new WorkspaceRMSEscalationRatesDTO { Year = DateTime.Today.Year - 1, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1 },
                new WorkspaceRMSEscalationRatesDTO { Year = DateTime.Today.Year, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1 },
                new WorkspaceRMSEscalationRatesDTO { Year = DateTime.Today.Year + 1, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1 }
        };

        /// <summary>
        /// Set up
        /// </summary>
        /// <returns>sut</returns>
        public BOEZoneTravelControllerLogic CreateSut()
        {
            this.Factory = new Mock<IFullObjectFactory>();
            this.boeLaborControllerLogic = new Mock<IBOELaborControllerLogic>();
            this.travelDTODataLoader = new Mock<ITravelDTODataLoader>();
            this.mstZoneTravelOriginDTODataLoader = new Mock<IMSTZoneTravelOriginDTODataLoader>();
            this.resourceDTODataLoader = new Mock<IResourceDTODataLoader>();
            this.mstZoneTravelDestinationDTODataLoader = new Mock<IMSTZoneTravelDestinationDTODataLoader>();
            this.mstZoneTravelResourceDTODataLoader = new Mock<IMSTZoneTravelResourceDTODataLoader>();
            this.perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            this.genBOEControllerLogic = new Mock<IGenBOEControllerLogic>();
            this.mstZoneTravelValidator = new Mock<IMSTZoneTravelValidator>();
            this.zoneTravelRatesFeesLoader = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            this.resourceDTODataLoader = new Mock<IResourceDTODataLoader>();
            this.retriever = new Mock<IRetriever>();
            this.commonDataMapper = new Mock<ICommonDataMapper>();
            this.permissionsDTODataLoader = new Mock<IPermissionsDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsDTODataLoader.Object);

            this.genBOEControllerLogic.Setup(x => x.ScrubRichTextPropertiesForSave(It.IsAny<object>())).Returns(new List<ValidationMessage>());
            this.mstZoneTravelValidator.Setup(x => x.ValidateTravelTaskDetails(It.IsAny<TravelDTO>(), It.IsAny<int>())).Returns(new Collection<ValidationMessage>());
            this.mstZoneTravelValidator.Setup(x => x.ValidateTravelTrips(It.IsAny<ICollection<MSTTravelTripType>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<ICollection<int>>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(new Collection<ValidationMessage>());
            this.mstZoneTravelValidator.Setup(x => x.ValidateTripDateForMultipleOccurrences(It.IsAny<Collection<MSTTravelTripType>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>())).Returns(new Collection<ValidationMessage>());

            this.zoneTravelRatesFeesLoader.Setup(x => x.getAllFeesAndCostsByWorkspace(It.IsAny<int>())).Returns(new List<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>
            {
                new WorkspaceRMSTravelNonzoneFeesAndCostsDTO { ModeID = (int)MSTTravelMode.None, MiscOther = 0, TravelAgencyFee = 0 },
                new WorkspaceRMSTravelNonzoneFeesAndCostsDTO { ModeID = (int)MSTTravelMode.NonZoneDomestic, MiscOther = 0, TravelAgencyFee = 0 },
                new WorkspaceRMSTravelNonzoneFeesAndCostsDTO { ModeID = (int)MSTTravelMode.NonZoneInternational, MiscOther = 0, TravelAgencyFee = 0 },
                new WorkspaceRMSTravelNonzoneFeesAndCostsDTO { ModeID = (int)MSTTravelMode.ZoneAirfare, MiscOther = 0, TravelAgencyFee = 0 },
                new WorkspaceRMSTravelNonzoneFeesAndCostsDTO { ModeID = (int)MSTTravelMode.ZoneNoAirfare, MiscOther = 0, TravelAgencyFee = 0 }
            });

            return new BOEZoneTravelControllerLogic(this.Factory.Object, this.boeLaborControllerLogic.Object, this.travelDTODataLoader.Object, this.mstZoneTravelOriginDTODataLoader.Object,
                this.mstZoneTravelDestinationDTODataLoader.Object, this.mstZoneTravelResourceDTODataLoader.Object, this.perfOrgLoader.Object, this.resourceDTODataLoader.Object,  this.genBOEControllerLogic.Object, this.mstZoneTravelValidator.Object, this.zoneTravelRatesFeesLoader.Object);
        }

        /// <summary>
        /// Test the GetTravelGridModelViews() method
        /// </summary>
        [TestMethod]
        public void TestGetTravelGridModelViews()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();

            int workspaceId = 11;
            TravelDTO travel = new TravelDTO();
            MSTTravelTripType trip = new MSTTravelTripType();
            BOETravelGridModelView expectedMV = new BOETravelGridModelView();

            //set up trip costs (other fields not needed)
            trip.ModeID = MSTTravelMode.NonZoneDomestic;
            trip.NonZoneAirfareEstimate = 200;
            trip.NonZoneCarRentalTrans = 100;
            trip.NonZonePerDiemDaily = 50;
            trip.NumOfDays = 2;
            trip.NumOfPeople = 3;
            trip.NonZoneNumCars = 1;
            trip.TripDate = DateTime.Now;
            trip.EstimateDate = DateTime.Now;

            //set up needed fields in travel dto and expected MV
            travel.Id = expectedMV.TravelID = 1;
            travel.TaskID = expectedMV.TaskID = "1";
            travel.TaskTitle = expectedMV.TaskTitle = "TestTitle";
            travel.StartDate = DateTime.Today.AddMonths(-1);
            expectedMV.TaskStartDate = ((DateTime)travel.StartDate).ToString("MM/yyyy");
            travel.EndDate = DateTime.Today.AddMonths(1);
            expectedMV.TaskEndDate = ((DateTime)travel.EndDate).ToString("MM/yyyy");
            travel.BOETaskElementOrder = expectedMV.BOETaskElementOrder = 1;
            travel.MSTTravelTrips.Add(trip);

            sut.CalculateTripCostsForNonZone(new List<MSTTravelTripType>() { trip }, 0, workspaceId, escalationRates);

            expectedMV.TotalCost = travel.MSTTravelTrips.Sum(x => x.Cost).ToString("N2");

            this.retriever.Setup(x => x.GetTravelCollectionByBoeID(1, false)).Returns(new Collection<TravelDTO>() { travel });
            FullBoe boe = new FullBoe() { Id = 1 };
            this.Factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(boe);

            Collection<BOETravelGridModelView> returnedMVs = sut.GetTravelGridModelViews(boe.Id, 0, workspaceId, escalationRates);

            //assert only the one MV that was set up is returned
            Assert.IsTrue(returnedMVs.Count() == 1);

            //Get single MV for easier comparison
            BOETravelGridModelView actualMV = returnedMVs.First();

            //assert the result is the same as the expected
            Assert.AreEqual(expectedMV.TravelID, actualMV.TravelID);
            Assert.AreEqual(expectedMV.TaskID, actualMV.TaskID);
            Assert.AreEqual(expectedMV.TaskTitle, actualMV.TaskTitle);
            Assert.AreEqual(expectedMV.TaskStartDate, actualMV.TaskStartDate);
            Assert.AreEqual(expectedMV.TaskEndDate, actualMV.TaskEndDate);
            Assert.AreEqual(expectedMV.BOETaskElementOrder, actualMV.BOETaskElementOrder);
            Assert.AreEqual(expectedMV.TotalCost, actualMV.TotalCost);
        }

        /// <summary>
        /// Test the GetTravelElementDetailsModelView() method
        /// </summary>
        [TestMethod]
        public void TestGetTravelElementDetailsModelView()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();

            FullWorkspace ws = new FullWorkspace();
            DateTime startDate = DateTime.Today.AddMonths(-1);
            DateTime endDate = DateTime.Today.AddMonths(1);
            BoeDTO boe = new BoeDTO()
            {
                Id = 1,
                StartDate = startDate,
                EndDate = endDate
            };
            TravelDTO travel = new TravelDTO()
            {
                Id = 1,
                BoeID = boe.Id,
                TaskID = "1",
                TaskTitle = "Test Title",
                StartDate = startDate,
                EndDate = endDate,
                Description = "Test Desc"
            };
            BOETravelElementDetailsModelView expectedMV = new BOETravelElementDetailsModelView()
            {
                TravelID = travel.Id,
                BOEID = boe.Id,
                TaskID = travel.TaskID,
                TaskTitle = travel.TaskTitle,
                StartDate = startDate.ToString("MM/yyyy"),
                EndDate = endDate.ToString("MM/yyyy"),
                TravelTaskDescription = travel.Description
            };

            this.Factory.Setup(x => x.CreateTravel(travel.Id)).Returns(travel);

            BOETravelElementDetailsModelView actualMV = sut.GetTravelElementDetailsModelView(ws, boe, travel.Id);

            Assert.AreEqual(expectedMV.TravelID, actualMV.TravelID);
            Assert.AreEqual(expectedMV.BOEID, actualMV.BOEID);
            Assert.AreEqual(expectedMV.TaskID, actualMV.TaskID);
            Assert.AreEqual(expectedMV.TaskTitle, actualMV.TaskTitle);
            Assert.AreEqual(expectedMV.StartDate, expectedMV.StartDate);
            Assert.AreEqual(expectedMV.EndDate, actualMV.EndDate);
            Assert.AreEqual(expectedMV.TravelTaskDescription, actualMV.TravelTaskDescription);
        }

        /// <summary>
        /// Test that GetTravelElementDetailsModelView() throws a null exception for a null boe
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetTravelElementDetailsModelView_NullException()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();
            sut.GetTravelElementDetailsModelView(null, null, null);
        }

        /// <summary>
        /// Test the GetTravelTripsGridModelViews() method
        /// </summary>
        [TestMethod]
        public void TestGetTravelTripsGridModelViews()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();
            int workspaceId = 11;
            int resourceListId = 20; // set to bad val for right now, will pull no res in controllerlogic
            int boeID = 1;
            DateTime startDate = DateTime.Today.AddMonths(-1);
            DateTime endDate = DateTime.Today.AddMonths(1);
            FullWorkspace ws = new FullWorkspace() { Id = workspaceId, ResourceListID = resourceListId };

            TravelDTO travel = new TravelDTO()
            {
                Id = 1,
                BoeID = boeID,
                TaskID = "1",
                TaskTitle = "Test Title",
                StartDate = startDate,
                EndDate = endDate,
                Description = "Test Desc"
            };

            MSTTravelTripType zoneTrip = new MSTTravelTripType()
            {
                BoeID = boeID,
                Id = 1,
                ModeID = MSTTravelMode.ZoneAirfare, 
                GroupID = 1,
                Segment = SegmentType.RMS,
                PerfOrgID = 1,
                EstimateDate = DateTime.Today,
                TripDate = DateTime.Today,
                Purpose = "Zone Test Purpose",
                NumOfPeople = 1,
                NumOfDays = 2,
                ZoneOriginID = 1,
                ZoneDestinationID = 1,
                ZoneDestCity = "Test City",
                ZoneResourceID = 1
            };
            travel.MSTTravelTrips.Add(zoneTrip);

            MSTTravelTripType nonzoneTrip = new MSTTravelTripType()
            {
                BoeID = boeID,
                Id = 2,
                ModeID = MSTTravelMode.NonZoneDomestic,
                GroupID = 2,
                Segment = SegmentType.RMS,
                PerfOrgID = 1,
                EstimateDate = DateTime.Today,
                TripDate = DateTime.Today,
                Purpose = "Nonzone Test Purpose",
                NumOfPeople = 2,
                NumOfDays = 3,
                NonZoneFrom = "Test From Location",
                NonZoneTo = "Test TO Location",
                NonZoneAirfareEstimate = 200,
                NonZoneCarRentalTrans = 100,
                NonZonePerDiemDaily = 50,
                NonZoneNumCars = 1,
                NonZoneResourceID = -1
            };
            travel.MSTTravelTrips.Add(nonzoneTrip);

            PerformingOrgDTO perfOrg = new PerformingOrgDTO()
            {
                Id = 1,
                PerformingOrgName = "Test",
                PerformingOrgDesc = "Test Description"
            };


            ResourceDTO resource = new ResourceDTO()
            {
                Id = 1,
                ResourceName = "Test",
                ResourceDesc = "Test Resource"
            };


            MSTZoneTravelOriginDTO origin = new MSTZoneTravelOriginDTO()
            {
                OriginID = 1,
                Origin = "Test Origin",
                Site = "A"
            };

            MSTZoneTravelDestinationDTO destination = new MSTZoneTravelDestinationDTO()
            {
                DestinationID = 1,
                Destination = "Test Dest",
                Abbreviation = "TD",
                Zone = 2
            };

            BOEZoneTravelTripsGridModelView expectedZoneMV = new BOEZoneTravelTripsGridModelView()
            {
                TravelTripID = zoneTrip.Id,
                GroupID = zoneTrip.GroupID,
                ModeID = zoneTrip.ModeID,
                PerformingOrgID = zoneTrip.PerfOrgID,
                PerformingOrgName = perfOrg.PerformingOrgName,
                NonZoneResourceID = resource.Id,
                NonZoneResourceName = resource.ResourceName,
                DateOfEstimate = zoneTrip.EstimateDate,
                EstTripDate = zoneTrip.TripDate,
                Purpose = zoneTrip.Purpose,
                NumOfPeople = zoneTrip.NumOfPeople,
                NumOfDays = zoneTrip.NumOfDays,
                OriginID = zoneTrip.ZoneOriginID,
                OriginName = origin.Origin,
                DestinationCity = zoneTrip.ZoneDestCity,
                DestinationStateID = zoneTrip.ZoneDestinationID,
                DestinationStateName = destination.Destination,
                Zone = destination.Zone
            };

            BOEZoneTravelTripsGridModelView expectedNonzoneMV = new BOEZoneTravelTripsGridModelView()
            {
                TravelTripID = nonzoneTrip.Id,
                GroupID = nonzoneTrip.GroupID,
                ModeID = nonzoneTrip.ModeID,
                PerformingOrgID = nonzoneTrip.PerfOrgID,
                PerformingOrgName = perfOrg.PerformingOrgName,
                NonZoneResourceID = resource.Id,
                NonZoneResourceName = resource.ResourceName,
                DateOfEstimate = nonzoneTrip.EstimateDate,
                EstTripDate = nonzoneTrip.TripDate,
                Purpose = nonzoneTrip.Purpose,
                NumOfPeople = nonzoneTrip.NumOfPeople,
                NumOfDays = nonzoneTrip.NumOfDays,
                FromLocation = nonzoneTrip.NonZoneFrom,
                ToLocation = nonzoneTrip.NonZoneTo,
                NumOfCars = nonzoneTrip.NonZoneNumCars,
                AirfareEst = nonzoneTrip.NonZoneAirfareEstimate,
                CarRentalTrans = nonzoneTrip.NonZoneCarRentalTrans,
                PerDiemDaily = nonzoneTrip.NonZonePerDiemDaily
            };

            // calculate up the cost, for comparison.. This method will be tested elsewhere, so we can trust it in this location
            sut.CalculateTripCostsForNonZone(new List<BOEZoneTravelTripsGridModelView>() { expectedNonzoneMV }, 0, workspaceId, escalationRates);

            Collection<BOEZoneTravelTripsGridModelView> expectedMVs = new Collection<BOEZoneTravelTripsGridModelView>() { expectedZoneMV, expectedNonzoneMV };

            this.Factory.Setup(x => x.CreateTravel(travel.Id)).Returns(travel);
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new List<FullWbs>());
            this.perfOrgLoader.Setup(x => x.GetByIds(new Collection<int>() { perfOrg.Id })).Returns(new Collection<PerformingOrgDTO>() { perfOrg });
            this.mstZoneTravelOriginDTODataLoader.Setup(x => x.GetOriginByOriginID(origin.OriginID)).Returns(origin);
            this.mstZoneTravelDestinationDTODataLoader.Setup(x => x.GetDestinationByDestinationID(destination.DestinationID)).Returns(destination);
            this.resourceDTODataLoader.Setup(x => x.GetByListIdAndElementOfCost(ws.ResourceListID, ElementOfCostType.Travel)).Returns(new List<ResourceDTO>());

            Collection<BOEZoneTravelTripsGridModelView> actualMVs = sut.GetTravelTripsGridModelViews(ws, travel.Id, escalationRates);

            //assert only the two MVs that were set up are returned
            Assert.IsTrue(actualMVs.Count == 2);
            
            //iterate through both MV collections to compare
            for(int i = 0; i < actualMVs.Count; i++)
            {
                Assert.AreEqual(expectedMVs[i].TravelTripID, actualMVs[i].TravelTripID);
                Assert.AreEqual(expectedMVs[i].GroupID, actualMVs[i].GroupID);
                Assert.AreEqual(expectedMVs[i].ModeID, actualMVs[i].ModeID);
                Assert.AreEqual(expectedMVs[i].PerformingOrgID, actualMVs[i].PerformingOrgID);
                Assert.AreEqual(expectedMVs[i].PerformingOrgName, actualMVs[i].PerformingOrgName);
                Assert.AreEqual(expectedMVs[i].DateOfEstimate, actualMVs[i].DateOfEstimate);
                Assert.AreEqual(expectedMVs[i].EstTripDate, actualMVs[i].EstTripDate);
                Assert.AreEqual(expectedMVs[i].Purpose, actualMVs[i].Purpose);
                Assert.AreEqual(expectedMVs[i].NumOfPeople, actualMVs[i].NumOfPeople);
                Assert.AreEqual(expectedMVs[i].NumOfDays, actualMVs[i].NumOfDays);
                Assert.AreEqual(expectedMVs[i].OriginID, actualMVs[i].OriginID);
                Assert.AreEqual(expectedMVs[i].OriginName, actualMVs[i].OriginName);
                Assert.AreEqual(expectedMVs[i].DestinationCity, actualMVs[i].DestinationCity);
                Assert.AreEqual(expectedMVs[i].DestinationStateID, actualMVs[i].DestinationStateID);
                Assert.AreEqual(expectedMVs[i].DestinationStateName, actualMVs[i].DestinationStateName);
                Assert.AreEqual(expectedMVs[i].Zone, actualMVs[i].Zone);
                Assert.AreEqual(expectedMVs[i].FromLocation, actualMVs[i].FromLocation);
                Assert.AreEqual(expectedMVs[i].ToLocation, actualMVs[i].ToLocation);
                Assert.AreEqual(expectedMVs[i].NumOfCars, actualMVs[i].NumOfCars);
                Assert.AreEqual(expectedMVs[i].AirfareEst, actualMVs[i].AirfareEst);
                Assert.AreEqual(expectedMVs[i].CarRentalTrans, actualMVs[i].CarRentalTrans);
                Assert.AreEqual(expectedMVs[i].PerDiemDaily, actualMVs[i].PerDiemDaily);
                Assert.AreEqual(expectedMVs[i].Cost, actualMVs[i].Cost);
            }
        }

        /// <summary>
        /// Test that ValidateTravelTrip properly returns validation messages
        /// </summary>
        [TestMethod]
        public void TestValidateTravelTrip()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();

            Collection<BOEZoneTravelTripsGridModelView> tripsGridMVCollection = new Collection<BOEZoneTravelTripsGridModelView>();

            BOEZoneTravelTripsGridModelView tripsGridMV = new BOEZoneTravelTripsGridModelView()
            {
                TravelTripID = 1,
                GroupID = 1,
                ModeID = MSTTravelMode.ZoneNoAirfare,
                PerformingOrgID = 1,
                PerformingOrgName = "Test PerfOrg",
                DateOfEstimate = DateTime.Today,
                EstTripDate = DateTime.Today.AddMonths(1),
                Purpose = string.Empty,
                NumOfPeople = 2,
                NumOfDays = 3,
                OriginID = 1,
                OriginName = "Test Origin",
                DestinationCity = "Test City",
                DestinationStateID = 1,
                DestinationStateName = "Test State",
                Zone = 1
            };

            tripsGridMVCollection.Add(tripsGridMV);

            //create multiple occurrences
            BOEZoneTravelTripsGridModelView additionalOccurrence = new BOEZoneTravelTripsGridModelView();
            for (int i = 0; i < 3; i++)
            {
                additionalOccurrence = new BOEZoneTravelTripsGridModelView();
                additionalOccurrence.EstTripDate = tripsGridMV.EstTripDate.AddMonths(i + 1);
                tripsGridMVCollection.Add(additionalOccurrence);
            }

            this.mstZoneTravelValidator.Setup(x => x.ValidateTravelTrips(It.IsAny<ICollection<MSTTravelTripType>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<ICollection<int>>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(new Collection<ValidationMessage>() { new ValidationMessage() });
            this.mstZoneTravelValidator.Setup(x => x.ValidateTripDateForMultipleOccurrences(It.IsAny<Collection<MSTTravelTripType>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>())).Returns(new Collection<ValidationMessage>() { new ValidationMessage() });

            Collection<ValidationMessage> result = sut.ValidateTravelTrip(tripsGridMVCollection, 1, DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), new List<int>());

            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// Test that ArgumentNullException is thrown when trips is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestValidateTravelTrip_EX()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();

            sut.ValidateTravelTrip(null, 1, DateTime.Today, DateTime.Today, null);
        }

        /// <summary>
        /// Test the DeleteTravelTask() method
        /// </summary>
        [TestMethod]
        public void TestDeleteTravelTask()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();

            int boeID = 1;
            DateTime startDate = DateTime.Today.AddMonths(-1);
            DateTime endDate = DateTime.Today.AddMonths(1);

            TravelDTO travel = new TravelDTO()
            {
                Id = 1,
                BoeID = boeID,
                TaskID = "1",
                TaskTitle = "Test Title",
                StartDate = startDate,
                EndDate = endDate,
                Description = "Test Desc"
            };

            MSTTravelTripType zoneTrip = new MSTTravelTripType()
            {
                BoeID = boeID,
                Id = 1,
                ModeID = MSTTravelMode.ZoneAirfare,
                GroupID = 1,
                Segment = SegmentType.RMS,
                PerfOrgID = 1,
                TripDate = DateTime.Today,
                Purpose = "Zone Test Purpose",
                NumOfPeople = 1,
                NumOfDays = 2,
                ZoneOriginID = 1,
                ZoneDestinationID = 1,
                ZoneDestCity = "Test City",
                ZoneResourceID = 1
            };
            travel.MSTTravelTrips.Add(zoneTrip);

            MSTTravelTripType nonzoneTrip = new MSTTravelTripType()
            {
                BoeID = boeID,
                Id = 2,
                ModeID = MSTTravelMode.NonZoneDomestic,
                GroupID = 2,
                Segment = SegmentType.RMS,
                PerfOrgID = 1,
                NonZoneResourceID = -1,
                TripDate = DateTime.Today,
                Purpose = "Nonzone Test Purpose",
                NumOfPeople = 2,
                NumOfDays = 3,
                NonZoneFrom = "Test From Location",
                NonZoneTo = "Test TO Location",
                NonZoneAirfareEstimate = 200,
                NonZoneCarRentalTrans = 100,
                NonZonePerDiemDaily = 50,
                NonZoneNumCars = 1
            };
            travel.MSTTravelTrips.Add(nonzoneTrip);

            this.Factory.Setup(x => x.CreateTravel(travel.Id)).Returns(travel);
            this.travelDTODataLoader.Setup(x => x.SaveTravels(It.IsAny<ICollection<TravelDTO>>())).Verifiable();

            sut.DeleteTravelTask(boeID, travel.Id);

            this.travelDTODataLoader.Verify(x => x.SaveTravels(It.IsAny<ICollection<TravelDTO>>()), Times.Once());
        }

        /// <summary>
        /// Test the SaveTravelTask() method
        /// </summary>
        [TestMethod]
        public void TestSaveTravelTask()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();

            int boeID = 1;

            DateTime startDate = DateTime.Today.AddMonths(-1);
            DateTime endDate = DateTime.Today.AddMonths(1);

            TravelDTO travel = new TravelDTO()
            {
                Id = 1,
                BoeID = boeID,
                TaskID = "1",
                TaskTitle = "Test Title",
                StartDate = startDate,
                EndDate = endDate,
                Description = "Test Desc"
            };

            MSTTravelTripType zoneTrip = new MSTTravelTripType()
            {
                BoeID = boeID,
                Id = 1,
                ModeID = MSTTravelMode.ZoneAirfare,
                GroupID = 1,
                Segment = SegmentType.RMS,
                PerfOrgID = 1,
                EstimateDate = DateTime.Today.AddMonths(-1),
                TripDate = DateTime.Today,
                Purpose = "Zone Test Purpose",
                NumOfPeople = 1,
                NumOfDays = 2,
                ZoneOriginID = 1,
                ZoneDestinationID = 1,
                ZoneDestCity = "Test City",
                ZoneResourceID = 1
            };
            travel.MSTTravelTrips.Add(zoneTrip);

            MSTTravelTripType nonzoneTrip = new MSTTravelTripType()
            {
                BoeID = boeID,
                Id = 2,
                ModeID = MSTTravelMode.NonZoneDomestic,
                GroupID = 2,
                Segment = SegmentType.RMS,
                PerfOrgID = 1,
                NonZoneResourceID = -1,
                EstimateDate = DateTime.Today.AddMonths(-1),
                TripDate = DateTime.Today,
                Purpose = "Nonzone Test Purpose",
                NumOfPeople = 2,
                NumOfDays = 3,
                NonZoneFrom = "Test From Location",
                NonZoneTo = "Test TO Location",
                NonZoneAirfareEstimate = 200,
                NonZoneCarRentalTrans = 100,
                NonZonePerDiemDaily = 50,
                NonZoneNumCars = 1
            };
            travel.MSTTravelTrips.Add(nonzoneTrip);

            MSTZoneTravelResourceDTO resource = new MSTZoneTravelResourceDTO()
            {
                ResourceID = 1,
                Resource = "TestResource",
                Description = "TestResource Description",
                LookupValue = "TRA2",
                IsAirfare = true,
                Zone = 2
            };

            BOETravelElementDetailsModelView existingDetailsMV = new BOETravelElementDetailsModelView()
            {
                TravelID = travel.Id,
                BOEID = boeID,
                TaskID = travel.TaskID,
                TaskTitle = travel.TaskTitle,
                StartDate = startDate.ToString("MM/yyyy"),
                EndDate = endDate.ToString("MM/yyyy"),
                TravelTaskDescription = travel.Description
            };

            BOETravelElementDetailsModelView newDetailsMV = new BOETravelElementDetailsModelView()
            {
                TravelID = -1,
                BOEID = boeID,
                TaskID = "2",
                TaskTitle = "New Task Title",
                StartDate = startDate.ToString("MM/yyyy"),
                EndDate = endDate.ToString("MM/yyyy"),
                TravelTaskDescription = "New Task Description"
            };

            BOEZoneTravelTripsGridModelView ZoneTripsGridMV = new BOEZoneTravelTripsGridModelView()
            {
                TravelTripID = zoneTrip.Id,
                GroupID = zoneTrip.GroupID,
                ModeID = zoneTrip.ModeID,
                PerformingOrgID = zoneTrip.PerfOrgID,
                PerformingOrgName = "Test PerfOrg",
                DateOfEstimate = zoneTrip.EstimateDate,
                EstTripDate = zoneTrip.TripDate,
                Purpose = zoneTrip.Purpose,
                NumOfPeople = zoneTrip.NumOfPeople,
                NumOfDays = zoneTrip.NumOfDays,
                OriginID = zoneTrip.ZoneOriginID,
                OriginName = "Test Origin",
                DestinationCity = zoneTrip.ZoneDestCity,
                DestinationStateID = zoneTrip.ZoneDestinationID,
                DestinationStateName = "Test State",
                Zone = resource.Zone
            };

            BOEZoneTravelTripsGridModelView NonzoneTripsGridMV = new BOEZoneTravelTripsGridModelView()
            {
                TravelTripID = nonzoneTrip.Id,
                GroupID = nonzoneTrip.GroupID,
                ModeID = nonzoneTrip.ModeID,
                PerformingOrgID = nonzoneTrip.PerfOrgID,
                PerformingOrgName = "Test PerfOrg",
                DateOfEstimate = nonzoneTrip.EstimateDate,
                NonZoneResourceID = nonzoneTrip.NonZoneResourceID,
                NonZoneResourceName = "Test NZ Resource",
                EstTripDate = nonzoneTrip.TripDate,
                Purpose = nonzoneTrip.Purpose,
                NumOfPeople = nonzoneTrip.NumOfPeople,
                NumOfDays = nonzoneTrip.NumOfDays,
                FromLocation = nonzoneTrip.NonZoneFrom,
                ToLocation = nonzoneTrip.NonZoneTo,
                NumOfCars = nonzoneTrip.NonZoneNumCars,
                AirfareEst = (long)nonzoneTrip.NonZoneAirfareEstimate,
                CarRentalTrans = (long)nonzoneTrip.NonZoneCarRentalTrans,
                PerDiemDaily = (long)nonzoneTrip.NonZonePerDiemDaily,
                Cost = 350
            };

            this.Factory.Setup(x => x.CreateTravel(travel.Id)).Returns(travel);
            this.mstZoneTravelResourceDTODataLoader.Setup(x => x.GetResourceByOriginZoneAndMode((int)ZoneTripsGridMV.OriginID, ZoneTripsGridMV.Zone, true)).Returns(resource);
            this.travelDTODataLoader.Setup(x => x.SaveTravels(It.IsAny<ICollection<TravelDTO>>())).Verifiable();

            //test for saving an existing task
            sut.SaveTravelTask(boeID, existingDetailsMV, new Collection<BOEZoneTravelTripsGridModelView>() { ZoneTripsGridMV, NonzoneTripsGridMV }, new List<int>());
            this.travelDTODataLoader.Verify(x => x.SaveTravels(It.IsAny<ICollection<TravelDTO>>()), Times.Once());

            //test for saving a new task 
            //Times.Exactly(2) because this will be the second call to SaveTravels since it was called once already for saving an existing task
            sut.SaveTravelTask(boeID, newDetailsMV, new Collection<BOEZoneTravelTripsGridModelView>() { ZoneTripsGridMV, NonzoneTripsGridMV }, new List<int>());
            this.travelDTODataLoader.Verify(x => x.SaveTravels(It.IsAny<ICollection<TravelDTO>>()), Times.Exactly(2));
        }

        /// <summary>
        /// Test that SaveTravelTask() throws a null exception when the inDetailsMV param is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSaveTravelTask_NullException_DetailsMV()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();
            Collection<BOEZoneTravelTripsGridModelView> tripsCollection = new Collection<BOEZoneTravelTripsGridModelView>();
            sut.SaveTravelTask(1, null, tripsCollection, new List<int>());
        }

        /// <summary>
        /// Test that SaveTravelTask() throws a null exception when the inTravelTripsCollection param is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSaveTravelTask_NullException_TripsCollection()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();
            BOETravelElementDetailsModelView detailsMV = new BOETravelElementDetailsModelView();
            sut.SaveTravelTask(1, detailsMV, null, new List<int>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCalculateTripCostsForNonZone_Null()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();

            ICollection<MSTTravelTripType> input = null;

            sut.CalculateTripCostsForNonZone(input, 0, 0, escalationRates);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCalculateTripCostsForNonZone_Null2()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();

            ICollection<BOEZoneTravelTripsGridModelView> input = null;

            sut.CalculateTripCostsForNonZone(input, 0, 0, escalationRates);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCalculateTripCostsForNonZone_Null3()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();

            ICollection<BOEZoneTravelTripsGridModelView> input = new List<BOEZoneTravelTripsGridModelView>();

            sut.CalculateTripCostsForNonZone(input, 0, 0, null);
        }

        [TestMethod]
        public void TestCalculateTripCostsForNonZone_Basic()
        {
            BOEZoneTravelControllerLogic sut = CreateSut();

            int workspaceId = 11;
            List<MSTTravelTripType> input = new List<MSTTravelTripType>()
            {
                new MSTTravelTripType()
                {
                    NumOfDays = 1,
                    NumOfPeople = 1,
                    NonZoneAirfareEstimate = 300,
                    EstimateDate = DateTime.Now,
                    TripDate = DateTime.Now,
                    ModeID = MSTTravelMode.NonZoneDomestic
                },
                new MSTTravelTripType()
                {
                    NumOfDays = 1,
                    NumOfPeople = 1,
                    NonZoneAirfareEstimate = 300,
                    EstimateDate = DateTime.Now,
                    TripDate = DateTime.Now,
                    ModeID = MSTTravelMode.ZoneAirfare
                },
                new MSTTravelTripType()
                {
                    NumOfDays = 1,
                    NumOfPeople = 1,
                    NonZoneAirfareEstimate = 300,
                    EstimateDate = DateTime.Now,
                    TripDate = DateTime.Now,
                    ModeID = MSTTravelMode.ZoneNoAirfare
                },
                new MSTTravelTripType()
                {
                    NumOfDays = 1,
                    NumOfPeople = 2,
                    NonZoneAirfareEstimate = 400,
                    EstimateDate = DateTime.Now,
                    TripDate = DateTime.Now,
                    ModeID = MSTTravelMode.NonZoneInternational
                }
            };

            sut.CalculateTripCostsForNonZone(input, 0, workspaceId, escalationRates);

            Assert.AreEqual(300, input[0].Cost); //NonZoneDomestic
            Assert.AreEqual(0, input[1].Cost); //ZoneAirfare
            Assert.AreEqual(0, input[2].Cost); //ZoneNoAirfare
            Assert.AreEqual(800, input[3].Cost); //NonZoneInternational
        }
    }
}