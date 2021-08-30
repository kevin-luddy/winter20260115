// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ValidationAttributes
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// This class provided Range validation based on a boolean dependent property.  
    /// If the property is set to true, then override the validation and return valid.
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class RangeIfAttribute : RangeAttribute
    {
        public string DependentProperty { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeIfAttribute"/> class.
        /// </summary>
        /// <param name="minimum">The minimum.</param>
        /// <param name="maximum">The maximum.</param>
        /// <param name="dependentProperty">The dependent property.</param>
        /// <exception cref="System.ArgumentNullException">dependentProperty</exception>
        public RangeIfAttribute(int minimum, int maximum, string dependentProperty)
            : base(minimum, maximum)
        {
            if (dependentProperty == null)
            {
                throw new ArgumentNullException(nameof(dependentProperty));
            }
            this.DependentProperty = dependentProperty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeIfAttribute"/> class.
        /// </summary>
        /// <param name="minimum">The minimum.</param>
        /// <param name="maximum">The maximum.</param>
        /// <param name="dependentProperty">The dependent property.</param>
        /// <exception cref="System.ArgumentNullException">dependentProperty</exception>
        public RangeIfAttribute(double minimum, double maximum, string dependentProperty)
            : base(minimum, maximum)
        {
            if (dependentProperty == null)
            {
                throw new ArgumentNullException(nameof(dependentProperty));
            }
            this.DependentProperty = dependentProperty;
        }

        /// <summary>
        /// Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            // get a reference to the property this validation depends upon
            var containerType = validationContext.ObjectInstance.GetType();
            var field = containerType.GetProperty(this.DependentProperty);

            if (field != null)
            {
                // get the value of the dependent property
                var dependentvalue = field.GetValue(validationContext.ObjectInstance, null);

                // compare the value against the target value
                if (dependentvalue != null && dependentvalue.Equals(true))
                {
                    // Validation is overridden
                    return ValidationResult.Success;
                }
            }
            
            return base.IsValid(value, validationContext);
        }
    }
}
