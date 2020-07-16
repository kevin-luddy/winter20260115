// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System;
    using DocumentFormat.OpenXml.Spreadsheet;
    using IES.Common.OfficeUtilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TableRangeTest
    {
        [TestMethod]
        public void TR_ConstructorTest()
        {
            string wsName = "WSName";
            Table table = new Table()
                {
                    DisplayName = "Test",
                    Id = 1,
                    Name = "Test",
                    Reference = "B2:D10",
                    TableColumns = new TableColumns()
                    {
                    }
                };
            TableRange sut = new TableRange(wsName, table);
            Assert.AreEqual(sut.SheetName, wsName);
            Assert.AreEqual(sut.Table, table);
            Assert.AreEqual(sut.TableName, "Test");
            Assert.AreEqual(sut.Begin, "B2");
            Assert.AreEqual(sut.End, "D10");
            Assert.AreEqual(sut.RowBegin, 2U);
            Assert.AreEqual(sut.RowEnd, 10U);
            Assert.AreEqual(sut.ColumnBegin, "B");
            Assert.AreEqual(sut.ColumnEnd, "D");
            Assert.AreEqual(sut.ColumnBeginIndex, 2);
            Assert.AreEqual(sut.ColumnEndIndex, 4);
        }

        [TestMethod]
        public void TR_ConstructorWideRightTest()
        {
            string wsName = "WSName";
            Table table = new Table()
            {
                DisplayName = "Test",
                Id = 1,
                Name = "Test",
                Reference = "B2:AD10",
                TableColumns = new TableColumns()
                {
                }
            };
            TableRange sut = new TableRange(wsName, table);
            Assert.AreEqual(sut.SheetName, wsName);
            Assert.AreEqual(sut.Table, table);
            Assert.AreEqual(sut.TableName, "Test");
            Assert.AreEqual(sut.Begin, "B2");
            Assert.AreEqual(sut.End, "AD10");
            Assert.AreEqual(sut.RowBegin, 2U);
            Assert.AreEqual(sut.RowEnd, 10U);
            Assert.AreEqual(sut.ColumnBegin, "B");
            Assert.AreEqual(sut.ColumnEnd, "AD");
            Assert.AreEqual(sut.ColumnBeginIndex, 2);
            Assert.AreEqual(sut.ColumnEndIndex, 30);
        }

        [TestMethod]
        public void TR_GetColumnNumberTest()
        {
            Assert.AreEqual(TableRange.GetColumnNumber("A"), 1);
            Assert.AreEqual(TableRange.GetColumnNumber("Z"), 26);
            Assert.AreEqual(TableRange.GetColumnNumber("AA"), 27);
            Assert.AreEqual(TableRange.GetColumnNumber("AZ"), 52);
            Assert.AreEqual(TableRange.GetColumnNumber("ZZZZ"), 475254);
        }

        [TestMethod]
        public void TR_GetColumnNameTest()
        {
            Assert.AreEqual(TableRange.GetColumnName(1), "A");
            Assert.AreEqual(TableRange.GetColumnName(26), "Z");
            Assert.AreEqual(TableRange.GetColumnName(27), "AA");
            Assert.AreEqual(TableRange.GetColumnName(52), "AZ");
            Assert.AreEqual(TableRange.GetColumnName(475254), "ZZZZ");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TR_Constructor_NullExceptionTest1()
        {
            TableRange sut = new TableRange(null, null);
            Assert.IsNull(sut);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TR_Constructor_NullExceptionTest2()
        {
            string wsName = "WSName";
            TableRange sut = new TableRange(wsName, null);
            Assert.IsNull(sut);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TR_GetColumnNumber_NullExceptionTest()
        {
            TableRange.GetColumnNumber(null);
        }

    }
}
