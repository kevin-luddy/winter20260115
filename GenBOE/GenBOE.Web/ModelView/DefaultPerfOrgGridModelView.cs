using System.Collections.ObjectModel;
using GenBOE.ActionLogic.ModelView;

namespace GenBOE.Web.ModelView
{
    public class DefaultPerfOrgGridModelView : PagedResultsModelView<int> 
    {
        public DefaultPerfOrgGridModelView()
        {
            CurrentPage = 1;
            PagedIndexes = new Collection<int>();
            PerfOrgResults = new Collection<DefaultPerfOrgMV>();
            ResultsPerPage = 200;
        }

        public int TotalResults { get; set; }
        public Collection<DefaultPerfOrgMV> PerfOrgResults { get; set; }
    }   
}
