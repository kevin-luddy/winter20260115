namespace GenBOE.DataBridge.Core.DTO.Export.BOE
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Dapper;
	using GenBOE.DataBridge.Core.ModelView;
	using GenBOE.DataBridge.Core.WorkspaceExportFormat;
	using GenTRAC.DataBridge.Core.DTO.User;
	using IES.Common.Core.PickList;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Inputs used for the BOE Exporters.
	/// </summary>
	public class BOEExportInputs
	{
		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger logger;

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

		///// <summary>
		///// Gets the full workspace, only to be used when rewriting is impossible.
		///// </summary>
		//public FullWorkspace FullWorkspace { get; }

		/// <summary>
		/// Gets all Travel Trips for the workspace.
		/// </summary>
		public IReadOnlyCollection<TripDTO> TravelTrips { get; set; } 

		/// <summary>
		/// Gets all Per Diems for all Travel Trips for the workspace.
		/// </summary>
		public IReadOnlyCollection<PerDiemDTO> PerDiemsForTravelTrips { get; set; }

		/// <summary>
		/// Gets all Miscellaneous travel rates for all Travel Trips for the workspace.
		/// </summary>
		public IReadOnlyCollection<MiscTravelRateDTO> MiscTravelRatesForTravelTrips { get; set; }

		/// <summary>
		/// Gets all Escalation Rates for the workspace.
		/// </summary>
		public IReadOnlyCollection<EscalationRatesDTO> EscalationRates { get; set; }

		/// <summary>
		/// Gets the task elements mapping with custom fields values and container ids.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<int, ICollection<KeyValuePair<int, int>>> TaskElementsMappingWithCustomFieldsValuesAndContainerIds { get; set; }

		/// <summary>
		/// Gets a mapping of all custom field values and custom fields to Boes that use them, for the entire WS
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> AssignedBoeIdsAndCustomFieldValuesMapping { get; set; }

		/// <summary>
		///// Gets the labor types mapping with custom fields values and container ids.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<int, ICollection<KeyValuePair<int, int>>> LaborTypesMappingWithCustomFieldsValuesAndContainerIds { get; set; }

		/// <summary>
		/// Gets the resources for ws resource list identifier.
		/// </summary>
		public IReadOnlyCollection<ResourceDTO> ResourcesForWsResourceListId { get; set; }

		/// <summary>
		/// Gets the resources for system resource list identifier.
		/// </summary>
		public IReadOnlyCollection<ResourceDTO> ResourcesForSystemResourceListId { get; set; }

		/// <summary>
		/// Gets the workspace variables.
		/// </summary>
		public IReadOnlyCollection<WorkspaceVariableDTO> WorkspaceVariables { get; set; }

		/// <summary>
		/// Gets the boes.
		/// </summary>
		public IReadOnlyCollection<BoeDTO> Boes { get; set; }

		/// <summary>
		/// Gets the task elements.
		/// </summary>
		public IReadOnlyCollection<BoeTaskElementDTO> TaskElements { get; set; }

		/// <summary>
		/// Gets the resources used in ws boes.
		/// </summary>
		public IReadOnlyCollection<ResourceDTO> ResourcesUsedInWsBoes { get; set; }

		/// <summary>
		/// Gets the workspace export formats.
		/// </summary>
		public IReadOnlyCollection<WorkspaceExportFormatDTO> WorkspaceExportFormats { get; set; }

		/// <summary>
		/// Gets the workspace.
		/// </summary>
		public WorkspaceDTO Workspace { get; }

		/// <summary>
		/// Gets the custom fields.
		/// </summary>
		public IReadOnlyCollection<CustomFieldDTO> CustomFields { get; set; }

		/// <summary>
		/// Gets the custom field values.
		/// </summary>
		public IReadOnlyCollection<CustomFieldValueDTO> CustomFieldValues { get; set; }

		/// <summary>
		/// Gets the clins.
		/// </summary>
		public IReadOnlyCollection<ClinDTO> Clins { get; set; }

		/// <summary>
		/// Clins belonging to the Workspace without Multi Clin
		/// </summary>
		public IReadOnlyCollection<ClinDTO> ClinsNoMultiClin { get; set; }

		/// <summary>
		/// Gets the WBS elements.
		/// </summary>
		public IReadOnlyCollection<WbsDTO> WbsElements { get; set; }

		/// <summary>
		/// Wbs Elements belonging to the Workspace without Multi
		/// </summary>
		public IReadOnlyCollection<WbsDTO> WbsElementsNoMultiWbs { get; set; }

		/// <summary>
		/// Gets the boe mapping with approver responses.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IDictionary<int, ICollection<BoeApproverResponseDTO>> BoeMappingWithApproverResponses { get; set; }

		/// <summary>
		/// Gets the get user data for boes for ws.
		/// </summary>
		public IReadOnlyCollection<UserDTO> GetUserDataForBoesForWs { get; set; }

		/// <summary>
		/// Gets the performing orgs used in boes.
		/// </summary>
		public IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsUsedInBoes { get; set; }

		/// <summary>
		/// Gets the performing orgs for ws list.
		/// </summary>
		public IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsForWsList { get; set; }

		/// <summary>
		/// Gets the boe ids and last user to submit them for approval mapping.
		/// </summary>
		public Dictionary<int, UserDTO> BoeIdsAndLastUserToSubmitThemForApprovalMapping { get; set; }

		/// <summary>
		/// Gets the odcs.
		/// </summary>
		public IReadOnlyCollection<OtherDirectCostDTO> Odcs { get; set; }

		/// <summary>
		/// Gets the travels.
		/// </summary>
		public IReadOnlyCollection<TravelDTO> Travels { get; set; }

		/// <summary>
		/// Gets the materials.
		/// </summary>
		public IReadOnlyCollection<MaterialDTO> Materials { get; set; }

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
		public IReadOnlyCollection<MoqTypeSelection> MOQTypes { get; set; }
	}
}
