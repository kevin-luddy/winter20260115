// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Enums
{
	using System.ComponentModel;

	/// <summary>
	/// Class to determine String Manipulation
	/// </summary>
	public enum StringManipulation
	{
		/// <summary>
		/// No Manipulation to string
		/// </summary>
		[Description("None")]
		None,

		/// <summary>
		/// Make string lower case
		/// </summary>
		[Description("ToLower")]
		ToLower,

		/// <summary>
		/// Make string upper case
		/// </summary>
		[Description("ToUpper")]
		ToUpper
	}
}
