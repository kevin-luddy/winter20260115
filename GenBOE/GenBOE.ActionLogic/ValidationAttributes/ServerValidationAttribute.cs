// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ValidationAttributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.Validation;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class ServerValidationAttribute : ValidationAttribute 
    {
        public ValidationType ValidationToPerform { get; set; }
        private ValidationFactory _ValidationFactory;

        public ServerValidationAttribute()
        {
            this._ValidationFactory = ValidationFactory.Instance;
            this.ValidationToPerform = ValidationType.None;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (this.ValidationToPerform == ValidationType.None)
            {
                throw new IES.Common.Exceptions.ValidationException("Validator does not exist.");
            }

            return this._ValidationFactory.getValidator(this.ValidationToPerform).isValid(value, null);
        }
    }
}