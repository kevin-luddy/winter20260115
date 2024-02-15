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
	using APTSPropricerApi.Common;
	using APTSPropricerApi.Connection;
	using APTSPropricerApi.DTOs;
	using Aspose.Cells;
	using EBS.Core;
	using EBS.ProPricer.Model;
	using EBS.ProPricer.Reports;
	using EBS.ProPricer.Reports.Export;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	/// <summary>
	/// Batch Reports Controller
	/// </summary>
	//[Authorize(Policy = "OnlyIesToken")]
	[Authorize(AuthenticationSchemes = Constants.IES_TOKEN_SCHEME)]
	[ApiController]
	[Route("api/BatchReports")]
	public class BatchReportsController : ControllerBase
	{
		/// <summary>
		/// The Pool Manager List
		/// </summary>
		private readonly PoolManagerList poolManagerList;

		/// <summary>
		/// The Logger
		/// </summary>
		private readonly ILogger<BatchReportsController> Logger;

		/// <summary>
		/// #ctor
		/// </summary>
		/// <param name="logger">The logger</param>
		/// <param name="poolManagerList">Pool manager list</param>
		public BatchReportsController(ILogger<BatchReportsController> logger, PoolManagerList poolManagerList)
		{
			this.Logger = logger;
			this.poolManagerList = poolManagerList;
		}

		// GET api/batchreports
		/// <summary>
		/// Return the list of batch reports from the global library.
		/// </summary>
		/// <param name="instanceId">The connection instance identifier.</param>
		/// <returns>Pro Pricer Response containing the List of Batch Reports</returns>
		[HttpGet]
		[Route("{instanceId}")]
		public ProPricerResponse<ICollection<BatchReportDto>> Get(int instanceId)
		{
			ProPricerResponse<ICollection<BatchReportDto>> response = new()
			{
				Data = new List<BatchReportDto>()
			};
			try
			{
				response = GetBatchReports(instanceId);
			}
			catch (Exception ex)
			{
				string message = $"Error retrieving Batch Reports from Pro Pricer for Connection Id: {instanceId}";
				Logger.LogError(ex, message);
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
		[Route("Export/{instanceId}")]
		public ProPricerResponse<ICollection<Table>> ExportBatchReport(int instanceId, [FromBody] ProPricerExportContainer container)
		{
			ProPricerResponse<ICollection<Table>> response = new();

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
					response = ExportBatchReport(instanceId, container, out tempFile);
				}
				catch (Exception ex)
				{
					string message = $"Error exporting Batch Report from Pro Pricer for Connection Id: {instanceId}, Proposal Id: {container.proposalId}, and Batch Report Id: {container.batchReportId}";
					Logger.LogError(ex, message);
					response.Messages.Add(message);
				}
				finally
				{
					// Delete the temp file if it exists, swallow the error
					try
					{
						if (System.IO.File.Exists(tempFile))
						{
							System.IO.File.Delete(tempFile);
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

		// POST api/batchreports/{instanceid}
		/// <summary>
		/// Creates a batch report PDF export
		/// </summary>
		/// <param name="instanceId">The connection instance identifier.</param>
		/// <param name="container">The param container for the Post event</param>
		/// <returns>Pro Pricer Response containing the Tables of data</returns>
		[HttpPost]
		[Route("ExportAsPdf/{instanceId}")]
		public ProPricerResponse<byte[]> ExportBatchReportAsPdf(int instanceId, [FromBody] ProPricerExportContainer container)
		{
			ProPricerResponse<byte[]> response = new();

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
					response = ExportBatchReportAsPdf(instanceId, container, out tempFile);
				}
				catch (Exception ex)
				{
					string message = $"Error exporting Batch Report as PDF from Pro Pricer for Connection Id: {instanceId}, Proposal Id: {container.proposalId}, and Batch Report Id: {container.batchReportId}";
					Logger.LogError(ex, message);
					response.Messages.Add(message);
				}
				finally
				{
					// Delete the temp file if it exists, swallow the error
					try
					{
						if (System.IO.File.Exists(tempFile))
						{
							System.IO.File.Delete(tempFile);
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
			ProPricerResponse<ICollection<BatchReportDto>> response = new()
			{
				Data = new List<BatchReportDto>()
			};
			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				ppc.Workspace.Reports.BatchReports.Open();
				foreach (BatchReport rep in ppc.Workspace.Reports.BatchReports.Items())
				{
					BatchReportDto dto = new(rep);
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
		/// <param name="tempFile">The location of the temporary file created by Pro Pricer export</param>
		/// <returns>A ProPricerResponse object containing a file exported via Batch Report</returns>
		internal ProPricerResponse<ICollection<Table>> ExportBatchReport(int instanceId, ProPricerExportContainer container, out string tempFile)
		{
			ProPricerResponse<ICollection<Table>> response = new()
			{
				Data = new List<Table>()
			};
			tempFile = GenerateBatchReportFile(instanceId, container.proposalId, container.batchReportId, response, ExportType.Excel);
			ReadBatchFile(tempFile, response);
			response.IsSuccessful = true;
			return response;
		}

		/// <summary>
		/// Exports a Batch Report
		/// </summary>
		/// <param name="instanceId">The connection instance identifier.</param>
		/// <param name="container">The param container for the Post event</param>
		/// <param name="tempFile">The location of the temporary file created by Pro Pricer export</param>
		/// <returns>A ProPricerResponse object containing a file exported via Batch Report</returns>
		internal ProPricerResponse<byte[]> ExportBatchReportAsPdf(int instanceId, ProPricerExportContainer container, out string tempFile)
		{
			ProPricerResponse<byte[]> response = new();
			tempFile = GenerateBatchReportFileAsPdf(instanceId, container.proposalId, container.batchReportId, response, ExportType.Pdf);
			response.IsSuccessful = true;
			return response;
		}

		/// <summary>
		/// Reads the Pro Pricer Batch File and Converts into Tables
		/// </summary>
		/// <param name="tempFile">The location of the temp file to read</param>
		/// <param name="response">The response object used to add Table data and error messages</param>
		private void ReadBatchFile(string tempFile, ProPricerResponse<ICollection<Table>> response)
		{
			TxtLoadOptions opts = new(LoadFormat.TabDelimited)
			{
				MemorySetting = MemorySetting.MemoryPreference,
				LoadFilter = new LoadFilter(LoadDataFilterOptions.CellData),
				CheckExcelRestriction = false,
				ConvertNumericData = false,
				ConvertDateTimeData = false
			};

			using (Workbook book = new(tempFile, opts))
			{
				foreach (Worksheet sheet in book.Worksheets)
				{
					Table convertedSheet = new();
					response.Data.Add(convertedSheet);
					foreach (Aspose.Cells.Row row in sheet.Cells.Rows)
					{
						Common.Row convertedRow = new();
						convertedSheet.Rows.Add(convertedRow);
						// Get enumerator from an object of Row
						System.Collections.IEnumerator rowEnumerator = row.GetEnumerator();
						// Traverse cells in the given row
						while (rowEnumerator.MoveNext())
						{
							Cell cell = rowEnumerator.Current as Aspose.Cells.Cell;

							if (cell.DisplayStringValue != null)
							{
								convertedRow.Cells.Add(new DetailedCell() { Value = cell.DisplayStringValue, ColumnRowValue = cell.Name });
							}
							else if (cell.Value != null)
							{
								convertedRow.Cells.Add(new DetailedCell() { Value = cell.Value.ToString(), ColumnRowValue = cell.Name });
							}
							else
							{
								convertedRow.Cells.Add(new DetailedCell() { Value = string.Empty, ColumnRowValue = cell.Name });
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Generates a Batch Report File
		/// </summary>
		/// <param name="instanceId">The connection instance identifier.</param>
		/// <param name="proposalId">The proposal Id to use for the Batch Report</param>
		/// <param name="batchReportId">The batch report Id</param>
		/// <param name="response">The response object used to add error messages into.</param>
		/// <returns>The temporary File location that was generated.</returns>
		private string GenerateBatchReportFile(int instanceId, string proposalId, string batchReportId, ProPricerResponse<ICollection<Table>> response, ExportType exportType)
		{
			string tempFile = null;
			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				Proposal proposal = null;
				BatchReport batchReport = null;
				try
				{
					Guid proposalGuid = new(proposalId);
					proposal = ppc.Workspace.Proposals.Find(proposalGuid).Value();

					if (proposal != null)
					{
						proposal.Open();
						ppc.Workspace.Reports.BatchReports.Open();
						batchReport = ppc.Workspace.Reports.BatchReports.Items().FirstOrDefault(b => b.Id.ToString() == batchReportId);
						if (batchReport != null)
						{
							tempFile = Utility.GenerateTempFile(batchReport, proposal, exportType);
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
					proposal?.Close();

					batchReport?.Close();

					ppc.Workspace.Reports.BatchReports.Close();
				}
			}

			return tempFile;
		}

		/// <summary>
		/// Generates a Batch Report File as PDF
		/// </summary>
		/// <param name="instanceId">The connection instance identifier.</param>
		/// <param name="proposalId">The proposal Id to use for the Batch Report</param>
		/// <param name="batchReportId">The batch report Id</param>
		/// <param name="response">The response object used to add error messages into.</param>
		/// <returns>The temporary File location that was generated.</returns>
		private string GenerateBatchReportFileAsPdf(int instanceId, string proposalId, string batchReportId, ProPricerResponse<byte[]> response, ExportType exportType)
		{
			string tempFile = null;
			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				Proposal proposal = null;
				BatchReport batchReport = null;
				try
				{
					Guid proposalGuid = new(proposalId);
					proposal = ppc.Workspace.Proposals.Find(proposalGuid).Value();

					if (proposal != null)
					{
						proposal.Open();
						ppc.Workspace.Reports.BatchReports.Open();
						batchReport = ppc.Workspace.Reports.BatchReports.Items().FirstOrDefault(b => b.Id.ToString() == batchReportId);
						if (batchReport != null)
						{
							tempFile = Utility.GenerateTempFile(batchReport, proposal, exportType);
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
					proposal?.Close();

					batchReport?.Close();

					ppc.Workspace.Reports.BatchReports.Close();
				}
			}

			return tempFile;
		}
	}
}