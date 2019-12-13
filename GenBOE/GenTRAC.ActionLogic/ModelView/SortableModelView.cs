// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using GenTRAC.ActionLogic.GeneralHelper;

    /// <summary>
    /// Model View to allow sorting
    /// </summary>
    public class SortableModelView<T>
    {
        /// <summary>
        /// Gets or sets the sort field
        /// </summary>
        public string SortField { get; set; }

        /// <summary>
        /// Gets or sets the order
        /// </summary>
        public SortOrder Order { get; set; }

        /// <summary>
        /// Data which will be sortable
        /// </summary>
        public ICollection<T> DataRows { get; set; }

        /// <summary>
        /// Sort the Data
        /// </summary>
        public void Sort()
        {
            SortHelper<T> sorter = new SortHelper<T>();
            this.DataRows = sorter.Sort(this.DataRows, this.SortField, this.Order);
        }
    }
}
