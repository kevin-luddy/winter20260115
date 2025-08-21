

namespace GenBOE.Dtos
{
	using IES.Common;
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Proposal DTO 
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class ProposalDTO
	{
		/// <summary>
		/// Default ctor
		/// </summary>
		public ProposalDTO()
		{
			PA_Number = string.Empty;
			PA_Version = -1;
			PA_Title = string.Empty;
			PA_Description = string.Empty;
			Line_of_Business = string.Empty;
			Pricing = string.Empty;
			Project_Start_Date = DateTime.MinValue;
			Project_End_Date = DateTime.MinValue;
			RFP_Number = string.Empty;
			Last_Modified_Date = DateTime.MinValue;
			Proposal_Status = string.Empty;
		}

		/// <summary>
		/// Line of Business ID
		/// </summary>
		public int? Line_of_Business_ID { get; set; }

		/// <summary>
		///  PA Number 
		/// </summary>
		public string PA_Number { get; set; }

		/// <summary>
		/// PA Version
		/// </summary>
		public short PA_Version { get; set; }

		/// <summary>
		/// PA Title
		/// </summary>
		public string PA_Title { get; set; }

		/// <summary>
		/// PA Description
		/// </summary>
		public string PA_Description { get; set; }

		/// <summary>
		///  Line of Business
		/// </summary>
		public string Line_of_Business { get; set; }

		/// <summary>
		/// Price  - by Name
		/// </summary>
		public string Pricing { get; set; }

		/// <summary>
		///  Project Start Date
		/// </summary>
		public DateTime? Project_Start_Date { get; set; }

		/// <summary>
		/// Project End Date
		/// </summary>
		public DateTime? Project_End_Date { get; set; }

		/// <summary>
		///  RFP Number
		/// </summary>
		public string RFP_Number { get; set; }

		/// <summary>
		/// Last Modified Date
		/// </summary>
		public DateTime? Last_Modified_Date { get; set; }

		/// <summary>
		/// Proposal Status
		/// </summary>
		public string Proposal_Status { get; set; }

	}
}
