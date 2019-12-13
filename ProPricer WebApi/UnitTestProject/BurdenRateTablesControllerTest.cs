using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class BurdenRateTablesControllerTest
    {
        [TestMethod]
        public void Get_Burden_Rate_Tables()
        {
            /// Arrange
            using (BurdenRateTablesController controller = new BurdenRateTablesController())
            {

                /// Act
                IEnumerable<BurdenRateTableDto> burdenTables = controller.Get(TestConstants.InstanceId);

                /// Assert
                int count = 0;
                using (IEnumerator<BurdenRateTableDto> enumerator = burdenTables.GetEnumerator())
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