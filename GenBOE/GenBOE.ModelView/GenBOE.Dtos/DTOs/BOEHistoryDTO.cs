// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    /// <summary>
    /// DTO that contains BOE History data
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class BOEHistoryDTO : IBOEMembership
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BOEHistoryDTO()
        {
            Field = 0;
            OldValue = string.Empty;
            NewValue = string.Empty;
            PerformedByETIUserId = 0;
            Date = DateTime.MinValue;
            BoeID = 0;
        }

        /// <summary>
        /// Gets or sets Field
        /// </summary>
        public FieldType Field { get; set; }

        /// <summary>
        /// Gets or sets OldValue
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// Gets or sets NewValue
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// Gets or sets PerformedByETIUserId
        /// </summary>
        public int PerformedByETIUserId { get; set; }

        /// <summary>
        /// Gets or sets Date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets BoeID
        /// </summary>
        public int BoeID { get; set; }
    }
}
