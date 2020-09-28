// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common.Exceptions;

    /// <summary>
    /// 
    /// </summary>
    public class LaborTaskDataModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LaborTaskDataModelView"/> class.
        /// </summary>
        public LaborTaskDataModelView()
        {
            this.LaborTypesData = new List<LaborTypeDataModelView>();
            this.ValidationErrors = new List<ValidationMessage>();
            this.TaskCustomFields = new List<BOECustomFieldModelView>();
            this.LaborCustomFields = new List<BOECustomFieldModelView>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this Task contains any discrete spreads (this determines whether to expand or collapse spreads module by default).
        /// </summary>
        public bool ContainsDiscrete { get; set; }

        /// <summary>
        /// Gets or sets the task element data.
        /// </summary>
        public TaskElementDetailModelView TaskElementData { get; set; }

        /// <summary>
        /// Gets or sets the labor types data.
        /// </summary>
        public ICollection<LaborTypeDataModelView> LaborTypesData { get; set; }

        /// <summary>
        /// Gets or sets the validation errors.
        /// </summary>
        public ICollection<ValidationMessage> ValidationErrors { get; set; }
        
        /// <summary>
        /// Gets the overall task custom fields.
        /// </summary>
        public ICollection<BOECustomFieldModelView> TaskCustomFields { get; internal set; }
        
        /// <summary>
        /// Gets the overall labor custom fields.
        /// </summary>
        public ICollection<BOECustomFieldModelView> LaborCustomFields { get; internal set; }
        
        /// <summary>
        /// Gets or sets the Adjacent Tasks.
        /// </summary>
        public AdjacentItems AdjacentItems { get; set; }

        /// <summary>
        /// Selected MOQ Types
        /// </summary>
        public ICollection<MoqTypeSelection> MOQTypes { get; set; }
    }
}
