// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using System;
    using System.Linq;
    using System.Transactions;
    using GenBOE.DataBridge.DTO;
    using IES.Common;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class TravelControllerLogic : ITravelControllerLogic
    {
        ITravelDTODataLoader _travelDTODataLoader;
        private ICustomFieldValueDTODataLoader customFieldValueLoader;

        public TravelControllerLogic(
            ITravelDTODataLoader inTravelDataLoader,
            ICustomFieldValueDTODataLoader customFieldValueLoader
            )
        {
            this._travelDTODataLoader = inTravelDataLoader;
            this.customFieldValueLoader = customFieldValueLoader;
        }

        /// <summary>
        /// Gets a <see cref="Boolean"/> indicating if the segment help should be shown for IS&amp;GS
        /// </summary>
        /// <value>a <see cref="Boolean"/> indicating if the segment help should be shown</value>
        public virtual Boolean ShowSegmentHelpLink
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Saves the travel elements with a new order.
        /// </summary>
        /// <param name="boeObject">full boe object.</param>
        /// <param name="theModelView">Collection of user changes to the order.</param>
        public virtual void ReOrderTaskElementOrder( FullBoe boeObject, TaskElementOrderCollection theModelView)
        {
            if (boeObject == null) { throw new ArgumentNullException(nameof(boeObject)); }

            // preload for save..
            ICollection<TravelDTO> travels = this._travelDTODataLoader.GetByBoeIds(new Collection<int>() { boeObject.Id }, true);
            
            // Update the Order of the Travel task elements. The order is from the user. 
            travels.Select(w => { w.BOETaskElementOrder = (theModelView.BOETaskElements.First(t => t.TaskID == w.Id)).ListOrder; return w; }).ToList();

            // Update the updatetype 
            travels.Select(x => { x.Updateable = UpdateType.Upsert; return x; }).ToList();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this._travelDTODataLoader.SaveTravels(travels);
                scope.Complete();
            }
        }
        
        /// <summary>
        /// Creates duplicates of a collection of Travel Task elements.
        /// </summary>
        /// <param name="duplicateRequest">Collection of travel task IDs (key) and the number of times (value) they should be duplicated.</param>
        /// <param name="inSourceBOE">Full BOE containing the Travel Tasks.</param>
        public void DuplicateTravelTaskElements(Dictionary<int, int> duplicateRequest, FullBoe inSourceBOE)
        {
            if (duplicateRequest == null) { throw new ArgumentNullException(nameof(duplicateRequest)); }
            if (inSourceBOE == null) { throw new ArgumentNullException(nameof(inSourceBOE)); }

            foreach (var task in duplicateRequest)
            {
                // Make the specified number of duplicates for the task
                for (int i = 1; i <= task.Value; i++)
                {
                    this.DuplicateTravelTaskInBoe(inSourceBOE, task.Key, i);
                }
            }
        }

        /// <summary>
        /// Creates an exact duplicate of a Travel Task in the same BOE.
        /// </summary>
        /// <param name="inSourceBOE">Full BOE containing travel task</param>
        /// <param name="taskElementToCopy">ID of Travel task to be copied</param>
        /// <param name="duplicateNumber">The number of the copy. This will be appended to the title in the duplicate created.</param>
        /// <returns>Duplicate task TravelDTO</returns>
        private void DuplicateTravelTaskInBoe(FullBoe inSourceBOE, int taskElementToCopy, int duplicateNumber)
        {
            TravelDTO taskToCopy = inSourceBOE.Travels.FirstOrDefault(x => x.Id == taskElementToCopy);
            if (taskToCopy != null)
            {
                TravelDTO travelDuplicate = taskToCopy.DeepClone();

                int resCfId = -1;

                travelDuplicate.Id = -1;
                travelDuplicate.Updateable = UpdateType.Upsert;
                travelDuplicate.TaskID = string.Empty;
                travelDuplicate.TaskTitle = Utilities.appendCopyPrefixToTitle(travelDuplicate.TaskTitle, duplicateNumber);
                travelDuplicate.BOETaskElementOrder = 2000; //New tasks should be put at bottom of order

                // if there there are task level custom fields, copy them
                if (travelDuplicate.CustomFieldValueContainers.Any())
                {
                    travelDuplicate.CustomFieldValueContainers = getCustomFieldContainerCopy(travelDuplicate.CustomFieldValueContainers);
                }

                // Copy all trips within the task
                if (travelDuplicate.TravelTrips.Any())
                {
                    var travelTripsToSave = new Collection<TravelTripType>();

                    int NewTravelTripID = -1;
                    foreach (var travelTrip in travelDuplicate.TravelTrips)
                    {
                        travelTrip.TravelTripID = NewTravelTripID;
                        travelTrip.Updateable = UpdateType.Upsert;

                        // if there there are resource level custom fields, copy them
                        if (travelTrip.CustomFieldValueContainers.Any())
                        {
                            travelTrip.CustomFieldValueContainers = getCustomFieldContainerCopy(travelTrip.CustomFieldValueContainers, resCfId);
                            resCfId -= travelTrip.CustomFieldValueContainers.Count;
                        }

                        travelTripsToSave.Add(travelTrip);
                        NewTravelTripID--;
                    }
                    travelDuplicate.TravelTrips = travelTripsToSave;
                }

                // Copy all MSTtrips within the task
                if (travelDuplicate.MSTTravelTrips.Any())
                {
                    var mstTravelTripsToSave = new Collection<MSTTravelTripType>();
                    int NewTravelTripID = -1;
                    foreach (var mstTravelTrip in travelDuplicate.MSTTravelTrips)
                    {
                        mstTravelTrip.Id = NewTravelTripID;
                        mstTravelTrip.Updateable = UpdateType.Upsert;
                        // if there there are resource level custom fields, copy them
                        if (mstTravelTrip.CustomFieldValueContainers.Any())
                        {
                            mstTravelTrip.CustomFieldValueContainers = getCustomFieldContainerCopy(mstTravelTrip.CustomFieldValueContainers);
                            resCfId -= mstTravelTrip.CustomFieldValueContainers.Count;
                        }
                        mstTravelTripsToSave.Add(mstTravelTrip);
                        NewTravelTripID--;
                    }
                    travelDuplicate.MSTTravelTrips = mstTravelTripsToSave;
                }

                this._travelDTODataLoader.SaveTravels(new Collection<TravelDTO> { travelDuplicate });
            }
        }

        /// <summary>
        /// Returns a new collection of custom field value containers that is a copy ofthe given custom field containers
        /// </summary>
        /// <param name="containersToCopy">Collection of custom field value containers to be copied</param>
        /// <returns>Collection of custom field value containers</returns>
        private Collection<CustomFieldValueContainer> getCustomFieldContainerCopy(Collection<CustomFieldValueContainer> containersToCopy, int newContainerId = -1)
        {
            Collection<CustomFieldValueContainer> newCustomFieldContainers = new Collection<CustomFieldValueContainer>();

            foreach (CustomFieldValueContainer cfvContainer in containersToCopy)
            {
                cfvContainer.ContainerID = newContainerId--;
                cfvContainer.Updateable = UpdateType.Upsert;

                if(cfvContainer.IsOpenEnded)
                {
                    cfvContainer.CustomFieldValueID = CreateDuplicateOpenEndedCustomField(cfvContainer.CustomFieldValueID);
                }

                //cfvContainer.OwnerID --> Automatically set to new parent ID in SaveBOETaskElementCustomFieldValueContainer, so no need to update here
                newCustomFieldContainers.Add(cfvContainer);

            }
            return newCustomFieldContainers;
        }

        /// <summary>
        /// Creates a duplicate of the given open ended cf value
        /// </summary>
        /// <param name="customFieldValueId">Id of CFV to duplicate</param>
        /// <returns>Id of duplicate cfv</returns>
        private int CreateDuplicateOpenEndedCustomField(int customFieldValueId)
        {
            CustomFieldValueDTO openEnded = this.customFieldValueLoader.GetById(customFieldValueId);
            openEnded.Id = -1;
            openEnded.CustomFieldValueID = -1;
            openEnded.Updateable = UpdateType.Upsert;

            int? result = this.customFieldValueLoader.Save(openEnded);

            return result.Value;
        }
    }
}
