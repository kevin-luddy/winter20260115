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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Transactions;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using ActionLogic.ModelView.Clin;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.WBS;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenBOE.Web.Common;
    using GenBOE.Web.ModelView;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;
	using ImportWbsResultsModelView = ModelView.ImportWbsResultsModelView;
	using WBSControllerLogic = ActionLogic.ControllerLogic.Backend.WBSControllerLogic;

	public class WBSController : GenBOEController
    {
        private Logger _log = new Logger(typeof(WBSController));

        private NestedWBSUtilities _nestedWbsUtilities = null;
        private BoeEmailer _Emailer = null;
        private BOEStateMachine _BOEStateMachine = null;
        private VariableCircularReferenceChecker _variableCircularReferenceChecker = null;
        private WbsImporter _WbsImporter = null;
        private BoeMediator _BoeMediator = null;
        private WbsExporter _WbsExporter = null;
        private ValidationHelper _ValidationHelper = null;
        private IBoeDTODataLoader boeLoader;
        private IWbsDTODataLoader wbsLoader;
        private const string wbsIDColumn = "genBOE WBS ID";
        private IWBSControllerLogic _WbsControllerLogic = null;

		/// <summary>
		/// Backend WBS Controller Logic
		/// </summary>
		private WBSControllerLogic wbsControllerLogic { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public WBSController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper,
            SiteMasterUtilities inSiteMasterUtilities,
            NestedWBSUtilities inNestedWBSUtilities,
            BoeEmailer inEmailer,
            BOEStateMachine inBOEStateMachine,
            VariableCircularReferenceChecker inVariableCircularReferenceChecker,
            WbsImporter inWbsImporter,
            WbsExporter inWbsExporter,
            BoeMediator inBoeMediator,
            ValidationHelper inValidationHelper,
            SystemMetrics inSystemMetrics,
            IFullObjectFactory factory,
            IBoeDTODataLoader boeLoader,
            IWbsDTODataLoader wbsLoader,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader permissionLoader,
            IGenBOEControllerLogic inControllerLogic,
            IWBSControllerLogic inWBSControllerLogic
			,WBSControllerLogic wbsControllerLogic
			)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, userLoader, permissionLoader, inControllerLogic)
        {
            _BOEStateMachine = inBOEStateMachine;
            _Emailer = inEmailer;
            _nestedWbsUtilities = inNestedWBSUtilities;
            _variableCircularReferenceChecker = inVariableCircularReferenceChecker;
            _WbsImporter = inWbsImporter;
            _BoeMediator = inBoeMediator;
            _WbsExporter = inWbsExporter;
            _ValidationHelper = inValidationHelper;
            this.boeLoader = boeLoader;
            this.wbsLoader = wbsLoader;
			this._WbsControllerLogic = inWBSControllerLogic;
			this.wbsControllerLogic = wbsControllerLogic;
		}

        /// <summary>
        /// Returns the ManageWBSs view
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public ViewResult Index(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "Index", SecurityPage.ManageWBS, SecurityAuthorization.Read, ws, null);

            ViewData["WorkspaceID"] = ws.Id;

            // Perform Action
            ViewResult toReturn = GetMasterView(WebConstants.VIEW_INDEX, workspace);

            // Finalize Action
            FinalizeAction(_log, "Index", sw);
            return toReturn;
        }

        public ActionResult GetBOECountForWBS(string workspace, int WbsID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            FullWbs wbsObject = this.Factory.CreateFullWbs(WbsID);

            Stopwatch sw = InitializeAction(_log, "GetBOECountForWBS", SecurityPage.ManageWBS, SecurityAuthorization.Read, ws, null);

            // Perform Action
            int result = wbsObject.Boes.Count;

            JsonResult toReturn = Json(new { Status = false });

            /** Valid Model Check */
            if (ModelState.IsValid)
            {
                toReturn = Json(new { Status = result });
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            // Finalize Action
            FinalizeAction(_log, "GetBOECountForWBS", sw);
            return toReturn;

        }

        #region Display

        /// <summary>
        /// Get the Manage WBS Grid MV
        /// </summary>
        /// <param name="workspace">the workspace</param>
        /// <returns>The MV for the Manage WBS grid</returns>
        public JsonResult GetManageWBSGridModel(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_GET_MANAGE_WBS_MODEL, SecurityPage.ManageWBS, SecurityAuthorization.Read, ws, null);

            // convert the collection of WBS DTO to a collection of ManageWBSModelView
            Collection<WbsDTO> wbsDTOs = ws.WbsElementsNoMultiWbs.ToCollection<WbsDTO>();

            ManageWBSGridModelView model = new ManageWBSGridModelView();
            model.ContainsOCI = ws.ContainsOCI;
            model.AvailableClins = ws.ClinsNoMultiClin.OrderBy(x => x.ClinNumber).Select(c => new ManageCLINModelView(c, string.Empty)).ToList();

            _nestedWbsUtilities.AdjustLevels(wbsDTOs);

            Collection<int> wbsids = GetWBSUsedByWorkspaceVariables(ws);
            ICollection<int> wbsInBoes = GetWbsUsedInBoes(ws);

            foreach (WbsDTO wbs in wbsDTOs)
            {
                Collection<ClinDTO> clins = ws.Clins.Where(x => wbs.ClinIDs.Contains(x.Id)).ToCollection<ClinDTO>();

                ManageWBSModelView wbsModelView = new ManageWBSModelView(wbs, clins);

                if (wbsids.Contains(wbs.Id))
                {
                    wbsModelView.InUse = true;
                }

                if (wbsInBoes.Contains(wbs.Id))
                {
                    wbsModelView.HasBOE = true;
                }

                wbsModelView.ParentOrChildHasBoe = this.ParentOrChildHasBoe(wbs, ws);

                model.WbsResults.Add(wbsModelView);
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_GET_MANAGE_WBS_MODEL, sw);

            return this.Json(model);
        }

        private Collection<int> GetWBSUsedByWorkspaceVariables(FullWorkspace workspace)
        {
            IReadOnlyCollection<WorkspaceVariableDTO> workspaceVariables = workspace.WorkspaceVariables;
            Collection<int> wbsids = new Collection<int>();

            foreach (WorkspaceVariableDTO workspaceVariable in workspaceVariables)
            {
                foreach (SelectBOEsToSum SelectedBOEsToSum in workspaceVariable.SelectedBOEsToSum)
                {
                    if (SelectedBOEsToSum.WBSID != null && !wbsids.Contains(SelectedBOEsToSum.WBSID.Value))
                    {
                        wbsids.Add(SelectedBOEsToSum.WBSID.Value);
                    }
                }
            }

            return wbsids;
        }

        /// <summary>
        /// Get the WBSs that are used by BOEs
        /// </summary>
        /// <param name="ws">The Workspace</param>
        /// <returns>The IDs of WBSs used by BOEs</returns>
        private ICollection<int> GetWbsUsedInBoes(FullWorkspace ws)
        {
            ICollection<int> toReturn = new Collection<int>();
            foreach(FullBoe boe in ws.Boes)
            {
                if(boe.WBSID != null)
                {
                    toReturn.Add((int)boe.WBSID);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Check if a parent WBS has a BOE
        /// </summary>
        /// <param name="wbs">wbs to check</param>
        /// <param name="ws">ws containing wbs</param>
        /// <returns>True if parent WBS has a BOE, otherwise false</returns>
        private bool ParentOrChildHasBoe(WbsDTO wbs, FullWorkspace ws)
        {
            ICollection<string> parentWbs = _nestedWbsUtilities.GetParentsWBSNumByChildWBS(wbs);
            foreach(string parentNumber in parentWbs)
            {
                WbsDTO parentDto = ws.WbsElements.FirstOrDefault(x => x.WbsNumber == parentNumber);
                if (parentDto != null && parentDto.inUse)
                {
                    return true;
                }
            }

            ICollection<WbsDTO> childWbs = wbsLoader.GetAllChildWbs(ws.Id, wbs.WbsNumber);
            foreach (WbsDTO childDto in childWbs)
            {
                if (childDto.inUse)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion Display

        #region AJAX Calls

        private struct ChangedValueContainer
        {
            public BoeDTO BOE { get; set; }
            public Collection<FieldChanged> Changes { get; set; }
        }

        /// <summary>
        /// Saves updates to ManageWBS
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="inManageWBSModelView"></param>
        /// <returns></returns>
        virtual public JsonResult SaveManageWBSUpdates(string workspace, Collection<ManageWBSModelView> wbsCollection)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "SaveManageWBSUpdates", SecurityPage.ManageWBS, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            if (wbsCollection == null)
            {
                throw new ArgumentNullException(nameof(wbsCollection));
            }

            // Perform Action
            JsonResult toReturn = Json(new { Status = true });            
            Collection<FullBoe> MultiBOEs = ws.Boes.Where(b => b.IsMultiClinWbs).ToCollection();
            // Validate WBS Unique Number and no circular references
            foreach (ManageWBSModelView manageWbsModelView in wbsCollection)
            {
                // if we're deleting the WBS, don't bother with the validation, just delete it
                if (manageWbsModelView.Deleted != true && !string.IsNullOrEmpty(manageWbsModelView.WbsNumber))
                {
                    string validationHelperResponse = _ValidationHelper.WBSRenumberValidation(ws.Id, manageWbsModelView.WbsID, manageWbsModelView.WbsNumber);

                    if (validationHelperResponse != null)
                    {
                        ModelState.AddModelError("WbsNumber", validationHelperResponse);
                    }
                }
                else if (manageWbsModelView.Deleted == true)
                {

                    bool inUsebyResource = false;
                    inUsebyResource = MultiBOEs.SelectMany(b => b.TaskElements).AsParallel().Any(l => l.taskElementLabors.Any(x => x.WBSID.HasValue && x.WBSID == manageWbsModelView.WbsID));

                    if (inUsebyResource)
                    {
                        ModelState.AddModelError("InUse", "'" + manageWbsModelView.WbsString + "' cannot be deleted because it is being used by a Resource Type in a BOE that has Resource Level WBS/CLIN selected.");
                    }
                }
            }
                        
            /** Valid Model Check */
            if (ModelState.IsValid)
            {
                _WbsControllerLogic.SaveManageWBS(wbsCollection, ws);
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }
            // Finalize Action
            FinalizeAction(_log, "SaveManageWBSUpdates", sw);
            return toReturn;
        }


        public JsonResult CreateBOEs(string workspace, Collection<int> selectedWbsIDs)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "CreateBOEs", SecurityPage.ManageWBS, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            if (selectedWbsIDs == null)
            {
                throw new ArgumentNullException(nameof(selectedWbsIDs));
            }

			wbsControllerLogic.CreateBOEs(ws, selectedWbsIDs.ToList());

            JsonResult toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, "CreateBOEs", sw);

            return toReturn;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ActionResult ExportWBS(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "ExportWBS", SecurityPage.ManageWBS, SecurityAuthorization.Read, ws, null);

            // Perform Action
            // Get Default Performing Orgs Data
            ICollection<FullWbs> WbsDTOs = ws.WbsElementsNoMultiWbs.ToCollection();

            Dictionary<int, string> clinStrings = new Dictionary<int, string>();

            foreach (FullWbs wbs in WbsDTOs)
            {
                ICollection<ClinDTO> clins = ws.Clins.Where(x => wbs.ClinIDs.Contains(x.Id)).ToList<ClinDTO>();

                string clinString = string.Join(", ", clins.Select(w => w.ClinNumber));
                clinStrings.Add(wbs.Id, clinString);
            }

            // Get WBS template file name
            string templateFileName = Server.MapPath("~/Templates/Export/WBSs.xlsx");

            // Call the export function in the business layer and get back the file name of the populated template.
            string exportedFileName = _WbsExporter.ExportToExcelFile(templateFileName, WbsDTOs.ToCollection<WbsDTO>(), clinStrings);

            string fileName = string.Format("GenBOE-{0}-WBSs.xlsx", ws.WorkspaceName);
            // Generate a custom ActionResult to cause a file download to the client
            FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            // Finalize Action
            FinalizeAction(_log, "ExportWBS", sw);

            return File(
                fileStream: fs,
                contentType: ExportFileDownloadBase.GetContentType(fileName),
                fileDownloadName: fileName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ActionResult ExportWBSTemplate(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "ExportWBSTemplate", SecurityPage.ManageWBS, SecurityAuthorization.Read, ws, null);

            // Perform Action

            // Get Default Performing Orgs Data
            Collection<WbsDTO> WbsDTOs = new Collection<WbsDTO>();

            Dictionary<int, string> clinStrings = new Dictionary<int, string>();

            // Get WBS template file name
            string templateFileName = Server.MapPath("~/Templates/Export/WBSs.xlsx");

            // Call the export function in the business layer and get back the file name of the populated template.
            string exportedFileName = _WbsExporter.ExportToExcelFile(templateFileName, WbsDTOs, clinStrings);

            string fileName = string.Format("GenBOE-{0}-WBSs.xlsx", ws.WorkspaceName);
            // Generate a custom ActionResult to cause a file download to the client
            FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            // Finalize Action
            FinalizeAction(_log, "ExportWBSTemplate", sw);

            return File(
                fileStream: fs,
                contentType: ExportFileDownloadBase.GetContentType(fileName),
                fileDownloadName: fileName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public ViewResult ImportWBS(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "ImportWBS", SecurityPage.ManageWBS, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            ViewResult toReturn = null;
            Collection<ImportWbsResultsModelView> theModelViews = new Collection<ImportWbsResultsModelView>();

            try
            {
                // Perform Action
                // If a file was uploaded successfully
                if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
                {
                    // Call the business layer to parse the uploaded file
                    // If the file was successfully parsed, add the results to the genBOE database
                    Collection<ImportedWbs> importResults = _WbsImporter.ImportWBSFromExcelFile(Request.Files[0].InputStream, ws);

					List<ImportedWbs> updatedWBSs = importResults.Where(w => w.ImportTypes.Contains(WbsImportResult.UpdateWbs)).ToList();

					VariableCircularReferenceCheckerCache cache = new VariableCircularReferenceCheckerCache();
                    foreach (ImportedWbs updatedWBS in updatedWBSs)
                    {
                        if (updatedWBS.Id > 0)
                        {
                            if (updatedWBS.WbsNumber.Length > 0)
                            {
                                // Validate chosen WBS Number for circular references
                                if (_variableCircularReferenceChecker.WBSRenumberCreatesCircularReference(cache, Factory.CreateFullWbs(updatedWBS), updatedWBS.WbsNumber, updatedWBSs.ToList<WbsDTO>(), ws))
                                {
                                    updatedWBS.ImportTypes.Remove(WbsImportResult.UpdateWbs);
                                    updatedWBS.ImportTypes.Add(WbsImportResult.CircularReferences);
                                }

                                // Validate renumbering
                                if (updatedWBS.ImportTypes.Any(x => x == WbsImportResult.UpdateWbs) && !updatedWBS.ImportTypes.Any(x => x == WbsImportResult.DeleteWbs))
                                {
                                    string validationHelperResponse = _ValidationHelper.WBSRenumberValidation(ws.Id, updatedWBS.Id, updatedWBS.WbsNumber);
                                    
                                    if (validationHelperResponse != null)
                                    {
                                        if (validationHelperResponse.Contains("parent"))
                                        {
                                            updatedWBS.ImportTypes.Remove(WbsImportResult.UpdateWbs);
                                            updatedWBS.ImportTypes.Add(WbsImportResult.ParentHasWbs);
                                        }
                                        else if (validationHelperResponse.Contains("child"))
                                        {
                                            updatedWBS.ImportTypes.Remove(WbsImportResult.UpdateWbs);
                                            updatedWBS.ImportTypes.Add(WbsImportResult.ChildHasWbs);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (ImportedWbs importResult in importResults)
                    {
                        foreach (WbsImportResult resultType in importResult.ImportTypes)
                        {
                            theModelViews.Add(new ImportWbsResultsModelView()
                            {
                                ImportType = (int)resultType,
                                WbsID = importResult.Id,
                                WbsNumber = importResult.WbsNumber,
                                WbsTitle = importResult.WbsTitle,
                                ClinID = importResult.ClinID,
                                ClinIDs = importResult.ClinIDs,
                                ClinNumber = importResult.ClinNumber,
                                ClinTitle = importResult.ClinTitle                                
                            });
                        }
                    }

                    // Find out what the user deleted from the Excel spreadsheet that is now going to be imported.
                    if (Request.Form["ImportType"] == "Existing")
                    {
                        this.FindWBSsToBeDeleted(workspace, theModelViews);
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
			IEnumerable<ImportWbsResultsModelView> dataToSave = from m in theModelViews
                             where m.ImportType == (int)WbsImportResult.CreateWbs || m.ImportType == (int)WbsImportResult.UpdateWbs || m.ImportType == (int)WbsImportResult.CreateBoe || m.ImportType == (int)WbsImportResult.DeleteWbs
                             select m;
            ViewData["SERIALIZED_DATA"] = serializer.Serialize(dataToSave);
            ViewData["DOCUMENT_DOMAIN"] = Request["documentDomain"];

            toReturn = View(WebConstants.VIEW_WBS_IMPORT_VERIFICATION, theModelViews);

            // Finalize Action
            FinalizeAction(_log, "ImportWBS", sw);

            return toReturn;
        }

        public JsonResult CompleteImportWBS(string workspace, Collection<ImportWbsResultsModelView> importResults)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "ImportWBS", SecurityPage.ManageWBS, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            JsonResult toReturn = Json(new { Status = true });

            if (importResults != null)
            {
				IEnumerable<ImportWbsResultsModelView> newWbsResults = from x in importResults
                                    where x.ImportType == (int)WbsImportResult.CreateWbs
                                    select x;

				IEnumerable<ImportWbsResultsModelView> updatedWbsResults = from x in importResults
                                        where x.ImportType == (int)WbsImportResult.UpdateWbs
                                        select x;

				IEnumerable<ImportWbsResultsModelView> boesToCreate = from x in importResults
                                   where x.ImportType == (int)WbsImportResult.CreateBoe
                                   select x;

				IEnumerable<ImportWbsResultsModelView> deletedWbsResults = from x in importResults
                                        where x.ImportType == (int)WbsImportResult.DeleteWbs
                                        select x;

                Collection<WbsDTO> wbssToSave = new Collection<WbsDTO>();
                Collection<BoeDTO> boeToSave = new Collection<BoeDTO>();

                foreach (ImportWbsResultsModelView newWbs in newWbsResults)
                {
                    WbsDTO newWbsDTO = newWbs.GetWBSDTO();
                    newWbsDTO.WorkspaceID = ws.Id;

                    // the Json import results converts & to &amp which needs to be returned back to &
                    newWbsDTO.WbsNumber = newWbsDTO.WbsNumber.Contains("&amp;") ? newWbsDTO.WbsNumber.Replace("&amp;", "&") : newWbsDTO.WbsNumber;
                    newWbsDTO.WbsTitle = newWbsDTO.WbsTitle.Contains("&amp;") ? newWbsDTO.WbsTitle.Replace("&amp;", "&") : newWbsDTO.WbsTitle;

                    wbssToSave.Add(newWbsDTO);
                }

                foreach (ImportWbsResultsModelView deletedWbs in deletedWbsResults)
                {
                    WbsDTO deletedWbsDTO = wbsLoader.GetById(deletedWbs.WbsID);
                    deletedWbsDTO.Updateable = UpdateType.Deleted;
                    wbssToSave.Add(deletedWbsDTO);
                }

                Dictionary<int, WbsDTO> oldWbsForEmailDict = new Dictionary<int, WbsDTO>();
                foreach (ImportWbsResultsModelView updatedWbs in updatedWbsResults)
                {
                    WbsDTO oldWbs = this.Factory.CreateFullWbs(updatedWbs.WbsID);
                    oldWbsForEmailDict.Add(oldWbs.Id, oldWbs); // stash 'old' away before changes are applied

                    // get it from cache again (this causes a clone, which keeps our original copy intact)
                    oldWbs = this.Factory.CreateFullWbs(updatedWbs.WbsID);

                    oldWbs = updatedWbs.GetWBSDTO(oldWbs);

                    // the Json import results converts & to &amp which needs to be returned back to &
                    oldWbs.WbsNumber = oldWbs.WbsNumber.Contains("&amp;") ? oldWbs.WbsNumber.Replace("&amp;", "&") : oldWbs.WbsNumber;
                    oldWbs.WbsTitle = oldWbs.WbsTitle.Contains("&amp;") ? oldWbs.WbsTitle.Replace("&amp;", "&") : oldWbs.WbsTitle;

                    // Do not set the update date, use the one from the database so that it will always be the newest version.
                    // We do not want optimistic locking during the import, as directed by the SE.
                    wbssToSave.Add(oldWbs);
                }

				// Check for circular references before saving
				VariableCircularReferenceCheckerCache cache = new VariableCircularReferenceCheckerCache();
                foreach (WbsDTO updatedWBS in wbssToSave)
                {
                    if (updatedWBS.Id > 0)
                    {
                        if (updatedWBS.WbsNumber.Length > 0)
                        {
                            // Validate chosen WBS Number for circular references
                            if (_variableCircularReferenceChecker.WBSRenumberCreatesCircularReference(cache, Factory.CreateFullWbs(updatedWBS), updatedWBS.WbsNumber, wbssToSave, ws))
                            {
                                // If a circular reference is found, don't save the offending WBS
                                updatedWBS.Updateable = UpdateType.None;
                            }
                        }
                    }
                }

                int newId = -1;
                foreach (ImportWbsResultsModelView newBoe in boesToCreate)
                {
                    BoeDTO newBoeDTO = newBoe.GetBOEDTO();
                    newBoeDTO.WorkspaceID = ws.Id;
                    // make every id unique for new boes
                    newBoeDTO.Id = newId--;
                    newBoeDTO.StartDate = ws.ContractStartDate;
                    newBoeDTO.EndDate = ws.ContractEndDate;

                    boeToSave.Add(newBoeDTO);
                }

                List<ChangedValueContainer> SendBOEUpdatedToAuthorsAndApprovers = new List<ChangedValueContainer>();
                IDictionary<int, int> newBoeIds;
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    this.wbsLoader.Save(wbssToSave);

                    // During an import, BOEs will only be created for WBSs that 1) Exist and 2) have BOEs already,
                    // so we don't care about any WBS IDs that changed during the save.

                    // get updated WCB IDs
                    foreach (BoeDTO boe in boeToSave)
                    {
                        boe.WCBID = this.boeLoader.GetWbsClinBoeXrefId(boe.WBSID, boe.CLINID, null);
                    }

                    newBoeIds = _BoeMediator.MediatedSaveBOEs(ws, boeToSave);

                    // at this point all commits have taken place and were successful (or an exception would have been thrown)
                    // so let's fire off emails, if applicable
                    ICollection<FullWbs> wbsObjects = this.Factory.CreateFullWbses(updatedWbsResults.Select(x => x.WbsID).ToList());

                    foreach (ImportWbsResultsModelView updatedWbs in updatedWbsResults)
                    {
                        // look at the BOEs related to the WBS
                        // if the WBS was in use and the BOE is in DRAFT state, send the email
                        FullWbs newWBS = wbsObjects.First(x => x.Id == updatedWbs.WbsID);

                        if (newWBS.inUse) // check in use
                        {
                            foreach (FullBoe boeForWBS in newWBS.Boes)
                            {
                                // check boe state
                                if (boeForWBS.State == BOEState.Draft || boeForWBS.State == BOEState.DraftLocked || boeForWBS.State == BOEState.AwaitingApproval || boeForWBS.State == BOEState.Approved)
                                {
                                    // wbs was in use, BOE in draft state ... send the email!

                                    // obtain the changed fields
                                    Collection<FieldChanged> changed = new Collection<FieldChanged>();
                                    if (oldWbsForEmailDict[updatedWbs.WbsID].WbsNumber != newWBS.WbsNumber)
                                    {
                                        changed.Add(new FieldChanged
                                        {
                                            Field = "WBS #",
                                            OldValue = oldWbsForEmailDict[updatedWbs.WbsID].WbsNumber,
                                            NewValue = newWBS.WbsNumber
                                        });
                                    }
                                    if (oldWbsForEmailDict[updatedWbs.WbsID].WbsTitle != newWBS.WbsTitle)
                                    {
                                        changed.Add(new FieldChanged
                                        {
                                            Field = "WBS Title",
                                            OldValue = oldWbsForEmailDict[updatedWbs.WbsID].WbsTitle,
                                            NewValue = newWBS.WbsTitle
                                        });
                                    }

                                    if (boeForWBS.State == BOEState.AwaitingApproval || boeForWBS.State == BOEState.Approved || boeForWBS.State == BOEState.DraftLocked)
                                    {
                                        BOEState currentState = boeForWBS.State;
                                        BOEState newBOEState = BOEState.Draft;
                                        // move the BOE back to DRAFT
                                        boeForWBS.State = newBOEState;
                                        boeForWBS.Updateable = UpdateType.Upsert;

                                        string errorMessage;
                                        if (_BOEStateMachine.PerformStateTransitionValidation(boeForWBS, ws, currentState, boeForWBS.State, out errorMessage))
                                        {
                                            _BoeMediator.MediatedSave(ws, boeForWBS);

                                            // save of BOE worked .. perform transition steps and send email
                                            _BOEStateMachine.PerformStateTransitionAction(boeForWBS, ws, currentState, boeForWBS.State);

                                            SendBOEUpdatedToAuthorsAndApprovers.Add(new ChangedValueContainer
                                            {
                                                BOE = boeForWBS,
                                                Changes = changed
                                            });
                                        }
                                        else
                                        {
                                            // we can't move the BOE back to DRAFT for some reason ... abort
                                            // pull this error message from the state machine itself
                                            throw new ValidationException(errorMessage);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    scope.Complete();
                }
                // database save complete, perform email sends now

                foreach (ChangedValueContainer SendBOEUpdatedToAuthorsAndApprover in SendBOEUpdatedToAuthorsAndApprovers)
                {
                    this._Emailer.SendBOEUpdatedToAuthorsAndApprovers(this.Factory.CreateFullBoe(SendBOEUpdatedToAuthorsAndApprover.BOE), SendBOEUpdatedToAuthorsAndApprover.Changes);
                }

                // Send out email for new BOEs
                ICollection<FullBoe> FullBoes = this.Factory.CreateFullBoes(newBoeIds.Values);
                foreach (FullBoe fullBoe in FullBoes)
                {
                    // It is a new BOE, and at least WBS was set 
                    this._Emailer.SendBOECLINWBSChanged(fullBoe, fullBoe.CLINID.HasValue, fullBoe.WBSID.HasValue, false);
                }
            }

            // Finalize Action
            FinalizeAction(_log, "ImportWBS", sw);

            return toReturn;
        }

        /// <summary>
        /// Create a collection of WBSs to be deleted by comparing what rows were deleted from the exported (and
        /// then imported) Excel spreadsheet against what WBSs belong to the workspace in the database.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="importResults"></param>
        public void FindWBSsToBeDeleted(string workspace, Collection<ImportWbsResultsModelView> importResults)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Get what's currently in the workspace from the database.
            ICollection<FullWbs> wbsDTOs = ws.WbsElementsNoMultiWbs.ToCollection();

            // Create another collection to be able to write to it.
            Collection<FullWbs> wbssToDelete = new Collection<FullWbs>();

            // Get all the rows from the imported Excel spreadsheet.
            ICollection<Dictionary<string, string>> allImportRows = _WbsImporter.GetImportWBSFromExcelFileCount(Request.Files[0].InputStream).ToList();

            // Get all WBS IDs from the spreadsheet.
            List<string> ssIds = new List<string>();

            foreach (Dictionary<string, string> dict in allImportRows)
            {
                if (dict.ContainsKey(wbsIDColumn))
                {
                    ssIds.Add(dict[wbsIDColumn]);
                }
            }

            // Add rows to be deleted to the "Import WBS" page displayed to the user.
            foreach (FullWbs checkRow in wbsDTOs)
            {
                if (!(ssIds.Contains((checkRow.Id).ToString())))
                {
                    if (!checkRow.inUse)
                    {
                        wbssToDelete.Add(checkRow);
                        if (importResults != null)
                        {
                            // Add WBS IDs to be deleted.
                            importResults.Add(new ImportWbsResultsModelView()
                            {
                                ImportType = (int)WbsImportResult.DeleteWbs,
                                WbsID = checkRow.Id,
                                WbsNumber = checkRow.WbsNumber,
                                WbsTitle = checkRow.WbsTitle
                            });
                        }
                    }
                    else if (checkRow.inUse)
                    {
                        if (importResults != null)
                        {
                            importResults.Add(new ImportWbsResultsModelView()
                            {
                                ImportType = (int)WbsImportResult.WbsInUse,
                                WbsID = checkRow.Id,
                                WbsNumber = checkRow.WbsNumber,
                                WbsTitle = checkRow.WbsTitle
                            });
                        }
                    }
                }
            }
        }

        #endregion
    }
}
