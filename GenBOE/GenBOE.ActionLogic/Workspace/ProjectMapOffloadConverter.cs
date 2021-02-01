// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using DataBridge.DTO;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;
    using Microsoft.Practices.Unity;
    using MoreLinq;

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

            ICollection<OffloadRatesDTO> offloadRates = offloadRatesLoader.GetByWorkspaceId(workspace.Id);
            DateTime discreteStartMonth = new DateTime(workspace.ContractStartDate.Year, 1, 15).Normalize();

            ICollection<FullBoe> boesToConvert = doOffload ? offloadClass.OffloadWorkspace(workspace.Boes.ToList(), workspace).Boes.ToList() : workspace.Boes.ToList();
            List<RTECustomTemplateQuestionAnswerModelView> rteOverrides = doOffload ? boesToConvert.SelectMany(x => x.TemplateQuestionsAndAnswers).ToList() : workspace.TemplateQuestionsAndAnswers.ToList();

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

                        string rationale = BuildRationaleFromNewMoqTypes(workspace, task.Id)
                            + RTEUtilities.TurnHTMLIntoPlainText(BOEExportConverter.GetRteOverride(task.BoeID, task.Id, task.MOQText, RteTemplateSource.TaskMOQ, rteOverrides));

                        ProjectMapModelView dto = new ProjectMapModelView()
                        {
                            ActivityID = activityId,
                            ActivityName = activityName,
                            AddDelete = laborResource.AddOrDelete ?? "A",
                            CamName = boe.CamName ?? task.CamName,
                            Category = boe.Category ?? task.Category,
                            ClassOfCost = (boe.ClassOfCost == ClassOfCost.None ? task.ClassOfCost : boe.ClassOfCost).GetDescription(),
                            Clin = clin?.ClinNumber ?? string.Empty,
                            CostCenter = perfOrg?.PerformingOrgName ?? string.Empty,
                            Dollars = laborResource.SpreadType == SpreadType.Cost ? laborResource.ValueSpread : null,
                            EndDate = laborResource.EndDate,
                            Hours = laborResource.SpreadType == SpreadType.Hours ? laborResource.ValueSpread : null,
                            InitialResource = resource?.ResourceName ?? string.Empty,
                            LegacyID = laborResource.LegacyID,
                            Rationale = rationale,
                            SowNumber = boe.SOW ?? task.SOW,
                            SowTitle = boe.SOWTitle ?? task.SOWTitle,
                            StartDate = laborResource.StartDate,
                            Task = RTEUtilities.TurnHTMLIntoPlainText(BOEExportConverter.GetRteOverride(task.BoeID, task.Id, task.Description, RteTemplateSource.TaskDescription, rteOverrides)),
                            WbsElementTitle = wbs?.WbsTitle ?? string.Empty,
                            WbsNumber = wbs?.WbsNumber ?? string.Empty,
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
        /// Builds a string for the Rationale field. This is needed for Excel Export
        /// </summary>
        /// <param name="workspace">Ws</param>
        /// <param name="taskId">Task Id</param>
        /// <returns>Rationale String</returns>
        private static string BuildRationaleFromNewMoqTypes(FullWorkspace workspace, int taskId)
        {
            _ = workspace ?? throw new ArgumentNullException(nameof(workspace));

            string result = string.Empty;

            if (workspace.UsingTemplateBOE)
            {
                MoqTypeTableDataLabels labels = new MoqTypeTableDataLabels();
                StringBuilder sb = new StringBuilder();

                workspace.MoqTypeSelections.Where(x => x.TaskId == taskId).OrderBy(x => x.Order).ForEach(moqType =>
                {
                    sb.AppendLine($"{moqType.SelectedMOQType.GetDescription()}:");

                    moqType.TableData.ForEach(table => 
                    {
                        sb.AppendLine($"{labels.TableName}: {table.TableName}");
                        sb.AppendLine($"{labels.DateOfReport}: {table.DateOfReport.ToShortDateString()}");
                        sb.AppendLine($"{labels.HistoricalProgramName}: {table.HistoricalProgramName}");
                        sb.AppendLine($"{labels.ContractNumber}: {table.ContractNumber}");
                        sb.AppendLine($"{labels.WbsElement}: {table.WbsElement}");
                        sb.AppendLine($"{labels.PoPStart}: {table.PoPStartString}");
                        sb.AppendLine($"{labels.PoPEnd}: {table.PoPEndString}");
                        sb.AppendLine($"{labels.TotalWbsHours}: {table.TotalWbsHours.ToString(Constants.DECIMAL_FORMATTING)}");
                        sb.AppendLine($"{labels.AdditionalQueryFilters}: {table.AdditionalQueryFilters}");
                        sb.AppendLine($"{labels.TotalRelevantHours}: {table.TotalRelevantHours.ToString(Constants.DECIMAL_FORMATTING)}");
                        sb.AppendLine();
                    });

                    #region RTE fields
                    if (moqType.SelectedMOQType == MOQType.CostEstimatingRelationships)
                    {
                        sb.AppendLine($"CER name: {RTEUtilities.TurnHTMLIntoPlainText(moqType.CerName)}");
                    } 
                    else if (moqType.SelectedMOQType == MOQType.ParametricEstimates)
                    {
                        sb.AppendLine($"Parametric model or tool name: {RTEUtilities.TurnHTMLIntoPlainText(moqType.CerName)}");
                    }
                    else if (moqType.SelectedMOQType == MOQType.AnalogousRelationships)
                    {
                        sb.AppendLine($"Analogous relationship name: {RTEUtilities.TurnHTMLIntoPlainText(moqType.CerName)}");
                    }

                    if (moqType.SelectedMOQType == MOQType.LOE)
                    {
                        sb.AppendLine($"Description of Hours required: {RTEUtilities.TurnHTMLIntoPlainText(moqType.DescriptionHoursRequired)}");
                    }
                    else if (moqType.SelectedMOQType == MOQType.SOW)
                    {
                        sb.AppendLine($"Description of Hours required & location in SOW: {RTEUtilities.TurnHTMLIntoPlainText(moqType.DescriptionHoursRequired)}");
                    }

                    if (moqType.SelectedMOQType == MOQType.SME)
                    {
                        sb.AppendLine($"The SME selected Expert judgement for this basis of estimate for the following reasons: {RTEUtilities.TurnHTMLIntoPlainText(moqType.SmeReason)}");
                        sb.AppendLine($"The logic and assumptions used to estimate hours is: {RTEUtilities.TurnHTMLIntoPlainText(moqType.SmeHoursLogic)}");
                        sb.AppendLine($"The logic and assumptions used to estimate duration is: {RTEUtilities.TurnHTMLIntoPlainText(moqType.SmeDurationLogic)}");
                        sb.AppendLine($"The following tasks are estimates in this BOE: {RTEUtilities.TurnHTMLIntoPlainText(moqType.SmeTaskEstimates)}");
                    }

                    if (moqType.SelectedMOQType != MOQType.SME)
                    {
                        sb.AppendLine($"Rationale: {RTEUtilities.TurnHTMLIntoPlainText(moqType.Rationale)}");
                    }

                    if (moqType.SelectedMOQType != MOQType.NonLabor)
                    {
                        sb.AppendLine($"Skill Mix Rationale: {RTEUtilities.TurnHTMLIntoPlainText(moqType.SkillMixRationale)}");
                    }
                    #endregion

                    sb.AppendLine();
                });

                result = sb.ToString();
            }

            return result;
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