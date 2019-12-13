// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using System.Web.Configuration;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

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
        /// mapper which returns user information
        /// </summary>
        private readonly IUserMapper userMapper;

        /// <summary>
        /// mediator which saves changed user information
        /// </summary>
        private readonly IUserMediator userMediator;

        /// <summary>
        /// The logger for this class.
        /// </summary>
        private readonly Logger logger = new Logger(typeof(ActiveDirectorySynchronization));

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="inADUtils">active directory utilities class</param>
        /// <param name="inUserMapper">mapper to obtain user information stored in gentrac</param>
        /// <param name="inUserMediator">mediator to save changed user information back to db</param>
        public ActiveDirectorySynchronization(IActiveDirectoryUtilities inADUtils,
            IUserMapper inUserMapper,
            IUserMediator inUserMediator)
        {
            this.adUtils = inADUtils;
            this.userMapper = inUserMapper;
            this.userMediator = inUserMediator;
        }

        /// <summary>
        /// Syncs account information for all users stored in genBOE with latest cooresponding data from Active Directory
        /// </summary>
        /// <returns>The collection of users who had their details altered</returns>
        public ICollection<UserDTO> SyncUpdateUsers()
        {
            ICollection<UserDTO> toReturn = new List<UserDTO>();

            // Get all stored genBOE users
            int usersNoPermission = 0;
            ICollection<UserDTO> allUsers = this.userMapper.GetAll();

            // first pull the users by AD groups for better performance
            List<UserData> allADGroupUsers = this.PullPtmUsers();
                       
            // Iterate over each genTRAC user
            foreach (UserDTO user in allUsers.Where(x => !string.IsNullOrEmpty(x.Ntid) && !string.IsNullOrEmpty(x.DisplayName) && !x.DisplayName.StartsWith("test")))
            {
                UserData activeDirectoryUser = allADGroupUsers.FirstOrDefault(u => u.Ntid == user.Ntid);
                // Get each genBOE user's AD data
                if (activeDirectoryUser == null)
                {
                    // this user doesn't have access anymore, still update them
                    usersNoPermission++;
                    activeDirectoryUser = this.adUtils.GetUserByQualifiedAccount(user.Ntid, false);
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
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
                        {
                            user.Updateable = UpdateType.Upsert;
                            user.DisplayName = activeDirectoryUser.DisplayName;
                            user.EmailAddress = activeDirectoryUser.Email;
                            user.FirstName = activeDirectoryUser.FirstName;
                            user.LastName = activeDirectoryUser.LastName;
                            user.PhoneNumber = activeDirectoryUser.Phone;
                            user.IsUsPerson = activeDirectoryUser.IsUsPerson;
                            user.IsSubcontractor = activeDirectoryUser.IsSubcontractor;

                            // Save the updated user
                            this.userMediator.SaveUser(user);

                            scope.Complete();
                        }

                        // Add the updated user to the return list
                        toReturn.Add(user);
                    }
                }
            }

            Console.WriteLine("There were " + usersNoPermission.ToString() + " users in DB that are not currently authorized.");
            this.logger.Info("There were " + usersNoPermission.ToString() + " users in DB that are not currently authorized.");

            return toReturn;
        }

        /// <summary>
        /// Pulls the PTM users by Authorization groups from app.config
        /// </summary>
        /// <returns>User data from AD for those groups.</returns>
        private List<UserData> PullPtmUsers()
        {
            ICollection<GroupData> groups = new List<GroupData>();

            string[] approvedADGroups = ConfigurationUtilities.GetAppSetting("PtmAuthGroups").Split(',');
            
            foreach (string group in approvedADGroups)
            {
                string[] splitGroup = group.Trim().Split('\\'); // split group into domain [0] and name [1] strings

                groups.Add(new GroupData() { Ntid = splitGroup[1] });
            }

            List<UserData> allADGroupUsers = new List<UserData>();
            foreach (GroupData group in groups)
            {
                ICollection<UserData> users = this.adUtils.GetAdGroupUsers(group.Ntid);
                if (users.Any())
                {
                    allADGroupUsers.AddRange(users);
                }
            }

            return allADGroupUsers;
        }
    }
}
