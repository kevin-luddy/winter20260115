// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    /// <summary>
    /// Class is used for validation of RTE field lengths
    /// </summary>
    public class RTEValidationMV
    {
        /// <summary>
        /// Field name
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// BoeId
        /// </summary>
        public int BoeId { get; set; }

        /// <summary>
        /// Task Id (if a task)
        /// </summary>
        public int? TaskId { get; set; }

        /// <summary>
        /// Text Value
        /// </summary>
        public string Value { get; set; }
    }
}
