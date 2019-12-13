// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace IES.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using IES.DataBridge.ModelViews;
    using Newtonsoft.Json;

    /// <summary>
    /// The RDM Revision Exporter.
    /// </summary>
    public class RdmRevisionExporter : IRdmRevisionExporter
    {
        #region Public Methods

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
        public void ExportRevisionAsJson(string currentUserDisplayName, RevisionModelView revision, ICollection<SectionModelView> sections, 
            ICollection<RateDetailModelView> rates, ICollection<CobraDetailModelView> cobraDetails,
            BurdenPoolGridModelView burdenPoolGridModel, string jsonFilePath)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            RevisionImportExportModelView revisionImportExportModelView = new RevisionImportExportModelView
            {
                GeneratedBy = currentUserDisplayName,
                GeneratedDate = DateTime.Now,
                Revision = revision,
                Sections = new List<SectionModelView>(sections),
                Rates = new List<RateDetailModelView>(rates),
                CobraDetails = new List<CobraDetailModelView>(cobraDetails),
                BurdenPoolGridModel = burdenPoolGridModel
            };

            // serialize JSON to a string and then write string to a file
            File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(revisionImportExportModelView));
        }

        #endregion
    }
}