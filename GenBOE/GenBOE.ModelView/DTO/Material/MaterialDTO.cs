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
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// Material Task Element
    /// </summary>
    [Serializable()]
    public class MaterialDTO : UpdateableDTO, IBOEMembership
    {
        [NonSerialized]
        private static MaterialDTODataLoader loader;

        public MaterialDTO()
        {
            this.Id = -1;
            this.TaskID = string.Empty;
            this.TaskTitle = string.Empty;
            this.StartDate = null;
            this.EndDate = null;

            if (loader == null) { loader = new MaterialDTODataLoader(); }
            this.taskDescription = null;
            this.moqText = null;
            this.WasMoqTextSet = false;
            this.WasDescriptionSet = false;
            this.preventRteDbLoad = false;
        }

        public string TaskID { get; set; }
        public string TaskTitle { get; set; }

        #region RTE Fields

        private string taskDescription;
        private string moqText;

        /// <summary>
        /// Indicates whether the Description field was set (either from user, or via RTE load)
        /// </summary>
        public bool WasDescriptionSet { get; set; }

        /// <summary>
        /// Indicates whether the MoqText field was set (either from user, or via RTE load)
        /// </summary>
        public bool WasMoqTextSet { get; set; }

        /// <summary>
        /// This is used to mark when we make the DB call, to prevent all subsequent calls, as RTE fields were loaded already
        /// </summary>
        private bool preventRteDbLoad { get; set; }

        // the Description
        public string TaskDescription
        {
            get
            {
                if (this.Id > 0 && !this.WasDescriptionSet && !this.preventRteDbLoad)
                {
                    this.preventRteDbLoad = true;
                    loader.LoadRTEFields(new List<MaterialDTO>() { this });
                }

                return this.taskDescription;
            }
            set
            {
                this.taskDescription = value;
                this.WasDescriptionSet = true;
            }
        }

        // The Moq Text
        public string MoqText
        {
            get
            {
                if (this.Id > 0 && !this.WasMoqTextSet && !this.preventRteDbLoad)
                {
                    this.preventRteDbLoad = true;
                    loader.LoadRTEFields(new List<MaterialDTO>() { this });
                }

                return this.moqText;
            }
            set
            {
                this.moqText = value;
                this.WasMoqTextSet = true;
            }
        }

        #endregion
        
        public int BoeID { get; set; }

        // the task start date, the earliest Material Type Expend Date
        public DateTime? StartDate { get; set; }

        // the task end date, the latest Material Type Expend Date
        public DateTime? EndDate { get; set; }

        #region Required Validation Messages
        public const string MOQ_TEXT_REQUIRED = "{0} is required.";
        public const string RESOURCE_TYPE_FOR_TASK_ELEMENT_REQUIRED = "At least one material is required for a task element.";
        public const string MATERIAL_TYPE_EXPEND_DATE_INVALID = "Expend Date must be between {0} and {1}.";
        #endregion Required Validation Messages
    }
}
