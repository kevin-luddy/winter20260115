// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Checklist
{
    /// <summary>
    /// Checklist Index model view
    /// </summary>
    public class ChecklistIndexModelView : PersistedDataModelView
    {
        /// <summary>
        /// Checklist version
        /// </summary>
        public int? Version { get; set; }

        /// <summary>
        /// Flag for whether checklist content should be displayed (will be false for migrated completed proposals)
        /// </summary>
        public bool ShouldDisplayChecklist { get; set; }

        /// <summary>
        /// Flag for whether current user is Pricer for this checklist
        /// </summary>
        public bool IsPricer { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ChecklistIndexModelView()
        {
            this.Version = null;
        }
    }
}
