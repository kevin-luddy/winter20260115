// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Core.IO.Export
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using Aspose.Words;
	using Aspose.Words.Markup;
	using Aspose.Words.Tables;
	using Common;
	using IES.Common.Core;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.OfficeUtilities;
	using IES.DataBridge.ModelViews;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Logging;

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
		/// The rate formatter
		/// </summary>
		private readonly RateFormatter rateFormatter;

		/// <summary>
		/// The token service
		/// </summary>
		private readonly ITokenService tokenService;

		/// <summary>
		/// default constructor
		/// </summary>
		/// <param name="rateFormatter">Rate Formatter</param>
		public PPRDExporter(ILogger<WordExporter> logger, RateFormatter rateFormatter, ITokenService tokenService) : base(logger)
		{
			this.rateFormatter = rateFormatter;
			this.tokenService = tokenService;
		}

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
		/// <param name="refNumberPrefixLevel">The prefix Level for the Reference Numbers.</param>
		/// <param name="portionMarkingRequired">Is Portion Marking Required</param>
		public async Task<IActionResult> ExportFullPPRDToWordFile(ICollection<SectionModelView> sections, ICollection<RateDetailModelView> rates, ICollection<FileAttachmentRowModelView> fileAttachments, string serverFileName, string clientFileName, RevisionModelView revision, int rateTableYears, int refNumberPrefixLevel, bool? portionMarkingRequired)
		{
			Stream stream = new MemoryStream(32000);
			await Export(serverFileName, (document) => { PopulatePPRDExport(document, sections, rates, fileAttachments, revision, rateTableYears, refNumberPrefixLevel); }, stream, portionMarkingRequired, tokenService);
			stream.Position = 0;
			return new FileStreamResult(stream, ExportFileDownloadBase.ContentType_DOCX)
			{
				FileDownloadName = clientFileName
			};
		}

		/// <summary>
		/// Generate a Word document containing the RDD sections and rates.
		/// </summary>
		/// <param name="sections">Collection of Section MVs</param>
		/// <param name="rates">Collection of RateDetail MVs</param>
		/// <param name="fileAttachments">Collection of File Attachment MVs</param>
		/// <param name="serverFileName">Server path to new file to generate.</param>
		/// <param name="revision">Revision modelview</param>
		/// <param name="rddDocument">The RDD document to use for creation.</param>
		/// <param name="stream">the stream to write the file back to for user download</param>
		/// <param name="refNumberPrefixLevel">The prefix Level for the Reference Numbers.</param>
		/// <param name="includeDocumentDetails">If document details (introduction, clarification, table of contents) should be included in the export</param>
		/// <param name="portionMarkingRequired">Is Portion Marking Required</param>
		public async Task ExportRDDToWordFile(ICollection<SectionModelView> sections, ICollection<RateDetailModelView> rates, ICollection<FileAttachmentRowModelView> fileAttachments, string serverFileName, RevisionModelView revision, DocumentDetailModelView rddDocument, Stream stream, int refNumberPrefixLevel, bool includeDocumentDetails = true, bool portionMarkingRequired = false)
		{
			int rateTableYears = rddDocument.EndYear - rddDocument.StartYear;

			await Export(serverFileName, (document) => { PopulatePPRDExport(document, sections, rates, fileAttachments, revision, rateTableYears, refNumberPrefixLevel, rddDocument, includeDocumentDetails); }, stream, portionMarkingRequired, tokenService);
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
		/// <param name="rddDocument">The RDD document model view.</param>
		/// <param name="includeDocumentDetails">If document details (introduction, clarification, table of contents) should be included in the export</param>
		/// <param name="refNumberPrefixLevel">The prefix Level for the Reference Numbers.</param>
		private void PopulatePPRDExport(Document document, ICollection<SectionModelView> sections, ICollection<RateDetailModelView> rates, 
			ICollection<FileAttachmentRowModelView> fileAttachments, RevisionModelView revision, int rateTableYears, int refNumberPrefixLevel, 
			DocumentDetailModelView rddDocument = null, bool includeDocumentDetails = true)
		{
			// Populate the header
			PopulatePPRDHeader(document, revision);

			if (includeDocumentDetails)
			{
				// Populate introduction text
				PopulateIntroduction(document, revision);
			}
			else
			{
				// Remove the document details - including introduction, clarification, and ToC
				RemoveElement(WordUtilities.GetTaggedElement(document, PPRDExporterConstants.CONTAINER_DOCUMENT_DETAILS));
			}

			// Get the section container template to begin the process of populating the body of the document
			StructuredDocumentTag sectionContainerTemplate = WordUtilities.GetTaggedElement(document, PPRDExporterConstants.CONTAINER_SECTION);

			Node lastElement = sectionContainerTemplate;

			// get the published year - use current year if not published
			int publishYear = revision.DatePublished == null
				? DateTime.Now.Year
				: ((DateTime)revision.DatePublished).Year;

			if (rddDocument != null)
			{
				publishYear = rddDocument.StartYear;
			}

			// Keep track of the last section for finding the reference number of the File Attachments Section (if needed)
			SectionModelView lastSection = sections.LastOrDefault();

			// Add File Attachments that do not have sections
			PopulateFileAttachments(fileAttachments, sectionContainerTemplate, lastElement, document, lastSection, refNumberPrefixLevel);

			int firstSectionId = sections.FirstOrDefault() == null ? -1 : sections.First().Id;

			// Reverse iterate over sections so they will be properly shown in the document
			foreach (SectionModelView section in sections.Reverse())
			{
				PopulateSectionContainer(section, rates, fileAttachments, publishYear, rateTableYears, 0, sectionContainerTemplate,
					lastElement, document, rddDocument, section.Id == firstSectionId, refNumberPrefixLevel);
			}

			if (sectionContainerTemplate != null)
			{
				// delete the template
				RemoveElement(sectionContainerTemplate);
			}

			// Clean up
			PerformFinalDocumentCleanup(document);
		}

		/// <summary>
		/// Populates elements in the PPRD Header
		/// </summary>
		/// <param name="document">Document</param>
		/// <param name="revision">Revision ModelView</param>
		private void PopulatePPRDHeader(Document document, RevisionModelView revision)
		{
			StructuredDocumentTagCollection sdts = document.Range.StructuredDocumentTags;

			foreach (StructuredDocumentTag tag in sdts)
			{
				HeaderFooter headerFooter = tag.GetAncestor(NodeType.HeaderFooter) as HeaderFooter;
				if (headerFooter != null && headerFooter.HeaderFooterType == HeaderFooterType.HeaderPrimary)
				{
					if (tag.Title == PPRDExporterConstants.FIELDNAME_REVISIONNUMBER)
					{
						WordUtilities.SetElementText(tag, revision.Revision);
						tag.Title = string.Empty;
					}
					else if (tag.Title == PPRDExporterConstants.FIELDNAME_PUBLISHDATE)
					{
						WordUtilities.SetElementText(tag, revision.RevisionPublishedInfo);
						tag.Title = string.Empty;
					}
				}
			}
		}

		/// <summary>
		/// Populates the introduction text
		/// </summary>
		/// <param name="document">The document</param>
		/// <param name="revision">Revision modelview</param>
		private void PopulateIntroduction(Document document, RevisionModelView revision)
		{
			// Get container element
			StructuredDocumentTag introductionContainerTemplate = WordUtilities.GetTaggedElement(document, PPRDExporterConstants.CONTAINER_INTRODUCTION);

			// populate History
			StructuredDocumentTag historyElement = WordUtilities.GetTaggedChildElement(introductionContainerTemplate,
				PPRDExporterConstants.FIELDNAME_HISTORY);
			WordUtilities.SetElementTextWithHTML(document, historyElement,
				ReplaceParagraphTags(revision.History));

			// populate Release Notes
			StructuredDocumentTag publishDateElement = WordUtilities.GetTaggedChildElement(introductionContainerTemplate,
				PPRDExporterConstants.FIELDNAME_PUBLISHDATE);
			WordUtilities.SetElementText(publishDateElement, revision.RevisionPublishedInfo);

			StructuredDocumentTag releaseNotesElement = WordUtilities.GetTaggedChildElement(introductionContainerTemplate,
				PPRDExporterConstants.FIELDNAME_RELEASENOTES);
			if (!string.IsNullOrWhiteSpace(revision.ReleaseNotes))
			{
				WordUtilities.SetElementTextWithHTML(document, releaseNotesElement,
					ReplaceParagraphTags(revision.ReleaseNotes));
			}
			else
			{
				RemoveElement(releaseNotesElement);
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
		/// <param name="rddDocument">The RDD document to create from.  If null, then export full PPRD.</param>
		/// <param name="firstSection">Bool noting if this is the first section</param>
		/// <param name="refNumberPrefixLevel">The prefix level for the Reference Numbers.</param>
		private void PopulateSectionContainer(SectionModelView section, ICollection<RateDetailModelView> rates, 
			ICollection<FileAttachmentRowModelView> fileAttachments, int publishYear, int rateTableYears, int subsectionLevel, 
			StructuredDocumentTag sectionContainerTemplate, Node lastElement, Document mainPart,
			DocumentDetailModelView rddDocument, bool firstSection, int refNumberPrefixLevel)
		{
			if (rddDocument == null || rddDocument.SelectedSectionIds.Contains(section.Id))
			{
				// only show internals for full PPRD
				bool showInternals = rddDocument == null;
				// clone the main container
				StructuredDocumentTag sectionContainer = sectionContainerTemplate.Clone(true) as StructuredDocumentTag;
				lastElement = lastElement.ParentNode.InsertAfter(sectionContainer, lastElement);

				// Insert a page break if section is a main section (2.0, 3.0, etc)
				// Page break is not needed for The first section because it will already be on a new page
				if (subsectionLevel == 0 && !firstSection)
				{
					StructuredDocumentTag pageBreakElement =
						WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.PAGE_BREAK);
					Paragraph pageBreak = pageBreakElement.GetChild(NodeType.Paragraph, 0, true) as Paragraph;
					Run run = new Run(mainPart, ControlChar.PageBreak);
					pageBreak.AppendChild(run);
				}

				// Populate the section number and title
				PopulateSectionTitle(sectionContainer, section, subsectionLevel, refNumberPrefixLevel);

				// Separate out the child node elements
				ICollection<SectionModelView> textAndTableMVs =
					section.ChildNodes.Where(x => x.ContentType is SectionContentType.Text or
												  SectionContentType.RateTable or SectionContentType.Address)
						.OrderBy(o => o.DisplayOrder).ToCollection();
				ICollection<SectionModelView> subsectionMVs =
					section.ChildNodes.Where(x => x.ContentType == SectionContentType.Section).ToCollection();

				// Get the container template
				StructuredDocumentTag textAndTableContainerTemplate =
					WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.CONTAINER_TEXTANDTABLES);
				Node lastTextTableElement = textAndTableContainerTemplate;

				// Populate text element(s) and table
				foreach (SectionModelView modelView in textAndTableMVs)
				{
					if (showInternals || !modelView.IsInternalSection.Value)
					{
						// clone the main container
						StructuredDocumentTag textAndTableContainer = textAndTableContainerTemplate.Clone(true) as StructuredDocumentTag;
						lastTextTableElement = lastTextTableElement.ParentNode.InsertAfter(textAndTableContainer, lastTextTableElement);

						// Get text and table elements
						StructuredDocumentTag textElement = WordUtilities.GetTaggedChildElement(textAndTableContainer,
							PPRDExporterConstants.FIELDNAME_TEXTELEMENT);
						StructuredDocumentTag rateTableElement =
							WordUtilities.GetTaggedChildElement(textAndTableContainer, PPRDExporterConstants.TABLE_RATES);
						StructuredDocumentTag addressTableElement =
							WordUtilities.GetTaggedChildElement(textAndTableContainer, PPRDExporterConstants.TABLE_ADDRESS);

						if (modelView.ContentType == SectionContentType.Text && textElement != null)
						{
							WordUtilities.SetElementTextWithHTML(mainPart, textElement, modelView.TextContent, false);

							if (modelView.IsInternalSection ?? false)
							{
								ApplyInternalSectionFormatting(textElement);
							}

							// Remove table elements
							RemoveElement(rateTableElement);
							RemoveElement(addressTableElement);
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
								PopulateRateTable(rateTableElement, sectionRates, firstYear, totalYears,
									modelView.DisplayRateCode);
							}
							else
							{
								// Make duplicate/template of table element
								StructuredDocumentTag templateTableElement = rateTableElement;
								StructuredDocumentTag currentInsertionElement = templateTableElement;

								// Get starting years for each version of the table
								ICollection<int> startYears = new Collection<int>();

								for (int i = 0; i < totalYears; i += PPRDExporterConstants.MAX_TABLE_ROW_YEARS)
								{
									startYears.Add(firstYear + i);
								}

								// Populate the tables
								foreach (int startYear in startYears)
								{
									StructuredDocumentTag tableElement = templateTableElement.Clone(true) as StructuredDocumentTag;

									// Number of years will be the max for all tables but the last, which will be the remaining number of years
									int years = startYear == startYears.Last()
										? firstYear + totalYears - startYear
										: PPRDExporterConstants.MAX_TABLE_ROW_YEARS - 1;

									PopulateRateTable(tableElement, sectionRates, startYear, years,
										modelView.DisplayRateCode);

									currentInsertionElement.ParentNode.InsertAfter(tableElement, currentInsertionElement);
									currentInsertionElement = tableElement;
								}

								// Remove the template table element
								templateTableElement.Remove();
							}

							// Remove the text address element
							RemoveElement(textElement);
							RemoveElement(addressTableElement);
						}
						else if (modelView.ContentType == SectionContentType.Address && addressTableElement != null)  //new address table code here
						{
							// Populate Address Table
							StructuredDocumentTag addressOffice = WordUtilities.GetTaggedChildElement(addressTableElement, PPRDExporterConstants.FIELDNAME_ADDRESSOFFICE);
							WordUtilities.SetElementText(addressOffice, modelView.Office);

							StructuredDocumentTag addressAgency = WordUtilities.GetTaggedChildElement(addressTableElement, PPRDExporterConstants.FIELDNAME_ADDRESSAGENCY);
							WordUtilities.SetElementText(addressAgency, modelView.Agency);

							StructuredDocumentTag addressLMBA = WordUtilities.GetTaggedChildElement(addressTableElement, PPRDExporterConstants.FIELDNAME_ADDRESSLMBA);
							WordUtilities.SetElementText(addressLMBA, modelView.LMBA);

							StructuredDocumentTag addressName = WordUtilities.GetTaggedChildElement(addressTableElement, PPRDExporterConstants.FIELDNAME_ADDRESSNAME);
							WordUtilities.SetElementText(addressName, modelView.Name);

							StructuredDocumentTag addressStreet = WordUtilities.GetTaggedChildElement(addressTableElement, PPRDExporterConstants.FIELDNAME_ADDRESSSTREET);
							WordUtilities.SetElementText(addressStreet, modelView.Street);

							StructuredDocumentTag addressCity = WordUtilities.GetTaggedChildElement(addressTableElement, PPRDExporterConstants.FIELDNAME_ADDRESSCITY);
							WordUtilities.SetElementText(addressCity, modelView.CityST);

							StructuredDocumentTag addressPhone = WordUtilities.GetTaggedChildElement(addressTableElement, PPRDExporterConstants.FIELDNAME_ADDRESSPHONE);
							WordUtilities.SetElementText(addressPhone, modelView.Phone);

							StructuredDocumentTag addressEmail = WordUtilities.GetTaggedChildElement(addressTableElement, PPRDExporterConstants.FIELDNAME_ADDRESSEMAIL);
							WordUtilities.SetElementText(addressEmail, modelView.Email);

							StructuredDocumentTag addressOther = WordUtilities.GetTaggedChildElement(addressTableElement, PPRDExporterConstants.FIELDNAME_ADDRESSOTHER);
							WordUtilities.SetElementText(addressOther, modelView.Other);


							// Adjust bottom border thickness
							Table addressTable = addressTableElement.GetChild(NodeType.Table, 0, true) as Table;
							AdjustTableBorders(addressTable);

							// Remove the text element
							RemoveElement(textElement);
							RemoveElement(rateTableElement);
						}
						else
						{
							// Remove all elements
							RemoveElement(textElement);
							RemoveElement(rateTableElement);
							RemoveElement(addressTableElement);
						}
					}
				}

				// Get the file attachments for this section
				ICollection<FileAttachmentRowModelView> sectionFileAttachments = fileAttachments.Where(f => f.SectionId == section.Id).ToList();
				AddFileAttachments(mainPart, textAndTableContainerTemplate, ref lastTextTableElement, sectionFileAttachments);

				// Remove the template element
				RemoveElement(textAndTableContainerTemplate);

				subsectionLevel++;

				// Reverse iterate over sections so they will be properly shown in the document
				foreach (SectionModelView subsectionElement in subsectionMVs.Reverse())
				{
					// Recursive call to populate subsections
					PopulateSectionContainer(subsectionElement, rates, fileAttachments, publishYear, rateTableYears, subsectionLevel, sectionContainerTemplate, lastElement, mainPart, rddDocument, firstSection, refNumberPrefixLevel);
				}
			}
		}

		/// <summary>
		/// Adds the file attachments to a clone of the specified template.
		/// </summary>
		/// <param name="mainPart">The main part.</param>
		/// <param name="textAndTableContainerTemplate">The text and table container template.</param>
		/// <param name="lastTextTableElement">The last text table element.</param>
		/// <param name="sectionFileAttachments">The section file attachments.</param>
		private void AddFileAttachments(Document mainPart, StructuredDocumentTag textAndTableContainerTemplate, ref Node lastTextTableElement, ICollection<FileAttachmentRowModelView> sectionFileAttachments)
		{
			if (sectionFileAttachments.Any())
			{
				foreach (FileAttachmentRowModelView attachment in sectionFileAttachments)
				{
					// clone the main container
					StructuredDocumentTag textAndTableContainer = textAndTableContainerTemplate.Clone(true) as StructuredDocumentTag;
					lastTextTableElement = lastTextTableElement.ParentNode.InsertAfter(textAndTableContainer, lastTextTableElement);

					// Get text and table elements
					StructuredDocumentTag textElement = WordUtilities.GetTaggedChildElement(textAndTableContainer,
						PPRDExporterConstants.FIELDNAME_TEXTELEMENT);
					StructuredDocumentTag rateTableElement =
						WordUtilities.GetTaggedChildElement(textAndTableContainer, PPRDExporterConstants.TABLE_RATES);
					StructuredDocumentTag addressTableElement =
							WordUtilities.GetTaggedChildElement(textAndTableContainer, PPRDExporterConstants.TABLE_ADDRESS);

					// Create the hyperlink text
					string hyperlink = string.Format("<a href=\"{1}\">{0}</a>", attachment.Name, attachment.Link);

					// Set the hyperlink
					WordUtilities.SetElementTextWithHTML(mainPart, textElement, hyperlink, false);

					// Remove table elements
					RemoveElement(rateTableElement);
					RemoveElement(addressTableElement);
				}
			}
		}

		/// <summary>
		/// Populate the section title and number
		/// </summary>
		/// <param name="sectionContainer">Section Container</param>
		/// <param name="section">Section ModelView</param>
		/// <param name="subsectionLevel">Section level - 0 for top-level, 1 for subsection, etc</param>
		/// <param name="refNumberPrefixLevel">The prefix level for the Reference Numbers.</param>
		private void PopulateSectionTitle(StructuredDocumentTag sectionContainer, SectionModelView section, int subsectionLevel, int refNumberPrefixLevel)
		{
			// Get section title container elements
			StructuredDocumentTag sectionTitleContainerElement =
				WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.CONTAINER_SECTIONTITLE);
			StructuredDocumentTag subsectionTitleContainerElement =
				WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.CONTAINER_SUBSECTIONTITLE);
			StructuredDocumentTag subsubsectionTitleContainerElement =
				WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.CONTAINER_SUBSUBSECTIONTITLE);

			if (subsectionLevel + refNumberPrefixLevel == 0)
			{
				// Top-level section (ex. "1.0")
				if (sectionTitleContainerElement != null)
				{
					// Populate Section number and title
					StructuredDocumentTag sectionNumberElement = WordUtilities.GetTaggedChildElement(sectionTitleContainerElement,
						PPRDExporterConstants.FIELDNAME_SECTIONNUMBER);
					if (sectionNumberElement != null)
					{
						WordUtilities.SetElementText(sectionNumberElement, section.ReferenceNumber + ControlChar.LineBreak);

						if (section.IsInternalSection == true)
						{
							ApplyInternalSectionFormatting(sectionNumberElement);
						}
					}

					StructuredDocumentTag sectionTitleElement = WordUtilities.GetTaggedChildElement(sectionTitleContainerElement,
						PPRDExporterConstants.FIELDNAME_SECTIONTITLE);
					if (sectionTitleElement != null)
					{
						WordUtilities.SetElementText(sectionTitleElement, section.Title);

						if (section.IsInternalSection == true)
						{
							ApplyInternalSectionFormatting(sectionTitleElement);
						}
					}
				}

				// Remove other containers
				RemoveIt(subsectionTitleContainerElement);
				RemoveIt(subsubsectionTitleContainerElement);
			}
			else if (subsectionLevel + refNumberPrefixLevel == 1)
			{
				// First level subsection (ex. 1.1)
				if (subsectionTitleContainerElement != null)
				{
					// Populate Section number and title
					StructuredDocumentTag subsectionNumberElement = WordUtilities.GetTaggedChildElement(subsectionTitleContainerElement,
						PPRDExporterConstants.FIELDNAME_SUBSECTIONNUMBER);
					if (subsectionNumberElement != null)
					{
						WordUtilities.SetElementText(subsectionNumberElement, section.ReferenceNumber);

						if (section.IsInternalSection == true)
						{
							ApplyInternalSectionFormatting(subsectionNumberElement);
						}
					}

					StructuredDocumentTag subsectionTitleElement = WordUtilities.GetTaggedChildElement(subsectionTitleContainerElement,
						PPRDExporterConstants.FIELDNAME_SUBSECTIONTITLE);
					if (subsectionTitleElement != null)
					{
						WordUtilities.SetElementText(subsectionTitleElement, section.Title);

						if (section.IsInternalSection == true)
						{
							ApplyInternalSectionFormatting(subsectionTitleElement);
						}
					}
				}

				// Remove other containers
				RemoveIt(sectionTitleContainerElement);
				RemoveIt(subsubsectionTitleContainerElement);
			}
			else
			{
				// Second level or lower (ex. 1.1.1)
				if (subsubsectionTitleContainerElement != null)
				{
					// Populate Section number and title
					StructuredDocumentTag subsubsectionNumberElement = WordUtilities.GetTaggedChildElement(subsubsectionTitleContainerElement,
						PPRDExporterConstants.FIELDNAME_SUBSUBSECTIONNUMBER);
					if (subsubsectionNumberElement != null)
					{
						WordUtilities.SetElementText(subsubsectionNumberElement, section.ReferenceNumber);

						if (section.IsInternalSection == true)
						{
							ApplyInternalSectionFormatting(subsubsectionNumberElement);
						}
					}

					StructuredDocumentTag subsubsectionTitleElement = WordUtilities.GetTaggedChildElement(subsubsectionTitleContainerElement,
						PPRDExporterConstants.FIELDNAME_SUBSUBSECTIONTITLE);
					if (subsubsectionTitleElement != null)
					{
						WordUtilities.SetElementText(subsubsectionTitleElement, section.Title);

						if (section.IsInternalSection == true)
						{
							ApplyInternalSectionFormatting(subsubsectionTitleElement);
						}
					}
				}

				// Remove other containers
				RemoveIt(sectionTitleContainerElement);
				RemoveIt(subsectionTitleContainerElement);
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
		private void PopulateRateTable(StructuredDocumentTag rateTableElement, ICollection<RateDetailModelView> rates, int startYear, int years, bool? showRateCode)
		{
			// Populate header row labels
			StructuredDocumentTag firstYearLabel = WordUtilities.GetTaggedChildElement(rateTableElement, PPRDExporterConstants.LABEL_YEAR);
			if (firstYearLabel != null)
			{
				WordUtilities.SetElementText(firstYearLabel, startYear);
			}

			// Remove Rate Code Table if needed and adjust cell widths
			if (showRateCode == false)
			{
				StructuredDocumentTag rateCodeColumn = WordUtilities.GetTaggedChildElement(rateTableElement, PPRDExporterConstants.LABEL_RATECODE);
				StructuredDocumentTag descriptionColumn = WordUtilities.GetTaggedChildElement(rateTableElement, PPRDExporterConstants.LABEL_DESCRIPTION);
				Cell descriptionHeaderCell = descriptionColumn.GetAncestor(NodeType.Cell) as Cell;

				WordUtilities.RemoveColumnFromTable(rateCodeColumn);

				if (descriptionHeaderCell != null)
				{
					SetCellWidth(descriptionHeaderCell, PPRDExporterConstants.DESCRIPTION_NO_RATE_CODE_COLUMN_WIDTH);
				}
			}

			Table rateTable = rateTableElement.GetChild(NodeType.Table, 0, true) as Table;
			if (rateTable != null)
			{
				// Make sure preferred table width is not set
				RemoveTablePreferredWidth(rateTable);

				// Set header row property to keep header between page breaks
				Row headerRow = rateTable.FirstRow;
				SetHeaderRow(headerRow);
				AdjustRowBorders(headerRow);

				// Append additional years to header row
				for (int i = 1; i <= years; i++)
				{
					AppendCellToRow(headerRow, (startYear + i).ToString());
				}

				// Initialize "insertion" row
				Row templateDataRow = rateTable.Rows[1];
				SetCannotSplit(templateDataRow);
				Row currentInsertionRow = templateDataRow;

				foreach (RateDetailModelView rate in rates)
				{
					// Create new row in the table
					Row row = CloneMarkedTemplateRow(templateDataRow);

					// Populate the row
					StructuredDocumentTag rateDescription = WordUtilities.GetTaggedChildElement(row, PPRDExporterConstants.FIELDNAME_RATECODEDESCRIPTION);

					if (showRateCode == true)
					{
						StructuredDocumentTag rateCode = WordUtilities.GetTaggedChildElement(row, PPRDExporterConstants.FIELDNAME_RATECODE);
						WordUtilities.SetElementText(rateCode, rate.RateCode);
					}
					else
					{
						Cell rateDescriptionCell = rateDescription.GetChild(NodeType.Cell, 0, true) as Cell;
						if (rateDescriptionCell != null)
						{
							SetCellWidth(rateDescriptionCell, PPRDExporterConstants.DESCRIPTION_NO_RATE_CODE_COLUMN_WIDTH);
						}
					}

					WordUtilities.SetElementText(rateDescription, rate.Description);

					StructuredDocumentTag initialYear = WordUtilities.GetTaggedChildElement(row, PPRDExporterConstants.FIELDNAME_RATECODEVALUE);
					RateYearModelView initialRateYearMV = rate.Values.FirstOrDefault(x => x.Year == startYear) ?? new RateYearModelView();
					string initialYearValue = rateFormatter.FormatRate(RateTarget.PPRD, rate.RateCategoryDescription, initialRateYearMV.Value);
					WordUtilities.SetElementText(initialYear, initialYearValue);

					// Append cells for additional years
					bool isRowEmpty = initialYearValue.Equals(CommonConstants.NOT_APPLICABLE);
					for (int i = 1; i <= years; i++)
					{
						RateYearModelView rateYearMV = rate.Values.FirstOrDefault(x => x.Year == startYear + i) ?? new RateYearModelView();
						string yearValue = rateFormatter.FormatRate(RateTarget.PPRD, rate.RateCategoryDescription, rateYearMV.Value);
						isRowEmpty = isRowEmpty && yearValue.Equals(CommonConstants.NOT_APPLICABLE);
						AppendCellToRow(row, yearValue);
					}

					// Add the row to the table (unless the row is empty, i.e. all values are "N/A")
					if (!isRowEmpty)
					{
						currentInsertionRow.ParentNode.InsertAfter(row, currentInsertionRow);
						currentInsertionRow = row;
					}
				}

				// Remove template row
				templateDataRow.Remove();

				// Adjust bottom border thickness
				AdjustTableBorders(rateTable);
			}
		}

		/// <summary>
		/// Populates the File Attachments Section.
		/// </summary>
		/// <param name="fileAttachments">The file attachments.</param>
		/// <param name="sectionContainerTemplate">The section container template.</param>
		/// <param name="lastElement">The last element.</param>
		/// <param name="Document">The main document part.</param>
		/// <param name="lastSection">The last section of the document (if one exists).</param>
		/// <param name="refNumberPrefixLevel">The prefix Level for the Reference Numbers.</param>
		private void PopulateFileAttachments(ICollection<FileAttachmentRowModelView> fileAttachments, 
			StructuredDocumentTag sectionContainerTemplate, Node lastElement, Document Document, SectionModelView lastSection, 
			int refNumberPrefixLevel)
		{
			ICollection<FileAttachmentRowModelView> attachments = fileAttachments.Where(f => f.SectionId == 0).ToList();

			if (attachments.Any())
			{
				// clone the main container
				StructuredDocumentTag sectionContainer = sectionContainerTemplate.Clone(true) as StructuredDocumentTag;
				lastElement = lastElement.ParentNode.InsertAfter(sectionContainer, lastElement);

				// Create the ReferenceNumber
				string referenceNumber = FIRST_SECTION_REFERENCE_NUMBER;
				if (lastSection != null)
				{
					double refNumber = double.Parse(lastSection.ReferenceNumber);
					refNumber++;
					referenceNumber = refNumber.ToString("###.0");
				}

				// Populate the section title using a fake SectionModel
				SectionModelView attachmentSection = new()
				{
					Title = FILE_ATTACHMENTS_TITLE,
					IsInternalSection = false,
					ReferenceNumber = referenceNumber
				};

				// Populate the section number and title
				PopulateSectionTitle(sectionContainer, attachmentSection, 0, refNumberPrefixLevel);

				// Get the container template
				StructuredDocumentTag textAndTableContainerTemplate =
					WordUtilities.GetTaggedChildElement(sectionContainer, PPRDExporterConstants.CONTAINER_TEXTANDTABLES);
				Node lastTextTableElement = textAndTableContainerTemplate;

				// Add the file attachments
				AddFileAttachments(Document, textAndTableContainerTemplate, ref lastTextTableElement, attachments);

				// Remove the template element
				RemoveElement(textAndTableContainerTemplate);
			}
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Perform the final cleanup of the document
		/// </summary>
		/// <param name="document">the document to clean</param>
		private void PerformFinalDocumentCleanup(Document document)
		{
			// Set View to Print layout
			document.ViewOptions.ViewType = Aspose.Words.Settings.ViewType.PageLayout;
			
			// Remove content controls
			WordUtilities.RemoveContentControls(document);

			// Clean up XML
			WordUtilities.CleanupDocumentXml(document);
		}

		/// <summary>
		/// Apply formatting for Internal Sections - blue, italic text
		/// </summary>
		/// <param name="element">element to apply formatting to</param>
		private void ApplyInternalSectionFormatting(StructuredDocumentTag element)
		{
			HashSet<Run> runs = element.GetChildNodes<Run>(NodeType.Run, true);
			
			if (runs.Any())
			{
				foreach (Run run in runs)
				{
					run.Font.Italic = true;
					run.Font.Color = ColorTranslator.FromHtml("#" + PPRDExporterConstants.INTERNALSECTIONTEXTCOLOR); // blue
				}
			}
		}

		/// <summary>
		/// Append cell to the end of a row, using the formatting and properties of that row
		/// </summary>
		/// <param name="row">Row to append cell to</param>
		/// <param name="text">Text for cell</param>
		private void AppendCellToRow(Row row, string text)
		{
			if (row != null)
			{
				// Get cell to use as template for properties - match with last cell in row, the one it will go next to
				Cell templateCell = row.LastCell;

				if (templateCell != null)
				{
					// Set the text and cell properties
					Cell cell = templateCell.Clone(false) as Cell;
					
					// Add text and run properties to run
					Run run = new(row.Document);
					run.Text = text;

					// Add run and paragraph properties to paragraph, add paragraph to cell
					Paragraph paragraph = new(row.Document);
					paragraph.AppendChild(run);
					cell.AppendChild(paragraph);

					// Append cell to row
					row.AppendChild(cell);
				}
			}
		}

		/// <summary>
		/// Set the header row property so header will show between page breaks
		/// </summary>
		/// <param name="headerRow">Row to set as header</param>
		private void SetHeaderRow(Row headerRow)
		{
			headerRow.RowFormat.HeadingFormat = true;
		}

		/// <summary>
		/// Adjust the bottom and right borders of the table to 1.5pt
		/// </summary>
		/// <param name="table">The table</param>
		private void AdjustTableBorders(Table table)
		{
			// TODO TIW is black correct color?
			table.SetBorder(BorderType.Bottom, LineStyle.Single, 1.5, Color.Black, true); 
			table.SetBorder(BorderType.Right, LineStyle.Single, 1.5, Color.Black, true);
		}

		/// <summary>
		/// Adjust the bottom border of the row to 1.5pt
		/// </summary>
		/// <param name="row">The row</param>
		private void AdjustRowBorders(Row row)
		{
			// There are no border properties for rows, so need to apply to each cell in the row
			foreach (Cell cell in row.Cells)
			{
				cell.CellFormat.Borders.Bottom.LineWidth = 1.5; // 1.5 point border
			}
		}

		/// <summary>
		/// Set the fixed width for a cell
		/// </summary>
		/// <param name="cell">Cell to adjust</param>
		/// <param name="width">Width to set (inches)</param>
		private void SetCellWidth(Cell cell, double width)
		{
			// Convert width from inches to points
			// points = inches*72 
			double convertedWidth = width * 72d;

			cell.CellFormat.PreferredWidth = PreferredWidth.FromPoints(convertedWidth);
		}

		/// <summary>
		/// Removes the Preferred width setting from the table
		/// </summary>
		/// <param name="table">Table to adjust</param>
		private void RemoveTablePreferredWidth(Table table)
		{
			table.PreferredWidth = null;
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