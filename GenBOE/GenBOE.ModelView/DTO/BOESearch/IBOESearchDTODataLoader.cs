// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using GenBOE.Dtos;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for BOESearchDTODataLoader
    /// </summary>
    public interface IBOESearchDTODataLoader
    {
        /// <summary>
        /// Gets the ids of all BOES  matching the advanced search parameters.
        /// </summary>
        /// <param name="inSearchParams">Advanced Search parameters.</param>
        /// <returns>BOE and Workspace ids of all matching BOEs.</returns>
        ICollection<BOESearchResultDTO> GetAdvancedSearchResults(BOESearchDTO inSearchParams);

        /// <summary>
        /// Gets the ids of all BOES  matching the advanced search parameters.
        /// </summary>
        /// <param name="inSearchParams">Advanced Search parameters.</param>
        /// <returns>BOE and Workspace ids of all matching BOEs.</returns>
        ICollection<BOESearchResultDTO> GetAdvancedSearchResults(BOEProjectMapSearchDTO inSearchParams);

        /// <summary>
        /// Gets the ids of all BOES matching the quick search parameters.
        /// </summary>
        /// <param name="inSearchParams">Quick Search parameters.</param>
        /// <returns>BOE and Workspace ids of all matching BOEs.</returns>
        ICollection<BOESearchResultDTO> GetQuickSearchResults(BOESearchDTO inSearchParams);

        /// <summary>
        /// Gets the ids of all BOES matching the quick search parameters.
        /// </summary>
        /// <param name="inSearchParams">Quick Search parameters.</param>
        /// <returns>BOE and Workspace ids of all matching BOEs.</returns>
        ICollection<BOESearchResultDTO> GetQuickSearchResults(BOEProjectMapSearchDTO inSearchParams);

        /// <summary>
        /// Gets the search results.
        /// </summary>
        /// <param name="projectMapIds">The projectMap ids.</param>
        /// <returns>A list of search results.</returns>
        ICollection<BOESearchResult> GetSearchResults(ICollection<int> projectMapIds);
    }
}
