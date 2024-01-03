// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Validation to ensure value is one of the valid values defined by the annotation
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Accessors defined, but code analysis still complaining.")]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ValidValuesAttribute : ValidationAttribute
    {
        /// <summary>
        /// Array of valid values passed in as part of annotation
        /// </summary>
        private readonly string[] validValues;

        public ValidValuesAttribute(params string[] validValues)
        {
            this.validValues = validValues;
        }

        /// <summary>
        /// Valid if value exists in validValues array
        /// </summary>
        /// <param name="value">object being validated</param>
        /// <returns>Returns true if value exists in validValues array</returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                value = string.Empty;
            }
            return this.validValues.Contains(value.ToString());
        }
    }
}