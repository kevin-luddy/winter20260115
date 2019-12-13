using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using GenBOE.DataBridge.DTO;
using System.Threading;
using GenBOE.Dtos;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class FindReplaceDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_getFindReferencesBOEDesc()
        {
            FindReplaceDTODataLoader sut = new FindReplaceDTODataLoader();

            FindReplaceDTO searchParams = new FindReplaceDTO();
 
            searchParams.FindText = "BOE";
            searchParams.WorkspaceID = this.Boe1.WorkspaceID; 

            Thread.Sleep(10000); // for some reason we need this sleep or this test case will always fail
            Collection<FindReplaceDTO> BOEDescresultsFromLoader = sut.getFindReferences(searchParams, searchParams.WorkspaceID);

            Assert.IsTrue(BOEDescresultsFromLoader.Count > 0, "Didn't find any matching results");
            FindReplaceDTO firstResponse = BOEDescresultsFromLoader.First();
            Assert.IsTrue(firstResponse.BOEId > 0, "Boe ID isn't valid");
            Assert.IsTrue(firstResponse.FoundText.Contains("BOE"), "couldn't match find text");
        }

        [TestMethod]
        public void L_getFindReferencesBOEDataSource()
        {
            FindReplaceDTODataLoader sut = new FindReplaceDTODataLoader();

            FindReplaceDTO searchParams = new FindReplaceDTO();

            searchParams.FindText = "Data";
            searchParams.WorkspaceID = this.Boe1.WorkspaceID;
            Thread.Sleep(10000); // for some reason we need this sleep or this test case will always fail
            Collection<FindReplaceDTO> DataSourceresultsFromLoader = sut.getFindReferences(searchParams, searchParams.WorkspaceID);

            // Assert
            Assert.IsTrue(DataSourceresultsFromLoader.Count > 0, "Didn't find any matching results");
            FindReplaceDTO firstResponse = DataSourceresultsFromLoader.First();
            Assert.IsTrue(firstResponse.BOEId > 0, "Boe ID isn't valid");
            Assert.IsTrue(firstResponse.FoundText.Contains("Data"), "couldn't match find text");

  
        }

        [TestMethod]
        public void L_getFindReferencesTaskTitle()
        {

            FindReplaceDTODataLoader sut = new FindReplaceDTODataLoader();

            FindReplaceDTO searchParams = new FindReplaceDTO();

            searchParams.WorkspaceID = this.Workspace.Id;
            searchParams.FindText = this.TaskElement.TaskTitle;

            Thread.Sleep(4500); // for some reason we need this sleep or this test case will always fail
            Collection<FindReplaceDTO> TaskTitleresultsFromLoader = sut.getFindReferences(searchParams, this.Workspace.Id);
          
            // Assert
            Assert.IsTrue(TaskTitleresultsFromLoader.Count > 0, "Didn't find any matching results");
            FindReplaceDTO firstResponse = TaskTitleresultsFromLoader.First();
            Assert.IsTrue(firstResponse.TaskElementID > 0, "Task Element ID isn't valid");
            Assert.IsTrue(firstResponse.FoundText.Contains(this.TaskElement.TaskTitle), "couldn't match find text");

        }

        [TestMethod]
        public void L_getFindReferencesTaskDesc()
        {
            FindReplaceDTODataLoader sut = new FindReplaceDTODataLoader();

            FindReplaceDTO searchParams = new FindReplaceDTO();

            searchParams.WorkspaceID = this.Workspace.Id;
            searchParams.FindText = "great";

            Thread.Sleep(4500); // for some reason we need this sleep or this test case will always fail
            Collection<FindReplaceDTO> TaskDescresultsFromLoader = sut.getFindReferences(searchParams, this.Workspace.Id);

            // Assert
            Assert.IsTrue(TaskDescresultsFromLoader.Count > 0, "Didn't find any matching results");
            FindReplaceDTO firstResponse = TaskDescresultsFromLoader.First();
            Assert.IsTrue(firstResponse.TaskElementID > 0, "Task Element ID isn't valid");
            Assert.IsTrue(firstResponse.FoundText.Contains("great"), "couldn't match find text");

           
        }

        [TestMethod]
        public void L_getFindReferencesMOQText()
        {
            FindReplaceDTODataLoader sut = new FindReplaceDTODataLoader();

            FindReplaceDTO searchParams = new FindReplaceDTO();
            searchParams.WorkspaceID = this.Workspace.Id;
            searchParams.FindText = "hours";

            Thread.Sleep(4500); // for some reason we need this sleep or this test case will always fail
            Collection<FindReplaceDTO> MOQTextresultsFromLoader = sut.getFindReferences(searchParams, this.Workspace.Id);
       
            // Assert
            Assert.IsTrue(MOQTextresultsFromLoader.Count > 0, "Didn't find any matching results");
            FindReplaceDTO firstResponse = MOQTextresultsFromLoader.First();
            Assert.IsTrue(firstResponse.TaskElementID > 0, "Task Element ID isn't valid");
            Assert.IsTrue(firstResponse.FoundText.Contains("hours"), "couldn't match find text");
        }
    }
}
