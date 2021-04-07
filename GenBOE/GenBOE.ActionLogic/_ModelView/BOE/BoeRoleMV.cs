namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// A Boe and its roles, meant for Bulk Role Assignment
    /// </summary>
    [Serializable]
    public class BoeRoleMV
    {
        /// <summary>
        /// Boe Id
        /// </summary>
        public int BoeId { get; set; } = -1;

        /// <summary>
        /// Is Multi WBS / Clin - for display purposes only, will not be used during a save
        /// </summary>
        public bool IsMultiWbsClin { get; set; } = false;

        /// <summary>
        /// Boe Title - for display purposes only, will not be used during a save
        /// </summary>
        public string BoeTitle { get; set; } = string.Empty;

        /// <summary>
        /// WBS String - for display purposes only, will not be used during a save
        /// </summary>
        public string WbsString { get; set; } = string.Empty;

        /// <summary>
        /// Clin String - for display purposes only, will not be used during a save
        /// </summary>
        public string ClinString { get; set; } = string.Empty;

        /// <summary>
        /// BOE State - for display purposes only, will not be used during a save
        /// </summary>
        public BOEState State { get; set; }

        /// <summary>
        /// Assigned authors, this will be saved for the Boe Id
        /// </summary>
        public ICollection<int> AssignedAuthors { get; set; } = new List<int>();

        /// <summary>
        /// Assigned approvers, this will be saved for the Boe Id
        /// </summary>
        public ICollection<int> AssignedApprovers { get; set; } = new List<int>();

        /// <summary>
        /// Assigned Subcontractor Authors, this will be saved for the Boe Id
        /// </summary>
        public ICollection<int> AssignedSubAuthors { get; set; } = new List<int>();
    }
}
