using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenBOE.PLD.Models;
using GenBOE.PLD.Models.Models;

namespace GenBOEConsoleApp
{
    class Program
    {
		static void Main(string[] args)
		{
			using (PldDBContext context = new PldDBContext())
			{
				List<Proposal> proposals = context.Proposals.Take(10).ToList();

				foreach(Proposal proposal in proposals)
				{
					Console.WriteLine($"PA:{proposal.PA_Number}");
				}


			}

			Console.ReadLine();

		}

	
    }

    
}
