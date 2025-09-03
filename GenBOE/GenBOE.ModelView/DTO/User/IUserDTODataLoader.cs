// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using System.Collections.ObjectModel;
	using System.Collections.Generic;
	using GenBOE.Dtos;
	using IES.Common;

	public interface IUserDTODataLoader
	{
		/// <summary>
		/// Get a user by their database id
		/// </summary>
		/// <param name="inUserId">id in the database</param>
		/// <returns>user if found, null if not</returns>
		UserDTO GetUserByID(int inUserId);

		/// <summary>
		/// Get all users
		/// </summary>
		/// <returns>all users</returns>
		Collection<UserDTO> GetAllUsers();

		/// <summary>
		/// Get all user ids
		/// </summary>
		/// <returns>all user ids</returns>
		Collection<int> GetAllUserIds();

		/// <summary>
		/// Get a list users by their database ids.
		/// </summary>
		/// <param name="inUserIds">Ids in the database.</param>
		/// <returns>List of users if found, empty list if not.</returns>
		ICollection<UserDTO> GetByIds(ICollection<int> inUserIds);

		/// <summary>
		/// Get a list users by their database ids.
		/// </summary>
		/// <param name="inUserIds">Ids in the database.</param>
		/// <returns>List of users if found, empty list if not.</returns>
		UserDTO GetUserForActiveUser();

		/// <summary>
		/// Return a user by looking them up by their NTID.  If the user does not exist but can be found in AD, it is created.
		/// </summary>
		/// <param name="inNtid">User's (or group's) NTID.</param>
		/// <returns>UserDTO for the user.  Null if user could not be found in AD.</returns>
		UserDTO GetOrCreateUserByNtid(string inNtid);

		/// <summary>
		/// Gets Ids of users based on their ntids.
		/// </summary>
		/// <param name="userData">List of NTIDs.</param>
		/// <returns>User ids</returns>
		ICollection<int> GetIdsByNtid(ICollection<string> ntids);

		/// <summary>
		/// Return a list of users who belong to a specific group
		/// </summary>
		/// <param name="inUserNTID">group id to locate users for</param>
		/// <returns>user IDs found, null if not found</returns>
		Collection<int> GetUserIDsByGroupID(int inGroupID);

		/// <summary>
		/// Saves a user.
		/// </summary>
		/// <param name="inUser">User DTO object</param>
		/// <returns>Returns UserDTO object</returns>
		UserDTO SaveUser(UserDTO inUserDto);

		/// <summary>
		/// See if a user exists
		/// </summary>
		/// <param name="inUserNTID">user ntid to check for</param>
		/// <param name="outUserId">if the group exists, return the id</param>
		/// <returns>true/false user exists</returns>
		bool UserExists(string inUserNTID, out int outUserId);

		/// <summary>
		/// Save Message Confirmation for a User
		/// </summary>
		/// <param name="userID">The User's ID</param>
		/// <param name="message">The Confirmation Message</param>
		void SaveMessageConfirmation(int userID, ConfirmationMessage message);

		/// <summary>
		/// Returns list of Message Confirmations for the User
		/// </summary>
		/// <param name="userID">ETI User ID</param>
		/// <returns>List of Message Confirmations for the User</returns>
		ICollection<ConfirmationMessage> GetMessageConfirmations(int userID);

		/// <summary>
		/// Get Author and Subcontractor Author UserDTOs for the given BOE ID
		/// </summary>
		/// <param name="boeId">BOE ID</param>
		/// <returns>Collection of UserDTOs for BOE Authors and Subcontractor Authors</returns>
		ICollection<UserDTO> GetBoeAuthors(int boeId);
	}
}
