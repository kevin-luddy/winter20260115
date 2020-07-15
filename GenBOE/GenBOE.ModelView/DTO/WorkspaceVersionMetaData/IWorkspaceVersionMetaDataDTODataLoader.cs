// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;

    public interface IWorkspaceVersionMetaDataDTODataLoader
    {
        void Delete(WorkspaceVersionMetaDataDTO inDeleteWorkspaceVersion, int inWorkspaceID);
        Collection<WorkspaceVersionMetaDataDTO> GetAllWorkspaceVersions();
        Collection<WorkspaceVersionMetaDataDTO> GetByIds(Collection<int> inVersionID);
        Collection<WorkspaceVersionMetaDataDTO> GetByWorkspaceID(int inWorkspaceID);
        void Save(Collection<WorkspaceVersionMetaDataDTO> inWorkspaceVersions, int inWorkspaceID);
        void Upsert(WorkspaceVersionMetaDataDTO inSaveWorkspaceVersion, int inWorkspaceID);
        string Restore(WorkspaceVersionMetaDataDTO inWorkspaceVersion, int inRestoredVersionBy);
    }
}
