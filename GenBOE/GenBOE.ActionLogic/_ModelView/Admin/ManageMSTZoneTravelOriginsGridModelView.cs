// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Admin
{
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using GenBOE.Dtos;

    public class ManageMSTZoneTravelOriginsGridModelView : PagedResultsModelView<int>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ManageMSTZoneTravelOriginsGridModelView()
        {
            this.CurrentPage = 1;
            this.PagedIndexes = new Collection<int>();
            this.ResultsPerPage = 100;
            this.MSTZoneTravelOriginsCollection = new Collection<MSTZoneTravelOriginModelView>();
        }

        /// <summary>
        /// Collection of Destination Model Views for the Manage Zone Travel Origins Page
        /// </summary>
        public ICollection<MSTZoneTravelOriginModelView> MSTZoneTravelOriginsCollection { get; set; }
    }
}