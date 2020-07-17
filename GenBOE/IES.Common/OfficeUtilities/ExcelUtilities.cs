// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.OfficeUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using IES.Common.classes;

    [ExcludeFromCodeCoverage]
    public static class ExcelUtilities
    {
        private const double NEW_COLUMN_WIDTH = 9;
        public const string CUSTOM_FIELD_IMPORT_EXPORT_PREFIX = "CF - ";

        // Create static Regex objects.
        private static Regex columnNameRegex = new Regex("[A-Za-z]+", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex rowIndexRegex = new Regex(@"\d+", RegexOptions.None, Constants.REGEX_TIMEOUT);
        private static Regex commentRowIndexRegex = new Regex("[0-9]+", RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Takes the name of a worksheet and returns that worksheet part from the given Excel document. An empty
        /// worksheet name will return the first sheet.
        /// </summary>
        /// <param name="document">The Excel document to parse</param>
        /// <param name="worksheetName">The name of the worksheet to search, or string.empty to use the first sheet</param>
        /// <returns>The specified, or default, worksheet part</returns>
        public static WorksheetPart GetSpecifiedWorksheetPart(SpreadsheetDocument document, string worksheetName)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            // Find the worksheet to parse, or grab the first one if no worksheet name was specified
            Sheet sheet = null;

            if (!string.IsNullOrEmpty(worksheetName))
            {
                sheet = document.WorkbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name != null && s.Name.HasValue && s.Name.Value.Equals(worksheetName, StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                sheet = document.WorkbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
            }

            if (sheet == null)
            {
                // The specified worksheet does not exist.
                return null;
            }

            WorksheetPart toReturn = (WorksheetPart)document.WorkbookPart.GetPartById(sheet.Id);
            
            // Set the worksheet that we'll be parsing
            return toReturn;
        }

        /// <summary>
        /// Removes the first N rows from a worksheet.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="v">The v.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public static void RemoveFirstRows(WorksheetPart worksheetPart, int numRowsToRemove)
        {
            if (worksheetPart == null)
            {
                throw new ArgumentNullException(nameof(worksheetPart));
            }

            if (numRowsToRemove <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numRowsToRemove), numRowsToRemove, "numRowsToRemove must not be a negative number.");
            }
            
            for (int i = 0; i < numRowsToRemove; i++)
            {
                Row firstRow = worksheetPart.Worksheet.Descendants<Row>().First();
                firstRow.Remove();
            }
        }

        /// <summary>
        /// Get all populated rows in the file, using the headerString collection to specify which column values to return.
        /// </summary>
        /// <param name="document">The Excel document to parse</param>
        /// <param name="worksheetName">The name of the worksheet to parse, or string.empty to use the first sheet</param>
        /// <param name="requiredColumns">List of columns that are required to be present in the import file</param>
        /// <param name="allColumns">List of all column names to retrieve values for</param>
        /// <returns>A collection of imported string collections. The primary collection represents each row in the import
        /// file, and each sub collection represents each cell in that row.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static ICollection<Dictionary<string, string>> GetAllRowsFilteredBySpecifiedHeaders(
            SpreadsheetDocument document,
            string worksheetName,
            string[] requiredColumns,
            string[] allColumns)
        {
            return GetAllRowsFilteredBySpecifiedHeaders(
                document,
                worksheetName,
                requiredColumns,
                allColumns,
                null,
                null);
        }

        /// <summary>
        /// Get all populated rows in the file, using the headerString collection to specify which column values to return.
        /// </summary>
        /// <param name="document">The Excel document to parse</param>
        /// <param name="worksheetName">The name of the worksheet to parse, or string.empty to use the first sheet</param>
        /// <param name="requiredColumns">List of columns that are required to be present in the import file</param>
        /// <param name="allColumns">List of all column names to retrieve values for</param>
        /// <param name="requiredValueColumns">List of columns that must contain a value in each imported row</param>
        /// <returns>A collection of imported string collections. The primary collection represents each row in the import
        /// file, and each sub collection represents each cell in that row.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static ICollection<Dictionary<string, string>> GetAllRowsFilteredBySpecifiedHeaders(
            SpreadsheetDocument document,
            string worksheetName,
            string[] requiredColumns,
            string[] allColumns,
            string[] requiredValueColumns)
        {
            return GetAllRowsFilteredBySpecifiedHeaders(
                document,
                worksheetName,
                requiredColumns,
                allColumns,
                requiredValueColumns,
                null);
        }

        /// <summary>
        /// Get all populated rows in the file, using the headerString collection to specify which column values to return.
        /// </summary>
        /// <param name="document">The Excel document to parse</param>
        /// <param name="worksheetName">The name of the worksheet to parse, or string.empty to use the first sheet</param>
        /// <param name="requiredColumns">List of columns that are required to be present in the import file</param>
        /// <param name="allColumns">List of all column names to retrieve values for</param>
        /// <param name="requiredValueColumns">List of columns that must contain a value in each imported row</param>
        /// <param name="uniqueValueColumns">List of columns that must have unique values across all imported rows</param>
        /// <param name="textOnlyValueColumns">Optional list of columns that should be treated as text only (and not be converted to numerical values).</param>
        /// <returns>A collection of imported string collections. The primary collection represents each row in the import
        /// file, and each sub collection represents each cell in that row.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static ICollection<Dictionary<string, string>> GetAllRowsFilteredBySpecifiedHeaders(
            SpreadsheetDocument document,
            string worksheetName,
            string[] requiredColumns,
            string[] allColumns,
            string[] requiredValueColumns,
            string[] uniqueValueColumns,
            string[] textOnlyValueColumns = null)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            if (allColumns == null)
            {
                throw new ArgumentNullException(nameof(allColumns));
            }

            // Initialize a collection of Key/Value pair Dictionary objects that will hold the rows of imported data
            Collection<Dictionary<string, string>> toReturn = new Collection<Dictionary<string, string>>();

            // Get the specified worksheet part
            WorksheetPart worksheetPart = GetSpecifiedWorksheetPart(document, worksheetName);

            // If we found the worksheet, let's move on. Otherwise bomb out.
            if (worksheetPart != null)
            {
                // Get column names from the list of header strings passed to this function
                var columns = new Dictionary<string, string>();

                // Create a list to hold names of columns that aren't found
                string headersNotFound = string.Empty;

                foreach (string headerString in allColumns)
                {
                    try
                    {
                        columns.Add(
                            GetColumnNameFromHeaderString(
                                document,
                                worksheetName,
                                headerString),
                            headerString);
                    }
                    catch (ColumnMissingException)
                    {
                        if (requiredColumns != null &&
                            requiredColumns.Contains(headerString))
                        {
                            // If the column isn't found, add the header name to the not found list and continue
                            headersNotFound += String.Format("'{0}', ", headerString);
                        }
                    }
                }

                // If some headers were not found, pass on the ColumnMissingException 
                if (headersNotFound.Length > 0)
                {
                    throw new ColumnMissingException(headersNotFound.Trim().TrimEnd(','));
                }

                // Initialize a cache to hold values so we can stop if we find any matching values in unique columns
                var valueCache = new Dictionary<string, List<string>>();
                
                // Create a list to hold names rows with duplicate values in unique columns
                Collection<string> uniqueValueExceptionMessages = new Collection<string>();

                if (uniqueValueColumns != null)
                {
                    foreach (var uniqueValueColumn in uniqueValueColumns)
                    {
                        valueCache.Add(uniqueValueColumn, new List<string>());
                    }
                }

                // Get all rows in the file AFTER THE HEADER ROW which contain a value in one of the columns specified
                // in the headerStrings params of this function. This filters out rows that have data filled in for columns
                // that aren't pertinent to the import.
                // The logic here is as follows: Get all rows whose index is greater than 1 (the header row) and in which
                //     there exists a cell in one of the columns specified in the headerString parameter that has a value
                //     consisting of more than just blank spaces.
                ICollection<Row> allRows = worksheetPart.Worksheet.Descendants<Row>().Where(
                    r => r.RowIndex != 1 &&
                         r.Descendants<Cell>().Any(
                             c => columns.Keys.Contains(ParseColumnName(c.CellReference.Value))
                                 && c.CellValue != null
                                 && (c.DataType != null || (c.DataType == null && c.CellValue.Text.Trim().Length > 0)))).ToList();
                
                if (allRows.Any())
                {
                    // Get a collection of shared strings in the document for pulling out cell values
                    SharedStringTablePart shareStringPart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
                    SharedStringItem[] sharedStringItems = shareStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();
                    Stylesheet stylesheet = document.WorkbookPart.WorkbookStylesPart.Stylesheet;

                    foreach (Row row in allRows)
                    {
                        Dictionary<string, string> rowToReturn = new Dictionary<string, string>();

                        foreach (var column in columns)
                        {
                            // Get the cell value for the current column and row
                            Cell cell = row.Descendants<Cell>().FirstOrDefault(c => ParseColumnName(c.CellReference.Value).Equals(column.Key, StringComparison.CurrentCultureIgnoreCase));
                            string cellValue = string.Empty;

                            // If the cell for the current column and row is found, get it's value
                            if (cell != null)
                            {
                                cellValue = GetCellValue(cell, sharedStringItems, stylesheet, textOnlyValueColumns != null && textOnlyValueColumns.Contains(column.Value));
                            }

                            // If the cell was null, or an empty string
                            if (cell == null || cellValue.Length == 0)
                            {
                                // If the null/empty cell was required
                                if (requiredValueColumns != null && requiredValueColumns.Contains(column.Value))
                                {
                                    // Pass up an exception to indicate that the cell must have a value
                                    throw new CellValueMissingException(column.Value);
                                }

                            }
                            else
                            {
                                // If everything checks out with the cell value, add it to the return dictionary
                                rowToReturn.Add(column.Value, cellValue);
                            }

                            if (valueCache.Count > 0 &&
                                cellValue.Length > 0 &&
                                valueCache.ContainsKey(column.Value))
                            {
                                valueCache[column.Value].Add(cellValue);
                            }
                        }

                        // If the row contained good data, add it to the return collection
                        if (rowToReturn.Count > 0)
                        {
                            toReturn.Add(rowToReturn);
                        }
                    }

                    // Throw exception for duplicate values in unique value coilumns
                    if (valueCache.Count > 0)
                    {
                        foreach (var uniqueValueColumn in valueCache)
                        {
                            // Get all duplicate values in the column
                            var duplicateValues = from value in uniqueValueColumn.Value
                                                  group value by value.Trim().ToLower() into groupedValues
                                                  where groupedValues.Count() > 1
                                                  select groupedValues.Key;

                            if (duplicateValues.Any())
                            {
                                // If the column has duplicates, add the header name and the duplicate values found to the
                                // exception message list and continue
                                uniqueValueExceptionMessages.Add(String.Format("{0}: {1}", uniqueValueColumn.Key, String.Join(", ", duplicateValues)));
                            }
                        }

                        // If some headers were not found, pass on the ColumnMissingException 
                        if (uniqueValueExceptionMessages.Count > 0)
                        {

                            throw new DuplicateValuesException(String.Join(" - ", uniqueValueExceptionMessages), uniqueValueExceptionMessages);
                        }
                    }
                }
            }
            
            return toReturn;
        }

        /// <summary>
        /// Given a worksheet and a row index, gets the row at the specified index.
        /// </summary>
        /// <param name="worksheetPart">Worksheet part</param>
        /// <param name="rowIndex">Row index</param>
        /// <returns>Row</returns>
        internal static Row GetRow(WorksheetPart worksheetPart, uint rowIndex)
        {
            return worksheetPart.Worksheet.GetFirstChild<SheetData>().Elements<Row>().Where(r => r.RowIndex == rowIndex).SingleOrDefault();
        }

        /// <summary>
        /// Sets the CF - prefix for exports and reads the import
        /// </summary>
        /// <param name="customFieldName">custom field name to set</param>
        /// <returns></returns>
        public static string SetPrefixCustomField(string customFieldName)
        {
            return CUSTOM_FIELD_IMPORT_EXPORT_PREFIX + customFieldName;
        }
        /// <summary>
        /// Sets the CF - prefix for required CF for export
        /// </summary>
        /// <param name="customFieldName">custom field name to set</param>
        /// <returns></returns>
        public static string SetPrefixCustomFieldRequired(string customFieldName)
        {
            return SetPrefixCustomField(customFieldName)+"*";
        }

          /// <summary>
        /// Removed the CF - prefix for imports and reads the normal custom field name
        /// </summary>
        /// <param name="customFieldNameWithPreFix"></param>
        /// <returns></returns>
          public static string RemoveCustomFieldPrefix(string customFieldNameWithPreFix)
          {
              if (customFieldNameWithPreFix == null)
              {
                  throw new ArgumentNullException(nameof(customFieldNameWithPreFix));
              }
              //checks to see if the custom field has the prefix CF - if it does it removes the prefix so it can check the names, else do the same. 
            return customFieldNameWithPreFix.StartsWith(CUSTOM_FIELD_IMPORT_EXPORT_PREFIX) ? customFieldNameWithPreFix.Substring(CUSTOM_FIELD_IMPORT_EXPORT_PREFIX.Length) : customFieldNameWithPreFix;
          }

        /// <summary>
        /// Returns a cell Object corresponding to a specific address on the worksheet
        /// </summary>
        /// <param name="worksheetPart">Worksheet part to search for cell address</param>
        /// <param name="cellReference">Cell reference (ie. B2)</param>
        /// <returns>Cell Object</returns>
        public static Cell GetCell(WorksheetPart worksheetPart, string cellReference)
        {
            if (worksheetPart == null)
            {
                throw new ArgumentNullException(nameof(worksheetPart));
            }
            return worksheetPart.Worksheet.Descendants<Cell>().SingleOrDefault(c => cellReference.Equals(c.CellReference));
        }

        /// <summary>
        /// Gets the cell value.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="workbookPart">The workbook part.</param>
        /// <returns>The cell value as a string.</returns>
        private static string GetCellValue(Cell cell, WorkbookPart workbookPart)
        {
            string toReturn = string.Empty;

            // If the content of the first cell is stored as a shared string, get the text of the first cell
            // from the SharedStringTablePart and return it. Otherwise, return the string value of the cell.
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                // Get a collection of shared strings in the document for pulling out cell values
                SharedStringTablePart shareStringPart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                SharedStringItem[] sharedStringItems = shareStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();

                toReturn = GetCellValue(cell, sharedStringItems, null);
            }
            else if (cell.CellValue != null)
            {
                toReturn = GetCellValue(cell, null, workbookPart.WorkbookStylesPart.Stylesheet);
            }

            toReturn = toReturn.Trim();

            return toReturn;
        }

        /// <summary>
        /// Gets the cell value.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="sharedStringItems">The shared string items.</param>
        /// <param name="stylesheet">The stylesheet.</param>
        /// <param name="textOnly">Optional.  Whether the cell value should be treated as text only (and not be converted to a numerical value).</param>
        /// <returns>The cell value as a string.</returns>
        public static string GetCellValue(Cell cell, SharedStringItem[] sharedStringItems, Stylesheet stylesheet, bool textOnly = false)
        {
            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }

            string toReturn = string.Empty;

            // If the content of the first cell is stored as a shared string, get the text of the first cell
            // from the SharedStringTablePart and return it. Otherwise, return the string value of the cell.
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                if (cell.CellValue != null)
                {
                    if (sharedStringItems == null)
                    {
                        throw new ArgumentNullException(nameof(sharedStringItems));
                    }
                    toReturn = sharedStringItems[int.Parse(cell.CellValue.Text)].InnerText;

                    // dealing with excel weirdness to avoid numbers such as 2.2000000000000002, 4.0999999999999996
                    // parsing it as a double removes the issue
                    if (!textOnly)
                    {
                        double temp;
                        if (Double.TryParse(toReturn, out temp)) { toReturn = temp.ToString(); }
                    }
                }
            }
            else if (cell.CellValue != null)
            {
                toReturn = GetValueForFormattedCell(cell, stylesheet, textOnly);
            }

            toReturn = toReturn.Trim();

            return toReturn;
        }

        /// <summary>
        /// Attempts to extract a useable value from a formatted Cell. Falls back on the cellValue.Text if
        /// extraction is not possible.
        /// </summary>
        /// <param name="cell">The formatted cell</param>
        /// <param name="stylesheet">The document's stylesheet</param>
        /// <param name="textOnly">Whether the cell value should be treated as text only (and not be converted to a numerical value).</param>
        /// <returns>A string value from the cell contents</returns>
        public static string GetValueForFormattedCell(Cell cell, Stylesheet stylesheet, bool textOnly)
        {
            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }
            if (stylesheet == null)
            {
                throw new ArgumentNullException(nameof(stylesheet));
            }
            // Set the return value to the text in the cell
            string toReturn = cell.CellValue == null ? null : cell.CellValue.Text;

            // If the cell is styled, we'll look for the format in the stylesheet
            if (cell.StyleIndex != null && cell.StyleIndex.HasValue)
            {
                var cellFormat = stylesheet.CellFormats.ElementAtOrDefault((int)cell.StyleIndex.Value);

                // If the style of the cell was found, we'll check for a number format
                if (cellFormat != null)
                {
                    double numericalValue;

                    // If the cell contains a formattable number and is not to be interpreted only as text, we'll continue. Parsing with double because Excel stores numbers as floating point but displays
                    // the number rounded when the floating point number spans many decimal places. By using double it will return the number you see in the spreadsheet. 
                    if (!textOnly && Double.TryParse(toReturn, out numericalValue))
                    {
                        var numberFormatID = (cellFormat as CellFormat).NumberFormatId;

                        // If the number format is defined, and not the General format
                        if (numberFormatID.HasValue)
                        {
                            if (numberFormatID < 164)
                            {
                                
                                switch (numberFormatID.Value)
                                {
                                    // Return Number as Text Format
                                    case 49:
                                        int intValue;
                                        if (!int.TryParse(toReturn, out intValue))
                                        {
                                            toReturn = numericalValue.ToString();
                                        }
                                        break;
                                    // Built-in Number Formats
                                    case 1:
                                        toReturn = numericalValue.ToString("0");
                                        break;
                                    case 2:
                                        toReturn = numericalValue.ToString("0.00");
                                        break;
                                    case 3:
                                        toReturn = numericalValue.ToString("#,##0");
                                        break;
                                    case 4:
                                        toReturn = numericalValue.ToString("#,##0.00");
                                        break;
                                    case 11:
                                        toReturn = numericalValue.ToString("0.00E+00");
                                        break;
                                    case 48:
                                        toReturn = numericalValue.ToString("##0.0E+0");
                                        break;

                                    // Built-in Money formats
                                    case 44:
                                        toReturn = numericalValue.ToString("\"$\"#,##0.00");
                                        break;

                                    // Built-in Percentage formats
                                    case 9:
                                        toReturn = numericalValue.ToString("0%");
                                        break;
                                    case 10:
                                        toReturn = numericalValue.ToString("0.00%");
                                        break;

                                    // Built-in Date formats
                                    case 14:
                                    case 15:
                                    case 16:
                                    case 17:
                                    case 22:
                                        toReturn = DateTime.FromOADate(Convert.ToDouble(numericalValue)).ToString("M/yyyy");
                                        break;

                                    default:
                                        toReturn = numericalValue.ToString();
                                        break;
                                }
                            }
                            // If the numbering format was not a built in format, let's check it manually
                            else
                            {
                                var numberingFormat = (from NumberingFormat n in stylesheet.NumberingFormats
                                                       where n.NumberFormatId.HasValue &&
                                                             n.NumberFormatId.Value == numberFormatID
                                                       select n).FirstOrDefault();

                                if (numberingFormat != null && numberingFormat.FormatCode.HasValue)
                                {
                                    var numberingFormatCode = numberingFormat.FormatCode.Value;

                                    // Check for custom Date format
                                    if (numberingFormatCode.Contains('m') && numberingFormatCode.Contains("yy"))
                                    {
                                        toReturn = DateTime.FromOADate(Convert.ToDouble(numericalValue)).ToString("M/yyyy");
                                    }
                                    else if (numberingFormatCode.Equals("\"$\"#,##0.00", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        toReturn = numericalValue.ToString(numberingFormatCode);
                                    }
                                    else
                                    {
                                        toReturn = numericalValue.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Checks the cell format for currency style.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="stylesheet">The stylesheet.</param>
        public static bool CheckCellFormatForAccounting(Cell cell, Stylesheet stylesheet)
        {
            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }

            if (stylesheet == null)
            {
                throw new ArgumentNullException(nameof(stylesheet));
            }
            
            bool styleSetToAccounting = false;
            // If the cell is styled, we'll look for the format in the stylesheet
            if (cell.StyleIndex != null && cell.StyleIndex.HasValue)
            {
                var cellFormat = stylesheet.CellFormats.ElementAtOrDefault((int)cell.StyleIndex.Value);

                // If the style of the cell was found, we'll check for a number format
                if (cellFormat != null)
                {
                    var numberFormatID = (cellFormat as CellFormat).NumberFormatId;

                    if (numberFormatID == 42 || numberFormatID == 44) // 8 is Currency
                    {
                        styleSetToAccounting = true;
                    }
                }
            }

            return styleSetToAccounting;
        }

        /// <summary>
        /// Get all column header strings from the first row of the Excel worksheet.
        /// </summary>
        /// <param name="document">The Excel document to parse</param>
        /// <param name="worksheetName">The name of the worksheet to parse, or string.empty to use the first sheet</param>
        /// <returns>Collection of column header strings</returns>
        public static ICollection<string> GetAllColumnHeaderStrings(SpreadsheetDocument document, string worksheetName)
        {
            ICollection<string> columnHeaders = new List<string>();
            var worksheetPart = GetSpecifiedWorksheetPart(document, worksheetName);

            if (worksheetPart != null)
            {
                var firstRow = worksheetPart.Worksheet.Descendants<Row>().First();
                columnHeaders = (from c in firstRow.Descendants<Cell>()
                                 select GetCellValue(c, document.WorkbookPart)).ToList();
            }

            return columnHeaders;
        }

        /// <summary>
        /// Take a column header string and find the corresponding Excel column name in the document.
        /// </summary>
        /// <param name="document">The Excel document to parse</param>
        /// <param name="worksheetName">The name of the worksheet to search, or string.empty to use the first sheet</param>
        /// <param name="headerString">The column header string to search for</param>
        /// <returns>An Excel column name (i.e. A, B, C, etc) for the specified header string</returns>
        public static string GetColumnNameFromHeaderString(SpreadsheetDocument document, string worksheetName, string headerString)
        {
            var worksheetPart = GetSpecifiedWorksheetPart(document, worksheetName);

            if (worksheetPart == null)
            {
                // The specified worksheet does not exist.
                return null;
            }

            var firstRow = worksheetPart.Worksheet.Descendants<Row>().First();

            var headerCell = (from c in firstRow.Descendants<Cell>()
                              where GetCellValue(c, document.WorkbookPart).Equals(headerString, StringComparison.CurrentCultureIgnoreCase)
                              select c).FirstOrDefault();

            if (headerCell != null)
            {
                return ParseColumnName(headerCell.CellReference);
            }

            // If the specified header was not found in columns A-Z, pass up an exception to indicate
            throw new ColumnMissingException(headerString);
        }

        /// <summary>
        /// Given a cell name, parse the specified cell to get the column name.
        /// </summary>
        /// <param name="cellName">The name of the cell to get the column name from</param>
        /// <returns>An Excel column name (i.e. A, B, C, etc)</returns>
        public static string ParseColumnName(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            var match = columnNameRegex.Match(cellName);

            // Return the column name
            return match.Value;
        }

        /// <summary>
        /// Given a cell name, parse the specified cell to get the row index.
        /// </summary>
        /// <param name="cellName">The name of the cell to get the row index from</param>
        /// <returns>An Excel row index (i.e. 1, 2, 3, etc)</returns>
        public static uint ParseRowIndex(string cellName)
        {
            // Create a regular expression to match the row index portion the cell name.
            var match = rowIndexRegex.Match(cellName);

            // Return the row index
            return uint.Parse(match.Value);
        }

        /// <summary>
        /// Takes a column name (like A, B, C, etc) and return the zero-based index number for that column (0, 1, 2, 3, etc)
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <returns>The zero-based column index.</returns>
        public static int GetColumnIndexFromColumnName(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }
            int toReturn = 0;
            columnName = columnName.ToUpper();

            for (var iChar = columnName.Length - 1; iChar >= 0; iChar--)
            {
                var colPiece = columnName[iChar];
                var colNum = colPiece - 64;
                toReturn = toReturn + colNum * (int)Math.Pow(26, columnName.Length - (iChar + 1));
            }

            return toReturn - 1;
        }

        /// <summary>
        /// Takes a zero-based index number for a column (0, 1, 2, 3, etc) and returns the character column name (like A, B, C, D, etc)
        /// </summary>
        /// <param name="columnIndex">Zero-based Column index.</param>
        /// <returns>The column name.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "columnIndex+1")]
        public static string GetColumnNameFromColumnIndex(int columnIndex)
        {
            var toReturn = string.Empty;

            var columnNumber = columnIndex + 1;
            while (columnNumber > 0)
            {
                var currentLetterNumber = (columnNumber - 1) % 26;
                var currentLetter = (char)(currentLetterNumber + 65);
                toReturn = currentLetter + toReturn;
                columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
            }

            return toReturn;
        }

        /// <summary>
        /// Creates a new Excel file row using the Office OpenXML SDK
        /// </summary>
        /// <param name="document"></param>
        /// <param name="rowIndex">The index where the row will be added. There must be no existing elements at this index.</param>
        /// <param name="columnStyles">The specific styles for each column.</param>
        /// <param name="originalRow">The original template row.</param>
        /// <param name="values">The String values, one per column, to add to the row.</param>
        /// <returns>A formatted row, ready to be inserted into an Excel file.</returns>
        public static Row CreateExcelContentRow(SpreadsheetDocument document, Int64 rowIndex, Dictionary<uint, uint> columnStyles, Row originalRow, params string[] values)
        {
            if (columnStyles == null) { throw new ArgumentNullException(nameof(columnStyles));}
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            // Create the new row and set its index.
            Row toReturn = new Row();
            toReturn.RowIndex = (UInt32)rowIndex;

            if (values.Length > 0)
            {
                // Cache of cells in the original Row
                var originalCells = new List<Cell>();

                if (originalRow != null)
                {
                    originalCells = originalRow.Descendants<Cell>().ToList();
                }

                // For each given String value, create a new text cell and append it to the row
                for (var ndx = 0; ndx < values.Length; ndx++)
                {
                    uint? style = null;
                    uint styleIndex;

                    // Get the style defined on the column header
                    if (columnStyles.TryGetValue((uint)(ndx + 1), out styleIndex))
                    {
                        style = styleIndex;
                    }
                    else if (originalRow != null && originalRow.StyleIndex != null && originalRow.StyleIndex.HasValue)
                    {
                        // try to get the style index off of the row
                        style = originalRow.StyleIndex.Value;
                    }

                    // Get the cell name to create
                    var cellReference = GetColumnNameFromColumnIndex(ndx) + rowIndex;

                    // Find the original cell in the template, if it exists
                    var originalCell = originalCells.FirstOrDefault(c => c.CellReference.HasValue && c.CellReference.Value.Equals(cellReference, StringComparison.CurrentCultureIgnoreCase));

                    // Create the new cell
                    var cell = CreateExcelCell(document, cellReference, values[ndx], style, originalCell);

                    if (cell != null)
                    {
                        // Add the cell to the file
                        toReturn.AppendChild(cell);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Extracted from CreateExcelCell
        /// </summary>
        /// <param name="document">Spreadsheet document</param>
        /// <param name="cell">Cell being updated</param>
        /// <param name="value">Value to be displayed in the cell</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "document")]
        public static void SetCellValue(SpreadsheetDocument document, Cell cell, string value)
        {
            if (cell == null) { throw new ArgumentNullException(nameof(cell)); }
            if (string.IsNullOrEmpty(value))
            {
                cell.CellValue = null;
            }
            else
            {
                // check the data type of the cell content to apply basic formatting
                decimal numericalValue;
                int intValue;
                
                // Note: if datetime check was the first check, a string number such as "121.09" would actually return true so check for numbers first

                if (int.TryParse(value, out intValue))
                {
                    cell.CellValue = new CellValue(value);
                    cell.DataType = CellValues.Number;
                }
                else if (decimal.TryParse(value, out numericalValue))
                {
                    cell.CellValue = new CellValue(value);
                    cell.DataType = CellValues.Number;

                    #region Shared string table

                    ///*
                    // * Note:
                    // * 
                    // * Decimal values must be stored in the shared strings table to avoid potential modification by Excel.
                    // * If not, then the possibility exists for Excel to change the precision of the value.  For example,
                    // * the value "7.654" might be saved as "7.653999999999999".  When you  do this, however, Excel will
                    // * tag the cell with a "Number Stored as Text" note.  But you can effectively eliminate this note
                    // * using an "ignore error" entry.
                    // * 
                    // */

                    //int ssidx;
                    //if ((ssidx = IndexOfSharedString(document, value)) == -1)
                    //{
                    //    AddSharedString(document, value, true);
                    //    ssidx = IndexOfSharedString(document, value);
                    //}

                    //cell.CellValue = new CellValue(ssidx.ToString());
                    //cell.DataType = CellValues.SharedString;

                    #endregion
                }
                else
                {
                    // assume the default is a string
                    // since all our Date export columns use text and not Date, this needs to be defined as string vs CellValues.Date
                    cell.CellValue = new CellValue(value);
                    cell.DataType = CellValues.String;
                }
            }
        }

        /// <summary>
        /// Return a reference to the IgnoredErrors node for the worksheet containing the given node element
        /// </summary>
        /// <param name="element">Element in the worksheet</param>
        /// <returns>IgnoredErrors node</returns>
        private static IgnoredErrors GetIgnoredErrors(OpenXmlElement element)
        {
            if (element != null)
            {
                OpenXmlElement node = element;
                while (node != null)
                {
                    Worksheet worksheet;
                    if ((worksheet = node as Worksheet) != null)
                    {
                        IgnoredErrors ignoredErrors = worksheet.Descendants<IgnoredErrors>().FirstOrDefault();
                        return ignoredErrors;
                    }
                    else
                    {
                        node = node.Parent;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Set the given range of cells to ignore Number-Stored-As-Text errors
        /// </summary>
        /// <param name="element">Element in the worksheet</param>
        /// <param name="cellRange">Range of cells</param>
        public static void SetIgnoredErrors(OpenXmlElement element, string cellRange)
        {
            IgnoredErrors ignoredErrors;
            if ((ignoredErrors = GetIgnoredErrors(element)) != null)
            {
                ignoredErrors.AppendChild<IgnoredError>(new IgnoredError
                {
                    SequenceOfReferences = new ListValue<StringValue>(new List<StringValue> { cellRange }),
                    NumberStoredAsText = BooleanValue.FromBoolean(true)
                });
            }
        }

        /// <summary>
        /// Create an Excel text cell using the Office OpenXML SDK
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="cellReference">The cell reference.</param>
        /// <param name="text">The text that will occupy the cell</param>
        /// <param name="style">The style.</param>
        /// <param name="originalCell">The original cell.</param>
        /// <returns>A formatted text cell, ready to be appended to a Row.</returns>
        private static Cell CreateExcelCell(SpreadsheetDocument document, string cellReference, string text, uint? style, Cell originalCell)
        {
            Cell toReturn = null;

            // If we passed in text we;'ll need to create a new cell
            if (!string.IsNullOrEmpty(text))
            {
                toReturn = new Cell();
                // Create the cell reference (i.e. A1) from the header letter and row index given
                toReturn.CellReference = cellReference;

                // If there was already a cell at this cellReference, use its style first
                if (originalCell != null &&
                    originalCell.StyleIndex != null &&
                    originalCell.StyleIndex.HasValue)
                {
                    toReturn.StyleIndex = originalCell.StyleIndex;
                }
                // If there was no cell at this CellReference, use the style defined on the header second
                else if (style.HasValue)
                {
                    toReturn.StyleIndex = style;
                }

                if (ForceAsStringValue(ref text))
                {
                    toReturn.CellValue = new CellValue(text);
                    toReturn.DataType = CellValues.String;
                }
                else if (ForceAsNumericalValue(ref text))
                {
                    toReturn.CellValue = new CellValue(text);
                    toReturn.DataType = CellValues.Number;
                }
                else if (SetTextAsBold(ref text))
                {
                    // Create a run for the text
                    Run run = new Run();
                    run.Append(new Text(text));

                    // Set the text as bold in the run properties
                    RunProperties runProperties = new RunProperties();
                    runProperties.Append(new Bold());
                    run.RunProperties = runProperties;

                    // Add run/text to the cell
                    InlineString inlineString = new InlineString();
                    inlineString.Append(run);
                    toReturn.DataType = CellValues.InlineString;
                    toReturn.Append(inlineString);
                }
                else
                {
                    SetCellValue(document, toReturn, text);
                }
            }
            // If we didn't pass in text, but there was already an existing cell at this CellReference, then
            // we'll copy that cell along with any text or styles that it contained
            else if (originalCell != null)
            {
                toReturn = originalCell.CloneNode(true) as Cell;
            }

            return toReturn;
        }

        /// <summary>
        /// When true, forces the format of the cell to be a string value.
        /// </summary>
        /// <param name="cellValue">Value of the cell to be formatted.</param>
        /// <returns>true if the cell format should be forced to a string</returns>
        internal static bool ForceAsStringValue(ref string cellValue)
        {
            bool forceAsString = false;

            // If the text starts w/ a leading 0, and is not a decimal number starting with 0. , or it's not 
            // just a single 0... then we assume that the datatype is text
            if (cellValue.StartsWith("0") && !cellValue.StartsWith("0.") && !cellValue.Equals("0"))
            {
                forceAsString = true;
            }
            else if (cellValue.StartsWith(CommonConstants.FORCE_AS_STRING_VALUE))
            {
                // strip off the force as string delimiter.
                cellValue = cellValue.Replace(CommonConstants.FORCE_AS_STRING_VALUE, "");
                forceAsString = true;
            }
            return forceAsString;
        }

        /// <summary>
        /// When true, forces the format of the cell to be a number.
        /// </summary>
        /// <param name="cellValue">Value of the cell to be formatted.</param>
        /// <returns>true if the cell format should be forced to a number</returns>
        internal static bool ForceAsNumericalValue(ref string cellValue)
        {
            bool forceAsNumber = false;

            // If the text starts w/ the constant, then force it.
            if (cellValue.StartsWith(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL))
            {
                // strip off the force & other text formattings
                cellValue = cellValue.Replace(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL, string.Empty).Replace(",", string.Empty).Replace("$", string.Empty);
                forceAsNumber = true;
            }

            return forceAsNumber;
        }

        /// <summary>
        /// When true, forces the format of the cell to be a number.
        /// </summary>
        /// <param name="cellValue">Value of the cell to be formatted.</param>
        /// <returns>true if the cell format should be forced to a number</returns>
        internal static bool SetTextAsBold(ref string cellValue)
        {
            bool setBold = false;

            // If the text starts w/ the constant, then force it.
            if (cellValue.StartsWith(CommonConstants.SET_AS_BOLD_FOR_EXCEL))
            {
                // strip off the force & other text formattings
                cellValue = cellValue.Replace(CommonConstants.SET_AS_BOLD_FOR_EXCEL, string.Empty);
                setBold = true;
            }

            return setBold;
        }

        /// <summary>
        /// Copies a column
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="worksheetName">Name of the worksheet.</param>
        /// <param name="columnHeaderString">The column header string.</param>
        /// <param name="newColumnHeaderStrings">The new column header strings.</param>
        public static void DuplicateColumn(SpreadsheetDocument document, string worksheetName, string columnHeaderString, params string[] newColumnHeaderStrings)
        {
            if (newColumnHeaderStrings == null) { throw new ArgumentNullException(nameof(newColumnHeaderStrings)); }
            if (newColumnHeaderStrings.Length > 0)
            {
                var duplications = newColumnHeaderStrings.Length - 1;

                var columnName = GetColumnNameFromHeaderString(document, worksheetName, columnHeaderString);
                var columnIndex = GetColumnIndexFromColumnName(columnName);

                var worksheetPart = GetSpecifiedWorksheetPart(document, worksheetName); ;

                if (worksheetPart != null)
                {
                    var worksheet = worksheetPart.Worksheet;

                    if (duplications > 0)
                    {
                        var columns = worksheet.Descendants<Column>().ToList();

                        var column = columns.FirstOrDefault(c => c.Min.HasValue && c.Min.Value == columnIndex + 1);

                        if (column != null)
                        {
                            var greaterColumns = columns.Where(c => c.Min > column.Min).OrderByDescending(c => c.Max.Value).ToList();

                            foreach (var greaterColumn in greaterColumns)
                            {
                                if (greaterColumn == greaterColumns.First())
                                {
                                    greaterColumn.Min = greaterColumn.Min + (uint)duplications;
                                }
                                else
                                {
                                    for (var ndx = (int)greaterColumn.Max.Value; ndx >= (int)greaterColumn.Min.Value; ndx--)
                                    {
                                        TransferAllCellsFromOneColumnToAnother(worksheetPart, GetColumnNameFromColumnIndex(ndx - 1), GetColumnNameFromColumnIndex(ndx + duplications - 1), true);
                                    }

                                    greaterColumn.Min = greaterColumn.Min + (uint)duplications;
                                    greaterColumn.Max = greaterColumn.Max + (uint)duplications;
                                }
                            }

                            for (var ndx = columnIndex + duplications; ndx >= columnIndex + 1; ndx--)
                            {
                                TransferAllCellsFromOneColumnToAnother(worksheetPart, columnName, GetColumnNameFromColumnIndex(ndx), false);
                            }

                            column.Max = column.Max + (uint)duplications;

                            //column.Descendants<AutoFilter>
                        }
                    }

                    foreach (var newColumnHeaderString in newColumnHeaderStrings)
                    {
                        columnName = GetColumnNameFromHeaderString(document, worksheetName, columnHeaderString);

                        var cell = worksheet.Descendants<Cell>().FirstOrDefault(c => ParseRowIndex(c.CellReference) == 1 && ParseColumnName(c.CellReference).Equals(columnName, StringComparison.CurrentCultureIgnoreCase));

                        if (cell != null)
                        {
                            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                            {
                                cell.DataType.Value = CellValues.String;
                            }

                            cell.CellValue.Text = newColumnHeaderString;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes a column
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="worksheetName">Name of the worksheet.</param>
        /// <param name="columnHeaderString">The column header string.</param>
        public static void RemoveColumn(SpreadsheetDocument document, string worksheetName, string columnHeaderString)
        {
            var columnName = GetColumnNameFromHeaderString(document, worksheetName, columnHeaderString);
            var columnIndex = GetColumnIndexFromColumnName(columnName);

            var worksheetPart = GetSpecifiedWorksheetPart(document, worksheetName);

            if (worksheetPart != null)
            {
                var worksheet = worksheetPart.Worksheet;

                var columns = worksheet.Descendants<Column>().ToList();

                var column = columns.FirstOrDefault(c => c.Min.HasValue && c.Min.Value == columnIndex + 1);

                if (column != null)
                {
                    RemoveAllCellsFromColumn(worksheetPart, columnName);

                    var greaterColumns = columns.Where(c => c.Min > column.Min).ToList();

                    foreach (var greaterColumn in greaterColumns)
                    {
                        if (greaterColumn == greaterColumns.Last())
                        {
                            greaterColumn.Min = greaterColumn.Min - 1;
                        }
                        else
                        {
                            for (var ndx = (int)greaterColumn.Min.Value; ndx <= (int)greaterColumn.Max.Value; ndx++)
                            {
                                TransferAllCellsFromOneColumnToAnother(worksheetPart, GetColumnNameFromColumnIndex(ndx - 1), GetColumnNameFromColumnIndex(ndx - 2), true);
                            }

                            greaterColumn.Min = greaterColumn.Min - 1;
                            greaterColumn.Max = greaterColumn.Max - 1;
                        }
                    }

                    column.Remove();
                }
            }
        }

        /// <summary>
        /// When a column is duplicated, we need to move everything to the right of it over. This function
        /// assists in that by moving everything from one column to another. 
        /// </summary>
        /// <param name="worksheetPart">The worksheet</param>
        /// <param name="fromColumnName">The column name to move data from</param>
        /// <param name="toColumnName">The column name to move data to</param>
        /// <param name="move">True to move everything from the column, false to make a copy of it.</param>
        private static void TransferAllCellsFromOneColumnToAnother(WorksheetPart worksheetPart, string fromColumnName, string toColumnName, bool move)
        {
            var fromColumnIndex = GetColumnIndexFromColumnName(fromColumnName);
            var toColumnIndex = GetColumnIndexFromColumnName(toColumnName);
            var delta = toColumnIndex - fromColumnIndex;

            var cellsToMove = worksheetPart.Worksheet.Descendants<Cell>().Where(c => ParseColumnName(c.CellReference).Equals(fromColumnName, StringComparison.CurrentCultureIgnoreCase)).ToList();

            foreach (var itemToMove in cellsToMove)
            {
                if (move)
                {
                    itemToMove.CellReference = toColumnName + ParseRowIndex(itemToMove.CellReference);
                }
                else
                {
                    var newItem = (Cell)itemToMove.CloneNode(true);
                    newItem.CellReference = toColumnName + ParseRowIndex(newItem.CellReference);
                    newItem.StyleIndex = itemToMove.StyleIndex;
                    itemToMove.InsertAfterSelf(newItem);
                }
            }

            var mergeCellsToMove = worksheetPart.Worksheet.Descendants<MergeCell>().Where(c => c.Reference.Value.Split(':').Select(r => ParseColumnName(r)).Contains(fromColumnName)).ToList();

            foreach (var itemToMove in mergeCellsToMove)
            {
                if (move)
                {
                    var updatedMergeCellReferences = itemToMove.Reference.Value.Split(':').Select(r => GetColumnNameFromColumnIndex(GetColumnIndexFromColumnName(ParseColumnName(r)) + delta) + ParseRowIndex(r));
                    itemToMove.Reference = String.Join(":", updatedMergeCellReferences);
                }
                else
                {
                    var newItem = (MergeCell)itemToMove.CloneNode(true);
                    var updatedMergeCellReferences = newItem.Reference.Value.Split(':').Select(r => GetColumnNameFromColumnIndex(GetColumnIndexFromColumnName(ParseColumnName(r)) + delta) + +ParseRowIndex(r));
                    newItem.Reference = String.Join(":", updatedMergeCellReferences);
                    itemToMove.InsertAfterSelf(newItem);
                }
            }

            //var commentsToMove = worksheetPart.WorksheetCommentsPart.Comments.Descendants<Comment>().Where(c => ParseColumnName(c.Reference).Equals(fromColumnName, StringComparison.CurrentCultureIgnoreCase)).ToList();

            //foreach (var itemToMove in commentsToMove)
            //{
            //    itemToMove.Reference = toColumnName.ToString() + ParseRowIndex(itemToMove.Reference);
            //}
        }

        /// <summary>
        /// When a column is deleted, we need to remove all of the cells under that column.
        /// </summary>
        /// <param name="worksheetPart">The worksheet</param>
        /// <param name="fromColumnName">The column name to remove</param>
        private static void RemoveAllCellsFromColumn(WorksheetPart worksheetPart, string fromColumnName)
        {
            var cellsToRemove = worksheetPart.Worksheet.Descendants<Cell>().Where(c => ParseColumnName(c.CellReference).Equals(fromColumnName, StringComparison.CurrentCultureIgnoreCase)).ToList();

            foreach (var itemToRemove in cellsToRemove)
            {
                itemToRemove.Remove();
            }

            var mergeCellsToRemove = worksheetPart.Worksheet.Descendants<MergeCell>().Where(c => c.Reference.Value.Split(':').Select(r => ParseColumnName(r)).Contains(fromColumnName)).ToList();

            foreach (var itemToRemove in mergeCellsToRemove)
            {
                itemToRemove.Remove();
            }

            //var commentsToMove = worksheetPart.WorksheetCommentsPart.Comments.Descendants<Comment>().Where(c => ParseColumnName(c.CellReference).Equals(fromColumnName, StringComparison.CurrentCultureIgnoreCase)).ToList();

            //foreach (var itemToMove in commentsToMove)
            //{
            //    itemToMove.Reference = toColumnName.ToString() + ParseRowIndex(itemToMove.Reference);
            //}
        }
        
        /// <summary>
        /// Copies a given template file and returns the path of the duplicate
        /// </summary>
        /// <param name="templateFileLocation">Path of the file to copy.</param>
        /// <returns>The path of the new copy.</returns>
        public static string CopyExcelTemplateFile(string templateFileLocation)
        {
            // Create a new random file name in the specified directory
            var toReturn = Path.GetDirectoryName(templateFileLocation) + "\\" + Path.GetRandomFileName() + ".xlsx";

            File.Copy(templateFileLocation, toReturn);

            // Make sure that the copied template is writable
            FileInfo copiedFileInfo = new FileInfo(toReturn);
            copiedFileInfo.IsReadOnly = false;

            return toReturn;
        }

        /// <summary>
        /// Builds up a dictionary of all the defined names for the excel document. These unique names give 
        /// semantic meaning to the cell and store's the sheet name and cell range information.  This helps
        /// prevent hard coding cell addresses when dealing with template text since if someone added a row
        /// of data above your cell then you would have to go and manually update all cell addresses in the 
        /// code. Now we just use the data provided in the defined names to access the cell addresses.
        /// </summary>
        /// <param name="workbookPart">Work Book Part</param>
        /// <returns>List of defined names</returns>
        internal static Dictionary<string, DefinedNameData> GetDefinedNames(WorkbookPart workbookPart)
        {
            Dictionary<string, DefinedNameData> definedNames = new Dictionary<string, DefinedNameData>();

            DefinedNames definedNamesElement = workbookPart.Workbook.GetFirstChild<DefinedNames>();

            if (definedNamesElement != null)
            {
                foreach (DefinedName name in definedNamesElement)
                {
                    // Parse defined name string (ie. <x:definedName name="ContractTypes" xmlns:x="http://schemas.openxmlformats.org/spreadsheetml/2006/main">'ProgramDto Info'!$B$14</x:definedName>)
                    string key = name.Name;
                    string reference = name.InnerText;

                    string sheetName = reference.Split('!')[0];
                    sheetName = sheetName.Trim('\'');

                    // Assumption: None of my defined names are relative defined names (i.e. A1)
                    string range = reference.Split('!')[1];
                    string[] rangeArray = range.Split('$');

                    if (rangeArray.Length >= 2)
                    {
                        string startCol = rangeArray[1];
                        string startRow = rangeArray[2].TrimEnd(':');

                        string endCol = null;
                        string endRow = null;

                        if (rangeArray.Length > 3)
                        {
                            endCol = rangeArray[3];
                            endRow = rangeArray[4];
                        }

                        definedNames[key] = new DefinedNameData() { Key = key, SheetName = sheetName, StartColumn = startCol, StartRow = startRow, EndColumn = endCol, EndRow = endRow, CellReference = startCol + startRow };
                    }
                }
            }

            return definedNames;
        }

        /// <summary>
        /// Get the worksheets from a passed in document
        /// </summary>
        /// <param name="document">The document to retrieve worksheets from</param>
        /// <returns>The <see cref="Sheets"/> containing the worksheets</returns>
        public static Sheets GetWorksheetsFromDocument(SpreadsheetDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            // Retrieve a reference to the workbook part.
            var wbPart = document.WorkbookPart;

            // Get the worksheets
            Sheets sheets = wbPart.Workbook.Sheets;

            return sheets;

        }

        /// <summary>
        /// Gets the <see cref="Worksheet"/> ID's from the passed in <see cref="Sheets"/>
        /// </summary>
        /// <param name="sheets">The <see cref="Sheets"/> object to retrieve the <see cref="Worksheet"/> ID's</param>
        /// <returns>A list of <see cref="System.Tuple"/>s containing the sheet name and ID</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Collection<Tuple<String, String>> GetWorksheetIDsFromSheets(Sheets sheets)
        {
            if (sheets == null) { throw new ArgumentNullException(nameof(sheets)); }

            Collection<Tuple<string, string>> SheetIDs = new Collection<Tuple<string, string>>();

            foreach (Sheet item in sheets)
            {
                String sheetName = item.GetAttributes().First(x => x.LocalName == "name").Value;
                SheetIDs.Add(new Tuple<string, string>(sheetName, item.Id));
            }

            return SheetIDs;
        }

        /// <summary>
        /// Gets the <see cref="WorksheetPart"/> from the <see cref="WorkbookPart"/> by the passed in worksheetID
        /// </summary>
        /// <param name="wbPart">The <see cref="WorkbookPart"/> containing the needed <see cref="WorksheetPart"/></param>
        /// <param name="sheetID">The ID of the desired <see cref="WorksheetPart"/></param>
        /// <returns>The desired <see cref="WorksheetPart"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static WorksheetPart GetWorksheetPartBySheetID(WorkbookPart wbPart, String sheetID)
        {
            if (wbPart == null) { throw new ArgumentNullException(nameof(wbPart)); }
            return (WorksheetPart)wbPart.GetPartById(sheetID);
        }

        /// <summary>
        /// Gets a list of <see cref="Table"/>s from the <see cref="WorksheetPart"/>
        /// </summary>
        /// <param name="wsp">The <see cref="WorksheetPart"/> we want <see cref="Table"/>s</param>
        /// <returns>A List of <see cref="Table"/>s</returns>
        public static Collection<Table> GetTablesInWorksheetPart(WorksheetPart wsp)
        {
            if (wsp == null) { throw new ArgumentNullException(nameof(wsp)); }
            Collection<Table> toReturn = new Collection<Table>();
            foreach (TableDefinitionPart item in wsp.TableDefinitionParts)
            {
                toReturn.Add(item.Table);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets a <see cref="Cell"/> object that is relative to the a source <see cref="Cell"/>
        /// </summary>
        /// <param name="sourceCell">the  source <see cref="Cell"/></param>
        /// <param name="rowOffset">The row offset used to find the desired cell</param>
        /// <param name="columnOffset">The column offset used to find the desired cell</param>
        /// <param name="wsp">The <see cref="WorksheetPart"/></param>
        /// <returns>The desired <see cref="Cell"/></returns>
        public static Cell GetCellByOffset(Cell sourceCell, int rowOffset, int columnOffset, WorksheetPart wsp)
        {
            if (sourceCell == null) { throw new ArgumentNullException(nameof(sourceCell)); }

            int SourceRowReferencePart = (int)ParseRowIndex(sourceCell.CellReference.Value);
            int SourceColumnReferencePart = GetColumnIndexFromColumnName(ParseColumnName(sourceCell.CellReference.Value));

            int TargetRowReferencPart = SourceRowReferencePart + rowOffset;
            int TargetColumnReferencePart = SourceColumnReferencePart + columnOffset;

            String TargetCellReference = String.Concat(ExcelUtilities.GetColumnNameFromColumnIndex(TargetColumnReferencePart), TargetRowReferencPart.ToString());
            return ExcelUtilities.GetCell(wsp, TargetCellReference);
        }

        #region Row insert and copy methods

        /// <summary>
        /// Inserts a new row at the desired index. If one already exists, then it is
        /// returned. If an rowToInsert is provided, then it is inserted into the desired
        /// rowIndex
        /// </summary>
        /// <param name="rowIndex">Row Index</param>
        /// <param name="worksheetPart">Worksheet Part</param>
        /// <param name="rowToInsert">Row to insert</param>
        /// <param name="isNewLastRow">Optional parameter - True, you can guarantee that this row is the last row (not replacing an existing last row) in the sheet to insert; false it is not</param>
        /// <returns>Inserted Row</returns>
        public static Row InsertRow(uint rowIndex, WorksheetPart worksheetPart, Row rowToInsert, bool isNewLastRow = false)
        {
            if (worksheetPart == null) { throw new ArgumentNullException(nameof(worksheetPart)); }

            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();

            Row retRow = !isNewLastRow ? sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex) : null;

            // If the worksheet does not contain a row with the specified row index, insert one.
            if (retRow != null)
            {
                // if retRow is not null and we are inserting a new row, then move all existing rows down.

                // NOTE: "If" conditions are not combined because we want to skip the parent "else" when the outside "if" is true.
                if (rowToInsert != null)
                {
                    // various cell references (on a COPIED row) need to be adjusted to fix mismatches because the row index changes
                    UpdateRowIndexes(worksheetPart, rowIndex, false);
                    UpdateCommentRowIndexes(worksheetPart, rowIndex, false);

                    // actually insert the new row into the sheet
                    retRow = sheetData.InsertBefore(rowToInsert, retRow);  // at this point, retRow still points to the row that had the insert rowIndex

                    string curIndex = retRow.RowIndex.ToString();
                    string newIndex = rowIndex.ToString();

                    foreach (Cell cell in retRow.Elements<Cell>())
                    {
                        // Update the references for the rows cells.
                        cell.CellReference = new StringValue(cell.CellReference.Value.Replace(curIndex, newIndex));
                    }

                    // Update the row index.
                    retRow.RowIndex = rowIndex;
                }
            }
            else
            {
                // Row doesn't exist yet, shifting not needed.
                // Rows must be in sequential order according to RowIndex. Determine where to insert the new row.
                Row refRow = !isNewLastRow ? sheetData.Elements<Row>().FirstOrDefault(row => row.RowIndex > rowIndex) : null;

                // use the insert row if it exists
                retRow = rowToInsert ?? new Row() { RowIndex = rowIndex };

                ICollection<Cell> cellsInRow = retRow.Elements<Cell>().ToList();

                if (cellsInRow.Any())
                {
                    string curIndex = retRow.RowIndex.ToString();
                    string newIndex = rowIndex.ToString();

                    foreach (Cell cell in cellsInRow)
                    {
                        // Update the references for the rows cells.
                        cell.CellReference = new StringValue(cell.CellReference.Value.Replace(curIndex, newIndex));
                    }

                    // Update the row index.
                    retRow.RowIndex = rowIndex;
                }

                sheetData.InsertBefore(retRow, refRow);
            }

            return retRow;
        }

        /// <summary>
        /// Deletes the row at the provided index from the worksheet.
        /// Automatically updates the references for all rows that follow this one.
        /// </summary>
        /// <param name="rowIndex">Index to delete</param>
        /// <param name="worksheetPart">WorksheetPart containing the row</param>
        internal static void DeleteRow(uint rowIndex, WorksheetPart worksheetPart)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();

            Row delRow = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex);

            // make sure a row exists before trying to delete it.
            if (delRow != null)
            {
                UpdateRowIndexes(worksheetPart, rowIndex, true);
                UpdateCommentRowIndexes(worksheetPart, rowIndex, true);

                // actually delete the row from the worksheet
                delRow.Remove();
            }
        }

        /// <summary>
        /// Deletes a row of the given WorksheetPart relative to another cell in the WorksheetPart.
        /// </summary>
        /// <param name="worksheetPart">WorksheetPart containing the row.</param>
        /// <param name="sourceCell">The source cell to use as starting point for offset row location.</param>
        /// <param name="rowOffset">The number of rows from the source cell that the row to be deleted is located.</param>
        internal static void DeleteRowByOffset(WorksheetPart worksheetPart, Cell sourceCell, uint rowOffset)
        {
            // Calculate the index of the row to be removed.
            uint sourceCellRow = ExcelUtilities.ParseRowIndex(sourceCell.CellReference.Value);
            uint rowIndexToRemove = sourceCellRow + rowOffset;

            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();

            Row delRow = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndexToRemove);

            // Make sure a row exists before trying to delete it.
            if (delRow != null)
            {
                UpdateRowIndexes(worksheetPart, rowIndexToRemove, true);
                UpdateCommentRowIndexes(worksheetPart, rowIndexToRemove, true);

                // Delete the row from the worksheet.
                delRow.Remove();
            }
        }

        /// <summary>
        /// Updates all of the Row indexes and the child Cells' CellReferences whenever
        /// a row is inserted or deleted.
        /// </summary>
        /// <param name="worksheetPart">Worksheet Part</param>
        /// <param name="rowIndex">Row Index being inserted or deleted</param>
        /// <param name="isDeletedRow">True if row was deleted, otherwise false</param>
        private static void UpdateRowIndexes(WorksheetPart worksheetPart, uint rowIndex, bool isDeletedRow)
        {
            // Get all the rows in the worksheet with equal or higher row index values than the one being inserted/deleted for reindexing.
            ICollection<Row> rows = worksheetPart.Worksheet.Descendants<Row>().Where(r => r.RowIndex.Value >= rowIndex).ToList();

            foreach (Row row in rows)
            {
                uint newIndex = isDeletedRow ? row.RowIndex - 1 : row.RowIndex + 1;
                string curRowIndex = row.RowIndex.ToString();
                string newRowIndex = newIndex.ToString();

                foreach (Cell cell in row.Elements<Cell>())
                {
                    // Update the references for the rows cells.
                    cell.CellReference = new StringValue(cell.CellReference.Value.Replace(curRowIndex, newRowIndex));
                }

                // Update the row index.
                row.RowIndex = newIndex;
            }
        }

        /// <summary>
        /// Updates row indices for cell comments when a row is inserted or deleted.
        /// </summary>
        /// <param name="worksheetPart">Worksheet Part</param>
        /// <param name="rowIndex">Row Index being inserted or deleted</param>
        /// <param name="isDeletedRow">True if row was deleted, otherwise false</param>
        private static void UpdateCommentRowIndexes(WorksheetPart worksheetPart, uint rowIndex, bool isDeletedRow)
        {
            // Get all the rows in the worksheet with equal or higher row index values than the one being inserted/deleted for reindexing.
            WorksheetCommentsPart commentsPart = worksheetPart.GetPartsOfType<WorksheetCommentsPart>().SingleOrDefault();

            if (commentsPart != null && commentsPart.Comments != null)
            {
                Comments comments = commentsPart.Comments;

                Match commentRowIndexMatch;
                uint commentRowIndex;

                foreach (Comment comment in comments.CommentList)
                {
                    commentRowIndexMatch = commentRowIndexRegex.Match(comment.Reference.Value);
                    if (commentRowIndexMatch.Success && uint.TryParse(commentRowIndexMatch.Value, out commentRowIndex) && commentRowIndex >= rowIndex)
                    {
                        // if being deleted, comment needs to be removed or moved up
                        if (isDeletedRow)
                        {
                            // if comment is on the row being removed, remove it
                            if (commentRowIndex == rowIndex)
                            {
                                comment.Remove();
                            }
                            else
                            {
                                // else comment needs to be moved up a row
                                comment.Reference.Value = comment.Reference.Value.Replace(commentRowIndexMatch.Value, (commentRowIndex - 1).ToString());
                            }
                        }
                        else
                        {
                            // else row is being inserted, move comment down
                            comment.Reference.Value = comment.Reference.Value.Replace(commentRowIndexMatch.Value, (commentRowIndex + 1).ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// After rows or columns have been inserted we need to redefine the sheet dimensions to include them
        /// </summary>
        /// <param name="worksheetPart">Sheet in which the table is</param>
        /// <param name="rowsAdded">The number of rows added</param>
        /// <param name="columnsAdded">The number of column added</param>
        /// <returns>The resulting sheet dimensions</returns>
        public static string RedefineSheetDimensions(WorksheetPart worksheetPart, uint rowsAdded, int columnsAdded)
        {
            if (worksheetPart == null) { throw new ArgumentNullException(nameof(worksheetPart)); }

            string initialReference = worksheetPart.Worksheet.SheetDimension.Reference;
            string resultingReference = initialReference;

            string[] boundaryCells = initialReference.Split(':');

            if (boundaryCells.Length > 1)
            {
                string fromCellReference = boundaryCells[0];
                string toCellReference = boundaryCells[1];

                // initial values
                int toCellColumnIndex = GetColumnIndexFromColumnName(ParseColumnName(toCellReference));
                uint toCellRowIndex = ParseRowIndex(toCellReference);

                // adjusted values
                toCellColumnIndex += columnsAdded;
                toCellRowIndex += rowsAdded;

                resultingReference = StringValue.FromString(string.Format("{0}:{1}{2}", fromCellReference, GetColumnNameFromColumnIndex(toCellColumnIndex + 1), toCellRowIndex));

                worksheetPart.Worksheet.SheetDimension.Reference = resultingReference;
            }

            return resultingReference;
        }

        #endregion

        /// <summary>
        /// Copy a worksheet
        /// </summary>
        /// <param name="spreadsheet">The spreadsheet.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="clonedSheetName">Name of the cloned sheet.</param>
        /// <param name="keepData">if set to <c>true</c> [keep data].</param>
        /// <returns>A copy of the worksheet.</returns>
        public static WorksheetPart CopyWorksheet(SpreadsheetDocument spreadsheet, string sheetName, string clonedSheetName, bool keepData = false)
        {
            if (spreadsheet == null) { throw new ArgumentNullException(nameof(spreadsheet)); }

            //Open workbook
            WorkbookPart workbookPart = spreadsheet.WorkbookPart;
            //Get the source sheet to be copied
            WorksheetPart sourceSheetPart = GetWorkSheetPartBySheetName(workbookPart, sheetName);
            //Take advantage of AddPart for deep cloning
            WorksheetPart clonedSheet;
            using (MemoryStream memory = new MemoryStream())
            {
                //Take advantage of AddPart for deep cloning
                SpreadsheetDocument tempSheet = SpreadsheetDocument.Create(memory, spreadsheet.DocumentType);
                WorkbookPart tempWorkbookPart = tempSheet.AddWorkbookPart();
                WorksheetPart tempWorksheetPart = tempWorkbookPart.AddPart<WorksheetPart>(sourceSheetPart);
                //Add cloned sheet and all associated parts to workbook
                clonedSheet = workbookPart.AddPart<WorksheetPart>(tempWorksheetPart);
            }
            if (clonedSheet != null)
            {
                if (keepData)
                {
                    //Clean up table definition parts (tables need unique ids)
                    uint numTableDefParts = ExcelUtilities.GetMaxTableDefPartId(spreadsheet);
                    if (numTableDefParts != 0)
                    {
                        FixupTableParts(clonedSheet, numTableDefParts);
                    }
                }
                else
                {
                    //Remove all table definitions and data
                    RemoveTablesAndData(clonedSheet);
                }

                // Generate a unique Sheet Id
                uint sheetId = ExcelUtilities.GetNextSheetId(spreadsheet.WorkbookPart.Workbook.Sheets);
                clonedSheet.Worksheet.SheetProperties.CodeName = "Sheet" + sheetId;
                //Add new sheet to main workbook part
                Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
                Sheet copiedSheet = new Sheet();
                copiedSheet.Name = clonedSheetName;
                copiedSheet.Id = workbookPart.GetIdOfPart(clonedSheet);
                copiedSheet.SheetId = sheetId;
                sheets.Append(copiedSheet);
                //Save Changes
                workbookPart.Workbook.Save();
            }
            return clonedSheet;
        }

        /// <summary>
        /// Get the WorkbookPart for the specified sheet
        /// </summary>
        /// <param name="workbookPart">The workbook part.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <returns></returns>
        internal static WorksheetPart GetWorkSheetPartBySheetName(WorkbookPart workbookPart, string sheetName)
        {
            //Get the relationship id of the sheetname
            string relId = workbookPart.Workbook.Descendants<Sheet>()
            .First(s => s.Name.Value.Equals(sheetName))
            .Id;
            return (WorksheetPart)workbookPart.GetPartById(relId);
        }

        /// <summary>
        /// Assign unique IDs to cloned table definition parts.
        /// </summary>
        /// <param name="worksheetPart">The worksheet part.</param>
        /// <param name="tableId">The table identifier.</param>
        static void FixupTableParts(WorksheetPart worksheetPart, uint tableId)
        {
            //Every table needs a unique id and name
            foreach (TableDefinitionPart tableDefPart in worksheetPart.TableDefinitionParts)
            {
                tableId++;
                tableDefPart.Table.Id = (uint)tableId;
                tableDefPart.Table.DisplayName = "CopiedTable" + tableId;
                tableDefPart.Table.Name = "CopiedTable" + tableId;
                tableDefPart.Table.Save();
            }
    }

        /// <summary>
        /// Returns the index of a shared string.
        /// </summary>
        /// <param name="spreadsheet">Spreadsheet to use</param>
        /// <param name="stringItem">String to search for</param>
        /// <returns>Index of a shared string. -1 if not found</returns>
        internal static int IndexOfSharedString(SpreadsheetDocument spreadsheet, string stringItem)
        {
            if (spreadsheet.WorkbookPart.SharedStringTablePart == null)
            {
                spreadsheet.WorkbookPart.AddNewPart<SharedStringTablePart>();
                spreadsheet.WorkbookPart.SharedStringTablePart.SharedStringTable = new SharedStringTable();
            }

            SharedStringTable sharedStringTable = spreadsheet.WorkbookPart.SharedStringTablePart.SharedStringTable;
            bool found = false;
            int index = 0;

            foreach (SharedStringItem sharedString in sharedStringTable.Elements<SharedStringItem>())
            {
                if (sharedString.InnerText == stringItem)
                {
                    found = true;
                    break; ;
                }
                index++;
            }

            return found ? index : -1;
        }

        /// <summary>
        /// Add a single string to shared strings table.
        /// Shared string table is created if it doesn't exist.
        /// </summary>
        /// <param name="spreadsheet">Spreadsheet to use</param>
        /// <param name="stringItem">string to add</param>
        /// <param name="save">Save the shared string table</param>
        /// <returns></returns>
        internal static bool AddSharedString(SpreadsheetDocument spreadsheet, string stringItem, bool save = true)
        {
            SharedStringTable sharedStringTable = spreadsheet.WorkbookPart.SharedStringTablePart.SharedStringTable;

            if (!sharedStringTable.Any(item => item.InnerText == stringItem))
            {
                if (stringItem.StartsWith(" ") || stringItem.EndsWith(" "))
                {
                    // Preserve Space		<t xml:space="preserve"> Hello World </t>
                    sharedStringTable.AppendChild(
                       new SharedStringItem(
                          new Text(stringItem) { Space = SpaceProcessingModeValues.Preserve }));
                }
                else
                {
                    sharedStringTable.AppendChild(
                       new SharedStringItem(
                          new Text(stringItem)));
                }

                // Save the changes
                if (save)
                {
                    sharedStringTable.Save();
                }
            }

            return true;
        }

        /// <summary>
        /// Helper method to set a cell value based on it's location relative to a sourceCell.
        /// </summary>
        /// <param name="spreadsheet">Spreadsheet to use.</param>
        /// <param name="worksheet">Worksheet to use.</param>
        /// <param name="sourceCell">The source cell to use as starting point for offset cell location.</param>
        /// <param name="RowOffset">The row offset from the source cell to change.</param>
        /// <param name="ColumnOffset">The column offset from the source cell to change.</param>
        /// <param name="stringValue">String value to set.</param>
        /// <param name="styleIndex">Style to use.</param>
        /// <param name="useSharedString">Use shared strings? If true and the string isn't found in shared strings, it will be added.</param>
        /// <param name="save">Save the worksheet.</param>
        /// <param name="newColumnWidth">Width of any new columns.</param>
        /// <returns>True if successful.</returns>
        public static bool SetCellValueByOffset(SpreadsheetDocument spreadsheet, Worksheet worksheet,
            Cell sourceCell, uint RowOffset, long ColumnOffset, string stringValue, uint? styleIndex, bool useSharedString, bool save = true, double newColumnWidth = NEW_COLUMN_WIDTH)
        {
            if (sourceCell == null) { throw new ArgumentNullException(nameof(sourceCell)); }

            uint sourceRowReferencePart = ExcelUtilities.ParseRowIndex(sourceCell.CellReference.Value);
            long sourceColumnReferencePart = TableRange.GetColumnNumber(ExcelUtilities.ParseColumnName(sourceCell.CellReference.Value));

            uint targetRowReferencPart = sourceRowReferencePart + RowOffset;
            long targetColumnReferencePart = sourceColumnReferencePart + ColumnOffset;

            return SetCellValue(spreadsheet, worksheet, targetRowReferencPart, targetColumnReferencePart, stringValue, styleIndex, useSharedString, save, newColumnWidth);
        }

        /// <summary>Sets a cell value to the specified stringValue.</summary>
        /// <param name="spreadsheet">Spreadsheet to use.</param>
        /// <param name="worksheet">Worksheet to use.</param>
        /// <param name="rowIndex">Index of the row (1-based).</param>
        /// <param name="columnIndex">Index of the column (1-based).</param>
        /// <param name="stringValue">String value to set.</param>
        /// <param name="styleIndex">Style to use.</param>
        /// <param name="useSharedString">Use shared strings? If true and the string isn't found in shared strings, it will be added.</param>
        /// <param name="save">Save the worksheet.</param>
        /// <param name="newColumnWidth">Width of any new columns.</param>
        /// <returns>True if successful.</returns>
        public static bool SetCellValue(SpreadsheetDocument spreadsheet, Worksheet worksheet, uint rowIndex, long columnIndex, string stringValue, uint? styleIndex, bool useSharedString, bool save = true, double newColumnWidth = NEW_COLUMN_WIDTH)
        {
            if (stringValue == null) { stringValue = string.Empty; }
            string columnValue = stringValue;
            CellValues cellValueType;

            // Add the shared string if necessary
            if (useSharedString)
            {
                if (IndexOfSharedString(spreadsheet, stringValue) == -1)
                {
                    AddSharedString(spreadsheet, stringValue, true);
                }
                columnValue = IndexOfSharedString(spreadsheet, stringValue).ToString();
                cellValueType = CellValues.SharedString;
            }
            else
            {
                cellValueType = CellValues.String;
            }

            return SetCellValue(worksheet, rowIndex, columnIndex, cellValueType, columnValue, styleIndex, save, newColumnWidth);
        }

        /// <summary>
        /// Sets a cell value. The row and the cell are created if they do not exist. If the cell exists, the contents of the cell is overwritten.
        /// </summary>
        /// <param name="worksheet">Worksheet to use.</param>
        /// <param name="rowIndex">Index of the row (1-based).</param>
        /// <param name="columnIndex">Index of the column (1-based).</param>
        /// <param name="valueType">Type of the value.</param>
        /// <param name="value">The actual value.</param>
        /// <param name="styleIndex">Index of the style to use. Null if no style is to be defined.</param>
        /// <param name="save">Save the worksheet?</param>
        /// <param name="newColumnWidth">Width of any new columns.</param>
        /// <returns>True if successful.</returns>
        public static bool SetCellValue(Worksheet worksheet, uint rowIndex, long columnIndex,
            CellValues valueType, string value, uint? styleIndex, bool save = true, double newColumnWidth = NEW_COLUMN_WIDTH)
        {
            if (worksheet == null) { throw new ArgumentNullException(nameof(worksheet)); }

            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            Row row;
            Row previousRow = null;
            Cell cell;
            Cell previousCell = null;
            Column previousColumn = null;
            string cellAddress = TableRange.GetColumnName(columnIndex) + rowIndex;

            // Check if the row exists, create if necessary
            if (sheetData.Elements<Row>().Count(item => item.RowIndex == rowIndex) != 0)
            {
                row = sheetData.Elements<Row>().First(item => item.RowIndex == rowIndex);
            }
            else
            {
                row = new Row() { RowIndex = rowIndex };
                for (uint counter = rowIndex - 1; counter > 0; counter--)
                {
                    previousRow = sheetData.Elements<Row>().FirstOrDefault(item => item.RowIndex == counter);
                    if (previousRow != null)
                    {
                        break;
                    }
                }
                sheetData.InsertAfter(row, previousRow);
            }

            // Check if the cell exists, create if necessary
            if (row.Elements<Cell>().Any(item => item.CellReference.Value == cellAddress))
            {
                cell = row.Elements<Cell>().First(item => item.CellReference.Value == cellAddress);
            }
            else
            {
                // Find the previous existing cell in the row
                for (long counter = columnIndex; counter >= 0; counter--)
                {
                    previousCell = row.Elements<Cell>().FirstOrDefault(item => item.CellReference.Value == TableRange.GetColumnName(counter) + rowIndex);
                    if (previousCell != null)
                    {
                        break;
                    }
                }
                cell = new Cell() { CellReference = cellAddress };
                row.InsertAfter(cell, previousCell);
            }

            // Check if the column collection exists
            var columns = worksheet.Elements<Columns>().FirstOrDefault() ?? worksheet.InsertAt(new Columns(), 0);

            // Check if the column exists
            if (columns.Elements<Column>().All(item => item.Min != columnIndex))
            {
                // Find the previous existing column in the columns
                for (long counter = columnIndex; counter >= 0; counter--)
                {
                    previousColumn = columns.Elements<Column>().FirstOrDefault(item => item.Min == counter);
                    if (previousColumn != null)
                    {
                        break;
                    }
                }
                uint colMinMax = Convert.ToUInt32(columnIndex + 1);
                columns.InsertAfter(
                   new Column()
                   {
                       Min = colMinMax,
                       Max = colMinMax,
                       CustomWidth = true,
                       Width = newColumnWidth
                   }, previousColumn);
            }

            // Add the value
            cell.CellValue = new CellValue(value);
            if (styleIndex != null)
            {
                cell.StyleIndex = styleIndex;
            }
            if (valueType != CellValues.Date)
            {
                cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(valueType);
            }

            if (save)
            {
                worksheet.Save();
            }

            return true;
        }

        /// <summary>
        /// Hide the specified worksheets.  But always leave at least one worksheet visible.
        /// </summary>
        /// <param name="spreadsheet">The spreadsheet.</param>
        /// <param name="sheetsToHide">The sheets to hide.</param>
        public static void HideWorksheets(SpreadsheetDocument spreadsheet, IList<string> sheetsToHide)
        {
            if (spreadsheet == null) { throw new ArgumentNullException(nameof(spreadsheet)); }
            if (sheetsToHide == null) { throw new ArgumentNullException(nameof(sheetsToHide)); }

            foreach (OpenXmlElement oxe in (spreadsheet.WorkbookPart.Workbook.Sheets).ChildElements)
            {
                string sheetName = ((Sheet)(oxe)).Name;
                IList<uint> visibleSheetIndexes = GetVisibleSheetIndices(spreadsheet.WorkbookPart.Workbook.Sheets);
                // Hide the sheet if it isn't the last visible sheet
                if (sheetsToHide.Contains(sheetName) && visibleSheetIndexes.Count > 1)
                {
                    ((Sheet)(oxe)).State = SheetStateValues.Hidden;

                    WorkbookView wv = spreadsheet.WorkbookPart.Workbook.BookViews.ChildElements.First<WorkbookView>();

                    if (wv != null)
                    {
                        visibleSheetIndexes = GetVisibleSheetIndices(spreadsheet.WorkbookPart.Workbook.Sheets);
                        wv.ActiveTab = visibleSheetIndexes[0];  // activate first visible sheet
                    }
                }
            }
            spreadsheet.WorkbookPart.Workbook.Save();
        }

        /// <summary>
        /// Remove all Table Definition Parts and worksheet data rows/cells.
        /// </summary>
        /// <param name="worksheetPart">The worksheet part.</param>
        public static void RemoveTablesAndData(WorksheetPart worksheetPart)
        {
            if (worksheetPart == null)
            {
                throw new ArgumentNullException(nameof(worksheetPart));
            }

            // Remove all TableDefinitionParts
            worksheetPart.DeleteParts<TableDefinitionPart>(worksheetPart.GetPartsOfType<TableDefinitionPart>());
            // Remove all data rows
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            sheetData.RemoveAllChildren<Row>();
        }


        /// <summary>
        /// Return a list of sheet index values for all visible sheets.
        /// </summary>
        /// <param name="sheets">The sheets.</param>
        /// <returns>List of indices for all visible sheets.</returns>
        internal static IList<uint> GetVisibleSheetIndices(Sheets sheets)
        {
            IList<uint> indexList = new List<uint>();
            uint index = 0;
            foreach (Sheet currentSheet in sheets.Descendants<Sheet>())
            {
                if (currentSheet.State == null || currentSheet.State.Value == SheetStateValues.Visible)
                {
                    indexList.Add(index);
                }
                index++;
            }
            return indexList;
        }

        /// <summary>
        /// Loop through all worksheets and set the SheetView element TabSelected attribute to false.
        /// </summary>
        /// <param name="spreadsheet">The spreadsheet.</param>
        public static void DeselectAllWorksheetTabs(SpreadsheetDocument spreadsheet)
        {
            if (spreadsheet == null) { throw new ArgumentNullException(nameof(spreadsheet)); }

            foreach (WorksheetPart worksheetPart in spreadsheet.WorkbookPart.WorksheetParts)
            {
                foreach (SheetView sheetView in worksheetPart.Worksheet.SheetViews)
                {
                    sheetView.TabSelected = false;
                }

            }
        }


        /// <summary>
        /// Loop through the sheets and find the max sheetId.
        /// </summary>
        /// <param name="sheets">The sheets.</param>
        /// <returns>Max SheetId + 1</returns>
        internal static uint GetNextSheetId(Sheets sheets)
        {
            uint maxSheetId = (uint)sheets.Count();  // default to number of sheets

            // Get the maximum sheetId
            Sheet sheet = sheets.GetFirstChild<Sheet>();
            while (sheet != null)
            {
                if (sheet.SheetId > maxSheetId)
                {
                    maxSheetId = sheet.SheetId;
                }
                sheet = sheet.NextSibling<Sheet>();
            }
            return maxSheetId + 1;
        }

        /// <summary>
        /// Loop through all worksheet parts and return the max TableDefinitionPart ID.
        /// </summary>
        /// <param name="spreadsheet">The spreadsheet to search.</param>
        /// <returns>The max TableDefinitionPart ID.</returns>
        public static uint GetMaxTableDefPartId(SpreadsheetDocument spreadsheet)
        {
            if (spreadsheet == null) { throw new ArgumentNullException(nameof(spreadsheet)); }

            uint maxTableDefPartId = 0U;
            // loop through all WorksheetParts
            foreach (WorksheetPart worksheetPart in spreadsheet.WorkbookPart.WorksheetParts)
            {
                foreach (TableDefinitionPart tdp in worksheetPart.GetPartsOfType<TableDefinitionPart>())
                {
                    if (tdp.Table.Id > maxTableDefPartId)
                    {
                        maxTableDefPartId = tdp.Table.Id;
                    }
                }
            }
            return maxTableDefPartId;
        }
    }

    /// <summary>
    /// Indicates that start/end dates were incorrect
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class DataErrorImportException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataErrorImportException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected DataErrorImportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataErrorImportException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DataErrorImportException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataErrorImportException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DataErrorImportException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataErrorImportException"/> class.
        /// </summary>
        public DataErrorImportException()
            : base()
        {
        }
    }

    /// <summary>
    /// Indicates that start/end dates were incorrect
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class DateImportException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateImportException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected DateImportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateImportException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DateImportException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateImportException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DateImportException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateImportException"/> class.
        /// </summary>
        public DateImportException()
            : base()
        {
        }
    }

    /// <summary>
    /// Indicates that an imported file was not a correct Excel format
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class NotExcelFileException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotExcelFileException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected NotExcelFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotExcelFileException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public NotExcelFileException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotExcelFileException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NotExcelFileException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotExcelFileException"/> class.
        /// </summary>
        public NotExcelFileException()
            : base()
        {
        }
    }

    /// <summary>
    /// Indicates that a required column was missing from the imported file
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class ColumnMissingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnMissingException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected ColumnMissingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnMissingException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ColumnMissingException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnMissingException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ColumnMissingException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnMissingException"/> class.
        /// </summary>
        public ColumnMissingException()
            : base()
        {
        }
    }

    /// <summary>
    /// Indicates that a required cell was missing a value
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class CellValueMissingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CellValueMissingException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected CellValueMissingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CellValueMissingException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public CellValueMissingException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CellValueMissingException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CellValueMissingException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CellValueMissingException"/> class.
        /// </summary>
        public CellValueMissingException()
            : base()
        {
        }
    }

    /// <summary>
    /// Indicates that a unique column has duplicate values in it
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class DuplicateValuesException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateValuesException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected DuplicateValuesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateValuesException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DuplicateValuesException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateValuesException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DuplicateValuesException(String message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateValuesException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="DetailDuplicateMessages">The detail duplicate messages.</param>
        public DuplicateValuesException(String message, Collection<string> DetailDuplicateMessages)
            : base(message)
        {
            this.DetailDuplicateExceptionMessages = DetailDuplicateMessages;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateValuesException"/> class.
        /// </summary>
        public DuplicateValuesException()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets the detail duplicate exception messages.
        /// </summary>
        public Collection<string> DetailDuplicateExceptionMessages { get; set; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <exception cref="System.ArgumentNullException">info</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("DetailDuplicateMessages", DetailDuplicateExceptionMessages);
            base.GetObjectData(info, context);
        }
    }
}
