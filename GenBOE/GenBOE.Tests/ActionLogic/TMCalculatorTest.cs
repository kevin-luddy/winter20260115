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
	using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
	using System.Linq;

    /// <summary>
    /// Test T&amp;M Calculator class
    /// </summary>
    [TestClass]
    public class TMCalculatorTest : MOQObject
    {
        #region Private members
        private Mock<IRetriever> retriever;
        private Mock<IFullObjectFactory> factory;
        private Mock<ICommonDataMapper> commonDataMapper;
        private Mock<IPermissionsDTODataLoader> permLoader;
		private Mock<IActiveDirectoryUtilities> activeDirectoryUtilities;
		#endregion Private members

		/// <summary>
		/// Initializes data before each test run for this class.
		/// </summary>
		[TestInitialize]
        public void Initialize()
        {
            retriever = new Mock<IRetriever>();
            factory = new Mock<IFullObjectFactory>();
            commonDataMapper = new Mock<ICommonDataMapper>();
            permLoader = new Mock<IPermissionsDTODataLoader>();
			activeDirectoryUtilities = new Mock<IActiveDirectoryUtilities>();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IActiveDirectoryUtilities), activeDirectoryUtilities.Object);
		}

        [TestMethod]
        public void BL_TMTotalCostForTaskSpread()
        {
            WorkspaceDTO workspaceDto = new WorkspaceDTO() { Id = 1, IsUsingTM = true };

            FullWorkspace workspace = new FullWorkspace(workspaceDto);

            ResourceDTO res1 = new ResourceDTO() { Id = 1, ResourceName = "Res1", SegRegion = WebConstants.SPACE_LEGACY_TM, ElementOfCost = ElementOfCostType.Sub, RateType = RateType.Hours };
            ResourceDTO res2 = new ResourceDTO() { Id = 2, ResourceName = "Res2", SegRegion = WebConstants.SPACE_LEGACY_TM, ElementOfCost = ElementOfCostType.Sub, RateType = RateType.Hours };
            ResourceDTO res3 = new ResourceDTO() { Id = 3, ResourceName = "Res3", SegRegion = WebConstants.SPACE_LEGACY_TM, ElementOfCost = ElementOfCostType.Sub, RateType = RateType.Hours };

            TMResourceRateDTO rr11 = new TMResourceRateDTO() { Id = 1, WorkspaceID = workspace.Id, ResourceID = res1.Id, ResourceRate = 0.11m, StartDate = new DateTime(2028, 6, 15), EndDate = new DateTime(2029, 5, 15) };
            TMResourceRateDTO rr12 = new TMResourceRateDTO() { Id = 2, WorkspaceID = workspace.Id, ResourceID = res1.Id, ResourceRate = 0.22m, StartDate = new DateTime(2029, 6, 15), EndDate = new DateTime(2030, 5, 15) };
            TMResourceRateDTO rr13 = new TMResourceRateDTO() { Id = 3, WorkspaceID = workspace.Id, ResourceID = res1.Id, ResourceRate = 0.33m, StartDate = new DateTime(2030, 6, 15), EndDate = new DateTime(2031, 5, 15) };
            TMResourceRateDTO rr21 = new TMResourceRateDTO() { Id = 4, WorkspaceID = workspace.Id, ResourceID = res2.Id, ResourceRate = 0.11m, StartDate = new DateTime(2029, 6, 15), EndDate = new DateTime(2030, 5, 15) };
            TMResourceRateDTO rr22 = new TMResourceRateDTO() { Id = 5, WorkspaceID = workspace.Id, ResourceID = res2.Id, ResourceRate = 0.22m, StartDate = new DateTime(2030, 6, 15), EndDate = new DateTime(2031, 5, 15) };
            TMResourceRateDTO rr31 = new TMResourceRateDTO() { Id = 6, WorkspaceID = workspace.Id, ResourceID = res3.Id, ResourceRate = 0.11m, StartDate = new DateTime(2028, 6, 15), EndDate = new DateTime(2031, 5, 15) };

            ICollection<TMResourceRateDTO> tmResourceRatesFromDB = new Collection<TMResourceRateDTO>() { rr11, rr12, rr13, rr21, rr22, rr31 };

            Collection<BoeTaskElementDTO> taskElements = new Collection<BoeTaskElementDTO>(){
                new BoeTaskElementDTO() { Id = 1, TaskElementType = TaskElementType.Labor,
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            ResourceID = res1.Id,
                            SpreadType = IES.Common.SpreadType.Hours,
                            SpreadCurveID = SpreadCurves.SpreadCurve10,
							StartDate = new DateTime(2028,6,15,0,0,0),
							EndDate = new DateTime(2029,7,15,0,0,0),
							ValueSpread = 100,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { Id = 1, LaborSpreadValue = 20, LaborSpreadDate = new DateTime(2028,6,15,0,0,0) },
                                new ResourceSpreadDto() { Id = 2, LaborSpreadValue = 20, LaborSpreadDate = new DateTime(2028,7,15,0,0,0) },
                                new ResourceSpreadDto() { Id = 3, LaborSpreadValue = 10, LaborSpreadDate = new DateTime(2028,8,15,0,0,0) },
                                new ResourceSpreadDto() { Id = 4, LaborSpreadValue = 10, LaborSpreadDate = new DateTime(2029,6,15,0,0,0) },
                                new ResourceSpreadDto() { Id = 5, LaborSpreadValue = 40, LaborSpreadDate = new DateTime(2029,7,15,0,0,0) }
                            }
                        }
                    }
                }
            };

            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspaceDto.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(taskElements);
            retriever.Setup(x => x.GetTMResourceRates((workspaceDto.Id))).Returns(tmResourceRatesFromDB);

			TMCalculator sut = new TMCalculator();
            decimal cost = 0;
			List<ResourceDTO> resources = new List<ResourceDTO> { res1, res2, res3 };

			IDictionary<int, string> resourceIdToSegmentRegion = resources.ToDictionary(r => r.Id, d => d.SegRegion); 
			foreach (BoeTaskElementDTO taskElement in workspace.TaskElements)
            {
				//get brc labors based on 1lmx start date
				ICollection<ResourceTypeDto> taskElementLabors = BRCValidationUtility.ProcessLaborTypesForBrc(taskElement.taskElementLabors, resourceIdToSegmentRegion, string.Empty);
				// none of these task element labors that cross the 1LMX barrier should be split to BRC
				Assert.AreEqual(taskElement.taskElementLabors.Count, taskElementLabors.Count);

				foreach (ResourceTypeDto laborTask in taskElementLabors)
				{
					cost = sut.TotalCostForTaskSpread(workspace, laborTask);
				}
            }
            Assert.IsTrue(cost == 16.5m, "Doesn't equal the amount calculated");
        }

        [ExpectedException(typeof(GenValidationException))]
        [TestMethod]
        public void BL_TMTotalCostForTaskSpread_MissingTMRatesForSpread()
        {
            WorkspaceDTO workspaceDto = new WorkspaceDTO() { Id = 1, IsUsingTM = true };

            FullWorkspace workspace = new FullWorkspace(workspaceDto);

            ResourceDTO res1 = new ResourceDTO() { Id = 1, SegRegion = WebConstants.SPACE_LEGACY_TM, ResourceName = "Res1", ElementOfCost = ElementOfCostType.Sub, RateType = RateType.Hours };
            ResourceDTO res2 = new ResourceDTO() { Id = 2, SegRegion = WebConstants.SPACE_LEGACY_TM, ResourceName = "Res2", ElementOfCost = ElementOfCostType.Sub, RateType = RateType.Hours };
            ResourceDTO res3 = new ResourceDTO() { Id = 3, SegRegion = WebConstants.SPACE_LEGACY_TM, ResourceName = "Res3", ElementOfCost = ElementOfCostType.Sub, RateType = RateType.Hours };

            TMResourceRateDTO rr11 = new TMResourceRateDTO() { Id = 1, WorkspaceID = workspace.Id, ResourceID = res1.Id, ResourceRate = 0.11m, StartDate = new DateTime(2032, 6, 15), EndDate = new DateTime(2033, 5, 15) };
            TMResourceRateDTO rr12 = new TMResourceRateDTO() { Id = 2, WorkspaceID = workspace.Id, ResourceID = res1.Id, ResourceRate = 0.22m, StartDate = new DateTime(2033, 6, 15), EndDate = new DateTime(2034, 5, 15) };
            TMResourceRateDTO rr13 = new TMResourceRateDTO() { Id = 3, WorkspaceID = workspace.Id, ResourceID = res1.Id, ResourceRate = 0.33m, StartDate = new DateTime(2034, 6, 15), EndDate = new DateTime(2035, 5, 15) };
            TMResourceRateDTO rr21 = new TMResourceRateDTO() { Id = 4, WorkspaceID = workspace.Id, ResourceID = res2.Id, ResourceRate = 0.11m, StartDate = new DateTime(2033, 6, 15), EndDate = new DateTime(2034, 5, 15) };
            TMResourceRateDTO rr22 = new TMResourceRateDTO() { Id = 5, WorkspaceID = workspace.Id, ResourceID = res2.Id, ResourceRate = 0.22m, StartDate = new DateTime(2034, 6, 15), EndDate = new DateTime(2035, 5, 15) };
            TMResourceRateDTO rr31 = new TMResourceRateDTO() { Id = 6, WorkspaceID = workspace.Id, ResourceID = res3.Id, ResourceRate = 0.11m, StartDate = new DateTime(2032, 6, 15), EndDate = new DateTime(2035, 5, 15) };

            ICollection<TMResourceRateDTO> tmResourceRatesFromDB = new Collection<TMResourceRateDTO>() { rr11, rr12, rr13, rr21, rr22, rr31 };

            Collection<BoeTaskElementDTO> taskElements = new Collection<BoeTaskElementDTO>(){
                    new BoeTaskElementDTO() { Id = 1, TaskElementType = TaskElementType.Labor,
                        taskElementLabors = new Collection<ResourceTypeDto>()
                        {
                            new ResourceTypeDto()
                            {
                                ResourceID = res2.Id,
                                SpreadType = IES.Common.SpreadType.Hours,
                                SpreadCurveID = SpreadCurves.SpreadCurve10,
                                ValueSpread = 100,
                                LaborSpreads = new Collection<ResourceSpreadDto>()
                                {
                                    // TM Rates for res2 start June of 2013. This should cause an exception for dates prior to then.
                                    new ResourceSpreadDto() { Id = 1, LaborSpreadValue = 20, LaborSpreadDate = new DateTime(2033,5,15,0,0,0) },
                                    new ResourceSpreadDto() { Id = 2, LaborSpreadValue = 20, LaborSpreadDate = new DateTime(2033,6,15,0,0,0) },
                                    new ResourceSpreadDto() { Id = 3, LaborSpreadValue = 20, LaborSpreadDate = new DateTime(2033,7,15,0,0,0) },
                                    new ResourceSpreadDto() { Id = 4, LaborSpreadValue = 20, LaborSpreadDate = new DateTime(2033,8,15,0,0,0) },
                                    new ResourceSpreadDto() { Id = 5, LaborSpreadValue = 20, LaborSpreadDate = new DateTime(2033,9,15,0,0,0) }
                                }
                            }
                        }
                    }
                };

            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(taskElements);
            retriever.Setup(x => x.GetTMResourceRates((workspaceDto.Id))).Returns(tmResourceRatesFromDB);

			TMCalculator sut = new TMCalculator();
            decimal cost = 0;

			List<ResourceDTO> resources = new List<ResourceDTO> { res1, res2, res3 };

			IDictionary<int, string> resourceIdToSegmentRegion = resources.ToDictionary(r => r.Id, d => d.SegRegion);
			foreach (BoeTaskElementDTO taskElement in workspace.TaskElements)
            {
				//get brc labors based on 1lmx start date
				ICollection<ResourceTypeDto> taskElementLabors = BRCValidationUtility.ProcessLaborTypesForBrc(taskElement.taskElementLabors, resourceIdToSegmentRegion, string.Empty);

                foreach (ResourceTypeDto laborTask in taskElementLabors)
                {
                    cost = sut.TotalCostForTaskSpread(workspace, laborTask);
                }
            }
        }
    }
}

