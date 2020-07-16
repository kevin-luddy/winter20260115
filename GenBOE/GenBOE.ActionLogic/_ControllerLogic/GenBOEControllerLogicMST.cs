// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using GenBOE.DataBridge.DTO;

    /// <summary>
    /// Controller logic class for the base controller
    /// </summary>
    public class GenBOEControllerLogicMST : GenBOEControllerLogic
    {
        private RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader;
        private IOffloadRatesDTOLoader offloadRatesLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        public GenBOEControllerLogicMST(RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader, IOffloadRatesDTOLoader offloadRatesLoader)
        {
            this.zoneTravelRatesFeesLoader = zoneTravelRatesFeesLoader;
            this.offloadRatesLoader = offloadRatesLoader;
        }

        /// <summary>
        /// Determines if either the Escalation Rates or Fees/Cost Rates are out of date.
        /// </summary>
        /// <param name="workspaceId">The id of the workspace to check.</param>
        /// <returns></returns>
        public override bool AreZoneTravelRatesOutOfDate(int workspaceId)
        {
            return this.zoneTravelRatesFeesLoader.AreCurrentEscalationRatesOutOfDate(workspaceId) || this.zoneTravelRatesFeesLoader.AreCurrentFeesOutOfDate(workspaceId);
        }

        /// <summary>
        /// Determines if the Offload Rates are out of date.
        /// </summary>
        /// <param name="workspaceId">The id of the workspace to check.</param>
        /// <returns></returns>
        public override bool AreOffloadRatesOutOfDate(int workspaceId)
        {
            return this.offloadRatesLoader.AreCurrentOffloadRatesOutOfDate(workspaceId);
        }
    }
}
