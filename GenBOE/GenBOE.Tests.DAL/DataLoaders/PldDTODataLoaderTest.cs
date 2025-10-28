using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using GenBOE.DataBridge.DTO;
using GenBOE.PLD.Models;
using GenBOE.PLD.Models.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GenBOE.Tests.DAL.DataLoaders
{
	[TestClass]
	public class PldDTODataLoaderTest
	{		
		private PldDTODataLoader _loader;

		[TestInitialize]
		public void Initialize()
		{
			_loader = new PldDTODataLoader(new LineOfBusinessDataLoader());
		}

		[TestMethod]
		public void GetAllProposals_ReturnsProposals()
		{	
				
				ICollection<Dtos.PLDProposalDTO> proposals = _loader.GetTopProposals();
							
				Assert.IsNotNull(proposals);
				Assert.IsTrue(proposals.Count > 0);
			
		}

		[TestMethod]
		public void GetAllActiveProposals_ReturnsActiveProposals()
		{			
			
				ICollection<Dtos.PLDProposalDTO> proposals = _loader.GetAllActiveProposals();

				Assert.IsNotNull(proposals);
				Assert.IsTrue(proposals.Count > 0);
			
		}

		[TestMethod]
		public void GetAllActiveProposalNames_ReturnsActiveProposalNames()
		{

				ICollection<string> proposalNames = _loader.GetAllActiveProposalNames();

				Assert.IsNotNull(proposalNames);
				Assert.IsTrue(proposalNames.Count > 0);
			
		}

	}
}
