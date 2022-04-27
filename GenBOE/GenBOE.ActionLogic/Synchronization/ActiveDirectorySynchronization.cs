// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using IES.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using System.Collections.ObjectModel;

    /// <summary>
    /// class to sync user details from ActiveDirectory to the local db tables containing this information
    /// </summary>
    public class ActiveDirectorySynchronization
    {
        /// <summary>
        /// active directory utility class
        /// </summary>
        private readonly IActiveDirectoryUtilities adUtils;

        /// <summary>
        /// loader which returns user information
        /// </summary>
        private readonly IUserDTODataLoader userDataLoader;

        /// <summary>
        /// The logger for this class.
        /// </summary>
        private readonly Logger logger = new Logger(typeof(ActiveDirectorySynchronization));

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="inADUtils">active directory utilities class</param>
        /// <param name="inUserDataLoader">user data loader</param>
        public ActiveDirectorySynchronization(IActiveDirectoryUtilities inADUtils,
            IUserDTODataLoader inUserDataLoader)
        {
            this.adUtils = inADUtils;
            this.userDataLoader = inUserDataLoader;
        }

        /// <summary>
        /// Syncs account information for all users stored in genBOE with latest cooresponding data from Active Directory
        /// </summary>
        /// <returns>The collection of users who had their details altered</returns>
        public ICollection<UserDTO> SyncUpdateUsers()
        {
            ICollection<UserDTO> toReturn = new List<UserDTO>();
            int usersNoPermission = 0;
            // Get all stored genBOE users
            Collection<UserDTO> allUsers = this.userDataLoader.GetAllUsers();

            // first pull the users by AD groups for better performance
            ICollection<GroupData> groups = adUtils.GetAuthorizationGroupsFromWebConfig();
            List<UserData> allADGroupUsers = new List<UserData>();
            foreach (GroupData group in groups)
            {
                ICollection<UserData> users = adUtils.GetAdGroupUsers(group.Ntid);
                if (users.Any())
                {
                    allADGroupUsers.AddRange(users);
                }
            }

            // get the list of users we DO NOT want to update
            ICollection<string> doNotUpdateUser = adUtils.GetNoADSynceAccountsFromWebConfig();
            foreach (string user in doNotUpdateUser)
            {
                UserDTO dto = allUsers.FirstOrDefault(x => x.NTID == user);
                
                if (dto != null)
                {
                    Console.WriteLine($"Removing user {user} from update consideration.");
                    this.logger.Info($"Removing user {user} from update consideration.");
                    allUsers.Remove(dto);
                }
            }

            // Iterate over each genBOE user
            foreach (UserDTO user in allUsers.Where(x=>!string.IsNullOrEmpty(x.NTID)))
            {
                UserData activeDirectoryUser = allADGroupUsers.FirstOrDefault(u => u.Ntid == user.NTID);
                // Get each genBOE user's AD data
                if (activeDirectoryUser == null)
                {
                    // this user doesn't have access anymore, still update them
                    usersNoPermission++;
                    activeDirectoryUser = this.adUtils.GetUserByQualifiedAccount(user.NTID, false);
                }

                // If the user exists in AD, continue
                if (activeDirectoryUser != null)
                {
                    // If any profile data has been updated in AD, we'll re-sync all of the profile data
                    if (user.DisplayName != activeDirectoryUser.DisplayName ||
                        user.EmailAddress != activeDirectoryUser.Email ||
                        user.FirstName != activeDirectoryUser.FirstName ||
                        user.LastName != activeDirectoryUser.LastName ||
                        user.PhoneNumber != activeDirectoryUser.Phone ||
                        user.IsUsPerson != activeDirectoryUser.IsUsPerson ||
                        user.IsSubcontractor != activeDirectoryUser.IsSubcontractor)
                    {
                        // Increase the transaction by 2 minutes here because loading from AD can take awhile if cache isn't working perfectly
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                        {
                            user.DisplayName = activeDirectoryUser.DisplayName;
                            user.EmailAddress = activeDirectoryUser.Email;
                            user.FirstName = activeDirectoryUser.FirstName;
                            user.LastName = activeDirectoryUser.LastName;
                            user.PhoneNumber = activeDirectoryUser.Phone;
                            user.IsUsPerson = activeDirectoryUser.IsUsPerson;
                            user.IsSubcontractor = activeDirectoryUser.IsSubcontractor;

                            // Save the updated user
                            this.userDataLoader.SaveUser(user);

                            scope.Complete();
                        }

                        // Add the updated user to the return list
                        toReturn.Add(user);
                    }
                }
            }

            Console.WriteLine($"There were {usersNoPermission} users in DB that are not currently authorized.");
            this.logger.Info($"There were {usersNoPermission} users in DB that are not currently authorized.");

            return toReturn;
        }
    }
}
