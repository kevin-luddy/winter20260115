using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class RolesControllerTest
    {
        [TestMethod]
        public void Get_Roles()
        {
            /// Arrange
            using (RolesController controller = new RolesController())
            {

                /// Act
                IEnumerable<RolesDto> roles = controller.Get(TestConstants.InstanceId);

                /// Assert
                int count = 0;
                using (IEnumerator<RolesDto> enumerator = roles.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        count++;
                    }
                }

                Assert.IsTrue(count > 0);
            }
        }

        [TestMethod]
        public void Get_Users_In_Role()
        {
            string firstrole = null;
            /// Arrange
            using (RolesController controller = new RolesController())
            {

                IEnumerable<RolesDto> roles = controller.Get(TestConstants.InstanceId);

                using (IEnumerator<RolesDto> enumerator = roles.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        firstrole = enumerator.Current.Id;
                    }
                }

                /// Act
                IEnumerable<UserDto> users = controller.Get(TestConstants.InstanceId, firstrole);
                int count = 0;
                using (IEnumerator<UserDto> enumerator = users.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        count++;
                    }
                }

                /// Assert
                Assert.IsTrue(count > 0);
            }
        }
    }
}