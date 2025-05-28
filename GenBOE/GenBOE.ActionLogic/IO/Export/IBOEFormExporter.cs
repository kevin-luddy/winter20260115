// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
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
	public class IBOETableRow : ITableRow
	{
		/// <summary>
		/// The Contract type.
		/// </summary>
		public string ContractType { get; set; }

		/// <summary>
		/// The IWTA Contract Type.
		/// </summary>
		public string IWTAType { get; set; }

		/// <summary>
		/// The WBS Number.
		/// </summary>
		public string WBS { get; set; }

		/// <summary>
		/// The WBS Padded Number.
		/// </summary>
		public string WbsPaddedNumber { get; set; }

		/// <summary>
		/// The CLIN Name (Number and Title)
		/// </summary>
		public string CLIN { get; set; }

		/// <summary>
		/// The CLIN Number
		/// </summary>
		public string ClinNumber { get; set; }

		/// <summary>
		/// The rolled-up Value for this WBS/CLIN Combo.
		/// </summary>
		public decimal Value { get; set; }
	}


	/// <summary>
	/// Used for exporting an IBOE Custom Form to a pre-formatted Work template.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class IBOEFormExporter : BOEFormExporter<BOEFormIBOEDTO, IBOETableRow>
	{
		#region Constants
		protected const string BUSINESS_AREA = "BusinessArea";
		protected const string IWTA_TYPE = "IWTAType";
		
		#endregion Constants

		#region Fields
		/// <summary>
		/// The location of the Template
		/// </summary>
		protected override string TemplateLocation { get { return "~/Templates/Export/IBOE_{0}.docx"; } }

		/// <summary>
		/// The location of the Template with portion marking
		/// </summary>
		protected override string TemplateLocationPortionMarking { get { return "~/Templates/Export/IBOE_withPortionMarkings_{0}.docx"; } }
		#endregion

		#region Constructor

		/// <summary>
		/// Default constructor for IBOE Form Exporter
		/// </summary>
		/// <param name="userDTODataLoader">The user dataloader.</param>
		/// <param name="resourceDTODataLoader">The resource dataloader.</param>
		/// <param name="tmCalculator">The T&amp;M Calculator.</param>
		public IBOEFormExporter(IUserDTODataLoader userDTODataLoader, IResourceDTODataLoader resourceDTODataLoader, TMCalculator tmCalculator)
			: base(userDTODataLoader, resourceDTODataLoader, tmCalculator)
		{
		}

		#endregion

		/// <summary>
		/// Populates the Word export template.
		/// </summary>
		/// <param name="workspace">Full workspace.</param>
		/// <param name="document">The document.</param>
		/// <param name="boeForm">The BOE Form instance to export.</param>
		/// <param name="isPortionMarkingEnabled">True if portion marking is enabled; False otherwise.</param>
		/// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace.</param>
		/// <param name="contractTypes">The contract types.</param>
		/// <exception cref="System.ArgumentNullException">workspace or document or boeForm</exception>
		protected override void PopulateDataExport(FullWorkspace workspace, WordprocessingDocument document, BOEFormIBOEDTO boeForm, bool isPortionMarkingEnabled, string proposalTitleAndRfpNumber, ICollection<PickListDto> contractTypes)
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

			ICollection<IBOETableRow> rows = this.PullRowsFromWorkspace(workspace, boeForm, boeForm.ResourceIds, contractTypes);

			// Set the fields that are displayed once
			this.SetField(document, EXPORT_DATE, boeForm.UpdateDate.ToString(BOEExporterConstants.DATE_FORMAT_STANDARD));
			this.SetField(document, REV, boeForm.Revision.ToString());
			this.SetField(document, BUSINESS_AREA, boeForm.BusinessArea);
			this.SetField(document, PROPOSAL_DATE, boeForm.ProposalDate);
			this.SetField(document, PERIOD_OF_PERFORMANCE, this.PeriodOfPerformance);
			this.SetField(document, PROPOSAL_TITLE, string.IsNullOrWhiteSpace(proposalTitleAndRfpNumber) ? boeForm.ProposalTitle : proposalTitleAndRfpNumber);
			this.SetHtmlField(document, DESCRIPTION, boeForm.Description, ref counters);
			this.SetHtmlField(document, BASIS_RATIONALE, boeForm.BasisAndRationale, ref counters);
			this.SetField(document, POC, boeForm.Poc);
			this.SetField(document, PHONE, boeForm.PocPhone);
			this.SetField(document, MANAGER, boeForm.Approver);
			this.SetField(document, MANAGER_PHONE, boeForm.ApproverPhone);

			IOrderedEnumerable<string> distinctCLINs = rows.Select<IBOETableRow, string>(tr => tr.CLIN).Distinct().OrderBy(c => c);

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
				foreach (IBOETableRow row in rows.Where(r => r.CLIN == clin).OrderBy(r => r.WbsPaddedNumber))
				{
					subtotal += Convert.ToDecimal(row.Value);
					// create a new data row in the table
					TableRow tableRow = this.CloneMarkedTemplateRow(templateDataRow);

					// populate the row
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, CONTRACT_TYPE), this.GenPortionMarkingText(isPortionMarkingEnabled) + row.ContractType);
					WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, IWTA_TYPE), this.GenPortionMarkingText(isPortionMarkingEnabled) + row.IWTAType);
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
		protected override IBOETableRow CreateRow(BOEFormIBOEDTO boeForm, WbsDTO wbs, ClinDTO clin, ICollection<PickListDto> contractTypes)
		{
			if (ReferenceEquals(boeForm, null))
			{
				throw new ArgumentNullException(nameof(boeForm));
			}

			string iwtaContractType = Constants.CONTRACT_TYPE_NOT_SET_STRING;

			if (clin != null && boeForm.ClinContractTypes.Any(c => c.ClinId == clin.Id))
			{
				iwtaContractType = Utilities.GetPickListText(boeForm.ClinContractTypes.First(c => c.ClinId == clin.Id).ContractType, contractTypes, Constants.CONTRACT_TYPE_NOT_SET_STRING);
			}

			return new IBOETableRow
			{
				CLIN = clin == null ? "N/A" : clin.ClinString,
				ClinNumber = clin == null ? "N/A" : clin.ClinNumber,
				WBS = wbs == null ? "N/A" : wbs.WbsNumber,
				WbsPaddedNumber = wbs == null ? "N/A" : wbs.WbsPaddedNumber,
				ContractType = clin == null ? "N/A" : Utilities.GetPickListText(clin.ContractType, contractTypes, Constants.CONTRACT_TYPE_NOT_SET_STRING),
				IWTAType = iwtaContractType
			};
		}

		/// <summary>
		/// Transforms a IBOEViewModel into a BOEFormIBOEDTO so exporter can use it
		/// </summary>
		/// <param name="iboe"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public BOEFormIBOEDTO TransformIBOEViewToFormDTO(IBOEViewModel iboe)
		{
			// Validations
			_ = iboe ?? throw new ArgumentNullException(nameof(iboe));

			BOEFormIBOEDTO iboeForm = new BOEFormIBOEDTO()
			{
				BusinessArea = iboe.BusinessArea,
				PTMTrackingNumber = iboe.PTMTrackingNumber,
				FormName = iboe.FormName,
				ProposalDate = iboe.ProposalDate,
				ProposalTitle = iboe.ProposalTitle,
				Description = iboe.Description,
				BasisAndRationale = iboe.BasisAndRationale,
			};

			return iboeForm;
		}
	}
}
