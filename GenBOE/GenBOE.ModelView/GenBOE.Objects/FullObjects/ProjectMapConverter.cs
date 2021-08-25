// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dtos;
    using GenBOE.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Converts a set of Project Map ModelViews to/from a Workspace.
    /// </summary>
    public static class ProjectMapConverter
    {
        /// <summary>
        /// Converts the project map data into workspace-type data.
        /// </summary>
        /// <param name="projectMapData">The project map data.</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>A DTO containing the converted data.</returns>
        public static ConvertedProjectMapDTO ConvertToWorkspace(IReadOnlyCollection<ProjectMapModelView> projectMapData, FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (ReferenceEquals(projectMapData, null))
            {
                throw new ArgumentNullException(nameof(projectMapData));
            }

            ConvertedProjectMapDTO convertedDto = new ConvertedProjectMapDTO();

            if (projectMapData.Any())
            {
                convertedDto.EarliestStart = projectMapData.Where(d => d.StartDate.HasValue).Select(d => d.StartDate.Value).Min().Normalize();
                convertedDto.LatestEnd = projectMapData.Where(d => d.EndDate.HasValue).Select(d => d.EndDate.Value).Max().Normalize();

                DateTime discreteStartMonth = new DateTime(convertedDto.EarliestStart.Year, 1, 15).Normalize();

                // Keep track of ids so that we connect the objects correctly
                // Note: taskId and resourceId will automatically propagate down during a bulk save
                int clinId = -1;
                int wbsId = -1;
                int boeId = -100000;
                int taskId = -100000;
                int resourceId = -100000;

                Dictionary<string, ResourceDTO> resourcesByUppercaseName = workspace.ResourcesForWsResourceListId.ToDictionary(r => r.ResourceName.ToUpper(), re => re);
                Dictionary<string, PerformingOrgDTO> performingOrgByUppercaseName = workspace.PerformingOrgsForWsList.ToDictionary(p => p.PerformingOrgName.ToUpper(), po => po);
                Dictionary<string, ClassOfCost> classOfCostByDescription = Enum.GetValues(typeof(ClassOfCost)).Cast<ClassOfCost>().ToDictionary(t => t.GetDescription(), t => t); 

                // group by tiers
                Dictionary<string, ICollection<ProjectMapModelView>> tieredData = GroupByTiers(projectMapData);
                foreach (ICollection<ProjectMapModelView> models in tieredData.Values)
                {
                    ProjectMapModelView firstModel = models.First();
                    ClassOfCost classOfCost;
                    if (firstModel.ClassOfCost == null || !classOfCostByDescription.TryGetValue(firstModel.ClassOfCost, out classOfCost))
                    {
                        classOfCost = ClassOfCost.None;
                    }
                    FullBoe boe = new FullBoe
                    {
                        Id = boeId--,
                        CamName = firstModel.CamName,
                        Category = firstModel.Category,
                        ClassOfCost = classOfCost,
                        Description = firstModel.ActivityName,
                        Title = firstModel.ActivityID,
                        SOW = firstModel.SowNumber,
                        SOWTitle = firstModel.SowTitle,
                        Updateable = UpdateType.Upsert,
                        WorkspaceID = workspace.Id,
                        StartDate = models.Select(m => m.StartDate).Where(d => d.HasValue).Min().Value,
                        EndDate = models.Select(m => m.EndDate).Where(d => d.HasValue).Max().Value
                    };

                    if (firstModel.Clin != null)
                    {
                        FullClin clin = convertedDto.Clins.FirstOrDefault(c => c.ClinNumber == firstModel.Clin);
                        if (clin == null)
                        {
                            clin = new FullClin()
                            {
                                Id = clinId--,
                                ClinTitle = firstModel.Clin,
                                ClinNumber = firstModel.Clin,
                                WorkspaceID = workspace.Id,
                                Updateable = UpdateType.Upsert
                            };
                            convertedDto.Clins.Add(clin);
                        }

                        boe.CLINID = clin.Id;
                    }

                    if (firstModel.WbsNumber != null && firstModel.WbsElementTitle != null)
                    {
                        FullWbs wbs = convertedDto.Wbs.FirstOrDefault(w => w.WbsNumber == firstModel.WbsNumber && w.WbsTitle == firstModel.WbsElementTitle);
                        if (wbs == null)
                        {
                            wbs = new FullWbs()
                            {
                                Id = wbsId--,
                                WbsTitle = firstModel.WbsElementTitle,
                                WbsNumber = firstModel.WbsNumber,
                                WorkspaceID = workspace.Id,
                                Updateable = UpdateType.Upsert
                            };
                            convertedDto.Wbs.Add(wbs);
                        }

                        boe.WBSID = wbs.Id;
                    }

                    convertedDto.Boes.Add(boe);

                    BoeTaskElementDTO task = new BoeTaskElementDTO()
                    {
                        BoeID = boe.Id,
                        Description = firstModel.Task,
                        StartDate = boe.StartDate,
                        EndDate = boe.EndDate,
                        Updateable = UpdateType.Upsert,
                        TotalCost = models.Where(m => m.Dollars.HasValue).Select(m => m.Dollars).Sum(),
                        TotalHours = models.Where(m => m.Hours.HasValue).Select(m => m.Hours).Sum(),
                        Id = taskId--,
                        MOQText = firstModel.Rationale
                    };

                    convertedDto.Tasks.Add(task);

                    boe.SetTaskElements(new List<BoeTaskElementDTO> { task });

                    foreach (ProjectMapModelView model in models)
                    {
                        ResourceTypeDto resource = new ResourceTypeDto()
                        {
                            Id = resourceId--,
                            BoeID = boe.Id,
                            CanOffload = model.Offload,
                            TieredPercentage = model.TieredPercentage,
                            EndDateValue = model.EndDate.Value,
                            StartDateValue = model.StartDate.Value,
                            TaskElementId = task.Id,
                            LegacyID = model.LegacyID,
                            PerformingOrgID = performingOrgByUppercaseName[model.CostCenter.ToUpper()].Id,
                            ResourceID = resourcesByUppercaseName[model.InitialResource.ToUpper()].Id,
                            SpreadCurveID = workspace.ProjectMapType == ProjectMapType.TimePhasedProjectMap ? SpreadCurves.DiscreteHours : SpreadCurves.Level,
                            SpreadType = model.Dollars.HasValue && model.Dollars.Value != 0m ? SpreadType.Cost : SpreadType.Hours,
                            Updateable = UpdateType.Upsert,
                            ValueSpread = model.Dollars.HasValue && model.Dollars.Value != 0m ? model.Dollars : model.Hours,
                            AddOrDelete = model.AddDelete,
                            HourSpreadLocked = true,
                            ProjectMapId = model.Id
                        };

                        if (workspace.ProjectMapType == ProjectMapType.TimePhasedProjectMap)
                        {
                            CreateLaborSpreads(resource, model, discreteStartMonth, boe.Id);
                        }
                        else
                        {
                            int decimalPrecision = resource.SpreadType == SpreadType.Cost ? workspace.CostDecimalPrecision : workspace.DecimalPrecision;
                            CreateLaborSpreadsLevelLoaded(resource, decimalPrecision);
                        }

                        task.taskElementLabors.Add(resource);
                    }
                }
            }

            return convertedDto;
        }

        /// <summary>
        /// Creates the labor spreads as level loaded.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="decimalPlaces">The decimal places.</param>
        private static void CreateLaborSpreadsLevelLoaded(ResourceTypeDto resource, int decimalPlaces)
        {
            if (resource.StartDate.HasValue && resource.EndDate.HasValue && resource.ValueSpread.HasValue)
            {
                // SpreadCurve3 is actually SpreadCurves.Level
                LaborSpreadRequest request = new LaborSpreadRequest(SpreadCurves.SpreadCurve3, resource.StartDateValue, resource.EndDateValue, resource.ValueSpread.Value);
                resource.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(request, decimalPlaces);

                resource.LaborSpreads.ToList().ForEach(x => x.Updateable = UpdateType.Upsert);
            }
        }

        /// <summary>
        /// Groups the project map data rows by tiers.
        /// </summary>
        /// <param name="projectMapData">The project map data.</param>
        /// <returns>Dictionary of project map data grouped by tier.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Dictionary<string, ICollection<ProjectMapModelView>> GroupByTiers(IReadOnlyCollection<ProjectMapModelView> projectMapData)
        {
            if (ReferenceEquals(projectMapData, null))
            {
                throw new ArgumentNullException(nameof(projectMapData));
            }

            Dictionary<string, ICollection<ProjectMapModelView>> tiers = new Dictionary<string, ICollection<ProjectMapModelView>>();
            foreach (ProjectMapModelView model in projectMapData)
            {
                string tier = $"{model.ActivityID}:{model.ActivityName}:{model.WbsElementTitle}:{model.WbsNumber}:{model.Clin}:{model.SowNumber}:{model.SowTitle}:{model.Task}";

                if (!tiers.ContainsKey(tier))
                {
                    tiers.Add(tier, new List<ProjectMapModelView>());
                }

                tiers[tier].Add(model);
            }

            return tiers;
        }

        /// <summary>
        /// Creates the labor spreads.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="model">The model.</param>
        /// <param name="startMonth">The start month.</param>
        /// <param name="boeId">The boe identifier.</param>
        private static void CreateLaborSpreads(ResourceTypeDto resource, ProjectMapModelView model, DateTime startMonth, int boeId)
        {
            if (model.DiscreteMonths != null && model.DiscreteMonths.Any())
            {
                DateTime currentMonth = startMonth;
                foreach (decimal? discreteValue in model.DiscreteMonths)
                {
                    if (currentMonth >= resource.StartDate)
                    {
                        if (currentMonth > resource.EndDate)
                        {
                            // we're past the range for the resource, kick out
                            break;
                        }

                        resource.LaborSpreads.Add(
                            new ResourceSpreadDto()
                            {
                                Updateable = UpdateType.Upsert,
                                BoeID = boeId,
                                LaborSpreadDate = currentMonth,
                                LaborSpreadValue = discreteValue ?? 0m,
                                LaborTypeId = resource.Id
                            });
                    }

                    currentMonth = currentMonth.AddMonths(1);
                }
            }
        }
    }
}