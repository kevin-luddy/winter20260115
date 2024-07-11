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
	public class CommonDisclosureSMDTODataLoaderTest
	{
		[ClassInitialize]
		public static void GetCurrentData(TestContext testContext)
		{
			GlobalTestCaseSetup.ResetGlobalWorkspaceID();
			GlobalTestCaseSetup.CreateBOE(GlobalTestCaseSetup.GlobalWorkspaceID);
			GlobalTestCaseSetup.CreateGlobalTaskElementID();
			GlobalTestCaseSetup.CreateGlobalMoqTypeSelectionId();
			GlobalTestCaseSetup.CreateCommonDisclosureSkillMix();
		}
		[TestMethod]
		public void GetCommonDisclosureSMDTOByMoqTypeSelection()
		{
			CommonDisclosureSMDTODataLoader sut = new CommonDisclosureSMDTODataLoader();
			ICollection<CommonDisclosureSkillMixDTO> commonDisclosureSMDTOData = sut.GetByMOQTypeSelectionID(GlobalTestCaseSetup.GlobalMoqTypeSelectionId);
			Assert.AreEqual(1, commonDisclosureSMDTOData.Count);
		}
		/// <summary>
		/// Remember: SkillMix is a Fill/Kill method and looks for MOQTypeSelectionID
		/// </summary>
		[TestMethod]
		public void UpdateCommonDisclosureSkillMix()
		{
			CommonDisclosureSMDTODataLoader sut = new CommonDisclosureSMDTODataLoader();
			CommonDisclosureSkillMixDTO updateCommonDisclosureSkillMix = new CommonDisclosureSkillMixDTO();
			updateCommonDisclosureSkillMix.Rationale = "Updated Rationale";
			updateCommonDisclosureSkillMix.Included = false;
			updateCommonDisclosureSkillMix.ProposedHours = 100;
			updateCommonDisclosureSkillMix.HistoricalHours = 100;
			updateCommonDisclosureSkillMix.BOESkillMix = 100;
			updateCommonDisclosureSkillMix.LaborSkillMix = 100;
			updateCommonDisclosureSkillMix.ResourceID = "Resource";
			updateCommonDisclosureSkillMix.BusinessResourceID = "Business Resource";
			updateCommonDisclosureSkillMix.BOEID = GlobalTestCaseSetup.GlobalBOEID;
			updateCommonDisclosureSkillMix.BOETaskElementID = GlobalTestCaseSetup.GlobalTaskElementID;
			updateCommonDisclosureSkillMix.MOQTypeSelectionID = GlobalTestCaseSetup.GlobalMoqTypeSelectionId;
			ICollection<CommonDisclosureSkillMixDTO> commonDisclosureSkillMixes = new List<CommonDisclosureSkillMixDTO>() { updateCommonDisclosureSkillMix };
			int? numRowsInserted = sut.InsertCommonDisclosureSM(commonDisclosureSkillMixes);
			Assert.AreEqual(1, numRowsInserted);
		}
		/// <summary>
		/// Remember: SkillMix is deleted via MOQTypeSelectionID
		/// </summary>
		[TestMethod]
		public void DeleteCommonDisclosureSkillMix()
		{
			CommonDisclosureSMDTODataLoader sut = new CommonDisclosureSMDTODataLoader();
			int? numRowsDeleted = sut.DeleteCommonDisclosureSMByMoqTypeSelection(GlobalTestCaseSetup.GlobalMoqTypeSelectionId);
			Assert.AreEqual(1, numRowsDeleted);
		}
	}
}