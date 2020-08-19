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
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Used for importing new genBOE custom field elements from an Excel file
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CustomFieldImporter
    {
        #region Constants

        private static Logger _log = new Logger(typeof(CustomFieldImporter));

        // Individual column names
        private const string idColumn = "ID";
        private const string descriptionColumn = "Description";

        // Array of the columns that must be contained in the imported file
        private static string[] requiredColumns = new string[] { idColumn, descriptionColumn };

        // Array of the columns in the imported file that must contain values
        private static string[] requiredValueColumns = new string[] { idColumn, descriptionColumn };

        // Array of the columns in the imported file that must contain unique values
        private static string[] uniqueValueColumns = new string[] { idColumn };

        /// <summary>
        /// An array of columns which should be treated as text only. In our case, it's both columns
        /// </summary>
        private static string[] textOnlyValueColumns = new string[] { idColumn, descriptionColumn };

        #endregion Constants

        #region Public Functions

        /// <summary>
        /// Returns a collection of Custom Field DTO objects that can be used to submit newly imported
        /// Custom Field elements into the genBOE DB.
        /// </summary>
        /// <param name="excelFileStream">A file stream holding the uploaded data from the user</param>
        /// <param name="listID">The ID of the list to associate these custom fields with</param>
        /// <returns>Collection of new Performing Org DTO objects holding all of the new Custom Field elements.</returns>
        public Collection<CustomFieldValueDTO> ImportFromExcelFile(Stream excelFileStream, int listID)
        {
            try
            {
                // Open the document as read-only.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                {
                    // Get a collection of all rows in the file, filtering out rows that only have data in
                    // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                    // in an enumerable collection
                    ICollection<Dictionary<string, string>> allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, requiredColumns, requiredColumns, requiredValueColumns, uniqueValueColumns, textOnlyValueColumns);

                    // Turn each row into a DTO object and return the collection
                    return this.CreateDTOsToReturn(allRows, listID);
                }
            }
            catch (FileFormatException)
            {
                _log.Error("Imported Custom Field file was an incorrect format.");
                throw new NotExcelFileException();
            }
            catch (ColumnMissingException)
            {
                _log.Error("Imported Custom Field file was missing a required column.");
                throw;
            }
            catch (CellValueMissingException)
            {
                _log.Error("Imported Custom Field file was missing a required cell value.");
                throw;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
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
        private Collection<CustomFieldValueDTO> CreateDTOsToReturn(ICollection<Dictionary<string, string>> allRows, int listID)
        {
            // Create the collection to return
            Collection<CustomFieldValueDTO> toReturn = new Collection<CustomFieldValueDTO>();

            // Set the ID counter. New DTO objects must have IDs < 0 and mutiple DTO elements submitted to
            // the loader must have different IDs. So, we'll go -1, -2, -3, etc.
            int currentPerformingOrgID = -1;

            // For each Dictionary object (representing imported row data)
            foreach (Dictionary<string, string> row in allRows)
            {
                // Create a new DTO object and set it's values using the imported row data
                CustomFieldValueDTO newCustomFieldValue = new CustomFieldValueDTO();
                newCustomFieldValue.Id = currentPerformingOrgID--;
                newCustomFieldValue.CustomFieldValueID = currentPerformingOrgID--;
                newCustomFieldValue.CustomFieldID = listID;

                // cell value name & description should not contain any new lines
                newCustomFieldValue.CustomFieldValueName = row[idColumn].Replace("\n", " ");
                newCustomFieldValue.CustomFieldValueDescription = row[descriptionColumn].Replace("\n", " ");

                newCustomFieldValue.CustomFieldValueInUseFlag = false;
                newCustomFieldValue.UpdateDate = DateTime.Now;
                newCustomFieldValue.Updateable = UpdateType.Upsert;

                // Add the new DTO to the collection to be returned
                toReturn.Add(newCustomFieldValue);
            }

            // Return the collection of new DTOs
            return toReturn;
        }

        #endregion Private Functions
    }
}