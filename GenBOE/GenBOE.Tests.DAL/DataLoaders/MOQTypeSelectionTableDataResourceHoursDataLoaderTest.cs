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
			GlobalTestCaseSetup.CreateSkillMix();
		}

		[TestMethod]
		public void GetMOQTypeSelectionTableDataResourceHoursDTOByMoqTypeSelection()
		{
			SkillMixDTOLoader sut = new SkillMixDTOLoader();
			ICollection<SkillMixDTO> skillMixDTOData = sut.GetByMOQTypeSelectionID(GlobalTestCaseSetup.GlobalMoqTypeSelectionId);
			Assert.AreEqual(1, skillMixDTOData.Count);
		}

		/// <summary>
		/// Remember: SkillMix is a Fill/Kill method and looks for MOQTypeSelectionID
		/// </summary>
		[TestMethod]
		public void UpdateSkillMix()
		{
			SkillMixDTOLoader sut = new SkillMixDTOLoader();

			SkillMixDTO updateSkillMix = new SkillMixDTO();
			updateSkillMix.Rationale = "Updated Rationale";
			updateSkillMix.Included = false;
			updateSkillMix.ProposedHours = 100;
			updateSkillMix.HistoricalHours = 100;
			updateSkillMix.BOESkillMix = 100;
			updateSkillMix.LaborSkillMix = 100;
			updateSkillMix.ResourceOld = "Old Resource";
			updateSkillMix.ResourceNew = "New Resource";
			updateSkillMix.BOEID = GlobalTestCaseSetup.GlobalBOEID;
			updateSkillMix.BOETaskElementID = GlobalTestCaseSetup.GlobalTaskElementID;
			updateSkillMix.MOQTypeSelectionID = GlobalTestCaseSetup.GlobalMoqTypeSelectionId;

			ICollection<SkillMixDTO> skillMixes = new List<SkillMixDTO>() { updateSkillMix };
			int? numRowsInserted = sut.InsertSkillMix(skillMixes);
			Assert.AreEqual(1, numRowsInserted);
		}

		/// <summary>
		/// Remember: SkillMix is deleted via MOQTypeSelectionID
		/// </summary>
		[TestMethod]
		public void DeleteSkillMix()
		{
			SkillMixDTOLoader sut = new SkillMixDTOLoader();
			int? numRowsDeleted = sut.DeleteSkillMixByMoqTypeSelection(GlobalTestCaseSetup.GlobalMoqTypeSelectionId);
			Assert.AreEqual(1, numRowsDeleted);
		}
	}
}
