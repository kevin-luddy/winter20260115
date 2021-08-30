// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Search
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    public class PerfOrgSearch
    {
        private Logger _log = new Logger (typeof(PerfOrgSearch));
        private IPerformingOrgDTODataLoader perfOrgLoader;

        public PerfOrgSearch(IPerformingOrgDTODataLoader perfOrgLoader)
        {
            this.perfOrgLoader = perfOrgLoader;
        }

        /// <summary>
        /// Search the default Performing Organization List Name (AKA ID on UI) and Description
        /// for the given input seach text
        /// </summary>
        /// <param name="inSearchText">string to search for</param>
        /// <returns>all possible matches with given string</returns>
        public Collection<PerformingOrgDTO> SearchDefaultPerfOrgs(string inSearchText)
        {
            Collection<PerformingOrgDTO> toReturn = new Collection<PerformingOrgDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                // Get the global performing orgs
                Collection<PerformingOrgDTO> globalPerformingOrgs = this.perfOrgLoader.GetGlobalPerformingOrgs();

                // search through the global performing org list
                var SearchList = from p in globalPerformingOrgs
                                 where p.PerformingOrgName.ToLower().Contains(inSearchText.ToLower()) || p.PerformingOrgDesc.ToLower().Contains(inSearchText.ToLower())
                                 select p;

                toReturn = new Collection<PerformingOrgDTO>(SearchList.ToArray());
            }

            return toReturn;
        }
    }
}
