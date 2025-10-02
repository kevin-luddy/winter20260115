namespace GenBOE.Tests.DAL.DataLoaders
{
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.DTO.SkillMixSummary;
	using GenBOE.Dtos;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	[TestClass]
	public class SkillMixSummaryDTOLoaderTest
	{
		[ClassInitialize]
		public static void GetCurrentData(TestContext testContext)
		{
			GlobalTestCaseSetup.ResetGlobalWorkspaceID();
			GlobalTestCaseSetup.CreateBOE(GlobalTestCaseSetup.GlobalWorkspaceID);
			GlobalTestCaseSetup.CreateGlobalTaskElementID();
			GlobalTestCaseSetup.CreateGlobalMoqTypeSelectionId();
			GlobalTestCaseSetup.CreateSkillMixSummary();
		}

		[TestMethod]
		public void GetSkillMixDTOByMoqTypeSelection()
		{
			SkillMixSummaryDTOLoader sut = new SkillMixSummaryDTOLoader();
			ICollection<SkillMixSummaryDTO> skillMixSummaryDTOData = sut.GetByBOETaskElementID(GlobalTestCaseSetup.GlobalTaskElementID);
			Assert.AreEqual(1, skillMixSummaryDTOData.Count);
		}

		/// <summary>
		/// Remember: SkillMix is a Fill/Kill method and looks for MOQTypeSelectionID
		/// </summary>
		[TestMethod]
		public void UpdateSkillMixSummary()
		{
			SkillMixSummaryDTOLoader sut = new SkillMixSummaryDTOLoader();
			SkillMixSummaryDTO updateSkillMixSummary = new SkillMixSummaryDTO();
			updateSkillMixSummary.Rationale = "Updated Rationale";
			updateSkillMixSummary.Included = false;
			updateSkillMixSummary.ProposedHours = 100;
			updateSkillMixSummary.HistoricalHours = 100;
			updateSkillMixSummary.ResourceHours = 100;
			updateSkillMixSummary.BusinessResourceHours = 100;
			updateSkillMixSummary.BOESkillMix = 100;
			updateSkillMixSummary.LaborSkillMix = 100;
			updateSkillMixSummary.ResourceID = "Resource";
			updateSkillMixSummary.BusinessResourceID = "Business Resource";
			updateSkillMixSummary.BOEID = GlobalTestCaseSetup.GlobalBOEID;
			updateSkillMixSummary.BOETaskElementID = GlobalTestCaseSetup.GlobalTaskElementID;
			updateSkillMixSummary.IsUserInput = true;
			ICollection<SkillMixSummaryDTO> skillMixSummaries = new List<SkillMixSummaryDTO>() { updateSkillMixSummary };
			int? numRowsInserted = sut.InsertSkillMixSummary(skillMixSummaries);
			Assert.AreEqual(1, numRowsInserted);
		}

		/// <summary>
		/// Remember: SkillMix is deleted via MOQTypeSelectionID
		/// </summary>
		[TestMethod]
		public void DeleteSkillMixSummary()
		{
			SkillMixSummaryDTOLoader sut = new SkillMixSummaryDTOLoader();
			int? numRowsDeleted = sut.DeleteSkillMixSummaryByBOETaskElementID(GlobalTestCaseSetup.GlobalTaskElementID);
			Assert.AreEqual(1, numRowsDeleted);
		}
	}
}
