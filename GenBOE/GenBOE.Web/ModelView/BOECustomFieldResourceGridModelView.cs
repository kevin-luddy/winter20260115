using System.Collections.ObjectModel;
using GenBOE.ActionLogic.ModelView;

namespace GenBOE.Web.ModelView
{
    public class BOECustomFieldResourceGridModelView : PagedResultsModelView<int>
    {
        public BOECustomFieldResourceGridModelView()
        {
            CurrentPage = 1;
            PagedIndexes = new Collection<int>();
            ResourceResults = new Collection<BOECustomFieldResourceModelView>();
            ResultsPerPage = 200;
        }

        public int TotalResults { get; set; }
        public Collection<BOECustomFieldResourceModelView> ResourceResults { get; set; }
    }
}