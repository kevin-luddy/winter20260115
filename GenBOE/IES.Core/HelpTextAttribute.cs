// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
    using System;

    /// <summary>
    /// Help text attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public sealed class HelpTextAttribute : Attribute
    {

		/// <summary>
		/// Attribute for providing help text.
		/// </summary>
		/// <param name="helpText">helpText</param>
		public HelpTextAttribute(string helpText)
        {
            this.HelpText = helpText;
        }

		/// <summary>
		/// Help text
		/// </summary>
		public string HelpText { get; }
	}
}
