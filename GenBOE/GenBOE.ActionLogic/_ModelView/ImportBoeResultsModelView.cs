using System.Collections.ObjectModel;

namespace GenBOE.ActionLogic.ModelView
{
    public class ImportBoeResultsModelView
    {
        public ImportBoeResultsModelView()
        {
            this.AuthorIDs = new Collection<int>();
            this.SubcontractorAuthorIDs = new Collection<int>();
            this.ApproverIDs = new Collection<int>();
        }

        public int BoeID { get; set; }
        public string BOETitle { get; set; }
        public string WbsString { get; set; }
        public int? WbsID { get; set; }
        public string ClinString { get; set; }
        public int? ClinID { get; set; }
        public Collection<int> AuthorIDs { get; set; }
        public Collection<int> SubcontractorAuthorIDs { get; set; }
        public Collection<int> ApproverIDs { get; set; }
        public int Status { get; set; }
        public int? BoeXrefID { get; set; }
        public bool isMaterial { get; set; }
        public bool IsMultiClinWbs { get; set; }
                
        // This needs to stay an "int" and not a "ImportResult" in order to translate correctly during model binding.
        public int ImportType { get; set; }
    }
}
