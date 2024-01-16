// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Net;
	using System.Text;
	using System.Web;
	using Exceptions;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Utilities;
	using Microsoft.AspNetCore.Authentication.Negotiate;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.Controllers;
	using Microsoft.AspNetCore.Mvc.Filters;
	using Microsoft.Extensions.Logging;

	[ApiController, Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
	public class IESController : ControllerBase, IActionFilter
	{
		/// <summary>
		/// The logger.
		/// </summary>
		protected ILogger log;

		/// <summary>
		/// Initializes a new instance of the <see cref="IESController"/> class.
		/// </summary>
		public IESController(ILogger logger)
		{
			log = logger;
		}

		/// <summary>
		/// Convert HTML to text.
		/// </summary>
		/// <param name="html">HTML</param>
		/// <param name="returnConvertedText">if false, the converted text will not be returned.  if null or true, the converted text will be returned.</param>
		/// <returns>Text</returns>
		[HttpPost("[action]")]
		public JsonResult ConvertHtmlToText(string html, bool? returnConvertedText)
		{
			string htmlDecoded = HttpUtility.UrlDecode(html);
			string text = GenBOEUtilities.ConvertHtmlToText(htmlDecoded);

			return new JsonResult(new
			{
				Text = returnConvertedText.HasValue && !returnConvertedText.Value ? string.Empty : text,  // return converted text by default (unless explicitly disabled)
				Length = text.Length,
				WhiteSpace = string.IsNullOrWhiteSpace(text)
			});
		}

		/// <summary>
		/// Scrub rich-text markup to remove unwanted items and convert image tag src-attribute route values to base-64.
		/// </summary>
		/// <param name="html">Rich-text markup</param>
		/// <returns>Scrubbed rich-text</returns>
		[HttpPost("[action]")]
		public ContentResult PreProcessRichTextPaste(string html)
		{
			string htmlDecoded = HttpUtility.UrlDecode(html);

			ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();

			string cleanedText = DoAnExtraScrubbingForRTEInput(htmlDecoded);

			string scrubbedHtml = HtmlAgilityPackUtilities.ScrubRichTextForSave(cleanedText, ConvertImageSrcAttribute, validationMessages);

			return new ContentResult
			{
				Content = scrubbedHtml,
				ContentType = "text/html"
			};
		}

		/// <summary>
		/// Convert a URL-based img-tag src attribute value to a base-64-based attribute value.
		/// </summary>
		/// <param name="src">Original src attribute value</param>
		/// <returns>Base-64 converted src attribute value</returns>
		private string ConvertImageSrcAttribute(string src)
		{
			string resultingSrc = src;

			Uri uri;

			if (src.StartsWith("data:"))
			{
				resultingSrc = src.Replace(' ', '+');  // replace spaces with plus-sign to ensure valid base-64 syntax
			}
			else if (Uri.TryCreate(src, UriKind.RelativeOrAbsolute, out uri))
			{
				try
				{
					// retrieve the URL-referenced image directly
					byte[] imageBytes = GenImageUtilities.DownloadImage(uri, ConfigurationUtilities.GetAppSetting("LMIProxy"));

					string base64String = GenImageUtilities.ConvertImageToBase64String(imageBytes);
					string mimeType = GenImageUtilities.GetImageMIMETypeFromExtension(uri.AbsolutePath);

					// e.g. "data:image/jpeg;base64,<<base64encodeddata>>"
					resultingSrc = $"data:{mimeType};base64,{base64String}";
				}
				catch (WebException webx)
				{
					log.LogWarning($"Could not retrieve image at \"{src}\".");

					StringBuilder sb = new StringBuilder();
					sb.AppendFormat("Message = {0}", webx.Message);
					sb.AppendFormat(", Status = {0}", webx.Status.ToString());
					foreach (DictionaryEntry entry in webx.Data)
					{
						sb.AppendFormat(", {0} = {1}", entry.Key, entry.Value);
					}

					string msg = sb.ToString();

					log.LogError(webx, msg);
				}
			}

			return resultingSrc;
		}

		/// <summary>
		/// Does extra scrubbing in case HTML garbage made it through. it removes scripts and comments (stuff <script> ... </script> and <!-- ... -->
		/// </summary>
		/// <param name="originalScrubbedText">Original text to be cleaned</param>
		/// <returns>Cleaned text</returns>
		[HttpPost("[action]")]
		public string DoAnExtraScrubbingForRTEInput(string originalScrubbedText)
		{
			string result = originalScrubbedText;

			result = RemoveTextBasedOnStartAndEndString(result, "<script", "</script>");
			result = RemoveTextBasedOnStartAndEndString(result, "<!--", "-->");

			return result;
		}

		/// <summary>
		/// Removes text between starting and ending strings
		/// </summary>
		/// <param name="originalScrubbedText">original text to be cleaned</param>
		/// <param name="startingString">Starting string</param>
		/// <param name="endingString">Ending string</param>
		/// <returns>Cleaned string</returns>
		private string RemoveTextBasedOnStartAndEndString(string originalScrubbedText, string startingString, string endingString)
		{
			string result = originalScrubbedText;

			while (true)
			{
				int startIndex = result.ToUpper().IndexOf(startingString.ToUpper(), StringComparison.Ordinal);

				if (startIndex == -1)
				{
					break;
				}

				int endIndex = result.ToUpper().IndexOf(endingString.ToUpper(), startIndex, StringComparison.Ordinal);

				if (endIndex == -1)
				{
					break;
				}

				result = result.Remove(startIndex, endIndex - startIndex + endingString.Length);
			}

			return result;
		}

		/// <summary>
		/// This method can be used to stream a text file to the browser that contains a series of error messages separated
		/// by newlines.  This is a quick and easy way to alert the user that there was a problem (e.g. an exception thrown)
		/// during file download processing.
		/// </summary>
		/// <param name="errorMessages">List of error messages</param>
		/// <returns>Text file</returns>
		protected IActionResult CreateTextFileWithErrorMessage(params string[] errorMessages)
		{
			Response.Headers.Clear();
			Response.ContentType = "text/plain";
			Response.Headers.Add("Content-Disposition", new Microsoft.Extensions.Primitives.StringValues("attachment;filename=error.txt"));

			byte[] newline = Encoding.ASCII.GetBytes("\r\n");

			if (errorMessages != null)
			{
				foreach (string errorMessage in errorMessages)
				{
					byte[] errorContent = Encoding.ASCII.GetBytes(errorMessage);
					Response.Body.Write(errorContent, 0, errorContent.Length);
					Response.Body.Write(newline, 0, newline.Length);
				}
			}

			Response.Body.Flush();

			return new EmptyResult();
		}

		/// <summary>
		/// This method can be used to stream a text file to the browser that contains a series of error messages separated
		/// by newlines.  This is a quick and easy way to alert the user that there was a problem (e.g. an exception thrown)
		/// during file download processing.
		/// </summary>
		/// <param name="ex">Exception</param>
		/// <returns>Text file</returns>
		protected IActionResult CreateTextFileWithErrorMessage(Exception ex)
		{
			if (ex == null)
			{
				return new EmptyResult();
			}
			else if (ConfigurationUtilities.GetAppSetting<bool>("LocalDebug"))
			{
				return CreateTextFileWithErrorMessage(ex.ToDisplayString());
			}
			else
			{
				string supportLink = CommonUtilities.ServiceCentralLink();

				return CreateTextFileWithErrorMessage(
					$"An error has occurred.  This might be the result of invalid data.  If the data is valid, and the error persists, please create a ticket with IES Helpdesk at {supportLink}.");
			}
		}

		/// <summary>
		/// Start logging/timing an action 
		/// </summary>
		/// <param name="logger">The logger</param>
		/// <param name="functionName">The function name being logged</param>
		/// <returns>The running stopwatch</returns>
		[NonAction]
		private void StartAction(string functionName)
		{
			if (functionName == null)
			{
				throw new ArgumentNullException(nameof(functionName));
			}

			StopwatchTimer sw = new StopwatchTimer(functionName, log);
			HttpContext.Items["Stopwatch"] = sw;
		}

		/// <summary>
		/// Finalizes an action logging the performance
		/// </summary>
		/// <param name="logger">The logger</param>
		/// <param name="functionName">The function name being logged</param>
		[NonAction]
		private void FinalizeAction(string functionName)
		{
			if (functionName == null)
			{
				throw new ArgumentNullException(nameof(functionName));
			}

			StopwatchTimer sw = HttpContext.Items["Stopwatch"] as StopwatchTimer;
			if (sw != null)
			{
				HttpContext.Items.Remove("Stopwatch");
				sw.Dispose();
			}
		}

		[NonAction]
		public void OnActionExecuting(ActionExecutingContext context)
		{
			if (context != null)
			{
				ControllerActionDescriptor descriptor = context.ActionDescriptor as ControllerActionDescriptor;
				string functionName = descriptor?.ActionName;
				StartAction(functionName);
			}
		}

		[NonAction]
		public void OnActionExecuted(ActionExecutedContext context)
		{
			if (context != null)
			{
				ControllerActionDescriptor descriptor = context.ActionDescriptor as ControllerActionDescriptor;
				string functionName = descriptor?.ActionName;
				FinalizeAction(functionName);
			}
		}
	}
}
