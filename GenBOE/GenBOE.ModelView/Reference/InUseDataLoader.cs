// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using IES.Common;
using System.Linq;
using GenBOE.Models;
using System.Collections.Generic;

namespace GenBOE.DataBridge.Reference
{
    /// <summary>
    /// The In Use Data loader identifies which data types/DTOS are in use
    /// </summary>
    public class InUseDataLoader : IInUseDataLoader
    {
        // log file
        private Logger _log = new Logger(typeof(InUseDataLoader));

        /// <summary>
        /// default constructor
        /// </summary>
        public InUseDataLoader() { }

        /// <summary>
        /// generic in use method
        /// </summary>
        /// <param name="inUseDataType">data type/DTO</param>
        /// <param name="inID">data type/DTO ID</param>
        /// <returns></returns>
        virtual public bool GetInUse(InUseDataType inUseDataType, int inID, int? inParentID)
        {
            bool inUse = false;
            switch (inUseDataType)
            {
                case InUseDataType.WorkspaceResources:
                    inUse = GetWorkspaceResourceInUse(inID, inParentID.Value);
                    break;
                case InUseDataType.SystemResources:
                case InUseDataType.SystemResourceRate: // the rate is determined in use if the system resource it's linked to is in use
                    inUse = GetSystemResourceInUse(inID);
                    break;
                default:
                    break;
            }

            return inUse;
        }

        /// <summary>
        /// determine if the resource is in use
        /// </summary>
        /// <param name="inID">resource ID</param>
        /// <param name="inResourceListID">resource list ID</param>
        /// <returns></returns>
        [DbQuery]
        virtual public bool GetWorkspaceResourceInUse(int inID, int inResourceListID)
        {
            bool inUse = false;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                // call getWorkspaceResourceInUseFlagByResourceID SP to determine if workspace resource is in use
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var result = gbe.getWorkspaceResourceInUseFlagByResourceID(inID, inResourceListID);
                    var a = (from r in result
                             select r).First();

                    if (a == 1)
                    {
                        inUse = true;
                    }
                }
            }
            
            return inUse;
        }

        /// <summary>
        /// Get system resource in use
        /// </summary>
        /// <param name="inID">resource ID</param>
        /// <returns>true if in use, false if not</returns>
        [DbQuery]
        virtual public bool GetSystemResourceInUse(int inID)
        {
            bool inUse = false;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                // call getResourceInUseFlagBySystemResourceID SP to determine if system resource is in use
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var result = gbe.getResourceInUseFlagBySystemResourceID(inID);
                    var a = (from r in result
                             select r).FirstOrDefault();

                    if (a == 1)
                    {
                        inUse = true;
                    }
                }
            }

            return inUse;
        }

        /// <summary>
        /// Get resource IDs that are in use given a resource list id 
        /// </summary>
        /// <param name="inResourceListID">resource list id</param>
        /// <returns>collection of in use resource ids</returns>
        [DbQuery]
        virtual public HashSet<int> GetWorkspaceResourceIDsInUseByListID(int inResourceListID)
        {
            HashSet<int> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.Database.CommandTimeout = 450;

                    var ResourceIDsList = gbe.getResourceInUseFlagByResourceListID(inResourceListID).Select(x => x.Value);

                    toReturn = new HashSet<int>(from x in ResourceIDsList select x);

                }
            }

            return toReturn;

        }


        /// <summary>
        /// Get resource IDs that are in use given a resource list id 
        /// </summary>
        /// <param name="inResourceListID">resource list id</param>
        /// <returns>collection of in use resource ids</returns>
        virtual public HashSet<int> GetWorkspaceResourceInUseByMultipleResources(ICollection<int> inMultipleResourceIDs, int inResourceListID)
        {
            HashSet<int> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                if (inMultipleResourceIDs != null)
                {
                    if (inMultipleResourceIDs.Any())
                    {
                        string strIDs = string.Join(",", inMultipleResourceIDs);

                        using (GenBoeEntities gbe = new GenBoeEntities())
                        {
                            gbe.Database.CommandTimeout = 450;

                            IEnumerable<int?> sprocResult = gbe.getWorkspaceResourceInUseFlagByMultipleResources(strIDs, inResourceListID);

                            toReturn = new HashSet<int>(from x in sprocResult select x.Value);
                        }
                    }
                }
            }

            return toReturn;
        }


        /// <summary>
        ///  Get resource IDs that are associated with the system/global list id of 1
        /// </summary>
        /// <returns>collection of in use system resource IDs </returns>
        [DbQuery]
        virtual public HashSet<int> GetSystemResourceIDsInUse()
        {
            HashSet<int> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var ResourceIDsList = gbe.getSystemResourceInUseFlag().Select(x => x.Value);

                    toReturn = new HashSet<int>(from x in ResourceIDsList select x);
                }
            }

            return toReturn;

        }

        /// <summary>
        /// Get performing org IDs that are in use given a performing org list id 
        /// </summary>
        /// <param name="inPerfOrgListID">performing org list id</param>
        /// <returns>collection of in use performing org ids</returns>
        [DbQuery]
        virtual public HashSet<int> GetWorkspacePerfOrgIDsInUseByPerfOrgListID(int inPerfOrgListID)
        {
            HashSet<int> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var PerfOrgIDsList = gbe.getPerformingOrganizationInUseFlagByPerformingOrganizationListID(inPerfOrgListID).Select(x => x.Value);

                    toReturn = new HashSet<int>(from x in PerfOrgIDsList select x);
                }
            }

            return toReturn;

        }
  
    }
}