namespace GenBOE.DataBridge.Core.DTO.Export.BOE
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Dapper;
	using GenBOE.DataBridge.Core;
	using GenBOE.DataBridge.Core.DTO.FullObjects;
	using GenBOE.DataBridge.Core.DTO.Travel;
	using GenBOE.DataBridge.Core.ModelView;
	using GenTRAC.DataBridge.Core.DTO.User;
	using IES.Common.Core.PickList;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Inputs used for the BOE Exporters.
	/// </summary>
	public class BOEExportInputs
	{
		/// <summary>
		/// Constructor from Request ViewModel
		/// </summary>
		/// <param name="requestModel"></param>
		public BOEExportInputs(ExportBoeWordRequestViewModel requestModel)
		{
			this.AllWorkspaceBoes = requestModel.AllWorkspaceBoes;
			this.AssignedBoeIdsAndCustomFieldValuesMapping = requestModel.AssignedBoeIdsAndCustomFieldValuesMapping;
			this.BoeIdsAndLastUserToSubmitThemForApprovalMapping = requestModel.BoeIdsAndLastUserToSubmitThemForApprovalMapping;
			this.BoeMappingWithApproverResponses = requestModel.BoeMappingWithApproverResponses;
			this.Boes = requestModel.Boes;
			this.Clins = requestModel.Clins;
			this.ClinsNoMultiClin = requestModel.ClinsNoMultiClin;
			this.ContractTypes = requestModel.ContractTypes;
			this.CustomFields = requestModel.CustomFields;
			this.CustomFieldValues = requestModel.CustomFieldValues;
			this.EscalationRates = requestModel.EscalationRates;
			this.GetUserDataForBoesForWs = requestModel.GetUserDataForBoesForWs;
			this.LaborTypesMappingWithCustomFieldsValuesAndContainerIds = requestModel.LaborTypesMappingWithCustomFieldsValuesAndContainerIds;
			this.LocationsUsedByTrips = requestModel.LocationsUsedByTrips;
			this.Materials = requestModel.Materials;
			this.MiscTravelRatesForTravelTrips = requestModel.MiscTravelRatesForTravelTrips;
			this.MOQTypes = requestModel.MOQTypes;
			this.Odcs = requestModel.Odcs;
			this.PerDiemsForTravelTrips = requestModel.PerDiemsForTravelTrips;
			this.PerformingOrgsForWsList = requestModel.PerformingOrgsForWsList;
			this.PerformingOrgsUsedInBoes = requestModel.PerformingOrgsUsedInBoes;
			this.ResourcesForSystemResourceListId = requestModel.ResourcesForSystemResourceListId;
			this.ResourcesForWsResourceListId = requestModel.ResourcesForWsResourceListId; 
			this.ResourcesUsedInWsBoes = requestModel.ResourcesUsedInWsBoes;
			this.RTETemplatesOverrides = requestModel.RTETemplatesOverrides;
			this.SummarizeByCustomField = requestModel.SummarizeByCustomField;
			this.TaskElements = requestModel.TaskElements;
			this.TaskElementsMappingWithCustomFieldsValuesAndContainerIds = requestModel.TaskElementsMappingWithCustomFieldsValuesAndContainerIds;
			this.Travels = requestModel.Travels;
			this.TravelTrips = requestModel.TravelTrips;
			this.WbsElements = requestModel.WbsElements;
			this.WbsElementsNoMultiWbs = requestModel.WbsElementsNoMultiWbs;
			this.Workspace = requestModel.Workspace;
			this.WorkspaceExportFormats = requestModel.WorkspaceExportFormats;
			this.WorkspaceVariables = requestModel.WorkspaceVariables;
		}

		/// <summary>
		/// Gets or sets all of the RTE Custom Template Overrides.
		/// </summary>
		public IReadOnlyCollection<RTECustomTemplateQuestionAnswerModelView> RTETemplatesOverrides { get; private set; }

		/// <summary>
		/// Gets all of the boes in the workspace.
		/// </summary>
		public IReadOnlyCollection<BoeDTO> AllWorkspaceBoes { get; private set; }

		/// <summary>
		/// Gets all Travel Trips for the workspace.
		/// </summary>
		public IReadOnlyCollection<TripDTO> TravelTrips { get; private set; } 

		/// <summary>
		/// Gets all Per Diems for all Travel Trips for the workspace.
		/// </summary>
		public IReadOnlyCollection<PerDiemDTO> PerDiemsForTravelTrips { get; private set; }

		/// <summary>
		/// Gets all Miscellaneous travel rates for all Travel Trips for the workspace.
		/// </summary>
		public IReadOnlyCollection<MiscTravelRateDTO> MiscTravelRatesForTravelTrips { get; private set; }

		/// <summary>
		/// Gets all Escalation Rates for the workspace.
		/// </summary>
		public IReadOnlyCollection<EscalationRatesDTO> EscalationRates { get; private set; }

		/// <summary>
		/// Gets the task elements mapping with custom fields values and container ids.
		/// </summary>
		public IDictionary<int, ICollection<KeyValuePair<int, int>>> TaskElementsMappingWithCustomFieldsValuesAndContainerIds { get; private set; }

		/// <summary>
		/// Gets a mapping of all custom field values and custom fields to Boes that use them, for the entire WS
		/// </summary>
		public IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> AssignedBoeIdsAndCustomFieldValuesMapping { get; private set; }

		/// <summary>
		///// Gets the labor types mapping with custom fields values and container ids.
		/// </summary>
		public IDictionary<int, ICollection<KeyValuePair<int, int>>> LaborTypesMappingWithCustomFieldsValuesAndContainerIds { get; private set; }

		/// <summary>
		/// Gets the resources for ws resource list identifier.
		/// </summary>
		public IReadOnlyCollection<ResourceDTO> ResourcesForWsResourceListId { get; private set; }

		/// <summary>
		/// Gets the resources for system resource list identifier.
		/// </summary>
		public IReadOnlyCollection<ResourceDTO> ResourcesForSystemResourceListId { get; private set; }

		/// <summary>
		/// Gets the workspace variables.
		/// </summary>
		public IReadOnlyCollection<WorkspaceVariableDTO> WorkspaceVariables { get; private set; }

		/// <summary>
		/// Gets the boes.
		/// </summary>
		public IReadOnlyCollection<BoeDTO> Boes { get; private set; }

		/// <summary>
		/// Gets the task elements.
		/// </summary>
		public IReadOnlyCollection<BoeTaskElementDTO> TaskElements { get; private set; }

		/// <summary>
		/// Gets the resources used in ws boes.
		/// </summary>
		public IReadOnlyCollection<ResourceDTO> ResourcesUsedInWsBoes { get; private set; }

		/// <summary>
		/// Gets the workspace export formats.
		/// </summary>
		public IReadOnlyCollection<WorkspaceExportFormatDTO> WorkspaceExportFormats { get; private set; }

		/// <summary>
		/// Gets the workspace.
		/// </summary>
		public WorkspaceDTO Workspace { get; private set; }

		/// <summary>
		/// Gets the custom fields.
		/// </summary>
		public IReadOnlyCollection<CustomFieldDTO> CustomFields { get; private set; }

		/// <summary>
		/// Gets the custom field values.
		/// </summary>
		public IReadOnlyCollection<CustomFieldValueDTO> CustomFieldValues { get; private set; }

		/// <summary>
		/// Gets the clins.
		/// </summary>
		public IReadOnlyCollection<ClinDTO> Clins { get; private set; }

		/// <summary>
		/// Clins belonging to the Workspace without Multi Clin
		/// </summary>
		public IReadOnlyCollection<ClinDTO> ClinsNoMultiClin { get; private set; }

		/// <summary>
		/// Gets the WBS elements.
		/// </summary>
		public IReadOnlyCollection<WbsDTO> WbsElements { get; private set; }

		/// <summary>
		/// Wbs Elements belonging to the Workspace without Multi
		/// </summary>
		public IReadOnlyCollection<WbsDTO> WbsElementsNoMultiWbs { get; private set; }

		/// <summary>
		/// Gets the boe mapping with approver responses.
		/// </summary>
		public IDictionary<int, ICollection<BoeApproverResponseDTO>> BoeMappingWithApproverResponses { get; private set; }

		/// <summary>
		/// Gets the get user data for boes for ws.
		/// </summary>
		public IReadOnlyCollection<UserDTO> GetUserDataForBoesForWs { get; private set; }

		/// <summary>
		/// Gets the performing orgs used in boes.
		/// </summary>
		public IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsUsedInBoes { get; private set; }

		/// <summary>
		/// Gets the performing orgs for ws list.
		/// </summary>
		public IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsForWsList { get; private set; }

		/// <summary>
		/// Gets the boe ids and last user to submit them for approval mapping.
		/// </summary>
		public IDictionary<int, UserDTO> BoeIdsAndLastUserToSubmitThemForApprovalMapping { get; private set; }

		/// <summary>
		/// Gets the odcs.
		/// </summary>
		public IReadOnlyCollection<OtherDirectCostDTO> Odcs { get; private set; }

		/// <summary>
		/// Gets the travels.
		/// </summary>
		public IReadOnlyCollection<TravelDTO> Travels { get; private set; }

		/// <summary>
		/// Locations used by trips
		/// </summary>
		public IReadOnlyCollection<LocationDTO> LocationsUsedByTrips { get; private set; }

		/// <summary>
		/// Gets the materials.
		/// </summary>
		public IReadOnlyCollection<MaterialDTO> Materials { get; private set; }

		/// <summary>
		/// Gets/Sets the "summarize by" parameter (custom field name at the Resource Types level) 
		/// used by the All BOEs report when running with the special format template.
		/// </summary>
		public string SummarizeByCustomField { get; private set; }

		/// <summary>
		/// Gets or sets the contract types.
		/// </summary>
		public ICollection<PickListDto> ContractTypes { get; private set; }

		/// <summary>
		/// Gets the MOQ Types
		/// </summary>
		public IReadOnlyCollection<MoqTypeSelection> MOQTypes { get; private set; }

		/// <summary>
		/// Clears the RTE Overrides List
		/// </summary>
		public void ClearRteOverrides()
		{
			this.RTETemplatesOverrides = new List<RTECustomTemplateQuestionAnswerModelView>().AsReadOnly();
		}
	}
}
