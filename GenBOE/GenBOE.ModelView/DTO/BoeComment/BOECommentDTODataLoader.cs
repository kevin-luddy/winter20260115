// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common;
    using System.Collections.ObjectModel;
    using GenBOE.Models;
    using GenBOE.Dtos;

    public class BOECommentDTODataLoader : DataLoader<BOECommentDTO>, IBOECommentDTODataLoader
    {
        public BOECommentDTODataLoader()
        {
            this.Log = new Logger(typeof(BOECommentDTODataLoader));
        }

        #region Retrieves

        /// <summary>
        /// Get the BOE Comment DTO data
        /// </summary>
        /// <param name="inBOEID"> BOE ID</param>
        /// <returns>BOE Comment DTO data</returns>
        [DbQuery]
        public override ICollection<BOECommentDTO> GetByIds(ICollection<int> ids)
        {
            ICollection<BOECommentDTO> toReturn;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from c in gbe.BOEComments.Where(c => ids.Contains(c.BOECommentID))
                                select new BOECommentDTO
                                {
                                    Id = c.BOECommentID,
                                    FieldID = c.FieldID,
                                    BOEComment = c.BOEComments,
                                    BOEResponseToCommentID = c.BOEResponseToCommentID,
                                    BOECommentETIUserID = c.BOECommentETIUserID,
                                    BoeID = c.BOEID,
                                    UpdateDate = c.UpdateDT
                                }).ToCollection<BOECommentDTO>();
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Get all BOE Comment IDs given a BOE ID
        /// </summary>
        /// <param name="inBoeID">BOE ID</param>
        /// <returns>list of BOE Comment IDs</returns>
        [DbQuery]
        public virtual ICollection<BOECommentDTO> GetByBoeId(int inBoeID)
        {
            Collection<BOECommentDTO> toReturn = new Collection<BOECommentDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from c in gbe.BOEComments.Where(c => c.BOEID == inBoeID)
                                select new BOECommentDTO
                                {
                                    Id = c.BOECommentID,
                                    FieldID = c.FieldID,
                                    BOEComment = c.BOEComments,
                                    BOEResponseToCommentID = c.BOEResponseToCommentID,
                                    BOECommentETIUserID = c.BOECommentETIUserID,
                                    BoeID = c.BOEID,
                                    UpdateDate = c.UpdateDT
                                }).ToCollection();
                }

                return toReturn;
            }
        }
        #endregion

        #region Commits

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert"></param>
        /// <returns></returns>
        protected override int? Upsert(BOECommentDTO dtoToUpsert)
        {
            int? result = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                if (dtoToUpsert != null)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        result = gbe.upsertBOEComment(dtoToUpsert.Id, dtoToUpsert.FieldID, dtoToUpsert.BOEComment, dtoToUpsert.BOECommentETIUserID, dtoToUpsert.BOEResponseToCommentID, dtoToUpsert.BoeID, dtoToUpsert.UpdateDate).First();
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete"></param>
        /// <returns></returns>
        protected override int? Delete(BOECommentDTO dtoToDelete)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                if (dtoToDelete != null)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.deleteBOEComment(dtoToDelete.Id, dtoToDelete.UpdateDate);
                    }

                    toReturn = dtoToDelete.Id;
                }
                return toReturn;
            }
        }

        #endregion
    }
}
