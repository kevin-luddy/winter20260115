// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Backend.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Transactions;
	using GenTRAC.DataBridge.Core.Common.Security;
	using GenTRAC.DataBridge.Core.DTO.Proposal;
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Logging;
	using RDSB.Backend.Common;

	/// <summary>
	/// Controller for Document Section
	/// </summary>
	[Route("api/Document")]
	public class DocumentController : RDSBController
    {
        /// <summary>
        /// The document controller logic.
        /// </summary>
        private readonly IDocumentControllerLogic documentControllerLogic;

        /// <summary>
        /// The rate detail loader
        /// </summary>
        private readonly IRateDetailLoader rateDetailLoader;

		/// <summary>
		/// Web Host Environment
		/// </summary>
		private readonly IWebHostEnvironment webHostEnvironment;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="securityInformation">The security information.</param>
		/// <param name="securityMapper">The security mapper.</param>
		/// <param name="documentControllerLogic">The document controller logic.</param>
		/// <param name="adUtils">Active Directory Utilities</param>
		/// <param name="whosOnlineLoader">Who's Online Loader</param>
		/// <param name="rateDetailLoader">Rate Loader</param>
		public DocumentController(ISecurityInformation securityInformation, ISecurityMapper securityMapper, 
			IDocumentControllerLogic documentControllerLogic, IActiveDirectoryService adUtils, 
			IWhosOnlineLoader whosOnlineLoader, IRateDetailLoader rateDetailLoader, IWebHostEnvironment webHostEnvironment,
			ILogger<DocumentController> logger, IHttpContextAccessor httpContextAccessor)
            : base(securityInformation, securityMapper, adUtils, whosOnlineLoader, logger, httpContextAccessor)
        {
            this.documentControllerLogic = documentControllerLogic;
            this.rateDetailLoader = rateDetailLoader;
			this.webHostEnvironment = webHostEnvironment;
        }

		/// <summary>
		/// Index for Document Section
		/// </summary>
		/// <returns>Documents for Homepage Grid</returns>
		[HttpGet("[action]")]
		public ICollection<DocumentGridModelView> GetDocuments()
        {
            ICollection<DocumentGridModelView> models = this.documentControllerLogic.RetrieveAllLinkedDocuments(this.SecurityMapper.GetRolesForLoggedInUser(), this.securityInformation.ActiveUserNTID);

            return models;
        }

		/// <summary>
		/// Get the proposals for the Add New Document dropdown 
		/// </summary>
		/// <returns>proposals for the Add New Document dropdown </returns>
		[HttpPost("[action]")]
		public ICollection<DocumentGridModelView> GetProposalsForNewDocument()
        {
            ICollection<ProposalDto> proposals = this.documentControllerLogic.RetrieveUnlinkedProposals(this.SecurityMapper.GetRolesForLoggedInUser(),
                    this.securityInformation.ActiveUserNTID).OrderByDescending(p => p.TrackingNumber).ToList();

            ICollection<DocumentGridModelView> toReturn = new Collection<DocumentGridModelView>();
            foreach (ProposalDto proposal in proposals)
            {
                toReturn.Add(new DocumentGridModelView() { ProposalId = proposal.Id, ProposalTitle = proposal.ProposalTitle, ProposalTrackingNumber = proposal.TrackingNumber});
            }

            return toReturn;
        }

		/// <summary>
		/// Saves new document for the selected Proposal
		/// </summary>
		/// <param name="id">Selected proposal ID</param>
		/// <returns>Json</returns>
		[HttpPost("[action]")]
		public bool SaveNewDocument(int id)
        {
            this.documentControllerLogic.SaveNewDocument(id);
            return true;
        }

		/// <summary>
		/// Saves the specified collection.
		/// POST: api/Document/Save
		/// </summary>
		/// <param name="document">The document to save.</param>
		/// <param name="id">The proposal Id, needed for authorization.</param>
		/// <returns>boolean result of the save</returns>
		[HttpPost("[action]")]
		public bool Save(DocumentDetailModelView document, int id)
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

            using (TransactionScope scope = new(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.Snapshot,
                    Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", CommonConstants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT))
                }))
            {
                this.documentControllerLogic.SaveDocument(document);
                
                scope.Complete();
            }

			return true;
        }

		/// <summary>
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="createIfNotExists">If true, create the RDSB document if it doesn't already exist. If false, only edit existing RDSB document.</param>
		/// <returns>Detailed Model View</returns>
		[HttpGet("[action]")]
		public DocumentDetailModelView GetDetailModelView(int id, bool? createIfNotExists = false)
        {
            DocumentDetailModelView model = this.documentControllerLogic.RetrieveDocumentDetailByProposalId(id, createIfNotExists);

            return model;
        }

		/// <summary>
		/// View for publishing a Document for the specified identifier.
		/// GET: api/Document/Publish/5
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="portionMarkingRequired">Is Portion Marking Required</param>
		/// <returns>Export document.</returns>
		[HttpGet("[action]")]
		public IActionResult Publish(int id, bool portionMarkingRequired)
        {
			IActionResult result = new EmptyResult();
            try
            {
				string serverFileName = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/Templates/Export/PPRDTemplate.docx");
				this.documentControllerLogic.GenerateRDD(id, serverFileName, portionMarkingRequired);
            }
            catch (GeneralAppException e)
            {
                this.log.LogError(e, "Error publishing/generating a PPRD document");
                result = this.CreateTextFileWithErrorMessage(e.Message);
            }

            return result;
        }

		/// <summary>
		/// Deletes the specified Document.
		/// POST: api/Document/Delete/5 
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns>Json Result of the deletion.</returns>
		[HttpPost("[action]")]
		public bool Delete(int id)
        {
            this.documentControllerLogic.DeleteDocument(id);
            return true;
        }

		/// <summary>
		/// Gets the Rate Codes for the selected Revision for the dropdown
		/// </summary>
		/// <param name="revisionID">Revision ID</param>
		/// <returns>Rate Codes for the selected Revision</returns>
		[HttpGet("[action]")]
		public ICollection<RdsbRateDetailModelView> GetRateCodesForRevision(int revisionID)
        {
            ICollection<RdsbRateDetailModelView> rates = this.rateDetailLoader.GetRatesForRdsbDocument(revisionID);
            
            return rates;
        }

		/// <summary>
		/// Gets the Sections for the selected Revision for the dropdown
		/// </summary>
		/// <param name="revisionID">Revision ID</param>
		/// <returns>Sections for the selected Revision</returns>
		[HttpGet("[action]")]
		public ICollection<SectionDetailModelView> GetSectionsForRevision(int revisionID)
        {
            ICollection<SectionDetailModelView> sections = this.documentControllerLogic.GetSectionsForRevision(revisionID);
            
            return sections;
        }
    }
}
