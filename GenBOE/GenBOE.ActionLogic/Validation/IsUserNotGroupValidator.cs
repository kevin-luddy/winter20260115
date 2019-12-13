// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class IsUserNotGroupValidator : Validator
    {
        /// <summary>
        /// Determines if a value is valid
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Collection of reasons why not valid</returns>
        public override Collection<String> validation(object value, Collection<Dictionary<String, String>> inData)
        {
            Collection<String> response = new Collection<string>();
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.GetType() != typeof(string))
            {
                throw new InvalidCastException("value");
            }

            string valueToValidate = (string)value;

            // Check to see if the user is a group..
            if (valueToValidate.Contains("."))
            {
                response.Add("Cost Volume Lead/Pricer must be an individual and not a group.");
            }

            return response;
        }
    }
}
