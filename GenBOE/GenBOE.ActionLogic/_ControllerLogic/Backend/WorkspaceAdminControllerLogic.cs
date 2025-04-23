// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic.ControllerLogic.Backend
{
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.DataBridge.Common;
	using IES.Common;
	using IES.Common.PickList;

	/// <summary>
	/// Workspace Admin Controller Logic
	/// </summary>
	public class WorkspaceAdminControllerLogic
	{
		/// <summary>
		/// Common Data Loader
		/// </summary>
		private ICommonDataLoader commonDataLoader;

		public WorkspaceAdminControllerLogic(ICommonDataLoader commonDataLoader)
		{
			this.commonDataLoader = commonDataLoader;
		}

		/// <summary>
		/// Builds sorted SelectList with only Contract Types selected at the workspace level and default option
		/// </summary>
		/// <param name="workspaceId"></param>
		/// <returns>sorted SelectList with workspace Contract Types only</returns>
		public ICollection<PickListDto> BuildContractTypeDropdownOptions(int workspaceId)
		{
			//Contract Type dropdown includes workspace contract types only
			List<PickListDto> selectedContractTypesList = this.commonDataLoader.GetSelectedContractTypes(workspaceId)
				.OrderBy(ct => ct.Text)
				.ToList();
			//Add 'Not Set' option for CLINs without contract type
			selectedContractTypesList.Insert(0, new PickListDto { Id = Constants.CONTRACT_TYPE_NOT_SET, Text = Constants.CONTRACT_TYPE_NOT_SET_STRING });
			return selectedContractTypesList;
		}
	}
}
