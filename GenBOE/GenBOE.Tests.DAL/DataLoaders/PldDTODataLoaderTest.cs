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
		//private PldDBContext _context;
		private PldDTODataLoader _loader;

		[TestInitialize]
		public void Initialize()
		{
			//_context = new PldDBContext();
			_loader = new PldDTODataLoader();
		}

		[TestMethod]
		public void GetAllProposals_ReturnsProposals()
		{	
				
				ICollection<Dtos.ProposalDTO> proposals = _loader.GetTopProposals();
							
				Assert.IsNotNull(proposals);
				Assert.IsTrue(proposals.Count > 0);
			
		}

		[TestMethod]
		public void GetByIds_WithValidPaNumbers_ReturnsProposals()
		{
			
				string[] paNumbers = new[] { "A09D0040", "A09D0047" };

				// Act
				ICollection<Dtos.ProposalDTO> proposals = _loader.GetByIds(paNumbers);

				// Assert
				Assert.IsNotNull(proposals);
				Assert.IsTrue(proposals.Count > 0);
			
		}

		[TestMethod]
		public void GetByIds_WithNoPaNumbers_ReturnsEmptyList()
		{
				string[] paNumbers = new string[0];

				// Act
				ICollection<Dtos.ProposalDTO> proposals = _loader.GetByIds(paNumbers);

				// Assert
				Assert.IsNotNull(proposals);
				Assert.AreEqual(0, proposals.Count);
			
		}

		

		[TestMethod]
		public void GetAllActiveProposals_ReturnsActiveProposals()
		{			
			
				// Act
				ICollection<Dtos.ProposalDTO> proposals = _loader.GetAllActiveProposals();

				// Assert
				Assert.IsNotNull(proposals);
				Assert.IsTrue(proposals.Count > 0);
			
		}

		[TestMethod]
		public void GetAllActiveProposalNames_ReturnsActiveProposalNames()
		{
			
				// Act
				ICollection<string> proposalNames = _loader.GetAllActiveProposalNames();

				// Assert
				Assert.IsNotNull(proposalNames);
				Assert.IsTrue(proposalNames.Count > 0);
			
		}



	}
}
