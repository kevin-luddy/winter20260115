// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;

    public interface IWorkspaceSearchDTODataLoader
    {
        Collection<int> GetWorkspaceSearchResults(WorkspaceSearchDTO inWorkspaceSearch);
    }
}
