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
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Objects;

    public class ExportToProPricerModelView : PersistedDataModelView, IComparable
    {
        public ExportToProPricerModelView()
            : base()
        {
            Name = string.Empty;
            Scope = ProPricerScope.Workspace;
            Tasks = new Collection<ProPricerTasks>();
            Resources = new Collection<ProPricerResources>();
            InvalidCustomFields = new Collection<string>();
            WorkspaceID = -1;
            Deleted = false;
            UpdateDate = DateTime.MinValue;
        }

        public ExportToProPricerModelView(ProPricerDTO dto)
            : this()
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            ID = dto.ExportID;
            Name = dto.FormatName;
            Scope = dto.Scope;
            Tasks = dto.ProPricerTasks;
            Resources = dto.ProPricerResources;
            WorkspaceID = dto.WorkspaceID.HasValue ? dto.WorkspaceID.Value : -1;
            Deleted = (dto.Updateable == UpdateType.Deleted) ? true : false;
            UpdateDate = dto.UpdateDate;
        }

        public ExportToProPricerModelView(ProPricerDTO dto, IFullObjectFactory factory, IReadOnlyCollection<CustomFieldDTO> availableCustomFields)
            : this(dto)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            // For System-scoped formats, determine which custom fields in the format are invalid because
            // they aren't contained in this workspace
            if (Scope == ProPricerScope.System)
            {

                var customTaskNames = (from t in Tasks
                                    where !string.IsNullOrWhiteSpace(t.CustomFieldName)
                                    select t.CustomFieldName.ToUpper()).ToArray();

                var customResourceNames = (from r in Resources
                                         where !string.IsNullOrWhiteSpace(r.CustomFieldName)
                                         select r.CustomFieldName.ToUpper()).ToArray();

                // Get distinct Names for all custom fields USED in the format
                var customFieldNamesInFormat = customTaskNames.Union(customResourceNames).Distinct();

                // For each custom field used in the format, get the matching custom field in this workspace.
                // Match using custom field name, BOE display, Task display and Labor Type display.
                var matchingCustomFields = (from c in customFieldNamesInFormat
                                            from a in availableCustomFields
                                            where c.Equals(a.CustomFieldName, StringComparison.CurrentCultureIgnoreCase)
                                            select new
                                            {
                                                CustomFieldName = c,
                                                MatchingDTO = a
                                            }
                                           ).ToDictionary(m => m.CustomFieldName, m => m.MatchingDTO);

                // Names of custom fields that ARE in the workspace
                var matchingCustomFieldNames = matchingCustomFields.Select(a => a.Value.CustomFieldName.ToUpper());

                // Names of custom fields used in the format, that aren't available in the workspace
                InvalidCustomFields = new Collection<string>(customFieldNamesInFormat.Except(matchingCustomFieldNames).ToArray());

                // If all custom fields in the format are also in this workspace, then we'll convert the original custom
                // field IDs to the corresponding IDs of fields in this workspace
                if (InvalidCustomFields.Count == 0)
                {
                    foreach (var task in Tasks)
                    {
                        if (!string.IsNullOrWhiteSpace(task.CustomFieldName))
                        {
                            task.CustomFieldID = matchingCustomFields[task.CustomFieldName.ToUpper()].Id;
                        }
                    }

                    foreach (var resource in Resources)
                    {
                        if (!string.IsNullOrWhiteSpace(resource.CustomFieldName))
                        {
                            resource.CustomFieldID = matchingCustomFields[resource.CustomFieldName.ToUpper()].Id;
                        }
                    }
                }
            }
        }

        public ProPricerDTO GetAssociatedDTO()
        {
            ProPricerDTO toReturn = new ProPricerDTO();

            toReturn.ExportID = ID;
            toReturn.FormatName = Name;
            toReturn.Scope = Scope;
            toReturn.ProPricerTasks = Tasks;
            toReturn.ProPricerResources = Resources;
            toReturn.Updateable = Deleted ? UpdateType.Deleted : UpdateType.Upsert;
            toReturn.UpdateDate = new DateTime(this.UpdateDate.Ticks);
            toReturn.WorkspaceID = WorkspaceID;

            return toReturn;
        }

        public static string GetFormattedCustomFieldID(ProPricerTasks task, bool isSystem = false)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            var toReturn = ((int)task.Task).ToString();

            if (task.CustomFieldID.HasValue)
            {
                toReturn = GetFormattedCustomFieldID(task.CustomFieldID.Value, task.Selection);
            }

            if (isSystem && !string.IsNullOrWhiteSpace(task.CustomFieldName))
            {
                toReturn = GetFormattedCustomFieldID(task.CustomFieldName, task.Selection);
            }

            return toReturn;
        }

        public static string GetFormattedCustomFieldID(ProPricerResources resource, bool isSystem = false)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            var toReturn = ((int)resource.Resource).ToString();

            if (resource.CustomFieldID.HasValue)
            {
                toReturn = GetFormattedCustomFieldID(resource.CustomFieldID.Value, resource.Selection);
            }

            if (isSystem && !string.IsNullOrWhiteSpace(resource.CustomFieldName))
            {
                toReturn = GetFormattedCustomFieldID(resource.CustomFieldName, resource.Selection);
            }

            return toReturn;
        }

        public static string GetFormattedCustomFieldID(int fieldID, ProPricerCustomFieldSelection selection)
        {
            return fieldID + "-" + (int)selection;
        }

        public static string GetFormattedCustomFieldID(string customeFieldName, ProPricerCustomFieldSelection selection)
        {
            return customeFieldName + "-" + (int)selection;
        }

        public int ID { get; set; }

        [Required(ErrorMessage = "Format Name is required.")]
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed for the Format Name.")]
        public string Name { get; set; }

        public ProPricerScope Scope { get; set; }
        public ICollection<ProPricerTasks> Tasks { get; set; }
        public ICollection<ProPricerResources> Resources { get; set; }
        public Collection<string> InvalidCustomFields { get; set; }
        public int WorkspaceID { get; set; }
        public bool Deleted { get; set; }
        
        // Gets an ordered list of the combined Task selections for this format
        public ICollection<string> OrderedTasks
        {
            get
            {
                var toReturn = new Collection<string>();

                var orderedTasks = from t in Tasks
                                   orderby t.ListOrder
                                   select GetFormattedCustomFieldID(t);

                if (orderedTasks.Count() > 0)
                {
                    toReturn = new Collection<string>(orderedTasks.ToArray());
                }

                return toReturn;
            }
        }

        // Gets an ordered list of the combined Resource selections for this format
        public ICollection<string> OrderedResources
        {
            get
            {
                var toReturn = new Collection<string>();

                var orderedResources = from r in Resources
                                       orderby r.ListOrder
                                       select GetFormattedCustomFieldID(r);

                if (orderedResources.Count() > 0)
                {
                    toReturn = new Collection<string>(orderedResources.ToArray());
                }

                return toReturn;
            }
        }

        // Gets an ordered list of the combined Task selections for this format
        public ICollection<string> SystemOrderedTasks
        {
            get
            {
                var toReturn = new Collection<string>();

                var orderedTasks = from t in Tasks
                                   orderby t.ListOrder
                                   select GetFormattedCustomFieldID(t, true);

                if (orderedTasks.Count() > 0)
                {
                    toReturn = new Collection<string>(orderedTasks.ToArray());
                }

                return toReturn;
            }
        }

        // Gets an ordered list of the combined Resource selections for this format
        public ICollection<string> SystemOrderedResources
        {
            get
            {
                var toReturn = new Collection<string>();

                var orderedResources = from r in Resources
                                       orderby r.ListOrder
                                       select GetFormattedCustomFieldID(r, true);

                if (orderedResources.Count() > 0)
                {
                    toReturn = new Collection<string>(orderedResources.ToArray());
                }

                return toReturn;
            }
        }

        #region IComparable overrides

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (obj.GetType() == typeof(ExportToProPricerModelView))
            {
                return CompareTo((ExportToProPricerModelView)obj);
            }

            else
            {
                throw new NotImplementedException(
                    String.Format(
                        "Cannot compare to {0} type.",
                        obj.GetType()));
            }
        }

        // Custom comparer to implement desired multi-sort order
        private int CompareTo(ExportToProPricerModelView otherModelView)
        {
            int toReturn = this.Scope.CompareTo(otherModelView.Scope);

            if (toReturn == 0)
            {
                toReturn = this.Name.CompareTo(otherModelView.Name);
            }

            return toReturn;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
        }

        public static bool operator ==(ExportToProPricerModelView first, ExportToProPricerModelView second)
        {
            if (((object)first) == null)
            {
                if (((object)second) == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (((object)second) == null)
            {
                return false;
            }

            return first.CompareTo(second) == 0;
        }

        public static bool operator !=(ExportToProPricerModelView first, ExportToProPricerModelView second)
        {
            if (first == null)
            {
                if (second == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (second == null)
            {
                return true;
            }

            return first.CompareTo(second) != 0;
        }

        public static bool operator <(ExportToProPricerModelView first, ExportToProPricerModelView second)
        {
            if (first == null)
            {
                if (second == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (second == null)
            {
                return false;
            }

            return first.CompareTo(second) < 0;
        }

        public static bool operator >(ExportToProPricerModelView first, ExportToProPricerModelView second)
        {
            if (first == null)
            {
                return false;
            }
            else if (second == null)
            {
                return true;
            }

            return first.CompareTo(second) > 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        #endregion IComparable overrides
    }
}