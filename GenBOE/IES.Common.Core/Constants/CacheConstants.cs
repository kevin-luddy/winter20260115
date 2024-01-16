namespace IES.Common.Core.Constants
{
    static public class CacheConstants
    {
        // Shared OpenXml lock
        readonly static public object OPEN_XML_LOCK = new object();

        // Security
        readonly static public string SECURITY_PERMISSIONS_BY_USERID = "Permissions_By_UserId".ToLower();

        // Common Reference Tables
        readonly static public string EXPORT_FORMAT = "ExportFormat".ToLower();
        readonly static public string SPREAD_CURVE = "SpreadCurve".ToLower();
        readonly static public string ROLES = "Roles".ToLower();
        readonly static public string SORTBY = "SortBy".ToLower();
        readonly static public string WORKSPACE_STATE = "WorkspaceState".ToLower();
        readonly static public string PROPOSAL_STATE_TYPE = "ProposalStateType".ToLower();
        readonly static public string ELEMENTS_OF_COST_TYPE = "ElementsOfCostType".ToLower();
        readonly static public string SIKORSKY_LEGACY_RESOURCES = "SikorskyLegacyResources".ToLower();
        readonly static public string BOE_STATE = "BOEState".ToLower();
        readonly static public string HM_STATUS = "HM_STATUS".ToLower();
        readonly static public string HM_LABORSOURCE = "HM_LABORSOURCE".ToLower();
        readonly static public string HM_USAGE = "HM_USAGE".ToLower();
        readonly static public string MOQ_TYPE = "MOQType".ToLower();
        readonly static public string EMAILS = "Emails".ToLower();
        readonly static public string LINE_OF_BUSINESS_TYPE = "LineOfBusinessType".ToLower();
        readonly static public string PROPOSAL_CLASS_TYPE = "ProposalClassType".ToLower();
        readonly static public string CONTRACT_TYPE = "ContractType".ToLower();
        readonly static public string SELECTED_CONTRACT_TYPE = "SelectedContractType".ToLower();
        readonly static public string REPORTS = "Reports".ToLower();
        readonly static public string FIELD_TYPE = "FieldType".ToLower();
        readonly static public string PROPRICER_FIELDS = "ProPricerFields".ToLower();
        readonly static public string PROJMAP_PROPRICER_FIELDS = "ProjMapProPricerFields".ToLower();
        readonly static public string SEGMENT_TYPE = "SegmentType".ToLower();
        readonly static public string RATE_TYPES = "RateTypes".ToLower();
        readonly static public string ODC_SPREAD_CURVE = "ODCSpreadCurve".ToLower();
        readonly static public string SUM_VARIABLE_RESOURCE_TYPE = "SUM_VARIABLE_RESOURCE_TYPE".ToLower();
        readonly static public string PROJECT_MAP_SPREAD_CURVE = "ProjectMapSpreadCurve".ToLower();
        readonly static public string RESOURCE_CLASS_TYPE = "ResourceClassType".ToLower();
        readonly static public string GET_ALL_REVISIONS = "GET_ALL_REVISIONS".ToLower();

        #region PTM CommonConstants

        /// <summary>
        /// User
        /// </summary>
        public const string USER = "User_";

        /// <summary>
        /// User exists
        /// </summary>
        public const string USER_EXISTS = "User_Exists_";

        /// <summary>
        /// User is online
        /// </summary>
        public const string USERS_ONLINE = "users_online";

        /// <summary>
        /// Roles for user
        /// </summary>
        public const string ROLES_FOR_USER = "RolesForUser_";

        /// <summary>
        /// PPR Checklist ID by Proposal ID
        /// </summary>
        public const string PPR_CHECKLIST_ID_BY_PROPOSAL_ID = "PPRChecklistId_By_ProposalId_";

        /// <summary>
        /// PPR Checklist content by PPR Checklist ID
        /// </summary>
        public const string PPR_CHECKLIST_CONTENT_BY_ID = "PPRChecklistContent_Id_";

        /// <summary>
        /// PAR Checklist ID by Proposal ID
        /// </summary>
        public const string PAR_CHECKLIST_ID_BY_PROPOSAL_ID = "PARChecklistId_By_ProposalId_";

        /// <summary>
        /// PAR Checklist content by PAR Checklist ID
        /// </summary>
        public const string PAR_CHECKLIST_CONTENT_BY_ID = "PARChecklistContent_Id_";

        /// <summary>
        /// Http Context caching for whether current action is saving permissions
        /// </summary>
        public const string SAVE_PERMISSIONS_ACTION = "SavePermissionsAction";

        #endregion PTM CommonConstants
    }

}
