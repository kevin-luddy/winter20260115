// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// View model for the full contents of the labor tab page
    /// </summary>
    public class LaborTabDataModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LaborTabDataModelView()
        {
            this.LaborTypesData = new Collection<LaborTypeDataModelView>();
        }

        #region Inputs only

        public string MOQEquation { get; set; }

        public decimal? MOQEquationTotal { get; set; }

        public DateTime? TaskStartDate { get; set; }

        public DateTime? TaskEndDate { get; set; }

        #endregion

        public ICollection<LaborTypeDataModelView> LaborTypesData { get; set; }

    }
}
