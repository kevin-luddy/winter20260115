// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.Tests.ActionLogic.Permissions
{
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Permissions;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PermissionsWorkspaceAdminValidationTest
    {       

        [TestMethod]
        public void ValidateWorkspaceAdminTest()
        {

			Mock<IPermissionsDTODataLoader> _permissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();
            _permissionsDTOLoader.Setup(x => x.GetWorkspacePermissions(5)).
                Returns(new Collection<PermissionsDTO>{ 
                            new PermissionsDTO { ETIUserId = 3 , NTID = "testuser1" , Role = Role.WorkspaceAdmin, WorkspaceId = 5 },
                            new PermissionsDTO { ETIUserId = 4 , NTID = "testuser2" , Role = Role.WorkspaceAdmin, WorkspaceId = 5 },
                            new PermissionsDTO { ETIUserId = 5 , NTID = "test.group" , Role = Role.WorkspaceAdmin, WorkspaceId = 5 }});

            PermissionsWorkspaceAdminValidation sut = new PermissionsWorkspaceAdminValidation(_permissionsDTOLoader.Object);

            Assert.IsTrue(sut.ValidateWorkspaceAdmin(5));
        }

        [TestMethod]
        public void ValidateWorkspaceAdminTest_Fail()
        {

			Mock<IPermissionsDTODataLoader> _permissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();
            _permissionsDTOLoader.Setup(x => x.GetWorkspacePermissions(5)).
                Returns(new Collection<PermissionsDTO>{ 
                            new PermissionsDTO { ETIUserId = 4 , NTID = "test.group1" , Role = Role.WorkspaceAdmin, WorkspaceId = 5 }});

            PermissionsWorkspaceAdminValidation sut = new PermissionsWorkspaceAdminValidation(_permissionsDTOLoader.Object);

            Assert.IsFalse(sut.ValidateWorkspaceAdmin(5));
        }

        [TestMethod]
        public void ValidateWorkspaceAdminTest_OneGroup()
        {

			Mock<IPermissionsDTODataLoader> _permissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();
            _permissionsDTOLoader.Setup(x => x.GetWorkspacePermissions(5)).
                Returns(new Collection<PermissionsDTO>{ 
                            new PermissionsDTO { ETIUserId = 4 , NTID="Test.Group", Role = Role.WorkspaceAdmin, WorkspaceId = 5 }});

            PermissionsWorkspaceAdminValidation sut = new PermissionsWorkspaceAdminValidation(_permissionsDTOLoader.Object);

            Assert.IsFalse(sut.ValidateWorkspaceAdmin(5));
        }

        [TestMethod]
        public void ValidateWorkspaceAdminTest_NullGroup()
        {

			Mock<IPermissionsDTODataLoader> _permissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();
            _permissionsDTOLoader.Setup(x => x.GetWorkspacePermissions(5)).
                Returns(new Collection<PermissionsDTO>{ 
                            new PermissionsDTO { ETIUserId = 1, NTID = "testuser1" , Role = Role.WorkspaceAdmin, WorkspaceId = 5 },
                            new PermissionsDTO { ETIUserId = 2, NTID = "testuser2" , Role = Role.WorkspaceAdmin, WorkspaceId = 5 },
                            new PermissionsDTO { ETIUserId = 3, NTID = "testuser3" , Role = Role.WorkspaceAdmin, WorkspaceId = 5 }});

            PermissionsWorkspaceAdminValidation sut = new PermissionsWorkspaceAdminValidation(_permissionsDTOLoader.Object);

            Assert.IsTrue(sut.ValidateWorkspaceAdmin(5));
        }

        [TestMethod]
        public void ValidateWorkspaceAdminTest_MultipleGroups()
        {

			Mock<IPermissionsDTODataLoader> _permissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();
            _permissionsDTOLoader.Setup(x => x.GetWorkspacePermissions(5)).
                Returns(new Collection<PermissionsDTO>{ 
                            new PermissionsDTO { ETIUserId = 1 , NTID="Test.Group1", Role = Role.WorkspaceAdmin, WorkspaceId = 5 },
                            new PermissionsDTO { ETIUserId = 2 , NTID="Test.Group2", Role = Role.WorkspaceAdmin, WorkspaceId = 5 },
                            new PermissionsDTO { ETIUserId = 3 , NTID="Test.Group3", Role = Role.WorkspaceAdmin, WorkspaceId = 5 } });

            PermissionsWorkspaceAdminValidation sut = new PermissionsWorkspaceAdminValidation(_permissionsDTOLoader.Object);

            Assert.IsTrue(sut.ValidateWorkspaceAdmin(5));
        }
    }
}
