// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace GenTRAC.Common.Enums
{
    /// <summary>
    /// All pick lists that are editable via admin page
    /// </summary>
    public enum PickListEnum
    {
        /// <summary>
        /// Proposal Type
        /// </summary>
        [Description("Proposal Type")]
        ProposalType = 1,

        /// <summary>
        /// Level of commitment
        /// </summary>
        [Description("Proposal Class")]
        ProposalClass = 2,

        /// <summary>
        /// Type of request
        /// </summary>
        [Description("Type of Request")]
        TypeOfRequest = 3
    }
}
