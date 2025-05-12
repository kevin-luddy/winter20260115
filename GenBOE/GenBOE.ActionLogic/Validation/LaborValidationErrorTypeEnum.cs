// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    /// <summary>
    /// Enum used for validation of Tasks, to keep track of the level at which the error has occurred
    /// </summary>
    public enum LaborValidationErrorTypeEnum
    {
        /// <summary>
        /// Task Element
        /// </summary>
        TaskElement = 1,

        /// <summary>
        /// Resource Type
        /// </summary>
        ResourceType = 2,

        /// <summary>
        /// Resource Spread
        /// </summary>
        ResourceSpread = 3,

        /// <summary>
        /// An issue w/ the delta not being 0
        /// </summary>
        Delta = 4,

        /// <summary>
        /// Resource Type & Spread Hours Sum
        /// </summary>
        ResourceTypeSpreadSum = 5,

        /// <summary>
        /// Resource Type & Spread Cost Sum
        /// </summary>
        ResourceTypeSpreadCost = 6,

        /// <summary>
        /// Other Issues
        /// </summary>
        Other = 7
    }
}