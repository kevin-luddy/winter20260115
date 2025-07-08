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
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;
	using Aspose.Words;
	using Aspose.Words.Markup;
	using Aspose.Words.Settings;
	using Aspose.Words.Tables;
	using GenBOE.DataBridge.Core;
	using GenBOE.DataBridge.Core.Common;
	using GenBOE.DataBridge.Core.Common.Calculations;
	using GenBOE.DataBridge.Core.Common.MOQ;
	using GenBOE.DataBridge.Core.DTO;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.DTO.FullObjects;
	using GenBOE.DataBridge.Core.DTO.Travel;
	using GenBOE.DataBridge.Core.IO.Export;
	using GenBOE.DataBridge.Core.Misc;
	using GenBOE.DataBridge.Core.ModelView;
	using IES.Common.Core;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Models;
	using IES.Common.Core.OfficeUtilities;
	using IES.Common.Core.Utilities;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Used for exporting a BOE to a pre-formatted Work template
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class BOECustomExporter : WordExporter, IBOECustomExporter
	{
		#region Fields

		protected ILogger _log { get; private set; }

		private ICommonDataMapper _ICommonDataMapper;

		private TravelTripCostCalculation _TravelTripCostCalculation;
		private IVariableSelectBOEtoSumCalculation _VariableSelectBOEtoSumCalculation;

		protected NumberFormatInfo _CurrencyFormatter { get; set; }

		#endregion

		#region Properties

		public string DefaultCurrencyFormat { get; protected set; }
		public string DefaultHoursFormat { get; protected set; }
		public int WorkspaceDecimalPrecision { get; protected set; }

		#endregion Properties

		#region Constructor

		public BOECustomExporter(
			ICommonDataMapper inICommonDataMapper,
			TravelTripCostCalculation inTravelTripCostCalculation,
			IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
			ILogger logger
			)
			: base()
		{
			_ICommonDataMapper = inICommonDataMapper;
			_TravelTripCostCalculation = inTravelTripCostCalculation;
			_CurrencyFormatter = new NumberFormatInfo();
			_CurrencyFormatter.CurrencyNegativePattern = 1;
			_CurrencyFormatter.CurrencySymbol = "$";
			DefaultCurrencyFormat = BOEExporterConstants.CURRENCY_FORMAT_DEFAULT;
			_VariableSelectBOEtoSumCalculation = inVariableSelectBOEtoSumCalculation;
			_log = logger;
		}

		#endregion

		#region Export

		/// <summary>
		/// Export data about the given BOE into a pre-formatted Word template and return the file path of
		/// the populated template.
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
		/// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
		/// <param name="components">List of selected components</param>
		/// <param name="exportFormat">Export file info</param>
		public void ExportBOEToWordFile(FileStream fileStream, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, 
			ICollection<BOESummaryGridModelView> boeSummaryGridModelViews, ICollection<BoeCustomReportComponent> components, 
			WorkspaceExportFormatDTO exportFormat)
		{
			if (exportFormat == null) { throw new ArgumentNullException(nameof(exportFormat)); }
			_ = exportInputs ?? throw new ArgumentNullException(nameof(exportInputs));

			this.ExportBOEToWordStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, components, fileStream, exportFormat);
		}
		
		/// <summary>
		/// Export data about the given BOE into a pre-formatted Word template and return the file path of
		/// the populated template.
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
		/// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
		/// <param name="components">List of selected components</param>
		/// <param name="returnStream">Output stream</param>
		/// <param name="exportFormat">Export file info</param>
		/// <returns>true if successful, exception otherwise</returns>
		public bool ExportBOEToWordStream(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			ICollection<BoeCustomReportComponent> components, Stream returnStream, WorkspaceExportFormatDTO exportFormat)
		{
			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			if (exportFormat == null)
			{
				throw new ArgumentNullException(nameof(exportFormat));
			}

			ChunkCounter counters = new ChunkCounter();

			// If the ModelViews have data
			if (boeExportModelViews != null && boeSummaryGridModelViews != null)
			{
				if (exportFormat.FileData == null)
				{
					// template file is on disk
					this.Export(exportFormat.PhysicalFilePathCache, (document) =>
					{
						this.PopulateDataExportBOE(exportInputs, document, boeExportModelViews, boeSummaryGridModelViews, components, ref counters);
					}, returnStream);
				}
				else
				{
					// template file content was serialized to the DB (i.e. this template was DERIVED from the master)
					this.Export(exportFormat.FileData, (document) =>
					{
						this.PopulateDataExportBOE(exportInputs, document, boeExportModelViews, boeSummaryGridModelViews, components, ref counters);
					}, returnStream);
				}
			}

			return true;
		}

		/// <summary>
		/// Sets the WorkspaceDecimalPrecision and DefaultHoursFormat variables for the export
		/// </summary>
		/// <param name="ws">Workspace containing BOE(s) in export</param>
		public void SetWorkspacePrecisionVariables(WorkspaceDTO ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			this.WorkspaceDecimalPrecision = ws.DecimalPrecision;
			this.DefaultHoursFormat = CommonUtilities.PrecisionFormattingString(this.WorkspaceDecimalPrecision);
		}

		/// <summary>
		/// This function will populate the BOEExportModelViews based on the BOE DTOs.
		/// </summary>
		/// <param name="boes">Full Boes.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns>the BOE Export ModelViews.</returns>
		public ICollection<BOEExportModelView> ConvertBoeDTOsToExportMVs(ICollection<BoeDTO> boes, BOEExportInputs exportInputs)
		{
			if (boes == null)
			{
				throw new ArgumentNullException(nameof(boes));
			}

			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			ICollection<BOEExportModelView> modelViews = new List<BOEExportModelView>();
			bool writelogstatements = ConfigurationUtilities.GetAppSetting<bool>(BOEExporterConstants.CONFIG_SETTING_LOG_VIEW_MODELS, false);

			// The workspace's export format doesn't have the correct template type if this is a user template so always use the exportFormatDTO
			WorkspaceExportFormatDTO exportFormatDTO = exportInputs.WorkspaceExportFormat;

			CustomFieldDTO BOESegregationCustomField = (from c in exportInputs.CustomFields
														where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_BOESegregation, StringComparison.CurrentCultureIgnoreCase) &&
														c.CustomFieldDisplayID == CustomFieldType.BoeDisplay
														select c).FirstOrDefault();

			CustomFieldDTO revCodeCustomField = (from c in exportInputs.CustomFields
												 where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_RevCode, StringComparison.CurrentCultureIgnoreCase)
												 && c.CustomFieldDisplayID == CustomFieldType.TaskDisplay
												 select c).FirstOrDefault();

			// Get Task Segregation for current task
			CustomFieldDTO taskSegregationCustomField = (from c in exportInputs.CustomFields
														 where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_TaskSegregation, StringComparison.CurrentCultureIgnoreCase) &&
														 c.CustomFieldDisplayID == CustomFieldType.TaskDisplay
														 select c).FirstOrDefault();

			foreach (BoeDTO boe in boes)
			{
				BOEExportModelView modelView = ConvertBoeDTOToExportMV(boe, exportInputs, writelogstatements, exportFormatDTO, BOESegregationCustomField, revCodeCustomField, taskSegregationCustomField);
				modelViews.Add(modelView);
			}

			return modelViews;
		}

		/// <summary>
		/// This function will populate the BOEExportModelView based on the BOE DTO
		/// </summary>
		/// <param name="boe">Full Boe</param>
		/// <returns>the BOE Export ModelView</returns>
		private BOEExportModelView ConvertBoeDTOToExportMV(BoeDTO boe, BOEExportInputs exportInputs, bool writelogstatements, WorkspaceExportFormatDTO exportFormatDTO, CustomFieldDTO BOESegregationCustomField,
			CustomFieldDTO revCodeCustomField, CustomFieldDTO taskSegregationCustomField)
		{

			if (writelogstatements)
			{
				_log.LogInformation("Exporting - BOECustomExporter - ConvertBoeDTOToExportMV - begin");
				_log.LogInformation("Exporting - BOECustomExporter - ConvertBoeDTOToExportMV - get DTO's begin");
			}

			// Get all the DTO data we're going to use

			// get the wbs 
			WbsDTO wbsDTO = exportInputs.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
			string wbsNum = wbsDTO != null ? wbsDTO.WbsNumber : BOEExporterConstants.UNASSIGNED_WBS_DISPLAY_TEXT;
			string wbsTitle = wbsDTO != null ? wbsDTO.WbsTitle : BOEExporterConstants.UNASSIGNED_WBS_DISPLAY_TEXT;

			// get the clin
			ClinDTO clinDTO = exportInputs.Clins.FirstOrDefault(x => x.Id == boe.CLINID);
			string clinNum = clinDTO != null ? clinDTO.ClinNumber : BOEExporterConstants.UNASSIGNED_CLIN_DISPLAY_TEXT;

			if (writelogstatements)
			{
				_log.LogInformation("Exporting - BOECustomExporter - ConvertBoeDTOToExportMV - get DTO's end");
			}

			// set up BOEExportModelView
			BOEExportModelView boeExportModelView = new BOEExportModelView
			{
				BoeID = boe.Id,
				ProgramName = exportInputs.Workspace.WorkspaceName,
				SolicitationNumber = exportInputs.Workspace.RFPNumber ?? string.Empty,
				WBSTitle = wbsTitle,
				WBSNumber = wbsNum,
				PaddedWbsName = wbsDTO != null ? wbsDTO.WbsPaddedNumber : string.Empty,
				CLINNumber = clinNum,
				CLINTitle = clinDTO != null ? clinDTO.ClinTitle == null ? string.Empty : clinDTO.ClinTitle : BOEExporterConstants.UNASSIGNED_CLIN_DISPLAY_TEXT,
				CLINStartDate = clinDTO != null && clinDTO.StartDate.HasValue ? clinDTO.StartDate.Value : (DateTime?)null,
				CLINEndDate = clinDTO != null && clinDTO.EndDate.HasValue ? clinDTO.EndDate.Value : (DateTime?)null,
				PaddedClinName = clinDTO != null ? clinDTO.ClinPaddedNumber : string.Empty,
				ProposalSubmittalDate = exportInputs.Workspace.ProposalSubmittalDate,
				ContainsOCI = exportInputs.Workspace.ContainsOCI,
				DataSource = BOEExportConverter.GetRteOverride(boe.Id, null, boe.DataSource, RteTemplateSource.BoeSources, exportInputs.RTETemplatesOverrides),
				IsMaterial = boe.isMaterial,
				IsMultiClinWbs = boe.IsMultiClinWbs,
				StartDate = boe.StartDate,
				EndDate = boe.EndDate,
				BOEDescription = BOEExportConverter.GetRteOverride(boe.Id, null, boe.Description, RteTemplateSource.BoeDescription, exportInputs.RTETemplatesOverrides),
				ExportFormat = exportFormatDTO.ExportFormat,
				SubmittedDate = boe.SubmitForApprovalDate.Year == DateTime.MinValue.Year ? string.Empty : boe.SubmitForApprovalDate.ToShortDateString(),
				BOETitle = boe.Title
			};

			// Get BOE Segregation for current BOE
			if (writelogstatements)
			{
				_log.LogInformation("Exporting - BOECustomExporter - ConvertBoeDTOToExportMV - get segregation info begin");
			}

			if (BOESegregationCustomField != null)
			{
				CustomFieldValueDTO BOESegregation = (from v in exportInputs.CustomFieldValues.Where(x => x.CustomFieldID == BOESegregationCustomField.Id)
													  from c in boe.CustomFieldValueContainers
													  where v.CustomFieldValueID == c.CustomFieldValueID
													  select v).FirstOrDefault();

				if (BOESegregation != null)
				{
					boeExportModelView.ExportFields[BOEExporterConstants.FieldName_BOESegregation] = string.Concat(
						BOESegregation.CustomFieldValueName == null ? string.Empty : BOESegregation.CustomFieldValueName,
						BOEExporterConstants.BLANK_SPACE,
						BOESegregation.CustomFieldValueDescription == null ? string.Empty : BOESegregation.CustomFieldValueDescription);
				}
			}

			if (writelogstatements)
			{
				_log.LogInformation("Exporting - BOECustomExporter - ConvertBoeDTOToExportMV - get segregation info end");
			}

			#region Authors

			boeExportModelView.Authors = this.DetermineBoeExportAuthors(boe, exportInputs.BoeIdsAndLastUserToSubmitThemForApprovalMapping, exportInputs.GetUserDataForBoesForWs);

			#endregion Authors

			#region Approvers

			boeExportModelView.Approvers = this.DetermineBoeExportApprovers(boe.Id, exportInputs.BoeMappingWithApproverResponses, exportInputs.GetUserDataForBoesForWs);

			#endregion Approvers

			Collection<BOEExportTaskElement> BOEExportTaskElements = new Collection<BOEExportTaskElement>();

			#region Process each task element

			this.ProcessLaborTasks(boe, exportInputs, BOEExportTaskElements, writelogstatements, revCodeCustomField, taskSegregationCustomField);
			this.ProcessODCTasks(boe, BOEExportTaskElements, writelogstatements, exportInputs);
			this.ProcessTravelTasks(boe, exportInputs, BOEExportTaskElements, writelogstatements);
			this.ProcessMaterialsTasks(boe, BOEExportTaskElements, writelogstatements, exportInputs);

			#endregion Process each task element

			boeExportModelView.TaskElements = BOEExportTaskElements;

			if (writelogstatements)
			{
				_log.LogInformation("Exporting - BOECustomExporter - ConvertBoeDTOToExportMV - end");
			}

			return boeExportModelView;
		}

		#endregion

		#region Data population logic

		#region Main data population method


		/// <summary>
		/// Populates the data export boe.
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="document">The document.</param>
		/// <param name="boeExportModelViews">The boe export model views.</param>
		/// <param name="ws">Full WS</param>
		/// <param name="boeSummaryGridModelViews">The boe summary grid model views.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <param name="counters">The counters.</param>
		private void PopulateDataExportBOE(BOEExportInputs exportInputs, Document document, ICollection<BOEExportModelView> boeExportModelViews, 
			ICollection<BOESummaryGridModelView> boeSummaryGridModelViews, ICollection<BoeCustomReportComponent> selectedComponents, ref ChunkCounter counters)
		{
			/*
             * SJR:Notes - Wireframes
             * 
             * https://isgs-gen.external.lmco.com/sites/Estimating_Init/doclib14/Wireframes/Output Format Templates.mht#_Toc351386902
             * 
             * 6.  Material BOE is separate from a normal BOE (see the example file).
             *     a.  For material BOE, the ‘Labor Hours Summary’ table shall not be shown.
             *     b.  For material BOE, the ‘BOE Hours Summary’ table shall not be shown.
             * 
             */

			if (FullObjectHelper.ShowEquivalentPersonsOption && exportInputs.Workspace.IsUsingEquivalentPerson)
			{
				WordUtilities.UpdateHoursLabel(document);
			}

			bool pageBreakAfterBoe = true;

			if (selectedComponents == null)
			{
				selectedComponents = HandleComponentsByCompany(Enum.GetValues(typeof(BoeCustomReportComponent)) as BoeCustomReportComponent[]);
			}

			// Check the file for both the portrait and landscape special boe table elements
			StructuredDocumentTag boeContainerTemplate = WordUtilities.GetTaggedElement(document, BOEExporterConstants.Container_BOE);
			StructuredDocumentTag boeSummaryContainerTemplate = WordUtilities.GetTaggedElement(document, BOEExporterConstants.Container_BOE_SUMMARY);

			if (boeContainerTemplate == null)
			{
				//If the template does not contain the normal BOE Container, it must contain the one created specifically
				//to note that there should not be a page break after each BOE
				boeContainerTemplate = WordUtilities.GetTaggedElement(document, BOEExporterConstants.Container_BOENoBreak);
				pageBreakAfterBoe = false;
			}

			//Determine if template needs to print BOE header information for each task
			bool containsBoeHeaderInTask =
						WordUtilities.GetTaggedElement(document, BOEExporterConstants.FieldName_WBSNumberInTask) != null
						|| WordUtilities.GetTaggedElement(document, BOEExporterConstants.FieldName_BOETitleInTask) != null;

			StructuredDocumentTag boeContainer = null;
			Node lastElement = boeContainerTemplate;

			if (boeExportModelViews.Any() || boeSummaryGridModelViews.Any())
			{
				// Populate the workspace-level content in the template
				SetProprietaryLabelsAndPrintDate(document, boeExportModelViews.First());
				this.SetWorkspaceFields(document, exportInputs.Workspace);

				#region Resource data

				Collection<ResourceDTO> iwtaResources = exportInputs.ResourcesForWsResourceListId.Where(i => i.ElementOfCost == ElementOfCostType.IWTA).ToCollection();
				Collection<ResourceDTO> lmLaborResources = exportInputs.ResourcesForWsResourceListId.Where(i => i.ElementOfCost == ElementOfCostType.LMLabor).ToCollection();
				Collection<ResourceDTO> materialResources = exportInputs.ResourcesForWsResourceListId.Where(i => i.ElementOfCost == ElementOfCostType.Materials).ToCollection();
				Collection<ResourceDTO> odcResources = exportInputs.ResourcesForWsResourceListId.Where(i => i.ElementOfCost == ElementOfCostType.ODC).ToCollection();
				Collection<ResourceDTO> subResources = exportInputs.ResourcesForWsResourceListId.Where(i => i.ElementOfCost == ElementOfCostType.Sub).ToCollection();
				Collection<ResourceDTO> travelResources = exportInputs.ResourcesForWsResourceListId.Where(i => i.ElementOfCost == ElementOfCostType.Travel).ToCollection();

				IDictionary<ElementOfCostType, Collection<ResourceDTO>> resourcesByElementOfCost = new Dictionary<ElementOfCostType, Collection<ResourceDTO>>
					{
						{ ElementOfCostType.IWTA, iwtaResources },
						{ ElementOfCostType.LMLabor, lmLaborResources },
						{ ElementOfCostType.Materials, materialResources },
						{ ElementOfCostType.ODC, odcResources },
						{ ElementOfCostType.Sub, subResources },
						{ ElementOfCostType.Travel, travelResources }
					};

				#endregion

				// Reverse iterate over all BOEs because this loop pushes them onto the top of the document (inserts after header)
				foreach (BOEExportModelView boeExportModelView in boeExportModelViews)
				{
					ICollection<BOESummaryGridModelView> boeSummaryGridModelView = boeSummaryGridModelViews.Where(y => y.BOEID == boeExportModelView.BoeID).ToList();

					#region Supporting data
					BoeDTO boe = exportInputs.AllWorkspaceBoes.First(x => x.Id == boeExportModelView.BoeID);
					List<BoeTaskElementDTO> taskElementCollection = exportInputs.TaskElements.Where(x => x.BoeID == boe.Id).ToList();
					#endregion

					#region Clone the main container

					// separate BOEs with page breaks if applicable for this template
					// (insert before this BOE if it is not the first)
					if (pageBreakAfterBoe && boeContainer != null)
					{
						Paragraph para = new Paragraph(document);
						Run run = new Run(document);
						para.AppendChild(run);
						run.Text = ControlChar.PageBreak;
						lastElement = lastElement.ParentNode.InsertAfter(para, lastElement);
					}

					// create container for this BOE
					// (clone the template, insert after the last BOE container or page break)
					boeContainer = boeContainerTemplate.Clone(true) as StructuredDocumentTag;
					lastElement = lastElement.ParentNode.InsertAfter(boeContainer, lastElement);

					#endregion

					#region Process the data (IS&GS Master Template)

					try  // catch and rethrow to give exception some context
					{

						ProcessBOEHeader(document, boeContainer, boeExportModelView, selectedComponents, ref counters);
						ProcessBOECustomFields(boeContainer, boeExportModelView, selectedComponents, exportInputs);
						ProcessTaskSummaryTable(boeContainer, boeExportModelView);
						ProcessResourceSummaryByResourceTypeTable(boeContainer, boeSummaryGridModelView, selectedComponents);
						ProcessResourceSummaryByResourceIDTable(boeContainer, boeExportModelView, selectedComponents);
						ProcessResourceSummaryByElementOfCostTable(boeContainer, boeExportModelView, boeSummaryGridModelView, selectedComponents);
						this.ProcessLaborHoursSummaryByCustomFieldTable(boeContainer, boeExportModelView, exportInputs.CustomFields, taskElementCollection, exportInputs, FullObjectHelper.ShowEquivalentPersonsOption && exportInputs.Workspace.IsUsingEquivalentPerson, exportInputs.SummarizeByCustomField);

						// Called twice, once for Calendar Year table, once for Govt Fiscal Year version of the table since both can be included
						this.ProcessLaborHoursSummaryTable(boeContainer, taskElementCollection, resourcesByElementOfCost, selectedComponents, FullObjectHelper.ShowEquivalentPersonsOption && exportInputs.Workspace.IsUsingEquivalentPerson, false);

						// Called twice, once for Calendar Year table, once for Govt Fiscal Year version of the table since both can be included
						List<LaborRollupByDateNew> laborTasksRollupCostData = ProcessLaborCostSummaryTable(boeContainer, exportInputs, boe, taskElementCollection, resourcesByElementOfCost, selectedComponents, false);
						ProcessLaborCostSummaryTable(boeContainer, exportInputs, boe, taskElementCollection, resourcesByElementOfCost, selectedComponents, true);

						List<LaborRollupByDateNew> nonLaborRollupCostData = ProcessNonLaborCostSummaryTable(boe, boeContainer, boeExportModelView, selectedComponents, exportInputs);
						ProcessLaborAndNonLaborCostSummaryTable(boeContainer, laborTasksRollupCostData, nonLaborRollupCostData, selectedComponents);

						ProcessAllTaskElements(document, boeContainer, taskElementCollection, boe, exportInputs, boeExportModelView, resourcesByElementOfCost, selectedComponents, containsBoeHeaderInTask, ref counters);

						ProcessBOEHoursSummaryTable(boeContainer, boeExportModelView, selectedComponents);
						this.ProcessBOECostSummaryTable(boeContainer, exportInputs.Workspace, travelResources, boeExportModelView, selectedComponents);
						ProcessBOESourcesOfData(document, boeContainer, boeExportModelView, selectedComponents, ref counters);
						ProcessBOESignaturesAndDatePrepared(boeContainer, boeExportModelView, selectedComponents);
					}
					catch (Exception ex)
					{
						// throw new exception with more info added
						throw new GeneralAppException(
							string.Format("Error encountered while exporting BOE-{0} WBS {1} {2} CLIN {3} {4}: {5}",
								boeExportModelView.BoeID,
								boeExportModelView.WBSNumber,
								boeExportModelView.WBSTitle,
								boeExportModelView.CLINNumber,
								boeExportModelView.CLINTitle,
								ex.Message),
							ex);
					}

					#endregion Process the data (IS&GS Master Template)

					// discard portions of the BOE and child objects to free up memory
					boe.Description = null;
					boe.DataSource = null;
					boeExportModelView.BOEDescription = null;
					boeExportModelView.DataSource = null;
					foreach (BOEExportTaskElement te in boeExportModelView.TaskElements)
					{
						te.BOETaskDesc = null;
						te.MOQText = null;
						// Do not null out taskElementLabors, they are needed if the MOQ Equation uses a Sum Of Variable
					}

					foreach (BoeTaskElementDTO te in taskElementCollection)
					{
						te.Description = null;
						te.MOQText = null;
						// Do not null out taskElementLabors, they are needed if the MOQ Equation uses a Sum Of Variable
					}
				}

				// discard the RTE overrides to free up memory
				exportInputs.ClearRteOverrides();

				// remove page break on last BOE
				if (boeContainer != null && boeContainer.LastChild is CompositeNode composite 
					&& string.IsNullOrEmpty(composite.LastChild.GetText()) &&
					composite.LastChild is Paragraph)
				{
					composite.LastChild.Remove();
				}
			}

			if (boeContainerTemplate != null)
			{
				// delete the template
				this.RemoveElement(boeContainerTemplate);
			}

			if (boeSummaryContainerTemplate != null)
			{
				// delete the template
				this.RemoveElement(boeSummaryContainerTemplate);
			}

			#region Final document cleanup

			#region Set "View" to "Print Layout"

			document.ViewOptions.ViewType = ViewType.PageLayout;
			
			#endregion

			// remove content controls
			WordUtilities.RemoveContentControls(document);

			WordUtilities.CleanupDocumentXml(document);

			#endregion
		}

		/// <summary>
		/// Handles the default behavior of the Additional Query Filters component
		/// </summary>
		/// <param name="selectedComponents">Selected components for the export</param>
		protected virtual ICollection<BoeCustomReportComponent> HandleComponentsByCompany(ICollection<BoeCustomReportComponent> selectedComponents)
		{
			// return with no changes - Additional Query Filters included by default for non-custom exports for RMS
			return selectedComponents;
		}

		#endregion

		#region Data population sub-methods

		#region BOE-level tables

		/// <summary>
		/// Process data for the BOE header in the template
		/// </summary>
		/// <param name="document">Template document</param>
		/// <param name="boeContainer">Template container for the BOE</param>
		/// <param name="boeExportModelView">Modelview for the BOE</param>
		/// <param name="selectedComponents">Template components selected for the export</param>
		/// <param name="counters"></param>
		private void ProcessBOEHeader(Document document, StructuredDocumentTag boeContainer, BOEExportModelView boeExportModelView, ICollection<BoeCustomReportComponent> selectedComponents, ref ChunkCounter counters)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			StructuredDocumentTag boeHeaderElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Container_BOEHeader);
			if (boeHeaderElement != null)
			{

				IDictionary<string, string> boeHeaderDataValueMappings = new Dictionary<string, string>
			{
				{ BOEExporterConstants.FieldName_RFPNumber, boeExportModelView.SolicitationNumber },
				{ BOEExporterConstants.FieldName_BOEStartDate, boeExportModelView.StartDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) },
				{ BOEExporterConstants.FieldName_BOEEndDate, boeExportModelView.EndDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) },
				{ BOEExporterConstants.FieldName_BOETitle, boeExportModelView.BOETitle }
			};

				#region Component selection filtering

				if (selectedComponents.Contains(BoeCustomReportComponent.BOEProgramName))
				{
					boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ProgramName, boeExportModelView.ProgramName);
				}
				else
				{
					WordUtilities.RemoveTableRowWithTaggedElement(boeHeaderElement, BOEExporterConstants.FieldName_ProgramName);
				}

				if (selectedComponents.Contains(BoeCustomReportComponent.BOEWBS))
				{
					boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_WBSNumber, boeExportModelView.WBSNumber);
					boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_WBSTitle, boeExportModelView.WBSTitle);
					boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_WBSStartDate, boeExportModelView.StartDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR));
					boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_WBSEndDate, boeExportModelView.EndDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR));
				}
				else
				{
					WordUtilities.RemoveTableRowWithTaggedElement(boeHeaderElement, BOEExporterConstants.FieldName_WBSNumber);
					WordUtilities.RemoveTableRowWithTaggedElement(boeHeaderElement, BOEExporterConstants.FieldName_WBSStartDate);
				}

				if (selectedComponents.Contains(BoeCustomReportComponent.BOECLIN))
				{
					boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_CLINNumber, boeExportModelView.CLINNumber);
					boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_CLINTitle, boeExportModelView.CLINTitle);
					boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_CLINStartDate, boeExportModelView.CLINStartDate.HasValue ? boeExportModelView.CLINStartDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : string.Empty);
					boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_CLINEndDate, boeExportModelView.CLINEndDate.HasValue ? boeExportModelView.CLINEndDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : string.Empty);
				}
				else
				{
					WordUtilities.RemoveTableRowWithTaggedElement(boeHeaderElement, BOEExporterConstants.FieldName_CLINNumber);
					WordUtilities.RemoveTableRowWithTaggedElement(boeHeaderElement, BOEExporterConstants.FieldName_CLINStartDate);
				}

				if (boeExportModelView.Authors != null)
				{
					StringBuilder authorList = new StringBuilder();
					bool firstAuthor = true;
					bool multiline = true;

					if (WordUtilities.GetTaggedChildElement(boeHeaderElement, BOEExporterConstants.FieldName_AuthorOneLine) != null)
					{
						multiline = false;
					}

					foreach (string author in boeExportModelView.Authors)
					{
						//remove "/signed/" from any authors
						string cleanAuthor = author;
						if (author.Contains("/signed/"))
						{
							int index = author.IndexOf("/signed/");
							cleanAuthor = author.Substring(0, index);
						}

						//combine authors into one string with each author on a new line
						if (firstAuthor)
						{
							authorList.Append(cleanAuthor);
							firstAuthor = false;
						}
						else if (multiline)
						{
							authorList.Append("\r\n");
							authorList.Append(cleanAuthor);
						}
						else
						{
							authorList.Append("; ");
							authorList.Append(cleanAuthor);
						}
					}

					if (multiline)
					{
						boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_Author, authorList.ToString());
					}
					else
					{
						boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_AuthorOneLine, authorList.ToString());
					}
				}
				else
				{
					WordUtilities.RemoveTableRowWithTaggedElement(boeHeaderElement, BOEExporterConstants.FieldName_Author);
				}

				// Only show if the BOE is set to show the Description field and it is a selected component.
				if (selectedComponents.Contains(BoeCustomReportComponent.BOEDescription))
				{
					//This is broken into 2 separate conditional statements because there's no need to check the template for the tag if it wasn't a selected component
					if (WordUtilities.GetTaggedChildElement(boeHeaderElement, BOEExporterConstants.FieldName_BOEDescription) != null)
					{
						boeHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_BOEDescription, boeExportModelView.BOEDescription);
					}
				}
				else
				{
					WordUtilities.RemoveTableRowWithTaggedElement(boeHeaderElement, BOEExporterConstants.FieldName_BOEDescription);
				}

				WordUtilities.RemoveTableRowWithTaggedElement(boeHeaderElement, BOEExporterConstants.FieldName_BOESummaryLevel);

				#endregion

				foreach (KeyValuePair<string, string> entry in boeHeaderDataValueMappings)
				{
					StructuredDocumentTag dataElement = WordUtilities.GetTaggedChildElement(boeHeaderElement, entry.Key);

					if (entry.Key == BOEExporterConstants.FieldName_BOEDescription)
					{
						WordUtilities.SetElementTextWithHTML(document, dataElement, entry.Value, ref counters);
					}
					else
					{
						WordUtilities.SetElementText(dataElement, entry.Value);
					}
				}
			}
		}

		/// <summary>
		/// Processes the boe custom fields.
		/// </summary>
		/// <param name="boeContainer">The boe container.</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <exception cref="ArgumentNullException">selectedComponents</exception>
		private void ProcessBOECustomFields(StructuredDocumentTag boeContainer, BOEExportModelView boeExportModelView, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Custom Fields (BOE-level)

			StructuredDocumentTag boeCustomFieldsContainerElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.CustomFields_BOEContainer);

			#region Component selection filtering

			if (boeCustomFieldsContainerElement != null)
			{
				IDictionary<CustomFieldValueDTO, CustomFieldDTO> boeCustomFields = new Dictionary<CustomFieldValueDTO, CustomFieldDTO>();

				// Only show if Custom Fields is a selected component.
				if (selectedComponents.Contains(BoeCustomReportComponent.BOECustomFields))
				{
					boeCustomFields = exportInputs.AssignedBoeIdsAndCustomFieldValuesMapping.Where(x => x.Key == boeExportModelView.BoeID).Select(x => x.Value).FirstOrDefault();

					IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> cfValues = new Dictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>>();
					cfValues.Add(0, boeCustomFields);

					PopulateCustomFields(boeCustomFieldsContainerElement, cfValues);
				}
				else
				{
					this.RemoveElement(boeCustomFieldsContainerElement);
				}
			}

			#endregion

			#endregion
		}

		/// <summary>
		/// Processes the boe hours summary table.
		/// </summary>
		/// <param name="boeContainer">The boe container.</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <exception cref="ArgumentNullException">
		/// selectedComponents or boeExportModelView</exception>
		protected virtual void ProcessBOEHoursSummaryTable(StructuredDocumentTag boeContainer, BOEExportModelView boeExportModelView, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}
			else if (boeExportModelView == null)
			{
				throw new ArgumentNullException(nameof(boeExportModelView));
			}

			#region BOE Hours Summary Table

			StructuredDocumentTag boeHoursSummaryTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_BOEHoursSummary);

			if (boeHoursSummaryTableElement == null)
			{
			}
			else
			{
				this.RemoveElement(boeHoursSummaryTableElement);
			}

			#endregion
		}

		/// <summary>
		/// Processes the boe cost summary table.
		/// </summary>
		/// <param name="boeContainer">The boe container.</param>
		/// <param name="workspaceData">The workspace data.</param>
		/// <param name="travelResources">The travel resources.</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <exception cref="ArgumentNullException">
		/// selectedComponents or boeExportModelView</exception>
		protected virtual void ProcessBOECostSummaryTable(StructuredDocumentTag boeContainer, WorkspaceDTO workspaceData, Collection<ResourceDTO> travelResources, BOEExportModelView boeExportModelView, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}
			else if (boeExportModelView == null)
			{
				throw new ArgumentNullException(nameof(boeExportModelView));
			}

			#region BOE Cost Summary Table

			StructuredDocumentTag boeCostSummaryTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_BOECostSummary);

			if (boeCostSummaryTableElement == null)
			{
			}

			else
			{
				this.RemoveElement(boeCostSummaryTableElement);
			}

			#endregion
		}

		/// <summary>
		/// Processes the boe sources of data.
		/// </summary>
		/// <param name="document">The document.</param>
		/// <param name="boeContainer">The boe container.</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <param name="counters">The counters.</param>
		/// <exception cref="ArgumentNullException">selectedComponents</exception>
		private void ProcessBOESourcesOfData(Document document, StructuredDocumentTag boeContainer, BOEExportModelView boeExportModelView,
			ICollection<BoeCustomReportComponent> selectedComponents, ref ChunkCounter counters)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			// Set the BOE Sources of Data text if the field was selected, the tag exists in the template, and the BOE is set to show the Sources of Data field.
			if (selectedComponents.Contains(BoeCustomReportComponent.BOESourcesofData))
			{
				StructuredDocumentTag sourcesOfDataElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.FieldName_BOESourcesOfData);
				if (sourcesOfDataElement != null)
				{
					WordUtilities.SetElementTextWithHTML(document, sourcesOfDataElement, boeExportModelView.DataSource, ref counters);
				}
			}
			// If Sources of Data was not selected or if the BOE is set to not show it, remove its container from the output.
			else
			{
				this.RemoveElement(WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Container_SourcesOfData));
			}
		}

		/// <summary>
		/// Processes the boe signatures and date prepared.
		/// </summary>
		/// <param name="boeContainer">The boe container.</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <exception cref="ArgumentNullException">selectedComponents</exception>
		private void ProcessBOESignaturesAndDatePrepared(StructuredDocumentTag boeContainer, BOEExportModelView boeExportModelView, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region BOE Signatures and Date Prepared

			StructuredDocumentTag boeSignaturesTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Container_BOESignatures);

			if (boeSignaturesTableElement == null)
			{
			}
			else if (selectedComponents.Contains(BoeCustomReportComponent.BOESignatures))
			{
				string proposalSubmittalDate = boeExportModelView.ProposalSubmittalDate.HasValue ? boeExportModelView.ProposalSubmittalDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_STANDARD) : string.Empty;
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.FieldName_BOEDatePrepared), proposalSubmittalDate);

				Collection<BoeExportApproverInfo> approvers = boeExportModelView.Approvers;
				string approvedBy = string.Join(WordUtilities.NEWLINE_CHAR.ToString(), approvers.Select(a => a.ApprovedBy));
				string approvedByDate = string.Join(WordUtilities.NEWLINE_CHAR.ToString(), approvers.Select(a => a.ApprovedDate));
				string preparedBy = string.Join(WordUtilities.NEWLINE_CHAR.ToString(), boeExportModelView.Authors);
				string preparedByDate = boeExportModelView.SubmittedDate;

				IDictionary<string, string> boeSignaturesTableDataValueMappings = new Dictionary<string, string>
						{
							{ BOEExporterConstants.FieldName_ApprovedBy, approvedBy },
							{ BOEExporterConstants.FieldName_ApprovedByDate, approvedByDate },
							{ BOEExporterConstants.FieldName_PreparedBy, preparedBy },
							{ BOEExporterConstants.FieldName_PreparedByDate, preparedByDate }
						};

				foreach (KeyValuePair<string, string> entry in boeSignaturesTableDataValueMappings)
				{
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(boeSignaturesTableElement, entry.Key), entry.Value);
				}
			}
			else
			{
				this.RemoveElement(boeSignaturesTableElement);
			}

			#endregion
		}

		/// <summary>
		/// Processes the Task Summary Table
		/// </summary>
		/// <param name="boeContainer">The boe container</param>
		/// <param name="boeExportModelView">The boe export modelview</param>
		private void ProcessTaskSummaryTable(StructuredDocumentTag boeContainer, BOEExportModelView boeExportModelView)
		{
			StructuredDocumentTag taskSummaryTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_TaskSummary);

			if (taskSummaryTableElement != null)
			{
				// Get row markers and template rows
				StructuredDocumentTag dataRowMarkerTag = WordUtilities.GetTaggedChildElement(taskSummaryTableElement, BOEExporterConstants.Marker_DataRow);
				Row templateDataRow = dataRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(templateDataRow);

				StructuredDocumentTag subtotalRowMarkerTag = WordUtilities.GetTaggedChildElement(taskSummaryTableElement, BOEExporterConstants.Marker_SubTotalsRow);
				Row templateSubtotalRow = subtotalRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(templateSubtotalRow);

				StructuredDocumentTag totalRowMarkerTag = WordUtilities.GetTaggedChildElement(taskSummaryTableElement, BOEExporterConstants.Marker_TotalsRow);
				Row templateTotalRow = totalRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(templateTotalRow);

				// initialize insertion row
				Row currentInsertionRow = templateDataRow;

				if (boeExportModelView.IsMultiClinWbs)
				{
					PopulateTaskSummaryTable_MultiWBS(boeExportModelView, templateDataRow, templateSubtotalRow, templateTotalRow, currentInsertionRow);
				}
				else
				{
					PopulateTaskSummaryTable(boeExportModelView, templateDataRow, templateSubtotalRow, templateTotalRow, currentInsertionRow);
				}
			}
		}

		/// <summary>
		/// Populate the Task Summary Table for non-Multi-Clin/WBS BOEs
		/// </summary>
		/// <param name="boeExportModelView">BOE Export Modelview</param>
		/// <param name="templateDataRow">template for the data row</param>
		/// <param name="templateTotalRow">template for the total row</param>
		/// <param name="currentInsertionRow">the current insertion row</param>
		private void PopulateTaskSummaryTable(BOEExportModelView boeExportModelView, Row templateDataRow, Row templateSubtotalRow, Row templateTotalRow, Row currentInsertionRow)
		{
			// There will only be one WBS, so no need for a subtotal
			this.RemoveElement(templateSubtotalRow);

			bool firstRow = true;
			decimal grandTotal = 0m;

			foreach (BOEExportTaskElement task in boeExportModelView.TaskElements.OrderBy(x => x.BOETaskElementOrder))
			{
				// Create a new row
				Row tableRow = this.CloneMarkedTemplateRow(templateDataRow);

				// Populate the row
				StructuredDocumentTag wbsNumberElement = WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_WBSNumber);
				StructuredDocumentTag wbsTitleElement = WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_WBSTitle);

				// Only print the WBS Number and Title on the first row
				if (firstRow)
				{
					WordUtilities.SetElementText(wbsNumberElement, string.IsNullOrEmpty(boeExportModelView.WBSNumber) ? "No WBS" : boeExportModelView.WBSNumber);
					WordUtilities.SetElementText(wbsTitleElement, string.IsNullOrEmpty(boeExportModelView.WBSTitle) ? "No WBS" : boeExportModelView.WBSTitle);
					firstRow = false;
				}
				else
				{
					this.RemoveElement(wbsNumberElement);
					this.RemoveElement(wbsTitleElement);
				}

				StructuredDocumentTag taskIdElement = WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_TaskElementID);
				if (!string.IsNullOrEmpty(task.BOETaskID))
				{
					WordUtilities.SetElementText(taskIdElement, "Task " + task.BOETaskID + ":");
				}
				else
				{
					this.RemoveElement(taskIdElement);
				}

				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_TaskElementTitle), task.TaskTitle);

				decimal hours = task.taskElementLabors.Sum(x => x.Hours ?? 0);
				grandTotal += hours;
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Hours), hours.ToString(DefaultHoursFormat));

				// Add the row to the table
				currentInsertionRow.ParentNode.InsertAfter(tableRow, currentInsertionRow);
				currentInsertionRow = tableRow;
			}

			// populate the total row
			Row totalRow = this.CloneMarkedTemplateRow(templateTotalRow);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalRow, BOEExporterConstants.FieldName_Total), grandTotal.ToString(DefaultHoursFormat));

			// add it to the table
			currentInsertionRow.ParentNode.InsertAfter(totalRow, currentInsertionRow);

			// remove template rows
			this.RemoveElement(templateDataRow);
			this.RemoveElement(templateTotalRow);
		}

		/// <summary>
		/// Populate the Task Summary Table for Multi-Clin/WBS BOEs
		/// </summary>
		/// <param name="boeExportModelView">BOE Export Modelview</param>
		/// <param name="templateDataRow">template for the data row</param>
		/// <param name="templateSubtotalRow">template for the subtotal row</param>
		/// <param name="templateTotalRow">template for the total row</param>
		/// <param name="currentInsertionRow">the current insertion row</param>
		private void PopulateTaskSummaryTable_MultiWBS(BOEExportModelView boeExportModelView, Row templateDataRow, Row templateSubtotalRow, Row templateTotalRow, Row currentInsertionRow)
		{
			ICollection<string> distinctWbs = boeExportModelView.TaskElements.SelectMany(x => x.taskElementLabors).Select(y => y.ExportFields[BOEExporterConstants.FieldName_WBSNumber]).Distinct().OrderBy(z => z).ToCollection();
			decimal grandTotal = 0m;

			foreach (string wbs in distinctWbs)
			{
				ICollection<IGrouping<string, BOEExportTaskElementLabor>> laborsGroupedByTask = boeExportModelView.TaskElements.SelectMany(x => x.taskElementLabors).Where(y => y.ExportFields[BOEExporterConstants.FieldName_WBSNumber] == wbs).GroupBy(z => z.ExportFields[BOEExporterConstants.FieldName_TaskElementID]).ToCollection();

				bool firstRow = true;
				decimal subTotal = 0m;

				foreach (IGrouping<string, BOEExportTaskElementLabor> laborGroup in laborsGroupedByTask)
				{
					// Create a new row
					Row tableRow = this.CloneMarkedTemplateRow(templateDataRow);

					// Populate the row
					StructuredDocumentTag wbsNumberElement = WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_WBSNumber);
					StructuredDocumentTag wbsTitleElement = WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_WBSTitle);

					// Only print the WBS Number and Title on the first row
					if (firstRow)
					{
						string wbsTitle = laborGroup.First().ExportFields[BOEExporterConstants.FieldName_WBSTitle];
						WordUtilities.SetElementText(wbsNumberElement, string.IsNullOrEmpty(wbs) ? "No WBS" : wbs);
						WordUtilities.SetElementText(wbsTitleElement, string.IsNullOrEmpty(wbsTitle) ? "No WBS" : wbsTitle);
						firstRow = false;
					}
					else
					{
						this.RemoveElement(wbsNumberElement);
						this.RemoveElement(wbsTitleElement);
					}

					BOEExportTaskElement task = boeExportModelView.TaskElements.FirstOrDefault(x => x.BOETaskElementID.ToString() == laborGroup.Key);
					string taskId = task != null ? task.BOETaskID : string.Empty;
					string taskTitle = task != null ? task.TaskTitle : string.Empty;
					decimal taskHours = laborGroup.Sum(x => x.Hours ?? 0);

					StructuredDocumentTag taskIdElement = WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_TaskElementID);
					if (!string.IsNullOrEmpty(taskId))
					{
						WordUtilities.SetElementText(taskIdElement, "Task " + taskId + ":");
					}
					else
					{
						this.RemoveElement(taskIdElement);
					}

					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_TaskElementTitle), taskTitle);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Hours), taskHours.ToString(DefaultHoursFormat));

					subTotal += taskHours;

					// add the row to the table
					currentInsertionRow.ParentNode.InsertAfter(tableRow, currentInsertionRow);
					currentInsertionRow = tableRow;
				}

				// populate the subtotal for the WBS
				Row subtotalRow = this.CloneMarkedTemplateRow(templateSubtotalRow);
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(subtotalRow, BOEExporterConstants.FieldName_WBSNumber), string.IsNullOrEmpty(wbs) ? "No WBS" : "WBS " + wbs);
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(subtotalRow, BOEExporterConstants.FieldName_SubTotal), subTotal.ToString(DefaultHoursFormat));

				grandTotal += subTotal;

				// add the row to the table
				currentInsertionRow.ParentNode.InsertAfter(subtotalRow, currentInsertionRow);
				currentInsertionRow = subtotalRow;
			}

			// populate the total row
			Row totalRow = this.CloneMarkedTemplateRow(templateTotalRow);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalRow, BOEExporterConstants.FieldName_Total), grandTotal.ToString(DefaultHoursFormat));

			// add it to the table
			currentInsertionRow.ParentNode.InsertAfter(totalRow, currentInsertionRow);

			// remove template rows
			this.RemoveElement(templateDataRow);
			this.RemoveElement(templateSubtotalRow);
			this.RemoveElement(templateTotalRow);
		}

		#endregion

		#region Other

		/// <summary>
		/// Processes the labor task custom fields.
		/// </summary>
		/// <param name="containerElement">The container element.</param>
		/// <param name="laborTaskElement">The labor task element.</param>
		/// <param name="allWorkspaceCustomFields">All workspace custom fields.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <exception cref="ArgumentNullException">selectedComponents</exception>
		private void ProcessLaborTaskCustomFields(StructuredDocumentTag containerElement, BOEExportTaskElement laborTaskElement, IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			StructuredDocumentTag laborCustomFieldsContainerElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.CustomFields_LaborContainer);

			if (laborCustomFieldsContainerElement == null) { }
			else if (selectedComponents.Contains(BoeCustomReportComponent.TaskCustomFields) && laborTaskElement.BOETaskElementID.HasValue)
			{
				Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueIdMappings = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
				if (exportInputs.TaskElementsMappingWithCustomFieldsValuesAndContainerIds.Any(x => x.Key == laborTaskElement.BOETaskElementID.Value))
				{ customFieldValueIdMappings.Add(laborTaskElement.BOETaskElementID.Value, exportInputs.TaskElementsMappingWithCustomFieldsValuesAndContainerIds.FirstOrDefault(x => x.Key == laborTaskElement.BOETaskElementID.Value).Value); }

				IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> laborTaskCustomFields = GetTaskElementCustomFields(customFieldValueIdMappings, allWorkspaceCustomFields, exportInputs);
				PopulateCustomFields(laborCustomFieldsContainerElement, laborTaskCustomFields);
			}
			else
			{
				this.RemoveElement(laborCustomFieldsContainerElement);
			}

		}

		/// <summary>
		/// Processes the resource-level custom fields before populating them
		/// </summary>
		/// <param name="containerElement">custom field container element</param>
		/// <param name="ResourceElement">resource element that contains the custom fields</param>
		/// <param name="allWorkspaceCustomFields">all custom fields in the workspace</param>
		/// <param name="exportInputs">The export inputs.</param>
		protected void ProcessResourceCustomFields(StructuredDocumentTag containerElement, BOEExportTaskElementLabor ResourceElement, IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, BOEExportInputs exportInputs)
		{
			if (ResourceElement == null) { throw new ArgumentNullException(nameof(ResourceElement)); }
			if (exportInputs == null) { throw new ArgumentNullException(nameof(exportInputs)); }

			StructuredDocumentTag resourceCustomFieldsContainerElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.CustomFields_ResourceContainer);

			if (resourceCustomFieldsContainerElement != null)
			{
				if (ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceID))
				{
					int laborTypeID = Convert.ToInt32(ResourceElement.ExportFields[BOEExporterConstants.FieldName_LaborTypeID]);
					Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueIdMappings = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
					if (exportInputs.LaborTypesMappingWithCustomFieldsValuesAndContainerIds.Any(x => x.Key == laborTypeID))
					{
						customFieldValueIdMappings.Add(laborTypeID, exportInputs.LaborTypesMappingWithCustomFieldsValuesAndContainerIds.FirstOrDefault(x => x.Key == laborTypeID).Value);
					}
					IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> laborTaskCustomFields = GetResourceCustomFields(customFieldValueIdMappings, allWorkspaceCustomFields, exportInputs);
					PopulateCustomFields(resourceCustomFieldsContainerElement, laborTaskCustomFields);
				}
				else
				{
					this.RemoveElement(resourceCustomFieldsContainerElement);
				}
			}
		}

		/// <summary>
		/// Sets the text for fields that appear once for the workspace, such as Program Name.
		/// </summary>
		/// <param name="document">Main Document</param>
		/// <param name="workspace">Full Workspace</param>
		private void SetWorkspaceFields(Document document, WorkspaceDTO workspace)
		{
			StructuredDocumentTag dataElement = WordUtilities.GetTaggedElement(document, BOEExporterConstants.FieldName_ProgramNameHeader);

			if (dataElement != null)
			{
				WordUtilities.SetElementText(dataElement, workspace.WorkspaceName);
			}
		}
		#endregion

		#region Main task element population method

		/// <summary>
		/// Gets the boe task elements by task element.
		/// </summary>
		/// <param name="allTaskElementDtos">All task element dtos.</param>
		/// <returns>The boe task elements by task element type.</returns>
		/// <exception cref="ArgumentNullException">allTaskElementDtos</exception>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected IDictionary<TaskElementType, ICollection<BoeTaskElementDTO>> GetBOETaskElementsByTaskElementType(ICollection<BoeTaskElementDTO> allTaskElementDtos)
		{
			if (allTaskElementDtos == null)
			{
				throw new ArgumentNullException(nameof(allTaskElementDtos));
			}

			#region DTOs

			/*
             * BoeTaskElementDTO includes:
             *      custom fields
             *      MOQ equation
             *      total hours, total cost
             *      [BOELaborType] list of resources, spread data:
             *          list of spread values
             *          resourceID
             *          spread curve type
             *          total spread hours
             *          total spread percent
             * 
             */

			// This only returns labor tab tasks (NOT Material, Travel or ODC)

			ICollection<BoeTaskElementDTO> laborTaskElementDtos = allTaskElementDtos.Where(t => t.TaskElementType == TaskElementType.Labor).ToList();

			#endregion

			return new Dictionary<TaskElementType, ICollection<BoeTaskElementDTO>>
			{
				{ TaskElementType.Labor, laborTaskElementDtos }
			};
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected IDictionary<BOEExportTaskElementType, ICollection<BOEExportTaskElement>> GetBOETaskElementsByElementOfCost(BOEExportModelView boeExportModelView)
		{
			if (boeExportModelView == null)
			{
				throw new ArgumentNullException(nameof(boeExportModelView));
			}

			#region Export data

			/*
             * BOEExportTaskElement includes:
             *      export fields
             *      MOQ equation
             *      [BOEExportTaskElementLabor] list of:
             *          total hours
             *          total cost
             *          export fields [display-column values]
             * 
             */

			// This has ALL tasks (Labor, plus Material, Travel, ODC)
			Collection<BOEExportTaskElement> allExportTaskElements = boeExportModelView.TaskElements;

			ICollection<BOEExportTaskElement> laborExportTaskElements = allExportTaskElements.Where(t => t.ElementType == BOEExportTaskElementType.Labor).ToList();
			ICollection<BOEExportTaskElement> travelExportTaskElements = allExportTaskElements.Where(t => t.ElementType == BOEExportTaskElementType.Travel).ToList();
			ICollection<BOEExportTaskElement> odcExportTaskElements = allExportTaskElements.Where(t => t.ElementType == BOEExportTaskElementType.ODC).ToList();
			ICollection<BOEExportTaskElement> materialExportTaskElements = allExportTaskElements.Where(t => t.ElementType == BOEExportTaskElementType.Material).ToList();

			#endregion

			return new Dictionary<BOEExportTaskElementType, ICollection<BOEExportTaskElement>>
			{
				{ BOEExportTaskElementType.Labor, laborExportTaskElements },
				{ BOEExportTaskElementType.Travel, travelExportTaskElements },
				{ BOEExportTaskElementType.ODC, odcExportTaskElements },
				{ BOEExportTaskElementType.Material, materialExportTaskElements }
			};
		}

		/// <summary>
		/// Processes all task elements.
		/// </summary>
		/// <param name="document">The document.</param>
		/// <param name="boeContainer">The boe container.</param>
		/// <param name="taskElementCollection">The task element collection.</param>
		/// <param name="ws">Full WS</param>
		/// <param name="exportBoe">The export boe.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <param name="resourcesByElementOfCost">The resources by element of cost.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <param name="containsBoeHeaderInTask">if set to <c>true</c> [contains boe header in task].</param>
		/// <param name="counters">The counters.</param>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private void ProcessAllTaskElements(Document document, StructuredDocumentTag boeContainer, ICollection<BoeTaskElementDTO> taskElementCollection, BoeDTO exportBoe, BOEExportInputs exportInputs, BOEExportModelView boeExportModelView,
			IDictionary<ElementOfCostType, Collection<ResourceDTO>> resourcesByElementOfCost, ICollection<BoeCustomReportComponent> selectedComponents, bool containsBoeHeaderInTask, ref ChunkCounter counters)
		{
			Collection<ResourceDTO> travelResources = resourcesByElementOfCost[ElementOfCostType.Travel];

			#region Task Elements

			#region Only consider task elements that have hours assigned

			int totalTaskElementsWithHours =
				boeExportModelView.TaskElements.Count;

			#endregion

			#region Locate all task containers (in the template)

			IDictionary<BOEExportTaskElementType, StructuredDocumentTag> taskContainerTemplateElements = new Dictionary<BOEExportTaskElementType, StructuredDocumentTag>();
			foreach (BOEExportTaskElementType taskElementType in Enum.GetValues(typeof(BOEExportTaskElementType)).Cast<BOEExportTaskElementType>())
			{
				string taskContainerTag = BOEExporterConstants.TaskContainerPrefix + taskElementType.GetName();
				StructuredDocumentTag taskContainerTemplateElement = WordUtilities.GetTaggedChildElement(boeContainer, taskContainerTag);
				taskContainerTemplateElements.Add(taskElementType, taskContainerTemplateElement);
			}

			#endregion

			#region Task section container element references

			StructuredDocumentTag laborSectionTitleElement = WordUtilities.GetTaggedChildElement(boeContainer, string.Format("{0}Labor", BOEExporterConstants.SectionTitlePrefix));
			StructuredDocumentTag travelSectionTitleElement = WordUtilities.GetTaggedChildElement(boeContainer, string.Format("{0}Travel", BOEExporterConstants.SectionTitlePrefix));
			StructuredDocumentTag odcSectionTitleElement = WordUtilities.GetTaggedChildElement(boeContainer, string.Format("{0}ODC", BOEExporterConstants.SectionTitlePrefix));
			StructuredDocumentTag materialSectionTitleElement = WordUtilities.GetTaggedChildElement(boeContainer, string.Format("{0}Material", BOEExporterConstants.SectionTitlePrefix));
			StructuredDocumentTag laborTaskContainerTemplateElement = taskContainerTemplateElements[BOEExportTaskElementType.Labor];
			StructuredDocumentTag travelTaskContainerTemplateElement = taskContainerTemplateElements[BOEExportTaskElementType.Travel];
			StructuredDocumentTag odcTaskContainerTemplateElement = taskContainerTemplateElements[BOEExportTaskElementType.ODC];
			StructuredDocumentTag materialTaskContainerTemplateElement = taskContainerTemplateElements[BOEExportTaskElementType.Material];

			#endregion

			////if (nonZeroTaskElements.Any())
			if (totalTaskElementsWithHours > 0)
			{
				#region Retrieve task element data and derive type-specific subsets

				IDictionary<TaskElementType, ICollection<BoeTaskElementDTO>> boeTaskElementsByTaskElementType = GetBOETaskElementsByTaskElementType(taskElementCollection);
				ICollection<BoeTaskElementDTO> allLaborTaskElements = boeTaskElementsByTaskElementType[TaskElementType.Labor].ToList();

				IDictionary<BOEExportTaskElementType, ICollection<BOEExportTaskElement>> boeTaskElementsByElementOfCost = GetBOETaskElementsByElementOfCost(boeExportModelView);
				ICollection<BOEExportTaskElement> allLaborTaskElementsFull = boeTaskElementsByElementOfCost[BOEExportTaskElementType.Labor].ToList();
				ICollection<BOEExportTaskElement> allTravelTaskElementsFull = boeTaskElementsByElementOfCost[BOEExportTaskElementType.Travel];
				ICollection<BOEExportTaskElement> allODCTaskElementsFull = boeTaskElementsByElementOfCost[BOEExportTaskElementType.ODC];
				ICollection<BOEExportTaskElement> allMaterialTaskElementsFull = boeTaskElementsByElementOfCost[BOEExportTaskElementType.Material];

				#endregion

				#region Task Container (Labor)

				#region Section Title (Labor)

				if (!allLaborTaskElements.Any())
				{
					this.RemoveElement(laborSectionTitleElement);
				}

				#endregion

				#region Process container (each labor task)

				//Remove columns not needed in the Resource Types Table for this BOE
				RemoveUnusedFieldsFromResourceTypesTable(exportInputs, boeExportModelView, laborTaskContainerTemplateElement);

				StructuredDocumentTag currentLaborTaskContainerInsertionPoint = laborTaskContainerTemplateElement;

				//order the elements by order id then the creation date.
				allLaborTaskElementsFull = allLaborTaskElementsFull.OrderBy(x => x.BOETaskElementOrder).ThenBy(y => y.BOETaskElementID).ToList();
				foreach (BOEExportTaskElement laborTaskElement in allLaborTaskElementsFull)
				{
					//  create (clone) a new container for this task
					StructuredDocumentTag containerElement = CloneContainerTemplate(laborTaskContainerTemplateElement);

					#region Process the data (IS&GS)
					if (containsBoeHeaderInTask)
					{
						ProcessTaskHeaderWithBoeData(containerElement, selectedComponents, boeExportModelView);
					}
					ProcessLaborTaskHeader(document, containerElement, laborTaskElement, selectedComponents, exportInputs, ref counters);
					this.ProcessLaborTaskCustomFields(containerElement, laborTaskElement, exportInputs.CustomFields, selectedComponents, exportInputs);
					this.ProcessLaborTaskResourceTable(containerElement, laborTaskElement, allLaborTaskElements, exportInputs.CustomFields, selectedComponents, exportInputs);
					ProcessLaborTaskHoursRollupTable(containerElement, laborTaskElement, allLaborTaskElements, selectedComponents, exportInputs, boeExportModelView);
					ProcessLaborTaskCostSpreadRollupTable(containerElement, exportInputs, boeExportModelView, laborTaskElement, allLaborTaskElements, selectedComponents, true);
					ProcessLaborTaskCostSpreadRollupTable(containerElement, exportInputs, boeExportModelView, laborTaskElement, allLaborTaskElements, selectedComponents, false);
					this.ProcessLaborTaskResources(containerElement, exportInputs, laborTaskElement, allLaborTaskElements, selectedComponents, exportBoe.IsMultiClinWbs);
					this.ProcessSkillMixTable(laborTaskElement, selectedComponents, containerElement, exportInputs);

					#endregion

					// add cloned container
					currentLaborTaskContainerInsertionPoint.ParentNode.InsertAfter(containerElement, currentLaborTaskContainerInsertionPoint);
					currentLaborTaskContainerInsertionPoint = containerElement;
				}

				// remove template container
				this.RemoveElement(laborTaskContainerTemplateElement);

				#endregion

				#endregion

				#region Task Container (Travel)

				#region Section Title (Travel)

				if (!allTravelTaskElementsFull.Any())
				{
					this.RemoveElement(travelSectionTitleElement);
				}

				#endregion

				#region Process container (each travel task)

				//Process the Travel Tasks if travel is included in the output format template
				if (travelTaskContainerTemplateElement != null)
				{
					StructuredDocumentTag currentTravelTaskContainerInsertionPoint = travelTaskContainerTemplateElement;
					//order the elements by order id
					allTravelTaskElementsFull = allTravelTaskElementsFull.OrderBy(x => x.BOETaskElementOrder).ToList();
					foreach (BOEExportTaskElement travelTaskElement in allTravelTaskElementsFull)
					{
						//  create (clone) a new container for this task
						StructuredDocumentTag containerElement = CloneContainerTemplate(travelTaskContainerTemplateElement);

						#region Process the data (IS&GS)

						ProcessTravelTaskHeader(document, containerElement, travelTaskElement, selectedComponents, ref counters);
						ProcessTravelTaskResourceTable(containerElement, travelTaskElement, selectedComponents);
						ProcessTravelTaskDirectCostRollupTable(containerElement, travelTaskElement, travelResources, selectedComponents, exportInputs, true);
						ProcessTravelTaskDirectCostRollupTable(containerElement, travelTaskElement, travelResources, selectedComponents, exportInputs, false);

						#endregion

						// add cloned container
						currentTravelTaskContainerInsertionPoint.ParentNode.InsertAfter(containerElement, currentTravelTaskContainerInsertionPoint);
						currentTravelTaskContainerInsertionPoint = containerElement;
					}

					// remove template container
					this.RemoveElement(travelTaskContainerTemplateElement);
				}

				#endregion

				#endregion

				#region Task Container (ODC)

				#region Section Title (ODC)

				if (!allODCTaskElementsFull.Any())
				{
					this.RemoveElement(odcSectionTitleElement);
				}

				#endregion

				#region Process container (each odc task)

				//Process the ODC Tasks if ODC is included in the output format template
				if (odcTaskContainerTemplateElement != null)
				{
					StructuredDocumentTag currentODCTaskContainerInsertionPoint = odcTaskContainerTemplateElement;
					//order the elements by order id
					allODCTaskElementsFull = allODCTaskElementsFull.OrderBy(x => x.BOETaskElementOrder).ToList();
					foreach (BOEExportTaskElement odcTaskElement in allODCTaskElementsFull)
					{
						//  create (clone) a new container for this task
						StructuredDocumentTag containerElement = CloneContainerTemplate(odcTaskContainerTemplateElement);

						#region Process the data (IS&GS)

						ProcessODCTaskHeader(document, containerElement, odcTaskElement, selectedComponents, ref counters);
						ProcessODCTaskResourceTable(containerElement, odcTaskElement, selectedComponents);
						ProcessODCTaskDirectCostRollupTable(containerElement, odcTaskElement, selectedComponents, exportInputs, false);
						ProcessODCTaskDirectCostRollupTable(containerElement, odcTaskElement, selectedComponents, exportInputs, true);
						ProcessODCTaskResources(containerElement, exportInputs, odcTaskElement, selectedComponents);

						#endregion

						// add cloned container
						currentODCTaskContainerInsertionPoint.ParentNode.InsertAfter(containerElement, currentODCTaskContainerInsertionPoint);
						currentODCTaskContainerInsertionPoint = containerElement;
					}

					// remove template container
					this.RemoveElement(odcTaskContainerTemplateElement);
				}

				#endregion

				#endregion

				#region Task Container (Material)

				#region Section Title (Material)

				if (!allMaterialTaskElementsFull.Any())
				{
					this.RemoveElement(materialSectionTitleElement);
				}

				#endregion

				#region Process container (each material task)
				//Process the Material Tasks if material is included in the output format template
				if (materialTaskContainerTemplateElement != null)
				{
					StructuredDocumentTag currentMaterialTaskContainerInsertionPoint = materialTaskContainerTemplateElement;

					foreach (BOEExportTaskElement materialTaskElement in allMaterialTaskElementsFull)
					{
						//  create (clone) a new container for this task
						StructuredDocumentTag containerElement = CloneContainerTemplate(materialTaskContainerTemplateElement);

						#region Process the data (IS&GS)

						ProcessMaterialTaskHeader(document, containerElement, materialTaskElement, selectedComponents, ref counters);
						ProcessMaterialTaskResourceTable(containerElement, materialTaskElement, selectedComponents);
						ProcessMaterialTaskDirectCostRollupTable(containerElement, materialTaskElement, selectedComponents, exportInputs, false);
						ProcessMaterialTaskDirectCostRollupTable(containerElement, materialTaskElement, selectedComponents, exportInputs, true);

						#endregion

						// add cloned container
						currentMaterialTaskContainerInsertionPoint.ParentNode.InsertAfter(containerElement, currentMaterialTaskContainerInsertionPoint);
						currentMaterialTaskContainerInsertionPoint = containerElement;
					}

					// remove template container
					this.RemoveElement(materialTaskContainerTemplateElement);
				}
				#endregion

				#endregion
			}
			else
			{
				#region Remove tasks element sections

				this.RemoveElement(laborSectionTitleElement);
				this.RemoveElement(travelSectionTitleElement);
				this.RemoveElement(odcSectionTitleElement);
				this.RemoveElement(materialSectionTitleElement);

				this.RemoveElement(laborTaskContainerTemplateElement);
				this.RemoveElement(travelTaskContainerTemplateElement);
				this.RemoveElement(odcTaskContainerTemplateElement);
				this.RemoveElement(materialTaskContainerTemplateElement);

				#endregion
			}

			#endregion
		}

		/// <summary>
		/// Deletes unnecessary columns (Res. Custom Fields, WBS, CLIN) from the resource types table for a given BOE.
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <param name="laborTaskContainerTemplateElement">The labor task container template element.</param>
		private static void RemoveUnusedFieldsFromResourceTypesTable(BOEExportInputs exportInputs, BOEExportModelView boeExportModelView, StructuredDocumentTag laborTaskContainerTemplateElement)
		{
			StructuredDocumentTag resourceTypesTableElement = WordUtilities.GetTaggedChildElement(laborTaskContainerTemplateElement, BOEExporterConstants.Table_ResourceTypes);

			if (resourceTypesTableElement != null)
			{
				//If there are no Resource Types level Custom Fields, delete column from table
				if (!exportInputs.CustomFields.Any(x => x.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay))
				{
					StructuredDocumentTag customFieldDescriptionBlock = WordUtilities.GetTaggedChildElement(resourceTypesTableElement, BOEExporterConstants.FieldName_CustomFieldDescription);
					if (customFieldDescriptionBlock != null)
					{
						WordUtilities.RemoveColumnFromTable(customFieldDescriptionBlock);
					}
				}
				//Delete WBS/CLIN columns if BOE is not Multi
				if (!boeExportModelView.IsMultiClinWbs)
				{
					StructuredDocumentTag resourceWbsBlock = WordUtilities.GetTaggedChildElement(laborTaskContainerTemplateElement, BOEExporterConstants.FieldName_ResourceWBS);
					StructuredDocumentTag resourceClinBlock = WordUtilities.GetTaggedChildElement(laborTaskContainerTemplateElement, BOEExporterConstants.FieldName_ResourceCLIN);
					if (resourceWbsBlock != null)
					{
						WordUtilities.RemoveColumnFromTable(resourceWbsBlock);
					}
					if (resourceClinBlock != null)
					{
						WordUtilities.RemoveColumnFromTable(resourceClinBlock);
					}
				}

				//Delete the Reference column if BOE is not a Summary BOE
				StructuredDocumentTag referenceBlock = WordUtilities.GetTaggedChildElement(resourceTypesTableElement, BOEExporterConstants.FieldName_SummaryReference);
				if (referenceBlock != null)
				{
					WordUtilities.RemoveColumnFromTable(referenceBlock);
				}
			}
		}

		/// <summary>
		/// Remove MOQ related containers only used when Template BOE is set to "No"
		/// For SSC, the MOQ Type container is always removed and the MOQ Rationale container is only removed if there are no MOQ RTE Templates
		/// </summary>
		/// <param name="wsHasMoqRteTemplate">Whether Worksace has RTE Templates for MOQ Rationale</param>
		/// <param name="containerElement">The container template</param>
		protected virtual void RemoveNonTemplateBoeContainers(bool wsHasMoqRteTemplate, StructuredDocumentTag containerElement)
		{
			WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQTypeContainer);

			if (!wsHasMoqRteTemplate)
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQRationaleContainer);
			}
		}

		#endregion

		#region Resource summary tables

		private void ProcessResourceSummaryByResourceTypeTable(StructuredDocumentTag boeContainer, ICollection<BOESummaryGridModelView> boeSummaryGridModelView, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Resource Summary By Resource Type Table

			StructuredDocumentTag resourceSummaryByResourceTypeTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_ResourceSummaryByResourceType);

			#region Component selection filtering

			if (resourceSummaryByResourceTypeTableElement == null)
			{
			}
			else if (selectedComponents.Contains(BoeCustomReportComponent.BOEResourceSummaryTable))
			{
				PopulateResourceSummaryByResourceTypeTable(resourceSummaryByResourceTypeTableElement, boeSummaryGridModelView);
			}
			else
			{
				this.RemoveElement(resourceSummaryByResourceTypeTableElement);
			}

			#endregion

			#endregion
		}

		private void ProcessResourceSummaryByElementOfCostTable(StructuredDocumentTag boeContainer, BOEExportModelView boeExportModelView, ICollection<BOESummaryGridModelView> boeSummaryGridModelView, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Resource Summary By Element Of Cost Table

			StructuredDocumentTag resourceSummaryByElementOfCostTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_ResourceSummaryByElementOfCost);

			#region Component selection filtering

			if (resourceSummaryByElementOfCostTableElement == null)
			{
			}
			else if (selectedComponents.Contains(BoeCustomReportComponent.BOEResourceSummaryTable))
			{
				PopulateResourceSummaryByElementOfCostTable(resourceSummaryByElementOfCostTableElement, boeExportModelView, boeSummaryGridModelView);
			}
			else
			{
				this.RemoveElement(resourceSummaryByElementOfCostTableElement);
			}

			#endregion

			#endregion
		}

		/// <summary>
		/// Processes the Resource Summary By Resource ID Table before populating it
		/// </summary>
		/// <param name="boeContainer">container for the BOE</param>
		/// <param name="boeExportModelView">Model View for the BOE Export</param>
		/// <param name="selectedComponents">selected components for the export</param>
		private void ProcessResourceSummaryByResourceIDTable(StructuredDocumentTag boeContainer, BOEExportModelView boeExportModelView, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Resource Summary By Resource ID Table

			StructuredDocumentTag resourceSummaryByResourceIDTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_ResourceSummaryByResourceID);

			if (resourceSummaryByResourceIDTableElement != null)
			{
				#region Component selection filtering

				if (selectedComponents.Contains(BoeCustomReportComponent.BOEResourceSummaryTable))
				{
					PopulateResourceSummaryByResourceIDTable(resourceSummaryByResourceIDTableElement, boeExportModelView);
				}
				else
				{
					this.RemoveElement(resourceSummaryByResourceIDTableElement);
				}

				#endregion
			}

			#endregion
		}

		#endregion

		#region Labor Hour Summary by Custom Field tables
		/// <summary>
		/// Process the data for the Labor Hours Summary By Custom Field table before populating it
		/// </summary>
		/// <param name="boeContainer">Container element for the BOE</param>
		/// <param name="boeExportModelView">Model View for the BOE Export</param>
		/// <param name="allWorkspaceCustomFields">All workspace custom fields.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="taskElementCollection">Collection of the BOE's task elements</param>
		/// <param name="isUsingEquivalentPerson">True if using EP, false if using Hours for spreads.</param>
		/// <param name="summarizeByCustomField">Name of custom field to group by when running All BOEs report with special format template.</param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual void ProcessLaborHoursSummaryByCustomFieldTable(StructuredDocumentTag boeContainer, BOEExportModelView boeExportModelView, IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, ICollection<BoeTaskElementDTO> taskElementCollection, BOEExportInputs exportInputs, bool isUsingEquivalentPerson, string summarizeByCustomField)
		{
			StructuredDocumentTag laborHoursSummaryByCustomFieldTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_LaborHoursSummaryByCustomField);

			if (laborHoursSummaryByCustomFieldTableElement != null)
			{
				if (!string.IsNullOrEmpty(summarizeByCustomField) && !summarizeByCustomField.Equals(BOEExporterConstants.SUMMARIZE_BY_NONE) && taskElementCollection.Any())
				{
					StructuredDocumentTag insertAfterElement = laborHoursSummaryByCustomFieldTableElement;

					// group the task labors by custom field value
					SortedDictionary<string, LaborResourceTypesTableData> laborSummaryData = GroupByCustomFieldValue(boeExportModelView, allWorkspaceCustomFields, taskElementCollection, exportInputs, summarizeByCustomField);

					// create and populate summary table for each custom field value
					foreach (KeyValuePair<string, LaborResourceTypesTableData> entry in laborSummaryData)
					{
						string customFieldValue = entry.Key;
						LaborResourceTypesTableData tableData = entry.Value;
						if (tableData.ResourcesData != null && tableData.ResourcesData.Any())
						{
							//  create (clone) a new container for this entry
							StructuredDocumentTag containerElement = laborHoursSummaryByCustomFieldTableElement.Clone(true) as StructuredDocumentTag;
							insertAfterElement.ParentNode.InsertAfter(containerElement, insertAfterElement);
							insertAfterElement = containerElement;

							PopulateLaborHoursSummaryByCustomFieldTable(containerElement, taskElementCollection, summarizeByCustomField, customFieldValue, tableData);
						}
					}
				}

				this.RemoveElement(laborHoursSummaryByCustomFieldTableElement);  // remove the table
			}
		}

		/// <summary>
		/// Group the Labor Hours data by custom field value.
		/// </summary>
		/// <param name="boeExportModelView">Model View for the BOE Export</param>
		/// <param name="allWorkspaceCustomFields">All workspace custom fields.</param>
		/// <param name="taskElementCollection">Collection of the BOE's task elements</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="summarizeByCustomField">Name of custom field to group by when running All BOEs report with special format template.</param>
		/// <returns>Dictionary containing LaborResourceTypesTableData for each custom field value.</returns>
		private SortedDictionary<string, LaborResourceTypesTableData> GroupByCustomFieldValue(BOEExportModelView boeExportModelView, IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, ICollection<BoeTaskElementDTO> taskElementCollection, BOEExportInputs exportInputs, string summarizeByCustomField)
		{
			SortedDictionary<string, LaborResourceTypesTableData> tableDataDictionary = new SortedDictionary<string, LaborResourceTypesTableData>();   // table data for each custom field value
			IDictionary<TaskElementType, ICollection<BoeTaskElementDTO>> boeTaskElementsByTaskElementType = GetBOETaskElementsByTaskElementType(taskElementCollection);
			ICollection<BoeTaskElementDTO> allLaborTaskElements = boeTaskElementsByTaskElementType[TaskElementType.Labor].ToList();
			IDictionary<BOEExportTaskElementType, ICollection<BOEExportTaskElement>> boeTaskElementsByElementOfCost = GetBOETaskElementsByElementOfCost(boeExportModelView);
			ICollection<BOEExportTaskElement> allLaborTaskElementsFull = boeTaskElementsByElementOfCost[BOEExportTaskElementType.Labor].ToList();

			foreach (BOEExportTaskElement laborTaskElement in allLaborTaskElementsFull)
			{
				// Retrieve custom field values for each labor task element.
				LaborResourceTypesTableData laborPerfOrgsData = laborTaskElement.taskElementLabors.Convert(WorkspaceDecimalPrecision);
				LoadResourceLevelCustomFields(laborTaskElement, allLaborTaskElements, allWorkspaceCustomFields, laborPerfOrgsData.ResourcesData, exportInputs);

				// Add each row to the proper summary table based on the custom field value
				foreach (LaborResourceTypesTableRowData rowData in laborPerfOrgsData.ResourcesData)
				{
					string customFieldValue = null;
					foreach (ExportCustomField customField in rowData.CustomFields)
					{
						if (customField.CustomFieldName.Equals(summarizeByCustomField))
						{
							customFieldValue = customField.CustomFieldValueDescription;
						}
					}

					if (string.IsNullOrEmpty(customFieldValue))
					{
						customFieldValue = "(Not Specified)";
					}

					// lookup summary table based on the custom field value
					LaborResourceTypesTableData tableData;
					if (!tableDataDictionary.TryGetValue(customFieldValue, out tableData))
					{
						tableDataDictionary.Add(customFieldValue, tableData =
							new LaborResourceTypesTableData
							{
								ResourcesData = new Collection<LaborResourceTypesTableRowData>(),
								HoursTotalComplete = 0,
								CostTotalComplete = 0
							});
					}

					tableData.ResourcesData.Add(rowData);
				}
			}

			return tableDataDictionary;
		}

		#endregion

		#region Hours and cost summary tables

		/// <summary>
		/// Process the data for the Labor Hours Summary Table before populating it
		/// </summary>
		/// <param name="boeContainer">Container element for the BOE</param>
		/// <param name="taskElementCollection">Collection of the BOE's task elements</param>
		/// <param name="resourcesByElementOfCost">Resources used by the BOE</param>
		/// <param name="selectedComponents">Components to be included in the export</param>
		/// <param name="isUsingEquivalentPerson">True if using EP, false if using Hours for spreads.</param>
		/// <param name="useGfy">use government fiscal year?</param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual void ProcessLaborHoursSummaryTable(StructuredDocumentTag boeContainer, ICollection<BoeTaskElementDTO> taskElementCollection, IDictionary<ElementOfCostType, Collection<ResourceDTO>> resourcesByElementOfCost, ICollection<BoeCustomReportComponent> selectedComponents, bool isUsingEquivalentPerson, bool useGfy)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Labor Hours Summary By Date Table

			StructuredDocumentTag laborHoursSummaryByDateTableElement = WordUtilities.GetTaggedChildElement(boeContainer,
				useGfy ? BOEExporterConstants.Table_GfyLaborHoursSummaryByDate : BOEExporterConstants.Table_LaborHoursSummaryByDate);

			bool byQuarter = false;
			if (useGfy && laborHoursSummaryByDateTableElement == null)
			{
				laborHoursSummaryByDateTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_GfyLaborHoursSummaryByQuarter);
				byQuarter = true;
			}

			if (selectedComponents.Contains(BoeCustomReportComponent.BOESpreadSummaryTables) && taskElementCollection.Any() && laborHoursSummaryByDateTableElement != null)
			{
				// compile the rollup data
				List<LaborRollupByDateNew> laborRollupData = this.GetRollupByYear(taskElementCollection, null, RateType.Hours, useGfy);
				IList<RollupSummaryByYearTableRowData> laborHoursSummaryRollupData = laborRollupData.Convert();

				RollupSummaryByYearTableData laborRollupTableData = new RollupSummaryByYearTableData
				{
					SummaryTotalComplete = laborHoursSummaryRollupData.Sum(d => d.YearTotal),
					YearlyData = laborHoursSummaryRollupData
				};

				PopulateRollupSummaryByYearTable(laborHoursSummaryByDateTableElement, null, laborRollupTableData, DefaultHoursFormat, byQuarter, useGfy);
			}
			else if (laborHoursSummaryByDateTableElement != null)
			{
				this.RemoveElement(laborHoursSummaryByDateTableElement);
				WordUtilities.RemoveTaggedElement(boeContainer, BOEExporterConstants.Container_BOESpreadTables);
			}
			#endregion
		}

		/// <summary>
		/// Processes the labor cost summary table.
		/// </summary>
		/// <param name="boeContainer">The boe container.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boe">The boe.</param>
		/// <param name="taskElementDtos">The task element dtos.</param>
		/// <param name="resourcesByElementOfCost">The resources by element of cost.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <param name="useGfy">use government fiscal year?</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// selectedComponents or boe or exportInputs</exception>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual List<LaborRollupByDateNew> ProcessLaborCostSummaryTable(StructuredDocumentTag boeContainer, BOEExportInputs exportInputs, BoeDTO boe, ICollection<BoeTaskElementDTO> taskElementDtos, IDictionary<ElementOfCostType, Collection<ResourceDTO>> resourcesByElementOfCost, ICollection<BoeCustomReportComponent> selectedComponents, bool useGfy)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			if (boe == null)
			{
				throw new ArgumentNullException(nameof(boe));
			}

			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			#region Labor Cost Summary By Date Table
			List<LaborRollupByDateNew> laborTasksRollupCostData = null;

			StructuredDocumentTag laborCostSummaryByDateTableElement = WordUtilities.GetTaggedChildElement(boeContainer,
				useGfy ? BOEExporterConstants.Table_GfyLaborCostSummaryByDate : BOEExporterConstants.Table_LaborCostSummaryByDate);

			bool byQuarter = false;
			if (useGfy && laborCostSummaryByDateTableElement == null)
			{
				laborCostSummaryByDateTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_GfyLaborCostSummaryByQuarter);
				byQuarter = true;
			}

			laborTasksRollupCostData = GetTaskCostRollup(taskElementDtos, exportInputs, null, useGfy);

			if (laborCostSummaryByDateTableElement != null && selectedComponents.Contains(BoeCustomReportComponent.BOESpreadSummaryTables))
			{
				RollupSummaryByYearTableData laborCostSummaryRollupData = laborTasksRollupCostData.ConvertToRollupSummaryByYear();

				PopulateRollupSummaryByYearTable(laborCostSummaryByDateTableElement, null, laborCostSummaryRollupData, DefaultCurrencyFormat, byQuarter, useGfy);
			}
			else
			{
				this.RemoveElement(laborCostSummaryByDateTableElement);
			}

			return laborTasksRollupCostData;

			#endregion
		}

		private List<LaborRollupByDateNew> ProcessNonLaborCostSummaryTable(BoeDTO boe, StructuredDocumentTag boeContainer, BOEExportModelView boeExportModelView, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Non-Labor Cost Summary By Date Table

			StructuredDocumentTag nonLaborCostSummaryByDateTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_NonLaborCostSummaryByDate);

			List<LaborRollupByDateNew> nonLaborRollupCostData = null;

			ICollection<OtherDirectCostDTO> odcElements = exportInputs.Odcs.Where(x => x.BoeID == boe.Id).ToList();
			ICollection<TravelDTO> travelElements = exportInputs.Travels.Where(x => x.BoeID == boe.Id).ToList();

			if (odcElements.Any() || travelElements.Any())
			{
				DateRange dateRange = GetODCTravelDateRange(odcElements, travelElements, false);

				// compile the rollup data
				nonLaborRollupCostData = this.GetODCTravelCostSummaryRollupByYearData(odcElements,
					boeExportModelView.TaskElements.SelectMany(x => x.taskElementLabors).ToCollection(), dateRange);
			}

			if (nonLaborCostSummaryByDateTableElement == null)
			{
			}
			else if (nonLaborRollupCostData == null || !selectedComponents.Contains(BoeCustomReportComponent.BOESpreadSummaryTables))
			{
				this.RemoveElement(nonLaborCostSummaryByDateTableElement);
			}
			else
			{
				IList<RollupSummaryByYearTableRowData> nonLaborRollupRowData = nonLaborRollupCostData.Convert();

				RollupSummaryByYearTableData nonLaborRollupTableData = new RollupSummaryByYearTableData
				{
					SummaryTotalComplete = nonLaborRollupRowData.Sum(d => d.YearTotal),
					YearlyData = nonLaborRollupRowData
				};

				PopulateRollupSummaryByYearTable(nonLaborCostSummaryByDateTableElement, null, nonLaborRollupTableData, DefaultCurrencyFormat, false);
			}

			return nonLaborRollupCostData;

			#endregion
		}

		private void ProcessLaborAndNonLaborCostSummaryTable(StructuredDocumentTag boeContainer, List<LaborRollupByDateNew> laborTasksRollupCostData, List<LaborRollupByDateNew> nonLaborRollupCostData, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Labor And Non Labor Cost Summary By Date Table

			StructuredDocumentTag laborAndNonLaborCostSummaryByDateTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_LaborAndNonLaborCostSummaryByDate);

			if (laborAndNonLaborCostSummaryByDateTableElement == null)
			{
			}
			else if (selectedComponents.Contains(BoeCustomReportComponent.BOESpreadSummaryTables))
			{
				/*
                 * Combine the results of the two previous rollups
                 * 
                 */
				List<LaborRollupByDateNew> laborAndNonLaborTaskRollupCostData = nonLaborRollupCostData == null ? laborTasksRollupCostData.Clone() : laborTasksRollupCostData.Merge(nonLaborRollupCostData);
				RollupSummaryByYearTableData laborAndNonLaborRollupCostData = laborAndNonLaborTaskRollupCostData.ConvertToRollupSummaryByYear();
				PopulateRollupSummaryByYearTable(laborAndNonLaborCostSummaryByDateTableElement, null, laborAndNonLaborRollupCostData, DefaultCurrencyFormat, false);
			}
			else
			{
				this.RemoveElement(laborAndNonLaborCostSummaryByDateTableElement);
			}

			#endregion
		}

		#endregion

		#region Spread rollup tables

		/// <summary>
		/// Process the Labor Task Hours Rollup Table before populating it
		/// </summary>
		/// <param name="containerElement">Container element for task</param>
		/// <param name="laborTaskElement">Element for the labor task</param>
		/// <param name="allLaborTaskElements">All task elements for the BOE</param>
		/// <param name="selectedComponents">Components selected for the output</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <exception cref="ArgumentNullException">selectedComponents</exception>
		protected virtual void ProcessLaborTaskHoursRollupTable(StructuredDocumentTag containerElement, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs, BOEExportModelView boeExportModelView)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			StructuredDocumentTag laborHoursRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_LaborHoursRollup);

			if (laborHoursRollupTableTemplateElement != null)
			{
				if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables))
				{
					PrepareLaborTaskHoursRollupTableData(laborHoursRollupTableTemplateElement, laborTaskElement, allLaborTaskElements, boeExportModelView, false, false);
				}
				else
				{
					this.RemoveElement(laborHoursRollupTableTemplateElement);
					WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.Container_TaskSpreadTables);
				}
			}
		}

		/// <summary>
		/// Prepare the data for the Labor Task Hours Rollup Table before populating it
		/// </summary>
		/// <param name="templateElement">Template element for the table</param>
		/// <param name="laborTaskElement">Element for the labor task</param>
		/// <param name="allLaborTaskElements">All task elements for the BOE</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <param name="useGfy">Should Government Fiscal Years be used</param>
		/// <param name="byQuarter">If table uses quarters instead of months</param>
		protected virtual void PrepareLaborTaskHoursRollupTableData(StructuredDocumentTag templateElement, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, BOEExportModelView boeExportModelView, bool useGfy, bool byQuarter)
		{
			BoeTaskElementDTO currentLaborTaskElement = allLaborTaskElements.FirstOrDefault(x => x.Id == laborTaskElement.BOETaskElementID.Value);

			// if description is present, then it should be used instead of resource id.
			bool useDescriptionInsteadOfName = WordUtilities.GetTaggedChildElement(WordUtilities.GetTaggedChildElement(templateElement, BOEExporterConstants.Marker_DataRow)
																	.GetAncestor(NodeType.Row), BOEExporterConstants.FieldName_ResourceDescription)
																	!= null;

			Dictionary<int, List<LaborRollupByDateNew>> currentLaborTaskRollupData = this.GetTaskHourRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useDescriptionInsteadOfName, useGfy, null, RateType.Hours);

			RollupSummaryByGroupByYearTableData laborSummaryRollupData = currentLaborTaskRollupData.Convert();
			PopulateRollupSummaryByGroupByYearTable(templateElement, null, laborSummaryRollupData, DefaultHoursFormat, byQuarter, useGfy);
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
		protected virtual void ProcessLaborTaskCostSpreadRollupTable(StructuredDocumentTag containerElement, BOEExportInputs exportInputs, BOEExportModelView boeExportModelView, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, ICollection<BoeCustomReportComponent> selectedComponents, bool useGfy)
		{
			//Nothing to do in ISGS mode
			return;
		}

		#endregion

		#region Task headers

		/// <summary>
		/// Gets the MOQ equation display text for an export
		/// </summary>
		/// <param name="taskElement">The task element containing the data needed to construct the moq equation display</param>
		/// <param name="exportInputs">export Inputs</param>
		/// <returns>The MOQ equation display text</returns>
		private string GetMOQEquationToDisplay(BOEExportTaskElement taskElement, BOEExportInputs exportInputs)
		{
			string MoqToDisplay = string.Empty;

			if (taskElement.ElementType == BOEExportTaskElementType.Labor)
			{
				// need to reformat the equation with variable markers <> so strings can be replaced as a whole 
				// the issue with replacing names with a contains is it's possible to have 2 task variables, DM BASE and SE, and since "SE" is in both task variable names, the contains
				// would try to replace a value for both objects so the "SE" in "DM BASE" would be replaced with a value which is incorrect.
				string FORMAT_MarkedVariableReplacement = "<{0}>";

				string originalMoq = Parser.UntagVariables(taskElement.MOQEquation, taskElement.WorkspaceVariables);

				// to find and replace with their values
				ICollection<string> validationResults = Common.MOQ.Parser.Validate(originalMoq).ToList();

				// Set the equation to the first result returned from Validate, which is the
				// re-formatted input equation
				string equation = validationResults.First();
				MoqToDisplay = equation;

				// If there were variables found in the equation, we'll replace them with their values
				if (validationResults.Count > 1)
				{
					// Iterate over all ordinary variables and replace any occurrences of those variables
					// in the equation with the corresponding value
					if (taskElement.OrdinaryVariables.Any())
					{
						foreach (OrdinaryVariableDto taskvar in taskElement.OrdinaryVariables)
						{
							string variableReplacementRegex = string.Format(FORMAT_MarkedVariableReplacement, taskvar.OrdinaryVariableName);
							Match s = Regex.Match(equation, variableReplacementRegex, RegexOptions.IgnoreCase, CommonConstants.REGEX_TIMEOUT);
							if (s.Success)
							{
								// need to pull the string from the equation instead of what is stored in the DB since variable names are always stored in UpperCase
								string variableNameWithCorrectCap = s.ToString().Trim(new char[] { '<', '>' });

								DataClassForSumOfBOEsCalculation sumOfBoesCalculator = new DataClassForSumOfBOEsCalculation();
								sumOfBoesCalculator.FillData(new List<OrdinaryVariableDto>() { taskvar }, null, exportInputs.WbsElements, exportInputs.AllWorkspaceBoes, exportInputs.TaskElements, exportInputs.ResourcesForWsResourceListId, exportInputs.Clins);
								decimal ordinaryVariableValue = _VariableSelectBOEtoSumCalculation.GetTaskVarLabelTotal(taskvar, sumOfBoesCalculator);

								MoqToDisplay = MoqToDisplay.Replace(MoqToDisplay, Regex.Replace(MoqToDisplay, variableReplacementRegex, Convert.ToDecimal(ordinaryVariableValue).ToString("0.#######") + " " + variableNameWithCorrectCap, RegexOptions.IgnoreCase, CommonConstants.REGEX_TIMEOUT));
							}
						}
					}

					// Iterate over all workspace variables and replace any occurrences of those variables
					// in the equation with the corresponding value
					if (taskElement.WorkspaceVariables.Any())
					{
						foreach (WorkspaceVariableDTO workspacevar in taskElement.WorkspaceVariables)
						{
							string variableReplacementRegex = string.Format(FORMAT_MarkedVariableReplacement, workspacevar.WorkspaceVariableName);
							Match s = Regex.Match(equation, variableReplacementRegex, RegexOptions.IgnoreCase, CommonConstants.REGEX_TIMEOUT);
							if (s.Success)
							{
								// need to pull the string from the equation instead of what is stored in the DB since variable names are always stored in UpperCase
								string variableNameWithCorrectCap = s.ToString().Trim(new char[] { '<', '>' });

								decimal workspaceVariableValue = workspacevar.WorkspaceVariableValue;
								MoqToDisplay = MoqToDisplay.Replace(MoqToDisplay, Regex.Replace(MoqToDisplay, variableReplacementRegex, Convert.ToDecimal(workspaceVariableValue).ToString("0.#######") + " " + variableNameWithCorrectCap, RegexOptions.IgnoreCase, CommonConstants.REGEX_TIMEOUT));

							}
						}
					}

				}
			}

			return MoqToDisplay;
		}

		/// <summary>
		/// Calculates the total of the MOQ equation for the export
		/// </summary>
		/// <param name="taskElement">Task element containing the MOQ</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="allTaskElements">All task elements in the BOE - needed to be able to correctly calculate variable values</param>
		/// <returns>Formatted string of the MOQ total value</returns>
		/// <exception cref="GenValidationException">The report could not be generated because there is an invalid MOQ Equation in the workspace. Please run the Validate All BOEs Report to determine the location of this error. Please correct the invalid MOQ Equation before attempting the export again.</exception>
		private string GetMOQTotal(BOEExportTaskElement taskElement, BOEExportInputs exportInputs, IReadOnlyCollection<BoeTaskElementDTO> allTaskElements)
		{
			decimal moqResult = 0;

			try
			{
				DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
				data.FillData(taskElement.OrdinaryVariables, taskElement.WorkspaceVariables, exportInputs.WbsElements, exportInputs.AllWorkspaceBoes, allTaskElements, exportInputs.ResourcesForWsResourceListId, exportInputs.Clins);

				string moqResultString = Common.MOQ.Parser.Calculate(taskElement.MOQEquation, taskElement.OrdinaryVariables, taskElement.WorkspaceVariables,
					_VariableSelectBOEtoSumCalculation, data, exportInputs.Workspace);

				decimal tempMOQResult;
				if (decimal.TryParse(moqResultString, out tempMOQResult))
				{
					moqResult = tempMOQResult;
				}
			}
			catch
			{
				throw new GenValidationException("The report could not be generated because there is an invalid MOQ Equation in the workspace. Please run the Validate All BOEs Report to determine the location of this error. Please correct the invalid MOQ Equation before attempting the export again.");
			}

			return moqResult == 0 ? string.Empty : " " + CommonUtilities.FormatStringWithPrecision(moqResult, WorkspaceDecimalPrecision);
		}

		/// <summary>
		/// Populates Task Element container with BOE Header level data
		/// </summary>
		/// <param name="containerElement">Task Container</param>
		/// <param name="selectedComponents">BOE Custom Report Selected Components</param>
		/// <param name="boeExportModelView">BOE Export Model View</param>
		private void ProcessTaskHeaderWithBoeData(StructuredDocumentTag containerElement,
			ICollection<BoeCustomReportComponent> selectedComponents, BOEExportModelView boeExportModelView)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			IDictionary<string, string> taskHeaderBoeDataValueMappings = new Dictionary<string, string>();

			//WBS Number
			if (selectedComponents.Contains(BoeCustomReportComponent.BOEWBS))
			{
				if (WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_WBSNumberInTask) != null)
				{
					taskHeaderBoeDataValueMappings.Add(BOEExporterConstants.FieldName_WBSNumberInTask, boeExportModelView.WBSNumber);
				}
			}
			//BOE Title
			if (WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_BOETitleInTask) != null)
			{
				taskHeaderBoeDataValueMappings.Add(BOEExporterConstants.FieldName_BOETitleInTask, boeExportModelView.BOETitle);
			}

			//Populate Fields
			foreach (KeyValuePair<string, string> entry in taskHeaderBoeDataValueMappings)
			{
				StructuredDocumentTag headerDataElement = WordUtilities.GetTaggedChildElement(containerElement, entry.Key);

				WordUtilities.SetElementText(headerDataElement, entry.Value);

			}

		}

		/// <summary>
		/// Processes the labor task header.
		/// </summary>
		/// <param name="document">The document.</param>
		/// <param name="containerElement">The container element.</param>
		/// <param name="laborTaskElement">The labor task element.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="ws">Full WS</param>
		/// <param name="counters">The counters.</param>
		/// <exception cref="ArgumentNullException">selectedComponents</exception>
		private void ProcessLaborTaskHeader(Document document, StructuredDocumentTag containerElement, BOEExportTaskElement laborTaskElement,
			ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs, ref ChunkCounter counters)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Labor task header data

			#region Adjust task start and end dates (as needed) to match resource/spread dates

			ICollection<DateTime> allStartDates = laborTaskElement.taskElementLabors.Where(r => r.StartDate.HasValue).Select(r => r.StartDate.Value).ToList();
			ICollection<DateTime> allEndDates = laborTaskElement.taskElementLabors.Where(r => r.EndDate.HasValue).Select(r => r.EndDate.Value).ToList();

			string adjustedTaskStartDate = allStartDates.Any() ? allStartDates.Min().ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : laborTaskElement.StartDate.HasValue ? laborTaskElement.StartDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : string.Empty;
			string adjustedTaskEndDate = allEndDates.Any() ? allEndDates.Max().ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : laborTaskElement.EndDate.HasValue ? laborTaskElement.EndDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : string.Empty;

			#endregion

			IDictionary<string, string> laborTaskHeaderDataValueMappings = new Dictionary<string, string>
					{
						{ BOEExporterConstants.FieldName_TaskElementTitle, laborTaskElement.TaskTitle },
						{ BOEExporterConstants.FieldName_TaskStartDate, adjustedTaskStartDate },
						{ BOEExporterConstants.FieldName_TaskEndDate, adjustedTaskEndDate }
					};

			#region Component selection filtering

			if (!string.IsNullOrEmpty(laborTaskElement.BOETaskID))
			{
				laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskElementID, laborTaskElement.BOETaskID);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.Container_TaskID);
			}

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskDescription))
			{
				//This is boken into 2 separate conditional statements because there's no need to check the template for the tag if it wasn't a selected component
				if (WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_TaskDescription) != null)
				{
					laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskDescription, laborTaskElement.BOETaskDesc);
				}
				else if (WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_TaskDescription_NoSpacing) != null)
				{
					laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskDescription_NoSpacing, laborTaskElement.BOETaskDesc);
				}
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskDescription);
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskDescriptionLabel);
			}

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskMOQType))
			{
				laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_MOQType, laborTaskElement.MOQType);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQTypeContainer);
			}

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskMOQEquation))
			{
				string moqEquationForDisplay = GetMOQEquationToDisplay(laborTaskElement, exportInputs);
				laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_MOQEquation, moqEquationForDisplay);
				//need to calculate MOQ total for cases where MOQ and total hours are not equal
				laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_MOQEquationResult, this.GetMOQTotal(laborTaskElement, exportInputs, exportInputs.TaskElements));
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQEquationContainer);
			}

			if (exportInputs.Workspace.UsingTemplateBOE)
			{
				bool wsHasMoqRteTemplate = exportInputs.RTETemplatesOverrides.Any(x => x.SourceId == (int)RteTemplateSource.TaskMOQ);

				RemoveNonTemplateBoeContainers(wsHasMoqRteTemplate, containerElement);

				if (selectedComponents.Contains(BoeCustomReportComponent.TaskMOQType))
				{
					StructuredDocumentTag templateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Container_MOQSelection);
					this.PopulateMOQTypeData(laborTaskElement, selectedComponents, document, templateElement, true, exportInputs, ref counters);

					// if using RTE Template for MOQ Types, populate the fields
					if (wsHasMoqRteTemplate)
					{
						// Update label
						laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_MethodOfQuotingLabel, "Additional MOQ Rationale: ");

						// populate RTE MOQ text
						if (WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_MethodOfQuoting) != null)
						{
							laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_MethodOfQuoting, laborTaskElement.MOQText);
						}
						else if (WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_MethodOfQuoting_NoSpacing) != null)
						{
							laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_MethodOfQuoting_NoSpacing, laborTaskElement.MOQText);
						}
					}
				}
				else
				{
					WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQTypeSelectionTitle);
					WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.Container_MOQSelection);
				}
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQTypeSelectionTitle);
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.Container_MOQSelection);

				if (selectedComponents.Contains(BoeCustomReportComponent.TaskMOQRationale))
				{
					if (WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_MethodOfQuoting) != null)
					{
						laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_MethodOfQuoting, laborTaskElement.MOQText);
					}
					else if (WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_MethodOfQuoting_NoSpacing) != null)
					{
						laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_MethodOfQuoting_NoSpacing, laborTaskElement.MOQText);
					}
				}
				else
				{
					WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQRationaleContainer);
				}
			}

			//Remove "Method of Quoting" Section heading if there are no MOQ selections made
			if (!selectedComponents.Contains(BoeCustomReportComponent.TaskMOQType) && !selectedComponents.Contains(BoeCustomReportComponent.TaskMOQEquation) && !selectedComponents.Contains(BoeCustomReportComponent.TaskMOQRationale))
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQSectionLabel);
			}

			if (!selectedComponents.Contains(BoeCustomReportComponent.TaskMOQType) && !selectedComponents.Contains(BoeCustomReportComponent.TaskMOQRationale))
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQTypeSelectionTitle);
			}

			if (laborTaskElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_TaskHoursTotal))
			{
				laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskHoursTotal, laborTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskHoursTotal]);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskHoursTotal);
			}

			if (laborTaskElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_TaskCostTotal))
			{
				laborTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskCostTotal, laborTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskCostTotal]);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskCostTotal);
			}

			#endregion

			#region Assign header items

			foreach (KeyValuePair<string, string> entry in laborTaskHeaderDataValueMappings)
			{
				StructuredDocumentTag headerDataElement = WordUtilities.GetTaggedChildElement(containerElement, entry.Key);

				if (entry.Key == BOEExporterConstants.FieldName_TaskDescription || entry.Key == BOEExporterConstants.FieldName_MethodOfQuoting)
				{
					WordUtilities.SetElementTextWithHTML(document, headerDataElement, entry.Value, ref counters, false);
				}
				else if (entry.Key == BOEExporterConstants.FieldName_TaskDescription_NoSpacing || entry.Key == BOEExporterConstants.FieldName_MethodOfQuoting_NoSpacing)
				{
					WordUtilities.SetElementTextWithHTML(document, headerDataElement, entry.Value, ref counters, true);
				}
				else
				{
					WordUtilities.SetElementText(headerDataElement, entry.Value);
				}
			}

			#endregion

			#endregion
		}

		/// <summary>
		/// Process and populate the data for the Travel Task Header
		/// </summary>
		/// <param name="document">The Word document to populate</param>
		/// <param name="containerElement">Container element for the Travel Task Header</param>
		/// <param name="travelTaskElement">Task element for the Travel task</param>
		/// <param name="selectedComponents">Selected components for the export</param>
		private void ProcessTravelTaskHeader(Document document, StructuredDocumentTag containerElement, BOEExportTaskElement travelTaskElement,
			ICollection<BoeCustomReportComponent> selectedComponents, ref ChunkCounter counters)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Travel task header data

			IDictionary<string, string> travelTaskHeaderDataValueMappings = new Dictionary<string, string>
							{
								{ BOEExporterConstants.FieldName_TaskElementTitle, travelTaskElement.TaskTitle },
								{ BOEExporterConstants.FieldName_TaskStartDate, travelTaskElement.StartDate.HasValue ? travelTaskElement.StartDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : string.Empty },
								{ BOEExporterConstants.FieldName_TaskEndDate, travelTaskElement.EndDate.HasValue ? travelTaskElement.EndDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : string.Empty }
							};

			if (!string.IsNullOrEmpty(travelTaskElement.BOETaskID))
			{
				travelTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskElementID, travelTaskElement.BOETaskID);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.Container_TaskID);
			}

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskDescription))
			{
				travelTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskDescription, travelTaskElement.BOETaskDesc);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskDescriptionLabel);
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskDescription);
			}

			foreach (KeyValuePair<string, string> entry in travelTaskHeaderDataValueMappings)
			{
				StructuredDocumentTag headerDataElement = WordUtilities.GetTaggedChildElement(containerElement, entry.Key);

				if (headerDataElement != null)
				{
					if (entry.Key == BOEExporterConstants.FieldName_TaskDescription)
					{
						WordUtilities.SetElementTextWithHTML(document, headerDataElement, entry.Value, ref counters);
					}
					else
					{
						WordUtilities.SetElementText(headerDataElement, entry.Value);
					}
				}
			}

			#endregion
		}

		/// <summary>
		/// Process and populate the data for the ODC Task Header
		/// </summary>
		/// <param name="document">The Word document to populate</param>
		/// <param name="containerElement">Container element for the ODC Task Header</param>
		/// <param name="odcTaskElement">Task element for the ODC task</param>
		/// <param name="selectedComponents">Selected components for the export</param>
		private void ProcessODCTaskHeader(Document document, StructuredDocumentTag containerElement, BOEExportTaskElement odcTaskElement,
			ICollection<BoeCustomReportComponent> selectedComponents, ref ChunkCounter counters)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region ODC task header data

			IDictionary<string, string> odcTaskHeaderDataValueMappings = new Dictionary<string, string>
							{
								{ BOEExporterConstants.FieldName_TaskElementTitle, odcTaskElement.TaskTitle },
								{ BOEExporterConstants.FieldName_TaskStartDate, odcTaskElement.StartDate.HasValue ? odcTaskElement.StartDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : string.Empty },
								{ BOEExporterConstants.FieldName_TaskEndDate, odcTaskElement.EndDate.HasValue ? odcTaskElement.EndDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : string.Empty }
							};

			#region Component selection filtering

			if (!string.IsNullOrEmpty(odcTaskElement.BOETaskID))
			{
				odcTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskElementID, odcTaskElement.BOETaskID);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.Container_TaskID);
			}

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskDescription))
			{
				odcTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskDescription, odcTaskElement.BOETaskDesc);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskDescriptionLabel);
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskDescription);
			}

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskMOQRationale))
			{
				odcTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_MethodOfQuoting, odcTaskElement.MOQText);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQSectionLabel);
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQRationaleContainer);
			}

			if (odcTaskElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_TaskHoursTotal))
			{
				odcTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskHoursTotal, odcTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskHoursTotal]);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskHoursTotal);
			}

			if (odcTaskElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_TaskCostTotal))
			{
				odcTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskCostTotal, odcTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskCostTotal]);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskCostTotal);
			}

			#endregion

			foreach (KeyValuePair<string, string> entry in odcTaskHeaderDataValueMappings)
			{
				StructuredDocumentTag headerDataElement = WordUtilities.GetTaggedChildElement(containerElement, entry.Key);

				if (headerDataElement != null)
				{
					if (entry.Key == BOEExporterConstants.FieldName_TaskDescription ||
						entry.Key == BOEExporterConstants.FieldName_MethodOfQuoting)
					{
						WordUtilities.SetElementTextWithHTML(document, headerDataElement, entry.Value, ref counters);
					}
					else
					{
						WordUtilities.SetElementText(headerDataElement, entry.Value);
					}
				}
			}

			#endregion
		}

		/// <summary>
		/// Process and populate the data for the Material Task Header
		/// </summary>
		/// <param name="document">The Word document to populate</param>
		/// <param name="containerElement">Container element for the Material Task Header</param>
		/// <param name="materialTaskElement">Task element for the Material task</param>
		/// <param name="selectedComponents">Selected components for the export</param>
		private void ProcessMaterialTaskHeader(Document document, StructuredDocumentTag containerElement, BOEExportTaskElement materialTaskElement,
			ICollection<BoeCustomReportComponent> selectedComponents, ref ChunkCounter counters)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Material task header data

			IDictionary<string, string> materialTaskHeaderDataValueMappings = new Dictionary<string, string>
							{
								{ BOEExporterConstants.FieldName_TaskElementTitle, materialTaskElement.TaskTitle },
								{ BOEExporterConstants.FieldName_TaskStartDate, materialTaskElement.StartDate.HasValue ? materialTaskElement.StartDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : string.Empty },
								{ BOEExporterConstants.FieldName_TaskEndDate, materialTaskElement.EndDate.HasValue ? materialTaskElement.EndDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR) : string.Empty }
							};

			#region Component selection filtering

			if (!string.IsNullOrEmpty(materialTaskElement.BOETaskID))
			{
				materialTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskElementID, materialTaskElement.BOETaskID);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.Container_TaskID);
			}

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskDescription))
			{
				materialTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_TaskDescription, materialTaskElement.BOETaskDesc);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskDescriptionLabel);
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_TaskDescription);
			}

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskMOQRationale))
			{
				materialTaskHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_MethodOfQuoting, materialTaskElement.MOQText);
			}
			else
			{
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQSectionLabel);
				WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQRationaleContainer);
			}

			#endregion

			foreach (KeyValuePair<string, string> entry in materialTaskHeaderDataValueMappings)
			{
				StructuredDocumentTag headerDataElement = WordUtilities.GetTaggedChildElement(containerElement, entry.Key);

				if (headerDataElement != null)
				{
					if (entry.Key == BOEExporterConstants.FieldName_TaskDescription ||
						entry.Key == BOEExporterConstants.FieldName_MethodOfQuoting)
					{
						WordUtilities.SetElementTextWithHTML(document, headerDataElement, entry.Value, ref counters);
					}
					else
					{
						WordUtilities.SetElementText(headerDataElement, entry.Value);
					}
				}
			}

			#endregion
		}

		#endregion

		#region Task resource tables

		/// <summary>
		/// Loads the resource level custom fields.
		/// </summary>
		/// <param name="laborTaskElement">The labor task element.</param>
		/// <param name="allLaborTaskElements">All labor task elements.</param>
		/// <param name="allWorkspaceCustomFields">All workspace custom fields.</param>
		/// <param name="resourcesData">The resources data.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <exception cref="ArgumentNullException">
		/// laborTaskElement
		/// or
		/// resourcesData
		/// or
		/// exportInputs
		/// </exception>
		protected void LoadResourceLevelCustomFields(BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, ICollection<LaborResourceTypesTableRowData> resourcesData, BOEExportInputs exportInputs)
		{
			if (laborTaskElement == null)
			{
				throw new ArgumentNullException(nameof(laborTaskElement));
			}

			if (resourcesData == null)
			{
				throw new ArgumentNullException(nameof(resourcesData));
			}

			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			/*
             * 
             * Retrieve resource-level custom fields
             * 
             */

			int taskElementId = laborTaskElement.BOETaskElementID.HasValue ? laborTaskElement.BOETaskElementID.Value : 0;
			ICollection<ResourceTypeDto> allCurrentTaskElementLaborTypes = allLaborTaskElements.Where(lt => lt.Id == taskElementId).SelectMany(lt => lt.taskElementLabors).ToList();

			Dictionary<int, ICollection<KeyValuePair<int, int>>> laborTypeCustomFieldValueIdMappings = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
			foreach (int laborTypeId in allCurrentTaskElementLaborTypes.Select(x => x.Id))
			{
				KeyValuePair<int, ICollection<KeyValuePair<int, int>>> laborMapping = exportInputs.LaborTypesMappingWithCustomFieldsValuesAndContainerIds.FirstOrDefault(x => x.Key == laborTypeId);
				if (!laborMapping.Equals(default(KeyValuePair<int, ICollection<KeyValuePair<int, int>>>)))
				{
					laborTypeCustomFieldValueIdMappings.Add(laborTypeId, laborMapping.Value);
					//laborTypeCustomFieldValueIdMappings[laborTypeId] = laborMapping.Value;
				}
			}

			IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> allTaskResourcesCustomFields = GetTaskElementCustomFields(laborTypeCustomFieldValueIdMappings, allWorkspaceCustomFields, exportInputs);

			foreach (LaborResourceTypesTableRowData laborPerfOrgsRowData in resourcesData)
			{
				int laborTypeId;

				if (int.TryParse(laborPerfOrgsRowData.LaborTypeID, out laborTypeId))
				{
					ResourceTypeDto matchingLaborType = allCurrentTaskElementLaborTypes.FirstOrDefault(lt => lt.Id == laborTypeId);
					if (matchingLaborType != null)
					{
						if (allTaskResourcesCustomFields.ContainsKey(laborTypeId) && laborTypeCustomFieldValueIdMappings.ContainsKey(laborTypeId))
						{
							IDictionary<CustomFieldValueDTO, CustomFieldDTO> laborTypeResourceCustomFields = allTaskResourcesCustomFields[laborTypeId];
							ICollection<int> laborTypeCustomFieldValueIds = laborTypeCustomFieldValueIdMappings[laborTypeId].Select(c => c.Value).ToList();

							ICollection<ExportCustomField> laborTypeCustomFields = (from cf in laborTypeResourceCustomFields
																					join id in laborTypeCustomFieldValueIds on cf.Key.CustomFieldValueID equals id
																					select new ExportCustomField
																					{
																						CustomFieldName = cf.Value.CustomFieldName,
																						CustomFieldValueName = cf.Key.CustomFieldValueName,
																						CustomFieldValueDescription = cf.Key.CustomFieldValueDescription,
																						IsOpenEnded = cf.Value.IsOpenEnded
																					}).ToList();

							laborPerfOrgsRowData.CustomFields = laborTypeCustomFields.ToCollection();
						}
					}
				}
			}
		}

		/// <summary>
		/// Processes the Labor Task Resource Table before populating it
		/// </summary>
		/// <param name="containerElement">Container element for task</param>
		/// <param name="laborTaskElement">Element for the labor task</param>
		/// <param name="allLaborTaskElements">All task elements for the BOE</param>
		/// <param name="allWorkspaceCustomFields">All custom fields in the current workspace</param>
		/// <param name="selectedComponents">Components selected for the output</param>
		/// <param name="exportInputs">The export inputs.</param>
		private void ProcessLaborTaskResourceTable(StructuredDocumentTag containerElement, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements,
			IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			if (laborTaskElement == null)
			{
				throw new ArgumentNullException(nameof(laborTaskElement));
			}

			#region Resource Types Table

			StructuredDocumentTag resourceTypesTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceTypes);

			if (resourceTypesTableElement == null)
			{
			}
			else if (selectedComponents.Contains(BoeCustomReportComponent.TaskResourceTypesSummaryTable))
			{
				PrepareLaborTaskResourceTableData(resourceTypesTableElement, laborTaskElement, allLaborTaskElements, allWorkspaceCustomFields, exportInputs);
			}
			else
			{
				this.RemoveElement(resourceTypesTableElement);
			}

			#endregion
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
		protected virtual void PrepareLaborTaskResourceTableData(StructuredDocumentTag tableElement, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, BOEExportInputs exportInputs)
		{
			if (laborTaskElement == null)
			{
				throw new ArgumentNullException(nameof(laborTaskElement));
			}

			#region Resource-level custom fields

			LaborResourceTypesTableData laborPerfOrgsData = laborTaskElement.taskElementLabors.Convert(WorkspaceDecimalPrecision);

			LoadResourceLevelCustomFields(laborTaskElement, allLaborTaskElements, allWorkspaceCustomFields, laborPerfOrgsData.ResourcesData, exportInputs);

			#endregion

			PopulateResourceTypesTable(tableElement, laborPerfOrgsData);
		}

		/// <summary>
		/// Process the data for the Travel Task Resource Table
		/// </summary>
		/// <param name="containerElement">Container element for the table</param>
		/// <param name="travelTaskElement">BOE Export Task Element for the Travel Task</param>
		/// <param name="selectedComponents">Selected components for the custom export</param>
		protected virtual void ProcessTravelTaskResourceTable(StructuredDocumentTag containerElement, BOEExportTaskElement travelTaskElement, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (travelTaskElement == null) { throw new ArgumentNullException(nameof(travelTaskElement)); }
			if (selectedComponents == null) { throw new ArgumentNullException(nameof(selectedComponents)); }

			#region Resource Types Table

			StructuredDocumentTag resourceTypesTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceTypes);

			if (selectedComponents.Contains(BoeCustomReportComponent.TaskResourceTypesSummaryTable) && resourceTypesTableElement != null)
			{
				TravelResourceTypesTableData travelPerfOrgsData = travelTaskElement.taskElementLabors.ConvertTravel();
				PopulateResourceTypesTable(resourceTypesTableElement, travelPerfOrgsData);
			}
			else
			{
				this.RemoveElement(resourceTypesTableElement);
			}

			#endregion
		}

		private void ProcessODCTaskResourceTable(StructuredDocumentTag containerElement, BOEExportTaskElement odcTaskElement, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Resource Types Table

			StructuredDocumentTag resourceTypesTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceTypes);

			if (resourceTypesTableElement == null)
			{
			}
			else if (selectedComponents.Contains(BoeCustomReportComponent.TaskResourceTypesSummaryTable))
			{
				ODCResourceTypesTableData odcPerfOrgsData = odcTaskElement.taskElementLabors.ConvertODC();
				PopulateResourceTypesTable(resourceTypesTableElement, odcPerfOrgsData);
			}
			else
			{
				this.RemoveElement(resourceTypesTableElement);
			}

			#endregion
		}

		private void ProcessMaterialTaskResourceTable(StructuredDocumentTag containerElement, BOEExportTaskElement materialTaskElement, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			#region Resource Types Table

			StructuredDocumentTag resourceTypesTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceTypes);

			if (resourceTypesTableElement == null)
			{
			}
			else if (selectedComponents.Contains(BoeCustomReportComponent.TaskResourceTypesSummaryTable))
			{
				MaterialResourceTypesTableData materialPerfOrgsData = materialTaskElement.taskElementLabors.ConvertMaterial();
				PopulateResourceTypesTable(resourceTypesTableElement, materialPerfOrgsData);
			}
			else
			{
				this.RemoveElement(resourceTypesTableElement);
			}

			#endregion
		}

		#endregion

		#region Task resource containers

		/// <summary>
		/// Processes the labor task resources.
		/// </summary>
		/// <param name="containerElement">The container element.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="laborTaskElement">The labor task element.</param>
		/// <param name="allLaborTaskElements">All labor task elements.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <param name="multiBoe">if set to <c>true</c> [multi boe].</param>
		protected virtual void ProcessLaborTaskResources(StructuredDocumentTag containerElement, BOEExportInputs exportInputs, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, ICollection<BoeCustomReportComponent> selectedComponents, bool multiBoe)
		{
			//nothing to do in ISGS mode
			return;
		}

		/// <summary>
		/// Processes the odc task resources.
		/// </summary>
		/// <param name="containerElement">The container element.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="odcTaskElement">The odc task element.</param>
		/// <param name="selectedComponents">The selected components.</param>
		protected virtual void ProcessODCTaskResources(StructuredDocumentTag containerElement, BOEExportInputs exportInputs, BOEExportTaskElement odcTaskElement, ICollection<BoeCustomReportComponent> selectedComponents)
		{
			//nothing to do in ISGS mode
			return;
		}

		/// <summary>
		/// Process and populate the Resource Cost Rollup table for an ODC resource
		/// </summary>
		/// <param name="containerElement">Container elment for the resource</param>
		/// <param name="odcTaskElement">Element for the ODC task containing the resource</param>
		/// <param name="ODCType">ODC task data, including spreads</param>
		protected void ProcessODCResourceCostRollupTable(StructuredDocumentTag containerElement, BOEExportTaskElement odcTaskElement, OtherDirectCostType ODCType)
		{
			if (odcTaskElement == null)
			{
				throw new ArgumentNullException(nameof(odcTaskElement));
			}

			StructuredDocumentTag resourceTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceCostRollup);

			if (resourceTableElement != null)
			{
				Collection<LaborRollupByDateNew> rollupData = this.GetODCResourceRollup(ODCType, odcTaskElement.taskElementLabors).ToCollection();
				IList<RollupSummaryByYearTableRowData> summaryRollupData = rollupData.Convert();

				RollupSummaryByYearTableData rollupTableData = new RollupSummaryByYearTableData
				{
					SummaryTotalComplete = summaryRollupData.Sum(s => s.YearTotal),
					YearlyData = summaryRollupData
				};

				PopulateRollupSummaryByYearTable(resourceTableElement, null, rollupTableData, DefaultCurrencyFormat, false);
			}
		}

		/// <summary>
		/// Gets the resource rollup data
		/// </summary>
		/// <param name="resourceDto">DTO containing the resource data</param>
		/// <param name="resourceElement">BOEExportTaskElementLabor with further resource data</param>
		/// <returns>Resource rollup data</returns>
		protected Collection<LaborRollupByDateNew> GetResourceRollupByYear(ResourceTypeDto resourceDto, BOEExportTaskElementLabor resourceElement)
		{
			if (resourceDto == null)
			{
				throw new ArgumentNullException(nameof(resourceDto));
			}

			if (resourceElement == null)
			{
				throw new ArgumentNullException(nameof(resourceElement));
			}

			Collection<LaborRollupByDateNew> rollupList = new Collection<LaborRollupByDateNew>();

			if (resourceDto.StartDate.HasValue && resourceDto.EndDate.HasValue)
			{
				for (int i = resourceDto.StartDate.Value.Year; i <= resourceDto.EndDate.Value.Year; i++)
				{
					LaborRollupByDateNew rollup = new LaborRollupByDateNew();

					rollup.Resource = resourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceName];

					rollup.Year = i;

					rollup.January = getResourceRollupForMonth(resourceDto, i, (int)Month.January);
					rollup.February = getResourceRollupForMonth(resourceDto, i, (int)Month.February);
					rollup.March = getResourceRollupForMonth(resourceDto, i, (int)Month.March);
					rollup.April = getResourceRollupForMonth(resourceDto, i, (int)Month.April);
					rollup.May = getResourceRollupForMonth(resourceDto, i, (int)Month.May);
					rollup.June = getResourceRollupForMonth(resourceDto, i, (int)Month.June);
					rollup.July = getResourceRollupForMonth(resourceDto, i, (int)Month.July);
					rollup.August = getResourceRollupForMonth(resourceDto, i, (int)Month.August);
					rollup.September = getResourceRollupForMonth(resourceDto, i, (int)Month.September);
					rollup.October = getResourceRollupForMonth(resourceDto, i, (int)Month.October);
					rollup.November = getResourceRollupForMonth(resourceDto, i, (int)Month.November);
					rollup.December = getResourceRollupForMonth(resourceDto, i, (int)Month.December);

					rollupList.Add(rollup);
				}
			}
			return rollupList;
		}

		/// <summary>
		/// Gets the resource rollup data for a particular month
		/// </summary>
		/// <param name="resource">Resource to get the rollup data for</param>
		/// <param name="year">Year of the rollup data</param>
		/// <param name="month">Month of the rollup data, as an int</param>
		/// <returns>resource rollup data for a particular month</returns>
		private decimal getResourceRollupForMonth(ResourceTypeDto resource, int year, int month)
		{
			return (from s in resource.LaborSpreads
					where s.LaborSpreadDate.Year == year && s.LaborSpreadDate.Month == month
					select s.LaborSpreadValue).Sum();
		}

		/// <summary>
		/// Gets the rollup data for an ODC resource
		/// </summary>
		/// <param name="odcType">ODC task data</param>
		/// <param name="labors">Labors for the ODC task</param>
		/// <returns>Rollup data for an ODC resource</returns>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private List<LaborRollupByDateNew> GetODCResourceRollup(OtherDirectCostType odcType, Collection<BOEExportTaskElementLabor> labors)
		{
			List<LaborRollupByDateNew> RollupByDate = new List<LaborRollupByDateNew>();

			if (odcType.StartDate.HasValue && odcType.EndDate.HasValue)
			{
				for (int i = odcType.StartDate.Value.Year; i <= odcType.EndDate.Value.Year; i++)
				{
					LaborRollupByDateNew Rollup = new LaborRollupByDateNew();

					Rollup.Resource = labors.First(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceID)
						&& int.Parse(c.ExportFields[BOEExporterConstants.FieldName_ResourceID]) == odcType.ResourceID.Value)
						.ExportFields[BOEExporterConstants.FieldName_ResourceName];

					Rollup.Year = i;

					Rollup.January = (from a in odcType.ODCSpreads
									  where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 1
									  select (decimal)a.CostSpreadValue).Sum() / 100m;

					Rollup.February = (from a in odcType.ODCSpreads
									   where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 2
									   select (decimal)a.CostSpreadValue).Sum() / 100m;

					Rollup.March = (from a in odcType.ODCSpreads
									where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 3
									select (decimal)a.CostSpreadValue).Sum() / 100m;

					Rollup.April = (from a in odcType.ODCSpreads
									where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 4
									select (decimal)a.CostSpreadValue).Sum() / 100m;

					Rollup.May = (from a in odcType.ODCSpreads
								  where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 5
								  select (decimal)a.CostSpreadValue).Sum() / 100m;

					Rollup.June = (from a in odcType.ODCSpreads
								   where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 6
								   select (decimal)a.CostSpreadValue).Sum() / 100m;

					Rollup.July = (from a in odcType.ODCSpreads
								   where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 7
								   select (decimal)a.CostSpreadValue).Sum() / 100m;

					Rollup.August = (from a in odcType.ODCSpreads
									 where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 8
									 select (decimal)a.CostSpreadValue).Sum() / 100m;

					Rollup.September = (from a in odcType.ODCSpreads
										where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 9
										select (decimal)a.CostSpreadValue).Sum() / 100m;

					Rollup.October = (from a in odcType.ODCSpreads
									  where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 10
									  select (decimal)a.CostSpreadValue).Sum() / 100m;

					Rollup.November = (from a in odcType.ODCSpreads
									   where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 11
									   select (decimal)a.CostSpreadValue).Sum() / 100m;

					Rollup.December = (from a in odcType.ODCSpreads
									   where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 12
									   select (decimal)a.CostSpreadValue).Sum() / 100m;

					RollupByDate.Add(Rollup);
				}
			}
			return RollupByDate;
		}

		#endregion

		#region Non-labor task direct cost rollup tables

		/// <summary>
		/// Processes the travel task direct cost rollup table.
		/// </summary>
		/// <param name="containerElement">The container element.</param>
		/// <param name="travelTaskElement">The travel task element.</param>
		/// <param name="travelResources">The travel resources.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="useGfy">use government fiscal year?</param>
		/// <exception cref="ArgumentNullException">
		/// selectedComponents
		/// or
		/// exportInputs
		/// </exception>
		protected virtual void ProcessTravelTaskDirectCostRollupTable(StructuredDocumentTag containerElement, BOEExportTaskElement travelTaskElement, Collection<ResourceDTO> travelResources,
			ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs, bool useGfy)
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

			if (travelCostRollupTableElement != null && selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables))
			{
				TravelDTO currentTravelTaskElement = exportInputs.Travels.First(x => x.Id == travelTaskElement.BOETaskElementID.Value);
				Dictionary<int, List<LaborRollupByDateNew>> currentTravelTaskRollupData = GetTravelCostRollup(new Collection<TravelDTO> { currentTravelTaskElement }, new Collection<BOEExportTaskElement> { travelTaskElement }, travelResources, useGfy);
				RollupSummaryByGroupByYearTableData travelSummaryRollupData = currentTravelTaskRollupData.Convert();
				PopulateRollupSummaryByGroupByYearTable(travelCostRollupTableElement, null, travelSummaryRollupData, DefaultCurrencyFormat, false);
			}
			else
			{
				this.RemoveElement(travelCostRollupTableElement);
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
		protected virtual void ProcessODCTaskDirectCostRollupTable(StructuredDocumentTag containerElement, BOEExportTaskElement odcTaskElement, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs, bool useGfy)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			#region ODC Direct Cost Rollup Table

			StructuredDocumentTag odcCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement,
				useGfy ? BOEExporterConstants.Table_GfyDirectCostRollup : BOEExporterConstants.Table_DirectCostRollup);

			if (odcCostRollupTableElement == null)
			{
			}
			else if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables))
			{
				OtherDirectCostDTO currentODCTaskElement = exportInputs.Odcs.First(x => x.Id == odcTaskElement.BOETaskElementID.Value);
				Dictionary<int, List<LaborRollupByDateNew>> currentODCTaskRollupData = this.GetODCCostRollup(new Collection<OtherDirectCostDTO> { currentODCTaskElement }, odcTaskElement.taskElementLabors, useGfy);
				RollupSummaryByGroupByYearTableData odcSummaryRollupData = currentODCTaskRollupData.Convert();
				PopulateRollupSummaryByGroupByYearTable(odcCostRollupTableElement, null, odcSummaryRollupData, DefaultCurrencyFormat, false);
			}
			else
			{
				this.RemoveElement(odcCostRollupTableElement);
			}

			#endregion
		}

		/// <summary>
		/// Processes the material task direct cost rollup table.
		/// </summary>
		/// <param name="containerElement">The container element.</param>
		/// <param name="materialTaskElement">The material task element.</param>
		/// <param name="selectedComponents">The selected components.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="useGfy">use government fiscal year?</param>
		/// <exception cref="ArgumentNullException">
		/// selectedComponents
		/// or
		/// exportInputs
		/// </exception>
		protected virtual void ProcessMaterialTaskDirectCostRollupTable(StructuredDocumentTag containerElement, BOEExportTaskElement materialTaskElement, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs, bool useGfy)
		{
			if (selectedComponents == null)
			{
				throw new ArgumentNullException(nameof(selectedComponents));
			}

			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			#region Material Direct Cost Rollup Table

			StructuredDocumentTag materialCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement,
				useGfy ? BOEExporterConstants.Table_GfyDirectCostRollup : BOEExporterConstants.Table_DirectCostRollup);

			bool byQuarter = false;
			if (useGfy && materialCostRollupTableElement == null)
			{
				materialCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyDirectCostRollupByQuarter);
				byQuarter = true;
			}

			if (materialCostRollupTableElement != null && selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables))
			{
				Dictionary<int, List<LaborRollupByDateNew>> currentMaterialTaskRollupData = new Dictionary<int, List<LaborRollupByDateNew>>();
				RollupSummaryByGroupByYearTableData materialSummaryRollupData = currentMaterialTaskRollupData.Convert();
				PopulateRollupSummaryByGroupByYearTable(materialCostRollupTableElement, null, materialSummaryRollupData, DefaultCurrencyFormat, byQuarter, useGfy);
			}
			else
			{
				this.RemoveElement(materialCostRollupTableElement);
			}
			#endregion
		}

		#endregion

		#endregion

		#region Data population helper methods

		/// <summary>
		/// Derive a lookup table of custom field names and values for each of the task's resource entries.
		/// </summary>
		/// <param name="customFieldValueIdMappings">Maps each resource entry (labor type id) to its custom field assignments</param>
		/// <param name="workspaceCustomFields">All custom fields</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns>
		/// Table of custom field names and values, indexed by labor type id
		/// </returns>
		private IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> GetTaskElementCustomFields(Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueIdMappings,
			IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields, BOEExportInputs exportInputs)
		{
			IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> taskElementCustomFields = new Dictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>>();

			foreach (KeyValuePair<int, ICollection<KeyValuePair<int, int>>> laborTaskCustomFieldIdMapping in customFieldValueIdMappings)
			{
				int laborTypeId = laborTaskCustomFieldIdMapping.Key;
				ICollection<KeyValuePair<int, int>> idPairs = laborTaskCustomFieldIdMapping.Value;

				// Get all custom field values for the task element.
				ICollection<int> customFieldValueIds = idPairs.Select(i => i.Value).Distinct().ToCollection<int>();
				ICollection<CustomFieldValueDTO> customFieldValues = exportInputs.CustomFieldValues.Where(x => customFieldValueIds.Contains(x.CustomFieldValueID)).ToList();

				foreach (CustomFieldValueDTO customFieldValueDto in customFieldValues.OrderBy(x => x.CustomFieldID))
				{
					CustomFieldDTO customFieldDto = workspaceCustomFields.FirstOrDefault(f => f.Id == customFieldValueDto.CustomFieldID);
					if (customFieldDto != null)
					{
						IDictionary<CustomFieldValueDTO, CustomFieldDTO> currentLaborTypeCustomFields;
						if (taskElementCustomFields.ContainsKey(laborTypeId))
						{
							currentLaborTypeCustomFields = taskElementCustomFields[laborTypeId];
						}
						else
						{
							currentLaborTypeCustomFields = new Dictionary<CustomFieldValueDTO, CustomFieldDTO>();
							taskElementCustomFields.Add(laborTypeId, currentLaborTypeCustomFields);
						}

						// add to list
						currentLaborTypeCustomFields.Add(customFieldValueDto, customFieldDto);
					}
				}
			}

			return taskElementCustomFields;
		}

		/// <summary>
		/// Gets resource-level custom fields
		/// </summary>
		/// <param name="customFieldValueIdMappings">Mappings for the custom field value ids</param>
		/// <param name="workspaceCustomFields">All custom fields in the workspace</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns></returns>
		private IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> GetResourceCustomFields(Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueIdMappings,
			IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields, BOEExportInputs exportInputs)
		{
			IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> resourceCustomFields = new Dictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>>();

			foreach (KeyValuePair<int, ICollection<KeyValuePair<int, int>>> resourceCustomFieldIdMapping in customFieldValueIdMappings)
			{
				int resourceId = resourceCustomFieldIdMapping.Key;
				ICollection<KeyValuePair<int, int>> idPairs = resourceCustomFieldIdMapping.Value;

				// Get all custom field values for the task element.
				ICollection<int> customFieldValueIds = idPairs.Select(i => i.Value).Distinct().ToCollection<int>();
				ICollection<CustomFieldValueDTO> customFieldValues = exportInputs.CustomFieldValues.Where(x => customFieldValueIds.Contains(x.CustomFieldValueID)).ToList();

				foreach (CustomFieldValueDTO customFieldValueDto in customFieldValues.OrderBy(x => x.CustomFieldID))
				{
					CustomFieldDTO customFieldDto = workspaceCustomFields.FirstOrDefault(f => f.Id == customFieldValueDto.CustomFieldID);
					if (customFieldDto != null)
					{
						IDictionary<CustomFieldValueDTO, CustomFieldDTO> currentResourceCustomFields;
						if (resourceCustomFields.ContainsKey(resourceId))
						{
							currentResourceCustomFields = resourceCustomFields[resourceId];
						}
						else
						{
							currentResourceCustomFields = new Dictionary<CustomFieldValueDTO, CustomFieldDTO>();
							resourceCustomFields.Add(resourceId, currentResourceCustomFields);
						}

						// add to list
						currentResourceCustomFields.Add(customFieldValueDto, customFieldDto);
					}
				}
			}

			return resourceCustomFields;
		}

		/// <summary>
		/// Sets the title for the table
		/// </summary>
		/// <param name="containerElement">table container element</param>
		/// <param name="tableTitle">title to name the table</param>
		protected void SetTableTitle(StructuredDocumentTag containerElement, string tableTitle)
		{
			/*
             * 
             *  SdtBlock                                                <== (containerElement)
             *      SdtProperties
             *          Tag = "LaborHoursSummaryByDateTable"
             *      SdtContentBlock                                     <== (tableContainerElement)
             *          Paragraph
             *          ------ case 1 ------ [e.g. Labor task subtables]
             *          SdtBlock                                        <== (tableTitleContainerElement:1)
             *              SdtProperties
             *                  Tag = "TableTitle"
             *              SdtContentBlock
             *                  Paragraph
             *                      Run (Nx)
             *                          Text = "Subcontractor Labor Cost Summary"
             *          ------ case 2 ------ [e.g. BOE-level summary tables]
             *          Paragraph
             *              SdtRun                                      <== (tableTitleContainerElement:2)
             *                  SdtProperties
             *                      Tag = TableTitle
             *                  SdtContentRun
             *                      Run (Nx)
             *                          Text = "Labor Hours Summary"
             *          (Paragraph)
             *          Table                                           <==  (tableElement)
             *          Paragraph
             * 
             */

			// locate the corresponding table element (and the parent that contains it)
			CompositeNode tableContainerElement = containerElement;
			Table tableElement = null;
			// TODO TIW check while loop
			while (tableContainerElement != null && (tableElement = tableContainerElement.GetChild(NodeType.Table, 0, true) as Table) == null)
			{
				tableContainerElement = tableContainerElement.ParentNode;
			}

			// locate the table-title container element
			StructuredDocumentTag tableTitleContainerElement;
			if ((tableTitleContainerElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_TableTitle)) == null)
			{
				tableTitleContainerElement = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.FieldName_TableTitle);
			}

			// assign the text value for the table title
			if (tableTitle != null && tableTitleContainerElement != null)
			{
				WordUtilities.SetElementText(tableTitleContainerElement, tableTitle);
			}

			// then move it to just before the table itself
			if (tableElement != null && tableTitleContainerElement != null)
			{
				CompositeNode tableTitleContainerParentElement = tableTitleContainerElement.ParentNode;

				if (tableTitleContainerElement is SdtRun && tableTitleContainerParentElement is Paragraph)
				{
					return;
				}
				else
				{
					tableTitleContainerParentElement.RemoveChild(tableTitleContainerElement);
					Node previousSibling;
					if ((previousSibling = tableElement.PreviousSibling) != null)
					{
						previousSibling.ParentNode.InsertAfter(tableTitleContainerElement, previousSibling);
					}
					else
					{
						tableElement.ParentNode.AppendChild(tableTitleContainerElement);
					}
				}
			}
		}

		#endregion

		#region General

		/// <summary>
		/// Clones the template for a container element
		/// </summary>
		/// <param name="containerElement">container element to be cloned</param>
		/// <returns></returns>
		protected StructuredDocumentTag CloneContainerTemplate(StructuredDocumentTag containerElement)
		{
			if (containerElement == null)
			{
				throw new ArgumentNullException(nameof(containerElement));
			}

			StructuredDocumentTag clonedContainerElement = containerElement.Clone(true) as StructuredDocumentTag;

			#region Delete IDs to avoid conflict with existing elements
			// TIW TODO 
			// clonedContainerElement.Id = 0;
			clonedContainerElement.Placeholder.Remove();

			#endregion

			return clonedContainerElement;
		}

		/// <summary>
		/// Populates the table row with monthly data.
		/// </summary>
		/// <param name="tableRow">The table row.</param>
		/// <param name="monthlyValues">The monthly values.</param>
		/// <param name="numericFormat">The numeric format.</param>
		private void PopulateTableRowWithMonthlyData(Row tableRow, ValuesByMonth<decimal> monthlyValues, string numericFormat)
		{
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Jan), numericFormat == null ? monthlyValues.January.ToString() : monthlyValues.January.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Feb), numericFormat == null ? monthlyValues.February.ToString() : monthlyValues.February.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Mar), numericFormat == null ? monthlyValues.March.ToString() : monthlyValues.March.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Apr), numericFormat == null ? monthlyValues.April.ToString() : monthlyValues.April.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_May), numericFormat == null ? monthlyValues.May.ToString() : monthlyValues.May.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Jun), numericFormat == null ? monthlyValues.June.ToString() : monthlyValues.June.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Jul), numericFormat == null ? monthlyValues.July.ToString() : monthlyValues.July.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Aug), numericFormat == null ? monthlyValues.August.ToString() : monthlyValues.August.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Sep), numericFormat == null ? monthlyValues.September.ToString() : monthlyValues.September.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Oct), numericFormat == null ? monthlyValues.October.ToString() : monthlyValues.October.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Nov), numericFormat == null ? monthlyValues.November.ToString() : monthlyValues.November.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Dec), numericFormat == null ? monthlyValues.December.ToString() : monthlyValues.December.ToString(numericFormat));
		}

		/// <summary>
		/// Populates the table row with the data for each quarter
		/// </summary>
		/// <param name="tableRow">Row to be populated</param>
		/// <param name="monthlyValues">Values for each month</param>
		/// <param name="numericFormat">format for numbers</param>
		/// <param name="useGfy">if using Government Fiscal Year</param>
		private void PopulateTableRowWithQuarterlyData(Row tableRow, ValuesByMonth<decimal> monthlyValues, string numericFormat, bool useGfy)
		{
			//get quarter values by adding up monthly values
			decimal Q1 = monthlyValues.January + monthlyValues.February + monthlyValues.March;
			decimal Q2 = monthlyValues.April + monthlyValues.May + monthlyValues.June;
			decimal Q3 = monthlyValues.July + monthlyValues.August + monthlyValues.September;
			decimal Q4 = monthlyValues.October + monthlyValues.November + monthlyValues.December;

			// Adjust if using Govt Fiscal Year
			if (useGfy)
			{
				decimal temp = Q4;
				Q4 = Q3;
				Q3 = Q2;
				Q2 = Q1;
				Q1 = temp;
			}

			//populate the fields
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Q1), numericFormat == null ? Q1.ToString() : Q1.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Q2), numericFormat == null ? Q2.ToString() : Q2.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Q3), numericFormat == null ? Q3.ToString() : Q3.ToString(numericFormat));
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Q4), numericFormat == null ? Q4.ToString() : Q4.ToString(numericFormat));
		}

		/// <summary>
		/// Populate rollup summary tables that are grouped by item, by year
		/// </summary>
		/// <param name="tableContainerElement">The table container element.</param>
		/// <param name="tableTitle">The table title.</param>
		/// <param name="data">The data.</param>
		/// <param name="numericFormat">The numeric format.</param>
		/// <param name="byQuarter">if set to <c>true</c> [by quarter].</param>
		/// <param name="useGfy">if using Government Fiscal Year</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">data</exception>
		/// <remarks>
		/// DirectCostRollupTable
		/// LaborHoursRollupTable
		/// BOEHoursSummaryTable
		/// BOECostSummaryTable
		/// </remarks>
		protected bool PopulateRollupSummaryByGroupByYearTable(StructuredDocumentTag tableContainerElement, string tableTitle, RollupSummaryByGroupByYearTableData data, string numericFormat, bool byQuarter, bool useGfy = false)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			bool populated;

			if (data.GroupData != null && data.GroupData.Any() && tableContainerElement != null)
			{
				/*
                 * SdtBlock (tag = "BOECostSummaryTable")
                 *     SdtContentBlock
                 *         Table
                 *             Row - Resource, Year, Jan ... Total
                 *             TableRow
                 *                 TableCell
                 *                     Paragraph
                 *                         SdtRun (tag = "Marker-DataRow")
                 *             TableRow
                 *                 TableCell
                 *                     Paragraph
                 *                         SdtRun (tag = "Marker-TotalsRow")
                 *             TableRow
                 *                 TableCell (2 cells only)
                 *             TableRow
                 *                 TableCell
                 *                     Paragraph
                 *                         SdtRun (tag = "Marker-SummaryDataRow")
                 *             TableRow
                 *                 TableCell
                 *                     Paragraph
                 *                         SdtRun (tag = "Marker-SummaryTotalsRow")
                 */

				SetTableTitle(tableContainerElement, tableTitle);

				// locate the table markers
				StructuredDocumentTag dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_DataRow);
				Row templateDataRow = dataRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(templateDataRow);

				StructuredDocumentTag dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_TotalsRow);
				Row templateTotalsRow = dataTotalsRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(templateTotalsRow);

				StructuredDocumentTag summaryDataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_SummaryDataRow);
				Row templateSummaryDataRow = summaryDataRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(templateSummaryDataRow);

				StructuredDocumentTag summaryTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_SummaryTotalsRow);
				Row templateSummaryTotalsRow = summaryTotalsRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(templateSummaryTotalsRow);

				// initialize the "insertion" row
				Row currentInsertionRow = templateDataRow;

				foreach (RollupSummaryByGroupByYearTableGroupData group in data.GroupData)
				{
					currentInsertionRow = SummaryRowHelper(numericFormat, byQuarter, templateDataRow, currentInsertionRow, group.YearlyData, group.GroupName, useGfy);

					// create a new total row for this group
					Row totalRow = this.CloneMarkedTemplateRow(templateTotalsRow);

					// populate the sub-total (for the group)
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalRow, BOEExporterConstants.FieldName_Total), numericFormat == null ? group.GroupSubTotal.ToString() : group.GroupSubTotal.ToString(numericFormat));

					// add the row to the table
					currentInsertionRow.ParentNode.InsertAfter(totalRow, currentInsertionRow);
					currentInsertionRow = totalRow;
				}

				int rowNumber = 0;

				foreach (RollupSummaryByYearTableRowData rowData in data.SummaryData)
				{
					++rowNumber;

					// create a new summary data row in the table
					Row tableRow = this.CloneMarkedTemplateRow(templateSummaryDataRow);

					// populate the row
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Year), rowData.Year);
					if (byQuarter)
					{
						PopulateTableRowWithQuarterlyData(tableRow, rowData.MonthlyValues, numericFormat, useGfy);
					}
					else
					{
						PopulateTableRowWithMonthlyData(tableRow, rowData.MonthlyValues, numericFormat);
					}
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Total), numericFormat == null ? rowData.YearTotal.ToString() : rowData.YearTotal.ToString(numericFormat));

					// remove the "Summary" label on all but the first row
					if (rowNumber > 1)
					{
						// TableRow
						//     TableCell
						//         Paragraph
						//             Run
						//                 Text
						//             Run
						//                 Text
						//             SdtRun
						//                 SdtProperties
						//                     Tag = Marker

						// remove the "Summary" label
						StructuredDocumentTag currRowSummaryDataTag = WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.Marker_SummaryDataRow);
						Paragraph paragraphElement = currRowSummaryDataTag.GetAncestor(NodeType.Paragraph) as Paragraph;
						ICollection<Run> childRunElements = paragraphElement.Runs.ToArray();
						foreach (Node childRun in childRunElements)
						{
							this.RemoveElement(childRun);
						}

						// remove the top border if it exists
						Cell firstCell = currRowSummaryDataTag.GetAncestor(NodeType.Cell) as Cell;
						Border top = firstCell.CellFormat?.Borders?.Top;
						if (top != null)
						{
							top.LineStyle = LineStyle.None;
						}
					}

					// add the row to the table
					currentInsertionRow.ParentNode.InsertAfter(tableRow, currentInsertionRow);
					currentInsertionRow = tableRow;
				}

				// create a new summary total row
				Row summaryTotalRow = this.CloneMarkedTemplateRow(templateSummaryTotalsRow);

				// populate the summary total
				if (byQuarter)
				{
					PopulateTableRowWithQuarterlyData(summaryTotalRow, data.SummaryTotalsByMonth, numericFormat, useGfy);
				}
				else
				{
					PopulateTableRowWithMonthlyData(summaryTotalRow, data.SummaryTotalsByMonth, numericFormat);
				}
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(summaryTotalRow, BOEExporterConstants.FieldName_Total), numericFormat == null ? data.SummaryTotalComplete.ToString() : data.SummaryTotalComplete.ToString(numericFormat));

				// add the row to the table
				currentInsertionRow.ParentNode.InsertAfter(summaryTotalRow, currentInsertionRow);

				// remove template rows
				this.RemoveElement(templateDataRow);
				this.RemoveElement(templateTotalsRow);
				this.RemoveElement(templateSummaryDataRow);
				this.RemoveElement(templateSummaryTotalsRow);

				populated = true;
			}
			else
			{
				this.RemoveElement(tableContainerElement);

				populated = false;
			}

			return populated;
		}

		/// <summary>
		/// Method to populate the summary row
		/// </summary>
		/// <param name="numericFormat">Numeric format</param>
		/// <param name="byQuarter">If table is by quarter</param>
		/// <param name="templateDataRow">template data row</param>
		/// <param name="currentInsertionRow">current insertion row</param>
		/// <param name="data">data</param>
		/// <param name="resource">resource</param>
		/// <param name="useGfy">if using Government Fiscal Year</param>
		/// <returns>the current insertion row</returns>
		private Row SummaryRowHelper(string numericFormat, bool byQuarter, Row templateDataRow, Row currentInsertionRow, ICollection<RollupSummaryByYearTableRowData> data, string resource = null, bool useGfy = false)
		{
			foreach (RollupSummaryByYearTableRowData rowData in data)
			{
				// create a new data row in the table
				Row tableRow = this.CloneMarkedTemplateRow(templateDataRow);

				// populate the row
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Resource), resource ?? string.Empty);
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceDescription), resource ?? string.Empty);

				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Year), rowData.Year);
				if (byQuarter)
				{
					PopulateTableRowWithQuarterlyData(tableRow, rowData.MonthlyValues, numericFormat, useGfy);
				}
				else
				{
					PopulateTableRowWithMonthlyData(tableRow, rowData.MonthlyValues, numericFormat);
				}
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Total), numericFormat == null ? rowData.YearTotal.ToString() : rowData.YearTotal.ToString(numericFormat));

				// add the row to the table
				currentInsertionRow.ParentNode.InsertAfter(tableRow, currentInsertionRow);
				currentInsertionRow = tableRow;
			}

			return currentInsertionRow;
		}

		/// <summary>
		/// Populate rollup tables that are summarized by year
		/// </summary>
		/// <param name="tableContainerElement">The table container element.</param>
		/// <param name="tableTitle">The table title.</param>
		/// <param name="data">The data.</param>
		/// <param name="numericFormat">The numeric format.</param>
		/// <param name="byQuarter">if set to <c>true</c> [by quarter].</param>
		/// <param name="useGfy">if using Government Fiscal Year for the table</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">data</exception>
		/// <remarks>
		/// LaborHoursSummaryByDateTable
		/// NonLaborHoursSummaryByDateTable
		/// LaborCostSummaryByDateTable
		/// NonLaborCostSummaryByDateTable
		/// LaborAndNonLaborCostSummaryByDateTable
		/// LaborHoursSummaryByCustomFieldTable
		/// </remarks>
		protected virtual bool PopulateRollupSummaryByYearTable(StructuredDocumentTag tableContainerElement, string tableTitle, RollupSummaryByYearTableData data, string numericFormat, bool byQuarter, bool useGfy = false)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			bool populated;

			if (data.YearlyData != null && data.YearlyData.Any())
			{
				SetTableTitle(tableContainerElement, tableTitle);

				// one row for each year (monthly values and total for year), plus one overall total row

				// populate the overall total - can just use the existing total row (it does not need to be cloned)
				StructuredDocumentTag dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_TotalsRow);
				if (dataTotalsRowMarkerTag != null)
				{
					Row overallTotalRow = dataTotalsRowMarkerTag.GetAncestor(NodeType.Row) as Row;
					this.SetCantSplit(overallTotalRow);
					StructuredDocumentTag overallTotalCellRun = WordUtilities.GetTaggedChildElement(overallTotalRow, BOEExporterConstants.FieldName_Total);
					WordUtilities.SetElementText(overallTotalCellRun, numericFormat == null ? data.SummaryTotalComplete.ToString() : data.SummaryTotalComplete.ToString(numericFormat));
					// remove placeholder (band-aid)
					overallTotalCellRun.Placeholder.Remove();
				}

				// locate the table markers
				StructuredDocumentTag dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_DataRow);

				// initialize the "insertion" row
				Row templateDataRow = dataRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(templateDataRow);
				Row currentInsertionRow = templateDataRow;

				currentInsertionRow = SummaryRowHelper(numericFormat, byQuarter, templateDataRow, currentInsertionRow, data.YearlyData, null, useGfy);

				// remove template rows
				templateDataRow.Remove();

				populated = true;
			}
			else
			{
				this.RemoveElement(tableContainerElement);
				populated = false;
			}

			return populated;
		}

		#endregion

		#region Resource Types tables

		/// <summary>
		/// Populate performing organizations tables
		/// </summary>
		/// <remarks>
		/// ResourceTypesTable
		/// </remarks>
		private void PopulateResourceTypesTable<T>(StructuredDocumentTag tableContainerElement, ICollection<T> resourcesData, Action<Row, T> populateDataRow)
			where T : ResourceTypesTableRowData
		{
			if (resourcesData.Any())
			{
				// different columns for both data and totals rows - logic will check for existence of tags to determine how to populate

				// locate the table markers
				StructuredDocumentTag dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_DataRow);

				// initialize the "insertion" row
				Row templateDataRow = dataRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(templateDataRow);
				Row currentInsertionRow = templateDataRow;

				foreach (T rowData in resourcesData)
				{
					// create a new data row in the table
					Row tableRow = this.CloneMarkedTemplateRow(templateDataRow);

					// populate the row with the "base" row data
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_PerformingOrgID), rowData.PerformingOrgID);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_PerformingOrg), rowData.PerformingOrgName);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_PerformingOrgDescription), rowData.PerformingOrgDescription);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceID), rowData.ResourceID);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceName), rowData.ResourceName);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceDescription), rowData.ResourceDescription);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_SegmentRegion), rowData.SegmentRegion);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceType), rowData.ResourceType);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Segment), rowData.Segment);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Cost), rowData.Cost.ToString(DefaultCurrencyFormat));

					// populate the row with the additional (task-specific) row data
					populateDataRow(tableRow, rowData);

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

		private void PopulateLaborHoursSummaryByCustomFieldTable(StructuredDocumentTag tableContainerElement, ICollection<BoeTaskElementDTO> taskElementCollection, string summarizeByCustomField, string customFieldValue, LaborResourceTypesTableData tableData)
		{
			if (tableContainerElement != null && tableData != null && tableData.ResourcesData.Any())
			{
				ICollection<int> laborTypeIds = tableData.ResourcesData.Select(x => int.Parse(x.LaborTypeID)).ToCollection();

				// compile the rollup data
				List<LaborRollupByDateNew> laborRollupData =
					this.GetLaborRollupByYear(taskElementCollection, laborTypeIds, RateType.Hours);
				IList<RollupSummaryByYearTableRowData> laborHoursSummaryRollupData = laborRollupData.Convert();

				RollupSummaryByYearTableData laborRollupTableData = new RollupSummaryByYearTableData
				{
					SummaryTotalComplete = laborHoursSummaryRollupData.Sum(d => d.YearTotal),
					YearlyData = laborHoursSummaryRollupData
				};

				PopulateRollupSummaryByYearTable(tableContainerElement, summarizeByCustomField + ": " + customFieldValue, laborRollupTableData, DefaultHoursFormat, false);
			}
		}

		private void PopulateResourceTypesTable(StructuredDocumentTag tableContainerElement, LaborResourceTypesTableData data)
		{
			if (data.ResourcesData.Any())
			{
				PopulateResourceTypesTable(tableContainerElement, data.ResourcesData, PopulateLaborResourceTypesTableRow);

				// can just use the existing total row (it does not need to be cloned)
				StructuredDocumentTag dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_TotalsRow);
				Row totalsRow = dataTotalsRowMarkerTag.GetAncestor(NodeType.Row) as Row;

				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_CostTotal), data.CostTotalComplete.ToString(DefaultCurrencyFormat));
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_HoursTotal), data.HoursTotalComplete.ToString(DefaultHoursFormat));

				LaborResourceTypesTableRowData nonzero = data.ResourcesData.FirstOrDefault(x => x.Cost != 0);
				//if there is no cost, remove notice saying costs are rounded since it is not needed
				if (nonzero == null)
				{
					StructuredDocumentTag roundingNotice = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.FieldName_RoundingNotice);
					this.RemoveElement(roundingNotice); //no need to check null since RemoveElement already does this

					StructuredDocumentTag roundingNoticeAsterisk = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.FieldName_RoundingNoticeAsterisk);
					this.RemoveElement(roundingNoticeAsterisk);
				}
			}
			else
			{
				this.RemoveElement(tableContainerElement);
			}
		}

		private void PopulateLaborResourceTypesTableRow(Row tableRow, LaborResourceTypesTableRowData rowData)
		{
			#region Custom Fields

			/*
             * SdtBlock
             *     SdtProperties
             *         Tag = ResourceTypesTable
             *     SdtContentBlock
             *         Table
             *             TableRow
             *                 TableCell
             *                     StdBlock             <==  [customFieldDescriptionBlock]
             *                         SdtProperties
             *                             Tag = CustomFieldDescription
             *                         SdtContentBlock      <==  [contentBlock]
             *                             Paragraph            <==  [paragraphTemplate]
             *                                 Run
             *                                     Text
             *                                 Run
             *                                     Text
             *                             Paragraph
             *                                 Run
             *                                     Text
             */

			StructuredDocumentTag customFieldDescriptionBlock = WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_CustomFieldDescription);

			//The column may have been deleted if there are no custom fields at the resource level
			if (customFieldDescriptionBlock != null)
			{
				SdtContentBlock contentBlock = customFieldDescriptionBlock.GetFirstChild<SdtContentBlock>();
				RemoveAllButOneElement(contentBlock, NodeType.Paragraph);
				Paragraph paragraphTemplate = contentBlock.GetFirstChild<Paragraph>();

				if (rowData.CustomFields.Any())
				{
					Paragraph currentInsertionElement = paragraphTemplate;

					foreach (ExportCustomField customField in rowData.CustomFields)
					{
						Paragraph nextCustomFieldElement = paragraphTemplate.Clone(true) as Paragraph;

						currentInsertionElement.ParentNode.InsertAfter(nextCustomFieldElement, currentInsertionElement);
						currentInsertionElement = nextCustomFieldElement;

						string customFieldString = customField.IsOpenEnded ?
							$"{customField.CustomFieldName}: {customField.CustomFieldValueDescription}" :
							$"{customField.CustomFieldName}: {customField.CustomFieldValueName} - {customField.CustomFieldValueDescription}";

						WordUtilities.SetElementText(nextCustomFieldElement, customFieldString);
					}

					paragraphTemplate.Remove();
				}
				else
				{   //There may be custom fields at the resource level, but none selected for this resource, so column will still print
					// cannot leave the content block empty - remove paragraph CONTENTS, but NOT the paragraph itself
					paragraphTemplate.RemoveAllChildren();
				}
			}


			#endregion

			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Hours), rowData.Hours);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ElementOfCost), rowData.ElementOfCost);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_SpreadCurve), rowData.SpreadCurve);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceWBS), rowData.WbsString);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceCLIN), rowData.ClinString);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_SummaryReference), rowData.SummaryReference);
		}

		private void PopulateResourceTypesTable(StructuredDocumentTag tableContainerElement, ODCResourceTypesTableData data)
		{
			if (data.ResourcesData.Any())
			{
				PopulateResourceTypesTable(tableContainerElement, data.ResourcesData, PopulateODCResourceTypesTableRow);

				// can just use the existing total row (it does not need to be cloned)
				StructuredDocumentTag dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_TotalsRow);
				Row totalsRow = dataTotalsRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(totalsRow);

				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_CostTotal), data.CostTotalComplete.ToString(DefaultCurrencyFormat));
			}
			else
			{
				this.RemoveElement(tableContainerElement);
			}
		}

		private void PopulateODCResourceTypesTableRow(Row tableRow, ODCResourceTypesTableRowData rowData)
		{
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ElementOfCost), rowData.ElementOfCost);
		}

		private void PopulateResourceTypesTable(StructuredDocumentTag tableContainerElement, TravelResourceTypesTableData data)
		{
			if (data.ResourcesData.Any())
			{
				PopulateResourceTypesTable(tableContainerElement, data.ResourcesData, PopulateTravelResourceTypesTableRow);

				// can just use the existing total row (it does not need to be cloned)
				StructuredDocumentTag dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_TotalsRow);
				Row totalsRow = dataTotalsRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(totalsRow);

				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_CostTotal), data.CostTotalComplete.ToString(DefaultCurrencyFormat));
			}
			else
			{
				this.RemoveElement(tableContainerElement);
			}
		}

		private void PopulateTravelResourceTypesTableRow(Row tableRow, TravelResourceTypesTableRowData rowData)
		{
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Mode), rowData.Mode);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Departure), rowData.Departure);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Destination), rowData.Destination);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ElementOfCost), rowData.ElementOfCost);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Purpose), rowData.Purpose);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Trips), rowData.Trips);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_People), rowData.People);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Days), rowData.Days);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Date), rowData.Date.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR));
		}

		private void PopulateResourceTypesTable(StructuredDocumentTag tableContainerElement, MaterialResourceTypesTableData data)
		{
			if (data.ResourcesData.Any())
			{
				PopulateResourceTypesTable(tableContainerElement, data.ResourcesData, PopulateMaterialResourceTypesTableRow);

				// can just use the existing total row (it does not need to be cloned)
				StructuredDocumentTag dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_TotalsRow);
				Row totalsRow = dataTotalsRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(totalsRow);

				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_CostTotal), data.CostTotalComplete.ToString(DefaultCurrencyFormat));
			}
			else
			{
				this.RemoveElement(tableContainerElement);
			}
		}

		private void PopulateMaterialResourceTypesTableRow(Row tableRow, MaterialResourceTypesTableRowData rowData)
		{
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ElementOfCost), ElementOfCostType.Materials.GetDescription());
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_EndDate), rowData.ExpendDate.ToString());
		}

		#endregion

		#region Resource Summary tables

		protected virtual void PopulateResourceSummaryByElementOfCostTable(StructuredDocumentTag tableContainerElement, BOEExportModelView boeExportModelView, ICollection<BOESummaryGridModelView> data)
		{
			if (data.Any())
			{
				// Category ==> Element of Cost

				// derive the rollup data
				ICollection<ResourceSummaryRowData> rollupData =
					data.Where(s => s.TotalCost.HasValue)
						.GroupBy(x => x.Category)
						.Select(g => new ResourceSummaryRowData
						{
							ResourceType = g.Key.ToString(),
							CostTotal = g.Sum(x => x.TotalCost),
							HoursTotal = g.Sum(x => x.TotalHours)
						})
						.OrderBy(x => x.ResourceType).ToList();

				PopulateResourceSummaryTable(tableContainerElement, rollupData);
			}
			else
			{
				this.RemoveElement(tableContainerElement);
			}
		}

		private void PopulateResourceSummaryByResourceTypeTable(StructuredDocumentTag tableContainerElement, ICollection<BOESummaryGridModelView> data)
		{
			if (data.Any())
			{
				// LaborType ==> Resource Type

				// derive the rollup data
				ICollection<ResourceSummaryRowData> rollupData =
					data.Where(s => s.TotalCost.HasValue)
						.GroupBy(x => x.LaborType ?? string.Empty)
						.Select(g => new ResourceSummaryRowData
						{
							ResourceType = g.Key.ToString(),
							CostTotal = g.Sum(x => x.TotalCost),
							HoursTotal = g.Sum(x => x.TotalHours)
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
		/// Populate the Resource Summary by Resource ID Table
		/// </summary>
		/// <param name="tableContainerElement">container element for the table</param>
		/// <param name="boeExportModelView">Model View for the BOE Export with the resources to use</param>
		protected virtual void PopulateResourceSummaryByResourceIDTable(StructuredDocumentTag tableContainerElement, BOEExportModelView boeExportModelView)
		{
			if (boeExportModelView != null)
			{
				ICollection<ResourceSummaryRowData> resourceData = boeExportModelView.TaskElements.SelectMany(t => t.taskElementLabors).Select(r => new ResourceSummaryRowData
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
		protected virtual void PopulateResourceSummaryTable(StructuredDocumentTag tableContainerElement, ICollection<ResourceSummaryRowData> rollupData)
		{
			if (rollupData != null && rollupData.Any())
			{
				// locate the table markers
				StructuredDocumentTag dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_DataRow);
				StructuredDocumentTag dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_TotalsRow);

				// initialize the "insertion" row
				Row templateDataRow = dataRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(templateDataRow);
				Row currentInsertionRow = templateDataRow;


				// accumulate totals
				decimal costTotal = 0m;
				decimal hoursTotal = 0m;

				bool hasCost = false;

				foreach (ResourceSummaryRowData rollupRowData in rollupData)
				{
					// create a new summary data row in the table
					Row tableRow = this.CloneMarkedTemplateRow(templateDataRow);

					decimal cost = rollupRowData.CostTotal.HasValue ? rollupRowData.CostTotal.Value : 0m;
					decimal hours = rollupRowData.HoursTotal.HasValue ? rollupRowData.HoursTotal.Value : 0m;

					if (cost != 0m)
					{
						hasCost = true;
					}

					costTotal += cost;
					hoursTotal += hours;

					// populate the row
					StructuredDocumentTag resourceTypeElement = WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceType);
					string resourceTypeValue = rollupRowData.ResourceType;
					ElementOfCostType elementOfCostEnumValue = resourceTypeValue.GetEnumeratedValue<ElementOfCostType>(ElementOfCostType.NotSet);
					if (elementOfCostEnumValue != ElementOfCostType.NotSet)
					{
						resourceTypeValue = elementOfCostEnumValue.GetDescription();
					}
					WordUtilities.SetElementText(resourceTypeElement, resourceTypeValue);

					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceName), rollupRowData.ResourceName);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceDescription), rollupRowData.ResourceDescription);

					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Cost), cost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter));
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Hours), hours.ToString(DefaultHoursFormat));

					// add the row to the table
					currentInsertionRow.ParentNode.InsertAfter(tableRow, currentInsertionRow);
					currentInsertionRow = tableRow;
				}

				// can just use the existing total row (it does not need to be cloned)
				Row totalsRow = dataTotalsRowMarkerTag.GetAncestor(NodeType.Row) as Row;
				this.SetCantSplit(totalsRow);

				// populate the overall totals
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_CostTotal), costTotal.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter));
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_HoursTotal), hoursTotal.ToString(DefaultHoursFormat));

				//if there is no cost, remove notice saying costs are rounded since it is not needed
				if (!hasCost)
				{
					StructuredDocumentTag roundingNotice = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.FieldName_RoundingNotice);
					this.RemoveElement(roundingNotice); //no need to check null since RemoveElement already does this

					StructuredDocumentTag roundingNoticeAsterisk = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.FieldName_RoundingNoticeAsterisk);
					this.RemoveElement(roundingNoticeAsterisk);
				}

				// remove template rows
				templateDataRow.Remove();
			}
			else
			{
				this.RemoveElement(tableContainerElement);
			}
		}

		#endregion

		#region Custom fields

		/// <summary>
		/// Populates the template with Custom Field Values for a BOE, tasks, or resources
		/// </summary>
		/// <param name="customFieldsContainerElement">The StructuredDocumentTag for the custom field data</param>
		/// <param name="customFieldValues">dictionaries of the custom fields and their values in a dictionary with each key representing the BOE, Task, or Resource the fields are in</param>
		private void PopulateCustomFields(StructuredDocumentTag customFieldsContainerElement, IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> customFieldValues)
		{
			if (customFieldValues.Any())
			{
				// initialize the "insertion" row
				SdtBlock contentBlockTemplate;

				SdtBlock currentInsertionBlock = contentBlockTemplate = customFieldsContainerElement as SdtBlock;

				/*
                 * SdtBlock
                 *      SdtProperties
                 *          Tag = CustomFields-Labor
                 *      SdtContent
                 *          Paragraph
                 *              SdtRun
                 *                  SdtProperties
                 *                      Tag = CustomFieldLabel, CustomFieldID, CustomFieldDescription
                 *                  SdtContentRun
                 *                      Run
                 *                          Text
                 */

				foreach (KeyValuePair<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> customFieldMappings in customFieldValues)
				{
					foreach (KeyValuePair<CustomFieldValueDTO, CustomFieldDTO> customField in customFieldMappings.Value)
					{
						SdtBlock contentBlock = contentBlockTemplate.Clone(true) as SdtBlock;

						CustomFieldValueDTO fieldValue = customField.Key;
						CustomFieldDTO field = customField.Value;

						StructuredDocumentTag customFieldLabelElement = WordUtilities.GetTaggedChildElement(contentBlock, BOEExporterConstants.FieldName_CustomFieldLabel);
						StructuredDocumentTag customFieldIdElement = WordUtilities.GetTaggedChildElement(contentBlock, BOEExporterConstants.FieldName_CustomFieldID);
						StructuredDocumentTag customFieldDescriptionElement = WordUtilities.GetTaggedChildElement(contentBlock, BOEExporterConstants.FieldName_CustomFieldDescription);

						// Note: The template has an indeterminate number of [multiple] Run elements containing "pieces" of the full text.
						// Delete all but the first Run.
						RemoveAllButOneElement(customFieldLabelElement.GetFirstChild<SdtContentRun>(), NodeType.Run);
						if (customFieldIdElement != null)
						{
							RemoveAllButOneElement(customFieldIdElement.GetFirstChild<SdtContentRun>(), NodeType.Run);
						}
						RemoveAllButOneElement(customFieldDescriptionElement.GetFirstChild<SdtContentRun>(), NodeType.Run);

						// set values
						WordUtilities.SetElementText(customFieldLabelElement, field.CustomFieldName);
						if (customFieldIdElement == null)
						{
							// Handle templates without CustomFieldID element (i.e. compatible with open-ended fields) as follows:
							// if open-ended, only show custom field description; otherwise concatenate custom field "name - description" in description element.
							string customFieldString = field.IsOpenEnded ?
								$"{fieldValue.CustomFieldValueDescription}" :
								$"{fieldValue.CustomFieldValueName} - {fieldValue.CustomFieldValueDescription}";
							WordUtilities.SetElementText(customFieldDescriptionElement, customFieldString);
						}
						else
						{
							// Handle old-style templates with CustomFieldID element and trailing dash "-" as follows:
							// if open-ended, set CustomFieldID to empty string; otherwise display name and description in their respective elements.
							WordUtilities.SetElementText(customFieldIdElement, field.IsOpenEnded ? string.Empty : fieldValue.CustomFieldValueName);
							WordUtilities.SetElementText(customFieldDescriptionElement, fieldValue.CustomFieldValueDescription);
						}

						// add the row to the table
						currentInsertionBlock.ParentNode.InsertAfter(contentBlock, currentInsertionBlock);
						currentInsertionBlock = contentBlock;
					}
				}

				contentBlockTemplate.Remove();
			}
			else
			{
				this.RemoveElement(customFieldsContainerElement);
			}
		}

		#endregion

		#endregion

		#region Word utilities

		/// <summary>
		/// Populates the proprietary label in the footer.
		/// Populates the Date label in the footer with the date of printing.
		/// </summary>
		/// <param name="document">The Word document to populate</param>
		/// <param name="boeExportModelView">Object to hold most of the BOE's data</param>
		private void SetProprietaryLabelsAndPrintDate(
			Document document,
			BOEExportModelView boeExportModelView)
		{
			// Get all of the footers
			foreach (Section section in document)
			{
				// Up to three different footers are possible in a section (for first, even and odd pages)
				// we check and update all of them.
				HeaderFooter footer = section.HeadersFooters[HeaderFooterType.FooterFirst];
				SetProprietaryLabelsAndPrintDate(footer, boeExportModelView);
				footer = section.HeadersFooters[HeaderFooterType.FooterPrimary];
				SetProprietaryLabelsAndPrintDate(footer, boeExportModelView);
				footer = section.HeadersFooters[HeaderFooterType.FooterEven];
				SetProprietaryLabelsAndPrintDate(footer, boeExportModelView);
			}
		}

		/// <summary>
		/// Populates the proprietary label in the footer.
		/// Populates the Date label in the footer with the date of printing.
		/// </summary>
		/// <param name="footer">The footer to populate</param>
		/// <param name="boeExportModelView">Object to hold most of the BOE's data</param>
		private void SetProprietaryLabelsAndPrintDate(
			HeaderFooter footer,
			BOEExportModelView boeExportModelView)
		{
			if (footer != null)
			{
				StructuredDocumentTag sdt = footer.Range.StructuredDocumentTags.GetByTitle(BOEExporterConstants.FieldName_HeaderFooter) as StructuredDocumentTag;
				if (sdt != null)
				{
					WordUtilities.SetElementText(sdt, boeExportModelView.ContainsOCI ? BOEExporterConstants.LMPI_OCI_LABEL_TEXT : BOEExporterConstants.LMPI_LABEL_TEXT);
					sdt.Title = string.Empty;
					// alias.Remove();  OpenXML SdtAlias is Title on IStructuredDocumentTag
				}

				sdt = footer.Range.StructuredDocumentTags.GetByTitle(BOEExporterConstants.FieldName_Date) as StructuredDocumentTag;
				if (sdt != null)
				{
					WordUtilities.SetElementText(sdt, DateTime.Today.ToShortDateString());
					sdt.Title = string.Empty;
					// alias.Remove();  OpenXML SdtAlias is Title on IStructuredDocumentTag
				}
			}
		}

		#endregion

		#region Task data processing

		/// <summary>
		/// Processes the labor tasks.
		/// </summary>
		/// <param name="boe">The boe.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="BOEExportTaskElements">The boe export task elements.</param>
		/// <param name="logEnabled">if set to <c>true</c> [log enabled].</param>
		/// <param name="revCodeCustomField">The rev code custom field.</param>
		/// <param name="taskSegregationCustomField">The task segregation custom field.</param>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private void ProcessLaborTasks(BoeDTO boe, BOEExportInputs exportInputs, Collection<BOEExportTaskElement> BOEExportTaskElements, bool logEnabled, CustomFieldDTO revCodeCustomField, CustomFieldDTO taskSegregationCustomField)
		{
			if (logEnabled) { _log.LogInformation("Exporting - BOECustomExporter - ProcessLaborTasks - labor tasks begin"); }

			// The workspace's export format doesn't have the correct template type if this is a user template so always use the exportFormatDTO
			WorkspaceExportFormatDTO exportFormatDTO = exportInputs.WorkspaceExportFormat;

			ICollection<BoeTaskElementDTO> allBoeTaskElements = exportInputs.TaskElements.Where(x => x.BoeID == boe.Id).ToList();
			IReadOnlyCollection<CustomFieldValueDTO> workspaceCustomFieldValues = exportInputs.CustomFieldValues;
			IDictionary<int, ElementOfCostTypeModelView> allElementOfCostTypes = _ICommonDataMapper.GetElementOfCostTypesDictionary();
			IDictionary<int, SpreadCurveModelView> allSpreadCurves = _ICommonDataMapper.getSpreadCurveDictionary();

			foreach (BoeTaskElementDTO boeTaskElement in allBoeTaskElements)
			{
				BOEExportTaskElement boeExportTaskElement = new BOEExportTaskElement();
				boeExportTaskElement.BoeID = boeTaskElement.BoeID;
				boeExportTaskElement.BOETaskDesc = BOEExportConverter.GetRteOverride(boeTaskElement.BoeID, boeTaskElement.Id, boeTaskElement.Description, RteTemplateSource.TaskDescription, exportInputs.RTETemplatesOverrides);
				boeExportTaskElement.BOETaskElementID = boeTaskElement.Id;
				boeExportTaskElement.BOETaskID = boeTaskElement.BOETaskID;
				boeExportTaskElement.EndDate = boeTaskElement.EndDate;
				boeExportTaskElement.MOQEquation = boeTaskElement.MOQHoursEquation;
				boeExportTaskElement.MOQText = BOEExportConverter.GetRteOverride(boeTaskElement.BoeID, boeTaskElement.Id, boeTaskElement.MOQText, RteTemplateSource.TaskMOQ, exportInputs.RTETemplatesOverrides);
				boeExportTaskElement.MOQType = boeTaskElement.MOQType.GetDescription();
				boeExportTaskElement.MOQTypes = exportInputs.MOQTypes.Where(x => x.TaskId == boeTaskElement.Id).ToList();
				boeExportTaskElement.OrdinaryVariables = boeTaskElement.OrdinaryVariables;
				boeExportTaskElement.StartDate = boeTaskElement.StartDate;
				boeExportTaskElement.TaskTitle = boeTaskElement.TaskTitle;
				boeExportTaskElement.IMS_ID = boeTaskElement.IMS_ID;
				boeExportTaskElement.BOETaskElementOrder = boeTaskElement.BOETaskElementOrder;
				
				if (exportInputs.Workspace.UsingTemplateBOE)
				{
					boeExportTaskElement.MOQTypes = exportInputs.MOQTypes.Where(x => x.TaskId == boeTaskElement.Id).ToCollection();

					if (BOETaskUtility.ShowSkillMixForTask(exportInputs.Workspace.CreationDate, exportInputs.Workspace.UsingTemplateBOE,
						exportInputs.Workspace.EnableSAPConnection, boeExportTaskElement.MOQTypes, boeTaskElement.Id, boeTaskElement.HasTMRates))
					{
						boeExportTaskElement.SkillMixTable = boeTaskElement.SkillMixTable;
						boeExportTaskElement.CommonDisclosureTable = boeTaskElement.CommonDisclosureTable;
						boeExportTaskElement.HasTMRates = boeTaskElement.HasTMRates;
					}
				}

				boeExportTaskElement.SetTaskElementType(boeTaskElement.TaskElementType);

				// Get Rev Code Value for current Task Element

				if (revCodeCustomField != null)
				{
					CustomFieldValueDTO revcode = (from v in workspaceCustomFieldValues
												   from c in boeTaskElement.CustomFieldValueContainers
												   where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == revCodeCustomField.Id
												   select v).FirstOrDefault();

					if (revcode != null)
					{
						boeExportTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskElementRevCode] = revcode.CustomFieldValueName;
					}
				}
				// get Workspace Variables associated with BOE DTO Task Element
				boeExportTaskElement.WorkspaceVariables = exportInputs.WorkspaceVariables.Where(x => boeTaskElement.WorkspaceVariableIDs.Contains(x.Id)).ToCollection<WorkspaceVariableDTO>();

				if (taskSegregationCustomField != null)
				{
					CustomFieldValueDTO TaskSegregation = (from v in workspaceCustomFieldValues
														   from c in boeTaskElement.CustomFieldValueContainers
														   where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == taskSegregationCustomField.Id
														   select v).FirstOrDefault();

					if (TaskSegregation != null)
					{
						boeExportTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskSegregation] = string.Concat(
							TaskSegregation.CustomFieldValueName == null ? string.Empty : TaskSegregation.CustomFieldValueName,
							BOEExporterConstants.BLANK_SPACE,
							TaskSegregation.CustomFieldValueDescription == null ? string.Empty : TaskSegregation.CustomFieldValueDescription);
					}
				}

				// get Task Element Labors
				Collection<BOEExportTaskElementLabor> BoeExportLabors = new Collection<BOEExportTaskElementLabor>();

				foreach (ResourceTypeDto laborType in boeTaskElement.taskElementLabors)
				{
					BOEExportTaskElementLabor boeExportLabor = new BOEExportTaskElementLabor(boeExportTaskElement);
					boeExportLabor.LaborTypeOrder = laborType.LaborTypeOrder;

					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_LaborTypeID] = laborType.Id.ToString();
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskElementID] = laborType.TaskElementId.ToString();

					if (laborType.SpreadType == SpreadType.Hours)
					{
						boeExportLabor.Hours = laborType.ValueSpread;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_LaborTypeHours] = boeExportLabor.Hours.HasValue ? boeExportLabor.Hours.Value.ToString(DefaultHoursFormat) : string.Empty;
					}
					else if (laborType.SpreadType == SpreadType.Cost)
					{
						boeExportLabor.Cost = (decimal)laborType.ValueSpread;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_LaborTypeCost] = boeExportLabor.Cost.HasValue ? boeExportLabor.Cost.Value.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter) : string.Empty;
					}

					decimal taskElementCostTotal = 0m;

					if (laborType.SpreadType == SpreadType.Cost)
					{
						taskElementCostTotal = laborType.LaborSpreads.Sum(s => s.LaborSpreadValue);
					}

					boeExportLabor.Cost = taskElementCostTotal;

					if (laborType.ResourceID.HasValue)
					{
						ResourceDTO resource = exportInputs.ResourcesUsedInWsBoes.First(x => x.Id == laborType.ResourceID.Value);
						if (exportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.DS_ES_STANDARD_PORTRAIT_WITH_COST ||
							exportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.DS_ES_STANDARD_PORTRAIT_WITHOUT_COST ||
							exportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.DS_STANDARD_PORTRAIT_WITHOUT_COST_2)
						{
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_LaborTypes] = resource.LaborType;
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_SegmentRegion] = resource.SegRegion;
						}
						else
						{
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_LaborTypes] = allElementOfCostTypes[(int)resource.ElementOfCost].ElementOfCostName;
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_SegmentRegion] = resource.Segment.ToString();
						}
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription] = resource.ResourceDesc;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceID] = resource.Id.ToString();
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] = resource.ResourceName;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost] = resource.ElementOfCost.ToString();
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceRateType] = resource.RateType.ToString();
					}

					if (CommonUtilities.IsBRCEnabledForWorkspace(exportInputs.Workspace.Shortname))
					{
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_BrcID] = laborType.BusinessResourceCodeID.HasValue
							? laborType.BusinessResourceCodeID.ToString() : string.Empty;
					}

					if (laborType.PerformingOrgID.HasValue)
					{
						PerformingOrgDTO perfOrg = exportInputs.PerformingOrgsUsedInBoes.First(x => x.Id == laborType.PerformingOrgID.Value);
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID] = perfOrg.Id.ToString();
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrg] = perfOrg.PerformingOrgName;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrgDescription] = perfOrg.PerformingOrgDesc;
					}

					if (boe.IsMultiClinWbs)
					{
						if (laborType.WBSID.HasValue)
						{
							WbsDTO wbs = exportInputs.WbsElements.First(x => x.Id == laborType.WBSID.Value);
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_WBSString] = wbs.WbsString;
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_WBSNumber] = wbs.WbsNumber;
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_WBSTitle] = wbs.WbsTitle;
						}
						else
						{
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_WBSNumber] = string.Empty;
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_WBSTitle] = string.Empty;
						}

						if (laborType.CLINID.HasValue)
						{
							ClinDTO clin = exportInputs.Clins.First(x => x.Id == laborType.CLINID.Value);
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_CLINString] = clin.ClinString;
						}
					}

					if (laborType.StartDate.HasValue)
					{
						boeExportLabor.StartDate = laborType.StartDate.Value;
					}

					if (laborType.EndDate.HasValue)
					{
						boeExportLabor.EndDate = laborType.EndDate.Value;
					}

					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_SpreadCurve] =
						laborType.SpreadCurveID.HasValue ? allSpreadCurves[(int)laborType.SpreadCurveID.Value].SpreadCurveName.Replace("Hours", FullObjectHelper.HoursLabel(exportInputs.Workspace)) : string.Empty;

					// Get Skill Level Value for current Resource Type
					CustomFieldDTO skillLevelCustomField = (from c in exportInputs.CustomFields
															where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_SkillLevel, StringComparison.CurrentCultureIgnoreCase) &&
															c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
															select c).FirstOrDefault();

					if (skillLevelCustomField != null)
					{
						CustomFieldValueDTO skillLevel = (from v in workspaceCustomFieldValues
														  from c in laborType.CustomFieldValueContainers
														  where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == skillLevelCustomField.Id
														  select v).FirstOrDefault();

						if (skillLevel != null)
						{
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeSkillLevel] = skillLevel.CustomFieldValueName;
						}
					}

					// Get Site Value for current Resource Type
					CustomFieldDTO siteCustomField = (from c in exportInputs.CustomFields
													  where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_Site, StringComparison.CurrentCultureIgnoreCase) &&
													  c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
													  select c).FirstOrDefault();

					if (siteCustomField != null)
					{
						CustomFieldValueDTO site = (from v in workspaceCustomFieldValues
													from c in laborType.CustomFieldValueContainers
													where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == siteCustomField.Id
													select v).FirstOrDefault();

						if (site != null)
						{
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeSite] = site.CustomFieldValueDescription;
						}
					}

					// Get Skill Mix Value for current Resource Type
					CustomFieldDTO skillMixCustomField = (from c in exportInputs.CustomFields
														  where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_SkillMix, StringComparison.CurrentCultureIgnoreCase) &&
														  c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
														  select c).FirstOrDefault();

					if (skillMixCustomField != null)
					{
						CustomFieldValueDTO skillMix = (from v in workspaceCustomFieldValues
														from c in laborType.CustomFieldValueContainers
														where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == skillMixCustomField.Id
														select v).FirstOrDefault();

						if (skillMix != null)
						{
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeSkillMix] = skillMix.CustomFieldValueDescription;
						}
						else
						{
							boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeSkillMix] = string.Empty;
						}
					}
					else
					{
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeSkillMix] = string.Empty;
					}

					BoeExportLabors.Add(boeExportLabor);
				}

				boeExportTaskElement.taskElementLabors = BoeExportLabors;

				boeExportTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskHoursTotal] = boeExportTaskElement.taskElementLabors.Where(l => l.Hours.HasValue).Sum(l => l.Hours.Value).ToString(DefaultHoursFormat);

				//only sum discrete costs
				boeExportTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskCostTotal] = boeExportTaskElement.taskElementLabors
					.Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_SpreadCurve))
					.Where(c => c.Cost.HasValue && c.ExportFields[BOEExporterConstants.FieldName_SpreadCurve].Equals(SpreadCurves.DiscreteCost.ToDescription()))
					.Sum(c => c.Cost.Value).ToString("C0", _CurrencyFormatter);

				BOEExportTaskElements.Add(boeExportTaskElement);

			}

			if (logEnabled) { _log.LogInformation("Exporting - BOECustomExporter - ProcessLaborTasks - labor tasks end"); }
		}

		/// <summary>
		/// Processes the odc tasks.
		/// </summary>
		/// <param name="boe">The boe.</param>
		/// <param name="BOEExportTaskElements">The boe export task elements.</param>
		/// <param name="logEnabled">if set to <c>true</c> [log enabled].</param>
		/// <param name="exportInputs">The export inputs.</param>
		private void ProcessODCTasks(BoeDTO boe, Collection<BOEExportTaskElement> BOEExportTaskElements, bool logEnabled, BOEExportInputs exportInputs)
		{
			if (logEnabled)
			{
				_log.LogInformation("Exporting - BOECustomExporter - ProcessODCTasks - odc tasks begin");
			}

			foreach (OtherDirectCostDTO odcElement in exportInputs.Odcs.Where(x => x.BoeID == boe.Id).ToList())
			{
				BOEExportTaskElement boeExportTaskElement = new BOEExportTaskElement();
				boeExportTaskElement.BoeID = boe.Id;
				boeExportTaskElement.BOETaskDesc = odcElement.TaskDescription;
				boeExportTaskElement.BOETaskElementID = odcElement.Id;
				boeExportTaskElement.MOQText = odcElement.MoqText;
				boeExportTaskElement.TaskTitle = odcElement.TaskTitle;
				boeExportTaskElement.BOETaskID = odcElement.TaskID;
				boeExportTaskElement.StartDate = odcElement.StartDate.HasValue ? odcElement.StartDate.Value : boe.StartDate;
				boeExportTaskElement.EndDate = odcElement.EndDate.HasValue ? odcElement.EndDate : boe.EndDate;
				boeExportTaskElement.ElementType = BOEExportTaskElementType.ODC;
				boeExportTaskElement.BOETaskElementOrder = odcElement.BOETaskElementOrder;

				// get Task Element Labors
				Collection<BOEExportTaskElementLabor> BoeExportLabors = new Collection<BOEExportTaskElementLabor>();

				// Aggregate ODC Types by matching Resource and Performing Org
				var aggregatedODCTypes = from odcType in odcElement.ODCTypes
										 group odcType by new
										 {
											 odcType.ResourceID,
											 odcType.PerformingOrgID
										 }
											 into grouping
										 select new
										 {
											 grouping.Key.ResourceID,
											 grouping.Key.PerformingOrgID,
											 Cost = (from odcType in grouping
													 where odcType.Cost.HasValue
													 select odcType.SpreadCurve == SpreadCurves.Load ?
														 odcType.Cost.Value * odcType.ODCSpreads.Count :
														 odcType.Cost.Value).Sum()
										 };

				foreach (var odcType in aggregatedODCTypes)
				{
					BOEExportTaskElementLabor boeExportLabor = new BOEExportTaskElementLabor(boeExportTaskElement);

					boeExportLabor.Cost = odcType.Cost;
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_LaborTypeCost] = odcType.Cost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter);

					if (odcType.ResourceID.HasValue)
					{
						ResourceDTO resource = exportInputs.ResourcesUsedInWsBoes.First(x => x.Id == odcType.ResourceID.Value);
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_LaborTypes] = resource.LaborType;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription] = resource.ResourceDesc;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] = resource.ResourceName;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceID] = resource.Id.ToString();
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost] = resource.ElementOfCost.ToString();
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceRateType] = resource.RateType.ToString();
					}

					if (odcType.PerformingOrgID.HasValue)
					{
						PerformingOrgDTO perfOrg = exportInputs.PerformingOrgsUsedInBoes.First(x => x.Id == odcType.PerformingOrgID.Value);
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID] = perfOrg.Id.ToString();
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrg] = perfOrg.PerformingOrgName;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrgDescription] = perfOrg.PerformingOrgDesc;
					}

					BoeExportLabors.Add(boeExportLabor);
				}

				boeExportTaskElement.taskElementLabors = BoeExportLabors;

				boeExportTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskCostTotal] = boeExportTaskElement.taskElementLabors.Where(l => l.Cost.HasValue).Sum(l => l.Cost.Value).ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter);
				boeExportTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskTypeDate] = boeExportTaskElement.StartDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR);

				BOEExportTaskElements.Add(boeExportTaskElement);
			}

			if (logEnabled)
			{
				_log.LogInformation("Exporting - BOECustomExporter - ProcessODCTasks - odc tasks end");
			}
		}

		/// <summary>
		/// Process Travel Task data for the export
		/// </summary>
		/// <param name="boe">BOE DTO for BOE containing Travel Task</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="BOEExportTaskElements">Collection of BOE Export Task Elements for the Travel Tasks</param>
		/// <param name="logEnabled">bool noting if log is enabled</param>
		protected virtual void ProcessTravelTasks(BoeDTO boe, BOEExportInputs exportInputs, Collection<BOEExportTaskElement> BOEExportTaskElements, bool logEnabled)
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
				_log.LogInformation("Exporting - BOECustomExporter - ProcessTravelTasks - travel tasks begin");
			}

			ICollection<TravelDTO> travelTasks = exportInputs.Travels.Where(x => x.BoeID == boe.Id).ToList();
			ICollection<ResourceDTO> travelResources = new Collection<ResourceDTO>();

			// only grab the travel resources if there are travel tasks
			if (travelTasks.Any())
			{
				travelResources = exportInputs.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.Travel).ToCollection();
			}

			foreach (TravelDTO travelElement in travelTasks)
			{
				BOEExportTaskElement boeExportTaskElement = new BOEExportTaskElement();
				boeExportTaskElement.BoeID = boe.Id;
				boeExportTaskElement.BOETaskDesc = travelElement.Description;
				boeExportTaskElement.BOETaskElementID = travelElement.Id;
				boeExportTaskElement.TaskTitle = travelElement.TaskTitle;
				boeExportTaskElement.StartDate = travelElement.StartDate.HasValue ? travelElement.StartDate : boe.StartDate;
				boeExportTaskElement.EndDate = travelElement.EndDate.HasValue ? travelElement.EndDate : boe.EndDate;
				boeExportTaskElement.ElementType = BOEExportTaskElementType.Travel;
				boeExportTaskElement.BOETaskElementOrder = travelElement.BOETaskElementOrder;

				// get Task Element Labors
				Collection<BOEExportTaskElementLabor> BoeExportLabors = new Collection<BOEExportTaskElementLabor>();

				foreach (TravelTripType travelTrip in travelElement.TravelTrips)
				{
					BOEExportTaskElementLabor boeExportLabor = new BOEExportTaskElementLabor(boeExportTaskElement);
					TripDTO systemTrip = exportInputs.TravelTrips.FirstOrDefault(t => t.TripID == travelTrip.SystemTripID);
					if (systemTrip == null)
					{
						throw new GeneralAppException("Did not find System Trip inside ExportInputs.TravelTrips with Id:" + travelTrip.SystemTripID);
					}

					decimal miscRate = exportInputs.MiscTravelRatesForTravelTrips.FirstOrDefault(x => x.Id == systemTrip.MiscTravelRateID).MiscTravelRate;
					PerDiemDTO perDiem = exportInputs.PerDiemsForTravelTrips.FirstOrDefault(x => x.Id == systemTrip.PerDiemID);

					boeExportLabor.Cost = _TravelTripCostCalculation.CalculateTravelCost(travelTrip, exportInputs.Workspace, exportInputs.WorkspaceHistory, systemTrip, miscRate, perDiem, exportInputs.EscalationRates).CostTotal;
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_LaborTypeCost] = boeExportLabor.Cost.Value.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter);
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TravelTripID] = travelTrip.TravelTripID.ToString();

					LocationDTO departureLocation = exportInputs.LocationsUsedByTrips.FirstOrDefault(x => x.Id == systemTrip.DepartureLocationID);
					LocationDTO destinationLocation = exportInputs.LocationsUsedByTrips.FirstOrDefault(x => x.Id == systemTrip.DestinationLocationID);

					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription] =
						departureLocation.LocationName +
						" to " +
						destinationLocation.LocationName +
						" - " +
						travelTrip.Purpose +
						" - " +
						travelTrip.NumOfTrips +
						" Trip(s), " +
						travelTrip.NumOfPeople +
						" Traveler(s), " +
						travelTrip.NumOfDays +
						" Day(s) in " +
						travelTrip.TripDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR);

					ResourceDTO resource = travelResources.FirstOrDefault(r => r.Segment == travelTrip.Segment);

					if (resource != null)
					{
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceID] = resource.Id.ToString();
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_LaborTypes] = resource.LaborType;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_SegmentRegion] = resource.Segment.ToString();
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] = resource.ResourceName;
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost] = resource.ElementOfCost.ToString();
						boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceRateType] = resource.RateType.ToString();
					}

					PerformingOrgDTO perfOrg = exportInputs.PerformingOrgsUsedInBoes.First(x => x.Id == travelTrip.PerfOrgID);
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID] = perfOrg.Id.ToString();
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrg] = perfOrg.PerformingOrgName;
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDeparture] = departureLocation.LocationName;
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDestination] = destinationLocation.LocationName;
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypePurpose] = travelTrip.Purpose;
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeTrips] = travelTrip.NumOfTrips.ToString();
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypePeople] = travelTrip.NumOfPeople.ToString();
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDays] = travelTrip.NumOfDays.ToString();
					boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDate] = travelTrip.TripDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR);

					BoeExportLabors.Add(boeExportLabor);
				}

				boeExportTaskElement.taskElementLabors = BoeExportLabors;

				boeExportTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskCostTotal] = boeExportTaskElement.taskElementLabors.Where(l => l.Cost.HasValue).Sum(l => l.Cost.Value).ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter);

				BOEExportTaskElements.Add(boeExportTaskElement);
			}

			if (logEnabled) { _log.LogInformation("Exporting - BOECustomExporter - ProcessTravelTasks - travel tasks end"); }
		}

		/// <summary>
		/// Processes the materials tasks.
		/// </summary>
		/// <param name="boe">The boe.</param>
		/// <param name="BOEExportTaskElements">The boe export task elements.</param>
		/// <param name="logEnabled">if set to <c>true</c> [log enabled].</param>
		/// <param name="exportInputs">The export inputs.</param>
		private void ProcessMaterialsTasks(BoeDTO boe, Collection<BOEExportTaskElement> BOEExportTaskElements, bool logEnabled, BOEExportInputs exportInputs)
		{
			if (logEnabled)
			{
				_log.LogInformation("Exporting - BOECustomExporter - ProcessMaterialsTasks - materials tasks begin");
			}

			foreach (MaterialDTO materialsElement in exportInputs.Materials.Where(x => x.BoeID == boe.Id).ToList())
			{
				BOEExportTaskElement boeExportTaskElement = new BOEExportTaskElement();
				boeExportTaskElement.BoeID = boe.Id;
				boeExportTaskElement.BOETaskDesc = materialsElement.TaskDescription;
				boeExportTaskElement.BOETaskElementID = materialsElement.Id;
				boeExportTaskElement.TaskTitle = materialsElement.TaskTitle;
				boeExportTaskElement.MOQText = materialsElement.MoqText;
				boeExportTaskElement.StartDate = materialsElement.StartDate;
				boeExportTaskElement.EndDate = materialsElement.EndDate;
				boeExportTaskElement.BOETaskID = materialsElement.TaskID;
				boeExportTaskElement.ElementType = BOEExportTaskElementType.Material;

				boeExportTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskCostTotal] = boeExportTaskElement.taskElementLabors.Where(l => l.Cost.HasValue).Sum(l => l.Cost.Value).ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, _CurrencyFormatter);

				BOEExportTaskElements.Add(boeExportTaskElement);

			}

			if (logEnabled)
			{
				_log.LogInformation("Exporting - BOECustomExporter - ProcessMaterialsTasks - material tasks end");
			}
		}

		#endregion

		#region Rollup data methods

		/// <summary>
		/// Gets the data needed for the date rollup
		/// </summary>
		/// <param name="taskElementCollection">The task elements containing the data to rollup</param>
		/// <param name="rateTypeFilter">Rate Type filter</param>
		/// <param name="resources">Resources</param>
		/// <param name="useGfy">use government fiscal year?</param>
		/// <returns>A <see cref="LaborRollupByDateNew"/> object containing the rolled up data</returns>

		protected virtual List<LaborRollupByDateNew> GetRollupByYear(ICollection<BoeTaskElementDTO> taskElementCollection, Collection<ResourceDTO> resources = null, RateType? rateTypeFilter = null, bool useGfy = false)
		{
			List<ResourceTypeDto> allTaskElementResources;
			if (resources == null)
			{
				allTaskElementResources = taskElementCollection.SelectMany(x => x.taskElementLabors).ToList().DeepClone();
			}
			else
			{
				//ignore unassigned resources
				ICollection<int> resourceIds = resources.Select(r => r.Id).ToList();
				allTaskElementResources = taskElementCollection.SelectMany(x => x.taskElementLabors).Where(r => r.ResourceID.HasValue && resourceIds.Contains(r.ResourceID.Value)).ToList().DeepClone();
			}

			// If using Govt Fiscal Year, add 1 year to dates in October - December as they are in the following fiscal year
			if (useGfy)
			{
				foreach (ResourceTypeDto resourceType in allTaskElementResources)
				{
					if (resourceType.StartDate.HasValue)
					{
						resourceType.StartDate = this.AdjustDateForGovtFiscalYear(resourceType.StartDate.Value);
					}

					if (resourceType.EndDate.HasValue)
					{
						resourceType.EndDate = this.AdjustDateForGovtFiscalYear(resourceType.EndDate.Value);
					}

					foreach (ResourceSpreadDto spread in resourceType.LaborSpreads)
					{
						spread.LaborSpreadDate = this.AdjustDateForGovtFiscalYear(spread.LaborSpreadDate);
					}
				}
			}

			List<LaborRollupByDateNew> RollupByDateList = new List<LaborRollupByDateNew>();

			Tuple<ResourceTypeDto, ResourceTypeDto> earliestAndLatestTask = WordUtilities.GetEarliestAndLatestItems(allTaskElementResources, x => x.StartDate.HasValue, x => x.StartDate.Value, x => x.EndDate.HasValue, x => x.EndDate.Value);
			ResourceTypeDto Earliest = earliestAndLatestTask.Item1;
			ResourceTypeDto Latest = earliestAndLatestTask.Item2;

			if (Earliest != null && Latest != null)
			{
				for (int i = Earliest.StartDate.Value.Year; i <= Latest.EndDate.Value.Year; i++)
				{
					LaborRollupByDateNew Rollup = new LaborRollupByDateNew();
					Rollup.Year = i;

					Rollup.January = GetRollupForMonth(allTaskElementResources, i, 1, resources, rateTypeFilter);
					Rollup.February = GetRollupForMonth(allTaskElementResources, i, 2, resources, rateTypeFilter);
					Rollup.March = GetRollupForMonth(allTaskElementResources, i, 3, resources, rateTypeFilter);
					Rollup.April = GetRollupForMonth(allTaskElementResources, i, 4, resources, rateTypeFilter);
					Rollup.May = GetRollupForMonth(allTaskElementResources, i, 5, resources, rateTypeFilter);
					Rollup.June = GetRollupForMonth(allTaskElementResources, i, 6, resources, rateTypeFilter);
					Rollup.July = GetRollupForMonth(allTaskElementResources, i, 7, resources, rateTypeFilter);
					Rollup.August = GetRollupForMonth(allTaskElementResources, i, 8, resources, rateTypeFilter);
					Rollup.September = GetRollupForMonth(allTaskElementResources, i, 9, resources, rateTypeFilter);
					Rollup.October = GetRollupForMonth(allTaskElementResources, i, 10, resources, rateTypeFilter);
					Rollup.November = GetRollupForMonth(allTaskElementResources, i, 11, resources, rateTypeFilter);
					Rollup.December = GetRollupForMonth(allTaskElementResources, i, 12, resources, rateTypeFilter);

					RollupByDateList.Add(Rollup);
				}
			}
			return RollupByDateList.OrderBy(x => x.Year).ToList();
		}

		private decimal GetRollupForMonth(List<ResourceTypeDto> taskElementCollection, int year, int month, Collection<ResourceDTO> resources, RateType? rateTypeFilter = null)
		{
			decimal sum;

			if (resources == null)
			{
				sum =
					(from f in taskElementCollection
					 where f.SpreadType == SpreadType.Hours
					 from g in f.LaborSpreads
					 where g.LaborSpreadDate.Year == year && g.LaborSpreadDate.Month == month
					 select g.LaborSpreadValue).Sum();
			}
			else
			{
				ICollection<int> resourceIds = resources.Select(r => r.Id).ToList();
				sum =
					(from f in taskElementCollection
					 where f.SpreadType == SpreadType.Hours && resourceIds.Contains(f.ResourceID.Value)
					 from g in f.LaborSpreads
					 where g.LaborSpreadDate.Year == year && g.LaborSpreadDate.Month == month
					 select g.LaborSpreadValue).Sum();
			}

			if (rateTypeFilter.HasValue && rateTypeFilter.Value == RateType.Cost)
			{
				sum = sum / 100m;
			}

			return sum;
		}

		/// <summary>
		/// Gets the data needed for the custom field rollup
		/// </summary>
		/// <param name="taskElementCollection">The task elements containing the data to rollup</param>
		/// <param name="laborTypeIds">Set of LaborTypeIds to rollup; if null, all task element labors will be rolled up</param>
		/// <returns>A <see cref="LaborRollupByDateNew"/> object containing the rolled up data</returns>

		protected virtual List<LaborRollupByDateNew> GetLaborRollupByYear(ICollection<BoeTaskElementDTO> taskElementCollection, ICollection<int> laborTypeIds = null, RateType? rateTypeFilter = null)
		{
			List<ResourceTypeDto> allTaskElementResources;
			if (laborTypeIds == null)
			{
				allTaskElementResources = taskElementCollection.SelectMany(x => x.taskElementLabors).ToList();
			}
			else
			{
				//ignore unassigned labors
				allTaskElementResources = taskElementCollection.SelectMany(x => x.taskElementLabors).Where(r => laborTypeIds.Contains(r.Id)).ToList();
			}

			List<LaborRollupByDateNew> RollupByDateList = new List<LaborRollupByDateNew>();

			Tuple<ResourceTypeDto, ResourceTypeDto> earliestAndLatestTask = WordUtilities.GetEarliestAndLatestItems(allTaskElementResources, x => x.StartDate.HasValue, x => x.StartDate.Value, x => x.EndDate.HasValue, x => x.EndDate.Value);
			ResourceTypeDto Earliest = earliestAndLatestTask.Item1;
			ResourceTypeDto Latest = earliestAndLatestTask.Item2;

			if (Earliest != null && Latest != null)
			{
				for (int i = Earliest.StartDate.Value.Year; i <= Latest.EndDate.Value.Year; i++)
				{
					LaborRollupByDateNew Rollup = new LaborRollupByDateNew();
					Rollup.Year = i;

					Rollup.January = GetLaborRollupForMonth(allTaskElementResources, i, 1, laborTypeIds, rateTypeFilter);
					Rollup.February = GetLaborRollupForMonth(allTaskElementResources, i, 2, laborTypeIds, rateTypeFilter);
					Rollup.March = GetLaborRollupForMonth(allTaskElementResources, i, 3, laborTypeIds, rateTypeFilter);
					Rollup.April = GetLaborRollupForMonth(allTaskElementResources, i, 4, laborTypeIds, rateTypeFilter);
					Rollup.May = GetLaborRollupForMonth(allTaskElementResources, i, 5, laborTypeIds, rateTypeFilter);
					Rollup.June = GetLaborRollupForMonth(allTaskElementResources, i, 6, laborTypeIds, rateTypeFilter);
					Rollup.July = GetLaborRollupForMonth(allTaskElementResources, i, 7, laborTypeIds, rateTypeFilter);
					Rollup.August = GetLaborRollupForMonth(allTaskElementResources, i, 8, laborTypeIds, rateTypeFilter);
					Rollup.September = GetLaborRollupForMonth(allTaskElementResources, i, 9, laborTypeIds, rateTypeFilter);
					Rollup.October = GetLaborRollupForMonth(allTaskElementResources, i, 10, laborTypeIds, rateTypeFilter);
					Rollup.November = GetLaborRollupForMonth(allTaskElementResources, i, 11, laborTypeIds, rateTypeFilter);
					Rollup.December = GetLaborRollupForMonth(allTaskElementResources, i, 12, laborTypeIds, rateTypeFilter);

					RollupByDateList.Add(Rollup);
				}
			}
			return RollupByDateList.OrderBy(x => x.Year).ToList();
		}

		private decimal GetLaborRollupForMonth(List<ResourceTypeDto> taskElementCollection, int year, int month, ICollection<int> laborTypeIds, RateType? rateTypeFilter = null)
		{
			decimal sum;

			if (laborTypeIds == null)
			{
				sum =
					(from f in taskElementCollection
					 where f.SpreadType == SpreadType.Hours
					 from g in f.LaborSpreads
					 where g.LaborSpreadDate.Year == year && g.LaborSpreadDate.Month == month
					 select g.LaborSpreadValue).Sum();
			}
			else
			{
				sum =
					(from f in taskElementCollection
					 where f.SpreadType == SpreadType.Hours && laborTypeIds.Contains(f.Id)
					 from g in f.LaborSpreads
					 where g.LaborSpreadDate.Year == year && g.LaborSpreadDate.Month == month
					 select g.LaborSpreadValue).Sum();
			}

			if (rateTypeFilter.HasValue && rateTypeFilter.Value == RateType.Cost)
			{
				sum = sum / 100m;
			}

			return sum;
		}

		/// <summary>
		/// Gets the odc travel cost summary rollup by year data.
		/// </summary>
		/// <param name="ODCElements">The odc elements.</param>
		/// <param name="labors">The labors.</param>
		/// <param name="dateRange">The date range.</param>
		/// <returns>The list of labor rollup by date</returns>
		private List<LaborRollupByDateNew> GetODCTravelCostSummaryRollupByYearData(ICollection<OtherDirectCostDTO> ODCElements,
			Collection<BOEExportTaskElementLabor> labors, DateRange dateRange)
		{
			List<LaborRollupByDateNew> RollupByDateList = new List<LaborRollupByDateNew>();

			for (int i = dateRange.StartDate.Value.Year; i <= dateRange.EndDate.Value.Year; i++)
			{
				LaborRollupByDateNew Rollup = new LaborRollupByDateNew();
				Rollup.Year = i;

				Rollup.January = GetRollupForMonth(ODCElements, labors, i, 1);
				Rollup.February = GetRollupForMonth(ODCElements, labors, i, 2);
				Rollup.March = GetRollupForMonth(ODCElements, labors, i, 3);
				Rollup.April = GetRollupForMonth(ODCElements, labors, i, 4);
				Rollup.May = GetRollupForMonth(ODCElements, labors, i, 5);
				Rollup.June = GetRollupForMonth(ODCElements, labors, i, 6);
				Rollup.July = GetRollupForMonth(ODCElements, labors, i, 7);
				Rollup.August = GetRollupForMonth(ODCElements, labors, i, 8);
				Rollup.September = GetRollupForMonth(ODCElements, labors, i, 9);
				Rollup.October = GetRollupForMonth(ODCElements, labors, i, 10);
				Rollup.November = GetRollupForMonth(ODCElements, labors, i, 11);
				Rollup.December = GetRollupForMonth(ODCElements, labors, i, 12);

				RollupByDateList.Add(Rollup);
			}

			return RollupByDateList;
		}

		/// <summary>
		/// Gets the rollup for month.
		/// </summary>
		/// <param name="odcElements">The odc elements.</param>
		/// <param name="labors">The labors.</param>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <returns>The rollup for a month.</returns>
		private decimal GetRollupForMonth(ICollection<OtherDirectCostDTO> odcElements, Collection<BOEExportTaskElementLabor> labors, int year, int month)
		{
			decimal sum =
				(from e in odcElements
				 from f in e.ODCTypes
				 from g in f.ODCSpreads
				 where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == year && g.ODCSpreadDate.Value.Month == month
				 select g.CostSpreadValue.Value / 100m).Sum() +
					(from e in labors
					 where (e.ElementType == BOEExportTaskElementType.Travel || e.ElementType == BOEExportTaskElementType.Material) &&
					 DateTime.Parse(e.ExportFields[BOEExporterConstants.FieldName_TaskTypeDate]).Year == year &&
					 DateTime.Parse(e.ExportFields[BOEExporterConstants.FieldName_TaskTypeDate]).Month == month
					 select e.Cost.Value).Sum();

			return sum;
		}

		/// <summary>
		/// Get the odc spread rollup for a particular month
		/// </summary>
		/// <param name="odcSpreads">Spread data for ODC task</param>
		/// <param name="year">Year of the spread data</param>
		/// <param name="month">Month of the spread data as an int</param>
		/// <returns>odc spread rollup</returns>
		protected decimal GetRollupForMonth(ICollection<OtherDirectCostSpread> odcSpreads, int year, int month)
		{
			decimal sum =
				(from e in odcSpreads
				 where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == year && e.ODCSpreadDate.Value.Month == month
				 select e.CostSpreadValue.Value / 100m).Sum();

			return sum;
		}

		/// <summary>
		/// Get cost rollup data for ODC Resources
		/// </summary>
		/// <param name="ODCElements">ODC resources</param>
		/// <param name="labors">Labors for the ODC task</param>
		/// <param name="useGfy">use government fiscal year?</param>
		/// <returns>Cost rollup data for ODC Resources</returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected Dictionary<int, List<LaborRollupByDateNew>> GetODCCostRollup(Collection<OtherDirectCostDTO> ODCElements, Collection<BOEExportTaskElementLabor> labors, bool useGfy)
		{
			Dictionary<int, List<LaborRollupByDateNew>> RollupByDate = new Dictionary<int, List<LaborRollupByDateNew>>();
			DateRange ODCElementsDateRange = GetODCTravelDateRange(ODCElements, null, useGfy);
			if (ODCElementsDateRange.StartDate.HasValue && ODCElementsDateRange.EndDate.HasValue)
			{
				ICollection<OtherDirectCostType> ODCElementTypes = ODCElements.SelectMany(x => x.ODCTypes).ToList();
				ICollection<int> odcResourceIds = ODCElementTypes.Where(t => t.ResourceID.HasValue).Select(t => t.ResourceID.Value).Distinct().ToList();

				foreach (int resourceId in odcResourceIds)
				{
					RollupByDate.Add(resourceId, new List<LaborRollupByDateNew>());

					ICollection<OtherDirectCostSpread> odcSpreads = ODCElementTypes.Where(testc => testc.ResourceID == resourceId).SelectMany(t => t.ODCSpreads).ToList().DeepClone();

					if (useGfy)
					{
						foreach (OtherDirectCostSpread spread in odcSpreads.Where(x => x.ODCSpreadDate.HasValue))
						{
							spread.ODCSpreadDate = this.AdjustDateForGovtFiscalYear(spread.ODCSpreadDate.Value);
						}
					}

					for (int i = ODCElementsDateRange.StartDate.Value.Year; i <= ODCElementsDateRange.EndDate.Value.Year; i++)
					{
						LaborRollupByDateNew Rollup = new LaborRollupByDateNew();
						Rollup.Resource = labors.First(x => int.Parse(x.ExportFields[BOEExporterConstants.FieldName_ResourceID]) == resourceId).ExportFields[BOEExporterConstants.FieldName_ResourceName];

						Rollup.Year = i;

						Rollup.January = GetRollupForMonth(odcSpreads, i, 1); //, rateTypeFilter);
						Rollup.February = GetRollupForMonth(odcSpreads, i, 2); //, rateTypeFilter);
						Rollup.March = GetRollupForMonth(odcSpreads, i, 3); //, rateTypeFilter);
						Rollup.April = GetRollupForMonth(odcSpreads, i, 4); //, rateTypeFilter);
						Rollup.May = GetRollupForMonth(odcSpreads, i, 5); //, rateTypeFilter);
						Rollup.June = GetRollupForMonth(odcSpreads, i, 6); //, rateTypeFilter);
						Rollup.July = GetRollupForMonth(odcSpreads, i, 7); //, rateTypeFilter);
						Rollup.August = GetRollupForMonth(odcSpreads, i, 8); //, rateTypeFilter);
						Rollup.September = GetRollupForMonth(odcSpreads, i, 9); //, rateTypeFilter);
						Rollup.October = GetRollupForMonth(odcSpreads, i, 10); //, rateTypeFilter);
						Rollup.November = GetRollupForMonth(odcSpreads, i, 11); //, rateTypeFilter);
						Rollup.December = GetRollupForMonth(odcSpreads, i, 12); //, rateTypeFilter);

						RollupByDate[resourceId].Add(Rollup);
					}
				}
			}
			return RollupByDate;
		}

		/// <summary>
		/// Get Travel Cost Rollup
		/// </summary>
		/// <param name="TravelElements">Travel elements</param>
		/// <param name="elements">boe export task elements</param>
		/// <param name="TravelResources">Travel resources</param>
		/// <param name="useGfy">use government fiscal year?</param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected Dictionary<int, List<LaborRollupByDateNew>> GetTravelCostRollup(Collection<TravelDTO> TravelElements, Collection<BOEExportTaskElement> elements, Collection<ResourceDTO> TravelResources, bool useGfy)
		{
			Dictionary<int, List<LaborRollupByDateNew>> RollupByDate = new Dictionary<int, List<LaborRollupByDateNew>>();

			DateRange TravelElementsDateRange = GetODCTravelDateRange(null, TravelElements, useGfy);
			if (TravelElementsDateRange.StartDate.HasValue && TravelElementsDateRange.EndDate.HasValue)
			{
				List<TravelTripType> TravelTrips = TravelElements.SelectMany(x => x.TravelTrips).ToList();
				var TravelGroups = from f in TravelTrips
								   group f by new
								   {
									   f.Segment
								   } into g
								   select new { g.Key.Segment };

				ICollection<BOEExportTaskElement> travelExportElements = elements.Where(g => g.ElementType == BOEExportTaskElementType.Travel).ToList();

				foreach (var item in TravelGroups)
				{
					RollupByDate.Add((int)item.Segment, new List<LaborRollupByDateNew>());

					ICollection<TravelTripType> travelTripsBySegment = (from f in TravelTrips where f.Segment == item.Segment select f).ToList().DeepClone();

					if (useGfy)
					{
						foreach (TravelTripType trip in travelTripsBySegment)
						{
							trip.TripDate = this.AdjustDateForGovtFiscalYear(trip.TripDate);
						}
					}

					for (int i = TravelElementsDateRange.StartDate.Value.Year; i <= TravelElementsDateRange.EndDate.Value.Year; i++)
					{
						LaborRollupByDateNew Rollup = new LaborRollupByDateNew();
						ResourceDTO Resource = TravelResources.FirstOrDefault(x => x.Segment == item.Segment);
						Rollup.Year = i;
						Rollup.Resource = Resource == null ? string.Empty : Resource.ResourceName;

						Rollup.January = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 1);
						Rollup.February = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 2);
						Rollup.March = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 3);
						Rollup.April = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 4);
						Rollup.May = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 5);
						Rollup.June = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 6);
						Rollup.July = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 7);
						Rollup.August = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 8);
						Rollup.September = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 9);
						Rollup.October = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 10);
						Rollup.November = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 11);
						Rollup.December = GetRollupForMonth(travelTripsBySegment, travelExportElements, i, 12);

						RollupByDate[(int)item.Segment].Add(Rollup);
					}
				}
			}

			return RollupByDate;
		}

		private decimal GetRollupForMonth(ICollection<TravelTripType> travelTripsBySegment, ICollection<BOEExportTaskElement> travelExportElements, int year, int month)  //, RateType? rateTypeFilter = null)
		{
			decimal sum =
				(from e in travelTripsBySegment
				 where e.TripDate.Year == year && e.TripDate.Month == month
				 select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[BOEExporterConstants.FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();

			return sum;
		}

		/// <summary>
		/// Gets the task hour rollup.
		/// </summary>
		/// <param name="taskElements">The task elements.</param>
		/// <param name="labors">The labors.</param>
		/// <param name="elementOfCostFilter">The element of cost filter.</param>
		/// <param name="rateTypeFilter">The rate type filter.</param>
		/// <param name="useGfy">Should Government Fiscal Years be used</param>
		/// <returns>List of labor rolled up by date.</returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected Dictionary<int, List<LaborRollupByDateNew>> GetTaskHourRollup(Collection<BoeTaskElementDTO> taskElements, Collection<BOEExportTaskElementLabor> labors, BOEExportModelView boeExportModelView, bool useDescriptionInsteadOfName, bool useGfy, ElementOfCostType? elementOfCostFilter = null, RateType? rateTypeFilter = null)
		{
			// Each Labor task element contains a set of resources, each mapped to its own Element of Cost (i.e. LM Labor, IWTA or Sub) and rate type (i.e. Hours vs. Cost)
			string elementOfCostFilterString = elementOfCostFilter.HasValue ? elementOfCostFilter.Value.ToString() : null;
			string rateTypeFilterString = rateTypeFilter.HasValue ? rateTypeFilter.Value.ToString() : null;

			Dictionary<int, List<LaborRollupByDateNew>> RollupByDate = new Dictionary<int, List<LaborRollupByDateNew>>();
			DateRange TaskElementsDateRange = GetTaskDateRange(taskElements, boeExportModelView, useGfy);
			if (TaskElementsDateRange.StartDate.HasValue && TaskElementsDateRange.EndDate.HasValue)
			{
				List<ResourceTypeDto> TaskElementLabors = taskElements.SelectMany(x => x.taskElementLabors).ToList();

				ICollection<int> resourceIds =
					(from f in TaskElementLabors.Where(t => t.ResourceID.HasValue)
					 group f by f.ResourceID.Value into g
					 select g.Key).ToList();

				foreach (int resourceId in resourceIds)
				{
					string resourceIdString = resourceId.ToString();
					BOEExportTaskElementLabor resourceEntry = labors.FirstOrDefault(x => x.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceID) && x.ExportFields[BOEExporterConstants.FieldName_ResourceID] == resourceIdString);
					string resourceName = useDescriptionInsteadOfName ? resourceEntry.ExportFields[BOEExporterConstants.FieldName_ResourceDescription]
						: resourceEntry.ExportFields[BOEExporterConstants.FieldName_ResourceName];

					string resourceElementOfCostString = resourceEntry.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost];
					string resourceRateTypeString = resourceEntry.ExportFields[BOEExporterConstants.FieldName_ResourceRateType];

					// apply element of cost and rate type filters
					if (elementOfCostFilterString != null && elementOfCostFilterString != resourceElementOfCostString)
					{
						continue;  // skip any resources that do not match the element of cost filter
					}
					else if (rateTypeFilterString != null && rateTypeFilterString != resourceRateTypeString)
					{
						continue;  // skip any resources that do not match the rate type filter
					}
					else
					{
						RateType rateTypeResolved = rateTypeFilter.HasValue ? rateTypeFilter.Value : resourceRateTypeString.GetEnumeratedValue<RateType>(RateType.NotSet);
						if (rateTypeResolved == RateType.Cost)
						{
							continue;  // this method only handles Hour-type resources
						}
					}

					RollupByDate.Add(resourceId, new List<LaborRollupByDateNew>());

					ICollection<ResourceSpreadDto> taskElementLaborsByResource = (from b in TaskElementLabors
																				  where b.ResourceID == resourceId
																				  select b.LaborSpreads).SelectMany(x => x).ToList().DeepClone();

					if (useGfy)
					{
						foreach (ResourceSpreadDto spread in taskElementLaborsByResource)
						{
							spread.LaborSpreadDate = this.AdjustDateForGovtFiscalYear(spread.LaborSpreadDate);
						}
					}

					for (int i = TaskElementsDateRange.StartDate.Value.Year; i <= TaskElementsDateRange.EndDate.Value.Year; i++)
					{
						LaborRollupByDateNew Rollup = new LaborRollupByDateNew();

						Rollup.Resource = resourceName;

						Rollup.Year = i;

						Rollup.January = GetRollupForMonth(taskElementLaborsByResource, i, 1);
						Rollup.February = GetRollupForMonth(taskElementLaborsByResource, i, 2);
						Rollup.March = GetRollupForMonth(taskElementLaborsByResource, i, 3);
						Rollup.April = GetRollupForMonth(taskElementLaborsByResource, i, 4);
						Rollup.May = GetRollupForMonth(taskElementLaborsByResource, i, 5);
						Rollup.June = GetRollupForMonth(taskElementLaborsByResource, i, 6);
						Rollup.July = GetRollupForMonth(taskElementLaborsByResource, i, 7);
						Rollup.August = GetRollupForMonth(taskElementLaborsByResource, i, 8);
						Rollup.September = GetRollupForMonth(taskElementLaborsByResource, i, 9);
						Rollup.October = GetRollupForMonth(taskElementLaborsByResource, i, 10);
						Rollup.November = GetRollupForMonth(taskElementLaborsByResource, i, 11);
						Rollup.December = GetRollupForMonth(taskElementLaborsByResource, i, 12);

						RollupByDate[resourceId].Add(Rollup);
					}
				}
			}
			return RollupByDate;
		}

		/// <summary>
		/// Gets the labor task cost rollup.
		/// </summary>
		/// <param name="taskElements">The task elements.</param>
		/// <param name="labors">The labors.</param>
		/// <param name="elementOfCostFilter">The element of cost filter.</param>
		/// <param name="rateTypeFilter">The rate type filter.</param>
		/// <param name="useGfy">Should Government Fiscal Years be used</param>
		/// <returns>List of labor task cost rolled up.</returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected Dictionary<int, List<LaborRollupByDateNew>> GetLaborTaskCostRollup(Collection<BoeTaskElementDTO> taskElements, Collection<BOEExportTaskElementLabor> labors, BOEExportModelView boeExportModelView, bool useGfy, ElementOfCostType? elementOfCostFilter = null, RateType? rateTypeFilter = null)
		{
			// Each Labor task element contains a set of resources, each mapped to its own Element of Cost (i.e. LM Labor, IWTA or Sub) and rate type (i.e. Hours vs. Cost)
			string elementOfCostFilterString = elementOfCostFilter.HasValue ? elementOfCostFilter.Value.ToString() : null;
			string rateTypeFilterString = rateTypeFilter.HasValue ? rateTypeFilter.Value.ToString() : null;

			Dictionary<int, List<LaborRollupByDateNew>> RollupByDate = new Dictionary<int, List<LaborRollupByDateNew>>();
			DateRange TaskElementsDateRange = GetTaskDateRange(taskElements, boeExportModelView, useGfy);
			if (TaskElementsDateRange.StartDate.HasValue && TaskElementsDateRange.EndDate.HasValue)
			{
				List<ResourceTypeDto> TaskElementLabors = taskElements.SelectMany(x => x.taskElementLabors).ToList();

				ICollection<int> resourceIds =
					(from f in TaskElementLabors.Where(t => t.ResourceID.HasValue)
					 group f by f.ResourceID.Value into g
					 select g.Key).ToList();

				foreach (int resourceId in resourceIds)
				{
					string resourceIdString = resourceId.ToString();
					BOEExportTaskElementLabor resourceEntry = labors.FirstOrDefault(x => x.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceID) && x.ExportFields[BOEExporterConstants.FieldName_ResourceID] == resourceIdString);
					string resourceName = resourceEntry.ExportFields[BOEExporterConstants.FieldName_ResourceName];
					string resourceElementOfCostString = resourceEntry.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost];
					string resourceRateTypeString = resourceEntry.ExportFields[BOEExporterConstants.FieldName_ResourceRateType];

					// apply element of cost and rate type filters
					if (elementOfCostFilterString != null && elementOfCostFilterString != resourceElementOfCostString)
					{
						continue;  // skip any resources that do not match the element of cost filter
					}
					else if (rateTypeFilterString != null && rateTypeFilterString != resourceRateTypeString)
					{
						continue;  // skip any resources that do not match the rate type filter
					}
					else
					{
						RateType rateTypeResolved = rateTypeFilter.HasValue ? rateTypeFilter.Value : resourceRateTypeString.GetEnumeratedValue<RateType>(RateType.NotSet);
						if (rateTypeResolved == RateType.Hours)
						{
							continue;  // this method only handles Cost-type resources
						}
					}

					RollupByDate.Add(resourceId, new List<LaborRollupByDateNew>());

					ICollection<ResourceSpreadDto> taskElementLaborsByResource = (from b in TaskElementLabors
																				  where b.ResourceID == resourceId
																				  select b.LaborSpreads).SelectMany(x => x).ToList().DeepClone();

					if (useGfy)
					{
						foreach (ResourceSpreadDto spread in taskElementLaborsByResource)
						{
							spread.LaborSpreadDate = this.AdjustDateForGovtFiscalYear(spread.LaborSpreadDate);
						}
					}

					for (int i = TaskElementsDateRange.StartDate.Value.Year; i <= TaskElementsDateRange.EndDate.Value.Year; i++)
					{
						LaborRollupByDateNew Rollup = new LaborRollupByDateNew();

						Rollup.Resource = resourceName;

						Rollup.Year = i;

						Rollup.January = GetRollupForMonth(taskElementLaborsByResource, i, 1);
						Rollup.February = GetRollupForMonth(taskElementLaborsByResource, i, 2);
						Rollup.March = GetRollupForMonth(taskElementLaborsByResource, i, 3);
						Rollup.April = GetRollupForMonth(taskElementLaborsByResource, i, 4);
						Rollup.May = GetRollupForMonth(taskElementLaborsByResource, i, 5);
						Rollup.June = GetRollupForMonth(taskElementLaborsByResource, i, 6);
						Rollup.July = GetRollupForMonth(taskElementLaborsByResource, i, 7);
						Rollup.August = GetRollupForMonth(taskElementLaborsByResource, i, 8);
						Rollup.September = GetRollupForMonth(taskElementLaborsByResource, i, 9);
						Rollup.October = GetRollupForMonth(taskElementLaborsByResource, i, 10);
						Rollup.November = GetRollupForMonth(taskElementLaborsByResource, i, 11);
						Rollup.December = GetRollupForMonth(taskElementLaborsByResource, i, 12);

						RollupByDate[resourceId].Add(Rollup);
					}
				}
			}

			return RollupByDate;
		}

		/// <summary>
		/// Gets the task cost rollup.
		/// </summary>
		/// <param name="taskElements">The task elements.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="resources">The resources.</param>
		/// <param name="useGfy">use government fiscal year?</param>
		/// <returns>List of task cost rolled up.</returns>
		/// <exception cref="ArgumentNullException">exportInputs</exception>

		protected virtual List<LaborRollupByDateNew> GetTaskCostRollup(ICollection<BoeTaskElementDTO> taskElements, BOEExportInputs exportInputs, Collection<ResourceDTO> resources, bool useGfy)
		{
			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			Dictionary<int, ValuesByMonth<decimal>> yearlyData = new Dictionary<int, ValuesByMonth<decimal>>();

			List<ResourceTypeDto> allTaskElementResources;
			if (resources == null)
			{
				allTaskElementResources = taskElements.SelectMany(x => x.taskElementLabors).ToList().DeepClone();
			}
			else
			{
				// ignore task resources which are not assigned under the designated Element of Cost
				ICollection<int> resourceIds = resources.Select(r => r.Id).ToList();
				allTaskElementResources = taskElements.SelectMany(x => x.taskElementLabors).Where(r => r.ResourceID.HasValue && resourceIds.Contains(r.ResourceID.Value)).ToList();
			}

			foreach (ResourceTypeDto resource in allTaskElementResources)
			{
				foreach (ResourceSpreadDto spread in resource.LaborSpreads)
				{
					// normalize the spread date for comparison against the resource rate schedule
					DateTime spreadDate = GenBOEUtilities.AdjustDateTimePrecision(spread.LaborSpreadDate, DateTimePrecision.Month);

					if (useGfy)
					{
						spreadDate = AdjustDateForGovtFiscalYear(spreadDate);
					}

					int spreadYear = spreadDate.Year;
					int spreadMonth = spreadDate.Month;

					ValuesByMonth<decimal> monthlyValues;
					if (yearlyData.ContainsKey(spreadYear))
					{
						monthlyValues = yearlyData[spreadYear];
					}
					else
					{
						monthlyValues = new ValuesByMonth<decimal>();
						yearlyData[spreadYear] = monthlyValues;
					}

					decimal costValue = 0m;

					if (resource.SpreadType == SpreadType.Cost)
					{
						costValue = spread.LaborSpreadValue;
					}

					switch (spreadMonth)
					{
						case 1: monthlyValues.January += costValue; break;
						case 2: monthlyValues.February += costValue; break;
						case 3: monthlyValues.March += costValue; break;
						case 4: monthlyValues.April += costValue; break;
						case 5: monthlyValues.May += costValue; break;
						case 6: monthlyValues.June += costValue; break;
						case 7: monthlyValues.July += costValue; break;
						case 8: monthlyValues.August += costValue; break;
						case 9: monthlyValues.September += costValue; break;
						case 10: monthlyValues.October += costValue; break;
						case 11: monthlyValues.November += costValue; break;
						case 12: monthlyValues.December += costValue; break;
						default: break;
					}
				}
			}

			List<LaborRollupByDateNew> results = new List<LaborRollupByDateNew>(yearlyData.Count);
			foreach (KeyValuePair<int, ValuesByMonth<decimal>> yearlyDataEntry in yearlyData)
			{
				results.Add(new LaborRollupByDateNew(yearlyDataEntry.Key, yearlyDataEntry.Value));
			}

			return results.OrderBy(x => x.Year).ToList();
		}

		/// <summary>
		/// Gets the rollup for a month for labor type resource spreads.
		/// </summary>
		/// <param name="taskElementLaborsByResource">Resource spread.</param>
		/// <param name="year">Year for spread.</param>
		/// <param name="month">Month for spread.</param>
		/// <returns></returns>
		private decimal GetRollupForMonth(ICollection<ResourceSpreadDto> taskElementLaborsByResource, int year, int month)
		{
			return (from a in taskElementLaborsByResource
					where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == month
					select a.LaborSpreadValue).Sum();
		}

		/// <summary>
		/// Adjust the given date for Government Fiscal Year - Dates in October through December occur in the following fiscal year
		/// </summary>
		/// <param name="date">date to adjust</param>
		/// <returns>Adjusted date</returns>
		protected DateTime AdjustDateForGovtFiscalYear(DateTime date)
		{
			if (date.Month >= 10 && date.Month <= 12)
			{
				return date.AddYears(1);
			}
			else
			{
				return date;
			}
		}

		#endregion

		#region Date range determination

		/// <summary>
		/// Gets the task date range.
		/// </summary>
		/// <param name="taskElementCollection">The task element collection.</param>
		/// <param name="boeExportModelView">The boe export model view.</param>
		/// <param name="useGfy">Should Government Fiscal Years be used</param>
		/// <returns></returns>
		private DateRange GetTaskDateRange(Collection<BoeTaskElementDTO> taskElementCollection, BOEExportModelView boeExportModelView, bool useGfy)
		{
			if (taskElementCollection == null || !taskElementCollection.Any()) { return new DateRange(); }

			Tuple<BoeTaskElementDTO, BoeTaskElementDTO> earliestAndLatestTask = WordUtilities.GetEarliestAndLatestItems(taskElementCollection, x => x.StartDate.HasValue, x => x.StartDate.Value, x => x.EndDate.HasValue, x => x.EndDate.Value);
			BoeTaskElementDTO Earliest = earliestAndLatestTask.Item1;
			BoeTaskElementDTO Latest = earliestAndLatestTask.Item2;

			DateRange toReturn = new DateRange();

			if (Earliest != null && Latest != null)
			{
				try
				{
					toReturn = new DateRange(useGfy ? new DateTime(this.AdjustDateForGovtFiscalYear(Earliest.StartDate.Value).Year, 1, 15) : Earliest.StartDate,
						useGfy ? new DateTime(this.AdjustDateForGovtFiscalYear(Latest.EndDate.Value).Year, 12, 15) : Latest.EndDate);
				}
				catch (InvalidOperationException)
				{
					// get the date from the boe
					toReturn = new DateRange(useGfy ? new DateTime(this.AdjustDateForGovtFiscalYear(boeExportModelView.StartDate).Year, 1, 15) : boeExportModelView.StartDate,
						useGfy ? new DateTime(this.AdjustDateForGovtFiscalYear(Latest.EndDate.Value).Year, 12, 15) : boeExportModelView.EndDate);
				}
			}
			else
			{
				toReturn = new DateRange(useGfy ? new DateTime(this.AdjustDateForGovtFiscalYear(boeExportModelView.StartDate).Year, 1, 15) : boeExportModelView.StartDate,
						useGfy ? new DateTime(this.AdjustDateForGovtFiscalYear(boeExportModelView.EndDate).Year, 12, 15) : boeExportModelView.EndDate);
			}

			return toReturn;

		}

		/// <summary>
		/// Get combined date range for travel and odc elements
		/// </summary>
		/// <param name="ODCElements">ODC elements for task</param>
		/// <param name="TravelElements">Travel Elements for task</param>
		/// <param name="useGfy">use government fiscal year?</param>
		/// <returns>combined date range for travel and odc elements</returns>
		protected DateRange GetODCTravelDateRange(ICollection<OtherDirectCostDTO> ODCElements, ICollection<TravelDTO> TravelElements, bool useGfy)
		{
			OtherDirectCostDTO ODCEarliest = null;
			OtherDirectCostDTO ODCLatest = null;
			TravelDTO TravelEarliest = null;
			TravelDTO TravelLatest = null;

			if (ODCElements != null)
			{
				Tuple<OtherDirectCostDTO, OtherDirectCostDTO> earliestAndLatestODC = WordUtilities.GetEarliestAndLatestItems(ODCElements, x => x.StartDate.HasValue, x => x.StartDate.Value, x => x.EndDate.HasValue, x => x.EndDate.Value);
				ODCEarliest = earliestAndLatestODC.Item1;
				ODCLatest = earliestAndLatestODC.Item2;
			}

			if (TravelElements != null)
			{
				Tuple<TravelDTO, TravelDTO> earliestAndLatestTravel = WordUtilities.GetEarliestAndLatestItems(TravelElements, x => x.StartDate.HasValue, x => x.StartDate.Value, x => x.EndDate.HasValue, x => x.EndDate.Value);
				TravelEarliest = earliestAndLatestTravel.Item1;
				TravelLatest = earliestAndLatestTravel.Item2;
			}

			if (ODCEarliest != null && ODCLatest != null || TravelEarliest != null && TravelLatest != null)
			{
				DateTime? Earliest = ODCEarliest != null && TravelEarliest != null ? ODCEarliest.StartDate.Value < TravelEarliest.StartDate.Value ? ODCEarliest.StartDate.Value : TravelEarliest.StartDate.Value : ODCEarliest == null ? TravelEarliest.StartDate.Value : ODCEarliest.StartDate.Value;
				DateTime? Latest = ODCLatest != null && TravelLatest != null ? ODCLatest.EndDate.Value < TravelLatest.EndDate.Value ? TravelLatest.EndDate.Value : ODCLatest.EndDate.Value : ODCLatest == null ? TravelLatest.EndDate.Value : ODCLatest.EndDate.Value;

				if (useGfy)
				{
					if (Earliest.HasValue)
					{
						Earliest = AdjustDateForGovtFiscalYear(Earliest.Value);
					}

					if (Latest.HasValue)
					{
						Latest = AdjustDateForGovtFiscalYear(Latest.Value);
					}
				}

				return new DateRange(Earliest, Latest);
			}
			else
			{
				return new DateRange();
			}

		}

		/// <summary>
		/// Export the BOEs as individual files and zip into single download.
		/// </summary>
		/// <param name="webHostEnvironment">Web host env</param>
		/// <param name="exportInputs">The export inputs</param>
		/// <param name="boeExportModelViews">Collection of BOE View Models</param>
		/// <param name="boeSummaryGridModelViews"></param>
		/// <param name="components">Custom components</param>
		/// <param name="exportFormat">Export format DTO</param>
		public string ExportBOEsToZipFile(IWebHostEnvironment webHostEnvironment, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, 
			ICollection<BOESummaryGridModelView> boeSummaryGridModelViews, ICollection<BoeCustomReportComponent> components, 
			WorkspaceExportFormatDTO exportFormat)
		{
			return AllBOEExportHelper.ExportCustomComponentBOEsToZipFile<bool>(
				webHostEnvironment,
				exportInputs,
				boeExportModelViews,
				boeSummaryGridModelViews,
				components,
				exportFormat,
				ExportBOEToWordStream);
		}
		#endregion
	}
}
