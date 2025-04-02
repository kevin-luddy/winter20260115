// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export.BOE
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using IES.Common;

	/// <summary>
	/// Export BOE to Word Request
	/// </summary>
	public class ExportBoeWordRequestViewModel
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public ExportBoeWordRequestViewModel()
		{ }

		/// <summary>
		/// Constructor from BOEExportInputs
		/// </summary>
		/// <param name="exportInputs">Export Inputs</param>
		public ExportBoeWordRequestViewModel(BOEExportInputs exportInputs)
		{
			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			this.AllWorkspaceBoes = exportInputs.AllWorkspaceBoes.ToList();
			this.AssignedBoeIdsAndCustomFieldValuesMapping = exportInputs.AssignedBoeIdsAndCustomFieldValuesMapping.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(x => new CustomFieldGrouping { CustomField = x.Value, CustomFieldValue = x.Key }).ToList());
			this.BoeIdsAndLastUserToSubmitThemForApprovalMapping = exportInputs.BoeIdsAndLastUserToSubmitThemForApprovalMapping;
			this.BoeMappingWithApproverResponses = exportInputs.BoeMappingWithApproverResponses.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); ;
			this.Boes = exportInputs.Boes.ToList();
			this.Clins = exportInputs.Clins.ToList();
			this.CustomFields = exportInputs.CustomFields.ToList();
			this.CustomFieldValues = exportInputs.CustomFieldValues.ToList();
			this.EscalationRates = exportInputs.EscalationRates.ToList();
			this.GetUserDataForBoesForWs = exportInputs.GetUserDataForBoesForWs.ToList();
			this.LaborTypesMappingWithCustomFieldsValuesAndContainerIds = exportInputs.LaborTypesMappingWithCustomFieldsValuesAndContainerIds;
			this.LocationsUsedByTrips = exportInputs.LocationsUsedByTrips.ToList();
			this.Materials = exportInputs.Materials.ToList();
			this.MiscTravelRatesForTravelTrips = exportInputs.MiscTravelRatesForTravelTrips.ToList();
			this.MOQTypes = exportInputs.MOQTypes.ToList();
			this.Odcs = exportInputs.Odcs.ToList();
			this.PerDiemsForTravelTrips = exportInputs.PerDiemsForTravelTrips.ToList();
			this.PerformingOrgsForWsList = exportInputs.PerformingOrgsForWsList.ToList();
			this.PerformingOrgsUsedInBoes = exportInputs.PerformingOrgsUsedInBoes.ToList();
			this.ResourcesForSystemResourceListId = exportInputs.ResourcesForSystemResourceListId.ToList();
			this.ResourcesForWsResourceListId = exportInputs.ResourcesForWsResourceListId.ToList();
			this.ResourcesUsedInWsBoes = exportInputs.ResourcesUsedInWsBoes.ToList();
			this.RTETemplatesOverrides = exportInputs.RTETemplatesOverrides.ToList();
			this.SummarizeByCustomField = exportInputs.SummarizeByCustomField;
			this.TaskElements = exportInputs.TaskElements.ToList();
			this.TaskElementsMappingWithCustomFieldsValuesAndContainerIds = exportInputs.TaskElementsMappingWithCustomFieldsValuesAndContainerIds;
			this.Travels = exportInputs.Travels.ToList();
			this.TravelTrips = exportInputs.TravelTrips.ToList();
			this.WbsElements = exportInputs.WbsElements.ToList();
			this.Workspace = exportInputs.Workspace;
			this.WorkspaceHistory = exportInputs.FullWorkspace.WorkspaceHistory.ToList();
			this.WorkspaceVariables = exportInputs.WorkspaceVariables.ToList();
		}

		/// <summary>
		/// Selected BOEs to export
		/// </summary>
		public List<BoeCustomReportComponent> SelectedComponents { get; set; }

		/// <summary>
		/// Should this use Custom Exporter or BOE Exporter
		/// </summary>
		public bool IsCustomExport { get; set; }

		/// <summary>
		/// Export Format Template
		/// </summary>
		public WorkspaceExportFormatDTO ExportFormatDTO { get; set; }

		/// <summary>
		/// BOE Export Models
		/// </summary>
		public List<BOEExportModelView> BoeExportModelViews { get; set; }

		/// <summary>
		/// BOE Summary Grid data
		/// </summary>
		public List<BOESummaryGridModelView> BoeSummaryGridModelViews { get; set; }

		/// <summary>
		/// If true, break out the output into separate files and return zip file
		/// </summary>
		public bool SegmentedOutput { get; set; }

		/// <summary>
		/// Gets or sets all of the RTE Custom Template Overrides.
		/// </summary>
		public List<RTECustomTemplateQuestionAnswerModelView> RTETemplatesOverrides { get; set; }

		/// <summary>
		/// Gets all of the boes in the workspace.
		/// </summary>
		public List<BoeDTO> AllWorkspaceBoes { get; set; }

		/// <summary>
		/// Gets all Travel Trips for the workspace.
		/// </summary>
		public List<TripDTO> TravelTrips { get; set; }

		/// <summary>
		/// Gets all Per Diems for all Travel Trips for the workspace.
		/// </summary>
		public List<PerDiemDTO> PerDiemsForTravelTrips { get; set; }

		/// <summary>
		/// Gets all Miscellaneous travel rates for all Travel Trips for the workspace.
		/// </summary>
		public List<MiscTravelRateDTO> MiscTravelRatesForTravelTrips { get; set; }

		/// <summary>
		/// Gets all Escalation Rates for the workspace.
		/// </summary>
		public List<EscalationRatesDTO> EscalationRates { get; set; }

		/// <summary>
		/// Gets the task elements mapping with custom fields values and container ids.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<int, ICollection<KeyValuePair<int, int>>> TaskElementsMappingWithCustomFieldsValuesAndContainerIds { get; set; }

		/// <summary>
		/// Gets a mapping of all custom field values and custom fields to Boes that use them, for the entire WS
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<int, List<CustomFieldGrouping>> AssignedBoeIdsAndCustomFieldValuesMapping { get; set; }

		/// <summary>
		///// Gets the labor types mapping with custom fields values and container ids.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<int, ICollection<KeyValuePair<int, int>>> LaborTypesMappingWithCustomFieldsValuesAndContainerIds { get; set; }

		/// <summary>
		/// Gets the resources for ws resource list identifier.
		/// </summary>
		public List<ResourceDTO> ResourcesForWsResourceListId { get; set; }

		/// <summary>
		/// Gets the resources for system resource list identifier.
		/// </summary>
		public List<ResourceDTO> ResourcesForSystemResourceListId { get; set; }

		/// <summary>
		/// Workspace History
		/// </summary>
		public List<WorkspaceHistoryDTO> WorkspaceHistory { get; set; }

		/// <summary>
		/// Gets the workspace variables.
		/// </summary>
		public List<WorkspaceVariableDTO> WorkspaceVariables { get; set; }

		/// <summary>
		/// Gets the boes.
		/// </summary>
		public List<BoeDTO> Boes { get; set; }

		/// <summary>
		/// Gets the task elements.
		/// </summary>
		public List<BoeTaskElementDTO> TaskElements { get; set; }

		/// <summary>
		/// Gets the resources used in ws boes.
		/// </summary>
		public List<ResourceDTO> ResourcesUsedInWsBoes { get; set; }

		/// <summary>
		/// Gets the workspace.
		/// </summary>
		public WorkspaceDTO Workspace { get; set; }

		/// <summary>
		/// Gets the custom fields.
		/// </summary>
		public List<CustomFieldDTO> CustomFields { get; set; }

		/// <summary>
		/// Gets the custom field values.
		/// </summary>
		public List<CustomFieldValueDTO> CustomFieldValues { get; set; }

		/// <summary>
		/// Gets the clins.
		/// </summary>
		public List<ClinDTO> Clins { get; set; }

		/// <summary>
		/// Gets the WBS elements.
		/// </summary>
		public List<WbsDTO> WbsElements { get; set; }

		/// <summary>
		/// Gets the boe mapping with approver responses.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<int, ICollection<BoeApproverResponseDTO>> BoeMappingWithApproverResponses { get; set; }

		/// <summary>
		/// Gets the get user data for boes for ws.
		/// </summary>
		public List<UserDTO> GetUserDataForBoesForWs { get; set; }

		/// <summary>
		/// Gets the performing orgs used in boes.
		/// </summary>
		public List<PerformingOrgDTO> PerformingOrgsUsedInBoes { get; set; }

		/// <summary>
		/// Gets the performing orgs for ws list.
		/// </summary>
		public List<PerformingOrgDTO> PerformingOrgsForWsList { get; set; }

		/// <summary>
		/// Gets the boe ids and last user to submit them for approval mapping.
		/// </summary>
		public Dictionary<int, UserDTO> BoeIdsAndLastUserToSubmitThemForApprovalMapping { get; set; }

		/// <summary>
		/// Gets the odcs.
		/// </summary>
		public List<OtherDirectCostDTO> Odcs { get; set; }

		/// <summary>
		/// Gets the travels.
		/// </summary>
		public List<TravelDTO> Travels { get; set; }

		/// <summary>
		/// Locations used by trips
		/// </summary>
		public List<LocationDTO> LocationsUsedByTrips { get; set; }

		/// <summary>
		/// Gets the materials.
		/// </summary>
		public List<MaterialDTO> Materials { get; set; }

		/// <summary>
		/// Gets/Sets the "summarize by" parameter (custom field name at the Resource Types level) 
		/// used by the All BOEs report when running with the special format template.
		/// </summary>
		public string SummarizeByCustomField { get; set; }

		/// <summary>
		/// Gets the MOQ Types
		/// </summary>
		public List<MoqTypeSelection> MOQTypes { get; set; }
	}
}
