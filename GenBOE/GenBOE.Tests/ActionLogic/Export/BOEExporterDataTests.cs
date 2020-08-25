// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Wordprocessing;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.Dtos;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEExporterDataTests
    {
        ValuesByMonth<decimal> valuesByMonth;

        [TestInitialize]
        public void Init()
        {
            valuesByMonth = new ValuesByMonth<decimal>()
            {
                January = 1m,
                February = 2m,
                March = 3m,
                April = 4m,
                May = 5m,
                June = 6m,
                July = 7m,
                August = 8m,
                September = 9m,
                October = 10m,
                November = 11m,
                December = 12m
            };

        }

        [TestMethod]
        public void ValuesByMonth_CloneTest()
        {
            //arrange


            //act
            ValuesByMonth<decimal> clone = (ValuesByMonth<decimal>)valuesByMonth.Clone();

            //assert
            AssertValueByMonthDeepEquality<decimal>(valuesByMonth, clone);

        }

        [TestMethod]
        public void RollupSummaryByYearTableRowData_CloneTest()
        {
            //arrange
            RollupSummaryByYearTableRowData rollup = new RollupSummaryByYearTableRowData()
            {
                Year = 2013,
                YearTotal = 123456.78m,
                MonthlyValues = valuesByMonth
            };

            //act
            RollupSummaryByYearTableRowData clone = (RollupSummaryByYearTableRowData)rollup.Clone();

            //assert
            Assert.AreEqual(rollup.Year, clone.Year);
            Assert.AreEqual(rollup.YearTotal, clone.YearTotal);
            AssertValueByMonthDeepEquality<decimal>(rollup.MonthlyValues, clone.MonthlyValues);
        }

        [TestMethod]
        public void BOEExportTaskContainer_DefaultContructor_Test()
        {
            //act
            BOEExportTaskContainer test = new BOEExportTaskContainer();

            //assert
            Assert.AreEqual(null, test.TaskContainer);
            Assert.AreEqual(null, test.TaskTypeRow);
            Assert.AreEqual(false, test.Duplicated);
        }

        [TestMethod]
        public void BOEExportTaskContainer_TaskContainerContructor_Test()
        {
            //arrange
            OpenXmlElement container = new SdtContentRun(string.Empty);

            //act
            BOEExportTaskContainer test = new BOEExportTaskContainer(container);

            //assert
            Assert.AreEqual(container, test.TaskContainer);
            Assert.AreEqual(null, test.TaskTypeRow);
            Assert.AreEqual(false, test.Duplicated);
        }

        [TestMethod]
        public void BOEExportTaskContainer_TaskContainerAndRowContructor_Test()
        {
            //arrange
            OpenXmlElement container = new SdtContentRun(string.Empty);

            Mock<TableRow> rowMock = new Mock<TableRow>();
            rowMock.Setup(s => s.InnerText).Returns("Test Row");

            //act
            BOEExportTaskContainer test = new BOEExportTaskContainer(container, rowMock.Object);

            //assert
            Assert.AreEqual(container, test.TaskContainer);
            Assert.AreEqual("Test Row", test.TaskTypeRow.InnerText);
            Assert.AreEqual(false, test.Duplicated);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "IES.Common.IO.Export.BOEExportTaskContainer"), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BOEExportTaskContainer_NullTaskContainer_ThrowsTest()
        {
            //act
            new BOEExportTaskContainer(null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "IES.Common.IO.Export.BOEExportTaskContainer"), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BOEExportTaskContainer_NullTaskTypeRow_ThrowsTest()
        {
            //arrange
            OpenXmlElement container = new SdtContentRun(string.Empty);

            //act
            new BOEExportTaskContainer(container, null);
        }

        [TestMethod]
        public void LaborRollupByDateNew_YearAndMonthsConstructor_Test()
        {
            //act
            LaborRollupByDateNew test = new LaborRollupByDateNew(2013, valuesByMonth);

            //assert
            Assert.AreEqual(2013, test.Year);
            Assert.AreEqual(string.Empty, test.Resource);
            Assert.AreEqual(valuesByMonth.January, test.January);
            Assert.AreEqual(valuesByMonth.February, test.February);
            Assert.AreEqual(valuesByMonth.March, test.March);
            Assert.AreEqual(valuesByMonth.April, test.April);
            Assert.AreEqual(valuesByMonth.May, test.May);
            Assert.AreEqual(valuesByMonth.June, test.June);
            Assert.AreEqual(valuesByMonth.July, test.July);
            Assert.AreEqual(valuesByMonth.August, test.August);
            Assert.AreEqual(valuesByMonth.September, test.September);
            Assert.AreEqual(valuesByMonth.October, test.October);
            Assert.AreEqual(valuesByMonth.November, test.November);
            Assert.AreEqual(valuesByMonth.December, test.December);
        }

        [TestMethod]
        public void LaborRollupByDateNew_CalculateTotal_Test()
        {
            //arrange 
            List<decimal> valueArray = new List<decimal>()
            {
                valuesByMonth.January,
                valuesByMonth.February,
                valuesByMonth.March,
                valuesByMonth.April,
                valuesByMonth.May,
                valuesByMonth.June,
                valuesByMonth.July,
                valuesByMonth.August,
                valuesByMonth.September,
                valuesByMonth.October,
                valuesByMonth.November,
                valuesByMonth.December
            };
            decimal valueSum = valueArray.Sum();

            //act
            LaborRollupByDateNew test = new LaborRollupByDateNew(2013, valuesByMonth);

            //assert
            Assert.AreEqual(valueSum, test.Total);
        }

        [TestMethod]
        public void LaborRollupByDateNew_Clone_Test()
        {
            //arrange 
            LaborRollupByDateNew original = new LaborRollupByDateNew(2013, valuesByMonth);

            //act
            LaborRollupByDateNew clone = (LaborRollupByDateNew)original.Clone();

            //assert
            AssertLaborRollupByDateNewDeepEquality(original, clone);
        }

        [TestMethod]
        public void ValuesByMonth_Sum_Test()
        {
            //arrange
            ValuesByMonth<decimal> second = (ValuesByMonth<decimal>)valuesByMonth.Clone();

            //act
            valuesByMonth.Sum(second);

            //assert
            Assert.AreEqual(second.January * 2, valuesByMonth.January);
            Assert.AreEqual(second.February * 2, valuesByMonth.February);
            Assert.AreEqual(second.March * 2, valuesByMonth.March);
            Assert.AreEqual(second.April * 2, valuesByMonth.April);
            Assert.AreEqual(second.May * 2, valuesByMonth.May);
            Assert.AreEqual(second.June * 2, valuesByMonth.June);
            Assert.AreEqual(second.July * 2, valuesByMonth.July);
            Assert.AreEqual(second.August * 2, valuesByMonth.August);
            Assert.AreEqual(second.September * 2, valuesByMonth.September);
            Assert.AreEqual(second.October * 2, valuesByMonth.October);
            Assert.AreEqual(second.November * 2, valuesByMonth.November);
            Assert.AreEqual(second.December * 2, valuesByMonth.December);
        }

        [TestMethod]
        public void LaborRollupByDateNew_Merge_Test()
        {
            //arrange
            IList<LaborRollupByDateNew> first = new List<LaborRollupByDateNew>() { new LaborRollupByDateNew(2013, valuesByMonth) };
            IList<LaborRollupByDateNew> second = new List<LaborRollupByDateNew>() { new LaborRollupByDateNew(2013, valuesByMonth) };

            LaborRollupByDateNew expectedResult = new LaborRollupByDateNew();
            expectedResult.January = valuesByMonth.January * 2;
            expectedResult.February = valuesByMonth.February * 2;
            expectedResult.March = valuesByMonth.March * 2;
            expectedResult.April = valuesByMonth.April * 2;
            expectedResult.May = valuesByMonth.May * 2;
            expectedResult.June = valuesByMonth.June * 2;
            expectedResult.July = valuesByMonth.July * 2;
            expectedResult.August = valuesByMonth.August * 2;
            expectedResult.September = valuesByMonth.September * 2;
            expectedResult.October = valuesByMonth.October * 2;
            expectedResult.November = valuesByMonth.November * 2;
            expectedResult.December = valuesByMonth.December * 2;
            expectedResult.Resource = string.Empty;
            expectedResult.Year = 2013;



            //act
            List<LaborRollupByDateNew> result = first.Merge(second);

            //assert
            Assert.IsTrue(result.Count == 1);
            AssertLaborRollupByDateNewDeepEquality(expectedResult, result[0]);
        }

        [TestMethod]
        public void LaborRollupByDateNew_MergeTwoDifferentYears_Test()
        {
            //arrange
            IList<LaborRollupByDateNew> first = new List<LaborRollupByDateNew>() { new LaborRollupByDateNew(2012, valuesByMonth) };
            IList<LaborRollupByDateNew> second = new List<LaborRollupByDateNew>() { new LaborRollupByDateNew(2013, valuesByMonth) };

            //act
            List<LaborRollupByDateNew> result = first.Merge(second);

            //assert
            Assert.IsTrue(result.Count == 2);
            AssertLaborRollupByDateNewDeepEquality(first[0], result[0]);
            AssertLaborRollupByDateNewDeepEquality(second[0], result[1]);
        }

        [TestMethod]
        public void LaborRollupByDateNew_ConvertTo_RollupSummaryByYearTableRowData_Test()
        {
            //arrange
            LaborRollupByDateNew source = new LaborRollupByDateNew(2013, valuesByMonth);
            IList<LaborRollupByDateNew> original = new List<LaborRollupByDateNew>() { source };

            //act
            IList<RollupSummaryByYearTableRowData> result = original.Convert();

            //assert
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(source.Year, result[0].Year);
            Assert.AreEqual(source.Total, result[0].YearTotal);
            Assert_LaborRollup_ValueByMonth_DeepEquality(result[0].MonthlyValues, source);
        }

        [TestMethod]
        public void LaborRollupByDateNew_ConvertTo_RollupSummaryByYearTableRowData_NullTest()
        {
            //arrange
            IList<LaborRollupByDateNew> original = null;

            //act
            IList<RollupSummaryByYearTableRowData> result = original.Convert();

            //assert
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void LaborRollupByDateNew_ConvertTo_RollupSummaryByGroupByYearTableData_Test()
        {
            //arrange
            LaborRollupByDateNew data = new LaborRollupByDateNew(2012, valuesByMonth);
            LaborRollupByDateNew data2 = new LaborRollupByDateNew(2013, valuesByMonth);

            Dictionary<int, List<LaborRollupByDateNew>> source =
                new Dictionary<int, List<LaborRollupByDateNew>>();
            source.Add(1, new List<LaborRollupByDateNew>() { data, data2 });

            //act
            RollupSummaryByGroupByYearTableData result = source.Convert();

            //assert
            Assert.AreEqual(data.Total + data2.Total, result.GroupData.ToArray()[0].GroupSubTotal);
            Assert.AreEqual(data.Total, result.GroupData.ToArray()[0].YearlyData.ToArray()[0].YearTotal);
            Assert_LaborRollup_ValueByMonth_DeepEquality(result.GroupData.ToArray()[0].YearlyData.ToArray()[0].MonthlyValues, data);


            Assert.AreEqual(data2.Total, result.GroupData.ToArray()[0].YearlyData.ToArray()[1].YearTotal);
            Assert.AreEqual(data.Total, result.GroupData.ToArray()[0].YearlyData.ToArray()[0].YearTotal);
            Assert_LaborRollup_ValueByMonth_DeepEquality(result.GroupData.ToArray()[0].YearlyData.ToArray()[1].MonthlyValues, data2);

            Assert.AreEqual(data.Total + data2.Total, result.SummaryTotalComplete);
        }

        [TestMethod]
        public void LaborRollupByDateNewDictionary_ConvertToRollupSummaryByYear_Test()
        {
            //arrange
            LaborRollupByDateNew data = new LaborRollupByDateNew(2012, valuesByMonth);
            LaborRollupByDateNew data2 = new LaborRollupByDateNew(2013, valuesByMonth);
            Dictionary<int, List<LaborRollupByDateNew>> source = new Dictionary<int, List<LaborRollupByDateNew>>()
            {
                {0, new List<LaborRollupByDateNew>(){data, data2}}
            };

            //act
            RollupSummaryByYearTableData result = source.ConvertToRollupSummaryByYear();

            //asert
            //the method does not calculate this, SHOULD IT????
            //Assert.AreEqual(data.Total + data2.Total, result.SummaryTotalComplete); 
            Assert_LaborRollup_ValueByMonth_DeepEquality(result.YearlyData.ToArray()[0].MonthlyValues, data);
            Assert_LaborRollup_ValueByMonth_DeepEquality(result.YearlyData.ToArray()[1].MonthlyValues, data2);
        }

        [TestMethod]
        public void LaborRollupByDateNewIList_ConvertToRollupSummaryByYear_Test()
        {
            //arrange
            LaborRollupByDateNew data = new LaborRollupByDateNew(2012, valuesByMonth);
            LaborRollupByDateNew data2 = new LaborRollupByDateNew(2013, valuesByMonth);
            IList<LaborRollupByDateNew> source = new List<LaborRollupByDateNew>() { data, data2 };

            //act
            RollupSummaryByYearTableData result = source.ConvertToRollupSummaryByYear();

            //assert
            Assert.AreEqual(data.Total + data2.Total, result.SummaryTotalComplete);
            Assert_LaborRollup_ValueByMonth_DeepEquality(result.YearlyData.ToArray()[0].MonthlyValues, data);
            Assert_LaborRollup_ValueByMonth_DeepEquality(result.YearlyData.ToArray()[1].MonthlyValues, data2);
        }

        [TestMethod]
        public void LaborRollupByDateNewIList_Clone_Test()
        {
            //arrange
            LaborRollupByDateNew data = new LaborRollupByDateNew(2013, valuesByMonth);
            IList<LaborRollupByDateNew> source = new List<LaborRollupByDateNew>() { data };

            //act
            List<LaborRollupByDateNew> result = source.Clone();

            //assert
            AssertLaborRollupByDateNewDeepEquality(data, result[0]);
        }

        [TestMethod]
        public void BOEExportTaskElementLaborCollection_Convert_Test()
        {
            //arrange
            BOEExportTaskElementLabor data = new BOEExportTaskElementLabor();
            Collection<BOEExportTaskElementLabor> source = new Collection<BOEExportTaskElementLabor>() { data };

            //act
            LaborResourceTypesTableData result = source.Convert(2);

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void BOEExportTaskElementLaborCollection_ConvertTravel_Test()
        {
            //arrange
            BOEExportTaskElementLabor data = new BOEExportTaskElementLabor();
            Collection<BOEExportTaskElementLabor> source = new Collection<BOEExportTaskElementLabor>() { data };

            //act
            TravelResourceTypesTableData result = source.ConvertTravel();

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void BOEExportTaskElementLaborCollection_ConvertRMSTravel_Test()
        {
            //arrange
            BOEExportTaskElementLabor data = new BOEExportTaskElementLabor();
            data.ExportFields[BOEExporterConstants.FieldName_Mode] = MSTTravelMode.ZoneNoAirfare.ToDescription();
            Collection<BOEExportTaskElementLabor> source = new Collection<BOEExportTaskElementLabor>() { data };

            //act
            RMSTravelResourceTypesTableData result = source.ConvertRMSTravel();

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void BOEExportTaskElementLaborCollection_ConvertOCD_Test()
        {
            //arrange
            BOEExportTaskElementLabor data = new BOEExportTaskElementLabor();
            Collection<BOEExportTaskElementLabor> source = new Collection<BOEExportTaskElementLabor>() { data };

            //act
            ODCResourceTypesTableData result = source.ConvertODC();

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void BOEExportTaskElementLaborCollection_ConvertMaterial_Test()
        {
            //arrange
            BOEExportTaskElementLabor data = new BOEExportTaskElementLabor();
            Collection<BOEExportTaskElementLabor> source = new Collection<BOEExportTaskElementLabor>() { data };

            //act
            MaterialResourceTypesTableData result = source.ConvertMaterial();

            //assert
            Assert.IsNotNull(result);
        }

        #region Helper Methods

        private void AssertValueByMonthDeepEquality<T>(ValuesByMonth<T> original, ValuesByMonth<T> compare) where T : struct
        {
            Assert.AreEqual(original.January, compare.January);
            Assert.AreEqual(original.February, compare.February);
            Assert.AreEqual(original.March, compare.March);
            Assert.AreEqual(original.April, compare.April);
            Assert.AreEqual(original.May, compare.May);
            Assert.AreEqual(original.June, compare.June);
            Assert.AreEqual(original.July, compare.July);
            Assert.AreEqual(original.August, compare.August);
            Assert.AreEqual(original.September, compare.September);
            Assert.AreEqual(original.October, compare.October);
            Assert.AreEqual(original.November, compare.November);
            Assert.AreEqual(original.December, compare.December);
        }

        private void AssertLaborRollupByDateNewDeepEquality(LaborRollupByDateNew original, LaborRollupByDateNew compare)
        {
            Assert.AreEqual(original.Year, compare.Year);
            Assert.AreEqual(original.Resource, compare.Resource);
            Assert.AreEqual(original.January, compare.January);
            Assert.AreEqual(original.February, compare.February);
            Assert.AreEqual(original.March, compare.March);
            Assert.AreEqual(original.April, compare.April);
            Assert.AreEqual(original.May, compare.May);
            Assert.AreEqual(original.June, compare.June);
            Assert.AreEqual(original.July, compare.July);
            Assert.AreEqual(original.August, compare.August);
            Assert.AreEqual(original.September, compare.September);
            Assert.AreEqual(original.October, compare.October);
            Assert.AreEqual(original.November, compare.November);
            Assert.AreEqual(original.December, compare.December);
            Assert.AreEqual(original.Total, compare.Total);
        }

        private void Assert_LaborRollup_ValueByMonth_DeepEquality(ValuesByMonth<decimal> original, LaborRollupByDateNew compare)
        {
            Assert.AreEqual(original.January, compare.January);
            Assert.AreEqual(original.February, compare.February);
            Assert.AreEqual(original.March, compare.March);
            Assert.AreEqual(original.April, compare.April);
            Assert.AreEqual(original.May, compare.May);
            Assert.AreEqual(original.June, compare.June);
            Assert.AreEqual(original.July, compare.July);
            Assert.AreEqual(original.August, compare.August);
            Assert.AreEqual(original.September, compare.September);
            Assert.AreEqual(original.October, compare.October);
            Assert.AreEqual(original.November, compare.November);
            Assert.AreEqual(original.December, compare.December);
        }

        #endregion
    }
}
