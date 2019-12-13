// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common;

    public interface ITravelTripTaskElementCustomFieldValueXREFLoader : IDataLoader<CustomFieldValueContainer>
    {
        /// <summary>
        /// Save BOE Travel Element Custom Field Value Containers
        /// </summary>
        /// <param name="inBOETravelElementCustomFieldValueContainers">custom field values </param>
        /// <param name="inBoeTravelElementID">boe element id</param>
        void SaveTravelTripTaskElementCustomFieldValueContainers(Collection<CustomFieldValueContainer> inBOETravelElementCustomFieldValueContainers, int inBoeTravelElementID);

        /// <summary>
        /// Save a single BOE Travel Element Custom Field Value Container
        /// </summary>
        /// <param name="inCustomFieldValueContainer">custom field values</param>
        /// <param name="inBoeTravelElementID">boe element id</param>
        void SaveTravelTripTaskElementCustomFieldValueContainer(CustomFieldValueContainer inCustomFieldValueContainer, int inBoeTravelElementID);
    }
}
