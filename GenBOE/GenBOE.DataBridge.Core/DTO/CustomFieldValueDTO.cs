// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common.Core.Models;

	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class CustomFieldValueDTO : UpdateableDTO
	{
		public CustomFieldValueDTO()
		{
		}

		public int CustomFieldValueID { get; set; }

		public string CustomFieldValueName { get; set; }

		public string CustomFieldValueDescription { get; set; }

		public int CustomFieldID { get; set; }

		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
		public bool CustomFieldValueInUseFlag { get; set; }

	}
}
