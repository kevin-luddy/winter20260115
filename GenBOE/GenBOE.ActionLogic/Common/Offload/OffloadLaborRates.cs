// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Offloads labor resources into new subcontractor resources.
    /// </summary>
    public class OffloadLaborRates
    {
        #region Offload text constants

        /// <summary>
        /// The Rationale text for a new Task.
        /// </summary>
        private readonly string NEW_TASK_RATIONALE = "Offload task for Activity {0} {1} Total Est Hrs at historical offload fraction "
            + "{2}  =  {3} Hrs for the period  {4} to {5}  at the escalated Offload Rates per Engineering Offload CER, {6}, {7}"
            + " Reference Offload Yearly Cost Breakdown Report for detailed offload allocation.";

        /// <summary>
        /// The Description text for a new Task.
        /// </summary>
        private readonly string NEW_TASK_DESCRIPTION = "Per the Engineering Offload CER, {0}, Offload task for Activity "
            + "{1} using the historical offload fraction for this resource at the current negotiated offload rate. CER {2}"
            + " Reference Offload Yearly Cost Breakdown Report for detailed offload allocation.";
        
        /// <summary>
        /// The Rationale text to append on an existing Task.
        /// </summary>
        private readonly string EXISTING_TASK_RATIONALE = " Per {0}, reference Offload Yearly Cost Breakdown Report for detailed offload allocation {1}"
            + " Baseline labor estimate decremented for offload: {2} - {3} = {4} Hrs";

        #endregion

        /// <summary>
        /// The logger for offload rates.
        /// </summary>
        private static Logger logger = new Logger(typeof(OffloadLaborRates));

        /// <summary>
        /// The offload rates loader.
        /// </summary>
        private IOffloadRatesDTOLoader offloadRatesLoader;

        /// <summary>
        /// The retriever for Resources/Perf Orgs.
        /// </summary>
        private static IRetriever retriever = GenBOEUnityContainer.Container.Resolve(typeof(IRetriever)) as IRetriever;

        /// <summary>
        /// The full object factory.
        /// </summary>
        private IFullObjectFactory fullObjectFactory;

        /// <summary>
        /// The system settings retriever.
        /// </summary>
        private ISystemSettingDTODataLoader systemSettingLoader;

        /// <summary>
        /// The current boe identifier.
        /// </summary>
        private int currentBoeId;

        /// <summary>
        /// The current task identifier
        /// </summary>
        private int currentTaskId;

        /// <summary>
        /// The current labor identifier
        /// </summary>
        private int currentLaborId;

        /// <summary>
        /// Initializes a new instance of the <see cref="OffloadLaborRates"/> class.
        /// </summary>
        public OffloadLaborRates()
        {
            this.offloadRatesLoader = GenBOEUnityContainer.Container.Resolve(typeof(IOffloadRatesDTOLoader)) as IOffloadRatesDTOLoader;
            this.fullObjectFactory = GenBOEUnityContainer.Container.Resolve(typeof(IFullObjectFactory)) as IFullObjectFactory;
            this.systemSettingLoader = GenBOEUnityContainer.Container.Resolve(typeof(ISystemSettingDTODataLoader)) as ISystemSettingDTODataLoader;
        }

        /// <summary>
        /// Offloads the Workspace labor rates.
        /// </summary>
        /// <param name="boes">The BOEs to offload.</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>The offloaded labor results including original and new offloaded boes.</returns>
        public OffloadLaborRatesResults OffloadWorkspace(ICollection<FullBoe> boes, FullWorkspace workspace)
        {
            if (boes == null)
            {
                throw new ArgumentNullException(nameof(boes));
            }

            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            ICollection<OffloadRatesDTO> offloadRates = this.offloadRatesLoader.GetByWorkspaceId(workspace.Id);

            if (offloadRates == null || offloadRates.None())
            {
                throw new ArgumentException($"No OffloadRates found for Workspace Id: {workspace.Id}.");
            }

            OffloadLaborRatesResults results;

            // reset the ids for any new DTOs
            this.currentBoeId = -1;
            this.currentLaborId = -1;
            this.currentTaskId = -1;

            // create a clone of the workspace so that we do not change anything in any cached workspaces
            if (workspace.IsProjectMapWorkspace)
            {
                FullProjectMapWorkspace projectMapWorkspace = this.fullObjectFactory.CreateClonedProjectMapWorkspace(workspace);
                workspace = projectMapWorkspace;
                boes = this.CreateProjectMapBoeCopies(boes, projectMapWorkspace);
            }
            else
            {
                workspace = this.fullObjectFactory.CreateFullWorkspace(workspace);
                workspace.LoadBoesAndTaskElementsRTEData();
                
                // create copies of the boes
                boes = this.CreateBoeCopies(boes, workspace);
            }

            string justifyingPublication = this.systemSettingLoader.GetSystemSetting(SystemSettingConstants.JUSTIFYING_PUBLICATION)?.Value ?? string.Empty;
            string projectMapOffloadText = this.systemSettingLoader.GetSystemSetting(SystemSettingConstants.PROJECT_MAP_OFFLOAD_TEXT)?.Value ?? string.Empty;

            if (workspace.IsProjectMapWorkspace)
            {
                results = this.OffloadAsProjectMap(boes, workspace, offloadRates, justifyingPublication, projectMapOffloadText);
            }
            else
            {
                // Add the new ResourceTypeDto (inserted after the current ResourceTypeDto that was offloaded)
                results = this.OffloadAsNonProjectMap(boes, workspace, offloadRates, justifyingPublication, projectMapOffloadText);
            }

            return results;
        }

        /// <summary>
        /// Creates the boe copies.
        /// </summary>
        /// <param name="boes">The boes.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>A list of copied boes.</returns>
        private ICollection<FullBoe> CreateBoeCopies(ICollection<FullBoe> boes, FullWorkspace workspace)
        {
            List<FullBoe> clonedBoes = new List<FullBoe>();
            foreach (FullBoe boe in boes)
            {
                FullBoe clonedBoe = this.fullObjectFactory.CreateFullBoe(boe);
                clonedBoe.SetTaskElements(workspace.TaskElements.ToList());
                clonedBoes.Add(clonedBoe);
            }

            return clonedBoes;
        }

        /// <summary>
        /// Creates the project map BOE copies.
        /// </summary>
        /// <param name="boes">The boes.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>A list of copied boes.</returns>
        private ICollection<FullBoe> CreateProjectMapBoeCopies(ICollection<FullBoe> boes, FullProjectMapWorkspace workspace)
        {
            List<FullBoe> clonedBoes = new List<FullBoe>();
            foreach (FullBoe boe in boes)
            {
                // The full projectmap workspace has already been cloned, so use the boe from there instead of reflection to create a new one
                FullBoe clonedBoe = workspace.BoesById[boe.Id];
                clonedBoes.Add(clonedBoe);
            }

            return clonedBoes;
        }

        /// <summary>
        /// Offloads the labor resource type into a sub resource type.
        /// </summary>
        /// <param name="offloadRateByYear">The offload rate in a dictionary keyed by year.</param>
        /// <param name="laborResource">The labor resource type.</param>
        /// <param name="subResource">The sub resource object.</param>
        /// <param name="hoursPrecision">The hours precision.</param>
        /// <param name="costPrecision">The cost precision.</param>
        /// <param name="offloadedHours">The number of offloaded hours.</param>
        /// <returns>A new Sub Resource Type and modifies the original Labor Resource Type.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// offloadRate or laborResource</exception>
        public ResourceTypeDto OffloadRate(Dictionary<int, OffloadRatesDTO> offloadRateByYear, ResourceTypeDto laborResource, ResourceDTO subResource, int hoursPrecision, int costPrecision, out decimal offloadedHours)
        {
            if (offloadRateByYear == null || !offloadRateByYear.Any())
            {
                throw new ArgumentNullException(nameof(offloadRateByYear));
            }

            if (ReferenceEquals(laborResource, null))
            {
                throw new ArgumentNullException(nameof(laborResource));
            }

            if (ReferenceEquals(subResource, null))
            {
                throw new ArgumentNullException(nameof(subResource));
            }

            SubResourceTypeDto subResourceType = new SubResourceTypeDto()
            {
                BoeID = laborResource.BoeID,
                CLINID = laborResource.CLINID,
                EndDateValue = laborResource.EndDateValue,
                PerformingOrgID = laborResource.PerformingOrgID,
                ResourceID = subResource.Id,
                SubResourceName = subResource.ResourceName,
                SpreadType = SpreadType.Cost,
                SpreadCurveID = SpreadCurves.DiscreteCost,
                StartDateValue = laborResource.StartDateValue,
                Updateable = UpdateType.Upsert,
                WBSID = laborResource.WBSID,
                AddOrDelete = laborResource.AddOrDelete,
                CanOffload = false,
                TieredPercentage = laborResource.TieredPercentage,
                CustomFieldValueContainers = laborResource.CustomFieldValueContainers,
                Id = this.currentLaborId--,
                InHouseResource = laborResource.ResourceID,
                InHouseResourceTypeId = laborResource.Id,
                OffLoadedHourSpreads = new Collection<ResourceSpreadDto>(),
                LegacyID = laborResource.LegacyID,
                IsOffloaded = true
            };

            decimal totalSubHours = 0m;
            decimal totalSubCost = 0m;

            // Group Labor spreads by year
            Dictionary<int, ResourceSpreadDto[]> spreadsByYear = this.GroupSpreadsByYear(laborResource.LaborSpreads);

            // Loop over labor spreads by year
            foreach (KeyValuePair<int, ResourceSpreadDto[]> kvp in spreadsByYear)
            {
                // Find the offload rate
                OffloadRatesDTO offloadRateDTO = offloadRateByYear[kvp.Key];

                // Determine the total hours to offload for this year
                decimal totalHoursThisYear = kvp.Value.Sum(ls => ls.LaborSpreadValue);
                decimal totalOffloadHoursThisYear = Utilities.AdjustPrecision(totalHoursThisYear * offloadRateDTO.Percent, hoursPrecision);

                // Find initial distribution, then smooth it into correct numbers
                decimal[] distribution = kvp.Value.Select(ls => ls.LaborSpreadValue * offloadRateDTO.Percent).ToArray();
                distribution = SpreadCurve.Smooth(totalOffloadHoursThisYear, distribution, 0, distribution.Length, hoursPrecision);

                // Calculate the sub cost by month
                for (int i = 0; i < kvp.Value.Length; i++)
                {
                    ResourceSpreadDto spread = kvp.Value[i];
                    decimal subHours = distribution[i];
                    totalSubHours += subHours;
                    spread.LaborSpreadValue -= subHours;
                    spread.Updateable = UpdateType.Upsert;
                    decimal subCost = Utilities.AdjustPrecision(subHours * offloadRateDTO.HourlyRate, costPrecision);
                    totalSubCost += subCost;

                    // Add sub cost spread to sub resource type
                    subResourceType.LaborSpreads.Add(new ResourceSpreadDto()
                    {
                        BoeID = laborResource.BoeID,
                        LaborSpreadDate = spread.LaborSpreadDate,
                        Updateable = UpdateType.Upsert,
                        LaborSpreadValue = subCost
                    });

                    // Add sub hour spread to sub resource type
                    subResourceType.OffLoadedHourSpreads.Add(new ResourceSpreadDto()
                    {
                        BoeID = laborResource.BoeID,
                        LaborSpreadDate = spread.LaborSpreadDate,
                        LaborSpreadValue = subHours
                    });
                }
            }

            decimal? originalResourceHours = laborResource.ValueSpread;

            subResourceType.ValueSpread = totalSubCost;
            subResourceType.InHouseLaborSpreads = laborResource.LaborSpreads;
            laborResource.ValueSpread -= totalSubHours;
            laborResource.Updateable = UpdateType.Upsert;

            offloadedHours = totalSubHours;

            logger.Debug(
                $"OffloadRate. New sub resource created. BOE Id: {laborResource.BoeID}, Original Resource Id: {laborResource.ResourceID}, Sub Resource Name: {subResource.ResourceName}, Original Hours: {originalResourceHours}, " +
                $"Post-Offload Hours: {laborResource.ValueSpread}, Offloaded Hours: {totalSubHours}, Offloaded Cost: {totalSubCost}");

            return subResourceType;
        }

        /// <summary>
        /// Offloads the labor resource, this method is public for testing purposes.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="offloadRates">The offload rates.</param>
        /// <param name="laborResource">The labor resource.</param>
        /// <param name="offloadedHours">The number of offloaded hours.</param>
        /// <param name="percent">The percentage that is being used for the offload.</param>
        /// <returns>Null if the labor resource was not offloaded, a new Sub resource if it was offloaded.</returns>
        /// <exception cref="System.ArgumentNullException">offloadRates or laborResource or workspace</exception>
        public ResourceTypeDto OffloadLaborResource(FullWorkspace workspace, ICollection<OffloadRatesDTO> offloadRates, ResourceTypeDto laborResource, out decimal offloadedHours, out decimal? percent)
        {
            if (offloadRates == null || !offloadRates.Any())
            {
                throw new ArgumentNullException(nameof(offloadRates));
            }

            if (laborResource == null)
            {
                throw new ArgumentNullException(nameof(laborResource));
            }

            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            ResourceTypeDto subResourceType = null;
            offloadedHours = 0m;
            OffloadLaborRatesValidationResults validationResults = ValidateLaborResourceCanOffload(laborResource, offloadRates, workspace);
            percent = validationResults.Percent;
            
            if (validationResults.IsValid)
            {
                // Offload the resource
                subResourceType = this.OffloadRate(validationResults.OffloadRateByYear, laborResource, validationResults.SubResource, workspace.DecimalPrecision, workspace.CostDecimalPrecision, out offloadedHours);
            }
            else if (validationResults.MissingYear)
            {
                // if there are missing years (but at least one year matches) throw an exception
                throw new GenValidationException($"There was a missing year(s) of Offload Rates during Offload for Resource: {validationResults.ResourceName}, Cost Center/Performing Org: {validationResults.PerfOrgName}, Start Year: {validationResults.StartYear}, End Year: {validationResults.EndYear}");
            } 

            logger.Debug(
                $"OffloadLaborResource. CanOffload: {validationResults.IsValid}, Labor Resource Id: {laborResource.Id}, Resource Id: {laborResource.ResourceID}, Sub Resource: {validationResults.SubResource?.ResourceName ?? ""}, Decimal Precision: {workspace.DecimalPrecision}, " +
                $"Cost Decimal Precision: {workspace.CostDecimalPrecision}, Offloaded Percent: {percent}, Offloaded Hours: {offloadedHours}");

            return subResourceType;
        }

        /// <summary>
        /// Validates the labor resource.
        /// </summary>
        /// <param name="laborResource">The labor resource.</param>
        /// <param name="offloadRates">The offload rates.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>Whether the labor resource can be offloaded.</returns>
        public static OffloadLaborRatesValidationResults ValidateLaborResourceCanOffload(ResourceTypeDto laborResource, ICollection<OffloadRatesDTO> offloadRates, FullWorkspace workspace)
        {
            if (laborResource == null)
            {
                throw new ArgumentNullException(nameof(laborResource));
            }

            if (offloadRates == null)
            {
                throw new ArgumentNullException(nameof(offloadRates));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            
            OffloadLaborRatesValidationResults results = new OffloadLaborRatesValidationResults();

            if (!laborResource.CanOffload)
            {
                results.InvalidWarningText = "Labor Resource not set to Offload";
            }
            else if (laborResource.SpreadType != SpreadType.Hours)
            {
                results.InvalidWarningText = "Labor Resource Spread Type not set to Hours";
            }
            else if (!laborResource.SpreadCurveID.HasValue)
            {
                results.InvalidWarningText = "Labor Resource Spread Curve is not set";
            }
            else if (!laborResource.ValueSpread.HasValue || laborResource.ValueSpread.Value == 0)
            {
                results.InvalidWarningText = "Labor Resource Hours set to 0";
            }
            else
            {
                PerformingOrgDTO perfOrg = workspace.PerformingOrgsForWsList.FirstOrDefault(p => p.Id == laborResource.PerformingOrgID);
                if (perfOrg == null)
                {
                    // performOrg not found, retrieve it directly from DB
                    perfOrg = OffloadLaborRates.retriever.GetPerformingOrgsByIds(new Collection<int> { laborResource.PerformingOrgID.Value }).FirstOrDefault();
                }
                ResourceDTO resource = workspace.ResourcesForWsResourceListId.FirstOrDefault(r => r.Id == laborResource.ResourceID);
                if (resource == null)
                {
                    // resource not found, retrieve it directly from DB
                    resource = OffloadLaborRates.retriever.GetResourcesByIds(new Collection<int> { laborResource.ResourceID.Value }).FirstOrDefault();
                }

                // Skip offloading if perfOrg or Resource not found
                if (perfOrg == null)
                {
                    results.InvalidWarningText = "Labor Resource Performing Org not found";
                }
                else if (resource == null)
                {
                    results.InvalidWarningText = "Labor Resource Resource not found";
                }
                else
                {
                    if (resource.ElementOfCost != ElementOfCostType.LMLabor)
                    {
                        results.InvalidWarningText = $"Resource Element of Cost Type for selected Resource {resource.ResourceName} is not set to Labor";
                    }
                    else
                    {

                        results.ResourceName = resource.ResourceName;
                        results.PerfOrgName = perfOrg.PerformingOrgName;
                        results.StartYear = laborResource.StartDateValue.Year;
                        results.EndYear = laborResource.EndDateValue.Year;
                        if (results.StartYear <= results.EndYear)
                        {
                            bool invalidData = false;
                            int? subresourceId = null;

                            for (int year = results.StartYear; year <= results.EndYear; year++)
                            {
                                OffloadRatesDTO offloadRate = offloadRates.FirstOrDefault(o => o.Year == year && o.PerformingOrg == perfOrg.PerformingOrgName && o.Resource == resource.ResourceName);
                                if (offloadRate != null)
                                {
                                    results.OffloadRateByYear.Add(year, offloadRate);
                                    results.SubResource = workspace.ResourcesForWsResourceListId.FirstOrDefault(r => r.ResourceName == offloadRate.SubResource && r.ElementOfCost == ElementOfCostType.Sub);
                                    if (results.SubResource != null)
                                    {
                                        if (subresourceId.HasValue)
                                        {
                                            if (subresourceId != results.SubResource.Id)
                                            {
                                                // the sub resources do not match across years for the perf org/ resource combination, break out
                                                logger.Error(
                                                    $"Sub Resources do not match across Offload Performing Org/Resource combinations for Workspace ID: {workspace.Id}, Perf Org: {offloadRate.PerformingOrg}, Resource {offloadRate.Resource}.");
                                                results.InvalidWarningText = "Sub Resources do not match across Offload Performing Org/Resource combination";
                                                invalidData = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            subresourceId = results.SubResource.Id;
                                        }
                                    }
                                    else
                                    {
                                        results.InvalidWarningText = $"Sub Resource in Offload Rate not found: {offloadRate.SubResource}";
                                        invalidData = true;
                                        break;
                                    }

                                    if (results.Percent.HasValue)
                                    {
                                        if (results.Percent != offloadRate.Percent)
                                        {
                                            // the percent does not match across years for the perf org/ resource combination, break out
                                            logger.Error(
                                                $"The Percent for Offloading does not match across Offload Performing Org/Resource combinations for Workspace ID: {workspace.Id}, Perf Org: {offloadRate.PerformingOrg}, Resource {offloadRate.Resource}.");
                                            results.InvalidWarningText = "The Percent for Offloading does not match across Offload Performing Org/Resource combination";
                                            invalidData = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        results.Percent = offloadRate.Percent;
                                    }
                                }
                                else
                                {
                                    invalidData = true;
                                    results.InvalidWarningText = $"Missing Year ({year}) for Offload Rate for this Performing Org/Resource combination";
                                    results.MissingYear = true;
                                }
                            }

                            if (!invalidData && subresourceId.HasValue)
                            {
                                results.IsValid = true;
                            }
                        }
                    }
                }
            }

            // reset missing year to false if missing all of the years, should only be true if there is at least one year found
            if (results.MissingYear && !results.OffloadRateByYear.Any())
            {
                results.InvalidWarningText = "Offload Rate not found for this Performing Org/Resource combination";
                results.MissingYear = false;
            }

            return results;
        }

        /// <summary>
        /// Offloads as non project map workspace.
        /// </summary>
        /// <param name="boes">The BOEs to offload.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="offloadRates">The offload rates.</param>
        /// <returns>The offloaded labor results.</returns>
        private OffloadLaborRatesResults OffloadAsNonProjectMap(ICollection<FullBoe> boes, FullWorkspace workspace, ICollection<OffloadRatesDTO> offloadRates, string justifyingPublication, string projectMapOffloadText)
        {
            logger.Info($"OffloadAsNonProjectMap. Workspace Id: {workspace.Id}");

            OffloadLaborRatesResults results = new OffloadLaborRatesResults();
            foreach (FullBoe boe in boes)
            {
                logger.Debug($"OffloadAsNonProjectMap. Processing BOE Id: {boe.Id}");

                results.Boes.Enqueue(boe);

                decimal originalTaskHours = 0;
                decimal offloadedTaskHours = 0;
                decimal percentOffload = 0;

                boe.TemplateQuestionsAndAnswers = workspace.TemplateQuestionsAndAnswers.Where(x => x.BoeId == boe.Id).ToList();

                foreach (BoeTaskElementDTO taskElement in boe.TaskElements)
                {
                    originalTaskHours = 0;
                    offloadedTaskHours = 0;

                    decimal taskElementTotalHoursOffloaded = 0m;
                    Collection<ResourceTypeDto> resources = taskElement.taskElementLabors;
                    // Create a new list that is ordered with new sub resources inserted directly after the resources they were created from.
                    Collection<ResourceTypeDto> newResourceList = new Collection<ResourceTypeDto>();
                    foreach (ResourceTypeDto laborResource in resources)
                    {
                        newResourceList.Add(laborResource);
                        originalTaskHours += laborResource.SpreadType == SpreadType.Hours ? laborResource.ValueSpread ?? 0 : 0;
                        decimal offloadedHours;
                        decimal? percent;
                        ResourceTypeDto newResource = this.OffloadLaborResource(workspace, offloadRates, laborResource, out offloadedHours, out percent);
                        if (newResource != null)
                        {
                            newResource.TaskElementId = taskElement.Id;
                            newResourceList.Add(newResource);
                            results.TotalHoursOffloaded += offloadedHours;
                            results.TotalCostOffloaded += newResource.ValueSpread ?? 0;
                            taskElement.TotalHours -= offloadedHours;
                            taskElementTotalHoursOffloaded += offloadedHours;
                        }

                        offloadedTaskHours += offloadedHours;
                        percentOffload = percent ?? 0;
                    }

                    if (taskElementTotalHoursOffloaded > 0m)
                    {
                        // adjust MOQ equation, if it is empty to begin with then do nothing since it will be invalid either way
                        if (!string.IsNullOrWhiteSpace(taskElement.MOQHoursEquation))
                        {
                            taskElement.MOQHoursEquation = taskElement.MOQHoursEquation + " - " + taskElementTotalHoursOffloaded.ToString();
                        }
                    } 

                    taskElement.taskElementLabors = newResourceList;

                    if (offloadedTaskHours != 0)
                    {
                        string moqText = string.Format(this.EXISTING_TASK_RATIONALE, justifyingPublication, projectMapOffloadText, originalTaskHours.ToString("F"),
                                        offloadedTaskHours.ToString("F"), (originalTaskHours - offloadedTaskHours).ToString("F"));

                        if (workspace.RteOverrides.Contains(RteTemplateSource.TaskMOQ) || workspace.UsingTemplateBOE)
                        {
                            boe.TemplateQuestionsAndAnswers.Add(new RTECustomTemplateQuestionAnswerModelView() 
                            { 
                                QuestionText = "Offload Statement",
                                AnswerText = moqText,
                                BoeId = boe.Id,
                                TaskId = taskElement.Id,
                                SortOrder = 10000,
                                SourceId = (int)RteTemplateSource.TaskMOQ
                            });
                        }
                        else
                        { 
                            taskElement.MOQText += moqText;
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Offloads as project map workspace.
        /// </summary>
        /// <param name="boes">The BOEs to offload.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="justifyingPublication">The Justifying Publication.</param>
        /// <param name="offloadRates">The offload rates.</param>
        /// <returns>The offloaded labor results.</returns>
        private OffloadLaborRatesResults OffloadAsProjectMap(ICollection<FullBoe> boes, FullWorkspace workspace, ICollection<OffloadRatesDTO> offloadRates, string justifyingPublication, string projectMapOffloadText)
        {
            logger.Info($"OffloadAsProjectMap. Workspace Id: {workspace.Id}");

            OffloadLaborRatesResults results = new OffloadLaborRatesResults();

            // Return the list of boes ordered with new offload boes inserted directly after the boes they were created from.
            foreach (FullBoe boe in boes)
            {
                logger.Debug($"OffloadAsNonProjectMap. Processing BOE Id: {boe.Id}");

                results.Boes.Enqueue(boe);

                FullBoe newBoe = null;
                BoeTaskElementDTO newTaskElement = null;

                decimal originalBoeHours = 0;
                decimal offloadedBoeHours = 0;
                decimal percentOffload = 0;

                foreach (BoeTaskElementDTO taskElement in boe.TaskElements)
                {
                    bool taskElementOffloaded = false;

                    if (!taskElement.WasDescriptionSet)
                    {
                        throw new GenValidationException("RTE Fields must be set on Project Map Workspaces when Offloading");
                    }

                    foreach (ResourceTypeDto laborResource in taskElement.taskElementLabors)
                    {
                        if (laborResource.SpreadCurveID.HasValue)
                        {
                            originalBoeHours += laborResource.SpreadType == SpreadType.Hours ? laborResource.ValueSpread ?? 0 : 0;

                            decimal offloadedHours;
                            decimal? percent;
                            ResourceTypeDto newResource = this.OffloadLaborResource(workspace, offloadRates, laborResource, out offloadedHours, out percent);

                            if (newResource != null)
                            {
                                taskElement.TotalHours -= offloadedHours;
                                results.TotalHoursOffloaded += offloadedHours;

                                // values needed for text updates for rationale/description of tasks/boes
                                offloadedBoeHours += offloadedHours;
                                percentOffload = percent ?? 0;
                                results.TotalCostOffloaded += newResource.ValueSpread ?? 0;

                                if (newBoe == null)
                                {
                                    taskElementOffloaded = true;

                                    // Create a new boe for the subcontractor boe
                                    newBoe = this.fullObjectFactory.CreateFullBoe();
                                    newBoe.Title = CreateNewActivityId(boe.Title, workspace, laborResource);
                                    newBoe.SOW = boe.SOW;
                                    newBoe.SOWTitle = boe.SOWTitle;
                                    newBoe.CamName = boe.CamName;
                                    newBoe.Category = boe.Category;
                                    newBoe.StartDate = boe.StartDate;
                                    newBoe.EndDate = boe.EndDate;
                                    newBoe.WBSID = boe.WBSID;
                                    newBoe.CLINID = boe.CLINID;
                                    newBoe.Description = boe.Description;
                                    newBoe.Id = this.currentBoeId--;
                                    newBoe.Updateable = UpdateType.Upsert;
                                    newBoe.ClassOfCost = boe.ClassOfCost;

                                    newTaskElement = new BoeTaskElementDTO()
                                    {
                                        BoeID = newBoe.Id,
                                        Id = this.currentTaskId--,
                                        BOETaskID = taskElement.BOETaskID,
                                        BOETaskElementOrder = 99,
                                        Description = string.Format(this.NEW_TASK_DESCRIPTION, justifyingPublication, newBoe.Title, projectMapOffloadText),
                                        EndDate = taskElement.EndDate,
                                        StartDate = taskElement.StartDate,
                                        taskElementLabors = new Collection<ResourceTypeDto>(),
                                        TaskElementType = TaskElementType.Labor,
                                        TaskTitle = taskElement.TaskTitle,
                                        TotalCost = 0,
                                        TotalHours = 0,
                                        Updateable = UpdateType.Upsert,
                                        MOQText = string.Format(this.NEW_TASK_RATIONALE,
                                            newBoe.Title,
                                            originalBoeHours.ToString("F"),
                                            percentOffload.ToString("P"),
                                            offloadedBoeHours.ToString("F"),
                                            newBoe.StartDate.ToString("MM/yyyy"),
                                            newBoe.EndDate.ToString("MM/yyyy"),
                                            justifyingPublication,
                                            projectMapOffloadText),
                                        SOW = taskElement.SOW,
                                        SOWTitle = taskElement.SOWTitle,
                                        Category = taskElement.Category,
                                        CamName = taskElement.CamName,
                                        ClassOfCost = taskElement.ClassOfCost
                                    };

                                    newBoe.SetTaskElements(new List<BoeTaskElementDTO> { newTaskElement });
                                }

                                newTaskElement.TotalCost += newResource.ValueSpread;
                                newTaskElement.taskElementLabors.Add(newResource);
                                newResource.TaskElementId = newTaskElement.Id;
                            }
                        }
                    }

                    if (taskElementOffloaded)
                    {
                        taskElement.MOQText += string.Format(this.EXISTING_TASK_RATIONALE, justifyingPublication, projectMapOffloadText, originalBoeHours.ToString("F"),
                                                            offloadedBoeHours.ToString("F"), (originalBoeHours - offloadedBoeHours).ToString("F"));
                    }
                }

                if (newBoe != null)
                {
                    logger.Debug($"OffloadAsNonProjectMap - Created a new BOE. New Activity Id: {newBoe.Title}");
                    results.Boes.Enqueue(newBoe);
                }
            }

            return results;
        }

        /// <summary>
        /// Creates the new offloaded ActivityId from the original ActivityId.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="laborResource">The labor resource.</param>
        /// <returns>A new ActivityId created from original labor resource's ActivityId.</returns>
        private static string CreateNewActivityId(string title, FullWorkspace workspace, ResourceTypeDto laborResource)
        {
            // New Activity ID is first 7 characters of old Activity ID + "OL" + Perf org Name + Resource Name

            // Using First() instead of FirstOrDefault() becuase no need to check for null for perfOrg/resource because the workflow would not get here if they were null
            PerformingOrgDTO perfOrg = workspace.PerformingOrgsForWsList.First(p => p.Id == laborResource.PerformingOrgID);
            ResourceDTO resource = workspace.ResourcesForWsResourceListId.First(r => r.Id == laborResource.ResourceID);

            int length = Math.Min(20, title.Length);
            string activityId = title.Substring(0, length) + "OL" + perfOrg.PerformingOrgName + resource.ResourceName;

            length = Math.Min(40, activityId.Length);
            activityId = activityId.Substring(0, length);

            return activityId;
        }

        /// <summary>
        /// Groups the spreads by year.
        /// </summary>
        /// <param name="laborSpreads">The labor spreads.</param>
        /// <returns>A dictionary keyed by year of resource spreads.</returns>
        private Dictionary<int, ResourceSpreadDto[]> GroupSpreadsByYear(Collection<ResourceSpreadDto> laborSpreads)
        {
            Dictionary<int, ResourceSpreadDto[]> groupByYear = new Dictionary<int, ResourceSpreadDto[]>();
            IEnumerable<int> years = laborSpreads.Select(ls => ls.LaborSpreadDate.Year).Distinct();

            foreach (int year in years)
            {
                groupByYear.Add(year, laborSpreads.Where(ls => ls.LaborSpreadDate.Year == year).ToArray());
            }

            return groupByYear;
        }
    }
}