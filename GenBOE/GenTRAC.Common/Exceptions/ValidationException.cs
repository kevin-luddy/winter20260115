// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Common.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Throw when validation fails.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class ValidationException : Exception
    {
        /// <summary>
        /// List of validation errors
        /// </summary>
        public ICollection<ValidationMessage> ValidationList { get; set; }

        /// <summary>
        /// Recommended constructor
        /// </summary>
        /// <param name="validationListErrors">Collection of errors</param>
        public ValidationException(ICollection<ValidationMessage> validationListErrors)
            : base()
        {
            this.ValidationList = new List<ValidationMessage>();
            ((List<ValidationMessage>)this.ValidationList).AddRange(validationListErrors);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="context">context</param>
        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.ValidationList = new List<ValidationMessage>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception</param>
        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.ValidationList = new List<ValidationMessage>() { new ValidationMessage(message) };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error message</param>
        public ValidationException(string message)
            : base(message)
        {
            this.ValidationList = new List<ValidationMessage>() { new ValidationMessage(message) };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="pkid">Pkid causing the error</param>
        public ValidationException(string message, int pkid)
            : base(message)
        {
            this.ValidationList = new List<ValidationMessage>() { new ValidationMessage(message) { PkId = pkid } };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ValidationException()
            : base()
        {
            this.ValidationList = new List<ValidationMessage>();
        }

        /// <summary>
        /// For serialization
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="context">context</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ValidationList", this.ValidationList);
        }
    }
}
