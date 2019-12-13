using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.ObjectModel;

namespace GenBOE.Web.ModelView
{
    public class DesignToCostPagedResultsModelView : PagedResultsModelView<int>
    {
        public DesignToCostPagedResultsModelView()
        {
            CurrentPage = 1;
            PagedIndexes = new Collection<int>();
            DTCElements = new Collection<DesignToCostModelView>();
            ResultsPerPage = 100;
            SearchFilter="";
        }

        public Collection<DesignToCostModelView> DTCElements { get; set; }
        public String SearchFilter { get; set; }
        
    }
}