// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;

    /// <summary>
    /// Bulk Archive Dto Class
    /// </summary>
    [Serializable]
    public class BulkArchiveDto
    {
        /// <summary>
        /// Start Date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Line of Business
        /// </summary>
        public string LineOfBusiness { get; set; }

        /// <summary>
        /// Program Area
        /// </summary>
        public string ProgramArea { get; set; }
    }
}
