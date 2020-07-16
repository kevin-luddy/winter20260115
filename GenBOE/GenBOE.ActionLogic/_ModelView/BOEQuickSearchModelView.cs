// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.Web.Mvc;
using System.Collections.ObjectModel;
using IES.Common;
using System.ComponentModel.DataAnnotations;

namespace GenBOE.ActionLogic.ModelView
{
    public class BOEQuickSearchModelView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "BOEs")]
        public BOEQuickSearchModelView()
        {
            this.Categories = new Collection<SelectListItem>() {
                new SelectListItem { Text = "Select Category", Value="" },
                new SelectListItem { Text = "BOEs in other Workspaces", Value = ((int)SearchCategory.BOEsInOtherWorkspaces).ToString() },
                new SelectListItem { Text = "BOE Content Templates", Value = ((int)SearchCategory.BOEContentTemplates).ToString() },
                new SelectListItem { Text = "BOEs in this Workspace", Value = ((int)SearchCategory.BOEsInThisWorkspace).ToString() },
                new SelectListItem { Text = "All", Value = ((int)SearchCategory.All).ToString() }
            };
        }

        public Collection<SelectListItem> Categories { get; set; }

        [Required(ErrorMessage="A category must be selected.")]
        public SearchCategory SelectedCategory { get; set; }

        [Required(ErrorMessage="Text to search for is required.")]
        [StringLength(200, ErrorMessage = "Quick Search must not exceed 200 chars.")]
        public string QuickSearchText { get; set; }

        public int WorkspaceID { get; set; }

        /// <summary>
        /// When true, the search was initiated from the "Copy from Boe" button, otherwise, the search was initiated from
        /// "Copy MOQ from BOE" in the MOQ Hours Equation dropdown.
        /// </summary>
        public bool IsCopyFromBoeContext { get; set; }
    }
}