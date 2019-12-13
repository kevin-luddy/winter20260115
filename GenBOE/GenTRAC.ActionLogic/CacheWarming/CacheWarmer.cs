// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.CacheWarming
{
    using System;
    using System.Diagnostics;
    using GenTRAC.DataBridge.Common.LoadersAndMappers;
    using IES.Common;

    /// <summary>
    /// Cache warmer
    /// </summary>
    public class CacheWarmer : ICacheWarmer
    {
        /// <summary>
        /// Logger to be used to log messages to file
        /// </summary>
        private Logger log = new Logger(typeof(CacheWarmer));

        /// <summary>
        /// The last time stamp used when forming logs for each cache warming mapper
        /// </summary>
        private long lastTimeStamp = 0;

        /// <summary>
        /// User cache warming mapper
        /// </summary>
        private ICacheWarmingMapper cacheWarmingUserMapper = null;

        /// <summary>
        /// Constructor for cache warming genTRAC application
        /// </summary>
        /// <param name="inCacheWarmingUserMapper">user cache warming mapper</param>
        public CacheWarmer(ICacheWarmingMapper inCacheWarmingUserMapper)
        {
            this.cacheWarmingUserMapper = inCacheWarmingUserMapper;
        }

        /// <summary>
        /// method to generate log message once a cache warming mapper is complete
        /// </summary>
        /// <param name="inTimer">timer to view the EllapsedMilliseconds in order to generate log</param>
        /// <param name="inCacheName">the name to use when generating log message</param>
        private void LogCacheWarming(Stopwatch inTimer, string inCacheName)
        {
            this.log.Info(inCacheName + " Cache Warming Complete: " + (inTimer.ElapsedMilliseconds - this.lastTimeStamp) + " milliseconds");

            this.lastTimeStamp = inTimer.ElapsedMilliseconds;
        }

        /// <summary>
        /// Perform all cache warming methods for all mappers
        /// NOTE: Catching generic exception here to avoid thread dying and taking app with it.  Everything is logged if there is a failure
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void DoWarmCache()
        {
            try
            {
                this.log.Info("Begin WarmCache");
                Stopwatch sw = new Stopwatch();
                sw.Start();

                this.cacheWarmingUserMapper.DoWarming();
                this.LogCacheWarming(sw, "User");

                this.lastTimeStamp = 0; // reset timer in case someone calls cache warm from UI

                this.log.Info("End WarmCache");
            }
            catch (Exception e)
            {
                this.log.Error(e, "Unable to complete cache warming!!!!!");
            }
        }
    }
}
