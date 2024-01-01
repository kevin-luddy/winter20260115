// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace IES.Standard
{
    using System;
    using System.Security.Principal;

    /// <summary>
    /// Interface for Security Information
    /// </summary>
    public interface ISecurityInformation
    {
        global::IES.Standard.UserData ActiveUserData { get; }
        string ActiveUserNTID { get; }
        string GetRoleAsString(global::System.Collections.ObjectModel.Collection<global::IES.Standard.Role> inRoles);
        string ResourceAccount { get; }
        Boolean IsDomesticUser(IPrincipal inPrincipal);
        Boolean IsRdmAdminUser(string inUserName);
        Boolean IsRdmCobraAdminUser(string inUserName);
        Boolean IsRdmViewerUser(string inUserName);
        Boolean IsIESPortalAdminUser(string inUserName);
        /// <summary>
        /// Determines if user is a subcontractor (and not overridden)
        /// </summary>
        /// <param name="ntid">NTID</param>
        /// <param name="isSubcontractor">bool from db if user is subcontractor</param>
        /// <returns>true if user is a subcontractor and not overridden</returns>
        Boolean IsSubcontractorUser(string ntid, bool? isSubcontractor);
        Boolean IsAllowedProPricerAccess(string userName);
        bool IsMemberOfADGroupInAppSettingsList(string inUserName, string inADGroupListAppSettingsKey);
    }
}
