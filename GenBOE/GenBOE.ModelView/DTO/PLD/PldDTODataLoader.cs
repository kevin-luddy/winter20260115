

namespace GenBOE.DataBridge.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;
	using IES.Common;
	using GenBOE.Dtos;
	using GenBOE.Models;
	using GenBOE.PLD.Models;

	public class PldDTODataLoader : DataLoader<ProposalDTO>, IPldDTODataLoader, IDisposable
	{
		private readonly PldDBContext _context;
		private bool _disposed;

		public PldDTODataLoader(PldDBContext context = null)
		{
			_context = context ?? new PldDBContext();
			this.Log = new Logger(typeof(PldDTODataLoader));
		}
		public override ICollection<ProposalDTO> GetByIds(ICollection<int> ids)
		{
			throw new NotImplementedException();
		}

		public List<ProposalDTO> GetByWorkspaceId(int wsId)
		{
			List<ProposalDTO> results = new List<ProposalDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(Log))
			{
				try
				{
					results = _context.Proposals
						.Where(p => p.WorkspaceID == wsId)
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
				catch(Exception ex)
				{
					Log.Error(ex, "Error getting proposals by Workspace ID");
					throw;
				}
			}

			return results;
		}

		protected override int? Delete(ProposalDTO dtoToDelete)
		{
			throw new NotImplementedException();
		}

		protected override int? Upsert(ProposalDTO dtoToUpsert)
		{
			throw new NotImplementedException();
		}


		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

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

		~PldDTODataLoader()
		{
			Dispose(false);
		}

	}

}
