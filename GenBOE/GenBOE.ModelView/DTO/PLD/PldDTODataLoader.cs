

namespace GenBOE.DataBridge.DTO
{
	using GenBOE.Dtos;
	using GenBOE.Models;
	using GenBOE.PLD.Models;
	using IES.Common;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Runtime.Remoting.Contexts;
	using System.Text;

	/// <summary>
	/// PLD DTO Data Loader
	/// </summary>
	public class PldDTODataLoader : IPldDTODataLoader, IDisposable
	{

		#region Fields
		/// <summary>
		/// Instance of the PLD DbContext
		/// </summary>
		private readonly PldDBContext _context;

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
		protected Logger Log { get; set; }

		#endregion

		public PldDTODataLoader()
		{
			_context = new PldDBContext();
			this.Log = new Logger(typeof(PldDTODataLoader));
		}

		public PldDTODataLoader(PldDBContext context = null)
		{
			_context = context ?? new PldDBContext();
			this.Log = new Logger(typeof(PldDTODataLoader));
		}
		
		/// <summary>
		/// Get Proposal by ID   "PA Number" is a string  
		/// </summary>
		/// <param name="paNumbers"></param>
		/// <returns></returns>
		public ICollection<ProposalDTO> GetByIds(ICollection<string> paNumbers)
		{
			List<ProposalDTO> results = new List<ProposalDTO>();

			if(paNumbers == null || paNumbers.Count == 0)
			{
				return results;
			}

			using (StopwatchTimer sw = new StopwatchTimer(Log))
			{
				try
				{
					results = _context.Proposals
						.Where(p => paNumbers.Contains(p.PA_Number))
						.Select(p => new ProposalDTO
						{
							PA_Number = p.PA_Number,
							PA_Title = p.PA_Title,
							PA_Description = p.PA_Description,
							PA_Version = p.PA_Version,
							Project_Start_Date = p.Project_Start_Date,
							Project_End_Date = p.Project_End_Date,
							Last_Modified_Date = p.Last_Modified_Date,
							Line_of_Business = p.Line_of_Business,
							Pricing = p.Pricing,
							RFP_Number = p.RFP_Number,
							Proposal_Status = p.Proposal_Status
						})
						.ToList();

				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error getting proposals by IDs");
					throw;
				}
			}

			return results;
		}

		/// <summary>
		/// Get All Proposals from PLD database view
		/// </summary>
		/// <returns></returns>
		public ICollection<ProposalDTO> GetAllProposals()
		{
			List<ProposalDTO> results = new List<ProposalDTO>();
			
			using (StopwatchTimer sw = new StopwatchTimer(Log))
			{
				try
				{
					results = _context.Proposals				
						.Select(p => new ProposalDTO
						{
							PA_Number = p.PA_Number,
							PA_Title = p.PA_Title,
							PA_Description = p.PA_Description,
							PA_Version = p.PA_Version,
							Project_Start_Date = p.Project_Start_Date,
							Project_End_Date = p.Project_End_Date,
							Last_Modified_Date = p.Last_Modified_Date,
							Line_of_Business = p.Line_of_Business,
							Pricing = p.Pricing,
							RFP_Number = p.RFP_Number,
							Proposal_Status = p.Proposal_Status
						})
						.ToList();

				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error getting all proposals");
					throw;
				}
			}

			return results;
		}

		/// <summary>
		///   Get All Active Proposals
		/// </summary>
		/// <param name="active"></param>
		/// <returns></returns>
		public ICollection<ProposalDTO> GetAllActiveProposals()
		{
			List<ProposalDTO> results = new List<ProposalDTO>();
			
			using (StopwatchTimer sw = new StopwatchTimer(Log))
			{
				try
				{
					

					results = _context.Proposals
						.Where(p => ACTIVE_STATUSES.Contains(p.Proposal_Status))
						.Select(p => new ProposalDTO
						{
							PA_Number = p.PA_Number.Trim(),
							PA_Title = p.PA_Title,
							PA_Description = p.PA_Description,
							PA_Version = p.PA_Version,
							Project_Start_Date = p.Project_Start_Date,
							Project_End_Date = p.Project_End_Date,
							Last_Modified_Date = p.Last_Modified_Date,
							Line_of_Business = p.Line_of_Business,
							Pricing = p.Pricing,
							RFP_Number = p.RFP_Number,
							Proposal_Status = p.Proposal_Status
						})
						.ToList();

				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error getting all active proposals");
					throw;
				}
			}

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

			using (StopwatchTimer sw = new StopwatchTimer(Log))
			{
				try
				{
					results = _context.Proposals
						.Where(p => ACTIVE_STATUSES.Contains(p.Proposal_Status))
						.Select(p => p.PA_Title)
						.ToList();

				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error getting all active proposals by name");
					throw;
				}
			}

			return results;
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
		/// Releases the unmanaged resources used and optionally releases managed resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (_context != null)
				{
					_context.Dispose();
				}
			}

			_disposed = true;
		}

		/// <summary>
		///   Finalizer that ensures resources are released if Dispose was not called.
		/// </summary>
		~PldDTODataLoader()
		{
			Dispose(false);
		}

	}

}
