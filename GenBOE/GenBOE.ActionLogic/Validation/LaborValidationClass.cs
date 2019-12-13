// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using IES.Common.Exceptions;

    /// <summary>
    /// A class used to send the validation errors back and forth (when doing labor task validation).. Holds on to all data needed
    /// </summary>
    public class LaborValidationClass
    {
        /// <summary>
        /// Error Message
        /// </summary>
        public ValidationMessage ErrorMessage { get; set; }

        /// <summary>
        /// Type of the error
        /// </summary>
        public LaborValidationErrorTypeEnum ErrorType { get; set; }

        /// <summary>
        /// Id of the Boe that caused the issue
        /// </summary>
        public int? BoeId { get; set; }

        /// <summary>
        /// Id of the Task Element that caused the issue
        /// </summary>
        public int? TaskElementId { get; set; }

        /// <summary>
        /// Id of the resource type that caused the issue
        /// </summary>
        public int? TypeId { get; set; }

        /// <summary>
        /// Id of the resource spread that caused the issue
        /// </summary>
        public int? SpreadId { get; set; }
    }
}