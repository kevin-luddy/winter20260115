// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Data.Entity.Core;
    using System.Data.SqlClient;
    using System.Linq;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;

    public class PerformingOrgListDTODataLoader : IPerformingOrgListDTODataLoader
    {
        private Logger _log = new Logger(typeof(PerformingOrgListDTODataLoader));

        /// <summary>
        /// Get the performing organization list data
        /// </summary>
        /// <param name="inPerfOrgListID">performing org list ID</param>
        /// <returns>performing org list data</returns>
        [DbQuery]
        virtual public PerformingOrgListDTO GetPerfOrgList(int inPerfOrgListID)
        {
            PerformingOrgListDTO toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var perfOrgList =
                       (from p in gbe.PerformingOrganizationLists
                        where p.PerformingOrganizationListID == inPerfOrgListID
                        select new PerformingOrgListDTO
                        {
                            PerformingOrgListID = p.PerformingOrganizationListID,
                            PerformingOrgListName = p.PerformingOrganizationListName,
                            UpdateDate = p.UpdateDT
                        }).FirstOrDefault();

                    toReturn = perfOrgList;
                }
            }

            return toReturn;
        }

        #region Commit


        /// <summary>
        /// Save the Performing Organization List
        /// </summary>
        /// <param name="inDefaultPerformingOrg">default performing org</param>
        virtual public int SavePerformingOrgList(PerformingOrgListDTO inPerformingOrgList)
        {
            if (inPerformingOrgList == null)
            {
                throw new ArgumentNullException(nameof(inPerformingOrgList));
            }
            if (inPerformingOrgList.Updateable == UpdateType.None)
            {
                throw new ArgumentException("please supply the Updateable argument");
            }
            if (inPerformingOrgList.Updateable == UpdateType.Deleted)
            {
                throw new ArgumentOutOfRangeException(nameof(inPerformingOrgList));
            }

            int id = 0;
            try
            {
                if (inPerformingOrgList.Updateable == UpdateType.Upsert)
                {
                    id = this.UpdatePerfOrgList(inPerformingOrgList);
                    inPerformingOrgList.PerformingOrgListID = id;
                }
            }
            catch (EntityCommandExecutionException ex)
            {
                _log.Error(ex);
                _log.Error(ex.InnerException);
                throw;
            }
            catch (SqlException sqlEx)
            {
                this._log.Error(sqlEx, "An exception occurred in " + sqlEx.Procedure + " and has the following message: " + sqlEx.Message + "/n");
                throw;
            }

            return id;
        }

        /// <summary>
        /// Clear all performing orgs from the given list
        /// </summary>
        /// <param name="inPerformingOrgList"></param>
        virtual public void ClearPerformingOrgList(PerformingOrgListDTO inPerformingOrgList)
        {
            if (inPerformingOrgList == null)
            {
                throw new ArgumentNullException(nameof(inPerformingOrgList));
            }

            try
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    if (inPerformingOrgList.Updateable == UpdateType.Upsert)
                    {
                        gbe.deletePerformingOrganizationByPerformingOrganizationListID(inPerformingOrgList.PerformingOrgListID, inPerformingOrgList.UpdateDate);
                    }
                }
            }
            catch (EntityCommandExecutionException ex)
            {
                _log.Error(ex);
                _log.Error(ex.InnerException);
                throw;
            }
            catch (SqlException sqlEx)
            {
                this._log.Error(sqlEx, "An exception occurred in " + sqlEx.Procedure + " and has the following message: " + sqlEx.Message + "/n");
                throw;
            }
        }
        
        /// <summary>
        /// update a  performing org list
        /// </summary>
        /// <param name="inUpdatePerfOrg">Perf Org List to update</param>
        private int UpdatePerfOrgList(PerformingOrgListDTO inUpdatePerfOrgList)
        {
            if (inUpdatePerfOrgList == null)
            {
                throw new ArgumentNullException(nameof(inUpdatePerfOrgList));
            }
            int newId = 0;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                try
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        newId = gbe.upsertPerformingOrganizationList(inUpdatePerfOrgList.PerformingOrgListID, inUpdatePerfOrgList.PerformingOrgListName, inUpdatePerfOrgList.UpdateDate).FirstOrDefault().Value;
                    }
                }
                catch (EntityCommandExecutionException ex)
                {
                    _log.Error(ex);
                    _log.Error(ex.InnerException);
                    throw;
                }
                catch (SqlException sqlEx)
                {
                    this._log.Error(sqlEx, "An exception occurred in " + sqlEx.Procedure + " and has the following message: " + sqlEx.Message + "/n");
                    throw;
                }
            }

            return newId;
        }

        #endregion Commit
    }
}
