// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using IES.Common.Core.Enums;

namespace IES.Common.Core.Security
{
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
