using System;
using System.Collections.ObjectModel;
using GenBOE.Common;
using GenBOE.DataBridge.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using GenBOE.Dtos;

namespace GenBOE.Tests.DAL.DataMappers
{
    [TestClass]
    public class PermissionsDTOMapperTest
    {
        [TestMethod]
        public void M_GetWorkspacePermissionsTest()
        {
            var permDataLoader = new Mock<IPermissionsDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();


            Collection<PermissionsDTO> toReturn = new Collection<PermissionsDTO>() { new PermissionsDTO() };
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetWorkspacePermissionsDelegate>(),
                It.IsAny<object[]>(),
                It.IsAny<string>()))
                .Returns(toReturn);

            PermissionsDTOMapper sut = new PermissionsDTOMapper(permDataLoader.Object, cacheDataLoader.Object, userLoader.Object);

            Collection<PermissionsDTO> returnedValue = sut.GetWorkspacePermissions(5);
            Assert.IsNotNull(returnedValue);
            Assert.IsTrue(returnedValue.Count > 0);
        }

        [TestMethod]
        public void M_GetWorkspacePermissionsByRoleAndUserIDTest()
        {
            var permDataLoader = new Mock<IPermissionsDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();

            Collection<PermissionsDTO> toReturn = new Collection<PermissionsDTO>() { 
                new PermissionsDTO { BOEId=1, ETIUserId = 1, Role = Role.WorkspaceUser, WorkspaceId = 5 },
                new PermissionsDTO { BOEId=1, ETIUserId = 1, Role = Role.WorkspaceReviewer, WorkspaceId = 5 },
                new PermissionsDTO { BOEId=1, ETIUserId = 2, Role = Role.WorkspaceReviewer, WorkspaceId = 5 },
                new PermissionsDTO { BOEId=1, ETIUserId = 3, Role = Role.WorkspaceUser, WorkspaceId = 5 },
                new PermissionsDTO { BOEId=1, ETIUserId = 1, Role = Role.WorkspaceAdmin, WorkspaceId = 5 }};
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetWorkspacePermissionsDelegate>(),
                It.IsAny<object[]>(),
                It.IsAny<string>()))
                .Returns(toReturn);
           
            
            PermissionsDTOMapper sut = new PermissionsDTOMapper(permDataLoader.Object, cacheDataLoader.Object, userLoader.Object);

            Collection<PermissionsDTO> returnedValue = sut.GetWorkspacePermissionsByRoleAndUserID(5, Role.WorkspaceUser, 1);
            Assert.IsNotNull(returnedValue);
            Assert.IsTrue(returnedValue.Count > 0);

            Collection<PermissionsDTO> returnedValue2 = sut.GetWorkspacePermissionsByRoleAndUserID(5, Role.WorkspaceReviewer, 2);
            Assert.IsTrue(returnedValue2.Count == 1);

            Collection<PermissionsDTO> returnedValue3 = sut.GetWorkspacePermissionsByRoleAndUserID(5, Role.WorkspaceAdmin, 1);
            Assert.IsTrue(returnedValue3.Count == 1);

            Collection<PermissionsDTO> returnedValue4 = sut.GetWorkspacePermissionsByRoleAndUserID(5, Role.Approver, 1);
            Assert.IsFalse(returnedValue4.Count > 1);

        }

        [TestMethod]
        public void M_GetWorkspacePermissionsByRoleTest()
        {
            var permDataLoader = new Mock<IPermissionsDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();

            Collection<PermissionsDTO> toReturn = new Collection<PermissionsDTO>() { 
                new PermissionsDTO { BOEId=1, ETIUserId = 1, Role = Role.WorkspaceUser, WorkspaceId = 5 },
                new PermissionsDTO { BOEId=1, ETIUserId = 1, Role = Role.WorkspaceReviewer, WorkspaceId = 5 },
                new PermissionsDTO { BOEId=1, ETIUserId = 2, Role = Role.WorkspaceReviewer, WorkspaceId = 5 },
                new PermissionsDTO { BOEId=1, ETIUserId = 3, Role = Role.WorkspaceUser, WorkspaceId = 5 },
                new PermissionsDTO { BOEId=1, ETIUserId = 1, Role = Role.WorkspaceAdmin, WorkspaceId = 5 }};
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetWorkspacePermissionsDelegate>(),
                It.IsAny<object[]>(),
                It.IsAny<string>()))
                .Returns(toReturn);


            PermissionsDTOMapper sut = new PermissionsDTOMapper(permDataLoader.Object, cacheDataLoader.Object, userLoader.Object);

            Collection<PermissionsDTO> returnedValue = sut.GetWorkspacePermissionsByRole(5, Role.WorkspaceUser);
            Assert.IsNotNull(returnedValue);
            Assert.IsTrue(returnedValue.Count > 0);

            Collection<PermissionsDTO> returnedValue2 = sut.GetWorkspacePermissionsByRole(5, Role.WorkspaceReviewer);
            Assert.IsTrue(returnedValue2.Count == 2);

            Collection<PermissionsDTO> returnedValue3 = sut.GetWorkspacePermissionsByRole(5, Role.WorkspaceAdmin);
            Assert.IsTrue(returnedValue3.Count == 1);

            Collection<PermissionsDTO> returnedValue4 = sut.GetWorkspacePermissionsByRole(5, Role.Approver);
            Assert.IsFalse(returnedValue4.Count > 1);

        }

        [TestMethod]
        public void M_GetAdminPermissionsTest()
        {
            var permDataLoader = new Mock<IPermissionsDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();

            Collection<PermissionsDTO> toReturn = new Collection<PermissionsDTO>() { new PermissionsDTO() };
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetAdminPermissionsDelegate>(),
                It.IsAny<object[]>(),
                It.IsAny<string>()))
                .Returns(toReturn);

            PermissionsDTOMapper sut = new PermissionsDTOMapper(permDataLoader.Object, cacheDataLoader.Object, userLoader.Object);

            Collection<PermissionsDTO> returnedValue = sut.GetAdminPermissions();
            Assert.IsNotNull(returnedValue);
            Assert.IsTrue(returnedValue.Count > 0);
        }
        
        //[TestMethod]
        //public void M_GetBOEPermissionsTest()
        //{
        //    var permDataLoader = new Mock<IPermissionsDTODataLoader>();
        //    var cacheDataLoader = new Mock<ICacheDataLoader>();
        //    Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();

        //    Collection<PermissionsDTO> toReturn = new Collection<PermissionsDTO>() { new PermissionsDTO() };
        //    cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBOEPermissionsDelegate>(),
        //        It.IsAny<object[]>(),
        //        It.IsAny<string>(), It.IsAny<bool>()))
        //        .Returns(toReturn);

        //    PermissionsDTOMapper sut = new PermissionsDTOMapper(permDataLoader.Object, cacheDataLoader.Object, userLoader.Object);
            
        //    Collection<PermissionsDTO> returnedValue = sut.GetBOEPermissions(new Collection<int>(){4});
        //    Assert.IsNotNull(returnedValue);
        //    Assert.IsTrue(returnedValue.Count > 0);
        //}

        [TestMethod]
        public void M_GetBOEPotentialPermissionsForWorkspaceTest()
        {
            var permDataLoader = new Mock<IPermissionsDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();

            Collection<PermissionsDTO> toReturn = new Collection<PermissionsDTO>() { new PermissionsDTO() };
            cacheDataLoader.Setup(x => x.GetData(It.IsAny<GetBOEPotentialPermissionsForWorkspaceDelegate>(),
                It.IsAny<object[]>(),
                It.IsAny<string>()))
                .Returns(toReturn);

            PermissionsDTOMapper sut = new PermissionsDTOMapper(permDataLoader.Object, cacheDataLoader.Object, userLoader.Object);

            Collection<PermissionsDTO> returnedValue = sut.GetBOEPotentialPermissionsForWorkspace(4);
            Assert.IsNotNull(returnedValue);
            Assert.IsTrue(returnedValue.Count > 0);
        }

        /// <summary>
        /// Save a random number of permisisons and verify the loader method was called for each
        /// </summary>
        [TestMethod]
        public void M_SavePermissionsTest()
        {
            var permDataLoader = new Mock<IPermissionsDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();

            // create collection
            Collection<PermissionsDTO> toSave = new Collection<PermissionsDTO>();
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();

            int numPermissions = MOQObject.randomNumberGenerator.Next(20);
            for (int i = 1; i < numPermissions; i++)
            {
                PermissionsDTO dto = new PermissionsDTO();
                dto.ETIUserId = i;
                toSave.Add(dto);

                userLoader.Setup(x => x.GetUserByID(dto.ETIUserId)).Returns(new UserDTO() { NTID = "hello" });
            }

            PermissionsDTOMapper sut = new PermissionsDTOMapper(permDataLoader.Object, cacheDataLoader.Object, userLoader.Object);

            sut.SavePermissions(toSave);

            foreach (PermissionsDTO dto in toSave)
            {
                permDataLoader.Verify(x => x.SavePermission(dto), Times.Once());
            }
        }

        [TestMethod]
        public void M_SaveMetricAdminPermissionsTest()
        {
            var permDataLoader = new Mock<IPermissionsDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();

            int numPermissions = MOQObject.randomNumberGenerator.Next(20);

            // create collection
            MetricAdminPermissionsDTO toSave = new MetricAdminPermissionsDTO(){ 
                ETIUserId = numPermissions,
                GroupId = 1
            };
          
            PermissionsDTOMapper sut = new PermissionsDTOMapper(permDataLoader.Object, cacheDataLoader.Object, userLoader.Object);

            sut.SaveMetricAdminPermission(toSave);

            permDataLoader.Verify(x => x.SaveMetricsPermission(toSave), Times.Once());
            
        }

        /// <summary>
        /// Verify exception on null permissions collection
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void M_SaveNullPermissionsTest()
        {
            var permDataLoader = new Mock<IPermissionsDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();

            PermissionsDTOMapper sut = new PermissionsDTOMapper(permDataLoader.Object, cacheDataLoader.Object, userLoader.Object);

            sut.SavePermissions(null);
        }

        /// <summary>
        /// Verify exception on null permission
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void M_SaveNullPermissionTest()
        {
            var permDataLoader = new Mock<IPermissionsDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();

            PermissionsDTOMapper sut = new PermissionsDTOMapper(permDataLoader.Object, cacheDataLoader.Object, userLoader.Object);

            sut.SavePotentialPermission(null);
        }

        [TestMethod]
        public void M_WarmCachePermissions()
        {
            var _PermissionsLoader = new Mock<IPermissionsDTODataLoader>();
            var cacheProxy = new Mock<ICache>();
            var cacheDataLoader = new Mock<CacheDataLoader>(cacheProxy.Object, -1, new GenBOEUtilities());
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();

            Collection<PermissionsDTO> Permissions1 = new Collection<PermissionsDTO> { new PermissionsDTO { PermissionId = 45 } };
            Collection<MetricAdminPermissionsDTO> Permissions2 = new Collection<MetricAdminPermissionsDTO> { new MetricAdminPermissionsDTO { PermissionId = 45, GroupId = 55 },
                                                                                                             new MetricAdminPermissionsDTO { PermissionId = 46, GroupId = 2 },
                                                                                                             new MetricAdminPermissionsDTO { PermissionId = 47, GroupId = 2 },
                                                                                                             new MetricAdminPermissionsDTO { PermissionId = 48, GroupId = 4 } };
            Collection<PermissionsDTO> Permissions3 = new Collection<PermissionsDTO> { new PermissionsDTO { PermissionId = 49, WorkspaceId = 1 },
                                                                                       new PermissionsDTO { PermissionId = 50, WorkspaceId = 2 },
                                                                                       new PermissionsDTO { PermissionId = 51, WorkspaceId = 3 },
                                                                                       new PermissionsDTO { PermissionId = 52, WorkspaceId = 3 },
                                                                                       new PermissionsDTO { PermissionId = 53, WorkspaceId = 4 } };
            Collection<PermissionsDTO> Permissions4 = new Collection<PermissionsDTO> { new PermissionsDTO { PermissionId = 54, BOEId = 15 },
                                                                                       new PermissionsDTO { PermissionId = 55, BOEId = 40 },
                                                                                       new PermissionsDTO { PermissionId = 56, BOEId = 30 },
                                                                                       new PermissionsDTO { PermissionId = 57, BOEId = 30 },
                                                                                       new PermissionsDTO { PermissionId = 58, BOEId = 40 } };
            Collection<PermissionsDTO> Permissions5 = new Collection<PermissionsDTO> { new PermissionsDTO { PermissionId = 59, WorkspaceId = 1 },
                                                                                       new PermissionsDTO { PermissionId = 60, WorkspaceId = 2 },
                                                                                       new PermissionsDTO { PermissionId = 61, WorkspaceId = 3 },
                                                                                       new PermissionsDTO { PermissionId = 62, WorkspaceId = 3 },
                                                                                       new PermissionsDTO { PermissionId = 63, WorkspaceId = 7 },
                                                                                       new PermissionsDTO { PermissionId = 64, WorkspaceId = 4 } };
            _PermissionsLoader.Setup(x => x.GetAdminPermissions()).Returns(Permissions1);
            _PermissionsLoader.Setup(x => x.GetAllMetricAdminPermissions()).Returns(Permissions2);
            _PermissionsLoader.Setup(x => x.GetAllWorkspacePermissions()).Returns(Permissions3);
            _PermissionsLoader.Setup(x => x.GetAllBOEPermissions()).Returns(Permissions4);
            _PermissionsLoader.Setup(x => x.GetAllBOEPotentialPermissions()).Returns(Permissions5);

            var sut = new CacheWarmingPermissionsDataMapper(_PermissionsLoader.Object, cacheDataLoader.Object, cacheProxy.Object, userLoader.Object);
            sut.WarmPermissionsCache();

            //verify the item was added to cache
            cacheProxy.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<Collection<MetricAdminPermissionsDTO>>(), -1), Times.Exactly(3));
            
            // 1 = Admin, 4 = Workspace, 3 = BOE, 5 = BOE Potential
            int times = 1 + 4 + 3 + 5;
            cacheProxy.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<Collection<PermissionsDTO>>(), -1), Times.Exactly(times));
        }

        [TestMethod]
        public void M_SaveWorkspaceHideHelp()
        {
            var permDataLoader = new Mock<IPermissionsDTODataLoader>();
            var cacheDataLoader = new Mock<ICacheDataLoader>();
            Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();

            PermissionsDTO toSave = new PermissionsDTO()
            {
                PermissionId = 1,
                ETIUserId = 1,
                HideWorkspaceHelp = true
            };

            userLoader.Setup(x => x.GetUserByID(toSave.ETIUserId)).Returns(new UserDTO() { NTID = "hello" });

            PermissionsDTOMapper sut = new PermissionsDTOMapper(permDataLoader.Object, cacheDataLoader.Object, userLoader.Object);

            sut.SaveWorkspaceHideHelp(toSave);

            permDataLoader.Verify(x => x.SaveWorkspaceHideHelp(toSave), Times.Once());

        }
    }
}
