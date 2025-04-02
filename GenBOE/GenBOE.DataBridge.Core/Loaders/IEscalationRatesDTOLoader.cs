// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Loaders
{
	using System.Collections.Generic;
	using GenBOE.DataBridge.Core.DTO;
	using IES.Common;
	using IES.Common.Core.Loaders;

	public interface IEscalationRatesDTOLoader : IDataLoader<EscalationRatesDTO>
	{
		/// <summary>
		/// Gets all.
		/// </summary>
		/// <returns></returns>
		ICollection<EscalationRatesDTO> GetAll();

		/// <summary>
		/// Gets the by workspace.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <returns></returns>
		ICollection<EscalationRatesDTO> GetByWorkspace(WorkspaceDTO workspace);
	}
}
