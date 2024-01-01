// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Standard;
 
    /// <summary>
    /// User Data Loader Class
    /// </summary>
    public class UserLoader : DataLoader<UserDTO>, IUserLoader
    {
        /// <summary>
        /// Active Directory Utils
        /// </summary>
        private IActiveDirectoryUtilities adUtils = null;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="adUtils">IActiveDirectoryUtilities</param>
        public UserLoader(IActiveDirectoryUtilities adUtils)
        {
            this.Log = new Logger(typeof(UserLoader));
            this.adUtils = adUtils;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public UserLoader()
        {
            this.Log = new Logger(typeof(UserLoader));
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>All users from the DB.</returns>
        [DbQuery]
        public ICollection<UserDTO> GetAll()
        {
            ICollection<UserDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("UserLoader.GetAllUsers", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.genTRACUsers.Select(entity => new UserDTO()
                    {
                        Id = entity.UserID,
                        DisplayName = entity.DisplayName,
                        EmailAddress = entity.EmailAddress,
                        FirstName = entity.FirstName,
                        LastName = entity.LastName,
                        Ntid = entity.NTID,
                        UserType = entity.NTID.Contains(".") ? UserType.Group : UserType.User,
                        PhoneNumber = entity.PhoneNumber,
                        UpdateDate = entity.UpdateDT,
                        IsGroup = entity.IsGroup,
                        IsSubcontractor = entity.IsSubcontractor,
                        IsUsPerson = entity.IsUsPerson
                    }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get All User Ids
        /// </summary>
        /// <returns>All User Ids</returns>
        [DbQuery]
        public ICollection<int> GetAllIds()
        {
            ICollection<int> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("UserLoader.GetAllUserIds", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = (from s in dbModel.genTRACUsers
                                                  select s.UserID).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// See if a user exists
        /// </summary>
        /// <param name="inUserNtid">user ntid to check for</param>
        /// <param name="outUserId">if the group exists, return the id</param>
        /// <returns>true/false user exists</returns>
        [DbQuery]
        public bool UserExists(string inUserNtid, out int outUserId)
        {
            outUserId = 0;
            using (StopwatchTimer sw = new StopwatchTimer("UserLoader.UserExists", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    outUserId = (from c in dbModel.genTRACUsers
                        where c.NTID == inUserNtid
                        select c.UserID).FirstOrDefault();
                }
            }

            // a userid of 0 indicates it was not found
            return outUserId != 0;
        }

        /// <summary>
        /// Get a user by their database id
        /// </summary>
        /// <param name="ids">id in the database</param>
        /// <returns>user if found, null if not</returns>
        [DbQuery]
        public override ICollection<UserDTO> GetByIds(ICollection<int> ids)
        {
            ICollection<UserDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("UserLoader.GetUserById", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.genTRACUsers.Where(x => ids.Contains(x.UserID)).Select(entity => new UserDTO()
                    {
                        Id = entity.UserID,
                        DisplayName = entity.DisplayName,
                        EmailAddress = entity.EmailAddress,
                        FirstName = entity.FirstName,
                        LastName = entity.LastName,
                        Ntid = entity.NTID,
                        UserType = entity.NTID.Contains(".") ? UserType.Group : UserType.User,
                        PhoneNumber = entity.PhoneNumber,
                        UpdateDate = entity.UpdateDT,
                        IsGroup = entity.IsGroup,
                        IsSubcontractor = entity.IsSubcontractor,
                        IsUsPerson = entity.IsUsPerson
                    }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// GetUserDTOFromADUser static method to convert UserData (from AD) to userDTO 
        /// returns users only, no groups
        /// </summary>
        /// <param name="user">UserData</param>
        /// <returns>UserDTO</returns>
        private static UserDTO GetUserDTOFromADUser(UserData user)
        {
            UserDTO toReturn = new UserDTO();
            // don't want groups, just indiv. users
            if (user != null && user.IsGroup == false) 
            {
                toReturn.DisplayName = user.DisplayName;
                toReturn.EmailAddress = user.Email;
                toReturn.FirstName = user.FirstName;
                toReturn.LastName = user.LastName;
                toReturn.Ntid = user.Ntid;
                toReturn.PhoneNumber = user.Phone;
                toReturn.UserType = UserType.User;
                toReturn.IsUsPerson = user.IsUsPerson;
                toReturn.IsSubcontractor = user.IsSubcontractor;
            }

            return toReturn;
        }

        /// <summary>
        /// GetUserDTOsByADGroup
        /// </summary>
        /// <param name="inADGroup">inADGroup</param>
        /// <returns>user DTOs based on adgroup</returns>
        public ICollection<UserDTO> GetUserDTOsByADGroup(string inADGroup)
        {
            ICollection<UserDTO> toReturn = new Collection<UserDTO>();
            if (string.IsNullOrEmpty(inADGroup))
            {
                throw new ArgumentNullException(nameof(inADGroup));
            }

            ICollection<UserData> users = this.adUtils.GetAdGroupUsers(inADGroup);
            foreach (UserData user in users)
            {
                toReturn.Add(GetUserDTOFromADUser(user));
            }

            return toReturn;
        }

        /// <summary>
        /// Return a user by looking them up by their ntid
        /// </summary>
        /// <param name="inNtid">users ntid to locate them by</param>
        /// <returns>user found, null if not found</returns>
        [DbQuery]
        public UserDTO GetByNtid(string inNtid)
        {
            if (inNtid == null)
            {
                throw new ArgumentNullException(nameof(inNtid));
            }

            UserDTO toReturn = null;
            
            using (StopwatchTimer sw = new StopwatchTimer("UserLoader.GetUserByNtid", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    // return the first value or null if not found
                    toReturn = dbModel.genTRACUsers.Where(x => x.NTID == inNtid).Select(entity => new UserDTO()
                    {
                        Id = entity.UserID,
                        DisplayName = entity.DisplayName,
                        EmailAddress = entity.EmailAddress,
                        FirstName = entity.FirstName,
                        LastName = entity.LastName,
                        Ntid = entity.NTID,
                        UserType = entity.NTID.Contains(".") ? UserType.Group : UserType.User,
                        PhoneNumber = entity.PhoneNumber,
                        UpdateDate = entity.UpdateDT,
                        IsGroup = entity.IsGroup,
                        IsSubcontractor = entity.IsSubcontractor,
                        IsUsPerson = entity.IsUsPerson
                    }).FirstOrDefault();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Return a list of users who belong to a specific group
        /// </summary>
        /// <param name="inGroupId">group id to locate users for</param>
        /// <returns>user IDs found, null if not found</returns>
        public ICollection<int> GetIdsByGroupId(int inGroupId)
        {
            using (StopwatchTimer sw = new StopwatchTimer("UserLoader.GetUserIdsByGroupId", Log))
            {
                throw new NotImplementedException("Not yet implemented");
            }
        }

        /// <summary>
        /// Upserts the user
        /// </summary>
        /// <param name="dtoToUpsert">User to upsert</param>
        /// <returns>Id of the user that was upserted</returns>
        protected override int? Upsert(UserDTO dtoToUpsert)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("UserLoader.UpsertUser", Log))
            {
                if (dtoToUpsert != null)
                {
                    // save
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        toReturn = dbModel.upsertgenTRACUser(
                            dtoToUpsert.Id,
                            dtoToUpsert.UpdateDate,
                            dtoToUpsert.Ntid,
                            dtoToUpsert.DisplayName,
                            dtoToUpsert.EmailAddress,
                            dtoToUpsert.PhoneNumber,
                            dtoToUpsert.FirstName,
                            dtoToUpsert.LastName,
                            dtoToUpsert.IsGroup,
                            dtoToUpsert.IsUsPerson,
                            dtoToUpsert.IsSubcontractor
                            ).FirstOrDefault();
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="dtoToDelete">User to delete</param>
        /// <returns>Id of the deleted user</returns>
        protected override int? Delete(UserDTO dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("UserLoader.DeleteUser", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    dbModel.deletegenTRACUser(dtoToDelete.Id, dtoToDelete.UpdateDate);
                }

                toReturn = dtoToDelete.Id;
            }

            return toReturn;
        }

        /// <summary>
        /// Get All Group User Ids
        /// </summary>
        /// <returns>All Group User Ids</returns>
        [DbQuery]
        public ICollection<int> GetAllGroupIds()
        {
            ICollection<int> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("UserLoader.GetAllGroupIds", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = (from s in dbModel.genTRACUsers
                                                  where s.IsGroup
                                                  select s.UserID).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get a new instance of the UsersOnlineDTO
        /// </summary>
        /// <returns>instance of the UsersOnlineDTO</returns>
        public UsersOnlineDTO GetUsersOnline()
        {
            UsersOnlineDTO toReturn = new UsersOnlineDTO();

            return toReturn;
        }
    }
}
