// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.IO.Export
{
    using System.Collections.Generic;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for RDM Revision Exporter
    /// </summary>
    public interface IRdmRevisionExporter
    {
        /// <summary>
        /// This function handles exporting an JSON representation of data in a revision.
        /// </summary>
        /// <param name="currentUserDisplayName">Display name for current user</param>
        /// <param name="revision">Revision MV to export</param>
        /// <param name="sections">Collection of Section MVs</param>
        /// <param name="rates">Collection of RateDetail MVs</param>
        /// <param name="cobraDetails">Collection of CobraDetail MVs</param>
        /// <param name="burdenPoolGridModel">Burden Pool Grid MV</param>
        /// <param name="jsonFilePath">JSON File Path</param>
        void ExportRevisionAsJson(string currentUserDisplayName, RevisionModelView revision, ICollection<SectionModelView> sections,
            ICollection<RateDetailModelView> rates, ICollection<CobraDetailModelView> cobraDetails,
            BurdenPoolGridModelView burdenPoolGridModel, string jsonFilePath);
    }
}
