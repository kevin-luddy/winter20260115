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
    using GenBOE.Objects;

    public class BoeLaborCostElementExistsValidator : Validator 
    {
        IFullObjectFactory factory;

        public BoeLaborCostElementExistsValidator(IFullObjectFactory factory)
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

                int boeID = Convert.ToInt32(BoeIDString);
                

                // only check if the BOE ID is valid and Material has been checked
                if (boeID > 0 && Material == "on")
                {
                    FullBoe boe = this.factory.CreateFullBoe(boeID);

                    if (boe.ContainsLaborCostElements)
                    {
                        response.Add("This BOE contains non-material data and cannot be changed to a Material BOE.");
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
