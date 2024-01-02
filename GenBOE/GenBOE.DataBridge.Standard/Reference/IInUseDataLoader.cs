// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Reference
{
    using System.Collections.Generic;

    public interface IInUseDataLoader
    {
        bool GetInUse(IES.Standard.InUseDataType inUseDataType, int inID, int? inParentID);
        bool GetWorkspaceResourceInUse(int inID, int inResourceListID);
        bool GetSystemResourceInUse(int inID);
        HashSet<int> GetWorkspaceResourceInUseByMultipleResources(ICollection<int> inMultipleResourceIDs, int inResourceListID);
        HashSet<int> GetWorkspaceResourceIDsInUseByListID(int inResourceListID);
        HashSet<int> GetSystemResourceIDsInUse();
        HashSet<int> GetWorkspacePerfOrgIDsInUseByPerfOrgListID(int inPerfOrgListID);
                     
    }
}
