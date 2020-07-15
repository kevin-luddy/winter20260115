// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// Changes that need to be applied to the Spread table on the UI.
    /// </summary>
    public class SpreadValueTableChanges
    {
        public int LaborTypeId { get; set; }

        /// <summary>
        /// If null, then this applies to an entire row
        /// </summary>
        public DateTime? SpreadMonth { get; set; }

        /// <summary>
        /// If non-null, then this identifies the specific spread value
        /// </summary>
        public int? SpreadValueId { get; set; }

        /// <summary>
        /// Specific monthly value (Cost OR Hours) for the resource type identified by <see cref="SpreadValueId"/>
        /// </summary>
        public decimal? SpreadValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SpreadValueTableChanges"/> is to be deleted.
        /// </summary>
        public bool Delete { get; set; }
    }

    /// <summary>
    /// Extension methods for <see cref="SpreadValueTableChanges"/>.
    /// </summary>
    public static class SpreadValueTableChangesExtensions
    {
        /// <summary>
        /// Return a change item object.  Developer should always use this method in lieu of an explicit constructor.
        /// </summary>
        /// <param name="changesList">Existing set of spread value table changes</param>
        /// <param name="laborTypeId">ID of the corresponding resource-type</param>
        /// <param name="spreadValueId">ID of the corresponding spread value entry</param>
        /// <returns>Change item object</returns>
        public static SpreadValueTableChanges GetOrCreateSpreadValueTableChanges(this ICollection<SpreadValueTableChanges> changesList, int laborTypeId, int spreadValueId)
        {
            SpreadValueTableChanges changes;
            if (changesList == null)
            {
                throw new ArgumentNullException(nameof(changesList));
            }
            else if ((changes = changesList.FirstOrDefault(c => c.SpreadValueId == spreadValueId && c.LaborTypeId == laborTypeId)) == null)
            {
                changes = new SpreadValueTableChanges
                {
                    SpreadValueId = spreadValueId,
                    LaborTypeId = laborTypeId
                };
                changesList.Add(changes);
            }
            return changes;
        }

        /// <summary>
        /// Return a change item object.  Developer should always use this method in lieu of an explicit constructor.
        /// </summary>
        /// <param name="changesList">Existing set of spread value table changes</param>
        /// <param name="laborTypeId">ID of the corresponding resource-type</param>
        /// <param name="spreadMonth">Month for the corresponding spread value entry</param>
        /// <returns>Change item object</returns>
        public static SpreadValueTableChanges GetOrCreateSpreadValueTableChanges(this ICollection<SpreadValueTableChanges> changesList, int laborTypeId, DateTime? spreadMonth)
        {
            SpreadValueTableChanges changes;
            if (changesList == null)
            {
                throw new ArgumentNullException(nameof(changesList));
            }
            else if ((changes = changesList.FirstOrDefault(c => c.SpreadMonth == spreadMonth && c.LaborTypeId == laborTypeId)) == null)
            {
                changes = new SpreadValueTableChanges
                {
                    SpreadMonth = spreadMonth,
                    LaborTypeId = laborTypeId
                };
                changesList.Add(changes);
            }
            return changes;
        }

        /// <summary>
        /// Copy pertinent spread data into the change item.
        /// </summary>
        /// <param name="changes">Change item</param>
        /// <param name="resourceTypeData">Resource-type data</param>
        /// <param name="spreadEntry">Spread data</param>
        public static void Copy(this SpreadValueTableChanges changes, LaborTypeDataModelView resourceTypeData, ResourceSpreadDto spreadEntry)
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }
            else if (resourceTypeData == null)
            {
                throw new ArgumentNullException(nameof(resourceTypeData));
            }
            else if (spreadEntry == null)
            {
                throw new ArgumentNullException(nameof(spreadEntry));
            }

            changes.SpreadMonth = spreadEntry.LaborSpreadDate.Normalize();
            changes.SpreadValueId = spreadEntry.Id;
            changes.SpreadValue = spreadEntry.LaborSpreadValue;
        }
    }
}
