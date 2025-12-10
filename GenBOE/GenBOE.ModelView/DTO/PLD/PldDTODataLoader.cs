// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Data.Entity;
	using System.Linq;
	using GenBOE.Dtos;
	using GenBOE.PLD.Models;
	using IES.Common;
	using IES.Common.PickList;

	/// <summary>
	/// PLD DTO Data Loader
	/// </summary>
	public class PldDTODataLoader : IPldDTODataLoader, IDisposable
	{

		#region Fields

		/// <summary>
		/// activeStatuses is for the known Active States that the Column in the View returns
		/// </summary>
		private static readonly string[] ACTIVE_STATUSES = new[] { "Open", "Submitted", "Negotiated", "In Negotiation" };

		/// <summary>
		/// boolean for dispose
		/// </summary>
		private bool _disposed;

		/// <summary>
		/// Logger
		/// </summary> 
		private readonly Logger logger;

		/// <summary>
		/// LOB Picklist
		/// </summary>
		private readonly ICollection<PickListDto> lobPickList;

		#endregion

		///
		// ctr PldDTODataLoader
		///
		public PldDTODataLoader(LineOfBusinessDataLoader lineOfBusinessDataLoader)
		{
			if (lineOfBusinessDataLoader == null)
			{
				throw new ArgumentNullException(nameof(lineOfBusinessDataLoader));
			}

			this.logger = new Logger(typeof(PldDTODataLoader));
			this.lobPickList = lineOfBusinessDataLoader.GetPickListValues();
		}

		/// <summary>
		/// Get All Proposals from PLD database view
		/// </summary>
		/// <returns></returns>
		public ICollection<PLDProposalDTO> GetTopProposals(string search = null)
		{

			using (PldDBContext ctx = new PldDBContext())
			{
				IQueryable<PLD.Models.Models.PLDProposal> query = ctx.Proposals.AsNoTracking();

				if (!string.IsNullOrWhiteSpace(search))
				{
					search = search.Trim();

					query = query.Where(p =>
					p.PA_Number.Contains(search) ||
					p.PA_Title.Contains(search));
				}

				DateTime cutoffDate = Utilities.GetPLDCutoffDate();

				query = query.Where(p => p.Last_Modified_Date >= cutoffDate)
					.OrderByDescending(p => p.Last_Modified_Date);


				List<PLDProposalDTO> results = query
					.Select(p => new PLDProposalDTO
					{
						PANumber = p.PA_Number.Trim(),
						Title = p.PA_Title.Trim(),
					})
					.Take(50)
					.ToList();

				return results;
			}

		}

		/// <summary>
		/// Get selected proposal with specific pa number
		/// </summary>
		/// <param name="paNumber"></param>
		/// <returns></returns>
		public PLDProposalDTO GetProposalDetails(string paNumber)
		{
			using (PldDBContext ctx = new PldDBContext())
			{

				PLDProposalDTO result = ctx.Proposals
					.AsNoTracking()
					.Where(p => p.PA_Number == paNumber)
					.Select(p => new PLDProposalDTO
					{
						PANumber = p.PA_Number.Trim(),
						Title = p.PA_Title,
						Description = p.PA_Description,
						Version = p.PA_Version,
						ProjectStartDate = p.Project_Start_Date,
						ProjectEndDate = p.Project_End_Date,
						LastModifiedDate = p.Last_Modified_Date,
						LineOfBusiness = p.Line_of_Business,
						Pricing = p.Pricing,
						RFPNumber = p.RFP_Number,
						ProposalStatus = p.Proposal_Status
					})
					.FirstOrDefault();

				if (result != null)
				{
					DoPostProcessing(new PLDProposalDTO[] { result });
				}

				return result;
			}
		}

		/// <summary>
		///   Get All Active Proposals
		/// </summary>
		/// <param name="active"></param>
		/// <returns></returns>
		public ICollection<PLDProposalDTO> GetAllActiveProposals()
		{
			List<PLDProposalDTO> results = new List<PLDProposalDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(logger))
			{
				try
				{

					using (PldDBContext ctx = new PldDBContext())
					{
						results = ctx.Proposals
							.AsNoTracking()
							.Where(p => ACTIVE_STATUSES.Contains(p.Proposal_Status))
							.Select(p => new PLDProposalDTO
							{
								PANumber = p.PA_Number.Trim(),
								Title = p.PA_Title,
								Description = p.PA_Description,
								Version = p.PA_Version,
								ProjectStartDate = p.Project_Start_Date,
								ProjectEndDate = p.Project_End_Date,
								LastModifiedDate = p.Last_Modified_Date,
								LineOfBusiness = p.Line_of_Business,
								Pricing = p.Pricing,
								RFPNumber = p.RFP_Number,
								ProposalStatus = p.Proposal_Status
							})
							.ToList();
					}
				}
				catch (Exception ex)
				{
					logger.Error(ex, "Error getting all active proposals");
					throw;
				}
			}

			DoPostProcessing(results);

			return results;
		}

		/// <summary>
		/// Get All Active Proposals by Name
		/// </summary>
		/// <param name="activeNames"></param>
		/// <returns></returns>
		public ICollection<string> GetAllActiveProposalNames()
		{
			List<string> results = new List<string>();

			using (StopwatchTimer sw = new StopwatchTimer(logger))
			{
				try
				{

					using (PldDBContext ctx = new PldDBContext())
					{

						results = ctx.Proposals.AsNoTracking()
							.Where(p => ACTIVE_STATUSES.Contains(p.Proposal_Status))
							.Select(p => p.PA_Title)
							.ToList();
					}
				}
				catch (Exception ex)
				{
					logger.Error(ex, "Error getting all active proposals by name");
					throw;
				}
			}

			return results;
		}

		/// <summary>
		/// Get the Last Modified Date for the Proposal with the given PA Number
		/// </summary>
		/// <param name="PaNumber">PA Number of the proposal</param>
		/// <returns>Last Modified Date if exists, otherwise null</returns>
		public DateTime? GetLastModifiedDate(string PaNumber)
		{
			using (PldDBContext ctx = new PldDBContext())
			{
				DateTime? result = ctx.Proposals
					.Where(p => p.PA_Number == PaNumber)
					.Select(p => p.Last_Modified_Date)
					.FirstOrDefault();

				return result;
			}
		}

		/// <summary>
		///  Releases all resources used by the current instance of the class.  This does call the protected method to release unmanaged resources.
		///  This also suppresses finalization to prevent the finalizer from running.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///  Dispose  FxCop friendly
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

		}

		/// <summary>
		/// Do post processing on the Proposals
		/// </summary>
		/// <param name="proposals">The proposals to post process</param>
		private void DoPostProcessing(ICollection<PLDProposalDTO> proposals)
		{
			foreach (PLDProposalDTO proposal in proposals)
			{
				string lobConvertedName = string.Empty;

				switch (proposal.LineOfBusiness.ToLower().Trim())
				{
					case "sac":
						lobConvertedName = "Sikorsky";
						break;
					case "iwss":
						lobConvertedName = "Integrated Warfare Systems and Sensors";
						break;
					case "tls":
						lobConvertedName = "Training and Logistics Solutions";
						break;
					case "new ventures":
						lobConvertedName = "new ventures";
						break;
					case "c6isr":
						lobConvertedName = "c6isr";
						break;
					case "cyber ships and advanced technologies":
					case "cyber, ships & advanced technologies":
						lobConvertedName = "Cyber, Ships & Advanced Technologies";
						break;
					default:
						lobConvertedName = string.Empty;
						break;
				}

				PickListDto match = this.lobPickList.FirstOrDefault(p => string.Equals(p.Text.Trim(), lobConvertedName.Trim(), StringComparison.OrdinalIgnoreCase));

				if (match != null)
				{
					proposal.LineOfBusiness = match.Text;
					proposal.LineOfBusinessId = match.Id;
				}
				else
				{
					proposal.LineOfBusiness = string.Empty;
					proposal.LineOfBusinessId = (int)0;
					continue;
				}
			}
		}
	}
}
