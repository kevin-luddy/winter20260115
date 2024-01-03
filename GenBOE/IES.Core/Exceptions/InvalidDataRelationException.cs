// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace IES.Core.Exceptions
{
	using System;
    using System.Diagnostics.CodeAnalysis;
	using System.Runtime.Serialization;
    
	[Serializable]
    [ExcludeFromCodeCoverage]
    public class InvalidDataRelationException : Exception
    {
        protected InvalidDataRelationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public InvalidDataRelationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InvalidDataRelationException(string message)
            : base(message)
        {
        }

        public InvalidDataRelationException()
            : base()
        {
        }
    }
}
