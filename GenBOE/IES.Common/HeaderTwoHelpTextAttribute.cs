// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;

    /// <summary>
    /// Header Help text attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public sealed class HeaderTwoHelpTextAttribute : Attribute
    {
        /// <summary>
        /// Help text
        /// </summary>
        private string headerTwoHelpText;

        /// <summary>
        /// Attribute for providing help text.
        /// </summary>
        /// <param name="headerTwoHelpText">helpText</param>
        public HeaderTwoHelpTextAttribute(string headerTwoHelpText)
        {
            this.headerTwoHelpText = headerTwoHelpText;
        }

        /// <summary>
        /// Help text
        /// </summary>
        public string HeaderTwoHelpText
        {
            get
            {
                return this.headerTwoHelpText;
            }
        }
    }
}
