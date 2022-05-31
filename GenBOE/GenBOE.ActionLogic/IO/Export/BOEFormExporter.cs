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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;
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
    public interface ITableRow
    {
        /// <summary>
        /// The WBS Number.
        /// </summary>
        string WBS { get; set; }

        /// <summary>
        /// The WBS Padded Number.
        /// </summary>
        string WbsPaddedNumber { get; set; }

        /// <summary>
        /// The CLIN Name
        /// </summary>
        string CLIN { get; set; }

        /// <summary>
        /// The rolled-up Value for this WBS/CLIN Combo.
        /// </summary>
        decimal Value { get; set; }
    }

    /// <summary>
    /// Used for exporting an IBOE Custom Form to a pre-formatted Work template.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class BOEFormExporter<T, TK> : WordExporter where T : BOEFormDTO where TK : class, ITableRow
    {
        #region Constants
        protected const string FOOTER_LMPI = "LMPI";
        protected const string PROPOSAL_DATE = "ProposalDate";
        protected const string PERIOD_OF_PERFORMANCE = "POP";
        protected const string PROPOSAL_TITLE = "ProposalTitle";
        protected const string DESCRIPTION = "TaskDescription";
        protected const string BASIS_RATIONALE = "BasisAndRationale";
        protected const string POC = "POC";
        protected const string PHONE = "Phone";
        protected const string MANAGER = "Manager";
        protected const string MANAGER_PHONE = "ManagerPhone";
        protected const string EXPORT_DATE = "ExportDate";
        protected const string REV = "Rev";
        protected const string CONTRACT_TYPE = "ContractType";
        protected const string WBS = "WBS";
        protected const string CLIN = "CLIN";
        protected const string VALUE = "Value";

        #endregion Constants

        #region Fields
        /// <summary>
        /// The currency formatter for strings.
        /// </summary>
        protected NumberFormatInfo _CurrencyFormatter { get; set; }
        /// <summary>
        /// The default currency format
        /// </summary>
        protected string DefaultCurrencyFormat { get; set; }

        /// <summary>
        /// The location of the Template
        /// </summary>
        protected abstract string TemplateLocation { get; }

        /// <summary>
        /// The location of the Template with portion marking enabled
        /// </summary>
        protected abstract string TemplateLocationPortionMarking { get; }

        /// <summary>
        /// Gets the period of performance.
        /// </summary>
        protected string PeriodOfPerformance { get; private set; }

        /// <summary>
        /// The Resource data loader.
        /// </summary>
        private IResourceDTODataLoader resourceLoader;

        /// <summary>
        /// T&amp;M rate calculator.
        /// </summary>
        private TMCalculator tmCalculator;
        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor for BOE Form Exporter
        /// </summary>
        /// <param name="userDTODataLoader">The user dataloader.</param>
        /// <param name="resourceDTODataLoader">The resource dataloader.</param>
        /// <param name="tmCalculator">The T&amp;M Calculator.</param>
        protected BOEFormExporter(IUserDTODataLoader userDTODataLoader, IResourceDTODataLoader resourceDTODataLoader, TMCalculator tmCalculator)
            : base(userDTODataLoader)
        {
            this._CurrencyFormatter = new NumberFormatInfo();
            this._CurrencyFormatter.CurrencyNegativePattern = 1;
            this._CurrencyFormatter.CurrencySymbol = "$";
            this.DefaultCurrencyFormat = BOEExporterConstants.CURRENCY_FORMAT_DEFAULT;
            this.resourceLoader = resourceDTODataLoader;
            this.tmCalculator = tmCalculator;
        }

        #endregion

        #region Export

        /// <summary>
        /// Export data about the given IBOE Form into a pre-formatted Word template and return the file path of
        /// the populated template.
        /// </summary>
        /// <param name="workspace">Full workspace.</param>
        /// <param name="fileNameToDisplayToBrowser">the file name to display to the browser in the download dialog</param>
        /// <param name="boeForm">The boeForm to export.</param>
        /// <param name="isPortionMarkingEnabled">True if portion marking is enabled; False otherwise.</param>
        /// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace.</param>
        /// <param name="contractTypes">The contract types from DB.</param>
        public virtual string ExportToWordFile(FullWorkspace workspace, string fileNameToDisplayToBrowser, T boeForm, bool isPortionMarkingEnabled, string proposalTitleAndRfpNumber, ICollection<PickListDto> contractTypes)
        {
            string templateLocation = isPortionMarkingEnabled ? this.TemplateLocationPortionMarking : this.TemplateLocation;
            string fileLocation = HttpContext.Current.Server.MapPath(string.Format(templateLocation, boeForm.Version.ToString()));
            
            // Create a new random file name in the specified directory
            string toReturn = Path.GetDirectoryName(fileLocation) + "\\" + Path.GetRandomFileName() + ".docx";

            // template file is on disk
            using (FileStream fs = new FileStream(toReturn, FileMode.CreateNew))
            {
                this.Export(fileLocation, (document) =>
                {
                    this.PopulateDataExport(workspace, document, boeForm, isPortionMarkingEnabled, proposalTitleAndRfpNumber, contractTypes);
                }, fs);
            }

            return toReturn;
        }

		/// <summary>
		/// Export data about the given BOE Form into a pre-formatted Word template and return the stream.
		/// </summary>
		/// <param name="workspace">Full workspace.</param>
		/// <param name="boeForm">The boeForm to export.</param>
		/// <param name="isPortionMarkingEnabled">True if portion marking is enabled; False otherwise.</param>
		/// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace.</param>
		/// <param name="contractTypes">The contract types from DB.</param>
		/// <returns>The Stream containing the export</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public virtual Stream ExportToStream(FullWorkspace workspace, T boeForm, bool isPortionMarkingEnabled, string proposalTitleAndRfpNumber, ICollection<PickListDto> contractTypes)
		{
			string templateLocation = isPortionMarkingEnabled ? this.TemplateLocationPortionMarking : this.TemplateLocation;
			string fileLocation = HttpContext.Current.Server.MapPath(string.Format(templateLocation, boeForm.Version.ToString()));

			// template file is on disk
			MemoryStream ms = new MemoryStream();
			this.Export(fileLocation, (document) =>
			{
				this.PopulateDataExport(workspace, document, boeForm, isPortionMarkingEnabled, proposalTitleAndRfpNumber, contractTypes);
			}, ms);

			// Seek back to beginning of Memory Stream
			ms.Seek(0, SeekOrigin.Begin);

			return ms;
		}

		#endregion

		protected abstract void PopulateDataExport(FullWorkspace workspace, WordprocessingDocument document, T boeForm, bool isPortionMarkingEnabled, string proposalTitleAndRfpNumber, ICollection<PickListDto> contractTypes);

        /// <summary>
        /// Pulls the table rows from the workspace.
        /// </summary>
        /// <param name="workspace">The workspace for the export.</param>
        /// <param name="boeForm">The BOE form.</param>
        /// <param name="resourceIds">The resource ids.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <returns>
        /// List of table rows for the form.
        /// </returns>
        protected ICollection<TK> PullRowsFromWorkspace(FullWorkspace workspace, T boeForm, ICollection<int> resourceIds, ICollection<PickListDto> contractTypes)
        {
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (ReferenceEquals(boeForm, null))
            {
                throw new ArgumentNullException(nameof(boeForm));
            }
            if (ReferenceEquals(resourceIds, null))
            {
                throw new ArgumentNullException(nameof(resourceIds));
            }

            Collection<TK> rows = new Collection<TK>();
            Dictionary<int, Dictionary<int, TK>> rowByClinByWBS = new Dictionary<int, Dictionary<int, TK>>();
            DateTime? popStart = null;
            DateTime? popEnd = null;
            foreach (BoeTaskElementDTO taskElement in workspace.TaskElements)
            {
                BoeDTO boe = workspace.Boes.First(b => b.Id == taskElement.BoeID);
                foreach (ResourceTypeDto laborTask in taskElement.taskElementLabors)
                {
                    int? clinId = boe.IsMultiClinWbs ? laborTask.CLINID : boe.CLINID;
                    int? wbsId = boe.IsMultiClinWbs ? laborTask.WBSID : boe.WBSID;

                    if (!clinId.HasValue)
                    {
                        clinId = -1;
                    }

                    if (!wbsId.HasValue)
                    {
                        wbsId = -1;
                    }

                    if (clinId.HasValue && wbsId.HasValue && laborTask.ResourceID.HasValue && resourceIds.Contains(laborTask.ResourceID.Value))
                    {
                        //it's a match, put it into the dictionary
                        Dictionary<int, TK> wbsDictionary;
                        if (!rowByClinByWBS.TryGetValue(clinId.Value, out wbsDictionary))
                        {
                            wbsDictionary = new Dictionary<int, TK>();
                            rowByClinByWBS.Add(clinId.Value, wbsDictionary);
                        }

                        TK row;
                        if (!wbsDictionary.TryGetValue(wbsId.Value, out row))
                        {
                            WbsDTO wbs = workspace.WbsElements.FirstOrDefault(w => w.Id == wbsId.Value);
                            ClinDTO clin = workspace.Clins.FirstOrDefault(c => c.Id == clinId.Value);
                            row = this.CreateRow(boeForm, wbs, clin, contractTypes);

                            wbsDictionary.Add(wbsId.Value, row);
                            rows.Add(row);
                        }

                        if (laborTask.ValueSpread.HasValue)
                        {
                            // determine if this task references a T&M resource                
                            ResourceDTO resource = this.resourceLoader.GetById(laborTask.ResourceID.Value);
                            bool isTMResource = (resource.ElementOfCost == ElementOfCostType.IWTA || resource.ElementOfCost == ElementOfCostType.Sub) && resource.RateType == RateType.Hours;

                            row.Value += workspace.IsUsingTM && isTMResource ? this.tmCalculator.TotalCostForTaskSpread(workspace, laborTask) : laborTask.ValueSpread.Value;
                            foreach(ResourceSpreadDto spread in laborTask.LaborSpreads)
                            {
                                if (spread.LaborSpreadValue != 0)
                                {
                                    if (!popStart.HasValue)
                                    {
                                        popStart = spread.LaborSpreadDate;
                                        popEnd = spread.LaborSpreadDate;
                                    }
                                    else
                                    {
                                        if (popStart.Value > spread.LaborSpreadDate)
                                        {
                                            popStart = spread.LaborSpreadDate;
                                        }

                                        if (popEnd.Value < spread.LaborSpreadDate)
                                        {
                                            popEnd = spread.LaborSpreadDate;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (popStart.HasValue)
            {
                this.PeriodOfPerformance = string.Format("{0} - {1}", popStart.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR), popEnd.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR));
            }
            else
            {
                this.PeriodOfPerformance = string.Empty;
            }

            return rows;
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
        protected abstract TK CreateRow(T boeForm, WbsDTO wbs, ClinDTO clin, ICollection<PickListDto> contractTypes);

        #region Word utilities

        /// <summary>
        /// Gets a Template Data Row from a document by field
        /// </summary>
        /// <param name="document">The word document edited via openxml.</param>
        /// <param name="field">The name of the tag in the document</param>
        /// <returns>The TableRow in the document tagged by the field.</returns>
        protected TableRow GetTemplateDataRow(WordprocessingDocument document, string field)
        {
            SdtElement dataRowMarkerTag = WordUtilities.GetTaggedElement(document, field);
            TableRow templateDataRow = dataRowMarkerTag.Ancestors<TableRow>().FirstOrDefault();
            this.SetCantSplit(templateDataRow);

            return templateDataRow;
        }

        /// <summary>
        /// Cleanup the document.
        /// </summary>
        /// <param name="document">The word document edited via openxml.</param>
        protected void DocumentCleanup(WordprocessingDocument document)
        {
            if (ReferenceEquals(document, null))
            {
                throw new ArgumentNullException(nameof(document));
            }

            #region Final document cleanup

            #region Set "View" to "Print Layout"

            if (document.MainDocumentPart.DocumentSettingsPart == null)
            {
                document.MainDocumentPart.AddNewPart<DocumentSettingsPart>();
            }
            if (document.MainDocumentPart.DocumentSettingsPart.Settings == null)
            {
                document.MainDocumentPart.DocumentSettingsPart.Settings = new Settings();
            }
            if (document.MainDocumentPart.DocumentSettingsPart.Settings.View == null)
            {
                document.MainDocumentPart.DocumentSettingsPart.Settings.View = new View();
            }
            document.MainDocumentPart.DocumentSettingsPart.Settings.View.Val = ViewValues.Print;

            #endregion

            // remove content controls
            WordUtilities.RemoveContentControls(document);
            WordUtilities.CleanupDocumentXml(document);

            #endregion
        }

        /// <summary>
        /// Sets the field in the document.
        /// </summary>
        /// <param name="document">The word document edited via openxml.</param>
        /// <param name="field">Name of the Content Control in Word Document.</param>
        /// <param name="value">The value to set for the field in the Document.</param>
        protected void SetField(WordprocessingDocument document, string field, string value)
        {
            SdtElement dataElement = WordUtilities.GetTaggedElement(document, field);

            if (dataElement != null)
            {
                WordUtilities.SetElementText(dataElement, value ?? string.Empty);
            }
        }

        /// <summary>
        /// Sets the html field in the document.
        /// </summary>
        /// <param name="document">The word document edited via openxml.</param>
        /// <param name="field">Name of the Content Control in Word Document.</param>
        /// <param name="value">The value to set for the field in the Document.</param>
        /// <param name="counters">Counters used to give html unique IDs.</param>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        protected void SetHtmlField(WordprocessingDocument document, string field, string value, ref ChunkCounter counters)
        {
            if (ReferenceEquals(document, null))
            {
                throw new ArgumentNullException(nameof(document));
            }

            SdtElement dataElement = WordUtilities.GetTaggedElement(document, field);

            if (dataElement != null)
            {
                WordUtilities.SetElementTextWithHTML(document.MainDocumentPart, dataElement, value, ref counters);
            }
        }

        /// <summary>
        /// Populates the proprietary label in the footer.
        /// </summary>
        /// <param name="document">The Word document to populate</param>
        /// <param name="containsOCI">True if the workspace contains OCI data</param>
        protected void SetProprietaryLabels(
            WordprocessingDocument document,
            bool containsOCI)
        {
            if (ReferenceEquals(document, null))
            {
                throw new ArgumentNullException(nameof(document));
            }

            // Get the proprietary alias in Footer
            SdtAlias alias = (from footerPart in document.MainDocumentPart.FooterParts
                              from sdtElement in footerPart.Footer.Descendants<SdtAlias>()
                              select sdtElement).FirstOrDefault(x => x.Val.Value == FOOTER_LMPI);

            if (alias != null)
            {
                // Get the Element that encapsulates the current alias
                var element = alias.Ancestors<SdtElement>().FirstOrDefault();

                // If the current element is not null, populate it with the appropriate data
                if (element != null)
                {
                    WordUtilities.SetElementText(element, containsOCI ? BOEExporterConstants.LMPI_OCI_LABEL_TEXT : BOEExporterConstants.LMPI_LABEL_TEXT);
                    alias.Remove();
                }
            }
        }

        #endregion

        #region helper methods
        /// <summary>
        /// Helper method to add portion marking text "(U) " as needed.
        /// </summary>
        /// <param name="isPortionMarkingEnabled"></param>
        /// <returns>"(U) " if portion marking is enabled; empty string otherwise.</returns>
        protected string GenPortionMarkingText(bool isPortionMarkingEnabled)
        {
            return isPortionMarkingEnabled ? BOEExporterConstants.PORTION_MARKING_U : string.Empty;
        }
        #endregion helper methods
    }
}
