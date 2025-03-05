namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Collections.ObjectModel;

	/// <summary>
	/// Subclass of ResourceTypeDto that captures additional details specific to off-loaded resources
	/// </summary>
	[Serializable]
	public class SubResourceTypeDto : ResourceTypeDto
	{
		/// <summary>
		/// In-house Resource Id (used to identify original in-house resource id)
		/// </summary>
		public int? InHouseResource { get; set; }

		/// <summary>
		/// Gets or sets the In-house resource type identifier (used to identify original in-house resource type).
		/// </summary>
		public int InHouseResourceTypeId { get; set; }

		/// <summary>
		/// Sub resource name
		/// </summary>
		public string SubResourceName { get; set; }

		/// <summary>
		/// In-house labor spreads (hours)
		/// </summary>
		public Collection<ResourceSpreadDto> InHouseLaborSpreads { get; set; }

		/// <summary>
		/// Off Loaded Hour Spreads
		/// </summary>
		public Collection<ResourceSpreadDto> OffLoadedHourSpreads { get; set; }

	}
}