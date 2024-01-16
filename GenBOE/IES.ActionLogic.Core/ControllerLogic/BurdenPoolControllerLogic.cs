// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common.Core;
    using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Mediator;

    /// <summary>
    /// Logic for the BurdenPool Controller.
    /// </summary>
    public class BurdenPoolControllerLogic : RdmControllerLogic, IBurdenPoolControllerLogic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BurdenPoolControllerLogic"/> class.
        /// </summary>
        /// <param name="revisionMediator">Revision Mediator</param>
        /// <param name="areaLockingLoader">Area Locking Loader</param>
        /// <param name="adUtils">AD Utilities</param>
        /// <param name="securityInfo">Security Information</param>
        public BurdenPoolControllerLogic(IAreaLockingLoader areaLockingLoader, IRevisionMediator revisionMediator, 
            IActiveDirectoryService adUtils, ISecurityInformation securityInfo)
            : base(areaLockingLoader, revisionMediator, adUtils, securityInfo)
        {
        }

        /// <summary>
        /// Validate Burden Pool Grid Data
        /// </summary>
        /// <param name="burdenPools">Collection of Burden Pool rows</param>
        /// <returns>A list of validation errors (if any).</returns>
        public ICollection<ValidationMessage> ValidateBurdenPools(Collection<BurdenPoolDetailModelView> burdenPools)
        {
            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();

            if (burdenPools.Any(c => !c.IsDeleted && string.IsNullOrWhiteSpace(c.BurdenPool)))
            {
                validationErrors.Add(new ValidationMessage("One or more rows are missing the required Burden Pool field."));
            }

            List<string> burdenPoolsMissingDescription = burdenPools.Where(x => !x.IsDeleted && string.IsNullOrWhiteSpace(x.Description)).Select(x => x.BurdenPool).ToList();

            if (burdenPoolsMissingDescription.Any())
            {
                validationErrors.Add(new ValidationMessage($"The Description field is required for the following Burden Pool(s): {string.Join(", ", burdenPoolsMissingDescription)}."));
            }

            return validationErrors;
        }
    }
}
