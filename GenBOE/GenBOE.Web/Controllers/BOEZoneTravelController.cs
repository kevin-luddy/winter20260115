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
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;
    using IES.Common.Exceptions;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenBOE.Web.Common;

    public class BOEZoneTravelController : GenBOEController
    {
        Logger _log = new Logger(typeof(BOEZoneTravelController));

        private IBOELaborControllerLogic boeLaborControllerLogic = null;
        private IMSTZoneTravelOriginDTODataLoader mstZoneTravelOriginDTODataLoader = null;
        private IMSTZoneTravelDestinationDTODataLoader mstZoneTravelDestinationDTODataLoader = null;
        private BOEZoneTravelControllerLogic zoneTravelControllerLogic = null;
        private ITravelControllerLogic _TravelControllerLogic = null;
        private RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public BOEZoneTravelController(ISecurityAccess SecurityAccess,
            CommonDataMapper CommonDataMapper,
            SiteMasterUtilities SiteMasterUtilities,
            SystemMetrics SystemMetrics,
            IFullObjectFactory factory,
            IUserDTODataLoader UserLoader,
            IPermissionsDTODataLoader PermissionsLoader,
            IGenBOEControllerLogic ControllerLogic,
            IBOELaborControllerLogic BoeLaborControllerLogic,
            IMSTZoneTravelOriginDTODataLoader MSTZoneTravelOriginDTODataLoader,
            IMSTZoneTravelDestinationDTODataLoader MSTZoneTravelDestinationDTODataLoader,
            BOEZoneTravelControllerLogic BOEZoneTravelControllerLogic,
            ITravelControllerLogic inTravelControllerLogic,
            RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader
            )
            : base(SecurityAccess, CommonDataMapper, SiteMasterUtilities, SystemMetrics, factory, UserLoader, PermissionsLoader, ControllerLogic)
        {
            this.boeLaborControllerLogic = BoeLaborControllerLogic;
            this.mstZoneTravelOriginDTODataLoader = MSTZoneTravelOriginDTODataLoader;
            this.mstZoneTravelDestinationDTODataLoader = MSTZoneTravelDestinationDTODataLoader;
            this.zoneTravelControllerLogic = BOEZoneTravelControllerLogic;
            this._TravelControllerLogic = inTravelControllerLogic;
            this.zoneTravelRatesFeesLoader = zoneTravelRatesFeesLoader;
        }

        /// <summary>
        /// Method to display the Zone Travel Composite view
        /// </summary>
        /// <param name="workspace">Workspace short name</param>
        /// <param name="boeID">ID of BOE that contains the zone travel element</param>
        /// <param name="travelElementID">ID of the Zone Travel element</param>
        /// <returns>Zone Travel Composite view</returns>
        virtual public ViewResult DisplayBOEZoneTravelComposite(string workspace, int boeID, int? travelElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEZoneTravelComposite", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

            bool IsSubContractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
                                    where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
                                    select p).Any();

            if (IsSubContractor)
            {
                throw new AuthorizationException();
            }

            // Perform Action
            ViewData["BOEID"] = boeID;
            ViewData["ZONETRAVELID"] = travelElementID;

            ViewResult toReturn = View(WebConstants.VIEW_ZONE_TRAVEL_ELEMENT_COMPOSITE);

            // Finalize Action
            FinalizeAction(_log, "DisplayZoneTravelComposite", sw);
            return toReturn;
        }

        /// <summary>
        /// Method to display the Zone Travel Grid view
        /// </summary>
        /// <param name="workspace">Workspace short name</param>
        /// <param name="boeID">ID of BOE that contains the zone travel element</param>
        /// <returns>Zone Travel Grid view</returns>
        public ViewResult DisplayBOEZoneTravelGrid(string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            bool IsSubContractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
                                    where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
                                    select p).Any();

            Stopwatch sw = null;
            try
            {
                // Initialize Action
                sw = InitializeAction(_log, "DisplayBOEZoneTravelGrid", SecurityPage.BOEZoneTravelGrid, SecurityAuthorization.Read, ws, boeID);
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
                ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates = this.zoneTravelRatesFeesLoader.getAllEscalationRatesByWorkspace(ws.Id);

                // Perform Action
                Collection<BOETravelGridModelView> theModelViews = zoneTravelControllerLogic.GetTravelGridModelViews(boeID, ws.DecimalPrecision, ws.Id, escalationRates);

                //create a var for list items
                var orderOfTaskElements = new Collection<SelectListItem>();
                //get a list of each task element.
                foreach (BOETravelGridModelView row in theModelViews)
                {
                    orderOfTaskElements.Add(new SelectListItem { Text = row.TaskID + " " + row.TaskTitle, Value = row.TravelID.ToString() });
                }
                ViewData["Order_Of_TaskElements"] = orderOfTaskElements;

                ViewData["BOEID"] = boeID;
                ViewData["DISABLE_ADD_TRAVEL"] = false;
                if (CheckPermissions(SecurityPage.BOETravelGrid, ws, boeID) != SecurityAuthorization.CreateReadUpdateDelete)
                {
                    ViewData["DISABLE_ADD_TRAVEL"] = true;
                }

                toReturn = View(WebConstants.VIEW_BOE_ZONE_TRAVEL_GRID, theModelViews);
            }

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEZoneTravelGrid", sw);
            return toReturn;
        }

        /// <summary>
        /// Method to display the Zone Travel Element Details view
        /// </summary>
        /// <param name="workspace">Workspace short name</param>
        /// <param name="boeID">ID of BOE that contains the zone travel element</param>
        /// <param name="travelElementID">ID of the Zone Travel element</param>
        /// <returns>Zone Travel Element Details view</returns>
        public ViewResult DisplayBOEZoneTravelElementDetails(string workspace, int boeID, int? travelElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEZoneTravelElementDetails", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

            // Perform Action
            BoeDTO boe = this.Factory.CreateFullBoe(boeID);
            ViewData["BOEState"] = (int)boe.State;
            ViewBag.RteFieldSize = ws.RteSizeLimit ?? Constants.MAX_RTE_LENGTH;

            BOETravelElementDetailsModelView theModelView = zoneTravelControllerLogic.GetTravelElementDetailsModelView(ws, boe, travelElementID);

            ViewResult toReturn = View(WebConstants.VIEW_ZONE_TRAVEL_ELEMENT_DETAILS, theModelView);

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEZoneTravelElementDetails", sw);
            return toReturn;
        }

        /// <summary>
        /// Method to display the Zone Travel Trips view
        /// </summary>
        /// <param name="workspace">Workspace short name</param>
        /// <param name="boeID">ID of BOE that contains the zone travel element</param>
        /// <param name="travelElementID">ID of the Zone Travel element</param>
        /// <returns>Zone Travel Trips view</returns>
        public ViewResult DisplayBOEZoneTravelTrips(string workspace, int boeID, int? travelElementID)
        {
            FullWorkspace workspaceObject = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEZoneTravelTrips", SecurityPage.BoeLaborTypes, SecurityAuthorization.Read, workspaceObject, boeID);

            ViewData["BOEID"] = boeID;
            ViewData["ZONETRAVELID"] = travelElementID;

            ViewResult toReturn = View(WebConstants.VIEW_ZONE_TRAVEL_TRIPS);

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEZoneTravelTrips", sw);
            return toReturn;
        }

        /// <summary>
        /// Method to display the Zone Travel Trips Grid view
        /// </summary>
        /// <param name="workspace">Workspace short name</param>
        /// <param name="boeID">ID of BOE that contains the zone travel element</param>
        /// <param name="travelElementID">ID of the Zone Travel element</param>
        /// <returns>Zone Travel Trips Grid view</returns>
        public ViewResult DisplayBOEZoneTravelTripsGrid(string workspace, int boeID, int? travelElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEZoneTravelTripsGrid", SecurityPage.BoeLaborTypes, SecurityAuthorization.Read, ws, boeID);

            ViewData["BOEID"] = boeID;
            ViewData["ZONETRAVELID"] = travelElementID;

            ViewData["CustomFields"] = this.boeLaborControllerLogic.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.LaborTypes);

            //Load all Modes, Origins, and Destinations for dropdowns
            ViewData["MSTTravelModes"] = Enum.GetValues(typeof(MSTTravelMode)).Cast<MSTTravelMode>();
            ViewData["Origins"] = mstZoneTravelOriginDTODataLoader.GetAllOrigins();
            ViewData["DestinationStates"] = mstZoneTravelDestinationDTODataLoader.GetAllDestinations();

            //Load Workspace Travel Agency Fee and Misc/Other Costs
            Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = zoneTravelRatesFeesLoader.getAllFeesAndCostsByWorkspace(ws.Id).ToDictionary(f => f.ModeID);

            ViewData["DomesticTravelAgencyFee"] = fees[(int)MSTTravelMode.NonZoneDomestic].TravelAgencyFee;
            ViewData["MiscOtherCosts"] = fees[(int)MSTTravelMode.NonZoneDomestic].MiscOther;
            ViewData["InternationalTravelAgencyFee"] = fees[(int)MSTTravelMode.NonZoneInternational].TravelAgencyFee;

            // get the BOE DTO
            FullBoe boe = this.Factory.CreateFullBoe(boeID);

            ViewBag.IsMulti = boe.IsMultiClinWbs;
            ViewData["BOEStartDate"] = boe.StartDate.ToString("MM/yyyy");
            ViewData["BOEEndDate"] = boe.EndDate.ToString("MM/yyyy");

            // Perform Action
            ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates = this.zoneTravelRatesFeesLoader.getAllEscalationRatesByWorkspace(ws.Id);

            Collection<BOEZoneTravelTripsGridModelView> theModelViews = zoneTravelControllerLogic.GetTravelTripsGridModelViews(ws, travelElementID, escalationRates);

            ViewResult toReturn = View(WebConstants.VIEW_ZONE_TRAVEL_TRIPS_GRID, theModelViews);

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEZoneTravelTripsGrid", sw);
            return toReturn;
        }

        /// <summary>
        /// Deletes all Zone Travel Tasks for  BOE
        /// </summary>
        /// <param name="workspace">Workspace containing the BOE</param>
        /// <param name="boeID">BOE containing the tasks</param>
        /// <returns>json result</returns>
        public JsonResult DeleteAllBOEZoneTravel(string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(_log, "DeleteAllBOETravel", SecurityPage.BoeLaborTypes,
               SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            zoneTravelControllerLogic.DeleteAllTravel(boeID);

            JsonResult toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, "DeleteAllBOETravel", sw);

            return toReturn;
        }

        /// <summary>
        /// Deletes single Zone Travel Task
        /// </summary>
        /// <param name="workspace">Workspace containing the BOE and task</param>
        /// <param name="boeID">BOE containing the task</param>
        /// <param name="TravelID">Task to be deleted</param>
        /// <returns>json result</returns>
        public JsonResult DeleteBOEZoneTravel(string workspace, int boeID, int TravelID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(_log, "DeleteBOETravel", SecurityPage.BoeLaborTypes, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            zoneTravelControllerLogic.DeleteTravelTask(boeID, TravelID);

            JsonResult toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, "DeleteBOETravel", sw);

            return toReturn;
        }

        /// <summary>
        /// reorders travel elements
        /// </summary>
        /// <param name="theModelView">user order</param>
        /// <param name="workspace">current workspace</param>
        /// <param name="boeID">boe id</param>
        /// <returns></returns>
        virtual public JsonResult SaveReorderZoneTravelTaskElements(TaskElementOrderCollection theModelView, string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            FullBoe boeObject = this.Factory.CreateFullBoe(boeID);


            if (theModelView == null)
            {
                throw new ArgumentNullException(nameof(theModelView));
            }

            if (boeObject.Travels.Count != theModelView.BOETaskElements.Count)
            {
                throw new ValidationException("Number of Task Elements in save does not match Number of Task Elements in Database.");
            }

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "GetDateShiftData", SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            
            _TravelControllerLogic.ReOrderTaskElementOrder(boeObject, theModelView);


            var toReturn = Json(new { Status = true });


            // Finalize Action
            FinalizeAction(_log, "GetDateShiftData", sw);
            return toReturn;
        }

        /// <summary>
        /// Perform validation (Required fields are populated, dates are in the proper range, costs and counts are not 0) and cost calculation (nonzone only) for an MST Travel Trip on Trip Add
        /// </summary>
        /// <param name="workspace">Workspace containing the Trip</param>
        /// <param name="boeID">ID of BOE containing the Trip</param>
        /// <param name="taskStartDate">Travel Task Start Date</param>
        /// <param name="taskEndDate">Travel Task End Date</param>
        /// <param name="dialogInputs">Inputs from the Add/Edit Trip dialog</param>
        /// <returns>JsonResult containing any nonzone costs and validation errors - if there are no errors, none are returned and the trip is added</returns>
        [HttpPost]
        virtual public JsonResult VerifyAndCalculateZoneTravelTrip(string workspace, int boeID, DateTime taskStartDate, DateTime taskEndDate, Collection<BOEZoneTravelTripsGridModelView> dialogInputs)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            if (dialogInputs == null) { throw new ArgumentNullException(nameof(dialogInputs)); }

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "VerifyAndCalculateZoneTravelTrip", SecurityPage.BoeLaborTypes, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates = this.zoneTravelRatesFeesLoader.getAllEscalationRatesByWorkspace(ws.Id);

            Collection<ValidationMessage> ValidationMessages = zoneTravelControllerLogic.ValidateTravelTrip(dialogInputs, boeID, taskStartDate, taskEndDate, escalationRates.Select(x => x.Year).Distinct().ToList());

            Dictionary<string, decimal> costs = new Dictionary<string, decimal>();

            // Only do calculations if there are no validation errors
            if (!ValidationMessages.Any())
            {
                foreach (BOEZoneTravelTripsGridModelView inputs in dialogInputs)
                {
                    // Calculations only needed for nonzone trips
                    if (inputs.ModeID == MSTTravelMode.NonZoneDomestic || inputs.ModeID == MSTTravelMode.NonZoneInternational)
                    {
                        zoneTravelControllerLogic.CalculateTripCostsForNonZone(new Collection<BOEZoneTravelTripsGridModelView>() { inputs }, ws.CostDecimalPrecision, ws.Id, escalationRates);
                        costs.Add(inputs.TravelTripID.ToString(), inputs.Cost);
                    }
                }
            }    

            var toReturn = Json(new { Status = true, result = costs, errors = ValidationMessages });

            // Finalize Action
            FinalizeAction(_log, "VerifyAndCalculateZoneTravelTrip", sw);

            return toReturn;
        }

        /// <summary>
        /// Saves Zone Travel task
        /// </summary>
        /// <param name="workspace">Workspace containing BOE and Task</param>
        /// <param name="boeID">BOE containing the teask</param>
        /// <param name="inDetailsMV">Travel Details Model View</param>
        /// <param name="inTravelTripsCollection">Trips Grid Model View</param>
        /// <returns>result of save</returns>
        virtual public ActionResult SaveEditZoneTravelDetailsComposite(string workspace, int boeID, BOETravelElementDetailsModelView inDetailsMV, Collection<BOEZoneTravelTripsGridModelView> inTravelTripsCollection)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "SaveEditZoneTravelDetailsComposite", SecurityPage.TaskElements,
                SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            if (inDetailsMV == null)
            {
                throw new ArgumentNullException(nameof(inDetailsMV));
            }

            if (inTravelTripsCollection == null)
            {
                inTravelTripsCollection = new Collection<BOEZoneTravelTripsGridModelView>();
            }

            // Perform Action
            ActionResult toReturn = Json(new { Status = false });

            // validate RTE field length
            if (ws.RteSizeLimit.HasValue)
            {
                if (!string.IsNullOrEmpty(inDetailsMV.TravelTaskDescription) && ws.RteSizeLimit < GenBOEUtilities.ConvertHtmlToText(inDetailsMV.TravelTaskDescription).Length)
                {
                    ModelState.AddModelError("Description", string.Format("The maximum length of Travel Task Description is {0} characters.", ws.RteSizeLimit.Value));
                }
            }

            /** Valid Model Check */
            if (ModelState.IsValid)
            {
                try
                {
                    ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates = this.zoneTravelRatesFeesLoader.getAllEscalationRatesByWorkspace(ws.Id);
                    
                    zoneTravelControllerLogic.SaveTravelTask(boeID, inDetailsMV, inTravelTripsCollection, escalationRates.Select(x => x.Year).Distinct().ToList());
                    toReturn = Json(new { Status = true });
                }
                catch (InvalidOperationException ex)
                {
                    _log.Error(ex.InnerException);
                }
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            // Finalize Action
            FinalizeAction(_log, "SaveEditZoneTravelDetailsComposite", sw);
            return toReturn;
        }
    }
}