// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;

    public interface IMiscTravelRateDTOLoader
    {
        /// <summary>
        /// Gets a collection of MiscTraveRateDTOs for a given list of Ids.
        /// </summary>
        /// <param name="travelRateIds">Travel Rate Ids.</param>
        /// <returns>MiscTraveRateDTOs</returns>
        ICollection<MiscTravelRateDTO> GetByIds(ICollection<int> travelRateIds);

        /// <summary>
        /// Retrieves a locked <see cref="MiscTravelRateDTO"/>s from the database if the workspace is locked, otherwise retrieves
        /// system misc travel rates.
        /// </summary>
        /// <param name="inMiscTravelRateIds">The IDs of the <see cref="MiscTravelRateDTO"/>s being retrieved</param>
        /// <param name="inWorkspace">The Workspace dto.</param>
        /// <returns>The <see cref="MiscTravelRateDTO"/>s requested</returns>
        ICollection<MiscTravelRateDTO> GetByIds(ICollection<int> inMiscTravelRateIds, WorkspaceDTO inWorkspace);

        MiscTravelRateDTO GetById(int inTravelRateID);
        MiscTravelRateDTO GetById(int inTravelRateID, WorkspaceDTO inWorkspace);
        Collection<MiscTravelRateDTO> GetAll();
        Dictionary<int, int> SaveMiscTravelRates(Collection<MiscTravelRateDTO> inMiscTravelRateDTOs);
    }
}
