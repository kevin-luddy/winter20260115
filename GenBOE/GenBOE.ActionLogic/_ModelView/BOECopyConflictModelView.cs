// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GenBOE.ActionLogic.ModelView
{
    public class BOECopyConflictModelView
    {
         public BOECopyConflictModelView()
         {
             this.taskTitle = string.Empty;
             this.workspaceVariableConflicts = new Dictionary<WorkspaceVariableModelView, decimal>();
             this.workspaceVariableCircularReferences = new Collection<WorkspaceVariableModelView>();
             this.ordinaryVariableCircularReferences = new Collection<BoeTaskOrdinaryVariableModelView>();
             this.performingOrgConflicts = new Collection<PerformingOrgModelView>();
             this.resourceConflicts = new Collection<ResourceModelView>();
             this.taskElementTypeConflict = new Collection<TaskElementDetailModelView>();
             this.taskIDConflict = string.Empty;
         }

        public string taskTitle { get; set; }
        public Dictionary<WorkspaceVariableModelView, decimal> workspaceVariableConflicts { get; set; }
        public Collection<WorkspaceVariableModelView> workspaceVariableCircularReferences { get; set; }
        public Collection<BoeTaskOrdinaryVariableModelView> ordinaryVariableCircularReferences { get; set; }
        public Collection<PerformingOrgModelView> performingOrgConflicts { get; set; }
        public Collection<ResourceModelView> resourceConflicts { get; set; }
        public Collection<TaskElementDetailModelView> taskElementTypeConflict { get; set; }
        public string taskIDConflict { get; set; }

    }
}
