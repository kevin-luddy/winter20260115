// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    
    /// <summary>
    /// BOEImportMerge class is responsible for extracting BoeDTO objects from a WorkofflineImport
    /// object, then retrieving the corresponding record from the database and
    /// merging the two records together
    /// </summary>
    public class BOEImportMerge 
    {
        private IBoeDTODataLoader boeLoader;
        private ICustomFieldValueDTODataLoader _customFieldValueLoader;

        public BOEImportMerge(IBoeDTODataLoader boeLoader, ICustomFieldValueDTODataLoader customFieldValueLoader)
        {
            this.boeLoader = boeLoader;
            this._customFieldValueLoader = customFieldValueLoader;
        }

        /// <summary>
        /// Extract a BoeDTO object from the provided WorkofflineImport object
        /// </summary>
        /// <param name="importData"></param>
        /// <returns></returns>
        public Collection<BoeDTO> ExtractValidBoesFromOfflineImport(WorkofflineImport importData)
        {
            Collection<BoeDTO> extractedBoes = new Collection<BoeDTO>();
            if (importData == null || importData.ImportedBoes.Count == 0)
            {
                return extractedBoes;
            }

            foreach(WorkofflineImportedBoe inBoe in importData.ImportedBoes)
            {
                if (inBoe != null)
                {
                    inBoe.CloneImportedCustomFieldsToContainer();
                    extractedBoes.Add(inBoe);
                }
            }

            return extractedBoes;
        }

        /// <summary>
        /// Merge the BoeDTO parameter with the saved Boe record from database.
        /// </summary>
        /// <param name="importedBoe"></param>
        /// <returns></returns>
        public BoeDTO MergeBoeWithData(BoeDTO importedBoe)
        {
            if (importedBoe == null)
            {
                return null;
            }

            BoeDTO originalDataToUpdate = this.boeLoader.GetById(importedBoe.Id);
            
            if(originalDataToUpdate == null)
            {
                throw new ArgumentException("BOE ID does not exist");
            }
            originalDataToUpdate.Updateable = UpdateType.Upsert;

            //BOE Start and End Dates are read-only via import. They are not to be overriden here

            if (importedBoe.Description != null)
            {
                originalDataToUpdate.Description = importedBoe.Description;
            }
            if (importedBoe.DataSource != null)
            {
                originalDataToUpdate.DataSource = importedBoe.DataSource;
            }
            if (importedBoe.Title != null)
            {
                originalDataToUpdate.Title = importedBoe.Title;
            }
            
            //State is never changed.

            IList<CustomFieldContainerFieldValueMapping> boeCustomFields = this._customFieldValueLoader.GetBOECustomFieldContainerFieldValueMappings(importedBoe.Id);
            originalDataToUpdate.CustomFieldValueContainers.Merge(importedBoe.CustomFieldValueContainers, boeCustomFields);
            
            return originalDataToUpdate;
        }
    }
}
