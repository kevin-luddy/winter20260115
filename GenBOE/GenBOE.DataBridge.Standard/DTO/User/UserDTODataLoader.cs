// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data.Entity.Core;
	using System.Data.SqlClient;
	using System.Linq;
	using GenBOE.Dtos;
	using GenBOE.Models;
	using IES.Standard;

	public class UserDTODataLoader : IUserDTODataLoader
	{
		private readonly ILogger _log;
		private ISecurityInformation _SecurityInformation;
		private IActiveDirectoryUtilities _ActiveDirectoryUtilities;

		/// <summary>
		/// Cache Object
		/// </summary>
		private ICache cache;

		/// <summary>
		/// The number of seconds to store in cache
		/// </summary>
		private int secondsToCache = 30;

		/// <summary>
		/// Cache key for User's data
		/// </summary>
		private string cacheKeyUser = "UserDataLoader_";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="inSecurityInformation">Security Information</param>
		/// <param name="inActiveDirectoryUtilities">Active Directory Utilities</param>
		public UserDTODataLoader(ISecurityInformation inSecurityInformation,
								 IActiveDirectoryUtilities inActiveDirectoryUtilities,
								 ICache cache,
								 ILogger logger)
		{
			_SecurityInformation = inSecurityInformation;
			_ActiveDirectoryUtilities = inActiveDirectoryUtilities;
			this.cache = cache;
			this._log = logger;
		}

		
		virtual public Collection<UserDTO> GetAllUsers()
		{
			Collection<UserDTO> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					IEnumerable<UserDTO> resultLinq = from s in gbe.ETIusers
													  select new UserDTO
													  {
														  UserID = s.ETIUserID,
														  NTID = s.NTID.ToLower(),
														  EmailAddress = (s.EmailAddress == null) ? string.Empty : s.EmailAddress.ToLower(),
														  PhoneNumber = s.PhoneNumber,
														  DisplayName = s.DisplayName,
														  LastName = s.LastName,
														  FirstName = s.FirstName,
														  UpdateDate = s.UpdateDT,
														  IsUsPerson = s.IsUsPerson,
														  IsSubcontractor = s.IsSubcontractor
													  };

					toReturn = new Collection<UserDTO>(resultLinq.ToArray());
				}
			}

			toReturn = toReturn.OrderBy(x => x.DisplayName).ToCollection<UserDTO>();

			return toReturn;
		}

		
		virtual public Collection<int> GetAllUserIds()
		{
			Collection<int> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					IEnumerable<int> resultLinq = (from s in gbe.ETIusers
												   select s.ETIUserID);

					toReturn = new Collection<int>(resultLinq.ToArray());
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Save Message Confirmation for a User
		/// </summary>
		/// <param name="userID">The User's ID</param>
		/// <param name="message">The Confirmation Message</param>
		
		public virtual void SaveMessageConfirmation(int userID, ConfirmationMessage message)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.insertMessageConfirmation(userID, (int)message);
			}
		}

		/// <summary>
		/// Returns list of Message Confirmations for the User
		/// </summary>
		/// <param name="userID">ETI User ID</param>
		/// <returns>List of Message Confirmations for the User</returns>
		
		public virtual ICollection<ConfirmationMessage> GetMessageConfirmations(int userID)
		{
			ICollection<ConfirmationMessage> toReturn = new List<ConfirmationMessage>();

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				var resultsLinq = from c in gbe.MessageConfirmations
								  where c.ETIUserId == userID
								  select c.MessageId;

				toReturn = resultsLinq.Select(r => (ConfirmationMessage)r).ToList();
			}

			return toReturn;
		}

		/// <summary>
		/// See if a user exists in the database.
		/// </summary>
		/// <param name="inUserNTID">user ntid to check for</param>
		/// <param name="outUserId">if the group exists, return the id</param>
		/// <returns>true/false user exists</returns>
		
		virtual public bool UserExists(string inUserNTID, out int outUserId)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				var resultsLinq = from c in gbe.ETIusers
								  where c.NTID == inUserNTID.ToLower()
								  select c.ETIUserID;

				outUserId = resultsLinq.FirstOrDefault();
			}

			// a userid of 0 indicates it was not found
			return outUserId != 0;
		}

		/// <summary>
		/// Get a user by their database id
		/// </summary>
		/// <param name="inUserId">id in the database</param>
		/// <returns>user if found, null if not</returns>
		
		virtual public UserDTO GetUserByID(int inUserId)
		{
			UserDTO toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					IEnumerable<UserDTO> resultLinq = from s in gbe.ETIusers
													  where s.ETIUserID == inUserId
													  select new UserDTO
													  {
														  UserID = s.ETIUserID,
														  DisplayName = s.DisplayName,
														  FirstName = s.FirstName,
														  LastName = s.LastName,
														  EmailAddress = (s.EmailAddress == null) ? string.Empty : s.EmailAddress.ToLower(),
														  NTID = s.NTID.ToLower(),
														  PhoneNumber = s.PhoneNumber,
														  UpdateDate = s.UpdateDT,
														  IsUsPerson = s.IsUsPerson,
														  IsSubcontractor = s.IsSubcontractor
													  };

					// return the first value or null if not found
					toReturn = resultLinq.FirstOrDefault();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Get a list users by their database ids.
		/// </summary>
		/// <param name="inUserIds">Ids in the database.</param>
		/// <returns>List of users if found, empty list if not.</returns>
		
		virtual public ICollection<UserDTO> GetByIds(ICollection<int> inUserIds)
		{
			ICollection<UserDTO> toReturn = new Collection<UserDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					IEnumerable<UserDTO> resultLinq = from s in gbe.ETIusers
													  where inUserIds.Contains(s.ETIUserID)
													  select new UserDTO
														  {
															  UserID = s.ETIUserID,
															  DisplayName = s.DisplayName,
															  FirstName = s.FirstName,
															  LastName = s.LastName,
															  EmailAddress = (s.EmailAddress == null) ? string.Empty : s.EmailAddress.ToLower(),
															  NTID = s.NTID.ToLower(),
															  PhoneNumber = s.PhoneNumber,
															  UpdateDate = s.UpdateDT,
															  IsUsPerson = s.IsUsPerson,
															  IsSubcontractor = s.IsSubcontractor
													  };
					if (resultLinq.Any())
					{
						toReturn = resultLinq.ToCollection<UserDTO>();
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Return a user by looking them up by their ntid
		/// </summary>
		/// <param name="inUserNTID">users ntid to locate them by</param>
		/// <returns>user found, null if not found</returns>
		
		private UserDTO GetUserByNTIDIfExists(string inNTID)
		{
			if (inNTID == null)
			{
				throw new ArgumentNullException(nameof(inNTID));
			}

			UserDTO toReturn = null;
			string key = this.cacheKeyUser + inNTID;

			// load the user from cache. if the key does not exist then null is returned
			toReturn = (UserDTO)this.cache.GetData(key);

			// if the user is not found in the cache, try to get it from the database
			if (toReturn == null)
			{
				using (StopwatchTimer sw = new StopwatchTimer(this._log))
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						IEnumerable<UserDTO> resultLinq = from s in gbe.ETIusers
														  where s.NTID == inNTID
														  select new UserDTO
														  {
															  UserID = s.ETIUserID,
															  DisplayName = s.DisplayName,
															  FirstName = s.FirstName,
															  LastName = s.LastName,
															  EmailAddress = (s.EmailAddress == null) ? string.Empty : s.EmailAddress.ToLower(),
															  NTID = s.NTID.ToLower(),
															  PhoneNumber = s.PhoneNumber,
															  UpdateDate = s.UpdateDT,
															  IsUsPerson = s.IsUsPerson,
															  IsSubcontractor = s.IsSubcontractor
														  };

						// return the first value or null if not found
						toReturn = resultLinq.FirstOrDefault();
					}
				}

				if (toReturn != null)
				{
					this.cache.Add(key, toReturn, secondsToCache);
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Return a user by looking them up by their NTID.  If the user does not exist but can be found in AD, it is created.
		/// </summary>
		/// <param name="inNtid">User's (or group's) NTID.</param>
		/// <returns>UserDTO for the user.  Null if user could not be found in AD.</returns>
		public virtual UserDTO GetOrCreateUserByNtid(string inNtid)
		{
			if (string.IsNullOrEmpty(inNtid))
			{
				throw new ArgumentNullException(inNtid);
			}

			if (inNtid.Contains('\\'))
			{
				throw new ArgumentException($"Do not include domain in NTID string - {inNtid}.");
			}

			UserDTO userDto = null;
			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				userDto = this.GetUserByNTIDIfExists(inNtid);
				if (userDto == null)
				{
					bool isGroup = inNtid.Contains('.');

					// Add the user from AD.
					UserData adUser = this._ActiveDirectoryUtilities.GetUserByQualifiedAccount(inNtid, isGroup);
					if (adUser != null)
					{
						userDto = this.SaveUser(new UserDTO
						{
							DisplayName = adUser.DisplayName,
							EmailAddress = adUser.Email,
							FirstName = adUser.FirstName,
							LastName = adUser.LastName,
							NTID = adUser.Ntid,
							PhoneNumber = adUser.Phone,
							UpdateDate = DateTime.Now,
							UserID = -1,
							IsUsPerson = adUser.IsUsPerson,
							IsSubcontractor = adUser.IsSubcontractor
						});
					}
				} 
			}

			return userDto;
		}

		/// <summary>
		/// Gets Ids of users based on their ntids.
		/// </summary>
		/// <param name="ntids">List of NTIDs.</param>
		/// <returns>User ids</returns>
		
		public virtual ICollection<int> GetIdsByNtid(ICollection<string> ntids)
		{
			if (ntids == null)
			{
				throw new ArgumentNullException(nameof(ntids));
			}

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					var data = (from s in gbe.ETIusers
								where ntids.Contains(s.NTID)
								select s.ETIUserID);

					return data.ToList();
				}
			}
		}

		/// <summary>
		/// Return a user by looking them up with their AD information.
		/// </summary>
		/// <param name="inUserData">Users AD information</param>
		/// <returns>User found, null if not found</returns>
		virtual public UserDTO GetUserByUserData(UserData inUserData)
		{
			if (inUserData == null)
			{
				throw new ArgumentNullException(nameof(inUserData));
			}

			return this.GetOrCreateUserByNtid(inUserData.Ntid);
		}

		/// <summary>
		/// Return the user dto for the actively logged in user
		/// </summary>
		/// <returns>user found, null if not found</returns>
		virtual public UserDTO GetUserForActiveUser()
		{
			return this.GetOrCreateUserByNtid(_SecurityInformation.ActiveUserNTID);
		}

		/// <summary>
		/// Return a list of users who belong to a specific group
		/// </summary>
		/// <param name="inUserNTID">group id to locate users for</param>
		/// <returns>user IDs found, null if not found</returns>
		
		virtual public Collection<int> GetUserIDsByGroupID(int inGroupID)
		{
			Collection<int> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					// Get all user IDs who have permissions defined using the specified group ID
					IEnumerable<int> resultLinqWorkspace = (from s in gbe.WorkspaceUserRoles
															select s.ETIUserID).Distinct();

					IEnumerable<int> resultLinqSystem = (from s in gbe.SystemUserRoles
														 select s.ETIUserID).Distinct();

					IEnumerable<int> resultLinqBOE = (from s in gbe.BOEPotentialRoles
													  select s.ETIUserID).Distinct();
					// NOTE: We don't bother pulling BOEUserRoles because the groupid there
					//       is not used (and has a WI to be removed) since the group can come from any
					//       group contained in the BOEPotentialRoles table

					// gather up all eti user ids
					List<int> allResults = new List<int>(resultLinqWorkspace);
					allResults.AddRange(resultLinqSystem);
					allResults.AddRange(resultLinqBOE);

					// get the distinct list of user ids to return
					var results = allResults.Distinct();

					if (results.Any())
					{
						// return the distinct set of user IDs
						toReturn = new Collection<int>(results.ToArray());
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Return a list of users who belong to the specified group.
		/// </summary>
		/// <param name="inGroupID">Group ID to retrieve members of</param>
		/// <returns>Collection of group members, or an empty set if there are none</returns>
		virtual public ICollection<UserDTO> GetUsersByGroupID(int inGroupID)
		{
			Collection<UserDTO> toReturn = new Collection<UserDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				// Get a list of all user Ids in the group.
				Collection<int> groupMemberIDs = this.GetUserIDsByGroupID(inGroupID);
				_log.Performance("DIRECT - GetUserIDsByGroupID", sw.ElapsedMilliseconds);

				if (groupMemberIDs != null && groupMemberIDs.Any())
				{
					toReturn = this.GetByIds(groupMemberIDs).OrderBy(x => x.DisplayName).ToCollection<UserDTO>();
				}
			}

			return toReturn;
		}

		virtual public UserDTO SaveUser(UserDTO inUserDto)
		{
			if (inUserDto == null)
			{
				throw new ArgumentNullException(nameof(inUserDto));
			}

			UserDTO toReturn = null;

			try
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn = GetUserByID(Convert.ToInt32(gbe.upsertUser(inUserDto.UserID,
																				inUserDto.NTID.ToLower(),
																				inUserDto.DisplayName,
																				string.IsNullOrEmpty(inUserDto.EmailAddress) ? string.Empty : inUserDto.EmailAddress.ToLower(),
																				inUserDto.PhoneNumber,
																				inUserDto.FirstName,
																				inUserDto.LastName,
																				inUserDto.UpdateDate,
																				inUserDto.IsUsPerson,
																				inUserDto.IsSubcontractor).FirstOrDefault()));
				}
			}
			catch (EntityCommandExecutionException ex)
			{
				_log.Error(ex, "An " + ex.InnerException + "occurred in " + ex.Source + " while saving " + inUserDto.ToString());
				throw;
			}
			catch (SqlException sqlEx)
			{
				_log.Error(sqlEx, "An exception occurred in " + sqlEx.Procedure + " while saving " + inUserDto.ToString() + " and has the following message: " + sqlEx.Message + "/n");
				throw;
			}
			catch (Exception e)
			{
				_log.Error(e, "an unhandled exception occurred " + " while saving " + inUserDto.ToString());
				throw;
			}

			return toReturn;
		}
	}
}
