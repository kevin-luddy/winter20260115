/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Controllers
{
	using APTSPropricerApi.Common;
	using APTSPropricerApi.Connection;
	using APTSPropricerApi.DTOs;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// The Pro Pricer Data API Controller for ACV to call
	/// </summary>
	[Authorize(AuthenticationSchemes = Constants.IES_TOKEN_SCHEME)]
	[ApiController]
	[Route("api/ProPricerData")]
	public class ProPricerDataApiController : ControllerBase
	{
		/// <summary>
		/// The logger for the controller
		/// </summary>
		private readonly ILogger logger;

		/// <summary>
		/// Pool Manager
		/// </summary>
		private readonly PoolManagerList poolManagerList;

		/// <summary>
		/// #ctor
		/// </summary>
		public ProPricerDataApiController(ILogger<ProPricerDataApiController> logger, PoolManagerList poolManagerList)
		{
			this.logger = logger;
			this.poolManagerList = poolManagerList;
		}

		/// <summary>
		/// Gets the Proposals
		/// </summary>
		/// <param name="instanceId">The connection instance id to retrieve proposals on</param>
		/// <returns>A list of proposals inside folders</returns>
		[Route("Proposals/{instanceId}")]
		[Authorize]
		[HttpGet]
		public ProPricerResponse<ICollection<ProposalFolderInfo>> Get(int instanceId)
		{
			ProPricerResponse<ICollection<ProposalFolderInfo>> response = new();

			try
			{
				using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
				{
					response.Data = Utility.GetAllProposals(ppc, logger);
					response.IsSuccessful = true;
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error retrieving proposals from Pro Pricer");
				response.Messages.Add("Error retrieving proposals from Pro Pricer");
			}

			return response;
		}

		/// <summary>
		/// Gets the Available Connections.
		/// </summary>
		/// <returns>Returns a collection of the pool manager instances.</returns>
		[Route("AvailableConnections")]
		[HttpGet]
		public ProPricerResponse<ICollection<PoolInstanceDto>> Get()
		{
			ProPricerResponse<ICollection<PoolInstanceDto>> response = new();

			try
			{
				response.Data = Utility.GetAllPoolInstances(poolManagerList, logger);
				response.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error retrieving Pool Instances");
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
		[Route("PricingData/{instanceId}/{proposalId}")]
		[Authorize]
		[HttpGet]
		public ProPricerResponse<PricingData> GetPricingData(int instanceId, string proposalId)
		{
			ProPricerResponse<PricingData> response = new();
			try
			{
				ProposalDto pDto = Utility.GetProposal(poolManagerList, logger, instanceId, proposalId);
				pDto.Tasks = Utility.GetTasksForProposal(poolManagerList, logger, instanceId, proposalId);

				response.Data = new PricingData
				{
					Totals = GetTotals(pDto),
					LineItems = GetLineItems(pDto)
				};

				response.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error retrieving Proposal Pricing Data");
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
		[Route("PricingDataTotals/{instanceId}/{proposalId}")]
		[Authorize]
		[HttpGet]
		public ProPricerResponse<PricingTotals> GetPricingDataTotals(int instanceId, string proposalId)
		{
			ProPricerResponse<PricingTotals> response = new();
			try
			{
				ProposalDto pDto = Utility.GetProposal(poolManagerList, logger, instanceId, proposalId);
				pDto.Tasks = Utility.GetTasksForProposal(poolManagerList, logger, instanceId, proposalId);

				response.Data = GetTotals(pDto);

				response.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error retrieving Proposal Pricing Totals");
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
				GrandTotal = Math.Round(proposalDto.Tasks.SelectMany(x => x.ResourceAssignments.SelectMany(y => y.BurdenCost.Where(z => z.Name == Constants.TOTAL_PRICE))).Sum(x => decimal.Parse(x.Value)), 2, MidpointRounding.AwayFromZero),
				CostTotal = Math.Round(proposalDto.Tasks.SelectMany(x => x.ResourceAssignments.SelectMany(y => y.BurdenCost.Where(z => z.Name == Constants.TOTAL_COST))).Sum(x => decimal.Parse(x.Value)), 2, MidpointRounding.AwayFromZero),
				ProfitTotal = Math.Round(proposalDto.Tasks.SelectMany(x => x.ResourceAssignments.SelectMany(y => y.BurdenCost.Where(z => z.Name == Constants.FEE_PROFIT))).Sum(x => decimal.Parse(x.Value)), 2, MidpointRounding.AwayFromZero)
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
					CLIN = x.SummaryFields.First(y => y.Key?.ToUpper() == Constants.CLIN || y.Key?.ToUpper() == Constants.CLIN_NUMBER)?.Value ?? String.Empty,
					ClinDescription = x.SummaryFields.First(y => y.Key?.ToUpper() == Constants.CLIN_DESC || y.Key?.ToUpper() == Constants.CLIN_TITLE)?.Value ?? String.Empty,
					Cost = x.ResourceAssignments.SelectMany(y => y.BurdenCost.Where(z => z.Name == Constants.TOTAL_PRICE)).Sum(t => decimal.Parse(t.Value))
				})
				.GroupBy(x => x.CLIN)
				.Select(t => new PricingLineItem { Name = t.Key, Description = t.FirstOrDefault()?.ClinDescription, Sum = Math.Round(t.Sum(s => s.Cost), 2, MidpointRounding.AwayFromZero) })
				.OrderBy(x => x.Name)
				.ToList();

			return totals;
		}
	}
}