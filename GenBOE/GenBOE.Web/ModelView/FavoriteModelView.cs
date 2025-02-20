// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.ModelView
{
	using System;

	/// <summary>
	/// The payload model for Favorite POST request
	/// </summary>
	[Serializable]
	public class FavoriteModelView
	{
		public FavoriteModelView()
		{
			this.WorkspaceId = -1;
			this.IsFavorite = false;
		}

		public FavoriteModelView(int workspaceId,  bool isFavorite)
		{
			this.WorkspaceId = workspaceId;
			this.IsFavorite = isFavorite;
		}

		/// <summary>
		/// Workspace Id
		/// </summary>
		public int WorkspaceId { get; set; }

		/// <summary>
		/// If Workspace is Favorite
		/// </summary>
		public bool IsFavorite { get; set; }
	}
}