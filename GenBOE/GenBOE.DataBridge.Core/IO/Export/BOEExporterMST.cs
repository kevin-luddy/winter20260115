// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.IO.Export
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using Aspose.Words;
	using Aspose.Words.Markup;
	using Aspose.Words.Tables;
	using GenBOE.DataBridge.Core.Common;
	using GenBOE.DataBridge.Core.Common.Calculations;
	using GenBOE.DataBridge.Core.DTO;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.DTO.Travel;
	using GenBOE.DataBridge.Core.IO.Export;
	using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using Microsoft.Extensions.Logging;

	[ExcludeFromCodeCoverage]
	public class BOEExporterMST : BOEExporter
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="BOEExporterMST"/> class.
		/// </summary>
		/// <param name="inIPermissionsDTOLoader">The permissions dto loader.</param>
		/// <param name="inIUserDTODataLoader">The user dto data loader.</param>
		/// <param name="inICommonDataMapper">The common data mapper.</param>
		/// <param name="inVariableSelectBOEtoSumCalculation">The variable select boe to sum calculation.</param>
		/// <param name="ADUtils">The ad utils.</param>
		/// <param name="exportConverter">The export converter.</param>
		public BOEExporterMST(
			ILogger<BOEExporter> logger,
			ICommonDataMapper inICommonDataMapper,
			IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
			IActiveDirectoryService ADUtils,
			BOEExportConverter exportConverter)
			: base(logger, 
			inICommonDataMapper,
			inVariableSelectBOEtoSumCalculation,
			ADUtils,
			exportConverter)
		{
		}

		#region Method Overrides
		/// <summary>
		/// Populates the year summary rollup table
		/// </summary>
		/// <param name="element">element to set</param>
		/// <param name="laborRollup">labor rollup data</param>
		/// <param name="Format">Format to use</param>
		/// <param name="NumberFormatter">Format for numbers</param>
		/// <param name="Font">Font to use</param>
		/// <param name="FontSize">Font size to use</param>
		/// <param name="headerFontSize">Font size for the table header</param>
		/// <param name="tag">Content control tag name</param>
		/// <param name="numberAlignment">Not used in MST - required for override - defaults to Right</param>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		protected override void PopulateBoeYearSummaryRollup(StructuredDocumentTag element, Collection<LaborRollupByDate> laborRollup, string Format, 
			NumberFormatInfo NumberFormatter, string Font, double FontSize, string headerFontSize, string tag,
			ParagraphAlignment numberAlignment)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (tag == null)
			{
				throw new ArgumentNullException(nameof(tag));
			}

			if (laborRollup == null)
			{
				throw new ArgumentNullException(nameof(laborRollup));
			}

			if (laborRollup.Select(x => x.Total).Sum() > 0)
			{
				element.RemoveAllChildren();

				//determine if table should be added and set bool
				bool addShading = tag.Contains("NoShading") ? false : true;

				//Create the table
				Table table = CreateRollupTable(element.Document);

				ParagraphProperties leftPP = new ParagraphProperties(new Justification() { Val = ParagraphAlignment.Left }, new SpacingBetweenLines() { After = "0" });
				ParagraphProperties rightPP = new ParagraphProperties(new Justification() { Val = ParagraphAlignment.Right }, new SpacingBetweenLines() { After = "0" });

				RunProperties boldRP = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });

				if (tag.Contains("TopTotal")) //if total goes on top of table, add now
				{
					Row tr = new Row(table.Document);

					TableCellProperties labelTCP = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct },
						new TableCellBorders(new BottomBorder() { Val = BorderValues.Nil }), new TableCellBorders(new LeftBorder() { Val = BorderValues.Nil }), new GridSpan() { Val = 2 });
					string label;
					if (tag.Contains("SummaryHourByDate"))
					{
						label = "Hours:\u00A0";
					}
					else if (tag.Contains("SummaryCostByDate"))
					{
						label = "Dollars:\u00A0";
					}
					else
					{
						label = "Total:\u00A0";
					}
					PopulateTableCell(tr, label, Font, "24", labelTCP, leftPP, boldRP);

					if (NumberFormatter != null)
					{
						NumberFormatter.CurrencySymbol = "$";
					}
					TableCellProperties totalTCP = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });
					string total = laborRollup.Select(x => x.Total).Sum().ToString(Format, NumberFormatter);
					PopulateTableCell(tr, total, Font, "24", totalTCP, leftPP, boldRP, new GridSpan() { Val = 2 });
					if (NumberFormatter != null)
					{
						NumberFormatter.CurrencySymbol = string.Empty;
					}

					table.Append(tr);
				}

				// Create the header row
				List<string> Headers = new List<string>() { "Year", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
				table.Append(CreateRollupHeaderRow(element.Document, Headers, Font, headerFontSize));

				bool evenRow = false;

				foreach (LaborRollupByDate item in laborRollup)
				{
					Row tr2 = new Row(table.Document);

					//so rows won't be split across pages
					TableRowProperties trp = new TableRowProperties();
					CantSplit cantSplit = new CantSplit();
					trp.Append(cantSplit);
					tr2.Append(trp);

					TableCellProperties tcp;
					if (evenRow)
					{
						//shaded cell
						tcp = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct },
							new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear });
					}
					else
					{
						tcp = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });
					}

					PopulateTableCell(tr2, item.Year.ToString(), Font, FontSize, tcp, leftPP);
					PopulateTableCell(tr2, item.January.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);
					PopulateTableCell(tr2, item.February.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);
					PopulateTableCell(tr2, item.March.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);
					PopulateTableCell(tr2, item.April.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);
					PopulateTableCell(tr2, item.May.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);
					PopulateTableCell(tr2, item.June.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);
					PopulateTableCell(tr2, item.July.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);
					PopulateTableCell(tr2, item.August.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);
					PopulateTableCell(tr2, item.September.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);
					PopulateTableCell(tr2, item.October.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);
					PopulateTableCell(tr2, item.November.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);
					PopulateTableCell(tr2, item.December.ToString(Format, NumberFormatter), Font, FontSize, tcp, rightPP);

					//if adding shading, toggle evenRow bool
					if (addShading)
					{
						evenRow = !evenRow;
					}
					table.AppendChild(tr2);
				}

				if (!tag.Contains("TopTotal"))
				{
					Row tr3 = new Row(table.Document);

					TableCellProperties labelTCP = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct },
						new TableCellBorders(new BottomBorder() { Val = BorderValues.Nil }), new TableCellBorders(new LeftBorder() { Val = BorderValues.Nil }));
					PopulateTableCell(tr3, "Total:\u00A0", Font, headerFontSize, labelTCP, leftPP, boldRP, new GridSpan() { Val = 1 });

					if (NumberFormatter != null)
					{
						NumberFormatter.CurrencySymbol = "$";
					}
					TableCellProperties totalTCP = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });
					PopulateTableCell(tr3, laborRollup.Select(x => x.Total).Sum().ToString(Format, NumberFormatter), Font, FontSize, totalTCP, rightPP, boldRP, new GridSpan() { Val = 2 });
					if (NumberFormatter != null)
					{
						NumberFormatter.CurrencySymbol = string.Empty;
					}

					table.AppendChild(tr3);
				}

				element.AppendChild(table);
			}
			else
			{
				element.RemoveIt();
			}
		}

		/// <summary>
		/// Create table for rollup
		/// </summary>
		/// <returns>returns rollup table</returns>
		protected override Table CreateRollupTable(DocumentBase document)
		{
			Table table = new Table(document);
			TableProperties props = new TableProperties(
				new TableBorders(
					new TopBorder
					{
						Val = new EnumValue<BorderValues>(BorderValues.None)
					},
					new BottomBorder
					{
						Val = new EnumValue<BorderValues>(BorderValues.None)
					},
					new LeftBorder
					{
						Val = new EnumValue<BorderValues>(BorderValues.None)
					},
					new RightBorder
					{
						Val = new EnumValue<BorderValues>(BorderValues.None)
					},
					new InsideHorizontalBorder
					{
						Val = new EnumValue<BorderValues>(BorderValues.None)
					},
					new InsideVerticalBorder
					{
						Val = new EnumValue<BorderValues>(BorderValues.None)
					}),
				new TableWidth() { Type = new EnumValue<TableWidthUnitValues>(TableWidthUnitValues.Pct), Width = "5000" },
				new TableLook() { Val = "04A0", FirstRow = true, LastRow = false, FirstColumn = true, LastColumn = false, NoHorizontalBand = false, NoVerticalBand = true },
				new TableCellMarginDefault()
				{
					TableCellLeftMargin = new TableCellLeftMargin() { Type = new EnumValue<TableWidthValues>(TableWidthValues.Dxa), Width = 58 }, //58 20ths of a point = 0.04"
					TableCellRightMargin = new TableCellRightMargin() { Type = new EnumValue<TableWidthValues>(TableWidthValues.Dxa), Width = 58 }
				}
			);
			table.AppendChild(props);
			return table;
		}

		/// <summary>
		/// Creates header row for rollup table
		/// </summary>
		/// <param name="Headers">table headers</param>
		/// <param name="inFont">font to use</param>
		/// <param name="inFontSize">font size to use</param>
		/// <param name="noAfterSpacing">Not used in MST - required for override - defaults to false</param>
		/// <returns>returns table header row</returns>
		protected override Row CreateRollupHeaderRow(DocumentBase document, List<string> Headers, string inFont, string inFontSize, bool noAfterSpacing = false)
		{
			TableRowProperties trp = new TableRowProperties(new TableHeader(), new CantSplit());
			Row tr = new Row(document);
			tr.Append(trp);
			if (Headers != null)
			{
				foreach (string header in Headers)
				{
					TableCellProperties tcp = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = "385" });
					RunProperties rp = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) }, new Underline() { Val = UnderlineValues.Single });
					ParagraphProperties pp;
					if (header == "Year")
					{
						pp = new ParagraphProperties(new Justification() { Val = ParagraphAlignment.Left }, new KeepNext() { Val = true }, new SpacingBetweenLines() { After = "0" });
					}
					else
					{
						pp = new ParagraphProperties(new Justification() { Val = ParagraphAlignment.Right }, new KeepNext() { Val = true }, new SpacingBetweenLines() { After = "0" });
					}
					PopulateTableCell(tr, header, inFont, inFontSize, tcp, pp, rp);
				}
			}
			return tr;
		}

		/// <summary>
		/// Gets the date range and rollup for the BOE
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelView">export model view for the BOE</param>
		/// <returns>
		/// Collection of labor rolled up by date.
		/// </returns>
		protected override Collection<LaborRollupByDate> GetCostRollup(BOEExportInputs exportInputs, BOEExportModelView boeExportModelView)
		{
			Collection<LaborRollupByDate> rollup = null;
			if (exportInputs != null)
			{
				ICollection<OtherDirectCostDTO> ODCElements = exportInputs.Odcs.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();
				ICollection<TravelDTO> TravelElements = exportInputs.Travels.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();
				ICollection<BoeTaskElementDTO> LaborElements = exportInputs.TaskElements.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();
				ICollection<MaterialDTO> MaterialElements = exportInputs.Materials.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();

				if (LaborElements.Any() || ODCElements.Any() || TravelElements.Any())
				{
					DateRange dateRange = GetAllTasksDateRange(LaborElements, ODCElements, TravelElements, MaterialElements, boeExportModelView);
					rollup = this.GetAllTasksCostSummaryRollupByYearData(LaborElements, ODCElements, boeExportModelView.TaskElements.SelectMany(x => x.taskElementLabors).ToCollection(), dateRange);
				}
			}
			return rollup;
		}

		/// <summary>
		/// Gets font size to be used in template
		/// </summary>
		/// <param name="defaultSize">default size to be used</param>
		/// <param name="templateType">template being used</param>
		/// <returns>the font size to be used in the template</returns>
		protected override string GetFontSize(int defaultSize, ExcelReportTemplateType templateType)
		{
			return "18";
		}

		/// <summary>
		/// Gets header font size to be used in template
		/// </summary>
		/// <param name="defaultSize">default size to be used</param>
		/// <param name="templateType">template being used</param>
		/// <returns>the font size to be used in the template</returns>
		protected override string GetHeaderFontSize(int defaultSize, ExcelReportTemplateType templateType)
		{
			return "18";
		}

		/// <summary>
		/// Gets the resource data for the non-travel task elements using the task element labors of the export modelview
		/// </summary>
		/// <param name="boeExportModelView">BOE Export ModelView containing the task element labors</param>
		/// <returns>Collection of Resource Data</returns>
		protected override ICollection<ResourceSummaryRowData> GetTaskElementResourceData(BOEExportModelView boeExportModelView)
		{
			if (boeExportModelView == null)
			{
				throw new ArgumentNullException(nameof(boeExportModelView));
			}

			ICollection<ResourceSummaryRowData> resourceData = new Collection<ResourceSummaryRowData>();

			foreach (BOEExportTaskElementLabor taskElementLabor in boeExportModelView.TaskElements.Where(e => e.ElementType != BOEExportTaskElementType.Travel).SelectMany(t => t.taskElementLabors))
			{
				resourceData.Add(new ResourceSummaryRowData()
				{
					ResourceType = taskElementLabor.ExportFields[FieldName_ResourceElementOfCost],
					ResourceName = taskElementLabor.ExportFields[FieldName_TaskTypeDescription],
					CostTotal = taskElementLabor.Cost.HasValue ? taskElementLabor.Cost.Value : 0m,
					HoursTotal = taskElementLabor.Hours.HasValue ? taskElementLabor.Hours.Value : 0m
				});
			}

			return resourceData;
		}

		#endregion

		#region Private Methods 
		/// <summary>
		/// Gets date range for all tasks in BOE
		/// </summary>
		/// <param name="LaborElements">BOE's Labor elements</param>
		/// <param name="ODCElements">BOE's ODC elements</param>
		/// <param name="TravelElements">BOE's Travel elements</param>
		/// <param name="MaterialElements">BOE's Material elements</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <returns>
		/// Date range from first date to last date between all tasks
		/// </returns>
		private DateRange GetAllTasksDateRange(ICollection<BoeTaskElementDTO> LaborElements, ICollection<OtherDirectCostDTO> ODCElements, ICollection<TravelDTO> TravelElements, ICollection<MaterialDTO> MaterialElements, BOEExportModelView boeExportModelView)
		{
			DateRange odcTravelRange = null;
			DateRange laborRange = null;
			DateRange materialRange = null;

			odcTravelRange = GetODCTravelDateRange(ODCElements, TravelElements);
			laborRange = GetTaskDateRange(LaborElements, boeExportModelView);
			materialRange = GetMaterialTaskDateRange(MaterialElements);

			if (odcTravelRange.StartDate != null && odcTravelRange.EndDate != null || laborRange.StartDate != null && laborRange.EndDate != null || materialRange.StartDate != null && materialRange.EndDate != null)
			{
				List<DateTime> startDates = new List<DateTime> { odcTravelRange.StartDate.HasValue ? odcTravelRange.StartDate.Value : DateTime.MinValue,
					laborRange.StartDate.HasValue ? laborRange.StartDate.Value : DateTime.MinValue,
					materialRange.StartDate.HasValue ? materialRange.StartDate.Value : DateTime.MinValue };
				List<DateTime> endDates = new List<DateTime> { odcTravelRange.EndDate.HasValue ? odcTravelRange.EndDate.Value : DateTime.MaxValue,
					laborRange.EndDate.HasValue ? laborRange.EndDate.Value : DateTime.MaxValue,
					materialRange.EndDate.HasValue ? materialRange.EndDate.Value : DateTime.MaxValue };
				DateTime? Earliest = null;
				DateTime? Latest = null;

				foreach (DateTime date in startDates)
				{
					if (date != DateTime.MinValue && (Earliest == null || date < Earliest))
					{
						Earliest = date;
					}
				}
				foreach (DateTime date in endDates)
				{
					if (date != DateTime.MaxValue && (Latest == null || date > Latest))
					{
						Latest = date;
					}
				}

				if (Earliest != null && Latest != null)
				{
					return new DateRange(Earliest, Latest);
				}
				else
				{
					return new DateRange();
				}
			}
			else
			{
				return new DateRange();
			}
		}

		/// <summary>
		/// Gets cost summary rollup for all tasks in BOE
		/// </summary>
		/// <param name="LaborElements">BOE's Labor elements</param>
		/// <param name="ODCElements">BOE's ODC elements</param>
		/// <param name="labors">BOE's labors</param>
		/// <param name="dateRange">date range of all tasks in BOE</param>
		/// <returns>Cost summary rollup for all tasks</returns>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private Collection<LaborRollupByDate> GetAllTasksCostSummaryRollupByYearData(ICollection<BoeTaskElementDTO> LaborElements, ICollection<OtherDirectCostDTO> ODCElements, Collection<BOEExportTaskElementLabor> labors, DateRange dateRange)
		{
			Collection<LaborRollupByDate> Rollup = GetODCTravelCostSummaryRollupByYearData(ODCElements, labors, dateRange); //get odc and travel rollup
			foreach (LaborRollupByDate rollupYear in Rollup)
			{
				rollupYear.January += (from e in LaborElements
									   from f in e.taskElementLabors
									   from g in f.LaborSpreads
									   where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 1
									   select g.LaborSpreadValue).Sum();

				rollupYear.February += (from e in LaborElements
										from f in e.taskElementLabors
										from g in f.LaborSpreads
										where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 2
										select g.LaborSpreadValue).Sum();

				rollupYear.March += (from e in LaborElements
									 from f in e.taskElementLabors
									 from g in f.LaborSpreads
									 where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 3
									 select g.LaborSpreadValue).Sum();

				rollupYear.April += (from e in LaborElements
									 from f in e.taskElementLabors
									 from g in f.LaborSpreads
									 where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 4
									 select g.LaborSpreadValue).Sum();

				rollupYear.May += (from e in LaborElements
								   from f in e.taskElementLabors
								   from g in f.LaborSpreads
								   where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 5
								   select g.LaborSpreadValue).Sum();

				rollupYear.June += (from e in LaborElements
									from f in e.taskElementLabors
									from g in f.LaborSpreads
									where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 6
									select g.LaborSpreadValue).Sum();

				rollupYear.July += (from e in LaborElements
									from f in e.taskElementLabors
									from g in f.LaborSpreads
									where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 7
									select g.LaborSpreadValue).Sum();

				rollupYear.August += (from e in LaborElements
									  from f in e.taskElementLabors
									  from g in f.LaborSpreads
									  where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 8
									  select g.LaborSpreadValue).Sum();

				rollupYear.September += (from e in LaborElements
										 from f in e.taskElementLabors
										 from g in f.LaborSpreads
										 where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 9
										 select g.LaborSpreadValue).Sum();

				rollupYear.October += (from e in LaborElements
									   from f in e.taskElementLabors
									   from g in f.LaborSpreads
									   where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 10
									   select g.LaborSpreadValue).Sum();

				rollupYear.November += (from e in LaborElements
										from f in e.taskElementLabors
										from g in f.LaborSpreads
										where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 11
										select g.LaborSpreadValue).Sum();

				rollupYear.December += (from e in LaborElements
										from f in e.taskElementLabors
										from g in f.LaborSpreads
										where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == rollupYear.Year && g.LaborSpreadDate.Month == 12
										select g.LaborSpreadValue).Sum();
			}

			return Rollup;
		}

		#endregion
	}
}