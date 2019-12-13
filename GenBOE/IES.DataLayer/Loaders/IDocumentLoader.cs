// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System.Collections.Generic;
    using IES.Common;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for a Document Loader
    /// </summary>
    /// <seealso cref="IES.Common.IDataLoader{IES.DataBridge.ModelViews.DocumentGridModelView}" />
    public interface IDocumentLoader : IDataLoader<DocumentGridModelView>
    {
        /// <summary>
        /// Gets the by proposal ids.
        /// </summary>
        /// <param name="proposalIds">The proposal ids.</param>
        /// <param name="latestRevisionId">The latest revision Id.</param>
        /// <returns>A collection of document model views.</returns>
        ICollection<DocumentGridModelView> GetByProposalIds(ICollection<int> proposalIds, int latestRevisionId);
    }
}
