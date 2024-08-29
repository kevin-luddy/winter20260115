// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export.BOE
{
	using System;
	using System.Collections.Generic;
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
		/// <exception cref="System.ArgumentNullException">workspace</exception>
		public BOEExportInputs(FullBoe boe, FullWorkspace workspace, ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplatesOverrides = null,
			ICollection<MoqTypeSelection> moqTypes = null)
        {
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            this.logger.Debug("Exporting - BOEExportInputs - Intitializing Inputs - begin");
            this.SetRteTemplateOverrides(rteTemplatesOverrides);
            this.SetMoqTypes(moqTypes);
            this.Boes = new List<BoeDTO> { boe }.AsReadOnly();
			this.TaskElements = workspace.TaskElements;
            this.Workspace = workspace;

            // since the resources used may not contain offloaded resources, need to manually get this
            this.ResourcesUsedInWsBoes = workspace.ResourcesUsedInWsBoes;
            this.FullWorkspace = workspace;
            this.logger.Debug("Exporting - BOEExportInputs - Intitializing Inputs - end");
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
        /// <exception cref="ArgumentNullException">workspace</exception>
        public BOEExportInputs(ICollection<FullBoe> boesToExport, ICollection<FullBoe> allWorkspaceBoes, ICollection<BoeTaskElementDTO> taskElements,
            FullWorkspace workspace, ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplatesOverrides = null, 
			ICollection<MoqTypeSelection> moqTypes = null, bool processLaborTypesForBrc = false)
        {
			_ = taskElements ?? throw new ArgumentNullException(nameof(taskElements));
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            this.logger.Debug("Exporting - BOEExportInputs - Intitializing Inputs - begin");
            IRetriever retriever = GenBOEUnityContainer.Container.Resolve(typeof(IRetriever)) as IRetriever;
            this.SetRteTemplateOverrides(rteTemplatesOverrides);
            this.SetMoqTypes(moqTypes);
            this.Boes = boesToExport.ToList<BoeDTO>().AsReadOnly();

			if (Utilities.IsBRCEnabledForWorkspace(workspace.Shortname) && processLaborTypesForBrc)
			{
				foreach (BoeTaskElementDTO taskElement in taskElements)
				{
					taskElement.taskElementLabors = BRCValidationUtility.ProcessLaborTypesForBrc(taskElement.taskElementLabors, workspace.Shortname).ToCollection();
				}
			}
			
			this.TaskElements = taskElements.ToList().AsReadOnly();
			PopulateLaborTypesMappingWithCustomFieldsValuesAndContainerIds();

			this.Workspace = workspace;

            // since the resources used may not contain offloaded resources, need to manually get this
            ICollection<int> resourceIds = taskElements.SelectMany(x => x.taskElementLabors).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value)
                        .Union(workspace.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value))
                        .Union(workspace.Odcs.SelectMany(x => x.ODCTypes).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value))
                        .Union(taskElements.SelectMany(x => x.taskElementLabors).Where(x => x.BusinessResourceCodeID.HasValue).Select(x => x.BusinessResourceCodeID.Value))
                        .Union(workspace.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.BusinessResourceCodeID.HasValue).Select(x => x.BusinessResourceCodeID.Value))
                        .Distinct().ToList();

            this.ResourcesUsedInWsBoes = retriever.GetResourcesByIds(resourceIds).ToList().AsReadOnly();
            this.FullWorkspace = workspace;
            this.AllWorkspaceBoes = allWorkspaceBoes.ToList<BoeDTO>().AsReadOnly();
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
        public IReadOnlyCollection<ResourceDTO> ResourcesUsedInWsBoes { get; }

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
        public IReadOnlyCollection<ClinDTO> Clins { get { return this.FullWorkspace.Clins; } }

        /// <summary>
        /// Clins belonging to the Workspace without Multi Clin
        /// </summary>
        public IReadOnlyCollection<FullClin> ClinsNoMultiClin { get { return this.FullWorkspace.ClinsNoMultiClin; } }

        /// <summary>
        /// Gets the WBS elements.
        /// </summary>
        public IReadOnlyCollection<WbsDTO> WbsElements { get { return this.FullWorkspace.WbsElements; } }

        /// <summary>
        /// Wbs Elements belonging to the Workspace without Multi
        /// </summary>
        public IReadOnlyCollection<FullWbs> WbsElementsNoMultiWbs { get { return this.FullWorkspace.WbsElementsNoMultiWbs; } }

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
        public IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsUsedInBoes { get { return this.FullWorkspace.PerformingOrgsUsedInBoes; } }

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
			// Initialize the dictionary
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

				// Add the collected key-value pairs to the dictionary
				if (keyValuePairs.Any())
				{
					this.LaborTypesMappingWithCustomFieldsValuesAndContainerIds[resource.Id] = keyValuePairs;
				}

			}
		}
	}
}
