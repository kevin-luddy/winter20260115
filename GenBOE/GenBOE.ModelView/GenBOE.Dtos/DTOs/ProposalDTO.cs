

namespace GenBOE.Dtos
{
	using IES.Common;
	using System;
	using System.Diagnostics.CodeAnalysis;

	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class ProposalDTO : UpdateableDTO
	{
		
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

		public string PA_Number { get; set; }

		public short PA_Version { get; set; }

		public string PA_Title { get; set; }

		public string PA_Description { get; set; }

		public string Line_of_Business { get; set; }

		public string Pricing { get; set; }

		public DateTime? Project_Start_Date { get; set; }

		public DateTime? Project_End_Date { get; set; }

		public string RFP_Number { get; set; }

		public DateTime? Last_Modified_Date { get; set; }

		public string Proposal_Status { get; set; }

		
	}
}
