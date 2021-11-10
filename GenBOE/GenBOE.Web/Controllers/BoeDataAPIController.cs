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

        private IBOEExporter boeExporter;

        private IBOECustomExporter boeCustomExporter;

        private IWorkspaceExportFormatDTODataLoader workspaceExportFormatDTOLoader;

        /// <summary>
        /// Logger
        /// </summary>
        private Logger logger = new Logger("BoeDataAPIController");

        /// <summary>
        /// Ctor
        /// </summary>
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HttpGet]
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
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HttpGet]
        public IHttpActionResult ExportAllBOEs(string workspaceId)
        {
            try
            {
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				HttpResponse response = HttpContext.Current.Response;
                FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceId);

				var permission = this.CheckPermission(SecurityPage.Reports, workspace);

				if (permission < SecurityAuthorization.Read)
				{
					return Unauthorized();
				}

				this.reportsControllerLogic.PrepareAllBOEsReport(workspace, this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id).Any(p => p.Role == Role.SubcontractorAuthor && p.ETIUserId == workspace.CurrentActiveUser.UserID), null, null, null, out bool isCustomExport,
					out WorkspaceExportFormatDTO wsExportFormatDTO, out BOEExportInputs exportInputs, out ICollection<BOEExportModelView> boeExportModelViews, out List<BOESummaryGridModelView> boeSummaryGridModelViews, false);

                string fileName = string.Format("genBOECustomExport-{0}.docx", workspace.WorkspaceName).Replace(",", string.Empty);

                response.ContentType = BOEExporterConstants.ContentType_DOCX;
                response.Clear();
                response.BufferOutput = true;
                response.AppendHeader(BOEExporterConstants.CONTENT_HEADER_NAME, string.Format(BOEExporterConstants.CONTENT_HEADER_FORMAT_STRING, fileName));

                if (isCustomExport)
				{
                    WorkspaceExportFormatDTO exportFormat = wsExportFormatDTO.ExportFormat.ParentTemplateId < 9001 || wsExportFormatDTO.ExportFormat.ParentTemplateId > 10000 || wsExportFormatDTO.ExportFormat.ParentTemplateId == null ? this.workspaceExportFormatDTOLoader.GetById((int)ExcelReportTemplateType.MASTER) : wsExportFormatDTO;
                    boeCustomExporter.ExportBOEToWordFileStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, workspace, null, response.OutputStream, exportFormat);
                }
				else
				{
                    boeExporter.ExportBOEToWordFileStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, workspace, fileName, response.OutputStream, wsExportFormatDTO.ExportFormat.TemplateType);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Is Service Alive?
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HttpGet]
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