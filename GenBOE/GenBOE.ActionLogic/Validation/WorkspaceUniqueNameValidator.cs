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

    public class WorkspaceUniqueNameValidator : Validator
    {
        IWorkspaceDTODataLoader workspaceLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkspaceUniqueNameValidator(IWorkspaceDTODataLoader workspaceLoader)
        {
            this.workspaceLoader = workspaceLoader;
        }

        public override Collection<string> validation(object value, Collection<Dictionary<string, string>> inData)
        {
            Collection<string> response = new Collection<string>();

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.GetType() != typeof(string))
            {
                throw new InvalidCastException("value");
            }

            string valueToValidate = (string)value;

            Dictionary<String, String> data = inData != null && inData.Count > 0 ? inData.First<Dictionary<String, String>>() : null;

            bool nameAlreadyExists = false;
            if (data != null && data.Keys.Contains("WorkspaceID"))
            {
                nameAlreadyExists = this.workspaceLoader.GetAllWsNamesAndTrackingNumberInfo(
                    ).Any(w => w.WorkspaceName.ToLower() == valueToValidate.ToLower()
                        && w.Id != Convert.ToInt32(data["WorkspaceID"]));
            }
            else
            {
                nameAlreadyExists = this.workspaceLoader.GetAllWsNamesAndTrackingNumberInfo().Any(w => w.WorkspaceName.ToLower() == valueToValidate.ToLower());
            }

            if (nameAlreadyExists)
            {
                response.Add("Workspace name is not unique");
            }
            
            return response;
        }
    }

}
