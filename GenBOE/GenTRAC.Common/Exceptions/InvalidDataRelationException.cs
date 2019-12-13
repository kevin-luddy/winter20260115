// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Common.Exceptions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Throw when the inner data does not correspond to the parent
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class InvalidDataRelationException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="context">context</param>
        protected InvalidDataRelationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="innerException">inner exception</param>
        public InvalidDataRelationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">message</param>
        public InvalidDataRelationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public InvalidDataRelationException()
            : base()
        {
        }
    }
}
