

namespace GenBOE.DataBridge.DTO
{
	using System.Collections.Generic;
	using GenBOE.Dtos;

	/// <summary>
	/// Interface for PLD Proposal Loader
	/// </summary>
	public interface IPldDTODataLoader
	{
		
		/// <summary>
		/// Get All Proposals from PLD database view
		/// </summary>
		/// <returns></returns>
		ICollection<PLDProposalDTO> GetTopProposals(string search = null);

		/// <summary>
		///  Get selected proposal with specific pa number
		/// </summary>
		/// <param name="paNumber"></param>
		/// <returns></returns>
		PLDProposalDTO GetProposalDetails(string paNumber);

		/// <summary>
		///  Get All Active Proposals
		/// </summary>
		/// <param name="active"></param>
		/// <returns></returns>
		ICollection<PLDProposalDTO> GetAllActiveProposals ();

		/// <summary>
		/// Get All Active Proposals by Name
		/// </summary>
		/// <param name="activeNames"></param>
		/// <returns></returns>
		ICollection<string> GetAllActiveProposalNames();
	}
}