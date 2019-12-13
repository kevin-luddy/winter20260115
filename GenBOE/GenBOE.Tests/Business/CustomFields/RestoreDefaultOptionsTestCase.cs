using System.Collections.Generic;
using System.Collections.ObjectModel;
using GenBOE.Business.CustomFields;
using IES.Common.classes;
using GenBOE.DataBridge.DTO;
using GenBOE.DataBridge.Reference;
using GenBOE.Dtos;
using GenBOE.Objects;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GenBOE.Tests.Business.CustomFields
{
    [TestClass]
    public class RestoreDefaultOptionsTestCase : MOQObject
    {
        Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();

        [TestMethod]
        public void RestorePerfOrg()
        {
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            var inUseLoader = new Mock<IInUseDataLoader>();

            // the default list to use
            Collection<PerformingOrgDTO> DefaultPerfOrgs = new Collection<PerformingOrgDTO> {
                new PerformingOrgDTO{Id=1, PerformingOrgName="PerfOrg1", PerformingOrgDesc="Perf Org 1"},
                new PerformingOrgDTO{Id=2, PerformingOrgName="PerfOrg2", PerformingOrgDesc="Perf Org 2 GLOBAL"},
                new PerformingOrgDTO{Id=3, PerformingOrgName="PerfOrg3", PerformingOrgDesc="Perf Org 3"},
                new PerformingOrgDTO{Id=4, PerformingOrgName="PerfOrg4", PerformingOrgDesc="Perf Org 4"},
                new PerformingOrgDTO{Id=5, PerformingOrgName="PerfOrg5", PerformingOrgDesc="Perf Org 5"},
                new PerformingOrgDTO{Id=6, PerformingOrgName="PerfOrg6", PerformingOrgDesc="Perf Org 6"}
            };


            // represents the current list to compare with the default one
            Collection<PerformingOrgDTO> perfOrgs = new Collection<PerformingOrgDTO> {
                new PerformingOrgDTO{Id=1, PerformingOrgName="PerfOrg1", PerformingOrgDesc="Perf Org 1"},
                new PerformingOrgDTO{Id=2, PerformingOrgName="PerfOrg2", PerformingOrgDesc="Perf Org 2"},
                new PerformingOrgDTO{Id=3, PerformingOrgName="PerfOrg3", PerformingOrgDesc="Perf Org 3 NOT GLOBAL"},
                new PerformingOrgDTO{Id=4, PerformingOrgName="PerfOrg4", PerformingOrgDesc="Perf Org 4"},
                new PerformingOrgDTO{Id=7, PerformingOrgName="PerfOrg7", PerformingOrgDesc="Perf Org 7"}
            };

            var factory = new Mock<IFullObjectFactory>();
            var retriever = new Mock<IRetriever>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            WorkspaceDTO workspace = new WorkspaceDTO {Id=this.Workspace.Id, PerfOrgListID=this.PerfOrgList.PerformingOrgListID };
            FullWorkspace ws = new FullWorkspace(workspace);

            retriever.Setup(x => x.GetPerformingOrgsByListId(this.PerfOrgList.PerformingOrgListID)).Returns(perfOrgs);
            perfOrgLoader.Setup(x => x.GetGlobalPerformingOrgs()).Returns(DefaultPerfOrgs);

            // performingOrgID of 1 is in use in this case
            HashSet<int> returnedIDs = new HashSet<int> { 1 };
            inUseLoader.Setup(x => x.GetWorkspacePerfOrgIDsInUseByPerfOrgListID(workspace.PerfOrgListID)).Returns(returnedIDs);

            var sut = new RestoreDefaultOptions(inUseLoader.Object, workspaceLoader.Object, perfOrgLoader.Object);

            RestoreOptionData optionsRestored = sut.RestorePerfOrg(ws);

            // there should be 2 options added, PerfOrg5 and PerfOrg6
            Assert.IsTrue(optionsRestored.OptionAdded.Count == 2, "Two options were not added");


            // there should be 2 options changed, PerfOrg2 and PerfOrg3. 
            // the result should have desc Perf Org 2 to Perf Org 2 GLOBAL
            // the result should have desc Perf Org 3 NOT GLOBAL to Perf Org
            Assert.IsTrue(optionsRestored.OptionChanged.Count == 2, "Two options were not changed");

            // there should be 1 option deleted, PerfOrg7
            Assert.IsTrue(optionsRestored.OptionDeleted.Count == 1, "Option was not deleted");
            Assert.IsTrue(optionsRestored.OptionDeleted[0].Name == "PerfOrg7", "PerfOrg7 was not the deleted one");

            // there should be 1 option not changed, PerfOrg1
            Assert.IsTrue(optionsRestored.OptionNotChanged.Count == 1, "OPtion was not changed");
            Assert.IsTrue(optionsRestored.OptionNotChanged[0].Name == "PerfOrg1", "PergOrg1 was not marked as in use");
        }
    }
}
