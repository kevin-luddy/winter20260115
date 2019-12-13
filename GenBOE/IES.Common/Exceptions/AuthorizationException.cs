// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Exceptions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [Serializable]
    [ExcludeFromCodeCoverage]
    public class AuthorizationException : Exception
    {
        protected AuthorizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public AuthorizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AuthorizationException(string message)
            : base(message)
        {
        }

        public AuthorizationException()
            : base()
        {
        }
    }
}