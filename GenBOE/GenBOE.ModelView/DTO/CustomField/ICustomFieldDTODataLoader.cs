// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Common;

    public interface ICustomFieldDTODataLoader : IDataLoader<CustomFieldDTO>
    {
        /// <summary>
        /// Gets custom fields by workspace Id.
        /// </summary>
        /// <param name="inWorkspaceID">Workspace Id.</param>
        /// <returns>Custom Field Dtos by workspace Id.</returns>
        ICollection<CustomFieldDTO> GetByWorkspaceId(int inWorkspaceID);

        /// <summary>
        /// Get all custom field values assigned to the given BOEs
        /// </summary>
        /// <param name="boeID">The BOE</param>
        /// <returns>Dictionary of custom field values for the BOEs</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> GetAllAssignedBOECustomFieldValuesForBoeIds(ICollection<int> boeIds);
    }
}
