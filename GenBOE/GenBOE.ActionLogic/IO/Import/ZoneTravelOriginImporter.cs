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
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Responsible for Zone Travel Origins-specific Excel import.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ZoneTravelOriginImporter
    {
        #region Constants

        private static Logger _log = new Logger(typeof(ZoneTravelOriginImporter));

        // Individual column names
        private const string ID_COLUMN = "OriginId";
        private const string ORIGIN_COLUMN = "Origin";
        private const string SITE_COLUMN = "Site";
        private const string PRZ1_COLUMN = "Zone 1 Per Diem";
        private const string PRZ2_COLUMN = "Zone 2 Per Diem";
        private const string PRZ3_COLUMN = "Zone 3 Per Diem";
        private const string PRZ4_COLUMN = "Zone 4 Per Diem";
        private const string PRZ5_COLUMN = "Zone 5 Per Diem";
        private const string PRZ6_COLUMN = "Zone 6 Per Diem";

        private const string TRZ1_COLUMN = "Zone 1 Airfare";
        private const string TRZ2_COLUMN = "Zone 2 Airfare";
        private const string TRZ3_COLUMN = "Zone 3 Airfare";
        private const string TRZ4_COLUMN = "Zone 4 Airfare";
        private const string TRZ5_COLUMN = "Zone 5 Airfare";
        private const string TRZ6_COLUMN = "Zone 6 Airfare";

        private const string NO_RATE = "NO-RATE";

        // Array of the columns that must be contained in the imported file
        private static readonly string[] requiredColumns = new string[] { ID_COLUMN, ORIGIN_COLUMN, SITE_COLUMN };

        private static readonly string[] allColumns = new string[] { ID_COLUMN, ORIGIN_COLUMN, SITE_COLUMN, PRZ1_COLUMN, PRZ2_COLUMN, PRZ3_COLUMN, PRZ4_COLUMN, PRZ5_COLUMN, PRZ6_COLUMN, TRZ1_COLUMN, TRZ2_COLUMN, TRZ3_COLUMN, TRZ4_COLUMN, TRZ5_COLUMN, TRZ6_COLUMN };

        // Array of the columns in the imported file that must contain values
        private static readonly string[] requiredValueColumns = new string[] { ORIGIN_COLUMN, SITE_COLUMN };

        // Array of the columns in the imported file that must contain unique values
        private static readonly string[] uniqueValueColumns = new string[] { ID_COLUMN, ORIGIN_COLUMN };

        #endregion Constants

        #region Public Functions

        /// <summary>
        /// Returns a collection of Zone Travel Origin ModelViews.
        /// </summary>
        /// <param name="excelFileStream">A file stream holding the uploaded data from the user</param>
        /// <param name="originalOrigins">The original Origins.</param>
        /// <returns>Collection of new Zone Travel Origin ModelViews objects.</returns>
        public static ICollection<MSTZoneTravelOriginModelView> ImportFromExcelFile(Stream excelFileStream, ICollection<MSTZoneTravelOriginModelView> originalOrigins)
        {
            try
            {
                // Open the document as read-only.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                {
                    // Get a collection of all rows in the file, filtering out rows that only have data in
                    // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                    // in an enumerable collection
                    ICollection<Dictionary<string, string>> allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, requiredColumns, allColumns, requiredValueColumns, uniqueValueColumns);

                    // Turn each row into a DTO object and return the collection
                    return CreateModelViewsToReturn(allRows, originalOrigins);
                }
            }
            catch (FileFormatException)
            {
                _log.Error("Imported Zone Travel Origin file was an incorrect format.");
                throw new NotExcelFileException();
            }
            catch (ColumnMissingException)
            {
                _log.Error("Imported Zone Travel Origin file was missing a required column.");
                throw;
            }
            catch (CellValueMissingException)
            {
                _log.Error("Imported Zone Travel Origin file was missing a required cell value.");
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
        /// <param name="originalOrigins">The original Origins</param>
        /// <returns>A collection of DTOs representing the newly imported values</returns>
        private static ICollection<MSTZoneTravelOriginModelView> CreateModelViewsToReturn(ICollection<Dictionary<string, string>> allRows, ICollection<MSTZoneTravelOriginModelView> originalOrigins)
        {
            // Create the collection to return
            List<MSTZoneTravelOriginModelView> toReturn = new List<MSTZoneTravelOriginModelView>();

            // Set the ID counter. New objects must have IDs < 0 and mutiple elements submitted to
            // the loader must have different IDs. So, we'll go -1, -2, -3, etc.
            int currentOriginID = -1;

            // For each Dictionary object (representing imported row data)
            foreach (Dictionary<string, string> row in allRows)
            {
                int originId;
                bool newOrigin = false;

                // Check to see if the originId column is set
                if (!row.ContainsKey(ID_COLUMN) || !int.TryParse(row[ID_COLUMN], out originId))
                {
                    originId = currentOriginID--;
                    newOrigin = true;
                }

                MSTZoneTravelOriginModelView zoneTravelOrigin = new MSTZoneTravelOriginModelView();

                // check to see if the originID is valid
                if (newOrigin)
                {
                    zoneTravelOrigin.OriginID = originId;
                }
                else
                {
                    zoneTravelOrigin = originalOrigins.FirstOrDefault(o => o.OriginID == originId);
                    if (zoneTravelOrigin == null)
                    {
                        throw new ValidationException(string.Format("The Origin Id with value {0} was not found in the Database.  Please refresh the page to review these latest changes.", originId));
                    }
                }

                zoneTravelOrigin.Origin = row[ORIGIN_COLUMN];
                zoneTravelOrigin.Site = row[SITE_COLUMN];

                // Note: perdiem and airfare rows can be left blank in the excel doc, which causes the row to not have those key/values
                if (!row.ContainsKey(PRZ1_COLUMN) || string.IsNullOrWhiteSpace(row[PRZ1_COLUMN]))
                {
                    zoneTravelOrigin.ResourcePRZ1 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourcePRZ1 = row[PRZ1_COLUMN];
                }

                if (!row.ContainsKey(PRZ2_COLUMN) || string.IsNullOrWhiteSpace(row[PRZ2_COLUMN]))
                {
                    zoneTravelOrigin.ResourcePRZ2 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourcePRZ2 = row[PRZ2_COLUMN];
                }

                if (!row.ContainsKey(PRZ3_COLUMN) || string.IsNullOrWhiteSpace(row[PRZ3_COLUMN]))
                {
                    zoneTravelOrigin.ResourcePRZ3 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourcePRZ3 = row[PRZ3_COLUMN];
                }

                if (!row.ContainsKey(PRZ4_COLUMN) || string.IsNullOrWhiteSpace(row[PRZ4_COLUMN]))
                {
                    zoneTravelOrigin.ResourcePRZ4 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourcePRZ4 = row[PRZ4_COLUMN];
                }

                if (!row.ContainsKey(PRZ5_COLUMN) || string.IsNullOrWhiteSpace(row[PRZ5_COLUMN]))
                {
                    zoneTravelOrigin.ResourcePRZ5 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourcePRZ5 = row[PRZ5_COLUMN];
                }

                if (!row.ContainsKey(PRZ6_COLUMN) || string.IsNullOrWhiteSpace(row[PRZ6_COLUMN]))
                {
                    zoneTravelOrigin.ResourcePRZ6 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourcePRZ6 = row[PRZ6_COLUMN];
                }

                // Airfare resources
                if (!row.ContainsKey(TRZ1_COLUMN) || string.IsNullOrWhiteSpace(row[TRZ1_COLUMN]))
                {
                    zoneTravelOrigin.ResourceTRZ1 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourceTRZ1 = row[TRZ1_COLUMN];
                }

                if (!row.ContainsKey(TRZ2_COLUMN) || string.IsNullOrWhiteSpace(row[TRZ2_COLUMN]))
                {
                    zoneTravelOrigin.ResourceTRZ2 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourceTRZ2 = row[TRZ2_COLUMN];
                }

                if (!row.ContainsKey(TRZ3_COLUMN) || string.IsNullOrWhiteSpace(row[TRZ3_COLUMN]))
                {
                    zoneTravelOrigin.ResourceTRZ3 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourceTRZ3 = row[TRZ3_COLUMN];
                }

                if (!row.ContainsKey(TRZ4_COLUMN) || string.IsNullOrWhiteSpace(row[TRZ4_COLUMN]))
                {
                    zoneTravelOrigin.ResourceTRZ4 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourceTRZ4 = row[TRZ4_COLUMN];
                }

                if (!row.ContainsKey(TRZ5_COLUMN) || string.IsNullOrWhiteSpace(row[TRZ5_COLUMN]))
                {
                    zoneTravelOrigin.ResourceTRZ5 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourceTRZ5 = row[TRZ5_COLUMN];
                }

                if (!row.ContainsKey(TRZ6_COLUMN) || string.IsNullOrWhiteSpace(row[TRZ6_COLUMN]))
                {
                    zoneTravelOrigin.ResourceTRZ6 = NO_RATE;
                }
                else
                {
                    zoneTravelOrigin.ResourceTRZ6 = row[TRZ6_COLUMN];
                }

                // Add the Origin to the collection to be returned
                toReturn.Add(zoneTravelOrigin);
            }

            // Return the collection of new DTOs
            return toReturn;
        }

        #endregion Private Functions
    }
}