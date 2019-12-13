using System;
using GenBOE.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using GenBOE.Common.Exceptions;
using System.Collections.Generic;
using GenBOE.DataBridge.DTO;
using Moq;
using GenBOE.Common.Permissions;
using System.Collections.ObjectModel;
using GenBOE.Dtos;

namespace GenBOE.Tests.Common
{
    [TestClass]
    public class GroupPermissionUtilitiesTest
    {
        
        private Mock<UserDTODataMapper> userMapper = new Mock<UserDTODataMapper>();
        private Mock<ActiveDirectoryUtilities> adUtil = new Mock<ActiveDirectoryUtilities>(60);
        private Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();
        private Mock<ICacheDataLoader> cacheLoader = new Mock<ICacheDataLoader>();
        private Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();
        private Mock<PermissionsDTOMapper> permissionMapper = new Mock<PermissionsDTOMapper>();
        private Mock<ISecurityInformation> securityInfo = new Mock<ISecurityInformation>();
        private void Setup()
        {
            permissionMapper = new Mock<PermissionsDTOMapper>(permissionLoader.Object, cacheLoader.Object, userLoader.Object);
            userMapper = new Mock<UserDTODataMapper>(userLoader.Object, cacheLoader.Object, securityInfo.Object, adUtil.Object);
            const string groupName = "group.group1";

            Collection<PermissionsDTO> potentialPermissions = new Collection<PermissionsDTO>();
            potentialPermissions.Add(new PermissionsDTO(){ WorkspaceId = 1, BOEId = 1, NTID = "user1", PermissionId = 1, Role = Role.Author, ETIUserId = 1, Id = 1});
            potentialPermissions.Add(new PermissionsDTO() { WorkspaceId = 1, BOEId = 2, NTID = "group.group1", PermissionId = 4, Role = Role.Author, ETIUserId = 3, Id = 4 });
            potentialPermissions.Add(new PermissionsDTO(){ WorkspaceId = 1, BOEId = 1, NTID = "user2", PermissionId = 2, Role = Role.Approver, ETIUserId = 2, Id = 2});
            
            permissionMapper.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(potentialPermissions);
            
            UserDTO user1 = new UserDTO() { DisplayName = "user1", NTID = "user1", UserID = 1, NTDomain = "domain" };
            UserDTO user2 = new UserDTO() { DisplayName = "user2", NTID = "user2", UserID = 2, NTDomain = "domain" };
            UserDTO user3 = new UserDTO() { DisplayName = "group.group1", NTID = "group1", UserID = 3, NTDomain = "domain" };

            UserData member1Data = new UserData() { DisplayName = "member1", Ntid = "member1", NtDomain = "domain" };
            UserData member2Data = new UserData() { DisplayName = "member2", Ntid = "member2", NtDomain = "domain" };
            UserDTO member1 = new UserDTO() { DisplayName = "member1", NTID = "member1", UserID = 10, NTDomain = "domain" };
            UserDTO member2 = new UserDTO() { DisplayName = "member2", NTID = "member2", UserID = 11, NTDomain = "domain" };

            userMapper.Setup(x => x.GetUserByID(1)).Returns(user1);
            userMapper.Setup(x => x.GetUserByID(2)).Returns(user2);
            userMapper.Setup(x => x.GetUserByID(3)).Returns(user3);
            userMapper.Setup(x => x.GetUserByUserData(member1Data)).Returns(member1);
            userMapper.Setup(x => x.GetUserByUserData(member2Data)).Returns(member2);
            
            var groupDomain = "groupDomain";
            adUtil.Setup(x => x.GetDomainName(UserType.Group, groupName)).Returns(groupDomain);
            List<UserData> groupMembers = new List<UserData>() { member1Data, member2Data };
            adUtil.Setup(x => x.GetAdGroupUsers(groupName, groupDomain)).Returns(groupMembers);      
        }

        [TestMethod]
        public void GetBOEPotentialPermissionsForWorkspaceTest()
        {
            this.Setup();
            GroupPermissionUtilities util = new GroupPermissionUtilities(permissionMapper.Object, userMapper.Object, adUtil.Object);
            Collection<UserDTO> authors = util.GetBOEPotentialPermissionsForWorkspace(1, Role.Author);
            Assert.IsTrue(authors.Any());
            Assert.AreEqual(authors.Count(), 3);
            Collection<UserDTO> approvers = util.GetBOEPotentialPermissionsForWorkspace(1, Role.Approver);
            Assert.IsTrue(approvers.Any());
            Assert.AreEqual(approvers.Count(), 1);
         
        }

    }
}
