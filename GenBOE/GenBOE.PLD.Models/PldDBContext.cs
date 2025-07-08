using GenBOE.PLD.Models.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenBOE.PLD.Models
{
    public class PldDBContext : DbContext
    {
		public PldDBContext() :
			base("name=PldDBContext")
		{
		}

		public virtual DbSet<PLDProposal> Proposals { get; set; }

    }
}
