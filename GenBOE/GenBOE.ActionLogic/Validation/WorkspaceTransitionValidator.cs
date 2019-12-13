using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenBOE.DTO;
using System.Collections.ObjectModel;
using GenBOE.Common;

namespace GenBOE.Business.Validation
{
    public class WorkspaceTransitionValidator : Validator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private WorkspaceStateMachine _WorkspaceStateMachine = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private WorkspaceDTOMapper _WorkspaceDTOMapper = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inWorkspaceMapper">Workspace Mapper</param>
        public WorkspaceTransitionValidator(WorkspaceStateMachine inWorkspaceStateMachine, WorkspaceDTOMapper inWorkspaceDTOMapper)
        {
            _WorkspaceStateMachine = inWorkspaceStateMachine;
            _WorkspaceDTOMapper = inWorkspaceDTOMapper;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "WorkspaceID"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "toState")]
        public override Collection<String> validation(object value, Dictionary<String, String> data)
        {
            Collection<String> response = new Collection<string>();
            WorkspaceState toState;

            if (value == null) {throw new ArgumentNullException("value");} else{
                toState = (WorkspaceState)Enum.Parse(typeof(WorkspaceState), Convert.ToString(value));
            }
                       
            if (data == null) throw new ArgumentNullException("data");

            if(data["WorkspaceID"] == null)
            {
              throw new InvalidOperationException("Expected WorkspaceID");
            }

            WorkspaceDTO thisWorkspace = _WorkspaceDTOMapper.GetWorkspaceById(Convert.ToInt32(data["WorkspaceID"]));

            response = _WorkspaceStateMachine.PerformStateTransitionValidation(thisWorkspace.WorkspaceID, thisWorkspace.WorkspaceState, toState);

            return response;
        }
    }

}
