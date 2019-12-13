// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [Serializable()]
    [ExcludeFromCodeCoverage]
    public class TMResourceRateDTO : ResourceRateDTO, IWorkspaceMembership
    {
        public TMResourceRateDTO() : base()
        {
            ResourceRateID = -1;
        }

        /// <summary>
        /// Gets or sets WorkspaceID
        /// </summary>
        public int WorkspaceID { get; set; }
    }
}
