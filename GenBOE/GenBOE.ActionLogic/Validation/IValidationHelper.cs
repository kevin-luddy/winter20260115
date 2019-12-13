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
    using IES.Common.Exceptions;
    using GenBOE.Dtos;

    public interface IValidationHelper
    {
        /// <summary>
        /// Validates whether a wbs number is Valid
        /// </summary>
        /// <param name="inStringValue">inWorkspaceID, inWbsID, inWBSNumber</param>
        /// <returns>The response if the number is valid or not. </returns>
        string WBSRenumberValidation(int inWorkspaceID, int inWbsID, string inWBSNumber);

        /// <summary>
        /// Validates whether start date and end date are valid
        /// </summary>
        /// <param name="inStringValue">inStartDate, inEndDate, </param>
        /// <returns>The response if the dates are valid or not. </returns>
        string StartEndDateValidation(DateTime? inStartDate, DateTime? inEndDate);

        /// <summary>
        /// Validates whether start date and end date are valid
        /// </summary>
        /// <param name="inStringValue">inStartDate, inEndDate, </param>
        /// <returns>The response if the dates are valid or not. </returns>
        bool IsOneRangeWithinAnotherValidation(DateTime? inStartDate, DateTime? inEndDate, DateTime? inWSStartDate, DateTime? inWSEndDate);

        /// <summary>
        /// Validates whether all BOE custom fields are valid
        /// </summary>
        /// <param name="inStringValue"> </param>
        /// <returns>The response if the custom fields are valid or not. </returns>
        Collection<ValidationMessage> BOEHeaderCustomFieldValidation(Collection<CustomFieldValueContainer> allSelections, ICollection<CustomFieldDTO> allCustomFields);
    }
}
