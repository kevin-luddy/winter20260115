// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
	public sealed class DateShiftDTO : UpdateableDTO, IDateShiftable
	{
		private readonly List<IDateShiftable> children = new List<IDateShiftable>();
		private bool hasSpread;
		private Level dateShiftLevel;

		/// <summary>
		/// Start date.
		/// </summary>
		DateTime? IDateShiftable.StartDate { get; set; }

		/// <summary>
		/// End date.
		/// </summary>
		DateTime? IDateShiftable.EndDate { get; set; }

		/// <summary>
		/// Child date shift objects.
		/// </summary>
		ICollection<IDateShiftable> IDateShiftable.Children => children;

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
		Level IDateShiftable.DateShiftLevel => dateShiftLevel;

		/// <summary>
		/// Date shift level value due to readonly date shift level on interface.
		/// </summary>
		public Level DateShiftLevelValue { get => dateShiftLevel; set => dateShiftLevel = value; }

		/// <summary>
		/// Updatable.
		/// </summary>
		UpdateType IDateShiftable.Updateable { get; set; }

		/// <summary>
		/// Boe ID.
		/// </summary>
		public int BoeId { get; set; }

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
		/// Start date.
		/// </summary>
		public DateTime StartDate { get; set; } = DateTime.MinValue;

		/// <summary>
		/// End date.
		/// </summary>
		public DateTime EndDate { get; set; } = DateTime.MaxValue;

		/// <summary>
		/// Value spread.
		/// </summary>
		public decimal? ValueSpread { get; set; }

		/// <summary>
		/// Conversion for incoming inherit classes.
		/// </summary>
		public static DateShiftDTO FromIDateShiftable(IDateShiftable dateShiftable)
		{
			if (dateShiftable == null)
			{
				throw new ArgumentNullException(nameof(dateShiftable));
			}

			DateShiftDTO dateShiftDTO = new DateShiftDTO();
			((IDateShiftable)dateShiftDTO).StartDate = dateShiftable.StartDate;
			((IDateShiftable)dateShiftDTO).EndDate = dateShiftable.EndDate;
			dateShiftDTO.DateShiftLevelValue = dateShiftable.DateShiftLevel;
			dateShiftDTO.HasSpreadValue = dateShiftable.HasSpread;

			if (dateShiftable is UpdateableDTO updateableDTO)
			{
				dateShiftDTO.Id = updateableDTO.Id;
				dateShiftDTO.UpdateDate = updateableDTO.UpdateDate;
				dateShiftDTO.UpdateDateLong = updateableDTO.UpdateDateLong;
				((IDateShiftable)dateShiftDTO).Updateable = updateableDTO.Updateable;
			}

			if (dateShiftable is BoeTaskElementDTO boeTaskElementDTO)
			{
				dateShiftDTO.CommonDisclosureTable = boeTaskElementDTO.CommonDisclosureTable;
				dateShiftDTO.SkillMixTable = boeTaskElementDTO.SkillMixTable;
				dateShiftDTO.TaskElementLabors = boeTaskElementDTO.taskElementLabors;
			}

			if (dateShiftable is ResourceTypeDto resourceTypeDto)
			{
				dateShiftDTO.LaborSpreads = resourceTypeDto.LaborSpreads;
				dateShiftDTO.SpreadCurveID = resourceTypeDto.SpreadCurveID;
				dateShiftDTO.StartDate = resourceTypeDto.StartDate ?? DateTime.MinValue;
				dateShiftDTO.EndDate = resourceTypeDto.EndDate ?? DateTime.MaxValue;
				dateShiftDTO.SpreadType = resourceTypeDto.SpreadType;
				dateShiftDTO.ValueSpread = resourceTypeDto.ValueSpread;
			}

			// Store the BOE Id
			switch (dateShiftable.DateShiftLevel)
			{
				case Level.BOE:
					dateShiftDTO.BoeId = ((UpdateableDTO)dateShiftable).Id;
					break;
				case Level.Task:
					dateShiftDTO.BoeId = ((BoeTaskElementDTO)dateShiftable).BoeID;
					break;
				case Level.Travel:
					dateShiftDTO.BoeId = ((TravelDTO)dateShiftable).BoeID;
					break;
			}

			foreach (IDateShiftable child in dateShiftable.Children)
			{
				dateShiftDTO.children.Add(FromIDateShiftable(child));
			}

			return dateShiftDTO;
		}
	}
}