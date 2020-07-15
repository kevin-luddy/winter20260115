// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace IES.Common.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    [ExcludeFromCodeCoverage]
    public class ValidationException : Exception
    {
        public string Title { get; set; }
		
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
            Title = string.Empty;
			this.ValidationList = new List<ValidationMessage>();
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
            Title = string.Empty;
			this.ValidationList = new List<ValidationMessage>() { new ValidationMessage(message) };
        }

        public ValidationException(string message)
            : base(message)
        {
            Title = string.Empty;
			this.ValidationList = new List<ValidationMessage>() { new ValidationMessage(message) };
        }

        public ValidationException(String message, String title)
            : base(message)
        {
            Title = title;
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

        
        public ValidationException()
            : base()
        {
            Title = string.Empty;
			this.ValidationList = new List<ValidationMessage>();
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("Title", Title);
			info.AddValue("ValidationList", this.ValidationList);
            base.GetObjectData(info, context);
        }
    }
}
