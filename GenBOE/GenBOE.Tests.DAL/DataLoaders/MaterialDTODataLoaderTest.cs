// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Models;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class MaterialDTODataLoaderTest
	{
		/// <summary>
		/// Create a loader for testing
		/// </summary>
		/// <returns>Test loader</returns>
		private MaterialDTODataLoader CreateTestLoader()
		{
			return new MaterialDTODataLoader();
		}

		/// <summary>
		/// Test GetByWorkspaceId
		/// </summary>
		[TestMethod]
		public void GetByWorkspaceIdTest()
		{
			MaterialDTODataLoader loader = CreateTestLoader();

			MaterialTaskElement material;
			BOE boe;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				material = gbe.MaterialTaskElements.FirstOrDefault(x => x.MaterialTaskDescription.Length > 0);
				boe = gbe.BOEs.FirstOrDefault(x => x.BOEID == material.BOEID);
			}

			if (boe!= null)
			{
				ICollection<MaterialDTO> result = loader.GetByWorkspaceId(boe.WorkspaceID);

				Assert.IsTrue(result.Any());
				Assert.IsTrue(result.Any(x => x.TaskTitle == material.MaterialTaskTitle));
				Assert.IsTrue(result.Any(x => x.TaskDescription == material.MaterialTaskDescription));
			}
		}
	}
}
