// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Attributes
{
    using System;

    /// <summary>
    /// Display order of an enumeration literal.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public sealed class DisplayOrderAttribute : Attribute
    {

        /// <summary>
        /// DisplayOrderAttribute to indicate if a data type enum is a questionaire or not.
        /// </summary>
        /// <param name="displayOrder">isAQuestionaire</param>
        public DisplayOrderAttribute(int displayOrder)
        {
            DisplayOrder = displayOrder;
        }

        /// <summary>
        /// Relative order in which the enums should be displayed.
        /// </summary>
        public int DisplayOrder { get; }
    }
}
