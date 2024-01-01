// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Standard;
    using IES.Standard.PickList;

    /// <summary>
    /// ProposalClassLULoader
    /// </summary>
    public class ProposalClassLULoader : PickListLoader
    {
        /// <summary>
        /// Default Ctor
        /// </summary>
        public ProposalClassLULoader(ILogger logger)
		{
			this.Log = logger;
		}

        /// <summary>
        /// Gets the corresponding LU table
        /// </summary>
        /// <returns>Levels Of Commitment</returns>
        public override ICollection<PickListDto> GetPickListValues()
        {
            ICollection<PickListDto> result = new List<PickListDto>();

            using (StopwatchTimer sw = new StopwatchTimer("ProposalClassLULoader.GetProposalTypes", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    result = dbModel.ProposalClassLUs.Select(x => new PickListDto()
                    {
                        Id = x.ProposalClassID,
                        Text = x.ProposalClass,
                        IsActive = x.IsActive,
                        InUse = x.Proposals.Any(),
                        IsReadOnly = x.IsReadOnly
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

            using (StopwatchTimer sw = new StopwatchTimer("ProposalClassLULoader.Upsert", Log))
            {
                if (dtoToUpsert != null)
                {
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        string text = dtoToUpsert.Text;

                        // if we are updating & the item is in use, then we need to make sure we only modify the IsActive (in other words, we need to keep the same text..)
                        if (dtoToUpsert.InUse && dtoToUpsert.Id >= 0)
                        {
                            text = dbModel.ProposalClassLUs.First(x => x.ProposalClassID == dtoToUpsert.Id).ProposalClass;
                        }

                        toReturn = dbModel.upsertProposalClass(dtoToUpsert.Id, text, dtoToUpsert.IsActive).First();
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

            using (StopwatchTimer sw = new StopwatchTimer("ProposalClassLULoader.Delete", Log))
            {
                // check the IsReadOnly flag from the DB before allowing delete
                bool isReadOnly;
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    isReadOnly = dbModel.ProposalClassLUs.First(x => x.ProposalClassID == dtoToDelete.Id).IsReadOnly;
                }

                if (dtoToDelete != null && !dtoToDelete.InUse && !isReadOnly)
                {
                    toReturn = dtoToDelete.Id;

                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        dbModel.deleteProposalClass(dtoToDelete.Id);
                    }
                }
            }

            return toReturn;
        }
    }
}
