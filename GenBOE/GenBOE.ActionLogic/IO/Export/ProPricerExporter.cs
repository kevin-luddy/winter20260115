// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using System.Text;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ZoneTravel;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.Compression;
	using IES.Common.Exceptions;

	/// <summary>
	/// Exports all EOC elements to ProPricer
	/// </summary>
	public class ProPricerExporter
	{
		#region Constants

		// constant used in Field 1 - Task ID for the ProPricer Task Data and Resource Cost/Hours report if an IS&GS Labor 
		private const string ISGS_LABOR_LIDN = "LIDN";

		// constant used in Field 1 - Task ID for the ProPricer Task Data and Resource Cost/Hours report if a subcontractor
		private const string SUBCONTRACTOR_SIDN = "SIDN";

		// constant used in Field 1 - Task ID for the ProPricer Task Data and Resource Cost/Hours report if an IWTA
		private const string IWTA_IIDN = "IIDN";

		// constant used in Field 1 - Task ID for the ProPricer Task Data and Resource Cost/Hours report if an ODC
		private const string ODC_OIDN = "OIDN";

		// constant used in Field 1 - Task ID for the ProPricer Task Data and Resource Cost/Hours report if Travel
		private const string TRAVEL_TIDN = "TIDN";

		// constant used in Field 1 - Task ID for the ProPricer Task Data and Resource Cost/Hours report if Materials
		private const string MATERIAL_MIDN = "MIDN";

		// constant used for the value of Field 12-Summary in Pro Pricer Task Data report
		private const string TOTAL = "TOTAL";

		// double quotes are needed around certain fields for the export like BOE title and WBS description
		private const string DOUBLE_QUOTE = "\u0022";

		// a comma has to follow each field in the file row
		private const string END_FIELD = ",";

		private const string FORMAT = "000000";

		/// <summary>
		/// 1LMX custom field name
		/// </summary>
		private const string ONE_LMX_CUSTOM_FIELD = "BRC";

		#endregion

		#region Properties & Constructor

		private TravelTripCostCalculation travelTripCostCalculation;
		private RMSZoneTravelRatesFeesDataLoader rmsZoneTravelRatesFeesDataLoader;
		private IRetriever retriever;
		private ICommonDataMapper commonDataMapper;
		private IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources;

		/// <summary>
		/// Constructor with the BOE DTO Loader, CLIN DTO Loader, and WBS DTO Loader
		/// </summary>
		/// <param name="travelTripCostCalculation">The travel trip cost calculation.</param>
		/// <param name="rmsZoneTravelRatesFeesDataLoader">The RMS zone travel rates fees data loader.</param>
		/// <param name="retriever">The retriever.</param>
		/// <param name="commonDataMapper">The common data mapper.</param>
		public ProPricerExporter(TravelTripCostCalculation travelTripCostCalculation, RMSZoneTravelRatesFeesDataLoader rmsZoneTravelRatesFeesDataLoader, IRetriever retriever, ICommonDataMapper commonDataMapper)
		{
			this.travelTripCostCalculation = travelTripCostCalculation;
			this.rmsZoneTravelRatesFeesDataLoader = rmsZoneTravelRatesFeesDataLoader;
			this.retriever = retriever;
			this.commonDataMapper = commonDataMapper;
		}
		#endregion

		/// <summary>
		/// This function handles exporting to ProPricer
		/// </summary>
		/// <param name="inProPricer"> Pro Pricer DTO</param>
		/// <param name="inZipPathFile">Zip Path File</param>
		/// <param name="inWorkspace">The full workspace.</param>
		/// <returns>Location of the zipped files.</returns>
		public string ExportReport(ProPricerDTO inProPricer, string inZipPathFile, FullWorkspace inWorkspace)
		{
			PpDataReadyForExport ppDataToExport = this.ExportProPricer(inProPricer, inWorkspace);


			// create the task data export stream
			string taskData = this.CreateDataOutputString(ppDataToExport.TaskData);

			// create the resource cost/hours stream
			string resourceData = this.CreateDataOutputString(ppDataToExport.ResourceData);

			// offload streams
			string offloadTaskData = null;
			string offloadResourceData = null;

			bool offloading = inWorkspace.ProjectMapType != ProjectMapType.StandardWithoutOffload;

			if (offloading)
			{
				offloadTaskData = this.CreateDataOutputString(ppDataToExport.OffloadTaskData);
				offloadResourceData = this.CreateDataOutputString(ppDataToExport.OffloadResourceData);
				this.allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(inWorkspace.IsProjectMapWorkspace);
			}

			string zippedFileName;
			byte[] taskDataByteArray = Encoding.ASCII.GetBytes(taskData);
			using (MemoryStream taskDataMemoryStream = new MemoryStream(taskDataByteArray))
			{
				// Turn resource data into a stream
				byte[] resourceCostByteArray = Encoding.ASCII.GetBytes(resourceData);
				using (MemoryStream resourceCostMemoryStream = new MemoryStream(resourceCostByteArray))
				{
					// Initialize collection of streams to zip
					Dictionary<string, Stream> zipContents = new Dictionary<string, Stream>();

					// Add both ProPricer streams to dictionary for zipping
					zipContents.Add("pp_data.tsk", taskDataMemoryStream);
					zipContents.Add("pp_data.res", resourceCostMemoryStream);

					if (offloading)
					{
						byte[] offloadTaskDataByteArray = Encoding.ASCII.GetBytes(offloadTaskData);
						using (MemoryStream offloadTaskDataMemoryStream = new MemoryStream(offloadTaskDataByteArray))
						{
							// Turn resource data into a stream
							byte[] offloadResourceCostByteArray = Encoding.ASCII.GetBytes(offloadResourceData);
							using (MemoryStream offloadResourceCostMemoryStream = new MemoryStream(offloadResourceCostByteArray))
							{
								// Add both ProPricer streams to dictionary for zipping
								zipContents.Add("pp_offloaddata.tsk", offloadTaskDataMemoryStream);
								zipContents.Add("pp_offloaddata.res", offloadResourceCostMemoryStream);

								// Have to zip inside here because of using statements
								// Zip files and return zip file to user as a Download
								zippedFileName = Zip.ZipFiles(zipContents, inZipPathFile);
							}
						}
					}
					else
					{
						// Zip files and return zip file to user as a Download
						zippedFileName = Zip.ZipFiles(zipContents, inZipPathFile);
					}
				}
			}

			return zippedFileName;
		}

		/// <summary>
		/// Creates the data output string.
		/// </summary>
		/// <param name="data">The data used to create the string.</param>
		/// <returns>All of the data inside one string.</returns>
		public string CreateDataOutputString(List<string> data)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			StringBuilder sb = new StringBuilder();
			foreach (string line in data)
			{
				// remove the extra , 
				if (line.Length > 0)
				{
					string updatedRowString = line.Remove(line.Length - 1, 1);
					// Do replacements: the first special char (even though it may not look like it!); replacing with std char
					sb.AppendLine(updatedRowString.Replace('–', '-').Replace(' ', ' ').Replace("’", "'").Replace("  ", " "));
				}
			}

			// the output string
			return sb.ToString();
		}

		/// <summary>
		/// This function actually does all the calculations to retrieve data for the Pro Pricer export report
		/// </summary>
		/// <param name="proPricerExport">The pro pricer export.</param>
		/// <param name="workspace">The workspace.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// inProPricerExport
		/// or
		/// workspace
		/// </exception>
		/// <exception cref="GenValidationException">
		/// Not all Task Element Labor Types have a Resource. Please make sure that your data is correct. Task Element data export failed.
		/// or
		/// Not all Task Element Labor Types have a Performing Org. Please make sure that your data is correct. Task Element data export failed.
		/// </exception>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public PpDataReadyForExport ExportProPricer(ProPricerDTO proPricerExport, FullWorkspace workspace)
		{
			if (proPricerExport == null)
			{
				throw new ArgumentNullException(nameof(proPricerExport));
			}

			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			#region BOE-J 66 -> need to abort the export if a task resource does not have a performing org or a resource selected.

			List<ResourceTypeDto> taskElementResourceTypes = workspace.TaskElements.SelectMany(x => x.taskElementLabors).ToList();
			List<int> perfOrgIdsFromDb = workspace.PerformingOrgsUsedInBoes.Select(x => x.Id).ToList();

			if (taskElementResourceTypes.Any(r => !r.ResourceID.HasValue && !r.BusinessResourceCodeID.HasValue))
			{ throw new GenValidationException("Not all Task Element Labor Types have a Resource and/or a Business Resource. Please make sure that your data is correct. Task Element data export failed."); }

			if (taskElementResourceTypes.Any(r => !r.PerformingOrgID.HasValue) || taskElementResourceTypes.Any(r => !perfOrgIdsFromDb.Contains(r.PerformingOrgID.Value)))
			{ throw new GenValidationException("Not all Task Element Labor Types have a Performing Org. Please make sure that your data is correct. Task Element data export failed."); }

			#endregion

			// get all BOE data in the workspace
			Collection<FullBoe> boes = workspace.Boes.ToCollection();

			// Resources
			// lists for labor, sub, material, iwta should be populated with both resources and BRCs
			Collection<int> laborResourceIDs = workspace.ResourcesUsedInWsBoes.Where(r => r.ElementOfCost == ElementOfCostType.LMLabor).Select(r => r.Id).ToCollection();
			Collection<int> iwtaResourceIDs = workspace.ResourcesUsedInWsBoes.Where(r => r.ElementOfCost == ElementOfCostType.IWTA).Select(r => r.Id).ToCollection();
			Collection<int> subContractorResourceIDs = workspace.ResourcesUsedInWsBoes.Where(r => r.ElementOfCost == ElementOfCostType.Sub).Select(r => r.Id).ToCollection();
			Collection<int> materialResourceIDs = workspace.ResourcesUsedInWsBoes.Where(r => r.ElementOfCost == ElementOfCostType.Materials).Select(r => r.Id).ToCollection();
			Collection<int> odcResourceIDs = workspace.ResourcesUsedInWsBoes.Where(r => r.ElementOfCost == ElementOfCostType.ODC).Select(r => r.Id).ToCollection();
			ICollection<ResourceDTO> travelResources = workspace.ResourcesUsedInWsBoes.Where(r => r.ElementOfCost == ElementOfCostType.Travel).ToList();
			Collection<int> travelResourceIDs = travelResources.Select(r => r.Id).ToCollection();
			bool isUsingEP = FullObjectHelper.ShowEquivalentPersonsOption && workspace.IsUsingEquivalentPerson;
			bool offloading = workspace.ProjectMapType != ProjectMapType.StandardWithoutOffload;
			CustomFieldDTO oneLmxCF = workspace.CustomFields.FirstOrDefault(c => c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay && c.CustomFieldName == ONE_LMX_CUSTOM_FIELD);
			Collection<int> oneLMXResourceIDs = GetOneLMXResourceIds(workspace, oneLmxCF);

			WsLevelInputsForExport wsDataForExport = new WsLevelInputsForExport()
			{
				ResourceDecimalPrecision = workspace.DecimalPrecision,
				CostDecimalPrecision = workspace.CostDecimalPrecision,
				PerfOrgs = workspace.PerformingOrgsUsedInBoes,
				Resources = workspace.ResourcesUsedInWsBoes,
				ResourcesInWs = workspace.ResourcesForWsResourceListId?.ToList(),
				WsCustomFields = workspace.CustomFields.ToCollection(),
				WsCustomFieldValues = workspace.CustomFieldValues.ToCollection(),
				Clins = workspace.Clins.ToCollection(),
				Wbses = workspace.WbsElements.ToCollection(),
				PPInputsToExport = proPricerExport,
				TaskElements = workspace.TaskElements.ToCollection(),
				IsProjectMapWorkspace = workspace.IsProjectMapWorkspace,
				IsUsingTemplateBOE = workspace.UsingTemplateBOE,
				MoqTypes = workspace.MoqTypeSelections.ToCollection(),
				OneLmxCustomField = oneLmxCF
			};

			if (offloading)
			{
				// offload the data
				OffloadLaborRates offloader = new OffloadLaborRates();
				OffloadLaborRatesResults results = offloader.OffloadWorkspace(boes, workspace);

				boes = results.Boes.ToCollection();
				wsDataForExport.TaskElements = boes.SelectMany(b => b.TaskElements).ToCollection();

				// need to update the resources since the new offloaded labor resources might use subcontractor resources not added
				ICollection<int> resourceIds = wsDataForExport.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value)
						.Union(workspace.Odcs.SelectMany(x => x.ODCTypes).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value))
						.Union(wsDataForExport.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.BusinessResourceCodeID.HasValue).Select(x => x.BusinessResourceCodeID.Value))
						.Distinct().ToList();

				wsDataForExport.Resources = this.retriever.GetResourcesByIds(resourceIds).ToList().AsReadOnly();
			}else if (Utilities.IsBRCEnabledForWorkspace(workspace.Shortname))
			{
				ICollection<int> resourceIds = wsDataForExport.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value)
						.Union(workspace.Odcs.SelectMany(x => x.ODCTypes).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value))
						.Union(wsDataForExport.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.BusinessResourceCodeID.HasValue).Select(x => x.BusinessResourceCodeID.Value))
						.Distinct().ToList();
				wsDataForExport.Resources = this.retriever.GetResourcesByIds(resourceIds).ToList().AsReadOnly();
			}
			//to remove
			bool has1LMXResources = oneLMXResourceIDs.Any();

			if (!Utilities.IsBRCEnabledForWorkspace(workspace.Shortname) && has1LMXResources)
			{
				ICollection<ResourceDTO> resourceDTOs = this.retriever.GetResourcesByIds(oneLMXResourceIDs);
				wsDataForExport.Resources = resourceDTOs.Union(wsDataForExport.Resources).ToList().AsReadOnly();
			}

			foreach (BoeDTO boe in boes)
			{
				this.ProcessBoe(workspace, boe, wsDataForExport, laborResourceIDs, iwtaResourceIDs, subContractorResourceIDs, materialResourceIDs,
					odcResourceIDs, travelResourceIDs, isUsingEP, travelResources, offloading, has1LMXResources);
			}

			return wsDataForExport.PpDataToBeExported;
		}

		/// <summary>
		/// Retrieve a list of 1LMX Resource Ids used in the 1LMX Custom Field on a Labor Type.
		/// </summary>
		/// <param name="workspace">The workspace to pull data from.</param>
		/// <param name="oneLmxCF">1LMX Custom Field</param>
		/// <returns>List of 1LMX Resource Ids.</returns>
		private Collection<int> GetOneLMXResourceIds(FullWorkspace workspace, CustomFieldDTO oneLmxCF)
		{
			Collection<int> oneLmxResourceIds = new Collection<int>();

			if (!Utilities.IsBRCEnabledForWorkspace(workspace.Shortname) && oneLmxCF != null)
			{
				// need to get IDs from this custom field into the list of Ids used
				List<string> resourceNames = workspace.CustomFieldValues.Where(c => c.CustomFieldID == oneLmxCF.Id).Select(f => f.CustomFieldValueName).ToList();
				IReadOnlyCollection<ResourceDTO> resources = workspace.ResourcesForWsResourceListId;

				oneLmxResourceIds = resources.Where(r => resourceNames.Contains(r.ResourceName)).Select(e => e.Id).ToCollection();
			}

			return oneLmxResourceIds;
		}

		#region Data Processing Helpers

		/// <summary>
		/// Processes the boe.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="boe">The boe.</param>
		/// <param name="wsDataForExport">The ws data for export.</param>
		/// <param name="laborResourceIDs">The labor resource ids.</param>
		/// <param name="iwtaResourceIDs">The iwta resource ids.</param>
		/// <param name="subContractorResourceIDs">The sub contractor resource ids.</param>
		/// <param name="materialResourceIDs">The material resource ids.</param>
		/// <param name="odcResourceIDs">The odc resource ids.</param>
		/// <param name="travelResourceIDs">The travel resource ids.</param>
		/// <param name="isUsingEP">if set to <c>true</c> [is using ep].</param>
		/// <param name="travelResources">The travel resources.</param>
		/// <param name="offloading">True if this export is offloading.</param>
		/// <param name="has1LMXResources">True if this export has 1LMX Resources that need split</param>
		private void ProcessBoe(FullWorkspace workspace, BoeDTO boe, WsLevelInputsForExport wsDataForExport,
			Collection<int> laborResourceIDs, Collection<int> iwtaResourceIDs, Collection<int> subContractorResourceIDs,
			Collection<int> materialResourceIDs, Collection<int> odcResourceIDs, Collection<int> travelResourceIDs, bool isUsingEP,
			ICollection<ResourceDTO> travelResources, bool offloading, bool has1LMXResources)
		{
			BoeLevelExportData boeLevelExportData = new BoeLevelExportData()
			{
				CustomFieldValueContainers = boe.CustomFieldValueContainers.ToCollection(),
				Clin = wsDataForExport.Clins.FirstOrDefault(x => x.Id == boe.CLINID),
				Wbs = wsDataForExport.Wbses.FirstOrDefault(x => x.Id == boe.WBSID),
				MultiBoe = boe.IsMultiClinWbs,

				// clean up carriage returns in BOE title - regex to handle when there is more than one CR in a row
				CleanBoeTitle = boe.Title.RemoveCarriageReturns(),
				CleanBoeDescription = boe.Description.RemoveCarriageReturns(),
				CamName = boe.CamName,
				Sow = boe.SOW,
				ProjectMapType = workspace.ProjectMapType,
				Category = boe.Category,
				ClassOfCost = boe.ClassOfCost
			};

			// Get all BOE elements
			ICollection<BoeTaskElementDTO> taskElements = wsDataForExport.TaskElements.Where(x => x.BoeID == boe.Id).ToList();
			CategorizeAllTaskElements(boeLevelExportData, laborResourceIDs, iwtaResourceIDs, subContractorResourceIDs,
				materialResourceIDs, odcResourceIDs, travelResourceIDs, taskElements);

			ICollection<MaterialDTO> materialElements = workspace.Materials.Where(x => x.BoeID == boe.Id).ToList();
			ICollection<TravelDTO> travelElements = workspace.Travels.Where(x => x.BoeID == boe.Id).ToList();
			ICollection<OtherDirectCostDTO> odcElements = workspace.Odcs.Where(x => x.BoeID == boe.Id).ToList();
			DetermineStartAndEndDates(boeLevelExportData, taskElements, materialElements, travelElements, odcElements);

			this.ProcessTaskElements(wsDataForExport, boeLevelExportData, boeLevelExportData.LaborElements,
				ElementOfCostType.LMLabor, laborResourceIDs, isUsingEP, offloading, has1LMXResources, workspace.Shortname);
			this.ProcessTaskElements(wsDataForExport, boeLevelExportData, boeLevelExportData.IWTAElements,
				ElementOfCostType.IWTA, iwtaResourceIDs, isUsingEP, offloading, has1LMXResources, workspace.Shortname);
			this.ProcessTaskElements(wsDataForExport, boeLevelExportData, boeLevelExportData.SubcontractorElements,
				ElementOfCostType.Sub, subContractorResourceIDs, isUsingEP, offloading, has1LMXResources, workspace.Shortname);
			this.ProcessTaskElements(wsDataForExport, boeLevelExportData, boeLevelExportData.MaterialLaborElements,
				ElementOfCostType.Materials, materialResourceIDs, isUsingEP, offloading, has1LMXResources, workspace.Shortname);
			this.ProcessTaskElements(wsDataForExport, boeLevelExportData, boeLevelExportData.TravelLaborElements,
				ElementOfCostType.Travel, travelResourceIDs, isUsingEP, offloading, has1LMXResources, workspace.Shortname);
			this.ProcessTaskElements(wsDataForExport, boeLevelExportData, boeLevelExportData.ODCLaborElements,
				ElementOfCostType.ODC, odcResourceIDs, isUsingEP, offloading, has1LMXResources, workspace.Shortname);
			this.ProcessOdcElements(wsDataForExport, boeLevelExportData, odcElements, odcResourceIDs);
			this.ProcessTravelElements(workspace, wsDataForExport, boeLevelExportData, travelElements, travelResources);
			this.ProcessRMSTravelElements(wsDataForExport, boeLevelExportData, travelElements, workspace);
		}

		/// <summary>
		/// Get the ProPricer Export for Labor, IWTA, and Subcontractor
		/// </summary>
		/// <param name="wsLevelData">The ws level data.</param>
		/// <param name="inputsForExport">The inputs for export.</param>
		/// <param name="taskElements">The task elements.</param>
		/// <param name="elementOfCost">The element of cost.</param>
		/// <param name="resourceIDs">The resource ids.</param>
		/// <param name="isUsingEquivalentPerson">if set to <c>true</c> [is using equivalent person].</param>
		/// <param name="offloading">if set to <c>true</c> [offloading].</param>
		/// <param name="has1LMXResources">if set to <c>true</c>, we need to account for splitting labor type for 1LMX</param>
		/// <param name="workspaceShortname">Workspace shortname</param>
		private void ProcessTaskElements(WsLevelInputsForExport wsLevelData, BoeLevelExportData inputsForExport, Collection<BoeTaskElementDTO> taskElements,
			ElementOfCostType elementOfCost, Collection<int> resourceIDs, bool isUsingEquivalentPerson, bool offloading, bool has1LMXResources, string workspaceShortname)
		{
			if (!taskElements.Any()) { return; }

			IDictionary<int, string> resourceIdToSegmentRegion = wsLevelData.Resources.ToDictionary(r => r.Id, d => d.SegRegion);

			if (!inputsForExport.MultiBoe)
			{
				// the data within the BOE Task Element will be part of the Resource Cost/Hours file
				int startingIndex = -1;

				foreach (BoeTaskElementDTO boeTask in taskElements)
				{
					// do not export to ProPricer if task doesn't have any total hours or cost or if there no offsets
					if (boeTask.TotalHours != 0 || boeTask.TotalCost != 0 || offloading || boeTask.taskElementLabors.Any(l => l.ValueSpread != 0))
					{
						List<ResourceTypeDto> taskResourcesEntriesForElementOfCost = boeTask.taskElementLabors.Where(r => (r.ResourceID.HasValue && resourceIDs.Contains(r.ResourceID.Value))
							|| (r.BusinessResourceCodeID.HasValue && resourceIDs.Contains(r.BusinessResourceCodeID.Value))).ToList();

						List<ResourceTypeDto> resourcesSplitforBrc = taskResourcesEntriesForElementOfCost;


						if (Utilities.IsBRCEnabledForWorkspace(workspaceShortname))
						{
							//clone resources with brc
							//maintain original ID for pro pricer ID mapping in generate resource row
							resourcesSplitforBrc = BRCValidationUtility.ProcessLaborTypesForBrc(taskResourcesEntriesForElementOfCost, resourceIdToSegmentRegion, workspaceShortname, startingIndex).ToList();
							if (resourcesSplitforBrc.Any())
							{
								startingIndex = Math.Min(startingIndex, resourcesSplitforBrc.Select(x => x.Id).Min() - 1);
							}
						}
						if (!Utilities.IsBRCEnabledForWorkspace(workspaceShortname) && has1LMXResources)
						{
							resourcesSplitforBrc = SplitTaskResourcesFor1LMX(taskResourcesEntriesForElementOfCost, resourceIDs, wsLevelData);
						}

						IDictionary<int, string> laborTypeIdToProPricerIdMappings = this.GenerateTaskRow(wsLevelData, inputsForExport, inputsForExport.Clin, inputsForExport.Wbs,
							elementOfCost, resourcesSplitforBrc, boeTask, resourcesSplitforBrc.Any(x => x.IsOffloaded), workspaceShortname);

						foreach (ResourceTypeDto labor in resourcesSplitforBrc)
						{
							this.GenerateResourceRow(wsLevelData, inputsForExport, inputsForExport.Clin, inputsForExport.Wbs, resourceIDs, boeTask,
								laborTypeIdToProPricerIdMappings, labor, isUsingEquivalentPerson, labor.IsOffloaded, workspaceShortname);
						}
					}
				}
			}
			else
			{
				// the data within the BOE Task Element will be part of the Resource Cost/Hours file
				int startingIndex = -1;
				foreach (BoeTaskElementDTO boeTask in taskElements)
				{
					List<ResourceTypeDto> taskResourcesEntriesForElementOfCost = boeTask.taskElementLabors.Where(r => (r.ResourceID.HasValue && resourceIDs.Contains(r.ResourceID.Value))
						|| (r.BusinessResourceCodeID.HasValue && resourceIDs.Contains(r.BusinessResourceCodeID.Value))).ToList();
					List<ResourceTypeDto> resourcesSplitforBrc = taskResourcesEntriesForElementOfCost;
					if (!Utilities.IsBRCEnabledForWorkspace(workspaceShortname) && has1LMXResources)
					{
						resourcesSplitforBrc = SplitTaskResourcesFor1LMX(taskResourcesEntriesForElementOfCost, resourceIDs, wsLevelData);
					} 
					else if(Utilities.IsBRCEnabledForWorkspace(workspaceShortname))
					{
						//clone resources with brc
						//maintain original ID for pro pricer ID mapping in generate resource row
						resourcesSplitforBrc = BRCValidationUtility.ProcessLaborTypesForBrc(taskResourcesEntriesForElementOfCost, resourceIdToSegmentRegion, workspaceShortname, startingIndex).ToList();
						if (resourcesSplitforBrc.Any())
						{
							startingIndex = Math.Min(startingIndex, resourcesSplitforBrc.Select(x => x.Id).Min() - 1);
						}
					}
					IDictionary<int, string> laborTypeIdToProPricerIdMappings = new Dictionary<int, string>();
					foreach (ResourceTypeDto labor in resourcesSplitforBrc)
					{
						// do not export to ProPricer if task doesn't have any total hours or cost or if there no offsets
						if (boeTask.TotalHours != 0 || boeTask.TotalCost != 0 || boeTask.taskElementLabors.Any(l => l.ValueSpread != 0))
						{
							ClinDTO resourceClin = wsLevelData.Clins.FirstOrDefault(i => i.Id == labor.CLINID.GetValueOrDefault(-1));
							WbsDTO resourceWbs = wsLevelData.Wbses.FirstOrDefault(i => i.Id == labor.WBSID.GetValueOrDefault(-1));

							IDictionary<int, string> mappings = this.GenerateTaskRow(wsLevelData, inputsForExport, resourceClin,
								resourceWbs, elementOfCost, new List<ResourceTypeDto>() { labor }, boeTask, labor.IsOffloaded, workspaceShortname);
							// merge the mappings
							foreach (KeyValuePair<int, string> kvp in mappings)
							{
								laborTypeIdToProPricerIdMappings[kvp.Key] = kvp.Value;
							}
						}
					}
					foreach (ResourceTypeDto labor in resourcesSplitforBrc)
					{
						// do not export to ProPricer if task doesn't have any total hours or cost or if there no offsets
						if (boeTask.TotalHours != 0 || boeTask.TotalCost != 0 || boeTask.taskElementLabors.Any(l => l.ValueSpread != 0))
						{
							ClinDTO resourceClin = wsLevelData.Clins.FirstOrDefault(i => i.Id == labor.CLINID.GetValueOrDefault(-1));
							WbsDTO resourceWbs = wsLevelData.Wbses.FirstOrDefault(i => i.Id == labor.WBSID.GetValueOrDefault(-1));

							this.GenerateResourceRow(wsLevelData, inputsForExport, resourceClin, resourceWbs, resourceIDs,
								boeTask, laborTypeIdToProPricerIdMappings, labor, isUsingEquivalentPerson, labor.IsOffloaded, workspaceShortname);
						}
					}
				}
			}
		}

		/// <summary>
		/// Splits the task labor resources into multiple if crossing 1LMX boundary
		/// </summary>
		/// <param name="taskResources">Task Resources to split</param>
		/// <param name="resourceIds">Running list of Resource Ids</param>
		/// <returns>Modified list of Task Resources</returns>
		private List<ResourceTypeDto> SplitTaskResourcesFor1LMX(List<ResourceTypeDto> taskResources, Collection<int> resourceIds, WsLevelInputsForExport wsLevelData)
		{
			List<ResourceTypeDto> splitResources = new List<ResourceTypeDto>();
			foreach (ResourceTypeDto taskResource in taskResources)
			{
				
				if (wsLevelData.OneLmxCustomField != null && taskResource.EndDate > Utilities.OneLmxStartDate)
				{
					// Find the 1LMX Custom Field linkage
					CustomFieldValueContainer container = taskResource.CustomFieldValueContainers.FirstOrDefault(cf => cf.CustomFieldID == wsLevelData.OneLmxCustomField.Id);
					if (container != null)
					{
						CustomFieldValueDTO customFieldValue = wsLevelData.WsCustomFieldValues.FirstOrDefault(v => v.CustomFieldValueID == container.CustomFieldValueID);
						if (customFieldValue != null)
						{
							// need to match the field value against the Name of a Resource
							ResourceDTO oneLmxResource = wsLevelData.Resources.FirstOrDefault(r => r.ResourceName == customFieldValue.CustomFieldValueName);
							if (oneLmxResource != null)
							{
								// need to split the task Resource
								ResourceTypeDto split = new ResourceTypeDto(taskResource);
								split.TaskElementId = taskResource.TaskElementId;
								split.LegacyID = taskResource.LegacyID;
								split.ProjectMapId = taskResource.ProjectMapId;
								split.IsOffloaded = taskResource.IsOffloaded;

								// Update Date Ranges
								split.StartDate = Utilities.OneLmxStartDate;
								taskResource.EndDate = Utilities.OneLmxStartDate;

								// fix labor spreads
								split.LaborSpreads = split.LaborSpreads.Where(s => s.LaborSpreadDate >= Utilities.OneLmxStartDate).ToCollection();
								taskResource.LaborSpreads = taskResource.LaborSpreads.Where(s => s.LaborSpreadDate < Utilities.OneLmxStartDate).ToCollection();

								// fix resource id
								split.ResourceID = oneLmxResource.Id;
								if (!resourceIds.Contains(oneLmxResource.Id))
								{
									resourceIds.Add(oneLmxResource.Id);
								}

								splitResources.Add(split);
							}
						}
					}
				}

				if (taskResource.LaborSpreads.Any())
				{
					splitResources.Add(taskResource);
				}
			}

			return splitResources.OrderBy(r => r.TaskElementId).ThenBy(t => t.Id).ThenBy(b => b.StartDate).ToList();
		}

		/// <summary>
		/// ProcessRMSTravelElements -- add rms travel trips to export
		/// </summary>
		/// <param name="wsLevelData"></param>
		/// <param name="inputsForExport"></param>
		/// <param name="travelElements"></param>
		/// <param name="workspace"></param>
		private void ProcessRMSTravelElements(
			 WsLevelInputsForExport wsLevelData,
			 BoeLevelExportData inputsForExport,
			 ICollection<TravelDTO> travelElements,
			 FullWorkspace workspace)
		{
			if (!travelElements.Any()) { return; }

			Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = this.rmsZoneTravelRatesFeesDataLoader.getAllFeesAndCostsByWorkspace(workspace.Id).ToDictionary(f => f.ModeID);

			ICollection<WorkspaceRMSEscalationRatesDTO> rates = this.rmsZoneTravelRatesFeesDataLoader.getAllEscalationRatesByWorkspace(workspace.Id);
			Dictionary<int, decimal> airfareEscalationRates = rates.ToDictionary(x => x.Year, y => y.AirfareRate);
			Dictionary<int, decimal> perDiemEscalationRates = rates.ToDictionary(x => x.Year, y => y.PerDiemRate);
			Dictionary<int, decimal> miscEscalationRates = rates.ToDictionary(x => x.Year, y => y.MiscRate);

			// the data within the BOE Task Element will be part of the Resource Cost/Hours file 
			foreach (TravelDTO travel in travelElements)
			{
				foreach (MSTTravelTripType mstTrip in travel.MSTTravelTrips)
				{
					++wsLevelData.ItemCounters.TravelId;
					// if zone, get to/from strings to use same propricer output field
					if (mstTrip.ModeID == MSTTravelMode.ZoneAirfare || mstTrip.ModeID == MSTTravelMode.ZoneNoAirfare)
					{
						mstTrip.NonZoneTo = mstTrip.ZoneDestCity + ", " + mstTrip.ZoneDestinationName.TrimEnd(); // get city/state for zone destination
						mstTrip.NonZoneFrom = mstTrip.ZoneOriginName;
					}
					else // non-zone, need calculations
					{
						NonZoneTravelCalculation calculationClass =
						new NonZoneTravelCalculation(
							mstTrip.NumOfDays ?? 0,
							mstTrip.NumOfPeople ?? 0,
							mstTrip.NonZoneNumCars ?? 0,
							mstTrip.NonZonePerDiemDaily ?? 0,
							mstTrip.NonZoneCarRentalTrans ?? 0,
							mstTrip.NonZoneAirfareEstimate ?? 0,
							airfareEscalationRates,
							perDiemEscalationRates,
							miscEscalationRates,
							mstTrip.EstimateDate.Year,
							mstTrip.TripDate.Year,
							mstTrip.ModeID == MSTTravelMode.NonZoneDomestic,
							fees[(int)mstTrip.ModeID].TravelAgencyFee,
							fees[(int)mstTrip.ModeID].MiscOther,
							wsLevelData.ResourceDecimalPrecision
						);

						mstTrip.Cost = calculationClass.TotalTripCost;
					}
					// generate task and resource rows for each trip
					this.GenerateMSTTaskTravelRow(wsLevelData, inputsForExport, travel, mstTrip);
					this.GenerateMSTTripTravelRow(wsLevelData, inputsForExport, travel, mstTrip);
				}
			}
		}

		private void ProcessTravelElements(FullWorkspace theWorkspace, WsLevelInputsForExport wsLevelData, BoeLevelExportData inputsForExport, ICollection<TravelDTO> travelElements,
			ICollection<ResourceDTO> inResources)
		{
			if (!travelElements.Any())
			{
				return;
			}

			// the data within the BOE Task Element will be part of the Resource Cost/Hours file
			foreach (TravelDTO travel in travelElements)
			{
				//for each resource row, travel will contain a row for each unique resource ID and perf org id
				// need to go into travel trips to obtain segment and then match it with the resource ids given segment and element of cost of travel 

				List<SegmentType> travelSegmentsInUse = (from t in travel.TravelTrips
														 select t.Segment).ToList();

				List<TravelData> TravelTripWithResource = (from t in travel.TravelTrips
														   from r in inResources
														   where travelSegmentsInUse.Contains(r.Segment) && t.Segment == r.Segment
														   select new TravelData
														   {
															   ResourceID = r.Id,
															   TravelTrip = t
														   }).ToList();

				List<int> PerfOrgsInUse = (from t in travel.TravelTrips
										   select t.PerfOrgID).Distinct().ToList();

				List<int> resources = (from t in TravelTripWithResource
									   select t.ResourceID).Distinct().ToList();



				// Pro Pricer Task ID. This may or may not be selected, but if it is selected, we may need to keep track of it for Resources too
				// start the count again
				++wsLevelData.ItemCounters.TravelId;

				foreach (int p in PerfOrgsInUse)
				{
					foreach (int resource in resources)
					{
						DateTime startTravelDate = new DateTime();
						DateTime endTravelDate = new DateTime();
						StringBuilder NewTaskRow = new StringBuilder();
						StringBuilder newResourceRow = new StringBuilder();
						//get traveltrip that is using this Perf Org and Resource
						List<TravelData> uniqueTripsByPerfOrgAndResource = (from t in TravelTripWithResource
																			where t.ResourceID == resource && t.TravelTrip.PerfOrgID == p
																			select t).ToList();

						List<TravelTripType> customFieldsInTrip = uniqueTripsByPerfOrgAndResource.Select(t => t.TravelTrip).Where(t => t.CustomFieldValueContainers.Any()).ToList();
						// if a unique trip couldn't be found, bypass it
						if (uniqueTripsByPerfOrgAndResource.Any())
						{
							// need to check if there are multiple trips and they have customfields assigned.
							if (uniqueTripsByPerfOrgAndResource.Count > 1 && customFieldsInTrip.Any())
							{
								while (uniqueTripsByPerfOrgAndResource.Any())
								{
									//get the trips that have the same custom field values
									List<TravelData> uniqueTripsByPerfOrgAndCustomFields = this.GetUniqueTripsByCustomFields(uniqueTripsByPerfOrgAndResource.FirstOrDefault(), uniqueTripsByPerfOrgAndResource);

									this.GenerateTaskTravelRow(wsLevelData, inputsForExport, travel, NewTaskRow, uniqueTripsByPerfOrgAndCustomFields);

									this.GenerateTripTravelRow(wsLevelData, inputsForExport, inResources, travel, out startTravelDate, out endTravelDate, p,
										resource, newResourceRow, uniqueTripsByPerfOrgAndCustomFields);

									this.ProPricerTripTravelCalulcateCost(theWorkspace, wsLevelData.PpDataToBeExported.ResourceData, startTravelDate, endTravelDate, newResourceRow, uniqueTripsByPerfOrgAndCustomFields);

									NewTaskRow.Clear();
									newResourceRow.Clear();

									//remove the trips that we used.
									uniqueTripsByPerfOrgAndCustomFields.ForEach(t => uniqueTripsByPerfOrgAndResource.Remove(t));
								}
							}
							else
							{
								this.GenerateTaskTravelRow(wsLevelData, inputsForExport, travel, NewTaskRow, uniqueTripsByPerfOrgAndResource);

								this.GenerateTripTravelRow(wsLevelData, inputsForExport, inResources, travel, out startTravelDate, out endTravelDate, p,
									resource, newResourceRow, uniqueTripsByPerfOrgAndResource);

								this.ProPricerTripTravelCalulcateCost(theWorkspace, wsLevelData.PpDataToBeExported.ResourceData, startTravelDate, endTravelDate, newResourceRow, uniqueTripsByPerfOrgAndResource);

								NewTaskRow.Clear();
								newResourceRow.Clear();
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Get the ProPricer Export rows for ODC Elements
		/// </summary>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private void ProcessOdcElements(WsLevelInputsForExport wsLevelData, BoeLevelExportData inputsForExport, ICollection<OtherDirectCostDTO> odcElements, Collection<int> inResourceIDs)
		{
			if (!odcElements.Any()) { return; }

			// the data within the BOE ODC Element will be part of the Resource Cost/Hours file
			foreach (OtherDirectCostDTO odc in odcElements)
			{
				IDictionary<int, string> odcIdToProPricerIdMappings = new Dictionary<int, string>();

				//Loop over the list of resource type entries (for the current task) 
				ICollection<OtherDirectCostType> taskResourcesEntries = odc.ODCTypes.Where(r => r.ResourceID.HasValue && inResourceIDs.Contains(r.ResourceID.Value)).ToList();
				foreach (OtherDirectCostType resourceTypeEntry in taskResourcesEntries)
				{
					StringBuilder NewTaskRow = new StringBuilder();

					// Pro Pricer Task ID. This may or may not be selected, but we may need to keep track of it for Resources too
					string proPricerID = ODC_OIDN + (++wsLevelData.ItemCounters.OdcId).ToString(FORMAT);
					odcIdToProPricerIdMappings[(int)resourceTypeEntry.ODCTypeID] = proPricerID;

					for (int x = 0; x < wsLevelData.PPInputsToExport.ProPricerTasks.Count; x++)
					{
						// get the field associated with this list order
						ProPricerTasks taskField = (from t in wsLevelData.PPInputsToExport.ProPricerTasks
										 where t.ListOrder == x
										 select t).FirstOrDefault();

						if (taskField == null)
						{
							throw new ArgumentException("The Task List in the Export Format is not ordered correctly. Please re-save the Export Format and then try to export again.");
						}

						switch (taskField.Task)
						{
							case ProPricerField_Task.BOEStartDate:
								NewTaskRow.Append(this.FormatExportDate(inputsForExport.EarliestStartDate)).Append(END_FIELD);
								break;
							case ProPricerField_Task.BOEEndDate:
								NewTaskRow.Append(this.FormatExportDate(inputsForExport.LatestEndDate)).Append(END_FIELD);
								break;
							case ProPricerField_Task.CLINNumber:
								NewTaskRow.Append((inputsForExport.Clin == null ? "" : inputsForExport.Clin.ClinNumber)).Append(END_FIELD);
								break;
							case ProPricerField_Task.CLINTitle:
								NewTaskRow.Append(DOUBLE_QUOTE).Append((inputsForExport.Clin == null ? "" : inputsForExport.Clin.ClinTitle)).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.ProPricerTaskID:
								NewTaskRow.Append(proPricerID).Append(END_FIELD);
								break;
							case ProPricerField_Task.TaskTitle:
								NewTaskRow.Append(DOUBLE_QUOTE).Append(odc.TaskTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.TOTAL:
								NewTaskRow.Append(TOTAL).Append(END_FIELD);
								break;
							case ProPricerField_Task.WBSNumber:
								NewTaskRow.Append((inputsForExport.Wbs == null ? "" : inputsForExport.Wbs.WbsNumber)).Append(END_FIELD);
								break;
							case ProPricerField_Task.WBSTitle:
								NewTaskRow.Append(DOUBLE_QUOTE).Append((inputsForExport.Wbs == null ? "" : inputsForExport.Wbs.WbsTitle)).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.GenBOEBOEID:
								NewTaskRow.Append(odc.BoeID).Append(END_FIELD);
								break;
							case ProPricerField_Task.GenBOETaskID:
								NewTaskRow.Append(odc.Id).Append(END_FIELD);
								break;
							case ProPricerField_Task.GenBOEResourceID:
								NewTaskRow.Append(resourceTypeEntry.ODCTypeID).Append(END_FIELD);
								break;
							case ProPricerField_Task.BOETitle:
								NewTaskRow.Append(DOUBLE_QUOTE).Append(inputsForExport.CleanBoeTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.TaskID:
								NewTaskRow.Append(DOUBLE_QUOTE).Append(odc.TaskID).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.BLANK:
								NewTaskRow.Append(END_FIELD);
								break;
							default:
								break;
						}

						if (taskField.CustomFieldID.HasValue)
						{
							CustomFieldType customFieldType = wsLevelData.WsCustomFields.First(i => i.Id == taskField.CustomFieldID.Value).CustomFieldDisplayID;
							ICollection<CustomFieldValueDTO> CustomFieldValues = wsLevelData.WsCustomFieldValues.Where(c => c.CustomFieldID == taskField.CustomFieldID.Value).ToCollection();

							switch (customFieldType)
							{
								case CustomFieldType.BoeDisplay:
									if (CustomFieldValues != null)
									{
										this.GetTaskBOEDisplayCustomFields(inputsForExport.CustomFieldValueContainers, NewTaskRow, taskField, CustomFieldValues);

									}
									break;
								case CustomFieldType.TaskDisplay:
								case CustomFieldType.LaborTypeDisplay:
									NewTaskRow.Append(END_FIELD);
									break;

							}
						}
					}

					wsLevelData.PpDataToBeExported.TaskData.Add(NewTaskRow.ToString());
				}

				foreach (OtherDirectCostType odcType in odc.ODCTypes)
				{
					StringBuilder newResourceRow = new StringBuilder();
					DateTime LSDate = new DateTime();


					if (odcType.StartDate.HasValue)
					{
						LSDate = odcType.StartDate.Value;

					} //only get the min spread date if the odc type doesn't have valid start date. Don't want to do this logic first since discrete spreads
					  // aren't stored in the DB and that could be potentially the 1st spread month with a value of 0
					else if (odcType.ODCSpreads.Any())
					{
						LSDate = odcType.ODCSpreads.Where(t => t.ODCSpreadDate.HasValue).Min(t => t.ODCSpreadDate.Value);
					}

					for (int x = 0; x < wsLevelData.PPInputsToExport.ProPricerResources.Count; x++)
					{
						// get the field associated with this list order
						ProPricerResources taskField = (from t in wsLevelData.PPInputsToExport.ProPricerResources
										 where t.ListOrder == x
										 select t).FirstOrDefault();

						if (taskField == null)
						{
							throw new ArgumentException("The Resource List in the Export Format is not ordered correctly. Please re-save the Export Format and then try to export again.");
						}

						switch (taskField.Resource)
						{
							case ProPricerField_Resources.CLINNumber:
								newResourceRow.Append((inputsForExport.Clin == null ? "" : inputsForExport.Clin.ClinNumber)).Append(END_FIELD);
								break;
							case ProPricerField_Resources.CLINTitle:
								newResourceRow.Append(DOUBLE_QUOTE).Append((inputsForExport.Clin == null ? "" : inputsForExport.Clin.ClinTitle.RemoveCarriageReturns())).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Resources.DISCRETE:
								newResourceRow.Append("D").Append(END_FIELD);
								break;
							case ProPricerField_Resources.IMSCode:
								newResourceRow.Append(END_FIELD);
								break;
							case ProPricerField_Resources.PerformingOrg:
								if (odcType.PerformingOrgID.HasValue)
								{
									PerformingOrgDTO perfOrg = wsLevelData.PerfOrgs.FirstOrDefault(z => z.Id == odcType.PerformingOrgID.Value);
									newResourceRow.Append(DOUBLE_QUOTE).Append(perfOrg.PerformingOrgName.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								}
								else { newResourceRow.Append(END_FIELD); }
								break;
							case ProPricerField_Resources.ProPricerTaskID:
								if (!odcIdToProPricerIdMappings.ContainsKey((int)odcType.ODCTypeID))
								{
									throw new GenValidationException("Not all types exist.  Please make sure that your data is correct.  ODC data export failed.");
								}

								newResourceRow.Append(odcIdToProPricerIdMappings[(int)odcType.ODCTypeID]).Append(END_FIELD);
								break;
							case ProPricerField_Resources.ResourceID:
								ResourceDTO resource = wsLevelData.Resources.FirstOrDefault(z => z.Id == odcType.ResourceID.Value);

								if (resource == null)
								{
									throw new GenValidationException("Not all required resources exist.  Please make sure that your data is correct.  ODC data export failed.");
								}

								newResourceRow.Append(DOUBLE_QUOTE).Append(resource.ResourceName.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Resources.StartDate:
								newResourceRow.Append(this.FormatExportDate(LSDate)).Append(END_FIELD);
								break;
							case ProPricerField_Resources.WBSNumber:
								newResourceRow.Append((inputsForExport.Wbs == null ? "" : inputsForExport.Wbs.WbsNumber)).Append(END_FIELD);
								break;
							case ProPricerField_Resources.WBSTitle:
								newResourceRow.Append(DOUBLE_QUOTE).Append((inputsForExport.Wbs == null ? "" : inputsForExport.Wbs.WbsTitle.RemoveCarriageReturns())).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Resources.GenBOEBOEID:
								newResourceRow.Append(odc.BoeID).Append(END_FIELD);
								break;
							case ProPricerField_Resources.GenBOETaskID:
								newResourceRow.Append(odc.Id).Append(END_FIELD);
								break;
							case ProPricerField_Resources.GenBOEResourceID:
								newResourceRow.Append(odcType.ODCTypeID).Append(END_FIELD);
								break;
							case ProPricerField_Resources.BOETitle:
								newResourceRow.Append(DOUBLE_QUOTE).Append(inputsForExport.CleanBoeTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Resources.TaskTitle:
								newResourceRow.Append(DOUBLE_QUOTE).Append(odc.TaskTitle.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Resources.TaskID:
								newResourceRow.Append(DOUBLE_QUOTE).Append(odc.TaskID).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Resources.BLANK:
								newResourceRow.Append(END_FIELD);
								break;
							case ProPricerField_Resources.EP:
								newResourceRow.Append(END_FIELD);
								break;
							default:
								break;
						}

						if (taskField.CustomFieldID.HasValue)
						{
							ICollection<CustomFieldValueDTO> CustomFieldValues = wsLevelData.WsCustomFieldValues.Where(c => c.CustomFieldID == taskField.CustomFieldID.Value).ToCollection();

							CustomFieldType customFieldType = wsLevelData.WsCustomFields.First(i => i.Id == taskField.CustomFieldID.Value).CustomFieldDisplayID;

							switch (customFieldType)
							{
								case CustomFieldType.BoeDisplay:
									if (CustomFieldValues != null && CustomFieldValues.Any())
									{
										this.GetResourceBOEDisplayCustomFields(inputsForExport.CustomFieldValueContainers, newResourceRow, taskField, CustomFieldValues);
									}
									break;
								case CustomFieldType.TaskDisplay:
								case CustomFieldType.LaborTypeDisplay:
									newResourceRow.Append(END_FIELD);
									break;
							}

						}
					}

					// Now get all the ODC spreads
					DateTime CurrentDate = odcType.StartDate.Value;
					while (CurrentDate <= odcType.EndDate.Value)
					{
						OtherDirectCostSpread odcSpread = (from s in odcType.ODCSpreads
										 where s.ODCSpreadDate.Value.Month == CurrentDate.Month &&
										  s.ODCSpreadDate.Value.Year == CurrentDate.Year
										 select s).FirstOrDefault();

						// discrete odc spreads aren't stored in the db so if the matching month/year isn't found then export a 0
						if (odcSpread != null)
						{
							newResourceRow.Append(((decimal)odcSpread.CostSpreadValue.Value / 100)).Append(END_FIELD);
						}
						else
						{
							newResourceRow.Append("0").Append(END_FIELD);
						}

						CurrentDate = CurrentDate.AddMonths(1);
					}

					wsLevelData.PpDataToBeExported.ResourceData.Add(newResourceRow.ToString());
				}
			}
		}

		#endregion

		#region Helpers that write individual rows

		/// <summary>
		/// Creating a task row for propricer
		/// </summary>
		/// <param name="isResourceOffloaded">if set to <c>true</c> then the Task Element is offloaded.</param>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		private IDictionary<int, string> GenerateTaskRow(WsLevelInputsForExport wsLevelData, BoeLevelExportData inputsForExport, ClinDTO clin, WbsDTO wbs,
			ElementOfCostType elementOfCost, List<ResourceTypeDto> taskResourcesEntriesForElementOfCost,
			BoeTaskElementDTO boeTask, bool isResourceOffloaded, string workspaceShortname)
		{
			// Key - labor type ID, value - ProPricer Task ID
			IDictionary<int, string> laborTypeIdToProPricerIdMappings = new Dictionary<int, string>();

			// Pseudocode for updated task row generation - PROPH-2463
			// Generate the line WITHOUT the ProPricer ID in front
			// Keep track of the last generated line and previous generated task ID outside of this method, do a compare of the line
			//	If the same line, use (set in the dictionary) the previous line's ProPricer ID
			//	If different, increment the correct ProPricer ID, prepend to the line, add the entire line to the correct list of lines,
			//	  and reset the previous generated task ID and previous generated line with the current values
			// Use the isResourceOffloaded flag for the if check (see below)

			bool firstResourceTypeEntry = true;
			string proPricerId = null;

			// Default values for the previously generated task row, used for comparing and incrementing the task ID as needed
			string previousGeneratedTaskRow = string.Empty;
			string previousGeneratedTaskId = string.Empty;
			int previousLaborTypeId = -1;

			/*
             * Loop over the list of resource type entries (for the current task) that correspond to the designated Element of Cost (input parameter).
             * 
             */
			foreach (ResourceTypeDto resourceTypeEntry in taskResourcesEntriesForElementOfCost)
			{
				// ProjectMap only wants the task exported once whereas everyone else wants it 1:1 with the number of ResourceTypes inside it
				bool generateNewTask = firstResourceTypeEntry || !wsLevelData.IsProjectMapWorkspace;

				// Generate the task line
				if (generateNewTask)
				{
					firstResourceTypeEntry = false;
					StringBuilder newTaskRow = new StringBuilder();
					for (int x = 0; x < wsLevelData.PPInputsToExport.ProPricerTasks.Count; x++)
					{
						// get the field associated with this list order
						ProPricerTasks taskField = (from t in wsLevelData.PPInputsToExport.ProPricerTasks
													where t.ListOrder == x
													select t).FirstOrDefault();

						if (taskField == null)
						{
							throw new ArgumentException("The Tasks in the Export Format are not ordered correctly. Please re-save the Export Format and then try to export again.");
						}

						switch (taskField.Task)
						{
							case ProPricerField_Task.BOEStartDate:
							case ProPricerField_Task.ProjMapStartDate:
								newTaskRow.Append(this.FormatExportDate(inputsForExport.EarliestStartDate)).Append(END_FIELD);
								break;
							case ProPricerField_Task.BOEEndDate:
							case ProPricerField_Task.ProjMapEndDate:
								newTaskRow.Append(this.FormatExportDate(inputsForExport.LatestEndDate)).Append(END_FIELD);
								break;
							case ProPricerField_Task.CLINNumber:
							case ProPricerField_Task.ProjMapClin:
								newTaskRow.Append((clin == null ? "" : clin.ClinNumber)).Append(END_FIELD);
								break;
							case ProPricerField_Task.CLINTitle:
								newTaskRow.Append(DOUBLE_QUOTE).Append((clin == null ? "" : clin.ClinTitle.RemoveCarriageReturns())).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							// Below is removed as of PROPH-2463, leaving this commented here just in case
							/* case ProPricerField_Task.ProPricerTaskID:
								newTaskRow.Append(proPricerId).Append(END_FIELD);
								break; */
							case ProPricerField_Task.TaskTitle:
								newTaskRow.Append(DOUBLE_QUOTE).Append(boeTask.TaskTitle.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.TOTAL:
								newTaskRow.Append(TOTAL).Append(END_FIELD);
								break;
							case ProPricerField_Task.WBSNumber:
							case ProPricerField_Task.ProjMapWbsNumber:
								newTaskRow.Append((wbs == null ? "" : wbs.WbsNumber)).Append(END_FIELD);
								break;
							case ProPricerField_Task.WBSTitle:
								newTaskRow.Append(DOUBLE_QUOTE).Append((wbs == null ? "" : wbs.WbsTitle.RemoveCarriageReturns())).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.MOQType:
								newTaskRow.Append(DOUBLE_QUOTE).Append(GetTaskMoqType(wsLevelData, boeTask)).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.PerformingOrg:
							case ProPricerField_Task.ProjMapCostCenter:
								string performingOrgName = wsLevelData.PerfOrgs.First(i => i.Id == resourceTypeEntry.PerformingOrgID.Value).PerformingOrgName;
								newTaskRow.Append(DOUBLE_QUOTE).Append(performingOrgName.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.ResourceID:
								string resourceName = wsLevelData.Resources.First(i => i.Id == resourceTypeEntry.ResourceID.Value).ResourceName;
								newTaskRow.Append(DOUBLE_QUOTE).Append(resourceName.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.GenBOEBOEID:
								newTaskRow.Append(boeTask.BoeID).Append(END_FIELD);
								break;
							case ProPricerField_Task.GenBOETaskID:
								newTaskRow.Append(boeTask.Id).Append(END_FIELD);
								break;
							case ProPricerField_Task.GenBOEResourceID:
								newTaskRow.Append(resourceTypeEntry.Id.ToString()).Append(END_FIELD);
								break;
							case ProPricerField_Task.BOETitle:
							case ProPricerField_Task.ProjMapTaskId:
								newTaskRow.Append(DOUBLE_QUOTE).Append(inputsForExport.CleanBoeTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.TaskID:
								newTaskRow.Append(DOUBLE_QUOTE).Append(boeTask.BOETaskID).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.BLANK:
								newTaskRow.Append(END_FIELD);
								break;
							case ProPricerField_Task.ProjMapActivityName:
								newTaskRow.Append(DOUBLE_QUOTE).Append(inputsForExport.CleanBoeDescription.Truncate(40)).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.ProjMapQuantity:
								newTaskRow.Append(DOUBLE_QUOTE).Append("1").Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.ProjMapCamName:
								string camName = inputsForExport.CamName ?? boeTask.CamName;
								newTaskRow.Append(DOUBLE_QUOTE).Append(camName.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.ProjMapSowNumber:
								string sow = inputsForExport.Sow ?? boeTask.SOW;
								newTaskRow.Append(DOUBLE_QUOTE).Append(sow.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.ProjMapAddDelete:
								newTaskRow.Append(DOUBLE_QUOTE).Append(resourceTypeEntry.AddOrDelete).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.ProjMapCategory:
								string category = inputsForExport.Category ?? boeTask.Category;
								newTaskRow.Append(DOUBLE_QUOTE).Append(category.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.ProjMapClassOfCost:
								string classOfCost = (inputsForExport.ClassOfCost == ClassOfCost.None ? boeTask.ClassOfCost : inputsForExport.ClassOfCost).GetDescription();
								newTaskRow.Append(DOUBLE_QUOTE).Append(classOfCost).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.ProjMapOldResource:
								newTaskRow.Append(DOUBLE_QUOTE).Append(this.commonDataMapper.GetSikorskyLegacyResourceID(resourceTypeEntry.LegacyID, this.allLegacyResources)).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.ProjMapResourceSegmentRegion:
							case ProPricerField_Task.ResourceSegmentRegion:
								string resourceSegRegion = GetMatchingSegmentRegionForResources(wsLevelData, resourceTypeEntry.ResourceID);
								newTaskRow.Append(DOUBLE_QUOTE).Append(resourceSegRegion.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.TaskUrl:
								string taskUrl = ConfigurationUtilities.GetAppSetting("ServerURL") + "/" + workspaceShortname + "/BOE/EditBOEIndex/boe/" + boeTask.BoeID + "#LMLabor/task/" + boeTask.Id;
								newTaskRow.Append(DOUBLE_QUOTE).Append(taskUrl.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
							case ProPricerField_Task.WorkspaceUrl:
								string workspaceUrl = ConfigurationUtilities.GetAppSetting("ServerURL") + "/" + workspaceShortname;
								newTaskRow.Append(DOUBLE_QUOTE).Append(workspaceUrl.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
								break;
						}

						if (taskField?.CustomFieldID != null)
						{
							CustomFieldType customFieldType = wsLevelData.WsCustomFields.First(i => i.Id == taskField.CustomFieldID.Value).CustomFieldDisplayID;
							ICollection<CustomFieldValueDTO> CustomFieldValues = wsLevelData.WsCustomFieldValues.Where(i => i.CustomFieldID == taskField.CustomFieldID.Value).ToCollection<CustomFieldValueDTO>();

							switch (customFieldType)
							{
								case CustomFieldType.BoeDisplay:
									if (CustomFieldValues != null && CustomFieldValues.Any())
									{
										this.GetTaskBOEDisplayCustomFields(inputsForExport.CustomFieldValueContainers, newTaskRow, taskField, CustomFieldValues);
									}
									break;
								case CustomFieldType.TaskDisplay:
									if (CustomFieldValues != null && CustomFieldValues.Any())
									{
										//check if custom field is within the task
										CustomFieldValueDTO taskCustomValue = CustomFieldValues.FirstOrDefault(c => boeTask.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));

										if (taskCustomValue != null)
										{
											if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
											{
												newTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
											}

											if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
											{
												newTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
											}
										}
										else // if the task custom field isn't in use, then we still need to add a placeholder for it
										{
											newTaskRow.Append(END_FIELD);
										}
									}
									break;
								case CustomFieldType.LaborTypeDisplay:
									if (CustomFieldValues != null && CustomFieldValues.Any())
									{
										//check if custom field is within the resource type
										CustomFieldValueDTO resourceCustomValue = CustomFieldValues.FirstOrDefault(c => resourceTypeEntry.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));

										if (resourceCustomValue != null)
										{
											if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
											{
												newTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(resourceCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
											}

											if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
											{
												newTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(resourceCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
											}
										}
										else // if the task custom field isn't in use, then we still need to add a placeholder for it
										{
											newTaskRow.Append(END_FIELD);
										}
									}
									break;
							}
						}
					}

					// If the new task row is the same as the previous line, use the previous line's ProPricer ID and do not generate a new row
					// Otherwise, increment the correct ProPricer ID, prepend to the line, and add the entire line to the correct list of lines
					if ((newTaskRow.ToString()).Equals(previousGeneratedTaskRow) && previousLaborTypeId == resourceTypeEntry.OriginalID)
					{
						laborTypeIdToProPricerIdMappings[resourceTypeEntry.Id] = previousGeneratedTaskId;
					}
					else
					{
						// Increment the correct ProPricer ID
						string taskIDString = string.Empty;
						int incrementCount = 0;

						if (elementOfCost == ElementOfCostType.LMLabor)
						{
							taskIDString = ISGS_LABOR_LIDN;
							incrementCount = ++wsLevelData.ItemCounters.LaborId;
						}
						else if (elementOfCost == ElementOfCostType.IWTA)
						{
							taskIDString = IWTA_IIDN;
							incrementCount = ++wsLevelData.ItemCounters.IwtaId;
						}
						else if (elementOfCost == ElementOfCostType.Sub)
						{
							taskIDString = SUBCONTRACTOR_SIDN;
							incrementCount = ++wsLevelData.ItemCounters.SubId;
						}
						else if (elementOfCost == ElementOfCostType.Materials)
						{
							taskIDString = MATERIAL_MIDN;
							incrementCount = ++wsLevelData.ItemCounters.MaterialId;
						}
						else if (elementOfCost == ElementOfCostType.Travel)
						{
							taskIDString = TRAVEL_TIDN;
							incrementCount = ++wsLevelData.ItemCounters.TravelId;
						}
						else if (elementOfCost == ElementOfCostType.ODC)
						{
							taskIDString = ODC_OIDN;
							incrementCount = ++wsLevelData.ItemCounters.OdcId;
						}

						proPricerId = taskIDString + incrementCount.ToString(FORMAT);

						laborTypeIdToProPricerIdMappings[resourceTypeEntry.Id] = proPricerId;

						// Reset the previously generated line with the current value (before we append the ProPricer ID)
						previousGeneratedTaskRow = newTaskRow.ToString();

						// Prepend to the line
						newTaskRow.Insert(0, proPricerId + END_FIELD);

						// Add the entire line to the correct list of lines
						// This is where the row generating happens
						if (isResourceOffloaded)
						{
							wsLevelData.PpDataToBeExported.OffloadTaskData.Add(newTaskRow.ToString());
						}
						else
						{
							wsLevelData.PpDataToBeExported.TaskData.Add(newTaskRow.ToString());
						}

						// Reset the previously generated task ID with this current value
						previousGeneratedTaskId = proPricerId;
					}
				}
				else
				{
					// make sure we map the labor type Id to the task ID
					laborTypeIdToProPricerIdMappings[resourceTypeEntry.Id] = previousGeneratedTaskId;
				}

				previousLaborTypeId = resourceTypeEntry.Id;
			}

			return laborTypeIdToProPricerIdMappings;
		}

		/// <summary>
		/// Searches for a resource with the given resource ID and returns its SegRegion property. The search is performed in the following order:
		/// 1. In the ResourcesInWs property of the given WsLevelData by Id.
		/// 2. In the Resources property of the given WsLevelData by Id.
		/// 3. In the ResourcesInWs property of the given WsLevelData by ResourceName (if found in step 2).
		/// </summary>
		/// <param name="wsLevelData">The WsLevelData to search in.</param>
		/// <param name="resourceId">The ID of the resource to find.</param>
		/// <returns>The SegRegion property of the matching resource, or an empty string if no match is found.</returns>
		private string GetMatchingSegmentRegionForResources(WsLevelInputsForExport wsLevelData, int? resourceId)
		{
			if (resourceId == null)
			{
				return string.Empty;
			}

			// First, look in the ResourcesInWs property based on the WsLevelData by Id
			ResourceDTO resource = wsLevelData.ResourcesInWs.FirstOrDefault(r => r.Id == resourceId);
			if (resource != null)
			{
				return resource.SegRegion;
			}

			// Match against wsLeveData.Resources by Id
			resource = wsLevelData.Resources.FirstOrDefault(r => r.Id == resourceId);
			if (resource != null)
			{
				string resourceName = resource.ResourceName;

				// Now match against wsLevelData.ResourcesInWs by ResourceName (from match above)
				ResourceDTO resourceInWs = wsLevelData.ResourcesInWs.FirstOrDefault(r => r.ResourceName == resourceName);
				if (resourceInWs != null)
				{
					return resourceInWs.SegRegion;
				}
			}

			return string.Empty;
		}

		/// <summary>
		/// Gets Task's MOQ Type for the export
		/// </summary>
		/// <param name="wsLevelData">WS Level Data</param>
		/// <param name="boeTask">Boe Task</param>
		/// <returns>MOQ Type String</returns>
		private static string GetTaskMoqType(WsLevelInputsForExport wsLevelData, BoeTaskElementDTO boeTask)
		{
			string moqType = string.Empty;

			if (wsLevelData.IsUsingTemplateBOE)
			{
				List<MoqTypeSelection> moqTypes = wsLevelData.MoqTypes.Where(z => z.TaskId == boeTask.Id).ToList();
				if (moqTypes.Count == 1)
				{
					moqType = moqTypes.First().SelectedMOQTypeText;
				}
				else if (moqTypes.Count > 1)
				{
					moqType = "Multiple";
				}
			}
			else
			{
				moqType = boeTask.MOQType.GetDescription();
			}

			return moqType;
		}

		/// <summary>
		/// Creates the resource row for output format. 
		/// </summary>
		/// <param name="isResourceOffloaded">if set to <c>true</c> then the Task Element is offloaded.</param>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private void GenerateResourceRow(WsLevelInputsForExport wsLevelData, BoeLevelExportData inputsForExport, ClinDTO clin, WbsDTO wbs,
			Collection<int> inResourceIDs, BoeTaskElementDTO boeTask,
			IDictionary<int, string> laborTypeIdToProPricerIdMappings, ResourceTypeDto labor, bool isUsingEquivalentPerson, bool isResourceOffloaded, string workspaceShortname)
		{
			// only want to export the resource associated with correct list of Resource IDs. 
			// For ex, if the labor contained 3 labors: 1 Labor, 1 IWTA, and 1 SubContractor. We only want to export the row that matched the current element of cost
			// we're searching for. Without this check, all 3 labors would be exported with labor, and then again with iwta elements, and lastly with subcontractors
			if (inResourceIDs.Contains(labor.ResourceID.Value))
			{
				StringBuilder newResourceRow = new StringBuilder();
				DateTime LSDate = new DateTime();

				// since Discrete spreads with 0 values aren't stored in the DB, use the labor start date to accurately display the 1st spread start date
				if (labor.StartDate.HasValue)
				{
					LSDate = labor.StartDate.Value;
				}
				else if (labor.LaborSpreads.Any()) // if labor start date isn't in the DB, then use the 1st start date of all the spreads
				{
					LSDate = labor.LaborSpreads.Min(t => t.LaborSpreadDate);
				}
				else
				{
					LSDate = inputsForExport.EarliestStartDate;
				}

				for (int x = 0; x < wsLevelData.PPInputsToExport.ProPricerResources.Count; x++)
				{
					// get the field associated with this list order
					ProPricerResources taskField = (from t in wsLevelData.PPInputsToExport.ProPricerResources
													where t.ListOrder == x
													select t).FirstOrDefault();

					switch (taskField.Resource)
					{
						case ProPricerField_Resources.CLINNumber:
							newResourceRow.Append(clin == null ? "" : clin.ClinNumber).Append(END_FIELD);
							break;
						case ProPricerField_Resources.CLINTitle:
							newResourceRow.Append(DOUBLE_QUOTE).Append(clin == null ? "" : clin.ClinTitle.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
						case ProPricerField_Resources.DISCRETE:
							newResourceRow.Append("D").Append(END_FIELD);
							break;
						case ProPricerField_Resources.IMSCode:
							newResourceRow.Append(boeTask.IMS_ID).Append(END_FIELD);
							break;
						case ProPricerField_Resources.PerformingOrg:
							PerformingOrgDTO perfOrg = wsLevelData.PerfOrgs.First(z => z.Id == labor.PerformingOrgID.Value);
							newResourceRow.Append(DOUBLE_QUOTE).Append(perfOrg.PerformingOrgName.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
						case ProPricerField_Resources.ProPricerTaskID:
							newResourceRow.Append(laborTypeIdToProPricerIdMappings[labor.Id]).Append(END_FIELD);
							break;
						case ProPricerField_Resources.ResourceID:
						case ProPricerField_Resources.ProjMapInitialResoure:
							ResourceDTO resource = wsLevelData.Resources.First(z => z.Id == labor.ResourceID.Value); 
							newResourceRow.Append(DOUBLE_QUOTE).Append(resource.ResourceName.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
						case ProPricerField_Resources.StartDate:
						case ProPricerField_Resources.ProjMapStartDate:
							newResourceRow.Append(this.FormatExportDate(LSDate)).Append(END_FIELD);
							break;
						case ProPricerField_Resources.WBSNumber:
							newResourceRow.Append((wbs == null ? "" : wbs.WbsNumber)).Append(END_FIELD);
							break;
						case ProPricerField_Resources.WBSTitle:
							newResourceRow.Append(DOUBLE_QUOTE).Append((wbs == null ? "" : wbs.WbsTitle.RemoveCarriageReturns())).Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
						case ProPricerField_Resources.GenBOEBOEID:
							newResourceRow.Append(boeTask.BoeID).Append(END_FIELD);
							break;
						case ProPricerField_Resources.GenBOETaskID:
							newResourceRow.Append(boeTask.Id).Append(END_FIELD);
							break;
						case ProPricerField_Resources.GenBOEResourceID:
							newResourceRow.Append(labor.Id).Append(END_FIELD);
							break;
						case ProPricerField_Resources.BOETitle:
						case ProPricerField_Resources.ProjMapTaskId:
							newResourceRow.Append(DOUBLE_QUOTE).Append(inputsForExport.CleanBoeTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
						case ProPricerField_Resources.TaskTitle:
							newResourceRow.Append(DOUBLE_QUOTE).Append(boeTask.TaskTitle.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
						case ProPricerField_Resources.TaskID:
							newResourceRow.Append(DOUBLE_QUOTE).Append(boeTask.BOETaskID).Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
						case ProPricerField_Resources.BLANK:
							newResourceRow.Append(END_FIELD);
							break;
						case ProPricerField_Resources.EP:
							if (isUsingEquivalentPerson && labor.SpreadType.Equals(SpreadType.Hours))
							{
								newResourceRow.Append(DOUBLE_QUOTE).Append("E").Append(DOUBLE_QUOTE).Append(END_FIELD);
							}
							else
							{
								newResourceRow.Append(END_FIELD);
							}
							break;
						case ProPricerField_Resources.ProjMapSpreadCode:
							newResourceRow.Append(DOUBLE_QUOTE).Append("D").Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
						case ProPricerField_Resources.ProjMapOldResource:
							newResourceRow.Append(DOUBLE_QUOTE).Append(this.commonDataMapper.GetSikorskyLegacyResourceID(labor.LegacyID, this.allLegacyResources)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
						case ProPricerField_Resources.ProjMapResourceSegmentRegion:
						case ProPricerField_Resources.ResourceSegmentRegion:
							string resourceSegRegion = GetMatchingSegmentRegionForResources(wsLevelData, labor.ResourceID);
							newResourceRow.Append(DOUBLE_QUOTE).Append(resourceSegRegion.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
						case ProPricerField_Resources.TaskUrl:
							string taskUrl = ConfigurationUtilities.GetAppSetting("ServerURL") + "/" + workspaceShortname + "/BOE/EditBOEIndex/boe/" + boeTask.BoeID + "#LMLabor/task/" + boeTask.Id;
							newResourceRow.Append(DOUBLE_QUOTE).Append(taskUrl.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
						case ProPricerField_Resources.WorkspaceUrl:
							string workspaceUrl = ConfigurationUtilities.GetAppSetting("ServerURL") + "/" + workspaceShortname;
							newResourceRow.Append(DOUBLE_QUOTE).Append(workspaceUrl.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
							break;
					}

					if (taskField.CustomFieldID.HasValue)
					{
						ICollection<CustomFieldValueDTO> CustomFieldValues = wsLevelData.WsCustomFieldValues.Where(i => i.CustomFieldID == taskField.CustomFieldID.Value).ToCollection();

						CustomFieldType customFieldType = wsLevelData.WsCustomFields.First(i => i.Id == taskField.CustomFieldID.Value).CustomFieldDisplayID;

						switch (customFieldType)
						{
							case CustomFieldType.BoeDisplay:
								if (CustomFieldValues != null && CustomFieldValues.Any())
								{
									this.GetResourceBOEDisplayCustomFields(inputsForExport.CustomFieldValueContainers, newResourceRow, taskField, CustomFieldValues);
								}
								break;
							case CustomFieldType.TaskDisplay:
								if (CustomFieldValues != null && CustomFieldValues.Any())
								{

									//check if custom field is within the task
									CustomFieldValueDTO taskCustomValue = CustomFieldValues.FirstOrDefault(c => boeTask.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));

									if (taskCustomValue != null)
									{
										if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
										{
											newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
										}

										if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
										{
											newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
										}
									}
									else // if the task custom field isn't in use, then we still need to add a placeholder for it
									{
										newResourceRow.Append(END_FIELD);
									}
								}
								break;
							case CustomFieldType.LaborTypeDisplay:
								if (CustomFieldValues != null && CustomFieldValues.Any())
								{
									CustomFieldValueDTO resourceCustomValue = CustomFieldValues.FirstOrDefault(c => labor.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));

									if (resourceCustomValue != null)
									{
										if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
										{
											newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(resourceCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
										}

										if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
										{
											newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(resourceCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
										}
									}
									else
									{
										newResourceRow.Append(END_FIELD);
									}
								}
								break;
						}
					}
				}

				// Now get all the labor spreads
				// Discrete spreads are only stored in the DB when a value for the month has been entered so to account for 0 months, add 0
				// if the month is missing in the start/end date range
				DateTime currentDate = labor.StartDateValue;
				while (currentDate <= labor.EndDateValue)
				{
					ResourceSpreadDto laborSpread = (from s in labor.LaborSpreads
													 where s.LaborSpreadDate.Month == currentDate.Month &&
													  s.LaborSpreadDate.Year == currentDate.Year
													 select s).FirstOrDefault();

					if (laborSpread != null)
					{
						if (labor.SpreadType.Equals(SpreadType.Hours))
						{
							newResourceRow.Append(Utilities.FormatStringWithPrecisionNoComma(laborSpread.LaborSpreadValue, wsLevelData.ResourceDecimalPrecision)).Append(END_FIELD);
						}
						else if (labor.SpreadType.Equals(SpreadType.Cost))
						{
							decimal costValue = laborSpread.LaborSpreadValue;
							newResourceRow.Append(costValue.ToString("F" + wsLevelData.CostDecimalPrecision)).Append(END_FIELD);
						}
					}
					else
					{
						newResourceRow.Append("0").Append(END_FIELD);
					}
					currentDate = currentDate.AddMonths(1);
				}

				if (isResourceOffloaded)
				{
					wsLevelData.PpDataToBeExported.OffloadResourceData.Add(newResourceRow.ToString());
				}
				else
				{
					wsLevelData.PpDataToBeExported.ResourceData.Add(newResourceRow.ToString());
				}
			}
		}

		/// <summary>
		/// Generates MST Trip Travel Row
		/// </summary>
		/// <param name="wsLevelData">Ws Level Data</param>
		/// <param name="inputsForExport">Inputs for Export</param>
		/// <param name="travel">Travel Data</param>
		/// <param name="trip">Trip Data</param>
		private void GenerateMSTTripTravelRow(WsLevelInputsForExport wsLevelData, BoeLevelExportData inputsForExport, TravelDTO travel, MSTTravelTripType trip)
		{
			this.GenerateMSTTripTravelRowInner(wsLevelData, inputsForExport, travel, trip, false);

			if (trip.ModeID == MSTTravelMode.ZoneAirfare)
			{
				this.GenerateMSTTripTravelRowInner(wsLevelData, inputsForExport, travel, trip, true);
			}
		}

		/// <summary>
		/// Generates MST Trip Travel Row
		/// </summary>
		/// <param name="wsLevelData">Ws Level Data</param>
		/// <param name="inputsForExport">Inputs for Export</param>
		/// <param name="travel">Travel Data</param>
		/// <param name="trip">Trip Data</param>
		/// <param name="izZoneAirfare">Indicates whether the mode in which we are running is Zone with Airfare (which results in a slightly different data being used)</param>
		private void GenerateMSTTripTravelRowInner(WsLevelInputsForExport wsLevelData, BoeLevelExportData inputsForExport, TravelDTO travel, MSTTravelTripType trip, bool izZoneAirfare)
		{
			StringBuilder newResourceRow = new StringBuilder();

			for (int x = 0; x < wsLevelData.PPInputsToExport.ProPricerResources.Count; x++)
			{
				// get the field associated with this list order
				ProPricerResources taskField = (from t in wsLevelData.PPInputsToExport.ProPricerResources
												where t.ListOrder == x
												select t).FirstOrDefault();

				if (taskField == null)
				{
					throw new ArgumentException("The Resource List in the Export Format is not ordered correctly. Please re-save the Export Format and then try to export again.");
				}

				switch (taskField.Resource)
				{
					case ProPricerField_Resources.CLINNumber:
						string clinNumber = inputsForExport.MultiBoe ? trip.ClinNumber : (inputsForExport.Clin == null ? string.Empty : inputsForExport.Clin.ClinNumber);

						newResourceRow.Append(clinNumber).Append(END_FIELD);
						break;
					case ProPricerField_Resources.CLINTitle:
						string clinTitle = inputsForExport.MultiBoe ? trip.ClinTitle : (inputsForExport.Clin == null ? string.Empty : inputsForExport.Clin.ClinTitle);

						newResourceRow.Append(DOUBLE_QUOTE).Append(clinTitle.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.DISCRETE:
						newResourceRow.Append("D").Append(END_FIELD);
						break;
					case ProPricerField_Resources.IMSCode:
						newResourceRow.Append(END_FIELD);
						break;
					case ProPricerField_Resources.ProPricerTaskID:
						newResourceRow.Append(TRAVEL_TIDN).Append(wsLevelData.ItemCounters.TravelId.ToString(FORMAT)).Append(END_FIELD);
						break;
					case ProPricerField_Resources.GenBOEResourceID:
						{
							string resource = izZoneAirfare ? trip.SecondaryResourceIdForExport : trip.ResourceIdForExport;

							newResourceRow.Append(DOUBLE_QUOTE).Append(resource.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
						}
						break;
					case ProPricerField_Resources.ResourceID:
						{
							string resource = izZoneAirfare ? trip.SecondaryResourceIdForExport : trip.ResourceIdForExport;

							newResourceRow.Append(DOUBLE_QUOTE).Append(resource.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
						}
						break;
					case ProPricerField_Resources.PerformingOrg:
						PerformingOrgDTO perfOrg = wsLevelData.PerfOrgs.First(z => z.Id == trip.PerfOrgID);
						newResourceRow.Append(DOUBLE_QUOTE).Append(perfOrg.PerformingOrgName.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.StartDate:
						newResourceRow.Append(this.FormatExportDate(trip.TripDate)).Append(END_FIELD);
						break;
					case ProPricerField_Resources.WBSNumber:
						string wbsNumber = inputsForExport.MultiBoe ? trip.WbsNumber : (inputsForExport.Wbs == null ? string.Empty : inputsForExport.Wbs.WbsNumber);
						newResourceRow.Append(wbsNumber).Append(END_FIELD);
						break;
					case ProPricerField_Resources.WBSTitle:
						string wbsTitle = inputsForExport.MultiBoe ? trip.WbsTitle : (inputsForExport.Wbs == null ? string.Empty : inputsForExport.Wbs.WbsTitle);
						newResourceRow.Append(DOUBLE_QUOTE).Append(wbsTitle.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.GenBOEBOEID:
						newResourceRow.Append(travel.BoeID).Append(END_FIELD);
						break;
					case ProPricerField_Resources.GenBOETaskID:
						newResourceRow.Append(travel.Id).Append(END_FIELD);
						break;
					case ProPricerField_Resources.BOETitle:
						newResourceRow.Append(DOUBLE_QUOTE).Append(inputsForExport.CleanBoeTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.TaskTitle:
						newResourceRow.Append(DOUBLE_QUOTE).Append(travel.TaskTitle.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.TaskID:
						newResourceRow.Append(DOUBLE_QUOTE).Append(travel.TaskID).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.BLANK:
						newResourceRow.Append(END_FIELD);
						break;
					case ProPricerField_Resources.EP:
						newResourceRow.Append(END_FIELD);
						break;
					default:
						break;
				}

				if (taskField.CustomFieldID.HasValue)
				{
					this.GenerateMSTTripTravelRowCustomFields(inputsForExport.CustomFieldValueContainers, wsLevelData, newResourceRow, taskField, travel, trip);
				}
			}
			if (trip.ModeID == MSTTravelMode.NonZoneDomestic || trip.ModeID == MSTTravelMode.NonZoneInternational)
			{
				// always write trip cost to last col if travel is non-zone
				newResourceRow.Append(DOUBLE_QUOTE).Append(trip.Cost.ToString("F2")).Append(DOUBLE_QUOTE).Append(END_FIELD);
			}
			else // zone travel, add # of people (perdiem only) or add #of people * # of days if
			{
				decimal zoneTripVal = izZoneAirfare ? trip.NumOfPeople.GetValueOrDefault(0) : (trip.NumOfPeople * trip.NumOfDays).GetValueOrDefault(0);
				newResourceRow.Append(DOUBLE_QUOTE).Append(zoneTripVal.ToString("F2")).Append(DOUBLE_QUOTE).Append(END_FIELD);
			}
			wsLevelData.PpDataToBeExported.ResourceData.Add(newResourceRow.ToString());
		}

		/// <summary>
		/// prints out travel task level information for propricer
		/// </summary>
		/// <param name="wsLevelData">Ws Level Data</param>
		/// <param name="inputsForExport">Inputs for Export</param>
		/// <param name="travel">Travel Data</param>
		/// <param name="trip">Trip Data</param>
		private void GenerateMSTTaskTravelRow(WsLevelInputsForExport wsLevelData, BoeLevelExportData inputsForExport, TravelDTO travel, MSTTravelTripType trip)
		{
			StringBuilder NewTaskRow = new StringBuilder();
			for (int x = 0; x < wsLevelData.PPInputsToExport.ProPricerTasks.Count; x++)
			{
				// get the field associated with this list order
				ProPricerTasks taskField = (from t in wsLevelData.PPInputsToExport.ProPricerTasks
								 where t.ListOrder == x
								 select t).FirstOrDefault();

				if (taskField == null)
				{
					throw new ArgumentException("The Task List in the Export Format is not ordered correctly. Please re-save the Export Format and then try to export again.");
				}

				switch (taskField.Task)
				{
					case ProPricerField_Task.BOEStartDate:
						NewTaskRow.Append(this.FormatExportDate(inputsForExport.EarliestStartDate)).Append(END_FIELD);
						break;
					case ProPricerField_Task.BOEEndDate:
						NewTaskRow.Append(this.FormatExportDate(inputsForExport.LatestEndDate)).Append(END_FIELD);
						break;
					case ProPricerField_Task.CLINNumber:
						string clinNumber = inputsForExport.MultiBoe ? trip.ClinNumber : (inputsForExport.Clin == null ? string.Empty : inputsForExport.Clin.ClinNumber);
						NewTaskRow.Append(clinNumber).Append(END_FIELD);
						break;
					case ProPricerField_Task.CLINTitle:
						string clinTitle = inputsForExport.MultiBoe ? trip.ClinTitle : (inputsForExport.Clin == null ? string.Empty : inputsForExport.Clin.ClinTitle);
						NewTaskRow.Append(DOUBLE_QUOTE).Append(clinTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.ProPricerTaskID:
						NewTaskRow.Append(TRAVEL_TIDN).Append(wsLevelData.ItemCounters.TravelId.ToString(FORMAT)).Append(END_FIELD);
						break;
					case ProPricerField_Task.GenBOEResourceID:
						NewTaskRow.Append(DOUBLE_QUOTE).Append(trip.ResourceIdForExport).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.ResourceID:
						NewTaskRow.Append(DOUBLE_QUOTE).Append(trip.ResourceIdForExport).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.PerformingOrg:
						PerformingOrgDTO perfOrg = wsLevelData.PerfOrgs.First(z => z.Id == trip.PerfOrgID);
						NewTaskRow.Append(DOUBLE_QUOTE).Append(perfOrg.PerformingOrgName).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.TaskTitle:
						NewTaskRow.Append(DOUBLE_QUOTE).Append(travel.TaskTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.TOTAL:
						NewTaskRow.Append(TOTAL).Append(END_FIELD);
						break;
					case ProPricerField_Task.WBSNumber:
						string wbsNumber = inputsForExport.MultiBoe ? trip.WbsNumber : (inputsForExport.Wbs == null ? string.Empty : inputsForExport.Wbs.WbsNumber);
						NewTaskRow.Append(wbsNumber).Append(END_FIELD);
						break;
					case ProPricerField_Task.WBSTitle:
						string wbsTitle = inputsForExport.MultiBoe ? trip.WbsTitle : (inputsForExport.Wbs == null ? string.Empty : inputsForExport.Wbs.WbsTitle);
						NewTaskRow.Append(DOUBLE_QUOTE).Append(wbsTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.GenBOEBOEID:
						NewTaskRow.Append(travel.BoeID).Append(END_FIELD);
						break;
					case ProPricerField_Task.GenBOETaskID:
						NewTaskRow.Append(travel.Id).Append(END_FIELD);
						break;
					case ProPricerField_Task.BOETitle:
						NewTaskRow.Append(DOUBLE_QUOTE).Append(inputsForExport.CleanBoeTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.TaskID:
						NewTaskRow.Append(DOUBLE_QUOTE).Append(travel.TaskID).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.BLANK:
						NewTaskRow.Append(END_FIELD);
						break;
					default:
						break;
				}
				if (taskField != null && taskField.CustomFieldID.HasValue)
				{
					this.GenerateMSTTaskTravelCustomField(wsLevelData, inputsForExport, travel, trip, taskField, NewTaskRow);
				}
			}
			wsLevelData.PpDataToBeExported.TaskData.Add(NewTaskRow.ToString());
		}

		/// <summary>
		/// Generates the Custom Field text for RMS Task Travel
		/// </summary>
		/// <param name="wsLevelData">WS Level Data</param>
		/// <param name="inputsForExport">Inputs for Export</param>
		/// <param name="travel">Travel Data</param>
		/// <param name="trip">Trip Data</param>
		/// <param name="taskField">Task Field</param>
		/// <param name="NewTaskRow">Task Row String</param>
		private void GenerateMSTTaskTravelCustomField(WsLevelInputsForExport wsLevelData, BoeLevelExportData inputsForExport, TravelDTO travel, MSTTravelTripType trip, ProPricerTasks taskField, StringBuilder NewTaskRow)
		{
			CustomFieldType customFieldType = wsLevelData.WsCustomFields.First(i => i.Id == taskField.CustomFieldID.Value).CustomFieldDisplayID;
			ICollection<CustomFieldValueDTO> CustomFieldValues = wsLevelData.WsCustomFieldValues.Where(i => i.CustomFieldID == taskField.CustomFieldID.Value).ToCollection();
			switch (customFieldType)
			{
				case CustomFieldType.BoeDisplay:
					if (CustomFieldValues != null && CustomFieldValues.Any())
					{
						this.GetTaskBOEDisplayCustomFields(inputsForExport.CustomFieldValueContainers, NewTaskRow, taskField, CustomFieldValues);
					}
					break;
				case CustomFieldType.TaskDisplay:
					if (CustomFieldValues != null && CustomFieldValues.Any())
					{
						//check if custom field is within the task
						CustomFieldValueDTO taskCustomValue = CustomFieldValues.FirstOrDefault(c => travel.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));

						if (taskCustomValue != null)
						{
							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
							{
								NewTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}

							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
							{
								NewTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}
						}
						else // if the task custom field isn't in use, then we still need to add a placeholder for it
						{
							NewTaskRow.Append(END_FIELD);
						}
					}
					break;
				case CustomFieldType.LaborTypeDisplay:
					if (CustomFieldValues != null && CustomFieldValues.Any())
					{
						//check if custom field is within the task
						CustomFieldValueDTO taskCustomValue = CustomFieldValues.FirstOrDefault(c => trip.CustomFieldValueContainers.Select(i => i.CustomFieldValueID).Contains(c.CustomFieldValueID));
						if (taskCustomValue != null)
						{
							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
							{
								NewTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}

							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
							{
								NewTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}
						}
						else // if the task custom field isn't in use, then we still need to add a placeholder for it
						{
							NewTaskRow.Append(END_FIELD);
						}
					}
					break;
			}
		}

		/// <summary>
		/// exports custom field information for travel trip rows
		/// </summary>
		private void GenerateMSTTripTravelRowCustomFields(Collection<CustomFieldValueContainer> boeCustomFieldContainers, WsLevelInputsForExport wsLevelData, StringBuilder newResourceRow,
			ProPricerResources taskField, TravelDTO travel, MSTTravelTripType trip)
		{
			ICollection<CustomFieldValueDTO> CustomFieldValues = wsLevelData.WsCustomFieldValues.Where(i => i.CustomFieldID == taskField.CustomFieldID.Value).ToCollection();
			CustomFieldType customFieldType = wsLevelData.WsCustomFields.First(i => i.Id == taskField.CustomFieldID.Value).CustomFieldDisplayID;

			switch (customFieldType)
			{
				case CustomFieldType.BoeDisplay:
					if (CustomFieldValues != null && CustomFieldValues.Any())
					{
						this.GetResourceBOEDisplayCustomFields(boeCustomFieldContainers, newResourceRow, taskField, CustomFieldValues);
					}
					break;
				case CustomFieldType.TaskDisplay:
					if (CustomFieldValues != null && CustomFieldValues.Any())
					{
						//check if custom field is within the task
						CustomFieldValueDTO taskCustomValue = CustomFieldValues.FirstOrDefault(c => travel.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));
						if (taskCustomValue != null)
						{
							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
							{
								newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}
							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
							{
								newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}
						}
						else // if the task custom field isn't in use, then we still need to add a placeholder for it
						{
							newResourceRow.Append(END_FIELD);
						}
					}
					break;
				case CustomFieldType.LaborTypeDisplay:
					if (CustomFieldValues != null && CustomFieldValues.Any())
					{
						//check if custom field is within the task
						CustomFieldValueDTO taskCustomValue = CustomFieldValues.FirstOrDefault(c => trip.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));

						if (taskCustomValue != null)
						{
							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
							{
								newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}

							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
							{
								newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}
						}
						else // if the task custom field isn't in use, or we have more than 1 trip
						{
							newResourceRow.Append(END_FIELD);
						}
					}
					break;
			}
		}

		private void GenerateTripTravelRow(WsLevelInputsForExport wsLevelData, BoeLevelExportData inputsForExport,
			ICollection<ResourceDTO> inResources, TravelDTO travel, out DateTime startTravelDate, out DateTime endTravelDate, int perfOrgID,
			int resource, StringBuilder newResourceRow, List<TravelData> uniqueTripsByPerfOrgAndResource)
		{
			startTravelDate = uniqueTripsByPerfOrgAndResource.Min(x => x.TravelTrip.TripDate);
			endTravelDate = uniqueTripsByPerfOrgAndResource.Max(x => x.TravelTrip.TripDate);

			for (int x = 0; x < wsLevelData.PPInputsToExport.ProPricerResources.Count; x++)
			{
				// get the field associated with this list order
				ProPricerResources taskField = (from t in wsLevelData.PPInputsToExport.ProPricerResources
												where t.ListOrder == x
												select t).FirstOrDefault();

				if (taskField == null)
				{
					throw new ArgumentException("The Resource List in the Export Format is not ordered correctly. Please re-save the Export Format and then try to export again.");
				}

				switch (taskField.Resource)
				{
					case ProPricerField_Resources.CLINNumber:
						newResourceRow.Append((inputsForExport.Clin == null ? "" : inputsForExport.Clin.ClinNumber)).Append(END_FIELD);
						break;
					case ProPricerField_Resources.CLINTitle:
						newResourceRow.Append(DOUBLE_QUOTE).Append((inputsForExport.Clin == null ? "" : inputsForExport.Clin.ClinTitle.RemoveCarriageReturns())).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.DISCRETE:
						newResourceRow.Append("D").Append(END_FIELD);
						break;
					case ProPricerField_Resources.IMSCode:
						newResourceRow.Append(END_FIELD);
						break;
					case ProPricerField_Resources.PerformingOrg:
						PerformingOrgDTO perfOrg = wsLevelData.PerfOrgs.First(z => z.Id == perfOrgID);
						newResourceRow.Append(DOUBLE_QUOTE).Append(perfOrg.PerformingOrgName.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.ProPricerTaskID:
						newResourceRow.Append(TRAVEL_TIDN).Append(wsLevelData.ItemCounters.TravelId.ToString(FORMAT)).Append(END_FIELD);
						break;
					case ProPricerField_Resources.ResourceID:
						ResourceDTO Resource = inResources.First(z => z.Id == resource);
						newResourceRow.Append(DOUBLE_QUOTE).Append(Resource.ResourceName.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.StartDate:
						newResourceRow.Append(this.FormatExportDate(startTravelDate)).Append(END_FIELD);
						break;
					case ProPricerField_Resources.WBSNumber:
						newResourceRow.Append((inputsForExport.Wbs == null ? "" : inputsForExport.Wbs.WbsNumber)).Append(END_FIELD);
						break;
					case ProPricerField_Resources.WBSTitle:
						newResourceRow.Append(DOUBLE_QUOTE).Append((inputsForExport.Wbs == null ? "" : inputsForExport.Wbs.WbsTitle.RemoveCarriageReturns())).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.GenBOEBOEID:
						newResourceRow.Append(travel.BoeID).Append(END_FIELD);
						break;
					case ProPricerField_Resources.GenBOETaskID:
						newResourceRow.Append(travel.Id).Append(END_FIELD);
						break;
					case ProPricerField_Resources.GenBOEResourceID:
						string allIds = string.Join(" ", uniqueTripsByPerfOrgAndResource.Select(t => t.TravelTrip.TravelTripID.ToString()));
						newResourceRow.Append(allIds).Append(END_FIELD);
						break;
					case ProPricerField_Resources.BOETitle:
						newResourceRow.Append(DOUBLE_QUOTE).Append(inputsForExport.CleanBoeTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.TaskTitle:
						newResourceRow.Append(DOUBLE_QUOTE).Append(travel.TaskTitle.RemoveCarriageReturns()).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.TaskID:
						newResourceRow.Append(DOUBLE_QUOTE).Append(travel.TaskID).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Resources.BLANK:
						newResourceRow.Append(END_FIELD);
						break;
					case ProPricerField_Resources.EP:
						newResourceRow.Append(END_FIELD);
						break;
					default:
						break;
				}

				if (taskField.CustomFieldID.HasValue)
				{
					this.GenerateTripTravelRowCustomFields(inputsForExport.CustomFieldValueContainers, wsLevelData, newResourceRow, taskField, travel, uniqueTripsByPerfOrgAndResource);
				}
			}
		}

		/// <summary>
		/// prints out travel task level information for propricer
		/// </summary>
		private void GenerateTaskTravelRow(WsLevelInputsForExport wsLevelData, BoeLevelExportData inputsForExport, TravelDTO travel, StringBuilder NewTaskRow,
			List<TravelData> uniqueTripsByPerfOrgAndResource)
		{
			for (int x = 0; x < wsLevelData.PPInputsToExport.ProPricerTasks.Count; x++)
			{
				// get the field associated with this list order
				ProPricerTasks taskField = (from t in wsLevelData.PPInputsToExport.ProPricerTasks
								 where t.ListOrder == x
								 select t).FirstOrDefault();

				if (taskField == null)
				{
					throw new ArgumentException("The Task List in the Export Format is not ordered correctly. Please re-save the Export Format and then try to export again.");
				}

				switch (taskField.Task)
				{
					case ProPricerField_Task.BOEStartDate:
						NewTaskRow.Append(this.FormatExportDate(inputsForExport.EarliestStartDate)).Append(END_FIELD);
						break;
					case ProPricerField_Task.BOEEndDate:
						NewTaskRow.Append(this.FormatExportDate(inputsForExport.LatestEndDate)).Append(END_FIELD);
						break;
					case ProPricerField_Task.CLINNumber:
						NewTaskRow.Append((inputsForExport.Clin == null ? "" : inputsForExport.Clin.ClinNumber)).Append(END_FIELD);
						break;
					case ProPricerField_Task.CLINTitle:
						NewTaskRow.Append(DOUBLE_QUOTE).Append((inputsForExport.Clin == null ? "" : inputsForExport.Clin.ClinTitle)).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.ProPricerTaskID:
						NewTaskRow.Append(TRAVEL_TIDN).Append(wsLevelData.ItemCounters.TravelId.ToString(FORMAT)).Append(END_FIELD);
						break;
					case ProPricerField_Task.TaskTitle:
						NewTaskRow.Append(DOUBLE_QUOTE).Append(travel.TaskTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.TOTAL:
						NewTaskRow.Append(TOTAL).Append(END_FIELD);
						break;
					case ProPricerField_Task.WBSNumber:
						NewTaskRow.Append((inputsForExport.Wbs == null ? "" : inputsForExport.Wbs.WbsNumber)).Append(END_FIELD);
						break;
					case ProPricerField_Task.WBSTitle:
						NewTaskRow.Append(DOUBLE_QUOTE).Append((inputsForExport.Wbs == null ? "" : inputsForExport.Wbs.WbsTitle)).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.GenBOEBOEID:
						NewTaskRow.Append(travel.BoeID).Append(END_FIELD);
						break;
					case ProPricerField_Task.GenBOETaskID:
						NewTaskRow.Append(travel.Id).Append(END_FIELD);
						break;
					case ProPricerField_Task.GenBOEResourceID:
						NewTaskRow.Append(END_FIELD);
						break;
					case ProPricerField_Task.BOETitle:
						NewTaskRow.Append(DOUBLE_QUOTE).Append(inputsForExport.CleanBoeTitle).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.TaskID:
						NewTaskRow.Append(DOUBLE_QUOTE).Append(travel.TaskID).Append(DOUBLE_QUOTE).Append(END_FIELD);
						break;
					case ProPricerField_Task.BLANK:
						NewTaskRow.Append(END_FIELD);
						break;
					default:
						break;
				}


				if (taskField != null && taskField.CustomFieldID.HasValue)
				{
					CustomFieldType customFieldType = wsLevelData.WsCustomFields.First(i => i.Id == taskField.CustomFieldID.Value).CustomFieldDisplayID;
					ICollection<CustomFieldValueDTO> CustomFieldValues = wsLevelData.WsCustomFieldValues.Where(i => i.CustomFieldID == taskField.CustomFieldID.Value).ToCollection();

					switch (customFieldType)
					{
						case CustomFieldType.BoeDisplay:
							if (CustomFieldValues != null && CustomFieldValues.Any())
							{
								this.GetTaskBOEDisplayCustomFields(inputsForExport.CustomFieldValueContainers, NewTaskRow, taskField, CustomFieldValues);
							}
							break;
						case CustomFieldType.TaskDisplay:
							if (CustomFieldValues != null && CustomFieldValues.Any())
							{
								//check if custom field is within the task
								CustomFieldValueDTO taskCustomValue = CustomFieldValues.FirstOrDefault(c => travel.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));

								if (taskCustomValue != null)
								{
									if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
									{
										NewTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
									}

									if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
									{
										NewTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
									}
								}
								else // if the task custom field isn't in use, then we still need to add a placeholder for it
								{
									NewTaskRow.Append(END_FIELD);
								}
							}
							break;
						case CustomFieldType.LaborTypeDisplay:
							if (CustomFieldValues != null && CustomFieldValues.Any())
							{
								//check if custom field is within the task
								CustomFieldValueDTO taskCustomValue = CustomFieldValues.FirstOrDefault(c => uniqueTripsByPerfOrgAndResource.FirstOrDefault().TravelTrip.CustomFieldValueContainers.Select(i => i.CustomFieldValueID).Contains(c.CustomFieldValueID));

								if (taskCustomValue != null)
								{
									if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
									{
										NewTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
									}

									if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
									{
										NewTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
									}
								}
								else // if the task custom field isn't in use, then we still need to add a placeholder for it
								{
									NewTaskRow.Append(END_FIELD);
								}
							}
							break;
					}
				}
			}

			wsLevelData.PpDataToBeExported.TaskData.Add(NewTaskRow.ToString());
		}

		/// <summary>
		/// exports custom field information for travel trip rows
		/// </summary>
		private void GenerateTripTravelRowCustomFields(Collection<CustomFieldValueContainer> boeCustomFieldContainers, WsLevelInputsForExport wsLevelData, StringBuilder newResourceRow,
			ProPricerResources taskField, TravelDTO travel, List<TravelData> uniqueTripsByPerfOrgAndResource)
		{
			ICollection<CustomFieldValueDTO> CustomFieldValues = wsLevelData.WsCustomFieldValues.Where(i => i.CustomFieldID == taskField.CustomFieldID.Value).ToCollection();
			CustomFieldType customFieldType = wsLevelData.WsCustomFields.First(i => i.Id == taskField.CustomFieldID.Value).CustomFieldDisplayID;

			switch (customFieldType)
			{
				case CustomFieldType.BoeDisplay:
					if (CustomFieldValues != null && CustomFieldValues.Any())
					{
						this.GetResourceBOEDisplayCustomFields(boeCustomFieldContainers, newResourceRow, taskField, CustomFieldValues);
					}
					break;
				case CustomFieldType.TaskDisplay:
					if (CustomFieldValues != null && CustomFieldValues.Any())
					{
						//check if custom field is within the task
						CustomFieldValueDTO taskCustomValue = CustomFieldValues.FirstOrDefault(c => travel.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));

						if (taskCustomValue != null)
						{
							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
							{
								newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}

							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
							{
								newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}
						}
						else // if the task custom field isn't in use, then we still need to add a placeholder for it
						{
							newResourceRow.Append(END_FIELD);
						}
					}
					break;
				case CustomFieldType.LaborTypeDisplay:
					if (CustomFieldValues != null && CustomFieldValues.Any())
					{
						//check if custom field is within the task
						CustomFieldValueDTO taskCustomValue = CustomFieldValues.FirstOrDefault(c => uniqueTripsByPerfOrgAndResource.FirstOrDefault().TravelTrip.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));

						if (taskCustomValue != null)
						{
							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
							{
								newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}

							if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
							{
								newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(taskCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
							}
						}
						else // if the task custom field isn't in use, or we have more than 1 trip
						{
							newResourceRow.Append(END_FIELD);
						}
					}
					break;
			}
		}

		#endregion

		#region Other Helpers

		/// <summary>
		/// Categorizes all task elements.
		/// </summary>
		/// <param name="collectionsOfInputs">The collections of inputs.</param>
		/// <param name="laborResourceIDs">The labor resource ids.</param>
		/// <param name="iwtaResourceIDs">The IWTA resource ids.</param>
		/// <param name="subContractorResourceIDs">The sub contractor resource ids.</param>
		/// <param name="materialResourceIDs">The material resource ids.</param>
		/// <param name="odcResourceIDs">The ODC resource ids.</param>
		/// <param name="travelResourceIDs">The travel resource ids.</param>
		/// <param name="taskElements">The task elements.</param>
		private static void CategorizeAllTaskElements(BoeLevelExportData collectionsOfInputs,
			 Collection<int> laborResourceIDs, Collection<int> iwtaResourceIDs, Collection<int> subContractorResourceIDs, Collection<int> materialResourceIDs, Collection<int> odcResourceIDs,
			 Collection<int> travelResourceIDs, ICollection<BoeTaskElementDTO> taskElements)
		{
			collectionsOfInputs.LaborElements = new Collection<BoeTaskElementDTO>(((from t in taskElements
																					from t2 in t.taskElementLabors
																					where t.taskElementLabors.Any() &&
																					laborResourceIDs.Contains(t2.ResourceID.HasValue ? t2.ResourceID.Value : t2.BusinessResourceCodeID.Value)
																					select t)).Distinct().ToArray());

			collectionsOfInputs.IWTAElements = new Collection<BoeTaskElementDTO>(((from t in taskElements
																				   from t2 in t.taskElementLabors
																				   where t.taskElementLabors.Any() &&
																				   iwtaResourceIDs.Contains(t2.ResourceID.HasValue ? t2.ResourceID.Value : t2.BusinessResourceCodeID.Value)
																				   select t)).Distinct().ToArray());

			collectionsOfInputs.SubcontractorElements = new Collection<BoeTaskElementDTO>(((from t in taskElements
																							from t2 in t.taskElementLabors
																							where t.taskElementLabors.Any() &&
																							subContractorResourceIDs.Contains(t2.ResourceID.HasValue ? t2.ResourceID.Value : t2.BusinessResourceCodeID.Value)
																							select t)).Distinct().ToArray());

			collectionsOfInputs.MaterialLaborElements = new Collection<BoeTaskElementDTO>(((from t in taskElements
																							from t2 in t.taskElementLabors
																							where t.taskElementLabors.Any() &&
																							materialResourceIDs.Contains(t2.ResourceID.HasValue ? t2.ResourceID.Value : t2.BusinessResourceCodeID.Value)
																							select t)).Distinct().ToArray());

			collectionsOfInputs.TravelLaborElements = new Collection<BoeTaskElementDTO>((from t in taskElements
																						 from t2 in t.taskElementLabors
																						 where t.taskElementLabors.Any() &&
																						 travelResourceIDs.Contains(t2.ResourceID.HasValue ? t2.ResourceID.Value : t2.BusinessResourceCodeID.Value)
																						 select t).Distinct().ToArray());

			collectionsOfInputs.ODCLaborElements = new Collection<BoeTaskElementDTO>((from t in taskElements
																					  from t2 in t.taskElementLabors
																					  where t.taskElementLabors.Any() &&
																					  odcResourceIDs.Contains(t2.ResourceID.HasValue ? t2.ResourceID.Value : t2.BusinessResourceCodeID.Value)
																					  select t).Distinct().ToArray());
		}

		private static void DetermineStartAndEndDates(BoeLevelExportData exportData, ICollection<BoeTaskElementDTO> TaskElements, ICollection<MaterialDTO> MaterialElements,
			ICollection<TravelDTO> TravelElements, ICollection<OtherDirectCostDTO> ODCElements)
		{
			DateTime earliestTaskDate = DateTime.MaxValue;
			DateTime earliestMaterialDate = DateTime.MaxValue;
			DateTime earliestTravelDate = DateTime.MaxValue;
			DateTime earliestODCDate = DateTime.MaxValue;

			DateTime latestTaskDate = DateTime.MinValue;
			DateTime latestMaterialDate = DateTime.MinValue;
			DateTime latestTravelDate = DateTime.MinValue;
			DateTime latestODCDate = DateTime.MinValue;

			if (TaskElements.Any())
			{
				earliestTaskDate = TaskElements.Min(t => (t.StartDate.HasValue ? t.StartDate.Value : DateTime.MaxValue));
				latestTaskDate = TaskElements.Max(t => t.EndDate.HasValue ? t.EndDate.Value : DateTime.MinValue);
			}

			if (MaterialElements.Any())
			{
				earliestMaterialDate = MaterialElements.Min(t => (t.StartDate.HasValue ? t.StartDate.Value : DateTime.MaxValue));
				latestMaterialDate = MaterialElements.Max(t => t.EndDate.HasValue ? t.EndDate.Value : DateTime.MinValue);
			}

			if (TravelElements.Any())
			{
				earliestTravelDate = TravelElements.Min(t => t.StartDate ?? DateTime.MaxValue);
				latestTravelDate = TravelElements.Max(t => t.EndDate ?? DateTime.MinValue);
			}

			if (ODCElements.Any())
			{
				earliestODCDate = ODCElements.Min(t => t.StartDate ?? DateTime.MaxValue);
				latestODCDate = ODCElements.Max(t => t.EndDate ?? DateTime.MinValue);
			}

			// Get the earliest date of all elements in the BOE for the Start Date
			DateTime earliestStartDate = DateTime.Compare(earliestTaskDate, earliestMaterialDate) <= 0 ? earliestTaskDate : earliestMaterialDate;
			earliestStartDate = DateTime.Compare(earliestStartDate, earliestTravelDate) <= 0 ? earliestStartDate : earliestTravelDate;
			earliestStartDate = DateTime.Compare(earliestStartDate, earliestODCDate) <= 0 ? earliestStartDate : earliestODCDate;

			// Get the latest date of all elements in the BOE for the End Date
			DateTime latestEndDate = DateTime.Compare(latestTaskDate, latestMaterialDate) >= 0 ? latestTaskDate : latestMaterialDate;
			latestEndDate = DateTime.Compare(latestEndDate, latestTravelDate) >= 0 ? latestEndDate : latestTravelDate;
			latestEndDate = DateTime.Compare(latestEndDate, latestODCDate) >= 0 ? latestEndDate : latestODCDate;

			exportData.EarliestStartDate = earliestStartDate;
			exportData.LatestEndDate = latestEndDate;
		}

		/// <summary>
		/// The ProPricer export date format does not have any "/" in the date nor does it have a day
		/// so this will function will remove those for a date that resembles 062010
		/// </summary>
		/// <param name="inDate">date to format correctly</param>
		/// <returns>formatted date</returns>
		public string FormatExportDate(DateTime inDate)
		{
			return inDate.Month.ToString("D2") + inDate.Year.ToString();
		}

		/// <summary>
		/// Get BOE Custom fields for the resource file
		/// </summary>
		/// <param name="newResourceRow">resource row</param>
		/// <param name="taskField">task fields</param>
		/// <param name="CustomFieldValues">Custom field values</param>
		/// <returns></returns>
		private void GetResourceBOEDisplayCustomFields(Collection<CustomFieldValueContainer> boeCustomFieldContainers, StringBuilder newResourceRow, ProPricerResources taskField,
			ICollection<CustomFieldValueDTO> CustomFieldValues)
		{
			//check if the custom field is within the boe
			Collection<CustomFieldValueContainer> boeCustomFields = boeCustomFieldContainers;

			if (boeCustomFields != null)
			{
				CustomFieldValueDTO boeCustomValue = CustomFieldValues.FirstOrDefault(c => boeCustomFields.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));

				if (boeCustomValue != null)
				{
					if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
					{
						newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(boeCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
					}

					if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
					{
						newResourceRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(boeCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
					}
				}
				else // if the BOE custom field isn't in use, then we still need to add a placeholder for it
				{
					newResourceRow.Append(END_FIELD);
				}
			}
		}

		/// <summary>
		/// Get BOE Display Custom Field information for the task file
		/// </summary>
		/// <param name="NewTaskRow">task row</param>
		/// <param name="taskField"> task field</param>
		/// <param name="CustomFieldValues">custom field value IDs</param>
		/// <returns></returns>
		private void GetTaskBOEDisplayCustomFields(Collection<CustomFieldValueContainer> boeCustomFieldContainers, StringBuilder NewTaskRow, ProPricerTasks taskField,
			ICollection<CustomFieldValueDTO> CustomFieldValues)
		{
			//check if the custom field is within the boe
			Collection<CustomFieldValueContainer> boeCustomFields = boeCustomFieldContainers;

			if (boeCustomFields != null)
			{
				CustomFieldValueDTO boeCustomValue = CustomFieldValues.FirstOrDefault(c => boeCustomFields.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));

				if (boeCustomValue != null)
				{
					if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldDescription)
					{
						NewTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(boeCustomValue.CustomFieldValueDescription)).Append(DOUBLE_QUOTE).Append(END_FIELD);
					}

					if (taskField.Selection == ProPricerCustomFieldSelection.CustomFieldID)
					{
						NewTaskRow.Append(DOUBLE_QUOTE).Append(Utilities.RemoveBrTagsFromText(boeCustomValue.CustomFieldValueName)).Append(DOUBLE_QUOTE).Append(END_FIELD);
					}
				}
				else // if the BOE custom field isn't in use, then we still need to add a placeholder for it
				{
					NewTaskRow.Append(END_FIELD);

				}
			}
		}

		/// <summary>
		/// Returns a collection of unique traveldata based on customfields
		/// </summary>
		/// <param name="trip">traveldata to compare</param>
		/// <param name="uniqueTrips">collection of all traveldata</param>
		/// <returns></returns>
		private List<TravelData> GetUniqueTripsByCustomFields(TravelData trip, List<TravelData> uniqueTrips)
		{
			//Taking in the the traveldata, we are comparing the custom fields of the trip to all the other trips that have the same custom field value selected - we dont care about the order they are in.
			//if any of the trips has the same custom field values as the trip we passed in return a collection of those trips.
			List<TravelData> toReturn = (from t in uniqueTrips
										 where new HashSet<int>(t.TravelTrip.CustomFieldValueContainers.Select(c => c.CustomFieldValueID)).SetEquals(trip.TravelTrip.CustomFieldValueContainers.Select(tc => tc.CustomFieldValueID).ToList())
										 select t).ToList();
			return toReturn;
		}



		/// <summary>
		/// Calulcates the cost total for trips
		/// </summary>
		/// <param name="theWorkspace">the full workspace.</param>
		/// <param name="resourceData"></param>
		/// <param name="startTravelDate">start date of the travel task</param>
		/// <param name="endTravelDate">End date of the travel task</param>
		/// <param name="newResourceRow">resource row that is being added to the export</param>
		/// <param name="uniqueTripsByPerfOrgAndResource">Collection of Trips that we are creating cost for.</param>
		private void ProPricerTripTravelCalulcateCost(FullWorkspace theWorkspace, ICollection<string> resourceData, DateTime startTravelDate, DateTime endTravelDate,
			StringBuilder newResourceRow, List<TravelData> uniqueTripsByPerfOrgAndResource)
		{
			DateTime TravelMonthDate = startTravelDate;
			bool ReachedEndTravelDate = false;
			while (!ReachedEndTravelDate)
			{
				decimal TotalCost = 0.0m;
				TotalCost = (from t in uniqueTripsByPerfOrgAndResource
							 where t.TravelTrip.TripDate.Month == TravelMonthDate.Month && t.TravelTrip.TripDate.Year == TravelMonthDate.Year
							 select this.travelTripCostCalculation.CalculateTravelCost(t.TravelTrip, theWorkspace).CostTotal).Sum();

				newResourceRow.Append((TotalCost == 0 ? string.Empty : TotalCost.ToString())).Append(END_FIELD);

				if (TravelMonthDate.Year == endTravelDate.Year && TravelMonthDate.Month == endTravelDate.Month)
				{
					ReachedEndTravelDate = true;
					resourceData.Add(newResourceRow.ToString());
				}

				TravelMonthDate = TravelMonthDate.AddMonths(1);
			}
		}

		#endregion
	}

	#region Support Classes

	internal class TravelData
	{
		public TravelData()
		{
			this.TravelTrip = new TravelTripType();
			this.ResourceID = 0;
		}

		public TravelTripType TravelTrip { get; set; }

		public int ResourceID { get; set; }
	}



	internal class BoeLevelExportData
	{
		public Collection<BoeTaskElementDTO> LaborElements { get; set; }

		public Collection<BoeTaskElementDTO> IWTAElements { get; set; }

		public Collection<BoeTaskElementDTO> SubcontractorElements { get; set; }

		public Collection<BoeTaskElementDTO> MaterialLaborElements { get; set; }

		public Collection<BoeTaskElementDTO> TravelLaborElements { get; set; }

		public Collection<BoeTaskElementDTO> ODCLaborElements { get; set; }

		public DateTime EarliestStartDate { get; set; }

		public DateTime LatestEndDate { get; set; }

		public Collection<CustomFieldValueContainer> CustomFieldValueContainers { get; set; }

		public ClinDTO Clin { get; set; }

		public WbsDTO Wbs { get; set; }

		public string CleanBoeTitle { get; set; }

		public string CleanBoeDescription { get; set; }

		public bool MultiBoe { get; set; }

		public string CamName { get; set; }

		public string Category { get; set; }

		public string Sow { get; set; }

		public ProjectMapType ProjectMapType { get; set; }

		public ClassOfCost ClassOfCost { get; set; }

		public BoeLevelExportData()
		{
			this.LaborElements = new Collection<BoeTaskElementDTO>();
			this.IWTAElements = new Collection<BoeTaskElementDTO>();
			this.SubcontractorElements = new Collection<BoeTaskElementDTO>();
			this.MaterialLaborElements = new Collection<BoeTaskElementDTO>();
			this.TravelLaborElements = new Collection<BoeTaskElementDTO>();
			this.ODCLaborElements = new Collection<BoeTaskElementDTO>();
		}
	}

	internal class CountersForProPricer
	{
		public int LaborId { get; set; }
		public int IwtaId { get; set; }
		public int SubId { get; set; }
		public int MaterialId { get; set; }
		public int TravelId { get; set; }
		public int OdcId { get; set; }

		public CountersForProPricer()
		{
			this.LaborId = this.IwtaId = this.SubId = this.MaterialId = this.TravelId = this.OdcId = 0;
		}
	}

	internal class WsLevelInputsForExport
	{
		public ProPricerDTO PPInputsToExport { get; set; }

		public int ResourceDecimalPrecision { get; set; }

		public int CostDecimalPrecision { get; set; }

		public IReadOnlyCollection<PerformingOrgDTO> PerfOrgs { get; set; }

		public IReadOnlyCollection<ResourceDTO> Resources { get; set; }

		public ICollection<ResourceDTO> ResourcesInWs { get; set; }

		public ICollection<CustomFieldValueDTO> WsCustomFieldValues { get; set; }

		public ICollection<CustomFieldDTO> WsCustomFields { get; set; }

		public ICollection<FullClin> Clins { get; set; }

		public ICollection<FullWbs> Wbses { get; set; }

		public PpDataReadyForExport PpDataToBeExported { get; set; }

		public CountersForProPricer ItemCounters { get; set; }

		/// <summary>
		/// Gets or sets the task elements.
		/// </summary>
		public ICollection<BoeTaskElementDTO> TaskElements { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is project map workspace.
		/// </summary>
		public bool IsProjectMapWorkspace { get; set; }

		/// <summary>
		/// Is the WS using MOQ Templates?
		/// </summary>
		public bool IsUsingTemplateBOE { get; set; }

		/// <summary>
		/// Moq Types
		/// </summary>
		public ICollection<MoqTypeSelection> MoqTypes { get; set; }

		/// <summary>
		/// 1LMX Custom Field
		/// </summary>
		public CustomFieldDTO OneLmxCustomField { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WsLevelInputsForExport"/> class.
		/// </summary>
		public WsLevelInputsForExport()
		{
			this.PerfOrgs = new ReadOnlyCollection<PerformingOrgDTO>(new List<PerformingOrgDTO>());
			this.Resources = new ReadOnlyCollection<ResourceDTO>(new List<ResourceDTO>());
			this.ResourcesInWs = new ReadOnlyCollection<ResourceDTO>(new List<ResourceDTO>());
			this.WsCustomFields = new List<CustomFieldDTO>();
			this.WsCustomFieldValues = new List<CustomFieldValueDTO>();
			this.Clins = new List<FullClin>();
			this.Wbses = new List<FullWbs>();
			this.PpDataToBeExported = new PpDataReadyForExport();
			this.ItemCounters = new CountersForProPricer();
			this.TaskElements = new Collection<BoeTaskElementDTO>();
			this.MoqTypes = new Collection<MoqTypeSelection>();
		}
	}

	/// <summary>
	/// Houses the ProPricer data that is ready to export.
	/// </summary>
	public class PpDataReadyForExport
	{
		/// <summary>
		/// Gets or sets the task data.
		/// </summary>
		public List<string> TaskData { get; set; }

		/// <summary>
		/// Gets or sets the resource data.
		/// </summary>
		public List<string> ResourceData { get; set; }

		/// <summary>
		/// Gets or sets the offload task data.
		/// </summary>
		public List<string> OffloadTaskData { get; set; }

		/// <summary>
		/// Gets or sets the offload resource data.
		/// </summary>
		public List<string> OffloadResourceData { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PpDataReadyForExport"/> class.
		/// </summary>
		public PpDataReadyForExport()
		{
			this.TaskData = new List<string>();
			this.ResourceData = new List<string>();
			this.OffloadTaskData = new List<string>();
			this.OffloadResourceData = new List<string>();
		}
	}

	#endregion
}
