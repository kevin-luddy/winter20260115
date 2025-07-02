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
		public void GetByWorkspaceId_ValidWorkspaceId_ReturnsProposalDTOs()
		{
			// Arrange
			int workspaceId = 1;
			List<Proposal> proposals = new List<Proposal>
			{
				new Proposal { WorkspaceID = workspaceId, PA_Number = "PA-1" },
				new Proposal { WorkspaceID = workspaceId, PA_Number = "PA-2" }
			};

			Mock<PldDBContext> mockContext = new Mock<PldDBContext>();
			Mock<DbSet<Proposal>> mockSet = new Mock<DbSet<Proposal>>();
			mockSet.As<IQueryable<Proposal>>().Setup(m => m.Provider).Returns(proposals.AsQueryable().Provider);
			mockSet.As<IQueryable<Proposal>>().Setup(m => m.Expression).Returns(proposals.AsQueryable().Expression);
			mockSet.As<IQueryable<Proposal>>().Setup(m => m.ElementType).Returns(proposals.AsQueryable().ElementType);
			mockSet.As<IQueryable<Proposal>>().Setup(m => m.GetEnumerator()).Returns(() => proposals.GetEnumerator());

			mockContext.Setup(c => c.Proposals).Returns(mockSet.Object);

			PldDTODataLoader dataLoader = new PldDTODataLoader(mockContext.Object);

			// Act
			List<Dtos.ProposalDTO> result = dataLoader.GetByWorkspaceId(workspaceId);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("PA-1", result[0].PA_Number);
			Assert.AreEqual("PA-2", result[1].PA_Number);
		}


	



	}
}
