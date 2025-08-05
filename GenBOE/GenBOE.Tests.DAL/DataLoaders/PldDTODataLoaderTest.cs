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

		private Mock<PldDBContext> _mockContext;
		private PldDTODataLoader _dataLoader;

		[TestInitialize]
		public void Initialize()
		{
			_mockContext = new Mock<PldDBContext>();
			_dataLoader = new PldDTODataLoader();
		}


		[TestMethod]
		public void GetAllProposals_ReturnsProposals()
		{
			// Arrange
			using (PldDBContext context = new PldDBContext())
			{
				PldDTODataLoader sut = new PldDTODataLoader(context);

				// Act
				ICollection<Dtos.ProposalDTO> proposals = sut.GetTopProposals();

				// Assert
				Assert.IsNotNull(proposals);
				Assert.IsTrue(proposals.Count > 0);
			}
		}

		[TestMethod]
		public void GetByIds_WithValidPaNumbers_ReturnsProposals()
		{
			// Arrange
			using (PldDBContext context = new PldDBContext())
			{
				PldDTODataLoader sut = new PldDTODataLoader(context);
				string[] paNumbers = new[] { "A09D0040", "A09D0047" };

				// Act
				ICollection<Dtos.ProposalDTO> proposals = sut.GetByIds(paNumbers);

				// Assert
				Assert.IsNotNull(proposals);
				Assert.IsTrue(proposals.Count > 0);
			}
		}

		[TestMethod]
		public void GetByIds_WithNoPaNumbers_ReturnsEmptyList()
		{
			// Arrange
			using (PldDBContext context = new PldDBContext())
			{
				PldDTODataLoader sut = new PldDTODataLoader(context);
				string[] paNumbers = new string[0];

				// Act
				ICollection<Dtos.ProposalDTO> proposals = sut.GetByIds(paNumbers);

				// Assert
				Assert.IsNotNull(proposals);
				Assert.AreEqual(0, proposals.Count);
			}
		}

		

		[TestMethod]
		public void GetAllActiveProposals_ReturnsActiveProposals()
		{
			// Arrange
			using (PldDBContext context = new PldDBContext())
			{
				PldDTODataLoader sut = new PldDTODataLoader(context);

				// Act
				ICollection<Dtos.ProposalDTO> proposals = sut.GetAllActiveProposals();

				// Assert
				Assert.IsNotNull(proposals);
				Assert.IsTrue(proposals.Count > 0);
			}
		}

		[TestMethod]
		public void GetAllActiveProposalNames_ReturnsActiveProposalNames()
		{
			// Arrange
			using (PldDBContext context = new PldDBContext())
			{
				PldDTODataLoader sut = new PldDTODataLoader(context);

				// Act
				ICollection<string> proposalNames = sut.GetAllActiveProposalNames();

				// Assert
				Assert.IsNotNull(proposalNames);
				Assert.IsTrue(proposalNames.Count > 0);
			}
		}



	}
}
