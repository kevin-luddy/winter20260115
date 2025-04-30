// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Common
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	public class ImportViewModel
	{
		/// <summary>
		/// Workspace Name
		/// </summary>
		public string Workspace { get; set; }

		public int TestNumber { get; set; }

		public bool TestBoolean { get; set; }
	}
}