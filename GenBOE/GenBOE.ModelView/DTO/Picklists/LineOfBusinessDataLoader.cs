// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using GenBOE.Models;
    using IES.Common;
    using IES.Common.PickList;

    /// <summary>
    /// LineOfBusinessDataLoader
    /// </summary>
    public class LineOfBusinessDataLoader : PickListLoader
    {
        /// <summary>
        /// LineOfBusinessDataLoader
        /// </summary>
        public LineOfBusinessDataLoader()
        {
            this.Log = new Logger(typeof(LineOfBusinessDataLoader));
        }

        /// <summary>
        /// Returns a Collection of all LOBs
        /// </summary>
        /// <returns>Collection of all LOBs</returns>
        public override ICollection<PickListDto> GetPickListValues()
        {
            ICollection<PickListDto> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = gbe.LineOfBusinesses.OrderBy(l => l.LineOfBusinessName)
                        .Select(x => new PickListDto()
                        {
                            Id = x.LineOfBusinessID,
                            Text = x.LineOfBusinessName,
                            IsActive = x.IsActive,
                            InUse = x.Workspaces.Any()
                        })
                        .ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Deletes a LOB.
        /// </summary>
        /// <param name="dtoToDelete">Dto that will be deleted.</param>
        /// <returns>Id of the deleted object</returns>
        protected override int? Delete(PickListDto dtoToDelete)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                if (dtoToDelete != null && !dtoToDelete.InUse)
                {
                    toReturn = dtoToDelete.Id;

                    using (GenBoeEntities dbModel = new GenBoeEntities())
                    {
                        dbModel.deleteLOB(dtoToDelete.Id);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upsert method to be overridden by the derived class
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
                    using (GenBoeEntities dbModel = new GenBoeEntities())
                    {
                        string text = dtoToUpsert.Text;
                        
                        // if we are updating & the item is in use, then we need to make sure we only modify the IsActive (in other words, we need to keep the same text..)
                        if (dtoToUpsert.InUse && dtoToUpsert.Id >= 0)
                        {
                            text = dbModel.LineOfBusinesses.First(x => x.LineOfBusinessID == dtoToUpsert.Id).LineOfBusinessName;
                        }

                        toReturn = dbModel.upsertLOB(dtoToUpsert.Id, text, dtoToUpsert.IsActive).First();
                    }
                }
            }

            return toReturn;
        }
    }
}