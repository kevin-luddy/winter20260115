// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenTRAC.DataBridge.DTO.Reports
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Standard;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Generic loader that can be used to obtain info for the SSRS reports
	/// </summary>
	public class ReportsLoader : IReportsLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger Log { get; private set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReportsLoader(ILogger<ReportsLoader> logger)
		{
			this.Log = logger;
		}

        /// <summary>
        /// Get pricers.
        /// </summary>
        /// <returns>pricer user ids</returns>
        public ICollection<int> GetPricerUserIds()
        {
            ICollection<int> toReturn = new Collection<int>();

            using (StopwatchTimer sw = new StopwatchTimer("ReportsLoader.GetPricerUserIds", this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    // get user ids that match the pricer type
                    var resultLinq = from x in dbModel.ProposalUserRoles
                                     where x.RoleID == (int)PtmRole.Pricer
                                     select x.UserID;

                    toReturn = resultLinq.Distinct().ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get a collection of years that a proposal exists in
        /// </summary>
        /// <returns>proposal years</returns>
        public ICollection<int> GetProposalYears()
        {
            ICollection<int> toReturn = new Collection<int>();

            using (StopwatchTimer sw = new StopwatchTimer("ReportsLoader.GetProposalYears", this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    // get all proposal's date created
                    var dates = from x in dbModel.Proposals
                                     select x.DateCreated;

                    var years = dates.Select(x => x.Value.Year);

                    toReturn = years.Distinct().OrderBy(x => x).ToList();
                }
            }

            return toReturn;
        }
    }
}
