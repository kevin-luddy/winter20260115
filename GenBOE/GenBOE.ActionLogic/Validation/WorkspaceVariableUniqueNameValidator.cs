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
    using IES.Common.Exceptions;
    using GenBOE.Dtos;
    using GenBOE.Objects;

    public class WorkspaceVariableUniqueNameValidator 
    {
        /// <summary>
        /// Validate a workspace variable name for uniqueness.  It cannot be the same as another
        /// workspace variable name or an ordinary variable name.
        /// </summary>
        /// <param name="variableNameToCheck">Variable name to check.</param>
        /// <param name="workspaceVariables">All workspace variables.</param>
        /// <param name="workspace">Full workspace object.</param>
        /// <returns>List of validation error messages.</returns>
        public virtual Collection<ValidationMessage> Validate(string variableNameToCheck, ICollection<WorkspaceVariableDTO> workspaceVariables, FullWorkspace workspace)
        {
            Collection<ValidationMessage> response = new Collection<ValidationMessage>();

            if (variableNameToCheck == null)
            {
                throw new ArgumentNullException(nameof(variableNameToCheck));
            }

            string valueToValidate = variableNameToCheck.Trim().ToLower();

            bool nameUsedByOrdVariable = false;
            bool nameUsedByWSVariable = false;
            
            // Get all ordinary variables in the workspace
            var ordinaryVariables = from boe in workspace.Boes
                                    from taskElement in workspace.TaskElements.Where(i => i.BoeID == boe.Id)
                                    from variable in taskElement.OrdinaryVariables
                                    select variable;

            nameUsedByWSVariable = workspaceVariables.Count(x => x.WorkspaceVariableName.Trim().ToLower() == valueToValidate) > 1;

            // Check to see if the name is in use by an ordinary variable in the workspace
            nameUsedByOrdVariable = (from ordinaryVariable in ordinaryVariables
                                        where valueToValidate.ToLower() == ordinaryVariable.OrdinaryVariableName.Trim().ToLower()
                                        select ordinaryVariable).Any();



            if (nameUsedByOrdVariable)
            {
                response.Add(new ValidationMessage(String.Format(
                        "Workspace variable name '{0}' is already in use by a variable inside of a Task Element in this workspace.",
                        variableNameToCheck
                        )));
            }
            else if (nameUsedByWSVariable)
            {
                response.Add(new ValidationMessage(String.Format(
                        "Workspace variable name '{0}' is not unique.",
                        variableNameToCheck
                        )));
            }
            
            return response;
        }

        /// <summary>
        /// Gets a list of distinct validation error messages.
        /// </summary>
        /// <param name="ValidationMessages">Validation error messages.</param>
        /// <returns>List of distinct validation error messages.</returns>
        public ICollection<ValidationMessage> CreateValidationErrorResponse(ICollection<ValidationMessage> ValidationMessages)
        {
            if (ValidationMessages == null)
            {
                throw new ArgumentNullException(nameof(ValidationMessages));
            }

            ICollection<ValidationMessage> distinctMessages = new Collection<ValidationMessage>();

            foreach (ValidationMessage message in ValidationMessages)
            {
                if (distinctMessages.FirstOrDefault(i => i.ValidationIssue == message.ValidationIssue) == null)
                {
                    distinctMessages.Add(message);
                }
            }
            return distinctMessages;
        }
    }
}
