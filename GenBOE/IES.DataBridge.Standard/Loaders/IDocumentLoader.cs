// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System.Collections.Generic;
    using IES.Standard;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for a Document Loader
    /// </summary>
    /// <seealso cref="IES.Standard.IDataLoader{IES.DataBridge.ModelViews.DocumentGridModelView}" />
    public interface IDocumentLoader : IDataLoader<DocumentGridModelView>
    {
        /// <summary>
        /// Gets the by proposal ids.
        /// </summary>
        /// <param name="proposalIds">The proposal ids.</param>
        /// <param name="latestRevisionId">The latest revision Id.</param>
        /// <returns>A collection of document model views.</returns>
        ICollection<DocumentGridModelView> GetByProposalIds(ICollection<int> proposalIds, int latestRevisionId);

        /// <summary>
        /// Check if a record exists for the given Proposal ID
        /// </summary>
        /// <param name="proposalId">PTM Proposal ID</param>
        /// <returns>true if record exists, otherwise false</returns>
        bool DoesRecordExist(int proposalId);
    }
}
