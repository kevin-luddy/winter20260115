// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Linq;
    using GenTRAC.Models;
    using IES.Common;

    /// <summary>
    /// Bulk Archive Loader
    /// </summary>
    public class BulkArchiveLoader : IBulkArchiveLoader
    {
        /// <summary>,
        /// Logger
        /// </summary>
        protected Logger Log { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public BulkArchiveLoader()
        {
            this.Log = new Logger(typeof(BulkArchiveLoader));
        }

        /// <summary>
        /// Apply Bulk Archive for the given BulkArchiveDTO's filters.
        /// </summary>
        /// <param name="dto">Bulk Archive DTO</param>
        /// <returns>Number of proposals archived</returns>
        public int ApplyBulkArchive(BulkArchiveDto dto)
        {
            int toReturn = 0;

            using (StopwatchTimer sw = new StopwatchTimer("BulkArchiveLoader.ApplyBulkArchive", this.Log))
            {
                if (dto != null)
                {
                    // Call "toUpper()" for Line of Business and Program Area since the Stored 
                    // Procedure expects the "All" case to be capitalized
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        toReturn = dbModel.archiveProposal(
                            dto.StartDate,
                            dto.EndDate,
                            dto.LineOfBusiness.ToUpper(),
                            dto.ProgramArea.ToUpper()).FirstOrDefault().GetValueOrDefault();
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Search Bulk Archive for the given BulkArchiveDTO's filters.
        /// </summary>
        /// <param name="dto">Bulk Archive DTO</param>
        /// <returns>Number of proposals archived</returns>
        [DbQuery]
        public int? SearchBulkArchive(BulkArchiveDto dto)
        {
            int? toReturn = 0;

            using (StopwatchTimer sw = new StopwatchTimer("BulkArchiveLoader.SearchBulkArchive", this.Log))
            {
                if (dto != null)
                {
                    // Call "toUpper()" for Line of Business and Program Area since the Stored 
                    // Procedure expects the "All" case to be capitalized
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        toReturn = dbModel.archiveProposalCount(
                            dto.StartDate,
                            dto.EndDate,
                            dto.LineOfBusiness.ToUpper(),
                            dto.ProgramArea.ToUpper()).FirstOrDefault();
                    }
                }
            }

            return toReturn;
        }
    }
}