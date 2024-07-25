namespace GenBOE.Dtos
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
			this.SkillMixID = -1;
			this.Rationale = string.Empty;
			this.Included = null;
			this.ProposedHours = 0;
			this.HistoricalHours = 0;
			this.BOESkillMix = 0;
			this.LaborSkillMix = 0;
			this.ResourceOld = string.Empty;
			this.ResourceNew = string.Empty;
			this.BOEID = -1;
			this.BOETaskElementID = -1;
			this.MOQTypeSelectionID = -1;
			this.IsPercentLocked = false;
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
		public bool? Included { get; set; }

		/// <summary>
		/// Get "Yes" or "No" string for Included, or an empty string if null
		/// </summary>
		public string IncludedString
		{
			get
			{
				if (Included.HasValue)
				{
					if (Included.Value)
					{
						return "Yes";
					}
					else
					{
						return "No";
					}
				}
				else
				{
					return string.Empty;
				}
			}
		}

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
		/// Foreign Key to MOQTypeSelectionID
		/// </summary>
		public int MOQTypeSelectionID { get; set; }

		/// <summary>
		/// Is the BOE Skill Mix Percent locked
		/// </summary>
		public bool IsPercentLocked { get; set; }
	}
}
