// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2021 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
	using System;
	using System.Collections.Generic;
	using System.IdentityModel.Tokens.Jwt;
	using System.Linq;
	using System.Security.Claims;
	using System.Text;
	using Microsoft.IdentityModel.Tokens;

	/// <summary>
	/// This is a Token Handling class that will help us to communicate between IES API (.Net 4.7) and the ACV IES API (.Net Core 5.0)
	/// </summary>
	public class TokenHandling
	{
		/// <summary>
		/// How many seconds is the token valid for
		/// </summary>
		private const int EXPIRATION_IN_SECONDS = 30;

		/// <summary>
		/// Security Key
		/// </summary>
		private const string KEY = "8tczr4ixjcq8jt43ijrgc8qfeil7w9wyl8irg3xlhwp7bvyr1jhan2voydt3fjz19vmm6kcybzhch6oqq0dk3omlnfziqrv67ev2y51wrzxkokno76ncrfz0gsleu334hud3ohdech32l3mku11vubw0nnhq90zpu83uty2nn7114v8rz3viibgwzlnrbgdx73nfwo0fe5or5jtnty516hoggiinkddd8roqw9pcq7yie08tej3yyw7onrkozi1g";

		/// <summary>
		/// Symmetric Security Key
		/// </summary>
		private static SymmetricSecurityKey SecurityKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));

		/// <summary>
		/// Generate a Token
		/// </summary>
		/// <returns>JWT Token for IES API Communication</returns>
		public string GenerateToken(string ntid)
		{
			JwtSecurityToken secToken = new JwtSecurityToken(
				signingCredentials: new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256),
				claims: new[]
				{
					new Claim(JwtRegisteredClaimNames.AuthTime, DateTime.Now.ToString()),
					new Claim("NTID", ntid)
				},
				expires: DateTime.UtcNow.AddMinutes(15)); // I'm setting this, as an extra layer of protection, but it is not sensitive enough for us to use it - it seems to be sensitive to hours, not minutes or seconds

			return new JwtSecurityTokenHandler().WriteToken(secToken);
		}

        /// <summary>
        /// Validate a Token. If the token is valid, this method returns user's NTID. If the token is invalid, it returns null.
        /// </summary>
        /// <param name="authToken">Token to validate</param>
        /// <returns>If the token is valid, this method returns user's NTID. If the token is invalid, it returns null.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public string GetNtidIfTokenIsValid(string authToken)
		{
			string userNtid = null;

			try
			{
				JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
				TokenValidationParameters validationParameters = new TokenValidationParameters()
				{
					ValidateLifetime = true,
					ValidateAudience = false,
					ValidateIssuer = false,
					IssuerSigningKey = SecurityKey
				};

				List<Claim> claims = tokenHandler.ValidateToken(authToken, validationParameters, out SecurityToken token).Claims.ToList();
				string claimValue = claims.First(x => x.Type == JwtRegisteredClaimNames.AuthTime).Value;
				DateTime claimExpDate = DateTime.Parse(claimValue).AddSeconds(EXPIRATION_IN_SECONDS);
				bool isValid = claimExpDate >= DateTime.Now;
				if (isValid)
				{
					userNtid = claims.First(x => x.Type == "NTID").Value;
				}
			}
			catch
			{
				userNtid = null;
			}

			return userNtid;
		}
	}
}