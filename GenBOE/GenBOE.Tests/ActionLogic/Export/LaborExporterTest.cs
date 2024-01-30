// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Import;
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
    public class LaborExporterTest
    {
        #region private Fields
        private Mock<IPermissionsDTODataLoader> permissionsLoader = null;
        private Mock<IRetriever> retriever = null;
        private Mock<IPerformingOrgDTODataLoader> perfOrgLoader = null;
        private Mock<ResourceDTODataLoader> resourceDTODataLoader = null;
        private Mock<ICommonDataMapper> commonDataMapper = null;
        private FullBoe BOEMultiFalse = null;
        private Mock<IFullObjectFactory> factory = null;

        private string templatePath;
        
        #endregion private Fields
        /// <summary>
        /// Initializes the test data.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            this.permissionsLoader = new Mock<IPermissionsDTODataLoader>();
            this.perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
            this.resourceDTODataLoader = new Mock<ResourceDTODataLoader>();
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
            foreach (PerformingOrgDTO perfOrg in CreatePerformingOrgs())
            {
                this.perfOrgLoader.Setup(x => x.GetById(perfOrg.Id)).Returns(perfOrg);
                
            }

            Collection<SpreadCurveModelView> spreadCurves = new Collection<SpreadCurveModelView>() 
            {
                new SpreadCurveModelView() { SpreadCurveID = SpreadCurves.DiscreteCost, SpreadCurveName = "Discrete Cost" },
                new SpreadCurveModelView() { SpreadCurveID = SpreadCurves.DiscreteHours, SpreadCurveName = "Discrete Hours" },
                new SpreadCurveModelView() { SpreadCurveID = SpreadCurves.Level, SpreadCurveName = "Level" },
                new SpreadCurveModelView() { SpreadCurveID = SpreadCurves.SpreadCurve1, SpreadCurveName = "Curve 1" },
                new SpreadCurveModelView() { SpreadCurveID = SpreadCurves.SpreadCurve13, SpreadCurveName = "Curve 13" }
            };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { BOEMultiFalse });

            this.commonDataMapper.Setup(x => x.getSpreadCurve()).Returns(spreadCurves);
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullWbs>() { });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullClin>() { });
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(CreatePerformingOrgs());
            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(1)).Returns(new List<CustomFieldDTO>());
            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(2)).Returns(new List<CustomFieldDTO>());
            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(3)).Returns(new List<CustomFieldDTO>());
            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(5)).Returns(new List<CustomFieldDTO>());

            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>());
            this.retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(new Collection<int>() { 55 })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>());
            this.retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(new Collection<int>() { 94802 })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>());
            this.retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(new Collection<int>() { 55, 94802, 94803 })).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>());
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { CreateExistingCostTaskElement() });
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(2, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { CreateExistingHoursTaskElement() });
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(3, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { CreateCombinedTaskElement() });
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(5, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { CreateExistingCostCustomFieldMultiTaskElement() });


            // Setup for custom fields
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(4, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { CreateExistingCostCustomFieldTaskElement() });
            Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldDictionary = CreateCustomFieldDictionary();
            this.retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(new Collection<int>() { 56 })).Returns(customFieldDictionary);
            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(4)).Returns(CreateCustomFields(4));
            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(5)).Returns(CreateCustomFields(5));

            this.retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(new List<int>() { 1, 2, 3}, It.IsAny<int>())).Returns(CreateCustomFieldValues());

            ICollection<ResourceDTO> resources = CreateLaborResources();
            this.resourceDTODataLoader.Setup(x => x.GetByListId(It.IsAny<int>())).Returns(resources);
            this.resourceDTODataLoader.Setup(x => x.GetByListIdAndElementOfCost(It.IsAny<int>(), It.IsAny<ICollection<ElementOfCostType>>())).Returns(resources);

            foreach (ResourceDTO resource in resources)
            {
                this.resourceDTODataLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
            }
            
            

            // save a copy of the Excel template file to template Path
            templatePath = Path.Combine(System.Environment.CurrentDirectory, Path.GetRandomFileName() + ".xlsx");
            System.Diagnostics.Debug.WriteLine("Template path is " + templatePath);
            File.WriteAllBytes(templatePath, Properties.Resources.LaborTypesAndSpread);
        }

        #region Private Methods

        private static Dictionary<int, ICollection<KeyValuePair<int, int>>> CreateCustomFieldDictionary()
        {
            Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldDictionary = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
            customFieldDictionary.Add(56, new Collection<KeyValuePair<int, int>>() { new KeyValuePair<int, int>(1, 2), new KeyValuePair<int, int>(2, 4), new KeyValuePair<int, int>(3, 5) });
            return customFieldDictionary;
        }

        private ICollection<CustomFieldValueDTO> CreateCustomFieldValues()
        {
            Collection<CustomFieldValueDTO> values = new Collection<CustomFieldValueDTO>()
            {
                new CustomFieldValueDTO() {
                    CustomFieldID = 1,
                    CustomFieldValueID = 1,
                    Id = 1,
                    CustomFieldValueName = "Splat",
                    CustomFieldValueDescription = "Flat",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 1,
                    CustomFieldValueID = 2,
                    Id = 2,
                    CustomFieldValueName = "Cat",
                    CustomFieldValueDescription = "Mat",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 2,
                    CustomFieldValueID = 3,
                    Id = 3,
                    CustomFieldValueName = "Whoosh",
                    CustomFieldValueDescription = "Sploosh",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO() {
                    CustomFieldID = 2,
                    CustomFieldValueID = 4,
                    Id = 4,
                    CustomFieldValueName = "Shot",
                    CustomFieldValueDescription = "Put",
                    CustomFieldValueInUseFlag = true
                },
                new CustomFieldValueDTO()
                {
                    CustomFieldID = 3,
                    CustomFieldValueID = 5,
                    Id = 5,
                    CustomFieldValueName = "Open Ended",
                    CustomFieldValueDescription = "User entered value",
                    CustomFieldValueInUseFlag = true
                }
            };

            return values;
        }

        private ICollection<CustomFieldDTO> CreateCustomFields(int workspaceId)
        {
            Collection<CustomFieldDTO> fields = new Collection<CustomFieldDTO>()
            {
                new CustomFieldDTO(){
                    Id = 1,
                    CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay,
                    CustomFieldName = "First Custom Field",
                    CustomFieldRequired = false,
                    WorkspaceID = workspaceId
                },
                new CustomFieldDTO(){
                    Id = 2,
                    CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay,
                    CustomFieldName = "Second Custom Field",
                    CustomFieldRequired = false,
                    WorkspaceID = workspaceId
                },
                new CustomFieldDTO()
                {
                    Id = 3,
                    CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay,
                    CustomFieldName = "Third Custom Field",
                    CustomFieldRequired = false,
                    IsOpenEnded = true,
                    WorkspaceID = workspaceId
                }
            };

            return fields;
        }


        /// <summary>
        /// Creates the performing orgs collection.
        /// </summary>
        /// <returns>A performing orgs collection.</returns>
        private static ICollection<PerformingOrgDTO> CreatePerformingOrgs()
        {
            PerformingOrgDTO perfOrg = new PerformingOrgDTO() { Id = 1, PerformingOrgName = "SSC", PerformingOrgDesc = "SSC" };

            return new List<PerformingOrgDTO>() { perfOrg };
        }

        /// <summary>
        /// Creates a list of valid labor resources.
        /// </summary>
        /// <returns>List of labor resources.</returns>
        private static ICollection<ResourceDTO> CreateLaborResources()
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
        /// Creates the existing cost task element.
        /// </summary>
        /// <returns>The existing cost task element</returns>
        private BoeTaskElementDTO CreateExistingCostTaskElement()
        {
            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>() {
                new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 1,
                    LaborSpreadDate = DateTime.Parse("1/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("2/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("3/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("4/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("5/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("6/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                }
            };

            return new BoeTaskElementDTO()
            {
                BoeID = 1,
                Description = "taskElementDesc",
                EndDate = DateTime.Parse("6/15/2016"),
                Id = 1,
                StartDate = DateTime.Parse("1/15/2016"),
                TaskElementType = TaskElementType.Labor,
                TotalCost = 60,
                taskElementLabors = new Collection<ResourceTypeDto>() {
                    new ResourceTypeDto() {
                        Id = 55,
                        BoeID = 1,
                        EndDateValue = DateTime.Parse("6/15/2016"),
                        StartDateValue = DateTime.Parse("1/15/2016"),
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
        /// Creates the existing cost task element.
        /// </summary>
        /// <returns>The existing cost task element</returns>
        private BoeTaskElementDTO CreateExistingCostCustomFieldTaskElement()
        {
            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>() {
                new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 1,
                    LaborSpreadDate = DateTime.Parse("1/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("2/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("3/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("4/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("5/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("6/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                }
            };

            return new BoeTaskElementDTO()
            {
                BoeID = 1,
                Description = "taskElementDesc",
                EndDate = DateTime.Parse("6/15/2016"),
                Id = 4,
                StartDate = DateTime.Parse("1/15/2016"),
                TaskElementType = TaskElementType.Labor,
                TotalCost = 60,
                TaskTitle = "Custom Fields Task",
                taskElementLabors = new Collection<ResourceTypeDto>() {
                    new ResourceTypeDto() {
                        Id = 56,
                        BoeID = 1,
                        EndDateValue = DateTime.Parse("6/15/2016"),
                        StartDateValue = DateTime.Parse("1/15/2016"),
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
        /// Creates the existing cost task element, with multi
        /// </summary>
        /// <returns>The existing cost task element</returns>
        private BoeTaskElementDTO CreateExistingCostCustomFieldMultiTaskElement()
        {
            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>() {
                new ResourceSpreadDto() {
                    BoeID = 2,
                    Id = 1,
                    LaborSpreadDate = DateTime.Parse("1/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 2,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("2/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 2,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("3/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 2,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("4/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 2,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("5/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 2,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("6/15/2016"),
                    LaborSpreadValue = 10,
                    LaborTypeId = 2
                }
            };

            return new BoeTaskElementDTO()
            {
                BoeID = 2,
                Description = "taskElementDesc",
                EndDate = DateTime.Parse("6/15/2016"),
                Id = 5,
                StartDate = DateTime.Parse("1/15/2016"),
                TaskElementType = TaskElementType.Labor,
                TotalCost = 60,
                taskElementLabors = new Collection<ResourceTypeDto>() {
                    new ResourceTypeDto() {
                        Id = 56,
                        BoeID = 1,
                        EndDateValue = DateTime.Parse("6/15/2016"),
                        StartDateValue = DateTime.Parse("1/15/2016"),
                        LaborSpreads = spreads,
                        PercentSpread = null,
                        PerformingOrgID = 1,
                        ResourceID = 5,
                        SpreadCurveID = SpreadCurves.DiscreteCost,
                        ValueSpread = 60,
                        WBSID =2,
                        CLINID =2,
                        PercentSpreadLocked = true,
                        HourSpreadLocked = true,
                        SpreadType = IES.Common.SpreadType.Cost
                    }
                }
            };
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
                    LaborSpreadDate = DateTime.Parse("1/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("2/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("3/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("4/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("5/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("6/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                }
            };

            return new BoeTaskElementDTO()
            {
                BoeID = 1,
                Description = "taskElementDesc",
                EndDate = DateTime.Parse("6/15/2016"),
                Id = 2,
                StartDate = DateTime.Parse("1/15/2016"),
                TaskElementType = TaskElementType.Labor,
                TotalHours = 6,
                taskElementLabors = new Collection<ResourceTypeDto>() {
                    new ResourceTypeDto() {
                        Id = 94802,
                        BoeID = 1,
                        EndDateValue = DateTime.Parse("6/15/2016"),
                        StartDateValue = DateTime.Parse("1/15/2016"),
                        LaborSpreads = spreads,
                        PercentSpread = null,
                        PerformingOrgID = 1,
                        ResourceID = 6,
                        SpreadCurveID = SpreadCurves.DiscreteHours,
                        ValueSpread = 6,
                        PercentSpreadLocked = false,
                        HourSpreadLocked = true,
                        SpreadType = IES.Common.SpreadType.Hours
                    }
                }
            };
        }


        /// <summary>
        /// Creates the existing hours task element.
        /// </summary>
        /// <returns>The existing hours task element.</returns>
        private BoeTaskElementDTO CreateExistingHoursPercentageLockTaskElement()
        {
            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>() {
                new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 1,
                    LaborSpreadDate = DateTime.Parse("1/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("2/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("3/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("4/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("5/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("6/15/2016"),
                    LaborSpreadValue = 1,
                    LaborTypeId = 2
                }
            };

            return new BoeTaskElementDTO()
            {
                BoeID = 1,
                Description = "taskElementDesc",
                EndDate = DateTime.Parse("6/15/2016"),
                Id = 2,
                StartDate = DateTime.Parse("1/15/2016"),
                TaskElementType = TaskElementType.Labor,
                TotalHours = 6,
                taskElementLabors = new Collection<ResourceTypeDto>() {
                    new ResourceTypeDto() {
                        Id = 94802,
                        BoeID = 1,
                        EndDateValue = DateTime.Parse("6/15/2016"),
                        StartDateValue = DateTime.Parse("1/15/2016"),
                        LaborSpreads = spreads,
                        PercentSpread = 100,
                        PerformingOrgID = 1,
                        ResourceID = 6,
                        SpreadCurveID = SpreadCurves.DiscreteHours,
                        PercentSpreadLocked = true,
                        HourSpreadLocked = false,
                        SpreadType = IES.Common.SpreadType.Hours
                    }
                }
            };
        }
        /// <summary>
        /// Creates the combined task element.
        /// </summary>
        /// <returns>A combined task element</returns>
        private BoeTaskElementDTO CreateCombinedTaskElement()
        {
            BoeTaskElementDTO cost = this.CreateExistingCostTaskElement();
            BoeTaskElementDTO hours = this.CreateExistingHoursTaskElement();

            cost.taskElementLabors.Add(hours.taskElementLabors.First());
            cost.Id = 3;

            Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>() {
                new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 1,
                    LaborSpreadDate = DateTime.Parse("2/15/2016"),
                    LaborSpreadValue = 6,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 2,
                    LaborSpreadDate = DateTime.Parse("3/15/2016"),
                    LaborSpreadValue = 7,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 3,
                    LaborSpreadDate = DateTime.Parse("4/15/2016"),
                    LaborSpreadValue = 8,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 4,
                    LaborSpreadDate = DateTime.Parse("5/15/2016"),
                    LaborSpreadValue = 9,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 5,
                    LaborSpreadDate = DateTime.Parse("6/15/2016"),
                    LaborSpreadValue = 0,
                    LaborTypeId = 2
                },
                 new ResourceSpreadDto() {
                    BoeID = 1,
                    Id = 6,
                    LaborSpreadDate = DateTime.Parse("7/15/2016"),
                    LaborSpreadValue = 3.33m,
                    LaborTypeId = 2
                }
            };

            cost.taskElementLabors.Add(new ResourceTypeDto()
            {
                Id = 94803,
                BoeID = 1,
                EndDateValue = DateTime.Parse("7/15/2016"),
                StartDateValue = DateTime.Parse("2/15/2016"),
                LaborSpreads = spreads,
                PercentSpread = null,
                PerformingOrgID = 1,
                ResourceID = 6,
                SpreadCurveID = SpreadCurves.SpreadCurve13,
                ValueSpread = 33.33m,
                PercentSpreadLocked = true,
                HourSpreadLocked = true,
                SpreadType = IES.Common.SpreadType.Hours
            });

            return cost;
        }

        /// <summary>
        /// Creates a Full Workspace.
        /// </summary>
        /// <returns>A Full Workspace</returns>
        private FullWorkspace CreateWorkspace(int taskElementId)
        {
            int workspaceId = taskElementId;

            FullWorkspace workspace = new FullWorkspace() {
                Id = workspaceId,
                ResourceListID = 5,
                ResourceDecimalPrecision = 2,
                CostDecimalPrecision = 2
            };

            return workspace;
        }

        #endregion Private Methods

        #region Tests

        /// <summary>
        /// Tests the Labor Export for a Cost Task Element with Multi
        /// </summary>
        [TestMethod]
        public void LaborExport_Cost_CustomFields_Multi()
        {
            FullBoe BOEMultiTrue = new FullBoe()
            {
                Id = 2,
                IsMultiClinWbs = true,
                WorkspaceID = 5         
            };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(5, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { BOEMultiTrue });
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(5)).Returns(new Collection<FullWbs>() { new FullWbs() { Id = 2, WbsNumber = "01", WbsTitle = "title" }, new FullWbs() { Id = 3, WbsNumber = "02", WbsTitle = "title2" } });
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(5)).Returns(new Collection<FullClin>() { new FullClin() { Id = 2, ClinNumber = "01", ClinTitle = "title" } });
            TestTaskElement(this.CreateExistingCostCustomFieldMultiTaskElement(), true);
            
        }

        /// <summary>
        /// Tests the Labor Export for a Cost Task Element with Multi.
        /// </summary>
        [TestMethod]
        public void LaborExport_Cost_CustomFields()
        {
            TestTaskElement(this.CreateExistingCostCustomFieldTaskElement());
        }
        /// <summary>
        /// Tests the Labor Export for a Cost Task Element.
        /// </summary>
        [TestMethod]
        public void LaborExport_Cost()
        {
            TestTaskElement(this.CreateExistingCostTaskElement());
        }

        /// <summary>
        /// Tests the Labor Export Template for a Cost Task Element.
        /// </summary>
        [TestMethod]
        public void LaborExport_Cost_Template()
        {
            TestTaskElement(this.CreateExistingCostTaskElement(), false, true);
        }
        
        /// <summary>
        /// Tests the Labor Export for an Hours Task Element.
        /// </summary>
        [TestMethod]
        public void LaborExport_Hours()
        {
            TestTaskElement(this.CreateExistingHoursTaskElement());
        }

        /// <summary>
        /// Tests the Labor Export for an EP Task Element.
        /// </summary>
        [TestMethod]
        public void LaborExport_EPs()
        {

			string beforeTest = System.Configuration.ConfigurationManager.AppSettings["ShowEquivalentPersonsOption"];
            System.Configuration.ConfigurationManager.AppSettings["ShowEquivalentPersonsOption"] = "true";
            FullObjectHelper.RefreshEPForTests();
            TestTaskElement(this.CreateExistingHoursTaskElement(), isEP: true);
            System.Configuration.ConfigurationManager.AppSettings["ShowEquivalentPersonsOption"] = beforeTest;
            FullObjectHelper.RefreshEPForTests();
        }

        /// <summary>
        /// Tests the Labor Export for an Hours Task Element.
        /// </summary>
        [TestMethod]
        public void LaborExport_HoursPercentageLock()
        {
            TestTaskElement(this.CreateExistingHoursPercentageLockTaskElement());
        }

        /// <summary>
        /// Tests the Labor Export for a Task Element with Cost and Hours Labor data.
        /// </summary>
        [TestMethod]
        public void LaborExport_Mixed()
        {
            TestTaskElement(this.CreateCombinedTaskElement());
        }

        #endregion Tests

        /// <summary>
        /// Tests the task element.
        /// </summary>
        /// <param name="taskElementDTO">The task element dto.</param>
        private void TestTaskElement(BoeTaskElementDTO taskElementDTO, bool isMulti = false, bool isTemplate = false, bool isEP = false)
        {
            FullWorkspace workspace = this.CreateWorkspace(taskElementDTO.Id);
            workspace.IsUsingEquivalentPerson = isEP;
            string excelFile = LaborTypeAndSpreadExporter.ExportToExcelFile(this.templatePath, this.resourceDTODataLoader.Object, this.commonDataMapper.Object, workspace, taskElementDTO, taskElementDTO.BoeID, isTemplate);

            VerifyExcelDocument(excelFile, taskElementDTO, workspace, isMulti, isTemplate, isEP);
        }

        /// <summary>
        /// Verifies the excel document.
        /// </summary>
        /// <param name="excelFile">The excel file.</param>
        /// <param name="taskElementDTO">The task element dto.</param>
        private void VerifyExcelDocument(string excelFile, BoeTaskElementDTO taskElementDTO, FullWorkspace workspace, bool isMulti, bool isTemplate, bool isEp = false)
        {
            Assert.IsTrue(File.Exists(excelFile));

            ICollection<string> spreadColumns = CalculateRequiredSpreadColumns(taskElementDTO);

            // create list of required columns
            List<string> requiredColumns = new List<string>() { LaborTypeAndSpreadImporter.LABOR_TYPE_ID_COL, ImportExportConstants.RESOURCE_COLUMN_HEADER, LaborTypeAndSpreadImporter.PERFORMING_ORG_COL };

            if (workspace.CustomFields.Any())
            {
                foreach (CustomFieldDTO customField in workspace.CustomFields)
                {
                    if (customField.CustomFieldRequired)
                    {
                        requiredColumns.Add(ExcelUtilities.SetPrefixCustomFieldRequired(customField.CustomFieldName));
                    }
                    else
                    {
                        requiredColumns.Add(ExcelUtilities.SetPrefixCustomField(customField.CustomFieldName));
                    }
                }
            }
            if(isMulti)
            {
                requiredColumns.Add(LaborTypeAndSpreadImporter.RESOURCE_WBS_HEADER);
                requiredColumns.Add(LaborTypeAndSpreadImporter.RESOURCE_CLIN_HEADER);

            }

            requiredColumns.AddRange(new List<string>() { ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER, LaborTypeAndSpreadImporter.PERCENT_SPREAD_COL, LaborTypeAndSpreadImporter.COST_COL });

            if (isEp)
            {
                requiredColumns.Add(LaborTypeAndSpreadImporter.EP_SPREAD_COL);
            }
            else
            {
                requiredColumns.Add(LaborTypeAndSpreadImporter.HOURS_SPREAD_COL);
            }

            requiredColumns.AddRange(spreadColumns);

            // Check to make sure that all of the required columns are there

            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(excelFile, false))
            {

                List<Dictionary<string, string>> rows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(spreadsheet, LaborTypeAndSpreadImporter.IMPORT_TAB, requiredColumns.ToArray(), requiredColumns.ToArray()).ToList();

                if (isTemplate)
                {
                    Assert.AreEqual(0, rows.Count);
                }
                else
                {
                    Assert.AreEqual(taskElementDTO.taskElementLabors.Count, rows.Count);

                    for (int i = 0; i < rows.Count; i++)
                    {
                        Dictionary<string, string> row = rows[i];
						ResourceTypeDto laborResource = taskElementDTO.taskElementLabors[i];

                        Assert.AreEqual(laborResource.Id, int.Parse(row[LaborTypeAndSpreadImporter.LABOR_TYPE_ID_COL]));
                        ICollection<ResourceDTO> resourcelist = CreateLaborResources();
                        ResourceDTO resource = resourcelist.FirstOrDefault(r => r.ResourceDesc == row[ImportExportConstants.RESOURCE_COLUMN_HEADER]);

                        Assert.IsNotNull(resource);
                        Assert.AreEqual(laborResource.ResourceID, resource.Id);

                        ICollection<PerformingOrgDTO> perfOrgList = CreatePerformingOrgs();
                        PerformingOrgDTO perfOrg = perfOrgList.FirstOrDefault(po => row[LaborTypeAndSpreadImporter.PERFORMING_ORG_COL] == (po.PerformingOrgName + " - " + po.PerformingOrgDesc));

                        Assert.AreEqual(laborResource.PerformingOrgID, perfOrg.Id);

                        Assert.AreEqual(laborResource.StartDateValue.ToMonthString(), DateTime.Parse(row[ImportExportConstants.START_DATE_COLUMN_HEADER]).ToMonthString());
                        Assert.AreEqual(laborResource.EndDateValue.ToMonthString(), DateTime.Parse(row[ImportExportConstants.END_DATE_COLUMN_HEADER]).ToMonthString());


                        if (laborResource.SpreadType == SpreadType.Cost)
                        {
                            Assert.AreEqual(laborResource.ValueSpread, decimal.Parse(row[LaborTypeAndSpreadImporter.COST_COL], NumberStyles.AllowCurrencySymbol | NumberStyles.Number, CultureInfo.CurrentCulture.NumberFormat));
                            Assert.AreEqual(0, decimal.Parse(row[LaborTypeAndSpreadImporter.PERCENT_SPREAD_COL]));

                            if (isEp)
                            {
                                Assert.AreEqual(0, decimal.Parse(row[LaborTypeAndSpreadImporter.EP_SPREAD_COL]));
                            }
                            else
                            {
                                Assert.AreEqual(0, decimal.Parse(row[LaborTypeAndSpreadImporter.HOURS_SPREAD_COL]));
                            }
                        }
                        else
                        {
                            if (laborResource.PercentSpreadLocked)
                            {
                                Assert.AreEqual(laborResource.PercentSpread ?? 0, decimal.Parse(row[LaborTypeAndSpreadImporter.PERCENT_SPREAD_COL]));
                                string outHours = string.Empty;
                                if (isEp)
                                {
                                    row.TryGetValue(LaborTypeAndSpreadImporter.EP_SPREAD_COL, out outHours);
                                }
                                else
                                {
                                    row.TryGetValue(LaborTypeAndSpreadImporter.HOURS_SPREAD_COL, out outHours);
                                }
                                Assert.AreEqual(null, outHours);
                            }
                            else
                            {
                                string outPercentage = string.Empty;
                                row.TryGetValue(LaborTypeAndSpreadImporter.PERCENT_SPREAD_COL, out outPercentage);
                                Assert.AreEqual(null, outPercentage);
                                if (isEp)
                                {
                                    Assert.AreEqual(laborResource.ValueSpread ?? 0, decimal.Parse(row[LaborTypeAndSpreadImporter.EP_SPREAD_COL]));
                                }
                                else
                                {
                                    Assert.AreEqual(laborResource.ValueSpread ?? 0, decimal.Parse(row[LaborTypeAndSpreadImporter.HOURS_SPREAD_COL]));
                                }
                            }
                        }

                        // Assert the Custom Fields
                        AssertCustomFields(workspace, row, laborResource);
                        if (isMulti)
                        {
                            Assert.IsTrue(row.ContainsKey(LaborTypeAndSpreadImporter.RESOURCE_WBS_HEADER));
                            Assert.IsTrue(row.ContainsKey(LaborTypeAndSpreadImporter.RESOURCE_CLIN_HEADER));
                            Assert.IsTrue(workspace.WbsElements.FirstOrDefault(x => x.WbsString == row[LaborTypeAndSpreadImporter.RESOURCE_WBS_HEADER]) != null);
                            Assert.IsTrue(workspace.Clins.FirstOrDefault(x => x.ClinString == row[LaborTypeAndSpreadImporter.RESOURCE_CLIN_HEADER]) != null);
                        }
                        else
                        {
                            Assert.IsFalse(row.ContainsKey(LaborTypeAndSpreadImporter.RESOURCE_WBS_HEADER));
                            Assert.IsFalse(row.ContainsKey(LaborTypeAndSpreadImporter.RESOURCE_CLIN_HEADER));
                        }

                        // Assert the spread values
                        foreach (ResourceSpreadDto spread in laborResource.LaborSpreads)
                        {
                            string month = spread.LaborSpreadDate.ToMonthString();

                            Assert.AreEqual(spread.LaborSpreadValue, decimal.Parse(row[month], NumberStyles.AllowCurrencySymbol | NumberStyles.Number, CultureInfo.CurrentCulture.NumberFormat));
                        }
                    }
                }
            }
        }

        private static void AssertCustomFields(FullWorkspace workspace, Dictionary<string, string> row, ResourceTypeDto laborResource)
        {
            if (workspace.CustomFields.Any())
            {
                Dictionary<int, ICollection<KeyValuePair<int, int>>> dictionary = CreateCustomFieldDictionary();
                ICollection<KeyValuePair<int, int>> laborPicks = null;

                if (!dictionary.TryGetValue(laborResource.Id, out laborPicks))
                {
                    laborPicks = new Collection<KeyValuePair<int, int>>();
                }

                foreach (CustomFieldDTO customField in workspace.CustomFields)
                {
                    string customFieldName = customField.CustomFieldRequired ? ExcelUtilities.SetPrefixCustomFieldRequired(customField.CustomFieldName) : ExcelUtilities.SetPrefixCustomField(customField.CustomFieldName);
                    Assert.IsTrue(row.ContainsKey(customFieldName));

                    if (laborPicks.Any(kv => kv.Key == customField.Id))
                    {
                        KeyValuePair<int, int> pairing = laborPicks.First(kv => kv.Key == customField.Id);

                        CustomFieldValueDTO customFieldValue = workspace.CustomFieldValues.First(cfv => cfv.Id == pairing.Value);
                        string expected = customField.IsOpenEnded
                            ? String.Format("{0}", customFieldValue.CustomFieldValueDescription)
                            : String.Format("{0} - {1}", customFieldValue.CustomFieldValueName,
                                customFieldValue.CustomFieldValueDescription);
                        Assert.AreEqual(expected, row[customFieldName]);
                    }
                    else
                    {
                        Assert.AreEqual(string.Empty, row[customFieldName]);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the required spread columns names.
        /// </summary>
        /// <param name="taskElementDTO">The task element dto.</param>
        /// <returns>Collection of Spread month names for column headers.</returns>
        private static ICollection<string> CalculateRequiredSpreadColumns(BoeTaskElementDTO taskElementDTO)
        {
            Collection<ResourceTypeDto> laborResources = taskElementDTO != null ? taskElementDTO.taskElementLabors : new Collection<ResourceTypeDto>();

            // determination complete range of spread dates across all resources (i.e. the total number of spread month columns needed)
            DateTime spreadStartDate = laborResources.Where(r => r.StartDate.HasValue).Select(r => r.StartDate.Value).Min();
            DateTime spreadEndDate = laborResources.Where(r => r.EndDate.HasValue).Select(r => r.EndDate.Value).Max(); 
            
            List<string> spreadMonthColumnHeaders = new List<string>();
            for (DateTime dt = spreadStartDate; dt <= spreadEndDate; dt = dt.AddMonths(1))
            {
                spreadMonthColumnHeaders.Add(dt.ToString("MM/yyyy"));
            }

            return spreadMonthColumnHeaders;
        }

    }
}
