using System.Collections.ObjectModel;
using GenBOE.ActionLogic.ModelView;

namespace GenBOE.Web.ModelView
{
    public class BOECustomFieldPerformingOrgGridModelView : PagedResultsModelView<int>
    {
        public BOECustomFieldPerformingOrgGridModelView()
        {
            CurrentPage = 1;
            PagedIndexes = new Collection<int>();
            PerformingOrgResults = new Collection<BOECustomFieldOptionModelView>();
            ResultsPerPage = 200;
        }

        public int TotalResults { get; set; }
        public Collection<BOECustomFieldOptionModelView> PerformingOrgResults { get; set; }
    }
}