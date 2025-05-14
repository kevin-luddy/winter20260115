// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Common Disclosure Skill Mix DTO
	/// </summary>
	[Serializable()]
	[ExcludeFromCodeCoverage]
	public class CommonDisclosureSkillMixDTO
	{
		// default constructor
		public CommonDisclosureSkillMixDTO()
		{
			this.CommonDisclosureSkillMixID = -1;
			this.Rationale = String.Empty;
			this.Included = false;
			this.ProposedHours = 0m;
			this.HistoricalHours = 0m;
			this.BOESkillMix = 0m;
			this.LaborSkillMix = 0m;
			this.ResourceID = String.Empty;
			this.BusinessResourceID = String.Empty;
			this.BOEID = -1;
			this.BOETaskElementID = -1;
			this.IsUserInput = false;
			this.UCOTHours = 0m;
			this.GrandTotalHours = 0m;
		}

		/// <summary>
		/// Primary Key
		/// </summary>
		public int CommonDisclosureSkillMixID { get; set; }

		/// <summary>
		/// User rationale
		/// </summary>
		public string Rationale { get; set; }

		/// <summary>
		/// User input to determine if BOE Skill Mix is included
		/// </summary>
		public bool Included { get; set; }

		/// <summary>
		/// Proposed hours to match related Skill Mix equation
		/// </summary>
		public decimal ProposedHours { get; set; }

		/// <summary>
		/// Historical hours to match related Skill Mix equation
		/// </summary>
		public decimal HistoricalHours { get; set; }

		/// <summary>
		/// BOE SKill Mix percentage
		/// </summary>
		public decimal? BOESkillMix { get; set; }

		/// <summary>
		/// Labor Skill Mix percentage
		/// </summary>
		public decimal LaborSkillMix { get; set; }

		/// <summary>
		/// Resource ID matching related row in Skill Mix table if included is yes
		/// </summary>
		public string ResourceID { get; set; }

		/// <summary>
		/// BRC ID; Resource ID can have many
		/// </summary>
		public string BusinessResourceID { get; set; }

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

		/// <summary>
		/// UCOT Hours, Space only
		/// </summary>
		public decimal UCOTHours { get; set; }

		/// <summary>
		/// Grand Total Hours (proposed + UCOT Hours), Space only
		/// </summary>
		public decimal GrandTotalHours { get; set; }
	}
}
