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
    using IES.Standard;

    [Serializable()]
    public class OtherDirectCostDTO : UpdateableDTO, IBOEMembership, IStartEndDates
    {
        [NonSerialized]
        private static OtherDirectCostDTODataLoader loader;

        public OtherDirectCostDTO()
        {
            this.Id = -1;
            this.TaskID = string.Empty;
            this.TaskTitle = string.Empty;
            this.ODCTypes = new Collection<OtherDirectCostType>();
            this.StartDate = DateTime.MinValue;
            this.EndDate = DateTime.MinValue;

            // TODO TIW if (loader == null) { loader = new OtherDirectCostDTODataLoader(); }
            this.taskDescription = null;
            this.moqText = null;
            this.WasMoqTextSet = false;
            this.WasDescriptionSet = false;
            this.preventRteDbLoad = false;
        }

        public string TaskID { get; set; }

        /// <summary>
        /// The TaskID string field from User Input
        /// </summary>
        public string BOETaskID { get; set; }

        /// <summary>
        /// The value of the order in which the task will appear in the boe listing
        /// </summary>
        public int BOETaskElementOrder { get; set; }

        public string TaskTitle { get; set; }
        
        #region RTE Fields

        private string taskDescription;
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

        // the BOE description
        public string TaskDescription
        {
            get
            {
                if (this.Id > 0 && !this.WasDescriptionSet && !this.preventRteDbLoad)
                {
                    this.preventRteDbLoad = true;
                    loader.LoadRTEFields(new List<OtherDirectCostDTO>() { this });
                }

                return this.taskDescription;
            }
            set
            {
                this.taskDescription = value;
                this.WasDescriptionSet = true;
            }
        }

        // The data source
        public string MoqText
        {
            get
            {
                if (this.Id > 0 && !this.WasMoqTextSet && !this.preventRteDbLoad)
                {
                    this.preventRteDbLoad = true;
                    loader.LoadRTEFields(new List<OtherDirectCostDTO>() { this });
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

        /// <summary>
        /// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
        /// </summary>
        internal IEnumerable<OtherDirectCostType> ODCTypesIEnum { get; set; }

        public Collection<OtherDirectCostType> ODCTypes { get; set; }

        public int BoeID { get; set; }

        // the task start date, the earliest ODC Type start date
        public DateTime? StartDate { get; set; }

        // the task end date, the latest ODC Type end date
        public DateTime? EndDate { get; set; }

        #region Required Validation Messages
        public const string MOQ_TEXT_REQUIRED = "{0} is required.";
        public const string RESOURCE_CODE_REQUIRED = "Resource is required.";
        public const string PERFORM_ORG_REQUIRED = "Performing Org is required.";
        public const string SPREAD_REQUIRED = "At least one spread is required for each ODC Type.";
        public const string AT_LEAST_ONE_ODC_TYPE_REQUIRED = "At least one ODC is required for a task element.";

        #endregion Required Validation Messages
    }
}
