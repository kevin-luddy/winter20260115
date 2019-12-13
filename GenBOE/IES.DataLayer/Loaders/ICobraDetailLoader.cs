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
    /// Cobra Grid Loader
    /// </summary>
    public interface ICobraDetailLoader : IBulkDataLoader<CobraDetailModelView>
    {
        /// <summary>
        /// Get COBRA Details by Revision.
        /// </summary>
        /// <param name="revision">Revision to retrieve.</param>
        /// <returns>All COBRA Mapping Details for the given revision.</returns>
        ICollection<CobraDetailModelView> GetCobraDetailsByRevision(RevisionModelView revision);

        /// <summary>
        /// Save CobraDetailModelView to database and reset Dirty flags.
        /// </summary>
        /// <param name="dirtyCobraDetails">The collection of CobraDetailModelView with changes.</param>
        void SaveDetails(ICollection<CobraDetailModelView> dirtyCobraDetails);
    }
}