using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class BOESearchDTODataLoaderTest
    {
        [ClassInitialize]
        static public void Initialize(TestContext testContext)
        {
            // Creates a searchable workspace
            // searchableWorkspaceID = GlobalTestCaseSetup.CreateWorkspace();

            // Set workspace as searchable
            // Set OCI to false

            // Create CLINs
            // Create WBSs and associate to...
            // Create BOEs for searching
            // Assign the BOEs to a user

            // Set workspace state to Working

            // Submit BOEs for approval
            // Approve BOEs

            // Set workspace state to Complete
        }

        //[TestMethod]
        //public void L_GetQuickSearchResults()
        //{
        //    var sut = new BOESearchDTODataLoader();

        //    // search on the word "note" in the boe template, it will return results
        //    BOESearchDTO searchQ = new BOESearchDTO { WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID, SelectedCategory = SearchCategory.BOEContentTemplates, QuickSearchText = "note" };

        //    Collection<int> returnedIDs = sut.GetQuickSearchResults(searchQ);
        //    Assert.IsTrue(returnedIDs.Any(), "No returned results");
        //}

        [TestMethod]
        public void GetAdvancedSearchResults()
        {

        }        
    }
}
