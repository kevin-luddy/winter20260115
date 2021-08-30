// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Validation
{
    /// <summary>
    /// Validation Methods Interface
    /// </summary>
    public interface IValidationMethods
    {
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
        bool IsStartEndDateValid(object startDateObject, string startDateDisplayName, object endDateObject, string endDateDisplayName, string dateFormat, bool isValidWhenEitherDateIsNull, bool isValidWhenDatesAreEqual, bool isValidWhenOneDateIsNullAndTheOtherIsNot, out string errorMessage);

        /// <summary>
        /// Test if user type is valid
        /// </summary>
        /// <param name="ntid">The NTID to check</param>
        /// <param name="requiredType">The required type to look for</param>
        /// <param name="isValidWhenNtidIsNull">What to do when a value is null</param>
        /// <param name="requiredUsPerson">Is the user required to be a US person</param>
        /// <param name="requiredLmEmployee">Is the user required to be an LM employee</param>
        /// <returns>True if valid, else false</returns>
        bool IsUserTypeValid(object ntid, IES.Common.UserType requiredType, bool isValidWhenNtidIsNull, bool requiredUsPerson, bool requiredLmEmployee);

        /// <summary>
        /// Test if enum value is active
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <param name="value">Enum value</param>
        /// <returns>True if active</returns>
        bool IsEnumActive<TEnum>(TEnum value);

        /// <summary>
        /// Validates User LM employee / US person properties. Return NULL if the user no longer exists
        /// </summary>
        /// <param name="ntid">NTID</param>
        /// <param name="requiredUsPerson">Is the user required to be a US person</param>
        /// <param name="requiredLmEmployee">Is the user required to be an LM employee</param>
        /// <returns>Valid or not</returns>
        bool? IsValidLMandUsEmployeeProperties(string ntid, bool requiredUsPerson, bool requiredLmEmployee);
    }
}
