// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Interfaces
{
	using System.Security.Principal;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;

	/// <summary>
	/// Interface for Security Information
	/// </summary>
	public interface ISecurityInformation
	{
		UserData ActiveUserData { get; }
		string ActiveUserNTID { get; }
		string GetRoleAsString(System.Collections.ObjectModel.Collection<Role> inRoles);
		string ResourceAccount { get; }
		bool IsDomesticUser(IPrincipal inPrincipal);
		bool IsRdmAdminUser(string inUserName);
		bool IsRdmCobraAdminUser(string inUserName);
		bool IsRdmViewerUser(string inUserName);
		bool IsIESPortalAdminUser(string inUserName);
		/// <summary>
		/// Determines if user is a subcontractor (and not overridden)
		/// </summary>
		/// <param name="ntid">NTID</param>
		/// <param name="isSubcontractor">bool from db if user is subcontractor</param>
		/// <returns>true if user is a subcontractor and not overridden</returns>
		bool IsSubcontractorUser(string ntid, bool? isSubcontractor);
		bool IsAllowedProPricerAccess(string userName);
		bool IsMemberOfADGroupInAppSettingsList(string inUserName, string inADGroupListAppSettingsKey);
	}
}
