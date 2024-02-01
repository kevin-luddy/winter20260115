// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ActionLogic
{
    using GenTRAC.ActionLogic.CacheWarming;
    using GenTRAC.DataBridge.Common.LoadersAndMappers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test cache warming operations
    /// </summary>
    [TestClass]
    public class CacheWarmerTest
    {
        #region Setup

        /// <summary>
        /// User Mapper
        /// </summary>
        private Mock<ICacheWarmingMapper> userMapper = null;

        /// <summary>
        /// Creates System
        /// </summary>
        /// <returns>Cache Warmer</returns>
        private CacheWarmer CreateSystem()
        {
            this.userMapper = new Mock<ICacheWarmingMapper>();

            CacheWarmer warmer = new CacheWarmer(this.userMapper.Object);
            return warmer;
        }

        #endregion Setup

        /// <summary>
        /// Test the cache warmer
        /// </summary>
        [TestMethod]
        public void TestDoWarm()
        {
            CacheWarmer warmer = this.CreateSystem();

            warmer.DoWarmCache();

            this.userMapper.Verify(x => x.DoWarming(), Times.Once());
        }
    }
}
