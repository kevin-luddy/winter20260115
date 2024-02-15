// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.BOECopier
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.ActionLogic.CopyBOE;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using IES.Common;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	/// <summary>
	/// Unit tests for BOECopier class
	/// </summary>
	[TestClass]
	public class BOECopierTests : MOQObject
	{
		/// <summary>
		/// Mocked out MOQ Type Data Loader
		/// </summary>
		private readonly Mock<IMoqTypeDataLoader> moqTypeDataLoader = new Mock<IMoqTypeDataLoader>();

		/// <summary>
		/// Create the System under test
		/// </summary>
		/// <returns>SUT</returns>
		private BOECopier CreateSUT()
		{
			BOECopier copier = new BOECopier(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, moqTypeDataLoader.Object, null);
			moqTypeDataLoader.Setup(c => c.Save(It.IsAny<ICollection<MoqTypeSelection>>())).Returns(new Dictionary<int, int>());

			return copier;
		}

		/// <summary>
		/// Copy MOQ Types using old Fiscal Week scenario for Space
		/// </summary>
		[TestMethod]
		public void CopyMOQTypesOldFiscalWeek()
		{
			BOECopier sut = CreateSUT();
			Utilities.IsSAPEnabledForSystem = true;
			List<MoqTypeSelection> moqTypesToCopy = new List<MoqTypeSelection>
			{
				new MoqTypeSelection
				{
					TableData = new List<MoqTableData>
					{
						new MoqTableData
						{
							QueryType = MoqTableData.WEEKLY,
							PoPStartWeek = 1,
							PoPStartYear = 2019,
							PoPEndWeek = 3,
							PoPEndYear = 2020
						}
					},
					SelectedMOQType = IES.Common.MOQType.Historical
				}
			};

			sut.CopyMoqTypes(moqTypesToCopy, 2, false, DateTime.Parse("12/20/2000"), DateTime.Now);

			moqTypeDataLoader.Verify(x => x.Save(It.Is<ICollection<MoqTypeSelection>>(u =>

				!u.First().TableData.First().PoPStartWeek.HasValue &&
				!u.First().TableData.First().PoPStartYear.HasValue &&
				!u.First().TableData.First().PoPEndWeek.HasValue &&
				!u.First().TableData.First().PoPEndYear.HasValue
			)), Times.Once());
		}

		/// <summary>
		/// Copy MOQ Types using new Fiscal Week scenario for Space
		/// </summary>
		[TestMethod]
		public void CopyMOQTypesNewFiscalWeek()
		{
			BOECopier sut = CreateSUT();
			Utilities.IsSAPEnabledForSystem = true;

			List<MoqTypeSelection> moqTypesToCopy = new List<MoqTypeSelection>
			{
				new MoqTypeSelection
				{
					TableData = new List<MoqTableData>
					{
						new MoqTableData
						{
							QueryType = MoqTableData.WEEKLY_DATETIME,
							PoPStart = DateTime.Now,
							PoPEnd = DateTime.Now
						}
					},
					SelectedMOQType = IES.Common.MOQType.Historical
				}
			};

			sut.CopyMoqTypes(moqTypesToCopy, 2, false, DateTime.Now, DateTime.Now);

			moqTypeDataLoader.Verify(x => x.Save(It.Is<ICollection<MoqTypeSelection>>(u =>

				!u.First().TableData.First().PoPStartWeek.HasValue &&
				!u.First().TableData.First().PoPStartYear.HasValue &&
				!u.First().TableData.First().PoPEndWeek.HasValue &&
				!u.First().TableData.First().PoPEndYear.HasValue
			)), Times.Once());
		}

		/// <summary>
		/// Copy MOQ Types using old Fiscal Week scenario for Space in same workspace
		/// </summary>
		[TestMethod]
		public void CopyMOQTypesOldFiscalWeekSameWorkspace()
		{
			BOECopier sut = CreateSUT();
			Utilities.IsSAPEnabledForSystem = true;

			List<MoqTypeSelection> moqTypesToCopy = new List<MoqTypeSelection>
			{
				new MoqTypeSelection
				{
					TableData = new List<MoqTableData>
					{
						new MoqTableData
						{
							QueryType = MoqTableData.WEEKLY_DATETIME,
							PoPStartWeek = 1,
							PoPStartYear = 2019,
							PoPEndWeek = 3,
							PoPEndYear = 2020
						}
					},
					SelectedMOQType = IES.Common.MOQType.Historical
				}
			};

			sut.CopyMoqTypes(moqTypesToCopy, 2, true, DateTime.Parse("12/20/2000"), DateTime.Now);

			moqTypeDataLoader.Verify(x => x.Save(It.Is<ICollection<MoqTypeSelection>>(u =>

				u.First().TableData.First().PoPStartWeek.HasValue &&
				u.First().TableData.First().PoPStartYear.HasValue &&
				u.First().TableData.First().PoPEndWeek.HasValue &&
				u.First().TableData.First().PoPEndYear.HasValue
			)), Times.Once());
		}
	}
}
