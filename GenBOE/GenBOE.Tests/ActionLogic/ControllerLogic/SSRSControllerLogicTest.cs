// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.ModelView.SSRS;
    using GenBOE.ActionLogic.Workspace;
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
    public class SSRSControllerLogicTest
    {
        SSRSControllerLogic sut;

        FullProjectMapWorkspace ws;
        FullClin clin1;
        FullWbs wbs1;
        FullBoe boe1;
        FullBoe boe2;
        FullBoe boe3;
        FullBoe boe4;
        PerformingOrgDTO perfOrg1;
        PerformingOrgDTO perfOrg2;
        ResourceDTO resource1;
        ResourceDTO resource2;
        ResourceDTO subresource1;
        ResourceDTO subresource2;
        Collection<ResourceTypeDto> laborResourcesTaskElement1;
        Collection<ResourceTypeDto> laborResourcesTaskElement2;
        Collection<ResourceTypeDto> laborResourcesTaskElement3;
        Collection<ResourceTypeDto> laborResourcesTaskElement4;
        Collection<ResourceSpreadDto> resourceSpreads = new Collection<ResourceSpreadDto>();

        // Setup Mock Objects and SUT.
        Mock<IRetriever> retriever = new Mock<IRetriever>();

        Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
        Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
        Mock<IPermissionsDTODataLoader> permissionsLoader = new Mock<IPermissionsDTODataLoader>();
        Mock<IOffloadRatesDTOLoader> offloadRatesDTOLoader = new Mock<IOffloadRatesDTOLoader>();
        Mock<IResourceDTODataLoader> resourceLoader = new Mock<IResourceDTODataLoader>();
        Mock<IWorkspaceDTODataLoader> workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
        Mock<ISystemSettingDTODataLoader> systemsLoader = new Mock<ISystemSettingDTODataLoader>();
        private Mock<IProjectMapDataLoader> projectMapLoader;
		private Mock<IActiveDirectoryUtilities> activeDirectoryUtilities = new Mock<IActiveDirectoryUtilities>();
		/// <summary>
		/// Initialize all objects required for tests.
		/// </summary>
		[TestInitialize]
        public void TestInitialize()
        {
            sut = new SSRSControllerLogic(offloadRatesDTOLoader.Object, resourceLoader.Object, workspaceLoader.Object, commonDataMapper.Object);

            this.projectMapLoader = new Mock<IProjectMapDataLoader>();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IActiveDirectoryUtilities), activeDirectoryUtilities.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IProjectMapDataLoader), this.projectMapLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader),
                permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ISystemSettingDTODataLoader), systemsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IOffloadRatesDTOLoader),
                offloadRatesDTOLoader.Object);

            // Setup ResourceType DTOs.
            laborResourcesTaskElement1 = new Collection<ResourceTypeDto>();
            laborResourcesTaskElement2 = new Collection<ResourceTypeDto>();
            laborResourcesTaskElement3 = new Collection<ResourceTypeDto>();
            laborResourcesTaskElement4 = new Collection<ResourceTypeDto>();

            // Add labor resources.
            laborResourcesTaskElement1.Add(new ResourceTypeDto()
            {
                LaborSpreads = new Collection<ResourceSpreadDto>()
                {
                    new ResourceSpreadDto()
                    {
                        LaborSpreadDate = DateTime.Parse("1/15/2020").Normalize(),
                        LaborSpreadValue = 8
                    },
                    new ResourceSpreadDto()
                    {
                        LaborSpreadDate = DateTime.Parse("2/15/2020").Normalize(),
                        LaborSpreadValue = 7
                    }
                },
                SpreadType = IES.Common.SpreadType.Cost,
                ResourceID = 2,
                PerformingOrgID = 2,
                ValueSpread = 15,
                StartDateValue = DateTime.Parse("1/15/2020").Normalize(),
                EndDateValue = DateTime.Parse("2/15/2020").Normalize()
            });

            laborResourcesTaskElement2.Add(new ResourceTypeDto()
            {
                LaborSpreads = new Collection<ResourceSpreadDto>()
                {
                    new ResourceSpreadDto()
                    {
                        LaborSpreadDate = DateTime.Parse("1/15/2019").Normalize(),
                        LaborSpreadValue = 6
                    },
                    new ResourceSpreadDto()
                    {
                        LaborSpreadDate = DateTime.Parse("2/15/2019").Normalize(),
                        LaborSpreadValue = 5
                    }
                },
                SpreadType = IES.Common.SpreadType.Cost,
                ResourceID = 2,
                PerformingOrgID = 2,
                ValueSpread = 11,
                StartDateValue = DateTime.Parse("1/15/2019").Normalize(),
                EndDateValue = DateTime.Parse("2/15/2019").Normalize()
            });

            laborResourcesTaskElement3.Add(new ResourceTypeDto()
            {
                LaborSpreads = new Collection<ResourceSpreadDto>()
                {
                    new ResourceSpreadDto()
                    {
                        LaborSpreadDate = DateTime.Parse("1/15/2018").Normalize(),
                        LaborSpreadValue = 4
                    },
                    new ResourceSpreadDto()
                    {
                        LaborSpreadDate = DateTime.Parse("2/15/2018").Normalize(),
                        LaborSpreadValue = 3
                    }
                },
                SpreadType = IES.Common.SpreadType.Hours,
                ResourceID = 1,
                PerformingOrgID = 1,
                ValueSpread = 7,
                HourSpreadLocked = true,
                StartDateValue = DateTime.Parse("1/15/2018").Normalize(),
                EndDateValue = DateTime.Parse("2/15/2018").Normalize()
            });

            laborResourcesTaskElement4.Add(new ResourceTypeDto()
            {
                LaborSpreads = new Collection<ResourceSpreadDto>()
                {
                    new ResourceSpreadDto()
                    {
                        LaborSpreadDate = DateTime.Parse("1/15/2017").Normalize(),
                        LaborSpreadValue = 2
                    },
                    new ResourceSpreadDto()
                    {
                        LaborSpreadDate = DateTime.Parse("2/15/2017").Normalize(),
                        LaborSpreadValue = 1
                    }
                },
                SpreadType = IES.Common.SpreadType.Hours,
                ResourceID = 1,
                PerformingOrgID = 1,
                ValueSpread = 3,
                HourSpreadLocked = true,
                StartDateValue = DateTime.Parse("1/15/2017").Normalize(),
                EndDateValue = DateTime.Parse("2/15/2017").Normalize()
            });

            // Create a resourceSpreads object for use in other tests.
            Collection<ResourceTypeDto> laborResources = new Collection<ResourceTypeDto>()
            {
                laborResourcesTaskElement1.First(),
                laborResourcesTaskElement2.First(),
                laborResourcesTaskElement3.First(),
                laborResourcesTaskElement4.First()
            };
            foreach (ResourceTypeDto laborResource in laborResources)
            {
                foreach (ResourceSpreadDto spread in laborResource.LaborSpreads)
                {
                    resourceSpreads.Add(spread);
                }
            }

            // Set up FullWorkspace, FullBOEs, and TaskElements.
            ws = new FullProjectMapWorkspace() {Id = 1, WorkspaceName = "Workspace1", ProjectMapType = ProjectMapType.NonTimePhasedProjectMap};
            clin1 = new FullClin() {Id = 1, WorkspaceID = 1, ClinTitle = "Clin1Title", ClinNumber = "Clin 1"};
            wbs1 = new FullWbs() {Id = 1, WbsNumber = "1.2.3", WbsTitle = "WbsTitle1"};
            boe1 = new FullBoe()
            {
                Id = -100000,
                Workspace = ws,
                WorkspaceID = ws.Id,
                CLINID = 1,
                WBSID = 1,
                Category = "Cat1",
                Title = "Boe1Title",
                Description = "Boe1Desc",
               
                StartDate = DateTime.Parse("1/15/2019").Normalize(),
                EndDate = DateTime.Parse("2/15/2020").Normalize(),
                ClassOfCost = ClassOfCost.Recurring
            };
            boe2 = new FullBoe()
            {
                Id = -100001,
                Workspace = ws,
                WorkspaceID = ws.Id,
                CLINID = 1,
                WBSID = 1,
                Category = "Cat1",
                Title = "Boe1Title",
                Description = "Boe1Desc",
               
                StartDate = DateTime.Parse("1/15/2019").Normalize(),
                EndDate = DateTime.Parse("2/15/2020").Normalize(),
                ClassOfCost = ClassOfCost.Recurring
            };
            boe3 = new FullBoe()
            {
                Id = -100002,
                Workspace = ws,
                WorkspaceID = ws.Id,
                CLINID = 1,
                WBSID = 1,
                Category = "Cat2",
                Title = "Boe2Title",
                Description = "Boe2Desc",
               
                StartDate = DateTime.Parse("1/15/2017").Normalize(),
                EndDate = DateTime.Parse("2/15/2018").Normalize(),
                ClassOfCost = ClassOfCost.NonRecurring
            };
            boe4 = new FullBoe()
            {
                Id = -100003,
                Workspace = ws,
                WorkspaceID = ws.Id,
                CLINID = 1,
                WBSID = 1,
                Category = "Cat2",
                Title = "Boe2Title",
                Description = "Boe2Desc",
               
                StartDate = DateTime.Parse("1/15/2017").Normalize(),
                EndDate = DateTime.Parse("2/15/2018").Normalize(),
                ClassOfCost = ClassOfCost.DevNonRecurring
            };
            BoeTaskElementDTO taskElement1 = new BoeTaskElementDTO()
            {
                Id = 1,
                BoeID = boe1.Id,
                taskElementLabors = laborResourcesTaskElement1,
                Description = "task1",
                StartDate = DateTime.Parse("1/15/2020").Normalize(),
                EndDate = DateTime.Parse("2/15/2020").Normalize(),
                MOQText = "rationale 1",
                BOETaskID = "111",
                TaskTitle = "task title 1"
            };
            BoeTaskElementDTO taskElement2 = new BoeTaskElementDTO()
            {
                Id = 2,
                BoeID = boe1.Id,
                taskElementLabors = laborResourcesTaskElement2,
                Description = "task2",
                StartDate = DateTime.Parse("1/15/2019").Normalize(),
                EndDate = DateTime.Parse("2/15/2019").Normalize(),
                MOQText = "rationale 2",
                BOETaskID = "222",
                TaskTitle = "task title 2"
            };
            BoeTaskElementDTO taskElement3 = new BoeTaskElementDTO()
            {
                Id = 3,
                BoeID = boe3.Id,
                taskElementLabors = laborResourcesTaskElement3,
                Description = "task3",
                StartDate = DateTime.Parse("1/15/2018").Normalize(),
                EndDate = DateTime.Parse("2/15/2018").Normalize(),
                MOQText = "rationale 3",
                BOETaskID = "333",
                TaskTitle = "task title 3"
            };
            BoeTaskElementDTO taskElement4 = new BoeTaskElementDTO()
            {
                Id = 4,
                BoeID = boe4.Id,
                taskElementLabors = laborResourcesTaskElement4,
                Description = "task4",
                StartDate = DateTime.Parse("1/15/2017").Normalize(),
                EndDate = DateTime.Parse("2/15/2017").Normalize(),
                MOQText = "rationale 4",
                BOETaskID = "444",
                TaskTitle = "task title 4"
            };

            // Set up Resources and PerformingOrgs.
            resource1 = new ResourceDTO() {Id = 1, ResourceName = "Resource1", ResourceDesc = "Resource1Desc"};
            resource2 = new ResourceDTO() {Id = 2, ResourceName = "Resource2", ResourceDesc = "Resource2Desc"};
            subresource1 = new ResourceDTO() { Id = 3, ResourceName = "SubResource1", ResourceDesc = "subResource1Desc" };
            subresource2 = new ResourceDTO() { Id = 4, ResourceName = "SubResource2", ResourceDesc = "subResource2Desc" };
            perfOrg1 = new PerformingOrgDTO()
            {
                Id = 1,
                PerformingOrgName = "PerfOrg1",
                PerformingOrgDesc = "PerfOrg1Desc"
            };
            perfOrg2 = new PerformingOrgDTO()
            {
                Id = 2,
                PerformingOrgName = "PerfOrg2",
                PerformingOrgDesc = "PerfOrg2Desc"
            };

            // Set up Offload Rates.
            Collection<OffloadRatesDTO> offloadRates = new Collection<OffloadRatesDTO>
            {
                new OffloadRatesDTO
                {
                    Id = 1,
                    HourlyRate = 20,
                    Percent = 0.50m,
                    PerformingOrg = "PerfOrg1",
                    Resource = "Resource1",
                    SubResource = "SubResource1",
                    Year = 2017
                },
                new OffloadRatesDTO
                {
                Id = 1,
                HourlyRate = 20,
                Percent = 0.50m,
                PerformingOrg = "PerfOrg1",
                Resource = "Resource1",
                SubResource = "SubResource1",
                Year = 2018
                },
                new OffloadRatesDTO
                {
                    Id = 1,
                    HourlyRate = 20,
                    Percent = 0.50m,
                    PerformingOrg = "PerfOrg2",
                    Resource = "Resource2",
                    SubResource = "SubResource2",
                    Year = 2018
                }
            };

            // Setup factories and mock calls.
            factory.Setup(x => x.CreateClonedProjectMapWorkspace(It.IsAny<FullWorkspace>())).Returns(ws);
            factory.Setup(x => x.CreateFullProjectMapWorkspace(ws.Shortname, It.IsAny<bool>())).Returns(ws);
            factory.Setup(x => x.CreateFullWorkspace(ws.Shortname, It.IsAny<bool>())).Returns(ws);
            factory.Setup(x => x.CreateFullWorkspace(ws.Id)).Returns(ws);
            factory.Setup(x => x.CreateFullWorkspace(ws)).Returns(ws);
            factory.Setup(x => x.CreateFullBoe(boe1.Id)).Returns(boe1);
            factory.Setup(x => x.CreateFullBoe(boe2.Id)).Returns(boe2);
            factory.Setup(x => x.CreateFullBoe(boe3.Id)).Returns(boe3);
            factory.Setup(x => x.CreateFullBoe(boe4.Id)).Returns(boe4);
            factory.Setup(x => x.CreateFullBoe(boe1)).Returns(boe1);
            factory.Setup(x => x.CreateFullBoe(boe2)).Returns(boe2);
            factory.Setup(x => x.CreateFullBoe(boe3)).Returns(boe3);
            factory.Setup(x => x.CreateFullBoe(boe4)).Returns(boe4);
            factory.Setup(x => x.CreateFullBoe(It.Is<BoeDTO>(b => b.Id == boe1.Id))).Returns(boe1);
            factory.Setup(x => x.CreateFullBoe(It.Is<BoeDTO>(b => b.Id == boe2.Id))).Returns(boe2);
            factory.Setup(x => x.CreateFullBoe(It.Is<BoeDTO>(b => b.Id == boe3.Id))).Returns(boe3);
            factory.Setup(x => x.CreateFullBoe(It.Is<BoeDTO>(b => b.Id == boe4.Id))).Returns(boe4);
            factory.Setup(x => x.CreateFullClin(It.IsAny<ClinDTO>())).Returns(clin1);
            factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(wbs1);
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> {boe1, boe2});
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe1.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<BoeTaskElementDTO> {taskElement1, taskElement2 });
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe2.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<BoeTaskElementDTO> {  });
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe3.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<BoeTaskElementDTO> {taskElement3 });
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe4.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<BoeTaskElementDTO> {taskElement4});
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new Collection<BoeTaskElementDTO> {taskElement1, taskElement2, taskElement3, taskElement4});
            retriever.Setup(x => x.GetClinsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullClin>() {clin1});
            retriever.Setup(x => x.GetResourcesByResourceListId(ws.Id))
                .Returns(new Collection<ResourceDTO>() {resource1, resource2, subresource1, subresource2});
            retriever.Setup(x => x.GetPerformingOrgsByListId(ws.Id))
                .Returns(new Collection<PerformingOrgDTO>() {perfOrg1, perfOrg2});
            retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>()))
                .Returns(new Collection<PerformingOrgDTO>() {perfOrg1, perfOrg2});
            retriever.Setup(x => x.GetOdcCollectionByBoeIds(It.IsAny<ICollection<int>>(), It.IsAny<bool>()))
                .Returns(new Collection<OtherDirectCostDTO>());
            retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), It.IsAny<bool>()))
                .Returns(new Collection<MaterialDTO>());
            retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>()))
                .Returns(new Collection<ResourceDTO>() {resource1, resource2, subresource1, subresource2 });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>()))
                .Returns(new Collection<FullWbs>() {wbs1});
            retriever.Setup(x => x.GetTravelByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new Collection<TravelDTO>());
            offloadRatesDTOLoader.Setup(x => x.GetByWorkspaceId(ws.Id)).Returns(offloadRates);
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO>()
            {
                (new PerformingOrgDTO() { Id = 1, PerformingOrgName = "Test Cost Center", PerformingOrgDesc = "Test Pricing Code" }),
                (new PerformingOrgDTO() { Id = 2, PerformingOrgName = "Test Cost Center", PerformingOrgDesc = "Test Pricing Code" })
            });

            resourceLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(new ResourceDTO() { ResourceName = "Test Resource" });
            
            ICollection<ProjectMapModelView> modelsWorkspace1 = new List<ProjectMapModelView> {
                new ProjectMapModelView { ClassOfCost = ClassOfCost.Recurring.ToDescription(), WbsNumber = "1.2.3", CostCenter = "PerfOrg2", ActivityID = "Boe1Title",
                    ActivityName = "Boe1Desc", WbsElementTitle = "WbsTitle1", InitialResource = "Resource2", StartDate = DateTime.Parse("1/15/2020").Normalize(), EndDate = DateTime.Parse("2/15/2020").Normalize(),
                    Clin = "Clin1Title", SowNumber = "sow", SowTitle = "sow title", Task = "task1", Hours = 0, Dollars = 15, Rationale = "rationale 1", CamName = "Test Name",
                    Category = "Cat1", Offload = false, AddDelete = "A", DiscreteMonths = new decimal?[] { 0,0,0,0,0,0,0,0,0,0,0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8,7} },
                new ProjectMapModelView { ClassOfCost = ClassOfCost.Recurring.ToDescription(), WbsNumber = "1.2.3", CostCenter = "PerfOrg2", ActivityID = "Boe1Title",
                    ActivityName = "Boe1Desc", WbsElementTitle = "WbsTitle1", InitialResource = "Resource2", StartDate = DateTime.Parse("1/15/2019").Normalize(), EndDate = DateTime.Parse("2/15/2019").Normalize(),
                    Clin = "Clin1Title", SowNumber = "sow", SowTitle = "sow title", Task = "task2", Hours = 0, Dollars = 11, Rationale = "rationale 2", CamName = "Test Name",
                    Category = "Cat1", Offload = false, AddDelete = "A", DiscreteMonths = new decimal?[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 5} },
                new ProjectMapModelView { ClassOfCost = ClassOfCost.NonRecurring.ToDescription(), WbsNumber = "1.2.3", CostCenter = "PerfOrg1", ActivityID = "Boe2Title",
                    ActivityName = "Boe2Desc", WbsElementTitle = "WbsTitle1", InitialResource = "Resource1", StartDate = DateTime.Parse("1/15/2018").Normalize(), EndDate = DateTime.Parse("2/15/2018").Normalize(),
                    Clin = "Clin1Title", SowNumber = "sow", SowTitle = "sow title", Task = "task3", Hours = 7, Dollars = 0, Rationale = "rationale 3", CamName = "Test Name",
                    Category = "Cat2", Offload = false, AddDelete = "A", DiscreteMonths = new decimal?[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  4, 3} },
                new ProjectMapModelView { ClassOfCost = ClassOfCost.DevNonRecurring.ToDescription(), WbsNumber = "1.2.3", CostCenter = "PerfOrg1", ActivityID = "Boe2Title",
                    ActivityName = "Boe2Desc", WbsElementTitle = "WbsTitle1", InitialResource = "Resource1", StartDate = DateTime.Parse("1/15/2017").Normalize(), EndDate = DateTime.Parse("2/15/2017").Normalize(),
                    Clin = "Clin1Title", SowNumber = "sow", SowTitle = "sow title", Task = "task4", Hours = 3, Dollars = 0, Rationale = "rationale 4", CamName = "Test Name",
                    Category = "Cat2", Offload = false, AddDelete = "A", DiscreteMonths = new decimal?[] { 2, 1} }
            };

            this.projectMapLoader.Setup(x => x.GetByWorkspaceId(1)).Returns(modelsWorkspace1);
            this.retriever.Setup(x => x.GetProjectMapDataByWorkspaceId(1)).Returns(modelsWorkspace1);
        }

        /// <summary>
        /// Test the ability to get Offload BOEs from a FullWorkspace.
        /// </summary>
        [TestMethod]
        public void GetOffloadBOEsFromFullWorkspaceTest()
        {
            FullProjectMapWorkspace ws = factory.Object.CreateFullWorkspace(1) as FullProjectMapWorkspace;
            List<FullBoe> offloadedBoes = sut.GetOffloadBOEsFromFullWorkspace(ws).ToList();
            Assert.AreEqual(4, offloadedBoes.Count, "The BOEs were not properly offloaded.");

            // Ensure boes are in the proper order.
            ICollection<FullBoe> expectedOrderedBoes = ProjectMapSorter.OrderBoes(offloadedBoes, ws);

            for (int i = 0; i < offloadedBoes.Count; i++)
            {
                Assert.AreEqual(expectedOrderedBoes.ElementAt(i).Id, offloadedBoes.ElementAt(i).Id);
            }
        }

        /// <summary>
        /// Test getting the minimum start year from amongst all of the resources.
        /// </summary>
        [TestMethod]
        public void GetMinimumResourceStartYearTest()
        {
            FullProjectMapWorkspace ws = factory.Object.CreateFullWorkspace(1) as FullProjectMapWorkspace;
            ICollection<FullBoe> offloadedBoes = sut.GetOffloadBOEsFromFullWorkspace(ws);
            ICollection<BoeTaskElementDTO> allTaskElements = offloadedBoes.SelectMany(x => x.TaskElements).ToList();
            int minimumStartYear = sut.GetMinimumResourceStartYear(offloadedBoes, allTaskElements);
            Assert.AreEqual(2017, minimumStartYear, "Minimum Start Year was not correctly calculated.");
        }

        /// <summary>
        /// Test calculating the labor spread summaries for each year.
        /// </summary>
        [TestMethod]
        public void GetCostAnalysisReportLaborSpreadSummariesByYearTest()
        {
            const int startYear = 2017;
            Dictionary<int, decimal> summaries =
                sut.GetCostAnalysisReportLaborSpreadSummariesByYear(startYear, resourceSpreads);
            Assert.AreEqual(3, summaries[startYear],
                string.Format("Labor Spread Summary incorrect for year {0}.", startYear.ToString()));
            Assert.AreEqual(7, summaries[startYear + 1],
                string.Format("Labor Spread Summary incorrect for year {0}.", (startYear + 1).ToString()));
            Assert.AreEqual(11, summaries[startYear + 2],
                string.Format("Labor Spread Summary incorrect for year {0}.", (startYear + 2).ToString()));
            Assert.AreEqual(15, summaries[startYear + 3],
                string.Format("Labor Spread Summary incorrect for year {0}.", (startYear + 3).ToString()));
        }

        /// <summary>
        /// Test massaging the data / retrieving the ModelViews for Cost Analysis Reports.
        /// </summary>
        [TestMethod]
        public void GetCostAnalysisReportModelViewsTest()
        {
            string reportTitle = SSRSReportType.CostAnalysis17Years.GetDescription();
            const int startYear = 2017;
            FullProjectMapWorkspace ws = factory.Object.CreateFullWorkspace(1) as FullProjectMapWorkspace;
            ICollection<FullBoe> offloadedBoes = sut.GetOffloadBOEsFromFullWorkspace(ws);
            ICollection<CostAnalysisReportRMSModelView> modelViews =
                sut.GetCostAnalysisReportModelViews(offloadedBoes, ws, SSRSReportType.CostAnalysis17Years);

            // Ensure the correct number of ModelViews were returned.
            Assert.AreEqual(4, modelViews.Count, "Incorrect number of ModelViews returned.");

            // Find each individual ModelView.
            CostAnalysisReportRMSModelView modelViewWithYear1Value = modelViews.Where(mv => mv.Year01 != 0).First();
            CostAnalysisReportRMSModelView modelViewWithYear2Value = modelViews.Where(mv => mv.Year02 != 0).First();
            CostAnalysisReportRMSModelView modelViewWithYear3Value = modelViews.Where(mv => mv.Year03 != 0).First();
            CostAnalysisReportRMSModelView modelViewWithYear4Value = modelViews.Where(mv => mv.Year04 != 0).First();

            // Evaluate each ModelView.
            ValidateCostAnalysisReportModelView(modelViewWithYear1Value, ws, boe4, clin1, wbs1, perfOrg1, resource1,
                laborResourcesTaskElement3.First().SpreadType, reportTitle, startYear, 1);
            ValidateCostAnalysisReportModelView(modelViewWithYear2Value, ws, boe3, clin1, wbs1, perfOrg1, resource1,
                laborResourcesTaskElement3.First().SpreadType, reportTitle, startYear, 2);
            ValidateCostAnalysisReportModelView(modelViewWithYear3Value, ws, boe2, clin1, wbs1, perfOrg2, resource2,
                laborResourcesTaskElement1.First().SpreadType, reportTitle, startYear, 3);
            ValidateCostAnalysisReportModelView(modelViewWithYear4Value, ws, boe1, clin1, wbs1, perfOrg2, resource2,
                laborResourcesTaskElement1.First().SpreadType, reportTitle, startYear, 4);
        }

        /// <summary>
        /// Test massaging the data / retrieving the ModelViews for Cost Analysis Reports with RMS.
        /// </summary>
        [TestMethod]
        public void GetCostAnalysisReportModelViewsTest_RMS()
        {
            string reportTitle = SSRSReportType.CostAnalysis17Years.GetDescription();
            const int startYear = 2017;
            // Start off with projectmap workspace to get "valid projectmap" BOEs instead of creating them from scratch
            FullProjectMapWorkspace ws = factory.Object.CreateFullWorkspace(1) as FullProjectMapWorkspace;
            ICollection<FullBoe> offloadedBoes = sut.GetOffloadBOEsFromFullWorkspace(ws);

            ICollection<BoeTaskElementDTO> goodTasks = offloadedBoes.SelectMany(b => b.TaskElements).ToList();
            int numGoodTasks = goodTasks.Count;
            int firstYear = goodTasks.Min(t => t.StartDate.Value.Year);

            // add a dummy BOE with no Tasks
            FullBoe dummyBoe = new FullBoe
            {
                Id = 789789,
                Title = "dummy BOE with no Tasks",
                SOWTitle = "dummy BOE with no Tasks"
            };

            offloadedBoes.Add(dummyBoe);
            dummyBoe.SetTaskElements(new List<BoeTaskElementDTO>());

            // add a dummy BOE with a Task with no resources with start year before the firstYear of valid data
            FullBoe boeWithBadTask = new FullBoe
            {
                Id = 890890,
                Title = "dummy BOE with bad Task",
                SOWTitle = "dummy BOE with bad Task"
            };
            boeWithBadTask.SetTaskElements(new BoeTaskElementDTO[] {
                new BoeTaskElementDTO {
                    Id = 098098,
                    Description = "bad task with no resources",
                    StartDate = new DateTime(firstYear - 1, 1, 1),
                    EndDate = new DateTime(firstYear - 1, 12, 1),
                    BoeID = boeWithBadTask.Id,
                TaskTitle = "booooooo 333"
                }
            });
            
            offloadedBoes.Add(boeWithBadTask);

            // add a bad Task to the first good BOE
            FullBoe goodBoe = offloadedBoes.First(b => b.TaskElements.Any());
            ICollection<BoeTaskElementDTO> goodBoeTasks = goodBoe.TaskElements.ToList();
            goodBoeTasks.Add(new BoeTaskElementDTO
            {
                Id = 123123,
                Description = "bad task with no resources on good boe",
                StartDate = new DateTime(firstYear - 1, 1, 1),
                EndDate = new DateTime(firstYear - 1, 12, 1),
                BoeID = boeWithBadTask.Id,
                TaskTitle = "boooooo 444o"
            });
            goodBoe.SetTaskElements(goodBoeTasks);

            // filter to get good boes
            ICollection<FullBoe> filteredGoodBoes = SSRSControllerLogic.FilterGoodBOEs(offloadedBoes);

            ICollection <CostAnalysisReportRMSModelView> modelViews =
                sut.GetCostAnalysisReportModelViews(filteredGoodBoes, ws, SSRSReportType.CostAnalysis17Years);

            // Ensure the correct number of ModelViews were returned.
            Assert.AreEqual(4, modelViews.Count, "Incorrect number of ModelViews returned.");

            // Find each individual ModelView.
            CostAnalysisReportRMSModelView modelViewWithYear1Value = modelViews.Where(mv => mv.Year01 != 0).First();
            CostAnalysisReportRMSModelView modelViewWithYear2Value = modelViews.Where(mv => mv.Year02 != 0).First();
            CostAnalysisReportRMSModelView modelViewWithYear3Value = modelViews.Where(mv => mv.Year03 != 0).First();
            CostAnalysisReportRMSModelView modelViewWithYear4Value = modelViews.Where(mv => mv.Year04 != 0).First();

            // Evaluate each ModelView.
            ValidateCostAnalysisReportModelView(modelViewWithYear1Value, ws, boe4, clin1, wbs1, perfOrg1, resource1,
                laborResourcesTaskElement3.First().SpreadType, reportTitle, startYear, 1);
            ValidateCostAnalysisReportModelView(modelViewWithYear2Value, ws, boe3, clin1, wbs1, perfOrg1, resource1,
                laborResourcesTaskElement3.First().SpreadType, reportTitle, startYear, 2);
            ValidateCostAnalysisReportModelView(modelViewWithYear3Value, ws, boe2, clin1, wbs1, perfOrg2, resource2,
                laborResourcesTaskElement1.First().SpreadType, reportTitle, startYear, 3);
            ValidateCostAnalysisReportModelView(modelViewWithYear4Value, ws, boe1, clin1, wbs1, perfOrg2, resource2,
                laborResourcesTaskElement1.First().SpreadType, reportTitle, startYear, 4);

            Assert.AreEqual(firstYear, modelViews.First().StartYear);

            modelViews =
                sut.GetCostAnalysisReportModelViews(filteredGoodBoes, ws, SSRSReportType.CostAnalysis8YearsFlat);
            reportTitle = SSRSReportType.CostAnalysis8YearsFlat.GetDescription();
            // Ensure the correct number of ModelViews were returned.
            Assert.AreEqual(4, modelViews.Count, "Incorrect number of ModelViews returned.");

            // Find each individual ModelView.
            modelViewWithYear1Value = modelViews.Where(mv => mv.Year01 != 0).First();
            modelViewWithYear2Value = modelViews.Where(mv => mv.Year02 != 0).First();
            modelViewWithYear3Value = modelViews.Where(mv => mv.Year03 != 0).First();
            modelViewWithYear4Value = modelViews.Where(mv => mv.Year04 != 0).First();

            // Evaluate each ModelView.
            ValidateCostAnalysisReportModelView(modelViewWithYear1Value, ws, boe4, clin1, wbs1, perfOrg1, resource1,
                laborResourcesTaskElement3.First().SpreadType, reportTitle, startYear, 1);
            ValidateCostAnalysisReportModelView(modelViewWithYear2Value, ws, boe3, clin1, wbs1, perfOrg1, resource1,
                laborResourcesTaskElement3.First().SpreadType, reportTitle, startYear, 2);
            ValidateCostAnalysisReportModelView(modelViewWithYear3Value, ws, boe2, clin1, wbs1, perfOrg2, resource2,
                laborResourcesTaskElement1.First().SpreadType, reportTitle, startYear, 3);
            ValidateCostAnalysisReportModelView(modelViewWithYear4Value, ws, boe1, clin1, wbs1, perfOrg2, resource2,
                laborResourcesTaskElement1.First().SpreadType, reportTitle, startYear, 4);

            Assert.AreEqual(firstYear, modelViews.First().StartYear);

            modelViews =
                sut.GetCostAnalysisReportModelViews(filteredGoodBoes, ws, SSRSReportType.CostAnalysis8Years);
            reportTitle = SSRSReportType.CostAnalysis8Years.GetDescription();
            // Ensure the correct number of ModelViews were returned.
            Assert.AreEqual(4, modelViews.Count, "Incorrect number of ModelViews returned.");

            // Find each individual ModelView.
            modelViewWithYear1Value = modelViews.Where(mv => mv.Year01 != 0).First();
            modelViewWithYear2Value = modelViews.Where(mv => mv.Year02 != 0).First();
            modelViewWithYear3Value = modelViews.Where(mv => mv.Year03 != 0).First();
            modelViewWithYear4Value = modelViews.Where(mv => mv.Year04 != 0).First();

            // Evaluate each ModelView.
            ValidateCostAnalysisReportModelView(modelViewWithYear1Value, ws, boe4, clin1, wbs1, perfOrg1, resource1,
                laborResourcesTaskElement3.First().SpreadType, reportTitle, startYear, 1);
            ValidateCostAnalysisReportModelView(modelViewWithYear2Value, ws, boe3, clin1, wbs1, perfOrg1, resource1,
                laborResourcesTaskElement3.First().SpreadType, reportTitle, startYear, 2);
            ValidateCostAnalysisReportModelView(modelViewWithYear3Value, ws, boe2, clin1, wbs1, perfOrg2, resource2,
                laborResourcesTaskElement1.First().SpreadType, reportTitle, startYear, 3);
            ValidateCostAnalysisReportModelView(modelViewWithYear4Value, ws, boe1, clin1, wbs1, perfOrg2, resource2,
                laborResourcesTaskElement1.First().SpreadType, reportTitle, startYear, 4);

            Assert.AreEqual(firstYear, modelViews.First().StartYear);
        }

        /// <summary>
        /// Test massaging the data / retrieving the ModelViews for Staffing Curves Reports.
        /// </summary>
        [TestMethod]
        public void GetStaffingCurvesReportModelViewsTest()
        {
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>()))
                .Returns(new List<PerformingOrgDTO>() { perfOrg1 });

            this.ws.ProjectMapType = ProjectMapType.TimePhasedProjectMap;
            this.ws.RefreshBoes();
            this.ws.LoadBoes();
            this.retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>()))
                .Returns(new List<PerformingOrgDTO>() { perfOrg1 });

            ICollection<FullBoe> offloadedBoes = this.sut.GetOffloadBOEsFromFullWorkspace(this.ws);
            ICollection<StaffingCurvesReportModelView> modelViews =
                this.sut.GetStaffingCurvesReportModelViews(offloadedBoes, this.ws);

            // staffing months for jan 2017 - dec 2020
            Assert.AreEqual(48, modelViews.Count);
            StaffingCurvesReportModelView jan2018 = modelViews.First(m => m.Date == DateTime.Parse("1/1/2018"));
            StaffingCurvesReportModelView feb2018 = modelViews.First(m => m.Date == DateTime.Parse("2/1/2018"));
            StaffingCurvesReportModelView jan2017 = modelViews.First(m => m.Date == DateTime.Parse("1/1/2017"));
            StaffingCurvesReportModelView feb2017 = modelViews.First(m => m.Date == DateTime.Parse("2/1/2017"));
            Assert.AreEqual(Utilities.AdjustPrecision(4m / 157m, 2), jan2018.FTE);
            Assert.AreEqual(this.perfOrg1.PerformingOrgDesc, jan2018.PerfOrg);
            Assert.AreEqual(Utilities.AdjustPrecision(3m / 157m, 2), feb2018.FTE);
            Assert.AreEqual(this.perfOrg1.PerformingOrgDesc, feb2018.PerfOrg);
            Assert.AreEqual(Utilities.AdjustPrecision(2m / 157m, 2), jan2017.FTE);
            Assert.AreEqual(this.perfOrg1.PerformingOrgDesc, jan2017.PerfOrg);
            Assert.AreEqual(Utilities.AdjustPrecision(1m / 157m, 2), feb2017.FTE);
            Assert.AreEqual(this.perfOrg1.PerformingOrgDesc, feb2017.PerfOrg);
        }

        //// <summary>
        /// Test massaging the data / retrieving the ModelViews for Summary Reports.
        /// </summary>
        [TestMethod]
        public void GetSummaryReportRMSModelViewsTest()
        {
            ICollection<FullBoe> offloadedBoes = this.sut.GetOffloadBOEsFromFullWorkspace(this.ws);
            ICollection<SummaryReportRMSModelView> modelViewsByClin =
                this.sut.GetSummaryReportRMSModelViews(offloadedBoes, this.ws, SSRSReportType.ProjectCLINCategoryCostSummary);

            ICollection<SummaryReportRMSModelView> modelViewsByCat =
                this.sut.GetSummaryReportRMSModelViews(offloadedBoes, this.ws, SSRSReportType.ProjectCategoryCLINCostSummary);

            Assert.AreEqual(modelViewsByCat.Sum(c => decimal.Parse(c.LaborHrs)), modelViewsByClin.Sum(c => decimal.Parse(c.LaborHrs)));
            Assert.AreEqual(modelViewsByCat.Sum(c => decimal.Parse(c.MatlCost)), modelViewsByClin.Sum(c => decimal.Parse(c.MatlCost)));

            Assert.AreEqual(modelViewsByCat.Count, modelViewsByClin.Count);

            foreach (SummaryReportRMSModelView clin in modelViewsByClin)
            {
                Assert.AreEqual("CLIN", clin.MajorGroupingLabel);
                Assert.AreEqual("Category", clin.MinorGroupingLabel);
            }

            foreach (SummaryReportRMSModelView cat in modelViewsByCat)
            {
                Assert.AreEqual("CLIN", cat.MinorGroupingLabel);
                Assert.AreEqual("Category", cat.MajorGroupingLabel);
            }
        }


        /// <summary>
        /// Test massaging the data / retrieving the ModelViews for BOE Summary Report.
        /// </summary>
        [TestMethod]
        public void GetBOESummaryReportModelViewsTest()
        {
            ICollection<FullBoe> offloadedBoes = this.sut.GetOffloadBOEsFromFullWorkspace(this.ws);
            ICollection<BOESummaryReportModelView> modelViews =
                this.sut.GetBOESummaryReportModelViews(offloadedBoes, this.ws);
            Assert.AreEqual(8, modelViews.Count); // 2 per boe task element (hours/cost)
            Assert.AreEqual(4, modelViews.Where(m => m.ResourceUnit == "Hours").Count());
            // cost cost hours hours
            AssertEmptySpreadCollection(
                modelViews.First(m => m.TaskDescription == "task1" && m.ResourceUnit == "Hours"));
            AssertEmptySpreadCollection(
                modelViews.First(m => m.TaskDescription == "task3" && m.ResourceUnit != "Hours"));
            Assert.AreEqual(15,
                modelViews.First(m => m.TaskDescription == "task1" && m.ResourceUnit != "Hours").Year04);
            Assert.AreEqual(11,
                modelViews.First(m => m.TaskDescription == "task2" && m.ResourceUnit != "Hours").Year03);
            Assert.AreEqual(7, modelViews.First(m => m.TaskDescription == "task3" && m.ResourceUnit == "Hours").Year02);
            Assert.AreEqual(3, modelViews.First(m => m.TaskDescription == "task4" && m.ResourceUnit == "Hours").Year01);
            foreach (BOESummaryReportModelView modelView in modelViews)
            {
                Assert.IsTrue(modelView.HeaderHTMLColumn2.Contains("Clin1Title"));
                Assert.AreEqual(ws.WorkspaceName, modelView.Project);

                if (modelView.BoeID == "-100000")
                {
                    Assert.AreEqual("Boe1Title", modelView.ActivityID);
                    Assert.AreEqual("Boe1Desc", modelView.ActivityName);
                    Assert.AreEqual(15, modelView.CostDollarsTotal);
                    Assert.AreEqual(0, modelView.SalaryHoursTotal);
                }
                else if (modelView.BoeID == "-100001")
                {
                    Assert.AreEqual("Boe1Title", modelView.ActivityID);
                    Assert.AreEqual("Boe1Desc", modelView.ActivityName);
                    Assert.AreEqual(11, modelView.CostDollarsTotal);
                    Assert.AreEqual(0, modelView.SalaryHoursTotal);
                }
                else if (modelView.BoeID == "-100002")
                {
                    Assert.AreEqual("Boe2Title", modelView.ActivityID);
                    Assert.AreEqual("Boe2Desc", modelView.ActivityName);
                    Assert.AreEqual(0, modelView.CostDollarsTotal);
                    Assert.AreEqual(7, modelView.SalaryHoursTotal);
                }
                else 
                {
                    Assert.AreEqual("Boe2Title", modelView.ActivityID);
                    Assert.AreEqual("Boe2Desc", modelView.ActivityName);
                    Assert.AreEqual(0, modelView.CostDollarsTotal);
                    Assert.AreEqual(3, modelView.SalaryHoursTotal);
                }
            }
        }

        /// <summary>
        /// Asserts the spread collection is empty.
        /// </summary>
        /// <param name="modelView">The model view.</param>
        private static void AssertEmptySpreadCollection(BOESummaryReportModelView modelView)
        {
            Assert.AreEqual(0m, modelView.Year01);
            Assert.AreEqual(0m, modelView.Year02);
            Assert.AreEqual(0m, modelView.Year03);
            Assert.AreEqual(0m, modelView.Year04);
            Assert.AreEqual(0m, modelView.Year05);
            Assert.AreEqual(0m, modelView.Year06);
            Assert.AreEqual(0m, modelView.Year07);
            Assert.AreEqual(0m, modelView.Year08);
            Assert.AreEqual(0m, modelView.Year09);
            Assert.AreEqual(0m, modelView.Year10);
            Assert.AreEqual(0m, modelView.Year11);
            Assert.AreEqual(0m, modelView.Year12);
            Assert.AreEqual(0m, modelView.Year13);
            Assert.AreEqual(0m, modelView.Year14);
            Assert.AreEqual(0m, modelView.Year15);
            Assert.AreEqual(0m, modelView.Year16);
            Assert.AreEqual(0m, modelView.Year17);
        }

        /// <summary>
        /// Verify the data for a CostAnalysisReportModelView.
        /// </summary>
        /// <param name="modelView">The ModelView to evaluate.</param>
        /// <param name="ws">The Workspace for the ModelView.</param>
        /// <param name="boe">The BOE for the ModelView.</param>
        /// <param name="clin">The CLIN for the ModelView.</param>
        /// <param name="wbs">The WBS for the ModelView.</param>
        /// <param name="perfOrg">The PerfOrg for the ModelView.</param>
        /// <param name="resource">The Resource for the ModelView.</param>
        /// <param name="spreadType">The SpreadType for the ModelView's Resource.</param>
        /// <param name="reportTitle">The Report Title for the ModelView.</param>
        /// <param name="startYear">The Start Year for the ModelView.</param>
        /// <param name="summariesForYear">The year of the Labor Spread Summaries to evaluate for the ModelView.</param>
        private void ValidateCostAnalysisReportModelView(CostAnalysisReportRMSModelView modelView, FullWorkspace ws,
            FullBoe boe, FullClin clin, FullWbs wbs, PerformingOrgDTO perfOrg, ResourceDTO resource,
            SpreadType spreadType, string reportTitle, int startYear, int summariesForYear)
        {
            // Verify all ModelView data.
            Assert.AreEqual(boe.Title, modelView.ActivityID, "ActivityID in the ModelView was not correct.");
            Assert.AreEqual(boe.Description, modelView.ActivityName, "ActivityName in the ModelView was not correct.");
            Assert.AreEqual(boe.Category, modelView.Category, "Category in the ModelView was not correct.");
            Assert.AreEqual(clin.ClinTitle, modelView.CLIN, "CLIN in the ModelView was not correct.");
            Assert.AreEqual(boe.ClassOfCost.ToDescription(), modelView.ClassOfCost, "Class of Cost in the ModelView was not correct.");
            Assert.AreEqual(perfOrg.PerformingOrgDesc, modelView.PerformingOrgDescription,
                "PerformingOrgDescription in the ModelView was not correct.");
            Assert.AreEqual(ws.WorkspaceName, modelView.Project, "Project in the ModelView was not correct.");
            Assert.AreEqual(reportTitle, modelView.ReportTitle, "ReportTitle in the ModelView was not correct.");
            Assert.AreEqual(resource.ResourceDesc, modelView.ResourceDescription,
                "ResourceDescription in the ModelView was not correct.");
            Assert.AreEqual(perfOrg.PerformingOrgName, modelView.CostCenter,
                "CostCenter in the ModelView was not correct.");
            Assert.AreEqual(resource.ResourceName, modelView.ResourceID,
                "ResourceID in the ModelView was not correct.");
            Assert.AreEqual(
                spreadType == SpreadType.Cost
                    ? Constants.COST_ANALYSIS_RESOURCE_UNIT_DIRECT_DOLLARS
                    : Constants.COST_ANALYSIS_RESOURCE_UNIT_HOURS, modelView.ResourceUnit,
                "ResourceUnit in the ModelView was not correct.");
            Assert.AreEqual(startYear, modelView.StartYear, "StartYear in the ModelView was not correct.");
            Assert.AreEqual(wbs.WbsNumber, modelView.WBS, "WBS in the ModelView was not correct.");

            // Check that the proper year has the proper value, and that other years are 0.
            switch (summariesForYear)
            {
                case 1:
                    Assert.AreEqual(3, modelView.Year01);
                    Assert.AreEqual(0, modelView.Year02);
                    Assert.AreEqual(0, modelView.Year03);
                    Assert.AreEqual(0, modelView.Year04);
                    break;
                case 2:
                    Assert.AreEqual(0, modelView.Year01);
                    Assert.AreEqual(7, modelView.Year02);
                    Assert.AreEqual(0, modelView.Year03);
                    Assert.AreEqual(0, modelView.Year04);
                    break;
                case 3:
                    Assert.AreEqual(0, modelView.Year01);
                    Assert.AreEqual(0, modelView.Year02);
                    Assert.AreEqual(11, modelView.Year03);
                    Assert.AreEqual(0, modelView.Year04);
                    break;
                case 4:
                    Assert.AreEqual(0, modelView.Year01);
                    Assert.AreEqual(0, modelView.Year02);
                    Assert.AreEqual(0, modelView.Year03);
                    Assert.AreEqual(15, modelView.Year04);
                    break;
                default:
                    Assert.Fail("Invalid Year Summary to check for test case.");
                    break;
            }

            // All other years should be 0.
            Assert.AreEqual(0, modelView.Year05);
            Assert.AreEqual(0, modelView.Year06);
            Assert.AreEqual(0, modelView.Year07);
            Assert.AreEqual(0, modelView.Year09);
            Assert.AreEqual(0, modelView.Year09);
            Assert.AreEqual(0, modelView.Year10);
            Assert.AreEqual(0, modelView.Year11);
            Assert.AreEqual(0, modelView.Year12);
            Assert.AreEqual(0, modelView.Year13);
            Assert.AreEqual(0, modelView.Year14);
            Assert.AreEqual(0, modelView.Year15);
            Assert.AreEqual(0, modelView.Year16);
            Assert.AreEqual(0, modelView.Year17);
        }

        /// <summary>
        /// Test retrieving the ModelViews for Pre Vs Post Offload Totals
        /// </summary>
        [TestMethod]
        public void GetPreVsPostOffloadTotalsModelViewsTest()
        {
            // Get expected input totals
            decimal expectedInputHours = Utilities.AdjustPrecision(this.ws.Boes.SelectMany(b => b.TaskElements)
                .SelectMany(t => t.taskElementLabors)
                .Where(l => l.SpreadType == SpreadType.Hours)
                .Select(tl => tl.ValueSpread ?? 0)
                .Sum(), this.ws.DecimalPrecision);
            decimal expectedInputCost = Utilities.AdjustPrecision(this.ws.Boes.SelectMany(b => b.TaskElements)
                .SelectMany(t => t.taskElementLabors)
                .Where(l => l.SpreadType == SpreadType.Cost)
                .Select(tl => tl.ValueSpread ?? 0)
                .Sum(), this.ws.CostDecimalPrecision);

            // Get offload results
            OffloadLaborRates offloader = new OffloadLaborRates();
            OffloadLaborRatesResults offloadResults = offloader.OffloadWorkspace(this.ws.Boes.ToList(), this.ws);
            ICollection<FullBoe> offloadBoes = offloadResults.Boes.ToCollection();

            // Get expected offload and output totals using offload results
            decimal expectedOffloadHours =
                Utilities.AdjustPrecision(offloadResults.TotalHoursOffloaded, this.ws.DecimalPrecision);
            decimal expectedOffloadCost =
                Utilities.AdjustPrecision(offloadResults.TotalCostOffloaded, this.ws.CostDecimalPrecision);
            decimal expectedOutputHours = Utilities.AdjustPrecision(offloadBoes.SelectMany(b => b.TaskElements)
                .SelectMany(t => t.taskElementLabors)
                .Where(l => l.SpreadType == SpreadType.Hours)
                .Select(tl => tl.ValueSpread ?? 0)
                .Sum(), this.ws.DecimalPrecision);
            decimal expectedOutputCost = Utilities.AdjustPrecision(offloadBoes.SelectMany(b => b.TaskElements)
                .SelectMany(t => t.taskElementLabors)
                .Where(l => l.SpreadType == SpreadType.Cost)
                .Select(tl => tl.ValueSpread ?? 0)
                .Sum(), this.ws.CostDecimalPrecision);

            // Calculate expected variance
            decimal expectedHoursVariance = expectedInputHours - expectedOffloadHours - expectedOutputHours;
            decimal expectedCostVariance = expectedInputCost + expectedOffloadCost - expectedOutputCost;

            // Get ModelViews
            ICollection<PreVsPostOffloadTotalsModelView> modelViews = this.sut.GetPreVsPostOffloadTotalsModelViews(this.ws);

            // Assert
            Assert.AreEqual(1, modelViews.Count);
            foreach (PreVsPostOffloadTotalsModelView modelView in modelViews)
            {
                Assert.AreEqual(expectedInputHours.ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(this.ws.DecimalPrecision)), modelView.HoursInputTotal);
                Assert.AreEqual(expectedOffloadHours.ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(this.ws.DecimalPrecision)), modelView.HoursOffloadTotal);
                Assert.AreEqual(expectedOutputHours.ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(this.ws.DecimalPrecision)), modelView.HoursOutputTotal);
                Assert.AreEqual(expectedHoursVariance.ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(this.ws.DecimalPrecision)), modelView.HoursVariance);
                
                Assert.AreEqual("$" + expectedInputCost.ToString(Utilities.CostPrecisionFormattingString(this.ws.CostDecimalPrecision)), modelView.CostInputTotal);
                Assert.AreEqual("$" + expectedOffloadCost.ToString(Utilities.CostPrecisionFormattingString(this.ws.CostDecimalPrecision)), modelView.CostOffloadTotal);
                Assert.AreEqual("$" + expectedOutputCost.ToString(Utilities.CostPrecisionFormattingString(this.ws.CostDecimalPrecision)), modelView.CostOutputTotal);
                Assert.AreEqual("$" + expectedCostVariance.ToString(Utilities.CostPrecisionFormattingString(this.ws.CostDecimalPrecision)), modelView.CostVariance);
            }
        }

        /// <summary>
        /// Test retrieving the ModelViews for Off Load Cost by Year.
        /// </summary>
        [TestMethod]
        public void GetOffloadCostByYearReportModelViewsTest()
        { 
            ICollection<ProjectMapModelView> modelsWorkspace1 = new List<ProjectMapModelView> {
                new ProjectMapModelView { ClassOfCost = ClassOfCost.Recurring.ToDescription(), WbsNumber = "Wbs11", CostCenter = "PerfOrg2", ActivityID = "Boe1Title",
                    ActivityName = "Boe1Desc", WbsElementTitle = "Wbs11", InitialResource = "Resource2", StartDate = DateTime.Parse("1/15/2020").Normalize(), EndDate = DateTime.Parse("2/15/2020").Normalize(),
                    Clin = "Clin1Title", SowNumber = "sow", SowTitle = "sow title", Task = "task1", Hours = 0, Dollars = 15, Rationale = "rationale1", CamName = "Test Name",
                    Category = "Cat1", Offload = false, AddDelete = "A", DiscreteMonths = new decimal?[] { 0,0,0,0,0,0,0,0,0,0,0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8,7} }
            };

            this.projectMapLoader.Setup(x => x.GetByWorkspaceId(1)).Returns(modelsWorkspace1);

            this.ws.RefreshBoes();
            this.ws.LoadBoes();

            Collection<ResourceSpreadDto> resourceSpreadDtos = new Collection<ResourceSpreadDto>()
            {
                new ResourceSpreadDto()
                {
                    BoeID = 5,
                    LaborSpreadDate = new DateTime(2017, 11, 1),
                    LaborSpreadValue = 101,
                    Updateable = UpdateType.None
                },
                new ResourceSpreadDto()
                {
                    BoeID = 5,
                    LaborSpreadDate = new DateTime(2017, 12, 1),
                    LaborSpreadValue = 106,
                    Updateable = UpdateType.None
                },
                new ResourceSpreadDto()
                {
                    BoeID = 5,
                    LaborSpreadDate = new DateTime(2018, 1, 1),
                    LaborSpreadValue = 201,
                    Updateable = UpdateType.None
                },
                new ResourceSpreadDto()
                {
                    BoeID = 5,
                    LaborSpreadDate = new DateTime(2018, 2, 1),
                    LaborSpreadValue = 206,
                    Updateable = UpdateType.None
                }
            };

            SubResourceTypeDto subResourceToTest = new SubResourceTypeDto
            {
                SubResourceName = "test sub resource",
                LaborSpreads = resourceSpreadDtos,
                InHouseLaborSpreads = resourceSpreadDtos,
                OffLoadedHourSpreads = resourceSpreadDtos,
                PerformingOrgID = 1,
                InHouseResource = 1
            };

            Collection<ResourceTypeDto> resourceTypeList = new Collection<ResourceTypeDto>();
            resourceTypeList.Add(subResourceToTest);

            BoeTaskElementDTO taskElement = new BoeTaskElementDTO
            {
                BoeID = 5,
                taskElementLabors = resourceTypeList,
                TaskTitle = "booooooo"
            };

            Collection<BoeTaskElementDTO> taskElementList = new Collection<BoeTaskElementDTO>();
            taskElementList.Add(taskElement);

            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(taskElementList);

            this.ws.Boes.ToCollection();

            FullBoe rwBoes = new FullBoe()
            {
                Id = 5,
                Title = "Test",
                Description = "Description",
                WBSID = ws.WbsElements.First().Id,
                CLINID = ws.Clins.First().Id
            };

            rwBoes.SetTaskElements(taskElementList);

            ICollection <FullBoe> rwBoe = new Collection<FullBoe> {rwBoes};

            List<OffloadCostByYearReportRMSModelView> offloadCostByYearReportRows = this.sut.GetOffloadCostByYearReportModelViews(rwBoe, this.ws).ToList();

            Assert.AreEqual(2, offloadCostByYearReportRows.Count);
            // Total Hours = In House Hours + Offload Hours = (106 + 101 + 201 + 206) + (106 + 101 + 201 + 206)
            Assert.AreEqual(1228, offloadCostByYearReportRows[0].TotalHours);
            Assert.AreEqual(414, offloadCostByYearReportRows[0].TotalHoursInYear);
            Assert.AreEqual(207, offloadCostByYearReportRows[0].TotalInHouseHours);
            Assert.AreEqual(207, offloadCostByYearReportRows[0].TotalOffLoadHours);
            Assert.AreEqual(rwBoes.Title, offloadCostByYearReportRows[0].ActivityId);
        }

        /// <summary>
        /// Test retrieving the ModelViews for Off Load Cost Summary Report
        /// </summary>
        [TestMethod]
        public void GetOffloadCostSummaryReportModelViewsTest()
        {
            Collection<OffloadCostByYearReportRMSModelView> offloadCostByYearReportRows =
                new Collection<OffloadCostByYearReportRMSModelView>
                {
                    new OffloadCostByYearReportRMSModelView
                    {
                        ActivityId = "abc",
                        CostCenter = "abc",
                        HourlyOffLoadRate = 20,
                        OffLoadPercent = .50m,
                        OffloadYear = "2017",
                        OffloadedResource = "abc",
                        Resource = "abc",
                        SumOfOLCost = 207,
                        TotalHours = 1226,
                        TotalHoursInYear = 413,
                        TotalInHouseHours = 206,
                        TotalOffLoadHours = 207,
                        Wbs = "1.2.3"
                    },
                    new OffloadCostByYearReportRMSModelView
                    {
                        ActivityId = "abc",
                        CostCenter = "abc",
                        HourlyOffLoadRate = 20,
                        OffLoadPercent = .50m,
                        OffloadYear = "2018",
                        OffloadedResource = "abc",
                        Resource = "abc",
                        SumOfOLCost = 407,
                        TotalHours = 1226,
                        TotalHoursInYear = 813,
                        TotalInHouseHours = 406,
                        TotalOffLoadHours = 407,
                        Wbs = "1.2.3"
                    }
                };

            IEnumerable<OffloadCostByYearReportRMSModelView> offloadCostSummaryReportRows = this.sut.ConsolidateOffloadCostByYear(offloadCostByYearReportRows);

            // 2 model views should be consolidated into 1
            Assert.AreEqual(1, offloadCostSummaryReportRows.Count());
            // Total hours is the same before and after consolidation because it captures the total across all years
            Assert.AreEqual(offloadCostByYearReportRows.First().TotalHours, offloadCostSummaryReportRows.First().TotalHours);
            // Consolidated in-house hours = 206 + 406 = 612
            Assert.AreEqual(offloadCostByYearReportRows.First().TotalInHouseHours + offloadCostByYearReportRows.Last().TotalInHouseHours, offloadCostSummaryReportRows.First().TotalInHouseHours);
            // Consolidated offload hours = 207 + 407 = 614
            Assert.AreEqual(offloadCostByYearReportRows.First().TotalOffLoadHours + offloadCostByYearReportRows.Last().TotalOffLoadHours, offloadCostSummaryReportRows.First().TotalOffLoadHours);
        }

        /// <summary>
        /// Test retrieving the ModelViews for RAM Report
        /// </summary>
        [TestMethod]
        public void GetRamReportModelViewsTest_ProjectMap()
        {
            ICollection<FullBoe> boes = sut.GetOffloadBOEsFromFullWorkspace(this.ws);

            ICollection<RAMReportModelView> expected = new Collection<RAMReportModelView>();

            resource2.ResourceName = resource1.ResourceName;
            retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>()))
                .Returns(new Collection<ResourceDTO>() { resource1, resource2 });

            foreach (FullBoe boe in boes)
            {
                foreach (BoeTaskElementDTO task in boe.TaskElements)
                {
                    var groupedResourceTypes = task.taskElementLabors.GroupBy(x => new { x.ResourceID, x.PerformingOrgID }).ToCollection();
                    foreach (var groupedResourceType in groupedResourceTypes)
                    {
                        decimal resourceValue = groupedResourceType.Sum(x => x.ValueSpread ?? 0);
                        string resourceValueString = groupedResourceType.First().SpreadType == SpreadType.Cost
                            ? "$" + resourceValue.ToString(
                                  Utilities.CostPrecisionFormattingString(this.ws.CostDecimalPrecision))
                            : resourceValue.ToString(Utilities.PrecisionFormattingString(this.ws.DecimalPrecision));

                        expected.Add(new RAMReportModelView()
                        {
                            Clin = boe.Clin.ClinTitle,
                            Wbs = boe.Wbs.WbsNumber,
                            ActivityId = boe.Title,
                            ActivityName = boe.Description,
                            ResourceCostCenter = "Resource1, Test Cost Center",
                            Value = resourceValueString
                        });
                    }
                }
            }
            
            ICollection<RAMReportModelView> results = sut.GetRamReportModelViews(boes, this.ws);

            Assert.AreEqual(expected.Count, results.Count);
            for(int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected.ElementAt(i).Clin, results.ElementAt(i).Clin);
                Assert.AreEqual(expected.ElementAt(i).Wbs, results.ElementAt(i).Wbs);
                Assert.AreEqual(expected.ElementAt(i).ActivityId, results.ElementAt(i).ActivityId);
                Assert.AreEqual(expected.ElementAt(i).ActivityName, results.ElementAt(i).ActivityName);
                Assert.AreEqual(expected.ElementAt(i).ResourceCostCenter, results.ElementAt(i).ResourceCostCenter);
                Assert.AreEqual(expected.ElementAt(i).Value, results.ElementAt(i).Value);
            }
        }

        /// <summary>
        /// Test retrieving the ModelViews for RAM Report
        /// </summary>
        [TestMethod]
        public void GetRamReportModelViewsTest_Standard()
        {
            FullWorkspace workspace = new FullWorkspace() { Id = 1, WorkspaceName = "Workspace1", ProjectMapType = ProjectMapType.StandardWithOffload };
            factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(workspace);
            retriever.Setup(x => x.GetQuestionsAndAnswersByWorkspaceId(workspace.Id)).Returns(new List<RTECustomTemplateQuestionAnswerModelView>());

            ICollection<FullBoe> boes = sut.GetOffloadBOEsFromFullWorkspace(workspace);

            ICollection<RAMReportModelView> expected = new Collection<RAMReportModelView>();


            resource2.ResourceName = resource1.ResourceName;
            retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>()))
                .Returns(new Collection<ResourceDTO>() { resource1, resource2 });

            foreach (FullBoe boe in boes)
            {
                foreach (BoeTaskElementDTO task in boe.TaskElements)
                {
                    var groupedResourceTypes = task.taskElementLabors.GroupBy(x => new { x.ResourceID, x.PerformingOrgID }).ToCollection();
                    foreach (var groupedResourceType in groupedResourceTypes)
                    {
                        decimal resourceValue = groupedResourceType.Sum(x => x.ValueSpread ?? 0);
                        string resourceValueString = groupedResourceType.First().SpreadType == SpreadType.Cost
                            ? "$" + resourceValue.ToString(
                                  Utilities.CostPrecisionFormattingString(workspace.CostDecimalPrecision))
                            : resourceValue.ToString(Utilities.PrecisionFormattingString(workspace.DecimalPrecision));

                        expected.Add(new RAMReportModelView()
                        {
                            Clin = boe.Clin.ClinNumber,
                            Wbs = boe.Wbs.WbsNumber,
                            ActivityId = task.BOETaskID,
                            ActivityName = task.TaskTitle,
                            ResourceCostCenter = "Resource1, Test Cost Center",
                            Value = resourceValueString
                        });
                    }
                }
            }

            ICollection<RAMReportModelView> results = sut.GetRamReportModelViews(boes, workspace);

            Assert.AreEqual(expected.Count, results.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected.ElementAt(i).Clin, results.ElementAt(i).Clin);
                Assert.AreEqual(expected.ElementAt(i).Wbs, results.ElementAt(i).Wbs);
                Assert.AreEqual(expected.ElementAt(i).ActivityId, results.ElementAt(i).ActivityId);
                Assert.AreEqual(expected.ElementAt(i).ActivityName, results.ElementAt(i).ActivityName);
                Assert.AreEqual(expected.ElementAt(i).ResourceCostCenter, results.ElementAt(i).ResourceCostCenter);
                Assert.AreEqual(expected.ElementAt(i).Value, results.ElementAt(i).Value);
            }
        }


        /// <summary>
        /// Test retrieving the ModelViews for Workbench Offload report.
        /// </summary>
        [TestMethod]
        public void GetWorkbenchOffloadModelViewsTest()
        {
            ICollection<FullBoe> boes = sut.GetOffloadBOEsFromFullWorkspace(this.ws);

            ICollection<WorkbenchOffloadModelView> expected = new Collection<WorkbenchOffloadModelView>();


            IEnumerable<IGrouping<int?, FullBoe>> groups = boes.GroupBy(b => b.CLINID);
            foreach (IGrouping<int?, FullBoe> group in groups)
            {
                ICollection<ResourceSpreadDto> spreads = group.SelectMany(b => b.TaskElements).SelectMany(t => t.taskElementLabors).Where(tl => tl is SubResourceTypeDto).SelectMany(tl2 => tl2.LaborSpreads).ToList();

                IEnumerable<IGrouping<int, ResourceSpreadDto>> spreadGroups = spreads.GroupBy(s => s.LaborSpreadDate.Year);
                foreach (IGrouping<int, ResourceSpreadDto> spreadGroup in spreadGroups)
                {
                    expected.Add(new WorkbenchOffloadModelView
                    {
                        Clin = this.ws.Clins.First(c => c.Id == group.Key).ClinTitle,
                        Year = spreadGroup.Key.ToString(),
                        Value = spreadGroup.Sum(r => r.LaborSpreadValue)
                    });
                }
            }

            expected = expected.OrderBy(o => o.Clin).ThenBy(t => t.Year).ToList();

            ICollection<WorkbenchOffloadModelView> results = sut.GetWorkbenchOffloadModelViews(boes, this.ws);
            results = results.OrderBy(o => o.Clin).ThenBy(t => t.Year).ToList();

            Assert.AreEqual(expected.Count, results.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected.ElementAt(i).Clin, results.ElementAt(i).Clin);
                Assert.AreEqual(expected.ElementAt(i).Year, results.ElementAt(i).Year);
                Assert.AreEqual(expected.ElementAt(i).Value, results.ElementAt(i).Value);
            }
        }

        /// <summary>
        /// Test retrieving the ModelViews for Project WBS Cost Summary By CLIN Report
        /// </summary>
        [TestMethod]
        public void GetProjectWbsCostSummaryByClinTest()
        {
            List <ProjectWbsCostSummaryByClinModelView> projectWbsCostSummaryByClinReportRows = this.sut.GetProjectCLINCostSummaryModelViews(this.ws.Boes.ToCollection(), this.ws).ToList();

            Assert.IsTrue(projectWbsCostSummaryByClinReportRows.Any(), "GetProjectWbsCostSummaryByClin should not return an empty list");

            foreach (ProjectWbsCostSummaryByClinModelView mv in projectWbsCostSummaryByClinReportRows)
            {
                Assert.IsFalse(string.IsNullOrEmpty(mv.WorkspaceName), "WorkspaceName should not be blank");
                Assert.IsFalse(string.IsNullOrEmpty(mv.Clin), "Clin should not be blank");
                Assert.IsFalse(string.IsNullOrEmpty(mv.Wbs), "Wbs should not be blank");
                Assert.IsFalse(string.IsNullOrEmpty(mv.WbsTitle), "WbsTitle should not be blank");
                Assert.IsFalse(string.IsNullOrEmpty(mv.ActivityID), "ActivityID should not be blank");
                Assert.IsFalse(string.IsNullOrEmpty(mv.ActivityName), "ActivityName should not be blank");
                Assert.IsFalse(string.IsNullOrEmpty(mv.CostCenter), "CostCenter should not be blank");
                Assert.IsFalse(string.IsNullOrEmpty(mv.Resource), "Resource should not be blank");
                Assert.IsNotNull(mv.StartDate, "StartDate should not be null");
                Assert.IsNotNull(mv.EndDate, "EndDate should not be null");
                Assert.IsFalse ((mv.MatlCost ?? 0) == 0 && (mv.LaborHrs ?? 0) == 0 ||
                    mv.MatlCost.HasValue && mv.MatlCost != 0 && mv.LaborHrs.HasValue && mv.LaborHrs != 0, "Either material or labor should be non-zero but not both.");
            }
        }

        /// <summary>
        /// Test retrieving the ModelViews for the RPS Report.
        /// </summary>
        [TestMethod]
        public void GetRPSReportModelViewsTest()
        {
            List<RPSReportModelView> rpsReportModelViews = this.sut.GetRPSReportModelViews(this.ws.Boes.ToCollection(), this.ws).ToList();

            Assert.IsTrue(rpsReportModelViews.Any(), "GetRPSReportModelViews should not return an empty list.");

            foreach (RPSReportModelView mv in rpsReportModelViews)
            {
                Assert.IsFalse(string.IsNullOrEmpty(mv.Project), "Project should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.Resource), "Resource should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.CostCenter), "CostCenter should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.CostCenterDescription), "CostCenterDescription should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.ResourceType), "ResourceType should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.Resource), "Resource should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.CostCenter), "CostCenter should not be blank.");
                Assert.IsTrue(mv.OffloadRate >= 0 && mv.OffloadRate <= 100, "OffloadRate is a percentage and must be between 0 and 100.");
                Assert.IsNotNull(mv.StartDate, "StartDate should not be null.");
                Assert.IsTrue(mv.Month > 0 && mv.Month <= Constants.SSRS_RPS_REPORT_NUMBER_OF_MONTH_COLUMNS, "Month was out of range.");
                Assert.IsTrue(mv.ResourceUnit == Constants.RPS_RESOURCE_UNIT_DOLLARS || mv.ResourceUnit == Constants.RPS_RESOURCE_UNIT_HOURS, "ResourceUnit was not properly determined.");
            }
        }

        /// <summary>
        /// Test creating the missing month elements with an empty ModelView collection.
        /// </summary>
        [TestMethod]
        public void RPSReportCreateMissingMonthsWithEmptyModelViewsTest()
        {
            Collection<RPSReportModelView> rpsReport = new Collection<RPSReportModelView>();

            this.sut.RPSReportCreateMissingMonths(rpsReport);

            Assert.IsFalse(rpsReport.Any(), "Elements should not have been created.");
        }

        /// <summary>
        /// Test creating the missing month elements with a non-empty ModelView collection in which the elements do not have a month assigned.
        /// </summary>
        [TestMethod]
        public void RPSReportCreateMissingMonthsWithEmptyMonthTest()
        {
            Collection<RPSReportModelView> rpsReport = new Collection<RPSReportModelView>();

            rpsReport.Add(new RPSReportModelView() { Project = "ProjectName1", Resource = "Resource1", CostCenter = "CostCenter1", CostCenterDescription = "CostCenter1 desc", ResourceType = "ResType1", OffloadRate = Convert.ToDecimal(1.23), Value = Convert.ToDecimal(1) });

            this.sut.RPSReportCreateMissingMonths(rpsReport);

            Assert.IsTrue(rpsReport.Any());
            Assert.AreEqual(Constants.SSRS_RPS_REPORT_NUMBER_OF_MONTH_COLUMNS, rpsReport.Where(r => r.Month != 0).Count(), "Blank RPS Report elements were not created.");
        }

        /// <summary>
        /// Test creating the missing month elements with a properly created ModelView.
        /// </summary>
        [TestMethod]
        public void RPSReportCreateMissingMonthsTest()
        {
            Collection<RPSReportModelView> rpsReport = new Collection<RPSReportModelView>();

            rpsReport.Add(new RPSReportModelView() { Project = "ProjectName1", Resource = "Resource1", CostCenter = "CostCenter1", CostCenterDescription = "CostCenter1 desc", ResourceType = "ResType1", OffloadRate = Convert.ToDecimal(1.23), Value = Convert.ToDecimal(1), Month = 1 });
            rpsReport.Add(new RPSReportModelView() { Project = "ProjectName1", Resource = "Resource2", CostCenter = "CostCenter2", CostCenterDescription = "CostCenter2 desc", ResourceType = "ResType2", OffloadRate = Convert.ToDecimal(4.56), Value = Convert.ToDecimal(3), Month = 3 });

            this.sut.RPSReportCreateMissingMonths(rpsReport);

            Assert.IsTrue(rpsReport.Any());
            Assert.AreEqual(Constants.SSRS_RPS_REPORT_NUMBER_OF_MONTH_COLUMNS, rpsReport.Where(r => r.Month != 0).Count(), "The correct number of additional blank RPS Report elements were not created.");
            Assert.AreEqual(Constants.SSRS_RPS_REPORT_NUMBER_OF_MONTH_COLUMNS, rpsReport.Select(r => r.Month).Distinct().Count(), "The additional blank RPS Report elements do not have distinct months.");
            Assert.AreEqual(0, rpsReport.Where(r => r.Month < 1).Count(), "Blank RPS Report elements were created without a month.");
            Assert.AreEqual(0, rpsReport.Where(r => r.Month > Constants.SSRS_RPS_REPORT_NUMBER_OF_MONTH_COLUMNS).Count(), "Blank RPS Report elements were created with a month that exceeds the report limits.");
        }

        /// <summary>
        /// Test calculating the Resource Month with identical date parameters.
        /// </summary>
        [TestMethod]
        public void GetRPSReportResourceMonthSameDateTest()
        {
            DateTime reportStartDate = new DateTime(2017, 1, 1);
            DateTime laborSpreadStartDate = new DateTime(2017, 1, 1);
            int month = this.sut.GetMonthRelativeToStartYear(reportStartDate, laborSpreadStartDate);
            Assert.AreEqual(1, month, "Spread Date with matching Start Date not returning correct month.");
        }

        /// <summary>
        /// Test calculating the Resource Month with dates rom the same year.
        /// </summary>
        [TestMethod]
        public void GetRPSReportResourceMonthSameYearTest()
        {
            DateTime reportStartDate = new DateTime(2017, 1, 1);
            DateTime laborSpreadStartDate = new DateTime(2017, 7, 1);
            int month = this.sut.GetMonthRelativeToStartYear(reportStartDate, laborSpreadStartDate);
            Assert.AreEqual(7, month, "Spread Date with same year as Start Date not returning correct month.");
        }

        /// <summary>
        /// Test calculating the Resource Month with a LaborSpreadStartDate that is earlier than the StartDate.
        /// </summary>
        [TestMethod]
        public void GetRPSReportResourceMonthLaborSpreadOutOfRangeTest()
        {
            DateTime reportStartDate = new DateTime(2017, 1, 1);
            DateTime laborSpreadStartDate = new DateTime(2016, 7, 1);
            int month = this.sut.GetMonthRelativeToStartYear(reportStartDate, laborSpreadStartDate);
            Assert.IsTrue(month < 0, "Spread Date prior to Start Date not returning negative month.");
        }

        /// <summary>
        /// Test calculating the Resource Month with dates that are in different years.
        /// </summary>
        [TestMethod]
        public void GetRPSReportResourceMonthYearDifferenceTest()
        {
            DateTime reportStartDate = new DateTime(2017, 1, 1);
            DateTime laborSpreadStartDate = new DateTime(2018, 1, 1);
            int month = this.sut.GetMonthRelativeToStartYear(reportStartDate, laborSpreadStartDate);
            Assert.AreEqual(13, month, "Spread Date in a different year than Start Date not returning correct month.");
        }

        /// <summary>
        /// Test calculating the Resource Month with a null StartDate.
        /// </summary>
        [TestMethod]
        public void GetRPSReportResourceMonthNullStartDateTest()
        {
            DateTime? reportStartDate = null;
            DateTime laborSpreadStartDate = new DateTime(2017, 1, 1);
            int month = this.sut.GetMonthRelativeToStartYear(reportStartDate, laborSpreadStartDate);
            Assert.AreEqual(-1, month, "Null Start Date not returning -1.");
        }

        /// <summary>
        /// Test calculating the Resource Month with a null LaborSpreadStartDate.
        /// </summary>
        [TestMethod]
        public void GetRPSReportResourceMonthNullLaborSpreadStartDateTest()
        {
            DateTime reportStartDate = new DateTime(2017, 1, 1); ;
            DateTime? laborSpreadStartDate = null;
            int month = this.sut.GetMonthRelativeToStartYear(reportStartDate, laborSpreadStartDate);
            Assert.AreEqual(-1, month, "Null Spread Date not returning -1.");
        }

        [TestMethod]
        public void GetPRPReportModelViewsTest()
        {
            List<PRPModelView> prpReportModelViews = this.sut.GetPRPModelViews(this.ws.Boes.ToCollection(), this.ws).ToList();

            Assert.IsTrue(prpReportModelViews.Any(), "GetPRPModelViews should not return an empty list.");

            foreach (PRPModelView mv in prpReportModelViews)
            {
                Assert.IsFalse(string.IsNullOrEmpty(mv.Project), "Project should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.CLIN), "CLIN should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.WBS), "WBS should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.ActivityId), "ActivityId should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.ResourceType), "ResourceType should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.Resource), "Resource should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.CostCenter), "CostCenter should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.CostCenterDescription), "CostCenterDescription should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.Category), "Category should not be blank.");
                Assert.IsFalse(string.IsNullOrEmpty(mv.CLIN), "CLIN should not be blank.");
                Assert.IsNotNull(mv.StartDate, "StartDate should not be null.");
                Assert.IsNotNull(mv.EndDate, "StartDate should not be null.");
                Assert.IsNotNull(mv.ReportStartDate, "ReportStartDate should not be null.");
                Assert.IsTrue(mv.Month > 0 && mv.Month <= Constants.SSRS_RPS_REPORT_NUMBER_OF_MONTH_COLUMNS, "Month was out of range.");
                Assert.IsTrue(mv.ResourceUnit == Constants.RPS_RESOURCE_UNIT_DOLLARS || mv.ResourceUnit == Constants.RPS_RESOURCE_UNIT_HOURS, "ResourceUnit was not properly determined.");
            }
        }

        /// <summary>
        /// Test the GetCostByPriceCodeResource method
        /// </summary>
        [TestMethod]
        public void GetCostByPriceCodeResourceTest()
        {
            ICollection<FullBoe> boes = sut.GetOffloadBOEsFromFullWorkspace(this.ws).ToCollection();
            ICollection<CostByPricingCodeReportModelView> results = this.sut.GetCostByCategoryPricingCodeReportModelView(boes, this.ws);

            this.ValidateCostByPricingCodeReportModelView(results);
        }

        /// <summary>
        /// Test the GetCostByCategoryPricingCodeReportModelView method
        /// </summary>
        [TestMethod]
        public void GetCostByCategoryPricingCodeReportModelViewTest()
        {
            ICollection<FullBoe> boes = sut.GetOffloadBOEsFromFullWorkspace(this.ws).ToCollection();
            ICollection<CostByPricingCodeReportModelView> results = this.sut.GetCostByCategoryPricingCodeReportModelView(boes, this.ws);

            this.ValidateCostByPricingCodeReportModelView(results);
        }

        /// <summary>
        /// Validates the results of GetCostByPriceCodeResourceTest and GetCostByCategoryPricingCodeReportModelView
        /// </summary>
        /// <param name="results">Results to validate</param>
        private void ValidateCostByPricingCodeReportModelView(ICollection<CostByPricingCodeReportModelView> results)
        {
            ICollection<FullBoe> boes = sut.GetOffloadBOEsFromFullWorkspace(this.ws).ToCollection();

            int reportStartYear = boes.Select(b => b.StartDate).Min().Year;

            Assert.IsTrue(results.Any());
            foreach (CostByPricingCodeReportModelView result in results)
            {
                Assert.AreEqual(this.ws.WorkspaceName, result.Project);
                Assert.IsFalse(string.IsNullOrEmpty(result.Resource));
                Assert.IsTrue(result.ResourceType == "Labor Hours" || result.ResourceType == "Cost Dollars");
                Assert.IsFalse(string.IsNullOrEmpty(result.CostCenter));
                Assert.IsFalse(string.IsNullOrEmpty(result.PricingCode));
                Assert.IsFalse(string.IsNullOrEmpty(result.Category));
                Assert.AreEqual(reportStartYear, result.StartYear);

                // Based on TestInitialize(), Years 5-17 should be 0
                Assert.AreEqual(0m, result.Year05);
                Assert.AreEqual(0m, result.Year06);
                Assert.AreEqual(0m, result.Year07);
                Assert.AreEqual(0m, result.Year08);
                Assert.AreEqual(0m, result.Year09);
                Assert.AreEqual(0m, result.Year10);
                Assert.AreEqual(0m, result.Year11);
                Assert.AreEqual(0m, result.Year12);
                Assert.AreEqual(0m, result.Year13);
                Assert.AreEqual(0m, result.Year14);
                Assert.AreEqual(0m, result.Year15);
                Assert.AreEqual(0m, result.Year16);
                Assert.AreEqual(0m, result.Year17);
            }

            // Based on TestInitialize(), the first MV Year01 should be 3 from laborResourcesTaskELement4
            // and Year02 should be 7 from laborResourcesTaskELement3. The rest should be 0.
            Assert.AreEqual(3m, results.ElementAt(0).Year01);
            Assert.AreEqual(7m, results.ElementAt(0).Year02);
            Assert.AreEqual(0m, results.ElementAt(0).Year03);
            Assert.AreEqual(0m, results.ElementAt(0).Year04);

            // Based on TestInitialize(), the second MV Year03 should be 11 from laborResourcesTaskELement2
            // and Year04 should be 15 from laborResourcesTaskELement1. The rest should be 0.
            Assert.AreEqual(0m, results.ElementAt(1).Year01);
            Assert.AreEqual(0m, results.ElementAt(1).Year02);
            Assert.AreEqual(11m, results.ElementAt(1).Year03);
            Assert.AreEqual(15m, results.ElementAt(1).Year04);
        }
    }
}
