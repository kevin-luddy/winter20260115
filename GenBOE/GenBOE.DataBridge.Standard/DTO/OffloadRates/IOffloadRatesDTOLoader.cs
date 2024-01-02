// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Standard;

    /// <summary>
    /// Interface for the Offload Rates DTO Loader.
    /// </summary>
    /// <seealso cref="GenBOE.DataBridge.IDataLoader{GenBOE.Dtos.OffloadRatesDTO}" />
    public interface IOffloadRatesDTOLoader : IDataLoader<OffloadRatesDTO>
    {
        /// <summary>
        /// Gets all system offload rates.
        /// </summary>
        /// <returns>Collection of offload rates at System level.</returns>
        ICollection<OffloadRatesDTO> GetAllSystemRates();

        /// <summary>
        /// Gets the by workspace identifier.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <returns>Collection of offload rates for a workspace.</returns>
        ICollection<OffloadRatesDTO> GetByWorkspaceId(int workspaceId);

        /// <summary>
        /// Copies the system default offload rates.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        void CopySystemDefaultOffloadRates(int workspaceId);

        /// <summary>
        /// Determines if the workspace offload rates are out of date compared to system rates.
        /// </summary>
        /// <param name="workspaceId">The workspace Id to check.</param>
        /// <returns>Bool that is true if rates are out of date, false if not</returns>
        bool AreCurrentOffloadRatesOutOfDate(int workspaceId);
    }
}
