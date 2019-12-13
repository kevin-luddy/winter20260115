// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Common.Security
{
    using System;
    using IES.Common;

    /// <summary>
    /// Immutable class to capture the response for use in code to understand
    /// a specific set of permissions based on a specific set of
    /// object IDs in the system.
    /// </summary>
    [Serializable]
    public class SecurityPermissionsResponse
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="authorizedRole">Authorized Role</param>
        /// <param name="proposalId">Proposal Id</param>
        public SecurityPermissionsResponse(PtmRole authorizedRole, int? proposalId)
        {
            this.AuthorizedRole = authorizedRole;
            this.ProposalID = proposalId;
        }

        /// <summary>
        /// The roles the user is authorized for
        /// </summary>
        public PtmRole AuthorizedRole { get; private set; }

        /// <summary>
        /// Optional proposalID the user is authorized for
        /// </summary>
        public int? ProposalID { get; private set; }
    }
}
