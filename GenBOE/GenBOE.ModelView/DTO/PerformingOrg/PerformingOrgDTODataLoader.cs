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

    public class PerformingOrgDTODataLoader : IPerformingOrgDTODataLoader
    {
        private Logger _log = new Logger(typeof(PerformingOrgDTODataLoader));
        private const int SYSTEM_PERF_ORG_LIST_ID = 1;

        public const int MAX_PERF_ORG_NAME_LENGTH = 20;

        public int GlobalListID
        {
            get
            {
                return SYSTEM_PERF_ORG_LIST_ID;
            }
        }

        public PerformingOrgDTODataLoader() { }

        /// <summary>
        /// Get the performing organizations by performing org list ID
        /// </summary>
        /// <param name="listId">performing organization list ID</param>
        /// <returns>collection of Performing Organizations</returns>
        [DbQuery]
        virtual public Collection<PerformingOrgDTO> GetByListId(int listId)
        {
            Collection<PerformingOrgDTO> toReturn = null;
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    Collection<PerformingOrganization> entities = null;

                    if (listId == SYSTEM_PERF_ORG_LIST_ID)
                    {
                        entities = gbe.PerformingOrganizations.Where(p => p.PerformingOrganizationListID == listId && p.DeletedFlag != true).ToCollection();
                    }
                    else
                    {
                        entities = (from p in gbe.PerformingOrganizations
                                    join wp in gbe.WorkspacePerformingOrganizations on p.PerformingOrganizationID equals wp.SystemPerformingOrganizationID
                                    where wp.PerformingOrganizationListID == listId
                                    select p).ToCollection();
                    }

                    toReturn = this.ConvertToDto(entities);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the GLOBAL performing organizations
        /// </summary>
        /// <returns>collection of Performing Organizations</returns>
        virtual public Collection<PerformingOrgDTO> GetGlobalPerformingOrgs()
        {
            return this.GetByListId(SYSTEM_PERF_ORG_LIST_ID);
        }

        /// <summary>
        /// Get a performing organization by its own ID
        /// </summary>
        /// <param name="inPerfOrgID">perf org ID</param>
        /// <returns>perf org data</returns>
        virtual public PerformingOrgDTO GetById(int inPerfOrgID)
        {
            return this.GetByIds(new List<int>() { inPerfOrgID }).FirstOrDefault();
        }

        /// <summary>
        /// Get a collection of performing organization by ids
        /// </summary>
        /// <param name="ids">Perf Org Ids</param>
        /// <returns>perf org data</returns>
        [DbQuery]
        virtual public Collection<PerformingOrgDTO> GetByIds(ICollection<int> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            Collection<PerformingOrgDTO> toReturn = new Collection<PerformingOrgDTO>();
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = this.ConvertToDto(gbe.PerformingOrganizations.Where(p => ids.Contains(p.PerformingOrganizationID)).ToCollection());
                }
            }

            return toReturn;
       }

        /// <summary>
        /// Get all performing orgs associated with the given performing org list ID & search term
        /// </summary>
        /// <param name="listId">perf org list ID</param>
        /// <param name="searchTerm">search term</param>
        /// <returns>performing org IDs</returns>
        [DbQuery]
        virtual public Collection<PerformingOrgDTO> GetByListIdAndPartialNameOrDescription(int listId, string searchTerm)
        {
            Collection<PerformingOrgDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    Collection<PerformingOrganization> entities = null;

                    if (listId == SYSTEM_PERF_ORG_LIST_ID)
                    {
                        entities = gbe.PerformingOrganizations.Where(b => b.PerformingOrganizationListID == listId && b.DeletedFlag != true
                            && (b.PerformingOrganizationName.Contains(searchTerm) || b.PerformingOrganizationDescription.Contains(searchTerm))).ToCollection();
                    }
                    else
                    {
                        entities = (from b in gbe.PerformingOrganizations
                                    join wp in gbe.WorkspacePerformingOrganizations on b.PerformingOrganizationID equals wp.SystemPerformingOrganizationID
                                    where wp.PerformingOrganizationListID == listId
                                            && (b.PerformingOrganizationName.Contains(searchTerm) || b.PerformingOrganizationDescription.Contains(searchTerm))
                                    select b).ToCollection();
                    }

                    toReturn = this.ConvertToDto(entities);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get all performing orgs associated with the given performing org list ID & name
        /// </summary>
        /// <param name="listId">perf org list ID</param>
        /// <param name="name">name</param>
        /// <returns>performing org IDs</returns>
        [DbQuery]
        virtual public PerformingOrgDTO GetByListIdAndName(int listId, string name)
        {
            PerformingOrgDTO toReturn = null;
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    Collection<PerformingOrganization> entities = null;

                    if (listId == SYSTEM_PERF_ORG_LIST_ID)
                    {
                        entities = gbe.PerformingOrganizations.Where(b => b.PerformingOrganizationListID == listId && b.DeletedFlag != true
                            && b.PerformingOrganizationName.Equals(name, StringComparison.CurrentCultureIgnoreCase)).ToCollection();
                    }
                    else
                    {
                        entities = (from b in gbe.PerformingOrganizations
                                    join wp in gbe.WorkspacePerformingOrganizations on b.PerformingOrganizationID equals wp.SystemPerformingOrganizationID
                                    where wp.PerformingOrganizationListID == listId && b.PerformingOrganizationName.Equals(name, StringComparison.CurrentCultureIgnoreCase)
                                    select b).ToCollection();
                    }

                    toReturn = this.ConvertToDto(entities).FirstOrDefault();
                }
            }

            return toReturn;
        }

        #region Commits
                    
        /// <summary>
        /// Save Performing Organizations
        /// </summary>
        /// <param name="inDefaultPerformingOrg">default performing org</param>
        virtual public Dictionary<int, int> SaveSystemPerformingOrgs(Collection<PerformingOrgDTO> inPerformingOrgs)
        {
            if (inPerformingOrgs == null)
            {
                throw new ArgumentNullException(nameof(inPerformingOrgs));
            }

            Dictionary<int, int> ids = new Dictionary<int, int>();

            foreach (PerformingOrgDTO perfOrg in inPerformingOrgs)
            {
                if (perfOrg.Updateable == UpdateType.None)
                {
                    throw new ArgumentException("please supply the Updateable argument");
                }

                if (perfOrg.Updateable == UpdateType.Deleted)
                {
                    DeleteSystemPerformingOrg(perfOrg);
                    ids.Add(perfOrg.Id, perfOrg.Id);
                }
                else if (perfOrg.Updateable == UpdateType.Upsert)
                {
                    perfOrg.PerformingOrgDesc = perfOrg.PerformingOrgDesc.Trim();
                    perfOrg.PerformingOrgName = perfOrg.PerformingOrgName.Trim();

                    int origid = perfOrg.Id;
                    int newid = UpdateSystemPerfOrg(perfOrg);
                    ids.Add(origid, newid);
                }
            }

            return ids;
        }

        /// <summary>
        /// Save workspace Performing Organizations
        /// </summary>
        /// <param name="inPerformingOrgs">workspace performing orgs</param>
        /// <param name="inWorkspacePerfOrgListID">workspace performing org list ID</param>
        virtual public Dictionary<int, int> SaveWorkspacePerformingOrgs(Collection<PerformingOrgDTO> inPerformingOrgs, int inWorkspacePerfOrgListID)
        {
            if (inPerformingOrgs == null)
            {
                throw new ArgumentNullException(nameof(inPerformingOrgs));
            }

            Dictionary<int, int> ids = new Dictionary<int, int>();

            foreach (PerformingOrgDTO perfOrg in inPerformingOrgs)
            {
                if (perfOrg.Updateable == UpdateType.None)
                {
                    throw new ArgumentException("please supply the Updateable argument");
                }

                if (perfOrg.Updateable == UpdateType.Deleted)
                {
                    DeleteWorkspacePerformingOrg(perfOrg, inWorkspacePerfOrgListID);
                    ids.Add(perfOrg.Id, perfOrg.Id);
                }
                else if (perfOrg.Updateable == UpdateType.Upsert)
                {
                    perfOrg.PerformingOrgDesc = perfOrg.PerformingOrgDesc.Trim();
                    perfOrg.PerformingOrgName = perfOrg.PerformingOrgName.Trim();

                    int origid = perfOrg.Id;
                    int newid = UpdateWorkspacePerfOrg(perfOrg, inWorkspacePerfOrgListID);
                    ids.Add(origid, newid);
                }
            }

            return ids;
        }

        /// <summary>
        /// Delete a system Performing Org
        /// </summary>
        /// <param name="inDeletePerformingOrg">Performing Org entry to delete</param>
        virtual public void DeleteSystemPerformingOrg(PerformingOrgDTO inDeletePerformingOrg)
        {
            if (inDeletePerformingOrg == null)
            {
                throw new ArgumentNullException(nameof(inDeletePerformingOrg));
            }

            try
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.deleteSystemPerformingOrganizationByPerformingOrganizationID(inDeletePerformingOrg.Id, inDeletePerformingOrg.UpdateDate);

                }
            }
            catch (SqlException sqlEx)
            {
                _log.Error(sqlEx, "An exception occurred in " + sqlEx.Procedure + " and has the following message " + sqlEx.Message);
                throw;
            }
        }

        /// <summary>
        /// Delete a workspace Performing Org
        /// </summary>
        /// <param name="inDeletePerformingOrg">Performing Org entry to delete</param>
        /// <param name="inWorkspacePerfOrgListID">workspace performing org list id</param>
        virtual public void DeleteWorkspacePerformingOrg(PerformingOrgDTO inDeletePerformingOrg, int inWorkspacePerfOrgListID)
        {
            if (inDeletePerformingOrg == null)
            {
                throw new ArgumentNullException(nameof(inDeletePerformingOrg));
            }

            try
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.deleteWorkspacePerformingOrganizationByPerformingOrganizationID(inDeletePerformingOrg.Id, inWorkspacePerfOrgListID);

                }
            }
            catch (SqlException sqlEx)
            {
                _log.Error(sqlEx, "An exception occurred in " + sqlEx.Procedure + " and has the following message " + sqlEx.Message);
                throw;
            }
        }

        /// <summary>
        /// update a default performing org
        /// </summary>
        /// <param name="inUpdatePerfOrg">Perf Org to update</param>
        private int UpdateSystemPerfOrg(PerformingOrgDTO inUpdatePerfOrg)
        {
            if (inUpdatePerfOrg == null)
            {
                throw new ArgumentNullException(nameof(inUpdatePerfOrg));
            }

            int resultID = 0;
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    resultID = Convert.ToInt32(gbe.upsertSystemPerformingOrganization(
                        inUpdatePerfOrg.Id,
                        inUpdatePerfOrg.PerformingOrgName,
                        inUpdatePerfOrg.PerformingOrgDesc,
                        SYSTEM_PERF_ORG_LIST_ID,
                        inUpdatePerfOrg.UpdateDate).FirstOrDefault());

                    // if the result ID is not a positive number, something bad went wrong so log it
                    if (resultID < 0)
                    {
                        _log.Error("The returned ID from upsertPerformingOrganization SP was negative");

                    }
                    else
                    {
                        // If this was a new performing org, set the new ID on the DTO for later use, if necessary
                        if (inUpdatePerfOrg.Id < 0)
                        {
                            inUpdatePerfOrg.Id = resultID;
                        }
                    }
                }
            }
            return resultID;
        }

        /// <summary>
        /// update a default performing org
        /// </summary>
        /// <param name="inUpdatePerfOrg">Perf Org to update</param>
        /// <param name="inWorkspacePerfOrgListID">workspace's performing org list id</param>
        private int UpdateWorkspacePerfOrg(PerformingOrgDTO inUpdatePerfOrg, int inWorkspacePerfOrgListID)
        {
            if (inUpdatePerfOrg == null)
            {
                throw new ArgumentNullException(nameof(inUpdatePerfOrg));
            }

            int resultID = 0;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    resultID = Convert.ToInt32(gbe.upsertWorkspacePerformingOrganization(
                        inUpdatePerfOrg.Id,
                        inUpdatePerfOrg.PerformingOrgName,
                        inUpdatePerfOrg.PerformingOrgDesc,
                        inWorkspacePerfOrgListID,
                        inUpdatePerfOrg.UpdateDate).FirstOrDefault());

                    // if the result ID is not a positive number, something bad went wrong so log it
                    if (resultID < 0)
                    {
                        _log.Error("The returned ID from upsertWorkspacePerformingOrganization SP was negative");
                    }
                    else
                    {
                        // If this was a new performing org, set the new ID on the DTO for later use, if necessary
                        if (inUpdatePerfOrg.Id < 0)
                        {
                            inUpdatePerfOrg.Id = resultID;
                        }
                    }
                }
            }

            return resultID;
        }

        #endregion

        #region Private

        /// <summary>
        /// Converts a collection of entities to a collection of dtos
        /// </summary>
        /// <param name="entities">Entities from the DB</param>
        /// <returns>Dtos</returns>
        private Collection<PerformingOrgDTO> ConvertToDto(Collection<PerformingOrganization> entities)
        {
            Collection<PerformingOrgDTO> result = new Collection<PerformingOrgDTO>();

            if (entities != null && entities.Any())
            {
                foreach (PerformingOrganization entity in entities)
                {
                    result.Add(new PerformingOrgDTO()
                    {
                        Id = entity.PerformingOrganizationID,
                        PerformingOrgName = entity.PerformingOrganizationName,
                        PerformingOrgDesc = entity.PerformingOrganizationDescription,
                        UpdateDate = entity.UpdateDT,
                        IsSystemPerfOrg = entity.PerformingOrganizationListID == SYSTEM_PERF_ORG_LIST_ID // need to know if we are linked to a system perf org or if this is a true workspace perf org
                    });
                }
            }

            return result.OrderBy(x => x.PerformingOrgName).ToCollection();
        }

        #endregion
    }
}
