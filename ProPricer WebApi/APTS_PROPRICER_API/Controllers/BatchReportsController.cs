/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Controllers
{

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Web.Http;
	using APTSPropricerApi.Common;
	using APTSPropricerApi.Connection;
	using APTSPropricerApi.DTOs;
	using Aspose.Cells;
	using DevExpress.Utils.About;
	using EBS.Core;
	using EBS.ProPricer.Model;
	using EBS.ProPricer.Reports;

	/// <summary>
	/// Batch Reports Controller
	/// </summary>
	[AllowAnonymous]
	public class BatchReportsController : ProPricerController
	{
		/// <summary>
		/// The logger for the controller
		/// </summary>
		private readonly Logger logger = new Logger(typeof(BatchReportsController));

		/// <summary>
		/// Token handler 
		/// </summary>
		private readonly TokenHandling tokenHandler = new TokenHandling();

		// GET api/batchreports
		/// <summary>
		/// Return the list of batch reports from the global library.
		/// </summary>
		/// <param name="instanceId">The connection instance identifier.</param>
		/// <returns>Pro Pricer Response containing the List of Batch Reports</returns>
		[HttpGet]
		public ProPricerResponse<ICollection<BatchReportDto>> Get(int instanceId)
		{
			ProPricerResponse<ICollection<BatchReportDto>> response = new ProPricerResponse<ICollection<BatchReportDto>>();
			response.Data = new List<BatchReportDto>();
			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();
				response = GetBatchReports(instanceId);
			}
			catch (Exception ex)
			{
				string message = $"Error retrieving Batch Reports from Pro Pricer for Connection Id: {instanceId}";
				logger.Error(ex, message);
				response.Messages.Add(message);
			}

			return response;
		}

		// POST api/batchreports/{instanceid}
		/// <summary>
		/// Creates a batch report export
		/// </summary>
		/// <param name="instanceId">The connection instance identifier.</param>
		/// <param name="container">The param container for the Post event</param>
		/// <returns>Pro Pricer Response containing the Tables of data</returns>
		[HttpPost]
		[Route("api/BatchReports/Export/{instanceId}")]
		public ProPricerResponse<string> ExportBatchReport(int instanceId, [FromBody] ProPricerExportContainer container)
		{
			ProPricerResponse<string> response = new ProPricerResponse<string>();

			if (container == null)
			{
				response.Messages.Add("The [POST] container passed in cannot be null.");
			}
			else if (string.IsNullOrEmpty(container.proposalId))
			{
				response.Messages.Add("The Proposal Id cannot be null.");
			}
			else if (string.IsNullOrEmpty(container.batchReportId))
			{
				response.Messages.Add("The Batch Report Id cannot be null.");
			}
			else
			{

				string tempFile = null;
				try
				{
					tokenHandler.AuthenticateUserFromAuthorizationToken();
					response = ExportBatchReport(instanceId, container, container.exportType);
				}
				catch (Exception ex)
				{
					string message = $"Error exporting Batch Report from Pro Pricer for Connection Id: {instanceId}, Proposal Id: {container.proposalId}, and Batch Report Id: {container.batchReportId}";
					logger.Error(ex, message);
					response.Messages.Add(message);
				}
				finally
				{
					// Delete the temp file if it exists, swallow the error
					try
					{
						if (File.Exists(tempFile))
						{
							File.Delete(tempFile);
						}
					}
					catch (Exception)
					{
						// error deleting file, will delete during next app startup
					}
				}
			}

			return response;
		}

		/// <summary>
		/// Gets the Batch Reports
		/// </summary>
		/// <param name="instanceId">The connection instance identifier.</param>
		/// <returns>A pro pricer response object containing reports</returns>
		internal ProPricerResponse<ICollection<BatchReportDto>> GetBatchReports(int instanceId)
		{
			ProPricerResponse<ICollection<BatchReportDto>> response = new ProPricerResponse<ICollection<BatchReportDto>>();
			response.Data = new List<BatchReportDto>();
			using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
			{
				ppc.Workspace.Reports.BatchReports.Open();
				foreach (BatchReport rep in ppc.Workspace.Reports.BatchReports.Items())
				{
					BatchReportDto dto = new BatchReportDto(rep);
					response.Data.Add(dto);
				}

				response.IsSuccessful = true;
				ppc.Workspace.Reports.BatchReports.Close();
			}

			return response;
		}

		/// <summary>
		/// Exports a Batch Report
		/// </summary>
		/// <param name="instanceId">The connection instance identifier.</param>
		/// <param name="container">The param container for the Post event</param>
		/// <param name="exportType">Export File Type to be generated by Pro Pricer (Excel, PDF, Word)</param>
		/// <returns>A ProPricerResponse object containing Tables from an exported Batch Report</returns>
		internal ProPricerResponse<string> ExportBatchReport(int instanceId, ProPricerExportContainer container, string exportType)
		{
			ProPricerResponse<string> response = new ProPricerResponse<string>();
			GenerateBatchReportFile(instanceId, container.proposalId, container.batchReportId, response, exportType);

			if (exportType.ToLower() == Constants.REPORT_TYPE_EXCEL.ToLower())
			{
				ReadBatchFile(response);
			}

			response.IsSuccessful = true;
			return response;
		}

		/// <summary>
		/// Reads the Pro Pricer Batch File and Converts into Tables
		/// </summary>
		/// <param name="response">The response object used to add Table data and error messages</param>
		private void ReadBatchFile(ProPricerResponse<string> response)
		{
			TxtLoadOptions opts = new TxtLoadOptions(LoadFormat.TabDelimited)
			{
				MemorySetting = MemorySetting.MemoryPreference,
				LoadFilter = new LoadFilter(LoadDataFilterOptions.CellData),
				CheckExcelRestriction = false,
				ConvertNumericData = false,
				ConvertDateTimeData = false
			};

			using (Workbook book = new Workbook(response.Data, opts))
			{
				foreach (Worksheet sheet in book.Worksheets)
				{
					Table convertedSheet = new Table();
					foreach (Aspose.Cells.Row row in sheet.Cells.Rows)
					{
						Common.Row convertedRow = new Common.Row();
						convertedSheet.Rows.Add(convertedRow);
						// Get enumerator from an object of Row
						System.Collections.IEnumerator rowEnumerator = row.GetEnumerator();
						// Traverse cells in the given row
						while (rowEnumerator.MoveNext())
						{
							Cell cell = rowEnumerator.Current as Aspose.Cells.Cell;
							convertedRow.Cells.Add(cell.Value?.ToString() ?? string.Empty);
						}
					}
				}

				MemoryStream ms = book.SaveToStream();
				ms.Position = 0;
				StreamReader sr = new StreamReader(ms);

				response.Data = sr.ReadToEnd();
			}
		}

		/// <summary>
		/// Generates a Batch Report File
		/// </summary>
		/// <param name="instanceId">The connection instance identifier.</param>
		/// <param name="proposalId">The proposal Id to use for the Batch Report</param>
		/// <param name="batchReportId">The batch report Id</param>
		/// <param name="response">The response object used to add error messages into.</param>
		/// <param name="exportType">The Export File Type</param>
		private static void GenerateBatchReportFile(int instanceId, string proposalId, string batchReportId, ProPricerResponse<string> response, string exportType)
		{
			string tempFile = null;
			using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
			{
				Proposal proposal = null;
				BatchReport batchReport = null;
				try
				{
					Guid proposalGuid = new Guid(proposalId);
					proposal = ppc.Workspace.Proposals.Find(proposalGuid).Value();

					if (proposal != null)
					{
						proposal.Open();
						ppc.Workspace.Reports.BatchReports.Open();
						batchReport = ppc.Workspace.Reports.BatchReports.Items().FirstOrDefault(b => b.Id.ToString() == batchReportId);
						if (batchReport != null)
						{
							batchReport.Open();
							tempFile = Path.GetRandomFileName();

							BatchReportContextManager mgr = new BatchReportContextManager(proposal);
							BatchReportRuntimeContext ctx = new BatchReportRuntimeContext(batchReport, mgr);
							ctx.Options.ExportType = ProPricerUtility.GetExportType(exportType);
							ctx.Options.Folder = Constants.TEMP_DIRECTORY;
							ctx.Options.FileName = Path.GetFileNameWithoutExtension(tempFile);
							ctx.Options.Destination = ReportDestination.File;
							ctx.Options.OutputMode = OutputMode.Combined;
							ctx.ProcessAll = true;

							BatchReportGenerator generator = new BatchReportGenerator(ctx);
							ctx.Generator = generator;

							generator.Process();

							tempFile = Path.Combine(ctx.Options.Folder, ctx.Options.FileName + ProPricerUtility.GetExportExtension(ctx.Options.ExportType));
						}
						else
						{
							response.Messages.Add("Batch Report was not found or could not be opened in the workspace.");
						}
					}
					else
					{
						response.Messages.Add("Proposal was not found or could not be opened in the workspace.");
					}
				}
				finally
				{
					if (proposal != null)
					{
						proposal.Close();
					}

					if (batchReport != null)
					{
						batchReport.Close();
					}

					ppc.Workspace.Reports.BatchReports.Close();
				}
			}

			response.Data = tempFile;

			//return tempFile;
		}
	}
}