// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception for DB Overload
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class DbQueryOverloadException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="context">context</param>
        protected DbQueryOverloadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="innerException">inner exception</param>
        public DbQueryOverloadException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">message</param>
        public DbQueryOverloadException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DbQueryOverloadException()
            : base()
        {
        }
    }
}
