// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2021 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Utilities
{
	using System;
	using System.Configuration;
	using System.IdentityModel.Tokens.Jwt;
	using System.Linq;
	using System.Security.Principal;
	using System.Threading;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Logging;
	using Microsoft.IdentityModel.Protocols;
	using Microsoft.IdentityModel.Protocols.OpenIdConnect;
	using Microsoft.IdentityModel.Tokens;

	/// <summary>
	/// This is a Token Handling class that will help us to communicate between IES API (.Net 4.7) and the ACV IES API (.Net Core 5.0)
	/// </summary>
	public class TokenHandling
	{
		/// <summary>
		/// Auth Domain
		/// </summary>
		private static readonly string AuthDomain = ConfigurationManager.AppSettings["oAuthDomain"];

		/// <summary>
		/// This is one of those things.. This URL is something that is a part of the OAuth2 (I'm guessing), so we just need to use it.
		/// </summary>
		private readonly string metadataAddressForAuthDomain = AuthDomain + ".well-known/openid-configuration";

		/// <summary>
		/// Logger
		/// </summary>
		private readonly ILogger logger;

		/// <summary>
		/// Context Accessor
		/// </summary>
		private readonly IHttpContextAccessor httpContextAccessor;

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="logger">Logger</param>
		/// <param name="httpContextAccessor">Http context accessor</param>
		public TokenHandling(ILogger<TokenHandling> logger, IHttpContextAccessor httpContextAccessor)
		{
			this.logger = logger;
			this.httpContextAccessor = httpContextAccessor;
		}

		/// <summary>
		/// Validate a Token, retrieve NTID from it
		/// </summary>
		/// <param name="token">Token to validate</param>
		/// <exception cref="UnauthorizedAccessException">If there are any issues parsing the token, we will throw an unauthorized exception</exception>
		/// <returns>If the token is valid, this method returns user's NTID. </returns>
		public string GetNtidIfTokenIsValid(string token)
		{
			_ = token ?? throw new ArgumentNullException(nameof(token));

			string userNtid = null;

			try
			{
				// We add "Bearer " to the token when we put it into the headers, so we then need to strip it out (in .Net Core this is done for us by our helpers)
				token = token.Replace("Bearer ", string.Empty);

				// This is "the way it's done" - that URL is something that must be a part of the OAuth2, just one of those things..
				IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddressForAuthDomain, new OpenIdConnectConfigurationRetriever());
				OpenIdConnectConfiguration openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;

				TokenValidationParameters validationParameters = new()
				{
					ValidateLifetime = true,
					ValidateAudience = false,
					ValidateIssuer = false,
					IssuerSigningKeys = openIdConfig.SigningKeys
				};

				// Validates the token first (throws if invalid). If valid, it searches all claims for the right one. Finally, the string is in the format of ntid@domain, so we strip out what we don't need.
				userNtid = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _).Claims.First(x => x.Type == "lmco_upn").Value.Split('@').First();
			}
			catch(Exception ex)
			{
				logger.LogError(ex, "Error validating Token");
				throw new UnauthorizedAccessException();
			}

			return userNtid;
		}

		/// <summary>
		/// Authenticates the user based on the token that is coming in from the headers
		///     The token is first validated, and if it is valid, then the user's NTID will be retrieved from it. 
		///     Finally, the NTID will be set into the System's Current Principal
		/// </summary>
		/// <returns>NTID from the token</returns>
		public string AuthenticateUserFromAuthorizationToken()
		{
			string token = this.httpContextAccessor.HttpContext.Request.Headers["IES_Authorization"];

			// Authenticate the call, and pull out the user's ntid.
			string ntid = GetNtidIfTokenIsValid(token);

			// Set the current user to the NTID that is coming in.
			GenericIdentity identity = new(ntid);
			System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(identity, Array.Empty<string>());

			return ntid;
		}
	}
}