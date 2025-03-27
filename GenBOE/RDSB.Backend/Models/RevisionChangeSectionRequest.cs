// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Backend.Models
{
	using System.Collections.Generic;

	public class RevisionChangeSectionRequest
	{
		public int fromRevisionID { get; set; }
		public int toRevisionID { get; set; }
		public ICollection<int> selectedSectionIds { get; set; }
	}
}
