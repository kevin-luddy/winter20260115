// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System.Collections.Generic;

    /// <summary>
    /// A collection of LaborTypeOrders that come in from the UI. 
    /// </summary>
    public class LaborTypeOrderCollection
    {
        /// <summary>
        /// Collection of Labor Types that contain orders
        /// </summary>
        public ICollection<LaborTypeOrder> LaborTypes { get; set; }
    }
}
