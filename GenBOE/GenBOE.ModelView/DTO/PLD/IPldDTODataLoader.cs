

namespace GenBOE.DataBridge.DTO
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using GenBOE.Dtos;
	using IES.Common;


	public interface IPldDTODataLoader : IDataLoader<ProposalDTO>
	{
		List<ProposalDTO> GetByWorkspaceId(int wsId);
	}

}
