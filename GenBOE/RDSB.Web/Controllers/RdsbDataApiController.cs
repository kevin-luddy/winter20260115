// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Web.Controllers
{
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Web.Http;
	using IES.Common;
	using IES.Common.OfficeUtilities;

	/// <summary>
	/// RDSB Data API Controller - used to serve up RDSB data for ACV (or other applications as needed)
	/// </summary>
	[AllowAnonymous]
	public class RdsbDataApiController : ApiController
	{
		#region Properties & Ctor

		/// <summary>
		/// Security Information
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private ISecurityInformation security;

		/// <summary>
		/// Token Handling
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private TokenHandling tokenHandler;

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("RdsbDataAPIController");

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="security">Security Information</param>
		/// <param name="tokenHandler">Token Handling</param>
		public RdsbDataApiController(ISecurityInformation security, TokenHandling tokenHandler)
		{
			this.security = security;
			this.tokenHandler = tokenHandler;
		}

		#endregion

		/// <summary>
		/// Check if an RDSB record exists for the given PTM Tracking Number
		/// </summary>
		/// <param name="ptmTrackingNumber">PTM Tracking Number</param>
		/// <returns>true if record exists, otherwise false</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ptmTrackingNumber")]
		public IESResponse<bool> DoesRdsbRecordExist(string ptmTrackingNumber)
		{
			IESResponse<bool> toReturn = new IESResponse<bool>();

			// TODO - check record

			return toReturn;
		}

		/// <summary>
		/// Export the RDSB Document
		/// </summary>
		/// <param name="ptmTrackingNumber">PTM Tracking Number</param>
		/// <param name="parentSectionNumber">Parent Section Number</param>
		/// <returns>RDSB Document in HTTP Response Message</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ptmTrackingNumber")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "parentSectionNumber")]
		public HttpResponseMessage ExportRdsbDocument(string ptmTrackingNumber, int parentSectionNumber)
		{
			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

			try
			{
				MemoryStream stream = new MemoryStream();
				string fileName = string.Empty;

				// TODO - export, name file

				stream.Position = 0; // We need to set this to return the file
				response.Content = new StreamContent(stream);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue(ExportFileDownloadBase.ContentType_DOCX);
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
				{
					FileName = fileName
				};
			}
			catch (Exception ex)
			{
				this.logger.Error(ex);
				response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}

			return response;
		}

		/// <summary>
		/// Is Service Alive?
		/// </summary>
		/// <returns>True/false</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public bool IsAlive()
		{
			bool result;

			try
			{
				// ToDo: add a DB grab, just to see if the DB is working.. To make the check more meaningful
				result = true;
			}
			catch
			{
				result = false;
			}

			return result;
		}
	}
}