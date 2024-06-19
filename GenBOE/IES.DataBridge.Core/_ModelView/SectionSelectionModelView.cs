// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	/// <summary>
	/// View Model for Section selections after changing Revision in RDSB
	/// </summary>
	public class SectionSelectionModelView
	{
		/// <summary>
		/// ctor
		/// </summary>
		public SectionSelectionModelView()
		{
			SelectedSections = new Collection<int>();
			UnmappedSections = new Collection<string>();
		}

		/// <summary>
		/// Sections to be selected after changing the revision
		/// </summary>
		public ICollection<int> SelectedSections { get; set; }

		/// <summary>
		/// Sections that were selected but are unable to be mapped in the new revision
		/// </summary>
		public ICollection<string> UnmappedSections { get; set; }
	}
}
