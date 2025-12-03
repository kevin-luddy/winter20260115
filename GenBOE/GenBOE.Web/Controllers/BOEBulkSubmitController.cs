// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Web.Mvc;
    using ActionLogic;
    using ActionLogic.Common;
    using ActionLogic.ControllerLogic;
    using ActionLogic.Metrics;
    using ActionLogic.WBS.BOE;
    using Common;
    using DataBridge.Common;
    using DataBridge.Common.Interfaces;
    using DataBridge.DTO;
    using Dtos;
    using IES.Common;
    using IES.Common.Exceptions;
    using ModelView;
    using Objects;

    /// <summary>
    /// BOE Bulk Submit Controller
    /// </summary>
    /// <seealso cref="GenBOE.Web.Common.GenBOEController" />
    public class BOEBulkSubmitController : GenBOEController
    {
        private Logger _log = new Logger(typeof(BOEBulkSubmitController));
        private IBOEControllerLogic boeControllerLogic;
        private IValidateBOE validateBOE;

        public BOEBulkSubmitController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper,
            SiteMasterUtilities inSiteMasterUtilities,
            SystemMetrics inSystemMetrics,
            IFullObjectFactory factory,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader permissionLoader,
            IGenBOEControllerLogic inControllerLogic,
            IBOEControllerLogic boeControllerLogic,
            IValidateBOE validateBOE)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, userLoader, permissionLoader, inControllerLogic)
        {
            this.boeControllerLogic = boeControllerLogic;
            this.validateBOE = validateBOE;
        }

        /// <summary>
        /// This function calls the BOE Bulk Submit view
        /// </summary>
        /// <param name="workspace">the workspace the view is associated with.</param>
        /// <returns>returns the BOE Bulk Submit view </returns>
        [HttpGet]
		public ViewResult Index(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBoeBulkSubmit", SecurityPage.BulkSubmit, SecurityAuthorization.Read, ws, null);

            // Perform Action
            ViewResult toReturn = GetMasterView(WebConstants.VIEW_BOE_BULK_SUBMIT, workspace);

            // Finalize Action
            FinalizeAction(_log, "DisplayBoeBulkSubmit", sw);
            return toReturn;
        }

		/// <summary>
		/// Gets the BOE bulk submit model.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <returns>JSON result containing the model.</returns>
		[HttpPost]
		public JsonResult GetBOEBulkSubmitModel(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Action Initialize
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_GET_BOE_BULK_SUBMIT_MODEL, SecurityPage.BulkSubmit, SecurityAuthorization.Read, ws, null);

            // get all data
            ICollection<BOEBulkSubmitModelView> modelViews = new List<BOEBulkSubmitModelView>();

            List<int> boeIds = ws.Boes.Select(x => x.Id).ToList();
            HashSet<PermissionsDTO> rolesForBoes = new HashSet<PermissionsDTO>(this.PermissionsLoader.GetBOEPermissions(boeIds));
            UserDTO currentUser = ws.CurrentActiveUser;

            ICollection<PermissionsDTO> roles = rolesForBoes.Where(r => r.ETIUserId == currentUser.UserID && (r.Role == Role.Author || r.Role == Role.SubcontractorAuthor)).ToList();

            // Get the list of BOEs that the user is author or subcontract author on
            boeIds = roles.Where(r => r.BOEId.HasValue).Select(o => o.BOEId.Value).Distinct().ToList();
            BOEState[] returningStates = new BOEState[] { BOEState.AwaitingApproval, BOEState.Draft, BOEState.DraftLocked };

            foreach (FullBoe boe in ws.Boes.Where(b => boeIds.Contains(b.Id) && returningStates.Contains(b.State)))
            {
                WbsDTO wbsDTO = ws.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                ClinDTO clinDTO = ws.Clins.FirstOrDefault(x => x.Id == boe.CLINID);
                BOEBulkSubmitModelView model = new BOEBulkSubmitModelView(boe, wbsDTO, clinDTO);
                if (model.State == BOEState.Draft || model.State == BOEState.DraftLocked)
                {
                    // need to validate to see if it can transition
                    ValidationBOEModelView validatedBOE = this.validateBOE.ValidateBOE_OnValidateBtnClick(boe, ws);
                    model.Invalid = !validatedBOE.isValid;
                }

                modelViews.Add(model);
            }

            // Action Finalize
            FinalizeAction(_log, WebConstants.ACTION_GET_BOE_BULK_SUBMIT_MODEL, sw);

            return Json(modelViews);
        }

        /// <summary>
        /// BOEs the bulk submit.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="boeStates">The BOE states.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[HttpPost]
		public JsonResult BOEBulkSubmit(string workspace, IDictionary<int, BOEState> boeStates)
        {
            if (boeStates == null || boeStates.Count == 0)
            {
                throw new GenValidationException("Nothing was selected.");
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_BOE_BULK_SUBMIT, SecurityPage.BulkSubmit, SecurityAuthorization.ReadUpdate, ws, null);
            List<string> errorMessages = new List<string>();

            Dictionary<int, BOEState> validTransitions = new Dictionary<int, BOEState>();

            // Validate the BOEs
            foreach (KeyValuePair<int, BOEState> boeState in boeStates)
            {
                if (boeState.Value == BOEState.Draft)
                {
                    // do not need to validate
                    validTransitions.Add(boeState.Key, boeState.Value);
                }
                else
                {
                    FullBoe boe = ws.Boes.First(b => b.Id == boeState.Key);
                    ValidationBOEModelView validatedBOE = this.validateBOE.ValidateBOE_OnValidateBtnClick(boe, ws);
                    if (validatedBOE.isValid)
                    {
                        validTransitions.Add(boeState.Key, boeState.Value);
                    }
                    else
                    {
                        errorMessages.Add(string.Format("Validation issues with BOE [Title:{0} (Id:{1})] ", boe.Title, boe.Id));
                    }
                }
            }

            // Bulk Save the States
            this.boeControllerLogic.SaveBOEStates(validTransitions, ws, errorMessages);

            JsonResult response = Json(new { Status = true, ErrorMessages = errorMessages });

            FinalizeAction(_log, WebConstants.ACTION_BOE_BULK_SUBMIT, sw);

            return response;
        }
    }
}