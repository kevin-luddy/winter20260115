using System.Collections.ObjectModel;
using GenBOE.DataBridge.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.Dtos;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class WorkspaceSearchDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_GetWorkspaceSearchResults()
        {
            var sut = new WorkspaceSearchDTODataLoader();

            
            // set up workspaceSearch to have data that is similar to what would be set up using the GlobalTestCaseSetup
            WorkspaceSearchDTO workspaceSearch = new WorkspaceSearchDTO { WorkspaceName = "*", WorkspaceDesc = null, CostVolumeLead = null, ProposalSubmitStartDate = null, ProposalSubmitEndDate = null, RFPNumber = null };

            Collection<int> results = sut.GetWorkspaceSearchResults(workspaceSearch);
            Assert.IsTrue(results.Count > 0, "No results found");
        }
    }
}
