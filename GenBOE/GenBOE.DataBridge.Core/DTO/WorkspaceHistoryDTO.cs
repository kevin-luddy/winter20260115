using System;
using System.Diagnostics.CodeAnalysis;
using GenBOE.DataBridge.Core.DTO.Common;
using IES.Common;
using IES.Common.Core.Enums;

//DTO that contains Workspace History data.
namespace GenBOE.DataBridge.Core.DTO
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class WorkspaceHistoryDTO : IWorkspaceMembership
	{
		public WorkspaceHistoryDTO()
		{
			OldValue = WorkspaceState.None;
			NewValue = WorkspaceState.None;
			PerformedByETIUserId = 0;
			Date = DateTime.MinValue;
		}

		public WorkspaceState OldValue { get; set; }
		public WorkspaceState NewValue { get; set; }
		public int PerformedByETIUserId { get; set; }
		public DateTime Date { get; set; }

		// Implemented Properties
		public int WorkspaceID { get; set; }
	}
}
