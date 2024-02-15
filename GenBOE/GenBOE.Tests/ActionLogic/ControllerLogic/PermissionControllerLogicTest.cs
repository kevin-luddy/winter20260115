// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PermissionControllerLogicTest
    {
        Mock<IRetriever> _retriever = new Mock<IRetriever>();
        Mock<IPermissionsDTODataLoader> _perissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
        Mock<ICommonDataMapper> _commonDataMapper = new Mock<ICommonDataMapper>();
        Mock<IFullObjectFactory> Factory = new Mock<IFullObjectFactory>();
        Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();
        Mock<IActiveDirectoryUtilities> _ADUTils = new Mock<IActiveDirectoryUtilities>();
        Mock<IUserDTODataLoader> _UserDTODataLoader = new Mock<IUserDTODataLoader>();
        Mock<MemoryCache> memCache = new Mock<MemoryCache>();
        Mock<SecurityInformation> _SecurityInformation;

        [TestInitialize]
        public void Init()
        {
            //Very important! This happens to allow for me to use the constructor on FullWorkspace
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _perissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);

            //The mock to use the Security INformation Class
            _SecurityInformation = new Mock<SecurityInformation>(_ADUTils.Object, memCache.Object);
        }

        //All Tests associated with SavePotentialPermission in PermissionControllerLogic.cs
        #region SavePotentialPermission Tests

        [TestMethod]
        public void SavePotentialPermissionTest()
        {
            //Arrange
            PermissionsDTO inPermission = new PermissionsDTO() { ETIUserId = 123, Updateable = UpdateType.Deleted };
            UserDTO myUser = new UserDTO() { DisplayName = "John", EmailAddress = "john.test@testing.center", FirstName = "Johnson", LastName = "Doe", NTID = "j123456", PhoneNumber = "555-5555", UpdateDate = new System.DateTime(2014, 2, 12), UserID = 12345 };

            _UserDTODataLoader.Setup(x => x.GetUserByID(inPermission.ETIUserId)).Returns(myUser);

            permissionLoader.Setup(x => x.GetUserPermissions(myUser)).
                Returns(new Collection<PermissionsDTO>{
                    new PermissionsDTO(){ Id = 1, ETIUserId = 1, Updateable = UpdateType.None, BOEId = 1, NTID = "j123456", PermissionId = 1, Role = Role.Approver, WorkspaceId = 1, HideWorkspaceHelp = false, UpdateDate = new DateTime(2014, 2, 12)},
                    new PermissionsDTO(){ Id = 2, ETIUserId = 2, Updateable = UpdateType.None, BOEId = 2, NTID = "j123456", PermissionId = 2, Role = Role.Author, WorkspaceId = 2, HideWorkspaceHelp = false, UpdateDate = new DateTime(2014, 2, 12)}});

            permissionLoader.Setup(x => x.SavePermission(inPermission));

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            //Act
            sut.SavePotentialPermission(inPermission);

            //My Assert since no value is returned
            permissionLoader.Verify(x => x.SavePermission(inPermission), Times.Once());
        }

        [TestMethod]
        public void SavePotentialPermissionTestNull()
        {
            //Arrange
            PermissionsDTO inPermission = new PermissionsDTO() { ETIUserId = 123, Updateable = UpdateType.None };
            UserDTO myUser = new UserDTO() { DisplayName = "John", EmailAddress = "john.test@testing.center", FirstName = "Johnson", LastName = "Doe", NTID = "j123456", PhoneNumber = "555-5555", UpdateDate = new System.DateTime(2014, 2, 12), UserID = 12345 };

            _UserDTODataLoader.Setup(x => x.GetUserByID(inPermission.ETIUserId)).Returns(myUser);

            permissionLoader.Setup(x => x.GetUserPermissions(myUser)).Returns((Collection<PermissionsDTO>)null);

            permissionLoader.Setup(x => x.SavePermission(inPermission));

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            //Act
            sut.SavePotentialPermission(inPermission);

            //My Assert since no value is returned
            permissionLoader.Verify(x => x.SavePermission(inPermission), Times.Once());
        }

        [TestMethod]
        public void SavePotentialPermissionTestDoesNotContains()
        {
            //Arrange
            PermissionsDTO inPermission = new PermissionsDTO() { ETIUserId = 123, Updateable = UpdateType.None };
            UserDTO myUser = new UserDTO() { DisplayName = "John", EmailAddress = "john.test@testing.center", FirstName = "Johnson", LastName = "Doe", NTID = "j123456", PhoneNumber = "555-5555", UpdateDate = new System.DateTime(2014, 2, 12), UserID = 12345 };

            _UserDTODataLoader.Setup(x => x.GetUserByID(inPermission.ETIUserId)).Returns(myUser);

            permissionLoader.Setup(x => x.GetUserPermissions(myUser)).
                Returns(new Collection<PermissionsDTO>{
                    new PermissionsDTO(){ Id = 1, ETIUserId = 1, Updateable = UpdateType.None, BOEId = 1, NTID = "j123456", PermissionId = 1, Role = Role.Approver, WorkspaceId = 1, HideWorkspaceHelp = false, UpdateDate = new DateTime(2014, 2, 12)},
                    new PermissionsDTO(){ Id = 2, ETIUserId = 2, Updateable = UpdateType.None, BOEId = 2, NTID = "j123456", PermissionId = 2, Role = Role.Author, WorkspaceId = 2, HideWorkspaceHelp = false, UpdateDate = new DateTime(2014, 2, 12)}});

            permissionLoader.Setup(x => x.SavePermission(inPermission));

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            //Act
            sut.SavePotentialPermission(inPermission);

            //My Assert since no value is returned
            permissionLoader.Verify(x => x.SavePermission(inPermission), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "inPermission Null was not caught")]
        public void SavePotentialPermissionTestNullException()
        {
            //Pass in null to make sure we throw the Null Excpetion
            PermissionsDTO inPermission = null;

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.SavePotentialPermission(inPermission);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Permission did not already exist")]
        public void SavePotentialPermissionTestException()
        {
            //Arrange
            PermissionsDTO inPermission = new PermissionsDTO() { ETIUserId = 123, Updateable = UpdateType.None };
            UserDTO myUser = new UserDTO() { DisplayName = "John", EmailAddress = "john.test@testing.center", FirstName = "Johnson", LastName = "Doe", NTID = "j123456", PhoneNumber = "555-5555", UpdateDate = new System.DateTime(2014, 2, 12), UserID = 12345 };

            _UserDTODataLoader.Setup(x => x.GetUserByID(inPermission.ETIUserId)).Returns(myUser);

            permissionLoader.Setup(x => x.GetUserPermissions(myUser)).
                Returns(new Collection<PermissionsDTO>{
                    new PermissionsDTO(){ Id = 1, ETIUserId = 1, Updateable = UpdateType.None, BOEId = 1, NTID = "j123456", PermissionId = 1, Role = Role.Approver, WorkspaceId = 1, HideWorkspaceHelp = false, UpdateDate = new DateTime(2014, 2, 12)},
                    new PermissionsDTO(){ Id = 2, ETIUserId = 2, Updateable = UpdateType.None, BOEId = 2, NTID = "j123456", PermissionId = 2, Role = Role.Author, WorkspaceId = 2, HideWorkspaceHelp = false, UpdateDate = new DateTime(2014, 2, 12)},
                    inPermission});

            permissionLoader.Setup(x => x.SavePermission(inPermission));

            //Pass in Mock setups
            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            //Act to throw ArgumentException "Permissions already exist"
            sut.SavePotentialPermission(inPermission);
        }
        #endregion 

        //All Tests associated with SaveNewPermission in PermissionControllerLogic.cs
        #region SaveNewPermission

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void SaveNewPermissionTest()
        {
            //String setup
            string workspace = "MyWorkspace";
            string EIds1 = "j123456"; string EIds2 = "k.123456"; string EIds3 = "j654321";

            //Class Setups
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            Collection<PermissionsDTO> currentWorkspacePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Approver, ETIUserId = 1 },
                 new PermissionsDTO() { Role = IES.Common.Role.SystemAdmin, ETIUserId = 1 }};
            Collection<PermissionsDTO> currentBoePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Author, ETIUserId = 1}};
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.Approver, Role.Author }, EntityIds = new Collection<string>() { EIds1 + " ; " + EIds2 + " ; " + EIds3 } };

            //For user EIds 1
            UserData EIds1Data = new UserData() { LastName = "Doe", FirstName = "John", DisplayName = "John", Email = "john.testing@testing.com", Ntid = EIds1, Phone = "555-5555", IsUsPerson = true, IsSubcontractor = false };
            UserDTO EIdsDTO = new UserDTO() { LastName = EIds1Data.LastName, FirstName = EIds1Data.FirstName, DisplayName = EIds1Data.DisplayName, NTID = EIds1Data.Ntid, PhoneNumber = EIds1Data.Phone, UserID = -1, UpdateDate = DateTime.Now, IsUsPerson = true, IsSubcontractor = false };

            //For user EIds 2
            UserData EIds2Data = new UserData() { LastName = "Marry", FirstName = "Jane", DisplayName = "Jane", Email = "jane.testing@testing.com", Ntid = EIds2, Phone = "444-4444", IsUsPerson = true, IsSubcontractor = false };
            UserDTO EIds2DTO = new UserDTO() { LastName = EIds2Data.LastName, FirstName = EIds2Data.FirstName, DisplayName = EIds2Data.DisplayName, NTID = EIds2Data.Ntid, PhoneNumber = EIds2Data.Phone, UserID = 2, UpdateDate = DateTime.Now, IsUsPerson = true, IsSubcontractor = false };

            //For User EIds 3
            UserData EIds3Data = new UserData() { LastName = "Marry", FirstName = "Jane", DisplayName = "Jane", Email = "jane.testing@testing.com", Ntid = EIds3, Phone = "333-3333", IsUsPerson = true, IsSubcontractor = false };
            UserDTO EIds3DTO = new UserDTO() { LastName = EIds3Data.LastName, FirstName = EIds3Data.FirstName, DisplayName = EIds3Data.DisplayName, NTID = EIds3Data.Ntid, PhoneNumber = EIds3Data.Phone, UserID = 1, UpdateDate = DateTime.Now, IsUsPerson = true, IsSubcontractor = false };

            //Mock Setups
            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(ws);
            permissionLoader.Setup(x => x.GetWorkspacePermissions(ws.Id)).Returns(currentWorkspacePermissions);
            permissionLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(ws.Id)).Returns(currentBoePermissions);

            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(new UserData());
            _ADUTils.Setup(x => x.IsGroup(EIds2)).Returns(true);
            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds3, false)).Returns(new UserData());
            _ADUTils.Setup(x => x.GetAuthorizationGroupsFromWebConfig()).Returns(new Collection<GroupData>());
            _ADUTils.Setup(x => x.CheckUsersBoeAccess(It.IsAny<ICollection<UserData>>(), It.IsAny<ICollection<GroupData>>())).Returns(new Dictionary<UserData, bool>());

            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds1)).Returns(EIdsDTO);
            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds2)).Returns(EIds2DTO);
            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds3)).Returns(EIds3DTO);

            int UserETID;
            _UserDTODataLoader.Setup(x => x.UserExists(EIds1, out UserETID)).Returns(false);
            _UserDTODataLoader.Setup(x => x.UserExists(EIds2, out UserETID)).Returns(true);

            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(EIds1Data);          //Since this is taking else path must be false(hard coded value)
            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds2, true)).Returns(EIds2Data);    //Since this is taking if path must be true(hard coded value)

            _UserDTODataLoader.Setup(x => x.SaveUser(It.IsAny<UserDTO>())).Returns(EIdsDTO); //Have to use It.IsAny since UpdateDate uses DateTime.Now

            _SecurityInformation.Setup(x => x.IsSubcontractorUser(EIds1, EIdsDTO.IsSubcontractor)).Returns(false);
            _SecurityInformation.Setup(x => x.IsSubcontractorUser(EIds2, EIds2DTO.IsSubcontractor)).Returns(false);
            _SecurityInformation.Setup(x => x.IsSubcontractorUser(EIds3, EIds3DTO.IsSubcontractor)).Returns(false);

            permissionLoader.Setup(x => x.SavePermission(new PermissionsDTO { BOEId = null, ETIUserId = EIdsDTO.UserID, Role = Role.Approver, Updateable = UpdateType.Upsert, WorkspaceId = ws.Id }));
            permissionLoader.Setup(x => x.SavePermission(new PermissionsDTO { BOEId = null, ETIUserId = EIdsDTO.UserID, Role = Role.Author, Updateable = UpdateType.Upsert, WorkspaceId = ws.Id }));

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            //Act
            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });

            //Verify since code does not return values
            permissionLoader.Verify(x => x.SavePermission(It.IsAny<PermissionsDTO>()), Times.Exactly(4));
            _ADUTils.Verify(x => x.IsGroup(EIds2), Times.Once());
            _ADUTils.Verify(x => x.GetUserByQualifiedAccount(EIds3, false), Times.Once());
            _ADUTils.Verify(x => x.GetUserByQualifiedAccount(EIds1, false), Times.Once());
            _UserDTODataLoader.Verify(x => x.GetOrCreateUserByNtid(EIds1), Times.Once());
            _UserDTODataLoader.Verify(x => x.GetOrCreateUserByNtid(EIds2), Times.Once());
            _UserDTODataLoader.Verify(x => x.GetOrCreateUserByNtid(EIds3), Times.Once());
            _SecurityInformation.Verify(x => x.IsSubcontractorUser(EIds1, EIdsDTO.IsSubcontractor), Times.Exactly(2));
            _SecurityInformation.Verify(x => x.IsSubcontractorUser(EIds2, EIds2DTO.IsSubcontractor), Times.Exactly(2));
            _SecurityInformation.Verify(x => x.IsSubcontractorUser(EIds3, EIds3DTO.IsSubcontractor), Times.Exactly(2));
        }

        [TestMethod]
        public void SaveNewPermissionTestAddSubcontractor()
        {
            //String setup
            string workspace = "MyWorkspace";
            string EIds1 = "j123456";

            //Class Setups
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            Collection<PermissionsDTO> currentWorkspacePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.None, ETIUserId = 1 }};
            Collection<PermissionsDTO> currentBoePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.None, ETIUserId = 1}};
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.SubcontractorAuthor }, EntityIds = new Collection<string>() { EIds1 } };

            //For user EIds 1
            UserData EIds1Data = new UserData() { LastName = "Doe", FirstName = "John", DisplayName = "John", Email = "john.testing@testing.com", Ntid = EIds1, Phone = "555-5555", IsUsPerson = true, IsSubcontractor = true };
            UserDTO EIdsDTO = new UserDTO() { LastName = EIds1Data.LastName, FirstName = EIds1Data.FirstName, DisplayName = EIds1Data.DisplayName, NTID = EIds1Data.Ntid, PhoneNumber = EIds1Data.Phone, UserID = -1, UpdateDate = DateTime.Now, IsUsPerson = true, IsSubcontractor = true };

            //Mock Setups
            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(ws);

            permissionLoader.Setup(x => x.GetWorkspacePermissions(ws.Id)).Returns(currentWorkspacePermissions);
            permissionLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(ws.Id)).Returns(currentBoePermissions);

            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(new UserData());
            _ADUTils.Setup(x => x.GetAuthorizationGroupsFromWebConfig()).Returns(new Collection<GroupData>());
            _ADUTils.Setup(x => x.CheckUsersBoeAccess(It.IsAny<ICollection<UserData>>(), It.IsAny<ICollection<GroupData>>())).Returns(new Dictionary<UserData, bool>());

            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds1)).Returns(EIdsDTO);

            _SecurityInformation.Setup(x => x.IsSubcontractorUser(EIds1, EIdsDTO.IsSubcontractor)).Returns(true);

            permissionLoader.Setup(x => x.SavePermission(new PermissionsDTO { BOEId = null, ETIUserId = EIdsDTO.UserID, Role = Role.SubcontractorAuthor, Updateable = UpdateType.Upsert, WorkspaceId = ws.Id }));

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });

            permissionLoader.Verify(x => x.SavePermission(It.IsAny<PermissionsDTO>()), Times.Once());
            _ADUTils.Verify(x => x.GetUserByQualifiedAccount(EIds1, false), Times.Once());
            _SecurityInformation.Verify(x => x.IsSubcontractorUser(EIds1, EIdsDTO.IsSubcontractor), Times.Exactly(1));
        }

        [TestMethod]
        [ExpectedException(typeof(GenValidationException), "Subcontractor users are only permitted Subcontractor Author permissions")]
        public void SaveNewPermissionTestSubcontractorException()
        {
            //String setup
            string workspace = "MyWorkspace";
            string EIds1 = "j123456";

            //Class Setups
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            Collection<PermissionsDTO> currentWorkspacePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Approver, ETIUserId = 1 }};
            Collection<PermissionsDTO> currentBoePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.None, ETIUserId = 1}};
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.Author }, EntityIds = new Collection<string>() { EIds1 } };

            //For user EIds 1
            UserData EIds1Data = new UserData() { LastName = "Doe", FirstName = "John", DisplayName = "John", Email = "john.testing@testing.com", Ntid = EIds1, Phone = "555-5555", IsUsPerson = true, IsSubcontractor = true };
            UserDTO EIdsDTO = new UserDTO() { LastName = EIds1Data.LastName, FirstName = EIds1Data.FirstName, DisplayName = EIds1Data.DisplayName, NTID = EIds1Data.Ntid, PhoneNumber = EIds1Data.Phone, UserID = -1, UpdateDate = DateTime.Now, IsUsPerson = true, IsSubcontractor = true };

            //Mock Setups
            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(ws);
            permissionLoader.Setup(x => x.GetWorkspacePermissions(ws.Id)).Returns(currentWorkspacePermissions);
            permissionLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(ws.Id)).Returns(currentBoePermissions);
            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds1)).Returns(EIdsDTO);

            int UserETID;
            _UserDTODataLoader.Setup(x => x.UserExists(EIds1, out UserETID)).Returns(false);
            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(EIds1Data);          //Since this is taking else path must be false(hard coded value)
            _UserDTODataLoader.Setup(x => x.SaveUser(It.IsAny<UserDTO>())).Returns(EIdsDTO); //Have to use It.IsAny since UpdateDate uses DateTime.Now
            _SecurityInformation.Setup(x => x.IsSubcontractorUser(EIds1, EIdsDTO.IsSubcontractor)).Returns(true);
            _ADUTils.Setup(x => x.GetAuthorizationGroupsFromWebConfig()).Returns(new Collection<GroupData>());
            _ADUTils.Setup(x => x.CheckUsersBoeAccess(It.IsAny<ICollection<UserData>>(), It.IsAny<ICollection<GroupData>>())).Returns(new Dictionary<UserData, bool>());

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });
        }

        [TestMethod]
        [ExpectedException(typeof(GenValidationException), "LM Users are not permitted Subcontractor Author permissions")]
        public void SaveNewPermissionTestSubcontractorToLMUserException()
        {
            //String setup
            string workspace = "MyWorkspace";
            string EIds1 = "j123456";

            //Class Setups
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            Collection<PermissionsDTO> currentWorkspacePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Approver, ETIUserId = 1 }};
            Collection<PermissionsDTO> currentBoePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.None, ETIUserId = 1}};
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.SubcontractorAuthor }, EntityIds = new Collection<string>() { EIds1 } };

            //For user EIds 1
            UserData EIds1Data = new UserData() { LastName = "Doe", FirstName = "John", DisplayName = "John", Email = "john.testing@testing.com", Ntid = EIds1, Phone = "555-5555" };
            UserDTO EIdsDTO = new UserDTO() { LastName = EIds1Data.LastName, FirstName = EIds1Data.FirstName, DisplayName = EIds1Data.DisplayName, NTID = EIds1Data.Ntid, PhoneNumber = EIds1Data.Phone, UserID = -1, UpdateDate = DateTime.Now };

            //Mock Setups
            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(ws);
            permissionLoader.Setup(x => x.GetWorkspacePermissions(ws.Id)).Returns(currentWorkspacePermissions);
            permissionLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(ws.Id)).Returns(currentBoePermissions);
            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds1)).Returns(EIdsDTO);

            int UserETID;
            _UserDTODataLoader.Setup(x => x.UserExists(EIds1, out UserETID)).Returns(false);
            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(EIds1Data);          //Since this is taking else path must be false(hard coded value)
            _UserDTODataLoader.Setup(x => x.SaveUser(It.IsAny<UserDTO>())).Returns(EIdsDTO); //Have to use It.IsAny since UpdateDate uses DateTime.Now
            _SecurityInformation.Setup(x => x.IsSubcontractorUser(EIds1, EIdsDTO.IsSubcontractor)).Returns(false);
            _ADUTils.Setup(x => x.GetAuthorizationGroupsFromWebConfig()).Returns(new Collection<GroupData>());
            _ADUTils.Setup(x => x.CheckUsersBoeAccess(It.IsAny<ICollection<UserData>>(), It.IsAny<ICollection<GroupData>>())).Returns(new Dictionary<UserData, bool>());

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });
        }

        [TestMethod]
        public void SaveNewPermissionTestAddSubcontractAdministrator()
        {
            //String setup
            string workspace = "MyWorkspace";
            string EIds1 = "j123456";

            //Class Setups
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            Collection<PermissionsDTO> currentWorkspacePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.None, ETIUserId = 1 }};
            Collection<PermissionsDTO> currentBoePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.None, ETIUserId = 1}};
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.SubcontractAdmin }, EntityIds = new Collection<string>() { EIds1 } };

            //For user EIds 1
            UserData EIds1Data = new UserData() { LastName = "Doe", FirstName = "John", DisplayName = "John", Email = "john.testing@testing.com", Ntid = EIds1, Phone = "555-5555" };
            UserDTO EIdsDTO = new UserDTO() { LastName = EIds1Data.LastName, FirstName = EIds1Data.FirstName, DisplayName = EIds1Data.DisplayName, NTID = EIds1Data.Ntid, PhoneNumber = EIds1Data.Phone, UserID = -1, UpdateDate = DateTime.Now };

            //Mock Setups
            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(ws);

            permissionLoader.Setup(x => x.GetWorkspacePermissions(ws.Id)).Returns(currentWorkspacePermissions);
            permissionLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(ws.Id)).Returns(currentBoePermissions);

            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(new UserData());
            _ADUTils.Setup(x => x.GetAuthorizationGroupsFromWebConfig()).Returns(new Collection<GroupData>());
            _ADUTils.Setup(x => x.CheckUsersBoeAccess(It.IsAny<ICollection<UserData>>(), It.IsAny<ICollection<GroupData>>())).Returns(new Dictionary<UserData, bool>());

            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds1)).Returns(new UserDTO());

            _SecurityInformation.Setup(x => x.IsSubcontractorUser(EIds1, EIdsDTO.IsSubcontractor)).Returns(false);

            permissionLoader.Setup(x => x.SavePermission(new PermissionsDTO { BOEId = null, ETIUserId = EIdsDTO.UserID, Role = Role.SubcontractAdmin, Updateable = UpdateType.Upsert, WorkspaceId = ws.Id }));

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });

            permissionLoader.Verify(x => x.SavePermission(It.IsAny<PermissionsDTO>()), Times.Once());
            _ADUTils.Verify(x => x.GetUserByQualifiedAccount(EIds1, false), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(GenValidationException), "User not found")]
        public void SaveNewPermissionTestDomainNameNull()
        {
            //String setup
            string workspace = "MyWorkspace";
            string EIds1 = "j123456";

            //Class Setups
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            Collection<PermissionsDTO> currentWorkspacePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Approver, ETIUserId = 1 },
                 new PermissionsDTO() { Role = IES.Common.Role.Author, ETIUserId = 1 }};
            Collection<PermissionsDTO> currentBoePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.None, ETIUserId = 1}};
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.Approver, Role.Author }, EntityIds = new Collection<string>() { EIds1 } };

            //Mock Setups
            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(ws);

            permissionLoader.Setup(x => x.GetWorkspacePermissions(ws.Id)).Returns(currentWorkspacePermissions);
            permissionLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(ws.Id)).Returns(currentBoePermissions);

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });
        }

        [TestMethod]
        [ExpectedException(typeof(GenValidationException), "The group k.123456 was not found")]
        public void SaveNewPermissionTestFullDomainNameStringNull()
        {
            //String setup
            string workspace = "MyWorkspace";
            string EIds2 = "k.123456";

            //Class Setups
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            Collection<PermissionsDTO> currentWorkspacePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Approver, ETIUserId = 1 },
                 new PermissionsDTO() { Role = IES.Common.Role.Author, ETIUserId = 1 }};
            Collection<PermissionsDTO> currentBoePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.None, ETIUserId = 1}};
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.Approver, Role.Author }, EntityIds = new Collection<string>() { EIds2 } };

            //Mock Setups
            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(ws);

            permissionLoader.Setup(x => x.GetWorkspacePermissions(ws.Id)).Returns(currentWorkspacePermissions);
            permissionLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(ws.Id)).Returns(currentBoePermissions);

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });
        }

        [TestMethod]
        [ExpectedException(typeof(GenValidationException), "At Least one user is required.")]
        public void SaveNewPermissionTestStringNullException()
        {

            string workspace = "MyWorkspace";
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.Approver, Role.Author }, EntityIds = new Collection<string>() { "" } };

            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(new FullWorkspace() { Id = 1 });

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });
        }

        [TestMethod]
        [ExpectedException(typeof(GenValidationException), "At Least one user is required.")]
        public void SaveNewPermissionTestEntityIdsException()
        {

            string workspace = "MyWorkspace";
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.Approver, Role.Author }, EntityIds = new Collection<string>() };

            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(new FullWorkspace() { Id = 1 });

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });
        }

        [TestMethod]
        [ExpectedException(typeof(GenValidationException), "At Least one level of access is required.")]
        public void SaveNewPermissionTestRolesException()
        {
            string workspace = "MyWorkspace";
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>(), EntityIds = new Collection<string>() { "1", "2" } };

            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(new FullWorkspace() { Id = 1 });

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "inPermission is Null was not caught")]
        public void SaveNewPermissionTestNullException()
        {
            string workspace = "MyWorkspace";
            SavePermissionModelView inPermission = null;

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [TestMethod]
        public void SaveNewPermissionTestDuplicateEntries()
        {
            //String setup
            string workspace = "MyWorkspace";
            string EIds1 = "j123456"; string EIds2 = "k.123456"; string EIds3 = "j654321";

            //Class Setups
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            Collection<PermissionsDTO> currentWorkspacePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Approver, ETIUserId = 1 },
                 new PermissionsDTO() { Role = IES.Common.Role.SystemAdmin, ETIUserId = 1 }};
            Collection<PermissionsDTO> currentBoePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Author, ETIUserId = 1}};
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.Approver, Role.Author }, EntityIds = new Collection<string>() { EIds1 + " ; " + EIds1 + " ; " + EIds2 + " ; " + EIds2 + " ; " + EIds3 } };

            //For user EIds 1
            UserData EIds1Data = new UserData() { LastName = "Doe", FirstName = "John", DisplayName = "John", Email = "john.testing@testing.com", Ntid = EIds1, Phone = "555-5555", IsUsPerson = true, IsSubcontractor = false };
            UserDTO EIdsDTO = new UserDTO() { LastName = EIds1Data.LastName, FirstName = EIds1Data.FirstName, DisplayName = EIds1Data.DisplayName, NTID = EIds1Data.Ntid, PhoneNumber = EIds1Data.Phone, UserID = -1, UpdateDate = DateTime.Now, IsUsPerson = true, IsSubcontractor = false };

            //For user EIds 2
            UserData EIds2Data = new UserData() { LastName = "Marry", FirstName = "Jane", DisplayName = "Jane", Email = "jane.testing@testing.com", Ntid = EIds2, Phone = "444-4444", IsUsPerson = true, IsSubcontractor = false };
            UserDTO EIds2DTO = new UserDTO() { LastName = EIds2Data.LastName, FirstName = EIds2Data.FirstName, DisplayName = EIds2Data.DisplayName, NTID = EIds2Data.Ntid, PhoneNumber = EIds2Data.Phone, UserID = 2, UpdateDate = DateTime.Now, IsUsPerson = true, IsSubcontractor = false };

            //For User EIds 3
            UserData EIds3Data = new UserData() { LastName = "Marry", FirstName = "Jane", DisplayName = "Jane", Email = "jane.testing@testing.com", Ntid = EIds3, Phone = "333-3333", IsUsPerson = true, IsSubcontractor = false };
            UserDTO EIds3DTO = new UserDTO() { LastName = EIds3Data.LastName, FirstName = EIds3Data.FirstName, DisplayName = EIds3Data.DisplayName, NTID = EIds3Data.Ntid, PhoneNumber = EIds3Data.Phone, UserID = 1, UpdateDate = DateTime.Now, IsUsPerson = true, IsSubcontractor = false };

            //Mock Setups
            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(ws);
            permissionLoader.Setup(x => x.GetWorkspacePermissions(ws.Id)).Returns(currentWorkspacePermissions);
            permissionLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(ws.Id)).Returns(currentBoePermissions);

            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(new UserData());
            _ADUTils.Setup(x => x.IsGroup(EIds2)).Returns(true);
            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds3, false)).Returns(new UserData());
            _ADUTils.Setup(x => x.GetAuthorizationGroupsFromWebConfig()).Returns(new Collection<GroupData>());
            _ADUTils.Setup(x => x.CheckUsersBoeAccess(It.IsAny<ICollection<UserData>>(), It.IsAny<ICollection<GroupData>>())).Returns(new Dictionary<UserData, bool>());

            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds1)).Returns(EIdsDTO);
            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds2)).Returns(EIds2DTO);
            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds3)).Returns(EIds3DTO);

            int UserETID;
            _UserDTODataLoader.Setup(x => x.UserExists(EIds1, out UserETID)).Returns(false);
            _UserDTODataLoader.Setup(x => x.UserExists(EIds2, out UserETID)).Returns(true);

            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(EIds1Data);          //Since this is taking else path must be false(hard coded value)
            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds2, true)).Returns(EIds2Data);    //Since this is taking if path must be true(hard coded value)

            _UserDTODataLoader.Setup(x => x.SaveUser(It.IsAny<UserDTO>())).Returns(EIdsDTO); //Have to use It.IsAny since UpdateDate uses DateTime.Now

            _SecurityInformation.Setup(x => x.IsSubcontractorUser(EIds1, EIdsDTO.IsSubcontractor)).Returns(false);
            _SecurityInformation.Setup(x => x.IsSubcontractorUser(EIds2, EIds2DTO.IsSubcontractor)).Returns(false);
            _SecurityInformation.Setup(x => x.IsSubcontractorUser(EIds3, EIds3DTO.IsSubcontractor)).Returns(false);

            permissionLoader.Setup(x => x.SavePermission(new PermissionsDTO { BOEId = null, ETIUserId = EIdsDTO.UserID, Role = Role.Approver, Updateable = UpdateType.Upsert, WorkspaceId = ws.Id }));
            permissionLoader.Setup(x => x.SavePermission(new PermissionsDTO { BOEId = null, ETIUserId = EIdsDTO.UserID, Role = Role.Author, Updateable = UpdateType.Upsert, WorkspaceId = ws.Id }));

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            //Act
            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });

            //Verify since code does not return values
            permissionLoader.Verify(x => x.SavePermission(It.IsAny<PermissionsDTO>()), Times.Exactly(4));
            _ADUTils.Verify(x => x.GetUserByQualifiedAccount(EIds1, false), Times.Once());
            _ADUTils.Verify(x => x.IsGroup(EIds2), Times.Once());
            _UserDTODataLoader.Verify(x => x.GetOrCreateUserByNtid(EIds1), Times.Once());
            _UserDTODataLoader.Verify(x => x.GetOrCreateUserByNtid(EIds2), Times.Once());
            _UserDTODataLoader.Verify(x => x.GetOrCreateUserByNtid(EIds3), Times.Once());
            _SecurityInformation.Verify(x => x.IsSubcontractorUser(EIds1, EIdsDTO.IsSubcontractor), Times.Exactly(2));
            _SecurityInformation.Verify(x => x.IsSubcontractorUser(EIds2, EIds2DTO.IsSubcontractor), Times.Exactly(2));
            _SecurityInformation.Verify(x => x.IsSubcontractorUser(EIds3, EIds3DTO.IsSubcontractor), Times.Exactly(2));
        }

        /// <summary>
        /// Test that an exception is thrown when a user does not have access to GenBOE
        /// </summary>
        [TestMethod, ExpectedException(typeof(GenValidationException))]
        public void SaveNewPermission_NoGenBoeAccess_User()
        {
            //String setup
            string workspace = "MyWorkspace";
            string EIds1 = "j123456";

            //Class Setups
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            Collection<PermissionsDTO> currentWorkspacePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Approver, ETIUserId = 1 },
                 new PermissionsDTO() { Role = IES.Common.Role.SystemAdmin, ETIUserId = 1 }};
            Collection<PermissionsDTO> currentBoePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Author, ETIUserId = 1}};
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.Approver, Role.Author }, EntityIds = new Collection<string>() { EIds1 } };

            //For user EIds 1
            UserData EIds1Data = new UserData() { LastName = "Doe", FirstName = "John", DisplayName = "John", Email = "john.testing@testing.com", Ntid = EIds1, Phone = "555-5555", IsUsPerson = true, IsSubcontractor = false };
            UserDTO EIdsDTO = new UserDTO() { LastName = EIds1Data.LastName, FirstName = EIds1Data.FirstName, DisplayName = EIds1Data.DisplayName, NTID = EIds1Data.Ntid, PhoneNumber = EIds1Data.Phone, UserID = -1, UpdateDate = DateTime.Now, IsUsPerson = true, IsSubcontractor = false };

            //Mock Setups
            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(ws);
            permissionLoader.Setup(x => x.GetWorkspacePermissions(ws.Id)).Returns(currentWorkspacePermissions);
            permissionLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(ws.Id)).Returns(currentBoePermissions);

            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(new UserData());
            _ADUTils.Setup(x => x.GetAuthorizationGroupsFromWebConfig()).Returns(new Collection<GroupData>());
            _ADUTils.Setup(x => x.CheckUsersBoeAccess(It.IsAny<ICollection<UserData>>(), It.IsAny<ICollection<GroupData>>())).Returns(new Dictionary<UserData, bool>() { { new UserData() { Ntid = EIds1 }, false } });

            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds1)).Returns(EIdsDTO);

            int UserETID;
            _UserDTODataLoader.Setup(x => x.UserExists(EIds1, out UserETID)).Returns(false);

            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(EIds1Data);          //Since this is taking else path must be false(hard coded value)

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            //Act
            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });
        }

        /// <summary>
        /// Test that an exception is thrown when a group member does not have access to GenBOE
        /// </summary>
        [TestMethod, ExpectedException(typeof(GenValidationException))]
        public void SaveNewPermission_NoGenBoeAccess_Group()
        {
            //String setup
            string workspace = "MyWorkspace";
            string EIds1 = "j.123456";

            //Class Setups
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            Collection<PermissionsDTO> currentWorkspacePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Approver, ETIUserId = 1 },
                 new PermissionsDTO() { Role = IES.Common.Role.SystemAdmin, ETIUserId = 1 }};
            Collection<PermissionsDTO> currentBoePermissions = new Collection<PermissionsDTO>(){
                 new PermissionsDTO() { Role = IES.Common.Role.Author, ETIUserId = 1}};
            SavePermissionModelView inPermission = new SavePermissionModelView() { Roles = new Collection<Role>() { Role.Approver, Role.Author }, EntityIds = new Collection<string>() { EIds1 } };

            //For user EIds 1
            UserData EIds1Data = new UserData() { LastName = "Doe", FirstName = "John", DisplayName = "John", Email = "john.testing@testing.com", Ntid = EIds1, Phone = "555-5555", IsUsPerson = true, IsSubcontractor = false };
            UserDTO EIdsDTO = new UserDTO() { LastName = EIds1Data.LastName, FirstName = EIds1Data.FirstName, DisplayName = EIds1Data.DisplayName, NTID = EIds1Data.Ntid, PhoneNumber = EIds1Data.Phone, UserID = -1, UpdateDate = DateTime.Now, IsUsPerson = true, IsSubcontractor = false };

            //Mock Setups
            Factory.Setup(x => x.CreateFullWorkspace(workspace, It.IsAny<bool>())).Returns(ws);
            permissionLoader.Setup(x => x.GetWorkspacePermissions(ws.Id)).Returns(currentWorkspacePermissions);
            permissionLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(ws.Id)).Returns(currentBoePermissions);

            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(new UserData());
            _ADUTils.Setup(x => x.GetAuthorizationGroupsFromWebConfig()).Returns(new Collection<GroupData>());
            _ADUTils.Setup(x => x.CheckUsersBoeAccess(It.IsAny<ICollection<UserData>>(), It.IsAny<ICollection<GroupData>>())).Returns(new Dictionary<UserData, bool>() { { new UserData() { Ntid = EIds1 }, false } });
            _ADUTils.Setup(x => x.IsGroup(EIds1)).Returns(true);

            _UserDTODataLoader.Setup(x => x.GetOrCreateUserByNtid(EIds1)).Returns(EIdsDTO);

            int UserETID;
            _UserDTODataLoader.Setup(x => x.UserExists(EIds1, out UserETID)).Returns(false);

            _ADUTils.Setup(x => x.GetUserByQualifiedAccount(EIds1, false)).Returns(EIds1Data);          //Since this is taking else path must be false(hard coded value)

            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            //Act
            sut.SaveNewPermissions(workspace, new SavePermissionModelView[] { inPermission });
        }
        #endregion

        #region Ws Admin Validation

        /// <summary>
        /// Tests ValidateWsAdminMustHaveCreateWsPermission.
        /// 
        /// In this case, this is an invalid assignment.. no roles
        /// </summary>
        [TestMethod, ExpectedException(typeof(GenValidationException))]
        public void ValidateWsAdminMustHaveCreateWsPermission_Invalid1()
        {
            string ntid = "someNtid";
            ICollection<Role> roles = new List<Role>() { Role.WorkspaceAdmin, Role.Author };
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            this.Factory.Setup(x => x.GetPermissionsForUser(ntid)).Returns(new List<SecurityPermissionsResponse>());
            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.ValidateWsAdminMustHaveCreateWsPermission(ntid, roles);
        }

        /// <summary>
        /// Tests ValidateWsAdminMustHaveCreateWsPermission.
        /// 
        /// In this case, this is an invalid assignment - existing ws admin doesn't apply to this WS
        /// </summary>
        [TestMethod, ExpectedException(typeof(GenValidationException))]
        public void ValidateWsAdminMustHaveCreateWsPermission_Invalid2()
        {
            string ntid = "someNtid";
            ICollection<Role> roles = new List<Role>() { Role.WorkspaceAdmin };
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            this.Factory.Setup(x => x.GetPermissionsForUser(ntid)).Returns(new List<SecurityPermissionsResponse>() { new SecurityPermissionsResponse(Role.WorkspaceAdmin, ws.Id + 1, null) });
            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.ValidateWsAdminMustHaveCreateWsPermission(ntid, roles);
        }

        /// <summary>
        /// Tests ValidateWsAdminMustHaveCreateWsPermission.
        /// 
        /// In this case, the result is "valid", because we are not assigning a WS Admin role
        /// </summary>
        [TestMethod]
        public void ValidateWsAdminMustHaveCreateWsPermission_NotAssigningWSAdmin()
        {
            string ntid = "someNtid";
            ICollection<Role> roles = new List<Role>() { Role.Author };
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.ValidateWsAdminMustHaveCreateWsPermission(ntid, roles);
        }

        /// <summary>
        /// Tests ValidateWsAdminMustHaveCreateWsPermission.
        /// 
        /// In this case, the result is valid, because the user already has an admin role (only, Create WS and Sys Admin are false)
        /// </summary>
        [TestMethod]
        public void ValidateWsAdminMustHaveCreateWsPermission_HasSystemAdminOnly()
        {
            string ntid = "someNtid";
            ICollection<Role> roles = new List<Role>() { Role.WorkspaceAdmin };
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            this.Factory.Setup(x => x.GetPermissionsForUser(ntid)).Returns(new List<SecurityPermissionsResponse>() { new SecurityPermissionsResponse(Role.SystemAdmin, null, null) });
            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.ValidateWsAdminMustHaveCreateWsPermission(ntid, roles);
        }

        /// <summary>
        /// Tests ValidateWsAdminMustHaveCreateWsPermission.
        /// 
        /// In this case, the result is valid, because the user has a create WS role (only, already assigned and Sys Admin are false)
        /// </summary>
        [TestMethod]
        public void ValidateWsAdminMustHaveCreateWsPermission_HasCreateWorkspaceOnly()
        {
            string ntid = "someNtid";
            ICollection<Role> roles = new List<Role>() { Role.WorkspaceAdmin };
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            this.Factory.Setup(x => x.GetPermissionsForUser(ntid)).Returns(new List<SecurityPermissionsResponse>() { new SecurityPermissionsResponse(Role.CreateWorkspacePermissions, null, null) });
            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.ValidateWsAdminMustHaveCreateWsPermission(ntid, roles);
        }

        /// <summary>
        /// Tests ValidateWsAdminMustHaveCreateWsPermission.
        /// 
        /// In this case, the result is invalid, because the user does not have a create WS or System Admin role. They have an existing WS Admin role, but it must be removed to make addtional changes to their roles.
        /// </summary>
        [TestMethod, ExpectedException(typeof(GenValidationException))]
        public void ValidateWsAdminMustHaveCreateWsPermission_HasExistingRoleOnly()
        {
            string ntid = "someNtid";
            ICollection<Role> roles = new List<Role>() { Role.WorkspaceAdmin, Role.Author };
            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            this.Factory.Setup(x => x.GetPermissionsForUser(ntid)).Returns(new List<SecurityPermissionsResponse>() { new SecurityPermissionsResponse(Role.WorkspaceAdmin, ws.Id, null) });
            PermissionControllerLogic sut = new PermissionControllerLogic(permissionLoader.Object, _UserDTODataLoader.Object, _ADUTils.Object, _SecurityInformation.Object, Factory.Object);

            sut.ValidateWsAdminMustHaveCreateWsPermission(ntid, roles);
        }

        #endregion
    }
}
