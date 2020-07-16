// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView;
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

    public class BOEOtherDirectCostController : GenBOEController
    {
        Logger _log = new Logger(typeof(BOEOtherDirectCostController));

        private ResourceDTODataLoader _ResourceDTODataLoader = null;
        private IBOEOtherDirectCostControllerLogic _BOEOtherDirectCostControllerLogic = null;
        private IPerformingOrgDTODataLoader perfOrgLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        public BOEOtherDirectCostController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper,
            SiteMasterUtilities inSiteMasterUtilities,
            ResourceDTODataLoader inResourceDTODataLoader,
            IPermissionsDTODataLoader inPermissionsLoader,
            SystemMetrics inSystemMetrics,
            IBOEOtherDirectCostControllerLogic inBOEOtherDirectCostControllerLogic,
            IFullObjectFactory factory,
            IUserDTODataLoader userLoader,
            IPerformingOrgDTODataLoader perfOrgLoader,
            IGenBOEControllerLogic inControllerLogic)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, userLoader, inPermissionsLoader, inControllerLogic)
        {
            _ResourceDTODataLoader = inResourceDTODataLoader;
            _BOEOtherDirectCostControllerLogic = inBOEOtherDirectCostControllerLogic;
            this.perfOrgLoader = perfOrgLoader;
        }

        virtual public ViewResult DisplayBOEOtherDirectCostComposite(string workspace, int boeID, int? odcElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEOtherDirectCostComposite", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

            bool IsSubContractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
                                    where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
                                    select p).Any();
            if (IsSubContractor)
            {
                throw new AuthorizationException();
            }
            // Perform Action
            ViewData["BOEID"] = boeID;
            if (odcElementID.HasValue)
            {
                ViewData["ODCID"] = odcElementID.Value;

            }

            ViewResult toReturn = View(WebConstants.VIEW_ODC_ELEMENT_COMPOSITE);

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEOtherDirectCostComposite", sw);
            return toReturn;
        }

        public ViewResult DisplayBOEOtherDirectCostGrid(string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            bool IsSubContractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
                                    where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
                                    select p).Any();

            Stopwatch sw = null;
            try
            {
                // Initialize Action
                sw = InitializeAction(_log, "DisplayBOEOtherDirectCost", SecurityPage.BoeODCGrid, SecurityAuthorization.Read, ws, boeID);
            }
            catch (AuthorizationException)
            {
                if (!IsSubContractor)
                {
                    throw;
                }
            }

            ViewResult toReturn;

            if (IsSubContractor)
            {
                toReturn = null;
            }
            else
            {

                // Perform Action
                Collection<BOEOtherDirectCostGridModelView> theModelViews = new Collection<BOEOtherDirectCostGridModelView>();

                FullBoe boe = this.Factory.CreateFullBoe(boeID);

                foreach (OtherDirectCostDTO otherDirectCostDTO in boe.OtherDirectCosts)
                {
                    BOEOtherDirectCostGridModelView mv = new BOEOtherDirectCostGridModelView
                    {
                        ODCID = otherDirectCostDTO.Id,
                        TaskID = otherDirectCostDTO.TaskID,
                        TaskTitle = otherDirectCostDTO.TaskTitle,
                        StartDate = otherDirectCostDTO.StartDate.HasValue ? otherDirectCostDTO.StartDate : boe.StartDate,
                        EndDate = otherDirectCostDTO.EndDate.HasValue ? otherDirectCostDTO.EndDate : boe.EndDate,
                        BOETaskElementOrder = otherDirectCostDTO.BOETaskElementOrder
                    };

                    theModelViews.Add(mv);
                }

                theModelViews = theModelViews.OrderBy(teOrder => teOrder.BOETaskElementOrder).ThenBy(teOrder => teOrder.ODCID).ToCollection();

                //create a var for list items
                var orderOfTaskElements = new Collection<SelectListItem>();
                //get a list of each task element.
                foreach (BOEOtherDirectCostGridModelView row in theModelViews)
                {
                    orderOfTaskElements.Add(new SelectListItem { Text = row.TaskID + " " + row.TaskTitle, Value = row.ODCID.ToString() });
                }


                ViewData["Order_Of_TaskElements"] = orderOfTaskElements;
                ViewData["BOEID"] = boeID;
                ViewData["DISABLE_ADD_ODC"] = false;
                if (CheckPermissions(SecurityPage.BoeODCGrid, ws, boeID) != SecurityAuthorization.CreateReadUpdateDelete)
                {
                    ViewData["DISABLE_ADD_ODC"] = true;
                }



                toReturn = View(WebConstants.VIEW_BOE_OTHER_DIRECT_COST_GRID, theModelViews);
            }

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEOtherDirectCost", sw);
            return toReturn;
        }

        public ViewResult DisplayBOEODCElementDetails(string workspace, int boeID, int? odcElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEOtherDirectCost", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

            ViewResult toReturn;

            bool IsSubContractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
                                    where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
                                    select p).Any();
            if (IsSubContractor)
            {
                toReturn = null;
            }
            else
            {
                // Perform Action
                ODCElementDetailsModelView mv = new ODCElementDetailsModelView();
                _BOEOtherDirectCostControllerLogic.PopulateCompanySpecificProperties(mv);

                // get the BOE DTO
                BoeDTO boe = this.Factory.CreateFullBoe(boeID);
                ViewData["BOEState"] = (int)boe.State;

                if (odcElementID.HasValue)
                {
                    OtherDirectCostDTO tempDTO = this.Factory.CreateOtherDirectCost((int)odcElementID);

                    mv.ODCID = tempDTO.Id;
                    mv.TaskID = tempDTO.TaskID;
                    mv.TaskTitle = tempDTO.TaskTitle;
                    mv.ODCTaskDescription = tempDTO.TaskDescription;
                    mv.ODCMOQText = tempDTO.MoqText;
                    mv.UpdateDate = tempDTO.UpdateDate;
                    mv.StartDate = tempDTO.StartDate.HasValue ? tempDTO.StartDate.Value.ToString("MM/yyyy") : boe.StartDate.ToString("MM/yyyy");
                    mv.EndDate = tempDTO.EndDate.HasValue ? tempDTO.EndDate.Value.ToString("MM/yyyy") : boe.EndDate.ToString("MM/yyyy");
                }
                else
                {
                    mv.ODCID = -1;
                    mv.StartDate = boe.StartDate.ToString("MM/yyyy");
                    mv.EndDate = boe.EndDate.ToString("MM/yyyy");
                }

                toReturn = View(WebConstants.VIEW_ODC_ELEMENT_DETAILS, mv);
            }

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEOtherDirectCost", sw);
            return toReturn;
        }

        public ViewResult DisplayBOEODCTypes(string workspace, int boeID, int? odcElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEODCTypes", SecurityPage.BoeLaborTypes, SecurityAuthorization.Read, ws, boeID);

            ViewData["BOEID"] = boeID;
            ViewData["ODCID"] = odcElementID;

            ViewResult toReturn = View(WebConstants.VIEW_ODC_TYPES);

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEODCTypes", sw);
            return toReturn;
        }

        public ViewResult DisplayBOEODCTypesGrid(string workspace, int boeID, int? odcElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEODCTypesGrid", SecurityPage.BoeLaborTypes, SecurityAuthorization.Read, ws, boeID);

            int resourceListID = ws.ResourceListID;

            ICollection<ResourceDTO> resources = _ResourceDTODataLoader.GetByListIdAndElementOfCost(resourceListID, ElementOfCostType.ODC);
            Collection<ResourceDataForGridsModelView> resourceModelViews = new Collection<ResourceDataForGridsModelView>();
            Collection<ResourceDataForGridsDescriptionModelView> resourceDescriptions = new Collection<ResourceDataForGridsDescriptionModelView>();

            foreach (ResourceDTO resource in resources)
            {
                ResourceDataForGridsModelView resourceMV = new ResourceDataForGridsModelView();
                resourceMV.ResourceID = resource.Id;
                resourceMV.ElementOfCost = resource.ElementOfCost;
                resourceMV.SpreadText = resource.ResourceName;

                resourceMV.ResourceCode = resource.ResourceName;
                resourceMV.BOESummaryText = resource.LaborType;

                resourceMV.ResourceDescription = resource.ResourceDesc;

                ResourceDataForGridsDescriptionModelView resourceDescription = resourceDescriptions.FirstOrDefault(x => x.ResourceDescription == resourceMV.ResourceDescription);
                if (resourceDescription == null && !string.IsNullOrEmpty(resourceMV.ResourceDescription))
                {
                    resourceDescription = new ResourceDataForGridsDescriptionModelView();
                    resourceDescription.ResourceDescription = resourceMV.ResourceDescription;
                    resourceDescription.ElementOfCost = resourceMV.ElementOfCost;
                    resourceDescriptions.Add(resourceDescription);
                }

                resourceModelViews.Add(resourceMV);
            }

            ViewData["BOEID"] = boeID;
            ViewData["ODCResources"] = resourceModelViews;
            ViewData["ODCResourceDescriptions"] = resourceDescriptions;
            ViewData["ODCElementId"] = odcElementID;
            ViewData["ODCSpreadCurve"] = _CommonDataMapper.getOdcSpreadCurve();

            IReadOnlyCollection<PerformingOrgDTO> orgs = ws.PerformingOrgsForWsList;
            Collection<PerformingOrgModelView> perfOrgModelViews = new Collection<PerformingOrgModelView>();
            foreach (PerformingOrgDTO org in orgs)
            {
                PerformingOrgModelView perforgMV = new PerformingOrgModelView()
                {
                    PerformingOrgID = org.Id,
                    PerformingOrgName = org.PerformingOrgName,
                    PerformingOrgDesc = org.PerformingOrgDesc
                };
                perfOrgModelViews.Add(perforgMV);
            }

            ViewData["PerfOrgs"] = perfOrgModelViews;

            // get the BOE DTO
            FullBoe boe = this.Factory.CreateFullBoe(boeID);

            ViewData["BOEStartDate"] = boe.StartDate.ToString("MM/yyyy");
            ViewData["BOEEndDate"] = boe.EndDate.ToString("MM/yyyy");

            // Perform Action
            Collection<ODCTypesGridModelView> theModelViews = new Collection<ODCTypesGridModelView>();

            // for the given task element ID, get the labor types (no spreads needed)
            if (odcElementID.HasValue)
            {
                OtherDirectCostDTO ODC = this.Factory.CreateOtherDirectCost((int)odcElementID);

                ICollection<ResourceDTO> resourcesFromDb = this._ResourceDTODataLoader.GetByIds(ODC.ODCTypes.Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value).Distinct().ToList());
                HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(ODC.ODCTypes.Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value).Distinct().ToList()));

                foreach (OtherDirectCostType ODCType in ODC.ODCTypes)
                {
                    ODCTypesGridModelView ODCostType = new ODCTypesGridModelView(ODCType);
                    ODCostType.ODCTypeID = ODCType.ODCTypeID;

                    if (ODCostType.ResourceID.HasValue)
                    {
                        ResourceDTO thisResource = resourcesFromDb.First(x => x.Id == ODCostType.ResourceID.Value);
                        ODCostType.ResourceDescription = thisResource.ResourceDesc;
                        ODCostType.BOESummaryText = thisResource.LaborType;
                    }

                    if (ODCostType.PerformingOrgID.HasValue)
                    {
                        PerformingOrgDTO aPerformingOrg = perfOrgsFromDb.First(x => x.Id == ODCostType.PerformingOrgID.Value);
                        ODCostType.PerformingOrgName = aPerformingOrg.PerformingOrgName;
                    }

                    theModelViews.Add(ODCostType);
                }
            }



            ViewResult toReturn = View(WebConstants.VIEW_ODC_TYPES_GRID, theModelViews);

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEODCTypesGrid", sw);
            return toReturn;
        }

        public ViewResult DisplayBOEODCSpread(string workspace, int boeID, int? odcElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEODCSpread", SecurityPage.BoeLaborTypes, SecurityAuthorization.Read, ws, boeID);
            ViewData["BOEID"] = boeID;
            ViewData["ODCID"] = odcElementID;

            FinalizeAction(_log, "DisplayBOEODCSpread", sw);
            return View(WebConstants.VIEW_ODC_SPREAD);
        }

        public ViewResult DisplayBOEODCSpreadGrid(string workspace, int boeID, int? odcElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            FullBoe boe = this.Factory.CreateFullBoe(boeID);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEODCSpreadGrid", SecurityPage.BoeLaborTypes, SecurityAuthorization.Read, ws, boeID);
            ViewData["BOEID"] = boeID;
            ViewData["ODCID"] = odcElementID;

            ViewData["BOEStartDate"] = boe.StartDate.ToString("MM/yyyy");
            ViewData["BOEEndDate"] = boe.EndDate.ToString("MM/yyyy");

            // Perform Action
            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            LinkedList<ODCSpreadDetailModelView> returnData = new LinkedList<ODCSpreadDetailModelView>();

            if (odcElementID.HasValue)
            {
                // for the given task element ID, get the labor types/spreads
                OtherDirectCostDTO ODC = this.Factory.CreateOtherDirectCost(odcElementID.Value);

                DataRelationshipVerifier.VerifyDataRelation(ODC, boeID);

                foreach (OtherDirectCostType ODCT in ODC.ODCTypes)
                {
                    //NO need to check data relationship since it was checked on it's parent
                    // need to get the labor spread info
                    ODCSpreadDetailModelView odcSpreadDetail = new ODCSpreadDetailModelView(ODCT, _ResourceDTODataLoader, perfOrgLoader);

                    returnData.AddLast(odcSpreadDetail);
                }
            }

            ViewData["ODCSJSON"] = serializer.Serialize(returnData);
            FinalizeAction(_log, "DisplayBOEODCSpreadGrid", sw);
            return View(WebConstants.VIEW_ODC_SPREAD_GRID, returnData);
        }

        virtual public ActionResult ExportODCSpread(string workspace, int boeID, int odcElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "ExportODCSpread", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

            OtherDirectCostDTO taskElement = this.Factory.CreateOtherDirectCost(odcElementID);
            string fileName = string.Empty;

            if (taskElement != null)
            {
                DataRelationshipVerifier.VerifyDataRelation(taskElement, boeID);
                fileName = ODCSpreadExporter.ExportToExcelFile(Server.MapPath("~/Templates/Export/ODCSpread.xlsx"), taskElement.ODCTypes, _ResourceDTODataLoader, this.perfOrgLoader);
            }
            else
            {
                fileName = ODCSpreadExporter.ExportToExcelFile(Server.MapPath("~/Templates/Export/ODCSpread.xlsx"), new Collection<OtherDirectCostType>(), _ResourceDTODataLoader, this.perfOrgLoader);
            }

            ExportFileDownloadResult toReturn = new ExportFileDownloadResult(fileName, ws.WorkspaceName + "_BOE-" + boeID + "_ODCSpreads.xlsx");

            // Finalize Action
            FinalizeAction(_log, "ExportODCSpread", sw);
            return toReturn;
        }

        virtual public ActionResult ExportODCType(string workspace, int boeID, int odcElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "ExportODCType", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

            OtherDirectCostDTO thisTaskElement = null;

            if (odcElementID != -1)
            {
                thisTaskElement = this.Factory.CreateOtherDirectCost(odcElementID);
                DataRelationshipVerifier.VerifyDataRelation(thisTaskElement, boeID);
            }

            string fileName = ODCTypeExporter.ExportToExcelFile(Server.MapPath("~/Templates/Export/ODCTypes.xlsx"), _ResourceDTODataLoader, _CommonDataMapper, ws, thisTaskElement);

            ExportFileDownloadResult toReturn = new ExportFileDownloadResult(fileName, ws.WorkspaceName + "_BOE-" + boeID + "_ODC-" + odcElementID + "_ODCTypes.xlsx");

            // Finalize Action
            FinalizeAction(_log, "ExportODCType", sw);
            return toReturn;
        }

        /// <summary>
        /// reorders odc taskelement
        /// </summary>
        /// <param name="theModelView">user order</param>
        /// <param name="workspace">workspace </param>
        /// <param name="boeID">current boeid</param>
        /// <returns></returns>
        virtual public JsonResult SaveReorderODCTaskElements(TaskElementOrderCollection theModelView, string workspace, int boeID)
        {
            throw new GenValidationException("ODC Tasks can no longer be reordered");
        }
    }
}
