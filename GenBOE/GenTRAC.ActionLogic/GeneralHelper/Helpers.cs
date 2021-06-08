// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.GeneralHelper
{
    using System;
    using IES.Common;

    /// <summary>
    /// General Helpers class
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Is Proposal Certification Required and Missing
        /// </summary>
        /// <param name="status">Proposal Status</param>
        /// <param name="submittedToContracts">Date Estimating Submits to Contracts is in the Checklist tab / table. 
        ///     NOT to be confused with:
        ///         - Workflow / Proposal Submitted Date (when the workflow is completed), 
        ///         - "Revised Anticipated Delivery Date" in the proposal general info tab / table</param>
        ///         - Actual Submittal Date in the Contracts tab
        /// <returns>True if the certification is considered missing and late</returns>
        public static bool IsProposalCertificationLate(ProposalStatus status, DateTime? submittedToContracts)
        {
            return status == ProposalStatus.Submitted
                && (!submittedToContracts.HasValue 
                    || DateTime.Now.Date > submittedToContracts.Value.AddDays(60).Date);
        }
    }
}