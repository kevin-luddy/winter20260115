// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.Controllers.Backend
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Http;
	using GenBOE.ActionLogic._ControllerLogic.Backend;
	using GenBOE.ActionLogic.ModelView.Workspace;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.PickList;

	/// <summary>
	/// Workspace Settings Controller
	/// </summary>
	public class WorkspaceSettingsController : BoeDataBaseAPIController
	{
		#region Properties & Ctor

		/// <summary>
		/// Service for Workspace Controller
		/// </summary>
		private WorkspaceSettingsControllerLogic workspaceSettingsControllerLogic { get; set; }

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("WorkspaceSettingsController");

		/// <summary>
		/// ctor
		/// </summary>
		public WorkspaceSettingsController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, 
			WorkspaceSettingsControllerLogic workspaceSettingsControllerLogic)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.workspaceSettingsControllerLogic = workspaceSettingsControllerLogic;
		}
		#endregion

		/// <summary>
		/// Get the Workspace Identification for a specific Workspace
		/// </summary>
		/// <param name="workspaceShortName">workspace short name</param>
		/// <returns>Workspace Identification</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<IWorkspaceIdentificationModelView> GetWorkspaceIdentification(string workspaceShortName)
		{
			IESSingleResponse<IWorkspaceIdentificationModelView> result = new IESSingleResponse<IWorkspaceIdentificationModelView>();

			try
			{
				FullWorkspace ws = Factory.CreateFullWorkspace(workspaceShortName);
				IWorkspaceIdentificationModelView workspaceIdentificationModelView = workspaceSettingsControllerLogic.GetWorkspaceIdentification(ws);
				result.Data = workspaceIdentificationModelView;
				result.IsSuccessful = true;

			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unkown error returning Workspace Identification: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get Pick List Values
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<PickListDto> GetLineOfBusinessType()
		{
			IESResponse<PickListDto> result = new IESResponse<PickListDto>();

			try
			{
				result.Data = workspaceSettingsControllerLogic.GetLineOfBusinessType();
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occured returning Line of Business data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get Proposal Class Options
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<PickListDto> GetProposalClassOptionList()
		{
			IESResponse<PickListDto> result = new IESResponse<PickListDto>();

			try
			{
				ICollection<PickListDto> proposalClassTypes = workspaceSettingsControllerLogic.GetProposalClassOptionList();
				result.Data = proposalClassTypes.Where(p => p.Id != Constants.PROPOSAL_CLASS_TYPE_NOT_SET).ToList();
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occured returning Line of Business data: {ex.Message}");
			}

			return result;
		}

		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<PickListDto> GetContractTypeOptionList()
		{
			IESResponse<PickListDto> result = new IESResponse<PickListDto>();

			try
			{
				ICollection<PickListDto> contractTypes = workspaceSettingsControllerLogic.GetContractTypeOptionList();
				result.Data = contractTypes;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occured returning list of Contract Types: {ex.Message}");
			}

			return result;
		}
	}
}