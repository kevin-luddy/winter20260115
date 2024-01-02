// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Standard;

    public interface ICustomFieldValueDTODataLoader : IBulkDataLoader<CustomFieldValueDTO>
    {
        /// <summary>
        /// Get custom field value dtos by custom field id.
        /// </summary>
        /// <param name="inCustomFieldID">Custom field id.</param>
        /// <returns>Collection of custom field value ids</returns>
        ICollection<CustomFieldValueDTO> GetCustomFieldValueDTOsByCustomFieldID(int inCustomFieldID);

        /// <summary>
        /// Get custom field value dtos by custom field ids.
        /// </summary>
        /// <param name="inCustomFieldID">Custom field ids.</param>
        /// <returns>Collection of custom field value ids</returns>
        ICollection<CustomFieldValueDTO> GetCustomFieldValueDTOsByCustomFieldIds(ICollection<int> inCustomFieldIds);

        /// <summary>
        /// Refresh the In Use Flags for a workspace
        /// </summary>
        /// <param name="workspaceId">The id of the workspace to refresh.</param>
        void RefreshCustomFieldInUseByWorkspaceID(int workspaceId);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByTaskElementIDs(Collection<int> inTaskElementIDs);

        /// <summary>
        /// Gets a mapping of MOQ Type Table to Custom Field Value ID
        /// </summary>
        /// <param name="moqTypeTableIds">MOQ Type Table IDs</param>
        /// <returns>Mapping of MOQ Type Table to Custom Field Value ID</returns> 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByMoqTypeTableIds(ICollection<int> moqTypeTableIds);

        /// <summary>
        /// Gets a mapping of labor type to custom field value Id.
        /// </summary>
        /// <param name="inLaborTypeIDs">labor ids</param>
        /// <returns>Mapping of labor types to custom field value id</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByLaborTypeIDs(Collection<int> inLaborTypeIDs);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        /// <summary>
        /// Gets a mapping of travel element to custom field value Id.
        /// </summary>
        /// <param name="inTravelElementIDs">travel element Ids.</param>
        /// <returns>Mapping of travel element to custom field value Id.</returns>
        Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByTravelElementIDs(Collection<int> inTravelElementIDs);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        /// <summary>
        /// Gets a mapping of travel trip to custom field value Id.
        /// </summary>
        /// <param name="inTravelTripIDs">travel trip Ids.</param>
        /// <returns>Mapping of travel trip to custom field value Id.</returns>
        Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByTravelTripIDs(Collection<int> inTravelTripIDs);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByResourceIDs(Collection<int> inResourceIDs);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Dictionary<int, ICollection<KeyValuePair<int, int>>> GetLaborTypeCustomFieldValueIDsContainerIDsByTaskElementIDs(Collection<int> inTaskElementIDs);

        IList<CustomFieldContainerFieldValueMapping> GetBOECustomFieldContainerFieldValueMappings(int boeID);
        IList<CustomFieldContainerFieldValueMapping> GetTaskElementCustomFieldContainerFieldValueMappings(int taskElementID);
        IList<CustomFieldContainerFieldValueMapping> GetResourceCustomFieldContainerFieldValueMappings(int laborTypeID);
    }
}
