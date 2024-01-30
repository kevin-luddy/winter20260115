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
    using GenBOE.Dtos;
    using GenBOE.Objects;

    public class ExportToProPricerFormatValidator : Validator
    {
        private IFullObjectFactory _factory;

        /// <summary>
        /// Constructor
        /// </summary>
        public ExportToProPricerFormatValidator(IFullObjectFactory factory)
        {
            this._factory = factory;
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

			ProPricerDTO dataToValidate = (ProPricerDTO)value;

            if (!dataToValidate.WorkspaceID.HasValue || dataToValidate.WorkspaceID < 0)
            {
                throw new ArgumentException("Format DTO must have WorkspaceID defined");
            }

            FullWorkspace workspace = this._factory.CreateFullWorkspace(dataToValidate.WorkspaceID.Value);

			// Validate unique format name
			bool duplicateFormatNames = (from f in workspace.ProPricerExports
                                        where f.ExportID != dataToValidate.ExportID &&
                                              f.Scope == dataToValidate.Scope &&
                                              f.FormatName.Trim().Equals(dataToValidate.FormatName.Trim(), StringComparison.CurrentCultureIgnoreCase)
                                        select f).Any();

            if (duplicateFormatNames)
            {
                response.Add("The format name must be unique within a Workspace scope.");
            }

            return response;
        }
    }

}
