// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
	using System.Data.Entity.Validation;
	using System.Data.SqlClient;
    using System.Linq;
	using System.Net.NetworkInformation;
	using System.Transactions;

    /*
     * GlobalTestCaseSetup sets up the most commonly used IDs  for test cases. 
     * These IDs include Workspace ID, BOE ID, WBS ID, Task Element ID, and Labor Type ID. 
     * If these IDs will be used in a test class, the test class must include a static function
     * that accepts 1 parameter and labeled with the [ClassInitialize] attribute. The[ClassInitialize] attribute 
     * will make sure this function is called first, before any other test method in the class. 
     * Inside of this static function, you can choose to call GlobalWorkspaceID or GlobalBOEID
     * (the 2 most commonly used, but others are available). If a workspace ID or BOE ID has not been created,
     * it will do so at this time. For an example of this code, look at BOEDetailsCommitMapperTest.cs or 
     * ManageCLINRetrieveDataLoaderTest.cs.
     * 
     * The one important thing to remember is when you are setting up a test method that could 
     * potentially delete any of the global IDs to include logic to reset that global ID.
     * If you do not reset the ID, then other tests could fail. For an example of how this is done,
     * look at BOEDetailsCommitLoaderTest.L_DeleteLMLaborType. In that test method, you will see that 
     * check has been added if the labor type about to be deleted is the Global Labor Type ID, if it is,
     * the global labor type ID is reset and GlobalTestCaseSetup.CreateGlobalBOELaborTypeID is called to create 
     * a new Global Labor Type ID. Since GlobalTestCaseSetup. CreateGlobalBOELaborTypeID will create a new labor
     * type, it’s possible that any get asserts could fail if you don’t account for this new labor type 
     * being added. 
*/

    public static class GlobalTestCaseSetup
    {
        private static Collection<string> _FirstNames { get; set; }
        private static Collection<string> _LastNames { get; set; }

        // global workspace ID
        private static int _GlobalWorkspaceID = 0;

        // global BOE ID
        private static int _GlobalBOEID = 0;

        // global BOE AuthorID
        private static int _GlobalBOEAuthorID = 0;

        // global WBS ID     
        private static int _GlobalWBSID = 0;

        //global WBS TITLE
        private static String _GlobalwbsTitleEnd = "";

        // global Export Format Id for workspace
        private static int _GlobalWorkspaceTemplateID = -1;

        // global Task Element ID
        private static int _GlobalTaskElementID = 0;

        // global BOE Labor Type ID
        private static int _GlobalBOELaborTypeID = 0;

        // global Custom Field ID
        private static int _GlobalCustomFieldID = 0;

        // global Custom Field Value ID
        private static int _GlobalCustomFieldValueID = 0;

        // global BOE Custom Field Value Xref ID
        private static int _GlobalBOECustomFieldValueXrefID = 0;

        // global BOE Labor Type Field Value Xref ID
        private static int _GlobalBOELaborTypeCustomFieldValueXrefID = 0;

        // global BOE Labor Type Field Value Xref ID
        private static int _GlobalBOETaskElementCustomFieldValueXrefID = 0;

        // global workspace export file (this is a .docx in byte form)
        private static byte[] _GlobalWorkspaceExportFile = null;

        // global ODC Element ID
        private static int _GlobalODCElementID = 0;

        //global material Element ID
        private static int _GlobalmaterialID = 0;

        private static Collection<UserCreated> _Users = new Collection<UserCreated>();

        // Get ETI UserID for tests
        private static int _GlobalETIUserID = 0;

        // global BOE Approver IDs
        // This is needed to get the approver's approval types from the BOEApproval db table
        private static int _GlobalApproverBOEID1 = 0;
        private static int _GlobalApproverBOEID2 = 0;
        private static int _GlobalApproverBOEID3 = 0;

        private static int _GlobalResourceListID = 0;
        private static int _GlobalResourceID = 0;
        private static int _GlobalPerfOrgListID = 0;
        private static int _GlobalPerfOrgID = 0;
        private static int _GlobalClinID = 0;

        private static int _GlobalPerDiemID = 0;
        private static int _GlobalDestLocationID = 0;
        private static int _GlobalDepartureLocationID = 0;

        private static int _GlobalMoqTypeSelectionId = 0;
		private static int _GlobalMOQTypeSelectionTableDataId = 0;

		private static int _GlobalMoqTypeTableId = 0;

		// Global MOQ Type Selection Table Data Resource Hours
		private static int _GlobalMOQTypeSelectionTableDataResourceHours = 0;
		// Global Common Disclosure Skill Mix ID
		private static int _GlobalCommonDisclosureSkillMixId = 0;
		// Global Skill Mix Summary ID
		private static int _GlobalSkillMixSummaryId = 0;
		// Global Skill Mix ID
		private static int _GlobalSkillMixId = 0;

        #region Get Global IDs

		#region Get Global IDs

		/// <summary>
		/// This function will get the currentworkspace that a class can use
		/// </summary>
		public static int GlobalWorkspaceID
        {
            get
            {
                if (_GlobalWorkspaceID == 0)
                {
                    _GlobalWorkspaceID = _CreateWorkspace(true);
                }

                return _GlobalWorkspaceID;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public static byte[] GlobalWorkspaceExportFile
        {
            get
            {
                // since this is tied to the global workspace, this will exist if there is a workspace, otherwise create one.
                if (_GlobalWorkspaceID == 0)
                {
                    _CreateWorkspace(true);
                }

                return _GlobalWorkspaceExportFile;
            }
        }

        public static int GlobalWorkspaceTemplateID
        {
            get
            {
                // since this is tied to the global workspace, this will exist if there is a workspace, otherwise create one.
                if (_GlobalWorkspaceID == 0)
                {
                    _CreateWorkspace(true);
                }

                return _GlobalWorkspaceTemplateID;
            }
        }

        /// <summary>
        /// This function will get the current wbs that a class can use. This is called
        /// from the creation of a BOE
        /// </summary>
        public static int GlobalWBSID
        {
            get
            {
                if (_GlobalWBSID == 0)
                {
                    _GlobalWBSID = CreateWBS();
                }
                return _GlobalWBSID;
            }
        }

        /// <summary>
        /// This function will get the current boe that a class can use
        /// </summary>
        public static int GlobalBOEID
        {
            get
            {
                if (_GlobalBOEID == 0)
                {
                    _GlobalBOEID = CreateBOE(GlobalWorkspaceID);
                }
                return _GlobalBOEID;
            }
        }

        /// <summary>
        /// This function will get the first BOE Approval for the global BOE
        /// </summary>
        public static int GlobalBOEApprover1
        {
            get
            {
                return _GlobalApproverBOEID1;
            }
        }

        /// <summary>
        /// This function will get the second BOE Approval for the global BOE
        /// </summary>
        public static int GlobalBOEApprover2
        {
            get
            {
                return _GlobalApproverBOEID2;
            }
        }

        /// <summary>
        /// This function will get the third BOE Approval for the global BOE
        /// </summary>
        public static int GlobalBOEApprover3
        {
            get
            {
                return _GlobalApproverBOEID3;
            }
        }

        public static int GlobalBOEAuthorID
        {
            get
            {
                if (GlobalBOEID != 0)
                {
                    return _GlobalBOEAuthorID;
                }
                return 0;
            }
        }

        /// <summary>
        /// This function will get the current resource list
        /// </summary>
        public static int GlobalResourceListID
        {
            get
            {
                // since this is tied to the global workspace, this will exist if there is a workspace, otherwise create one.
                if (_GlobalWorkspaceID == 0 || _GlobalResourceListID == 0)
                {
                    _CreateWorkspace(true);
                }
                return _GlobalResourceListID;
            }
        }

        /// <summary>
        /// This function will get the current resource
        /// </summary>
        public static int GlobalResourceID
        {
            get
            {
                CreateResourceIDByWorkspace();

                return _GlobalResourceID;
            }
        }

        /// <summary>
        /// This function will get the current perf org list
        /// </summary>
        public static int GlobalPerfOrgListID
        {
            get
            {
                // since this is tied to the global workspace, this will exist if there is a workspace, otherwise create one.
                if (_GlobalWorkspaceID == 0 || _GlobalPerfOrgListID == 0)
                {
                    _CreateWorkspace(true);
                }
                return _GlobalPerfOrgListID;
            }
        }

        /// <summary>
        /// This function will get the current perf org
        /// </summary>
        public static int GlobalPerfOrgID
        {
            get
            {
                CreatePerfOrgIDByWorkspace();

                return _GlobalPerfOrgID;
            }
        }

        /// <summary>
        /// Returns all users in the collection
        /// </summary>
        /// <returns></returns>
        public static Collection<UserCreated> GetAllUsers()
        {
            return _Users;
        }

        /// <summary>
        /// This function will get the current wbs that a class can use. This is called
        /// from the creation of a BOE
        /// </summary>
        public static string GlobalwbsTitleEnd
        {
            get
            {
                return _GlobalwbsTitleEnd;
            }
        }

        /// <summary>
        /// This function will get the current wbs that a class can use. This is called
        /// from the creation of a BOE
        /// </summary>
        public static int GlobalTaskElementID
        {
            get
            {
                CreateGlobalTaskElementID();

                return _GlobalTaskElementID;
            }
        }

        /// <summary>
        /// This function will get the current wbs that a class can use. This is called
        /// from the creation of a BOE
        /// </summary>
        public static int GlobalBOELaborTypeID
        {
            get
            {
                CreateGlobalBOELaborTypeID();

                return _GlobalBOELaborTypeID;
            }
        }

        public static int GlobalMoqTypeSelectionId
        {
            get
            {
                CreateGlobalMoqTypeSelectionId();

                return _GlobalMoqTypeSelectionId;
            }
        }

		public static int GlobalMoqTypeSelectionTableDataId
		{
			get
			{
				CreateGlobalMoqTypeSelectionTableDataId();

				return _GlobalMOQTypeSelectionTableDataId;
			}
		}

		public static int GlobalMoqTypeTableId
        {
            get
            {
                CreateGlobalMoqTypeTableId();

                return _GlobalMoqTypeTableId;
            }
        }
        
        public static int GetEtiUserID()
        {

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                _GlobalETIUserID = (from g in gbe.ETIusers
                                    select g.ETIUserID).First();
            }

            return _GlobalETIUserID;
        }

        /// <summary>
        /// This function will get the current clin that a class can use
        /// </summary>
        public static int GlobalClinID
        {
            get
            {
                if (_GlobalClinID == 0)
                {
                    _GlobalClinID = CreateCLIN();
                }
                return _GlobalClinID;
            }
        }

        public static int GlobalPerDiemID
        {
            get
            {
                if (_GlobalPerDiemID == 0)
                {
                    CreatePerDiem();
                }

                return _GlobalPerDiemID;
            }
        }
        public static int GlobalDestLocationID
        {
            get
            {
                if (_GlobalDestLocationID == 0)
                {
                  _GlobalDestLocationID =  _CreateLocation();
                }

                return _GlobalDestLocationID;
            }
        }
        public static int GlobalDepartureLocationID
        {
            get
            {
                if (_GlobalDepartureLocationID == 0)
                {
                  _GlobalDepartureLocationID=  _CreateLocation();
                }

                return _GlobalDepartureLocationID;
            }
        }
		public static int GlobalCommonDisclosureSkillMix
		{
			get
			{
				if (_GlobalCommonDisclosureSkillMixId == 0)
				{
					_GlobalCommonDisclosureSkillMixId = _CreateCommonDisclosureSkillMix();
				}

				return _GlobalCommonDisclosureSkillMixId;
			}
		}

		#endregion Get Global IDs

		#region Public Creation Methods

		/// <summary>
		/// Check to see if ID is zero, create otherwise
		/// </summary>
		public static int CreateWorkspace()
        {
            return (_CreateWorkspace(false));
        }


        /// <summary>
        /// This function will get the current custom field ID
        /// </summary>
        public static int GlobalCustomFieldID
        {
            get
            {
                if (_GlobalCustomFieldID == 0)
                {
                    _GlobalCustomFieldID = CreateCustomField(GlobalWorkspaceID);
                }
                return _GlobalCustomFieldID;
            }
        }

        /// <summary>
        /// This function will get the current custom field value ID
        /// </summary>
        public static int GlobalCustomFieldValueID
        {
            get
            {
                if (_GlobalCustomFieldValueID == 0)
                {
                    _GlobalCustomFieldValueID = CreateCustomFieldValue();
                }
                return _GlobalCustomFieldValueID;
            }
        }


        /// <summary>
        /// This function will get the current BOE custom field value xref ID
        /// </summary>
        public static int GlobalBOECustomFieldValueXrefID
        {
            get
            {
                if (_GlobalBOECustomFieldValueXrefID == 0)
                {
                    _GlobalBOECustomFieldValueXrefID = CreateBOECustomFieldValueXref();
                }
                return _GlobalBOECustomFieldValueXrefID;
            }
        }

        /// <summary>
        /// This function will get the current labor type custom field value xref ID
        /// </summary>
        public static int GlobalLaborTypeCustomFieldValueXrefID
        {
            get
            {
                if (_GlobalBOELaborTypeCustomFieldValueXrefID == 0)
                {
                    _GlobalBOELaborTypeCustomFieldValueXrefID = CreateLaborTypeCustomFieldValueXref();
                }
                return _GlobalBOELaborTypeCustomFieldValueXrefID;
            }
        }

        /// <summary>
        /// This function will get the current task element custom field value xref ID
        /// </summary>
        public static int GlobalTaskElementCustomFieldValueXrefID
        {
            get
            {
                if (_GlobalBOETaskElementCustomFieldValueXrefID == 0)
                {
                    _GlobalBOETaskElementCustomFieldValueXrefID = CreateBOECustomFieldValueXref();
                }
                return _GlobalBOETaskElementCustomFieldValueXrefID;
            }
        }

        /// <summary>
        /// Creates a bunch of ETI users with no permissions
        /// </summary>
        /// <param name="numUsers"></param>
        /// <returns></returns>
        public static Collection<ETIuser> CreatePermissionlessUsers(int numUsers)
        {
            Collection<ETIuser> toReturn = new Collection<ETIuser>();

            for (int i = 0; i < numUsers; i++)
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // Create a new user
                    Models.ETIuser NewUser = new Models.ETIuser();
                    NewUser.ETIUserID = -1; // insert
                    NewUser.EmailAddress = "blah@blah.com";

                    // try to adjust theNTID since we can generate duplicates...
                    string unique = CreateRandomWord(4);
                    string firstname = _FirstNames[MOQObject.randomNumberGenerator.Next(_FirstNames.Count() - 1)];
                    string lastname = _LastNames[MOQObject.randomNumberGenerator.Next(_LastNames.Count() - 1)];
                    string username = firstname + " " + lastname;

                    NewUser.DisplayName = username;

                    // create the ntID from the users randomly generated name, we have to stay within the max # of 
                    // characters in the name though.  place some random GUID chars after the name to help with uniqueness
                    NewUser.NTID = firstname.Substring(0, firstname.Length >= 1 ? 1 : firstname.Length) +
                                    lastname.Substring(0, lastname.Length >= 5 ? 5 : lastname.Length) + unique;

                    NewUser.PhoneNumber = "555/555-5555";
                    NewUser.UpdateDT = DateTime.Now;

                    gbe.ETIusers.Add(NewUser);
                    gbe.SaveChanges();

                    toReturn.Add(NewUser);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// For a user assign them into a specific role
        /// </summary>
        /// <param name="inUserCreated">The user to assign</param>
        /// <param name="inRoleToCreate">the role to assign the user to</param>
        public static void CreateGlobalUserPermission(UserCreated inUserCreated,
                                                 Role inRoleToCreate)
        {
            _GrantUserPermission(GlobalWorkspaceID, GlobalBOEID, inUserCreated, inRoleToCreate);
        }

        /// <summary>
        /// Check to see if ID is zero, create otherwise
        /// </summary>
        public static int CreateBOE(int workspaceID, int clinID = 0, int wbsID = 0, int inNumberOfAuthors = 1, int inNumberOfApprovers = 3, int inNumberOfReviewers = 0,
                                       int inNumberOfSysAdmins = 1, int inNumberOfWorkspaceAdmins = 1, int inNumberOfMetricAdmins=1, bool isMultiClinWbs=false)
        {
            return _CreateBOE(workspaceID, clinID, wbsID,
                inNumberOfAuthors, inNumberOfApprovers, inNumberOfReviewers, inNumberOfSysAdmins, inNumberOfWorkspaceAdmins, inNumberOfMetricAdmins, isMultiClinWbs);
        }

        /// <summary>
        /// Check to see if ID is zero, create otherwise
        /// </summary>
        public static int CreateGlobalBOE()
        {
            return GlobalBOEID;
        }

        /// <summary>
        /// Check to see if ID is zero, create otherwise
        /// </summary>
        public static int CreateWBS()
        {
            return _CreateWBS();
        }

        /// <summary>
        /// Creates a CLIN in the global workspace
        /// </summary>
        /// <returns></returns>
        public static int CreateCLIN()
        {
            return CreateCLIN(GlobalWorkspaceID);
        }

        /// <summary>
        /// Creates a CLIN in any workspace
        /// </summary>
        /// <param name="workspaceID"></param>
        /// <returns></returns>
        public static int CreateCLIN(int workspaceID)
        {
            return _CreateCLIN(workspaceID);
        }

        /// <summary>
        /// Check to see if ID is zero, create otherwise
        /// </summary>
        public static void CreateGlobalWBSTitle()
        {
            if (String.IsNullOrEmpty(_GlobalwbsTitleEnd))
            {
                _GlobalwbsTitleEnd = MOQObject.randomNumberGenerator.Next().ToString();
            }
        }

        /// <summary>
        /// Check to see if ID is zero, create otherwise
        /// </summary>
        public static void CreateGlobalTaskElementID()
        {
            if (_GlobalTaskElementID == 0)
            {
                _GlobalTaskElementID = _CreateTaskElement();
            }
        }

        /// <summary>
        /// Check to see if ID is zero, create otherwise
        /// </summary>
        public static void CreateGlobalBOELaborTypeID()
        {
            if (_GlobalBOELaborTypeID == 0)
            {
                _GlobalBOELaborTypeID = _CreateBOELaborType();
            }
        }

        public static void CreateGlobalMoqTypeSelectionId()
        {
            if (_GlobalMoqTypeSelectionId == 0)
            {
                _GlobalMoqTypeSelectionId = _CreateMoqTypeSelection();
            }
        }

		public static void CreateGlobalMoqTypeSelectionTableDataId()
		{
			if (_GlobalMOQTypeSelectionTableDataId == 0)
			{
				_GlobalMOQTypeSelectionTableDataId = _CreateMOQTypeSelectionTableDataResourceHours();
			}
		}

		public static void CreateGlobalMoqTypeTableId()
        {
            if (_GlobalMoqTypeTableId == 0)
            {
                _GlobalMoqTypeTableId = _CreateMoqTypeTable();
            }
        }

        public static void CreatePerDiem()
        {
            if (_GlobalPerDiemID == 0)
            {
                _GlobalPerDiemID = _CreatePerDiem();
            }
        }

        public static void CreateDestinationLocation()
        {
            if (_GlobalDestLocationID == 0)
            {
                _GlobalDestLocationID = _CreateLocation();
            }
        }
        public static void CreateDepartureLocation()
        {
            if (_GlobalDepartureLocationID == 0)
            {
				_GlobalDepartureLocationID = _CreateLocation();
            }
        }

		public static void CreateSkillMix()
		{
			if (_GlobalSkillMixId == 0)
			{
				_GlobalSkillMixId = _CreateSkillMix();
			}
		}

		public static void CreateCommonDisclosureSkillMix()
		{
			if (_GlobalCommonDisclosureSkillMixId == 0)
			{
				_GlobalCommonDisclosureSkillMixId = _CreateCommonDisclosureSkillMix();
			}
		}

		public static void CreateSkillMixSummary()
		{
			if (_GlobalSkillMixSummaryId == 0)
			{
				_GlobalSkillMixSummaryId = _CreateSkillMixSummary();
			}
		}

		public static void CreateMOQTypeSelectionTableDataResourceHours()
		{
			if (_GlobalMOQTypeSelectionTableDataResourceHours == 0)
			{
				_GlobalMOQTypeSelectionTableDataResourceHours = _CreateMOQTypeSelectionTableDataResourceHours();
			}
		}

		#endregion Public Creation Methods

		#region Global Reset Methods

		/// <summary>
		/// Reset the ID so any calls to Create will create new records
		/// </summary>
		public static void ResetGlobalWorkspaceID()
        {
            _GlobalWorkspaceID = 0;
            _GlobalWBSID = 0;
            _GlobalBOEID = 0;
            _GlobalClinID = 0;
            _GlobalWorkspaceID = GlobalWorkspaceID;
        }

        /// <summary>
        /// Reset the ID so any calls to Create will create new records
        /// </summary>
        public static void ResetGlobalBOEID()
        {
            _GlobalBOEID = 0;
            _GlobalBOEID = GlobalBOEID;
        }

        /// <summary>
        /// Reset the ID so any calls to Create will create new records
        /// </summary>
        public static void ResetGlobalWBSID()
        {
            _GlobalWBSID = 0;
            _GlobalWBSID = GlobalWBSID;
        }

        /// <summary>
        /// Reset the ID so any calls to Create will create new records
        /// </summary>
        public static void ResetGlobalTaskElementID()
        {
            _GlobalTaskElementID = 0;
        }

        public static void ResetGlobalODCElementID()
        {
            _GlobalODCElementID = 0;
        }

        /// <summary>
        /// Reset the ID so any calls to Create will create new records
        /// </summary>
        public static void ResetGlobalBOELaborTypeID()
        {
            _GlobalBOELaborTypeID = 0;
        }

        /// <summary>
        /// Reset the ID so any calls to Create will create new records
        /// </summary>
        public static void ResetGlobalClinID()
        {
            _GlobalClinID = 0;
            _GlobalClinID = GlobalClinID;

        }

        public static void ResetGlobalResourceListID()
        {
            _GlobalResourceListID = 0;
        }

        public static void ResetGlobalResourceID()
        {
            _GlobalResourceID = 0;
        }

        public static void ResetGlobalPerfOrgListID()
        {
            _GlobalPerfOrgListID = 0;
        }

        public static void ResetGlobalPerfOrgID()
        {
            _GlobalPerfOrgID = 0;
        }

        public static void ResetGlobalCustomFieldID()
        {
            _GlobalCustomFieldID = 0;
        }

        public static void ResetGlobalCustomFieldValueID()
        {
            _GlobalCustomFieldValueID = 0;
        }

        public static void ResetGlobalBOECustomFieldValueXrefID()
        {
            _GlobalBOECustomFieldValueXrefID = 0;
        }

        public static void ResetGlobalBOELaborTypeCustomFieldValueXrefID()
        {
            _GlobalBOELaborTypeCustomFieldValueXrefID = 0;
        }

        public static void ResetGlobalBOETaskElementCustomFieldValueXrefID()
        {
            _GlobalBOETaskElementCustomFieldValueXrefID = 0;
        }
        #endregion Global Reset Methods

        #region Private Create Global IDs

        /// <summary>
        /// Create a workspace using LINQ since we don't have an upsertWorkspace SP yet
        /// </summary>
        /// <returns>Workspace ID that was created</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static int _CreateWorkspace(bool isGlobal)
        {
            // get a user from the ETIUser table
            GetEtiUserID();
            int workspaceToUse = 0;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                int resourceListID = CreateResourceList();
                int performingOrgListID = CreatePerfOrgList();
                string tempName = CreateRandomWord(8);

                // create a few resources
                for (int i = 0; i < 10; i++)
                {
                    CreateResourceInList(resourceListID);
                    CreatePerformingOrgInList(performingOrgListID);
                }

                // insert new export template for this workspace
                Models.OutputFormatTemplate outputFormat = new Models.OutputFormatTemplate();
                outputFormat.TemplateID = -1;
                outputFormat.Template = "Mock_" + tempName;
                outputFormat.TemplateDescription = "Mock_" + tempName + " longer description";
                outputFormat.TemplateFile = (from t in gbe.OutputFormatTemplates  // just pull from db
                                             select t.TemplateFile).First();
                _GlobalWorkspaceExportFile = outputFormat.TemplateFile;
                gbe.OutputFormatTemplates.Add(outputFormat);
                gbe.SaveChanges();

				IQueryable<int> outputFormatTemplateId = from w in gbe.OutputFormatTemplates
                                      where w.Template == "Mock_" + tempName
                                      select w.TemplateID;
                _GlobalWorkspaceTemplateID = outputFormatTemplateId.FirstOrDefault();

                // create new Workspace
                Models.Workspace NewWorkspace = new Models.Workspace();
                NewWorkspace.WorkspaceID = -1;
                NewWorkspace.WorkspaceName = "Mock_" + tempName;
                string shortName = "sn_" + tempName.ToLower();
                NewWorkspace.WorkspaceShortName = shortName;
                NewWorkspace.WorkspaceDescription = "Desc_" + tempName;
                NewWorkspace.UpdateDT = DateTime.Now;
                NewWorkspace.TemplateID = _GlobalWorkspaceTemplateID;
                NewWorkspace.WorkspaceStateID = 1;
                NewWorkspace.CreatedByETIUserID = _GlobalETIUserID;
                NewWorkspace.CostVolumeLeadPricerUserID = _GlobalETIUserID;
                NewWorkspace.PerformingOrganizationListID = performingOrgListID;
                NewWorkspace.ResourceListID = resourceListID;
                NewWorkspace.AllowSearch = false;
                NewWorkspace.ProposalSubmitDate = Convert.ToDateTime("02/01/2011");
                NewWorkspace.NumProPricerExport = 0;
                NewWorkspace.ProposalStatusID = 0;
                NewWorkspace.StatusComment = "Global Test Case Setup";
                NewWorkspace.BOEExportSortByID = (int)ExportSortBOEBy.WBS;
                NewWorkspace.CostPrecision = 2;
                NewWorkspace.ProjectMapTypeID = (int)ProjectMapType.StandardWithoutOffload;
                gbe.Workspaces.Add(NewWorkspace);
                gbe.SaveChanges();

				IQueryable<int> getSpaces = from w in gbe.Workspaces
                                where w.WorkspaceName == "Mock_" + tempName
                                select w.WorkspaceID;

                workspaceToUse = getSpaces.FirstOrDefault();

                Models.OutputFormatTemplateWorkspaceXREF newOutputFormatXREF = new Models.OutputFormatTemplateWorkspaceXREF();
                newOutputFormatXREF.OutputFormatID = -1;
                newOutputFormatXREF.TemplateID = _GlobalWorkspaceTemplateID;
                newOutputFormatXREF.WorkspaceID = workspaceToUse;
                gbe.OutputFormatTemplateWorkspaceXREFs.Add(newOutputFormatXREF);
                gbe.SaveChanges();
                Models.OutputFormatTemplateWorkspaceXREF lsOutputFormatXREF = new Models.OutputFormatTemplateWorkspaceXREF();
                lsOutputFormatXREF.OutputFormatID = -1;
                lsOutputFormatXREF.TemplateID = 9001; // master
                lsOutputFormatXREF.WorkspaceID = workspaceToUse;
                gbe.OutputFormatTemplateWorkspaceXREFs.Add(lsOutputFormatXREF);
                gbe.SaveChanges();
                Models.OutputFormatTemplateWorkspaceXREF portraitOutputFormatXREF = new Models.OutputFormatTemplateWorkspaceXREF();
                portraitOutputFormatXREF.OutputFormatID = -1;
                portraitOutputFormatXREF.TemplateID = 100001; // ssc portrait
                portraitOutputFormatXREF.WorkspaceID = workspaceToUse;
                gbe.OutputFormatTemplateWorkspaceXREFs.Add(portraitOutputFormatXREF);
                gbe.SaveChanges();

                _InsertCLINSForWorkspace(workspaceToUse);
                CreateWorkspaceVariable(workspaceToUse);
                _CreateUsers(workspaceToUse, null, 0, 0, 0, 0, 1, 0);

                if (isGlobal)
                {
                    _GlobalResourceListID = resourceListID;
                    _GlobalPerfOrgListID = performingOrgListID;
                    _GlobalWorkspaceID = workspaceToUse;
                }

            }

            return workspaceToUse;
        }

        /// <summary>
        /// create CLINS for a given workspace
        /// </summary>
        private static void _InsertCLINSForWorkspace(int inWorkspaceID)
        {

            ClinDTODataLoader manageCLINDataLoader = new ClinDTODataLoader();

            Collection<ClinDTO> createCLINS = new Collection<ClinDTO>();

            int clinIDCount = -1;
            // create 3 new CLINS
            for (int x = 0; x < 2; x++)
            {
				ClinDTO singleCLIN = new ClinDTO();
                singleCLIN.Id = clinIDCount;
                singleCLIN.ClinNumber = "Moq" + MOQObject.randomNumberGenerator.Next().ToString() + x.ToString();
                singleCLIN.ClinTitle = Guid.NewGuid().ToString();
                singleCLIN.UpdateDate = DateTime.Now;
                singleCLIN.Updateable = UpdateType.Upsert;
                singleCLIN.StartDate = Convert.ToDateTime("08/01/2010");
                singleCLIN.EndDate = Convert.ToDateTime("04/01/2011");
                singleCLIN.WorkspaceID = inWorkspaceID;

                createCLINS.Add(singleCLIN);
                clinIDCount--;
            }

            using (TransactionScope scope = new TransactionScope())
            {
                manageCLINDataLoader.Save(createCLINS);
                scope.Complete();
            }
        }

        /// <summary>
        /// Create a BOE using LINQ. While creating a BOE, this function will also
        /// call to create CLINs, a WBS, create users (for authors/approvers), and
        /// create task elements (which creates a labor type and labor spread)
        /// </summary>
        /// <returns>BOE ID that was created</returns
        /// NOTE : Suppression is easier than tracking down a dispose bug for testing only code
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "GenBOE.DataBridge.DTO.ClinDTODataLoader"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static int _CreateBOE(int workspaceID = 0, int clinID = 0, int wbsID = 0,
            int inNumberOfAuthors = 1, int inNumberOfApprovers = 3, int inNumberOfReviewers = 3,
            int inNumberOfSysAdmins = 1, int inNumberOfWorkspaceAdmins = 1, int inNumberOfMetricAdmins = 1, bool multiClinWbs = false)
        {
            // clean up any global IDs underneath BOE
            _GlobalTaskElementID = 0;
            _GlobalBOELaborTypeID = 0;
            _GlobalBOEAuthorID = 0;
            _GlobalODCElementID = 0;

            // Verify we have a workspace, if not use the global one.
            if (workspaceID == 0)
            {
                workspaceID = GlobalWorkspaceID;
            }

            // if a WBS was not supplied, use the global one
            if (wbsID == 0)
            {
                wbsID = GlobalWBSID;

                WbsDTODataLoader wbsDataLoader = new WbsDTODataLoader();

                // If the WBS has a clin, get it
                if (wbsDataLoader.GetById(wbsID).ClinIDs.Count > 0)
                {
                    clinID = wbsDataLoader.GetById(wbsID).ClinIDs[0];
                }
            }

            // if a CLIN was not supplied, and the WBS has none associated with it, create some new ones
            if (clinID == 0)
            {
                ClinDTODataLoader clinDataLoader = new ClinDTODataLoader();

                _InsertCLINSForWorkspace(workspaceID);

                Collection<int> getClins = new Collection<int>(clinDataLoader.GetByWorkspaceId(workspaceID).Select(x => x.Id).ToArray());

                clinID = getClins.ToArray()[MOQObject.randomNumberGenerator.Next(getClins.Count - 1)];
            }

            // gets current user ID
            GetEtiUserID();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // create new BOE
                // have to create a BOE this way since we currently don't have a manage boe SP/functionimport
                Models.BOE NewBOE = new Models.BOE();
                NewBOE.BOEID = -1;
                NewBOE.BOEStateID = 1;
                NewBOE.BOEStartDate = Convert.ToDateTime("09/01/2010");
                NewBOE.BOEEndDate = Convert.ToDateTime("11/01/2012");
                NewBOE.BOETitle = "New BOE Title";
                NewBOE.BOEDescription = "New Global BOE";
                NewBOE.UpdateDT = DateTime.Now;
                NewBOE.MetricDisclosureAcknowledge = false;
				NewBOE.IsMultiClinWbs = multiClinWbs;
				// NewBOE.WorkspaceID = GlobalWorkspaceID;

				// save the BOE early to get the BOEID so we have it available (we'll save again)
				int boeToUse = gbe.upsertBOE(-1, NewBOE.BOEID, wbsID, clinID, NewBOE.BOEStateID,
                    NewBOE.BOEStartDate, NewBOE.BOEEndDate, null, workspaceID, NewBOE.UpdateDT, NewBOE.BOEDescription, "Data Source", _GlobalETIUserID, NewBOE.MetricDisclosureAcknowledge, NewBOE.NumAuthorReassigned, false, NewBOE.BOETitle,"", null, NewBOE.IsMultiClinWbs).FirstOrDefault().Value;

                // create users to be used for author and approver
                // TBD: If we want to associate roles, we will have to add it here 
                _Users = _CreateUsers(workspaceID, boeToUse, inNumberOfAuthors, inNumberOfApprovers, inNumberOfReviewers, inNumberOfSysAdmins, inNumberOfWorkspaceAdmins, inNumberOfMetricAdmins);
				IEnumerable<UserCreated> author = from u in _Users
                             where ((Role)(u.Role)).Equals(Role.Author)
                             select u;
				IEnumerable<UserCreated> approvers = from u in _Users
                                where ((Role)(u.Role)).Equals(Role.Approver)
                                select u;

                Collection<int> approverList = new Collection<int>(approvers.Select(x => x.User.ETIUserID).ToArray());
                // set the global approver IDs so it can be used in other test cases
                for (int x = 0; x < inNumberOfApprovers; x++)
                {
                    if (x == 0)
                    {
                        _GlobalApproverBOEID1 = approverList[x];
                    }
                    else if (x == 1)
                    {
                        _GlobalApproverBOEID2 = approverList[x];
                    }
                    else if (x == 2)
                    {
                        _GlobalApproverBOEID3 = approverList[x];
                    }
                }

                // The CreateUsers function above created an author so we'll always have 1 to use
                _GlobalBOEAuthorID = author.First().User.ETIUserID;

                // set _GlobalBOEID
                _GlobalBOEID = boeToUse;

                // create a workspace variable that uses this boe
                CreateSumofBoeWorkspaceVariableIDWithBoeID(_GlobalBOEID);
                return boeToUse;
            }
        }

        /// <summary>
        /// Create new users
        /// optionally create a systemadmin and workspaceadmin depending on parameters
        /// </summary>
        /// <returns>Collection of users created</returns>
        private static Collection<UserCreated> _CreateUsers(int? workspaceID, int? boeID, int inNumberOfAuthors, int inNumberOfApprovers, int inNumberOfReviewers,
                                                            int inNumberOfSysAdmins, int inNumberOfWorkspaceAdmins, int inNumberOfMetricAdmins)
        {
            if (!workspaceID.HasValue)
            {
                workspaceID = _GlobalWorkspaceID;
            }
            if (!boeID.HasValue)
            {
                boeID = _GlobalBOEID;
            }

            _Users = new Collection<UserCreated>();
            // Create users for BOE
            // Note: UserDataLoader needs to be reworked which is why it isn't being used here

            for (int i = 0; i < inNumberOfAuthors; i++)
            {
                _CreateUserDatabase(workspaceID.Value, boeID.Value, _Users, Role.Author);
            }

            for (int i = 0; i < inNumberOfApprovers; i++)
            {
                _CreateUserDatabase(workspaceID.Value, boeID.Value, _Users, Role.Approver);
            }

            for (int i = 0; i < inNumberOfReviewers; i++)
            {
                _CreateUserDatabase(workspaceID.Value, boeID.Value, _Users, Role.WorkspaceReviewer);
            }

            for (int i = 0; i < inNumberOfSysAdmins; i++)
            {
                _CreateUserDatabase(workspaceID.Value, boeID.Value, _Users, Role.SystemAdmin);
            }

            for (int i = 0; i < inNumberOfMetricAdmins; i++)
            {
                _CreateUserDatabase(workspaceID.Value, boeID.Value, _Users, Role.MetricsAdmin);
            }

            for (int i = 0; i < inNumberOfWorkspaceAdmins; i++)
            {
                _CreateUserDatabase(workspaceID.Value, boeID.Value, _Users, Role.WorkspaceAdmin);
            }

            return _Users;
        }

        /// <summary>
        /// For a user assign them into a specific role
        /// </summary>
        /// <param name="inUserCreated">The user to assign</param>
        /// <param name="inRoleToCreate">the role to assign the user to</param>
        private static void _GrantUserPermission(int workspaceID, int boeID, UserCreated inUserCreated,
                                                 Role inRoleToCreate)
        {
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // if this is a system role, add them to SystemUserRole table
                if (inRoleToCreate == Role.SystemAdmin ||
                    inRoleToCreate == Role.MetricsAdmin)
                {
                    Models.SystemUserRole role = new SystemUserRole();
                    role.SystemUserRoleID = -1; // insert
                    role.UpdateDT = DateTime.Now;
                    role.ETIUserID = inUserCreated.User.ETIUserID;
                    role.RoleID = (int)inRoleToCreate;

                    gbe.SystemUserRoles.Add(role);
                }
                // if this is a workspace role, add them to the WorkspaceUserRole table
                else if (inRoleToCreate == Role.WorkspaceAdmin ||
                         inRoleToCreate == Role.WorkspaceUser || 
                         inRoleToCreate == Role.SubcontractAdmin)
                {

                    Models.WorkspaceUserRole role = new WorkspaceUserRole();
                    role.WorkspaceUserRoleID = -1; // insert
                    role.WorkspaceID = workspaceID;
                    role.UpdateDT = DateTime.Now;
                    role.ETIUserID = inUserCreated.User.ETIUserID;
                    role.RoleID = (int)inRoleToCreate;

                    gbe.WorkspaceUserRoles.Add(role);
                }
                // else, this is a BOE user role, add them to the BOEUserRole
                else
                {
                    Models.BOEUserRole role = new BOEUserRole();
                    role.BOEUserRoleID = -1; // insert
                    role.BOEID = boeID;
                    role.UpdateDT = DateTime.Now;
                    role.ETIUserID = inUserCreated.User.ETIUserID;
                    role.RoleID = (int)inRoleToCreate;

                    gbe.BOEUserRoles.Add(role);
                }
                gbe.SaveChanges();
            }
        }

        /// <summary>
        /// Create a user and add them to the inUsersCreated collection
        /// </summary>
        /// <param name="inUsersCreated">Collection to add this new user to</param>
        /// <param name="inRoleToCreate">Role to create user in</param>
        private static void _CreateUserDatabase(int workspaceID, int boeID, Collection<UserCreated> inUsersCreated,
                                                Role inRoleToCreate)
        {
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                _CreateUserDatabaseImpl(workspaceID, boeID, inUsersCreated, inRoleToCreate, gbe);
            }
        }

        private static void _CreateUserDatabaseImpl(int workspaceID, int boeID, Collection<UserCreated> inUsersCreated, Role inRoleToCreate, GenBoeEntities gbe)
        {
            // Create a new user
            Models.ETIuser NewUser = new Models.ETIuser();
            NewUser.ETIUserID = -1; // insert
            NewUser.EmailAddress = "blah@blah.com";

            // try to adjust theNTID since we can generate duplicates...
            string unique = Guid.NewGuid().ToString().Substring(0, 4);
            string firstname = _FirstNames[MOQObject.randomNumberGenerator.Next(_FirstNames.Count() - 1)];
            string lastname = _LastNames[MOQObject.randomNumberGenerator.Next(_LastNames.Count() - 1)];
            string username = firstname + " " + lastname;

            NewUser.DisplayName = username;

            // create the ntID from the users randomly generated name, we have to stay within the max # of 
            // characters in the name though.  place some random GUID chars after the name to help with uniqueness
            NewUser.NTID = firstname.Substring(0, firstname.Length >= 1 ? 1 : firstname.Length) +
                            lastname.Substring(0, lastname.Length >= 5 ? 5 : lastname.Length) + unique;

            NewUser.PhoneNumber = "555/555-5555";
            NewUser.UpdateDT = DateTime.Now;

            gbe.ETIusers.Add(NewUser);

            UserCreated user = new UserCreated();
            user.User = NewUser;
            user.Role = inRoleToCreate;

            // Link the new user into the "potential" roles WorkspacePermissions table
            if (inRoleToCreate.Equals(Role.Approver) ||
                inRoleToCreate.Equals(Role.Author))
            {

                Models.BOEPotentialRole permission = new Models.BOEPotentialRole();
                //permission.ETIGroupID = null;
                permission.ETIuser = NewUser;
                permission.BOEPotentialRoleID = -1; // insert
                permission.WorkspaceID = workspaceID;
                permission.RoleID = (int)inRoleToCreate;
                permission.UserRemoved = Convert.ToInt16(WorkspaceUserRemoved.Default);
                permission.UpdateDT = DateTime.Now;

                gbe.BOEPotentialRoles.Add(permission);
            }
            else if (inRoleToCreate.Equals(Role.WorkspaceReviewer))
            {
                Models.WorkspaceUserRole permission = new Models.WorkspaceUserRole();
                //permission.ETIGroupID = null;
                permission.ETIuser = NewUser;
                permission.WorkspaceID = workspaceID;
                permission.RoleID = (int)inRoleToCreate;
                permission.UpdateDT = DateTime.Now;

                gbe.WorkspaceUserRoles.Add(permission);
            }

            inUsersCreated.Add(user);

            // retry 10 times with 10 different names to attempt to get a unique one that doesn't 
            // collide
            for (int i = 0; i <= 9; i++)
            {
                try
                {
                    gbe.SaveChanges();
                    break; // save worked .. no errorrs
                }
                catch (SqlException)
                {
                    // error, possibly NTID unique conflict
                    System.Console.Out.WriteLine(string.Format("Tried to save user with NTID {0}, going to try again with new id.  Retry attempt {1}.", NewUser.NTID, i));

                    // try to adjust theNTID since we can generate duplicates...
                    unique = Guid.NewGuid().ToString().Substring(0, 4);
                    firstname = _FirstNames[MOQObject.randomNumberGenerator.Next(_FirstNames.Count() - 1)];
                    lastname = _LastNames[MOQObject.randomNumberGenerator.Next(_LastNames.Count() - 1)];
                    username = firstname + " " + lastname;

                    NewUser.DisplayName = username;

                    // create the ntID from the users randomly generated name, we have to stay within the max # of 
                    // characters in the name though.  place some random GUID chars after the name to help with uniqueness
                    NewUser.NTID = firstname.Substring(0, firstname.Length >= 1 ? 1 : firstname.Length) +
                                    lastname.Substring(0, lastname.Length >= 5 ? 5 : lastname.Length) + unique;
                }
            }

            // every gets workspace user
            _GrantUserPermission(workspaceID, boeID, user, Role.WorkspaceUser);

            // Add their specific role requested
            _GrantUserPermission(workspaceID, boeID, user, inRoleToCreate);
        }

        /// <summary>
        /// create a WBS that can be used for a BOE
        /// </summary>
        /// <returns>WBS ID</returns>
        private static int _CreateWBS(Collection<int> clinIDs = null)
        {
            WbsDTODataLoader manageWBSLoader = new WbsDTODataLoader();
            Collection<WbsDTO> wbsDTOs = new Collection<WbsDTO>();
            WbsDTO wbsDTO = new WbsDTO();
            int WBSID = 0;

            CreateGlobalWBSTitle();

            wbsDTO.Id = -1;
            wbsDTO.WbsNumber = MOQObject.randomNumberGenerator.Next(99999).ToString();
            wbsDTO.WbsTitle = _GlobalwbsTitleEnd;
            wbsDTO.WorkspaceID = GlobalWorkspaceID;
            wbsDTO.UpdateDate = DateTime.Now;
            wbsDTO.Updateable = UpdateType.Upsert;

            // The same CLIN that was used for a BOE had to be used for this WBS
            wbsDTO.ClinIDs = clinIDs;

            wbsDTOs.Add(wbsDTO);

            using (TransactionScope scope = new TransactionScope())
            {
                manageWBSLoader.Save(wbsDTOs);
                scope.Complete();
            }
            
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IQueryable<int> getWBSForBOE = from w in gbe.WorkBreakdownStructures
                                   where w.WorkspaceID == GlobalWorkspaceID && w.WBSTitle == _GlobalwbsTitleEnd
                                   select w.WBSID;
                WBSID = Convert.ToInt32(getWBSForBOE.FirstOrDefault());
            }

            CreateSumofBoeWorkspaceVariableIDWithWbsID(WBSID);
            return WBSID;
        }

        private static int _CreateCLIN(int workspaceID)
        {
            ClinDTODataLoader manageCLINDataLoader = new ClinDTODataLoader();

			ClinDTO singleCLIN = new ClinDTO();
            singleCLIN.Id = -1;
            singleCLIN.ClinNumber = "Moq" + MOQObject.randomNumberGenerator.Next().ToString();
            singleCLIN.ClinTitle = Guid.NewGuid().ToString();
            singleCLIN.UpdateDate = DateTime.Now;
            singleCLIN.Updateable = UpdateType.Upsert;
            singleCLIN.StartDate = Convert.ToDateTime("08/01/2010");
            singleCLIN.EndDate = Convert.ToDateTime("04/01/2011");
            singleCLIN.WorkspaceID = workspaceID;

            using (TransactionScope scope = new TransactionScope())
            {
                int? id = manageCLINDataLoader.Save(singleCLIN);

                // create a workspace variable tied to this clin
                CreateSumofBoeWorkspaceVariableIDWithClinID(id.Value);
                scope.Complete();
                return id.Value;
            }
        }

        /// <summary>
        /// This function will create a new task element. The value returned from this function
        /// can be used instead of hardcoding any task element IDs for tests.
        /// CreateTaskElement also calls for the creation of labor type
        /// </summary>
        /// <returns>task element ID</returns>
        private static int _CreateTaskElement()
        {
            //// Create a new task element            
            IResourceTypeLoader resourceTypeLoader = new ResourceTypeLoader(); 
            IResourceSpreadLoader resourceSpreadLoader = new ResourceSpreadLoader();
            IOrdinaryVariableLoader ordinaryVariableLoader = new OrdinaryVariableLoader();
            IBoeTaskElementCustomFieldValueXREFLoader taskElementCustomFieldLoader = new BoeTaskElementCustomFieldValueXREFLoader();
            ILaborTypeCustomFieldValueXREFLoader laborTypeCustomFieldLoader = new LaborTypeCustomFieldValueXREFLoader();
			SkillMixDTOLoader skillMixDTOLoader = new SkillMixDTOLoader();
			CommonDisclosureSMDTODataLoader commonDisclosureLoader = new CommonDisclosureSMDTODataLoader();

			BoeTaskElementDTODataLoader boeTaskElementLoader = new BoeTaskElementDTODataLoader(resourceTypeLoader, resourceSpreadLoader, ordinaryVariableLoader, taskElementCustomFieldLoader, laborTypeCustomFieldLoader, skillMixDTOLoader, commonDisclosureLoader);
            BoeTaskElementDTO newElement = new BoeTaskElementDTO();
            newElement.Id = -1;
            newElement.BOETaskID = "B4";
            newElement.TaskTitle = Guid.NewGuid().ToString();
            newElement.Description = "a great new moq task";
            newElement.StartDate = Convert.ToDateTime("09/01/2010");
            newElement.EndDate = Convert.ToDateTime("07/01/2015");
            newElement.MOQHoursEquation = "14000 Hours * 85% ComplexityFactor";
            newElement.MOQText = "hours and factor";
            newElement.MOQType = MOQType.SSCAnalogySimilarTo;
            //newElement.MOQType = MOQType.MSTComparisonAnalogyMethod;

            newElement.BoeID = GlobalBOEID;
            newElement.UpdateDate = DateTime.Now;



            // create 3 new sum of Boe ordinary/task variables
            // 1 will reference a summary wbs, another a summary clin, and then the another a boe id
            Collection<OrdinaryVariableDto> OrdinaryVars = new Collection<OrdinaryVariableDto>();

            // create an ordinary var that is a sum of boe
            OrdinaryVariableDto OrdinaryVar = new OrdinaryVariableDto();
            OrdinaryVar.Updateable = UpdateType.Upsert;
            OrdinaryVar.Id = -1;
            OrdinaryVar.OrdinaryVariableName = "Employee";
            OrdinaryVar.OrdinaryVariableValue = 245m;
            OrdinaryVar.UpdateDate = DateTime.Now;
            OrdinaryVar.SortBOEBy = VarSortBOEBy.WBS;
            OrdinaryVar.ValueType = VarValueType.SumOfBOEs;
            SelectBOEsToSum selectBoe = new SelectBOEsToSum();
            selectBoe.WBSID = null;
            selectBoe.BoeID = GlobalTestCaseSetup.GlobalBOEID;
            selectBoe.CLINID = null;
            OrdinaryVar.SelectedBOEsToSum.Add(selectBoe);
            OrdinaryVars.Add(OrdinaryVar);

            OrdinaryVariableDto OrdinaryVar2 = new OrdinaryVariableDto();
            OrdinaryVar2.Updateable = UpdateType.Upsert;
            OrdinaryVar2.Id = -2;
            OrdinaryVar2.OrdinaryVariableName = "SourceCode";
            OrdinaryVar2.OrdinaryVariableValue = 10m;
            OrdinaryVar2.UpdateDate = DateTime.Now;
            OrdinaryVar2.SortBOEBy = VarSortBOEBy.WBS;
            OrdinaryVar2.ValueType = VarValueType.SumOfBOEs;
            SelectBOEsToSum selectBoe2 = new SelectBOEsToSum();
            selectBoe2.WBSID = GlobalTestCaseSetup.GlobalWBSID;
            selectBoe2.BoeID = null;
            selectBoe2.CLINID = null;
            OrdinaryVar2.SelectedBOEsToSum.Add(selectBoe2);
            OrdinaryVars.Add(OrdinaryVar2);

            OrdinaryVariableDto OrdinaryVar3 = new OrdinaryVariableDto();
            OrdinaryVar3.Updateable = UpdateType.Upsert;
            OrdinaryVar3.Id = -3;
            OrdinaryVar3.OrdinaryVariableName = "Vacation";
            OrdinaryVar3.OrdinaryVariableValue = 40m;
            OrdinaryVar3.UpdateDate = DateTime.Now;
            OrdinaryVar3.SortBOEBy = VarSortBOEBy.CLIN;
            OrdinaryVar3.ValueType = VarValueType.SumOfBOEs;
            SelectBOEsToSum selectBoe3 = new SelectBOEsToSum();
            selectBoe3.WBSID = null;
            selectBoe3.BoeID = null;
            selectBoe3.CLINID = GlobalClinID;
            OrdinaryVar3.SelectedBOEsToSum.Add(selectBoe3);
            OrdinaryVars.Add(OrdinaryVar3);


            newElement.OrdinaryVariables = OrdinaryVars;
            using (TransactionScope scope = new TransactionScope())
            {
                int newTaskElementID = boeTaskElementLoader.CreateOrSaveTaskElementDetail(GlobalBOEID, newElement);
                _GlobalTaskElementID = newTaskElementID;

                scope.Complete();
                return newTaskElementID;
            }
        }

        /// <summary>
        /// This function will create a new BOE Labor Type and creates a labor spread that
        /// can be used by this labor type
        /// </summary>
        /// <returns>labor type ID</returns>
        private static int _CreateBOELaborType()
        {
            ResourceTypeDto createLaborType = new ResourceTypeDto();
            IResourceTypeLoader resourceTypeLoader = new ResourceTypeLoader();
            IResourceSpreadLoader resourceSpreadLoader = new ResourceSpreadLoader();
            IOrdinaryVariableLoader ordinaryVariableLoader = new OrdinaryVariableLoader();
            IBoeTaskElementCustomFieldValueXREFLoader taskElementCustomFieldLoader = new BoeTaskElementCustomFieldValueXREFLoader();
            ILaborTypeCustomFieldValueXREFLoader laborTypeCustomFieldLoader = new LaborTypeCustomFieldValueXREFLoader();
			SkillMixDTOLoader skillMixDTOLoader = new SkillMixDTOLoader();
			CommonDisclosureSMDTODataLoader commonDisclosureLoader = new CommonDisclosureSMDTODataLoader();

			BoeTaskElementDTODataLoader boeTaskElementLoader = new BoeTaskElementDTODataLoader(resourceTypeLoader, resourceSpreadLoader, ordinaryVariableLoader, taskElementCustomFieldLoader, laborTypeCustomFieldLoader, skillMixDTOLoader, commonDisclosureLoader);

            createLaborType.Id = -1;
            //createLaborType.BOETaskElementID = GlobalTestCaseSetup.GlobalTaskElementID;
            createLaborType.ResourceID = GlobalTestCaseSetup.GlobalResourceID;
            createLaborType.PerformingOrgID = GlobalTestCaseSetup.GlobalPerfOrgID;
            createLaborType.SpreadCurveID = SpreadCurves.SpreadCurve2;
            createLaborType.SpreadType = SpreadType.Hours;
            createLaborType.PercentSpread = 11;
            createLaborType.ValueSpread = 10;
            createLaborType.StartDateValue = Convert.ToDateTime("09/01/2010");
            createLaborType.EndDateValue = Convert.ToDateTime("11/01/2010");
            createLaborType.UpdateDate = DateTime.Now;
            createLaborType.Updateable = UpdateType.Upsert;
            createLaborType.CanOffload = true;
            createLaborType.TieredPercentage = 1.2m;
            createLaborType.AddOrDelete = "A";
			createLaborType.CLINID = GlobalTestCaseSetup.GlobalClinID;

            // Act

            Collection<ResourceSpreadDto> laborSpreadCollection = new Collection<ResourceSpreadDto>();

            // create a labor spread
            ResourceSpreadDto laborSpread = new ResourceSpreadDto();
            laborSpread.Id = -1;
            laborSpread.LaborSpreadDate = Convert.ToDateTime("09/01/2010");
            laborSpread.LaborSpreadValue = 8;
            laborSpread.UpdateDate = DateTime.Now;
            laborSpread.BoeID = GlobalBOEID;
            laborSpread.Updateable = UpdateType.Upsert;

            laborSpreadCollection.Add(laborSpread);

            createLaborType.LaborSpreads = laborSpreadCollection;

            // Save BOE Labor Type
            int BOELaborTypeID;
            using (TransactionScope scope = new TransactionScope())
            {
                BOELaborTypeID = boeTaskElementLoader.CreateorSaveLMLaborType(GlobalTaskElementID, createLaborType);

                // Set _GlobalBOELaborTypeID
                _GlobalBOELaborTypeID = BOELaborTypeID;

                // Save BOE Labor Spread
                boeTaskElementLoader.CreateOrSaveLMLaborSpread(GlobalBOELaborTypeID, laborSpread);
                scope.Complete();
            }

            return BOELaborTypeID;
        }

        private static int _CreateMoqTypeSelection()
        {
            IMoqTypeTableCustomFieldValueXREFLoader moqTypeTableCustomFieldValueLoader = new MoqTypeTableCustomFieldValueXREFLoader();
			IMOQTypeSelectionTableDataResourceHoursDTOLoader resourceHoursLoader = new MOQTypeSelectionTableDataResourceHoursDTOLoader();
			SkillMixDTOLoader skillMixDTOLoader = new SkillMixDTOLoader();
			CommonDisclosureSMDTODataLoader commonDisclosureLoader = new CommonDisclosureSMDTODataLoader();

			MoqTypeDataLoader moqTypeDataLoader = new MoqTypeDataLoader(moqTypeTableCustomFieldValueLoader, resourceHoursLoader);

			MoqTypeSelection moqTypeSelection = new MoqTypeSelection()
            {
                Id = -1,
                TaskId = GlobalTestCaseSetup.GlobalTaskElementID,
                SelectedMOQType = MOQType.Historical,
                CerName = "test name",
                DescriptionHoursRequired = "test desc",
                SmeReason = "test reason",
                SmeHoursLogic = "test hours logic",
                SmeDurationLogic = "test duration logic",
                SmeTaskEstimates = "test task estimates",
                Rationale = "test rationale",
                SkillMixRationale = "test skill mix",
				HistoricalReferenceExplanation = "test historical reference explanation",
                BoeId = GlobalTestCaseSetup.GlobalBOEID,
                Updateable = UpdateType.Upsert
            };

            MoqTableData moqTableData = new MoqTableData()
            {
                Id = -1,
                TableName = "test table",
                RepositoryName = "test repo",
                QueryType = "query type",
                DateOfReport = DateTime.Now,
                HistoricalProgramName = "test name",
                ContractNumber = "test contract",
                WbsElement = "test wbs",
                PoPStart = DateTime.Now.AddDays(-3),
                PoPEnd = DateTime.Now.AddDays(3),
                TotalWbsHours = 200,
                AdditionalQueryFilters = "test filters",
                TotalRelevantHours = 150,
                Updateable = UpdateType.Upsert
            };

            moqTypeSelection.TableData.Add(moqTableData);

            int? moqTypeSelectionId;
            using (TransactionScope scope = new TransactionScope())
            {
                moqTypeSelectionId = moqTypeDataLoader.Save(moqTypeSelection);
                _GlobalMoqTypeSelectionId = moqTypeSelectionId ?? 0;
                scope.Complete();
            }

            return moqTypeSelectionId ?? 0;
        }

		private static int _CreateMoqTypeTable()
        {
            // table created with MOQ Type Selection, so make sure one is made
            CreateGlobalMoqTypeSelectionId();
            
            IMoqTypeTableCustomFieldValueXREFLoader moqTypeTableCustomFieldValueLoader = new MoqTypeTableCustomFieldValueXREFLoader();
			IMOQTypeSelectionTableDataResourceHoursDTOLoader resourceHoursLoader = new MOQTypeSelectionTableDataResourceHoursDTOLoader();
			SkillMixDTOLoader skillMixDTOLoader = new SkillMixDTOLoader();
			CommonDisclosureSMDTODataLoader commonDisclosureLoader = new CommonDisclosureSMDTODataLoader();

			MoqTypeDataLoader moqTypeDataLoader = new MoqTypeDataLoader(moqTypeTableCustomFieldValueLoader, resourceHoursLoader);


			// Get MOQ Type Selection and get ID from there
			MoqTypeSelection moqTypeSelection = moqTypeDataLoader.GetById(GlobalTestCaseSetup.GlobalMoqTypeSelectionId);

            int tableId = moqTypeSelection.TableData.First().Id;

            _GlobalMoqTypeTableId = tableId;

            return tableId;
        }
        
        /// <summary>
        /// This function will create one workspace variable
        /// </summary>
        /// <param name="inGlobalWorkspaceID">global workspace ID</param>
        private static void CreateWorkspaceVariable(int inGlobalWorkspaceID)
        {
            // Add one workspace variable to the global workspace
            using (GenBoeEntities gbe = new GenBoeEntities())
            {

                Models.WorkspaceVariable wvar = new Models.WorkspaceVariable();
                wvar.WorkspaceVariableID = -1;
                wvar.WorkspaceVariableName = "Pi";
                wvar.WorkspaceVariableValue = 3.14m;
                wvar.WorkspaceID = inGlobalWorkspaceID;
                wvar.UpdateDT = DateTime.Now;
                wvar.SortByID = (int)VarSortBOEBy.WBS;
                wvar.ValueTypeID = (int)VarValueType.Discrete;


                gbe.WorkspaceVariables.Add(wvar);
                gbe.SaveChanges();
            }


        }

        /// <summary>
        /// Check to see if ID is zero, create otherwise
        /// </summary>
        public static int CreateResourceList()
        {
            int resourceListID = -1;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                string name = Guid.NewGuid().ToString().Substring(0, 10);
                Models.ResourceList rList = new Models.ResourceList();
                rList.ResourceListID = -1;
                rList.ResourceListName = name;
                rList.UpdateDT = DateTime.Now;
                gbe.ResourceLists.Add(rList);
                gbe.SaveChanges();

                resourceListID = (from r in gbe.ResourceLists
                                  where r.ResourceListName == name
                                  select r.ResourceListID).FirstOrDefault();
            }
            return resourceListID;
        }

        /// <summary>
        /// Check to see if ID is zero, create otherwise
        /// </summary>
        public static void CreateResourceIDByWorkspace()
        {
            ResourceDTODataLoader resourceLoader = new ResourceDTODataLoader();
            
            if (_GlobalResourceID == 0)
            {
                ResourceDTO resource = new ResourceDTO();
                string name = Guid.NewGuid().ToString().Substring(0, 10);
                resource.Id = -1;
                resource.ResourceName = name;
                resource.ResourceDesc = name;
                resource.SegRegion = name;
                resource.LaborType = name;
                resource.RateType = RateType.Hours;
                resource.UpdateDate = DateTime.Now;
                resource.ElementOfCost = ElementOfCostType.LMLabor;
                resource.Updateable = UpdateType.Upsert;
                Collection<ResourceDTO> resources2 = new Collection<ResourceDTO> {resource };

                WorkspaceDTO tempWorkspace = new WorkspaceDTO();
                tempWorkspace.ResourceListID = GlobalResourceListID;

                IDictionary<int, int> resources = resourceLoader.SaveWorkspaceResources(tempWorkspace, resources2);

                if (resources.ContainsKey(-1))
                {
                    _GlobalResourceID = resources[-1];
                }

            }
        }

        public static int CreatePerfOrgList()
        {
            int performingOrgListID = -1;

            // Add one workspace variable to the global workspace
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                string name = Guid.NewGuid().ToString().Substring(0, 10);
                Models.PerformingOrganizationList poList = new Models.PerformingOrganizationList();
                poList.PerformingOrganizationListID = -1;
                poList.PerformingOrganizationListName = name;
                poList.UpdateDT = DateTime.Now;
                gbe.PerformingOrganizationLists.Add(poList);
                gbe.SaveChanges();

                performingOrgListID = (from r in gbe.PerformingOrganizationLists
                                       where r.PerformingOrganizationListName == name
                                       select r.PerformingOrganizationListID).FirstOrDefault();
            }
            return performingOrgListID;
        }

        /// <summary>
        /// Check to see if ID is zero, create otherwise
        /// </summary>
        public static void CreatePerfOrgIDByWorkspace()
        {
            if (_GlobalPerfOrgID == 0)
            {
                // Add one workspace variable to the global workspace
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    string name = Guid.NewGuid().ToString().Substring(0, 10);
                    Models.PerformingOrganization p = new PerformingOrganization();
                    p.PerformingOrganizationID = -1;
                    p.PerformingOrganizationName = name;
                    p.PerformingOrganizationDescription = name;
                    p.PerformingOrganizationListID = GlobalPerfOrgListID;
                    p.UpdateDT = DateTime.Now;
                    gbe.PerformingOrganizations.Add(p);
                    gbe.SaveChanges();

                    _GlobalPerfOrgID = (from perfOrg in gbe.PerformingOrganizations
                                        where perfOrg.PerformingOrganizationName == name
                                        select perfOrg.PerformingOrganizationID).FirstOrDefault();
                }
            }
        }

        public static int CreatePerformingOrgInList(int performingOrgListID)
        {
            int newItemID = -1;
            // Add one workspace variable to the global workspace
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                string name = Guid.NewGuid().ToString().Substring(0, 10);
                Models.PerformingOrganization p = new PerformingOrganization();
                p.PerformingOrganizationID = -1;
                p.PerformingOrganizationName = name;
                p.PerformingOrganizationDescription = name;
                p.PerformingOrganizationListID = performingOrgListID;
                p.UpdateDT = DateTime.Now;
                gbe.PerformingOrganizations.Add(p);
                gbe.SaveChanges();

                newItemID = (from perfOrg in gbe.PerformingOrganizations
                             where perfOrg.PerformingOrganizationName == name
                             select perfOrg.PerformingOrganizationID).FirstOrDefault();
            }
            return newItemID;
        }

        public static int CreateResourceInList(int resourceListID)
        {
            int newItemID = -1;
            // Add one workspace variable to the global workspace
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                string name = Guid.NewGuid().ToString().Substring(0, 10);
                Models.Resource r = new Resource();
                r.ResourceID = -1;
                r.ResourceName = name;
                r.ResourceDescription = name;
                r.ResourceListID = resourceListID;
                r.SegmentRegion = "UnitTest";
                r.LaborType = "UnitTest";
                r.UpdateDT = DateTime.Now;
                r.CostElementID = (int)ElementOfCostType.LMLabor;
                r.RateTypeID = (int)RateType.Hours;
                gbe.Resources.Add(r);
                gbe.SaveChanges();

                newItemID = (from resources in gbe.Resources
                             where resources.ResourceName == name
                             select resources.ResourceID).FirstOrDefault();
            }
            return newItemID;
        }

        /// <summary>
        /// creates custom field
        /// </summary>
        /// <param name="inGlobalWorkspaceID">Global Workspace ID</param>
        /// <returns>Custom Field ID</returns>
        public static int CreateCustomField(int inGlobalWorkspaceID)
        {
            int newCustomFieldID = -1;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                string name = Guid.NewGuid().ToString().Substring(0, 20);
                Models.CustomField c = new CustomField();
                c.CustomFieldID = -1;
                c.CustomFieldName = name;
                c.CustomFieldDisplayID = 1;
                c.CustomFieldRequired = false;
                c.WorkspaceID = inGlobalWorkspaceID;
                c.UpdateDT = DateTime.Now;
                gbe.CustomFields.Add(c);
                gbe.SaveChanges();

                newCustomFieldID = (from customField in gbe.CustomFields
                                    where customField.CustomFieldName == name
                                    select customField.CustomFieldID).FirstOrDefault();
            }
            return newCustomFieldID;
        }

        /// <summary>
        /// Creates Custom Field Value
        /// </summary>
        /// <param name="inGlobalCustomFieldID">Custom Field ID</param>
        /// <returns>Custom Field Value ID</returns>
        public static int CreateCustomFieldValue()
        {
            int newCustomFieldValue = -1;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                string name = Guid.NewGuid().ToString().Substring(0, 20);
                string desc = Guid.NewGuid().ToString().Substring(0, 30);
                Models.CustomFieldValue c = new CustomFieldValue();
                c.CustomFieldID = GlobalCustomFieldID;
                c.CustomFieldValueDescription = desc;
                c.CustomFieldValueID = -1;
                c.CustomFieldValueInUseFlag = false;
                c.CustomFieldValueName = name;
                c.UpdateDT = DateTime.Now;
                gbe.CustomFieldValues.Add(c);
                gbe.SaveChanges();

                newCustomFieldValue = (from customfieldValue in gbe.CustomFieldValues
                                       where customfieldValue.CustomFieldValueDescription == desc
                                       select customfieldValue.CustomFieldValueID).FirstOrDefault();
            }

            return newCustomFieldValue;

        }

        /// <summary>
        /// Creates BOE Custom Field Value Xref
        /// </summary>
        /// <returns>BOE Custom Field Value Xref ID</returns>
        public static int CreateBOECustomFieldValueXref()
        {
            int newBOECustomFieldValueXrefValue = -1;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                Models.BOECustomFieldValueXREF b = new BOECustomFieldValueXREF();
                b.BCFVID = -1;
                b.BOEID = GlobalBOEID;
                b.CustomFieldValueID = GlobalCustomFieldValueID;
                b.UpdateDT = DateTime.Now;
                gbe.BOECustomFieldValueXREFs.Add(b);
                gbe.SaveChanges();

                newBOECustomFieldValueXrefValue = (from boecustomFieldValueXref in gbe.BOECustomFieldValueXREFs
                                                   where boecustomFieldValueXref.BOEID == GlobalBOEID &&
                                                   boecustomFieldValueXref.CustomFieldValueID == GlobalCustomFieldValueID
                                                   select boecustomFieldValueXref.BCFVID).FirstOrDefault();
            }

            return newBOECustomFieldValueXrefValue;
        }

        /// <summary>
        /// Creates Labor Type Custom Field Value Xref
        /// </summary>
        /// <returns>Labor Type Custom Field Value Xref ID</returns>
        public static int CreateLaborTypeCustomFieldValueXref()
        {
            int newLaborTypeCustomFieldValueXrefValue = -1;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                BOELaborTypeCustomFieldValueXREF ltcf = gbe.BOELaborTypeCustomFieldValueXREFs.Where(x => x.BOELaborTypeID == GlobalBOELaborTypeID && x.CustomFieldValueID == GlobalCustomFieldValueID).FirstOrDefault();
                if (ltcf == null)
                {
                    Models.BOELaborTypeCustomFieldValueXREF b = new BOELaborTypeCustomFieldValueXREF();
                    b.BLTCFVID = -1;
                    b.BOELaborTypeID = GlobalBOELaborTypeID;
                    b.CustomFieldValueID = GlobalCustomFieldValueID;
                    b.UpdateDT = DateTime.Now;
                    gbe.BOELaborTypeCustomFieldValueXREFs.Add(b);
                    gbe.SaveChanges();

                    newLaborTypeCustomFieldValueXrefValue = (from laboTypecustomFieldValueXref in gbe.BOELaborTypeCustomFieldValueXREFs
                                                             where laboTypecustomFieldValueXref.BOELaborTypeID == GlobalBOELaborTypeID &&
                                                             laboTypecustomFieldValueXref.CustomFieldValueID == GlobalCustomFieldValueID
                                                             select laboTypecustomFieldValueXref.BLTCFVID).FirstOrDefault();
                }
                else
                {
                    newLaborTypeCustomFieldValueXrefValue = ltcf.BLTCFVID;
                }
            }

            return newLaborTypeCustomFieldValueXrefValue;
        }

        /// <summary>
        /// Deletes all existing Labor Type Custom Field Value Xrefs.
        /// </summary>
        public static void DeleteAllLaborTypeCustomFieldValueXref()
        {
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // Need to join with Custom Field Values then Custom Fields in order to check if Open Ended
                var ltcfs = from xref in gbe.BOELaborTypeCustomFieldValueXREFs
                            join cfv in gbe.CustomFieldValues on xref.CustomFieldValueID equals cfv.CustomFieldValueID
                            join cf in gbe.CustomFields on cfv.CustomFieldID equals cf.CustomFieldID
                            where xref.BOELaborTypeID == GlobalBOELaborTypeID && xref.CustomFieldValueID == GlobalCustomFieldValueID
                            select new { xref, cf };
                
                foreach (var ltcf in ltcfs)
                {
                    if (ltcf != null)
                    {
                        using (GenBoeEntities gbeDelete = new GenBoeEntities())
                        {
                            gbeDelete.deleteBOELaborTypeCustomFieldValue(ltcf.xref.BLTCFVID, ltcf.xref.BOELaborTypeID, ltcf.xref.CustomFieldValueID, ltcf.xref.UpdateDT, ltcf.cf.IsOpenEnded);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates Task Element Custom Field Value Xref
        /// </summary>
        /// <returns>Task Element Custom Field Value Xref ID</returns>
        public static int CreateTaskElementCustomFieldValueXref()
        {
            int newTaskElementCustomFieldValueXrefValue = -1;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                Models.BOETaskElementCustomFieldValueXREF b = new BOETaskElementCustomFieldValueXREF();
                b.BTECFVID = -1;
                b.BOETaskElementID = GlobalTaskElementID;
                b.CustomFieldValueID = GlobalCustomFieldValueID;
                b.UpdateDT = DateTime.Now;
                gbe.BOETaskElementCustomFieldValueXREFs.Add(b);
                gbe.SaveChanges();

                newTaskElementCustomFieldValueXrefValue = (from taskElementcustomFieldValueXref in gbe.BOETaskElementCustomFieldValueXREFs
                                                           where taskElementcustomFieldValueXref.BOETaskElementID == GlobalTaskElementID &&
                                                           taskElementcustomFieldValueXref.CustomFieldValueID == GlobalCustomFieldValueID
                                                           select taskElementcustomFieldValueXref.BTECFVID).FirstOrDefault();
            }

            return newTaskElementCustomFieldValueXrefValue;
        }

        // create moq type cf xref

        private static int _CreatePerDiem()
        {
            int perDiemID = 0;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                string qual = "Mock" + Guid.NewGuid().ToString().Substring(0, 20);;
                Models.PerDiem pd = new PerDiem();
                pd.PerDiemID = -1;
                pd.HotelRate = 10m;
                pd.MIERate = 10m;
                pd.PerDiemDestination = "MockPerDiemDest";
                pd.Qualification = qual;
                pd.PerDiemNotes = "blah blah";
                pd.PerDiemLastUpdateETIUserID = GlobalBOEAuthorID;
                pd.UpdateDT = DateTime.Now;
                gbe.PerDiems.Add(pd);
                gbe.SaveChanges();

                perDiemID = (from p in gbe.PerDiems
                             where p.Qualification == qual && p.PerDiemDestination == "MockPerDiemDest"
                             select p.PerDiemID).FirstOrDefault();

            }

            return perDiemID;
        }

        private static int _CreateLocation()
        {
            int locationID = 0;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                string qual = "Mock" + Guid.NewGuid().ToString().Substring(0, 20); ;
                Models.Location pd = new Location();
                pd.LocationID = -1;
                pd.LocationName = qual;
                pd.UpdatedByETIUserID = GlobalBOEAuthorID;
                pd.UpdateDT = DateTime.Now;
                gbe.Locations.Add(pd);
                gbe.SaveChanges();

                locationID = (from p in gbe.Locations
                             where p.LocationName==qual
                             select p.LocationID).FirstOrDefault();

            }

            return locationID;
        }

		/// <summary>
		/// Mock Create Skill Mix
		/// </summary>
		/// <returns></returns>
		private static int _CreateSkillMix()
		{
			int skillMixId = 0;
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				string rat = "Mock" + Guid.NewGuid().ToString().Substring(0, 5);
				string oldr = "OldMock" + Guid.NewGuid().ToString().Substring(0, 5);
				string newr = "NewMock" + Guid.NewGuid().ToString().Substring(0, 5);
				Models.SkillMix sm = new SkillMix();
				sm.SkillMixID = -1;
				sm.Included = true;
				sm.Rationale = rat;
				sm.ProposedHours = 100;
				sm.HistoricalHours = 100;
				sm.BOESkillMix = 100;
				sm.LaborSkillMix = 100;
				sm.ResourceOld = oldr;
				sm.ResourceNew = newr;
				sm.BOEID = GlobalBOEID;
				sm.BOETaskElementID = GlobalTaskElementID;
				gbe.SkillMixes.Add(sm);
				gbe.SaveChanges();

				skillMixId = (from s in gbe.SkillMixes
							  where s.Rationale == rat
							  select s.SkillMixID).FirstOrDefault();
			}

			return skillMixId;
		}

        #endregion Private Create Global IDs

		/// <summary>
		/// Mock Create Skill Mix
		/// </summary>
		/// <returns></returns>
		private static int _CreateCommonDisclosureSkillMix()
		{
			int commonDisclosureSkillMixId = 0;
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				string rat = "Mock" + Guid.NewGuid().ToString().Substring(0, 5);
				string resource = "resourceMock" + Guid.NewGuid().ToString().Substring(0, 5);
				string businessResource = "brcMock" + Guid.NewGuid().ToString().Substring(0, 5);
				Models.CommonDisclosureSkillMix cdsm = new CommonDisclosureSkillMix();
				cdsm.CommonDisclosureSkillMixID = -1;
				cdsm.Included = false;
				cdsm.Rationale = rat;
				cdsm.ProposedHours = 100;
				cdsm.HistoricalHours = 100;
				cdsm.BOESkillMix = 100;
				cdsm.LaborSkillMix = 100;
				cdsm.ResourceID = resource;
				cdsm.BusinessResourceID = businessResource;
				cdsm.BOEID = GlobalBOEID;
				cdsm.BOETaskElementID = GlobalTaskElementID;
				gbe.CommonDisclosureSkillMixes.Add(cdsm);
				gbe.SaveChanges();
				commonDisclosureSkillMixId = (from s in gbe.CommonDisclosureSkillMixes
							  where s.Rationale == rat
							  select s.CommonDisclosureSkillMixID).FirstOrDefault();
			}
			return commonDisclosureSkillMixId;
		}

		/// <summary>
		/// Mock Create Skill Mix
		/// </summary>
		/// <returns></returns>
		private static int _CreateSkillMixSummary()
		{
			int skillMixSummaryId = 0;
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				string rat = "Mock" + Guid.NewGuid().ToString().Substring(0, 5);
				string resource = "resourceMock" + Guid.NewGuid().ToString().Substring(0, 5);
				string businessResource = "BrcMock" + Guid.NewGuid().ToString().Substring(0, 5);
				Models.SkillMixSummary cdsm = new SkillMixSummary();
				cdsm.SkillMixSummaryID = -1;
				cdsm.Included = false;
				cdsm.Rationale = rat;
				cdsm.ProposedHours = 100;
				cdsm.HistoricalHours = 100;
				cdsm.ResourceHours = 100;
				cdsm.BusinessResourceHours = 100;
				cdsm.BOESkillMix = 100;
				cdsm.LaborSkillMix = 100;
				cdsm.ResourceID = resource;
				cdsm.BusinessResourceID = businessResource;
				cdsm.BOEID = GlobalBOEID;
				cdsm.BOETaskElementID = GlobalTaskElementID;
				gbe.SkillMixSummaries.Add(cdsm);
				gbe.SaveChanges();

				skillMixSummaryId = (from s in gbe.SkillMixSummaries
									 where s.Rationale == rat
											  select s.SkillMixSummaryID).FirstOrDefault();
			}
			return skillMixSummaryId;
		}
		/// <summary>
		/// Mock Create MOQ Type Selection Table Data Resource Hours.
		/// </summary>
		/// <returns></returns>
		private static int _CreateMOQTypeSelectionTableDataResourceHours()
		{
			int moqTypeSelectionTableDataResourceId = 0;
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				int MOQTypeSelectionTableDataId = MOQObject.randomNumberGenerator.Next(99999);
				string resource = "resourceMock" + Guid.NewGuid().ToString().Substring(0, 5);
				Models.MOQTypeSelectionTableDataResourceHour moq = new MOQTypeSelectionTableDataResourceHour();
				moq.MOQTypeSelectionTableDataResourceHoursId = -1;
				moq.ResourceName = resource;
				moq.WbsHours = 100;
				moq.TotalHours = 100;
				moq.MOQTypeSelectionTableDataId = MOQTypeSelectionTableDataId;
				moq.BOEID = GlobalBOEID;
				moq.BOETaskElementID = GlobalTaskElementID;
				gbe.MOQTypeSelectionTableDataResourceHours.Add(moq);
				gbe.SaveChanges();
				moqTypeSelectionTableDataResourceId = (from s in gbe.MOQTypeSelectionTableDataResourceHours
											  where s.MOQTypeSelectionTableDataId == MOQTypeSelectionTableDataId
													   select s.MOQTypeSelectionTableDataResourceHoursId).FirstOrDefault();
			}
			return moqTypeSelectionTableDataResourceId;
		}

		#endregion Private Create Global IDs

		public static string CreateRandomWord(int size)
        {
            return CreateRandomWord(size, false, string.Empty);
        }

        public static string CreateRandomWord(int size, bool allowNumbers)
        {
            return CreateRandomWord(size, allowNumbers, string.Empty);
        }

        public static string CreateRandomWord(int size, bool allowNumbers, string extraAllowedChars)
        {
            string viableChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (allowNumbers)
            {
                viableChars = string.Concat(viableChars, "0123456789");
            }
            viableChars = string.Concat(viableChars, extraAllowedChars);

            string word = string.Empty;

            for (int i = 0; i < size; i++)
            {
                word = string.Concat(word, viableChars[MOQObject.randomNumberGenerator.Next(viableChars.Length)]);
            }

            return word;
        }

        public static int CreateSumofBoeWorkspaceVariableIDWithBoeID(int inBoeID)
        {
            int toReturn = 0;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                Models.WorkspaceVariable wvar = new Models.WorkspaceVariable();
                wvar.WorkspaceVariableID = -1;
                wvar.WorkspaceVariableName = "Pi";
                wvar.WorkspaceVariableValue = 3.14m;
                wvar.WorkspaceID = GlobalWorkspaceID;
                wvar.UpdateDT = DateTime.Now;
                wvar.SortByID = (int)VarSortBOEBy.WBS;
                wvar.ValueTypeID = (int)VarValueType.SumOfBOEs;


                gbe.WorkspaceVariables.Add(wvar);
                gbe.SaveChanges();

                Models.SumOfBOE_WorkspaceVariableXREF b = new SumOfBOE_WorkspaceVariableXREF();
                b.WVSumID = -1;
                b.WorkspaceVariableID = wvar.WorkspaceVariableID;
                b.BOEID = inBoeID;
                b.CLINID = null;
                b.WBSID = null;
                gbe.SumOfBOE_WorkspaceVariableXREF.Add(b);
                gbe.SaveChanges();

                toReturn = Convert.ToInt32(b.WVSumID);
            }

            return toReturn;
        }

        public static int CreateSumofBoeWorkspaceVariableIDWithClinID(int inClinID)
        {
            int toReturn = 0;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                Models.WorkspaceVariable wvar = new Models.WorkspaceVariable();
                wvar.WorkspaceVariableID = -1;
                wvar.WorkspaceVariableName = "Pi";
                wvar.WorkspaceVariableValue = 3.14m;
                wvar.WorkspaceID = GlobalWorkspaceID;
                wvar.UpdateDT = DateTime.Now;
                wvar.SortByID = (int)VarSortBOEBy.CLIN;
                wvar.ValueTypeID = (int)VarValueType.SumOfBOEs;


                gbe.WorkspaceVariables.Add(wvar);
                gbe.SaveChanges();

                Models.SumOfBOE_WorkspaceVariableXREF b = new SumOfBOE_WorkspaceVariableXREF();
                b.WVSumID = -1;
                b.WorkspaceVariableID = wvar.WorkspaceVariableID;
                b.CLINID = inClinID;
                b.WBSID = null;
                b.BOEID = null;
                gbe.SumOfBOE_WorkspaceVariableXREF.Add(b);
                gbe.SaveChanges();

                toReturn = Convert.ToInt32(b.WVSumID);
            }

            return toReturn;
        }

        public static int CreateSumofBoeWorkspaceVariableIDWithWbsID(int inWbsID)
        {
            int toReturn = 0;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                Models.WorkspaceVariable wvar = new Models.WorkspaceVariable();
                wvar.WorkspaceVariableID = -1;
                wvar.WorkspaceVariableName = "Pi";
                wvar.WorkspaceVariableValue = 3.14m;
                wvar.WorkspaceID = GlobalWorkspaceID;
                wvar.UpdateDT = DateTime.Now;
                wvar.SortByID = (int)VarSortBOEBy.WBS;
                wvar.ValueTypeID = (int)VarValueType.SumOfBOEs;


                gbe.WorkspaceVariables.Add(wvar);
                gbe.SaveChanges();

                Models.SumOfBOE_WorkspaceVariableXREF b = new SumOfBOE_WorkspaceVariableXREF();
                b.WVSumID = -1;
                b.WorkspaceVariableID = wvar.WorkspaceVariableID;
                b.CLINID = null;
                b.WBSID = inWbsID;
                b.BOEID = null;
                gbe.SumOfBOE_WorkspaceVariableXREF.Add(b);
                gbe.SaveChanges();

                toReturn = Convert.ToInt32(b.WVSumID);
            }

            return toReturn;
        }
        #region init names
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static GlobalTestCaseSetup()
        {
            // NOTE: jim - I tried and tried to get these values in a file to be read in ... it worked great locally (After hours of trying to get
            // the files copied to the output file where the test executed.  I could not get it to run on TFS, the files would not copy to the folder
            // where the build lives.  So I just bit the bullet and put the names directly in her.  I am so sorry.  :(

            // load the user names into our array from file
            string[] fnames = {"AARON",
                            "ABDUL",
                            "ABE",
                            "ABEL",
                            "ABRAHAM",
                            "ABRAM",
                            "ADALBERTO",
                            "ADAM",
                            "ADAN",
                            "ADOLFO",
                            "ADOLPH",
                            "ADRIAN",
                            "AGUSTIN",
                            "AHMAD",
                            "AHMED",
                            "AL",
                            "ALAN",
                            "ALBERT",
                            "ALBERTO",
                            "ALDEN",
                            "ALDO",
                            "ALEC",
                            "ALEJANDRO",
                            "ALEX",
                            "ALEXANDER",
                            "ALEXIS",
                            "ALFONSO",
                            "ALFONZO",
                            "ALFRED",
                            "ALFREDO",
                            "ALI",
                            "ALLAN",
                            "ALLEN",
                            "ALONSO",
                            "ALONZO",
                            "ALPHONSE",
                            "ALPHONSO",
                            "ALTON",
                            "ALVA",
                            "ALVARO",
                            "ALVIN",
                            "AMADO",
                            "AMBROSE",
                            "AMOS",
                            "ANDERSON",
                            "ANDRE",
                            "ANDREA",
                            "ANDREAS",
                            "ANDRES",
                            "ANDREW",
                            "ANDY",
                            "ANGEL",
                            "ANGELO",
                            "ANIBAL",
                            "ANTHONY",
                            "ANTIONE",
                            "ANTOINE",
                            "ANTON",
                            "ANTONE",
                            "ANTONIA",
                            "ANTONIO",
                            "ANTONY",
                            "ANTWAN",
                            "ARCHIE",
                            "ARDEN",
                            "ARIEL",
                            "ARLEN",
                            "ARLIE",
                            "ARMAND",
                            "ARMANDO",
                            "ARNOLD",
                            "ARNOLDO",
                            "ARNULFO",
                            "ARON",
                            "ARRON",
                            "ART",
                            "ARTHUR",
                            "ARTURO",
                            "ASA",
                            "ASHLEY",
                            "AUBREY",
                            "AUGUST",
                            "AUGUSTINE",
                            "AUGUSTUS",
                            "AURELIO",
                            "AUSTIN",
                            "AVERY",
                            "BARNEY",
                            "BARRETT",
                            "BARRY",
                            "BART",
                            "BARTON",
                            "BASIL",
                            "BEAU",
                            "BEN",
                            "BENEDICT",
                            "BENITO",
                            "BENJAMIN",
                            "BENNETT",
                            "BENNIE",
                            "BENNY",
                            "BENTON",
                            "BERNARD",
                            "BERNARDO",
                            "BERNIE",
                            "BERRY",
                            "BERT",
                            "BERTRAM",
                            "BILL",
                            "BILLIE",
                            "BILLY",
                            "BLAINE",
                            "BLAIR",
                            "BLAKE",
                            "BO",
                            "BOB",
                            "BOBBIE",
                            "BOBBY",
                            "BOOKER",
                            "BORIS",
                            "BOYCE",
                            "BOYD",
                            "BRAD",
                            "BRADFORD",
                            "BRADLEY",
                            "BRADLY",
                            "BRADY",
                            "BRAIN",
                            "BRANDEN",
                            "BRANDON",
                            "BRANT",
                            "BRENDAN",
                            "BRENDON",
                            "BRENT",
                            "BRENTON",
                            "BRET",
                            "BRETT",
                            "BRIAN",
                            "BRICE",
                            "BRITT",
                            "BROCK",
                            "BRODERICK",
                            "BROOKS",
                            "BRUCE",
                            "BRUNO",
                            "BRYAN",
                            "BRYANT",
                            "BRYCE",
                            "BRYON",
                            "BUCK",
                            "BUD",
                            "BUDDY",
                            "BUFORD",
                            "BURL",
                            "BURT",
                            "BURTON",
                            "BUSTER",
                            "BYRON",
                            "CALEB",
                            "CALVIN",
                            "CAMERON",
                            "CAREY",
                            "CARL",
                            "CARLO",
                            "CARLOS",
                            "CARLTON",
                            "CARMELO",
                            "CARMEN",
                            "CARMINE",
                            "CAROL",
                            "CARROL",
                            "CARROLL",
                            "CARSON",
                            "CARTER",
                            "CARY",
                            "CASEY",
                            "CECIL",
                            "CEDRIC",
                            "CEDRICK",
                            "CESAR",
                            "CHAD",
                            "CHADWICK",
                            "CHANCE",
                            "CHANG",
                            "CHARLES",
                            "CHARLEY",
                            "CHARLIE",
                            "CHAS",
                            "CHASE",
                            "CHAUNCEY",
                            "CHESTER",
                            "CHET",
                            "CHI",
                            "CHONG",
                            "CHRIS",
                            "CHRISTIAN",
                            "CHRISTOPER",
                            "CHRISTOPHER",
                            "CHUCK",
                            "CHUNG",
                            "CLAIR",
                            "CLARENCE",
                            "CLARK",
                            "CLAUD",
                            "CLAUDE",
                            "CLAUDIO",
                            "CLAY",
                            "CLAYTON",
                            "CLEMENT",
                            "CLEMENTE",
                            "CLEO",
                            "CLETUS",
                            "CLEVELAND",
                            "CLIFF",
                            "CLIFFORD",
                            "CLIFTON",
                            "CLINT",
                            "CLINTON",
                            "CLYDE",
                            "CODY",
                            "COLBY",
                            "COLE",
                            "COLEMAN",
                            "COLIN",
                            "COLLIN",
                            "COLTON",
                            "COLUMBUS",
                            "CONNIE",
                            "CONRAD",
                            "CORDELL",
                            "COREY",
                            "CORNELIUS",
                            "CORNELL",
                            "CORTEZ",
                            "CORY",
                            "COURTNEY",
                            "COY",
                            "CRAIG",
                            "CRISTOBAL",
                            "CRISTOPHER",
                            "CRUZ",
                            "CURT",
                            "CURTIS",
                            "CYRIL",
                            "CYRUS",
                            "DALE",
                            "DALLAS",
                            "DALTON",
                            "DAMIAN",
                            "DAMIEN",
                            "DAMION",
                            "DAMON",
                            "DAN",
                            "DANA",
                            "DANE",
                            "DANIAL",
                            "DANIEL",
                            "DANILO",
                            "DANNIE",
                            "DANNY",
                            "DANTE",
                            "DARELL",
                            "DAREN",
                            "DARIN",
                            "DARIO",
                            "DARIUS",
                            "DARNELL",
                            "DARON",
                            "DARREL",
                            "DARRELL",
                            "DARREN",
                            "DARRICK",
                            "DARRIN",
                            "DARRON",
                            "DARRYL",
                            "DARWIN",
                            "DARYL",
                            "DAVE",
                            "DAVID",
                            "DAVIS",
                            "DEAN",
                            "DEANDRE",
                            "DEANGELO",
                            "DEE",
                            "DEL",
                            "DELBERT",
                            "DELMAR",
                            "DELMER",
                            "DEMARCUS",
                            "DEMETRIUS",
                            "DENIS",
                            "DENNIS",
                            "DENNY",
                            "DENVER",
                            "DEON",
                            "DEREK",
                            "DERICK",
                            "DERRICK",
                            "DESHAWN",
                            "DESMOND",
                            "DEVIN",
                            "DEVON",
                            "DEWAYNE",
                            "DEWEY",
                            "DEWITT",
                            "DEXTER",
                            "DICK",
                            "DIEGO",
                            "DILLON",
                            "DINO",
                            "DION",
                            "DIRK",
                            "DOMENIC",
                            "DOMINGO",
                            "DOMINIC",
                            "DOMINICK",
                            "DOMINIQUE",
                            "DON",
                            "DONALD",
                            "DONG",
                            "DONN",
                            "DONNELL",
                            "DONNIE",
                            "DONNY",
                            "DONOVAN",
                            "DONTE",
                            "DORIAN",
                            "DORSEY",
                            "DOUG",
                            "DOUGLAS",
                            "DOUGLASS",
                            "DOYLE",
                            "DREW",
                            "DUANE",
                            "DUDLEY",
                            "DUNCAN",
                            "DUSTIN",
                            "DUSTY",
                            "DWAIN",
                            "DWAYNE",
                            "DWIGHT",
                            "DYLAN",
                            "EARL",
                            "EARLE",
                            "EARNEST",
                            "ED",
                            "EDDIE",
                            "EDDY",
                            "EDGAR",
                            "EDGARDO",
                            "EDISON",
                            "EDMOND",
                            "EDMUND",
                            "EDMUNDO",
                            "EDUARDO",
                            "EDWARD",
                            "EDWARDO",
                            "EDWIN",
                            "EFRAIN",
                            "EFREN",
                            "ELBERT",
                            "ELDEN",
                            "ELDON",
                            "ELDRIDGE",
                            "ELI",
                            "ELIAS",
                            "ELIJAH",
                            "ELISEO",
                            "ELISHA",
                            "ELLIOT",
                            "ELLIOTT",
                            "ELLIS",
                            "ELLSWORTH",
                            "ELMER",
                            "ELMO",
                            "ELOY",
                            "ELROY",
                            "ELTON",
                            "ELVIN",
                            "ELVIS",
                            "ELWOOD",
                            "EMANUEL",
                            "EMERSON",
                            "EMERY",
                            "EMIL",
                            "EMILE",
                            "EMILIO",
                            "EMMANUEL",
                            "EMMETT",
                            "EMMITT",
                            "EMORY",
                            "ENOCH",
                            "ENRIQUE",
                            "ERASMO",
                            "ERIC",
                            "ERICH",
                            "ERICK",
                            "ERIK",
                            "ERIN",
                            "ERNEST",
                            "ERNESTO",
                            "ERNIE",
                            "ERROL",
                            "ERVIN",
                            "ERWIN",
                            "ESTEBAN",
                            "ETHAN",
                            "EUGENE",
                            "EUGENIO",
                            "EUSEBIO",
                            "EVAN",
                            "EVERETT",
                            "EVERETTE",
                            "EZEKIEL",
                            "EZEQUIEL",
                            "EZRA",
                            "FABIAN",
                            "FAUSTINO",
                            "FAUSTO",
                            "FEDERICO",
                            "FELIPE",
                            "FELIX",
                            "FELTON",
                            "FERDINAND",
                            "FERMIN",
                            "FERNANDO",
                            "FIDEL",
                            "FILIBERTO",
                            "FLETCHER",
                            "FLORENCIO",
                            "FLORENTINO",
                            "FLOYD",
                            "FOREST",
                            "FORREST",
                            "FOSTER",
                            "FRANCES",
                            "FRANCESCO",
                            "FRANCIS",
                            "FRANCISCO",
                            "FRANK",
                            "FRANKIE",
                            "FRANKLIN",
                            "FRANKLYN",
                            "FRED",
                            "FREDDIE",
                            "FREDDY",
                            "FREDERIC",
                            "FREDERICK",
                            "FREDRIC",
                            "FREDRICK",
                            "FREEMAN",
                            "FRITZ",
                            "GABRIEL",
                            "GAIL",
                            "GALE",
                            "GALEN",
                            "GARFIELD",
                            "GARLAND",
                            "GARRET",
                            "GARRETT",
                            "GARRY",
                            "GARTH",
                            "GARY",
                            "GASTON",
                            "GAVIN",
                            "GAYLE",
                            "GAYLORD",
                            "GENARO",
                            "GENE",
                            "GEOFFREY",
                            "GEORGE",
                            "GERALD",
                            "GERALDO",
                            "GERARD",
                            "GERARDO",
                            "GERMAN",
                            "GERRY",
                            "GIL",
                            "GILBERT",
                            "GILBERTO",
                            "GINO",
                            "GIOVANNI",
                            "GIUSEPPE",
                            "GLEN",
                            "GLENN",
                            "GONZALO",
                            "GORDON",
                            "GRADY",
                            "GRAHAM",
                            "GRAIG",
                            "GRANT",
                            "GRANVILLE",
                            "GREG",
                            "GREGG",
                            "GREGORIO",
                            "GREGORY",
                            "GROVER",
                            "GUADALUPE",
                            "GUILLERMO",
                            "GUS",
                            "GUSTAVO",
                            "GUY",
                            "HAI",
                            "HAL",
                            "HANK",
                            "HANS",
                            "HARLAN",
                            "HARLAND",
                            "HARLEY",
                            "HAROLD",
                            "HARRIS",
                            "HARRISON",
                            "HARRY",
                            "HARVEY",
                            "HASSAN",
                            "HAYDEN",
                            "HAYWOOD",
                            "HEATH",
                            "HECTOR",
                            "HENRY",
                            "HERB",
                            "HERBERT",
                            "HERIBERTO",
                            "HERMAN",
                            "HERSCHEL",
                            "HERSHEL",
                            "HILARIO",
                            "HILTON",
                            "HIPOLITO",
                            "HIRAM",
                            "HOBERT",
                            "HOLLIS",
                            "HOMER",
                            "HONG",
                            "HORACE",
                            "HORACIO",
                            "HOSEA",
                            "HOUSTON",
                            "HOWARD",
                            "HOYT",
                            "HUBERT",
                            "HUEY",
                            "HUGH",
                            "HUGO",
                            "HUMBERTO",
                            "HUNG",
                            "HUNTER",
                            "HYMAN",
                            "IAN",
                            "IGNACIO",
                            "IKE",
                            "IRA",
                            "IRVIN",
                            "IRVING",
                            "IRWIN",
                            "ISAAC",
                            "ISAIAH",
                            "ISAIAS",
                            "ISIAH",
                            "ISIDRO",
                            "ISMAEL",
                            "ISRAEL",
                            "ISREAL",
                            "ISSAC",
                            "IVAN",
                            "IVORY",
                            "JACINTO",
                            "JACK",
                            "JACKIE",
                            "JACKSON",
                            "JACOB",
                            "JACQUES",
                            "JAE",
                            "JAIME",
                            "JAKE",
                            "JAMAAL",
                            "JAMAL",
                            "JAMAR",
                            "JAME",
                            "JAMEL",
                            "JAMES",
                            "JAMEY",
                            "JAMIE",
                            "JAMISON",
                            "JAN",
                            "JARED",
                            "JAROD",
                            "JARRED",
                            "JARRETT",
                            "JARROD",
                            "JARVIS",
                            "JASON",
                            "JASPER",
                            "JAVIER",
                            "JAY",
                            "JAYSON",
                            "JC",
                            "JEAN",
                            "JED",
                            "JEFF",
                            "JEFFEREY",
                            "JEFFERSON",
                            "JEFFERY",
                            "JEFFREY",
                            "JEFFRY",
                            "JERALD",
                            "JERAMY",
                            "JERE",
                            "JEREMIAH",
                            "JEREMY",
                            "JERMAINE",
                            "JEROLD",
                            "JEROME",
                            "JEROMY",
                            "JERRELL",
                            "JERROD",
                            "JERROLD",
                            "JERRY",
                            "JESS",
                            "JESSE",
                            "JESSIE",
                            "JESUS",
                            "JEWEL",
                            "JEWELL",
                            "JIM",
                            "JIMMIE",
                            "JIMMY",
                            "JOAN",
                            "JOAQUIN",
                            "JODY",
                            "JOE",
                            "JOEL",
                            "JOESPH",
                            "JOEY",
                            "JOHN",
                            "JOHNATHAN",
                            "JOHNATHON",
                            "JOHNIE",
                            "JOHNNIE",
                            "JOHNNY",
                            "JOHNSON",
                            "JON",
                            "JONAH",
                            "JONAS",
                            "JONATHAN",
                            "JONATHON",
                            "JORDAN",
                            "JORDON",
                            "JORGE",
                            "JOSE",
                            "JOSEF",
                            "JOSEPH",
                            "JOSH",
                            "JOSHUA",
                            "JOSIAH",
                            "JOSPEH",
                            "JOSUE",
                            "JUAN",
                            "JUDE",
                            "JUDSON",
                            "JULES",
                            "JULIAN",
                            "JULIO",
                            "JULIUS",
                            "JUNIOR",
                            "JUSTIN",
                            "KAREEM",
                            "KARL",
                            "KASEY",
                            "KEENAN",
                            "KEITH",
                            "KELLEY",
                            "KELLY",
                            "KELVIN",
                            "KEN",
                            "KENDALL",
                            "KENDRICK",
                            "KENETH",
                            "KENNETH",
                            "KENNITH",
                            "KENNY",
                            "KENT",
                            "KENTON",
                            "KERMIT",
                            "KERRY",
                            "KEVEN",
                            "KEVIN",
                            "KIETH",
                            "KIM",
                            "KING",
                            "KIP",
                            "KIRBY",
                            "KIRK",
                            "KOREY",
                            "KORY",
                            "KRAIG",
                            "KRIS",
                            "KRISTOFER",
                            "KRISTOPHER",
                            "KURT",
                            "KURTIS",
                            "KYLE",
                            "LACY",
                            "LAMAR",
                            "LAMONT",
                            "LANCE",
                            "LANDON",
                            "LANE",
                            "LANNY",
                            "LARRY",
                            "LAUREN",
                            "LAURENCE",
                            "LAVERN",
                            "LAVERNE",
                            "LAWERENCE",
                            "LAWRENCE",
                            "LAZARO",
                            "LEANDRO",
                            "LEE",
                            "LEIF",
                            "LEIGH",
                            "LELAND",
                            "LEMUEL",
                            "LEN",
                            "LENARD",
                            "LENNY",
                            "LEO",
                            "LEON",
                            "LEONARD",
                            "LEONARDO",
                            "LEONEL",
                            "LEOPOLDO",
                            "LEROY",
                            "LES",
                            "LESLEY",
                            "LESLIE",
                            "LESTER",
                            "LEVI",
                            "LEWIS",
                            "LINCOLN",
                            "LINDSAY",
                            "LINDSEY",
                            "LINO",
                            "LINWOOD",
                            "LIONEL",
                            "LLOYD",
                            "LOGAN",
                            "LON",
                            "LONG",
                            "LONNIE",
                            "LONNY",
                            "LOREN",
                            "LORENZO",
                            "LOU",
                            "LOUIE",
                            "LOUIS",
                            "LOWELL",
                            "LOYD",
                            "LUCAS",
                            "LUCIANO",
                            "LUCIEN",
                            "LUCIO",
                            "LUCIUS",
                            "LUIGI",
                            "LUIS",
                            "LUKE",
                            "LUPE",
                            "LUTHER",
                            "LYLE",
                            "LYMAN",
                            "LYNDON",
                            "LYNN",
                            "LYNWOOD",
                            "MAC",
                            "MACK",
                            "MAJOR",
                            "MALCOLM",
                            "MALCOM",
                            "MALIK",
                            "MAN",
                            "MANUAL",
                            "MANUEL",
                            "MARC",
                            "MARCEL",
                            "MARCELINO",
                            "MARCELLUS",
                            "MARCELO",
                            "MARCO",
                            "MARCOS",
                            "MARCUS",
                            "MARGARITO",
                            "MARIA",
                            "MARIANO",
                            "MARIO",
                            "MARION",
                            "MARK",
                            "MARKUS",
                            "MARLIN",
                            "MARLON",
                            "MARQUIS",
                            "MARSHALL",
                            "MARTIN",
                            "MARTY",
                            "MARVIN",
                            "MARY",
                            "MASON",
                            "MATHEW",
                            "MATT",
                            "MATTHEW",
                            "MAURICE",
                            "MAURICIO",
                            "MAURO",
                            "MAX",
                            "MAXIMO",
                            "MAXWELL",
                            "MAYNARD",
                            "MCKINLEY",
                            "MEL",
                            "MELVIN",
                            "MERLE",
                            "MERLIN",
                            "MERRILL",
                            "MERVIN",
                            "MICAH",
                            "MICHAEL",
                            "MICHAL",
                            "MICHALE",
                            "MICHEAL",
                            "MICHEL",
                            "MICKEY",
                            "MIGUEL",
                            "MIKE",
                            "MIKEL",
                            "MILAN",
                            "MILES",
                            "MILFORD",
                            "MILLARD",
                            "MILO",
                            "MILTON",
                            "MINH",
                            "MIQUEL",
                            "MITCH",
                            "MITCHEL",
                            "MITCHELL",
                            "MODESTO",
                            "MOHAMED",
                            "MOHAMMAD",
                            "MOHAMMED",
                            "MOISES",
                            "MONROE",
                            "MONTE",
                            "MONTY",
                            "MORGAN",
                            "MORRIS",
                            "MORTON",
                            "MOSE",
                            "MOSES",
                            "MOSHE",
                            "MURRAY",
                            "MYLES",
                            "MYRON",
                            "NAPOLEON",
                            "NATHAN",
                            "NATHANAEL",
                            "NATHANIAL",
                            "NATHANIEL",
                            "NEAL",
                            "NED",
                            "NEIL",
                            "NELSON",
                            "NESTOR",
                            "NEVILLE",
                            "NEWTON",
                            "NICHOLAS",
                            "NICK",
                            "NICKOLAS",
                            "NICKY",
                            "NICOLAS",
                            "NIGEL",
                            "NOAH",
                            "NOBLE",
                            "NOE",
                            "NOEL",
                            "NOLAN",
                            "NORBERT",
                            "NORBERTO",
                            "NORMAN",
                            "NORMAND",
                            "NORRIS",
                            "NUMBERS",
                            "OCTAVIO",
                            "ODELL",
                            "ODIS",
                            "OLEN",
                            "OLIN",
                            "OLIVER",
                            "OLLIE",
                            "OMAR",
                            "OMER",
                            "OREN",
                            "ORLANDO",
                            "ORVAL",
                            "ORVILLE",
                            "OSCAR",
                            "OSVALDO",
                            "OSWALDO",
                            "OTHA",
                            "OTIS",
                            "OTTO",
                            "OWEN",
                            "PABLO",
                            "PALMER",
                            "PARIS",
                            "PARKER",
                            "PASQUALE",
                            "PAT",
                            "PATRICIA",
                            "PATRICK",
                            "PAUL",
                            "PEDRO",
                            "PERCY",
                            "PERRY",
                            "PETE",
                            "PETER",
                            "PHIL",
                            "PHILIP",
                            "PHILLIP",
                            "PIERRE",
                            "PORFIRIO",
                            "PORTER",
                            "PRESTON",
                            "PRINCE",
                            "QUENTIN",
                            "QUINCY",
                            "QUINN",
                            "QUINTIN",
                            "QUINTON",
                            "RAFAEL",
                            "RALEIGH",
                            "RALPH",
                            "RAMIRO",
                            "RAMON",
                            "RANDAL",
                            "RANDALL",
                            "RANDELL",
                            "RANDOLPH",
                            "RANDY",
                            "RAPHAEL",
                            "RASHAD",
                            "RAUL",
                            "RAY",
                            "RAYFORD",
                            "RAYMON",
                            "RAYMOND",
                            "RAYMUNDO",
                            "REED",
                            "REFUGIO",
                            "REGGIE",
                            "REGINALD",
                            "REID",
                            "REINALDO",
                            "RENALDO",
                            "RENATO",
                            "RENE",
                            "REUBEN",
                            "REX",
                            "REY",
                            "REYES",
                            "REYNALDO",
                            "RHETT",
                            "RICARDO",
                            "RICH",
                            "RICHARD",
                            "RICHIE",
                            "RICK",
                            "RICKEY",
                            "RICKIE",
                            "RICKY",
                            "RICO",
                            "RIGOBERTO",
                            "RILEY",
                            "ROB",
                            "ROBBIE",
                            "ROBBY",
                            "ROBERT",
                            "ROBERTO",
                            "ROBIN",
                            "ROBT",
                            "ROCCO",
                            "ROCKY",
                            "ROD",
                            "RODERICK",
                            "RODGER",
                            "RODNEY",
                            "RODOLFO",
                            "RODRICK",
                            "RODRIGO",
                            "ROGELIO",
                            "ROGER",
                            "ROLAND",
                            "ROLANDO",
                            "ROLF",
                            "ROLLAND",
                            "ROMAN",
                            "ROMEO",
                            "RON",
                            "RONALD",
                            "RONNIE",
                            "RONNY",
                            "ROOSEVELT",
                            "RORY",
                            "ROSARIO",
                            "ROSCOE",
                            "ROSENDO",
                            "ROSS",
                            "ROY",
                            "ROYAL",
                            "ROYCE",
                            "RUBEN",
                            "RUBIN",
                            "RUDOLF",
                            "RUDOLPH",
                            "RUDY",
                            "RUEBEN",
                            "RUFUS",
                            "RUPERT",
                            "RUSS",
                            "RUSSEL",
                            "RUSSELL",
                            "RUSTY",
                            "RYAN",
                            "SAL",
                            "SALVADOR",
                            "SALVATORE",
                            "SAM",
                            "SAMMIE",
                            "SAMMY",
                            "SAMUAL",
                            "SAMUEL",
                            "SANDY",
                            "SANFORD",
                            "SANG",
                            "SANTIAGO",
                            "SANTO",
                            "SANTOS",
                            "SAUL",
                            "SCOT",
                            "SCOTT",
                            "SCOTTIE",
                            "SCOTTY",
                            "SEAN",
                            "SEBASTIAN",
                            "SERGIO",
                            "SETH",
                            "SEYMOUR",
                            "SHAD",
                            "SHANE",
                            "SHANNON",
                            "SHAUN",
                            "SHAWN",
                            "SHAYNE",
                            "SHELBY",
                            "SHELDON",
                            "SHELTON",
                            "SHERMAN",
                            "SHERWOOD",
                            "SHIRLEY",
                            "SHON",
                            "SID",
                            "SIDNEY",
                            "SILAS",
                            "SIMON",
                            "SOL",
                            "SOLOMON",
                            "SON",
                            "SONNY",
                            "SPENCER",
                            "STACEY",
                            "STACY",
                            "STAN",
                            "STANFORD",
                            "STANLEY",
                            "STANTON",
                            "STEFAN",
                            "STEPHAN",
                            "STEPHEN",
                            "STERLING",
                            "STEVE",
                            "STEVEN",
                            "STEVIE",
                            "STEWART",
                            "STUART",
                            "SUNG",
                            "SYDNEY",
                            "SYLVESTER",
                            "TAD",
                            "TANNER",
                            "TAYLOR",
                            "TED",
                            "TEDDY",
                            "TEODORO",
                            "TERENCE",
                            "TERRANCE",
                            "TERRELL",
                            "TERRENCE",
                            "TERRY",
                            "THAD",
                            "THADDEUS",
                            "THANH",
                            "THEO",
                            "THEODORE",
                            "THERON",
                            "THOMAS",
                            "THURMAN",
                            "TIM",
                            "TIMMY",
                            "TIMOTHY",
                            "TITUS",
                            "TOBIAS",
                            "TOBY",
                            "TOD",
                            "TODD",
                            "TOM",
                            "TOMAS",
                            "TOMMIE",
                            "TOMMY",
                            "TONEY",
                            "TONY",
                            "TORY",
                            "TRACEY",
                            "TRACY",
                            "TRAVIS",
                            "TRENT",
                            "TRENTON",
                            "TREVOR",
                            "TREY",
                            "TRINIDAD",
                            "TRISTAN",
                            "TROY",
                            "TRUMAN",
                            "TUAN",
                            "TY",
                            "TYLER",
                            "TYREE",
                            "TYRELL",
                            "TYRON",
                            "TYRONE",
                            "TYSON",
                            "ULYSSES",
                            "VAL",
                            "VALENTIN",
                            "VALENTINE",
                            "VAN",
                            "VANCE",
                            "VAUGHN",
                            "VERN",
                            "VERNON",
                            "VICENTE",
                            "VICTOR",
                            "VINCE",
                            "VINCENT",
                            "VINCENZO",
                            "VIRGIL",
                            "VIRGILIO",
                            "VITO",
                            "VON",
                            "WADE",
                            "WALDO",
                            "WALKER",
                            "WALLACE",
                            "WALLY",
                            "WALTER",
                            "WALTON",
                            "WARD",
                            "WARNER",
                            "WARREN",
                            "WAYLON",
                            "WAYNE",
                            "WELDON",
                            "WENDELL",
                            "WERNER",
                            "WES",
                            "WESLEY",
                            "WESTON",
                            "WHITNEY",
                            "WILBER",
                            "WILBERT",
                            "WILBUR",
                            "WILBURN",
                            "WILEY",
                            "WILFORD",
                            "WILFRED",
                            "WILFREDO",
                            "WILL",
                            "WILLARD",
                            "WILLIAM",
                            "WILLIAMS",
                            "WILLIAN",
                            "WILLIE",
                            "WILLIS",
                            "WILLY",
                            "WILMER",
                            "WILSON",
                            "WILTON",
                            "WINFORD",
                            "WINFRED",
                            "WINSTON",
                            "WM",
                            "WOODROW",
                            "WYATT",
                            "XAVIER",
                            "YONG",
                            "YOUNG",
                            "ZACHARIAH",
                            "ZACHARY",
                            "ZACHERY",
                            "ZACK",
                            "ZACKARY",
                            "ZANE",
                            "MARY",
                            "PATRICIA",
                            "LINDA",
                            "BARBARA",
                            "ELIZABETH",
                            "JENNIFER",
                            "MARIA",
                            "SUSAN",
                            "MARGARET",
                            "DOROTHY",
                            "LISA",
                            "NANCY",
                            "KAREN",
                            "BETTY",
                            "HELEN",
                            "SANDRA",
                            "DONNA",
                            "CAROL",
                            "RUTH",
                            "SHARON",
                            "MICHELLE",
                            "LAURA",
                            "SARAH",
                            "KIMBERLY",
                            "DEBORAH",
                            "JESSICA",
                            "SHIRLEY",
                            "CYNTHIA",
                            "ANGELA",
                            "MELISSA",
                            "BRENDA",
                            "AMY",
                            "ANNA",
                            "REBECCA",
                            "VIRGINIA",
                            "KATHLEEN",
                            "PAMELA",
                            "MARTHA",
                            "DEBRA",
                            "AMANDA",
                            "STEPHANIE",
                            "CAROLYN",
                            "CHRISTINE",
                            "MARIE",
                            "JANET",
                            "CATHERINE",
                            "FRANCES",
                            "ANN",
                            "JOYCE",
                            "DIANE",
                            "ALICE",
                            "JULIE",
                            "HEATHER",
                            "TERESA",
                            "DORIS",
                            "GLORIA",
                            "EVELYN",
                            "JEAN",
                            "CHERYL",
                            "MILDRED",
                            "KATHERINE",
                            "JOAN",
                            "ASHLEY",
                            "JUDITH",
                            "ROSE",
                            "JANICE",
                            "KELLY",
                            "NICOLE",
                            "JUDY",
                            "CHRISTINA",
                            "KATHY",
                            "THERESA",
                            "BEVERLY",
                            "DENISE",
                            "TAMMY",
                            "IRENE",
                            "JANE",
                            "LORI",
                            "RACHEL",
                            "MARILYN",
                            "ANDREA",
                            "KATHRYN",
                            "LOUISE",
                            "SARA",
                            "ANNE",
                            "JACQUELINE",
                            "WANDA",
                            "BONNIE",
                            "JULIA",
                            "RUBY",
                            "LOIS",
                            "TINA",
                            "PHYLLIS",
                            "NORMA",
                            "PAULA",
                            "DIANA",
                            "ANNIE",
                            "LILLIAN",
                            "EMILY",
                            "ROBIN",
                            "PEGGY",
                            "CRYSTAL",
                            "GLADYS",
                            "RITA",
                            "DAWN",
                            "CONNIE",
                            "FLORENCE",
                            "TRACY",
                            "EDNA",
                            "TIFFANY",
                            "CARMEN",
                            "ROSA",
                            "CINDY",
                            "GRACE",
                            "WENDY",
                            "VICTORIA",
                            "EDITH",
                            "KIM",
                            "SHERRY",
                            "SYLVIA",
                            "JOSEPHINE",
                            "THELMA",
                            "SHANNON",
                            "SHEILA",
                            "ETHEL",
                            "ELLEN",
                            "ELAINE",
                            "MARJORIE",
                            "CARRIE",
                            "CHARLOTTE",
                            "MONICA",
                            "ESTHER",
                            "PAULINE",
                            "EMMA",
                            "JUANITA",
                            "ANITA",
                            "RHONDA",
                            "HAZEL",
                            "AMBER",
                            "EVA",
                            "DEBBIE",
                            "APRIL",
                            "LESLIE",
                            "CLARA",
                            "LUCILLE",
                            "JAMIE",
                            "JOANNE",
                            "ELEANOR",
                            "VALERIE",
                            "DANIELLE",
                            "MEGAN",
                            "ALICIA",
                            "SUZANNE",
                            "MICHELE",
                            "GAIL",
                            "BERTHA",
                            "DARLENE",
                            "VERONICA",
                            "JILL",
                            "ERIN",
                            "GERALDINE",
                            "LAUREN",
                            "CATHY",
                            "JOANN",
                            "LORRAINE",
                            "LYNN",
                            "SALLY",
                            "REGINA",
                            "ERICA",
                            "BEATRICE",
                            "DOLORES",
                            "BERNICE",
                            "AUDREY",
                            "YVONNE",
                            "ANNETTE",
                            "JUNE",
                            "SAMANTHA",
                            "MARION",
                            "DANA",
                            "STACY",
                            "ANA",
                            "RENEE",
                            "IDA",
                            "VIVIAN",
                            "ROBERTA",
                            "HOLLY",
                            "BRITTANY",
                            "MELANIE",
                            "LORETTA",
                            "YOLANDA",
                            "JEANETTE",
                            "LAURIE",
                            "KATIE",
                            "KRISTEN",
                            "VANESSA",
                            "ALMA",
                            "SUE",
                            "ELSIE",
                            "BETH",
                            "JEANNE",
                            "VICKI",
                            "CARLA",
                            "TARA",
                            "ROSEMARY",
                            "EILEEN",
                            "TERRI",
                            "GERTRUDE",
                            "LUCY",
                            "TONYA",
                            "ELLA",
                            "STACEY",
                            "WILMA",
                            "GINA",
                            "KRISTIN",
                            "JESSIE",
                            "NATALIE",
                            "AGNES",
                            "VERA",
                            "WILLIE",
                            "CHARLENE",
                            "BESSIE",
                            "DELORES",
                            "MELINDA",
                            "PEARL",
                            "ARLENE",
                            "MAUREEN",
                            "COLLEEN",
                            "ALLISON",
                            "TAMARA",
                            "JOY",
                            "GEORGIA",
                            "CONSTANCE",
                            "LILLIE",
                            "CLAUDIA",
                            "JACKIE",
                            "MARCIA",
                            "TANYA",
                            "NELLIE",
                            "MINNIE",
                            "MARLENE",
                            "HEIDI",
                            "GLENDA",
                            "LYDIA",
                            "VIOLA",
                            "COURTNEY",
                            "MARIAN",
                            "STELLA",
                            "CAROLINE",
                            "DORA",
                            "JO",
                            "VICKIE",
                            "MATTIE",
                            "TERRY",
                            "MAXINE",
                            "IRMA",
                            "MABEL",
                            "MARSHA",
                            "MYRTLE",
                            "LENA",
                            "CHRISTY",
                            "DEANNA",
                            "PATSY",
                            "HILDA",
                            "GWENDOLYN",
                            "JENNIE",
                            "NORA",
                            "MARGIE",
                            "NINA",
                            "CASSANDRA",
                            "LEAH",
                            "PENNY",
                            "KAY",
                            "PRISCILLA",
                            "NAOMI",
                            "CAROLE",
                            "BRANDY",
                            "OLGA",
                            "BILLIE",
                            "DIANNE",
                            "TRACEY",
                            "LEONA",
                            "JENNY",
                            "FELICIA",
                            "SONIA",
                            "MIRIAM",
                            "VELMA",
                            "BECKY",
                            "BOBBIE",
                            "VIOLET",
                            "KRISTINA",
                            "TONI",
                            "MISTY",
                            "MAE",
                            "SHELLY",
                            "DAISY",
                            "RAMONA",
                            "SHERRI",
                            "ERIKA",
                            "KATRINA",
                            "CLAIRE",
                            "LINDSEY",
                            "LINDSAY",
                            "GENEVA",
                            "GUADALUPE",
                            "BELINDA",
                            "MARGARITA",
                            "SHERYL",
                            "CORA",
                            "FAYE",
                            "ADA",
                            "NATASHA",
                            "SABRINA",
                            "ISABEL",
                            "MARGUERITE",
                            "HATTIE",
                            "HARRIET",
                            "MOLLY",
                            "CECILIA",
                            "KRISTI",
                            "BRANDI",
                            "BLANCHE",
                            "SANDY",
                            "ROSIE",
                            "JOANNA",
                            "IRIS",
                            "EUNICE",
                            "ANGIE",
                            "INEZ",
                            "LYNDA",
                            "MADELINE",
                            "AMELIA",
                            "ALBERTA",
                            "GENEVIEVE",
                            "MONIQUE",
                            "JODI",
                            "JANIE",
                            "MAGGIE",
                            "KAYLA",
                            "SONYA",
                            "JAN",
                            "LEE",
                            "KRISTINE",
                            "CANDACE",
                            "FANNIE",
                            "MARYANN",
                            "OPAL",
                            "ALISON",
                            "YVETTE",
                            "MELODY",
                            "LUZ",
                            "SUSIE",
                            "OLIVIA",
                            "FLORA",
                            "SHELLEY",
                            "KRISTY",
                            "MAMIE",
                            "LULA",
                            "LOLA",
                            "VERNA",
                            "BEULAH",
                            "ANTOINETTE",
                            "CANDICE",
                            "JUANA",
                            "JEANNETTE",
                            "PAM",
                            "KELLI",
                            "HANNAH",
                            "WHITNEY",
                            "BRIDGET",
                            "KARLA",
                            "CELIA",
                            "LATOYA",
                            "PATTY",
                            "SHELIA",
                            "GAYLE",
                            "DELLA",
                            "VICKY",
                            "LYNNE",
                            "SHERI",
                            "MARIANNE",
                            "KARA",
                            "JACQUELYN",
                            "ERMA",
                            "BLANCA",
                            "MYRA",
                            "LETICIA",
                            "PAT",
                            "KRISTA",
                            "ROXANNE",
                            "ANGELICA",
                            "JOHNNIE",
                            "ROBYN",
                            "FRANCIS",
                            "ADRIENNE",
                            "ROSALIE",
                            "ALEXANDRA",
                            "BROOKE",
                            "BETHANY",
                            "SADIE",
                            "BERNADETTE",
                            "TRACI",
                            "JODY",
                            "KENDRA",
                            "JASMINE",
                            "NICHOLE",
                            "RACHAEL",
                            "CHELSEA",
                            "MABLE",
                            "ERNESTINE",
                            "MURIEL",
                            "MARCELLA",
                            "ELENA",
                            "KRYSTAL",
                            "ANGELINA",
                            "NADINE",
                            "KARI",
                            "ESTELLE",
                            "DIANNA",
                            "PAULETTE",
                            "LORA",
                            "MONA",
                            "DOREEN",
                            "ROSEMARIE",
                            "ANGEL",
                            "DESIREE",
                            "ANTONIA",
                            "HOPE",
                            "GINGER",
                            "JANIS",
                            "BETSY",
                            "CHRISTIE",
                            "FREDA",
                            "MERCEDES",
                            "MEREDITH",
                            "LYNETTE",
                            "TERI",
                            "CRISTINA",
                            "EULA",
                            "LEIGH",
                            "MEGHAN",
                            "SOPHIA",
                            "ELOISE",
                            "ROCHELLE",
                            "GRETCHEN",
                            "CECELIA",
                            "RAQUEL",
                            "HENRIETTA",
                            "ALYSSA",
                            "JANA",
                            "KELLEY",
                            "GWEN",
                            "KERRY",
                            "JENNA",
                            "TRICIA",
                            "LAVERNE",
                            "OLIVE",
                            "ALEXIS",
                            "TASHA",
                            "SILVIA",
                            "ELVIRA",
                            "CASEY",
                            "DELIA",
                            "SOPHIE",
                            "KATE",
                            "PATTI",
                            "LORENA",
                            "KELLIE",
                            "SONJA",
                            "LILA",
                            "LANA",
                            "DARLA",
                            "MAY",
                            "MINDY",
                            "ESSIE",
                            "MANDY",
                            "LORENE",
                            "ELSA",
                            "JOSEFINA",
                            "JEANNIE",
                            "MIRANDA",
                            "DIXIE",
                            "LUCIA",
                            "MARTA",
                            "FAITH",
                            "LELA",
                            "JOHANNA",
                            "SHARI",
                            "CAMILLE",
                            "TAMI",
                            "SHAWNA",
                            "ELISA",
                            "EBONY",
                            "MELBA",
                            "ORA",
                            "NETTIE",
                            "TABITHA",
                            "OLLIE",
                            "JAIME",
                            "WINIFRED",
                            "KRISTIE",
                            "MARINA",
                            "ALISHA",
                            "AIMEE",
                            "RENA",
                            "MYRNA",
                            "MARLA",
                            "TAMMIE",
                            "LATASHA",
                            "BONITA",
                            "PATRICE",
                            "RONDA",
                            "SHERRIE",
                            "ADDIE",
                            "FRANCINE",
                            "DELORIS",
                            "STACIE",
                            "ADRIANA",
                            "CHERI",
                            "SHELBY",
                            "ABIGAIL",
                            "CELESTE",
                            "JEWEL",
                            "CARA",
                            "ADELE",
                            "REBEKAH",
                            "LUCINDA",
                            "DORTHY",
                            "CHRIS",
                            "EFFIE",
                            "TRINA",
                            "REBA",
                            "SHAWN",
                            "SALLIE",
                            "AURORA",
                            "LENORA",
                            "ETTA",
                            "LOTTIE",
                            "KERRI",
                            "TRISHA",
                            "NIKKI",
                            "ESTELLA",
                            "FRANCISCA",
                            "JOSIE",
                            "TRACIE",
                            "MARISSA",
                            "KARIN",
                            "BRITTNEY",
                            "JANELLE",
                            "LOURDES",
                            "LAUREL",
                            "HELENE",
                            "FERN",
                            "ELVA",
                            "CORINNE",
                            "KELSEY",
                            "INA",
                            "BETTIE",
                            "ELISABETH",
                            "AIDA",
                            "CAITLIN",
                            "INGRID",
                            "IVA",
                            "EUGENIA",
                            "CHRISTA",
                            "GOLDIE",
                            "CASSIE",
                            "MAUDE",
                            "JENIFER",
                            "THERESE",
                            "FRANKIE",
                            "DENA",
                            "LORNA",
                            "JANETTE",
                            "LATONYA",
                            "CANDY",
                            "MORGAN",
                            "CONSUELO",
                            "TAMIKA",
                            "ROSETTA",
                            "DEBORA",
                            "CHERIE",
                            "POLLY",
                            "DINA",
                            "JEWELL",
                            "FAY",
                            "JILLIAN",
                            "DOROTHEA",
                            "NELL",
                            "TRUDY",
                            "ESPERANZA",
                            "PATRICA",
                            "KIMBERLEY",
                            "SHANNA",
                            "HELENA",
                            "CAROLINA",
                            "CLEO",
                            "STEFANIE",
                            "ROSARIO",
                            "OLA",
                            "JANINE",
                            "MOLLIE",
                            "LUPE",
                            "ALISA",
                            "LOU",
                            "MARIBEL",
                            "SUSANNE",
                            "BETTE",
                            "SUSANA",
                            "ELISE",
                            "CECILE",
                            "ISABELLE",
                            "LESLEY",
                            "JOCELYN",
                            "PAIGE",
                            "JONI",
                            "RACHELLE",
                            "LEOLA",
                            "DAPHNE",
                            "ALTA",
                            "ESTER",
                            "PETRA",
                            "GRACIELA",
                            "IMOGENE",
                            "JOLENE",
                            "KEISHA",
                            "LACEY",
                            "GLENNA",
                            "GABRIELA",
                            "KERI",
                            "URSULA",
                            "LIZZIE",
                            "KIRSTEN",
                            "SHANA",
                            "ADELINE",
                            "MAYRA",
                            "JAYNE",
                            "JACLYN",
                            "GRACIE",
                            "SONDRA",
                            "CARMELA",
                            "MARISA",
                            "ROSALIND",
                            "CHARITY",
                            "TONIA",
                            "BEATRIZ",
                            "MARISOL",
                            "CLARICE",
                            "JEANINE",
                            "SHEENA",
                            "ANGELINE",
                            "FRIEDA",
                            "LILY",
                            "ROBBIE",
                            "SHAUNA",
                            "MILLIE",
                            "CLAUDETTE",
                            "CATHLEEN",
                            "ANGELIA",
                            "GABRIELLE",
                            "AUTUMN",
                            "KATHARINE",
                            "SUMMER",
                            "JODIE",
                            "STACI",
                            "LEA",
                            "CHRISTI",
                            "JIMMIE",
                            "JUSTINE",
                            "ELMA",
                            "LUELLA",
                            "MARGRET",
                            "DOMINIQUE",
                            "SOCORRO",
                            "RENE",
                            "MARTINA",
                            "MARGO",
                            "MAVIS",
                            "CALLIE",
                            "BOBBI",
                            "MARITZA",
                            "LUCILE",
                            "LEANNE",
                            "JEANNINE",
                            "DEANA",
                            "AILEEN",
                            "LORIE",
                            "LADONNA",
                            "WILLA",
                            "MANUELA",
                            "GALE",
                            "SELMA",
                            "DOLLY",
                            "SYBIL",
                            "ABBY",
                            "LARA",
                            "DALE",
                            "IVY",
                            "DEE",
                            "WINNIE",
                            "MARCY",
                            "LUISA",
                            "JERI",
                            "MAGDALENA",
                            "OFELIA",
                            "MEAGAN",
                            "AUDRA",
                            "MATILDA",
                            "LEILA",
                            "CORNELIA",
                            "BIANCA",
                            "SIMONE",
                            "BETTYE",
                            "RANDI",
                            "VIRGIE",
                            "LATISHA",
                            "BARBRA",
                            "GEORGINA",
                            "ELIZA",
                            "LEANN",
                            "BRIDGETTE",
                            "RHODA",
                            "HALEY",
                            "ADELA",
                            "NOLA",
                            "BERNADINE",
                            "FLOSSIE",
                            "ILA",
                            "GRETA",
                            "RUTHIE",
                            "NELDA",
                            "MINERVA",
                            "LILLY",
                            "TERRIE",
                            "LETHA",
                            "HILARY",
                            "ESTELA",
                            "VALARIE",
                            "BRIANNA",
                            "ROSALYN",
                            "EARLINE",
                            "CATALINA",
                            "AVA",
                            "MIA",
                            "CLARISSA",
                            "LIDIA",
                            "CORRINE",
                            "ALEXANDRIA",
                            "CONCEPCION",
                            "TIA",
                            "SHARRON",
                            "RAE",
                            "DONA",
                            "ERICKA",
                            "JAMI",
                            "ELNORA",
                            "CHANDRA",
                            "LENORE",
                            "NEVA",
                            "MARYLOU",
                            "MELISA",
                            "TABATHA",
                            "SERENA",
                            "AVIS",
                            "ALLIE",
                            "SOFIA",
                            "JEANIE",
                            "ODESSA",
                            "NANNIE",
                            "HARRIETT",
                            "LORAINE",
                            "PENELOPE",
                            "MILAGROS",
                            "EMILIA",
                            "BENITA",
                            "ALLYSON",
                            "ASHLEE",
                            "TANIA",
                            "TOMMIE",
                            "ESMERALDA",
                            "KARINA",
                            "EVE",
                            "PEARLIE",
                            "ZELMA",
                            "MALINDA",
                            "NOREEN",
                            "TAMEKA",
                            "SAUNDRA",
                            "HILLARY",
                            "AMIE",
                            "ALTHEA",
                            "ROSALINDA",
                            "JORDAN",
                            "LILIA",
                            "ALANA",
                            "GAY",
                            "CLARE",
                            "ALEJANDRA",
                            "ELINOR",
                            "MICHAEL",
                            "LORRIE",
                            "JERRI",
                            "DARCY",
                            "EARNESTINE",
                            "CARMELLA",
                            "TAYLOR",
                            "NOEMI",
                            "MARCIE",
                            "LIZA",
                            "ANNABELLE",
                            "LOUISA",
                            "EARLENE",
                            "MALLORY",
                            "CARLENE",
                            "NITA",
                            "SELENA",
                            "TANISHA",
                            "KATY",
                            "JULIANNE",
                            "JOHN",
                            "LAKISHA",
                            "EDWINA",
                            "MARICELA",
                            "MARGERY",
                            "KENYA",
                            "DOLLIE",
                            "ROXIE",
                            "ROSLYN",
                            "KATHRINE",
                            "NANETTE",
                            "CHARMAINE",
                            "LAVONNE",
                            "ILENE",
                            "KRIS",
                            "TAMMI",
                            "SUZETTE",
                            "CORINE",
                            "KAYE",
                            "JERRY",
                            "MERLE",
                            "CHRYSTAL",
                            "LINA",
                            "DEANNE",
                            "LILIAN",
                            "JULIANA",
                            "ALINE",
                            "LUANN",
                            "KASEY",
                            "MARYANNE",
                            "EVANGELINE",
                            "COLETTE",
                            "MELVA",
                            "LAWANDA",
                            "YESENIA",
                            "NADIA",
                            "MADGE",
                            "KATHIE",
                            "EDDIE",
                            "OPHELIA",
                            "VALERIA",
                            "NONA",
                            "MITZI",
                            "MARI",
                            "GEORGETTE",
                            "CLAUDINE",
                            "FRAN",
                            "ALISSA",
                            "ROSEANN",
                            "LAKEISHA",
                            "SUSANNA",
                            "REVA",
                            "DEIDRE",
                            "CHASITY",
                            "SHEREE",
                            "CARLY",
                            "JAMES",
                            "ELVIA",
                            "ALYCE",
                            "DEIRDRE",
                            "GENA",
                            "BRIANA",
                            "ARACELI",
                            "KATELYN",
                            "ROSANNE",
                            "WENDI",
                            "TESSA",
                            "BERTA",
                            "MARVA",
                            "IMELDA",
                            "MARIETTA",
                            "MARCI",
                            "LEONOR",
                            "ARLINE",
                            "SASHA",
                            "MADELYN",
                            "JANNA",
                            "JULIETTE",
                            "DEENA",
                            "AURELIA",
                            "JOSEFA",
                            "AUGUSTA",
                            "LILIANA",
                            "YOUNG",
                            "CHRISTIAN",
                            "LESSIE",
                            "AMALIA",
                            "SAVANNAH",
                            "ANASTASIA",
                            "VILMA",
                            "NATALIA",
                            "ROSELLA",
                            "LYNNETTE",
                            "CORINA",
                            "ALFREDA",
                            "LEANNA",
                            "CAREY",
                            "AMPARO",
                            "COLEEN",
                            "TAMRA",
                            "AISHA",
                            "WILDA",
                            "KARYN",
                            "CHERRY",
                            "QUEEN",
                            "MAURA",
                            "MAI",
                            "EVANGELINA",
                            "ROSANNA",
                            "HALLIE",
                            "ERNA",
                            "ENID",
                            "MARIANA",
                            "LACY",
                            "JULIET",
                            "JACKLYN",
                            "FREIDA",
                            "MADELEINE",
                            "MARA",
                            "HESTER",
                            "CATHRYN",
                            "LELIA",
                            "CASANDRA",
                            "BRIDGETT",
                            "ANGELITA",
                            "JANNIE",
                            "DIONNE",
                            "ANNMARIE",
                            "KATINA",
                            "BERYL",
                            "PHOEBE",
                            "MILLICENT",
                            "KATHERYN",
                            "DIANN",
                            "CARISSA",
                            "MARYELLEN",
                            "LIZ",
                            "LAURI",
                            "HELGA",
                            "GILDA",
                            "ADRIAN",
                            "RHEA",
                            "MARQUITA",
                            "HOLLIE",
                            "TISHA",
                            "TAMERA",
                            "ANGELIQUE",
                            "FRANCESCA",
                            "BRITNEY",
                            "KAITLIN",
                            "LOLITA",
                            "FLORINE",
                            "ROWENA",
                            "REYNA",
                            "TWILA",
                            "FANNY",
                            "JANELL",
                            "INES",
                            "CONCETTA",
                            "BERTIE",
                            "ALBA",
                            "BRIGITTE",
                            "ALYSON",
                            "VONDA",
                            "PANSY",
                            "ELBA",
                            "NOELLE",
                            "LETITIA",
                            "KITTY",
                            "DEANN",
                            "BRANDIE",
                            "LOUELLA",
                            "LETA",
                            "FELECIA",
                            "SHARLENE",
                            "LESA",
                            "BEVERLEY",
                            "ROBERT",
                            "ISABELLA",
                            "HERMINIA",
                            "TERRA",
                            "CELINA"};

            _FirstNames = new Collection<string>(fnames);

            string[] lnames = {"SMITH",
                                "JOHNSON",
                                "WILLIAMS",
                                "JONES",
                                "BROWN",
                                "DAVIS",
                                "MILLER",
                                "WILSON",
                                "MOORE",
                                "TAYLOR",
                                "ANDERSON",
                                "THOMAS",
                                "JACKSON",
                                "WHITE",
                                "HARRIS",
                                "MARTIN",
                                "THOMPSON",
                                "GARCIA",
                                "MARTINEZ",
                                "ROBINSON",
                                "CLARK",
                                "RODRIGUEZ",
                                "LEWIS",
                                "LEE",
                                "WALKER",
                                "HALL",
                                "ALLEN",
                                "YOUNG",
                                "HERNANDEZ",
                                "KING",
                                "WRIGHT",
                                "LOPEZ",
                                "HILL",
                                "SCOTT",
                                "GREEN",
                                "ADAMS",
                                "BAKER",
                                "GONZALEZ",
                                "NELSON",
                                "CARTER",
                                "MITCHELL",
                                "PEREZ",
                                "ROBERTS",
                                "TURNER",
                                "PHILLIPS",
                                "CAMPBELL",
                                "PARKER",
                                "EVANS",
                                "EDWARDS",
                                "COLLINS",
                                "STEWART",
                                "SANCHEZ",
                                "MORRIS",
                                "ROGERS",
                                "REED",
                                "COOK",
                                "MORGAN",
                                "BELL",
                                "MURPHY",
                                "BAILEY",
                                "RIVERA",
                                "COOPER",
                                "RICHARDSON",
                                "COX",
                                "HOWARD",
                                "WARD",
                                "TORRES",
                                "PETERSON",
                                "GRAY",
                                "RAMIREZ",
                                "JAMES",
                                "WATSON",
                                "BROOKS",
                                "KELLY",
                                "SANDERS",
                                "PRICE",
                                "BENNETT",
                                "WOOD",
                                "BARNES",
                                "ROSS",
                                "HENDERSON",
                                "COLEMAN",
                                "JENKINS",
                                "PERRY",
                                "POWELL",
                                "LONG",
                                "PATTERSON",
                                "HUGHES",
                                "FLORES",
                                "WASHINGTON",
                                "BUTLER",
                                "SIMMONS",
                                "FOSTER",
                                "GONZALES",
                                "BRYANT",
                                "ALEXANDER",
                                "RUSSELL",
                                "GRIFFIN",
                                "DIAZ",
                                "HAYES",
                                "MYERS",
                                "FORD",
                                "HAMILTON",
                                "GRAHAM",
                                "SULLIVAN",
                                "WALLACE",
                                "WOODS",
                                "COLE",
                                "WEST",
                                "JORDAN",
                                "OWENS",
                                "REYNOLDS",
                                "FISHER",
                                "ELLIS",
                                "HARRISON",
                                "GIBSON",
                                "MCDONALD",
                                "CRUZ",
                                "MARSHALL",
                                "ORTIZ",
                                "GOMEZ",
                                "MURRAY",
                                "FREEMAN",
                                "WELLS",
                                "WEBB",
                                "SIMPSON",
                                "STEVENS",
                                "TUCKER",
                                "PORTER",
                                "HUNTER",
                                "HICKS",
                                "CRAWFORD",
                                "HENRY",
                                "BOYD",
                                "MASON",
                                "MORALES",
                                "KENNEDY",
                                "WARREN",
                                "DIXON",
                                "RAMOS",
                                "REYES",
                                "BURNS",
                                "GORDON",
                                "SHAW",
                                "HOLMES",
                                "RICE",
                                "ROBERTSON",
                                "HUNT",
                                "BLACK",
                                "DANIELS",
                                "PALMER",
                                "MILLS",
                                "NICHOLS",
                                "GRANT",
                                "KNIGHT",
                                "FERGUSON",
                                "ROSE",
                                "STONE",
                                "HAWKINS",
                                "DUNN",
                                "PERKINS",
                                "HUDSON",
                                "SPENCER",
                                "GARDNER",
                                "STEPHENS",
                                "PAYNE",
                                "PIERCE",
                                "BERRY",
                                "MATTHEWS",
                                "ARNOLD",
                                "WAGNER",
                                "WILLIS",
                                "RAY",
                                "WATKINS",
                                "OLSON",
                                "CARROLL",
                                "DUNCAN",
                                "SNYDER",
                                "HART",
                                "CUNNINGHAM",
                                "BRADLEY",
                                "LANE",
                                "ANDREWS",
                                "RUIZ",
                                "HARPER",
                                "FOX",
                                "RILEY",
                                "ARMSTRONG",
                                "CARPENTER",
                                "WEAVER",
                                "GREENE",
                                "LAWRENCE",
                                "ELLIOTT",
                                "CHAVEZ",
                                "SIMS",
                                "AUSTIN",
                                "PETERS",
                                "KELLEY",
                                "FRANKLIN",
                                "LAWSON",
                                "FIELDS",
                                "GUTIERREZ",
                                "RYAN",
                                "SCHMIDT",
                                "CARR",
                                "VASQUEZ",
                                "CASTILLO",
                                "WHEELER",
                                "CHAPMAN",
                                "OLIVER",
                                "MONTGOMERY",
                                "RICHARDS",
                                "WILLIAMSON",
                                "JOHNSTON",
                                "BANKS",
                                "MEYER",
                                "BISHOP",
                                "MCCOY",
                                "HOWELL",
                                "ALVAREZ",
                                "MORRISON",
                                "HANSEN",
                                "FERNANDEZ",
                                "GARZA",
                                "HARVEY",
                                "LITTLE",
                                "BURTON",
                                "STANLEY",
                                "NGUYEN",
                                "GEORGE",
                                "JACOBS",
                                "REID",
                                "KIM",
                                "FULLER",
                                "LYNCH",
                                "DEAN",
                                "GILBERT",
                                "GARRETT",
                                "ROMERO",
                                "WELCH",
                                "LARSON",
                                "FRAZIER",
                                "BURKE",
                                "HANSON",
                                "DAY",
                                "MENDOZA",
                                "MORENO",
                                "BOWMAN",
                                "MEDINA",
                                "FOWLER",
                                "BREWER",
                                "HOFFMAN",
                                "CARLSON",
                                "SILVA",
                                "PEARSON",
                                "HOLLAND",
                                "DOUGLAS",
                                "FLEMING",
                                "JENSEN",
                                "VARGAS",
                                "BYRD",
                                "DAVIDSON",
                                "HOPKINS",
                                "MAY",
                                "TERRY",
                                "HERRERA",
                                "WADE",
                                "SOTO",
                                "WALTERS",
                                "CURTIS",
                                "NEAL",
                                "CALDWELL",
                                "LOWE",
                                "JENNINGS",
                                "BARNETT",
                                "GRAVES",
                                "JIMENEZ",
                                "HORTON",
                                "SHELTON",
                                "BARRETT",
                                "OBRIEN",
                                "CASTRO",
                                "SUTTON",
                                "GREGORY",
                                "MCKINNEY",
                                "LUCAS",
                                "MILES",
                                "CRAIG",
                                "RODRIQUEZ",
                                "CHAMBERS",
                                "HOLT",
                                "LAMBERT",
                                "FLETCHER",
                                "WATTS",
                                "BATES",
                                "HALE",
                                "RHODES",
                                "PENA",
                                "BECK",
                                "NEWMAN",
                                "HAYNES",
                                "MCDANIEL",
                                "MENDEZ",
                                "BUSH",
                                "VAUGHN",
                                "PARKS",
                                "DAWSON",
                                "SANTIAGO",
                                "NORRIS",
                                "HARDY",
                                "LOVE",
                                "STEELE",
                                "CURRY",
                                "POWERS",
                                "SCHULTZ",
                                "BARKER",
                                "GUZMAN",
                                "PAGE",
                                "MUNOZ",
                                "BALL",
                                "KELLER",
                                "CHANDLER",
                                "WEBER",
                                "LEONARD",
                                "WALSH",
                                "LYONS",
                                "RAMSEY",
                                "WOLFE",
                                "SCHNEIDER",
                                "MULLINS",
                                "BENSON",
                                "SHARP",
                                "BOWEN",
                                "DANIEL",
                                "BARBER",
                                "CUMMINGS",
                                "HINES",
                                "BALDWIN",
                                "GRIFFITH",
                                "VALDEZ",
                                "HUBBARD",
                                "SALAZAR",
                                "REEVES",
                                "WARNER",
                                "STEVENSON",
                                "BURGESS",
                                "SANTOS",
                                "TATE",
                                "CROSS",
                                "GARNER",
                                "MANN",
                                "MACK",
                                "MOSS",
                                "THORNTON",
                                "DENNIS",
                                "MCGEE",
                                "FARMER",
                                "DELGADO",
                                "AGUILAR",
                                "VEGA",
                                "GLOVER",
                                "MANNING",
                                "COHEN",
                                "HARMON",
                                "RODGERS",
                                "ROBBINS",
                                "NEWTON",
                                "TODD",
                                "BLAIR",
                                "HIGGINS",
                                "INGRAM",
                                "REESE",
                                "CANNON",
                                "STRICKLAND",
                                "TOWNSEND",
                                "POTTER",
                                "GOODWIN",
                                "WALTON",
                                "ROWE",
                                "HAMPTON",
                                "ORTEGA",
                                "PATTON",
                                "SWANSON",
                                "JOSEPH",
                                "FRANCIS",
                                "GOODMAN",
                                "MALDONADO",
                                "YATES",
                                "BECKER",
                                "ERICKSON",
                                "HODGES",
                                "RIOS",
                                "CONNER",
                                "ADKINS",
                                "WEBSTER",
                                "NORMAN",
                                "MALONE",
                                "HAMMOND",
                                "FLOWERS",
                                "COBB",
                                "MOODY",
                                "QUINN",
                                "BLAKE",
                                "MAXWELL",
                                "POPE",
                                "FLOYD",
                                "OSBORNE",
                                "PAUL",
                                "MCCARTHY",
                                "GUERRERO",
                                "LINDSEY",
                                "ESTRADA",
                                "SANDOVAL",
                                "GIBBS",
                                "TYLER",
                                "GROSS",
                                "FITZGERALD",
                                "STOKES",
                                "DOYLE",
                                "SHERMAN",
                                "SAUNDERS",
                                "WISE",
                                "COLON",
                                "GILL",
                                "ALVARADO",
                                "GREER",
                                "PADILLA",
                                "SIMON",
                                "WATERS",
                                "NUNEZ",
                                "BALLARD",
                                "SCHWARTZ",
                                "MCBRIDE",
                                "HOUSTON",
                                "CHRISTENSEN",
                                "KLEIN",
                                "PRATT",
                                "BRIGGS",
                                "PARSONS",
                                "MCLAUGHLIN",
                                "ZIMMERMAN",
                                "FRENCH",
                                "BUCHANAN",
                                "MORAN",
                                "COPELAND",
                                "ROY",
                                "PITTMAN",
                                "BRADY",
                                "MCCORMICK",
                                "HOLLOWAY",
                                "BROCK",
                                "POOLE",
                                "FRANK",
                                "LOGAN",
                                "OWEN",
                                "BASS",
                                "MARSH",
                                "DRAKE",
                                "WONG",
                                "JEFFERSON",
                                "PARK",
                                "MORTON",
                                "ABBOTT",
                                "SPARKS",
                                "PATRICK",
                                "NORTON",
                                "HUFF",
                                "CLAYTON",
                                "MASSEY",
                                "LLOYD",
                                "FIGUEROA",
                                "CARSON",
                                "BOWERS",
                                "ROBERSON",
                                "BARTON",
                                "TRAN",
                                "LAMB",
                                "HARRINGTON",
                                "CASEY",
                                "BOONE",
                                "CORTEZ",
                                "CLARKE",
                                "MATHIS",
                                "SINGLETON",
                                "WILKINS",
                                "CAIN",
                                "BRYAN",
                                "UNDERWOOD",
                                "HOGAN",
                                "MCKENZIE",
                                "COLLIER",
                                "LUNA",
                                "PHELPS",
                                "MCGUIRE",
                                "ALLISON",
                                "BRIDGES",
                                "WILKERSON",
                                "NASH",
                                "SUMMERS",
                                "ATKINS",
                                "WILCOX",
                                "PITTS",
                                "CONLEY",
                                "MARQUEZ",
                                "BURNETT",
                                "RICHARD",
                                "COCHRAN",
                                "CHASE",
                                "DAVENPORT",
                                "HOOD",
                                "GATES",
                                "CLAY",
                                "AYALA",
                                "SAWYER",
                                "ROMAN",
                                "VAZQUEZ",
                                "DICKERSON",
                                "HODGE",
                                "ACOSTA",
                                "FLYNN",
                                "ESPINOZA",
                                "NICHOLSON",
                                "MONROE",
                                "WOLF",
                                "MORROW",
                                "KIRK",
                                "RANDALL",
                                "ANTHONY",
                                "WHITAKER",
                                "OCONNOR",
                                "SKINNER",
                                "WARE",
                                "MOLINA",
                                "KIRBY",
                                "HUFFMAN",
                                "BRADFORD",
                                "CHARLES",
                                "GILMORE",
                                "DOMINGUEZ",
                                "ONEAL",
                                "BRUCE",
                                "LANG",
                                "COMBS",
                                "KRAMER",
                                "HEATH",
                                "HANCOCK",
                                "GALLAGHER",
                                "GAINES",
                                "SHAFFER",
                                "SHORT",
                                "WIGGINS",
                                "MATHEWS",
                                "MCCLAIN",
                                "FISCHER",
                                "WALL",
                                "SMALL",
                                "MELTON",
                                "HENSLEY",
                                "BOND",
                                "DYER",
                                "CAMERON",
                                "GRIMES",
                                "CONTRERAS",
                                "CHRISTIAN",
                                "WYATT",
                                "BAXTER",
                                "SNOW",
                                "MOSLEY",
                                "SHEPHERD",
                                "LARSEN",
                                "HOOVER",
                                "BEASLEY",
                                "GLENN",
                                "PETERSEN",
                                "WHITEHEAD",
                                "MEYERS",
                                "KEITH",
                                "GARRISON",
                                "VINCENT",
                                "SHIELDS",
                                "HORN",
                                "SAVAGE",
                                "OLSEN",
                                "SCHROEDER",
                                "HARTMAN",
                                "WOODARD",
                                "MUELLER",
                                "KEMP",
                                "DELEON",
                                "BOOTH",
                                "PATEL",
                                "CALHOUN",
                                "WILEY",
                                "EATON",
                                "CLINE",
                                "NAVARRO",
                                "HARRELL",
                                "LESTER",
                                "HUMPHREY",
                                "PARRISH",
                                "DURAN",
                                "HUTCHINSON",
                                "HESS",
                                "DORSEY",
                                "BULLOCK",
                                "ROBLES",
                                "BEARD",
                                "DALTON",
                                "AVILA",
                                "VANCE",
                                "RICH",
                                "BLACKWELL",
                                "YORK",
                                "JOHNS",
                                "BLANKENSHIP",
                                "TREVINO",
                                "SALINAS",
                                "CAMPOS",
                                "PRUITT",
                                "MOSES",
                                "CALLAHAN",
                                "GOLDEN",
                                "MONTOYA",
                                "HARDIN",
                                "GUERRA",
                                "MCDOWELL",
                                "CAREY",
                                "STAFFORD",
                                "GALLEGOS",
                                "HENSON",
                                "WILKINSON",
                                "BOOKER",
                                "MERRITT",
                                "MIRANDA",
                                "ATKINSON",
                                "ORR",
                                "DECKER",
                                "HOBBS",
                                "PRESTON",
                                "TANNER",
                                "KNOX",
                                "PACHECO",
                                "STEPHENSON",
                                "GLASS",
                                "ROJAS",
                                "SERRANO",
                                "MARKS",
                                "HICKMAN",
                                "ENGLISH",
                                "SWEENEY",
                                "STRONG",
                                "PRINCE",
                                "MCCLURE",
                                "CONWAY",
                                "WALTER",
                                "ROTH",
                                "MAYNARD",
                                "FARRELL",
                                "LOWERY",
                                "HURST",
                                "NIXON",
                                "WEISS",
                                "TRUJILLO",
                                "ELLISON",
                                "SLOAN",
                                "JUAREZ",
                                "WINTERS",
                                "MCLEAN",
                                "RANDOLPH",
                                "LEON",
                                "BOYER",
                                "VILLARREAL",
                                "MCCALL",
                                "GENTRY",
                                "CARRILLO",
                                "KENT",
                                "AYERS",
                                "LARA",
                                "SHANNON",
                                "SEXTON",
                                "PACE",
                                "HULL",
                                "LEBLANC",
                                "BROWNING",
                                "VELASQUEZ",
                                "LEACH",
                                "CHANG",
                                "HOUSE",
                                "SELLERS",
                                "HERRING",
                                "NOBLE",
                                "FOLEY",
                                "BARTLETT",
                                "MERCADO",
                                "LANDRY",
                                "DURHAM",
                                "WALLS",
                                "BARR",
                                "MCKEE",
                                "BAUER",
                                "RIVERS",
                                "EVERETT",
                                "BRADSHAW",
                                "PUGH",
                                "VELEZ",
                                "RUSH",
                                "ESTES",
                                "DODSON",
                                "MORSE",
                                "SHEPPARD",
                                "WEEKS",
                                "CAMACHO",
                                "BEAN",
                                "BARRON",
                                "LIVINGSTON",
                                "MIDDLETON",
                                "SPEARS",
                                "BRANCH",
                                "BLEVINS",
                                "CHEN",
                                "KERR",
                                "MCCONNELL",
                                "HATFIELD",
                                "HARDING",
                                "ASHLEY",
                                "SOLIS",
                                "HERMAN",
                                "FROST",
                                "GILES",
                                "BLACKBURN",
                                "WILLIAM",
                                "PENNINGTON",
                                "WOODWARD",
                                "FINLEY",
                                "MCINTOSH",
                                "KOCH",
                                "BEST",
                                "SOLOMON",
                                "MCCULLOUGH",
                                "DUDLEY",
                                "NOLAN",
                                "BLANCHARD",
                                "RIVAS",
                                "BRENNAN",
                                "MEJIA",
                                "KANE",
                                "BENTON",
                                "JOYCE",
                                "BUCKLEY",
                                "HALEY",
                                "VALENTINE",
                                "MADDOX",
                                "RUSSO",
                                "MCKNIGHT",
                                "BUCK",
                                "MOON",
                                "MCMILLAN",
                                "CROSBY",
                                "BERG",
                                "DOTSON",
                                "MAYS",
                                "ROACH",
                                "CHURCH",
                                "CHAN",
                                "RICHMOND",
                                "MEADOWS",
                                "FAULKNER",
                                "ONEILL",
                                "KNAPP",
                                "KLINE",
                                "BARRY",
                                "OCHOA",
                                "JACOBSON",
                                "GAY",
                                "AVERY",
                                "HENDRICKS",
                                "HORNE",
                                "SHEPARD",
                                "HEBERT",
                                "CHERRY",
                                "CARDENAS",
                                "MCINTYRE",
                                "WHITNEY",
                                "WALLER",
                                "HOLMAN",
                                "DONALDSON",
                                "CANTU",
                                "TERRELL",
                                "MORIN",
                                "GILLESPIE",
                                "FUENTES",
                                "TILLMAN",
                                "SANFORD",
                                "BENTLEY",
                                "PECK",
                                "KEY",
                                "SALAS",
                                "ROLLINS",
                                "GAMBLE",
                                "DICKSON",
                                "BATTLE",
                                "SANTANA",
                                "CABRERA",
                                "CERVANTES",
                                "HOWE",
                                "HINTON",
                                "HURLEY",
                                "SPENCE",
                                "ZAMORA",
                                "YANG",
                                "MCNEIL",
                                "SUAREZ",
                                "CASE",
                                "PETTY",
                                "GOULD",
                                "MCFARLAND",
                                "SAMPSON",
                                "CARVER",
                                "BRAY",
                                "ROSARIO",
                                "MACDONALD",
                                "STOUT",
                                "HESTER",
                                "MELENDEZ",
                                "DILLON",
                                "FARLEY",
                                "HOPPER",
                                "GALLOWAY",
                                "POTTS",
                                "BERNARD",
                                "JOYNER",
                                "STEIN",
                                "AGUIRRE",
                                "OSBORN",
                                "MERCER",
                                "BENDER",
                                "FRANCO",
                                "ROWLAND",
                                "SYKES",
                                "BENJAMIN",
                                "TRAVIS",
                                "PICKETT",
                                "CRANE",
                                "SEARS",
                                "MAYO",
                                "DUNLAP",
                                "HAYDEN",
                                "WILDER",
                                "MCKAY",
                                "COFFEY",
                                "MCCARTY",
                                "EWING",
                                "COOLEY",
                                "VAUGHAN",
                                "BONNER",
                                "COTTON",
                                "HOLDER",
                                "STARK",
                                "FERRELL",
                                "CANTRELL",
                                "FULTON",
                                "LYNN",
                                "LOTT",
                                "CALDERON",
                                "ROSA",
                                "POLLARD",
                                "HOOPER",
                                "BURCH",
                                "MULLEN",
                                "FRY",
                                "RIDDLE",
                                "LEVY",
                                "DAVID",
                                "DUKE",
                                "ODONNELL",
                                "GUY",
                                "MICHAEL",
                                "BRITT",
                                "FREDERICK",
                                "DAUGHERTY",
                                "BERGER",
                                "DILLARD",
                                "ALSTON",
                                "JARVIS",
                                "FRYE",
                                "RIGGS",
                                "CHANEY",
                                "ODOM",
                                "DUFFY",
                                "FITZPATRICK",
                                "VALENZUELA",
                                "MERRILL",
                                "MAYER",
                                "ALFORD",
                                "MCPHERSON",
                                "ACEVEDO",
                                "DONOVAN",
                                "BARRERA",
                                "ALBERT",
                                "COTE",
                                "REILLY",
                                "COMPTON",
                                "RAYMOND",
                                "MOONEY",
                                "MCGOWAN",
                                "CRAFT",
                                "CLEVELAND",
                                "CLEMONS",
                                "WYNN",
                                "NIELSEN",
                                "BAIRD",
                                "STANTON",
                                "SNIDER",
                                "ROSALES",
                                "BRIGHT",
                                "WITT",
                                "STUART",
                                "HAYS",
                                "HOLDEN",
                                "RUTLEDGE",
                                "KINNEY",
                                "CLEMENTS",
                                "CASTANEDA",
                                "SLATER",
                                "HAHN",
                                "EMERSON",
                                "CONRAD",
                                "BURKS",
                                "DELANEY",
                                "PATE",
                                "LANCASTER",
                                "SWEET",
                                "JUSTICE",
                                "TYSON",
                                "SHARPE",
                                "WHITFIELD",
                                "TALLEY",
                                "MACIAS",
                                "IRWIN",
                                "BURRIS",
                                "RATLIFF",
                                "MCCRAY",
                                "MADDEN",
                                "KAUFMAN",
                                "BEACH",
                                "GOFF",
                                "CASH",
                                "BOLTON",
                                "MCFADDEN",
                                "LEVINE",
                                "GOOD",
                                "BYERS",
                                "KIRKLAND",
                                "KIDD",
                                "WORKMAN",
                                "CARNEY",
                                "DALE",
                                "MCLEOD",
                                "HOLCOMB",
                                "ENGLAND",
                                "FINCH",
                                "HEAD",
                                "BURT",
                                "HENDRIX",
                                "SOSA",
                                "HANEY",
                                "FRANKS",
                                "SARGENT",
                                "NIEVES",
                                "DOWNS",
                                "RASMUSSEN",
                                "BIRD",
                                "HEWITT",
                                "LINDSAY",
                                "LE",
                                "FOREMAN",
                                "VALENCIA",
                                "ONEIL",
                                "DELACRUZ",
                                "VINSON",
                                "DEJESUS",
                                "HYDE",
                                "FORBES",
                                "GILLIAM",
                                "GUTHRIE",
                                "WOOTEN",
                                "HUBER",
                                "BARLOW",
                                "BOYLE",
                                "MCMAHON",
                                "BUCKNER",
                                "ROCHA",
                                "PUCKETT",
                                "LANGLEY",
                                "KNOWLES",
                                "COOKE",
                                "VELAZQUEZ",
                                "WHITLEY",
                                "NOEL",
                                "VANG"};

            _LastNames = new Collection<string>(lnames);
        }
        #endregion init names

        public static int GetLocation()
        {
            int toReturn = 0;
            string lName = Guid.NewGuid().ToString().Substring(0, 10);
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
				ETIuser user = (from u in gbe.ETIusers

                            select u).First();

                Models.Location locale = new Models.Location();
                locale.LocationID = -1;
                locale.LocationName = "MOCK" + lName;
                locale.UpdatedByETIUserID = user.ETIUserID;

                gbe.Locations.Add(locale);
                gbe.SaveChanges();
                toReturn = locale.LocationID;

            }

            return toReturn;
        }

        public static int GetSystemTrip()
        {
			TripDTODataLoader sut = new TripDTODataLoader();

            // create a trip 
            TripDTO trip = new TripDTO();
            trip.TripID = -1;
            trip.Updateable = UpdateType.Upsert;
            trip.UpdateDate = DateTime.Now;
            trip.MiscTravelRateID = GetMiscTravelRateID();
            trip.DepartureLocationID = GlobalDepartureLocationID;
            trip.DestinationLocationID = GlobalDestLocationID;
            trip.PerDiemID = GlobalPerDiemID;
            trip.Fare = 50;
            trip.RTMiles = 50;
            trip.FareUpdatedByUserID = GlobalTestCaseSetup.GlobalBOEAuthorID;

            sut.SaveTrips(new Collection<TripDTO> { trip });

            ICollection<TripDTO> trips = sut.GetAllTrips();

            int tripToSaveID =  (from t in trips
                                 where t.DepartureLocationID == _GlobalDepartureLocationID && t.DestinationLocationID==_GlobalDestLocationID
                                 select t.TripID).First();

            return tripToSaveID;
        }

        public static int GetMiscTravelRateID()
        {
            int toReturn = 0;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                Models.TravelMiscRate travelRate = new TravelMiscRate();
                travelRate.SortCode = 77;
                travelRate.TravelMiscRateID = -1;
                travelRate.MiscellaneousRate = 50;
                travelRate.TransportationMode = "MockGlobalMode";
                travelRate.UpdateDT = DateTime.Now;

                gbe.TravelMiscRates.Add(travelRate);
                gbe.SaveChanges();

                toReturn = (from t in gbe.TravelMiscRates
                            where t.TransportationMode == "MockGlobalMode"
                            select t.TravelMiscRateID).First();
            }
          
            return toReturn;
        }

    }

    /// <summary>
    /// Class to encapsulate the users added via the test seed program.
    /// Not interested in designing for visibility etc.
    /// </summary>
    public class UserCreated
    {
        public ETIuser User { get; set; }
        public Role Role { get; set; }
    }
}

