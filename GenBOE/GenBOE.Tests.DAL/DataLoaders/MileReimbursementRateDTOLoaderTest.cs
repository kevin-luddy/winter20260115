using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.Dtos;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class MileReimbursementRateDTOLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_GetMileReimbursementRateDTO()
        {
            var sut = new GenBOE.DataBridge.DTO.MileReimbursementRateDTOLoader();

            MileReimbursementRateDTO reimbursementRate = sut.GetMileReimbursementRateDTO();

            Assert.IsTrue(reimbursementRate != null);
        }
    }
}
