using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using IES.Common.Exceptions;
using HtmlAgilityPack;

namespace IES.Common
{
	/// <summary>
	/// This class is for general utility functions for the whole solution to use.
	/// </summary>
	public static class GenBOEUtilities
	{

		/// <summary>
		/// Perform a deep Copy of the object.
		/// Taken from http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
		/// </summary>
		/// <typeparam name="T">The type of object being copied.</typeparam>
		/// <param name="source">The object instance to copy.</param>
		/// <returns>The copied object.</returns>
		public static T Clone<T>(T source)
		{
			if (!typeof(T).IsSerializable)
			{
				throw new ArgumentException("The type must be serializable.", nameof(source));
			}

			// Don't serialize a null object, simply return the default for that object
			if (Object.ReferenceEquals(source, null))
			{
				return default(T);
			}

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream();
			using (stream)
			{
				formatter.Serialize(stream, source);
				stream.Seek(0, SeekOrigin.Begin);
				return (T)formatter.Deserialize(stream);
			}
		}

		//default to month
		public static DateTime AdjustDateTimePrecision(DateTime inDateTime)
		{
			return AdjustDateTimePrecision(inDateTime, DateTimePrecision.Month);
		}

		public static DateTime AdjustDateTimePrecision(DateTime inDateTime, DateTimePrecision inPrecision)
		{
			DateTime returnDate;
			switch (inPrecision)
			{
				case DateTimePrecision.Day:
					returnDate = new DateTime(
						inDateTime.Year,
						inDateTime.Month,
						inDateTime.Day,
						12,
						0,
						0,
						0,
						inDateTime.Kind);
					break;
				case DateTimePrecision.Month:
					returnDate = new DateTime(
						inDateTime.Year,
						inDateTime.Month,
						15,
						12,
						0,
						0,
						0,
						inDateTime.Kind);
					break;
				default:
					returnDate = new DateTime(
						inDateTime.Year,
						inDateTime.Month,
						15,
						12,
						0,
						0,
						0,
						inDateTime.Kind);
					break;
			}
			return returnDate;
		}

		/// <summary>
		/// Convert HTML to text.
		/// </summary>
		/// <param name="html">HTML</param>
		/// <returns>Text</returns>
		public static string ConvertHtmlToText(string html)
		{
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(html);
			string text = HttpUtility.HtmlDecode(document.DocumentNode.InnerText);
			return text;
		}

		/// <summary>
		/// Use the HtmlAgilityPack to identify and remove styling and/or markup that are known to cause problems
		/// for the "HtmlConverter" third-party HTML conversion utility.
		/// </summary>
		/// <param name="html">HTML</param>
		/// <param name="validationMessages">Repository for validation messages</param>
		/// <seealso cref="NotesFor.HtmlToOpenXml.HtmlConverter"/>
		public static string ScrubRichTextForSave(string html, ICollection<ValidationMessage> validationMessages)
		{
			return HtmlAgilityPackUtilities.ScrubRichTextForSave(html, validationMessages);
		}
	}
}
