// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Standard;
    using GenBOE.Dtos;
    using GenBOE.Models;
	using Microsoft.Extensions.Logging;

	public class ProPricerDTODataLoader : IProPricerDTODataLoader
    {
        private readonly ILogger _log;

        /// <summary>
        /// default ctor
        /// </summary>
        /// <param name="logger">Logger</param>
        public ProPricerDTODataLoader(ILogger<ProPricerDTODataLoader> logger) : base(logger)
		{
			this._log = logger;
		}

        #region Retrieves
        
        /// <summary>
        /// Gets all ProPricer Exports for a workspace.  This includes all system and workspace level exports.
        /// </summary>
        /// <param name="workspaceId">Workspace Id.</param>
        /// <returns>ProPricerDtos for the system and given workspace.</returns>
        
        public ICollection<ProPricerDTO> GetByWorkspaceId(int workspaceId)
        {
            List<ProPricerDTO> proPricerDtos = new List<ProPricerDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    proPricerDtos = gbe.ProPricerExports
                        .Where(x => x.WorkspaceID == workspaceId)
                        .Select(x => new ProPricerDTO
                        {
                            ExportID = x.ProPricerExportID,
                            Scope = ProPricerScope.Workspace,
                            WorkspaceID = x.WorkspaceID,
                            UpdateDate = x.UpdateDT,
                            FormatName = x.ProPricerExportName,

                            ProPricerTasksIEnum =
                                x.ProPricerFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Tasks)
                                    .Select(i => new ProPricerTasks
                                    {
                                        CustomFieldID = null, // needs to be done due to LINQ restrictions..
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Task = (ProPricerField_Task)i.ProPricerFieldID,
                                        ListOrder = i.ListOrder,
                                        Selection = ProPricerCustomFieldSelection.None,
                                        IsProjectMapField = i.ProPricerFieldLU.ProPricerCompanyID == 4
                                    })
                                .Concat(x.ProPricerCustomFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Tasks)
                                    .Select(i => new ProPricerTasks
                                    {
                                        CustomFieldID = i.CustomFieldID,
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Task = 0, // needs to be done due to LINQ restrictions..
                                        ListOrder = i.ListOrder,
                                        Selection = (ProPricerCustomFieldSelection)i.ProPricerCustomFieldSelectionID,
                                        IsProjectMapField = false
                                    }))
                                    .OrderBy(i => i.ListOrder),

                            ProPricerResourcesIEnum =
                                x.ProPricerFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Resources)
                                    .Select(i => new ProPricerResources
                                    {
                                        CustomFieldID = null, // needs to be done due to LINQ restrictions..
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Resource = (ProPricerField_Resources)i.ProPricerFieldID,
                                        ListOrder = i.ListOrder,
                                        Selection = ProPricerCustomFieldSelection.None,
                                        IsProjectMapField = i.ProPricerFieldLU.ProPricerCompanyID == 4
                                    })
                                .Concat(x.ProPricerCustomFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Resources)
                                    .Select(i => new ProPricerResources
                                    {
                                        CustomFieldID = i.CustomFieldID,
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Resource = 0, // needs to be done due to LINQ restrictions..
                                        ListOrder = i.ListOrder,
                                        Selection = (ProPricerCustomFieldSelection)i.ProPricerCustomFieldSelectionID,
                                        IsProjectMapField = false
                                    }))
                                    .OrderBy(i => i.ListOrder)

                        }).ToList();
                }

                proPricerDtos.ForEach(
                    x =>
                    {
                        x.ProPricerTasks = x.ProPricerTasksIEnum.ToCollection(); x.ProPricerTasksIEnum = null;
                        x.ProPricerResources = x.ProPricerResourcesIEnum.ToCollection(); x.ProPricerResourcesIEnum = null;
                        //Used to determine which ProPricer templates to display
                        x.IsProjectMapTemplate = x.ProPricerResources.Any(r => r.IsProjectMapField) || x.ProPricerTasks.Any(t => t.IsProjectMapField);
                    }
                );
            }

            // Retrieve the System exports as well
            ICollection<ProPricerDTO> systemExports = this.GetAllSystemExports();
            proPricerDtos.AddRange(systemExports);

            return proPricerDtos;
        }

        /// <summary>
        /// Gets all System Exports.
        /// </summary>
        /// <returns>ProPricerDtos for the system only.</returns>
        
        public ICollection<ProPricerDTO> GetAllSystemExports()
        {
            List<ProPricerDTO> proPricerDtos = new List<ProPricerDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    proPricerDtos = gbe.SystemProPricerExports
                        .Select(x => new ProPricerDTO
                        {
                            ExportID = x.SystemProPricerExportID,
                            Scope = ProPricerScope.System,
                            WorkspaceID = null,
                            UpdateDate = x.UpdateDT,
                            FormatName = x.ProPricerExportName,

                            ProPricerTasksIEnum =
                                x.SystemProPricerFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Tasks)
                                    .Select(i => new ProPricerTasks
                                    {
                                        CustomFieldID = null, // needs to be done due to LINQ restrictions..
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Task = (ProPricerField_Task)i.ProPricerFieldID,
                                        ListOrder = i.ListOrder,
                                        Selection = ProPricerCustomFieldSelection.None,
                                        IsProjectMapField = i.ProPricerFieldLU.ProPricerCompanyID == 4
                                    })
                                .Concat(x.SystemProPricerCustomFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Tasks)
                                    .Select(i => new ProPricerTasks
                                    {
                                        CustomFieldID = null, // needs to be done due to LINQ restrictions..
                                        CustomFieldName = i.CustomField,
                                        Task = 0, // needs to be done due to LINQ restrictions..
                                        ListOrder = i.ListOrder,
                                        Selection = (ProPricerCustomFieldSelection)i.ProPricerCustomFieldSelectionID,
                                        IsProjectMapField = false
                                    }))
                                    .OrderBy(i => i.ListOrder),

                            ProPricerResourcesIEnum =
                                x.SystemProPricerFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Resources)
                                    .Select(i => new ProPricerResources
                                    {
                                        CustomFieldID = null, // needs to be done due to LINQ restrictions..
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Resource = (ProPricerField_Resources)i.ProPricerFieldID,
                                        ListOrder = i.ListOrder,
                                        Selection = ProPricerCustomFieldSelection.None,
                                        IsProjectMapField = i.ProPricerFieldLU.ProPricerCompanyID == 4
                                    })
                                .Concat(x.SystemProPricerCustomFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Resources)
                                    .Select(i => new ProPricerResources
                                    {
                                        CustomFieldID = null, // needs to be done due to LINQ restrictions..
                                        CustomFieldName = i.CustomField,
                                        Resource = 0, // needs to be done due to LINQ restrictions..
                                        ListOrder = i.ListOrder,
                                        Selection = (ProPricerCustomFieldSelection)i.ProPricerCustomFieldSelectionID,
                                        IsProjectMapField = false
                                    }))
                                    .OrderBy(i => i.ListOrder)

                        }).ToList();
                }

                proPricerDtos.ForEach(
                    x =>
                    {
                        x.ProPricerTasks = x.ProPricerTasksIEnum.ToCollection(); x.ProPricerTasksIEnum = null;
                        x.ProPricerResources = x.ProPricerResourcesIEnum.ToCollection(); x.ProPricerResourcesIEnum = null;
                        //Used to determine which ProPricer templates to display
                        x.IsProjectMapTemplate = x.ProPricerResources.Any(r => r.IsProjectMapField) || x.ProPricerTasks.Any(t => t.IsProjectMapField);
                    }
                );
            }

            return proPricerDtos;
        }

        /// <summary>
        /// Get the Pro Pricer Export report by Export Id.
        /// </summary>
        /// <param name="inExportID">Export Id.</param>
        /// <param name="scope">The scope for the Id.</param>
        /// <returns>ProPricerDto for the given Export Id or null if it does not exist.</returns>
        public ProPricerDTO GetById(int inExportID, ProPricerScope scope)
        {
            if (scope == ProPricerScope.Workspace)
            {
                return this.GetByIdWorkspace(inExportID);
            }
            else
            {
                return this.GetByIdSystem(inExportID);
            }
        }

        /// <summary>
        /// Get the Workspace Pro Pricer Export report by Export Id.
        /// </summary>
        /// <param name="inExportID">Export Id.</param>
        /// <param name="scope">The scope for the Id.</param>
        /// <returns>ProPricerDto for the given Export Id or null if it does not exist.</returns>
        
        private ProPricerDTO GetByIdWorkspace(int inExportID)
        {
            ProPricerDTO proPricerDto = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    proPricerDto = gbe.ProPricerExports
                        .Where(x => x.ProPricerExportID == inExportID)
                        .Select(x => new ProPricerDTO
                        {
                            ExportID = x.ProPricerExportID,
                            Scope = ProPricerScope.Workspace,
                            WorkspaceID = x.WorkspaceID,
                            UpdateDate = x.UpdateDT,
                            FormatName = x.ProPricerExportName,

                            ProPricerTasksIEnum =
                                x.ProPricerFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Tasks)
                                    .Select(i => new ProPricerTasks
                                    {
                                        CustomFieldID = null, // needs to be done due to LINQ restrictions..
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Task = (ProPricerField_Task)i.ProPricerFieldID,
                                        ListOrder = i.ListOrder,
                                        Selection = ProPricerCustomFieldSelection.None
                                    })
                                .Concat(x.ProPricerCustomFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Tasks)
                                    .Select(i => new ProPricerTasks
                                    {
                                        CustomFieldID = i.CustomFieldID,
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Task = 0, // needs to be done due to LINQ restrictions..
                                        ListOrder = i.ListOrder,
                                        Selection = (ProPricerCustomFieldSelection)i.ProPricerCustomFieldSelectionID
                                    }))
                                    .OrderBy(i => i.ListOrder),

                            ProPricerResourcesIEnum =
                                x.ProPricerFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Resources)
                                    .Select(i => new ProPricerResources
                                    {
                                        CustomFieldID = null, // needs to be done due to LINQ restrictions..
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Resource = (ProPricerField_Resources)i.ProPricerFieldID,
                                        ListOrder = i.ListOrder,
                                        Selection = ProPricerCustomFieldSelection.None
                                    })
                                .Concat(x.ProPricerCustomFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Resources)
                                    .Select(i => new ProPricerResources
                                    {
                                        CustomFieldID = i.CustomFieldID,
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Resource = 0, // needs to be done due to LINQ restrictions..
                                        ListOrder = i.ListOrder,
                                        Selection = (ProPricerCustomFieldSelection)i.ProPricerCustomFieldSelectionID
                                    }))
                                    .OrderBy(i => i.ListOrder)

                        }).FirstOrDefault();
                }

                if (proPricerDto != null)
                {
                    proPricerDto.ProPricerTasks = proPricerDto.ProPricerTasksIEnum.ToCollection(); proPricerDto.ProPricerTasksIEnum = null;
                    proPricerDto.ProPricerResources = proPricerDto.ProPricerResourcesIEnum.ToCollection(); proPricerDto.ProPricerResourcesIEnum = null;
                }
            }

            return proPricerDto;
        }

        /// <summary>
        /// Get the System Pro Pricer Export report by Export Id.
        /// </summary>
        /// <param name="inExportID">Export Id.</param>
        /// <param name="scope">The scope for the Id.</param>
        /// <returns>ProPricerDto for the given Export Id or null if it does not exist.</returns>
        
        private ProPricerDTO GetByIdSystem(int inExportID)
        {
            ProPricerDTO proPricerDto = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    proPricerDto = gbe.SystemProPricerExports
                        .Where(x => x.SystemProPricerExportID == inExportID)
                        .Select(x => new ProPricerDTO
                        {
                            ExportID = x.SystemProPricerExportID,
                            Scope = ProPricerScope.System,
                            WorkspaceID = null,
                            UpdateDate = x.UpdateDT,
                            FormatName = x.ProPricerExportName,

                            ProPricerTasksIEnum =
                                x.SystemProPricerFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Tasks)
                                    .Select(i => new ProPricerTasks
                                    {
                                        CustomFieldID = null, // needs to be done due to LINQ restrictions..
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Task = (ProPricerField_Task)i.ProPricerFieldID,
                                        ListOrder = i.ListOrder,
                                        Selection = ProPricerCustomFieldSelection.None
                                    })
                                .Concat(x.SystemProPricerCustomFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Tasks)
                                    .Select(i => new ProPricerTasks
                                    {
                                        CustomFieldID = null,
                                        CustomFieldName = i.CustomField,
                                        Task = 0, // needs to be done due to LINQ restrictions..
                                        ListOrder = i.ListOrder,
                                        Selection = (ProPricerCustomFieldSelection)i.ProPricerCustomFieldSelectionID
                                    }))
                                    .OrderBy(i => i.ListOrder),

                            ProPricerResourcesIEnum =
                                x.SystemProPricerFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Resources)
                                    .Select(i => new ProPricerResources
                                    {
                                        CustomFieldID = null, // needs to be done due to LINQ restrictions..
                                        CustomFieldName = null, // needs to be done due to LINQ restrictions..
                                        Resource = (ProPricerField_Resources)i.ProPricerFieldID,
                                        ListOrder = i.ListOrder,
                                        Selection = ProPricerCustomFieldSelection.None
                                    })
                                .Concat(x.SystemProPricerCustomFieldXREFs.Where(i => i.ProPricerTypeID == (int)ProPricerType.Resources)
                                    .Select(i => new ProPricerResources
                                    {
                                        CustomFieldID = null, // needs to be done due to LINQ restrictions..
                                        CustomFieldName = i.CustomField,
                                        Resource = 0, // needs to be done due to LINQ restrictions..
                                        ListOrder = i.ListOrder,
                                        Selection = (ProPricerCustomFieldSelection)i.ProPricerCustomFieldSelectionID
                                    }))
                                    .OrderBy(i => i.ListOrder)

                        }).FirstOrDefault();
                }

                if (proPricerDto != null)
                {
                    proPricerDto.ProPricerTasks = proPricerDto.ProPricerTasksIEnum.ToCollection(); proPricerDto.ProPricerTasksIEnum = null;
                    proPricerDto.ProPricerResources = proPricerDto.ProPricerResourcesIEnum.ToCollection(); proPricerDto.ProPricerResourcesIEnum = null;
                }
            }

            return proPricerDto;
        }

        #endregion Retrieves

        #region Commits

        /// <summary>
        /// Saves a ProPricerDto.
        /// </summary>
        /// <param name="inProPricer">ProPricerDto</param>
        public virtual void SaveProPricerExport(ProPricerDTO inProPricer)
        {
            if (inProPricer == null)
            {
                throw new ArgumentNullException(nameof(inProPricer));
            }

            if (inProPricer.Scope == ProPricerScope.System)
            {
                throw new ArgumentException("System ProPricer Export sent to Workspace API");
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                if (inProPricer.Updateable == UpdateType.Deleted)
                {
                    this.DeleteProPricerExport(inProPricer);
                }
                else if (inProPricer.Updateable == UpdateType.Upsert)
                {
                    this.UpsertProPricerExport(inProPricer);
                }
            }
        }

        /// <summary>
        /// Saves a System ProPricerDto.
        /// </summary>
        /// <param name="inProPricer">ProPricerDto</param>
        public virtual void SaveSystemProPricerExport(ProPricerDTO inProPricer)
        {
            if (inProPricer == null)
            {
                throw new ArgumentNullException(nameof(inProPricer));
            }

            if (inProPricer.Scope == ProPricerScope.Workspace)
            {
                throw new ArgumentException("Workspace ProPricer Export sent to System API");
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                if (inProPricer.Updateable == UpdateType.Deleted)
                {
                    this.DeleteSystemProPricerExport(inProPricer);
                }
                else if (inProPricer.Updateable == UpdateType.Upsert)
                {
                    this.UpsertSystemProPricerExport(inProPricer);
                }
            }
        }

        /// <summary>
        /// Saves an update for a System ProPricer Custom Field Name.
        /// </summary>
        /// <param name="originalName">The original name.</param>
        /// <param name="updatedName">The updated name.</param>
        public virtual void SaveSystemProPricerCustomField(string originalName, string updatedName)
        {
            ICollection<ProPricerDTO> systemExports = this.GetAllSystemExports().Where(s => s.ProPricerResources.Any(r => r.CustomFieldName == originalName) || s.ProPricerTasks.Any(t => t.CustomFieldName == originalName)).ToList();

            foreach (ProPricerDTO export in systemExports)
            {
                if (export.ProPricerResources.Any())
                {
                    foreach (ProPricerResources resource in export.ProPricerResources)
                    {
                        if (resource.CustomFieldName == originalName)
                        {
                            resource.CustomFieldName = updatedName;
                        }
                    }
                }

                if (export.ProPricerTasks.Any())
                {
                    foreach (ProPricerTasks task in export.ProPricerTasks)
                    {
                        if (task.CustomFieldName == originalName)
                        {
                            task.CustomFieldName = updatedName;
                        }
                    }
                }

                export.Updateable = UpdateType.Upsert;
                this.UpsertSystemProPricerExport(export);
            }
        }

        /// <summary>
        /// Delete the ProPricer Export data by deleting all resources and then the tasks
        /// </summary>
        /// <param name="inProPricer"></param>
        private void DeleteProPricerExport(ProPricerDTO inProPricer)
        {
            if (inProPricer == null)
            {
                throw new ArgumentNullException(nameof(inProPricer));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.deleteProPricerExport(inProPricer.ExportID, inProPricer.UpdateDate);
                }
            }
        }

        /// <summary>
        /// Delete the System ProPricer Export data.
        /// </summary>
        /// <param name="inProPricer"></param>
        private void DeleteSystemProPricerExport(ProPricerDTO inProPricer)
        {
            if (inProPricer == null)
            {
                throw new ArgumentNullException(nameof(inProPricer));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.deleteSystemProPricerExport(inProPricer.ExportID, inProPricer.UpdateDate);
                }
            }
        }
        
        /// <summary>
        /// Save the ProPricer Export data. The resource/task data will be deleted and then resaved with any updated data. This avoids
        /// keeping track of UpdateDT in the Xref tables
        /// </summary>
        /// <param name="inProPricer"></param>
        private void UpsertProPricerExport(ProPricerDTO inProPricer)
        {
            if (inProPricer == null)
            {
                throw new ArgumentNullException(nameof(inProPricer));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                DateTime updatedTime = inProPricer.UpdateDate;
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // need to delete all previous Resources and Tasks in the Xref tables before the save (only on an edit)
                    if (inProPricer.ExportID > 0)
                    {
                        gbe.deleteProPricerExportResources(inProPricer.ExportID, inProPricer.UpdateDate);

                        updatedTime = this.GetById(inProPricer.ExportID, ProPricerScope.Workspace).UpdateDate; // after the delete, the UpdateDate is updated

                        gbe.deleteProPricerExportTasks(inProPricer.ExportID, updatedTime);

                        // get new date
                        updatedTime = this.GetById(inProPricer.ExportID, ProPricerScope.Workspace).UpdateDate;
                    }

                    int updatedExportID = (from s in
                                               gbe.upsertProPricerExport(inProPricer.ExportID, inProPricer.FormatName.Replace("'", string.Empty).Replace("\"", string.Empty), inProPricer.WorkspaceID, updatedTime)
                                           select s).FirstOrDefault().Value;

                    updatedTime = this.GetById(updatedExportID, ProPricerScope.Workspace).UpdateDate;

                    // only save the tasks/resources fields if the export ID came back as a positive #
                    if (updatedExportID > 0)
                    {

                        // insert the tasks/resources fields
                        foreach (ProPricerTasks task in inProPricer.ProPricerTasks)
                        {
                            if (task.CustomFieldID.HasValue)
                            {
                                gbe.insertProPricerExportCustomField(updatedExportID, task.CustomFieldID, (int)ProPricerType.Tasks, (int)task.Selection, task.ListOrder, updatedTime);

                            }
                            else
                            {
                                gbe.insertProPricerExportField(updatedExportID, (int)task.Task, (int)ProPricerType.Tasks, task.ListOrder, updatedTime);
                            }
                        }

                        foreach (ProPricerResources resources in inProPricer.ProPricerResources)
                        {
                            if (resources.CustomFieldID.HasValue)
                            {
                                gbe.insertProPricerExportCustomField(updatedExportID, resources.CustomFieldID, (int)ProPricerType.Resources, (int)resources.Selection, resources.ListOrder, updatedTime);

                            }
                            else
                            {
                                gbe.insertProPricerExportField(updatedExportID, (int)resources.Resource, (int)ProPricerType.Resources, resources.ListOrder, updatedTime);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save the System ProPricer Export data. The resource/task data will be deleted and then resaved with any updated data. This avoids
        /// keeping track of UpdateDT in the Xref tables
        /// </summary>
        /// <param name="inProPricer"></param>
        private void UpsertSystemProPricerExport(ProPricerDTO inProPricer)
        {
            if (inProPricer == null)
            {
                throw new ArgumentNullException(nameof(inProPricer));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                DateTime updatedTime = inProPricer.UpdateDate;
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // all previous Resources and Tasks in the Xref tables deleted during upsert
                    int updatedExportID = (from s in
                                               gbe.upsertSystemProPricerExport(inProPricer.ExportID, inProPricer.FormatName.Replace("'", string.Empty).Replace("\"", string.Empty), updatedTime)
                                           select s).FirstOrDefault().Value;

                    updatedTime = this.GetById(updatedExportID, ProPricerScope.System).UpdateDate;

                    // only save the tasks/resources fields if the export ID came back as a positive #
                    if (updatedExportID > 0)
                    {

                        // insert the tasks/resources fields
                        foreach (ProPricerTasks task in inProPricer.ProPricerTasks)
                        {
                            if (string.IsNullOrWhiteSpace(task.CustomFieldName))
                            {
                                gbe.insertSystemProPricerExportField(updatedExportID, (int)task.Task, (int)ProPricerType.Tasks, task.ListOrder, updatedTime);
                            }
                            else
                            {
                                gbe.insertSystemProPricerExportCustomField(updatedExportID, task.CustomFieldName, (int)ProPricerType.Tasks, (int)task.Selection, task.ListOrder, updatedTime);
                            }
                        }

                        foreach (ProPricerResources resources in inProPricer.ProPricerResources)
                        {
                            if (string.IsNullOrWhiteSpace(resources.CustomFieldName))
                            {
                                gbe.insertSystemProPricerExportField(updatedExportID, (int)resources.Resource, (int)ProPricerType.Resources, resources.ListOrder, updatedTime);
                            }
                            else
                            {
                                gbe.insertSystemProPricerExportCustomField(updatedExportID, resources.CustomFieldName, (int)ProPricerType.Resources, (int)resources.Selection, resources.ListOrder, updatedTime);
                            }
                        }
                    }
                }
            }
        }

        #endregion Commits
    }
}