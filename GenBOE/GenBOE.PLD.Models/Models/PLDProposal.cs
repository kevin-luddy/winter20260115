using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenBOE.PLD.Models.Models
{
	[Table("V_PLD_GenBOE", Schema = "dbo")]
	public class PLDProposal
    {		
	
		[Column("PA Number")]
		public string PA_Number { get; set; }

		[Key]
		[Column("PA Version")]
		public short PA_Version { get; set; }

		[Column("PA Title")]
		public string PA_Title { get; set; }

		[Column("PA Description")]
		public string PA_Description { get; set; }

		[Column("Line of Business")]
		public string Line_of_Business { get; set; }

		[Column("Pricing")]
		public string Pricing { get; set; }

		[Column("Project Start Date")]
		public DateTime? Project_Start_Date { get; set; }

		[Column("Project End Date")]
		public DateTime? Project_End_Date { get; set; }

		[Column("RFP Number")]
		public string RFP_Number { get; set; }

		[Column("Last Modified Date")]
		public DateTime? Last_Modified_Date { get; set; }

		[Column("Proposal Status")]
		public string Proposal_Status { get; set; }	

		
	}






	
}
