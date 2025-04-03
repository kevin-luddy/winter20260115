using System.Diagnostics.CodeAnalysis;
using IES.Common.Core;
using IES.Common.Core.Enums;
using IES.Common.Core.Interfaces;
using IES.Common.Core.Models;

namespace GenBOE.DataBridge.Core.DTO
{
	[Serializable()]
	[ExcludeFromCodeCoverage]
	public class ResourceDTO : UpdateableDTO, ICachableDTO
	{
		public ResourceDTO()
		{
			Id = -1;
			ResourceName = string.Empty;
			ResourceDesc = string.Empty;
			LaborType = string.Empty;
			SegRegion = string.Empty;
			BurdenPool = string.Empty;
			ElementOfCost = ElementOfCostType.NotSet;
			Segment = SegmentType.None;
			RateType = RateType.NotSet;
			CalculatedSegment = SegmentType.None;
			Deleted = false;
			isSystemResource = false;
		}

		public string ResourceName { get; set; }
		public string ResourceDesc { get; set; }
		public string LaborType { get; set; }
		public string SegRegion { get; set; }
		public string BurdenPool { get; set; }
		public ElementOfCostType ElementOfCost { get; set; }
		public SegmentType Segment { get; set; }
		public RateType RateType { get; set; }
		public SegmentType CalculatedSegment { get; set; }
		public bool Deleted { get; set; }

		// used only to determine if this resource is a system or workspace resource for caching purposes
		public bool isSystemResource { get; set; }

		//HACK
		// there is an issue with a not null enums and this gets around it
		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
		internal int? SegmentTypeInt { set { Segment = value.HasValue ? (SegmentType)value.Value : SegmentType.None; } }

		public int GetPrimaryKeyID()
		{
			return Id;
		}

		/// <summary>
		/// Gets the string representation of the Rate Type.
		/// </summary>
		public string RateTypeString
		{
			get
			{
				string rateTypeString = string.Empty;
				rateTypeString = RateType.ToString();

				return rateTypeString;
			}
		}

		public string ResourceTypeCategory
		{
			get
			{
				string resourceTypeCategory;

				switch (ElementOfCost)
				{
					case ElementOfCostType.IWTA:
					case ElementOfCostType.Sub:
					case ElementOfCostType.Materials:
					case ElementOfCostType.ODC:
					case ElementOfCostType.Travel:
						resourceTypeCategory = ElementOfCost.GetResourceTypeCategory();
						break;

					default:
						resourceTypeCategory = LaborType;
						break;
				}

				return resourceTypeCategory;
			}
		}
	}
}
