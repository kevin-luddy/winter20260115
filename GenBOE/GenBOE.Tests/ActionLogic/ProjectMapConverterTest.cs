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
    /// Tests the Project Map Converter
    /// </summary>
    [TestClass]
    public class ProjectMapConverterTest
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
                PerformingOrgDesc = "P",
                PerformingOrgName = "P",
                Id = 15
            };
            retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

            ResourceDTO resource1 = new ResourceDTO()
            {
                ResourceDesc = "R",
                ResourceName = "R",
                Id = 20,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Hours
            };

            ClinDTO clin = new ClinDTO()
            {
                Id = 6
            };

            retriever.Setup(x => x.GetClinsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullClin>() { new FullClin(clin) });

            WbsDTO wbs = new WbsDTO()
            {
                Id = 7
            };

            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullWbs>() { new FullWbs(wbs) });

            retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO>() { resource1 });

            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), userLoader.Object);
        }

        /// <summary>
        /// Tests the group by tiers.
        /// </summary>
        [TestMethod]
        public void Test_GroupByTiers()
        {
            ProjectMapModelView[] data = GetProjectMapModelViews();
            Dictionary<string, ICollection<ProjectMapModelView>> tiers = ProjectMapConverter.GroupByTiers(data);

            Assert.AreEqual(2, tiers.Count);
            foreach (KeyValuePair<string, ICollection<ProjectMapModelView>> tier in tiers)
            {
                if (tier.Key.StartsWith("BSW1023"))
                {
                    Assert.AreEqual(2, tier.Value.Count);
                }
                else
                {
                    Assert.AreEqual(3, tier.Value.Count);
                }
            }
        }

        /// <summary>
        /// Tests the convert to workspace.
        /// </summary>
        [TestMethod]
        public void Test_ConvertToWorkspace()
        {
            FullWorkspace ws = new FullWorkspace();
            ProjectMapModelView[] data = GetProjectMapModelViews();
            Dictionary<string, ICollection<ProjectMapModelView>> tiers = ProjectMapConverter.GroupByTiers(data);
            ConvertedProjectMapDTO convertedDTO = ProjectMapConverter.ConvertToWorkspace(data, ws);
            AssertEquality(data, convertedDTO);

            Assert.AreEqual(tiers.Values.First().Select(t => t.StartDate).Min(), convertedDTO.Boes.First().StartDate);
            Assert.AreEqual(tiers.Values.First().Select(t => t.StartDate).Min(), convertedDTO.Tasks.First().StartDate);
            Assert.AreEqual(tiers.Values.First().Select(t => t.EndDate).Max(), convertedDTO.Boes.First().EndDate);
            Assert.AreEqual(tiers.Values.First().Select(t => t.EndDate).Max(), convertedDTO.Tasks.First().EndDate);

            Assert.AreEqual(tiers.Values.Last().Select(t => t.StartDate).Min(), convertedDTO.Boes.Last().StartDate);
            Assert.AreEqual(tiers.Values.Last().Select(t => t.StartDate).Min(), convertedDTO.Tasks.Last().StartDate);
            Assert.AreEqual(tiers.Values.Last().Select(t => t.EndDate).Max(), convertedDTO.Boes.Last().EndDate);
            Assert.AreEqual(tiers.Values.Last().Select(t => t.EndDate).Max(), convertedDTO.Tasks.Last().EndDate);
        }

        /// <summary>
        /// Asserts the equality of the data compared to the converted data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="convertedDTO">The converted dto.</param>
        /// <exception cref="System.ArgumentNullException">convertedDTO or data</exception>
        public static void AssertEquality(ProjectMapModelView[] data, ConvertedProjectMapDTO convertedDTO)
        {
            if (ReferenceEquals(convertedDTO, null))
            {
                throw new ArgumentNullException(nameof(convertedDTO));
            }

            if (ReferenceEquals(data, null))
            {
                throw new ArgumentNullException(nameof(data));
            }

            Assert.IsNotNull(convertedDTO);
            Assert.AreEqual(2, convertedDTO.Boes.Count);
            Assert.AreEqual(2, convertedDTO.Tasks.Count);
            Assert.AreEqual(2, convertedDTO.Clins.Count);
            Assert.AreEqual(2, convertedDTO.Wbs.Count);

            Assert.IsTrue(convertedDTO.Clins.Any(c => c.ClinNumber == data.First().Clin));
            Assert.IsTrue(convertedDTO.Clins.First(c => c.ClinNumber == data.First().Clin).ClinTitle == data.First().Clin);
            Assert.IsTrue(convertedDTO.Clins.Any(c => c.ClinNumber == data.Last().Clin));
            Assert.IsTrue(convertedDTO.Clins.First(c => c.ClinNumber == data.Last().Clin).ClinTitle == data.Last().Clin);


            Assert.IsTrue(convertedDTO.Wbs.Any(c => c.WbsNumber == data.First().WbsNumber));
            Assert.IsTrue(convertedDTO.Wbs.First(c => c.WbsNumber == data.First().WbsNumber).WbsTitle == data.First().WbsElementTitle);
            Assert.IsTrue(convertedDTO.Wbs.Any(c => c.WbsNumber == data.Last().WbsNumber));
            Assert.IsTrue(convertedDTO.Wbs.First(c => c.WbsNumber == data.Last().WbsNumber).WbsTitle == data.Last().WbsElementTitle);

            AssertAreEqual(data[0], convertedDTO.Boes.First(), convertedDTO.Tasks.First(), convertedDTO.Tasks.First().taskElementLabors.First());
            AssertAreEqual(data[1], convertedDTO.Boes.First(), convertedDTO.Tasks.First(), convertedDTO.Tasks.First().taskElementLabors.Last());
            AssertAreEqual(data[2], convertedDTO.Boes.Last(), convertedDTO.Tasks.Last(), convertedDTO.Tasks.Last().taskElementLabors.First());
            AssertAreEqual(data[3], convertedDTO.Boes.Last(), convertedDTO.Tasks.Last(), convertedDTO.Tasks.Last().taskElementLabors.ElementAt(1));
            AssertAreEqual(data[4], convertedDTO.Boes.Last(), convertedDTO.Tasks.Last(), convertedDTO.Tasks.Last().taskElementLabors.Last());
        }

        /// <summary>
        /// Asserts that they are equal.
        /// </summary>
        /// <param name="projectMapModelView">The project map model view.</param>
        /// <param name="boe">The boe.</param>
        /// <param name="task">The task.</param>
        /// <param name="resource">The resource.</param>
        private static void AssertAreEqual(ProjectMapModelView projectMapModelView, BoeDTO boe, BoeTaskElementDTO task, ResourceTypeDto resource)
        {
            Assert.AreEqual(projectMapModelView.ActivityID, boe.Title);
            Assert.AreEqual(projectMapModelView.ActivityName, boe.Description);
            Assert.AreEqual(projectMapModelView.AddDelete, resource.AddOrDelete);
            Assert.AreEqual(projectMapModelView.Rationale, task.MOQText);
            Assert.AreEqual(projectMapModelView.SowNumber, boe.SOW);
            Assert.AreEqual(projectMapModelView.SowTitle, boe.SOWTitle);
            Assert.AreEqual(projectMapModelView.Task, task.Description);
            Assert.AreEqual(projectMapModelView.CamName, boe.CamName);
            Assert.AreEqual(projectMapModelView.Category, boe.Category);
            Assert.AreEqual(resource.PerformingOrgID, 15);
            Assert.AreEqual(projectMapModelView.EndDate.Normalize(), resource.EndDate.Normalize());
            Assert.AreEqual(projectMapModelView.LegacyID, resource.LegacyID);
            if (resource.SpreadType == SpreadType.Cost)
            {
                Assert.AreEqual(projectMapModelView.Dollars, resource.ValueSpread);
            }
            else
            {
                Assert.AreEqual(projectMapModelView.Hours, resource.ValueSpread);
            }
            Assert.AreEqual(resource.ResourceID, 20);
            Assert.AreEqual(projectMapModelView.Offload, resource.CanOffload);
            Assert.AreEqual(projectMapModelView.TieredPercentage, resource.TieredPercentage);
        }

        /// <summary>
        /// Gets the project map model views.
        /// </summary>
        /// <returns>List of dummy project map model views used in the above tests and DAL tests.</returns>
        public static ProjectMapModelView[] GetProjectMapModelViews()
        {

            var projectMapRows = new[]
            {
                new ProjectMapModelView { ClassOfCost= "DNR", LegacyID = 1, WbsNumber = "4.01.06.01", CostCenter = "P", ActivityID = "BSW1023", ActivityName = "A/C #1 - Conduct MDSC Review & Resolve Issues - Pre Instrumentation", WbsElementTitle = "Development Test Reviews", InitialResource = "R", StartDate = DateTime.Parse("1/15/2019"), EndDate = DateTime.Parse("2/1/2019"), Clin = "0001", SowNumber = "MOT", SowTitle = "TEST READINESS REVIEW (TRR), DEVELOPMENT TEST AWR", Task = "Flight Test Engineers - Special Projects (4043WWC) are to conduct and participate in Sikorsky\'s Model Development Safety Committee (MDSC) Review.  This review is pre-instrumentation.  This is to review the SANG program\'s overview (presentation), Test Plans, and safety data before a flight release can be provided for the Development Test (DT) program.", Hours = 6, Dollars = 0, Rationale = "Flight Test Engineers - Special Projects (4043WWC) are to participate.", CamName = "Stange, B.", Category = "ENG", Offload = true, AddDelete = "D", TieredPercentage = 2.3m},
                new ProjectMapModelView { ClassOfCost= "DNR", LegacyID = 2, WbsNumber = "4.01.06.01", CostCenter = "P", ActivityID = "BSW1023", ActivityName = "A/C #1 - Conduct MDSC Review & Resolve Issues - Pre Instrumentation", WbsElementTitle = "Development Test Reviews", InitialResource = "R", StartDate = DateTime.Parse("01/15/2019"), EndDate = DateTime.Parse("02/01/2019"), Clin = "0001", SowNumber = "MOT", SowTitle = "TEST READINESS REVIEW (TRR), DEVELOPMENT TEST AWR", Task = "Flight Test Engineers - Special Projects (4043WWC) are to conduct and participate in Sikorsky\'s Model Development Safety Committee (MDSC) Review.  This review is pre-instrumentation.  This is to review the SANG program\'s overview (presentation), Test Plans, and safety data before a flight release can be provided for the Development Test (DT) program.", Hours = 6, Dollars = 0, Rationale = "Flight Test Engineers - Special Projects (4043WWC) are to participate.", CamName = "Stange, B.", Category = "ENG", TieredPercentage = 3.4m},
                new ProjectMapModelView { ClassOfCost= "DNR", LegacyID = 3, WbsNumber = "3.05", CostCenter = "P", ActivityID = "BSW1209", ActivityName = "2018 System/Software Quality Assurance", WbsElementTitle = "System/Software Quality Assurance", InitialResource = "R", StartDate = DateTime.Parse("01/02/2018"), EndDate = DateTime.Parse("12/24/2018"), Clin = "0002", SowNumber = "3.5.15", SowTitle = "Software Quality Assurance", Task = "The Contractor shall conduct on-going software quality evaluations of software development", CamName = "Sawyer (Kaczor)", Category = "ENG"},
                new ProjectMapModelView { ClassOfCost= "DNR", WbsNumber = "3.05", CostCenter = "P", ActivityID = "BSW1209", ActivityName = "2018 System/Software Quality Assurance", WbsElementTitle = "System/Software Quality Assurance", InitialResource = "R", StartDate = DateTime.Parse("01/02/2019"), EndDate = DateTime.Parse("12/24/2019"), Clin = "0002", SowNumber = "3.5.15", SowTitle = "Software Quality Assurance", Task = "The Contractor shall conduct on-going software quality evaluations of software development", CamName = "Sawyer (Kaczor)", Category = "ENG"},
                new ProjectMapModelView { ClassOfCost= "DNR", WbsNumber = "3.05", CostCenter = "P", ActivityID = "BSW1209", ActivityName = "2018 System/Software Quality Assurance", WbsElementTitle = "System/Software Quality Assurance", InitialResource = "R", StartDate = DateTime.Parse("01/02/2020"), EndDate = DateTime.Parse("10/13/2020"), Clin = "0002", SowNumber = "3.5.15", SowTitle = "Software Quality Assurance", Task = "The Contractor shall conduct on-going software quality evaluations of software development", CamName = "Sawyer (Kaczor)", Category = "ENG"}
            };

            Random rand = new Random(5);
            foreach (var mv in projectMapRows)
            {
                mv.StartDate = mv.StartDate.Normalize();
                mv.EndDate = mv.EndDate.Normalize();
                mv.DiscreteMonths = new decimal?[204];
                for (int i = 0; i < 204; i++)
                {
                    mv.DiscreteMonths[i] = rand.Next(10);
                }
            }

            return projectMapRows;
        }
    }
}
