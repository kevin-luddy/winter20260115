using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class ResourcesControllerTest
    {
        [TestMethod]
        public void Get_Resources()
        {
            /// Arrange
            using (ResourcesController controller = new ResourcesController())
            {

                /// Act
                IEnumerable<ResourcesDto> resources = controller.Get(TestConstants.InstanceId);

                /// Assert
                int count = 0;
                using (IEnumerator<ResourcesDto> enumerator = resources.GetEnumerator())
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