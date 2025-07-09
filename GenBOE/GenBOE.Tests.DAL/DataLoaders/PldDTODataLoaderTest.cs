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

		//[TestMethod]
		//public void GetByWorkspaceId_ValidWorkspaceId_ReturnsProposalDTOs()
		//{
		//	// Arrange
		//	int workspaceId = 1;
		//	List<PLDProposal> proposals = new List<PLDProposal>
		//	{
		//		new PLDProposal { PA_Number = "PA-1" },
		//		new PLDProposal { PA_Number = "PA-2" }
		//	};

		//	Mock<PldDBContext> mockContext = new Mock<PldDBContext>();
		//	Mock<DbSet<PLDProposal>> mockSet = new Mock<DbSet<PLDProposal>>();
		//	mockSet.As<IQueryable<PLDProposal>>().Setup(m => m.Provider).Returns(proposals.AsQueryable().Provider);
		//	mockSet.As<IQueryable<PLDProposal>>().Setup(m => m.Expression).Returns(proposals.AsQueryable().Expression);
		//	mockSet.As<IQueryable<PLDProposal>>().Setup(m => m.ElementType).Returns(proposals.AsQueryable().ElementType);
		//	mockSet.As<IQueryable<PLDProposal>>().Setup(m => m.GetEnumerator()).Returns(() => proposals.GetEnumerator());

		//	mockContext.Setup(c => c.Proposals).Returns(mockSet.Object);

		//	PldDTODataLoader dataLoader = new PldDTODataLoader(mockContext.Object);

		//	// Act
		//	List<Dtos.ProposalDTO> result = dataLoader.GetByWorkspaceId(workspaceId);

		//	// Assert
		//	Assert.IsNotNull(result);
		//	Assert.AreEqual(2, result.Count);
		//	Assert.AreEqual("PA-1", result[0].PA_Number);
		//	Assert.AreEqual("PA-2", result[1].PA_Number);
		//}


		[TestMethod]
		public void GetAllProposals_ReturnsProposals()
		{
			// Arrange
			using (PldDBContext context = new PldDBContext())
			{
				PldDTODataLoader sut = new PldDTODataLoader(context);

				// Act
				ICollection<Dtos.ProposalDTO> proposals = sut.GetAllProposals();

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
				ICollection<Dtos.ProposalDTO> proposals = sut.GetAllActiveProposals(1);

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
				ICollection<string> proposalNames = sut.GetAllActiveProposalNames(1);

				// Assert
				Assert.IsNotNull(proposalNames);
				Assert.IsTrue(proposalNames.Count > 0);
			}
		}



	}
}
