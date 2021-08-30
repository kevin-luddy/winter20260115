// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
    using GenBOE.ActionLogic.Common;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;
    using IES.Common.PickList;

    /// <summary>
    /// Used for importing new genBOE CLIN elements from an Excel file
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CLINImporter : ICLINImporter
    {
        #region Constants

        private static Logger _log = new Logger(typeof(CLINImporter));

        private IFullObjectFactory factory;

        // Individual column names
        protected const string clinIDColumn = "genBOE CLIN ID";

        #endregion Constants

        #region Public Functions

        /// <summary>
        /// Initializes a new instance of the <see cref="CLINImporter"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public CLINImporter(
            IFullObjectFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Imports the clins from excel file.
        /// </summary>
        /// <param name="excelFileStream">The excel file stream.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="allContractTypes">All of the Contract Types</param>
        /// <returns>A collection of imported CLINs.</returns>
        public Collection<ImportedClin> ImportClinsFromExcelFile(Stream excelFileStream, FullWorkspace workspace, ICollection<PickListDto> allContractTypes)
        {
            if (excelFileStream == null)
            {
                throw new ArgumentNullException(nameof(excelFileStream));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (allContractTypes == null)
            {
                throw new ArgumentNullException(nameof(allContractTypes));
            }

            // Add the Not Set value
            allContractTypes.Add(new PickListDto { Id = Constants.CONTRACT_TYPE_NOT_SET, Text = Constants.CONTRACT_TYPE_NOT_SET_STRING });

            using (StopwatchTimer sw = new StopwatchTimer(_log))
            {
                try
                {
                    Collection<ImportedClin> importResults;

                    // Open the document as read-only.
                    using (var document = SpreadsheetDocument.Open(excelFileStream, false))
                    {
                        // Get a collection of all rows in the file, filtering out rows that only have data in
                        // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                        // in an enumerable collection
                        var allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, ImportExportConstants.CLINS, this.GetRequiredColumns(), this.GetRequiredColumns(), null, null, this.GetTextOnlyColumns());

                        // Turn each row into a DTO object and return the collection
                        importResults = this.CreateImportedClins(allRows, workspace, allContractTypes);
                    }

                    return importResults;
                }
                catch (FileFormatException)
                {
                    throw new NotExcelFileException("Imported file was an incorrect format.");
                }
            }
        }

        #endregion Public Functions

        #region Protected Functions

        /// <summary>
        /// Gets the required columns.
        /// </summary>
        protected virtual string[] GetRequiredColumns()
        {
            return new string[] { clinIDColumn, ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER, ImportExportConstants.CLIN_TITLE_COLUMN_HEADER, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER };
        }

        /// <summary>
        /// Gets the columns that should be treated as text and not converted to numeric values.
        /// </summary>
        /// <returns></returns>
        protected virtual string[] GetTextOnlyColumns()
        {
            return new string[] { ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER, ImportExportConstants.CLIN_TITLE_COLUMN_HEADER };
        }

        /// <summary>
        /// Creates the imported clins.
        /// </summary>
        /// <param name="allRows">All rows.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="allContractTypes">All of the contract types</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// workspace or allRows
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected Collection<ImportedClin> CreateImportedClins(ICollection<Dictionary<string, string>> allRows, FullWorkspace workspace, ICollection<PickListDto> allContractTypes)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (allRows == null)
            {
                throw new ArgumentNullException(nameof(allRows));
            }

            // Create the collection to return
            var toReturn = new Collection<ImportedClin>();

            // Set the ID counter. New Clin objects must have IDs < 0 and mutiple Clin elements submitted to
            // the loader must have different IDs. So, we'll go -1, -2, -3, etc.
            var currentClinID = -1;

            // Get all Clins currently in the workspace
            var existingClinsForWorkspace = workspace.Clins;
            
            // For each Dictionary object (representing imported row data)
            foreach (Dictionary<string, string> row in allRows)
            {
                ICollection<FullClin> currentClins = (from c in existingClinsForWorkspace
                                   where (row.ContainsKey(clinIDColumn) && c.Id.ToString() != row[clinIDColumn].Trim()) ||
                                         (!row.ContainsKey(clinIDColumn))
                                   select c).ToList();

                ICollection<Dictionary<string, string>> uniqueClinNumberLinqResultsFromSpreadsheet = (from r in allRows
                                                                 where row.ContainsKey(ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER) &&
                                                                       r.ContainsKey(ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER) &&
                                                                       r[ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER].IsEquivalentTo(row[ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER])
                                                                 select r).ToList();

                ICollection<FullClin> uniqueClinNumberLinqResultsFromDatabase = (from c in currentClins
                                                              where row.ContainsKey(ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER) &&
                                                                    c.ClinNumber.IsEquivalentTo(row[ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER])
                                                              select c).ToList();

                ICollection<Dictionary<string, string>> uniqueClinIDLinqResults = (from r in allRows
                                              where row.ContainsKey(clinIDColumn) && r.ContainsKey(clinIDColumn) && r[clinIDColumn] == row[clinIDColumn]
                                              select r).ToList();

                bool headersValid = this.CheckHeaders(workspace, toReturn, ref currentClinID, row, uniqueClinNumberLinqResultsFromSpreadsheet, uniqueClinNumberLinqResultsFromDatabase, uniqueClinIDLinqResults, allContractTypes);

                if (headersValid)
                {
                    try
                    {
                        var clinID = Convert.ToInt32(row[clinIDColumn]);

                        // Keep a copy of the old Clin data for comparison
                        var oldClin = this.factory.CreateFullClin(clinID);

                        // If there is no existing CLIN with the ID in this row, we'll create a new CLIN
                        if (oldClin == null)
                        {
                            // Add the new Model View to the collection to be returned
                            toReturn.Add(this.CreateNewImportedClin(row, currentClinID--, workspace, allContractTypes));
                        }
                        // Otherwise we'll add a CLIN Update
                        else
                        {
                            try
                            {
                                ImportedClin newClin = this.CreateUpdateClin(workspace, row, clinID, oldClin, allContractTypes);

                                // If there are updates or parsing issues with the existing Clin, we'll add it to the return collection
                                if (newClin.ImportTypes.Count > 0)
                                {
                                    toReturn.Add(newClin);
                                }
                            }
                            // Catch exceptions where the genBOE CLIN ID supplied does not exist in the current workspace
                            catch (InvalidDataRelationException)
                            {
                                toReturn.Add(this.CreateNewImportedClin(row, currentClinID--, workspace, allContractTypes));
                                // create new row
                            }
                            catch (ArgumentNullException)
                            {
                                toReturn.Add(this.CreateNewImportedClin(row, currentClinID--, workspace, allContractTypes));
                                // create new row
                            }
                        }
                    }
                    // Catch exceptions where the genBOE Clin ID column is not an integer.
                    catch (FormatException)
                    {
                        toReturn.Add(this.CreateNewImportedClin(row, currentClinID--, workspace, allContractTypes));
                    }
                    catch (OverflowException)
                    {
                        toReturn.Add(this.CreateNewImportedClin(row, currentClinID--, workspace, allContractTypes));
                    }
                }
            }

            var toTruncate = from t in toReturn
                             where !string.IsNullOrEmpty(t.ClinTitle) &&
                                t.ClinTitle.Length > 100 &&
                                (t.ImportTypes.Contains(ClinImportResult.UpdateClin) ||
                                t.ImportTypes.Contains(ClinImportResult.CreateClin))
                             select t;

            foreach (var result in toTruncate)
            {
                result.ImportTypes.Add(ClinImportResult.TruncateTitle);
            }

            return toReturn;
        }

        /// <summary>
        /// Creates the update clin.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="row">The row.</param>
        /// <param name="clinID">The clin identifier.</param>
        /// <param name="oldClin">The old clin.</param>
        /// <param name="allContractTypes">All of the contract types.</param>
        /// <returns>An imported clin that is to be updated.</returns>
        protected virtual ImportedClin CreateUpdateClin(FullWorkspace workspace, Dictionary<string, string> row, int clinID, FullClin oldClin, ICollection<PickListDto> allContractTypes)
        {
            if (ReferenceEquals(oldClin, null))
            {
                throw new ArgumentNullException(nameof(oldClin));
            }
            if (ReferenceEquals(row, null))
            {
                throw new ArgumentNullException(nameof(row));
            }
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            // Create a ModelView with the Clin DTO to represent the new imported Clin data
            ImportedClin newClin = new ImportedClin(this.factory.CreateFullClin(clinID));

            DataRelationshipVerifier.VerifyDataRelation(newClin, workspace.Id);

            newClin.ClinNumber = row[ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER];
            newClin.ClinTitle = row[ImportExportConstants.CLIN_TITLE_COLUMN_HEADER];
            this.ParseStartAndEndDates(row, workspace, newClin);

            // If the Clin was changed and there were no other parsing issues, then we'll set this
            // Clin as an Update
            if (!newClin.ClinNumber.Trim().Equals(oldClin.ClinNumber.Trim(), StringComparison.CurrentCultureIgnoreCase) ||
                !newClin.ClinTitle.Trim().Equals(oldClin.ClinTitle.Trim(), StringComparison.CurrentCultureIgnoreCase) ||
                newClin.StartDate != oldClin.StartDate ||
                newClin.EndDate != oldClin.EndDate)
            {
                newClin.ImportTypes.Add(ClinImportResult.UpdateClin);
            }

            return newClin;
        }

        /// <summary>
        /// Checks the headers.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="toReturn">To return.</param>
        /// <param name="currentClinID">The current clin identifier.</param>
        /// <param name="row">The row.</param>
        /// <param name="uniqueClinNumberLinqResultsFromSpreadsheet">The unique clin number linq results from spreadsheet.</param>
        /// <param name="uniqueClinNumberLinqResultsFromDatabase">The unique clin number linq results from database.</param>
        /// <param name="uniqueClinIDLinqResults">The unique clin identifier linq results.</param>
        /// <param name="allContractTypes">All of the contract types.</param>
        /// <returns>True/false if the headers are valid.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        protected virtual bool CheckHeaders(FullWorkspace workspace, Collection<ImportedClin> toReturn, ref int currentClinID, Dictionary<string, string> row, ICollection<Dictionary<string, string>> uniqueClinNumberLinqResultsFromSpreadsheet, ICollection<FullClin> uniqueClinNumberLinqResultsFromDatabase, ICollection<Dictionary<string, string>> uniqueClinIDLinqResults, ICollection<PickListDto> allContractTypes)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            if (toReturn == null)
            {
                throw new ArgumentNullException(nameof(toReturn));
            }

            if (uniqueClinNumberLinqResultsFromSpreadsheet == null)
            {
                throw new ArgumentNullException(nameof(uniqueClinNumberLinqResultsFromSpreadsheet));
            }

            if (uniqueClinNumberLinqResultsFromDatabase == null)
            {
                throw new ArgumentNullException(nameof(uniqueClinNumberLinqResultsFromDatabase));
            }

            if (uniqueClinIDLinqResults == null)
            {
                throw new ArgumentNullException(nameof(uniqueClinIDLinqResults));
            }

            bool validHeaders = false;
            if (!row.ContainsKey(ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER) || !row.ContainsKey(ImportExportConstants.CLIN_TITLE_COLUMN_HEADER))
            {
                var addMessage = !toReturn.Any(x => x.ImportTypes.Contains(ClinImportResult.MissingNumberOrTitle));

                // Only add this message once for the entire import
                if (addMessage)
                {
                    // Add the new Model View to the collection to be returned
                    toReturn.Add(new ImportedClin(null, null, null, null, ClinImportResult.MissingNumberOrTitle));
                }
            }
            else if (uniqueClinNumberLinqResultsFromSpreadsheet.Count + uniqueClinNumberLinqResultsFromDatabase.Count > 1)
            {
                // Add the new Model View to the collection to be returned
                toReturn.Add(new ImportedClin(row[ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER], row[ImportExportConstants.CLIN_TITLE_COLUMN_HEADER], null, null, ClinImportResult.NonUniqueClinNumber));
            }
            else if (uniqueClinIDLinqResults.Count > 1)
            {
                // Add the new Model View to the collection to be returned
                toReturn.Add(new ImportedClin(row[ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER], row[ImportExportConstants.CLIN_TITLE_COLUMN_HEADER], null, null, ClinImportResult.NonUniqueClinID));
            }
            else if (row[ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER].Length > 20)
            {
                // Add the new Model View to the collection to be returned
                toReturn.Add(new ImportedClin(row[ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER], row[ImportExportConstants.CLIN_TITLE_COLUMN_HEADER], null, null, ClinImportResult.ClinNumberTooLong));
            }
            else if (!row.Keys.Contains(clinIDColumn) || string.IsNullOrEmpty(row[clinIDColumn].Trim()))
            {
                // Add the new Model View to the collection to be returned
                toReturn.Add(this.CreateNewImportedClin(row, currentClinID--, workspace, allContractTypes));
            }
            else
            {
                validHeaders = true;
            }

            return validHeaders;
        }

        /// <summary>
        /// Creates the new imported clin.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="clinID">The clin identifier.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="allContractTypes">The contract types.</param>
        /// <returns>A new imported CLIN.</returns>
        protected virtual ImportedClin CreateNewImportedClin(Dictionary<string, string> row, int clinID, WorkspaceDTO workspace, ICollection<PickListDto> allContractTypes)
        {
            if (ReferenceEquals(row, null))
            {
                throw new ArgumentNullException(nameof(row));
            }
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            var toReturn = new ImportedClin
            {
                Id = clinID,
                ClinNumber = row[ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER],
                ClinTitle = row[ImportExportConstants.CLIN_TITLE_COLUMN_HEADER],
                WorkspaceID = workspace.Id,
                Updateable = UpdateType.Upsert
               
            };

            this.ParseStartAndEndDates(row, workspace, toReturn);

            toReturn.ImportTypes.Add(ClinImportResult.CreateClin);

            return toReturn;
        }

        /// <summary>
        /// Parses the start and end dates.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="clin">The clin.</param>
        protected void ParseStartAndEndDates(Dictionary<string, string> row, WorkspaceDTO workspace, ImportedClin clin)
        {
            if (ReferenceEquals(clin, null))
            {
                throw new ArgumentNullException(nameof(clin));
            }
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (ReferenceEquals(row, null))
            {
                throw new ArgumentNullException(nameof(row));
            }

            // Set both dates to null
            clin.StartDate = clin.EndDate = null;

            // Start Date is optional, but if it is included it must be in the proper format and must be after
            // the Workspace's Contract Start Date
            if (row.Keys.Contains(ImportExportConstants.START_DATE_COLUMN_HEADER) && !string.IsNullOrEmpty(row[ImportExportConstants.START_DATE_COLUMN_HEADER].Trim()))
            {
                DateTime startDate;

                // Make sure the date is in the proper format
                if (DateTime.TryParse(row[ImportExportConstants.START_DATE_COLUMN_HEADER], out startDate))
                {
                    startDate = GenBOEUtilities.AdjustDateTimePrecision(startDate, DateTimePrecision.Month);

                    // Make sure that the start date is after the workspace start date
                    if (startDate < workspace.ContractStartDate)
                    {
                        clin.ImportTypes.Add(ClinImportResult.StartDateTooEarly);
                    }
                    else
                    {
                        clin.StartDate = startDate;
                    }
                }
                else
                {
                    clin.ImportTypes.Add(ClinImportResult.InvalidStartDateFormat);
                }
            }

            // End Date is optional, but if it is included it must be in the proper format and must be before
            // the Workspace's Contract End Date
            if (row.Keys.Contains(ImportExportConstants.END_DATE_COLUMN_HEADER) && !string.IsNullOrEmpty(row[ImportExportConstants.END_DATE_COLUMN_HEADER].Trim()))
            {
                DateTime endDate;

                // Make sure the date is in the proper format
                if (DateTime.TryParse(row[ImportExportConstants.END_DATE_COLUMN_HEADER], out endDate))
                {
                    endDate = GenBOEUtilities.AdjustDateTimePrecision(endDate, DateTimePrecision.Month);

                    // Make sure that the end date is before the workspace start date
                    if (endDate > workspace.ContractEndDate)
                    {
                        clin.ImportTypes.Add(ClinImportResult.EndDateTooLate);
                    }
                    else
                    {
                        clin.EndDate = endDate;
                    }
                }
                else
                {
                    clin.ImportTypes.Add(ClinImportResult.InvalidEndDateFormat);
                }
            }

            // If both dates are valid, but the end isn't after the start
            if ( (clin.StartDate != null && clin.EndDate != null) &&
                 (clin.StartDate > clin.EndDate) )
            {
                // Make sure both dates end up as null since they were invalid
                clin.StartDate = clin.EndDate = null;
                clin.ImportTypes.Add(ClinImportResult.StartDateMustBeBeforeEndDate);
            }

            // If only one date was valid
            else if ((clin.StartDate == null && clin.EndDate != null) ||
                (clin.StartDate != null && clin.EndDate == null))
            {
                // Make sure both dates end up as null since only one was valid
                clin.StartDate = clin.EndDate = null;
                clin.ImportTypes.Add(ClinImportResult.NoDatesOrBothDatesRequired);
            }
        }

        #endregion Protected Functions
    }

    public enum ClinImportResult
    {
        CreateClin = 0,
        UpdateClin = 1,
        MissingNumberOrTitle = 2,
        NonUniqueClinNumber = 3,
        TruncateTitle = 4,
        ClinNumberTooLong = 5,
        StartDateTooEarly = 6,
        EndDateTooLate = 7,
        NonUniqueClinID = 8,
        InvalidStartDateFormat = 9,
        InvalidEndDateFormat = 10,
        NoDatesOrBothDatesRequired = 11,
        StartDateMustBeBeforeEndDate = 12,
        MissingInvalidContractType = 13
    }

    [ExcludeFromCodeCoverage]
    public class ImportedClin : ClinDTO
    {
        public ImportedClin()
        {
            this.ImportTypes = new Collection<ClinImportResult>();
        }

        public ImportedClin(string inClinNumber, string inClinTitle, DateTime? inStartDate, DateTime? inEndDate, ClinImportResult inImportResult)
            : this()
        {
            this.ClinNumber = inClinNumber;
            this.ClinTitle = inClinTitle;
            this.StartDate = inStartDate;
            this.EndDate = inEndDate;
            this.ImportTypes.Add(inImportResult);
        }

        public ImportedClin(ClinDTO clin)
            : this()
        {
            if (clin != null)
            {
                this.Id = clin.Id;
                this.ClinNumber = clin.ClinNumber;
                this.ClinTitle = clin.ClinTitle;
                this.StartDate = clin.StartDate;
                this.EndDate = clin.EndDate;
                this.InUse = clin.InUse;
                this.Updateable = clin.Updateable;
                this.UpdateDate = clin.UpdateDate;
                this.WorkspaceID = clin.WorkspaceID;
            }
        }

        public Collection<ClinImportResult> ImportTypes { get; set; }

        public long? StartDateLong
        {
            get
            {
                return this.StartDate?.Ticks;
            }
            set
            {
                this.StartDate = value.HasValue ? new DateTime(value.Value) : (DateTime?)null;
            }
        }

        public long? EndDateLong
        {
            get
            {
                return this.EndDate?.Ticks;
            }
            set
            {
                this.EndDate = value.HasValue ? new DateTime(value.Value) : (DateTime?)null;
            }
        }
    }
}