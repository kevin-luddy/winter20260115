// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
    using System;

    /// <summary>
    /// Header Help text attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public sealed class HeaderTwoHelpTextAttribute : Attribute
    {

		/// <summary>
		/// Attribute for providing help text.
		/// </summary>
		/// <param name="headerTwoHelpText">helpText</param>
		public HeaderTwoHelpTextAttribute(string headerTwoHelpText)
        {
            this.HeaderTwoHelpText = headerTwoHelpText;
        }

		/// <summary>
		/// Help text
		/// </summary>
		public string HeaderTwoHelpText { get; }
	}
}
