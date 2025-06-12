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

	/// <summary>
	/// DTO that will contain Date Shifts.
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public sealed class DateShiftDTO : UpdateableDTO, IDateShiftable
	{
		private readonly List<IDateShiftable> children = new List<IDateShiftable>();
		private bool hasSpread;
		private object originalObject;

		/// <summary>
		/// Start date.
		/// </summary>
		public DateTime? StartDate { get; set; }

		/// <summary>
		/// End date.
		/// </summary>
		public DateTime? EndDate { get; set; }

		/// <summary>
		/// Child date shift objects.
		/// </summary>
		ICollection<IDateShiftable> IDateShiftable.Children => children;

		/// <summary>
		/// Parent of the date shift object.
		/// </summary>
		public IDateShiftable Parent { get; set; }

		/// <summary>
		/// Parent id of the date shift object.
		/// </summary>
		public int? ParentId { get; set; }

		/// <summary>
		/// Has spread?
		/// </summary>
		bool IDateShiftable.HasSpread => hasSpread;

		/// <summary>
		/// Has spread value due to readonly has spread on interface.
		/// </summary>
		public bool HasSpreadValue { get => hasSpread; set => hasSpread = value; }

		/// <summary>
		/// Date shift level.
		/// </summary>
		public Level DateShiftLevel { get; set; }

		/// <summary>
		/// Date shift level value due to readonly date shift level on interface.
		/// </summary>
		public int Level { get; set; }

		/// <summary>
		/// Boe ID.
		/// </summary>
		public int BoeId { get; set; }

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
		/// Original object data.
		/// </summary>
		public object OriginalObject { get => this.originalObject; set => this.originalObject = value; }

		/// <summary>
		/// Conversion for incoming inherit classes.
		/// </summary>
		public static DateShiftDTO FromIDateShiftable(IDateShiftable dateShiftable, bool isLoading = false)
		{
			if (dateShiftable == null)
			{
				throw new ArgumentNullException(nameof(dateShiftable));
			}

			if (dateShiftable.Updateable != UpdateType.Upsert && !isLoading)
			{
				return null;
			}

			DateShiftDTO dateShiftDTO = new DateShiftDTO();
			dateShiftDTO.OriginalObject = dateShiftable;
			dateShiftDTO.StartDate = dateShiftable.StartDate;
			dateShiftDTO.EndDate = dateShiftable.EndDate;
			dateShiftDTO.Level = (int)dateShiftable.DateShiftLevel;
			dateShiftDTO.DateShiftLevel = dateShiftable.DateShiftLevel;
			dateShiftDTO.HasSpreadValue = dateShiftable.HasSpread;
			dateShiftDTO.Updateable = dateShiftable.Updateable;

			if (dateShiftable is IUpdateableDTO updateableDTO)
			{
				dateShiftDTO.Id = updateableDTO.Id;
				dateShiftDTO.UpdateDate = updateableDTO.UpdateDate;
			}

			if (dateShiftable is FullBoe boeDTO)
			{
				dateShiftDTO.BOEStateID = (int)boeDTO.State;
				dateShiftDTO.ParentId = boeDTO.CLINID;
			}

			if (dateShiftable is BoeTaskElementDTO boeTaskElementDTO)
			{
				dateShiftDTO.BoeId = boeTaskElementDTO.BoeID;
				dateShiftDTO.BOETaskElementId = boeTaskElementDTO.Id;
				dateShiftDTO.CommonDisclosureTable = boeTaskElementDTO.CommonDisclosureTable;
				dateShiftDTO.SkillMixTable = boeTaskElementDTO.SkillMixTable;
				dateShiftDTO.TaskElementLabors = boeTaskElementDTO.taskElementLabors;
				dateShiftDTO.ParentId = boeTaskElementDTO.BoeID;
			}

			if (dateShiftable is ResourceTypeDto resourceTypeDto && (resourceTypeDto.SpreadCurveID == SpreadCurves.DiscreteCost || resourceTypeDto.SpreadCurveID == SpreadCurves.DiscreteHours))
			{
				// Check if Labor Spreads is not null and if not null then save them later.
				dateShiftDTO.LaborSpreads = resourceTypeDto.LaborSpreads;
			}

			foreach (IDateShiftable child in dateShiftable.Children)
			{
				DateShiftDTO childDTO = FromIDateShiftable(child);

				if (childDTO != null)
				{
					dateShiftDTO.children.Add(childDTO);
				}
			}

			return dateShiftDTO;
		}
	}
}