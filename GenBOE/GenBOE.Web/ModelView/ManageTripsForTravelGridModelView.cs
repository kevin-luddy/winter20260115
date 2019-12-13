using System.Collections.ObjectModel;
using System.Collections.Generic;
using GenBOE.ActionLogic.ModelView;

namespace GenBOE.Web.ModelView
{
    public class ManageTripsForTravelGridModelView : PagedResultsModelView<int>
    {
        public ManageTripsForTravelGridModelView()
            : base()
        {
            CurrentPage = 1;
            PagedIndexes = new Collection<int>();
            ResultsPerPage = 100;
            TripsForTravelCollection = new Collection<TripForTravelModelView>();
        }

        public ICollection<TripForTravelModelView> TripsForTravelCollection { get; set; }
    }
}