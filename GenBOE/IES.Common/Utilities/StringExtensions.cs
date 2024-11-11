// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using DocumentFormat.OpenXml;

	/// <summary>
	/// Extensions for string class
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// String comparision, returns true if both sides are null/empty/whitespace
		/// </summary>
		/// <param name="firstString">string 1</param>
		/// <param name="secondString">string 2</param>
		/// <returns></returns>
		public static bool NullEmptyEquals(this string firstString, string secondString)
		{
			if (string.IsNullOrWhiteSpace(firstString) && string.IsNullOrWhiteSpace(secondString))
			{
				return true;
			}

			return firstString == secondString;
		}
	}
}
