// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Http;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using IES.Common;

	/// <summary>
	/// BOE Data Controller, original intent is for it to be used by ACV to pull data in, but realistically, it is serving up BOE data, hence the name.
	/// </summary>
	[AllowAnonymous]
	public class BoeDataAPIController : BoeDataBaseAPIController
	{
		#region Properties & Ctor

		/// <summary>
		/// Proposal Loader
		/// </summary>
		private IWorkspaceDTODataLoader loader;

		/// <summary>
		/// Token Handling
		/// </summary>
		private TokenHandling tokenHandler;

		/// <summary>
		/// Reports controller
		/// </summary>
		private IReportsControllerLogic reportsControllerLogic;

		/// <summary>
		/// BOE Exporter
		/// </summary>
		private IBOEExporter boeExporter;

		/// <summary>
		/// BOE custom exporter
		/// </summary>
		private IBOECustomExporter boeCustomExporter;

		/// <summary>
		/// Workspace Export fromat loader
		/// </summary>
		private IWorkspaceExportFormatDTODataLoader workspaceExportFormatDTOLoader;

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("BoeDataAPIController");

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="loader">Workspace loader</param>
		/// <param name="tokenHandler">Token handler</param>
		/// <param name="reportsControllerLogic">Reports controller logic</param>
		/// <param name="securityAccess">Security Access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>
		/// <param name="boeExporter">BOE exporter</param>
		/// <param name="boeCustomExporter">BOE custom exporter</param>
		/// <param name="workspaceExportFormatDTOLoader">Workspace export format loader</param>
		public BoeDataAPIController(IWorkspaceDTODataLoader loader, TokenHandling tokenHandler, IReportsControllerLogic reportsControllerLogic, ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, IBOEExporter boeExporter, IBOECustomExporter boeCustomExporter, IWorkspaceExportFormatDTODataLoader workspaceExportFormatDTOLoader) 
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.loader = loader;
			this.tokenHandler = tokenHandler;
			this.reportsControllerLogic = reportsControllerLogic;
			this.boeExporter = boeExporter;
			this.boeCustomExporter = boeCustomExporter;
			this.workspaceExportFormatDTOLoader = workspaceExportFormatDTOLoader;
		}
		#endregion

		/// <summary>
		/// Get Proposal data for ACV. Limits the number of records returned to 100.
		/// </summary>
		/// <param name="ptmTrackingNumber">PTM Tracking Number</param>
		/// <returns>Proposal Data</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public ICollection<AcvWorkspaceData> GetWorkspaceDataForProposal(string ptmTrackingNumber)
		{
			List<AcvWorkspaceData> result;

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				ICollection<(int Id, string shortName, string longName)> data = this.loader.GetWorkspaceDataForProposal(ptmTrackingNumber);
				result = data.Select(x => new AcvWorkspaceData() { Id = x.Id, ShortName = x.shortName, LongName = x.longName }).ToList();
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result = null;
			}

			return result;
		}

		/// <summary>
		/// Export all BOEs given workspace 
		/// </summary>
		/// <param name="workspaceId">Id of workspace</param>
		/// <returns>HttpResponseMessage</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public HttpResponseMessage ExportAllBOEs(string workspaceId)
		{
			try
			{

				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceId);

				SecurityAuthorization permission = this.CheckPermission(SecurityPage.Reports, workspace);

				if (permission < SecurityAuthorization.Read)
				{
					new HttpResponseMessage(HttpStatusCode.Unauthorized);
				}

				HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

				string fileName = string.Format("genBOE-Export-{0}.docx", workspace.WorkspaceName).Replace(",", string.Empty);

				MemoryStream stream = new MemoryStream();

				this.reportsControllerLogic.PrepareAllBOEsReport(workspace, this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id).Any(p => p.Role == Role.SubcontractorAuthor && p.ETIUserId == workspace.CurrentActiveUser.UserID), null, null, null, out bool isCustomExport, out WorkspaceExportFormatDTO wsExportFormatDTO, out BOEExportInputs exportInputs, out ICollection<BOEExportModelView> boeExportModelViews, out List<BOESummaryGridModelView> boeSummaryGridModelViews, false);

				if (isCustomExport)
				{
					WorkspaceExportFormatDTO exportFormat = wsExportFormatDTO.ExportFormat.ParentTemplateId < 9001 || wsExportFormatDTO.ExportFormat.ParentTemplateId > 10000 || wsExportFormatDTO.ExportFormat.ParentTemplateId == null ? this.workspaceExportFormatDTOLoader.GetById((int)ExcelReportTemplateType.MASTER) : wsExportFormatDTO;
					boeCustomExporter.ExportBOEToWordFileStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, workspace, null, stream, exportFormat);
				}
				else
				{
					boeExporter.ExportBOEToWordFileStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, workspace, fileName, stream, wsExportFormatDTO.ExportFormat.TemplateType);
				}

				stream.Position = 0; //We need to set this to return the file

				response.Content = new StreamContent(stream);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue(BOEExporterConstants.ContentType_DOCX);
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
				{
					FileName = fileName
				};

				return response;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}
		}

		/// <summary>
		/// Is Service Alive?
		/// </summary>
		/// <returns>True/false</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public bool IsAlive()
		{
			bool result;

			try
			{
				// ToDo: add a DB grab, just to see if the DB is working.. To make the check more meaningful
				result = true;
			}
			catch
			{
				result = false;
			}

			return result;
		}
	}
}