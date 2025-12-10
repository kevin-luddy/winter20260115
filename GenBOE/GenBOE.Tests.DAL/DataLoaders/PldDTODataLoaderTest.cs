// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
	using System;
	using System.Collections.Generic;
	using System.Data.Entity;
	using System.Linq;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.PLD.Models;
	using GenBOE.PLD.Models.Models;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	/// <summary>
	/// Test Class for PldDTODataLoader
	/// </summary>
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

		/// <summary>
		/// Test GetLastModifiedDate returns a valid date when one exists
		/// </summary>
		[TestMethod]
		public void GetLastModifiedDate_ReturnsValidDate()
		{
			PLDProposalDTO testPA;
			using (PldDBContext ctx = new PldDBContext())
			{
				testPA = ctx.Proposals.Where(x => x.Last_Modified_Date != null)
					.Select(x => new PLDProposalDTO()
					{
						PANumber = x.PA_Number,
						LastModifiedDate = x.Last_Modified_Date
					})
					.FirstOrDefault();
			}

			DateTime? result = _loader.GetLastModifiedDate(testPA.PANumber);

			Assert.IsNotNull(result);
			Assert.AreEqual(testPA.LastModifiedDate, result.Value);
		}

		/// <summary>
		/// Test GetLastModifiedDate returns null when there is no PA for the PA Number
		/// </summary>
		[TestMethod]
		public void GetLastModifiedDate_ReturnsNullForInvalidPANumber()
		{
			DateTime? result = _loader.GetLastModifiedDate("FAKE PA NUMBER");

			Assert.IsNull(result);
		}
	}
}
