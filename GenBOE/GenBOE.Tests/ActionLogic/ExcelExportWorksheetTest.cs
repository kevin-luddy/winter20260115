// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using IES.Common.OfficeUtilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExcelExportWorksheetTest
    {
        [TestMethod]
        public void EEW_DefaultConstructorTest()
        {
            ExcelExportWorksheet sut = new ExcelExportWorksheet();
            Assert.AreEqual(sut.WorksheetName, string.Empty);
        }
        [TestMethod]
        public void EEW_ConstructorTest()
        {
            string wsName = "WSName";
            ExcelExportWorksheet sut = new ExcelExportWorksheet(wsName);
            Assert.AreEqual(sut.WorksheetName, wsName);
        }
        [TestMethod]
        public void EEW_AddCellsTest()
        {
            ExcelExportWorksheet sut = new ExcelExportWorksheet();
            string[] cells1 = { "Hello" };
            string[] cells2 = { "World" };
            string[] cells3 = { "Foo", "Bar" };
            sut.Add(cells1);
            sut.Add(cells2);
            sut.Add(cells3);
            Assert.AreEqual(sut.Count, 3);
        }

    }
}
