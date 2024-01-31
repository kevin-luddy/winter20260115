// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    /// <summary>
    /// Test Model Helper, used to test validation directly on models.
    /// </summary>
    public sealed class TestModelHelper
    {
        /// <summary>
        /// TestModelHelper Constructor
        /// </summary>
        private TestModelHelper()
        {
        }

        /// <summary>
        /// Validate validates a models validation attributes
        /// </summary>
        /// <param name="model">z</param>
        /// <returns>n</returns>
        public static IList<ValidationResult> Validate(object model)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            ValidationContext validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, results, true);
            if (model is IValidatableObject)
            {
                (model as IValidatableObject).Validate(validationContext);
            }

            return results;
        }
    }
}