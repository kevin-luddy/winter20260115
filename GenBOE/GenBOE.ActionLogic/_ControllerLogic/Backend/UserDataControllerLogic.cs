// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic.Backend
{
	using GenBOE.ActionLogic.ModelView.Backend;
	using IES.Common;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// User data controller logic class (service).
	/// </summary>
	public class UserDataControllerLogic
	{
		/// <summary>
		/// Home controller logic.
		/// </summary>
		private IHomeControllerLogic homeControllerLogic { get; set; }

		/// <summary>
		/// ctor.
		/// </summary>
		public UserDataControllerLogic(IHomeControllerLogic homeControllerLogic)
		{
			this.homeControllerLogic = homeControllerLogic;
		}

		/// <summary>
		/// Gets a user based on a provided ntid.
		/// </summary>
		/// <param name="ntid">Ntid.</param>
		/// <returns>User data found.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public UserDataViewModel GetUserLookupData(string ntid)
		{
			ICollection<UserData> matchingUsers = string.IsNullOrEmpty(ntid) ? new List<UserData>() : this.homeControllerLogic.SearchUsers(ntid, ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Exact);

			UserDataViewModel userData = new UserDataViewModel()
			{
				UserAccount = ntid,
				UserFullName = matchingUsers.FirstOrDefault().DisplayName,
				IsGroup = matchingUsers.FirstOrDefault().IsGroup,
				WorkPhone = matchingUsers.FirstOrDefault().Phone
			};

			return userData;
		}
	}
}