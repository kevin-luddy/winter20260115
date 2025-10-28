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
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    public class TravelDTODataLoader : ITravelDTODataLoader
    {
        private Logger _log = new Logger(typeof(TravelDTODataLoader));
        private ITravelTripTaskElementCustomFieldValueXREFLoader travelTripTaskElementCustomFieldLoader = null;
        private ITravelTripCustomFieldValueXREFLoader travelTripCustomFieldLoader = null;

        /// <summary>
        /// Loads travel element information
        /// </summary>
        /// <param name="travelTripTaskElementCustomFieldLoader">task element custom field loader</param>
        /// <param name="travelTripCustomFieldLoader">trip element custom field loader</param>
        public TravelDTODataLoader(ITravelTripTaskElementCustomFieldValueXREFLoader travelTripTaskElementCustomFieldLoader, ITravelTripCustomFieldValueXREFLoader travelTripCustomFieldLoader)
        {
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
        public virtual TravelDTO GetById(int inTravelID, bool includeRTEFields = false)
        {
            return this.GetByIds(new Collection<int> { inTravelID }).FirstOrDefault();
        }

        /// <summary>
        /// Gets Travel data by Ids
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        [DbQuery]
        public virtual ICollection<TravelDTO> GetByIds(ICollection<int> ids, bool includeRTEFields = false)
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
                                   CustomFieldValueContainersIEnum = gbe.TravelTripTaskElementCustomFieldValueXREFs.Where(cf => cf.TravelTripTaskElementID == t.TravelTripTaskElementID)
                                                .Select(cf => new CustomFieldValueContainer
                                                {
                                                    ContainerID = cf.TTECFVID,
                                                    CustomFieldValueID = cf.CustomFieldValueID,
                                                    UpdateDate = cf.UpdateDT
                                                }),
                                   TravelTripsIEnum = gbe.TravelTrips.Where(tt => tt.TravelTripTaskElementID == t.TravelTripTaskElementID)
                                                .Select(tt => new TravelTripType
                                                {
                                                    Id = tt.TravelTripID,
                                                    TravelTripID = tt.TravelTripID,
                                                    GroupID = tt.GroupID,
                                                    Segment = (SegmentType)tt.SegmentID,
                                                    TripDate = tt.TripDate,
                                                    SystemTripID = tt.TripID,
                                                    NumOfDays = tt.NumDays,
                                                    NumOfPeople = tt.NumPeople,
                                                    NumOfTrips = tt.NumTrips,
                                                    Purpose = tt.Purpose,
                                                    BoeID = t.BOEID,
                                                    UpdateDate = tt.UpdateDT,
                                                    PerfOrgID = tt.PerformingOrganizationID,
                                                    LockedDate = tt.TripLockedDT,
                                                    CustomFieldValueContainersIEnum = gbe.TravelTripCustomFieldValueXREFs.Where(cf => cf.TravelTripID == tt.TravelTripID)
                                                                 .Select(cf => new CustomFieldValueContainer
                                                                 {
                                                                     ContainerID = cf.TCFVID,
                                                                     CustomFieldValueID = cf.CustomFieldValueID,
                                                                     UpdateDate = cf.UpdateDT
                                                                 })
                                                })
                               }).ToList();
                }

                travels.ForEach(
                        x =>
                        {
                            x.CustomFieldValueContainers = x.CustomFieldValueContainersIEnum.ToCollection(); x.CustomFieldValueContainersIEnum = null;
                            x.TravelTrips = x.TravelTripsIEnum.ToCollection();
                            x.TravelTrips.ToList().ForEach(tt =>
                            {
                                tt.CustomFieldValueContainers = tt.CustomFieldValueContainersIEnum.ToCollection(); tt.CustomFieldValueContainersIEnum = null;
                            });
                            x.TravelTripsIEnum = null;
                            x.StartDate = x.StartDate.Normalize();
                            x.EndDate = x.EndDate.Normalize();
                        }
                    );
            }

            return travels;
        }

        /// <summary>
        /// Gets Travel data by Workspace Id
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        [DbQuery]
        public virtual ICollection<TravelDTO> GetByWorkspaceId(int wsId, bool includeRTEFields = false)
        {
            List<TravelDTO> travels = new List<TravelDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    travels = (from t in gbe.TravelTripTaskElements
                               join b in gbe.BOEs on t.BOEID equals b.BOEID
                               where b.WorkspaceID == wsId
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
                                   CustomFieldValueContainersIEnum = gbe.TravelTripTaskElementCustomFieldValueXREFs.Where(cf => cf.TravelTripTaskElementID == t.TravelTripTaskElementID)
                                                .Select(cf => new CustomFieldValueContainer
                                                {
                                                    ContainerID = cf.TTECFVID,
                                                    CustomFieldValueID = cf.CustomFieldValueID,
                                                    UpdateDate = cf.UpdateDT
                                                }),
                                   TravelTripsIEnum = gbe.TravelTrips.Where(tt => tt.TravelTripTaskElementID == t.TravelTripTaskElementID)
                                                .Select(tt => new TravelTripType
                                                {
                                                    Id = tt.TravelTripID,
                                                    TravelTripID = tt.TravelTripID,
                                                    GroupID = tt.GroupID,
                                                    Segment = (SegmentType)tt.SegmentID,
                                                    TripDate = tt.TripDate,
                                                    SystemTripID = tt.TripID,
                                                    NumOfDays = tt.NumDays,
                                                    NumOfPeople = tt.NumPeople,
                                                    NumOfTrips = tt.NumTrips,
                                                    Purpose = tt.Purpose,
                                                    BoeID = t.BOEID,
                                                    UpdateDate = tt.UpdateDT,
                                                    PerfOrgID = tt.PerformingOrganizationID,
                                                    LockedDate = tt.TripLockedDT,
                                                    CustomFieldValueContainersIEnum = gbe.TravelTripCustomFieldValueXREFs.Where(cf => cf.TravelTripID == tt.TravelTripID)
                                                                 .Select(cf => new CustomFieldValueContainer
                                                                 {
                                                                     ContainerID = cf.TCFVID,
                                                                     CustomFieldValueID = cf.CustomFieldValueID,
                                                                     UpdateDate = cf.UpdateDT
                                                                 })
                                                })
                               }).OrderBy(x => x.BoeID).ToList();
                }

                travels.ForEach(
                        x =>
                        {
                            x.CustomFieldValueContainers = x.CustomFieldValueContainersIEnum.ToCollection(); x.CustomFieldValueContainersIEnum = null;
                            x.TravelTrips = x.TravelTripsIEnum.ToCollection();
                            x.TravelTrips.ToList().ForEach(tt =>
                            {
                                tt.CustomFieldValueContainers = tt.CustomFieldValueContainersIEnum.ToCollection(); tt.CustomFieldValueContainersIEnum = null;
                            });
                            x.TravelTripsIEnum = null;
                            x.StartDate = x.StartDate.Normalize();
                            x.EndDate = x.EndDate.Normalize();
                        }
                    );
            }

            return travels;
        }

        /// <summary>
        /// Gets Travel data by Boe Ids
        /// </summary>
        /// <param name="boeIds">Boe Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        [DbQuery]
        public virtual ICollection<TravelDTO> GetByBoeIds(ICollection<int> boeIds, bool includeRTEFields = false)
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
                                   CustomFieldValueContainersIEnum = gbe.TravelTripTaskElementCustomFieldValueXREFs.Where(cf => cf.TravelTripTaskElementID == t.TravelTripTaskElementID)
                                                .Select(cf => new CustomFieldValueContainer
                                                {
                                                    ContainerID = cf.TTECFVID,
                                                    CustomFieldValueID = cf.CustomFieldValueID,
                                                    UpdateDate = cf.UpdateDT
                                                }),
                                   TravelTripsIEnum = gbe.TravelTrips.Where(tt => tt.TravelTripTaskElementID == t.TravelTripTaskElementID)
                                                .Select(tt => new TravelTripType
                                                {
                                                    Id = tt.TravelTripID,
                                                    TravelTripID = tt.TravelTripID,
                                                    GroupID = tt.GroupID,
                                                    Segment = (SegmentType)tt.SegmentID,
                                                    TripDate = tt.TripDate,
                                                    SystemTripID = tt.TripID,
                                                    NumOfDays = tt.NumDays,
                                                    NumOfPeople = tt.NumPeople,
                                                    NumOfTrips = tt.NumTrips,
                                                    Purpose = tt.Purpose,
                                                    BoeID = t.BOEID,
                                                    UpdateDate = tt.UpdateDT,
                                                    PerfOrgID = tt.PerformingOrganizationID,
                                                    LockedDate = tt.TripLockedDT,
                                                    CustomFieldValueContainersIEnum = gbe.TravelTripCustomFieldValueXREFs.Where(cf => cf.TravelTripID == tt.TravelTripID)
                                                                 .Select(cf => new CustomFieldValueContainer
                                                                 {
                                                                     ContainerID = cf.TCFVID,
                                                                     CustomFieldValueID = cf.CustomFieldValueID,
                                                                     UpdateDate = cf.UpdateDT
                                                                 })
                                                })
                               }).ToList();
                }

                travels.ForEach(
                        x =>
                        {
                            x.CustomFieldValueContainers = x.CustomFieldValueContainersIEnum.ToCollection(); x.CustomFieldValueContainersIEnum = null;
                            x.TravelTrips = x.TravelTripsIEnum.ToCollection();
                            x.TravelTrips.ToList().ForEach(tt =>
                            {
                                tt.CustomFieldValueContainers = tt.CustomFieldValueContainersIEnum.ToCollection(); tt.CustomFieldValueContainersIEnum = null;
                            });
                            x.TravelTripsIEnum = null;
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
        [DbQuery]
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

        #endregion Retrieves

        #region Commits

        /// <summary>
        /// Save all the travels and their travel trips
        /// </summary>
        /// <param name="inTravels">Collection of Travel elements to save.</param>
        public virtual Dictionary<int, int> SaveTravels(ICollection<TravelDTO> inTravels)
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
                foreach (TravelTripType travelTrip in travel.TravelTrips)
                {
                    if (travelTrip.Updateable == UpdateType.Deleted)
                    {
                        this.DeleteTravelTrip(travelTrip);

                    }
                    else if (travelTrip.Updateable == UpdateType.Upsert)
                    {
                        int travelTripID = this.UpsertTravelTrip(travelId, travelTrip);
                        this.travelTripCustomFieldLoader.SaveTravelTripCustomFieldValueContainers(travelTrip.CustomFieldValueContainers, travelTripID);
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
				System.Data.Entity.Core.Objects.ObjectResult<int?> resultsLinq = gbe.upsertTravelTripTaskElement(inTravel.Id, inTravel.TaskID, inTravel.TaskTitle, inTravel.Description, inTravel.BoeID, inTravel.UpdateDate, inTravel.StartDate, inTravel.EndDate, inTravel.BOETaskElementOrder);

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
                gbe.deleteTravelTripTaskElement(inTravel.Id, inTravel.UpdateDate);
            }
        }

        /// <summary>
        /// Inserts or Updates a travel trip.
        /// </summary>
        /// <param name="inTravelID">Travel Id.</param>
        /// <param name="inTravelTrip">Trip Dto.</param>
        /// <returns>Id of the Trip.</returns>
        protected int UpsertTravelTrip(int inTravelID, TravelTripType inTravelTrip)
        {
            if (inTravelTrip == null)
            {
                throw new ArgumentNullException(nameof(inTravelTrip));
            }

            int toReturn = 0;
            DateTime intervalDate = DateTime.MinValue;
            bool isFirstTrip = true;
            int newTravelTrip = -1;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                if (inTravelTrip.NumOfOccurences == 0)
                {
					System.Data.Entity.Core.Objects.ObjectResult<int?> resultsLinq = gbe.upsertTravelTrip(inTravelTrip.TravelTripID, inTravelTrip.GroupID, (int)inTravelTrip.Segment, inTravelTrip.PerfOrgID, inTravelTrip.SystemTripID, inTravelTrip.TripDate, inTravelTrip.NumOfTrips, inTravelTrip.NumOfPeople, inTravelTrip.NumOfDays, inTravelTrip.Purpose, inTravelID, inTravelTrip.UpdateDate);


                    toReturn = Convert.ToInt32(resultsLinq.FirstOrDefault());
                }
                else
                {
                    for (int x = 0; x < inTravelTrip.NumOfOccurences; x++)
                    {
                        // the first trip saves uses the inputted date
                        if (isFirstTrip)
                        {
							System.Data.Entity.Core.Objects.ObjectResult<int?> resultsLinq = gbe.upsertTravelTrip(inTravelTrip.TravelTripID, null, (int)inTravelTrip.Segment, inTravelTrip.PerfOrgID, inTravelTrip.SystemTripID, inTravelTrip.TripDate, inTravelTrip.NumOfTrips, inTravelTrip.NumOfPeople, inTravelTrip.NumOfDays, inTravelTrip.Purpose, inTravelID, inTravelTrip.UpdateDate);

                            toReturn = Convert.ToInt32(resultsLinq.FirstOrDefault());
                            intervalDate = inTravelTrip.TripDate;
                            isFirstTrip = false;
                        }
                        else
                        {
                            intervalDate = intervalDate.AddMonths(inTravelTrip.NumOfIntervals);
							System.Data.Entity.Core.Objects.ObjectResult<int?> resultsLinq = gbe.upsertTravelTrip(newTravelTrip, null, (int)inTravelTrip.Segment, inTravelTrip.PerfOrgID, inTravelTrip.SystemTripID, intervalDate, inTravelTrip.NumOfTrips, inTravelTrip.NumOfPeople, inTravelTrip.NumOfDays, string.Empty, inTravelID, inTravelTrip.UpdateDate);

                            toReturn = Convert.ToInt32(resultsLinq.FirstOrDefault());
                        }
                    }
                }
            } // end gbe

            return toReturn;
        }

        /// <summary>
        /// Deletes a travel trip.
        /// </summary>
        /// <param name="inDeleteTravelTrip">Travel trip to delete.</param>
        protected void DeleteTravelTrip(TravelTripType inDeleteTravelTrip)
        {
            if (inDeleteTravelTrip == null)
            {
                throw new ArgumentNullException(nameof(inDeleteTravelTrip));
            }

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.deleteTravelTrip(inDeleteTravelTrip.TravelTripID, inDeleteTravelTrip.UpdateDate);
            }
        }

        /// <summary>
        /// Deletes All Travel Tasks and Trips for a BOE 
        /// </summary>
        /// <param name="boeId">BOE Id</param>
        public void DeleteAllTravelTripTaskElements(int boeId)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbm = new GenBoeEntities())
                {
                    gbm.deleteAllTravelTripTaskElements(boeId);
                }
            }
        }

        #endregion Commits
    }
}