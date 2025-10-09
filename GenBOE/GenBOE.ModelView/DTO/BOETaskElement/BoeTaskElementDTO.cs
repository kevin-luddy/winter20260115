// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
	using GenBOE.Dtos;
	using IES.Common;
    using IES.Common.Interfaces;
	using Newtonsoft.Json;

	/// <summary>
	/// This is the task element data associated with the BOE DTO
	/// </summary>
	[ExcludeFromCodeCoverage]
    [Serializable()]
    public class BoeTaskElementDTO : UpdateableDTO, IBOEMembership, IStartEndDates, ICachableDTO, IDateShiftable
    {
		[JsonIgnore]
        [NonSerialized]
        private static BoeTaskElementDTODataLoader loader;

        public BoeTaskElementDTO() : base()
        {
            this.Id = -1;
            this.BOETaskID = string.Empty;
            this.TaskTitle = string.Empty;
            this.MOQType = MOQType.None;
            this.MOQTypeName = string.Empty;
            this.MOQHoursEquation = string.Empty;
            this.IMS_ID = string.Empty;
            this.StartDate = DateTime.MinValue;
            this.EndDate = DateTime.MinValue;
            this.OrdinaryVariables = new Collection<OrdinaryVariableDto>();
            this.WorkspaceVariableIDs = new Collection<int>();
            this.taskElementLabors = new Collection<ResourceTypeDto>();
            this.LaborTypeWarningFlag = false;
            this.CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();
            this.TaskElementType = TaskElementType.Labor;

            if (loader == null) { loader = new BoeTaskElementDTODataLoader(new ResourceTypeLoader(), new ResourceSpreadLoader(), new OrdinaryVariableLoader(),
                                                    new BoeTaskElementCustomFieldValueXREFLoader(), new LaborTypeCustomFieldValueXREFLoader(), new SkillMixDTOLoader(), new CommonDisclosureSMDTODataLoader()); }
            this.description = null;
            this.moqText = null;
            this.WasMoqTextSet = false;
            this.WasDescriptionSet = false;
            this.preventRteDbLoad = false;
			this.AuthorUserId = null;
			this.AuthorDisplayName = string.Empty;
        }

        #region RTE Fields

        private string description;
        private string moqText;

        /// <summary>
        /// Indicates whether the Description field was set (either from user, or via RTE load)
        /// </summary>
        public bool WasDescriptionSet { get; set; }

        /// <summary>
        /// Indicates whether the Data Source field was set (either from user, or via RTE load)
        /// </summary>
        public bool WasMoqTextSet { get; set; }

        /// <summary>
        /// This is used to mark when we make the DB call, to prevent all subsequent calls, as RTE fields were loaded already
        /// </summary>
        private bool preventRteDbLoad { get; set; }

        // the description
        public string Description
        {
            get
            {
                if (this.Id > 0 && !this.WasDescriptionSet && !this.preventRteDbLoad)
                {
                    this.preventRteDbLoad = true;
                    loader.LoadRTEFields(new List<BoeTaskElementDTO>() { this });
                }

                return this.description;
            }
            set
            {
                this.description = value;
                this.WasDescriptionSet = true;
            }
        }

        // The Moq Text
        public string MOQText
        {
            get
            {
                if (this.Id > 0 && !this.WasMoqTextSet && !this.preventRteDbLoad)
                {
                    this.preventRteDbLoad = true;
                    loader.LoadRTEFields(new List<BoeTaskElementDTO>() { this });
                }

                return this.moqText;
            }
            set
            {
                this.moqText = value;
                this.WasMoqTextSet = true;
            }
        }

        /// <summary>
        /// Loads the rte fields.
        /// </summary>
        public void LoadRTEFields()
        {
            if (!this.preventRteDbLoad)
            {
                this.preventRteDbLoad = true;
                loader.LoadRTEFields(new List<BoeTaskElementDTO>() { this });
            }
        }

        #endregion

        /// <summary>
        /// The TaskID string field from User Input
        /// </summary>
        public string BOETaskID { get; set; }

        /// <summary>
        /// The value of the order in which the task will appear in the boe listing
        /// </summary>
        public int BOETaskElementOrder { get; set; }

        // the task title
        public string TaskTitle { get; set; }

        // The MOQ Type
        public string MOQTypeName { get; set; }

		// The Total Relevant Hours
		public decimal MOQTotalRelevantHours { get; set; }

        // the labor types associated with a task element
        public Collection<ResourceTypeDto> taskElementLabors { get; set; }

        // the MOQ Hours Equation
        public string MOQHoursEquation { get; set; }

        // the task start date
        public DateTime? StartDate { get; set; }

        // the task end date
        public DateTime? EndDate { get; set; }

        // the ordinary variables within the MOQ equation
        public Collection<OrdinaryVariableDto> OrdinaryVariables { get; set; }

		// the workspace variable IDs currently associated with the MOQ equation
        public Collection<int> WorkspaceVariableIDs { get; set; }

        // the MOQ Type ID
        public MOQType MOQType { get; set; }
        
        // the Total Hours
        public decimal? TotalHours { get; set; }

		// Total UCOT Hours
		public decimal? UCOTHours { get; set; }

		// Total Hours and UCOT Hours combined
		public decimal? TotalHoursWithUCOT { get; set; }

        // The total cost
        public decimal? TotalCost { get; set; }

		// Boolean to control whether UCOT is enabled for specific task
		public bool IsUCOTEnabledForTask { get; set; }

        public int BoeID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        public bool? LaborTypeWarningFlag { get; set; }

        public string IMS_ID { get; set; }

		//BOE Task Element Custom Fields
        public Collection<CustomFieldValueContainer> CustomFieldValueContainers { get; set; }

        // the task element type
        public TaskElementType TaskElementType { get; set; }


		/// <summary>
		/// Skill Mix table
		/// </summary>
		public ICollection<SkillMixModelView> SkillMixTable { get; set; } = new List<SkillMixModelView>();

		/// <summary>
		/// Common Disclosure table
		/// </summary>
		public ICollection<CommonDisclosureModelView> CommonDisclosureTable { get; set; } = new List<CommonDisclosureModelView>();

		/// <summary>
		/// Gets or sets the Author User Id, referenced from the ETIUser table
		/// </summary>
		public int? AuthorUserId { get; set; }

		/// <summary>
		/// Gets or sets the Author display name
		/// </summary>
		public string AuthorDisplayName { get; set; }

		/// <summary>
		/// Gets the children that can be shifted.
		/// </summary>
		public ICollection<IDateShiftable> Children
        {
            get
            {
                return new List<IDateShiftable>(this.taskElementLabors);
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
                return Level.Task;
            }
        }

        /// <summary>
        /// For ICacheable -> to return PK
        /// </summary>
        /// <returns>Primary Key</returns>
        public int GetPrimaryKeyID()
        {
            return this.Id;
        }

        #region Project Map Fields

        /// <summary>
        /// Gets or sets the sow.
        /// </summary>
        public string SOW { get; set; }

        /// <summary>
        /// Gets or sets the sow title.
        /// </summary>
        public string SOWTitle { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the name of the cam.
        /// </summary>
        public string CamName { get; set; }

        /// <summary>
        /// Gets or sets the class of cost.
        /// </summary>
        public ClassOfCost ClassOfCost { get; set; }

        #endregion Project Map Fields


        #region UpdateableDTO

        /// <summary>
        /// Propagates the new 'parent' DTO Id to all first level 'child' DTOs in collections.
        /// </summary>
        /// <param name="newParentId">new id of the parent DTO</param>
        override protected void PropagateNewParentIdToChildDTOs(int newParentId)
        {
            //// now update the child DTOs in each collection on this element
            if (this.OrdinaryVariables != null && this.OrdinaryVariables.Any())
            {
                foreach (OrdinaryVariableDto ordinaryVarDto in this.OrdinaryVariables)
                {
                    ordinaryVarDto.TaskElementId = newParentId;
                }
            }

            if (this.taskElementLabors != null && this.taskElementLabors.Any())
            {
                foreach (ResourceTypeDto resourceTypeDto in this.taskElementLabors)
                {
                    resourceTypeDto.TaskElementId = newParentId;
                }
            }

            if (this.CustomFieldValueContainers != null && this.CustomFieldValueContainers.Any())
            {
                foreach (CustomFieldValueContainer containerDto in this.CustomFieldValueContainers)
                {
                    containerDto.OwnerID = newParentId;
                }
            }
        }

        #endregion
    }
}


