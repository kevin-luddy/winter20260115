// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;

    /// <summary>
    /// BOE DTO for previous version of a workspace
    /// </summary>
    public class BoeVersionDTO
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public BoeVersionDTO()
        {
            BoeId = -1;
            BoeTitle = string.Empty;
            WbsNumber = String.Empty;
            Wbs = String.Empty;
            ClinNumber = String.Empty;
            Clin = String.Empty;
        }

        /// <summary>
        /// BOE ID
        /// </summary>
        public int BoeId { get; set; }

        /// <summary>
        /// BOE Title
        /// </summary>
        public string BoeTitle { get; set; }

        /// <summary>
        /// WBS Number
        /// </summary>
        public string WbsNumber { get; set; }

        /// <summary>
        /// WBS Title
        /// </summary>
        public string Wbs { get; set; }

        /// <summary>
        /// CLIN Number
        /// </summary>
        public string ClinNumber { get; set; }

        /// <summary>
        /// CLIN Title
        /// </summary>
        public string Clin { get; set; }
    }
}
