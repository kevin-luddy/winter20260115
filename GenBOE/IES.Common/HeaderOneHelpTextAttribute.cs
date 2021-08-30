// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;

    /// <summary>
    /// Header Help text attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public sealed class HeaderOneHelpTextAttribute : Attribute
    {
        /// <summary>
        /// Help text
        /// </summary>
        private string headerOneHelpText;

        /// <summary>
        /// Attribute for providing help text.
        /// </summary>
        /// <param name="headerOneHelpText">helpText</param>
        public HeaderOneHelpTextAttribute(string headerOneHelpText)
        {
            this.headerOneHelpText = headerOneHelpText;
        }

        /// <summary>
        /// Help text
        /// </summary>
        public string HeaderOneHelpText
        {
            get
            {
                return this.headerOneHelpText;
            }
        }
    }
}
