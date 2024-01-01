// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Common.Security
{
    using System.Collections.Generic;
    using GenTRAC.DataBridge.DTO;
    using IES.Standard;

    /// <summary>
    /// The security mapper will load in the security data from the database
    /// based on the user currently logged in.
    /// </summary>
    public class SecurityMapper : ISecurityMapper
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger log;

        /// <summary>
        /// Loader to load in all permissions for a user in the system
        /// </summary>
        private ISecurityUserAuthorizationsDataLoader securityUserAuthorizationsDataLoader;

        /// <summary>
        /// Security information, primarily user's Ntid (domain/Ntid)
        /// </summary>
        private ISecurityInformation securityInformation;

        /// <summary>
        /// User data mapper
        /// </summary>
        private IUserMapper userMapper;

        /// <summary>
        /// Lock object
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// declare private instance so we can cache returns about users existance for a brief period of time
        /// </summary>
        private readonly ICache cache;

        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        /// <param name="inSecurityUserAuthorizationsDataLoader">The data loader to use for obtaining database role permissions</param>
        /// <param name="inSecurityInformation">Information about the currently logged in user</param>
        /// <param name="inUserMapper">User Dto Data Mapper.</param>
        public SecurityMapper(ISecurityUserAuthorizationsDataLoader inSecurityUserAuthorizationsDataLoader,
                                ISecurityInformation inSecurityInformation,
                                IUserMapper inUserMapper,
                                ILogger logger,
                                ICache cache)
        {
            this.securityUserAuthorizationsDataLoader = inSecurityUserAuthorizationsDataLoader;
            this.securityInformation = inSecurityInformation;
            this.userMapper = inUserMapper;
            this.log = logger;
            this.cache = cache;
        }

        /// <summary>
        /// Get the roles for the currently logged in user
        /// </summary>
        /// <returns>A collection of permissions based on the parameters passed in</returns>
        public IReadOnlyCollection<SecurityPermissionsResponse> GetRolesForLoggedInUser()
        {
            this.log.Debug("BEGIN Get permissions for logged in user for " + this.securityInformation.ActiveUserNTID);

            IReadOnlyCollection<SecurityPermissionsResponse> rolesForUser = this.GetRolesForUser(this.securityInformation.ActiveUserNTID);

            this.log.Debug("END Get permissions for logged in user for " + this.securityInformation.ActiveUserNTID);

            return rolesForUser;
        }

        /// <summary>
        /// Get the roles for a specific user Id
        /// </summary>
        /// <param name="inUserNtid">The user's NT ID</param>
        /// <returns>A collection of permissions based on the parameters passed in</returns>
        public IReadOnlyCollection<SecurityPermissionsResponse> GetRolesForUser(string inUserNtid)
        {
            lock (this.lockObj)
            {
                this.log.Debug("BEGIN Get permissions for specific user for " + inUserNtid);
                UserDTO user = this.userMapper.GetByNtid(inUserNtid);

                string key = CacheConstants.ROLES_FOR_USER + inUserNtid;
                IReadOnlyCollection<SecurityPermissionsResponse> rolesForUser;

                // if this is a save action, permissions should be cleared from cache
                bool isSaveAction = false;
                // TODO TIW
                //if (HttpContext.Current != null && HttpContext.Current.Items.Contains(CacheConstants.SAVE_PERMISSIONS_ACTION))
                //{
                //    isSaveAction = true;
                //}

                if (this.cache.Contains(key))
                {
                    rolesForUser = this.cache.GetData(key) as IReadOnlyCollection<SecurityPermissionsResponse>;
                }
                else
                {
                    rolesForUser = this.securityUserAuthorizationsDataLoader.GetPermissionsForUser(user);

                    if (!isSaveAction)
                    {
                        this.cache.Add(key, rolesForUser, 3); // cache for 3 seconds
                    }
                }

                if (isSaveAction)
                {
                    // clear memory cache on a save
                    this.cache.Remove(key);
                }

                return rolesForUser;
            }
        }
    }
}