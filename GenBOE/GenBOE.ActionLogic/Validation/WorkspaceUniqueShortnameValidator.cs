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
    using GenBOE.DataBridge.DTO;

    public class WorkspaceUniqueShortnameValidator : Validator
    {
        IWorkspaceDTODataLoader workspaceLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkspaceUniqueShortnameValidator(IWorkspaceDTODataLoader workspaceLoader)
        {
            this.workspaceLoader = workspaceLoader;
        }

        /// <summary>
        /// Determines if a value is valid
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

            bool nameAlreadyExists = nameAlreadyExists = this.workspaceLoader.GetAllWsNamesAndTrackingNumberInfo().Any(w => w.Shortname.ToLower() == valueToValidate.ToLower());

            if (nameAlreadyExists)
            {
                response.Add("Shortname not Unique");
            }

            return response;
        }
    }
}
