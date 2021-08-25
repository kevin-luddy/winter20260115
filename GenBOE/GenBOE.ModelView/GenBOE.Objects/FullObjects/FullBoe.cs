// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;

    [Serializable()]
    public class FullBoe : BoeDTO, IDateShiftable
    {
        [NonSerialized]
        private IRetriever retriever;

        [NonSerialized]
        private IFullObjectFactory factory;

        private FullWbs wbs;
        private FullClin clin;
        private FullWorkspace workspace;
        private ReadOnlyCollection<WorkspaceVariableDTO> workspaceVariables;
        private ReadOnlyCollection<int> workspaceVariablesIdsForBoe;
        private ReadOnlyCollection<MaterialDTO> materials;
        private int? materialCount;
        private ReadOnlyCollection<ResourceTypeDto> laborTypes;
        private ReadOnlyCollection<BoeTaskElementDTO> taskElements;
        private ReadOnlyCollection<int> taskVariableIds;
        private ReadOnlyCollection<OtherDirectCostDTO> otherDirectCosts;
        private ReadOnlyCollection<TravelDTO> travels;
        private ReadOnlyCollection<BoeApproverResponseDTO> approverResponses;
        private ReadOnlyCollection<BOECommentDTO> comments;
        private ReadOnlyCollection<MoqTypeSelection> moqTypeSelections;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="boeId">Boe Id</param>
        internal FullBoe() : base()
        {
            this.retriever = GenBOEUnityContainer.Resolve<IRetriever>();
            this.factory = GenBOEUnityContainer.Resolve<IFullObjectFactory>();
            this.TemplateQuestionsAndAnswers = new Collection<RTECustomTemplateQuestionAnswerModelView>();
        }

        /// <summary>
        /// Constructor based on the dto
        /// </summary>
        /// <param name="boe">Boe Dto</param>
        internal FullBoe(BoeDTO boe) : this()
        {
            if (boe != null)
            {
                foreach (PropertyInfo prop in typeof(BoeDTO).GetProperties())
                {
                    // if the Boe's RTE fields were not loaded yet, we do not copy them
                    bool skipPropertyCopy = (prop.Name == "Description" && !boe.WasDescriptionSet) 
                                         || (prop.Name == "DataSource" && !boe.WasDataSourceSet);

                    if (!skipPropertyCopy && prop.CanRead && prop.CanWrite)
                    { 
                        this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(boe, null), null);
                    }
                }
            }
        }

        /// <summary>
        /// Populates RTE data for all Boes
        /// </summary>
        public void LoadBOEsRTEData()
        {
            this.retriever.PopulateRTEData(new List<FullBoe>() { this });
        }

        /// <summary>
        /// Boe's Clin
        /// </summary>
        public FullClin Clin
        {
            get
            {
                if (this.clin == null)
                {
                    if (this.CLINID.HasValue)
                    {
                        ClinDTO clinDto = this.retriever.GetClinById(this.CLINID.Value);
                        this.clin = this.factory.CreateFullClin(clinDto);
                    }
                }

                return this.clin;
            }
        }

        /// <summary>
        /// Boe's Comments
        /// </summary>
        public IReadOnlyCollection<BOECommentDTO> Comments
        {
            get
            {
                if (this.comments == null)
                {
                    this.comments = this.retriever.GetCommentsByBoeId(this.Id).ToList().AsReadOnly();
                }

                return this.comments;
            }
        }

        /// <summary>
        /// Boe's Wbs
        /// </summary>
        public FullWbs Wbs
        {
            get
            {
                if (this.wbs == null)
                {
                    if (this.WBSID.HasValue)
                    {
                        this.wbs = this.factory.CreateFullWbs(this.retriever.GetWbsById(this.WBSID.Value));
                    }
                }

                return this.wbs;
            }
        }

        /// <summary>
        /// Workspace for the BOE
        /// </summary>
        public FullWorkspace Workspace
        {
            get
            {
                if (this.workspace == null)
                {
                    this.workspace = retriever.GetFullWorkspaceById(this.WorkspaceID);
                }

                return this.workspace;
            }

            // Added this so it can be manually set, avoiding the need to load it from the DB
            set
            {
                this.workspace = value;
            }
        }

        /// <summary>
        /// All workspace variables belonging to the workspace.
        /// </summary>
        public IReadOnlyCollection<WorkspaceVariableDTO> WorkspaceVariables
        {
            get
            {
                if (this.workspaceVariables == null)
                {
                    this.workspaceVariables = this.retriever.GetWorkspaceVariableDTOsByWorkspaceId(this.WorkspaceID).ToList().AsReadOnly();
                }

                return this.workspaceVariables;
            }
        }

        /// <summary>
        /// All workspace variable Ids where the BOE is part of the sum of BOEs for the variable.
        /// </summary>
        public IReadOnlyCollection<int> WorkspaceVariablesIdsForBoe
        {
            get
            {
                if (this.workspaceVariablesIdsForBoe == null)
                {
                    this.workspaceVariablesIdsForBoe = this.retriever.GetWorkspaceVariableIdsForBoe(this.Id).ToList().AsReadOnly();
                }

                return this.workspaceVariablesIdsForBoe;
            }
        }

        /// <summary>
        /// Gets all Materials for the BOE.
        /// </summary>
        public IReadOnlyCollection<MaterialDTO> Materials
        {
            get
            {
                if (this.materials == null)
                {
                    this.materials = this.retriever.GetMaterialCollectionByBoeID(this.Id, false).ToList().AsReadOnly();
                }

                return this.materials;
            }
        }

        /// <summary>
        /// Gets the number of materials for the BOE
        /// </summary>
        public int MaterialCount
        {
            get
            {
                if (this.materialCount == null)
                {
                    this.materialCount = this.retriever.GetNumberOfMaterialsForBoeId(this.Id);
                }

                return this.materialCount.Value;
            }
        }

        /// <summary>
        /// Gets all Other Direct Costs for the BOE.
        /// </summary>
        public IReadOnlyCollection<OtherDirectCostDTO> OtherDirectCosts
        {
            get
            {
                if (this.otherDirectCosts == null)
                {
                    this.otherDirectCosts = this.retriever.GetOdcCollectionByBoeIds(new List<int>() { this.Id }, false).ToList().AsReadOnly();
                }

                return this.otherDirectCosts;
            }
        }

        /// <summary>
        /// Populates RTE data for ODCs
        /// </summary>
        public void LoadODCsRTEData()
        {
            if (this.otherDirectCosts == null)
            {
                // Data has not been pulled yet, so we can do a full retrieval, including the RTE data
                this.otherDirectCosts = this.retriever.GetOdcCollectionByBoeIds(new List<int>() { this.Id }, true).ToList().AsReadOnly();
            }
            else
            {
                // Data was already retrieved, so we are only missing the RTE data, which we'll now load
                this.retriever.PopulateRTEData(this.otherDirectCosts);
            }
        }

        /// <summary>
        /// Labor Types
        /// </summary>
        public IReadOnlyCollection<ResourceTypeDto> LaborTypes
        {
            get
            {
                if (this.laborTypes == null)
                {
                    this.laborTypes = this.retriever.GetLaborTypesTypesForBoeId(this.Id).ToList().AsReadOnly();
                }

                return this.laborTypes;
            }
        }

        /// <summary>
        /// Gets a list of task elements associated with the Boe.
        /// </summary>
        public IReadOnlyCollection<BoeTaskElementDTO> TaskElements
        {
            get
            {
                if (this.taskElements == null)
                {
                    this.taskElements = this.retriever.GetBoeTaskElementCollectionByBoeId(this.Id, false, this.Workspace.DecimalPrecision, this.Workspace.CostDecimalPrecision).ToList().AsReadOnly();
                }

                return taskElements;
            }
        }

        /// <summary>
        /// Populates RTE data for Task Elements
        /// </summary>
        public void LoadTaskElementRTEData()
        {
            if (this.taskElements == null)
            {
                // Data has not been pulled yet, so we can do a full retrieval, including the RTE data
                this.taskElements = this.retriever.GetBoeTaskElementCollectionByBoeId(this.Id, true, this.Workspace.DecimalPrecision, this.Workspace.CostDecimalPrecision).ToList().AsReadOnly();
            }
            else
            {
                // Data was already retrieved, so we are only missing the RTE data, which we'll now load
                this.retriever.PopulateRTEData(this.taskElements);
            }
        }

        /// <summary>
        /// Gets a collection of Travels elements associated with the Boe.
        /// </summary>
        public IReadOnlyCollection<TravelDTO> Travels
        {
            get
            {
                if (this.travels == null)
                {
                    this.travels = this.retriever.GetTravelCollectionByBoeID(this.Id, false).ToList().AsReadOnly();
                }

                return travels;
            }
        }

        /// <summary>
        /// Populates RTE data for Travels
        /// </summary>
        public void LoadTravelRTEData()
        {
            if (this.travels == null)
            {
                // Data has not been pulled yet, so we can do a full retrieval, including the RTE data
                this.travels = this.retriever.GetTravelCollectionByBoeID(this.Id, true).ToList().AsReadOnly();
            }
            else
            {
                // Data was already retrieved, so we are only missing the RTE data, which we'll now load
                this.retriever.PopulateRTEData(this.travels);
            }
        }

        /// <summary>
        /// Gets a list of approver responses associated with a Boe.
        /// </summary>
        /// <param name="boeId">Boe Id.</param>
        /// <returns>Approver responses associated to a Boe.</returns>
        public IReadOnlyCollection<BoeApproverResponseDTO> ApproverResponses
        {
            get
            {
                if (this.approverResponses == null)
                {
                    this.approverResponses = this.retriever.GetApproverResponseCollectionByBoeId(this.Id).ToList().AsReadOnly();
                }
                return this.approverResponses;
            }
        }

        /// <summary>
        /// Task Variable Ids
        /// </summary>
        public IReadOnlyCollection<int> TaskVariableIds
        {
            get
            {
                if (this.taskVariableIds == null)
                {
                    this.taskVariableIds = this.retriever.GetTaskVariableIdsByBoeId(this.Id).ToList().AsReadOnly();
                }

                return this.taskVariableIds;
            }
        }

        /// <summary>
        /// Does Boe contain Discrete Labor Types
        /// </summary>
        public bool ContainsDiscreteLaborTypes
        {
            get
            {
                return this.LaborTypes.Any(x => x.SpreadCurveID == SpreadCurves.DiscreteHours);
            }
        }

        /// <summary>
        /// Does Boe Exist given Custom Field Id
        /// </summary>
        /// <param name="customFieldId">Custom Field Id</param>
        public bool DoesBoeExistGivenCustomFieldId(int customFieldId)
        {
            return this.retriever.CheckIfBoeExistsGivenCustomFieldID(this.Id, customFieldId);
        }

        /// <summary>
        /// Does Boe Contain Labor Cost Elements
        /// </summary>
        public bool ContainsLaborCostElements
        {
            get
            {
                return this.retriever.CheckIfBoeContainsLaborCostElements(this.Id);
            }
        }

        /// <summary>
        /// Does Boe Contain Material Elements
        /// </summary>
        public bool ContainsMaterialElements
        {
            get
            {
                return this.retriever.CheckIfBoeContainsMaterialElements(this.Id);
            }
        }

        /// <summary>
        /// Get all custom field values assigned to the given BOE.
        /// </summary>
        /// <returns>Dictionary of custom field values to custom field dto for the BOE.</returns>
        public IDictionary<CustomFieldValueDTO, CustomFieldDTO> AssignedBOECustomFieldValues
        {
            get
            {
                return this.retriever.GetAllAssignedBOECustomFieldValuesForBOE(this.Id);
            }
        }

        /// <summary>
        /// Gets the children that can be shifted.
        /// </summary>
        public ICollection<IDateShiftable> Children
        {
            get
            {
                List<IDateShiftable> children = new List<IDateShiftable>();
                if (this.TaskElements.Any())
                {
                    children.AddRange(this.TaskElements);
                }

                if (this.Travels.Any())
                {
                    children.AddRange(this.Travels);
                }

                return children;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a spread of values.
        /// </summary>
        public bool HasSpread
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the date shift level.
        /// </summary>
        public Level DateShiftLevel
        {
            get
            {
                return Level.BOE;
            }
        }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        DateTime? IDateShiftable.StartDate
        {
            get
            {
                return this.StartDate;
            }

            set
            {
                this.StartDate = value ?? DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        DateTime? IDateShiftable.EndDate
        {
            get
            {
                return this.EndDate;
            }

            set
            {
                this.EndDate = value ?? DateTime.MinValue;
            }
        }

        /// <summary>
        /// Overrides the task elements
        /// </summary>
        /// <param name="tasks">Task Elements</param>
        public void SetTaskElements(ICollection<BoeTaskElementDTO> tasks)
        {
            this.taskElements = tasks.Where(t => t.BoeID == this.Id).ToList().AsReadOnly();
        }

        /// <summary>
        /// Filters the input collection to only those travels with the same boe ID.
        /// </summary>
        /// <param name="travels">The travels.</param>
        public void SetTravels(ICollection<TravelDTO> travelDtos)
        {
            this.travels = travelDtos.Where(t => t.BoeID == this.Id).ToList().AsReadOnly();
        }

        /// <summary>
        /// Updates the workspace variables.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public void UpdateWorkspaceVariables(ICollection<WorkspaceVariableDTO> variables)
        {
            this.workspaceVariables = variables.ToList().AsReadOnly();
        }

        /// <summary>
        /// Template Questions & Answers
        /// </summary>
        public ICollection<RTECustomTemplateQuestionAnswerModelView> TemplateQuestionsAndAnswers { get; set; }

        /// <summary>
        /// MOQ Type Selections for the BOE
        /// </summary>
        public IReadOnlyCollection<MoqTypeSelection> MoqTypeSelections
        {
            get
            {
                if (this.moqTypeSelections == null)
                {
                    this.moqTypeSelections = this.retriever.GetMoqTypeSelectionsByBoeId(this.Id).ToList().AsReadOnly();
                }

                return this.moqTypeSelections;
            }
        }
    }
}
