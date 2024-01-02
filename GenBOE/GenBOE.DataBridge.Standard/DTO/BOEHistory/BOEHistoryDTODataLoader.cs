// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Standard;
    using GenBOE.Dtos;
    using GenBOE.Models;

    /// <summary>
    /// BOE History DTO Data loader
    /// </summary>
    public class BOEHistoryDTODataLoader : IBOEHistoryDTODataLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _log;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BOEHistoryDTODataLoader(ILogger logger)
		{
			this._log = logger;
		}

        #region Retrieves

        /// <summary>
        /// Get the BOE History DTO for a given BOE ID
        /// </summary>
        /// <param name="inBoeID">BOE ID</param>
        /// <returns>all the BOE History logs</returns>
        
        virtual public ICollection<BOEHistoryDTO> GetBOEHistory(int inBoeID)
        {
            ICollection<BOEHistoryDTO> toReturn = new Collection<BOEHistoryDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from b in gbe.getBOEHistoryLog(inBoeID)
                                select new BOEHistoryDTO
                    {
                        Field = (FieldType)b.FieldID,
                        OldValue = b.OldValue,
                        NewValue = b.NewValue,
                        PerformedByETIUserId = b.ETIUserID,
                        Date = b.UpdateDT,
                        BoeID = inBoeID
                    }).ToCollection<BOEHistoryDTO>();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Used during exports, this methods gets (for every BoeId in the WS) the userId of the last user to submit the Boe for approval
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>Mapping of BoeIds and UserIds</returns>
        virtual public Dictionary<int, UserDTO> GetBoeIdsAndAuthorsThatLastSubmittedItForApprovalForWs(int wsId)
        {
            Dictionary<int, UserDTO> result = new Dictionary<int, UserDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var temp = gbe.BOEStateHistories
                    .Where(z => z.UpdatedBOEStateID == (int)BOEState.AwaitingApproval)
                    .Where(x => gbe.BOEs.Where(z => z.WorkspaceID == wsId).Select(z => z.BOEID).Contains(x.BOEID))
                    .Join(gbe.ETIusers, h => h.ChangedByETIUserID, u => u.ETIUserID, (h, u) => new
                    {
                        BoeId = h.BOEID,
                        DateOfChange = h.UpdateDT,
                        User = new UserDTO()
                        {
                            UserID = u.ETIUserID,
                            DisplayName = u.DisplayName,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            EmailAddress = (u.EmailAddress == null) ? string.Empty : u.EmailAddress.ToLower(),
                            NTID = u.NTID.ToLower(),
                            PhoneNumber = u.PhoneNumber,
                            UpdateDate = u.UpdateDT
                        }
                    }).ToList();

                List<int> boeIds = temp.Select(x => x.BoeId).Distinct().ToList();

                foreach (int id in boeIds)
                {
                    result.Add(id, temp.Where(x => x.BoeId == id).OrderBy(x => x.DateOfChange, SortOrder.Descending).Select(x => x.User).First());
                }
            }

            return result;
        }

        #endregion
    }
}
