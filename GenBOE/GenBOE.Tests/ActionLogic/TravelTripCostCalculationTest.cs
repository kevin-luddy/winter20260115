// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TravelTripCostCalculationTest : MOQObject
    {
        Mock<IRetriever> retriever = new Mock<IRetriever>();
        private Mock<IPermissionsDTODataLoader> _permissions = new Mock<IPermissionsDTODataLoader>();

        public void BL_CalculateTravelCost_SegmentLMSI()
        {
			Mock<ITripDTODataLoader> tripLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> MiscRateLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<IEscalationRatesDTOLoader> EscRateLoader = new Mock<IEscalationRatesDTOLoader>();
			Mock<IPerDiemDTODataLoader> PerDiemLoader = new Mock<IPerDiemDTODataLoader>();
			TravelTripCostCalculation sut = new TravelTripCostCalculation();

            WorkspaceDTO workspaceDto = new WorkspaceDTO();
            workspaceDto.Id = 1;

            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            FullWorkspace workspace = new FullWorkspace(workspaceDto);

            TripDTO trip = new TripDTO { TripID = 1, PerDiemID = 1, Fare = 1230.25m, MiscTravelRateID = 1, RentalCarRate = 32 };
            tripLoader.Setup(x => x.GetTripDTOByTripID(1, workspace)).Returns(trip);

            PerDiemDTO perDiem = new PerDiemDTO { Id = 1, HotelRate = 75,  MIERate = 20.5m };
            PerDiemLoader.Setup(x => x.GetPerDiemDTOByPerDiemID(perDiem.Id, workspace)).Returns(perDiem);
            MiscRateLoader.Setup(x => x.GetById(1, It.IsAny<WorkspaceDTO>())).Returns(new MiscTravelRateDTO { Id = 1, MiscTravelRate = 75 });

            EscalationRatesDTO escRate = new EscalationRatesDTO {EscalationRateID=1, LMSIEscalation=50, DevEscalation=100, Year=2011 };
            EscRateLoader.Setup(x => x.GetByWorkspace(workspace)).Returns(new Collection<EscalationRatesDTO> { escRate });

            TravelTripType travelTrip = new TravelTripType {TravelTripID=1, TripDate=Convert.ToDateTime("07/01/2011"), BoeID=this.Boe1.Id, GroupID=1, NumOfDays=2, NumOfIntervals=0, NumOfOccurences=0, NumOfPeople=4, NumOfTrips=2, PerfOrgID=this.Perforg.Id, Segment=SegmentType.LS, SystemTripID=1 };

            decimal cost = sut.CalculateTravelCost(travelTrip, workspace).CostTotal;
            Assert.IsTrue(cost > 0, "no cost");
            Assert.IsTrue(cost == 590835, "doesn't equal the amount i calculated");

        }

        [TestMethod]
        public void BL_CalculateTravelCost_SegmentNonLMSI()
        {
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
			TravelTripCostCalculation sut = new TravelTripCostCalculation();

            Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);
            
            WorkspaceDTO workspaceDto = new WorkspaceDTO() { Id = 1 };
            FullWorkspace workspace = new FullWorkspace(workspaceDto);

            TripDTO trip = new TripDTO { TripID = 1, PerDiemID = 1, Fare = 1230.25m, MiscTravelRateID = 1, RentalCarRate = 32 };
            PerDiemDTO perDiem = new PerDiemDTO { Id = 1, HotelRate = 75, MIERate = 20.5m };
            MiscTravelRateDTO travelRate = new MiscTravelRateDTO { Id = 1, MiscTravelRate = 75 };
            EscalationRatesDTO escRate = new EscalationRatesDTO { EscalationRateID = 1, LMSIEscalation = 50, DevEscalation = 100, Year = 2011 };
            TravelTripType travelTrip = new TravelTripType { TravelTripID = 1, TripDate = Convert.ToDateTime("07/01/2011"), BoeID = this.Boe1.Id, GroupID = 1, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.TS, SystemTripID = 1 };

            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO>() { new TravelDTO() { TravelTrips = new List<TravelTripType>() { travelTrip } } });
            this.retriever.Setup(x => x.GetTravelTripsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<TripDTO>() { trip });
            this.retriever.Setup(x => x.GetTravelTripPerDiemsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<PerDiemDTO>() { perDiem });
            this.retriever.Setup(x => x.GetTravelTripMiscTravelRatesForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<MiscTravelRateDTO>() { travelRate });
            this.retriever.Setup(x => x.GetEscalationRatesByWorkspace(workspace)).Returns(new List<EscalationRatesDTO>() { escRate });

            decimal cost = sut.CalculateTravelCost(travelTrip, workspace).CostTotal;
            Assert.IsTrue(cost > 0, "no cost");
            Assert.IsTrue(cost == 1170085, "doesn't equal the amount i calculated");

        }

        /// <summary>
        /// Tests the new MIE formula (story 8721) when the workspace state is in initialized, the trip date is beyond the set trip date, and the trip is a single day
        /// Since number of days is 1 the new formula shoudn't be used.
        /// </summary>
        [TestMethod]
        public void BL_CalculateTravelCost_UpdatedMIECalc_AfterSetTripDateWSStatusInitializedSingleDay()
        {
            //Activation and trip comparison dates set in app.config
            //<add key="MIECostUpdateActivationDate" value="01/01/2012"/>
            //<add key ="MIECostUpdateTripDate" value="07/01/2012"/>
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
			TravelTripCostCalculation sut = new TravelTripCostCalculation();

            WorkspaceDTO workspaceDto = new WorkspaceDTO();
            workspaceDto.Id = 1;

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            FullWorkspace workspace = new FullWorkspace(workspaceDto);

            TripDTO trip = new TripDTO { TripID = 1, PerDiemID = 1, Fare = 1230.25m, MiscTravelRateID = 1, RentalCarRate = 32 };
            PerDiemDTO perDiem = new PerDiemDTO { Id = 1, HotelRate = 75, MIERate = 20.5m };
            EscalationRatesDTO escRate = new EscalationRatesDTO { EscalationRateID = 1, LMSIEscalation = 50, DevEscalation = 100, Year = 2012 };
            TravelTripType travelTrip = new TravelTripType { TravelTripID = 1, TripDate = Convert.ToDateTime("08/01/2012"), BoeID = this.Boe1.Id, GroupID = 1, NumOfDays = 1, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.LS, SystemTripID = 1 };
            MiscTravelRateDTO travelRate = new MiscTravelRateDTO { Id = 1, MiscTravelRate = 75 };

            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO>() { new TravelDTO() { TravelTrips = new List<TravelTripType>() { travelTrip } } });
            this.retriever.Setup(x => x.GetTravelTripsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<TripDTO>() { trip });
            this.retriever.Setup(x => x.GetTravelTripPerDiemsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<PerDiemDTO>() { perDiem });
            this.retriever.Setup(x => x.GetTravelTripMiscTravelRatesForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<MiscTravelRateDTO>() { travelRate });
            this.retriever.Setup(x => x.GetEscalationRatesByWorkspace(workspace)).Returns(new List<EscalationRatesDTO>() { escRate });

            decimal cost = sut.CalculateTravelCost(travelTrip, workspace).CostTotal;
            Assert.IsTrue(cost > 0, "no cost");
            Assert.IsTrue(cost == 545343, "doesn't equal the amount i calculated");

        }

        /// <summary>
        /// Tests the new MIE formula (story 8721) when the workspace state is in initialized, the trip date is beyond the set trip date, and the trip is multiple days
        /// Since number of days > 1 the new formula should be used.
        /// </summary>
        [TestMethod]
        public void BL_CalculateTravelCost_UpdatedMIECalc_AfterSetTripDateWSStatusInitializedMultipleDays()
        {
            //Activation and trip comparison dates set in app.config
            //<add key="MIECostUpdateActivationDate" value="01/01/2012"/>
            //<add key ="MIECostUpdateTripDate" value="07/01/2012"/>
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
			TravelTripCostCalculation sut = new TravelTripCostCalculation();

            WorkspaceDTO workspaceDto = new WorkspaceDTO();
            workspaceDto.Id = 1;

            Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);
            FullWorkspace workspace = new FullWorkspace(workspaceDto);

            TripDTO trip = new TripDTO { TripID = 1, PerDiemID = 1, Fare = 1230.25m, MiscTravelRateID = 1, RentalCarRate = 32 };
            PerDiemDTO perDiem = new PerDiemDTO { Id = 1, HotelRate = 75, MIERate = 20.5m };
            EscalationRatesDTO escRate = new EscalationRatesDTO { EscalationRateID = 1, LMSIEscalation = 50, DevEscalation = 100, Year = 2012 };
            TravelTripType travelTrip = new TravelTripType { TravelTripID = 1, TripDate = Convert.ToDateTime("08/01/2012"), BoeID = this.Boe1.Id, GroupID = 1, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.LS, SystemTripID = 1 };
            MiscTravelRateDTO travelRate = new MiscTravelRateDTO { Id = 1, MiscTravelRate = 75 };

            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO>() { new TravelDTO() { TravelTrips = new List<TravelTripType>() { travelTrip } } });
            this.retriever.Setup(x => x.GetTravelTripsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<TripDTO>() { trip });
            this.retriever.Setup(x => x.GetTravelTripPerDiemsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<PerDiemDTO>() { perDiem });
            this.retriever.Setup(x => x.GetTravelTripMiscTravelRatesForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<MiscTravelRateDTO>() { travelRate });
            this.retriever.Setup(x => x.GetEscalationRatesByWorkspace(workspace)).Returns(new List<EscalationRatesDTO>() { escRate });

            decimal cost = sut.CalculateTravelCost(travelTrip, workspace).CostTotal;
            Assert.IsTrue(cost > 0, "no cost");
            Assert.IsTrue(cost == 588744, "doesn't equal the amount i calculated");

        }

        /// <summary>
        /// Tests the new MIE formula (story 8721) when the workspace state is in working, the trip date is beyond the set trip date, and the trip is a single day
        /// Since number of days is 1 the new formula shoudn't be used.
        /// </summary>
        [TestMethod]
        public void BL_CalculateTravelCost_UpdatedMIECalc_AfterSetTripDateWSStatusWorkingSingleDay()
        {
            //Activation and trip comparison dates set in app.config
            //<add key="MIECostUpdateActivationDate" value="01/01/2012"/>
            //<add key ="MIECostUpdateTripDate" value="07/01/2012"/>
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
			TravelTripCostCalculation sut = new TravelTripCostCalculation();

            WorkspaceDTO workspaceDto = new WorkspaceDTO();
            workspaceDto.Id = 1;

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            FullWorkspace workspace = new FullWorkspace(workspaceDto);

            TripDTO trip = new TripDTO { TripID = 1, PerDiemID = 1, Fare = 1230.25m, MiscTravelRateID = 1, RentalCarRate = 32 };
            PerDiemDTO perDiem = new PerDiemDTO { Id = 1, HotelRate = 75, MIERate = 20.5m };
            EscalationRatesDTO escRate = new EscalationRatesDTO { EscalationRateID = 1, LMSIEscalation = 50, DevEscalation = 100, Year = 2012 };
            TravelTripType travelTrip = new TravelTripType { TravelTripID = 1, TripDate = Convert.ToDateTime("08/01/2012"), BoeID = this.Boe1.Id, GroupID = 1, NumOfDays = 1, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.LS, SystemTripID = 1 };
            MiscTravelRateDTO travelRate = new MiscTravelRateDTO { Id = 1, MiscTravelRate = 75 };

            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO>() { new TravelDTO() { TravelTrips = new List<TravelTripType>() { travelTrip } } });
            this.retriever.Setup(x => x.GetTravelTripsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<TripDTO>() { trip });
            this.retriever.Setup(x => x.GetTravelTripPerDiemsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<PerDiemDTO>() { perDiem });
            this.retriever.Setup(x => x.GetTravelTripMiscTravelRatesForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<MiscTravelRateDTO>() { travelRate });
            this.retriever.Setup(x => x.GetEscalationRatesByWorkspace(workspace)).Returns(new List<EscalationRatesDTO>() { escRate });

            workspace.WorkspaceState = WorkspaceState.Working;

            decimal cost = sut.CalculateTravelCost(travelTrip, workspace).CostTotal;
            Assert.IsTrue(cost > 0, "no cost");
            Assert.IsTrue(cost == 545343, "doesn't equal the amount i calculated");

        }

        /// <summary>
        /// Tests the new MIE formula (story 8721) when the workspace state is in working, the trip date is beyond the set trip date, and the trip is multiple days
        /// Since number of days > 1 the new formula should be used.
        /// </summary>
        [TestMethod]
        public void BL_CalculateTravelCost_UpdatedMIECalc_AfterSetTripDateWSStatusWorkingMultipleDays()
        {
            //Activation and trip comparison dates set in app.config
            //<add key="MIECostUpdateActivationDate" value="01/01/2012"/>
            //<add key ="MIECostUpdateTripDate" value="07/01/2012"/>
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
			TravelTripCostCalculation sut = new TravelTripCostCalculation();

            WorkspaceDTO workspaceDto = new WorkspaceDTO();
            workspaceDto.Id = 1;

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            FullWorkspace workspace = new FullWorkspace(workspaceDto);

            TripDTO trip = new TripDTO { TripID = 1, PerDiemID = 1, Fare = 1230.25m, MiscTravelRateID = 1, RentalCarRate = 32 };
            PerDiemDTO perDiem = new PerDiemDTO { Id = 1, HotelRate = 75, MIERate = 20.5m };
            EscalationRatesDTO escRate = new EscalationRatesDTO { EscalationRateID = 1, LMSIEscalation = 50, DevEscalation = 100, Year = 2012 };
            TravelTripType travelTrip = new TravelTripType { TravelTripID = 1, TripDate = Convert.ToDateTime("08/01/2012"), BoeID = this.Boe1.Id, GroupID = 1, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.LS, SystemTripID = 1 };
            MiscTravelRateDTO travelRate = new MiscTravelRateDTO { Id = 1, MiscTravelRate = 75 };

            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO>() { new TravelDTO() { TravelTrips = new List<TravelTripType>() { travelTrip } } });
            this.retriever.Setup(x => x.GetTravelTripsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<TripDTO>() { trip });
            this.retriever.Setup(x => x.GetTravelTripPerDiemsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<PerDiemDTO>() { perDiem });
            this.retriever.Setup(x => x.GetTravelTripMiscTravelRatesForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<MiscTravelRateDTO>() { travelRate });
            this.retriever.Setup(x => x.GetEscalationRatesByWorkspace(workspace)).Returns(new List<EscalationRatesDTO>() { escRate });

            workspace.WorkspaceState = WorkspaceState.Working;

            decimal cost = sut.CalculateTravelCost(travelTrip, workspace).CostTotal;
            Assert.IsTrue(cost > 0, "no cost");
            Assert.IsTrue(cost == 588744, "doesn't equal the amount i calculated");

        }

        /// <summary>
        /// Tests the new MIE formula (story 8721) when the workspace state is in locked, the workspace was set to locked prior to activation date, 
        /// the trip date is beyond the set trip date, and the trip is multiple days
        /// Since number of days > 1 && the workspace was set to locked before the activation date the new formula shouldn't be used.
        /// </summary>
        [TestMethod]
        public void BL_CalculateTravelCost_UpdatedMIECalc_AfterSetTripDateWSStatusLockedBeforeMultipleDays()
        {
            //Activation and trip comparison dates set in app.config
            //<add key="MIECostUpdateActivationDate" value="01/01/2012"/>
            //<add key ="MIECostUpdateTripDate" value="07/01/2012"/>
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

            int wsid = 99;
            WorkspaceDTO workspaceDto = new WorkspaceDTO();
            workspaceDto.Id = wsid;

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            FullWorkspace workspace = new FullWorkspace(workspaceDto);

            Collection<WorkspaceHistoryDTO> workspaceHistory = new Collection<WorkspaceHistoryDTO>
            {
                new WorkspaceHistoryDTO { WorkspaceID = wsid, Date = new DateTime(2010, 11, 01), OldValue = WorkspaceState.None, NewValue = WorkspaceState.Initialization, PerformedByETIUserId = 1},
                new WorkspaceHistoryDTO { WorkspaceID = wsid, Date = new DateTime(2010, 12, 01), OldValue = WorkspaceState.Initialization, NewValue = WorkspaceState.Working, PerformedByETIUserId = 2},
                new WorkspaceHistoryDTO { WorkspaceID = wsid, Date = new DateTime(2011, 12, 01), OldValue = WorkspaceState.Working, NewValue = WorkspaceState.Locked, PerformedByETIUserId = 3}
            };

            retriever.Setup(x => x.GetWorkspaceHistoryByWorkspaceId(wsid)).Returns(workspaceHistory);

			TravelTripCostCalculation sut = new TravelTripCostCalculation();

            TripDTO trip = new TripDTO { TripID = 1, PerDiemID = 1, Fare = 1230.25m, MiscTravelRateID = 1, RentalCarRate = 32 };
            PerDiemDTO perDiem = new PerDiemDTO { Id = 1, HotelRate = 75, MIERate = 20.5m };
            EscalationRatesDTO escRate = new EscalationRatesDTO { EscalationRateID = 1, LMSIEscalation = 50, DevEscalation = 100, Year = 2012 };
            TravelTripType travelTrip = new TravelTripType { TravelTripID = 1, TripDate = Convert.ToDateTime("08/01/2012"), BoeID = this.Boe1.Id, GroupID = 1, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.LS, SystemTripID = 1 };
            MiscTravelRateDTO travelRate = new MiscTravelRateDTO { Id = 1, MiscTravelRate = 75 };

            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO>() { new TravelDTO() { TravelTrips = new List<TravelTripType>() { travelTrip } } });
            this.retriever.Setup(x => x.GetTravelTripsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<TripDTO>() { trip });
            this.retriever.Setup(x => x.GetTravelTripPerDiemsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<PerDiemDTO>() { perDiem });
            this.retriever.Setup(x => x.GetTravelTripMiscTravelRatesForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<MiscTravelRateDTO>() { travelRate });
            this.retriever.Setup(x => x.GetEscalationRatesByWorkspace(workspace)).Returns(new List<EscalationRatesDTO>() { escRate });

            workspace.WorkspaceState = WorkspaceState.Locked;

            decimal cost = sut.CalculateTravelCost(travelTrip, workspace).CostTotal;
            Assert.IsTrue(cost > 0, "no cost");
            Assert.IsTrue(cost == 590835, "doesn't equal the amount i calculated");

        }

        /// <summary>
        /// Tests the new MIE formula (story 8721) when the workspace state is in locked, the workspace was set to locked prior to activation date, 
        /// the trip date is beyond the set trip date, and the trip is multiple days
        /// Since number of days > 1 && the workspace was set to locked after the activation date the new formula should be used.
        /// </summary>
        [TestMethod]
        public void BL_CalculateTravelCost_UpdatedMIECalc_AfterSetTripDateWSStatusLockedAfterMultipleDays()
        {
            //Activation and trip comparison dates set in app.config
            //<add key="MIECostUpdateActivationDate" value="01/01/2012"/>
            //<add key ="MIECostUpdateTripDate" value="07/01/2012"/>
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

            int wsid = 99;
            WorkspaceDTO workspaceDto = new WorkspaceDTO();
            workspaceDto.Id = wsid;
            Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);

            FullWorkspace workspace = new FullWorkspace(workspaceDto);

            Collection<WorkspaceHistoryDTO> workspaceHistory = new Collection<WorkspaceHistoryDTO>
            {
                new WorkspaceHistoryDTO { WorkspaceID = wsid, Date = new DateTime(2010, 11, 01), OldValue = WorkspaceState.None, NewValue = WorkspaceState.Initialization, PerformedByETIUserId = 1},
                new WorkspaceHistoryDTO { WorkspaceID = wsid, Date = new DateTime(2010, 12, 01), OldValue = WorkspaceState.Initialization, NewValue = WorkspaceState.Working, PerformedByETIUserId = 2},
                new WorkspaceHistoryDTO { WorkspaceID = wsid, Date = new DateTime(2012, 02, 01), OldValue = WorkspaceState.Working, NewValue = WorkspaceState.Locked, PerformedByETIUserId = 3}
            };

            retriever.Setup(x => x.GetWorkspaceHistoryByWorkspaceId(wsid)).Returns(workspaceHistory);

			TravelTripCostCalculation sut = new TravelTripCostCalculation();

            TripDTO trip = new TripDTO { TripID = 1, PerDiemID = 1, Fare = 1230.25m, MiscTravelRateID = 1, RentalCarRate = 32 };
            PerDiemDTO perDiem = new PerDiemDTO { Id = 1, HotelRate = 75, MIERate = 20.5m };
            EscalationRatesDTO escRate = new EscalationRatesDTO { EscalationRateID = 1, LMSIEscalation = 50, DevEscalation = 100, Year = 2012 };
            TravelTripType travelTrip = new TravelTripType { TravelTripID = 1, TripDate = Convert.ToDateTime("08/01/2012"), BoeID = this.Boe1.Id, GroupID = 1, NumOfDays = 2, NumOfIntervals = 0, NumOfOccurences = 0, NumOfPeople = 4, NumOfTrips = 2, PerfOrgID = this.Perforg.Id, Segment = SegmentType.LS, SystemTripID = 1 };
            MiscTravelRateDTO travelRate = new MiscTravelRateDTO { Id = 1, MiscTravelRate = 75 };

            this.retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO>() { new TravelDTO() { TravelTrips = new List<TravelTripType>() { travelTrip } } });
            this.retriever.Setup(x => x.GetTravelTripsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<TripDTO>() { trip });
            this.retriever.Setup(x => x.GetTravelTripPerDiemsForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<PerDiemDTO>() { perDiem });
            this.retriever.Setup(x => x.GetTravelTripMiscTravelRatesForWorkspace(It.IsAny<Collection<int>>(), workspace)).Returns(new List<MiscTravelRateDTO>() { travelRate });
            this.retriever.Setup(x => x.GetEscalationRatesByWorkspace(workspace)).Returns(new List<EscalationRatesDTO>() { escRate });

            workspace.WorkspaceState = WorkspaceState.Locked;

            decimal cost = sut.CalculateTravelCost(travelTrip, workspace).CostTotal;
            Assert.IsTrue(cost > 0, "no cost");
            Assert.IsTrue(cost == 588744, "doesn't equal the amount i calculated");
        }
    }
}