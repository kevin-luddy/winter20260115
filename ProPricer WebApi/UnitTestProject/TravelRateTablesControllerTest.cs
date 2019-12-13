using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class TravelRateTablesControllerTest
    {
        [TestMethod]
        public void Get_Travel_Rate_Tables()
        {
            /// Arrange
            using (TravelRateTablesController controller = new TravelRateTablesController())
            {

                /// Act
                IEnumerable<TravelRateTableDto> travelTables = controller.Get(TestConstants.InstanceId);

                /// Assert
                int count = 0;
                using (IEnumerator<TravelRateTableDto> enumerator = travelTables.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        count++;
                    }
                }

                Assert.IsTrue(count > 0);
            }
        }
    }
}