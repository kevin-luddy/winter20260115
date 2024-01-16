// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using IES.Common.Core.Exceptions;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for the Admin Controller Logic.
    /// </summary>
    public interface IAdminControllerLogic : IRdmControllerLogic
    {
        /// <summary>
        /// Determines whether the provided Cobra mapping details are valid.
        /// </summary>
        /// <param name="collection">The collection of Cobra Details.</param>
        /// <returns>A list of validation errors (if any).</returns>
        ICollection<ValidationMessage> ValidateCobraDetailModelViews(ICollection<CobraDetailModelView> collection);

        /// <summary>
        /// Determines whether the provided Cobra Year Configurations are valid.
        /// </summary>
        /// <param name="collection">The collection of Cobra Year Configurations.</param>
        /// <returns>A list of validation errors (if any).</returns>
        ICollection<ValidationMessage> ValidateCobraYearConfigurations(ICollection<CobraYearGridModelView> collection);

        /// <summary>
        /// Determines whether the provided start and end year values are valid.
        /// </summary>
        /// <param name="startYear">Start Year</param>
        /// <param name="endYear">End Year</param>
        /// <returns>A list of validation errors (if any).</returns>
        ICollection<ValidationMessage> ValidateRateYearConfiguration(string startYear, string endYear);

        /// <summary>
        /// Gets the rate code replication model view.
        /// </summary>
        /// <returns>Model View.</returns>
        RateCodeReplicationModelView GetRateCodeReplication();

        /// <summary>
        /// Validates the rate code replications.
        /// </summary>
        /// <param name="rateCodes">The rate codes.</param>
        /// <returns>A list of Validation Errors (if any).</returns>
        ICollection<ValidationMessage> ValidateRateCodeReplication(ICollection<RateCodeModelView> rateCodes);

        /// <summary>
        /// Saves the rate code replications.
        /// </summary>
        /// <param name="rateCodes">The rate code replication to save.</param>
        void SaveRateCodeReplication(ICollection<RateCodeModelView> rateCodes);

        /// <summary>
        /// Clear the Revision cache.
        /// </summary>
        void ClearRevisionCache();
    }
}
