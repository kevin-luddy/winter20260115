// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IESPortal.Backend.Controllers
{
	using System;
	using System.Collections.Generic;
	using IES.ActionLogic.Common;
	using IES.Common.Core;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using IESPortal.Backend.Models;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;

	[Route("api/Home")]
	public class HomeController : IESController
	{
        /// <summary>
        /// The banner mediator
        /// </summary>
        private readonly BannerMediator bannerMediator;

		/// <summary>
		/// The security information.
		/// </summary>
		private ISecurityInformation securityInformation;

		/// <summary>
		/// Offline Application loader
		/// </summary>
		private readonly IOfflineApplicationLoader offlineApplicationLoader;

		/// <summary>
		/// Configuration
		/// </summary>
		private readonly IConfiguration configuration;

		/// <summary>
		/// Initializes a new instance of the <see cref="HomeController"/> class.
		/// </summary>
		/// <param name="securityInformation">The security information.</param>
		/// <param name="bannerMediator">The banner mediator.</param>
		public HomeController(ILogger<HomeController> logger, IConfiguration configuration, ISecurityInformation securityInformation, BannerMediator bannerMediator, IOfflineApplicationLoader offlineApplicationLoader) 
            : base(logger)
        {
			this.securityInformation = securityInformation;
            this.configuration = configuration;
            this.bannerMediator = bannerMediator;
            this.offlineApplicationLoader = offlineApplicationLoader;
        }

        /// <summary>
        /// Gets the banner for this application.
        /// </summary>
		/// <param name="active">Comma-separated list of application names</param>
        /// <returns>Banners for an application</returns>
        [HttpGet("[action]")]
		public ICollection<BannerModelView> GetBanners(string active)
        {
            if (string.IsNullOrWhiteSpace(active))
            {
                throw new GenValidationException("The application was not specified when retrieving banners.");
            }

            // Retrieve Banner(s) for this application
            ICollection<BannerModelView> banners = this.bannerMediator.GetAllActiveForApps(active);

            return banners;
        }

		/// <summary>
		/// Get Header Links
		/// </summary>
		/// <returns>Header Links</returns>
        [HttpGet("[action]")]
        public ICollection<HeaderLink> GetHeaderLinks()
        {
            List<HeaderLink> headerLinks = new List<HeaderLink>
            {
                new HeaderLink
                {
                    Name = "IES",
                    Url = configuration["IESUrl"]
                },
				new HeaderLink
				{
					Name = "PTM",
					Url = configuration["PTMUrl"]
				},
				new HeaderLink
				{
					Name = "BOE",
					Url = configuration["BOEUrl"]
				},
				new HeaderLink
				{
					Name = "ACV",
					Url = configuration["ACVUrl"]
				},
				new HeaderLink
				{
					Name = "PRO PRICER",
					Url = configuration["PPUrl"],
					IsNewWindow = true
				},
				new HeaderLink
				{
					Name = "NLF",
					Url = configuration["NLFUrl"]
				},
				new HeaderLink
				{
					Name = "RDM",
					Url = configuration["RDMUrl"]
				},
				new HeaderLink
				{
					Name = "RDSB",
					Url = configuration["RDSBUrl"]
				},
				new HeaderLink
				{
					Name = "RPM",
					Url = configuration["RPMUrl"]
				},
				new HeaderLink
				{
					Name = "eEPP",
					Url = configuration["EEPPUrl"]
				},
			};

			if (this.securityInformation.IsIESPortalAdminUser(this.securityInformation.ActiveUserNTID))
			{
				headerLinks.Add(new HeaderLink
				{
					Name = "Admin",
					Url = System.IO.Path.Combine(configuration["AdminUrl"], "Admin")
				});
			}

			return headerLinks;
        }

        [HttpGet("[action]")]
		public OfflineApplicationModelView GetAppOffline(string app)
        {
            if(string.IsNullOrEmpty(app))
            {
                throw new ArgumentNullException(nameof(app));
            }

            OfflineApplicationModelView appData = this.offlineApplicationLoader.GetByApplication(app);

            return appData;
        }
  //      private void SetUrls()
  //      {
  //          ViewBag.BOEUrl = ConfigurationManager.AppSettings["BOEUrl"];
  //          ViewBag.RPMUrl = ConfigurationManager.AppSettings["RPMUrl"];
  //          ViewBag.PTMUrl = ConfigurationManager.AppSettings["PTMUrl"];
  //          ViewBag.RDMUrl = ConfigurationManager.AppSettings["RDMUrl"];
  //          ViewBag.RDSBUrl = ConfigurationManager.AppSettings["RDSBUrl"];
  //          ViewBag.PPUrl = ConfigurationManager.AppSettings["PPUrl"];
  //          ViewBag.ACVUrl = ConfigurationManager.AppSettings["ACVUrl"];
		//	ViewBag.NLFUrl = ConfigurationManager.AppSettings["NLFUrl"];
		//	ViewBag.eEPPUrl = ConfigurationManager.AppSettings["EEPPUrl"];
  //          ViewBag.AdminUrl = ConfigurationManager.AppSettings["AdminUrl"];
		//}
    }
}