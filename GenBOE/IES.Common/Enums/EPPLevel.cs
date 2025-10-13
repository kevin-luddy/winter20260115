// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
	using System.ComponentModel;

	/// <summary>
	/// EPP Level Enum - must match enum in eEPP
	/// </summary>
	public enum EPPLevel
	{
		[Description("Program")]
		Program = 1,

		[Description("LOB")]
		LOB = 2,

		[Description("Space")]
		Space = 3,

		[Description("Corporate")]
		Corp = 4
	}
}