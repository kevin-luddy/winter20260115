// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public interface IMaterialDTODataLoader
    {
        /// <summary>
        /// Save materials data
        /// </summary>
        /// <param name="inMaterialDTOs">Materials to save</param>
        void SaveMaterials(ICollection<MaterialDTO> inMaterialDTOs);

        /// <summary>
        /// Gets the number of materials for the specified Boe
        /// </summary>
        /// <param name="boeId">BOE Id</param>
        /// <returns>Number of materials</returns>
        int GetNumberOfMaterialsForBoeId(int inBoeID);

        /// <summary>
        /// Gets a Material by Id
        /// </summary>
        /// <param name="inMaterialID">Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Material for the id</returns>
        MaterialDTO GetById(int inMaterialID, bool includeRTEFields = false);

        /// <summary>
        /// Gets Materials by Boe Id
        /// </summary>
        /// <param name="boeIds">Boe Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Material Dtos</returns>
        Collection<MaterialDTO> GetByBoeIds(Collection<int> boeIds, bool includeRTEFields = false);

        /// <summary>
        /// Pulls RTE fields for the DTOs, and updates them as needed
        /// </summary>
        /// <param name="dtos">DTOs that whose RTE fields will be loaded, if they are null</param>
        void LoadRTEFields(ICollection<MaterialDTO> dtos);

        /// <summary>
        /// Deletes All Material elements for a BOE 
        /// </summary>
        /// <param name="boeId">BOE Id</param>
        void DeleteAllMaterialTaskElements(int boeId);
    }
}
