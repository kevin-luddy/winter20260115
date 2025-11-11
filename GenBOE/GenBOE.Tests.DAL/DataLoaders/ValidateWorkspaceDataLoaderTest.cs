namespace GenBOE.Tests.DAL.DataLoaders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using GenBOE.DataBridge.DTO;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ValidateWorkspaceDataLoaderTest
	{
		[TestMethod]
		public void L_ValidateWorkspacePoPValid()
		{
			ValidateWorkspaceDataLoader sut = new ValidateWorkspaceDataLoader();

			int workspaceId = 1;
			DateTime startDate = new DateTime(2015, 3, 15);
			DateTime endDate = new DateTime(2021, 3, 15);

			bool isValid = sut.ValidateWorkspacePoP(workspaceId, startDate, endDate);

			Assert.IsTrue(isValid, "Workspace should be valid within the given date range");
		}

		[TestMethod]
		public void L_ValidateWorkspacePoPInvalid()
		{
			ValidateWorkspaceDataLoader sut = new ValidateWorkspaceDataLoader();

			int workspaceId = 32864;
			DateTime startDate = new DateTime(2022, 2, 15);
			DateTime endDate = new DateTime(2034, 6, 15);

			bool isValid = sut.ValidateWorkspacePoP(workspaceId, startDate, endDate);

			Assert.IsFalse(isValid, "Workspace should not be valid within the given date range");
		}
	}
}
