// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using IES.Common;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Http;
	using System.Web.Http.Cors;

	/// <summary>
	/// User data controller for getting user data.
	/// </summary>
	[EnableCors("*", "*", "*", SupportsCredentials = true)]
	public class UserDataController : BoeDataBaseAPIController
	{
		#region Properties & Ctor

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("BOEConfigurationController");

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="securityAccess">Security Access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>
		public UserDataController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{

		}
		#endregion

		/// <summary>
		/// Gets the user look up data by Ntid.
		/// </summary>
		/// <param name="ntid">Ntid</param>
		/// <returns>User data matching ntid.</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<UserDataViewModel> GetUserLookupData(string ntid)
		{
			IESResponse<UserDataViewModel> result = new IESResponse<UserDataViewModel>();

			try
			{
				Console.WriteLine(ntid);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Workspace menu data: {ex.Message}");
			}

			return result;
		}

		///// <summary>
		///// Search Active Directory by user account name and return exact match
		///// </summary>
		///// <param name="userAccount">The user's NT account name</param>
		///// <returns>Exact match (only)</returns>
		//public JsonResult SearchUserName(string userAccount)
		//{
		//	ICollection<UserData> matchingUsers = string.IsNullOrEmpty(userAccount) ? new List<UserData>() : this.homeLogic.SearchUsers(userAccount, ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Exact);

		//	JsonResult result;

		//	if (matchingUsers.Count == 1)
		//	{
		//		UserData match = matchingUsers.First();

		//		result = this.Json(new
		//		{
		//			success = true,
		//			userAccount = userAccount,
		//			userFullName = match.DisplayName,
		//			isGroup = match.IsGroup,
		//			workPhone = match.Phone
		//		});
		//	}
		//	else
		//	{
		//		result = this.Json(new
		//		{
		//			success = false,
		//			error = "No exact match was found"
		//		});
		//	}

		//	return result;
		//}
	}
}