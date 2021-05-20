// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.Common
{
    using System;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Security.Principal;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using GenBOE.Dtos;
    using IES.Common;

    [TestClass]
    public class SecurityInformationTest
    {
        [TestMethod]
        public void GetRoleSecurityRolesAsStringsNULL()
        {
            var adUtils = new Mock<IActiveDirectoryUtilities>();
            var memCache = new Mock<MemoryCache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);

            string returned = sut.GetRoleAsString((Collection<Role>)null);
            Assert.AreEqual("<none>", returned);
        }

        [TestMethod]
        public void GetRoleSecurityRolesAsStringsEmpty()
        {
            var adUtils = new Mock<IActiveDirectoryUtilities>();
            var memCache = new Mock<MemoryCache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);

            Assert.AreEqual("", sut.GetRoleAsString(new Collection<Role>()));
        }

        [TestMethod]
        public void GetRoleSecurityRolesAsStringsValued()
        {
            var adUtils = new Mock<IActiveDirectoryUtilities>();
            var memCache = new Mock<MemoryCache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);

            Collection<Role> roles = new Collection<Role>();
            roles.Add(Role.WorkspaceReviewer);
            roles.Add(Role.Approver);


            Assert.AreEqual("WorkspaceReviewer,Approver", sut.GetRoleAsString(roles));
        }

        [TestMethod]
        public void IsDomesticUserTest()
        {
            var adUtils = new Mock<IActiveDirectoryUtilities>();
            var memCache = new Mock<MemoryCache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);

            string ingroupdomestic = "mockindomestic";

            var principalMock = new Mock<IPrincipal>();
            principalMock.Setup(x => x.IsInRole(ingroupdomestic)).Returns(false);
            principalMock.Setup(x => x.IsInRole(@"US\EBS.EstimationInitiative.DevTeam")).Returns(false);

            // first test to make sure things return false when they have no membership
            bool returned = sut.IsDomesticUser(principalMock.Object);
            Assert.IsFalse(returned);
            principalMock.Verify(x => x.IsInRole(ingroupdomestic), Times.Exactly(1));
            principalMock.Verify(x => x.IsInRole(@"US\EBS.EstimationInitiative.DevTeam"), Times.Exactly(1));

            // now test that if we're an a* account we work ok
            principalMock.Setup(x => x.IsInRole(@"US\EBS.EstimationInitiative.DevTeam")).Returns(true);
            returned = sut.IsDomesticUser(principalMock.Object);
            Assert.IsTrue(returned);
            principalMock.Verify(x => x.IsInRole(ingroupdomestic), Times.Exactly(2));
            principalMock.Verify(x => x.IsInRole(@"US\EBS.EstimationInitiative.DevTeam"), Times.Exactly(2));

            // now test that if we're in the domestic we're good
            principalMock.Setup(x => x.IsInRole(ingroupdomestic)).Returns(true);
            returned = sut.IsDomesticUser(principalMock.Object);
            Assert.IsTrue(returned);
            principalMock.Verify(x => x.IsInRole(ingroupdomestic), Times.Exactly(3));
            principalMock.Verify(x => x.IsInRole(@"US\EBS.EstimationInitiative.DevTeam"), Times.Exactly(2));
        }
        
        /// <summary>
        /// Validates IsSubcontractorUser for a non-subcontractor
        /// </summary>
        [TestMethod]
        public void isSubcontractorUser_NotSub()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<ICache> memCache = new Mock<ICache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTO user = new UserDTO() { NTID = "test", IsSubcontractor = false };

            bool returned = sut.IsSubcontractorUser(user.NTID, user.IsSubcontractor);
            Assert.IsFalse(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// Validates IsSubcontractorUser for a subcontractor
        /// </summary>
        [TestMethod]
        public void isSubcontractorUser_Sub()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<ICache> memCache = new Mock<ICache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTO user = new UserDTO() { NTID = "test", IsSubcontractor = true };
            adUtils.Setup(x => x.IsMemberOfADGroup(user.NTID, It.IsAny<string>())).Returns(false);

            bool returned = sut.IsSubcontractorUser(user.NTID, user.IsSubcontractor);
            Assert.IsTrue(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(user.NTID, It.IsAny<string>()), Times.Once());
        }

        /// <summary>
        /// Validates IsSubcontractorUser for a subcontractor who is treated as an LM user
        /// </summary>
        [TestMethod]
        public void isSubcontractorUser_TreatedAsLmUser()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<ICache> memCache = new Mock<ICache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTO user = new UserDTO() { NTID = "test", IsSubcontractor = true };
            adUtils.Setup(x => x.IsMemberOfADGroup(user.NTID, It.IsAny<string>())).Returns(true);

            bool returned = sut.IsSubcontractorUser(user.NTID, user.IsSubcontractor);
            Assert.IsFalse(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(user.NTID, It.IsAny<string>()), Times.Once());
        }

        /// <summary>
        /// Validates IsSubcontractorUser caching
        /// </summary>
        [TestMethod]
        public void isSubcontractorUser_Cache()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<ICache> memCache = new Mock<ICache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTO user = new UserDTO() { NTID = "test", IsSubcontractor = true };
            memCache.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
            memCache.Setup(x => x.GetData(It.IsAny<string>())).Returns(true);

            bool returned = sut.IsSubcontractorUser(user.NTID, user.IsSubcontractor);
            Assert.IsTrue(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// Test argument null is thrown when ntid is null for IsSubcontractorUser
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void isSubcontractorUser_Exception_NullNtid()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<ICache> memCache = new Mock<ICache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);
            sut.IsSubcontractorUser((string)null, false);
        }

        /// <summary>
        /// Test argument null is thrown when IsSubcontractor is null for IsSubcontractorUser
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void isSubcontractorUser_Exception_NullIsSubcontractor()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<ICache> memCache = new Mock<ICache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);
            sut.IsSubcontractorUser("test", null);
        }

        /// <summary>
        /// Validates IsSubcontractorUser with override set for a subcontractor
        /// </summary>
        public void isSubcontractorUser_Override_Sub()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<ICache> memCache = new Mock<ICache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTO user = new UserDTO() { NTID = "test", IsSubcontractor = true };
            ConfigurationManager.AppSettings["OverrideSubNonUs"] = "true";

            bool returned = sut.IsSubcontractorUser(user.NTID, user.IsSubcontractor);
            Assert.IsFalse(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// Validates IsSubcontractorUser with override set for a non-subcontractor
        /// </summary>
        [TestMethod]
        public void isSubcontractorUser_Override_NotSub()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<ICache> memCache = new Mock<ICache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTO user = new UserDTO() { NTID = "test", IsSubcontractor = false };
            ConfigurationManager.AppSettings["OverrideSubNonUs"] = "true";

            bool returned = sut.IsSubcontractorUser(user.NTID, user.IsSubcontractor);
            Assert.IsFalse(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// Validates IsSubcontractorUser with override set for a user with IsSubcontractor set to null
        /// </summary>
        [TestMethod]
        public void isSubcontractorUser_Override_Null()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<ICache> memCache = new Mock<ICache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTO user = new UserDTO() { NTID = "test", IsSubcontractor = null };
            ConfigurationManager.AppSettings["OverrideSubNonUs"] = "true";

            bool returned = sut.IsSubcontractorUser(user.NTID, user.IsSubcontractor);
            Assert.IsFalse(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// Validates IsRdmAdminUser(domain, name) method
        /// </summary>
        [TestMethod]
        public void isRdmAdminUser_ById()
        {
            var adUtils = new Mock<IActiveDirectoryUtilities>();
            var memCache = new MemoryCache();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache);

            memCache.ClearCache();

            string inlmemp = "employee";
            string inrdmadmin = "rdmadmin";
            string rdmAdminGroup1 = "ebs.ies.dev.rdmadmins";
            string rdmAdminGroup2 = "ebs.ies.rdmadmins";

            // Mock Employee as member of neither RDM Admin group
            adUtils.Setup(x => x.IsMemberOfADGroup(inlmemp, rdmAdminGroup1)).Returns(false);
            adUtils.Setup(x => x.IsMemberOfADGroup(inlmemp, rdmAdminGroup2)).Returns(false);
            // Mock RdmAdmin as member of rdmAdminGroup1, but no other groups
            adUtils.Setup(x => x.IsMemberOfADGroup(inrdmadmin, rdmAdminGroup1)).Returns(true);
            adUtils.Setup(x => x.IsMemberOfADGroup(inrdmadmin, rdmAdminGroup2)).Returns(false);

            // first test to make sure things return false when they have no membership
            bool returned = sut.IsRdmAdminUser(inlmemp);
            Assert.IsFalse(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(inlmemp, rdmAdminGroup1), Times.Exactly(1));
            adUtils.Verify(x => x.IsMemberOfADGroup(inlmemp, rdmAdminGroup2), Times.Exactly(1));

            memCache.ClearCache();

            // now test that if we're in the RDM Admin role we're good
            returned = sut.IsRdmAdminUser(inrdmadmin);
            Assert.IsTrue(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(inrdmadmin, rdmAdminGroup1), Times.Exactly(1));
            adUtils.Verify(x => x.IsMemberOfADGroup(inrdmadmin, rdmAdminGroup2), Times.Exactly(0));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void isRdmAdminUser_ById_Exception_noName()
        {
            var adUtils = new Mock<IActiveDirectoryUtilities>();
            var memCache = new Mock<MemoryCache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);
            sut.IsRdmAdminUser((string)null);
        }

        [TestMethod]
        public void isAllowedProPricerAccessTest()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            MemoryCache memCache = new MemoryCache();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache);

            memCache.ClearCache();

            string noAccess = "no access";
            string hasAccess = "has access";
            string group = "ebs.estimationinitiative.approveddevandtestusers";

            adUtils.Setup(x => x.IsMemberOfADGroup(noAccess, group)).Returns(false);
            adUtils.Setup(x => x.IsMemberOfADGroup(hasAccess, group)).Returns(true);

            bool result = sut.IsAllowedProPricerAccess(noAccess);
            Assert.IsFalse(result);
            adUtils.Verify(x => x.IsMemberOfADGroup(noAccess, group), Times.Exactly(1));

            memCache.ClearCache();
            
            result = sut.IsAllowedProPricerAccess(hasAccess);
            Assert.IsTrue(result);
            adUtils.Verify(x => x.IsMemberOfADGroup(hasAccess, group), Times.Exactly(1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void isAllowedProPricerAccessTest_NoName()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<MemoryCache> memCache = new Mock<MemoryCache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);
            sut.IsAllowedProPricerAccess(null);
        }

        /// <summary>
        /// Validates IsMemberOfADGroupInAppSettingsList(domain, name) method
        /// </summary>
        [TestMethod]
        public void IsMemberOfADGroupInAppSettingsList_ById()
        {
            var adUtils = new Mock<IActiveDirectoryUtilities>();
            var memCache = new MemoryCache();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache);

            memCache.ClearCache();

            string emp1 = "employee1";
            string emp2 = "employee2";
            string group1 = "ebs.group1";
            string group2 = "ebs.group2";
            string groupKey1 = "TestGroups1";   //    <add key="TestGroups1" value="ebs.group1,ebs.group2"/>
            string groupKey2 = "TestGroups2";   //    <add key="TestGroups1" value="us\ebs.group1,us\ebs.group2"/>

            // Mock Employee1 as not a member of either group
            adUtils.Setup(x => x.IsMemberOfADGroup(emp1, group1)).Returns(false);
            adUtils.Setup(x => x.IsMemberOfADGroup(emp1, group2)).Returns(false);
            // Mock Employee2 as a member of group2 only
            adUtils.Setup(x => x.IsMemberOfADGroup(emp2, group1)).Returns(false);
            adUtils.Setup(x => x.IsMemberOfADGroup(emp2, group2)).Returns(true);

            // first test to make sure things return false when they have no membership
            bool returned = sut.IsMemberOfADGroupInAppSettingsList(emp1, groupKey1);    // test group names without domains
            Assert.IsFalse(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(emp1, group1), Times.Exactly(1));
            adUtils.Verify(x => x.IsMemberOfADGroup(emp1, group2), Times.Exactly(1));
            returned = sut.IsMemberOfADGroupInAppSettingsList(emp1, groupKey2);         // test group names with domains
            Assert.IsFalse(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(emp1, group1), Times.Exactly(2));
            adUtils.Verify(x => x.IsMemberOfADGroup(emp1, group2), Times.Exactly(2));

            memCache.ClearCache();

            // now test that if we're in the group role we're good
            returned = sut.IsMemberOfADGroupInAppSettingsList(emp2, groupKey1);         // test group names without domains
            Assert.IsTrue(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(emp2, group1), Times.Exactly(1));
            adUtils.Verify(x => x.IsMemberOfADGroup(emp2, group2), Times.Exactly(1));
            returned = sut.IsMemberOfADGroupInAppSettingsList(emp2, groupKey2);         // test group names with domains
            Assert.IsTrue(returned);
            adUtils.Verify(x => x.IsMemberOfADGroup(emp2, group1), Times.Exactly(2));
            adUtils.Verify(x => x.IsMemberOfADGroup(emp2, group2), Times.Exactly(2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsMemberOfADGroupInAppSettingsList_ExceptionTest2()
        {
            var adUtils = new Mock<IActiveDirectoryUtilities>();
            var memCache = new Mock<MemoryCache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);
            sut.IsMemberOfADGroupInAppSettingsList(string.Empty, "RDMAdminGroups");   // empty user name
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsMemberOfADGroupInAppSettingsList_ExceptionTest3()
        {
            var adUtils = new Mock<IActiveDirectoryUtilities>();
            var memCache = new Mock<MemoryCache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);
            sut.IsMemberOfADGroupInAppSettingsList("employee", "");   // empty app config key
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsMemberOfADGroupInAppSettingsList_ExceptionTest4()
        {
            var adUtils = new Mock<IActiveDirectoryUtilities>();
            var memCache = new Mock<MemoryCache>();
            SecurityInformation sut = new SecurityInformation(adUtils.Object, memCache.Object);
            sut.IsMemberOfADGroupInAppSettingsList("employee", "missingADGroupList");   // empty or missing AD group list for app config key
        }
    }
}
