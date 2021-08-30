// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// Interface for the MoqTypeTableCustomFieldValueXREFLoader
    /// </summary>
    public interface IMoqTypeTableCustomFieldValueXREFLoader : IBulkDataLoader<CustomFieldValueContainer>
    {
        /// <summary>
        /// Save MOQ Type Table Custom Field Value Containers
        /// </summary>
        /// <param name="moqTypeTableCustomFieldValueContainers">MOQ Type Table Custom Field Value Containers to save</param>
        /// <param name="moqTypeTableId">MOQ Type Table ID</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        ICollection<int?> SaveMoqTypeTableCustomFieldValueContainers(ICollection<CustomFieldValueContainer> moqTypeTableCustomFieldValueContainers, int moqTypeTableId);

        /// <summary>
        /// Save a single MOQ Type Table Custom Field Value Container
        /// </summary>
        /// <param name="moqTypeTableCustomFieldValueContainer">MOQ Type Table Custom Field Value Container to save</param>
        /// <param name="moqTypeTableId">MOQ Type Table ID</param>
        int? SaveMoqTypeTableCustomFieldValueContainer(CustomFieldValueContainer moqTypeTableCustomFieldValueContainer, int moqTypeTableId);
    }
}
