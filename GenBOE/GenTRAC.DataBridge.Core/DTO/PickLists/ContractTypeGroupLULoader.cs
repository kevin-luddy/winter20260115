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
    using IES.Common.Core;
    using IES.Common.Core.PickList;
	using IES.Common.Core.Utilities;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// ContractTypeGroupLULoader
	/// </summary>
	public class ContractTypeGroupLULoader : PickListLoader
    {
        /// <summary>
        /// Default Ctor
        /// </summary>
        public ContractTypeGroupLULoader(ILogger<ContractTypeGroupLULoader> logger) : base(logger)
		{
		}

        /// <summary>
        /// Gets the corresponding LU table
        /// </summary>
        /// <returns>Levels Of Commitment</returns>
        public override ICollection<PickListDto> GetPickListValues()
        {
            ICollection<PickListDto> result = new List<PickListDto>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    result = dbModel.ContractTypeGroupLUs.Select(x => new PickListDto()
                    {
                        Id = x.ContractTypeGroupID,
                        Text = x.ContractTypeGroup,
                        IsActive = x.IsActive,
                        InUse = x.Proposals.Any()
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

            using (StopwatchTimer sw = new StopwatchTimer("ContractTypeGroupLULoader.Upsert", Log))
            {
                if (dtoToUpsert != null)
                {
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        string text = dtoToUpsert.Text;

                        // if we are updating & the item is in use, then we need to make sure we only modify the IsActive (in other words, we need to keep the same text..)
                        if (dtoToUpsert.InUse && dtoToUpsert.Id >= 0)
                        {
                            text = dbModel.ContractTypeGroupLUs.First(x => x.ContractTypeGroupID == dtoToUpsert.Id).ContractTypeGroup;
                        }

                        toReturn = dbModel.upsertContractTypeGroup(dtoToUpsert.Id, text, dtoToUpsert.IsActive).First();
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

            using (StopwatchTimer sw = new StopwatchTimer("ContractTypeGroupLULoader.Delete", Log))
            {
                if (dtoToDelete != null && !dtoToDelete.InUse)
                {
                    toReturn = dtoToDelete.Id;

                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        dbModel.deleteContractTypeGroup(dtoToDelete.Id);
                    }
                }
            }

            return toReturn;
        }
    }
}
