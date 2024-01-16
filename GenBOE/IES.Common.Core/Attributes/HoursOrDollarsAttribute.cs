// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Attributes
{
	using System;
	using System.ComponentModel.DataAnnotations;
	using IES.Common.Core.Models;

	/// <summary>
	/// Ensures that at least one and not both Hours and Dollars is non-zero
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class HoursOrDollarsAttribute : ValidationAttribute
    {
        /// <summary>
        /// Valid if at least one and not both Hours and Dollars is non-zero
        /// </summary>
        /// <param name="value">object being validated</param>
        /// <returns>Returns true if at least one and not both Hours and Dollars is non-zero</returns>
        public override bool IsValid(object value)
        {
            return true;
        }

        /// <summary>
        /// Valid if at least one and not both Hours and Dollars is non-zero
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>Returns true if at least one and not both Hours and Dollars is non-zero</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            var model = validationContext.ObjectInstance as ProjectMapModelView;
            if ((model.Dollars ?? 0) == 0 && (model.Hours ?? 0) == 0 ||
                model.Dollars.HasValue && model.Dollars != 0 && model.Hours.HasValue && model.Hours != 0)
            {
                return new ValidationResult(ErrorMessage);
            }

            return base.IsValid(value, validationContext);
        }
    }
}