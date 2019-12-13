using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GenBOE.Common;
using GenBOE.DataBridge.Common;
using GenBOE.DataBridge.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using GenBOE.Dtos;

namespace GenBOE.Tests.DAL.DataMappers
{
    [TestClass]
    public class BoeTaskElementDTODataMapperTest
    {
        private Boolean _SpaceEnabled = ConfigurationUtilities.GetAppSetting<Boolean>("SpaceSystemsFeaturesEnabled", false);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestMethod]
        public void M_GetBoeTaskElementIDsByBoeID()
        {
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            int boeid = 1;
            int taskelementid = 2;
            Collection<int> Ids = new Collection<int> { taskelementid };
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBOETaskElementIDsByBoeIDDelegate>(),
                    It.IsAny<object[]>(),
                    CacheConstants.BOE_TASK_ELEMENT_IDS_BY_BOE_ID + "_" + boeid, false))
                .Returns(Ids);

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);


            Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO();
            boeTaskElement.BoeID = boeid;
            boeTaskElement.Id = 1;
            boeTaskElements.Add(boeTaskElement);

            ResourceTypeDto mockLT = new ResourceTypeDto();
            mockLT.ValueSpread = 45;
            mockLT.SpreadCurveID = SpreadCurves.DiscreteCost;
            Collection<ResourceTypeDto> allLT = new Collection<ResourceTypeDto>();
            allLT.Add(mockLT);
            boeTaskElement.taskElementLabors = allLT;

            taskElementLoader.Setup(x => x.GetBoeTaskElementIDsByBoeIDs(It.IsAny<Collection<int>>())).Returns(Ids);
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBoeTaskElementsByIdsDelegate>(), CacheConstants.BOE_TASK_ELEMENT_DTO_BY_ID, It.IsAny<Dictionary<string, int>>()))
                .Returns(new Collection<BoeTaskElementDTO>() { boeTaskElement });

            taskElementLoader.Setup(x => x.GetTaskElementIDsByBoeID(boeid)).Returns(Ids);
            taskElementLoader.Setup(x => x.GetBoeTaskElementByTaskElementID(taskelementid)).Returns(boeTaskElement);
            Collection<BoeTaskElementDTO> boeTaskElements2 = sut.GetBoeTaskElementCollectionByBoeID(boeid);
            Assert.IsTrue(boeTaskElements2.Count > 0, "There were no task Elements tied to the global boe id");

        }

        [TestMethod]
        public void M_GetBoeTaskElementByBoeTaskElementID()
        {
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            int taskelementid = 2;
            int boeid = 4;
            Collection<int> Ids = new Collection<int> { taskelementid };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = taskelementid, Description = "hello", BoeID = boeid };
            taskElementLoader.Setup(x => x.GetTaskElementIDsByBoeID(boeid)).Returns(Ids);
            taskElementLoader.Setup(x => x.GetBoeTaskElementByTaskElementID(taskelementid)).Returns(boeTaskElement);

            taskElementLoader.Setup(x => x.GetBoeTaskElementIDsByBoeIDs(It.IsAny<Collection<int>>())).Returns(Ids);
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBoeTaskElementsByIdsDelegate>(), CacheConstants.BOE_TASK_ELEMENT_DTO_BY_ID, It.IsAny<Dictionary<string, int>>())).Returns(new Collection<BoeTaskElementDTO>() { boeTaskElement });

            Collection<BoeTaskElementDTO> ids = sut.GetBoeTaskElementCollectionByBoeID(boeid);
            BoeTaskElementDTO toReturn = ids[0];
            Assert.IsTrue(toReturn != null, "No global task element found");
            Assert.AreEqual(toReturn.Description, boeTaskElement.Description, "Descriptions did not match");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestMethod]
        public void M_SaveBoeTaskElements()
        {

            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                                historicalMetricMapper.Object,
                                genDataMetricMapper.Object,
                                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);


            int taskelementid = -1;
            int boeid = 4;
            Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO();
            boeTaskElement.Id = taskelementid;
            boeTaskElement.BoeID = boeid;
            boeTaskElement.BOETaskID = "B99";
            boeTaskElement.Description = "Mock test";
            boeTaskElement.Updateable = UpdateType.Upsert;
            boeTaskElement.UpdateDate = DateTime.Now;
            boeTaskElement.OrdinaryVariables = new Collection<OrdinaryVariableDto> { new OrdinaryVariableDto { Id = 4, OrdinaryVariableName = "TaskVar1", ValueType = VarValueType.SumOfBOEs, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { CLINID = 3 } } } };
            boeTaskElements.Add(boeTaskElement);
            taskElementLoader.Setup(x => x.SaveBoeTaskElements(new Collection<BoeTaskElementDTO>() { boeTaskElement })).Returns(new Dictionary<int, int>() { { boeTaskElement.Id, 1 } });
            //Dictionary<int, ICollection<KeyValuePair<int, int>>> temp = new Dictionary<int, ICollection<KeyValuePair<int, int>>>() { { boeTaskElement.BOETaskElementID, new Collection<KeyValuePair<int, int>>() { new KeyValuePair<int, int>(1, 1) } } };
            customFieldValueMapper.Setup(x => x.GetCustomFieldValueIDsContainerIDsByTaskElementIDs(new Collection<int>() { 1 })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>() { { boeTaskElement.Id, new Collection<KeyValuePair<int, int>>() { new KeyValuePair<int, int>(1, 1) } } });
            historicalMetricMapper.Setup(x => x.GetHistoricalMetricIDsByTaskElementIDs(new Collection<int>() { 1 })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            genDataMetricMapper.Setup(x => x.GetGenDataMetricIDsByTaskElementIDs(new Collection<int>() { 1 })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            poMapper.Setup(x => x.GetPerformingOrgIDsByTaskElementIDs(new Collection<int>() { 1 })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            taskElementLoader.Setup(x => x.GetBoeIDsByTaskElementIDs(new Collection<int>() { 1 })).Returns(new Dictionary<int, int>() { { boeTaskElement.Id, boeTaskElement.BoeID } });
            //workspaceVarMapper.Setup(x => x.GetWorkspaceVariableIDsByTaskElementIDs(new Collection<int>() { 1 })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            wbsMapper.Setup(x => x.GetWbsIDsByTaskElementIDs(new Collection<int>() { 1 })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            customFieldValueMapper.Setup(x => x.GetCustomFieldValueIDsContainerIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>() { { boeTaskElement.Id, new Collection<KeyValuePair<int, int>>() { new KeyValuePair<int, int>(1, 1) } } });
            historicalMetricMapper.Setup(x => x.GetHistoricalMetricIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            genDataMetricMapper.Setup(x => x.GetGenDataMetricIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            poMapper.Setup(x => x.GetPerformingOrgIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            taskElementLoader.Setup(x => x.GetBoeIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, int>() { { boeTaskElement.Id, boeTaskElement.BoeID } });
            //workspaceVarMapper.Setup(x => x.GetWorkspaceVariableIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            wbsMapper.Setup(x => x.GetWbsIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });

            //save
            ((IInternalBoeTaskElementDTODataMapper)sut).SaveBoeTaskElements(boeTaskElements);

            // verify cache was cleared. nothing else to assert on a mapper save
            cacheDataLoader.Verify(x => x.Remove(CacheConstants.BOE_TASK_ELEMENT_DTO_BY_ID + "_" + 1), Times.Once());

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestMethod]
        public void M_VerifyMOQEquation()
        {

            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var workspaceVarMapper = new Mock<IWorkspaceVariableDTODataMapper>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var clinMapper = new Mock<IClinDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                                workspaceVarMapper.Object,
                                historicalMetricMapper.Object, customFieldValueMapper.Object, clinMapper.Object, wbsMapper.Object, poMapper.Object);

            int taskelementid = -1;
            int boeid = 4;
            Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO();
            BOELaborType laborType = new BOELaborType();
            boeTaskElement.BOETaskElementID = taskelementid;
            boeTaskElement.BoeID = boeid;
            boeTaskElement.BOETaskID = "B99";
            boeTaskElement.Description = "Mock test";
            boeTaskElement.Updateable = GenBOE.ModelView.Common.UpdateType.Upsert;
            boeTaskElement.UpdateDate = DateTime.Now;
            boeTaskElement.OrdinaryVariables = new Collection<BoeTaskOrdinaryVariable> { new BoeTaskOrdinaryVariable { OrdinaryVariableID = 4, OrdinaryVariableName = "TaskVar1", ValueType = VarValueType.SumOfBOEs, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { CLINID = 3 } } } };
            boeTaskElement.taskElementLabors = new Collection<BOELaborType>() {laborType};
            boeTaskElements.Add(boeTaskElement);

            // Should fail verification
            laborType.ValueSpread = -5;
            boeTaskElement.MOQHoursEquation = "";
            ((IInternalBoeTaskElementDTODataMapper)sut).VerifyMOQEquation(12, boeTaskElements);
            laborType.ValueSpread = 5;
            ((IInternalBoeTaskElementDTODataMapper)sut).VerifyMOQEquation(12, boeTaskElements);

            // Should pass verification
            laborType.ValueSpread = 0;
            ((IInternalBoeTaskElementDTODataMapper)sut).VerifyMOQEquation(12, boeTaskElements);
            laborType.ValueSpread = 5;
            boeTaskElement.MOQHoursEquation = "5";
            ((IInternalBoeTaskElementDTODataMapper)sut).VerifyMOQEquation(12, boeTaskElements);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestMethod]
        public void M_SaveBOETaskElementLaborTypeWarning()
        {
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new BoeTaskElementDTODataMapper(
               taskElementLoader.Object,
               cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            int taskelementid = 2;
            int boeid = 4;
            Collection<BoeTaskElementDTO> boetaskElementCollection = new Collection<BoeTaskElementDTO>();

            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = taskelementid, BoeID = boeid, LaborTypeWarningFlag = true };
            boetaskElementCollection.Add(boeTaskElement);


            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBoeTaskElementDTODelegate>(), It.IsAny<object[]>(), CacheConstants.BOE_TASK_ELEMENT_DTO_BY_ID + "_" + taskelementid, It.IsAny<bool>()))
                .Returns(boeTaskElement);

            taskElementLoader.Setup(x => x.GetBoeTaskElementByTaskElementID(boeTaskElement.Id)).Returns(boeTaskElement);
            taskElementLoader.Setup(x => x.SaveBOETaskElementLaborTypeWarning(boeTaskElement.Id, boeTaskElement.LaborTypeWarningFlag.Value));
            taskElementLoader.Setup(x => x.GetBoeIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, int>() { { boeTaskElement.Id, boeTaskElement.BoeID } });
            //workspaceVarLoader.Setup(x => x.GetWorkspaceVariableIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            wbsMapper.Setup(x => x.GetWbsIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            customFieldValueMapper.Setup(x => x.GetCustomFieldValueIDsContainerIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>() { { boeTaskElement.Id, new Collection<KeyValuePair<int, int>>() { new KeyValuePair<int, int>(1, 1) } } });
            historicalMetricMapper.Setup(x => x.GetHistoricalMetricIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            genDataMetricMapper.Setup(x => x.GetGenDataMetricIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            poMapper.Setup(x => x.GetPerformingOrgIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });

            sut.SaveBOETaskElementLaborTypeWarning(boeTaskElement.Id, boeTaskElement.LaborTypeWarningFlag.Value);
            taskElementLoader.Verify(x => x.SaveBOETaskElementLaborTypeWarning(boeTaskElement.Id, boeTaskElement.LaborTypeWarningFlag.Value), Times.Once());
        }

        [TestMethod]
        public void M_WarmCacheTaskElements()
        {
            var _BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var cacheProxy = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(cacheProxy.Object, -1, new GenBOEUtilities());
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            int taskelementid = 2;
            int boeid = 4;

            Collection<BoeTaskElementDTO> taskElements = new Collection<BoeTaskElementDTO> { new BoeTaskElementDTO { Id = taskelementid, BoeID = boeid, Description = "warm cache test" } };
            _BoeTaskElementLoader.Setup(x => x.GetAllBoeTaskElements()).Returns(taskElements);

            var sut = new CacheWarmingBoeTaskElementMapper(_BoeTaskElementLoader.Object, cacheDataLoader.Object, cacheProxy.Object,
                historicalMetricMapper.Object, genDataMetricMapper.Object, customFieldValueMapper.Object, wbsMapper.Object,
                poMapper.Object);
            sut.WarmTaskElementCache(taskElements);

            //verify the item was added to cache
            cacheProxy.Verify(x => x.Add(CacheConstants.BOE_TASK_ELEMENT_DTO_BY_ID + "_" + taskelementid, taskElements[0], -1), Times.Once());
        }

        [TestMethod]
        //Note: There is actually no get for custom field value containers
        public void GetBOETaskElementCustomFieldValueContainers()
        {
            int customFieldValueid = 2;
            int taskelementid = 4;
            int boeid = 6;

            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            CustomFieldValueContainer customField = new CustomFieldValueContainer();
            customField.ContainerID = 1;
            customField.CustomFieldValueID = customFieldValueid;


            BoeTaskElementDTO boeTask = new BoeTaskElementDTO { Id = taskelementid, BoeID = boeid, CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { customField } };
            taskElementLoader.Setup(x => x.GetBoeTaskElementByTaskElementID(taskelementid)).Returns(boeTask);
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBoeTaskElementDTODelegate>(), It.IsAny<object[]>(), CacheConstants.BOE_TASK_ELEMENT_DTO_BY_ID + "_" + taskelementid, It.IsAny<bool>()))
.Returns(boeTask);
            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object,
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);



            //sut
            boeTask = sut.GetBoeTaskElementByBoeTaskElementID(taskelementid);
            //Assert
            Assert.IsTrue(boeTask.CustomFieldValueContainers.Count == 1, "No custom field returned");
            Assert.IsTrue(boeTask.CustomFieldValueContainers[0].CustomFieldValueID == customFieldValueid, "Custom Field Value ID was not the global one");
        }

        [TestMethod]
        //Note: There is actually no Get method
        public void GetBOELaborTypeCustomFieldValueContainers()
        {
            int customFieldValueid = 2;
            int taskelementid = 4;
            int boeid = 6;
            int boelabortypeid = 8;

            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            CustomFieldValueContainer customField = new CustomFieldValueContainer();
            customField.ContainerID = 1;
            customField.CustomFieldValueID = customFieldValueid;
            ResourceTypeDto labor = new ResourceTypeDto { Id = boelabortypeid, BoeID = boeid, CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { customField } };

            BoeTaskElementDTO boeTask = new BoeTaskElementDTO { Id = taskelementid, BoeID = boeid, taskElementLabors = new Collection<ResourceTypeDto> { labor } };
            taskElementLoader.Setup(x => x.GetBoeTaskElementByTaskElementID(taskelementid)).Returns(boeTask);
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBoeTaskElementDTODelegate>(), It.IsAny<object[]>(), CacheConstants.BOE_TASK_ELEMENT_DTO_BY_ID + "_" + taskelementid, It.IsAny<bool>()))
.Returns(boeTask);
            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);



            //sut
            boeTask = sut.GetBoeTaskElementByBoeTaskElementID(taskelementid);
            //Assert
            Assert.IsTrue(boeTask.taskElementLabors[0].CustomFieldValueContainers.Count == 1, "No custom field returned");
            Assert.IsTrue(boeTask.taskElementLabors[0].CustomFieldValueContainers[0].CustomFieldValueID == customFieldValueid, "Custom Field Value ID was not the global one");

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestMethod]
        public void SaveBOELaborTypeCustomValueFieldXrefs()
        {
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            //var workspaceVarLoader = new Mock<IWorkspaceDTODataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            int customFieldValueid = 2;
            int taskelementid = -1;
            int boeid = 6;
            int resourceId = 10;
            int perfOrgId = 12;

            Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO();
            boeTaskElement.Id = taskelementid;
            boeTaskElement.BoeID = boeid;
            boeTaskElement.BOETaskID = "B99";
            boeTaskElement.Description = "Mock test";
            boeTaskElement.Updateable = UpdateType.Upsert;
            boeTaskElement.UpdateDate = DateTime.Now;

            Collection<ResourceTypeDto> boeLaborTypes = new Collection<ResourceTypeDto>();
            ResourceTypeDto labor = new ResourceTypeDto();
            labor.BoeID = boeid;
            labor.Updateable = UpdateType.Upsert;
            labor.Id = -1;
            labor.ResourceID = resourceId;
            labor.PerformingOrgID = perfOrgId;

            CustomFieldValueContainer customField = new CustomFieldValueContainer();
            customField.ContainerID = 1;
            customField.CustomFieldValueID = customFieldValueid;
            labor.CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { customField };
            boeLaborTypes.Add(labor);
            boeTaskElement.taskElementLabors = boeLaborTypes;
            boeTaskElements.Add(boeTaskElement);

            taskElementLoader.Setup(x => x.GetBoeIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, int>() { { boeTaskElement.Id, boeTaskElement.BoeID } });
            taskElementLoader.Setup(x => x.SaveBoeTaskElements(boeTaskElements)).Returns(new Dictionary<int, int>() { { boeTaskElement.Id, boeTaskElement.Id } });
           // workspaceVarLoader.Setup(x => x.GetWorkspaceVariableIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            wbsMapper.Setup(x => x.GetWbsIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            customFieldValueMapper.Setup(x => x.GetCustomFieldValueIDsContainerIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>() { { boeTaskElement.Id, new Collection<KeyValuePair<int, int>>() { new KeyValuePair<int, int>(1, 1) } } });
            historicalMetricMapper.Setup(x => x.GetHistoricalMetricIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            genDataMetricMapper.Setup(x => x.GetGenDataMetricIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            poMapper.Setup(x => x.GetPerformingOrgIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            //save
            ((IInternalBoeTaskElementDTODataMapper)sut).SaveBoeTaskElements(boeTaskElements);

            // verify cache was cleared. nothing else to assert on a mapper save
            cacheDataLoader.Verify(x => x.Remove(CacheConstants.BOE_TASK_ELEMENT_DTO_BY_ID + "_" + taskelementid), Times.Once());
        }

        [TestMethod]
        public void CheckIfBoeLaborTypetExistsGivenCustomFieldID()
        {
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            int customFieldID = 2;
            int boelabortypeid = 14;

            taskElementLoader.Setup(x => x.CheckIfBoeLaborTypeExistsGivenCustomFieldID(boelabortypeid, customFieldID)).Returns(true);
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            bool boeLaborTypeExists = sut.CheckIfBoeLaborTypeExistsGivenCustomFieldID(boelabortypeid, customFieldID);
            Assert.IsTrue(boeLaborTypeExists, "The Boe Labor Type did not exist");

        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestMethod]
        public void SaveBOETaskElementCustomValueFieldXrefs()
        {
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            int boeid = 2;
            int customFieldValueId = 4;
            int boetaskelementId = -1;

            Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO();
            boeTaskElement.Id = boetaskelementId;
            boeTaskElement.BoeID = boeid;
            boeTaskElement.BOETaskID = "B99";
            boeTaskElement.Description = "Mock test";
            boeTaskElement.Updateable = UpdateType.Upsert;
            boeTaskElement.UpdateDate = DateTime.Now;

            // add custom field
            CustomFieldValueContainer customField = new CustomFieldValueContainer();
            customField.ContainerID = 1;
            customField.CustomFieldValueID = customFieldValueId;
            boeTaskElement.CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { customField };

            boeTaskElements.Add(boeTaskElement);

            taskElementLoader.Setup(x => x.GetBoeIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, int>() { { boeTaskElement.Id, boeTaskElement.BoeID } });
            taskElementLoader.Setup(x => x.SaveBoeTaskElements(boeTaskElements)).Returns(new Dictionary<int, int>() { { boeTaskElement.Id, boeTaskElement.Id } });
            //workspaceVarLoader.Setup(x => x.GetWorkspaceVariableIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            wbsMapper.Setup(x => x.GetWbsIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            customFieldValueMapper.Setup(x => x.GetCustomFieldValueIDsContainerIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>() { { boeTaskElement.Id, new Collection<KeyValuePair<int, int>>() { new KeyValuePair<int, int>(1, 1) } } });
            historicalMetricMapper.Setup(x => x.GetHistoricalMetricIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            genDataMetricMapper.Setup(x => x.GetGenDataMetricIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            poMapper.Setup(x => x.GetPerformingOrgIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            //save
            ((IInternalBoeTaskElementDTODataMapper)sut).SaveBoeTaskElements(boeTaskElements);

            // verify cache was cleared. nothing else to assert on a mapper save
            cacheDataLoader.Verify(x => x.Remove(CacheConstants.BOE_TASK_ELEMENT_DTO_BY_ID + "_" + boetaskelementId), Times.Once());
        }

        [TestMethod]
        public void CheckIfBoeTaskElementExistsGivenCustomFieldID()
        {
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            int customFieldID = 1;
            int boelabortypeid = 2;

            taskElementLoader.Setup(x => x.CheckIfBoeTaskElementExistsGivenCustomFieldID(boelabortypeid, customFieldID)).Returns(true);
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object,
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            bool boeTaskEleementExists = sut.CheckIfBoeTaskElementExistsGivenCustomFieldID(boelabortypeid, customFieldID);
            Assert.IsTrue(boeTaskEleementExists, "The Boe Labor Type did not exist");

        }

        [TestMethod]
        public void M_GetBoeTaskElementIDsByWorkspaceVariableID()
        {
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            Collection<int> taskElementIDs = new Collection<int> { 1, 2, 3 };
            int TaskVarID = 1;

            string key = CacheConstants.TASK_ELEMENT_IDS_BASED_ON_WORKSPACE_VARIABLE_ID + TaskVarID.ToString();
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetCollectionOfIdsById>(), It.IsAny<object[]>(), key, false)).Returns(new Collection<int>(taskElementIDs));

            taskElementLoader.Setup(x => x.GetBoeTaskElementIdsByWorkspaceVariableID(TaskVarID)).Returns(taskElementIDs);
            var sut = new BoeTaskElementDTODataMapper(taskElementLoader.Object, cacheDataLoader.Object, 
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            Assert.IsTrue(sut.GetBoeTaskElementIDsByWorkspaceVariableID(TaskVarID).Count == taskElementIDs.Count, "Task Element IDs were the same");
        }

        [TestMethod]
        public void M_WarmCacheTaskElementIDs()
        {
            int boe1id = 2;
            int boe2id = 4;

            var _BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var cacheProxy = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(cacheProxy.Object, -1, new GenBOEUtilities());
            Collection<BoeTaskElementDTO> taskElements = new Collection<BoeTaskElementDTO> { new BoeTaskElementDTO { Id = 1, BoeID = boe1id, Description = "warm cache test" }, new BoeTaskElementDTO { Id = 2, BoeID = boe2id }, new BoeTaskElementDTO { Id = 3, BoeID = boe1id, Description = "warm cache test" } };
            _BoeTaskElementLoader.Setup(x => x.GetAllBoeTaskElements()).Returns(taskElements);
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new CacheWarmingBoeTaskElementMapper(_BoeTaskElementLoader.Object, cacheDataLoader.Object, cacheProxy.Object, historicalMetricMapper.Object, genDataMetricMapper.Object, customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);
            sut.WarmBoeTaskElementIDs(taskElements);

            //verify the item was added to cache
            cacheDataLoader.Verify(x => x.GetData(It.IsAny<GetBOETaskElementIDsByBoeIDDelegate>(), It.IsAny<object[]>(), CacheConstants.BOE_TASK_ELEMENT_IDS_BY_BOE_ID + "_" + boe1id, false));
            cacheDataLoader.Verify(x => x.GetData(It.IsAny<GetBOETaskElementIDsByBoeIDDelegate>(), It.IsAny<object[]>(), CacheConstants.BOE_TASK_ELEMENT_IDS_BY_BOE_ID + "_" + boe2id, false));

        }
        [TestMethod]
        public void M_GetHistoricalMetricIdsByTaskElementID()
        {
            int taskElementID = 1;
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            Collection<int> Ids = new Collection<int> { 1, 2, 3 };
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetHistoricalMetricsIDsByTaskElementIDDelegate>(),
           It.IsAny<object[]>(),
            CacheConstants.HistoricalMetrics_IDS_By_TaskElementID + "_" + taskElementID, false))
            .Returns(Ids);

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            taskElementLoader.Setup(x => x.GetHistoricalMetricsByTaskElementID(taskElementID)).Returns(Ids);

            Collection<int> historicalMetricIDs = sut.GetHistoricalMetricIdsByTaskElementID(taskElementID);
            Assert.AreEqual(historicalMetricIDs.Count, Ids.Count, "The ID counts didn't match");
        }

        [TestMethod]
        public void M_GetGenDataMetricIdsByTaskElementID()
        {
            int taskElementID = 1;
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            Collection<int> Ids = new Collection<int> { 1, 2, 3 };
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetGenDataMetricsIDsByTaskElementIDDelegate>(),
           It.IsAny<object[]>(),
            CacheConstants.GENDATAMETRICS_IDS_BY_TASKELEMENTID + taskElementID, false))
            .Returns(Ids);
            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object,
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            taskElementLoader.Setup(x => x.GetGenDataMetricsByTaskElementID(taskElementID)).Returns(Ids);

            Collection<int> historicalMetricIDs = sut.GetGenDataMetricIdsByTaskElementID(taskElementID);
            Assert.AreEqual(historicalMetricIDs.Count, Ids.Count, "The ID counts didn't match");
        }


        [TestMethod]
        public void M_GetHistoricalMetricsByTaskElementID()
        {
            int taskElementID = 1;
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            Collection<int> Ids = new Collection<int> { 1, 2, 3 };
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetHistoricalMetricsIDsByTaskElementIDDelegate>(),
           It.IsAny<object[]>(),
            CacheConstants.HistoricalMetrics_IDS_By_TaskElementID + "_" + taskElementID, false))
            .Returns(Ids);

            //setup historical metrics
            HistoricalMetricDTO metric1 = new HistoricalMetricDTO { ID = 1 };
            HistoricalMetricDTO metric2 = new HistoricalMetricDTO { ID = 2 };
            HistoricalMetricDTO metric3 = new HistoricalMetricDTO { ID = 3 };
            historicalMetricMapper.Setup(x => x.GetHistoricalMetricByID(Ids[0])).Returns(metric1);
            historicalMetricMapper.Setup(x => x.GetHistoricalMetricByID(Ids[1])).Returns(metric2);
            historicalMetricMapper.Setup(x => x.GetHistoricalMetricByID(Ids[2])).Returns(metric3);

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            taskElementLoader.Setup(x => x.GetHistoricalMetricsByTaskElementID(taskElementID)).Returns(Ids);

            Collection<int> historicalMetricIDs = sut.GetHistoricalMetricIdsByTaskElementID(taskElementID);
            Assert.AreEqual(historicalMetricIDs.Count, Ids.Count, "The ID counts didn't match");

            Collection<HistoricalMetricDTO> metrics = sut.GetHistoricalMetricsByTaskElementID(taskElementID);
            Assert.IsTrue(metrics.Contains(metric1), "Metric 1 not returned");
            Assert.IsTrue(metrics.Contains(metric2), "Metric 1 not returned");
            Assert.IsTrue(metrics.Contains(metric3), "Metric 1 not returned");
        }

        [TestMethod]
        public void M_GetGenDataMetricsByTaskElementID()
        {
            int taskElementID = 1;
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            Collection<int> Ids = new Collection<int> { 1, 2, 3 };
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetGenDataMetricsIDsByTaskElementIDDelegate>(),
           It.IsAny<object[]>(),
            CacheConstants.GENDATAMETRICS_IDS_BY_TASKELEMENTID + taskElementID, false))
            .Returns(Ids);

            //setup historical metrics
            GenDataMetricDTO metric1 = new GenDataMetricDTO { Id = 1 };
            GenDataMetricDTO metric2 = new GenDataMetricDTO { Id = 2 };
            GenDataMetricDTO metric3 = new GenDataMetricDTO { Id = 3 };
            genDataMetricMapper.Setup(x => x.GetById(Ids[0])).Returns(metric1);
            genDataMetricMapper.Setup(x => x.GetById(Ids[1])).Returns(metric2);
            genDataMetricMapper.Setup(x => x.GetById(Ids[2])).Returns(metric3);

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object,
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            taskElementLoader.Setup(x => x.GetGenDataMetricsByTaskElementID(taskElementID)).Returns(Ids);

            Collection<int> genDataMetricIDs = sut.GetGenDataMetricIdsByTaskElementID(taskElementID);
            Assert.AreEqual(genDataMetricIDs.Count, Ids.Count, "The ID counts didn't match");

            Collection<GenDataMetricDTO> metrics = sut.GetGenDataMetricsByTaskElementID(taskElementID);
            Assert.IsTrue(metrics.Contains(metric1), "Metric 1 not returned");
            Assert.IsTrue(metrics.Contains(metric2), "Metric 1 not returned");
            Assert.IsTrue(metrics.Contains(metric3), "Metric 1 not returned");
        }

        [TestMethod]
        public void M_WarmBOETaskElementHistoricalMetricsIDsCache()
        {
            int boeID = 1;
            var _BoeTaskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();
            var cacheProxy = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(cacheProxy.Object, -1, new GenBOEUtilities());
            BoeTaskElementDTO boeTask1 = new BoeTaskElementDTO { Id = 1, BoeID = boeID, Description = "warm cache test" };
            Collection<int> historicMetric1 = new Collection<int> { 1, 2 };
            BoeTaskElementDTO boeTask2 = new BoeTaskElementDTO { Id = 2, BoeID = boeID, Description = "warm cache test" };
            Collection<int> historicMetric2 = new Collection<int> { 3, 4 };

            Collection<BoeTaskElementDTO> taskElements = new Collection<BoeTaskElementDTO> { boeTask1, boeTask2 };
            _BoeTaskElementLoader.Setup(x => x.GetAllBoeTaskElements()).Returns(taskElements);
            if (_SpaceEnabled)
            {
                _BoeTaskElementLoader.Setup(x => x.GetHistoricalMetricsByTaskElementID(boeTask1.Id)).Returns(historicMetric1);
                _BoeTaskElementLoader.Setup(x => x.GetHistoricalMetricsByTaskElementID(boeTask1.Id)).Returns(historicMetric2);
            }
            else
            {
                _BoeTaskElementLoader.Setup(x => x.GetGenDataMetricsByTaskElementID(boeTask1.Id)).Returns(historicMetric1);
                _BoeTaskElementLoader.Setup(x => x.GetGenDataMetricsByTaskElementID(boeTask1.Id)).Returns(historicMetric2);
            }

            var sut = new CacheWarmingBoeTaskElementMapper(_BoeTaskElementLoader.Object, cacheDataLoader.Object, cacheProxy.Object, historicalMetricMapper.Object, genDataMetricMapper.Object, customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);
            sut.WarmBOETaskElementHistoricalMetricsIDsCache(taskElements);

            //verify the item was added to cache
            if (_SpaceEnabled)
            {
            cacheDataLoader.Verify(x => x.GetData(It.IsAny<GetHistoricalMetricsIDsByTaskElementIDDelegate>(), It.IsAny<object[]>(), CacheConstants.HistoricalMetrics_IDS_By_TaskElementID + "_" + 1, false));
            cacheDataLoader.Verify(x => x.GetData(It.IsAny<GetHistoricalMetricsIDsByTaskElementIDDelegate>(), It.IsAny<object[]>(), CacheConstants.HistoricalMetrics_IDS_By_TaskElementID + "_" + 2, false));
            }
            else
            {
                cacheDataLoader.Verify(x => x.GetData(It.IsAny<GetGenDataMetricsIDsByTaskElementIDDelegate>(), It.IsAny<object[]>(), CacheConstants.GENDATAMETRICS_IDS_BY_TASKELEMENTID + 1, false));
                cacheDataLoader.Verify(x => x.GetData(It.IsAny<GetGenDataMetricsIDsByTaskElementIDDelegate>(), It.IsAny<object[]>(), CacheConstants.GENDATAMETRICS_IDS_BY_TASKELEMENTID + 2, false));
            }

        }

        [TestMethod]
        public void M_GetTaskVariableByTaskVariableID()
        {
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            OrdinaryVariableDto taskVar = new OrdinaryVariableDto { Id = 400, OrdinaryVariableName = "TaskVar1" };

            taskElementLoader.Setup(x => x.GetTaskVariableByTaskVariableID(taskVar.Id)).Returns(taskVar);


            OrdinaryVariableDto taskVarReturn = sut.GetTaskVariableByTaskVariableID(taskVar.Id);

            Assert.AreEqual(taskVar, taskVarReturn, "Task Var was not returned");

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestMethod]
        public void M_ClearCacheKeysBasedOnTaskElementIDs()
        {
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var workspaceVarLoader = new Mock<IWorkspaceVariableDTODataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            int taskelementid = 2;
            int boeid = 4;
            Collection<int> Ids = new Collection<int> { taskelementid };
            BoeTaskElementDTO boeTaskElement = new BoeTaskElementDTO { Id = taskelementid, Description = "hello", BoeID = boeid };
            taskElementLoader.Setup(x => x.GetTaskElementIDsByBoeID(boeid)).Returns(Ids);
            taskElementLoader.Setup(x => x.GetBoeTaskElementByTaskElementID(taskelementid)).Returns(boeTaskElement);
            taskElementLoader.Setup(x => x.GetBoeIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, int>() { { boeTaskElement.Id, boeTaskElement.BoeID } });
            workspaceVarLoader.Setup(x => x.GetWorkspaceVariableIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            wbsMapper.Setup(x => x.GetWbsIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            customFieldValueMapper.Setup(x => x.GetCustomFieldValueIDsContainerIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>() { { boeTaskElement.Id, new Collection<KeyValuePair<int, int>>() { new KeyValuePair<int, int>(1, 1) } } });
            historicalMetricMapper.Setup(x => x.GetHistoricalMetricIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            genDataMetricMapper.Setup(x => x.GetGenDataMetricIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });
            poMapper.Setup(x => x.GetPerformingOrgIDsByTaskElementIDs(new Collection<int>() { boeTaskElement.Id })).Returns(new Dictionary<int, ICollection<int>>() { { boeTaskElement.Id, new Collection<int>() { 1 } } });

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBoeTaskElementDTODelegate>(),
                    It.IsAny<object[]>(),
                    CacheConstants.BOE_TASK_ELEMENT_DTO_BY_ID + "_" + taskelementid,
                    It.IsAny<bool>()))
                .Returns(boeTaskElement);

            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBOETaskElementIDsByBoeIDDelegate>(),
                    It.IsAny<object[]>(),
                    CacheConstants.BOE_TASK_ELEMENT_IDS_BY_BOE_ID + "_" + boeid, false))
                .Returns(Ids);

            BoeTaskElementClearCacheData boeTaskElementClearCacheData = new BoeTaskElementClearCacheData();
            sut.ClearCacheKeys(Ids, boeTaskElementClearCacheData);

            cacheDataLoader.Verify(x => x.Remove(CacheConstants.BOE_TASK_ELEMENT_DTO_BY_ID + "_" + taskelementid), Times.Once());

        }

        [TestMethod]
        public void M_GetResourceLinkedTaskElementIDs()
        {
            var taskElementLoader = new Mock<IBoeTaskElementDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            var historicalMetricMapper = new Mock<IHistoricalMetricDTODataMapper>();
            var genDataMetricMapper = new Mock<IGenDataMetricMapper>();
            var customFieldValueMapper = new Mock<ICustomFieldValueDTODataMapper>();
            var wbsMapper = new Mock<IWbsDTODataMapper>();
            var poMapper = new Mock<IPerformingOrgDTODataMapper>();

            var sut = new BoeTaskElementDTODataMapper(
                taskElementLoader.Object,
                cacheDataLoader.Object,
                historicalMetricMapper.Object, 
                genDataMetricMapper.Object,
                customFieldValueMapper.Object, wbsMapper.Object, poMapper.Object);

            int taskelementid = 2;
            int resourceID = 4;
            int workspaceid = 22;
            Collection<int> Ids = new Collection<int> { taskelementid };
            taskElementLoader.Setup(x => x.GetBoeTaskElementIdsByWorkspaceIDAndResourceID(workspaceid, resourceID)).Returns(Ids);
            Collection<int> returnedIDs = sut.GetResourceLinkedTaskElementIDs(resourceID, workspaceid);

            Assert.AreEqual(Ids, returnedIDs, "collection of ids didn't match");

            taskElementLoader.Verify(x => x.GetBoeTaskElementIdsByWorkspaceIDAndResourceID(workspaceid, resourceID), Times.Once());
     
        }
    }
}
