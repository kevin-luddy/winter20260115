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
	using System.Web.Http;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.PickList;

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
		/// <param name="traceTableExporter">Trace Table data exporter</param>
		/// <param name="boeFormControllerLogic">BOE Form Controller logic</param>
		/// <param name="contractTypeLoader">Pick List loader for Contract Types</param>
		public BoeDataAPIController(IWorkspaceDTODataLoader loader, TokenHandling tokenHandler, IReportsControllerLogic reportsControllerLogic, ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, IBOEExporter boeExporter, IBOECustomExporter boeCustomExporter, IWorkspaceExportFormatDTODataLoader workspaceExportFormatDTOLoader, ITraceTableExporter traceTableExporter, IBOEFormControllerLogic boeFormControllerLogic, IBOEFormPBOEDTODataLoader boeFormPBOEDTODataLoader, IActiveDirectoryUtilities activeDirectoryUtilities, IUserDTODataLoader userDataLoader, ContractTypeLoader contractTypeLoader)
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
			this.activeDirectoryUtilities = activeDirectoryUtilities;
			this.userDataLoader = userDataLoader;
			this.contractTypeLoader = contractTypeLoader;
		}
		#endregion

		/// <summary>
		/// Get Proposal data for ACV. Limits the number of records returned to 100.
		/// </summary>
		/// <param name="ptmTrackingNumber">PTM Tracking Number</param>
		/// <returns>Proposal Data</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<AcvWorkspaceData> GetWorkspaceDataForProposal(string ptmTrackingNumber)
		{
			IESResponse<AcvWorkspaceData> result = new IESResponse<AcvWorkspaceData>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				ICollection<(int Id, string shortName, string longName, bool containsOCI)> data = this.loader.GetWorkspaceDataForProposal(ptmTrackingNumber);
				List<AcvWorkspaceData> acvWorkspaces = data.Select(x => new AcvWorkspaceData() { Id = x.Id, ShortName = x.shortName, LongName = x.longName }).ToList();

				foreach ((int Id, string shortName, string longName, bool containsOCI) workspace in data)
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
		public HttpResponseMessage ExportAllBOEs(string workspaceShortName)
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
		[HttpGet]
		public IESResponse<NlfWorkspaceInnerData> GetAllWorkspaceInnerDataForNlf()
		{
			IESResponse<NlfWorkspaceInnerData> result = new IESResponse<NlfWorkspaceInnerData>();
			try
			{
				ICollection<NlfWorkspaceInnerDataDTO> boes = new Collection<NlfWorkspaceInnerDataDTO>();
				boes = loader.GetAllWorkspaceInnerDataForNlf();
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
		[HttpGet]
		public IESResponse<NlfWorkspaceInnerData> GetAllWorkspaceInnerDataByTrackingNumbersForNlf(ICollection<string> trackingNumbers)
		{
			IESResponse<NlfWorkspaceInnerData> result = new IESResponse<NlfWorkspaceInnerData>();
			try
			{
				string ntid = tokenHandler.AuthenticateUserFromAuthorizationToken();
				ICollection<NlfWorkspaceInnerDataDTO> boes = new Collection<NlfWorkspaceInnerDataDTO>();
				boes = loader.GetWorkspaceInnerDataByNtidForNlf(ntid, trackingNumbers);
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
				string ntid = tokenHandler.AuthenticateUserFromAuthorizationToken();

				// check if user is system admin
				IReadOnlyCollection<SecurityPermissionsResponse> permissions = this.Factory.GetPermissionsForUser(ntid);
				bool isSystemAdmin = permissions.Any(x => x.AuthorizedRole == Role.SystemAdmin);

				ICollection<NlfWorkspaceInnerDataDTO> boes = new Collection<NlfWorkspaceInnerDataDTO>();

				if (isSystemAdmin && workspaceId > 0)
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
				string ntid = tokenHandler.AuthenticateUserFromAuthorizationToken();

				// Check if user is System Admin
				IReadOnlyCollection<SecurityPermissionsResponse> permissions = this.Factory.GetPermissionsForUser(ntid);
				ICollection<WorkspaceDTO> workspaces = loader.GetWorkspacesByTrackingNumber(trackingNumber);
				bool isAllowed = permissions.Any(x => x.AuthorizedRole == Role.SystemAdmin || workspaces.Any(y => y.Id == x.WorkspaceId));

				if (isAllowed)
				{
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
				else
				{
					string message = "Invalid permission to Workspace with tracking number: " + trackingNumber + ".";
					logger.Error(message + " NTID: " + ntid);
					result.Messages.Add(message);
					result.IsSuccessful = false;
					result.Data = null;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occured returning Material PBoe Data for given Workspace with tracking number: {trackingNumber}: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Checks to see if workspace exists and user has authorization to it
		/// </summary>
		/// <param name="workspaceID">Workspace ID</param>
		/// <returns>Boolean</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<bool> CheckAuthorizationForWorkspaceID(int workspaceID)
		{

			IESResponse<bool> result = new IESResponse<bool>();

			try
			{
				string ntid = tokenHandler.AuthenticateUserFromAuthorizationToken();

				// Check if user is System Admin
				IReadOnlyCollection<SecurityPermissionsResponse> permissions = this.Factory.GetPermissionsForUser(ntid);
				bool isAllowed = permissions.Any(x => x.WorkspaceId == workspaceID || x.AuthorizedRole == Role.SystemAdmin);

				if (isAllowed)
				{
					result.Data = new Collection<bool>() { true };
					result.IsSuccessful = true;
				}
				else
				{
					result.Data = new Collection<bool>() { false };
					string message = "Invalid permission to Workspace with ID:" + workspaceID + ".";
					logger.Error(message + " NTID: " + ntid);
					result.Messages.Add(message);
					result.IsSuccessful = false;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occurred checking authorization for Workspace ID: {workspaceID}: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get all PBOEs for a given Workspace.
		/// </summary>
		/// <param name="workspaceID">Workspace ID</param>
		/// <returns>Collection of PBOEs by Workspace ID</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<PBOEData> GetPBOEsForTrackingNumber(string trackingNumber)
		{
			IESResponse<PBOEData> result = new IESResponse<PBOEData>();

			try
			{
				string ntid = tokenHandler.AuthenticateUserFromAuthorizationToken();

				// Check if user is System Admin
				IReadOnlyCollection<SecurityPermissionsResponse> permissions = this.Factory.GetPermissionsForUser(ntid);
				ICollection<WorkspaceDTO> workspaces = loader.GetWorkspacesByTrackingNumber(trackingNumber);
				bool isAllowed = permissions.Any(x => x.AuthorizedRole == Role.SystemAdmin || workspaces.Any(y => y.Id == x.WorkspaceId));

				if (isAllowed)
				{
					result.Data = boeFormPBOEDTODataLoader.GetPBOEsForWorkspace(trackingNumber).Select<PBOEDataDTO, PBOEData>(x =>
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
							IsCompetitionException = x.IsCompetitionException.GetValueOrDefault(),
							IsCommercialItemException = x.IsCommercialItemException.GetValueOrDefault(),
							IsCCoPDThresholdException = x.IsCCoPDThresholdException.GetValueOrDefault(),
							IsCCoPDOtherException = x.IsCCoPDOtherException.GetValueOrDefault(),
							ExpectedCCoPDApplicability = x.ExpectedCCoPDApplicability.GetDescription(),
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
							SupplierProposalManagerEmail = approver != null ? approver.Email : string.Empty
						};
					}).ToList();

					result.IsSuccessful = true;
				}
				else
				{
					string message = "Invalid permission to Workspace with tracking number:" + trackingNumber + ".";
					logger.Error(message + " NTID: " + ntid);
					result.Messages.Add(message);
					result.IsSuccessful = false;
					result.Data = null;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occured returning Material PBoe Data for given Workspace with tracking number: {trackingNumber}: {ex.Message}");
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
				string ntid = tokenHandler.AuthenticateUserFromAuthorizationToken();

				// Check if user is System Admin
				IReadOnlyCollection<SecurityPermissionsResponse> permissions = this.Factory.GetPermissionsForUser(ntid);
				ICollection<WorkspaceDTO> workspaces = loader.GetWorkspacesByTrackingNumber(trackingNumber);
				bool isAllowed = permissions.Any(x => x.AuthorizedRole == Role.SystemAdmin || workspaces.Any(y => y.Id == x.WorkspaceId));

				if (isAllowed)
				{
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
							SupplierProposalManagerEmail = approver != null ? approver.Email : string.Empty
						};
					}).ToList();

					result.IsSuccessful = true;
				}
				else
				{
					string message = $"Invalid permission to Workspace with tracking number: {trackingNumber} and PBOE ID: {pboeID}.";
					logger.Error(message + " NTID: " + ntid);
					result.Messages.Add(message);
					result.IsSuccessful = false;
					result.Data = null;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown Error occured returning PBoe Data for given Workspace with tracking number: {trackingNumber} and PBOE ID: {pboeID}: {ex.Message}");
			}

			return result;
		}
	}
}