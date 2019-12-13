using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.Dtos;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class EscalationRatesDTODataLoaderTest : MOQLoaderObject
    {

        [TestMethod]
        public void L_GetAllDTOs()
        {
            var sut = new GenBOE.DataBridge.DTO.EscalationRatesDTOLoader();

            ICollection<EscalationRatesDTO> escalationRates = sut.GetAll();

            Assert.IsTrue(escalationRates.Count() > 0);
        }
    }
}
