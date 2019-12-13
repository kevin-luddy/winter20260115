// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web;
    using Common;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// The PPRD Exporter.
    /// </summary>
    public class PPRDExporter : WordExporter, IPPRDExporter
    {
        /// <summary>
        /// The file attachments title
        /// </summary>
        private const string FILE_ATTACHMENTS_TITLE = "File Attachments";

        /// <summary>
        /// The first section reference number
        /// </summary>
        private const string FIRST_SECTION_REFERENCE_NUMBER = "1.0";

        /// <summary>
        /// Generate a Word document containing the full PPRD.
        /// </summary>
        /// <param name="sections">Collection of Section MVs</param>
        /// <param name="rates">Collection of RateDetail MVs</param>
        /// <param name="fileAttachments">Collection of File Attachment MVs</param>
        /// <param name="serverFileName">Server path to new file to generate.</param>
        /// <param name="clientFileName">the file name to display to the browser in the download dialog</param>
        /// <param name="revision">Revision modelview</param>
        /// <param name="rateTableYears">Number of years to include in the rate tables</param>
        /// <param name="response">the web response object to write the file back to for user download</param>
        public void ExportFullPPRDToWordFile(ICollection<SectionModelView> sections, ICollection<RateDetailModelView> rates, ICollection<FileAttachmentRowModelView> fileAttachments, string serverFileName, string clientFileName, RevisionModelView revision, int rateTableYears, HttpResponseBase response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            ChunkCounter counters = new ChunkCounter();

            // setup the response correctly with BufferOutput since this is going to be awhile...
            response.ContentType = PPRDExporterConstants.CONTENTTYPE_DOCX;
            response.Clear();
            response.AppendHeader(PPRDExporterConstants.CONTENT_HEADER_NAME, string.Format(PPRDExporterConstants.CONTENT_HEADER_FORMAT_STRING, clientFileName));

            this.Export(serverFileName, (document) => { this.PopulatePPRDExport(document, sections, rates, fileAttachments, revision, rateTableYears, ref counters); }, response.OutputStream);
        }

        /// <summary>
        /// Generate a Word document containing the RDD sections and rates.
        /// </summary>
        /// <param name="sections">Collection of Section MVs</param>
        /// <param name="rates">Collection of RateDetail MVs</param>
        /// <param name="fileAttachments">Collection of File Attachment MVs</param>
        /// <param name="serverFileName">Server path to new file to generate.</param>
        /// <param name="clientFileName">the file name to display to the browser in the download dialog</param>
        /// <param name="revision">Revision modelview</param>
        /// <param name="rddDocument">The RDD document to use for creation.</param>
        /// <param name="response">the web response object to write the file back to for user download</param>
        public void ExportRDDToWordFile(ICollection<SectionModelView> sections, ICollection<RateDetailModelView> rates, ICollection<FileAttachmentRowModelView> fileAttachments, string serverFileName, string clientFileName, RevisionModelView revision, DocumentDetailModelView rddDocument, HttpResponseBase response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            ChunkCounter counters = new ChunkCounter();

            // setup the response correctly with BufferOutput since this is going to be awhile...
            response.ContentType = PPRDExporterConstants.CONTENTTYPE_DOCX;
            response.Clear();
            response.AppendHeader(PPRDExporterConstants.CONTENT_HEADER_NAME, string.Format(PPRDExporterConstants.CONTENT_HEADER_FORMAT_STRING, clientFileName));

            int rateTableYears = rddDocument.EndYear - rddDocument.StartYear;

            this.Export(serverFileName, (document) => { this.PopulatePPRDExport(document, sections, rates, fileAttachments, revision, rateTableYears, ref counters, rddDocument); }, response.OutputStream);
        }

        #region Populate Methods

        /// <summary>
        /// Populate the Word document with the the full PPRD.
        /// </summary>
        /// <param name="document">Word document to store the PPRD.</param>
        /// <param name="sections">Section model views</param>
        /// <param name="rates">Rate Detail model views</param>
        /// <param name="fileAttachments">Collection of File Attachment MVs</param>
        /// <param name="revision">Revision modelview</param>
        /// <param name="rateTableYears">Number of years to include in the rate tables</param>
        /// <param name="counters">The chunk counters</param>
        /// <param name="rddDocument">The RDD document model view.</param>
        private void PopulatePPRDExport(WordprocessingDocument document, ICollection<SectionModelView> sections, ICollection<RateDetailModelView> rates, ICollection<FileAttachmentRowModelView> fileAttachments, RevisionModelView revision, int rateTableYears, ref ChunkCounter counters, DocumentDetailModelView rddDocument = null)
        {
            // Populate the header
            this.PopulatePPRDHeader(document, revision);

            // Populate introduction text
            this.PopulateIntroduction(document, revision, ref counters);

            // Get the section container template to begin the process of populating the body of the document
            SdtElement sectionContainerTemplate = WordUtilities.GetTaggedElement(document, PPRDExporterConstants.CONTAINER_SECTION);
            
            OpenXmlElement lastElement = sectionContainerTemplate;

            // get the published year - use current year if not published
            int publishYear = revision.DatePublished == null
                ? DateTime.Now.Year
                : ((DateTime) revision.DatePublished).Year;

            if (rddDocument != null)
            {
                publishYear = rddDocument.StartYear;
            }

            // Keep track of the last section for finding the reference number of the File Attachments Section (if needed)
            SectionModelView lastSection = sections.LastOrDefault();

            // Add File Attachments that do not have sections
            this.PopulateFileAttachments(fileAttachments, sectionContainerTemplate, lastElement, document.MainDocumentPart, ref counters, lastSection);

            int firstSectionId = sections.FirstOrDefault() == null ? -1 : sections.First().Id;

            // Reverse iterate over sections so they will be properly shown in the document
            foreach (SectionModelView section in sections.Reverse())
            {
                this.PopulateSectionContainer(section, rates, fileAttachments, publishYear, rateTableYears, 0, sectionContainerTemplate,
                    lastElement, document.MainDocumentPart, ref counters, rddDocument, section.Id == firstSectionId);
            }

            if (sectionContainerTemplate != null)
            {
                // delete the template
                this.RemoveElement(sectionContainerTemplate);
            }
            
            // Clean up
            this.PerformFinalDocumentCleanup(document);
        }

        /// <summary>
        /// Populates elements in the PPRD Header
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="revision">Revision ModelView</param>
        private void PopulatePPRDHeader(WordprocessingDocument document, RevisionModelView revision)
        {
            ICollection<SdtAlias> headerElements = (from headerPart in document.MainDocumentPart.HeaderParts
                from sdtElement in headerPart.Header.Descendants<SdtAlias>()
                select sdtElement).ToCollection();
            
            // Populate the revision number
            SdtAlias revNumberAlias = headerElements.FirstOrDefault(x => x.Val.Value == PPRDExporterConstants.FIELDNAME_REVISIONNUMBER);
            if (revNumberAlias != null)
            {
                SdtElement revNumberElement = revNumberAlias.Ancestors<SdtElement>().FirstOrDefault();
                if (revNumberElement != null)
                {
                    WordUtilities.SetElementText(revNumberElement, revision.Revision);
                    revNumberAlias.Remove();
                }
            }

            // Populate the publish date
            SdtAlias publishDateAlias = headerElements.FirstOrDefault(x => x.Val.Value == PPRDExporterConstants.FIELDNAME_PUBLISHDATE);
            if (publishDateAlias != null)
            {
                SdtElement publishDateElement = publishDateAlias.Ancestors<SdtElement>().FirstOrDefault();
                if (publishDateElement != null)
                {
                    WordUtilities.SetElementText(publishDateElement, revision.RevisionPublishedInfo);
                    publishDateAlias.Remove();
                }
            }
        }

        /// <summary>
        /// Populates the introduction text
        /// </summary>
        /// <param name="document">The document</param>
        /// <param name="revision">Revision modelview</param>
        /// <param name="counters">The chunk counters</param>
        private void PopulateIntroduction(WordprocessingDocument document, RevisionModelView revision, ref ChunkCounter counters)
        {
            // Get container element
            SdtElement introductionContainerTemplate = WordUtilities.GetTaggedElement(document, PPRDExporterConstants.CONTAINER_INTRODUCTION);
            
            // populate History
            SdtElement historyElement = WordUtilities.GetTaggedChildElement(introductionContainerTemplate,
                PPRDExporterConstants.FIELDNAME_HISTORY);
            WordUtilities.SetElementTextWithHTML(document.MainDocumentPart, historyElement,
                this.ReplaceParagraphTags(revision.History), ref counters);

            // populate Release Notes
            SdtElement publishDateElement = WordUtilities.GetTaggedChildElement(introductionContainerTemplate,
                PPRDExporterConstants.FIELDNAME_PUBLISHDATE);
            WordUtilities.SetElementText(publishDateElement, revision.RevisionPublishedInfo);

            SdtElement releaseNotesElement = WordUtilities.GetTaggedChildElement(introductionContainerTemplate,
                PPRDExporterConstants.FIELDNAME_RELEASENOTES);
            if (!string.IsNullOrWhiteSpace(revision.ReleaseNotes))
            {
                WordUtilities.SetElementTextWithHTML(document.MainDocumentPart, releaseNotesElement,
                    this.ReplaceParagraphTags(revision.ReleaseNotes), ref counters);
            }
            else
            {
                this.RemoveElement(releaseNotesElement);
            }
        }

        /// <summary>
        /// Populate the Section Container
        /// </summary>
        /// <param name="section">Section MV</param>
        /// <param name="rates">Rate Detail MVs</param>
        /// <param name="fileAttachments">Collection of File Attachment MVs</param>
        /// <param name="publishYear">Publish year</param>
        /// <param name="rateTableYears">Number of years to include in the rate tables</param>
        /// <param name="subsectionLevel">Subsection level</param>
        /// <param name="sectionContainerTemplate">The Section Container Template</param>
        /// <param name="lastElement">Last Element</param>
        /// <param name="mainPart">The main document part</param>
        /// <param name="counters">The chunk counters</param>
        /// <param name="rddDocument">The RDD document to create from.  If null, then export full PPRD.</param>
        /// <param name="firstSection">Bool noting if this is the first section</param>
        private void PopulateSectionContainer(SectionModelView section, ICollection<RateDetailModelView> rates, ICollection<FileAttachmentRowModelView> fileAttachments, int publishYear, int rateTableYears, int subsectionLevel, SdtElement sectionContainerTemplate, OpenXmlElement lastElement, MainDocumentPart mainPart, ref ChunkCounter counters, DocumentDetailModelView rddDocument, bool firstSection)
        {
            if (rddDocument == null || rddDocument.SelectedSectionIds.Contains(section.Id))
            {
                // only show internals for full PPRD
                bool showInternals = rddDocument == null;
                // clone the main container
                SdtElement sectionContainer = sectionContainerTemplate.CloneNode(true) as SdtElement;
                lastElement = lastElement.InsertAfterSelf(sectionContainer);

                // Insert a page break if section is a main section (2.0, 3.0, etc)
                // Page break is not needed for The first section because it will already be on a new page
                if (subsectionLevel == 0 && !firstSection)
                {
                    SdtElement pageBreakElement =
                        WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.PAGE_BREAK);
                    Paragraph pageBreak = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
                    pageBreakElement.Append(pageBreak);
                }

                // Populate the section number and title
                this.PopulateSectionTitle(sectionContainer, section, subsectionLevel);

                // Separate out the child node elements
                ICollection<SectionModelView> textAndTableMVs =
                    section.ChildNodes.Where(x => x.ContentType == SectionContentType.Text ||
                                                  x.ContentType == SectionContentType.RateTable)
                        .OrderBy(o => o.DisplayOrder).ToCollection();
                ICollection<SectionModelView> subsectionMVs =
                    section.ChildNodes.Where(x => x.ContentType == SectionContentType.Section).ToCollection();

                // Get the container template
                SdtElement textAndTableContainerTemplate =
                    WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.CONTAINER_TEXTANDTABLES);
                OpenXmlElement lastTextTableElement = textAndTableContainerTemplate;

                // Populate text element(s) and table
                foreach (SectionModelView modelView in textAndTableMVs)
                {
                    if (showInternals || !modelView.IsInternalSection.Value)
                    {
                        // clone the main container
                        SdtElement textAndTableContainer = textAndTableContainerTemplate.CloneNode(true) as SdtElement;
                        lastTextTableElement = lastTextTableElement.InsertAfterSelf(textAndTableContainer);

                        // Get text and table elements
                        SdtElement textElement = WordUtilities.GetTaggedChildElement(textAndTableContainer,
                            PPRDExporterConstants.FIELDNAME_TEXTELEMENT);
                        SdtElement rateTableElement =
                            WordUtilities.GetTaggedChildElement(textAndTableContainer, PPRDExporterConstants.TABLE_RATES);

                        if (modelView.ContentType == SectionContentType.Text && textElement != null)
                        {
                            WordUtilities.SetElementTextWithHTML(mainPart, textElement, modelView.TextContent, ref counters, false, modelView.IsInternalSection ?? false);

                            // Remove table elements
                            this.RemoveElement(rateTableElement);
                        }
                        else if (modelView.ContentType == SectionContentType.RateTable && rateTableElement != null)
                        {
                            // Get rates for section
                            ICollection<RateDetailModelView> sectionRates = rates.Where(x => x.Section == section.Id)
                                .OrderBy(x => x.RateCode).ToCollection();

                            // For backward-looking categories (FCCOM, Fringe, G&A, and Overhead), display [publishYear - backwardLookingYears] thru [publishYear + rateTableYears].
                            // Otherwise, display [publishYear] thru [publishYear + 5].
                            RateDetailModelView firstRate = sectionRates.FirstOrDefault();
                            int backwardLookingYears = firstRate != null &&
                                (firstRate.RateCategory == RateCategory.Fccom || firstRate.RateCategory == RateCategory.Fringe ||
                                firstRate.RateCategory == RateCategory.GA || firstRate.RateCategory == RateCategory.Overhead) ? CommonConstants.RATE_TABLE_YEARS_TO_DISPLAY_BACKWARD_LOOKING_ADJUSTMENT : 0;

                            if (rddDocument != null)
                            {
                                backwardLookingYears = 0;
                            }

                            int totalYears = rateTableYears + backwardLookingYears;
                            int firstYear = publishYear - backwardLookingYears;

                            if (totalYears <= PPRDExporterConstants.MAX_TABLE_ROW_YEARS) 
                            {
                                // Populate Rate Table
                                this.PopulateRateTable(rateTableElement, sectionRates, firstYear, totalYears,
                                    modelView.DisplayRateCode);
                            }
                            else
                            {
                                // Make duplicate/template of table element
                                SdtElement templateTableElement = rateTableElement;
                                SdtElement currentInsertionElement = templateTableElement;

                                // Get starting years for each version of the table
                                ICollection<int> startYears = new Collection<int>();

                                for (int i = 0; i < totalYears; i += PPRDExporterConstants.MAX_TABLE_ROW_YEARS)
                                {
                                    startYears.Add(firstYear + i);
                                }

                                // Populate the tables
                                foreach (int startYear in startYears)
                                {
                                    SdtElement tableElement = templateTableElement.CloneNode(true) as SdtElement;

                                    // Number of years will be the max for all tables but the last, which will be the remaining number of years
                                    int years = startYear == startYears.Last()
                                        ? (firstYear + totalYears) - startYear
                                        : PPRDExporterConstants.MAX_TABLE_ROW_YEARS - 1;

                                    this.PopulateRateTable(tableElement, sectionRates, startYear, years,
                                        modelView.DisplayRateCode);

                                    currentInsertionElement.InsertAfterSelf(tableElement);
                                    currentInsertionElement = tableElement;
                                }
                                
                                // Remove the template table element
                                templateTableElement.Remove();
                            }

                            // Remove the text element
                            this.RemoveElement(textElement);
                        }
                        else
                        {
                            // Remove all elements
                            this.RemoveElement(textElement);
                            this.RemoveElement(rateTableElement);
                        }
                    }
                }

                // Get the file attachments for this section
                ICollection<FileAttachmentRowModelView> sectionFileAttachments = fileAttachments.Where(f => f.SectionId == section.Id).ToList();
                this.AddFileAttachments(mainPart, ref counters, textAndTableContainerTemplate, ref lastTextTableElement, sectionFileAttachments);

                // Remove the template element
                this.RemoveElement(textAndTableContainerTemplate);

                subsectionLevel++;

                // Reverse iterate over sections so they will be properly shown in the document
                foreach (SectionModelView subsectionElement in subsectionMVs.Reverse())
                {
                    // Recursive call to populate subsections
                    this.PopulateSectionContainer(subsectionElement, rates, fileAttachments, publishYear, rateTableYears, subsectionLevel, sectionContainerTemplate, lastElement, mainPart, ref counters, rddDocument, firstSection);
                }
            }
        }

        /// <summary>
        /// Adds the file attachments to a clone of the specified template.
        /// </summary>
        /// <param name="mainPart">The main part.</param>
        /// <param name="counters">The counters.</param>
        /// <param name="textAndTableContainerTemplate">The text and table container template.</param>
        /// <param name="lastTextTableElement">The last text table element.</param>
        /// <param name="sectionFileAttachments">The section file attachments.</param>
        private void AddFileAttachments(MainDocumentPart mainPart, ref ChunkCounter counters, SdtElement textAndTableContainerTemplate, ref OpenXmlElement lastTextTableElement, ICollection<FileAttachmentRowModelView> sectionFileAttachments)
        {
            if (sectionFileAttachments.Any())
            {
                foreach (FileAttachmentRowModelView attachment in sectionFileAttachments)
                {
                    // clone the main container
                    SdtElement textAndTableContainer = textAndTableContainerTemplate.CloneNode(true) as SdtElement;
                    lastTextTableElement = lastTextTableElement.InsertAfterSelf(textAndTableContainer);

                    // Get text and table elements
                    SdtElement textElement = WordUtilities.GetTaggedChildElement(textAndTableContainer,
                        PPRDExporterConstants.FIELDNAME_TEXTELEMENT);
                    SdtElement rateTableElement =
                        WordUtilities.GetTaggedChildElement(textAndTableContainer, PPRDExporterConstants.TABLE_RATES);

                    // Create the hyperlink text
                    string hyperlink = string.Format("<a href=\"{1}\">{0}</a>", attachment.Name, attachment.Link);

                    // Set the hyperlink
                    WordUtilities.SetElementTextWithHTML(mainPart, textElement, hyperlink, ref counters, false, false);

                    // Remove table elements
                    this.RemoveElement(rateTableElement);
                }
            }
        }

        /// <summary>
        /// Populate the section title and number
        /// </summary>
        /// <param name="sectionContainer">Section Container</param>
        /// <param name="section">Section ModelView</param>
        /// <param name="subsectionLevel">Section level - 0 for top-level, 1 for subsection, etc</param>
        private void PopulateSectionTitle(SdtElement sectionContainer, SectionModelView section, int subsectionLevel)
        {
            // Get section title container elements
            SdtElement sectionTitleContainerElement =
                WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.CONTAINER_SECTIONTITLE);
            SdtElement subsectionTitleContainerElement =
                WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.CONTAINER_SUBSECTIONTITLE);
            SdtElement subsubsectionTitleContainerElement =
                WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.CONTAINER_SUBSUBSECTIONTITLE);
            
            if (subsectionLevel == 0)
            {
                // Top-level section (ex. "1.0")
                if (sectionTitleContainerElement != null)
                {
                    // Populate Section number and title
                    SdtElement sectionNumberElement = WordUtilities.GetTaggedChildElement(sectionTitleContainerElement,
                        PPRDExporterConstants.FIELDNAME_SECTIONNUMBER);
                    if (sectionNumberElement != null)
                    {
                        WordUtilities.SetElementText(sectionNumberElement, section.ReferenceNumber);

                        if (section.IsInternalSection == true)
                        {
                            this.ApplyInternalSectionFormatting(sectionNumberElement);
                        }
                    }

                    SdtElement sectionTitleElement = WordUtilities.GetTaggedChildElement(sectionTitleContainerElement,
                        PPRDExporterConstants.FIELDNAME_SECTIONTITLE);
                    if (sectionTitleElement != null)
                    {
                        WordUtilities.SetElementText(sectionTitleElement, section.Title);

                        if (section.IsInternalSection == true)
                        {
                            this.ApplyInternalSectionFormatting(sectionTitleElement);
                        }
                    }
                }

                // Remove other containers
                this.RemoveIt(subsectionTitleContainerElement);
                this.RemoveIt(subsubsectionTitleContainerElement);
            }
            else if (subsectionLevel == 1)
            {
                // First level subsection (ex. 1.1)
                if (subsectionTitleContainerElement != null)
                {
                    // Populate Section number and title
                    SdtElement subsectionNumberElement = WordUtilities.GetTaggedChildElement(subsectionTitleContainerElement,
                        PPRDExporterConstants.FIELDNAME_SUBSECTIONNUMBER);
                    if (subsectionNumberElement != null)
                    {
                        WordUtilities.SetElementText(subsectionNumberElement, section.ReferenceNumber);

                        if (section.IsInternalSection == true)
                        {
                            this.ApplyInternalSectionFormatting(subsectionNumberElement);
                        }
                    }

                    SdtElement subsectionTitleElement = WordUtilities.GetTaggedChildElement(subsectionTitleContainerElement,
                        PPRDExporterConstants.FIELDNAME_SUBSECTIONTITLE);
                    if (subsectionTitleElement != null)
                    {
                        WordUtilities.SetElementText(subsectionTitleElement, section.Title);

                        if (section.IsInternalSection == true)
                        {
                            this.ApplyInternalSectionFormatting(subsectionTitleElement);
                        }
                    }
                }

                // Remove other containers
                this.RemoveIt(sectionTitleContainerElement);
                this.RemoveIt(subsubsectionTitleContainerElement);
            }
            else
            {
                // Second level or lower (ex. 1.1.1)
                if (subsubsectionTitleContainerElement != null)
                {
                    // Populate Section number and title
                    SdtElement subsubsectionNumberElement = WordUtilities.GetTaggedChildElement(subsubsectionTitleContainerElement,
                        PPRDExporterConstants.FIELDNAME_SUBSUBSECTIONNUMBER);
                    if (subsubsectionNumberElement != null)
                    {
                        WordUtilities.SetElementText(subsubsectionNumberElement, section.ReferenceNumber);

                        if (section.IsInternalSection == true)
                        {
                            this.ApplyInternalSectionFormatting(subsubsectionNumberElement);
                        }
                    }

                    SdtElement subsubsectionTitleElement = WordUtilities.GetTaggedChildElement(subsubsectionTitleContainerElement,
                        PPRDExporterConstants.FIELDNAME_SUBSUBSECTIONTITLE);
                    if (subsubsectionTitleElement != null)
                    {
                        WordUtilities.SetElementText(subsubsectionTitleElement, section.Title);

                        if (section.IsInternalSection == true)
                        {
                            this.ApplyInternalSectionFormatting(subsubsectionTitleElement);
                        }
                    }
                }

                // Remove other containers
                this.RemoveIt(sectionTitleContainerElement);
                this.RemoveIt(subsectionTitleContainerElement);
            }
        }

        /// <summary>
        /// Popualtes the Rate Table
        /// </summary>
        /// <param name="rateTableElement">Rate table element in document</param>
        /// <param name="rates">Rates for the section</param>
        /// <param name="startYear">First year for table</param>
        /// <param name="years">Number of years to include in the table in addition to first year</param>
        /// <param name="showRateCode">Bool noting if rate code column should be included</param>
        private void PopulateRateTable(SdtElement rateTableElement, ICollection<RateDetailModelView> rates, int startYear, int years, bool? showRateCode)
        {
            // Populate header row labels
            SdtElement firstYearLabel = WordUtilities.GetTaggedChildElement(rateTableElement, PPRDExporterConstants.LABEL_YEAR);
            if (firstYearLabel != null)
            {
                WordUtilities.SetElementText(firstYearLabel, startYear);
            }

            // Remove Rate Code Table if needed and adjust cell widths
            if (showRateCode == false)
            {
                SdtElement rateCodeColumn = WordUtilities.GetTaggedChildElement(rateTableElement, PPRDExporterConstants.LABEL_RATECODE);
                SdtElement descriptionColumn = WordUtilities.GetTaggedChildElement(rateTableElement, PPRDExporterConstants.LABEL_DESCRIPTION);
                TableCell descriptionHeaderCell = descriptionColumn.Ancestors<TableCell>().FirstOrDefault();

                WordUtilities.removeColumnFromTable(rateCodeColumn);

                if (descriptionHeaderCell != null)
                {
                    this.SetCellWidth(descriptionHeaderCell, PPRDExporterConstants.DESCRIPTION_NO_RATE_CODE_COLUMN_WIDTH);
                }
            }

            Table rateTable = rateTableElement.Descendants<Table>().FirstOrDefault();
            if (rateTable != null)
            {
                // Make sure preferred table width is not set
                this.RemoveTablePreferredWidth(rateTable);

                // Set header row property to keep header between page breaks
                TableRow headerRow = rateTable.Descendants<TableRow>().First();
                this.SetHeaderRow(headerRow);
                this.AdjustRowBorders(headerRow);

                // Append additional years to header row
                for (int i = 1; i <= years; i++)
                {
                    this.AppendCellToRow(headerRow, (startYear + i).ToString());
                }

                // Initialize "insertion" row
                TableRow templateDataRow = rateTable.Descendants<TableRow>().ElementAt(1);
                this.SetCannotSplit(templateDataRow);
                TableRow currentInsertionRow = templateDataRow;

                foreach (RateDetailModelView rate in rates)
                {
                    // Create new row in the table
                    TableRow row = this.CloneMarkedTemplateRow(templateDataRow);

                    // Populate the row
                    SdtElement rateDescription = WordUtilities.GetTaggedChildElement(row, PPRDExporterConstants.FIELDNAME_RATECODEDESCRIPTION);

                    if (showRateCode == true)
                    {
                        SdtElement rateCode = WordUtilities.GetTaggedChildElement(row, PPRDExporterConstants.FIELDNAME_RATECODE);
                        WordUtilities.SetElementText(rateCode, rate.RateCode);
                    }
                    else
                    {
                        TableCell rateDescriptionCell = rateDescription.Descendants<TableCell>().FirstOrDefault();
                        if (rateDescriptionCell != null)
                        {
                            this.SetCellWidth(rateDescriptionCell, PPRDExporterConstants.DESCRIPTION_NO_RATE_CODE_COLUMN_WIDTH);
                        }
                    }
                    
                    WordUtilities.SetElementText(rateDescription, rate.Description);
                    
                    SdtElement initialYear = WordUtilities.GetTaggedChildElement(row, PPRDExporterConstants.FIELDNAME_RATECODEVALUE);
                    RateYearModelView initialRateYearMV = rate.Values.FirstOrDefault(x => x.Year == startYear) ?? new RateYearModelView();
                    string initialYearValue = RateFormatter.FormatRate(RateTarget.PPRD, rate.RateCategoryDescription, initialRateYearMV.Value);
                    WordUtilities.SetElementText(initialYear, initialYearValue);

                    // Append cells for additional years
                    bool isRowEmpty = initialYearValue.Equals(Constants.NOT_APPLICABLE);
                    for (int i = 1; i <= years; i++)
                    {
                        RateYearModelView rateYearMV = rate.Values.FirstOrDefault(x => x.Year == startYear + i) ?? new RateYearModelView();
                        string yearValue = RateFormatter.FormatRate(RateTarget.PPRD, rate.RateCategoryDescription, rateYearMV.Value);
                        isRowEmpty = isRowEmpty && yearValue.Equals(Constants.NOT_APPLICABLE);
                        this.AppendCellToRow(row, yearValue);
                    }

                    // Add the row to the table (unless the row is empty, i.e. all values are "N/A")
                    if (!isRowEmpty)
                    {
                        currentInsertionRow.InsertAfterSelf(row);
                        currentInsertionRow = row;
                    }
                }

                // Remove template row
                templateDataRow.Remove();

                // Adjust bottom border thickness
                this.AdjustTableBorders(rateTable);
            }
        }

        /// <summary>
        /// Populates the File Attachments Section.
        /// </summary>
        /// <param name="fileAttachments">The file attachments.</param>
        /// <param name="sectionContainerTemplate">The section container template.</param>
        /// <param name="lastElement">The last element.</param>
        /// <param name="mainDocumentPart">The main document part.</param>
        /// <param name="counters">The counters.</param>
        /// <param name="lastSection">The last section of the document (if one exists).</param>
        private void PopulateFileAttachments(ICollection<FileAttachmentRowModelView> fileAttachments, SdtElement sectionContainerTemplate, OpenXmlElement lastElement, MainDocumentPart mainDocumentPart, ref ChunkCounter counters, SectionModelView lastSection)
        {
            ICollection<FileAttachmentRowModelView> attachments = fileAttachments.Where(f => f.SectionId == 0).ToList();

            if (attachments.Any())
            {
                // clone the main container
                SdtElement sectionContainer = sectionContainerTemplate.CloneNode(true) as SdtElement;
                lastElement = lastElement.InsertAfterSelf(sectionContainer);

                // Create the ReferenceNumber
                string referenceNumber = FIRST_SECTION_REFERENCE_NUMBER;
                if (lastSection != null)
                {
                    double refNumber = double.Parse(lastSection.ReferenceNumber);
                    refNumber++;
                    referenceNumber = refNumber.ToString("###.0");
                }

                // Populate the section title using a fake SectionModel
                SectionModelView attachmentSection = new SectionModelView
                {
                    Title = FILE_ATTACHMENTS_TITLE,
                    IsInternalSection = false,
                    ReferenceNumber = referenceNumber
                };

                // Populate the section number and title
                this.PopulateSectionTitle(sectionContainer, attachmentSection, 0);

                // Get the container template
                SdtElement textAndTableContainerTemplate =
                    WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.CONTAINER_TEXTANDTABLES);
                OpenXmlElement lastTextTableElement = textAndTableContainerTemplate;

                // Add the file attachments
                this.AddFileAttachments(mainDocumentPart, ref counters, textAndTableContainerTemplate, ref lastTextTableElement, attachments);

                // Remove the template element
                this.RemoveElement(textAndTableContainerTemplate);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Perform the final cleanup of the document
        /// </summary>
        /// <param name="document">the document to clean</param>
        private void PerformFinalDocumentCleanup(WordprocessingDocument document)
        {
            // Set View to Print layout
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

            // Remove content controls
            WordUtilities.RemoveContentControls(document);

            // Clean up XML
            WordUtilities.CleanupDocumentXml(document);
        }

        /// <summary>
        /// Apply formatting for Internal Sections - blue, italic text
        /// </summary>
        /// <param name="element">element to apply formatting to</param>
        private void ApplyInternalSectionFormatting(SdtElement element)
        {
            RunProperties runProperties = new RunProperties();

            Italic italic = new Italic() { Val = OnOffValue.FromBoolean(true) };
            Color color = new Color() { Val = PPRDExporterConstants.INTERNALSECTIONTEXTCOLOR }; // blue

            runProperties.Append(italic);
            runProperties.Append(color);

            if (element.GetFirstChild<SdtContentRun>() != null && element.GetFirstChild<SdtContentRun>().GetFirstChild<Run>() != null)
            {
                element.GetFirstChild<SdtContentRun>().GetFirstChild<Run>().PrependChild(runProperties);
            }
        }

        /// <summary>
        /// Append cell to the end of a row, using the formatting and properites of that row
        /// </summary>
        /// <param name="row">Row to append cell to</param>
        /// <param name="text">Text for cell</param>
        private void AppendCellToRow(TableRow row, string text)
        {
            if (row != null)
            {
                // Get cell to use as template for properties - match with last cell in row, the one it will go next to
                TableCell templateCell = row.Descendants<TableCell>().LastOrDefault();

                if (templateCell != null)
                {
                    // Set the cell, paragraph, and run properties
                    TableCellProperties cellProperties = new TableCellProperties(templateCell.TableCellProperties.CloneNode(true));
                    ParagraphProperties paraProperties = templateCell.Descendants<ParagraphProperties>().FirstOrDefault();
                    RunProperties runProperties = templateCell.Descendants<RunProperties>().FirstOrDefault();

                    // Set the text and cell properties
                    TableCell cell = new TableCell();
                    Text cellText = new Text() { Text = text };
                    cell.PrependChild(cellProperties);
                    
                    // Add text and run properties to run
                    Run run = new Run();
                    run.Append(cellText);
                    run.PrependChild(runProperties != null ? runProperties.CloneNode(true) : new RunProperties());

                    // Add run and paragraph properties to paragraph, add paragraph to cell
                    Paragraph paragraph = new Paragraph(run);
                    paragraph.PrependChild(paraProperties != null ? paraProperties.CloneNode(true) : new ParagraphProperties());
                    cell.Append(paragraph);

                    // Append cell to row
                    row.Append(cell);
                }
            }
        }

        /// <summary>
        /// Set the header row property so header will show between page breaks
        /// </summary>
        /// <param name="headerRow">Row to set as header</param>
        private void SetHeaderRow(TableRow headerRow)
        {
            if (headerRow.TableRowProperties == null)
            {
                headerRow.TableRowProperties = new TableRowProperties();
            }

            headerRow.TableRowProperties.AppendChild(new TableHeader());
        }

        /// <summary>
        /// Adjust the bottom and right borders of the table to 1.5pt
        /// </summary>
        /// <param name="table">The table</param>
        private void AdjustTableBorders(Table table)
        {
            TableProperties tableProperties = table.Descendants<TableProperties>().FirstOrDefault();
            if (tableProperties != null && tableProperties.TableBorders != null)
            {
                if (tableProperties.TableBorders.BottomBorder != null)
                {
                    tableProperties.TableBorders.BottomBorder.Size = 12U; // 1.5 point border
                }

                if (tableProperties.TableBorders.RightBorder != null)
                {
                    tableProperties.TableBorders.RightBorder.Size = 12U; // 1.5 point border
                }
            }
        }

        /// <summary>
        /// Adjust the bottom border of the row to 1.5pt
        /// </summary>
        /// <param name="row">The row</param>
        private void AdjustRowBorders(TableRow row)
        {
            // There are no border properties for rows, so need to apply to each cell in the row
            ICollection<TableCell> cells = row.Descendants<TableCell>().ToCollection();
            foreach (TableCell cell in cells)
            {
                TableCellProperties cellProperties = cell.Descendants<TableCellProperties>().FirstOrDefault();
                if (cellProperties != null && cellProperties.TableCellBorders != null &&
                    cellProperties.TableCellBorders.BottomBorder != null)
                {
                    cellProperties.TableCellBorders.BottomBorder.Size = 12U; // 1.5 point border
                }
            }
        }

        /// <summary>
        /// Set the fixed width for a cell
        /// </summary>
        /// <param name="cell">Cell to adjust</param>
        /// <param name="width">Width to set (inches)</param>
        private void SetCellWidth(TableCell cell, decimal width)
        {
            // Convert width from inches to 20ths of a point
            // points = inches*72, 20ths of a point = points*20
            decimal convertedWidth = width * 72 * 20;

            TableCellProperties cellProperties = cell.Descendants<TableCellProperties>().FirstOrDefault();
            if (cellProperties != null)
            {
                cellProperties.TableCellWidth = new TableCellWidth() { Width = convertedWidth.ToString(), Type = TableWidthUnitValues.Dxa };
            }
        }

        /// <summary>
        /// Removes the Preferred width setting from the table
        /// </summary>
        /// <param name="table">Table to adjust</param>
        private void RemoveTablePreferredWidth(Table table)
        {
            TableProperties tableProperties = table.Descendants<TableProperties>().FirstOrDefault();
            if (tableProperties != null)
            {
                tableProperties.TableWidth = new TableWidth() { Type = TableWidthUnitValues.Nil };
            }
        }

        /// <summary>
        /// Replaces paragraph tags in html text with div tags
        /// Fixes line spacing issues in export
        /// </summary>
        /// <param name="htmlText">HTML text to replace tags in</param>
        /// <returns>string with paragraph tags replaced with div tags</returns>
        internal string ReplaceParagraphTags(string htmlText)
        {
            return htmlText.Replace(PPRDExporterConstants.P_START_TAG, PPRDExporterConstants.DIV_START_TAG).Replace(PPRDExporterConstants.P_END_TAG, PPRDExporterConstants.DIV_END_TAG);
        }

        #endregion
    }
}