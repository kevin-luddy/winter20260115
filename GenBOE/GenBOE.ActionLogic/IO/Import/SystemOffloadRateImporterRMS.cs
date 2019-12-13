// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using Common;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;
    using static IES.Common.OfficeUtilities.ExcelUtilities;

    /// <summary>
    /// Responsible for Offload Rates-specific Excel import.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SystemOffloadRateImporterRMS
    {
        #region Constants

        /// <summary>
        /// Array of the columns that must be contained in the imported file.
        /// </summary>
        private static readonly string[] RequiredColumns = new string[] { IdColumn, YearColumn, ImportExportConstants.RESOURCE_COLUMN_HEADER, PerformingOrgColumn, ImportExportConstants.SUB_RESOURCE_COLUMN_HEADER,  PercentColumn, HourlyRateColumn};

        /// <summary>
        /// All columns allowed in the import.
        /// </summary>
        private static readonly string[] AllColumns = new string[] { IdColumn, YearColumn, ImportExportConstants.RESOURCE_COLUMN_HEADER, PerformingOrgColumn, ImportExportConstants.SUB_RESOURCE_COLUMN_HEADER,  PercentColumn, HourlyRateColumn };

        /// <summary>
        /// Array of the columns in the imported file that must contain values.
        /// </summary>
        private static readonly string[] RequiredValueColumns = new string[] { YearColumn, ImportExportConstants.RESOURCE_COLUMN_HEADER, PerformingOrgColumn, ImportExportConstants.SUB_RESOURCE_COLUMN_HEADER,  PercentColumn, HourlyRateColumn };

        /// <summary>
        /// Array of the columns in the imported file that must contain unique values.
        /// </summary>
        private static readonly string[] UniqueValueColumns = new string[] { IdColumn };

        /// <summary>
        /// The logger for the class.
        /// </summary>
        private static Logger logger = new Logger(typeof(SystemOffloadRateImporterRMS));

        /// <summary>
        /// The identifier column header.
        /// </summary>
        private const string IdColumn = "Id";

        /// <summary>
        /// The year column header.
        /// </summary>
        private const string YearColumn = "Year";

        /// <summary>
        /// The performing org column header.
        /// </summary>
        private const string PerformingOrgColumn = "Performing Org";

        /// <summary>
        /// The percent column header.
        /// </summary>
        private const string PercentColumn = "Percent to Offload";

        /// <summary>
        /// The hourly rate column header.
        /// </summary>
        private const string HourlyRateColumn = "Hourly Rate";

        #endregion Constants

        #region Public Functions

        /// <summary>
        /// Returns a collection of Offload Rate ModelViews.
        /// </summary>
        /// <param name="excelFileStream">A file stream holding the uploaded data from the user</param>
        /// <param name="originalRates">The original Offload Rates.</param>
        /// <returns>Collection of new Offload Rate ModelViews objects.</returns>
        public static ICollection<OffloadRatesDTO> ImportFromExcelFile(Stream excelFileStream, ICollection<OffloadRatesDTO> originalRates)
        {
            try
            {
                // Open the document as read-only.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                {
                    // Get a collection of all rows in the file, filtering out rows that only have data in
                    // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                    // in an enumerable collection
                    ICollection<Dictionary<string, string>> allRows = GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, RequiredColumns, AllColumns, RequiredValueColumns, UniqueValueColumns);

                    // Turn each row into a DTO object and return the collection
                    return CreateDTOsToReturn(allRows, originalRates);
                }
            }
            catch (FileFormatException)
            {
                logger.Error("Imported System Offload Rates file was an incorrect format.");
                throw new NotExcelFileException();
            }
            catch (ColumnMissingException)
            {
                logger.Error("Imported System Offload Rates file was missing a required column.");
                throw;
            }
            catch (CellValueMissingException)
            {
                logger.Error("Imported System Offload Rates file was missing a required cell value.");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        #endregion Public Functions

        #region Private Functions

        /// <summary>
        /// Converts a collection of Key/Value pair Dictionary objects into a collection of DTOs.
        /// </summary>
        /// <param name="allRows">Collection of Key/Value pair Dictionary objects representing imported rows.</param>
        /// <param name="originalRates">The original Offload Rates</param>
        /// <returns>A collection of DTOs representing the newly imported values</returns>
        private static ICollection<OffloadRatesDTO> CreateDTOsToReturn(ICollection<Dictionary<string, string>> allRows, ICollection<OffloadRatesDTO> originalRates)
        {
            // Create the collection to return
            List<OffloadRatesDTO> toReturn = new List<OffloadRatesDTO>(originalRates);

            // first set all original rates to be deleted
            foreach (OffloadRatesDTO rate in originalRates)
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
                bool isNewOffloadRate = false;

                // Check to see if the Id column is set
                if (!row.ContainsKey(IdColumn) || !int.TryParse(row[IdColumn], out id))
                {
                    id = currentID--;
                    isNewOffloadRate = true;
                }

                OffloadRatesDTO offloadRate = new OffloadRatesDTO();

                // check to see if the Id is valid
                if (isNewOffloadRate)
                {
                    offloadRate.Id = id;
                    offloadRate.Id = id;

                    // Add the new Offload Rate to the collection to be returned
                    toReturn.Add(offloadRate);
                }
                else
                {
                    offloadRate = originalRates.FirstOrDefault(o => o.Id == id);
                    if (offloadRate == null)
                    {
                        throw new ValidationException(string.Format("The Id with value {0} was not found in the Database.  Please refresh the page to review these latest changes.", id));
                    }

                    // Change the original rate to be an upsert
                    offloadRate.Updateable = UpdateType.Upsert;
                }

                // Set the Year and other Rate Properties
                offloadRate.Year = int.Parse(row[YearColumn]);
                offloadRate.Resource = row[ImportExportConstants.RESOURCE_COLUMN_HEADER];
                offloadRate.PerformingOrg = row[PerformingOrgColumn];
                offloadRate.SubResource = row[ImportExportConstants.SUB_RESOURCE_COLUMN_HEADER];
                offloadRate.Percent = decimal.Parse(row[PercentColumn]);
                offloadRate.HourlyRate = decimal.Parse(row[HourlyRateColumn]);
            }

            // Return the collection of new DTOs
            return toReturn;
        }

        #endregion Private Functions
    }
}