// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Standard.Exceptions;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for the Burden Pool Controller Logic.
    /// </summary>
    public interface IBurdenPoolControllerLogic : IRdmControllerLogic
    {
        /// <summary>
        /// Validate Burden Pool Grid Data
        /// </summary>
        /// <param name="burdenPools">Collection of Burden Pool rows</param>
        /// <returns>A list of validation errors (if any).</returns>
        ICollection<ValidationMessage> ValidateBurdenPools(Collection<BurdenPoolDetailModelView> burdenPools);
    }
}
