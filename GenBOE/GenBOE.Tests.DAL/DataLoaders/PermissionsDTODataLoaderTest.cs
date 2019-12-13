using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IES.Common;
using GenBOE.DataBridge.DTO;
using GenBOE.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.Dtos;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class PermissionsDTODataLoaderTest : MOQLoaderObject
    {
        // tests depend on 1 author, 2 approvers, 0 reviewers
        // 1 sys admin
        // 1 workspace admin
        // 5 users total, 4 workspace users

        IActiveDirectoryUtilities adUtils = new ActiveDirectoryUtilities();

        [TestMethod]
        public void L_GetAllBoePermissions()
        {
            var sut = new PermissionsDTODataLoader(adUtils);
            IEnumerable<PermissionsDTO> results = sut.GetAllBOEPermissions().Where(x => x.BOEId == this.Boe1.Id || x.BOEId == this.Boe2.Id);

            Assert.AreEqual(6, results.Count(), "The number of boe permissions returned did not match the number in the database.");
        }

        [TestMethod]
        public void L_GetAllBoePotential()
        {
            var sut = new PermissionsDTODataLoader(adUtils);
            IEnumerable<PermissionsDTO> results = sut.GetAllBOEPotentialPermissions().Where(x => x.WorkspaceId == this.Workspace.Id);

            // 2 authors, 4 approvers
            Assert.AreEqual(3, results.Count(), "The number of boe potential permissions returned did not match the number in the database.");
        }

        [TestMethod]
        public void L_GetAdminPermissionsTest()
        {
            PermissionsDTODataLoader sut = new PermissionsDTODataLoader(adUtils);
            IEnumerable<PermissionsDTO> perms = sut.GetAdminPermissions().Where(x => x.ETIUserId == this.SysAdmin.UserID && x.Role == Role.SystemAdmin);
            Assert.AreEqual(1, perms.Count());
        }

        [TestMethod]
        public void L_GetCreateWorkspacePermissionsTest()
        {
            PermissionsDTODataLoader sut = new PermissionsDTODataLoader(adUtils);
            IEnumerable<PermissionsDTO> perms = sut.GetCreateWorkspacePermissions().Where(x => x.ETIUserId == this.CreateWorkspacePermissions.UserID && x.Role == Role.CreateWorkspacePermissions);
            Assert.AreEqual(1, perms.Count());
        }

        [TestMethod]
        public void L_GetWorkspacePermissionsTest()
        {
            PermissionsDTODataLoader sut = new PermissionsDTODataLoader(adUtils);
            Collection<PermissionsDTO> permissions = sut.GetWorkspacePermissions(this.Workspace.Id);

            Assert.AreEqual(1, permissions.Where(x => x.Role.Equals(Role.WorkspaceAdmin)).Select(x => x.ETIUserId).Count());

            Assert.AreEqual(3, permissions.Where(x => x.Role.Equals(Role.WorkspaceUser)).Select(x => x.ETIUserId).Count());

            Assert.AreEqual(0, permissions.Where(x => x.Role.Equals(Role.WorkspaceReviewer)).Select(x => x.ETIUserId).Count());
        }

        [TestMethod]
        public void L_GetBOEPermissionsTest()
        {
            PermissionsDTODataLoader sut = new PermissionsDTODataLoader(adUtils);
            Collection<PermissionsDTO> boePermissions = sut.GetBOEPermissions(new List<int>(){this.Boe1.Id});

            // 1 author
            Assert.AreEqual(1, boePermissions.Where(x => x.Role.Equals(Role.Author)).Count());

            // 2 approvers
            Assert.AreEqual(2, boePermissions.Where(x => x.Role.Equals(Role.Approver)).Count());
        }

        [TestMethod]
        public void L_GetAllNtIdsForWorkspacePermissionTest()
        {
            PermissionsDTODataLoader sut = new PermissionsDTODataLoader(adUtils);
            ICollection<string> ntids = sut.GetAllNtIdsForWorkspacePermission(Role.SubcontractAdmin);
            Assert.IsNotNull(ntids);
            Assert.IsTrue(ntids.Any());
        }

        [TestMethod]
        public void L_GetBOEPotentialPermissionsForWorkspaceTest()
        {
            PermissionsDTODataLoader sut = new PermissionsDTODataLoader(adUtils);
            Collection<PermissionsDTO> permissions = sut.GetBOEPotentialPermissionsForWorkspace(this.Workspace.Id);

            // 1 author
            Assert.AreEqual(1, permissions.Where(x => x.Role.Equals(Role.Author)).Select(x => x.ETIUserId).Count());

            // 2 approvers
            Assert.AreEqual(2, permissions.Where(x => x.Role.Equals(Role.Approver)).Select(x => x.ETIUserId).Count());
        }

        [TestMethod]
        public void L_SavePermissionTest()
        {
            PermissionsDTODataLoader sut = new PermissionsDTODataLoader(adUtils);

            Collection<UserDTO> users = new Collection<UserDTO>
            {
                this._CreateNewUserSaved(),
                this._CreateNewUserSaved(),
                this._CreateNewUserSaved(),
                this._CreateNewUserSaved(),
                this._CreateNewUserSaved(),
                this._CreateNewUserSaved(),
                this._CreateNewUserSaved()
            };

            PermissionsDTO author = new PermissionsDTO();
            author.ETIUserId = users[0].UserID;
            author.Role = Role.Author;
            author.WorkspaceId = this.Workspace.Id;
            author.Updateable = UpdateType.Upsert;
            author.UpdateDate = DateTime.Now;

            PermissionsDTO approver = new PermissionsDTO();
            approver.ETIUserId = users[1].UserID;
            approver.Role = Role.Approver;
            approver.WorkspaceId = this.Workspace.Id;
            approver.Updateable = UpdateType.Upsert;
            approver.UpdateDate = DateTime.Now;

            PermissionsDTO workspaceReviewer = new PermissionsDTO();
            workspaceReviewer.ETIUserId = users[2].UserID;
            workspaceReviewer.Role = Role.WorkspaceReviewer;
            workspaceReviewer.WorkspaceId = this.Workspace.Id;
            workspaceReviewer.Updateable = UpdateType.Upsert;
            workspaceReviewer.UpdateDate = DateTime.Now;

            PermissionsDTO workspaceAdmin = new PermissionsDTO();
            workspaceAdmin.ETIUserId = users[3].UserID;
            workspaceAdmin.Role = Role.WorkspaceAdmin;
            workspaceAdmin.WorkspaceId = this.Workspace.Id;
            workspaceAdmin.Updateable = UpdateType.Upsert;
            workspaceAdmin.UpdateDate = DateTime.Now;

            PermissionsDTO systemAdmin = new PermissionsDTO();
            systemAdmin.ETIUserId = users[4].UserID;
            systemAdmin.Role = Role.SystemAdmin;
            systemAdmin.WorkspaceId = this.Workspace.Id;
            systemAdmin.Updateable = UpdateType.Upsert;
            systemAdmin.UpdateDate = DateTime.Now;

            PermissionsDTO metricsAdmin = new PermissionsDTO();
            metricsAdmin.ETIUserId = users[5].UserID;
            metricsAdmin.Role = Role.MetricsAdmin;
            metricsAdmin.WorkspaceId = this.Workspace.Id;
            metricsAdmin.Updateable = UpdateType.Upsert;
            metricsAdmin.UpdateDate = DateTime.Now;

            PermissionsDTO subcontractorAuthor = new PermissionsDTO();
            subcontractorAuthor.ETIUserId = users[6].UserID;
            subcontractorAuthor.Role = Role.SubcontractorAuthor;
            subcontractorAuthor.WorkspaceId = this.Workspace.Id;
            subcontractorAuthor.Updateable = UpdateType.Upsert;
            subcontractorAuthor.UpdateDate = DateTime.Now;


            // Verify that the permissions don't exist yet.
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var permission = from p in gbe.BOEPotentialRoles
                                 where p.ETIUserID == author.ETIUserId ||
                                    p.ETIUserID == approver.ETIUserId
                                 select p;
                Assert.IsNull(permission.FirstOrDefault());

                var userRole = from p in gbe.WorkspaceUserRoles
                               where p.ETIUserID == author.ETIUserId ||
                                    p.ETIUserID == approver.ETIUserId ||
                                    p.ETIUserID == workspaceReviewer.ETIUserId ||
                                    p.ETIUserID == workspaceAdmin.ETIUserId
                               select p;
                Assert.IsNull(userRole.FirstOrDefault());

                var systemRole = from p in gbe.SystemUserRoles
                               where p.ETIUserID == systemAdmin.ETIUserId ||
                                    p.ETIUserID == metricsAdmin.ETIUserId
                               select p;
                Assert.IsNull(systemRole.FirstOrDefault());
            }

            // Call the test
            sut.SavePermission(author);
            sut.SavePermission(approver);
            sut.SavePermission(workspaceReviewer);
            sut.SavePermission(workspaceAdmin);
            sut.SavePermission(systemAdmin);
            sut.SavePermission(metricsAdmin);
            sut.SavePermission(subcontractorAuthor);

            // Assert
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var permission = from p in gbe.BOEPotentialRoles
                                 where p.ETIUserID == author.ETIUserId ||
                                    p.ETIUserID == approver.ETIUserId ||
                                    p.ETIUserID == subcontractorAuthor.ETIUserId
                                 select p;
                Assert.AreEqual(3, permission.Count());

                var userRole = from p in gbe.WorkspaceUserRoles
                               where p.ETIUserID == workspaceReviewer.ETIUserId ||
                                    p.ETIUserID == workspaceAdmin.ETIUserId
                               select p;
                Assert.AreEqual(2, userRole.Count());

                var systemRole = from p in gbe.SystemUserRoles
                                 where p.ETIUserID == systemAdmin.ETIUserId ||
                                    p.ETIUserID == metricsAdmin.ETIUserId
                                 select p;
                Assert.AreEqual(2, systemRole.Count());
            }

            // now delete the permissions
            PermissionsDTO authorD = sut.GetBOEPotentialPermissionsForWorkspace(this.Workspace.Id).Where(x => x.ETIUserId == users[0].UserID).First();
            authorD.Updateable = UpdateType.Deleted;

            PermissionsDTO approverD = sut.GetBOEPotentialPermissionsForWorkspace(this.Workspace.Id).Where(x => x.ETIUserId == users[1].UserID).First();
            approverD.Updateable = UpdateType.Deleted;

            PermissionsDTO reviewerD = sut.GetWorkspacePermissions(this.Workspace.Id).Where(x => x.ETIUserId == users[2].UserID).First();
            reviewerD.Updateable = UpdateType.Deleted;

            PermissionsDTO wsAdminD = sut.GetWorkspacePermissions(this.Workspace.Id).Where(x => x.ETIUserId == users[3].UserID).First();
            wsAdminD.Updateable = UpdateType.Deleted;

            PermissionsDTO sysAdminD = sut.GetAdminPermissions().Where(x => x.ETIUserId == users[4].UserID).First();
            sysAdminD.Updateable = UpdateType.Deleted;

            PermissionsDTO metricsAdminD = sut.GetAdminPermissions().Where(x => x.ETIUserId == users[5].UserID).First();
            metricsAdminD.Updateable = UpdateType.Deleted;

            PermissionsDTO subcontractorAuthorD = sut.GetBOEPotentialPermissionsForWorkspace(this.Workspace.Id).Where(x => x.ETIUserId == users[6].UserID).First();
            subcontractorAuthorD.Updateable = UpdateType.Deleted;


            // test delete
            sut.SavePermission(authorD);
            sut.SavePermission(approverD);
            sut.SavePermission(reviewerD);
            sut.SavePermission(wsAdminD);
            sut.SavePermission(sysAdminD);
            sut.SavePermission(metricsAdminD);
            sut.SavePermission(subcontractorAuthorD);

            // Assert
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var permission = from p in gbe.BOEPotentialRoles
                                 where p.ETIUserID == authorD.ETIUserId ||
                                    p.ETIUserID == approverD.ETIUserId ||
                                    p.ETIUserID == subcontractorAuthor.ETIUserId
                                 select p;
                Assert.AreEqual(0, permission.Count());

                var userRole = from p in gbe.WorkspaceUserRoles
                               where p.ETIUserID == reviewerD.ETIUserId ||
                                    p.ETIUserID == wsAdminD.ETIUserId
                               select p;
                Assert.AreEqual(0, userRole.Count());

                var systemRole = from p in gbe.SystemUserRoles
                                 where p.ETIUserID == sysAdminD.ETIUserId ||
                                    p.ETIUserID == metricsAdminD.ETIUserId
                                 select p;
                Assert.AreEqual(0, systemRole.Count());
            }

            this.ResetTestData();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void L_SaveNullPermission()
        {
            PermissionsDTODataLoader sut = new PermissionsDTODataLoader(adUtils);
            sut.SavePermission(null);
        }

        [TestMethod]
        public void L_SaveWorkspaceHideHelp()
        {
            PermissionsDTODataLoader sut = new PermissionsDTODataLoader(adUtils);

            //save one workspace admin
            Collection<UserDTO> users = new Collection<UserDTO> { this._CreateNewUserSaved() };

            PermissionsDTO workspaceAdmin = new PermissionsDTO();
            workspaceAdmin.ETIUserId = users[0].UserID;
            workspaceAdmin.Role = Role.WorkspaceAdmin;
            workspaceAdmin.WorkspaceId = this.Workspace.Id;
            workspaceAdmin.Updateable = UpdateType.Upsert;
            workspaceAdmin.UpdateDate = DateTime.Now;

            sut.SavePermission(workspaceAdmin);

            // get the permission that we just saved
            PermissionsDTO waPermission = sut.GetWorkspacePermissions(this.Workspace.Id).Where(x => x.ETIUserId == users[0].UserID &&
                 x.WorkspaceId == this.Workspace.Id && x.Role == Role.WorkspaceAdmin).First();

            Assert.IsTrue(waPermission.HideWorkspaceHelp == false, "Hide workspace was not false");

            waPermission.HideWorkspaceHelp = true;
            sut.SaveWorkspaceHideHelp(waPermission);

            waPermission = sut.GetWorkspacePermissions(this.Workspace.Id).Where(x => x.ETIUserId == users[0].UserID &&
                 x.WorkspaceId == this.Workspace.Id && x.Role == Role.WorkspaceAdmin).First();

            Assert.IsTrue(waPermission.HideWorkspaceHelp == true, "save workspace hide help did not work");

            this.ResetTestData();
        }

        /// <summary>
        /// Test GetCreateWorkspaceRolesForPtm
        /// </summary>
        [TestMethod]
        public void L_GetCreateWorkspaceRolesForPtm()
        {
            PermissionsDTODataLoader sut = new PermissionsDTODataLoader(adUtils);

            ICollection<KeyValuePair<string, string>> result = sut.GetCreateWorkspaceRolesForPtm("TestNTID", "TestUser");

            // Assert the System Admin and Create Workspace Permission are included
            Assert.IsNotNull(result.Any(x => x.Key == this.SysAdmin.NTID && x.Value == this.SysAdmin.DisplayName));
            Assert.IsNotNull(result.Any(x => x.Key == this.CreateWorkspacePermissions.NTID && x.Value == this.CreateWorkspacePermissions.DisplayName));
            Assert.IsNotNull(result.Any(x => x.Key == "TestNTID" && x.Value == "TestUser"));

            // Assert the list was alphabetized
            Assert.IsTrue(result.SequenceEqual(result.OrderBy(x => x.Value)));

            // Assert the list is distinct
            Assert.AreEqual(result.Distinct().Count(), result.Count());

            this.ResetTestData();
        }
    }
}
