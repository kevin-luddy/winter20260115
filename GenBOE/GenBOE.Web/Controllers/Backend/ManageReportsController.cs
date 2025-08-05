// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System;
	using System.Linq;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using IES.Common;
	using GenBOE.Dtos;
	using GenBOE.DataBridge.Common;
	using System.Collections.ObjectModel;
	using System.Collections.Generic;
	using IES.Common.classes;
	using GenBOE.Web.Common;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic._ModelView.Backend;

	/// <summary>
	/// Manage Permissions Controller for getting workspace home data.
	/// </summary>
	public class ManageReportsController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Service for PermissionsController
		/// </summary>
		//private IGenBOEControllerLogic inControllerLogic { get; set; }

		/// <summary>
		/// Logger
		/// </summary>
		private readonly Logger logger = new Logger("ReportsController");

		/// <summary>
		/// Business logic for the reports controller
		/// </summary>
		private IReportsControllerLogic reportsControllerLogic { get; set; }


		/// <summary>
		/// ctor
		/// </summary>
		public ManageReportsController(ISecurityAccess inSecurityAccess, IReportsControllerLogic reportsControllerLogic,
			IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader)
			: base(inSecurityAccess, factory, userLoader, permissionsLoader)
		{
			//this.UserLoader = userLoader;
			this.reportsControllerLogic = reportsControllerLogic;
		}

		/// <summary>
		/// Display the Exports View
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		[System.Web.Http.HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<ExportReportViewModel> DisplayExports(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			IESSingleResponse<ExportReportViewModel> result = new IESSingleResponse<ExportReportViewModel>();

			try
			{
				ExportReportViewModel reportView = reportsControllerLogic.GetDisplayExports(ws);
				result.Data = reportView;
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			return result;
		}

		/// <summary>
		/// Display the General Reports View
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		[System.Web.Http.HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<Collection<GeneralReportViewModel>> DisplayGeneralReports(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			IESSingleResponse<Collection<GeneralReportViewModel>> result = new IESSingleResponse<Collection<GeneralReportViewModel>>();

			try
			{
				Collection<GeneralReportViewModel> theModelViews = reportsControllerLogic.GetDisplayGeneralReports(ws);
				result.Data = theModelViews;
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			return result;
		}
	}
}