// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// / TODO - refactor the validators WI 29245
    /// </summary>
    public abstract class Validator
    {
        /// <summary>
        /// Determines if a value is valid
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public bool isValid(object value, Collection<Dictionary<string, string>> inData)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            return (validation(value, inData).Count == 0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public abstract Collection<string> validation(object value, Collection<Dictionary<string, string>> inData);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual Collection<string> validation(object value, Collection<Dictionary<string, object>> inData)
        {
            throw  new NotImplementedException("validation is not implemented.");
        }

        static public String CreateValidationErrorResponse(ICollection<String> ValidationMessages)
        {
            if (ValidationMessages == null)
            {
                return "";
            }

            StringBuilder errors = new StringBuilder();
            foreach (String message in ValidationMessages.Distinct())
            {
                errors.AppendLine(message);
            }
            return errors.ToString();
        }
    }
}
