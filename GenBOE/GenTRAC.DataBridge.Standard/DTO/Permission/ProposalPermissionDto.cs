// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using IES.Standard;

    /// <summary>
    /// Permission Dto
    /// </summary>
    [Serializable]
    public class ProposalPermissionDto : PermissionDto
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public ProposalPermissionDto()
        {
            this.ProposalID = null;
            this.ResourceType = ResourceType.NotSet;
        }

        /// <summary>
        /// Proposal Id
        /// </summary>
        public int? ProposalID { get; set; }

        /// <summary>
        /// if the role is additional Estimating resource 1 or 2, we need resource type
        /// </summary>
        public ResourceType ResourceType { get; set; }
    }
}
