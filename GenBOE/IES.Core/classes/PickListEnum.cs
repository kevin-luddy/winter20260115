// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace IES.Core
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
        TypeOfRequest = 3,

        /// <summary>
        /// Line Of Business
        /// </summary>
        [Description("Line Of Business")]
        LineOfBusiness = 4,

        /// <summary>
        /// Program Area
        /// </summary>
        [Description("Program Area")]
        ProgramArea = 5,

        /// <summary>
        /// Contract Type
        /// </summary>
        [Description("Contract Type")]
        ContractType = 6,

        /// <summary>
        /// Contract Type Group
        /// </summary>
        [Description("Contract Type Group")]
        ContractTypeGroup = 7
    }
}
