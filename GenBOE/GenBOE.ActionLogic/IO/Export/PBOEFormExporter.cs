// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;
	using GenBOE.ActionLogic.BOE;
	using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;
    using IES.Common.PickList;

    /// <summary>
    /// Rolled-up Table-Row.
    /// </summary>
    public class PBOETableRow : ITableRow
    {
        /// <summary>
        /// The Contract type.
        /// </summary>
        public string ContractType { get; set; }

        /// <summary>
        /// The Supplier Contract Type.
        /// </summary>
        public string SupplierContractType { get; set; }

        /// <summary>
        /// The WBS Number.
        /// </summary>
        public string WBS { get; set; }

        /// <summary>
        /// The WBS Padded Number.
        /// </summary>
        public string WbsPaddedNumber { get; set; }

        /// <summary>
        /// The CLIN Name
        /// </summary>
        public string CLIN { get; set; }

        /// <summary>
        /// The rolled-up Value for this WBS/CLIN Combo.
        /// </summary>
        public decimal Value { get; set; }
    }


    /// <summary>
    /// Used for exporting an PBOE Custom Form to a pre-formatted Work template.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PBOEFormExporter : BOEFormExporter<BOEFormPBOEDTO, PBOETableRow>
    {
        #region Constants

        private const string SUPPLIER_CONTRACT_TYPE = "SupplierContractType";
        private const string SUPPLIER = "Supplier";
        private const string RFP = "RFP";
        private const string PROPOSAL_NUMBER = "ProposalNumber";
        private const string VALIDITY_DATE = "ValidityDate";
        private const string CCOPD = "ccopd";
        private const string CCOPD_YES = "ccopd_yes";
        private const string CCOPD_NO = "ccopd_no";
        private const string CCOPD_NA = "ccopd_na";
        private const string CCOPD_COMMERCIAL = "ccopd_commercial";
        private const string CCOPD_COMPETITION = "ccopd_competition";
        private const string CCOPD_THRESHOLD = "ccopd_threshold";
        private const string CCOPD_OTHER = "ccopd_other";
        private const string CCOPD_OTHER_TEXT = "ccopd_other_text";
        private const string VENDOR_ID = "VendorId";
        private const string SUPPLIER_PROPOSED_VALUE = "SupplierProposedValue";

        // Basis of and Rationale for LM Proposed Value
        private const string PROPOSED_SOURCE_SELECTION = "ProposedSourceSelection";
        private const string PROPOSED_COMMERCIALITY = "ProposedCommerciality";
        private const string PROPOSED_TECHNICAL_EVALUATION = "ProposedTechnicalEvaluation";
        private const string PROPOSED_PRICE_ANALYSIS = "ProposedPriceAnalysis";
        private const string PROPOSED_COST_ANALYSIS = "ProposedCostAnalysis";
        private const string PROPOSED_VALUE_SUMMARY = "ProposedValueSummary";

        // Schedule of Events
        private const string SHOULD_COST_ESTIMATE = "ShouldCostEstimate";
        private const string RFP_RELEASE = "RFPRelease";
        private const string FIRM_SUPPLIER = "FirmSupplier";
        private const string SOURCE_SELECTION = "SourceSelection";
        private const string CID = "CID";
        private const string PRICE_ANALYSIS = "PriceAnalysis";
        private const string TECHNICAL_EVALUATION = "TechnicalEvaluation";
        private const string FACT_FINDING = "FactFinding";
        private const string COST_ANALYSIS = "CostAnalysis";
        private const string COST_ANALYSIS_UNQUALIFIED = "CostAnalysisUnqualified";
        private const string GOVT_PRICING_CCOPD_REQUEST = "GovtPricingCCOPDRequest";
        private const string GOVT_PRICING_CCOPD_REVIEW = "GovtPricingCCOPDReceipt";
        private const string SUPPLIER_NEGOTIATIONS = "SupplierNegotiations";
        private const string MOU = "MOU";
        private const string PLANNED_DATE_A = "PlannedDate_A";
        private const string PLANNED_DATE_B = "PlannedDate_B";

        #endregion


        #region Fields
        /// <summary>
        /// The location of the Template
        /// </summary>
        protected override string TemplateLocation { get { return "~/Templates/Export/PBOE_{0}.docx"; } }

        /// <summary>
        /// The location of the Template with portion markings enabled
        /// </summary>
        protected override string TemplateLocationPortionMarking { get { return "~/Templates/Export/PBOE_withPortionMarkings_{0}.docx"; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor for PBOE Form Exporter
        /// </summary>
        /// <param name="userDTODataLoader">The user dataloader.</param>
        /// <param name="resourceDTODataLoader">The resource dataloader.</param>
        /// <param name="tmCalculator">The T&amp;M Calculator.</param>
        public PBOEFormExporter(IUserDTODataLoader userDTODataLoader, IResourceDTODataLoader resourceDTODataLoader, TMCalculator tmCalculator, IRetriever retriever)
            : base(userDTODataLoader, resourceDTODataLoader, tmCalculator, retriever)
        {
        }

        #endregion

        /// <summary>
        /// Populates the data export.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="document">The document.</param>
        /// <param name="boeForm">The boe form.</param>
        /// <param name="isPortionMarkingEnabled">True if portion marking is enabled; False otherwise.</param>
        /// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <exception cref="System.ArgumentNullException">workspace or document or boeForm</exception>
        protected override void PopulateDataExport(FullWorkspace workspace, WordprocessingDocument document, BOEFormPBOEDTO boeForm, bool isPortionMarkingEnabled, string proposalTitleAndRfpNumber, ICollection<PickListDto> contractTypes)
        {
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (ReferenceEquals(document, null))
            {
                throw new ArgumentNullException(nameof(document));
            }
            if (ReferenceEquals(boeForm, null))
            {
                throw new ArgumentNullException(nameof(boeForm));
            }

            ChunkCounter counters = new ChunkCounter();
            this.SetProprietaryLabels(document, workspace.ContainsOCI);

            ICollection<PBOETableRow> rows = this.PullRowsFromWorkspace(workspace, boeForm, boeForm.ResourceIds, contractTypes);

            // Set the fields that are displayed once
            this.SetField(document, EXPORT_DATE, boeForm.UpdateDate.ToString(BOEExporterConstants.DATE_FORMAT_STANDARD));
            this.SetField(document, REV, boeForm.Revision.ToString());
            this.SetField(document, SUPPLIER, boeForm.SupplierName);
            this.SetField(document, RFP, boeForm.RFP);
            this.SetField(document, PERIOD_OF_PERFORMANCE, this.PeriodOfPerformance);
            this.SetField(document, PROPOSAL_NUMBER, boeForm.ProposalNumber);
            this.SetField(document, PROPOSAL_DATE, boeForm.ProposalDate);
            this.SetField(document, VALIDITY_DATE, boeForm.ValidityDate);
            this.SetField(document, PROPOSAL_TITLE, string.IsNullOrWhiteSpace(proposalTitleAndRfpNumber) ? boeForm.ProposalTitle : proposalTitleAndRfpNumber);
            this.SetHtmlField(document, DESCRIPTION, boeForm.Description, ref counters);
            this.SetField(document, POC, boeForm.Poc);
            this.SetField(document, PHONE, boeForm.PocPhone);
            this.SetField(document, MANAGER, boeForm.Approver);
            this.SetField(document, MANAGER_PHONE, boeForm.ApproverPhone);
            this.SetField(document, VENDOR_ID, boeForm.VendorId);
            this.SetField(document, SUPPLIER_PROPOSED_VALUE, boeForm.SupplierProposedValue.HasValue ? boeForm.SupplierProposedValue.Value.ToString("C", CultureInfo.CurrentCulture) : String.Empty);

            // ccopd checkboxes
            this.SetCheckbox(document, CCOPD, boeForm.CCoPDApplies);
            this.SetCheckbox(document, CCOPD_YES, boeForm.SupplierCCoPD == TripleBooleanState.Yes);
            this.SetCheckbox(document, CCOPD_NO, boeForm.SupplierCCoPD == TripleBooleanState.No);
            this.SetCheckbox(document, CCOPD_NA, boeForm.SupplierCCoPD == TripleBooleanState.NA);
            this.SetCheckbox(document, CCOPD_COMMERCIAL, boeForm.CommercialItemExceptionApplies);
            this.SetCheckbox(document, CCOPD_COMPETITION, boeForm.CompetitionExceptionApplies);
            this.SetCheckbox(document, CCOPD_THRESHOLD, boeForm.LessThanThresholdExceptionApplies);
            this.SetCheckbox(document, CCOPD_OTHER, boeForm.OtherExceptionApplies);
            this.SetField(document, CCOPD_OTHER_TEXT, boeForm.OtherExceptionApplies ? boeForm.OtherText : string.Empty);

            // Schedule of events
            this.SetScheduleEventDateField(document, SHOULD_COST_ESTIMATE, boeForm.ShouldCostEstimate, boeForm.ShouldCostEstimateDate, boeForm.ShouldCostEstimateText);
            this.SetScheduleEventDateField(document, RFP_RELEASE, boeForm.RFPRelease, boeForm.RFPReleaseDate, boeForm.RFPReleaseText);
            this.SetScheduleEventDateField(document, FIRM_SUPPLIER, boeForm.FirmSupplierReceipt, boeForm.FirmSupplierReceiptDate, boeForm.FirmSupplierReceiptText);
            this.SetScheduleEventDateField(document, SOURCE_SELECTION, boeForm.SourceSelection, boeForm.SourceSelectionDate, boeForm.SourceSelectionText);
            this.SetScheduleEventDateField(document, CID, boeForm.CID, boeForm.CIDDate, boeForm.CIDText);
            this.SetScheduleEventDateField(document, PRICE_ANALYSIS, boeForm.PriceAnalysis, boeForm.PriceAnalysisDate, boeForm.PriceAnalysisText);
            this.SetScheduleEventDateField(document, TECHNICAL_EVALUATION, boeForm.TechnicalEvaluation, boeForm.TechnicalEvaluationDate, boeForm.TechnicalEvaluationText);
            this.SetScheduleEventDateField(document, FACT_FINDING, boeForm.FactFinding, boeForm.FactFindingDate, boeForm.FactFindingText);
            this.SetScheduleEventDateField(document, COST_ANALYSIS, boeForm.CostAnalysis, boeForm.CostAnalysisDate, boeForm.CostAnalysisText);
            this.SetScheduleEventDateField(document, GOVT_PRICING_CCOPD_REQUEST, boeForm.GovtPricing, boeForm.GovtPricingDate, boeForm.GovtPricingText);
            this.SetScheduleEventDateField(document, SUPPLIER_NEGOTIATIONS, boeForm.SupplierNegotiations, boeForm.SupplierNegotiationsDate, boeForm.SupplierNegotiationsText);
            this.SetScheduleEventDateField(document, MOU, boeForm.MOU, boeForm.MOUDate, boeForm.MOUText);
            // IES-1017: New fields, guard against older forms having null
            this.SetScheduleEventDateField(document, GOVT_PRICING_CCOPD_REVIEW, boeForm.GovtPricingReceived == null ? ScheduleEvent.NA : (ScheduleEvent)boeForm.GovtPricingReceived, boeForm.GovtPricingReceivedDate, boeForm?.GovtPricingReceivedText);
            this.SetScheduleEventDateField(document, COST_ANALYSIS_UNQUALIFIED, boeForm.CostAnalysisUnqual == null ? ScheduleEvent.NA : (ScheduleEvent)boeForm.CostAnalysisUnqual, boeForm.CostAnalysisUnqualDate, boeForm?.CostAnalysisUnqualText);
            this.SetDateField(document, PLANNED_DATE_A, boeForm.PlannedDate_WrittenApproval);
            this.SetDateField(document, PLANNED_DATE_B, boeForm.PlannedDate_ApprovedSubmission);
            
            // IES-1017: New RTE Fields
            this.SetHtmlField(document, PROPOSED_COMMERCIALITY, boeForm.CommercialityDescription, ref counters);
            this.SetHtmlField(document, PROPOSED_SOURCE_SELECTION, boeForm.SourceSelectionDescription, ref counters);
            this.SetHtmlField(document, PROPOSED_TECHNICAL_EVALUATION, boeForm.TechnicalEvaluationDescription, ref counters);
            this.SetHtmlField(document, PROPOSED_PRICE_ANALYSIS, boeForm.PriceAnalysisDescription, ref counters);
            this.SetHtmlField(document, PROPOSED_COST_ANALYSIS, boeForm.CostAnalysisDescription, ref counters);
            this.SetHtmlField(document, PROPOSED_VALUE_SUMMARY, boeForm.RationaleValueSummary, ref counters);

            IOrderedEnumerable<string> distinctCLINs = rows.Select<PBOETableRow, string>(tr => tr.CLIN).Distinct().OrderBy(c => c);

            // locate the table markers
            TableRow templateDataRow = this.GetTemplateDataRow(document, BOEExporterConstants.Marker_DataRow);
            TableRow templateSubtotalDataRow = this.GetTemplateDataRow(document, BOEExporterConstants.Marker_SubTotalsRow);
            TableRow templateTotalDataRow = this.GetTemplateDataRow(document, BOEExporterConstants.Marker_TotalsRow);

            // initialize the "insertion" row
            TableRow currentInsertionRow = templateDataRow;
            decimal total = 0m;
            foreach (string clin in distinctCLINs)
            {
                decimal subtotal = 0m;
                foreach (PBOETableRow row in rows.Where(r => r.CLIN == clin).OrderBy(r => r.WbsPaddedNumber))
                {
                    subtotal += Convert.ToDecimal(row.Value);
                    // create a new data row in the table
                    TableRow tableRow = this.CloneMarkedTemplateRow(templateDataRow);

					// populate the row
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, SUPPLIER_CONTRACT_TYPE), this.GenPortionMarkingText(isPortionMarkingEnabled) + row.SupplierContractType);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, WBS), this.GenPortionMarkingText(isPortionMarkingEnabled) + row.WBS);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, CLIN), this.GenPortionMarkingText(isPortionMarkingEnabled) + row.CLIN);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, VALUE), this.GenPortionMarkingText(isPortionMarkingEnabled) + row.Value.ToString(this.DefaultCurrencyFormat, this._CurrencyFormatter));

					// add the row to the table
					currentInsertionRow.InsertAfterSelf(tableRow);
					currentInsertionRow = tableRow;
				}

				// populate the sub-total (for the group)
				TableRow subtotalRow = this.CloneMarkedTemplateRow(templateSubtotalDataRow);
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(subtotalRow, CLIN), this.GenPortionMarkingText(isPortionMarkingEnabled) + "Subtotal " + clin);
				WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(subtotalRow, VALUE), this.GenPortionMarkingText(isPortionMarkingEnabled) + subtotal.ToString(this.DefaultCurrencyFormat, this._CurrencyFormatter));

				total += subtotal;

				// add the row to the table
				currentInsertionRow.InsertAfterSelf(subtotalRow);
				currentInsertionRow = subtotalRow;
			}

			// populate the total row
			TableRow totalRow = this.CloneMarkedTemplateRow(templateTotalDataRow);
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalRow, CLIN), this.GenPortionMarkingText(isPortionMarkingEnabled) + "Total");
			WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalRow, VALUE), this.GenPortionMarkingText(isPortionMarkingEnabled) + total.ToString(this.DefaultCurrencyFormat, this._CurrencyFormatter));

			// add the row to the table
			currentInsertionRow.InsertAfterSelf(totalRow);

            this.RemoveElement(templateDataRow);
            this.RemoveElement(templateSubtotalDataRow);
            this.RemoveElement(templateTotalDataRow);

            this.DocumentCleanup(document);
        }

        /// <summary>
        /// Creates a Table Row rollup for wbs/clin.
        /// </summary>
        /// <param name="boeForm">The boe form to pull information for the rollup row.</param>
        /// <param name="wbs">The wbs for the row.</param>
        /// <param name="clin">The Clin for the row.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <returns>
        /// New ITableRow for this wbs/clin combo.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">boeForm</exception>
        protected override PBOETableRow CreateRow(BOEFormPBOEDTO boeForm, WbsDTO wbs, ClinDTO clin, ICollection<PickListDto> contractTypes)
        {
            if (ReferenceEquals(boeForm, null))
            {
                throw new ArgumentNullException(nameof(boeForm));
            }

            string supplierContractType = Constants.CONTRACT_TYPE_NOT_SET_STRING;

            if (clin != null && boeForm.ClinContractTypes.Any(c => c.ClinId == clin.Id))
            {
                supplierContractType = Utilities.GetPickListText(boeForm.ClinContractTypes.First(c => c.ClinId == clin.Id).ContractType, contractTypes, Constants.CONTRACT_TYPE_NOT_SET_STRING);
            }

			return new PBOETableRow
			{
				CLIN = clin == null ? "N/A" : clin.ClinString,
				WBS = wbs == null ? "N/A" : wbs.WbsNumber,
				WbsPaddedNumber = wbs == null ? "N/A" : wbs.WbsPaddedNumber,
				ContractType = clin == null ? "N/A" : Utilities.GetPickListText(clin.ContractType, contractTypes, Constants.CONTRACT_TYPE_NOT_SET_STRING),
				SupplierContractType = supplierContractType
			};
        }

        /// <summary>
        /// Sets a checkbox to be selected or unselected.
        /// </summary>
        /// <param name="document">The word document edited via openxml.</param>
        /// <param name="field">Name of the Content Control in Word Document.</param>
        /// <param name="isChecked">Whether the checkbox should be selected.</param>
        private void SetCheckbox(WordprocessingDocument document, string field, bool isChecked)
        {
            if (!isChecked)
            {
                BookmarkStart bookmark = document.MainDocumentPart.Document.Descendants<BookmarkStart>().FirstOrDefault(s => s.Name == field);
                if (bookmark != null)
                {
                    DefaultCheckBoxFormFieldState checkbox = bookmark.PreviousSibling().Descendants<DefaultCheckBoxFormFieldState>().First();
                    checkbox.Val.Value = false;
                    checkbox.Val.InnerText = "0";
                }
            }
        }

        /// <summary>
        /// Sets a schedule of event date field.
        /// </summary>
        /// <param name="document">The word document edited via openxml.</param>
        /// <param name="field">Name of the Content Control in Word Document.</param>
        /// <param name="scheduleEvent">The schedule event type.</param>
        /// <param name="scheduleEventDate">The date for the scheduled event.</param>
        private void SetScheduleEventDateField(WordprocessingDocument document, string field, ScheduleEvent scheduleEvent, DateTime? scheduleEventDate, string scheduleEventPlannedText)
        {
            SdtElement dataElement = WordUtilities.GetTaggedElement(document, field);

            if (dataElement != null)
            {
                string value = string.Empty;
                if (scheduleEvent == ScheduleEvent.NA)
                {
                    value = "NA";
                }
                else
                {
                    if (scheduleEvent == ScheduleEvent.Actual && scheduleEventDate.HasValue)
                    {
                        value = scheduleEventDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_STANDARD) + " (a)";
                    }
                    else if (scheduleEvent == ScheduleEvent.Planned)
                    {
                        if (scheduleEventDate.HasValue)
                        {
                            value = scheduleEventDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_STANDARD) + " (p)";
                        }
                        else if (!string.IsNullOrWhiteSpace(scheduleEventPlannedText))
                        {
                            value = scheduleEventPlannedText + " (p)";
                        }
                    }
                }
                WordUtilities.SetElementText(dataElement, value);
            }
        }

        /// <summary>
        /// Sets a date field.
        /// </summary>
        /// <param name="document">The word document edited via openxml.</param>
        /// <param name="field">Name of the Content Control in Word Document.</param>
        /// <param name="dateValue">The date for the template.</param>
        private void SetDateField(WordprocessingDocument document, string field, DateTime? dateValue)
        {
            SdtElement dataElement = WordUtilities.GetTaggedElement(document, field);

            if (dataElement != null)
            {
                string value = string.Empty;
                if (dateValue.HasValue)
                {
                    value = dateValue.Value.ToString(BOEExporterConstants.DATE_FORMAT_STANDARD);
                }
                WordUtilities.SetElementText(dataElement, value);
            }
        }

		/// <summary>
		/// Transforms a PBOEViewModel into a BOEFormPBOEDTO so exporter can use it
		/// </summary>
		/// <param name="pboe">PBOEViewModel</param>
		/// <returns>
		/// BOEFormPBOEDTO
		/// </returns>
		public BOEFormPBOEDTO transformPBOEViewToFormDTO(PBOEViewModel pboe)
		{

			// Validations
			//already validated but wasn't good enough from VS
			_ = pboe ?? throw new ArgumentNullException(nameof(pboe));

			BOEFormPBOEDTO pboeForm = new BOEFormPBOEDTO()
			{
				CCoPDApplies = pboe.CCoPD == ExpectedCCoPDApplicability.CCoPDApplies,
				CommercialItemExceptionApplies = pboe.CCoPD == ExpectedCCoPDApplicability.CommercialItemExceptionApplies,
				CompetitionExceptionApplies = pboe.CCoPD == ExpectedCCoPDApplicability.CompetitionExceptionApplies,
				LessThanThresholdExceptionApplies = pboe.CCoPD == ExpectedCCoPDApplicability.ThresholdExceptionApplies,
				OtherExceptionApplies = pboe.CCoPD == ExpectedCCoPDApplicability.OtherExceptionApplies,
				OtherText = pboe.CCoPDOtherText,
				RFP = pboe.RFP,
				ProposalNumber = pboe.ProposalNumber,
				SupplierName = pboe.SupplierName,
				ValidityDate = pboe.ValidityDate,
				VendorId = pboe.VendorId,
				SupplierProposedValue = pboe.SupplierProposedValue,
				ShouldCostEstimate = pboe.ShouldCostEstimate,
				ShouldCostEstimateDate = pboe.ShouldCostEstimateDate,
				ShouldCostEstimateText = pboe.ShouldCostEstimateText,
				RFPRelease = pboe.RFPRelease,
				RFPReleaseDate = pboe.RFPReleaseDate,
				RFPReleaseText = pboe.RFPReleaseText,
				FirmSupplierReceipt = pboe.FirmSupplierReceipt,
				FirmSupplierReceiptDate = pboe.FirmSupplierReceiptDate,
				FirmSupplierReceiptText = pboe.FirmSupplierReceiptText,
				SourceSelection = pboe.SourceSelection,
				SourceSelectionDate = pboe.SourceSelectionDate,
				SourceSelectionText = pboe.SourceSelectionText,
				CID = pboe.CID,
				CIDDate = pboe.CIDDate,
				CIDText = pboe.CIDText,
				PriceAnalysis = pboe.PriceAnalysis,
				PriceAnalysisDate = pboe.PriceAnalysisDate,
				PriceAnalysisText = pboe.PriceAnalysisText,
				TechnicalEvaluation = pboe.TechnicalEvaluation,
				TechnicalEvaluationDate = pboe.TechnicalEvaluationDate,
				TechnicalEvaluationText = pboe.TechnicalEvaluationText,
				FactFinding = pboe.FactFinding,
				FactFindingDate = pboe.FactFindingDate,
				FactFindingText = pboe.FactFindingText,
				CostAnalysis = pboe.CostAnalysis,
				CostAnalysisDate = pboe.CostAnalysisDate,
				CostAnalysisText = pboe.CostAnalysisText,
				GovtPricing = pboe.GovtPricing,
				GovtPricingDate = pboe.GovtPricingDate,
				GovtPricingText = pboe.GovtPricingText,
				SupplierNegotiations = pboe.SupplierNegotiations,
				SupplierNegotiationsDate = pboe.SupplierNegotiationsDate,
				SupplierNegotiationsText = pboe.SupplierNegotiationsText,
				MOU = pboe.MOU,
				MOUDate = pboe.MOUDate,
				MOUText = pboe.MOUText,
				GovtPricingReceived = pboe.GovtPricingReceived,
				GovtPricingReceivedDate = pboe.GovtPricingReceivedDate,
				GovtPricingReceivedText = pboe.GovtPricingReceivedText,
				CostAnalysisUnqual = pboe.CostAnalysisUnqual,
				CostAnalysisUnqualDate = pboe.CostAnalysisUnqualDate,
				CostAnalysisUnqualText = pboe.CostAnalysisUnqualText,
				PlannedDate_WrittenApproval = pboe.PlannedDate_WrittenApproval,
				PlannedDate_ApprovedSubmission = pboe.PlannedDate_ApprovedSubmission,
				SupplierCCoPD = pboe.SupplierCCoPD,
				SourceSelectionDescription = pboe.SourceSelectionDescription,
				CommercialityDescription = pboe.CommercialityDescription,
				TechnicalEvaluationDescription = pboe.TechnicalEvaluationDescription,
				PriceAnalysisDescription = pboe.PriceAnalysisDescription,
				CostAnalysisDescription = pboe.CostAnalysisDescription,
				RationaleValueSummary = pboe.RationaleValueSummary
			};

			return pboeForm;
		}
	}
}
