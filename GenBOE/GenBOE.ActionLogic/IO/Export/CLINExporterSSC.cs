// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.ActionLogic.Common;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;
    using IES.Common.PickList;

    [ExcludeFromCodeCoverage]
    public class CLINExporterSSC : ICLINExporter
    {
        /// <summary>
        /// Exports CLINs to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Clin Excel file template</param>
        /// <param name="clinDTOs">The collection of CLINs to export</param>
        /// <param name="workspace">The workspace for the CLINs.</param>
        /// <param name="contractTypes">All of the contract types.</param>
        /// <returns>Path to the exported Clin file</returns>
        public string ExportToExcelFile(string templateFileLocation, Collection<ClinDTO> clinDTOs, WorkspaceDTO workspace, ICollection<PickListDto> contractTypes)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }
            if (clinDTOs == null)
            {
                throw new ArgumentNullException(nameof(clinDTOs));
            }
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

			ExcelExportWorksheet worksheet = this.GetExcelExportWorksheet(clinDTOs, contractTypes);
			ExcelExportWorksheet optionsSheet = this.PopulateOptionsList(workspace, contractTypes);

            string toReturn = ExcelUtilities.CopyExcelTemplateFile(templateFileLocation);

            // Create the document object in memory
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(toReturn, true))
            {
                // Get the specified worksheet part
                WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, worksheet.WorksheetName);
                ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, worksheet, 2);
                this.AddDataValidation(worksheetPart, worksheet.Count);

                // Get the options specified worksheet part
                worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, optionsSheet.WorksheetName);
                ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, optionsSheet, 1);


                // Adjust existing Defined Names
                Dictionary<string, int> lengths = new Dictionary<string, int>()
                {
                    { "ContractTypes", workspace.SelectedContractTypes.Count + 1 }
                };
                ExcelExporter.AdjustDefinedNames(spreadsheet, lengths);
            }

            // Return the file path
            return toReturn;
        }

        /// <summary>
        /// Gets the excel export worksheet.
        /// </summary>
        /// <param name="clinDTOs">The clin dtos.</param>
        /// <returns>New Excel Export worksheet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "ExcelExportWorksheet's base class is List<T> but Collection<T> does not have AddRange()")]
        public ExcelExportWorksheet GetExcelExportWorksheet(Collection<ClinDTO> clinDTOs, ICollection<PickListDto> contractTypes)
        {
            if (clinDTOs == null)
            {
                throw new ArgumentNullException(nameof(clinDTOs));
            }

			ExcelExportWorksheet toReturn = new ExcelExportWorksheet(ImportExportConstants.CLINS);

            if (clinDTOs.Count > 0)
            {
                foreach (ClinDTO clin in clinDTOs)
                {
                    toReturn.Add(
                        clin.Id.ToString(),
                        CommonConstants.FORCE_AS_STRING_VALUE + clin.ClinNumber,
                        CommonConstants.FORCE_AS_STRING_VALUE + clin.ClinTitle,
                        String.Format("{0:M/yyyy}", clin.StartDate),
                        String.Format("{0:M/yyyy}", clin.EndDate),
                        Utilities.GetPickListText(clin.ContractType, contractTypes, Constants.CONTRACT_TYPE_NOT_SET_STRING)
                        );
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Populates the options list worksheet.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <returns></returns>
        private ExcelExportWorksheet PopulateOptionsList(WorkspaceDTO workspace, ICollection<PickListDto> contractTypes)
        {
            // Create collections of strings for each row in the export file
            ExcelExportWorksheet optionsListWorksheet = new ExcelExportWorksheet(ImportExportConstants.OPTIONS_LISTS);

            // Options List Column Headers
            optionsListWorksheet.Add(new string[] { ImportExportConstants.CLIN_CONTRACT_TYPES_OPTIONS_HEADER });

            // Default Option for Contract Types
            optionsListWorksheet.Add(new string[] { Constants.CONTRACT_TYPE_NOT_SET_STRING });

            // Add option value rows
            if (workspace.SelectedContractTypes != null && workspace.SelectedContractTypes.Any())
            {
                foreach (int contract in workspace.SelectedContractTypes)
                {
                    string text = Utilities.GetPickListText(contract, contractTypes, Constants.CONTRACT_TYPE_NOT_SET_STRING);
                    optionsListWorksheet.Add(new string[] { text });
                }
            }

            return optionsListWorksheet;
        }

        /// <summary>
        /// Adds the data validation.
        /// </summary>
        /// <param name="worksheetPart">The worksheet part.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void AddDataValidation(WorksheetPart worksheetPart, int count)
        {
            uint startDataRowIndex = 2;
            uint endDataRowIndex = (uint)(count + 11); // all of the data rows and ten extra

            Dictionary<string, string> dataValidationReferences = new Dictionary<string, string>();
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.CLIN_CONTRACT_TYPES_OPTIONS_HEADER,
                (ImportExportConstants.CLIN_CONTRACT_TYPE_COLUMN_OFFSET), startDataRowIndex, endDataRowIndex);

            ExcelExporter.AddDataValidations(dataValidationReferences, worksheetPart);
        }
    }
}
