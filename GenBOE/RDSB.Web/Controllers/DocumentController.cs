// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using System.Web.Mvc;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using IES.ActionLogic.ControllerLogic;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using RDSB.Web.Common;

    /// <summary>
    /// Controller for Document Section
    /// </summary>
    public class DocumentController : RDSBController
    {
        /// <summary>
        ///  The logger
        /// </summary>
        private Logger logger = new Logger(typeof(DocumentController));

        /// <summary>
        /// The document controller logic.
        /// </summary>
        private IDocumentControllerLogic documentControllerLogic;

        /// <summary>
        /// The rate detail loader
        /// </summary>
        private IRateDetailLoader rateDetailLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="securityInformation">The security information.</param>
        /// <param name="securityMapper">The security mapper.</param>
        /// <param name="documentControllerLogic">The document controller logic.</param>
        /// <param name="adUtils">Active Directory Utilities</param>
        /// <param name="whosOnlineLoader">Who's Online Loader</param>
        /// <param name="rateDetailLoader">Rate Loader</param>
        public DocumentController(ISecurityInformation securityInformation, ISecurityMapper securityMapper, IDocumentControllerLogic documentControllerLogic, IActiveDirectoryUtilities adUtils, IWhosOnlineLoader whosOnlineLoader, IRateDetailLoader rateDetailLoader)
            : base(securityInformation, securityMapper, adUtils, whosOnlineLoader)
        {
            this.documentControllerLogic = documentControllerLogic;
            this.rateDetailLoader = rateDetailLoader;
        }

        /// <summary>
        /// Index for Document Section
        /// </summary>
        /// <returns>Index page for Document.</returns>
        public ActionResult Index()
        {
            ICollection<DocumentGridModelView> models = this.documentControllerLogic.RetrieveAllLinkedDocuments(this.SecurityMapper.GetRolesForLoggedInUser(), this.SecurityInformation.ActiveUserNTID);

            return this.View(models);
        }

        /// <summary>
        /// Get the proposals for the Add New Document dropdown 
        /// </summary>
        /// <returns>proposals for the Add New Document dropdown </returns>
        [HttpPost]
        public ActionResult GetProposalsForNewDocument()
        {
            ICollection<ProposalDto> proposals = this.documentControllerLogic.RetrieveUnlinkedProposals(this.SecurityMapper.GetRolesForLoggedInUser(),
                    this.SecurityInformation.ActiveUserNTID).OrderByDescending(p => p.TrackingNumber).ToList();

            ICollection<DocumentGridModelView> toReturn = new Collection<DocumentGridModelView>();
            foreach (ProposalDto proposal in proposals)
            {
                toReturn.Add(new DocumentGridModelView() { ProposalId = proposal.Id, ProposalTitle = proposal.ProposalTitle, ProposalTrackingNumber = proposal.TrackingNumber});
            }

            return this.Json(toReturn);
        }

        /// <summary>
        /// Saves new document for the selected Proposal
        /// </summary>
        /// <param name="id">Selected proposal ID</param>
        /// <returns>Json</returns>
        [HttpPost]
        public ActionResult SaveNewDocument(int id)
        {
            this.documentControllerLogic.SaveNewDocument(id);
            return this.Json(new { status = true });
        }

        /// <summary>
        /// Saves the specified collection.
        /// POST: Document/Save
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <param name="id">The proposal Id, needed for authorization.</param>
        /// <returns>Json Result of the save.</returns>
        [HttpPost]
        public ActionResult Save([ModelBinder(typeof(JsonNetModelBinder))] DocumentDetailModelView document, int id)
        {
            if (document == null)
            {
                throw new GenValidationException("Document cannot be null.");
            }

            if (id != document.ProposalId)
            {
                // id is needed as a param for authorization
                throw new GenValidationException("The Proposal Id is invalid.");
            }

            ICollection<ValidationMessage> validationErrors = this.documentControllerLogic.ValidateDocumentDetailModelView(document);
            if (validationErrors.Any())
            {
                throw new GenValidationException(validationErrors);
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.Snapshot,
                    Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT))
                }))
            {
                this.documentControllerLogic.SaveDocument(document);
                
                scope.Complete();
            }

            return this.Json(new { Status = true });
        }

        /// <summary>
        /// View for Editing a Document for the specified identifier.
        /// GET: Document/Edit/5
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="createIfNotExists">If true, create the RDSB document if it doesn't already exist. If false, only edit existing RDSB document.</param>
        /// <returns>Edit View for a specific identifier.</returns>
        public ActionResult Edit(int id, bool? createIfNotExists = false)
        {
            DocumentDetailModelView model = this.documentControllerLogic.RetrieveDocumentDetailByProposalId(id, createIfNotExists);

            return this.View(model);
        }

        /// <summary>
        /// View for publishing a Document for the specified identifier.
        /// GET: Document/Publish/5
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Export document.</returns>
        public ActionResult Publish(int id)
        {
            ActionResult result = new EmptyResult();
            try
            {
                string serverFileName = this.Server.MapPath("~/Templates/Export/PPRDTemplate.docx");

                this.documentControllerLogic.GenerateRDD(id, serverFileName, this.Response);
            }
            catch (GeneralAppException e)
            {
                this.logger.Error(e);
                result = this.CreateTextFileWithErrorMessage(e.Message);
            }

            return result;
        }

        /// <summary>
        /// Deletes the specified Document.
        /// POST: Document/Delete/5 
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Json Result of the deletion.</returns>
        [HttpPost]
        public JsonResult Delete(int id)
        {
            this.documentControllerLogic.DeleteDocument(id);
            return this.Json(new { Status = true });
        }

        /// <summary>
        /// Gets the Rate Codes for the selected Revision for the dropdown
        /// </summary>
        /// <param name="revisionID">Revision ID</param>
        /// <returns>Rate Codes for the selected Revision</returns>
        [HttpPost]
        public JsonResult GetRateCodesForRevision(int revisionID)
        {
            ICollection<RdsbRateDetailModelView> rates = this.rateDetailLoader.GetRatesForRdsbDocument(revisionID);
            
            return this.Json(rates);
        }

        /// <summary>
        /// Gets the Sections for the selected Revision for the dropdown
        /// </summary>
        /// <param name="revisionID">Revision ID</param>
        /// <returns>Sections for the selected Revision</returns>
        [HttpPost]
        public JsonResult GetSectionsForRevision(int revisionID)
        {
            ICollection<SectionDetailModelView> sections = this.documentControllerLogic.GetSectionsForRevision(revisionID);
            
            return this.Json(sections);
        }
    }
}
