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
    using GenBOE.DataBridge.DTO;
    using IES.Common;
    using GenBOE.Objects;

    public class CLINValidator : Validator
    {
        private IFullObjectFactory factory;

        /// <summary>
        /// Constructor
        /// </summary>
        public CLINValidator(IFullObjectFactory factory)
        {
            this.factory = factory;
        }

        public override Collection<String> validation(object value, Collection<Dictionary<String, String>> inData)
        {
            Collection<String> response = new Collection<string>();
            ClinDTODataLoader _clinDTODataLoader = new ClinDTODataLoader();

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.GetType() != typeof(FullClin))
            {
                throw new InvalidCastException("value");
            }

            FullClin dataToValidate = value as FullClin;

            FullWorkspace workspace = this.factory.CreateFullWorkspace(dataToValidate.WorkspaceID);

            // Get all CLINs in the workspace
            var existingCLINs = workspace.Clins;

            // Validate unique CLIN number
            var duplicateCLINNumbers = (from c in existingCLINs
                                        where c.Id != dataToValidate.Id &&
                                              c.ClinPaddedNumber.Equals(_clinDTODataLoader.PadClinNumber(dataToValidate.ClinNumber))
                                        select c).Any();

            if (duplicateCLINNumbers)
            {
                response.Add("A CLIN Number must be unique.");
            }

            // Validate CLIN Start and End Dates against Contract Start and End Dates
            if (dataToValidate.StartDate.HasValue && GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(dataToValidate.StartDate), DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision(workspace.ContractStartDate, DateTimePrecision.Month))
            {
                response.Add("The Start Date must be within the Contract Start Date.");
            }

            if (dataToValidate.EndDate.HasValue && GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(dataToValidate.EndDate), DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision(workspace.ContractEndDate, DateTimePrecision.Month))
            {
                response.Add("The End Date must be within the Contract End Date.");
            }

            // dates aren't required but if there is a start date then there must be an end date
            if (dataToValidate.StartDate.HasValue && !dataToValidate.EndDate.HasValue)
            {
                response.Add("End Date is required.");
            }

            // dates aren't required but if there is an end date then there must be a start date
            if (!dataToValidate.StartDate.HasValue && dataToValidate.EndDate.HasValue)
            {
                response.Add("Start Date is required.");
            }

            return response;
        }
    }

}
