// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using ActionLogic;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.NewValidation;
    using GenBOE.ActionLogic.ZoneTravel;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;

    public class BOEZoneTravelControllerLogic
    {
        private IFullObjectFactory factory;
        private IBOELaborControllerLogic boeLaborControllerLogic;
        private ITravelDTODataLoader travelDTODataLoader;
        private IMSTZoneTravelOriginDTODataLoader mstZoneTravelOriginDTODataLoader;
        private IMSTZoneTravelDestinationDTODataLoader mstZoneTravelDestinationDTODataLoader;
        private IMSTZoneTravelResourceDTODataLoader mstZoneTravelResourceDTODataLoader;
        private IPerformingOrgDTODataLoader perfOrgLoader;
        private IResourceDTODataLoader resourceDTODataLoader;
        private IGenBOEControllerLogic genBOEControllerLogic;
        private IMSTZoneTravelValidator mstZoneTravelValidator;
        private RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader;


        /// <summary>
        /// Constructor
        /// </summary>
        public BOEZoneTravelControllerLogic(
            IFullObjectFactory factory,
            IBOELaborControllerLogic boeLaborControllerLogic,
            ITravelDTODataLoader travelDTODataLoader,
            IMSTZoneTravelOriginDTODataLoader mstZoneTravelOriginDTODataLoader,
            IMSTZoneTravelDestinationDTODataLoader mstZoneTravelDestinationDTODataLoader,
            IMSTZoneTravelResourceDTODataLoader mstZoneTravelResourceDTODataLoader,
            IPerformingOrgDTODataLoader perfOrgLoader,
            IResourceDTODataLoader resourceDTODataLoader,
            IGenBOEControllerLogic genBOEControllerLogic,
            IMSTZoneTravelValidator mstZoneTravelValidator,
            RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader)
        {
            this.factory = factory;
            this.boeLaborControllerLogic = boeLaborControllerLogic;
            this.travelDTODataLoader = travelDTODataLoader;
            this.mstZoneTravelOriginDTODataLoader = mstZoneTravelOriginDTODataLoader;
            this.mstZoneTravelDestinationDTODataLoader = mstZoneTravelDestinationDTODataLoader;
            this.mstZoneTravelResourceDTODataLoader = mstZoneTravelResourceDTODataLoader;
            this.perfOrgLoader = perfOrgLoader;
            this.resourceDTODataLoader = resourceDTODataLoader;
            this.genBOEControllerLogic = genBOEControllerLogic;
            this.mstZoneTravelValidator = mstZoneTravelValidator;
            this.zoneTravelRatesFeesLoader = zoneTravelRatesFeesLoader;
        }

        /// <summary>
        /// Gets the BOE Travel Grid ModelViews
        /// </summary>
        /// <param name="boeID">ID of BOE containing Travel Grid</param>
        /// <param name="decimalPrecision">WS Decimal Precision</param>
        /// <param name="workspaceId">The WS id.</param>
        /// <param name="escalationRates">Airfare Escalation Rates</param>
        /// <returns>ModelViews for BOE Travel Grid</returns>
        public Collection<BOETravelGridModelView> GetTravelGridModelViews(int boeID, int decimalPrecision, int workspaceId, ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates)
        {
            Collection<BOETravelGridModelView> toReturn = new Collection<BOETravelGridModelView>();

            FullBoe boeObject = this.factory.CreateFullBoe(boeID);

            foreach (TravelDTO travelDTO in boeObject.Travels)
            {
                // Calculate all Non-Zone trip costs
                this.CalculateTripCostsForNonZone(travelDTO.MSTTravelTrips, decimalPrecision, workspaceId, escalationRates);
                decimal totalCost = travelDTO.MSTTravelTrips.Sum(x => x.Cost);

                toReturn.Add(new BOETravelGridModelView
                {
                    TravelID = travelDTO.Id,
                    TaskID = travelDTO.TaskID,
                    TaskTitle = travelDTO.TaskTitle,
                    TaskStartDate = travelDTO.StartDate.HasValue ? travelDTO.StartDate.Value.ToString("MM/yyyy") : boeObject.StartDate.ToString("MM/yyyy"),
                    TaskEndDate = travelDTO.EndDate.HasValue ? travelDTO.EndDate.Value.ToString("MM/yyyy") : boeObject.EndDate.ToString("MM/yyyy"),
                    TotalCost = totalCost.ToString("N2"),
                    BOETaskElementOrder = travelDTO.BOETaskElementOrder
                });
            }

            return toReturn.OrderBy(teOrder => teOrder.BOETaskElementOrder).ThenBy(teOrder => teOrder.TravelID).ToCollection();
        }

        /// <summary>
        /// Gets the Travel Element Details ModelView
        /// </summary>
        /// <param name="ws">Workspace containing the Travel</param>
        /// <param name="boe">BOE containing the Travel</param>
        /// <param name="travelElementID">ID of the Travel Element</param>
        /// <returns>Travel Element Details ModelView</returns>
        public BOETravelElementDetailsModelView GetTravelElementDetailsModelView(FullWorkspace ws, BoeDTO boe, int? travelElementID)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            BOETravelElementDetailsModelView toReturn;

            if (travelElementID.HasValue)
            {
                TravelDTO tempDTO = this.factory.CreateTravel(travelElementID.Value);

                toReturn = new BOETravelElementDetailsModelView(tempDTO);
                if (!tempDTO.StartDate.HasValue)
                {
                    toReturn.StartDate = boe.StartDate.ToString("MM/yyyy");
                }
                if (!tempDTO.EndDate.HasValue)
                {
                    toReturn.EndDate = boe.EndDate.ToString("MM/yyyy");
                }

                #region Custom Fields

                if (tempDTO.CustomFieldValueContainers != null)
                {
                    foreach (CustomFieldValueContainer selection in tempDTO.CustomFieldValueContainers)
                    {
                        toReturn.CustomFieldValues.Add(new CustomFieldSelectionModelView
                        {
                            CustomFieldValueID = selection.CustomFieldValueID,
                            SelectionID = selection.ContainerID,
                            CustomFieldID = selection.CustomFieldID,
                            UpdateDate = selection.UpdateDate,
                            IsOpenEnded = selection.IsOpenEnded,
                            OpenEndedValue = selection.OpenEndedValue
                        });
                    }
                }

                #endregion
            }
            else
            {
                toReturn = new BOETravelElementDetailsModelView();
                toReturn.BOEID = boe.Id;
                toReturn.StartDate = boe.StartDate.ToString("MM/yyyy");
                toReturn.EndDate = boe.EndDate.ToString("MM/yyyy");
            }

            toReturn.CustomFieldOptions = this.boeLaborControllerLogic.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.Task);

            return toReturn;
        }

        /// <summary>
        /// Gets the BOE Zone Travel Trips Grid ModelViews
        /// </summary>
        /// <param name="ws">Full Workspace</param>
        /// <param name="travelElementID">ID of the Travel Element</param>
        /// <param name="escalationRates">Airfare Escalation Rates</param>
        /// <returns>BOE Zone Travel Trips Grid ModelViews</returns>
        public Collection<BOEZoneTravelTripsGridModelView> GetTravelTripsGridModelViews(FullWorkspace ws, int? travelElementID,
            ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates)
        {
            if (ws == null) { throw new ArgumentNullException(nameof(ws)); }

            Collection<BOEZoneTravelTripsGridModelView> toReturn = new Collection<BOEZoneTravelTripsGridModelView>();
            if (!travelElementID.HasValue) { return toReturn; }

            TravelDTO travel = this.factory.CreateTravel(travelElementID.Value);
            HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(travel.MSTTravelTrips.Select(x => x.PerfOrgID).Distinct().ToList()));
            HashSet<ClinDTO> clinsFromDb = new HashSet<ClinDTO>(ws.Clins);
            HashSet<WbsDTO> wbsFromDb = new HashSet<WbsDTO>(ws.WbsElements);

            HashSet<ResourceDTO> nonZoneResourcesFromDb = new HashSet<ResourceDTO>(this.resourceDTODataLoader.GetByListIdAndElementOfCost(ws.ResourceListID, ElementOfCostType.Travel));

            // Calculate all Non-Zone trip costs
            this.CalculateTripCostsForNonZone(travel.MSTTravelTrips, ws.DecimalPrecision, ws.Id, escalationRates);

            foreach (MSTTravelTripType TravelTrip in travel.MSTTravelTrips)
            {
                BOEZoneTravelTripsGridModelView travelTripToAdd = new BOEZoneTravelTripsGridModelView(TravelTrip);

                travelTripToAdd.PerformingOrgName = perfOrgsFromDb.First(x => x.Id == travelTripToAdd.PerformingOrgID).PerformingOrgName;

                travelTripToAdd.DecimalPlaces = ws.DecimalPrecision;
                if (travelTripToAdd.ClinId.HasValue) { travelTripToAdd.ClinText = clinsFromDb.First(x => x.Id == travelTripToAdd.ClinId).ClinString; }
                if (travelTripToAdd.WbsId.HasValue) { travelTripToAdd.WbsText = wbsFromDb.First(x => x.Id == travelTripToAdd.WbsId).WbsString; }

                if (travelTripToAdd.ModeID == MSTTravelMode.ZoneAirfare || travelTripToAdd.ModeID == MSTTravelMode.ZoneNoAirfare)
                {
                    travelTripToAdd.OriginName = this.mstZoneTravelOriginDTODataLoader.GetOriginByOriginID((int)travelTripToAdd.OriginID).Origin;

                    MSTZoneTravelDestinationDTO destinationDTO = this.mstZoneTravelDestinationDTODataLoader.GetDestinationByDestinationID((int)travelTripToAdd.DestinationStateID);
                    travelTripToAdd.DestinationStateName = destinationDTO.Destination;
                    travelTripToAdd.Zone = destinationDTO.Zone;
                }

                if (travelTripToAdd.ModeID == MSTTravelMode.NonZoneDomestic || travelTripToAdd.ModeID == MSTTravelMode.NonZoneInternational)
                {
                    if (travelTripToAdd.NonZoneResourceID.HasValue && travelTripToAdd.NonZoneResourceID > 0)
                    {
                        travelTripToAdd.NonZoneResourceName = nonZoneResourcesFromDb.First(x => x.Id == travelTripToAdd.NonZoneResourceID).ResourceName;
                    }
                }
                toReturn.Add(travelTripToAdd);
            }
            return toReturn;
        }

        /// <summary>
        /// Validates inputs of a new/updated MST Travel Trip
        /// Required fields are populated, dates are in the proper range, costs and counts are not 0
        /// </summary>
        /// <param name="trips">Modelview of the Trip inputs</param>
        /// <param name="boeID">ID of the BOE containing the Trip</param>
        /// <param name="taskStartDate">Task Start Date</param>
        /// <param name="taskEndDate">Task End Date</param>
        /// <param name="escalationRateYears">Escalation Rate years (for validation)</param>
        public Collection<ValidationMessage> ValidateTravelTrip(Collection<BOEZoneTravelTripsGridModelView> trips, int boeID, DateTime taskStartDate, DateTime taskEndDate, ICollection<int> escalationRateYears)
        {
            if (trips == null) { throw new ArgumentNullException(nameof(trips)); }
            if (escalationRateYears == null) { throw new ArgumentNullException(nameof(escalationRateYears)); }

            List<ValidationMessage> validationMessages = new List<ValidationMessage>();

            string container = "addEditTripDialogForm";

            // Multiple trips only occurs with multiple occurences, where the only difference would be the date
            // For the rest of the fields, we only need to validate the first trip, otherwise we can get duplicate messages
            BOEZoneTravelTripsGridModelView firstTrip = trips.FirstOrDefault();
            if (firstTrip != null)
            {
                MSTTravelTripType tripType = this.AddTripToTravel(boeID, new MSTTravelTripType(), firstTrip);
                validationMessages.AddRange(this.mstZoneTravelValidator.ValidateTravelTrips(new Collection<MSTTravelTripType>() { tripType }, taskStartDate, taskEndDate, boeID, escalationRateYears, false, container));
            }

            // since trip dates are different for multiple occurences, we need to verify each one
            if (trips.Skip(1).Any())
            {
                Collection<MSTTravelTripType> tripTypes = new Collection<MSTTravelTripType>();

                // skip the first one since it was already verified above
                foreach (BOEZoneTravelTripsGridModelView trip in trips.Skip(1))
                {
                    tripTypes.Add(this.AddTripToTravel(boeID, new MSTTravelTripType(), trip));
                }

                ICollection<ValidationMessage> tripMessages = this.mstZoneTravelValidator.ValidateTripDateForMultipleOccurrences(tripTypes, taskStartDate, taskEndDate, container);

                if (tripMessages.Any())
                {
                    validationMessages.AddRange(tripMessages);
                }
            }

            return validationMessages.ToCollection();
        }

        /// <summary>
        /// Deletes All Travel Tasks and Trips for a BOE
        /// </summary>
        /// <param name="boeID">ID of BOE containing Travel to be deleted</param>
        public void DeleteAllTravel(int boeID)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.travelDTODataLoader.DeleteAllTravelTripTaskElements(boeID);
                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes single Travel Task and its Trips
        /// </summary>
        /// <param name="boeID">BOE Containing Travel</param>
        /// <param name="TravelID">ID of Travel Task to be deleted</param>
        public void DeleteTravelTask(int boeID, int TravelID)
        {
            TravelDTO dtoToDelete = this.factory.CreateTravel(TravelID);

            DataRelationshipVerifier.VerifyDataRelation(dtoToDelete, boeID);

            dtoToDelete.Updateable = UpdateType.Deleted;

            Collection<TravelDTO> tempTravelCollToDel = new Collection<TravelDTO>();
            tempTravelCollToDel.Add(dtoToDelete);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.travelDTODataLoader.SaveTravels(tempTravelCollToDel);
                scope.Complete();
            }
        }

        /// <summary>
        /// Saves new and updates existing Travel Tasks and associated Trips
        /// </summary>
        /// <param name="boeID">BOE containing Travel Task</param>
        /// <param name="inDetailsMV">Travel Task Details ModelView</param>
        /// <param name="inTravelTripsCollection">Travel Trips Grid ModelView</param>
        /// <param name="escalationRateYears">Escalation Rates (for validation)</param>
        public void SaveTravelTask(int boeID, BOETravelElementDetailsModelView inDetailsMV, Collection<BOEZoneTravelTripsGridModelView> inTravelTripsCollection, ICollection<int> escalationRateYears)
        {
            if (inDetailsMV == null)
            {
                throw new ArgumentNullException(nameof(inDetailsMV));
            }
            if (inTravelTripsCollection == null)
            {
                throw new ArgumentNullException(nameof(inTravelTripsCollection));
            }

            List<ValidationMessage> ValidationMessages = new List<ValidationMessage>();

            ValidationMessages.AddRange(this.genBOEControllerLogic.ScrubRichTextPropertiesForSave(inDetailsMV));

            TravelDTO travelDTOToSave;
            int newTaskCustomFieldId = 0;

            if (inDetailsMV.TravelID < 0)
            {
                TravelDTO newTravel = new TravelDTO();
                newTravel.Updateable = UpdateType.Upsert;
                newTravel.TaskID = inDetailsMV.TaskID;
                newTravel.Id = inDetailsMV.TravelID;
                newTravel.TaskTitle = inDetailsMV.TaskTitle;
                newTravel.Description = inDetailsMV.TravelTaskDescription;
                if (inDetailsMV.StartDate != null)
                {
                    newTravel.StartDate = inDetailsMV.StartDate.ToDateTimeMidMonth();
                }
                if (inDetailsMV.EndDate != null)
                {
                    newTravel.EndDate = inDetailsMV.EndDate.ToDateTimeMidMonth();
                }
                newTravel.BoeID = boeID;
                //if the taskelement is new we will save the order id with 2000. This is so the taskelement always goes to the bottom of the page.
                newTravel.BOETaskElementOrder = 2000;

                foreach (CustomFieldSelectionModelView custom in inDetailsMV.CustomFieldValues)
                {
                    newTravel.CustomFieldValueContainers.Add(new CustomFieldValueContainer()
                    {
                        Id = custom.SelectionID < 0 ? --newTaskCustomFieldId : custom.SelectionID,
                        CustomFieldID = custom.CustomFieldID,
                        CustomFieldValueID = custom.CustomFieldValueID,
                        Updateable = UpdateType.Upsert,
                        ContainerID = custom.SelectionID < 0 ? --newTaskCustomFieldId : custom.SelectionID,
                        IsOpenEnded = custom.IsOpenEnded,
                        OpenEndedValue = custom.OpenEndedValue,
                        UpdateDate = custom.UpdateDate
                    });
                }

                foreach (BOEZoneTravelTripsGridModelView travelTripMV in inTravelTripsCollection)
                {
                    newTravel.MSTTravelTrips.Add(this.AddTripToTravel(boeID, new MSTTravelTripType(), travelTripMV));
                }

                travelDTOToSave = newTravel;
            }
            else // task Element was an edit
            {
                TravelDTO existingTravel = this.factory.CreateTravel(inDetailsMV.TravelID);
                TravelDTO oldTravel = this.factory.CreateTravel(inDetailsMV.TravelID);
                DataRelationshipVerifier.VerifyDataRelation(existingTravel, boeID);

                existingTravel.Updateable = UpdateType.Upsert;
                existingTravel.TaskID = inDetailsMV.TaskID;
                existingTravel.Id = inDetailsMV.TravelID;
                existingTravel.TaskTitle = inDetailsMV.TaskTitle;
                existingTravel.Description = inDetailsMV.TravelTaskDescription;
                existingTravel.StartDate = inDetailsMV.StartDate.ToDateTimeMidMonth();
                existingTravel.EndDate = inDetailsMV.EndDate.ToDateTimeMidMonth();
                existingTravel.UpdateDate = inDetailsMV.UpdateDate;
                existingTravel.BoeID = boeID;
                existingTravel.CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();

                foreach (CustomFieldSelectionModelView custom in inDetailsMV.CustomFieldValues)
                {
                    existingTravel.CustomFieldValueContainers.Add(new CustomFieldValueContainer()
                    {
                        Id = custom.SelectionID < 0 ? --newTaskCustomFieldId : custom.SelectionID,
                        CustomFieldID = custom.CustomFieldID,
                        CustomFieldValueID = custom.CustomFieldValueID != -1 || custom.IsOpenEnded ? custom.CustomFieldValueID
                            : oldTravel.CustomFieldValueContainers.Where(x => x.ContainerID == custom.SelectionID).Select(x => x.CustomFieldValueID).FirstOrDefault(),
                        Updateable = (!custom.IsOpenEnded && custom.CustomFieldValueID != -1) || (custom.IsOpenEnded && !string.IsNullOrEmpty(custom.OpenEndedValue)) ? UpdateType.Upsert : (!custom.IsOpenEnded || custom.SelectionID > 0) ? UpdateType.Deleted : UpdateType.None,
                        ContainerID = custom.SelectionID < 0 ? --newTaskCustomFieldId : custom.SelectionID,
                        UpdateDate = custom.UpdateDate,
                        IsOpenEnded = custom.IsOpenEnded,
                        OpenEndedValue = custom.OpenEndedValue
                    });
                }

                // handle trips
                foreach (BOEZoneTravelTripsGridModelView travelTripMV in inTravelTripsCollection)
                {
                    MSTTravelTripType travelTripType = new MSTTravelTripType();
                    if (travelTripMV.TravelTripID < 0 && !travelTripMV.Deleted)
                    {
                        travelTripType.Updateable = UpdateType.Upsert;

                        existingTravel.MSTTravelTrips.Add(this.AddTripToTravel(boeID, travelTripType, travelTripMV));
                    }
                    else
                    {
                        //Update existing
                        travelTripType = (from l in existingTravel.MSTTravelTrips
                                          where l.Id == travelTripMV.TravelTripID
                                          select l).FirstOrDefault();
                        if (travelTripType != null)
                        {
                            existingTravel.MSTTravelTrips.Remove(travelTripType); // remove before adding updated
                            existingTravel.MSTTravelTrips.Add(this.AddTripToTravel(boeID, travelTripType, travelTripMV));
                        }
                    }

                }

                travelDTOToSave = existingTravel;
            }
            ValidationMessages.AddRange(this.mstZoneTravelValidator.ValidateTravelTaskDetails(travelDTOToSave, boeID));
            ValidationMessages.AddRange(this.mstZoneTravelValidator.ValidateTravelTrips(travelDTOToSave.MSTTravelTrips, travelDTOToSave.StartDate, travelDTOToSave.EndDate, boeID, escalationRateYears, true, "ZoneTravelTripsGridContainer"));

            if (ValidationMessages.Any())
            {
                throw new GenValidationException(ValidationMessages);
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.travelDTODataLoader.SaveTravels(new Collection<TravelDTO>() { travelDTOToSave });
                scope.Complete();
            }
        }

        /// <summary>
        /// Prepares Trip data to be added to Task for saving
        /// </summary>
        /// <param name="boeID">BOE containing Travel/Trip</param>
        /// <param name="travelTripType">Trip data before updated from ModelView</param>
        /// <param name="travelTripMV">ModelView containing rest of Trip data</param>
        /// <returns>Prepared Trip data</returns>
        private MSTTravelTripType AddTripToTravel(int boeID, MSTTravelTripType travelTripType, BOEZoneTravelTripsGridModelView travelTripMV)
        {
            if (travelTripMV.Deleted)
            {
                travelTripType.Updateable = UpdateType.Deleted;
                return travelTripType;
            }

            travelTripType.Updateable = UpdateType.Upsert;
            travelTripType.Id = travelTripMV.TravelTripID;
            travelTripType.Segment = SegmentType.RMS; //RMS is currently the only segment type for Zone Travel
            travelTripType.ModeID = travelTripMV.ModeID;
            travelTripType.GroupID = travelTripMV.GroupID;
            travelTripType.PerfOrgID = travelTripMV.PerformingOrgID;
            travelTripType.Purpose = travelTripMV.Purpose;
            travelTripType.EstimateDate = GenBOEUtilities.AdjustDateTimePrecision(travelTripMV.DateOfEstimate);
            travelTripType.TripDate = travelTripMV.EstTripDate;
            travelTripType.ClinId = travelTripMV.ClinId;
            travelTripType.WbsId = travelTripMV.WbsId;

            travelTripType.NumOfDays = travelTripMV.NumOfDays;
            travelTripType.NumOfPeople = travelTripMV.NumOfPeople;

            if (travelTripMV.ModeID == MSTTravelMode.ZoneAirfare || travelTripMV.ModeID == MSTTravelMode.ZoneNoAirfare)
            {
                travelTripType.ZoneOriginID = travelTripMV.OriginID ?? -1;
                travelTripType.ZoneOriginName = travelTripMV.OriginName;
                travelTripType.ZoneDestCity = travelTripMV.DestinationCity;
                travelTripType.ZoneDestinationID = travelTripMV.DestinationStateID ?? -1;
                travelTripType.ZoneDestinationName = travelTripMV.DestinationStateName;

                //Get Zone Travel Resource
                MSTZoneTravelResourceDTO resource = this.mstZoneTravelResourceDTODataLoader.GetResourceByOriginZoneAndMode((int)travelTripType.ZoneOriginID, travelTripMV.Zone, (travelTripMV.ModeID == MSTTravelMode.ZoneAirfare));
                travelTripType.ZoneResourceID = resource == null ? -1 : resource.ResourceID;
            }
            else if (travelTripMV.ModeID == MSTTravelMode.NonZoneDomestic || travelTripMV.ModeID == MSTTravelMode.NonZoneInternational)
            {
                travelTripType.NonZoneFrom = travelTripMV.FromLocation;
                travelTripType.NonZoneTo = travelTripMV.ToLocation;
                travelTripType.NonZoneAirfareEstimate = travelTripMV.AirfareEst;
                travelTripType.NonZonePerDiemDaily = travelTripMV.PerDiemDaily;
                travelTripType.NonZoneCarRentalTrans = travelTripMV.CarRentalTrans;
                travelTripType.NonZoneNumCars = travelTripMV.NumOfCars;
                travelTripType.NonZoneResourceID = travelTripMV.NonZoneResourceID;
            }

            travelTripType.UpdateDate = travelTripMV.UpdateDate;
            travelTripType.BoeID = boeID;

            int newTripCustomFieldId = 0;
            foreach (CustomFieldSelectionModelView custom in travelTripMV.CustomFieldValues)
            {
                travelTripType.CustomFieldValueContainers.Add(new CustomFieldValueContainer()
                {
                    Id = custom.SelectionID < 0 ? --newTripCustomFieldId : custom.SelectionID,
                    CustomFieldValueID = custom.CustomFieldValueID,
                    CustomFieldID = custom.CustomFieldID,
                    Updateable = (!custom.IsOpenEnded && custom.CustomFieldValueID != -1) || (custom.IsOpenEnded && !string.IsNullOrEmpty(custom.OpenEndedValue)) ? UpdateType.Upsert : (!custom.IsOpenEnded || custom.SelectionID > 0) ? UpdateType.Deleted : UpdateType.None,
                    ContainerID = custom.SelectionID < 0 ? --newTripCustomFieldId : custom.SelectionID,
                    UpdateDate = custom.UpdateDate,
                    IsOpenEnded = custom.IsOpenEnded,
                    OpenEndedValue = custom.OpenEndedValue
                });
            }
            return travelTripType;
        }

        /// <summary>
        /// Calculates Trip Cost for Non-Zone travel and sets it into the trip
        /// </summary>
        /// <param name="travelTrips">Travel trips to calculate</param>
        /// <param name="decimalPrecision">Decimal Precision</param>
        /// <param name="workspaceId">The id of the workspace to use for getting rates.</param>
        /// <param name="escalationRates">Escalation Rates</param>
        public void CalculateTripCostsForNonZone(ICollection<MSTTravelTripType> travelTrips, int decimalPrecision, int workspaceId,
            ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates)
        {
            if (travelTrips == null) { throw new ArgumentNullException(nameof(travelTrips)); }
            if (escalationRates == null) { throw new ArgumentNullException(nameof(escalationRates)); }

            Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = this.zoneTravelRatesFeesLoader.getAllFeesAndCostsByWorkspace(workspaceId).ToDictionary(f => f.ModeID);

            travelTrips.Where(x => x.ModeID == MSTTravelMode.NonZoneDomestic || x.ModeID == MSTTravelMode.NonZoneInternational).ToList()
                .ForEach(trip => trip.Cost = this.CalculateTripCostsForNonZone(new BOEZoneTravelTripsGridModelView(trip), fees[(int)trip.ModeID].TravelAgencyFee,
                fees[(int)trip.ModeID].MiscOther,
                escalationRates.ToDictionary(rate => rate.Year, rate => rate.AirfareRate),
                escalationRates.ToDictionary(rate => rate.Year, rate => rate.PerDiemRate),
                escalationRates.ToDictionary(rate => rate.Year, rate => rate.MiscRate),
                decimalPrecision));
        }

        /// <summary>
        /// Calculates Trip Cost for Non-Zone travel and sets it into the trip
        /// </summary>
        /// <param name="travelTrips">Travel trips to calculate</param>
        /// <param name="decimalPrecision">Decimal Precision</param>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="escalationRates">Escalation Rates</param>
        public void CalculateTripCostsForNonZone(ICollection<BOEZoneTravelTripsGridModelView> travelTrips, int decimalPrecision, int workspaceId, 
            ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates)
        {
            if (travelTrips == null) { throw new ArgumentNullException(nameof(travelTrips)); }
            if (escalationRates == null) { throw new ArgumentNullException(nameof(escalationRates)); }

            Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = this.zoneTravelRatesFeesLoader.getAllFeesAndCostsByWorkspace(workspaceId).ToDictionary(f => f.ModeID);

            travelTrips.Where(x => x.ModeID == MSTTravelMode.NonZoneDomestic || x.ModeID == MSTTravelMode.NonZoneInternational).ToList()
                .ForEach(trip => trip.Cost = this.CalculateTripCostsForNonZone(trip, fees[(int)trip.ModeID].TravelAgencyFee, fees[(int)trip.ModeID].MiscOther,
                escalationRates.ToDictionary(rate => rate.Year, rate => rate.AirfareRate),
                escalationRates.ToDictionary(rate => rate.Year, rate => rate.PerDiemRate),
                escalationRates.ToDictionary(rate => rate.Year, rate => rate.MiscRate),
                decimalPrecision));
        }

        /// <summary>
        /// Calculates the cost of a non-zone trip
        /// </summary>
        /// <param name="trip">Trip to calculate</param>
        /// <param name="travelAgencyFee">Travel Agency Fee</param>
        /// <param name="miscOtherFee">Misc Fee</param>
        /// <param name="airfareEscalationRates">Airfare Escalation Rates</param>
        /// <param name="perDiemEscalationRates">Per Diem Escalation Rates</param>
        /// <param name="miscEscalationRates">Misc Escalation Rates</param>
        /// <returns>Cost of the trip</returns>
        /// <param name="decimalPrecision">Decimal Precision</param>
        private decimal CalculateTripCostsForNonZone(BOEZoneTravelTripsGridModelView trip, decimal travelAgencyFee, decimal miscOtherFee, 
            Dictionary<int, decimal> airfareEscalationRates, Dictionary<int, decimal> perDiemEscalationRates, Dictionary<int, decimal> miscEscalationRates, int decimalPrecision)
        {
            NonZoneTravelCalculation calculationClass = new NonZoneTravelCalculation(
                    trip.NumOfDays ?? 0,
                    trip.NumOfPeople ?? 0,
                    trip.NumOfCars ?? 0,
                    trip.PerDiemDaily ?? 0,
                    trip.CarRentalTrans ?? 0,
                    trip.AirfareEst ?? 0,
                    airfareEscalationRates,
                    perDiemEscalationRates,
                    miscEscalationRates,
                    trip.YearOfEstimate ?? DateTime.Now.Year,
                    trip.EstTripDate.Year,
                    trip.ModeID == MSTTravelMode.NonZoneDomestic,
                    travelAgencyFee,
                    miscOtherFee,
                    decimalPrecision);

            decimal cost = Math.Round(calculationClass.TotalTripCost, 2);

            return cost;
        }
    }
}