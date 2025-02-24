// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace IES.DataBridge.ModelViews
{
    using System.Collections.Generic;

    /// <summary>
    /// The Model for a grid in the Revision Grid/Table.
    /// </summary>
    public class RevisionGridModelView  
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RevisionGridModelView"/> class.
        /// </summary>
        public RevisionGridModelView()
        {
            this.Revisions = new List<RevisionModelView>();
        }

        /// <summary>
        /// Gets or sets the Revisions.
        /// </summary>
        public ICollection<RevisionModelView> Revisions { get; set; }

        /// <summary>
        /// Gets or sets the flag whether the user is RDM Admin.
        /// </summary>
        public bool IsRdmAdminUser { get; set; }

        /// <summary>
        /// Gets or sets the flag whether the user is RDM Cobra Admin.
        /// </summary>
        public bool IsRdmCobraAdminUser { get; set; }
    }
}
