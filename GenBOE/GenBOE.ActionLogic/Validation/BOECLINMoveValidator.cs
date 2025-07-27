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

    public class BOECLINMoveValidator : Validator
    {
        private readonly VariableCircularReferenceChecker _VariableCircularReferenceChecker;
        private readonly IFullObjectFactory factory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inVariableCircularReferenceChecker">CR Checker</param>
        public BOECLINMoveValidator(VariableCircularReferenceChecker inVariableCircularReferenceChecker, IFullObjectFactory factory)
        {
            this._VariableCircularReferenceChecker = inVariableCircularReferenceChecker;
            this.factory = factory;
        }

        /// <summary>
        /// Validates that associating the CLIN with this BOE does not introduce a circular reference.
        /// </summary>
        /// <param name="value">the CLIN id as a string</param>
        /// <param name="inData">dictionary containing one entry: key = "BoeID", value = the boe id as a string</param>
        /// <returns>Collection of one error message if validation fails. Otherwise, an empty collection is returned.</returns>
        [Obsolete("Use the more efficient version of 'validation'")]
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

			int valueToValidate = Convert.ToInt32(value);

            Dictionary<String, String> data = inData != null && inData.Count > 0 ? inData.First<Dictionary<String, String>>() : null;

            if (data == null)
            {
                response.Add("Invalid data");
            }
            else if (data.Keys.Contains("BoeID"))
            {
                string boeID;
                if (data.TryGetValue("BoeID", out boeID))
                {
					int boeIDInt = Convert.ToInt32(boeID);

                    if (boeIDInt > 0 && valueToValidate > 0)
                    {
						VariableCircularReferenceCheckerCache cache = new VariableCircularReferenceCheckerCache();

                        FullBoe boeObject = this.factory.CreateFullBoe(boeIDInt);
                        FullClin clin = this.factory.CreateFullClin(valueToValidate);
                        FullWorkspace ws = boeObject.Workspace;

                        bool createsCircularReference = this._VariableCircularReferenceChecker.BOECLINMoveCreatesCircularReference(ws, cache, boeObject, clin, null);

                        if (createsCircularReference)
                        {
                            response.Add("The selected CLIN # would create a circular reference.");
                        }
                    }
                }
            }

            return response;
        }

  
        /// <summary>
        /// Validates that associating the CLIN with this BOE does not introduce a circular reference.
        /// </summary>
        /// <param name="value">the CLIN </param>
        /// <param name="inData">dictionary containing one entry: key = "BoeID", value = the boe id as a string</param>
        /// <returns>Collection of one error message if validation fails. Otherwise, an empty collection is returned.</returns>
        public override Collection<string> validation(object value, Collection<Dictionary<string, object>> inData)
        {
            Collection<string> response = new Collection<string>();

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.GetType() != typeof(FullClin))
            {
                throw new InvalidCastException("value");
            }

            FullClin valueToValidate = value as FullClin;

            Dictionary<String, Object> data = inData != null && inData.Count > 0 ? inData.FirstOrDefault<Dictionary<String, Object>>() : null;

            if (data == null)
            {
                response.Add("Invalid data");
            }
            else if (data.Keys.Contains("Boe"))
            {
				FullBoe boe;
				object boeObject;
				if (!data.TryGetValue("Boe", out boeObject) || boeObject == null || (boe = boeObject as FullBoe) == null)
				{
					throw new ArgumentException("data['BOE']");
				}

				FullWorkspace workspace;

				// now grab the workspace
				object workspaceObject;
				if (!data.TryGetValue("Workspace", out workspaceObject) || workspaceObject == null || (workspace = workspaceObject as FullWorkspace) == null)
				{
					throw new ArgumentException("data['Workspace']");
				}


				// now find the clin with the id
				VariableCircularReferenceCheckerCache cache = new VariableCircularReferenceCheckerCache();

                bool createsCircularReference = this._VariableCircularReferenceChecker.BOECLINMoveCreatesCircularReference(workspace, cache, boe, valueToValidate, null);

                if (createsCircularReference)
                {
                        response.Add("The selected CLIN # would create a circular reference.");
                }
            }

            return response;
        }
    }
}
