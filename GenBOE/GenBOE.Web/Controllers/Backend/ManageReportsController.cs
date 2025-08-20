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
	using System.Linq;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Web.Http;
	using GenBOE.ActionLogic._ModelView.Backend;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IESSAPClient;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.ActionLogic.Reporting;
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
		/// BOE Status Report
		/// </summary>
		private IBOEStatusReport boeStatusReport { get; set; }


		/// <summary>
		/// ctor
		/// </summary>
		public ManageReportsController(ISecurityAccess inSecurityAccess, IReportsControllerLogic reportsControllerLogic,
			IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, IBOEStatusReport boeStatusReport)
			: base(inSecurityAccess, factory, userLoader, permissionsLoader)
		{
			this.reportsControllerLogic = reportsControllerLogic;
			this.boeStatusReport = boeStatusReport;
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

			return result;
		}

		/// <summary>
		/// Get the BOE Status Reports Data for Reports Page
		/// </summary>
		/// <param name="workspace">workspace short name</param>
		/// <returns>BOE Status Reports Data</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<BOEStatusReportView> GetBOEStatusReport(string workspace)
		{
			IESSingleResponse<BOEStatusReportView> result = new IESSingleResponse<BOEStatusReportView>();
			BOEStatusReportView data = new BOEStatusReportView();

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
				Stopwatch sw = this.InitializeAction(this.logger, WebConstants.ACTION_DISPLAY_BOE_STATUS_REPORT, SecurityPage.Reports, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);

				// UCOT check (only need this because the totals are calculated
				// but the variable for IsUCOTEnabledForWorkspace within the Workspace is not set properly.
				// Force check here and pass into the Views
				data.isUCOTEnabledForWorkspace = Utilities.ShowUCOTForWorkspace(ws.CreationDate, ws.Shortname);

				// Call the BL to generate the status report
				// All BOEs for the workspace as a default
				List<FullBoe> boes = ws.Boes.ToList();
				List<BoeTaskElementDTO> tasks = ws.TaskElements.ToList();

				bool isOffloading = ws.ProjectMapType != ProjectMapType.StandardWithoutOffload;
				if (isOffloading)
				{
					OffloadLaborRates offloader = new OffloadLaborRates();
					List<int> selectedBoeIds = boes.Select(b => b.Id).ToList();
					OffloadLaborRatesResults results = offloader.OffloadWorkspace(boes.Where(b => selectedBoeIds.Contains(b.Id)).ToList(), ws);

					boes = results.Boes.ToList();
					tasks = boes.SelectMany(b => b.TaskElements).ToList();
				}

				BOEExportInputs exportInputs = new BOEExportInputs(boes, boes, tasks, ws);
				Collection<BOEStatusReportModelView> reportData = boeStatusReport.GenerateBOEStatusReport(exportInputs);
				ICollection<BOEStatusReportGrid> reportGrid = boeStatusReport.ConvertBOEStatusData(reportData);
				data.boeStatusReportModel = reportGrid;

				result.Data = data;

				FinalizeAction(logger, WebConstants.ACTION_DISPLAY_BOE_STATUS_REPORT, sw);
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

			try
			{
				Stopwatch sw = this.InitializeAction(this.logger, WebConstants.ACTION_DISPLAY_BOE_DISCREPANCY, SecurityPage.Reports, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);
				ICollection<BoeDiscrepancyReportModelView> theModelViews = reportsControllerLogic.GenerateDataForBoeDiscrepancyReport(ws, true);
				result.Data = theModelViews;
				result.IsSuccessful = true;

				FinalizeAction(logger, WebConstants.ACTION_DISPLAY_BOE_DISCREPANCY, sw);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

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
		public IESSingleResponse<string> GetReportHoursLabel(string workspace)
		{
			IESSingleResponse<string> result = new IESSingleResponse<string>();

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
				string hoursLabel = FullObjectHelper.HoursLabel(ws);
				result.Data = hoursLabel;
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