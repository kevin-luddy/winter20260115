// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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

        /// <summary>
        /// Constructor with Resource DTO and Performing Org DTO Mapper
        /// </summary>
        /// <param name="perfOrgLoader"></param>
        public RestoreDefaultOptions(IInUseDataLoader inInUseLoader, IWorkspaceDTODataLoader workspaceLoader, IPerformingOrgDTODataLoader perfOrgLoader)
        {
            this._InUseDataLoader = inInUseLoader;
            this.workspaceLoader = workspaceLoader;
            this.perfOrgLoader = perfOrgLoader;
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
                var perfOrgsAdded =
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
                var perfOrgsChanged =
                    (from p in defaultPerfOrgs
                    from c in currentPerfOrgs
                    where c.PerformingOrgName == p.PerformingOrgName && (c.PerformingOrgDesc != p.PerformingOrgDesc) && perfOrgIDsInUse.Contains(c.Id) != true // don't bother checking if an in use has changed
                    select new RestoreOptionChanged
                    {
                        ID = c.Id,
                        Name = c.PerformingOrgName,
                        Desc = c.PerformingOrgDesc,
                        ChangedToID = p.PerformingOrgName,
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
                                                                          PerformingOrgName = po.ChangedToID,
                                                                          PerformingOrgDesc = po.ChangedToDesc,
                                                                          UpdateDate = po.UpdateDate,
                                                                          Updateable = UpdateType.Upsert
                                                                      }).ToList();
                savePerfOrgList.AddRange(perfOrgsChangedToSave.ToList());

                // find the options that were deleted from the current Perf Org List because it was not in the default Perf Org List
                Collection<RestoreOption> optionsDeleted = new Collection<RestoreOption>();
                var perfOrgsDeleted =
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

                var PerfOrgsInUse = (from p in currentPerfOrgs
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
    }
}
