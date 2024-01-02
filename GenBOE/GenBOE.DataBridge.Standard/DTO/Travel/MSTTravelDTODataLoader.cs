// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Standard;

    public class MSTTravelDTODataLoader : ITravelDTODataLoader
    {
        /// <summary>
        /// A private class to be used only in this file
        /// </summary>
        private class CustomKeyValuePair<T, TT>
        {
            public T Key { get; set; }
            public TT Value { get; set; }
        }

        private readonly ILogger _log;
        private ITravelTripTaskElementCustomFieldValueXREFLoader travelTripTaskElementCustomFieldLoader = null;
        private ITravelTripCustomFieldValueXREFLoader travelTripCustomFieldLoader = null;

        /// <summary>
        /// Loads travel element information
        /// </summary>
        /// <param name="travelTripTaskElementCustomFieldLoader">task element custom field loader</param>
        /// <param name="travelTripCustomFieldLoader">trip element custom field loader</param>
        public MSTTravelDTODataLoader(ITravelTripTaskElementCustomFieldValueXREFLoader travelTripTaskElementCustomFieldLoader, ITravelTripCustomFieldValueXREFLoader travelTripCustomFieldLoader, ILogger logger)
		{
			this._log = logger;
			this.travelTripTaskElementCustomFieldLoader = travelTripTaskElementCustomFieldLoader;
            this.travelTripCustomFieldLoader = travelTripCustomFieldLoader;
        }

        #region Retrieves

        /// <summary>
        /// Get travel data by travel Id.
        /// </summary>
        /// <param name="inTravelID">Travel Id.</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Travel Dto for given Id.</returns>
        virtual public TravelDTO GetById(int inTravelID, bool includeRTEFields = false)
        {
            return this.GetByIds(new Collection<int> { inTravelID }).FirstOrDefault();
        }

        /// <summary>
        /// Gets Travel data by Workspace Id
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        
        virtual public ICollection<TravelDTO> GetByIds(ICollection<int> ids, bool includeRTEFields = false)
        {
            List<TravelDTO> travels = new List<TravelDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    travels = (from t in gbe.TravelTripTaskElements
                               where ids.Contains(t.TravelTripTaskElementID)
                               select new TravelDTO
                               {
                                   // 1 RTE field:
                                   Description = includeRTEFields ? t.TravelTaskDescription : null,
                                   WasDescriptionSet = includeRTEFields,

                                   Id = t.TravelTripTaskElementID,
                                   TaskID = t.TravelTaskID,
                                   UpdateDate = t.UpdateDT,
                                   BoeID = t.BOEID,
                                   TaskTitle = t.TravelTaskTitle,
                                   StartDate = t.TaskStartDate,
                                   EndDate = t.TaskEndDate,
                                   BOETaskElementOrder = t.SortOrderID,
                                   CustomFieldValueContainersIEnum = t.TravelTripTaskElementCustomFieldValueXREFs.Select(cf => new CustomFieldValueContainer
                                   {
                                       ContainerID = cf.TTECFVID,
                                       CustomFieldValueID = cf.CustomFieldValueID,
                                       UpdateDate = cf.UpdateDT,
                                       CustomFieldID = cf.CustomFieldValue.CustomFieldID,
                                       IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
                                       OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription
                                   }),
                                   MSTTravelTripsIEnum = t.MSTTravelTrips.Select(tt =>
                                            new MSTTravelTripType
                                            {
                                                Id = tt.MSTTravelTripID,
                                                ModeID = (MSTTravelMode)tt.ModeID,
                                                GroupID = tt.GroupID,
                                                Segment = SegmentType.RMS,
                                                TripDate = tt.TripDate,
                                                Purpose = tt.Purpose,
                                                BoeID = t.BOEID,
                                                UpdateDate = tt.UpdateDT,
                                                PerfOrgID = tt.PerformingOrganization.PerformingOrganizationID,
                                                EstimateDate = tt.EstimateDate,
                                                NumOfDays = tt.NumDays,
                                                NumOfPeople = tt.NumPeople,
                                                ZoneOriginID = tt.MSTZoneTravelOrigin.OriginID,
                                                ZoneOriginName = tt.MSTZoneTravelOrigin.Origin ?? string.Empty,
                                                ZoneDestCity = tt.ZoneDestCity,
                                                ZoneDestinationID = tt.MSTZoneTravelDestination.DestinationID,
                                                ZoneDestinationName = tt.MSTZoneTravelDestination.Destination ?? string.Empty,
                                                ZoneResourceID = tt.MSTZoneTravelResource.ResourceID,
                                                ZoneDestinationZone = tt.MSTZoneTravelDestination.Zone,
                                                NonZoneFrom = tt.NonZoneFrom,
                                                NonZoneTo = tt.NonZoneTo,
                                                NonZoneAirfareEstimate = tt.NonZoneAirFareEstimate,
                                                NonZonePerDiemDaily = tt.NonZonePerDiemDaily,
                                                NonZoneCarRentalTrans = tt.NonZoneCarRentalTrans,
                                                NonZoneNumCars = tt.NonZoneNumCars,
                                                NonZoneResourceID = tt.NonZoneResourceID,
                                                ClinId = tt.ClinId,
                                                WbsId = tt.WbsId,
                                                ClinNumber = tt.CLIN.DisplayedCLINNumber,
                                                ClinTitle = tt.CLIN.CLINTitle,
                                                WbsNumber = tt.WorkBreakdownStructure.DisplayedWBSNumber,
                                                WbsTitle = tt.WorkBreakdownStructure.WBSTitle,
                                                CustomFieldValueContainersIEnum = tt.MSTTravelTripCustomFieldValueXREFs.Select(cf => new CustomFieldValueContainer
                                                {
                                                    ContainerID = cf.MSTTCFVID,
                                                    CustomFieldValueID = cf.CustomFieldValue.CustomFieldValueID,
                                                    UpdateDate = cf.UpdateDT,
                                                    CustomFieldID = cf.CustomFieldValue.CustomFieldID,
                                                    IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
                                                    OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription
                                                }),
                                                ResourceIdForExport = tt.ModeID == (int)MSTTravelMode.ZoneAirfare || tt.ModeID == (int)MSTTravelMode.ZoneNoAirfare
                                                    ? /*Zone - needs to be the per-diem*/
                                                    // Find the correct origin, then look through the associated resources to find the correct one based on the Zone & it being Per Diem 
                                                    gbe.MSTZoneTravelOrigins.Where(tO => tO.OriginID == tt.MSTZoneTravelResource.OriginID).SelectMany(tO => tO.MSTZoneTravelResources)
                                                        .FirstOrDefault(x => x.Zone == tt.MSTZoneTravelResource.Zone && !x.isAirfare).Resource

                                                    : /*Non-Zone*/ tt.Resource.ResourceName,
                                                SecondaryResourceIdForExport = tt.ModeID == (int)MSTTravelMode.ZoneAirfare 
                                                    ? /* Zone w/ airfare - needs to be the airfare resource, assuming that the airfare was selected*/
                                                    // Find the correct origin, then look through the associated resources to find the correct one based on the Zone & it being airfare
                                                    gbe.MSTZoneTravelOrigins.Where(tO => tO.OriginID == tt.MSTZoneTravelResource.OriginID).SelectMany(tO => tO.MSTZoneTravelResources)
                                                        .FirstOrDefault(x => x.Zone == tt.MSTZoneTravelResource.Zone && x.isAirfare).Resource
                                                    : null
                                            })
                               }).ToList();
                }

                travels.ForEach(
                        x =>
                        {
                            x.CustomFieldValueContainers = x.CustomFieldValueContainersIEnum.ToCollection(); x.CustomFieldValueContainersIEnum = null;
                            x.MSTTravelTrips = x.MSTTravelTripsIEnum.ToCollection();
                            x.MSTTravelTrips.ToList().ForEach(tt =>
                            {
                                tt.CustomFieldValueContainers = tt.CustomFieldValueContainersIEnum.ToCollection(); tt.CustomFieldValueContainersIEnum = null;
                            });
                            x.MSTTravelTripsIEnum = null;
                            x.StartDate = x.StartDate.Normalize();
                            x.EndDate = x.EndDate.Normalize();
                        }
                    );
            }

            return travels;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        
        virtual public ICollection<TravelDTO> GetByWorkspaceId(int wsId, bool includeRTEFields = false)
        {
            List<TravelDTO> travels = new List<TravelDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                List<CLIN> clins;
                List<WorkBreakdownStructure> wbs;
                List<CustomKeyValuePair<int, CustomFieldValueContainer>> cfsTravels;
                List<CustomKeyValuePair<int, CustomFieldValueContainer>> cfsTravelTrips;
                List<MSTZoneTravelResource> mstZoneTravelResources;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    travels = gbe.TravelTripTaskElements.Where(t => t.BOE.WorkspaceID == wsId).Select(t => new TravelDTO
                              {
                                // 1 RTE field:
                                Description = includeRTEFields ? t.TravelTaskDescription : null,
                                WasDescriptionSet = includeRTEFields,

                                Id = t.TravelTripTaskElementID,
                                TaskID = t.TravelTaskID,
                                UpdateDate = t.UpdateDT,
                                BoeID = t.BOEID,
                                TaskTitle = t.TravelTaskTitle,
                                StartDate = t.TaskStartDate,
                                EndDate = t.TaskEndDate,
                                BOETaskElementOrder = t.SortOrderID,
                                MSTTravelTripsIEnum = t.MSTTravelTrips.Select(tt => new MSTTravelTripType
                                {
                                    Id = tt.MSTTravelTripID,
                                    ModeID = (MSTTravelMode)tt.ModeID,
                                    GroupID = tt.GroupID,
                                    Segment = SegmentType.RMS,
                                    TripDate = tt.TripDate,
                                    Purpose = tt.Purpose,
                                    BoeID = t.BOEID,
                                    UpdateDate = tt.UpdateDT,
                                    PerfOrgID = tt.PerformingOrganization.PerformingOrganizationID,
                                    EstimateDate = tt.EstimateDate,
                                    NumOfDays = tt.NumDays,
                                    NumOfPeople = tt.NumPeople,
                                    ZoneOriginID = tt.MSTZoneTravelOrigin.OriginID,
                                    ZoneOriginName = tt.MSTZoneTravelOrigin.Origin ?? string.Empty,
                                    ZoneDestCity = tt.ZoneDestCity,
                                    ZoneDestinationID = tt.MSTZoneTravelDestination.DestinationID,
                                    ZoneDestinationName = tt.MSTZoneTravelDestination.Destination ?? string.Empty,
                                    ZoneResourceID = tt.MSTZoneTravelResource.ResourceID,
                                    ResourceZoneId = tt.MSTZoneTravelResource.Zone,
                                    ResourceOriginId = tt.MSTZoneTravelResource.OriginID,
                                    ZoneDestinationZone = tt.MSTZoneTravelDestination.Zone,
                                    NonZoneFrom = tt.NonZoneFrom,
                                    NonZoneTo = tt.NonZoneTo,
                                    NonZoneAirfareEstimate = tt.NonZoneAirFareEstimate,
                                    NonZonePerDiemDaily = tt.NonZonePerDiemDaily,
                                    NonZoneCarRentalTrans = tt.NonZoneCarRentalTrans,
                                    NonZoneNumCars = tt.NonZoneNumCars,
                                    NonZoneResourceID = tt.NonZoneResourceID,
                                    ClinId = tt.ClinId,
                                    WbsId = tt.WbsId,
                                    ResourceIdForExport = tt.Resource.ResourceName,
                                    SecondaryResourceIdForExport = null
                                })
                             }).ToList();

                    clins = gbe.CLINs.Where(x => x.WorkspaceID == wsId).ToList();
                    wbs = gbe.WorkBreakdownStructures.Where(x => x.WorkspaceID == wsId).ToList();

                    cfsTravels = gbe.TravelTripTaskElements.Where(t => t.BOE.WorkspaceID == wsId)
                            .SelectMany(x => x.TravelTripTaskElementCustomFieldValueXREFs)
                            .Select(cf => new CustomKeyValuePair<int, CustomFieldValueContainer>() {
                                Key = cf.TravelTripTaskElementID,
                                Value = new CustomFieldValueContainer()
                                {
                                    ContainerID = cf.TTECFVID,
                                    UpdateDate = cf.UpdateDT,
                                    CustomFieldValueID = cf.CustomFieldValueID,
                                    CustomFieldID = cf.CustomFieldValue.CustomFieldID,
                                    IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
                                    OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription
                                }
                            }).ToList();

                    cfsTravelTrips = gbe.TravelTripTaskElements.Where(t => t.BOE.WorkspaceID == wsId)
                            .SelectMany(x => x.MSTTravelTrips.SelectMany(z => z.MSTTravelTripCustomFieldValueXREFs))
                            .Select(cf => new CustomKeyValuePair<int, CustomFieldValueContainer>() { 
                                Key = cf.MSTTravelTripID,
                                Value = new CustomFieldValueContainer()
                                {
                                    ContainerID = cf.MSTTCFVID,
                                    UpdateDate = cf.UpdateDT,
                                    CustomFieldValueID = cf.MSTCustomFieldValueID,
                                    CustomFieldID = cf.CustomFieldValue.CustomFieldID,
                                    IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
                                    OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription
                                }
                            }).ToList();

                    // there's about 400 records there, and filtering these is a pain, so we'll just grab them all. it's a system level dataset
                    mstZoneTravelResources = gbe.MSTZoneTravelResources.ToList();
                }

                travels.ForEach(x =>
                {
                    x.CustomFieldValueContainers = cfsTravels.Where(z => z.Key == x.Id).Select(z => z.Value).ToCollection();

                    x.MSTTravelTrips = x.MSTTravelTripsIEnum.ToCollection();
                    x.MSTTravelTrips.ToList().ForEach(tt =>
                    {
                        tt.CustomFieldValueContainers = cfsTravelTrips.Where(z => z.Key == tt.Id).Select(z => z.Value).ToCollection();

                        if (tt.ClinId.HasValue)
                        {
                            tt.ClinNumber = clins.First(z => z.CLINID == tt.ClinId).DisplayedCLINNumber;
                            tt.ClinTitle = clins.First(z => z.CLINID == tt.ClinId).CLINTitle;
                        }

                        if(tt.WbsId.HasValue)
                        {
                            tt.WbsNumber = wbs.First(z => z.WBSID == tt.WbsId).DisplayedWBSNumber;
                            tt.WbsTitle = wbs.First(z => z.WBSID == tt.WbsId).WBSTitle;
                        }

                        if(tt.ModeID == MSTTravelMode.ZoneAirfare || tt.ModeID == MSTTravelMode.ZoneNoAirfare)
                        {
                            tt.ResourceIdForExport = mstZoneTravelResources
                                    .FirstOrDefault(y => tt.ResourceOriginId == y.OriginID && tt.ResourceZoneId == y.Zone && !y.isAirfare)?.Resource;
                        }

                        if(tt.ModeID == MSTTravelMode.ZoneAirfare)
                        {
                            tt.SecondaryResourceIdForExport = mstZoneTravelResources
                                    .FirstOrDefault(y => tt.ResourceOriginId == y.OriginID && tt.ResourceZoneId == y.Zone && y.isAirfare)?.Resource;
                        }
                    });
                    x.MSTTravelTripsIEnum = null;
                    x.StartDate = x.StartDate.Normalize();
                    x.EndDate = x.EndDate.Normalize();
                });
            }

            return travels;
        }

        /// <summary>
        /// Gets Travel data by Boe Ids
        /// </summary>
        /// <param name="boeIds">Boe Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        
        virtual public ICollection<TravelDTO> GetByBoeIds(ICollection<int> boeIds, bool includeRTEFields = false)
        {
            List<TravelDTO> travels = new List<TravelDTO>();
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    travels = (from t in gbe.TravelTripTaskElements
                               where boeIds.Contains(t.BOEID)
                               select new TravelDTO
                               {
                                   // 1 RTE field:
                                   Description = includeRTEFields ? t.TravelTaskDescription : null,
                                   WasDescriptionSet = includeRTEFields,

                                   Id = t.TravelTripTaskElementID,
                                   TaskID = t.TravelTaskID,
                                   UpdateDate = t.UpdateDT,
                                   BoeID = t.BOEID,
                                   TaskTitle = t.TravelTaskTitle,
                                   StartDate = t.TaskStartDate,
                                   EndDate = t.TaskEndDate,
                                   BOETaskElementOrder = t.SortOrderID,
                                   CustomFieldValueContainersIEnum = t.TravelTripTaskElementCustomFieldValueXREFs.Select(cf => new CustomFieldValueContainer
                                            {
                                                ContainerID = cf.TTECFVID,
                                                CustomFieldValueID = cf.CustomFieldValueID,
                                                UpdateDate = cf.UpdateDT,
                                                CustomFieldID = cf.CustomFieldValue.CustomFieldID,
                                                IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
                                                OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription
                                   }),
                                   MSTTravelTripsIEnum = t.MSTTravelTrips.Select(tt =>
                                            new MSTTravelTripType
                                            {
                                                Id = tt.MSTTravelTripID,
                                                ModeID = (MSTTravelMode)tt.ModeID,
                                                GroupID = tt.GroupID,
                                                Segment = SegmentType.RMS,
                                                TripDate = tt.TripDate,
                                                Purpose = tt.Purpose,
                                                BoeID = t.BOEID,
                                                UpdateDate = tt.UpdateDT,
                                                PerfOrgID = tt.PerformingOrganization.PerformingOrganizationID,
                                                EstimateDate = tt.EstimateDate,
                                                NumOfDays = tt.NumDays,
                                                NumOfPeople = tt.NumPeople,
                                                ZoneOriginID = tt.MSTZoneTravelOrigin.OriginID,
                                                ZoneOriginName = tt.MSTZoneTravelOrigin.Origin ?? string.Empty,
                                                ZoneDestCity = tt.ZoneDestCity,
                                                ZoneDestinationID = tt.MSTZoneTravelDestination.DestinationID,
                                                ZoneDestinationName = tt.MSTZoneTravelDestination.Destination ?? string.Empty,
                                                ZoneResourceID = tt.MSTZoneTravelResource.ResourceID,
                                                ZoneDestinationZone = tt.MSTZoneTravelDestination.Zone,
                                                NonZoneFrom = tt.NonZoneFrom,
                                                NonZoneTo = tt.NonZoneTo,
                                                NonZoneAirfareEstimate = tt.NonZoneAirFareEstimate,
                                                NonZonePerDiemDaily = tt.NonZonePerDiemDaily,
                                                NonZoneCarRentalTrans = tt.NonZoneCarRentalTrans,
                                                NonZoneNumCars = tt.NonZoneNumCars,
                                                NonZoneResourceID = tt.NonZoneResourceID,
                                                ClinId = tt.ClinId,
                                                WbsId = tt.WbsId,
                                                ClinNumber = tt.CLIN.DisplayedCLINNumber,
                                                ClinTitle = tt.CLIN.CLINTitle,
                                                WbsNumber = tt.WorkBreakdownStructure.DisplayedWBSNumber,
                                                WbsTitle = tt.WorkBreakdownStructure.WBSTitle,
                                                CustomFieldValueContainersIEnum = tt.MSTTravelTripCustomFieldValueXREFs.Select(cf => new CustomFieldValueContainer
                                                {
                                                    ContainerID = cf.MSTTCFVID,
                                                    CustomFieldValueID = cf.CustomFieldValue.CustomFieldValueID,
                                                    UpdateDate = cf.UpdateDT,
                                                    CustomFieldID = cf.CustomFieldValue.CustomFieldID,
                                                    IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
                                                    OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription
                                                }),
                                                ResourceIdForExport = tt.ModeID == (int)MSTTravelMode.ZoneAirfare || tt.ModeID == (int)MSTTravelMode.ZoneNoAirfare
                                                    ? /*Zone - needs to be the per-diem*/
                                                      // Find the correct origin, then look through the associated resources to find the correct one based on the Zone & it being Per Diem 
                                                    gbe.MSTZoneTravelOrigins.Where(tO => tO.OriginID == tt.MSTZoneTravelResource.OriginID).SelectMany(tO => tO.MSTZoneTravelResources)
                                                        .FirstOrDefault(x => x.Zone == tt.MSTZoneTravelResource.Zone && !x.isAirfare).Resource

                                                    : /*Non-Zone*/ tt.Resource.ResourceName,
                                                SecondaryResourceIdForExport = tt.ModeID == (int)MSTTravelMode.ZoneAirfare
                                                    ? /* Zone w/ airfare - needs to be the airfare resource, assuming that the airfare was selected*/
                                                      // Find the correct origin, then look through the associated resources to find the correct one based on the Zone & it being airfare
                                                    gbe.MSTZoneTravelOrigins.Where(tO => tO.OriginID == tt.MSTZoneTravelResource.OriginID).SelectMany(tO => tO.MSTZoneTravelResources)
                                                        .FirstOrDefault(x => x.Zone == tt.MSTZoneTravelResource.Zone && x.isAirfare).Resource
                                                    : null
                                            })
                               }).ToList();
                }

                travels.ForEach(
                        x =>
                        {
                            x.CustomFieldValueContainers = x.CustomFieldValueContainersIEnum.ToCollection(); x.CustomFieldValueContainersIEnum = null;
                            x.MSTTravelTrips = x.MSTTravelTripsIEnum.ToCollection();
                            x.MSTTravelTrips.ToList().ForEach(tt =>
                            {
                                tt.CustomFieldValueContainers = tt.CustomFieldValueContainersIEnum.ToCollection(); tt.CustomFieldValueContainersIEnum = null;
                            });
                            x.MSTTravelTripsIEnum = null;
                            x.StartDate = x.StartDate.Normalize();
                            x.EndDate = x.EndDate.Normalize();
                        }
                    );
            }

            return travels;
        }

        #region RTE Load Methods

        /// <summary>
        /// Pulls RTE fields for the DTOs, and updates them as needed
        /// </summary>
        /// <param name="dtos">DTOs that whose RTE fields will be loaded, if they are null</param>
        
        public void LoadRTEFields(ICollection<TravelDTO> dtos)
        {
            if (dtos == null || !dtos.Any()) { return; }

            List<RteFieldsHelper> dataFromDb = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                // Only want to pull RTE for an object when:
                //   - the object exists (Id > 0)
                //   - at least one of the RTE fields has not been inserted into, or retrieved already
                List<int> dtoIds = dtos.Where(x => x.Id > 0 && !x.WasDescriptionSet && x.Updateable != UpdateType.Deleted).Select(x => x.Id).ToList();

                if (dtoIds.Any())
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

                        // get the basic BOE data from the sprocResults
                        dataFromDb = (from t in gbe.TravelTripTaskElements
                                      where dtoIds.Contains(t.TravelTripTaskElementID)
                                      select new RteFieldsHelper
                                      {
                                          Id = t.TravelTripTaskElementID,
                                          Description = t.TravelTaskDescription
                                      }).ToList();
                    }

                    dataFromDb.AsParallel().ForAll(dbData =>
                    {
                        TravelDTO dto = dtos.First(b => b.Id == dbData.Id);

                        // We do not want to overwrite existing data during, so we only fill missing data, if available
                        dto.Description = dto.WasDescriptionSet ? dto.Description : dbData.Description;
                    });
                }
            }
        }

        #endregion

        #endregion

        #region Commits

        /// <summary>
        /// Save all the travels and their travel trips
        /// </summary>
        /// <param name="inTravels">Collection of Travel elements to save.</param>
        virtual public Dictionary<int, int> SaveTravels(ICollection<TravelDTO> inTravels)
        {
            if (inTravels == null)
            {
                throw new ArgumentNullException(nameof(inTravels));
            }

            // Make sure that RTE data is loaded, that way we do not wipe it out..
            this.LoadRTEFields(inTravels);

            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            // travel should be the first items saved on an update, LAST item saved on a delete
            foreach (TravelDTO travel in inTravels)
            {
                if (travel.Updateable == UpdateType.None)
                {
                    throw new ArgumentException("please supply the Updateable argument");
                }

                int travelId = travel.Id;

                // save travel first on update
                if (travel.Updateable == UpdateType.Upsert)
                {
                    travelId = this.UpsertTravel(travel);
                    this.travelTripTaskElementCustomFieldLoader.SaveTravelTripTaskElementCustomFieldValueContainers(travel.CustomFieldValueContainers, travelId);
                    toReturn.Add(travel.Id, travelId);
                }

                // check to see which travel trips need to be saved
                foreach (MSTTravelTripType MSTTravelTrip in travel.MSTTravelTrips)
                {
                    if (MSTTravelTrip.Updateable == UpdateType.Deleted)
                    {
                        this.DeleteMSTTravelTrip(MSTTravelTrip);

                    }
                    else if (MSTTravelTrip.Updateable == UpdateType.Upsert)
                    {
                        int MSTTravelTripID = this.UpsertMSTTravelTrip(travelId, MSTTravelTrip);
                        this.travelTripCustomFieldLoader.SaveTravelTripCustomFieldValueContainers(MSTTravelTrip.CustomFieldValueContainers, MSTTravelTripID);
                    }

                }

                // save travel last if delete
                if (travel.Updateable == UpdateType.Deleted)
                {
                    this.DeleteTravel(travel);
                    toReturn.Add(travel.Id, travel.Id);
                }

            } // end foreach

            return toReturn;
        }

        /// <summary>
        /// Updates or inserts a travel element.
        /// </summary>
        /// <param name="inTravel">Travel element to save.</param>
        /// <returns>Travel Id after the save.</returns>
        protected int UpsertTravel(TravelDTO inTravel)
        {
            if (inTravel == null)
            {
                throw new ArgumentNullException(nameof(inTravel));
            }

            this.LoadRTEFields(new List<TravelDTO>() { inTravel });

            int TRAVEL_ID = 0;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var resultsLinq = gbe.upsertTravelTripTaskElement(inTravel.Id, inTravel.TaskID, inTravel.TaskTitle, inTravel.Description, inTravel.BoeID, inTravel.UpdateDate, inTravel.StartDate, inTravel.EndDate, inTravel.BOETaskElementOrder);

                TRAVEL_ID = Convert.ToInt32(resultsLinq.SingleOrDefault());
            }
            return TRAVEL_ID;
        }

        /// <summary>
        /// Delete the travel element.
        /// </summary>
        /// <param name="inTravel">The travel element.</param>
        protected void DeleteTravel(UpdateableDTO inTravel)
        {
            if (inTravel == null)
            {
                throw new ArgumentNullException(nameof(inTravel));
            }
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.deleteMSTTravelTripTaskelement(inTravel.Id, inTravel.UpdateDate);
            }
        }

        /// <summary>
        /// Inserts or Updates a travel trip.
        /// </summary>
        /// <param name="inMSTTravelID">Travel Id.</param>
        /// <param name="inMSTTravelTrip">Trip Dto.</param>
        /// <returns>Id of the Trip.</returns>
        protected int UpsertMSTTravelTrip(int inMSTTravelID, MSTTravelTripType inMSTTravelTrip)
        {
            if (inMSTTravelTrip == null)
            {
                throw new ArgumentNullException(inMSTTravelID.ToString());
            }
            int toReturn = 0;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var resultsLinq = gbe.upsertMSTTravelTrip
                (
                    inMSTTravelTrip.Id,
                    (int)inMSTTravelTrip.ModeID,
                    inMSTTravelID,
                    inMSTTravelTrip.UpdateDate,
                    inMSTTravelTrip.GroupID, 
                    (int)inMSTTravelTrip.Segment, 
                    inMSTTravelTrip.Purpose,
                    inMSTTravelTrip.PerfOrgID,
                    inMSTTravelTrip.TripDate,
                    inMSTTravelTrip.EstimateDate,
                    inMSTTravelTrip.NumOfPeople,
                    inMSTTravelTrip.NumOfDays,
                    inMSTTravelTrip.ZoneOriginID,
                    inMSTTravelTrip.ZoneDestCity,
                    inMSTTravelTrip.ZoneDestinationID,
                    inMSTTravelTrip.ZoneResourceID,
                    inMSTTravelTrip.NonZoneFrom,
                    inMSTTravelTrip.NonZoneTo,
                    inMSTTravelTrip.NonZoneAirfareEstimate,
                    inMSTTravelTrip.NonZonePerDiemDaily,
                    inMSTTravelTrip.NonZoneCarRentalTrans,
                    inMSTTravelTrip.NonZoneNumCars,
                    inMSTTravelTrip.NonZoneResourceID,
                    inMSTTravelTrip.ClinId,
                    inMSTTravelTrip.WbsId
                );
                toReturn = Convert.ToInt32(resultsLinq.FirstOrDefault());
            } // end gbe
            return toReturn;
        }

        /// <summary>
        /// Deletes a MSTTravel trip.
        /// </summary>
        /// <param name="inDeleteMSTTravelTrip">MSTTravel trip to delete.</param>
        protected void DeleteMSTTravelTrip(UpdateableDTO inDeleteMSTTravelTrip)
        {
            if (inDeleteMSTTravelTrip == null)
            {
                throw new ArgumentNullException(nameof(inDeleteMSTTravelTrip));
            }

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.deleteMSTTravelTrip(inDeleteMSTTravelTrip.Id, inDeleteMSTTravelTrip.UpdateDate);
            }
        }

        /// <summary>
        /// Deletes All MST Travel Tasks and Trips for a BOE 
        /// </summary>
        /// <param name="boeId">BOE Id</param>
        public void DeleteAllTravelTripTaskElements(int boeId)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbm = new GenBoeEntities())
                {
                    gbm.deleteAllMSTTravelTripTaskElements(boeId);
                }
            }
        }
        #endregion
    }
}