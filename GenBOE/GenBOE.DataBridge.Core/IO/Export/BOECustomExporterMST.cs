// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using GenBOE.DataBridge.Core.IO.Export;

namespace GenBOE.DataBridge.Core.IO.Export
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Threading.Tasks;
	using Aspose.Words;
	using Aspose.Words.Markup;
	using Aspose.Words.Tables;
	using GenBOE.DataBridge.Core.Common;
	using GenBOE.DataBridge.Core.Common.Calculations;
	using GenBOE.DataBridge.Core.DTO;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.DTO.Travel;
	using GenBOE.DataBridge.Core.IO.Export;
	using GenBOE.DataBridge.Core.Loaders;
	using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;
	using IES.Common.Core.OfficeUtilities;
	using IES.Common.Core.Utilities;
	using Microsoft.Extensions.Logging;

	[ExcludeFromCodeCoverage]
	public class BOECustomExporterMST : BOECustomExporter
	{
		private MSTZoneTravelResourceDTODataLoader mstZoneTravelResourceDTODataLoader;
		private RMSZoneTravelRatesFeesDataLoader rmsZoneTravelRatesFeesDataLoader;

		public BOECustomExporterMST
			(
				ICommonDataMapper inICommonDataMapper,
				TravelTripCostCalculation inTravelTripCostCalculation,
				IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
				MSTZoneTravelResourceDTODataLoader mstZoneTravelResourceDTODataLoader,
				RMSZoneTravelRatesFeesDataLoader rmsZoneTravelRatesFeesDataLoader,
				ILogger<BOECustomExporterMST> logger
			) : base(
				inICommonDataMapper,
				inTravelTripCostCalculation,
				inVariableSelectBOEtoSumCalculation,
				logger
			)
		{
			// constructor
			DefaultCurrencyFormat = BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS;
			this.mstZoneTravelResourceDTODataLoader = mstZoneTravelResourceDTODataLoader;
			this.rmsZoneTravelRatesFeesDataLoader = rmsZoneTravelRatesFeesDataLoader;
		}

		#region Method Overrides

		#region Resource Summary

		/// <summary>
		/// Populate the Resource Summary by Resource ID Table
		/// </summary>
		/// <param name="tableContainerElement">container element for the table</param>
		/// <param name="boeExportModelView">Model View for the BOE Export with the resources to use</param>
		protected override void PopulateResourceSummaryByResourceIDTable(StructuredDocumentTag tableContainerElement, BOEExportModelView boeExportModelView)
		{
			if (boeExportModelView != null)
			{
				ICollection<ResourceSummaryRowData> resourceData = new Collection<ResourceSummaryRowData>();

				foreach (BOEExportTaskElementLabor taskElementLabor in boeExportModelView.TaskElements.Where(e => e.ElementType != BOEExportTaskElementType.Travel).SelectMany(t => t.taskElementLabors))
				{
					resourceData.Add(new ResourceSummaryRowData()
					{
						ResourceType = taskElementLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost],
						ResourceName = taskElementLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] + " - " + taskElementLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription],
						CostTotal = taskElementLabor.Cost.HasValue ? taskElementLabor.Cost.Value : 0m,
						HoursTotal = taskElementLabor.Hours.HasValue ? taskElementLabor.Hours.Value : 0m
					});
				}

				ICollection<ResourceSummaryRowData> rollupData =
					resourceData
						.GroupBy(x => x.GroupKey)
						.Select(g => new ResourceSummaryRowData
						{
							ResourceType = g.First().ResourceType,
							ResourceName = g.First().ResourceName,
							PeopleTotal = g.Sum(x => x.PeopleTotal),
							HoursTotal = g.Sum(x => x.HoursTotal),
							CostTotal = g.Sum(x => x.CostTotal)
						})
						.OrderBy(x => x.ResourceType).ToList();

				PopulateResourceSummaryTable(tableContainerElement, rollupData);
			}
			else
			{
				this.RemoveElement(tableContainerElement);
			}
		}

		/// <summary>
		/// Populates the resource summary table
		/// </summary>
		/// <param name="tableContainerElement">container element for the table</param>
		/// <param name="rollupData">rollup data to be displayed in the table</param>
		protected override void PopulateResourceSummaryTable(StructuredDocumentTag tableContainerElement, ICollection<ResourceSummaryRowData> rollupData)
		{
			if (rollupData != null && rollupData.Any())
			{
				// locate the table markers
				StructuredDocumentTag dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_DataRow);
				StructuredDocumentTag dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_TotalsRow);

				// initialize the "insertion" row
				Row templateDataRow = dataRowMarkerTag.GetAncestor(Aspose.Words.NodeType.Row) as Row;
				if (templateDataRow != null)
				{
					Row currentInsertionRow = templateDataRow;

					// accumulate totals
					decimal matSubIwtaCostTotal = 0m;
					decimal otherCostTotal = 0m;
					decimal hoursTotal = 0m;
					decimal costTotal = 0m;

					foreach (ResourceSummaryRowData rollupRowData in rollupData)
					{
						//create a new summary data row in the table
						//clone marked template row
						Row tableRow = templateDataRow.Clone(true) as Row;
						Parallel.ForEach(tableRow.GetChildNodes(NodeType.StructuredDocumentTag, true), descendant =>
						{
							StructuredDocumentTag tag = descendant as StructuredDocumentTag;
							tag.Placeholder?.Remove();
						}
						);

						decimal hours = rollupRowData.HoursTotal.HasValue ? rollupRowData.HoursTotal.Value : 0m;
						hoursTotal += hours;

						decimal matSubIwtaCost = 0m;
						decimal otherCost = 0m;
						decimal cost = rollupRowData.CostTotal.HasValue ? rollupRowData.CostTotal.Value : 0m;
						if (rollupRowData.ResourceType == ElementOfCostType.Materials.ToString()
							|| rollupRowData.ResourceType == ElementOfCostType.Sub.ToString()
							|| rollupRowData.ResourceType == ElementOfCostType.IWTA.ToString())
						{
							matSubIwtaCost = cost;
						}
						else if (rollupRowData.ResourceType == ElementOfCostType.ODC.ToString()
							|| rollupRowData.ResourceType == ElementOfCostType.Travel.ToString())
						{
							otherCost = cost;
						}

						matSubIwtaCostTotal += matSubIwtaCost;
						otherCostTotal += otherCost;
						costTotal += cost;

						//populate the row
						WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceName), rollupRowData.ResourceName);
						WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Hours), hours.ToString(DefaultHoursFormat));
						WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_MatSubIWTACost), matSubIwtaCost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter));
						WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_OtherCost), otherCost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter));
						WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Cost), cost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter));

						//add the row to the table
						currentInsertionRow.ParentNode.InsertAfter(tableRow, currentInsertionRow);
						currentInsertionRow = tableRow;
					}

					//total row doesn't need to be cloned
					Row totalsRow = dataTotalsRowMarkerTag.GetAncestor(NodeType.Row) as Row;

					if (totalsRow != null)
					{
						//populate the overall totals
						WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_HoursTotal), hoursTotal.ToString(DefaultHoursFormat));
						WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_MatSubIWTACostTotal), matSubIwtaCostTotal.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter));
						WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_OtherCostTotal), otherCostTotal.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter));
						WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_CostTotal), costTotal.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter));
					}

					//remove template rows
					templateDataRow.RemoveIt();
				}
			}
			else
			{
				this.RemoveElement(tableContainerElement);
			}
		}

		/// <summary>
		/// Prepare data for the Labor Task Resource Table before populating it
		/// </summary>
		/// <param name="tableElement">StructuredDocumentTag for the Labor Task Resource Table</param>
		/// <param name="laborTaskElement">Element for the labor task</param>
		/// <param name="allLaborTaskElements">All task elements for the BOE</param>
		/// <param name="allWorkspaceCustomFields">All custom fields in the current workspace</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <exception cref="ArgumentNullException">laborTaskElement</exception>
		protected override void PrepareLaborTaskResourceTableData(StructuredDocumentTag tableElement, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, BOEExportInputs exportInputs)
		{
			if (laborTaskElement == null)
			{
				throw new ArgumentNullException(nameof(laborTaskElement));
			}

			ICollection<ResourceSummaryRowData> resourceData = laborTaskElement.taskElementLabors.Select(r => new ResourceSummaryRowData
			{
				ResourceType = r.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost],
				ResourceName = r.ExportFields[BOEExporterConstants.FieldName_ResourceName] + " - " + r.ExportFields[BOEExporterConstants.FieldName_ResourceDescription],
				CostTotal = r.Cost.HasValue ? r.Cost.Value : 0m,
				HoursTotal = r.Hours.HasValue ? r.Hours.Value : 0m
			}).ToList();

			ICollection<ResourceSummaryRowData> rollupData =
				resourceData
					.GroupBy(x => x.GroupKey)
					.Select(g => new ResourceSummaryRowData
					{
						ResourceType = g.First().ResourceType,
						ResourceName = g.First().ResourceName,
						CostTotal = g.Sum(x => x.CostTotal),
						HoursTotal = g.Sum(x => x.HoursTotal)
					})
					.OrderBy(x => x.ResourceType).ToList();

			PopulateResourceSummaryTable(tableElement, rollupData);
		}

		#endregion

		#region Spread Rollup Tables

		/// <summary>
		/// Prepare the data for the Labor Task Hours Rollup Table before populating it
		/// </summary>
		/// <param name="templateElement">Template element for the table</param>
		/// <param name="laborTaskElement">Element for the labor task</param>
		/// <param name="allLaborTaskElements">All task elements for the BOE</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <param name="useGfy">Should Government Fiscal Years be used</param>
		/// <param name="byQuarter">If table is by quarter</param>
		/// <exception cref="ArgumentNullException">templateElement</exception>
		protected override void PrepareLaborTaskHoursRollupTableData(StructuredDocumentTag templateElement, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, BOEExportModelView boeExportModelView, bool useGfy, bool byQuarter)
		{
			if (templateElement == null)
			{
				throw new ArgumentNullException(nameof(templateElement));
			}

			BoeTaskElementDTO currentLaborTaskElement = allLaborTaskElements.FirstOrDefault(x => x.Id == laborTaskElement.BOETaskElementID.Value);
			ICollection<BoeTaskElementDTO> currentLaborTaskElementAsICollection = new Collection<BoeTaskElementDTO>();
			currentLaborTaskElementAsICollection.Add(currentLaborTaskElement);

			StructuredDocumentTag currentInsertionElement = templateElement;

			List<LaborRollupByDateNew> laborRollupData = this.GetRollupByYear(currentLaborTaskElementAsICollection, null, RateType.Hours, useGfy);
			IList<RollupSummaryByYearTableRowData> laborHoursSummaryRollupData = laborRollupData.Convert();

			RollupSummaryByYearTableData laborRollupTableData = new RollupSummaryByYearTableData
			{
				SummaryTotalComplete = laborHoursSummaryRollupData.Sum(d => d.YearTotal),
				YearlyData = laborHoursSummaryRollupData
			};

			StructuredDocumentTag HourRollupTableElement = templateElement.Clone(true) as StructuredDocumentTag;
			if (PopulateRollupSummaryByYearTable(HourRollupTableElement, null, laborRollupTableData, DefaultHoursFormat, byQuarter, useGfy))
			{
				currentInsertionElement.ParentNode.InsertAfter(HourRollupTableElement, currentInsertionElement);
				currentInsertionElement = HourRollupTableElement;
			}

			this.RemoveElement(templateElement);
		}

		/// <summary>
		/// Process the Labor Task Cost Spread Rollup Table before populating it
		/// </summary>
		/// <param name="containerElement">Container element for task</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <param name="laborTaskElement">Element for the labor task</param>
		/// <param name="allLaborTaskElements">All task elements for the BOE</param>
		/// <param name="selectedComponents">Components selected for the output</param>
		/// <param name="useGfy">Should Government Fiscal Years be used</param>
		/// <exception cref="ArgumentNullException">selectedComponents</exception>
		protected override void ProcessLaborTaskCostSpreadRollupTable(StructuredDocumentTag containerElement, BOEExportInputs exportInputs, BOEExportModelView boeExportModelView, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, ICollection<BoeCustomReportComponent> selectedComponents, bool useGfy)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Cost Spread Rollup Table

			StructuredDocumentTag costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement,
				useGfy ? BOEExporterConstants.Table_GfyCostSpreadRollup : BOEExporterConstants.Table_CostSpreadRollup);

			bool byQuarter = false;
			if (useGfy && costSpreadRollupTableTemplateElement == null)
			{
				costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_CostSpreadRollupByQuarter);
				byQuarter = true;
			}

			if (costSpreadRollupTableTemplateElement != null)
			{
				if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables))
				{
					BoeTaskElementDTO currentLaborTaskElement = allLaborTaskElements.FirstOrDefault(x => x.Id == laborTaskElement.BOETaskElementID.Value);
					ICollection<BoeTaskElementDTO> currentLaborTaskElementAsICollection = new Collection<BoeTaskElementDTO>();
					currentLaborTaskElementAsICollection.Add(currentLaborTaskElement);

					StructuredDocumentTag currentInsertionElement = costSpreadRollupTableTemplateElement;

					List<LaborRollupByDateNew> laborRollupData = GetTaskCostRollup(currentLaborTaskElementAsICollection, exportInputs, null, useGfy);
					IList<RollupSummaryByYearTableRowData> laborCostSummaryRollupData = laborRollupData.Convert();

					RollupSummaryByYearTableData laborRollupTableData = new RollupSummaryByYearTableData
					{
						SummaryTotalComplete = laborCostSummaryRollupData.Sum(d => d.YearTotal),
						YearlyData = laborCostSummaryRollupData
					};

					StructuredDocumentTag CostRollupTableElement = costSpreadRollupTableTemplateElement.Clone(true) as StructuredDocumentTag;

					if (PopulateRollupSummaryByYearTable(CostRollupTableElement, null, laborRollupTableData, DefaultCurrencyFormat, byQuarter, useGfy))
					{
						currentInsertionElement = currentInsertionElement.ParentNode.InsertAfter(CostRollupTableElement, currentInsertionElement);
					}
				}
				this.RemoveElement(costSpreadRollupTableTemplateElement);
			}
			#endregion
		}

		/// <summary>
		/// Process the ODC Task Direct Cost Rollup Table before populating it
		/// </summary>
		/// <param name="containerElement">Container element for task</param>
		/// <param name="odcTaskElement">Element for the ODC task</param>
		/// <param name="selectedComponents">Components selected for the output</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="useGfy">use government fiscal year?</param>
		/// <exception cref="ArgumentNullException">
		/// selectedComponents
		/// or
		/// exportInputs
		/// </exception>
		protected override void ProcessODCTaskDirectCostRollupTable(StructuredDocumentTag containerElement, BOEExportTaskElement odcTaskElement, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs, bool useGfy)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			#region ODC Cost Rollup Table

			StructuredDocumentTag costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement,
				useGfy ? BOEExporterConstants.Table_GfyDirectCostRollup : BOEExporterConstants.Table_DirectCostRollup);

			bool byQuarter = false;
			if (useGfy && costSpreadRollupTableTemplateElement == null)
			{
				costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyDirectCostRollupByQuarter);
				byQuarter = true;
			}

			if (costSpreadRollupTableTemplateElement != null)
			{
				if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables))
				{
					StructuredDocumentTag currentInsertionElement = costSpreadRollupTableTemplateElement;

					OtherDirectCostDTO currentODCTaskElement = exportInputs.Odcs.First(x => x.Id == odcTaskElement.BOETaskElementID.Value);
					List<LaborRollupByDateNew> ODCRollupData = GetODCCostRollup(currentODCTaskElement, useGfy);
					IList<RollupSummaryByYearTableRowData> ODCCostSummaryRollupData = ODCRollupData.Convert();

					RollupSummaryByYearTableData ODCRollupTableData = new RollupSummaryByYearTableData
					{
						SummaryTotalComplete = ODCCostSummaryRollupData.Sum(d => d.YearTotal),
						YearlyData = ODCCostSummaryRollupData
					};

					StructuredDocumentTag CostRollupTableElement = costSpreadRollupTableTemplateElement.Clone(true) as StructuredDocumentTag;

					if (PopulateRollupSummaryByYearTable(CostRollupTableElement, null, ODCRollupTableData, DefaultCurrencyFormat, byQuarter, useGfy))
					{
						currentInsertionElement = currentInsertionElement.ParentNode.InsertAfter(CostRollupTableElement, currentInsertionElement);
					}
				}
				this.RemoveElement(costSpreadRollupTableTemplateElement);
			}
			#endregion
		}

		#endregion

		#region Resource Container

		/// <summary>
		/// Process the Task Resources for a Labor Task
		/// </summary>
		/// <param name="containerElement">Container element for the task</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="laborTaskElement">Element for the labor task</param>
		/// <param name="allLaborTaskElements">All labor task elements in the workspace</param>
		/// <param name="selectedComponents">Components selected for the output</param>
		/// <param name="multiBoe">Bool to note if BOE is multi wbs/clin</param>
		/// <exception cref="ArgumentNullException">
		/// selectedComponents
		/// or
		/// laborTaskElement
		/// or
		/// exportInputs
		/// </exception>
		protected override void ProcessLaborTaskResources(StructuredDocumentTag containerElement, BOEExportInputs exportInputs, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, ICollection<BoeCustomReportComponent> selectedComponents, bool multiBoe)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			if (laborTaskElement == null)
			{
				throw new ArgumentNullException(nameof(laborTaskElement));
			}

			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			#region Resource Container

			StructuredDocumentTag laborResourceContainerTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.ResourceContainerPrefix + BOEExporterConstants.TaskType_Labor);

			if (laborResourceContainerTemplateElement != null)
			{
				if (selectedComponents.Contains(BoeCustomReportComponent.ResourceInfoAndSpreadTables))
				{
					IOrderedEnumerable<BOEExportTaskElementLabor> orderedResources;
					if (CommonUtilities.IsBRCEnabledForWorkspace(exportInputs.Workspace.Shortname))
					{
						orderedResources = laborTaskElement.taskElementLabors
						.Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_BrcID)
						&& c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceID)
						&& c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrgID))
						.OrderBy(o => o.ExportFields[BOEExporterConstants.FieldName_BrcID])
						.ThenByDescending(r => r.ExportFields[BOEExporterConstants.FieldName_ResourceID])
						.ThenBy(p => p.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]);
					}
					else
					{
						orderedResources = laborTaskElement.taskElementLabors
						.Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceID)
						&& c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrgID))
						.OrderBy(o => o.ExportFields[BOEExporterConstants.FieldName_ResourceID])
						.ThenBy(p => p.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]);
					}

					StructuredDocumentTag currentInsertionPoint = laborResourceContainerTemplateElement;

					foreach (BOEExportTaskElementLabor resourceElement in orderedResources)
					{
						StructuredDocumentTag laborResourceContainerElement = CloneContainerTemplate(laborResourceContainerTemplateElement);

						ProcessResourceHeader(laborResourceContainerElement, resourceElement);
						this.ProcessResourceCustomFields(laborResourceContainerElement, resourceElement, exportInputs.CustomFields, exportInputs);
						ProcessResourceHoursRollupTable(laborResourceContainerElement, resourceElement, allLaborTaskElements, laborTaskElement);
						ProcessResourceCostRollupTable(laborResourceContainerElement, resourceElement, allLaborTaskElements, laborTaskElement);

						currentInsertionPoint.ParentNode.InsertAfter(laborResourceContainerElement, currentInsertionPoint);
						currentInsertionPoint = laborResourceContainerElement;
					}

					this.RemoveElement(laborResourceContainerTemplateElement);
				}
				else
				{
					this.RemoveElement(laborResourceContainerTemplateElement);
				}
			}

			#endregion
		}

		/// <summary>
		/// Process the Task Resources for an ODC Task
		/// </summary>
		/// <param name="containerElement">Container element for the task</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="odcTaskElement">Element for the odc task</param>
		/// <param name="selectedComponents">Components selected for the output</param>
		/// <exception cref="ArgumentNullException">
		/// selectedComponents
		/// or
		/// odcTaskElement
		/// or
		/// exportInputs
		/// </exception>
		protected override void ProcessODCTaskResources(StructuredDocumentTag containerElement, BOEExportInputs exportInputs, BOEExportTaskElement odcTaskElement, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			if (odcTaskElement == null)
			{
				throw new ArgumentNullException(nameof(odcTaskElement));
			}

			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			#region Resource Container

			StructuredDocumentTag odcResourceContainerTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.ResourceContainerPrefix + BOEExporterConstants.TaskType_ODC);

			if (odcResourceContainerTemplateElement != null)
			{
				if (selectedComponents.Contains(BoeCustomReportComponent.ResourceInfoAndSpreadTables))
				{
					ICollection<OtherDirectCostType> ODCTypes = exportInputs.Odcs
						.Where(t => t.Id == odcTaskElement.BOETaskElementID)
						.SelectMany(o => o.ODCTypes)
						.OrderBy(x => x.ResourceID)
						.ThenBy(y => y.PerformingOrgID)
						.ToList();

					HashSet<ResourceDTO> resourcesForOdcs = new HashSet<ResourceDTO>(exportInputs.ResourcesUsedInWsBoes);
					HashSet<PerformingOrgDTO> performingOrgsFromDb = new HashSet<PerformingOrgDTO>(exportInputs.PerformingOrgsUsedInBoes);
					StructuredDocumentTag currentInsertionPoint = odcResourceContainerTemplateElement;

					foreach (OtherDirectCostType odcType in ODCTypes)
					{
						/*
                         * Create a BOEExportTaskElementLabor for each ODC Type. The BOEExportTaskElementLabor 
                         * that exists inodcTaskElement is an aggregate of ODC types, so we can't use them. 
                         * Each individual ODC Type should be printed out for MST.
                        */
						BOEExportTaskElementLabor boeExportLabor = new BOEExportTaskElementLabor(odcTaskElement);
						boeExportLabor.Cost = odcType.SpreadCurve == SpreadCurves.Load ?
															 odcType.Cost.Value * odcType.ODCSpreads.Count :
															 odcType.Cost.Value;

						if (odcType.ResourceID.HasValue)
						{
							ResourceDTO resource = resourcesForOdcs.FirstOrDefault(x => x.Id == odcType.ResourceID.Value);
							if (resource != null)
							{
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_LaborTypes] = resource.LaborType;
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription] = resource.ResourceDesc;
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] = resource.ResourceName;
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceID] = resource.Id.ToString();
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost] = resource.ElementOfCost.ToString();
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceRateType] = resource.RateType.ToString();
							}
						}

						if (odcType.PerformingOrgID.HasValue)
						{
							PerformingOrgDTO perfOrg = performingOrgsFromDb.FirstOrDefault(x => x.Id == odcType.PerformingOrgID.Value);
							if (perfOrg != null)
							{
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID] = perfOrg.Id.ToString();
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrg] = perfOrg.PerformingOrgName;
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrgDescription] = perfOrg.PerformingOrgDesc;
							}
						}

						boeExportLabor.StartDate = odcType.StartDate;
						boeExportLabor.EndDate = odcType.EndDate;


						StructuredDocumentTag odcResourceContainerElement = CloneContainerTemplate(odcResourceContainerTemplateElement);

						ProcessResourceHeader(odcResourceContainerElement, boeExportLabor);
						ProcessODCResourceCostRollupTable(odcResourceContainerElement, odcTaskElement, odcType);

						currentInsertionPoint.ParentNode.InsertAfter(odcResourceContainerElement, currentInsertionPoint);
						currentInsertionPoint = odcResourceContainerElement;
					}
					this.RemoveElement(odcResourceContainerTemplateElement);
				}
				else
				{
					this.RemoveElement(odcResourceContainerTemplateElement);
				}
			}

			#endregion
		}

		#endregion

		/// <summary>
		/// Process Travel Task data for the export
		/// </summary>
		/// <param name="boe">BOE DTO for BOE containing Travel Task</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="BOEExportTaskElements">Collection of BOE Export Task Elements for the Travel Tasks</param>
		/// <param name="logEnabled">bool noting if log is enabled</param>
		/// <exception cref="ArgumentNullException">
		/// exportInputs
		/// or
		/// BOEExportTaskElements
		/// </exception>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		protected override void ProcessTravelTasks(BoeDTO boe, BOEExportInputs exportInputs, Collection<BOEExportTaskElement> BOEExportTaskElements, bool logEnabled)
		{
			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			if (BOEExportTaskElements == null)
			{
				throw new ArgumentNullException(nameof(BOEExportTaskElements));
			}

			if (logEnabled)
			{
				_log.Log(Microsoft.Extensions.Logging.LogLevel.Information, "Exporting - BOECustomExporter - ProcessTravelTasks - travel tasks begin");
			}

			ICollection<TravelDTO> travelTasks = exportInputs.Travels.Where(x => x.BoeID == boe.Id).ToList();
			ICollection<MSTZoneTravelResourceDTO> zoneTravelResources = new Collection<MSTZoneTravelResourceDTO>();
			ICollection<ResourceDTO> nonzoneTravelResources = new Collection<ResourceDTO>();
			ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates = new Collection<WorkspaceRMSEscalationRatesDTO>();
			Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = new Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO>();

			// only grab the travel resources, rates, and fees if there are travel tasks
			if (travelTasks.Any())
			{
				zoneTravelResources = mstZoneTravelResourceDTODataLoader.GetAllResources();
				nonzoneTravelResources = exportInputs.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.Travel).ToCollection();

				//only get if there are nonzone trips, otherwise not needed
				if (travelTasks.Any(x => x.MSTTravelTrips.Any(y => y.ModeID == MSTTravelMode.NonZoneDomestic || y.ModeID == MSTTravelMode.NonZoneInternational)))
				{
					escalationRates = rmsZoneTravelRatesFeesDataLoader.getAllEscalationRatesByWorkspace(exportInputs.Workspace.Id);
					fees = rmsZoneTravelRatesFeesDataLoader.getAllFeesAndCostsByWorkspace(exportInputs.Workspace.Id).ToDictionary(f => f.ModeID);
				}
			}

			foreach (TravelDTO travelElement in travelTasks)
			{
				BOEExportTaskElement boeExportTaskElement = new BOEExportTaskElement();
				boeExportTaskElement.BoeID = boe.Id;
				boeExportTaskElement.BOETaskDesc = travelElement.Description;
				boeExportTaskElement.BOETaskElementID = travelElement.Id;
				boeExportTaskElement.BOETaskID = travelElement.TaskID;
				boeExportTaskElement.TaskTitle = travelElement.TaskTitle;
				boeExportTaskElement.StartDate = travelElement.StartDate.HasValue ? travelElement.StartDate : boe.StartDate;
				boeExportTaskElement.EndDate = travelElement.EndDate.HasValue ? travelElement.EndDate : boe.EndDate;
				boeExportTaskElement.ElementType = BOEExportTaskElementType.Travel;
				boeExportTaskElement.BOETaskElementOrder = travelElement.BOETaskElementOrder;

				// get Task Element Labors
				Collection<BOEExportTaskElementLabor> BoeExportLabors = new Collection<BOEExportTaskElementLabor>();

				foreach (MSTTravelTripType travelTrip in travelElement.MSTTravelTrips)
				{
					BOEExportTaskElementLabor boeExportLabor = new BOEExportTaskElementLabor(boeExportTaskElement);

					decimal people = travelTrip.NumOfPeople ?? 0m;
					decimal days = (travelTrip.NumOfDays ?? 0m) * people;

					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TravelTripID] = travelTrip.Id.ToString();
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_GroupID] = travelTrip.GroupID.ToString();
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Mode] = travelTrip.ModeID.ToDescription();
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypePurpose] = travelTrip.Purpose;
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_People] = CommonUtilities.FormatStringWithPrecision(people, exportInputs.Workspace.DecimalPrecision);
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Days] = CommonUtilities.FormatStringWithPrecision(days, exportInputs.Workspace.DecimalPrecision);
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TripDate] = boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDate] = travelTrip.TripDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR);

					if (boe.IsMultiClinWbs)
					{
						string wbsString = travelTrip.WbsId == null ? "None" : CommonUtilities.FormatNumberTitleString(travelTrip.WbsNumber, travelTrip.WbsTitle, " - ");
						string clinString = travelTrip.ClinId == null ? "None" : CommonUtilities.FormatNumberTitleString(travelTrip.ClinNumber, travelTrip.ClinTitle, " - ");
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_MultiLabel] = "WBS: " + wbsString + ", CLIN: " + clinString;
					}

					if (travelTrip.ModeID == MSTTravelMode.ZoneNoAirfare || travelTrip.ModeID == MSTTravelMode.ZoneAirfare)
					{
						if (travelTrip.ModeID == MSTTravelMode.ZoneAirfare)
						{
							MSTZoneTravelResourceDTO primaryResource = zoneTravelResources.FirstOrDefault(r => r.Resource == travelTrip.ResourceIdForExport && r.OriginID == travelTrip.ZoneOriginID);
							if (primaryResource != null)
							{
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Zone] = primaryResource.Zone.ToString();
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] = primaryResource.Resource;
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription] = primaryResource.Description;
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost] = ElementOfCostType.Travel.ToString();
							}
							MSTZoneTravelResourceDTO secondaryResource = zoneTravelResources.FirstOrDefault(r => r.Resource == travelTrip.SecondaryResourceIdForExport);
							if (secondaryResource != null)
							{
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_SecondaryResourceName] = secondaryResource.Resource;
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_SecondaryResourceDescription] = secondaryResource.Description;
							}
						}
						else
						{
							MSTZoneTravelResourceDTO resource = zoneTravelResources.FirstOrDefault(r => r.ResourceID == travelTrip.ZoneResourceID && r.OriginID == travelTrip.ZoneOriginID);
							if (resource != null)
							{
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Zone] = resource.Zone.ToString();
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] = resource.Resource;
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription] = resource.Description;
								boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost] = ElementOfCostType.Travel.ToString();
							}
						}

						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDeparture] = travelTrip.ZoneOriginName;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDestination] = travelTrip.ZoneDestCity + ", " + travelTrip.ZoneDestinationName;
						boeExportLabor.Cost = 0m;
					}
					else if (travelTrip.ModeID == MSTTravelMode.NonZoneDomestic || travelTrip.ModeID == MSTTravelMode.NonZoneInternational)
					{
						ResourceDTO resource = nonzoneTravelResources.FirstOrDefault(r => r.Id == travelTrip.NonZoneResourceID);
						if (resource != null)
						{
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] = resource.ResourceName;
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription] = resource.ResourceDesc;
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost] = resource.ElementOfCost.ToString();
						}

						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDeparture] = travelTrip.NonZoneFrom;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDestination] = travelTrip.NonZoneTo;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Cars] = CommonUtilities.FormatStringWithPrecision((decimal)travelTrip.NonZoneNumCars, exportInputs.Workspace.DecimalPrecision);
						boeExportLabor.Cost = this.CalculateNonzoneTravelTripCost(exportInputs.Workspace, travelTrip, escalationRates, fees);
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Cost] = boeExportLabor.Cost.Value.ToString(BOEExporterConstants.CURRENCY_FORMAT_DEFAULT, _CurrencyFormatter);
					}

					BoeExportLabors.Add(boeExportLabor);
				}

				boeExportTaskElement.taskElementLabors = BoeExportLabors;

				boeExportTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskCostTotal] = boeExportTaskElement.taskElementLabors.Where(l => l.Cost.HasValue).Sum(l => l.Cost.Value).ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter);

				BOEExportTaskElements.Add(boeExportTaskElement);
			}

			if (logEnabled) { _log.LogInformation("Exporting - BOECustomExporter - ProcessTravelTasks - travel tasks end"); }
		}

		/// <summary>
		/// Process the data for the Travel Task Resource Table
		/// </summary>
		/// <param name="containerElement">Container element for the table</param>
		/// <param name="travelTaskElement">BOE Export Task Element for the Travel Task</param>
		/// <param name="selectedComponents">Selected components for the custom export</param>
		protected override void ProcessTravelTaskResourceTable(StructuredDocumentTag containerElement, BOEExportTaskElement travelTaskElement, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (travelTaskElement == null) { throw new ArgumentNullException(nameof(travelTaskElement)); }
			if (selectedComponents == null) { throw new ArgumentNullException(nameof(selectedComponents)); }

			#region Resource Types Table

			//table elements
			StructuredDocumentTag resourceTypesTableElement_Zone = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceTypes_ZoneTravel);
			StructuredDocumentTag resourceTypesTableElement_Nonzone = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceTypes_NonzoneTravel);

			//table label elements
			StructuredDocumentTag resourceTypesTableLabelElement_Zone = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_ZoneSummaryLabel);
			StructuredDocumentTag resourceTypesTableLabelElement_Nonzone = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_NonzoneSummaryLabel);

			bool isMulti = travelTaskElement.taskElementLabors.Any(x => x.ExportFields.ContainsKey(BOEExporterConstants.FieldName_MultiLabel));

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskResourceTypesSummaryTable) && resourceTypesTableElement_Zone != null)
			{
				Collection<BOEExportTaskElementLabor> travelTaskElementLabors = travelTaskElement.taskElementLabors.Where(x => x.ExportFields.ContainsValue(MSTTravelMode.ZoneNoAirfare.GetDescription()) || x.ExportFields.ContainsValue(MSTTravelMode.ZoneAirfare.GetDescription())).ToCollection();
				if (isMulti)
				{
					populateTravelTripsTableForMultiBOEs(resourceTypesTableElement_Zone, resourceTypesTableLabelElement_Zone, travelTaskElementLabors, true);
				}
				else
				{
					this.RemoveElement(WordUtilities.GetTaggedChildElement(resourceTypesTableElement_Zone, BOEExporterConstants.FieldName_MultiLabel));
					RMSTravelResourceTypesTableData travelTableData = travelTaskElementLabors.ConvertRMSTravel();
					PopulateTravelTripsTable(resourceTypesTableElement_Zone, resourceTypesTableLabelElement_Zone, travelTableData, true);
				}
			}
			else
			{
				this.RemoveElement(resourceTypesTableLabelElement_Zone);
				this.RemoveElement(resourceTypesTableElement_Zone);
			}

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskResourceTypesSummaryTable) && resourceTypesTableElement_Nonzone != null)
			{
				Collection<BOEExportTaskElementLabor> travelTaskElementLabors = travelTaskElement.taskElementLabors.Where(x => x.ExportFields.ContainsValue(MSTTravelMode.NonZoneDomestic.GetDescription()) || x.ExportFields.ContainsValue(MSTTravelMode.NonZoneInternational.GetDescription())).ToCollection();
				if (isMulti)
				{
					populateTravelTripsTableForMultiBOEs(resourceTypesTableElement_Nonzone, resourceTypesTableLabelElement_Nonzone, travelTaskElementLabors, false);
				}
				else
				{
					this.RemoveElement(WordUtilities.GetTaggedChildElement(resourceTypesTableElement_Nonzone, BOEExporterConstants.FieldName_MultiLabel));
					RMSTravelResourceTypesTableData travelTableData = travelTaskElementLabors.ConvertRMSTravel();
					PopulateTravelTripsTable(resourceTypesTableElement_Nonzone, resourceTypesTableLabelElement_Nonzone, travelTableData, false);
				}
			}
			else
			{
				this.RemoveElement(resourceTypesTableLabelElement_Nonzone);
				this.RemoveElement(resourceTypesTableElement_Nonzone);
			}

			#endregion
		}

		/// <summary>
		/// Process the Travel Task Direct Cost Rollup Table before populating it
		/// </summary>
		/// <param name="containerElement">Container element for task rollup table</param>
		/// <param name="travelTaskElement">Travel task element data</param>
		/// <param name="travelResources">Resources used by the travel task</param>
		/// <param name="selectedComponents">Components selected for the output</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="useGfy">use government fiscal year?</param>
		/// <exception cref="ArgumentNullException">
		/// selectedComponents
		/// or
		/// exportInputs
		/// </exception>
		protected override void ProcessTravelTaskDirectCostRollupTable(StructuredDocumentTag containerElement, BOEExportTaskElement travelTaskElement, Collection<ResourceDTO> travelResources, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs, bool useGfy)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			#region Travel Direct Cost Rollup Table

			StructuredDocumentTag travelCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement,
				useGfy ? BOEExporterConstants.Table_GfyDirectCostRollup : BOEExporterConstants.Table_DirectCostRollup);

			bool byQuarter = false;
			if (useGfy && travelCostRollupTableElement == null)
			{
				travelCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyDirectCostRollupByQuarter);
				byQuarter = true;
			}

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables) && travelCostRollupTableElement != null)
			{
				TravelDTO currentTravelTaskElement = exportInputs.Travels.First(x => x.Id == travelTaskElement.BOETaskElementID.Value);
				List<LaborRollupByDateNew> currentTravelTaskRollupData = GetNonzoneTravelCostRollup(currentTravelTaskElement, travelTaskElement, useGfy);
				RollupSummaryByYearTableData travelSummaryRollupData = currentTravelTaskRollupData.ConvertToRollupSummaryByYear();
				PopulateRollupSummaryByYearTable(travelCostRollupTableElement, null, travelSummaryRollupData, DefaultCurrencyFormat, byQuarter, useGfy);
			}
			else
			{
				this.RemoveElement(travelCostRollupTableElement);
			}

			#endregion
		}

		/// <summary>
		/// Remove MOQ related containers only used when Template BOE is set to "No"
		/// For RMS, this is the MOQ Type Container, but only if there are no MOQ RTE Templates
		/// </summary>
		/// <param name="wsHasMoqRteTemplate">Whether Worksace has RTE Templates for MOQ Rationale</param>
		/// <param name="containerElement">The container template</param>
		protected override void RemoveNonTemplateBoeContainers(bool wsHasMoqRteTemplate, StructuredDocumentTag containerElement)
		{
			if (!wsHasMoqRteTemplate)
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQTypeContainer);
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Get the cost rollup for an ODC task
		/// </summary>
		/// <param name="ODCElement">DTO for the ODC task</param>
		/// <param name="useGfy">Use Govt Fiscal Year</param>
		/// <returns>Cost rollup for an ODC task</returns>
		private List<LaborRollupByDateNew> GetODCCostRollup(OtherDirectCostDTO ODCElement, bool useGfy)
		{
			List<LaborRollupByDateNew> RollupList = new List<LaborRollupByDateNew>();
			DateRange ODCDateRange = GetODCTravelDateRange(new Collection<OtherDirectCostDTO> { ODCElement }, null, useGfy);
			if (ODCDateRange.StartDate.HasValue && ODCDateRange.EndDate.HasValue)
			{
				ICollection<int> odcResourceIds = ODCElement.ODCTypes.Where(t => t.ResourceID.HasValue).Select(t => t.ResourceID.Value).Distinct().ToList();

				for (int i = ODCDateRange.StartDate.Value.Year; i <= ODCDateRange.EndDate.Value.Year; i++)
				{
					LaborRollupByDateNew Rollup = new LaborRollupByDateNew();
					Rollup.Resource = "";
					Rollup.Year = i;

					foreach (int resourceId in odcResourceIds)
					{
						ICollection<OtherDirectCostSpread> odcSpreads = ODCElement.ODCTypes.Where(t => t.ResourceID == resourceId).SelectMany(t => t.ODCSpreads).ToList().DeepClone();

						if (useGfy)
						{
							foreach (OtherDirectCostSpread spread in odcSpreads.Where(x => x.ODCSpreadDate.HasValue))
							{
								spread.ODCSpreadDate = this.AdjustDateForGovtFiscalYear(spread.ODCSpreadDate.Value);
							}
						}

						Rollup.January += GetRollupForMonth(odcSpreads, i, 1);
						Rollup.February += GetRollupForMonth(odcSpreads, i, 2);
						Rollup.March += GetRollupForMonth(odcSpreads, i, 3);
						Rollup.April += GetRollupForMonth(odcSpreads, i, 4);
						Rollup.May += GetRollupForMonth(odcSpreads, i, 5);
						Rollup.June += GetRollupForMonth(odcSpreads, i, 6);
						Rollup.July += GetRollupForMonth(odcSpreads, i, 7);
						Rollup.August += GetRollupForMonth(odcSpreads, i, 8);
						Rollup.September += GetRollupForMonth(odcSpreads, i, 9);
						Rollup.October += GetRollupForMonth(odcSpreads, i, 10);
						Rollup.November += GetRollupForMonth(odcSpreads, i, 11);
						Rollup.December += GetRollupForMonth(odcSpreads, i, 12);
					}
					RollupList.Add(Rollup);
				}
			}
			return RollupList;
		}

		/// <summary>
		/// Process and populate the resource header with appropriate data
		/// </summary>
		/// <param name="containerElement">Container element for the resource</param>
		/// <param name="ResourceElement">Resource with data to populate the header</param>
		private void ProcessResourceHeader(StructuredDocumentTag containerElement, BOEExportTaskElementLabor ResourceElement)
		{
			IDictionary<string, string> resourceHeaderDataValueMappings = new Dictionary<string, string>
			{
				{BOEExporterConstants.FieldName_ResourceStartDate, ResourceElement.StartDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR)},
				{BOEExporterConstants.FieldName_ResourceEndDate, ResourceElement.EndDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR)}
			};

			if (ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceDescription))
			{
				resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceDescription, ResourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceDescription]);
			}
			else
			{
				resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceDescription, "NO JOB CODE");
			}

			if (ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrg) && ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrgDescription))
			{
				resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_PerformingOrg, ResourceElement.ExportFields[BOEExporterConstants.FieldName_PerformingOrg] + " - " + ResourceElement.ExportFields[BOEExporterConstants.FieldName_PerformingOrgDescription]);
			}
			else
			{
				resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceDescription, "NO PERF ORG");
			}

			foreach (KeyValuePair<string, string> entry in resourceHeaderDataValueMappings)
			{
				StructuredDocumentTag headerDataElement = WordUtilities.GetTaggedChildElement(containerElement, entry.Key);
				WordUtilities.SetElementText(headerDataElement, entry.Value);
			}
		}

		/// <summary>
		/// Process and populate the Resource Hours Rollup Table
		/// </summary>
		/// <param name="containerElement">Container element for the resource</param>
		/// <param name="ResourceElement">Resource with the data to populate the table</param>
		/// <param name="allLaborTaskElements">All labor task elements in the workspace</param>
		/// <param name="currentLaborTaskElement">Labor task element containing the resource</param>
		private void ProcessResourceHoursRollupTable(StructuredDocumentTag containerElement, BOEExportTaskElementLabor ResourceElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, BOEExportTaskElement currentLaborTaskElement)
		{
			StructuredDocumentTag resourceHoursTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceHoursRollup);

			if (resourceHoursTableElement != null)
			{
				BoeTaskElementDTO currentLaborTaskDto = allLaborTaskElements.FirstOrDefault(x => x.Id == currentLaborTaskElement.BOETaskElementID);
				if (currentLaborTaskDto != null)
				{
					ResourceTypeDto currentResourceTypeDTO = currentLaborTaskDto.taskElementLabors
						.FirstOrDefault(r => r.ResourceID.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceID]
							&& r.Id.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_LaborTypeID]
							&& r.PerformingOrgID.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]
							&& r.StartDate == ResourceElement.StartDate && r.EndDate == ResourceElement.EndDate);

					if (currentResourceTypeDTO != null && currentResourceTypeDTO.SpreadType.Equals(SpreadType.Hours))
					{
						List<LaborRollupByDateNew> rollupData = GetResourceRollupByYear(currentResourceTypeDTO, ResourceElement).ToList();
						IList<RollupSummaryByYearTableRowData> hoursSummaryRollupData = rollupData.Convert();

						RollupSummaryByYearTableData rollupTableData = new RollupSummaryByYearTableData
						{
							SummaryTotalComplete = hoursSummaryRollupData.Sum(s => s.YearTotal),
							YearlyData = hoursSummaryRollupData
						};

						PopulateRollupSummaryByYearTable(resourceHoursTableElement, null, rollupTableData, DefaultHoursFormat, false);
					}
					else
					{
						this.RemoveElement(resourceHoursTableElement);
					}
				}
				else
				{
					this.RemoveElement(resourceHoursTableElement);
				}
			}
		}

		/// <summary>
		/// Process and populate the Resource Cost Rollup table
		/// </summary>
		/// <param name="containerElement">Container elment for the resource</param>
		/// <param name="ResourceElement">Resource with the data to populate the table</param>
		/// <param name="allLaborTaskElements">All labor task elements in the workspace</param>
		/// <param name="currentLaborTaskElement">Labor task element containing the resource</param>
		private void ProcessResourceCostRollupTable(StructuredDocumentTag containerElement, BOEExportTaskElementLabor ResourceElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, BOEExportTaskElement currentLaborTaskElement)
		{
			StructuredDocumentTag resourceCostTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceCostRollup);

			if (resourceCostTableElement != null)
			{
				BoeTaskElementDTO currentLaborTaskDto = allLaborTaskElements.FirstOrDefault(x => x.Id == currentLaborTaskElement.BOETaskElementID);
				if (currentLaborTaskDto != null)
				{
					ResourceTypeDto currentResourceTypeDTO = currentLaborTaskDto.taskElementLabors
							.FirstOrDefault(r => r.ResourceID.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceID]
							&& r.Id.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_LaborTypeID]
								&& r.PerformingOrgID.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]
								&& r.StartDate == ResourceElement.StartDate && r.EndDate == ResourceElement.EndDate);

					if (currentResourceTypeDTO != null && currentResourceTypeDTO.SpreadType.Equals(SpreadType.Cost))
					{
						List<LaborRollupByDateNew> rollupData = GetResourceRollupByYear(currentResourceTypeDTO, ResourceElement).ToList();
						IList<RollupSummaryByYearTableRowData> costSummaryRollupData = rollupData.Convert();

						RollupSummaryByYearTableData rollupTableData = new RollupSummaryByYearTableData
						{
							SummaryTotalComplete = costSummaryRollupData.Sum(s => s.YearTotal),
							YearlyData = costSummaryRollupData
						};

						PopulateRollupSummaryByYearTable(resourceCostTableElement, null, rollupTableData, DefaultCurrencyFormat, false);
					}
					else
					{
						this.RemoveElement(resourceCostTableElement);
					}
				}
				else
				{
					this.RemoveElement(resourceCostTableElement);
				}
			}
		}

		/// <summary>
		/// Calculates the total cost for a Nonzone Travel Trip
		/// </summary>
		/// <param name="trip">Trip to calculate costs for</param>
		/// <param name="escalationRates">Collection of airfare escalation rates for the workspace</param>
		/// <param name="fees">Dictionary of all Travel Agency Fees and Misc/Other Costs for the Workspace</param>
		/// <returns>
		/// Total cost for the Trip
		/// </returns>
		private decimal CalculateNonzoneTravelTripCost(WorkspaceDTO workspace, MSTTravelTripType trip, ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates,
			Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees)
		{
			Dictionary<int, decimal> airfareEscalationRates = escalationRates.ToDictionary(x => x.Year, y => y.AirfareRate);
			Dictionary<int, decimal> perDiemEscalationRates = escalationRates.ToDictionary(x => x.Year, y => y.PerDiemRate);
			Dictionary<int, decimal> miscEscalationRates = escalationRates.ToDictionary(x => x.Year, y => y.MiscRate);

			NonZoneTravelCalculation calculationClass = new NonZoneTravelCalculation(trip.NumOfDays ?? 0, trip.NumOfPeople ?? 0, trip.NonZoneNumCars ?? 0, trip.NonZonePerDiemDaily ?? 0,
				trip.NonZoneCarRentalTrans ?? 0, trip.NonZoneAirfareEstimate ?? 0, airfareEscalationRates, perDiemEscalationRates, miscEscalationRates,
				trip.EstimateDate.Year, trip.TripDate.Year, trip.ModeID == MSTTravelMode.NonZoneDomestic,
				fees[(int)trip.ModeID].TravelAgencyFee, fees[(int)trip.ModeID].MiscOther, workspace.CostDecimalPrecision);

			return calculationClass.TotalTripCost;
		}

		/// <summary>
		/// Populates the Travel Trips Tables
		/// </summary>
		/// <param name="tableContainerElement">Container element for the table</param>
		/// <param name="tableLabelElement">Container element for the table label</param>
		/// <param name="data">data to populate the table with</param>
		/// <param name="isZone">bool noting if trip is zone (true) or nonzone (false)</param>
		private void PopulateTravelTripsTable(StructuredDocumentTag tableContainerElement, StructuredDocumentTag tableLabelElement, RMSTravelResourceTypesTableData data, bool isZone)
		{
			if (data.ResourcesData.Any())
			{
				if (isZone)
				{
					PopulateZoneTravelTripsTable(tableContainerElement, data.ResourcesData);
				}
				else
				{
					PopulateNonzoneTravelTripsTable(tableContainerElement, data.ResourcesData);
				}
			}
			else
			{
				this.RemoveElement(tableLabelElement);
				this.RemoveElement(tableContainerElement);
			}
		}

		/// <summary>
		/// Populates the Zone Travel Trips table
		/// </summary>
		/// <param name="tableContainerElement">Container element for the table</param>
		/// <param name="resourcesData">The resources data.</param>
		private void PopulateZoneTravelTripsTable(StructuredDocumentTag tableContainerElement, ICollection<RMSTravelResourceTypesTableRowData> resourcesData)
		{
			if (resourcesData.Any())
			{
				// different columns for both data and totals rows - logic will check for existence of tags to determine how to populate

				// locate the table markers
				StructuredDocumentTag dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_DataRow);

				// initialize the "insertion" row
				Row templateDataRow = dataRowMarkerTag.GetAncestor(Aspose.Words.NodeType.Row) as Row;
				this.SetCantSplit(templateDataRow);
				Row currentInsertionRow = templateDataRow;

				foreach (RMSTravelResourceTypesTableRowData rowData in resourcesData)
				{
					// create a new data row in the table
					Row tableRow = this.CloneMarkedTemplateRow(templateDataRow);

					// populate the row with the data
					PopulateZoneTravelTripsTableRow(tableRow, rowData, false);

					// add the row to the table
					currentInsertionRow.ParentNode.InsertAfter(tableRow, currentInsertionRow);
					currentInsertionRow = tableRow;

					// Add second row for airfare resource for airfare trips
					if (rowData.IsAirfareTrip)
					{
						tableRow = this.CloneMarkedTemplateRow(templateDataRow);
						PopulateZoneTravelTripsTableRow(tableRow, rowData, true);
						currentInsertionRow.ParentNode.InsertAfter(tableRow, currentInsertionRow);
						currentInsertionRow = tableRow;
					}
				}

				// remove template rows
				templateDataRow.Remove();
			}
			else
			{
				this.RemoveElement(tableContainerElement);
			}
		}

		/// <summary>
		/// Populate a row of the Zone Travel Trips Table
		/// </summary>
		/// <param name="tableRow">Row to be populated</param>
		/// <param name="rowData">Data to populate the row with</param>
		/// <param name="isAirfareResource">Bool noting if resource is Airfare (true) or Per Diem (false)</param>
		private void PopulateZoneTravelTripsTableRow(Row tableRow, RMSTravelResourceTypesTableRowData rowData, bool isAirfareResource)
		{
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_GroupID), rowData.GroupID);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceName), isAirfareResource ? rowData.SecondaryResourceName : rowData.ResourceName);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Departure), rowData.Departure);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Destination), rowData.Destination);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Zone), rowData.Zone);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Purpose), rowData.Purpose);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_TripDate), rowData.TripDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR));
			// Empty for per diem resource in airfare trip
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_People), rowData.People.ToString());
			// Empty for airfare resource in airfare trip
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Days), isAirfareResource ? string.Empty : rowData.Days.ToString());
		}

		/// <summary>
		/// Populates the Nonzone Travel Trips table
		/// </summary>
		/// <param name="tableContainerElement">Container element for the table</param>
		/// <param name="resourcesData">The resources data.</param>
		private void PopulateNonzoneTravelTripsTable(StructuredDocumentTag tableContainerElement, ICollection<RMSTravelResourceTypesTableRowData> resourcesData)
		{
			if (resourcesData.Any())
			{
				// different columns for both data and totals rows - logic will check for existence of tags to determine how to populate

				// locate the table markers
				StructuredDocumentTag dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_DataRow);

				// initialize the "insertion" row
				Row templateDataRow = dataRowMarkerTag.GetAncestor(Aspose.Words.NodeType.Row) as Row;
				this.SetCantSplit(templateDataRow);
				Row currentInsertionRow = templateDataRow;

				foreach (RMSTravelResourceTypesTableRowData rowData in resourcesData)
				{
					// create a new data row in the table
					Row tableRow = this.CloneMarkedTemplateRow(templateDataRow);

					// populate the row with the data
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_GroupID), rowData.GroupID);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceName), rowData.ResourceName);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Departure), rowData.Departure);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Destination), rowData.Destination);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Purpose), rowData.Purpose);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_TripDate), rowData.TripDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR));
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_People), rowData.People.ToString());
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Days), rowData.Days.ToString());
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Cars), rowData.Cars.ToString());

					// add the row to the table
					currentInsertionRow.ParentNode.InsertAfter(tableRow, currentInsertionRow);
					currentInsertionRow = tableRow;
				}

				// remove template rows
				templateDataRow.Remove();
			}
			else
			{
				this.RemoveElement(tableContainerElement);
			}
		}

		/// <summary>
		/// Populates Travel Trips Tables for each WBS/CLIN combination in the Task
		/// </summary>
		/// <param name="tableElement">Sdt Element for the table (either zone or nonzone summary table)</param>
		/// <param name="tableLabelElement">Container element for the table label</param>
		/// <param name="travelTaskElementLabors">BOE Export Task Element for the Travel Task Labors (trips), filtered for zone or nonzone trips</param>
		/// <param name="isZone">bool noting is trip is zone (true) or nonzone (false)</param>
		private void populateTravelTripsTableForMultiBOEs(StructuredDocumentTag tableElement, StructuredDocumentTag tableLabelElement, Collection<BOEExportTaskElementLabor> travelTaskElementLabors, bool isZone)
		{
			StructuredDocumentTag currentTable = tableElement;
			IEnumerable<IGrouping<string, BOEExportTaskElementLabor>> multiGroups = travelTaskElementLabors.GroupBy(x => x.ExportFields[BOEExporterConstants.FieldName_MultiLabel]);
			foreach (IGrouping<string, BOEExportTaskElementLabor> group in multiGroups)
			{
				StructuredDocumentTag clonedTable = tableElement.Clone(true) as StructuredDocumentTag;
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(clonedTable, BOEExporterConstants.FieldName_MultiLabel), group.First().ExportFields[BOEExporterConstants.FieldName_MultiLabel]);
				RMSTravelResourceTypesTableData travelTableData = group.ToCollection().ConvertRMSTravel();
				PopulateTravelTripsTable(clonedTable, tableLabelElement, travelTableData, isZone);
				if (travelTableData.ResourcesData.Any())
				{
					currentTable.ParentNode.InsertAfter(clonedTable, currentTable);
					currentTable = clonedTable;
				}
			}
			this.RemoveElement(tableElement);
		}

		/// <summary>
		/// Gets the nonzone travel cost rollup data
		/// </summary>
		/// <param name="TravelElement">Travel dto containing the trips to rollup</param>
		/// <param name="element">BOE Export Task Element for the Travel Task</param>
		/// <param name="useGfy">Use Govt Fiscal Year</param>
		/// <returns>The cost rollup data for the nonzone trips in the task</returns>
		private List<LaborRollupByDateNew> GetNonzoneTravelCostRollup(TravelDTO TravelElement, BOEExportTaskElement element, bool useGfy)
		{
			List<LaborRollupByDateNew> toReturn = new List<LaborRollupByDateNew>();

			DateRange TravelElementsDateRange = GetODCTravelDateRange(null, new Collection<TravelDTO> { TravelElement }, false);
			if (TravelElementsDateRange.StartDate.HasValue && TravelElementsDateRange.EndDate.HasValue)
			{
				ICollection<MSTTravelTripType> TravelTrips = TravelElement.MSTTravelTrips.DeepClone();

				if (useGfy)
				{
					foreach (MSTTravelTripType trip in TravelTrips)
					{
						trip.TripDate = this.AdjustDateForGovtFiscalYear(trip.TripDate);
					}
				}

				for (int i = TravelElementsDateRange.StartDate.Value.Year; i <= TravelElementsDateRange.EndDate.Value.Year; i++)
				{
					LaborRollupByDateNew Rollup = new LaborRollupByDateNew();
					Rollup.Year = i;

					Rollup.January = GetRollupForMonth(TravelTrips, element, i, 1);
					Rollup.February = GetRollupForMonth(TravelTrips, element, i, 2);
					Rollup.March = GetRollupForMonth(TravelTrips, element, i, 3);
					Rollup.April = GetRollupForMonth(TravelTrips, element, i, 4);
					Rollup.May = GetRollupForMonth(TravelTrips, element, i, 5);
					Rollup.June = GetRollupForMonth(TravelTrips, element, i, 6);
					Rollup.July = GetRollupForMonth(TravelTrips, element, i, 7);
					Rollup.August = GetRollupForMonth(TravelTrips, element, i, 8);
					Rollup.September = GetRollupForMonth(TravelTrips, element, i, 9);
					Rollup.October = GetRollupForMonth(TravelTrips, element, i, 10);
					Rollup.November = GetRollupForMonth(TravelTrips, element, i, 11);
					Rollup.December = GetRollupForMonth(TravelTrips, element, i, 12);

					toReturn.Add(Rollup);
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Calculates the sum of the nonzone travel costs for the given month
		/// </summary>
		/// <param name="travelTrips">Collection of Trips for the Travel Task</param>
		/// <param name="travelExportElement">BOE Export Task Element for the Travel Task</param>
		/// <param name="year">Year of costs to rollup</param>
		/// <param name="month">Month of costs to roll up</param>
		/// <returns>The sum of the nonzone trip costs for the month</returns>
		private decimal GetRollupForMonth(ICollection<MSTTravelTripType> travelTrips, BOEExportTaskElement travelExportElement, int year, int month)
		{
			decimal sum =
				(from e in travelTrips
				 where e.TripDate.Year == year && e.TripDate.Month == month
				 select travelExportElement.taskElementLabors.First(f => int.Parse(f.ExportFields[BOEExporterConstants.FieldName_TravelTripID]) == e.Id).Cost.Value).Sum();

			return sum;
		}

		#endregion
	}
}