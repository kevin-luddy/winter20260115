// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// A Model View class for a PPR&amp;D Document
    /// </summary>
    /// <seealso cref="IES.DataBridge.ModelViews.DocumentModelView" />
    public class PPRDModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PPRDModelView"/> class.
        /// </summary>
        public PPRDModelView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PPRDModelView"/> class.
        /// </summary>
        /// <param name="revision">Revision</param>
        public PPRDModelView(RevisionModelView revision)
        {
            this.Revision = revision;
         }

         /// <summary>
        /// Gets or sets the PPR&amp;D Revision Id.
        /// </summary>
        public RevisionModelView Revision { get; set; }

        /// <summary>
        /// Top-level section nodes
        /// </summary>
        public ICollection<SectionModelView> ChildNodes { get; set; }

        /// <summary>
        /// Gets or sets lock information
        /// </summary>
        public LockModelView LockInfo { get; set; }

        /// <summary>
        /// Gets or sets the section content type to use in dropdowns on front-end. 
        /// </summary>
        public ICollection<OptionModelView> SectionContentTypeOptions { get; set; }
    }
}