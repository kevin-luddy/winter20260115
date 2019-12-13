// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using IES.Common;
using GenBOE.Objects;
using System.Collections.ObjectModel;

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// A collection of TaskElementDuplicateFormModelView for a particular task type in a BOE
    /// </summary>
    public class TaskElementDuplicateFormCollection
    {
        public FullBoe Boe { get; set; }

        public TaskType TaskType { get; set; }

        /// <summary>
        /// Collection of task element IDs and the number of duplicates that should be created for each
        /// </summary>
        public Collection<TaskElementDuplicateFormModelView> DuplicateTaskRequests { get; set; }
    }
}
