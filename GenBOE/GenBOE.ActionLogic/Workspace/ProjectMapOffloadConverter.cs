// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using DataBridge.DTO;
    using GenBOE.ActionLogic.Common;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Converts a set of Project Map ModelViews from a Workspace.
    /// </summary>
    public static class ProjectMapOffloadConverter
    {
        /// <summary>
        /// Offload class to help w/ getting the data offloaded.
        /// </summary>
        private static OffloadLaborRates offloadClass = new OffloadLaborRates();

        /// <summary>
        /// The offload rates loader used to get the offload rates.
        /// </summary>
        private static IOffloadRatesDTOLoader offloadRatesLoader = GenBOEUnityContainer.Container.Resolve(typeof(IOffloadRatesDTOLoader)) as IOffloadRatesDTOLoader;

        /// <summary>
        /// Converts the workspace data into project map data.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="doOffload">Do we need to offload first?</param>
        /// <returns>A collection of project map data.</returns>
        public static IReadOnlyCollection<ProjectMapModelView> ConvertToProjectMap(FullWorkspace workspace, bool doOffload, bool checkForOffloadWarnings)
        {
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            Collection<ProjectMapModelView> projectMapData = new Collection<ProjectMapModelView>();

            ICollection<FullBoe> boesToConvert = doOffload ? offloadClass.OffloadWorkspace(workspace.Boes.ToList(), workspace).Boes.ToList() : workspace.Boes.ToList();
            ICollection<OffloadRatesDTO> offloadRates = offloadRatesLoader.GetByWorkspaceId(workspace.Id);

            DateTime discreteStartMonth = new DateTime(workspace.ContractStartDate.Year, 1, 15).Normalize();

            foreach (FullBoe boe in boesToConvert)
            {
                foreach (BoeTaskElementDTO task in boe.TaskElements)
                {
                    foreach (ResourceTypeDto laborResource in task.taskElementLabors)
                    {
                        PerformingOrgDTO perfOrg = workspace.PerformingOrgsForWsList.FirstOrDefault(p => p.Id == laborResource.PerformingOrgID);
                        ResourceDTO resource = workspace.ResourcesForWsResourceListId.FirstOrDefault(r => r.Id == laborResource.ResourceID);
                        WbsDTO wbs = workspace.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                        FullClin clin = workspace.Clins.FirstOrDefault(x => x.Id == boe.CLINID);

                        // Project map -> activity is fed from BOE. Standard -> fed from task. (facepalm)
                        string activityId = workspace.IsProjectMapWorkspace ? boe.Title : task.BOETaskID;
                        string activityName = workspace.IsProjectMapWorkspace ? boe.Description : task.TaskTitle;

                        ProjectMapModelView dto = new ProjectMapModelView()
                        {
                            ActivityID = activityId,
                            ActivityName = activityName,
                            AddDelete = laborResource.AddOrDelete ?? "A",
                            CamName = boe.CamName,
                            Category = boe.Category,
                            ClassOfCost = boe.ClassOfCost.GetDescription(),
                            Clin = (clin == null) ? string.Empty : clin.ClinNumber,
                            CostCenter = (perfOrg == null) ? string.Empty : perfOrg.PerformingOrgName,
                            Dollars = laborResource.SpreadType == SpreadType.Cost ? laborResource.ValueSpread : null,
                            EndDate = laborResource.EndDate,
                            Hours = laborResource.SpreadType == SpreadType.Hours ? laborResource.ValueSpread : null,
                            InitialResource = (resource == null) ? string.Empty : resource.ResourceName,
                            LegacyID = laborResource.LegacyID,
                            Rationale = task.MOQText,
                            SowNumber = boe.SOW,
                            SowTitle = boe.SOWTitle,
                            StartDate = laborResource.StartDate,
                            Task = task.Description,
                            WbsElementTitle = (wbs == null) ? string.Empty : wbs.WbsTitle,
                            WbsNumber = (wbs == null) ? string.Empty : wbs.WbsNumber,
                            Offload = laborResource.CanOffload,
                            TieredPercentage = laborResource.TieredPercentage
                        };

                        if (workspace.ProjectMapType == ProjectMapType.TimePhasedProjectMap)
                        {
                            dto.DiscreteMonths = new decimal?[204];
                            DateTime currentMonth = discreteStartMonth;
                            for (int i = 0; i < 204; i++)
                            {
                                ResourceSpreadDto spread = laborResource.LaborSpreads.FirstOrDefault(s => s.LaborSpreadDate == currentMonth);
                                decimal? monthValue = null;
                                if (spread != null)
                                {
                                    monthValue = spread.LaborSpreadValue;
                                }

                                dto.DiscreteMonths[i] = monthValue;
                                currentMonth = currentMonth.AddMonths(1);
                            }
                        }

                        // Add offload warnings only if we are not doing offload and only when showing to the grid
                        if (checkForOffloadWarnings && !doOffload)
                        {
                            if (dto.Offload)
                            {
                                OffloadLaborRatesValidationResults results = OffloadLaborRates.ValidateLaborResourceCanOffload(laborResource, offloadRates, workspace);
                                dto.OffloadWarning = !results.IsValid;
                                dto.OffloadWarningText = results.InvalidWarningText;
                            }
                        }

                        projectMapData.Add(dto);
                    }
                }
            }

            return projectMapData.ToList().AsReadOnly();
        }

        /// <summary>
        /// Adds the offload warnings to project map data.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void AddOffloadWarnings(FullWorkspace workspace, IReadOnlyCollection<ProjectMapModelView> projectMapData)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (projectMapData == null)
            {
                throw new ArgumentNullException(nameof(projectMapData));
            }

            ICollection<OffloadRatesDTO> offloadRates = offloadRatesLoader.GetByWorkspaceId(workspace.Id);
            Dictionary<string, ResourceDTO> resourcesByUppercaseName = workspace.ResourcesForWsResourceListId.ToDictionary(r => r.ResourceName.ToUpper(), re => re);
            Dictionary<string, PerformingOrgDTO> performingOrgByUppercaseName = workspace.PerformingOrgsForWsList.ToDictionary(p => p.PerformingOrgName.ToUpper(), po => po);

            foreach (ProjectMapModelView model in projectMapData)
            {
                if (model.Offload)
                {
                    ResourceTypeDto laborResource = new ResourceTypeDto()
                    {
                        CanOffload = model.Offload,
                        TieredPercentage = model.TieredPercentage,
                        EndDateValue = model.EndDate.Value,
                        StartDateValue = model.StartDate.Value,
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

                    OffloadLaborRatesValidationResults results = OffloadLaborRates.ValidateLaborResourceCanOffload(laborResource, offloadRates, workspace);
                    model.OffloadWarning = !results.IsValid;
                    model.OffloadWarningText = results.InvalidWarningText;
                }
            }
        }
    }
}