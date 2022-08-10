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
    using System.Linq;
    using System.Web.Http;
    using APTSPropricerApi.Common;
    using APTSPropricerApi.DTOs;

    /// <summary>
    /// The Pro Pricer Data API Controller for ACV to call
    /// </summary>
    [AllowAnonymous]
    public class ProPricerDataApiController : ApiController
    {
        /// <summary>
        /// The logger for the controller
        /// </summary>
        private readonly Logger logger = new Logger(typeof(ProPricerDataApiController));

        /// <summary>
        /// Token handler 
        /// </summary>
        private readonly TokenHandling tokenHandler = new TokenHandling();

        /// <summary>
        /// Gets the Proposals
        /// </summary>
        /// <param name="instanceId">The connection instance id to retrieve proposals on</param>
        /// <returns>A list of proposals inside folders</returns>
        [Route("api/ProPricerData/Proposals/{instanceId}")]
        [HttpGet]
        public ProPricerResponse<ICollection<ProposalFolderInfo>> Get(int instanceId)
        {
            ProPricerResponse<ICollection<ProposalFolderInfo>> response = new ProPricerResponse<ICollection<ProposalFolderInfo>>();

            try
            {
                tokenHandler.AuthenticateUserFromAuthorizationToken();

                using (ProposalsController pc = new ProposalsController())
                {
                    response.Data = pc.Get(instanceId);
                    response.IsSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                response.Messages.Add("Error retrieving proposals from Pro Pricer");
            }

            return response; 
        }

        /// <summary>
        /// Gets the Available Connections.
        /// </summary>
        /// <returns>Returns a collection of the pool manager instances.</returns>
        [Route("api/ProPricerData/AvailableConnections")]
        [HttpGet]
        public ProPricerResponse<ICollection<PoolInstanceDto>> Get()
        {
            ProPricerResponse<ICollection<PoolInstanceDto>> response = new ProPricerResponse<ICollection<PoolInstanceDto>>();

            try
            {
                tokenHandler.AuthenticateUserFromAuthorizationToken();

                using (PoolInstanceController pc = new PoolInstanceController())
                {
                    response.Data = pc.Get().ToList();
                    response.IsSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                response.Messages.Add("Error retrieving Pool Instances");
            }

            return response;
        }

        /// <summary>
        /// Gets Pricing Data for a proposal
        /// </summary>
        /// <param name="instanceId">Connection instance Id</param>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Pricing data for a Proposal</returns>
        [Route("api/ProPricerData/PricingData/{instanceId}/{proposalId}")]
        [HttpGet]
        public ProPricerResponse<PricingData> GetPricingData(int instanceId, string proposalId)
        {
            ProPricerResponse<PricingData> response = new ProPricerResponse<PricingData>();
            try
            {
                tokenHandler.AuthenticateUserFromAuthorizationToken();

                using (ProposalandTasksController ptc = new ProposalandTasksController())
                {
                    ProposalDto pDto = ptc.Get(instanceId, proposalId);

                    response.Data = new PricingData
                    {
                        Totals = GetTotals(pDto),
                        LineItems = GetLineItems(pDto)
                    };

                    response.IsSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                response.Messages.Add("Error retrieving Proposal Pricing Data");
            }

            return response;
        }

        /// <summary>
        /// Gets Pricing Data Totals (only) for a proposal
        /// </summary>
        /// <param name="instanceId">Connection instance Id</param>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Pricing data for a Proposal</returns>
        [Route("api/ProPricerData/PricingDataTotals/{instanceId}/{proposalId}")]
        [HttpGet]
        public ProPricerResponse<PricingTotals> GetPricingDataTotals(int instanceId, string proposalId)
        {
            ProPricerResponse<PricingTotals> response = new ProPricerResponse<PricingTotals>();
            try
            {
                tokenHandler.AuthenticateUserFromAuthorizationToken();

                using (ProposalandTasksController ptc = new ProposalandTasksController())
                {
                    ProposalDto pDto = ptc.Get(instanceId, proposalId);

                    response.Data = GetTotals(pDto);

                    response.IsSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                response.Messages.Add("Error retrieving Proposal Pricing Totals");
            }

            return response;
        }

        /// <summary>
        /// Get the Totals for a Proposal
        /// </summary>
        /// <param name="proposalDto">The proposal object</param>
        /// <returns>Totals Data for a Proposal</returns>
        private PricingTotals GetTotals(ProposalDto proposalDto)
        {
            return new PricingTotals
            {
                GrandTotal = Math.Round(proposalDto.Tasks.SelectMany(x => x.ResourceAssignments.SelectMany(y => y.BurdenCost.Where(z => z.Name == "Total Prc"))).Sum(x => decimal.Parse(x.Value)), 2, MidpointRounding.AwayFromZero),
                CostTotal = Math.Round(proposalDto.Tasks.SelectMany(x => x.ResourceAssignments.SelectMany(y => y.BurdenCost.Where(z => z.Name == "Total Cst"))).Sum(x => decimal.Parse(x.Value)), 2, MidpointRounding.AwayFromZero),
                ProfitTotal = Math.Round(proposalDto.Tasks.SelectMany(x => x.ResourceAssignments.SelectMany(y => y.BurdenCost.Where(z => z.Name == "Fee/Prft"))).Sum(x => decimal.Parse(x.Value)), 2, MidpointRounding.AwayFromZero)
            };
        }

        /// <summary>
        /// Get Line Item Data for a Proposal
        /// </summary>
        /// <param name="pDto">The proposal object</param>
        /// <returns>Collection of Line Item Data for a Proposal</returns>
        private ICollection<PricingLineItem> GetLineItems(ProposalDto pDto)
        {
            List<PricingLineItem> totals = pDto.Tasks
                .Select(x => new
                {
                    Name = x.Name,
                    CLIN = x.SummaryFields.First(y => y.Key == "CLIN" || y.Key == "CLIN #").Value,
                    ClinDescription = x.SummaryFields.First(y => y.Key == "CLIN Desc" || y.Key == "CLIN Title").Value,
                    Cost = x.ResourceAssignments.SelectMany(y => y.BurdenCost.Where(z => z.Name == "Total Prc")).Sum(t => decimal.Parse(t.Value))
                })
                .GroupBy(x => x.CLIN)
                .Select(t => new PricingLineItem { Name = t.Key, Description = t.First().ClinDescription, Sum = Math.Round(t.Sum(s => s.Cost), 2, MidpointRounding.AwayFromZero) })
                .OrderBy(x => x.Name)
                .ToList();

            return totals;
        }
    }
}