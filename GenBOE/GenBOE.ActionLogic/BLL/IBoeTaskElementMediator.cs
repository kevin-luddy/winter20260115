// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BLL
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;

    public interface IBoeTaskElementMediator
    {
        Dictionary<int, int> MediatedSaveTaskElements(Collection<BoeTaskElementDTO> inBOETaskElementCollection, FullWorkspace fullWorkspace);
                
        /// <summary>
        /// Bulk Saves boe task elements. This method should be used as opposed to MediatedSaveTaskElements even
        /// if only one task element is being saved. The bulk save will also save all resources, spreads, task variable, ordinary variables
        /// and custom fields in one database call each in addition to the task elements themselves.
        /// </summary>
        /// <param name="inBOETaskElementCollectionToSave">task elements to save</param>
        /// <param name="inWorkspace">Full workspace</param>
        /// <returns>Dictionary where key = old task element id and value = new task element id</returns>
        IDictionary<int, int> MediatedBulkSaveTaskElements(
            ICollection<BoeTaskElementDTO> inBOETaskElementCollectionToSave,
            FullWorkspace inWorkspace); 
    }
}
