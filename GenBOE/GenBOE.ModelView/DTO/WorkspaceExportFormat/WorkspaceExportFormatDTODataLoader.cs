// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.Dtos;
	using GenBOE.Models;
	using IES.Common;

	public class WorkspaceExportFormatDTODataLoader : DataLoader<WorkspaceExportFormatDTO>, IWorkspaceExportFormatDTODataLoader
	{
		private Logger _log = new Logger(typeof(WorkspaceExportFormatDTODataLoader));

		/// <summary>
		/// Constructor
		/// </summary>
		public WorkspaceExportFormatDTODataLoader()
		{
		}

		/// <summary>
		/// Get all the export format Ids for a given workspace.  Typically this list will be 'portrait', 
		/// 'landscape' and any other formats that have been selected by the WS Admin
		/// </summary>
		/// <param name="inWorkspaceId">the workspace id of interest</param>
		/// <returns>the Ids of the export formats for this workspace</returns>
		[DbQuery]
		public virtual Collection<WorkspaceExportFormatNameDTO> GetWorkspaceExportFormatNamesForWorkspace(int inWorkspaceId)
		{
			Collection<WorkspaceExportFormatNameDTO> toReturn = new Collection<WorkspaceExportFormatNameDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{    
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					// Get templates for the given workspace as well as those available to all workspaces
					toReturn = (from t in gbe.OutputFormatTemplates
								join s in gbe.OutputFormatTemplateWorkspaceXREFs
								on t.TemplateID equals s.TemplateID into templateJoin
								from u in templateJoin.DefaultIfEmpty()
								where u.WorkspaceID == inWorkspaceId || t.IsAvailableToAllWorkspaces
								select new WorkspaceExportFormatNameDTO
								{
									ExportFormat = new ExcelReportTemplate
									{
										TemplateId = t.TemplateID,
										ParentTemplateId = t.ParentTemplateID
									},
									Id = t.TemplateID,
									UpdateDate = t.UpdateDT,
									ExportFormatName = t.Template,
									ExportFormatDescription = t.TemplateDescription,
									IsActive = t.IsActive,
									IsAvailableToAllWorkspaces = t.IsAvailableToAllWorkspaces
								}).Distinct().ToCollection<WorkspaceExportFormatNameDTO>();
				}

				/*
				 * As a safeguard:  Post-process the list to enforce "data correctness".
				 * 
				 * Note: It is possible we might be able to remove this if/when we can guarantee that these conditions have stopped occurring
				 * and that all prior bad data has been cleaned up.
				 * 
				 */
				foreach (WorkspaceExportFormatNameDTO exportFormat in toReturn)
				{
					if (exportFormat.ExportFormat.TemplateId == exportFormat.ExportFormat.ParentTemplateId)
					{
						exportFormat.ExportFormat.ParentTemplateId = null;  // an entry should never reference itself
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// All the export formats the system knows about (i.e. 'Landscape', 'Portrait', + ALL others for ALL workspaces)
		/// </summary>
		/// <returns>the Ids of the export formats known to the system</returns>
		[DbQuery]
		public virtual Collection<WorkspaceExportFormatNameDTO> GetAllWorkspaceExportFormatIds()
		{
			Collection<WorkspaceExportFormatNameDTO> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn = (from t in gbe.OutputFormatTemplates
								select new WorkspaceExportFormatNameDTO
								{
									ExportFormat = new ExcelReportTemplate
									{
										TemplateId = t.TemplateID,
										ParentTemplateId = t.ParentTemplateID
									},
									Id = t.TemplateID,
									ExportFormatName = t.Template,
									ExportFormatDescription = t.TemplateDescription,
									UpdateDate = t.UpdateDT,
									IsActive = t.IsActive,
									IsAvailableToAllWorkspaces = t.IsAvailableToAllWorkspaces
								}).ToCollection<WorkspaceExportFormatNameDTO>();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Get the WS Export Format Name 
		/// </summary>
		/// <param name="templateID">The template ID of the workspace export format.</param>
		/// <returns>The WS Export Format Name</returns>
		[DbQuery]
		public WorkspaceExportFormatNameDTO GetNameById(int templateID)
		{
			WorkspaceExportFormatNameDTO exportName;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					exportName = (from t in gbe.OutputFormatTemplates
								  where t.TemplateID == templateID
								  select new WorkspaceExportFormatNameDTO
								  {
									  ExportFormat = new ExcelReportTemplate
									  {
										  TemplateId = t.TemplateID,
										  ParentTemplateId = t.ParentTemplateID
									  },
									  Id = t.TemplateID,
									  UpdateDate = t.UpdateDT,
									  ExportFormatName = t.Template,
									  ExportFormatDescription = t.TemplateDescription,
									  IsActive = t.IsActive,
									  IsAvailableToAllWorkspaces = t.IsAvailableToAllWorkspaces
								  }).FirstOrDefault();
				}
			}

			return exportName;
		}

		/// <summary>
		/// Get the export format DTO for a given DTO Id
		/// </summary>
		/// <param name="inExportFormatId">The Id of the export format</param>
		/// <returns>The DTO of interest that correponds to the Id</returns>
		[DbQuery]
		public override ICollection<WorkspaceExportFormatDTO> GetByIds(ICollection<int> ids)
		{
			ICollection<WorkspaceExportFormatDTO> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn = (from t in gbe.OutputFormatTemplates
								where ids.Contains(t.TemplateID)
								select new WorkspaceExportFormatDTO
								{
									ExportFormat = new ExcelReportTemplate
									{
										TemplateId = t.TemplateID,
										ParentTemplateId = t.ParentTemplateID
									},
									Id = t.TemplateID,
									ExportFormatName = t.Template,
									ExportFormatDescription = t.TemplateDescription,
									FileData = t.TemplateFile,
									IsActive = t.IsActive,
									UpdateDate = t.UpdateDT,
									IsAvailableToAllWorkspaces = t.IsAvailableToAllWorkspaces
								}).ToCollection<WorkspaceExportFormatDTO>();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Return the workspace ids that can be assigned for a given export format id
		/// NOTE: Automatically EXCLUDE workspaces in the closed or complete states
		/// </summary>
		/// <param name="inExportFormatId">The export format id to filter workspaces on</param>
		/// <returns>the workspace ids that can be assigned for a given export format id</returns>
		[DbQuery(2)]
		public virtual Collection<int> GetAvailableWorkspaceIdsForExportFormatId(int inExportFormatId)
		{
			Collection<int> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					IEnumerable<int> assignedWorkspacesForFormatId = (from s in gbe.OutputFormatTemplateWorkspaceXREFs
																	  where s.TemplateID == inExportFormatId
																	  select s.WorkspaceID);
					toReturn = (from s in gbe.Workspaces
																	 where s.WorkspaceStateID != (int)WorkspaceState.Closed && s.WorkspaceStateID != (int)WorkspaceState.Complete
																	 select s.WorkspaceID).Except(assignedWorkspacesForFormatId).ToCollection();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Return the workspace ids that are already assigned for a given export format id
		/// NOTE: Automatically EXCLUDE workspaces in the closed or complete states
		/// </summary>
		/// <param name="inExportFormatId">The export format id to filter workspaces on</param>
		/// <returns>the workspace ids that are already assigned for a given export format id</returns>
		[DbQuery(2)]
		public virtual Collection<int> GetAssignedWorkspaceIdsForExportTemplateId(int inExportFormatId)
		{
			Collection<int> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					IEnumerable<int> assignedWorkspacesForFormatId = (from s in gbe.OutputFormatTemplateWorkspaceXREFs
																	  where s.TemplateID == inExportFormatId
																	  select s.WorkspaceID);
					IEnumerable<int> allWorkspacesForChosenOnes = (from s in gbe.Workspaces
																   where s.WorkspaceStateID != (int)WorkspaceState.Closed && s.WorkspaceStateID != (int)WorkspaceState.Complete
																   select s.WorkspaceID).Intersect(assignedWorkspacesForFormatId);

					toReturn = new Collection<int>(allWorkspacesForChosenOnes.ToArray());
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Return the workspace ids that are have selected a template for use.
		/// </summary>
		/// <param name="inExportFormatId">The export format id to filter workspaces on</param>
		/// <returns>the workspace ids that are already assigned for a given export format id</returns>
		[DbQuery]
		public virtual Collection<int> GetWorkspaceIdsByExportTemplateId(int inExportFormatId)
		{
			Collection<int> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					IEnumerable<int> workspaceIds = (from s in gbe.Workspaces
													 where s.TemplateID == inExportFormatId
													 select s.WorkspaceID);

					toReturn = new Collection<int>(workspaceIds.ToArray());
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Save an export format into the system.  This is NOT associated to a workspace, but is just the DTO information.
		/// </summary>
		/// <param name="dtoToUpsert">The DTO to save</param>
		/// <returns>The Id of the new export format</returns>
		protected override int? Upsert(WorkspaceExportFormatDTO dtoToUpsert)
		{
			if (dtoToUpsert == null)
			{
				throw new ArgumentNullException(nameof(dtoToUpsert));
			}

			int? toReturn = 0;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				int? sprocResult = gbe.upsertOutputFormatTemplate(
					dtoToUpsert.ExportFormat.TemplateId,
					dtoToUpsert.ExportFormatName,
					dtoToUpsert.ExportFormatDescription,
					dtoToUpsert.FileData,
					dtoToUpsert.UpdateDate,
					dtoToUpsert.ExportFormat.ParentTemplateId,
					dtoToUpsert.IsAvailableToAllWorkspaces).FirstOrDefault();

				toReturn = sprocResult ?? 0;
			}

			return toReturn;
		}

		/// <summary>
		/// Remove the association of a collection of workspaceIds to a templateId.  This is 
		/// typically displayed in the picklist where a WS Admin can associate 
		/// Workspaces to an Export Template.
		/// </summary>
		/// <param name="inWorkspaceIds">The workspaces to remove the associate for </param>
		/// <param name="inTemplateIdForWorkspaces">The template whose associatiation is being removed</param>
		public virtual void DeleteWorkspaceExportFormatPicklist(Collection<int> inWorkspaceIds, int inTemplateIdForWorkspaces)
		{
			if (inWorkspaceIds == null)
			{
				throw new ArgumentNullException(nameof(inWorkspaceIds));
			}

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					foreach (int inWorkspaceId in inWorkspaceIds)
					{
						gbe.deleteOutputFormatTemplateWorkspace(inWorkspaceId, inTemplateIdForWorkspaces);
					}
				}
			}
		}

		/// <summary>
		/// Deletes a WorkspaceExportFormat dto
		/// </summary>
		/// <param name="dtoToDelete">Export Format template to delete.</param>
		/// <returns>Id of the deleted item.</returns>
		protected override int? Delete(WorkspaceExportFormatDTO dtoToDelete)
		{
			if (dtoToDelete == null)
			{
				throw new ArgumentNullException(nameof(dtoToDelete));
			}

			int? toReturn = 0;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				int? sprocResult = gbe.archiveOutputFormatTemplate(
					dtoToDelete.ExportFormat.TemplateId,
					dtoToDelete.UpdateDate).FirstOrDefault();
				toReturn = sprocResult.HasValue ? sprocResult.Value : 0;
			}

			return toReturn;
		}

		/// <summary>
		/// Deletes the XREF for the template in all Workspaces
		/// </summary>
		/// <param name="templateId">Template to delete</param>
		public void DeleteTemplateForAllWorkspaces(int templateId)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.deleteOutputFormatTemplateForAllWorkspaces(templateId);
				}
			}
		}

		/// <summary>
		/// Associate a collection of workspaceIds to a templateId.  This is 
		/// typically displayed in the picklist where a WS Admin can associate 
		/// Workspaces to an Export Template.
		/// </summary>
		/// <param name="inWorkspaceIds">The workspaces to associate the template to</param>
		/// <param name="inTemplateIdForWorkspaces">The template being associated to the workspaces</param>
		public virtual void InsertWorkspaceExportFormatPicklist(Collection<int> inWorkspaceIds, int inTemplateIdForWorkspaces)
		{
			if (inWorkspaceIds == null)
			{
				throw new ArgumentNullException(nameof(inWorkspaceIds));
			}

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					foreach (int inWorkspaceId in inWorkspaceIds)
					{

						gbe.insertOutputFormatTemplateWorkspace(inWorkspaceId, inTemplateIdForWorkspaces);
					}
				}
			}
		}

		/// <summary>
		/// Associate a collection of templateIds to a workspace id.  This is 
		/// typically displayed in the picklist where a WS Admin can associate 
		/// Workspaces to an Export Template.
		/// </summary>
		/// <param name="inTemplatesForWorkspaces">The templates being associated to the workspaces</param>
		/// <param name="inWorkspaceId">The workspace to associate the templates to</param>
		public virtual void InsertWorkspaceExportFormatsPickList(Collection<WorkspaceExportFormatNameDTO> inTemplatesForWorkspaces, int inWorkspaceId)
		{
			if (inTemplatesForWorkspaces == null)
			{
				throw new ArgumentNullException(nameof(inTemplatesForWorkspaces));
			}

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					foreach (WorkspaceExportFormatNameDTO inTemplate in inTemplatesForWorkspaces)
					{
						gbe.insertOutputFormatTemplateWorkspace(inWorkspaceId, inTemplate.Id);
					}
				}
			}
		}

		/// <summary>
		/// Restores the given archived template to active status
		/// </summary>
		/// <param name="templateDto">Template to restore</param>
		public void RestoreTemplate(WorkspaceExportFormatDTO templateDto)
		{
			if (templateDto == null)
			{
				throw new ArgumentNullException(nameof(templateDto));
			}

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.restoreOutputFormatTemplate(templateDto.ExportFormat.TemplateId, templateDto.UpdateDate);
			}
		}
	}
}
