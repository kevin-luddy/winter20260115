// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web;
    using DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// By going through this factory, properties on the full objects can be mocked out in tests.
    /// </summary>
    public class FullObjectFactory : IFullObjectFactory
    {
        private IWorkspaceDTODataLoader workspaceLoader;
        private IUserDTODataLoader userLoader;
        public IBoeDTODataLoader BoeLoader { get; set; }
        private IClinDTODataLoader clinLoader;
        private IWbsDTODataLoader wbsLoader;
        private IMaterialDTODataLoader materialLoader;
        private IBoeTaskElementDTODataLoader taskElementLoader;
        private IOtherDirectCostDTODataLoader otherDirectCostLoader;
        private ITravelDTODataLoader travelLoader;
        private IBoeApproverResponseDTODataLoader approverResponseLoader;
        private ISecurityUserAuthorizationsDataLoader securityLoader;
        private ICustomFieldDTODataLoader customFieldLoader;

        #region Cache Setup

        /// <summary>
        /// Cache Object
        /// </summary>
        private MemoryCache cache;

        /// <summary>
        /// The number of seconds to store in cache
        /// </summary>
        private int permissionsSecondsToCache = 10;

        /// <summary>
        /// The number of seconds to store WS in cache - should be very small to prevent stale data from hanging around
        /// </summary>
        private int wsSecondsToCache = 5;

        /// <summary>
        /// The number of seconds to cache last access time for a workspace - 30 minutes
        /// </summary>
        private int lastAccessSecondsToCache = 60 * 30;

        /// <summary>
        /// Key for the cache
        /// </summary>
        private string cacheKeyFullWorkspaceByShortName = "WorkspaceByShortName_";

        /// <summary>
        /// Cache key for User's permissions
        /// </summary>
        private string cacheKeyUsersPermissions = "UsersPermissions_";

        /// <summary>
        /// The cache key last accessed workspace
        /// </summary>
        private string cacheKeyLastAccessed = "LastAccessed{0}::{1}";

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public FullObjectFactory(IWorkspaceDTODataLoader workspaceLoader, IBoeDTODataLoader boeLoader, IClinDTODataLoader clinLoader, 
            IWbsDTODataLoader wbsLoader, IMaterialDTODataLoader materialLoader,
            IBoeTaskElementDTODataLoader taskElementLoader, IOtherDirectCostDTODataLoader otherDirectCostLoader,
            ITravelDTODataLoader travelLoader, IBoeApproverResponseDTODataLoader approverResponseLoader, ISecurityUserAuthorizationsDataLoader securityLoader, ICustomFieldDTODataLoader customFieldLoader,
            IUserDTODataLoader userLoader)
        {
            this.workspaceLoader = workspaceLoader;
            this.BoeLoader = boeLoader;
            this.clinLoader = clinLoader;
            this.wbsLoader = wbsLoader;
            this.materialLoader = materialLoader;
            this.taskElementLoader = taskElementLoader;
            this.otherDirectCostLoader = otherDirectCostLoader;
            this.travelLoader = travelLoader;
            this.approverResponseLoader = approverResponseLoader;
            this.securityLoader = securityLoader;
            this.customFieldLoader = customFieldLoader;
            this.userLoader = userLoader;

            this.cache = new MemoryCache();
        }

        /// <summary>
        /// Clears the permissions cache.
        /// </summary>
        /// <param name="ntId">The nt identifier.</param>
        public void ClearPermissionsCache(string ntId)
        {
            string key = this.cacheKeyUsersPermissions + ntId;
            
            this.cache.Remove(key);
        }

        /// <summary>
        /// Gets permissions for the specified user
        /// </summary>
        /// <param name="ntid">User</param>
        /// <returns>Permissions for the user</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        public IReadOnlyCollection<SecurityPermissionsResponse> GetPermissionsForUser(string ntid)
        {
            Collection<SecurityPermissionsResponse> data = null;
            bool ignoreCache = HttpContext.Current.Items.Contains("IgnorePermissionsCache");
            string key = this.cacheKeyUsersPermissions + ntid;

            object fromCache = this.cache.GetData(key);

            if (fromCache == null || ignoreCache)
            {
                lock(string.Intern(key))
                {
                    if (ignoreCache || !this.cache.Contains(key))
                    {
                        this.cache.Remove(key);
                        fromCache = this.securityLoader.GetPermissionsForUser(ntid);

                        if (!ignoreCache && fromCache != null)
                        {
                            this.cache.Add(key, fromCache, permissionsSecondsToCache);
                        }
                    }
                }

                // This is needed to set fromCache because of the ignoreCache parameter
                if (!ignoreCache)
                {
                    fromCache = this.cache.GetData(key);
                }
            }
            data = fromCache as Collection<SecurityPermissionsResponse>;

            return data.ToList().AsReadOnly();
        }

        /// <summary>
        /// Clears the workspace cache.
        /// </summary>
        /// <param name="shortname">Short name of the workspace.</param>
        public void ClearWorkspaceCache(string shortname)
        {
            string key = this.cacheKeyFullWorkspaceByShortName + shortname;

            this.cache.Remove(key);
        }

        /// <summary>
        /// Creates a full workspace.
        /// </summary>
        /// <param name="shortname">shortname</param>
        /// <param name="ignoreAndClearCache">Should the cache be cleared out?</param>
        /// <returns>full workspace</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        public FullWorkspace CreateFullWorkspace(string shortname, bool ignoreAndClearCache = false)
        {
            WorkspaceDTO data = null;
            string key = this.cacheKeyFullWorkspaceByShortName + shortname;


            object fromCache = this.cache.GetData(key);

            if (fromCache == null || ignoreAndClearCache)
            {
                lock (string.Intern(key))
                {
                    if (ignoreAndClearCache || !this.cache.Contains(key))
                    {
                        this.cache.Remove(key);
                        fromCache = this.workspaceLoader.GetByShortname(shortname);

                        if (!ignoreAndClearCache && fromCache != null)
                        {
                            this.cache.Add(key, fromCache, wsSecondsToCache);
                        }
                    }
                }

                // This is needed to set fromCache because of the ignoreCache parameter
                if (!ignoreAndClearCache && fromCache == null)
                {
                    fromCache = this.cache.GetData(key);
                }
            }

            data = fromCache as WorkspaceDTO;

            if (data != null)
            {
                this.LogWorkspaceAccess(data.Id);
            }

            if (data != null && data.IsProjectMapWorkspace)
            {
                return new FullProjectMapWorkspace(data);
            }
            else
            {
                return new FullWorkspace(data);
            }
        }

        /// <summary>
        /// Creates the full project map workspace.
        /// </summary>
        /// <param name="shortname">The shortname.</param>
        /// <param name="ignoreAndClearCache">if set to <c>true</c> [ignore and clear cache].</param>
        /// <returns>A full project map workspace.</returns>
        public FullProjectMapWorkspace CreateFullProjectMapWorkspace(string shortname, bool ignoreAndClearCache = false)
        {
            FullProjectMapWorkspace workspace = this.CreateFullWorkspace(shortname, ignoreAndClearCache) as FullProjectMapWorkspace;

            if (workspace == null)
            {
                throw new ArgumentException($"Workspace name {shortname} is not a Project Map Workspace.");
            }

            this.LogWorkspaceAccess(workspace.Id);

            return workspace;
        }

        /// <summary>
        /// Creates a full workspace.
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <returns>full workspace</returns>
        public FullWorkspace CreateFullWorkspace(int workspaceId)
        {
            WorkspaceDTO workspace = this.workspaceLoader.GetById(workspaceId);

            return this.CreateFullWorkspace(workspace);
        }

        /// <summary>
        /// Creates a full workspace.
        /// </summary>
        /// <param name="workspace">workspace dto</param>
        /// <returns>full workspace</returns>
        public FullWorkspace CreateFullWorkspace(WorkspaceDTO workspace)
        {
            if (workspace != null)
            {
                this.LogWorkspaceAccess(workspace.Id);
            }

            if (workspace != null && workspace.IsProjectMapWorkspace)
            {
                return new FullProjectMapWorkspace(workspace);
            }
            else
            {
                return new FullWorkspace(workspace);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        private void LogWorkspaceAccess(int workspaceId)
        {
            try
            {
                UserDTO currentUser = this.userLoader.GetUserForActiveUser();
                string key = string.Format(this.cacheKeyLastAccessed, workspaceId, currentUser.UserID);

                object fromCache = this.cache.GetData(key);

                if (fromCache == null)
                {
                    lock (string.Intern(key))
                    {
                        if (!this.cache.Contains(key))
                        {
                            this.workspaceLoader.UpdateLastAccessed(workspaceId, currentUser.UserID);
                            this.cache.Add(key, key, this.lastAccessSecondsToCache);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // empty catch
            }
        }

        /// <summary>
        /// Creates the cloned project map workspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <returns></returns>
        public FullProjectMapWorkspace CreateClonedProjectMapWorkspace(FullWorkspace workspace)
        {
            return new FullProjectMapWorkspace(workspace as FullProjectMapWorkspace);
        }

        /// <summary>
        /// Creates a full boe.
        /// </summary>
        /// <param name="boeId">boe id</param>
        /// <returns>full boe</returns>
        public FullBoe CreateFullBoe(int boeId)
        {
            return new FullBoe(this.BoeLoader.GetByIds(new Collection<int> { boeId }).First());
        }

        /// <summary>
        /// Creates a full boe
        /// </summary>
        /// <param name="boe">boe dto</param>
        /// <returns>full boe</returns>
        public FullBoe CreateFullBoe(BoeDTO boe)
        {
            return new FullBoe(boe);
        }

        /// <summary>
        /// Creates an empty full boe.
        /// </summary>
        /// <param name="boe">boe dto</param>
        /// <returns>full boe</returns>
        public FullBoe CreateFullBoe()
        {
            return new FullBoe();
        }

        /// <summary>
        /// Creates a collection of Full Boes
        /// </summary>
        /// <param name="boeIds">Boe Ids</param>
        /// <returns>Collection of full boes</returns>
        public ICollection<FullBoe> CreateFullBoes(ICollection<int> boeIds)
        {
            List<FullBoe> boes = new List<FullBoe>();

            ICollection<BoeDTO> rawBoes = this.BoeLoader.GetByIds(boeIds);
            foreach (BoeDTO boe in rawBoes)
            {
                boes.Add(new FullBoe(boe));
            }

            return boes;
        }

        /// <summary>
        /// Creates a collection of Full Boes, and preloads RTE data into them
        /// </summary>
        /// <param name="boeIds">Boe Ids</param>
        /// <returns>Collection of full boes</returns>
        public ICollection<FullBoe> CreateFullBoesWithRteData(ICollection<int> boeIds)
        {
            List<FullBoe> boes = new List<FullBoe>();

            ICollection<BoeDTO> rawBoes = this.BoeLoader.GetByIds(boeIds, true);
            foreach (BoeDTO boe in rawBoes)
            {
                boes.Add(new FullBoe(boe));
            }

            return boes;
        }

        /// <summary>
        /// Creates Full Clin based on Id
        /// </summary>
        /// <param name="clinId">Clin Id</param>
        /// <returns>Full Clin</returns>
        public FullClin CreateFullClin(int clinId)
        {
            return new FullClin(this.clinLoader.GetById(clinId));
        }

        /// <summary>
        /// Creates Empty Full Clin
        /// </summary>
        /// <returns>Full Clin</returns>
        public FullClin CreateFullClin()
        {
            return new FullClin(new ClinDTO());
        }

        /// <summary>
        /// Creates Full Clin based on Clin Dto
        /// </summary>
        /// <param name="clin">Clin Dto</param>
        /// <returns>Full Clin</returns>
        public FullClin CreateFullClin(ClinDTO clin)
        {
            return new FullClin(clin);
        }

        /// <summary>
        /// Creates a collection of Full Clins based on Ids
        /// </summary>
        /// <param name="clinIds">Clin Ids</param>
        /// <returns>an iCollection of Full Clins</returns>
        public ICollection<FullClin> CreateFullClins(ICollection<int> clinIds)
        {
            List<FullClin> result = new List<FullClin>();
            ICollection<ClinDTO> rawClins = this.clinLoader.GetByIds(clinIds);

            foreach (ClinDTO aClin in rawClins)
            {
                result.Add(new FullClin(aClin));
            }

            return result;
        }

        /// <summary>
        /// Creates Full Wbs
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <returns>Full Wbs</returns>
        public FullWbs CreateFullWbs(int wbsId)
        {
            return new FullWbs(this.wbsLoader.GetById(wbsId));
        }

        /// <summary>
        /// Creates Full Wbs
        /// </summary>
        /// <param name="wbs">Wbs</param>
        /// <returns>Full Wbs</returns>
        public FullWbs CreateFullWbs(WbsDTO wbs)
        {
            return new FullWbs(wbs);
        }

        /// <summary>
        /// Creates an empty Full Wbs
        /// </summary>
        /// <returns>Full Wbs</returns>
        public FullWbs CreateFullWbs()
        {
            return new FullWbs();
        }

        /// <summary>
        /// Creates Full Wbs objects based on Ids
        /// </summary>
        /// <param name="wbsIds">Wbs Ids</param>
        /// <returns>Corresponding Data</returns>
        public ICollection<FullWbs> CreateFullWbses(ICollection<int> wbsIds)
        {
            List<FullWbs> result = new List<FullWbs>();
            ICollection<WbsDTO> wbsElements = this.wbsLoader.GetByIds(wbsIds);

            foreach (WbsDTO aWbs in wbsElements)
            {
                result.Add(new FullWbs(aWbs));
            }

            return result;

        }

        /// <summary>
        /// Gets Material element from an Id.
        /// </summary>
        /// <param name="materialId">Material Id</param>
        /// <returns>Full material element.</returns>
        public MaterialDTO GetMaterialById(int materialId)
        {
            return this.materialLoader.GetById(materialId);
        }

        /// <summary>
        /// Creates Full Task element from an Id.
        /// </summary>
        /// <param name="taskElementId">Task Element Id</param>
        /// <returns>Full task element.</returns>
        public BoeTaskElementDTO CreateTaskElement(int taskElementId, int hoursPrecision, int costPrecision)
        {
            return this.taskElementLoader.GetById(taskElementId, hoursPrecision, costPrecision);
        }

        /// <summary>
        /// Creates Travel dto from an Id.
        /// </summary>
        /// <param name="travelId">Travel Id</param>
        /// <returns>Travel Dto.</returns>
        public TravelDTO CreateTravel(int travelId)
        {
            return this.travelLoader.GetById(travelId);
        }

        /// <summary>
        /// Creates an Other Direct Cost from an Id.
        /// </summary>
        /// <param name="otherDirectCostId">Other Direct Cost Id</param>
        /// <returns>Other Direct Cost Dto.</returns>
        public OtherDirectCostDTO CreateOtherDirectCost(int otherDirectCostId)
        {
            return this.otherDirectCostLoader.GetById(otherDirectCostId);
        }

        /// <summary>
        /// Creates an Approver Response from an Id.
        /// </summary>
        /// <param name="approverResponseId">Approver response Id.</param>
        /// <returns>Approver Response DTO.</returns>
        public BoeApproverResponseDTO CreateApproverResponse(int approverResponseId)
        {
            return this.approverResponseLoader.GetById(approverResponseId);
        }

        /// <summary>
        /// Creates a Custom Field from an Id.
        /// </summary>
        /// <param name="customFieldId">Custom Field Id.</param>
        /// <returns>Custom Field DTO.</returns>
        public CustomFieldDTO CreateCustomField(int customFieldId)
        {
            return this.customFieldLoader.GetById(customFieldId);
        }

        /// <summary>
        /// Create a collect of Custom Field by Ids.
        /// </summary>
        /// <param name="customFieldIds">Custom Field Ids.</param>
        /// <returns>Custom Field DTO collection.</returns>
        public ICollection<CustomFieldDTO> CreateCustomFields(ICollection<int> customFieldIds)
        {
            return this.customFieldLoader.GetByIds(customFieldIds);
        }
    }
}
