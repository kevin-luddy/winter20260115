// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;

    [ExcludeFromCodeCoverage]
    public class WorkspaceVersionModelView
    {
        /// <summary>
        /// The state of the workspace at the time it was backed up.
        /// </summary>
        private WorkspaceState workspaceState = WorkspaceState.None;
 
        /// <summary>
        /// Default constructor.
        /// </summary>
        public WorkspaceVersionModelView()
        {
            VersionID = -1;
            VersionName = string.Empty;
            DateCreated = DateTime.MinValue;
            CreatedByDisplayName = string.Empty;
            Boes = new Collection<BoeVersionDTO>();
        }

        /// <summary>
        /// Alternate constructor.
        /// </summary>
        /// <param name="inVersion">Workspace version meta data.</param>
        /// <param name="inCreatedByUser">User who created the backup version.</param>
        public WorkspaceVersionModelView(WorkspaceVersionMetaDataDTO inVersion, UserDTODataLoader inCreatedByUser, ICollection<BoeVersionDTO> inBoes)
        {
            if (inCreatedByUser == null)
            {
                throw new ArgumentNullException(nameof(inCreatedByUser));
            }
            if (inVersion == null)
            {
                throw new ArgumentNullException(nameof(inVersion));
            }

            VersionID = inVersion.VersionID;
            VersionName = inVersion.VersionName;
            DateCreated = inVersion.DateCreated;
            CreatedByDisplayName = inCreatedByUser.GetUserByID(inVersion.CreatedByID).DisplayName;
            this.workspaceState = inVersion.VersionState;
            this.Boes = inBoes;
        }

        /// <summary>
        /// Workspace version Id.
        /// </summary>
        public int VersionID { get; set; }

        /// <summary>
        /// Workspace version name.
        /// </summary>
        public string VersionName { get; set; }

        /// <summary>
        /// Date/Time the backup version was created.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// User display name that created the version.
        /// </summary>
        public String CreatedByDisplayName { get; set; }

        /// <summary>
        /// The final status of the version after it is restored.
        /// </summary>
        public String RestoreToWorkspaceState
        {
            get
            {
                // If the version was backed up in the initialization state, it is restored in initialization. All
                // other states will be restored as Working.
                WorkspaceState restoreState = WorkspaceState.Working;
                if (this.workspaceState == WorkspaceState.Initialization)
                {
                    restoreState = WorkspaceState.Initialization;
                }
                return restoreState.ToString();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is system backup.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system backup; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystemBackup
        {
            get
            {
                return this.VersionName.StartsWith(CommonConstants.AUTO_SYSTEM_BACKUP_DAILY);
            }
        }

        /// <summary>
        /// The BOEs for the backup version
        /// </summary>
        public ICollection<BoeVersionDTO> Boes { get; set; }
    }
}

