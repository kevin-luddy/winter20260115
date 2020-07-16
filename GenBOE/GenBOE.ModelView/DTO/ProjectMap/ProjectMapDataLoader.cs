// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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

    public class ProjectMapDataLoader : BulkDataLoader<ProjectMapModelView, ProjectMap>, IProjectMapDataLoader
    {

        /// <summary>
        /// The page size when paging.
        /// </summary>
        private readonly int pageSize = ConfigurationUtilities.GetAppSetting<int>("ProjectMapPageSize");

        /// <summary>
        /// The project map spread loader
        /// </summary>
        private IProjectMapSpreadLoader projectMapSpreadLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectMapSpreadLoader">The project map spread loader.</param>
        public ProjectMapDataLoader(IProjectMapSpreadLoader projectMapSpreadLoader)
        {
            this.Log = new Logger(typeof(ProjectMapDataLoader));
            this.projectMapSpreadLoader = projectMapSpreadLoader;
        }

        #region Multi DbQuery Methods

        /// <summary>
        /// Gets the by ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns>ProjectMapModelViews for the specified Ids</returns>
        public override ICollection<ProjectMapModelView> GetByIds(ICollection<int> ids)
        {
            // The ProjectMapModelView to return
            List<ProjectMapModelView> toReturn;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // get the ProjectMap data from the sprocResults
                    toReturn = (from pm in gbe.ProjectMaps
                                where ids.Contains(pm.ID)
                                orderby pm.ID
                                select new ProjectMapModelView
                                {
                                    ActivityID = pm.ActivityID,
                                    ActivityName = pm.ActivityName,
                                    AddDelete = pm.AddOrDelete,
                                    CamName = pm.CamName,
                                    Category = pm.Category,
                                    ClassOfCost = pm.ClassOfCost,
                                    TieredPercentage = pm.TieredPercentage,
                                    Clin = pm.CLIN,
                                    CostCenter = pm.CostCenter,
                                    Dollars = pm.Dollars,
                                    EndDate = pm.EndDate,
                                    Hours = pm.Hours,
                                    InitialResource = pm.Resource,
                                    Offload = pm.CanOffload,
                                    LegacyID = pm.LegacyResourceID,
                                    LegacyResourceID = pm.SikorskyLegacyResource.ResourceID,
                                    Rationale = pm.Rationale,
                                    SowNumber = pm.SOW,
                                    SowTitle = pm.SOWTitle,
                                    StartDate = pm.StartDate,
                                    Task = pm.Task,
                                    WbsElementTitle = pm.WbsElementTitle,
                                    WbsNumber = pm.WbsNumber,
                                    WorkspaceId = pm.WorkspaceId,
                                    Id = pm.ID
                                }).ToList();

                    int? workspaceId = toReturn.FirstOrDefault()?.WorkspaceId;
                    List<ProjectMapSpread> spreads = new List<ProjectMapSpread>();
                    DateTime discreteStartMonth = toReturn.Any() ? new DateTime(toReturn.Min(m => m.StartDate.Value).Year, 1, 15).Normalize() : DateTime.MaxValue;
                    if (workspaceId.HasValue)
                    {
                        spreads = gbe.ProjectMapSpreads.Where(s => s.WorkspaceId == workspaceId).ToList();
                        

                        spreads.AsParallel().ForAll(
                            x =>
                            {
                                x.SpreadDate = x.SpreadDate.Normalize();
                            }
                        );
                    }

                    toReturn.AsParallel().ForAll(
                            x =>
                            {
                                x.StartDate = x.StartDate.Normalize();
                                x.EndDate = x.EndDate.Normalize();
                                if (spreads.Any())
                                {
                                    x.DiscreteMonths = CreateSpreadArray(spreads.Where(s => s.ProjectMapId == x.Id).ToList(), discreteStartMonth);
                                }
                            }
                        );
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets all ProjectMapModelViews for the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <returns>ProjectMapModelViews for the specified WS</returns>
        [DbQuery(2)]
        public ICollection<ProjectMapModelView> GetByWorkspaceId(int workspaceId)
        {
            // The ProjectMapModelView to return
            List<ProjectMapModelView> toReturn;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

                    // get the basic Project Map data from the sprocResults
                    toReturn = (from pm in gbe.ProjectMaps
                                where pm.WorkspaceId == workspaceId
                                orderby pm.ID
                                select new ProjectMapModelView
                                {
                                    ActivityID = pm.ActivityID,
                                    ActivityName = pm.ActivityName,
                                    AddDelete = pm.AddOrDelete,
                                    CamName = pm.CamName,
                                    Category = pm.Category,
                                    ClassOfCost = pm.ClassOfCost,
                                    TieredPercentage = pm.TieredPercentage,
                                    Clin = pm.CLIN,
                                    CostCenter = pm.CostCenter, 
                                    Dollars = pm.Dollars,
                                    EndDate = pm.EndDate,
                                    Hours = pm.Hours,
                                    InitialResource = pm.Resource,
                                    Offload = pm.CanOffload,
                                    LegacyID = pm.LegacyResourceID,
                                    LegacyResourceID = pm.SikorskyLegacyResource.ResourceID,
                                    Rationale = pm.Rationale,
                                    SowNumber = pm.SOW,
                                    SowTitle = pm.SOWTitle,
                                    StartDate = pm.StartDate,
                                    Task = pm.Task,
                                    WbsElementTitle = pm.WbsElementTitle,
                                    WbsNumber = pm.WbsNumber, 
                                    WorkspaceId = pm.WorkspaceId,
                                    Id = pm.ID                                   
                                }).ToList();
                    List<ProjectMapSpread> spreads = gbe.ProjectMapSpreads.Where(s => s.WorkspaceId == workspaceId).ToList();

                    DateTime discreteStartMonth = toReturn.Any() ? new DateTime(toReturn.Min(m => m.StartDate.Value).Year, 1, 15).Normalize() : DateTime.MaxValue;

                    spreads.AsParallel().ForAll(
                        x =>
                        {
                            x.SpreadDate = x.SpreadDate.Normalize();
                        }
                    );

                    toReturn.AsParallel().ForAll(
                            x =>
                            {
                                x.StartDate = x.StartDate.Normalize();
                                x.EndDate = x.EndDate.Normalize();
                                if (spreads.Any())
                                {
                                    x.DiscreteMonths = CreateSpreadArray(spreads.Where(s => s.ProjectMapId == x.Id).ToList(), discreteStartMonth);
                                }
                            }
                        );
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the paged data.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="page">The page.</param>
        /// <returns>A page of data for the front-end.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public ProjectMapPageModelView GetProjectMapPagedData(int workspaceId, int page)
        {
            if (page < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page), page, "page must not be a negative number or zero.");
            }

            // The ProjectMapPageModelView to return
            ProjectMapPageModelView toReturn = new ProjectMapPageModelView();
            int numSkipped = pageSize * (page - 1);

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                List<ProjectMapSpread> spreads;
                DateTime discreteStartMonth;
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // get the basic Project Map data from the sprocResults
                    toReturn.Data = gbe.ProjectMaps.Where(pm => pm.WorkspaceId == workspaceId).OrderBy(p => p.OrderID).Skip(numSkipped).Take(pageSize).Select<ProjectMap, ProjectMapModelView>(
                        pm => new ProjectMapModelView
                                {
                                    ActivityID = pm.ActivityID,
                                    ActivityName = pm.ActivityName,
                                    AddDelete = pm.AddOrDelete,
                                    CamName = pm.CamName,
                                    Category = pm.Category,
                                    ClassOfCost = pm.ClassOfCost,
                                    TieredPercentage = pm.TieredPercentage,
                                    Clin = pm.CLIN,
                                    CostCenter = pm.CostCenter,
                                    Dollars = pm.Dollars,
                                    EndDate = pm.EndDate,
                                    Hours = pm.Hours,
                                    InitialResource = pm.Resource,
                                    Offload = pm.CanOffload,
                                    LegacyID = pm.LegacyResourceID,
                                    LegacyResourceID = pm.SikorskyLegacyResource.ResourceID,
                                    Rationale = pm.Rationale,
                                    SowNumber = pm.SOW,
                                    SowTitle = pm.SOWTitle,
                                    StartDate = pm.StartDate,
                                    Task = pm.Task,
                                    WbsElementTitle = pm.WbsElementTitle,
                                    WbsNumber = pm.WbsNumber,
                                    WorkspaceId = pm.WorkspaceId,
                                    Id = pm.ID
                                }).ToList();

                    IEnumerable<int> Ids = toReturn.Data.Select(p => p.Id);

                    spreads = gbe.ProjectMapSpreads.Where(s => s.WorkspaceId == workspaceId && Ids.Contains(s.ProjectMapId)).ToList();

                    discreteStartMonth = new DateTime(gbe.Workspaces.Where(w => w.WorkspaceID == workspaceId).First().ContractStartDate.Year, 1, 15).Normalize();

                    var sums = gbe.ProjectMaps
                         .Where(pm => pm.WorkspaceId == workspaceId)
                         .GroupBy(pm => pm.WorkspaceId)
                         .Select(g =>
                             new
                             {
                                 Hours = g.Sum(s => s.Hours),
                                 Dollars = g.Sum(s => s.Dollars),
                                 Rows = g.Count()
                             }).FirstOrDefault();

                    if (sums != null)
                    {
                        toReturn.TotalRows = sums.Rows;
                        toReturn.TotalDollars = sums.Dollars ?? 0m;
                        toReturn.TotalHours = sums.Hours ?? 0m;
                    }
                }

                spreads.AsParallel().ForAll(
                                        x =>
                                        {
                                            x.SpreadDate = x.SpreadDate.Normalize();
                                        }
                                    );

                toReturn.Data.AsParallel().ForAll(
                            x =>
                            {
                                x.StartDate = x.StartDate.Normalize();
                                x.EndDate = x.EndDate.Normalize();
                                if (spreads.Any())
                                {
                                    x.DiscreteMonths = CreateSpreadArray(spreads.Where(s => s.ProjectMapId == x.Id).ToList(), discreteStartMonth);
                                }
                            }
                        );
            }

            return toReturn;
        }

        /// <summary>
        /// Creates the spread array.
        /// </summary>
        /// <param name="spreadList">The spread list.</param>
        /// <param name="discreteStartMonth">The discrete start month.</param>
        /// <returns>A 17-year list of spreads</returns>
        private decimal?[] CreateSpreadArray(List<ProjectMapSpread> spreadList, DateTime discreteStartMonth)
        {
            decimal?[] discreteMonths = new decimal?[204];
            DateTime currentMonth = discreteStartMonth;
            for (int i = 0; i < 204; i++)
            {
                ProjectMapSpread spread = spreadList.FirstOrDefault(s => s.SpreadDate == currentMonth);
                decimal? monthValue = null;
                if (spread != null)
                {
                    monthValue = spread.SpreadValue;
                }

                discreteMonths[i] = monthValue;
                currentMonth = currentMonth.AddMonths(1);
            }

            return discreteMonths;
        }

        #endregion

        /// <summary>
        /// Converts the dto into it's associated TEntityType.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>
        /// TEntityType
        /// </returns>
        protected override ProjectMap ConvertDtoToEntity(ProjectMapModelView dtoToConvert)
        {
            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }
            
            return new ProjectMap
            {
                ActivityID = dtoToConvert.ActivityID,
                ActivityName = dtoToConvert.ActivityName,
                AddOrDelete = dtoToConvert.AddDelete,
                CamName = dtoToConvert.CamName,
                CanOffload = dtoToConvert.Offload,
                Category = dtoToConvert.Category,
                ClassOfCost = dtoToConvert.ClassOfCost,
                TieredPercentage = dtoToConvert.TieredPercentage,
                CLIN = dtoToConvert.Clin,
                CostCenter = dtoToConvert.CostCenter,
                Dollars = dtoToConvert.Dollars,
                EndDate = dtoToConvert.EndDate.Value,
                Hours = dtoToConvert.Hours,
                ID = dtoToConvert.Id,
                LegacyResourceID = dtoToConvert.LegacyID,
                Rationale = dtoToConvert.Rationale,
                Resource = dtoToConvert.InitialResource,
                SOW = dtoToConvert.SowNumber,
                SOWTitle = dtoToConvert.SowTitle,
                StartDate = dtoToConvert.StartDate.Value,
                Task = dtoToConvert.Task,
                WbsElementTitle = dtoToConvert.WbsElementTitle,
                WbsNumber = dtoToConvert.WbsNumber,
                WorkspaceId = dtoToConvert.WorkspaceId
            };
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for ProjectMapModelViews
        /// </summary>
        /// <returns>Meta data required for bulk save processing</returns>
        public override BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.BOE_DB_CONTEXT_NAME);

            metaData.BulkInsertStoredProcedureName = "insertProjectMapviaTableParameter";
            
            metaData.BulkInsertStoredProcedureReturnsUpdateDate = false;
            metaData.BulkUpdateStoredProcedureReturnsUpdateDate = false;

            // This is the table type defined in the database for the input arg to the delete, insert, and update stored procedures
            metaData.DBTableTypeName = "TT_ProjectMap";

            // Name of the stored procedure argument for all 3 bulk stored procedures
            metaData.StoredProcedureTableTypeParameterName = "@ProjectMap";

            // The order here matters. It must match exactly the order of the TT_ProjectMap in the database.
            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "WorkspaceId", "WbsNumber", "WbsElementTitle", "ActivityID", "ActivityName", "Resource", "CostCenter", "StartDate",
                "EndDate", "CLIN", "Task", "SOW", "SOWTitle", "Rationale", "CamName", "Category", "Hours", "Dollars", "CanOffload", "AddOrDelete", "ClassOfCost", "TieredPercentage", "LegacyResourceID"
            };

            return metaData;
        }

        /// <summary>
        /// Performs a bulk save of the ProjectMapModelViews.
        /// </summary>
        /// <param name="dtosToSave">the set of dtos to bulk save</param>
        /// <returns>Dictionary where the key is the old dto id and the value is the new id.</returns>
        public override IDictionary<int, int> BulkSave(ICollection<ProjectMapModelView> dtosToSave)
        {
            if (dtosToSave == null) { throw new ArgumentNullException(nameof(dtosToSave)); }
            if (dtosToSave.Any(d => d.Updateable == UpdateType.None)) { throw new ArgumentException("One or more Project map Elements has UpdateType of None.", nameof(dtosToSave)); }

            IDictionary<int, int> toReturn = new Dictionary<int, int>();

            if (dtosToSave.Any())
            {
                if (this.Log.DebugEnabled)
                {
                    foreach (ProjectMapModelView aDto in dtosToSave)
                    {
                        this.Log.Debug($"ProjectMapDataLoader.BulkSave => ActivityId: {aDto.ActivityID}, Cost Center: {aDto.CostCenter}, Activity Type Code: {aDto.InitialResource}");
                    }
                }

                using (StopwatchTimer sw = new StopwatchTimer(this.Log, "Bulk Save Project Map Data"))
                {
                    // Bulk save ProjectMapModelView details
                    toReturn = base.BulkSave(dtosToSave);
                }

                #region Bulk Save Spreads
                using (StopwatchTimer sw = new StopwatchTimer(this.Log, "Bulk Save Project Map Spreads"))
                {
                    DateTime earliestStart = dtosToSave.Where(d => d.StartDate.HasValue).Select(d => d.StartDate.Value).Min().Normalize();
                    DateTime discreteStartMonth = new DateTime(earliestStart.Year, 1, 15).Normalize();
                    ICollection<ProjectMapSpreadModelView> spreads = dtosToSave.SelectMany(d => d.CreateSpreads(discreteStartMonth)).ToList();
                    if (spreads.Any())
                    {
                        this.projectMapSpreadLoader.BulkSave(spreads);
                    }
                }
                #endregion

            }

            return toReturn;
        }

        /// <summary>
        /// Upsert method to be overridden by the derived class
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert.</param>
        /// <returns>
        /// Int representing the id of the upserted item.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override int? Upsert(ProjectMapModelView dtoToUpsert)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete method to be overridden by the derived class.
        /// </summary>
        /// <param name="dtoToDelete">Dto that will be deleted.</param>
        /// <returns>
        /// Id of the deleted object
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override int? Delete(ProjectMapModelView dtoToDelete)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete all data in a WS. Used by project map, for kill / fill
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="updateDate">Last Update Date</param>
        public void DeleteAllInWs(int workspaceId, DateTime updateDate)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbm = new GenBoeEntities())
                {
                    gbm.deleteAllBOEsInWs(workspaceId, updateDate);
                }
            }
        }
    }
}