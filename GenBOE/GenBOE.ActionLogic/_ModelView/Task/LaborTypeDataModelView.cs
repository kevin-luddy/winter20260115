// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Linq;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.Dtos;
	using IES.Common;

	/// <summary>
	/// Labor Type Data for new Angular front-end
	/// </summary>
	/// <seealso cref="GenBOE.ActionLogic.ModelView.PersistedDataModelView" />
	public class LaborTypeDataModelView : PersistedDataModelView
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public LaborTypeDataModelView()
		{
			this.BOELaborTypeID = -1;
			this.ResourceID = null;
			this.ResourceName = String.Empty;
			this.ResourceType = String.Empty;
			this.ResourceDescription = String.Empty;
			this.BusinessResourceCodeID = null;
			this.BusinessResourceCodeName = string.Empty;
			this.BusinessResourceCodeType = string.Empty;
			this.BusinessResourceCodeDescription = string.Empty;
			this.PerformingOrgID = 0;
			this.PerformingOrgName = String.Empty;
			this.SpreadCurveID = SpreadCurves.DiscreteHours;
			this.RateType = RateType.Hours;
			this.PercentSpread = 0;
			this.HourSpread = 0;
			this.UcotHours = 0;
			this.CostSpread = 0;
			this.StartDate = "01/1970";
			this.EndDate = "01/1970";
			this.UpdateUserID = 0;
			this.Deleted = false;
			this.BOETaskElementID = 0;
			this.CustomFieldValues = new List<CustomFieldSelectionModelView>();
			this.HourSpreadLocked = false;
			this.PercentSpreadLocked = false;
			this.WBSID = -1;
			this.CLINID = -1;
			this.Spreads = new List<LaborSpreadDataModelView>();
			this.UcotSpreads = new List<LaborSpreadDataModelView>();
			this.NewLaborType = false;
			this.LaborTypeOrder = 2000; // New resource types should be put at bottom of order
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="inBoeLaborType">The <see cref="ResourceTypeDto"/> object used to populate properties</param>
		/// <param name="inResource">The <see cref="ResourceDTO"/> object used to populate properties</param>
		/// <param name="inBusinessResourceCode"> The <see cref="ResourceDTO"/> object used to populate properties</param>
		/// <param name="perfOrg">The <see cref="PerformingOrgDTO" /> object used to populate properties.</param>
		/// <param name="calculateUCOT">Whether to calculate UCOT</param>
		/// <param name="ucotFactor">The UCOT Factor</param>
		/// <param name="precision">The workspace hours precision</param>
		public LaborTypeDataModelView(ResourceTypeDto inBoeLaborType, ResourceDTO inResource, ResourceDTO inBusinessResourceCode, PerformingOrgDTO perfOrg, decimal ucotFactor, bool calculateUCOT, int precision)
			: this()
		{
			if (inBoeLaborType == null) { throw new ArgumentNullException(nameof(inBoeLaborType)); }
			if (inResource == null) { throw new ArgumentNullException(nameof(inResource)); }
			if (inBusinessResourceCode == null) { throw new ArgumentNullException(nameof(inBusinessResourceCode)); }
			if (perfOrg == null)
			{
				throw new ArgumentNullException(nameof(perfOrg));
			}

			this.BOELaborTypeID = inBoeLaborType.Id;
			this.ResourceID = inBoeLaborType.ResourceID;
			this.BusinessResourceCodeID = inBoeLaborType.BusinessResourceCodeID;
			this.PerformingOrgID = inBoeLaborType.PerformingOrgID;
			this.PerformingOrgName = perfOrg.PerformingOrgName;
			this.SpreadCurveID = inBoeLaborType.SpreadCurveID;
			this.PercentSpread = inBoeLaborType.PercentSpread;

			this.ResourceName = inResource.ResourceName;
			this.ResourceType = inResource.ResourceTypeCategory;
			this.ResourceDescription = inResource.ResourceDesc;
			this.RateType = inResource.RateType;

			if (this.RateType == RateType.NotSet)
			{
				this.RateType = inBusinessResourceCode.RateType;
			}

			this.BusinessResourceCodeName = inBusinessResourceCode.ResourceName;
			this.BusinessResourceCodeType = inBusinessResourceCode.ResourceTypeCategory;
			this.BusinessResourceCodeDescription = inBusinessResourceCode.ResourceDesc;
			this.CanOffload = inBoeLaborType.CanOffload;
			this.TieredPercentage = inBoeLaborType.TieredPercentage;
			this.ElementOfCost = (int)inResource?.ElementOfCost;
			if (this.ElementOfCost == 0)
			{
				this.ElementOfCost = (int)inBusinessResourceCode.ElementOfCost;
			}
			this.LaborTypeOrder = inBoeLaborType.LaborTypeOrder;

			// Hours/Cost is based on the resource type.
			if (this.RateType == RateType.Hours)
			{
				this.HourSpread = inBoeLaborType.ValueSpread.HasValue ? (decimal?)Convert.ToDecimal(inBoeLaborType.ValueSpread.Value) : null;
			}
			else if (this.RateType == RateType.Cost)
			{
				this.CostSpread = inBoeLaborType.ValueSpread.HasValue ? (decimal?)Convert.ToDecimal(inBoeLaborType.ValueSpread.Value) : null;
			}

			this.StartDate = inBoeLaborType.StartDate.Value.ToString("MM/yyyy");
			this.EndDate = inBoeLaborType.EndDate.Value.ToString("MM/yyyy");
			this.UpdateDate = inBoeLaborType.UpdateDate;
			this.PercentSpreadLocked = inBoeLaborType.PercentSpreadLocked;
			this.HourSpreadLocked = inBoeLaborType.HourSpreadLocked;
			this.WBSID = inBoeLaborType.WBSID;
			this.CLINID = inBoeLaborType.CLINID;

			if (inBoeLaborType.LaborSpreads != null && inBoeLaborType.LaborSpreads.Any())
			{
				this.Spreads = inBoeLaborType.LaborSpreads.Select(ls => new LaborSpreadDataModelView
				{
					LaborSpreadDate = ls.LaborSpreadDate.ToMonthString(),
					LaborSpreadValue = ls.LaborSpreadValue,
					UpdateDate = ls.UpdateDate,
					UpdateDateLong = ls.UpdateDateLong
				}).ToList();

				if (calculateUCOT)
				{
					this.UcotSpreads = new List<LaborSpreadDataModelView>();
					decimal sumUCOT = 0m;
					foreach (ResourceSpreadDto dto in inBoeLaborType.LaborSpreads.OrderBy(l => l.LaborSpreadDate))
					{
						if (dto.LaborSpreadDate >= Utilities.OneLmxStartDate)
						{
							decimal nonPrecisionUCOT = dto.LaborSpreadValue * ucotFactor / 100.0m;
							sumUCOT += nonPrecisionUCOT;
							decimal precisionUCOT = Utilities.AdjustPrecision(nonPrecisionUCOT, precision);

							this.UcotSpreads.Add(new LaborSpreadDataModelView()
							{
								LaborSpreadDate = dto.LaborSpreadDate.ToMonthString(),
								LaborSpreadValue = precisionUCOT,
								UpdateDate = dto.UpdateDate,
								UpdateDateLong = dto.UpdateDateLong
							});
						}
					}

					this.UcotHours = Utilities.AdjustPrecision(sumUCOT, precision);

					// Now smooth the UCOT Hours
					if (this.UcotSpreads.Any())
					{
						decimal[] ucotSpreadValues = SpreadCurve.Smooth(this.UcotHours ?? 0m, this.UcotSpreads.Select(s => s.LaborSpreadValue ?? 0m).ToArray(), 0, this.UcotSpreads.Count, precision);

						// Reset the values to the Smooth'ed array to guarantee precision and no loss of rounding values
						for (int i = 0; i < this.UcotSpreads.Count; i++)
						{
							this.UcotSpreads.ElementAt(i).LaborSpreadValue = ucotSpreadValues[i];
						}
					}
				}
				else
				{
					this.UcotSpreads = new List<LaborSpreadDataModelView>();
					this.UcotHours = 0m;
				}
			}
			else
			{
				this.Spreads = new List<LaborSpreadDataModelView>();
				this.UcotSpreads = new List<LaborSpreadDataModelView>();
				this.UcotHours = 0m;
			}
		}

		/// <summary>
		/// Gets or sets the spreads.
		/// </summary>
		public ICollection<LaborSpreadDataModelView> Spreads { get; set; }

		/// <summary>
		/// Gets or sets the UCOT spreads.
		/// </summary>
		public ICollection<LaborSpreadDataModelView> UcotSpreads { get; set; }
		
		/// <summary>
		/// Gets/Sets BOELaborTypeID
		/// </summary>
		public int? BOELaborTypeID { get; set; }

		/// <summary>
		/// Gets/Sets ResourceID
		/// </summary>
		public int? ResourceID { get; set; }

		/// <summary>
		/// Gets/Sets ResourceName
		/// </summary>
		public string ResourceName { get; set; }

		/// <summary>
		/// Gets/Sets Resource Type (e.g. E1, T2, Subcontractor)
		/// </summary>
		public string ResourceType { get; set; }

		/// <summary>
		/// Gets/Sets ResourceDescription
		/// </summary>
		public string ResourceDescription { get; set; }

		/// <summary>
		/// Gets/Sets BusinessResourceCodeID
		/// </summary>
		public int? BusinessResourceCodeID { get; set; }

		/// <summary>
		/// Gets/Sets BusinessResourceCodeName
		/// </summary>
		public string BusinessResourceCodeName { get; set; }

		/// <summary>
		/// Gets/Sets BusinessResourceCodeType
		/// </summary>
		public string BusinessResourceCodeType { get; set; }

		/// <summary>
		/// Gets/Sets BusinessResourceCodeDescription
		/// </summary>
		public string BusinessResourceCodeDescription { get; set; }

		/// <summary>
		/// Gets/Sets PerformingOrgID
		/// </summary>
		public int? PerformingOrgID { get; set; }

		/// <summary>
		/// Gets/Sets PerformingOrgName
		/// </summary>
		public string PerformingOrgName { get; set; }

		/// <summary>
		/// Gets/Sets SpreadCurveID
		/// </summary>
		[Display(Name = "Spread Curve")]
		public SpreadCurves? SpreadCurveID { get; set; }

		/// <summary>
		/// Rate type - based on the Resource Type (Cost or Hours).
		/// </summary>
		public RateType RateType { get; set; }

		/// <summary>
		/// Gets whether this item has cost disabled, used for cost input.
		/// </summary>
		public bool IsCostDisabled
		{
			get
			{
				bool disabled = true;
				if (this.RateType == RateType.Cost && (this.SpreadCurveID != null && this.SpreadCurveID.Value != SpreadCurves.DiscreteCost))
				{
					// Enable cost input.
					disabled = false;
				}

				return disabled;
			}
		}

		/// <summary>
		/// Gets whether this item has hours disabled, used for hours input.
		/// </summary>
		public bool IsHoursDisabled
		{
			get
			{
				bool disabled = true;
				if (this.RateType == RateType.Hours && (this.SpreadCurveID != null && this.SpreadCurveID.Value != SpreadCurves.DiscreteHours))
				{
					// Enable cost input.
					disabled = false;
				}

				return disabled;
			}
		}

		/// <summary>
		/// Gets/Sets PercentSpread
		/// </summary>
		[Display(Name = "Percent Spread")]
		public decimal? PercentSpread { get; set; }

		/// <summary>
		/// Gets/Sets HourSpread
		/// </summary>
		[Display(Name = "Hours Spread")]
		public decimal? HourSpread { get; set; }

		/// <summary>
		/// Gets/Sets Ucot Hours
		/// </summary>
		[Display(Name = "UCOT Hours")]
		public decimal? UcotHours { get; set; }

		/// <summary>
		/// Gets/Sets CostSpread
		/// </summary>
		[RegularExpression(@"(^[+-]?\d{0,10}([.]\d{1,2})?$)", ErrorMessage = "Cost Spread must be between -9,999,999,999.99 and 9,999,999,999.99 and contain only 2 decimal places.")]
		[Display(Name = "Cost")]
		public decimal? CostSpread { get; set; }

		/// <summary>
		/// Gets/Sets StartDate
		/// </summary>
		[RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Start Date must be in MM/YYYY format.")]
		[DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
		public string StartDate { get; set; }

		/// <summary>
		/// Gets/Sets EndDate
		/// </summary>
		[RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "End Date must be in MM/YYYY format.")]
		[DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
		public string EndDate { get; set; }

		/// <summary>
		/// Gets/Sets UpdateUserID
		/// </summary>
		public int UpdateUserID { get; set; }

		/// <summary>
		/// Gets/Sets Deleted
		/// </summary>
		public bool Deleted { get; set; }

		/// <summary>
		/// Gets/Sets BOETaskElementID
		/// </summary>
		public int? BOETaskElementID { get; set; }

		/// <summary>
		/// Gets/Sets CustomFieldValues
		/// </summary>
		public ICollection<CustomFieldSelectionModelView> CustomFieldValues { get; set; }

		/// <summary>
		/// Gets/Sets ElementOfCost
		/// </summary>
		[Display(Name = "Element of Cost")]
		public int ElementOfCost { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance can offload.
		/// </summary>
		public bool CanOffload { get; set; }

		/// <summary>
		/// Gets or sets the tiered percentage value for project map workspaces.
		/// </summary>
		public decimal? TieredPercentage { get; set; }

		/// <summary>
		/// Gets/Sets PercentSpreadLocked
		/// Percent Spread Locked, true is locked, false is unlocked
		/// </summary>
		public bool PercentSpreadLocked { get; set; }

		/// <summary>
		/// Gets/Sets HourSpreadLocked
		/// Hour Spread Locked, true if locked, false is unlocked
		/// </summary>
		public bool HourSpreadLocked { get; set; }

		/// <summary>
		/// Gets/Sets Resource Type WBSID
		/// </summary>
		public int? WBSID { get; set; }

		/// <summary>
		/// Gets/Sets Resource Type CLINID
		/// </summary>
		public int? CLINID { get; set; }

		/// <summary>
		/// Gets/Sets bool noting if the labor type is the new blank row in the resource types table
		/// </summary>
		public bool NewLaborType { get; set; }

		/// <summary>
		/// The value of the order in which the resource will appear in the task listing
		/// </summary>
		public int LaborTypeOrder { get; set; }
	}

	/// <summary>
	/// Model view for extensions to LMLaborType
	/// </summary>
	public static class LaborTypeDataModelViewExtensions
	{
		/// <summary>
		/// Converts a <see cref="LaborTypeDataModelView"/> to a <see cref="ResourceTypeDto"/>
		/// </summary>
		/// <param name="laborType">The <see cref="LMLaborTypeModelView"/> object to convert</param>
		/// <param name="boeId">The Boe ID</param>
		/// <returns>The <see cref="ResourceTypeDto"/></returns>
		public static ResourceTypeDto ToBOELaborType(this LaborTypeDataModelView laborType, int? boeId)
		{
			if (laborType == null)
			{
				throw new ArgumentNullException(nameof(laborType));
			}

			DateTime startDate;
			DateTime endDate;
			if (!DateTime.TryParse(laborType.StartDate, out startDate))
			{
				startDate = DateTime.MinValue;
			}
			if (!DateTime.TryParse(laborType.EndDate, out endDate))
			{
				endDate = DateTime.MinValue;
			}

			decimal? spreadValue;
			if (laborType.RateType == RateType.Hours)
			{
				spreadValue = laborType.HourSpread.HasValue ? laborType.HourSpread.Value : (decimal?)null;
			}
			else
			{
				// Spread cost
				spreadValue = laborType.CostSpread.HasValue ? laborType.CostSpread.Value : (decimal?)null;
			}

			// BOEJ-792 - This ResourceTypeDto was being initialized incorrectly. TaskElementId was being set to
			// the laborType.BOELaborTypeID and the Id was not being set at all.  Now set the Id properly to
			// laborType.BOELaborTypeID and TaskElementId is set to laborType.BOETaskElementID.
			ResourceTypeDto result = new ResourceTypeDto
			{
				BoeID = boeId.HasValue ? boeId.Value : -1,
				Id = laborType.BOELaborTypeID.HasValue ? laborType.BOELaborTypeID.Value : -1,
				TaskElementId = laborType.BOETaskElementID.HasValue ? laborType.BOETaskElementID.Value : -1,
				CustomFieldValueContainers = null,
				EndDateValue = endDate.Normalize(DateTimePrecision.Month),
				ValueSpread = spreadValue,
				HourSpreadLocked = laborType.HourSpreadLocked,
				LaborSpreads = null,
				PercentSpread = laborType.PercentSpread,
				PercentSpreadLocked = laborType.PercentSpreadLocked,
				PerformingOrgID = laborType.PerformingOrgID,
				ResourceID = laborType.ResourceID,
				BusinessResourceCodeID = laborType.BusinessResourceCodeID,
				SpreadCurveID = laborType.SpreadCurveID,
				StartDateValue = startDate.Normalize(DateTimePrecision.Month),
				Updateable = UpdateType.None,
				UpdateDate = laborType.UpdateDate,
				SpreadType = laborType.RateType == RateType.Hours ? SpreadType.Hours : SpreadType.Cost,
				WBSID = laborType.WBSID,
				CLINID = laborType.CLINID
			};

			return result;
		}

		/// <summary>
		/// Gets Non Deleted Resource Entries
		/// </summary>
		/// <param name="allResourceEntries">The collection of <see cref="LaborTypeDataModelView"/></param>
		/// <returns>The collection of <see cref="LaborTypeDataModelView"/> that are not deleted</returns>
		public static ICollection<LaborTypeDataModelView> GetNonDeletedResourceEntries(this ICollection<LaborTypeDataModelView> allResourceEntries)
		{
			return allResourceEntries.Where(r => !r.Deleted).ToList();
		}
	}
}
