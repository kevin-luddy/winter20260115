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
    using System.Linq;
    using IES.Common;
    using IES.Common.Exceptions;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;

    public class ValidationHelper : IValidationHelper
    {
        ICustomFieldValueDTODataLoader _CustomFieldValueDTODataLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        public ValidationHelper(ICustomFieldValueDTODataLoader inCustomFieldValueDTODataLoader)
        {
            this._CustomFieldValueDTODataLoader = inCustomFieldValueDTODataLoader;
        }


        /// <summary>
        /// Validates whether a wbs number is Valid
        /// </summary>
        /// <param name="inStringValue">inWorkspaceID, inWbsID, inWBSNumber</param>
        /// <returns>The response if the number is valid or not. </returns>
        public string WBSRenumberValidation(int inWorkspaceID, int inWbsID, string inWBSNumber)
        {
            // This method currently uses the Validation Factory to do it's logic because there was no reason to re-write the current validation.
            //If doing 2.0 Generation Validation do not use the validation factory logic.
            // Validate WBS Unique Number and no circular references
            Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>>();

                validationData.Add(new Dictionary<string, string>() {
                        {"WorkspaceID", inWorkspaceID.ToString() },
                        {"WbsID", inWbsID.ToString() }
                    });

                var validatorResponse = ValidationFactory.Instance.getValidator(ValidationType.WBSRenumber).validation(inWBSNumber, validationData);

                if (validatorResponse.Count > 0)
                {
                    return String.Join("\n", validatorResponse);
                }
                else
                {
                    return null;
                }
        }

        /// <summary>
        /// Validates whether start date and end date are valid
        /// </summary>
        /// <param name="inStringValue">inStartDate, inEndDate, </param>
        /// <returns>The response if the dates are valid or not. </returns>
        public string StartEndDateValidation(DateTime? inStartDate, DateTime? inEndDate)
        {

            if (inStartDate.HasValue && inEndDate.HasValue)
            {

                if (inEndDate < inStartDate)
                {
                    return "Start Date must be after the End Date";
                }

                if (inStartDate > inEndDate)
                {
                    return "End Date must be after the Start Date";
                }
            }

            return null;
        }

        /// <summary>
        /// Validates whether start date and end date are valid
        /// </summary>
        /// <param name="inStringValue">inStartDate, inEndDate, </param>
        /// <returns>The response if the dates are valid or not. </returns>
        public bool IsOneRangeWithinAnotherValidation(DateTime? inStartDate, DateTime? inEndDate, DateTime? inWSStartDate, DateTime? inWSEndDate)
        {
            if (inStartDate.HasValue && inWSStartDate.HasValue && inEndDate.HasValue && inWSEndDate.HasValue)
            {
                if ((inStartDate < inWSStartDate) || (inWSEndDate < inEndDate))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates whether all BOE custom fields are valid
        /// </summary>
        /// <param name="inStringValue"> </param>
        /// <returns>The response if the custom fields are valid or not. </returns>
        public Collection<ValidationMessage> BOEHeaderCustomFieldValidation(Collection<CustomFieldValueContainer> allSelections, ICollection<CustomFieldDTO> allCustomFields)
        {
            Collection<ValidationMessage> allErrors = new Collection<ValidationMessage>();

            if (allCustomFields != null && allCustomFields.Any())
            {
                ICollection<CustomFieldValueDTO> allCustomFieldValues = this._CustomFieldValueDTODataLoader.GetCustomFieldValueDTOsByCustomFieldIds(allCustomFields.Select(i => i.Id).ToCollection<int>());

                foreach (CustomFieldDTO customField in allCustomFields)
                {
                    if (customField.CustomFieldRequired)
                    {
                        if (!customField.IsOpenEnded) 
                        {
                            ICollection<CustomFieldValueDTO> allCustomFieldValueDTOs = allCustomFieldValues.Where(i => i.CustomFieldID == customField.Id).ToCollection<CustomFieldValueDTO>();
                            ICollection<int> allValueIDs = (from x in allCustomFieldValueDTOs
                                                            select x.CustomFieldValueID).ToList();

                            bool requiredNotFound = true;

                            foreach (int valueID in allValueIDs)
                            {
                                if (allSelections != null)
                                {
                                    foreach (CustomFieldValueContainer container in allSelections)
                                    {
                                        if (container.CustomFieldValueID == valueID && container.Updateable != UpdateType.Deleted)
                                        {
                                            requiredNotFound = false;
                                        }
                                    }
                                }

                            }

                            if (requiredNotFound)
                            {
                                ValidationMessage tempMessage = new ValidationMessage();
                                tempMessage.FieldName = "CustomField";
                                tempMessage.ValidationIssue = ("Custom Field " + customField.CustomFieldName + " is required.");
                                allErrors.Add(tempMessage);
                            } 
                        }
                        else
                        {
                            CustomFieldValueContainer cfValueContainer = allSelections.FirstOrDefault(x => x.CustomFieldID == customField.Id);
                            if(string.IsNullOrEmpty(cfValueContainer?.OpenEndedValue))
                            {
                                ValidationMessage tempMessage = new ValidationMessage();
                                tempMessage.FieldName = "CustomField";
                                tempMessage.ValidationIssue = ("Custom Field " + customField.CustomFieldName + " is required.");
                                allErrors.Add(tempMessage);
                            }
                        }
                    }
                }
            }
            if (allErrors.Any())
            {
                return allErrors;
            }
            else
            {
                return null;
            }

        }

    }

}
