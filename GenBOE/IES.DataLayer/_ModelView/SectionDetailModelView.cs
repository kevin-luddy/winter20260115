// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Collections.Generic;

    /// <summary>
    /// Section Details for RDSB
    /// </summary>
    public class SectionDetailModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SectionDetailModelView"/> class.
        /// </summary>
        public SectionDetailModelView()
        {
            this.IsRdsbRequired = false;
            this.ChildNodes = new List<SectionDetailModelView>();
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the reference number.
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has table.
        /// </summary>
        public bool HasTable { get; set; }

        /// <summary>
        /// Gets or sets whether the Section is required for RDSB
        /// </summary>
        public bool IsRdsbRequired { get; set; }

        /// <summary>
        /// Gets or sets the child nodes.
        /// </summary>
        public ICollection<SectionDetailModelView> ChildNodes { get; set; }
    }
}
