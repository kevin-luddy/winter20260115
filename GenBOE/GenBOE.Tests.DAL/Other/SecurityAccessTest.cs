using System;
using System.Collections.ObjectModel;
using System.Linq;
using IES.Common;
using GenBOE.DataBridge.Common;
using GenBOE.DataBridge.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using GenBOE.Dtos;

namespace GenBOE.Tests.DAL.Other
{
    [TestClass]
    public class SecurityAccessTest
    {
        private Collection<SecurityPermissionsResponse> authorizedPermissions;

        /// <summary>
        /// Run a test case based on the parameters passed in
        /// </summary>
        /// <param name="inAuthorizedRoles">The authorized roles to test the users permissions for</param>
        /// <param name="inRequestedPages">The requested pages to test the permissions</param>
        /// <param name="inWorkspaceStates">The workspace states to test the permissions</param>
        /// <param name="inBoeStates">The BOE states to test permissions in</param>
        /// <param name="inWorkspaceID">The ID of the workspace</param>
        /// <param name="inBOEID">The ID of the BOE</param>
        /// <param name="inExpectedAuthorization">The expected authorization based on the parameters</param>
        private void _TestExecution(Role[] inAuthorizedRoles,
                                    SecurityPage[] inRequestedPages,
                                    WorkspaceState[] inWorkspaceStates,
                                    BOEState[] inBoeStates,
                                    int? inWorkspaceID,
                                    int? inBOEID,
                                    SecurityAuthorization inExpectedAuthorization)
        {
            int maxBOEState = inBoeStates.Cast<int>().Max();
            int maxWorkspaceState = inWorkspaceStates.Cast<int>().Max();
            WorkspaceDTO ws = null;
            if(inWorkspaceID.HasValue)
            {
                ws = new WorkspaceDTO() { Id = inWorkspaceID.Value, WorkspaceState = (WorkspaceState)maxWorkspaceState };
            }

            // test for selected pages
            foreach (SecurityPage page in inRequestedPages)
            {
                SecurityPermissionsRequested requestedPermissions = null;
                SecurityAccess sut = this._TestSetup(page, // securitypage
                                                        inBOEID, // requested boeid
                                                        inWorkspaceID, // requested workspaceid
                                                        inAuthorizedRoles, // authorized roles
                                                        inBOEID, // authorized BOEid
                                                        inWorkspaceID, // authorized workspace id
                                                        (BOEState)maxBOEState, // authorized boestate (pick max state for proper init)
                                                        (WorkspaceState)maxWorkspaceState, // authorized workspace state (pick max state for proper init)
                                                        out requestedPermissions); // permissions object itself

                SecurityAuthorization authorization = sut.IsAuthorized(requestedPermissions, ws, authorizedPermissions);

                Assert.AreEqual(inExpectedAuthorization, authorization);
            }

        }
            
        
        /// <summary>
        /// Setup the permissions requested object for test usage
        /// </summary>
        /// <param name="inPageRequested">page enum being requested</param>
        /// <param name="inRequestedBOEID">boeid being requested</param>
        /// <param name="inRequestedWorkspaceID">workspaceid being requested</param>
        /// <param name="inAuthorizedRoles">authorized roles to return to caller from security call</param>
        /// <param name="inAuthorizedBOEID">authorized boeid to return to caller from security call</param>
        /// <param name="inAuthorizedBOEState">The state of the BOE the user is authorized for in the given role</param>
        /// <param name="inAuthorizedWorkspaceState">The state of the workspace the user is authorized for in the given role</param>
        /// <param name="inAuthorizedWorkspaceID">authorized workspaceid to return to caller from security call</param>
        /// <param name="outRequestedPermissions">the requested permission object to be passed to "IsAuthorized"</param>
        /// <returns></returns>
        private SecurityAccess _TestSetup(SecurityPage inPageRequested,
                                       int? inRequestedBOEID,
                                       int? inRequestedWorkspaceID,
                                       Role[] inAuthorizedRoles,
                                       int? inAuthorizedBOEID,
                                       int? inAuthorizedWorkspaceID,
                                       BOEState inAuthorizedBOEState,
                                       WorkspaceState inAuthorizedWorkspaceState,
                                       out SecurityPermissionsRequested outRequestedPermissions)
        {
            Mock<IWorkspaceDTODataLoader> workspaceLoader = new Mock<IWorkspaceDTODataLoader>();

            if (inRequestedWorkspaceID.HasValue)
            {
                WorkspaceDTO returnedWorkspace = new WorkspaceDTO();
                returnedWorkspace.WorkspaceState = inAuthorizedWorkspaceState;
                workspaceLoader.Setup(x => x.GetById(inRequestedWorkspaceID.Value)).Returns(returnedWorkspace);
            }

            Mock<IBoeDTODataLoader> boeLoader = new Mock<IBoeDTODataLoader>();
            if (inRequestedBOEID.HasValue)
            {
                boeLoader.Setup(x => x.GetBoeState(inRequestedBOEID.Value)).Returns(inAuthorizedBOEState);
            }

            #region Permissions Requested Setup

            // build a request object
            SecurityPermissionsRequested requestedPermission = new SecurityPermissionsRequested();
            requestedPermission.PageToCheck = inPageRequested;
            requestedPermission.BOEId = inRequestedBOEID;
            requestedPermission.WorkspaceId = inRequestedWorkspaceID;

            #endregion

            // assign the request object to the out parm so the caller now has it
            outRequestedPermissions = requestedPermission;

            #region Authorized Permissions Setup

            // build the response we will use in the mock object for what the user is actually authorized for
            authorizedPermissions = new Collection<SecurityPermissionsResponse>();

            foreach (Role role in inAuthorizedRoles)
            {
                SecurityPermissionsResponse authorizedPermission = new SecurityPermissionsResponse ( role, inAuthorizedWorkspaceID, inAuthorizedBOEID );
                authorizedPermissions.Add(authorizedPermission);
            }

            #endregion

            SecurityAccess sut = new SecurityAccess(boeLoader.Object);

            return sut;
        }

        [TestMethod]
        public void IsAuthorizedHomePage_ReadUsers()
        {
            Role[] roles = new Role[] { Role.None, 
                Role.WorkspaceUser,
                Role.Author,
                Role.SubcontractorAuthor,
                Role.WorkspaceAuthor,
                Role.WorkspaceReviewer,
                Role.Approver,
                Role.WorkspaceAdmin,
                Role.SystemAdmin};

            _TestExecution(roles, // roles
                           new SecurityPage[] { SecurityPage.Home }, // pages
                           (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState)), // workspace states
                           (BOEState[])Enum.GetValues(typeof(BOEState)), // boe states
                           null, // boeid
                           null, // workspace id
                           SecurityAuthorization.CreateReadUpdateDelete); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedHomePage_NoAccessUsers()
        {
            Role[] roles = new Role[] { Role.MetricsAdmin };

            _TestExecution(roles, // roles
                           new SecurityPage[] { SecurityPage.Home }, // pages
                           (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState)), // workspace states
                           (BOEState[])Enum.GetValues(typeof(BOEState)), // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }
       
        [TestMethod]
        public void IsAuthorizedManageClinManageWBSManageBOEs_InitializedWorking_CRUD()
        {
            Role[] roles = new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.ManageCLINs, SecurityPage.ManageWBS, SecurityPage.ManageBOEs };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           (BOEState[])Enum.GetValues(typeof(BOEState)), // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.CreateReadUpdateDelete); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedManageClinManageWBSManageBOEs_InitializedWorking_None()
        {
            Role[] roles = new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor, Role.WorkspaceReviewer,
                                                        Role.Approver, Role.MetricsAdmin, Role.SubcontractAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.ManageCLINs, SecurityPage.ManageWBS, SecurityPage.ManageBOEs };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           (BOEState[])Enum.GetValues(typeof(BOEState)), // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedManageClinManageWBSManageBOEs_LockedReviewCompleteClosed_Read()
        {
            Role[] roles = new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Locked, 
                                                WorkspaceState.Complete, WorkspaceState.Closed };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.ManageCLINs, SecurityPage.ManageWBS, SecurityPage.ManageBOEs };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           (BOEState[])Enum.GetValues(typeof(BOEState)), // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.Read); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedManageClinManageWBSManageBOEs_LockedReviewCompleteClosed_None()
        {
            Role[] roles = new Role[] { Role.WorkspaceUser, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor, Role.WorkspaceReviewer,
                                                        Role.Approver, Role.MetricsAdmin, Role.SubcontractAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Locked, 
                                                WorkspaceState.Complete, WorkspaceState.Closed };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.ManageCLINs, SecurityPage.ManageWBS, SecurityPage.ManageBOEs };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           (BOEState[])Enum.GetValues(typeof(BOEState)), // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedWorkspaceAdminPermissions_CRUD()
        {
            Role[] roles = new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.WorkspaceAdminPermissions };

            _TestExecution(roles, // roles
                           pages, // pages
                           (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState)), // workspace states
                           (BOEState[])Enum.GetValues(typeof(BOEState)), // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.CreateReadUpdateDelete); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedWorkspaceAdminPermissions_None()
        {
            Role[] roles = new Role[] { Role.WorkspaceUser, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor, Role.WorkspaceReviewer,
                                                        Role.Approver, Role.MetricsAdmin, Role.SubcontractAdmin };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.WorkspaceAdminPermissions };

            _TestExecution(roles, // roles
                           pages, // pages
                           (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState)), // workspace states
                           (BOEState[])Enum.GetValues(typeof(BOEState)), // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedEditBOEs_Initialization_InitialiazationAssignedDeleted_Read()
        {
            Role[] roles = new Role[] { Role.WorkspaceUser, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor, Role.WorkspaceReviewer,
                                                        Role.Approver, Role.WorkspaceAdmin, Role.SystemAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Initialization };

            BOEState[] boeStates =
                new BOEState[] { BOEState.Unassigned, BOEState.Draft };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.EditBOEHeader };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.Read); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedEditBOEs_Initialization_InitialiazationAssignedDeleted_None()
        {
            Role[] roles = new Role[] { Role.MetricsAdmin };

            WorkspaceState[] workspaceStates = new WorkspaceState[] { WorkspaceState.Initialization };

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.Draft };

            SecurityPage[] pages = new SecurityPage[] { SecurityPage.EditBOEHeader };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedEditBOEs_Working_InitializationAwaitingApprovalCompleteDeleted_Read()
        {
            Role[] roles = new Role[] { Role.WorkspaceUser, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor, Role.WorkspaceReviewer,
                                                        Role.Approver, Role.WorkspaceAdmin, Role.SystemAdmin, Role.SubcontractAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Working };

            BOEState[] boeStates =
                new BOEState[] { BOEState.Unassigned, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.EditBOEHeader };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.Read); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedEditBOEs_Working_InitializationAwaitingApprovalCompleteDeleted_None()
        {
            Role[] roles = new Role[] { Role.MetricsAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Working };

            BOEState[] boeStates =
                new BOEState[] { BOEState.Unassigned, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.EditBOEHeader };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedEditBOEs_Working_Assigned_Read()
        {
            Role[] roles = new Role[] { Role.WorkspaceUser, Role.WorkspaceReviewer, Role.Approver, Role.SystemAdmin, Role.SubcontractAdmin };

            WorkspaceState[] workspaceStates = new WorkspaceState[] { WorkspaceState.Working };

            BOEState[] boeStates = new BOEState[] { BOEState.Draft };

            SecurityPage[] pages = new SecurityPage[] { SecurityPage.EditBOEHeader };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.Read); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedEditBOEs_Working_Assigned_ReadUpdate()
        {
            Role[] roles = new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor, Role.WorkspaceAdmin };

            WorkspaceState[] workspaceStates = new WorkspaceState[] { WorkspaceState.Working };

            BOEState[] boeStates = new BOEState[] { BOEState.Draft };

            SecurityPage[] pages = new SecurityPage[] { SecurityPage.EditBOEHeader };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.ReadUpdate); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedEditBOEs_Working_Assigned_None()
        {
            Role[] roles = new Role[] { Role.MetricsAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Working };

            BOEState[] boeStates =
                new BOEState[] { BOEState.Draft };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.EditBOEHeader };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedEditBOEs_LockedReviewCompletedClosed_All_Read()
        {
            Role[] roles = new Role[] { Role.WorkspaceUser, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor, Role.WorkspaceReviewer,
                                                        Role.Approver, Role.WorkspaceAdmin, Role.SystemAdmin, Role.SubcontractAdmin };

            WorkspaceState[] workspaceStates = new WorkspaceState[] { WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed };

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.Draft, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages = new SecurityPage[] { SecurityPage.EditBOEHeader };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.Read); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedEditBOEs_LockedReviewCompletedClosed_All_None()
        {
            Role[] roles = new Role[] { Role.MetricsAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed };

            BOEState[] boeStates = new BOEState[] {BOEState.Unassigned, BOEState.Draft, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.EditBOEHeader };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedTaskElements_InitializionLockedReviewCompletedClosed_All_Read()
        {
            Role[] roles = new Role[] { Role.WorkspaceUser, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor, Role.WorkspaceReviewer,
                                                        Role.Approver, Role.WorkspaceAdmin, Role.SystemAdmin, Role.SubcontractAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed };

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.Draft, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.TaskElements };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.Read); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedTaskElements_InitializionLockedReviewCompletedClosed_All_None()
        {
            Role[] roles = new Role[] { Role.MetricsAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed };

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.Draft, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.TaskElements };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedTaskElements_Working_InitializationAwaitingApprovalCompleteDeleted_Read()
        {
            Role[] roles = new Role[] { Role.WorkspaceUser, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor, Role.WorkspaceReviewer,
                                                        Role.Approver, Role.WorkspaceAdmin, Role.SystemAdmin, Role.SubcontractAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Working };

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.TaskElements };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.Read); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedTaskElements_Working_InitializationAwaitingApprovalCompleteDeleted_None()
        {
            Role[] roles = new Role[] { Role.MetricsAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Working };

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.TaskElements };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedTaskElements_Working_Assigned_R()
        {
            Role[] roles = new Role[] { Role.WorkspaceUser, Role.WorkspaceReviewer,
                                                        Role.Approver, Role.WorkspaceAdmin, Role.SystemAdmin, Role.SubcontractAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Working };

            BOEState[] boeStates = new BOEState[] { BOEState.Draft };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.TaskElements };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.Read); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedTaskElements_Working_Assigned_CRUD()
        {
            Role[] roles = new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor };

            WorkspaceState[] workspaceStates = new WorkspaceState[] { WorkspaceState.Working };

            BOEState[] boeStates = new BOEState[] { BOEState.Draft };

            SecurityPage[] pages = new SecurityPage[] { SecurityPage.TaskElements };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.CreateReadUpdateDelete); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedTaskElements_Working_Assigned_None()
        {
            Role[] roles = new Role[] { Role.MetricsAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Working };

            BOEState[] boeStates = new BOEState[] { BOEState.Draft };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.TaskElements };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedMetricsAdminPage_All_All_None()
        {
            Role[] roles = new Role[] { Role.WorkspaceUser, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor, Role.WorkspaceReviewer,
                                                        Role.Approver, Role.WorkspaceAdmin, Role.SubcontractAdmin };

            WorkspaceState[] workspaceStates = (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState));

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.Draft, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.MetricsAdmin };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedMetricsAdminPage_All_All_CRUD()
        {
            Role[] roles = new Role[] { Role.MetricsAdmin, Role.SystemAdmin };

            WorkspaceState[] workspaceStates = (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState));

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.Draft, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.MetricsAdmin };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.CreateReadUpdateDelete); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedGlobalConfigPage_All_All_None()
        {
            Role[] roles = new Role[] { Role.WorkspaceUser, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor, Role.WorkspaceReviewer,
                                                        Role.Approver, Role.WorkspaceAdmin, Role.MetricsAdmin, Role.SubcontractAdmin };

            WorkspaceState[] workspaceStates = (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState));

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.Draft, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.GlobalConfig };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedGlobalConfigPage_All_All_CRUD()
        {
            Role[] roles = new Role[] { Role.SystemAdmin };

            WorkspaceState[] workspaceStates = (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState));

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.Draft, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.GlobalConfig };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.CreateReadUpdateDelete); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedReports_All_All_None()
        {
            Role[] roles = new Role[] { Role.SubcontractorAuthor };

            WorkspaceState[] workspaceStates = (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState));

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.Draft, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.Reports };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           boeStates, // boe states
                           2, // boeid
                           1, // workspace id
                           SecurityAuthorization.None); // expected authorization
        }

        /// <summary>
        /// Test that authorization for SSRS Reports is Read for System and WS Admin
        /// </summary>
        [TestMethod]
        public void IsAuthorizedSSRSReports_Read()
        {
            Role[] roles = new Role[] { Role.SystemAdmin, Role.WorkspaceAdmin };

            WorkspaceState[] workspaceStates = (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState));

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.Draft, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages = new SecurityPage[] { SecurityPage.SSRSReports };

            this._TestExecution(roles, // roles
                pages, // pages
                workspaceStates, // workspace states
                boeStates, // boe states
                1, // workspace id
                null, // boe id
                SecurityAuthorization.Read); // expected authorization
        }

        /// <summary>
        /// Test that authorization for SSRS Reports is None for any roles that aren't System or WS Admin
        /// </summary>
        [TestMethod]
        public void IsAuthorizedSSRSReports_None()
        {
            Role[] roles = new Role[] { Role.Approver, Role.Author, Role.SubcontractAdmin, Role.SubcontractorAuthor, Role.WorkspaceAuthor,
                Role.WorkspaceReviewer, Role.WorkspaceUser, Role.None };

            WorkspaceState[] workspaceStates = (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState));

            BOEState[] boeStates = new BOEState[] { BOEState.Unassigned, BOEState.Draft, BOEState.AwaitingApproval, BOEState.Approved };

            SecurityPage[] pages = new SecurityPage[] { SecurityPage.SSRSReports };

            this._TestExecution(roles, // roles
                pages, // pages
                workspaceStates, // workspace states
                boeStates, // boe states
                1, // workspace id
                null, // boe id
                SecurityAuthorization.None); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedManageBOEForms_Locked_R()
        {
            Role[] roles = new Role[] { Role.SystemAdmin, Role.SystemAdmin };

            WorkspaceState[] workspaceStates =
                new WorkspaceState[] { WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed };

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.ManageBOEForms };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                          (BOEState[])Enum.GetValues(typeof(BOEState)), // boe states
                           2, // workspace id
                           null, // boe id
                           SecurityAuthorization.Read); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedManageBOEForms_Working_Assigned_CRUD()
        {
            Role[] roles = new Role[] { Role.SystemAdmin, Role.SubcontractAdmin };

            WorkspaceState[] workspaceStates = new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working };

            SecurityPage[] pages = new SecurityPage[] { SecurityPage.ManageBOEForms };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           (BOEState[])Enum.GetValues(typeof(BOEState)), // boe states
                           2, // workspace id
                           null, // boe id
                           SecurityAuthorization.CreateReadUpdateDelete); // expected authorization
        }

        [TestMethod]
        public void IsAuthorizedManageBOEForms_All_All_None()
        {
            Role[] roles = new Role[] { Role.MetricsAdmin, Role.Approver, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAuthor,
                Role.WorkspaceReviewer, Role.MetricsAdmin };

            WorkspaceState[] workspaceStates =
                (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState));

            SecurityPage[] pages =
                new SecurityPage[] { SecurityPage.ManageBOEForms };

            _TestExecution(roles, // roles
                           pages, // pages
                           workspaceStates, // workspace states
                           (BOEState[])Enum.GetValues(typeof(BOEState)), // boe states
                           2, // workspace id
                           null, // boe id
                           SecurityAuthorization.None); // expected authorization
        }
    }

}