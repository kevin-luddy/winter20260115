// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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
    using IES.Common;
    using IES.Common.classes;

    public class TravelTripCustomFieldValueXREFLoader : DataLoader<CustomFieldValueContainer>, ITravelTripCustomFieldValueXREFLoader
    {
        private Logger _log = new Logger(typeof(TravelTripCustomFieldValueXREFLoader));

        /// <summary>
        /// Default constructor
        /// </summary>
        public TravelTripCustomFieldValueXREFLoader()
        {
        }
        /// <summary>
        /// Get CustomfieldValueContainers by their ids.
        /// </summary>
        /// <param name="ids">id of the BOE Travel trip Custom Field Value ID to retrieve</param>
        /// <returns>Collection of custom fields for a trip</returns>
        override public ICollection<CustomFieldValueContainer> GetByIds(ICollection<int> ids)
        {
            Collection<CustomFieldValueContainer> containers = new Collection<CustomFieldValueContainer>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                containers = (from data in gbe.TravelTripCustomFieldValueXREFs
                              where ids.Contains(data.TCFVID)
                              select new CustomFieldValueContainer
                              {
                                  OwnerID = data.TravelTripID,
                                  ContainerID = data.TCFVID,
                                  CustomFieldValueID = data.CustomFieldValueID,
                                  UpdateDate = data.UpdateDT,
                                  CustomFieldID = data.CustomFieldValue.CustomFieldID,
                                  IsOpenEnded = data.CustomFieldValue.CustomField.IsOpenEnded,
                                  OpenEndedValue = data.CustomFieldValue.CustomFieldValueDescription
                              }).ToCollection();
            }

            return containers;
        }
       
        /// <summary>
        /// Returns CustomFieldID for a given CustomFieldValueID for use in validating customFieldValueContainers
        /// </summary>
        /// <param name="CustomFieldValueID"></param>
        /// <returns>CustomFieldID</returns>
        public int GetCustomFieldID(int CustomFieldValueID)
        {
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                return gbe.CustomFieldValues.Where(x => x.CustomFieldValueID == CustomFieldValueID).Select(x => x.CustomFieldID).FirstOrDefault();
            }
        }

        /// <summary>
        /// This funciton will handle an individual BOE Travel trips Custom Field Value delete
        /// </summary>
        /// <param name="dtoToDelete">the selected Travel trip to delete</param>
        protected override int? Delete(CustomFieldValueContainer dtoToDelete)
        {
            int? toReturn = null;
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                if (dtoToDelete == null)
                {
                    throw new ArgumentNullException(nameof(dtoToDelete));
                }

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    switch (SystemConfiguration.Instance().CompanyMode)
                    {
                        // OwnerID is a generic property that holds the id of the owner of this custom field value and in this case it's the BOETravelElementID
                        case CompanyConfiguration.MST:
                            toReturn = gbe.deleteMSTTravelTripCustomFieldValue(
                                dtoToDelete.ContainerID,
                                dtoToDelete.OwnerID,
                                dtoToDelete.CustomFieldValueID,
                                dtoToDelete.UpdateDate,
                                dtoToDelete.IsOpenEnded);
                            break;
                        default:
                            toReturn = gbe.deleteTravelTripCustomFieldValue(
                                dtoToDelete.ContainerID,
                                dtoToDelete.OwnerID,
                                dtoToDelete.CustomFieldValueID,
                                dtoToDelete.UpdateDate);
                            break;
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Save/Create the BOE Travel trip Custom Field Value
        /// </summary>
        /// <param name="dtoToUpsert">the trip to save</param>
        /// <returns>the Travel trip details</returns>
        override protected int? Upsert(CustomFieldValueContainer dtoToUpsert)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                if (dtoToUpsert == null)
                {
                    throw new ArgumentNullException(nameof(dtoToUpsert));
                }

                int containerID = 0;

                int? sprocResults = null;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {

                    switch (SystemConfiguration.Instance().CompanyMode)
                    {
                        case CompanyConfiguration.MST:
                            sprocResults = gbe.upsertMSTTravelTripCustomFieldValue(
                                    dtoToUpsert.ContainerID,
                                    dtoToUpsert.OwnerID,
                                    dtoToUpsert.CustomFieldID,
                                    dtoToUpsert.CustomFieldValueID,
                                    dtoToUpsert.OpenEndedValue,
                                    dtoToUpsert.UpdateDate,
                                    dtoToUpsert.IsOpenEnded).FirstOrDefault();
                            break;
                        default:
                            sprocResults = gbe.upsertTravelTripCustomFieldValue(
                                 dtoToUpsert.ContainerID,
                                 dtoToUpsert.OwnerID,
                                 dtoToUpsert.CustomFieldValueID,
                                 dtoToUpsert.UpdateDate).FirstOrDefault();
                            break;
                    }
                    
                    containerID = sprocResults.HasValue ? sprocResults.Value : 0;

                    // Set the new ID on the DTO for later use, if necessary
                    if (dtoToUpsert.Id < 0)
                    {
                        dtoToUpsert.Id = containerID;
                    }
                } // end using gbe

                return containerID;
            }
        }


        /// <summary>
        /// Saves collection of trip custom fields.
        /// </summary>
        /// <param name="inBOETravelTripCustomFieldValueContainers">collection of custom field values</param>
        /// <param name="inBoeTravelTripID">travel element id</param>
        virtual public void SaveTravelTripCustomFieldValueContainers(Collection<CustomFieldValueContainer> inBOETravelTripCustomFieldValueContainers, int inBoeTravelTripID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                if (inBOETravelTripCustomFieldValueContainers == null)
                {
                    throw new ArgumentNullException(nameof(inBOETravelTripCustomFieldValueContainers));
                }

                foreach (CustomFieldValueContainer TravelTripCustomFieldValueXREF in inBOETravelTripCustomFieldValueContainers)
                {
                    this.SaveTravelTripFieldValueContainer(TravelTripCustomFieldValueXREF, inBoeTravelTripID);
                }
            }
        }

        /// <summary>
        /// Save a single BOE Travel Trip Custom Field Value Container
        /// </summary>
        /// <param name="inCustomFieldValueContainer"></param>
        virtual public void SaveTravelTripFieldValueContainer(CustomFieldValueContainer inCustomFieldValueContainer, int inBoeTravelTripID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                if (inCustomFieldValueContainer == null)
                {
                    throw new ArgumentNullException(nameof(inCustomFieldValueContainer));
                }

                inCustomFieldValueContainer.OwnerID = inBoeTravelTripID;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {

                    if (inCustomFieldValueContainer.Updateable == UpdateType.Deleted)
                    {
                        this.Delete(inCustomFieldValueContainer);
                    }
                    else if (inCustomFieldValueContainer.Updateable == UpdateType.Upsert)
                    {
                        this.Upsert(inCustomFieldValueContainer);
                    }

                }
            }
        }

    }
}