namespace GenBOE.PLD.Models
{
	using GenBOE.PLD.Models.Models;
	using System.Data.Entity;


	public class PldDBContext : DbContext
    {
		public PldDBContext() :
			base("name=PldDBContext")
		{
		}

		public virtual DbSet<PLDProposal> Proposals { get; set; }

    }
}
