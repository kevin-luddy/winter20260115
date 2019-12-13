using System;

namespace GenBOE.Web.ModelView
{
    public class WorkspaceToCopyBOEListModelView
    {
        public WorkspaceToCopyBOEListModelView()
        {
            BOEID = -1;
            BOETitle = string.Empty;
            WBS_NUM = String.Empty;
            WBS = String.Empty;
            CLIN_NUM = String.Empty;
            CLIN = String.Empty;
            Description = String.Empty;
        }

        public int BOEID { get; set; }
        public string BOETitle { get; set; }
        public string WBS_NUM { get; set; }
        public string WBS { get; set; }
        public string CLIN_NUM { get; set; }
        public string CLIN { get; set; }
        public string Description { get; set; }
    }
}