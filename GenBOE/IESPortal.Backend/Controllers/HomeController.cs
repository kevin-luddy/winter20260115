// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IESPortal.Backend.Controllers
{
	using HtmlAgilityPack;
	using IES.ActionLogic.Core.Common;
	using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using IES.DataBridge.ModelViews;
	using IESPortal.Backend.Models;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	[Route("api/Home")]
	public class HomeController : IESController
	{
		/// <summary>
		/// The banner mediator
		/// </summary>
		private readonly BannerMediator bannerMediator;

		/// <summary>
		/// Active Directory Service
		/// </summary>
		private readonly IActiveDirectoryService activeDirectoryService;

		/// <summary>
		/// Configuration
		/// </summary>
		private readonly IConfiguration configuration;

		/// <summary>
		/// ctor
		/// </summary>
		public HomeController(ILogger<HomeController> logger, IConfiguration configuration, ISecurityInformation securityInformation, BannerMediator bannerMediator,
			IActiveDirectoryService activeDirectory)
			: base(logger, securityInformation, configuration)
		{
			this.configuration = configuration;
			this.bannerMediator = bannerMediator;
			this.activeDirectoryService = activeDirectory;
		}

		/// <summary>
		/// Get User Data
		/// </summary>
		/// <returns></returns>
		[HttpGet("[action]")]
		public UserData GetUserData()
		{
			UserData user = null;
			string ntId = this.securityInformation.ActiveUserNTID;
			if (!string.IsNullOrWhiteSpace(ntId))
			{
				user = this.activeDirectoryService.GetUserByQualifiedAccount(ntId, false);
			}

			return user;
		}

		/// <summary>
		/// Gets a user based on a provided ntid.
		/// </summary>
		/// <param name="ntid">Ntid.</param>
		/// <returns>User data found.</returns>
		[HttpGet("[action]")]
		public IESResponse<UserDataViewModel> GetUserLookupData(string ntid)
		{
			IESResponse<UserDataViewModel> result = new ();

			try
			{
				ICollection<UserData> matchingUsers = string.IsNullOrEmpty(ntid) ? new List<UserData>() : this.activeDirectoryService.SearchUsers(ntid, ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Exact);

				if (matchingUsers.Any())
				{
					UserDataViewModel userData = new()
					{
						UserAccount = ntid,
						UserFullName = matchingUsers.First().DisplayName,
						IsGroup = matchingUsers.First().IsGroup,
						WorkPhone = matchingUsers.First().Phone
					};

					result.Data = userData;
				}
				else
				{
					result.Data = null;
					result.Messages.Add($"Could not find a user with the NTID: {ntid}.");
				}

				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				result.Messages.Add($"Unknown error occurred returning user lookup data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Gets a user based on FormData provided.
		/// </summary>
		/// <param name="searchString">String to search</param>
		/// <param name="searchBy">Search By</param>
		/// <param name="matchType">Active Directory Match Type</param>
		/// <returns>User data found.</returns>
		[HttpPost("[action]")]
		public IESResponse<ICollection<UserDataViewModel>> GetUserLookupDataAsPost([FromForm] string searchString, [FromForm] ActiveDirectorySearchBy searchBy, [FromForm] ActiveDirectoryMatchType matchType)
		{
			IESResponse<ICollection<UserDataViewModel>> result = new();

			try
			{
				ICollection<UserData> matchingUsers = string.IsNullOrEmpty(searchString) ? new List<UserData>() : this.activeDirectoryService.SearchUsers(searchString, ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Exact);

				if (matchingUsers.Any())
				{

					result.Data = matchingUsers.Select(x => new UserDataViewModel
					{
						UserAccount = x.Ntid,
						UserFullName = x.DisplayName,
						IsGroup = x.IsGroup,
						WorkPhone = x.Phone
					}).ToList();
				}
				else
				{
					result.Data = null;
					result.Messages.Add($"Could not find a user with the Parameters => Search:  {searchString}, Search By: {searchBy.GetDescription()}, and Match Type: {matchType.GetDescription()} ");
				}

				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				result.Messages.Add($"Unknown error occurred returning user lookup data: {ex.Message}");
			}

			return result;
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
			List<HeaderLink> headerLinks = new()
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
					Name = "PROPRICER",
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
					Url = System.IO.Path.Combine(configuration["AdminUrl"])
				});
			}

			return headerLinks;
		}
	}
}