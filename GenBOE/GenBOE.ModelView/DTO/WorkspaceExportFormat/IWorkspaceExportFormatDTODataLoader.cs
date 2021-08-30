// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common;

    public interface IWorkspaceExportFormatDTODataLoader : IDataLoader<WorkspaceExportFormatDTO>
    {
        System.Collections.ObjectModel.Collection<WorkspaceExportFormatDTO> GetAllWorkspaceExportFormatIds();
        System.Collections.ObjectModel.Collection<WorkspaceExportFormatDTO> GetWorkspaceExportFormatsForWorkspace(int inWorkspaceId);
        void InsertWorkspaceExportFormatPicklist(Collection<int> inWorkspaceIds, int inTemplateIdForWorkspaces);
        void DeleteWorkspaceExportFormatPicklist(Collection<int> inWorkspaceIds, int inTemplateIdForWorkspaces);
        Collection<int> GetAvailableWorkspaceIdsForExportFormatId(int inExportFormatId);
        Collection<int> GetAssignedWorkspaceIdsForExportTemplateId(int inExportFormatId);
        Collection<int> GetWorkspaceIdsByExportTemplateId(int inExportFormatId);

        /// <summary>
        /// Deletes the XREF for the template in all Workspaces
        /// </summary>
        /// <param name="templateId">Template to delete</param>
        void DeleteTemplateForAllWorkspaces(int templateId);

        /// <summary>
        /// Restores the given archived template to active status
        /// </summary>
        /// <param name="templateDto">Template to restore</param>
        void RestoreTemplate(WorkspaceExportFormatDTO templateDto);
    }
}
