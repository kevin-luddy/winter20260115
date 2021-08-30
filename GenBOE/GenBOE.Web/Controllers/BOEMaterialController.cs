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
    using System.Linq;
    using System.Transactions;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.Metrics;
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
    using IES.Common.OfficeUtilities;

    public class BOEMaterialController : GenBOEController
    {
        private Logger _log = new Logger(typeof(BOEMaterialController));

        private IMaterialDTODataLoader _materialDTODataLoader = null;
        private IBOEMaterialControllerLogic _BOEMaterialControllerLogic = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public BOEMaterialController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper,
            SiteMasterUtilities inSiteMasterUtilities,
            IMaterialDTODataLoader inMaterialDTODataLoader,
            SystemMetrics inSystemMetrics,
            IFullObjectFactory factory,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader permissionLoader,
            IBOEMaterialControllerLogic inBOEMaterialControllerLogic,
            IGenBOEControllerLogic inControllerLogic)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, userLoader, permissionLoader, inControllerLogic)
        {
            _materialDTODataLoader = inMaterialDTODataLoader;
            this.Factory = factory;
            _BOEMaterialControllerLogic = inBOEMaterialControllerLogic;
        }

        /// <summary>
        /// Display BOE Material Composite view
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="boeID"></param>
        /// <param name="materialID"></param>
        /// <returns></returns>
        public ViewResult DisplayBOEMaterialComposite(string workspace, int boeID, int? materialID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEMaterialComposite", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

            // Perform Action
            ViewData["BOEID"] = boeID;
            if (materialID.HasValue)
            {
                ViewData["MaterialID"] = materialID.Value;

            }

            ViewResult toReturn = View(WebConstants.VIEW_MATERIAL_COMPOSITE);

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEMaterialComposite", sw);
            return toReturn;
        }

        /// <summary>
        /// Display the overview Material Grid 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="boeID"></param>
        /// <returns></returns>
        public ViewResult DisplayBOEMaterialGrid(string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            FullBoe fullBoe = this.Factory.CreateFullBoe(boeID);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEMaterialGrid", SecurityPage.BOETravelGrid, SecurityAuthorization.Read, ws, boeID);

            // Perform Action

            BOEMaterialGridModelView theModelView = new BOEMaterialGridModelView();
            ViewData["BOEID"] = boeID;
            theModelView.DeleteAction = WebConstants.ACTION_DELETE_BOE_MATERIAL;
            theModelView.DisplayEvent = WebConstants.EVENT_DISPLAY_MATERIAL_ELEMENT_DETAILS;

            IReadOnlyCollection<MaterialDTO> taskElementCollection = fullBoe.Materials; 
           
            theModelView.TaskElements = new Collection<BOEMaterialGridRow>((
                from t in taskElementCollection
                select new BOEMaterialGridRow
                {
                    MaterialID = t.Id,
                    TaskID = t.TaskID,
                    TaskTitle = t.TaskTitle,
                    UpdateDate = t.UpdateDate
                }).ToArray());



            ViewResult toReturn = View(WebConstants.VIEW_BOE_MATERIAL_GRID, theModelView);

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEMaterialGrid", sw);
            return toReturn;
        }

        /// <summary>
        /// Display the Material Task Element Details
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="boeID"></param>
        /// <param name="materialID"></param>
        /// <returns></returns>
        public ViewResult DisplayBOEMaterialElementDetails(string workspace, int boeID, int? materialID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEMaterialElementDetails", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

            // Perform Action
            MaterialDetailsModelView mv = new MaterialDetailsModelView();
            _BOEMaterialControllerLogic.PopulateCompanySpecificProperties(mv);

            if (materialID.HasValue)
            {
                MaterialDTO tempDTO = this.Factory.GetMaterialById(materialID.Value);

                mv.MaterialID = tempDTO.Id;
                mv.TaskTitle = tempDTO.TaskTitle;
                mv.TaskDescription = tempDTO.TaskDescription;
                mv.MoqText = tempDTO.MoqText;
                mv.TaskID = tempDTO.TaskID;
                mv.UpdateDate = tempDTO.UpdateDate;
            }
            else
            {
                mv.MaterialID = -1;

            }

            ViewResult toReturn = View(WebConstants.VIEW_MATERIAL_ELEMENT_DETAILS, mv);

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEMaterialElementDetails", sw);
            return toReturn;
        }

        public JsonResult DeleteBOEMaterial(string workspace, int boeID, int MaterialID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(_log, "DeleteBOEMaterial", SecurityPage.BOEMaterialsTypes,
                SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            MaterialDTO dtoToDelete = this.Factory.GetMaterialById(MaterialID);
            dtoToDelete.Updateable = UpdateType.Deleted;
            Collection<MaterialDTO> tempDtoCollToDel = new Collection<MaterialDTO>();
            tempDtoCollToDel.Add(dtoToDelete);

            _materialDTODataLoader.SaveMaterials(tempDtoCollToDel);

            JsonResult toReturn = Json(new { Status = true });

            FinalizeAction(_log, "DeleteBOEMaterial", sw);

            return toReturn;
        }

        /// <summary>
        /// Save Materials
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="boeID"></param>
        /// <param name="inDetailsWV"></param>
        /// <param name="inTypesGridMVCollection"></param>
        /// <returns></returns>
        public ActionResult SaveEditMaterialDetailsComposite(string workspace, int boeID, MaterialDetailsModelView inDetailsWV)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            FullBoe boe = this.Factory.CreateFullBoe(boeID);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "SaveEditMaterialDetailsComposite", SecurityPage.TaskElements,
                SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            if (inDetailsWV == null)
            {
                throw new ArgumentNullException(nameof(inDetailsWV));
            }

            // Perform Action
            if (ModelState.IsValid)
            {
                // check to see there is already 1 detail present .. if so throw an exception
                if (boe.MaterialCount >= 1 && inDetailsWV.MaterialID < 0)
                {
                    // we only allow 1 detail in a material BOE per business rules
                    throw new GenValidationException("More than 1 detail for a Material BOE is not allowed.  Please delete the detail and try again.");
                }

                ICollection<ValidationMessage> richTextValidationMessages = this.ScrubViewModelRichTextForSave(inDetailsWV);
                if (richTextValidationMessages.Any())
                {
                    throw new GenValidationException(richTextValidationMessages);
                }

                MaterialDTO MaterialDTOtoSave = new MaterialDTO();
                MaterialDTOtoSave.BoeID = boeID;
                MaterialDTOtoSave.Id = inDetailsWV.MaterialID.Value;
                MaterialDTOtoSave.TaskDescription = inDetailsWV.TaskDescription;
                MaterialDTOtoSave.TaskID = inDetailsWV.TaskID;
                MaterialDTOtoSave.TaskTitle = inDetailsWV.TaskTitle;
                MaterialDTOtoSave.MoqText = inDetailsWV.MoqText;
                MaterialDTOtoSave.UpdateDate = inDetailsWV.UpdateDate;
                MaterialDTOtoSave.Updateable = UpdateType.Upsert;

                _materialDTODataLoader.SaveMaterials(new Collection<MaterialDTO> { MaterialDTOtoSave });
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            FinalizeAction(_log, "SaveEditMaterialDetailsComposite", sw);
            return Json(new { Status = true });

        }

        /// <summary>
        /// Delete all the material's materials(aka resource type)
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="boeID"></param>
        /// <param name="inDetailsWV"></param>
        /// <param name="inTypesGridMVCollection"></param>
        /// <returns></returns>
        public ActionResult DeleteAllMaterials(string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DeleteAllMaterials", SecurityPage.BOEMaterialsTypes, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                _materialDTODataLoader.DeleteAllMaterialTaskElements(boeID);
                scope.Complete();
            }

            FinalizeAction(_log, "DeleteAllMaterials", sw);
            return Json(new { Status = true });

        }
    }
}
