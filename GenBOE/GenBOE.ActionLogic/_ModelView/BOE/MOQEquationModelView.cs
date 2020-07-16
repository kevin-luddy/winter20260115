// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;

    /// <summary>
    /// Model view for the MOQEquationField partial view.  It contains the MOQ equation and its variables.
    /// </summary>
    public class MOQEquationModelView
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MOQEquationModelView()
        {
            this.MOQHoursEquation = string.Empty;
            this.MOQCostEquation = string.Empty;
            this.TaskOrdinaryVariables = new Collection<BoeTaskOrdinaryVariableModelView>();
            this.WorkspaceVariableIDs = new Collection<int>();
            this.TypeOfMoqEquation = MOQEquationType.Hours;
            this.MoqEquationName = string.Empty;
            this.TaskElementId = 0;
            this.MOQType = MOQType.None;
            this.MOQText = string.Empty;
            this.PMMetricsUsed = new Collection<MSTMetricDetailsDTO>();
            this.Company = SystemConfiguration.Instance().CompanyMode;
            this.MOQTextLabel = String.Empty;
            this.HelpText = String.Empty;
            this.MoqTemplateAnswers = new List<RTECustomTemplateQuestionAnswerModelView>();
        }

        /// <summary>
        /// Alternate constructor.
        /// </summary>
        /// <param name="inBoeTaskElement">Task element used to compose the model view.</param>
        /// <param name="inVariableSelectBOEtoSumCalculation">VariableSelectBOEtoSumCalculation</param>
        /// <param name="workspace">Full workspace object.</param>
        public MOQEquationModelView(BoeTaskElementDTO inBoeTaskElement, IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation, FullWorkspace workspace)
            : this()
        {
            if (inBoeTaskElement != null)
            {
                this.MOQHoursEquation = inBoeTaskElement.MOQHoursEquation;
                this.WorkspaceVariableIDs = inBoeTaskElement.WorkspaceVariableIDs;
                this.TaskElementId = inBoeTaskElement.Id;
                this.MOQType = inBoeTaskElement.MOQType;
                this.MOQText = inBoeTaskElement.MOQText;

                // Add each ordinary variable as a variable model view object
                foreach (OrdinaryVariableDto boeTaskOrdinaryVariable in inBoeTaskElement.OrdinaryVariables)
                {
                    this.TaskOrdinaryVariables.Add(new BoeTaskOrdinaryVariableModelView(boeTaskOrdinaryVariable, inVariableSelectBOEtoSumCalculation, workspace));
                }
            }
            else
            {
                this.TaskElementId = 0;
            }
        }

        /// <summary>
        /// The MOQ Equation. This regular expression represents characters NOT allowed in MOQ Equation. x22 represents "
        /// </summary>
        [RegularExpression(@"^[^!#&:;<=>?@\'{|}~[\\\]\x22]{1,250}$", ErrorMessage = "An invalid character was entered.")]
        [StringLength(250, ErrorMessage = "A maximum of 250 characters are allowed for the MOQ Equation.")]
        public string MOQEquation
        {
            get
            {
                if (this.TypeOfMoqEquation == MOQEquationType.Hours)
                {
                    return this.MOQHoursEquation;
                }
                else
                {
                    return this.MOQCostEquation;
                }
            }
        }

        /// <summary>
        /// Gets or sets MOQ Hours Equation
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public string MOQHoursEquation { private get; set; }

        /// <summary>
        /// Gets or sets MOQ Cost Equation
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public string MOQCostEquation { private get; set; }

        /// <summary>
        /// Ordinary variables used in the MOQ equation.
        /// </summary>
        public Collection<BoeTaskOrdinaryVariableModelView> TaskOrdinaryVariables { get; set; }

        /// <summary>
        /// Workspace variables used in the MOQ equation.
        /// </summary>
        public Collection<int> WorkspaceVariableIDs { get; set; }

        /// <summary>
        /// Hours or Cost.
        /// </summary>
        public MOQEquationType TypeOfMoqEquation { get; set; }

        /// <summary>
        /// This is for naming the specific moq equation on a page where multiple equations exist.
        /// </summary>
        public string MoqEquationName { get; set; }

        /// <summary>
        /// Task element containing the MOQ equation.
        /// </summary>
        public int TaskElementId { get; set; }

        /// <summary>
        /// Gets or sets MOQ Type.
        /// </summary>
        public MOQType MOQType { get; set; }

        /// <summary>
        /// Gets or sets MOQ Text.
        /// </summary>
        [RichText(RichTextDbColumn.BOE_TASK_ELEMENT_MOQ_TEXT, "TaskElementId")]
        public string MOQText { get; set; }

        /// <summary>
        /// Historical Metrics from MST PMM.
        /// </summary>
        public ICollection<MSTMetricDetailsDTO> PMMetricsUsed { get; set; }

        /// <summary>
        /// When true shows the "Search Estimating Catalog" link to insert historical metrics into task element.
        /// </summary>
        public bool ShowSearchMetricsLink { get; set; }

        #region ISGS Versus SSC terminology
        
        /// <summary>
        /// Indicates if the application is running as IS&amp;GS, Space or MST. 
        /// </summary>
        public CompanyConfiguration Company { get; set; }

        /// <summary>
        /// Label to be used for the MOQ Equation field.
        /// </summary>
        public string MOQEquationLabel
        {
            get
            {
                return CommonConstants.BOE_MOQ_EQUATION_LABEL;
            }
        }

        /// <summary>
        /// Label used for MOQ Label field.
        /// </summary>
        public string MOQTextLabel { get; set; }
        public string HelpText { get; set; }
        #endregion 

        /// <summary>
        /// Gets or sets the RTE Custom Template Answers at Task level.
        /// </summary>
        public ICollection<RTECustomTemplateQuestionAnswerModelView> MoqTemplateAnswers { get; set; }
        
    }

    public enum MOQEquationType
    {
        Hours= 0,
        Cost = 1
    }
}