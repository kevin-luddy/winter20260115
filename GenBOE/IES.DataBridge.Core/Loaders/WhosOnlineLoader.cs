// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using IES.Common.Core;
	using IES.Common.Core.Models;
	using IES.Common.Core.Utilities;
	using IES.DataBridge.ModelViews;
	using IES.Models;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Who's Online Loader
	/// </summary>
	public class WhosOnlineLoader : IWhosOnlineLoader
	{
		/// <summary>
		/// Logger
		/// </summary>
		private readonly ILogger log;

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="logger">logger</param>
		public WhosOnlineLoader(ILogger<WhosOnlineLoader> logger)
		{
			this.log = logger;
		}

		/// <summary>
		/// Updates the database with the latest access time of the current user.
		/// </summary>
		/// <param name="userData">User Data</param>
		/// <param name="applicationName">Name of the application the user is accessing</param>
		public void UpdateLastAccessTime(UserData userData, string applicationName)
		{
			if (userData == null)
			{
				throw new ArgumentNullException(nameof(userData));
			}

			using (StopwatchTimer sw = new(this.log))
			{
				// Upsert SP
				using (IESEntities iesEntities = new())
				{
					iesEntities.upsertUserLog(userData.Ntid, userData.DisplayName, applicationName);
				} // end using iesEntities
			}
		}

		/// <summary>
		/// Gets Who's Online Data
		/// </summary>
		/// <param name="applicationName">Name of the application to get Who's Online for</param>
		/// <returns>Who's Online Data</returns>
		public ICollection<WhosOnlineModelView> GetWhosOnlineData(string applicationName)
		{
			ICollection<WhosOnlineModelView> toReturn = new Collection<WhosOnlineModelView>();

			using (StopwatchTimer sw = new(this.log))
			{
				using (IESEntities iesEntities = new())
				{
					// Get up to 30 most recent users
					Collection<UserLog> users = (from allUsers in iesEntities.UserLogs
												 where allUsers.Application == applicationName
												 orderby allUsers.LogInUpdateDT descending
												 select allUsers).Take(30).ToCollection();
					
					foreach (UserLog user in users)
					{
						toReturn.Add(new WhosOnlineModelView()
						{
							DisplayName = user.DisplayName,
							Ntid = user.NTID,
							TimeLastAccessed = user.LogInUpdateDT
						});
					}
				} // end using iesEntities
			}

			return toReturn;
		}
	}
}
