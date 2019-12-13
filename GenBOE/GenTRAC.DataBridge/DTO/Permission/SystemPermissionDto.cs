// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// System Permission Dto
    /// </summary>
    [Serializable]
    public class SystemPermissionDto : PermissionDto
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public SystemPermissionDto()
        {
            this.LineOfBusinessIDs = new Collection<int>();
        }

        /// <summary>
        /// Line Of Business Ids for a system viewer/Proposal Setup Admin
        /// </summary>
        public ICollection<int> LineOfBusinessIDs { get; set; }
    }
}
