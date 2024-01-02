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
    using IES.Standard;
    using GenBOE.Dtos;
    using GenBOE.Models;
	using Microsoft.Extensions.Logging;

	public class WorkspaceVariableDTODataLoader : IWorkspaceVariableDTODataLoader
    {
        private readonly ILogger _log;

        public WorkspaceVariableDTODataLoader(ILogger<WorkspaceVariableDTODataLoader> logger)
		{
			this._log = logger;
		}

        #region Retrieves

        /// <summary>
        /// Get all workspace variables for a given workspace Id.
        /// </summary>
        /// <param name="wsId">Workspace Id.</param>
        /// <returns>Collection of workspace variables for a workspace.</returns>
        
        virtual public ICollection<WorkspaceVariableDTO> GetByWorkspaceID(int wsId)
        {
            List<WorkspaceVariableDTO> toReturn = new List<WorkspaceVariableDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = gbe.WorkspaceVariables.Where(v => v.WorkspaceID == wsId).Select(v => new WorkspaceVariableDTO
                        {
                            Id = v.WorkspaceVariableID,
                            WorkspaceVariableName = v.WorkspaceVariableName,
                            WorkspaceVariableValue = v.WorkspaceVariableValue ?? 0,
                            IsPercentage = v.IsPercentage,
                            WorkspaceID = v.WorkspaceID,
                            SortBOEBy = (VarSortBOEBy)v.SortByID,
                            ValueType = (VarValueType)v.ValueTypeID,
                            UpdateDate = v.UpdateDT,
                            InUse = v.BOETaskElementWorkspaceVariableXREFs.Any(),

                            TaskElementIdsIEnum = v.BOETaskElementWorkspaceVariableXREFs.Select(x => x.BOETaskElementID).OrderBy(x => x),

                            SumVariableResourceTypeIDsIEnum = v.WorkspaceVariableSumVariableResourceTypeXREFs.Select(x => x.SumVariableResourceTypeID).OrderBy(x => x),

                            SelectedBOEsToSumIEnum = v.SumOfBOE_WorkspaceVariableXREF.Select(x => new SelectBOEsToSum
                                                                                                {
                                                                                                    OVSumID = x.WVSumID,
                                                                                                    OrdinaryVariableID = x.WorkspaceVariableID,
                                                                                                    BoeID = x.BOEID,
                                                                                                    CLINID = x.CLINID,
                                                                                                    WBSID = x.WBSID
                                                                                                })
                        }).ToList();

                    toReturn.ForEach(x => 
                    {
                        x.TaskElementIds = x.TaskElementIdsIEnum.ToCollection(); x.TaskElementIdsIEnum = null;
                        x.SumVariableResourceTypeIDs = x.SumVariableResourceTypeIDsIEnum.ToCollection(); x.SumVariableResourceTypeIDsIEnum = null;

                        x.SelectedBOEsToSum = x.SelectedBOEsToSumIEnum.ToCollection(); x.SelectedBOEsToSumIEnum = null;
                    });
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get workspace variable DTO based on the workspace variable ID
        /// </summary>
        /// <param name="inWorkspaceVarID">workspace variable ID</param>
        /// <returns>DTO</returns>
        virtual public WorkspaceVariableDTO GetById(int inWorkspaceVarID)
        {
            return this.GetByIds(new List<int>() { inWorkspaceVarID }).FirstOrDefault();
        }

        /// <summary>
        /// Returns a collection of Workspace Variable DTOs based on the Collection of Ids.
        /// </summary>
        /// <param name="ids">Capture Ids</param>
        /// <returns>The matching DTOs</returns>
        
        public ICollection<WorkspaceVariableDTO> GetByIds(IReadOnlyCollection<int> ids)
        {
            List<WorkspaceVariableDTO> toReturn = new List<WorkspaceVariableDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = gbe.WorkspaceVariables.Where(v => ids.Contains(v.WorkspaceVariableID)).Select(v => new WorkspaceVariableDTO
                    {
                        Id = v.WorkspaceVariableID,
                        WorkspaceVariableName = v.WorkspaceVariableName,
                        WorkspaceVariableValue = v.WorkspaceVariableValue ?? 0,
                        IsPercentage = v.IsPercentage,
                        WorkspaceID = v.WorkspaceID,
                        SortBOEBy = (VarSortBOEBy)v.SortByID,
                        ValueType = (VarValueType)v.ValueTypeID,
                        UpdateDate = v.UpdateDT,
                        InUse = v.BOETaskElementWorkspaceVariableXREFs.Any(),

                        TaskElementIdsIEnum = v.BOETaskElementWorkspaceVariableXREFs.Select(x => x.BOETaskElementID).OrderBy(x => x),

                        SumVariableResourceTypeIDsIEnum = v.WorkspaceVariableSumVariableResourceTypeXREFs.Select(x => x.SumVariableResourceTypeID).OrderBy(x => x),

                        SelectedBOEsToSumIEnum = v.SumOfBOE_WorkspaceVariableXREF.Select(x => new SelectBOEsToSum
                        {
                            OVSumID = x.WVSumID,
                            OrdinaryVariableID = x.WorkspaceVariableID,
                            BoeID = x.BOEID,
                            CLINID = x.CLINID,
                            WBSID = x.WBSID
                        })
                    }).ToList();

                    toReturn.ForEach(x =>
                    {
                        x.TaskElementIds = x.TaskElementIdsIEnum.ToCollection(); x.TaskElementIdsIEnum = null;
                        x.SumVariableResourceTypeIDs = x.SumVariableResourceTypeIDsIEnum.ToCollection(); x.SumVariableResourceTypeIDsIEnum = null;

                        x.SelectedBOEsToSum = x.SelectedBOEsToSumIEnum.ToCollection(); x.SelectedBOEsToSumIEnum = null;
                    });
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get BOE IDs that are currently using the workspace variable ID
        /// </summary>
        /// <param name="inWorkspaceVarID">workspace variable ID</param>
        /// <returns>Collection of distinct BOE IDs</returns>
        
        virtual public Collection<int> GetBOEIDsUsingWorkspaceVarID(int inWorkspaceVarID)
        {
            Collection<int> toReturn = new Collection<int>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var response = from b in gbe.BOETaskElementWorkspaceVariableXREFs
                                   where b.WorkspaceVariableID == inWorkspaceVarID
                                   select b.BOETaskElement.BOEID;

                    if (response.Count() > 0)
                    {
                        toReturn = new Collection<int>(response.ToArray());
                    }
                }
            }
            
            return toReturn;
        }

        #endregion

        #region Commits

        /// <summary>
        /// Save a collection of workspace variable DTOs
        /// </summary>
        /// <param name="inWorkspaceVars">collection of Workspace variable DTOs</param>
        virtual public Dictionary<int, int> SaveWorkspaceVariables(Collection<WorkspaceVariableDTO> inWorkspaceVars)
        {
            if (inWorkspaceVars == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceVars));
            }
            if (inWorkspaceVars.Any(x => x.Updateable == UpdateType.None))
            {
                throw new ArgumentException("Please supply the Updateable Argument");
            }

            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            foreach (WorkspaceVariableDTO workspaceVar in inWorkspaceVars)
            {
                if (workspaceVar.Updateable == UpdateType.Upsert)
                {
                    toReturn.Add(workspaceVar.Id, this.UpsertWorkspaceVariable(workspaceVar));
                }
                else if (workspaceVar.Updateable == UpdateType.Deleted)
                {
                    this.DeleteWorkspaceVariable(workspaceVar);
                    toReturn.Add(workspaceVar.Id, workspaceVar.Id);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upsert a single workspace variable DTO
        /// </summary>
        /// <param name="inSaveWorkspaceVar">workspace variable DTO</param>
        private int UpsertWorkspaceVariable(WorkspaceVariableDTO inSaveWorkspaceVar)
        {
            if (inSaveWorkspaceVar == null)
            {
                throw new ArgumentNullException(nameof(inSaveWorkspaceVar));
            }

            int WorkspaceVarID = 0;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // Save the workspace variable
                    WorkspaceVarID = Convert.ToInt32(gbe.upsertWorkspaceVariable(
                        inSaveWorkspaceVar.Id,
                        inSaveWorkspaceVar.WorkspaceVariableName,
                        inSaveWorkspaceVar.WorkspaceVariableValue,
                        inSaveWorkspaceVar.WorkspaceID,
                        (int)inSaveWorkspaceVar.SortBOEBy,
                        (int)inSaveWorkspaceVar.ValueType,
                        inSaveWorkspaceVar.IsPercentage,
                        inSaveWorkspaceVar.UpdateDate).FirstOrDefault());

                } // end gbe

                // if the result ID is not a positive number, something bad went wrong so log it
                if (WorkspaceVarID < 0)
                {
                    this._log.LogError("The returned ID from upsertWorkspaceVariable SP was negative");
                }
                else
                {
                    // delete all the sum of boe workspace variables before saving the new list
                    // this way the front end doesn't have to keep track of all the checks/unchecks
                    // Also this cleans up the variable if it changed from SumOfBOEs to Discrete
                    this.DeleteSumOfBoeByWorkspaceVarID(WorkspaceVarID);

                    if (inSaveWorkspaceVar.SelectedBOEsToSum.Any())
                    {
                        this.InsertSumOfBoeByWorkspace(inSaveWorkspaceVar, WorkspaceVarID);
                    }

                    // If this was a new workspace variable, set the new ID on the DTO for later use, if necessary
                    if (inSaveWorkspaceVar.Id < 0)
                    {
                        inSaveWorkspaceVar.Id = WorkspaceVarID;
                    }

                    // delete all the workspace variables resource types before saving the new resources
                    // this way the front end doesn't have to keep track of all the checks/unchecks
                    this.DeleteWorkspaceVarResourceType(WorkspaceVarID);
                    if (inSaveWorkspaceVar.SumVariableResourceTypeIDs.Any())
                    {
                        this.InsertWorkspaceVarResourceType(inSaveWorkspaceVar, WorkspaceVarID);
                    }
                }
            }

            return WorkspaceVarID;
        }

        /// <summary>
        /// Delete a single workspace variable DTO
        /// </summary>
        /// <param name="inDeleteWorkspaceVar">delete workspace variable</param>
        private void DeleteWorkspaceVariable(WorkspaceVariableDTO inDeleteWorkspaceVar)
        {
            // check if the input is null
            if (inDeleteWorkspaceVar == null)
            {
                throw new ArgumentNullException(nameof(inDeleteWorkspaceVar));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                // before deleting the main workspace variable, check to see if we're deleting a
                // SumOfBOEs variable. If we are, then need to delete the Sum of BOEs Xref entries first
                if (inDeleteWorkspaceVar.ValueType.Equals(VarValueType.SumOfBOEs))
                {
                    this.DeleteSumOfBoeByWorkspaceVarID(inDeleteWorkspaceVar.Id);
                }

                // delete the associated resource types
                if (inDeleteWorkspaceVar.SumVariableResourceTypeIDs.Count > 0)
                {
                    this.DeleteWorkspaceVarResourceType(inDeleteWorkspaceVar.Id);
                }

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // Delete the workspace variable
                    gbe.deleteWorkspaceVariable(inDeleteWorkspaceVar.Id, inDeleteWorkspaceVar.UpdateDate);

                } // end gbe
            }
        }

        /// <summary>
        /// Deletes the Sum of BOE workspace variable
        /// </summary>
        /// <param name="inDeleteWorkspaceVar">workspace variable</param>
        private void DeleteSumOfBoeByWorkspaceVarID(int inDeleteWorkspaceVarID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // Delete the sum of BOE by workspace variable ID
                    gbe.deleteSumOfBOEByWorkspaceVariableID(inDeleteWorkspaceVarID);
                } // end gbe
            }
        }

        /// <summary>
        /// Insert the Sum of BOE for a workspace variable
        /// </summary>
        /// <param name="inWorkspaceVariable">the workspace variable DTO</param>
        /// <param name="inSavedWorkspaceVariableID">the recently saved workspace variable ID</param>
        private void InsertSumOfBoeByWorkspace(WorkspaceVariableDTO inWorkspaceVariable, int inSavedWorkspaceVariableID)
        {
            if (inWorkspaceVariable == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceVariable));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    foreach (SelectBOEsToSum boeToSum in inWorkspaceVariable.SelectedBOEsToSum)
                    {
                        int? pkid = gbe.insertSumOfBOE_WorkspaceVariable(inSavedWorkspaceVariableID, boeToSum.CLINID, boeToSum.WBSID, boeToSum.BoeID).FirstOrDefault();

                        if (pkid.HasValue)
                        {
                            boeToSum.OVSumID = pkid.Value;
                            boeToSum.OrdinaryVariableID = inSavedWorkspaceVariableID;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delete all the workspace variable resource types
        /// </summary>
        /// <param name="inDeleteWorkspaceVarID"></param>
        private void DeleteWorkspaceVarResourceType(int inDeleteWorkspaceVarID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // Delete the resource type by workspace variable ID
                    gbe.deleteWorkspaceVariableResourceType(inDeleteWorkspaceVarID);
                } // end gbe
            }
        }

        /// <summary>
        /// Insert the workspace variable resource type selection
        /// </summary>
        /// <param name="inWorkspaceVariable"></param>
        /// <param name="inSavedWorkspaceVariableID"></param>
        private void InsertWorkspaceVarResourceType(WorkspaceVariableDTO inWorkspaceVariable, int inSavedWorkspaceVariableID)
        {
            if (inWorkspaceVariable == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceVariable));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    foreach (int resourceTypeID in inWorkspaceVariable.SumVariableResourceTypeIDs)
                    {
                        gbe.insertWorkspaceVariableResourceType(inSavedWorkspaceVariableID, resourceTypeID);
                    }

                }
            }
        }

        #endregion Commits
    }
}
