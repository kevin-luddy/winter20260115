// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;

    /// <summary>
    /// Help text attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public sealed class HelpTextAttribute : Attribute
    {
        /// <summary>
        /// Help text
        /// </summary>
        private string helpText;

        /// <summary>
        /// Attribute for providing help text.
        /// </summary>
        /// <param name="helpText">helpText</param>
        public HelpTextAttribute(string helpText)
        {
            this.helpText = helpText;
        }

        /// <summary>
        /// Help text
        /// </summary>
        public string HelpText
        {
            get
            {
                return this.helpText;
            }
        }
    }
}
