// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Mediator for retrieving and editing Banners, includes caching them.
    /// </summary>
    public class BannerMediator
    {
        /// <summary>
        /// The banner loader
        /// </summary>
        private IBannerLoader bannerLoader;

        /// <summary>
        /// The banners
        /// </summary>
        private static IReadOnlyCollection<BannerModelView> banners;

        /// <summary>
        /// The banner lock
        /// </summary>
        private static object bannerLock = new object();

        /// <summary>
        /// Gets the banners.
        /// </summary>
        private IReadOnlyCollection<BannerModelView> Banners
        {
            get
            {
                if (banners == null)
                {
                    lock (bannerLock)
                    {
                        if (banners == null)
                        {
                            banners = this.bannerLoader.GetAll().ToList().AsReadOnly();
                        }
                    }
                }

                return banners;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BannerMediator"/> class.
        /// </summary>
        /// <param name="bannerLoader">The banner loader.</param>
        public BannerMediator(IBannerLoader bannerLoader)
        {
            this.bannerLoader = bannerLoader;
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void ClearCache()
        {
            lock (bannerLock)
            {
                banners = null;
            }
        }

        /// <summary>
        /// Saves the specified banner.
        /// </summary>
        /// <param name="banner">The banner.</param>
        public void Save(BannerModelView banner)
        {
            lock (bannerLock)
            {
                // Just reset banners to null so next Get retrieves them from DB
                // This weeds out weird edge cases of DB not matching the cached versions
                banners = null;

                this.bannerLoader.Save(banner);
            }
        }

        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The Banner with that Id or null if not found.</returns>
        public BannerModelView GetById(int id)
        {
            return this.Banners.FirstOrDefault(b => b.Id == id);
        }

        /// <summary>
        /// Gets all of the banners
        /// </summary>
        /// <returns>All Banners</returns>
        public IReadOnlyCollection<BannerModelView> GetAll()
        {
            return this.Banners;
        }

        /// <summary>
        /// Gets all for application.
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <returns>All of the banners for an Application</returns>
        public ICollection<BannerModelView> GetAllActiveForApps(string appName)
        {
            if (string.IsNullOrEmpty(appName))
            {
                throw new ArgumentNullException(nameof(appName));
            }

            DateTime now = DateTime.Now;

            string[] apps = new string[] { appName };
            if (appName.Contains(","))
            {
                apps = appName.Split(',');
            }

            // return banners for this app where the start date has already past and one of the following
            //      There is no end date
            //      This is maintenance banner
            //      This is warning banner and end date has not happened yet
            return this.Banners.Where(b => b.SelectedApps.Intersect(apps).Any() && b.StartDate < now && (b.HoursToShow <= 0 || (!b.TurnOffTicker || b.StartDate.AddHours(b.HoursToShow) > now))).ToList();
        }
    }
}
