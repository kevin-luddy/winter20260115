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
    using GenBOE.Objects;

    public class BOEMaterialElementExistsValidator : Validator 
    {
        IFullObjectFactory factory;

        public BOEMaterialElementExistsValidator()
        {

        }

        public BOEMaterialElementExistsValidator(IFullObjectFactory factory)
        {
            this.factory = factory;
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

            if (data != null && data.Keys.Contains("BoeID") && data.Keys.Contains("Material"))
            {
                string BoeIDString;
                string Material;
                data.TryGetValue("BoeID", out BoeIDString);
                data.TryGetValue("Material", out Material);

                int BoeID = Convert.ToInt32(BoeIDString);
                FullBoe boe = this.factory.CreateFullBoe(BoeID);

                if (BoeID > 0 && Material == "false")
                {
                    if (boe.ContainsMaterialElements)
                    {
                        response.Add("This BOE contains materials and cannot be changed from a Material BOE");
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
