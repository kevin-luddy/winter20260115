using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
{
    /// <summary>
    /// This class will return the different states for a workspace
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class WorkspaceStateModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WorkspaceStateModelView()
        {
            WorkspaceStateID = 0;
            WorkspaceState = String.Empty;
        }

        /// <summary>
        /// The ID of the workspace state
        /// </summary>
        public int WorkspaceStateID { get; set; }

        /// <summary>
        /// the name of the workspace state
        /// </summary>
        public string WorkspaceState { get; set; }
    }
}
