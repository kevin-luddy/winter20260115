// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Reporting
{
	using GenBOE.ActionLogic.Reporting;
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
	}
}
