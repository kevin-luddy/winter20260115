// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using GenTRAC.DataBridge.Common;
    using IES.Standard;

    /// <summary>
    /// PAR Checklist Content Mapper
    /// </summary>
    public class PARChecklistContentMapper : IChecklistContentMapper
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected Logger Log { get; set; }

        /// <summary>
        /// Cache loader
        /// </summary>
        protected ICacheDataLoader CacheLoader { get; set; }

        /// <summary>
        /// Data loader for the mapper
        /// </summary>
        protected IChecklistContentLoader DataLoader { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="inChecklistContentLoader">Checklist Content Loader</param>
        /// <param name="inCacheLoader">Cache Loader</param>
        public PARChecklistContentMapper(IChecklistContentLoader inChecklistContentLoader, ICacheDataLoader inCacheLoader)
        {
            this.Log = new Logger(typeof(PARChecklistContentMapper));

            this.DataLoader = inChecklistContentLoader;
            this.CacheLoader = inCacheLoader;
        }

        /// <summary>
        /// Get Checklist Content DTO by Proposal ID
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>Checklist Content DTO</returns>
        public ChecklistContentDto GetChecklistByProposalId(int proposalId)
        {
            ChecklistContentDto result = null;

            using (StopwatchTimer sw = new StopwatchTimer("PARChecklistContentMapper.GetChecklistByProposalId", this.Log))
            {
                string idKey = CacheConstants.PAR_CHECKLIST_ID_BY_PROPOSAL_ID + proposalId;

                // load checklist id
                GetIdByIdDelegate cacheIdDelegate = new GetIdByIdDelegate(this.DataLoader.GetChecklistIdByProposalId);
                var tempId = this.CacheLoader.GetData(cacheIdDelegate, new object[] { proposalId }, idKey);

                if (tempId != null)
                {
                    int id = (int)tempId;

                    // now load checklist content
                    string contentKey = CacheConstants.PAR_CHECKLIST_CONTENT_BY_ID + id;
                    GetDtoByIdDelegate cacheContentDelegate = new GetDtoByIdDelegate(this.DataLoader.GetChecklistByProposalId);
                    var tempResult = this.CacheLoader.GetData(cacheContentDelegate, new object[] { proposalId }, contentKey);

                    if (tempResult != null)
                    {
                        result = (ChecklistContentDto)tempResult;
                    }
                }
            }

            return result;
        }
    }
}
