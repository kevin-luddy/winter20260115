// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.ModelView.Admin;

    public class ManageOffloadRatesGridModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManageOffloadRatesGridModelView"/> class.
        /// </summary>
        public ManageOffloadRatesGridModelView()
        {
            OffloadRateModelViews = new Collection<OffloadRateModelView>();
        }

        /// <summary>
        /// Gets or sets the offload rate model views.
        /// </summary>
        public Collection<OffloadRateModelView> OffloadRateModelViews { get; set; }

    }
}