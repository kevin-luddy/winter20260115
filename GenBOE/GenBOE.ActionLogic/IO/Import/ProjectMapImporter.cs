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
    using System.Web.Configuration;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.ActionLogic.Common;
    using GenBOE.DataBridge.Common;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;
    using static IES.Common.OfficeUtilities.ExcelUtilities;

    /// <summary>
    /// Responsible for ProjectMap-specific Excel import.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ProjectMapImporter
    {
        #region Constants

        /// <summary>
        /// The default value for the add/delete column.
        /// </summary>
        private const string ADD_DELETE_DEFAULT = "A";

        /// <summary>
        /// Array of the columns that must be contained in the imported file.
        /// </summary>
        private static readonly string[] RequiredColumns = new string[]
        {
            ImportExportConstants.PROJECTMAP_WBS_NUMBER_COLUMN_HEADER,
            ImportExportConstants.ACTIVITY_ID_COLUMN_HEADER,
            ImportExportConstants.ACTIVITY_NAME_COLUMN_HEADER,
            ImportExportConstants.WBS_ELEMENT_TITLE_COLUMN_HEADER,
            ImportExportConstants.INITIAL_RESOURCE_COLUMN_HEADER,
            ImportExportConstants.COST_CENTER_COLUMN_HEADER,
            ImportExportConstants.LEGACY_RESOURCE_COLUMN_HEADER,
            ImportExportConstants.START_DATE_COLUMN_HEADER,
            ImportExportConstants.END_DATE_COLUMN_HEADER,
            ImportExportConstants.CLIN_COLUMN_HEADER,
            ImportExportConstants.SOW_COLUMN_HEADER,
            ImportExportConstants.SOW_TITLE_COLUMN_HEADER,
            ImportExportConstants.TASK_COLUMN_HEADER,
            ImportExportConstants.HOURS_COLUMN_HEADER,
            ImportExportConstants.DOLLARS_COLUMN_HEADER,
            ImportExportConstants.CAM_NAME_COLUMN_HEADER,
            ImportExportConstants.CATEGORY_COLUMN_HEADER,
            ImportExportConstants.OFFLOAD_COLUMN_HEADER,
            ImportExportConstants.ADD_DELETE_COLUMN_HEADER,
            ImportExportConstants.CLASS_OF_COST_COLUMN_HEADER
        };

        /// <summary>
        /// All columns allowed in the import.
        /// </summary>
        private static readonly string[] AllColumns = new string[]
        {
            ImportExportConstants.PROJECTMAP_WBS_NUMBER_COLUMN_HEADER,
            ImportExportConstants.ACTIVITY_ID_COLUMN_HEADER,
            ImportExportConstants.ACTIVITY_NAME_COLUMN_HEADER,
            ImportExportConstants.WBS_ELEMENT_TITLE_COLUMN_HEADER,
            ImportExportConstants.INITIAL_RESOURCE_COLUMN_HEADER,
            ImportExportConstants.COST_CENTER_COLUMN_HEADER,
            ImportExportConstants.LEGACY_RESOURCE_COLUMN_HEADER,
            ImportExportConstants.START_DATE_COLUMN_HEADER,
            ImportExportConstants.END_DATE_COLUMN_HEADER,
            ImportExportConstants.CLIN_COLUMN_HEADER,
            ImportExportConstants.SOW_COLUMN_HEADER,
            ImportExportConstants.SOW_TITLE_COLUMN_HEADER,
            ImportExportConstants.TASK_COLUMN_HEADER,
            ImportExportConstants.HOURS_COLUMN_HEADER,
            ImportExportConstants.DOLLARS_COLUMN_HEADER,
            ImportExportConstants.RATIONALE_COLUMN_HEADER,
            ImportExportConstants.BOE_COLUMN_HEADER,
            ImportExportConstants.CAM_NAME_COLUMN_HEADER,
            ImportExportConstants.CATEGORY_COLUMN_HEADER,
            ImportExportConstants.OFFLOAD_COLUMN_HEADER,
            ImportExportConstants.ADD_DELETE_COLUMN_HEADER,
            ImportExportConstants.CLASS_OF_COST_COLUMN_HEADER,
            ImportExportConstants.TIERED_PERCENTAGE_COLUMN_HEADER
        };

        /// <summary>
        /// Array of the columns in the imported file that must contain values.
        /// </summary>
        private static readonly string[] RequiredValueColumns = new string[]
        {
            ImportExportConstants.PROJECTMAP_WBS_NUMBER_COLUMN_HEADER,
            ImportExportConstants.ACTIVITY_ID_COLUMN_HEADER,
            ImportExportConstants.ACTIVITY_NAME_COLUMN_HEADER,
            ImportExportConstants.WBS_ELEMENT_TITLE_COLUMN_HEADER,
            ImportExportConstants.INITIAL_RESOURCE_COLUMN_HEADER,
            ImportExportConstants.COST_CENTER_COLUMN_HEADER,
            ImportExportConstants.START_DATE_COLUMN_HEADER,
            ImportExportConstants.END_DATE_COLUMN_HEADER,
            ImportExportConstants.CLIN_COLUMN_HEADER,
            ImportExportConstants.CLASS_OF_COST_COLUMN_HEADER
        };

        /// <summary>
        /// Array of the columns in the imported file that must contain unique values.
        /// </summary>
        private static readonly string[] UniqueValueColumns = new string[] { };

        /// <summary>
        /// Array of the columns that must be parsed as text.
        /// </summary>
        private static readonly string[] TextOnlyColumns = new string[]
        {

            ImportExportConstants.PROJECTMAP_WBS_NUMBER_COLUMN_HEADER,
            ImportExportConstants.ACTIVITY_ID_COLUMN_HEADER,
            ImportExportConstants.ACTIVITY_NAME_COLUMN_HEADER,
            ImportExportConstants.WBS_ELEMENT_TITLE_COLUMN_HEADER,
            ImportExportConstants.INITIAL_RESOURCE_COLUMN_HEADER,
            ImportExportConstants.COST_CENTER_COLUMN_HEADER,
            ImportExportConstants.LEGACY_RESOURCE_COLUMN_HEADER,
            ImportExportConstants.CLIN_COLUMN_HEADER,
            ImportExportConstants.SOW_COLUMN_HEADER,
            ImportExportConstants.SOW_TITLE_COLUMN_HEADER,
            ImportExportConstants.TASK_COLUMN_HEADER,
            ImportExportConstants.RATIONALE_COLUMN_HEADER,
            ImportExportConstants.BOE_COLUMN_HEADER,
            ImportExportConstants.CAM_NAME_COLUMN_HEADER,
            ImportExportConstants.CATEGORY_COLUMN_HEADER,
            ImportExportConstants.OFFLOAD_COLUMN_HEADER,
            ImportExportConstants.ADD_DELETE_COLUMN_HEADER,
            ImportExportConstants.CLASS_OF_COST_COLUMN_HEADER
        };
        
        /// <summary>
        /// The logger for the class.
        /// </summary>
        private static Logger logger = new Logger(typeof(ProjectMapImporter));

        #endregion Constants

        #region Public Functions

        /// <summary>
        /// Returns a collection of Project Map ModelViews.
        /// </summary>
        /// <param name="excelFileStream">A file stream holding the uploaded data from the user</param>
        /// <param name="projectMapType">The type of project map for this workspace.</param>
        /// <returns>Collection of new Project Map ModelView objects.</returns>
        public static ICollection<ProjectMapModelView> ImportFromExcelFile(Stream excelFileStream, ProjectMapType projectMapType)
        {
            ICollection<Dictionary<string, string>> allRows = GetAllRows(excelFileStream, projectMapType);

            // Turn each row into a DTO object and return the collection
            return CreateDTOsToReturn(allRows, projectMapType);
        }

        /// <summary>
        /// Validates Legacy Resources
        /// </summary>
        /// <param name="excelFileStream">Excel Stream</param>
        /// <param name="projectMapType">Project Map Type</param>
        /// <returns>Resulting data</returns>
        public static Collection<ValidationMessage> ValidateLegacyResources(Stream excelFileStream, ProjectMapType projectMapType)
        {
            ICollection<Dictionary<string, string>> allRows = GetAllRows(excelFileStream, projectMapType);

            ICommonDataMapper commonDataMapper = IES.Common.classes.GenBOEUnityContainer.Resolve<ICommonDataMapper>();
            IDictionary<string, SikorskyLegacyResourceDTO> allLegacyResources = commonDataMapper.GetSikorskyLegacyResourcesConversionDictionary();
            Collection<ValidationMessage> errors = new Collection<ValidationMessage>();

            foreach (Dictionary<string, string> row in allRows)
            {
                if (row.ContainsKey(ImportExportConstants.LEGACY_RESOURCE_COLUMN_HEADER))
                {
                    int? temp = commonDataMapper.GetSikorskyLegacyID(row[ImportExportConstants.LEGACY_RESOURCE_COLUMN_HEADER], allLegacyResources);

                    if (!temp.HasValue && !string.IsNullOrEmpty(row[ImportExportConstants.LEGACY_RESOURCE_COLUMN_HEADER]))
                    {
                        errors.Add(new ValidationMessage()
                        {
                            TreatAsWarning = false,
                            ValidationIssue = "Legacy Resource is invalid",
                            FieldName = row[ImportExportConstants.LEGACY_RESOURCE_COLUMN_HEADER]
                        });
                    }
                }
            }

            return errors;
        }

        #endregion Public Functions

        #region Private Functions

        /// <summary>
        /// Gets all rows to process
        /// </summary>
        /// <param name="excelFileStream">File Stream</param>
        /// <param name="projectMapType">Project map type</param>
        /// <returns></returns>
        private static ICollection<Dictionary<string, string>> GetAllRows(Stream excelFileStream, ProjectMapType projectMapType)
        {
            ICollection<Dictionary<string, string>> allRows;
            try
            {
                // Open the document as read-only.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                {
                    // Get a collection of all rows in the file, filtering out rows that only have data in
                    // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                    // in an enumerable collection

                    List<string> requiredColumns = new List<string>(RequiredColumns);

                    List<string> allColumns = new List<string>(AllColumns);

                    // update required columns for project map type
                    if (projectMapType == ProjectMapType.NonTimePhasedProjectMap)
                    {
                        requiredColumns.Add(ImportExportConstants.RATIONALE_COLUMN_HEADER);
                    }
                    else
                    {
                        // Add 17 years of months for bucketized/time-phased project maps
                        for (int i = 1; i <= 204; i++)
                        {
                            allColumns.Add("M" + i.ToString());
                            requiredColumns.Add("M" + i.ToString());
                        }

                        requiredColumns.Add(ImportExportConstants.BOE_COLUMN_HEADER);
                    }

                    allRows = GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, requiredColumns.ToArray(),
                        allColumns.ToArray(), RequiredValueColumns, UniqueValueColumns, TextOnlyColumns);

                    int maximum;
                    if (!int.TryParse(WebConfigurationManager.AppSettings["MaximumRowsInProjectMap"], out maximum))
                    {
                        maximum = 15000;
                    }

                    if (allRows.Count > maximum)
                    {
                        throw new GenValidationException($"You are only allowed to import maximum of {maximum} rows.");
                    }
                }
            }
            catch (FileFormatException)
            {
                logger.Error("Imported Project Maps file was an incorrect format.");
                throw new NotExcelFileException();
            }
            catch (ColumnMissingException)
            {
                logger.Error("Imported Project Maps file was missing a required column.");
                throw;
            }
            catch (CellValueMissingException)
            {
                logger.Error("Imported Project Maps file was missing a required cell value.");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
            return allRows;
        }

        /// <summary>
        /// Converts a collection of Key/Value pair Dictionary objects into a collection of DTOs.
        /// </summary>
        /// <param name="allRows">Collection of Key/Value pair Dictionary objects representing imported rows.</param>
        /// <param name="projectMapType">The type of project map for this workspace.</param>
        /// <returns>A collection of DTOs representing the newly imported values</returns>
        private static ICollection<ProjectMapModelView> CreateDTOsToReturn(ICollection<Dictionary<string, string>> allRows, ProjectMapType projectMapType)
        {
            // Create the collection to return
            List<ProjectMapModelView> toReturn = new List<ProjectMapModelView>();

            // Retrieve the Sikorsky Legacy Resources.
            ICommonDataMapper commonDataMapper = IES.Common.classes.GenBOEUnityContainer.Resolve<ICommonDataMapper>();
            IDictionary<string, SikorskyLegacyResourceDTO> allLegacyResources = commonDataMapper.GetSikorskyLegacyResourcesConversionDictionary();

            // For each Dictionary object (representing imported row data)
            foreach (Dictionary<string, string> row in allRows)
            {
                ProjectMapModelView projectMapRow = new ProjectMapModelView();

                // Add the new Project Map to the collection to be returned
                toReturn.Add(projectMapRow);

                // Set the Year and other Rate Properties
                projectMapRow.InitialResource = row[ImportExportConstants.INITIAL_RESOURCE_COLUMN_HEADER];
                projectMapRow.ActivityID = row[ImportExportConstants.ACTIVITY_ID_COLUMN_HEADER];
                projectMapRow.WbsNumber = row[ImportExportConstants.PROJECTMAP_WBS_NUMBER_COLUMN_HEADER];
                projectMapRow.ActivityName = row[ImportExportConstants.ACTIVITY_NAME_COLUMN_HEADER];
                projectMapRow.WbsElementTitle = row[ImportExportConstants.WBS_ELEMENT_TITLE_COLUMN_HEADER];
                projectMapRow.CostCenter = row[ImportExportConstants.COST_CENTER_COLUMN_HEADER];
                if (row.ContainsKey(ImportExportConstants.LEGACY_RESOURCE_COLUMN_HEADER))
                {
                    projectMapRow.LegacyID = commonDataMapper.GetSikorskyLegacyID(row[ImportExportConstants.LEGACY_RESOURCE_COLUMN_HEADER], allLegacyResources);
                }
                
                DateTime tmpDate;
                if (DateTime.TryParse(row[ImportExportConstants.START_DATE_COLUMN_HEADER], out tmpDate))
                {
                    projectMapRow.StartDate = tmpDate.Normalize(DateTimePrecision.Month);
                }

                if (DateTime.TryParse(row[ImportExportConstants.END_DATE_COLUMN_HEADER], out tmpDate))
                {
                    projectMapRow.EndDate = tmpDate.Normalize(DateTimePrecision.Month);
                }
 
                projectMapRow.Clin = row[ImportExportConstants.CLIN_COLUMN_HEADER];
                projectMapRow.ClassOfCost = row[ImportExportConstants.CLASS_OF_COST_COLUMN_HEADER];
                if (row.ContainsKey(ImportExportConstants.SOW_COLUMN_HEADER))
                {
                    projectMapRow.SowNumber = row[ImportExportConstants.SOW_COLUMN_HEADER];
                }

                if (row.ContainsKey(ImportExportConstants.SOW_TITLE_COLUMN_HEADER))
                {
                    projectMapRow.SowTitle = row[ImportExportConstants.SOW_TITLE_COLUMN_HEADER];
                }

                if (row.ContainsKey(ImportExportConstants.TASK_COLUMN_HEADER))
                {
                    projectMapRow.Task = row[ImportExportConstants.TASK_COLUMN_HEADER];
                }

                if (row.ContainsKey(ImportExportConstants.CAM_NAME_COLUMN_HEADER))
                {
                    projectMapRow.CamName = row[ImportExportConstants.CAM_NAME_COLUMN_HEADER];
                }

                if (row.ContainsKey(ImportExportConstants.CATEGORY_COLUMN_HEADER))
                {
                    projectMapRow.Category = row[ImportExportConstants.CATEGORY_COLUMN_HEADER];
                }

                if (row.ContainsKey(ImportExportConstants.ADD_DELETE_COLUMN_HEADER))
                {
                    projectMapRow.AddDelete = row[ImportExportConstants.ADD_DELETE_COLUMN_HEADER] ?? ADD_DELETE_DEFAULT; // default to 'A' if the value is null
                }
                else
                {
                    projectMapRow.AddDelete = ADD_DELETE_DEFAULT;
                }

                if (row.ContainsKey(ImportExportConstants.HOURS_COLUMN_HEADER))
                {
                    decimal hours;
                    if (decimal.TryParse(row[ImportExportConstants.HOURS_COLUMN_HEADER], out hours))
                    {
                        projectMapRow.Hours = hours;
                    }
                }

                if (row.ContainsKey(ImportExportConstants.DOLLARS_COLUMN_HEADER))
                {
                    string dollarsString = row[ImportExportConstants.DOLLARS_COLUMN_HEADER];
                    if (!string.IsNullOrWhiteSpace(dollarsString))
                    {
                        dollarsString = dollarsString.Replace("$", string.Empty);
                        decimal dollars;
                        if (decimal.TryParse(dollarsString, out dollars))
                        {
                            projectMapRow.Dollars = dollars;
                        }
                    }
                }

                if (row.ContainsKey(ImportExportConstants.OFFLOAD_COLUMN_HEADER))
                {
                    // original Sikorsky template stored values in boolean Excel format which is 0/1.  bool.parse does not like it, so check for that first just in case they copy/paste from old templates
                    if (row[ImportExportConstants.OFFLOAD_COLUMN_HEADER] == "1")
                    {
                        projectMapRow.Offload = true;
                    }
                    else if (row[ImportExportConstants.OFFLOAD_COLUMN_HEADER] == "0")
                    {
                        projectMapRow.Offload = false;
                    }
                    else
                    {
                        projectMapRow.Offload = bool.Parse(row[ImportExportConstants.OFFLOAD_COLUMN_HEADER].ToLower());
                    }
                }

                if (projectMapType == ProjectMapType.NonTimePhasedProjectMap)
                {
                    if (row.ContainsKey(ImportExportConstants.RATIONALE_COLUMN_HEADER))
                    {
                        projectMapRow.Rationale = row[ImportExportConstants.RATIONALE_COLUMN_HEADER];
                    }
                }
                else
                {
                    if (row.ContainsKey(ImportExportConstants.BOE_COLUMN_HEADER))
                    {
                        projectMapRow.Rationale = row[ImportExportConstants.BOE_COLUMN_HEADER];
                    }

                    // 17 years of months for bucketized/time-phased project maps
                    projectMapRow.DiscreteMonths = new decimal?[204];
                    for (int i = 1; i <= 204; i++)
                    {
                        string rowHeader = "M" + i.ToString();
                        decimal monthValue;
                        if (row.ContainsKey(rowHeader) && decimal.TryParse(row[rowHeader], out monthValue))
                        {
                            projectMapRow.DiscreteMonths[i - 1] = monthValue;
                        }
                    }
                }

                if (row.ContainsKey(ImportExportConstants.TIERED_PERCENTAGE_COLUMN_HEADER))
                {
                    decimal tieredPercentage;
                    if (decimal.TryParse(row[ImportExportConstants.TIERED_PERCENTAGE_COLUMN_HEADER], out tieredPercentage))
                    {
                        projectMapRow.TieredPercentage = tieredPercentage;
                    }
                }
            }
            // Return the collection of new DTOs
            return toReturn;
        }

        #endregion Private Functions
    }
}