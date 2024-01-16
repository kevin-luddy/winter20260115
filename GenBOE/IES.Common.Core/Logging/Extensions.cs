// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2021 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Logging
{
	using System;
	using System.ComponentModel;
	using System.Net.Http;
	using System.Reflection;
	using IES.Common.Core.Constants;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Primitives;
	using Microsoft.Net.Http.Headers;

	/// <summary>
	/// Extensions
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Add Authorization Header to the HttpClient, if the token is provided
		/// </summary>
		/// <param name="client">HttpClient</param>
		/// <param name="token">Token</param>
		public static void AddAuthorizationHeader(this HttpClient client, string token)
		{
			if (!string.IsNullOrEmpty(token))
			{
				client.DefaultRequestHeaders.Remove(HeaderNames.Authorization);
				client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Constants.TOKEN_PREFIX + token);
			}
		}

		/// <summary>
		/// Add custom correlation id to the header of the request. Used for downstream service requests
		/// </summary>
		/// <param name="client">HttpClient</param>
		/// <param name="correlationId">Correlation Id</param>
		public static void AddCorrelationId(this HttpClient client, string correlationId)
		{
			if (!string.IsNullOrEmpty(correlationId))
			{
				client.DefaultRequestHeaders.Add(Constants.CORRELATION_HEADER_NAME, correlationId);
			}
		}

		/// <summary>
		/// Get correlation Id from HTTP request header
		/// </summary>
		/// <param name="context">HttpContext</param>
		public static string GetCorrelationId(this HttpContext context)
		{
			string correlationId = null;

			if (context.Request.Headers.TryGetValue(Constants.CORRELATION_HEADER_NAME, out StringValues id))
			{
				correlationId = id;
			}

			return correlationId;
		}

		/// <summary>
		/// Method to get Value of Enum from Description
		/// Seen here: https://stackoverflow.com/a/4367868
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="description"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static T GetValueFromDescription<T>(string description) where T : Enum
		{
			foreach (FieldInfo field in typeof(T).GetFields())
			{
				if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
					is DescriptionAttribute attribute)
				{
					if (attribute.Description == description)
					{
						return (T)field.GetValue(null);
					}
				}
				else
				{
					if (field.Name == description)
					{
						return (T)field.GetValue(null);
					}
				}
			}

			throw new ArgumentException("Not Found", nameof(description));
		}
	}
}
