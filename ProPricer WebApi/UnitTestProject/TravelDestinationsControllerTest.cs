using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class TravelDestinationsControllerTest
    {
        [TestMethod]
        public void Get_Travel_Destinations()
        {
            /// Arrange
            using (TravelDestinationsController controller = new TravelDestinationsController())
            {

                /// Act
                IEnumerable<TravelDestinationsDto> travelDestinations = controller.Get(TestConstants.InstanceId);

                /// Assert
                int count = 0;
                using (IEnumerator<TravelDestinationsDto> enumerator = travelDestinations.GetEnumerator())
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