// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.SqlClient;
    using System.Linq;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;

    public class WorkspaceVersionMetaDataDTODataLoader : IWorkspaceVersionMetaDataDTODataLoader
    {
        private Logger _log = new Logger(typeof(WorkspaceVersionMetaDataDTODataLoader));
        private const string RESTORE_WARNING_MESSAGE = "WARNING:";

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceVersionMetaDataDTODataLoader"/> class.
        /// </summary>
        public WorkspaceVersionMetaDataDTODataLoader() { }

        /// <summary>
        /// Get workspace version by workspace version ID
        /// </summary>
        /// <param name="inVersionID">workspace version ID</param>
        /// <returns>workspace version meta data</returns>
        [DbQuery]
        public Collection<WorkspaceVersionMetaDataDTO> GetByIds(Collection<int> inVersionID)
        {
            Collection<WorkspaceVersionMetaDataDTO> toReturn = new Collection<WorkspaceVersionMetaDataDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from w in gbe.WorkspaceVersions
                                where inVersionID.Contains(w.VersionID)
                                select new WorkspaceVersionMetaDataDTO
                                {
                                    Id = w.VersionID,
                                    VersionID = w.VersionID,
                                    VersionName = w.VersionName,
                                    CreatedByID = w.CreatedByETIUserID,
                                    VersionState = (WorkspaceState)w.WorkspaceStateID,
                                    DateCreated = w.VersionCreated,
                                    WorkspaceID = w.WorkspaceID
                                }).ToCollection<WorkspaceVersionMetaDataDTO>();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the workspace version IDs given a workspace ID
        /// </summary>
        /// <param name="inWorkspaceID">workspace ID</param>
        /// <returns>workspace version IDs</returns>
        [DbQuery]
        virtual public Collection<WorkspaceVersionMetaDataDTO> GetByWorkspaceID(int inWorkspaceID)
        {
            Collection<WorkspaceVersionMetaDataDTO> toReturn = new Collection<WorkspaceVersionMetaDataDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from w in gbe.WorkspaceVersions
                                where w.WorkspaceID == inWorkspaceID
                                orderby w.VersionCreated descending
                                select new WorkspaceVersionMetaDataDTO
                                {
                                    Id = w.VersionID,
                                    VersionID = w.VersionID,
                                    VersionName = w.VersionName,
                                    CreatedByID = w.CreatedByETIUserID,
                                    VersionState = (WorkspaceState)w.WorkspaceStateID,
                                    DateCreated = w.VersionCreated,
                                    WorkspaceID = w.WorkspaceID
                                }).ToCollection<WorkspaceVersionMetaDataDTO>();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the BOEs for the previous version of the Workspace
        /// </summary>
        /// <param name="versionID">Version ID</param>
        /// <param name="workspaceID">Workspace ID</param>
        /// <returns>BOEs for the previous version of the Workspace</returns>
        [DbQuery]
        virtual public ICollection<BoeVersionDTO> GetBoesByVersionID(int versionID, int workspaceID)
        {
            ICollection<BoeVersionDTO> toReturn = new Collection<BoeVersionDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from x in gbe.WBS_CLIN_BOE_XREF1
                                join b in gbe.BOE1 on new { A = x.BOEID.Value, B = x.VersionID } equals new { A = b.BOEID, B = b.VersionID }
                                join c in gbe.CLIN1 on  new { A = x.CLINID.Value, B = x.VersionID } equals new { A = c.CLINID, B = c.VersionID } into cx
                                from c in cx.DefaultIfEmpty()
                                join w in gbe.WorkBreakdownStructure1 on new { A = x.WBSID.Value, B = x.VersionID } equals new { A = w.WBSID, B = w.VersionID } into wx
                                from w in wx.DefaultIfEmpty()
                                where x.VersionID == versionID && b.WorkspaceID == workspaceID
                                select new BoeVersionDTO
                                {
                                    BoeId = b.BOEID,
                                    BoeTitle = b.BOETitle,
                                    ClinNumber = c.DisplayedCLINNumber,
                                    Clin = c.CLINTitle,
                                    WbsNumber = w.DisplayedWBSNumber,
                                    Wbs = w.WBSTitle
                                }).ToCollection<BoeVersionDTO>();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets for automatic backup generation.
        /// </summary>
        /// <param name="wsIds">The ws identifiers.</param>
        /// <returns></returns>
        [DbQuery]
        virtual public Collection<WorkspaceVersionMetaDataDTO> GetForAutoBackupGeneration(ICollection<int> wsIds)
        {
            Collection<WorkspaceVersionMetaDataDTO> toReturn = new Collection<WorkspaceVersionMetaDataDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = gbe.WorkspaceVersions.Where(x => wsIds.Contains(x.WorkspaceID) && x.VersionName.StartsWith(CommonConstants.AUTO_SYSTEM_BACKUP_DAILY))
                                    .OrderByDescending(x => x.VersionCreated)
                                    .Select(x => new WorkspaceVersionMetaDataDTO
                                    {
                                        VersionID = x.VersionID,
                                        VersionName = x.VersionName,
                                        VersionState = (WorkspaceState)x.WorkspaceStateID,
                                        DateCreated = x.VersionCreated,
                                        WorkspaceID = x.WorkspaceID
                                    }).ToCollection();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get all workspace versions. Primarily used by warm cache
        /// </summary>
        /// <returns></returns>
        [DbQuery]
        virtual public Collection<WorkspaceVersionMetaDataDTO> GetAllWorkspaceVersions()
        {
            Collection<WorkspaceVersionMetaDataDTO> toReturn = new Collection<WorkspaceVersionMetaDataDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from w in gbe.WorkspaceVersions
                                select new WorkspaceVersionMetaDataDTO
                                {
                                    Id = w.VersionID,
                                    VersionID = w.VersionID,
                                    VersionName = w.VersionName,
                                    CreatedByID = w.CreatedByETIUserID,
                                    VersionState = (WorkspaceState)w.WorkspaceStateID,
                                    DateCreated = w.VersionCreated,
                                    WorkspaceID = w.WorkspaceID
                                }).ToCollection<WorkspaceVersionMetaDataDTO>();

                    foreach (WorkspaceVersionMetaDataDTO meta in toReturn)
                    {
                        meta.DateCreated = GenBOEUtilities.AdjustDateTimePrecision(meta.DateCreated, DateTimePrecision.Day);
                    }

                }
            }

            return toReturn;
        }

        /// <summary>
        /// Save the workspace versions
        /// </summary>
        /// <param name="inWorkspaceVersions">workspace verisons to save</param>
        virtual public void Save(Collection<WorkspaceVersionMetaDataDTO> inWorkspaceVersions, int inWorkspaceID)
        {
            if (inWorkspaceVersions == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceVersions));
            }

            foreach (WorkspaceVersionMetaDataDTO workspaceVersion in inWorkspaceVersions)
            {
                if (workspaceVersion.Updateable == UpdateType.Upsert)
                {
                    Upsert(workspaceVersion, inWorkspaceID);
                }
                else if (workspaceVersion.Updateable == UpdateType.Deleted)
                {
                    Delete(workspaceVersion, inWorkspaceID);
                }

            }
        }

        /// <summary>
        /// Upsert the workspace version
        /// </summary>
        /// <param name="inSaveWorkspaceVersion">the workspace version meta data to update</param>
        virtual public void Upsert(WorkspaceVersionMetaDataDTO inSaveWorkspaceVersion, int inWorkspaceID)
        {
            if (inSaveWorkspaceVersion == null)
            {
                throw new ArgumentNullException(nameof(inSaveWorkspaceVersion));
            }

            try
            {
                using (StopwatchTimer sw = new StopwatchTimer(this._log))
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.Database.CommandTimeout = 1000;

                        int linqResults = gbe.createWorkspaceVersion(inSaveWorkspaceVersion.VersionName, inSaveWorkspaceVersion.CreatedByID, (int)inSaveWorkspaceVersion.VersionState, inWorkspaceID).FirstOrDefault().Value;

                        if (linqResults < 0)
                        {
                            _log.Error("The WorkspaceVersionMetaDataDataLoader.UpsertWorkspaceVersion Version ID did not save correctly and returned a negative ID value");
                        }

                    } // end gbe
                }
            }
            catch (SqlException ex)
            {
                _log.Error(ex);
                throw new GeneralAppException("There was an error during creation of a backup.  Contact a system administrator for assistance.");
            }
        }
        
        /// <summary>
        /// The workspace version meta data to delete
        /// </summary>
        /// <param name="inDeleteWorkspaceVersion">workspace version meta data to delete</param>
        virtual public void Delete(WorkspaceVersionMetaDataDTO inDeleteWorkspaceVersion, int inWorkspaceID)
        {
            if (inDeleteWorkspaceVersion == null)
            {
                throw new ArgumentNullException(nameof(inDeleteWorkspaceVersion));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // As weird as it is, in prod there has been at least 1 WS where it needed extended time. BOEJ-4122
                    gbe.Database.CommandTimeout = 360;
                    gbe.deleteWorkspaceVersion(inWorkspaceID, inDeleteWorkspaceVersion.VersionID);
                }
            }
        }

        /// <summary>
        /// restore the current workspace with the workspace version
        /// </summary>
        /// <param name="inWorkspaceVersion"></param>
        /// <returns></returns>

        virtual public string Restore(WorkspaceVersionMetaDataDTO inWorkspaceVersion, int inRestoredVersionBy)
        {
            string restoreFailureString = string.Empty;

            if (inWorkspaceVersion == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceVersion));
            }

            // If there was an error, the linqResults will contain a string with why it was unsuccessful
            // if the string is empty, restore was successful
            using (GenBoeEntities gbe = new GenBoeEntities())
             {
                 gbe.Database.CommandTimeout = 1800;
                 string linqResults = gbe.restoreWorkspaceVersion(inWorkspaceVersion.VersionID, inRestoredVersionBy, inWorkspaceVersion.WorkspaceID).FirstOrDefault();

                if (!string.IsNullOrEmpty(linqResults))
                {
                    if (linqResults.StartsWith(RESTORE_WARNING_MESSAGE))
                    {
                        // this is just a warning that we created, let it pass through.
                        restoreFailureString = linqResults;
                    }
                    else
                    {
                        _log.Error("Error during Workspace restore: " + linqResults);
                        throw new GeneralAppException("There was an error during Workspace Restore.  Contact a system administrator for assistance.");
                    }
                }
                
            } // end gbe
 
            return restoreFailureString;
        }
    }
}
