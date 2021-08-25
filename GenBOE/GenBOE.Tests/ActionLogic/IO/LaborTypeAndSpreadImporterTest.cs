// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.IO
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.IO.Import;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class for LaborTypeAndSpreadImporterTest
    /// </summary>
    [TestClass]
    public class LaborTypeAndSpreadImporterTest
    {
        [TestMethod]
        public void Test_NormalizeImportTypesResults_Null()
        {
            LaborTypeAndSpreadImporter.NormalizeImportTypesResults(null);

            Assert.IsTrue(true); // if it didn't throw an exception, we are good :)
        }

        [TestMethod]
        public void Test_NormalizeImportTypesResults_WithData()
        {
            ImportedLaborType input = new ImportedLaborType();
            input.ImportTypes = new Collection<LaborTypeImportResult>()
            {
                LaborTypeImportResult.MissingData,
                LaborTypeImportResult.ResourceMultiValuesInvalid,
                LaborTypeImportResult.InvalidData,
                LaborTypeImportResult.CostDecimalPrecisionViolation, 
                LaborTypeImportResult.CostSpreadRangeInvalid,
                LaborTypeImportResult.MissingData,
                LaborTypeImportResult.ResourceMultiValuesInvalid,
                LaborTypeImportResult.InvalidData
            };

            LaborTypeAndSpreadImporter.NormalizeImportTypesResults(input);

            Assert.IsTrue(input.ImportTypes.Count(x => x == LaborTypeImportResult.MissingData) == 0);
            Assert.IsTrue(input.ImportTypes.Count(x => x == LaborTypeImportResult.ResourceMultiValuesInvalid) == 0);
            Assert.IsTrue(input.ImportTypes.Count(x => x == LaborTypeImportResult.InvalidData) == 1);
            Assert.IsTrue(input.ImportTypes.Count(x => x == LaborTypeImportResult.CostDecimalPrecisionViolation) == 0);
            Assert.IsTrue(input.ImportTypes.Count(x => x == LaborTypeImportResult.CostSpreadRangeInvalid) == 1);
        }

        [TestMethod]
        public void IsInvalidSpreadDateColumnHeader_Test()
        {
            string[] allColumns = { "Hello", "12/2017", "01/2018", "02/2018", "World" };
            Assert.IsFalse(LaborTypeAndSpreadImporter.IsInvalidSpreadDateColumnHeader("01/2018", allColumns));      // valid (MM/YYYY date in allColumns)
            Assert.IsFalse(LaborTypeAndSpreadImporter.IsInvalidSpreadDateColumnHeader("1/2018", allColumns));       // valid (MM/YYYY date in allColumns)
            Assert.IsFalse(LaborTypeAndSpreadImporter.IsInvalidSpreadDateColumnHeader("01/01/2018", allColumns));   // valid (not MM/YYYY date)
            Assert.IsFalse(LaborTypeAndSpreadImporter.IsInvalidSpreadDateColumnHeader("11/01/2018", allColumns));   // valid (not MM/YYYY date)
            Assert.IsFalse(LaborTypeAndSpreadImporter.IsInvalidSpreadDateColumnHeader("a/2018", allColumns));       // valid (not MM/YYYY date)
            Assert.IsFalse(LaborTypeAndSpreadImporter.IsInvalidSpreadDateColumnHeader("13/2018", allColumns));      // valid (not MM/YYYY date)
            Assert.IsFalse(LaborTypeAndSpreadImporter.IsInvalidSpreadDateColumnHeader("01/18", allColumns));        // valid (not MM/YYYY date)
            Assert.IsFalse(LaborTypeAndSpreadImporter.IsInvalidSpreadDateColumnHeader("Category", allColumns));     // valid (not MM/YYYY date)
            Assert.IsFalse(LaborTypeAndSpreadImporter.IsInvalidSpreadDateColumnHeader("Hello", allColumns));        // valid (in allColumns)
            Assert.IsTrue(LaborTypeAndSpreadImporter.IsInvalidSpreadDateColumnHeader("11/2017", allColumns));       // invalid (MM/YYYY and outside date range)
            Assert.IsTrue(LaborTypeAndSpreadImporter.IsInvalidSpreadDateColumnHeader("03/2018", allColumns));       // invalid (MM/YYYY and outside date range)
        }
    }
}
