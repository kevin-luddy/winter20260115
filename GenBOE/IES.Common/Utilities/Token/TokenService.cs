// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
	using System;
	using System.Threading.Tasks;
	using System.Configuration;
	using System.Net.Http;
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using System.Net.Http.Headers;
	using IES.Common.Exceptions;

	public class TokenService : ITokenService, IDisposable
	{
		/// <summary>
		/// Internal Cache for tokens
		/// </summary>
		internal readonly ICache cache;

		/// <summary>
		/// Whether the Token Service has been disposed
		/// </summary>
		private bool isDisposed;

		/// <summary>
		/// Cache key for Token
		/// </summary>
		private const string CACHE_KEY_TOKEN = "Token";

		/// <summary>
		/// OAuth Client Id
		/// </summary>
		private readonly string clientId;

		/// <summary>
		/// OAuth Secret
		/// </summary>
		private readonly string clientSecret;

		/// <summary>
		/// Logger for Token Service
		/// </summary>
		private readonly Logger _log = new Logger(typeof(TokenService));

		/// <summary>
		/// Http Client for Token Service
		/// </summary>
		private readonly HttpClient _client = new HttpClient();

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="memoryCache">Memory Cache</param>
		public TokenService(ICache memoryCache)
		{
			this.cache = memoryCache;
			string authority = ConfigurationManager.AppSettings["oAuthDomain"];
			this.clientId = ConfigurationManager.AppSettings["oAuthIESClientId"];
			this.clientSecret = ConfigurationManager.AppSettings["oAuthIESClientSecret"];
			this._client.BaseAddress = new Uri(authority);
			_client.DefaultRequestHeaders.Add("cache-control", "no-cache");
		}

		/// <summary>
		/// Get token
		/// </summary>
		/// <returns>Token</returns>
		public async Task<Token> GetToken()
		{
			Token token = null;

			if (!this.cache.Contains(CACHE_KEY_TOKEN))
			{
				token = await this.CreateToken();
				this.cache.AddAbsolute(CACHE_KEY_TOKEN, token, 5 * 60);
			}
			else
			{
				token = this.cache.GetData(CACHE_KEY_TOKEN) as Token;
			}

			return token;
		}

		/// <summary>
		/// Creates the token
		/// </summary>
		/// <returns>New token</returns>
		private async Task<Token> CreateToken()
		{
			Token token = null;

			List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
			postData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
			postData.Add(new KeyValuePair<string, string>("client_id", clientId));
			postData.Add(new KeyValuePair<string, string>("client_secret", clientSecret));

			HttpContent content = new FormUrlEncodedContent(postData);
			content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			HttpResponseMessage response = _client.PostAsync("/as/token.oauth2", content).Result;

			if (response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();

				token = JsonConvert.DeserializeObject<Token>(responseBody);
			}
			else
			{
				this._log.Error("Error creating new Token.");
				throw new GeneralAppException("Error creating OAuth Token");
			}

			return token;
		}

		/// <summary>
		/// Dispose of Token
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// The bulk of the clean-up code is implemented in Dispose(bool)
		/// </summary>
		/// <param name="disposing">Dispose flag</param>
		protected virtual void Dispose(bool disposing)
		{
			if (this.isDisposed)
			{
				return;
			}

			if (disposing)
			{
				// free managed resources
				this._client.Dispose();
			}

			this.isDisposed = true;
		}
	}
}

