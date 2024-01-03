// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2017 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Linq;
    using IES.Core.Exceptions;
    using IES.Models;

    /// <summary>
    /// The Model for a row in the Rate Configuration.
    /// </summary>
    public class RateConfigurationModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RateConfigurationModelView"/> class.
        /// </summary>
        public RateConfigurationModelView()
        {
            using (IESEntities context = new IESEntities())
            {
                Revision revision = context.Revisions.Where(r => r.DatePublished == null).First();
                if (revision != null)
                {
                    this.MinYear = revision.StartYear;
                    this.MaxYear = revision.EndYear;
                }
                else
                {
                    throw new GeneralAppException("Work in Progress Revision was not found.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum year.
        /// </summary>
        public int MinYear { get; set; }

        /// <summary>
        /// Gets or sets the maximum year.
        /// </summary>
        public int MaxYear { get; set; }
    }
}
