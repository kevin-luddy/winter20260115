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
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ModelView.Clin;
    using GenBOE.ActionLogic.ModelView.Workspace;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenBOE.Web.Common;
    using GenBOE.Web.ModelView;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;
    using IES.Common.PickList;

    public class CLINController : GenBOEController
    {
        private Logger _log = new Logger(typeof(CLINController));

        private IBoeEmailer _Emailer;
        private IBOEStateMachine _BOEStateMachine;
        private IVariableSelectBOEtoSumCalculation _variableSelectBOEtoSumCalculation;
        private IBoeTaskElementRecalculation _BoeTaskElementRecalculation;
        private IWorkspaceVariableDTODataLoader _workspaceVariableLoader;
        private ICLINImporter _ClinImporter;
        private ICLINExporter _ClinExporter;
        private IBoeTaskElementMediator _BoeTaskElementMediator;
        private IBoeMediator _BoeMediator;
        private IValidationHelper _validationHelper;
        private IClinDTODataLoader clinLoader;
        private ICommonDataLoader _CommonDataLoader;
        private ContractTypeLoader contractTypeLoader;

        /// <summary>
        /// The constructor
        /// </summary>
        public CLINController(ISecurityAccess inSecurityAccess,
            ICommonDataMapper inCommonDataMapper,
            SiteMasterUtilities inSiteMasterUtilities,
            IBoeEmailer inEmailer,
            IBOEStateMachine inBOEStateMachine,
            IVariableSelectBOEtoSumCalculation inVarSelectBoeToSumCalculation,
            IBoeTaskElementRecalculation inBoeTaskElementRecalculation,
            IWorkspaceVariableDTODataLoader inWorkspaceVarLoader,
            ICLINImporter inClinImporter,
            ICLINExporter inClinExporter,
            IBoeTaskElementMediator inBoeTaskElementMediator,
            IBoeMediator inBoeMediator,
            IValidationHelper inValidationHelper,
            SystemMetrics inSystemMetrics,
            IFullObjectFactory factory,
            IClinDTODataLoader clinLoader,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader permissionLoader,
            IGenBOEControllerLogic inControllerLogic,
            ICommonDataLoader inCommonDataLoader,
            ContractTypeLoader contractTypeLoader
            )
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, userLoader, permissionLoader, inControllerLogic)
        {
            this._Emailer = inEmailer;
            this._BOEStateMachine = inBOEStateMachine;
            this._variableSelectBOEtoSumCalculation = inVarSelectBoeToSumCalculation;
            this._BoeTaskElementRecalculation = inBoeTaskElementRecalculation;
            this._workspaceVariableLoader = inWorkspaceVarLoader;
            this._ClinImporter = inClinImporter;
            this._ClinExporter = inClinExporter;
            this._BoeTaskElementMediator = inBoeTaskElementMediator;
            this._BoeMediator = inBoeMediator;
            this._validationHelper = inValidationHelper;
            this.clinLoader = clinLoader;
            this._CommonDataLoader = inCommonDataLoader;
            this.contractTypeLoader = contractTypeLoader;
        }

        /// <summary>
        /// Returns the ManageCLINs view
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public ViewResult Index(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this._log, "Index", SecurityPage.ManageCLINs, SecurityAuthorization.Read, ws, null);

            // Perform Action
            ViewResult toReturn = this.GetMasterView(WebConstants.VIEW_INDEX, workspace);

            // Finalize Action
            this.FinalizeAction(this._log, "Index", sw);
            return toReturn;
        }

        public JsonResult GetManageClinGridModel(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_GET_MANAGE_CLIN_MODEL, SecurityPage.ManageCLINs, SecurityAuthorization.Read, ws, null);

            ManageCLINGridModelView theModelView = new ManageCLINGridModelView();
            theModelView.HideContractType = SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST;
            theModelView.ContractTypeList = this.BuildContractTypeDropdownOptions(ws.Id);
            ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();
			// convert the DTOs to model views
			IOrderedEnumerable<FullClin> clins = ws.ClinsNoMultiClin.OrderBy(c => c.ClinPaddedNumber);

            foreach (FullClin clin in clins)
            {
                string contract = Utilities.GetPickListText(clin.ContractType, contractTypes, Constants.CONTRACT_TYPE_NOT_SET_STRING);
                theModelView.ClinResults.Add(new ManageCLINModelView(clin, contract));
            }

            // Finalize Action
            this.FinalizeAction(this._log, WebConstants.ACTION_GET_MANAGE_CLIN_MODEL, sw);

            return this.Json(theModelView);
        }

        /// <summary>
        /// Builds sorted SelectList with only Contract Types selected at the workspace level and default option
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns>sorted SelectList with workspace Contract Types only</returns>
        private ICollection<PickListDto> BuildContractTypeDropdownOptions(int workspaceId)
        {
            //Contract Type dropdown includes workspace contract types only
            List<PickListDto> selectedContractTypesList = this._CommonDataLoader.GetSelectedContractTypes(workspaceId)
                .OrderBy(ct => ct.Text)
                .ToList();
            //Add 'Not Set' option for CLINs without contract type
            selectedContractTypesList.Insert(0, new PickListDto { Id = Constants.CONTRACT_TYPE_NOT_SET, Text = Constants.CONTRACT_TYPE_NOT_SET_STRING });
            return selectedContractTypesList;
        }

        /// <summary>
        /// Get the BOE count associated with the CLIN
        /// </summary>
        /// <param name="workspace">The Workspace.</param>
        /// <param name="ClinID">CLIN ID</param>
        /// <returns>number of BOEs associated with the CLIN</returns>
        public ActionResult GetBOECountForClin(string workspace, int ClinID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = this.InitializeAction(this._log, "GetBOECountForClin", SecurityPage.ManageCLINs, SecurityAuthorization.Read, ws, null);

            JsonResult toReturn;

            /** Valid Model Check */
            if (this.ModelState.IsValid)
            {
                // Perform Action
                DataRelationshipVerifier.VerifyDataRelation(this.Factory.CreateFullClin(ClinID), ws.Id);
                int result = this.clinLoader.GetBoeCountByClinID(ClinID);
                toReturn = this.Json(new { Status = result } );
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(this.ModelState));
            }

            // Finalize Action
            this.FinalizeAction(this._log, "GetBOECountForClin", sw);
            return toReturn;
        }

        /// <summary>
        /// Deletes a group of CLINs.
        /// </summary>
        /// <param name="workspace">The workspace the CLINs are assoicated with</param>
        /// <param name="inDeletedClins">Collection of deleted CLINs</param>
        /// <returns>If successfull,empty string is return. Otherwise, exception error text to be handled in the post:error </returns>
        public virtual JsonResult DeleteCLINs(string workspace, Collection<ManageCLINModelView> inDeletedClins)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, "DeleteCLINs", SecurityPage.ManageCLINs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            JsonResult toReturn = this.Json(new { Status = true });

            // inDeletedClins could be null if user tries to only delete CLINS that are in use.
            // in that case, we will not do anything.  There is already a warning that is displayed
            // to the user.
            if (inDeletedClins != null)
            {
                foreach (ManageCLINModelView deletedCLIN in inDeletedClins)
                {
                    // Only delete CLINs that have positive IDs
                    if (deletedCLIN.ClinID > 0 && deletedCLIN.Deleted == true)
                    {
                        this.SaveClin(workspace, deletedCLIN);
                    }
                }
            }
            // Finalize Action
            this.FinalizeAction(this._log, "DeleteCLINs", sw);

            return toReturn;
        }

        /// <summary>
        /// This function is called from the ManageCLINGrid partial view when the user selects "Save."
        /// This function will check if the input is valid and clean up any of the data if any newly 
        /// created CLINs is deleted=true which is not valid
        /// </summary>
        /// <param name="workspace">the workspace the CLINs are assoicated with</param>
        /// <param name="inUpdatedClin">collection of new or edited CLINs</param>
        /// <returns>If successfull,empty string is return. Otherwise, exception error text to be handled in the post:error </returns>
        public virtual JsonResult SaveClin(string workspace, ManageCLINModelView inUpdatedClin)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, "SaveClin", SecurityPage.ManageCLINs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            // Dictionary to keep track of task variable IDs that need to be updated and their old variable total
            Dictionary<int, decimal> WorkspaceVarOldValueD = new Dictionary<int, decimal>();

            // BOE's Labor Spread values that need to be recalculated because of any changes made during this save
            List<BoeTaskElementDTO> boeTaskElementsToRecalculate = new List<BoeTaskElementDTO>();

            // Perform Action
            // check if the input is null
            if (inUpdatedClin == null)
            {
                throw new ArgumentNullException(nameof(inUpdatedClin));
            }

            JsonResult toReturn = this.Json(new { Status = true });

            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

            // only validate data if it exists
            if (inUpdatedClin.ClinID > 0 || (inUpdatedClin.ClinID < 0 && inUpdatedClin.Deleted == false))
            {
                if (this.ModelState.IsValid)
                {
                    // Project updatedClin to be DTO
                    FullClin updatedClin = inUpdatedClin.ClinID > 0 ? this.Factory.CreateFullClin(inUpdatedClin.ClinID) : this.Factory.CreateFullClin();
                    updatedClin.ClinNumber = inUpdatedClin.ClinNumber;
                    updatedClin.ClinTitle = inUpdatedClin.ClinTitle;
                    updatedClin.Updateable = inUpdatedClin.Deleted ? UpdateType.Deleted : UpdateType.Upsert;
                    updatedClin.UpdateDate = inUpdatedClin.UpdateDate;
                    updatedClin.WorkspaceID = ws.Id;
                    updatedClin.ContractType = inUpdatedClin.ContractType;

                    // only apply dates if the CLIN is new or not in use
                    if (updatedClin.Id < 0 || !updatedClin.InUse)
                    {
                        updatedClin.StartDate = string.IsNullOrEmpty(inUpdatedClin.StartDate) ? (DateTime?)null : inUpdatedClin.StartDate.ToDateTimeMidMonth();
                        updatedClin.EndDate = string.IsNullOrEmpty(inUpdatedClin.EndDate) ? (DateTime?)null : inUpdatedClin.EndDate.ToDateTimeMidMonth();
                    }

                    // Don't bother validating deletions
                    if (updatedClin.Updateable != UpdateType.Deleted)
                    {
						// Validate CLIN
						CLINValidator validator = new CLINValidator(this.Factory);
						Collection<string> validationerrors = validator.validation(updatedClin, (Collection<Dictionary<string, string>>)null);

                        if (validationerrors.Count > 0)
                        {
                            foreach (string message in validationerrors)
                            {
                                ValidationErrors.Add(new ValidationMessage("Clin", message));
                            }
                        }

                        // validate dates if its a new clin
                        if (updatedClin.Id < 0)
                        {
                            string startEndDateValidation = this._validationHelper.StartEndDateValidation(updatedClin.StartDate, updatedClin.EndDate);

                            if (startEndDateValidation != null)
                            {
                                ValidationErrors.Add(new ValidationMessage("Clin", startEndDateValidation));
                            }
                        }
                    }

                    //get the potential multiboes
                    Collection<FullBoe> MultiBOEs = ws.Boes.Where(x => x.IsMultiClinWbs == true).ToCollection();
                   
                    //find any boes that have resources using the clin
                    Collection<FullBoe> boesUsingClin = (from b in MultiBOEs
	    from l in b.TaskElements
	    from x in l.taskElementLabors
	    where x.CLINID.HasValue && x.CLINID == updatedClin.Id
	    select b).ToCollection<FullBoe>();
                    //if there are duplicates lets filter those out.
                    boesUsingClin = boesUsingClin.Distinct().ToCollection<FullBoe>();

                    //if we have any boes and the clin is being deleted stop the process
                    if (boesUsingClin.Any() && updatedClin.Updateable == UpdateType.Deleted)
                    {
                        ValidationErrors.Add(new ValidationMessage("Clin", "'" + updatedClin.ClinString + "' cannot be deleted because it is being used by a Resource Type in a BOE that has Resource Level WBS/CLIN selected."));
                    }

                    if (ValidationErrors.Count > 0)
                    {
                        throw new GenValidationException(ValidationErrors);
                    }

                    ClinDTO oldClin = this.Factory.CreateFullClin(inUpdatedClin.ClinID);

                    // Save the CLINs
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                    {
                        ICollection<WorkspaceVariableDTO> workspaceVariablesOld = new Collection<WorkspaceVariableDTO>();
                        if (updatedClin.Updateable == UpdateType.Deleted)
                        {                               
                            if (updatedClin.WorkspaceVariableIds.Any())
                            {
                                workspaceVariablesOld = ws.WorkspaceVariables.Where(i => updatedClin.WorkspaceVariableIds.Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();
                                foreach (WorkspaceVariableDTO workspaceVar in workspaceVariablesOld)
                                {
                                    DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                                    data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);
                                
                                    decimal oldTotalValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
                                    WorkspaceVarOldValueD.Add(workspaceVar.Id, oldTotalValue);
                                }
                            }

                            boeTaskElementsToRecalculate.AddRange(this._BoeTaskElementRecalculation.RecalculateLaborWithClin(updatedClin, VariableType.Task, ws));
                            boeTaskElementsToRecalculate.AddRange(this._BoeTaskElementRecalculation.RecalculateLaborWithClin(updatedClin, VariableType.Workspace, ws));

                            // save all the task elements that were effected by a CLIN deletion
                            this._BoeTaskElementMediator.MediatedSaveTaskElements(new Collection<BoeTaskElementDTO>(boeTaskElementsToRecalculate), ws);
                        }

						// get the unique BOE IDs from boeTaskElementsToRecalculate so we can set their state back to Draft
						Collection<int> BoeIDsToCheck = new Collection<int>(boeTaskElementsToRecalculate.Where(x => x.BoeID > 0).Select(x => x.BoeID).ToList());
                        
                        ICollection<FullBoe> boesToCheck = this.Factory.CreateFullBoes(BoeIDsToCheck);
                        //add on the boes with the multiboes
                        boesToCheck = boesToCheck.Concat(boesUsingClin).ToCollection<FullBoe>();
                        

                        foreach (FullBoe boe in boesToCheck)
                        {
                            bool StateChange = false;
                            BOEState oldBOEState = boe.State;
                            BOEState newBOEState = BOEState.Draft;
                            if (boe.State == BOEState.Approved || boe.State == BOEState.AwaitingApproval || boe.State == BOEState.DraftLocked)
                            {
                                // Validate the Awaiting Approval or Approved to Draft state transition
                                string validationMessage = string.Empty;
                                if (!this._BOEStateMachine.PerformStateTransitionValidation(this.Factory.CreateFullBoe(boe), ws, oldBOEState, newBOEState, out validationMessage))
                                {
                                    throw new ValidationException(validationMessage);
                                }

                                // If the transition is valid, set the BOE to Draft and save it
                                boe.State = newBOEState;
                                StateChange = true;
                            }

                            if (StateChange)
                            {
                                boe.Updateable = UpdateType.Upsert;
                                this._BoeMediator.MediatedSave(ws, boe);
                                this._BOEStateMachine.PerformStateTransitionAction(this.Factory.CreateFullBoe(boe), ws, oldBOEState, boe.State);
                            }
                        }

                        this.clinLoader.Save(updatedClin);

                        if (WorkspaceVarOldValueD.Any())
                        {
                            foreach (WorkspaceVariableDTO workspaceVar in workspaceVariablesOld)
                            {
                                DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                                data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);
                            
                                workspaceVar.WorkspaceVariableValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
                                workspaceVar.Updateable = UpdateType.Upsert;
                                this._workspaceVariableLoader.SaveWorkspaceVariables(new Collection<WorkspaceVariableDTO> { workspaceVar });
                            }
                        }

                        scope.Complete();
                    }

                    // Send emails and change BOE statuses for In Use CLINs
                    // This is purposely outside the transaction so that if the email fails to send then the transaction does not fail
                    if (inUpdatedClin.ClinID > 0 && inUpdatedClin.Deleted == false)
                    {
                        this.ProcessInUseUpdatedCLIN(ws, oldClin, this.Factory.CreateFullClin(inUpdatedClin.ClinID));
                    }

                    if (inUpdatedClin.ClinID > 0 && inUpdatedClin.Deleted == false)
                    {
						// just make sure we have the latest version of the DTO. (the returned DTO has the updated ID, but not the updated UpdateDate)
						FullClin modifiedClin = this.Factory.CreateFullClin(updatedClin.Id);
                        ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();
                        string contract = Utilities.GetPickListText(modifiedClin.ContractType, contractTypes, Constants.CONTRACT_TYPE_NOT_SET_STRING);

                        toReturn = this.Json(new ManageCLINModelView(modifiedClin, contract));
                    }
                }
                else
                {
                    throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(this.ModelState));
                }
            }
            else
            {
                this._log.Info("No CLINs have been updated as part of SaveEditCLIN");
                toReturn = this.Json(new { Status = false });
            }

            // Finalize Action
            this.FinalizeAction(this._log, "SaveClin", sw);
            return toReturn;
        }

        /// <summary>
        /// Imports an Excel file and parses out CLINs to be imported. The results are passed back to the UI
        /// for acceptance by the user. They can then be passed back to CompleteImportCLINs to save the imported
        /// data into the system.
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns>JSON formatted results from import operation</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public JsonResult ImportCLINs(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = this.InitializeAction(this._log, "ImportCLINs", SecurityPage.ManageCLINs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            JsonResult toReturn = null;

            // If a file was uploaded successfully
            if (this.Request.Files.Count > 0 && this.Request.Files[0].FileName.Length > 0)
            {
                try
                {
                    ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();
					Collection<ImportedClin> results = this._ClinImporter.ImportClinsFromExcelFile(this.Request.Files[0].InputStream, ws, contractTypes);

                    toReturn = this.GenerateJsonUploadResponse(true, results, this.Request.Files[0].FileName);
                }
                // Catch custom exceptions from ExcelImporter and ResourcesImporter and generate friendly
                // exception messages to display for the user
                catch (NotExcelFileException)
                {
                    toReturn = this.GenerateJsonUploadResponse(false, "File is an invalid format. File must be in a MS Excel (.xlsx) format.");
                }
                catch (ColumnMissingException ex1)
                {
                    toReturn = this.GenerateJsonUploadResponse(false, "File does not contain all of the required columns. The following columns are missing: {0}.", ex1.Message);
                }
                catch (Exception ex0)
                {
                    this._log.Error(ex0, "Unknown Import Resources Error.");
                    toReturn = this.GenerateJsonUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
                }
            }
            // If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
            else
            {
                toReturn = this.GenerateJsonUploadResponse(false, "No file selected for upload");
            }

            // Finalize Action
            this.FinalizeAction(this._log, "ImportCLINs", sw);

            return toReturn;
        }

        /// <summary>
        /// Completes an import operation by saving valid imported CLINs into the DB.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="importResults">The CLIN import data to be saved</param>
        /// <returns>True</returns>
        public JsonResult CompleteImportCLINs(string workspace, Collection<ImportedClin> importResults)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, "CompleteImportCLINs", SecurityPage.ManageCLINs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JsonResult toReturn = this.Json(new { Status = true });

            if (importResults != null)
            {
				// Get NEW CLINs from the imported data
				IEnumerable<ImportedClin> newClinResults = from x in importResults
                                     where x.ImportTypes.Contains(ClinImportResult.CreateClin)
                                     select x;

				// Get EXISTING UPDATED CLINs from the imported data
				List<ImportedClin> updatedClinResults = (from x in importResults
                                         where x.ImportTypes.Contains(ClinImportResult.UpdateClin)
                                         select x).ToList();

				// Initialize the collection to hold CLINs that will be saved.
				Collection<ClinDTO> clinToSave = new Collection<ClinDTO>();

                // Start the collection off with all of the new CLINs from the imported data
                foreach (ImportedClin newClin in newClinResults)
                {
                    // Call Long Tick Properties with their own values to force DateTimes to be set properly
                    newClin.StartDateLong = newClin.StartDateLong;
                    newClin.EndDateLong = newClin.EndDateLong;

                    // the Json import results converts & to &amp which needs to be returned back to &
                    newClin.ClinNumber = newClin.ClinNumber.Contains("&amp;") ? newClin.ClinNumber.Replace("&amp;", "&") : newClin.ClinNumber;
                    newClin.ClinTitle = newClin.ClinTitle.Contains("&amp;") ? newClin.ClinTitle.Replace("&amp;", "&") : newClin.ClinTitle;

                    clinToSave.Add(newClin);
                }

				// Create a dictionary to hold all of the updated CLINs before they are updated in the DB
				Dictionary<int, ClinDTO> oldClinForEmailDict = new Dictionary<int, ClinDTO>();

                foreach (ImportedClin updatedClin in updatedClinResults)
                {
					FullClin oldClin = this.Factory.CreateFullClin(updatedClin.Id);

                    // Stash 'old' CLIN away before changes are applied to the DB
                    oldClinForEmailDict.Add(oldClin.Id, oldClin);

                    // the Json import results converts & to &amp which needs to be returned back to &
                    oldClin.ClinNumber = updatedClin.ClinNumber.Contains("&amp") ? updatedClin.ClinNumber.Replace("&amp;", "&") : updatedClin.ClinNumber;
                    oldClin.ClinTitle = updatedClin.ClinTitle.Contains("&amp") ? updatedClin.ClinTitle.Replace("&amp;", "&") : updatedClin.ClinTitle;


                    // If there are any date errors, we'll leave the dates alone and use old values
                    if (!(updatedClin.ImportTypes.Contains(ClinImportResult.NoDatesOrBothDatesRequired) ||
                          updatedClin.ImportTypes.Contains(ClinImportResult.StartDateMustBeBeforeEndDate) ||
                          updatedClin.ImportTypes.Contains(ClinImportResult.InvalidStartDateFormat) ||
                          updatedClin.ImportTypes.Contains(ClinImportResult.StartDateTooEarly) ||
                          updatedClin.ImportTypes.Contains(ClinImportResult.InvalidEndDateFormat) ||
                          updatedClin.ImportTypes.Contains(ClinImportResult.EndDateTooLate)))
                    {
                        oldClin.StartDate = updatedClin.StartDateLong.HasValue ? new DateTime(updatedClin.StartDateLong.Value) : (DateTime?)null;
                        oldClin.EndDate = updatedClin.EndDateLong.HasValue ? new DateTime(updatedClin.EndDateLong.Value) : (DateTime?)null;
                    }

                    oldClin.ContractType = updatedClin.ContractType;

                    oldClin.Updateable = UpdateType.Upsert;

                    // Do not set the update date, use the one from the database so that it will always be the newest version.
                    // We do not want optomistic locking during the import, as directed by the SE.
                    clinToSave.Add(oldClin);
                }

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    // Save the CLINs
                    this.clinLoader.Save(clinToSave);

                    // at this point all commits have taken place and were succesful (or an exception would have been thrown)
                    // so let's fire off emails, if applicable
                    foreach (ImportedClin updatedClin in updatedClinResults)
                    {
                        // look at the BOEs related to the CLIN
                        // if the CLIN was in use and the BOE is in DRAFT state, send the email

                        // Send emails and change BOE statuses for In Use CLINs
                        this.ProcessInUseUpdatedCLIN(ws, oldClinForEmailDict[updatedClin.Id], this.Factory.CreateFullClin(updatedClin));
                    }

                    scope.Complete();
                }

            }

            // Finalize Action
            this.FinalizeAction(this._log, "CompleteImportCLINs", sw);

            return toReturn;
        }

        /// <summary>
        /// Checks an in use CLIN that was updated for changes. If there were changes, then authors and approvers of
        /// the CLIN's BOEs are notified accordingly. Awaiting Approval/Approved BOEs are moved back to draft
        /// </summary>
        /// <param name="currentWorkspace">The current workspace</param>
        /// <param name="oldCLIN">The old CLIN DTO</param>
        /// <param name="newCLIN">The new CLIN DTO</param>
        private void ProcessInUseUpdatedCLIN(FullWorkspace currentWorkspace, ClinDTO oldCLIN, FullClin newCLIN)
        {
            if (newCLIN.InUse) // check in use
            {
				IReadOnlyCollection<FullBoe> boesForUpdatedClin = newCLIN.Boes;

                foreach (FullBoe boeForClin in boesForUpdatedClin)
                {
                    // check boe state
                    if (boeForClin.State == BOEState.Draft || boeForClin.State == BOEState.DraftLocked || boeForClin.State == BOEState.AwaitingApproval || boeForClin.State == BOEState.Approved)
                    {
                        // CLIN was in use, BOE in draft state ... send the email!
                        bool updateNeeded = false;

                        // obtain the changed fields
                        Collection<FieldChanged> changed = new Collection<FieldChanged>();
                        if (oldCLIN.ClinNumber != newCLIN.ClinNumber)
                        {
                            updateNeeded = true;
                            changed.Add(new FieldChanged
                            {
                                Field = "CLIN #",
                                OldValue = oldCLIN.ClinNumber,
                                NewValue = newCLIN.ClinNumber
                            });
                        }
                        if (oldCLIN.ClinTitle != newCLIN.ClinTitle)
                        {
                            updateNeeded = true;
                            changed.Add(new FieldChanged
                            {
                                Field = "CLIN Title",
                                OldValue = oldCLIN.ClinTitle,
                                NewValue = newCLIN.ClinTitle
                            });
                        }
                        if (oldCLIN.StartDate != newCLIN.StartDate)
                        {
                            if (newCLIN.StartDate > boeForClin.StartDate || newCLIN.StartDate > boeForClin.EndDate)
                            {
                                updateNeeded = true;
                            }
                            changed.Add(new FieldChanged
                            {
                                Field = "CLIN Start Date",
                                OldValue = oldCLIN.StartDate.HasValue ? oldCLIN.StartDate.Value.ToString("MM/yyyy") : string.Empty,
                                NewValue = newCLIN.StartDate.HasValue ? newCLIN.StartDate.Value.ToString("MM/yyyy") : string.Empty
                            });
                        }
                        if (oldCLIN.EndDate != newCLIN.EndDate)
                        {
                            if (newCLIN.EndDate < boeForClin.EndDate || newCLIN.EndDate < boeForClin.StartDate)
                            {
                                updateNeeded = true;
                            }
                            changed.Add(new FieldChanged
                            {
                                Field = "CLIN End Date",
                                OldValue = oldCLIN.EndDate.HasValue ? oldCLIN.EndDate.Value.ToString("MM/yyyy") : string.Empty,
                                NewValue = newCLIN.EndDate.HasValue ? newCLIN.EndDate.Value.ToString("MM/yyyy") : string.Empty
                            });
                        }
                        if (oldCLIN.ContractType != newCLIN.ContractType)
                        {
                            updateNeeded = true;
                            changed.Add(new FieldChanged
                            {
                                Field = "Contract Type",
                                OldValue = oldCLIN.ContractType.ToString(),
                                NewValue = newCLIN.ContractType.ToString()
                            });
                        }

                        if (boeForClin.State == BOEState.AwaitingApproval || boeForClin.State == BOEState.Approved || boeForClin.State == BOEState.DraftLocked)
                        {
                            BOEState currentState = boeForClin.State;
                            BOEState newBOEState = BOEState.Draft;
                            // move the BOE back to DRAFT
                            boeForClin.State = newBOEState;
                            boeForClin.Updateable = UpdateType.Upsert;

                            string errorMessage;
                            if (updateNeeded)
                            {
                                if (this._BOEStateMachine.PerformStateTransitionValidation(this.Factory.CreateFullBoe(boeForClin), currentWorkspace, currentState, boeForClin.State, out errorMessage))
                                {
                                    this._BoeMediator.MediatedSave(currentWorkspace, boeForClin);

                                    // save of BOE worked .. perform transition steps and send email
                                    this._BOEStateMachine.PerformStateTransitionAction(this.Factory.CreateFullBoe(boeForClin), currentWorkspace, currentState, boeForClin.State);
                                }
                                else
                                {
                                    // we can't move the BOE back to DRAFT for some reason ... abort
                                    // pull this error message from the state machine itself
                                    throw new ValidationException(errorMessage);
                                }
                            }
                        }

                        if (updateNeeded)
                        {
                            this._Emailer.SendCLINUpdatedToAuthorsAndApprovers(this.Factory.CreateFullBoe(boeForClin), changed);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Exports all CLINs for the workspace
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns>An ActionResult for the file being exported</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ActionResult ExportCLINs(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = this.InitializeAction(this._log, "ExportCLINs", SecurityPage.ManageCLINs, SecurityAuthorization.Read, ws, null);

            ActionResult toReturn = null;

            /** Valid Model Check */
            if (this.ModelState.IsValid)
            {
                //filter out multi boe's to hide from user. 
                Collection<FullClin> clins = ws.ClinsNoMultiClin.OrderBy(c => c.ClinNumber).ToCollection();

				// Get the CLIN template file name
				string templateFileName = this.Server.MapPath("~/Templates/Export/CLINs.xlsx");

                if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
                {
                    templateFileName = this.Server.MapPath("~/Templates/Export/CLINsRMS.xlsx");
                }
                ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();

				string exportFile = this._ClinExporter.ExportToExcelFile(templateFileName, clins, ws, contractTypes);

                if (exportFile.Length > 0)
                {
                    string fileName = string.Format("GenBOE-{0}-CLINs.xlsx", ws.WorkspaceName);
                    // Generate a custom ActionResult to cause a file download to the client
                    FileStream fs = new FileStream(exportFile, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                    // Finalize Action
                    FinalizeAction(_log, "ExportCLINs", sw);

                    toReturn = File(
                        fileStream: fs,
                        contentType: ExportFileDownloadBase.GetContentType(fileName),
                        fileDownloadName: fileName);
                }

            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(this.ModelState));
            }

            // Finalize Action
            this.FinalizeAction(this._log, "PageManageCLIN", sw);

            return toReturn;
        }
    }
}
