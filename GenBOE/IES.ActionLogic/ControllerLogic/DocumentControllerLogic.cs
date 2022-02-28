// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using System.Web;
    using DataBridge.Loaders;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.ModelViews;
    using IO.Export;

    /// <summary>
    /// Logic for the Document Controller.
    /// </summary>
    public class DocumentControllerLogic : IDocumentControllerLogic
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly Logger logger = new Logger(typeof(DocumentControllerLogic));

        /// <summary>
        /// The proposal loader
        /// </summary>
        private readonly IProposalLoader proposalLoader;

        /// <summary>
        /// The document loader
        /// </summary>
        private readonly IDocumentLoader documentLoader;

        /// <summary>
        /// The document detail loader
        /// </summary>
        private readonly IDocumentDetailLoader documentDetailLoader;

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        private readonly IActiveDirectoryUtilities adUtils;

        /// <summary>
        /// Security Information
        /// </summary>
        private readonly ISecurityInformation securityInformation;

        /// <summary>
        /// Revision Loader
        /// </summary>
        private readonly IRevisionLoader revisionLoader;

        /// <summary>
        /// The section loader
        /// </summary>
        private readonly ISectionLoader sectionLoader;

        /// <summary>
        /// The rate detail loader
        /// </summary>
        private readonly IRateDetailLoader rateDetailLoader;

        /// <summary>
        /// The file attachment loader
        /// </summary>
        private readonly IFileAttachmentLoader fileAttachmentLoader;

        /// <summary>
        /// PPRD Exporter
        /// </summary>
        private readonly IPPRDExporter pprdExporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentControllerLogic" /> class.
        /// </summary>
        /// <param name="proposalLoader">The proposal loader.</param>
        /// <param name="documentLoader">The Document loader.</param>
        /// <param name="documentDetailLoader">The Document Detail loader</param>
        /// <param name="adUtils">AD Utils</param>
        /// <param name="securityInfo">Security Info</param>
        /// <param name="revisionLoader">Revision Loader</param>
        /// <param name="sectionLoader">The section loader.</param>
        /// <param name="rateDetailLoader">The rate detail loader.</param>
        /// <param name="fileAttachmentLoader">The file attachment loader.</param>
        /// <param name="pprdExporter">The PPRD exporter.</param>
        public DocumentControllerLogic(IProposalLoader proposalLoader, IDocumentLoader documentLoader, IDocumentDetailLoader documentDetailLoader, IActiveDirectoryUtilities adUtils, ISecurityInformation securityInfo, IRevisionLoader revisionLoader,
            ISectionLoader sectionLoader, IRateDetailLoader rateDetailLoader, IFileAttachmentLoader fileAttachmentLoader, IPPRDExporter pprdExporter)
        {
            this.proposalLoader = proposalLoader;
            this.documentLoader = documentLoader;
            this.documentDetailLoader = documentDetailLoader;
            this.adUtils = adUtils;
            this.securityInformation = securityInfo;
            this.revisionLoader = revisionLoader;
            this.sectionLoader = sectionLoader;
            this.rateDetailLoader = rateDetailLoader;
            this.fileAttachmentLoader = fileAttachmentLoader;
            this.pprdExporter = pprdExporter;
        }

        /// <summary>
        /// Retrieves all linked documents.
        /// </summary>
        /// <param name="roles">Roles for the active user.</param>
        /// <param name="activeUserNtid">The active user's ntid</param>
        /// <returns>
        /// Collection of model view objects
        /// </returns>
        public ICollection<DocumentGridModelView> RetrieveAllLinkedDocuments(IReadOnlyCollection<SecurityPermissionsResponse> roles, string activeUserNtid)
        {
            bool isAdmin = roles.Any(r => r.AuthorizedRole == PtmRole.Admin);

            ICollection<ProposalDto> proposals = isAdmin ? this.proposalLoader.GetAllSlim() : this.proposalLoader.GetProposalsByUser(activeUserNtid);

            proposals = proposals.Where(p => p.DocumentId.HasValue && p.CustomerType != CustomerType.Commercial && p.CustomerType != CustomerType.InternationalCommercial).ToList();

            ICollection<DocumentGridModelView> models = new List<DocumentGridModelView>();
            if (proposals.Any())
            {
                models = this.RetrieveDocuments(proposals, isAdmin);
            }

            models = models.OrderByDescending(x => x.ProposalTrackingNumber).ToList();

            return models;
        }

        /// <summary>
        /// Gets the unlinked proposals the user has access to edit that are in progress or submitted.
        /// </summary>
        /// <param name="roles">Roles for the active user.</param>
        /// <param name="activeUserNtid">The active user's ntid</param>
        /// <returns>A collection of unlinked (to RDSB) proposals.</returns>
        public ICollection<ProposalDto> RetrieveUnlinkedProposals(IReadOnlyCollection<SecurityPermissionsResponse> roles, string activeUserNtid)
        {
            bool isAdmin = roles.Any(r => r.AuthorizedRole == PtmRole.Admin);

            ICollection<ProposalDto> proposals = isAdmin ? this.proposalLoader.GetAllSlim() : this.proposalLoader.GetProposalsByUser(activeUserNtid);
            proposals = proposals.Where(p => 
                    (p.ProposalStatus == ProposalStatus.InProgress || p.ProposalStatus == ProposalStatus.PendingCertification) 
                    && !p.DocumentId.HasValue
                    && !p.IsForecastProposal && p.ProposalStatus != ProposalStatus.Revised
                    && p.CustomerType != CustomerType.Commercial && p.CustomerType != CustomerType.InternationalCommercial 
                    && (isAdmin || roles.Any(r => r.ProposalID == p.Id && Constants.EDIT_ROLES.Contains(r.AuthorizedRole)))).ToList();

            // do a sanity check to make sure there are no documents that think they are linked to proposals
            if (proposals.Any())
            {
                ICollection<DocumentGridModelView> models = this.RetrieveDocuments(proposals, isAdmin);
                if (models.Any())
                {
                    // ok, we have some discrepancies....remove them from the proposals being returned, and update the proposals
                    foreach (DocumentGridModelView model in models)
                    {
                        ProposalDto proposal = proposals.FirstOrDefault(p => p.Id == model.ProposalId);
                        if (proposal != null)
                        {
                            proposals.Remove(proposal);

                            // retrieve the original proposal with all properties set
                            proposal = this.proposalLoader.GetById(proposal.Id);
                            proposal.DocumentId = model.Id;
                            proposal.Updateable = UpdateType.Upsert;
                            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                            {
                                this.proposalLoader.Save(proposal);
                                scope.Complete();
                            }
                        }
                    }
                }
            }

            return proposals;
        }

        /// <summary>
        /// Save a document
        /// </summary>
        /// <param name="document">Document to save</param>
        public void SaveDocument(DocumentDetailModelView document)
        {
            if(document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.Updateable = UpdateType.Upsert;
            this.documentDetailLoader.Save(document);
        }

        /// <summary>
        /// Deletes the document.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        public void DeleteDocument(int proposalId)
        {
            ProposalDto proposal;
            DocumentGridModelView model = this.RetrieveDocumentByProposalId(proposalId, out proposal);
            model.Updateable = UpdateType.Deleted;

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                {
                    this.documentLoader.Save(new DocumentGridModelView[] { model });
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, string.Format("Could not delete document for proposal with ID {0}.", proposalId));
                throw;
            }

            proposal.Updateable = UpdateType.Upsert;
            proposal.DocumentId = null;

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                {
                    // This has to be done inside another transaction because it is a different Database
                    this.proposalLoader.Save(proposal);
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, string.Format("Could not update proposal with ID {0} to remove the document Id", proposalId));
                throw;
            }
        }

        /// <summary>
        /// Saves the new document.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        public void SaveNewDocument(int proposalId)
        {
            if (proposalId <= 0)
            {
                throw new GenValidationException("There was no proposal Id passed back.");
            }

            ProposalDto proposal = this.proposalLoader.GetById(proposalId);

            if (proposal == null)
            {
                throw new GenValidationException("The proposal Id passed in is invalid: " + proposalId.ToString());
            }

            DocumentGridModelView model = this.RetrieveDocuments(new ProposalDto[] { proposal }).FirstOrDefault();

            if (model != null)
            {
                throw new GenValidationException("There already exists a document for ProposalId: " + proposalId.ToString());
            }

            // RDM Revision will be set to latest published by default
            model = new DocumentGridModelView
            {
                Id = -1,
                Updateable = UpdateType.Upsert,
                ProposalId = proposalId,
                DocumentCreatedBy = this.adUtils.GetUserByQualifiedAccount(this.securityInformation.ActiveUserNTID, false).DisplayName,
                StartYear = DateTime.Now.Year,
                EndYear = DateTime.Now.Year + 5
            };

            int documentId;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                documentId = this.documentLoader.Save(new DocumentGridModelView[] { model }).First().Value;
                scope.Complete();
            }

            proposal.Updateable = UpdateType.Upsert;
            proposal.DocumentId = documentId;

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                {
                    // This has to be done inside another transaction because it is a different Database
                    this.proposalLoader.Save(proposal);
                    scope.Complete();
                }
            }
            catch (Exception)
            {
                this.logger.Error(string.Format("Could not update proposal with ID {0} to add the document ID {1}", proposalId, documentId));
                throw;
            }
        }

        /// <summary>
        /// Retrieves the document for the proposal id passed in.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <returns>The document associated with the proposal, null if not found.</returns>
        public DocumentGridModelView RetrieveDocumentByProposalId(int proposalId)
        {
            ProposalDto proposal;
            return this.RetrieveDocumentByProposalId(proposalId, out proposal);
        }

        /// <summary>
        /// Retrieves the document for the proposal id passed in.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <param name="proposal">The proposal for the id passed in.</param>
        /// <returns>The document associated with the proposal, null if not found.</returns>
        private DocumentGridModelView RetrieveDocumentByProposalId(int proposalId, out ProposalDto proposal)
        {
            if (proposalId <= 0)
            {
                throw new GenValidationException("There was no proposal Id passed back.");
            }

            proposal = this.proposalLoader.GetById(proposalId);

            if (proposal == null)
            {
                throw new GenValidationException("The proposal Id passed in is invalid: " + proposalId.ToString());
            }

            DocumentGridModelView modelView = this.RetrieveDocuments(new ProposalDto[] { proposal }).FirstOrDefault();

            if (modelView == null)
            {
                if (proposal.DocumentId.HasValue)
                {
                    this.logger.Error("The proposal Id passed in is linked to a document but the document is not in the DB for id " + proposalId.ToString());
                    throw new GenValidationException("The document could not be found.");
                }

                throw new GenValidationException("The proposal Id passed in is not linked to a document");
            }

            return modelView;
        }

        /// <summary>
        /// Retrieves the documents for the proposals passed in.
        /// </summary>
        /// <param name="proposals">The proposals.</param>
        /// <param name="isAdmin">Is User Admin</param>
        /// <returns>The documents associated with the proposals</returns>
        private ICollection<DocumentGridModelView> RetrieveDocuments(ICollection<ProposalDto> proposals, bool isAdmin = false)
        {
            if (proposals == null)
            {
                throw new ArgumentNullException(nameof(proposals));
            }

            RevisionModelView latestRevision = this.revisionLoader.GetAll().Where(r => r.DatePublished.HasValue).OrderByDescending(r => r.DatePublished).First();

            ICollection<DocumentGridModelView> models = this.documentLoader.GetByProposalIds(proposals.Select(p => p.Id).ToList(), latestRevision.Id);

            foreach (DocumentGridModelView model in models)
            {
                ProposalDto proposal = proposals.FirstOrDefault(p => p.Id == model.ProposalId);
                if (proposal == null)
                {
                    this.logger.Error("Did not find a Proposal for Document retrieved with Id: " + model.Id);
                }
                else
                {
                    model.ProposalStatus = proposal.ProposalStatus.GetDescription();
                    model.ProposalTrackingNumber = proposal.TrackingNumber;
                    model.ProposalTitle = proposal.ProposalTitle;
                    model.IsReadOnly = !isAdmin && !proposal.HasWriteAccessToLinkedDocument;
                }
            }

            if (models.Count != proposals.Count)
            {
                // we are missing linkages somewhere
                foreach (ProposalDto proposal in proposals)
                {
                    if (!models.Any(m => m.ProposalId == proposal.Id))
                    {
                        this.logger.Error("Did not find a linked document for Proposal with Id: " + proposal.Id);
                    }
                }
            }

            return models;
        }

        /// <summary>
        /// Retrieves the document detail for the proposal id passed in.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <param name="createIfNotExists">If true, create the RDSB document if it doesn't already exist. If false, return existing RDSB document if found.</param>
        /// <returns>The document details associated with the proposal, null if not found.</returns>
        public DocumentDetailModelView RetrieveDocumentDetailByProposalId(int proposalId, bool? createIfNotExists = false)
        {
            if (proposalId <= 0)
            {
                throw new GenValidationException("There was no proposal Id passed back.");
            }

            ProposalDto proposal = this.proposalLoader.GetById(proposalId);

            if (proposal == null)
            {
                throw new GenValidationException("The proposal Id passed in is invalid: " + proposalId.ToString());
            }

            DocumentDetailModelView modelView = this.documentDetailLoader.GetByProposalId(proposalId);

            if (modelView == null || modelView.Id <= 0)
            {
                if (proposal.DocumentId.HasValue)
                {
                    this.logger.Error("The proposal Id passed in is linked to a document but the document is not in the DB for id " + proposalId.ToString());
                    throw new GenValidationException("The document could not be found.");
                }

                if (createIfNotExists == true)
                {
                    this.SaveNewDocument(proposalId);
                    modelView = this.documentDetailLoader.GetByProposalId(proposalId);
                }

                if (modelView == null || modelView.Id <= 0)
                {
                    throw new GenValidationException("The proposal Id passed in is not linked to a document");
                }
            }

            // set remaining proposal info
            modelView.ProposalTitle = proposal.ProposalTitle;
            modelView.TrackingNumber = proposal.TrackingNumber;
            modelView.ProposalStatus = proposal.ProposalStatus.ToDescription();

            // Get available revisions
            Collection<RevisionModelView> revisions = this.revisionLoader.GetAll().Where(x => x.DatePublished.HasValue).OrderByDescending(x => x.DatePublished).ToCollection();
            RevisionModelView firstRevision = revisions.First();
            foreach (RevisionModelView revision in revisions)
            {
                if (revision.Id == firstRevision.Id || revision.Id == modelView.SelectedRevisionId)
                {
                    modelView.AvailableRevisions.Add(revision);
                }
            }

            // set IsUsingLatest
            modelView.IsUsingLatest = firstRevision.Id == modelView.SelectedRevisionId;

            return modelView;
        }

        /// <summary>
        /// Validates the document detail model view.
        /// </summary>
        /// <param name="document">The document to validate.</param>
        /// <returns>A list of validation messages.</returns>
        public ICollection<ValidationMessage> ValidateDocumentDetailModelView(DocumentDetailModelView document)
        {
            ICollection<ValidationMessage> messages = new List<ValidationMessage>();

            RevisionModelView revision = null;
            if (document.SelectedRevisionId.HasValue)
            {
                revision = this.revisionLoader.GetAll().FirstOrDefault(r => r.DatePublished.HasValue && r.Id == document.SelectedRevisionId);
            }

            if (revision == null)
            {
                messages.Add(new ValidationMessage("SelectedRevisionId", "Revision is required."));
            }

            if (document.StartYear <= 0)
            {
                messages.Add(new ValidationMessage("StartYear", "Start Year is required."));
            }

            if (document.EndYear <= 0)
            {
                messages.Add(new ValidationMessage("EndYear", "End Year is required."));
            }

            if (document.StartYear > document.EndYear)
            {
                messages.Add(new ValidationMessage("StartYear", "Start Year must be less than or equal to End Year."));
            }

            // if we made it here with no messages, then check for start/end years inside revision
            if (messages.None())
            {
                if (document.StartYear < revision.StartYear || document.StartYear > revision.EndYear)
                {
                    messages.Add(new ValidationMessage("StartYear", string.Format("Start Year must be within the Revision's specified Years {0} - {1}.", revision.StartYear, revision.EndYear)));
                }

                if (document.EndYear < revision.StartYear || document.EndYear > revision.EndYear)
                {
                    messages.Add(new ValidationMessage("EndYear", string.Format("End Year must be within the Revision's specified Years {0} - {1}.", revision.StartYear, revision.EndYear)));
                }
            }

            // check rate codes not blank
            if (document.SelectedRateCodeIds == null || document.SelectedRateCodeIds.None())
            {
                messages.Add(new ValidationMessage("SelectedRateCodeIds", "At least one Rate Code must be selected."));
            }
            else if (revision != null)
            {
                ICollection<RdsbRateDetailModelView> rates = this.rateDetailLoader.GetRatesForRdsbDocument(document.SelectedRevisionId.Value);
                if (document.SelectedRateCodeIds.Any(r => !rates.Any(rr => rr.Id == r)))
                {
                    messages.Add(new ValidationMessage("SelectedRateCodeIds", "At least one Rate Code could not be found."));
                }
            }

            // check sections not blank
            if (document.SelectedSectionIds == null || document.SelectedSectionIds.None())
            {
                messages.Add(new ValidationMessage("SelectedSectionIds", "At least one Section must be selected."));
            }
            else if (revision != null)
            {
                ICollection<SectionModelView> sections = this.sectionLoader.RetrieveAllSections(new RevisionModelView() { Id = document.SelectedRevisionId.Value }, true);
                ICollection<int> allSectionIds = this.GetSectionIds(sections, false);
                if (document.SelectedSectionIds.Any(s => !allSectionIds.Contains(s)))
                {
                    messages.Add(new ValidationMessage("SelectedSectionIds", "At least one Section could not be found."));
                }
                
                ICollection<int> requiredSectionIds = this.GetSectionIds(sections, true);
                if (requiredSectionIds.Any(s => !document.SelectedSectionIds.Contains(s)))
                {
                    messages.Add(new ValidationMessage("SelectedSectionIds", "At least one required Section was not selected."));
                }
            }

            return messages;
        }

        /// <summary>
        /// Gets the Sections for the selected Revision for the dropdown
        /// </summary>
        /// <param name="revisionId">Revision ID</param>
        /// <returns>Sections for the selected Revision</returns>
        public ICollection<SectionDetailModelView> GetSectionsForRevision(int revisionId)
        {
            ICollection<SectionModelView> sections =
                this.sectionLoader.RetrieveAllSections(new RevisionModelView() { Id = revisionId });

            // convert into section detail model view stripping out internal sections and non-section content
            ICollection<SectionDetailModelView> details = this.ConvertSections(sections);

            return details;
        }

        /// <summary>
        /// Generates the RDD document for the Proposal Id passed in.
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="serverFileName">Server File Name</param>
        /// <param name="httpResponse">HTTP response object</param>
        public void GenerateRDD(int proposalId, string serverFileName, HttpResponseBase httpResponse)
        {
            if (serverFileName == null)
            {
                throw new ArgumentNullException(nameof(serverFileName));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            // get the document based off id
            DocumentDetailModelView modelView = this.RetrieveDocumentDetailByProposalId(proposalId);

            if (modelView == null)
            {
                throw new ArgumentException("There is no Document assigned to this proposal Id: " + proposalId.ToString());
            }

            if (!modelView.SelectedRevisionId.HasValue)
            {
                throw new ArgumentException("The selected Document does not have a Revision chosen.");
            }

            // Get Revision MV
            RevisionModelView revisionMV = this.revisionLoader.GetAll().FirstOrDefault(r => r.Id == modelView.SelectedRevisionId.Value);

            if (revisionMV == null)
            {
                throw new ArgumentException("Revision ID selected for this Document is invalid.");
            }

            string clientFileName = string.Format("{0}_{1}_{2}-{3}.docx", modelView.TrackingNumber, modelView.ProposalTitle, modelView.StartYear, modelView.EndYear).Replace(",", "_");

            // Get SectionsMVs 
            string refNumberPrefix = string.IsNullOrWhiteSpace(modelView.ParentSection) ? string.Empty : modelView.ParentSection + ".";
            ICollection<SectionModelView> sections = this.sectionLoader.GetAll(revisionMV, false, modelView.SelectedSectionIds, refNumberPrefix);

            // Get Rates
            ICollection<RateDetailModelView> rates = this.rateDetailLoader.GetRatesByRevision(revisionMV);

            // Remove Labor Rates not selected
            rates = rates.Where(r => r.RateCategory != RateCategory.DirectLabor || modelView.SelectedRateCodeIds.Contains(r.Id)).ToList();

            // Get File Attachments
            ICollection<FileAttachmentRowModelView> fileAttachments = this.fileAttachmentLoader.GetByRevision(revisionMV.Id);

            this.pprdExporter.ExportRDDToWordFile(sections, rates, fileAttachments, serverFileName, clientFileName, revisionMV, modelView, httpResponse);
        }

        /// <summary>
        /// Converts the sections.
        /// </summary>
        /// <param name="sections">The sections.</param>
        /// <returns>A treeview of the section details.</returns>
        private ICollection<SectionDetailModelView> ConvertSections(ICollection<SectionModelView> sections)
        {
            ICollection<SectionDetailModelView> details = new List<SectionDetailModelView>();
            foreach (SectionModelView section in sections)
            {
                if ((!section.IsInternalSection.HasValue || !section.IsInternalSection.Value) && section.ContentType == SectionContentType.Section)
                {
                    SectionDetailModelView detail = new SectionDetailModelView
                    {
                        Id = section.Id,
                        Title = section.Title,
                        ReferenceNumber = section.ReferenceNumber,
                        HasTable = section.ChildNodes.Any(s => (!s.IsInternalSection.HasValue || !s.IsInternalSection.Value) && s.ContentType == SectionContentType.RateTable),
                        IsRdsbRequired = section.IsRdsbRequired
                    };

                    details.Add(detail);
                    if (section.ChildNodes != null && section.ChildNodes.Any())
                    {
                        // recursively call the children and set them on converted details
                        detail.ChildNodes = this.ConvertSections(section.ChildNodes);
                    }
                }
            }

            return details;
        }

        /// <summary>
        /// Gets the section ids.
        /// </summary>
        /// <param name="sections">The sections.</param>
        /// <param name="onlyRequiredSections">Whether to get only required section ids (true) or all section ids (false)</param>
        /// <returns>All of the ids for the sections and their children.</returns>
        private ICollection<int> GetSectionIds(ICollection<SectionModelView> sections, bool onlyRequiredSections)
        {
            ICollection<int> ids = new List<int>();
            foreach (SectionModelView section in sections)
            {
                // If only getting required sections, make sure they're also not internal just in case
                if (!onlyRequiredSections || (section.IsRdsbRequired && (!section.IsInternalSection ?? true)))
                {
                    ids.Add(section.Id);
                }

                if (section.ChildNodes != null && section.ChildNodes.Any())
                {
                    ids.AddRange(this.GetSectionIds(section.ChildNodes, onlyRequiredSections));
                }
            }

            return ids;
        }
    }
}
