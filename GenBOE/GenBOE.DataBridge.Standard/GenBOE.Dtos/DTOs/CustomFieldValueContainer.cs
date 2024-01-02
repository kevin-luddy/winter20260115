// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using IES.Standard;

    /// <summary>
    /// DTO that contains data for all custom field value xref data
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class CustomFieldValueContainer : UpdateableDTO
    {
        public CustomFieldValueContainer()
        {
            ContainerID = -1;
            CustomFieldID = -1;
            CustomFieldValueID = -1;
            IsOpenEnded = false;
            OpenEndedValue = string.Empty;
        }

        // primary key in custom field value xref table
        public int ContainerID { get; set; }

        // custom field ID
        public int CustomFieldID { get; set; }

        // custom field value ID
        public int CustomFieldValueID { get; set; }

        /// <summary>
        /// Gets/Sets bool noting if custom field is open ended
        /// </summary>
        public bool IsOpenEnded { get; set; }

        /// <summary>
        /// Gets/Sets Open Ended Value
        /// Not used for standard custom fields
        /// </summary>
        public string OpenEndedValue { get; set; }

        /// <summary>
        /// ID of the owner. For example, BOE Task Element ID
        /// </summary>
        public int OwnerID { get; set; }

        /// <summary>
        /// This method is required by the bulk load. It is a callback to update any children dtos with the new Id of an inserted
        /// custom field value. In this case, ContainerID is the primary key while the bulk load just deals with Ids. 
        /// </summary>
        /// <param name="newParentId">New id of the DTO on insert.</param>
        override protected void PropagateNewParentIdToChildDTOs(int newParentId)
        {
            ContainerID = newParentId;
        }
    }

    /// <summary>
    /// Triplicate mapping of XREF Container ID, Custom Field ID and Custom Field Value ID
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class CustomFieldContainerFieldValueMapping
    {
        public CustomFieldContainerFieldValueMapping()
        {
            this.ContainerID = -1;
            this.CustomFieldID = -1;
            this.CustomFieldValueID = -1;
            this.CustomFieldValueDescription = string.Empty;
        }

        public int ContainerID { get; set; }
        public int CustomFieldID { get; set; }
        public int CustomFieldValueID { get; set; }
        public string CustomFieldValueDescription { get; set; }
    }

    public static class CustomFieldValueContainerExtensions
    {
        public static void Merge(this ICollection<CustomFieldValueContainer> existing, ICollection<CustomFieldValueContainer> newest, ICollection<CustomFieldContainerFieldValueMapping> mappings)
        {
            if (existing == null)
            {
                throw new ArgumentNullException(nameof(existing));
            }

            var addQuery =
                from i in newest
                join e in mappings on i.ContainerID equals e.CustomFieldID into g
                from k in g.DefaultIfEmpty()
                select new
                {
                    ContainerID = 0,
                    CustomFieldID = i.ContainerID,
                    CustomFieldValueID = i.CustomFieldValueID,
                    OpenEndedValue = i.OpenEndedValue,
                    IsOpenEnded = i.IsOpenEnded,
                    Existing = k
                };

            var updateQuery =
                (from e in mappings
                join i in newest on e.CustomFieldID equals i.ContainerID into g
                from k in g.DefaultIfEmpty()
                select new
                {
                    ContainerID = e.ContainerID,
                    CustomFieldID = e.CustomFieldID,
                    CustomFieldValueID = e.CustomFieldValueID,
                    OpenEndedValue = e.CustomFieldValueDescription,
                    Imported = k
                }).ToList();
            
            ICollection<CustomFieldValueContainer> customFieldValueAdds = addQuery.Where(q => q.Existing == null).Select(r => new CustomFieldValueContainer { ContainerID = r.ContainerID, CustomFieldID = r.CustomFieldID, CustomFieldValueID = r.CustomFieldValueID, IsOpenEnded = r.IsOpenEnded, OpenEndedValue = r.OpenEndedValue }).ToList();
            ICollection<int> customFieldContainerIdsToBeDeleted = updateQuery.Where(q => q.Imported == null).Select(q => q.ContainerID).ToList();
            ICollection<CustomFieldValueContainer> customFieldValueUpdates = updateQuery.Where(q => q.Imported != null && ((!q.Imported.IsOpenEnded && q.CustomFieldValueID != q.Imported.CustomFieldValueID) || q.Imported.IsOpenEnded && q.OpenEndedValue != q.Imported.OpenEndedValue)).Select(r => new CustomFieldValueContainer { ContainerID = r.ContainerID, CustomFieldValueID = r.Imported.CustomFieldValueID, OpenEndedValue = r.Imported.OpenEndedValue }).ToList();

            foreach (CustomFieldValueContainer customField in customFieldValueAdds)
            {
                existing.Add(new CustomFieldValueContainer
                {
                    ContainerID = -1,
                    CustomFieldValueID = customField.CustomFieldValueID,
                    CustomFieldID = customField.CustomFieldID,
                    IsOpenEnded = customField.IsOpenEnded,
                    OpenEndedValue = customField.OpenEndedValue,
                    Updateable = UpdateType.Upsert
                });
            }

            foreach (int containerID in customFieldContainerIdsToBeDeleted)
            {
                CustomFieldValueContainer existingValue = existing.FirstOrDefault(f => f.ContainerID == containerID);
                existingValue.Updateable = UpdateType.Deleted;
            }

            foreach (CustomFieldValueContainer customField in customFieldValueUpdates)
            {
                CustomFieldValueContainer existingValue = existing.FirstOrDefault(f => f.ContainerID == customField.ContainerID);
                existingValue.Updateable = UpdateType.Upsert;
                existingValue.CustomFieldValueID = customField.CustomFieldValueID;
                existingValue.OpenEndedValue = customField.OpenEndedValue;
            }
        }
    }
}
