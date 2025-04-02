// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic.IO.Export.BOE
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using GenBOE.Dtos;

	/// <summary>
	/// A grouping of a custom field and value
	/// </summary>
	public class CustomFieldGrouping
	{
		public CustomFieldValueDTO CustomFieldValue { get; set; }

		public CustomFieldDTO CustomField { get; set; }
	}
}
