// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
	using Newtonsoft.Json;

	/// <summary>
	/// Token
	/// </summary>
	public class Token
	{

		/// <summary>
		/// Gets or sets the Access Token
		/// </summary>
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the Token type (i.e. Bearer)
		/// </summary>
		[JsonProperty("token_type")]
		public string TokenType { get; set; }

		/// <summary>
		/// Gets or sets when the token expires
		/// </summary>
		[JsonProperty("expires_in")]
		public int ExpiresIn { get; set; }

		/// <summary>
		/// Gets or sets the Refresh Token
		/// </summary>
		[JsonProperty("refresh_token")]
		public string RefreshToken { get; set; }
	}
}
