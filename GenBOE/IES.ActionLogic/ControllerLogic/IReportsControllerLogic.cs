// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Web;
    using System.Web.Mvc;
    using IES.Common.OfficeUtilities;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for the RDM Reports Controller Logic.
    /// </summary>
    public interface IReportsControllerLogic : IRdmControllerLogic
    {
        /// <summary>
        /// Generate a zip file containing the ProPricer direct and burden rate exports.
        /// </summary>
        /// <param name="zipPathFile">The server path where the zip file will be created</param>
        /// <param name="versionNumber">PPR&amp;D version number</param>
        /// <param name="rates">PPR&amp;D rates</param>
        /// <param name="burdenPools">PPR&amp;D ProPricer burden pools</param>
        /// <param name="burdenElements">PPR&amp;D ProPricer burden elements</param>
        /// <returns>An ActionResult.</returns>
        ActionResult ExportProPricerData(string zipPathFile, string versionNumber,
            ICollection<RateDetailModelView> rates, ICollection<BurdenPoolDetailModelView> burdenPools, 
            ICollection<BurdenElementModelView> burdenElements);

        /// <summary>
        /// Generates the Full PPRD document
        /// </summary>
        /// <param name="id">Revision ID</param>
        /// <param name="serverFileName">Server File Name</param>
        /// <param name="httpResponse">HTTP response object</param>
        /// <param name="portionMarkingRequired">Is Portion Marking Required</param>
        Task GenerateFullPPRD(string id, string serverFileName, HttpResponseBase httpResponse, bool? portionMarkingRequired);

        /// <summary>
        /// Generates a file containing revision data as JSON.
        /// </summary>
        /// <param name="id">Revision Id to export</param>
        /// <param name="jsonFilePath">The server path where the JSON file will be created</param>
        /// <returns>Revision data as JSON.</returns>
        ActionResult ExportRevisionAsJson(string id, string jsonFilePath);
    }
}
