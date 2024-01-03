// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using GenTRAC.DataBridge.DTO.Contracts;
    using IES.Core;
    using System.Collections.Generic;

    /// <summary>
    /// Cage Codes Loader Interface
    /// </summary>
    public interface ICageCodesLoader
    {
        /// <summary>
        /// Get all Cage Codes data
        /// </summary>
        /// <returns>All Cage Codes information</returns>
        ICollection<CageCodeDTO> GetAllCageCodesData();

        /// <summary>
        /// Gets the data associated to a cage code by a given cage code.
        /// </summary>
        /// <param name="cageCode">Cage Code</param>
        /// <returns>Specified cage code data</returns>
        CageCodeDTO GetDataByCageCode(string cageCode);
    }
}