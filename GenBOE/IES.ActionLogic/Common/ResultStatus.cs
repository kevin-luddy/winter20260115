// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2021 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Common
{
	/// <summary>
	/// Result statuses
	/// </summary>
	public enum ResultStatus
	{
		/// <summary>
		/// Succeeded - Ok
		/// </summary>
		Ok = 0,

		/// <summary>
		/// Failed to run
		/// </summary>
		Failed = 1,

		/// <summary>
		/// Failed to find
		/// </summary>
		NotFound = 2,

		/// <summary>
		/// Not authroized
		/// </summary>
		NotAuthorized = 3,

		/// <summary>
		/// Invalid input
		/// </summary>
		InvalidInput = 4,

		/// <summary>
		/// Http failure
		/// </summary>
		HttpFailure = 5,

		/// <summary>
		/// Warnings to Display
		/// </summary>
		Warnings = 6
	}
}
