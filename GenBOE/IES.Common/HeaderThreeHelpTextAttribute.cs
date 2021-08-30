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
    public sealed class HeaderThreeHelpTextAttribute : Attribute
    {
        /// <summary>
        /// Help text
        /// </summary>
        private string headerThreeHelpText;

        /// <summary>
        /// Attribute for providing help text.
        /// </summary>
        /// <param name="headerThreeHelpText">helpText</param>
        public HeaderThreeHelpTextAttribute(string headerThreeHelpText)
        {
            this.headerThreeHelpText = headerThreeHelpText;
        }

        /// <summary>
        /// Help text
        /// </summary>
        public string HeaderThreeHelpText
        {
            get
            {
                return this.headerThreeHelpText;
            }
        }
    }
}
