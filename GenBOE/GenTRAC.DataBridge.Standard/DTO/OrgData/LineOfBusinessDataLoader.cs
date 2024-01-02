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
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// LineOfBusinessDataLoader
	/// </summary>
	public class LineOfBusinessDataLoader : PickListLoader
    {
        /// <summary>
        /// LineOfBusinessDataLoader
        /// </summary>
        public LineOfBusinessDataLoader(ILogger<LineOfBusinessDataLoader> logger) : base(logger)
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
                    toReturn = gbe.LineOfBusinessLUs.Select(x => new PickListDto()
                    {
                        Id = x.LineOfBusinessID,
                        Text = x.LineOfBusinessName,
                        IsActive = x.IsActive,
                        InUse = x.Proposals.Any()
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
                        dbModel.deleteLOB(dtoToDelete.Id);
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
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        string text = dtoToUpsert.Text;
                        
                        // if we are updating & the item is in use, then we need to make sure we only modify the IsActive (in other words, we need to keep the same text..)
                        if (dtoToUpsert.InUse && dtoToUpsert.Id >= 0)
                        {
                            text = dbModel.LineOfBusinessLUs.First(x => x.LineOfBusinessID == dtoToUpsert.Id).LineOfBusinessName;
                        }

                        toReturn = dbModel.upsertLOB(dtoToUpsert.Id, text, dtoToUpsert.IsActive).First();
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the children pick list.
        /// </summary>
        public override PickListEnum? ChildrenPickList
        {
            get
            {
                return PickListEnum.ProgramArea;
            }
        }
    }
}