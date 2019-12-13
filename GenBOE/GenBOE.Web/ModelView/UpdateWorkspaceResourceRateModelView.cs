using System;

namespace GenBOE.Web.ModelView
{
    public class UpdateWorkspaceResourceRateModelView
    {
        public UpdateWorkspaceResourceRateModelView()
        {
            this.LastUpdatedTimeZoneTravel = null;
            this.LastUpdatedTimeOffloadRates = null;
            this.ShowZoneTravelRatesDialog = false;
            this.ShowOffloadRatesDialog = false;
        }

        /// <summary>
        /// Indicates whether the Zone Travel "Update Rates" dialog should be shown.
        /// </summary>
        public bool ShowZoneTravelRatesDialog { get; set; }

        /// <summary>
        /// Indicates whether the Update Offload Rates dialog should be shown
        /// </summary>
        public bool ShowOffloadRatesDialog { get; set; }

        /// <summary>
        /// Stores the last updated date of the Zone Travel Rates
        /// </summary>
        public DateTime? LastUpdatedTimeZoneTravel { get; set; }

        /// <summary>
        /// Stores the last updated date of the Offload Rates
        /// </summary>
        public DateTime? LastUpdatedTimeOffloadRates { get; set; }
    }
}