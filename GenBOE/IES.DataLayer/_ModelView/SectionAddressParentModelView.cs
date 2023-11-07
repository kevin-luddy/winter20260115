// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// A truncated model of the Section object, used for recursively traversing through sections to get the Title
	/// </summary>
	public class SectionAddressParentModelView
	{
		/// <summary>
		/// The unique ID for that address
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The title for where the address resides
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The ID of the parent section, if any
		/// </summary>
		public int? ParentID { get; set; }
	}
}
