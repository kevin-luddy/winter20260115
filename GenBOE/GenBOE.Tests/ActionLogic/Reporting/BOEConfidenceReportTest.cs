// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Reporting
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.Reporting;
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Enums;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class BOEConfidenceReportTest
	{
		/// <summary>
		/// Get the System Under Test
		/// </summary>
		/// <returns>an instance of BOEConfidenceReports</returns>
		public BOEConfidenceReport GetSUT()
		{
			return new BOEConfidenceReport();
		}

		#region PoP Algorithm Tests

		#region Date Format tests

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMMM yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMMM_yyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMMM yyyy"); 
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMM yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMM_yyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMM yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MM yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MM_yyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MM yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "M yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_M_yyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("M yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMMM yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMMM_yy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMMM yy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMM yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMM_yy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMM yy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MM yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MM_yy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MM yy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "M yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_M_yy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("M yy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMMM-yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMMMDashyyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMMM-yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMM-yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMMDashyyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMM-yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MM-yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMDashyyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MM-yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "M-yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MDashyyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("M-yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMMM-yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMMMDashyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMMM-yy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMM-yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMMDashyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMM-yy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MM-yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMDashyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MM-yy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "M-yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MDashyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("M-yy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMMM/yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMMMSlashyyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMMM/yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMM/yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMMSlashyyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMM/yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MM/yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMSlashyyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MM/yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "M/yyyy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MSlashyyyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("M/yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMMM/yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMMMSlashyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMMM/yy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MMM/yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMMSlashyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MMM/yy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "MM/yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MMSlashyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("MM/yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Testing GetPoPConfidenceResults for "M/yy" format
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MSlashyy()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = GetTestStringsForFormat("M/yyyy");
			ICollection<DateRange> dateRanges = GetTestDatesForTestingFormats();

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(dateRanges, ref rteFields);

			AssertDateFormatTestResults(result, dateRanges);
		}

		/// <summary>
		/// Test that GetPoPConfidenceResults for date ranges with mixed formats in case a user does that for some reason
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MixedFormats()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"I don't know why someone would write a date like January 24 to 2/2024 but testing just in case",
				"And a test for a single year range like 03 - Apr 2024 as well"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 2, 15)),
				new DateRange(new DateTime(2024, 3, 15), new DateTime(2024, 4, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.IsTrue(result.PoPDateResults.All(x => x.Value == PoPMatchResult.Match));
		}

		/// <summary>
		/// Get the test strings for RTE fields that contain each month
		/// </summary>
		/// <param name="format">Format of the date string to use</param>
		/// <returns>Strings with formatted dates</returns>
		private ICollection<string> GetTestStringsForFormat(string format)
		{
			DateTime january = new DateTime(2024, 1, 15);
			DateTime february = new DateTime(2018, 2, 15);
			DateTime march = new DateTime(2024, 3, 15);
			DateTime april = new DateTime(2021, 4, 15);
			DateTime may = new DateTime(2021, 5, 15);
			DateTime june = new DateTime(2021, 6, 15);
			DateTime july = new DateTime(2019, 7, 15);
			DateTime august = new DateTime(2020, 8, 15);
			DateTime september = new DateTime(2025, 9, 15);
			DateTime october = new DateTime(2023, 10, 15);
			DateTime november = new DateTime(2025, 11, 15);
			DateTime december = new DateTime(2022, 12, 15);

			return new Collection<string>()
			{
				$"Lorem ipsum {january.ToString(format)} - {march.ToString(format)} dolor sit amet, consectetuer adipiscing elit. {december.ToString(format)} to {october.ToString(format)} Aenean commodo ligula eget dolor.",
				$"Aenean massa. {april.ToString(format)} thru {june.ToString(format)} Cum sociis natoque penatibus {july.ToString(format)} et magnis dis parturient montes {february.ToString(format)} through {august.ToString(format)}, nascetur ridiculus mus.",
				$"Donec quam felis {may.ToString(format)}, ultricies nec, pellentesque eu, pretium quis, sem. {september.ToString(format)} - {november.ToString(format)}"
			};
		}

		/// <summary>
		/// Get test dates to use as PoP dates for the date format tests
		/// </summary>
		/// <returns>test dates</returns>
		private ICollection<DateRange> GetTestDatesForTestingFormats()
		{
			return new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 3, 15)),
				new DateRange(new DateTime(2021, 4, 15), new DateTime(2021, 6, 15)),
				new DateRange(new DateTime(2018, 2, 15), new DateTime(2021, 8, 15)),
				new DateRange(new DateTime(2021, 5, 15), new DateTime(2024, 11, 15)),
				new DateRange(new DateTime(2000, 1, 15), new DateTime(2001, 1, 15))
			};
		}

		/// <summary>
		/// Assert results for date format tests
		/// To be used with GetTestStringsForFormat() and GetTestDatesForTestingFormats()
		/// </summary>
		/// <param name="result">The result to assert</param>
		/// <param name="dateRanges">The test date ranges</param>
		/// <param name="rteFields">The RTE fields after GetPoPConfidenceResults was called</param>
		/// <param name="originalRteFields">The original values of the RTE fields</param>
		private void AssertDateFormatTestResults(ConfidenceReportPoPResultDTO result, ICollection<DateRange> dateRanges)
		{
			Assert.IsNotNull(result);

			// Assert PoP Date results
			Assert.AreEqual(dateRanges.Count, result.PoPDateResults.Count);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(0).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(1).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(2).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(3).Value);
			Assert.AreEqual(PoPMatchResult.Missing, result.PoPDateResults.ElementAt(4).Value);

			// Assert No Match dates
			Assert.AreEqual(6, result.NoMatchDates.Count);
			Assert.IsTrue(result.NoMatchDates.Any(x => x == new DateTime(2022, 12, 15).Normalize()));
			Assert.IsTrue(result.NoMatchDates.Any(x => x == new DateTime(2023, 10, 15).Normalize()));
			Assert.IsTrue(result.NoMatchDates.Any(x => x == new DateTime(2019, 7, 15).Normalize()));
			Assert.IsTrue(result.NoMatchDates.Any(x => x == new DateTime(2020, 8, 15).Normalize()));
			Assert.IsTrue(result.NoMatchDates.Any(x => x == new DateTime(2025, 9, 15).Normalize()));
			Assert.IsTrue(result.NoMatchDates.Any(x => x == new DateTime(2025, 11, 15).Normalize()));
		}

		#endregion Date Format tests

		#region Date separator tests

		/// <summary>
		/// Test that GetPoPConfidenceResults is functional for dates separated by a dash
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_Dash()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This is a test string containing the date range January 2024 - February 2024",
				"This string contains multiple dashes and 03-2023 - 05-2025 as the date range",
				"And another string but with no spaces in 4-2021-6-2022 to be sure that also works",
				"And 7 - 8-2024 for a single year range with multiple dashes",
				"finally a single date of 09-2025 to be sure it's not picked up by the date range regex"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 2, 15)),
				new DateRange(new DateTime(2023, 3, 15), new DateTime(2025, 5, 15)),
				new DateRange(new DateTime(2021, 4, 15), new DateTime(2022, 6, 15)),
				new DateRange(new DateTime(2024, 7, 15), new DateTime(2024, 8, 15)),
				new DateRange(new DateTime(2025, 9, 15), new DateTime(2025, 10, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.AreEqual(ranges.Count, result.PoPDateResults.Count);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(0).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(1).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(2).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(3).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(4).Value);
		}

		/// <summary>
		/// Test that GetPoPConfidenceResults is functional for dates separated by "to"
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_To()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This is a test string containing the date range January 2024 to February 2024",
				"THIS STRING IS ALL CAPS MAR 2023 TO MAY 2025 TO TEST IT IS CASE INSENSITIVE",
				"And 7 to 8-2024 for a single year range",
				"finally October 2020 to October 2024 to make sure the 'to' in October doesn't interfere"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 2, 15)),
				new DateRange(new DateTime(2023, 3, 15), new DateTime(2025, 5, 15)),
				new DateRange(new DateTime(2024, 7, 15), new DateTime(2024, 8, 15)),
				new DateRange(new DateTime(2020, 10, 15), new DateTime(2024, 10, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.AreEqual(ranges.Count, result.PoPDateResults.Count);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(0).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(1).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(2).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(3).Value);
		}

		/// <summary>
		/// Test that GetPoPConfidenceResults is functional for dates separated by "through"
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_Through()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This is a test string containing the date range January 2024 through February 2024",
				"THIS STRING IS ALL CAPS MAR 2023 THROUGH MAY 2025 TO TEST IT IS CASE INSENSITIVE",
				"And 7 through 8-2024 for a single year range"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 2, 15)),
				new DateRange(new DateTime(2023, 3, 15), new DateTime(2025, 5, 15)),
				new DateRange(new DateTime(2024, 7, 15), new DateTime(2024, 8, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.AreEqual(ranges.Count, result.PoPDateResults.Count);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(0).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(1).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(2).Value);
		}

		/// <summary>
		/// Test that GetPoPConfidenceResults is functional for dates separated by "thru"
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_Thru()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This is a test string containing the date range January 2024 thru February 2024",
				"THIS STRING IS ALL CAPS MAR 2023 THRU MAY 2025 TO TEST IT IS CASE INSENSITIVE",
				"And 7 thru 8-2024 for a single year range"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 2, 15)),
				new DateRange(new DateTime(2023, 3, 15), new DateTime(2025, 5, 15)),
				new DateRange(new DateTime(2024, 7, 15), new DateTime(2024, 8, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.AreEqual(ranges.Count, result.PoPDateResults.Count);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(0).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(1).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(2).Value);
		}

		#endregion Date separator tests

		#region PoP Match Result tests

		/// <summary>
		/// Test that PoPMatchResult.Match is returned for full matches
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_Match()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() { 
				"String containing an exact match for 1/2024 to 12/2024"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.First().Value);
		}

		/// <summary>
		/// Test that PoPMatchResult.Partial is returned for only single date matches
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_Partial()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"String containing 1/2024 to 2/2025 which only has the start date",
				"String containing 3/2023 thru 4/2024 which only has the end date",
				"The start date is here May 2024",
				"But the end date is in another field June 2024",
				"And just a single date of 9/24"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 2, 15)),
				new DateRange(new DateTime(2024, 3, 15), new DateTime(2024, 4, 15)),
				new DateRange(new DateTime(2024, 5, 15), new DateTime(2024, 6, 15)),
				new DateRange(new DateTime(2024, 9, 15), new DateTime(2024, 10, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.IsTrue(result.PoPDateResults.All(x => x.Value == PoPMatchResult.Partial));
		}

		/// <summary>
		/// Test that PoPMatchResult.Match is returned when two individual dates that are sequential match the PoP
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_SequentialSingleDates()
		{

			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"Jul 2024 is the start date and the end date is Aug 2024 but they're not written as a date range",
				"December 2024 is the end date and January 2024 is the start date, so this is partial",
				"04/24 is the start date 05/24 is in the middle and 06/2024 is the end date, so it's also partial",
				"A start date of September 2024 in one string",
				"And the end date of October 2024 is in another",
				"Lastly a string with Feb 24 and May 24 and Mar 24 which would result in partial, but then Feb 24 again but this time followed by Mar 24 for a full match"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 7, 15), new DateTime(2024, 8, 15)),
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15)),
				new DateRange(new DateTime(2024, 4, 15), new DateTime(2024, 6, 15)),
				new DateRange(new DateTime(2024, 9, 15), new DateTime(2024, 10, 15)),
				new DateRange(new DateTime(2024, 2, 15), new DateTime(2024, 3, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(0).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(1).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(2).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(3).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(4).Value);
		}

		/// <summary>
		/// Test that PoPMatchResult.Partial is returned when two individual dates that are sequential match the PoP but are in different RTEs even if there's other matching dates in the RTE
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_SequentialSingleDatesDifferentRTE()
		{

			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This string has December 2024 as well as January 2024 in it so both start and end dates are in the same RTE but not sequential",
				"And this string contains December 2024 sequentially after the start date, but is a different RTE"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(0).Value);
		}

		/// <summary>
		/// Test that PoPMatchResult.Missing is returned for missing dates
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_Missing()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This string contains dates like 1/23 and 12-2025 and Feb 2023 thru Aug 2024 but not the PoP Date"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 12, 1))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.AreEqual(PoPMatchResult.Missing, result.PoPDateResults.First().Value);
		}

		#endregion PoP Match Result tests

		#region Other PoP Tests

		/// <summary>
		/// Test GetPoPConfidenceResults for date ranges that only contain a single year (ex. Jan - March 2024)
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_SingleYearRanges()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"String containing a few dates 01 to 12/2024 in a few May - June-2022 different Oct thru Nov 2025 formats"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15)),
				new DateRange(new DateTime(2022, 5, 15), new DateTime(2022, 6, 15)),
				new DateRange(new DateTime(2025, 10, 15), new DateTime(2025, 11, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.IsTrue(result.PoPDateResults.All(x => x.Value == PoPMatchResult.Match));
		}

		/// <summary>
		/// Test expected No Match dates are returned, including those from partial matches, unused date ranges, and unused single dates
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_NoMatchDates()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"String containing a partial start date match 1/2024 to 11/2024",
				"String containing a partial end date match February 2024 through March 2024",
				"Apr 24 thru May 24 is a range that won't be matched and an unused single date of July 2024"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15)),
				new DateRange(new DateTime(2023, 12, 15), new DateTime(2024, 3, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			Assert.IsNotNull(result);
			Assert.AreEqual(5, result.NoMatchDates.Count);
			Assert.IsTrue(result.NoMatchDates.Contains(new DateTime(2024, 11, 15).Normalize()));
			Assert.IsTrue(result.NoMatchDates.Contains(new DateTime(2024, 2, 15).Normalize()));
			Assert.IsTrue(result.NoMatchDates.Contains(new DateTime(2024, 4, 15).Normalize()));
			Assert.IsTrue(result.NoMatchDates.Contains(new DateTime(2024, 5, 15).Normalize()));
			Assert.IsTrue(result.NoMatchDates.Contains(new DateTime(2024, 7, 15).Normalize()));
		}

		/// <summary>
		/// Test dates with matches are not added to No Match dates
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_MatchedDatesNotInNoMatchDates()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"String containing a partial start date match 1/2024 to 11/2024",
				"String containing a partial end date match February 2024 through March 2024",
				"And a full match for 1-22 - 12-22"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15)),
				new DateRange(new DateTime(2023, 12, 15), new DateTime(2024, 3, 15)),
				new DateRange(new DateTime(2022, 1, 15), new DateTime(2022, 12, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			// Assert that dates that have a match are not in the No Match dates list
			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.NoMatchDates.Count);
			Assert.IsFalse(result.NoMatchDates.Contains(new DateTime(2024, 1, 15).Normalize()));
			Assert.IsFalse(result.NoMatchDates.Contains(new DateTime(2024, 3, 15).Normalize()));
			Assert.IsFalse(result.NoMatchDates.Contains(new DateTime(2022, 1, 15).Normalize()));
			Assert.IsFalse(result.NoMatchDates.Contains(new DateTime(2022, 12, 15).Normalize()));
			Assert.IsTrue(result.NoMatchDates.Contains(new DateTime(2024, 11, 15).Normalize()));
			Assert.IsTrue(result.NoMatchDates.Contains(new DateTime(2024, 2, 15).Normalize()));
		}

		/// <summary>
		/// Test GetPoPConfidenceResults for when PoP start and end both have matches, but not in a range in the RTE
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_StartAndEndSeparateMatches()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This string has 2 ranges: Jan - Feb 2024 and November to December 2024"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15)),
				new DateRange(new DateTime(2024, 2, 15), new DateTime(2024, 11, 15)),
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			// Assert that dates that have a match are not in the No Match dates list
			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.PoPDateResults.Count);
			Assert.IsTrue(result.PoPDateResults.All(x => x.Value == PoPMatchResult.Partial));
			Assert.IsFalse(result.NoMatchDates.Contains(ranges.ElementAt(0).StartDate.Value));
			Assert.IsFalse(result.NoMatchDates.Contains(ranges.ElementAt(0).EndDate.Value));
			Assert.IsFalse(result.NoMatchDates.Contains(ranges.ElementAt(1).StartDate.Value));
			Assert.IsFalse(result.NoMatchDates.Contains(ranges.ElementAt(1).EndDate.Value));
		}

		/// <summary>
		/// Test GetPoPConfidenceResults properly removes dates from the rte fields that are passed by ref
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_RteDatesRemoved()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This string has a January 2024 - February 2024 date range in it",
				"This string has a Mar 2024 single date in it",
				"This string 04-24 is filled 5-2025 with multiple Jun 2024 thru May 2025 dates and August to December 2025 ranges"
			};

			ICollection<string> originalRteFields = rteFields;

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			CollectionAssert.AreNotEqual(originalRteFields.ToList(), rteFields.ToList());
			Assert.IsFalse(rteFields.ElementAt(0).Contains("January 2024 - February 2024"));
			Assert.IsFalse(rteFields.ElementAt(1).Contains("Mar 2024"));
			Assert.IsFalse(rteFields.ElementAt(2).Contains("04-24"));
			Assert.IsFalse(rteFields.ElementAt(2).Contains("5-2025"));
			Assert.IsFalse(rteFields.ElementAt(2).Contains("Jun 2024 thru May 2025"));
			Assert.IsFalse(rteFields.ElementAt(2).Contains("August to December 2025"));
		}

		/// <summary>
		/// Test GetPoPConfidenceResults when there are duplicate PoP dates
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_DuplicatePoP()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This is a test string Jan 2024 to Dec 2024 for an RTE it contains a date"
			};

			// 3 of the same PoP
			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15)),
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15)),
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			// Assert that there are 3 PoP results and all are matched
			Assert.IsNotNull(result);
			Assert.AreEqual(3, result.PoPDateResults.Count);
			Assert.IsTrue(result.PoPDateResults.All(x => x.Value == PoPMatchResult.Match));
		}

		/// <summary>
		/// Test GetPoPConfidenceResults when there are duplicate dates in the RTEs
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_DuplicateRteDates()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This is a test string Jan 2024 to Dec 2024 for an RTE it contains the same date range Jan 2024 to Dec 2024 multiple times Jan 2024 to Dec 2024",
				"This 02/2024-03/2024 string contains Feb 2024 thru Mar 2024 the same date range in February to March 24 multiple formats",
				"And this Apr 2024 string contains a single Apr 2024 date multiple times for partial Apr 2024 matches",
				"Last June 24 string also contains 06/2024 duplicates of the same 6/24 single date in multiple formats"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15)),
				new DateRange(new DateTime(2024, 2, 15), new DateTime(2024, 3, 15)),
				new DateRange(new DateTime(2024, 4, 15), new DateTime(2024, 5, 15)),
				new DateRange(new DateTime(2024, 5, 15), new DateTime(2024, 6, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			// Assert that there are only 4 PoP results and no No Match dates
			Assert.IsNotNull(result);
			Assert.AreEqual(4, result.PoPDateResults.Count);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(0).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(1).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(2).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(3).Value);
			Assert.IsFalse(result.NoMatchDates.Any());
		}

		/// <summary>
		/// Test GetPoPConfidenceResults when two PoP dates overlap on start/end
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_OverlappingPoPStartEnd()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This is a test string Jan 2024 - Dec 2024"
			};

			// Date ranges that start/end on both Jan 2024 and Dec 2024
			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15)),
				new DateRange(new DateTime(2023, 12, 15), new DateTime(2024, 1, 15)),
				new DateRange(new DateTime(2024, 12, 15), new DateTime(2025, 1, 15)),
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 2, 15)),
				new DateRange(new DateTime(2024, 11, 15), new DateTime(2024, 12, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			// Assert that there are 5 PoP results and with the expected match result
			Assert.IsNotNull(result);
			Assert.AreEqual(5, result.PoPDateResults.Count);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(0).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(1).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(2).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(3).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(4).Value);
		}

		/// <summary>
		/// Test GetPoPConfidenceResults when two dates overlap on start/end in the RTE
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_OverlappingRTEStartEnd()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This Nov 2023 to Jan 24 is a test string Jan 2024 - Dec 2024 with overlapping Nov thru Dec 2024 dates"
			};

			// Date ranges that match the ranges and have partial matches on the overlapping dates
			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15)),
				new DateRange(new DateTime(2023, 11, 15), new DateTime(2024, 1, 15)),
				new DateRange(new DateTime(2024, 11, 15), new DateTime(2024, 12, 15)),
				new DateRange(new DateTime(2023, 12, 15), new DateTime(2024, 1, 15)),
				new DateRange(new DateTime(2024, 10, 15), new DateTime(2024, 12, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			// Assert that there are 5 PoP results and with the expected match result
			Assert.IsNotNull(result);
			Assert.AreEqual(5, result.PoPDateResults.Count);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(0).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(1).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(2).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(3).Value);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(4).Value);
		}

		/// <summary>
		/// Test GetPoPConfidenceResults when a PoP is 1 month long and has the same start and end date
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_SamePoPStartEnd()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This PoP is only 1 month January to January 2024",
				"And a single date Feb 2024 that should also match"
			};

			// Date ranges that match the ranges and have partial matches on the overlapping dates
			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 1, 15)),
				new DateRange(new DateTime(2024, 2, 15), new DateTime(2024, 2, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			// Assert that both PoP dates return as a full match
			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.PoPDateResults.Count);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(0).Value);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(1).Value);
		}

		/// <summary>
		/// Test GetPoPConfidenceResults does not find dates that include days (ex. 1/1/2024)
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_DayDates()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This string has a date range with the day in it January 1 2024 - December 31 2024",
				"And more formats like 1/1/2024 to 12/31/2024 and 1-1-24 thru 12-31-24 and Jan 1 24 through Dec 31 24",
				"Lastly some individual dates 01/1/24, 12-31-2024, Jan 1 24, December 31 24"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			// Assert that both the PoP is not found and there are no No Match dates because the dates won't be found
			Assert.IsNotNull(result);
			Assert.AreEqual(PoPMatchResult.Missing, result.PoPDateResults.ElementAt(0).Value);
			Assert.IsFalse(result.NoMatchDates.Any());
		}

		/// <summary>
		/// Test GetPoPConfidenceResults gets a partial match when one of the dates in a range contains a day
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_DayDatesParital()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"This string contains date ranges like Jan 1 2024 to December 2024 or 01-2024 thru 12-31-2024 with a day in only one of the dates"
			};

			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 15), new DateTime(2024, 12, 15))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			// Assert that both the PoP is partially found
			Assert.IsNotNull(result);
			Assert.AreEqual(PoPMatchResult.Partial, result.PoPDateResults.ElementAt(0).Value);
			Assert.IsFalse(result.NoMatchDates.Any());
		}

		/// <summary>
		/// Test GetPoPConfidenceResults when provided PoP Dates that are not normalized
		/// </summary>
		[TestMethod]
		public void TestGetPoPConfidenceResults_NonNormalizedDates()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> rteFields = new Collection<string>() {
				"The PoP is January to December 2024"
			};

			// Date range with non-normalized dates (15th of the month) that still match the dates in the RTE
			ICollection<DateRange> ranges = new Collection<DateRange>()
			{
				new DateRange(new DateTime(2024, 1, 5), new DateTime(2024, 12, 25))
			};

			ConfidenceReportPoPResultDTO result = sut.GetPoPConfidenceResults(ranges, ref rteFields);

			// Assert that both PoP dates return as a full match
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.PoPDateResults.Count);
			Assert.AreEqual(PoPMatchResult.Match, result.PoPDateResults.ElementAt(0).Value);
		}

		#endregion Other PoP Tests

		#endregion PoP Algorithm Tests

		#region Math Algorithm Tests

		/// <summary>
		/// SomeMatchesTestForGetMathConfidenceResults
		/// </summary>
		[TestMethod]
		public void SomeMatchesTestForGetMathConfidenceResults()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> sampleNumbersToMatch = new Collection<string>
			{
				"1 + 2 + 34 - 6/5*12.0 + variableHERE - 3 work / 212314142124-313 = 414,000"
			};

			ICollection<string> sampleRteFields = new Collection<string>
			{
				"1 random text",
				"34.0 hmmm + yep 12",
				"313",
				"25 work3",
				" 16",
				"idk, random string: no numbers here!!",
				" result = 414,000 ",
				"414000"
			};

			ICollection<decimal> expectedMatches = sut.ExtractDecimals(sampleRteFields);
			ConfidenceReportMathResultDTO result = sut.GetMathConfidenceResults(sampleNumbersToMatch, ref sampleRteFields);

			foreach (decimal value in expectedMatches)
			{
				if (result.Matches.ContainsKey(value))
				{
					Assert.IsTrue(result.Matches[value]);
				}
			}
		}

		/// <summary>
		/// EmptyFirstCollectionTestForGetMathConfidenceResults
		/// </summary>
		[TestMethod]
		public void EmptyFirstCollectionTestForGetMathConfidenceResults()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> sampleNumbersToMatch = new Collection<string>();

			ICollection<string> sampleRteFields = new Collection<string>
			{
				"1 random text",
				"34 hmmm + yep 12",
				"313",
				"25 work3",
				" 16",
				"idk, random string: no numbers here!!",
				" result = 414,000 ",
				"414000"
			};

			ICollection<decimal> expectedMatches = sut.ExtractDecimals(sampleRteFields);
			ConfidenceReportMathResultDTO result = sut.GetMathConfidenceResults(sampleNumbersToMatch, ref sampleRteFields);

			foreach (decimal value in expectedMatches)
			{
				if (result.Matches.ContainsKey(value))
				{
					Assert.IsTrue(result.Matches[value]);
				}
			}
		}

		/// <summary>
		/// EmptySecondCollectionTestForGetMathConfidenceResults
		/// </summary>
		[TestMethod]
		public void EmptySecondCollectionTestForGetMathConfidenceResults()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> sampleNumbersToMatch = new Collection<string>
			{
				"1 + 2 + 34 - 6/5*12 + variableHERE - 3 work / 212314142124-313 = 414,000"
			};

			ICollection<string> sampleRteFields = new Collection<string>();

			ICollection<decimal> expectedMatches = sut.ExtractDecimals(sampleRteFields);
			ConfidenceReportMathResultDTO result = sut.GetMathConfidenceResults(sampleNumbersToMatch, ref sampleRteFields);

			foreach (decimal value in expectedMatches)
			{
				if (result.Matches.ContainsKey(value))
				{
					Assert.IsTrue(result.Matches[value]);
				}
			}
		}

		/// <summary>
		/// EmptyCollectionsTestForGetMathConfidenceResults
		/// </summary>
		[TestMethod]
		public void EmptyCollectionsTestForGetMathConfidenceResults()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> sampleNumbersToMatch = new Collection<string>();

			ICollection<string> sampleRteFields = new Collection<string>();

			ICollection<decimal> expectedMatches = sut.ExtractDecimals(sampleRteFields);
			ConfidenceReportMathResultDTO result = sut.GetMathConfidenceResults(sampleNumbersToMatch, ref sampleRteFields);

			foreach (decimal value in expectedMatches)
			{
				if (result.Matches.ContainsKey(value))
				{
					Assert.IsTrue(result.Matches[value]);
				}
			}
		}

		/// <summary>
		/// NoMatchesTestForGetMathConfidenceResults
		/// </summary>
		[TestMethod]
		public void NoMatchesTestForGetMathConfidenceResults()
		{
			BOEConfidenceReport sut = GetSUT();

			ICollection<string> sampleNumbersToMatch = new Collection<string>
			{
				"1 + 2 + 34 - 6/5*12 + variableHERE - 3 work / 212314142124-313 = 414,000"
			};

			ICollection<string> sampleRteFields = new Collection<string>
			{
				"121 random text",
				"345 hmmm + yep 12.1",
				"3133",
				"25 work31",
				" 16",
				"idk, random string: no numbers here!!",
				" result = 414,000,000 ",
				"4144000"
			};

			ICollection<decimal> expectedMatches = sut.ExtractDecimals(sampleRteFields);
			ConfidenceReportMathResultDTO result = sut.GetMathConfidenceResults(sampleNumbersToMatch, ref sampleRteFields);

			foreach (decimal value in expectedMatches)
			{
				if (result.Matches.ContainsKey(value))
				{
					Assert.IsTrue(result.Matches[value]);
				}
			}
		}

		#endregion Math Algorithm Tests
	}
}
