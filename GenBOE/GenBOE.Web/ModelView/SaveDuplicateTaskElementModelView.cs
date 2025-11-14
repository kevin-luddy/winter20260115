// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.ModelView
{
	using System;
	using GenBOE.ActionLogic.ModelView;
	using IES.Common;

	/// <summary>
	/// POST body for Saving duplicate task elements
	/// </summary>
	[Serializable]
	public class SaveDuplicateTaskElementModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public SaveDuplicateTaskElementModelView() 
		{
			this.taskElementDuplicateFormCollection = null;
			this.workspace = string.Empty;
			this.boeId = -1;
			this.taskType = 0;
		}

		/// <summary>
		/// Task elements about to be duplicated
		/// </summary>
		public TaskElementDuplicateFormCollection taskElementDuplicateFormCollection { get; set; }

		/// <summary>
		/// workspace shortname
		/// </summary>
		public string workspace { get; set; }

		/// <summary>
		/// boeId
		/// </summary>
		public int boeId { get; set; }

		/// <summary>
		/// Task Type
		/// </summary>
		public TaskType taskType { get; set; }
	}
}