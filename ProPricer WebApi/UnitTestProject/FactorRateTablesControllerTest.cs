using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class FactorRateTablesControllerTest
    {
        [TestMethod]
        public void Get_Factor_Rate_Tables()
        {
            /// Arrange
            using (FactorRateTablesController controller = new FactorRateTablesController())
            {

                /// Act
                IEnumerable<FactorRateTableDto> factorTables = controller.Get(TestConstants.InstanceId);

                /// Assert
                int count = 0;
                using (IEnumerator<FactorRateTableDto> enumerator = factorTables.GetEnumerator())
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