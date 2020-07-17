// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenTRAC.DataBridge.DTO.Reports
{
    using System.Collections.Generic;

    /// <summary>
    /// interface for Reports Loader
    /// </summary>
    public interface IReportsLoader
    {
        /// <summary>
        /// Get pricers.
        /// </summary>
        /// <returns>pricer user ids</returns>
        ICollection<int> GetPricerUserIds();
            
        /// <summary>
        /// Get a collection of years that a proposal exists in
        /// </summary>
        /// <returns>proposal years</returns>
        ICollection<int> GetProposalYears();
    }
}
