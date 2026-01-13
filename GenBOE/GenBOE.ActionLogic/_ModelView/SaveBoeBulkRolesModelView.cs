// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2026 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic._ModelView
{
	using GenBOE.ActionLogic.ModelView;
	using System.Collections.Generic;
	
	/// <summary>
	/// Model view class that represents BOEs data to save
	/// </summary>
	public class SaveBoeBulkRolesModelView
	{
		public SaveBoeBulkRolesModelView()
		{
			this.Workspace = string.Empty;
			this.BoeRolesToSave = new List<ManageBOEModelView>();
		}

		/// <summary>
		/// Current workspace
		/// </summary>
		public string Workspace { get; set; }

		/// <summary>
		/// BOEs to save
		/// </summary>
		public ICollection<ManageBOEModelView> BoeRolesToSave { get; set; }
	}
}
