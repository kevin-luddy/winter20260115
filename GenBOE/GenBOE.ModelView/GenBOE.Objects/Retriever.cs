// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.classes;

	public class Retriever : IRetriever
	{
		#region Fields and Constructors

		private readonly IClinDTODataLoader clinLoader;
		private readonly IWbsDTODataLoader wbsLoader;
		private readonly IWorkspaceDTODataLoader workspaceLoader;
		private readonly IBoeDTODataLoader boeLoader;
		private readonly IWorkspaceVariableDTODataLoader workspaceVariableLoader;
		private readonly IBOECommentDTODataLoader boeCommentLoader;
		private readonly IWorkspaceHistoryDTODataLoader workspaceHistoryLoader;
		private readonly IMaterialDTODataLoader materialLoader;
		private readonly IResourceTypeLoader resourceTypeLoader;
		private readonly IBoeTaskElementDTODataLoader taskElementLoader;
		private readonly IOtherDirectCostDTODataLoader otherDirectCostLoader;
		private	readonly IResourceDTODataLoader ResourceLoader;
		private readonly ITravelDTODataLoader travelLoader;
		private readonly IBoeApproverResponseDTODataLoader approverResponseLoader;
		private readonly IWorkspaceExportFormatDTODataLoader workspaceExportFormatLoader;
		private readonly ICustomFieldDTODataLoader customFieldLoader;
		private readonly IWorkspaceVersionMetaDataDTODataLoader workspaceVersionMetaDataLoader;
		private readonly IProPricerDTODataLoader proPricerLoader;
		private readonly IPerformingOrgDTODataLoader perfOrgLoader;
		private readonly IPerformingOrgListDTODataLoader performingOrgListLoader;
		private readonly IEscalationRatesDTOLoader escalationRateLoader;
		private readonly ITripDTODataLoader tripDataLoader;
		private readonly IPerDiemDTODataLoader perDiemLoader;
		private readonly IMiscTravelRateDTOLoader miscTravelRateLoader;
		private readonly IUserDTODataLoader userDataLoader;
		private readonly ILocationDTODataLoader locationLoader;
		private readonly ICustomFieldValueDTODataLoader customFieldValueLoader;
		private readonly IBOEHistoryDTODataLoader boeHistoryLoader;
		private readonly ITMResourceRateDTODataLoader tmResourceRateLoader;
		private readonly IProjectMapDataLoader projectMapDataLoader;
		private readonly IRteTemplateDataLoader rteTemplateDataLoader;
		private readonly IMoqTypeDataLoader moqTypeDataLoader;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="clinLoader">Clin Loader</param>
		/// <param name="wbsLoader">Wbs Loader</param>
		/// <param name="workspaceLoader">Workspace Loader</param>
		/// <param name="boeLoader">Boe Loader</param>
		/// <param name="workspaceVariableLoader">Workspace Variable Loader.</param>
		/// <param name="workspaceHistoryDTODataLoader">Workspace History Loader</param>
		/// <param name="materialLoader">Material Loader</param>
		/// <param name="boeCommentLoader">Boe Comment Loader</param>
		/// <param name="resourceTypeLoader">Resource Loader</param>
		/// <param name="taskElementLoader">Task Element Loader</param>
		/// <param name="otherDirectCostLoader">Other Direct Cost Loader</param>
		/// <param name="travelLoader">Travel Element loader.</param>
		/// <param name="approverResponseLoader">Boe Approver Response loader.</param>
		/// <param name="workspaceExportFormatLoader">The workspace export format loader.</param>
		/// <param name="customFieldLoader">Custom Field loader.</param>
		/// <param name="workspaceVersionMetaDataLoader">The workspace version meta data loader.</param>
		/// <param name="proPricerLoader">The pro pricer loader.</param>
		/// <param name="perfOrgLoader">Performing Organization loader.</param>
		/// <param name="performingOrgListLoader">Performing Organization list loader.</param>
		/// <param name="escalationRateLoader">Escalation Rate loader.</param>
		/// <param name="tripDataLoader">Travel trip loader.</param>
		/// <param name="perDiemLoader">Travel trip Per Diem loader.</param>
		/// <param name="miscTravelRateLoader">Misc Travel Rate loader.</param>
		/// <param name="userDataLoader">User data Loader.</param>
		/// <param name="locationLoader">The location loader.</param>
		/// <param name="customFieldValueLoader">The custom field value loader.</param>
		/// <param name="boeHistoryLoader">The boe history loader.</param>
		/// <param name="tmResourceRateLoader">T&amp;M Resource Rate Loader.</param>
		/// <param name="projectMapDataLoader">The project map data loader.</param>
		public Retriever(IClinDTODataLoader clinLoader, IWbsDTODataLoader wbsLoader, IWorkspaceDTODataLoader workspaceLoader, IBoeDTODataLoader boeLoader,
			IWorkspaceVariableDTODataLoader workspaceVariableLoader, 
			IWorkspaceHistoryDTODataLoader workspaceHistoryDTODataLoader, 
			IMaterialDTODataLoader materialLoader, IBOECommentDTODataLoader boeCommentLoader, IResourceTypeLoader resourceTypeLoader,
			IBoeTaskElementDTODataLoader taskElementLoader, IOtherDirectCostDTODataLoader otherDirectCostLoader, IResourceDTODataLoader resourceLoader,
			ITravelDTODataLoader travelLoader, IBoeApproverResponseDTODataLoader approverResponseLoader, IWorkspaceExportFormatDTODataLoader workspaceExportFormatLoader,
			ICustomFieldDTODataLoader customFieldLoader, IWorkspaceVersionMetaDataDTODataLoader workspaceVersionMetaDataLoader, IProPricerDTODataLoader proPricerLoader,
			IPerformingOrgDTODataLoader perfOrgLoader, IPerformingOrgListDTODataLoader performingOrgListLoader,
			IEscalationRatesDTOLoader escalationRateLoader, ITripDTODataLoader tripDataLoader, IPerDiemDTODataLoader perDiemLoader,
			IMiscTravelRateDTOLoader miscTravelRateLoader, IUserDTODataLoader userDataLoader, ILocationDTODataLoader locationLoader,
			ICustomFieldValueDTODataLoader customFieldValueLoader, IBOEHistoryDTODataLoader boeHistoryLoader, ITMResourceRateDTODataLoader tmResourceRateLoader,
			IProjectMapDataLoader projectMapDataLoader, IRteTemplateDataLoader rteTemplateDataLoader, IMoqTypeDataLoader moqTypeDataLoader)
		{
			this.clinLoader = clinLoader;
			this.wbsLoader = wbsLoader;
			this.workspaceLoader = workspaceLoader;
			this.boeLoader = boeLoader;
			this.workspaceVariableLoader = workspaceVariableLoader;
			this.boeCommentLoader = boeCommentLoader;
			this.workspaceHistoryLoader = workspaceHistoryDTODataLoader;
			this.materialLoader = materialLoader;
			this.resourceTypeLoader = resourceTypeLoader;
			this.taskElementLoader = taskElementLoader;
			this.otherDirectCostLoader = otherDirectCostLoader;
			this.ResourceLoader = resourceLoader;
			this.travelLoader = travelLoader;
			this.approverResponseLoader = approverResponseLoader;
			this.workspaceExportFormatLoader = workspaceExportFormatLoader;
			this.customFieldLoader = customFieldLoader;
			this.workspaceVersionMetaDataLoader = workspaceVersionMetaDataLoader;
			this.proPricerLoader = proPricerLoader;
			this.perfOrgLoader = perfOrgLoader;
			this.performingOrgListLoader = performingOrgListLoader;
			this.escalationRateLoader = escalationRateLoader;
			this.tripDataLoader = tripDataLoader;
			this.perDiemLoader = perDiemLoader;
			this.miscTravelRateLoader = miscTravelRateLoader;
			this.userDataLoader = userDataLoader;
			this.locationLoader = locationLoader;
			this.customFieldValueLoader = customFieldValueLoader;
			this.boeHistoryLoader = boeHistoryLoader;
			this.tmResourceRateLoader = tmResourceRateLoader;
			this.projectMapDataLoader = projectMapDataLoader;
			this.rteTemplateDataLoader = rteTemplateDataLoader;
			this.moqTypeDataLoader = moqTypeDataLoader;
		}

		#endregion

		#region Lists by Workspace Id

		/// <summary>
		/// Gets all the clins that belong to the workspace
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>Clins</returns>
		public ICollection<FullClin> GetClinsByWorkspaceId(int workspaceId)
		{
			List<FullClin> result = new List<FullClin>();
			ICollection<ClinDTO> rawClins = this.clinLoader.GetByWorkspaceId(workspaceId);

			if (rawClins != null && rawClins.Any())
			{
				foreach (ClinDTO clin in rawClins)
				{
					result.Add(new FullClin(clin));
				}
			}

			return result;
		}

		/// <summary>
		/// Returns all travel IDs for a given workspace.
		/// </summary>
		/// <param name="inWorkspaceID">Workspace Id</param>
		/// <param name="loadRteData">Indicate whether RTE data should be loaded automatically</param>
		/// <returns>Collection of travel dtos by workspace Id.</returns>
		public ICollection<TravelDTO> GetTravelByWorkspaceId(int inWorkspaceID, bool loadRteData)
		{
			return this.travelLoader.GetByWorkspaceId(inWorkspaceID, loadRteData);
		}

		/// <summary>
		/// Gets all travel element trips in the workspace.
		/// </summary>
		/// <param name="tripIds">Trip Ids.</param>
		/// <param name="workspace">Workspace.</param>
		/// <returns>Travel Trips for the workspace.</returns>
		public ICollection<TripDTO> GetTravelTripsForWorkspace(ICollection<int> tripIds, WorkspaceDTO workspace)
		{
			return this.tripDataLoader.GetByIds(tripIds, workspace);
		}

		/// <summary>
		/// Gets travel trip data by trip Id
		/// </summary>
		/// <param name="id">Trip Id</param>
		/// <param name="workspace">Workspace</param>
		/// <returns>Trip Dto</returns>
		public TripDTO GetTravelTripById(int id, WorkspaceDTO workspace)
		{
			return this.tripDataLoader.GetTripDTOByTripID(id, workspace);
		}

		/// <summary>
		/// Gets all per diems for travel element trips in the workspace.
		/// </summary>
		/// <param name="perDiemIds">Per Diem Ids.</param>
		/// <param name="workspace">Workspace.</param>
		/// <returns>Travel Trip per diems for the workspace.</returns>
		public ICollection<PerDiemDTO> GetTravelTripPerDiemsForWorkspace(ICollection<int> perDiemIds, WorkspaceDTO workspace)
		{
			return this.perDiemLoader.GetByIds(perDiemIds, workspace);
		}

		/// <summary>
		/// Gets all misc travel rates for travel element trips in the workspace.
		/// </summary>
		/// <param name="rateIds">Misc Travel Rate Ids.</param>
		/// <param name="workspace">Workspace.</param>
		/// <returns>Travel Trip per diems for the workspace.</returns>
		public ICollection<MiscTravelRateDTO> GetTravelTripMiscTravelRatesForWorkspace(ICollection<int> rateIds, WorkspaceDTO workspace)
		{
			return this.miscTravelRateLoader.GetByIds(rateIds, workspace);
		}


		/// <summary>
		/// Gets all Escalation rates for a workspace.
		/// </summary>
		/// <param name="workspace">Workspace.</param>
		/// <returns>Escalation rates for the workspace.</returns>
		public ICollection<EscalationRatesDTO> GetEscalationRatesByWorkspace(WorkspaceDTO workspace)
		{
			return this.escalationRateLoader.GetByWorkspace(workspace);
		}

		/// <summary>
		/// Gets all Custom Fields for the given workspace id.
		/// </summary>
		/// <param name="workspaceId">Workspace Id.</param>
		/// <returns>All CustomField Dto for a workspace.</returns>
		public ICollection<CustomFieldDTO> GetCustomFieldsByWorkspaceId(int workspaceId)
		{
			return this.customFieldLoader.GetByWorkspaceId(workspaceId);
		}

		/// <summary>
		/// Gets custom field values based on the custom field ids
		/// </summary>
		/// <param name="customFieldIds"></param>
		/// <param name="workspaceId">The workspace Id.</param>
		/// <returns></returns>
		public ICollection<CustomFieldValueDTO> GetCustomFieldValuesByFieldIds(ICollection<int> customFieldIds, int workspaceId)
		{
			return this.customFieldValueLoader.GetCustomFieldValueDTOsByCustomFieldIds(customFieldIds);
		}

		/// <summary>
		/// Gets a mapping of task element ids and custom field ids/container ids
		/// </summary>
		/// <param name="taskElementIds">Task Elements</param>
		/// <returns>Data</returns>
		public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIdsByTaskElementIds(Collection<int> taskElementIds)
		{
			return this.customFieldValueLoader.GetCustomFieldValueIDsContainerIDsByTaskElementIDs(taskElementIds);
		}

		/// <summary>
		/// Gets a mapping of MOQ Type Table ids and custom field ids/container ids
		/// </summary>
		/// <param name="moqTypeTableIds">MOQ Type Table IDs</param>
		/// <returns>Data</returns>
		public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIdsByMoqTypeTableIds(Collection<int> moqTypeTableIds)
		{
			return this.customFieldValueLoader.GetCustomFieldValueIDsContainerIDsByMoqTypeTableIds(moqTypeTableIds);
		}

		/// <summary>
		/// Gets a mapping of labor type ids and custom field ids/container ids
		/// </summary>
		/// <param name="laborTypeIds">Labor Types</param>
		/// <returns>Data</returns>
		public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIdsByLaborTypeIds(Collection<int> laborTypeIds)
		{
			return this.customFieldValueLoader.GetCustomFieldValueIDsContainerIDsByLaborTypeIDs(laborTypeIds);
		}
		/// <summary>
		/// Gets a mapping of travel element ids and custom field ids/container ids
		/// </summary>
		/// <param name="travelElementIds">Travel Elements</param>
		/// <returns>Data</returns>
		public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIdsByTravelElementIds(Collection<int> travelElementIds)
		{
			return this.customFieldValueLoader.GetCustomFieldValueIDsContainerIDsByTravelElementIDs(travelElementIds);
		}
		/// <summary>
		/// Gets a mapping of travel trip ids and custom field ids/container ids
		/// </summary>
		/// <param name="travelTripIds">Travel Trip </param>
		/// <returns>Data</returns>
		public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIdsByTravelTripsIds(Collection<int> travelTripIds)
		{
			return this.customFieldValueLoader.GetCustomFieldValueIDsContainerIDsByTravelTripIDs(travelTripIds);
		}
		/// <summary>
		/// Gets all Wbs elements that belong to the workspace
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>Wbs Elements</returns>
		public ICollection<FullWbs> GetFullWbsElementsByWorkspaceId(int workspaceId)
		{
			List<FullWbs> result = new List<FullWbs>();
			ICollection<WbsDTO> rawWbs = this.wbsLoader.GetByWorkspaceId(workspaceId);

			if (rawWbs != null && rawWbs.Any())
			{
				foreach(WbsDTO wbs in rawWbs)
				{
					result.Add(new FullWbs(wbs));
				}
			}

			return result;
		}

		/// <summary>
		/// Gets all workspace variables in a workspace.
		/// </summary>
		/// <param name="workspaceId">Workspace Id.</param>
		/// <returns>All workspace variables in a workspace.</returns>
		public ICollection<WorkspaceVariableDTO> GetWorkspaceVariableDTOsByWorkspaceId(int workspaceId)
		{
			return this.workspaceVariableLoader.GetByWorkspaceID(workspaceId);
		}

		/// <summary>
		/// All workspace variable Ids where the BOE is part of the sum of BOEs for the variable.
		/// </summary>
		/// <param name="boeId">Boe Id.</param>
		/// <returns>All workspace variable ids for the Boe where it is summed.</returns>
		public ICollection<int> GetWorkspaceVariableIdsForBoe(int boeId)
		{
			return this.boeLoader.GetWorkspaceVariableIdsByBoeId(boeId);
		}

		/// <summary>
		/// Get Boes By Clin Id
		/// </summary>
		/// <param name="clinId">Clin Id</param>
		/// <returns>Corresponding Boes</returns>
		public ICollection<FullBoe> GetFullBoesByClinId(int clinId)
		{
			ICollection<BoeDTO> boes = this.boeLoader.GetByClinIds(new List<int>() { clinId });

			List<FullBoe> result = new List<FullBoe>();

			foreach (BoeDTO boe in boes)
			{
				result.Add(new FullBoe(boe));
			}

			return result;
		}

		/// <summary>
		/// Get Boe Comments By Boe Id
		/// </summary>
		/// <param name="boeId">Boe Id</param>
		/// <returns>Corresponding Boe Comments</returns>
		public ICollection<BOECommentDTO> GetCommentsByBoeId(int boeId)
		{
			return this.boeCommentLoader.GetByBoeId(boeId);
		}

		/// <summary>
		/// Gets workspace history by workspace Id.
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>Workspace History DTOs</returns>
		public ICollection<WorkspaceHistoryDTO> GetWorkspaceHistoryByWorkspaceId(int workspaceId)
		{
			return this.workspaceHistoryLoader.GetWorkspaceHistory(workspaceId);
		}

		/// <summary>
		/// Get ProPricer Exports by workspace Id.  This includes both system and workspace type exports.
		/// </summary>
		/// <param name="workspaceId">Workspace Id.</param>
		/// <returns>ProPricer Export Dtos.</returns>
		public ICollection<ProPricerDTO> GetProPricerExportsByWorkspaceId(int workspaceId)
		{
			return this.proPricerLoader.GetByWorkspaceId(workspaceId);
		}

		/// <summary>
		/// Gets all Materials by BOE Id,
		/// </summary>
		/// <param name="inBoeID">BOE Id.</param>
		/// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
		/// <returns>MaterialDTOs by BOE Id.</returns>
		public ICollection<MaterialDTO> GetMaterialCollectionByBoeID(int inBoeID, bool includeRTEFields)
		{
			return this.GetMaterialsByBoeIds(new Collection<int>() { inBoeID }, includeRTEFields);
		}

		/// <summary>
		/// Gets all Travels by BOE Id,
		/// </summary>
		/// <param name="inBoeID">BOE Id.</param>
		/// <param name="loadRteData">Indicate whether RTE data should be loaded automatically</param>
		/// <returns>TravelDTOs by BOE Id.</returns>
		public ICollection<TravelDTO> GetTravelCollectionByBoeID(int inBoeID, bool loadRteData)
		{
			ICollection<TravelDTO> travels = this.travelLoader.GetByBoeIds(new Collection<int>() { inBoeID }, loadRteData);
			return travels;
		}
		
		#endregion

		#region Individual Dtos

		/// <summary>
		/// Gets the currently active user.
		/// </summary>
		public UserDTO GetCurrentActiveUser()
		{
			return userDataLoader.GetUserForActiveUser();
		}

		/// <summary>
		/// Gets Workspace Dto By Workspace Id
		/// </summary>
		/// <param name="workspaceId">Id</param>
		/// <returns>Workspace</returns>
		public WorkspaceDTO GetWorkspaceById(int workspaceId)
		{
			return this.workspaceLoader.GetById(workspaceId);
		}

		/// <summary>
		/// Gets Full Workspace Object By Workspace Id
		/// </summary>
		/// <param name="workspaceId">Id</param>
		/// <returns>Workspace</returns>
		public FullWorkspace GetFullWorkspaceById(int workspaceId)
		{
			return new FullWorkspace(this.workspaceLoader.GetById(workspaceId));
		}

		/// <summary>
		/// Get Clin By Id
		/// </summary>
		/// <param name="clinId">Id</param>
		/// <returns>Clin</returns>
		public ClinDTO GetClinById(int clinId)
		{
			return this.clinLoader.GetById(clinId);
		}

		/// <summary>
		/// Gets workspace export format by workspace Id.
		/// </summary>
		/// <param name="wsId">Workspace Id</param>
		/// <returns>Workspace Export Format DTOs</returns>
		public ICollection<WorkspaceExportFormatDTO> GetWorkspaceExportFormatsByWorkspaceId(int wsId)
		{
			return this.workspaceExportFormatLoader.GetWorkspaceExportFormatsForWorkspace(wsId);
		}

		/// <summary>
		/// Get Workspace Export Format DTO by the template ID
		/// </summary>
		/// <param name="id">Template ID</param>
		/// <returns>Workspace Export Format DTO for the given template ID</returns>
		public WorkspaceExportFormatDTO GetWorkspaceExportFormatByTemplateId(int id)
		{
			return this.workspaceExportFormatLoader.GetById(id);
		}

		/// <summary>
		/// Get the WS Export Format's Name 
		/// </summary>
		/// <param name="templateID">The template ID of the workspace export format.</param>
		/// <returns>The name of the WS Export Format</returns>
		public string GetWorkspaceExportFormatNameByTemplateId(int templateID)
		{
			return this.workspaceExportFormatLoader.GetNameById(templateID);
		}

		public ICollection<WorkspaceVersionMetaDataDTO> GetWorkspaceVersionMetaDataByWorkspaceId(int wsId)
		{
			return this.workspaceVersionMetaDataLoader.GetByWorkspaceID(wsId);
		}

		/// <summary>
		/// Get Clins By Ids
		/// </summary>
		/// <param name="clinIds">Ids</param>
		/// <returns>Clin</returns>
		public ICollection<ClinDTO> GetClinsByIds(ICollection<int> clinIds)
		{
			return this.clinLoader.GetByIds(clinIds);
		}

		/// <summary>
		/// Get Wbs By Id
		/// </summary>
		/// <param name="wbsId">Id</param>
		/// <returns>Wbs</returns>
		public WbsDTO GetWbsById(int wbsId)
		{
			return this.wbsLoader.GetById(wbsId);
		}

		#endregion

		/// <summary>
		/// Gets Workspace Variables Ids by Clin Id
		/// </summary>
		/// <param name="clinId">Clin Id</param>
		/// <returns>Workspace Variable Ids</returns>
		public ICollection<int> GetWorkspaceVariablesByClinId(int clinId)
		{
			return this.clinLoader.GetWorkspaceVariableIDsByClinID(clinId);
		}

		/// <summary>
		/// Gets Task Variable Ids by Clin Id
		/// </summary>
		/// <param name="clinId">Clin Id</param>
		/// <returns>Boe Task Variable Ids</returns>
		public ICollection<int> GetTaskVariableIdsByClinId(int clinId)
		{
			return this.clinLoader.GetTaskVariableIDsByClinID(clinId);
		}

		/// <summary>
		/// Gets the number of materials for the specified Boe
		/// </summary>
		/// <param name="boeId">BOE Id</param>
		/// <returns>Number of materials</returns>
		public int GetNumberOfMaterialsForBoeId(int boeId)
		{
			return this.materialLoader.GetNumberOfMaterialsForBoeId(boeId);
		}

		/// <summary>
		/// Returns resources based on Resource List Id
		/// </summary>
		/// <param name="resourceListId">Resource List Id</param>
		/// <returns>Resources</returns>
		public ICollection<ResourceDTO> GetResourcesByResourceListId(int resourceListId)
		{
			return this.ResourceLoader.GetByListId(resourceListId);
		}

		/// <summary>
		/// Returns resources based on Resource Ids
		/// </summary>
		/// <param name="resourceIds">Resource Ids</param>
		/// <returns>Resources</returns>
		public ICollection<ResourceDTO> GetResourcesByIds(ICollection<int> resourceIds)
		{
			return this.ResourceLoader.GetByIds(resourceIds);
		}

		/// <summary>
		/// Returns all system level LM Labor resources.
		/// </summary>
		/// <returns>System Level LM Labor Resources.</returns>
		public ICollection<ResourceDTO> GetSystemLmLaborResources()
		{
			return this.ResourceLoader.GetByListIdAndElementOfCost(this.ResourceLoader.GlobalListID, ElementOfCostType.LMLabor);
		}

		/// <summary>
		/// Gets all T&amp;M resource rates for the workspace.
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>T&amp;M resource rates for a given workspace.</returns>
		public ICollection<TMResourceRateDTO> GetTMResourceRates(int workspaceId)
		{
			return this.tmResourceRateLoader.GetByWorkspaceId(workspaceId);
		}

		/// <summary>
		/// Get Performing Orgs based on the list Id
		/// </summary>
		/// <param name="performingOrgListId">List Id</param>
		/// <returns>Performing List Id</returns>
		public ICollection<PerformingOrgDTO> GetPerformingOrgsByListId(int performingOrgListId)
		{
			return this.perfOrgLoader.GetByListId(performingOrgListId);
		}

		/// <summary>
		/// Get Performing Orgs based on the Ids
		/// </summary>
		/// <param name="ids">Ids</param>
		/// <returns>Performing Orgs/returns>
		public ICollection<PerformingOrgDTO> GetPerformingOrgsByIds(ICollection<int> ids)
		{
			return this.perfOrgLoader.GetByIds(ids);
		}

		/// <summary>
		/// Get location data by ids
		/// </summary>
		/// <param name="ids">Ids of locations</param>
		/// <returns>Corresponding locations</returns>
		public ICollection<LocationDTO> GetLocationDataByIds(ICollection<int> ids)
		{
			return this.locationLoader.GetByIds(ids);
		}

		/// <summary>
		/// Gets the global performing organization list.
		/// </summary>
		/// <returns>Global performing organization list name.</returns>
		public PerformingOrgListDTO GetPerfOrgList()
		{
			return this.performingOrgListLoader.GetPerfOrgList(CommonConstants.GLOBAL_PERFORMING_ORG_LIST_ID);
		}

		/// <summary>
		/// Get Materials for Boe Ids
		/// </summary>
		/// <param name="boeIds">Boe Ids</param>
		/// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
		/// <returns>Material Dtos</returns>
		public ICollection<MaterialDTO> GetMaterialsByBoeIds(Collection<int> boeIds, bool includeRTEFields)
		{
			return this.materialLoader.GetByBoeIds(boeIds, includeRTEFields).ToList();
		}

		/// <summary>
		/// Get Materials for the Workspace with the given ID
		/// </summary>
		/// <param name="workspaceId">Workspace ID</param>
		/// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
		/// <returns>Material Dtos</returns>
		public ICollection<MaterialDTO> GetMaterialsByWorkspaceId(int workspaceId, bool includeRTEFields)
		{
			return this.materialLoader.GetByWorkspaceId(workspaceId, includeRTEFields).ToList();
		}

		/// <summary>
		/// Used during exports, this methods gets (for every BoeId in the WS) the last user to submit the Boe for approval
		/// </summary>
		/// <param name="wsId">Workspace Id</param>
		/// <returns>Mapping of BoeIds and UserIds</returns>
		public Dictionary<int, UserDTO> GetBoeIdsAndAuthorsThatLastSubmittedItForApprovalForWs(int wsId)
		{
			return this.boeHistoryLoader.GetBoeIdsAndAuthorsThatLastSubmittedItForApprovalForWs(wsId);
		}

		/// <summary>
		/// Gets User Data by UserIds
		/// </summary>
		/// <param name="ids">User Ids</param>
		/// <returns>User Data</returns>
		public ICollection<UserDTO> GetUsersByIds(ICollection<int> ids)
		{
			return this.userDataLoader.GetByIds(ids);
		}

		#region Boe Stuff

		/// <summary>
		/// GetLaborTypesTypesForBoeId
		/// </summary>
		/// <param name="boeId">Boe Id</param>
		/// <returns>Corresponding Labor Types</returns>
		public ICollection<ResourceTypeDto> GetLaborTypesTypesForBoeId(int boeId)
		{
			return this.resourceTypeLoader.GetByBoeId(boeId);
		}

		/// <summary>
		/// Check if a BOE exists given a custom field ID
		/// </summary>
		/// <param name="inBOEID">BOE</param>
		/// <param name="customFieldId">Custom Field</param>
		/// <returns>true if it exists, false if not</returns>
		public bool CheckIfBoeExistsGivenCustomFieldID(int boeId, int customFieldId)
		{
			return this.boeLoader.CheckIfBoeExistsByCustomFieldId(boeId, customFieldId);
		}

		/// <summary>
		/// check if the BOE contains Labor/Cost Elements(Labor, Mission, ODC, etc)
		/// </summary>
		/// <param name="boeId"></param>
		/// <returns></returns>
		public bool CheckIfBoeContainsLaborCostElements(int boeId)
		{
			return this.boeLoader.BoeContainsLaborCostElement(boeId);
		}

		/// <summary>
		/// check if the BOE contains Material Elements
		/// </summary>
		/// <param name="boeId"></param>
		/// <returns></returns>
		public bool CheckIfBoeContainsMaterialElements(int boeId)
		{
			return this.boeLoader.BoeContainsMaterialElement(boeId);
		}

		/// <summary>
		/// Gets Task Variable Ids based on Boe
		/// </summary>
		/// <param name="boeId">Boe Id</param>
		/// <returns>Task Variable Ids</returns>
		public ICollection<int> GetTaskVariableIdsByBoeId(int boeId)
		{
			return this.boeLoader.GetTaskVariableIdsByBoeId(boeId);
		}

		/// <summary>
		/// Gets a list of task elements associated with a Boe.
		/// </summary>
		/// <param name="boeId">Boe Id.</param>
		/// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
		/// <param name="costPrecision">The Cost precision for the workspace.</param>
		/// <param name="hoursPrecision">The Hours precision for the workspace.</param>
		/// <returns>Task elements associated to a Boe.</returns>
		public ICollection<BoeTaskElementDTO> GetBoeTaskElementCollectionByBoeId(int boeId, bool includeRTEFields, int hoursPrecision, int costPrecision)
		{
			return this.taskElementLoader.GetByBoeId(boeId, hoursPrecision, costPrecision, includeRTEFields);
		}

		/// <summary>
		/// Gets a list of task elements associated with a Workspace.
		/// </summary>
		/// <param name="workspaceId">Workspace Id.</param>
		/// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
		/// <param name="costPrecision">The Cost precision for the workspace.</param>
		/// <param name="hoursPrecision">The Hours precision for the workspace.</param>
		/// <returns>Task elements associated to a Workspace.</returns>
		public ICollection<BoeTaskElementDTO> GetBoeTaskElementCollectionByWorkspaceId(int workspaceId, bool includeRTEFields, int hoursPrecision, int costPrecision)
		{
			return this.taskElementLoader.GetByWorkspaceId(workspaceId, includeRTEFields, hoursPrecision, costPrecision);
		}

		/// <summary>
		/// Gets a list of approver responses associated with a Boe.
		/// </summary>
		/// <param name="boeId">Boe Id.</param>
		/// <returns>Approver responses associated to a Boe.</returns>
		public ICollection<BoeApproverResponseDTO> GetApproverResponseCollectionByBoeId(int boeId)
		{
			return this.approverResponseLoader.GetByBoeId(boeId);
		}

		/// <summary>
		/// Gets a list of approver responses associated with a Workspace.
		/// </summary>
		/// <param name="workspaceId">The Workspace.</param>
		/// <returns>Approver responses associated to a Workspace.</returns>
		public IDictionary<int, ICollection<BoeApproverResponseDTO>> GetApproverResponseCollectionByWorkspaceId(int workspaceId)
		{
			return this.approverResponseLoader.GetByWorkspaceId(workspaceId);
		}

		/// <summary>
		/// Gets a list of approver user ids associated with Boes inside a workspace.
		/// </summary>
		/// <param name="workspaceId">Workspace to get approval user Ids for.</param>
		/// <returns>List of approver user ids associated with Boes inside a workspace.</returns>
		public ICollection<int> GetApprovalUserIdsByWorkspaceId(int workspaceId)
		{
			return this.approverResponseLoader.GetApprovalUserIdsByWorkspaceId(workspaceId);
		}

		/// <summary>
		/// Get all custom field values assigned to the given BOE.
		/// </summary>
		/// <returns>Dictionary of custom field values to custom field dto for the BOE</returns>
		public IDictionary<CustomFieldValueDTO, CustomFieldDTO> GetAllAssignedBOECustomFieldValuesForBOE(int boeId)
		{
			return this.customFieldLoader.GetAllAssignedBOECustomFieldValuesForBoeIds(new List<int>() { boeId })[boeId];
		}

		/// <summary>
		/// Get all custom field values assigned to the given BOE.
		/// </summary>
		/// <returns>Dictionary of custom field values to custom field dto for the BOE</returns>
		public IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> GetAllAssignedBOECustomFieldValuesForBoeIds(ICollection<int> boeIds)
		{
			return this.customFieldLoader.GetAllAssignedBOECustomFieldValuesForBoeIds(boeIds);
		}

		/// <summary>
		/// Gets a list of other direct Costs associated with Boes.
		/// </summary>
		/// <param name="boeIds">Boe Ids</param>
		/// <param name="loadRteData">Indicate whether RTE data should be loaded automatically</param>
		/// <returns>Other direct Costs associated with Boes.</returns>
		public ICollection<OtherDirectCostDTO> GetOdcCollectionByBoeIds(ICollection<int> boeIds, bool loadRteData)
		{
			return this.otherDirectCostLoader.GetByBoeIds(boeIds, loadRteData);
		}

		/// <summary>
		/// Gets a list of other direct Costs associated with Workspace
		/// </summary>
		/// <param name="workspaceId">Workspace ID</param>
		/// <param name="loadRteData">Indicate whether RTE data should be loaded automatically</param>
		/// <returns>Other direct Costs associated with the Workspace.</returns>
		public ICollection<OtherDirectCostDTO> GetOdcCollectionByWorkspaceId(int workspaceId, bool loadRteData)
		{
			return this.otherDirectCostLoader.GetByWorkspaceId(workspaceId, loadRteData);
		}

		/// <summary>
		/// Gets all FullBoe objects that belong to the workspace
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <param name="loadRteData">Indicate whether RTE data should be loaded automatically</param>
		/// <param name="taskElements">Task Elements to populate into BOEs, assuming they are populated with RTE data if loadRteData is true</param>
		/// <returns>FullBoe objects</returns>
		public ICollection<FullBoe> GetFullBoesByWorkspaceId(int workspaceId, bool loadRteData, IEnumerable<BoeTaskElementDTO> taskElements)
		{
			ICollection<BoeDTO> boes = this.boeLoader.GetByWorkspaceId(workspaceId, loadRteData);

			List<FullBoe> result = new List<FullBoe>();

			foreach (BoeDTO boe in boes)
			{
				FullBoe fullBoe = new FullBoe(boe);

				// Load the boe's tasks more efficiently if possible
				if (taskElements != null)
				{
					fullBoe.SetTaskElements(taskElements);
				}

				result.Add(fullBoe);
			}

			return result;
		}
		
		#endregion

		#region Wbs Stuff

		/// <summary>
		/// Gets Boes that belong to the Wbs
		/// </summary>
		/// <param name="wbsId">Wbs Id</param>
		/// <returns>Corresponding Boes</returns>
		public ICollection<FullBoe> GetBoesByWbs(int wbsId)
		{
			ICollection<FullBoe> toReturn = new List<FullBoe>();
			ICollection<BoeDTO> rawBoes = this.boeLoader.GetByWbsId(wbsId);

			foreach (BoeDTO aBoe in rawBoes)
			{
				toReturn.Add(new FullBoe(aBoe));
			}

			return toReturn;
		}

		/// <summary>
		/// Gets Boes WITH nesting that belong to the Wbs
		/// </summary>
		/// <param name="wbsId">Wbs Id</param>
		/// <returns>Corresponding Boes</returns>
		public ICollection<FullBoe> GetBoesWithNestingByWbs(int wbsId)
		{
			ICollection<FullBoe> toReturn = new List<FullBoe>();

			ICollection<int> boeIds = this.wbsLoader.GetBoeIdsForWbsIdWithNesting(wbsId);

			ICollection<BoeDTO> rawBoes = this.boeLoader.GetByIds(boeIds);

			foreach (BoeDTO aBoe in rawBoes)
			{
				toReturn.Add(new FullBoe(aBoe));
			}

			return toReturn;
		}

		/// <summary>
		/// Gets parent Wbs Elements
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <param name="wbsNumber">Wbs Number</param>
		/// <returns>A collection of Full Wbs Elements</returns>
		public ICollection<FullWbs> GetParentWbs(int workspaceId, string wbsNumber)
		{
			ICollection<FullWbs> toReturn = new List<FullWbs>();

			ICollection<WbsDTO> parentWbsObjects = this.wbsLoader.GetAllParentWbs(workspaceId, wbsNumber);

			foreach (WbsDTO parentWbs in parentWbsObjects)
			{
				toReturn.Add(new FullWbs(parentWbs));
			}

			return toReturn;
		}

		/// <summary>
		/// Gets child Wbs Elements
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <param name="wbsNumber">Wbs Number</param>
		/// <returns>A collection of Full Wbs Elements</returns>
		public ICollection<FullWbs> GetChildWbs(int workspaceId, string wbsNumber)
		{
			ICollection<FullWbs> toReturn = new List<FullWbs>();

			ICollection<WbsDTO> childWbsObjects = this.wbsLoader.GetAllChildWbs(workspaceId, wbsNumber);

			foreach (WbsDTO childWbs in childWbsObjects)
			{
				toReturn.Add(new FullWbs(childWbs));
			}

			return toReturn;
		}

		/// <summary>
		/// Gets Task Variables based on Wbs Id
		/// </summary>
		/// <param name="wbsId">Wbs Id</param>
		/// <returns>Task Variables</returns>
		public ICollection<int> GetTaskVariablesByWbsId(int wbsId)
		{
			return this.wbsLoader.GetTaskVariableIdsById(wbsId);
		}

		/// <summary>
		/// Gets Workspace Variables based on Wbs Id
		/// </summary>
		/// <param name="wbsId">Wbs Id</param>
		/// <returns>Task Variables</returns>
		public ICollection<int> GetWorkspaceVariablesByWbsId(int wbsId)
		{
			return this.wbsLoader.GetWorkspaceVariableIdsById(wbsId);
		}

		#endregion

		#region RTE Handling

		/// <summary>
		/// Populates RTE fields for the supplied objects
		/// </summary>
		/// <param name="itemsToLoad">items whose RTE fields will be loaded</param>
		public void PopulateRTEData(ICollection<FullBoe> itemsToLoad)
		{ 
			this.boeLoader.LoadRTEFields(itemsToLoad as ICollection<BoeDTO>);
		}

		/// <summary>
		/// Populates RTE fields for the supplied objects
		/// </summary>
		/// <param name="itemsToLoad">items whose RTE fields will be loaded</param>
		public void PopulateRTEData(ICollection<TravelDTO> itemsToLoad)
		{
			this.travelLoader.LoadRTEFields(itemsToLoad);
		}

		/// <summary>
		/// Populates RTE fields for the supplied objects
		/// </summary>
		/// <param name="itemsToLoad">items whose RTE fields will be loaded</param>
		public void PopulateRTEData(ICollection<OtherDirectCostDTO> itemsToLoad)
		{
			this.otherDirectCostLoader.LoadRTEFields(itemsToLoad);
		}

		/// <summary>
		/// Populates RTE fields for the supplied objects
		/// </summary>
		/// <param name="itemsToLoad">items whose RTE fields will be loaded</param>
		public void PopulateRTEData(ICollection<MaterialDTO> itemsToLoad)
		{
			this.materialLoader.LoadRTEFields(itemsToLoad);
		}

		/// <summary>
		/// Populates RTE fields for the supplied objects
		/// </summary>
		/// <param name="itemsToLoad">items whose RTE fields will be loaded</param>
		public void PopulateRTEData(ICollection<BoeTaskElementDTO> itemsToLoad)
		{
			this.taskElementLoader.LoadRTEFields(itemsToLoad);
		}

		#endregion

		/// <summary>
		/// Gets the project map data by workspace identifier.
		/// </summary>
		/// <param name="workspaceId">The workspace identifier.</param>
		/// <returns>A collection of project map data.</returns>
		public ICollection<ProjectMapModelView> GetProjectMapDataByWorkspaceId(int workspaceId)
		{
			return this.projectMapDataLoader.GetByWorkspaceId(workspaceId);
		}

		/// <summary>
		/// Gets the project map paged data.
		/// </summary>
		/// <param name="workspaceId">The workspace identifier.</param>
		/// <param name="page">The page number.</param>
		/// <returns>A page of data for the front-end.</returns>
		public ProjectMapPageModelView GetProjectMapPagedData(int workspaceId, int page)
		{
			return this.projectMapDataLoader.GetProjectMapPagedData(workspaceId, page);
		}

		/// <summary>
		/// Gets RTE Template Overrides for the specific workspace
		/// </summary>
		public ICollection<RteTemplateSource> GetWsRteOverrides(int workspaceId)
		{
			return this.rteTemplateDataLoader.GetTemplates(workspaceId).SelectMany(x => x.Assigned).Distinct().Select(x => (RteTemplateSource)x).ToList();
		}

		/// <summary>
		/// Gets the Questions and Answers by Boe Id.
		/// </summary>
		/// <param name="workspaceId">The Id of a workspace.</param>
		/// <param name="boeId">The Boe Id.</param>
		/// <returns>Combo of questions and answers for the BOE.</returns>
		public ICollection<RTECustomTemplateQuestionAnswerModelView> GetRTETemplatesAndAnswersByBoeId(int workspaceId, int boeId)
		{
			return this.rteTemplateDataLoader.GetByBoeId(workspaceId, boeId);
		}

		/// <summary>
		/// Gets Questions and Answers By Workspace Id
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>Questions and Answers</returns>
		public ICollection<RTECustomTemplateQuestionAnswerModelView> GetQuestionsAndAnswersByWorkspaceId(int workspaceId)
		{
			return this.rteTemplateDataLoader.GetQuestionsAndAnswersByWorkspaceId(workspaceId);
		}

		/// <summary>
		/// Gets MOQ Type selections for the workspace
		/// </summary>
		/// <param name="wsId">WS Id</param>
		/// <returns>Selected MOQ Types with data</returns>
		public ICollection<MoqTypeSelection> GetMoqTypeSelectionsByWorkspaceId(int wsId)
		{
			return this.moqTypeDataLoader.GetByWorkspaceId(wsId);
		}

		/// <summary>
		/// Gets MOQ Type selections for the BOE
		/// </summary>
		/// <param name="boeId">Boe Id</param>
		/// <returns>Selected MOQ Types with data</returns>
		public ICollection<MoqTypeSelection> GetMoqTypeSelectionsByBoeId(int boeId)
		{
			return this.moqTypeDataLoader.GetByBoeId(boeId);
		}
	}
}