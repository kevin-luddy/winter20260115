// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;

    /// <summary>
    /// BOE Search DTO Data Loader
    /// </summary>
    public class BOESearchDTODataLoader : IBOESearchDTODataLoader
    {
        #region Constants
        /// <summary>
        /// Constant for quick search delimiter
        /// </summary>
        public static readonly string QUICK_SEARCH_DELIMITER = "~";

        /// <summary>
        /// Constant for advanced search delimiter
        /// </summary>
        public static readonly string ADVANCED_SEARCH_DELIMITER = " AND ";
        #endregion

        /// <summary>
        /// Logger
        /// </summary>
        private Logger log = new Logger(typeof(BOESearchDTODataLoader));

        /// <summary>
        /// Default constructor
        /// </summary>
        public BOESearchDTODataLoader() { }

        #region Retrieves

        /// <summary>
        /// Gets the ids of all BOES  matching the quick search parameters
        /// </summary>
        /// <param name="inSearchParams">Quick Search parameters</param>
        /// <returns>ids of all matching BOEs</returns>
        [DbQuery]
        virtual public ICollection<BOESearchResultDTO> GetQuickSearchResults(BOESearchDTO inSearchParams)
        {
            if (inSearchParams == null)
            {
                throw new ArgumentNullException(nameof(inSearchParams));
            }

            ICollection<BOESearchResultDTO> toReturn = new Collection<BOESearchResultDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.Database.CommandTimeout = 360;

                    toReturn = (from b in gbe.getBOEIDByWorkspaceQuickSearch(
                                    FormSearchString(inSearchParams.QuickSearchText, QUICK_SEARCH_DELIMITER),
                                    (int)inSearchParams.SelectedCategory,
                                    inSearchParams.WorkspaceID, 
                                    inSearchParams.BOEID, 
                                    inSearchParams.SearchResultsThreshold)
                                select new BOESearchResultDTO
                                {
                                    BOEID = b.BOEID,
                                    WorkspaceID = b.WorkspaceId
                                }).ToCollection<BOESearchResultDTO>();
                }
            }
            
            return toReturn;
        }

        /// <summary>
        /// Gets the ids of all BOES  matching the quick search parameters
        /// </summary>
        /// <param name="inSearchParams">Quick Search parameters</param>
        /// <returns>ids of all matching BOEs</returns>
        [DbQuery]
        virtual public ICollection<BOESearchResultDTO> GetQuickSearchResults(BOEProjectMapSearchDTO inSearchParams)
        {
            if (inSearchParams == null)
            {
                throw new ArgumentNullException(nameof(inSearchParams));
            }

            ICollection<BOESearchResultDTO> toReturn = new Collection<BOESearchResultDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.Database.CommandTimeout = 360;

                    toReturn = (from b in gbe.getProjectMapIDForProjectMapQuickSearch(
                                    FormSearchString(inSearchParams.QuickSearchText, QUICK_SEARCH_DELIMITER),
                                    inSearchParams.WorkspaceID,
                                    inSearchParams.SearchResultsThreshold)
                                select new BOESearchResultDTO
                                {
                                    ProjectMapId = b.ProjectMapId.Value,
                                    WorkspaceID = b.WorkspaceId.Value
                                }).ToCollection<BOESearchResultDTO>();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the ids of all BOES  matching the advanced search parameters
        /// </summary>
        /// <param name="inSearchParams">Advanced Search parameters</param>
        /// <returns>ids of all matching BOEs</returns>
        [DbQuery]
        virtual public ICollection<BOESearchResultDTO> GetAdvancedSearchResults(BOESearchDTO inSearchParams)
        {
            if (inSearchParams == null)
            {
                throw new ArgumentNullException(nameof(inSearchParams));
            }

            ICollection<BOESearchResultDTO> toReturn = new Collection<BOESearchResultDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.Database.CommandTimeout = 360;

                    toReturn = (from b in gbe.getBOEIDByWorkspaceAdvancedSearch(
                                                             FormSearchString(inSearchParams.WorkspaceName, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.WorkspaceDescription, ADVANCED_SEARCH_DELIMITER),
                                                             inSearchParams.StartDate,
                                                             inSearchParams.EndDate,
                                                             FormSearchString(inSearchParams.RFP, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.BoeDescription, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.TaskTitle, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.TaskDescription, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.SourcesOfData, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.PerformingOrg, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.CostVolumeLeadUser, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.AuthorUser, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.ApproverUser, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.CLINNumber, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.CLINTitle, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.WBSNumber, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.WBSTitle, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.BoeTitle, ADVANCED_SEARCH_DELIMITER),
                                                             Convert.ToInt32(inSearchParams.SelectedCategory),
                                                             inSearchParams.WorkspaceID,
                                                             inSearchParams.BOEID,
                                                             inSearchParams.SearchResultsThreshold
                                                             )
                                select new BOESearchResultDTO
                                {
                                    BOEID = b.BOEID,
                                    WorkspaceID = b.WorkspaceId
                                }).ToCollection<BOESearchResultDTO>();

                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the ids of all BOES  matching the advanced search parameters
        /// </summary>
        /// <param name="inSearchParams">Advanced Search parameters</param>
        /// <returns>ids of all matching BOEs</returns>
        [DbQuery]
        virtual public ICollection<BOESearchResultDTO> GetAdvancedSearchResults(BOEProjectMapSearchDTO inSearchParams)
        {
            if (inSearchParams == null)
            {
                throw new ArgumentNullException(nameof(inSearchParams));
            }

            ICollection<BOESearchResultDTO> toReturn = new Collection<BOESearchResultDTO>();

            string wbsNumberSearch = string.Concat("%", (inSearchParams.WBSNumber ?? ""), "%");
            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.Database.CommandTimeout = 360;

                    toReturn = (from b in gbe.getProjectMapIDForProjectMapAdvancedSearch(
                                                             FormSearchString(inSearchParams.WorkspaceName, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.WorkspaceDescription, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.ActivityId, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.ActivityName, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.CamName, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.Category, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.SowTitle, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.Task, ADVANCED_SEARCH_DELIMITER),
                                                             FormSearchString(inSearchParams.Rationale, ADVANCED_SEARCH_DELIMITER),
                                                             wbsNumberSearch,
                                                             FormSearchString(inSearchParams.WBSTitle, ADVANCED_SEARCH_DELIMITER),
                                                             inSearchParams.WorkspaceID,
                                                             inSearchParams.SearchResultsThreshold
                                                             )
                                select new BOESearchResultDTO
                                {
                                    ProjectMapId = b.ProjectMapId,
                                    WorkspaceID = b.WorkspaceId
                                }).ToCollection<BOESearchResultDTO>();

                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the search results.
        /// </summary>
        /// <param name="projectMapIds">The projectMap ids.</param>
        /// <returns>A list of search results.</returns>
        public virtual ICollection<BOESearchResult> GetSearchResults(ICollection<int> projectMapIds)
        {
            if (projectMapIds == null || projectMapIds.None())
            {
                throw new ArgumentNullException(nameof(projectMapIds));
            }

            List<BOESearchResult> toReturn = new List<BOESearchResult>();

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.Database.CommandTimeout = 360;

                    // get the basic ProjectMap data from the sprocResults
                    toReturn = (from p in gbe.ProjectMaps
                                where projectMapIds.Contains(p.ID)
                                join w in gbe.Workspaces on p.WorkspaceId equals w.WorkspaceID
                                orderby p.ID
                                select new BOESearchResult
                                {
                                    // 2 RTE fields:
                                    BOEDescription = p.ActivityName,
                                    ProjectMapId = p.ID,
                                    BOETitle = p.ActivityID,
                                    CamName = p.CamName,
                                    Category = p.Category,
                                    ClinNumber = string.Empty,
                                    ClinTitle = p.CLIN,
                                    IsCopyAllTaskElementsSelected = true,
                                    WBSNumber = p.WbsNumber,
                                    WBSTitle = p.WbsElementTitle,
                                    WorkspaceName = w.WorkspaceName,
                                    WorkspaceShortName = w.WorkspaceShortName,
                                    WorkspaceId = p.WorkspaceId,
                                    SubmittalDate = w.ProposalSubmitDate,
                                    ProjectMapTask = p.Task
                                }).ToList();

                }
            }

            return toReturn;
        }

        /// <summary>
        /// Creates the formatted search string
        /// </summary>
        /// <param name="searchParam">search parameter</param>
        /// <param name="delimiter">delimiter between search parameters</param>
        /// <returns>formatted search string</returns>
        public static string FormSearchString(string searchParam, string delimiter)
        {
            if (delimiter == null)
            {
                throw new ArgumentNullException(nameof(delimiter));
            }

            string toReturn = string.Empty;
            if (!string.IsNullOrWhiteSpace(searchParam))
            {
                StringBuilder finalSearchString = new StringBuilder();
                while (searchParam.Contains("\""))
                {
                    string beginning = searchParam.Substring(0, searchParam.IndexOf('"')).Trim();
                    string middle = searchParam.Substring(searchParam.IndexOf('"') + 1, searchParam.Length - (searchParam.IndexOf('"') + 1));
                    string end = middle.Substring(middle.IndexOf('"') + 1, middle.Length - (middle.IndexOf('"') + 1)).Trim();
                    if(middle.IndexOf('"') != -1)
                    {
                        middle = middle.Substring(0, middle.IndexOf('"')).Trim();
                    }
                    finalSearchString.Append('"').Append(middle).Append('"').Append(delimiter);
                    searchParam = beginning + " " + end;
                }
                string[] searchStrings = searchParam.Trim().Split(' ');
                foreach (string searchString in searchStrings)
                {
                    // Make sure search string has at least one alphanumeric character
                    if (!string.IsNullOrEmpty(searchString) && searchString.Any(x => Char.IsLetterOrDigit(x)))
                    {
                        finalSearchString.Append('"').Append(searchString).Append('"').Append(delimiter);
                    }
                }
                toReturn = finalSearchString.Remove(finalSearchString.Length - delimiter.Length, delimiter.Length).ToString();
            }
            else
            {
                toReturn = "\"\"";
            }

            toReturn = toReturn.Replace(",\"", "\"");
            return toReturn;
        }

        #endregion
    }
}
