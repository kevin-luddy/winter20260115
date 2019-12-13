using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class UserControllerTest
    {
        [TestMethod]
        public void Get_Users()
        {
            // Arrange
            using (UserController controller = new UserController())
            {

                // Act
                IEnumerable<UserDto> users = controller.Get(TestConstants.InstanceId);

                // Assert
                int count = 0;
                using (IEnumerator<UserDto> enumerator = users.GetEnumerator())
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
        public void Get_User_Role()
        {
            string firstuser = null;
            // Arrange
            using (UserController controller = new UserController())
            {
                IEnumerable<UserDto> users = controller.Get(TestConstants.InstanceId);

                using (IEnumerator<UserDto> enumerator = users.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        firstuser = enumerator.Current.Id;
                    }
                }

                // Act
                RolesDto role = controller.Get(TestConstants.InstanceId, firstuser);

                // Assert
                Assert.IsTrue(role.Id != null);
            }
        }
    }
}