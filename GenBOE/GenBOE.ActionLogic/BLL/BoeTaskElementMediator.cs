// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;

    public class BoeTaskElementMediator : IBoeTaskElementMediator
    {
        IBoeTaskElementDTODataLoader _TaskElementLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        public BoeTaskElementMediator(
            IBoeTaskElementDTODataLoader inTaskElementLoader)
        {
            this._TaskElementLoader = inTaskElementLoader;
        }

        public Dictionary<int, int> MediatedSaveTaskElements(Collection<BoeTaskElementDTO> inBOETaskElementCollection, FullWorkspace fullWorkspace)
        {
            if (fullWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fullWorkspace));
            }
            if (inBOETaskElementCollection == null)
            {
                throw new ArgumentNullException(nameof(inBOETaskElementCollection));
            }

            // load up RTE data if it was not loaded before, to prevent deletion of that data
            this._TaskElementLoader.LoadRTEFields(inBOETaskElementCollection);

            Dictionary<int, int> toReturn = this._TaskElementLoader.SaveBoeTaskElements(inBOETaskElementCollection);

            fullWorkspace.RefreshTaskElements();

            return toReturn;
        }
        

        /// <summary>
        /// Bulk Saves boe task elements. This method should be used as opposed to MediatedSaveTaskElements even
        /// if only one task element is being saved. The bulk save will also save all resources, spreads, task variable, ordinary variables
        /// and custom fields in one database call each in addition to the task elements themselves.
        /// </summary>
        /// <param name="inBOETaskElementCollectionToSave">Modified task elements to save</param>
        /// <param name="inWorkspace">workspace</param>
        /// <returns>Dictionary where key = old task element id and value = new task element id</returns>
        public IDictionary<int, int> MediatedBulkSaveTaskElements(ICollection<BoeTaskElementDTO> inBOETaskElementCollectionToSave, FullWorkspace inWorkspace)
        {
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }
            if (inBOETaskElementCollectionToSave == null)
            {
                throw new ArgumentNullException(nameof(inBOETaskElementCollectionToSave));
            }

            IDictionary<int, int> toReturn = new Dictionary<int, int>();

            if (inBOETaskElementCollectionToSave.Any(t => t.Updateable != UpdateType.None))
            {
                // load up RTE data if it was not loaded before, to prevent deletion of that data
                this._TaskElementLoader.LoadRTEFields(inBOETaskElementCollectionToSave);

                toReturn = this._TaskElementLoader.BulkSave(inBOETaskElementCollectionToSave);

                // Refresh the task elements. 
                inWorkspace.RefreshTaskElements();
            
                // after a bulk save, we need to remove any task elements that were deleted.
                // UpdateType.None will be a sideeffect of the save and IS correct
                inBOETaskElementCollectionToSave = (from te in inBOETaskElementCollectionToSave
                                                    where te.Updateable == UpdateType.None
                                                    select te).ToCollection();

                // also make sure to remove any deleted children in the remaining task elements
                foreach (BoeTaskElementDTO te in inBOETaskElementCollectionToSave)
                {
                    te.taskElementLabors = (from labor in te.taskElementLabors
                                            where labor.Updateable == UpdateType.None
                                            select labor).ToCollection();

                    te.OrdinaryVariables = (from variable in te.OrdinaryVariables
                                            where variable.Updateable == UpdateType.None
                                            select variable).ToCollection();
                }
            }

            return toReturn;
        }
    }
}
