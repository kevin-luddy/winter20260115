namespace GenBOE.DataBridge.GenBOE.Dtos.DTOs
{
	using IES.Common;
	using IES.Common.Interfaces;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	[Serializable()]
	[ExcludeFromCodeCoverage]
	public class BusinessResourceCodeDTO : UpdateableDTO, ICachableDTO
	{
		public BusinessResourceCodeDTO()
		{
			Id = -1;
			BusinessResourceCodeName = string.Empty;
			BusinessResourceCodeDesc = string.Empty;
			LaborType = string.Empty;
			SegRegion = string.Empty;
			BurdenPool = string.Empty;
			ElementOfCost = ElementOfCostType.NotSet;
			Segment = SegmentType.None;
			RateType = RateType.NotSet;
			CalculatedSegment = SegmentType.None;
			isSystemBusinessResourceCode = false;
		}

		public string BusinessResourceCodeName { get; set; }
		public string BusinessResourceCodeDesc { get; set; }
		public string LaborType { get; set; }
		public string SegRegion { get; set; }
		public string BurdenPool { get; set; }
		public ElementOfCostType ElementOfCost { get; set; }
		public SegmentType Segment { get; set; }
		public RateType RateType { get; set; }
		public SegmentType CalculatedSegment { get; set; }

		// used only to determine if this business resource code is a system or workspace resource for caching purposes
		public bool isSystemBusinessResourceCode { get; set; }

		//HACK
		// there is an issue with a not null enums and this gets around it
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
		internal int? SegmentTypeInt { set { Segment = value.HasValue ? (SegmentType)value.Value : SegmentType.None; } }

		public int GetPrimaryKeyID()
		{
			return this.Id;
		}

		/// <summary>
		/// Gets the string representation of the Rate Type.
		/// </summary>
		public string RateTypeString
		{
			get
			{
				string rateTypeString = String.Empty;
				rateTypeString = this.RateType.ToString();

				return rateTypeString;
			}
		}

		public string BusinessResourceCodeTypeCategory
		{
			get
			{
				string resourceTypeCategory;

				switch (this.ElementOfCost)
				{
					case ElementOfCostType.IWTA:
					case ElementOfCostType.Sub:
					case ElementOfCostType.Materials:
					case ElementOfCostType.ODC:
					case ElementOfCostType.Travel:
						resourceTypeCategory = this.ElementOfCost.GetResourceTypeCategory();
						break;

					default:
						resourceTypeCategory = this.LaborType;
						break;
				}

				return resourceTypeCategory;
			}
		}
	}
}
