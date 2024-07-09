namespace GenBOE.Dtos
{
	using System;

	/// <summary>
	/// DTO for MOQ Type Selection Table Data Resource Hours.
	/// </summary>
	[Serializable()]
	public class MOQTypeSelectionTableDataResourceHoursDTO
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public MOQTypeSelectionTableDataResourceHoursDTO()
		{
			this.MOQTypeSelectionTableDataResourceHoursId = -1;
			this.ResourceName = string.Empty;
			this.WbsHours = 0;
			this.TotalHours = 0;
			this.MOQTypeSelectionTableDataId = 0;
			this.BOETaskElementID = -1;
			this.BOEID = -1;
		}

		/// <summary>
		/// MOQ Type Selection Table Data Resource Hours primary key
		/// </summary>
		public int MOQTypeSelectionTableDataResourceHoursId { get; set; }

		/// <summary>
		/// Resource
		/// </summary>
		public string ResourceName { get; set; }

		/// <summary>
		/// Wbs Hours
		/// </summary>
		public decimal WbsHours { get; set; }

		/// <summary>
		/// Total Hours
		/// </summary>
		public decimal TotalHours { get; set; }

		/// <summary>
		/// MOQ Type Selection Table Data Id FK
		/// </summary>
		public int MOQTypeSelectionTableDataId { get; set; }

		/// <summary>
		/// BOE Task Element Id FK
		/// </summary>
		public int BOETaskElementID { get; set; }

		/// <summary>
		/// BOE Id FK
		/// </summary>
		public int BOEID { get; set; }
	}
}