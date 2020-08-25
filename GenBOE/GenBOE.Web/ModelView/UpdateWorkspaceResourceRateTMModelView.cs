// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System;

    /// <summary>
    /// UpdateWorkspaceResourceRateTMModelView
    /// </summary>
    public class UpdateWorkspaceResourceRateTMModelView
    {
        /// <summary>
        /// constructor
        /// </summary>
        public UpdateWorkspaceResourceRateTMModelView()
        {
            LastUpdatedTimeLMRates = null;
            this.ShowLMRatesDialog = false;
        }

        public DateTime? LastUpdatedTimeLMRates { get; set; }

        public bool ShowLMRatesDialog { get; set; }

    }
}