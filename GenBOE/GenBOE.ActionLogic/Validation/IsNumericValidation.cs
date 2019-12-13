// -----------------------------------------------------------------------
// <copyright file="IsNumericValidation.cs" company="IS&GS;">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System.Text.RegularExpressions;
    using IES.Common;

    /// <summary>
    /// Used to check the validation of a string to see if its numerical
    /// </summary>
    public class IsNumericValidation
    {
        /// <summary>
        /// Create static Regex object for IsNumeric.
        /// </summary>
        private static Regex regexIsNumeric = new Regex(@"^\d+$", RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Determines if string is Numeric
        /// </summary>
        /// <param name="sValue">search Text</param>
        /// <returns>bool</returns>
        public bool isNumeric(string sValue)
        {
            return (regexIsNumeric.Match(sValue).Success);
        }


        /// <summary>
        /// Determines if string is an integer
        /// </summary>
        /// <param name="sValue">search Text</param>
        /// <returns>bool</returns>
        public bool isInteger(string sValue)
        {
            bool bResult = false;

            if (this.isNumeric(sValue))
            {
                int myInt;
                bResult = int.TryParse(sValue, out myInt);
            }

            return bResult;
        }
    }
}
