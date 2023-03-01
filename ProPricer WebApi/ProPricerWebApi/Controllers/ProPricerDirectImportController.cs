/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi
{
	using APTSPropricerApi.DTOs;
	using Microsoft.AspNetCore.Mvc;
	using System.IO;

	/// <summary>
	/// ProPricer Direct Import controller (used by GenBOE)
	/// </summary>
	/// <seealso cref="APTSPropricerApi.ProPricerController" />
	public class ProPricerDirectImportController : ProPricerController
	{
		/// <summary>
		/// The exporter class (Ray's old code, slightly modified)
		/// </summary>
		private readonly ProPricerProposalExporter exporter;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProPricerDirectImportController"/> class.
		/// </summary>
		public ProPricerDirectImportController(ProPricerProposalExporter proPricerProposalExporter, ILogger<ProPricerDirectImportController> logger) : base(logger)
		{
			this.exporter = proPricerProposalExporter;
		}

		// POST api/proposals
		/// <summary>
		/// Posts the specified instance identifier.
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="id">The proposal identifier.</param>
		/// <param name="container">The container housing the import data.</param>
		/// <returns></returns>
		[HttpPost]
		[Route("{instanceId}/{id}")]
		public ProPricerExportResults Post(int instanceId, string id, [FromBody] ProPricerImportContainer container)
		{
			string tempPathFileName = Path.GetTempPath();

			ProPricerExportResults result = this.exporter.ExportToProPricerProposal(tempPathFileName, id, instanceId, container.taskData, container.resourceData, container.taskExportOption, container.resourceExportOption);

			return result;
		}
	}
}