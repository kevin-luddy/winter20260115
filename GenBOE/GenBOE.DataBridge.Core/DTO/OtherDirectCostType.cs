using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using IES.Common;
using System.Collections.ObjectModel;
using IES.Common.Core.Models;
using GenBOE.DataBridge.Core.DTO.Common;
using IES.Common.Core.Interfaces;
using IES.Common.Core.Enums;

namespace GenBOE.DataBridge.Core.DTO
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class OtherDirectCostType : UpdateableDTO, IBOEMembership, IStartEndDates
	{
		public OtherDirectCostType()
		{
			ODCTypeID = -1;
			ResourceID = 0;
			PerformingOrgID = 0;
			SpreadCurve = SpreadCurves.DiscreteHours;
			StartDate = DateTime.MinValue;
			EndDate = DateTime.MinValue;
			ODCSpreads = new Collection<OtherDirectCostSpread>();
		}

		public OtherDirectCostType(OtherDirectCostType inODCType) : this()
		{
			if (inODCType != null)
			{
				ODCTypeID = inODCType.ODCTypeID;
				ResourceID = inODCType.ResourceID;
				PerformingOrgID = inODCType.PerformingOrgID;
				SpreadCurve = inODCType.SpreadCurve;
				StartDate = inODCType.StartDate;
				EndDate = inODCType.EndDate;
				ODCSpreads = inODCType.ODCSpreads;
				Cost = inODCType.Cost;
			}
		}


		public int? ODCTypeID { get; set; }

		// the resource code ID
		public int? ResourceID { get; set; }

		// the performing organization ID
		public int? PerformingOrgID { get; set; }

		public SpreadCurves SpreadCurve { get; set; }

		// the start date
		public DateTime? StartDate { get; set; }

		// the end date
		public DateTime? EndDate { get; set; }

		public Collection<OtherDirectCostSpread> ODCSpreads { get; set; }

		public int BoeID { get; set; }

		public decimal? Cost { get; set; }
	}
}
