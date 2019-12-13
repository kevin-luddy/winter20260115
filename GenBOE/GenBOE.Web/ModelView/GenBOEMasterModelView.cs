namespace GenBOE.Web.ModelView
{
    public class GenBOEMasterModelView
    {
        public GenBOEMasterModelView()
        {
            ProposalName = "Unable to get Proposal name";
            WorkspaceState = "Unable to get Workspace Status";
            HeaderFooter = "Lockheed Martin Proprietary Information";
        }

        public string ProposalName { get; set; }
        public string WorkspaceState { get; set; }
        public string HeaderFooter { get; set; }
    }
}
