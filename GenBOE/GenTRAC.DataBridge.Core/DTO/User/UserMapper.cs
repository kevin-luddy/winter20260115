// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
    using System.Transactions;
    using GenTRAC.DataBridge.Common;
    using IES.Core;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// User Dto Data Mapper
	/// </summary>
	public class UserMapper : DataMapper<UserDTO, IUserLoader>, IInternalUserMapper
    {
        /// <summary>
        /// Security Information
        /// </summary>
        private ISecurityInformation securityInformation;

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        private IActiveDirectoryUtilities activeDirectoryUtilities;

        /// <summary>
        /// the cache
        /// </summary>
        private ICache cache = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inUserDataLoader">User data loader</param>
        /// <param name="inCacheDataLoader">Cache data loader</param>
        /// <param name="inSecurityInformation">Security Information</param>
        /// <param name="inActiveDirectoryUtilities">Active Directory Utilities</param>
        /// <param name="inCache">Cache</param>
        public UserMapper(IUserLoader inUserDataLoader,
                          ICacheDataLoader inCacheDataLoader,
                          ISecurityInformation inSecurityInformation,
                          IActiveDirectoryUtilities inActiveDirectoryUtilities,
                          ICache inCache,
                          ILogger logger) 
            :base (logger)
        {
            this.DataLoader = inUserDataLoader;
            this.CacheLoader = inCacheDataLoader;
            this.securityInformation = inSecurityInformation;
            this.activeDirectoryUtilities = inActiveDirectoryUtilities;
            this.cache = inCache;

            this.CacheKeyForGetById = CacheConstants.USER;
        }

        /// <summary>
        /// Get all user IDs
        /// </summary>
        /// <returns>returns all user ids in the system.</returns>
        public ICollection<int> GetAllIds()
        {
            ICollection<int> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("UserMapper.GetAllIds", Log))
            {
                // not cache, hardlinked
                toReturn = this.DataLoader.GetAllIds();
            }

            return toReturn;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>all users</returns>
        public ICollection<UserDTO> GetAll()
        {
            ICollection<UserDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("UserMapper.GetAllUsers", Log))
            {
                // get a list of all user Ids (not cache, hardlinked)
                ICollection<int> allUserIDs = this.DataLoader.GetAllIds();

				if (allUserIDs != null && allUserIDs.Any())
                {
                    // go through mapper so we take advantage of cache
                    var allMembers = this.GetDtos(allUserIDs);
                    toReturn = allMembers.OrderBy(x => x.DisplayName).ToArray();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get Users Online from Cache
        /// </summary>
        /// <returns>UsersOnlineDTO</returns>
        public UsersOnlineDTO GetUsersOnline()
        {
            UsersOnlineDTO toReturn = null;
            
            GetUsersOnlineDelegate usersOnlineDelegate = new GetUsersOnlineDelegate(this.DataLoader.GetUsersOnline);
            toReturn = this.CacheLoader.GetData(usersOnlineDelegate, new object[] { }, CacheConstants.USERS_ONLINE) as UsersOnlineDTO;

            return toReturn;
        }

        /// <summary>
        /// Updates the cached version of the online user status object
        /// </summary>
        /// <param name="user">user</param>
        /// <param name="inTrackingNumber">proposal Tracking number</param>
        public void UpdateUsersStatus(UserData user, string inTrackingNumber)
        {
            UsersOnlineDTO usersOnline = this.GetUsersOnline();

            usersOnline.UpdateUsersStatus(user, inTrackingNumber);

            this.cache.Remove(CacheConstants.USERS_ONLINE);
            this.cache.Add(CacheConstants.USERS_ONLINE, usersOnline, -1);
        }

        /// <summary>
        /// Return a user by looking them up with their AD information
        /// </summary>
        /// <param name="inUserData">users AD information</param>
        /// <returns>user found, null if not found</returns>
        virtual public UserDTO GetByUserData(UserData inUserData)
        {
            if (inUserData == null)
            {
                throw new ArgumentNullException(nameof(inUserData));
            }

            return this.GetByNtid(inUserData.Ntid);
        }

        /// <summary>
        /// Return the user dto for the actively logged in user
        /// </summary>
        /// <returns>user found, null if not found</returns>
        virtual public UserDTO GetActiveUser()
        {
            return this.GetByNtid(this.securityInformation.ActiveUserNTID);
        }

        /// <summary>
        /// Return a user by looking them up by their Ntid
        /// NOTE: This will retrieve the user from AD if they do not exist and add
        /// them into the system/cache.
        /// </summary>
        /// <param name="inNtid">users domain to locate them by</param>
        /// <returns>user found, null if not found</returns>
        virtual public UserDTO GetByNtid(string inNtid)
        {
            if (string.IsNullOrEmpty(inNtid))
            {
                throw new ArgumentNullException(inNtid);
            }

            UserDTO toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("UserMapper.GetByNtid", Log))
            {
                if (inNtid.Contains('\\'))
                {
                    // throw an exception ... not ok to include domain in userid string
                    throw new ArgumentException("Do not include domain in Ntid string - " + inNtid);
                }

                int outUserId = 0;
                if (!this.UserExists(inNtid, out outUserId))
                {
                    // add the user from AD
                    bool isGroup = this.activeDirectoryUtilities.IsGroup(inNtid);
                    UserData adUser = this.activeDirectoryUtilities.GetUserByQualifiedAccount(inNtid, isGroup);
                    if (adUser != null)
                    {
                        if (System.Transactions.Transaction.Current != null)
                        {
                            (this as IInternalDataMapper<UserDTO>).Save(new UserDTO
                            {
                                DisplayName = adUser.DisplayName,
                                EmailAddress = adUser.Email,
                                FirstName = adUser.FirstName,
                                LastName = adUser.LastName,
                                Ntid = adUser.Ntid,
                                PhoneNumber = adUser.Phone,
                                UpdateDate = DateTime.Now,
                                Id = -1,
                                Updateable = UpdateType.Upsert,
                                IsGroup = adUser.IsGroup,
                                IsUsPerson = adUser.IsUsPerson,
                                IsSubcontractor = adUser.IsUsPerson
                            });
                        }
                        else
                        {
                            // need to provide a transaction for the save
                            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(ConfigurationManager.AppSettings["TransactionTimeout"])) }))
                            {
                                (this as IInternalDataMapper<UserDTO>).Save(new UserDTO
                                {
                                    DisplayName = adUser.DisplayName,
                                    EmailAddress = adUser.Email,
                                    FirstName = adUser.FirstName,
                                    LastName = adUser.LastName,
                                    Ntid = adUser.Ntid,
                                    PhoneNumber = adUser.Phone,
                                    UpdateDate = DateTime.Now,
                                    Id = -1,
                                    Updateable = UpdateType.Upsert,
                                    IsGroup = adUser.IsGroup,
                                    IsUsPerson = adUser.IsUsPerson,
                                    IsSubcontractor = adUser.IsUsPerson
                                });
                                scope.Complete();
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                string key = CacheConstants.USER + inNtid;
                GetUserByNtidDelegate cacheDelegate = new GetUserByNtidDelegate(this.DataLoader.GetByNtid);
                toReturn = CacheLoader.GetData(cacheDelegate, new object[] { inNtid }, key) as UserDTO;
            }

            return toReturn;
        }

        /// <summary>
        /// See if a user exists
        /// </summary>
        /// <param name="inUserNtid">user Ntid to check for</param>
        /// <param name="outUserId">if the user exists, return the id</param>
        /// <returns>true/false user exists</returns>
        virtual public bool UserExists(string inUserNtid, out int outUserId)
        {
            string key = CacheConstants.USER_EXISTS + inUserNtid;

            if (this.cache.Contains(key))
            {
                int? userId = this.cache.GetData(key) as int?;

                outUserId = userId.HasValue ? userId.Value : 0;
            }
            else
            {
                // not found in cache .. load and stuff in cache
                this.DataLoader.UserExists(inUserNtid, out outUserId);

                if (outUserId != 0)
                {
                    this.cache.Add(key, outUserId, 30); // cache for 30 seconds
                }
            }

            return outUserId != 0;
        }

        /// <summary>
        /// remove cache keys associated with a save
        /// </summary>
        /// <param name="dto">the User that was saved</param>
        public override void ClearCacheKeys(UserDTO dto)
        {
            // no need to clear something that is null
            if (dto != null)
            {
                // remove cache
                CacheLoader.Remove(CacheConstants.USER + dto.Ntid);
                CacheLoader.Remove(CacheConstants.USER + dto.Id);
                CacheLoader.Remove(CacheConstants.USERS_ONLINE);
                this.cache.Remove(CacheConstants.USER_EXISTS + dto.Ntid);
            }
        }

        /// <summary>
        /// Get all groups
        /// </summary>
        /// <returns>all groups</returns>
        public ICollection<UserDTO> GetAllGroups()
        {
            ICollection<UserDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("UserMapper.GetAllGroups", Log))
            {
                // get a list of all group user Ids (not cache, hardlinked)
                ICollection<int> allGroupUserIDs = this.DataLoader.GetAllGroupIds();

				if (allGroupUserIDs != null && allGroupUserIDs.Any())
                {
                    // go through mapper so we take advantage of cache
                    var allMembers = this.GetDtos(allGroupUserIDs);
                    toReturn = allMembers.OrderBy(x => x.DisplayName).ToArray();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get User Dtos by User Ids
        /// </summary>
        /// <param name="userIds">collection of user ids</param>
        /// <returns>collection of user dtos</returns>
        virtual public ICollection<UserDTO> GetUserDtosByUserIds(ICollection<int> userIds)
        {
            ICollection<UserDTO> toReturn = null;
            // go through mapper so we take advantage of cache
            var allMembers = this.GetDtos(userIds);
            toReturn = allMembers.OrderBy(x => x.DisplayName).ToArray();
            return toReturn;
        }
    }
}
