// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Web;
	using DataBridge.ModelViews;
	using GenTRAC.DataBridge.Common.Security;
	using GenTRAC.DataBridge.DTO;
	using IES.Common.Exceptions;

	/// <summary>
	/// Interface for the Document Controller Logic.
	/// </summary>
	public interface IDocumentControllerLogic
    {
        /// <summary>
        /// Gets the unlinked proposals the user has access to edit that are in progress.
        /// </summary>
        /// <param name="roles">Roles for the active user.</param>
        /// <param name="activeUserNtid">The active user's ntid</param>
        /// <returns>A collection of unlinked (to RDSB) proposals.</returns>
        ICollection<ProposalDto> RetrieveUnlinkedProposals(IReadOnlyCollection<SecurityPermissionsResponse> roles, string activeUserNtid);

        /// <summary>
        /// Retrieves all linked documents.
        /// </summary>
        /// <param name="roles">Roles for the active user.</param>
        /// <param name="activeUserNtid">The active user's ntid</param>
        /// <returns>
        /// Collection of model view objects
        /// </returns>
        ICollection<DocumentGridModelView> RetrieveAllLinkedDocuments(IReadOnlyCollection<SecurityPermissionsResponse> roles, string activeUserNtid);

        /// <summary>
        /// Retrieves the document for the proposal id passed in.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <returns>The document associated with the proposal, null if not found.</returns>
        DocumentGridModelView RetrieveDocumentByProposalId(int proposalId);

        /// <summary>
        /// Retrieves the document detail for the proposal id passed in.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <param name="createIfNotExists">If true, create the RDSB document if it doesn't already exist. If false, return existing RDSB document if found.</param>
        /// <returns>The document details associated with the proposal, null if not found.</returns>
        DocumentDetailModelView RetrieveDocumentDetailByProposalId(int proposalId, bool? createIfNotExists = false);

        /// <summary>
        /// Save a document
        /// </summary>
        /// <param name="document">Document to save</param>
        void SaveDocument(DocumentDetailModelView document);

        /// <summary>
        /// Deletes the document.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        void DeleteDocument(int proposalId);

        /// <summary>
        /// Saves the new document.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        void SaveNewDocument(int proposalId);

        /// <summary>
        /// Validates the document detail model view.
        /// </summary>
        /// <param name="document">The document to validate.</param>
        /// <returns>A list of validation messages.</returns>
        ICollection<ValidationMessage> ValidateDocumentDetailModelView(DocumentDetailModelView document);

        /// <summary>
        /// Gets the Sections for the selected Revision for the dropdown
        /// </summary>
        /// <param name="revisionId">Revision ID</param>
        /// <returns>Sections for the selected Revision</returns>
        ICollection<SectionDetailModelView> GetSectionsForRevision(int revisionId);

        /// <summary>
		/// Generates the RDD document for the Proposal Id passed in.
		/// </summary>
		/// <param name="proposalId">Proposal ID</param>
		/// <param name="serverFileName">Server File Name</param>
		/// <param name="httpResponse">HTTP response object</param>
        /// <param name="portionMarkingRequired">Is Portion Marking Required</param>
		void GenerateRDD(int proposalId, string serverFileName, HttpResponseBase httpResponse, bool portionMarkingRequired);

        /// <summary>
        /// Generates the RDD document for the Proposal Id passed in.
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="serverFileName">Server File Name</param>
        /// <param name="stream">stream to write the file back to for download</param>
		/// <param name="modelView">document detail modelview (if available)</param>
        /// <param name="parentSectionOverride">Override value for Parent Section - used in ACV</param>
        /// <param name="includeDocumentDetails">If document details (introduction, clarification, table of contents) should be included in the export</param>
        /// <param name="portionMarkingRequired">Is Portion Marking Required</param>
        void GenerateRDD(int proposalId, string serverFileName, Stream stream, DocumentDetailModelView modelView, string parentSectionOverride = null, bool includeDocumentDetails = true, bool portionMarkingRequired = false);

        /// <summary>
        /// Check if RDSB Record exists for the given PTM Proposal ID
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>true if record exists, otherwise false</returns>
        bool DoesRdsbRecordExistForProposalId(int proposalId);

        /// <summary>
		/// Gets data necessary for automation of a coversheet. Specifically sections that contain 1) CASB and 2) Non-Compliance data
		/// </summary>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <returns>Data to support a Cover Sheet creation</returns>
		(string CasbSection, string NonComplianceSection) GetCoverSheetData(int proposalId);

		/// <summary>
		/// Gets data necessary for CPS Reports
		/// </summary>
		/// <param name="rateCodes">List of rate codes</param>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <returns>Data to support a CPS Report</returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		ICollection<string> GetTopLevelSectionsForRateCodes(ICollection<string> rateCodes, int proposalId);

		/// <summary>
		/// Gets data necessary for CPS Reports
		/// </summary>
		/// <param name="rateDescriptions">List of rate descriptions</param>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <returns>Data to support a CPS Report</returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		ICollection<string> GetTopLevelSectionsForRateDescriptions(ICollection<string> rateDescriptions, int proposalId);

		/// <summary>
		/// Get all of the addresses based on restricting it to the Include In Cover Sheet property and for the specific PPR&D version
		/// </summary>
		/// <param name="ptmTrackingId">The PTM Tracking #/Proposal ID</param>
		/// <returns>A collection of addresses</returns>
		ICollection<SectionAddressModelView> GetAddresses(int ptmTrackingId);
	}
}
