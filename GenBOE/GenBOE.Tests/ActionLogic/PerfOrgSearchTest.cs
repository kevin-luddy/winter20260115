// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Common.Search;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PerfOrgSearchTest : MOQObject
    {
        Mock<IPerformingOrgDTODataLoader> PerfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();

        [TestMethod]
        public void SearchDefaultPerfOrgs()
        {
            // the default list to use
            Collection<PerformingOrgDTO> DefaultPerfOrgs = new Collection<PerformingOrgDTO> {
            new PerformingOrgDTO{Id=1, PerformingOrgName="PerfOrg1", PerformingOrgDesc="Perf Org 1"},
            new PerformingOrgDTO{Id=2, PerformingOrgName="PerfOrg2", PerformingOrgDesc="Perf Org 2 GLOBAL"},
            new PerformingOrgDTO{Id=3, PerformingOrgName="PerfOrg3", PerformingOrgDesc="Perf Org 3"},
            new PerformingOrgDTO{Id=4, PerformingOrgName="PerfOrg4", PerformingOrgDesc="Perf Org 4 GLOBAL"},
            new PerformingOrgDTO{Id=5, PerformingOrgName="PerfOrg5", PerformingOrgDesc="Perf Org 5"},
            new PerformingOrgDTO{Id=6, PerformingOrgName="Perglobal", PerformingOrgDesc="Perf Org 6"}

            };
            PerfOrgLoader.Setup(x=>x.GetGlobalPerformingOrgs()).Returns(DefaultPerfOrgs);

            var sut = new PerfOrgSearch(PerfOrgLoader.Object);
            Collection<PerformingOrgDTO> searchResults = sut.SearchDefaultPerfOrgs("GLOB");
            Collection<PerformingOrgDTO> searchResults2 = sut.SearchDefaultPerfOrgs("glob");
            Collection<PerformingOrgDTO> searchResults3 = sut.SearchDefaultPerfOrgs("GlOb");
            // GLOB should have been found in 2 desc's and one name
            Assert.IsTrue(searchResults.Count == 3, "GlOB was not found");
            Assert.IsTrue(searchResults2.Count == 3, "GlOB was not found");
            Assert.IsTrue(searchResults3.Count == 3, "GlOB was not found");

        }

        [TestMethod]
        public void SearchDefaultPerfOrgs_NoResults()
        {
            // the default list to use
            Collection<PerformingOrgDTO> DefaultPerfOrgs = new Collection<PerformingOrgDTO> {
            new PerformingOrgDTO{Id=1, PerformingOrgName="PerfOrg1", PerformingOrgDesc="Perf Org 1"},
            new PerformingOrgDTO{Id=2, PerformingOrgName="PerfOrg2", PerformingOrgDesc="Perf Org 2 GLOBAL"},
            new PerformingOrgDTO{Id=3, PerformingOrgName="PerfOrg3", PerformingOrgDesc="Perf Org 3"},
            new PerformingOrgDTO{Id=4, PerformingOrgName="PerfOrg4", PerformingOrgDesc="Perf Org 4 GLOBAL"},
            new PerformingOrgDTO{Id=5, PerformingOrgName="PerfOrg5", PerformingOrgDesc="Perf Org 5"},
            new PerformingOrgDTO{Id=6, PerformingOrgName="PerGLOBAL", PerformingOrgDesc="Perf Org 6"}

            };
            PerfOrgLoader.Setup(x => x.GetGlobalPerformingOrgs()).Returns(DefaultPerfOrgs);

            var sut = new PerfOrgSearch(PerfOrgLoader.Object);
            Collection<PerformingOrgDTO> searchResults = sut.SearchDefaultPerfOrgs("BLAH");

            // BLAH was not found
            Assert.IsTrue(searchResults.Count == 0, "BLAH WAS FOUND");

        }
    }
}
