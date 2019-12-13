// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Used for importing new genBOE WBS elements from an Excel file
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResourcesImporter
    {
        #region Constants

        private Logger _log = new Logger(typeof(ResourcesImporter));

        // Individual column names
        private const string idColumn = "ID";
        private const string descriptionColumn = "Description";
        private const string segmentRegionColumn = "Segment/Region";
        private const string laborTypeColumn = "Labor Type";
        private const string rateTypeColumn = "Rate Type";
        private const string elementOfCostColumn = "Element of Cost";

        // Array of the columns that must be contained in the imported file
        private readonly string[] requiredColumns = new string[] {idColumn, descriptionColumn, segmentRegionColumn, laborTypeColumn, rateTypeColumn, elementOfCostColumn };

        // Array of the columns in the imported file that must contain values
        private readonly string[] requiredValueColumns = new string[] { idColumn, descriptionColumn, segmentRegionColumn, laborTypeColumn, rateTypeColumn, elementOfCostColumn };

        // Array of the columns in the imported file that must contain unique values
        private readonly string[] uniqueValueColumns = new string[] { idColumn, descriptionColumn };

        private IResourceDTODataLoader _ResourceLoader;

        #endregion Constants

        public ResourcesImporter(IResourceDTODataLoader inResourceLoader)
        {
            this._ResourceLoader = inResourceLoader;
        }

        #region Public Functions

        /// <summary>
        /// Returns a collection of Resource DTO objects that can be used to submit newly imported
        /// Resource elements into the genBOE DB.
        /// </summary>
        /// <param name="excelFileStream">A file stream holding the uploaded data from the user</param>
        /// <param name="listID">The ID of the list to associate these performing orgs with</param>
        /// <returns>Collection of new Resource DTO objects holding all of the new WBS elements.</returns>
        public Collection<ResourceDTO> ImportFromExcelFile(Stream excelFileStream, int listID)
        {
            try
            {
                // Open the document as read-only.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                {
                    // Get a collection of all rows in the file, filtering out rows that only have data in
                    // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                    // in an enumerable collection
                    ICollection<Dictionary<string, string>> allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, this.requiredColumns, this.requiredColumns, this.requiredValueColumns, this.uniqueValueColumns);

                    // Turn each row into a DTO object and return the collection
                    return this.CreateDTOsToReturn(allRows, listID);
                }
            }
            catch (FileFormatException)
            {
                this._log.Error("Imported Resources file was an incorrect format.");
                throw new NotExcelFileException();
            }
            catch (ColumnMissingException)
            {
                this._log.Error("Imported Resources file was missing a required column.");
                throw;
            }
            catch (CellValueMissingException)
            {
                this._log.Error("Imported Resources file was missing a required cell value.");
                throw;
            }
            catch (Exception ex)
            {
                this._log.Error(ex);
                throw;
            }
        }

        #endregion Public Functions

        #region Private Functions

        /// <summary>
        /// Converts a collection of Key/Value pair Dictionary objects into a collection of DTOs.
        /// </summary>
        /// <param name="allRows">Collection of Key/Value pair Dictionary objects representing imported rows.</param>
        /// <param name="listID">The ID of the list to associate these performing orgs with</param>
        /// <returns>A collection of DTOs representing the newly imported values</returns>
        private Collection<ResourceDTO> CreateDTOsToReturn(ICollection<Dictionary<string, string>> allRows, int listID)
        {
            // Create the collection to return
            Collection<ResourceDTO> toReturn = new Collection<ResourceDTO>();

            // Set the ID counter. New DTOs must have IDs < 0 and mutiple DTO elements submitted to
            // the loader must have different IDs. So, we'll go -1, -2, -3, etc.
            int currentResourceID = -1;

            ICollection<ResourceDTO> resourcesForListId = this._ResourceLoader.GetByListId(listID);

            // For each Dictionary object (representing imported row data)
            foreach (Dictionary<string, string> row in allRows)
            {
                // if the resource ID exists, then get the ID and the UpdateDate. the rest of the data can be gotten from the excel sheet
                ResourceDTO newResource = new ResourceDTO();

                if (row.ContainsKey(idColumn))
                {
                    // find out if the resource we're importing already exists given it's Name and List ID
                    // if it does, grab the resource ID. if not, its new
                    var res = resourcesForListId.FirstOrDefault(x => x.ResourceName == row[idColumn]);

                    if (res != null)
                    {
                        newResource.Id = res.Id;
                        newResource.UpdateDate = res.UpdateDate;
                    }
                    else
                    {
                        newResource.Id = currentResourceID--;
                    }
                }

                newResource.ResourceName = row[idColumn];
                newResource.ResourceDesc = row[descriptionColumn];
                newResource.SegRegion = row[segmentRegionColumn];
                newResource.LaborType = row[laborTypeColumn];
                newResource.Segment = SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST ? SegmentType.RMS : SegmentType.SSC;
                newResource.ElementOfCost = (row[elementOfCostColumn] == "LM Labor") ? ElementOfCostType.LMLabor : (ElementOfCostType) Enum.Parse(typeof(ElementOfCostType), row[elementOfCostColumn]);
                newResource.RateType = !row.ContainsKey(rateTypeColumn) ? RateType.NotSet : (RateType)Enum.Parse(typeof(RateType), row[rateTypeColumn]);
                newResource.Updateable = UpdateType.Upsert;

                // Add the new DTO to the collection to be returned
                toReturn.Add(newResource);
            }

            // Return the collection of new DTOs
            return toReturn;
        }

        #endregion Private Functions
    }
}
