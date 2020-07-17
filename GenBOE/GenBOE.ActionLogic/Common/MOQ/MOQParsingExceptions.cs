// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.MOQ
{
    using System;
    using System.Runtime.Serialization;

    #region Custom Exception Types

    /// <summary>
    /// Indicates that a parsed MOQ Variable contains invalid characters
    /// </summary>
    [Serializable]
    public class GeneralMOQParsingException : Exception
    {
        protected GeneralMOQParsingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public GeneralMOQParsingException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public GeneralMOQParsingException(String message)
            : base(message)
        {
        }

        public GeneralMOQParsingException()
        {
        }
    }

    /// <summary>
    /// Indicates that a MOQ equation being calculated contains an error
    /// </summary>
    [Serializable]
    public class GeneralMOQCalculationException : Exception
    {
        protected GeneralMOQCalculationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public GeneralMOQCalculationException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public GeneralMOQCalculationException(String message)
            : base(message)
        {
        }

        public GeneralMOQCalculationException()
        {
        }
    }

    #endregion Custom Exception Types

}
