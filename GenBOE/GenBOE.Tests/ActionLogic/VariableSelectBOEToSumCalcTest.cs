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

    [TestClass]
    public class VariableSelectBOEToSumCalcTest
    {
        Mock<IRetriever> retriever = new Mock<IRetriever>();
        Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
        Mock<IResourceDTODataLoader> ResourceDM = new Mock<IResourceDTODataLoader>();
        Mock<IPerformingOrgDTODataLoader> PerfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
        Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
        Mock<IPermissionsDTODataLoader> permissionsLoader = new Mock<IPermissionsDTODataLoader>();

        [TestInitialize()]
        public void TestInitialize()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsLoader.Object);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void GetWorkspaceVarLabelTotal()
        {
            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };
            FullWorkspace ws = new FullWorkspace(workspace);
            this.retriever.Setup(x => x.GetFullWorkspaceById(workspace.Id)).Returns(ws);

            //setup clin
            BoeDTO boeClin = new BoeDTO { Id = 2, WorkspaceID = workspace.Id, CLINID = 1 };

            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.LS, ElementOfCost = ElementOfCostType.Sub };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "3ME2", ResourceDesc = "3ME2 - ES On Prem Mtn E2", SegRegion = "3M", LaborType = "E2", Segment = SegmentType.ES, ElementOfCost = ElementOfCostType.LMLabor };

            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            BoeTaskElementDTO boeTaskElementClin = new BoeTaskElementDTO { TotalHours = 6500, BoeID = boeClin.Id, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boeClin.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementClin });


            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { TotalHours = 6500, BoeID = boeClin.Id, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boeClin.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement });
            ClinDTO clin = new ClinDTO { Id = 1 };
            FullClin fullClin = new FullClin(clin);
            this.factory.Setup(x => x.CreateFullClin(clin)).Returns(fullClin);
            this.retriever.Setup(x => x.GetClinById(clin.Id)).Returns(clin);
            Collection<int> boeClinIDs = new Collection<int>();
            int id = boeClin.Id;
            boeClinIDs.Add(id);
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1, resource2 });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new List<FullWbs>());
            retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin> { fullClin });

            //setup boe
            ClinDTO clin2 = new ClinDTO { Id = 2 };
            FullClin fullClin2 = new FullClin(clin2);
            this.factory.Setup(x => x.CreateFullClin(clin2)).Returns(fullClin2);
            this.retriever.Setup(x => x.GetClinById(clin2.Id)).Returns(clin2);
            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = workspace.Id, CLINID = clin2.Id };
            BoeTaskElementDTO boeTaskElement2 = new BoeTaskElementDTO { TotalHours = 200, BoeID = boe.Id, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource2.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 200 } } } } };

            FullBoe boeObject = new FullBoe(boe);
            FullBoe boeClinObject = new FullBoe(boeClin);

            this.retriever.Setup(x => x.GetFullBoesByClinId(clin.Id)).Returns(new Collection<FullBoe> { boeClinObject });
            this.retriever.Setup(x => x.GetFullBoesByClinId(clin2.Id)).Returns(new Collection<FullBoe> { boeObject });

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObject, boeClinObject });
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementClin, boeTaskElement, boeTaskElement2 });

            //setup workspace variable with a selected BOE, clin, and a wbs ID
            Collection<SelectBOEsToSum> selectedBoes = new Collection<SelectBOEsToSum>();
            SelectBOEsToSum boeSum = new SelectBOEsToSum();
            boeSum.BoeID = boe.Id;
            selectedBoes.Add(boeSum);
            SelectBOEsToSum clinSum = new SelectBOEsToSum();
            clinSum.CLINID = clin.Id;
            selectedBoes.Add(clinSum);

            ResourceDM.Setup(x => x.GetByIds(new Collection<int> { resource1.Id })).Returns(new Collection<ResourceDTO> { resource1 });
            ResourceDM.Setup(x => x.GetByIds(new Collection<int> { resource2.Id })).Returns(new Collection<ResourceDTO> { resource2 });

            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 1, ValueType = VarValueType.SumOfBOEs, SelectedBOEsToSum = selectedBoes, SumVariableResourceTypeIDs = new Collection<int> { (int)SumVariableResourceType.LOESub, (int)SumVariableResourceType.ESLabor } };

			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation(); 
            data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

            decimal returnTotal = sut.GetWorkspaceVarLabelTotal(workspaceVar, data);
            Assert.AreEqual(returnTotal, 6500 + 6500 + 200, "The totals were not as expected");
        }

        [TestMethod]
        public void GetWorkspaceVarLabelTotalSpaceSystems()
        {
            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };
            FullWorkspace ws = new FullWorkspace(workspace);
            this.retriever.Setup(x => x.GetFullWorkspaceById(workspace.Id)).Returns(ws);

            //setup clin
            BoeDTO boeClin = new BoeDTO { Id = 2, WorkspaceID = workspace.Id, CLINID=1 };
            FullBoe boeObject = new FullBoe(boeClin);

            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.Sub };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "3ME2", ResourceDesc = "3ME2 - ES On Prem Mtn E2", SegRegion = "3M", LaborType = "E2", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };

            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            BoeTaskElementDTO boeTaskElementClin = new BoeTaskElementDTO { TotalHours = 6500, BoeID = boeClin.Id, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { TotalHours = 6500, BoeID = boeClin.Id, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            ClinDTO clin = new ClinDTO { Id = 1 };
            FullClin fullClin = new FullClin(clin);
            this.factory.Setup(x => x.CreateFullClin(clin)).Returns(fullClin);
            this.retriever.Setup(x => x.GetClinById(clin.Id)).Returns(clin);
            Collection<int> boeClinIDs = new Collection<int>();
            int id = boeClin.Id;
            boeClinIDs.Add(id);
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1, resource2 });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new List<FullWbs>());
            retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin> { fullClin });

            //setup boe
            ClinDTO clin2 = new ClinDTO { Id = 2 };
            FullClin fullClin2 = new FullClin(clin2);
            this.factory.Setup(x => x.CreateFullClin(clin2)).Returns(fullClin2);
            this.retriever.Setup(x => x.GetClinById(clin2.Id)).Returns(clin2);
            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = workspace.Id, CLINID = clin2.Id };
            FullBoe boeObject2 = new FullBoe(boe);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObject, boeObject2 });

            this.retriever.Setup(x => x.GetFullBoesByClinId(clin.Id)).Returns(new Collection<FullBoe> { boeObject });
            this.retriever.Setup(x => x.GetFullBoesByClinId(clin2.Id)).Returns(new Collection<FullBoe> { boeObject2 });

            BoeTaskElementDTO boeTaskElement2 = new BoeTaskElementDTO { TotalHours = 200, BoeID = boe.Id, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource2.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 200 } } } } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementClin, boeTaskElement, boeTaskElement2 });

            //setup workspace variable with a selected BOE, clin, and a wbs ID
            Collection<SelectBOEsToSum> selectedBoes = new Collection<SelectBOEsToSum>();
            SelectBOEsToSum boeSum = new SelectBOEsToSum();
            boeSum.BoeID = boe.Id;
            selectedBoes.Add(boeSum);
            SelectBOEsToSum clinSum = new SelectBOEsToSum();
            clinSum.CLINID = clin.Id;
            selectedBoes.Add(clinSum);

            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 1, ValueType = VarValueType.SumOfBOEs, SelectedBOEsToSum = selectedBoes, SumVariableResourceTypeIDs = new Collection<int> { (int)SumVariableResourceType.SSCLOESub, (int)SumVariableResourceType.SSCLMLabor } };

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

            decimal returnTotal = sut.GetWorkspaceVarLabelTotal(workspaceVar, data);
            Assert.AreEqual(returnTotal, 6500 + 6500 + 200, "The totals were not as expected");
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void GetWorkspaceVarLabelTotal_UpdateBOEtasks()
        {
            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 });
            //setup clin

            FullBoe boeClin = new FullBoe(new BoeDTO { Id = 2, WorkspaceID = workspace.Id, CLINID = 1 });
            this.retriever.Setup(x => x.GetFullWorkspaceById(workspace.Id)).Returns(workspace);

            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.LS, ElementOfCost = ElementOfCostType.Sub };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "3ME2", ResourceDesc = "3ME2 - ES On Prem Mtn E2", SegRegion = "3M", LaborType = "E2", Segment = SegmentType.ES, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource3 = new ResourceDTO { Id = 3, ResourceName = "3ME3", ResourceDesc = "3ME3 - ES On Prem Mtn E3", SegRegion = "3M", LaborType = "E3", Segment = SegmentType.ES, ElementOfCost = ElementOfCostType.LMLabor };

            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            BoeTaskElementDTO boeTaskElementClin = new BoeTaskElementDTO { TotalHours = 6500, BoeID = boeClin.Id, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { TotalHours = 6500, BoeID = boeClin.Id, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            ClinDTO clin = new ClinDTO { Id = 1 };
            FullClin fullClin = new FullClin(clin);
            this.factory.Setup(x => x.CreateFullClin(clin)).Returns(fullClin);
            this.retriever.Setup(x => x.GetClinById(clin.Id)).Returns(clin);
            Collection<int> boeClinIDs = new Collection<int>();
            int id = boeClin.Id;
            boeClinIDs.Add(id);

            //setup boe
            ClinDTO clin2 = new ClinDTO { Id = 2 };
            FullClin fullClin2 = new FullClin(clin2);
            this.factory.Setup(x => x.CreateFullClin(clin2)).Returns(fullClin2);
            this.retriever.Setup(x => x.GetClinById(clin2.Id)).Returns(clin2);
            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = workspace.Id, CLINID = clin2.Id };
            FullBoe boeObj = new FullBoe(boe);
            FullBoe boeObjClin = new FullBoe(boeClin);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObj, boeObjClin });

            this.retriever.Setup(x => x.GetFullBoesByClinId(clin.Id)).Returns(new Collection<FullBoe> { boeObjClin });
            this.retriever.Setup(x => x.GetFullBoesByClinId(clin2.Id)).Returns(new Collection<FullBoe> { boeObj });

            BoeTaskElementDTO boeTaskElement2 = new BoeTaskElementDTO { TotalHours = 200, BoeID = boe.Id, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { Id = 1, SpreadType = IES.Common.SpreadType.Hours, ResourceID = resource2.Id, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 200 } } } } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementClin, boeTaskElement, boeTaskElement2 });
            retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1, resource2, resource3 });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new List<FullWbs>());
            retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new Collection<FullClin> { fullClin });

            //setup workspace variable with a selected BOE, clin, and a wbs ID
            Collection<SelectBOEsToSum> selectedBoes = new Collection<SelectBOEsToSum>();
            SelectBOEsToSum boeSum = new SelectBOEsToSum();
            boeSum.BoeID = boe.Id;
            selectedBoes.Add(boeSum);
            SelectBOEsToSum clinSum = new SelectBOEsToSum();
            clinSum.CLINID = clin.Id;
            selectedBoes.Add(clinSum);

            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 1, ValueType = VarValueType.SumOfBOEs, SelectedBOEsToSum = selectedBoes, SumVariableResourceTypeIDs = new Collection<int> { (int)SumVariableResourceType.LOESub, (int)SumVariableResourceType.ESLabor } };

			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, workspace);

            boeTaskElement2.taskElementLabors[0].Updateable = UpdateType.Deleted;
            boeTaskElement2.taskElementLabors.Add(new ResourceTypeDto { ResourceID = resource3.Id, SpreadType = IES.Common.SpreadType.Hours, Updateable = UpdateType.Upsert, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 400 } } });
            data.UpdateBOETasks(boeTaskElement2.BoeID, new Collection<BoeTaskElementDTO> { boeTaskElement2 }, workspace);

            decimal returnTotal = sut.GetWorkspaceVarLabelTotal(workspaceVar, data);
            Assert.AreEqual(returnTotal, 6500 + 6500 + 400, "The totals were not as expected");
        }

        [TestMethod]
        public void GetTotalBasedOnBoeID()
        {
            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };
            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID=workspace.Id };

            FullWorkspace ws = new FullWorkspace(workspace);
            Collection<FullBoe> fullBoes = new Collection<FullBoe>() { new FullBoe(boe) };

            this.retriever.Setup(i => i.GetFullBoesByWorkspaceId(fullBoes[0].WorkspaceID, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(fullBoes);
            this.retriever.Setup(x => x.GetFullWorkspaceById(fullBoes[0].WorkspaceID)).Returns(ws);

            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.DS, ElementOfCost = ElementOfCostType.LMLabor };

            BoeTaskElementDTO boeTaskElement1 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement1 });
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1 });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new List<FullWbs>());
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.ESLabor, (int)SumVariableResourceType.LOEIWTA };

			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = 1 } } } }, null, ws);

            decimal returnTotal = sut.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, data);
            Assert.AreEqual(returnTotal, 6500, "The totals were not as expected");
        }

        [TestMethod]
        public void GetTotalBasedOnBoeIDSpaceSystems()
        {
            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };

            //setup boe
            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = workspace.Id };
            FullWorkspace ws = new FullWorkspace(workspace);
            FullBoe boeObj = new FullBoe(boe);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boeObj });
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws.Id)).Returns(ws);

            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };

            BoeTaskElementDTO boeTaskElement1 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement1 });
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1 });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new List<FullWbs>());
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.SSCLMLabor, (int)SumVariableResourceType.SSCLOEIWTA };

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = 1 } } } }, null, ws);

            decimal returnTotal = sut.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, data);
            Assert.AreEqual(returnTotal, 6500, "The totals were not as expected");
        }

        [TestMethod]
        public void GetTotalBasedOnWBSID()
        {
            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };

            //set up wbs
            FullWbs wbs = new FullWbs( new WbsDTO { Id = 1, WorkspaceID = 100, Level = 1 } );

            //setup boe
            Collection<BoeDTO> boes = new Collection<BoeDTO> {
                new BoeDTO { Id = 1, WBSID = wbs.Id, WorkspaceID=workspace.Id },
                new BoeDTO { Id = 2, WBSID = wbs.Id, WorkspaceID=workspace.Id }
            };
            FullWorkspace ws = new FullWorkspace(workspace);
            FullBoe boeObj = new FullBoe(boes[0]);
            FullBoe boeObj2 = new FullBoe(boes[1]);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boeObj, boeObj2 });
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws.Id)).Returns(ws);

            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.DS, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "3ME2", ResourceDesc = "3ME2 - ES On Prem Mtn E2", SegRegion = "3M", LaborType = "E2", Segment = SegmentType.ES, ElementOfCost = ElementOfCostType.IWTA };

            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            BoeTaskElementDTO boeTaskElement1 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            BoeTaskElementDTO boeTaskElement2 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource2.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 800 } } } } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement1, boeTaskElement2 });
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1, resource2 });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullWbs> { wbs });
            retriever.Setup(x => x.GetBoesWithNestingByWbs(wbs.Id)).Returns(new Collection<FullBoe>() { boeObj, boeObj2 });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());

			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.LOEIWTA };

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = 1, WBSID = 1 }, new SelectBOEsToSum() { BoeID = 2, WBSID = 1 } } } }, null, ws);

            decimal returnTotal = sut.GetTotalBasedOnWBSID(wbs.Id, SumVariableResourceTypes, data);
            Assert.AreEqual(returnTotal, 6500 + 800, "The totals were not as expected");
        }

        [TestMethod]
        public void GetTotalBasedOnWBSIDSpaceSystems()
        {
            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };

            //set up wbs
            FullWbs wbs = new FullWbs( new WbsDTO { Id = 1, WorkspaceID = 100, Level = 1 } );

            //setup boe
            Collection<BoeDTO> boes = new Collection<BoeDTO> {
                new BoeDTO { Id = 1, WBSID = wbs.Id, WorkspaceID=workspace.Id },
                new BoeDTO { Id = 2, WBSID = wbs.Id, WorkspaceID=workspace.Id }
            };

            FullWorkspace ws = new FullWorkspace(workspace);
            FullBoe boeObj = new FullBoe(boes[0]);
            FullBoe boeObj2 = new FullBoe(boes[1]);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boeObj, boeObj2 });
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws.Id)).Returns(ws);

            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "3ME2", ResourceDesc = "3ME2 - ES On Prem Mtn E2", SegRegion = "3M", LaborType = "E2", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.IWTA };

            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            BoeTaskElementDTO boeTaskElement1 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            BoeTaskElementDTO boeTaskElement2 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource2.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 800 } } } } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement1, boeTaskElement2 });
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1, resource2 });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullWbs> { wbs });
            retriever.Setup(x => x.GetBoesWithNestingByWbs(wbs.Id)).Returns(new Collection<FullBoe>() { boeObj, boeObj2 });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.SSCLMLabor, (int)SumVariableResourceType.SSCLOEIWTA};

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = 1, WBSID = 1 }, new SelectBOEsToSum() { BoeID = 2, WBSID = 1 } } } }, null, ws);

            decimal returnTotal = sut.GetTotalBasedOnWBSID(wbs.Id, SumVariableResourceTypes, data);
            Assert.AreEqual(returnTotal, 6500 + 800, "The totals were not as expected");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void GetTotalBasedOnCLINID()
        {
            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };
            FullWorkspace ws = new FullWorkspace(workspace);

            //set up wbs
            ClinDTO clin = new ClinDTO { Id = 1 };
            FullClin fullClin = new FullClin(clin);
            this.factory.Setup(x => x.CreateFullClin(clin)).Returns(fullClin);
            this.retriever.Setup(x => x.GetClinById(clin.Id)).Returns(clin);

            //setup boe
            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID=workspace.Id, CLINID = 1 };
            BoeDTO boe2 = new BoeDTO { Id = 2, WorkspaceID = workspace.Id, CLINID = 1 };

            FullBoe boeObject = new FullBoe(boe);
            FullBoe boeObject2 = new FullBoe(boe2);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boeObject, boeObject2 });
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws.Id)).Returns(ws);
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin> { fullClin });

            ICollection<FullBoe> clinBoes = new Collection<FullBoe> { boeObject, boeObject2 };
            this.retriever.Setup(x => x.GetFullBoesByClinId(clin.Id)).Returns(clinBoes);

            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.DS, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "3ME2", ResourceDesc = "3ME2 - ES On Prem Mtn E2", SegRegion = "3M", LaborType = "E2", Segment = SegmentType.ES, ElementOfCost = ElementOfCostType.LMLabor };

            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            BoeTaskElementDTO boeTaskElement1 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            BoeTaskElementDTO boeTaskElement2 = new BoeTaskElementDTO { TotalHours = 2500, BoeID = 2, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource2.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 100 }, new ResourceSpreadDto { LaborSpreadValue = 1500 } } } } };
            BoeTaskElementDTO boeTaskElement3 = new BoeTaskElementDTO { TotalHours = 100, BoeID = 3 }; // since this task element has no labor type, it should be skipped regardless if there are total hours (but in the real world, this wouldn't happen)
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement1, boeTaskElement2, boeTaskElement3 });
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1, resource2 });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new List<FullWbs>());

			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);
            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.ESLabor, (int)SumVariableResourceType.TSLabor };

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = 1, CLINID = clin.Id }, new SelectBOEsToSum() { BoeID = 2, CLINID = clin.Id } } } }, null, ws);

            decimal returnTotal = sut.GetTotalBasedOnCLINID(clin.Id, SumVariableResourceTypes, data);
            Assert.AreEqual(returnTotal, 6500 + 100 + 1500, "The totals were not as expected");
        }

        [TestMethod]
        public void GetTotalBasedOnCLINIDSpaceSystems()
        {
            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };
            FullWorkspace ws = new FullWorkspace(workspace);

            //set up wbs
            ClinDTO clin = new ClinDTO { Id = 1 };
            FullClin fullClin = new FullClin(clin);
            this.factory.Setup(x => x.CreateFullClin(clin)).Returns(fullClin);
            this.retriever.Setup(x => x.GetClinById(clin.Id)).Returns(clin);

            //setup boe
            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = workspace.Id, CLINID = 1 };
            BoeDTO boe2 = new BoeDTO { Id = 2, WorkspaceID = workspace.Id, CLINID = 1 };
            FullBoe boeObject = new FullBoe(boe);
            FullBoe boeObject2 = new FullBoe(boe2);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boeObject, boeObject2 });
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws.Id)).Returns(ws);
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin> { fullClin });

            ICollection<FullBoe> clinBoes = new Collection<FullBoe> { boeObject, boeObject2 };
            this.retriever.Setup(x => x.GetFullBoesByClinId(clin.Id)).Returns(clinBoes);

            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "3ME2", ResourceDesc = "3ME2 - ES On Prem Mtn E2", SegRegion = "3M", LaborType = "E2", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };

            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            BoeTaskElementDTO boeTaskElement1 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            BoeTaskElementDTO boeTaskElement2 = new BoeTaskElementDTO { TotalHours = 2500, BoeID = 2, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource2.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 100 }, new ResourceSpreadDto { LaborSpreadValue = 1500 } } } } };
            BoeTaskElementDTO boeTaskElement3 = new BoeTaskElementDTO { TotalHours = 100, BoeID = 3 }; // since this task element has no labor type, it should be skipped regardless if there are total hours (but in the real world, this wouldn't happen)
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement1, boeTaskElement2, boeTaskElement3 });
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1, resource2 });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new List<FullWbs>());

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);
            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.SSCLMLabor };

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = 1, CLINID = clin.Id }, new SelectBOEsToSum() { BoeID = 2, CLINID = clin.Id } } } }, null, ws);

            decimal returnTotal = sut.GetTotalBasedOnCLINID(clin.Id, SumVariableResourceTypes, data);
            Assert.AreEqual(returnTotal, 6500 + 100 + 1500, "The totals were not as expected");
        }

        [TestMethod]
        public void GetTaskVarLabelTotal()
        {
            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };

            //set up wbs
            WbsDTO wbs = new WbsDTO { Id = 1, WorkspaceID = 100, Level = 1 };

            //setup boe
            Collection<BoeDTO> boes = new Collection<BoeDTO> {
                new BoeDTO { Id = 1, WBSID = wbs.Id, WorkspaceID=workspace.Id },
                new BoeDTO { Id = 2, WBSID = 101, WorkspaceID=workspace.Id }
            };

            FullWorkspace ws = new FullWorkspace(workspace);
            Collection<FullBoe> fullBoes = new Collection<FullBoe>() { new FullBoe(boes[0]), new FullBoe(boes[1]) };

            this.retriever.Setup(x => x.GetWorkspaceById(fullBoes[0].WorkspaceID)).Returns(ws);
            this.retriever.Setup(i => i.GetFullBoesByWorkspaceId(fullBoes[0].WorkspaceID, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(fullBoes);
            this.retriever.Setup(x => x.GetFullWorkspaceById(fullBoes[0].WorkspaceID)).Returns(ws);

            this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>() { new FullWbs(wbs) });
            this.factory.Setup(x => x.CreateFullWbs(wbs)).Returns(new FullWbs(wbs));
            this.retriever.Setup(x => x.GetBoesWithNestingByWbs(It.IsAny<int>())).Returns(new List<FullBoe>() { new FullBoe(boes[0]) });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new List<FullWbs>() { new FullWbs(wbs) });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());

            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.LS, ElementOfCost = ElementOfCostType.Sub };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "3ME2", ResourceDesc = "3ME2 - ES On Prem Mtn E2", SegRegion = "3M", LaborType = "E2", Segment = SegmentType.ES, ElementOfCost = ElementOfCostType.LMLabor };
            retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO> { resource1, resource2 });

            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            BoeTaskElementDTO boeTaskElement1 = new BoeTaskElementDTO { TotalHours = 3500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 3500 } } } } };
            BoeTaskElementDTO boeTaskElement2 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 2, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement1, boeTaskElement2 });

            //setup boe task variable with a selected Boe and clin
            //setup workspace variable with a selected BOE, clin, and a wbs ID
            Collection<SelectBOEsToSum> selectedBoes = new Collection<SelectBOEsToSum>();
            SelectBOEsToSum boeSum = new SelectBOEsToSum();
            boeSum.BoeID = boes[1].Id;
            selectedBoes.Add(boeSum);
            SelectBOEsToSum wbsSum = new SelectBOEsToSum();
            wbsSum.WBSID = wbs.Id;
            selectedBoes.Add(wbsSum);

            OrdinaryVariableDto taskVar = new OrdinaryVariableDto { Id = 1, ValueType = VarValueType.SumOfBOEs, SelectedBOEsToSum = selectedBoes, SortBOEBy = VarSortBOEBy.WBS, SumVariableResourceTypeIDs = new Collection<int> { (int)SumVariableResourceType.LOESub, (int)SumVariableResourceType.ESLabor } };

			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(new List<OrdinaryVariableDto>() { taskVar }, null, ws);

            decimal returnTotal = sut.GetTaskVarLabelTotal(taskVar, data);
            Assert.AreEqual(10000, returnTotal, "The totals were not as expected");
        }

        [TestMethod]
        public void GetTaskVarLabelTotalSpaceSystems()
        {

            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };

            //set up wbs
            WbsDTO wbs = new WbsDTO { Id = 1, WorkspaceID = 100, Level = 1 };

            //setup boe
            Collection<BoeDTO> boes = new Collection<BoeDTO> {
                new BoeDTO { Id = 1, WBSID = wbs.Id, WorkspaceID=workspace.Id },
                new BoeDTO { Id = 2, WBSID = 101, WorkspaceID=workspace.Id }
            };

            FullWorkspace ws = new FullWorkspace(workspace);
            Collection<FullBoe> fullBoes = new Collection<FullBoe>() { new FullBoe(boes[0]), new FullBoe(boes[1]) };

            this.retriever.Setup(x => x.GetWorkspaceById(fullBoes[1].WorkspaceID)).Returns(ws);
            this.retriever.Setup(i => i.GetFullBoesByWorkspaceId(fullBoes[0].WorkspaceID, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(fullBoes);

            this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>() { new FullWbs(wbs) });
            this.factory.Setup(x => x.CreateFullWbs(wbs)).Returns(new FullWbs(wbs));
            this.retriever.Setup(x => x.GetBoesWithNestingByWbs(It.IsAny<int>())).Returns(new List<FullBoe>() { new FullBoe(boes[0]) });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new List<FullWbs>() { new FullWbs(wbs) });

            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.Sub };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "3ME2", ResourceDesc = "3ME2 - ES On Prem Mtn E2", SegRegion = "3M", LaborType = "E2", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };
            retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO> { resource1, resource2 });

            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            BoeTaskElementDTO boeTaskElement1 = new BoeTaskElementDTO { TotalHours = 3500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 3500 } } } } };
            BoeTaskElementDTO boeTaskElement2 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 2, taskElementLabors = new Collection<ResourceTypeDto> { new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } } } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement1, boeTaskElement2 });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());

            //setup boe task variable with a selected Boe and clin
            //setup workspace variable with a selected BOE, clin, and a wbs ID
            Collection<SelectBOEsToSum> selectedBoes = new Collection<SelectBOEsToSum>();
            SelectBOEsToSum boeSum = new SelectBOEsToSum();
            boeSum.BoeID = boes[1].Id;
            selectedBoes.Add(boeSum);
            SelectBOEsToSum wbsSum = new SelectBOEsToSum();
            wbsSum.WBSID = wbs.Id;
            selectedBoes.Add(wbsSum);

            OrdinaryVariableDto taskVar = new OrdinaryVariableDto { Id = 1, ValueType = VarValueType.SumOfBOEs, SelectedBOEsToSum = selectedBoes, SortBOEBy = VarSortBOEBy.WBS, SumVariableResourceTypeIDs = new Collection<int> { (int)SumVariableResourceType.SSCLOESub, (int)SumVariableResourceType.SSCLMLabor } };

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(new List<OrdinaryVariableDto>() { taskVar }, null, ws);

            decimal returnTotal = sut.GetTaskVarLabelTotal(taskVar, data);
            Assert.AreEqual(10000, returnTotal, "The totals were not as expected");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), TestMethod]
        public void GetTotalBasedOnBoeID2SpaceSystems()
        {
            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            //setup boe
            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };
            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = workspace.Id };
            FullWorkspace ws = new FullWorkspace(workspace);
            FullBoe boeObj = new FullBoe(boe);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boeObj });
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws.Id)).Returns(ws);
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());

            // before enhancement 12460 we would filter out resources based on the name, but now as long as the segment is correct we will grab it
            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "R1", ResourceDesc = "R1 Desc", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "R2", ResourceDesc = "R2 Desc", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource3 = new ResourceDTO { Id = 3, ResourceName = "R3", ResourceDesc = "R3 Desc", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.IWTA };
            ResourceDTO resource4 = new ResourceDTO { Id = 4, ResourceName = "R4", ResourceDesc = "R4 Desc", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.Materials };
            ResourceDTO resource5 = new ResourceDTO { Id = 5, ResourceName = "R5", ResourceDesc = "R5 Desc", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.ODC };
            ResourceDTO resource6 = new ResourceDTO { Id = 6, ResourceName = "R6", ResourceDesc = "R6 Desc", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.Sub };
            ResourceDTO resource7 = new ResourceDTO { Id = 7, ResourceName = "R7", ResourceDesc = "R7 Desc", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.Sub };
            ResourceDTO resource8 = new ResourceDTO { Id = 8, ResourceName = "R8", ResourceDesc = "R8 Desc", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.Travel };

            ResourceTypeDto boeLabor1 = new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 101 } } };
            ResourceTypeDto boeLabor2 = new ResourceTypeDto { ResourceID = resource2.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 102 } } };
            ResourceTypeDto boeLabor3 = new ResourceTypeDto { ResourceID = resource3.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 103 } } };
            ResourceTypeDto boeLabor4 = new ResourceTypeDto { ResourceID = resource4.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 104 } } };
            ResourceTypeDto boeLabor5 = new ResourceTypeDto { ResourceID = resource5.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 105 } } };
            ResourceTypeDto boeLabor6 = new ResourceTypeDto { ResourceID = resource6.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 106 } } };
            ResourceTypeDto boeLabor7 = new ResourceTypeDto { ResourceID = resource7.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 107 } } };
            ResourceTypeDto boeLabor8 = new ResourceTypeDto { ResourceID = resource8.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 108 } } };

            BoeTaskElementDTO boeTaskElement1 = new BoeTaskElementDTO { TotalHours = 8360, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor1, boeLabor2, boeLabor3, boeLabor4, boeLabor5, boeLabor6, boeLabor7, boeLabor8 } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement1 });
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1, resource2, resource3, resource4, resource5, resource6, resource7, resource8 });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new List<FullWbs>());

            ResourceDM.Setup(x => x.GetByIds(new Collection<int> { resource1.Id, resource2.Id, resource3.Id, resource4.Id, resource5.Id, resource6.Id, resource7.Id, resource8.Id })).Returns(new Collection<ResourceDTO> { resource1, resource2, resource3, resource4, resource5, resource6, resource7, resource8 });

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.SSCLMLabor, (int)SumVariableResourceType.SSCLOEIWTA, (int)SumVariableResourceType.SSCLOESub };

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = 1 } } } }, null, ws);

            decimal returnTotal = sut.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, data);
            Assert.AreEqual(returnTotal, 519, "The totals were not as expected"); // the total is 23550 since we selected LM Labor
        }

        [TestMethod]
        public void GetTotalBasedOnBoeID_Enhancement12460()
        {
            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            //setup boe
            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };
            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID=workspace.Id };
            FullWorkspace ws = new FullWorkspace(workspace);
            FullBoe boeObj = new FullBoe(boe);


            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boeObj });
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws.Id)).Returns(ws);

            // before enhancement 12460 we would filter out resources based on the name, but now as long as the segment is correct we will grab it
            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "ResourceTest", ResourceDesc = "BLAH - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.DS, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "LS HBR", ResourceDesc = "LS HBR", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.LS, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource3 = new ResourceDTO { Id = 3, ResourceName = "ES Resource", ResourceDesc = "ES Resource", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.ES, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource4 = new ResourceDTO { Id = 4, ResourceName = "TS R", ResourceDesc = "TS R", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.TS, ElementOfCost = ElementOfCostType.LMLabor };
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1, resource2, resource3, resource4 });

            ResourceTypeDto boeLabor1 = new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } };
            ResourceTypeDto boeLabor2 = new ResourceTypeDto { ResourceID = resource2.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6505 } } };
            ResourceTypeDto boeLabor3 = new ResourceTypeDto { ResourceID = resource3.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 8200 } } };
            ResourceTypeDto boeLabor4 = new ResourceTypeDto { ResourceID = resource4.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 2345 } } };

            BoeTaskElementDTO boeTaskElement1 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor1, boeLabor2, boeLabor3, boeLabor4 } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement1 });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new List<FullWbs>());
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.ESLabor, (int)SumVariableResourceType.LOEIWTA };

			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = 1 } } } }, null, ws);

            decimal returnTotal = sut.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, data);
            Assert.AreEqual(returnTotal, 14700, "The totals were not as expected"); // the total is 14,700 since we did not select LS Labor or TS Labor checkbox

            Collection<int> SumVariableResourceTypes2 = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.ESLabor, (int)SumVariableResourceType.LOEIWTA, (int)SumVariableResourceType.LSLabor, (int)SumVariableResourceType.TSLabor };
            returnTotal = sut.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes2, data);
            Assert.AreEqual(returnTotal, 23550, "The totals were not as expected"); // the total is 23550 since we now selected LS Labor, TS Labor, DS Labor, and ES Labor
        }

        [TestMethod]
        public void GetTotalBasedOnBoeID_Enhancement12460SpaceSystems()
        {
            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 1, PerformingOrgName = "LALA" };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            //setup boe
            WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "test", ResourceListID = 3 };
            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = workspace.Id };

            FullWorkspace ws = new FullWorkspace(workspace);
            FullBoe boeObj = new FullBoe(boe);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boeObj });
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws.Id)).Returns(ws);

            // before enhancement 12460 we would filter out resources based on the name, but now as long as the segment is correct we will grab it
            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "ResourceTest", ResourceDesc = "BLAH - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource2 = new ResourceDTO { Id = 2, ResourceName = "LS HBR", ResourceDesc = "LS HBR", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource3 = new ResourceDTO { Id = 3, ResourceName = "ES Resource", ResourceDesc = "ES Resource", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDTO resource4 = new ResourceDTO { Id = 4, ResourceName = "TS R", ResourceDesc = "TS R", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.SSC, ElementOfCost = ElementOfCostType.LMLabor };
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO> { resource1, resource2, resource3, resource4 });

            ResourceTypeDto boeLabor1 = new ResourceTypeDto { ResourceID = resource1.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6500 } } };
            ResourceTypeDto boeLabor2 = new ResourceTypeDto { ResourceID = resource2.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 6505 } } };
            ResourceTypeDto boeLabor3 = new ResourceTypeDto { ResourceID = resource3.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 8200 } } };
            ResourceTypeDto boeLabor4 = new ResourceTypeDto { ResourceID = resource4.Id, SpreadType = IES.Common.SpreadType.Hours, PerformingOrgID = perfOrg.Id, LaborSpreads = new Collection<ResourceSpreadDto> { new ResourceSpreadDto { LaborSpreadValue = 2345 } } };

            BoeTaskElementDTO boeTaskElement1 = new BoeTaskElementDTO { TotalHours = 6500, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto> { boeLabor1, boeLabor2, boeLabor3, boeLabor4 } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElement1 });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new List<FullWbs>());
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());

            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.SSCLMLabor, (int)SumVariableResourceType.SSCLOEIWTA };

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = 1 } } } }, null, ws);

            decimal returnTotal = sut.GetTotalBasedOnBoeID(boe.Id, SumVariableResourceTypes, data);
            Assert.AreEqual(returnTotal, 23550, "The totals were not as expected"); // the total is 23550 since we selected LM Labor
        }

        #region Exception Tests
        private static void SetupExceptionTest(out Mock<IPerformingOrgDTODataLoader> PerfOrgLoader)
        {
			Mock<IResourceDTODataLoader> ResourceDM = new Mock<IResourceDTODataLoader>();
            PerfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            ResourceDTO resource1 = new ResourceDTO { Id = 1, ResourceName = "1CE1", ResourceDesc = "1CE1 - DS On Prem West E1", SegRegion = "1C", LaborType = "E1", Segment = SegmentType.DS, ElementOfCost = ElementOfCostType.LMLabor };
            ResourceDM.Setup(x => x.GetByIds(new Collection<int> { resource1.Id })).Returns(new Collection<ResourceDTO> { resource1 });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest1()
        {
            SetupExceptionTest(out PerfOrgLoader);
			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);

            sut.GetTotalBasedOnCLINID(1, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest1SpaceSystems()
        {
            SetupExceptionTest(out PerfOrgLoader);
			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);

            sut.GetTotalBasedOnCLINID(1, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest2()
        {
            SetupExceptionTest(out PerfOrgLoader);

			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);
            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.ESLabor, (int)SumVariableResourceType.TSLabor };

            sut.GetTotalBasedOnCLINID(1, SumVariableResourceTypes, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest2SpaceSystems()
        {
            SetupExceptionTest(out PerfOrgLoader);

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);
            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.ESLabor, (int)SumVariableResourceType.TSLabor };

            sut.GetTotalBasedOnCLINID(1, SumVariableResourceTypes, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest3()
        {
            SetupExceptionTest(out PerfOrgLoader);
			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);

            sut.GetTotalBasedOnBoeID(1, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest3SpaceSystems()
        {
            SetupExceptionTest(out PerfOrgLoader);
			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);

            sut.GetTotalBasedOnBoeID(1, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest4()
        {
            SetupExceptionTest(out PerfOrgLoader);

			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);
            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.ESLabor, (int)SumVariableResourceType.TSLabor };

            sut.GetTotalBasedOnBoeID(1, SumVariableResourceTypes, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest4SpaceSystems()
        {
            SetupExceptionTest(out PerfOrgLoader);

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);
            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.ESLabor, (int)SumVariableResourceType.TSLabor };

            sut.GetTotalBasedOnBoeID(1, SumVariableResourceTypes, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest5()
        {
            SetupExceptionTest(out PerfOrgLoader);
			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);

            sut.GetTaskVarLabelTotal(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest5SpaceSystems()
        {
            SetupExceptionTest(out PerfOrgLoader);
			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);

            sut.GetTaskVarLabelTotal(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest6()
        {
            SetupExceptionTest(out PerfOrgLoader);
			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);
            Collection<SelectBOEsToSum> selectedBoes = new Collection<SelectBOEsToSum>();
            OrdinaryVariableDto taskVar = new OrdinaryVariableDto { Id = 1, ValueType = VarValueType.SumOfBOEs, SelectedBOEsToSum = selectedBoes, SortBOEBy = VarSortBOEBy.WBS, SumVariableResourceTypeIDs = new Collection<int> { (int)SumVariableResourceType.SSCLOESub, (int)SumVariableResourceType.SSCLMLabor } };

            sut.GetTaskVarLabelTotal(taskVar, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest6SpaceSystems()
        {
            SetupExceptionTest(out PerfOrgLoader);

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);
            Collection<SelectBOEsToSum> selectedBoes = new Collection<SelectBOEsToSum>();
            OrdinaryVariableDto taskVar = new OrdinaryVariableDto { Id = 1, ValueType = VarValueType.SumOfBOEs, SelectedBOEsToSum = selectedBoes, SortBOEBy = VarSortBOEBy.WBS, SumVariableResourceTypeIDs = new Collection<int> { (int)SumVariableResourceType.SSCLOESub, (int)SumVariableResourceType.SSCLMLabor } };

            sut.GetTaskVarLabelTotal(taskVar, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest7()
        {
            SetupExceptionTest(out PerfOrgLoader);
			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);

            sut.GetTotalBasedOnWBSID(1, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest7SpaceSystems()
        {
            SetupExceptionTest(out PerfOrgLoader);
			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);

            sut.GetTotalBasedOnWBSID(1, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest8()
        {
            SetupExceptionTest(out PerfOrgLoader);

			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);
            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.ESLabor, (int)SumVariableResourceType.TSLabor };

            sut.GetTotalBasedOnWBSID(1, SumVariableResourceTypes, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest8SpaceSystems()
        {
            SetupExceptionTest(out PerfOrgLoader);

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);
            Collection<int> SumVariableResourceTypes = new Collection<int> { (int)SumVariableResourceType.DSLabor, (int)SumVariableResourceType.ESLabor, (int)SumVariableResourceType.TSLabor };

            sut.GetTotalBasedOnWBSID(1, SumVariableResourceTypes, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest9()
        {
            SetupExceptionTest(out PerfOrgLoader);
			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);

            sut.GetWorkspaceVarLabelTotal(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest9SpaceSystems()
        {
            SetupExceptionTest(out PerfOrgLoader);
			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);

            sut.GetWorkspaceVarLabelTotal(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest10()
        {
            SetupExceptionTest(out PerfOrgLoader);

			VariableSelectBOEtoSumCalculation sut = new VariableSelectBOEtoSumCalculation(PerfOrgLoader.Object);
            Collection<SelectBOEsToSum> selectedBoes = new Collection<SelectBOEsToSum>();
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 1, ValueType = VarValueType.SumOfBOEs, SelectedBOEsToSum = selectedBoes, SumVariableResourceTypeIDs = new Collection<int> { (int)SumVariableResourceType.LOESub, (int)SumVariableResourceType.ESLabor } };

            sut.GetWorkspaceVarLabelTotal(workspaceVar, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VSBTSC_IsValidExceptionTest10SpaceSystems()
        {
            SetupExceptionTest(out PerfOrgLoader);

			VariableSelectBOEtoSumCalculationSpaceSystems sut = new VariableSelectBOEtoSumCalculationSpaceSystems(PerfOrgLoader.Object);
            Collection<SelectBOEsToSum> selectedBoes = new Collection<SelectBOEsToSum>();
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = 1, ValueType = VarValueType.SumOfBOEs, SelectedBOEsToSum = selectedBoes, SumVariableResourceTypeIDs = new Collection<int> { (int)SumVariableResourceType.LOESub, (int)SumVariableResourceType.ESLabor } };

            sut.GetWorkspaceVarLabelTotal(workspaceVar, null);
        }

        #endregion
    }
}