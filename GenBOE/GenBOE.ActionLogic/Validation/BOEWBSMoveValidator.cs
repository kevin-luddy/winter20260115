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

    public class BOEWBSMoveValidator : Validator
    {
        private VariableCircularReferenceChecker _VariableCircularReferenceChecker;
        IFullObjectFactory factory;

        /// <summary>
        /// Constructor
        /// </summary>
        public BOEWBSMoveValidator(VariableCircularReferenceChecker inVariableCircularReferenceChecker, IFullObjectFactory factory)
        {
            this._VariableCircularReferenceChecker = inVariableCircularReferenceChecker;
            this.factory = factory;
        }

        /// <summary>
        /// Validates that the association between the boe and the wbs does not create a circular reference. If it does
        /// an error message is returned. If it doesn't an empty collection is returned.
        /// </summary>
        /// <param name="value">string representing the WBS id</param>
        /// <param name="inData">dictionary where the key is "BoeID" and the value is the string representing the boe id </param>
        /// <returns>one or more error messages if the association fails validation</returns>
        [Obsolete("Use the more efficient version of 'validation'.")]
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

            var valueToValidate = Convert.ToInt32(value);

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
                    var boeIDInt = Convert.ToInt32(boeID);

                    if (boeIDInt > 0 && valueToValidate > 0)
                    {
                        var cache = new VariableCircularReferenceCheckerCache();

                        FullBoe boe = this.factory.CreateFullBoe(boeIDInt);
                        FullWorkspace ws = boe.Workspace;
                        FullWbs wbs = this.factory.CreateFullWbs(valueToValidate);

                        bool createsCircularReference = this._VariableCircularReferenceChecker.BOEWBSMoveCreatesCircularReference(cache, boe, wbs, null, ws);

                        if (createsCircularReference)
                        {
                            response.Add("The selected WBS # would create a circular reference.");
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

            if (value.GetType() != typeof(FullWbs))
            {
                throw new InvalidCastException("value");
            }

            FullWbs valueToValidate = value as FullWbs;

            Dictionary<String, Object> data = inData != null && inData.Any() ? inData.First<Dictionary<String, Object>>() : null;

            if (data == null)
            {
                response.Add("Invalid data");
            }
            else if (data.Keys.Contains("Boe"))
            {
                FullBoe boe = null;
                FullWorkspace workspace = null;

                Object boeObject; 
                if (!data.TryGetValue("Boe", out boeObject) || boeObject == null || (boe = boeObject as FullBoe) == null)
                {
                    throw new ArgumentException("data['BOE']");
                }

                // now grab the workspace
                Object workspaceObject = null;
                if (!data.TryGetValue("Workspace", out workspaceObject) || workspaceObject == null || (workspace = workspaceObject as FullWorkspace) == null)
                {
                    throw new ArgumentException("data['Workspace']");
                }

                var cache = new VariableCircularReferenceCheckerCache();

                FullWbs wbs = this.factory.CreateFullWbs(valueToValidate);

                bool createsCircularReference = this._VariableCircularReferenceChecker.BOEWBSMoveCreatesCircularReference(cache, boe, wbs, null, workspace);

                if (createsCircularReference)
                {
                    response.Add("The selected WBS # would create a circular reference.");
                }
            }

            return response;
        }
    }
}
