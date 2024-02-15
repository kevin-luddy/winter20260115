// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Security
{
	using IES.Common.Core.Enums;

	/// <summary>
	/// Composite of authorization and role enumerated values
	/// </summary>
	public class SecurityAuthorizationAndRole
	{
		/// <summary>
		/// Authorization
		/// </summary>
		public SecurityAuthorization Authorization { get; set; }

		/// <summary>
		/// Role
		/// </summary>
		public PtmRole Role { get; set; }
	}
}
