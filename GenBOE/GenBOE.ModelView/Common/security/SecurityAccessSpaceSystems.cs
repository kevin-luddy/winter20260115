// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common.security
{
    using IES.Common;
    using GenBOE.DataBridge.DTO;

    public class SecurityAccessSpaceSystems : SecurityAccess
    {
        public SecurityAccessSpaceSystems(IBoeDTODataLoader boeLoader)
            : base(boeLoader)
        {
        }

        protected override void CustomizeMatrix()
        {
            // Edit BOE (header - all besides description)
            InitializeMatrix(new SecurityPage[] { SecurityPage.EditBOEHeader },
                        new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },
                        new BOEState[] { BOEState.Draft },
                        new Role[] { Role.Author, Role.WorkspaceAdmin },
                        SecurityAuthorization.ReadUpdate);

            InitializeMatrix(new SecurityPage[] { SecurityPage.EditBOEHeaderDescription },
                        new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },
                        new BOEState[] { BOEState.Draft },
                        new Role[] { Role.WorkspaceAdmin },
                        SecurityAuthorization.ReadUpdate);

            // Task Elements
            InitializeMatrix(new SecurityPage[] { SecurityPage.TaskElements, SecurityPage.MOQEquationField, SecurityPage.TaskElementsMissionTask },
                     new WorkspaceState[] { WorkspaceState.Working },
                     new BOEState[] { BOEState.Draft },
                     new Role[] { Role.Author, Role.WorkspaceAdmin },
                     SecurityAuthorization.CreateReadUpdateDelete);

            InitializeMatrix(new SecurityPage[] { SecurityPage.TaskElements },
                     new WorkspaceState[] { WorkspaceState.Locked },
                     new BOEState[] { BOEState.Draft },
                     new Role[] { Role.Author, Role.WorkspaceAdmin },
                     SecurityAuthorization.CreateReadUpdateDelete);

            // Labor Travel, Material, and ODC Grids
            InitializeMatrix(new SecurityPage[] { SecurityPage.BOELaborGrid, SecurityPage.BOETravelGrid, SecurityPage.BoeODCGrid, SecurityPage.BoeMaterialsGrid },
                    new WorkspaceState[] { WorkspaceState.Working },
                    new BOEState[] { BOEState.Draft },
                    new Role[] { Role.Author, Role.WorkspaceAdmin },
                    SecurityAuthorization.CreateReadUpdateDelete);

            // Labor Types
            InitializeMatrix(new SecurityPage[] { SecurityPage.BoeLaborTypes },
             new WorkspaceState[] { WorkspaceState.Working },
             new BOEState[] { BOEState.Draft },
             new Role[] { Role.Author, Role.WorkspaceAdmin },
             SecurityAuthorization.CreateReadUpdateDelete);

            // Materials
            InitializeMatrix(new SecurityPage[] { SecurityPage.BOEMaterialsTypes },
                     new WorkspaceState[] { WorkspaceState.Working },
                     new BOEState[] { BOEState.Draft },
                     new Role[] { Role.Author, Role.WorkspaceAdmin },
                     SecurityAuthorization.CreateReadUpdateDelete);

            // BOE Comments
            InitializeMatrix(new SecurityPage[] { SecurityPage.BOECommentResponse },
                     new WorkspaceState[] { WorkspaceState.Working },
                     new BOEState[] { BOEState.Draft },
                     new Role[] { Role.Author, Role.WorkspaceAdmin },
                     SecurityAuthorization.CreateReadUpdateDelete);

            // Edit BOE Buttons
            InitializeMatrix(new SecurityPage[] { SecurityPage.SubmitForReview, SecurityPage.SubmitForApproval },
                     new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },  // Authors and Admins can still submit for review/approval when the workspace is locked
                     new BOEState[] { BOEState.Draft, BOEState.DraftLocked },
                     new Role[] { Role.Author, Role.WorkspaceAdmin },
                     SecurityAuthorization.ReadUpdate);

            // BOE Bulk Submit
            InitialMatrixAllBOEStates(new SecurityPage[] { SecurityPage.BulkSubmit },
                     new WorkspaceState[] { WorkspaceState.Working, WorkspaceState.Locked },  // Authors and Admins can still submit for review/approval when the workspace is locked
                     new Role[] { Role.Author, Role.WorkspaceAdmin },
                     SecurityAuthorization.ReadUpdate);

            //BOE Search
            InitializeMatrix(new SecurityPage[] { SecurityPage.BoeSearch },
                new WorkspaceState[] { WorkspaceState.Working },
                new BOEState[] { BOEState.Draft },
                new Role[] { Role.Author, Role.WorkspaceAdmin },
                SecurityAuthorization.CreateReadUpdateDelete);

            // Boe and Task dates
            InitializeMatrix(new SecurityPage[] { SecurityPage.BoeTaskDates },
                new WorkspaceState[] { WorkspaceState.Working },
                new BOEState[] { BOEState.Draft },
                new Role[] { Role.Author, Role.WorkspaceAdmin },
                SecurityAuthorization.CreateReadUpdateDelete);

            //Copy BOE Conflicts
            InitializeMatrixAllBOEStatesAndAllWorkspaceStates(new SecurityPage[] { SecurityPage.BoeCopyConflicts },
                new Role[] { Role.Author, Role.WorkspaceAdmin },
                SecurityAuthorization.Read);
        }
    }
}
