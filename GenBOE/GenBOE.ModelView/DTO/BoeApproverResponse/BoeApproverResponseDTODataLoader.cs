// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    public class BoeApproverResponseDTODataLoader : DataLoader<BoeApproverResponseDTO>, IBoeApproverResponseDTODataLoader
    {
        public BoeApproverResponseDTODataLoader()
        {
            this.Log = new Logger(typeof(BoeApproverResponseDTODataLoader));
        }

        [DbQuery]
        public override ICollection<BoeApproverResponseDTO> GetByIds(ICollection<int> ids)
        {
            ICollection<BoeApproverResponseDTO> toReturn;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from b in gbe.BOEApprovals.Where(x => ids.Contains(x.BOEApprovalID))
                            select new BoeApproverResponseDTO
                            {
                                Id = b.BOEApprovalID,
                                ETIUserID = b.ApprovalETIUserID,
                                BoeID = b.BOEID,
                                ApproverResponded = b.ApprovedFlag,
                                UpdateDate = b.UpdateDT
                            }).ToCollection<BoeApproverResponseDTO>();
            }

            SetApproverResponse(toReturn);

            return toReturn;
        }

        /// <summary>
        /// Gets a list of approver responses associated with a Boe.
        /// </summary>
        /// <param name="boeId">Boe Id.</param>
        /// <returns>Approver responses associated to a Boe.</returns>
        public ICollection<BoeApproverResponseDTO> GetByBoeId(int boeId)
        {
            return this.GetByBoeIds(new List<int>() { boeId });
        }

        /// <summary>
        /// Gets a list of approver responses associated with a Boe int the boeIds.
        /// </summary>
        /// <param name="boeIds">Boe Ids to get approver responses for.</param>
        /// <returns>Approver responses associated to the Boes.</returns>
        [DbQuery]
        public ICollection<BoeApproverResponseDTO> GetByBoeIds(ICollection<int> boeIds)
        {
            ICollection<BoeApproverResponseDTO> approverResponses = new Collection<BoeApproverResponseDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                approverResponses = (from b in gbe.BOEApprovals
                                     where boeIds.Contains(b.BOEID)
                                     select new BoeApproverResponseDTO
                                     {
                                         Id = b.BOEApprovalID,
                                         ETIUserID = b.ApprovalETIUserID,
                                         BoeID = b.BOEID,
                                         ApproverResponded = b.ApprovedFlag,
                                         UpdateDate = b.UpdateDT
                                     }).ToCollection<BoeApproverResponseDTO>();
            }

            SetApproverResponse(approverResponses);

            return approverResponses;
        }

        /// <summary>
        /// Gets a list of approver responses associated with a Workspace.
        /// </summary>
        /// <param name="workspaceId">The Workspace.</param>
        /// <returns>Approver responses associated to a Workspace.</returns>
        [DbQuery]
        public IDictionary<int, ICollection<BoeApproverResponseDTO>> GetByWorkspaceId(int workspaceId)
        {
            Dictionary<int, ICollection<BoeApproverResponseDTO>> approverResponses = new Dictionary<int, ICollection<BoeApproverResponseDTO>>();

            // retrieve all approver responses by the workspace
            ICollection<BoeApproverResponseDTO> approverResponsesList = new Collection<BoeApproverResponseDTO>();
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                approverResponsesList = (from ba in gbe.BOEApprovals
                                         where ba.BOE.WorkspaceID == workspaceId
                                     select new BoeApproverResponseDTO
                                     {
                                         Id = ba.BOEApprovalID,
                                         ETIUserID = ba.ApprovalETIUserID,
                                         BoeID = ba.BOEID,
                                         ApproverResponded = ba.ApprovedFlag,
                                         UpdateDate = ba.UpdateDT
                                     }).ToCollection<BoeApproverResponseDTO>();
            }

            SetApproverResponse(approverResponsesList);

            // shove the responses into a dictionary keyed by the BOE Id
            foreach (BoeApproverResponseDTO response in approverResponsesList)
            {
                ICollection<BoeApproverResponseDTO> boeList;
                if (!approverResponses.TryGetValue(response.BoeID, out boeList))
                {
                    boeList = new List<BoeApproverResponseDTO>();
                    approverResponses.Add(response.BoeID, boeList);
                }

                boeList.Add(response);
            }

            return approverResponses;
        }

        /// <summary>
        /// Gets a list of approver user ids associated with Boes inside a workspace.
        /// </summary>
        /// <param name="workspaceId">Workspace to get approval user Ids for.</param>
        /// <returns>List of approver user ids associated with Boes inside a workspace.</returns>
        [DbQuery]
        public ICollection<int> GetApprovalUserIdsByWorkspaceId(int workspaceId)
        {
            ICollection<int> userIds = new List<int>();
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                userIds = (from ba in gbe.BOEApprovals
                            where ba.BOE.WorkspaceID == workspaceId
                            select ba.ApprovalETIUserID).Distinct().ToList();
            }

            return userIds;
        }

        /// <summary>
        /// Get the approval IDs based on boe id
        /// </summary>
        /// <param name="inBoeID"></param>
        /// <returns></returns>
        [DbQuery, Obsolete("Used for testing only")]
        internal ICollection<int> GetIdsByBoeID(int inBoeID)
        {
            List<int> result = new List<int>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                result = (from b in gbe.BOEApprovals
                          where b.BOEID == inBoeID
                          select b.BOEApprovalID).ToList();
            }

            return result;
        }

        /// <summary>
        /// Get all BOE Approvals in the system
        /// </summary>
        /// <returns>all approvals</returns>
        [DbQuery, Obsolete("Used for testing only")]
        internal ICollection<BoeApproverResponseDTO> GetAll()
        {
            ICollection<BoeApproverResponseDTO> result;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                result = (from b in gbe.BOEApprovals
                          select new BoeApproverResponseDTO
                          {
                                Id = b.BOEApprovalID,
                                ETIUserID = b.ApprovalETIUserID,
                                BoeID = b.BOEID,
                                ApproverResponded = b.ApprovedFlag,
                                UpdateDate = b.UpdateDT
                           }).ToCollection<BoeApproverResponseDTO>();
            }

            SetApproverResponse(result);

            return result;
        }

        #region Commits

        /// <summary>
        /// Deletes the specified dto.
        /// </summary>
        /// <param name="dtoToDelete">Dto to delete.</param>
        /// <returns>Id of the deleted object.</returns>
        protected override int? Delete(BoeApproverResponseDTO dtoToDelete)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                if (dtoToDelete != null)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.deleteBOEApprover(dtoToDelete.Id, dtoToDelete.UpdateDate, dtoToDelete.CurrentUserETIUserID);
                    }

                    toReturn = dtoToDelete.Id;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upsert the specified dto
        /// </summary>
        /// <param name="dtoToUpsert">dto to upsert</param>
        /// <returns>primary key</returns>
        protected override int? Upsert(BoeApproverResponseDTO dtoToUpsert)
        {
            int? result = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                if (dtoToUpsert != null)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        if (dtoToUpsert.Id > 0)
                        {
                            switch (dtoToUpsert.ApproverResponse)
                            {
                                case ApproverReponseType.Approved:
                                    dtoToUpsert.ApproverResponded = true;
                                    break;
                                case ApproverReponseType.Rejected:
                                    dtoToUpsert.ApproverResponded = false;
                                    break;
                                default:
                                    dtoToUpsert.ApproverResponded = null;
                                    break;
                            }

                            result = gbe.updateBOEApproval(dtoToUpsert.Id, dtoToUpsert.BoeID, dtoToUpsert.ETIUserID, dtoToUpsert.ApproverResponded, dtoToUpsert.UpdateDate).FirstOrDefault();
                        }
                        else
                        {
                            result = gbe.insertBOEApprover(
                                dtoToUpsert.BoeID,
                                dtoToUpsert.ETIUserID,
                                dtoToUpsert.CurrentUserETIUserID).FirstOrDefault();
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Sets the ApproverResponse property in each of the given dtos.
        /// </summary>
        /// <param name="approverResponseDtos">Boe approver response dtos.</param>
        private void SetApproverResponse(ICollection<BoeApproverResponseDTO> approverResponseDtos)
        {
            if (approverResponseDtos != null && approverResponseDtos.Any())
            {
                foreach (BoeApproverResponseDTO response in approverResponseDtos)
                {
                    // the BOEApprovals table is only keeping track of Approved or Rejected status by the ApprovedFlag
                    // Depending on what that flag is, set the enum ApproverResponse
                    // We want to use an enum so we can keep track of Approvers who have not yet responded
                    if (response.ApproverResponded == null)
                    {
                        response.ApproverResponse = ApproverReponseType.None;
                    }
                    else if (response.ApproverResponded.Value == true)
                    {
                        response.ApproverResponse = ApproverReponseType.Approved;
                    }
                    else
                    {
                        response.ApproverResponse = ApproverReponseType.Rejected;
                    }
                }
            }
        }
    }
}