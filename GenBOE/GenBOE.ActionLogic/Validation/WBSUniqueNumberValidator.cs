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

    public class WBSUniqueNumberValidator : Validator
    {
        private IWbsDTODataLoader wbsLoader;


        /// <summary>
        /// Constructor
        /// </summary> 
        public WBSUniqueNumberValidator(IWbsDTODataLoader wbsLoader)
        {
            this.wbsLoader = wbsLoader;
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

            if (data != null && data.Keys.Contains("WorkspaceID") && !data.Keys.Contains("WbsID"))
            {
                string workspaceID;
                data.TryGetValue("WorkspaceID", out workspaceID);

                bool isUnique = this.wbsLoader.IsWbsNumberUnique(valueToValidate, Convert.ToInt32(workspaceID), null);
                
                if (!isUnique)
                {
                    response.Add("The WBS # must be unique.");
                }
            }
            else if (data != null && data.Keys.Contains("WorkspaceID") && data.Keys.Contains("WbsID"))
            {
                string workspaceID;
                data.TryGetValue("WorkspaceID", out workspaceID);

                string wbsID;
                data.TryGetValue("WbsID", out wbsID);

                bool isUnique = this.wbsLoader.IsWbsNumberUnique(valueToValidate, Convert.ToInt32(workspaceID), Convert.ToInt32(wbsID));

                if (!isUnique)
                {
                    response.Add("The WBS # must be unique.");
                }
            }
            else
            {
                response.Add("Invalid data");
            }

            return response;
        }
    }
}
