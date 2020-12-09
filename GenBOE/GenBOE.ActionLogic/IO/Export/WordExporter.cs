// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;

    [ExcludeFromCodeCoverage]
    public abstract class WordExporter : BOEExportUtilities
    {
        protected WordExporter(IUserDTODataLoader UserDTODataLoader)
            : base(UserDTODataLoader)
        {
        }

        #region Methods

        /// <summary>
        /// Run the export: Read the Word template file into an in-memory OpenXml Wordprocessing document, call the designated data
        /// population method, then return the results as a byte stream.
        /// </summary>
        /// <param name="templateFilePathFull">Full path to the Word template file</param>
        /// <param name="populateData">(Optional) parameters and data to be passed back into the <code>PopulateData</code> method</param>
        /// <param name="stream">Stream into which to write the exported Word document.</param>
        protected void Export(string templateFilePathFull, Action<WordprocessingDocument> populateData, Stream stream)
        {
            // open a copy of the Excel template file into memory
            byte[] byteArray = File.ReadAllBytes(templateFilePathFull);

            this.Export(byteArray, populateData, stream);
        }

        /// <summary>
        /// Run the export: Read the Word template file into an in-memory OpenXml Wordprocessing document, call the designated data
        /// population method, then return the results as a byte stream.
        /// </summary>
        /// <param name="byteArray">Contents of the Word template file</param>
        /// <param name="populateData">(Optional) parameters and data to be passed back into the <code>PopulateData</code> method</param>
        /// <param name="stream">Stream into which to write the exported Word document.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
        protected void Export(byte[] byteArray, Action<WordprocessingDocument> populateData, Stream stream)
        {
            if (byteArray == null)
            {
                throw new ArgumentNullException(nameof(byteArray));
            }

            if (populateData == null)
            {
                throw new ArgumentNullException(nameof(populateData));
            }

            string tempFilename = Path.GetTempFileName();
            new FileInfo(tempFilename).Attributes |= FileAttributes.Temporary;
            using (Stream documentStream = new FileStream(tempFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose))
            {
                documentStream.Write(byteArray, 0, byteArray.Length);

                // synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    // Create the document object in memory
                    using (WordprocessingDocument document = WordprocessingDocument.Open(documentStream, true))
                    {
                        // Call the worker method to load-in the data
                        populateData(document);

                        // Save all the changes
                        this.SaveDocument(document);
                    }

                    // write the document from the file into the caller's stream
                    documentStream.Seek(0, SeekOrigin.Begin);
                    documentStream.CopyTo(stream);
                }
            }
        }

        /// <summary>
        /// Save the document.
        /// </summary>
        /// <param name="document">The OpenXml Word document object</param>
        protected void SaveDocument(WordprocessingDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.MainDocumentPart.Document.Save();
        }

        #endregion
    }

    [ExcludeFromCodeCoverage]
    public static class OpenXmlExtensions
    {
        public static void RemoveIt(this OpenXmlElement element)
        {
            if (element != null && element.Parent != null)
            {
                element.Remove();
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class BOEExportUtilities
    {
        protected IUserDTODataLoader _IUserDTODataLoader { get; set; }

        public BOEExportUtilities(IUserDTODataLoader UserDTODataLoader)
        {
            this._IUserDTODataLoader = UserDTODataLoader;
        }

        /// <summary>
        /// Determines the boe export approvers.
        /// </summary>
        /// <param name="boeId">The boe identifier.</param>
        /// <param name="boeMappingWithApproverResponses">The boe mapping with approver responses.</param>
        /// <param name="getUserDataForBoesForWs">The get user data for boes for ws.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">boeMappingWithApproverResponses</exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected Collection<BoeExportApproverInfo> DetermineBoeExportApprovers(int boeId, IDictionary<int, ICollection<BoeApproverResponseDTO>> boeMappingWithApproverResponses, IReadOnlyCollection<UserDTO> getUserDataForBoesForWs)
        {
            if (boeMappingWithApproverResponses == null) { throw new ArgumentNullException(nameof(boeMappingWithApproverResponses)); }

            Collection<BoeExportApproverInfo> boeExportApprovers = new Collection<BoeExportApproverInfo>();

            // Pull the approvers from the mapping so we do not go to the DB for every BOE
            ICollection<BoeApproverResponseDTO> boeApprovers;

            if (boeMappingWithApproverResponses.TryGetValue(boeId, out boeApprovers))
            {
                foreach (BoeApproverResponseDTO boeApprover in boeApprovers)
                {
                    BoeExportApproverInfo boeExportApprover = new BoeExportApproverInfo();
                    UserDTO user = getUserDataForBoesForWs.FirstOrDefault(x => x.UserID == boeApprover.ETIUserID);

                    if (user != null)
                    {
                        string reportName = this.GenerateReportFormattedName(user);

                        if (boeApprover.ApproverResponse == ApproverReponseType.Approved)
                        {
                            boeExportApprover.ApprovedBy = reportName + BOEExporterConstants.BLANK_SPACE + CommonConstants.SIGNED;
                            boeExportApprover.ApprovedDate = boeApprover.UpdateDate.ToShortDateString();
                        }
                        else
                        {
                            boeExportApprover.ApprovedBy = reportName;
                            boeExportApprover.ApprovedDate = string.Empty; // if the BOE hasn't been approved, the date field should be empty
                        }

                        boeExportApprovers.Add(boeExportApprover);
                    }
                }
            }

            return boeExportApprovers;
        }

        /// <summary>
        /// Determines the boe export authors.
        /// </summary>
        /// <param name="boe">The boe.</param>
        /// <param name="boeIdsAndLastUserToSubmitThemForApprovalMapping">The boe ids and last user to submit them for approval mapping.</param>
        /// <param name="getUserDataForBoesForWs">The get user data for boes for ws.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// boe
        /// or
        /// boeIdsAndLastUserToSubmitThemForApprovalMapping
        /// </exception>
        protected Collection<string> DetermineBoeExportAuthors(BoeDTO boe, IDictionary<int, UserDTO> boeIdsAndLastUserToSubmitThemForApprovalMapping, IReadOnlyCollection<UserDTO> getUserDataForBoesForWs)
        {
            if (boe == null) { throw new ArgumentNullException(nameof(boe)); }
            if (boeIdsAndLastUserToSubmitThemForApprovalMapping == null) { throw new ArgumentNullException(nameof(boeIdsAndLastUserToSubmitThemForApprovalMapping)); }

            Collection<string> boeExportAuthors = new Collection<string>();
            bool wasSubmittedForApproval = (boe.State == BOEState.AwaitingApproval || boe.State == BOEState.Approved);
            UserDTO lastSubmitterUser = null;
            int lastSubmitterUserId = 0;

            if (wasSubmittedForApproval)
            {
                // if the BOE has been submitted, the boe history needs to be examined to determine which 
                // of the potential multiple Authors submitted it for Approval
                boeIdsAndLastUserToSubmitThemForApprovalMapping.TryGetValue(boe.Id, out lastSubmitterUser);

                if (lastSubmitterUser != null)
                {
                    lastSubmitterUserId = lastSubmitterUser.UserID;
                }
            }

            if (boe.AuthorIDs.Any() || boe.SubcontractorAuthorIDs.Any())
            {
                foreach (int authorId in boe.SubcontractorAuthorIDs)
                {
                    UserDTO user = getUserDataForBoesForWs.FirstOrDefault(x => x.UserID == authorId);

                    if (user != null)
                    {
                        string reportName = this.GenerateReportFormattedName(user);

                        boeExportAuthors.Add(authorId == lastSubmitterUserId ?
                            reportName + CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX + BOEExporterConstants.BLANK_SPACE + CommonConstants.SIGNED :
                            reportName + CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX);
                    }
                }

                foreach (int authorId in boe.AuthorIDs)
                {
                    UserDTO user = getUserDataForBoesForWs.FirstOrDefault(x => x.UserID == authorId);

                    if (user != null)
                    {
                        string reportName = this.GenerateReportFormattedName(user);

                        boeExportAuthors.Add(authorId == lastSubmitterUserId ?
                            reportName + BOEExporterConstants.BLANK_SPACE + CommonConstants.SIGNED :
                            reportName);
                    }
                }
            }

            // Add submitteruserid if it wasn't an author or sub
            if (lastSubmitterUserId != 0 &&
                !boe.AuthorIDs.Contains(lastSubmitterUserId) &&
                !boe.SubcontractorAuthorIDs.Contains(lastSubmitterUserId))
            {
                if (lastSubmitterUser != null)
                {
                    string reportName = this.GenerateReportFormattedName(lastSubmitterUser);
                    boeExportAuthors.Add(reportName + BOEExporterConstants.BLANK_SPACE + CommonConstants.SIGNED);
                }
            }

            return boeExportAuthors;
        }

        /// <summary>
        /// Generates a report name in the format "Firstname MI. Lastname"
        /// </summary>
        /// <param name="user">user to format</param>
        /// <returns>Formatted name</returns>
        private string GenerateReportFormattedName(UserDTO user)
        {
            if (!string.IsNullOrEmpty(user.MiddleName))
            {
                return user.FirstName + BOEExporterConstants.BLANK_SPACE + user.MiddleName.Substring(0, 1) + "."
                    + BOEExporterConstants.BLANK_SPACE + user.LastName;
            }
            else
            {
                return user.FirstName + BOEExporterConstants.BLANK_SPACE + user.LastName;
            }
        }

        /// <summary>
        /// Do not allow the contents of the row to be split between pages.
        /// </summary>
        /// <param name="row">The row</param>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Cant")]
        protected void SetCantSplit(TableRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            TableRowProperties tableRowProperties;
            if ((tableRowProperties = row.TableRowProperties) == null)
            {
                tableRowProperties = new TableRowProperties();
                row.Append(tableRowProperties);
            }

            if (!tableRowProperties.Descendants<CantSplit>().Any())
            {
                CantSplit cantSplit = new CantSplit();
                tableRowProperties.Append(cantSplit);
            }
        }

        /// <summary>
        /// Clone a table row
        /// </summary>
        /// <param name="markedRow">Table row to clone</param>
        /// <returns>Cloned copy of the table row</returns>
        protected TableRow CloneMarkedTemplateRow(TableRow markedRow)
        {
            if (markedRow == null)
            {
                throw new ArgumentNullException(nameof(markedRow));
            }

            TableRow clonedRow = markedRow.CloneNode(true) as TableRow;

            #region Delete IDs to avoid conflict with existing elements

            foreach (SdtId id in clonedRow.Descendants<SdtId>())
            {
                id.Remove();
            }

            foreach (SdtPlaceholder placeholder in clonedRow.Descendants<SdtPlaceholder>())
            {
                placeholder.Remove();
            }

            #endregion

            return clonedRow;
        }

        /// <summary>
        /// Remove an element from the document (DOM)
        /// </summary>
        /// <param name="element">Element to remove</param>
        protected void RemoveElement(OpenXmlElement element)
        {
            if (element == null)
            {
                return;
            }

            OpenXmlElement parentElement = element.Parent;

            if (parentElement != null)  // was the element already removed?
            {
                element.Remove();

                // enforce well-formedness of table cell XML (i.e. must contain a paragraph)
                if (parentElement is TableCell && !parentElement.Descendants<Paragraph>().Any())
                {
                    TableRow row = parentElement.Parent as TableRow;
                    if (row != null && row.Descendants<TableCell>().Count() == 1)
                    {
                        row.Remove();  // if this is the only cell, then (because it is empty) just remove the entire row
                    }
                    else
                    {
                        parentElement.AppendChild(new Paragraph());
                    }
                }
            }
        }

        /// <summary>
        /// Remove the table row containing the element
        /// If no row, just remove the element
        /// </summary>
        /// <param name="element">Sdt Element in the row to remove</param>
        protected void RemoveElementRow(SdtElement element)
        {
            if (element != null)
            {
                TableRow row = element.Ancestors<TableRow>().FirstOrDefault();
                if (row != null)
                {
                    row.RemoveIt();
                }
                else
                {
                    row.Parent.RemoveIt();
                }
            }
        }

        /// <summary>
        /// Populate the MOQ Type Data
        /// </summary>
        /// <param name="laborTaskElement">Labor Task Element</param>
        /// <param name="selectedComponents">Selected Components</param>
        /// <param name="mainDocumentPart">Main Document Part</param>
        /// <param name="moqTypeContainerTemplate">MOQ container template element</param>
        /// <param name="customExport">If using custom exporter</param>
        /// <param name="counters">counters</param>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "5#")]
        protected void PopulateMOQTypeData(BOEExportTaskElement laborTaskElement, ICollection<BoeCustomReportComponent> selectedComponents,
            MainDocumentPart mainDocumentPart, SdtElement moqTypeContainerTemplate, bool customExport, ref ChunkCounter counters)
        {
            _ = laborTaskElement ?? throw new ArgumentNullException(nameof(laborTaskElement));
            _ = selectedComponents ?? throw new ArgumentNullException(nameof(selectedComponents));

            if (moqTypeContainerTemplate != null)
            {
                // create/clone template of MOQ Type fields
                SdtElement moqTypeContainer = null;
                OpenXmlElement lastElement = moqTypeContainerTemplate;
            
                foreach (MoqTypeSelection moqType in laborTaskElement.MOQTypes)
                {
                    moqTypeContainer = moqTypeContainerTemplate.CloneNode(true) as SdtElement;
                    lastElement = lastElement.InsertAfterSelf<SdtElement>(moqTypeContainer);

                    #region container elements

                    SdtElement cerPmArContainer = WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.Container_CerPmAr);
                    SdtElement sowLoeContainer = WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.Container_SowLoe);
                    SdtElement smeContainer = WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.Container_SME);
                    SdtElement moqTypeTableContainer = WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.Table_MOQType);
                    SdtElement rationaleContainer = WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.Container_MOQTypeRationale);
                    SdtElement skillMixContainer = WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.Container_SkillMix);

                    #endregion

                    // populate unique fields and remove unused fields for MOQ Type                    
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_MOQTypeSelection),
                        moqType.SelectedMOQType.GetDescription());
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_MOQTypeDescription),
                        this.GetMoqTypeDescription(moqType.SelectedMOQType, laborTaskElement.MOQTypes.Count > 1));

                    switch (moqType.SelectedMOQType)
                    {
                        case MOQType.Historical:
                        case MOQType.Comparative:
                            if (customExport)
                            {
                                this.RemoveElement(cerPmArContainer);
                                this.RemoveElement(sowLoeContainer);
                                this.RemoveElement(smeContainer);
                            }
                            else
                            {
                                this.RemoveCerPrArRows(moqTypeContainer);
                                this.RemoveSoeLowRows(moqTypeContainer);
                                this.RemoveSMERows(moqTypeContainer);
                            }

                            this.PopulateMOQTableData(moqType, selectedComponents, moqTypeTableContainer);
                            break;
                        case MOQType.CostEstimatingRelationships:
                        case MOQType.ParametricEstimates:
                        case MOQType.AnalogousRelationships:
                            if (customExport)
                            {
                                this.RemoveElement(sowLoeContainer);
                                this.RemoveElement(smeContainer);
                                this.RemoveElement(moqTypeTableContainer);
                            }
                            else
                            {
                                this.RemoveSoeLowRows(moqTypeContainer);
                                this.RemoveSMERows(moqTypeContainer);
                                this.RemoveMoqTableRow(moqTypeContainer);
                            }

                            string labelPrefix;
                            if (moqType.SelectedMOQType == MOQType.CostEstimatingRelationships)
                            {
                                labelPrefix = "CER";
                            }
                            else if (moqType.SelectedMOQType == MOQType.ParametricEstimates)
                            {
                                labelPrefix = "Parametric model or tool";
                            }
                            else
                            {
                                labelPrefix = "Analogous relationship";
                            }

                            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_CerPmArNameLabel), labelPrefix + " name");
                            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_CerPmArName), moqType.CerName);
                            break;
                        case MOQType.SOW:
                        case MOQType.LOE:
                            if (customExport)
                            {
                                this.RemoveElement(cerPmArContainer);
                                this.RemoveElement(smeContainer);
                                this.RemoveElement(moqTypeTableContainer);
                            }
                            else
                            {
                                this.RemoveCerPrArRows(moqTypeContainer);
                                this.RemoveSMERows(moqTypeContainer);
                                this.RemoveMoqTableRow(moqTypeContainer);
                            }

                            string label = "Description of Hours required";
                            if (moqType.SelectedMOQType == MOQType.SOW)
                            {
                                label += " & location in SOW";
                            }

                            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_HoursDescriptionLabel), label);
                            WordUtilities.SetElementTextWithHTML(mainDocumentPart, WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_HoursDescription),
                                moqType.DescriptionHoursRequired, ref counters, true);                            
                            break;
                        case MOQType.SME:
                            if (customExport)
                            {
                                this.RemoveElement(cerPmArContainer);
                                this.RemoveElement(sowLoeContainer);
                                this.RemoveElement(moqTypeTableContainer);
                            }
                            else
                            {
                                this.RemoveCerPrArRows(moqTypeContainer);
                                this.RemoveSoeLowRows(moqTypeContainer);
                                this.RemoveMoqTableRow(moqTypeContainer);
                            }

                            WordUtilities.SetElementTextWithHTML(mainDocumentPart, WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_SMEReasons),
                                moqType.SmeReason, ref counters, true);
                            WordUtilities.SetElementTextWithHTML(mainDocumentPart, WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_SMEHoursLogic),
                                moqType.SmeHoursLogic, ref counters, true);
                            WordUtilities.SetElementTextWithHTML(mainDocumentPart, WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_SMEDurationLogic),
                                moqType.SmeDurationLogic, ref counters, true);
                            WordUtilities.SetElementTextWithHTML(mainDocumentPart, WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_SMETasks),
                                moqType.SmeTaskEstimates, ref counters, true);
                            break;
                        case MOQType.NonLabor:
                            if (customExport)
                            {
                                this.RemoveElement(cerPmArContainer);
                                this.RemoveElement(sowLoeContainer);
                                this.RemoveElement(smeContainer);
                                this.RemoveElement(moqTypeTableContainer);
                            }
                            else
                            {
                                this.RemoveCerPrArRows(moqTypeContainer);
                                this.RemoveSoeLowRows(moqTypeContainer);
                                this.RemoveSMERows(moqTypeContainer);
                                this.RemoveMoqTableRow(moqTypeContainer);
                            }
                            break;
                        default:
                            break;
                    }

                    // populate rationale/skill mix for all
                    if (moqType.SelectedMOQType != MOQType.SME && (!customExport || selectedComponents.Contains(BoeCustomReportComponent.TaskMOQRationale)))
                    {
                        WordUtilities.SetElementTextWithHTML(mainDocumentPart, WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_Rationale),
                            moqType.Rationale, ref counters, true);
                    }
                    else if (customExport)
                    {
                        this.RemoveElement(rationaleContainer);
                    }
                    else
                    {
                        WordUtilities.RemoveTableRowWithTaggedElement(moqTypeContainer, BOEExporterConstants.FieldName_Rationale);
                    }

                    if (moqType.SelectedMOQType != MOQType.NonLabor)
                    {
                        WordUtilities.SetElementTextWithHTML(mainDocumentPart, WordUtilities.GetTaggedChildElement(moqTypeContainer, BOEExporterConstants.FieldName_SkillMix),
                            moqType.SkillMixRationale, ref counters, true);
                    }
                    else if (customExport)
                    {
                        this.RemoveElement(skillMixContainer);
                    }
                    else
                    {
                        WordUtilities.RemoveTableRowWithTaggedElement(moqTypeContainer, BOEExporterConstants.FieldName_SkillMix);
                    }
                }

                // delete template
                this.RemoveElement(moqTypeContainerTemplate);
            }
        }

        /// <summary>
        /// Get the MOQ Type description
        /// </summary>
        /// <param name="moqType">MOQ Type</param>
        /// <param name="multiMoq">If there is more than one MOQ Type</param>
        /// <returns></returns>
        protected string GetMoqTypeDescription(MOQType moqType, bool multiMoq)
        {
            string toReturn;

            if (multiMoq)
            {
                toReturn = "This portion of the task is based on "; 
            }
            else
            {
                toReturn = "This task is based on ";
            }

            switch (moqType)
            {
                case MOQType.Historical:
                    toReturn += "historical data:";
                    break;
                case MOQType.Comparative:
                    toReturn += "similar historical data:";
                    break;
                case MOQType.CostEstimatingRelationships:
                    toReturn += "a Cost Estimating Relationship (CER):";
                    break;
                case MOQType.ParametricEstimates:
                    toReturn += "Parametric Model/tool:";
                    break;
                case MOQType.AnalogousRelationships:
                    toReturn += "Analogous Relationship:";
                    break;
                case MOQType.SOW:
                    toReturn += "a Statement of Work (SOW) directive:";
                    break;
                case MOQType.LOE:
                    toReturn += "a Level of Effort (LOE):";
                    break;
                case MOQType.SME:
                    toReturn += "Subject Matter Expert (SME) Judgement:";
                    break;
                case MOQType.NonLabor:
                    toReturn = "This task is Non-Labor:";
                    break;
                default:
                    break;
            }

            return toReturn;
        }

        /// <summary>
        /// Populate the MOQ Table data
        /// </summary>
        /// <param name="moqType">MOQ Type containing the table data</param>
        /// <param name="selectedComponents">selected components for the export</param>
        /// <param name="moqTypeTableTemplate">template element for the MOQ Type table</param>
        private void PopulateMOQTableData(MoqTypeSelection moqType, ICollection<BoeCustomReportComponent> selectedComponents, SdtElement moqTypeTableTemplate)
        {
            if (moqTypeTableTemplate != null)
            {
                // create/clone template of MOQ Type table
                SdtElement moqTypeTableContainer = null;
                OpenXmlElement lastElement = moqTypeTableTemplate;
                int? lastTableId = moqType.TableData.LastOrDefault()?.Id;

                foreach (MoqTableData table in moqType.TableData)
                {
                    moqTypeTableContainer = moqTypeTableTemplate.CloneNode(true) as SdtElement;
                    lastElement = lastElement.InsertAfterSelf<SdtElement>(moqTypeTableContainer);

                    // populate shared fields
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_TableName), table.TableName);
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_RepositoryName), table.RepositoryName);
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_QueryType), table.QueryType);
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_DateOfReport), table.DateOfReport.ToString(BOEExporterConstants.DATE_FORMAT_STANDARD));
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_HistoricalProgramName), table.HistoricalProgramName);
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_ContractNumber), table.ContractNumber);
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_WbsElement), table.WbsElement);
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_PoPStartDate), table.PoPStart.ToString(BOEExporterConstants.DATE_FORMAT_STANDARD));
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_PoPEndDate), table.PoPEnd.ToString(BOEExporterConstants.DATE_FORMAT_STANDARD));
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_TotalWBSHours), table.TotalWbsHours.ToString("G29"));
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_TotalRelevantHours), table.TotalRelevantHours.ToString("G29"));
                    
                    // populate/remove Additional Query filters based on selected components
                    if (selectedComponents.Contains(BoeCustomReportComponent.TaskMOQAdditionalQueryFilters) || selectedComponents.Contains(BoeCustomReportComponent.TaskMOQEmployeeIDFilters))
                    {
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_AdditionalQueryFilters), table.AdditionalQueryFilters);
                    }
                    else
                    {
                        WordUtilities.RemoveTableRowWithTaggedElement(moqTypeTableContainer, BOEExporterConstants.FieldName_AdditionalQueryFilters);
                    }

                    // remove the note unless last/only table
                    if (table.Id != lastTableId)
                    {
                        this.RemoveElement(WordUtilities.GetTaggedChildElement(moqTypeTableContainer, BOEExporterConstants.FieldName_MOQTypeTableNote));
                    }
                }

                // delete template
                this.RemoveElement(moqTypeTableTemplate);
            }
        }

        /// <summary>
        /// Removes the row with the MOQ Type table
        /// </summary>
        /// <param name="moqTypeContainer">MOQ Type Container</param>
        private void RemoveMoqTableRow(SdtElement moqTypeContainer)
        {
            WordUtilities.RemoveTableRowWithTaggedElement(moqTypeContainer, BOEExporterConstants.Table_MOQType);
        }

        /// <summary>
        /// Remove the rows with CER/PR/AR elements
        /// </summary>
        /// <param name="moqTypeContainer">MOQ Type Container</param>
        private void RemoveCerPrArRows(SdtElement moqTypeContainer)
        {
            WordUtilities.RemoveTableRowWithTaggedElement(moqTypeContainer, BOEExporterConstants.FieldName_CerPmArName);
        }

        /// <summary>
        /// Remove the row with SOE/LOW Elements
        /// </summary>
        /// <param name="moqTypeContainer">MOQ Type Container</param>
        private void RemoveSoeLowRows(SdtElement moqTypeContainer)
        {
            WordUtilities.RemoveTableRowWithTaggedElement(moqTypeContainer, BOEExporterConstants.FieldName_HoursDescription);
        }

        /// <summary>
        /// Remove the rows with SME Elements
        /// </summary>
        /// <param name="moqTypeContainer">MOQ Type Container</param>
        private void RemoveSMERows(SdtElement moqTypeContainer)
        {
            WordUtilities.RemoveTableRowWithTaggedElement(moqTypeContainer, BOEExporterConstants.FieldName_SMEReasons);
            WordUtilities.RemoveTableRowWithTaggedElement(moqTypeContainer, BOEExporterConstants.FieldName_SMETasksBelowLabel);
            WordUtilities.RemoveTableRowWithTaggedElement(moqTypeContainer, BOEExporterConstants.FieldName_SMEHoursLogic);
            WordUtilities.RemoveTableRowWithTaggedElement(moqTypeContainer, BOEExporterConstants.FieldName_SMEDurationLogic);
            WordUtilities.RemoveTableRowWithTaggedElement(moqTypeContainer, BOEExporterConstants.FieldName_SMETasks);
        }
    }
}
