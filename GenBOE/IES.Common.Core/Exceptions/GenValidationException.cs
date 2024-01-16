// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	/// <summary>
	/// A Validation Exception.
	/// </summary>
	/// <seealso cref="System.Exception" />
	[Serializable()]
	public class GenValidationException : Exception
	{

		public List<ValidationMessage> ValidationList { get; set; }

		protected GenValidationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			ValidationList = new List<ValidationMessage>();
		}

		/// <summary>
		/// Not the recommended method to create
		/// it is recommened that you create this once you have you collection of ValidationMessages
		/// </summary>
		/// <param name="message">message will be added to the ValidationList</param>
		public GenValidationException(String message, Exception innerException)
			: base(message, innerException)
		{
			ValidationList = new List<ValidationMessage>();
			ValidationList.Add(new ValidationMessage() { ValidationIssue = message });
		}

		/// <summary>
		/// Not the recommended method to create
		/// it is recommened that you create this once you have you collection of ValidationMessages
		/// </summary>
		/// <param name="message">message will be added to the ValidationList</param>
		public GenValidationException(String message)
			: base(message)
		{
			ValidationList = new List<ValidationMessage>();
			ValidationList.Add(new ValidationMessage() { ValidationIssue = message });

		}

		/// <summary>
		/// Recommended constructor
		/// </summary>
		/// <param name="ValidationListErrors"></param>
		public GenValidationException(IEnumerable<ValidationMessage> ValidationListErrors)
			: base()
		{
			ValidationList = new List<ValidationMessage>();
			ValidationList.AddRange(ValidationListErrors);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenValidationException"/> class.
		/// </summary>
		/// <param name="message">message will not be added to the Validation List.</param>
		/// <param name="validationMessage">The validation message.</param>
		public GenValidationException(string message, string validationMessage)
			: base(message)
		{
			this.ValidationList = new List<ValidationMessage>();
			ValidationMessage vm = new ValidationMessage(validationMessage);
			this.ValidationList.Add(vm);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenValidationException"/> class.
		/// </summary>
		public GenValidationException()
			: base()
		{
			ValidationList = new List<ValidationMessage>();
		}

		/// <summary>
		/// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
		/// <exception cref="System.ArgumentNullException">info</exception>
		/// <PermissionSet>
		/// </PermissionSet>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			info.AddValue("ValidationList", ValidationList);
			base.GetObjectData(info, context);
		}
	}
}
