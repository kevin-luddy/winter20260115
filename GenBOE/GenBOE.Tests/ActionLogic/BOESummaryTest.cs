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
    using System.Linq;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestClass]
    public class BOESummaryTest : MOQObject
    {
        private Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
        private Mock<IRetriever> retriever = new Mock<IRetriever>();
        private Mock<IPermissionsDTODataLoader> _permissions = new Mock<IPermissionsDTODataLoader>();
      

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [TestMethod]
        public void BL_BOESummaryTest()
        {
            int wsid = 99;
            WorkspaceDTO workspaceDto = new WorkspaceDTO();
            workspaceDto.Id = wsid;
            Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);

            FullWorkspace workspace = new FullWorkspace(workspaceDto);
            FullBoe boe = new FullBoe(Boe1);

            var ResourceLoader = new Mock<IResourceDTODataLoader>();
            var TravelTripCostCalculator = new Mock<TravelTripCostCalculation>();

            ResourceTypeDto labor = new ResourceTypeDto { ResourceID = Resource.Id, BoeID = Boe1.Id, Id = 1, ValueSpread = 200 };
            BoeTaskElementDTO task = new BoeTaskElementDTO { Id = 1, BoeID = Boe1.Id, taskElementLabors = new Collection<ResourceTypeDto> { labor }, TaskElementType = IES.Common.TaskElementType.Labor };

            ResourceLoader.Setup(x => x.GetById(this.Resource.Id)).Returns(this.Resource);
            ResourceLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { this.Resource });
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(It.IsAny<int>(), false, It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO>() { task });
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new List<FullBoe>() { boe });
            retriever.Setup(x => x.GetOdcCollectionByBoeIds(new Collection<int>() { boe.Id }, false)).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = Perforg.Id, ResourceID = Resource.Id } } } });
            retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
            retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, false)).Returns(new Collection<MaterialDTO> { });
            retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { this.Resource });

            List <WorkspaceRMSTravelNonzoneFeesAndCostsDTO> workspaceRMSTravelNonzoneFeesandCosts = new List<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>
            {
                new WorkspaceRMSTravelNonzoneFeesAndCostsDTO { ModeID = (int)MSTTravelMode.None, MiscOther = 1, TravelAgencyFee = 1 },
                new WorkspaceRMSTravelNonzoneFeesAndCostsDTO { ModeID = (int)MSTTravelMode.NonZoneDomestic, MiscOther = 1, TravelAgencyFee =1},
                new WorkspaceRMSTravelNonzoneFeesAndCostsDTO { ModeID = (int)MSTTravelMode.NonZoneInternational, MiscOther = 1, TravelAgencyFee = 1 },
                new WorkspaceRMSTravelNonzoneFeesAndCostsDTO { ModeID = (int)MSTTravelMode.ZoneAirfare, MiscOther = 1, TravelAgencyFee = 1 },
                new WorkspaceRMSTravelNonzoneFeesAndCostsDTO { ModeID = (int)MSTTravelMode.ZoneNoAirfare, MiscOther =1, TravelAgencyFee = 1 }
            };

            List<WorkspaceRMSEscalationRatesDTO> workspaceRMSEscalationRates = new List<WorkspaceRMSEscalationRatesDTO>
            {
                new WorkspaceRMSEscalationRatesDTO { Year = 2015, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1, Id = 1, WorkspaceId = wsid },
                new WorkspaceRMSEscalationRatesDTO { Year = 2016, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1, Id = 1, WorkspaceId = wsid },
                new WorkspaceRMSEscalationRatesDTO { Year = 2017, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1, Id = 1, WorkspaceId = wsid },
                new WorkspaceRMSEscalationRatesDTO { Year = 2018, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1, Id = 1, WorkspaceId = wsid },
                new WorkspaceRMSEscalationRatesDTO { Year = 2019, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1, Id = 1, WorkspaceId = wsid },
                new WorkspaceRMSEscalationRatesDTO { Year = 2020, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1, Id = 1, WorkspaceId = wsid },
                new WorkspaceRMSEscalationRatesDTO { Year = 2021, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1, Id = 1, WorkspaceId = wsid },
                new WorkspaceRMSEscalationRatesDTO { Year = 2022, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1, Id = 1, WorkspaceId = wsid }
            };

            TravelDTO travel = new TravelDTO();
            travel.BoeID = boe.Id;

            MSTTravelTripType trip1 = new MSTTravelTripType();
            //set up trip costs (other fields not needed)
            trip1.ModeID = MSTTravelMode.NonZoneDomestic;
            trip1.NonZoneAirfareEstimate = 200;
            trip1.NonZoneCarRentalTrans = 100;
            trip1.NonZonePerDiemDaily = 50;
            trip1.NumOfDays = 2;
            trip1.NumOfPeople = 3;
            trip1.NonZoneNumCars = 1;
            trip1.TripDate = DateTime.Now;
            trip1.EstimateDate = DateTime.Now;

            MSTTravelTripType trip2 = new MSTTravelTripType();
            //set up trip costs (other fields not needed)
            trip2.ModeID = MSTTravelMode.NonZoneInternational;
            trip2.NonZoneAirfareEstimate = 100;
            trip2.NonZoneCarRentalTrans = 10;
            trip2.NonZonePerDiemDaily = 20;
            trip2.NumOfDays = 1;
            trip2.NumOfPeople = 1;
            trip2.NonZoneNumCars = 1;
            trip2.TripDate = DateTime.Now;
            trip2.EstimateDate = DateTime.Now;

            MSTTravelTripType trip3 = new MSTTravelTripType();
            //set up trip costs (other fields not needed)
            trip3.ModeID = MSTTravelMode.ZoneAirfare;
            trip3.NonZoneAirfareEstimate = 200;
            trip3.NonZoneCarRentalTrans = 100;
            trip3.NonZonePerDiemDaily = 50;
            trip3.NumOfDays = 2;
            trip3.NumOfPeople = 3;
            trip3.NonZoneNumCars = 1;
            trip3.TripDate = DateTime.Now;
            trip3.EstimateDate = DateTime.Now;

            MSTTravelTripType trip4 = new MSTTravelTripType();
            //set up trip costs (other fields not needed)
            trip4.ModeID = MSTTravelMode.ZoneNoAirfare;
            trip4.NonZoneAirfareEstimate = 200;
            trip4.NonZoneCarRentalTrans = 100;
            trip4.NonZonePerDiemDaily = 50;
            trip4.NumOfDays = 2;
            trip4.NumOfPeople = 3;
            trip4.NonZoneNumCars = 1;
            trip4.TripDate = DateTime.Now;
            trip4.EstimateDate = DateTime.Now;

            //set up needed fields in travel dto and expected MV
            travel.Id = 1;
            travel.TaskID = "1";
            travel.TaskTitle = "TestTitle";
            travel.StartDate = DateTime.Today.AddMonths(-1);
            travel.EndDate = DateTime.Today.AddMonths(1);
            travel.MSTTravelTrips.Add(trip1);
            travel.MSTTravelTrips.Add(trip2);
            travel.MSTTravelTrips.Add(trip3);
            travel.MSTTravelTrips.Add(trip4);

            var rmsTripCalculate = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            rmsTripCalculate.Setup(x => x.getAllFeesAndCostsByWorkspace(workspace.Id)).Returns(workspaceRMSTravelNonzoneFeesandCosts);
            rmsTripCalculate.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(workspaceRMSEscalationRates);
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, false)).Returns(new Collection<TravelDTO>() { travel });
            retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, false)).Returns(new Collection<TravelDTO> { travel });
            retriever.Setup(x => x.GetMaterialsByBoeIds(new Collection<int> { boe.Id }, false)).Returns(new Collection<MaterialDTO> { });
            retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
            factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id)).Returns(new List<FullBoe>() { boe });
            
            var sut = new BOESummary(TravelTripCostCalculator.Object, rmsTripCalculate.Object);
            BOEExportInputs exportInputs = new BOEExportInputs(boe, workspace);
            ICollection<BOESummaryGridModelView> views = sut.GetBOESummaryGridModelViews(boe, exportInputs, false); 
            Assert.IsTrue(views.Count > 0, "No view returned");
            Assert.IsTrue(views.Count(x => x.LaborType == "ODC") == 1, "LaborTypeMismatch");
            Assert.IsTrue(views.Count(x => x.LaborType == "Travel") == 1, "LaborTypeMismatch");
            Assert.IsTrue(views.Count(x => x.ModeID == MSTTravelMode.ZoneAirfare) == 1);
            Assert.IsTrue(views.Count(x => x.ModeID == MSTTravelMode.NonZoneDomestic) == 0);  // Non-Zone is bundled under Travel LaborType (with modeId set to None)
            Assert.IsTrue(views.Count(x => x.ModeID == MSTTravelMode.ZoneNoAirfare) == 1);
            Assert.IsTrue(views.Where(x => x.ModeID == MSTTravelMode.ZoneNoAirfare).First().RollupCount == 999);
            Assert.IsTrue(views.Where(x => x.LaborType == "Travel").First().TotalCost == 1070);
        }
    }
}












