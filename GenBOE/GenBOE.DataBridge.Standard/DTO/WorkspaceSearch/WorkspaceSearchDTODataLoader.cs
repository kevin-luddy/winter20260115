// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Linq;
    using IES.Standard;
    using System.Collections.ObjectModel;
    using GenBOE.Models;
    using GenBOE.Dtos;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Workspace Search DTO Data Loader
	/// </summary>
	public class WorkspaceSearchDTODataLoader : IWorkspaceSearchDTODataLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _log;

        /// <summary>
        /// Default constructor
        /// </summary>
        public WorkspaceSearchDTODataLoader(ILogger<WorkspaceSearchDTODataLoader> logger)
		{
			this._log = logger;
		}

        /// <summary>
        /// Get the workspace IDs of any workspace matching the search criteria
        /// </summary>
        /// <param name="inWorkspaceSearch">search criteria</param>
        /// <returns>Workspace IDs</returns>
        
        public Collection<int> GetWorkspaceSearchResults(WorkspaceSearchDTO inWorkspaceSearch)
        {
            Collection<int> toReturn = new Collection<int>();

            if (inWorkspaceSearch == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceSearch));
            }

            // need to add double quotes around all strings for the SQL Full Text Search to work right especially with * wildcard
            string name = BOESearchDTODataLoader.FormSearchString(inWorkspaceSearch.WorkspaceName, BOESearchDTODataLoader.ADVANCED_SEARCH_DELIMITER);
            name = (name != "\"*\"") ? name : "\"\"";
            name = name.Replace("\" AND \"*\"", " *\"");
            name = name.Replace("\"*\" AND \"", "\"* ");
            name = name.Replace("AND \"-\"", ""); // cannot use space - space in a sql search query
            
            string desc = BOESearchDTODataLoader.FormSearchString(inWorkspaceSearch.WorkspaceDesc, BOESearchDTODataLoader.ADVANCED_SEARCH_DELIMITER);
            desc = (desc != "\"*\"") ? desc : "\"\"";
            desc = desc.Replace("\" AND \"*\"", " *\"");
            desc = desc.Replace("\"*\" AND \"", "\"* ");
            desc = desc.Replace("AND \"-\"", ""); // cannot use space - space in a sql search query

            string rfp = BOESearchDTODataLoader.FormSearchString(inWorkspaceSearch.RFPNumber, BOESearchDTODataLoader.ADVANCED_SEARCH_DELIMITER);
            rfp = (rfp != "\"*\"") ? rfp : "\"\"";

            string costVolumeLead = BOESearchDTODataLoader.FormSearchString(inWorkspaceSearch.CostVolumeLead, BOESearchDTODataLoader.ADVANCED_SEARCH_DELIMITER);
            costVolumeLead = (costVolumeLead != "\"*\"") ? costVolumeLead : "\"\"";

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var resultsLinq = from w in 
                                          gbe.getWorkspaceIDByWorkspaceAdvancedSearch(name, desc, inWorkspaceSearch.ProposalSubmitStartDate, inWorkspaceSearch.ProposalSubmitEndDate, rfp, costVolumeLead, (int?)inWorkspaceSearch.ProjectMapType)
                                          select w.Value;
                    
                    toReturn = new Collection<int>(resultsLinq.ToArray());
                }
            }

            return toReturn;
        }
    }
}
