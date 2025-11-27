// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// A Search Result returning all BOE information
    /// </summary>
    public class BOESearchResult
    {
        private Collection<TaskElementModelView> taskElements;

        /// <summary>
        /// Gets or sets the WBS number.
        /// </summary>
        public string WBSNumber { get; set; }

        /// <summary>
        /// Gets or sets the WBS title.
        /// </summary>
        public string WBSTitle { get; set; }

        /// <summary>
        /// Gets or sets the clin number.
        /// </summary>
        public string ClinNumber { get; set; }

        /// <summary>
        /// Gets or sets the clin title.
        /// </summary>
        public string ClinTitle { get; set; }

        /// <summary>
        /// Gets or sets the name of the workspace.
        /// </summary>
        public string WorkspaceName { get; set; }

        /// <summary>
        /// Gets or sets the short name of the workspace.
        /// </summary>
        public string WorkspaceShortName { get; set; }

        /// <summary>
        /// Gets or sets the workspace identifier.
        /// </summary>
        public int WorkspaceId { get; set; }

        /// <summary>
        /// Gets or sets the submittal date.
        /// </summary>
        public DateTime? SubmittalDate { get; set; }

        /// <summary>
        /// Gets or sets the boe title.
        /// </summary>
        public string BOETitle { get; set; }

        /// <summary>
        /// Gets or sets the boe description.
        /// </summary>
        public string BOEDescription { get; set; }

        /// <summary>
        /// Gets or sets the display name of the author.
        /// </summary>
       public string AuthorDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the boeid.
        /// </summary>
        public int BOEID { get; set; }

        /// <summary>
        /// When true, all task elements are selected to be copied.
        /// </summary>
        public bool IsCopyAllTaskElementsSelected { get; set; }

        /// <summary>
        /// Gets or sets candidate task elements to copy.
        /// </summary>
        public Collection<TaskElementModelView> TaskElements
        {
            get
            {
                if (taskElements == null)
                {
                    taskElements = new Collection<TaskElementModelView>();
                }
                return taskElements;
            }
            set
            {
                taskElements = value == null ? new Collection<TaskElementModelView>() : taskElements = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the cam.
        /// </summary>
        public string CamName { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the project map identifier.
        /// </summary>
        public int ProjectMapId { get; set; }

        /// <summary>
        /// Gets or sets the project map task.
        /// </summary>
        public string ProjectMapTask { get; set; }
    }

    /// <summary>
    /// Represents a task element to be copied.
    /// </summary>
    public class TaskElementModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskElementModelView"/> class.
        /// </summary>
        public TaskElementModelView()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskElementModelView"/> class.
        /// </summary>
        /// <param name="boeTaskElementDTO">The boe task element dto.</param>
        /// <param name="moqEquationResult">The moq equation result.</param>
        /// <exception cref="System.ArgumentNullException">boeTaskElementDTO</exception>
        public TaskElementModelView(BoeTaskElementDTO boeTaskElementDTO, string moqEquationResult)
        {
            if (boeTaskElementDTO == null)
            {
                throw new ArgumentNullException(nameof(boeTaskElementDTO));
            }
            this.IsCopySelected = true;
            this.Id = boeTaskElementDTO.Id;
            this.MoqEquation = boeTaskElementDTO.MOQHoursEquation;
            this.TaskTitle = boeTaskElementDTO.TaskTitle;
            this.MoqResult = moqEquationResult;
        }

        /// <summary>
        /// When true, the checkbox to copy is selected.
        /// </summary>
        public bool IsCopySelected { get; set; }

        /// <summary>
        /// The Id of the task
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Task Title.
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// Task Element MOQ Equation plus the values of the MOQ variables.
        /// </summary>
        public string MoqEquation { get; set; }

        /// <summary>
        /// Result of the Task Element MOQ Equation
        /// </summary>
        public string MoqResult { get; set; }
    }
}
