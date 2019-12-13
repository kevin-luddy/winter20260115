// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GenBOEMetricsDTODataLoaderTest
    {
        [TestMethod]
        public void L_GetGenBOEMetrics()
        {
            Collection<Workspace> wsData;
            Collection<BOE> boeData;
            
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                wsData = gbe.Workspaces.Where(w => !w.IsDeleted.HasValue || !w.IsDeleted.Value).ToCollection();

                boeData = gbe.BOEs.Where(b => !b.Workspace.IsDeleted.HasValue || !b.Workspace.IsDeleted.Value).ToCollection();
            }

            GenBOEMetricsDTODataLoader GenBOEMetricsDTODataLoaderToTest = new GenBOEMetricsDTODataLoader();

            GenBOEMetricsDTO resultsFromLoader = GenBOEMetricsDTODataLoaderToTest.GetGenBOEMetrics();

            Assert.IsTrue(resultsFromLoader.WorkspacesAll == wsData.Count());
            Assert.IsTrue(resultsFromLoader.WorkspacesClosed == wsData.Where(x => x.WorkspaceStateID == (int)WorkspaceState.Closed).Count());
            Assert.IsTrue(resultsFromLoader.WorkspacesComplete == wsData.Where(x => x.WorkspaceStateID == (int)WorkspaceState.Complete).Count());
            Assert.IsTrue(resultsFromLoader.WorkspacesInitialization == wsData.Where(x => x.WorkspaceStateID == (int)WorkspaceState.Initialization).Count());
            Assert.IsTrue(resultsFromLoader.WorkspacesWorking == wsData.Where(x => x.WorkspaceStateID == (int)WorkspaceState.Working).Count());

            Assert.IsTrue(resultsFromLoader.BoesAll == boeData.Count());
            Assert.IsTrue(resultsFromLoader.BoesApproved == boeData.Where(x => x.BOEStateID == (int)BOEState.Approved).Count());
            Assert.IsTrue(resultsFromLoader.BoesAwaitingApproval == boeData.Where(x => x.BOEStateID == (int)BOEState.AwaitingApproval).Count());
            Assert.IsTrue(resultsFromLoader.BoesDraft == boeData.Where(x => x.BOEStateID == (int)BOEState.Draft).Count() + boeData.Where(x => x.BOEStateID == (int)BOEState.DraftLocked).Count());
            Assert.IsTrue(resultsFromLoader.BoesUnassigned == boeData.Where(x => x.BOEStateID == (int)BOEState.Unassigned).Count());
        }
    }
}
