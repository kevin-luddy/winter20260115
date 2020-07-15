// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System.Collections.Generic;
    using ActionLogic.ModelView.Clin;
    using GenBOE.ActionLogic.ModelView;

    public class ManageWBSGridModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManageWBSGridModelView"/> class.
        /// </summary>
        public ManageWBSGridModelView()
        {
            WbsResults = new List<ManageWBSModelView>();
            AvailableClins = new List<ManageCLINModelView>();
        }

        /// <summary>
        /// Gets or sets the available clins.
        /// </summary>
        public ICollection<ManageCLINModelView> AvailableClins { get; set; }

        /// <summary>
        /// Gets or sets the WBS results.
        /// </summary>
        public ICollection<ManageWBSModelView> WbsResults { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the workspace [contains oci].
        /// </summary>
        public bool ContainsOCI { get; set; }
    }
}