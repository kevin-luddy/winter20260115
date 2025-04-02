namespace GenBOE.DataBridge.Core.DTO
{
	using System;

	/// <summary>
	/// DTO for SkillMix table data
	/// </summary>
	[Serializable()]
	public class SkillMixDTO
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public SkillMixDTO()
		{
			SkillMixID = -1;
			Rationale = string.Empty;
			Included = false;
			ProposedHours = 0;
			HistoricalHours = 0;
			BOESkillMix = 0;
			LaborSkillMix = 0;
			ResourceOld = string.Empty;
			ResourceNew = string.Empty;
			BOEID = -1;
			BOETaskElementID = -1;
			IsUserInput = false;
		}

		/// <summary>
		/// Skill Mix primary key
		/// </summary>
		public int SkillMixID { get; set; }

		/// <summary>
		/// User type rationale
		/// </summary>
		public string Rationale { get; set; }

		/// <summary>
		/// Check if historical resource proposed
		/// </summary>
		public bool Included { get; set; }

		/// <summary>
		/// User/Calculated Hours of proposed percents
		/// </summary>
		public decimal ProposedHours { get; set; }

		/// <summary>
		/// Ledger hours by resource
		/// </summary>
		public decimal HistoricalHours { get; set; }

		/// <summary>
		/// User/Calculated % of proposed hours
		/// </summary>
		public decimal? BOESkillMix { get; set; }

		/// <summary>
		/// % of Total hours in Ledgers
		/// </summary>
		public decimal LaborSkillMix { get; set; }

		/// <summary>
		/// Ledger Resource
		/// </summary>
		public string ResourceOld { get; set; }

		/// <summary>
		/// Ledger resource converted to today's resource
		/// </summary>
		public string ResourceNew { get; set; }

		/// <summary>
		/// Foreign Key to BOE table
		/// </summary>
		public int BOEID { get; set; }

		/// <summary>
		/// Foreign Key to BOETaskElement table
		/// </summary>
		public int BOETaskElementID { get; set; }

		/// <summary>
		/// Is this data User Input
		/// </summary>
		public bool IsUserInput { get; set; }
	}
}
