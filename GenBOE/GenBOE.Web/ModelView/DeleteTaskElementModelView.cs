// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.ModelView
{
	using System;
	using GenBOE.ActionLogic.ModelView;

	/// <summary>
	/// Payload model for HttpDelete DeleteTaskElement
	/// </summary>
	[Serializable]
	public class DeleteTaskElementModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public DeleteTaskElementModelView()
		{
			this.workspaceShortName = String.Empty;
			this.boeId = -1;
			this.deletedTask = null;
		}

		/// <summary>
		/// Ctir
		/// </summary>
		/// <param name="workspaceShortName">workspace shortname string</param>
		/// <param name="boeId">boe id where labor task is being deleted from</param>
		/// <param name="deletedTask">labor task about to be deleted</param>
		public DeleteTaskElementModelView(string workspaceShortName, int boeId, GenericTaskElementGridRow deletedTask)
		{
			this.workspaceShortName = workspaceShortName;
			this.boeId = boeId;
			this.deletedTask = deletedTask;
		}

		/// <summary>
		/// Workspace short name
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// boeId where the Task Element is being deleted from
		/// </summary>
		public int boeId { get; set; }

		/// <summary>
		/// Task Element to be deleted
		/// </summary>
		public GenericTaskElementGridRow deletedTask { get; set; }
	}
}