// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView
{
    /// <summary>
    /// Approvals Index model view
    /// </summary>
    public class ApprovalsIndexModelView : PersistedDataModelView
    {
        /// <summary>
        /// Checklist version
        /// </summary>
        public int? Version { get; set; }

        /// <summary>
        /// Bool noting if any roles are missing
        /// </summary>
        public bool MissingRole { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ApprovalsIndexModelView()
        {
            this.Version = null;
            this.MissingRole = false;
            this.LeadEstimatorName = "Unspecified";
        }

        /// <summary>
        /// Flag for whether current user is Pricer for this proposal.
        /// </summary>
        public bool IsPricer { get; set; }

        /// <summary>
        /// Gets or sets the name of the lead estimator.
        /// </summary>
        public string LeadEstimatorName { get; set; }
    }
}
