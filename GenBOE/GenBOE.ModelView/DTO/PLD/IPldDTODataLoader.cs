

namespace GenBOE.DataBridge.DTO
{
	using System.Collections.Generic;
	using GenBOE.Dtos;
	using IES.Common;


	public interface IPldDTODataLoader
	{
		
		/// <summary>
		/// Get All Proposals from PLD database view
		/// </summary>
		/// <returns></returns>
		ICollection<ProposalDTO> GetAllProposals();

		/// <summary>
		///  Get All Active Proposals
		/// </summary>
		/// <param name="active"></param>
		/// <returns></returns>
		ICollection<ProposalDTO> GetAllActiveProposals (int active);

		/// <summary>
		/// Get All Active Proposals by Name
		/// </summary>
		/// <param name="activeNames"></param>
		/// <returns></returns>
		ICollection<string> GetAllActiveProposalNames(int activeNames);

		/// <summary>
		/// Get Proposal by ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		ICollection<ProposalDTO> GetByIds(ICollection<string> paNumbers);

	}

}
