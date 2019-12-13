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
    using GenBOE.ActionLogic.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.DataBridge.Reference;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    public class TMResourceRateImporter : ResourceRateImporter
    {

        private Logger logger = new Logger(typeof(TMResourceRateImporter));
        private ITMResourceRateDTODataLoader tmResourceRateDTOLoader;
        private IResourceDTODataLoader resourceDTODataLoader;

        // Col constants
        private const string RESOURCE_NAME = "Resource ID";
        private const string BASE_RATE = "Rate $";
        private const string RESOURCE_RATE_ID = "genBOE Resource Rate ID";

        // Array of the columns that must be contained in the imported file
        private readonly string[] requiredColumns = new string[] { RESOURCE_NAME, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, BASE_RATE };

        private readonly string[] toRetrieveColumns = new string[] { RESOURCE_NAME, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, BASE_RATE, RESOURCE_RATE_ID };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tmResourceLoader">T&amp;M Resource data loader</param>
        /// <param name="resourceDTODataLoader">Resource data loader</param>
        public TMResourceRateImporter(ITMResourceRateDTODataLoader tmResourceLoader, IResourceDTODataLoader resourceDTODataLoader)
        {
            this.tmResourceRateDTOLoader = tmResourceLoader;
            this.resourceDTODataLoader = resourceDTODataLoader;
        }

        /// <summary>
        /// Import all the rates in the excel file.
        /// </summary>
        /// <param name="excelFileStream"></param>
        /// <returns></returns>
        public ICollection<ImportedTMResourceRate> ImportTMResourceRateFromExcelFile(Stream excelFileStream, WorkspaceDTO workspace, TMResourceRatesImportType importType, InUseDataLoader _InUseDataLoader)
        {
            if (excelFileStream == null)
            {
                throw new ArgumentNullException(nameof(excelFileStream));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this.logger))
            {
                try
                {
                    ICollection<ImportedTMResourceRate> importResults = null;

                    // Open the document as read-only.
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                    {
                        // Get a collection of all rows in the file, filtering out rows that only have data in
                        // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                        // in an enumerable collection
                        ICollection<Dictionary<string, string>> allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, this.requiredColumns, this.toRetrieveColumns);

                        // Turn each row into a DTO object and return the collection
                        importResults = this.CreateImportedTMResourceRates(allRows, workspace, importType, _InUseDataLoader);
                    }

                    return importResults;
                }
                catch (FileFormatException)
                {
                    throw new NotExcelFileException("Imported file was an incorrect format.");
                }
            }
        }

        /// <summary>
        /// Creates the collection of T&amp;M rates to be imported, or with their errors.
        /// </summary>
        /// <param name="allRows"></param>
        /// <returns></returns>
        private ICollection<ImportedTMResourceRate> CreateImportedTMResourceRates(ICollection<Dictionary<string, string>> allRows, WorkspaceDTO workspace, TMResourceRatesImportType importType, InUseDataLoader _InUseDataLoader)
        {
            List<ImportedTMResourceRate> importResults = new List<ImportedTMResourceRate>();

            ICollection<TMResourceRateDTO> allExistingResourceRates = this.tmResourceRateDTOLoader.GetByWorkspaceId(workspace.Id);

            ICollection<ResourceDTO> allResources = (from x in this.resourceDTODataLoader.GetByListId(workspace.ResourceListID)
                where
                (
                    (x.ElementOfCost == ElementOfCostType.Sub ||
                     x.ElementOfCost == ElementOfCostType.IWTA) &&
                    x.RateType == RateType.Hours)
                select x).ToArray();

            int newID = 0;

            foreach (Dictionary<string, string> row in allRows)
            {
                // perform "first-pass" validation (e.g. missing data field values, etc.)
                ImportedTMResourceRate importRecord = this.CreateResult(row, allResources, workspace, _InUseDataLoader);

                if (!importRecord.ImportTypes.Any())  // skip records which have already been flagged with issues
                {
                    // look for an existing record in the DB (that matches the PK)
                    TMResourceRateDTO matchingRate = allExistingResourceRates.FirstOrDefault(r => r.ResourceRateID == importRecord.ResourceRateID);

                    if (matchingRate == null)  // no existing match in the DB - this is a new new entry
                    {
                        importRecord.ResourceRateID = --newID;
                        importRecord.Updateable = UpdateType.Upsert;
                    }
                    else  // matches an existing entry - continue processing
                    {
                        // determine if ANY column values have actually been changed ...
                        if ((importRecord.ResourceID != matchingRate.ResourceID) ||
                            (importRecord.StartDate.GetValueOrDefault(DateTime.MinValue) != matchingRate.StartDate.GetValueOrDefault(DateTime.MinValue)) ||
                            (importRecord.EndDate.GetValueOrDefault(DateTime.MinValue) != matchingRate.EndDate.GetValueOrDefault(DateTime.MinValue)) ||
                            (importRecord.ResourceRate.GetValueOrDefault(0m) != matchingRate.ResourceRate.GetValueOrDefault(0m)))
                        {
                            if (importRecord.ResourceID != matchingRate.ResourceID)
                            {
                                /*
                                 * If the resource drop-down selection was changed (in the import file), then treat the matching row and
                                 * the imported row as two SEPARATE entries:
                                 * 
                                 *    --> If NOT in-use, then DELETE the matching row and ADD the imported row
                                 *    --> Otherwise, update/reuse the matching row with the new import edits
                                 * 
                                 */
                                if (importRecord.inUse)
                                {
                                    importRecord.ImportTypes.Add(ResourceRatesImportResult.Updated);
                                }
                                else  // NOT in-use
                                {
                                    // create a separate object for the deletion
                                    ImportedTMResourceRate toDelete = importRecord.Clone() as ImportedTMResourceRate;

                                    toDelete.ImportTypes.Add(ResourceRatesImportResult.Updated);
                                    toDelete.Updateable = UpdateType.Deleted;

                                    importResults.Add(toDelete);

                                    // designate the original object as a NEW entry (it will be added to the list below)
                                    importRecord.ResourceRateID = --newID;
                                    importRecord.ImportTypes.Add(ResourceRatesImportResult.Added);
                                }
                            }

                            importRecord.Updateable = UpdateType.Upsert;
                            importRecord.UpdateDate = matchingRate.UpdateDate;
                        }
                        else  // ... if not, then no update required
                        {
                            importRecord.Updateable = UpdateType.None;
                        }
                    }
                }

                importResults.Add(importRecord);
            }

            //At this point we have built a list of everything we are importing.

            // assign Add OR Update disposition (to items that are still valid [i.e. no errors])
            ICollection<ImportedTMResourceRate> stillValidImports = importResults.Where(r => !r.ImportTypes.Any()).ToList();
            foreach (ImportedTMResourceRate validImport in stillValidImports)
            {
                if (validImport.ResourceRateID <= 0)
                {
                    validImport.ImportTypes.Add(ResourceRatesImportResult.Added);
                }
                else if (importType == TMResourceRatesImportType.importNew)
                {
                    validImport.ImportTypes.Add(ResourceRatesImportResult.NotUpdateable);
                }
                else
                {
                    validImport.ImportTypes.Add(ResourceRatesImportResult.Updated);
                }
            }

            // finally, validate dates (i.e. detect overlaps and gaps) for each resource's rate schedule
            ICollection<ImportedTMResourceRate> remainingResourceRates = importResults.Where(r => r.Updateable != UpdateType.Deleted).ToList();
            ICollection<int> resultingResourceIds = remainingResourceRates.Select(r => r.ResourceID).Distinct().ToList();
            foreach (int resourceId in resultingResourceIds)
            {
                // get imported rates marked for upsert for this resource (by ResourceName)
                string resourceName = allResources.Single(r => r.Id == resourceId).ResourceName;
                ICollection<ImportedTMResourceRate> importedResourceRates = remainingResourceRates.Where(r => r.ResourceName == resourceName && r.Updateable == UpdateType.Upsert).ToList();

                // get existing rates for this resource
                ICollection<ResourceRateDTO> existingResourceRates = allExistingResourceRates.Where(r => r.ResourceID == resourceId).ToList<ResourceRateDTO>();
                // filter down to the ones that are not being updated
                existingResourceRates = (from e in existingResourceRates
                    where !importedResourceRates.Any(i => i.ResourceRateID == e.ResourceRateID)
                    select e).ToList();

                // combine the two collections and check dates for gaps or overlaps
                ICollection < ResourceRateDTO > ratesToCheck = existingResourceRates.Concat(importedResourceRates).ToList();
                Collection<ResourceRatesImportResult> dateCheckErrors = this.sequentialDateCheck(ratesToCheck);
                if (dateCheckErrors.Any())
                {
                    foreach (ImportedTMResourceRate rateEntry in importedResourceRates)
                    {
                        rateEntry.ImportTypes.Clear();
                        rateEntry.ImportTypes = dateCheckErrors;
                    }
                }
            }

            importResults.OrderBy(c => c.ResourceName);

            return importResults;
        }

        /// <summary>
        /// Creates a single result
        /// </summary>
        /// <param name="row"></param>
        /// <param name="inResources"></param>
        /// <param name="workspace"></param>
        /// <param name="InUseDataLoader"></param>
        /// <returns></returns>
        public ImportedTMResourceRate CreateResult(Dictionary<string, string> row, ICollection<ResourceDTO> inResources, WorkspaceDTO workspace, InUseDataLoader InUseDataLoader)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            else if (InUseDataLoader == null)
            {
                throw new ArgumentNullException(nameof(InUseDataLoader));
            }

            ImportedTMResourceRate currentResult = new ImportedTMResourceRate
            {
                WorkspaceID = workspace.Id
            };

            if (row != null)
            {
                if (row.ContainsKey(RESOURCE_RATE_ID))
                {
                    int resourceRateId;
                    if (int.TryParse(row[RESOURCE_RATE_ID], out resourceRateId))
                    {
                        if (resourceRateId > 0)
                        {
                            currentResult.ResourceRateID = resourceRateId;
                        }
                    }
                }

                // Check for required data.
                if (row.ContainsKey(RESOURCE_NAME))
                {
                    ResourceDTO thisResource = (from resourceMatches in inResources where resourceMatches.ResourceName.IsEquivalentTo(row[RESOURCE_NAME]) select resourceMatches).FirstOrDefault();

                    if (thisResource != null)
                    {
                        currentResult.ResourceID = thisResource.Id;
                        currentResult.ResourceName = thisResource.ResourceName;

                        if (currentResult.ResourceRateID > 0)
                        {
                            currentResult.inUse = InUseDataLoader.GetInUse(InUseDataType.WorkspaceResources, thisResource.Id, workspace.ResourceListID);
                        }
                    }
                    else
                    {
                        currentResult.ImportTypes.Add(ResourceRatesImportResult.MissingResource);
                    }
                }
                else
                {
                    currentResult.ImportTypes.Add(ResourceRatesImportResult.MissingRequiredData);
                }

                if (row.ContainsKey(ImportExportConstants.START_DATE_COLUMN_HEADER))
                {
                    currentResult.StartDateString = row[ImportExportConstants.START_DATE_COLUMN_HEADER];
                    DateTime parsedDate;
                    if (DateTime.TryParse(row[ImportExportConstants.START_DATE_COLUMN_HEADER], out parsedDate))
                    {
                        currentResult.StartDate = GenBOEUtilities.AdjustDateTimePrecision(parsedDate, DateTimePrecision.Month);
                    }
                    else
                    {
                        currentResult.ImportTypes.Add(ResourceRatesImportResult.InvalidStartDateFormat);
                    }
                }
                else
                {
                    currentResult.ImportTypes.Add(ResourceRatesImportResult.MissingRequiredData);
                }

                if (row.ContainsKey(ImportExportConstants.END_DATE_COLUMN_HEADER))
                {
                    currentResult.EndDateString = row[ImportExportConstants.END_DATE_COLUMN_HEADER];
                    DateTime parsedDate;
                    if (DateTime.TryParse(row[ImportExportConstants.END_DATE_COLUMN_HEADER], out parsedDate))
                    {
                        currentResult.EndDate = GenBOEUtilities.AdjustDateTimePrecision(parsedDate, DateTimePrecision.Month);
                    }
                    else
                    {
                        currentResult.ImportTypes.Add(ResourceRatesImportResult.InvalidEndDateFormat);
                    }

                }
                else
                {
                    currentResult.ImportTypes.Add(ResourceRatesImportResult.MissingRequiredData);
                }

                if (row.ContainsKey(BASE_RATE))
                {
                    currentResult.RateString = row[BASE_RATE];
                    decimal parsedDecimal;
                    if (decimal.TryParse(row[BASE_RATE], out parsedDecimal))
                    {
                        if (parsedDecimal < 10000 && parsedDecimal > 0)
                        {
                            currentResult.ResourceRate = parsedDecimal;
                        }
                        else
                        {
                            currentResult.ImportTypes.Add(ResourceRatesImportResult.InvalidRateFormat);
                        }
                    }
                    else
                    {
                        currentResult.ImportTypes.Add(ResourceRatesImportResult.InvalidRateFormat);
                    }

                }
                else
                {
                    currentResult.ImportTypes.Add(ResourceRatesImportResult.MissingRequiredData);
                }

                if ((currentResult.EndDate.HasValue && currentResult.StartDate.HasValue) && (currentResult.EndDate < currentResult.StartDate))
                {
                    currentResult.ImportTypes.Add(ResourceRatesImportResult.InvalidDateRange);
                }

            }

            return currentResult;
        }
    }

    public enum TMResourceRatesImportType
    {
        replaceAll = 0,
        importNew = 1,
        importUpdates = 2
    }

    [ExcludeFromCodeCoverage]
    public class ImportedTMResourceRate : TMResourceRateDTO, ICloneable
    {
        public Collection<ResourceRatesImportResult> ImportTypes { get; set; }
        public String ResourceName { get; set; }
        public String StartDateString { get; set; }
        public String EndDateString { get; set; }
        public Boolean inUse { get; set; }
        public String RateString { get; set; }

        public long StartDateLong
        {
            get
            {
                if (this.StartDate.HasValue)
                {
                    return this.StartDate.Value.Ticks;
                }
                else
                {
                    return DateTime.MinValue.Ticks;
                }
            }
            set
            {
                this.StartDate = new DateTime(value);
            }
        }

        public long EndDateLong
        {
            get
            {
                if (this.EndDate.HasValue)
                {
                    return this.EndDate.Value.Ticks;
                }
                else
                {
                    return DateTime.MaxValue.Ticks;
                }
            }
            set
            {
                this.EndDate = new DateTime(value);
            }
        }

        public ImportedTMResourceRate()
        {
            this.ImportTypes = new Collection<ResourceRatesImportResult>();
            this.ResourceName = "";
        }

        public ImportedTMResourceRate(TMResourceRateDTO inDTO)
            : this()
        {
            if (inDTO != null)
            {
                this.ResourceRateID = inDTO.ResourceRateID;
                this.ResourceID = inDTO.ResourceID;
                this.StartDate = inDTO.StartDate;
                this.EndDate = inDTO.EndDate;
            }
        }

        public object Clone()
        {
            ImportedTMResourceRate clone = new ImportedTMResourceRate
            {
                EndDate = this.EndDate,
                EndDateLong = this.EndDateLong,
                EndDateString = (string)this.EndDateString.Clone(),
                Id = this.Id,
                inUse = this.inUse,
                RateString = (string)this.RateString.Clone(),
                ResourceID = this.ResourceID,
                ResourceName = (string)this.ResourceName.Clone(),
                ResourceRate = this.ResourceRate,
                ResourceRateID = this.ResourceRateID,
                StartDate = this.StartDate,
                StartDateLong = this.StartDateLong,
                StartDateString = (string)this.StartDateString.Clone(),
                Updateable = this.Updateable,
                UpdateDate = this.UpdateDate,
                UpdateDateLong = this.UpdateDateLong,
                WorkspaceID = this.WorkspaceID
            };

            clone.ImportTypes = new Collection<ResourceRatesImportResult>();
            foreach (ResourceRatesImportResult importResult in this.ImportTypes)
            {
                clone.ImportTypes.Add(importResult);
            }

            return clone;
        }
    }
}
