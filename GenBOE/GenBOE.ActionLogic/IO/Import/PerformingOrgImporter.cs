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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Used for importing new genBOE WBS elements from an Excel file
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PerformingOrgImporter
    {
        #region Constants

        private Logger _log = new Logger(typeof(PerformingOrgImporter));

        // Individual column names
        private const string idColumn = "ID";
        private const string descriptionColumn = "Description";

        // Array of the columns that must be contained in the imported file
        private readonly string[] requiredColumns = new string[] { idColumn, descriptionColumn };

        // Array of the columns in the imported file that must contain values
        private readonly string[] requiredValueColumns = new string[] { idColumn, descriptionColumn };

        // Array of the columns in the imported file that must contain unique values
        private readonly string[] uniqueValueColumns = new string[] { idColumn };

        #endregion Constants

        #region Public Functions
        
        /// <summary>
        /// Returns a collection of Performing Org DTO objects that can be used to submit newly imported
        /// Performing Org elements into the genBOE DB.
        /// </summary>
        /// <param name="excelFileStream">A file stream holding the uploaded data from the user</param>
        /// <returns>Collection of new Performing Org DTO objects holding all of the new Performing Org elements.</returns>
        public Collection<PerformingOrgDTO> ImportFromExcelFile(Stream excelFileStream)
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
                    return this.CreateDTOsToReturn(allRows);
                }
            }
            catch (FileFormatException)
            {
                this._log.Error("Imported Performing Org file was an incorrect format.");
                throw new NotExcelFileException();
            }
            catch (ColumnMissingException)
            {
                this._log.Error("Imported Performing Org file was missing a required column.");
                throw;
            }
            catch (CellValueMissingException)
            {
                this._log.Error("Imported Performing Org file was missing a required cell value.");
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
        /// <returns>A collection of DTOs representing the newly imported values</returns>
        private Collection<PerformingOrgDTO> CreateDTOsToReturn(ICollection<Dictionary<string, string>> allRows)
        {
            // Create the collection to return
            Collection<PerformingOrgDTO> toReturn = new Collection<PerformingOrgDTO>();

            // Set the ID counter. New DTO objects must have IDs < 0 and mutiple DTO elements submitted to
            // the loader must have different IDs. So, we'll go -1, -2, -3, etc.
            int currentPerformingOrgID = -1;

            // For each Dictionary object (representing imported row data)
            foreach (Dictionary<string, string> row in allRows)
            {
                // Create a new DTO object and set it's values using the imported row data
                PerformingOrgDTO newPerformingOrg = new PerformingOrgDTO();
                newPerformingOrg.Id = currentPerformingOrgID--;
                newPerformingOrg.PerformingOrgName = row[idColumn];
                newPerformingOrg.PerformingOrgDesc = row[descriptionColumn];
                newPerformingOrg.Updateable = UpdateType.Upsert;

                // Add the new DTO to the collection to be returned
                toReturn.Add(newPerformingOrg);
            }

            // Return the collection of new DTOs
            return toReturn;
        }

        #endregion Private Functions
    }
}