// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.DTO.Request;
	using GenBOE.Models;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Transactions;

	/// <summary>
	/// Test Class for Request Data Loader
	/// </summary>
	[TestClass]
	public class RequestDataLoaderTest
	{
		[TestMethod]
		public void ShouldSaveRequest()
		{
			// Arrange
			RequestDataLoader sut = new RequestDataLoader();
			sut.Delete(RequestType.CalculateActuals, "testNtid");

			try
			{
				// Act
				int? newRequest = sut.Insert(RequestType.CalculateActuals, "testNtid");

				// Assert
				Assert.AreEqual(true, (newRequest.Value > 0));
			}
			finally
			{
				// Cleanup
				sut.Delete(RequestType.CalculateActuals, "testNtid");
			}
		}

		[TestMethod]
		public void ShouldNotAllowDuplicateRequest()
		{
			// Arrange
			RequestDataLoader sut = new RequestDataLoader();
			sut.Delete(RequestType.CalculateActuals, "testNtid");

			try
			{
				// Act
				int? newRequest = sut.Insert(RequestType.CalculateActuals, "testNtid");
				int? duplicateTestRequest = sut.Insert(RequestType.CalculateActuals, "testNtid");

				// Assert
				Assert.AreEqual(true, (newRequest.Value > 0));
				Assert.AreEqual(false, (duplicateTestRequest != null && duplicateTestRequest.Value > 0));
			}
			finally
			{
				// Cleanup
				sut.Delete(RequestType.CalculateActuals, "testNtid");
			}
		}

		[TestMethod]
		public void ShouldDeleteRequest()
		{
			// Arrange
			RequestDataLoader sut = new RequestDataLoader();
			sut.Delete(RequestType.CalculateActuals, "testNtid");

			try
			{
				// Act
				int? newRequest = sut.Insert(RequestType.CalculateActuals, "testNtid");
				int? duplicateTestRequest = sut.Insert(RequestType.CalculateActuals, "testNtid");

				// Assert first insert
				Assert.AreEqual(true, (newRequest.Value > 0));
				Assert.AreEqual(false, (duplicateTestRequest != null && duplicateTestRequest.Value > 0));

				// Act - delete and try again
				sut.Delete(RequestType.CalculateActuals, "testNtid");
				duplicateTestRequest = sut.Insert(RequestType.CalculateActuals, "testNtid");

				// Assert after delete
				Assert.AreEqual(true, (duplicateTestRequest.Value > 0));
			}
			finally
			{
				// Cleanup
				sut.Delete(RequestType.CalculateActuals, "testNtid");
			}
		}
	}
}
