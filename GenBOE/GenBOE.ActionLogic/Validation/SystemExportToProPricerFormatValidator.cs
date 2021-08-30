// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;

    public class SystemExportToProPricerFormatValidator : Validator
    {
        private IProPricerDTODataLoader proPricerLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        public SystemExportToProPricerFormatValidator(IProPricerDTODataLoader proPricerLoader)
        {
            this.proPricerLoader = proPricerLoader;
        }



        public override Collection<String> validation(object value, Collection<Dictionary<String, String>> inData)
        {
            Collection<String> response = new Collection<string>();

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.GetType() != typeof(ProPricerDTO))
            {
                throw new InvalidCastException("value");
            }

            var dataToValidate = (ProPricerDTO)value;

            if (!dataToValidate.WorkspaceID.HasValue && dataToValidate.WorkspaceID > 0)
            {
                throw new ArgumentException("Format DTO must not have WorkspaceID defined.");
            }

            // Validate unique format name
            var duplicateFormatNames = (from f in this.proPricerLoader.GetAllSystemExports()
                                        where f.ExportID != dataToValidate.ExportID &&
                                            f.FormatName.Trim().Equals(dataToValidate.FormatName.Trim(), StringComparison.CurrentCultureIgnoreCase)
                                        select f).Any();

            if (duplicateFormatNames)
            {
                response.Add("The format name must be unique within System scope.");
            }

            return response;
        }
    }

}
