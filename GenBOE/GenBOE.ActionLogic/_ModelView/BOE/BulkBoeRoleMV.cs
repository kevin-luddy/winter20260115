namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    /// <summary>
    /// A class for Bulk Role Assignment page
    /// </summary>
    [Serializable]
    public class BulkBoeRoleMV
    {
        /// <summary>
        /// WS Id
        /// </summary>
        public int WorkspaceId { get; set; } = -1;

        /// <summary>
        /// Potential WS Authors
        /// </summary>
        public ICollection<SelectListItem> PotentialAuthors { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Potential WS Approvers
        /// </summary>
        public ICollection<SelectListItem> PotentialWsApprovers { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Potential WS Subcontractor Authors
        /// </summary>
        public ICollection<SelectListItem> PotentialWsSubAuthors { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// All Assigned Boe Roles
        /// </summary>
        public ICollection<BoeRoleMV> BoeRoleDetails { get; set; } = new List<BoeRoleMV>();
    }
}
