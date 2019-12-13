using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class CurvesControllerTest
    {
        [TestMethod]
        public void Get_Curve_List_Returns_Curves()
        {
            /// Arrange
            using (CurvesController controller = new CurvesController())
            {

                /// Act
                IEnumerable<CurvesDto> curvesResult = controller.Get(TestConstants.InstanceId);

                /// Assert
                int count = 0;
                using (IEnumerator<CurvesDto> enumerator = curvesResult.GetEnumerator())
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