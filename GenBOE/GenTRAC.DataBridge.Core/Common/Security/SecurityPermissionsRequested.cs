// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Common.Security
{
    using System.Diagnostics.CodeAnalysis;
    using IES.Core;

    /// <summary>
    /// Class to capture the request for permissions
    /// for a specific set of roles.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SecurityPermissionsRequested
    {
        /// <summary>
        /// The page to check, required
        /// </summary>
        public PtmSecurityPage PageToCheck { get; set; }

        /// <summary>
        /// The optional ProposalId to look at specific permissions for (required if using Proposal Roles)
        /// </summary>
        public int? ProposalId { get; set; }

        /// <summary>
        /// Default constructor.  Set all enums to 'None' values by default.
        /// </summary>
        public SecurityPermissionsRequested()
        {
            this.PageToCheck = PtmSecurityPage.None;
        }

        /// <summary>
        /// Pretty print details about object
        /// </summary>
        /// <returns>string representing object</returns>
        public override string ToString()
        {
            return string.Format("Page [{0}] CaptureId [{1}]",
                                        this.PageToCheck,
                                        this.ProposalId.HasValue ? this.ProposalId.Value.ToString() : "<null>");
        }
    }
}
