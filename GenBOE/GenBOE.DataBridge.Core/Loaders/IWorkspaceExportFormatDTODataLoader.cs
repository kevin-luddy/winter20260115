// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Loaders
{
	using System.Collections.ObjectModel;
	using GenBOE.DataBridge.Core;
	using IES.Common.Core.Loaders;

	public interface IWorkspaceExportFormatDTODataLoader : IDataLoader<WorkspaceExportFormatDTO>
	{
		Collection<WorkspaceExportFormatDTO> GetAllWorkspaceExportFormatIds();
		Collection<WorkspaceExportFormatDTO> GetWorkspaceExportFormatsForWorkspace(int inWorkspaceId);
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

		/// <summary>
		/// Get the WS Export Format's Name 
		/// </summary>
		/// <param name="templateID">The template ID of the workspace export format.</param>
		/// <returns>The name of the WS Export Format</returns>
		string GetNameById(int templateID);
	}
}
