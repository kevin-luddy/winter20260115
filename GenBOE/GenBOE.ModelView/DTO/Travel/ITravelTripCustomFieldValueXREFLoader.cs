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

    public interface ITravelTripCustomFieldValueXREFLoader : IDataLoader<CustomFieldValueContainer>
    {
        /// <summary>
        /// Save BOE Travel trip Custom Field Value Containers
        /// </summary>
        /// <param name="inBOETravelTripCustomFieldValueContainers">Collection of custom fields for trips</param>
        /// <param name="inBoeTravelTripID">travel element id</param>
        void SaveTravelTripCustomFieldValueContainers(Collection<CustomFieldValueContainer> inBOETravelTripCustomFieldValueContainers, int inBoeTravelTripID);

        /// <summary>
        /// Save a single BOE Travel Trip Custom Field Value Container
        /// </summary>
        /// <param name="inCustomFieldValueContainer">Custom field for trips</param>
        /// <param name="inBoeTravelTripID">travel element id</param>
         void SaveTravelTripFieldValueContainer(CustomFieldValueContainer inCustomFieldValueContainer, int inBoeTravelTripID);

        /// <summary>
        /// Returns CustomFieldID for a given CustomFieldValueID for use in validating customFieldValueContainers
        /// </summary>
        /// <param name="CustomFieldValueID"></param>
        /// <returns>CustomFieldID</returns>
        int GetCustomFieldID(int CustomFieldValueID);
    }
}
