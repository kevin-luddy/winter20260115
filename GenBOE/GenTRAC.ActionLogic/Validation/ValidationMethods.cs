// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Validation
{
    using System;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Contains validation methods for objects in the system.
    /// </summary>
    public class ValidationMethods : GenTRAC.ActionLogic.Validation.IValidationMethods
    {
        /// <summary>
        /// The default date format
        /// </summary>
        private const string DEFAULT_DATE_FORMAT = "MM/dd/yyyy";

        /// <summary>
        /// The active directory utilities
        /// </summary>
        private IES.Common.IActiveDirectoryUtilities adUtilities = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inAdUtilities">Active Directory Utilities</param>
        public ValidationMethods(IES.Common.IActiveDirectoryUtilities inAdUtilities)
        {
            this.adUtilities = inAdUtilities;
        }

        #region General Validation

        /// <summary>
        /// Test if two dates are valid dates and in the correct order
        /// </summary>
        /// <param name="startDateObject">Start Date</param>
        /// <param name="startDateDisplayName">Display name for the Start Date</param>
        /// <param name="endDateObject">End Date</param>
        /// <param name="endDateDisplayName">Display name for the End Date</param>
        /// <param name="dateFormat">Date format required for the Start and End Date</param>
        /// <param name="isValidWhenEitherDateIsNull">What to do when a value is null</param>
        /// <param name="isValidWhenDatesAreEqual">What to do if the values are equal</param>
        /// <param name="isValidWhenOneDateIsNullAndTheOtherIsNot">What do if one value is null and the other isn't</param>
        /// <param name="errorMessage">If validation fails, this is the error message</param>
        /// <returns>True if valid, else false</returns>
        /// SUPPRESSION NOTE: WILL BE CHECKING THE INPUT PARAMETERS IN A UNIQUE WAY WITH THE BOOLEANS, IGNORE NORMAL RULES
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public bool IsStartEndDateValid(object startDateObject, string startDateDisplayName, object endDateObject, string endDateDisplayName, string dateFormat, bool isValidWhenEitherDateIsNull, bool isValidWhenDatesAreEqual, bool isValidWhenOneDateIsNullAndTheOtherIsNot, out string errorMessage)
        {
            errorMessage = string.Empty;

            if ((startDateObject == null || startDateObject.ToString().Trim().Length == 0)
                && (endDateObject == null || endDateObject.ToString().Trim().Length == 0))
            {
                errorMessage = "Start or end date is required.";
                return isValidWhenEitherDateIsNull;
            }

            if ((startDateObject == null || startDateObject.ToString().Trim().Length == 0) &&
               (endDateObject != null || endDateObject.ToString().Trim().Length > 0) ||
                (endDateObject == null || endDateObject.ToString().Trim().Length == 0) &&
               (startDateObject != null || startDateObject.ToString().Trim().Length > 0))
            {
                errorMessage = "Start and end date must be populated or both empty";
                return isValidWhenOneDateIsNullAndTheOtherIsNot;
            }

            DateTime startDate;
            DateTime endDate;

            if (string.IsNullOrEmpty(startDateDisplayName))
            {
                startDateDisplayName = "Start Date";
            }

            if (string.IsNullOrEmpty(endDateDisplayName))
            {
                endDateDisplayName = "End Date";
            }

            try
            {
                string startDateString;
                if (dateFormat == null || (startDateString = startDateObject as string) == null)
                {
                    startDate = Convert.ToDateTime(startDateObject);
                }
                else
                {
                    startDate = startDateString.ToDateTime(dateFormat);
                }
            }
            catch (FormatException)
            {
                errorMessage = string.Format("{0} must be a valid date, and in the format '{1}'.", startDateDisplayName, dateFormat ?? DEFAULT_DATE_FORMAT);
                return false;
            }
            catch (InvalidCastException)
            {
                errorMessage = string.Format("{0} must be a valid date, and in the format '{1}'.", startDateDisplayName, dateFormat ?? DEFAULT_DATE_FORMAT);
                return false;
            }

            try
            {
                string endDateString;
                if (dateFormat == null || (endDateString = endDateObject as string) == null)
                {
                    endDate = Convert.ToDateTime(endDateObject);
                }
                else
                {
                    endDate = endDateString.ToDateTime(dateFormat);
                }
            }
            catch (FormatException)
            {
                errorMessage = string.Format("{0} must be a valid date, and in the format '{1}'.", endDateDisplayName, dateFormat ?? DEFAULT_DATE_FORMAT);
                return false;
            }
            catch (InvalidCastException)
            {
                errorMessage = string.Format("{0} must be a valid date, and in the format '{1}'.", endDateDisplayName, dateFormat ?? DEFAULT_DATE_FORMAT);
                return false;
            }

            bool result = true;
            if (startDate > endDate)
            {
                result = false;
                errorMessage = string.Format("{0} has to be before {1}", startDateDisplayName, endDateDisplayName);
            }
            else if (startDate == endDate && !isValidWhenDatesAreEqual)
            {
                result = false;
                errorMessage = string.Format("{0} cannot be the same as {1}.", startDateDisplayName, endDateDisplayName);
            }

            return result;
        }

        /// <summary>
        /// Public for Mock Tests
        /// </summary>
        /// <param name="ntid">The NTID to check</param>
        /// <param name="requiredType">The required type to look for</param>
        /// <param name="isValidWhenNtidIsNull">What to do when a value is null</param>
        /// <param name="requiredUsPerson">Is the user required to be a US person</param>
        /// <param name="requiredLmEmployee">Is the user required to be an LM employee</param>
        /// <returns>True if valid, else false</returns>
        public bool IsUserTypeValid(object ntid, IES.Common.UserType requiredType, bool isValidWhenNtidIsNull, bool requiredUsPerson, bool requiredLmEmployee)
        {
            // If the NTID is null, we cannot continue validating. Return
            if (ntid == null || ntid.ToString().Trim().Length == 0)
            {
                return isValidWhenNtidIsNull;
            }

            string ntidString = ntid.ToString().Trim();
            UserData user = null;

            bool isValid = false;

            // Check for a valid group
            if (requiredType == IES.Common.UserType.Group || requiredType == IES.Common.UserType.NotSet)
            {
                isValid = this.adUtilities.IsValidADGroup(ntidString);
            }

            // Check for a valid user
            if (requiredType == IES.Common.UserType.User || requiredType == IES.Common.UserType.NotSet)
            {
                user = this.adUtilities.GetUserByQualifiedAccount(ntidString, false);

                isValid = user != null ? true : isValid;
            }

            if (user != null && isValid)
            {
                isValid = IsValidLMandUsEmployeeProperties(user, requiredUsPerson, requiredLmEmployee);
            }

            return isValid;
        }

        /// <summary>
        /// Validates User LM employee / US person properties. Return NULL if the user no longer exists
        /// </summary>
        /// <param name="ntid">NTID</param>
        /// <param name="requiredUsPerson">Is the user required to be a US person</param>
        /// <param name="requiredLmEmployee">Is the user required to be an LM employee</param>
        /// <returns>Valid or not</returns>
        public bool? IsValidLMandUsEmployeeProperties(string ntid, bool requiredUsPerson, bool requiredLmEmployee)
        {
            UserData user = this.adUtilities.GetUserByQualifiedAccount(ntid, false);
            return user == null ? (bool?)null : IsValidLMandUsEmployeeProperties(user, requiredUsPerson, requiredLmEmployee);
        }

        /// <summary>
        /// Validates User LM employee / US person properties. Return NULL if the user no longer exists
        /// </summary>
        /// <param name="user">User from AD</param>
        /// <param name="requiredUsPerson">Is the user required to be a US person</param>
        /// <param name="requiredLmEmployee">Is the user required to be an LM employee</param>
        /// <returns>Valid or not</returns>
        private static bool IsValidLMandUsEmployeeProperties(UserData user, bool requiredUsPerson, bool requiredLmEmployee)
        {
            // invalid = (required && !usPerson) => valid = ! (required && !usPerson) => valid = !required || usPerson
            return (!requiredUsPerson || user.IsUsPerson == true)
                    && (!requiredLmEmployee || user.IsSubcontractor == false);
        }

        #endregion General Validation
        #region Number Validation

        /// <summary>
        /// Error message when the item is too long
        /// </summary>
        private const string FAILED_LENGTH = "Maximum length of the field {0} is {2}. Value {1} fails this requirement.";

        /// <summary>
        /// Error message for whole numbers
        /// </summary>
        private const string FAILED_NUMERIC_INT = "The field {0} has to be a whole number";

        /// <summary>
        /// Error message for decimal numbers
        /// </summary>
        private const string FAILED_NUMERIC_DECIMAL = "The field {0} has to be a number";

        /// <summary>
        /// Validation message for failing number too small
        /// </summary>
        private const string FAILED_MIN_ALLOWED_VALUE = "The field {0} has to be greater than {1}";

        /// <summary>
        /// Error message for max allowed value
        /// </summary>
        private const string FAILED_MAX_ALLOWED_VALUE = "The field {0} has to be smaller than {1}";

        /// <summary>
        /// Validate Money Item
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="fieldName">Field Name</param>
        /// <param name="maxLength">Max allowed length</param>
        /// <param name="decimalsAllowed">Decimal or Int</param>
        /// <param name="minAllowedValue">Min Allowed value</param>
        /// <param name="maxAllowedValue">Max allowed value</param>
        /// <returns>A null or a validation message</returns>
        public static ValidationMessage ValidateMoneyItem(string item, string fieldName, int maxLength, bool decimalsAllowed, decimal? minAllowedValue, decimal? maxAllowedValue)
        {
            if (!string.IsNullOrEmpty(item))
            {
                string adjustedItem = item.Replace("$", string.Empty).Replace(" ", string.Empty).Replace("(", "-").Replace(")", string.Empty);

                if (adjustedItem.Length == 1 && adjustedItem.Equals("-"))
                {
                    return new ValidationMessage(fieldName, string.Format(FAILED_NUMERIC_INT, fieldName));
                }

                if (!string.IsNullOrEmpty(adjustedItem))
                {
                    return ValidateLengthAndNumerics(fieldName, maxLength, decimalsAllowed, adjustedItem, minAllowedValue, maxAllowedValue);
                }
            }

            return null;
        }

        /// <summary>
        /// Validate length and numberic portion
        /// </summary>
        /// <param name="fieldName">Field Name</param>
        /// <param name="maxLength">Max allowed length</param>
        /// <param name="decimalsAllowed">Decimal or Int</param>
        /// <param name="adjustedItem">Item to verify</param>
        /// <param name="minAllowedValue">Min Allowed value</param>
        /// <param name="maxAllowedValue">Max allowed value</param>
        /// <returns>A null or a validation message</returns>
        private static ValidationMessage ValidateLengthAndNumerics(string fieldName, int maxLength, bool decimalsAllowed, string adjustedItem, decimal? minAllowedValue, decimal? maxAllowedValue)
        {
            if (adjustedItem.Replace(".", string.Empty).Replace(",", string.Empty).Length > maxLength)
            {
                return new ValidationMessage(fieldName, string.Format(FAILED_LENGTH, fieldName, adjustedItem, maxLength));
            }

            if (decimalsAllowed)
            {
                decimal value;
                if (!decimal.TryParse(adjustedItem.Replace(",", string.Empty), out value))
                {
                    return new ValidationMessage(fieldName, string.Format(FAILED_NUMERIC_DECIMAL, fieldName));
                }
                else
                {
                    if (maxAllowedValue.HasValue && value > maxAllowedValue)
                    {
                        return new ValidationMessage(fieldName, string.Format(FAILED_MAX_ALLOWED_VALUE, fieldName, maxAllowedValue.Value));
                    }

                    if (minAllowedValue.HasValue && value < minAllowedValue)
                    {
                        return new ValidationMessage(fieldName, string.Format(FAILED_MIN_ALLOWED_VALUE, fieldName, minAllowedValue.Value));
                    }
                }
            }
            else
            {
                int value;
                if (!int.TryParse(adjustedItem.Replace(",", string.Empty), out value))
                {
                    long value1;
                    if (!long.TryParse(adjustedItem.Replace(",", string.Empty), out value1))
                    {
                        return new ValidationMessage(fieldName, string.Format(FAILED_NUMERIC_INT, fieldName));
                    }
                    else
                    {
                        if (maxAllowedValue.HasValue && value1 > maxAllowedValue)
                        {
                            return new ValidationMessage(fieldName, string.Format(FAILED_MAX_ALLOWED_VALUE, fieldName, maxAllowedValue.Value));
                        }

                        if (minAllowedValue.HasValue && value1 < minAllowedValue)
                        {
                            return new ValidationMessage(fieldName, string.Format(FAILED_MIN_ALLOWED_VALUE, fieldName, minAllowedValue.Value));
                        }
                    }
                }
                else
                {
                    if (maxAllowedValue.HasValue && value > maxAllowedValue)
                    {
                        return new ValidationMessage(fieldName, string.Format(FAILED_MAX_ALLOWED_VALUE, fieldName, maxAllowedValue.Value));
                    }

                    if (minAllowedValue.HasValue && value < minAllowedValue)
                    {
                        return new ValidationMessage(fieldName, string.Format(FAILED_MIN_ALLOWED_VALUE, fieldName, minAllowedValue.Value));
                    }
                }
            }

            return null;
        }

        #endregion

        #region Number Validation

        /// <summary>
        /// Test if enum value is active
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <param name="value">Enum value</param>
        /// <returns>True if active</returns>
        public bool IsEnumActive<TEnum>(TEnum value)
        {
            return value.IsActive();
        }

        #endregion
    }
}
