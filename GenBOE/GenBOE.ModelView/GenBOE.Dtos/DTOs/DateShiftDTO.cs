// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using IES.Common;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;

	/// <summary>
	/// DTO that will contain Date Shifts.
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public sealed class DateShiftDTO : UpdateableDTO
	{
		//private bool hasSpread;

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
		public HashSet<DateShiftDTO> Children { get; set; } = new HashSet<DateShiftDTO>();

		///// <summary>
		///// Child date shift objects.
		///// </summary>
		//ICollection<IDateShiftable> IDateShiftable.Children => Children;

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
		public Level DateShiftLevel { get; set; }

		/// <summary>
		/// Date shift level value due to readonly date shift level on interface.
		/// </summary>
		public int Level { get; set; }

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
		public Collection<ResourceSpreadDto> LaborSpreads { get; set; }

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
		/// Spread curve id.
		/// </summary>
		public SpreadCurves? SpreadCurveID { get; set; }

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

		/// <summary>
		/// Conversion for incoming inherit classes.
		/// </summary>
		public static DateShiftDTO FromIDateShiftable(DateShiftDTO incomingDateShiftData)
		{
			if (incomingDateShiftData == null)
			{
				throw new ArgumentNullException(nameof(incomingDateShiftData));
			}

			// TODO Thomas: Need to set the stuff here
			DateShiftDTO dateShiftDTO = new DateShiftDTO();
			dateShiftDTO.StartDate = incomingDateShiftData.StartDate;
			dateShiftDTO.EndDate = incomingDateShiftData.EndDate;
			dateShiftDTO.Level = (int)incomingDateShiftData.DateShiftLevel;
			dateShiftDTO.DateShiftLevel = incomingDateShiftData.DateShiftLevel;
			dateShiftDTO.HasSpread = incomingDateShiftData.HasSpread;
			dateShiftDTO.Updateable = incomingDateShiftData.Updateable;

			if (incomingDateShiftData is IUpdateableDTO updateableDTO)
			{
				dateShiftDTO.Id = updateableDTO.Id;
				dateShiftDTO.UpdateDate = updateableDTO.UpdateDate;
			}

			if (incomingDateShiftData.DateShiftLevel == IES.Common.Level.CLIN)
			{
				dateShiftDTO.ClinId = incomingDateShiftData.Id;
				dateShiftDTO.WorkspaceId = incomingDateShiftData.WorkspaceId;
				dateShiftDTO.ParentId = incomingDateShiftData.WorkspaceId;
			}

			//if (dateShiftData is FullClin clinDTO && clinDTO != null)
			//{
			//	dateShiftDTO.ClinId = clinDTO.Id;
			//	dateShiftDTO.WorkspaceId = clinDTO.WorkspaceID;
			//	dateShiftDTO.ParentId = clinDTO.WorkspaceID;
			//}

			if (incomingDateShiftData.DateShiftLevel == IES.Common.Level.BOE)
			{
				dateShiftDTO.BOEStateID = incomingDateShiftData.BOEStateID;
				dateShiftDTO.ParentId = incomingDateShiftData.ClinId ?? incomingDateShiftData.WorkspaceId;
				dateShiftDTO.ClinId = incomingDateShiftData.ClinId;
				dateShiftDTO.WbsId = incomingDateShiftData.WbsId;
				dateShiftDTO.WorkspaceId = incomingDateShiftData.WorkspaceId;
			}

			//if (incomingDateShiftData is FullBoe boeDTO && boeDTO != null)
			//{
			//	dateShiftDTO.BOEStateID = (int)boeDTO.State;
			//	dateShiftDTO.ParentId = boeDTO.CLINID ?? boeDTO.WorkspaceID;
			//	dateShiftDTO.ClinId = boeDTO.CLINID;
			//	dateShiftDTO.WbsId = boeDTO.WBSID;
			//	dateShiftDTO.WorkspaceId = boeDTO.WorkspaceID;
			//}

			if (incomingDateShiftData.DateShiftLevel == IES.Common.Level.Task)
			{
				dateShiftDTO.BoeId = incomingDateShiftData.BoeId;
				dateShiftDTO.BOETaskElementId = incomingDateShiftData.Id;
				dateShiftDTO.CommonDisclosureTable = incomingDateShiftData.CommonDisclosureTable;
				dateShiftDTO.SkillMixTable = incomingDateShiftData.SkillMixTable;
				dateShiftDTO.TaskElementLabors = incomingDateShiftData.TaskElementLabors;
				dateShiftDTO.ParentId = incomingDateShiftData.BoeId;
			}

			//if (incomingDateShiftData is BoeTaskElementDTO boeTaskElementDTO && boeTaskElementDTO != null)
			//{
			//	dateShiftDTO.BoeId = boeTaskElementDTO.BoeID;
			//	dateShiftDTO.BOETaskElementId = boeTaskElementDTO.Id;
			//	dateShiftDTO.CommonDisclosureTable = boeTaskElementDTO.CommonDisclosureTable;
			//	dateShiftDTO.SkillMixTable = boeTaskElementDTO.SkillMixTable;
			//	dateShiftDTO.TaskElementLabors = boeTaskElementDTO.taskElementLabors;
			//	dateShiftDTO.ParentId = boeTaskElementDTO.BoeID;
			//}

			if (incomingDateShiftData.DateShiftLevel == IES.Common.Level.Labor)
			{
				// Check if Labor Spreads is not null and if not null then save them later.
				dateShiftDTO.LaborSpreads = incomingDateShiftData.LaborSpreads;
				dateShiftDTO.SpreadCurveID = incomingDateShiftData.SpreadCurveID;
			}

			//	if (incomingDateShiftData is ResourceTypeDto resourceTypeDto && resourceTypeDto != null && (resourceTypeDto.SpreadCurveID == SpreadCurves.DiscreteCost || resourceTypeDto.SpreadCurveID == SpreadCurves.DiscreteHours))
			//{
			//	// Check if Labor Spreads is not null and if not null then save them later.
			//	dateShiftDTO.LaborSpreads = resourceTypeDto.LaborSpreads;
			//	dateShiftDTO.SpreadCurveID = resourceTypeDto.SpreadCurveID;
			//}

			if (incomingDateShiftData.DateShiftLevel == IES.Common.Level.Workspace)
			{
				dateShiftDTO.WorkspaceVersionMetaData = incomingDateShiftData.WorkspaceVersionMetaData.ToList();
				dateShiftDTO.WorkspaceState = incomingDateShiftData.WorkspaceState;
			}

			//	if (incomingDateShiftData is FullWorkspace fullWorkspace && fullWorkspace != null)
			//{
			//	dateShiftDTO.WorkspaceVersionMetaData = fullWorkspace.WorkspaceVersionMetaData.ToList();
			//	dateShiftDTO.WorkspaceState = fullWorkspace.WorkspaceState;
			//}

			HashSet<(int Id, int Level)> uniqueChildren = new HashSet<(int Id, int Level)>();

			foreach (DateShiftDTO child in incomingDateShiftData.Children)
			{
				DateShiftDTO childDTO = FromIDateShiftable(child);

				if (childDTO != null)
				{
					if (uniqueChildren.Add((childDTO.Id, (int)childDTO.DateShiftLevel)))
					{
						dateShiftDTO.Children.Add(childDTO);
					}
				}
			}

			return dateShiftDTO;
		}
	}
}