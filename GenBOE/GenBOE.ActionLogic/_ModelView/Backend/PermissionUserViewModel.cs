// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Backend
{
	/// <summary>
	/// BOE Configuration Class (data returned from GenBOE's Web.config going to the new GenBOE UI)
	/// 
	/// THIS MUST MATCH THE BOE SERVICE, DO NOT CHANGE UNLESS CHANGES ARE MADE TO BOTH CLASSES.
	/// </summary>
	public class PermissionUserViewModel
	{
		public PermissionUserViewModel()
		{
			DisplayName = string.Empty;
			UserID = int.MinValue;
		}

		/// <summary>
		/// Users display name
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Users id
		/// </summary>
		public int UserID { get; set; }
	}
}
