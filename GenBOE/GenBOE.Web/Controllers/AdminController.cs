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
	using System.Data.Entity.Core;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Transactions;
	using System.Web.Mvc;
	using System.Web.Script.Serialization;
	using ActionLogic.ModelView;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Search;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.Metrics;
	using GenBOE.ActionLogic.ModelView.Admin;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.Reference;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.OfficeUtilities;

	/// <summary>
	/// Implementation note: 
	/// 
	/// Since our code is not calling these methods directly, all logging should be on the Info level instead of the Debug level.
	/// This is true ONLY for this controller.
	/// </summary>
	public class AdminController : GenBOEController
    {
        private readonly Logger _log = new Logger(typeof(AdminController));
        private readonly PermissionControllerLogic _PermissionControllerLogic;
        private readonly IActiveDirectoryUtilities _ActiveDirectoryUtilities;
        private readonly ISecurityInformation _SecurityInformation;
        private readonly ResourceDTODataLoader _ResourceDTODataLoader;
        private readonly IResourceListDTODataLoader _ResourceListDTODataLoader;
        private readonly PerformingOrgListDTODataLoader _PerformingOrgListDTODataLoader;
        private readonly IWorkspaceExportFormatDTODataLoader _WorkspaceExportFormatDTOLoader;
        private readonly MiscTravelRateDTOLoader miscTravelRateDTOLoader;
        private readonly IEscalationRatesDTOLoader escalationRatesDTOLoader;
        private readonly IOffloadRatesDTOLoader offloadRatesDTOLoader;
        private readonly IMileReimbursementRateDTOLoader _MileReimbursementRateDTOLoader;
        private readonly TripDTODataLoader _TripDTODataLoader;
        private readonly TripsExporter _TripsExporter;
        private readonly TripsImporter _TripsImporter;
        private readonly PerDiemDTODataLoader _PerDiemDTODataLoader;
        private readonly LocationDTODataLoader _LocationDTODataLoader;
        private readonly ResourcesImporter _ResourcesImporter;
        private readonly PerformingOrgImporter _PerformingOrgImporter;
        private readonly InUseDataLoader _InUseDataLoader;
        private readonly PackageUtilities _PackageUtilities;
        private readonly IAdminControllerLogic _ControllerLogic = null;
        private const string TRAVEL_ORIGINS_EXPORT_TEMPLATE = "~/Templates/Export/TravelOrigins.xlsx";
        private const string SYSTEM_ESCALATION_RATES_EXPORT_TEMPLATE = "~/Templates/Export/EscalationRatesRMS.xlsx";
        private const string SYSTEM_OFFLOAD_RATES_EXPORT_TEMPLATE = "~/Templates/Export/OffloadRatesRMS.xlsx";
        private readonly IWorkspaceDTODataLoader workspaceLoader;
        private readonly IPerformingOrgDTODataLoader perfOrgLoader;
        private readonly IUserDTODataLoader userDataLoader;
        private readonly ISystemSettingDTODataLoader systemSettingLoader;
        private readonly TrainingImporter trainingImporter = new TrainingImporter();
        private readonly IProPricerDTODataLoader proPricerDTODataLoader;
        private readonly ITokenService tokenService;

        /// <summary>
        /// Create static Regex object for FileFormat.
        /// </summary>
        private static readonly Regex regexFileFormat = new Regex(@"^.+\.docx?$", RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityAccess">Access to the security APIs</param>
        /// <param name="inWorkspaceMapper">Workspace APIs for manipulating workspaces</param>
        /// <param name="inCache"></param>
        /// <param name="inIUserDTODataLoader">Load user info, primarily email address for our needs</param>
        public AdminController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper,
            SiteMasterUtilities inSiteMasterUtilities,
            IWorkspaceExportFormatDTODataLoader inIWorkspaceExportFormatDTOLoader,
            UserDTODataLoader inIUserDTODataLoader,
            IPermissionsDTODataLoader inIPermissionsDTOLoader,
            PermissionControllerLogic inPermissionControllerLogic,
            IActiveDirectoryUtilities inIActiveDirectoryUtilities,
            ISecurityInformation inISecurityInformation,
            ResourceDTODataLoader inIResourceDTODataLoader,
            IResourceListDTODataLoader inIResourceListDTODataLoader,
            PerformingOrgListDTODataLoader inIPerformingOrgListDTODataLoader,
            MiscTravelRateDTOLoader inIMiscTravelRateDTOLoader,
            IEscalationRatesDTOLoader inEscalationRatesDTOLoader,
            IOffloadRatesDTOLoader inOffloadRatesDTOLoader,
            IMileReimbursementRateDTOLoader inIMileReimbursementRateDTOLoader,
            TripDTODataLoader inITripDTODataLoader,
            TripsExporter inTripsExporer,
            TripsImporter inTripsImporter,
            PerDiemDTODataLoader inIPerDiemDTODataLoader,
            LocationDTODataLoader inILocationDTODataLoader,
            ResourcesImporter inResourcesImporter,
            PerformingOrgImporter inPerformingOrgImporter,
            SystemMetrics inSystemMetrics,
            InUseDataLoader InUseDataLoader,
            IFullObjectFactory factory,
            IWorkspaceDTODataLoader workspaceLoader,
            IAdminControllerLogic inAdminControllerLogic,
            PackageUtilities inPackageUtilities,
            IPerformingOrgDTODataLoader perfOrgLoader,
            IGenBOEControllerLogic inControllerLogic,
            ISystemSettingDTODataLoader systemSettingLoader,
            IProPricerDTODataLoader proPricerDTODataLoader,
            ITokenService tokenService)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inIUserDTODataLoader, inIPermissionsDTOLoader, inControllerLogic)
        {
			this._WorkspaceExportFormatDTOLoader = inIWorkspaceExportFormatDTOLoader;
			this._PermissionControllerLogic = inPermissionControllerLogic;
			this._ActiveDirectoryUtilities = inIActiveDirectoryUtilities;
			this._SecurityInformation = inISecurityInformation;
			this._ResourceDTODataLoader = inIResourceDTODataLoader;
			this._PerformingOrgListDTODataLoader = inIPerformingOrgListDTODataLoader;
			this._ResourceListDTODataLoader = inIResourceListDTODataLoader;
            this.miscTravelRateDTOLoader = inIMiscTravelRateDTOLoader;
            this.escalationRatesDTOLoader = inEscalationRatesDTOLoader;
            this.offloadRatesDTOLoader = inOffloadRatesDTOLoader;
			this._MileReimbursementRateDTOLoader = inIMileReimbursementRateDTOLoader;
			this._TripDTODataLoader = inITripDTODataLoader;
			this._TripsExporter = inTripsExporer;
			this._TripsImporter = inTripsImporter;
			this._PerDiemDTODataLoader = inIPerDiemDTODataLoader;
			this._LocationDTODataLoader = inILocationDTODataLoader;
			this._ResourcesImporter = inResourcesImporter;
			this._PerformingOrgImporter = inPerformingOrgImporter;
			this._InUseDataLoader = InUseDataLoader;
            this.workspaceLoader = workspaceLoader;
			this._PackageUtilities = inPackageUtilities;
			this._ControllerLogic = inAdminControllerLogic;
            this.perfOrgLoader = perfOrgLoader;
            this.userDataLoader = inIUserDTODataLoader;
            this.systemSettingLoader = systemSettingLoader;
            this.proPricerDTODataLoader = proPricerDTODataLoader;
            this.tokenService = tokenService;
        }

        #region display

        /// <summary>
        /// My Info View for an Admin
        /// </summary>
        /// <returns>My Info view for an Admin</returns>
		[HttpGet]
        public async Task<ViewResult> MyInfo()
        {
            // Action Init
            Stopwatch sw = InitializeAction(_log, WebConstants.VIEW_SYSTEM_ADMIN_INFO, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Create Model
            UserDTO user = this.UserLoader.GetUserForActiveUser();
            Token token = await this.tokenService.GetToken();
            MyInfoModelView myInfoModel = new MyInfoModelView()
            {
                DisplayName = user.DisplayName,
                NtId = user.NTID,
                BearerToken = token.AccessToken
            };

            // Perform Action
            ViewResult toReturn = View(WebConstants.VIEW_SYSTEM_ADMIN_INFO, myInfoModel);

            // Action Finalize
            FinalizeAction(_log, WebConstants.VIEW_SYSTEM_ADMIN_INFO, sw);

            return toReturn;
        }

        /// <summary>
        /// Displays the System Admin View
        /// </summary>
        /// <returns>View</returns>
		[HttpGet]
        public ViewResult SystemAdmin()
        {
            // Action Init
            Stopwatch sw = InitializeAction(_log, "SystemAdmin", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Perform Action
            ViewResult toReturn = View(WebConstants.VIEW_SYSTEM_ADMIN);

            // Action Finalize
            FinalizeAction(_log, "SystemAdmin", sw);

            return toReturn;
        }

		/// <summary>
		/// Displays the System Admin Jump Page
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ViewResult DisplaySystemAdminJump()
        {
            // Action Initialize
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_SYSTEM_ADMIN_JUMP, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ViewResult toReturn = View(WebConstants.VIEW_SYSTEM_ADMIN_JUMP);

            // Action Finalize
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_SYSTEM_ADMIN_JUMP, sw);

            return toReturn;
        }

		/// <summary>
		/// Displays the System Admin Jump Page
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ViewResult DisplaySystemPermissions()
        {
            // Action Initialize
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_SYSTEM_PERMISSIONS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            Collection<PermissionsGridModelView> permissions = _GetPermissionsGrid(Role.SystemAdmin);


            ViewResult toReturn = View(WebConstants.VIEW_SYSTEM_PERMISSIONS_GRID, permissions);

            // Action Finalize
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_SYSTEM_PERMISSIONS, sw);

            return toReturn;
        }

        ///<summary>
        ///Get members of group
        ///</summary>
        ///<param name="groupName">Name of the group to get users from</param>
        [HttpPost]
		public JsonResult GetGroupMembers(string groupName)
        {
            Collection<string> memberList = new Collection<string>();
            ICollection<UserData> members = _ActiveDirectoryUtilities.GetAdGroupUsers(groupName);
            ICollection<UserData> orderedMembers = members.OrderBy(m => m.DisplayName).ToList();
            foreach (UserData member in orderedMembers)
            {
                memberList.Add(member.DisplayName);
            }
            JsonResult toReturn;
            toReturn = Json(memberList);
            return toReturn;
        }

		/// <summary>
		/// Displays the Create Workspace Permissions Page
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ViewResult DisplayCreateWorkspacePermissions()
        {
            // Action Initialize
            Stopwatch sw = InitializeAction(_log, "DisplayCreateWorkspacePermissions", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            Collection<PermissionsGridModelView> permissions = _GetPermissionsGrid(Role.CreateWorkspacePermissions);

            ViewResult toReturn = View(WebConstants.VIEW_CREATE_WORKSPACE_PERMISSIONS_GRID, permissions);

            // Action Finalize
            FinalizeAction(_log, "DisplayCreateWorkspacePermissions", sw);

            return toReturn;
        }

        /// <summary>
        /// Archive an output format template
        /// </summary>
        /// <param name="templateId">id to delete</param>
        /// <param name="updateDate">Update date</param>
        /// <returns>json success result</returns>
		[HttpPost]
		public JsonResult ArchiveOutputFormatTemplate(int templateId, long updateDate)
        {
            Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_ARCHIVE_OUTPUT_FORMAT_TEMPLATE, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);
            JsonResult toReturn = this.Json(new { Success = true });

            WorkspaceExportFormatDTO template = this._WorkspaceExportFormatDTOLoader.GetById(templateId);
            template.UpdateDate = new DateTime(updateDate);
            template.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this._WorkspaceExportFormatDTOLoader.Save(template);
                scope.Complete();
            }
                        
            this.FinalizeAction(this._log, WebConstants.ACTION_ARCHIVE_OUTPUT_FORMAT_TEMPLATE, sw);

            return toReturn;
        }

		/// <summary>
		/// Restore an archived output format template
		/// </summary>
		/// <param name="templateId">Template ID</param>
		/// <param name="updateDate">Update Date</param>
		/// <returns>json success result</returns>
		[HttpPost]
		public JsonResult RestoreOutputFormatTemplate(int templateId, long updateDate)
        {
            Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_RESTORE_OUTPUT_FORMAT_TEMPLATE, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);
            JsonResult toReturn = this.Json(new { Success = true });

            WorkspaceExportFormatDTO template = this._WorkspaceExportFormatDTOLoader.GetById(templateId);
            template.UpdateDate = new DateTime(updateDate);

            this._WorkspaceExportFormatDTOLoader.RestoreTemplate(template);

            this.FinalizeAction(this._log, WebConstants.ACTION_RESTORE_OUTPUT_FORMAT_TEMPLATE, sw);

            return toReturn;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HttpPost]
		public ContentResult EditOutputFormatTemplate(string templateName, string templateDescription, int templateId, long updateDate)
        {
            Stopwatch sw = InitializeAction(_log, "EditOutputFormatTemplate", SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);
            
            ContentResult toReturn = null;
            
            if (ModelState.IsValid)
            {
                // Perform validation shared between edit and save
                Collection<ValidationMessage> ValidationErrors = this._ControllerLogic.ValidateTemplateOnSaveOrEdit(templateName, templateDescription);

                // Perform validation unique to edit
                if (templateId <= 0)
                {
                    ValidationErrors.Add(new ValidationMessage("Template Id is required."));
                }

                if (Request.Files.Count != 0 && Request.Files[0].ContentLength != 0 && !regexFileFormat.IsMatch(Request.Files[0].FileName))
                {
                    ValidationErrors.Add(new ValidationMessage("Template File must be in .doc or .docx file format."));
                }
                
                if (ValidationErrors.Any())
                {
                    toReturn = GenerateUploadResponse(ValidationErrors);
                }
                else
                {
                    try
                    {
                        WorkspaceExportFormatDTO exportFormat = _WorkspaceExportFormatDTOLoader.GetById(templateId);
                        exportFormat.ExportFormatName = templateName;
                        exportFormat.ExportFormatDescription = templateDescription;
                        exportFormat.Updateable = UpdateType.Upsert;
                        exportFormat.UpdateDate = new DateTime(updateDate);
                        if (Request.Files.Count != 0 && Request.Files[0].ContentLength != 0)
                        {
                            exportFormat.ExportFormat.ParentTemplateId = _PackageUtilities.DetermineParentTemplateId(Request.Files[0].InputStream);
                            using (MemoryStream ms = new MemoryStream())
                            {
                                Request.Files[0].InputStream.Position = 0;
                                Request.Files[0].InputStream.CopyTo(ms);
                                exportFormat.FileData = ms.ToArray();
                            }
                        }

                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                        {
                            _WorkspaceExportFormatDTOLoader.Save(exportFormat);
                            scope.Complete();
                        }

                        if (System.IO.File.Exists(exportFormat.PhysicalFilePathCache))
                        { 
                            System.IO.File.Delete(exportFormat.PhysicalFilePathCache); 
                        }

                        toReturn = GenerateUploadResponse(true, "Edit Export Format Complete");
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "Unknown Edit Export Template Resources Error.");
                        if (!String.IsNullOrEmpty(ex.InnerException.Message))
                        {
                            //print the error message from the db (i.e. duplicate name or out of sync data)
                            toReturn = GenerateUploadResponse(false, ex.InnerException.Message);
                        }
                        else
                        {
                            toReturn = GenerateUploadResponse(false, "A general error occurred.  Please retry.");
                        }
                    }
                }
            }
            else
            {
                toReturn = GenerateUploadResponse(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            FinalizeAction(_log, "EditOutputFormatTemplate", sw);

            return toReturn;
        }

        /// <summary>
        /// Save the new output format template for use by workspaces
        /// </summary>
        /// <param name="TemplateName">the template name</param>
        /// <param name="TemplateDescription">the template description</param>
        /// <returns>success or failure to be interpreted by iframe code</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HttpPost]
		public ContentResult SaveNewOutputFormatTemplate(string TemplateName, string TemplateDescription)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_WORKSPACE_OUTPUT_FORMAT_NEW_TEMPLATE, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);
            
            ContentResult toReturn = null;

            if (ModelState.IsValid)
            {
                // Perform validation shared between edit and save
                Collection<ValidationMessage> ValidationErrors = this._ControllerLogic.ValidateTemplateOnSaveOrEdit(TemplateName, TemplateDescription);
                
                // Perform validation unique to save
                if (Request.Files.Count == 0 || Request.Files[0].ContentLength == 0)
                {
                    ValidationErrors.Add(new ValidationMessage("A Template File is required."));
                }
                else if (!regexFileFormat.IsMatch(Request.Files[0].FileName))
                {
                    ValidationErrors.Add(new ValidationMessage("Template File must be in .doc or .docx file format."));
                }

                if (ValidationErrors.Any())
                {
                    toReturn = GenerateUploadResponse(ValidationErrors);
                }
                else
                {
                    try
                    {
                        // If a file was uploaded successfully then push the entire record into the database (including the file as a byte[]
                        if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                Request.Files[0].InputStream.Position = 0;
                                Request.Files[0].InputStream.CopyTo(ms);

                                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                                {
                                    _WorkspaceExportFormatDTOLoader.Save(new WorkspaceExportFormatDTO
                                    {
                                        ExportFormatDescription = TemplateDescription,
                                        ExportFormatName = TemplateName,
                                        ExportFormat = new ExcelReportTemplate((int)ExcelReportTemplateType.NotSet, _PackageUtilities.DetermineParentTemplateId(Request.Files[0].InputStream)),
                                        FileData = ms.ToArray(),
                                        Updateable = UpdateType.Upsert
                                    });

                                    scope.Complete();
                                }
                            }
                        }

                        toReturn = GenerateUploadResponse(true, "Add New Export Format Complete");
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "Unknown Add Export Template Resources Error.");
                        if (!String.IsNullOrEmpty(ex.InnerException.Message))
                        {
                            //print the error message from the db (i.e. duplicate name)
                            toReturn = GenerateUploadResponse(false, ex.InnerException.Message);
                        }
                        else
                        {
                            toReturn = GenerateUploadResponse(false, "A general error occurred.  Please retry.");
                        }
                    }
                }
            }
            else
            {
                toReturn = GenerateUploadResponse(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            FinalizeAction(_log, WebConstants.ACTION_SAVE_WORKSPACE_OUTPUT_FORMAT_NEW_TEMPLATE, sw);

            return toReturn;
        }

		/// <summary>
		/// Get the output format template as a word document content type
		/// </summary>
		/// <param name="id">the template id from the database</param>
		/// <returns>The word file, complete with correct content type</returns>
		[HttpGet]
		public FileContentResult GetOutputFormatTemplate(int? id)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_GET_OUTPUT_FORMAT_TEMPLATE, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            FileContentResult toReturn = null;

            WorkspaceExportFormatDTO template = _WorkspaceExportFormatDTOLoader.GetById(id.Value);

            // Return the template as a download for the user
            using (MemoryStream mem = _PackageUtilities.UpdateDocumentVersion(template.FileData, template.PhysicalFilePathCache, template.ExportFormat))
            {
                toReturn = new FileContentResult(mem.ToArray(), ExportFileDownloadBase.ContentType_DOCX);
                toReturn.FileDownloadName = template.ExportFormatName + ".docx";
            }

            FinalizeAction(_log, WebConstants.ACTION_GET_OUTPUT_FORMAT_TEMPLATE, sw);

            return toReturn;
        }

		/// <summary>
		/// Displays the System Admin Manage Output Template
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ViewResult DisplayManageOutputFormatTemplates()
        {
            // Action Initialize
            Stopwatch sw = InitializeAction(_log, "DisplayManageOutputFormatTemplates", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_OUTPUT_TEMPLATES);

            // Action Finalize
            FinalizeAction(_log, "DisplayManageOutputFormatTemplates", sw);

            return toReturn;
        }

		/// <summary>
		/// Displays the partial view for the default resources page
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ViewResult DisplayDefaultResources()
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_DEFAULT_RESOURCES, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            DefaultResourcesModelView theModelView = this._ControllerLogic.DisplayDefaultResourcesLogic();

			ViewData["RateTypes"] = theModelView.rateTypes;
			ViewData["ElementOfCostTypes"] = theModelView.elementOfCostTypes;
            
            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_DEFAULT_RESOURCES, theModelView);

            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_DEFAULT_RESOURCES, sw);
            return toReturn;
        }

		/// <summary>
		/// Displays the partial view for the Manage Misc Rates page
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ViewResult DisplayManageMiscRates()
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_MISC_RATES, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Initialize modelView to return
            MiscRateModelView theModelView = new MiscRateModelView();

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_MISC_RATES, theModelView);

            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_MISC_RATES, sw);
            return toReturn;
        }

		/// <summary>
		/// Displays the partial view for the Manage Misc Rates page
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ViewResult DisplayManageMiscRatesGrid()
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_MISC_RATES_GRID, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Initialize modelView to return
            ManageMiscRatesGridModelView theModelView = new ManageMiscRatesGridModelView();

            // Get the rates list
            Collection<MiscTravelRateDTO> ratesForMV = new Collection<MiscTravelRateDTO>();


            ratesForMV = miscTravelRateDTOLoader.GetAll();
            if (ratesForMV != null)
            {
                Collection<MiscTravelRateDTO> SortedRatesForMV = new Collection<MiscTravelRateDTO>((from r in ratesForMV
		            orderby r.SortCode
		            select r).ToArray());


                // Create the modelview all the rates
                foreach (MiscTravelRateDTO rateDTO in SortedRatesForMV)
                {
                    if (!rateDTO.lockedRate)
                    {
                        MiscRateModelView temp = new MiscRateModelView(rateDTO);
                        theModelView.MiscRateModelView.Add(temp);
                    }

                }
            }

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_MISC_RATES_GRID, theModelView);

            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_MISC_RATES_GRID, sw);
            return toReturn;
        }

		/// <summary>
		/// Displays the partial view for the Trips for travel page
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ViewResult DisplayManageTripsForTravel()
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_TRIPS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            Collection<SelectListItem> modeSelects = new Collection<SelectListItem>();

            IEnumerable<MiscTravelRateDTO> miscTravelRates = miscTravelRateDTOLoader.GetAll().Where(x=>x.lockedRate != true);

            MiscTravelRateDTO[] miscTravelRateSorted = (from r in miscTravelRates
                                        orderby r.MiscTravelRateMode
                                        select r).ToArray();
            foreach (MiscTravelRateDTO miscTravelRate in miscTravelRateSorted)
            {
                modeSelects.Add(new SelectListItem() { Text = miscTravelRate.MiscTravelRateMode, Value = miscTravelRate.Id.ToString() });

            }
            ViewData["Modes"] = modeSelects;

            // Initialize modelView to return
            TripForTravelModelView theModelView = new TripForTravelModelView();

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_TRIPS, theModelView);

            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_TRIPS, sw);
            return toReturn;
        }

		/// <summary>
		/// Displays the partial view for the Manage Trips for travel page
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ViewResult DisplayManageTripsForTravelGrid(
            string tripIDSearchText,
            string departureSearchText,
            string destinationSearchText,
            string perDiemDestSearchText,
            string modeSearchText,
            string qualificationSearchText,
            string departureCodeSearchText,
            string destinationCodeSearchText,
            string sortField, string sortDirection)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_TRIPS_GRID, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ManageTripsForTravelGridModelView theModelView =
                this._GetManageTripsForTravelGridData(
                tripIDSearchText,
                departureSearchText,
                destinationSearchText,
                perDiemDestSearchText,
                modeSearchText,
                qualificationSearchText,
                departureCodeSearchText,
                destinationCodeSearchText);

            this._SortManageTripsForTravel(theModelView, sortField, sortDirection);

            // store the paged results according to sorted order
            theModelView.PagedIndexes = new Collection<int>(theModelView.TripsForTravelCollection.Select(x => x.TripID).ToArray());

            // trim to match page size
            theModelView.TripsForTravelCollection = theModelView.TripsForTravelCollection.Take(theModelView.ResultsPerPage).ToArray();

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_TRIPS_GRID, theModelView);

            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_TRIPS_GRID, sw);
            return toReturn;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private ManageTripsForTravelGridModelView _SortManageTripsForTravel(ManageTripsForTravelGridModelView theModelView, string inSortField, string inSortDirection)
        {
            ViewData["SortField"] = inSortField;
            ViewData["SortDirection"] = inSortDirection;

            const string SORT_TRIPID = "tripid";
            const string SORT_DEPARTURE = "departure";
            const string SORT_DESTINATION = "destination";
            const string SORT_PERDIEMDEST = "perdiemdest";
            const string SORT_MODE = "mode";
            const string SORT_QUALIFICATION = "qualification";
            const string SORT_FARE = "fare";
            const string SORT_HOTEL = "hotel";
            const string SORT_MIE = "mie";
            const string SORT_CAR = "car";
            const string SORT_MISC = "misc";
            const string SORT_DEPARTURECODE = "departurecode";
            const string SORT_DESTINATIONCODE = "destinationcode";

            const string DIRECTION_ASCENDING = "Asc";
            const string DIRECTION_DESCENDING = "Desc";

            if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_TRIPID)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.TripID
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.TripID descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_DEPARTURE)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.DepartureLocationName
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.DepartureLocationName descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_DESTINATION)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.DestinationLocationName
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.DestinationLocationName descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_PERDIEMDEST)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.PerDiemDestination
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.PerDiemDestination descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_MODE)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.Mode
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.Mode descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_QUALIFICATION)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.Qualification
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.Qualification descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_FARE)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.Fare
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.Fare descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_HOTEL)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.HotelRate
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.HotelRate descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_MIE)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.MIERate
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.MIERate descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_CAR)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.RentalCar
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.RentalCar descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_MISC)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.MiscRate
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.MiscRate descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_DEPARTURECODE)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.DepartureLocationCode
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.DepartureLocationCode descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else if (string.IsNullOrEmpty(inSortField) || inSortField == SORT_DESTINATIONCODE)
            {
                if (string.IsNullOrEmpty(inSortDirection) || inSortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.DestinationLocationCode
		            select x).ToArray());
                }
                else if (inSortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.TripsForTravelCollection = new Collection<TripForTravelModelView>((from x in theModelView.TripsForTravelCollection
		            orderby x.DestinationLocationCode descending
		            select x).ToArray());
                }
                else
                {
                    throw new ValidationException("Invalid sort direction, unable to sort Manage Trips for Travel.");
                }
            }
            else
            {
                throw new ValidationException("Invalid sort field, unable to sort Manage Trips for Travel.");
            }

            return theModelView;
        }


        private ManageTripsForTravelGridModelView _GetManageTripsForTravelGridData(
            string inTripIDSearchText,
            string inDepartureSearchText,
            string inDestinationSearchText,
            string inPerDiemDestSearchText,
            string inModeSearchText,
            string inQualificationSearchText,
            string inDepartureCodeSearchText,
            string inDestinationCodeSearchText)
        {
            ManageTripsForTravelGridModelView theModelView = new ManageTripsForTravelGridModelView();

            ICollection<TripDTO> allTripDTOs = _TripDTODataLoader.GetAllTrips();
            HashSet<LocationDTO> allDepartureLocations = new HashSet<LocationDTO>(_LocationDTODataLoader.GetByIds(allTripDTOs.Select(i => i.DepartureLocationID).ToCollection<int>()));
            HashSet<LocationDTO> allDestinationLocations = new HashSet<LocationDTO>(_LocationDTODataLoader.GetByIds(allTripDTOs.Select(i => i.DestinationLocationID).ToCollection<int>()));
            HashSet<MiscTravelRateDTO> allMisTravelRates = new HashSet<MiscTravelRateDTO>(miscTravelRateDTOLoader.GetByIds(allTripDTOs.Select(i => i.MiscTravelRateID).ToCollection<int>()));
            HashSet<PerDiemDTO> allPerdiems = new HashSet<PerDiemDTO>(_PerDiemDTODataLoader.GetByIds(allTripDTOs.Select(i => i.PerDiemID).ToCollection<int>()));
            HashSet<UserDTO> allUpdatingUsers = new HashSet<UserDTO>(userDataLoader.GetByIds(allTripDTOs.Select(i => i.FareUpdatedByUserID).ToCollection<int>()));

            foreach (TripDTO dto in allTripDTOs)
            {
                PerDiemDTO perDiemDTO = allPerdiems.First(i => i.Id == dto.PerDiemID);
                MiscTravelRateDTO miscDTO = allMisTravelRates.First(i => i.Id == dto.MiscTravelRateID);
                LocationDTO departure = allDepartureLocations.FirstOrDefault(i => i.Id == dto.DepartureLocationID);
                LocationDTO destination = allDestinationLocations.FirstOrDefault(i => i.Id == dto.DestinationLocationID);
                UserDTO fareUpdatedByUser = allUpdatingUsers.FirstOrDefault(i => i.UserID == dto.FareUpdatedByUserID);

                if (!dto.LockedRate)
                {
                    // check for at least 1 search criteria being valued
                    if (!string.IsNullOrEmpty(inTripIDSearchText) ||
                        !string.IsNullOrEmpty(inDepartureSearchText) ||
                        !string.IsNullOrEmpty(inDestinationSearchText) ||
                        !string.IsNullOrEmpty(inPerDiemDestSearchText) ||
                        !string.IsNullOrEmpty(inModeSearchText) ||
                        !string.IsNullOrEmpty(inQualificationSearchText) ||
                        !string.IsNullOrEmpty(inDepartureCodeSearchText) || 
                        !string.IsNullOrEmpty(inDestinationCodeSearchText))
                    {
                        string mode = miscDTO.MiscTravelRateMode;
                        string qualification = perDiemDTO.Qualification;

                        // apply search criterias
                        if (string.IsNullOrEmpty(inTripIDSearchText) || dto.TripID.ToString().ToLower().Contains(inTripIDSearchText.ToLower().Trim()))
                        {
                            if (string.IsNullOrEmpty(inDepartureSearchText) || ((departure != null) && !string.IsNullOrEmpty(departure.LocationName) && departure.LocationName.ToLower().Contains(inDepartureSearchText.ToLower().Trim())))
                            {
                                if (string.IsNullOrEmpty(inDestinationSearchText) || ((destination != null) && !string.IsNullOrEmpty(destination.LocationName) && destination.LocationName.ToLower().Contains(inDestinationSearchText.ToLower().Trim())))
                                {
                                    if (string.IsNullOrEmpty(inPerDiemDestSearchText) || !string.IsNullOrEmpty(perDiemDTO.PerDiemDestination) && perDiemDTO.PerDiemDestination.ToLower().Contains(inPerDiemDestSearchText.ToLower().Trim()))
                                    {
                                        if (string.IsNullOrEmpty(inModeSearchText) || !string.IsNullOrEmpty(mode) && mode.ToLower().Contains(inModeSearchText.ToLower().Trim()))
                                        {
	if (string.IsNullOrEmpty(inQualificationSearchText) || !string.IsNullOrEmpty(qualification) && qualification.ToLower().Contains(inQualificationSearchText.ToLower().Trim()))
	{
	    if (string.IsNullOrEmpty(inDepartureCodeSearchText) || !string.IsNullOrEmpty(dto.DepartureLocationCode) && dto.DepartureLocationCode.ToLower().Contains(inDepartureCodeSearchText.ToLower().Trim()))
	    {
	        if (string.IsNullOrEmpty(inDestinationCodeSearchText) || !string.IsNullOrEmpty(dto.DestinationLocationCode) && dto.DestinationLocationCode.ToLower().Contains(inDestinationCodeSearchText.ToLower().Trim()))
	        {
	            theModelView.PagedIndexes.Add(dto.TripID);

	            TripForTravelModelView result = new TripForTravelModelView(dto,
	                miscDTO, fareUpdatedByUser, perDiemDTO, departure, destination);
	            theModelView.TripsForTravelCollection.Add(result);
	        }
	    }
	}
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // no search criteria
                        theModelView.PagedIndexes.Add(dto.TripID);

                        TripForTravelModelView result = new TripForTravelModelView(dto, miscDTO, fareUpdatedByUser, perDiemDTO, departure, destination);
                        theModelView.TripsForTravelCollection.Add(result);
                    }
                }
            }

            return theModelView;
        }

        /// <summary>
        /// Displays the Manage Zone Travel Origins Page
        /// </summary>
        /// <returns></returns>
        public ViewResult DisplayManageZoneTravel()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayZoneTravel", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_ZONE_TRAVEL);

            FinalizeAction(_log, "DisplayZoneTravel", sw);

            return toReturn;
        }

        /// <summary>
        /// Displays the Grid for the Manage Zone Travel Origins Page
        /// </summary>
        /// <returns></returns>
        public ViewResult DisplayManageZoneTravelGrid()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayManageZoneTravelGrid", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ManageMSTZoneTravelOriginsGridModelView theModelView = _ControllerLogic.GetZoneTravelOriginGridData();

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_ZONE_TRAVEL_GRID, theModelView);

            FinalizeAction(_log, "DisplayManageZoneTravelGrid", sw);

            return toReturn;
        }
        
        /// <summary>
        /// Displays the Manage Zone Travel Destinations Page
        /// </summary>
        /// <returns></returns>
        public ViewResult DisplayManageZoneTravelDestinations()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayZoneTravelDestinations", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            Collection<SelectListItem> zoneSelects = new Collection<SelectListItem>();
            zoneSelects.Add(new SelectListItem() { Text = "1", Value = "1" });
            zoneSelects.Add(new SelectListItem() { Text = "2", Value = "2" });
            zoneSelects.Add(new SelectListItem() { Text = "3", Value = "3" });
            zoneSelects.Add(new SelectListItem() { Text = "4", Value = "4" });
            zoneSelects.Add(new SelectListItem() { Text = "5", Value = "5" });
            zoneSelects.Add(new SelectListItem() { Text = "6", Value = "6" });
            ViewData["Zones"] = zoneSelects;

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_ZONE_TRAVEL_DESTINATIONS);

            FinalizeAction(_log, "DisplayZoneTravelDestinations", sw);

            return toReturn;
        }

        /// <summary>
        /// Displays the grid for the Manage Zone Travel Destinations Page
        /// </summary>
        /// <returns></returns>
        public ViewResult DisplayManageZoneTravelDestinationsGrid()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayManageZoneTravelDestinationsGrid", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ManageMSTZoneTravelDestinationsGridModelView theModelView = _ControllerLogic.GetZoneTravelDestinationsGridData();

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_ZONE_TRAVEL_DESTINATIONS_GRID, theModelView);

            FinalizeAction(_log, "DisplayManageZoneTravelDestinationsGrid", sw);

            return toReturn;
        }

        /// <summary>
        /// Displays the Manage Fees and Costs for Nonzone Travel page
        /// </summary>
        /// <returns></returns>
        public ViewResult DisplayManageNonzoneFeesandCosts()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayManageNonzoneFeesandCosts", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_NONZONE_FEES_AND_COSTS);

            FinalizeAction(_log, "DisplayManageNonzoneFeesandCosts", sw);

            return toReturn;
        }

        /// <summary>
        /// Displays the grid for the Manage Fees and Costs for Nonzone Travel page
        /// </summary>
        /// <returns></returns>
        public ViewResult DisplayManageNonzoneFeesandCostsGrid()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayManageNonzoneFeesandCostsGrid", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            NonzoneFeesAndCostsGridModelView theModelView = _ControllerLogic.GetNonzoneFeesAndCostsGridData();

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_NONZONE_FEES_AND_COSTS_GRID, theModelView); 

            FinalizeAction(_log, "DisplayManageNonzoneFeesandCostsGrid", sw);

            return toReturn;
        }

        /// <summary>
        /// Displays the partial view for the Manage escalations rates page
        /// </summary>
        /// <returns></returns>
        public ViewResult DisplayManageEscalationRates()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayManageEscalationRates", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Initialize modelView to return
            EscalationRateModelView theModelView = new EscalationRateModelView();

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_ESCALATION_RATES, theModelView);

            FinalizeAction(_log, "DisplayManageEscalationRates", sw);
            return toReturn;
        }

        /// <summary>
        /// Displays the partial view for the Manage Escalation Rates page
        /// </summary>
        /// <returns></returns>
        public ViewResult DisplayManageEscalationRatesGrid()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayManageEscalationRatesGrid", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Initialize modelView to return
            ManageEscalationRatesGridModelView theModelView = new ManageEscalationRatesGridModelView();

            // Get the rates list
            ICollection<EscalationRatesDTO> ratesForMV = escalationRatesDTOLoader.GetAll();

            if (ratesForMV.Any())
            {

                Collection<EscalationRatesDTO> SortedRatesForMV = new Collection<EscalationRatesDTO>((from r in ratesForMV
		              orderby r.Year
		              select r).ToArray());
                // Create the modelview all the rates
                foreach (EscalationRatesDTO rateDTO in SortedRatesForMV)
                {
                    EscalationRateModelView temp = new EscalationRateModelView(rateDTO);
                    temp.DevEscalation = Math.Round(temp.DevEscalation, 3);
                    temp.LMSIEscalation = Math.Round(temp.LMSIEscalation, 3);
                    temp.MiscRate = Math.Round(temp.MiscRate, 3);

                    theModelView.EscalationRateModelViews.Add(temp);
                }
            }

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_ESCALATION_RATES_GRID, theModelView);

            FinalizeAction(_log, "DisplayManageEscalationRatesGrid", sw);
            return toReturn;
        }

        /// <summary>
        /// Displays the Manage Offloading Rates page
        /// </summary>
        /// <returns>Manage Offload Rates View.</returns>
        public ViewResult DisplayManageOffloadRates()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayManageOffloadRates", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);
            OffloadRatesModelView model = new OffloadRatesModelView();
            ICollection<ResourceDTO> allResources = _ResourceDTODataLoader.GetGlobalResources();
            model.Resources = allResources.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor && x.RateType == RateType.Hours).ToList();
            model.SubcontractorResources = allResources.Where(x => x.ElementOfCost == ElementOfCostType.Sub && x.RateType == RateType.Cost).ToList();
            model.PerformingOrgs = this.perfOrgLoader.GetGlobalPerformingOrgs();

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_OFFLOAD_RATES, model);

            FinalizeAction(_log, "DisplayManageOffloadRates", sw);

            return toReturn;
        }

        /// <summary>
        /// Displays the Manage Legacy Resources page
        /// </summary>
        /// <returns>Manage Legacy Resources View.</returns>
        public ViewResult DisplayManageLegacyResources()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayManageLegacyResources", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);
            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_LEGACY_RESOURCES, _CommonDataMapper.GetSikorskyLegacyResources());

            FinalizeAction(_log, "DisplayManageLegacyResources", sw);

            return toReturn;
        }

        /// <summary>
        /// Pages the system offload rates.
        /// </summary>
        /// <param name="inPagedResults">The incoming paged results.</param>
        /// <returns>The results after an action has occurred (new page, initial page, deletion/save).</returns>
        public ViewResult PageSystemOffloadRates(PagedSystemOffloadRateModelView inPagedResults)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_PAGE_SYSTEM_OFFLOAD_RATES, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Initialize modelView to return
            PagedSystemOffloadRateModelView theModelView = null;

            if (inPagedResults != null)
            {
                theModelView = inPagedResults;
            }
            else
            {
                theModelView = new PagedSystemOffloadRateModelView();
            }

            //// Get the rates list
            ICollection<OffloadRatesDTO> allDTOs = offloadRatesDTOLoader.GetAllSystemRates().OrderBy(o => o.Resource).ThenBy(p => p.PerformingOrg).ThenBy(y => y.Year).ToList();

            if (theModelView.PagedIndexes.None())
            {
                theModelView.PagedIndexes = new Collection<int>((from dto in allDTOs
	                     select dto.Id).ToList<int>());
            }

            theModelView.Results.Clear();

            if (allDTOs.Any())
            {
                theModelView.Results = allDTOs.Skip(theModelView.StartArrayIndex).Take(theModelView.ResultsPerPage).Select(m => new OffloadRateModelView(m)).ToList();
            }

            ViewResult toReturn = View(WebConstants.VIEW_PAGE_SYSTEM_OFFLOAD_RATES, theModelView);

            FinalizeAction(_log, WebConstants.ACTION_PAGE_SYSTEM_OFFLOAD_RATES, sw);
            return toReturn;
        }

        /// <summary>
        /// Displays the partial view for the Manage mileage Reimbursement rates page
        /// </summary>
        /// <returns></returns>
        public ViewResult DisplayManageMileageReimbursement()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayManageMileageReimbursement", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Initialize modelView to return
            ManageMileageReimbursementModelView theModelView = new ManageMileageReimbursementModelView();

            // Get the rates
            MileReimbursementRateDTO rateForMV = _MileReimbursementRateDTOLoader.GetMileReimbursementRateDTO();

            if (rateForMV != null)
            {
                // Create the modelview all the rates
                theModelView.MileageReimbursementRate = rateForMV.MileReimbursementRate;
            }

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_MILEAGE_REIMBURSEMENT, theModelView);

            FinalizeAction(_log, "DisplayManageMileageReimbursement", sw);
            return toReturn;
        }

        /// <summary>
        /// Displays the Manage System Settings page
        /// </summary>
        /// <returns>System Settings view</returns>
        public ViewResult DisplayManageSystemSettings()
        {
            // Action Initialize
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_SYSTEM_SETTINGS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_SYSTEM_SETTINGS);

            // Action Finalize
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_SYSTEM_SETTINGS, sw);

            return toReturn;
        }

        /// <summary>
        /// Gets the system settings.
        /// </summary>
        /// <returns>Json result of the system settings.</returns>
        public JsonResult GetSystemSettings()
        {
            Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_SYSTEM_SETTINGS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

			// Get the current system settings
			ICollection<SystemSettingDTO> systemSettings;
			if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
			{
				systemSettings = this.systemSettingLoader.GetSkillMixSettings();
			}
			else
			{
				systemSettings = this.systemSettingLoader.GetSystemSettings();
			}

			this.FinalizeAction(this._log, WebConstants.ACTION_SYSTEM_SETTINGS, sw);

            return this.Json(systemSettings);
        }

        /// <summary>
        /// Displays the partial view for the Manage Email Preferences page
        /// </summary>
        /// <returns>Partial View.</returns>
        public ViewResult DisplayManageEmailPreferences()
        {
            Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_DISPLAY_MANAGE_EMAIL_PREFERENCES, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ViewResult toReturn = this.View(WebConstants.VIEW_MANAGE_EMAIL_PREFERENCES);

            this.FinalizeAction(this._log, WebConstants.ACTION_DISPLAY_MANAGE_EMAIL_PREFERENCES, sw);
            return toReturn;
        }

        /// <summary>
        /// Displays the overdue training.
        /// </summary>
        /// <returns>Overdue Training View</returns>
        public ViewResult DisplayOverdueTraining()
        {
            Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_DISPLAY_OVERDUE_TRAINING, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ICollection<string> courseNames = this._ControllerLogic.GetTrainingCourseGroups().Select(g => g.CourseGroupName).ToList();
            ViewResult toReturn = this.View(WebConstants.VIEW_MANAGE_OVERDUE_TRAINING, courseNames);

            this.FinalizeAction(this._log, WebConstants.ACTION_DISPLAY_OVERDUE_TRAINING, sw);
            return toReturn;
        }

        /// <summary>
        /// Displays the System ProPricer Exports.
        /// </summary>
        /// <returns>System ProPricer Exports View</returns>
        public ViewResult DisplaySystemProPricerExports()
        {
            Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_DISPLAY_SYSTEM_PROPRICER_EXPORTS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Get Select list for Tasks
            ICollection<SelectListItem> taskSelectList_Unselected = new Collection<SelectListItem>();
            ICollection<SelectListItem> taskSelectList_Selected = new Collection<SelectListItem>();
            IDictionary<int, EnumTypeModelView> allProPricerFieldNames = this._CommonDataMapper.GetProPricerFieldsDictionary(false, Utilities.IsAssignTaskAuthorEnabledForSystem);
            if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
            {
                // need to add Project Map stuff as well
                IDictionary<int, EnumTypeModelView> projectMapFieldNames = this._CommonDataMapper.GetProPricerFieldsDictionary(true, false);
                foreach(KeyValuePair<int, EnumTypeModelView> kvp in projectMapFieldNames)
                {
                    if (!kvp.Value.EnumTypeName.StartsWith("Project Map - "))
                    {
                        kvp.Value.EnumTypeName = "Project Map - " + kvp.Value.EnumTypeName;
                    }
                }

                allProPricerFieldNames.AddRange(projectMapFieldNames);
            }


            List<int> fieldIDs = allProPricerFieldNames.Select(x => x.Value.EnumTypeID).ToList();
            foreach (ProPricerField_Task task in Enum.GetValues(typeof(ProPricerField_Task)))
            {
                if (task != ProPricerField_Task.BLANK && fieldIDs.Contains((int)task))
                {
                    taskSelectList_Unselected.Add(new SelectListItem { Text = allProPricerFieldNames[(int)task].EnumTypeName, Value = ((int)task).ToString() });
                }
            }

            // Get Select list for Resources
            ICollection<SelectListItem> resourceSelectList_Unselected = new Collection<SelectListItem>();
            ICollection<SelectListItem> resourceSelectList_Selected = new Collection<SelectListItem>();
            
            foreach (ProPricerField_Resources resource in Enum.GetValues(typeof(ProPricerField_Resources)))
            {
                if (resource != ProPricerField_Resources.BLANK && fieldIDs.Contains((int)resource))
                {
                    resourceSelectList_Unselected.Add(new SelectListItem { Text = allProPricerFieldNames[(int)resource].EnumTypeName, Value = ((int)resource).ToString() });
                }
            }

            // Get list of custom fields for all system templates
            ICollection<ProPricerDTO> formatDtos = this.proPricerDTODataLoader.GetAllSystemExports();
            ICollection<string> customFieldNames = formatDtos.SelectMany(f => f.ProPricerResources.Where(r => !string.IsNullOrWhiteSpace(r.CustomFieldName)).Select(r => r.CustomFieldName)).ToList();
            ICollection<string> taskNames = formatDtos.SelectMany(f => f.ProPricerTasks.Where(t => !string.IsNullOrWhiteSpace(t.CustomFieldName)).Select(t => t.CustomFieldName)).ToList();
            customFieldNames.AddRange(taskNames);
            customFieldNames = customFieldNames.Distinct().OrderBy(f => f).ToList();

            foreach (string customFieldName in customFieldNames)
            {
                taskSelectList_Unselected.Add(new SelectListItem { Text = customFieldName + " Description", Value = ExportToProPricerModelView.GetFormattedCustomFieldID(customFieldName, ProPricerCustomFieldSelection.CustomFieldDescription) });
                taskSelectList_Unselected.Add(new SelectListItem { Text = customFieldName + " ID", Value = ExportToProPricerModelView.GetFormattedCustomFieldID(customFieldName, ProPricerCustomFieldSelection.CustomFieldID) });
                resourceSelectList_Unselected.Add(new SelectListItem { Text = customFieldName + " Description", Value = ExportToProPricerModelView.GetFormattedCustomFieldID(customFieldName, ProPricerCustomFieldSelection.CustomFieldDescription) });
                resourceSelectList_Unselected.Add(new SelectListItem { Text = customFieldName + " ID", Value = ExportToProPricerModelView.GetFormattedCustomFieldID(customFieldName, ProPricerCustomFieldSelection.CustomFieldID) });
            }


            // Pass all select list items to the View
            this.ViewData["CustomFieldNames"] = customFieldNames;
            this.ViewData["SelectList_Task_Unselected"] = taskSelectList_Unselected;
            this.ViewData["SelectList_Task_Selected"] = taskSelectList_Selected;

            this.ViewData["SelectList_Resources_Unselected"] = resourceSelectList_Unselected;
            this.ViewData["SelectList_Resources_Selected"] = resourceSelectList_Selected;


            ViewResult toReturn = this.View(WebConstants.VIEW_MANAGE_SYSTEM_PROPRICER_EXPORTS);

            this.FinalizeAction(this._log, WebConstants.ACTION_DISPLAY_SYSTEM_PROPRICER_EXPORTS, sw);
            return toReturn;
        }

		/// <summary>
		/// Displays the Manage UCOT page
		/// </summary>
		/// <param name="workspace">The workspace</param>
		/// <returns>The view for Manage UCOT</returns>
		public virtual ActionResult DisplayManageUCOT(string workspace)
		{
			Stopwatch sw = InitializeAction(this._log, WebConstants.ACTION_DISPLAY_MANAGE_UCOT, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);
			decimal ucot = this._ControllerLogic.GetUcotSystemSettingsValue();
			ViewResult toReturn = View(WebConstants.VIEW_MANAGE_UCOT, ucot);
			// Finalize Action
			FinalizeAction(this._log, WebConstants.ACTION_DISPLAY_MANAGE_UCOT, sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the partial view for the performing orgs page
		/// </summary>
		/// <returns></returns>
		public ViewResult DisplayDefaultPerfOrgs()
        {
            Stopwatch sw = InitializeAction(_log, "DisplayDefaultPerfOrgs", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Initialize modelView to return
            DefaultPerfOrgModelView theModelView = new DefaultPerfOrgModelView();

            // Get the global performing orgs list
            PerformingOrgListDTO globalPerformingOrgsList = _PerformingOrgListDTODataLoader.GetPerfOrgList(CommonConstants.GLOBAL_PERFORMING_ORG_LIST_ID);
            
            if (globalPerformingOrgsList != null)
            {
                // Create the modelview with the list and performing orgs collection
                theModelView = new DefaultPerfOrgModelView(globalPerformingOrgsList);
            }

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_DEFAULT_PERF_ORGS, theModelView);

            FinalizeAction(_log, "DisplayDefaultPerfOrgs", sw);
            return toReturn;
        }

        /// <summary>
        /// View the grid to manage the output formats
        /// </summary>
        /// <param name="getActiveTemplates">Whether method should get Active or Archived templates</param>
        /// <returns>grid view with all export formats</returns>
        public ViewResult ManageOutputFormatTemplatesGrid(bool getActiveTemplates = true)
        {
            // Initialize Action
            Stopwatch sw = this.InitializeAction(this._log, "ManageOutputFormatTemplatesGrid", SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // get the actual export format DTOs and order by their template name
            List<WorkspaceExportFormatNameDTO> exportFormats = this._WorkspaceExportFormatDTOLoader.GetAllWorkspaceExportFormatIds().Where(x => x.IsActive == getActiveTemplates).OrderBy(x => x.ExportFormatName).ToList();
            
            ManageOutputFormatTemplatesModelView mv = new ManageOutputFormatTemplatesModelView();
            mv.GridRows = new Collection<ManageOutputFormatTemplatesModelViewRow>();
            foreach (WorkspaceExportFormatNameDTO exportFormat in exportFormats)
            {
                mv.PagedIndexes.Add(exportFormat.ExportFormat.TemplateId);
                if (mv.GridRows.Count < mv.ResultsPerPage)
                {
                    mv.GridRows.Add(new ManageOutputFormatTemplatesModelViewRow
                    {
                        TemplateDescription = exportFormat.ExportFormatDescription,
                        TemplateName = exportFormat.ExportFormatName,
                        UpdateDate = exportFormat.UpdateDate,
                        TemplateId = exportFormat.ExportFormat.TemplateId,
                        IsAvailableToAllWorkspaces = exportFormat.IsAvailableToAllWorkspaces,
                        IsActive = exportFormat.IsActive
                    });
                }
            }

            ViewResult toReturn = this.View(WebConstants.VIEW_MANAGE_OUTPUT_TEMPLATES_GRID, mv);

            // Action Finalize
            this.FinalizeAction(this._log, "ManageOutputFormatTemplatesGrid", sw);

            return toReturn;
        }

        /// <summary>
        /// Page the output format
        /// </summary>
        /// <returns></returns>
		[HttpPost]
        public ViewResult PageOutputFormat(ManageOutputFormatTemplatesModelView inOutputFormat)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_PAGE_OUTPUT_FORMAT, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            if (inOutputFormat == null)
            {
                throw new ArgumentNullException(nameof(inOutputFormat));
            }

            inOutputFormat.GridRows = new Collection<ManageOutputFormatTemplatesModelViewRow>();

            for (int i = inOutputFormat.StartArrayIndex; i <= inOutputFormat.EndArrayIndex; i++)
            {
                inOutputFormat.GridRows.Add(new ManageOutputFormatTemplatesModelViewRow(
                    _WorkspaceExportFormatDTOLoader.GetById(inOutputFormat.PagedIndexes[i])));
            }

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_OUTPUT_TEMPLATES_GRID, inOutputFormat);

            // Action Finalize
            FinalizeAction(_log, WebConstants.ACTION_PAGE_OUTPUT_FORMAT, sw);

            return toReturn;
        }

		[HttpPost]
		public ViewResult DisplayManageDefaultPerfOrgsGrid()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_DEFAULT_PERF_ORGS_GRID, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Get the global performing orgs
            Collection<PerformingOrgDTO> globalPerformingOrgs = this.perfOrgLoader.GetGlobalPerformingOrgs();

            DefaultPerfOrgGridModelView theModelView = new DefaultPerfOrgGridModelView();

            foreach (PerformingOrgDTO performingOrg in globalPerformingOrgs)
            {
                theModelView.PagedIndexes.Add(performingOrg.Id);
                if (theModelView.PerfOrgResults.Count < theModelView.ResultsPerPage)
                {
                    theModelView.PerfOrgResults.Add(new DefaultPerfOrgMV(performingOrg));
                }
            }

            theModelView.TotalResults = theModelView.PagedIndexes.Count;

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_DEFAULT_PERF_ORGS_GRID, theModelView);

            // Action Finalize
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_DEFAULT_PERF_ORGS_GRID, sw);

            return toReturn;
        }

		[HttpPost]
		public ViewResult DisplayManageDefaultResourcesGrid(string searchText, bool showLabor, bool showIWTA, bool showSub, bool showODC, bool showTravel, bool showMaterials)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_DEFAULT_RESOURCES_GRID, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            DefaultResourcesGridModelView theModelView = _GetAndFilterResources(searchText, showLabor, showIWTA, showSub, showODC, showTravel, showMaterials);

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_DEFAULT_RESOURCES_GRID, theModelView);

            // Action Finalize
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_MANAGE_DEFAULT_RESOURCES_GRID, sw);

            return toReturn;
        }

		/// <summary>
		/// Display the System ProPricer Grid
		/// </summary>
		/// <param name="workspace">the workspace</param>
		/// <returns>the ProPricer grid view</returns>
		[HttpPost]
        public ViewResult DisplayProPricerGrid()
        {
            // Initialize Action
            Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_DISPLAY_EXPORT_TO_PROPRICER_GRID, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            IReadOnlyCollection<CustomFieldDTO> availableCustomFields = new List<CustomFieldDTO>().AsReadOnly();
            ICollection<ProPricerDTO> formatDtos = this.proPricerDTODataLoader.GetAllSystemExports();
            List<ExportToProPricerModelView> theModelViews = formatDtos.Select(f => new ExportToProPricerModelView(f, this.Factory, availableCustomFields)).ToList();

            theModelViews.Sort();

            ViewResult toReturn = this.View(WebConstants.VIEW_SYSTEM_EXPORT_TO_PROPRICER_GRID, theModelViews);

            // Finalize Action
            this.FinalizeAction(this._log, WebConstants.ACTION_DISPLAY_EXPORT_TO_PROPRICER_GRID, sw);
            return toReturn;
        }


        #endregion display

        #region AJAX Calls

        /// <summary>
        /// Saves a ProPricer Export Format
        /// </summary>
        /// <param name="inModelView">The export format to save/delete</param>
        /// <param name="workspace">The workspace</param>
        /// <returns>json result</returns>
		[HttpPost]
        public JsonResult SaveProPricerExportFormat(ExportToProPricerModelView inModelView)
        {
            if (inModelView == null) { throw new ArgumentNullException(nameof(inModelView)); }

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_PROPRICER_EXPORT_FORMAT, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

			JsonResult toReturn = Json(new { Status = true });

            ProPricerDTO dto = inModelView.GetAssociatedDTO();

            // If this is not a deletion, validate the modelView
            if (!inModelView.Deleted)
            {
                if (ModelState.IsValid)
                {
					// Validate Format
					SystemExportToProPricerFormatValidator validator = new SystemExportToProPricerFormatValidator(this.proPricerDTODataLoader);
					Collection<string> validationerrors = validator.validation(dto, (Collection<Dictionary<string, string>>)null);

                    if (validationerrors.Count != 0)
                    {
                        throw new GenValidationException(SystemExportToProPricerFormatValidator.CreateValidationErrorResponse(validationerrors));
                    }
                }
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.proPricerDTODataLoader.SaveSystemProPricerExport(dto);
                scope.Complete();
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_PROPRICER_EXPORT_FORMAT, sw);
            return toReturn;
        }

        /// <summary>
        /// Saves an update for a System ProPricer Custom Field Name.
        /// </summary>
        /// <param name="originalName">The original name.</param>
        /// <param name="updatedName">The updated name.</param>
		[HttpPost]
        public JsonResult SaveProPricerCustomField(string originalName, string updatedName)
        {
            if (string.IsNullOrEmpty(originalName))
            {
                throw new ArgumentNullException(nameof(originalName));
            }

            if (string.IsNullOrEmpty(updatedName))
            {
                throw new ArgumentNullException(nameof(updatedName));
            }

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_PROPRICER_EXPORT_CUSTOM_FIELD, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

			JsonResult toReturn = Json(new { Status = true });

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.proPricerDTODataLoader.SaveSystemProPricerCustomField(originalName, updatedName);
                scope.Complete();
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_PROPRICER_EXPORT_CUSTOM_FIELD, sw);
            return toReturn;
        }

		/// <summary>
		/// Deletes a collection of ProPricer Export Formats.
		/// </summary>
		/// <param name="inFormatsToDelete">The collection of export formats to delete</param>
		/// <returns>True</returns>
		[HttpPost]
		public JsonResult DeleteProPricerExportFormats(ICollection<ExportToProPricerModelView> inFormatsToDelete)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DELETE_PROPRICER_EXPORT_FORMATS, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            if (inFormatsToDelete == null)
            {
                throw new ArgumentNullException(nameof(inFormatsToDelete));
            }

            if (inFormatsToDelete.Count() > 0)
            {
                foreach (ExportToProPricerModelView format in inFormatsToDelete)
                {
                    // Save each deleted format
                    if (format.Deleted)
                    {
                        format.Scope = ProPricerScope.System;
                        SaveProPricerExportFormat(format);
                    }
                }
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DELETE_PROPRICER_EXPORT_FORMATS, sw);
            return Json(true);
        }

        /// <summary>
        /// Imports the overdue training.
        /// </summary>
        /// <returns>Json result for import</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
        public JsonResult ImportOverdueTraining(string courseName)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_IMPORT_OVERDUE_TRAINING, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            // Perform Action
            JsonResult toReturn = null;

            // If a file was uploaded successfully
            if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
            {
                try
                {
                    ICollection<string> courseIds = this._ControllerLogic.GetTrainingCourseGroups().SelectMany(g => g.Courses).Select(c => c.CourseID).ToList();

                    ICollection<TrainingModelView> models = this.trainingImporter.ImportFromExcelFile(Request.Files[0].InputStream, courseIds);

                    ICollection<TrainingModelView> overdueList = this._ControllerLogic.GetOverdueTraining(this._ActiveDirectoryUtilities, models, courseName);

                    // Return a success message
                    toReturn = this.Json(new { Status = true, Data = overdueList });
                }
                // Catch custom exceptions from ExcelImporter and TrainingImporter and generate friendly
                // exception messages to display for the user
                catch (NotExcelFileException)
                {
                    toReturn = this.Json(new { Status = false, Message = "File is in an invalid format. File must be in a MS Excel format (.xlsx or .xls) with a WorkSheet named Report." });
                }
                catch (ColumnMissingException ex2)
                {
                    toReturn = this.Json(new { Status = false, Message = string.Format("File does not contain all of the required columns. File must contain the required columns: {0}. The following columns are missing: {1}.", string.Join(", ", this.trainingImporter.RequiredColumns), ex2.Message) });
                }
                catch (CellValueMissingException ex3)
                {
                    toReturn = this.Json(new { Status = false, Message = string.Format("A row in the file does not contain a value for one of the required columns: {0}. Every filled row must have a value for each. Check the following column: {1}.", string.Join(", ", this.trainingImporter.RequiredColumns), ex3.Message) });
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Unknown Import Training Error.");
                    toReturn = this.Json(new { Status = false, Message = "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import." });
                }
            }
            // If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
            else
            {
                toReturn = this.Json(new { Status = false, Message = "No file selected for upload" });
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_IMPORT_OVERDUE_TRAINING, sw);
            return toReturn;
        }

        /// <summary>
        /// Gets the system email preferences.
        /// </summary>
        /// <returns>Json result of the system email preferences.</returns>
		[HttpPost]
        public JsonResult GetSystemEmailPreferences()
        {
            Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_SYSTEM_EMAIL_PREFERENCES, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Get the current preferences
            ICollection<EmailModelDomain> emails = this._CommonDataMapper.GetEmails().OrderBy(e => e.Category).ThenBy(e => e.Recipient).ToList();

            this.FinalizeAction(this._log, WebConstants.ACTION_SYSTEM_EMAIL_PREFERENCES, sw);

            return this.Json(emails);
        }

        /// <summary>
        /// This is being used in a view (ManageOutputFormatTemplatesGrid.ascx), so even though it looks silly, there's a purpose to it
        /// </summary>
        private struct WorkspaceInfoForTemplateId
        {
            public string ElVal { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Get is used in a view, so it's not actually uncalled")]
            public int AttrVal { get; set; }
        }

		[HttpPost]
        public ContentResult GetAssignedWorkspaceInfo(int templateId)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_GET_ASSIGNED_WORKSPACE_INFO, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // gather up the workspaces that are assigned to this template
            Collection<int> assignedWorkspaceIds = _WorkspaceExportFormatDTOLoader.GetWorkspaceIdsByExportTemplateId(templateId);
            ICollection<WorkspaceDTO> workspaces = this.workspaceLoader.GetByIds(assignedWorkspaceIds);

            Collection<WorkspaceStateModelView> workspaceStates = _CommonDataMapper.getWorkspaceStates();
            Collection<UserDTO> users = this.UserLoader.GetAllUsers();

            var toReturn = workspaces.OrderBy(x => x.WorkspaceName).Select(x => new { 
                Workspace =  x.WorkspaceName,
                State = workspaceStates.First(y => y.WorkspaceStateID == (int)x.WorkspaceState).WorkspaceState,
                Pricer = users.First(y => y.UserID == x.CostVolumeLeadPricerUserID).DisplayName
            });

            StringBuilder html = new StringBuilder();
            foreach (var result in toReturn) {
                html.Append("<tr>");
                html.Append("<td>" + result.Workspace + "</td>");
                html.Append("<td>" + result.State + "</td>");
                html.Append("<td>" + result.Pricer + "</td>");
                html.Append("</tr>");
            }

            if (html.Length == 0)
            {
                html.Append("<tr><td colspan=\"3\">No workspace are using this template.</td></tr>");
            }

            FinalizeAction(_log, WebConstants.ACTION_GET_ASSIGNED_WORKSPACE_INFO, sw);

            return Content(html.ToString());
        }

		[HttpPost]
        public JsonResult GetWorkspacesUsingTemplate(int templateId)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_GET_WORKSPACES_USING_TEMPLATE, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // gather up the workspaces that are assigned to this template
            Collection<int> workspaceIds = _WorkspaceExportFormatDTOLoader.GetWorkspaceIdsByExportTemplateId(templateId);
            ICollection<WorkspaceDTO> workspaces = workspaceLoader.GetByIds(workspaceIds);

            var toReturn = workspaces.OrderBy(x => x.WorkspaceName).Select(x => new
            {
                Name = x.WorkspaceName,
                Id = x.Id
            });

            FinalizeAction(_log, WebConstants.ACTION_GET_WORKSPACES_USING_TEMPLATE, sw);

            return Json(toReturn);
        }

        /// <summary>
        /// Return information about the workspaces that are already assigned for a given export format id
        /// NOTE: Automatically EXCLUDE workspaces in the closed or complete states
        /// </summary>
        /// <param name="inExportFormatId">The export format id to filter workspaces on</param>
        /// <returns>information about the workspaces that are already assigned for a given export format id</returns>
		[HttpPost]
        public JsonResult GetWorkspacesAssignedForExportTemplateId(int inExportTemplateId)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_ASSIGNED_WORKSPACES_FOR_OUTPUT_FORMAT_TEMPLATE, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // gather up the workspaces that are assigned to this template
            Collection<int> assignedWorkspaceIds = _WorkspaceExportFormatDTOLoader.GetAssignedWorkspaceIdsForExportTemplateId(inExportTemplateId);
            Collection<WorkspaceInfoForTemplateId> wsInfos = new Collection<WorkspaceInfoForTemplateId>();
            ICollection<WorkspaceDTO> assignedWorkspaces = this.workspaceLoader.GetByIds(assignedWorkspaceIds);
            foreach (WorkspaceDTO workspace in assignedWorkspaces)
            {
                wsInfos.Add(new WorkspaceInfoForTemplateId
                {
                    ElVal = workspace.WorkspaceName,
                    AttrVal = workspace.Id
                });
            }

            wsInfos = new Collection<WorkspaceInfoForTemplateId>((from w in wsInfos
	                      orderby w.ElVal
	                      select w).ToArray());

            FinalizeAction(_log, WebConstants.ACTION_ASSIGNED_WORKSPACES_FOR_OUTPUT_FORMAT_TEMPLATE, sw);

            return Json(wsInfos);
        }

        /// <summary>
        /// Return information about the workspaces that are available to be assigned for a given export format id
        /// NOTE: Automatically EXCLUDE workspaces in the closed or complete states
        /// </summary>
        /// <param name="inExportFormatId">The export format id to filter workspaces on</param>
        /// <returns>information about the workspaces that are available to be assigned for a given export format id</returns>
		[HttpPost]
        public JsonResult GetAvailableWorkspaceIdsForExportFormatId(int inExportTemplateId)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_AVAILABLE_WORKSPACES_FOR_OUTPUT_FORMAT_TEMPLATE, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // gather up the workspaces that are available to be assigned for this template
            Collection<int> assignedWorkspaceIds = _WorkspaceExportFormatDTOLoader.GetAvailableWorkspaceIdsForExportFormatId(inExportTemplateId);
            Collection<WorkspaceInfoForTemplateId> wsInfos = new Collection<WorkspaceInfoForTemplateId>();
            ICollection<WorkspaceDTO> assignedWorkspaces = this.workspaceLoader.GetByIds(assignedWorkspaceIds);
            foreach (WorkspaceDTO workspace in assignedWorkspaces)
            {
                wsInfos.Add(new WorkspaceInfoForTemplateId
                {
                    ElVal = workspace.WorkspaceName,
                    AttrVal = workspace.Id
                });
            }

            wsInfos = new Collection<WorkspaceInfoForTemplateId>((from w in wsInfos
	                      orderby w.ElVal
	                      select w).ToArray());

            FinalizeAction(_log, WebConstants.ACTION_AVAILABLE_WORKSPACES_FOR_OUTPUT_FORMAT_TEMPLATE, sw);

            return Json(wsInfos);
        }

        /// <summary>
        /// Saves Workspaces selected for Export Format Template 
        /// </summary>
        /// <param name="inTemplateId">The template id</param>
        /// <param name="isAvailableToAllWorkspaces">Whether template is available to all workspaces or workspaces were manually selected</param>
        /// <param name="inUserSelectedAssignedWsIds">Selected Workspace Ids</param>
        /// <returns></returns>
		[HttpPost]
        public JsonResult SaveExportTemplatesForWorkspaces(int inTemplateId, bool isAvailableToAllWorkspaces, int[] inUserSelectedAssignedWsIds)
        {
            Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_SAVE_WORKSPACES_FOR_OUTPUT_TEMPLATES, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            WorkspaceExportFormatDTO template = this._WorkspaceExportFormatDTOLoader.GetById(inTemplateId);

            if (isAvailableToAllWorkspaces)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    // Set IsAvailableToAllWorkspaces using upsert
                    template.IsAvailableToAllWorkspaces = true;
                    template.Updateable = UpdateType.Upsert;
                    this._WorkspaceExportFormatDTOLoader.Save(new Collection<WorkspaceExportFormatDTO>() {template});

                    // Clear XREFs for all workspaces
                    this._WorkspaceExportFormatDTOLoader.DeleteTemplateForAllWorkspaces(inTemplateId);

                    scope.Complete();
                }
            }
            else
            {
                if (inUserSelectedAssignedWsIds == null)
                {
                    inUserSelectedAssignedWsIds = new int[] { };
                }
                
                // look up the original list of workspaces associated to this template and associate the new ones
                // and remove the ones no longer associated
                ICollection<int> wsDeletedByUser = this._WorkspaceExportFormatDTOLoader.GetAssignedWorkspaceIdsForExportTemplateId(inTemplateId).Except(inUserSelectedAssignedWsIds).ToCollection();

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    // If template was previously available to all, set this field to false
                    if (template.IsAvailableToAllWorkspaces)
                    {
                        template.IsAvailableToAllWorkspaces = false;
                        template.Updateable = UpdateType.Upsert;
                        this._WorkspaceExportFormatDTOLoader.Save(new Collection<WorkspaceExportFormatDTO>() { template });
                    }

                    // delete assigned first
                    this._WorkspaceExportFormatDTOLoader.DeleteWorkspaceExportFormatPicklist(new Collection<int>(wsDeletedByUser.ToArray()), inTemplateId);

                    // fetch assigned list after first performing delete to get most current information
                    ICollection<int> wsAddedByUser = inUserSelectedAssignedWsIds.Except(this._WorkspaceExportFormatDTOLoader.GetAssignedWorkspaceIdsForExportTemplateId(inTemplateId)).ToCollection();

                    this._WorkspaceExportFormatDTOLoader.InsertWorkspaceExportFormatPicklist(new Collection<int>(wsAddedByUser.ToArray()), inTemplateId);

                    scope.Complete();
                } 
            }

            this.FinalizeAction(_log, WebConstants.ACTION_SAVE_WORKSPACES_FOR_OUTPUT_TEMPLATES, sw);

            return this.Json(new { Status = true });
        }

        /// <summary>
        /// Saves the system email preferences.
        /// </summary>
        /// <param name="emails">The emails.</param>
        /// <returns>A Json value</returns>
		[HttpPost]
        public JsonResult SaveSystemEmailPreferences(ICollection<EmailModelDomain> emails)
        {
            if (emails == null)
            {
                throw new ArgumentNullException(nameof(emails));
            }

            Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_SAVE_SYSTEM_EMAIL_PREFERENCES, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            if (ModelState.IsValid)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    this._CommonDataMapper.SaveSystemEmails(emails);
                    scope.Complete();
                }
            }
            else
            {
                throw new ValidationException(SiteMasterUtilities.CreateValidationErrorResponse(ModelState));
            }

            this.FinalizeAction(this._log, WebConstants.ACTION_SAVE_SYSTEM_EMAIL_PREFERENCES, sw);

            return this.Json(new { Status = true });
		}

		/// <summary>
		/// Saves the system email preferences.
		/// </summary>
		/// <param name="emails">The emails.</param>
		/// <returns>A Json value</returns>
		[HttpPost]
		public JsonResult SaveUCOT(decimal? ucot)
		{
			if (ucot == null)
			{
				throw new ArgumentNullException(nameof(ucot));
			}

			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_SAVE_UCOT, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);
			bool isSuccess = this._ControllerLogic.SaveUcotSystemSettings(ucot.Value);

			this.FinalizeAction(this._log, WebConstants.ACTION_SAVE_UCOT, sw);

			return this.Json(new { Status = isSuccess });
		}

		/// <summary>
		/// A method to save a new system permissions set.
		/// </summary>
		/// <param name="inPermission">SavePermission MV</param>
		/// <returns>A Json value</returns>
		[HttpPost]
		public JsonResult SaveNewSystemPermissions(SavePermissionsModelView inPermission)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_NEW_SYSTEM_PERMISSIONS, SecurityPage.SystemAdmin,
                SecurityAuthorization.CreateReadUpdateDelete, null, null);

            Role inRole = Role.SystemAdmin;
            JsonResult toReturn = SaveNewPermissions(inPermission, inRole);


            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_NEW_SYSTEM_PERMISSIONS, sw);
            return toReturn;
        }

        /// <summary>
        /// A method to save a new create workspace permissions set.
        /// </summary>
        /// <param name="inPermission">SavePermission MV</param>
        /// <returns>A Json value</returns>
		[HttpPost]
        public JsonResult SaveNewCreateWorkspacePermissions(SavePermissionsModelView inPermission)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_NEW_CREATE_WORKSPACE_PERMISSIONS, SecurityPage.SystemAdmin,
                SecurityAuthorization.CreateReadUpdateDelete, null, null);

            Role inRole = Role.CreateWorkspacePermissions;
            JsonResult toReturn = SaveNewPermissions(inPermission, inRole);


            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_NEW_CREATE_WORKSPACE_PERMISSIONS, sw);
            return toReturn;
        }

        /// <summary>
        /// A method to save a new permissions set.
        /// </summary>
        /// <param name="inPermission">SavePermission MV</param>
        /// <returns>A Json value</returns>
		[HttpPost]
        private JsonResult SaveNewPermissions(SavePermissionsModelView inPermission, Role inRole)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_PERMISSIONS, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            if (inPermission == null)
            {
                throw new ArgumentNullException(nameof(inPermission));
            }

            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();


            if (inPermission.EntityIds.Count() == 0 ||
                string.IsNullOrEmpty(inPermission.EntityIds.FirstOrDefault())) // the model returns an empty string 
            {
                ValidationErrors.Add(new ValidationMessage("AtLeastOneUser", "At Least one user is required."));
                throw new GenValidationException(ValidationErrors);
            }
            JsonResult toReturn = Json(new { Status = true });

            /** Valid Model Check */
            if (ModelState.IsValid)
            {
                Collection<String> messages = new Collection<String>();

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {

                    // check to see if we were given a group and it exists
                    foreach (string entity in inPermission.EntityIds)
                    {
                        string entityTrimmed = entity.Trim();
                        bool isGroup = false;
						
                        // check to see if this is a group, then insert if not yet in our database
						if (entityTrimmed.Contains('.'))
                        {
                            isGroup = true;

                            if (entityTrimmed.Contains('\\'))
                            {
                                entityTrimmed = entityTrimmed.Split(new char[] { '\\' })[1];
                            }

                            if (!this._ActiveDirectoryUtilities.IsGroup(entity.Trim()))
                            {
                                ValidationErrors.Add(new ValidationMessage("The group '" + entity + "' was not found"));
                            }
                        } // end if we are a group
                        else
                        {
							// details from AD
							UserData user = this._ActiveDirectoryUtilities.GetUserByQualifiedAccount(entity, false);
							if (user == null)
							{
								ValidationErrors.Add(new ValidationMessage("UserNotFound", "User not found"));
							}
							else if (this.GetGenBOEAccess(new Collection<UserData>() { user }).Any(x => !x.Value))
							{
								ValidationErrors.Add(new ValidationMessage("NoGenBoeAccess", user.DisplayName + " does not have access to genBOE and cannot be added to this Workspace's permissions. Please have the user request access."));
							}
						}

						if (ValidationErrors.Any())
						{
							throw new GenValidationException(ValidationErrors);
						}

						// check to see if user is in the database
						UserDTO userDTO = this.UserLoader.GetOrCreateUserByNtid(entityTrimmed);

						Collection<PermissionsDTO> Permissions = this.PermissionsLoader.GetUserPermissions(userDTO);

                        PermissionsDTO check = new PermissionsDTO
                        {
                            ETIUserId = userDTO.UserID,
                            Role = inRole
                        };
                        
                        // throw exception (pending inserts get rolled back) if attempt is made to grant permission to a subcontractor 
                        if (!isGroup && _SecurityInformation.IsSubcontractorUser(userDTO.NTID, userDTO.IsSubcontractor))
                        {
                            string message = "";
                            if (inRole == IES.Common.Role.CreateWorkspacePermissions)
                            {   // customize validation message for Create Workspace permission
                                message = "Subcontractor users are not permitted Create Workspace permission";
                            }

                            if (inRole == IES.Common.Role.SystemAdmin)
                            {   // customize validation message for System Administrator permission
                                message = "Subcontractor users are not permitted System Administrator permission";
                            }
                            ValidationErrors.Add(new ValidationMessage("SubcontractorAuthor", message));
                        }

                        if (ValidationErrors.Any() || Permissions == null || !Permissions.Contains(check))
                        {

                            _PermissionControllerLogic.SavePotentialPermission(new PermissionsDTO
                            {
                                BOEId = null, // no BOE at this point since we're managing workspace permissions
                                ETIUserId = userDTO.UserID,
                                Role = inRole,
                                Updateable = UpdateType.Upsert
                            });

                            // clear their permissions cache
							this.Factory.ClearPermissionsCache(userDTO.NTID);
						}
                        else
                        {
                            ValidationErrors.Add(new ValidationMessage("Permissions already exist"));
                        }


                        //} // end if this is a user

                    } // end foreach entity to insert
                    if (ValidationErrors.Any())
                    {
                        throw new GenValidationException(ValidationErrors);
                    }
                    toReturn = Json(new { Status = true, Messages = messages });
                    scope.Complete();
                } // end transaction scope

            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            FinalizeAction(_log, WebConstants.ACTION_SAVE_PERMISSIONS, sw);

            return toReturn;
        }

        /// <summary>
        /// Checks to see if the action will remove the persons System admin access.
        /// </summary>
        /// <param name="inEntityId">The entity ID</param>
        /// <param name="inRoles">The list of roles(if this is null a delete is being performed)</param>
        /// <returns>json true or false</returns>
		[HttpPost]
        public JsonResult CheckIfUserWillLoseTheirSystemAdminAccess(int inEntityId, Collection<Role> inRoles)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_CHECK_IS_USER_WILL_LOSE_THEIR_SYSTEM_ADMIN_ACCESS, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);
            JsonResult result;

            int currentUserID = this.UserLoader.GetUserForActiveUser().UserID;

            if (inEntityId != currentUserID)
            {
                result = Json(false);
            }
            else
            {

                if (inRoles != null)
                {//means we are editing a person or group
                    if (inRoles.Contains(Role.WorkspaceAdmin))
                    {
                        //if they contain WS admin then just return false since they will not lose it.
                        return Json(new { Status = false });
                    }
                }

                // Get the users current roles
                bool onlyOneCopyOfThisUserWithSystemAdmin = !(this.PermissionsLoader.GetAdminPermissions().Where(x => x.ETIUserId == currentUserID && x.Role == Role.SystemAdmin).Count() > 1);

                result = Json(new { Status = onlyOneCopyOfThisUserWithSystemAdmin });
            }

            FinalizeAction(_log, WebConstants.ACTION_CHECK_IS_USER_WILL_LOSE_THEIR_SYSTEM_ADMIN_ACCESS, sw);

            return result;
        }

        /// <summary>
        /// Checks to see if the action will remove the persons ability to create workspace.
        /// </summary>
        /// <param name="inType">the type of entity the action is beign performed on.</param>
        /// <param name="inEntityId">The entity ID</param>
        /// <param name="inRoles">The list of roles(if this is null a delete is being performed)</param>
        /// <returns>json true or false</returns>
		[HttpPost]
        public JsonResult CheckIfUserWillLoseTheirCreateWorkspacePermissionsAccess(int inEntityId, Collection<Role> inRoles)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_CHECK_IS_USER_WILL_LOSE_THEIR_CREATE_WORKSPACE_PERMISSIONS_ACCESS, SecurityPage.CreateWorkspacePermissions, SecurityAuthorization.CreateReadUpdateDelete, null, null);
            JsonResult result;

            int currentUserID = this.UserLoader.GetUserForActiveUser().UserID;
            if (inEntityId != currentUserID)
            {
                result = Json(new { Status = false });
            }
            else
            {
                if (inRoles != null)
                {//means we are editing a person or group
                    if (inRoles.Contains(Role.WorkspaceAdmin))
                    {
                        //if they contain WS admin then just return false since they will not lose it.
                        return Json(new { Status = false });
                    }
                }

                // Get the users current roles
                bool allowed = !(this.PermissionsLoader.GetCreateWorkspacePermissions().Where(x => x.ETIUserId == currentUserID && x.Role == Role.CreateWorkspacePermissions).Count() > 1);

                return Json(new { Status = allowed });
            }

            FinalizeAction(_log, WebConstants.ACTION_CHECK_IS_USER_WILL_LOSE_THEIR_CREATE_WORKSPACE_PERMISSIONS_ACCESS, sw);

            return result;
        }

        /// <summary>
        /// Delete system admin for a user
        /// </summary>
        ///<param name="inUserID">The User ID to delete permissions from.</param>
        /// <returns></returns>
		[HttpPost]
        public JsonResult DeleteUserSystemPermissions(int inUserID)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DELETE_USER_SYSTEM_PERMISSIONS, SecurityPage.SystemAdmin,
                SecurityAuthorization.CreateReadUpdateDelete, null, null);

            JsonResult toReturn = DeleteUserPermissions(inUserID, Role.SystemAdmin);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DELETE_USER_SYSTEM_PERMISSIONS, sw);
            return toReturn;
        }

        /// <summary>
        /// Delete Create Workspace Permissions for a user
        /// </summary>
        ///<param name="inUserID">The User ID to delete permissions from.</param>
        /// <returns></returns>
		[HttpPost]
        public JsonResult DeleteUserCreateWorkspacePermissions(int inUserID)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DELETE_USER_CREATE_WORKSPACE_PERMISSIONS, SecurityPage.SystemAdmin,
                SecurityAuthorization.CreateReadUpdateDelete, null, null);

            JsonResult toReturn = DeleteUserPermissions(inUserID, Role.CreateWorkspacePermissions);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DELETE_USER_CREATE_WORKSPACE_PERMISSIONS, sw);
            return toReturn;
        }

        /// <summary>
        /// Delete admin for a user
        /// </summary>
        ///<param name="inUserID">The User ID to delete permissions from.</param>
        /// <returns></returns>
        private JsonResult DeleteUserPermissions(int inUserID, Role inRole)
        {

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DELETE_USER_PERMISSIONS, SecurityPage.SystemAdmin,
                SecurityAuthorization.CreateReadUpdateDelete, null, null);

            // Perform Action
            JsonResult toReturn = Json(new { Status = false });
            if (ModelState.IsValid)
            {
                // keep a running list of permissions to save .. so we can do them all at 
                // once and not individually where we would be flushing cache after each save
                List<PermissionsDTO> permissionsToSave = new List<PermissionsDTO>();
                List<PermissionsDTO> admins = this.PermissionsLoader.GetAdminPermissions().Where(x => x.Role == inRole).Select(x => x).ToList();

                if (admins.Count < 2 && inRole == Role.SystemAdmin)
                {
                    throw new ValidationException("The System Administrator cannot be deleted. In order to delete the user, at least one other System Administrator must exist.", "Cannot Delete System Administrator");
                }

                //find and mark the user to delete.
                PermissionsDTO permissionForUser = admins.FirstOrDefault(x => x.ETIUserId == inUserID && x.Role == inRole);

                // adjust updateable field and then add to our save list if permission exists
                if (permissionForUser != null)
                {
                permissionForUser.Updateable = UpdateType.Deleted;
                permissionsToSave.Add(permissionForUser);
                }


                // perform all saves here .. we didn't want to do it above because we're also mining for data above and each time we save we blow away cache
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    foreach (PermissionsDTO permissionToSave in permissionsToSave)
                    {
                        _PermissionControllerLogic.SavePotentialPermission(permissionToSave);
                    }

                    scope.Complete();
                }

                toReturn = Json(new { Status = true });

            }
            else
            {
                throw new ValidationException(SiteMasterUtilities.CreateValidationErrorResponse(ModelState));
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DELETE_USER_PERMISSIONS, sw);
            return toReturn;
        }

        /// <summary>
        /// Saves changes to default resources list
        /// </summary>
        /// <param name="resources">The Default Resources Model View</param>
        /// <returns></returns>
		[HttpPost]
        public virtual JsonResult SaveResources(Collection<DefaultResourceModelView> resources)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_DEFAULT_RESOURCES, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            JsonResult toReturn = Json(new { Status = false });
            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

            /** Check for valid model state */
            if (ModelState.IsValid)
            {
                // validate performing org name
                foreach (DefaultResourceModelView resource in resources)
                {
                    // only check new items.
                    if (resource.Deleted == false)
                    {
                        Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>>();
                        validationData.Add(new Dictionary<string, string>() {
                            {"ResourceListID", _ResourceDTODataLoader.GlobalListID.ToString() },
                            {"ResourceID", resource.ResourceID.ToString() }
                        });
                        Validator validator = ValidationFactory.Instance.getValidator(ValidationType.ResourceUniqueID);
                        if (validator.validation(resource.ID, validationData).Count > 0)
                        {
                            ValidationErrors.Add(new ValidationMessage("ResourceIDUnique", "The resource ID must be unique"));
                        }

                        validator = ValidationFactory.Instance.getValidator(ValidationType.ResourceUniqueDesc);
                        if (validator.validation(resource.Description, validationData).Count > 0)
                        {
                            ValidationErrors.Add(new ValidationMessage("ResourceDescUnique", "The resource Description must be unique"));
                        }
                    }
                }


                if (ValidationErrors.Any())
                {
                    throw new GenValidationException(ValidationErrors);
                }

                // Create a list of updated resources from the UI for upsert/deletion
                Collection<ResourceDTO> resourceDTOs = new Collection<ResourceDTO>();

                ICollection<ResourceDTO> resourcesFromDb = this._ResourceDTODataLoader.GetByIds(resources.Select(x => x.ResourceID).Distinct().ToList());
                
                foreach (DefaultResourceModelView resource in resources)
                {
                    if (resource.Deleted == false)
                    {
                        ResourceDTO resourceDTO = null;
                
                        // new resource
                        if (resource.ResourceID < 0)
                        {
                            resourceDTO = new ResourceDTO();
                        }
                        else
                        {
                            resourceDTO = resourcesFromDb.First(x => x.Id == resource.ResourceID);

                            if (resourceDTO == null)
                            {
                                throw new InvalidDataRelationException("The resource was not found.");
                            }
                        }

                        resourceDTO.LaborType = resource.LaborType;
                        resourceDTO.ResourceDesc = resource.Description;
                        resourceDTO.ResourceName = resource.ID;
                        resourceDTO.SegRegion = resource.SegmentRegion;

                        resourceDTO.Segment = SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST ? SegmentType.RMS : SegmentType.SSC;
                        resourceDTO.ElementOfCost = resource.ElementOfCostId;

                        resourceDTO.UpdateDate = resource.UpdateDate;
                        resourceDTO.Updateable = UpdateType.Upsert;
                        resourceDTO.RateType = resource.RateTypeID;

                        resourceDTOs.Add(resourceDTO);
                    }
                    else
                    {
                        ResourceDTO resourceDTO = resourcesFromDb.First(x => x.Id == resource.ResourceID);

                        if (resourceDTO == null)
                        {
                            throw new InvalidDataRelationException("The resource was not found.");
                        }

                        resourceDTO.UpdateDate = resource.UpdateDate;
                        resourceDTO.Updateable = UpdateType.Deleted;

                        resourceDTOs.Add(resourceDTO);
                    }
                }


                // perform save
                if (resourceDTOs.Any())
                {
                    // Save the updated data
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                    {
                        _ResourceDTODataLoader.SaveSystemResources(resourceDTOs);
                        scope.Complete();
                    }
                }

                toReturn = Json(new { Status = true });
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_DEFAULT_RESOURCES, sw);
            return toReturn;
        }

        /// <summary>
        /// Saves a mileage reimbursement rate
        /// </summary>
        /// <returns></returns>
		[HttpPost]
        public virtual JsonResult SaveMileageReimbursement(ManageMileageReimbursementModelView mileageMV)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_MILEAGE_REIMBURSEMENT, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            if (mileageMV == null)
            {
                throw new ArgumentNullException(nameof(mileageMV));
            }

            JsonResult toReturn;
            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

            if (ModelState.IsValid)
            {
                MileReimbursementRateDTO mileageDTO = _MileReimbursementRateDTOLoader.GetMileReimbursementRateDTO();
                mileageDTO.MileReimbursementRate = mileageMV.MileageReimbursementRate;
                mileageDTO.Updateable = UpdateType.Upsert;

                if (ValidationErrors.Any())
                {
                    throw new GenValidationException(ValidationErrors);
                }

                // Save the updated data
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    _MileReimbursementRateDTOLoader.SaveMileReimbursementRateDTO(mileageDTO);
                    scope.Complete();
                }
                toReturn = Json(new { Status = true });
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_MILEAGE_REIMBURSEMENT, sw);
            return toReturn;
        }

        /// <summary>
        /// Saves changes to misc rates list
        /// </summary>
        /// <returns></returns>
		[HttpPost]
        public virtual JsonResult SaveMiscRates(Collection<MiscRateModelView> miscRates)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_MISC_RATES, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            if (miscRates == null)
            {
                throw new ArgumentNullException(nameof(miscRates));
            }

            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
            JsonResult toReturn = Json(new { Status = false });

            Collection<MiscTravelRateDTO> allCurrentTravelRates = miscTravelRateDTOLoader.GetAll();

            foreach(MiscRateModelView miscRateToSave in miscRates)
            {
                if (miscRateToSave.SortCode < 100 && miscRateToSave.SortCode > 0)
                {
                    List<MiscTravelRateDTO> matchingRate = (from x in allCurrentTravelRates
                                        where x.SortCode == miscRateToSave.SortCode && x.Id != miscRateToSave.MiscTravelRateID
                                        select x).ToList();
                    if (matchingRate.Count > 0)
                    {
                        ValidationErrors.Add(new ValidationMessage("Sort Code", "Sort Code " + miscRateToSave.SortCode + " must be unique"));
                    }
                }
                else
                {
                    ValidationErrors.Add(new ValidationMessage("Sort Code", "Sort Code may only be two digits max."));
                }

                bool exists = allCurrentTravelRates.Any(x => x.MiscTravelRateMode.IsEquivalentTo(miscRateToSave.MiscTravelRateMode)
	       && x.Id != miscRateToSave.MiscTravelRateID)? true:false;

                if (exists)
                {
                    ValidationErrors.Add(new ValidationMessage("UniqueMiscMode", string.Format("There is already a Travel Miscellaneous Rate with Transportation Mode: {0}", 
	                                     miscRateToSave.MiscTravelRateMode)));
                }
            }

            Collection<MiscTravelRateDTO> miscTravelRateDTOs = new Collection<MiscTravelRateDTO>();

             /** Valid Model Check */
            if (ModelState.IsValid)
            {
                foreach (MiscRateModelView rate in miscRates)
                {
                    if (rate.Deleted == false)
                    {
                        MiscTravelRateDTO miscTravelRateDTO = null;
                        // new resource
                        if (rate.MiscTravelRateID < 0)
                        {
                            miscTravelRateDTO = new MiscTravelRateDTO();
                        }
                        else
                        {
                            miscTravelRateDTO = allCurrentTravelRates.FirstOrDefault(i => i.Id == rate.MiscTravelRateID);

                            if (miscTravelRateDTO == null)
                            {
                                throw new InvalidDataRelationException("The rate was not found.");
                            }
                        }

                        miscTravelRateDTO.MiscTravelRate = rate.MiscTravelRate;
                        miscTravelRateDTO.Id = rate.MiscTravelRateID;
                        miscTravelRateDTO.MiscTravelRateMode = rate.MiscTravelRateMode;
                        miscTravelRateDTO.SortCode = rate.SortCode;
                        miscTravelRateDTO.Updateable = UpdateType.Upsert;

                        miscTravelRateDTOs.Add(miscTravelRateDTO);
                    }
                    else
                    {
                        MiscTravelRateDTO rateDTO = allCurrentTravelRates.FirstOrDefault(i => i.Id == rate.MiscTravelRateID);

                        if (rateDTO == null)
                        {
                            throw new InvalidDataRelationException("The rate was not found.");
                        }

                        rateDTO.Updateable = UpdateType.Deleted;

                        miscTravelRateDTOs.Add(rateDTO);
                    }
                }


                if (ValidationErrors.Any())
                {
                    throw new GenValidationException(ValidationErrors);
                }

                // Save the updated data
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    miscTravelRateDTOLoader.SaveMiscTravelRates(miscTravelRateDTOs);
                    scope.Complete();
                }
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_MISC_RATES, sw);
            return toReturn;
        }

        /// <summary>
        /// Saves changes to misc rates list
        /// </summary>
        /// <returns></returns>
		[HttpPost]
        public virtual JsonResult SaveEscalationRates(Collection<EscalationRateModelView> escRates)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_ESCALATION_RATES, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            if (escRates == null)
            {
                throw new ArgumentNullException(nameof(escRates));
            }

            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

            Collection<EscalationRatesDTO> escTravelRateDTOs = new Collection<EscalationRatesDTO>();
            ICollection<EscalationRatesDTO> allRates = escalationRatesDTOLoader.GetAll();
                
            foreach (EscalationRateModelView rate in escRates)
            {
                if (rate.Deleted == false)
                {
                    this._ControllerLogic.ValidateEscalationRate(rate, allRates, ValidationErrors);

                    if (ValidationErrors.Any())
                    {
                        throw new GenValidationException(ValidationErrors);
                    }

                    // set new and update record
                    EscalationRatesDTO escTravelRateDTO = null;

                    if (rate.EscalationRateID < 0)
                    {
                        escTravelRateDTO = new EscalationRatesDTO();

                    }
                    else
                    {
                        escTravelRateDTO = this.escalationRatesDTOLoader.GetById(rate.EscalationRateID);

                        if (escTravelRateDTO == null)
                        {
                            throw new InvalidDataRelationException("The rate was not found.");
                        }
                    }

                    escTravelRateDTO.DevEscalation = (rate.DevEscalation / 100);
                    escTravelRateDTO.EscalationRateID = rate.EscalationRateID;
                    escTravelRateDTO.Id = rate.EscalationRateID;
                    escTravelRateDTO.LMSIEscalation = (rate.LMSIEscalation / 100);
                    escTravelRateDTO.MiscRate = (rate.MiscRate / 100);
                    escTravelRateDTO.Year = rate.Year;
                    escTravelRateDTO.Updateable = UpdateType.Upsert;

                    escTravelRateDTOs.Add(escTravelRateDTO);
                }
                else
                {
                    EscalationRatesDTO rateDTO = this.escalationRatesDTOLoader.GetById(rate.EscalationRateID);

                    if (rateDTO == null)
                    {
                        throw new InvalidDataRelationException("The rate was not found.");
                    }

                    rateDTO.Updateable = UpdateType.Deleted;

                    escTravelRateDTOs.Add(rateDTO);
                }
            }
                

            if (ValidationErrors.Any())
            {
                throw new GenValidationException(ValidationErrors);
            }

            // Save the updated data
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                escalationRatesDTOLoader.Save(escTravelRateDTOs);
                scope.Complete();
            }

            JsonResult toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_ESCALATION_RATES, sw);
            return toReturn;
        }

        /// <summary>
        /// Saves changes to Offload Rates.
        /// </summary>
        /// <returns>JsonResult of the save.</returns>
		[HttpPost]
        public virtual JsonResult SaveOffloadRates(Collection<OffloadRateModelView> offloadRates)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_OFFLOAD_RATES, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            if (offloadRates == null)
            {
                throw new ArgumentNullException(nameof(offloadRates));
            }

            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

            JsonResult toReturn = Json(new { Status = false });

            if (ModelState.IsValid)
            {
                Collection<OffloadRatesDTO> offloadRateDTOs = new Collection<OffloadRatesDTO>();
                ICollection<OffloadRatesDTO> allRates = this.offloadRatesDTOLoader.GetAllSystemRates();

                // filter it down to only rates that do not appear to be changing
                // otherwise we would get false findings
                ICollection<OffloadRatesDTO> ratesForValidation = allRates
                    .Where(x => !offloadRates.Any(z => z.Year == x.Year && z.Resource.Equals(x.Resource) 
                    && z.PerformingOrg.Equals(x.PerformingOrg)))
                    .ToList();

                foreach (OffloadRateModelView rate in offloadRates)
                {
                    if (rate.Deleted == false)
                    {
                        this._ControllerLogic.ValidateOffloadRate(rate, ratesForValidation, ValidationErrors);

                        if (ValidationErrors.Any())
                        {
                            throw new GenValidationException(ValidationErrors);
                        }

                        // set new and update record
                        OffloadRatesDTO offloadRateDTO = null;

                        if (rate.OffloadRateID < 0)
                        {
                            offloadRateDTO = new OffloadRatesDTO();

                        }
                        else
                        {
                            offloadRateDTO = allRates.SingleOrDefault(r => rate.OffloadRateID == r.Id);

                            if (offloadRateDTO == null)
                            {
                                throw new InvalidDataRelationException("The rate was not found.");
                            }
                        }

                        offloadRateDTO.Id = rate.OffloadRateID;
                        offloadRateDTO.HourlyRate = rate.HourlyRate;
                        offloadRateDTO.Percent = rate.PercentToOffload;
                        offloadRateDTO.PerformingOrg = rate.PerformingOrg;
                        offloadRateDTO.Resource = rate.Resource;
                        offloadRateDTO.SubResource = rate.SubcontractorResource;
                        offloadRateDTO.Year = rate.Year;
                        offloadRateDTO.Updateable = UpdateType.Upsert;

                        offloadRateDTOs.Add(offloadRateDTO);

                        // we need to make sure that if we added multiple records, they pass validation against each other as well
                        ratesForValidation.Add(offloadRateDTO);
                    }
                    else
                    {
                        OffloadRatesDTO rateDTO = allRates.Single(r => rate.OffloadRateID == r.Id);

                            if (rateDTO == null)
                            {
                                throw new InvalidDataRelationException("The offload rate was not found for id ." + rate.OffloadRateID.ToString());
                            }

                        rateDTO.Updateable = UpdateType.Deleted;

                            offloadRateDTOs.Add(rateDTO);
                        }
                    }

                if (ValidationErrors.Any())
                {
                    throw new GenValidationException(ValidationErrors);
                }

                // Save the updated data
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 10, 0) }))
                {
                    offloadRatesDTOLoader.Save(offloadRateDTOs);
                    scope.Complete();
                }
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_OFFLOAD_RATES, sw);
            return toReturn;
        }

        /// <summary>
        /// Saves system settings
        /// </summary>
        /// <param name="systemSettings">System settings to save</param>
        /// <returns>JsonResult of the save.</returns>
		[HttpPost]
        public virtual JsonResult SaveSystemSettings(ICollection<SystemSettingDTO> systemSettings)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_SYSTEM_SETTINGS, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            if (systemSettings == null)
            {
                throw new ArgumentNullException(nameof(systemSettings));
            }

            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>(); 

            if (ModelState.IsValid)
            {
                this._ControllerLogic.ValidateSystemSettings(systemSettings, validationErrors);
                if (validationErrors.Any())
                {
                    throw new GenValidationException(validationErrors);
                }

                // Save the updated data
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    // Upsert all system settings in the list
                    foreach (SystemSettingDTO systemSetting in systemSettings)
                    {
                        this.systemSettingLoader.SaveSystemSetting(systemSetting);
                    }

                    scope.Complete();
                }

				// Skill Mix settings only (Space only) - update the utilities method
				IEnumerable<string> systemSettingsForSkillMix = systemSettings.Select(x => x.Key);
				if (systemSettingsForSkillMix.Contains(Constants.SKILL_MIX_BLACKLIST))
				{
					Utilities.UpdateSkillMixBlacklistSettings(
						systemSettings.FirstOrDefault(x => x.Key.Equals(Constants.SKILL_MIX_BLACKLIST)) == null ? string.Empty :
							systemSettings.FirstOrDefault(x => x.Key.Equals(Constants.SKILL_MIX_BLACKLIST)).Value);
				}
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            JsonResult toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_SYSTEM_SETTINGS, sw);
            return toReturn;
        }

        /// <summary>
        /// Saves a resource list name
        /// </summary>
        /// <param name="resourceList"></param>
        /// <returns></returns>
		[HttpPost]
        public virtual JsonResult SaveResourceList(DefaultResourcesModelView resourceList)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_RESOURCE_LIST, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);
            if (resourceList == null)
            {
                throw new ArgumentNullException(nameof(resourceList));
            }
            JsonResult toReturn;

            if (ModelState.IsValid)
            {
                ResourceListDTO resourceListDTO = _ResourceListDTODataLoader.GetResourceList(_ResourceDTODataLoader.GlobalListID);
                if (resourceListDTO == null)
                { 
                    resourceListDTO = new ResourceListDTO();
                    resourceListDTO.ResourceListID = resourceList.ListID;
                }
                resourceListDTO.ResourceListName = resourceList.ListName;
                resourceListDTO.UpdateDate = resourceList.UpdateDate;
                resourceListDTO.Updateable = UpdateType.Upsert;

                // Save the updated data
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    _ResourceListDTODataLoader.SaveResourceList(resourceListDTO);
                    scope.Complete();
                }
                toReturn = Json(new { Status = true });
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_RESOURCE_LIST, sw);
            return toReturn;
        }

        /// <summary>
        /// Allows uploads of Excel files for importing new Resource elements
        /// </summary>
        /// <returns>A string indicating the result of the import operation.</returns>

        [HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual ContentResult ImportResources()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_IMPORT_DEFAULT_RESOURCES, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            // Perform Action
            ContentResult toReturn = null;

            // If a file was uploaded successfully
            if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
            {
                try
                {
                    bool importLabor = Request.Form["ImportISGSLabor"] != null;
                    bool importIWTA = Request.Form["ImportIWTA"] != null;
                    bool importSub = Request.Form["ImportSub"] != null;
                    bool importODC = Request.Form["ImportODC"] != null;

                    bool importTravel = Request.Form["ImportTravel"] != null;
                    bool importMaterials = Request.Form["ImportMaterials"] != null;

                    // Call the business layer to parse the uploaded file
                    // If the file was successfully parsed, add the results to the genBOE database
                    Collection<ResourceDTO> resourcesFromImportFile = _ResourcesImporter.ImportFromExcelFile(Request.Files[0].InputStream, _ResourceDTODataLoader.GlobalListID);

                    ICollection<ResourceDTO> newResources = new Collection<ResourceDTO>();
                    ICollection<ResourceDTO> allDeletedResources = new Collection<ResourceDTO>(); 
                    ICollection<ResourceDTO> deletedResources = new Collection<ResourceDTO>();
                    ICollection<ResourceDTO> globalResources = _ResourceDTODataLoader.GetGlobalResources();

                    if (importLabor || importIWTA || importSub || importODC || importTravel || importMaterials)
                    {
                        if (importLabor)
                        {
                            newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor)).ToList();
                            allDeletedResources = allDeletedResources.Concat(globalResources.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor)).ToList();
                        }
                        if (importIWTA)
                        {
                            newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.IWTA)).ToList();
                            allDeletedResources = allDeletedResources.Concat(globalResources.Where(x => x.ElementOfCost == ElementOfCostType.IWTA)).ToList();
                        }
                        if (importSub)
                        {
                            newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.Sub)).ToList();
                            allDeletedResources = allDeletedResources.Concat(globalResources.Where(x => x.ElementOfCost == ElementOfCostType.Sub)).ToList();
                        }
                        if (importODC)
                        {
                            newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.ODC)).ToList();
                            allDeletedResources = allDeletedResources.Concat(globalResources.Where(x => x.ElementOfCost == ElementOfCostType.ODC)).ToList();
                        }
                        if (importTravel)
                        {
                            newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.Travel)).ToList();
                            allDeletedResources = allDeletedResources.Concat(globalResources.Where(x => x.ElementOfCost == ElementOfCostType.Travel)).ToList();
                        }
                        if (importMaterials)
                        {
                            newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.Materials)).ToList();
                            allDeletedResources = allDeletedResources.Concat(globalResources.Where(x => x.ElementOfCost == ElementOfCostType.Materials)).ToList();
                        }

						// get list of deleted resources. This query only pulls back rows that are in the database but not in the excel file
						deletedResources = (from a in allDeletedResources
							where !(from n in newResources
									select n.Id).Contains(a.Id)
							select a).ToList();

                        // in use resources to only allow certain fields to be updated
                        HashSet<int> resourceIDsInUse = _InUseDataLoader.GetSystemResourceIDsInUse();
                        
                        // Set Updateable on each deleted item
                        foreach (ResourceDTO deletedResource in deletedResources)
                        {
                            deletedResource.Updateable = UpdateType.Deleted;
                        }

                        // Process Adds. Adds are defined as new resources with a negative resource ID
                        List<ResourceDTO> addedResources = newResources.Where(x => x.Id < 0).ToList();

                        // Set UpdateDate and Updateable on each added item
                        foreach (ResourceDTO addedResource in addedResources)
                        {
                            addedResource.UpdateDate = DateTime.Now;
                            addedResource.Updateable = UpdateType.Upsert;
                        }

						// get edited resources that aren't currently in use. any resource field can be updated
						ICollection<ResourceDTO> editedResourcesQuery =
                            (from db in globalResources
                            join file in resourcesFromImportFile on db.Id equals file.Id
                            where
                                !resourceIDsInUse.Contains(file.Id) &&
                                (db.ResourceName != file.ResourceName ||
                                db.ResourceDesc != file.ResourceDesc ||
                                db.SegRegion != file.SegRegion ||
                                db.LaborType != file.LaborType ||
                                db.Segment != file.Segment ||
                                db.RateType != file.RateType ||
                                db.ElementOfCost != file.ElementOfCost)
                            select file).ToList();

                        // get in use/has rates edited resources. the only valid fields that can change are desc, segmentregion, and labor type
                        ICollection<ResourceDTO> InUseEditedResourcesQuery =
                            (from current in globalResources
                            from changed in resourcesFromImportFile
                            where current.Id == changed.Id &&
                            resourceIDsInUse.Contains(changed.Id) &&
                            (current.ResourceDesc != changed.ResourceDesc ||
                                current.SegRegion != changed.SegRegion ||
                                current.LaborType != changed.LaborType)
                            select changed).ToList();

                        List<ResourceDTO> ChangedResources = editedResourcesQuery.Concat(InUseEditedResourcesQuery).ToList();

                        Collection<string> ValidationErrors = new Collection<string>();
                        foreach (ResourceDTO resource in ChangedResources)
                        {
                            resource.Updateable = UpdateType.Upsert;

                            if (!string.IsNullOrEmpty(resource.ResourceName) && resource.ResourceName.Length > ResourceDTODataLoader.RESOURCE_NAME_LENGTH)
                            {
                                ValidationErrors.Add(string.Format("The resource ID {0} is too long, it must be less than or equal to {1} characters.", resource.ResourceName, ResourceDTODataLoader.RESOURCE_NAME_LENGTH));
                            }

                            if (!string.IsNullOrEmpty(resource.ResourceDesc) && resource.ResourceDesc.Length > ResourceDTODataLoader.RESOURCE_DESC_LENGTH)
                            {
                                ValidationErrors.Add(string.Format("The resource ID {0} has a description that is too long, it must be less than or equal to {1} characters.", resource.ResourceName, ResourceDTODataLoader.RESOURCE_DESC_LENGTH));
                            }
                        }

                        // validate new resources
                        foreach (ResourceDTO resource in addedResources)
                        {
                            Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>>();
                            validationData.Add(new Dictionary<string, string>() {
                            {"ResourceListID", _ResourceDTODataLoader.GlobalListID.ToString() },
                            {"ResourceID", resource.Id.ToString() }
                            });
                            Validator validator = ValidationFactory.Instance.getValidator(ValidationType.ResourceUniqueID);
                            if (validator.validation(resource.ResourceName, validationData).Count > 0)
                            {
                                ValidationErrors.Add(string.Format("The resource ID {0} must be unique.", resource.ResourceName));
                            }

                            validator = ValidationFactory.Instance.getValidator(ValidationType.ResourceUniqueDesc);
                            if (validator.validation(resource.ResourceDesc, validationData).Count > 0)
                            {
                                // check to make sure the match was not to itself (this can happen if the ID was changed in which case it is 'new' and the old version is in teh deleted resources)
                                if (!deletedResources.Any(r => r.ResourceDesc == resource.ResourceDesc))
                                {
                                    if (allDeletedResources.Any(r => r.ResourceDesc == resource.ResourceDesc))
                                    {
                                        ValidationErrors.Add(string.Format("The ID for resource ID {0} cannot be updated since the ID is in use.", resource.ResourceName));
                                    }
                                    else
                                    {
                                        ValidationErrors.Add(string.Format("The resource Description for resource ID {0} must be unique.", resource.ResourceName));
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(resource.ResourceName) && resource.ResourceName.Length > ResourceDTODataLoader.RESOURCE_NAME_LENGTH)
                            {
                                ValidationErrors.Add(string.Format("The resource ID {0} is too long, it must be less than or equal to {1} characters.", resource.ResourceName, ResourceDTODataLoader.RESOURCE_NAME_LENGTH));
                            }

                            if (!string.IsNullOrEmpty(resource.ResourceDesc) && resource.ResourceDesc.Length > ResourceDTODataLoader.RESOURCE_DESC_LENGTH)
                            {
                                ValidationErrors.Add(string.Format("The resource ID {0} has a description that is too long, it must be less than or equal to {1} characters.", resource.ResourceName, ResourceDTODataLoader.RESOURCE_DESC_LENGTH));
                            }
                        }

                        if (ValidationErrors.Any())
                        {
                            string validationErrors = string.Join(Environment.NewLine, ValidationErrors);
                            toReturn = GenerateUploadResponse(false, validationErrors);
                        }
                        else
                        {
                            Collection<ResourceDTO> toSave = new Collection<ResourceDTO>(deletedResources.Concat(addedResources).Concat(ChangedResources).ToArray());

                            // Save the new list
                            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                            {
                                _ResourceDTODataLoader.SaveSystemResources(toSave);
                                scope.Complete();
                            }

                            // Return a success message
                            toReturn = GenerateUploadResponse(true, "Import Complete");
                        }
                    }
                    else
                    {
                        toReturn = GenerateUploadResponse(false, "At least one Element of Cost must be selected for import.");
                    }
                }
                // Catch custom exceptions from ExcelImporter and ResourcesImporter and generate friendly
                // exception messages to display for the user
                catch (NotExcelFileException)
                {
                    toReturn = GenerateUploadResponse(false, "File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).");
                }
                catch (ColumnMissingException ex2)
                {
                    toReturn = GenerateUploadResponse(false, "File does not contain all of the required columns. File must contain 'ID', 'Description', 'Segment/Region', 'Labor Type', 'Segment', 'Rate Type' and 'Element of Cost' columns. The following columns are missing: {0}.", ex2.Message);
                }
                catch (CellValueMissingException ex3)
                {
                    toReturn = GenerateUploadResponse(false, "A row in the file does not contain a value for ID, Description, Segment/Region, Labor Type, Rate Type or Element of Cost. Every filled row must have a value for each. Check the following column: {0}.", ex3.Message);
                }
                catch (DuplicateValuesException ex4)
                {
                    //send detail dup exceptions to UI
                    StringBuilder _messages = new StringBuilder();
                    foreach ( string msg in ex4.DetailDuplicateExceptionMessages)
                    {
                        if (msg.StartsWith("ID"))
                        {
                            _messages.AppendLine("IDs must be unique. The following IDs are not unique: " + msg + ".  " + "<br/>");
                        }
                        else
                        {
                            _messages.AppendLine("Descriptions must be unique. The following descriptions are not unique: " + msg + ".  ");
                        }
                    }

                    toReturn = GenerateUploadResponse(false, _messages.ToString());
      
                }
                catch (EntityCommandExecutionException)
                {
                    toReturn = GenerateUploadResponse(false, "The Default Resources List was recently updated by another user. Please refresh the page to review these latest changes. Once the page is refreshed, you can try your import operation again.");
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Unknown Import Resources Error.");
                    toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
                }
            }
            // If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
            else
            {
                toReturn = GenerateUploadResponse(false, "No file selected for upload");
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_IMPORT_DEFAULT_RESOURCES, sw);
            return toReturn;
        }

        /// <summary>
        /// Exports the zone travel origins.
        /// </summary>
        /// <returns>Download Result for the Zone Travel Origins.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[HttpGet]
        public ActionResult ExportZoneTravelOrigins()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_ZONE_TRAVEL_ORIGINS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ICollection<MSTZoneTravelOriginModelView> origins = _ControllerLogic.GetZoneTravelOrigins();

            // Get Origins template file name
            string templateFileName = Server.MapPath(TRAVEL_ORIGINS_EXPORT_TEMPLATE);
            
            // Call the export function in the business layer and get back the file name of the populated template.
            string exportedFileName = ZoneTravelOriginExporter.ExportToExcelFile(templateFileName, origins);

            string fileName = "ZoneTravel_Origins.xlsx";
            // Generate a custom ActionResult to cause a file download to the client
            FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            
            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_EXPORT_ZONE_TRAVEL_ORIGINS, sw);

            return File(
                fileStream: fs,
                contentType: ExportFileDownloadBase.GetContentType(fileName),
                fileDownloadName: fileName);
        }

        /// <summary>
        /// Imports the Zone Travel Origins.
        /// </summary>
        /// <returns>Content Result housing the errors or the success.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
        public ContentResult ImportOrigins()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_IMPORT_ORIGINS, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            // Perform Action
            ContentResult toReturn = null;

            // If a file was uploaded successfully
            if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
            {
                try
                {
                    // Get the original Origins
                    ICollection<MSTZoneTravelOriginModelView> originalOrigins = _ControllerLogic.GetZoneTravelOrigins();

                    // Call the business layer to parse the uploaded file
                    // If the file was successfully parsed, add the results to the genBOE database
                    ICollection <MSTZoneTravelOriginModelView> origins = ZoneTravelOriginImporter.ImportFromExcelFile(Request.Files[0].InputStream, originalOrigins);

                    _log.Debug("Saving Import of Origins");

                    // Save the new list
                    _ControllerLogic.SaveOriginData(origins);

                    // Return a success message
                    toReturn = GenerateUploadResponse(true, "Import Complete");
                    
                }
                // Catch custom exceptions from ExcelImporter and PerformingOrgImporter and generate friendly
                // exception messages to display for the user
                catch (NotExcelFileException)
                {
                    toReturn = GenerateUploadResponse(false, "File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).");
                }
                catch (ColumnMissingException ex2)
                {
                    toReturn = GenerateUploadResponse(false, "File does not contain all of the required columns. File must contain 'OriginId', 'Origin', and 'Site' columns. The following columns are missing: {0}.", ex2.Message);
                }
                catch (CellValueMissingException ex3)
                {
                    toReturn = GenerateUploadResponse(false, "A row in the file does not contain a value for Origin, Site or both. Each row must have Origin and Site. Check the following column: {0}.", ex3.Message);
                }
                catch (DuplicateValuesException ex4)
                {
                    toReturn = GenerateUploadResponse(false, "OriginIds must be unique. The following OriginIds are not unique: {0}", ex4.Message);
                }
                catch (EntityCommandExecutionException ex)
                {
                    _log.Error(ex);
                    _log.Error(ex.InnerException);
                    toReturn = GenerateUploadResponse(false, "The Zone Travel Origins were recently updated by another user. Please refresh the page to review these latest changes. Once the page is refreshed, you can try your import operation again using a fresh Export as the base template.");
                }
                catch (ValidationException ex5)
                {
                    toReturn = GenerateUploadResponse(false, ex5.Message);
                }
                catch (GenValidationException ex6)
                {
                    string message = ex6.Message;
                    if (ex6.ValidationList.Any())
                    {
                        message = string.Join(" ", ex6.ValidationList.Select(l => l.ValidationIssue));
                    }
                    toReturn = GenerateUploadResponse(false, message);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Unknown Import Origins Error.");
                    toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
                }
            }
            // If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
            else
            {
                toReturn = GenerateUploadResponse(false, "No file selected for upload");
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_IMPORT_ORIGINS, sw);
            return toReturn;
        }

        /// <summary>
        /// Exports the Escalation Rates.
        /// Note:  This is RMS-specific, will need to be reworked to support Space
        /// </summary>
        /// <returns>Download Result for the Escalation Rates.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[HttpGet]
        public ActionResult ExportEscalationRates()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_ESCALATION_RATES, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Get the rates list
            ICollection<EscalationRatesDTO> ratesForMV = escalationRatesDTOLoader.GetAll().OrderBy(r => r.Year).ToList();

            // Get Escalation Rates template file name
            string templateFileName = Server.MapPath(SYSTEM_ESCALATION_RATES_EXPORT_TEMPLATE);

            // Call the export function in the business layer and get back the file name of the populated template.
            string exportedFileName = SystemEscalationRatesExporterRMS.ExportToExcelFile(templateFileName, ratesForMV);

            string fileName = "Admin_EscalationRates.xlsx";
            // Generate a custom ActionResult to cause a file download to the client
            FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_EXPORT_ESCALATION_RATES, sw);

            return File(
                fileStream: fs,
                contentType: ExportFileDownloadBase.GetContentType(fileName),
                fileDownloadName: fileName);
        }

        /// <summary>
        /// Imports the Escalation Rates.
        /// Note:  This is RMS-specific, will need to be reworked to support Space
        /// </summary>
        /// <returns>Content Result housing the errors or the success.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
        public ContentResult ImportEscalationRates()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_IMPORT_ESCALATION_RATES, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            // Perform Action
            ContentResult toReturn = null;

            // If a file was uploaded successfully
            if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
            {
                try
                {
                    // Get the original Escalation Rates
                    ICollection<EscalationRatesDTO> originalEscalationRates = escalationRatesDTOLoader.GetAll();

                    // Call the business layer to parse the uploaded file
                    // If the file was successfully parsed, add the results to the genBOE database
                    ICollection<EscalationRatesDTO> escalationRates = SystemEscalationRateImporterRMS.ImportFromExcelFile(Request.Files[0].InputStream, originalEscalationRates);

                    // Convert to Modelviews for saving
                    Collection<EscalationRateModelView> escalationRatesModelViews = new Collection<EscalationRateModelView>();
                    foreach (EscalationRatesDTO rate in escalationRates)
                    {
                        EscalationRateModelView modelView = new EscalationRateModelView(rate);
                        if (rate.Updateable == UpdateType.Deleted)
                        {
                            modelView.Deleted = true;
                        }
                        
                        escalationRatesModelViews.Add(modelView);
                    }

                    _log.Debug("Saving Import of Escalation rates");

                    // Save the new list
                    this.SaveEscalationRates(escalationRatesModelViews);

                    // Return a success message
                    toReturn = GenerateUploadResponse(true, "Import Complete");

                }
                // Catch custom exceptions from ExcelImporter and PerformingOrgImporter and generate friendly
                // exception messages to display for the user
                catch (NotExcelFileException)
                {
                    toReturn = GenerateUploadResponse(false, "File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).");
                }
                catch (ColumnMissingException ex2)
                {
                    toReturn = GenerateUploadResponse(false, "File does not contain all of the required columns. File must contain 'Id', 'Year', and 'Rate' columns. The following columns are missing: {0}.", ex2.Message);
                }
                catch (CellValueMissingException ex3)
                {
                    toReturn = GenerateUploadResponse(false, "A row in the file does not contain a value for Year, Rate or both. Each row must have Year and Rate. Check the following column: {0}.", ex3.Message);
                }
                catch (DuplicateValuesException ex4)
                {
                    toReturn = GenerateUploadResponse(false, "Ids must be unique. The following Ids are not unique: {0}", ex4.Message);
                }
                catch (EntityCommandExecutionException ex)
                {
                    _log.Error(ex);
                    _log.Error(ex.InnerException);
                    toReturn = GenerateUploadResponse(false, "The Escalation Rates were recently updated by another user. Please refresh the page to review these latest changes. Once the page is refreshed, you can try your import operation again.");
                }
                catch (ValidationException ex5)
                {
                    toReturn = GenerateUploadResponse(false, ex5.Message);
                }
                catch (GenValidationException ex6)
                {
                    string message = ex6.Message;
                    if (ex6.ValidationList.Any())
                    {
                        message = string.Join(" ", ex6.ValidationList.Select(l => l.ValidationIssue));
                    }
                    toReturn = GenerateUploadResponse(false, message);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Unknown Import Escalation Rates Error.");
                    toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
                }
            }
            // If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
            else
            {
                toReturn = GenerateUploadResponse(false, "No file selected for upload");
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_IMPORT_ESCALATION_RATES, sw);
            return toReturn;
        }

        /// <summary>
        /// Exports the Offload Rates.
        /// Note:  This is RMS-specific, will need to be reworked to support Space
        /// </summary>
        /// <returns>Download Result for the Offload Rates.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[HttpGet]
        public ActionResult ExportOffloadRates()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_OFFLOAD_RATES, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Get the rates list
            ICollection<OffloadRatesDTO> ratesForMV = offloadRatesDTOLoader.GetAllSystemRates().OrderBy(r => r.Resource).ThenBy(r => r.PerformingOrg).ThenBy(r => r.Year).ToList();

            // Get Offload Rates template file name
            string templateFileName = Server.MapPath(SYSTEM_OFFLOAD_RATES_EXPORT_TEMPLATE);

            ICollection<ResourceDTO> systemResources = this._ResourceDTODataLoader.GetGlobalResources();
            ICollection<PerformingOrgDTO> systemPerformingOrgs = this.perfOrgLoader.GetGlobalPerformingOrgs();

            // Call the export function in the business layer and get back the file name of the populated template.
            string exportedFileName = SystemOffloadRatesExporterRMS.ExportToExcelFile(templateFileName, ratesForMV, systemPerformingOrgs, systemResources);
            
            string fileName = "Admin_OffloadRates.xlsx";
            // Generate a custom ActionResult to cause a file download to the client
            FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            // Finalize Action
            FinalizeAction(_log, "WebConstants.ACTION_EXPORT_OFFLOAD_RATES", sw);

            return File(
                fileStream: fs,
                contentType: ExportFileDownloadBase.GetContentType(fileName),
                fileDownloadName: fileName);
        }

        /// <summary>
        /// Imports the Offload Rates.
        /// Note:  This is RMS-specific
        /// </summary>
        /// <returns>Content Result housing the errors or the success.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
        public ContentResult ImportOffloadRates()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_IMPORT_OFFLOAD_RATES, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            // Perform Action
            ContentResult toReturn = null;

            // If a file was uploaded successfully
            if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
            {
                try
                {
                    // Get the original Offload Rates
                    ICollection<OffloadRatesDTO> originalOffloadRates = this.offloadRatesDTOLoader.GetAllSystemRates();

                    // Call the business layer to parse the uploaded file
                    // If the file was successfully parsed, add the results to the genBOE database
                    ICollection<OffloadRatesDTO> offloadRates = SystemOffloadRateImporterRMS.ImportFromExcelFile(Request.Files[0].InputStream, originalOffloadRates);

                    // Convert to Modelviews for saving
                    Collection<OffloadRateModelView> offloadRatesModelViews = new Collection<OffloadRateModelView>();
                    foreach (OffloadRatesDTO rate in offloadRates)
                    {
                        OffloadRateModelView modelView = new OffloadRateModelView(rate);
                        if (rate.Updateable == UpdateType.Deleted)
                        {
                            modelView.Deleted = true;
                        }

                        offloadRatesModelViews.Add(modelView);
                    }

                    _log.Debug("Saving Import of Offload rates");

                    // Save the new list
                    // Extra validation is done inside the Save
                    this.SaveOffloadRates(offloadRatesModelViews);

                    // Return a success message
                    toReturn = GenerateUploadResponse(true, "Import Complete");

                }
                // Catch custom exceptions from ExcelImporter and PerformingOrgImporter and generate friendly
                // exception messages to display for the user
                catch (NotExcelFileException)
                {
                    toReturn = GenerateUploadResponse(false, "File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).");
                }
                catch (ColumnMissingException ex2)
                {
                    toReturn = GenerateUploadResponse(false, "File does not contain all of the required columns. The following columns are missing: {0}.", ex2.Message);
                }
                catch (CellValueMissingException ex3)
                {
                    toReturn = GenerateUploadResponse(false, "A row in the file does not contain a value for the required columns. Each row must have every column filled out. Check the following column: {0}.", ex3.Message);
                }
                catch (DuplicateValuesException ex4)
                {
                    toReturn = GenerateUploadResponse(false, "Ids must be unique. The following Ids are not unique: {0}", ex4.Message);
                }
                catch (EntityCommandExecutionException ex)
                {
                    _log.Error(ex);
                    _log.Error(ex.InnerException);
                    toReturn = GenerateUploadResponse(false, "The Offload Rates were recently updated by another user. Please refresh the page to review these latest changes. Once the page is refreshed, you can try your import operation again.");
                }
                catch (ValidationException ex5)
                {
                    toReturn = GenerateUploadResponse(false, ex5.Message);
                }
                catch (GenValidationException ex6)
                {
                    string message = ex6.Message;
                    if (ex6.ValidationList.Any())
                    {
                        message = string.Join(" ", ex6.ValidationList.Select(l => l.ValidationIssue));
                    }
                    toReturn = GenerateUploadResponse(false, message);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Unknown Import Offload Rates Error.");
                    toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
                }
            }
            // If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
            else
            {
                toReturn = GenerateUploadResponse(false, "No file selected for upload");
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_IMPORT_OFFLOAD_RATES, sw);
            return toReturn;
        }

        /// <summary>
        /// Exports Resources to a pre-formatted MS Excel template and sends the file as a download
        /// to the user.
        /// </summary>
        /// <returns>A special ActionResult that generates a file download for the user to download the
        /// populated Excel template.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[HttpGet]
        public ActionResult ExportResources(bool showLabor, bool showIWTA, bool showSub, bool showODC, bool showTravel, bool showMaterials, string searchText)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_DEFAULT_RESOURCES, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Perform Action

            // Get Default Resources Data and Resource List

            // if the search text came in as null, then no string was inputted so set to empty string not null. this saves the url from being /""
            if (searchText == null)
            {
                searchText = string.Empty;
            }
            DefaultResourcesGridModelView filteredResourcesMV = this._GetAndFilterResources(searchText, showLabor, showIWTA, showSub, showODC, showTravel, showMaterials);
            ICollection<ResourceDTO> resources = _ResourceDTODataLoader.GetByIds(filteredResourcesMV.PagedIndexes);

            ResourceListDTO resourceList = _ResourceListDTODataLoader.GetResourceList(CommonConstants.GLOBAL_LIST_ID);

            // Get Resources template file name
            string templateFileName = Server.MapPath("~/Templates/Export/Resources.xlsx");

            // Call the export function in the business layer and get back the file name of the populated template.
            string exportedFileName = ResourcesExporter.ExportToExcelFile(templateFileName, resources, _CommonDataMapper);

            string fileName = string.Format("GenBOEResources-{0}.xlsx", resourceList.ResourceListName);
            // Generate a custom ActionResult to cause a file download to the client
            FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_EXPORT_DEFAULT_RESOURCES, sw);

            return File(
                fileStream: fs,
                contentType: ExportFileDownloadBase.GetContentType(fileName),
                fileDownloadName: fileName);
        }

        /// <summary>
        /// Download the default resources exmport template
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[HttpGet]
		public ActionResult ExportDefaultResourcesTemplate(string searchText)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_DEFAULT_RESOURCES_TEMPLATE, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Perform Action

            // if the search text came in as null, then no string was inputted so set to empty string not null. this saves the url from being /""
            if (searchText == null)
            {
                searchText = string.Empty;
            }

            ResourceListDTO resourceList = _ResourceListDTODataLoader.GetResourceList(CommonConstants.GLOBAL_LIST_ID);

            // Get resources template file name
            string templateFileName = Server.MapPath("~/Templates/Export/Resources.xlsx");

            // Call the export function in the business layer and get back the file name of the populated template.
            string exportedFileName = ResourcesExporter.ExportTemplate(templateFileName, _CommonDataMapper);

            string fileName = string.Format("GenBOEResources-{0}.xlsx", resourceList.ResourceListName);
            // Generate a custom ActionResult to cause a file download to the client
            FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_EXPORT_DEFAULT_RESOURCES_TEMPLATE, sw);

            return File(
                fileStream: fs,
                contentType: ExportFileDownloadBase.GetContentType(fileName),
                fileDownloadName: fileName);
        }
        
        /// <summary>
        /// Allows uploads of Excel files for importing new Performing Org elements
        /// </summary>
        /// <returns>A string indicating the result of the import operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
        public virtual ContentResult ImportPerformingOrgs()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_IMPORT_DEFAULT_PERFORMING_ORGS, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            // Perform Action
            ContentResult toReturn = null;

            // If a file was uploaded successfully
            if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
            {
                try
                {
                    // Create a DTO to represent the default list and submit it to be cleared. We'll submit with
                    // update date to ensure optimistic locking in case someone edited the list before this import
                    // was submitted
                    PerformingOrgListDTO listDTO = new PerformingOrgListDTO();
                    listDTO.Updateable = UpdateType.Upsert;
                    listDTO.UpdateDate = new DateTime(long.Parse(Request["PerformingOrgListUpdateDateLong"]));
                    listDTO.PerformingOrgListID = int.Parse(Request["PerformingOrgListID"]);

                    // Call the business layer to parse the uploaded file
                    // If the file was successfully parsed, add the results to the genBOE database
                    Collection<PerformingOrgDTO> newPerformingOrgs = _PerformingOrgImporter.ImportFromExcelFile(Request.Files[0].InputStream);

                    //Validate new perf orgs
                    Collection<string> ValidationErrors = new Collection<string>();
                    foreach(PerformingOrgDTO perfOrg in newPerformingOrgs)
                    {
                        if(perfOrg.PerformingOrgName.Length > PerformingOrgDTODataLoader.MAX_PERF_ORG_NAME_LENGTH)
                        {
                            ValidationErrors.Add(string.Format("The Performing Org ID {0} is too long. IDs must be {1} characters or less.", perfOrg.PerformingOrgName, PerformingOrgDTODataLoader.MAX_PERF_ORG_NAME_LENGTH));
                        }

                        if (perfOrg.PerformingOrgDesc.Length > Constants.PERF_ORG_DESC_MAX_LENGTH)
                        {
                            ValidationErrors.Add(string.Format("The Description for Performing Org ID {0} is too long. Descriptions must be {1} characters or less.", perfOrg.PerformingOrgName, Constants.PERF_ORG_DESC_MAX_LENGTH));
                        }
                    }

                    if (ValidationErrors.Any())
                    {
                        string validationErrors = string.Join("<br/>", ValidationErrors);
                        toReturn = GenerateUploadResponse(false, validationErrors);
                    }
                    else
                    {
                        _log.Debug("Clearing Performing Org List");

                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                        {
                            // Clear the list first
                            if (listDTO.PerformingOrgListID > 0)
                            {
                                _PerformingOrgListDTODataLoader.ClearPerformingOrgList(listDTO);
                            }

                            _log.Debug("Saving New Performing Org List");

                            // Save the new list
                            this.perfOrgLoader.SaveSystemPerformingOrgs(newPerformingOrgs);

                            scope.Complete();
                        }

                        // Return a success message
                        toReturn = GenerateUploadResponse(true, "Import Complete");
                    }
                }
                // Catch custom exceptions from ExcelImporter and PerformingOrgImporter and generate friendly
                // exception messages to display for the user
                catch (NotExcelFileException)
                {
                    toReturn = GenerateUploadResponse(false, "File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).");
                }
                catch (ColumnMissingException ex2)
                {
                    toReturn = GenerateUploadResponse(false, "File does not contain all of the required columns. File must contain 'ID' and 'Description' columns. The following columns are missing: {0}.", ex2.Message);
                }
                catch (CellValueMissingException ex3)
                {
                    toReturn = GenerateUploadResponse(false, "A row in the file does not contain a value for ID, Description or both. Each row must have an ID and Description. Check the following column: {0}.", ex3.Message);
                }
                catch (DuplicateValuesException ex4)
                {
                    toReturn = GenerateUploadResponse(false, "IDs must be unique. The following IDs are not unique: {0}", ex4.Message);
                }
                catch (EntityCommandExecutionException ex)
                {
                    _log.Error(ex);
                    _log.Error(ex.InnerException);
                    toReturn = GenerateUploadResponse(false, "The Default Performing Organizations List was recently updated by another user. Please refresh the page to review these latest changes. Once the page is refreshed, you can try your import operation again.");
                }
                catch (ValidationException ex5)
                {
                    toReturn = GenerateUploadResponse(false, ex5.Message);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Unknown Import Performing Orgs Error.");
                    toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
                }
            }
            // If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
            else
            {
                toReturn = GenerateUploadResponse(false, "No file selected for upload");
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_IMPORT_DEFAULT_PERFORMING_ORGS, sw);
            return toReturn;
        }

        /// <summary>
        /// Exports Performing Orgs to a pre-formatted MS Excel template and sends the file as a download
        /// to the user.
        /// </summary>
        /// <returns>A special ActionResult that generates a file download for the user to download the
        /// populated Excel template.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[HttpGet]
        public ActionResult ExportPerformingOrgs()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_DEFAULT_PERFORMING_ORGS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Perform Action
            Collection<PerformingOrgDTO> performingOrgs = this.perfOrgLoader.GetGlobalPerformingOrgs();
            PerformingOrgListDTO performingOrgList = _PerformingOrgListDTODataLoader.GetPerfOrgList(CommonConstants.GLOBAL_PERFORMING_ORG_LIST_ID);

            // Get Performing Orgs template file name
            string templateFileName = Server.MapPath("~/Templates/Export/PerformingOrgs.xlsx");

            // Call the export function in the business layer and get back the file name of the populated template.
            string exportedFileName = PerformingOrgsExporter.ExportToExcelFile(templateFileName, performingOrgs);

            string fileName = string.Format("GenBOEPerformingOrgs-{0}.xlsx", performingOrgList.PerformingOrgListName);
            // Generate a custom ActionResult to cause a file download to the client
            FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_EXPORT_DEFAULT_PERFORMING_ORGS, sw);

            return File(
                fileStream: fs,
                contentType: ExportFileDownloadBase.GetContentType(fileName),
                fileDownloadName: fileName);
        }

        /// <summary>
        /// Save the default performing organizations
        /// </summary>
        /// <param name="inPerfOrg">data on the partial view</param>
        /// <returns></returns>
		[HttpPost]
        public virtual JsonResult SavePerfOrgs(DefaultPerfOrgModelView inPerfOrg)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_DEFAULT_PERF_ORGS, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);
            if (inPerfOrg == null)
            {
                throw new ArgumentNullException(nameof(inPerfOrg));
            }

            // get the original list data from the DB
            PerformingOrgListDTO originalPerfOrgListFromDB = _PerformingOrgListDTODataLoader.GetPerfOrgList(CommonConstants.GLOBAL_PERFORMING_ORG_LIST_ID);

            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

            JsonResult toReturn = Json(new { Status = false });

            /** Valid Model Check */
            if (ModelState.IsValid)
            {
                // validate performing org name
                foreach (DefaultPerfOrgMV performingOrg in inPerfOrg.DefaultPerfOrgs)
                {
                    // only check new items.
                    if (performingOrg.Deleted == false)
                    {
                        Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>>();
                        validationData.Add(new Dictionary<string, string>() {
                            {"PerformingOrgListID", ((PerformingOrgDTODataLoader)perfOrgLoader).GlobalListID.ToString() },
                            {"PerformingOrgID", performingOrg.PerfOrgID.ToString() }
                        });
                        Validator validator = ValidationFactory.Instance.getValidator(ValidationType.PerformingOrgUniqueID);
                        if (validator.validation(performingOrg.PerfOrgName, validationData).Count > 0)
                        {
                            ValidationErrors.Add(new ValidationMessage("UniquePerfID", "The performing organization ID must be unique"));
                        }
                    }
                }

                if (ValidationErrors.Any())
                {
                    throw new GenValidationException(ValidationErrors);
                }

                PerformingOrgListDTO updatedPerfOrgList = new PerformingOrgListDTO { PerformingOrgListID = inPerfOrg.PerfOrgListID, PerformingOrgListName = inPerfOrg.PerfOrgListName, UpdateDate = inPerfOrg.UpdateDate };

                // if the performing org list name doesn't match what's in the db, set as upsert
                if (originalPerfOrgListFromDB == null || (updatedPerfOrgList.PerformingOrgListName != originalPerfOrgListFromDB.PerformingOrgListName))
                {
                    updatedPerfOrgList.Updateable = UpdateType.Upsert;
                }
               
             
                Collection<PerformingOrgDTO> updatedPerfOrgs = new Collection<PerformingOrgDTO>(inPerfOrg.DefaultPerfOrgs.Select(x =>
                    new PerformingOrgDTO
                    {
                        Id = x.PerfOrgID,
                        PerformingOrgName = x.PerfOrgName,
                        PerformingOrgDesc = x.PerfOrgDesc,
                        UpdateDate = x.UpdateDate,
                        Updateable = x.Deleted == true ? UpdateType.Deleted : UpdateType.Upsert
                    }).ToArray());



                // Save the updated data
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    if (updatedPerfOrgList.Updateable == UpdateType.Upsert)
                    {
                        _PerformingOrgListDTODataLoader.SavePerformingOrgList(updatedPerfOrgList);
                    }

                    this.perfOrgLoader.SaveSystemPerformingOrgs(updatedPerfOrgs);
                    scope.Complete();
                }

                toReturn = Json(new { Status = true });
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_DEFAULT_PERF_ORGS, sw);
            return toReturn;
        }

        /// <summary>
        /// Search the default performing organization list given a search string
        /// </summary>
        /// <param name="inSearchText">search string</param>
        /// <returns>list that contains the search string in either the Name or Description column</returns>
        [HttpPost]
		public virtual ActionResult SearchPerfOrgs(string inSearchText)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SEARCH_PERF_ORGS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ViewResult toReturn = null;

            /** Valid Model Check */
            if (ModelState.IsValid)
            {

                // Initialize modelView to return
                DefaultPerfOrgGridModelView theModelView = new DefaultPerfOrgGridModelView();

                // call the BL for the search results
                PerfOrgSearch perfOrgSearchBL = new PerfOrgSearch(this.perfOrgLoader);
                Collection<PerformingOrgDTO> searchResult = perfOrgSearchBL.SearchDefaultPerfOrgs(inSearchText);

                if (searchResult.Count != 0)
                {
                    // Create the modelview with the list and performing orgs collection
                    theModelView.PerfOrgResults = new Collection<DefaultPerfOrgMV>();
                    foreach (PerformingOrgDTO performingOrg in searchResult)
                    {
                        theModelView.PagedIndexes.Add(performingOrg.Id);
                        if (theModelView.PerfOrgResults.Count < theModelView.ResultsPerPage)
                        {
                            theModelView.PerfOrgResults.Add(new DefaultPerfOrgMV(performingOrg));
                        }
                    }
                }

                theModelView.TotalResults = this.perfOrgLoader.GetGlobalPerformingOrgs().Count;

                toReturn = View(WebConstants.VIEW_MANAGE_DEFAULT_PERF_ORGS_GRID, theModelView);

            }
            else
            {
                throw new ValidationException(SiteMasterUtilities.CreateValidationErrorResponse(ModelState));
            }

            FinalizeAction(_log, WebConstants.ACTION_SEARCH_PERF_ORGS, sw);
            return toReturn;
        }

        /// <summary>
        /// Page the performing orgs
        /// </summary>
        /// <returns></returns>
		[HttpPost]
        public ViewResult PagePerfOrgs(DefaultPerfOrgGridModelView performingOrgs)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_PAGE_PERF_ORGS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            if (performingOrgs == null)
            {
                throw new ArgumentNullException(nameof(performingOrgs));
            }

            performingOrgs.PerfOrgResults = new Collection<DefaultPerfOrgMV>();

            Collection<int> ids = new Collection<int>();
            for (int i = performingOrgs.StartArrayIndex; i <= performingOrgs.EndArrayIndex; i++)
            {
                ids.Add(performingOrgs.PagedIndexes[i]);
            }

            HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(ids.Distinct().ToList()));

            for (int i = performingOrgs.StartArrayIndex; i <= performingOrgs.EndArrayIndex; i++)
            {
                performingOrgs.PerfOrgResults.Add(new DefaultPerfOrgMV(perfOrgsFromDb.First(x => x.Id == performingOrgs.PagedIndexes[i])));
            }

            performingOrgs.TotalResults = this.perfOrgLoader.GetGlobalPerformingOrgs().Count;

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_DEFAULT_PERF_ORGS_GRID, performingOrgs);

            // Action Finalize
            FinalizeAction(_log, WebConstants.ACTION_PAGE_PERF_ORGS, sw);

            return toReturn;
        }

        /// <summary>
        /// Search the default resource list given a search string
        /// </summary>
        /// <param name="inSearchText">search string</param>
        /// <returns>list that contains the search string in either the Name or Description column</returns>
        [HttpPost]
		public virtual ActionResult SearchResources(string searchText, bool showLabor, bool showIWTA, bool showSub, bool showODC, bool showTravel, bool showMaterials)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SEARCH_RESOURCES, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            ViewResult toReturn = null;

            /** Valid Model Check */
            if (ModelState.IsValid)
            {

                DefaultResourcesGridModelView theModelView = _GetAndFilterResources(searchText, showLabor, showIWTA, showSub, showODC, showTravel, showMaterials);
                toReturn = View(WebConstants.VIEW_MANAGE_DEFAULT_RESOURCES_GRID, theModelView);

            }
            else
            {
                throw new ValidationException(SiteMasterUtilities.CreateValidationErrorResponse(ModelState));
            }


            FinalizeAction(_log, WebConstants.ACTION_SEARCH_RESOURCES, sw);
            return toReturn;
        }

        private DefaultResourcesGridModelView _GetAndFilterResources(string searchText, bool showLabor, bool showIWTA, bool showSub, bool showODC, bool showTravel, bool showMaterials)
        {
            if (searchText == null)
            {
                throw new ArgumentNullException(nameof(searchText));
            }

            // Initialize modelView to return
            DefaultResourcesGridModelView theModelView = new DefaultResourcesGridModelView();
            theModelView.ResourceResults = new Collection<DefaultResourceModelView>();

            // search results
            ICollection<ResourceDTO> allResources = _ResourceDTODataLoader.GetGlobalResources();
            List<ResourceDTO> filteredResources = new List<ResourceDTO>();
            if (showLabor)
            {
                filteredResources.AddRange(allResources.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor));
            }
            if (showIWTA)
            {
                filteredResources.AddRange(allResources.Where(x => x.ElementOfCost == ElementOfCostType.IWTA));
            }
            if (showSub)
            {
                filteredResources.AddRange(allResources.Where(x => x.ElementOfCost == ElementOfCostType.Sub));
            }
            if (showODC)
            {
                filteredResources.AddRange(allResources.Where(x => x.ElementOfCost == ElementOfCostType.ODC));
            }
            if (showTravel)
            {
                filteredResources.AddRange(allResources.Where(x => x.ElementOfCost == ElementOfCostType.Travel));
            }
            if (showMaterials)
            {
                filteredResources.AddRange(allResources.Where(x => x.ElementOfCost == ElementOfCostType.Materials));
            }

            IDictionary<int,RateTypeModelView> rateTypeValues = _CommonDataMapper.GetRateTypesDictionary();
            IDictionary<int,ElementOfCostTypeModelView> elementOfCostValues = _CommonDataMapper.GetElementOfCostTypesDictionary();
            HashSet<int> resourceIDsInUse = _InUseDataLoader.GetSystemResourceIDsInUse();
            filteredResources = filteredResources.OrderBy(x => x.ResourceName).ToList();
            foreach (ResourceDTO resource in filteredResources)
            {
                string elementOfCostName = elementOfCostValues[(int)resource.ElementOfCost].ElementOfCostName;
                string rateTypeName = rateTypeValues[(int)resource.RateType].RateTypeName;

                if (string.IsNullOrEmpty(rateTypeName))
                {
                    rateTypeName = string.Empty;
                }
                if (string.IsNullOrEmpty(elementOfCostName))
                {
                    elementOfCostName = string.Empty;
                }

                if (resource.ResourceName.ContainsEquivalent(searchText) ||
                    resource.ResourceDesc.ContainsEquivalent(searchText) ||
                    resource.SegRegion.ContainsEquivalent(searchText) ||
                    resource.LaborType.ContainsEquivalent(searchText) ||
                    rateTypeName.ContainsEquivalent(searchText) ||
                    elementOfCostName.ContainsEquivalent(searchText))
                {
                    theModelView.PagedIndexes.Add(resource.Id);
                    if (theModelView.ResourceResults.Count < theModelView.ResultsPerPage)
                    {
                        DefaultResourceModelView mv = new DefaultResourceModelView(resource, rateTypeName, elementOfCostName);
                        if (resourceIDsInUse.Contains(mv.ResourceID))
                        {
                            mv.InUse = true;
                        }
                        else
                        {
                            mv.InUse = false;
                        }
                        theModelView.ResourceResults.Add(mv);
                    }
                }
            }

            theModelView.TotalResults = filteredResources.Count;
            return theModelView;
        }

        /// <summary>
        /// Page the resources
        /// </summary>
        /// <returns></returns>
		[HttpPost]
        public ViewResult PageResources(DefaultResourcesGridModelView resources)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_PAGE_RESOURCES, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            ViewResult toReturn = null;

            /** Valid Model Check */
            if (ModelState.IsValid)
            {
                resources.ResourceResults = new Collection<DefaultResourceModelView>();
                IDictionary<int,RateTypeModelView> rateTypeValues = _CommonDataMapper.GetRateTypesDictionary();
                IDictionary<int,ElementOfCostTypeModelView> elementOfCostValues = _CommonDataMapper.GetElementOfCostTypesDictionary();
                HashSet<int> resourceIDsInUse = _InUseDataLoader.GetSystemResourceIDsInUse();

                if (resources.StartArrayIndex < 0)
                {
                    resources.CurrentPage = 1;
                }

                ICollection<ResourceDTO> resourcesFromDb = this._ResourceDTODataLoader.GetByIds(resources.PagedIndexes);
                
                for (int i = resources.StartArrayIndex; i <= resources.EndArrayIndex; i++)
                {
                    ResourceDTO resource = resourcesFromDb.First(x => x.Id == resources.PagedIndexes[i]);

                    string elementOfCostName = elementOfCostValues[(int)resource.ElementOfCost].ElementOfCostName;
                    string rateTypeName = rateTypeValues[(int)resource.RateType].RateTypeName;

                    DefaultResourceModelView mv = new DefaultResourceModelView(resource, rateTypeName, elementOfCostName);
                    if (resourceIDsInUse.Contains(mv.ResourceID))
                    {
                        mv.InUse = true;
                    }
                    else
                    {
                        mv.InUse = false;
                    }

                    resources.ResourceResults.Add(mv);
                    
                }

                resources.TotalResults = _ResourceDTODataLoader.GetGlobalResources().Count;

                toReturn = View(WebConstants.VIEW_MANAGE_DEFAULT_RESOURCES_GRID, resources);
            }
            else
            {
                throw new ValidationException(SiteMasterUtilities.CreateValidationErrorResponse(ModelState));
            }

            // Action Finalize
            FinalizeAction(_log, WebConstants.ACTION_PAGE_RESOURCES, sw);

            return toReturn;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		[HttpPost]
        public ViewResult PageTripResults(ManageTripsForTravelGridModelView tripResults)
        {
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_PAGE_TRIP_RESULTS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            if (tripResults == null)
            {
                throw new ArgumentNullException(nameof(tripResults));
            }

            Collection<int> tripIds = new Collection<int>();
            for (int i = tripResults.StartArrayIndex; i <= tripResults.EndArrayIndex; i++)
            {
                tripIds.Add(tripResults.PagedIndexes[i]);
            }

            // Get all the required Trip Data for the next page and create the model views.
            ICollection<TripDTO> allTripDTOs = _TripDTODataLoader.GetByIds(tripIds);
            HashSet<LocationDTO> allDepartureLocations = new HashSet<LocationDTO>(_LocationDTODataLoader.GetByIds(allTripDTOs.Select(i => i.DepartureLocationID).ToCollection<int>()));
            HashSet<LocationDTO> allDestinationLocations = new HashSet<LocationDTO>(_LocationDTODataLoader.GetByIds(allTripDTOs.Select(i => i.DestinationLocationID).ToCollection<int>()));
            HashSet<MiscTravelRateDTO> allMisTravelRates = new HashSet<MiscTravelRateDTO>(miscTravelRateDTOLoader.GetByIds(allTripDTOs.Select(i => i.MiscTravelRateID).ToCollection<int>()));
            HashSet<PerDiemDTO> allPerdiems = new HashSet<PerDiemDTO>(_PerDiemDTODataLoader.GetByIds(allTripDTOs.Select(i => i.PerDiemID).ToCollection<int>()));
            HashSet<UserDTO> allUpdatingUsers = new HashSet<UserDTO>(userDataLoader.GetByIds(allTripDTOs.Select(i => i.FareUpdatedByUserID).ToCollection<int>()));

            foreach(TripDTO trip in allTripDTOs)
            {
                MiscTravelRateDTO miscTravelRate = allMisTravelRates.First(t => t.Id == trip.MiscTravelRateID);
                LocationDTO departure = allDepartureLocations.FirstOrDefault(t => t.Id == trip.DepartureLocationID);
                LocationDTO destination = allDestinationLocations.FirstOrDefault(t => t.Id == trip.DestinationLocationID);
                UserDTO fareUpdatedByUser = allUpdatingUsers.FirstOrDefault(t => t.UserID == trip.FareUpdatedByUserID);
                PerDiemDTO perDiem = allPerdiems.First(t => t.Id == trip.PerDiemID);

                tripResults.TripsForTravelCollection.Add(new TripForTravelModelView(trip, miscTravelRate, fareUpdatedByUser, perDiem, departure, destination)); 
            }

            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_TRIPS_GRID, tripResults);

            FinalizeAction(_log, WebConstants.ACTION_PAGE_TRIP_RESULTS, sw);
            return toReturn;
        }

        /// <summary>
        /// Deletes a group of Trips.
        /// </summary>
        /// <param name="inDeletedTrips">Collection of deleted Trips</param>
        /// <returns>If successful,empty string is return. Otherwise, exception error text to be handled in the post:error </returns>
        [HttpPost]
		public virtual JsonResult DeleteTrips(Collection<TripForTravelModelView> inDeletedTrips)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DELETE_TRIPS, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            // Perform Action
            // check if the input is null
            if (inDeletedTrips == null)
            {
                throw new ArgumentNullException(nameof(inDeletedTrips));
            }

            JsonResult toReturn = Json(new { Status = true });

            // no scope needed since it's defined in SaveTrip
            foreach (TripForTravelModelView deletedTrip in inDeletedTrips)
            {
                // Only delete Trips that have positive IDs
                if (deletedTrip.TripID > 0 && deletedTrip.Deleted)
                {
                    SaveTrip(deletedTrip);
                }
            }


            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DELETE_TRIPS, sw);

            return toReturn;
        }

        /// <summary>
        /// Saves a Trip.
        /// </summary>
        /// <param name="inTrip">Trip to save.</param>
        /// <returns>If successful,empty string is return. Otherwise, exception error text to be handled in the post:error </returns>
        [HttpPost]
		public virtual JsonResult SaveTrip(TripForTravelModelView inTrip)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_TRIP, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            // Perform Action
            // check if the input is null
            if (inTrip == null)
            {
                throw new ArgumentNullException(nameof(inTrip));
            }

            JsonResult toReturn = Json(new { Status = true });

            #region Do some validation and preprocessing, before entering the transaction..

            int currentUserID = this.UserLoader.GetUserForActiveUser().UserID;

            // If we are doing a deletion, this will contain the dto(s) to delete
            Collection<TripDTO> tripsToDelete = new Collection<TripDTO>();

            // If we are doing an insertion, these individual DTOs need to be processed & then saved
            TripDTO tripDTO = null;
            PerDiemDTO perDiemDTO = null;
            LocationDTO departureLocationDTO = null;
            LocationDTO destinationLocationDTO = null;

            this.ValidateAndPreProcessTripDataForSaving(inTrip, ref tripsToDelete, ref tripDTO, ref perDiemDTO, ref departureLocationDTO, ref destinationLocationDTO);

            #endregion

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                // Deletes
                if (inTrip.Deleted)
                {
                    _TripDTODataLoader.SaveTrips(tripsToDelete);
                }
                // Upserts
                else
                {
                    // Save Departure Location
                    if (departureLocationDTO.Id < 0)
                    {
                        departureLocationDTO.Updateable = UpdateType.Upsert;
                        departureLocationDTO.LastUpdatedBy = currentUserID;
                        int? newId = _LocationDTODataLoader.Save(departureLocationDTO);

                        tripDTO.DepartureLocationID = newId.HasValue ? newId.Value : 0;
                    }
                    else
                    {
                        tripDTO.DepartureLocationID = departureLocationDTO.Id;
                    }

                    // Save Destination Location
                    if (destinationLocationDTO != null && destinationLocationDTO.Id < 0)
                    {
                        destinationLocationDTO.Updateable = UpdateType.Upsert;
                        destinationLocationDTO.LastUpdatedBy = currentUserID;
                        int? newId = _LocationDTODataLoader.Save(destinationLocationDTO);

                        tripDTO.DestinationLocationID = newId.HasValue ? newId.Value : 0;
                    }
                    else
                    {
                        tripDTO.DestinationLocationID = destinationLocationDTO.Id;
                    }

                    // Save Per Diem
                    perDiemDTO.LastUpdatedBy = currentUserID;
                    tripDTO.PerDiemID = _PerDiemDTODataLoader.SavePerDiem(perDiemDTO);

                    // Save Trip
                    tripDTO.FareUpdatedByUserID = currentUserID;
                    _TripDTODataLoader.SaveTrips(new Collection<TripDTO>() { tripDTO });
                }

                scope.Complete();
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_TRIP, sw);

            return toReturn;
        }

		[HttpPost]
        public virtual JsonResult AutocompleteTripLocationName(string searchTerm)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_AUTOCOMPLETE_LOCATION_NAME, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Perform Action
            IEnumerable<string> locations = (from t in _LocationDTODataLoader.GetAllLocations()
                             where t.LocationName != null &&
                                   t.LocationName.StartsWith(searchTerm, StringComparison.CurrentCultureIgnoreCase)
                             select t.LocationName).Distinct();

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_AUTOCOMPLETE_LOCATION_NAME, sw);

            return Json(locations);
        }

		[HttpPost]
        public virtual JsonResult AutocompleteTripDepartureLocationCode(string searchTerm, string locationName)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_AUTOCOMPLETE_DEP_LOCATION_CODE, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Perform Action
            IEnumerable<string> locationCodes = (from l in _LocationDTODataLoader.GetAllLocations()
                                 join t in _TripDTODataLoader.GetAllTrips()
                                 on l.Id equals t.DepartureLocationID
                                 where l.LocationName != null &&
                                 t.DepartureLocationCode != null
                                 && l.LocationName.StartsWith(locationName, StringComparison.CurrentCultureIgnoreCase)
                                 select  t.DepartureLocationCode).Distinct();
                

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_AUTOCOMPLETE_DEP_LOCATION_CODE, sw);

            return Json(locationCodes);
        }

		[HttpPost]
        public virtual JsonResult AutocompleteTripDestinationLocationCode(string searchTerm, string locationName)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_AUTOCOMPLETE_DES_LOCATION_CODE, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Perform Action
            IEnumerable<string> locationCodes = (from l in _LocationDTODataLoader.GetAllLocations()
                                 join t in _TripDTODataLoader.GetAllTrips()
                                 on l.Id equals t.DestinationLocationID
                                 where l.LocationName != null &&
                                 t.DestinationLocationCode != null
                                 && l.LocationName.StartsWith(locationName, StringComparison.CurrentCultureIgnoreCase)
                                 select t.DestinationLocationCode).Distinct();


            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_AUTOCOMPLETE_DES_LOCATION_CODE, sw);

            return Json(locationCodes);
        }

		[HttpPost]
        public virtual JsonResult GetLocationDetails(string name, string code)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_GET_LOCATION_DETAILS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            JsonResult toReturn = new JsonResult();

            if (!string.IsNullOrWhiteSpace(name))
            {
                // Perform Action
                // If the code passed in was null, get the first location whose name matches the name passed in.
                // If the code was not null, get the location whose name and code match those passed in.
                var location = (from t in _LocationDTODataLoader.GetAllLocations()
                                where t.LocationName.Equals(name.Trim(), StringComparison.CurrentCultureIgnoreCase) 
                                select new
                                {
                                    t.Id
                                }).FirstOrDefault();

                if (location != null)
                {
                toReturn = Json(location);
            }
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_GET_LOCATION_DETAILS, sw);

            return toReturn;
        }

		[HttpPost]
        public virtual JsonResult AutocompleteTripPerDiemLocation(string searchTerm)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_AUTOCOMPLETE_PER_DIEM, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Perform Action
            IEnumerable<string> locations = (from t in _PerDiemDTODataLoader.GetAllPerDiem()
                             where t.PerDiemDestination.StartsWith(searchTerm, StringComparison.CurrentCultureIgnoreCase)
                             select t.PerDiemDestination).Distinct();

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_AUTOCOMPLETE_PER_DIEM, sw);

            return Json(locations);
        }

		[HttpPost]
        public virtual JsonResult GetPerDiemLocationDetails(string name, string qualification)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_GET_PER_DIEM_LOCATION_DETAILS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            JsonResult toReturn = new JsonResult();

            // Perform Action
            var location = (from t in _PerDiemDTODataLoader.GetAllPerDiem()
                            where t.PerDiemDestination != null &&
                                  t.PerDiemDestination.Equals(name.Trim(), StringComparison.CurrentCultureIgnoreCase) &&
                                  ((t.Qualification == null && string.IsNullOrWhiteSpace(qualification)) ||
                                    (t.Qualification != null && t.Qualification.Equals(qualification.Trim(), StringComparison.CurrentCultureIgnoreCase)))
                            select new
                            {
                                PerDiemID = t.Id,
                                t.PerDiemNotes,
                                HotelRate = t.HotelRate.ToString("#.00"),
                                MIERate = t.MIERate.ToString("#.00"),
                                PerDiemLastUpdated =
                                    (t.LastUpdatedBy.HasValue ? this.UserLoader.GetUserByID(t.LastUpdatedBy.Value).DisplayName : string.Empty) +
                                    " " +
                                    t.PerDiemLastUpdatedDate.ToString("MM/dd/yyyy"),
                                PerDiemUpdateDateLong = t.UpdateDate.Ticks.ToString()
                            }).FirstOrDefault();

            if (location != null)
            {
                toReturn = Json(location);
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_GET_PER_DIEM_LOCATION_DETAILS, sw);

            return toReturn;
        }

		[HttpPost]
        public virtual JsonResult AutocompleteTripQualification(string searchTerm)
        {
            //Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_AUTOCOMPLETE_QUALIFICATION, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Perform Action
            IEnumerable<string> locations = (from t in _PerDiemDTODataLoader.GetAllPerDiem()
                             where t.Qualification != null &&
                                   t.Qualification.StartsWith(searchTerm, StringComparison.CurrentCultureIgnoreCase)
                             select t.Qualification).Distinct();

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_AUTOCOMPLETE_QUALIFICATION, sw);

            return Json(locations);
        }

        /// <summary>
        /// Export all Trips
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [HttpGet]
		[Obsolete("No longer used")]
		public ActionResult ExportTrips()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_TRIPS, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Get Performing Orgs template file name
            string templateFileName = Server.MapPath("~/Templates/Export/Trips.xlsx");

            // Call the export function in the business layer and get back the file name of the populated template.
            string exportedFileName = _TripsExporter.ExportToExcelFile(templateFileName);

            string fileName = "GenBOE-Trips.xlsx";
            // Generate a custom ActionResult to cause a file download to the client
            FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_EXPORT_TRIPS, sw);

            return File(
                fileStream: fs,
                contentType: ExportFileDownloadBase.GetContentType(fileName),
                fileDownloadName: fileName);
        }

        /// <summary>
        /// Export the Trips template
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [HttpGet]
		[Obsolete("No longer used")]
		public ActionResult ExportTripsTemplate()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_TRIPS_TEMPLATE, SecurityPage.SystemAdmin, SecurityAuthorization.Read, null, null);

            // Get Performing Orgs template file name
            string templateFileName = Server.MapPath("~/Templates/Export/Trips.xlsx");

            // Call the export function in the business layer and get back the file name of the populated template.
            string exportedFileName = _TripsExporter.ExportTemplate(templateFileName);

            string fileName = "GenBOE-Trips.xlsx";
            // Generate a custom ActionResult to cause a file download to the client
            FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_EXPORT_TRIPS_TEMPLATE, sw);

            return File(
                fileStream: fs,
                contentType: ExportFileDownloadBase.GetContentType(fileName),
                fileDownloadName: fileName); 
        }

        /// <summary>
        /// Imports Trips
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
		[Obsolete("No longer used")]
		public ViewResult ImportTrips()
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_IMPORT_TRIPS, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            ViewResult toReturn = null;
            ImportTripsResultsModelView theModelView = new ImportTripsResultsModelView();

            try
            {
                // Perform Action
                // If a file was uploaded successfully
                if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
                {
                    ICollection<PerDiemDTO> allPerDiems = _PerDiemDTODataLoader.GetAllPerDiem();
                    ICollection<LocationDTO> allLocations = _LocationDTODataLoader.GetAllLocations();

                    // Call the business layer to parse the uploaded file
                    // If the file was successfully parsed, add the results to the genBOE database
                    ImportedTripResults importResults =
                        _TripsImporter.ImportTripsFromExcelFile(Request.Files[0].InputStream, allPerDiems, allLocations);

                    foreach (ImportedTrip importResult in importResults.ImportedTrips)
                    {
                        foreach (TripImportResult resultType in importResult.ImportTypes)
                        {
                            theModelView.ImportedTrips.Add(new ImportedTripModelView()
                            {
                                Mode = importResult.Mode,
                                MiscTravelRateID = importResult.MiscTravelRateID,
                                DepartureLocation = importResult.DepartureLocationName,
                                DepartureID = importResult.DepartureLocationID,
                                DepartureCode = importResult.DepartureLocationCode,
                                DestinationID = importResult.DestinationLocationID,
                                DestinationCode = importResult.DestinationLocationCode,
                                PerDiemID = importResult.PerDiemID,
                                PerDiemDestination = importResult.PerDiemDestination,
                                Qualification = importResult.Qualification,
                                Fare = importResult.Fare,
                                RTMiles = importResult.RTMiles,
                                MissingFields = importResult.MissingFields,
                                InvalidField = importResult.InvalidField,
                                InvalidValue = importResult.InvalidValue,
                                TripID = importResult.TripID,
                                ImportType = (int)resultType,
                                RentalCarRate = importResult.RentalCarRate
                            });
                        }
                    }

                    // add per diems
                    foreach (PerDiemDTO modifiedPerDiem in importResults.ModifiedPerDiems)
                    {
                        theModelView.ImportedPerDiems.Add(new ImportedPerDiemModelView()
                        {
                            PerDiemID = modifiedPerDiem.Id,
                            PerDiemDestination = modifiedPerDiem.PerDiemDestination,
                            Qualification = modifiedPerDiem.Qualification,
                            HotelRate = modifiedPerDiem.HotelRate,
                            MIERate = modifiedPerDiem.MIERate,
                            PerDiemNotes = modifiedPerDiem.PerDiemNotes
                        });
                    }

                    foreach (LocationDTO newLocation in importResults.NewLocations)
                    {
                        theModelView.ImportedLocations.Add(new ImportedLocationModelView()
                        {
                            LocationID = newLocation.Id,
                            LocationName = newLocation.LocationName
                        });
                    }
                }

                ViewData["ERRORS_OCCURRED"] = false;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                ViewData["ERRORS_OCCURRED"] = true;
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            IEnumerable<ImportedTripModelView> dataToSave = from m in theModelView.ImportedTrips
                             where m.ImportType == (int)TripImportResult.AddNewTrip || m.ImportType == (int)TripImportResult.UpdateExistingTrip
                             select m;
            ViewData["SERIALIZED_DATA"] = serializer.Serialize(from x in dataToSave
	                   select new
	                   {
	                       Mode = x.Mode,
	                       MiscTravelRateID = x.MiscTravelRateID,
	                       DepartureID = x.DepartureID,
	                       DepartureCode = x.DepartureCode,
	                       DestinationID = x.DestinationID,
	                       DestinationCode = x.DestinationCode,
	                       PerDiemID = x.PerDiemID,
	                       Fare = x.Fare.ToString(),
	                       RTMiles = x.RTMiles.ToString(),
	                       RentalCarRate = x.RentalCarRate.ToString(),
	                       MissingFields = x.MissingFields,
	                       InvalidField = x.InvalidField,
	                       InvalidValue = x.InvalidValue,
	                       TripID = x.TripID,
	                       ImportType = x.ImportType
	                   });
            ViewData["DOCUMENT_DOMAIN"] = Request["documentDomain"];

            toReturn = View(WebConstants.VIEW_MANAGE_TRIPS_IMPORT_VERIFICATION, theModelView);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_IMPORT_TRIPS, sw);

            return toReturn;
        }

		[HttpPost]
		[Obsolete("No longer used")]
		public JsonResult CompleteTripsImport(ImportTripsResultsModelView importResults)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_COMPLETE_TRIPS_IMPORT, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            if (importResults == null)
            {
                throw new ArgumentNullException(nameof(importResults));
            }

            JsonResult toReturn = Json(new { Status = false });

            Collection<TripDTO> tripsToSave = new Collection<TripDTO>();
            Collection<PerDiemDTO> perDiemsToSave = new Collection<PerDiemDTO>();
            Collection<LocationDTO> locationsToSave = new Collection<LocationDTO>();
            int currentUserID = this.UserLoader.GetUserForActiveUser().UserID;

            foreach (ImportedLocationModelView location in importResults.ImportedLocations)
            {
                if (location.LocationID > 0)
                {
                    throw new ValidationException("Cannot edit an existing location");
                }

                LocationDTO locationDTO = new LocationDTO();
                locationDTO.Id = location.LocationID;
                locationDTO.LocationName = location.LocationName;
                locationDTO.LastUpdatedBy = currentUserID;
                locationDTO.Updateable = UpdateType.Upsert;

                locationsToSave.Add(locationDTO);
            }

            ICollection<PerDiemDTO> allImportedPerDiems = _PerDiemDTODataLoader.GetByIds(importResults.ImportedPerDiems.Select(i => i.PerDiemID).ToCollection<int>());
            foreach (ImportedPerDiemModelView perDiem in importResults.ImportedPerDiems)
            {
                PerDiemDTO perDiemDTO = null;
                if (perDiem.PerDiemID > 0)
                {
                    perDiemDTO = allImportedPerDiems.First(i => i.Id == perDiem.PerDiemID);
                }
                else
                {
                    perDiemDTO = new PerDiemDTO();
                    perDiemDTO.Id = perDiem.PerDiemID;
                    perDiemDTO.PerDiemDestination = perDiem.PerDiemDestination;
                    perDiemDTO.Qualification = perDiem.Qualification;
                }

                perDiemDTO.HotelRate = perDiem.HotelRate;
                perDiemDTO.MIERate = perDiem.MIERate;
                perDiemDTO.PerDiemNotes = perDiem.PerDiemNotes;
                perDiemDTO.LastUpdatedBy = currentUserID;
                perDiemDTO.Updateable = UpdateType.Upsert;

                perDiemsToSave.Add(perDiemDTO);
            }

            HashSet<TripDTO> importedTrips = new HashSet<TripDTO>(_TripDTODataLoader.GetByIds(importResults.ImportedTrips.Select(i => i.TripID).ToCollection<int>()));
            foreach (ImportedTripModelView trip in importResults.ImportedTrips)
            {
                TripDTO tripDTO = null;
                if (trip.TripID > 0)
                {
                    tripDTO = importedTrips.First(i => i.TripID == trip.TripID);
                }
                else
                {
                    tripDTO = new TripDTO();
                    tripDTO.TripID = trip.TripID;
                }

                tripDTO.MiscTravelRateID = trip.MiscTravelRateID;
                tripDTO.PerDiemID = trip.PerDiemID;
                tripDTO.DepartureLocationID = trip.DepartureID;
                tripDTO.DepartureLocationCode = trip.DepartureCode;
                tripDTO.DestinationLocationID = trip.DestinationID;
                tripDTO.DestinationLocationCode = trip.DestinationCode;
                tripDTO.Fare = trip.Fare;
                tripDTO.RTMiles = trip.RTMiles;
                tripDTO.FareUpdatedByUserID = currentUserID;
                tripDTO.Updateable = UpdateType.Upsert;
                tripDTO.RentalCarRate = trip.RentalCarRate;
                
                tripsToSave.Add(tripDTO);
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                // save locations
                foreach (LocationDTO location in locationsToSave)
                {
                    int oldLocationId = location.Id;
                    int? newLocationId = _LocationDTODataLoader.Save(location);
                    location.Id = newLocationId.HasValue ? newLocationId.Value : 0;

                    // update departures
                    foreach (TripDTO trip in tripsToSave.Where(x => x.DepartureLocationID == oldLocationId))
                    {
                        trip.DepartureLocationID = location.Id;
                    }

                    // update destinations
                    foreach (TripDTO trip in tripsToSave.Where(x => x.DestinationLocationID == oldLocationId))
                    {
                        trip.DestinationLocationID = location.Id;
                    }
                }

                // save per diems
                foreach (PerDiemDTO perDiem in perDiemsToSave)
                {
                    int oldPerDiemID = perDiem.Id;
                    perDiem.Id = _PerDiemDTODataLoader.SavePerDiem(perDiem);


                    // update trips with new per diems
                    foreach (TripDTO trip in tripsToSave.Where(x => x.PerDiemID == oldPerDiemID))
                    {
                        trip.PerDiemID = perDiem.Id;
                    }
                }

                // save the trips as one block .. this saves time in clearing cache
                _TripDTODataLoader.SaveTrips(tripsToSave);

                scope.Complete();
            }

            toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_COMPLETE_TRIPS_IMPORT, sw);

            return toReturn;
        }

        /// <summary>
        /// Saves Origin for MST Zone Travel - Upsert or Delete
        /// </summary>
        /// <param name="inOrigin">Origin to save</param>
        /// <returns></returns>
		[HttpPost]
        public virtual JsonResult SaveOrigin(MSTZoneTravelOriginModelView inOrigin)
        {
            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_ORIGIN, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            // Perform Action
            // check if the input is null
            if (inOrigin == null)
            {
                throw new ArgumentNullException(nameof(inOrigin));
            }

            JsonResult toReturn = Json(new { Status = true });

            if (!ModelState.IsValid)
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            _ControllerLogic.SaveOriginData(new Collection<MSTZoneTravelOriginModelView> { inOrigin });         

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_ORIGIN, sw);

            return toReturn;
        }

        /// <summary>
        /// Deletes Origins and their Resources for MST Zone Travel
        /// </summary>
        /// <param name="inDeletedOrigins">Origins to delete</param>
        /// <returns></returns>
		[HttpPost]
        public virtual JsonResult DeleteOrigins(Collection<MSTZoneTravelOriginModelView> inDeletedOrigins)
        {
            //Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DELETE_ORIGINS, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            //Perform Action
            //check if the input is null
            if (inDeletedOrigins == null)
            {
                throw new ArgumentNullException(nameof(inDeletedOrigins));
            }

            JsonResult toReturn = Json(new { Status = true });

            foreach (MSTZoneTravelOriginModelView deletedOrigin in inDeletedOrigins)
            {
                //Only delete if ID is positive and set for deletion
                if (deletedOrigin.OriginID > 0 && deletedOrigin.Deleted)
                {
                    _ControllerLogic.SaveOriginData(new Collection<MSTZoneTravelOriginModelView> { deletedOrigin });
                }
            }

            //Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DELETE_ORIGINS, sw);

            return toReturn;
        }

        /// <summary>
        /// Saves Destination - updates to assigned zone
        /// </summary>
        /// <param name="inDestination">Destination to save</param>
        /// <returns></returns>
        [HttpPost]
		public virtual JsonResult SaveDestination(MSTZoneTravelDestinationModelView inDestination)
        {
            //Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_DESTINATION, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            //Perform Action
            //check if input is null
            if (inDestination == null)
            {
                throw new ArgumentNullException(nameof(inDestination));
            }

            JsonResult toReturn = Json(new { Status = true });
            
            //Validate and preprocess
            if(!ModelState.IsValid)
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            //Save
            _ControllerLogic.SaveDestinationData(inDestination);

            //Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_DESTINATION, sw);

            return toReturn;
        }

        /// <summary>
        /// Save updates to Fees and Costs for Nonzone Travel
        /// </summary>
        /// <param name="feesAndCosts">Fees and Costs modelview</param>
        /// <returns></returns>
        [HttpPost]
		public virtual JsonResult SaveFeesAndCosts(NonzoneFeesAndCostsModelView feesAndCosts)
        {
            //Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_FEES_AND_COSTS, SecurityPage.SystemAdmin, SecurityAuthorization.CreateReadUpdateDelete, null, null);

            //Perform Action
            if (feesAndCosts == null)
            {
                throw new ArgumentNullException(nameof(feesAndCosts));
            }

            JsonResult toReturn = Json(new { status = true });

            //Save
            if (ModelState.IsValid)
            {
                _ControllerLogic.SaveFeesAndCostsData(feesAndCosts);
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            //Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_FEES_AND_COSTS, sw);

            return toReturn;
        }

        #endregion AJAX Calls

        #region Private Methods

        /// <summary>
        /// Validates and preprocesses the trip data in order for it to be saved
        /// </summary>
        /// <param name="inTrip">Trip we are trying to save</param>
        /// <param name="tripsToDelete">If the trip is being deleted, this collection gets filled in w/ the trip</param>
        /// <param name="refTripDTO">If the trip is being inserted, this is where the tripDto is loaded into</param>
        /// <param name="refPerDiemDTO">If the trip is being inserted, this is where the perDiemDto is loaded into</param>
        /// <param name="refDepartureLocationDTO">If the trip is being inserted, this is where the departureLocation is loaded into</param>
        /// <param name="refDestinationLocationDTO">If the trip is being inserted, this is where the destinationLocation is loaded into</param>
        private void ValidateAndPreProcessTripDataForSaving(TripForTravelModelView inTrip, ref Collection<TripDTO> tripsToDelete, ref TripDTO refTripDTO, ref PerDiemDTO refPerDiemDTO, ref LocationDTO refDepartureLocationDTO, ref LocationDTO refDestinationLocationDTO)
        {
            if (inTrip.Deleted)
            {
                TripDTO tripToDelete = _TripDTODataLoader.GetTripByTripID(inTrip.TripID);
                tripToDelete.Updateable = UpdateType.Deleted;

                tripsToDelete.Add(tripToDelete);
            }
            else if (!ModelState.IsValid)
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }
            else
            {
                TripDTO tripDTO = inTrip.GetTripDTO();

                if (tripDTO.LockedRate == true) { throw new ValidationException("This Trip has been locked by a workspace and cannot be updated"); }

                LocationDTO departureLocationDTO = inTrip.GetDepartureLocationDTO();
                LocationDTO destinationLocationDTO = inTrip.GetDestinationLocationDTO();
                PerDiemDTO perDiemDTO = inTrip.GetPerDiemDTO();

                // only check for trip uniquess if new trip. edit trip can't change any of the properties that make a trip unique.
                if (tripDTO.TripID < 0)
                {
                    int uniqueTripCount = _TripDTODataLoader.GetTripByUniqueTripDataUsingLocationNames(tripDTO.MiscTravelRateID, destinationLocationDTO.LocationName, departureLocationDTO.LocationName, perDiemDTO.Qualification).Count();

                    if (uniqueTripCount > 0)
                    {
                        throw new ValidationException("The data you have entered matches an existing trip definition.  To create a new trip you must change one of the following inputs:  Mode, Departure Location, Destination, or Qualification.");
                    }
                }

                if (perDiemDTO.LockedRate == true) { throw new ValidationException("Per Diem for this Trip has been locked by a workspace and cannot be updated"); }

                // check to see if per diem exists based on data since we can't trust '-1' as insert
                if (perDiemDTO.Id < 0)
                {
                    ICollection<PerDiemDTO> perDiems = _PerDiemDTODataLoader.GetAllPerDiem();
                    PerDiemDTO perDiemExists = (from p in perDiems
						where p.PerDiemDestination.ToLower().Equals(perDiemDTO.PerDiemDestination.ToLower()) &&
							p.Qualification.IsEquivalentTo(perDiemDTO.Qualification)
						select p).FirstOrDefault();

                    if (perDiemExists != null)
                    {
                        tripDTO.PerDiemID = perDiemExists.Id;
                        perDiemDTO.Id = perDiemExists.Id;
                        perDiemDTO.UpdateDate = perDiemExists.UpdateDate;
                    }
                }
                else
                {
                    // check to see if per diem is > 0 (which indicates an existing per diem is being used)
                    // BUT also ensure the qualification is the same for this per diem because the user
                    // may have overriden the qualification with their own value and thus will trigger a new
                    // per diem being saved in this case
                    PerDiemDTO perDiemFromDB = _PerDiemDTODataLoader.GetByIds(new Collection<int> { perDiemDTO.Id }).First();
                    string perdiemDTOQualString = perDiemDTO.Qualification ?? string.Empty;
                    string perdiemFromDBQualString = perDiemFromDB.Qualification ?? string.Empty;
                    if (!perdiemDTOQualString.ToLower().Equals(perdiemFromDBQualString.ToLower()))
                    {
                        // user entered new qualification so we have a new per diem and we need to save as new
                        perDiemDTO.Id = -1;
                    }
                }

                //double check that the departure location doesn't exist already
                if (departureLocationDTO.Id < 0)
                {
                    LocationDTO location = (from t in _LocationDTODataLoader.GetAllLocations()
                                    where t.LocationName != null &&
                                          t.LocationName.Equals(departureLocationDTO.LocationName.Trim(), StringComparison.CurrentCultureIgnoreCase)
                                    select t).FirstOrDefault();

                    // there was actually a matching departure location so get it now
                    if (location != null)
                    {
                        departureLocationDTO = location;
                    }
                }

                //double check that the destination location doesn't exist already
                if (destinationLocationDTO.Id < 0)
                {
                    LocationDTO location = (from t in _LocationDTODataLoader.GetAllLocations()
                                    where t.LocationName != null &&
                                          t.LocationName.Equals(destinationLocationDTO.LocationName.Trim(), StringComparison.CurrentCultureIgnoreCase)
                                    select t).FirstOrDefault();

                    // there was actually a matching departure location so get it now
                    if (location != null)
                    {
                        destinationLocationDTO = location;
                    }
                }

                // This has to be done because we cannot use objects w/ ref in queries and other places in this method
                refTripDTO = tripDTO;
                refDepartureLocationDTO = departureLocationDTO;
                refDestinationLocationDTO = destinationLocationDTO;
                refPerDiemDTO = perDiemDTO;
            }
        }

		/// <summary>
		/// Checks if users have access to GenBoe
		/// </summary>
		/// <param name="usersToCheck">Users to check</param>
		/// <returns>A dictionary of users and a bool indicating if they have access or not</returns>
		private Dictionary<UserData, bool> GetGenBOEAccess(ICollection<UserData> usersToCheck)
		{
			ICollection<GroupData> groups = this._ActiveDirectoryUtilities.GetAuthorizationGroupsFromWebConfig();

			Dictionary<UserData, bool> result = _ActiveDirectoryUtilities.CheckUsersBoeAccess(usersToCheck, groups);

			return result;
		}

		private Collection<PermissionsGridModelView> _GetPermissionsGrid(Role inRole)
		{
			// get the permissions dtos for each area
			Collection<PermissionsDTO> permissionsForSystem = this.PermissionsLoader.GetAdminPermissions();

			// combine them into one list
			List<PermissionsDTO> allPerms = new List<PermissionsDTO>();
			allPerms.AddRange(permissionsForSystem);

			//get rid of all the roles that aren't managed in this view.
			allPerms = allPerms
						.Where(onlyRolesWeCareAbout =>
							onlyRolesWeCareAbout.Role == inRole)
							.Select(onlyRolesWeCareAbout => onlyRolesWeCareAbout).ToList<PermissionsDTO>();

			Collection<PermissionsGridModelView> permissions = new Collection<PermissionsGridModelView>();

			List<int> distinctUsers = (from u in allPerms
									   select u.ETIUserId).Distinct().ToList();
			IDictionary<int, RoleModelView> roleModelViews = this._CommonDataMapper.getRolesDictionary();

			foreach (int distinctUser in distinctUsers)
			{
				// for each group grab the users for the group.  note this will include the 'null' group
				PermissionsGridModelView modelViewToAdd = new PermissionsGridModelView();

				UserDTO user = this.UserLoader.GetUserByID(distinctUser);
				modelViewToAdd.isGroup = user.NTID.Contains(".");
				modelViewToAdd.Users = new Collection<PermissionUserModelView>{
					(from u in allPerms
					where u.ETIUserId == distinctUser
					orderby u.ETIUserId
					select new PermissionUserModelView
					{
						DisplayName = user.DisplayName,
						UserID = distinctUser
					}).FirstOrDefault()};

				// convert the  role into the proper modelview class
				Collection<PermissionRoleModelView> rolesToAdd = new Collection<PermissionRoleModelView>();
				rolesToAdd.Add(new PermissionRoleModelView()
				{
					RoleID = (int)inRole,
					RoleName = roleModelViews[(int)inRole].RoleName
				});
				modelViewToAdd.Roles = rolesToAdd;

				permissions.Add(modelViewToAdd);
			} // foreach distinct user

			IEnumerable<PermissionsGridModelView> nonGroups = from t in permissions
															  from u in t.Users
															  orderby u.DisplayName
															  select t;

			List<PermissionsGridModelView> reGrouping = new List<PermissionsGridModelView>();
			reGrouping.AddRange(nonGroups);

			return new Collection<PermissionsGridModelView>(reGrouping);

		} // end GetPermissionsGridForGroups

		#endregion
	}
}
 