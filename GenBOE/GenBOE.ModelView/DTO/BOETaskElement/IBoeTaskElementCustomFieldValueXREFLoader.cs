// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// Interface for the BOETaskElementCustomFieldValueXREF
    /// </summary>
    public interface IBoeTaskElementCustomFieldValueXREFLoader : IBulkDataLoader<CustomFieldValueContainer>
    {
        /// <summary>
        /// Save BOE Task Element Custom Field Value Containers
        /// </summary>
        /// <param name="inBOETaskElementCustomFieldValueContainers"></param>
        void SaveBOETaskElementCustomFieldValueContainers(Collection<CustomFieldValueContainer> inBOETaskElementCustomFieldValueContainers, int inBoeTaskElementID);

        /// <summary>
        /// Save a single BOE Task Element Custom Field Value Container
        /// </summary>
        /// <param name="inCustomFieldValueContainer"></param>
        void SaveBOETaskElementCustomFieldValueContainer(CustomFieldValueContainer inCustomFieldValueContainer, int inBoeTaskElementID);
    }
}
