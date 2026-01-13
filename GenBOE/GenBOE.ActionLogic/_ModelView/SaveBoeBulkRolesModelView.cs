// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2026 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic._ModelView
{
	using GenBOE.ActionLogic.ModelView;
	using System.Collections.Generic;

	public class SaveBoeBulkRolesModelView
	{
		public SaveBoeBulkRolesModelView()
		{
			this.Workspace = string.Empty;
			this.BoeRolesToSave = new List<ManageBOEModelView>();
		}

		public string Workspace { get; set; }

		public ICollection<ManageBOEModelView> BoeRolesToSave { get; set; }
	}
}
