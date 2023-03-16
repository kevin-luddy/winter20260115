// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;

    /// <summary>
    /// MOQ Table Data
    /// </summary>
    public class MoqTableDataModelView
    {
        /// <summary>
        /// Gets or sets the WBS Element
        /// </summary>
        public string WbsElement { get; set; }

        /// <summary>
        /// Gets or sets the Query Type
        /// </summary>
        public string QueryType { get; set; }

        /// <summary>
        /// Gets or sets the POP Start
        /// </summary>
        public DateTime? PoPStart { get; set; }

        /// <summary>
        /// Gets or sets the POP End
        /// </summary>
        public DateTime? PoPEnd { get; set; }

		/// <summary>
		/// Gets or sets the POP Start using Fiscal Weeks in yyyyWW format
		/// </summary>
		public string PoPStartFW { get; set; }

		/// <summary>
		/// Gets or sets the POP End using Fiscal Weeks in yyyyWW format
		/// </summary>
		public string PoPEndFW { get; set; }

		/// <summary>
		/// Gets or sets the Query Filters
		/// </summary>
		public string Filters { get; set; }

        /// <summary>
        /// Gets or sets the Table Id
        /// </summary>
        public int TableId { get; set; }
    }
}