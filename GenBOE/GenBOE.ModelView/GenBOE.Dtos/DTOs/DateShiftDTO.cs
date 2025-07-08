// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using GenBOE.DataBridge.DTO;
	using IES.Common;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// DTO that will contain Date Shifts.
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public sealed class DateShiftDTO : UpdateableDTO
	{
		private Level dateShiftLevel;

		/// <summary>
		/// Start date.
		/// </summary>
		public DateTime? StartDate { get; set; }

		/// <summary>
		/// End date.
		/// </summary>
		public DateTime? EndDate { get; set; }

		/// <summary>
		/// Children date shift objects.
		/// </summary>
		public List<DateShiftDTO> Children { get; set; } = new List<DateShiftDTO>();

		/// <summary>
		/// Parent of the date shift object.
		/// </summary>
		public DateShiftDTO Parent { get; set; }

		/// <summary>
		/// Parent id of the date shift object.
		/// </summary>
		public int? ParentId { get; set; }

		/// <summary>
		/// Has spread value due to readonly has spread on interface.
		/// </summary>
		public bool HasSpread { get; set; }

		/// <summary>
		/// Date shift level.
		/// </summary>
		public Level DateShiftLevel
		{
			get => dateShiftLevel;
			set
			{
				dateShiftLevel = value;
				Level = (int)value;
			}
		}

		/// <summary>
		/// Date shift level value due to readonly date shift level on interface.
		/// </summary>
		public int Level { get; private set; }

		/// <summary>
		/// Workspace Id (highest parent level for any date shift).
		/// </summary>
		public int WorkspaceId { get; set; }

		/// <summary>
		/// Boe ID.
		/// </summary>
		public int BoeId { get; set; }

		/// <summary>
		/// WBS id.
		/// </summary>
		public int? WbsId { get; set; }

		/// <summary>
		/// Clin Id.
		/// </summary>
		public int? ClinId { get; set; }

		/// <summary>
		/// Boe task element Id if date shiftable is a task.
		/// </summary>
		public int? BOETaskElementId { get; set; }

		/// <summary>
		/// BOE State ID.
		/// </summary>
		public int BOEStateID { get; set; }

		/// <summary>
		/// Labor spreads across resources that needs to be date shifted.
		/// </summary>
		public Collection<ResourceSpreadDto> LaborSpreads { get; set; } = new Collection<ResourceSpreadDto>();

		/// <summary>
		/// Skill Mix table
		/// </summary>
		public ICollection<SkillMixModelView> SkillMixTable { get; set; } = new List<SkillMixModelView>();

		/// <summary>
		/// Common Disclosure table
		/// </summary>
		public ICollection<CommonDisclosureModelView> CommonDisclosureTable { get; set; } = new List<CommonDisclosureModelView>();

		/// <summary>
		/// The task element labors associated with the task.
		/// </summary>
		public Collection<ResourceTypeDto> TaskElementLabors { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int LaborTypeId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int ResourceId { get; set; }

		/// <summary>
		/// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
		/// </summary>
		internal IEnumerable<ResourceSpreadDto> LaborSpreadsIEnum { get; set; }

		/// <summary>
		/// Spread curve id.
		/// </summary>
		public SpreadCurves? SpreadCurveID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int SpreadCurveIdValue { get; set; }

		/// <summary>
		/// Gets/Sets Spread Type (Hours/Cost).
		/// </summary>
		public SpreadType SpreadType { get; set; }

		/// <summary>
		/// Value spread.
		/// </summary>
		public decimal? ValueSpread { get; set; }

		/// <summary>
		/// Workspace version meta data.
		/// </summary>
		public ICollection<WorkspaceVersionMetaDataDTO> WorkspaceVersionMetaData { get; set; } = new List<WorkspaceVersionMetaDataDTO>();

		/// <summary>
		/// Workspace state.
		/// </summary>
		public WorkspaceState WorkspaceState { get; set; }
	}
}