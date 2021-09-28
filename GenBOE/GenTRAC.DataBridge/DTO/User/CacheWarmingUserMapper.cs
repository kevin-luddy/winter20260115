// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenTRAC.DataBridge.Common.LoadersAndMappers;
    using IES.Common;

    /// <summary>
    /// cache warming user dto data mapper
    /// </summary>
    public class CacheWarmingUserMapper : UserMapper, ICacheWarmingMapper
    {
        /// <summary>
        /// Cache
        /// </summary>
        private ICache cache = null;

        /// <summary>
        /// Seconds to cache items in the cache.  -1 indicates infinite which will
        /// allow .net to garbage collect as needed.
        /// </summary>
        private int secondsToCacheItems = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inUserLoader">The User Loader</param>
        /// <param name="inCacheLoader">The Cache Loader</param>
        /// <param name="inSecurityInformation">The Security Information</param>
        /// <param name="inActiveDirectoryUtilities">The Active Directory Utils</param>
        /// <param name="inCache">The Cache</param>
        public CacheWarmingUserMapper(IUserLoader inUserLoader, ICacheDataLoader inCacheLoader, ISecurityInformation inSecurityInformation, IActiveDirectoryUtilities inActiveDirectoryUtilities, ICache inCache)
            : base(inUserLoader, inCacheLoader, inSecurityInformation, inActiveDirectoryUtilities, inCache)
        {
            this.cache = inCache;
        }

        /// <summary>
        /// Warm the capture cache
        /// </summary>
        public void DoWarming()
        {
            ICollection<UserDTO> users = this.DataLoader.GetAll();
            foreach (UserDTO user in users)
            {
                string key = CacheConstants.USER + user.Id;
                this.cache.Add(key, user, this.secondsToCacheItems);

                key = CacheConstants.USER + user.Ntid;
                this.cache.Add(key, user, this.secondsToCacheItems);
            }
        }
    }
}
