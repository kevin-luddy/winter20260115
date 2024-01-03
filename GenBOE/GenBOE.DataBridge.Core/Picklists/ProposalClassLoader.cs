// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using IES.Core;
    using IES.Core.PickList;
    using GenBOE.Models;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// ProposalClassLULoader
	/// </summary>
	public class ProposalClassLoader : PickListLoader
    {
        /// <summary>
        /// Default Ctor
        /// </summary>
        public ProposalClassLoader(ILogger<ProposalClassLoader> logger) : base(logger)
		{
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
                using (GenBoeEntities dbModel = new GenBoeEntities())
                {
                    result = dbModel.ProposalClassLUs.Select(x => new PickListDto()
                    {
                        Id = x.ProposalClassID,
                        Text = x.ProposalClass,
                        IsActive = x.IsActive,
                        InUse = x.Workspaces.Any()
                    }).OrderBy(p => p.Text).ToList();
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

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                if (dtoToUpsert != null)
                {
                    using (GenBoeEntities dbModel = new GenBoeEntities())
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

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                if (dtoToDelete != null && !dtoToDelete.InUse)
                {
                    toReturn = dtoToDelete.Id;

                    using (GenBoeEntities dbModel = new GenBoeEntities())
                    {
                        dbModel.deleteProposalClass(dtoToDelete.Id);
                    }
                }
            }

            return toReturn;
        }
    }
}
