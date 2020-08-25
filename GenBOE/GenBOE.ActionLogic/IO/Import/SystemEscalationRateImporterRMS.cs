// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;
    using static IES.Common.OfficeUtilities.ExcelUtilities;

    /// <summary>
    /// Responsible for Escalation Rates-specific Excel import.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SystemEscalationRateImporterRMS
    {
        #region Constants

        private static Logger _log = new Logger(typeof(SystemEscalationRateImporterRMS));

        // Individual column names
        private const string idColumn = "Id";
        private const string yearColumn = "Year";
        private const string rateColumn = "Escalation Rate";
        
        // Array of the columns that must be contained in the imported file
        private static readonly string[] requiredColumns = new string[] { idColumn, yearColumn, rateColumn };

        private static readonly string[] allColumns = new string[] { idColumn, yearColumn, rateColumn };

        // Array of the columns in the imported file that must contain values
        private static readonly string[] requiredValueColumns = new string[] { yearColumn, rateColumn };

        // Array of the columns in the imported file that must contain unique values
        private static readonly string[] uniqueValueColumns = new string[] { idColumn, yearColumn };

        #endregion Constants

        #region Public Functions

        /// <summary>
        /// Returns a collection of Escalation Rate ModelViews.
        /// </summary>
        /// <param name="excelFileStream">A file stream holding the uploaded data from the user</param>
        /// <param name="originalRates">The original Escalation Rates.</param>
        /// <returns>Collection of new Escalation Rate ModelViews objects.</returns>
        public static ICollection<EscalationRatesDTO> ImportFromExcelFile(Stream excelFileStream, ICollection<EscalationRatesDTO> originalRates)
        {
            try
            {
                // Open the document as read-only.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                {
                    // Get a collection of all rows in the file, filtering out rows that only have data in
                    // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                    // in an enumerable collection
                    ICollection<Dictionary<string, string>> allRows = GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, requiredColumns, allColumns, requiredValueColumns, uniqueValueColumns);

                    // Turn each row into a DTO object and return the collection
                    return CreateDTOsToReturn(allRows, originalRates);
                }
            }
            catch (FileFormatException)
            {
                _log.Error("Imported System Escalation Rates file was an incorrect format.");
                throw new NotExcelFileException();
            }
            catch (ColumnMissingException)
            {
                _log.Error("Imported System Escalation Rates file was missing a required column.");
                throw;
            }
            catch (CellValueMissingException)
            {
                _log.Error("Imported System Escalation Rates file was missing a required cell value.");
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
        /// <param name="originalRates">The original Escalation Rates</param>
        /// <returns>A collection of DTOs representing the newly imported values</returns>
        private static ICollection<EscalationRatesDTO> CreateDTOsToReturn(ICollection<Dictionary<string, string>> allRows, ICollection<EscalationRatesDTO> originalRates)
        {
            // Create the collection to return
            List<EscalationRatesDTO> toReturn = new List<EscalationRatesDTO>(originalRates);

            // first set all original rates to be deleted
            foreach (EscalationRatesDTO rate in originalRates)
            {
                rate.Updateable = UpdateType.Deleted;
            }

            // Set the ID counter. New objects must have IDs < 0 and mutiple elements submitted to
            // the loader must have different IDs. So, we'll go -1, -2, -3, etc.
            int currentID = -1;

            // For each Dictionary object (representing imported row data)
            foreach (Dictionary<string, string> row in allRows)
            {
                int id;
                bool isNewEscalationRate = false;

                // Check to see if the Id column is set
                if (!row.ContainsKey(idColumn) || !int.TryParse(row[idColumn], out id))
                {
                    id = currentID--;
                    isNewEscalationRate = true;
                }

                EscalationRatesDTO escalationRate = new EscalationRatesDTO();

                // check to see if the Id is valid
                if (isNewEscalationRate)
                {
                    escalationRate.EscalationRateID = id;
                    escalationRate.Id = id;

                    // Add the new Escalation Rate to the collection to be returned
                    toReturn.Add(escalationRate);
                }
                else
                {
                    escalationRate = originalRates.FirstOrDefault(o => o.EscalationRateID == id);
                    if (escalationRate == null)
                    {
                        throw new ValidationException(string.Format("The Id with value {0} was not found in the Database.  Please refresh the page to review these latest changes.", id));
                    }

                    // Change the original rate to be an upsert
                    escalationRate.Updateable = UpdateType.Upsert;
                }

                // Set the Year and Escalation Rate
                escalationRate.Year = int.Parse(row[yearColumn]);
                escalationRate.DevEscalation = decimal.Parse(row[rateColumn]) / 100m; // rates are input as percent%, but DTOs are saved as decimals so need to be divided by 100
            }

            // Return the collection of new DTOs
            return toReturn;
        }

        #endregion Private Functions
    }
}