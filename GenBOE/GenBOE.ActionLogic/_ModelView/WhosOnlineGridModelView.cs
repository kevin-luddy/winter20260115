// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic._ModelView
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.Dtos;
	using IES.Common;

	/// <summary>
	/// View model for Whos Online users.  Contains paging support.
	/// </summary>
	public class WhosOnlineGridModelView
	{
		/// <summary>
		/// Paramater less constructor required by serialization of the object from the
		/// GenBOEMetricsWhosOnline page.  
		/// </summary>
		public WhosOnlineGridModelView()
		{
		}

		/// <summary>
		/// Constructor used when building the initial Whos Online object.
		/// </summary>
		/// <param name="usersOnlineDetails">Detail descriptor of online users.</param>
		public WhosOnlineGridModelView(GenBOEUsersOnlineDTO usersOnlineDetails)
		{
			if (usersOnlineDetails != null && usersOnlineDetails.UserOnlineDetailsCollection != null)
			{
				// Sort by time last accessed.
				UserResults = usersOnlineDetails.UserOnlineDetailsCollection.OrderByDescending(x => x.TimeLastAccessed).ToCollection();

				foreach (UserOnlineDetails user in UserResults)
				{
					TimeSpan timeSinceLast = DateTime.Now - user.TimeLastAccessed;

					if (timeSinceLast.Days < 1) { user.TimeSinceLastAccess = timeSinceLast.ToString("hh\\:mm\\:ss"); }
					else { user.TimeSinceLastAccess = "More than 24 hours"; }
				}
			}
		}

		/// <summary>
		/// All online users.
		/// </summary>
		public ICollection<UserOnlineDetails> UserResults { get; set; }
	}
}
