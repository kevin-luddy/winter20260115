/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using System;
using System.Collections.Generic;
using System.Web.Http;
using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;
using EBS.Core;
using EBS.ProPricer.ImportExport.Ascii;
using EBS.ProPricer.Model;

namespace APTSPropricerApi.Controllers
{
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
        public void Post(int instanceId, [FromBody] ProPricerExportContainer container)
        {
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                Guid proposalGuid = new Guid(container.proposalId);
                Proposal proposal = ppc.Workspace.Proposals.Find(proposalGuid).Value();
                if (proposal != null)
                {
                    proposal.Open();
                    
                    // TODO custom export for Batch Report for this proposal

                    proposal.Close();
                }
                else
                {
                    throw new ArgumentException("Proposal was not found or could not be opened in the workspace.", "proposalId");
                }
            }

        }
    }
}