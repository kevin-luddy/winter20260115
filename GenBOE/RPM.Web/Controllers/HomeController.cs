// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RPM.Web.Controllers
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Web.Mvc;
    using IES.Common;
    using RPM.DataBridge.DataLoaders;
    using RPM.DataBridge.Models;

    /// <summary>
    /// The Homepage Controller.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class HomeController : IESController
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly Logger log = new Logger(typeof(HomeController));

        #region Views 

        /// <summary>
        /// Returns the view for the Index page.
        /// </summary>
        /// <returns>The view for the Index page.</returns>
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Returns the view for the Proposals page.
        /// </summary>
        /// <returns>The view for the Proposals page.</returns>
        public ActionResult Proposals()
        {
            return this.View();
        }

        /// <summary>
        /// Returns the view for the Opportunities page.
        /// </summary>
        /// <returns>The view for the Opportunities page.</returns>
        public ActionResult Opportunities()
        {
            return this.View();
        }

        /// <summary>
        /// Returns the view for the ProposalMetrics page.
        /// </summary>
        /// <returns>The view for the ProposalMetrics page.</returns>
        public ActionResult ProposalMetrics()
        {
            return this.View();
        }

        #endregion Views

        /// <summary>
        /// Retrieves the PTM metrics data.
        /// </summary>
        /// <param name="year">The year to retrieve metrics data over.</param>
        /// <returns>The PTM metrics data.</returns>
        public JsonResult RetrieveMetricsData(int year)
        {
            JsonResult toReturn;
            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                MetricsDataModelView theModelViews = new MetricsDataModelView();
                ProposalDataMVDataLoader loader = new ProposalDataMVDataLoader();

                theModelViews.LOBs = loader.GetLoBShortNames();
                theModelViews.ProgramAreas = loader.GetProgramAreaShortNames();
                theModelViews.Proposals = loader.GetMetricsModelView(year);

                toReturn = this.Json(theModelViews);
            }

            toReturn.MaxJsonLength = int.MaxValue;
            return toReturn;
        }

        /// <summary>
        /// Retrieves the PTM proposal data.
        /// </summary>
        /// <returns>The PTM proposal data.</returns>
        public JsonResult RetrieveProposalData()
        {
            JsonResult toReturn = new JsonResult();
            try
            {
                using (StopwatchTimer sw = new StopwatchTimer(this.log))
                {
                    ProposalDataModelView theModelViews = new ProposalDataModelView();
                    ProposalDataMVDataLoader loader = new ProposalDataMVDataLoader();
                    theModelViews.LOBs = loader.GetLoBs();
                    theModelViews.ContractTypes = loader.GetContractTypes();
                    theModelViews.ProgramAreas = loader.GetProgramAreas();
                    theModelViews.Proposals = loader.GetProposalModelView();
                    int minYear = DateTime.Now.Year;
                    foreach (ProposalModelView proposal in theModelViews.Proposals)
                    {
                        if (proposal.CreatedDate.HasValue)
                        {
                            minYear = Math.Min(minYear, proposal.CreatedDate.Value.Year);
                        }
                    }

                    theModelViews.MinYear = minYear;
                    toReturn = this.Json(theModelViews);
                }

                toReturn.MaxJsonLength = int.MaxValue;
                return toReturn;
            }
            catch (Exception ex)
            {
                this.log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the opportunity data.
        /// </summary>
        /// <returns>The Opportunity Data.</returns>
        public JsonResult RetrieveOpportunityData()
        {
            JsonResult toReturn = new JsonResult();
            try
            {
                OTISOpportunityDataMVDataLoader otisLoader = new OTISOpportunityDataMVDataLoader();
                OTISOpportunityDataModelView otisModelViews = new OTISOpportunityDataModelView();
                using (StopwatchTimer sw = new StopwatchTimer(this.log))
                {
                    string url = ConfigurationManager.AppSettings["otis_url"];
                    Uri uri = SafeUriUtility.safeUri(@url);
                    otisModelViews.Opportunities = otisLoader.GetOTISOpportunityModelView(uri);
                    this.log.Performance("Finished RetrieveOpportunityData().", sw.ElapsedMilliseconds);
                }

                toReturn = this.Json(otisModelViews);
                toReturn.MaxJsonLength = int.MaxValue;
                return toReturn;
            }
            catch (WebException ex)
            {
                this.log.Error(ex);
                return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}