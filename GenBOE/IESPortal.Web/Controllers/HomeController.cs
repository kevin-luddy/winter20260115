// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IESPortal.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Web.Mvc;
    using IES.ActionLogic.Common;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;

    [IES.Common.Exceptions.HandleError]
    public class HomeController : Controller
    {
        /// <summary>
        /// The banner mediator
        /// </summary>
        private BannerMediator bannerMediator;

        /// <summary>
        /// The security information.
        /// </summary>
        private ISecurityInformation securityInformation;

        /// <summary>
        /// Offline Application loader
        /// </summary>
        private IOfflineApplicationLoader offlineApplicationLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="securityInformation">The security information.</param>
        /// <param name="bannerMediator">The banner mediator.</param>
        public HomeController(ISecurityInformation securityInformation, BannerMediator bannerMediator, IOfflineApplicationLoader offlineApplicationLoader)
        {
            this.securityInformation = securityInformation;
            this.bannerMediator = bannerMediator;
            this.offlineApplicationLoader = offlineApplicationLoader;
        }

        /// <summary>
        /// Returns the Admin index view.
        /// </summary>
        /// <returns>Returns the Admin index view.</returns>
        public ActionResult Index()
        {
            this.SetUrls();

            // show special BOE logo for Frank?
            ViewBag.showLogoBoeFrank = false;
            string frankBoeLogoVisibilityPercentage = ConfigurationManager.AppSettings["FrankBoeLogoVisibilityPercentage"];
            int percent;
            if (int.TryParse(frankBoeLogoVisibilityPercentage, out percent))
            {
                Random rand = new Random();
                int randomValue = rand.Next(1, 101);
                ViewBag.showLogoBoeFrank = randomValue <= percent && this.securityInformation.ActiveUserNTID.Equals("ffelicio");
            }

            ViewBag.ShowCarousel = this.securityInformation.ActiveUserNTID.Equals("ffelicio") || this.securityInformation.IsIESPortalAdminUser(this.securityInformation.ActiveUserNTID);

            return View();
        }

        /// <summary>
        /// Returns the Header for other Applications.
        /// </summary>
        /// <param name="active">The active.</param>
        /// <returns>The header only for applications.</returns>
        public ActionResult HeaderInternal(string active)
        {
            ViewBag.ActiveTab = active;
            ViewBag.IsAdmin = this.securityInformation.IsIESPortalAdminUser(this.securityInformation.ActiveUserNTID);

            this.SetUrls();
            return View();
        }

        /// <summary>
        /// Gets the banner for this application.
        /// </summary>
        /// <returns></returns>
        public ActionResult Banner(string active)
        {
            if (string.IsNullOrWhiteSpace(active))
            {
                throw new GenValidationException("The application was not specified when retrieving banners.");
            }

            ViewBag.IsMultipleApps = active.Contains(",");
            // Retrieve Banner(s) for this application
            ICollection<BannerModelView> banners = this.bannerMediator.GetAllActiveForApps(active);

            return View(banners);
        }

        public ActionResult AppOffline(string app)
        {
            if(string.IsNullOrEmpty(app))
            {
                throw new ArgumentNullException(nameof(app));
            }

            OfflineApplicationModelView appData = this.offlineApplicationLoader.GetByApplication(app);

            return View(appData);
        }

        /// <summary>
        /// Returns the Header for this IES Portal.
        /// </summary>
        /// <returns>The header.</returns>
        public ActionResult Header()
        {
            return View();
        }

        /// <summary>
        /// Gets the banner for this application.
        /// </summary>
        /// <returns></returns>
        public ActionResult BannerInternal(string active)
        {
            if (string.IsNullOrWhiteSpace(active))
            {
                throw new GenValidationException("The application was not specified when retrieving banners.");
            }

            ViewBag.IsMultipleApps = active.Contains(",");
            // Retrieve Banner(s) for this application
            ICollection<BannerModelView> banners = this.bannerMediator.GetAllActiveForApps(active);

            return View(banners);
        }

        private void SetUrls()
        {
            ViewBag.BOEUrl = ConfigurationManager.AppSettings["BOEUrl"];
            ViewBag.RPMUrl = ConfigurationManager.AppSettings["RPMUrl"];
            ViewBag.PTMUrl = ConfigurationManager.AppSettings["PTMUrl"];
            ViewBag.RDMUrl = ConfigurationManager.AppSettings["RDMUrl"];
            ViewBag.RDSBUrl = ConfigurationManager.AppSettings["RDSBUrl"];
            ViewBag.PPUrl = ConfigurationManager.AppSettings["PPUrl"];
            ViewBag.ACVUrl = ConfigurationManager.AppSettings["ACVUrl"];
            ViewBag.eEPPUrl = ConfigurationManager.AppSettings["EEPPUrl"];
            ViewBag.AdminUrl = ConfigurationManager.AppSettings["AdminUrl"];
        }
    }
}