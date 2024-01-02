// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using IES.Standard;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class TravelTripTaskElementCustomFieldValueXREFLoader : DataLoader<CustomFieldValueContainer>, ITravelTripTaskElementCustomFieldValueXREFLoader
    {
        /// <summary>
        /// Default constructor
        /// </summary>
         public TravelTripTaskElementCustomFieldValueXREFLoader(ILogger logger) : base(logger)
		{
		}
         /// <summary>
         /// Get CustomfieldValueContainers by their ids.
         /// </summary>
         /// <param name="ids">id of the BOE Travel Element Custom Field Value ID to retrieve</param>
         /// <returns>Collection of custom fields values in use by travel element</returns>
         override public ICollection<CustomFieldValueContainer> GetByIds(ICollection<int> ids)
         {
             Collection<CustomFieldValueContainer> containers = new Collection<CustomFieldValueContainer>();

             using (GenBoeEntities gbe = new GenBoeEntities())
             {
                 containers = (from data in gbe.TravelTripTaskElementCustomFieldValueXREFs
                               where ids.Contains(data.TTECFVID)
                               select new CustomFieldValueContainer
                               {
                                   OwnerID = data.TravelTripTaskElementID,
                                   ContainerID = data.TTECFVID,
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
         /// This funciton will handle an individual BOE Travel Element Custom Field Value delete
         /// </summary>
         /// <param name="dtoToDelete">the selected Travel Element to delete</param>
         protected override int? Delete(CustomFieldValueContainer dtoToDelete)
         {
             int? toReturn = null;
             using (StopwatchTimer sw = new StopwatchTimer(this.Log))
             {
                 if (dtoToDelete == null)
                 {
                     throw new ArgumentNullException(nameof(dtoToDelete));
                 }

                 using (GenBoeEntities gbe = new GenBoeEntities())
                 {
                    toReturn = gbe.deleteTravelTripTaskElementCustomFieldValue(
                            dtoToDelete.ContainerID,
                            dtoToDelete.OwnerID,
                            dtoToDelete.CustomFieldValueID,
                            dtoToDelete.UpdateDate,
                            dtoToDelete.IsOpenEnded);
                 }
             }

             return toReturn;
         }

         /// <summary>
         /// Save/Create the BOE Travel Element Custom Field Value
         /// </summary>
         /// <param name="dtoToUpsert">the element to save</param>
         /// <returns>the Travel element details</returns>
         override protected int? Upsert(CustomFieldValueContainer dtoToUpsert)
         {
             using (StopwatchTimer sw = new StopwatchTimer(this.Log))
             {
                 if (dtoToUpsert == null)
                 {
                     throw new ArgumentNullException(nameof(dtoToUpsert));
                 }

                 int containerID = 0;

                 using (GenBoeEntities gbe = new GenBoeEntities())
                 {
                    int? sprocResults =
                        gbe.upsertTravelTripTaskElementCustomFieldValue(
                            dtoToUpsert.ContainerID,
                            dtoToUpsert.OwnerID,
                            dtoToUpsert.CustomFieldID,
                            dtoToUpsert.CustomFieldValueID,
                            dtoToUpsert.OpenEndedValue,
                            dtoToUpsert.UpdateDate,
                            dtoToUpsert.IsOpenEnded).FirstOrDefault();

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
         ///Save a collection of BOE Travel Element Custom Field Value Container
        /// </summary>
        /// <param name="inBOETravelElementCustomFieldValueContainers">Collection of custom field values to save </param>
        /// <param name="inBoeTravelElementID">travel element id</param>
         virtual public void SaveTravelTripTaskElementCustomFieldValueContainers(Collection<CustomFieldValueContainer> inBOETravelElementCustomFieldValueContainers, int inBoeTravelElementID)
         {
             using (StopwatchTimer sw = new StopwatchTimer(this.Log))
             {
                 if (inBOETravelElementCustomFieldValueContainers == null)
                 {
                     throw new ArgumentNullException(nameof(inBOETravelElementCustomFieldValueContainers));
                 }

                 foreach (CustomFieldValueContainer TravelTripTaskElementCustomFieldValueXREF in inBOETravelElementCustomFieldValueContainers)
                 {
                     this.SaveTravelTripTaskElementCustomFieldValueContainer(TravelTripTaskElementCustomFieldValueXREF, inBoeTravelElementID);
                 }
             }
         }

         /// <summary>
         /// Save a single BOE Travel Element Custom Field Value Container
         /// </summary>
         /// <param name="inCustomFieldValueContainer">custom field value to save</param>
         /// <param name="inBoeTravelElementID">travel element id</param>
         virtual public void SaveTravelTripTaskElementCustomFieldValueContainer(CustomFieldValueContainer inCustomFieldValueContainer, int inBoeTravelElementID)
         {
             using (StopwatchTimer sw = new StopwatchTimer(this.Log))
             {
                 if (inCustomFieldValueContainer == null)
                 {
                     throw new ArgumentNullException(nameof(inCustomFieldValueContainer));
                 }

                 inCustomFieldValueContainer.OwnerID = inBoeTravelElementID;

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
