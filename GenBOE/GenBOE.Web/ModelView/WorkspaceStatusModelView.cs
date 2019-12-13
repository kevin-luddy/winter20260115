using System;
using IES.Common;
using GenBOE.ActionLogic.ModelView;
using GenBOE.Dtos;

namespace GenBOE.Web.ModelView
{
    public class WorkspaceStatusModelView : PersistedDataModelView
    {
        public WorkspaceStatusModelView()
            : base()
        {
        }

        public WorkspaceStatusModelView(WorkspaceDTO inWorkspaceDTO)
            : base()
        {
            if (inWorkspaceDTO == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceDTO));
            }

            WorkspaceStatus = inWorkspaceDTO.WorkspaceState;
            UpdateDate = inWorkspaceDTO.UpdateDate;
            WorkspaceName = inWorkspaceDTO.WorkspaceName;
          
        }

        public WorkspaceState WorkspaceStatus { get; set; }
        public string WorkspaceName { get; set; }
    }
}