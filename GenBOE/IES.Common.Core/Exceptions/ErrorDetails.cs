// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Exceptions
{
	using System.Text.Json;

	/// <summary>
	/// Error Details
	/// </summary>
	public class ErrorDetails
	{
		public int StatusCode { get; set; }
		public string Message { get; set; }
		public override string ToString()
		{
			return JsonSerializer.Serialize(this);
		}
	}
}
