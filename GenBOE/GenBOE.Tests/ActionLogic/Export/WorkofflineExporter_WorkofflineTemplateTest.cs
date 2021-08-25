// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.Common;
    using IES.Common;
    using IES.Common.OfficeUtilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test methods for WorkofflineExport.WorkofflineTemplate inner class.
    /// </summary>
    [TestClass]
    public class WorkofflineExporter_WorkofflineTemplateTest
    {
        /// <summary>
        /// Create static Regex object for ColumnName.
        /// </summary>
        private static Regex regexColumnName = new Regex("[A-Za-z]+");

        /// <summary>
        /// Create static Regex object for RowIndex.
        /// </summary>
        private static Regex regexRowIndex = new Regex(@"\d+");

        [TestMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
        public void WE_WT_ConstructorTest()
        {
            byte[] documentStream;

            // open a copy of the Excel template file into memory
            byte[] byteArray = Properties.Resources.Workoffline;

            using (MemoryStream memory = new MemoryStream())
            {
                // synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    memory.Write(byteArray, 0, (int)byteArray.Length);

                    // Create the document object in memory
                    using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(memory, true))
                    {
                        WorkofflineTemplate sut = new WorkofflineTemplate(spreadsheet, ImportExportConstants.TABLE_TEMPLATES);
                        Assert.AreEqual(sut.WorkspaceTable.TableName, ImportExportConstants.WORKSPACE_TABLE_TEMPLATE);
                        Assert.AreEqual(sut.BoeTable.TableName, ImportExportConstants.BOE_TABLE_TEMPLATE);
                        Assert.AreEqual(sut.TaskTable.TableName, ImportExportConstants.TASK_TABLE_TEMPLATE);
                        Assert.AreEqual(sut.ResourceTable.TableName, ImportExportConstants.RESOURCE_TABLE_TEMPLATE);

                        // Verify the Table rows/cells are within the table range
                        VerifyTableRowsAndCellsWithinRange(sut.WorkspaceTable, sut.WorkspaceTableRows);
                        VerifyTableRowsAndCellsWithinRange(sut.BoeTable, sut.BoeTableRows);
                        VerifyTableRowsAndCellsWithinRange(sut.TaskTable, sut.TaskTableRows);
                        VerifyTableRowsAndCellsWithinRange(sut.ResourceTable, sut.ResourceTableRows);

                        // Verify the custom field styles are set
                        Assert.IsTrue(sut.BoeCustomFieldNameStyleIndex > 0);
                        Assert.IsTrue(sut.BoeCustomFieldValueStyleIndex > 0);
                        Assert.IsTrue(sut.TaskCustomFieldNameStyleIndex > 0);
                        Assert.IsTrue(sut.TaskCustomFieldValueStyleIndex > 0);

                        // Verify the Dynamic column styles are set
                        Assert.IsTrue(sut.ResourceBannerCellStyle > 0);
                        Assert.IsTrue(sut.ResourceCustomHeaderStyleIndex > 0);
                        Assert.IsTrue(sut.ResourceHoursHeaderStyleIndex > 0);
                        Assert.IsTrue(sut.ResourceCostHeaderStyleIndex > 0);
                        Assert.IsTrue(sut.ResourceCustomStyleIndex > 0);
                        Assert.IsTrue(sut.ResourceHoursStyleIndex > 0);
                        Assert.IsTrue(sut.ResourceCostStyleIndex > 0);
                    }
                    // Pull the resulting document from memory into the byte array
                    documentStream = memory.ToArray();
                    Assert.IsNotNull(documentStream);
                }
            }
        }

        private void VerifyTableRowsAndCellsWithinRange(TableRange tableRange, IList<Row> tableRows)
        {
            long beginColumnIndex = tableRange.ColumnBeginIndex;
            uint beginRow = tableRange.RowBegin;
            long endColumnIndex = tableRange.ColumnEndIndex;
            uint endRow = tableRange.RowEnd;

            foreach (Row row in tableRows)
            {
                Assert.IsTrue(row.RowIndex >= beginRow && row.RowIndex <= endRow);
                foreach (Cell cell in row.Elements<Cell>())
                {
                    long colIndex = TableRange.GetColumnNumber(ParseColumnName(cell.CellReference.Value));
                    uint rowIndex = ParseRowIndex(cell.CellReference.Value);
                    Assert.IsTrue(colIndex >= beginColumnIndex && colIndex <= endColumnIndex);
                    Assert.IsTrue(rowIndex >= beginRow && rowIndex <= endRow);
                }
            }
        }


        [TestMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
        public void WE_WT_GenerateTableDefinitionPartFromTemplateTest()
        {
            byte[] documentStream;

            // open a copy of the Excel template file into memory
            byte[] byteArray = Properties.Resources.Workoffline;

            using (MemoryStream memory = new MemoryStream())
            {
                // synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    memory.Write(byteArray, 0, (int)byteArray.Length);

                    // Create the document object in memory
                    using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(memory, true))
                    {
                        WorkofflineTemplate sut = new WorkofflineTemplate(spreadsheet, ImportExportConstants.TABLE_TEMPLATES);
                        string partId = "rId99";
                        WorksheetPart worksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>(partId);
                        uint newTableDefPartId = 999U;
                        string newTableName = "Test";
                        long newColBeginIndex = 4;
                        uint newRowBeginIndex = 4;
                        long newColEndIndex = 15;
                        uint newRowEndIndex = 15;

                        TableDefinitionPart newTableDefPart = WorkofflineTemplate.GenerateTableDefinitionPartFromTemplate(sut.ResourceTable, worksheetPart, newTableDefPartId, newTableName, newColBeginIndex, newRowBeginIndex, newColEndIndex, newRowEndIndex);

                        Assert.IsNotNull(newTableDefPart);
                        Assert.AreEqual(worksheetPart.TableDefinitionParts.ToCollection().Count, 1);
                        Assert.AreEqual(worksheetPart.TableDefinitionParts.ToCollection()[0], newTableDefPart);
                        Assert.IsTrue(newTableDefPart.Table.Reference.HasValue);
                        // Verify the new table properties
                        Assert.AreEqual(newTableDefPart.Table.Name.Value, newTableName);
                        Assert.AreEqual(newTableDefPart.Table.Id.Value, newTableDefPartId);
                        Assert.AreEqual(newTableDefPart.Table.Reference.Value, "D4:O15");
                        // Verify additional columns were added
                        long expectedColumns = newColEndIndex - newColBeginIndex + 1U;
                        Assert.AreEqual(newTableDefPart.Table.TableColumns.Count.Value, Convert.ToUInt32(expectedColumns));
                        Assert.AreEqual(newTableDefPart.Table.TableColumns.Elements<TableColumn>().ToCollection().Count, Convert.ToInt32(expectedColumns));
                    }
                    // Pull the resulting document from memory into the byte array
                    documentStream = memory.ToArray();
                    Assert.IsNotNull(documentStream);
                }
            }
        }

        [TestMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
        public void WE_WT_CopyTemplateTableTest()
        {
            byte[] documentStream;

            // open a copy of the Excel template file into memory
            byte[] byteArray = Properties.Resources.Workoffline;

            using (MemoryStream memory = new MemoryStream())
            {
                // synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    memory.Write(byteArray, 0, (int)byteArray.Length);

                    // Create the document object in memory
                    using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(memory, true))
                    {
                        WorkofflineTemplate sut = new WorkofflineTemplate(spreadsheet, ImportExportConstants.TABLE_TEMPLATES);
                        WorksheetPart worksheetPart = AddWorksheet(spreadsheet, "Test");
                        long newColBeginIndex = 4;
                        uint newRowBeginIndex = 4;

                        uint resultRowIndex = WorkofflineTemplate.CopyTemplateTable(sut.ResourceTable, sut.ResourceTableRows, worksheetPart, newColBeginIndex, newRowBeginIndex);
                        // Verify the proper number of rows were added
                        uint expectedNumRowsAdded = (sut.ResourceTable.RowEnd - sut.ResourceTable.RowBegin) + 1;
                        Assert.AreEqual(resultRowIndex, newRowBeginIndex + expectedNumRowsAdded);
                        // Verify correct number of row elements were created
                        Worksheet worksheet = worksheetPart.Worksheet;
                        SheetData sheetData = worksheet.GetFirstChild<SheetData>();
                        Assert.AreEqual(sheetData.Elements<Row>().ToCollection().Count, Convert.ToInt32(expectedNumRowsAdded));
                        // Verify new rows and cells are in the proper locations
                        long expectedColEndIndex = newColBeginIndex + (sut.ResourceTable.ColumnEndIndex - sut.ResourceTable.ColumnBeginIndex);
                        uint expecctedRowEndIndex = newRowBeginIndex + (sut.ResourceTable.RowEnd - sut.ResourceTable.RowBegin);
                        foreach (Row row in sheetData.Elements<Row>())
                        {
                            Assert.IsTrue(row.RowIndex >= newRowBeginIndex && row.RowIndex < resultRowIndex);
                            foreach (Cell cell in row.Elements<Cell>())
                            {
                                long colIndex = TableRange.GetColumnNumber(ParseColumnName(cell.CellReference.Value));
                                uint rowIndex = ParseRowIndex(cell.CellReference.Value);
                                Assert.IsTrue(colIndex >= newColBeginIndex && colIndex <= expectedColEndIndex);
                                Assert.IsTrue(rowIndex >= newRowBeginIndex && rowIndex <= expecctedRowEndIndex);
                            }
                        }
                        // Pull the resulting document from memory into the byte array
                        documentStream = memory.ToArray();
                        Assert.IsNotNull(documentStream);
                    }
                }
            }
        }

        [TestMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
        public void WE_WT_DecrementResourceIdCounterTest()
        {
            byte[] documentStream;

            // open a copy of the Excel template file into memory
            byte[] byteArray = Properties.Resources.Workoffline;

            using (MemoryStream memory = new MemoryStream())
            {
                // synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    memory.Write(byteArray, 0, (int)byteArray.Length);

                    // Create the document object in memory
                    using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(memory, true))
                    {
                        WorkofflineTemplate sut = new WorkofflineTemplate(spreadsheet, ImportExportConstants.TABLE_TEMPLATES);
                        int firstResourceId = sut.DecrementResourceIdCounter();
                        int secondResourceId = sut.DecrementResourceIdCounter();
                        int thirdResourceId = sut.DecrementResourceIdCounter();
                        Assert.IsTrue(firstResourceId < 0);
                        Assert.IsTrue(firstResourceId == secondResourceId + 1);
                        Assert.IsTrue(secondResourceId == thirdResourceId + 1);
                    }
                    // Pull the resulting document from memory into the byte array
                    documentStream = memory.ToArray();
                    Assert.IsNotNull(documentStream);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WE_WT_Constructor_NullExceptionTest1()
        {
            WorkofflineTemplate sut = new WorkofflineTemplate(null, null);
            Assert.IsNull(sut);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
        public void WE_WT_Constructor_NullExceptionTest2()
        {
            byte[] documentStream;

            // open a copy of the Excel template file into memory
            byte[] byteArray = Properties.Resources.Workoffline;

            using (MemoryStream memory = new MemoryStream())
            {
                // synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    memory.Write(byteArray, 0, (int)byteArray.Length);

                    // Create the document object in memory
                    using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(memory, true))
                    {
                        WorkofflineTemplate sut = new WorkofflineTemplate(spreadsheet, null);
                        Assert.IsNull(sut);

                        // Pull the resulting document from memory into the byte array
                        documentStream = memory.ToArray();
                        Assert.IsNotNull(documentStream);
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WE_WT_GenerateTableDefinitionPartFromTemplate_NullExceptionTest1()
        {
            uint newTableDefPartId = 1U;
            long newColBeginIndex = 1;
            uint newRowBeginIndex = 2;
            long newColEndIndex = 3;
            uint newRowEndIndex = 4;

            TableDefinitionPart newTableDefPart = WorkofflineTemplate.GenerateTableDefinitionPartFromTemplate(null, null, newTableDefPartId, null, newColBeginIndex, newRowBeginIndex, newColEndIndex, newRowEndIndex);
            Assert.IsNull(newTableDefPart);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
        public void WE_WT_GenerateTableDefinitionPartFromTemplate_NullExceptionTest2()
        {
            byte[] documentStream;

            // open a copy of the Excel template file into memory
            byte[] byteArray = Properties.Resources.Workoffline;

            using (MemoryStream memory = new MemoryStream())
            {
                // synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    memory.Write(byteArray, 0, (int)byteArray.Length);

                    // Create the document object in memory
                    using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(memory, true))
                    {
                        WorkofflineTemplate sut = new WorkofflineTemplate(spreadsheet, ImportExportConstants.TABLE_TEMPLATES);
                        uint newTableDefPartId = 1U;
                        long newColBeginIndex = 1;
                        uint newRowBeginIndex = 2;
                        long newColEndIndex = 3;
                        uint newRowEndIndex = 4;

                        TableDefinitionPart newTableDefPart = WorkofflineTemplate.GenerateTableDefinitionPartFromTemplate(sut.WorkspaceTable, null, newTableDefPartId, null, newColBeginIndex, newRowBeginIndex, newColEndIndex, newRowEndIndex);
                        Assert.IsNull(newTableDefPart);

                        // Pull the resulting document from memory into the byte array
                        documentStream = memory.ToArray();
                        Assert.IsNotNull(documentStream);
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
        public void WE_WT_GenerateTableDefinitionPartFromTemplate_NullExceptionTest3()
        {
            byte[] documentStream;

            // open a copy of the Excel template file into memory
            byte[] byteArray = Properties.Resources.Workoffline;

            using (MemoryStream memory = new MemoryStream())
            {
                // synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    memory.Write(byteArray, 0, (int)byteArray.Length);

                    // Create the document object in memory
                    using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(memory, true))
                    {
                        WorkofflineTemplate sut = new WorkofflineTemplate(spreadsheet, ImportExportConstants.TABLE_TEMPLATES);
                        string partId = "rId99";
                        WorksheetPart worksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>(partId);
                        uint newTableDefPartId = 1U;
                        long newColBeginIndex = 1;
                        uint newRowBeginIndex = 2;
                        long newColEndIndex = 3;
                        uint newRowEndIndex = 4;

                        TableDefinitionPart newTableDefPart = WorkofflineTemplate.GenerateTableDefinitionPartFromTemplate(sut.WorkspaceTable, worksheetPart, newTableDefPartId, null, newColBeginIndex, newRowBeginIndex, newColEndIndex, newRowEndIndex);
                        Assert.IsNull(newTableDefPart);

                        // Pull the resulting document from memory into the byte array
                        documentStream = memory.ToArray();
                        Assert.IsNotNull(documentStream);
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WE_WT_CopyTemplateTable_NullExceptionTest1()
        {
            long newColBeginIndex = 4;
            uint newRowBeginIndex = 4;

            uint rowIndex = WorkofflineTemplate.CopyTemplateTable(null, null, null, newColBeginIndex, newRowBeginIndex);

            Assert.IsTrue(rowIndex > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
        public void WE_WT_CopyTemplateTable_NullExceptionTest2()
        {
            byte[] documentStream;

            // open a copy of the Excel template file into memory
            byte[] byteArray = Properties.Resources.Workoffline;

            using (MemoryStream memory = new MemoryStream())
            {
                // synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    memory.Write(byteArray, 0, (int)byteArray.Length);

                    // Create the document object in memory
                    using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(memory, true))
                    {
                        WorkofflineTemplate sut = new WorkofflineTemplate(spreadsheet, ImportExportConstants.TABLE_TEMPLATES);
                        long newColBeginIndex = 4;
                        uint newRowBeginIndex = 4;

                        uint rowIndex = WorkofflineTemplate.CopyTemplateTable(sut.ResourceTable, null, null, newColBeginIndex, newRowBeginIndex);

                        Assert.IsTrue(rowIndex > 0);

                        // Pull the resulting document from memory into the byte array
                        documentStream = memory.ToArray();
                        Assert.IsNotNull(documentStream);
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
        public void WE_WT_CopyTemplateTable_NullExceptionTest3()
        {
            byte[] documentStream;

            // open a copy of the Excel template file into memory
            byte[] byteArray = Properties.Resources.Workoffline;

            using (MemoryStream memory = new MemoryStream())
            {
                // synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    memory.Write(byteArray, 0, (int)byteArray.Length);

                    // Create the document object in memory
                    using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(memory, true))
                    {
                        WorkofflineTemplate sut = new WorkofflineTemplate(spreadsheet, ImportExportConstants.TABLE_TEMPLATES);
                        long newColBeginIndex = 4;
                        uint newRowBeginIndex = 4;

                        uint rowIndex = WorkofflineTemplate.CopyTemplateTable(sut.ResourceTable, sut.ResourceTableRows, null, newColBeginIndex, newRowBeginIndex);

                        Assert.IsTrue(rowIndex > 0);

                        // Pull the resulting document from memory into the byte array
                        documentStream = memory.ToArray();
                        Assert.IsNotNull(documentStream);
                    }
                }
            }
        }
        /// <summary>
        /// Given a cell name, parse the specified cell to get the column name.
        /// </summary>
        /// <param name="cellName">The name of the cell to get the cooumn name from</param>
        /// <returns>An Excel column name (i.e. A, B, C, etc)</returns>
        private string ParseColumnName(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            var match = regexColumnName.Match(cellName);

            // Return the column name
            return match.Value;
        }

        /// <summary>
        /// Given a cell name, parse the specified cell to get the row index.
        /// </summary>
        /// <param name="cellName">The name of the cell to get the row index from</param>
        /// <returns>An Excel row index (i.e. 1, 2, 3, etc)</returns>
        private uint ParseRowIndex(string cellName)
        {
            // Create a regular expression to match the row index portion the cell name.
            var match = regexRowIndex.Match(cellName);

            // Return the row index
            return uint.Parse(match.Value);
        }

        private WorksheetPart AddWorksheet(SpreadsheetDocument spreadsheet, string name)
        {
            Sheets sheets = spreadsheet.WorkbookPart.Workbook.GetFirstChild<Sheets>();
            Sheet sheet;
            WorksheetPart worksheetPart;

            // Generate a unique Sheet Id
            uint sheetId = GetNextSheetId(spreadsheet.WorkbookPart.Workbook.Sheets);
            SheetProperties sheetProperties = new SheetProperties() { CodeName = "Sheet" + sheetId };

            // Add the worksheetpart
            worksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
            SheetData sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetProperties, sheetData);
            worksheetPart.Worksheet.Save();

            // Add the sheet and make relation to workbook
            sheet = new Sheet()
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = sheetId,
                Name = name
            };
            sheets.Append(sheet);
            spreadsheet.WorkbookPart.Workbook.Save();

            return worksheetPart;
        }

        private uint GetNextSheetId(Sheets sheets)
        {
            uint maxSheetId = 0U;

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
    }
}
