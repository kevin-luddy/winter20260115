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
	using System.Configuration;
	using System.Diagnostics;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Transactions;
	using System.Web.Mvc;
	using ActionLogic.ModelView.BOE;
	using DataBridge.Reference;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.Validation;
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
	using IES.Common.PickList;
    using Microsoft.VisualBasic.Logging;

	public class BOEFormController : GenBOEController
    {
        Logger log = new Logger(typeof(BOEFormController));
        private IBOEFormControllerLogic boeFormControllerLogic = null;
        private IResourceDTODataLoader resourceDTODataLoader = null;
        private InUseDataLoader inUseDataLoader = null;
        private ContractTypeLoader contractTypeLoader = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public BOEFormController(ISecurityAccess securityAccess,
            ICommonDataMapper commonDataMapper,
            SiteMasterUtilities siteMasterUtilities,
            SystemMetrics systemMetrics,
            IFullObjectFactory fullObjectFactory,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader permissionsLoader,
            IGenBOEControllerLogic genBOEontrollerLogic,
            IBOEFormControllerLogic boeFormControllerLogic,
            IResourceDTODataLoader resourceDTODataLoader,
            InUseDataLoader inUseDataLoader,
            ContractTypeLoader contractTypeLoader
             )
            : base(securityAccess, commonDataMapper, siteMasterUtilities, systemMetrics, fullObjectFactory, userLoader, permissionsLoader, genBOEontrollerLogic)
        {
            this.boeFormControllerLogic = boeFormControllerLogic;
            this.resourceDTODataLoader = resourceDTODataLoader;
            this.inUseDataLoader = inUseDataLoader;
            this.contractTypeLoader = contractTypeLoader;
        }

        #region public methods

        /// <summary>
        /// Validate the INL Form Resources to make sure the corresponding T&amp;M Resource Rates are valid.
        /// </summary>
        /// <param name="workspace">A workspace to validate boe forms against.</param>
        /// <param name="i">IBOE form IDs to validate</param>
        /// <param name="p">PBOE form IDs to validate</param>
        /// <returns></returns>
        public JsonResult ValidateBOEFormTMResources(string workspace, Collection<int> i, Collection<int> p)
        {
            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            Collection<int> validatedResourceIds = new Collection<int>();
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            this.boeFormControllerLogic.ValidateBOEFormsTMResources(validationErrors, validatedResourceIds, ws, i, p);
            return this.Json(new { status = !validationErrors.Any(), validationErrors });  
        }

        /// <summary>
        /// Exports a BOE to a pre-formatted MS Word template and sends the file as a download
        /// to the user.
        /// </summary>
        /// <returns>A special ActionResult that generates a file download for the user to download the
        /// populated Word template.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public ActionResult ExportBOEForm(string workspace, Collection<int> i, Collection<int> p)
        {
            ActionResult result;
            try
            {
                if ((i == null || i.Count == 0) && (p == null || p.Count == 0))
                {
                    throw new ArgumentException("There must be at least one BOE Form selected to export.");
                }

                FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
				Stopwatch sw = InitializeAction(log, "ExportBOEForm", SecurityPage.ManageBOEForms, SecurityAuthorization.Read, ws, null);

                bool isSubcontractorUser = (from pr in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
                                            where pr.Role == Role.SubcontractorAuthor && pr.ETIUserId == ws.CurrentActiveUser.UserID
                                            select pr).Any();

                if (isSubcontractorUser)
                {
                    throw new AuthorizationException("Subcontractors are not allowed to export BOE Forms.");
                }

                // Array of filenames returned from Export. The first entry is the physical file location, and the second is the proposed file name.
                string[] fileNames;

                // reduces null check complexity below
                if (i == null)
                {
                    i = new Collection<int>();
                }
                if (p == null)
                {
                    p = new Collection<int>();
                }

                bool isPortionMarkingEnabled = SiteMasterUtilities.IsPortionMarkingEnabled;
                ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();
                if ((!i.Any() && p.Count == 1))
                {
                    // only exporting one PBOE, just return the form itself
                    fileNames = this.boeFormControllerLogic.ExportBOEFormReport(ws, p.First(), BOEFormType.PBOE, isPortionMarkingEnabled, contractTypes);
                    
                    // Generate a custom ActionResult to cause a file download to the client
                    FileStream fs = new FileStream(fileNames[0], FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                    result = File(
                        fileStream: fs,
                        contentType: ExportFileDownloadBase.GetContentType(fileNames[1]),
                        fileDownloadName: fileNames[1]);
                }
                else if (!p.Any() && i.Count == 1)
                {
                    // only exporting one IBOE, just return the form itself
                    fileNames = this.boeFormControllerLogic.ExportBOEFormReport(ws, i.First(), BOEFormType.IBOE, isPortionMarkingEnabled, contractTypes);
                    // Generate a custom ActionResult to cause a file download to the client
                    FileStream fs = new FileStream(fileNames[0], FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                    result = File(
                        fileStream: fs,
                        contentType: ExportFileDownloadBase.GetContentType(fileNames[1]),
                        fileDownloadName: fileNames[1]);
                }
                else
                {
                    Collection<string> filesToRemove = new Collection<string>();

                    // zip the multiple returns up
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                        {
                            foreach (int iboeId in i)
                            {
                                fileNames = this.boeFormControllerLogic.ExportBOEFormReport(ws, iboeId, BOEFormType.IBOE, isPortionMarkingEnabled, contractTypes);
                                archive.CreateEntryFromFile(fileNames[0], fileNames[1]);
                                filesToRemove.Add(fileNames[0]);
                            }

                            foreach (int pboeId in p)
                            {
                                fileNames = this.boeFormControllerLogic.ExportBOEFormReport(ws, pboeId, BOEFormType.PBOE, isPortionMarkingEnabled, contractTypes);
                                archive.CreateEntryFromFile(fileNames[0], fileNames[1]);
                                filesToRemove.Add(fileNames[0]);
                            }
                        }

                        result = File(memoryStream.ToArray(), "application/zip", "INL_Form_Exports.zip");

                        // cleanup temporary files
                        foreach(string file in filesToRemove)
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                }

                FinalizeAction(log, "ExportBOEForm", sw);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                result = this.CreateTextFileWithErrorMessage(ex);
            }

            return result;
        }

        /// <summary>
        /// Returns the ManageBOEs view
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns>ViewResult.</returns>
        public ViewResult Index(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "Index", SecurityPage.ManageBOEForms, SecurityAuthorization.Read, ws, null);

            // Perform Action
            ViewResult toReturn = GetMasterView(WebConstants.VIEW_INDEX, workspace);

            // Finalize Action
            FinalizeAction(log, "Index", sw);
            return toReturn;
        }

        /// <summary>
        /// Displays the "Create Form" for the BOE forms.
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns>ViewResult.</returns>
        public ViewResult CreateForm(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(log, "CreateForm", SecurityPage.ManageBOEForms, SecurityAuthorization.ReadUpdate, this.Factory.CreateFullWorkspace(workspace), null);

            HashSet<int> resourceIDsInUse = this.inUseDataLoader.GetWorkspaceResourceIDsInUseByListID(ws.ResourceListID);
            IOrderedEnumerable<ResourceDTO> resources = resourceDTODataLoader.GetByListId(ws.ResourceListID).Where(r => resourceIDsInUse.Contains(r.Id)).OrderBy(r => r.Segment).ThenBy(r => r.ResourceDesc);

            BOEFormMasterModelView modelView = new BOEFormMasterModelView
            {
                IWTAResources = resources.Where(r => r.ElementOfCost == ElementOfCostType.IWTA && (r.RateType == RateType.Cost || r.RateType == RateType.Hours)).ToCollection(),
                SubResources = resources.Where(r => r.ElementOfCost == ElementOfCostType.Sub && (r.RateType == RateType.Cost || r.RateType == RateType.Hours)).ToCollection(),
                // send in 0 for the boeFormId so that nothing is matched and the query sends back all distinct ids
                IWTAInUseResourceIds = this.boeFormControllerLogic.GetInUseResources(BOEFormType.IBOE, ws.Id, 0),
                SubInUseResourceIds = this.boeFormControllerLogic.GetInUseResources(BOEFormType.PBOE, ws.Id, 0),
                Clins = ws.ClinsNoMultiClin.OrderBy(c => c.ClinPaddedNumber).ToList(),
                ProposalTitleAndRfpNumber = ActionLogicUtility.GetProposalTitleAndRfpNumber(ws),
                IBOEModel = new BOEFormIBOEModelView { Version = this.boeFormControllerLogic.GetCurrentFormVersion(BOEFormType.IBOE) },
                PBOEModel = new BOEFormPBOEModelView { Version = this.boeFormControllerLogic.GetCurrentFormVersion(BOEFormType.PBOE) }
            };

            InitializeMasterViewModel(modelView, ws);

            modelView.InitialWorkspaceTitle = string.IsNullOrWhiteSpace(ws.ProposalTitle) ? ws.WorkspaceName : ws.ProposalTitle;

            FinalizeAction(log, "CreateForm", sw);
            return View(modelView);
        }

        /// <summary>
        /// Displays the View for a specific Boe FormType when Creating a Form.
        /// </summary>
        /// <param name="workspace">The workspace to create a form inside.</param>
        /// <param name="boeFormType">The Form Type to create.</param>
        /// <returns>A View used to create a BOE Form.</returns>
        public ViewResult CreateBoeForm(string workspace, BOEFormType boeFormType)
        {
            if (boeFormType == BOEFormType.NotSet)
            {
                throw new ArgumentException("boeFormType must be set.");
            }

            if (String.IsNullOrEmpty(workspace))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(log, "CreateBoeForm", SecurityPage.ManageBOEForms, SecurityAuthorization.ReadUpdate, this.Factory.CreateFullWorkspace(workspace), null);

            BOEFormMasterModelView modelView = new BOEFormMasterModelView
            {
                // send in 0 for the boeFormId so that nothing is matched and the query sends back all distinct ids
                IWTAInUseResourceIds = this.boeFormControllerLogic.GetInUseResources(BOEFormType.IBOE, ws.Id, 0),
                SubInUseResourceIds = this.boeFormControllerLogic.GetInUseResources(BOEFormType.PBOE, ws.Id, 0),
                Clins = ws.ClinsNoMultiClin.OrderBy(c => c.ClinPaddedNumber).ToList(),
                ProposalTitleAndRfpNumber = ActionLogicUtility.GetProposalTitleAndRfpNumber(ws),
                IBOEModel = new BOEFormIBOEModelView { Version = this.boeFormControllerLogic.GetCurrentFormVersion(BOEFormType.IBOE) },
                PBOEModel = new BOEFormPBOEModelView { Version = this.boeFormControllerLogic.GetCurrentFormVersion(BOEFormType.PBOE) }
            };

            ViewData["ContractTypes"] = this.contractTypeLoader.GetPickListValues();

            InitializeMasterViewModel(modelView, ws);

            modelView.InitialWorkspaceTitle = string.IsNullOrWhiteSpace(ws.ProposalTitle) ? ws.WorkspaceName : ws.ProposalTitle;
            string viewName = string.Format("Forms/{0}_{1}", boeFormType.GetDescription(), this.boeFormControllerLogic.GetCurrentFormVersion(boeFormType));
            
            FinalizeAction(log, "CreateBoeForm", sw);
            return View(viewName, modelView);
        }

        /// <summary>
        /// Displays the "Update Form" for the BOE forms.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="boeFormId"></param>
        /// <param name="boeFormType"></param>
        /// <returns>ViewResult.</returns>
        public ViewResult UpdateForm(string workspace, int boeFormId, BOEFormType boeFormType)
        {
            if (boeFormId<0)
            {
                throw new ArgumentException("boeFormId is required when editing a BOE Form.");
            }
            if (boeFormType == BOEFormType.NotSet)
            {
                throw new ArgumentException("boeFormType is required when editing a BOE Form.");
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(log, "CreateForm", SecurityPage.ManageBOEForms, SecurityAuthorization.Read, ws, null);

            HashSet<int> resourceIDsInUse = this.inUseDataLoader.GetWorkspaceResourceIDsInUseByListID(ws.ResourceListID);
            IOrderedEnumerable<ResourceDTO> resources = resourceDTODataLoader.GetByListId(ws.ResourceListID).Where(r => resourceIDsInUse.Contains(r.Id)).OrderBy(r => r.Segment).ThenBy(r => r.ResourceDesc);

            BOEFormMasterModelView modelView = new BOEFormMasterModelView
            {
                IWTAResources = resources.Where(r => r.ElementOfCost == ElementOfCostType.IWTA && (r.RateType == RateType.Cost || r.RateType == RateType.Hours)).ToCollection(),
                SubResources = resources.Where(r => r.ElementOfCost == ElementOfCostType.Sub && (r.RateType == RateType.Cost || r.RateType == RateType.Hours)).ToCollection(),
                // send in 0 for the boeFormId so that nothing is matched and the query sends back all distinct ids
                IWTAInUseResourceIds = this.boeFormControllerLogic.GetInUseResources(BOEFormType.IBOE, ws.Id, boeFormId),
                SubInUseResourceIds = this.boeFormControllerLogic.GetInUseResources(BOEFormType.PBOE, ws.Id, boeFormId),
                Clins = ws.ClinsNoMultiClin.OrderBy(c => c.ClinPaddedNumber).ToList(),
                ProposalTitleAndRfpNumber = ActionLogicUtility.GetProposalTitleAndRfpNumber(ws),
                BOEFormType = boeFormType
            };
            ViewData["ContractTypes"] = this.contractTypeLoader.GetPickListValues();

            InitializeMasterViewModel(modelView, ws);

            if (boeFormType == BOEFormType.IBOE)
            {
                modelView.IBOEModel = this.boeFormControllerLogic.GetIBOEForm(ws.Id, boeFormId, modelView.ProposalTitleAndRfpNumber);
            }
            else
            {
                modelView.PBOEModel = this.boeFormControllerLogic.GetPBOEForm(ws.Id, boeFormId, modelView.ProposalTitleAndRfpNumber);
            }

            FinalizeAction(log, "UpdateForm", sw);
            return View(modelView);
        }

        /// <summary>
        /// Starting Page for BOE Forms.
        /// </summary>
        /// <returns>ViewResult.</returns>
        public ViewResult DisplayManageBOEForms(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "ManageBOEForms", SecurityPage.ManageBOEForms, SecurityAuthorization.Read, ws, null);
            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_BOE_FORMS);
            // Finalize Action
            FinalizeAction(log, "ManageBOEForms", sw);
            return toReturn;
        }

        /// <summary>
        /// Displays Grid of Items.
        /// </summary>
        /// <returns>ViewResult.</returns>
        public ViewResult DisplayManageBOEFormsGrid(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "ManageBOEFormsGrid", SecurityPage.ManageBOEForms, SecurityAuthorization.Read, ws, null);

            ICollection<BOEFormModelView> forms = this.boeFormControllerLogic.GetSummaryForms(ws);
            ViewBag.IsUsingTM = ws.IsUsingTM;
            ViewResult toReturn = View(WebConstants.VIEW_MANAGE_BOE_FORMS_GRID, forms);

            // Action Finalize
            FinalizeAction(log, "ManageBOEFormsGrid", sw);

            return toReturn;
        }

        /// <summary>
        /// Deletes a group of BOE Forms.
        /// </summary>
        /// <param name="boeFormsVM">Collection of BOE Forms marked for deletion</param>
        /// <returns>If successfull,empty string is return. Otherwise, exception error text to be handled in the post:error </returns>
        virtual public JsonResult DeleteBOEForms(string workspace, Collection<BOEFormModelView> boeFormsVM)
        {
            if (boeFormsVM == null)
            {
                throw new ArgumentNullException(nameof(boeFormsVM));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(log, WebConstants.ACTION_DELETE_BOE_FORMS, SecurityPage.ManageBOEForms, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                foreach (BOEFormModelView deletedForm in boeFormsVM.Where(x => x.BOEFormId > 0 && x.UpdateType == UpdateType.Deleted))
                {
                    if (deletedForm.BOEFormType == BOEFormType.IBOE)
                    {
                        this.boeFormControllerLogic.DeleteBOEFormIBOE(deletedForm.BOEFormId);
                    }
                    else if (deletedForm.BOEFormType == BOEFormType.PBOE)
                    {
                        this.boeFormControllerLogic.DeleteBOEFormPBOE(deletedForm.BOEFormId);
                    }
                }
                scope.Complete();
            }
            FinalizeAction(log, WebConstants.ACTION_DELETE_BOE_FORMS, sw);

            return Json(new { Status = true }); // everything ok!
        }

        /// <summary>
        /// Saves an IBOE (create and update).
        /// </summary>
        /// <param name="workspace">The workspace for the IBOE.</param>
        /// <param name="boeFormsVM">The view model for the IBOE.</param>
        /// <returns>JSON status message.</returns>
        public virtual JsonResult SaveIBOE(string workspace, BOEFormIBOEModelView boeFormsVM)
        {
            if (boeFormsVM == null)
            {
                throw new ArgumentNullException(nameof(boeFormsVM));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(log, WebConstants.ACTION_SAVE_IBOE_FORM, SecurityPage.ManageBOEForms, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            if (ModelState.IsValid)
            {
                Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>(this.ScrubViewModelRichTextForSave(boeFormsVM).ToList());
                if (validationErrors.Any())
                {
                    throw new GenValidationException(validationErrors);
                }
                else
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                    {
                        this.boeFormControllerLogic.SaveBOEFormIBOE(boeFormsVM, ws.Id);
                        scope.Complete();
                    }
                }
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            FinalizeAction(log, WebConstants.ACTION_SAVE_IBOE_FORM, sw);

            return Json(new { status = true });  // Everything ok!
        }

        /// <summary>
        /// Saves a PBOE (create and update)
        /// </summary>
        /// <param name="workspace">The workspace for the PBOE.</param>
        /// <param name="boeFormsVM">The view model for the PBOE.</param>
        /// <returns>JSON status message.</returns>
        public virtual JsonResult SavePBOE(string workspace, BOEFormPBOEModelView boeFormsVM)
        {
            if (boeFormsVM == null)
            {
                throw new ArgumentNullException(nameof(boeFormsVM));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(log, WebConstants.ACTION_SAVE_PBOE_FORM, SecurityPage.ManageBOEForms, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            if (ModelState.IsValid)
            {
                Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>(this.ScrubViewModelRichTextForSave(boeFormsVM).ToList());

				// Check that only one option selected for Certified Cost or Pricing Data (CCoPD) Applicability
                // Do this by adding all options to a bool array, then getting a count of bools set to true via .Count(x=>x), equivalent to .Count(x=> x==true)
                // then check that the count is 1 or less
				bool[] expectedApplicabilities = new bool[] { boeFormsVM.CCoPDApplies, boeFormsVM.CommercialItemExceptionApplies, 
                    boeFormsVM.CompetitionExceptionApplies, boeFormsVM.LessThanThresholdExceptionApplies, boeFormsVM.OtherExceptionApplies };
                if (expectedApplicabilities.Count(x => x) > 1)
                {
                    validationErrors.Add(new ValidationMessage(ValidationConstants.PBOE_ONE_CCOPD_APPLICABILITY));
                }

                if (validationErrors.Any())
                {
                    throw new GenValidationException(validationErrors);
                }
                else
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                    {
                        this.boeFormControllerLogic.SaveBOEFormPBOE(boeFormsVM, ws.Id);
                        scope.Complete();
                    }
                }
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            FinalizeAction(log, WebConstants.ACTION_SAVE_PBOE_FORM, sw);

            return Json(new { Status = true }); // everything ok!
        }

        /// <summary>
        /// Validates the IBOE Form.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="boeFormsVM">The BOE form viewmodel.</param>
        /// <returns>Json result success or validation errors.</returns>
        public virtual JsonResult ValidateIBOEForm(string workspace, BOEFormIBOEModelView boeFormsVM)
        {
            if (boeFormsVM == null)
            {
                throw new ArgumentNullException(nameof(boeFormsVM));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(log, WebConstants.ACTION_VALIDATE_IBOE_FORM, SecurityPage.ManageBOEForms, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            if (ModelState.IsValid)
            {
                Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>(this.ScrubViewModelRichTextForSave(boeFormsVM).ToList());
                BOEFormIBOEDTO iboe = this.boeFormControllerLogic.ConvertModelViewToDto(boeFormsVM, ws.Id);
                this.boeFormControllerLogic.ValidateIBOE(iboe, validationErrors, ActionLogicUtility.GetProposalTitleAndRfpNumber(ws));

                if (validationErrors.Any())
                {
                    throw new GenValidationException(validationErrors);
                }
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            FinalizeAction(log, WebConstants.ACTION_VALIDATE_IBOE_FORM, sw);

            return Json(new { Status = true }); // everything ok!
        }

        /// <summary>
        /// Validates the PBOE Form.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="boeFormsVM">The BOE form viewmodel.</param>
        /// <returns>Json result success or validation errors.</returns>
        public virtual JsonResult ValidatePBOEForm(string workspace, BOEFormPBOEModelView boeFormsVM)
        {
            if (boeFormsVM == null)
            {
                throw new ArgumentNullException(nameof(boeFormsVM));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(log, WebConstants.ACTION_VALIDATE_PBOE_FORM, SecurityPage.ManageBOEForms, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            if (ModelState.IsValid)
            {
                Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>(this.ScrubViewModelRichTextForSave(boeFormsVM).ToList());
                BOEFormPBOEDTO pboe = this.boeFormControllerLogic.ConvertModelViewToDto(boeFormsVM, ws.Id);
                this.boeFormControllerLogic.ValidatePBOE(pboe, validationErrors, ActionLogicUtility.GetProposalTitleAndRfpNumber(ws));

                if (validationErrors.Any())
                {
                    throw new GenValidationException(validationErrors);
                }
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            FinalizeAction(log, WebConstants.ACTION_VALIDATE_PBOE_FORM, sw);

            return Json(new { Status = true }); // everything ok!
        }
    }

    #endregion public methods
}