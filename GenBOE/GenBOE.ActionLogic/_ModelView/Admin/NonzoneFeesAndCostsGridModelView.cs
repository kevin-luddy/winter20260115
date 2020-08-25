// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Admin
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class NonzoneFeesAndCostsGridModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NonzoneFeesAndCostsGridModelView()
        {
            this.NonzoneFeesAndCostsCollection = new Collection<NonzoneFeesAndCostsModelView>();
        }

        /// <summary>
        /// Collection of Nonzone Fees and Costs ModelViews for the Manage Fees and Costs for Nonzone Travel Page
        /// </summary>
        public ICollection<NonzoneFeesAndCostsModelView> NonzoneFeesAndCostsCollection { get; set; }
    }
}
