// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using IES.Common.Exceptions;

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// Interface for view models that support pre-save validation
    /// </summary>
    public interface IValidateForSave
    {
        /// <summary>
        /// Determine if there are any validation errors associated with saving a view model
        /// </summary>
        /// <param name="fieldPrefix">Field name prefix</param>
        /// <param name="allowResClinWbs">True if BOE is marked as Multi and therefore allows a WBS/CLIN at the resource level.</param>
        /// <param name="isUsingEP">True if Workspace is using Equivalent Persons; False if using Hours.</param>
        /// <returns>Collection of validation errors</returns>
        IList<ValidationMessage> ValidateForSave(string fieldPrefix, bool allowResClinWbs, bool isUsingEP);
    }
}
