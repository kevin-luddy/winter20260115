// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.Dtos;
    using IES.Common;

    public class CustomFieldImportModelView
    {
        /// <summary>
        /// Gets or sets the primary key in custom field value xref table.
        /// </summary>
        public int ContainerID { get; set; }

        /// <summary>
        /// Gets or sets the custom field value identifier.
        /// </summary>
        public int CustomFieldValueID { get; set; }

        /// <summary>
        /// Update Type
        /// </summary>
        public int Updateable { get; set; }

        /// <summary>
        /// Unique Id for the DTO
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the update date as a long.
        /// </summary>
        public string UpdateDateLong { get; set; }

        /// <summary>
        /// Gets or sets the Custom Field ID
        /// </summary>
        public int CustomFieldID { get; set; }

        /// <summary>
        /// Gets or sets bool noting if CF is open ended
        /// </summary>
        public bool IsOpenEnded { get; set; }

        /// <summary>
        /// Gets or sets the value of the Open Ended CF, or the CF description
        /// </summary>
        public string OpenEndedValue { get; set; }
    }

    /// <summary>
    /// Extensions for the CustomFieldImportModelView class.
    /// </summary>
    public static class CustomFieldImportModelViewExtensions
    {
        /// <summary>
        /// Converts the list to a containers list.
        /// </summary>
        /// <param name="customFields">The custom fields.</param>
        /// <returns>List of custom field value containers</returns>
        public static Collection<CustomFieldValueContainer> ToContainers(this ICollection<CustomFieldImportModelView> customFields)
        {
            Collection<CustomFieldValueContainer> containers = new Collection<CustomFieldValueContainer>();

            if (customFields != null && customFields.Any())
            {
                foreach (CustomFieldImportModelView customField in customFields)
                {
                    containers.Add(new CustomFieldValueContainer()
                    {
                        Id = customField.Id,
                        Updateable = (UpdateType)customField.Updateable,
                        UpdateDate = new DateTime(long.Parse(customField.UpdateDateLong)),
                        ContainerID = customField.ContainerID,
                        CustomFieldID = customField.CustomFieldID,
                        CustomFieldValueID = customField.CustomFieldValueID,
                        IsOpenEnded = customField.IsOpenEnded,
                        OpenEndedValue = customField.OpenEndedValue
                    });
                }
            }

            return containers;
        }
    }
}