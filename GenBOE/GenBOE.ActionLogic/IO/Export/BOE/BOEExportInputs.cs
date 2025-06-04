// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export.BOE
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.PickList;
	using Microsoft.Practices.Unity;
	using MoreLinq;

	/// <summary>
	/// Inputs used for the BOE Exporters.
	/// </summary>
	public class BOEExportInputs
	{
		/// <summary>
		/// The logger.
		/// </summary>
		private Logger logger = new Logger(typeof(BOEExportInputs));

		/// <summary>
		/// Initializes a new instance of the <see cref="BOEExportInputs"/> class.
		/// This method is used when only needing one boe.
		/// </summary>
		/// <param name="boe">The boe.</param>
		/// <param name="workspace">The workspace.</param>
		/// <param name="moqTypes">The moq types selected</param>
		/// <param name="rteTemplatesOverrides">The RTE Template overrides</param>
		/// <exception cref="System.ArgumentNullException">workspace</exception>
		public BOEExportInputs(FullBoe boe, FullWorkspace workspace, ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplatesOverrides = null,
			ICollection<MoqTypeSelection> moqTypes = null)
		{
			if (ReferenceEquals(workspace, null))
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			if (ReferenceEquals(boe, null))
			{
				throw new ArgumentNullException(nameof(boe));
			}

			this.logger.Debug("Exporting - BOEExportInputs - Initializing Inputs - begin");
			this.SetRteTemplateOverrides(rteTemplatesOverrides);
			this.SetMoqTypes(moqTypes);
			this.Boes = new List<BoeDTO> { boe.ToDTO() }.AsReadOnly();
			this.TaskElements = workspace.TaskElements.DeepClone();
			this.Workspace = workspace.ToDTO();
			bool isBRCEnabled = Utilities.IsBRCEnabledForWorkspace(workspace.Shortname);
			
			// since the resources used may not contain offloaded resources, need to manually get this
			this.ResourcesUsedInWsBoes = workspace.ResourcesUsedInWsBoes;
			this.PerformingOrgsUsedInBoes = workspace.PerformingOrgsUsedInBoes;
			this.FullWorkspace = workspace;
			this.CalculateSkillMix(workspace, isBRCEnabled, this.TaskElements);

			this.logger.Debug("Exporting - BOEExportInputs - Initializing Inputs - end");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BOEExportInputs" /> class.
		/// </summary>
		/// <param name="boesToExport">The boes.</param>
		/// <param name="allWorkspaceBoes">All of the workspace's boes.</param>
		/// <param name="taskElements">The task elements for entire workspace.</param>
		/// <param name="workspace">The workspace.</param>
		/// <param name="rteTemplatesOverrides">RTE Template Overrides</param>
		/// <param name="moqTypes">MOQ Types</param>
		/// <param name="processLaborTypesForBrc">Should Labor Types be processed for BRCs?</param>
		/// <param name="processLaborTypesForUCOT">Should Labor Types be processed for UCOT?</param>
		/// <exception cref="ArgumentNullException">workspace</exception>
		public BOEExportInputs(ICollection<FullBoe> boesToExport, ICollection<FullBoe> allWorkspaceBoes, ICollection<BoeTaskElementDTO> taskElements,
			FullWorkspace workspace, ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplatesOverrides = null,
			ICollection<MoqTypeSelection> moqTypes = null, bool processLaborTypesForBrc = false, bool processLaborTypesForUCOT = false)
		{
			_ = taskElements ?? throw new ArgumentNullException(nameof(taskElements));
			if (ReferenceEquals(workspace, null))
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			taskElements = taskElements.DeepClone();
			this.PerformingOrgsUsedInBoes = workspace.PerformingOrgsUsedInBoes;

			this.logger.Debug("Exporting - BOEExportInputs - Initializing Inputs - begin");
			IRetriever retriever = GenBOEUnityContainer.Container.Resolve(typeof(IRetriever)) as IRetriever;
			this.SetRteTemplateOverrides(rteTemplatesOverrides);
			this.SetMoqTypes(moqTypes);
            this.Boes = boesToExport.Select(x => x.ToDTO()).ToList().AsReadOnly();
            this.Clins = workspace.Clins.Select(x => x.ToDTO()).ToList().AsReadOnly();
            this.WbsElements = workspace.WbsElements.Select(x => new WbsDTO(x)).ToList().AsReadOnly();

            // since the resources used may not contain offloaded resources, need to manually get this
            ICollection<int> resourceIds = taskElements.SelectMany(x => x.taskElementLabors).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value)
						.Union(workspace.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value))
						.Union(workspace.Odcs.SelectMany(x => x.ODCTypes).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value))
						.Union(taskElements.SelectMany(x => x.taskElementLabors).Where(x => x.BusinessResourceCodeID.HasValue).Select(x => x.BusinessResourceCodeID.Value))
						.Union(workspace.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.BusinessResourceCodeID.HasValue).Select(x => x.BusinessResourceCodeID.Value))
						.Distinct().ToList();

			if (Utilities.ShowUCOTForWorkspace(workspace.CreationDate) && processLaborTypesForUCOT)
			{
				// Setup the ucot performing orgs.
				PerformingOrgDTO ucotPerformingOrg = new PerformingOrgDTO
				{
					Id = Constants.UCOT_PERF_ORG_ID,
					PerformingOrgDesc = Constants.UCOT_LABEL,
					PerformingOrgName = Constants.UCOT_LABEL
				};

				this.PerformingOrgsUsedInBoes = this.PerformingOrgsUsedInBoes.Concat(new[] { ucotPerformingOrg }).ToList().AsReadOnly();
			}

			bool isBRCEnabled = Utilities.IsBRCEnabledForWorkspace(workspace.Shortname);
			
			this.ResourcesUsedInWsBoes = retriever.GetResourcesByIds(resourceIds).ToList().AsReadOnly();
			this.CalculateSkillMix(workspace, isBRCEnabled, taskElements);

			int startingIndex = -1;
			if (isBRCEnabled && processLaborTypesForBrc)
			{
				IDictionary<int, string> resourceIdToSegmentRegion = this.ResourcesUsedInWsBoes.ToDictionary(r => r.Id, d => d.SegRegion);
				foreach (BoeTaskElementDTO taskElement in taskElements)
				{
					taskElement.taskElementLabors = BRCValidationUtility.ProcessLaborTypesForBrc(taskElement.taskElementLabors, resourceIdToSegmentRegion, workspace.Shortname, startingIndex).ToCollection();
					if (taskElement.taskElementLabors.Any())
					{
						startingIndex = Math.Min(startingIndex, taskElement.taskElementLabors.Select(x => x.Id).Min() - 1);
					}
				}
			}

			if (Utilities.ShowUCOTForWorkspace(workspace.CreationDate) && processLaborTypesForUCOT)
			{
				List<ResourceTypeDto> taskElementLabors = new List<ResourceTypeDto>();
				HashSet<int> updatedTaskIds = new HashSet<int>();
				// Need to make sure we are only adding UCOT to tasks that qualify
				foreach (BoeTaskElementDTO boeTaskElement in taskElements)
				{
					// Get the MOQ Types for this task
					ICollection<MoqTypeSelection> taskMOQs = this.MOQTypes.Where(m => m.TaskId == boeTaskElement.Id).ToList();
					if (taskMOQs.Count == 1)
					{
						MoqTypeSelection moqType = taskMOQs.First();
						if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems &&
							(moqType.SelectedMOQType == MOQType.Historical || moqType.SelectedMOQType == MOQType.Comparative || moqType.SelectedMOQType == MOQType.AnalogousRelationships))
						{
							taskElementLabors.AddRange(boeTaskElement.taskElementLabors);
							updatedTaskIds.Add(boeTaskElement.Id);
						}
					}
				}

				// Filter by element of cost only want to work on labor
				Dictionary<int, ElementOfCostType> laborToElementOfCost = new Dictionary<int, ElementOfCostType>();
				foreach (ResourceTypeDto labor in taskElementLabors)
				{
					ResourceDTO resource = this.ResourcesUsedInWsBoes.FirstOrDefault(x => x.Id == labor.ResourceID);

					laborToElementOfCost[labor.Id] = resource?.ElementOfCost ?? ElementOfCostType.NotSet;
				}

				// Add UCOT data
				taskElementLabors = AddUCOT(taskElementLabors, workspace.UCOTFactor, laborToElementOfCost);

				// Update TaskElements with the new labors
				foreach (BoeTaskElementDTO taskElement in taskElements)
				{
					if (updatedTaskIds.Contains(taskElement.Id))
					{
						taskElement.taskElementLabors = taskElementLabors.Where(x => x.TaskElementId == taskElement.Id).ToCollection();
						taskElement.TotalHours = taskElementLabors.Where(tl => tl.SpreadType == SpreadType.Hours).Sum(l => l.ValueSpread);
					}
				}
			}

			this.TaskElements = taskElements.ToList().AsReadOnly();
			PopulateLaborTypesMappingWithCustomFieldsValuesAndContainerIds();

			this.Workspace = workspace.ToDTO();

			this.FullWorkspace = workspace;
            this.AllWorkspaceBoes = allWorkspaceBoes.Select(x => x.ToDTO()).ToList().AsReadOnly();
            this.logger.Debug("Exporting - BOEExportInputs - Intitializing Inputs - end");
		}

		/// <summary>
		/// Gets or sets all of the RTE Custom Template Overrides.
		/// </summary>
		public IReadOnlyCollection<RTECustomTemplateQuestionAnswerModelView> RTETemplatesOverrides { get; private set; }

		/// <summary>
		/// The original collection of RTE Custom Template Overrides.
		/// </summary>
		private ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplatesOverridesCollection;

		/// <summary>
		/// Gets all of the boes in the workspace.
		/// </summary>
		public IReadOnlyCollection<BoeDTO> AllWorkspaceBoes { get; }

		/// <summary>
		/// Gets the full workspace, only to be used when rewriting is impossible.
		/// </summary>
		public FullWorkspace FullWorkspace { get; }

		/// <summary>
		/// Gets all Travel Trips for the workspace.
		/// </summary>
		public IReadOnlyCollection<TripDTO> TravelTrips { get { return this.FullWorkspace.TravelTrips; } }

		/// <summary>
		/// Gets all Per Diems for all Travel Trips for the workspace.
		/// </summary>
		public IReadOnlyCollection<PerDiemDTO> PerDiemsForTravelTrips { get { return this.FullWorkspace.PerDiemsForTravelTrips; } }

		/// <summary>
		/// Gets all Miscellaneous travel rates for all Travel Trips for the workspace.
		/// </summary>
		public IReadOnlyCollection<MiscTravelRateDTO> MiscTravelRatesForTravelTrips { get { return this.FullWorkspace.MiscTravelRatesForTravelTrips; } }

		/// <summary>
		/// Gets all Escalation Rates for the workspace.
		/// </summary>
		public IReadOnlyCollection<EscalationRatesDTO> EscalationRates { get { return this.FullWorkspace.EscalationRates; } }

		/// <summary>
		/// Locations used by trips
		/// </summary>
		public IReadOnlyCollection<LocationDTO> LocationsUsedByTrips { get { return this.FullWorkspace.LocationsUsedByTrips; } }

		/// <summary>
		/// Gets the task elements mapping with custom fields values and container ids.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<int, ICollection<KeyValuePair<int, int>>> TaskElementsMappingWithCustomFieldsValuesAndContainerIds { get { return this.FullWorkspace.TaskElementsMappingWithCustomFieldsValuesAndContainerIds; } }

		/// <summary>
		/// Gets a mapping of all custom field values and custom fields to Boes that use them, for the entire WS
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> AssignedBoeIdsAndCustomFieldValuesMapping { get { return this.FullWorkspace.AssignedBoeIdsAndCustomFieldValuesMapping; } }

		/// <summary>
		///// Gets the labor types mapping with custom fields values and container ids.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<int, ICollection<KeyValuePair<int, int>>> LaborTypesMappingWithCustomFieldsValuesAndContainerIds { get; private set; }

		/// <summary>
		/// Gets the resources for ws resource list identifier.
		/// </summary>
		public IReadOnlyCollection<ResourceDTO> ResourcesForWsResourceListId { get { return this.FullWorkspace.ResourcesForWsResourceListId; } }

		/// <summary>
		/// Gets the resources for system resource list identifier.
		/// </summary>
		public IReadOnlyCollection<ResourceDTO> ResourcesForSystemResourceListId { get { return this.FullWorkspace.ResourcesForSystemResourceListId; } }

		/// <summary>
		/// Gets the workspace variables.
		/// </summary>
		public IReadOnlyCollection<WorkspaceVariableDTO> WorkspaceVariables { get { return this.FullWorkspace.WorkspaceVariables; } }

		/// <summary>
		/// Gets the boes.
		/// </summary>
		public IReadOnlyCollection<BoeDTO> Boes { get; }

		/// <summary>
		/// Gets the task elements.
		/// </summary>
		public IReadOnlyCollection<BoeTaskElementDTO> TaskElements { get; }

		/// <summary>
		/// Gets the resources used in ws boes.
		/// </summary>
		public IReadOnlyCollection<ResourceDTO> ResourcesUsedInWsBoes { get; private set; }

		/// <summary>
		/// Gets the workspace export formats.
		/// </summary>
		public IReadOnlyCollection<WorkspaceExportFormatDTO> WorkspaceExportFormats { get { return this.FullWorkspace.WorkspaceExportFormats; } }

		/// <summary>
		/// Gets the workspace.
		/// </summary>
		public WorkspaceDTO Workspace { get; }

		/// <summary>
		/// Gets the custom fields.
		/// </summary>
		public IReadOnlyCollection<CustomFieldDTO> CustomFields { get { return this.FullWorkspace.CustomFields; } }

		/// <summary>
		/// Gets the custom field values.
		/// </summary>
		public IReadOnlyCollection<CustomFieldValueDTO> CustomFieldValues { get { return this.FullWorkspace.CustomFieldValues; } }

        /// <summary>
        /// Gets the clins.
        /// </summary>
        public IReadOnlyCollection<ClinDTO> Clins { get; }

        /// <summary>
        /// Clins belonging to the Workspace without Multi Clin
        /// </summary>
        public IReadOnlyCollection<ClinDTO> ClinsNoMultiClin { get { return this.FullWorkspace.ClinsNoMultiClin; } }

        /// <summary>
        /// Gets the WBS elements.
        /// </summary>
        public IReadOnlyCollection<WbsDTO> WbsElements { get; }

        /// <summary>
        /// Wbs Elements belonging to the Workspace without Multi
        /// </summary>
        public IReadOnlyCollection<WbsDTO> WbsElementsNoMultiWbs { get { return this.FullWorkspace.WbsElementsNoMultiWbs; } }

        /// <summary>
        /// Gets the boe mapping with approver responses.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IDictionary<int, ICollection<BoeApproverResponseDTO>> BoeMappingWithApproverResponses { get { return this.FullWorkspace.BoeMappingWithApproverResponses; } }

		/// <summary>
		/// Gets the get user data for boes for ws.
		/// </summary>
		public IReadOnlyCollection<UserDTO> GetUserDataForBoesForWs { get { return this.FullWorkspace.GetUserDataForBoesForWs; } }

		/// <summary>
		/// Gets the performing orgs used in boes.
		/// </summary>
		public IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsUsedInBoes { get; }

		/// <summary>
		/// Gets the performing orgs for ws list.
		/// </summary>
		public IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsForWsList { get { return this.FullWorkspace.PerformingOrgsForWsList; } }

		/// <summary>
		/// Gets the boe ids and last user to submit them for approval mapping.
		/// </summary>
		public Dictionary<int, UserDTO> BoeIdsAndLastUserToSubmitThemForApprovalMapping { get { return this.FullWorkspace.BoeIdsAndLastUserToSubmitThemForApprovalMapping; } }

		/// <summary>
		/// Gets the odcs.
		/// </summary>
		public IReadOnlyCollection<OtherDirectCostDTO> Odcs { get { return this.FullWorkspace.Odcs; } }

		/// <summary>
		/// Gets the travels.
		/// </summary>
		public IReadOnlyCollection<TravelDTO> Travels { get { return this.FullWorkspace.Travels; } }

		/// <summary>
		/// Gets the materials.
		/// </summary>
		public IReadOnlyCollection<MaterialDTO> Materials { get { return this.FullWorkspace.Materials; } }

		/// <summary>
		/// Gets/Sets the "summarize by" parameter (custom field name at the Resource Types level) 
		/// used by the All BOEs report when running with the special format template.
		/// </summary>
		public string SummarizeByCustomField { get; set; }

		/// <summary>
		/// Gets or sets the contract types.
		/// </summary>
		public ICollection<PickListDto> ContractTypes { get; set; }

		/// <summary>
		/// Gets the MOQ Types
		/// </summary>
		public IReadOnlyCollection<MoqTypeSelection> MOQTypes { get; private set; }

		/// <summary>
		/// Sets the RTE Template Overrides, providing a null check.
		/// </summary>
		/// <param name="rteTemplatesOverrides">The rte template overrides.</param>
		private void SetRteTemplateOverrides(ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplatesOverrides)
		{
			if (rteTemplatesOverrides == null)
			{
				this.RTETemplatesOverrides = new List<RTECustomTemplateQuestionAnswerModelView>().AsReadOnly();
			}
			else
			{
				this.rteTemplatesOverridesCollection = rteTemplatesOverrides;
				this.RTETemplatesOverrides = new List<RTECustomTemplateQuestionAnswerModelView>(rteTemplatesOverrides).AsReadOnly();
			}
		}

		/// <summary>
		/// Clears the RTE Overrides List
		/// </summary>
		public void ClearRteOverrides()
		{
			// release all references inside original list to free up memory.
			if (this.rteTemplatesOverridesCollection != null)
			{
				this.rteTemplatesOverridesCollection.Clear();
			}

			this.RTETemplatesOverrides = new List<RTECustomTemplateQuestionAnswerModelView>().AsReadOnly();
		}

		/// <summary>
		/// Sets the MOQ Types
		/// Empty collection if null
		/// </summary>
		/// <param name="moqTypes">MOQ Types</param>
		private void SetMoqTypes(ICollection<MoqTypeSelection> moqTypes)
		{
			if (moqTypes == null)
			{
				this.MOQTypes = new List<MoqTypeSelection>().AsReadOnly();
			}
			else
			{
				this.MOQTypes = new List<MoqTypeSelection>(moqTypes).AsReadOnly();
			}
		}

		/// <summary>
		/// Populates the LaborTypesMappingWithCustomFieldsValuesAndContainerIds dictionary with relevant data.
		/// </summary>
		private void PopulateLaborTypesMappingWithCustomFieldsValuesAndContainerIds()
		{
			this.LaborTypesMappingWithCustomFieldsValuesAndContainerIds = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
			ICollection<ResourceTypeDto> resourceTypes = this.TaskElements.SelectMany(x => x.taskElementLabors).ToList();
			foreach (ResourceTypeDto resource in resourceTypes)
			{
				// Process the CustomFieldValueContainers for each child
				List<KeyValuePair<int, int>> keyValuePairs = new List<KeyValuePair<int, int>>();

				foreach (CustomFieldValueContainer customFieldValueContainer in resource.CustomFieldValueContainers)
				{
					keyValuePairs.Add(new KeyValuePair<int, int>(
						customFieldValueContainer.ContainerID,
						customFieldValueContainer.CustomFieldValueID
					));
				}

				// Add keyValuePairs to dictionary if it has value
				if (keyValuePairs.Any())
				{
					this.LaborTypesMappingWithCustomFieldsValuesAndContainerIds[resource.Id] = keyValuePairs;
				}

			}
		}

		/// <summary>
		/// Adds UCOT (Uncompensated Overtime) where applicable
		/// </summary>
		/// <param name="taskElementLabors">The task Element labors</param>
		/// <param name="ucotFactor">The UCOT Factor</param>
		/// <param name="laborToElementOfCost">Labor to element cost dictionary</param>
		/// <returns>UCOT resources.</returns>
		private List<ResourceTypeDto> AddUCOT(List<ResourceTypeDto> taskElementLabors, decimal ucotFactor, Dictionary<int, ElementOfCostType> laborToElementOfCost)
		{
			List<ResourceTypeDto> ucotLabors = taskElementLabors.ToList();

			int ucotResourceIndex = Constants.UCOT_RESOURCE_ID;

			// Keyed by original resource name to new ucot resource 
			Dictionary<int, ResourceDTO> ucotResourceBindings = new Dictionary<int, ResourceDTO>();
			decimal ucotMultiplier = ucotFactor / 100.0m;
			int idCounter = -100;

			// Now, we loop over all the spreads and add the UCOT factor where needed
			foreach (ResourceTypeDto labor in taskElementLabors)
			{

				// UCOT is only applicable if ResourceTypeDto is Hours and LMLabor Element of Cost, and Spread is past 1LMX date
				if (labor.SpreadType == SpreadType.Hours &&
					laborToElementOfCost[labor.Id] == ElementOfCostType.LMLabor &&
					labor.LaborSpreads != null && labor.LaborSpreads.Any() &&
					labor.EndDate >= Utilities.OneLmxStartDate && labor.BusinessResourceCodeID.HasValue)
				{
					int newLaborTypeId = idCounter--;

					// If it doesn't find the ucot resource then we need to create the ucot resource and add it back into our dictonary to pair the labor resource with its ucot counterpart (i.e. C1MDAAA1 vs. C1MDAAA1-UCOT)
					if (!ucotResourceBindings.TryGetValue(labor.BusinessResourceCodeID.Value, out ResourceDTO ucotResource))
					{
						ResourceDTO originalResource = this.ResourcesUsedInWsBoes.First(x => x.Id == labor.BusinessResourceCodeID);

						ucotResource = new ResourceDTO
						{
							Id = ucotResourceIndex--,
							ElementOfCost = ElementOfCostType.LMLabor,
							ResourceDesc = $"{originalResource.ResourceDesc}-{Constants.UCOT_LABEL}",
							ResourceName = $"{originalResource.ResourceName}-{Constants.UCOT_LABEL}",
							SegRegion = originalResource.SegRegion,
						};

						ucotResourceBindings.Add(labor.BusinessResourceCodeID.Value, ucotResource);
					}

					Collection<ResourceSpreadDto> spreads = new Collection<ResourceSpreadDto>();
					foreach (ResourceSpreadDto spread in labor.LaborSpreads)
					{
						spreads.Add(new ResourceSpreadDto()
						{
							LaborSpreadDate = spread.LaborSpreadDate,
							LaborTypeId = newLaborTypeId,
							BoeID = spread.BoeID,
							Id = idCounter--,
							LaborSpreadValue = (Utilities.OneLmxStartDate <= spread.LaborSpreadDate)
								? spread.LaborSpreadValue * ucotMultiplier
								: 0.0m
						});
					}

					ResourceTypeDto ucot = new ResourceTypeDto()
					{
						BoeID = labor.BoeID,
						LaborSpreads = spreads,
						CLINID = labor.CLINID,
						WBSID = labor.WBSID,
						EndDate = labor.EndDate,
						StartDate = labor.StartDate,
						SpreadCurveID = labor.SpreadCurveID,
						SpreadType = labor.SpreadType,
						BusinessResourceCodeID = ucotResource.Id,
						ResourceID = ucotResource.Id,
						PerformingOrgID = Constants.UCOT_PERF_ORG_ID,
						TaskElementId = labor.TaskElementId,
						Id = newLaborTypeId,
						ValueSpread = spreads.Sum(s => s.LaborSpreadValue)
					};

					ucotLabors.Add(ucot);
				}
			}

			List<ResourceDTO> updatedResources = new List<ResourceDTO>(this.ResourcesUsedInWsBoes);
			updatedResources.AddRange(ucotResourceBindings.Values);

			this.ResourcesUsedInWsBoes = updatedResources.AsReadOnly();

			return ucotLabors;
		}

		/// <summary>
		/// Calculate Skill Mix on the Tasks as needed
		/// </summary>
		/// <param name="isBRCEnabled">Is brc enabled</param>
		/// <param name="ws">The Full Workspace</param>
		/// <param name="taskElements">The task elements</param>
		private void CalculateSkillMix(FullWorkspace ws, bool isBRCEnabled, IEnumerable<BoeTaskElementDTO> taskElements)
		{
			if (Utilities.ShowSkillMixForWorkspace(ws.CreationDate))
			{
				foreach (BoeTaskElementDTO task in taskElements)
				{
					if (BOETaskUtility.ShowSkillMixForTask(ws, task))
					{
						// Get all MOQ Resource Hours for Task
						ICollection<MOQTypeSelectionTableDataResourceHoursDTO> moqResourceHours = ws.MoqTypeSelections.Where(m => m.TaskId == task.Id).SelectMany(m => m.TableData).SelectMany(t => t.ResourceHours).ToList();

						// convert Task's labor resources to modelview
						List<LaborTypeDataModelView> laborTypes = new List<LaborTypeDataModelView>();
						foreach (ResourceTypeDto labor in task.taskElementLabors)
						{
							ResourceDTO resource = new ResourceDTO();
							if (labor.ResourceID != null)
							{
								resource = this.ResourcesUsedInWsBoes.First(x => x.Id == labor.ResourceID.Value);
							}

							ResourceDTO businessResourceCode = new ResourceDTO();
							if (labor.BusinessResourceCodeID != null && labor.BusinessResourceCodeID > 0)
							{
								businessResourceCode = this.ResourcesUsedInWsBoes.First(x => x.Id == labor.BusinessResourceCodeID.Value);
							}

							PerformingOrgDTO perfOrg = new PerformingOrgDTO();
							if (labor.PerformingOrgID != null)
							{
								perfOrg = this.PerformingOrgsUsedInBoes.First(x => x.Id == labor.PerformingOrgID.Value);
							}

							LaborTypeDataModelView laborToAdd = new LaborTypeDataModelView(labor, resource, businessResourceCode, perfOrg, ws.UCOTFactor, Utilities.ShowUCOTForWorkspace(ws.CreationDate) && businessResourceCode.ElementOfCost == ElementOfCostType.LMLabor && businessResourceCode.RateType == RateType.Hours, ws.DecimalPrecision);

							laborTypes.Add(laborToAdd);
						}


						RefreshSkillMixModelView refreshedData = SkillMixUtility.RefreshSkillMixTables(moqResourceHours, laborTypes, task.SkillMixTable, task.CommonDisclosureTable, isBRCEnabled,
							!ws.EnableSAPConnection);

						// Now reset the data
						task.SkillMixTable = refreshedData.SkillMixRows;
						task.CommonDisclosureTable = refreshedData.CommonDisclosureRows;
					}
					else
					{
						// can safely zero out any bad data
						task.SkillMixTable.Clear();
						task.CommonDisclosureTable.Clear();
					}
				}
			}
		}
	}
}