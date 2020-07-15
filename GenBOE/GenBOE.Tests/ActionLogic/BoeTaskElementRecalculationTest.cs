// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic;
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

    //ToDo: Fix these once the refactor is done

    [TestClass]
    public class BoeTaskElementRecalculationTest : MOQObject
    {
        Mock<IRetriever> retriever = new Mock<IRetriever>();
        
        private Mock<ICommonDataMapper> _CommonDataMapper = new Mock<ICommonDataMapper>();

        [TestInitialize]
        new public void Setup()
        {
        }
                
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void BL_RecalculateLaborWithTaskVariable()
        {
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = this.Boe1.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLabor = new ResourceTypeDto {
                SpreadType = IES.Common.SpreadType.Hours, 
                BoeID = this.Boe1.Id, 
                Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, 
                ValueSpread = 25000, 
                PercentSpread = 77, 
                PerformingOrgID = this.Perforg.Id, 
                ResourceID = this.Resource.Id, 
                StartDateValue = Convert.ToDateTime("04/20/2011"), 
                EndDateValue = Convert.ToDateTime("04/20/2011"), 
                LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { 
                Id = 2, 
                BoeID = this.Boe1.Id, 
                MOQHoursEquation = "15000 + BLAH + 2", 
                LaborTypeWarningFlag = false, 
                taskElementLabors = new Collection<ResourceTypeDto> { boeLabor } };
            
            OrdinaryVariableDto taskOrdinaryVar = new OrdinaryVariableDto { BoeID = this.Boe1.Id, Id = 2, OrdinaryVariableName = "BLAH", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = null, CLINID = this.Clin1.Id, WBSID = null } }, TaskElementId = boeTaskElement.Id };
            boeTaskElement.OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVar };

            var _ResourceLoader = new Mock<IResourceDTODataLoader>();
            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>(); 
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            Collection<int> SumVariableResourceTypes = new Collection<int> { 
                (int)SumVariableResourceType.DSLabor, 
                (int)SumVariableResourceType.LSLabor, 
                (int)SumVariableResourceType.TSLabor };

            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
            Mock<IPermissionsDTODataLoader> permLoader = new Mock<IPermissionsDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);
            FullClin fullClin1 = new FullClin(Clin1);
            factory.Setup(x => x.CreateFullClin(fullClin1)).Returns(new FullClin(Clin1));
            retriever.Setup(x => x.GetFullBoesByClinId(this.Clin1.Id)).Returns(new Collection<FullBoe>());
            retriever.Setup(x => x.GetClinById(this.Clin1.Id)).Returns(this.Clin1);
            retriever.Setup(x => x.GetTaskVariableIdsByClinId(this.Clin1.Id)).Returns(new Collection<int> { });

            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new List<FullWbs>() { new FullWbs(Wbs) });
            retriever.Setup(x => x.GetTaskVariablesByWbsId(Wbs.Id)).Returns(new List<int>());
            retriever.Setup(x => x.GetTaskVariableIdsByBoeId(this.Boe1.Id)).Returns(new List<int>());
            retriever.Setup(x => x.GetWorkspaceVariablesByWbsId(Wbs.Id)).Returns(new List<int>());
            retriever.Setup(x => x.GetBoesWithNestingByWbs(Wbs.Id)).Returns(new List<FullBoe>() { new FullBoe(this.Boe1) });
            retriever.Setup(x => x.GetWbsById(this.Wbs.Id)).Returns(this.Wbs);
            retriever.Setup(x => x.GetFullWorkspaceById(this.Workspace.Id)).Returns(new FullWorkspace(this.Workspace));
            retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<WorkspaceVariableDTO>());
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(this.Workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullBoe>() { new FullBoe(this.Boe1) });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(this.Workspace.Id)).Returns(new List<FullWbs>() { new FullWbs(Wbs) });
            retriever.Setup(x => x.GetClinsByWorkspaceId(this.Workspace.Id)).Returns(new Collection<FullClin>() { fullClin1 });
            retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO>() { this.Resource });

            FullWorkspace ws = new FullWorkspace(this.Workspace);
            retriever.Setup(x => x.GetFullWorkspaceById(this.Workspace.Id)).Returns(new FullWorkspace(this.Workspace));
            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnCLINID(this.Clin1.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(0); //???

            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            #region Setup Containers & mappers for the parser data grab

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });


            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

            #endregion

            Collection<BoeTaskElementDTO> output = sut.RecalculateLaborWithVariable(taskOrdinaryVar.Id, VariableType.Task, ws, new Collection<BoeTaskElementDTO>() {boeTaskElement} );
            Assert.AreEqual(output.Count, 1, "None or more than 1 task element was found");
            Assert.AreEqual(output[0].LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");
        }
         


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_RecalculateLaborWithWBS_TaskVariableTest()
        {
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = this.Boe1.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = this.Boe1.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = this.Boe1.Id, MOQHoursEquation = "15000 + BLAH + 2", LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor } };
            OrdinaryVariableDto taskOrdinaryVar = new OrdinaryVariableDto { BoeID = this.Boe1.Id, Id = 2, OrdinaryVariableName = "BLAH", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = null, CLINID = null, WBSID = this.Wbs.Id } }, TaskElementId = boeTaskElement.Id };
            boeTaskElement.OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVar };

            var OrdinaryTaskElementLoader = new Mock<IOrdinaryVariableLoader>();
            OrdinaryTaskElementLoader.Setup(x => x.GetById(taskOrdinaryVar.Id)).Returns(taskOrdinaryVar);

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };

            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();
            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnWBSID(this.Wbs.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(0);

            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

            List<BoeTaskElementDTO> output = sut.RecalculateLaborWithWBS((FullWbs)this.Wbs, VariableType.Task, ws.Object);
            Assert.AreEqual(output.Count, 1, "None or more than 1 task element was found");
            Assert.AreEqual(output[0].LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Collections.Generic.Dictionary`2<System.Int32,System.Collections.Generic.ICollection`1<System.Int32>>")]
        public void BL_RecalculateLaborWithCLIN_TaskVariableTest()
        {
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            ClinDTO clin = new ClinDTO {Id=1, WorkspaceID=this.Workspace.Id };
            OrdinaryVariableDto taskOrdinaryVar = new OrdinaryVariableDto { BoeID = this.Boe1.Id, Id = 2, OrdinaryVariableName = "BLAH", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = null, CLINID = clin.Id, WBSID = null } } };
            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = this.Boe1.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = this.Boe1.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = this.Boe1.Id, MOQHoursEquation = "15000 + BLAH + 2", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVar }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor }, TaskElementType = TaskElementType.Labor };
            taskOrdinaryVar.TaskElementId = boeTaskElement.Id;

            var OrdinaryTaskElementLoader = new Mock<IOrdinaryVariableLoader>();
            OrdinaryTaskElementLoader.Setup(x => x.GetById(taskOrdinaryVar.Id)).Returns(taskOrdinaryVar);

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };

            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();
            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnCLINID(clin.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(0); //???

            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);


            List<BoeTaskElementDTO> output = sut.RecalculateLaborWithClin(new FullClin(clin), VariableType.Task, ws.Object);
            Assert.AreEqual(output.Count, 1, "None or more than 1 task element was found");
            Assert.AreEqual(output[0].LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Collections.Generic.Dictionary`2<System.Int32,System.Collections.Generic.ICollection`1<System.Int32>>")]
        public void BL_RecalculateLaborWithBOE_TaskVariableTest()
        {

            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id };
            BoeDTO boe2 = new BoeDTO {Id=2 };
            OrdinaryVariableDto taskOrdinaryVar = new OrdinaryVariableDto { BoeID = this.Boe1.Id, Id = 2, OrdinaryVariableName = "BLAH", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };
            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "15000 + BLAH + 2", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVar }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor }, TaskElementType = TaskElementType.Labor };
            taskOrdinaryVar.TaskElementId = boeTaskElement.Id;

            var OrdinaryTaskElementLoader = new Mock<IOrdinaryVariableLoader>();
            OrdinaryTaskElementLoader.Setup(x => x.GetById(taskOrdinaryVar.Id)).Returns(taskOrdinaryVar);

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };
            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();

            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(900);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

            Collection<BoeTaskElementDTO> output = sut.RecalculateLaborWithBoe(new FullBoe(boe), VariableType.Task, ws.Object);
            Assert.AreEqual(output.Count, 1, "None or more than 1 task element was found");
            Assert.AreEqual(output[0].LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");
        }

        public void BL_RecalculateLaborWithTaskElement_TaskVariableTest()
        {
            BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 };
            OrdinaryVariableDto taskOrdinaryVar = new OrdinaryVariableDto { BoeID = this.Boe1.Id, Id = 2, OrdinaryVariableName = "BLAH", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };
            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "15000 + BLAH + 2", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVar }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor }, TaskElementType = TaskElementType.Labor };
            taskOrdinaryVar.TaskElementId = boeTaskElement.Id;

            var OrdinaryTaskElementLoader = new Mock<IOrdinaryVariableLoader>();
            OrdinaryTaskElementLoader.Setup(x => x.GetById(taskOrdinaryVar.Id)).Returns(taskOrdinaryVar);

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };
            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();

            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(900);
            VariableSelectBoeToSum.Setup(x => x.GetTaskVarLabelTotal(taskOrdinaryVar, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(taskOrdinaryVar.OrdinaryVariableValue.Value);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);
            BoeTaskElementDTO output = sut.RecalculateLaborWithTaskElement(boeTaskElement, VariableType.Task, ws.Object);

            Assert.AreEqual(output.LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");

            var spreads = from t in output.taskElementLabors
                          from s in t.LaborSpreads
                          select s;

            var LTHourSpreadTotal = output.taskElementLabors.Select(t => t.ValueSpread).Sum();
            Assert.IsTrue(spreads.Count() == 1, "One LS did not come back");
            Assert.IsTrue(output.taskElementLabors.Count() == 1, "One LT did not come back");

            int HourSpread = Convert.ToInt32(Math.Round((Convert.ToInt32(15502) * Convert.ToDecimal(77) / 100)));

            Assert.AreEqual(Convert.ToInt32(LTHourSpreadTotal), HourSpread, "The Hour Spread was not correct");
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_TaskVariableRecursion_TaskVariableTest()
        {
            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();

            // This test case will test task variable recurison
            // The global boe will not contain any task variables, but the BOE's CLIN and WBS's will be in use in other task elements.

            ClinDTO clin = new ClinDTO { Id = 1, WorkspaceID = this.Workspace.Id };

            BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id, CLINID = clin.Id, WBSID = this.Wbs.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 }; // the boe associated with the global boe's clin
            BoeDTO boe3 = new BoeDTO { Id = 3 }; // the boe associated with the global boe's wbs

            // The global BOE will not have an oridnary
             ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = this.Boe1.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
             ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = this.Boe1.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = this.Boe1.Id, MOQHoursEquation = "15002", LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor } };

            // task elements associated with global boe's clin
            OrdinaryVariableDto taskOrdinaryVarClin = new OrdinaryVariableDto { BoeID = boe2.Id, Id = 3, OrdinaryVariableName = "BLAH", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.CLIN, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = null, CLINID = clin.Id, WBSID = null } } };
            ResourceSpreadDto boeLSClin = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLaborClin = new ResourceTypeDto { BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSClin } };
            BoeTaskElementDTO boeTaskElementClin = new BoeTaskElementDTO { Id = 3, BoeID = boe2.Id, MOQHoursEquation = "25000 + BLAH + 2", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVarClin }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborClin } };
            taskOrdinaryVarClin.TaskElementId = boeTaskElementClin.Id;

            // task elements associated with global boe's WBS
            OrdinaryVariableDto taskOrdinaryVarWBS = new OrdinaryVariableDto { BoeID = boe3.Id, Id = 4, OrdinaryVariableName = "BAGS", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = null, CLINID = null, WBSID = this.Wbs.Id } } };
            ResourceSpreadDto boeLSWBS = new ResourceSpreadDto { BoeID = boe3.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLaborWBS = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe3.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve15, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSWBS } };
            BoeTaskElementDTO boeTaskElementWBS = new BoeTaskElementDTO { Id = 4, BoeID = boe3.Id, MOQHoursEquation = "25000 + BAGS + 2", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVarWBS }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborWBS } };
            taskOrdinaryVarWBS.TaskElementId = boeTaskElementWBS.Id;

            var OrdinaryTaskElementLoader = new Mock<IOrdinaryVariableLoader>();
            OrdinaryTaskElementLoader.Setup(x => x.GetById(taskOrdinaryVarClin.Id)).Returns(taskOrdinaryVarClin);
            OrdinaryTaskElementLoader.Setup(x => x.GetById(taskOrdinaryVarWBS.Id)).Returns(taskOrdinaryVarWBS);

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElementClin.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElementClin);
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElementWBS.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElementWBS);

            var _ResourceLoader = new Mock<IResourceDTODataLoader>();
            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };
            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(900);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            #region Setup Containers & mappers for the parser data grab

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe2.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe3.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());


            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

            #endregion

            //Make call
            Collection<BoeTaskElementDTO> output = sut.RecalculateLaborWithBoe(new FullBoe(boe), VariableType.Task, ws.Object);

            // Assert
            Assert.AreEqual(output.Count, 2, "None or more than 2 task elements was found");
            Assert.AreEqual(output[0].MOQHoursEquation, boeTaskElementClin.MOQHoursEquation, "The return task element didn't match the CLIN task element");
            Assert.AreEqual(output[1].MOQHoursEquation, boeTaskElementWBS.MOQHoursEquation, "The return task element didn't match the WBS task element");
        }

        public void BL_RecalculateLaborWithTaskElementContainsTaskAndWorkspaceVar()
        {
            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();

            ////Collection<int> TaskVarIDs = new Collection<int> { 2 };
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 };
            OrdinaryVariableDto taskOrdinaryVar = new OrdinaryVariableDto { BoeID = this.Boe1.Id, Id = 2, OrdinaryVariableName = "BLAH", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO {WorkspaceID = this.Workspace.Id, Id=3, WorkspaceVariableName="BAGS", WorkspaceVariableValue=200m, ValueType=VarValueType.Discrete};
            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "15000 + BLAH + 2 + BAGS", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVar }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor }, WorkspaceVariableIDs = new Collection<int> { workspaceVar.Id }, TaskElementType = TaskElementType.Labor };
            taskOrdinaryVar.TaskElementId = boeTaskElement.Id;

            var OrdinaryTaskElementLoader = new Mock<IOrdinaryVariableLoader>();
            OrdinaryTaskElementLoader.Setup(x => x.GetById(taskOrdinaryVar.Id)).Returns(taskOrdinaryVar);

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);
            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };

            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(900);
            VariableSelectBoeToSum.Setup(x => x.GetTaskVarLabelTotal(taskOrdinaryVar, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(taskOrdinaryVar.OrdinaryVariableValue.Value);
            VariableSelectBoeToSum.Setup(x => x.GetWorkspaceVarLabelTotal(workspaceVar, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(workspaceVar.WorkspaceVariableValue);

            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IBoeTaskElementDTODataLoader), BoeTaskElementLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);
            BoeTaskElementDTO output = sut.RecalculateLaborWithTaskElement(boeTaskElement, VariableType.Task, ws.Object);

            Assert.AreEqual(output.LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");

            var spreads = from t in output.taskElementLabors
                          from s in t.LaborSpreads
                          select s;

            var LTHourSpreadTotal = output.taskElementLabors.Select(t => t.ValueSpread).Sum();
            Assert.IsTrue(spreads.Count() == 1, "One LS did not come back");
            Assert.IsTrue(output.taskElementLabors.Count() == 1, "One LT did not come back");

            int HourSpread = Convert.ToInt32(Math.Round((Convert.ToInt32(15702) * Convert.ToDecimal(77) / 100)));

            Assert.AreEqual(Convert.ToInt32(LTHourSpreadTotal), HourSpread, "The Hour Spread was not correct");

        }

        public void BL_RecalculateLaborWithWBSParent()
        {
            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();

            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            OrdinaryVariableDto taskOrdinaryVar = new OrdinaryVariableDto { BoeID = this.Boe1.Id, Id = 2, OrdinaryVariableName = "BLAH", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = null, CLINID = null, WBSID = this.Wbs.Id } } };
            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = this.Boe1.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = this.Boe1.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = this.Boe1.Id, MOQHoursEquation = "15000 + BLAH + 2", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVar }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor } };
            taskOrdinaryVar.TaskElementId = boeTaskElement.Id;

            var OrdinaryTaskElementLoader = new Mock<IOrdinaryVariableLoader>();
            OrdinaryTaskElementLoader.Setup(x => x.GetById(taskOrdinaryVar.Id)).Returns(taskOrdinaryVar);

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };

            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnWBSID(this.Wbs.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(0);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();

                IDictionary<int, ICollection<int>> aDict = new Dictionary<int, ICollection<int>>();
                aDict.Add(this.Wbs.Id, new Collection<int>() { this.Boe1.Id });

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

            List<BoeTaskElementDTO> output = sut.RecalculateLaborWithWBS((FullWbs)this.Wbs, VariableType.Task, ws.Object);
            Assert.AreEqual(output.Count, 1, "None or more than 1 task element was found");
            Assert.AreEqual(output[0].LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");
        }

        public void BL_RecalculateLaborWithWBS_WorkspaceVariableTest()
        {
            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();

            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 2, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = null, CLINID = null, WBSID = this.Wbs.Id } } };
            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = this.Boe1.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = this.Boe1.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = this.Boe1.Id, MOQHoursEquation = "15000 + <WSVAR:2> + 2", WorkspaceVariableIDs = new Collection<int> { workspaceVar.Id }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor } };


            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);
            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };

            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);


            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnWBSID(this.Wbs.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(0);

            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);
            List<BoeTaskElementDTO> output = sut.RecalculateLaborWithWBS((FullWbs)this.Wbs, VariableType.Workspace, ws.Object);
            Assert.AreEqual(output.Count, 1, "None or more than 1 task element was found");
            Assert.AreEqual(output[0].LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");
        }

        public void BL_RecalculateLaborWithCLIN_WorkspaceVariableTest()
        {
            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();

            Collection<int> WorkspaceVarIDs = new Collection<int> { 2 };
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            ClinDTO clin = new ClinDTO { Id = 1, WorkspaceID = this.Workspace.Id };
           
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 2, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = null, CLINID = clin.Id, WBSID = null } } };
           
             ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = this.Boe1.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
             ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = this.Boe1.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = this.Boe1.Id, MOQHoursEquation = "15000 + <WSVAR:2> + 2", WorkspaceVariableIDs = WorkspaceVarIDs, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor }, TaskElementType = TaskElementType.Labor };


            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };

            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnCLINID(clin.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(0);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            #region Setup Containers & mappers for the parser data grab

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            IDictionary<int, ICollection<int>> aDict = new Dictionary<int, ICollection<int>>();
            aDict.Add(this.Wbs.Id, new Collection<int>() { this.Boe1.Id });

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

            #endregion

            List<BoeTaskElementDTO> output = sut.RecalculateLaborWithClin(new FullClin(clin), VariableType.Workspace, ws.Object);
            Assert.AreEqual(output.Count, 1, "None or more than 1 task element was found");
            Assert.AreEqual(output[0].LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Collections.Generic.Dictionary`2<System.Int32,System.Collections.Generic.ICollection`1<System.Int32>>")]
        public void BL_RecalculateLaborWithBOE_WorkspaceVariableTest()
        {
            Collection<int> WorkspaceVarIDs = new Collection<int> { 2 };
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 };
          
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 2, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };
            
             ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
             ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "15000 + <WSVAR:2>  + 2", WorkspaceVariableIDs = WorkspaceVarIDs, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor }, TaskElementType = TaskElementType.Labor };


            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };

            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();
            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(900);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);
            

            Collection<BoeTaskElementDTO> output = sut.RecalculateLaborWithBoe(new FullBoe(boe), VariableType.Workspace, ws.Object);
            Assert.AreEqual(output.Count, 1, "None or more than 1 task element was found");
            Assert.AreEqual(output[0].LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");
        }

        public void BL_RecalculateLaborWithTaskElement_WorkspaceVariableTest()
        {
            Collection<int> WorkspaceVarIDs = new Collection<int> { 2 };
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 };
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 2, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };
           
            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "15000 + <WSVAR:2> + 2", WorkspaceVariableIDs = WorkspaceVarIDs, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor }, TaskElementType = TaskElementType.Labor };


            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };

            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();
            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(900);
            VariableSelectBoeToSum.Setup(x => x.GetWorkspaceVarLabelTotal(workspaceVar, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(workspaceVar.WorkspaceVariableValue);
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
          
            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);


            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);
            BoeTaskElementDTO output = sut.RecalculateLaborWithTaskElement(boeTaskElement, VariableType.Workspace, ws.Object);

            Assert.AreEqual(output.LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");

            var spreads = from t in output.taskElementLabors
                          from s in t.LaborSpreads
                          select s;

            var LTHourSpreadTotal = output.taskElementLabors.Select(t => t.ValueSpread).Sum();
            Assert.IsTrue(spreads.Count() == 1, "One LS did not come back");
            Assert.IsTrue(output.taskElementLabors.Count() == 1, "One LT did not come back");

            int HourSpread = Convert.ToInt32(Math.Round((Convert.ToInt32(15502) * Convert.ToDecimal(77) / 100)));

            Assert.AreEqual(Convert.ToInt32(LTHourSpreadTotal), HourSpread, "The Hour Spread was not correct");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_TaskVariableRecursion_WorkspaceVariableTest()
        {
            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();

            // This test case will test workspace variable recurison
            // The global boe will not contain any workspace variables, but the BOE's CLIN and WBS's will be in use in other task elements.

             Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);

            ClinDTO clin = new ClinDTO { Id = 1, WorkspaceID = this.Workspace.Id };

            BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id, CLINID = clin.Id, WBSID = this.Wbs.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 }; // the boe associated with the global boe's clin
            BoeDTO boe3 = new BoeDTO { Id = 3 }; // the boe associated with the global boe's wbs

            // The global BOE will not have an workspace variable
            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = this.Boe1.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = this.Boe1.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = this.Boe1.Id, MOQHoursEquation = "15002", LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor } };

            // task elements associated with global boe's clin
     
            WorkspaceVariableDTO workspaceVarClin = new WorkspaceVariableDTO { Id = 3, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.CLIN, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum{BoeID=null, CLINID=clin.Id, WBSID=null}} };
            ResourceSpreadDto boeLSClin = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLaborClin = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSClin } };
            BoeTaskElementDTO boeTaskElementClin = new BoeTaskElementDTO { Id = 3, BoeID = boe2.Id, MOQHoursEquation = "25000 + <WSVAR:3> + 2", WorkspaceVariableIDs = new Collection<int> { 3}, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborClin } };

            // task elements associated with global boe's WBS
            WorkspaceVariableDTO workspaceVarWBS = new WorkspaceVariableDTO { Id = 4, WorkspaceVariableName = "BAGS", WorkspaceVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = null, CLINID = null, WBSID = this.Wbs.Id } } };
           
            ResourceSpreadDto boeLSWBS = new ResourceSpreadDto { BoeID = boe3.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLaborWBS = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe3.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve15, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSWBS } };
            BoeTaskElementDTO boeTaskElementWBS = new BoeTaskElementDTO { Id = 4, BoeID = boe3.Id, MOQHoursEquation = "25000 + <WSVAR:4>  + 2", WorkspaceVariableIDs = new Collection<int> {4 }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborWBS } };

            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe2.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe3.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElementClin.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElementClin);
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElementWBS.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElementWBS);

            var _ResourceLoader = new Mock<IResourceDTODataLoader>();
            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVarClin.Id)).Returns(workspaceVarClin);
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVarWBS.Id)).Returns(workspaceVarWBS);

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };

            VariableSelectBoeToSum.Setup(x => x.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(900);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            // Make call

            #region Setup Containers

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

            #endregion

            Collection<BoeTaskElementDTO> output = sut.RecalculateLaborWithBoe(new FullBoe(boe), VariableType.Workspace, ws.Object);

            // Assert
            Assert.AreEqual(output.Count, 2, "None or more than 2 task elements was found");
            Assert.AreEqual(output[0].MOQHoursEquation, boeTaskElementClin.MOQHoursEquation, "The return task element didn't match the CLIN task element");
            Assert.AreEqual(output[1].MOQHoursEquation, boeTaskElementWBS.MOQHoursEquation, "The return task element didn't match the WBS task element");
        }

        public void BL_RecalculateLaborWithTaskElement_WorkspaceVariableTestBug4925()
        {
            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();

            Collection<int> WorkspaceVarIDs = new Collection<int> { 2 };
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            BoeDTO boe2 = new BoeDTO { Id = 2 };
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 2, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = 51m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };

            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 51 };
            ResourceSpreadDto boeLS2 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 51 };
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 51, PercentSpread = 50, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            ResourceTypeDto boeLabor2 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 3, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = 51, PercentSpread = 50, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS2 } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "50 + <WSVAR:2>", WorkspaceVariableIDs = WorkspaceVarIDs, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor, boeLabor2 }, TaskElementType = TaskElementType.Labor };


            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            VariableSelectBoeToSum.Setup(x => x.GetWorkspaceVarLabelTotal(workspaceVar, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(workspaceVar.WorkspaceVariableValue);
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();
            IDictionary<int, ICollection<int>> aDict = new Dictionary<int, ICollection<int>>();
            aDict.Add(this.Wbs.Id, new Collection<int>() { this.Boe1.Id });

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

            BoeTaskElementDTO output = sut.RecalculateLaborWithTaskElement(boeTaskElement, VariableType.Workspace, ws.Object);

            Assert.AreEqual(output.LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");

            var spreads = from t in output.taskElementLabors
                          from s in t.LaborSpreads
                          select s;

            var LTHourSpreadTotal = output.taskElementLabors.Select(t => t.ValueSpread).Sum(); 
            Assert.IsTrue(spreads.Count() == 2, "One LS did not come back");
            Assert.IsTrue(output.taskElementLabors.Count() == 2, "One LT did not come back");

            int HourSpread = Convert.ToInt32(Math.Round((Convert.ToInt32(101) * Convert.ToDecimal(100) / 100)));

            Assert.AreEqual(Convert.ToInt32(LTHourSpreadTotal), HourSpread, "The Hour Spread was not correct");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        public void BL_RecalculateLaborWithTaskElement_WorkspaceVariableTestBug5520()
        {
            Collection<int> WorkspaceVarIDs = new Collection<int> { 2 };
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            ////BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 };
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 2, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = 21m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };

            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 21 };
            ResourceSpreadDto boeLS2 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 2, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 1 };
            ResourceSpreadDto boeLS3 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 3, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 1 };
            ResourceSpreadDto boeLS4 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 4, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 1 };
            ResourceSpreadDto boeLS5 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 5, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 1 };
            ResourceSpreadDto boeLS6 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 6, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 1 };
            ResourceSpreadDto boeLS7 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 7, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 1 };
            ResourceSpreadDto boeLS8 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 8, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 1 };
            ResourceSpreadDto boeLS9 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 9, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 1 };
            ResourceSpreadDto boeLS10 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 9, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 1 };

            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 21, PercentSpread = 82, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            ResourceTypeDto boeLabor2 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 3, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = 1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS2 } };
            ResourceTypeDto boeLabor3 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 4, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS3 } };
            ResourceTypeDto boeLabor4 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 5, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS4 } };
            ResourceTypeDto boeLabor5 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 6, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = 1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS5 } };
            ResourceTypeDto boeLabor6 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 7, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS6 } };
            ResourceTypeDto boeLabor7 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 8, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS7 } };
            ResourceTypeDto boeLabor8 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 9, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = 1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS8 } };
            ResourceTypeDto boeLabor9 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 10, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS9 } };
            ResourceTypeDto boeLabor10 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 11, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS10 } };

            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "4 + <WSVAR:2>", WorkspaceVariableIDs = WorkspaceVarIDs, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor, boeLabor2, boeLabor3, boeLabor4, boeLabor5, boeLabor6, boeLabor7, boeLabor8, boeLabor9, boeLabor10 }, TaskElementType = TaskElementType.Labor };


            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();
            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            VariableSelectBoeToSum.Setup(x => x.GetWorkspaceVarLabelTotal(workspaceVar, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(workspaceVar.WorkspaceVariableValue);
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();
            IDictionary<int, ICollection<int>> aDict = new Dictionary<int, ICollection<int>>();
            aDict.Add(this.Wbs.Id, new Collection<int>() { this.Boe1.Id });

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);


            BoeTaskElementDTO output = sut.RecalculateLaborWithTaskElement(boeTaskElement, VariableType.Workspace, ws.Object);

            Assert.AreEqual(output.LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");

            var spreads = from t in output.taskElementLabors
                          from s in t.LaborSpreads
                          select s;

            var LTHourSpreadTotal = output.taskElementLabors.Select(t => t.ValueSpread).Sum();
            Assert.IsTrue(spreads.Count() == 10, "One LS did not come back");
            Assert.IsTrue(output.taskElementLabors.Count() == 10, "One LT did not come back");
            Assert.IsTrue(output.taskElementLabors[0].ValueSpread == 21, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[1].ValueSpread == 1, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[2].ValueSpread == 1, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[3].ValueSpread == 1, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[4].ValueSpread == 1, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[5].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[6].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[7].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[8].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[9].ValueSpread == 0, "value spread not right");

            int HourSpread = Convert.ToInt32(Math.Round((Convert.ToInt32(25) * Convert.ToDecimal(100) / 100)));

            Assert.AreEqual(Convert.ToInt32(LTHourSpreadTotal), HourSpread, "The Hour Spread was not correct");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        public void BL_RecalculateLaborWithTaskElement_WorkspaceVariableTestBug5520_NegativeCase()
        {
            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();

            Collection<int> WorkspaceVarIDs = new Collection<int> { 2 };
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            ////BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 };
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 2, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = -21m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };

            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = -21 };
            ResourceSpreadDto boeLS2 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 2, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = -1 };
            ResourceSpreadDto boeLS3 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 3, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = -1 };
            ResourceSpreadDto boeLS4 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 4, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = -1 };
            ResourceSpreadDto boeLS5 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 5, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = -1 };
            ResourceSpreadDto boeLS6 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 6, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = -1 };
            ResourceSpreadDto boeLS7 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 7, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = -1 };
            ResourceSpreadDto boeLS8 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 8, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = -1 };
            ResourceSpreadDto boeLS9 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 9, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = -1 };
            ResourceSpreadDto boeLS10 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 9, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = -1 };

            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = -21, PercentSpread = 82, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, PercentSpreadLocked = false, HourSpreadLocked = false };
            ResourceTypeDto boeLabor2 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 3, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = -1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS2 }, PercentSpreadLocked = false, HourSpreadLocked = false };
            ResourceTypeDto boeLabor3 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 4, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = -1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS3 }, PercentSpreadLocked = false, HourSpreadLocked = false };
            ResourceTypeDto boeLabor4 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 5, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = -1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS4 }, PercentSpreadLocked = false, HourSpreadLocked = false };
            ResourceTypeDto boeLabor5 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 6, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = -1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS5 }, PercentSpreadLocked = false, HourSpreadLocked = false };
            ResourceTypeDto boeLabor6 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 7, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = -1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS6 }, PercentSpreadLocked = false, HourSpreadLocked = false };
            ResourceTypeDto boeLabor7 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 8, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = -1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS7 }, PercentSpreadLocked = false, HourSpreadLocked = false };
            ResourceTypeDto boeLabor8 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 9, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = -1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS8 }, PercentSpreadLocked = false, HourSpreadLocked = false };
            ResourceTypeDto boeLabor9 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 10, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = -1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS9 }, PercentSpreadLocked = false, HourSpreadLocked = false };
            ResourceTypeDto boeLabor10 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 11, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = -1, PercentSpread = 2, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS10 }, PercentSpreadLocked = false, HourSpreadLocked = false };

            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "-4 + <WSVAR:2>", WorkspaceVariableIDs = WorkspaceVarIDs, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor, boeLabor2, boeLabor3, boeLabor4, boeLabor5, boeLabor6, boeLabor7, boeLabor8, boeLabor9, boeLabor10 }, TaskElementType = TaskElementType.Labor };


            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            VariableSelectBoeToSum.Setup(x => x.GetWorkspaceVarLabelTotal(workspaceVar, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(workspaceVar.WorkspaceVariableValue);
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);


            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);
            BoeTaskElementDTO output = sut.RecalculateLaborWithTaskElement(boeTaskElement, VariableType.Workspace, ws.Object);

            Assert.AreEqual(output.LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");

            var spreads = from t in output.taskElementLabors
                          from s in t.LaborSpreads
                          select s;

            var LTHourSpreadTotal = output.taskElementLabors.Select(t => t.ValueSpread).Sum();
            Assert.IsTrue(spreads.Count() == 10, "One LS did not come back");
            Assert.IsTrue(output.taskElementLabors.Count() == 10, "One LT did not come back");
            Assert.IsTrue(output.taskElementLabors[0].ValueSpread == -21, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[1].ValueSpread == -1, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[2].ValueSpread == -1, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[3].ValueSpread == -1, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[4].ValueSpread == -1, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[5].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[6].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[7].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[8].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[9].ValueSpread == 0, "value spread not right");

            long HourSpread = Convert.ToInt64(Math.Round((Convert.ToInt64(-25) * Convert.ToDecimal(100) / 100)));

            Assert.AreEqual(Convert.ToInt64(LTHourSpreadTotal), HourSpread, "The Hour Spread was not correct");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        public void BL_RecalculateLaborWithTaskElement_WorkspaceVariableTestBug5520_NegativeCase2()
        {
            Collection<int> WorkspaceVarIDs = new Collection<int> { 2 };
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            ////BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 };
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 2, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = -23m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };

            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = -23 };
            ResourceSpreadDto boeLS2 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 2, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 0 };
            ResourceSpreadDto boeLS3 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 3, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 0 };
            ResourceSpreadDto boeLS4 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 4, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 0 };
            ResourceSpreadDto boeLS5 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 5, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 0 };
            ResourceSpreadDto boeLS6 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 6, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 0 };
            ResourceSpreadDto boeLS7 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 7, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 0 };
            ResourceSpreadDto boeLS8 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 8, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 0 };
            ResourceSpreadDto boeLS9 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 9, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 0 };
            ResourceSpreadDto boeLS10 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 10, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 0 };

            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = -23, PercentSpread = 91, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS } };
            ResourceTypeDto boeLabor2 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 3, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = 0, PercentSpread = 1, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS2 } };
            ResourceTypeDto boeLabor3 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 4, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 0, PercentSpread = 1, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS3 } };
            ResourceTypeDto boeLabor4 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 5, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 0, PercentSpread = 1, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS4 } };
            ResourceTypeDto boeLabor5 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 6, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = 0, PercentSpread = 1, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS5 } };
            ResourceTypeDto boeLabor6 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 7, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 0, PercentSpread = 1, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS6 } };
            ResourceTypeDto boeLabor7 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 8, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 0, PercentSpread = 1, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS7 } };
            ResourceTypeDto boeLabor8 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 9, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = 0, PercentSpread = 1, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS8 } };
            ResourceTypeDto boeLabor9 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 10, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 0, PercentSpread = 1, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS9 } };
            ResourceTypeDto boeLabor10 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 11, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 0, PercentSpread = 1, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS10 } };

            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "-2 + <WSVAR:2>", WorkspaceVariableIDs = WorkspaceVarIDs, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor, boeLabor2, boeLabor3, boeLabor4, boeLabor5, boeLabor6, boeLabor7, boeLabor8, boeLabor9, boeLabor10 }, TaskElementType = TaskElementType.Labor };


            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();
            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            VariableSelectBoeToSum.Setup(x => x.GetWorkspaceVarLabelTotal(workspaceVar, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(workspaceVar.WorkspaceVariableValue);
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);

            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);
            BoeTaskElementDTO output = sut.RecalculateLaborWithTaskElement(boeTaskElement, VariableType.Workspace, ws.Object);

            Assert.AreEqual(output.LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");

            var spreads = from t in output.taskElementLabors
                          from s in t.LaborSpreads
                          select s;

            var LTHourSpreadTotal = output.taskElementLabors.Select(t => t.ValueSpread).Sum();
            Assert.IsTrue(spreads.Count() == 10, "One LS did not come back");
            Assert.IsTrue(output.taskElementLabors.Count() == 10, "One LT did not come back");
            Assert.IsTrue(output.taskElementLabors[0].ValueSpread == -23, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[1].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[2].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[3].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[4].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[5].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[6].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[7].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[8].ValueSpread == 0, "value spread not right");
            Assert.IsTrue(output.taskElementLabors[9].ValueSpread == -2, "value spread not right");

            int HourSpread = Convert.ToInt32(Math.Round((Convert.ToInt32(-25) * Convert.ToDecimal(100) / 100)));

            Assert.AreEqual(Convert.ToInt32(LTHourSpreadTotal), HourSpread, "The Hour Spread was not correct");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_RecalculateLaborWithTaskElement_7266HourSpreadLocked()
        {
            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();

            Collection<int> WorkspaceVarIDs = new Collection<int> { 2 };
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            ////BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 };
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 2, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = 59m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };

            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 51 };
            ResourceSpreadDto boeLS2 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 51 };
            var OriginalPercentSpread = 50;
            var NonLockedValue = 51;
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = NonLockedValue, PercentSpread = OriginalPercentSpread, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, PercentSpreadLocked = true, HourSpreadLocked = false };
            ResourceTypeDto boeLabor2 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 3, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = 51, PercentSpread = OriginalPercentSpread, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS2 }, PercentSpreadLocked = false, HourSpreadLocked = true };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "50 + <WSVAR:2>", WorkspaceVariableIDs = WorkspaceVarIDs, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor, boeLabor2 }, TaskElementType = TaskElementType.Labor };


            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);
 
            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            VariableSelectBoeToSum.Setup(x => x.GetWorkspaceVarLabelTotal(workspaceVar, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(workspaceVar.WorkspaceVariableValue);
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();
            IDictionary<int, ICollection<int>> aDict = new Dictionary<int, ICollection<int>>();
            aDict.Add(this.Wbs.Id, new Collection<int>() { this.Boe1.Id });


            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

            BoeTaskElementDTO output = sut.RecalculateLaborWithTaskElement(boeTaskElement, VariableType.Workspace, ws.Object);

            Assert.AreEqual(output.LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");

            var spreads = from t in output.taskElementLabors
                          from s in t.LaborSpreads
                          select s;

            //moq total will be 109
            // to begin with
            // 1 LT is 51 value, percent spread locked
            // 1 LT is 51 value, hour spread locked
            // so 51 will remain for the second LT, and 55 will be changed for the first one
            // no smoothing to occur
            var LTHourSpreadTotal = output.taskElementLabors.Select(t => t.ValueSpread).Sum();
            Assert.IsTrue(spreads.Count() == 2, "One LS did not come back");
            Assert.IsTrue(output.taskElementLabors.Count() == 2, "One LT did not come back");
            Assert.IsTrue(output.taskElementLabors.Where(x => x.Id == 3).Select(x => x.ValueSpread).First() == boeLabor2.ValueSpread, "The Value Spread changed which is not right");
            Assert.IsTrue(output.taskElementLabors.Where(x => x.Id == 3).Select(x => x.PercentSpread).First() != OriginalPercentSpread, "The Percent Spread did not change which is incorrect");
            Assert.IsTrue(output.taskElementLabors.Where(x => x.Id == 2).Select(x => x.ValueSpread).First() != NonLockedValue, "The Value Spread stayed the same which is not correct");
            Assert.IsTrue(output.taskElementLabors.Where(x => x.Id == 2).Select(x => x.PercentSpread).First() == OriginalPercentSpread, "The Percent Spread changed which is not correct");

            int HourSpread = Convert.ToInt32(Math.Round((Convert.ToInt32(109) * Convert.ToDecimal(100) / 100)));

            // since the spreads changed with the locked values, the expected today of 109 and what the actual total is should not match
            Assert.AreNotEqual(Convert.ToInt32(LTHourSpreadTotal), HourSpread, "The Hour Spread was not correct");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void BL_RecalculateLaborWithTaskElement_Int64HourSpreadLocked()
        {
            Collection<int> WorkspaceVarIDs = new Collection<int> { 2 };
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            ////BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 };
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 2, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = 900000059m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };

            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 51 };
            ResourceSpreadDto boeLS2 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 51 };
            var OriginalPercentSpread = 50;
            var NonLockedValue = 51;
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = NonLockedValue, PercentSpread = OriginalPercentSpread, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, PercentSpreadLocked = true, HourSpreadLocked = false };
            ResourceTypeDto boeLabor2 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 3, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = 51, PercentSpread = OriginalPercentSpread, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS2 }, PercentSpreadLocked = false, HourSpreadLocked = true };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "9000000050 + <WSVAR:2>", WorkspaceVariableIDs = WorkspaceVarIDs, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor, boeLabor2 }, TaskElementType = TaskElementType.Labor };


            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);

            Mock<FullWorkspace> ws = new Mock<FullWorkspace>();
            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            VariableSelectBoeToSum.Setup(x => x.GetWorkspaceVarLabelTotal(workspaceVar, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(workspaceVar.WorkspaceVariableValue);
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();
            IDictionary<int, ICollection<int>> aDict = new Dictionary<int, ICollection<int>>();
            aDict.Add(this.Wbs.Id, new Collection<int>() { this.Boe1.Id });

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);


            BoeTaskElementDTO output = sut.RecalculateLaborWithTaskElement(boeTaskElement, VariableType.Workspace, ws.Object);

            Assert.AreEqual(output.LaborTypeWarningFlag, true, "The LT Warning flag was not converted to true");

            var spreads = from t in output.taskElementLabors
                          from s in t.LaborSpreads
                          select s;

            //moq total will be 9900000109
            // to begin with
            // 1 LT is 51 value, percent spread locked
            // 1 LT is 51 value, hour spread locked
            // so 51 will remain for the second LT, and 55 will be changed for the first one
            // no smoothing to occur
            var LTHourSpreadTotal = output.taskElementLabors.Select(t => t.ValueSpread).Sum();
            Assert.IsTrue(spreads.Count() == 2, "One LS did not come back");
            Assert.IsTrue(output.taskElementLabors.Count() == 2, "One LT did not come back");
            Assert.IsTrue(output.taskElementLabors.Where(x => x.Id == 3).Select(x => x.ValueSpread).First() == boeLabor2.ValueSpread, "The Value Spread changed which is not right");
            Assert.IsTrue(output.taskElementLabors.Where(x => x.Id == 3).Select(x => x.PercentSpread).First() != OriginalPercentSpread, "The Percent Spread did not change which is incorrect");
            Assert.IsTrue(output.taskElementLabors.Where(x => x.Id == 2).Select(x => x.ValueSpread).First() != NonLockedValue, "The Value Spread stayed the same which is not correct");
            Assert.IsTrue(output.taskElementLabors.Where(x => x.Id == 2).Select(x => x.PercentSpread).First() == OriginalPercentSpread, "The Percent Spread changed which is not correct");

            long HourSpread = Convert.ToInt64(Math.Round((Convert.ToInt64(9900000109) * Convert.ToDecimal(100) / 100)));

            // since the spreads changed with the locked values, the expected today of 9900000109 and what the actual total is should not match
            Assert.AreNotEqual(Convert.ToInt64(LTHourSpreadTotal), HourSpread, "The Hour Spread was not correct");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Collections.Generic.Dictionary`2<System.Int32,System.Collections.Generic.ICollection`1<System.Int32>>")]
        public void BL_CalculateMOQHoursTotal()
        {
            Collection<int> WorkspaceVarIDs = new Collection<int> { 2 };
            Collection<int> TaskElementIDs = new Collection<int>();
            int taskID2 = 2;
            TaskElementIDs.Add(taskID2);
            BoeDTO boe = new BoeDTO { Id = this.Boe1.Id, WorkspaceID = this.Workspace.Id };
            BoeDTO boe2 = new BoeDTO { Id = 2 };
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 2, WorkspaceVariableName = "BLAH", WorkspaceVariableValue = 59m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = this.Boe1.Id, CLINID = null, WBSID = null } } };

            ResourceSpreadDto boeLS = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 51 };
            ResourceSpreadDto boeLS2 = new ResourceSpreadDto { BoeID = boe2.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("05/20/2011"), LaborSpreadValue = 51 };
            var OriginalPercentSpread = 50;
            var NonLockedValue = 51;
            ResourceTypeDto boeLabor = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 2, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = NonLockedValue, PercentSpread = OriginalPercentSpread, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, PercentSpreadLocked = true, HourSpreadLocked = false };
            ResourceTypeDto boeLabor2 = new ResourceTypeDto { SpreadType = IES.Common.SpreadType.Hours, BoeID = boe2.Id, Id = 3, SpreadCurveID = SpreadCurves.SpreadCurve16, ValueSpread = 51, PercentSpread = OriginalPercentSpread, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("05/20/2011"), EndDateValue = Convert.ToDateTime("05/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLS2 }, PercentSpreadLocked = false, HourSpreadLocked = true };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, MOQHoursEquation = "50 + <WSVAR:2>", WorkspaceVariableIDs = WorkspaceVarIDs, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor, boeLabor2 }, TaskElementType = TaskElementType.Labor };


            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });

            var BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            BoeTaskElementLoader.Setup(x => x.GetById(boeTaskElement.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTaskElement);

            var perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            var VariableSelectBoeToSum = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
            FullWorkspace ws = new FullWorkspace(this.Workspace);
            var WorkspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            VariableSelectBoeToSum.Setup(x => x.GetWorkspaceVarLabelTotal(workspaceVar, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(workspaceVar.WorkspaceVariableValue);
            WorkspaceVariableLoader.Setup(x => x.GetById(workspaceVar.Id)).Returns(workspaceVar);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var sut = new BoeTaskElementRecalculation(VariableSelectBoeToSum.Object, factory.Object);

            Mock<IResourceDTODataLoader> _ResourceLoader = new Mock<IResourceDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

            
            string output = sut.CalculateMOQHoursTotal(boeTaskElement, ws);

            Assert.IsTrue(String.Equals(output, "109"), "the moq hr total is incorrect");

        }
    }
}
