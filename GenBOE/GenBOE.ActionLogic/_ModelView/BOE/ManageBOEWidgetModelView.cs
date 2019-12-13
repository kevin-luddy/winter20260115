using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Mvc;
using GenBOE.ActionLogic.ModelView.Clin;
using IES.Common;

namespace GenBOE.ActionLogic.ModelView.BOE
{
    public class ManageBOEGridWidgetModelView
    {
        public ManageBOEGridWidgetModelView()
        {
            this.PotentialApprovers = new Collection<SelectListItem>();
            this.PotentialAuthors = new Collection<SelectListItem>();
            this.PotentialSubcontractorAuthors = new Collection<SelectListItem>();
            this.PotentialClins = new Collection<ManageCLINModelView>();
            this.PotentialWbs = new Collection<SelectListItem>();
            this.BoeResults = new Collection<ManageBOEModelView>();

            this.ManageBoeHeaderInfo = string.Empty;
            this.WorkspaceState = WorkspaceState.None;
        }

        public Collection<SelectListItem> PotentialWbs { get; set; }
        public Collection<ManageCLINModelView> PotentialClins { get; set; }
        public Collection<SelectListItem> PotentialAuthors { get; set; }
        public Collection<SelectListItem> PotentialSubcontractorAuthors { get; set; }
        public Collection<SelectListItem> PotentialApprovers { get; set; }
        public string DefaultStartDate { get; set; }
        public string DefaultEndDate { get; set; }

        public bool AllowBOEStateChanges { get; set; }

        public WorkspaceState WorkspaceState { get; set; }

        public ICollection<ManageBOEModelView> BoeResults { get; set; }

        /// <summary>
        /// Company specific header information for display on the manage Boe page.
        /// </summary>
        public string ManageBoeHeaderInfo { get; set; }

        /// <summary>
        /// Gets or sets the multi WBS identifier.
        /// </summary>
        public int MultiWBSId { get; set; }

        /// <summary>
        /// Gets or sets the name of the multi WBS.
        /// </summary>
        public string MultiWBSName { get; set; }
        
        /// <summary>
        /// the ID and name of the MultiBOE Clin
        /// </summary>
        public ManageCLINModelView MultiClin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [contains oci].
        /// </summary>
        public bool ContainsOCI { get; set; }
    }
}
