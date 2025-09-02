// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.IO;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Web.Http;
	using GenBOE.ActionLogic._ModelView.Backend;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.ActionLogic.Reporting;
	using GenBOE.ActionLogic.WBS.BOE;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using IES.Common;

	/// <summary>
	/// Manage Permissions Controller for getting workspace home data.
	/// </summary>
	public class ManageReportsController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Logger
		/// </summary>
		private readonly Logger logger = new Logger("ReportsController");

		/// <summary>
		/// Business logic for the reports controller
		/// </summary>
		private IReportsControllerLogic reportsControllerLogic { get; set; }

		/// <summary>
		/// Validate BOE Controller Logic
		/// </summary>
		private IValidateBOE validateBOE { get; set; }

		/// <summary>
		/// ctor
		/// </summary>
		public ManageReportsController(ISecurityAccess inSecurityAccess, IReportsControllerLogic reportsControllerLogic, IValidateBOE validateBOE,
			IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader)
			: base(inSecurityAccess, factory, userLoader, permissionsLoader)
		{
			this.reportsControllerLogic = reportsControllerLogic;
			this.validateBOE = validateBOE;
		}

		/// <summary>
		/// Get Exports data for report page
		/// </summary>
		/// <param name="workspace">workspace short name</param>
		/// <returns>Exports Data</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<ExportReportViewModel> GetExports(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			IESSingleResponse<ExportReportViewModel> result = new IESSingleResponse<ExportReportViewModel>();

			// Start Stopwatch to measure performance
			Stopwatch sw = InitializeAction(logger, WebConstants.GET_EXPORTS, SecurityPage.Reports, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, null);

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

			// Finalize Action
			FinalizeAction(logger, WebConstants.GET_EXPORTS, sw);

			return result;
		}

		/// <summary>
		/// Get General reports data for report page
		/// </summary>
		/// <param name="workspace">workspace short name</param>
		/// <returns>General Reports Data</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESResponse<GeneralReportViewModel> GetGeneralReports(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			IESResponse<GeneralReportViewModel> result = new IESResponse<GeneralReportViewModel>();

			// Start Stopwatch to measure performance
			Stopwatch sw = InitializeAction(logger, WebConstants.GET_GENERAL_REPORT, SecurityPage.Reports, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, null);

			try
			{
				ICollection<GeneralReportViewModel> theModelViews = reportsControllerLogic.GetDisplayGeneralReports(ws);
				result.Data = theModelViews;
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			// Finalize Action
			FinalizeAction(logger, WebConstants.GET_GENERAL_REPORT, sw);

			return result;
		}

		/// <summary>
		/// Get the BOE Discrepancy Reports Data for Reports Page
		/// </summary>
		/// <param name="workspace">Workspace Short Name</param>
		/// <returns>BOE Discrepancy Report Model View</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESResponse<BoeDiscrepancyReportModelView> GetBOEDiscrepancyReport(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			IESResponse<BoeDiscrepancyReportModelView> result = new IESResponse<BoeDiscrepancyReportModelView>();

			// Start Stopwatch to measure performance
			Stopwatch sw = InitializeAction(logger, WebConstants.GET_BOE_DISCREPANCY_REPORT, SecurityPage.Reports, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, null);

			try
			{
				ICollection<BoeDiscrepancyReportModelView> theModelViews = reportsControllerLogic.GenerateDataForBoeDiscrepancyReport(ws, true);
				result.Data = theModelViews;
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			// Finalize Action
			FinalizeAction(logger, WebConstants.GET_BOE_DISCREPANCY_REPORT, sw);

			return result;
		}

		/// <summary>
		/// Get the Hours Label used in the BOE Discrepancy Report
		/// </summary>
		/// <param name="workspace">workspace short name</param>
		/// <returns>label string</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<string> GetBOEDiscrepancyReportHoursLabel(string workspace)
		{
			IESSingleResponse<string> result = new IESSingleResponse<string>();
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Start Stopwatch to measure performance
			Stopwatch sw = InitializeAction(logger, WebConstants.GET_BOE_DISCREPANCY_REPORT_HOURS_LABEL, SecurityPage.Reports, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, null);

			try
			{
				string hoursLabel = FullObjectHelper.HoursLabel(ws);
				result.Data = hoursLabel;
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			// Finalize Action
			FinalizeAction(logger, WebConstants.GET_BOE_DISCREPANCY_REPORT_HOURS_LABEL, sw);

			return result;
		}

		/// <summary>
		/// Get Validate All BOEs Report
		/// </summary>
		/// <param name="workspaceShortname">Workspace Shortname</param>
		/// <returns>Validate All BOEs Report as a ModelView</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<ValidationAllBOEModelView> GetValidateAllBOEsReport(string workspaceShortname)
		{
			IESSingleResponse<ValidationAllBOEModelView> result = new IESSingleResponse<ValidationAllBOEModelView>();
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);

			// Start Stopwatch to measure performance
			Stopwatch sw = InitializeAction(logger, WebConstants.GET_VALIDATE_ALL_BOES_REPORT, SecurityPage.Reports, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, null);

			try
			{
				result.Data = validateBOE.ValidateAllBOEs(ws);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			// Finalize Action
			FinalizeAction(logger, WebConstants.GET_VALIDATE_ALL_BOES_REPORT, sw);

			return result;
		}

		/// <summary>
		/// Export BOE Discrepancy Report
		/// </summary>
		/// <param name="workspace">workspace shortname</param>
		/// <returns>filestream</returns>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public HttpResponseMessage ExportBOEDiscrepancyReport([FromBody] ExportFileModelView exportBOEDiscrepancyReportModelView)
		{
			if (exportBOEDiscrepancyReportModelView == null)
			{
				throw new ArgumentNullException(nameof(exportBOEDiscrepancyReportModelView));
			}

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(exportBOEDiscrepancyReportModelView.workspaceShortName);
				Stopwatch sw = this.InitializeAction(this.logger, WebConstants.ACTION_EXPORT_BOE_DISCREPANCY, SecurityPage.Reports, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);

				ICollection<BoeDiscrepancyReportModelView> theModelViews = reportsControllerLogic.GenerateDataForBoeDiscrepancyReport(ws, true);
				MemoryStream ms = reportsControllerLogic.ExportBOEDiscrepancyReport(ws.Shortname, theModelViews);

				HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
				response.Content = new StreamContent(ms);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue(BOEExporterConstants.ContentType_XLSX);
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
				response.Content.Headers.ContentDisposition.FileName = "BOEDiscrepancyReport.xlsx";

				FinalizeAction(logger, WebConstants.ACTION_EXPORT_BOE_DISCREPANCY, sw);

				return response;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
				{
					Content = new StringContent("unknown error exporting BOE Discrepancy")
				};
			}
		}
	}
}