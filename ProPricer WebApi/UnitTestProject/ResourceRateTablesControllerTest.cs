using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class ResourceRateTablesControllerTest
    {
        

        [TestMethod]
        public void Get_Resource_Rate_Tables()
        {
            /// Arrange
            using (ResourceRateTablesController controller = new ResourceRateTablesController())
            {

                /// Act
                IEnumerable<ResourceRateTableDto> resourceTables = controller.Get(TestConstants.InstanceId);

                /// Assert
                int count = 0;
                using (IEnumerator<ResourceRateTableDto> enumerator = resourceTables.GetEnumerator())
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