// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Admin
{
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    public class ManageMSTZoneTravelDestinationsGridModelView : PagedResultsModelView<int>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ManageMSTZoneTravelDestinationsGridModelView()
        {
            this.CurrentPage = 1;
            this.PagedIndexes = new Collection<int>();
            this.ResultsPerPage = 100;
            this.MSTZoneTravelDestinationsCollection = new Collection<MSTZoneTravelDestinationModelView>();
        }

        /// <summary>
        /// Collection of Destination Model Views for the Manage Zone Travel Destinations Page
        /// </summary>
        public ICollection<MSTZoneTravelDestinationModelView> MSTZoneTravelDestinationsCollection { get; set; }
    }
}