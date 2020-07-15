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

    public class BOEMaterialExistsForWBSValidator : Validator 
    {
        public BOEMaterialExistsForWBSValidator()
        {

        }
        private IWbsDTODataLoader wbsLoader;
        public BOEMaterialExistsForWBSValidator(IWbsDTODataLoader wbsLoader)
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

            Dictionary<String, String> data = inData != null && inData.Count > 0 ? inData.First<Dictionary<String, String>>() : null;

            if (data != null && data.Keys.Contains("BoeID") && 
                 (data.Keys.Contains("WbsID") || data.Keys.Contains("ClinID"))
                   && data.Keys.Contains("Material"))
            {
                string boeID;
                string wbsID;
                string clinID;
                string Material;
                data.TryGetValue("BoeID", out boeID);
                data.TryGetValue("WbsID", out wbsID);
                data.TryGetValue("ClinID", out clinID);
                data.TryGetValue("Material", out Material);

                /*
                if (clinID == null)
                {

                }
                 * */

                if (Material == "on")
                {
                    if (Convert.ToInt32(wbsID) == -1)
                    {
                        response.Add("A WBS is required to create a Material BOE.");
                    }
                    else
                    {
                        bool isAlreadyTied = this.wbsLoader.IsWbsTiedToMaterialBoeAndClin(Convert.ToInt32(boeID), Convert.ToInt32(wbsID), Convert.ToInt32(clinID));
                        bool wbsContainsMaterial = this.wbsLoader.IsWbsTiedToMaterialBoe(Convert.ToInt32(boeID), Convert.ToInt32(wbsID));

                        if (isAlreadyTied)
                        {
                            response.Add("A Material BOE already exists for this WBS and CLIN.");
                        }
                        else if (wbsContainsMaterial)
                        {
                            response.Add("A Material BOE already exists for this WBS.");
                        }
                    }
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
