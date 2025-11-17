// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Tests.DAL.DataLoaders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;
	using System.Transactions;
	using DocumentFormat.OpenXml.InkML;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Models;
	using IES.Common;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Workspace Validation Test
	/// </summary>
	[TestClass]
	public class ValidateWorkspaceDataLoaderTest
	{

		/// <summary>
		/// BOE Loader to create and delete invalid BOE for testing
		/// </summary>
		/// <returns></returns>
		private BoeDTODataLoader CreateBOETestLoader()
		{
			return new BoeDTODataLoader();
		}

		/// <summary>
		/// CLIN Loader to create and delete the invalid CLIN for testing
		/// </summary>
		/// <returns></returns>
		private ClinDTODataLoader CreateCLINTestLoader()
		{
			return new ClinDTODataLoader();
		}

		/// <summary>
		/// Valid Workspace
		/// </summary>
		[TestMethod]
		public void L_ValidateWorkspacePoPValid()
		{
			ValidateWorkspaceDataLoader sut = new ValidateWorkspaceDataLoader();

			int workspaceId = 32733;
			DateTime startDate = new DateTime(2025, 3, 15);
			DateTime endDate = new DateTime(2037, 7, 15);

			bool isValid = sut.ValidateWorkspacePoP(workspaceId, startDate, endDate);

			Assert.IsTrue(isValid, "Workspace should be valid within the given date range");
		}

		/// <summary>
		/// Invalid workspace when CLIN is outside of PoP
		/// </summary>
		[TestMethod]
		public void L_ValidateWorkspacePoPInvalidCLIN()
		{
			ValidateWorkspaceDataLoader sut = new ValidateWorkspaceDataLoader();

			int workspaceId = 1;
			DateTime startDate = new DateTime(2015, 3, 15);
			DateTime endDate = new DateTime(2021, 3, 15);

			bool isValid = sut.ValidateWorkspacePoP(workspaceId, startDate, endDate);

			Assert.IsFalse(isValid, "Workspace is not valid because CLIN is outside of PoP");
		}

		/// <summary>
		/// Invalid 
		/// </summary>
		[TestMethod]
		public void L_ValidateWorkspacePoPInvalidBOE()
		{
			ValidateWorkspaceDataLoader sut = new ValidateWorkspaceDataLoader();

			int workspaceId = 32733;
			DateTime startDate = new DateTime(2025, 3, 15);
			DateTime endDate = new DateTime(2037, 7, 15);

			int clinId = GlobalTestCaseSetup.CreateCLIN(workspaceId);
			int boeId = GlobalTestCaseSetup.CreateBOE(workspaceId, clinId);

			bool isValid = sut.ValidateWorkspacePoP(workspaceId, startDate, endDate);

			Assert.IsFalse(isValid, "Workspace is not valid because BOE is outside of PoP");

			ClinDTODataLoader clinLoader = this.CreateCLINTestLoader();
			ICollection<ClinDTO> clins = clinLoader.GetByWorkspaceId(workspaceId);

			ClinDTO clinToDelete = new ClinDTO();
			clinToDelete = (from c in clins
							where c.Id == clinId
							select c).FirstOrDefault();
			clinToDelete.Updateable = UpdateType.Deleted;

			BoeDTODataLoader boeLoader = this.CreateBOETestLoader();
			ICollection<BoeDTO> boes = boeLoader.GetByWorkspaceId(workspaceId);

			BoeDTO boeToDelete = new BoeDTO();
			boeToDelete = (from b in boes
						   where b.Id == boeId
						   select b).FirstOrDefault();
			boeToDelete.Updateable = UpdateType.Deleted;

			using (TransactionScope scope = new TransactionScope())
			{
				boeLoader.Save(boeToDelete);
				clinLoader.Save(clinToDelete);

				scope.Complete();
			}
		}


	}
}
