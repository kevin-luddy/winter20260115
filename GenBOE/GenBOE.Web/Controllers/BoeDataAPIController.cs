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
	using System.Threading.Tasks;
	using System.Web.Http;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.BOE;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.Reference;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.PickList;
	using MoreLinq;


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
		private readonly IWorkspaceDTODataLoader loader;

		/// <summary>
		/// Token Handling
		/// </summary>
		private readonly TokenHandling tokenHandler;

		/// <summary>
		/// Reports controller
		/// </summary>
		private readonly IReportsControllerLogic reportsControllerLogic;

		/// <summary>
		/// BOE Form Controller Logic
		/// </summary>
		private readonly IBOEFormControllerLogic boeFormControllerLogic;

		/// <summary>
		/// BOE Exporter
		/// </summary>
		private readonly IBOEExporter boeExporter;

		/// <summary>
		/// BOE custom exporter
		/// </summary>
		private readonly IBOECustomExporter boeCustomExporter;

		/// <summary>
		/// Workspace Export format loader
		/// </summary>
		private readonly IWorkspaceExportFormatDTODataLoader workspaceExportFormatDTOLoader;

		/// <summary>
		/// Trace Table data exporter
		/// </summary>
		private readonly ITraceTableExporter traceTableExporter;

		/// <summary>
		/// BOE Form PBOE Data Loader
		/// </summary>
		private readonly IBOEFormPBOEDTODataLoader boeFormPBOEDTODataLoader;

		/// <summary>
		/// BOE Form IBOE Data Loader
		/// </summary>
		private readonly IBOEFormIBOEDTODataLoader boeFormIBOEDTODataLoader;

		/// <summary>
		/// Active Directory Utilities
		/// </summary>
		private readonly IActiveDirectoryUtilities activeDirectoryUtilities;

		/// <summary>
		/// User Data Loader
		/// </summary>
		private IUserDTODataLoader userDataLoader { get; set; }

		/// <summary>
		/// Contract Type loader
		/// </summary>
		private readonly ContractTypeLoader contractTypeLoader;

		/// <summary>
		/// TM Resource Rate Loader
		/// </summary>
		private readonly ITMResourceRateDTODataLoader tmResourceRateLoader;

		/// <summary>
		/// Resource Loader
		/// </summary>
		private readonly IResourceDTODataLoader resourceLoader;

		/// <summary>
		/// TM Calculator Utility
		/// </summary>
		private readonly TMCalculator tmCalculator;

		/// <summary>
		/// In-use data loader
		/// </summary>
		private readonly IInUseDataLoader inUseDataLoader;

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("BoeDataAPIController");

		/// <summary>
		/// BOE Reports Http Service
		/// </summary>
		private readonly BOEReportsHttpService boeReportsHttpService = new BOEReportsHttpService();

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
		/// <param name="traceTableExporter">Trace Table data exporter</param>
		/// <param name="boeFormControllerLogic">BOE Form Controller logic</param>
		/// <param name="contractTypeLoader">Pick List loader for Contract Types</param>
		/// <param name="tmResourceRateLoader">TM Resource Loader</param>
		public BoeDataAPIController(IWorkspaceDTODataLoader loader, TokenHandling tokenHandler, IReportsControllerLogic reportsControllerLogic, ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, IBOEExporter boeExporter, IBOECustomExporter boeCustomExporter, IWorkspaceExportFormatDTODataLoader workspaceExportFormatDTOLoader, ITraceTableExporter traceTableExporter, IBOEFormControllerLogic boeFormControllerLogic, IBOEFormPBOEDTODataLoader boeFormPBOEDTODataLoader, IBOEFormIBOEDTODataLoader boeFormIBOEDTODataLoader, IActiveDirectoryUtilities activeDirectoryUtilities, IUserDTODataLoader userDataLoader, ContractTypeLoader contractTypeLoader, IResourceDTODataLoader resourceLoader, TMCalculator tmCalculator, IInUseDataLoader inUseDataLoader, ITMResourceRateDTODataLoader tmResourceRateLoader)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.loader = loader;
			this.tokenHandler = tokenHandler;
			this.reportsControllerLogic = reportsControllerLogic;
			this.boeExporter = boeExporter;
			this.boeCustomExporter = boeCustomExporter;
			this.workspaceExportFormatDTOLoader = workspaceExportFormatDTOLoader;
			this.traceTableExporter = traceTableExporter;
			this.boeFormControllerLogic = boeFormControllerLogic;
			this.boeFormPBOEDTODataLoader = boeFormPBOEDTODataLoader;
			this.boeFormIBOEDTODataLoader = boeFormIBOEDTODataLoader;
			this.activeDirectoryUtilities = activeDirectoryUtilities;
			this.userDataLoader = userDataLoader;
			this.contractTypeLoader = contractTypeLoader;
			this.resourceLoader = resourceLoader;
			this.tmCalculator = tmCalculator;
			this.inUseDataLoader = inUseDataLoader;
			this.tmResourceRateLoader = tmResourceRateLoader;
		}
		#endregion

		/// <summary>
		/// Get Workspace data for ACV. Limits the number of records returned to 100.
		/// </summary>
		/// <param name="ptmTrackingNumber">PTM Tracking Number</param>
		/// <returns>Workspace Data</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<AcvWorkspaceData> GetWorkspaceDataForProposal(string ptmTrackingNumber)
		{
			IESResponse<AcvWorkspaceData> result = new IESResponse<AcvWorkspaceData>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				ICollection<(int Id, string shortName, string longName, bool containsOCI, string ptmTrackingNumber)> data = this.loader.GetWorkspaceDataForProposal(ptmTrackingNumber);
				ParseAcvWorkspaceData(result, data);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Workspace data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get Workspace data for ACV. Limits the number of records returned to 100.
		/// </summary>
		/// <returns>Workspace Data List</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<AcvWorkspaceData> GetWorkspaceData()
		{
			IESResponse<AcvWorkspaceData> result = new IESResponse<AcvWorkspaceData>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				ICollection<(int Id, string shortName, string longName, bool containsOCI, string ptmTrackingNumber)> data = this.loader.GetWorkspaceData();
				ParseAcvWorkspaceData(result, data);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Workspace data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Gets all of the Custom Field Names for a Workspace.
		/// </summary>
		/// <param name="workspaceShortName">Short name of the workspace</param>
		/// <returns>HttpResponseMessage</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<string> GetWorkspaceCustomFieldNames(string workspaceShortName)
		{
			IESResponse<string> result = new IESResponse<string>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceShortName);
				if (this.HasOciPermission(SecurityPage.BoeCustomFields, workspace))
				{
					result.Data = workspace.CustomFields.Select(c => c.CustomFieldName).ToList();
					result.IsSuccessful = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Workspace Custom Field Name data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Export a pboe based on subcontractor name
		/// </summary>
		/// <param name="workspaceShortName">Short name of the workspace</param>
		/// <param name="subcontractor">Subcontractor name</param>
		/// <returns>HttpResponseMessage</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public HttpResponseMessage ExportPBOE(string workspaceShortName, string subcontractor)
		{
			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceShortName);
				if (!this.HasOciPermission(SecurityPage.Reports, workspace))
				{
					return new HttpResponseMessage(HttpStatusCode.Unauthorized);
				}

				bool isPortionMarkingEnabled = SiteMasterUtilities.IsPortionMarkingEnabled;
				ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();

				int? pboeId = this.boeFormControllerLogic.GetSummaryForms(workspace).FirstOrDefault(s => s.BOEFormType == BOEFormType.PBOE && s.BOEFormName == subcontractor)?.BOEFormId;
				if (pboeId.HasValue)
				{
					Stream stream = this.boeFormControllerLogic.ExportBOEFormReportAsStream(workspace, pboeId.Value, BOEFormType.PBOE, isPortionMarkingEnabled, contractTypes);

					string fileName = "PBOE_" + subcontractor + ".docx";

					HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
					response.Content = new StreamContent(stream);
					response.Content.Headers.ContentType = new MediaTypeHeaderValue(BOEExporterConstants.ContentType_DOCX);
					response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
					{
						FileName = fileName
					};

					return response;
				}
				else
				{
					logger.Error($"Unknown Subcontractor {subcontractor} sent in for workspace {workspaceShortName}");
					return new HttpResponseMessage(HttpStatusCode.InternalServerError);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}
		}

		/// <summary>
		/// Export an iboe based on iwta name
		/// </summary>
		/// <param name="workspaceShortName">Short name of the workspace</param>
		/// <param name="iwtaName">Iwta name</param>
		/// <returns>HttpResponseMessage</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public HttpResponseMessage ExportIBOE(string workspaceShortName, string iwtaName)
		{
			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceShortName);
				if (!this.HasOciPermission(SecurityPage.Reports, workspace))
				{
					return new HttpResponseMessage(HttpStatusCode.Unauthorized);
				}

				bool isPortionMarkingEnabled = SiteMasterUtilities.IsPortionMarkingEnabled;
				ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();

				int? iboeId = this.boeFormControllerLogic.GetSummaryForms(workspace).FirstOrDefault(s => s.BOEFormType == BOEFormType.IBOE && s.BOEFormName == iwtaName)?.BOEFormId;
				if (iboeId.HasValue)
				{
					Stream stream = this.boeFormControllerLogic.ExportBOEFormReportAsStream(workspace, iboeId.Value, BOEFormType.IBOE, isPortionMarkingEnabled, contractTypes);

					string fileName = "IBOE_" + iwtaName + ".docx";

					HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
					response.Content = new StreamContent(stream);
					response.Content.Headers.ContentType = new MediaTypeHeaderValue(BOEExporterConstants.ContentType_DOCX);
					response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
					{
						FileName = fileName
					};

					return response;
				}
				else
				{
					logger.Error($"Unknown IWTA name {iwtaName} sent in for workspace {workspaceShortName}");
					return new HttpResponseMessage(HttpStatusCode.InternalServerError);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}
		}

		/// <summary>
		/// Export all BOEs given workspace 
		/// </summary>
		/// <param name="workspaceShortName">Short name of the workspace</param>
		/// <returns>HttpResponseMessage</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public async Task<HttpResponseMessage> ExportAllBOEs(string workspaceShortName)
		{
			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceShortName);
				if (!this.HasOciPermission(SecurityPage.Reports, workspace))
				{
					return new HttpResponseMessage(HttpStatusCode.Unauthorized);
				}

				HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

				string fileName = Utilities.StripIllegalFileNameCharacters(string.Format("genBOE-Export-{0}.docx", workspace.WorkspaceName).Replace(",", string.Empty));

				MemoryStream stream = new MemoryStream();

				this.reportsControllerLogic.PrepareAllBOEsReport(workspace, this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id)
																									.Any(p => p.Role == Role.SubcontractorAuthor && p.ETIUserId == workspace.CurrentActiveUser.UserID),
																												null,
																												null,
																												null,
																												out bool isCustomExport,
																												out WorkspaceExportFormatDTO wsExportFormatDTO,
																												out BOEExportInputs exportInputs,
																												out ICollection<BOEExportModelView> boeExportModelViews,
																												out List<BOESummaryGridModelView> boeSummaryGridModelViews,
																												false);

				if (isCustomExport)
				{
					wsExportFormatDTO = wsExportFormatDTO.ExportFormat.ParentTemplateId < 9001 || wsExportFormatDTO.ExportFormat.ParentTemplateId > 10000 || wsExportFormatDTO.ExportFormat.ParentTemplateId == null ? this.workspaceExportFormatDTOLoader.GetById((int)ExcelReportTemplateType.MASTER) : wsExportFormatDTO;
				}

				if (Utilities.IsReportGenerationExternal)
				{
					await this.boeReportsHttpService.ExportBOEsToWordStream(null, stream, isCustomExport, wsExportFormatDTO, exportInputs, boeExportModelViews,
						boeSummaryGridModelViews, false);
				}
				else
				{
					if (isCustomExport)
					{
						boeCustomExporter.ExportBOEToWordFileStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, workspace, null, stream, wsExportFormatDTO);
					}
					else
					{
						boeExporter.ExportBOEToWordFileStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, workspace, fileName, stream, wsExportFormatDTO.ExportFormat.TemplateType);
					}
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
		/// Exports all BOEs as a zip file stream
		/// </summary>
		/// <param name="workspaceShortName">The workspaceShortName</param>
		/// <returns>Zip file stream</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		[HttpGet]
		public async Task<IESSingleResponse<Byte[]>> ExportAllBOEsAsStream(string workspaceShortName)
		{
			tokenHandler.AuthenticateUserFromAuthorizationToken();
			IESSingleResponse<Byte[]> response = new IESSingleResponse<Byte[]>();

			try
			{
				FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceShortName);
				MemoryStream stream = new MemoryStream();

				this.reportsControllerLogic.PrepareAllBOEsReport(
					workspace,
					this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id)
						.Any(p => p.Role == Role.SubcontractorAuthor && p.ETIUserId == workspace.CurrentActiveUser.UserID),
					null,
					null,
					null,
					out bool isCustomExport,
					out WorkspaceExportFormatDTO wsExportFormatDTO,
					out BOEExportInputs exportInputs,
					out ICollection<BOEExportModelView> boeExportModelViews,
					out List<BOESummaryGridModelView> boeSummaryGridModelViews,
					false);

				if (isCustomExport)
				{
					// distinguish between MASTER and legacy templates
					// if legacy, use original MASTER, otherwise use selected template
					wsExportFormatDTO = wsExportFormatDTO.ExportFormat.ParentTemplateId < 9001 || wsExportFormatDTO.ExportFormat.ParentTemplateId > 10000 || wsExportFormatDTO.ExportFormat.ParentTemplateId == null ? this.workspaceExportFormatDTOLoader.GetById((int)ExcelReportTemplateType.MASTER) : wsExportFormatDTO;
				}
				if (Utilities.IsReportGenerationExternal)
				{
					await this.boeReportsHttpService.ExportBOEsToWord(null, null, isCustomExport, wsExportFormatDTO, exportInputs, boeExportModelViews,
						boeSummaryGridModelViews, true, stream, true);
				}
				else if (isCustomExport)
				{
					this.boeCustomExporter.ExportBOEsToZipFile(
						exportInputs,
						boeExportModelViews,
						boeSummaryGridModelViews,
						workspace,
						null,
						null,
						string.Format("genBOEExport-{0}.zip", workspace.WorkspaceName).Replace(",", string.Empty),
						wsExportFormatDTO,
						stream,
						true);
				}
				else
				{
					this.boeExporter.ExportBOEsToZipFile(
					exportInputs,
					boeExportModelViews,
					boeSummaryGridModelViews,
					workspace,
					null,
					string.Format("genBOEExport-{0}.zip", workspace.WorkspaceName).Replace(",", string.Empty),
					wsExportFormatDTO.PhysicalFilePathCache,
					wsExportFormatDTO.ExportFormat.TemplateType,
					stream,
					true
					);
				}

				response.Data = stream.ToArray();
				response.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				response.Messages.Add($"Error occured while trying to export all boes as zip file: {ex.Message}");
			}

			return response;
		}

		/// <summary>
		/// Gets BOE looping Data for use in ACV
		/// </summary>
		/// <param name="workspaceShortName">The workspace short name</param>
		/// <returns>List of BOELoopingData</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<BOELoopingData> GetBOELoopingData(string workspaceShortName)
		{
			IESResponse<BOELoopingData> result = new IESResponse<BOELoopingData>();
			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceShortName);
				if (this.HasOciPermission(SecurityPage.ManageBOEs, workspace))
				{
					decimal taskElementTotal = 0m;
					decimal boeTotal = 0m;

					result.Data.AddRange(
						workspace.Boes.Select(x =>
						{
							x.TaskElements.ForEach(t =>
							{
								taskElementTotal += t.TotalCost ?? 0;
							});
							boeTotal += taskElementTotal;

							return new BOELoopingData
							{
								BOEId = x.Id,
								BOEName = x.Title,
								WBS = x.Wbs != null ? x.Wbs.WbsTitle : "NO WBS",
								CLIN = x.Clin != null ? x.Clin.ClinTitle : "NO CLIN",
								TotalCost = taskElementTotal
							};
						})
					);

					result.IsSuccessful = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Workspace BOE data: {ex.Message}");
			}

			return result;
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

		/// <summary>
		/// Get the genBOE Workspace data for use with a Trace Table in ACV
		/// </summary>
		/// <param name="workspaceShortName">Workspace short name</param>
		/// <param name="settingsData">Trace Table Settings Data</param>
		/// <returns>genBOE Workspace data for use with a Trace Table in ACV</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
		public IESResponse<TraceTableBoeData> GetWorkspaceDataForTraceTable(string workspaceShortName, TraceTableSettingsData settingsData)
		{
			IESResponse<TraceTableBoeData> boeData = new IESResponse<TraceTableBoeData>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceShortName);
				if (this.HasOciPermission(SecurityPage.Reports, workspace))
				{
					boeData.Data = traceTableExporter.ExportTraceTableData(workspace, settingsData);
					boeData.IsSuccessful = true;
				}

			}
			catch (Exception ex)
			{
				logger.Error(ex);
				boeData.Messages.Add($"Unknown error occurred returning Workspace data for Trace Table: {ex.Message}");
			}

			return boeData;
		}

		/// <summary>
		/// Get the genBOE Workspace data group for use with a Trace Table in ACV
		/// </summary>
		/// <param name="workspaceShortName">Workspace short name</param>
		/// <param name="settingsData">Trace Table Settings Data</param>
		/// <returns>genBOE Workspace data group for use with a Trace Table in ACV</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
		public IESResponse<TraceTableBoeDataGroup> GetWorkspaceDataForTraceTableGroup(string workspaceShortName, TraceTableSettingsData settingsData)
		{
			IESResponse<TraceTableBoeDataGroup> boeData = new IESResponse<TraceTableBoeDataGroup>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceShortName);
				if (this.HasOciPermission(SecurityPage.Reports, workspace))
				{
					boeData.Data = traceTableExporter.ExportTraceTableDataGroup(workspace, settingsData);
					boeData.IsSuccessful = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				boeData.Messages.Add($"Unknown error occurred returning Workspace data group for Trace Table: {ex.Message}");
			}

			return boeData;
		}

		/// <summary>
		/// Gets all of the IWTA Company Names for a Workspace
		/// </summary>
		/// <param name="workspaceShortName">Short name of the workspace</param>
		/// <returns>HttpResponseMessage</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<BOEFormData> GetIwtaCompanies(string workspaceShortName)
		{
			IESResponse<BOEFormData> result = new IESResponse<BOEFormData>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceShortName);
				if (this.HasOciPermission(SecurityPage.ManageBOEForms, workspace))
				{
					ICollection<BOEFormModelView> forms = this.boeFormControllerLogic.GetSummaryForms(workspace);

					result.Data = forms.Where(f => f.BOEFormType == BOEFormType.IBOE).Select(p =>
						new BOEFormData()
						{
							Name = p.BOEFormName,
							TotalCost = workspace.IsUsingTM ? p.TotalCost + p.TMCost : p.TotalCost
						}).ToList();
					result.IsSuccessful = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occurred returning IBOE data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Gets all of the Subcontractors for a Workspace
		/// </summary>
		/// <param name="workspaceID">Workspace Id</param>
		/// <returns>HttpResponseMessage</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<BOEFormData> GetSubcontractors(int workspaceID)
		{
			IESResponse<BOEFormData> result = new IESResponse<BOEFormData>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceID);
				if (this.HasOciPermission(SecurityPage.ManageBOEForms, workspace))
				{
					ICollection<BOEFormModelView> forms = this.boeFormControllerLogic.GetSummaryForms(workspace);

					result.Data = forms.Where(f => f.BOEFormType == BOEFormType.PBOE).Select(p =>
						new BOEFormData()
						{
							PBOEId = p.BOEFormId,
							Name = p.NLFSupplierName,
							TotalCost = workspace.IsUsingTM ? p.TotalCost + p.TMCost : p.TotalCost,
							IsIncomplete = p.IsIncomplete
						}).ToList();
					result.IsSuccessful = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occurred returning PBOE data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Gets all of the Subcontractors for a Workspace
		/// </summary>
		/// <param name="trackingNumber">Tracking Number</param>
		/// <returns>HttpResponseMessage</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<BOEFormData> GetSubcontractors(string trackingNumber)
		{
			IESResponse<BOEFormData> result = new IESResponse<BOEFormData>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace workspace = this.Factory.CreateFullWorkspace(trackingNumber);
				if (this.HasOciPermission(SecurityPage.ManageBOEForms, workspace))
				{
					result.Data = GetSubcontractorsData(workspace);
					result.IsSuccessful = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occurred returning PBOE data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Gets all of the Subcontractors for a Workspace for NLF
		/// </summary>
		/// <param name="trackingNumber">Tracking Number</param>
		/// <returns>HttpResponseMessage</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<BOEFormData> GetSubcontractorsForNLF(string trackingNumber)
		{
			IESResponse<BOEFormData> result = new IESResponse<BOEFormData>();

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				result.Data = GetSubcontractorsData(Factory.CreateFullWorkspace(trackingNumber));
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occurred returning PBOE data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get the ContainsOCI setting for the given workspace
		/// </summary>
		/// <param name="workspaceShortName">Workspace Short Name</param>
		/// <returns>HttpResponseMessage containing the ContainsOCI setting</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<bool> GetOciSetting(string workspaceShortName)
		{
			IESResponse<bool> result = new IESResponse<bool>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				bool? containsOci = loader.GetWorkspaceOciSettingByShortname(workspaceShortName);

				if (containsOci != null)
				{
					result.Data = new Collection<bool>() { containsOci.Value };
					result.IsSuccessful = true;
				}
				else
				{
					result.Messages.Add("Unable to retrieve OCI setting.");
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occurred returning OCI setting data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get workspaces for user (id via token) for use in NLF
		/// </summary>
		/// <returns>List of Workspace Data for user for use in NLF</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
		public IESResponse<NlfWorkspaceInnerData> GetAllWorkspaceInnerDataForNlf()
		{
			IESResponse<NlfWorkspaceInnerData> result = new IESResponse<NlfWorkspaceInnerData>();
			try
			{
				tokenHandler.ValidateAuthorizationToken();

				ICollection<NlfWorkspaceInnerDataDTO> boes = loader.GetAllWorkspaceInnerDataForNlf();
				result.Data = boes.Select<NlfWorkspaceInnerDataDTO, NlfWorkspaceInnerData>(x => new NlfWorkspaceInnerData()
				{
					WorkspaceId = x.WorkspaceId,
					WorkspaceUrl = x.WorkspaceUrl,
					WorkspaceName = x.WorkspaceName,
					WorkspaceCreationDate = x.WorkspaceCreationDate,
					PTMTrackingNumber = x.PTMTrackingNumber,
					EstimatingLead = x.EstimatingLead,
					LineOfBusinessId = x.LineOfBusiness is null ? -1 : x.LineOfBusiness.LineOfBusinessID,
					LineOfBusinessName = x.LineOfBusiness is null ? String.Empty : x.LineOfBusiness.LineOfBusinessName
				}).ToCollection();

				result.IsSuccessful = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = "Invalid permission to get all Workspace inner data.";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occurred returning NLF Workspace data: {ex.Message}");
				result.IsSuccessful = false;
			}

			return result;
		}

		/// <summary>
		/// Get workspaces for user (id via token) for use in NLF
		/// </summary>
		/// <returns>List of Workspace Data for user for use in NLF</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
		public IESResponse<NlfWorkspaceInnerData> GetAllWorkspaceInnerDataByTrackingNumbersForNlf([FromBody] ICollection<string> trackingNumbers, [FromUri] string nlfApiKey = null, [FromUri] string ntid = null)
		{
			IESResponse<NlfWorkspaceInnerData> result = new IESResponse<NlfWorkspaceInnerData>();
			try
			{
				tokenHandler.ValidateAuthorizationToken();

				if (trackingNumbers != null)
				{
					ICollection<NlfWorkspaceInnerDataDTO> boes = new Collection<NlfWorkspaceInnerDataDTO>();
					if (nlfApiKey != null && nlfApiKey == NlfApiKey())
					{
						boes = loader.GetWorkspaceInnerDataForNlf(trackingNumbers);
					}
					else
					{
						boes = loader.GetWorkspaceInnerDataByNtidForNlf(ntid, trackingNumbers);
					}
					result.Data = boes.Select<NlfWorkspaceInnerDataDTO, NlfWorkspaceInnerData>(x => new NlfWorkspaceInnerData()
					{
						WorkspaceId = x.WorkspaceId,
						WorkspaceUrl = x.WorkspaceUrl,
						WorkspaceName = x.WorkspaceName,
						WorkspaceCreationDate = x.WorkspaceCreationDate,
						PTMTrackingNumber = x.PTMTrackingNumber,
						EstimatingLead = x.EstimatingLead,
						LineOfBusinessId = x.LineOfBusiness is null ? -1 : x.LineOfBusiness.LineOfBusinessID,
						LineOfBusinessName = x.LineOfBusiness is null ? String.Empty : x.LineOfBusiness.LineOfBusinessName
					}).ToCollection();

					result.IsSuccessful = true;
				}
				else
				{
					throw new ArgumentNullException("trackingNumbers");
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = "Invalid permission to Workspace with tracking numbers:" + string.Join(", ", trackingNumbers) + ". ";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occurred returning NLF Workspace data: {ex.Message}");
				result.IsSuccessful = false;
			}

			return result;
		}

		/// <summary>
		/// Get workspace inner data for user (id via token) for use in NLF using Workspace Id
		/// </summary>
		/// <returns>Workspace Inner Data for user for use in NLF</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<NlfWorkspaceInnerData> GetNlfWorkspaceInnerDataByWorkspaceIdForUser(int workspaceId)
		{
			IESResponse<NlfWorkspaceInnerData> result = new IESResponse<NlfWorkspaceInnerData>();

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				ICollection<NlfWorkspaceInnerDataDTO> boes = new Collection<NlfWorkspaceInnerDataDTO>();

				if (workspaceId > 0)
				{
					// get all workspaces
					boes = loader.GetWorkspaceInnerDataForNlf(workspaceId);
				}

				result.Data = boes.Select<NlfWorkspaceInnerDataDTO, NlfWorkspaceInnerData>(x => new NlfWorkspaceInnerData()
				{
					WorkspaceId = x.WorkspaceId,
					WorkspaceUrl = x.WorkspaceUrl,
					WorkspaceName = x.WorkspaceName,
					WorkspaceCreationDate = x.WorkspaceCreationDate,
					PTMTrackingNumber = x.PTMTrackingNumber,
					EstimatingLead = x.EstimatingLead,
					LineOfBusinessId = x.LineOfBusiness is null ? -1 : x.LineOfBusiness.LineOfBusinessID,
					LineOfBusinessName = x.LineOfBusiness is null ? String.Empty : x.LineOfBusiness.LineOfBusinessName
				}).ToCollection();

				result.IsSuccessful = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = "Invalid permission to Workspace with id:" + workspaceId + ". ";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occurred returning NLF Workspace inner data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get workspace inner data for user (id via token) for use in NLF using Workspace Id
		/// </summary>
		/// <param name="trackingNumber">Tracking Number</param>
		/// <returns>Workspace Inner Data for user for use in NLF</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<NlfWorkspaceInnerData> GetNlfWorkspaceInnerDataByTrackingNumberForUser(string trackingNumber)
		{
			IESResponse<NlfWorkspaceInnerData> result = new IESResponse<NlfWorkspaceInnerData>();

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				ICollection<NlfWorkspaceInnerDataDTO> boes = new Collection<NlfWorkspaceInnerDataDTO>();

				if (!string.IsNullOrWhiteSpace(trackingNumber))
				{
					// get all workspaces
					boes = loader.GetWorkspaceInnerDataForNlf(trackingNumber);
				}

				result.Data = boes.Select<NlfWorkspaceInnerDataDTO, NlfWorkspaceInnerData>(x => new NlfWorkspaceInnerData()
				{
					WorkspaceId = x.WorkspaceId,
					WorkspaceUrl = x.WorkspaceUrl,
					WorkspaceName = x.WorkspaceName,
					WorkspaceCreationDate = x.WorkspaceCreationDate,
					PTMTrackingNumber = x.PTMTrackingNumber,
					EstimatingLead = x.EstimatingLead,
					LineOfBusinessId = x.LineOfBusiness is null ? -1 : x.LineOfBusiness.LineOfBusinessID,
					LineOfBusinessName = x.LineOfBusiness is null ? String.Empty : x.LineOfBusiness.LineOfBusinessName
				}).ToCollection();

				result.IsSuccessful = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = "Invalid permission to Workspace with tracking number:" + trackingNumber + ". ";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occurred returning NLF Workspace inner data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get Material PBoe Data for given Workspace
		/// </summary>
		/// <param name="workspaceID">Workspace ID</param>
		/// <returns>Collection of Material PBoe Data</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<MPBoeData> GetMaterialPBoeForTrackingNumber(string trackingNumber)
		{
			IESResponse<MPBoeData> result = new IESResponse<MPBoeData>();

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				result.Data = loader.GetMaterialPBoeForWorkspace(trackingNumber).Select<MPBoeDataDTO, MPBoeData>(x => new MPBoeData()
				{
					CLINNumbers = x.CLINNumbers,
					RFPNumber = x.RFPNumber,
					PTMProposalTitle = x.PTMProposalTitle,
					WBSNumbers = x.WBSNumbers,
					WorkspaceName = x.WorkspaceName,
					ShortName = x.ShortName,
					TrackingNumber = x.TrackingNumber
				}).ToCollection();

				result.IsSuccessful = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = "Invalid permission to Workspace with tracking number:" + trackingNumber + ". ";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Error occurred returning Material PBoe Data for given Workspace with tracking number: {trackingNumber}: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get all PBOEs for a given Workspace.
		/// </summary>
		/// <param name="trackingNumber">Tracking Number</param>
		/// <returns>Collection of PBOEs by Tracking Number</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<PBOEData> GetPBOEsForTrackingNumber(string trackingNumber)
		{
			IESResponse<PBOEData> result = new IESResponse<PBOEData>();

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				result.Data = boeFormPBOEDTODataLoader.GetPBOEsForTrackingNumber(trackingNumber).Select<PBOEDataDTO, PBOEData>(x =>
				{
					UserData approver = activeDirectoryUtilities.SearchUsers(x.Approver, ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.StartsWith).FirstOrDefault();
					UserData contractsLead = activeDirectoryUtilities.SearchUsers(x.ContractsLeadDisplayName, ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.StartsWith).FirstOrDefault();
					UserDTO leadEstimator = userDataLoader.GetUserByID(x.LeadEstimatorId);

					return new PBOEData()
					{
						PBoeID = x.PBoeID,
						FormName = x.FormName,
						Description = x.Description,
						BasisAndRationale = x.BasisAndRationale,
						SupplierName = x.SupplierName,
						VendorId = x.VendorId,
						RFP = x.RFP,
						ProposalNumber = x.ProposalNumber,
						SubResources = x.SubResources,
						Revision = x.Revision,
						FormVersion = x.FormVersion,
						TotalCost = x.TotalCost.GetValueOrDefault(),
						SupplierProposedValue = x.SupplierProposedValue,
						ProposalTitle = x.ProposalTitle,
						CCoPD = x.CCoPD,
						CCoPDOtherText = x.CCoPDOtherText,
						IsCCoPD = x.IsCCoPD.GetValueOrDefault(),
						IsCompetitionException = x.IsCompetitionException.GetValueOrDefault(),
						IsCommercialItemException = x.IsCommercialItemException.GetValueOrDefault(),
						IsCCoPDThresholdException = x.IsCCoPDThresholdException.GetValueOrDefault(),
						IsCCoPDOtherException = x.IsCCoPDOtherException.GetValueOrDefault(),
						ShouldCostEstimate = x.ShouldCostEstimate.GetValueOrDefault(),
						ShouldCostEstimateDate = x.ShouldCostEstimateDate.GetValueOrDefault(),
						ShouldCostEstimateText = x.ShouldCostEstimateText,
						SowWritten = x.SowWritten.GetValueOrDefault(),
						SowWrittenDate = x.SowWrittenDate.GetValueOrDefault(),
						SowWrittenText = x.SowWrittenText,
						FirmSupplierReceipt = x.FirmSupplierReceipt.GetValueOrDefault(),
						FirmSupplierReceiptDate = x.FirmSupplierReceiptDate.GetValueOrDefault(),
						FirmSupplierReceiptText = x.FirmSupplierReceiptText,
						SourceSelection = x.SourceSelection.GetValueOrDefault(),
						SourceSelectionDate = x.SourceSelectionDate.GetValueOrDefault(),
						SourceSelectionText = x.SourceSelectionText,
						CID = x.CID.GetValueOrDefault(),
						CIDDate = x.CIDDate.GetValueOrDefault(),
						CIDText = x.CIDText,
						GovtReview = x.GovtReview.GetValueOrDefault(),
						GovtReviewDate = x.GovtReviewDate.GetValueOrDefault(),
						GovtReviewText = x.GovtReviewText,
						PriceAnalysis = x.PriceAnalysis.GetValueOrDefault(),
						PriceAnalysisDate = x.PriceAnalysisDate.GetValueOrDefault(),
						PriceAnalysisText = x.PriceAnalysisText,
						FactFinding = x.FactFinding.GetValueOrDefault(),
						FactFindingDate = x.FactFindingDate.GetValueOrDefault(),
						FactFindingText = x.FactFindingText,
						CostAnalysis = x.CostAnalysis.GetValueOrDefault(),
						CostAnalysisDate = x.CostAnalysisDate.GetValueOrDefault(),
						CostAnalysisText = x.CostAnalysisText,
						GovtPricing = x.GovtPricing.GetValueOrDefault(),
						GovtPricingDate = x.GovtReviewDate.GetValueOrDefault(),
						GovtPricingText = x.GovtReviewText,
						GovtPricingReceived = x.GovtPricingReceived.GetValueOrDefault(),
						GovtPricingReceivedDate = x.GovtPricingReceivedDate.GetValueOrDefault(),
						GovtPricingReceivedText = x.GovtPricingReceivedText,
						MOU = x.MOU.GetValueOrDefault(),
						MOUDate = x.MOUDate.GetValueOrDefault(),
						MOUText = x.MOUText,
						CostAnalysisUnqualified = x.CostAnalysisUnqualified.GetValueOrDefault(),
						CostAnalysisUnqualifiedDate = x.CostAnalysisUnqualifiedDate.GetValueOrDefault(),
						CostAnalysisUnqualifiedText = x.CostAnalysisUnqualifiedText,
						TechnicalEvaluation = x.TechnicalEvaluation.GetValueOrDefault(),
						TechnicalEvaluationDate = x.TechnicalEvaluationDate.GetValueOrDefault(),
						TechnicalEvaluationText = x.TechnicalEvaluationText,
						RFPRelease = x.RFPRelease.GetValueOrDefault(),
						RFPReleaseToSupplierDate = x.RFPReleaseToSupplierDate.GetValueOrDefault(),
						RFPReleaseText = x.RFPReleaseText,
						SupplierNegotiations = x.SupplierNegotiations.GetValueOrDefault(),
						SupplierNegotiationsDate = x.SupplierNegotiationsDate.GetValueOrDefault(),
						SupplierNegotiationsText = x.SupplierNegotiationsText,
						ProposalDate = x.ProposalDate,
						ValidityDate = x.ValidityDate,
						SupplierProposalSupportingDataIncluded = x.SupplierProposalSupportingDataIncluded.GetValueOrDefault(),
						PriceAnalysisIncluded = x.PriceAnalysisIncluded.GetValueOrDefault(),
						CommercialItemDocIncluded = x.CommercialItemDocIncluded.GetValueOrDefault(),
						CostAnalysisIncluded = x.CostAnalysisIncluded.GetValueOrDefault(),
						Approver = x.Approver,
						LeadEstimatorId = x.LeadEstimatorId,
						LeadEstimatorDisplayName = leadEstimator != null ? leadEstimator.DisplayName : string.Empty,
						LeadEstimatorEmail = leadEstimator != null ? leadEstimator.EmailAddress : string.Empty,
						SupplierProposalManagerDisplayName = approver != null ? approver.DisplayName : string.Empty,
						SupplierProposalManagerEmail = approver != null ? approver.Email : string.Empty,
						SupplierProposalManagerPhone = approver != null ? approver.Phone : string.Empty,
						ContractsLeadDisplayName = contractsLead != null ? contractsLead.DisplayName : string.Empty,
						ContractsLeadEmail = contractsLead != null ? contractsLead.Email : string.Empty,
						ContractsLeadPhone = contractsLead != null ? contractsLead.Phone : string.Empty,
						PlannedDateWrittenApproval = x.PlannedDateWrittenApproval.GetValueOrDefault(),
						PlannedDateApprovedSubmission = x.PlannedDateApprovedSubmission.GetValueOrDefault(),
						SupplierCCoPD = x.SupplierCCoPD.GetValueOrDefault(),
						SourceSelectionDescription = x.SourceSelectionDescription,
						CommercialityDescription = x.CommercialityDescription,
						TechnicalEvaluationDescription = x.TechnicalEvaluationDescription,
						PriceAnalysisDescription = x.PriceAnalysisDescription,
						CostAnalysisDescription = x.CostAnalysisDescription,
						RationaleValueSummary = x.RationaleValueSummary,
						ClinContractXrefs = x.ClinContractXrefs,
						PTMTrackingNumber = x.TrackingNumber,
						WorkspaceId = x.WorkspaceId,
						LineOfBusinessName = x.LineOfBusinessName
					};
				}).ToList();

				result.IsSuccessful = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = "Invalid permission to Workspace with tracking number:" + trackingNumber + ". ";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Error occurred returning PBOE Data for given Workspace with tracking number: {trackingNumber}: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get all IBOEs for a given Workspace.
		/// </summary>
		/// <param name="trackingNumber">Tracking Number</param>
		/// <returns>Collection of IBOEs by Tracking Number</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<IBOEData> GetIBOEsForTrackingNumber(string trackingNumber)
		{
			IESResponse<IBOEData> result = new IESResponse<IBOEData>();

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				result.Data = boeFormIBOEDTODataLoader.GetByTrackingNumber(trackingNumber).Select<BOEFormIBOEDTO, IBOEData>(x =>
				{
					UserData poc = activeDirectoryUtilities.SearchUsers(x.Poc, ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.StartsWith).FirstOrDefault();
					UserData approver = activeDirectoryUtilities.SearchUsers(x.Approver, ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.StartsWith).FirstOrDefault();

					return new IBOEData()
					{
						UpdateDT = DateTime.UtcNow,
						PTMTrackingNumber = trackingNumber,
						FormName = x.FormName,
						Description = x.Description,
						BasisAndRationale = x.BasisAndRationale,
						ProposalTitle = x.ProposalTitle,
						ProposalDate = x.ProposalDate,
						Poc = poc != null ? poc.DisplayName : string.Empty,
						PocPhone = poc != null ? poc.Phone : string.Empty,
						PocEmail = poc != null ? poc.Email : string.Empty,
						Approver = approver != null ? approver.DisplayName : string.Empty,
						ApproverPhone = approver != null ? approver.Phone : string.Empty,
						ApproverEmail = approver != null ? approver.Email : string.Empty,
						Revision = x.Revision,
						FormVersion = x.Version,
						BusinessArea = x.BusinessArea,
						Resources = x.Resources,
						IBOEClinContractXREF = x.ClinContractXrefs
					};
				}).ToList();

				result.IsSuccessful = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = "Invalid permission to Workspace with tracking number:" + trackingNumber + ". ";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Error occurred returning IBOE Data for given Workspace with tracking number: {trackingNumber}: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get a single PBOE from a Tracking Number and PBOE ID.
		/// </summary>
		/// <param name="trackingNumber">Tracking Number</param>
		/// <param name="pboeID">PBOE ID</param>
		/// <returns>Single PBOE by Tracking Number and PBOE ID</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<PBOEData> GetPBOEByIDs(string trackingNumber, int pboeID)
		{
			IESResponse<PBOEData> result = new IESResponse<PBOEData>();

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				result.Data = boeFormPBOEDTODataLoader.GetPBOEByIDs(trackingNumber, pboeID).Select<PBOEDataDTO, PBOEData>(x =>
				{
					UserData approver = activeDirectoryUtilities.SearchUsers(x.Approver, ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.StartsWith).FirstOrDefault();
					UserDTO leadEstimator = userDataLoader.GetUserByID(x.LeadEstimatorId);

					return new PBOEData()
					{
						PBoeID = x.PBoeID,
						SupplierName = x.SupplierName,
						VendorId = x.VendorId,
						SubResources = x.SubResources,
						TotalCost = x.TotalCost.GetValueOrDefault(),
						SupplierProposedValue = x.SupplierProposedValue,
						IsCCoPD = x.IsCCoPD.GetValueOrDefault(),
						IsCommercialItemException = x.IsCommercialItemException.GetValueOrDefault(),
						IsCompetitionException = x.IsCompetitionException.GetValueOrDefault(),
						IsCCoPDOtherException = x.IsCCoPDOtherException.GetValueOrDefault(),
						IsCCoPDThresholdException = x.IsCCoPDThresholdException.GetValueOrDefault(),
						PriceAnalysis = x.PriceAnalysis.GetValueOrDefault(),
						PriceAnalysisDate = x.PriceAnalysisDate.GetValueOrDefault(),
						CostAnalysis = x.CostAnalysis.GetValueOrDefault(),
						CostAnalysisDate = x.CostAnalysisDate.GetValueOrDefault(),
						GovtPricingReceived = x.GovtPricingReceived.GetValueOrDefault(),
						GovtPricingReceivedDate = x.GovtPricingReceivedDate.GetValueOrDefault(),
						CostAnalysisUnqualified = x.CostAnalysisUnqualified.GetValueOrDefault(),
						CostAnalysisUnqualifiedDate = x.CostAnalysisUnqualifiedDate.GetValueOrDefault(),
						TechnicalEvaluation = x.TechnicalEvaluation.GetValueOrDefault(),
						TechnicalEvaluationDate = x.TechnicalEvaluationDate.GetValueOrDefault(),
						RFPReleaseToSupplierDate = x.RFPReleaseToSupplierDate.GetValueOrDefault(),
						SupplierNegotiationsDate = x.SupplierNegotiationsDate.GetValueOrDefault(),
						ProposalDate = x.ProposalDate,
						ValidityDate = x.ValidityDate,
						Approver = x.Approver,
						LeadEstimatorId = x.LeadEstimatorId,
						LeadEstimatorDisplayName = leadEstimator != null ? leadEstimator.DisplayName : string.Empty,
						LeadEstimatorEmail = leadEstimator != null ? leadEstimator.EmailAddress : string.Empty,
						SupplierProposalManagerDisplayName = approver != null ? approver.DisplayName : string.Empty,
						SupplierProposalManagerEmail = approver != null ? approver.Email : string.Empty,
						WorkspaceId = x.WorkspaceId,
						WorkspaceName = x.WorkspaceName
					};
				}).ToList();

				result.IsSuccessful = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = $"Invalid permission to Workspace with tracking number: {trackingNumber} and PBOE ID: {pboeID}.";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Error occurred returning PBoe Data for given Workspace with tracking number: {trackingNumber} and PBOE ID: {pboeID}: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get PBOE Totals for NLF
		/// </summary>
		/// <param name="postModel">Post Model containing all required data to retrieve Totals for PBOE</param>
		/// <returns>Collection of Rows for the PBOE that have totals calculated</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
		public IESResponse<PBOERow> GetPBOETotals([FromBody] BOETotalsPostModel postModel)
		{
			// Validations
			_ = postModel ?? throw new ArgumentNullException(nameof(postModel));

			IESResponse<PBOERow> result = new IESResponse<PBOERow>();

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				ICollection<BOEFormIBOEDTO> iboeDtos = new List<BOEFormIBOEDTO>();
				ICollection<BOEFormPBOEDTO> pboeDtos = new List<BOEFormPBOEDTO>();

				ICollection<WorkspaceDTO> workspaces = loader.GetWorkspacesByTrackingNumber(postModel.TrackingNumber).Where(x => x.CurrentPTMWorkspace).ToList();

				foreach (WorkspaceDTO ws in workspaces)
				{
					// Get Prerequisite Data
					FullWorkspace fullWorkspace = this.Factory.CreateFullWorkspace(ws.Id);
					ICollection<ResourceDTO> workspaceResources = this.resourceLoader.GetByListId(ws.ResourceListID);
					tmCalculator.GetBOETotals<PBOERow>(postModel, result, iboeDtos, pboeDtos, fullWorkspace, workspaceResources, resourceLoader);
				}

				result.IsSuccessful = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = $"Invalid permission to Workspace with tracking number: {postModel.TrackingNumber} and BOE Type: {postModel.BOEFormType.GetDescription()}";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Error occurred returning PBOE Totals : {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get IBOE Totals for NLF
		/// </summary>
		/// <param name="postModel">Post Model containing all required data to retrieve Totals for IBOE</param>
		/// <returns>Collection of Rows for the IBOE that have totals calculated</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
		public IESResponse<IBOERow> GetIBOETotals([FromBody] BOETotalsPostModel postModel)
		{
			// Validations
			_ = postModel ?? throw new ArgumentNullException(nameof(postModel));

			IESResponse<IBOERow> result = new IESResponse<IBOERow>();

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				ICollection<WorkspaceDTO> workspaces = loader.GetWorkspacesByTrackingNumber(postModel.TrackingNumber).Where(x => x.CurrentPTMWorkspace).ToList();

				ICollection<BOEFormIBOEDTO> iboeDtos = new List<BOEFormIBOEDTO>();
				ICollection<BOEFormPBOEDTO> pboeDtos = new List<BOEFormPBOEDTO>();

				foreach (WorkspaceDTO ws in workspaces)
				{
					if (ws.IsUsingTM)
					{
						// Get Prerequisite Data
						FullWorkspace fullWorkspace = this.Factory.CreateFullWorkspace(ws.Id);
						ICollection<ResourceDTO> workspaceResources = this.resourceLoader.GetByListId(ws.ResourceListID);
						tmCalculator.GetBOETotals<IBOERow>(postModel, result, iboeDtos, pboeDtos, fullWorkspace, workspaceResources, resourceLoader);
					}
				}

				result.IsSuccessful = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = $"Invalid permission to Workspace with tracking number: {postModel.TrackingNumber} and BOE Type: {postModel.BOEFormType.GetDescription()}";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Error occurred returning IBOE Totals : {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get clin data for pboes for NLF export
		/// </summary>
		/// <param name="pboes">list of pboes</param>
		/// <returns>Returns the list of pboes populated with clin data</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
		public IESResponse<PBOEViewModel> GetPBOEClinData([FromBody] List<PBOEViewModel> pboes)
		{
			// Validations
			_ = pboes ?? throw new ArgumentNullException(nameof(pboes));

			IESResponse<PBOEViewModel> result = new IESResponse<PBOEViewModel>();

			if (pboes == null || !pboes.Any())
			{
				string message = $"No PBOES from NLF";
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
				return result;
			}

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				ICollection<WorkspaceDTO> workspaces = loader.GetWorkspacesByTrackingNumber(pboes.First().PTMTrackingNumber).Where(x => x.CurrentPTMWorkspace).ToList();

				// Instantiate PBOE Exporter
				PBOEFormExporter exporter = new PBOEFormExporter(userDataLoader, resourceLoader, tmCalculator);


				foreach (PBOEViewModel pboe in pboes)
				{
					pboe.PboeClinData = new List<PBOEClinData>();
					foreach (WorkspaceDTO workspace in workspaces)
					{
						// Get Prerequisite Data
						FullWorkspace fullWorkspace = this.Factory.CreateFullWorkspace(workspace.Id);
						//transform PBOEViewModel into BOEFormPBOEDTO
						//put it in the exporter because it is PBOE specific
						BOEFormPBOEDTO pboeForm = exporter.transformPBOEViewToFormDTO(pboe);

						//need to convert resources into a list of ints (resource IDs)
						ICollection<ResourceDTO> workspaceResources = this.resourceLoader.GetByListId(fullWorkspace.ResourceListID);
						ICollection<ResourceDTO> actualResources = workspaceResources.Where(x => pboe.Resources.Contains(x.ResourceName)).ToList();
						List<int> resourceIds = actualResources.Select(r => r.Id).ToList();

						//if we don't have sub resources, still return the pboe with the pop data
						if (resourceIds != null && resourceIds.Any())
						{
							ICollection<PBOETableRow> rowData = exporter.PullRowsFromWorkspace(fullWorkspace, pboeForm, resourceIds, this.contractTypeLoader.GetPickListValues());
							foreach (PBOETableRow row in rowData)
							{
								// Use the contract type selected in NLF
								PBOECLINContractXREFData clinContractXref = pboe.PboeClinContractXREF.FirstOrDefault(x => x.Title == row.ClinNumber);
								if (clinContractXref != null)
								{
									row.ContractType = clinContractXref.ContractTypeName;
								}

								PBOEClinData data = new PBOEClinData(row);
								pboe.PboeClinData.Add(data);
							}
						}
					}
					pboe.SupplierPOP = exporter.PeriodOfPerformance;
					result.Data.Add(pboe);
				}

				if (result.Data.Count > 0)
				{
					result.IsSuccessful = true;
				}
				else
				{
					string message = $"No PBOE CLIN Data for given Tracking Number: {pboes.First().PTMTrackingNumber}";
					result.Messages.Add(message);
					result.IsSuccessful = false;
					result.Data = null;
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = $"Invalid permission to Workspace with tracking number: {pboes.First().PTMTrackingNumber}";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Error occurred returning PBOE CLIN Data: {ex.Message}");
			}
			return result;
		}

		/// <summary>
		/// Get clin data for iboes for NLF export
		/// </summary>
		/// <param name="iboes">list of iboes</param>
		/// <returns>Returns the list of iboes populated with clin data</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
		public IESResponse<IBOEViewModel> GetIBOEClinData([FromBody] List<IBOEViewModel> iboes)
		{
			// Validations
			_ = iboes ?? throw new ArgumentNullException(nameof(iboes));

			IESResponse<IBOEViewModel> result = new IESResponse<IBOEViewModel>();

			if (iboes == null || !iboes.Any())
			{
				string message = $"No IBOEs from NLF";
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
				return result;
			}

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				ICollection<WorkspaceDTO> workspaces = loader.GetWorkspacesByTrackingNumber(iboes.First().PTMTrackingNumber).Where(x => x.CurrentPTMWorkspace).ToList();

				// Instantiate IBOE Exporter
				IBOEFormExporter exporter = new IBOEFormExporter(userDataLoader, resourceLoader, tmCalculator);

				foreach (IBOEViewModel iboe in iboes)
				{
					bool containsOCi = false;
					iboe.IboeClinData = new List<IBOEClinData>();
					foreach (WorkspaceDTO workspace in workspaces)
					{
						// Get Prerequisite Data
						FullWorkspace fullWorkspace = this.Factory.CreateFullWorkspace(workspace.Id);
						// transform IBOEViewModel into BOEFormIBOEDTO
						// put it in the exporter because it is IBOE specific
						BOEFormIBOEDTO iboeForm = exporter.TransformIBOEViewToFormDTO(iboe);

						// need to convert resources into a list of ints (resource IDs)
						ICollection<ResourceDTO> workspaceResources = this.resourceLoader.GetByListId(fullWorkspace.ResourceListID);
						ICollection<ResourceDTO> actualResources = workspaceResources.Where(x => iboe.Resources.Contains(x.ResourceName)).ToList();
						List<int> resourceIds = actualResources.Select(r => r.Id).ToList();

						// if we don't have sub resources, still return the iboe with the pop data
						if (resourceIds != null && resourceIds.Any())
						{
							ICollection<IBOETableRow> rowData = exporter.PullRowsFromWorkspace(fullWorkspace, iboeForm, resourceIds, this.contractTypeLoader.GetPickListValues());
							foreach (IBOETableRow row in rowData)
							{
								// Use the contract type selected in NLF
								IBOECLINContractXREFData clinContractXref = iboe.IBOECLINContractXREF.FirstOrDefault(x => x.Title == row.ClinNumber);
								if (clinContractXref != null)
								{
									row.ContractType = clinContractXref.ContractTypeName;
								}

								IBOEClinData data = new IBOEClinData(row);
								iboe.IboeClinData.Add(data);
							}
						}

						if (workspace.ContainsOCI)
						{
							containsOCi = true;
						}
					}
					iboe.PeriodOfPerformance = exporter.PeriodOfPerformance;
					iboe.ContainsOCI = containsOCi;
					result.Data.Add(iboe);
				}

				if (result.Data.Count > 0)
				{
					result.IsSuccessful = true;
				}
				else
				{
					string message = $"No IBOE CLIN Data for given Tracking Number: {iboes.First().PTMTrackingNumber}";
					result.Messages.Add(message);
					result.IsSuccessful = false;
					result.Data = null;
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = $"Invalid permission to Workspace with tracking number: {iboes.First().PTMTrackingNumber}";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Error occurred returning IBOE CLIN Data: {ex.Message}");
			}
			return result;
		}

		/// <summary>
		/// Get in-use resources for the 'current' workspaces for the given tracking number and element of cost for NLF
		/// </summary>
		/// <param name="trackingNumber">Tracking Number</param>
		/// <param name="elementOfCost">Element of Cost of the resources to get (Sub for PBOE, IWTA for IBOE)</param>
		/// <returns>Resource data for the in-use resources</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<NlfResourceData> GetInUseResourcesForNlf(string trackingNumber, ElementOfCostType elementOfCost)
		{
			IESResponse<NlfResourceData> result = new IESResponse<NlfResourceData>();

			try
			{
				tokenHandler.ValidateAuthorizationToken();

				ICollection<WorkspaceDTO> workspaces = loader.GetWorkspacesByTrackingNumber(trackingNumber).Where(x => x.CurrentPTMWorkspace).ToCollection();

				foreach (WorkspaceDTO workspace in workspaces)
				{
					ICollection<TMResourceRateDTO> workspaceTMResourceRates = tmResourceRateLoader.GetByWorkspaceId(workspace.Id);
					IList<string> distinctTMRateResourceNames = workspaceTMResourceRates.Select(x => x.ResourceName).Distinct().ToList();

					HashSet<int> inUseIds = inUseDataLoader.GetWorkspaceResourceIDsInUseByListID(workspace.ResourceListID);
					ICollection<ResourceDTO> inUseResources = resourceLoader.GetByListId(workspace.ResourceListID)
						.Where(x => x.ElementOfCost == elementOfCost && inUseIds.Contains(x.Id) && !result.Data.Any(y => y.ResourceName == x.ResourceName)).ToCollection();

					// Remove Hour-type resources and if there are no T&M rates
					inUseResources = inUseResources.Where(x => x.RateType != RateType.Hours || distinctTMRateResourceNames.Contains(x.ResourceName)).ToCollection();

					result.Data.AddRange(inUseResources.Select(x => new NlfResourceData()
					{
						ResourceName = x.ResourceName,
						ResourceDescription = x.ResourceDesc
					}));
				}

				result.IsSuccessful = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				string message = "Invalid permission to Workspace with tracking number:" + trackingNumber + ". ";
				logger.Error(message + ex);
				result.Messages.Add(message);
				result.IsSuccessful = false;
				result.Data = null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Error occurred returning in-use {elementOfCost.GetDescription()} Resources for given Workspaces with tracking number: {trackingNumber}: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Gets the NLF API Key from app settings
		/// </summary>
		/// <returns>NLF API Key</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string NlfApiKey()
		{
			return ConfigurationUtilities.GetAppSetting("NlfApiKey");
		}

		/// <summary>
		/// Get Subcontractor form data
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <returns>Collection of Subcontractor Form Data</returns>
		private ICollection<BOEFormData> GetSubcontractorsData(FullWorkspace workspace)
		{
			ICollection<BOEFormModelView> forms = this.boeFormControllerLogic.GetSummaryForms(workspace);

			return forms.Where(f => f.BOEFormType == BOEFormType.PBOE).Select(p =>
				new BOEFormData()
				{
					PBOEId = p.BOEFormId,
					Name = p.NLFSupplierName,
					TotalCost = workspace.IsUsingTM ? p.TotalCost + p.TMCost : p.TotalCost,
					IsIncomplete = p.IsIncomplete
				}).ToList();
		}

		/// <summary>
		/// Parse out Workspace Data to AcvWorkspaceData Model and populate Result
		/// </summary>
		/// <param name="result">Response to send back to caller</param>
		/// <param name="data">Data to parse out</param>
		private void ParseAcvWorkspaceData(IESResponse<AcvWorkspaceData> result, ICollection<(int Id, string shortName, string longName, bool containsOCI, string ptmTrackingNumber)> data)
		{
			List<AcvWorkspaceData> acvWorkspaces = data.Select(x => new AcvWorkspaceData() { Id = x.Id, ShortName = x.shortName, LongName = x.longName, PtmTrackingNumber = x.ptmTrackingNumber, IsCurrentWorkspace = true }).ToList();

			foreach ((int Id, string shortName, string longName, bool containsOCI, string ptmTrackingNumber) workspace in data)
			{
				if (workspace.containsOCI)
				{
					// do permission check if OCI
					// permission check throws exceptions so we need to catch them and handle
					FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace.shortName);
					try
					{
						SecurityAuthorization permission = this.CheckPermission(SecurityPage.WorkspaceHome, ws);

						if (permission < SecurityAuthorization.Read)
						{
							acvWorkspaces.RemoveAll(x => x.Id == workspace.Id);
						}
					}
					catch (ValidationException)
					{
						acvWorkspaces.RemoveAll(x => x.Id == workspace.Id);
					}
					catch (UnauthorizedAccessException)
					{
						acvWorkspaces.RemoveAll(x => x.Id == workspace.Id);
					}
				}
			}

			result.Data = acvWorkspaces;
			result.IsSuccessful = true;
		}
	}
}