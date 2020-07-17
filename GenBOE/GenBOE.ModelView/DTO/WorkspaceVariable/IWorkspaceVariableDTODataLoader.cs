// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;

    public interface IWorkspaceVariableDTODataLoader
    {
        WorkspaceVariableDTO GetById(int inWorkspaceVarID);
        Collection<int> GetBOEIDsUsingWorkspaceVarID(int inWorkspaceVarID);
        Dictionary<int, int> SaveWorkspaceVariables(Collection<WorkspaceVariableDTO> inWorkspaceVars);
        ICollection<WorkspaceVariableDTO> GetByWorkspaceID(int wsId);
        ICollection<WorkspaceVariableDTO> GetByIds(IReadOnlyCollection<int> ids);
    }
}
