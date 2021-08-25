// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Reporting
{
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Class to generate a ModelView used for BOE Status report display and export
    /// </summary>
    public class BOEStatusReportSpaceSystems : BOEStatusReport
    {
        public BOEStatusReportSpaceSystems(
            ICommonDataMapper inICommonDataMapper,
            IUserDTODataLoader inIUserDTODataLoader,
            IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
            IPermissionsDTODataLoader inPermissionsLoader,
            TravelTripCostCalculation inTravelTripCostCalculator)
            : base(inICommonDataMapper,
                   inIUserDTODataLoader,
                   inVariableSelectBOEtoSumCalculation,
                   inPermissionsLoader,
                   inTravelTripCostCalculator)
        {
            // nothing to do here
        }

        /// <summary>
        /// Return a collection of Space Systems ResourceType IDs to be summed.
        /// </summary>
        /// <returns></returns>
        public override Collection<int> GetResourceTypesToBeSummed()
        {
            return new Collection<int>
            {
                (int)SumVariableResourceType.SSCLMLabor,
                (int)SumVariableResourceType.SSCLOEIWTA,
                (int)SumVariableResourceType.SSCLOESub
            };
        }
    }
}
