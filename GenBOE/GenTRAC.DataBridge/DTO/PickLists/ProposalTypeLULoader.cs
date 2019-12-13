// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Common;
    using IES.Common.PickList;

    /// <summary>
    /// Proposal LU Loader
    /// </summary>
    public class ProposalTypeLULoader : PickListLoader
    {
        /// <summary>
        /// Default Ctor
        /// </summary>
        public ProposalTypeLULoader()
        {
            this.Log = new Logger(typeof(ProposalTypeLULoader));
        }

        /// <summary>
        /// Gets the corresponding LU table
        /// </summary>
        /// <returns>Proposal Types</returns>
        public override ICollection<PickListDto> GetPickListValues()
        {
            ICollection<PickListDto> result = new List<PickListDto>();

            using (StopwatchTimer sw = new StopwatchTimer("ProposalTypeLULoader.GetProposalTypes", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    result = dbModel.ProposalTypeLUs.Select(x => new PickListDto()
                    {
                        Id = x.ProposalTypeID,
                        Text = x.ProposalType,
                        IsActive = x.IsActive,
                        InUse = x.Proposals.Any()
                    }).ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">Dto that will be upserted</param>
        /// <returns>New Id</returns>
        protected override int? Upsert(PickListDto dtoToUpsert)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("ProposalTypeLULoader.Upsert", Log))
            {
                if (dtoToUpsert != null)
                {
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        string text = dtoToUpsert.Text;

                        // if we are updating & the item is in use, then we need to make sure we only modify the IsActive (in other words, we need to keep the same text..)
                        if (dtoToUpsert.InUse && dtoToUpsert.Id > 0)
                        {
                            text = dbModel.ProposalTypeLUs.First(x => x.ProposalTypeID == dtoToUpsert.Id).ProposalType;
                        }

                        toReturn = dbModel.upsertProposalType(dtoToUpsert.Id, text, dtoToUpsert.IsActive).First();
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Dto To Delete</param>
        /// <returns>Id of object being deleted</returns>
        protected override int? Delete(PickListDto dtoToDelete)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("ProposalTypeLULoader.Delete", Log))
            {
                if (dtoToDelete != null && !dtoToDelete.InUse)
                {
                    toReturn = dtoToDelete.Id;

                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        dbModel.deleteProposalType(dtoToDelete.Id);
                    }
                }
            }

            return toReturn;
        }
    }
}
