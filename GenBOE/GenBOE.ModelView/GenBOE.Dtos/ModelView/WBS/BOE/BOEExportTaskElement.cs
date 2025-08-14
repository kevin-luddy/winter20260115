// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using IES.Common;

	[ExcludeFromCodeCoverage]
    //// This class is used within BOEExportModelView to keep track of the different
    // task elements for a BOE 
    public class BOEExportTaskElement
    {
        public BOEExportTaskElement()
            : base()
        {
            BOETaskElementID = 0;
            BOETaskID = string.Empty;
            BOETaskDesc = string.Empty;
            TaskTitle = string.Empty;
            MOQEquation = string.Empty;
            MOQText = string.Empty;
            MOQTotal = string.Empty;
            this.MOQType = string.Empty;
            SourceOfData = string.Empty;
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MinValue;
            OrdinaryVariables = new Collection<OrdinaryVariableDto>();
            WorkspaceVariables = new Collection<WorkspaceVariableDTO>();
            taskElementLabors = new Collection<BOEExportTaskElementLabor>();
            IMS_ID = string.Empty;
            ExportFields = new Dictionary<string, string>();
            this.MOQTypes = new Collection<MoqTypeSelection>();
			SkillMixTable = new Collection<SkillMixModelView>();
			CommonDisclosureTable = new Collection<CommonDisclosureModelView>();
        }

        public void SetTaskElementType(TaskElementType taskElementType)
        {
            switch (taskElementType)
            {
                case TaskElementType.Labor:
                    ElementType = BOEExportTaskElementType.Labor;
                    break;

                default:
                    ElementType = BOEExportTaskElementType.None;
                    break;
            }
        }

        public int BoeID { get; set; }
        public string BOETaskDesc { get; set; }
		public string BOETaskAuthor { get; set; }
        public string BOETaskID { get; set; }
        public int? BOETaskElementID { get; set; }

        /// <summary>
        /// The value of the order in which the task will appear in the boe listing
        /// </summary>
        public int BOETaskElementOrder { get; set; }

        public string TaskTitle { get; set; }
        public string MOQText { get; set; }
        public ICollection<MoqTypeSelection> MOQTypes { get; set; }
        public string SourceOfData { get; set; }
        public Collection<BOEExportTaskElementLabor> taskElementLabors { get; set; }
        public string MOQEquation { get; set; }
        public string MOQTotal { get; set; }
        public string MOQType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Collection<OrdinaryVariableDto> OrdinaryVariables { get; set; }
        public Collection<WorkspaceVariableDTO> WorkspaceVariables { get; set; }
        public BOEExportTaskElementType ElementType { get; set; }
        public string IMS_ID { get; set; }
        public Dictionary<string, string> ExportFields { get; set; }
        public Collection<BOEExportTaskElementMOQVariableModelView> MOQVariableModelViews { get; set; }
        public String POP
        { 
            get
            {
                String toReturn = String.Empty;

                if (this.taskElementLabors != null && this.taskElementLabors.Count > 0)
                {
                    IOrderedEnumerable<BOEExportTaskElementLabor> OrderedStartDateTaskElements = taskElementLabors.OrderBy(x => x.StartDate, SortOrder.Ascending);
                    IOrderedEnumerable<BOEExportTaskElementLabor> OrderedEndDateTaskElements = taskElementLabors.OrderBy(x => x.EndDate, SortOrder.Descending);
                    BOEExportTaskElementLabor Earliest = OrderedStartDateTaskElements.FirstOrDefault(x => x.StartDate.HasValue == true);
                    BOEExportTaskElementLabor Latest = OrderedEndDateTaskElements.FirstOrDefault(x => x.EndDate.HasValue == true);
                    toReturn = Earliest == null && Latest == null ? String.Empty :
                        Earliest == null ? "? - " + Latest.EndDate.Value.ToString("MM/yyyy") : Latest == null ? Earliest.EndDate.Value.ToString("MM/yyyy") + " - ?" :
                        Earliest.StartDate.Value.ToString("MM/yyyy") + " - " + Latest.EndDate.Value.ToString("MM/yyyy");
                }

                return toReturn;
            }
        }

		/// <summary>
		/// Skill Mix table
		/// </summary>
		public ICollection<SkillMixModelView> SkillMixTable { get; set; }

		/// <summary>
		/// Common Disclosure table
		/// </summary>
		public ICollection<CommonDisclosureModelView> CommonDisclosureTable { get; set; }
	}

	public enum BOEExportTaskElementType
    {
        None = 0,
        Labor = 1,
        Travel = 4,
        ODC = 5,
        Material = 6
    }
}
