// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView
{
    using System;

    /// <summary>
    /// Model view for the persisted data
    /// </summary>
    public abstract class PersistedDataModelView
    {
        /// <summary>
        /// Initializes a new instance of the PersistedDataModelView class
        /// </summary>
        protected PersistedDataModelView()
        {
            this.UpdateDate = DateTime.MinValue;
            this.IsReadOnly = "true";
        }

        /// <summary>
        /// Gets or sets the update date
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Gets or sets a long version of the update date
        /// </summary>
        public string UpdateDateLong
        {
            get
            {
                return this.UpdateDate.Ticks.ToString();
            }

            set
            {
                this.UpdateDate = new DateTime(long.Parse(value));
            }
        }

        /// <summary>
        /// Flag for read only
        /// </summary>
        public string IsReadOnly { get; set; }
    }
}
