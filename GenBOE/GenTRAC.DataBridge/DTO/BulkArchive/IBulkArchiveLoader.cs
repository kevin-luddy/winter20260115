// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    /// <summary>
    /// Interface for Bulk Archive Loader
    /// </summary>
    public interface IBulkArchiveLoader
    {
        /// <summary>
        /// Archive the proposals
        /// </summary>
        /// <param name="dto">Bulk Archive DTO</param>
        /// <returns>Number of proposals archived</returns>
        int ApplyBulkArchive(BulkArchiveDto dto);

        /// <summary>
        /// Search only for the affected proposals.
        /// </summary>
        /// <param name="dto">Bulk Archive DTO</param>
        /// <returns>Number of proposals found</returns>
        int? SearchBulkArchive(BulkArchiveDto dto);
    }
}