// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
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

    /// <summary>
    /// Tests the Project Map Sorter
    /// </summary>
    [TestClass]
    public class ProjectMapSorterTest
    {
        #region private Fields
        private Mock<IRetriever> retriever;
        private IFullObjectFactory fullObjectFactory;
        private Mock<ICommonDataMapper> commonDataMapper;
        private Mock<IPermissionsDTODataLoader> permissionsLoader;
        private Mock<IUserDTODataLoader> userLoader;

        #endregion private Fields

        /// <summary>
        /// Initializes the test data.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            fullObjectFactory = new FullObjectFactory(null, null, null, null, null, null, null, null, null, null, null, null);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), fullObjectFactory);

            retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            userLoader = new Mock<IUserDTODataLoader>();

            
            commonDataMapper = new Mock<ICommonDataMapper>();
            permissionsLoader = new Mock<IPermissionsDTODataLoader>();

            PerformingOrgDTO perfOrg = new PerformingOrgDTO()
            {
                IsSystemPerfOrg = false,
                PerformingOrgDesc = "P1",
                PerformingOrgName = "P1",
                Id = 11
            };
            PerformingOrgDTO perfOrg2 = new PerformingOrgDTO()
            {
                IsSystemPerfOrg = false,
                PerformingOrgDesc = "P2",
                PerformingOrgName = "P2",
                Id = 12
            };
            PerformingOrgDTO perfOrg3 = new PerformingOrgDTO()
            {
                IsSystemPerfOrg = false,
                PerformingOrgDesc = "P3",
                PerformingOrgName = "P3",
                Id = 13
            };


            retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg, perfOrg2, perfOrg3 });

            ResourceDTO resource1 = new ResourceDTO()
            {
                ResourceDesc = "R1",
                ResourceName = "R1",
                Id = 21,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Hours
            };

            ResourceDTO resource2 = new ResourceDTO()
            {
                ResourceDesc = "R2",
                ResourceName = "R2",
                Id = 22,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Hours
            };

            ResourceDTO resource3 = new ResourceDTO()
            {
                ResourceDesc = "R3",
                ResourceName = "R3",
                Id = 23,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Hours
            };

            ClinDTO clin = new ClinDTO()
            {
                Id = 1,
                ClinNumber = "1"
            };

            ClinDTO clin2 = new ClinDTO()
            {
                Id = 2,
                ClinNumber = "1.1"
            };

            ClinDTO clin3 = new ClinDTO()
            {
                Id = 3,
                ClinNumber = "1.2"
            };

            retriever.Setup(x => x.GetClinsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullClin>() { new FullClin(clin), new FullClin(clin2), new FullClin(clin3) });

            retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO>() { resource1, resource2, resource3 });

            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), userLoader.Object);
        }
        
        /// <summary>
        /// Tests the sorting of project data
        /// </summary>
        [TestMethod]
        public void Test_OrderBoes()
        {
            ICollection<FullBoe> unsortedBoes = GetUnsortedBoes();
            FullWorkspace ws = new FullWorkspace();

            ICollection<FullBoe> sortedBoes = ProjectMapSorter.OrderBoes(unsortedBoes, ws);

            // This test is set up so the expected sort order will be in order of BOE IDs
            ICollection<FullBoe> expectedSort = unsortedBoes.OrderBy(x => x.Id).ToCollection();

            for (int i = 0; i < sortedBoes.Count; i++)
            {
                Assert.AreEqual(expectedSort.ElementAt(i).Id, sortedBoes.ElementAt(i).Id);
            }
        }

        /// <summary>
        /// Gets the unsorted boes.
        /// </summary>
        /// <returns>A list of unsorted BOEs</returns>
        private ICollection<FullBoe> GetUnsortedBoes()
        {
            // sorted by clin number, resource name, perforg name, activityid
            // ids for clin/resource/pergorg are already sorted correctly inline with their id

            FullBoe full1 = CreateFullBoe(1, 1, 21, 11, "ActivityId1");
            FullBoe full2 = CreateFullBoe(2, 1, 21, 11, "ActivityId2");
            FullBoe full3 = CreateFullBoe(3, 1, 21, 11, "ActivityId3");
            FullBoe full4 = CreateFullBoe(4, 1, 21, 12, "ActivityId3");
            FullBoe full5 = CreateFullBoe(5, 1, 21, 13, "ActivityId3");
            FullBoe full6 = CreateFullBoe(6, 1, 22, 11, "ActivityId1");
            FullBoe full7 = CreateFullBoe(7, 1, 22, 12, "ActivityId1");
            FullBoe full8 = CreateFullBoe(8, 1, 22, 12, "ActivityId2");
            FullBoe full9 = CreateFullBoe(9, 1, 22, 13, "ActivityId1");

            FullBoe full10 = CreateFullBoe(10, 2, 21, 11, "ActivityId1");
            FullBoe full11 = CreateFullBoe(11, 2, 21, 11, "ActivityId2");
            FullBoe full12 = CreateFullBoe(12, 2, 21, 12, "ActivityId1");
            FullBoe full13 = CreateFullBoe(13, 2, 22, 11, "ActivityId1");
            FullBoe full14 = CreateFullBoe(14, 2, 22, 11, "ActivityId2");
            FullBoe full15 = CreateFullBoe(15, 2, 22, 12, "ActivityId1");
            FullBoe full16 = CreateFullBoe(16, 2, 22, 12, "ActivityId2");
            FullBoe full17 = CreateFullBoe(17, 2, 23, 11, "ActivityId1");
            FullBoe full18 = CreateFullBoe(18, 2, 23, 13, "ActivityId1");
            FullBoe full19 = CreateFullBoe(19, 2, 23, 13, "ActivityId3");
            FullBoe full20 = CreateFullBoe(20, 3, 21, 11, "ActivityId1");
            FullBoe full21 = CreateFullBoe(21, 3, 23, 12, "ActivityId1");
            return new List<FullBoe>
            {
                full16, full13, full18, full21, full1, full9, full11, full5, full4, full6, full14, full2, full20, full10, full3, full17, full7, full8, full12, full15, full19
            };
        }

        /// <summary>
        /// Creates the full boe.
        /// </summary>
        /// <param name="boeId">The boe identifier.</param>
        /// <param name="clinId">The clin identifier.</param>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="performingOrgId">The performing org identifier.</param>
        /// <param name="activityId">The activity identifier.</param>
        /// <returns></returns>
        private static FullBoe CreateFullBoe(int boeId, int clinId, int resourceId, int performingOrgId, string activityId)
        {
            BoeDTO boe1 = new BoeDTO
            {
                Id = boeId,
                Title = activityId,
                CLINID = clinId
            };
            FullBoe full1 = new FullBoe(boe1);
            full1.SetTaskElements(new List<BoeTaskElementDTO>
            {
                new BoeTaskElementDTO
                {
                    BoeID = boeId,
                    taskElementLabors = new Collection<ResourceTypeDto>
                    {
                        new ResourceTypeDto
                        {
                            ResourceID = resourceId,
                            PerformingOrgID = performingOrgId
                        }
                    }
                }
            });
            return full1;
        }
    }
}
