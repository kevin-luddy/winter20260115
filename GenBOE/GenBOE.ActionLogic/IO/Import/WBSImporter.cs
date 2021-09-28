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
    using System.Text.RegularExpressions;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Used for importing new genBOE WBS elements from an Excel file
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class WbsImporter
    {
        // Create static Regex object for WbsNumber.
        private static Regex regexWbsNumber = new Regex(ValidationConstants.WBS_NUMBER, RegexOptions.None, Constants.REGEX_TIMEOUT);

        #region Constants

        private Logger _log = new Logger(typeof(WbsImporter));

        private IFullObjectFactory factory;
        private IWbsDTODataLoader wbsDtoDataLoader;
        private IRetriever retriever;

        // Individual column names
        private const string wbsIDColumn = "genBOE WBS ID";
        private const string clinNumberColumn = "CLINs";

        private const string wbsPaddedNumber = "WBS Padded #";

        // Array of the columns that must be contained in the imported file
        private readonly string[] requiredColumns = new string[] { wbsIDColumn, ImportExportConstants.WBS_NUMBER_COLUMN_HEADER, ImportExportConstants.WBS_TITLE_COLUMN_HEADER, clinNumberColumn };

        // Array of columns that should be treated as text and not converted to numeric values.
        private readonly string[] textOnlyColumns = new string[] { wbsIDColumn, clinNumberColumn, ImportExportConstants.WBS_NUMBER_COLUMN_HEADER, ImportExportConstants.WBS_TITLE_COLUMN_HEADER };

        #endregion Constants

        #region Public Functions

        public WbsImporter(IFullObjectFactory factory, IWbsDTODataLoader wbsDtoDataLoader, IRetriever retriever)
        {
            this.factory = factory;
            this.wbsDtoDataLoader = wbsDtoDataLoader;
            this.retriever = retriever;
        }

        public Collection<ImportedWbs> ImportWBSFromExcelFile(Stream excelFileStream, FullWorkspace workspace)
        {
            if (excelFileStream == null)
            {
                throw new ArgumentNullException(nameof(excelFileStream));
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                try
                {
                    Collection<ImportedWbs> importResults = new Collection<ImportedWbs>();

                    // Open the document as read-only.
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                    {
                        // Get a collection of all rows in the file, filtering out rows that only have data in
                        // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                        // in an enumerable collection
                        ICollection<Dictionary<string, string>> allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, this.requiredColumns, this.requiredColumns, null, null, this.textOnlyColumns);

                        // Turn each row into a DTO object and return the collection
                        importResults = this.CreateImportedWbss(allRows, workspace);
                    }

                    return importResults;
                }
                catch (FileFormatException)
                {
                    throw new NotExcelFileException("Imported file was an incorrect format.");
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ICollection<Dictionary<string, string>> GetImportWBSFromExcelFileCount(Stream excelFileStream)
        {
            if (excelFileStream == null)
            {
                throw new ArgumentNullException(nameof(excelFileStream));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                try
                {
                    ICollection<Dictionary<string, string>> allRows;
                    // Open the document as read-only.
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                    {
                        // Get a collection of all rows in the file, filtering out rows that only have data in
                        // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                        // in an enumerable collection
                        allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, this.requiredColumns, this.requiredColumns);
                    }

                    return allRows;
                }
                catch (FileFormatException)
                {
                    throw new NotExcelFileException("Imported file was an incorrect format.");
                }
            }
        }

        #endregion Public Functions

        #region Private Functions

        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private Collection<ImportedWbs> CreateImportedWbss(
            ICollection<Dictionary<string, string>> allRows,
            FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            // Create the collection to return
            Collection<ImportedWbs> toReturn = new Collection<ImportedWbs>();

            // Set the ID counter. New WBS objects must have IDs < 0 and mutiple WBS elements submitted to
            // the mapper must have different IDs. So, we'll go -1, -2, -3, etc.
            int currentWBSID = -1;

            ICollection<ClinDTO> existingClinsForWorkspace = workspace.ClinsNoMultiClin.ToCollection<ClinDTO>();
            ICollection<WbsDTO> allCurrentWBSs = workspace.WbsElements.ToList<WbsDTO>();

            foreach (Dictionary<string, string> row in allRows)
            {
                // Convert the wbs number to the padded representation as stored in the DB. Required for uniqueness check.
                if (row.ContainsKey(ImportExportConstants.WBS_NUMBER_COLUMN_HEADER))
                {
                    string wbsNumber = row[ImportExportConstants.WBS_NUMBER_COLUMN_HEADER];
                    if (!string.IsNullOrEmpty(wbsNumber))
                    {
                        row.Add(wbsPaddedNumber, this.wbsDtoDataLoader.PadWBSNumber(wbsNumber));
                    }
                }
            }

            // For each Dictionary object (representing imported row data)
            foreach (Dictionary<string, string> row in allRows)
            {
                var currentWBSs = (from r in allCurrentWBSs
                                   where (row.ContainsKey(wbsIDColumn) && r.Id.ToString() != row[wbsIDColumn].Trim()) ||
                                     (!row.ContainsKey(wbsIDColumn))
                                   select r).ToArray();

                var uniqueWbsNumberLinqResultsFromSpreadsheet = (from r in allRows
                                                                 where row.ContainsKey(wbsPaddedNumber) && r.ContainsKey(wbsPaddedNumber) && r[wbsPaddedNumber].IsEquivalentTo(row[wbsPaddedNumber])
                                                                 select r).ToArray();

                var uniqueWbsNumberLinqResultsFromDatabase = (from r in currentWBSs
                                                              where row.ContainsKey(wbsPaddedNumber) && r.WbsPaddedNumber.IsEquivalentTo(row[wbsPaddedNumber])
                                                              select r).ToArray();

                var uniqueWbsIDLinqResults = (from r in allRows
                                             where row.ContainsKey(wbsIDColumn) && r.ContainsKey(wbsIDColumn) && r[wbsIDColumn] == row[wbsIDColumn]
                                             select r).ToArray();

                if (!row.ContainsKey(ImportExportConstants.WBS_NUMBER_COLUMN_HEADER) || !row.ContainsKey(ImportExportConstants.WBS_TITLE_COLUMN_HEADER))
                {
                    // Add the new Model View to the collection to be returned
                    toReturn.Add(new ImportedWbs(null, null, null, null, WbsImportResult.MissingNumberOrTitle));
                }
                else if (uniqueWbsNumberLinqResultsFromSpreadsheet.Length > 1 || uniqueWbsNumberLinqResultsFromDatabase.Any() && !row.Keys.Contains(wbsIDColumn))
                {
                    // Add the new Model View to the collection to be returned
                    toReturn.Add(new ImportedWbs(row[ImportExportConstants.WBS_NUMBER_COLUMN_HEADER], row[ImportExportConstants.WBS_TITLE_COLUMN_HEADER], null, null, WbsImportResult.NonUniqueWbsNumber));
                }
                else if (uniqueWbsIDLinqResults.Length + uniqueWbsNumberLinqResultsFromDatabase.Length > 1)
                {
                    // Add the new Model View to the collection to be returned
                    toReturn.Add(new ImportedWbs(row[ImportExportConstants.WBS_NUMBER_COLUMN_HEADER], row[ImportExportConstants.WBS_TITLE_COLUMN_HEADER], null, null, WbsImportResult.NonUniqueWbsID));
                }                
                else if (!regexWbsNumber.IsMatch(row[ImportExportConstants.WBS_NUMBER_COLUMN_HEADER]) || row[ImportExportConstants.WBS_NUMBER_COLUMN_HEADER].Length > 30)
                {
                    // Add the new Model View to the collection to be returned
                    toReturn.Add(new ImportedWbs(row[ImportExportConstants.WBS_NUMBER_COLUMN_HEADER], row[ImportExportConstants.WBS_TITLE_COLUMN_HEADER], null, null, WbsImportResult.InvalidWbsNumberFormat));
                }
                else if (!row.Keys.Contains(wbsIDColumn) || string.IsNullOrEmpty(row[wbsIDColumn].Trim()))
                {
                    // Add the new Model View to the collection to be returned
                    this.AddNewImportedWBS(row, currentWBSID--, workspace.Id, existingClinsForWorkspace.ToCollection(), toReturn);
                }
                else
                {
                    try
                    {
                        int wbsID = Convert.ToInt32(row[wbsIDColumn]);

                        ImportedWbs wbs = new ImportedWbs(this.factory.CreateFullWbs(wbsID));

                        try
                        {
                            DataRelationshipVerifier.VerifyDataRelation(wbs, workspace.Id);                  

                            // get CLINs 
                            bool clinsAdded = false;
                            bool clinsRemoved = false;
                            Collection<int> importedClinIDs = new Collection<int>();

                            if (row.ContainsKey(clinNumberColumn))
                            {
                                foreach (string clin in row[clinNumberColumn].Split(','))
                                {
                                    string trimmedClin = clin.ToLower().Trim();
                                    ClinDTO selectedClin = existingClinsForWorkspace.FirstOrDefault(x => x.ClinNumber.ToLower().Trim() == trimmedClin);
                                    
                                    if (selectedClin != null)
                                    {
                                        importedClinIDs.Add(selectedClin.Id);
                                    }
                                    else
                                    {
                                        toReturn.Add(new ImportedWbs(null, null, clin, null, WbsImportResult.ClinsDoNotExist));
                                    }
                                }
                            }

                            // determine if clins were added
                            foreach (int clinID in importedClinIDs)
                            {
                                if (clinsAdded)
                                {
                                    break;
                                }

                                if (!wbs.ClinIDs.Contains(clinID))
                                {
                                    clinsAdded = true;
                                }
                            }

                            // determine if clins were removed
                            foreach (int clinID in wbs.ClinIDs)
                            {
                                if (clinsRemoved)
                                {
                                    break;
                                }

                                if (!importedClinIDs.Contains(clinID))
                                {
                                    clinsRemoved = true;
                                }
                            }

                            bool hasDefiniteChanges = row[ImportExportConstants.WBS_NUMBER_COLUMN_HEADER].IsNotEquivalentTo(wbs.WbsNumber) ||
                                row[ImportExportConstants.WBS_TITLE_COLUMN_HEADER].IsNotEquivalentTo(wbs.WbsTitle) ||
                                clinsAdded || (clinsRemoved && wbs.inUse == false);

                            if (hasDefiniteChanges || clinsRemoved)
                            {
                                wbs.WbsNumber = row[ImportExportConstants.WBS_NUMBER_COLUMN_HEADER];
                                wbs.WbsTitle = row[ImportExportConstants.WBS_TITLE_COLUMN_HEADER];
                                if (hasDefiniteChanges)
                                {
                                    wbs.ImportTypes.Add(WbsImportResult.UpdateWbs);
                                }
                                // make the collection writable
                                wbs.ClinIDs = new Collection<int>(wbs.ClinIDs.ToList());
                                toReturn.Add(wbs);

                                ICollection<FullBoe> currentBOEs = this.retriever.GetBoesByWbs(wbs.Id);

                                if (currentBOEs.Count > 0)
                                {
                                    if (clinsRemoved)
                                    {
                                        foreach (int clinID in wbs.ClinIDs)
                                        {
                                            FullClin clinObject = this.factory.CreateFullClin(clinID);

                                            if (!importedClinIDs.Contains(clinID))
                                            {
                                                ImportedWbs removedClin = new ImportedWbs(wbs.WbsNumber, wbs.WbsTitle, clinObject.ClinNumber, clinObject.ClinTitle, WbsImportResult.InUseClinRemoved);
                                                toReturn.Add(removedClin);
                                            }
                                        }
                                    }

                                    foreach (int clinID in importedClinIDs)
                                    {
                                        FullClin clinObject = this.factory.CreateFullClin(clinID);

                                        if (!wbs.ClinIDs.Contains(clinObject.Id))
                                        {
                                            ImportedWbs newBOE = new ImportedWbs(wbs.WbsNumber, wbs.WbsTitle, clinObject.ClinNumber, clinObject.ClinTitle, WbsImportResult.CreateBoe, clinObject.Id);
                                            newBOE.Id = wbs.Id;
                                            toReturn.Add(newBOE);
                                            wbs.ClinIDs.Add(clinObject.Id);
                                        }
                                    }
                                }
                                else
                                {
                                    wbs.ClinIDs = importedClinIDs;
                                }
                            }
                        }
                        // Catch exceptions where the genBOE WBS ID supplied does not exist in the current workspace
                        catch (InvalidDataRelationException)
                        {
                            this.AddNewImportedWBS(row, currentWBSID--, workspace.Id, existingClinsForWorkspace.ToCollection(), toReturn);
                            // create new row
                        }
                        catch (ArgumentNullException)
                        {
                            this.AddNewImportedWBS(row, currentWBSID--, workspace.Id, existingClinsForWorkspace.ToCollection(), toReturn);
                            // create new row
                        }
                    }
                        // Catch exceptions where the genBOE WBS ID column is not an integer.
                    catch (FormatException)
                    {
                        this.AddNewImportedWBS(row, currentWBSID--, workspace.Id, existingClinsForWorkspace.ToCollection(), toReturn);
                    }
                    catch (OverflowException)
                    {
                        this.AddNewImportedWBS(row, currentWBSID--, workspace.Id, existingClinsForWorkspace.ToCollection(), toReturn);
                    }                    
                }                
            }

            var toTruncate = from t in toReturn
                             where !string.IsNullOrEmpty(t.WbsTitle) && 
                                t.WbsTitle.Length > 100 && 
                                (t.ImportTypes.Contains(WbsImportResult.UpdateWbs) || 
                                t.ImportTypes.Contains(WbsImportResult.CreateWbs))
                             select t;

            foreach (ImportedWbs result in toTruncate)
            {
                result.ImportTypes.Add(WbsImportResult.TruncateTitle);
            }

            return toReturn;
        }

        private void AddNewImportedWBS(
            Dictionary<string, string> row,
            int wbsID,
            int workspaceID,
            Collection<ClinDTO> existingClinsForWorkspace,
            Collection<ImportedWbs> importedWbsCollection)
        {
            ImportedWbs newWBS = new ImportedWbs(row[ImportExportConstants.WBS_NUMBER_COLUMN_HEADER], row[ImportExportConstants.WBS_TITLE_COLUMN_HEADER], null, null, WbsImportResult.CreateWbs);

            // Create a new Model View object and set it's values using the imported row data
            newWBS.Id = wbsID;
            newWBS.WorkspaceID = workspaceID;
            newWBS.Updateable = UpdateType.Upsert;
            
            // CLINs are not required for a new WBS, so we'll make sure that some were imported before
            // adding them to the new Model View
            if (row.Keys.Contains(clinNumberColumn))
            {
                // collection of CLIN IDs
                Collection<int> clinIDs = new Collection<int>();

                // Split the CLIN numbers at the commas and iterate over the results
                foreach (string clinNumber in row[clinNumberColumn].Replace(" ", "").Split(','))
                {
                    // Convert CLIN NUMBER to CLIN ID
                    int clinID = existingClinsForWorkspace.Where(c => String.Compare(c.ClinNumber, clinNumber, true) == 0).Select(c => c.Id).FirstOrDefault();

                    if (clinID > 0)
                    {
                        // add the CLIN ID
                        clinIDs.Add(clinID);
                    }
                    else
                    {
                        if (importedWbsCollection.Where(x => x.ClinNumber == clinNumber).Select(x => x.ClinNumber).FirstOrDefault() == null)
                        {
                            importedWbsCollection.Add(new ImportedWbs(null, null, clinNumber, null, WbsImportResult.ClinsDoNotExist));
                        }
                    }
                }

                newWBS.ClinIDs = clinIDs;
            }

            importedWbsCollection.Add(newWBS);
        }

        #endregion Private Functions
    }

    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum WbsImportResult
    {
        CreateWbs = 0,
        UpdateWbs = 1,
        CreateBoe = 2,
        MissingNumberOrTitle = 3,
        NonUniqueWbsNumber = 4,
        TruncateTitle = 5,
        InvalidWbsNumberFormat = 6,
        ClinsDoNotExist = 7,
        NonUniqueWbsID = 8,
        CircularReferences = 9,
        InUseClinRemoved = 10,
        DeleteWbs = 13,
        WbsInUse = 14,
        ParentHasWbs = 15,
        ChildHasWbs = 16
    }

    [ExcludeFromCodeCoverage]
    public class ImportedWbs : WbsDTO
    {
        public ImportedWbs()
        {
            this.ImportTypes = new Collection<WbsImportResult>();
        }

        public ImportedWbs(string inWbsNumber, string inWbsTitle, string inClinNumber, string inClinTitle, WbsImportResult inImportResult, int inClinID = -1)
            : this()
        {
            this.WbsNumber = inWbsNumber;

            if(!string.IsNullOrEmpty(this.WbsNumber) && this.WbsNumber.Contains("E"))
            {
                decimal result;
                if (decimal.TryParse(this.WbsNumber, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result))
                {
                    this.WbsNumber = result.ToString();
                }
            }

            this.WbsTitle = inWbsTitle;
            this.ClinID = inClinID;
            this.ClinNumber = inClinNumber;
            this.ClinTitle = inClinTitle;
            this.ImportTypes.Add(inImportResult);
        }

        public ImportedWbs(WbsDTO wbs)
            : this()
        {
            if (wbs != null)
            {
                this.ClinIDs = wbs.ClinIDs;
                this.ClinsInUse = wbs.ClinsInUse;
                this.inUse = wbs.inUse;
                this.Level = wbs.Level;
                this.Updateable = wbs.Updateable;
                this.UpdateDate = wbs.UpdateDate;
                this.Id = wbs.Id;
                this.WbsNumber = wbs.WbsNumber;
                this.WbsPaddedNumber = wbs.WbsPaddedNumber;
                this.WbsTitle = wbs.WbsTitle;
                this.WorkspaceID = wbs.WorkspaceID;
            }
        }

        public Collection<WbsImportResult> ImportTypes { get; set; }
        public int ClinID { get; set; }
        public string ClinNumber { get; set; }
        public string ClinTitle { get; set; }
    }
}
