// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.CustomFields
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.Reference;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;

	public class RestoreDefaultOptions
	{
		private static Logger _log = new Logger(typeof(RestoreDefaultOptions));
		private IInUseDataLoader _InUseDataLoader;
		private IWorkspaceDTODataLoader workspaceLoader;
		private IPerformingOrgDTODataLoader perfOrgLoader;
		private IResourceDTODataLoader resourceLoader;

		/// <summary>
		/// Constructor with Resource DTO and Performing Org DTO Mapper
		/// </summary>
		/// <param name="perfOrgLoader"></param>
		public RestoreDefaultOptions(IInUseDataLoader inInUseLoader, IWorkspaceDTODataLoader workspaceLoader, IPerformingOrgDTODataLoader perfOrgLoader, IResourceDTODataLoader resourceLoader)
		{
			this._InUseDataLoader = inInUseLoader;
			this.workspaceLoader = workspaceLoader;
			this.perfOrgLoader = perfOrgLoader;
			this.resourceLoader = resourceLoader;
		}

		/// <summary>
		/// This function will determine what options were added/changed/deleted/not changed between
		/// a standard Performing Org List and the Default Performing Org List.
		/// NOTE: At the end of this function, the RestorePerfOrg will call the Save 
		/// </summary>
		/// <param name="inPerfOrgListID">the Perf Org List ID that is being restored</param>
		/// <returns>The options that were different between the input list and the default list</returns>
		public RestoreOptionData RestorePerfOrg(FullWorkspace inWorkspace)
		{
			if (inWorkspace == null)
			{
				throw new ArgumentNullException(nameof(inWorkspace));
			}

			// to return
			RestoreOptionData restoreOptions = new RestoreOptionData();

			using (StopwatchTimer sw = new StopwatchTimer(_log))
			{

				// the performing Orgs to Save
				Collection<PerformingOrgDTO> savePerfOrgs = new Collection<PerformingOrgDTO>();
				List<PerformingOrgDTO> savePerfOrgList = new List<PerformingOrgDTO>();

				// Get the current Perf Org List and the default Perf Org List
				IReadOnlyCollection<PerformingOrgDTO> currentPerfOrgs = inWorkspace.PerformingOrgsForWsList;
				Collection<PerformingOrgDTO> defaultPerfOrgs = this.perfOrgLoader.GetGlobalPerformingOrgs();

				// find the options that were added to the default Perf Org List but not in the current Perf Org List
				Collection<RestoreOption> optionsAdded = new Collection<RestoreOption>();
				List<RestoreOption> perfOrgsAdded =
					(from p in defaultPerfOrgs
					 where !(from c in currentPerfOrgs
							 select c.PerformingOrgName).Contains(p.PerformingOrgName)
					 select new RestoreOption
					 {
						 Name = p.PerformingOrgName,
						 Desc = p.PerformingOrgDesc,
					 }).ToList();

				optionsAdded = new Collection<RestoreOption>(perfOrgsAdded.ToArray());
				restoreOptions.OptionAdded = optionsAdded;

				// the Perf Orgs that have been marked as Add should be added to the collection to save
				List<PerformingOrgDTO> perfOrgsAddedToSave = (from po in perfOrgsAdded
															  select new PerformingOrgDTO
															  {     // need to set id's since objects are added to a dictionary, set below
																  PerformingOrgName = po.Name,
																  PerformingOrgDesc = po.Desc,
																  Updateable = UpdateType.Upsert
															  }).ToList();

				int newid = -1;
				foreach (PerformingOrgDTO item in perfOrgsAddedToSave)
				{
					item.Id = newid;
					newid--;
				}

				savePerfOrgList.AddRange(perfOrgsAddedToSave);

				// find the options that were changed between the default Perf Org List and the current Perf Org List
				// the Name(shown on the UI as ID) would be the same between the two lists
				Collection<RestoreOptionChanged> optionsChanged = new Collection<RestoreOptionChanged>();

				HashSet<int> perfOrgIDsInUse = this._InUseDataLoader.GetWorkspacePerfOrgIDsInUseByPerfOrgListID(inWorkspace.PerfOrgListID);
				List<RestoreOptionChanged> perfOrgsChanged =
					(from p in defaultPerfOrgs
					 from c in currentPerfOrgs
					 where c.PerformingOrgName == p.PerformingOrgName && (c.PerformingOrgDesc != p.PerformingOrgDesc) && perfOrgIDsInUse.Contains(c.Id) != true // don't bother checking if an in use has changed
					 select new RestoreOptionChanged
					 {
						 ID = c.Id,
						 Name = c.PerformingOrgName,
						 Desc = c.PerformingOrgDesc,
						 ChangedToName = p.PerformingOrgName,
						 ChangedToDesc = p.PerformingOrgDesc,
						 UpdateDate = c.UpdateDate
					 }).ToList();

				optionsChanged = new Collection<RestoreOptionChanged>(perfOrgsChanged.ToArray());
				restoreOptions.OptionChanged = optionsChanged;

				// the Perf Orgs that have been marked as changed should be added to the collection to save
				ICollection<PerformingOrgDTO> perfOrgsChangedToSave = (from po in perfOrgsChanged
																	   select new PerformingOrgDTO
																	   {
																		   Id = po.ID,
																		   PerformingOrgName = po.ChangedToName,
																		   PerformingOrgDesc = po.ChangedToDesc,
																		   UpdateDate = po.UpdateDate,
																		   Updateable = UpdateType.Upsert
																	   }).ToList();
				savePerfOrgList.AddRange(perfOrgsChangedToSave.ToList());

				// find the options that were deleted from the current Perf Org List because it was not in the default Perf Org List
				Collection<RestoreOption> optionsDeleted = new Collection<RestoreOption>();
				List<RestoreOption> perfOrgsDeleted =
					(from p in currentPerfOrgs
					 where !(from d in defaultPerfOrgs
							 select d.PerformingOrgName).Contains(p.PerformingOrgName) && perfOrgIDsInUse.Contains(p.Id) != true // don't bother checking if an in use has been deleted
					 select new RestoreOption
					 {
						 ID = p.Id,
						 Name = p.PerformingOrgName,
						 Desc = p.PerformingOrgDesc,
						 UpdateDate = p.UpdateDate
					 }).ToList();

				optionsDeleted = new Collection<RestoreOption>(perfOrgsDeleted.ToArray());
				restoreOptions.OptionDeleted = optionsDeleted;

				// the Perf Orgs that have been marked as deleted should be added to the collection to save
				ICollection<PerformingOrgDTO> perfOrgsDeletedToSave = (from po in perfOrgsDeleted
																	   select new PerformingOrgDTO
																	   {
																		   Id = po.ID,
																		   PerformingOrgName = po.Name,
																		   PerformingOrgDesc = po.Desc,
																		   UpdateDate = po.UpdateDate,
																		   Updateable = UpdateType.Deleted
																	   }).ToList();
				savePerfOrgList.AddRange(perfOrgsDeletedToSave);

				// find the options that could not be edited because they are in use in the current Perf Org List
				Collection<RestoreOption> optionsNotchanged = new Collection<RestoreOption>();

				List<RestoreOption> PerfOrgsInUse = (from p in currentPerfOrgs
													 where perfOrgIDsInUse.Contains(p.Id)
													 select new RestoreOption
													 {
														 Name = p.PerformingOrgName,
														 Desc = p.PerformingOrgDesc
													 }).ToList();
				optionsNotchanged = new Collection<RestoreOption>(PerfOrgsInUse.ToList());
				restoreOptions.OptionNotChanged = optionsNotchanged;

				// Since these options couldn't be changed, do not need to anything to the savePerfOrgList

				// save the performing organizations so it can be in synch with the default perf org list
				savePerfOrgs = new Collection<PerformingOrgDTO>(savePerfOrgList.ToList());
				this.perfOrgLoader.SaveWorkspacePerformingOrgs(savePerfOrgs, inWorkspace.PerfOrgListID);

				// set the updatePerfOrgChange flag to false
				this.workspaceLoader.UpdatePerfOrgChangeFlag(inWorkspace, false);
			}

			return restoreOptions;
		}

		/// <summary>
		/// This function will determine what options were added/changed/deleted/not changed between
		/// a the system-level Resource List and the Workspace Resource List.
		/// </summary>
		/// <param name="workspace">workspace</param>
		/// <returns>The options that were different between the input list and the default list</returns>
		public RestoreOptionData RestoreSystemResources(FullWorkspace workspace)
		{
			_ = workspace ?? throw new ArgumentNullException(nameof(workspace));

			RestoreOptionData restoreOptions = new RestoreOptionData();

			using (StopwatchTimer sw = new StopwatchTimer(_log))
			{
				List<ResourceDTO> saveResourceList = new List<ResourceDTO>();

				IReadOnlyCollection<ResourceDTO> workspaceResources = workspace.ResourcesForWsResourceListId;
				ICollection<ResourceDTO> systemResources = resourceLoader.GetGlobalResources();

				// find the options that were added to the System Resource List but not in the Workspace Resource List
				Collection<RestoreOption> resourcesAdded = GetAddedResources(workspaceResources, systemResources);
				restoreOptions.OptionAdded = resourcesAdded;

				// the Resources that have been marked as Add should be added to the collection to save
				List<ResourceDTO> resourcesAddedToSave = (from r in resourcesAdded
														  select new ResourceDTO
														  {
															  ResourceName = r.Name,
															  ResourceDesc = r.Desc,
															  SegRegion = r.SegRegion,
															  LaborType = r.LaborType,
															  RateType = r.RateType,
															  ElementOfCost = r.ElementofCost,
															  Updateable = UpdateType.Upsert
														  }).ToList();

				// set IDs to negatives for save as new resources
				int newid = -1;
				foreach (ResourceDTO item in resourcesAddedToSave)
				{
					item.Id = newid;
					newid--;
				}

				saveResourceList.AddRange(resourcesAddedToSave);

				// find the options that were changed between the system Resource List and the Workspace Resource List
				// the Name(shown on the UI as ID) would be the same between the two lists
				HashSet<int> resourceIDsInUse = _InUseDataLoader.GetWorkspaceResourceIDsInUseByListID(workspace.ResourceListID);
				Collection<RestoreOptionChanged> resourcesChanged = GetChangedResources(workspaceResources, systemResources, resourceIDsInUse);
				restoreOptions.OptionChanged = resourcesChanged;

				// the Resources that have been marked as changed should be added to the collection to save
				ICollection<ResourceDTO> resourcesChangedToSave = (from r in resourcesChanged
																   select new ResourceDTO
																   {
																	   Id = r.ID,
																	   ResourceName = r.ChangedToName,
																	   ResourceDesc = r.ChangedToDesc,
																	   SegRegion = r.ChangedToSegRegion,
																	   LaborType = r.ChangedToLaborType,
																	   RateType = r.ChangedToRateType,
																	   ElementOfCost = r.ChangedToElementOfCost,
																	   UpdateDate = r.UpdateDate,
																	   Updateable = UpdateType.Upsert
																   }).ToList();

				saveResourceList.AddRange(resourcesChangedToSave);

				// find the options that were deleted from the Workspace Resource List because it was not in the System Resource List
				Collection<RestoreOption> resourcesDeleted = GetDeletedResources(workspaceResources, systemResources, resourceIDsInUse);
				restoreOptions.OptionDeleted = resourcesDeleted;

				// the Resources that have been marked as deleted should be added to the collection to save
				ICollection<ResourceDTO> resourcesDeletedToSave = (from r in resourcesDeleted
																  select new ResourceDTO
																  {
																	  Id = r.ID,
																	  ResourceName = r.Name,
																	  ResourceDesc = r.Desc,
																	  SegRegion = r.SegRegion,
																	  LaborType= r.LaborType,
																	  RateType = r.RateType,
																	  ElementOfCost = r.ElementofCost,
																	  UpdateDate = r.UpdateDate,
																	  Updateable = UpdateType.Deleted
																  }).ToList();

				saveResourceList.AddRange(resourcesDeletedToSave);

				// find the options that could not be edited because they are in use in the Workspace Resource List
				Collection<RestoreOption> resourcesInUse = (from r in workspaceResources
															where resourceIDsInUse.Contains(r.Id)
															select new RestoreOption
															{
																Name = r.ResourceName,
																Desc = r.ResourceDesc,
																SegRegion = r.SegRegion,
																LaborType = r.LaborType,
																RateType = r.RateType,
																ElementofCost = r.ElementOfCost
															}).ToCollection();

				// Since these options couldn't be changed, do not need to add to the saveResourceList, just add to restore options
				restoreOptions.OptionNotChanged = resourcesInUse;

				// save the resources so it can be in sync with the system resource list
				this.resourceLoader.SaveWorkspaceResources(workspace, saveResourceList.ToCollection());
			}

			return restoreOptions;
		}

		/// <summary>
		/// Have Workspace Resources changed from System-level resources?
		/// Used to display Restore Resources button
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool ResourcesHaveChanged(FullWorkspace workspace)
		{
			_ = workspace ?? throw new ArgumentNullException(nameof(workspace));

			bool resourcesHaveChanged = false;

			using (StopwatchTimer sw = new StopwatchTimer(_log))
			{
				IReadOnlyCollection<ResourceDTO> workspaceResources = workspace.ResourcesForWsResourceListId;
				ICollection<ResourceDTO> systemResources = resourceLoader.GetGlobalResources();
				HashSet<int> resourceIDsInUse = _InUseDataLoader.GetWorkspaceResourceIDsInUseByListID(workspace.ResourceListID);

				resourcesHaveChanged = GetAddedResources(workspaceResources, systemResources).Any()
					|| GetChangedResources(workspaceResources, systemResources, resourceIDsInUse).Any()
					|| GetDeletedResources(workspaceResources, systemResources, resourceIDsInUse).Any();
			}

			return resourcesHaveChanged;
		}

		/// <summary>
		/// Get resources in System Resources that will be added to Workspace Reources
		/// </summary>
		/// <param name="workspaceResources">Workspace-level resources</param>
		/// <param name="systemResources">System-level resources</param>
		/// <returns>Collection of RestoreOption containing Resources to be added</returns>
		private Collection<RestoreOption> GetAddedResources(IReadOnlyCollection<ResourceDTO> workspaceResources, ICollection<ResourceDTO> systemResources)
		{
			return (from s in systemResources
					where !(from w in workspaceResources
							select w.ResourceName).Contains(s.ResourceName)
					select new RestoreOption
					{
						Name = s.ResourceName,
						Desc = s.ResourceDesc,
						SegRegion = s.SegRegion,
						LaborType = s.LaborType,
						ElementofCost = s.ElementOfCost,
						RateType = s.RateType
					}).ToCollection();
		}

		/// <summary>
		/// Get Workspace Resources that were changed that will be updated with the System Resource values (excluding in-use resources)
		/// </summary>
		/// <param name="workspaceResources">Workspace-level resources</param>
		/// <param name="systemResources">System-level resources</param>
		/// <param name="resourceIDsInUse">IDs for Workspace-level resources that are in use</param>
		/// <returns>Collection of RestoreOptionChanged containing Resources to be updated</returns>
		private Collection<RestoreOptionChanged> GetChangedResources(IReadOnlyCollection<ResourceDTO> workspaceResources, ICollection<ResourceDTO> systemResources, HashSet<int> resourceIDsInUse)
		{
			return (from s in systemResources
					from w in workspaceResources
					where w.ResourceName == s.ResourceName 
						&& (w.ResourceDesc != s.ResourceDesc 
							|| w.SegRegion != s.SegRegion 
							|| w.LaborType != s.LaborType
							|| w.RateType != s.RateType
							|| w.ElementOfCost != s.ElementOfCost) 
						&& !resourceIDsInUse.Contains(w.Id)
					select new RestoreOptionChanged
					{
						ID = w.Id,
						Name = w.ResourceName,
						Desc = w.ResourceDesc,
						SegRegion = w.SegRegion,
						LaborType = w.LaborType,
						ElementOfCost = w.ElementOfCost,
						RateType = w.RateType,
						ChangedToName = s.ResourceName,
						ChangedToDesc = s.ResourceDesc,
						ChangedToSegRegion = s.SegRegion,
						ChangedToLaborType = s.LaborType,
						ChangedToElementOfCost = s.ElementOfCost,
						ChangedToRateType = s.RateType,
						UpdateDate = w.UpdateDate
					}).ToCollection();
		}

		/// <summary>
		/// Get Workspace Resources that will be deleted because they are no longer in System-level resources (excluding in-use resources)
		/// </summary>
		/// <param name="workspaceResources">Workspace-level resources</param>
		/// <param name="systemResources">System-level resources</param>
		/// <param name="resourceIDsInUse">IDs for Workspace-level resources that are in use</param>
		/// <returns>Collection of RestoreOption containing Resources to be deleted</returns>
		private Collection<RestoreOption> GetDeletedResources(IReadOnlyCollection<ResourceDTO> workspaceResources, ICollection<ResourceDTO> systemResources, HashSet<int> resourceIDsInUse)
		{
			return (from w in workspaceResources
					where !(from s in systemResources
							select s.ResourceName).Contains(w.ResourceName) && !resourceIDsInUse.Contains(w.Id)
					select new RestoreOption
					{
						ID = w.Id,
						Name = w.ResourceName,
						Desc = w.ResourceDesc,
						SegRegion = w.SegRegion,
						LaborType = w.LaborType,
						ElementofCost = w.ElementOfCost,
						RateType = w.RateType,
						UpdateDate = w.UpdateDate
					}).ToCollection();
		}
	}
}
