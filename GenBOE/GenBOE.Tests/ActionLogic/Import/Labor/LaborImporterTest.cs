// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LaborImporterTest
    {
        private Mock<IPermissionsDTODataLoader> permissionsLoader = null;
        private Mock<IRetriever> retriever = null;
        private Mock<IFullObjectFactory> factory = null;

        private Mock<IPerformingOrgDTODataLoader> perfOrgLoader = null;
        private PerformingOrgDTO perfOrg = null;
        private FullBoe BOEMultiFalse = null;
        private Mock<IResourceDTODataLoader> resourceDTODataLoader = null;
        private Mock<IMoqTypeDataLoader> moqTypeDataLoader = null;
        private Mock<ICommonDataMapper> commonDataMapper = null;
        private LaborTypeAndSpreadImporter laborImporter;

        /// <summary>
        /// Initializes the test data.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            this.permissionsLoader = new Mock<IPermissionsDTODataLoader>();
            this.perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            this.resourceDTODataLoader = new Mock<IResourceDTODataLoader>();
            this.moqTypeDataLoader = new Mock<IMoqTypeDataLoader>();
            this.commonDataMapper = new Mock<ICommonDataMapper>();
            this.retriever = new Mock<IRetriever>();
            this.factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), this.factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPerformingOrgDTODataLoader), this.perfOrgLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), this.commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), this.retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), this.permissionsLoader.Object);
            BOEMultiFalse = new FullBoe()
            {
                Id = 1,

                IsMultiClinWbs = false
            };
            // setup loaders
            perfOrg = new PerformingOrgDTO() { Id = 1, PerformingOrgName = "SSC", PerformingOrgDesc = "SSC" };
            this.perfOrgLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(perfOrg);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullBoe>() { BOEMultiFalse });
            SetupSpreadCurves();
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullWbs>() { });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullClin>() { });

            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new List<PerformingOrgDTO>() { perfOrg });
            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(1)).Returns(new List<CustomFieldDTO>());
            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>());
            this.retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(new Collection<int>() { 55 })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>());
            this.retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(new Collection<int>() { 94802 })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>());
            this.retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(new Collection<int>() { 55, 94802, 94803 })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>());
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { CreateExistingCostTaskElement() });
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(2, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { CreateExistingHoursTaskElement() });

            // Setup for custom fields
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(4, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { CreateExistingHoursTaskElementWithCustomFields() });
            Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldDictionary = CreateCustomFieldDictionary();
            this.retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(new Collection<int>() { 65 })).Returns(customFieldDictionary);
            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(4)).Returns(CreateCustomFields());
            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(new List<int>() { 1, 2, 3 }, It.IsAny<int>())).Returns(CreateCustomFieldValues());

            ICollection<ResourceDTO> resources = CreateLaborResource();
            this.resourceDTODataLoader.Setup(x => x.GetByListId(It.IsAny<int>())).Returns(resources);

            foreach (ResourceDTO resource in resources)
            {
                this.resourceDTODataLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
            }

            laborImporter = new LaborTypeAndSpreadImporter(this.resourceDTODataLoader.Object, this.perfOrgLoader.Object, this.commonDataMapper.Object, this.moqTypeDataLoader.Object);
        }

        private void SetupSpreadCurves()
        {
            Collection<SpreadCurveModelView> spreadCurves = new Collection<SpreadCurveModelView>()
            {
                new SpreadCurveModelView() { SpreadCurveID = SpreadCurves.DiscreteCost, SpreadCurveName = "Discrete Cost" },
                new SpreadCurveModelView() { SpreadCurveID = SpreadCurves.DiscreteHours, SpreadCurveName = "Discrete Hours" },
                new SpreadCurveModelView() { SpreadCurveID = SpreadCurves.Level, SpreadCurveName = "Level" },
                new SpreadCurveModelView() { SpreadCurveID = SpreadCurves.SpreadCurve1, SpreadCurveName = "Curve 1" },
                new SpreadCurveModelView() { SpreadCurveID = SpreadCurves.SpreadCurve53, SpreadCurveName = "Curve 53" }
            };
            this.commonDataMapper.Setup(x => x.getSpreadCurve()).Returns(spreadCurves);
        }

        #region Private Methods

        private ICollection<CustomFieldValueDTO> CreateCustomFieldValues()
        {
            Collection<CustomFieldValueDTO> values = new Collection<CustomFieldValueDTO>()
            {
                new CustomFieldValueDTO() {
                    CustomFieldID = 1,
                    CustomFieldValueID = 1,
                    Id = 1,
                    CustomFieldValueName = "IAVA",
                    CustomFieldValueDescription = "something 1",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 1,
                    CustomFieldValueID = 2,
                    Id = 2,
                    CustomFieldValueName = "OM",
                    CustomFieldValueDescription = "Operations & Mainten",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 1,
                    CustomFieldValueID = 3,
                    Id = 3,
                    CustomFieldValueName = "UE",
                    CustomFieldValueDescription = "Unit Element",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 1,
                    CustomFieldValueID = 4,
                    Id = 4,
                    CustomFieldValueName = "I3",
                    CustomFieldValueDescription = "Just for Inc 3",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 1,
                    CustomFieldValueID = 13,
                    Id = 13,
                    CustomFieldValueName = "PP",
                    CustomFieldValueDescription = "Partially Prep",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 2,
                    CustomFieldValueID = 5,
                    Id = 5,
                    CustomFieldValueName = "BA",
                    CustomFieldValueDescription = "British Aerospace",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 2,
                    CustomFieldValueID = 6,
                    Id = 6,
                    CustomFieldValueName = "GD",
                    CustomFieldValueDescription = "General Dynamics",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 2,
                    CustomFieldValueID = 7,
                    Id = 7,
                    CustomFieldValueName = "NE",
                    CustomFieldValueDescription = "Norther Electirc",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 2,
                    CustomFieldValueID = 8,
                    Id = 8,
                    CustomFieldValueName = "RA",
                    CustomFieldValueDescription = "Raytheon",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 2,
                    CustomFieldValueID = 9,
                    Id = 9,
                    CustomFieldValueName = "TS",
                    CustomFieldValueDescription = "Top Sirloin",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 2,
                    CustomFieldValueID = 10,
                    Id = 10,
                    CustomFieldValueName = "OP",
                    CustomFieldValueDescription = "Oatmeal Porridge",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 2,
                    CustomFieldValueID = 11,
                    Id = 11,
                    CustomFieldValueName = "I3",
                    CustomFieldValueDescription = "Inc 3",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 2,
                    CustomFieldValueID = 12,
                    Id = 12,
                    CustomFieldValueName = "PP",
                    CustomFieldValueDescription = "Partially Prep",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO()
                {
                    CustomFieldID = 3,
                    CustomFieldValueID = 14,
                    CustomFieldValueName = "Open Ended",
                    CustomFieldValueDescription = "User entered value",
                    CustomFieldValueInUseFlag = true
                }
            };


            return values;
        }

        private ICollection<CustomFieldDTO> CreateCustomFields()
        {
            Collection<CustomFieldDTO> fields = new Collection<CustomFieldDTO>()
            {
                new CustomFieldDTO(){
                    Id = 1,
                    CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay,
                    CustomFieldName = "FC",
                    CustomFieldRequired = false,
                    WorkspaceID = 4
                },
                new CustomFieldDTO(){
                    Id = 2,
                    CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay,
                    CustomFieldName = "PC",
                    CustomFieldRequired = false,
                    WorkspaceID = 4
                },
                new CustomFieldDTO()
                {
                    Id = 3,
                    CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay,
                    CustomFieldName = "OE",
                    CustomFieldRequired = false,
                    IsOpenEnded = true,
                    WorkspaceID = 4
                }
            };

            return fields;
        }

        private static Dictionary<int, ICollection<KeyValuePair<int, int>>> CreateCustomFieldDictionary()
        {
            Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldDictionary = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
            customFieldDictionary.Add(65, new Collection<KeyValuePair<int, int>>() { new KeyValuePair<int, int>(1, 1), new KeyValuePair<int, int>(2, 5), new KeyValuePair<int, int>(3, 14) });
            return customFieldDictionary;
        }
        /// <summary>
        /// Creates a list of valid labor resources.
        /// </summary>
        /// <returns>List of labor resources.</returns>
        private ICollection<ResourceDTO> CreateLaborResource()
        {
            ResourceDTO resource1 = new ResourceDTO()
            {
                Id = 5,
                ResourceName = "ABCD",
                ResourceDesc = "Resource1",
                Segment = SegmentType.SSC,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Cost
            };

            ResourceDTO resource2 = new ResourceDTO()
            {
                Id = 6,
                ResourceName = "EFGH",
                ResourceDesc = "Resource2",
                Segment = SegmentType.SSC,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Hours
            };

            return new List<ResourceDTO>() { resource1, resource2 };
        }

      
        /// <summary>
        /// Creates the existing hours task element.
        /// </summary>
        /// <returns>The existing hours task element.</returns>
        private BoeTaskElementDTO CreateExistingHoursTaskElement()
        {
            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>() {
                new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 1,
                    LaborSpreadDate = DateTime.Parse("1/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("2/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("3/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("4/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("5/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("6/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                }
            };

            return new BoeTaskElementDTO()
            {
                BoeID = 1,
                Description = "taskElementDesc",
                EndDate = DateTime.Parse("6/15/2016").Normalize(),
                Id = 1,
                StartDate = DateTime.Parse("1/15/2016").Normalize(),
                TaskElementType = TaskElementType.Labor,
                TotalHours = 6,
                taskElementLabors = new Collection<ResourceTypeDto>() {
                    new ResourceTypeDto() {
                        Id = 94802,
                        BoeID = 1,
                        EndDateValue = DateTime.Parse("6/15/2016").Normalize(),
                        StartDateValue = DateTime.Parse("1/15/2016").Normalize(),
                        LaborSpreads = spreads,
                        PercentSpread = null,
                        PerformingOrgID = 1,
                        ResourceID = 6,
                        SpreadCurveID = SpreadCurves.DiscreteHours,
                        ValueSpread = 6,
                        PercentSpreadLocked = true,
                        HourSpreadLocked = true,
                        SpreadType = IES.Common.SpreadType.Hours
                    }
                }
            };
        }

        /// <summary>
        /// Create an existing hours Task Element with MOQ Types
        /// </summary>
        /// <returns></returns>
        private BoeTaskElementDTO CreateExistingHoursTaskElementWithMOQTypes()
        {
            BoeTaskElementDTO taskElement = this.CreateExistingHoursTaskElement();

            taskElement.taskElementLabors.First().MoqTypeSelectionId = (int)MOQType.Historical;

            ICollection<MoqTypeSelection> moqTypes = new Collection<MoqTypeSelection>()
            {
                new MoqTypeSelection()
                {
                    Id = 1,
                    BoeId = taskElement.BoeID,
                    TaskId = taskElement.Id,
                    SelectedMOQType = MOQType.Historical
                },
                new MoqTypeSelection()
                {
                    Id = 2,
                    BoeId = taskElement.BoeID,
                    TaskId = taskElement.Id,
                    SelectedMOQType = MOQType.AnalogousRelationships
                }
            };

            this.moqTypeDataLoader.Setup(x => x.GetByBoeId(It.IsAny<int>())).Returns(moqTypes);

            return taskElement;
        }

        /// <summary>
        /// Creates the existing hours task element for a MultiBOE
        /// </summary>
        /// <returns>The existing hours task element.</returns>
        private BoeTaskElementDTO CreateExistingHoursTaskElementMulti()
        {
            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>() {
                new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 1,
                    LaborSpreadDate = DateTime.Parse("1/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("2/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("3/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("4/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("5/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("6/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                }
            };

            return new BoeTaskElementDTO()
            {
                BoeID = 2,
                Description = "taskElementDesc",
                EndDate = DateTime.Parse("6/15/2016").Normalize(),
                Id = 1,
                StartDate = DateTime.Parse("1/15/2016").Normalize(),
                TaskElementType = TaskElementType.Labor,
                TotalHours = 6,
                taskElementLabors = new Collection<ResourceTypeDto>() {
                    new ResourceTypeDto() {
                        Id = 94802,
                        BoeID = 2,
                        EndDateValue = DateTime.Parse("6/15/2016").Normalize(),
                        StartDateValue = DateTime.Parse("1/15/2016").Normalize(),
                        LaborSpreads = spreads,
                        PercentSpread = null,
                        PerformingOrgID = 1,
                        ResourceID = 6,
                        SpreadCurveID = SpreadCurves.DiscreteHours,
                        ValueSpread = 6,
                        WBSID = 3,
                        PercentSpreadLocked = true,
                        HourSpreadLocked = true,
                        SpreadType = IES.Common.SpreadType.Hours
                    }
                }
            };
        }

        /// <summary>
        /// Creates the existing hours task element for a MultiBOE
        /// </summary>
        /// <returns>The existing hours task element.</returns>
        private BoeTaskElementDTO CreateExistingHoursTaskElementMultiNull()
        {
            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>() {
                new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 1,
                    LaborSpreadDate = DateTime.Parse("1/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("2/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("3/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("4/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("5/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("6/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                }
            };

            return new BoeTaskElementDTO()
            {
                BoeID = 2,
                Description = "taskElementDesc",
                EndDate = DateTime.Parse("6/15/2016").Normalize(),
                Id = 1,
                StartDate = DateTime.Parse("1/15/2016").Normalize(),
                TaskElementType = TaskElementType.Labor,
                TotalHours = 6,
                taskElementLabors = new Collection<ResourceTypeDto>() {
                    new ResourceTypeDto() {
                        Id = 94802,
                        BoeID = 2,
                        EndDateValue = DateTime.Parse("6/15/2016").Normalize(),
                        StartDateValue = DateTime.Parse("1/15/2016").Normalize(),
                        LaborSpreads = spreads,
                        PercentSpread = null,
                        PerformingOrgID = 1,
                        ResourceID = 6,
                        SpreadCurveID = SpreadCurves.DiscreteHours,
                        ValueSpread = 6,
                        PercentSpreadLocked = true,
                        HourSpreadLocked = true,
                        SpreadType = IES.Common.SpreadType.Hours
                    }
                }
            };
        }

        /// <summary>
        /// Creates the custom containers.
        /// </summary>
        /// <returns>Collection of custom field value containers.</returns>
        private static Collection<CustomFieldValueContainer> CreateCustomContainers()
        {
            DateTime updateDate = new DateTime(2015, 5, 5);
            Collection<CustomFieldValueContainer> containers = new Collection<CustomFieldValueContainer>()
            {
                new CustomFieldValueContainer() {
                    ContainerID = 1,
                    CustomFieldValueID = 1,
                    Id = 1,
                    Updateable = UpdateType.None,
                    UpdateDate = updateDate
                },
                new CustomFieldValueContainer() {
                    ContainerID = 2,
                    CustomFieldValueID = 5,
                    Id = 2,
                    Updateable = UpdateType.None,
                    UpdateDate = updateDate
                },
                new CustomFieldValueContainer()
                {
                    ContainerID = 3,
                    CustomFieldID = 3,
                    CustomFieldValueID = 14,
                    Id = 3,
                    Updateable = UpdateType.None,
                    UpdateDate = updateDate,
                    IsOpenEnded = true,
                    OpenEndedValue = "User entered value"
                }
            };

            return containers;
        }

        /// <summary>
        /// Creates the existing hours task element with custom fields.
        /// </summary>
        /// <returns>The existing hours task element with custom fields.</returns>
        private BoeTaskElementDTO CreateExistingHoursTaskElementWithCustomFields()
        {
            BoeTaskElementDTO taskElement = this.CreateExistingHoursTaskElement();
            taskElement.taskElementLabors.First().CustomFieldValueContainers = CreateCustomContainers();
            taskElement.taskElementLabors.First().Id = 65;
            return taskElement;
        }
        /// <summary>
        /// Creates the existing hours task element with custom fields for a MultiBOE
        /// </summary>
        /// <returns>The existing hours task element with custom fields.</returns>
        private BoeTaskElementDTO CreateExistingHoursTaskElementWithCustomFieldsMulti()
        {
            BoeTaskElementDTO taskElement = this.CreateExistingHoursTaskElementMulti();
            taskElement.taskElementLabors.First().CustomFieldValueContainers = CreateCustomContainers();
            taskElement.taskElementLabors.First().Id = 65;
            return taskElement;
        }

        /// <summary>
        /// Creates the existing hours task element.
        /// </summary>
        /// <returns>The existing hours task element.</returns>
        private BoeTaskElementDTO CreateExistingInvalidSpreadTotalHoursTaskElement()
        {
            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>() {
                new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 1,
                    LaborSpreadDate = DateTime.Parse("1/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("2/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("3/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("4/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("5/15/2016").Normalize(),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("6/15/2016").Normalize(),
                    LaborSpreadValue = 123456789123456,
                    LaborTypeId = 2
                }
            };

            return new BoeTaskElementDTO()
            {
                BoeID = 1,
                Description = "taskElementDesc",
                EndDate = DateTime.Parse("6/15/2016").Normalize(),
                Id = 1,
                StartDate = DateTime.Parse("1/15/2016").Normalize(),
                TaskElementType = TaskElementType.Labor,
                TotalHours = 6,
                taskElementLabors = new Collection<ResourceTypeDto>() {
                    new ResourceTypeDto() {
                        Id = 57,
                        BoeID = 1,
                        EndDateValue = DateTime.Parse("6/15/2016").Normalize(),
                        StartDateValue = DateTime.Parse("1/15/2016").Normalize(),
                        LaborSpreads = spreads,
                        PercentSpread = null,
                        PerformingOrgID = 1,
                        ResourceID = 6,
                        SpreadCurveID = SpreadCurves.DiscreteHours,
                        ValueSpread = 6,
                        PercentSpreadLocked = true,
                        HourSpreadLocked = true,
                        SpreadType = IES.Common.SpreadType.Hours
                    }
                }
            };
        }

        /// <summary>
        /// Creates the existing cost task element.
        /// </summary>
        /// <returns>The existing cost task element</returns>
        private BoeTaskElementDTO CreateExistingCostTaskElement()
        {
            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>() {
                new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 1,
                    LaborSpreadDate = DateTime.Parse("1/15/2016").Normalize(),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("2/15/2016").Normalize(),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("3/15/2016").Normalize(),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("4/15/2016").Normalize(),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("5/15/2016").Normalize(),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("6/15/2016").Normalize(),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                }
            };

            return new BoeTaskElementDTO()
            {
                BoeID = 1,
                Description = "taskElementDesc",
                EndDate = DateTime.Parse("6/15/2016").Normalize(),
                Id = 1,
                StartDate = DateTime.Parse("1/15/2016").Normalize(),
                TaskElementType = TaskElementType.Labor,
                TotalCost = 60,
                taskElementLabors = new Collection<ResourceTypeDto>() {
                    new ResourceTypeDto() {
                        Id = 55,
                        BoeID = 1,
                        EndDateValue = DateTime.Parse("6/15/2016").Normalize(),
                        StartDateValue = DateTime.Parse("1/15/2016").Normalize(),
                        LaborSpreads = spreads,
                        PercentSpread = null,
                        PerformingOrgID = 1,
                        ResourceID = 5,
                        SpreadCurveID = SpreadCurves.DiscreteCost,
                        ValueSpread = 60,
                        PercentSpreadLocked = true,
                        HourSpreadLocked = true,
                        SpreadType = IES.Common.SpreadType.Cost
                    }
                }
            };
        }

        /// <summary>
        /// Creates an existing cost task element with an invalid spread total.
        /// </summary>
        /// <returns>A Cost task element.</returns>
        private BoeTaskElementDTO CreateExistingInvalidSpreadTotalCostTaskElement()
        {
            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>() {
                new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 1,
                    LaborSpreadDate = DateTime.Parse("1/15/2016").Normalize(),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("2/15/2016").Normalize(),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("3/15/2016").Normalize(),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("4/15/2016").Normalize(),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("5/15/2016").Normalize(),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("6/15/2016").Normalize(),
                    LaborSpreadValue = 123456789123456,
                    LaborTypeId = 2
                }
            };

            return new BoeTaskElementDTO()
            {
                BoeID = 1,
                Description = "taskElementDesc",
                EndDate = DateTime.Parse("6/15/2016").Normalize(),
                Id = 1,
                StartDate = DateTime.Parse("1/15/2016").Normalize(),
                TaskElementType = TaskElementType.Labor,
                TotalCost = 60,
                taskElementLabors = new Collection<ResourceTypeDto>() {
                    new ResourceTypeDto() {
                        Id = 56,
                        BoeID = 1,
                        EndDateValue = DateTime.Parse("6/15/2016").Normalize(),
                        StartDateValue = DateTime.Parse("1/15/2016").Normalize(),
                        LaborSpreads = spreads,
                        PercentSpread = null,
                        PerformingOrgID = 1,
                        ResourceID = 5,
                        SpreadCurveID = SpreadCurves.DiscreteCost,
                        ValueSpread = 60,
                        PercentSpreadLocked = true,
                        HourSpreadLocked = true,
                        SpreadType = IES.Common.SpreadType.Cost
                    }
                }
            };
        }

        /// <summary>
        /// Creates a Full Workspace.
        /// </summary>
        /// <param name="isEp">Is the workspace using EP.</param>
        /// <param name="usingTemplateBOE">Is the Workspace using Template BOE</param>
        /// <returns>A Full Workspace</returns>
        private FullWorkspace CreateWorkspace(bool isEp = false, bool usingTemplateBOE = false)
        {
            return new FullWorkspace() {
                Id = 1,
                ResourceListID = 5,
                ResourceDecimalPrecision = 0,
                CostDecimalPrecision = 2,
                IsUsingEquivalentPerson = isEp,
                UsingTemplateBOE = usingTemplateBOE
            };
        }

        #endregion Private Methods

        #region Tests

        /// <summary>
        /// Tests Max Decimal Places.
        /// </summary>
        [TestMethod]
        public void LaborImport_MaxDecimalPlaces()
        {
            decimal num;
            Assert.IsTrue(ImportUtils.IsMaxDecimalPlaces("43.2", 2, out num));
            Assert.IsTrue(ImportUtils.IsMaxDecimalPlaces("43.2", 1, out num));
            Assert.IsFalse(ImportUtils.IsMaxDecimalPlaces("43.2", 0, out num));

            Assert.IsTrue(ImportUtils.IsMaxDecimalPlaces("43", 2, out num));
            Assert.IsTrue(ImportUtils.IsMaxDecimalPlaces("43", 1, out num));
            Assert.IsTrue(ImportUtils.IsMaxDecimalPlaces("43", 0, out num));

            Assert.IsTrue(ImportUtils.IsMaxDecimalPlaces("17.5", 2, out num));
            Assert.IsTrue(ImportUtils.IsMaxDecimalPlaces("17.5", 1, out num));
            Assert.IsFalse(ImportUtils.IsMaxDecimalPlaces("17.5", 0, out num));
        }

        /// <summary>
        /// Tests Labor import with custom fields.
        /// </summary>
        [TestMethod]
        public void LaborImport_CustomFields_NoSelection()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursCustomFieldsNoSelection))
            {
                FullWorkspace workspace = this.CreateWorkspace();
                workspace.Id = 4;
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElementWithCustomFields(), workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.AddLaborType)));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.UpdateLaborType)));
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));

            Collection<CustomFieldValueContainer> firstRowContainers = result.First().CustomFieldValueContainers;
            Collection<CustomFieldValueContainer> secondRowContainers = result.Last().CustomFieldValueContainers;

            // first row is an existing row, with 2 updates for a non-selected field
            Assert.AreEqual(2, firstRowContainers.Count(c => c.Updateable != UpdateType.None));
            Assert.AreEqual(2, firstRowContainers.Count(c => c.Updateable == UpdateType.Deleted));
            // second row is a new row, with 1 custom field selected
            Assert.AreEqual(1, secondRowContainers.Count(c => c.Updateable != UpdateType.None));

            Assert.AreEqual(-1, firstRowContainers.First(c => c.Updateable != UpdateType.None).CustomFieldValueID);
            Assert.AreEqual(2, firstRowContainers.First(c => c.Updateable != UpdateType.None).ContainerID);
        }
        
        /// <summary>
        /// Tests Labor import with custom fields.
        /// </summary>
        [TestMethod]
        public void LaborImport_CustomFields()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursCustomFields))
            {
                FullWorkspace workspace = this.CreateWorkspace();
                workspace.Id = 4;
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElementWithCustomFields(), workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.AddLaborType)));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.UpdateLaborType)));
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
            
            Collection<CustomFieldValueContainer> firstRowContainers = result.First().CustomFieldValueContainers;
            Collection<CustomFieldValueContainer> secondRowContainers = result.Last().CustomFieldValueContainers;

            // first row is an existing row, with 2 update and 1 non-update custom field
            Assert.AreEqual(2, firstRowContainers.Count(c => c.Updateable != UpdateType.None));
            // second row is a new row, with 3 custom fields
            Assert.AreEqual(3, secondRowContainers.Count(c => c.Updateable != UpdateType.None));

            Assert.AreEqual(6, firstRowContainers.First(c => c.Updateable != UpdateType.None).CustomFieldValueID);
            Assert.AreEqual(2, firstRowContainers.First(c => c.Updateable != UpdateType.None).ContainerID);
        }

        /// <summary>
        /// Test Labor Import for labor with MOQ Types
        /// </summary>
        [TestMethod]
        public void LaborImport_MOQTypes()
        {
            ICollection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursMoqTypes))
            {
                FullWorkspace workspace = this.CreateWorkspace(false, true);
                workspace.Id = 4;

                BoeTaskElementDTO taskElement = this.CreateExistingHoursTaskElementWithMOQTypes();

                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, taskElement, workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.AddLaborType)));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.UpdateLaborType)));
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
        }

        /// <summary>
        /// Test Labor Import for labor with MOQ Types
        /// </summary>
        [TestMethod]
        public void LaborImport_MOQTypes_Missing()
        {
            ICollection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursMoqTypesMissing))
            {
                FullWorkspace workspace = this.CreateWorkspace(false, true);
                workspace.Id = 4;

                BoeTaskElementDTO taskElement = this.CreateExistingHoursTaskElementWithMOQTypes();

                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, taskElement, workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.MoqTypeMissingOrInvalid)));
        }

        /// <summary>
        /// Test Labor Import for labor with MOQ Types
        /// </summary>
        [TestMethod]
        public void LaborImport_MOQTypes_Invalid()
        {
            ICollection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursMoqTypesInvalid))
            {
                FullWorkspace workspace = this.CreateWorkspace(false, true);
                workspace.Id = 4;

                BoeTaskElementDTO taskElement = this.CreateExistingHoursTaskElementWithMOQTypes();

                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, taskElement, workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.MoqTypeMissingOrInvalid)));
        }

        /// <summary>
        /// Tests Labor import with Multi enabled, WBS
        /// </summary>
        [TestMethod]
        public void LaborImport_WithMulti_WBS()
        {
           FullBoe BOEMultiTrue = new FullBoe()
            {
                Id = 2,
                IsMultiClinWbs = true
            };
           this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(4)).Returns(new Collection<FullBoe>() { BOEMultiTrue });
           this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(4)).Returns(new Collection<FullWbs>() { new FullWbs() { Id = 2, WbsNumber = "01", WbsTitle = "title" }, new FullWbs() { Id = 3, WbsNumber = "02", WbsTitle = "title2" } });
           this.retriever.Setup(x => x.GetClinsByWorkspaceId(4)).Returns(new Collection<FullClin>() { });

            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMultiWBS))
            {
                FullWorkspace workspace = this.CreateWorkspace();
                workspace.Id = 4;
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElementMulti(), workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.AddLaborType)));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.UpdateLaborType)));
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
            Assert.AreEqual(2, result.First().WBSID);
            Assert.AreEqual(2, result.Last().WBSID);
            Assert.AreEqual(null, result.First().CLINID);
            Assert.AreEqual(null, result.Last().CLINID);

        }

        /// <summary>
        /// Tests Labor import with Multi enabled, WBS
        /// </summary>
        [TestMethod]
        public void LaborImport_InvalidSpreadCurve()
        {
            FullBoe BOEMultiTrue = new FullBoe()
            {
                Id = 2,
                IsMultiClinWbs = true
            };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(4)).Returns(new Collection<FullBoe>() { BOEMultiTrue });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(4)).Returns(new Collection<FullWbs>() { new FullWbs() { Id = 2, WbsNumber = "01", WbsTitle = "title" }, new FullWbs() { Id = 3, WbsNumber = "02", WbsTitle = "title2" } });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(4)).Returns(new Collection<FullClin>() { });
            this.commonDataMapper.Setup(x => x.getSpreadCurve()).Returns(new Collection<SpreadCurveModelView>());

            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMultiWBS))
            {
                FullWorkspace workspace = this.CreateWorkspace();
                workspace.Id = 4;
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElementMulti(), workspace, false);
            }

            // Change the spread curves back after running import
            this.SetupSpreadCurves();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.MissingData)));
            
        }

        /// <summary>
        /// Tests Labor import with Multi enabled, WBS
        /// </summary>
        [TestMethod]
        public void LaborImport_WithMulti_BadWBS()
        {
            FullBoe BOEMultiTrue = new FullBoe()
            {
                Id = 2,
                IsMultiClinWbs = true
            };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(4)).Returns(new Collection<FullBoe>() { BOEMultiTrue });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(4)).Returns(new Collection<FullWbs>() { new FullWbs() { Id = 3, WbsNumber = "03", WbsTitle = "title3" }, new FullWbs() { Id = 3, WbsNumber = "02", WbsTitle = "title2" } });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(4)).Returns(new Collection<FullClin>() { });

            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMultiWBS))
            {
                FullWorkspace workspace = this.CreateWorkspace();
                workspace.Id = 4;
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElementMulti(), workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.First().ImportTypes.Contains(LaborTypeImportResult.ResourceMultiValuesInvalid));

        }
        /// <summary>
        /// Tests Labor import with Multi enabled, Clin
        /// </summary>
        [TestMethod]
        public void LaborImport_WithMulti_Clin()
        {
            FullBoe BOEMultiTrue = new FullBoe()
            {
                Id = 2,
                IsMultiClinWbs = true
            };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(4)).Returns(new Collection<FullBoe>() { BOEMultiTrue });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(4)).Returns(new Collection<FullWbs>() { });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(4)).Returns(new Collection<FullClin>() { new FullClin() {Id =2, ClinNumber ="01", ClinTitle = "title"}});

            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMultiClin))
            {
                FullWorkspace workspace = this.CreateWorkspace();
                workspace.Id = 4;
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElementMulti(), workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.AddLaborType)));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.UpdateLaborType)));
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
            Assert.AreEqual(2, result.First().CLINID);
            Assert.AreEqual(2, result.Last().CLINID);
            Assert.AreEqual(null, result.First().WBSID);
            Assert.AreEqual(null, result.Last().WBSID);
        }

        /// <summary>
        /// Tests Labor import with Multi enabled, CustomFields
        /// </summary>
        [TestMethod]
        public void LaborImport_CustomFields_WithMulti()
        {
            FullBoe BOEMultiTrue = new FullBoe()
            {
                Id = 2,
                IsMultiClinWbs = true
            };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(4)).Returns(new Collection<FullBoe>() { BOEMultiTrue });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(4)).Returns(new Collection<FullWbs>() { new FullWbs() { Id = 2, WbsNumber = "01", WbsTitle = "title" }, new FullWbs() { Id = 3, WbsNumber = "02", WbsTitle = "title2" } });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(4)).Returns(new Collection<FullClin>() { new FullClin() { Id = 2, ClinNumber = "01", ClinTitle = "title" } });

            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.laborimportmulticustomfields))
            {
                FullWorkspace workspace = this.CreateWorkspace();
                workspace.Id = 4;
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElementWithCustomFieldsMulti(), workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.AddLaborType)));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.UpdateLaborType)));
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
            Assert.AreEqual(null, result.First().CLINID);
            Assert.AreEqual(2, result.Last().CLINID);
            Assert.AreEqual(2, result.First().WBSID);
            Assert.AreEqual(null, result.Last().WBSID);

            Collection<CustomFieldValueContainer> firstRowContainers = result.First().CustomFieldValueContainers;
            Collection<CustomFieldValueContainer> secondRowContainers = result.Last().CustomFieldValueContainers;

            // first row is an existing row, with 1 update and 2 non-update custom fields
            Assert.AreEqual(1, firstRowContainers.Count(c => c.Updateable != UpdateType.None));
            // second row is a new row, with 3 custom fields
            Assert.AreEqual(3, secondRowContainers.Count(c => c.Updateable != UpdateType.None));

            Assert.AreEqual(6, firstRowContainers.First(c => c.Updateable != UpdateType.None).CustomFieldValueID);
            Assert.AreEqual(2, firstRowContainers.First(c => c.Updateable != UpdateType.None).ContainerID);
        }

        /// <summary>
        /// Tests Labor import with Multi enabled, Null
        /// </summary>
        [TestMethod]
        public void LaborImport_WithMulti_Null()
        {
            FullBoe BOEMultiTrue = new FullBoe()
            {
                Id = 2,
                IsMultiClinWbs = true
            };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(4)).Returns(new Collection<FullBoe>() { BOEMultiTrue });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(4)).Returns(new Collection<FullWbs>() { });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(4)).Returns(new Collection<FullClin>() { new FullClin() { Id = 2, ClinNumber = "01", ClinTitle = "title" } });

            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMultiNull))
            {
                FullWorkspace workspace = this.CreateWorkspace();
                workspace.Id = 4;
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElementMultiNull(), workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.AddLaborType)));
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.UpdateLaborType)));
            Assert.AreEqual(true, result.First().ImportTypes.Contains(LaborTypeImportResult.ResourceMultiValuesInvalid));
            Assert.AreEqual(true, result.Last().ImportTypes.Contains(LaborTypeImportResult.ResourceMultiValuesInvalid));

            Assert.AreEqual(null, result.First().CLINID);
            Assert.AreEqual(null, result.Last().CLINID);
            Assert.AreEqual(null, result.First().WBSID);
            Assert.AreEqual(null, result.Last().WBSID);
        }

        /// <summary>
        /// Tests Labor import with Multi enabled, Null
        /// </summary>
        [TestMethod]
        public void LaborImport_WithMulti_BadClin()
        {
            FullBoe BOEMultiTrue = new FullBoe()
            {
                Id = 2,
                IsMultiClinWbs = true
            };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(4)).Returns(new Collection<FullBoe>() { BOEMultiTrue });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(4)).Returns(new Collection<FullWbs>() { });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(4)).Returns(new Collection<FullClin>() { new FullClin() { Id = 3, ClinNumber = "03", ClinTitle = "title3" } });

            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMultiClin))
            {
                FullWorkspace workspace = this.CreateWorkspace();
                workspace.Id = 4;
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElementMulti(), workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.First().ImportTypes.Contains(LaborTypeImportResult.ResourceMultiValuesInvalid));
        }

        /// <summary>
        /// Tests Labor import with custom fields.
        /// </summary>
        [TestMethod]
        public void LaborImport_CustomFields_NoChange()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursCustomFieldsNoChange))
            {
                FullWorkspace workspace = this.CreateWorkspace();
                workspace.Id = 4;
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElementWithCustomFields(), workspace, false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.AddLaborType)));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.UnChanged)));
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));

            Collection<CustomFieldValueContainer> firstRowContainers = result.First().CustomFieldValueContainers;
            Collection<CustomFieldValueContainer> secondRowContainers = result.Last().CustomFieldValueContainers;

            // first row is an existing row, with 3 non-update custom field
            Assert.AreEqual(0, firstRowContainers.Count(c => c.Updateable != UpdateType.None));
            // second row is a new row, with 3 custom fields
            Assert.AreEqual(3, secondRowContainers.Count(c => c.Updateable != UpdateType.None));
        }

        /// <summary>
        /// Tests Labor Import with a file that has a bad format.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotExcelFileException))]
        public void LaborImportResult_BadFileFormat()
        {
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImporterBadFormat))
            {
                this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }
        }

        /// <summary>
        /// Tests Labor Import with a file that is not an excel file. 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotExcelFileException))]
        public void LaborImportResult_NotExcelFile()
        {
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImporterNotExcel))
            {
                this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }
        }

        /// <summary>
        /// Tests Labor Import with a null filename.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LaborImportResult_NullFile()
        {
            this.laborImporter.ImportLaborTypeFromExcelFile(null, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
        }

        /// <summary>
        /// Tests Labor Import with a null task element parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LaborImportResult_NullTaskElement()
        {
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHours))
            {
                this.laborImporter.ImportLaborTypeFromExcelFile(file, null, this.CreateWorkspace(), true);
            }
        }

        /// <summary>
        /// Tests Labor Import with a null workspace parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LaborImportResult_NullWorkspace()
        {
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHours))
            {
                this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), null, true);
            }
        }

        /// <summary>
        /// Tests Labor Import with a null rows parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LaborImportResult_NoRows()
        {
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportNoRows))
            {
                this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), null, true);
            }
        }

        /// <summary>
        /// Tests Labor Import with invalid dates.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(DateImportException))]
        public void LaborImport_InvalidDates()
        {
            // Start is after the end
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursInvalidDates))
            {
                this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }
        }

        /// <summary>
        /// Tests Labor Import with missing start date.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(DateImportException))]
        public void LaborImport_MissingStartDate()
        {
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMissingStartDate))
            {
                this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }
        }

        /// <summary>
        /// Tests Labor Import with missing end date.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(DateImportException))]
        public void LaborImport_MissingEndDate()
        {
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMissingEndDate))
            {
                this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with no change, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_NoChange_NewOnly()
        {
            Collection<ImportedLaborType> result;
            
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHours))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with no change.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_NoChange_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHours))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with EPs labor resource with no change.
        /// </summary>
        [TestMethod]
        public void LaborImport_EPs_NoChange_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            FullObjectHelper.RefreshEPForTests();
            ConfigurationManager.AppSettings["ShowEquivalentPersonsOption"] = "true";
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportEPs))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(true), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));

            FullObjectHelper.RefreshEPForTests();
            ConfigurationManager.AppSettings["ShowEquivalentPersonsOption"] = "false";
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with no change, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_NoChange_NewOnly()
        {
            Collection<ImportedLaborType> result;

            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCost))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with no change.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_NoChange_IncludeExisting()
        {
            Collection<ImportedLaborType> result;

            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCost))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid resource.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidResource()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursInvalidResource))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.MissingData)));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid performing org.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidPerformingOrg()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursInvalidPerfOrg))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.InvalidData)));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid start date.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidStart()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursInvalidStart))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.LaborTypeDateOutsideOfPOPDateRange)));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid end date.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidEnd()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursInvalidEnd))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.LaborTypeDateOutsideOfPOPDateRange)));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with missing resource.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_MissingResource()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMissingResource))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.MissingData)));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with missing performing Org.
        /// </summary>
        [TestMethod]
        public void LaborImport_EPs_MissingPerformingOrgAndEPs()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMissingPerfOrg))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.MissingData)));
        }
        
        /// <summary>
         /// Tests Labor Import with Hours labor resource with missing performing Org.
         /// </summary>
        [TestMethod]
        public void LaborImport_Hours_MissingPerformingOrg()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMissingPerfOrg))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.MissingData)));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with missing spread curve.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_MissingSpreadCurve()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportMissingSpreadCurve))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.MissingData)));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with spread data that has a sum that is too large, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidSpreadData_LargerSum_NewOnly()
        {
            // No warnings in return if the sum of the total hours is larger than allowed
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataLargerSum))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with spread data that has a sum that is too large.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidSpreadData_LargerSum_IncludeExisting()
        {
            // No warnings in return if the sum of the total hours is larger than allowed
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataLargerSum))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with missing spread data, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_MissingSpreadData_Values_NewOnly()
        {
            // No warning since missing spread values are treated as zeros
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataMissingValue))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with missing spread data, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_MissingSpreadData_Values_IncludeExisting()
        {
            // No warning since missing spread values are treated as zeros
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataMissingValue))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with missing spread month.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_MissingSpreadData_MissingMonth_IncludeExisting()
        {
            // no warning since missing months are treated as zeros
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataMissingMonth))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with missing spread month, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_MissingSpreadData_MissingMonth_NewOnly()
        {
            // no warning since missing months are treated as zeros
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataMissingMonth))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with extra spread month data.  The import should fail.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidSpreadData_ExtraMonth_IncludeExisting()
        {
            // months outside of start/end date should cause import to fail
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataExtraMonth))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.SpreadMonthColumnInvalid)));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with extra spread month data, only importing new rows.  The import should fail.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidSpreadData_ExtraMonth_NewOnly()
        {
            // months outside of start/end date should cause import to fail
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataExtraMonth))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.SpreadMonthColumnInvalid)));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid spread month data.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidSpreadData_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportInvalidSpreadData))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.DecimalPrecisionViolation))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid spread month data, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidSpreadData_NewOnly()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportInvalidSpreadData))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.DecimalPrecisionViolation))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid spread month data that is too long.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidSpreadData_TooLong_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataTooLong))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.ResourceHourValueTooLarge))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid spread month data that is too long, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidSpreadData_TooLong_NewOnly()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataTooLong))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.ResourceHourValueTooLarge))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid spread month data that has decimals too long.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidSpreadData_DecimalsTooLong_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataDecimalsTooLong))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.DecimalPrecisionViolation)))); 
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid spread month data that has decimals too long, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidSpreadData_DecimalsTooLong_NewOnly()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataDecimalsTooLong))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UnChanged)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.DecimalPrecisionViolation)))); 
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with change required for curve and rate type, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_ChangeCurveAndRateType_NewOnly()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursChangeCurveAndRateType))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.RateTypeSpreadTypeAgreement)));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with change required for curve and rate type.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_ChangeCurveAndRateType_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursChangeCurveAndRateType))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.RateTypeSpreadTypeAgreement)));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with curve changed, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_ChangeCurve_NewOnly()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursChangeCurve))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));

            Assert.AreEqual(0, result.First().ImportedLaborSpreads.Count); // there should be no spreadmonths for the new task because they are calculated after the import
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with curve changed.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_ChangeCurve_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            BoeTaskElementDTO hoursElement = this.CreateExistingHoursTaskElement();
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursChangeCurve))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, hoursElement, this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));

            ImportedLaborType laborType = result.First(lt => lt.BoeID == hoursElement.BoeID);
            Assert.AreEqual(0, laborType.ImportedLaborSpreads.Count(ls => ls.ImportTypes.Contains(LaborSpreadImportResult.UpdateSpread))); // no laborspread updates by changing to Curve 53 because labor spreads are recalculated after import
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with percentage updated (no hours).
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_PercentageOnly_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            BoeTaskElementDTO hoursElement = this.CreateExistingHoursTaskElement();
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursPercentage))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, hoursElement, this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));

            ImportedLaborType laborType = result.First(lt => lt.BoeID == hoursElement.BoeID);
            Assert.AreEqual(50m, laborType.PercentSpread);
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with cost decimal precision problem, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_CostDecimalPrecision_NewOnly()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostDecimalPrecision))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.CostDecimalPrecisionViolation)));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with cost decimal precision problem.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_CostDecimalPrecision_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostDecimalPrecision))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.CostDecimalPrecisionViolation)));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with missing cost, only importing new rows.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ColumnMissingException))]
        public void LaborImport_Cost_MissingCost_NewOnly()
        {
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostMissingCost))
            {
                this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), true);
            }
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with missing cost.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ColumnMissingException))]
        public void LaborImport_Cost_MissingCost_IncludeExisting()
        {
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostMissingCost))
            {
                this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), false);
            }
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with cost spread decimal precision problem, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_CostSpread_CostDecimalPrecision_NewOnly()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostSpreadDecimalPrecision))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.CostDecimalPrecisionViolation))));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with cost spread decimal precision problem.
        /// </summary>
        [TestMethod]
        public void LaborImport_CostSpread_CostDecimalPrecision_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostSpreadDecimalPrecision))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.CostDecimalPrecisionViolation))));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with cost too large, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_CostTooLarge_NewOnly()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostTooLarge))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            // TODO This is currently allowed (Cost column can be larger than 12 digits)
            //Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.ResourceCostValueTooLarge)));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with cost too large.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_CostTooLarge_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostTooLarge))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            // TODO This is currently allowed (Cost column can be larger than 12 digits)
            //Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.ResourceCostValueTooLarge)));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with cost spread value too large, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_CostSpreadTooLarge_NewOnly()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostSpreadTooLarge))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.ResourceCostValueTooLarge))));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with cost spread value too large.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_CostSpreadTooLarge_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostSpreadTooLarge))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.ResourceCostValueTooLarge))));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with cost spread decimal precision problem, only importing new rows.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_CostSpreadDecimalPrecision_NewOnly()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostSpreadDecimalPrecision))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), true);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.CostDecimalPrecisionViolation))));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with cost spread decimal precision problem.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_CostSpreadDecimalPrecision_IncludeExisting()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostSpreadDecimalPrecision))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingCostTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.CostDecimalPrecisionViolation))));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with the original resource (from DB but Mocked) with an invalid Total spread.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_InvalidOriginalTotalSpread()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostInvalidTotal))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingInvalidSpreadTotalCostTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.ResourceCostValueTooLarge))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with the original resource (from DB but Mocked) with an invalid Total spread.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidOriginalTotalSpread()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursInvalidTotal))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingInvalidSpreadTotalHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it == LaborSpreadImportResult.ResourceHourValueTooLarge))));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid decimal value for spread value.
        /// </summary>
        [TestMethod]
        public void LaborImport_Hours_InvalidSpreadDecimal()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportHoursSpreadInvalidDecimal))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any(ls => ls.ImportTypes.Any(it => it == LaborSpreadImportResult.DecimalPrecisionViolation))));
        }


        /// <summary>
        /// Tests Labor Import with Cost labor resource with an invalid spread range.
        /// </summary>
        [TestMethod]
        public void LaborImport_Cost_InvalidSpreadRange()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportCostSpreadRangeInvalid))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingInvalidSpreadTotalCostTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.CostSpreadRangeInvalid)));
        }

        /// <summary>
        /// Tests Labor Import with Cost labor resource with an invalid spread range.
        /// </summary>
        [TestMethod]
        public void LaborImport_InvalidResourceId()
        {
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportInvalidResourceId))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingInvalidSpreadTotalCostTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.ResourceTypeIDMissingOrInvalid)));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with invalid spread month value (outside spread start/end date range).
        /// </summary>
        [TestMethod]
        public void LaborImportSpreadData_InvalidSpreadMonthValue()
        {
            // Test case with 0 value for month outside spread start/end date range
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataInvalidStartMonth0))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.SpreadMonthValueOutsideDateRange)));

            // Test case with non-zero value for month outside spread start/end date range
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataInvalidStartMonth))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(lt => lt.ImportTypes.Any(it => it == LaborTypeImportResult.SpreadMonthValueOutsideDateRange)));
        }

        /// <summary>
        /// Tests Labor Import with Hours labor resource with valid spread month value (values outside spread start/end date range are empty).
        /// </summary>
        [TestMethod]
        public void LaborImportSpreadData_ValidSpreadMonthValue()
        {
            // Test case with empty value for month outside spread start/end date range (should pass, since value is empty)
            Collection<ImportedLaborType> result;
            using (MemoryStream file = new MemoryStream(Properties.Resources.LaborImportSpreadDataValidStartMonth))
            {
                result = this.laborImporter.ImportLaborTypeFromExcelFile(file, this.CreateExistingHoursTaskElement(), this.CreateWorkspace(), false);
            }

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(lt => lt.ImportTypes.Any(it => it != LaborTypeImportResult.AddLaborType && it != LaborTypeImportResult.UpdateLaborType)));
            Assert.IsTrue(result.Any(l => l.ImportedLaborSpreads.Any()));
            Assert.IsFalse(result.Any(l => l.ImportedLaborSpreads.Any(lt => lt.ImportTypes.Any(it => it != LaborSpreadImportResult.UpdateSpread))));
        }

        #endregion Tests
    }
}
