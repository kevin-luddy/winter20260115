// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.OfficeUtilities
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using DocumentFormat.OpenXml;
	using DocumentFormat.OpenXml.Packaging;
	using DocumentFormat.OpenXml.Spreadsheet;

	/// <summary>
	/// Class responsible for generic Excel exports.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static class ExcelExporter
	{
		/// <summary>
		/// Exports a collection of strings to a given Excel template.
		/// </summary>
		/// <param name="templateFileLocation">Location of the template file to fill in</param>
		/// <param name="sheets">Collection of worksheet objects containing worksheet name and rows of strings</param>
		/// <returns>The location of the filled in template file</returns>
		public static string ExportToExcelFile(string templateFileLocation, params ExcelExportWorksheet[] sheets)
		{
			return ExportToExcelFile(templateFileLocation, sheets.ToList());
		}

		/// <summary>
		/// Exports a collection of strings to a given Excel template.
		/// </summary>
		/// <param name="templateFileLocation">Location of the template file to fill in</param>
		/// <param name="duplicateTemplateFile">Specifies whether or not the specified template file should be duplicated and the name returned,
		/// or used directly</param>
		/// <param name="sheets">Collection of worksheet objects containing worksheet name and rows of strings</param>
		/// <returns>The location of the filled in template file</returns>
		public static string ExportToExcelFile(string templateFileLocation, bool duplicateTemplateFile, params ExcelExportWorksheet[] sheets)
		{
			return ExportToExcelFile(templateFileLocation, duplicateTemplateFile, sheets.ToList());
		}

		/// <summary>
		/// Exports a collection of strings to a given Excel template.
		/// </summary>
		/// <param name="templateFileLocation">Location of the template file to fill in</param>
		/// <param name="sheets">Collection of worksheet objects containing worksheet name and rows of strings</param>
		/// <returns>The location of the filled in template file</returns>
		public static string ExportToExcelFile(string templateFileLocation, ICollection<ExcelExportWorksheet> sheets)
		{
			return ExportToExcelFile(templateFileLocation, true, sheets);
		}

		/// <summary>
		/// Exports a collection of strings to a given Excel template.
		/// </summary>
		/// <param name="templateFileLocation">Location of the template file to fill in</param>
		/// <param name="duplicateTemplateFile">Specifies whether or not the specified template file should be duplicated and the name returned,
		/// or used directly</param>
		/// <param name="sheets">Collection of worksheet objects containing worksheet name and rows of strings</param>
		/// <returns>The location of the filled in template file</returns>
		public static string ExportToExcelFile(string templateFileLocation, bool duplicateTemplateFile, ICollection<ExcelExportWorksheet> sheets)
		{
			return ExportToExcelFile(templateFileLocation, duplicateTemplateFile, sheets, null);
		}

		/// <summary>
		/// Exports a collection of strings to a given Excel template.
		/// </summary>
		/// <param name="templateFileLocation">Location of the template file to fill in</param>
		/// <param name="duplicateTemplateFile">Specifies whether or not the specified template file should be duplicated and the name returned,
		/// or used directly</param>
		/// <param name="sheets">Collection of worksheet objects containing worksheet name and rows of strings</param>
		/// <param name="startRows">The start rows for the sheets passed in, can be null.</param>
		/// <param name="updateHoursLabel">Whether or not to update the HOURS label to EPs.</param>
		/// <returns>The location of the filled in template file</returns>
		public static string ExportToExcelFile(string templateFileLocation, bool duplicateTemplateFile, ICollection<ExcelExportWorksheet> sheets, int?[] startRows, bool updateHoursLabel = false)
		{
			// Check inputs
			if (templateFileLocation == null)
			{
				throw new ArgumentNullException(nameof(templateFileLocation));
			}

			if (sheets == null)
			{
				throw new ArgumentNullException(nameof(sheets));
			}

			if (startRows != null && startRows.Length != sheets.Count)
			{
				throw new ArgumentException("The length of start Rows needs to match the count of Sheets");
			}

			string toReturn;

			if (duplicateTemplateFile)
			{
				// Create a new random file name in the specified directory
				toReturn = ExcelUtilities.CopyExcelTemplateFile(templateFileLocation);
			}
			else
			{
				toReturn = templateFileLocation;

				// Make sure that the copied template is writable
				FileInfo copiedFileInfo = new FileInfo(toReturn);
				if (copiedFileInfo.IsReadOnly)
				{
					throw new ArgumentException($"The specified template file '{templateFileLocation}' is read-only. Please set the duplicateTemplateFile parameter to true to make a copy of the template, or pass in the name of an already-copied and writable template file.");
				}
			}

			if (updateHoursLabel)
			{
				UpdateHoursLabel(toReturn);
			}

			for (int i = 0; i < sheets.Count; i++)
			{
				int? startRow = (startRows == null) ? null : startRows[i];
				ExportToCopiedExcelFile(toReturn, sheets.ElementAt(i), startRow);
			}

			return toReturn;
		}

		/// <summary>
		/// Export data to an Excel template
		/// </summary>
		/// <param name="templateFileLocation">Location of the template file to fill in</param>
		/// <param name="populateData">Callback to load the document with data</param>
		/// <returns>The completed Excel file, as a byte stream</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity"), SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
		public static byte[] ExportToExcelFile(string templateFileLocation, PopulateDataDelegate populateData)
		{
			// Check inputs
			if (templateFileLocation == null)
			{
				throw new ArgumentNullException(nameof(templateFileLocation));
			}

			byte[] documentStream;

			// open a copy of the Excel template file into memory
			byte[] byteArray = File.ReadAllBytes(templateFileLocation);

			using (MemoryStream memory = new MemoryStream())
			{
				// synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
				lock (CacheConstants.OPEN_XML_LOCK)
				{
					memory.Write(byteArray, 0, byteArray.Length);

					// Create the document object in memory
					using (SpreadsheetDocument document = SpreadsheetDocument.Open(memory, true))
					{
						// process the document
						populateData?.Invoke(document);
					}

					// Pull the resulting document from memory into the byte array
					documentStream = memory.ToArray();
				}
			}

			return documentStream;
		}

		/// <summary>
		/// Exports a collection of strings to a given Excel template. DOES NOT MAKE A COPY OF THE FILE LOCATION
		/// PASSED IN. USE THIS FUNCTION ONLY AFTER DUPLICATING A TEMPLATE FILE ON DISK.
		/// </summary>
		/// <param name="fileLocation">Location of the already copied file to fill in</param>
		/// <param name="worksheet">Worksheet object containing worksheet name and rows of strings</param>
		/// <param name="startRow">The starting row to copy into.</param>
		/// <returns>The location of the filled in template file</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
		private static void ExportToCopiedExcelFile(string fileLocation, ExcelExportWorksheet worksheet, int? startRow)
		{
			lock (CacheConstants.OPEN_XML_LOCK)
			{
				using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(fileLocation, true))
				{
					// Get the specified worksheet part
					WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, worksheet.WorksheetName);

					PopulateDataRows(spreadsheet, worksheetPart, worksheet, startRow);
				}
			}
		}

		/// <summary>
		/// Worker method for copying row data to an Excel document
		/// </summary>
		/// <param name="document">The spreadsheet document.</param>
		/// <param name="worksheetPart">Worksheet part</param>
		/// <param name="worksheetData">Row data</param>
		/// <param name="startRow">Starting row index</param>
		public static void PopulateDataRows(SpreadsheetDocument document, WorksheetPart worksheetPart, ExcelExportWorksheet worksheetData, int? startRow)
		{
			if (worksheetData == null)
			{
				throw new ArgumentNullException(nameof(worksheetData));
			}

			// If we found the worksheet, let's move on. Otherwise bomb out.
			if (worksheetPart != null)
			{
				Dictionary<uint, uint> columnStyles = new Dictionary<uint, uint>();
				foreach (Column column in worksheetPart.Worksheet.Descendants<Column>())
				{
					if (column.Min.HasValue && column.Style != null && column.Style.HasValue)
					{
						columnStyles.Add(column.Min.Value, column.Style.Value);

						if (column.Max.HasValue && column.Max != column.Min)
						{
							for (uint i = column.Min.Value + 1; i <= column.Max.Value; i++)
							{
								columnStyles.Add(i, column.Style.Value);
							}
						}
					}
				}

				// Grab the sheet data from the selected worksheet
				SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

				// The index to start inserting rows into the Excel file
				Int64 rowIndex = 2;
				if (startRow.HasValue)
				{
					rowIndex = (int)startRow;
				}

				List<Row> originalRows = sheetData.Descendants<Row>().ToList();

				// Iterate over rows of data and append each as an Excel Row object
				foreach (ICollection<string> row in worksheetData)
				{
					Row originalRow = originalRows.FirstOrDefault(r => r.RowIndex == rowIndex);

					Row newRow = ExcelUtilities.CreateExcelContentRow(document, rowIndex, columnStyles, originalRow, row.ToArray());

					if (originalRow != null)
					{
						originalRow.Remove();
					}

					sheetData.AppendChild(newRow);

					rowIndex++;
				}
			}
		}

		/// <summary>
		/// Set data into a series of rows in an Excel document
		/// </summary>
		/// <param name="document">The Spreadsheet document.</param>
		/// <param name="worksheetPart">Worksheet part</param>
		/// <param name="worksheetData">Row data</param>
		/// <param name="startRow">Starting row index</param>

		public static void AssignRowDataValues(SpreadsheetDocument document, WorksheetPart worksheetPart, ExcelExportWorksheet worksheetData, uint startRow)
		{
			if (worksheetData == null)
			{
				throw new ArgumentNullException(nameof(worksheetData));
			}

			// If we found the worksheet, let's move on. Otherwise bomb out.
			if (worksheetPart != null)
			{
				SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
				List<Row> allRows = sheetData.Descendants<Row>().ToList();

				uint rowIndex = startRow;

				foreach (ICollection<string> rowData in worksheetData)
				{
					string[] cellValues = rowData.ToArray();

					Row row = allRows.FirstOrDefault(r => r.RowIndex == rowIndex);

					int cellIdx = 0;
					ICollection<Cell> allCells = row.Elements<Cell>().ToList();
					foreach (Cell cell in allCells)
					{
						ExcelUtilities.SetCellValue(document, cell, cellValues[cellIdx]);

						if (++cellIdx == cellValues.Length)
						{
							break;
						}
					}

					++rowIndex;
				}
			}
		}

		/// <summary>
		/// Adjusts the defined names.
		/// </summary>
		/// <param name="templateFileLocation">The template file location.</param>
		/// <param name="lengths">The lengths.</param>
		[SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
		public static void AdjustDefinedNames(string templateFileLocation, Dictionary<string, int> lengths)
		{
			// Adjust names
			lock (CacheConstants.OPEN_XML_LOCK)
			{
				using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(templateFileLocation, true))
				{
					AdjustDefinedNames(spreadsheet, lengths);
				}
			}
		}

		[SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
		public static void AdjustDefinedNames(SpreadsheetDocument spreadsheet, Dictionary<string, int> lengths)
		{
			if (spreadsheet == null)
			{
				throw new ArgumentNullException(nameof(spreadsheet));
			}

			if (lengths == null)
			{
				throw new ArgumentNullException(nameof(lengths));
			}

			// Adjust names
			foreach (DefinedName name in spreadsheet.WorkbookPart.Workbook.GetFirstChild<DefinedNames>())
			{
				if (lengths.ContainsKey(name.Name))
				{
					int length;
					lengths.TryGetValue(name.Name, out length);

					if (length > 1)
					{
						length++;
						int posReference = name.InnerXml.IndexOf('!');
						string reference = name.InnerXml.Substring(posReference + 1);
						string[] tokens = reference.Split(new char[] { '$', ':' });
						reference = "$" + tokens[1] + "$" + tokens[2] + ":$" + tokens[1] + "$" + length;
						name.Text = name.Text.Substring(0, posReference + 1) + reference;
					}
				}
			}
		}

		/// <summary>
		/// Add the specified definedName/cell reference to the data validations set.
		/// 
		/// Notes:
		/// OpenXML DataValidation elements map a defined name to a sequence of cell references
		/// separated by spaces, e.g. A3 A10 B30:B35 A22.
		/// Each reference may be a single cell or a range of cells.
		/// 
		/// We use dataValidationReferences dictionary to map each defined name to the corresponding sequence of references.
		/// For example:
		///     DefinedName     =>  SequenceOfReferences
		///     ----------------------------------------
		///     CustomField_111			=>  C20 C47 C78
		///     Resource				=>  B28:B30 B46:B48
		///     Business ResourceCode	=>	C28:C30 C46:C48
		///     PerfOrgs				=>  D28:D30 D46:C48
		///     SpreadCurve				=>  G28:G30 G46:G48
		///     CustomField_222			=>  J28:J30 J46:J48
		///     CustomField_333			=>  K28:K30 K46:K48
		/// </summary>
		/// <param name="definedName"></param>
		/// <param name="colIndex"></param>
		/// <param name="startRowIndex"></param>
		/// <param name="endRowIndex"></param>
		/// <param name="dataValidationReferences"></param>
		public static void AddCellReferenceToDataValidationDictionary(Dictionary<string, string> dataValidationReferences,
			string definedName, int colIndex, uint startRowIndex, uint endRowIndex)
		{
			if (dataValidationReferences == null)
			{
				throw new ArgumentNullException(nameof(dataValidationReferences));
			}
			// add cell reference to data validation map
			string cellRange = (startRowIndex == endRowIndex) ?
				ExcelUtilities.GetColumnNameFromColumnIndex(colIndex) + startRowIndex :
				ExcelUtilities.GetColumnNameFromColumnIndex(colIndex) + startRowIndex + ":" + ExcelUtilities.GetColumnNameFromColumnIndex(colIndex) + endRowIndex;
			if (dataValidationReferences.ContainsKey(definedName))
			{
				// append new cell range to existing value(s)
				dataValidationReferences[definedName] = dataValidationReferences[definedName] + " " + cellRange;
			}
			else
			{
				// add new cell range
				dataValidationReferences.Add(definedName, cellRange);
			}
		}

		/// <summary>
		/// Adds the data validations into the worksheet part.
		/// </summary>
		/// <param name="dataValidationReferences">The data validation references.</param>
		/// <param name="worksheetPart">The worksheet part.</param>
		/// <param name="showErrorMessage">Whether to show the error message or not for invalid data.</param>
		public static void AddDataValidations(Dictionary<string, string> dataValidationReferences, WorksheetPart worksheetPart, bool showErrorMessage = true)
		{
			if (dataValidationReferences == null)
			{
				throw new ArgumentNullException(nameof(dataValidationReferences));
			}
			if (worksheetPart == null)
			{
				throw new ArgumentNullException(nameof(worksheetPart));
			}
			DataValidations dataValidations = new DataValidations() { Count = Convert.ToUInt32(dataValidationReferences.Count) };

			// Add data validations for each defined name
			foreach (KeyValuePair<string, string> dataValidationReference in dataValidationReferences)
			{
				DataValidation dataValidation = new DataValidation()
				{
					Type = DataValidationValues.List,
					AllowBlank = true,
					ShowInputMessage = true,
					ShowErrorMessage = showErrorMessage,
					SequenceOfReferences = new ListValue<StringValue>() { InnerText = dataValidationReference.Value.Trim() }
				};
				Formula1 formula1 = new Formula1();
				formula1.Text = dataValidationReference.Key;    // defined name
				dataValidation.Append(formula1);
				dataValidations.Append(dataValidation);
			}

			// NOTE:  The below can cause problems with opening up Excel if there are no current DataValidations, but there are Conditional Formattings.

			// Insert the new data validations after the current ones
			worksheetPart.Worksheet.InsertAfter(dataValidations, worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault());
		}

		public static void AddDefinedNames(SpreadsheetDocument spreadsheet, Dictionary<string, string> newDefinedNames)
		{
			if (spreadsheet == null)
			{
				throw new ArgumentNullException(nameof(spreadsheet));
			}
			if (newDefinedNames == null)
			{
				throw new ArgumentNullException(nameof(newDefinedNames));
			}
			// Add names
			DefinedNames definedNames = spreadsheet.WorkbookPart.Workbook.GetFirstChild<DefinedNames>();
			foreach (KeyValuePair<string, string> newDefinedName in newDefinedNames)
			{
				definedNames.AppendChild(new DefinedName() { Name = newDefinedName.Key, Text = newDefinedName.Value });
			}
		}

		/// <summary>
		/// Updates the Hours labels to EPs if needed.
		/// </summary>
		/// <param name="fileLocation">The location of the file to update.</param>
		private static void UpdateHoursLabel(string fileLocation)
		{

			lock (CacheConstants.OPEN_XML_LOCK)
			{
				using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(fileLocation, true))
				{
					// Change all the text that has Hours to EPs (Equivalent Persons)
					SharedStringTablePart sstpart = spreadsheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();

					foreach (SharedStringItem item in sstpart.SharedStringTable.Elements<SharedStringItem>())
					{
						if (!string.IsNullOrWhiteSpace(item.InnerText) && item.InnerText.Contains("Hours"))
						{
							Text text2 = item.Descendants<Text>().First();
							text2.Text = text2.Text.Replace("Hours", "EPs");
						}
					}
					sstpart.SharedStringTable.Save();
				}
			}
		}
	}

	/// <summary>
	/// Delegate for processing the Excel document
	/// </summary>
	/// <param name="document">The OpenXml Excel document object</param>
	public delegate void PopulateDataDelegate(SpreadsheetDocument document);

}
