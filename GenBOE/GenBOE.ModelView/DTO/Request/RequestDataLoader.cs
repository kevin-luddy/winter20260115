// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO.Request
{
	using GenBOE.Models;
	using IES.Common;
	using System;
	using System.Data.Entity.Core;
	using System.Data.Entity.Core.Objects;
	using System.Data.SqlClient;

	public class RequestDataLoader : IRequestDataLoader
	{
		/// <summary>
		/// Logger
		/// </summary>
		protected Logger log { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public RequestDataLoader()
		{
			this.log = new Logger(typeof(RequestDataLoader));
		}

		/// <summary>
		/// Deletes a request
		/// </summary>
		/// <param name="requestType">request of record to be deleted</param>
		/// <param name="ntid">ntid associated with request</param>
		public void Delete(RequestType requestType, string ntid)
		{
			if (string.IsNullOrWhiteSpace(ntid))
			{
				throw new ArgumentNullException(nameof(ntid));
			}
			try
			{
				using (StopwatchTimer sw = new StopwatchTimer(log))
				{
					using (GenBoeEntities dbModel = new GenBoeEntities())
					{
						dbModel.deleteRequest((int)requestType, ntid);
					}
				}
			}
			catch (EntityCommandExecutionException ex)
			{
				log.Error(ex, $"An {ex.InnerException} occurred in {ex.Source} while deleting requestType: {requestType}, Ntid: {ntid}");
			}
			catch (SqlException sqlEx)
			{
				log.Error(sqlEx, $"An exception occurred in {sqlEx.Procedure} while deleting requestType: {requestType}, Ntid: {ntid} and has the following message: " + sqlEx.Message + "/n");
			}
			catch (Exception e)
			{
				log.Error(e, $"an unhandled exception occurred while saving  requestType: {requestType}, Ntid: {ntid}");
				throw;
			}
		}

		/// <summary>
		/// Deletes all requests
		/// </summary>
		public void DeleteAll()
		{
			using (StopwatchTimer sw = new StopwatchTimer(log))
			{
				using (GenBoeEntities dbModel = new GenBoeEntities())
				{
					dbModel.deleteAllRequests();
				}
			}
		}

		/// <summary>
		/// Insert a request
		/// </summary>
		/// <param name="requestType">request type of record to be saved</param>
		/// <param name="ntid">ntid associated with request</param>
		/// <returns>bool indicating failure or success</returns>
		public int? Insert(RequestType requestType, string ntid)
		{
			int? result = null;

			if (string.IsNullOrWhiteSpace(ntid))
			{
				throw new ArgumentNullException(nameof(ntid));
			}

			try
			{
				using (StopwatchTimer sw = new StopwatchTimer(log))
				{
					using (GenBoeEntities dbModel = new GenBoeEntities())
					{
						ObjectParameter newRequestIdParam = new ObjectParameter("NewRequestID", typeof(int));
						result = Convert.ToInt32(dbModel.insertRequest((int)requestType, ntid, newRequestIdParam));
						if (newRequestIdParam.Value != DBNull.Value)
						{
							result = (int)newRequestIdParam.Value;
						}
					}
				}
			}
			catch (EntityCommandExecutionException ex)
			{
				log.Error(ex, $"An {ex.InnerException} occurred in {ex.Source} while saving requestType: {requestType}, Ntid: {ntid}");
			}
			catch (SqlException sqlEx)
			{
				log.Error(sqlEx, $"An exception occurred in {sqlEx.Procedure} while saving requestType: {requestType}, Ntid: {ntid} and has the following message: " + sqlEx.Message + "/n");
			}
			catch (Exception e)
			{
				log.Error(e, $"an unhandled exception occurred while saving  requestType: {requestType}, Ntid: {ntid}");
				throw;
			}

			return result;
		}
	}
}
