// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;

    public interface IFindReplaceDTODataLoader
    {
        Collection<FindReplaceDTO> getFindReferences(FindReplaceDTO inFindParams, int inWorkspaceId);
    }
}
