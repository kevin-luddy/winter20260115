
namespace APTSPropricerApi.Common
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text.Encodings.Web;

    /// <summary>
    /// IES Token Scheme Handler
    /// </summary>
    public class TokenAuthenticationSchemeHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        /// <summary>
        /// This is one of those things.. This URL is something that is a part of the OAuth2, so we just need to use it.
        /// </summary>
        private readonly string metadataAddressForAuthDomain;

        /// <summary>
        /// #ctor
        /// </summary>
        /// <param name="options">The options</param>
        /// <param name="logger">The logger</param>
        /// <param name="encoder">Url Encoder</param>
        /// <param name="clock">System Clock</param>
        /// <param name="configuration">Configuration</param>
        public TokenAuthenticationSchemeHandler(
            IOptionsMonitor<TokenAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration) : base(options, logger, encoder, clock)
        {
            string authDomain = configuration.GetValue<string>("oAuthDomain");
            this.metadataAddressForAuthDomain = authDomain + ".well-known/openid-configuration";
        }

        /// <summary>
        /// Handles Authentication Asynchronously
        /// </summary>
        /// <returns></returns>
        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Read the token from request headers
            string token = this.Request.Headers["IES_Authorization"];

            // Authenticate the call, and pull out the user's ntid.
            string ntid = await GetNtidIfTokenIsValid(token);

            if (!string.IsNullOrWhiteSpace(ntid))
            {
                // If the session is valid, return success:
                // Set the current user to the NTID that is coming in.
                GenericIdentity identity = new (ntid);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, this.Scheme.Name);
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
        /// <exception cref="UnauthorizedAccessException">If there are any issues parsing the token, we will throw an unauthorized exception</exception>
        /// <returns>If the token is valid, this method returns user's NTID. </returns>
        public async Task<string> GetNtidIfTokenIsValid(string token)
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

                TokenValidationParameters validationParameters = new ()
                {
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    IssuerSigningKeys = openIdConfig.SigningKeys
                };

                // Validates the token first (throws if invalid). If valid, it searches all claims for the right one. Finally, the string is in the format of ntid@fully.qualitified.domain, so we strip out what we don't need.
                // ProPricer needs the id in the form of DOMAIN\ntid
                string upn = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _).Claims.FirstOrDefault(x => x.Type == "lmco_upn")?.Value;
                if (!string.IsNullOrEmpty(upn))
                {
                    string[] parts = upn.Split('@');
                    if (parts.Length == 2)
                    {
                        userNtid = parts.First();
                        string fullyQualifiedDomain = parts.Last();

                        if (!string.IsNullOrWhiteSpace(fullyQualifiedDomain))
                        {
                            string domain = fullyQualifiedDomain.Split('.').First().ToUpper();
                            userNtid = domain + @"\" + userNtid;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error validating IES Token");
                throw new UnauthorizedAccessException();
            }

            return userNtid;
        }
    }
}
