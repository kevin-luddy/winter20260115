// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
    /// <summary>
    /// This class holds constants used for talking to the Controllers.
    /// </summary>
    public static class IESWebConstants
    {
        #region Content Types
        /// <summary>
        /// The content type name for text.
        /// </summary>
        public readonly static string CONTENT_TYPE_TEXT = "Text";

        /// <summary>
        /// The content type name for tables.
        /// </summary>
        public readonly static string CONTENT_TYPE_TABLE = "Table";

        /// <summary>
        /// The content type name for text.
        /// </summary>
        public readonly static string CONTENT_TYPE_ATTACHMENT = "Attachment";

        #endregion

        #region Document Types
        /// <summary>
        /// The document type name for PPR&amp;D.
        /// </summary>
        public readonly static string DOCUMENT_TYPE_PPRD = "PPRD";

        /// <summary>
        /// The document type name for Document.
        /// </summary>
        public readonly static string DOCUMENT_TYPE_DOCUMENT = "CostVolume";

        #endregion

        #region Home

        /// <summary>
        /// The name for the Home Controller
        /// </summary>
        public readonly static string CONTROLLER_HOME = "Home";

        /// <summary>
        /// The action name for getting PPR&amp;D revisions
        /// </summary>
        public readonly static string ACTION_GET_REVISIONS = "GetRevisions";

        /// <summary>
        /// The action name for locking a revision.
        /// </summary>
        public readonly static string ACTION_LOCK = "Lock";

        /// <summary>
        /// The action name for unlocking a revision.
        /// </summary>
        public readonly static string ACTION_UNLOCK = "Unlock";

        /// <summary>
        /// The action name for refreshing the lock for a revision.
        /// </summary>
        public readonly static string ACTION_REFRESH_LOCK = "RefreshLock";

        #endregion Home

        #region export PPRD&D
        /// <summary>
        /// Generate Full PPRD Controller.
        /// </summary>
        public readonly static string ACTION_GENERATE_FULL_PPRD = "GenerateFullPPRD";

        #endregion

        #region PPR&D 
        /// <summary>
        /// The name for the PPR&amp;D Controller.
        /// </summary>
        public readonly static string CONTROLLER_PPRD = "PPRD";

        /// <summary>
        /// The view name for the PPR&amp;D page.
        /// </summary>
        public readonly static string VIEW_PPRD = "Index";

        /// <summary>
        /// The action name for retrieving the PPR&amp;D data.
        /// </summary>
        public readonly static string ACTION_GET_PPRD = "GetPPRD";

        /// <summary>
        /// The action name for saving a PPR&amp;D Documents.
        /// </summary>
        public readonly static string ACTION_SAVE_PPRD = "Save";

        /// <summary>
        /// The action name for validating a deletion of a Section.
        /// </summary>
        public readonly static string ACTION_VALIDATE_DELETE_SECTION = "ValidateDeleteSection";

        #endregion PPR&D

        #region Banner

        /// <summary>
        /// The action name for saving a Banner.
        /// </summary>
        public readonly static string ACTION_SAVE_BANNER = "SaveBanner";

        /// <summary>
        /// The action name for deleting a Banner.
        /// </summary>
        public readonly static string ACTION_DELETE_BANNER = "DeleteBanner";

        /// <summary>
        /// The action for editing a Banner.
        /// </summary>
        public readonly static string ACTION_EDIT_BANNER = "EditBanner";

        /// <summary>
        /// The view banners
        /// </summary>
        public readonly static string VIEW_BANNERS = "Banners";

        /// <summary>
        /// The action clear cache
        /// </summary>
        public readonly static string ACTION_CLEAR_CACHE = "ClearCache";

        #endregion Banner

        #region Document

        /// <summary>
        /// The name for the Document Controller.
        /// </summary>
        public readonly static string CONTROLLER_DOCUMENT = "Document";

        /// <summary>
        /// The action name for the Document index page
        /// </summary>
        public readonly static string ACTION_INDEX_DOCUMENT = "Index";

        /// <summary>
        /// The action name for editing a Document.
        /// </summary>
        public readonly static string ACTION_EDIT_DOCUMENT = "Edit";

        /// <summary>
        /// The action name for saving a Document.
        /// </summary>
        public readonly static string ACTION_SAVE_DOCUMENT = "Save";

        /// <summary>
        /// The action name for deleting a Document.
        /// </summary>
        public readonly static string ACTION_DELETE_DOCUMENT = "Delete";

        /// <summary>
        /// The action name for publishing a Document.
        /// </summary>
        public readonly static string ACTION_PUBLISH_DOCUMENT = "Publish";

        /// <summary>
        /// The action name for getting the proposals for adding a new Document
        /// </summary>
        public readonly static string ACTION_GET_PROPOSALS_FOR_NEW_DOCUMENT = "GetProposalsForNewDocument";

        /// <summary>
        /// The action name for saving a new Document
        /// </summary>
        public readonly static string ACTION_SAVE_NEW_DOCUMENT = "SaveNewDocument";

        /// <summary>
        /// The action name for getting the rate codes for the selected revision on the edit document page
        /// </summary>
        public readonly static string ACTION_GET_RATE_CODES_FOR_REVISION = "GetRateCodesForRevision";

        /// <summary>
        /// The action name for getting the sections for the selected revision on the edit document page
        /// </summary>
        public readonly static string ACTION_GET_SECTIONS_FOR_REVISION = "GetSectionsForRevision";

        #endregion

        #region Version

        /// <summary>
        /// The name for the Version Controller
        /// </summary>
        public readonly static string CONTROLLER_VERSION = "Version";

        /// <summary>
        /// The view name for the Version index.
        /// </summary>
        public readonly static string VIEW_VERSION = "Version";

        /// <summary>
        /// The action name for updating the version diff
        /// </summary>
        public readonly static string ACTION_GET_VERSION_DIFFERENCES = "GetVersionDifferences";

        /// <summary>
        /// The view name for the Differences partial page
        /// </summary>
        public readonly static string VIEW_VERSION_DIFF = "Comparison";

        /// <summary>
        /// The action name for publishing a revision.
        /// </summary>
        public readonly static string ACTION_PUBLISH = "Publish";

        /// <summary>
        /// The action name to rollback a revision.
        /// </summary>
        public readonly static string ACTION_ROLLBACK = "Rollback";

        /// <summary>
        /// The action to validate rates for a revision.
        /// </summary>
        public readonly static string ACTION_VALIDATE_RATES = "ValidateRates";

        #endregion

        #region Rate        

        /// <summary>
        /// The action name to get rates by version.
        /// </summary>
        public readonly static string ACTION_GET_RATES_BY_VERSION = "GetRatesByVersion";

        /// <summary>
        /// The action name to get WIP Rates with section information;
        /// </summary>
        public readonly static string ACTION_GET_WIP_RATE_SECTIONS = "GetWIPRateSections";

        /// <summary>
        /// The action name for exporting the rates.
        /// </summary>
        public readonly static string ACTION_EXPORT_RATES = "Export";

        /// <summary>
        /// The action name for validating a rate code file.
        /// </summary>
        public readonly static string ACTION_VALIDATE_RATE_CODES = "ValidateRateCodes";

        /// <summary>
        /// The action name for importing a rate code file.
        /// </summary>
        public readonly static string ACTION_IMPORT_RATE_CODES = "ImportRateCodes";

        /// <summary>
        /// The action name for exporting a rate code file.
        /// </summary>
        public readonly static string ACTION_EXPORT_RATE_CODES = "ExportRateCodes";

        /// <summary>
        /// The action name for downloading the example rate codes import file.
        /// </summary>
        public readonly static string ACTION_DOWNLOAD_EXAMPLE_RATE_CODES_IMPORT_FILE = "DownloadRateCodesImportExample";

        /// <summary>
        /// The action name for importing rate file.
        /// </summary>
        public readonly static string ACTION_IMPORT_RATES = "ImportRates";

        /// <summary>
        /// The action name for downloading the example rates import file.
        /// </summary>
        public readonly static string ACTION_DOWNLOAD_EXAMPLE_RATES_IMPORT_FILE = "DownloadRatesImportExample";

        /// <summary>
        /// The action name for saving Rates.
        /// </summary>
        public readonly static string ACTION_SAVE_RATE = "Save";

        /// <summary>
        /// The action name for viewing the rates page.
        /// </summary>
        public readonly static string ACTION_VIEW_RATES = "View";

        /// <summary>
        /// The name for the Rate Controller.
        /// </summary>
        public readonly static string CONTROLLER_RATE = "Rate";

        /// <summary>
        /// The default history in years to display in grid.
        /// </summary>
        public readonly static int DEFAULT_RATE_HISTORY = 5;

        /// <summary>
        /// The view name for creating a Rate.
        /// </summary>
        public readonly static string VIEW_CREATE_RATE = "Create";

        #endregion Rate

        #region Burden Pool
        /// <summary>
        /// The name for the Burden Mapping Controller.
        /// </summary>
        public readonly static string CONTROLLER_BURDEN_POOL = "BurdenPool";

        /// <summary>
        /// The action name to get burden pools by version.
        /// </summary>
        public readonly static string ACTION_GET_BURDEN_POOLS = "GetBurdenPools";

        /// <summary>
        /// The action name for saving Burden Pools.
        /// </summary>
        public readonly static string ACTION_SAVE_BURDEN_POOL = "Save";

        /// <summary>
        /// The action name for viewing the burden pools page.
        /// </summary>
        public readonly static string ACTION_VIEW_BURDEN_POOLS = "View";

        #endregion Burden Pool

        #region Reports

        /// <summary>
        /// The name for the Reports Controller.
        /// </summary>
        public readonly static string CONTROLLER_REPORTS = "Reports";

        /// <summary>
        /// The view name for the Reports index.
        /// </summary>
        public readonly static string VIEW_REPORTS = "Reports";

        /// <summary>
        /// The action name for exporting Cobra data.
        /// </summary>
        public readonly static string ACTION_EXPORT_COBRA_DATA = "ExportCobraData";

        /// <summary>
        /// The action name for exporting ProPricer data.
        /// </summary>
        public readonly static string ACTION_EXPORT_PRO_PRICER_DATA = "ExportProPricerData";

        /// <summary>
        /// The action name for exporting a revision as JSON.
        /// </summary>
        public readonly static string ACTION_EXPORT_REVISION_AS_JSON = "ExportRevisionAsJson";

        #endregion

        #region Admin

        /// <summary>
        /// The name for the Admin Controller.
        /// </summary>
        public readonly static string CONTROLLER_ADMIN = "Admin";

        /// <summary>
        /// The view name for the CobraYearConfiguration index.
        /// </summary>
        public readonly static string VIEW_COBRA_YEAR_CONFIGURATION = "CobraYearConfiguration";

        /// <summary>
        /// The action name for getting the Cobra Year Configuration values.
        /// </summary>
        public readonly static string ACTION_GET_COBRA_YEAR_CONFIGURATION = "GetCobraYearConfiguration";

        /// <summary>
        /// The action name for saving the Cobra Year Configuration values.
        /// </summary>
        public readonly static string ACTION_SAVE_COBRA_YEAR_CONFIGURATION = "SaveCobraYearConfiguration";

        /// <summary>
        /// The view name for the Cobra mapping detail index.
        /// </summary>
        public readonly static string VIEW_COBRA_DETAILS = "CobraDetails";

        /// <summary>
        /// The action name to get Cobra mapping details by version.
        /// </summary>
        public readonly static string ACTION_GET_COBRA_DETAILS_BY_VERSION = "GetCobraDetailsByVersion";

        /// <summary>
        /// The action name for saving the Cobra mapping details.
        /// </summary>
        public readonly static string ACTION_SAVE_COBRA_DETAILS = "SaveCobraDetails";

        /// <summary>
        /// The view name for the Resource Code Replication.
        /// </summary>
        public readonly static string VIEW_RATE_CODE_REPLICATION = "RateCodeReplication";

        /// <summary>
        /// The action name for retrieving the Rate Code Replication values.
        /// </summary>
        public readonly static string ACTION_GET_RATE_CODE_REPLICATION = "GetRateCodeReplication";

        /// <summary>
        /// The action name for saving the Resource Code Replication.
        /// </summary>
        public readonly static string ACTION_SAVE_RATE_CODE_REPLICATION = "SaveRateCodeReplication";

        /// <summary>
        /// The view name for the RevisionConfiguration index.
        /// </summary>
        public readonly static string VIEW_REVISION_CONFIGURATION = "RevisionConfiguration";

        /// <summary>
        /// The action name for retrieving the Revision Configuration values.
        /// </summary>
        public readonly static string ACTION_GET_REVISION_CONFIGURATION = "GetRevisionConfiguration";

        /// <summary>
        /// The action name for saving the Revision Configuration values.
        /// </summary>
        public readonly static string ACTION_SAVE_REVISION_CONFIGURATION = "SaveRevisionConfiguration";

        /// <summary>
        /// The action name for saving the Get Menu Options.
        /// </summary>
        public readonly static string ACTION_GET_MENU_OPTIONS = "GetMenuOptions";

        /// <summary>
        /// The action to save pick lists.
        /// </summary>
        public readonly static string ACTION_SAVE_MANAGE_PICK_LISTS = "SaveManagePickLists";

        /// <summary>
        /// The action to fix pick list errors
        /// </summary>
        public readonly static string ACTION_FIX_PICK_LIST_ERRORS = "FixPickListErrors";

        /// <summary>
        /// Display Manage Pick Lists page
        /// </summary>
        public const string DISPLAY_MANAGE_PICK_LISTS = "DisplayManagePickLists";

        /// <summary>
        /// Display page to select which pick list to manage
        /// </summary>
        public const string DISPLAY_SELECT_PICK_LIST_TO_MANAGE = "DisplaySelectPickListToManage";

        /// <summary>
        /// Display page to manage which applications are offline
        /// </summary>
        public const string MANAGE_OFFLINE_APPLICATIONS = "ManageOfflineApplications";

        /// <summary>
        /// Save Manage Offline Applications
        /// </summary>
        public const string SAVE_OFFLINE_APPLICATIONS = "SaveManageOfflineApplications";

        #endregion

        #region Who's Online

        /// <summary>
        /// The action name for showing the who's online button
        /// </summary>
        public readonly static string ACTION_SHOW_WHOS_ONLINE = "CanViewWhosOnline";
        
        /// <summary>
        /// The action name for getting who's online
        /// </summary>
        public readonly static string ACTION_GET_WHOS_ONLINE = "GetWhosOnline";

        #endregion

        #region File Attachments
        
        /// <summary>
        /// The name for the File Attachments Controller.
        /// </summary>
        public readonly static string CONTROLLER_FILE_ATTACHMENTS = "FileAttachment";

        /// <summary>
        /// The action name to get File Attachments by version.
        /// </summary>
        public readonly static string ACTION_GET_FILE_ATTACHMENTS = "GetFileAttachments";

        /// <summary>
        /// The action name for saving File Attachments.
        /// </summary>
        public readonly static string ACTION_SAVE_FILE_ATTACHMENTS = "Save";

        /// <summary>
        /// The view name for the File Attachments index.
        /// </summary>
        public readonly static string VIEW_FILE_ATTACHMENTS = "Index";
        #endregion File Attachments
    }
}
