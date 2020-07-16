// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace IES.DataBridge.ModelViews
{
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// The Model for a grid in the Burden Pool Grid/Table.
    /// </summary>
    public class BurdenPoolGridModelView  
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BurdenPoolGridModelView"/> class.
        /// </summary>
        public BurdenPoolGridModelView()
        {
            this.BurdenElements = new List<BurdenElementModelView>();
            this.RateCodes = new List<OptionModelView>();
            this.LockInfo = new LockModelView();
        }

        /// <summary>
        /// Gets or sets the selected revision Id.
        /// </summary>
        public int? SelectedRevisionId { get; set; }

        /// <summary>
        /// version collection
        /// </summary>
        public ICollection<RevisionOptionModelView> Versions { get; set; }

        /// <summary>
        /// Revision
        /// </summary>
        public RevisionModelView Revision { get; set; }

        /// <summary>
        /// Gets or sets the Burden Pools.
        /// </summary>
        public ICollection<BurdenPoolDetailModelView> BurdenPools { get; set; }

        /// <summary>
        /// Gets or sets the Burden Elements to be mapped.
        /// </summary>
        public IList<BurdenElementModelView> BurdenElements { get; set; }

        /// <summary>
        /// Gets or sets the rate codes to use in dropdowns on front-end.
        /// </summary>
        public ICollection<OptionModelView> RateCodes { get; set; }

        /// <summary>
        /// Gets or sets lock information
        /// </summary>
        public LockModelView LockInfo { get; set; }
    }
}
