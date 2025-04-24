// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO.Request
{
	public interface IRequestDataLoader
	{
		/// <summary>
		/// Insert a request
		/// </summary>
		/// <param name="requestType">request type of record to be saved</param>
		/// <param name="ntid">ntid associated with request</param>
		/// <returns>bool indicating failure or success</returns>
		int? Insert(RequestType requestType, string ntid);

		/// <summary>
		/// Deletes a request
		/// </summary>
		/// <param name="requestType">request type of record to be saved</param>
		/// <param name="ntid">ntid associated with request</param>
		void Delete(RequestType requestType, string ntid);

		/// <summary>
		/// Deletes all requests
		/// </summary>
		void DeleteAll();
	}
}
