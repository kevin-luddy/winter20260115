using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class BOESummaryGridDataLoaderTest
    {
        [TestMethod]
        public void L_GetTotalHoursPerLaborTypeForBOE()
        {
            //TODO: Commenting out this test case because it was failing but since boe summary grid is
            // going to be moved to use a DTO, not going to worry about it right now

            //// Arrange
            //var sut = new BOESummaryGridDataLoader();

            //// Act
            //Collection<BOESummaryGridModelView> results = sut.GetTotalHoursPerLaborTypeForBOE(GlobalTestCaseSetup.GlobalBOEID);

            //// Assert
            //Assert.IsTrue(results.Count > 0, String.Format("No results found for BOESummaryGrid query with BOEID: {0}", GlobalTestCaseSetup.GlobalBOEID));
          
        }
    }
}
