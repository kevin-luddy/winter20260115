// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Common;

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
