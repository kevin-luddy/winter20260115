using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Web.ModelView
{
    [ExcludeFromCodeCoverage]
    public class ChooseWorkspaceModelView
    {
        public ChooseWorkspaceModelView()
        {
            WorkspaceShortname = "-1";
            WorkspaceName = null;
            WorkspaceId = 0;
        }

        public string WorkspaceShortname { get; set; }
        public string WorkspaceName { get; set; }
        public int WorkspaceId { get; set; }
    }
}
