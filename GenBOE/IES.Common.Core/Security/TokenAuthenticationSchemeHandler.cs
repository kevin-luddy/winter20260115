// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Security
{
	using System.IdentityModel.Tokens.Jwt;
	using System.Security.Claims;
	using System.Security.Principal;
	using System.Text.Encodings.Web;
	using IES.Common.Core.Configuration;
	using Microsoft.AspNetCore.Authentication;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.Options;
	using Microsoft.IdentityModel.Protocols;
	using Microsoft.IdentityModel.Protocols.OpenIdConnect;
	using Microsoft.IdentityModel.Tokens;

	/// <summary>
	/// IES Token Scheme Handler
	/// </summary>
	public class TokenAuthenticationSchemeHandler : AuthenticationHandler<TokenAuthenticationOptions>
	{
		/// <summary>
		/// Auth Domain
		/// </summary>
		private static readonly string AuthDomain = ConfigurationUtilities.GetAppSetting("Federation:Authority");

		/// <summary>
		/// Valid Issuer is Auth Domain without trailing slash
		/// </summary>
		private static readonly string ValidIssuer = AuthDomain.Substring(0, AuthDomain.Length - 1);

		/// <summary>
		/// Valid Audiences
		/// </summary>
		private static readonly ICollection<string> ValidAudiences = ConfigurationUtilities.GetAppSetting("Federation:ValidAudiences").Split(",");

		/// <summary>
		/// This is one of those things.. This URL is something that is a part of the OAuth2 (I'm guessing), so we just need to use it.
		/// </summary>
		private static readonly string metadataAddressForAuthDomain = AuthDomain + ".well-known/openid-configuration";

		/// <summary>
		/// static #ctor
		/// </summary>
		static TokenAuthenticationSchemeHandler()
		{
			ValidIssuer = AuthDomain.EndsWith("/") ? AuthDomain.Substring(0, AuthDomain.Length - 1) : AuthDomain;
		}

		/// <summary>
		/// #ctor
		/// </summary>
		/// <param name="options">The options</param>
		/// <param name="logger">The logger</param>
		/// <param name="encoder">Url Encoder</param>
		public TokenAuthenticationSchemeHandler(
			IOptionsMonitor<TokenAuthenticationOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder) : base(options, logger, encoder) 
		{
		}

		/// <summary>
		/// Handles Authentication Asynchronously
		/// </summary>
		/// <returns></returns>
		protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			// Check for [AllowAnonymous] decorator on the endpoint
			Endpoint endpoint = Context.GetEndpoint();

			if (endpoint != null)
			{
				IAllowAnonymous allowAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>();

				if (allowAnonymous != null)
				{
					return AuthenticateResult.NoResult();
				}
			}

			// Read the token from request headers
			string token = this.Request.Headers["IES_Authorization"];

			if (string.IsNullOrWhiteSpace(token))
			{
				// this may be pulled from the regular Authorization Header by Swagger
				token = this.Request.Headers["Authorization"];
			}

			// Authenticate the call, and pull out the user's ntid.
			string ntid = await GetNtidIfTokenIsValid(token, this.Logger);

			if (!string.IsNullOrWhiteSpace(ntid))
			{
				// If the session is valid, return success:
				// Set the current user to the NTID that is coming in.
				GenericIdentity identity = new(ntid);
				ClaimsPrincipal principal = new(identity);
				AuthenticationTicket ticket = new(principal, this.Scheme.Name);
				return AuthenticateResult.Success(ticket);
			}
			else
			{
				// If the token is missing or the session is invalid, return failure:
				return AuthenticateResult.Fail("Authentication failed");
			}
		}

		/// <summary>
		/// Validate a Token, retrieve NTID from it
		/// </summary>
		/// <param name="token">Token to validate</param>
		/// <returns>If the token is valid, this method returns user's NTID. </returns>
		public static async Task<string> GetNtidIfTokenIsValid(string token, ILogger logger)
		{
			_ = token ?? throw new ArgumentNullException(nameof(token));

			string userNtid = null;

			try
			{
				// We add "Bearer " to the token when we put it into the headers, so we then need to strip it out (in .Net Core this is done for us by our helpers)
				token = token.Replace("Bearer ", string.Empty);

				// This is "the way it's done" - that URL is something that must be a part of the OAuth2, just one of those things..
				IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddressForAuthDomain, new OpenIdConnectConfigurationRetriever());
				OpenIdConnectConfiguration openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);

				TokenValidationParameters validationParameters = new()
				{
					ValidateLifetime = true,
					ValidateAudience = true,
					ValidateIssuer = true,
					IssuerSigningKeys = openIdConfig.SigningKeys,
					ValidAudiences = ValidAudiences,
					ValidIssuer = ValidIssuer
				};

				// Validates the token first (throws if invalid). If valid, it searches all claims for the right one. Finally, the string is in the format of ntid@fully.qualitified.domain, so we strip out what we don't need.
				// ProPricer needs the id in the form of DOMAIN\ntid
				ClaimsPrincipal claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out SecurityToken validatedToken);
				string upn = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "lmco_upn")?.Value;
				if (!string.IsNullOrEmpty(upn))
				{
					string[] parts = upn.Split('@');
					if (parts.Length == 2)
					{
						userNtid = parts.First();
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error validating IES Token");
				userNtid = null;
			}

			return userNtid;
		}
	}
}
