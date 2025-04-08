// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using GenBOE.DataBridge.Core.DTO.Common;
	using IES.Common;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;

	/// <summary>
	/// This is the labor type data associated with the BOE DTO
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class ResourceTypeDto : UpdateableDTO, IBOEMembership, IStartEndDates, IDateShiftable
	{
		public ResourceTypeDto()
		{
			Id = -1;
			ResourceID = 0;
			PerformingOrgID = 0;
			spreadCurveIDField = null;
			PercentSpread = 0;
			ValueSpread = 0;
			StartDate = DateTime.MaxValue;
			EndDate = DateTime.MinValue;
			LaborSpreads = new Collection<ResourceSpreadDto>();
			CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();
			HourSpreadLocked = false;
			PercentSpreadLocked = false;
			WBSID = null;
			CLINID = null;
			IsAddOrDelete = null;
			LaborTypeOrder = 2000; // New resource types should be put at bottom of order
			BusinessResourceCodeID = 0;
			OriginalID = 0;
		}

		/*
         * used for the importers to create a new imported type from the DB.
         */
		public ResourceTypeDto(ResourceTypeDto inBOELaborType) : this()
		{
			if (inBOELaborType != null)
			{
				BoeID = inBOELaborType.BoeID;
				Id = inBOELaborType.Id;
				EndDate = inBOELaborType.EndDate;
				LaborSpreads = inBOELaborType.LaborSpreads;
				PercentSpread = inBOELaborType.PercentSpread;
				PerformingOrgID = inBOELaborType.PerformingOrgID;
				ResourceID = inBOELaborType.ResourceID;
				SpreadCurveID = inBOELaborType.SpreadCurveID;
				StartDate = inBOELaborType.StartDate;
				ValueSpread = inBOELaborType.ValueSpread;
				CustomFieldValueContainers = inBOELaborType.CustomFieldValueContainers;
				PercentSpreadLocked = inBOELaborType.PercentSpreadLocked;
				HourSpreadLocked = inBOELaborType.HourSpreadLocked;
				SpreadType = inBOELaborType.SpreadType;
				WBSID = inBOELaborType.WBSID;
				CLINID = inBOELaborType.CLINID;
				IsAddOrDelete = inBOELaborType.IsAddOrDelete;
				CanOffload = inBOELaborType.CanOffload;
				TieredPercentage = inBOELaborType.TieredPercentage;
				LaborTypeOrder = inBOELaborType.LaborTypeOrder;
				BusinessResourceCodeID = inBOELaborType.BusinessResourceCodeID;
				OriginalID = inBOELaborType.OriginalID;
			}
		}

		// the resource code ID
		public int? ResourceID { get; set; }

		// the performing organization ID
		public int? PerformingOrgID { get; set; }

		// the spread curve ID (field)
		private SpreadCurves? spreadCurveIDField;

		// the spread curve ID
		public SpreadCurves? SpreadCurveID
		{
			get
			{
				return spreadCurveIDField;
			}

			set
			{
				spreadCurveIDField = value;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
		public int? SpreadCurveIDValue { set { SpreadCurveID = value.HasValue ? (SpreadCurves)value.Value : null; } }

		/// <summary>
		/// Gets/Sets Spread Type (Hours/Cost)
		/// </summary>
		public SpreadType SpreadType { get; set; }

		// the percent spread
		public decimal? PercentSpread { get; set; }

		// the Hour spread
		// Before It 23, this variable was named HourSpread. It is now ValueSpread to represent Hour or Cost Spread
		// the SpreadType will determine which spread it is
		public decimal? ValueSpread { get; set; }

		// the start date
		public DateTime StartDateValue { get; set; }

		/// <summary>
		/// Gets or sets the start date.
		/// </summary>
		public DateTime? StartDate
		{
			get
			{
				return StartDateValue;
			}
			set
			{
				StartDateValue = value.HasValue ? value.Value : DateTime.MaxValue;
			}
		}

		// the end date
		public DateTime EndDateValue { get; set; }

		/// <summary>
		/// Gets or sets the end date.
		/// </summary>
		public DateTime? EndDate
		{
			get
			{
				return EndDateValue;
			}
			set
			{
				EndDateValue = value.HasValue ? value.Value : DateTime.MinValue;
			}
		}

		// the labor spreads associated with a labor type
		public Collection<ResourceSpreadDto> LaborSpreads { get; set; }

		public int BoeID { get; set; }

		/// <summary>
		/// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
		/// </summary>
		internal IEnumerable<CustomFieldValueContainer> CustomFieldValueContainersIEnum { get; set; }

		//Labor Type custom fields
		public Collection<CustomFieldValueContainer> CustomFieldValueContainers { get; set; }

		// Percent Spread Locked, true is locked, false is unlocked
		public bool PercentSpreadLocked { get; set; }

		// Hour Spread Locked, true if locked, false is unlocked
		public bool HourSpreadLocked { get; set; }

		public int TaskElementId { get; set; }

		// Resource Type WBS Id
		public int? WBSID { get; set; }

		// Resource Type CLIN Id
		public int? CLINID { get; set; }

		/// <summary>
		/// Propagates the new 'parent' DTO Id to all first level 'child' DTOs in collections.
		/// </summary>
		/// <param name="newParentId">new id of the parent DTO</param>
		override protected void PropagateNewParentIdToChildDTOs(int newParentId)
		{
			//// now update the child DTOs in each collection on this element
			if (LaborSpreads != null && LaborSpreads.Any())
			{
				foreach (ResourceSpreadDto resourceSpreadDto in LaborSpreads)
				{
					resourceSpreadDto.LaborTypeId = newParentId;
				}
			}

			if (CustomFieldValueContainers != null && CustomFieldValueContainers.Any())
			{
				foreach (CustomFieldValueContainer container in CustomFieldValueContainers)
				{
					container.OwnerID = newParentId;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance can try to offload.
		/// </summary>
		public bool CanOffload { get; set; }

		/// <summary>
		/// Gets or sets the tiered percentage value for project map workspaces.
		/// </summary>
		public decimal? TieredPercentage { get; set; }

		/// <summary>
		/// Gets or sets Add or Delete value
		/// 
		/// Null is possible for non Project Map Workspaces
		/// 
		/// For Project Map Workspaces
		///     D => delete
		///     A and anything else => add
		/// </summary>
		public string AddOrDelete { get; set; }

		/// <summary>
		/// The translation of IsAddOrDelete for DB needs
		/// 
		/// String -> bool
		///     Null -> Null
		///     D -> False
		///     anything else -> True
		///     
		/// Bool -> String
		///     Null -> Null
		///     True -> A
		///     False -> D
		/// </summary>
		public bool? IsAddOrDelete
		{
			get
			{
				return AddOrDelete == null ? null : !AddOrDelete.ToUpper().Equals("D");
			}

			set
			{
				AddOrDelete = !value.HasValue ? null : value.Value ? "A" : "D";
			}
		}

		/// <summary>
		/// ID for the Legacy Resource for Sikorsky's use in Project Map
		/// </summary>
		public int? LegacyID { get; set; }

		/// <summary>
		/// Gets or sets the project map identifier.
		/// </summary>
		public int ProjectMapId { get; set; }

		/// <summary>
		/// Gets the children that can be shifted.
		/// </summary>
		public ICollection<IDateShiftable> Children
		{
			get
			{
				return new List<IDateShiftable>();
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has a spread of values.
		/// </summary>
		public bool HasSpread
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets the date shift level.
		/// </summary>
		public Level DateShiftLevel
		{
			get
			{
				return Level.Labor;
			}
		}

		/// <summary>
		/// The value of the order in which the Labor Type will appear in the Task Element 
		/// </summary>
		public int LaborTypeOrder { get; set; }

		/// <summary>
		/// Auto property for bulk save to work properly.
		/// </summary>
		public int LaborSortID { get { return LaborTypeOrder; } }

		/// <summary>
		/// Indicates whether the Resource Type was generated by the offload process. Used by ProPricer exporter.
		/// </summary>
		public bool IsOffloaded { get; set; } = false;

		/// <summary>
		/// The BRC Resource ID
		/// </summary>
		public int? BusinessResourceCodeID { get; set; }

		/// <summary>
		/// The Original ID, used for BRCs
		/// </summary>
		public int OriginalID { get; set; }
	}

	public static class BOELaborTypeExtensions
	{
		public static ICollection<DateTime> GetSpreadDatesFull(this ICollection<ResourceTypeDto> laborTypes)
		{
			DateTime spreadStartDate = laborTypes.Where(s => s.StartDate.HasValue).Select(s => s.StartDate.Value).Min();
			DateTime spreadEndDate = laborTypes.Where(s => s.EndDate.HasValue).Select(s => s.EndDate.Value).Max();

			ICollection<DateTime> spreadDates = new List<DateTime>();

			// display the header (all dates across the spread)
			for (DateTime dt = spreadStartDate.Date; dt.Date <= spreadEndDate; dt = dt.AddMonths(1))
			{
				spreadDates.Add(dt);
			}

			return spreadDates;
		}
	}
}
