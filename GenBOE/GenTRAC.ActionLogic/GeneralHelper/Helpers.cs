// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
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
        /// <param name="actualSubmittalDate">Actual Submittal Date is in the Checklist tab / table. 
        ///     NOT to be confused with:
        ///         - Workflow / Proposal Submitted Date (when the workflow is completed), 
        ///         - "Revised Submittal Date" in the proposal general info tab / table</param>
        /// <returns>True if the certification is considered missing and late</returns>
        public static bool IsProposalCertificationLate(ProposalStatus status, DateTime? actualSubmittalDate)
        {
            return status == ProposalStatus.Submitted
                && (!actualSubmittalDate.HasValue 
                    || DateTime.Now.Date > actualSubmittalDate.Value.AddDays(60).Date);
        }
    }
}