// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;

    /// <summary>
    /// Class the requests CRUD Authorizations to a given page for given roles for the currently logged in user
    /// </summary>
    public class SecurityAccess : ISecurityAccess
    {
        private readonly Logger _log = new Logger(typeof(SecurityAccess));

        private readonly IBoeDTODataLoader boeLoader;

        /// <summary>
        /// The Security lookup matrix, structured as follow:
        /// Matrix[SecurityPage][WorkspaceState][BOEState][SecurityAdminRole || Role || SecurityWorkspaceRole]
        /// The 1st index, SecurityPage, is the page the user is looking for inAuthorization information about
        /// The 2nd index, WorkspaceState, is the state of the workspace (Initialization, Working, etc)
        /// The 3rd index, BOEState, is the state of the Boe (Assigned, Awaiting Approval, etc)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
        private int[, , ,] _SecurityMatrix; // not possible to replace with jagged array per CA1814
        private bool _Initialized = false;

        /// <summary>
        /// This property getter wraps the <code>_SecurityMatrix</code> initialization logic (outside the context of the
        /// constructor logic) so that subclasses can override the <code>CustomizeMatrix</code> method.  It should be
        /// used for all post-constructor access to the security matrix.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
        private int[, , ,] TheSecurityMatrix
        {
            get
            {
                if (!_Initialized)
                {
                    Stopwatch sw = new Stopwatch();

                    sw.Start();
                    _log.Debug("Starting to initialize security matrix...");

                    InitializeMatrix();
                    CustomizeMatrix();
                    FinalizeMatrix();

                    sw.Stop();
                    _log.Debug("Finished initializing security matrix.  Took " + sw.ElapsedMilliseconds + " milliseconds.");

                    _Initialized = true;

                    PrintMatrix();
                }

                return _SecurityMatrix;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SecurityAccess(IBoeDTODataLoader boeLoader)
        {
            this.boeLoader = boeLoader;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
        protected void InitializeMatrix()
        {
            #region Allocate storage for the security matrix and initialize default authorization ==> None

            // get the indexes size, go to the database vs. looking at the enum since the 2 should be in sync
            int numOfPages = Convert.ToInt32(Enum.GetValues(typeof(SecurityPage)).Cast<SecurityPage>().Max());
            int numOfBOEState = Convert.ToInt32(Enum.GetValues(typeof(BOEState)).Cast<BOEState>().Max());
            int numOfWSStates = Convert.ToInt32(Enum.GetValues(typeof(WorkspaceState)).Cast<WorkspaceState>().Max());
            int numOfRoles = Convert.ToInt32(Enum.GetValues(typeof(Role)).Cast<Role>().Max());

            // add 1 to each node to allocate the correct number of nodes in the array
            // since it's a 0 index
            _SecurityMatrix = new int[numOfPages + 1, numOfWSStates + 1, numOfBOEState + 1, numOfRoles + 1];

            // initial the matrix with no access for all dictionary combinations
            foreach (SecurityPage page in Enum.GetValues(typeof(SecurityPage)))
            {
                foreach (WorkspaceState workspaceState in Enum.GetValues(typeof(WorkspaceState)))
                {
                    foreach (BOEState boeState in Enum.GetValues(typeof(BOEState)))
                    {
                        foreach (Role role in Enum.GetValues(typeof(Role)))
                        {
                            InitializeMatrix(new SecurityPage[] { page }, new WorkspaceState[] { workspaceState }, new BOEState[] { boeState }, new Role[] { role }, SecurityAuthorization.None);
                        }
                    }
                }
            }

            #endregion

            #region Initialize the base-level security matrix

            // The following initialization code is based on the wireframe matrix located at:
            //      http://unityspaces.global.lmco.com/spaces/Estimating_Init/doclib14/Wireframes/Permissions.mht

            // Including Role.None will allow all users (even those who have no permissions and have never visited the site before) to have 
            // access to that page.

            // Home
            if (!Utilities.IsReadOnly())
            {
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.Home },
					 new Role[] { Role.None, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
					 SecurityAuthorization.CreateReadUpdateDelete);

				// SelectWorkspace
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.SelectWorkspace },
						 new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
						 SecurityAuthorization.Read);

				// Workspace Home
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.WorkspaceHome },
						 new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
						 SecurityAuthorization.Read);

				// Workspace Home
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceHome },
						 new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working },
						 new Role[] { Role.WorkspaceAdmin },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Manage CLINs, Manage WBS, Manage BOEs
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.ManageCLINs, SecurityPage.ManageWBS, SecurityPage.ManageBOEs },
						 new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working },
						 new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
						 SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.ManageCLINs, SecurityPage.ManageWBS, SecurityPage.ManageBOEs },
						 new WorkspaceState[] { WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed },
						 new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
						 SecurityAuthorization.Read);

				InitializeMatrix(new SecurityPage[] { SecurityPage.ManageBOEs },
						 new WorkspaceState[] { WorkspaceState.Locked },
						 new BOEState[] { BOEState.None },  // to allow updates from the Manage BOE summary grid
						 new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
						 SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceRecalculateActuals },
						 new WorkspaceState[] { WorkspaceState.Working },
						 new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Manage INL Forms
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.ManageBOEForms },
						new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked },
						new Role[] { Role.SubcontractAdmin, Role.SystemAdmin, Role.WorkspaceAdmin },
						SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.ManageBOEForms },
						 new WorkspaceState[] { WorkspaceState.Complete, WorkspaceState.Closed },
						 new Role[] { Role.SubcontractAdmin, Role.SystemAdmin, Role.WorkspaceAdmin },
						 SecurityAuthorization.Read);

				// Workspace Admin Permissions
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.WorkspaceAdminPermissions },
						 new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Edit BOE (header - description) [WS Admin has read/write to this field]
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.EditBOEHeaderDescription },
						 new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
						 SecurityAuthorization.Read);

				InitializeMatrix(new SecurityPage[] { SecurityPage.EditBOEHeaderDescription },
						 new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },
						 new BOEState[] { BOEState.Draft },
						 new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAdmin },
						 SecurityAuthorization.ReadUpdate);

				// Edit BOE (header - all besides description) [difference is WSAdmin only has readonly for these fields (WI 3479)]
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.EditBOEHeader },
						new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
						SecurityAuthorization.Read);

				// exclude the About Tools menu option from Subcontractors
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.AboutToolsMenu },
						new Role[] { Role.Author, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
						SecurityAuthorization.Read);

				// exclude the Help menu option from Subcontractors
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.HelpMenu },
						new Role[] { Role.Author, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
						SecurityAuthorization.Read);

				// exclude the Contact menu option from Subcontractors
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.ContactMenu },
						new Role[] { Role.Author, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
						SecurityAuthorization.Read);

				// exclude the generation Home menu option from Subcontractors
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.GenerationHome },
						new Role[] { Role.Author, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
						SecurityAuthorization.Read);

				InitializeMatrix(new SecurityPage[] { SecurityPage.EditBOEHeader },
						 new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },
						 new BOEState[] { BOEState.Draft },
						 new Role[] { Role.Author, Role.SubcontractorAuthor },
						 SecurityAuthorization.ReadUpdate);

				// Task Elements
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.TaskElements, SecurityPage.MOQEquationField, SecurityPage.TaskElementsMissionTask },
						 new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
						 SecurityAuthorization.Read);

				InitializeMatrix(new SecurityPage[] { SecurityPage.TaskElements, SecurityPage.MOQEquationField, SecurityPage.TaskElementsMissionTask },
						 new WorkspaceState[] { WorkspaceState.Working },
						 new BOEState[] { BOEState.Draft },
						 new Role[] { Role.Author, Role.SubcontractorAuthor },
						 SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrix(new SecurityPage[] { SecurityPage.TaskElements },
						 new WorkspaceState[] { WorkspaceState.Locked },
						 new BOEState[] { BOEState.Draft },
						 new Role[] { Role.Author, Role.SubcontractorAuthor },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Boe and Task dates
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BoeTaskDates },
					new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
					SecurityAuthorization.Read);

				InitializeMatrix(new SecurityPage[] { SecurityPage.BoeTaskDates },
						 new WorkspaceState[] { WorkspaceState.Working },
						 new BOEState[] { BOEState.Draft },
						 new Role[] { Role.Author, Role.SubcontractorAuthor },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Travel, Material, and ODC Grids (no subcontractorAuthor access WI 18262)
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BOETravelGrid, SecurityPage.BoeODCGrid, SecurityPage.BoeMaterialsGrid, SecurityPage.BOEZoneTravelGrid },
						 new Role[] { Role.Author, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
						 SecurityAuthorization.Read);

				InitializeMatrix(new SecurityPage[] { SecurityPage.BOETravelGrid, SecurityPage.BoeODCGrid, SecurityPage.BoeMaterialsGrid, SecurityPage.BOEZoneTravelGrid },
						new WorkspaceState[] { WorkspaceState.Working },
						new BOEState[] { BOEState.Draft },
						new Role[] { Role.Author, Role.WorkspaceAdmin },
						SecurityAuthorization.CreateReadUpdateDelete);

				//Labor Grid
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BOELaborGrid },
						new Role[] { Role.Author, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractorAuthor, Role.SubcontractAdmin },
						SecurityAuthorization.Read);

				InitializeMatrix(new SecurityPage[] { SecurityPage.BOELaborGrid },
						new WorkspaceState[] { WorkspaceState.Working },
						new BOEState[] { BOEState.Draft },
						new Role[] { Role.Author, Role.SubcontractorAuthor },
						SecurityAuthorization.CreateReadUpdateDelete);

				// Labor Types
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BoeLaborTypes },
						 new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin, Role.SubcontractAdmin },
						 SecurityAuthorization.Read);

				InitializeMatrix(new SecurityPage[] { SecurityPage.BoeLaborTypes },
						 new WorkspaceState[] { WorkspaceState.Working },
						 new BOEState[] { BOEState.Draft },
						 new Role[] { Role.Author, Role.SubcontractorAuthor },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Materials (no subcontractorAuthor access WI 18262)
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BOEMaterialsTypes },
						 new Role[] { Role.Author, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SystemAdmin },
						 SecurityAuthorization.Read);

				InitializeMatrix(new SecurityPage[] { SecurityPage.BOEMaterialsTypes },
						 new WorkspaceState[] { WorkspaceState.Working },
						 new BOEState[] { BOEState.Draft },
						 new Role[] { Role.Author },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// BOE Approvals
				InitializeMatrix(new SecurityPage[] { SecurityPage.BOEApproval },
						 new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },  // Approvers can still approve/reject when the workspace is Locked
						 new BOEState[] { BOEState.AwaitingApproval },
						 new Role[] { Role.Approver },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// BOE Comments
				InitializeMatrix(new SecurityPage[] { SecurityPage.BOEComment },
						 new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },  // Reviewers can still comment when the workspace is Locked
						 new BOEState[] { BOEState.Draft },
						 new Role[] { Role.WorkspaceReviewer },
						 SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrix(new SecurityPage[] { SecurityPage.BOECommentResponse },
						 new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },  // Authors can still respond to approver or reviewer comments when the workspace is locked
						 new BOEState[] { BOEState.Draft },
						 new Role[] { Role.Author, Role.SubcontractorAuthor },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Edit BOE Buttons
				InitializeMatrix(new SecurityPage[] { SecurityPage.SubmitForReview, SecurityPage.SubmitForApproval },
						 new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },  // Authors can still submit for review/approval when the workspace is locked
						 new BOEState[] { BOEState.Draft, BOEState.DraftLocked },
						 new Role[] { Role.Author, Role.SubcontractorAuthor },
						 SecurityAuthorization.ReadUpdate);

				// BOE Bulk Submit
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.BulkSubmit },
						 new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },  // Authors and Admins can still submit for review/approval when the workspace is locked
						 new Role[] { Role.Author, Role.SubcontractorAuthor },
						 SecurityAuthorization.ReadUpdate);

				// Workspace RTE Templates
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.RTETemplates },
						 new Role[] { Role.SystemAdmin, Role.WorkspaceAdmin },
						 SecurityAuthorization.Read);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.RTETemplates },
					new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Initialization, WorkspaceState.Locked },
						 new Role[] { Role.SystemAdmin, Role.WorkspaceAdmin },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Workspace Delete
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.WorkspaceDelete },
						new Role[] { Role.SystemAdmin, Role.WorkspaceAdmin }, SecurityAuthorization.CreateReadUpdateDelete);

				// Workspace Restore
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.WorkspaceRestore },
						new Role[] { Role.SystemAdmin }, SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrix(new SecurityPage[] { SecurityPage.ValidateBOE },
					   new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },  // Need to be able to validate the BOE as part of Submit-for-Approval
					   new BOEState[] { BOEState.Draft },
					   new Role[] { Role.Author, Role.SubcontractorAuthor, Role.Approver, Role.MetricsAdmin, Role.SystemAdmin, Role.WorkspaceReviewer, Role.WorkspaceAdmin, Role.SubcontractAdmin },
					   SecurityAuthorization.ReadUpdate);

				// Metrics Admin
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.MetricsAdmin },
						 new Role[] { Role.MetricsAdmin, Role.SystemAdmin },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Create Workspace Permissions
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.CreateWorkspacePermissions },
						 new Role[] { Role.CreateWorkspacePermissions, Role.SystemAdmin },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Global Config Pages
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.GlobalConfig },
						 new Role[] { Role.SystemAdmin },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Workspace Resource Page
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceResource },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Workspace Performing Org Page
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspacePerfOrg },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Workspace Settings & Save Workspace Version
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettings, SecurityPage.WorkspaceSettingsStatus, SecurityPage.WorkspaceLaborCost, SecurityPage.BoeCustomFieldResource, SecurityPage.SaveVersion },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettings },
					new WorkspaceState[] { WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.Read);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettingsStatus, SecurityPage.WorkspaceLaborCost, SecurityPage.SaveVersion },
					new WorkspaceState[] { WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// BOEJ-2890 - Allow workspace admin to change output format template while workspace is locked (as well as in Initialization or Working states).
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettingsOutputFormatTemplate },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettingsOutputFormatTemplate },
					new WorkspaceState[] { WorkspaceState.Complete, WorkspaceState.Closed },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.Read);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettingsShareAndAllowSearch },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Save Workspace Version - Author privileges
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.SaveVersion },
					new WorkspaceState[] { WorkspaceState.Working },
					new Role[] { Role.Author },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Reports
				// Reports (no subcontractorAuthor access WI 18262)
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.Reports },
					new Role[] { Role.Approver, Role.Author, Role.SystemAdmin, Role.WorkspaceAdmin, Role.WorkspaceReviewer, Role.WorkspaceUser, Role.SubcontractAdmin },
					SecurityAuthorization.Read);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.Reports },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// RMS SSRS Reports
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.SSRSReports },
					new Role[] { Role.SystemAdmin, Role.WorkspaceAdmin },
					SecurityAuthorization.Read);

				// Admin
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.Admin },
					new Role[] { Role.SystemAdmin, Role.MetricsAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.SystemAdmin },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Find and Replace
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.FindReplace },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.ReadUpdate);

				// ExportToProPricer
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.ExportToProPricer },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.ExportToProPricerSystemAdmin },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Help
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.Help, SecurityPage.GenBOEHelp },
						 new Role[] { Role.SystemAdmin, Role.WorkspaceAdmin, Role.Author, Role.SubcontractorAuthor, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceUser, Role.SubcontractAdmin },
						 SecurityAuthorization.Read);

				// Historical Metric Search Results
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.HistoricalMetricSearch },
					new Role[] { Role.WorkspaceAdmin, Role.WorkspaceUser },
					SecurityAuthorization.CreateReadUpdateDelete);

				//BOE Search
				InitializeMatrix(new SecurityPage[] { SecurityPage.BoeSearch },
					new WorkspaceState[] { WorkspaceState.Working },
					new BOEState[] { BOEState.Draft },
					new Role[] { Role.Author, Role.SubcontractorAuthor },
					SecurityAuthorization.CreateReadUpdateDelete);

				//ProjectMap BOE Search
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.ProjectMapBoeSearch },
					new WorkspaceState[] { WorkspaceState.Working },
					new Role[] { Role.Author, Role.SubcontractorAuthor, Role.WorkspaceAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				//Copy BOE Conflicts
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BoeCopyConflicts },
					new Role[] { Role.Author, Role.SubcontractorAuthor },
					SecurityAuthorization.Read);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.UpdateLockedResourceRatesMenuOption },
					new WorkspaceState[] { WorkspaceState.Locked },
					new Role[] { Role.WorkspaceAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.UpdateZoneTravelRatesMenuOption },
					new Role[] { Role.WorkspaceAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.UpdateOffloadRatesMenuOption },
					new Role[] { Role.WorkspaceAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.UpdateUCOTFactorMenuOption },
					new Role[] { Role.WorkspaceAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.UpdateLockedResourceRates },
					new Role[] { Role.SystemAdmin, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.WorkspaceReviewer, Role.SubcontractorAuthor, Role.SubcontractAdmin },
					SecurityAuthorization.Read);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.UpdateLockedResourceRates },
					new WorkspaceState[] { WorkspaceState.Locked },
					new Role[] { Role.WorkspaceAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Workspace BOE Custom Field Resource
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.BoeCustomFieldResource },
					new WorkspaceState[] { WorkspaceState.Locked, WorkspaceState.Closed },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.BoeCustomFieldResource },
					new WorkspaceState[] { WorkspaceState.Complete },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.Read);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BoeCustomFieldResource },
					new Role[] { Role.SubcontractAdmin },
					SecurityAuthorization.Read);

				//Workspace BOE Custom Fields (not Resource or Perf Orgs)
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.BoeCustomFields },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked, WorkspaceState.Closed },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.BoeCustomFields },
					new WorkspaceState[] { WorkspaceState.Complete },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.Read);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.EditBoeLockedState },
					new WorkspaceState[] { WorkspaceState.Locked },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.GettingStartedMenuOption },
					new Role[] { Role.WorkspaceAdmin, Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);
			} 
            else
            {
				Role[] allRoles = new Role[] { Role.None, Role.Author, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.MetricsAdmin, Role.SystemAdmin, Role.WorkspaceUser, Role.CreateWorkspacePermissions, Role.SubcontractorAuthor, Role.SubcontractAdmin };
				Role[] allButSubcontractor = new Role[] { Role.None, Role.SubcontractAdmin, Role.SubcontractorAuthor };

                // Home
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.Home },
					new Role[] { Role.None },
					SecurityAuthorization.CreateReadUpdateDelete);

				// SelectWorkspace
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.SelectWorkspace },
					allRoles,
					SecurityAuthorization.Read);

				// Workspace Home
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.WorkspaceHome },
					allRoles,
					SecurityAuthorization.Read);

				// Manage CLINs, Manage WBS, Manage BOEs
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.ManageCLINs, SecurityPage.ManageWBS, SecurityPage.ManageBOEs },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.ManageCLINs, SecurityPage.ManageWBS, SecurityPage.ManageBOEs },
					 new WorkspaceState[] { WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed },
					 new Role[] { Role.SystemAdmin },
					 SecurityAuthorization.Read);

				InitializeMatrix(new SecurityPage[] { SecurityPage.ManageBOEs },
					 new WorkspaceState[] { WorkspaceState.Locked },
					 new BOEState[] { BOEState.None },  // to allow updates from the Manage BOE summary grid
					 new Role[] { Role.SystemAdmin },
					 SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceRecalculateActuals },
					 new WorkspaceState[] { WorkspaceState.Working },
					 new Role[] { Role.SystemAdmin },
					 SecurityAuthorization.CreateReadUpdateDelete);

				// Manage INL Forms
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.ManageBOEForms },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.ManageBOEForms },
					 new WorkspaceState[] { WorkspaceState.Complete, WorkspaceState.Closed },
					 new Role[] { Role.SystemAdmin },
					 SecurityAuthorization.Read);

				// Workspace Admin Permissions
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.WorkspaceAdminPermissions },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Edit BOE (header - description) [WS Admin has read/write to this field]
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.EditBOEHeaderDescription },
					allRoles,
					SecurityAuthorization.Read);

				// Edit BOE (header - all besides description) [difference is WSAdmin only has readonly for these fields (WI 3479)]
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.EditBOEHeader },
					allRoles,
					SecurityAuthorization.Read);

				// exclude the About Tools menu option from Subcontractors
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.AboutToolsMenu },
					allButSubcontractor,
					SecurityAuthorization.Read);

				// exclude the Help menu option from Subcontractors
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.HelpMenu },
					allRoles,
					SecurityAuthorization.Read);

				// exclude the Contact menu option from Subcontractors
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.ContactMenu },
					allRoles,
					SecurityAuthorization.Read);

				// exclude the generation Home menu option from Subcontractors
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.GenerationHome },
					allRoles,
					SecurityAuthorization.Read);

				// Task Elements
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.TaskElements, SecurityPage.MOQEquationField, SecurityPage.TaskElementsMissionTask },
					allRoles,
					SecurityAuthorization.Read);

				// Boe and Task dates
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.BoeTaskDates },
					allRoles,
					SecurityAuthorization.Read);

				// Travel, Material, and ODC Grids (no subcontractorAuthor access WI 18262)
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(
                    new SecurityPage[] { SecurityPage.BOETravelGrid, SecurityPage.BoeODCGrid, SecurityPage.BoeMaterialsGrid, SecurityPage.BOEZoneTravelGrid },
					allButSubcontractor,
					SecurityAuthorization.Read);

				//Labor Grid
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BOELaborGrid },
					allRoles,
					SecurityAuthorization.Read);

				// Labor Types
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BoeLaborTypes },
					allRoles,
					SecurityAuthorization.Read);

				// Materials (no subcontractorAuthor access WI 18262)
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BOEMaterialsTypes }, 
					allButSubcontractor,
					SecurityAuthorization.Read);

				// BOE Comments
				InitializeMatrix(new SecurityPage[] { SecurityPage.BOEComment },
					new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },  // Reviewers can still comment when the workspace is Locked
					new BOEState[] { BOEState.Draft },
					allRoles,
					SecurityAuthorization.Read);

				InitializeMatrix(new SecurityPage[] { SecurityPage.BOECommentResponse },
					new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },  // Authors can still respond to approver or reviewer comments when the workspace is locked
					new BOEState[] { BOEState.Draft },
					allRoles,
					SecurityAuthorization.Read);

				// Edit BOE Buttons
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.SubmitForReview, SecurityPage.SubmitForApproval },
					allRoles,
					SecurityAuthorization.Read);

				// BOE Bulk Submit
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BulkSubmit },
					allRoles,
					SecurityAuthorization.Read);

				// Workspace RTE Templates
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.RTETemplates },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.Read);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.RTETemplates },
				new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Initialization, WorkspaceState.Locked },
					 new Role[] { Role.SystemAdmin },
					 SecurityAuthorization.CreateReadUpdateDelete);

				// Workspace Delete
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.WorkspaceDelete },
					new Role[] { Role.SystemAdmin }, SecurityAuthorization.CreateReadUpdateDelete);

				// Workspace Restore
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.WorkspaceRestore },
					new Role[] { Role.SystemAdmin }, SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrix(new SecurityPage[] { SecurityPage.ValidateBOE },
	                new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },  // Need to be able to validate the BOE as part of Submit-for-Approval
	                new BOEState[] { BOEState.Draft },
	                allRoles,
	                SecurityAuthorization.Read);

				// Metrics Admin
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.MetricsAdmin },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Create Workspace Permissions
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.CreateWorkspacePermissions },
						 new Role[] { Role.SystemAdmin },
						 SecurityAuthorization.CreateReadUpdateDelete);

				// Global Config Pages
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.GlobalConfig },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Workspace Resource Page
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceResource },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Workspace Performing Org Page
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspacePerfOrg },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Workspace Settings & Save Workspace Version
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettings, SecurityPage.WorkspaceSettingsStatus, SecurityPage.WorkspaceLaborCost, SecurityPage.BoeCustomFieldResource, SecurityPage.SaveVersion },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettings },
					new WorkspaceState[] { WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.Read);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettingsStatus, SecurityPage.WorkspaceLaborCost, SecurityPage.SaveVersion },
					new WorkspaceState[] { WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// BOEJ-2890 - Allow workspace admin to change output format template while workspace is locked (as well as in Initialization or Working states).
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettingsOutputFormatTemplate },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettingsOutputFormatTemplate },
					new WorkspaceState[] { WorkspaceState.Complete, WorkspaceState.Closed },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.Read);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.WorkspaceSettingsShareAndAllowSearch },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked, WorkspaceState.Complete, WorkspaceState.Closed },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Reports
				// Reports (no subcontractorAuthor access WI 18262)
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.Reports },
					allButSubcontractor,
					SecurityAuthorization.Read);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.Reports },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// RMS SSRS Reports
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.SSRSReports },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.Read);

				// Admin
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.Admin },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.SystemAdmin },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Find and Replace
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.FindReplace },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.ReadUpdate);

				// ExportToProPricer
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.ExportToProPricer },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.ExportToProPricerSystemAdmin },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				// Help
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.Help, SecurityPage.GenBOEHelp },
                    allRoles, 
                    SecurityAuthorization.Read);
				
				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.UpdateLockedResourceRates },
					allRoles,
					SecurityAuthorization.Read);

				// Workspace BOE Custom Field Resource
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.BoeCustomFieldResource },
					new WorkspaceState[] { WorkspaceState.Locked, WorkspaceState.Closed },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.BoeCustomFieldResource },
					new WorkspaceState[] { WorkspaceState.Complete },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.Read);

				//Workspace BOE Custom Fields (not Resource or Perf Orgs)
				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.BoeCustomFields },
					new WorkspaceState[] { WorkspaceState.Initialization, WorkspaceState.Working, WorkspaceState.Locked, WorkspaceState.Closed },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.BoeCustomFields }, new WorkspaceState[] { WorkspaceState.Complete },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.Read);

				InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.EditBoeLockedState },
					new WorkspaceState[] { WorkspaceState.Locked },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);

				InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.GettingStartedMenuOption },
					new Role[] { Role.SystemAdmin },
					SecurityAuthorization.CreateReadUpdateDelete);
			}

            #endregion
        }

        /// <summary>
        /// Method allowing subclasses to apply their own updates to the base-level security matrix
        /// </summary>
        protected virtual void CustomizeMatrix()
        {
        }

        protected void FinalizeMatrix()
        {
            #region Post-processing (All implementations)

            /*
             * Initialize Locked-workspace authorizations for the BOE-Draft/DraftLocked states.
             * Fine-tuning of these authorizations follows.
             * 
             */
            foreach (SecurityPage page in Enum.GetValues(typeof(SecurityPage)))
            {
                foreach (Role role in Enum.GetValues(typeof(Role)))
                {
                    int currentAuthorization, newAuthorization;  // SecurityAuthorization enum

                    // First:  DraftLocked authorizations are essentially a copy of the original Locked/Draft ones
                    currentAuthorization = _SecurityMatrix[(int)page, (int)WorkspaceState.Locked, (int)BOEState.DraftLocked, (int)role];
                    newAuthorization = _SecurityMatrix[(int)page, (int)WorkspaceState.Locked, (int)BOEState.Draft, (int)role];
                    if (newAuthorization > currentAuthorization)  // avoid downgrading
                    {
                        _SecurityMatrix[(int)page, (int)WorkspaceState.Locked, (int)BOEState.DraftLocked, (int)role] = newAuthorization;
                    }

                    // Second:  Draft authorizations are essentially a copy of the original Working/Draft ones
                    currentAuthorization = _SecurityMatrix[(int)page, (int)WorkspaceState.Locked, (int)BOEState.Draft, (int)role];
                    newAuthorization = _SecurityMatrix[(int)page, (int)WorkspaceState.Working, (int)BOEState.Draft, (int)role];
                    if (newAuthorization > currentAuthorization)  // avoid downgrading
                    {
                        _SecurityMatrix[(int)page, (int)WorkspaceState.Locked, (int)BOEState.Draft, (int)role] = newAuthorization;
                    }
                }
            }

            #endregion
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void PrintMatrix()
        {
            bool print = false;

            if (print)  // set to "true" while debugging to create a readable, Excel-based CSV of the security matrix
            {
                string companyName = ConfigurationUtilities.GetAppSetting("CompanyConfiguration") ?? CompanyConfiguration.ISGS.ToString();
                string outputFilename = string.Format(@"C:\Users\{0}\Desktop\SecurityMatrix-{1}.csv", System.Environment.UserName, companyName);

                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(System.IO.File.Open(outputFilename, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite)))
                {
                    sw.WriteLine("PAGE,WORKSPACE,BOE,ROLE,AUTHORIZATION");

                    foreach (SecurityPage page in Enum.GetValues(typeof(SecurityPage)))
                    {
                        foreach (WorkspaceState workspaceState in Enum.GetValues(typeof(WorkspaceState)))
                        {
                            foreach (BOEState boeState in Enum.GetValues(typeof(BOEState)))
                            {
                                foreach (Role role in Enum.GetValues(typeof(Role)))
                                {
                                    SecurityAuthorization authorization = (SecurityAuthorization)_SecurityMatrix[(int)page, (int)workspaceState, (int)boeState, (int)role];
                                    sw.WriteLine("{0},{1},{2},{3},{4}", page.ToString(), workspaceState.ToString(), boeState.ToString(), role.ToString(), authorization.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialize elements in the matrix for ALL BOE states and ALL Workspace states.
        /// This is a convenience method to quickly and easily populate nodes in the matrix.
        /// </summary>
        /// <param name="inPages">Pages to seed in the matrix</param>
        /// <param name="inRoles">Roles to seed in the matrix from enums SecurityAdminRole, Role, SecurityWorkspaceRole</param>
        /// <param name="inAuthorization">Authorizations to seed in the matrix</param>
        protected void InitializeMatrixAllBOEStatesAndAllWorkspaceStates(SecurityPage[] inPages, Role[] inRoles, SecurityAuthorization inAuthorization)
        {
            foreach (BOEState boeState in Enum.GetValues(typeof(BOEState)))
            {
                foreach (WorkspaceState wsState in Enum.GetValues(typeof(WorkspaceState)))
                {
                    this.InitializeMatrix(inPages, new WorkspaceState[] { wsState }, new BOEState[] { boeState }, inRoles, inAuthorization);
                }
            }
        }

        /// <summary>
        /// Initialize elements in the matrix for ALL BOE states
        /// This is a convenience method to quickly and easily populate nodes in the matrix.
        /// </summary>
        /// <param name="inPages">Pages to seed in the matrix</param>
        /// <param name="inWorkspaceState">Workspace states to seed in the matrix</param>
        /// <param name="inRoles">Roles to seed in the matrix from enums SecurityAdminRole, Role, SecurityWorkspaceRole</param>
        /// <param name="inAuthorization">Authorizations to seed in the matrix</param>
        protected void InitialMatrixAllBOEStates(SecurityPage[] inPages, WorkspaceState[] inWorkspaceStates, Role[] inRoles, SecurityAuthorization inAuthorization)
        {
            foreach (BOEState boeState in Enum.GetValues(typeof(BOEState)))
            {
                this.InitializeMatrix(inPages, inWorkspaceStates, new BOEState[] { boeState }, inRoles, inAuthorization);
            }
        }

        /// <summary>
        /// Initialize elements in the matrix for ALL Workspace states
        /// This is a convenience method to quickly and easily populate nodes in the matrix.
        /// </summary>
        /// <param name="inPages">Pages to seed in the matrix</param>
        /// <param name="inBOEState">BOE states to seed in the matrix</param>
        /// <param name="inRoles">Roles to seed in the matrix from enums SecurityAdminRole, Role, SecurityWorkspaceRole</param>
        /// <param name="inAuthorization">Authorizations to seed in the matrix</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void InitialMatrixAllWorkspaceStates(SecurityPage[] inPages, BOEState[] inBOEStates, Role[] inRoles, SecurityAuthorization inAuthorization)
        {
            foreach (WorkspaceState wsState in Enum.GetValues(typeof(WorkspaceState)))
            {
                this.InitializeMatrix(inPages, new WorkspaceState[] { wsState }, inBOEStates, inRoles, inAuthorization);
            }
        }

        /// <summary>
        /// Initialize elements in the matrix
        /// This is a convenience method to quickly and easily populate nodes in the matrix.
        /// </summary>
        /// <param name="inPages">Pages to seed in the matrix</param>
        /// <param name="inWorkspaceState">Workspace states to seed in the matrix</param>
        /// <param name="inBOEState">BOE states to seed in the matrix</param>
        /// <param name="inRoles">Roles to seed in the matrix from enums SecurityAdminRole, Role, SecurityWorkspaceRole</param>
        /// <param name="inAuthorization">Authorizations to seed in the matrix</param>
        protected void InitializeMatrix(SecurityPage[] inPages, WorkspaceState[] inWorkspaceState, BOEState[] inBOEState, Role[] inRoles, SecurityAuthorization inAuthorization)
        {
            if (inPages == null)
            {
                throw new ArgumentNullException(nameof(inPages), "inPages cannot be null");
            }
            if (inWorkspaceState == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceState), "inWorkspaceState cannot be null");
            }
            if (inBOEState == null)
            {
                throw new ArgumentNullException(nameof(inBOEState), "inBOEState cannot be null");
            }
            if (inRoles == null)
            {
                throw new ArgumentNullException(nameof(inRoles), "inRoles cannot be null");
            }

            foreach (WorkspaceState wsState in inWorkspaceState)
            {
                foreach (BOEState boeState in inBOEState)
                {
                    foreach (Role role in inRoles)
                    {
                        foreach (SecurityPage page in inPages)
                        {
                            _SecurityMatrix[(int)page, (int)wsState, (int)boeState, (int)role] = (int)inAuthorization;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return true/false depending on users authorizations for the roles requested
        /// </summary>
        /// <param name="inPermissions">The roles to determine and see if user has any of these roles in the system</param>
        /// <returns>Return true if the user has any of the roles passed in, false otherwise</returns>
        virtual public SecurityAuthorization IsAuthorized(SecurityPermissionsRequested inPermissions, WorkspaceDTO workspace, IReadOnlyCollection<SecurityPermissionsResponse> rolesForUser)
        {
            if (inPermissions == null)
            {
                throw new ArgumentNullException(nameof(inPermissions));
            }

            List<SecurityPermissionsResponse> userRoles = rolesForUser.ToList();

            // If you aren't allowed to have a WS Admin permission (controlled via Create WS/System Admin Permissions), you shouldn't have it. 
            // This is horrid, but it's necessary because you can assign a role to an AD group, hence the secondary check being necessary
            if (!userRoles.Any(x => x.AuthorizedRole == Role.CreateWorkspacePermissions || x.AuthorizedRole == Role.SystemAdmin))
            {
                userRoles = userRoles.Where(x => x.AuthorizedRole != Role.WorkspaceAdmin).ToList();
            }
            
            SecurityAuthorization authorization = GetAuthorizationsRoles(inPermissions, userRoles, workspace);

            return authorization;
        }

        /// <summary>
        /// Get the highest level of authorizations for the roles 
        /// </summary>
        /// <param name="inPermission">The requested permissions</param>
        /// <param name="rolesForUser">The user's roles</param>
        /// <returns>The highest security permission present between for the roles the user has</returns>
        private SecurityAuthorization GetAuthorizationsRoles(SecurityPermissionsRequested inPermission,
            List<SecurityPermissionsResponse> rolesForUser, WorkspaceDTO workspace)
        {
            #region Figure out requirement of WS and Boe and verify that it was as needed

            bool wsRequired = false;
            bool boeRequired = false;

            switch (inPermission.PageToCheck)
            {
                case SecurityPage.AboutToolsMenu:
                case SecurityPage.ContactMenu:
                case SecurityPage.HelpMenu:
                case SecurityPage.GenerationHome:
                case SecurityPage.GenBOEHelp:
                case SecurityPage.Home:
                case SecurityPage.MetricsAdmin:
                case SecurityPage.GlobalConfig:
                case SecurityPage.Admin:
                case SecurityPage.SystemAdmin:
                case SecurityPage.HistoricalMetricSearch:
                case SecurityPage.CreateWorkspacePermissions:
                    wsRequired = false;
                    boeRequired = false;
                    break;

                case SecurityPage.ManageCLINs:
                case SecurityPage.ManageBOEs:
                case SecurityPage.ManageWBS:
                case SecurityPage.WorkspaceSettings:
                case SecurityPage.WorkspaceSettingsStatus:
                case SecurityPage.WorkspaceSettingsOutputFormatTemplate:
                case SecurityPage.WorkspaceSettingsShareAndAllowSearch:
                case SecurityPage.WorkspaceAdminPermissions:
                case SecurityPage.Reports:
                case SecurityPage.SSRSReports:
                case SecurityPage.SelectWorkspace:
                case SecurityPage.WorkspaceHome:
                case SecurityPage.FindReplace:
                case SecurityPage.ExportToProPricer:
                case SecurityPage.ExportToProPricerSystemAdmin:
                case SecurityPage.Help:
                case SecurityPage.ProjectMapBoeSearch:
                case SecurityPage.BoeCopyConflicts:
                case SecurityPage.UpdateLockedResourceRates:
                case SecurityPage.UpdateLockedResourceRatesMenuOption:
                case SecurityPage.WorkspaceLaborCost:
                case SecurityPage.BoeCustomFieldResource:
                case SecurityPage.BoeCustomFields:
                case SecurityPage.SaveVersion:
                case SecurityPage.WorkspaceResource:
                case SecurityPage.WorkspacePerfOrg:
                case SecurityPage.EditBoeLockedState:
                case SecurityPage.ManageBOEForms:
                case SecurityPage.UpdateZoneTravelRatesMenuOption:
                case SecurityPage.UpdateOffloadRatesMenuOption:
				case SecurityPage.UpdateUCOTFactorMenuOption:
                case SecurityPage.BulkSubmit:
                case SecurityPage.WorkspaceDelete:
                case SecurityPage.WorkspaceRestore:
                case SecurityPage.GettingStartedMenuOption:
                case SecurityPage.RTETemplates:
				case SecurityPage.WorkspaceRecalculateActuals:
					wsRequired = true;
                    boeRequired = false;
                    break;

                case SecurityPage.EditBOEHeaderDescription:
                case SecurityPage.EditBOEHeader:
                case SecurityPage.TaskElements:
                case SecurityPage.TaskElementsMissionTask:
                case SecurityPage.MOQEquationField:
                case SecurityPage.BOEApproval:
                case SecurityPage.BOEComment:
                case SecurityPage.BOECommentResponse:
                case SecurityPage.SubmitForReview:
                case SecurityPage.SubmitForApproval:
                case SecurityPage.ValidateBOE:
                case SecurityPage.BoeLaborTypes:
                case SecurityPage.BOEMaterialsTypes:
                case SecurityPage.BOETravelGrid:
                case SecurityPage.BOEZoneTravelGrid:
                case SecurityPage.BOELaborGrid:
                case SecurityPage.BoeMaterialsGrid:
                case SecurityPage.BoeODCGrid:
                case SecurityPage.BoeSearch:
                case SecurityPage.BoeTaskDates:
                    wsRequired = true;
                    boeRequired = true;
                    break;

                default:
                    throw new ArgumentException("Page " + inPermission.PageToCheck + " not found.");
            }

            // enforce the page requirements for BOEID && workspaceID
            if (wsRequired && workspace == null)
            {
                throw new ArgumentException("The workspace is required for security checks and was not supplied for page type " + inPermission.PageToCheck);
            }
            else if (boeRequired && inPermission.BOEId == null)
            {
                throw new ArgumentException("The boe ID is required for security checks and was not supplied for page type " + inPermission.PageToCheck);
            }

            #endregion

            BOEState boeState = BOEState.None;
            if (boeRequired)
            {
                boeState = this.boeLoader.GetBoeState(inPermission.BOEId.Value);
            }

            // A deleted workspace is only accessible to system admins
            if (wsRequired && workspace.HasBeenDeleted && rolesForUser.All(x => x.AuthorizedRole != Role.SystemAdmin))
            {
                return SecurityAuthorization.None;
            }

            SecurityAuthorization authorization = SecurityAuthorization.None;
            foreach (SecurityPermissionsResponse permissionResponse in rolesForUser)
            {
                WorkspaceState currentWorkspaceState = WorkspaceState.None;
                BOEState currentBOEState = BOEState.None;

                switch (permissionResponse.AuthorizedRole)
                {
                    // the Role.None case needs to remain in the switch since it gets assigned permissions in the security matrix.
                    case Role.None:
                    case Role.MetricsAdmin:
                    case Role.SystemAdmin:
                    case Role.CreateWorkspacePermissions:
                        // if the workspace is required, and the WS ID is the WS ID the user requested get the current state, otherwise continue on the next element.. 
                        if (wsRequired)
                        {
                            currentWorkspaceState = workspace.WorkspaceState;
                        }

                        // get boe state b/c if we are a workspace user we don't need the BOE ID to match
                        // but we do need the current state.  An example of this is the TaskElements page
                        // where an assigned author on the workspace but not on THIS BOE should still
                        // potentially have READ access.
                        if (boeRequired)
                        {
                            currentBOEState = boeState;
                        }

                        break;

                    case Role.WorkspaceAdmin:
                    case Role.WorkspaceUser:
                    case Role.WorkspaceReviewer:
                    case Role.SubcontractAdmin:
                        // if the workspace is required, and the WS ID is the WS ID the user requested get the current state, otherwise continue on the next element.. 
                        if (wsRequired && permissionResponse.WorkspaceId != inPermission.WorkspaceId.Value)
                        {
                            continue;
                        }
                        else
                        {
                            if (wsRequired)
                            {
                                currentWorkspaceState = workspace.WorkspaceState;
                            }

                            // get boe state b/c if we are a workspace user we don't need the BOE ID to match
                            // but we do need the current state.  An example of this is the TaskElements page
                            // where an assigned author on the workspace but not on THIS BOE should still
                            // potentially have READ access.
                            if (boeRequired)
                            {
                                currentBOEState = boeState;
                            }
                        }

                        break;

                    case Role.Approver:
                    case Role.Author:
                    case Role.SubcontractorAuthor:
                        if (inPermission.PageToCheck == SecurityPage.SaveVersion && inPermission.BOEId != null)
                        {
                            boeRequired = true;
                        }

                        // if the boeid is required and the BOEID if the BOEID the user requested get the current state, otherwise continue on the next element..
                        if ((boeRequired && permissionResponse.BOEId != inPermission.BOEId) ||
                            (wsRequired && permissionResponse.WorkspaceId != inPermission.WorkspaceId.Value))
                        {
                            continue;
                        }
                        else
                        {
                            if (boeRequired)
                            {
                                currentBOEState = boeState;
                            }

                            if (wsRequired)
                            {
                                currentWorkspaceState = workspace.WorkspaceState;
                            }
                        }

                        break;

                    default:
                        _log.Error("Unknown role returned from database " + permissionResponse.AuthorizedRole);
                        break;
                }

                // Check the matrix ...
                // if neither ws nor boe state are required .. check perms
                // OR if ws required but ws state is NOT None
                // OR if boe required but boe state is NOT None ... 
                // OTHERWISE fall to the else because we don't want to consult the matrix
                // since the caller has passed us a weird setup
                if ((!wsRequired && !boeRequired) 
                                ||
                    (wsRequired && currentWorkspaceState != WorkspaceState.None) 
                                ||
                    (boeRequired && currentBOEState != BOEState.None))
                {
                    // check the authorization and get the max from our running list
                    authorization = _Max((SecurityAuthorization)TheSecurityMatrix[(int)inPermission.PageToCheck, (int)currentWorkspaceState,
                                (int)currentBOEState, (int)permissionResponse.AuthorizedRole], authorization);
                }
                else
                {
                    // even though permissions were required for a workspace
                    // and/or boe the state is none .. dive deeper

                    if (wsRequired && currentWorkspaceState == WorkspaceState.None)
                    {
                        // for a workspace .. we should never have it in the 'none' state
                        string errorMsg = "User requested " + inPermission.ToString() + " but the Workspace state is " + currentWorkspaceState + ".";
                        throw new ArgumentOutOfRangeException(errorMsg);
                    }

                    if (boeRequired && currentBOEState == BOEState.None)
                    {
                        // for a workspace .. we should never have it in the 'none' state
                        string errorMsg = "User requested " + inPermission.ToString() + " but the BOE state is " + currentBOEState + ".";
                        throw new ArgumentOutOfRangeException(errorMsg);
                    }
                }
            }

            return authorization;
        }

        /// <summary>
        /// Determine the max value between 2 different authorizations
        /// </summary>
        /// <param name="inSecurityAuthorizationsLeft">The authorization to compare</param>
        /// <param name="inSecurityAuthorizationsRight">The other authorization to compare</param>
        /// <returns>the highest enum value between the 2 parameters</returns>
        private SecurityAuthorization _Max(SecurityAuthorization inSecurityAuthorizationsLeft, SecurityAuthorization inSecurityAuthorizationsRight)
        {
            return (SecurityAuthorization)Math.Max((int)inSecurityAuthorizationsLeft, (int)inSecurityAuthorizationsRight);
        }
    }
}
