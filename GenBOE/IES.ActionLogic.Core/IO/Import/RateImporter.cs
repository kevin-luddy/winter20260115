// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Web;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using IES.Core.Exceptions;
    using IES.Core.OfficeUtilities;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// The Rate Importer.
    /// </summary>
    public static class RateImporter
    {
        /// <summary>
        /// Retrieve rates From Excel File.
        /// </summary>
        /// <param name="excelFile">Excel file to import.</param>
        /// <param name="importedRateCodes">Rate Codes that were found in the import file, used to filter GetRatesByRevision.</param>
        /// <returns>Collection of imported rates for the current version.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public static Collection<RateDetailModelView> GetRatesFromExcelFile(Stream excelFile, out Collection<string> importedRateCodes)
        {
            Collection<RateDetailModelView> importRateDetails = new Collection<RateDetailModelView>();
            importedRateCodes = new Collection<string>();
            if (excelFile == null)
            {
                throw new GenValidationException("Import file is empty.");
            }

            try
            {
                // Open the document as read-only.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFile, false))
                {
                    WorkbookPart workbook = document.WorkbookPart;

                    // Get the specified worksheet part
                    foreach (Sheet worksheet in workbook.Workbook.Descendants<Sheet>())
                    {
                        // If we found the worksheet, load it.
                        if (worksheet != null)
                        {
                            WorksheetPart wsPart = (WorksheetPart)workbook.GetPartById(worksheet.Id);

                            Row headerRow = wsPart.Worksheet.Descendants<Row>().First();
                            ICollection<Row> dataRows = wsPart.Worksheet.Descendants<Row>().Skip(1).ToList();
                            Dictionary<string, string> headerDictionary = new Dictionary<string, string>();

                            // Get a collection of shared strings in the document for pulling out cell values
                            SharedStringTablePart sharedStringPart = workbook.GetPartsOfType<SharedStringTablePart>().First();
                            SharedStringItem[] sharedStringItems = sharedStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();
                            Stylesheet stylesheet = workbook.WorkbookStylesPart.Stylesheet;

                            // Process header row.
                            foreach (Cell cell in headerRow)
                            {
                                // Create Rate Year list.
                                headerDictionary.Add(ExcelUtilities.ParseColumnName(cell.CellReference), ExcelUtilities.GetCellValue(cell, sharedStringItems, stylesheet));
                            }

                            if (dataRows.Any())
                            {
                                foreach (Row row in dataRows)
                                {
                                    RateDetailModelView newRateDetail = new RateDetailModelView();
                                    // Create Rate Detail
                                    foreach (Cell cell in row.Descendants<Cell>())
                                    {
                                        string columnName = headerDictionary[ExcelUtilities.ParseColumnName(cell.CellReference)];
                                        switch (columnName)
                                        {
                                            case "Rate Code":
                                                newRateDetail.RateCode = ExcelUtilities.GetCellValue(cell, sharedStringItems, stylesheet);
                                                if (!string.IsNullOrEmpty(newRateDetail.RateCode))
                                                {
                                                    importedRateCodes.Add(newRateDetail.RateCode);
                                                    newRateDetail.Dirty = true;         // Rate Code is the only required field, if not Dirty, then row is invalid.
                                                }

                                                break;
                                            default:
                                                // Years
                                                int year;

                                                if (int.TryParse(columnName, out year))
                                                {
                                                    RateYearModelView newRateYearMV = new RateYearModelView();
                                                    decimal newValue;
                                                    newRateYearMV.Year = year;

                                                    // check the cell's format to make sure it is not accounting
                                                    if (ExcelUtilities.CheckCellFormatForAccounting(cell, stylesheet))
                                                    {
                                                        throw new GenValidationException(string.Format("The style for cell {0} was set to Accounting.  Please change this to Currency.", cell.CellReference));
                                                    }

                                                    string cellValue = ExcelUtilities.GetCellValue(cell, sharedStringItems, stylesheet);
                                                    cellValue = cellValue.Replace("$", string.Empty);
                                                    if (!string.IsNullOrEmpty(cellValue) && decimal.TryParse(cellValue, out newValue))
                                                    {
                                                        // If the excel precision blew up, round the number.
                                                        string newValueString = newValue.ToString();
                                                        int decimalIndex = newValueString.IndexOf('.');
                                                        if (newValueString.Length > 15 && decimalIndex >= 0)
                                                        {
                                                            int decimalOffset = decimalIndex + 2;  // Add 1 for the '.' and 1 to strip last character.
                                                            newRateYearMV.Value = Math.Round(newValue, newValueString.Length - decimalOffset);
                                                            double dropZeros = (double)newRateYearMV.Value;
                                                            newRateYearMV.Value = (decimal)dropZeros;
                                                        }
                                                        else
                                                        {
                                                            newRateYearMV.Value = newValue;
                                                        }

                                                        newRateYearMV.Dirty = true;
                                                    }

                                                    if (newRateDetail.Values == null)
                                                    {
                                                        newRateDetail.Values = new Collection<RateYearModelView>();
                                                    }

                                                    newRateDetail.Values.Add(newRateYearMV);
                                                }
                                                else
                                                {
                                                    throw new GenValidationException(String.Format("Invalid Column Header: {0}", columnName));
                                                }

                                                break;
                                        }
                                    }

                                    if (newRateDetail.Dirty)
                                    {
                                        importRateDetails.Add(newRateDetail);
                                    }
                                    else if (newRateDetail.Values != null && newRateDetail.Values.Any(x => x.Dirty))
                                    {
                                        throw new GenValidationException("Rate Code column is required for Import.");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (FileFormatException)
            {
                throw new GenValidationException("Imported file was an incorrect format, must be .xlsx or .xlsm file.");
            }

            return importRateDetails;
        }              
    }
}
