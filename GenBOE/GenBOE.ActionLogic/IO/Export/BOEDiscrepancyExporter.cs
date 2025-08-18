// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Text.RegularExpressions;
	using DocumentFormat.OpenXml;
	using DocumentFormat.OpenXml.Packaging;
	using DocumentFormat.OpenXml.Spreadsheet;
	using GenBOE.ActionLogic.Reporting;

	[ExcludeFromCodeCoverage]
	public static class BOEDiscrepancyExporter
	{
		/// <summary>
		/// Generates the BOE Discrepancy Report for genBOE Angular
		/// </summary>
		/// <param name="workspaceShortName">workspace shortname</param>
		/// <param name="boeDiscrepancyData">BOE Discrepancy Data</param>
		/// <returns>Memory stream of excel sheet</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static MemoryStream ExportBOEDiscrepancyToExcelFile(string workspaceShortName, ICollection<BoeDiscrepancyReportModelView> boeDiscrepancyData)
		{
			if (boeDiscrepancyData == null)
			{
				throw new ArgumentNullException(nameof(boeDiscrepancyData));
			}

			MemoryStream ms = new MemoryStream();

			using (SpreadsheetDocument doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
			{
				WorkbookPart wbPart = doc.AddWorkbookPart();
				wbPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

				WorkbookStylesPart stylesPart = wbPart.AddNewPart<WorkbookStylesPart>();
				stylesPart.Stylesheet = BuildStyleSheet();
				stylesPart.Stylesheet.Save();

				WorksheetPart wsPart = wbPart.AddNewPart<WorksheetPart>();
				SheetData sheetData = new SheetData();

				uint currentRowIndex = 1;

				Row titleRow = new Row { RowIndex = currentRowIndex++ };
				titleRow.Append(GenerateTitleTextCell($"BOE Discrepancy Report - {workspaceShortName}"));
				sheetData.Append(titleRow);

				Row headerRow = new Row { RowIndex = currentRowIndex++ };
				headerRow.Append(GenerateHeaderCell(string.Empty));
				headerRow.Append(GenerateHeaderCell("WBS"));
				headerRow.Append(GenerateHeaderCell("BOE Title"));
				headerRow.Append(GenerateHeaderCell("CLIN"));
				headerRow.Append(GenerateHeaderCell("BOE Author(s)"));
				sheetData.Append(headerRow);

				foreach (BoeDiscrepancyReportModelView discrepancyData in boeDiscrepancyData)
				{
					Row discrepancyParentRow = new Row { RowIndex = currentRowIndex++ };
					discrepancyParentRow.Append(GenerateTextCell(string.Empty));
					discrepancyParentRow.Append(GenerateTextCell(discrepancyData.Wbs, true));
					discrepancyParentRow.Append(GenerateTextCell(discrepancyData.BoeTitle, true));
					discrepancyParentRow.Append(GenerateTextCell(discrepancyData.Clin, true));
					discrepancyParentRow.Append(GenerateWrappedTextCell(discrepancyData.BoeAuthors, true));
					sheetData.Append(discrepancyParentRow);

					// TaskDetails Header Row Creation
					Row taskDetailsHeaderRow = new Row { RowIndex = currentRowIndex++ };
					taskDetailsHeaderRow.Append(GenerateTaskDetailHeaderCell(string.Empty));
					taskDetailsHeaderRow.Append(GenerateTaskDetailHeaderCell(string.Empty));
					taskDetailsHeaderRow.Append(GenerateTaskDetailHeaderCell("Task Id"));
					taskDetailsHeaderRow.Append(GenerateTaskDetailHeaderCell("Task Title"));
					taskDetailsHeaderRow.Append(GenerateTaskDetailHeaderCell("Discrepancy"));
					sheetData.Append(taskDetailsHeaderRow);

					foreach (BoeTaskDetailsMV taskDetails in discrepancyData.ElementsWithIssues)
					{
						Row taskDetailsRow = new Row { RowIndex = currentRowIndex++ };
						taskDetailsRow.Append(GenerateTextCell(string.Empty));
						taskDetailsRow.Append(GenerateTextCell(string.Empty));
						taskDetailsRow.Append(GenerateTextCell(taskDetails.DisplayedTaskId));
						taskDetailsRow.Append(GenerateTextCell(taskDetails.TaskTitle));
						taskDetailsRow.Append(GenerateTextCell(taskDetails.InconsistencyText));
						sheetData.Append(taskDetailsRow);
					}

					sheetData.Append(new Row { RowIndex = currentRowIndex++ });
				}

				wsPart.Worksheet = new Worksheet();
				wsPart.Worksheet.Append(sheetData);

				Sheets sheets = wbPart.Workbook.AppendChild(new Sheets());
				sheets.Append(new Sheet
				{
					Id = wbPart.GetIdOfPart(wsPart),
					SheetId = 1,
					Name = "Sheet1"
				});

				wbPart.Workbook.Save();
			}

			ms.Position = 0;
			return ms;
		}

		/// <summary>
		/// Regex used to filter out the <br\> in Author(s)
		/// </summary>
		private static readonly Regex BrRegex = new Regex(@"<br\s*/?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// Stylesheet is basically a style template Excel will use to apply fonts/stylings
		/// </summary>
		/// <returns>Excel Stylesheet</returns>
		private static Stylesheet BuildStyleSheet()
		{
			Fonts fonts = new Fonts(
				new Font(),
				new Font(new Bold()),
				new Font(new Bold(), new FontSize { Val = 16.5 }),
				new Font(new Bold(), new Color { Rgb = "FFA52A2A" }),
				new Font(new Bold(), new Color { Rgb = "FFFFFFFF" })
			);

			Fills fills = new Fills(
				new Fill(new PatternFill { PatternType = PatternValues.None }),
				new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),
				new Fill(new PatternFill(new ForegroundColor { Rgb = "FFADD8E6" }) { PatternType = PatternValues.Solid }),
				new Fill(new PatternFill(new ForegroundColor { Rgb = "FF696969" }) { PatternType = PatternValues.Solid })
			);

			Borders borders = new Borders(new Border());

			CellFormats cellFormats = new CellFormats(
				new CellFormat(),
				new CellFormat { ApplyAlignment = true, Alignment = new Alignment { WrapText = true } },
				new CellFormat { FontId = 1, ApplyFont = true },
				new CellFormat { FontId = 1, ApplyFont = true, ApplyAlignment = true, Alignment = new Alignment { WrapText = true } },
				new CellFormat { FontId = 2, ApplyFont = true },
				new CellFormat { FontId = 3, FillId = 2, ApplyFont = true, ApplyFill = true },
				new CellFormat { FontId = 4, FillId = 3, ApplyFont = true, ApplyFill = true }
			);

			return new Stylesheet(fonts, fills, borders, cellFormats);
		}

		/// <summary>
		/// Generate Header Cells
		/// </summary>
		/// <param name="text">header text</param>
		/// <returns>Header Cell</returns>
		private static Cell GenerateHeaderCell(string text)
		{
			return new Cell
			{
				DataType = CellValues.String,
				CellValue = new CellValue(text ?? string.Empty),
				StyleIndex = 6u,
			};
		}

		/// <summary>
		/// Generate the header styling for Task Details
		/// </summary>
		/// <param name="text">header text</param>
		/// <returns></returns>
		private static Cell GenerateTaskDetailHeaderCell(string text)
		{
			return new Cell
			{
				DataType = CellValues.String,
				CellValue = new CellValue(text ?? string.Empty),
				StyleIndex = 5u,
			};
		}

		/// <summary>
		/// Put Text into cell
		/// </summary>
		/// <param name="text">cell value</param>
		/// <param name="bold">is bold or not</param>
		/// <returns>Cell with Text</returns>
		private static Cell GenerateTextCell(string text, bool bold = false)
		{
			return new Cell
			{
				DataType = CellValues.String,
				CellValue = new CellValue(text ?? string.Empty),
				StyleIndex = bold ? 3u : 1u,
			};
		}

		/// <summary>
		/// Add text that needs line breaks in the cell
		/// </summary>
		/// <param name="text">cell value</param>
		/// <param name="bold">is bold or not</param>
		/// <returns>Cell with text wrapped</returns>
		private static Cell GenerateWrappedTextCell(string text, bool bold = false)
		{
			text = text ?? string.Empty;

			if (BrRegex.IsMatch(text))
			{
				text = BrRegex.Replace(text, "\r\n");
			}

			if (text.Contains("\n") && !text.Contains("\r\n"))
			{
				text = text.Replace("\n", "\r\n");
			}

			return new Cell
			{
				DataType = CellValues.InlineString,
				InlineString = new InlineString(
					new Text(text) { Space = SpaceProcessingModeValues.Preserve }
				),
				StyleIndex = bold ? 3u : 1u
			};
		}

		/// <summary>
		/// Title Text with it's own styling
		/// </summary>
		/// <param name="text">Title text</param>
		/// <returns>Title Cell</returns>
		private static Cell GenerateTitleTextCell(string text)
		{
			return new Cell
			{
				DataType = CellValues.String,
				CellValue = new CellValue(text ?? string.Empty),
				StyleIndex = 4u,
			};
		}
	}
}
