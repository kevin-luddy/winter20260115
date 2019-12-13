using System.Collections.ObjectModel;
using IES.Common;
using GenBOE.DataBridge.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.Dtos;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class WorkspaceHistoryDTODataLoaderTest : MOQLoaderObject
    {
        public void SaveWorkspaceSettings()
        {
            var sut = new WorkspaceDTODataLoader();

            // Get the workspace data
            WorkspaceDTO workspaceModel = sut.GetById(this.Workspace.Id);

            workspaceModel.WorkspaceState = WorkspaceState.Initialization;

            // Save the workspace settings
            sut.SaveWorkspaceSettings(this.Author.UserID, workspaceModel);

            // Get the workspace data
            workspaceModel = sut.GetById(this.Workspace.Id);

            workspaceModel.WorkspaceState = WorkspaceState.Working;

            // Save the workspace settings
            sut.SaveWorkspaceSettings(this.Author.UserID, workspaceModel);

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetWorkspaceHistoryDTO()
        {
            var sut = new WorkspaceHistoryDTODataLoader();
            Collection<WorkspaceHistoryDTO> WorkspaceHistory = new Collection<WorkspaceHistoryDTO>();
            WorkspaceHistory = sut.GetWorkspaceHistory(this.Workspace.Id);

            // Assert
            Assert.IsTrue(WorkspaceHistory != null, "Workspace history was null");
            Assert.IsTrue(WorkspaceHistory.Count > 0);

            this.SaveWorkspaceSettings();
        }
    }
}
