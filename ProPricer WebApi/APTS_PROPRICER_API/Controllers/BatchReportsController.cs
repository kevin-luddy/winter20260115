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
    using APTSPropricerApi.Connection;
    using APTSPropricerApi.DTOs;
    using EBS.Core;
    using EBS.ProPricer.Model;
    using EBS.ProPricer.Reports;

    /// <summary>
    /// Batch Reports Controller
    /// </summary>
    public class BatchReportsController : ProPricerController
    {
        // GET api/batchreports
        /// <summary>
        /// Return the list of batch reports from the global library.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        public ICollection<BatchReportDto> Get(int instanceId)
        {
            ICollection<BatchReportDto> result = new List<BatchReportDto>();
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                ppc.Workspace.Reports.BatchReports.Open();
                foreach (BatchReport rep in ppc.Workspace.Reports.BatchReports.Items())
                {
                    BatchReportDto dto = new BatchReportDto(rep);
                    result.Add(dto);
                }

                ppc.Workspace.Reports.BatchReports.Close();
            }

            return result;
        }

        // POST api/batchreports/{instanceid}
        /// <summary>
        /// Creates a batch report export
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="container"></param>
        public string Post(int instanceId, [FromBody] ProPricerExportContainer container)
        {
            string resultData = string.Empty;
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                Guid proposalGuid = new Guid(container.proposalId);
                Proposal proposal = ppc.Workspace.Proposals.Find(proposalGuid).Value();
                BatchReport batchReport = null;
                if (proposal != null)
                {
                    try
                    {
                        proposal.Open();
                        ppc.Workspace.Reports.BatchReports.Open();
                        batchReport = ppc.Workspace.Reports.BatchReports.Items().FirstOrDefault(b => b.Id.ToString() == container.batchReportId);
                        batchReport.Open();

                        string tempFile = Path.GetRandomFileName();
                        
                        System.Diagnostics.Debug.WriteLine(batchReport.IsEditable());
                        BatchReportContextManager mgr = new BatchReportContextManager(proposal);
                        BatchReportRuntimeContext ctx = new BatchReportRuntimeContext(batchReport, mgr);
                        ctx.Options.ExportType = EBS.ProPricer.Reports.Export.ExportType.Excel;
                        // TODO:  we should be putting this into a custom folder named something like DeleteMe
                        // Also, consider adding to global.asax.cs to delete any files in that folder on app shutdown
                        ctx.Options.Folder = Path.GetTempPath();
                        ctx.Options.FileName = Path.GetFileNameWithoutExtension(tempFile);
                        ctx.Options.Destination = ReportDestination.File;
                        ctx.Options.OutputMode = OutputMode.Combined;
                        ctx.ProcessAll = true;
                        
                        BatchReportGenerator generator = new BatchReportGenerator(ctx);
                        ctx.Generator = generator;

                        generator.Process();

                        // TODO Post process file
                        tempFile = Path.Combine(ctx.Options.Folder, ctx.Options.FileName + ".xlsx");

                        resultData = File.ReadAllText(tempFile);
                        File.Delete(tempFile);
                    }
                    finally
                    {
                        if (batchReport != null)
                        {
                            batchReport.Close();
                        }

                        ppc.Workspace.Reports.BatchReports.Close();
                        proposal.Close();
                    }
                }
                else
                {
                    throw new ArgumentException("Proposal was not found or could not be opened in the workspace.");
                }

                return resultData;
            }

        }
    }
}