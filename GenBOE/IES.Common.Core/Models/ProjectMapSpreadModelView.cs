// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Models
{
	/// <summary>
	/// Model View for Project Map Items/Rows.
	/// </summary>
	public class ProjectMapSpreadModelView : UpdateableDTO
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectMapSpreadModelView"/> class.
		/// </summary>
		public ProjectMapSpreadModelView()
		{
			Id = -1;
		}

		/// <summary>
		/// Gets or sets the workspace identifier.
		/// </summary>
		public int WorkspaceId { get; set; }

		/// <summary>
		/// Gets or sets the project map identifier.
		/// </summary>
		public int ProjectMapId { get; set; }

		/// <summary>
		/// Gets or sets the spread date.
		/// </summary>
		public DateTime SpreadDate { get; set; }

		/// <summary>
		/// Gets or sets the spread value.
		/// </summary>
		public decimal SpreadValue { get; set; }

		/// <summary>
		/// Propagates the new 'parent' DTO Id to all 'child' DTOs in collections.
		/// </summary>
		/// <param name="newParentId">new id of the parent DTO</param>
		protected override void PropagateNewParentIdToChildDTOs(int newParentId)
		{
			// do nothing
		}
	}
}