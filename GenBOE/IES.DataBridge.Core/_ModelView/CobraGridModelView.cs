// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Common.Core;

    /// <summary>
    /// The Model View used for COBRA mapping detail Grid/Table.
    /// </summary>
    public class CobraGridModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CobraGridModelView"/> class.
        /// </summary>
        public CobraGridModelView()
        {
            this.Versions = new Collection<RevisionOptionModelView>();
            this.LockInfo = new LockModelView();
        }

        /// <summary>
        /// Gets or sets the selected revision Id
        /// </summary>
        public int? SelectedRevisionId { get; set; }

        /// <summary>
        /// Gets or sets the versions of rates.
        /// This does include Work in Progress.
        /// </summary>
        public ICollection<RevisionOptionModelView> Versions { get; set; }

        /// <summary>
        /// Gets or sets the COBRA details.
        /// </summary>
        public ICollection<CobraDetailModelView> CobraDetails { get; set; }

        /// <summary>
        /// Gets or sets the Cobra Codes to use in dropdowns on front-end for Code1.
        /// </summary>
        public ICollection<OptionModelView> CobraCodes { get; set; }

        /// <summary>
        /// Gets or sets lock information
        /// </summary>
        public LockModelView LockInfo { get; set; }
    }
}
