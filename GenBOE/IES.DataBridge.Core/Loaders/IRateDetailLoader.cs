// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
	using System.Collections.Generic;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Loaders;
	using IES.Common.Core.Models;
	using IES.DataBridge.ModelViews;

	/// <summary>
	/// Rate Grid Loader
	/// </summary>
	public interface IRateDetailLoader : IBulkDataLoader<RateDetailModelView>
    {
        /// <summary>
        /// Get rates by Revision.
        /// </summary>
        /// <param name="revision">Revision to retrieve.</param>
        /// <returns>All Rate Details for the given revision.</returns>
        ICollection<RateDetailModelView> GetRatesByRevision(RevisionModelView revision);

        /// <summary>
        /// Gets the wip Rates with section information.
        /// </summary>
        /// <param name="wipRevision">The WIP revision.</param>
        /// <returns>All of the Rates with Section detail for the WIP.</returns>
        ICollection<RateSectionModelView> GetWIPRateSections(RevisionModelView wipRevision);

        /// <summary>
        /// Get the Rate Details for the rates being imported.
        /// </summary>
        /// <param name="revision">Revision to retrieve.</param>
        /// <param name="rateCodes">Rate Codes being imported.</param>
        /// <returns>All Rate Details for the given revision.</returns>
        ICollection<RateDetailModelView> GetRatesForImport(RevisionModelView revision, string[] rateCodes);

        /// <summary>
        /// Get rates that have changed.
        /// </summary>
        /// <param name="revision">Revision to retrieve.</param>
        /// <param name="previousRevision">Previous revision to retrieve.</param>
        /// <returns>Rate Details with changes between revisions.</returns>
        ICollection<RateDetailModelView> GetComparableRates(RevisionOptionModelView revision, RevisionOptionModelView previousRevision);

        /// <summary>
        /// Gets all Resource Classes as an options list.
        /// </summary>
        /// <param name="revisionId">Revision Id</param>
        /// <returns>Resource Class options list.</returns>
        ICollection<OptionModelView> GetResourceClassOptions(int revisionId);

        /// <summary>
        /// Save RateGridModelView to database and reset Dirty flags.
        /// </summary>
        /// <param name="dirtyRateDetails">The collection of RateDetailModelViews with changes.</param>
        void SaveDetails(ICollection<RateDetailModelView> dirtyRateDetails);

        /// <summary>
        /// RateDescriptionsToMappings
        /// </summary>
        /// <param name="newRateCode">Rate Code for new objects.</param>
        /// <param name="dtoToUpsert">The RateDetail object.</param>
        void RateDescriptionsToMappings(int? newRateCode, RateDetailModelView dtoToUpsert);

        /// <summary>
        /// Gets the necessary details of Rate Codes for a given Revision for the RDSB Edit Document dropdowns
        /// Only retrieve rates for Direct Labor.
        /// </summary>
        /// <param name="revisionId">Revision ID</param>
        /// <returns>necessary details of Rate Codes for the RDSB Edit Document dropdown</returns>
        ICollection<RdsbRateDetailModelView> GetRatesForRdsbDocument(int revisionId);

        /// <summary>
        /// Gets the rate codes for a revision.
        /// </summary>
        /// <param name="revisionId">The revision identifier.</param>
        /// <returns>A collection of Rate Codes</returns>
        ICollection<RateDto> GetRateCodesForRevision(int revisionId);

        /// <summary>
        /// Verifies the rate code replication rate codes.
        /// </summary>
        /// <param name="revisionId">The revision identifier for WIP.</param>
        /// <returns>A list of validation errors if there are any.</returns>
        ICollection<ValidationMessage> VerifyRateCodeReplication(int revisionId);
    }
}