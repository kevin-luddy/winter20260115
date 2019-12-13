using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class SpreadControllerTest
    {
        [TestMethod]
        public void Spread_Returns_Months_And_Values()
        {
            string firstCurve = string.Empty;

            // Arrange
            using (CurvesController curves = new CurvesController())
            {

                // get the id of the first curve
                IEnumerable<CurvesDto> curvesResult = curves.Get(TestConstants.InstanceId);

                using (IEnumerator<CurvesDto> enumerator = curvesResult.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        firstCurve = enumerator.Current.Id;
                    }
                }

                SpreadController spread = new SpreadController();

                // Act
                // Spread 100 units over a 12 month period
                IEnumerable<SpreadDto> spreadResult = spread.Get(TestConstants.InstanceId, 100, firstCurve, "2020-01", "2020-12");

                // Assert
                int count = 0;
                using (IEnumerator<SpreadDto> enumerator = spreadResult.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        count++;
                    }
                }

                // should get a set of 12 monthly spread entries back
                Assert.IsTrue(count == 12);
            }
        }
    }
}