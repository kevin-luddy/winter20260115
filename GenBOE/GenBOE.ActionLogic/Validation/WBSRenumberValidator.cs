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
    using GenBOE.ActionLogic.WBS;
    using GenBOE.Objects;

    public class WBSRenumberValidator : Validator
    {
        private VariableCircularReferenceChecker _VariableCircularReferenceChecker;
        private IFullObjectFactory factory;

        /// <summary>
        /// Constructor
        /// </summary>
        public WBSRenumberValidator(VariableCircularReferenceChecker inVariableCircularReferenceChecker, IFullObjectFactory factory)
        {
            this._VariableCircularReferenceChecker = inVariableCircularReferenceChecker;
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

            var valueToValidate = value as string;

            Dictionary<String, String> data = inData != null && inData.Count > 0 ? inData.First<Dictionary<String, String>>() : null;

            if (data == null)
            {
                response.Add("Invalid data");
            }
            else if (data.Keys.Contains("WbsID") && data.Keys.Contains("WorkspaceID"))
            {
                string wbsID;
                if (data.TryGetValue("WbsID", out wbsID))
                {
                    var wbsIDInt = Convert.ToInt32(wbsID);

                    if (wbsIDInt > 0)
                    {
                        var cache = new VariableCircularReferenceCheckerCache();

                        FullWbs wbs = this.factory.CreateFullWbs(wbsIDInt);
                        FullWorkspace ws = wbs.Workspace;

                        bool createsCircularReference = this._VariableCircularReferenceChecker.WBSRenumberCreatesCircularReference(cache, wbs, valueToValidate, null, ws);

                        if (createsCircularReference)
                        {
                            response.Add("The new WBS # would create a circular reference.");
                        }

                        string workspaceIDString;
                        FullWbs currentWbs = this.factory.CreateFullWbs(wbsIDInt);

                        if (currentWbs.inUse && data.TryGetValue("WorkspaceID", out workspaceIDString))
                        {
                            int workspaceID;
                            if (Int32.TryParse(workspaceIDString, out workspaceID))
                            {
                                // Create a full wbs with just the wbs number and ws id so we can get parents and children without bringing in the loader
                                FullWbs updatedWbs = new FullWbs() { WbsNumber = valueToValidate, WorkspaceID = workspaceID };

                                // Check for parents with the new value
                                ICollection<FullWbs> newParentWbs = updatedWbs.AllParentWbs;
                                FullWbs inUseParent = newParentWbs.FirstOrDefault(x => x.inUse && !currentWbs.AllParentWbs.Any(y => y.Id == x.Id) && currentWbs.Id != x.Id);
                                if (inUseParent != null)
                                {
                                    response.Add("WBS # cannot be changed to " + valueToValidate + " because a BOE currently exists for " + inUseParent.WbsNumber
                                        + " which would now be a parent of " + valueToValidate + ". All BOEs for one WBS will need to be first moved to the other before making this change.");
                                }

                                // Check for children with the new value
                                ICollection<FullWbs> newChildWbs = updatedWbs.AllChildWbs;
                                FullWbs inUseChild = newChildWbs.FirstOrDefault(x => x.inUse && !currentWbs.AllChildWbs.Any(y => y.Id == x.Id) && currentWbs.Id != x.Id);
                                if (inUseChild != null)
                                {
                                    response.Add("WBS # cannot be changed to " + valueToValidate + " because a BOE currently exists for " + inUseChild.WbsNumber
                                        + " which would now be a child of " + valueToValidate + ". All BOEs for one WBS will need to be first moved to the other before making this change.");
                                }
                            }
                        }
                    }
                }
            }

            var wbsUniqueNumberValidatorMessages = ValidationFactory.Instance.getValidator(ValidationType.WBSUniqueNumber).validation(value, inData);

            return new Collection<string>(wbsUniqueNumberValidatorMessages.Union(response).ToArray());
        }
    }
}
