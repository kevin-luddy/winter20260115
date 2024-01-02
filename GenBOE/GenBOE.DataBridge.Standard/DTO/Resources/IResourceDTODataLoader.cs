// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using IES.Standard;
    using System.Collections.Generic;
    using GenBOE.Dtos;

    public interface IResourceDTODataLoader
    {
        /// <summary>
        /// Global List Id
        /// </summary>
        int GlobalListID { get; }

        Dictionary<int, int> SaveSystemResources(ICollection<ResourceDTO> inSystemResources);
        Dictionary<int, int> SaveWorkspaceResources(WorkspaceDTO inWorkspace, ICollection<ResourceDTO> inWorkspaceResources);
        ICollection<ResourceDTO> GetByListId(int inResourceListID);
        ICollection<ResourceDTO> GetByIds(ICollection<int> inResourceIDs);

		/// <summary>
		/// Gets the Resource Names by an incoming list of Resource IDs.
		/// </summary>
		/// <param name="inResourceIDs">Resource IDs</param>
		/// <returns>Collection of Resource Names</returns>
		IList<string> GetResourceNamesByIds(IList<int> inResourceIDs);

		/// <summary>
		/// Get Resource By Resource Desc and List Id
		/// </summary>
		/// <param name="inResourceDescription">Resource Description</param>
		/// <param name="inResourceListID">List Id</param>
		/// <returns>Resource Dto</returns>
		ResourceDTO GetByDescriptionAndListId(string inResourceDescription, int inResourceListID);

        /// <summary>
        /// Get Resource By Resource Name and List Id
        /// </summary>
        /// <param name="inResourceName">Resource Name</param>
        /// <param name="inResourceListID">List Id</param>
        /// <returns>Resource Dto</returns>
        ResourceDTO GetByNameAndListId(string inResourceName, int inResourceListID);

        /// <summary>
        /// Is Resource Desc Unique
        /// </summary>
        /// <param name="resourceDesc">Resource Description</param>
        /// <param name="resourceListID">List Id</param>
        /// <param name="resourceID">Resource Id</param>
        /// <returns>Is resource description unique</returns>
        bool IsResourceDescriptionUnique(string resourceDesc, int resourceListID, int? resourceID);

        /// <summary>
        /// Is Resource Name Unique?
        /// </summary>
        /// <param name="resourceName">Resource Name</param>
        /// <param name="resourceListID">List Id</param>
        /// <param name="resourceID">Resource Id</param>
        /// <returns>Is resource name unique</returns>
        bool IsResourceNameUnique(string resourceName, int resourceListID, int? resourceID);

        /// <summary>
        /// Get Global Resources
        /// </summary>
        /// <returns>Global Resources</returns>
        ICollection<ResourceDTO> GetGlobalResources();

        /// <summary>
        /// Get Resource by Id
        /// </summary>
        /// <param name="inResourceID">Resource Id</param>
        /// <returns>Resource Dto</returns>
        ResourceDTO GetById(int inResourceID);

        /// <summary>
        /// Get resource IDs by resource List ID and element of cost type
        /// </summary>
        /// <param name="inResourceListID"></param>
        /// <param name="inElementOfCostType"></param>
        /// <returns></returns>
        ICollection<ResourceDTO> GetByListIdAndElementOfCost(int inResourceListID, ICollection<ElementOfCostType> inElementOfCostTypes);

        /// <summary>
        /// Get Resources By List Id and Element of Cost Type
        /// </summary>
        /// <param name="inResourceListID">List Id</param>
        /// <param name="inElementOfCostType">Element of Cost</param>
        /// <returns>Resource Dtos</returns>
        ICollection<ResourceDTO> GetByListIdAndElementOfCost(int inResourceListID, ElementOfCostType inElementOfCostType);
    }
}
