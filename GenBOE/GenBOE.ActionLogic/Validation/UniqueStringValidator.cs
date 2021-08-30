// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;

    public class UniqueStringValidatorHelper
    {
        /// <summary>
        /// Validates whether a collection of strings is unique.
        /// </summary>
        /// <param name="inStringValue">Collection of Strings</param>
        /// <returns>Boolean Value if the string is </returns>
        public Boolean Validator(ICollection<string> inStringValue)
        {
            if (inStringValue == null)
            {
                throw new ArgumentNullException(nameof(inStringValue));
            }

            Boolean isUnique = true;

            foreach (string wbsNumber in inStringValue)
            {
                int count = 0;

                foreach (string wbsNumber2 in inStringValue)
                {
                    if (wbsNumber.ToLower() == wbsNumber2.ToLower())
                    {
                        count++;
                    }
                }

                if (count != 1)
                {
                    isUnique = false;
                }
            }

            return isUnique;
        }
    }
}
