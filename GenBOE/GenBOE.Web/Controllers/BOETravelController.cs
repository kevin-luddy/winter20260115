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
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView;
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

    public class BOETravelController : GenBOEController
    {
		private readonly Logger _log = new Logger(typeof(BOETravelController));
		private readonly TravelDTODataLoader _travelDTODataLoader = null;
        private readonly TripDTODataLoader _tripDTODataLoader = null;
		private readonly IMiscTravelRateDTOLoader miscTravelLoader = null;
		private readonly ILocationDTODataLoader _LocationDTODataLoader;
		private readonly PerDiemDTODataLoader _PerDiemLoader = null;
		private readonly ResourceDTODataLoader _ResourceLoader = null;
        private readonly TravelTripCostCalculation _TravelTripCostCalculator;
		private readonly IPerformingOrgDTODataLoader perfOrgLoader;
        private readonly ITravelControllerLogic _TravelControllerLogic;
        private readonly IBOELaborControllerLogic _BoeLaborControllerLogic = null;

        #region public methods

        /// <summary>
        /// Constructor
        /// </summary>
        public BOETravelController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper,
            SiteMasterUtilities inSiteMasterUtilities,
            TravelDTODataLoader inTravelDTODataLoader,
            TripDTODataLoader inTripDataLoader,
            IMiscTravelRateDTOLoader inMiscTravelLoader,
            PerDiemDTODataLoader inPerDiemLoader,
            LocationDTODataLoader inILocationDTODataLoader,
            ResourceDTODataLoader inResourceDTODataLoader,
            IUserDTODataLoader inUserLoader,
            IPermissionsDTODataLoader inPermissionsLoader,
            TravelTripCostCalculation inTravelTripCostCalculator,
            SystemMetrics inSystemMetrics,
            IFullObjectFactory factory,
            IPerformingOrgDTODataLoader perfOrgLoader,
            IGenBOEControllerLogic inControllerLogic,
            ITravelControllerLogic inTravelControllerLogic,
            IBOELaborControllerLogic inBoeLaborControllerLogic)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inUserLoader, inPermissionsLoader, inControllerLogic)
        {
            _travelDTODataLoader = inTravelDTODataLoader;
            _tripDTODataLoader = inTripDataLoader;
            miscTravelLoader = inMiscTravelLoader;
            _PerDiemLoader = inPerDiemLoader;
            _LocationDTODataLoader = inILocationDTODataLoader;
            _ResourceLoader = inResourceDTODataLoader;
            _TravelTripCostCalculator = inTravelTripCostCalculator;
            this.perfOrgLoader = perfOrgLoader;
            this._TravelControllerLogic = inTravelControllerLogic;
            this._BoeLaborControllerLogic = inBoeLaborControllerLogic;
        }

        [HttpPost]
		public virtual ViewResult DisplayBOETravelComposite(string workspace, int boeID, int? travelElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_TRAVEL_COMPOSITE, SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

            bool IsSubContractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
                                    where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
                                    select p).Any();
            if (IsSubContractor)
            {
                throw new AuthorizationException();
            }

            // Perform Action
            ViewData["BOEID"] = boeID;
            if (travelElementID.HasValue)
            {
                ViewData["TRAVELID"] = travelElementID.Value;

            }

            ViewResult toReturn = View(WebConstants.VIEW_TRAVEL_ELEMENT_COMPOSITE);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_TRAVEL_COMPOSITE, sw);
            return toReturn;
        }

		[HttpPost]
		public ViewResult DisplayBOETravelGrid(string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            bool IsSubContractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
                                    where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
                                    select p).Any();

            Stopwatch sw = null;
            try
            {
                // Initialize Action
                sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_TRAVEL_GRID, SecurityPage.BOETravelGrid, SecurityAuthorization.Read, ws, boeID);
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
                Collection<BOETravelGridModelView> theModelViews = new Collection<BOETravelGridModelView>();
                FullBoe boeObject = this.Factory.CreateFullBoe(boeID);

                if (boeObject.Travels.Any())
                {
                    foreach (TravelDTO travelDTO in boeObject.Travels)
                    {
                        decimal totalCost = 0;
                        foreach (TravelTripType tripType in travelDTO.TravelTrips)
                        {
                            TripDTO trip = ws.TravelTrips.First(i => i.TripID == tripType.SystemTripID);
                            MiscTravelRateDTO miscRateDTO = ws.MiscTravelRatesForTravelTrips.First(i => i.Id == trip.MiscTravelRateID);
                            PerDiemDTO perDiem = ws.PerDiemsForTravelTrips.First(i => i.Id == trip.PerDiemID);
                            totalCost += _TravelTripCostCalculator.CalculateTravelCost(tripType, ws, trip, miscRateDTO.MiscTravelRate, perDiem, ws.EscalationRates).CostTotal;
                        }

                        BOETravelGridModelView mv = new BOETravelGridModelView
                        {
                            TravelID = travelDTO.Id,
                            TaskID = travelDTO.TaskID,
                            TaskTitle = travelDTO.TaskTitle,
                            TaskStartDate = travelDTO.StartDate.HasValue ? travelDTO.StartDate.Value.ToString("MM/yyyy") : boeObject.StartDate.ToString("MM/yyyy"),
                            TaskEndDate = travelDTO.EndDate.HasValue ? travelDTO.EndDate.Value.ToString("MM/yyyy") : boeObject.EndDate.ToString("MM/yyyy"),
                            TotalCost = totalCost.ToString("N2"),
                            BOETaskElementOrder = travelDTO.BOETaskElementOrder
                        };

                        theModelViews.Add(mv);
                    }

                    theModelViews = theModelViews.OrderBy(teOrder => teOrder.BOETaskElementOrder).ThenBy(teOrder => teOrder.TravelID).ToCollection();
                }

				//create a var for list items
				Collection<SelectListItem> orderOfTaskElements = new Collection<SelectListItem>();
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


                toReturn = View(WebConstants.VIEW_BOE_TRAVEL_GRID, theModelViews);
            }
            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_TRAVEL_GRID, sw);
            return toReturn;
        }

		[ChildActionOnly, HttpGet]
        public ViewResult DisplayBOETravelElementDetails(string workspace, int boeID, int? travelElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_TRAVEL_ELEMENT_DETAILS, SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);


            // Perform Action
            BOETravelElementDetailsModelView theModelView;
            BoeDTO boe = this.Factory.CreateFullBoe(boeID);
            ViewData["BOEState"] = (int)boe.State;

            if (travelElementID.HasValue)
            {
                TravelDTO tempDTO = this.Factory.CreateTravel(travelElementID.Value);

                theModelView = new BOETravelElementDetailsModelView(tempDTO);
                if (!tempDTO.StartDate.HasValue || !tempDTO.EndDate.HasValue)
                {
                    if (!tempDTO.StartDate.HasValue)
                    {
                        theModelView.StartDate = boe.StartDate.ToString("MM/yyyy");
                    }
                    if (!tempDTO.EndDate.HasValue)
                    {
                        theModelView.EndDate = boe.EndDate.ToString("MM/yyyy");
                    }
                }

                #region Custom Fields

                Collection<CustomFieldSelectionModelView> selectionsViewModel = new Collection<CustomFieldSelectionModelView>();

                Collection<CustomFieldValueContainer> selections;
                if ((selections = tempDTO.CustomFieldValueContainers) != null)
                {
                    foreach (CustomFieldValueContainer selection in selections)
                    {
                        selectionsViewModel.Add(new CustomFieldSelectionModelView
                        {
                            CustomFieldValueID = selection.CustomFieldValueID,
                            SelectionID = selection.ContainerID,
                            UpdateDate = selection.UpdateDate
                        });
                    }

                    theModelView.CustomFieldValues = selectionsViewModel;
                }

                #endregion
            }
            else
            {
                theModelView = new BOETravelElementDetailsModelView();
                theModelView.BOEID = boeID;
                theModelView.StartDate = boe.StartDate.ToString("MM/yyyy");
                theModelView.EndDate = boe.EndDate.ToString("MM/yyyy");
            }
            theModelView.CustomFieldOptions = _BoeLaborControllerLogic.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.Task);

            ViewResult toReturn = View(WebConstants.VIEW_TRAVEL_ELEMENT_DETAILS, theModelView);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_TRAVEL_ELEMENT_DETAILS, sw);
            return toReturn;

        }

		[ChildActionOnly, HttpGet]
		public ViewResult DisplayBOETravelTrips(string workspace, int boeID, int? travelElementID)
        {
            FullWorkspace workspaceObject = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_TRAVEL_TRIPS, SecurityPage.BoeLaborTypes, SecurityAuthorization.Read, workspaceObject, boeID);

            ViewData["BOEID"] = boeID;
            ViewData["TRAVELID"] = travelElementID;

            ViewResult toReturn = View(WebConstants.VIEW_TRAVEL_TRIPS);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_TRAVEL_TRIPS, sw);
            return toReturn;

        }

		[HttpPost]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public ViewResult DisplayBOETravelTripsGrid(string workspace, int boeID, int? travelElementID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_TRAVEL_TRIPS_GRID, SecurityPage.BoeLaborTypes, SecurityAuthorization.Read, ws, boeID);

            ViewData["BOEID"] = boeID;
            ViewData["TravelElementId"] = travelElementID;

            ICollection<ResourceDTO> travelResources = _ResourceLoader.GetByListIdAndElementOfCost(ws.ResourceListID, ElementOfCostType.Travel);
            Collection<SegmentTypeModelView> wsTravelSegmentModelViews = CreateViewDataTravelTripSegments(travelResources);

            ViewData["TravelTripSegments"] = wsTravelSegmentModelViews;
            
            Collection<MiscRateModelView> modes = new Collection<MiscRateModelView>();

            HashSet<MiscTravelRateDTO> allMiscTravelRates = new HashSet<MiscTravelRateDTO>(miscTravelLoader.GetAll());
            foreach (MiscTravelRateDTO theDTO in allMiscTravelRates)
            {
                if (!theDTO.lockedRate)
                {
                    modes.Add(new MiscRateModelView(theDTO));
                }
            }

            ViewData["CustomFields"] = this._BoeLaborControllerLogic.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.LaborTypes);

            Collection<SelectListItem> destinations = new Collection<SelectListItem>();
            Collection<SelectListItem> departures = new Collection<SelectListItem>();

            ICollection<TripDTO> allTrips = _tripDTODataLoader.GetAllTrips();
            HashSet<int> allDepartureIDs = new HashSet<int>(allTrips.Select(x => x.DepartureLocationID).Distinct());
            HashSet<Tuple<int, int>> allDestinations = new HashSet<Tuple<int, int>>();
            foreach (TripDTO trip in allTrips)
            {
                Tuple<int, int> tripData = new Tuple<int, int>(trip.DestinationLocationID, trip.PerDiemID);
                if (!allDestinations.Contains(tripData))
                {
                    allDestinations.Add(tripData);
                }
            }

            List<int> departureAndDestinationIds = new List<int>();
            departureAndDestinationIds.AddRange(allDestinations.Select(x => x.Item1).ToList());
            departureAndDestinationIds.AddRange(allDepartureIDs);
            departureAndDestinationIds = departureAndDestinationIds.Distinct().ToList();
            ICollection<LocationDTO> departuresAndDestinations = _LocationDTODataLoader.GetByIds(departureAndDestinationIds);

            HashSet<LocationDTO> departureLocations = new HashSet<LocationDTO>(departuresAndDestinations.Where(x => allDepartureIDs.Contains(x.Id)));
            HashSet<LocationDTO> destinationLocations = new HashSet<LocationDTO>(departuresAndDestinations.Where(x => allDestinations.Select(i => i.Item1).Contains(x.Id)));

            foreach (int depID in allDepartureIDs)
            {
                SelectListItem departureLocation = new SelectListItem();
                departureLocation.Text = departureLocations.First(l => l.Id == depID).LocationName;
                departureLocation.Value = depID.ToString();
                departures.Add(departureLocation);
            }

            HashSet<PerDiemDTO> allPerdiems = new HashSet<PerDiemDTO>(_PerDiemLoader.GetByIds(allDestinations.Select(i => i.Item2).ToCollection<int>()));
            foreach (Tuple<int, int> destination in allDestinations)
            {
                SelectListItem destinationLocation = new SelectListItem();
                destinationLocation.Text = destinationLocations.First(l => l.Id == destination.Item1).LocationName;
                PerDiemDTO perDiem = allPerdiems.First(p => p.Id == destination.Item2);

                if (!string.IsNullOrEmpty(perDiem.Qualification))
                {
                    destinationLocation.Text += " - " + perDiem.Qualification;
                }

                destinationLocation.Value = destination.ToString();
                destinations.Add(destinationLocation);
            }

            ViewData["TravelTripModes"] = modes.OrderBy(x => x.MiscTravelRateMode);
            ViewData["Departures"] = departures.OrderBy(x => x.Text);
            ViewData["Destinations"] = destinations.OrderBy(x => x.Text);

            // get the BOE DTO
            FullBoe boe = this.Factory.CreateFullBoe(boeID);

            ViewData["BOEStartDate"] = boe.StartDate.ToString("MM/yyyy");
            ViewData["BOEEndDate"] = boe.EndDate.ToString("MM/yyyy");

            // Perform Action
            Collection<BOETravelTripsGridModelView> theModelViews = new Collection<BOETravelTripsGridModelView>();

            // for the given task element ID, get the labor types (no spreads needed)
            if (travelElementID.HasValue)
            {
                TravelDTO travel = this.Factory.CreateTravel(travelElementID.Value);
                HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(travel.TravelTrips.Select(x => x.PerfOrgID).Distinct().ToList()));

                HashSet<EscalationRatesDTO> escalations = new HashSet<EscalationRatesDTO>(ws.EscalationRates);

                foreach (TravelTripType TravelTrip in travel.TravelTrips)
                {
                    BOETravelTripsGridModelView travelTripToAdd = new BOETravelTripsGridModelView(TravelTrip);

                    TripDTO thisTrip = ws.TravelTrips.First(i => i.TripID == TravelTrip.SystemTripID);
                    PerDiemDTO perDiemDTO = ws.PerDiemsForTravelTrips.First(i => i.Id == thisTrip.PerDiemID);
                    MiscTravelRateDTO miscRateDto = ws.MiscTravelRatesForTravelTrips.First(x => x.Id == thisTrip.MiscTravelRateID);

                    travelTripToAdd.Mode = miscRateDto.MiscTravelRateMode;
                    travelTripToAdd.ModeID = thisTrip.MiscTravelRateID;
                    travelTripToAdd.PerDiemID = thisTrip.PerDiemID;
                    travelTripToAdd.DepartureID = thisTrip.DepartureLocationID;
                    travelTripToAdd.DepartureName = departureLocations.First(i => i.Id == thisTrip.DepartureLocationID).LocationName;
                    travelTripToAdd.PerformingOrgName = perfOrgsFromDb.First(x => x.Id == travelTripToAdd.PerformingOrgID).PerformingOrgName;
                    travelTripToAdd.DestinationName = string.IsNullOrEmpty(perDiemDTO.Qualification) ?
                        destinationLocations.First(i => i.Id == thisTrip.DestinationLocationID).LocationName :
                        destinationLocations.First(i => i.Id == thisTrip.DestinationLocationID).LocationName + " - " + perDiemDTO.Qualification;
                    travelTripToAdd.DestinationID = thisTrip.DestinationLocationID;

                    travelTripToAdd.Cost = (long)(_TravelTripCostCalculator.CalculateTravelCost(TravelTrip, ws, thisTrip, miscRateDto.MiscTravelRate, perDiemDTO, escalations).CostTotal * 100);
                    theModelViews.Add(travelTripToAdd);
                }
            }

            theModelViews = new Collection<BOETravelTripsGridModelView>((from r in theModelViews
                                                                         orderby r.GroupID, r.TripDate
                                                                         select r).ToArray());
            ViewData["ShowSegmentHelpLink"] = _TravelControllerLogic.ShowSegmentHelpLink;
            ViewResult toReturn = View(WebConstants.VIEW_TRAVEL_TRIPS_GRID, theModelViews);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_TRAVEL_TRIPS_GRID, sw);
            return toReturn;
        }

		[HttpPost]
		public JsonResult DeleteAllBOETravel(string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DELETE_ALL_BOE_TRAVEL, SecurityPage.BoeLaborTypes,
               SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                _travelDTODataLoader.DeleteAllTravelTripTaskElements(boeID);
                scope.Complete();
            }

            JsonResult toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DELETE_ALL_BOE_TRAVEL, sw);

            return toReturn;
        }

		[HttpPost]
		public JsonResult CalculateBOETravelTripCost(string workspace, int boeID, TravelTripType TravelTrip)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_CALCULATE_TRAVEL_TRIPS, SecurityPage.TaskElements,
                SecurityAuthorization.Read, ws, boeID);

            JsonResult toReturn = Json(new { Status = (long)(_TravelTripCostCalculator.CalculateTravelCost(TravelTrip, ws).CostTotal * 100) });

            FinalizeAction(_log, WebConstants.ACTION_CALCULATE_TRAVEL_TRIPS, sw);
            return toReturn;
        }

		[HttpPost]
		public JsonResult DeleteBOETravel(string workspace, int boeID, int TravelID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DELETE_BOE_TRAVEL, SecurityPage.BoeLaborTypes, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            TravelDTO dtoToDelete = this.Factory.CreateTravel(TravelID);

            DataRelationshipVerifier.VerifyDataRelation(dtoToDelete, boeID);

            dtoToDelete.Updateable = UpdateType.Deleted;

            Collection<TravelDTO> tempTravelCollToDel = new Collection<TravelDTO>();
            tempTravelCollToDel.Add(dtoToDelete);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                _travelDTODataLoader.SaveTravels(tempTravelCollToDel);
                scope.Complete();
            }

            JsonResult toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DELETE_BOE_TRAVEL, sw);

            return toReturn;
        }

		/// <summary>
		/// reorders travel elements
		/// </summary>
		/// <param name="theModelView">user order</param>
		/// <param name="workspace">current workspace</param>
		/// <param name="boeID">boe id</param>
		/// <returns></returns>
		[HttpPost]
		public virtual JsonResult SaveReorderTravelTaskElements(TaskElementOrderCollection theModelView, string workspace, int boeID)
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
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_REORDER_TRAVEL_TASK_ELEMENTS, SecurityPage.BoeLaborTypes, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);


            _TravelControllerLogic.ReOrderTaskElementOrder(boeObject, theModelView);

			JsonResult toReturn = Json(new { Status = true });


            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_REORDER_TRAVEL_TASK_ELEMENTS, sw);
            return toReturn;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[HttpPost]
		public virtual ActionResult SaveEditTravelDetailsComposite(string workspace, int boeID, BOETravelElementDetailsModelView inDetailsWV, Collection<BOETravelTripsGridModelView> inTravelTripsCollection)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_EDIT_TRAVEL_DETAILS_COMPOSITE, SecurityPage.TaskElements,
                SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            if (inDetailsWV == null)
            {
                throw new ArgumentNullException(nameof(inDetailsWV));
            }

            if (inTravelTripsCollection == null)
            {
                inTravelTripsCollection = new Collection<BOETravelTripsGridModelView>();
            }

            List<ValidationMessage> ValidationMessages = new List<ValidationMessage>();

            bool TravelTaskDateChanged = false;

            // Perform Action
            ActionResult toReturn = Json(new { Status = false });
            List<ValidationMessage> richTextValidationMessages = new List<ValidationMessage>();

            /** Valid Model Check */
            if (ModelState.IsValid)
            {
                //remove any new LaborType items that have also been deleted.
                //TODO I think this can be removed now.
                //31587
                foreach (BOETravelTripsGridModelView TravelTripType in inTravelTripsCollection)
                {
                    if (TravelTripType.TravelTripID < 0 && TravelTripType.Deleted == true)
                    {
                        inTravelTripsCollection.Remove(TravelTripType);
                    }
                }

                try
                {
                    richTextValidationMessages.AddRange(this.ScrubViewModelRichTextForSave(inDetailsWV));
                    Collection<CustomFieldValueContainer> TaskCustomFieldSelections = new Collection<CustomFieldValueContainer>();
                    Collection<TravelDTO> TravelDTOsTOSave = new Collection<TravelDTO>();
                    int newTaskCustomFieldId = 0;
                    // if the odc Element is new, then just add it to the editBOE
                    // everything in the odc element will be assumed new as well
                    if (inDetailsWV.TravelID < 0)
                    {
                        TravelDTO newTravel = new TravelDTO();
                        newTravel.Updateable = UpdateType.Upsert;
                        newTravel.TaskID = inDetailsWV.TaskID;
                        newTravel.Id = inDetailsWV.TravelID;
                        newTravel.TaskTitle = inDetailsWV.TaskTitle;
                        newTravel.Description = inDetailsWV.TravelTaskDescription;
                        if (inDetailsWV.StartDate != null)
                        {
                            newTravel.StartDate = inDetailsWV.StartDate.ToDateTimeMidMonth();
                        }
                        if (inDetailsWV.EndDate != null)
                        {
                            newTravel.EndDate = inDetailsWV.EndDate.ToDateTimeMidMonth();
                        }
                        newTravel.BoeID = boeID;
                        //if the taskelement is new we will save the order id with 2000. This is so the taskelement always goes to the bottom of the page.
                        newTravel.BOETaskElementOrder = 2000;

                        if (inDetailsWV.CustomFieldValues.Any())
                        {
                            foreach (CustomFieldSelectionModelView custom in inDetailsWV.CustomFieldValues)
                            {
                                UpdateType typeOfUpdate = UpdateType.None;
                                int customFieldValueID = 0;

                                typeOfUpdate = UpdateType.Upsert;
                                customFieldValueID = custom.CustomFieldValueID;

                                TaskCustomFieldSelections.Add(new CustomFieldValueContainer()
                                {
                                    Id = custom.SelectionID < 0 ? --newTaskCustomFieldId : custom.SelectionID,
                                    CustomFieldValueID = customFieldValueID,
                                    Updateable = typeOfUpdate,
                                    ContainerID = custom.SelectionID < 0 ? --newTaskCustomFieldId : custom.SelectionID,
                                    UpdateDate = custom.UpdateDate
                                });
                            }
                        }
                        newTravel.CustomFieldValueContainers = TaskCustomFieldSelections;
                        // handle types
                        Collection<TravelTripType> travelTrips = new Collection<TravelTripType>();

                        foreach (BOETravelTripsGridModelView travelTripMV in inTravelTripsCollection)
                        {
                            TravelTripType travelTripType = new TravelTripType();

                            travelTrips.Add(AddTripToTravel(boeID, travelTripType, travelTripMV));

                        }
                        newTravel.TravelTrips = travelTrips;
                        TravelDTOsTOSave.Add(newTravel);
                    }
                    else // task Element was an edit
                    {
                        TravelDTO existingTravel = this.Factory.CreateTravel(inDetailsWV.TravelID);
                        TravelDTO oldTravel = this.Factory.CreateTravel(inDetailsWV.TravelID);
                        DataRelationshipVerifier.VerifyDataRelation(existingTravel, boeID);

                        existingTravel.Updateable = UpdateType.Upsert;
                        existingTravel.TaskID = inDetailsWV.TaskID;
                        existingTravel.Id = inDetailsWV.TravelID;
                        existingTravel.TaskTitle = inDetailsWV.TaskTitle;
                        existingTravel.Description = inDetailsWV.TravelTaskDescription;

                        // need to find out if the travel task date changed for below validation checks
                        if (inDetailsWV.StartDate.ToDateTimeMidMonth() != GenBOEUtilities.AdjustDateTimePrecision(existingTravel.StartDate.Value, DateTimePrecision.Month))
                        {
                            TravelTaskDateChanged = true;
                        }
                        existingTravel.StartDate = inDetailsWV.StartDate.ToDateTimeMidMonth();

                        // need to find out if the travel task date changed for below validation checks
                        if (inDetailsWV.EndDate.ToDateTimeMidMonth() != GenBOEUtilities.AdjustDateTimePrecision(existingTravel.EndDate.Value, DateTimePrecision.Month))
                        {
                            TravelTaskDateChanged = true;
                        }
                        existingTravel.EndDate = inDetailsWV.EndDate.ToDateTimeMidMonth();
                        existingTravel.UpdateDate = inDetailsWV.UpdateDate;
                        existingTravel.BoeID = boeID;
                        if (inDetailsWV.CustomFieldValues.Any())
                        {
                            foreach (CustomFieldSelectionModelView custom in inDetailsWV.CustomFieldValues)
                            {
                                UpdateType typeOfUpdate = UpdateType.None;
                                int customFieldValueID = 0;
                                if (custom.CustomFieldValueID != -1)
                                {
                                    typeOfUpdate = UpdateType.Upsert;
                                    customFieldValueID = custom.CustomFieldValueID;
                                }
                                else
                                {
                                    typeOfUpdate = UpdateType.Deleted;

                                    customFieldValueID = oldTravel.CustomFieldValueContainers.Where(x => x.ContainerID == custom.SelectionID).Select(x => x.CustomFieldValueID).FirstOrDefault();
                                }
                                TaskCustomFieldSelections.Add(new CustomFieldValueContainer()
                                {
                                    Id = custom.SelectionID < 0 ? --newTaskCustomFieldId : custom.SelectionID,
                                    CustomFieldValueID = customFieldValueID,
                                    Updateable = typeOfUpdate,
                                    ContainerID = custom.SelectionID < 0 ? --newTaskCustomFieldId : custom.SelectionID,
                                    UpdateDate = custom.UpdateDate
                                });
                            }
                        }
                        existingTravel.CustomFieldValueContainers = TaskCustomFieldSelections;

                        // handle task element labors
                        Collection<TravelTripType> travelTrips = new Collection<TravelTripType>();

                        foreach (BOETravelTripsGridModelView travelTripMV in inTravelTripsCollection)
                        {
                            TravelTripType travelTripType = new TravelTripType();
                            if (travelTripMV.TravelTripID < 0 && !travelTripMV.Deleted)
                            {
                                travelTripType.Updateable = UpdateType.Upsert;

                                travelTrips.Add(AddTripToTravel(boeID, travelTripType, travelTripMV));
                            }
                            else
                            {
                                //Update existing
                                travelTripType = (from l in existingTravel.TravelTrips
                                                  where l.TravelTripID == travelTripMV.TravelTripID
                                                  select l).FirstOrDefault();
                                if (travelTripType != null)
                                {
                                    travelTrips.Add(AddTripToTravel(boeID, travelTripType, travelTripMV));
                                }
                            }

                        }
                        existingTravel.TravelTrips = travelTrips;
                        TravelDTOsTOSave.Add(existingTravel);
                    }

                    // perform date range validation
                    List<string> validationerrors = new List<string>();

                    // first validate the travelDTOs
                    BoeDTO boeDTO = this.Factory.CreateFullBoe(boeID);
                    StartEndDateTypeValidator validator = new StartEndDateTypeValidator(boeDTO.StartDate, boeDTO.EndDate, "BOE", "Task");
                    ICollection<IStartEndDates> toValidate = TravelDTOsTOSave.Cast<IStartEndDates>().Select(x => x).ToList();
                    validationerrors.AddRange(validator.validation(toValidate, (Collection<Dictionary<string, string>>)null));

                    Collection<Dictionary<string, string>> travelTaskDict = new Collection<Dictionary<string, string>>();
                    travelTaskDict.Add(new Dictionary<string, string>());
                    travelTaskDict.First<Dictionary<string, string>>().Add("BOEID", boeID.ToString());
                    travelTaskDict.First<Dictionary<string, string>>().Add("TaskID", inDetailsWV.TaskID);
                    travelTaskDict.First<Dictionary<string, string>>().Add("TravelID", inDetailsWV.TravelID.ToString());

                    Validator uniqueIdValidator = ValidationFactory.Instance.getValidator(ValidationType.BoeTaskIDUnique);
                    if (uniqueIdValidator.validation(inDetailsWV.TaskID, travelTaskDict).Any())
                    {
                        ValidationMessages.Add(new ValidationMessage("TaskID", "The Task ID must be unique among Labor, Travel and ODC task elements."));
                    }
                    Collection<BOECustomFieldModelView> TravelTaskElementCustomFields = _BoeLaborControllerLogic.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.Task);


                    foreach (BOECustomFieldModelView TripCF in TravelTaskElementCustomFields)
                    {
                        Collection<BOECustomFieldOptionModelView> requiredCustomFields = TripCF.CustomFieldMetaData.isRequired ? TripCF.CustomFieldOptions : null;
                        if (requiredCustomFields != null && requiredCustomFields.Any())
                        {
                            bool found = false;

                            ICollection<int> RequiredOptionIDs = (from AO in requiredCustomFields select AO.CustomFieldOptionID).ToList();
                            ICollection<int> SelectedValues = (from SV in inDetailsWV.CustomFieldValues select SV.CustomFieldValueID).ToList();

                            foreach (int sv in SelectedValues)
                            {
                                if (RequiredOptionIDs.Contains(sv))
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                ValidationMessages.Add(new ValidationMessage("CustomField", string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, TripCF.CustomFieldMetaData.FieldName)));
                            }
                        }
                    }


                    // gather errors up, if any
                    foreach (string error in validationerrors)
                    {
                        ValidationMessages.Add(new ValidationMessage("TaskElements", error));
                    }


                    Collection<BOECustomFieldModelView> TripCustomFields = _BoeLaborControllerLogic.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.LaborTypes);

                    // then validate each travel trip
                    foreach (TravelDTO travelDTO in TravelDTOsTOSave)
                    {
                        // for new or edited travel trips
                        foreach (TravelTripType travelTrip in travelDTO.TravelTrips)
                        {
                            if (travelTrip.Updateable != UpdateType.Deleted)
                            {
                                foreach (BOECustomFieldModelView TripCF in TripCustomFields)
                                {
                                    Collection<BOECustomFieldOptionModelView> RFOs = TripCF.CustomFieldMetaData.isRequired ? TripCF.CustomFieldOptions : null;
                                    if (RFOs != null && RFOs.Any())
                                    {
                                        bool found = false;

                                        ICollection<int> RequiredOptionIDs = (from AO in RFOs select AO.CustomFieldOptionID).ToList();
                                        ICollection<int> SelectedValues = (from SV in travelTrip.CustomFieldValueContainers select SV.CustomFieldValueID).ToList();

                                        foreach (int sv in SelectedValues)
                                        {
                                            if (RequiredOptionIDs.Contains(sv))
                                            {
                                                found = true;
                                                break;
                                            }
                                        }

                                        if (!found)
                                        {
                                            ValidationMessages.Add(new ValidationMessage("CustomField", string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, TripCF.CustomFieldMetaData.FieldName)));
                                        }
                                    }
                                }


                                // the date for the travel trip must be contained within the boe start/end date
                                if (GenBOEUtilities.AdjustDateTimePrecision(travelTrip.TripDate, DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision((travelDTO.StartDate.HasValue ? travelDTO.StartDate.Value : boeDTO.StartDate), DateTimePrecision.Month) && travelTrip.Updateable != UpdateType.Deleted)
                                {
                                    String message = "Trip Date for " + travelTrip.Purpose + " must start on or after the task start date";
                                    ValidationMessages.Add(new ValidationMessage("", message, "TravelTripsGridContainer"));

                                }

                                // adjust the date by adding the trip duration to the end date.
                                // afterwards we need to reset the date to the first of the new month .. since we don't care about 
                                // the day of the month, only month comparisons and everything is stored as 1st of the month
                                DateTime endDateWithTripDurationAdded = travelTrip.TripDate.AddDays(travelTrip.NumOfDays);
                                endDateWithTripDurationAdded = new DateTime(endDateWithTripDurationAdded.Year, endDateWithTripDurationAdded.Month, 1);
                                if (GenBOEUtilities.AdjustDateTimePrecision(endDateWithTripDurationAdded, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision((travelDTO.EndDate.HasValue ? travelDTO.EndDate.Value : boeDTO.EndDate), DateTimePrecision.Month) && travelTrip.Updateable != UpdateType.Deleted)
                                {
                                    String message = "Trip Date for " + travelTrip.Purpose + " must be finished before the task end date";
                                    ValidationMessages.Add(new ValidationMessage("", message, "TravelTripsGridContainer"));

                                }
                            }
                        }

                        // the above loop logic only works if the travel and trip both were changed since the UI only passes back what has been changed
                        // need to add an extra check to see if the travel dto trip changed only but no or only some of its trips were passed back
                        if (TravelTaskDateChanged)
                        {
                            // now that we know the travel task date changed and no trips were picked up, make sure there were no previously saved trips we need to check
                            ICollection<TravelTripType> savedTravelTrips = this.Factory.CreateTravel(travelDTO.Id).TravelTrips;

							IEnumerable<int> savedTripIDs = savedTravelTrips.Select(x => x.TravelTripID);
							IEnumerable<int> toSaveTripIDs = travelDTO.TravelTrips.Select(x => x.TravelTripID);
							IEnumerable<int> tripIDsToCheck = savedTripIDs.Except(toSaveTripIDs);

                            if (tripIDsToCheck.Count() != 0)
                            {
                                // just need the travel trips that weren't passed back for a save
                                foreach (TravelTripType travelTrip in savedTravelTrips)
                                {
                                    if (tripIDsToCheck.Contains(travelTrip.TravelTripID))
                                    {
                                        CheckTravelAndTravelTripDates(ValidationMessages, travelDTO, travelTrip);
                                    }
                                }
                            }
                        }
                    }

                    ValidationMessages.AddRange(richTextValidationMessages);

                    if (ValidationMessages.Any())
                    {
                        throw new GenValidationException(ValidationMessages);
                    }
                    else
                    {
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                        {
                            _travelDTODataLoader.SaveTravels(TravelDTOsTOSave);
                            scope.Complete();
                        }

                        toReturn = Json(new { Status = true });
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _log.Error(ex.InnerException);
                }
            }

            else if (!ModelState.IsValid)
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }
            else
            {
                toReturn = Json(new { Status = false });
            }

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_EDIT_TRAVEL_DETAILS_COMPOSITE, sw);
            return toReturn;

        }

        private TravelTripType AddTripToTravel(int boeID, TravelTripType travelTripType, BOETravelTripsGridModelView travelTripMV)
        {
            if (travelTripMV.Deleted)
            {
                travelTripType.Updateable = UpdateType.Deleted;
            }
            else
            {
                travelTripType.Updateable = UpdateType.Upsert;
                travelTripType.GroupID = travelTripMV.GroupID;
                travelTripType.NumOfDays = travelTripMV.numOfDays;
                travelTripType.NumOfPeople = travelTripMV.numOfPeople;
                travelTripType.NumOfTrips = travelTripMV.numOfTrips;
                travelTripType.Purpose = travelTripMV.Purpose;
                travelTripType.PerfOrgID = travelTripMV.PerformingOrgID;
                travelTripType.Segment = travelTripMV.Segment;
                travelTripType.SystemTripID = travelTripMV.SystemTripID;
                travelTripType.TravelTripID = travelTripMV.TravelTripID;
                travelTripType.TripDate = travelTripMV.TripDate;
                travelTripType.UpdateDate = travelTripMV.UpdateDate;
                travelTripType.BoeID = boeID;
                Collection<CustomFieldValueContainer> TripsCustomFieldSelections = new Collection<CustomFieldValueContainer>();
                if (travelTripMV.CustomFieldValues.Any())
                {
                    int newTripCustomFieldId = 0;
                    foreach (CustomFieldSelectionModelView custom in travelTripMV.CustomFieldValues)
                    {

                        UpdateType typeOfUpdate = UpdateType.None;
                        int customFieldValueID = 0;
                        if (custom.CustomFieldValueID != -1)
                        {
                            typeOfUpdate = UpdateType.Upsert;
                            customFieldValueID = custom.CustomFieldValueID;
                        }
                        else
                        {
                            typeOfUpdate = UpdateType.Deleted;

                            customFieldValueID = custom.CustomFieldValueID;
                        }


                        TripsCustomFieldSelections.Add(new CustomFieldValueContainer()
                        {
                            Id = custom.SelectionID < 0 ? --newTripCustomFieldId : custom.SelectionID,
                            CustomFieldValueID = customFieldValueID,
                            Updateable = typeOfUpdate,
                            ContainerID = custom.SelectionID < 0 ? --newTripCustomFieldId : custom.SelectionID,
                            UpdateDate = custom.UpdateDate
                        });
                    }
                }
                travelTripType.CustomFieldValueContainers = TripsCustomFieldSelections;
            }
            return travelTripType;
        }

		[HttpPost]
		public JsonResult GetFilteredTripData(string departure, string destination, string mode, string qualification)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

			LocationDTO[] allLocations = _LocationDTODataLoader.GetAllLocations().ToArray();
			MiscTravelRateDTO[] allMiscTravelRates = miscTravelLoader.GetAll().ToArray();
			TripDTO[] allTrips = _tripDTODataLoader.GetAllTrips().ToArray();
			PerDiemDTO[] allPerDiemDTOs = _PerDiemLoader.GetAllPerDiem().ToArray();

            HashSet<int> allDepartures = string.IsNullOrEmpty(departure) ?
                new HashSet<int>(allTrips.Select(x => x.DepartureLocationID).Distinct()) :
                new HashSet<int>(allLocations.Where(x => x.LocationName.IsEquivalentTo(departure)).Select(x => x.Id));

            HashSet<int> allDestinations = string.IsNullOrEmpty(destination) ?
                new HashSet<int>(allTrips.Select(x => x.DestinationLocationID).Distinct()) :
                new HashSet<int>(allLocations.Where(x => x.LocationName.IsEquivalentTo(destination)).Select(x => x.Id));

            HashSet<int> allPerDiems = string.IsNullOrEmpty(qualification) ?
                            (string.IsNullOrEmpty(destination) ?
                            new HashSet<int>(allPerDiemDTOs.Select(x => x.Id)) :
                            new HashSet<int>(allPerDiemDTOs.Where(x => string.IsNullOrEmpty(x.Qualification)).Select(x => x.Id))) :
                            new HashSet<int>(allPerDiemDTOs.Where(x => !string.IsNullOrEmpty(x.Qualification) && x.Qualification.IsEquivalentTo(qualification)).Select(x => x.Id));

            HashSet<int> allModes = string.IsNullOrEmpty(mode) ?
                new HashSet<int>(allTrips.Select(x => x.MiscTravelRateID)) :
                new HashSet<int>(allMiscTravelRates.Where(x => x.MiscTravelRateMode.IsEquivalentTo(mode)).Select(x => x.Id));

			// find all matches
			TripDTO[] filteredTrips = (from x in allTrips
                                 where allModes.Contains(x.MiscTravelRateID) &&
                                    allDepartures.Contains(x.DepartureLocationID) &&
                                    allDestinations.Contains(x.DestinationLocationID) &&
                                    allPerDiems.Contains(x.PerDiemID)
                                 select x).ToArray();

            SortedDictionary<string, string> departures = new SortedDictionary<string, string>();
            SortedDictionary<string, string> destinations = new SortedDictionary<string, string>();
            SortedDictionary<string, string> modes = new SortedDictionary<string, string>();

            foreach (TripDTO trip in filteredTrips)
            {
                LocationDTO departureDTO = allLocations.FirstOrDefault(x => x.Id == trip.DepartureLocationID);
                LocationDTO destinationDTO = allLocations.FirstOrDefault(x => x.Id == trip.DestinationLocationID);
                MiscTravelRateDTO modeDTO = allMiscTravelRates.FirstOrDefault(x => x.Id == trip.MiscTravelRateID);
                PerDiemDTO perDiemDTO = allPerDiemDTOs.FirstOrDefault(x => x.Id == trip.PerDiemID);

                if (!departures.ContainsKey(departureDTO.LocationName))
                {
                    departures.Add(departureDTO.LocationName, departureDTO.Id.ToString());
                }

                string destinationKey = string.IsNullOrEmpty(perDiemDTO.Qualification) ?
                    destinationDTO.LocationName :
                    destinationDTO.LocationName + " - " + perDiemDTO.Qualification;

                Tuple<int, int> destinationValue = new Tuple<int, int>(destinationDTO.Id, perDiemDTO.Id);

                // THIS ISSUE SEEMS TO BE RESOLVED, IF NOT USE COMMENTED CODE BELOW INSTEAD:
                // Limit the destination results incase the user tries to select this first.  IE can't handle more.  Once they select a departure this list gets refined anyway.
                //if (destinations.Count <= 500 && !destinations.ContainsKey(destinationKey))
                if (!destinations.ContainsKey(destinationKey))
                {
                    destinations.Add(destinationKey, destinationValue.ToString());
                }
                if (!String.IsNullOrEmpty(destination))
                {
                    // if the destination is populated we must consider the qualification
                    if (((String.IsNullOrEmpty(qualification) && String.IsNullOrEmpty(perDiemDTO.Qualification)) ||
                        (!String.IsNullOrEmpty(qualification) && qualification.CompareTo(perDiemDTO.Qualification) == 0)) &&
                        (!modes.ContainsKey(modeDTO.MiscTravelRateMode)))
                    {
                        modes.Add(modeDTO.MiscTravelRateMode, modeDTO.Id.ToString());
                    }
                }
                else
                {
                    if (!modes.ContainsKey(modeDTO.MiscTravelRateMode))
                    {
                        modes.Add(modeDTO.MiscTravelRateMode, modeDTO.Id.ToString());
                    }
                }
            }

            return Json(new
            {
                Departures = ConvertToOptionListWithText(departures, true),
                Destinations = ConvertToOptionListWithText(destinations, true),
                Modes = ConvertToOptionListWithText(modes, true)
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[HttpPost]
		public JsonResult GetTripID(BOETravelTripsGridModelView inTrip, String destination, String workspace, String qualification, string startDate, string endDate)
        {

            JsonResult toReturn = Json(new { Status = false });
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            if (string.IsNullOrEmpty(inTrip.DepartureName))
            {
                this.ModelState.AddModelError("DepartureName", "You must enter a Departure Loc from the list.");
            }

            if (string.IsNullOrEmpty(inTrip.DestinationName))
            {
                this.ModelState.AddModelError("DestinationName", "You must enter a Destination from the list.");
            }

            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime boeStartDate = DateTime.Parse(startDate);
                if (boeStartDate > inTrip.TripDate)
                {
                    ModelState.AddModelError("StartTripDate", "Trip Date has to be after Task Start Date");
                }
            }
            if (inTrip.numOfTrips == 0)
            {
                ModelState.AddModelError("NumOfTrips", "Trips cannot equal to 0");
            }

            Collection<BOECustomFieldModelView> TripCustomFields = _BoeLaborControllerLogic.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.LaborTypes);

            foreach (BOECustomFieldModelView TripCF in TripCustomFields)
            {
                Collection<BOECustomFieldOptionModelView> RFOs = TripCF.CustomFieldMetaData.isRequired ? TripCF.CustomFieldOptions : null;
                if (RFOs != null && RFOs.Any())
                {
                    bool found = false;

                    ICollection<int> RequiredOptionIDs = (from AO in RFOs select AO.CustomFieldOptionID).ToList();
                    ICollection<int> SelectedValues = (from SV in inTrip.CustomFieldValues select SV.CustomFieldValueID).ToList();

                    foreach (int sv in SelectedValues)
                    {
                        if (RequiredOptionIDs.Contains(sv))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        ModelState.AddModelError("CustomField", string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, TripCF.CustomFieldMetaData.FieldName));
                    }
                }
            }

            this.ValidateTripMultipleOccurences(inTrip);

            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime boeEndDate = DateTime.Parse(endDate);
                if (boeEndDate < inTrip.TripDate)
                {
                    ModelState.AddModelError("EndTripDate", "Trip Date has to be before Task End Date");
                }

                if (inTrip.Interval.HasValue && inTrip.numOfOccurrences.HasValue)
                {
                    // need to make sure that all of the new occurences will happen before the task end date
                    // we do not need to validate that they happen after the start date, that will be taken care of by validation above
                    for (int i = 1; i < inTrip.numOfOccurrences.Value; i++)
                    {
                        if (boeEndDate < inTrip.TripDate.AddMonths(i * inTrip.Interval.Value))
                        {
                            ModelState.AddModelError("EndTripDate" + i.ToString(), "For all trip occurences, the Trip Date has to be before Task End Date.");
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
				LocationDTO[] allLocations = _LocationDTODataLoader.GetAllLocations().ToArray();
				MiscTravelRateDTO[] allMiscTravelRates = miscTravelLoader.GetAll().ToArray();
				TripDTO[] allTrips = _tripDTODataLoader.GetAllTrips().ToArray();
				PerDiemDTO[] allPerDiemDTOs = _PerDiemLoader.GetAllPerDiem().ToArray();

                HashSet<int> allDepartures = string.IsNullOrEmpty(inTrip.DepartureName) ?
                    new HashSet<int>(allTrips.Select(x => x.DepartureLocationID).Distinct()) :
                    new HashSet<int>(allLocations.Where(x => x.LocationName.IsEquivalentTo(inTrip.DepartureName)).Select(x => x.Id));

                HashSet<int> allDestinations = string.IsNullOrEmpty(destination) ?
                    new HashSet<int>(allTrips.Select(x => x.DestinationLocationID).Distinct()) :
                    new HashSet<int>(allLocations.Where(x => x.LocationName.IsEquivalentTo(destination)).Select(x => x.Id));

                HashSet<int> allPerDiems = string.IsNullOrEmpty(qualification) ?
                    new HashSet<int>(allPerDiemDTOs.Select(x => x.Id)) :
                    new HashSet<int>(allPerDiemDTOs.Where(x => !string.IsNullOrEmpty(x.Qualification) && x.Qualification.IsEquivalentTo(qualification)).Select(x => x.Id));

                HashSet<int> allModes = string.IsNullOrEmpty(inTrip.Mode) ?
                    new HashSet<int>(allTrips.Select(x => x.MiscTravelRateID)) :
                    new HashSet<int>(allMiscTravelRates.Where(x => x.MiscTravelRateMode.IsEquivalentTo(inTrip.Mode)).Select(x => x.Id));

				IEnumerable<TripDTO> trips = (from x in allTrips
                             where allDepartures.Contains(x.DepartureLocationID) &&
                             allDestinations.Contains(x.DestinationLocationID) &&
                             allModes.Contains(x.MiscTravelRateID) &&
                             allPerDiems.Contains(x.PerDiemID)
                             select x);
                TripDTO SelectedTrip = null;
                foreach (TripDTO trip in trips)
                {
                    PerDiemDTO perDiemDTO = allPerDiemDTOs.Where(x => x.Id == trip.PerDiemID).SingleOrDefault();

                    if ((String.IsNullOrEmpty(qualification) && String.IsNullOrEmpty(perDiemDTO.Qualification)) ||
                        (!String.IsNullOrEmpty(qualification) && qualification.CompareTo(perDiemDTO.Qualification) == 0))
                    {
                        SelectedTrip = trip;
                        break;
                    }
                }
                if (SelectedTrip != null)
                {
                    toReturn = this.Json(new { Status = SelectedTrip.TripID });
                }
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            return toReturn;
        }


        #endregion public methods

        #region private methods

        /// <summary>
        /// Validates the multiple occurrences properties of a <see cref="BOETravelTripsGridModelView"/>
        /// and adds ModelError objects to the ModelState if needed
        /// </summary>
        /// <param name="inTrip">the <see cref="BOETravelTripsGridModelView"/> object to validate</param>
        private void ValidateTripMultipleOccurences(BOETravelTripsGridModelView inTrip)
        {
            if (inTrip.CreateMultiple && !inTrip.Interval.HasValue)
            {
                this.ModelState.AddModelError("Interval", "Interval must be a value from 1 to 99.");
            }

            if (inTrip.CreateMultiple && !inTrip.numOfOccurrences.HasValue)
            {
                this.ModelState.AddModelError("# Occurrences", "# Occurrences must be a value from 1 to 99.");
            }
        }

        /// <summary>
        /// This method will check if a travel trip is within the travel task's start/end date
        /// </summary>
        /// <param name="ValidationMessages">validation messages</param>
        /// <param name="inTravelDTO">travel dto(travel task element)</param>
        /// <param name="inTravelTrip">travel trip</param>
        private static void CheckTravelAndTravelTripDates(List<ValidationMessage> ValidationMessages, TravelDTO inTravelDTO, TravelTripType inTravelTrip)
        {
            // the date for the travel trip must be contained within the travel task start/end date
            if (GenBOEUtilities.AdjustDateTimePrecision(inTravelTrip.TripDate, DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision((inTravelDTO.StartDate.Value), DateTimePrecision.Month) && inTravelTrip.Updateable != UpdateType.Deleted)
            {
                String message = "Trip Date for " + inTravelTrip.Purpose + " must start on or after the task start date";
                ValidationMessages.Add(new ValidationMessage("", message, "TravelTripsGridContainer"));

            }

            // adjust the date by adding the trip duration to the end date.
            // afterwards we need to reset the date to the first of the new month .. since we don't care about 
            // the day of the month, only month comparisons and everything is stored as 1st of the month
            DateTime endDateWithTripDurationAdded = inTravelTrip.TripDate.AddDays(inTravelTrip.NumOfDays);
            endDateWithTripDurationAdded = new DateTime(endDateWithTripDurationAdded.Year, endDateWithTripDurationAdded.Month, 1);
            if (GenBOEUtilities.AdjustDateTimePrecision(endDateWithTripDurationAdded, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision((inTravelDTO.EndDate.Value), DateTimePrecision.Month) && inTravelTrip.Updateable != UpdateType.Deleted)
            {
                String message = "Trip Date for " + inTravelTrip.Purpose + " must be finished before the task end date";
                ValidationMessages.Add(new ValidationMessage("", message, "TravelTripsGridContainer"));

            }
        }

        /// <summary>
        /// Will create a list of segments to send to the view for the Add trip display.
        /// </summary>
        /// <param name="travelResources">List of travel resources</param>
        /// <returns>Collection of Segments where each segment only appears once.  </returns>
        private static Collection<SegmentTypeModelView> CreateViewDataTravelTripSegments(ICollection<ResourceDTO> travelResources)
        {
            Dictionary<int, SegmentTypeModelView> oneSegment = new Dictionary<int, SegmentTypeModelView>();
			foreach (ResourceDTO travelResource in travelResources)
            {
                SegmentTypeModelView tempSegment = new SegmentTypeModelView();
                tempSegment.SegmentTypeID = (int)travelResource.Segment;
                tempSegment.SegmentTypeName = travelResource.Segment.ToString();
                if (!oneSegment.ContainsKey(tempSegment.SegmentTypeID))
                {
                    oneSegment.Add(tempSegment.SegmentTypeID, tempSegment);
                }
            }
			Collection<SegmentTypeModelView> wsTravelSegmentModelViews = oneSegment.Values.ToCollection();
			return wsTravelSegmentModelViews;
        }

        #endregion private methods
    }
}
