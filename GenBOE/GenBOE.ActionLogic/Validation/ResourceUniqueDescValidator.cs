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

    public class ResourceUniqueDescValidator : Validator
    {
        private IResourceDTODataLoader _ResourceLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        public ResourceUniqueDescValidator(IResourceDTODataLoader inResourceLoader)
        {
            this._ResourceLoader = inResourceLoader;
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

            if (data != null && data.Keys.Contains("ResourceListID") && !data.Keys.Contains("ResourceID"))
            {
                string resourceListID;
                data.TryGetValue("ResourceListID", out resourceListID);

                bool isUnique = this._ResourceLoader.IsResourceDescriptionUnique(valueToValidate, Convert.ToInt32(resourceListID), null);

                if (!isUnique)
                {
                    response.Add("The Resource Description must be unique");
                }
            }
            else if (data != null && data.Keys.Contains("ResourceListID") && data.Keys.Contains("ResourceID"))
            {
                string resourceListID;
                data.TryGetValue("ResourceListID", out resourceListID);

                string resourceID;
                data.TryGetValue("ResourceID", out resourceID);

                bool isUnique = this._ResourceLoader.IsResourceDescriptionUnique(valueToValidate, Convert.ToInt32(resourceListID), Convert.ToInt32(resourceID));

                if (!isUnique)
                {
                    response.Add("The Resource Description must be unique");
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
