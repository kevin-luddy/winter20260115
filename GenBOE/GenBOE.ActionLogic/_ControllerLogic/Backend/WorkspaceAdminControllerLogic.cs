// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic.ControllerLogic.Backend
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.WBS;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.PickList;
	using IES.Common.Exceptions;

	/// <summary>
	/// Workspace Admin Controller Logic
	/// </summary>
	public class WorkspaceAdminControllerLogic
	{
		/// <summary>
		/// Common Data Loader
		/// </summary>
		private ICommonDataLoader commonDataLoader;

        /// <summary>
        /// wbs Loader
        /// </summary>
        private IWbsDTODataLoader wbsLoader;

        /// <summary>
        /// nested wbs utilities
        /// </summary>
		private NestedWBSUtilities _nestedWbsUtilities = null;

		public WorkspaceAdminControllerLogic(ICommonDataLoader commonDataLoader, NestedWBSUtilities nestedWbsUtilities, IWbsDTODataLoader wbsLoader)
		{
			this.commonDataLoader = commonDataLoader;
			_nestedWbsUtilities = nestedWbsUtilities;
			this.wbsLoader = wbsLoader;
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

		/// <summary>
		/// Get the WBS Grid MV
		/// </summary>
		/// <param name="workspace">the workspace</param>
		/// <returns>The MV for the Manage WBS grid</returns>
		public ICollection<ManageWBSModelView> GetManageWBSModel(FullWorkspace workspace)
		{
			if(workspace == null)
			{
				throw new GeneralAppException("workspace cannot be null");
			}

			// convert the collection of WBS DTO to a collection of ManageWBSModelView
			Collection<WbsDTO> wbsDTOs = workspace.WbsElementsNoMultiWbs.ToCollection<WbsDTO>();

			Collection<ManageWBSModelView> result = new Collection<ManageWBSModelView>();

			_nestedWbsUtilities.AdjustLevels(wbsDTOs);

			Collection<int> wbsids = GetWBSUsedByWorkspaceVariables(workspace);
			ICollection<int> wbsInBoes = GetWbsUsedInBoes(workspace);

			foreach (WbsDTO wbs in wbsDTOs)
			{
				Collection<ClinDTO> clins = workspace.Clins.Where(x => wbs.ClinIDs.Contains(x.Id)).ToCollection<ClinDTO>();

				ManageWBSModelView wbsModelView = new ManageWBSModelView(wbs, clins);

				if (wbsids.Contains(wbs.Id))
				{
					wbsModelView.InUse = true;
				}

				if (wbsInBoes.Contains(wbs.Id))
				{
					wbsModelView.HasBOE = true;
				}

				wbsModelView.ParentOrChildHasBoe = this.ParentOrChildHasBoe(wbs, workspace);

				result.Add(wbsModelView);
			}

			return result;
        }

        /// <summary>
        /// Get IDs of WBSs that are used by workspace variables
        /// </summary>
        /// <param name="ws">The Workspace</param>
        /// <returns>The IDs of WBSs used by workspace variables</returns>
        private Collection<int> GetWBSUsedByWorkspaceVariables(FullWorkspace workspace)
		{
			IReadOnlyCollection<WorkspaceVariableDTO> workspaceVariables = workspace.WorkspaceVariables;
			Collection<int> wbsids = new Collection<int>();

			foreach (WorkspaceVariableDTO workspaceVariable in workspaceVariables)
			{
				foreach (SelectBOEsToSum SelectedBOEsToSum in workspaceVariable.SelectedBOEsToSum)
				{
					if (SelectedBOEsToSum.WBSID != null && !wbsids.Contains(SelectedBOEsToSum.WBSID.Value))
					{
						wbsids.Add(SelectedBOEsToSum.WBSID.Value);
					}
				}
			}

			return wbsids;
		}

		/// <summary>
		/// Get the WBSs that are used by BOEs
		/// </summary>
		/// <param name="ws">The Workspace</param>
		/// <returns>The IDs of WBSs used by BOEs</returns>
		private ICollection<int> GetWbsUsedInBoes(FullWorkspace ws)
		{
			ICollection<int> toReturn = new Collection<int>();
			foreach (FullBoe boe in ws.Boes)
			{
				if (boe.WBSID != null)
				{
					toReturn.Add((int)boe.WBSID);
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Check if a parent WBS has a BOE
		/// </summary>
		/// <param name="wbs">wbs to check</param>
		/// <param name="ws">ws containing wbs</param>
		/// <returns>True if parent WBS has a BOE, otherwise false</returns>
		private bool ParentOrChildHasBoe(WbsDTO wbs, FullWorkspace ws)
		{
			ICollection<string> parentWbs = _nestedWbsUtilities.GetParentsWBSNumByChildWBS(wbs);
			foreach (string parentNumber in parentWbs)
			{
				WbsDTO parentDto = ws.WbsElements.FirstOrDefault(x => x.WbsNumber == parentNumber);
				if (parentDto != null && parentDto.inUse)
				{
					return true;
				}
			}

			ICollection<WbsDTO> childWbs = wbsLoader.GetAllChildWbs(ws.Id, wbs.WbsNumber);
			foreach (WbsDTO childDto in childWbs)
			{
				if (childDto.inUse)
				{
					return true;
				}
			}

			return false;
		}
	}
}
