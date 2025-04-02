// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Common.MOQ
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

		public GeneralMOQParsingException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public GeneralMOQParsingException(string message)
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

		public GeneralMOQCalculationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public GeneralMOQCalculationException(string message)
			: base(message)
		{
		}

		public GeneralMOQCalculationException()
		{
		}
	}

	#endregion Custom Exception Types

}
