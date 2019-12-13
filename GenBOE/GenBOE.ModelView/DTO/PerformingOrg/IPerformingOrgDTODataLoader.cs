using System.Collections.ObjectModel;
using System.Collections.Generic;
using GenBOE.Dtos;

namespace GenBOE.DataBridge.DTO
{
    public interface IPerformingOrgDTODataLoader
    {
        Dictionary<int, int> SaveSystemPerformingOrgs(Collection<PerformingOrgDTO> inPerformingOrgs);
        Dictionary<int, int> SaveWorkspacePerformingOrgs(Collection<PerformingOrgDTO> inPerformingOrgs, int inWorkspacePerfOrgListID);

        PerformingOrgDTO GetById(int inPerfOrgID);

        /// <summary>
        /// Get the performing organizations by performing org list ID
        /// </summary>
        /// <param name="listId">performing organization list ID</param>
        /// <returns>collection of Performing Organizations</returns>
        Collection<PerformingOrgDTO> GetByListId(int listId);

        /// <summary>
        /// Get a collection of performing organization by ids
        /// </summary>
        /// <param name="ids">Perf Org Ids</param>
        /// <returns>perf org data</returns>
        Collection<PerformingOrgDTO> GetByIds(ICollection<int> ids);

        /// <summary>
        /// Get all performing orgs associated with the given performing org list ID & search term
        /// </summary>
        /// <param name="listId">perf org list ID</param>
        /// <param name="searchTerm">search term</param>
        /// <returns>performing org IDs</returns>
        Collection<PerformingOrgDTO> GetByListIdAndPartialNameOrDescription(int listId, string searchTerm);

        /// <summary>
        /// Get all performing orgs associated with the given performing org list ID & name
        /// </summary>
        /// <param name="listId">perf org list ID</param>
        /// <param name="name">name</param>
        /// <returns>performing org IDs</returns>
        PerformingOrgDTO GetByListIdAndName(int listId, string name);

        /// <summary>
        /// Get the GLOBAL performing organizations
        /// </summary>
        /// <returns>collection of Performing Organizations</returns>
        Collection<PerformingOrgDTO> GetGlobalPerformingOrgs();
    }
}
