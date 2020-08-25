// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Common
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test the SecurityMapper
    /// </summary>
    [TestClass]
    public class SecurityMapperTest
    {
        /// <summary>
        /// Test ability to retrieve roles for the current user
        /// </summary>
        [TestMethod]
        public void GetRolesforLoggedInUserTest()
        {
            var userAuthDL = new Mock<ISecurityUserAuthorizationsDataLoader>();
            var securityInformation = new Mock<IES.Common.ISecurityInformation>();
            var userDTOMapper = new Mock<IUserMapper>();
            securityInformation.Setup(x => x.ActiveUserNTID).Returns("testuser");
            UserDTO user = new UserDTO();
            user.Id = 21;
            user.Ntid = "testuser";
            userDTOMapper.Setup(x => x.GetByNtid(user.Ntid)).Returns(user);

            // setup admin roles to request
            Collection<SecurityPermissionsResponse> securityPermsResponse = new Collection<SecurityPermissionsResponse>();
            securityPermsResponse.Add(new SecurityPermissionsResponse(PtmRole.Admin, null));

            // Return the security permissions we mock'ed out when data loader is called with user
            userAuthDL.Setup(x => x.GetPermissionsForUser(user)).Returns(securityPermsResponse);

            SecurityMapper sut = new SecurityMapper(userAuthDL.Object,
                                                    securityInformation.Object,
                                                    userDTOMapper.Object);

            // invoke mapper requesting roles 
            IReadOnlyCollection<SecurityPermissionsResponse> returnedPerms = sut.GetRolesForLoggedInUser();
            Assert.IsNotNull(returnedPerms, "Permissions returned from GetRolesForLoggedInUser is null.  Perhaps the delegate call did not work correctly?");
            Assert.IsTrue(returnedPerms.Count == 1);

            // we told the loader to return the user is a system admin
            Assert.AreEqual(PtmRole.Admin, returnedPerms.ElementAt(0).AuthorizedRole);
        }
    }
}
