// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Validates the field is before the given end date field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    [ExcludeFromCodeCoverage]
    public sealed class StartEndDateValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Handle to the validation methods
        /// </summary>
        private IValidationMethods validationMethods = null;

        /// <summary>
        /// Initializes a new instance of the StartEndDateValidationAttribute class
        /// </summary>
        public StartEndDateValidationAttribute()
            : base()
        {
            this.EndDate = string.Empty;
            this.CanBeEqual = false;
            this.ValidIfEitherDateIsNull = true;
            this.ErrorMessage = "StartEndDate validation failed";
            this.DateFormat = null;
            this.validationMethods = IES.Common.classes.GenBOEUnityContainer.Container.Resolve(typeof(IValidationMethods)) as IValidationMethods;
            this.ValidIfOneDateIsNullAndTheOtherIsNot = true;
        }

        /// <summary>
        /// Gets or sets end date
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the start and end date can be equal
        /// </summary>
        public bool CanBeEqual { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return valid if either start or end date are null
        /// </summary>
        public bool ValidIfEitherDateIsNull { get; set; }

        /// <summary>
        /// Gets or sets the format string for any date values
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return valid if one date is null and the other isn't
        /// </summary>
        public bool ValidIfOneDateIsNullAndTheOtherIsNot { get; set; }

        /// <summary>
        /// Checks the dates validitiy
        /// </summary>
        /// <param name="value">Object to validate</param>
        /// <param name="validationContext">The context</param>
        /// <returns>True if valid, otherwise false</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            Type contextType = validationContext.ObjectInstance.GetType();

            if (contextType.GetProperty(this.EndDate) == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "EndDate must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.EndDate,
                        contextType.Name));
            }

            object startDateObject = value;
            object endDateObject = contextType.GetProperty(this.EndDate).GetValue(validationContext.ObjectInstance, null);

            string startDateDisplayName = validationContext.DisplayName;

            // get display name for the end date
            PropertyInfo endDateProp = contextType.GetProperty(this.EndDate);
            object[] attributes = endDateProp.GetCustomAttributes(typeof(DisplayAttribute), false);
            DisplayAttribute displayAttr = attributes.Any() ? attributes.First() as DisplayAttribute : null;
            string endDateDisplayName = (displayAttr == null) ? null : displayAttr.Name;

            string errorMessage = null;
            return this.validationMethods.IsStartEndDateValid(startDateObject, startDateDisplayName, endDateObject, endDateDisplayName, this.DateFormat, this.ValidIfEitherDateIsNull, this.CanBeEqual, this.ValidIfOneDateIsNullAndTheOtherIsNot, out errorMessage) ? ValidationResult.Success : new ValidationResult(errorMessage);                              
        }
    }    
}
