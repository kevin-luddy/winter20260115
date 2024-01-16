// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.PickList;
	using IES.Common.Core.Utilities;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// ProgramAreaDataLoader
	/// </summary>
	public class ProgramAreaDataLoader : PickListLoader
    {
        /// <summary>
        /// ProgramAreaDataLoader
        /// </summary>
        public ProgramAreaDataLoader(ILogger<ProgramAreaDataLoader> logger) : base(logger)
		{
		}

        /// <summary>
        /// Abstract method for retrieving Pick List Values
        /// </summary>
        /// <returns>Pick list values</returns>
        public override ICollection<PickListDto> GetPickListValues()
        {
            ICollection<PickListDto> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (genTRACEntities gbe = new genTRACEntities())
                {
                    toReturn = gbe.ProgramAreaLUs.Select(x => new PickListDto()
                    {
                        Id = x.ProgramAreaID,
                        Text = x.ProgramAreaName,
                        IsActive = x.IsActive,
                        InUse = x.Proposals.Any(),
                        ParentId = x.LineOfBusinessID
                    }).OrderBy(p => p.Text).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Deletes a picklist dto
        /// </summary>
        /// <param name="dtoToDelete">The dto to delete</param>
        /// <returns>Id of the deleted object</returns>
        protected override int? Delete(PickListDto dtoToDelete)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                if (dtoToDelete != null && !dtoToDelete.InUse)
                {
                    toReturn = dtoToDelete.Id;

                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        dbModel.deleteProgramArea(dtoToDelete.Id);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upsert method for a picklist dto
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert.</param>
        /// <returns>Int representing the id of the upserted item.</returns>
        protected override int? Upsert(PickListDto dtoToUpsert)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                if (dtoToUpsert != null)
                {
                    if (dtoToUpsert.ParentIds == null || dtoToUpsert.ParentIds.Count != 1)
                    {
                        throw new ArgumentException("The Program Area needs a Line of Business selected as the Parent.");
                    }

                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        string text = dtoToUpsert.Text;
                        
                        // if we are updating & the item is in use, then we need to make sure we only modify the IsActive (in other words, we need to keep the same text..)
                        if (dtoToUpsert.InUse && dtoToUpsert.Id >= 0)
                        {
                            text = dbModel.ProgramAreaLUs.First(x => x.ProgramAreaID == dtoToUpsert.Id).ProgramAreaName;
                        }

                        toReturn = dbModel.upsertProgramArea(dtoToUpsert.Id, text, dtoToUpsert.IsActive, dtoToUpsert.ParentIds.First()).First();
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the parent pick list type, if applicable.
        /// </summary>
        public override PickListEnum? ParentPickList
        {
            get
            {
                return PickListEnum.LineOfBusiness;
            }
        }
    }
}