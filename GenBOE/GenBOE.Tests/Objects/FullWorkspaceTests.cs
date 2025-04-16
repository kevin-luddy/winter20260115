// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.PickList;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FullWorkspaceTests
    {
        /// <summary>
        /// Test workspace dto that can be used with any test. Gets initialized in Initialze method which gets called before each
        /// test run.
        /// </summary>
        private WorkspaceDTO _workspaceDto;

        private Mock<IPermissionsDTODataLoader> _perissionsDtoDataLoader;
        private Mock<IRetriever> _retriever;
        private Mock<ICommonDataMapper> _commonDataMapper;
        private Mock<IFullObjectFactory> _factory;

        /// <summary>
        /// Initializes data before each test run for this class.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _perissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
            _retriever = new Mock<IRetriever>();
            _commonDataMapper = new Mock<ICommonDataMapper>();
            _factory = new Mock<IFullObjectFactory>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _perissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), _factory.Object);

            _workspaceDto = new WorkspaceDTO
            {
                AllowSearch = true,
                ContainsOCI = false,
                ContainsTemplate = false,
                ContractEndDate = new DateTime(2014, 12, 31),
                ContractStartDate = new DateTime(2014, 01, 01),
                CostVolumeLeadPricerUserID = 1,
                CreatedByUserID = 2,
                DateDeleted = null,
                Description = "Test Workspace",
                HasBeenDeleted = false,
                Id = 1,
                LineOfBusiness = new PickListDto
                {
                    IsActive = true,
                    Text = "Civil",
                    Id = 1
                },
                NumberOfTimesExportedToProPricer = 0,
                PerfOrgListID = 1,
                PerfOrgsChanged = false,
                ProposalClass = new PickListDto { Id = 1, Text = "Firm" },
                ProposalStatus = ProposalStatusType.Won,
                ProposalSubmittalDate = null,
                ProposalTitle = "Proposal Title",
                ResourceListID = 1,
                RFPNumber = "RFP101010",
                Segment = SegmentType.DS,
                SelectedContractTypes = new Collection<int> { 1001 },
                Shortname = "rayTest",
                StatusComment = "Workspace status comment",
                TemplateID = 1,
                TrackingNumber = "2014-01234",
                UpdateDate = DateTime.Now,
                WorkspaceName = "Rays Test Workspace",
                WorkspaceState = WorkspaceState.Working,
                ResourceDecimalPrecision = 4
            };
        }

        /// <summary>
        /// FullWorkspace constructor test.
        /// </summary>
        [TestMethod]
        public void FullWorkspaceConstructorTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);

            // Assert all properties are set from the given workspace by reflecting over the two objects.
            foreach (PropertyInfo prop in typeof(WorkspaceDTO).GetProperties())
            {
                Assert.AreEqual(prop.GetValue(_workspaceDto, null), prop.GetValue(fullWorkspace, null)); 
            }
        }

        /// <summary>
        /// FullWorkspace get and refresh Current Active User test.
        /// </summary>
        [TestMethod]
        public void CurrentActiveUserTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            UserDTO user = new UserDTO
            {
                NTID = "rfrankso",
                UserID = 1
            };

            _retriever.Setup(i => i.GetCurrentActiveUser()).Returns(user);
            
            Assert.AreEqual(user, fullWorkspace.CurrentActiveUser);
            // Call a second time. Ensure it does not go to the DB again to retrieve the user.
            UserDTO user2 = fullWorkspace.CurrentActiveUser;
            _retriever.Verify(x => x.GetCurrentActiveUser(), Times.Exactly(1));
            Assert.AreEqual(user, user2);

            // Refresh should force the object to go back to the DB and re-retrieve the user.
            fullWorkspace.RefreshCurrentActiveUser();
            user2 = fullWorkspace.CurrentActiveUser;
            _retriever.Verify(x => x.GetCurrentActiveUser(), Times.Exactly(2));
            Assert.AreEqual(user, fullWorkspace.CurrentActiveUser);
        }

        /// <summary>
        /// FullWorkspace get workspace state test.
        /// </summary>
        [TestMethod]
        public void WorkspaceStateNameTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);

            _commonDataMapper.Setup(i => i.getWorkspaceStateName(fullWorkspace.WorkspaceState)).Returns("Working");

            Assert.AreEqual(fullWorkspace.WorkspaceStateName, WorkspaceState.Working.ToString());
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            string state = fullWorkspace.WorkspaceStateName;
            _commonDataMapper.Verify(x => x.getWorkspaceStateName(fullWorkspace.WorkspaceState), Times.Exactly(1));
            Assert.AreEqual(state, WorkspaceState.Working.ToString());
        }

        /// <summary>
        /// FullWorkspace get permissions test.
        /// </summary>
        [TestMethod]
        public void WorkspacePermissionsTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>();
            permissions.Add(new PermissionsDTO { Id = 1, ETIUserId = 1, PermissionId = 1, WorkspaceId = 1 });
            _perissionsDtoDataLoader.Setup(i => i.GetWorkspacePermissions(fullWorkspace.Id)).Returns(permissions);

            IReadOnlyCollection<PermissionsDTO> returnedPermissions = fullWorkspace.WorkspacePermissions;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedPermissions = fullWorkspace.WorkspacePermissions;
            _perissionsDtoDataLoader.Verify(x => x.GetWorkspacePermissions(fullWorkspace.Id), Times.Exactly(1));
            Assert.AreEqual(1, returnedPermissions.Count);
            Assert.AreEqual(permissions[0].Id, returnedPermissions.First().Id);
        }

        /// <summary>
        /// FullWorkspace get clins test.
        /// </summary>
        [TestMethod]
        public void WorkspaceClinsTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<FullClin> clins = new Collection<FullClin>();
            clins.Add(new FullClin(new ClinDTO { Id = 1, WorkspaceID = 1 }));
            _retriever.Setup(i => i.GetClinsByWorkspaceId(fullWorkspace.Id)).Returns(clins);

            IReadOnlyCollection<FullClin> returnedClins = fullWorkspace.Clins;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedClins = fullWorkspace.Clins;
            _retriever.Verify(x => x.GetClinsByWorkspaceId(fullWorkspace.Id), Times.Exactly(1));
            Assert.AreEqual(1, returnedClins.Count);
            Assert.AreEqual(clins[0].Id, returnedClins.First().Id);
        }

        /// <summary>
        /// FullWorkspace get wbs test.
        /// </summary>
        [TestMethod]
        public void WorkspaceWbsTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<FullWbs> wbs = new Collection<FullWbs>();
            wbs.Add(new FullWbs(new WbsDTO { Id = 1, WorkspaceID = 1, WbsPaddedNumber="100000" }));
            _retriever.Setup(i => i.GetFullWbsElementsByWorkspaceId(fullWorkspace.Id)).Returns(wbs);

            Collection<FullWbs> returnedWbs = fullWorkspace.WbsElements.ToCollection();
            Assert.IsTrue(returnedWbs[0].Id == wbs[0].Id && returnedWbs[0].WorkspaceID == wbs[0].WorkspaceID && returnedWbs[0].WbsPaddedNumber == wbs[0].WbsPaddedNumber);
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedWbs = fullWorkspace.WbsElements.ToCollection();
            _retriever.Verify(x => x.GetFullWbsElementsByWorkspaceId(fullWorkspace.Id), Times.Exactly(1));
            Assert.IsTrue(returnedWbs[0].Id == wbs[0].Id && returnedWbs[0].WorkspaceID == wbs[0].WorkspaceID && returnedWbs[0].WbsPaddedNumber == wbs[0].WbsPaddedNumber);
        }

        /// <summary>
        /// FullWorkspace get boe test.
        /// </summary>
        [TestMethod]
        public void WorkspaceBoeTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<FullBoe> boe = new Collection<FullBoe>()
            {
                new FullBoe(new BoeDTO { Id = 1, WorkspaceID = 1 })
            };
            
            _retriever.Setup(i => i.GetFullBoesByWorkspaceId(fullWorkspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(boe);

            IReadOnlyCollection<FullBoe> returnedBoe = fullWorkspace.Boes;

            Assert.AreEqual(boe[0].Id, returnedBoe.First().Id);
            
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedBoe = fullWorkspace.Boes;
            _retriever.Verify(x => x.GetFullBoesByWorkspaceId(fullWorkspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>()), Times.Exactly(1));
            
            // Refresh and ensure task elements are re-fetched from the DB.
            fullWorkspace.RefreshBoes();
            returnedBoe = fullWorkspace.Boes;
            _retriever.Verify(x => x.GetFullBoesByWorkspaceId(fullWorkspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>()), Times.Exactly(2));
            Assert.AreEqual(1, returnedBoe.Count);
            Assert.AreEqual(boe[0].Id, returnedBoe.First().Id);

            // Now need to check the RTE fields.. Do a load of RTE data only
            fullWorkspace.RefreshBoes();
            returnedBoe = fullWorkspace.Boes;
            Assert.IsTrue(returnedBoe.Any());
            Assert.IsFalse(returnedBoe.First().WasDataSourceSet);
            Assert.IsFalse(returnedBoe.First().WasDescriptionSet);

            _retriever.Setup(x => x.PopulateRTEData(boe));
            fullWorkspace.LoadBoesAndTaskElementsRTEData();
            Assert.IsTrue(returnedBoe.Any());
            _retriever.Verify(x => x.PopulateRTEData(boe), Times.Exactly(1));

            // Do a freash load w/ the data
            fullWorkspace.RefreshBoes();
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(fullWorkspace.Id, true, fullWorkspace.DecimalPrecision, fullWorkspace.CostDecimalPrecision)).Returns(new List<BoeTaskElementDTO>());
			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(fullWorkspace.Id, true, It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(boe);
            fullWorkspace.LoadBoesAndTaskElementsRTEData();
            returnedBoe = fullWorkspace.Boes; // this should just return the Boe, as the load would have loaded the whole thing
            Assert.IsTrue(returnedBoe.Any());
            _retriever.Verify(x => x.GetFullBoesByWorkspaceId(fullWorkspace.Id, true, It.IsAny<IEnumerable<BoeTaskElementDTO>>()), Times.Exactly(1));
            _retriever.Verify(x => x.GetFullBoesByWorkspaceId(fullWorkspace.Id, false, It.IsAny<IEnumerable<BoeTaskElementDTO>>()), Times.Exactly(3)); // this would have been called by .Boes, but that should not be loading anything
        }

        /// <summary>
        /// FullWorkspace get task elements test.
        /// </summary>
        [TestMethod]
        public void WorkspaceTaskElementsTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<BoeTaskElementDTO> taskElements = new Collection<BoeTaskElementDTO>();
            taskElements.Add(new BoeTaskElementDTO { Id = 1, BoeID = 1 });
            taskElements.Add(new BoeTaskElementDTO { Id = 2, BoeID = 1 });
            _retriever.Setup(i => i.GetBoeTaskElementCollectionByWorkspaceId(fullWorkspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(taskElements);

            IReadOnlyCollection<BoeTaskElementDTO> returnedTaskElements = fullWorkspace.TaskElements;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedTaskElements = fullWorkspace.TaskElements;
            _retriever.Verify(x => x.GetBoeTaskElementCollectionByWorkspaceId(fullWorkspace.Id, false, It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
            // Refresh and ensure task elements are re-fetched from the DB.
            fullWorkspace.RefreshTaskElements();
            returnedTaskElements = fullWorkspace.TaskElements;
            _retriever.Verify(x => x.GetBoeTaskElementCollectionByWorkspaceId(fullWorkspace.Id, false, It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
            Assert.IsTrue(returnedTaskElements.Count == 2);
            Assert.AreEqual(taskElements[0].Id, returnedTaskElements.First().Id);
            Assert.AreEqual(taskElements[1].Id, returnedTaskElements.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get workspace variables test.
        /// </summary>
        [TestMethod]
        public void WorkspaceWorkspaceVariablesTest()
        {

















            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<WorkspaceVariableDTO> workspaceVariables = new Collection<WorkspaceVariableDTO>();
            workspaceVariables.Add(new WorkspaceVariableDTO { Id = 1, WorkspaceID = 1 });
            workspaceVariables.Add(new WorkspaceVariableDTO { Id = 2, WorkspaceID = 1 });
            _retriever.Setup(i => i.GetWorkspaceVariableDTOsByWorkspaceId(fullWorkspace.Id)).Returns(workspaceVariables);

            IReadOnlyCollection<WorkspaceVariableDTO> returnedWorkspaceVariables = fullWorkspace.WorkspaceVariables;
            
            // Call a second time. Ensure it does not go to the DB again to retrieve the state.
            returnedWorkspaceVariables = fullWorkspace.WorkspaceVariables;
            _retriever.Verify(x => x.GetWorkspaceVariableDTOsByWorkspaceId(fullWorkspace.Id), Times.Exactly(1));
            // Refresh and ensure workspace variables are re-fetched from the DB.
            fullWorkspace.RefreshWorkspaceVariables();
            returnedWorkspaceVariables = fullWorkspace.WorkspaceVariables;
            _retriever.Verify(x => x.GetWorkspaceVariableDTOsByWorkspaceId(fullWorkspace.Id), Times.Exactly(2));
            Assert.IsTrue(returnedWorkspaceVariables.Count == 2);

            Assert.AreEqual(workspaceVariables[0].Id, returnedWorkspaceVariables.First().Id);
            Assert.AreEqual(workspaceVariables[1].Id, returnedWorkspaceVariables.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get travel test.
        /// </summary>
        [TestMethod]
        public void WorkspaceTravelsTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<TravelDTO> travels = new Collection<TravelDTO>();
            travels.Add(new TravelDTO { Id = 1, BoeID = 1 });
            travels.Add(new TravelDTO { Id = 2, BoeID = 1 });
            _retriever.Setup(i => i.GetTravelByWorkspaceId(fullWorkspace.Id, It.IsAny<bool>())).Returns(travels);



            IReadOnlyCollection<TravelDTO> returnedTravels = fullWorkspace.Travels;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedTravels = fullWorkspace.Travels;
            _retriever.Verify(x => x.GetTravelByWorkspaceId(fullWorkspace.Id, It.IsAny<bool>()), Times.Exactly(1));
            Assert.IsTrue(returnedTravels.Count == 2);
            Assert.AreEqual(travels[0].Id, returnedTravels.First().Id);
            Assert.AreEqual(travels[1].Id, returnedTravels.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get travel trips test.
        /// </summary>
        [TestMethod]
        public void WorkspaceTravelTripsTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            
            ICollection<TripDTO> trips = new Collection<TripDTO>();
            trips.Add(new TripDTO { Id = 1 });
            trips.Add(new TripDTO { Id = 2 });
            _retriever.Setup(i => i.GetTravelTripsForWorkspace(It.IsAny<Collection<int>>(), fullWorkspace)).Returns(trips);

            Collection<TravelDTO> travels = new Collection<TravelDTO>();
            travels.Add(new TravelDTO { Id = 1, BoeID = 1, TravelTrips = new Collection<TravelTripType> { new TravelTripType { SystemTripID = 1 } } });
            travels.Add(new TravelDTO { Id = 2, BoeID = 1 });
            _retriever.Setup(i => i.GetTravelByWorkspaceId(fullWorkspace.Id, It.IsAny<bool>())).Returns(travels);

            IReadOnlyCollection<TripDTO> returnedTrips = fullWorkspace.TravelTrips;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedTrips = fullWorkspace.TravelTrips;
            _retriever.Verify(x => x.GetTravelTripsForWorkspace(It.IsAny<Collection<int>>(), fullWorkspace), Times.Exactly(1));
            Assert.IsTrue(returnedTrips.Count == 2);
            Assert.AreEqual(travels[0].Id, returnedTrips.First().Id);
            Assert.AreEqual(travels[1].Id, returnedTrips.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get travel trip per diems test.
        /// </summary>
        [TestMethod]
        public void WorkspacePerDiemTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);

            ICollection<TripDTO> trips = new Collection<TripDTO>();
            trips.Add(new TripDTO { Id = 1 });
            trips.Add(new TripDTO { Id = 2 });
            _retriever.Setup(i => i.GetTravelTripsForWorkspace(It.IsAny<Collection<int>>(), fullWorkspace)).Returns(trips);

            Collection<TravelDTO> travels = new Collection<TravelDTO>();
            travels.Add(new TravelDTO { Id = 1, BoeID = 1, TravelTrips = new Collection<TravelTripType> { new TravelTripType { SystemTripID = 1 } } });
            travels.Add(new TravelDTO { Id = 2, BoeID = 1 });
            _retriever.Setup(i => i.GetTravelByWorkspaceId(fullWorkspace.Id, It.IsAny<bool>())).Returns(travels);

            Collection<PerDiemDTO> perDiems = new Collection<PerDiemDTO>();
            perDiems.Add(new PerDiemDTO { Id = 1 });
            perDiems.Add(new PerDiemDTO { Id = 2 });
            _retriever.Setup(i => i.GetTravelTripPerDiemsForWorkspace(It.IsAny<Collection<int>>(), fullWorkspace)).Returns(perDiems);

            IReadOnlyCollection<PerDiemDTO> returnedPerDiems = fullWorkspace.PerDiemsForTravelTrips;
            
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedPerDiems = fullWorkspace.PerDiemsForTravelTrips;
            _retriever.Verify(x => x.GetTravelTripPerDiemsForWorkspace(It.IsAny<Collection<int>>(), fullWorkspace), Times.Exactly(1));
            Assert.IsTrue(returnedPerDiems.Count == 2);
            Assert.AreEqual(perDiems[0].Id, returnedPerDiems.First().Id);
            Assert.AreEqual(perDiems[1].Id, returnedPerDiems.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get travel trips misc rates test.
        /// </summary>
        [TestMethod]
        public void WorkspaceTravelRatesTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);

            ICollection<TripDTO> trips = new Collection<TripDTO>();
            trips.Add(new TripDTO { Id = 1 });
            trips.Add(new TripDTO { Id = 2 });
            _retriever.Setup(i => i.GetTravelTripsForWorkspace(It.IsAny<Collection<int>>(), fullWorkspace)).Returns(trips);

            Collection<TravelDTO> travels = new Collection<TravelDTO>();
            travels.Add(new TravelDTO { Id = 1, BoeID = 1, TravelTrips = new Collection<TravelTripType> { new TravelTripType { SystemTripID = 1 } } });
            travels.Add(new TravelDTO { Id = 2, BoeID = 1 });
            _retriever.Setup(i => i.GetTravelByWorkspaceId(fullWorkspace.Id, It.IsAny<bool>())).Returns(travels);

            Collection<MiscTravelRateDTO> rates = new Collection<MiscTravelRateDTO>();
            rates.Add(new MiscTravelRateDTO { Id = 1 });
            rates.Add(new MiscTravelRateDTO { Id = 2 });
            _retriever.Setup(i => i.GetTravelTripMiscTravelRatesForWorkspace(It.IsAny<Collection<int>>(), fullWorkspace)).Returns(rates);

            IReadOnlyCollection<MiscTravelRateDTO> returnedRates = fullWorkspace.MiscTravelRatesForTravelTrips;
            

            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedRates = fullWorkspace.MiscTravelRatesForTravelTrips;
            _retriever.Verify(x => x.GetTravelTripMiscTravelRatesForWorkspace(It.IsAny<Collection<int>>(), fullWorkspace), Times.Exactly(1));
            Assert.IsTrue(returnedRates.Count == 2);
            Assert.AreEqual(rates[0].Id, returnedRates.First().Id);
            Assert.AreEqual(rates[1].Id, returnedRates.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get workspace history test.
        /// </summary>
        [TestMethod]
        public void WorkspaceHistoryTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<WorkspaceHistoryDTO> history = new Collection<WorkspaceHistoryDTO>();
            history.Add(new WorkspaceHistoryDTO {  WorkspaceID = 1 });
            history.Add(new WorkspaceHistoryDTO { WorkspaceID = 1 });
            _retriever.Setup(i => i.GetWorkspaceHistoryByWorkspaceId(fullWorkspace.Id)).Returns(history);

            IReadOnlyCollection<WorkspaceHistoryDTO> returnedHistory = fullWorkspace.WorkspaceHistory;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedHistory = fullWorkspace.WorkspaceHistory;
            _retriever.Verify(x => x.GetWorkspaceHistoryByWorkspaceId(fullWorkspace.Id), Times.Exactly(1));
            Assert.IsTrue(returnedHistory.Count == 2);
            Assert.AreEqual(history[0].WorkspaceID, returnedHistory.First().WorkspaceID);
            Assert.AreEqual(history[1].WorkspaceID, returnedHistory.Last().WorkspaceID);
        }

        /// <summary>
        /// FullWorkspace get Escalation Rates test.
        /// </summary>
        [TestMethod]
        public void WorkspaceEscalationRatesTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<EscalationRatesDTO> rates = new Collection<EscalationRatesDTO>();
            rates.Add(new EscalationRatesDTO { Id = 1 });
            rates.Add(new EscalationRatesDTO { Id = 2 });
            _retriever.Setup(i => i.GetEscalationRatesByWorkspace(fullWorkspace)).Returns(rates);

            IReadOnlyCollection<EscalationRatesDTO> returnedRates = fullWorkspace.EscalationRates;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedRates = fullWorkspace.EscalationRates;
            _retriever.Verify(x => x.GetEscalationRatesByWorkspace(fullWorkspace), Times.Exactly(1));
            Assert.IsTrue(returnedRates.Count == 2);
            Assert.AreEqual(rates[0].Id, returnedRates.First().Id);
            Assert.AreEqual(rates[1].Id, returnedRates.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get Workspace Resources test.
        /// </summary>
        [TestMethod]
        public void WorkspaceResourcesTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            resources.Add(new ResourceDTO { Id = 1 });
            resources.Add(new ResourceDTO { Id = 2 });
            _retriever.Setup(i => i.GetResourcesByResourceListId(fullWorkspace.ResourceListID)).Returns(resources);

            IReadOnlyCollection<ResourceDTO> returnedResources = fullWorkspace.ResourcesForWsResourceListId;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedResources = fullWorkspace.ResourcesForWsResourceListId;
            _retriever.Verify(x => x.GetResourcesByResourceListId(fullWorkspace.Id), Times.Exactly(1));
            Assert.IsTrue(returnedResources.Count == 2);
            Assert.AreEqual(resources[0].Id, returnedResources.First().Id);
            Assert.AreEqual(resources[1].Id, returnedResources.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get Workspace Export Formats test.
        /// </summary>
        [TestMethod]
        public void WorkspaceExportFormatTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            fullWorkspace.TemplateID = 3;

            Collection<WorkspaceExportFormatDTO> export = new Collection<WorkspaceExportFormatDTO>();
            export.Add(new WorkspaceExportFormatDTO { Id = 1 });
            export.Add(new WorkspaceExportFormatDTO { Id = 2 });

            this._retriever.Setup(i => i.GetWorkspaceExportFormatsByWorkspaceId(fullWorkspace.Id)).Returns(export);
            this._retriever.Setup(x => x.GetWorkspaceExportFormatByTemplateId(fullWorkspace.TemplateID))
                .Returns(new WorkspaceExportFormatDTO() {Id = 3});

            IReadOnlyCollection<WorkspaceExportFormatDTO> returnedExports = fullWorkspace.WorkspaceExportFormats;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedExports = fullWorkspace.WorkspaceExportFormats;
            this._retriever.Verify(x => x.GetWorkspaceExportFormatsByWorkspaceId(fullWorkspace.Id), Times.Exactly(1));
            Assert.IsTrue(returnedExports.Count == 3);
            Assert.AreEqual(export[0].Id, returnedExports.First().Id);
            Assert.AreEqual(export[1].Id, returnedExports.ElementAt(1).Id);
            Assert.AreEqual(fullWorkspace.TemplateID, returnedExports.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get Workspace Version Meta Data test.
        /// </summary>
        [TestMethod]
        public void WorkspaceVersionMetaDataTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<WorkspaceVersionMetaDataDTO> data = new Collection<WorkspaceVersionMetaDataDTO>();
            data.Add(new WorkspaceVersionMetaDataDTO { Id = 1 });
            data.Add(new WorkspaceVersionMetaDataDTO { Id = 2 });
            _retriever.Setup(i => i.GetWorkspaceVersionMetaDataByWorkspaceId(fullWorkspace.Id)).Returns(data);

            IReadOnlyCollection<WorkspaceVersionMetaDataDTO> returnedData = fullWorkspace.WorkspaceVersionMetaData;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedData = fullWorkspace.WorkspaceVersionMetaData;
            _retriever.Verify(x => x.GetWorkspaceVersionMetaDataByWorkspaceId(fullWorkspace.Id), Times.Exactly(1));
            Assert.IsTrue(returnedData.Count == 2);
            Assert.AreEqual(data[0].Id, returnedData.First().Id);
            Assert.AreEqual(data[1].Id, returnedData.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get Custom Fields test.
        /// </summary>
        [TestMethod]
        public void WorkspaceCustomFieldsTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<CustomFieldDTO> fields = new Collection<CustomFieldDTO>();
            fields.Add(new CustomFieldDTO { Id = 1 });
            fields.Add(new CustomFieldDTO { Id = 2 });
            _retriever.Setup(i => i.GetCustomFieldsByWorkspaceId(fullWorkspace.Id)).Returns(fields);

            IReadOnlyCollection<CustomFieldDTO> returnedFields = fullWorkspace.CustomFields;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedFields = fullWorkspace.CustomFields;
            _retriever.Verify(x => x.GetCustomFieldsByWorkspaceId(fullWorkspace.Id), Times.Exactly(1));
            Assert.IsTrue(returnedFields.Count == 2);
            Assert.AreEqual(fields[0].Id, returnedFields.First().Id);
            Assert.AreEqual(fields[1].Id, returnedFields.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get Pro Pricer Exports test.
        /// </summary>
        [TestMethod]
        public void WorkspaceProPricerExportsTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<ProPricerDTO> exports = new Collection<ProPricerDTO>();
            exports.Add(new ProPricerDTO { Id = 1, WorkspaceID = 1 });
            exports.Add(new ProPricerDTO { Id = 2, WorkspaceID = 1 });
            _retriever.Setup(i => i.GetProPricerExportsByWorkspaceId(fullWorkspace.Id)).Returns(exports);

            IReadOnlyCollection<ProPricerDTO> returnedExports = fullWorkspace.ProPricerExports;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedExports = fullWorkspace.ProPricerExports;
            _retriever.Verify(x => x.GetProPricerExportsByWorkspaceId(fullWorkspace.Id), Times.Exactly(1));
            Assert.IsTrue(returnedExports.Count == 2);
            Assert.AreEqual(exports[0].Id, returnedExports.First().Id);
            Assert.AreEqual(exports[1].Id, returnedExports.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get Performing Orgs test.
        /// </summary>
        [TestMethod]
        public void WorkspacePerformingOrgsTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<PerformingOrgDTO> orgs = new Collection<PerformingOrgDTO>();
            orgs.Add(new PerformingOrgDTO { Id = 1 });
            orgs.Add(new PerformingOrgDTO { Id = 2 });
            _retriever.Setup(i => i.GetPerformingOrgsByListId(fullWorkspace.PerfOrgListID)).Returns(orgs);

            IReadOnlyCollection<PerformingOrgDTO> returnedOrgs = fullWorkspace.PerformingOrgsForWsList;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedOrgs = fullWorkspace.PerformingOrgsForWsList;
            _retriever.Verify(x => x.GetPerformingOrgsByListId(fullWorkspace.PerfOrgListID), Times.Exactly(1));
            Assert.IsTrue(returnedOrgs.Count == 2);
            Assert.AreEqual(orgs[0].Id, returnedOrgs.First().Id);
            Assert.AreEqual(orgs[1].Id, returnedOrgs.Last().Id);
        }

        /// <summary>
        /// FullWorkspace get Performing Org List test.
        /// </summary>
        [TestMethod]
        public void WorkspacePerformingOrgListTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            PerformingOrgListDTO orgs = new PerformingOrgListDTO { Id = 1, PerformingOrgListID = 1 };
            _retriever.Setup(i => i.GetPerfOrgList()).Returns(orgs);

            PerformingOrgListDTO returnedOrgs = fullWorkspace.PerformingOrgList;
            Assert.AreEqual(returnedOrgs, orgs);
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedOrgs = fullWorkspace.PerformingOrgList;
            _retriever.Verify(x => x.GetPerfOrgList(), Times.Exactly(1));
            Assert.AreEqual(returnedOrgs, orgs);
        }

        /// <summary>
        /// FullWorkspace get Materials test.
        /// </summary>
        [TestMethod]
        public void WorkspaceMaterialsTest()
        {
            FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
            Collection<MaterialDTO> materials = new Collection<MaterialDTO>
			{
				new MaterialDTO { Id = 1 },
				new MaterialDTO { Id = 2 }
			};

            _retriever.Setup(i => i.GetMaterialsByWorkspaceId(fullWorkspace.Id, false)).Returns(materials);

            IReadOnlyCollection<MaterialDTO> returnedMaterials = fullWorkspace.Materials;
            // Call a second time. Ensure it does not go to the DB again to retrieve.
            returnedMaterials = fullWorkspace.Materials;

            _retriever.Verify(x => x.GetMaterialsByWorkspaceId(fullWorkspace.Id, false), Times.Once());
            Assert.IsTrue(returnedMaterials.Count == 2);
            Assert.AreEqual(materials[0].Id, returnedMaterials.First().Id);
            Assert.AreEqual(materials[1].Id, returnedMaterials.Last().Id);
        }

		/// <summary>
		/// FullWorkspace LoadMaterialsRTEData test.
		/// </summary>
		[TestMethod]
		public void LoadMaterialsRTEDataTest()
		{
			FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
			Collection<MaterialDTO> materials = new Collection<MaterialDTO>
			{
				new MaterialDTO { Id = 1 },
				new MaterialDTO { Id = 2 }
			};

			_retriever.Setup(i => i.GetMaterialsByWorkspaceId(fullWorkspace.Id, true)).Returns(materials);

			// Call twice - first call hits GetMaterialsByWorkspaceId, second hits PopulateRTEData
			fullWorkspace.LoadMaterialsRTEData();
			fullWorkspace.LoadMaterialsRTEData();

			IReadOnlyCollection<MaterialDTO> returnedMaterials = fullWorkspace.Materials;

			// verify GetMaterialsByWorkspaceId called by LoadMaterialsRTEData
			_retriever.Verify(x => x.GetMaterialsByWorkspaceId(fullWorkspace.Id, true), Times.Once());
			// verify not called by .Materials
			_retriever.Verify(x => x.GetMaterialsByWorkspaceId(fullWorkspace.Id, false), Times.Never());
			_retriever.Verify(x => x.PopulateRTEData(It.IsAny<ICollection<MaterialDTO>>()), Times.Once());
			Assert.IsTrue(returnedMaterials.Count == 2);
			Assert.AreEqual(materials[0].Id, returnedMaterials.First().Id);
			Assert.AreEqual(materials[1].Id, returnedMaterials.Last().Id);
		}

		/// <summary>
		/// Verifies the last accessed only called once.
		/// </summary>
		[TestMethod]
        public void VerifyLastAccessedTest()
        {
            Mock<IWorkspaceDTODataLoader> wsLoader = new Mock<IWorkspaceDTODataLoader>();
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();
            FullObjectFactory sut = new FullObjectFactory(wsLoader.Object, null, null, null, null, null, null, null, null, null, null, userLoader.Object);
            int wsId = 3;
            UserDTO user = new UserDTO { UserID = 5 };
            userLoader.Setup(x => x.GetUserForActiveUser()).Returns(user);
            wsLoader.Setup(x => x.UpdateLastAccessed(wsId, user.UserID));
            wsLoader.Setup(x => x.GetById(wsId)).Returns(new WorkspaceDTO { Id = wsId });

			// Clear cache to ensure it's empty when running all tests
			sut.ClearLastAccessCache(wsId, user.UserID);

            FullWorkspace ws = sut.CreateFullWorkspace(wsId);
            ws = sut.CreateFullWorkspace(wsId);

            wsLoader.Verify(x => x.UpdateLastAccessed(wsId, user.UserID), Times.Exactly(1));
		}

		/// <summary>
		/// FullWorkspace get Odcs test.
		/// </summary>
		[TestMethod]
		public void WorkspaceOdcsTest()
		{
			FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
			Collection<OtherDirectCostDTO> odcs = new Collection<OtherDirectCostDTO>
			{
				new OtherDirectCostDTO { Id = 1 },
				new OtherDirectCostDTO { Id = 2 }
			};

			_retriever.Setup(i => i.GetOdcCollectionByWorkspaceId(fullWorkspace.Id, false)).Returns(odcs);

			IReadOnlyCollection<OtherDirectCostDTO> returnedOdcs = fullWorkspace.Odcs;
			// Call a second time. Ensure it does not go to the DB again to retrieve.
			returnedOdcs = fullWorkspace.Odcs;

			_retriever.Verify(x => x.GetOdcCollectionByWorkspaceId(fullWorkspace.Id, false), Times.Once());
			Assert.IsTrue(returnedOdcs.Count == 2);
			Assert.AreEqual(odcs[0].Id, returnedOdcs.First().Id);
			Assert.AreEqual(odcs[1].Id, returnedOdcs.Last().Id);
		}

		/// <summary>
		/// FullWorkspace LoadODCsRTEData test.
		/// </summary>
		[TestMethod]
		public void LoadODCsRTEDataTest()
		{
			FullWorkspace fullWorkspace = new FullWorkspace(_workspaceDto);
			Collection<OtherDirectCostDTO> odcs = new Collection<OtherDirectCostDTO>
			{
				new OtherDirectCostDTO { Id = 1 },
				new OtherDirectCostDTO { Id = 2 }
			};

			_retriever.Setup(i => i.GetOdcCollectionByWorkspaceId(fullWorkspace.Id, true)).Returns(odcs);

			// Call twice - first call hits GetMaterialsByWorkspaceId, second hits PopulateRTEData
			fullWorkspace.LoadODCsRTEData();
			fullWorkspace.LoadODCsRTEData();

			IReadOnlyCollection<OtherDirectCostDTO> returnedOdcs = fullWorkspace.Odcs;

			// verify GetOdcCollectionByWorkspaceId called by LoadMaterialsRTEData
			_retriever.Verify(x => x.GetOdcCollectionByWorkspaceId(fullWorkspace.Id, true), Times.Once());
			// verify not called by .Materials
			_retriever.Verify(x => x.GetOdcCollectionByWorkspaceId(fullWorkspace.Id, false), Times.Never());
			_retriever.Verify(x => x.PopulateRTEData(It.IsAny<ICollection<OtherDirectCostDTO>>()), Times.Once());
			Assert.IsTrue(returnedOdcs.Count == 2);
			Assert.AreEqual(odcs[0].Id, returnedOdcs.First().Id);
			Assert.AreEqual(odcs[1].Id, returnedOdcs.Last().Id);
		}
	}
}
