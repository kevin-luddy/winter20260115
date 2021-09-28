// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.OfficeUtilities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [ExcludeFromCodeCoverage]
    internal static class CSVUtilities
    {
       
    }

    /// <summary>
    /// Indicates that an imported file was not a text file.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class NotTextCSVFileException : Exception
    {
        protected NotTextCSVFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public NotTextCSVFileException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NotTextCSVFileException(String message)
            : base(message)
        {
        }

        public NotTextCSVFileException()
            : base()
        {
        }
    }

    /// <summary>
    /// Indicates that a required column was missing from the imported file
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class IncorrectColumnCountException : Exception
    {
        protected IncorrectColumnCountException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public IncorrectColumnCountException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public IncorrectColumnCountException(String message)
            : base(message)
        {
        }

        public IncorrectColumnCountException()
            : base()
        {
        }
    }
}
