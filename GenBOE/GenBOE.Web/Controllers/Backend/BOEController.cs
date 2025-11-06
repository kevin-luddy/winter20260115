// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers.Backend
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Web.Http;
	using System.Web.Http.Cors;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic._ModelView.Backend;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.Exceptions;

	/// <summary>
	/// BOEController used for /boe/editboeindex/boe/
	/// </summary>
	[EnableCors("*", "*", "*", SupportsCredentials = true)]
	public class BOEController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Logger
		/// </summary>
		private readonly Logger logger = new Logger("BOEController");

		/// <summary>
		/// BOE Controller Logic
		/// </summary>
		private IBOEControllerLogic boeControllerLogic { get; set; }

		/// <summary>
		/// Task Element Validation
		/// </summary>
		private TaskElementValidation taskElementValidation { get; set; }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="securityAccess">Security Access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>
		/// <param name="homeControllerLogic">Home Controller Logic</param>
		public BOEController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader,
			IBOEControllerLogic boeControllerLogic, TaskElementValidation taskElementValidation)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.boeControllerLogic = boeControllerLogic;
			this.taskElementValidation = taskElementValidation;
		}

		/// <summary>
		/// Get BOE Headers
		/// </summary>
		/// <param name="workspaceShortname">Workspace Short Name</param>
		/// <param name="boeId">BOE ID</param>
		/// <returns>BOEHeaderModelView</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<BOEHeaderViewModel> GetBOEHeader(string workspaceShortname, int boeId)
		{
			IESSingleResponse<BOEHeaderViewModel> result = new IESSingleResponse<BOEHeaderViewModel>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);

			Stopwatch sw = InitializeAction(logger, WebConstants.GET_BOE_HEADER, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, boeId);

			FullBoe boe = this.Factory.CreateFullBoe(boeId);

			try
			{
				result.Data = boeControllerLogic.GetBOEHeaderViewModel(boe, ws);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			FinalizeAction(logger, WebConstants.GET_BOE_HEADER, sw);
			return result;
		}

		/// <summary>
		/// Get Task Element Grid
		/// </summary>
		/// <param name="workspaceShortname">Workspace shortname string</param>
		/// <param name="boeId">BOE Id</param>
		/// <returns></returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<GenericTaskElementGridModelView> GetTaskElementGrid(string workspaceShortname, int boeId)
		{
			IESSingleResponse<GenericTaskElementGridModelView> result = new IESSingleResponse<GenericTaskElementGridModelView>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);
			Stopwatch sw = InitializeAction(logger, WebConstants.GET_TASK_ELEMENT_GRID, SecurityPage.BOELaborGrid, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, boeId);

			try
			{
				FullBoe boe = this.Factory.CreateFullBoe(boeId);
				GenericTaskElementGridModelView theModelView = boeControllerLogic.GetTaskGridModelView(boe, ws);
				result.Data = theModelView;
				result.Data.TaskElements = theModelView.TaskElements.OrderBy(teOrder => teOrder.BOETaskElementOrder).ThenBy(teOrder => teOrder.TaskElementDetailID).ToCollection();

				ICollection<int> invalidTaskElementIds = this.taskElementValidation.GetInvalidTaskElementIds(ws, boe.TaskElements);
				result.Data.TaskElements
					.AsParallel()
					.Where(x => x.TaskElementDetailID.HasValue && invalidTaskElementIds.Contains(x.TaskElementDetailID.Value))
					.ForAll(z => z.FailedValidation = true);

				// ReadOnly check is for being able to delete a Task Element
				bool readOnly = true;
				// if the entire site is not readonly
				if (!SiteMasterUtilities.IsReadOnly())
				{
					// check the users permission
					readOnly = CheckPermission(SecurityPage.BOELaborGrid, ws, boeId) != SecurityAuthorization.CreateReadUpdateDelete;
				}
				result.Data.IsReadOnly = readOnly;

				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			FinalizeAction(logger, WebConstants.GET_TASK_ELEMENT_GRID, sw);
			return result;
		}

		/// <summary>
		/// Delete Labor Task Element from Grid
		/// </summary>
		/// <param name="deleteTaskElementModelView">Labor Task to be Deleted</param>
		/// <returns>Successful boolean check</returns>
		[HttpDelete]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<bool> DeleteTaskElement([FromBody] DeleteTaskElementModelView deleteTaskElementModelView)
		{
			_ = deleteTaskElementModelView ?? throw new ArgumentNullException(nameof(deleteTaskElementModelView));

			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(deleteTaskElementModelView.workspaceShortName);
			Stopwatch sw = InitializeAction(logger, WebConstants.DELETE_TASK_ELEMENT, SecurityPage.BOELaborGrid, SecurityAuthorization.CreateReadUpdateDelete, new List<WorkspaceDTO> { ws }, deleteTaskElementModelView.boeId);

			try
			{
				FullBoe boe = this.Factory.CreateFullBoe(deleteTaskElementModelView.boeId);
				boeControllerLogic.DeleteTaskElement(ws, boe, deleteTaskElementModelView.deletedTask);
				result.IsSuccessful = true;
				result.Data = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			FinalizeAction(logger, WebConstants.DELETE_TASK_ELEMENT, sw);
			return result;
		}

		/// <summary>
		/// Save the new order of the Labor Task Elements
		/// </summary>
		/// <param name="sortedTaskElementModelView">HTTP POST Body</param>
		/// <returns></returns>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<bool> SaveTaskElementOrder([FromBody] SortedTaskElementModelView sortedTaskElementModelView)
		{
			_ = sortedTaskElementModelView ?? throw new ArgumentNullException(nameof(sortedTaskElementModelView));

			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(sortedTaskElementModelView.workspaceShortName);
			Stopwatch sw = InitializeAction(logger, WebConstants.SAVE_SORTED_TASK_ELEMENTS, SecurityPage.BOELaborGrid, SecurityAuthorization.CreateReadUpdateDelete, new List<WorkspaceDTO> { ws }, sortedTaskElementModelView.boeId);

			try
			{
				FullBoe boe = this.Factory.CreateFullBoe(sortedTaskElementModelView.boeId);

				System.Collections.ObjectModel.Collection<TaskElementOrder> sortedTaskElements = new System.Collections.ObjectModel.Collection<TaskElementOrder>(sortedTaskElementModelView.sortedTaskElements
				.Select(x => new TaskElementOrder
				{
					TaskID = (int)x.TaskElementDetailID,
					ListOrder = x.BOETaskElementOrder
				}).ToList());
				TaskElementOrderCollection taskElementOrderCollection = new TaskElementOrderCollection()
				{
					BOETaskElements = sortedTaskElements
				};

				boeControllerLogic.ReOrderTaskElementOrder(ws, boe, taskElementOrderCollection);
				result.IsSuccessful = true;
				result.Data = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			FinalizeAction(logger, WebConstants.SAVE_SORTED_TASK_ELEMENTS, sw);
			return result;
		}

		/// <summary>
		/// Get BOE Headers Description
		/// </summary>
		/// <param name="workspaceShortname">Workspace Short Name</param>
		/// <param name="boeId">BOE ID</param>
		/// <returns>BOEHeaderDescriptionModelView</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<BOEHeaderDescriptionModelView> GetBOEHeaderDescription(string workspaceShortname, int boeId)
		{
			IESSingleResponse<BOEHeaderDescriptionModelView> result = new IESSingleResponse<BOEHeaderDescriptionModelView>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_DISPLAY_BOE_HEADER_DESCRIPTION, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, boeId);

			try
			{
				FullBoe boe = this.Factory.CreateFullBoe(boeId);
				result.Data = boeControllerLogic.GetBOEHeaderDescriptionMv(boe, ws);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			FinalizeAction(logger, WebConstants.GET_BOE_HEADER, sw);
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
		public IESSingleResponse<bool> SaveEditBOEHeader([FromBody] SaveBoeHeaderModelView saveBOEHeader)
		{
			_ = saveBOEHeader ?? throw new ArgumentNullException(nameof(saveBOEHeader));

			IESSingleResponse<bool> result = new IESSingleResponse<bool>();
			bool descriptionOnly = false;

			FullWorkspace ws = this.Factory.CreateFullWorkspace(saveBOEHeader.workspaceShortName);
			Stopwatch sw = InitializeAction(logger, "SaveEditBOEHeader", SecurityPage.BOELaborGrid, SecurityAuthorization.CreateReadUpdateDelete, new List<WorkspaceDTO> { ws }, saveBOEHeader.boeHeader.BOEID);

			try
			{
				FullBoe boe = this.Factory.CreateFullBoe(saveBOEHeader.boeHeader.BOEID);

				IBOEHeaderModelView boeHeader = new BOEHeaderModelView();
				boeHeader.BOEID = saveBOEHeader.boeHeader.BOEID;
				boeHeader.Title = saveBOEHeader.boeHeader.Title;
				boeHeader.CustomFieldValues = new Collection<CustomFieldSelectionModelView>();


				foreach (BOECustomFieldModelView item in saveBOEHeader.boeHeader.CustomFieldValues)
				{
					CustomFieldSelectionModelView field = new CustomFieldSelectionModelView();

					if (item.CustomFieldMetaData.CustomFieldValueID != 0 && item.CustomFieldMetaData.SelectionID != 0)
					{
						BOECustomFieldOptionModelView selectedOption = item.CustomFieldOptions.First(option => option.CustomFieldOptionID == item.CustomFieldMetaData.CustomFieldValueID);
						field.CustomFieldID = item.CustomFieldMetaData.isOpenEnded ? item.CustomFieldMetaData.CustomFieldID : -1;
						field.CustomFieldValueID = item.CustomFieldMetaData.CustomFieldValueID;
						field.IsOpenEnded = item.CustomFieldMetaData.isOpenEnded;
						field.OpenEndedValue = selectedOption.Description;
						field.SelectionID = item.CustomFieldMetaData.SelectionID;
						field.UpdateDate = item.UpdateDate;
						field.UpdateDateLong = item.UpdateDateLong;

						boeHeader.CustomFieldValues.Add(field);
					}
				}

				boeControllerLogic.SaveEditBoeHeader(ws, boe, boeHeader, saveBOEHeader.boeHeader.Description, descriptionOnly);

				result.IsSuccessful = true;
				result.Data = true;
			}
			catch (GenValidationException ex)
			{
				logger.Error(ex);
				result.Messages = ex.ValidationList.Select(x => x.ValidationIssue).ToList();
			}

			FinalizeAction(logger, "SaveEditBOEHeader", sw);
			return result;
		}
	}
}