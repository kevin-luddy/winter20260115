namespace GenBOE.Tests.DAL.DataLoaders
{
	using System;
	using System.Collections.ObjectModel;
	using GenBOE.DataBridge.DTO;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System.Transactions;
	using GenBOE.Dtos;
	using IES.Common;
	using System.Linq;
	using System.Collections.Generic;

	[TestClass]
	public class MOQTypeSelectionTableDataResourceHoursDataLoaderTest
	{
		[ClassInitialize]
		public static void GetCurrentData(TestContext testContext)
		{
			GlobalTestCaseSetup.ResetGlobalWorkspaceID();
			GlobalTestCaseSetup.CreateBOE(GlobalTestCaseSetup.GlobalWorkspaceID);
			GlobalTestCaseSetup.CreateGlobalTaskElementID();
			GlobalTestCaseSetup.CreateGlobalMoqTypeSelectionId();
			GlobalTestCaseSetup.CreateGlobalMoqTypeSelectionTableDataId();
			GlobalTestCaseSetup.CreateMOQTypeSelectionTableDataResourceHours();
		}

		[TestMethod]
		public void GetMOQTypeSelectionTableDataResourceHoursDTOByMoqTypeSelection()
		{
			MOQTypeSelectionTableDataResourceHoursDTOLoader sut = new MOQTypeSelectionTableDataResourceHoursDTOLoader();
			ICollection<MOQTypeSelectionTableDataResourceHoursDTO> moqTypSelectionTableResourceHours = sut.GetByMOQTypeSelectionTableDataId(GlobalTestCaseSetup.GlobalMoqTypeSelectionTableDataId);
			Assert.AreEqual(1, moqTypSelectionTableResourceHours.Count);
		}

		/// <summary>
		/// Remember: MOQTypeSelectionTableDataResourceHours is a Fill/Kill method and looks for MOQTypeSelectionID
		/// </summary>
		[TestMethod]
		public void UpdateMOQTypeSelectionTableDataResourceHoursDTOByMoqTypeSelectionByTableDataId()
		{
			MOQTypeSelectionTableDataResourceHoursDTOLoader sut = new MOQTypeSelectionTableDataResourceHoursDTOLoader();

			MOQTypeSelectionTableDataResourceHoursDTO updateMOQTypeSelectionTableDataResourceHours = new MOQTypeSelectionTableDataResourceHoursDTO();
			updateMOQTypeSelectionTableDataResourceHours.ResourceName = "Updated Rationale";
			updateMOQTypeSelectionTableDataResourceHours.BRCName = "Updated Rationale BRC";
			updateMOQTypeSelectionTableDataResourceHours.WbsHours = 100;
			updateMOQTypeSelectionTableDataResourceHours.TotalHours = 100;
			updateMOQTypeSelectionTableDataResourceHours.MOQTypeSelectionTableDataId = 100;
			updateMOQTypeSelectionTableDataResourceHours.BOETaskElementID = 100;
			updateMOQTypeSelectionTableDataResourceHours.BOEID = 100;

			ICollection<MOQTypeSelectionTableDataResourceHoursDTO> moqTypeSelectionTableDataResourceHours = new List<MOQTypeSelectionTableDataResourceHoursDTO>() { updateMOQTypeSelectionTableDataResourceHours };
			int? numRowsInserted = sut.InsertMOQTypeSelectionTableDataResourceHours(moqTypeSelectionTableDataResourceHours);
			Assert.AreEqual(1, numRowsInserted);
		}

		/// <summary>
		/// Remember: MOQTypeSelectionTableDataResourceHours is deleted via MOQTypeSelectionTableDataId
		/// </summary>
		[TestMethod]
		public void DeleteMOQTypeSelectionTableDataResourceHours()
		{
			MOQTypeSelectionTableDataResourceHoursDTOLoader sut = new MOQTypeSelectionTableDataResourceHoursDTOLoader();
			int? numRowsDeleted = sut.DeleteByMoqTypeSelectionTableDataId(GlobalTestCaseSetup.GlobalMoqTypeSelectionTableDataId);
			Assert.AreEqual(1, numRowsDeleted);
		}
	}
}
