// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using IES.Common;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for Document Detail Loader
    /// </summary>
    public interface IDocumentDetailLoader : IDataLoader<DocumentDetailModelView>
    {
        /// <summary>
        /// Gets Document Detail MV by the proposal id
        /// </summary>
        /// <param name="proposalId">proposal id</param>
        /// <returns>Document Detail MV</returns>
        DocumentDetailModelView GetByProposalId(int proposalId);
    }
}
