// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace CopyWorkspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
    using GenBOE.Models;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Program to copy a workspace between two databases.
    /// </summary>
    class Program
    {
        private static string copyFromDatabase;
        private static string copyToDatabase;
        private static string workspaceShortNameToCopy;
        const string CONNECTION_NAME_SOURCE = "GenBoeEntities";
        const string CONNECTION_NAME_COPY_TO = "GenBoeEntitiesCopyTo";
        private static IActiveDirectoryUtilities adUtils;

        static void Main()
        {
            // This program will retrieve a workspace from one database and create a new copy inside another database
            Console.WriteLine("Running under following Company: " + SystemConfiguration.Instance().CompanyMode.ToString());

            LoadContainers loadContainers = new LoadContainers();
            loadContainers.InitializeFactory();
            adUtils = new ActiveDirectoryUtilities();
            copyToDatabase = ConfigurationManager.ConnectionStrings[CONNECTION_NAME_COPY_TO].ConnectionString;
            copyFromDatabase = ConfigurationManager.ConnectionStrings[CONNECTION_NAME_SOURCE].ConnectionString;
            workspaceShortNameToCopy = ConfigurationUtilities.GetAppSetting("workspaceShortNameToCopy");
            Console.WriteLine();
            Console.WriteLine("Copying from Database: " + copyFromDatabase);
            Console.WriteLine();
            Console.WriteLine("Copying to Database: " + copyToDatabase);
            Console.WriteLine();
            Console.WriteLine("Workspace short name to copy: " + workspaceShortNameToCopy);
            try
            {
                CopyTheWorkspace(workspaceShortNameToCopy);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Copies the workspace.
        /// </summary>
        /// <param name="shortWorkspaceName">Short name of the workspace.</param>
        private static void CopyTheWorkspace(string shortWorkspaceName)
        {
            Console.WriteLine("Loading data for workspace to copy...");
            IFullObjectFactory factory = GetNewFactory();
            IUserDTODataLoader userLoader = GenBOEUnityContainer.Container.Resolve(typeof(IUserDTODataLoader)) as IUserDTODataLoader;
            Collection<UserDTO> users = userLoader.GetAllUsers();
            FullWorkspace ws = factory.CreateFullWorkspace(shortWorkspaceName, true);
            Console.WriteLine("Loading Perf Orgs...");
            IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsUsedInBoes = ws.PerformingOrgsForWsList;
            Console.WriteLine("Loading Tasks and BOEs...");
            ws.LoadBoesAndTaskElementsRTEData();
            Console.WriteLine("Loading Travel...");
            ws.LoadTravelRTEData();
            Console.WriteLine("Loading Permissions...");
            IReadOnlyCollection<PermissionsDTO> WorkspacePermissions = ws.WorkspacePermissions;
            Console.WriteLine("Loading Clins...");
            IReadOnlyCollection<FullClin> Clins = ws.Clins;
            Console.WriteLine("Loading WBS...");
            IReadOnlyCollection<FullWbs> WbsElements = ws.WbsElements;
            IReadOnlyCollection<FullBoe> Boes = ws.Boes;
            Console.WriteLine("Loading Other Data...");
            IReadOnlyCollection<WorkspaceVariableDTO> WorkspaceVariables = ws.WorkspaceVariables;
            IReadOnlyCollection<BoeTaskElementDTO> TaskElements = ws.TaskElements;
            IReadOnlyCollection<TravelDTO> Travels = ws.Travels;
            IReadOnlyCollection<TripDTO> TravelTrips = ws.TravelTrips;
            IReadOnlyCollection<ResourceDTO> ResourcesUsedInWsBoes = ws.ResourcesForWsResourceListId;
			IReadOnlyCollection<CustomFieldDTO> CustomFields = ws.CustomFields;
            IDictionary<int, ICollection<BoeApproverResponseDTO>> approvers = new BoeApproverResponseDTODataLoader().GetByWorkspaceId(ws.Id);
            IReadOnlyCollection<CustomFieldValueDTO> CustomFieldValues = ws.CustomFieldValues;
            ICollection<PermissionsDTO> boePotentialPermissions = new PermissionsDTODataLoader(adUtils).GetBOEPotentialPermissionsForWorkspace(ws.Id);
            
            ICollection<BOEFormIBOEDTO> iboes = new BOEFormIBOEDTODataLoader().GetByWorkspaceId(ws.Id);
            ICollection<BOEFormPBOEDTO> pboes = new BOEFormPBOEDTODataLoader().GetByWorkspaceId(ws.Id);

			Collection<WorkspaceExportFormatDTO> copiedTemplateTypes = new WorkspaceExportFormatDTODataLoader().GetWorkspaceExportFormatsForWorkspace(ws.Id);

            Console.WriteLine("Finished Loading Data to copy, changing Database.");
            ChangeDatabase();

            Console.WriteLine("Creating User Id dictionary mapping");
            userLoader = GenBOEUnityContainer.Container.Resolve(typeof(IUserDTODataLoader)) as IUserDTODataLoader;
            Collection<UserDTO> destinationUsers = userLoader.GetAllUsers();
            IDictionary<int, int> mappedUserIds = FindUserMapping(users, destinationUsers);

            Console.WriteLine("Creating new workspace...");
            FullWorkspace newWs = CreateNewWorkspace(ws, mappedUserIds);

            Console.WriteLine("The new workspace shortname: " + newWs.Shortname);
            Console.WriteLine("Copying data over...");
            DatabaseWorkspaceCopier copier = GenBOEUnityContainer.Container.Resolve(typeof(DatabaseWorkspaceCopier)) as DatabaseWorkspaceCopier;

            copier.CopyWorkspace(ws, newWs, Boes.ToCollection(), true, true, true, boePotentialPermissions, approvers, ResourcesUsedInWsBoes.ToList(),
				CustomFieldValues.ToList(), TaskElements.ToList(), Travels.ToList(), PerformingOrgsUsedInBoes.ToList(), iboes, pboes, mappedUserIds, users);

            Console.WriteLine("Done.");
        }

        private static IDictionary<int, int> FindUserMapping(Collection<UserDTO> users, Collection<UserDTO> destinationUsers)
        {
            Dictionary<int, int> mappedUserIds = new Dictionary<int, int>();
            foreach (UserDTO user in users)
            {
                UserDTO destinationUser = destinationUsers.FirstOrDefault(d => d.NTID == user.NTID);
                if (destinationUser != null)
                {
                    mappedUserIds.Add(user.UserID, destinationUser.UserID);
                }
            }

            return mappedUserIds;
        }

        /// <summary>
        /// Creates the new workspace.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <returns></returns>
        private static FullWorkspace CreateNewWorkspace(FullWorkspace ws, IDictionary<int, int> mappedUserIds)
        {
            // Create the Workspace
            WorkspaceDTO newWorkspaceDTO = new WorkspaceDTO();
            newWorkspaceDTO.ContainsOCI = ws.ContainsOCI;
            newWorkspaceDTO.ContractEndDate = Convert.ToDateTime(ws.ContractEndDate);
            newWorkspaceDTO.ContractStartDate = Convert.ToDateTime(ws.ContractStartDate);
            newWorkspaceDTO.Description = ws.Description;
            newWorkspaceDTO.ProposalSubmittalDate = ws.ProposalSubmittalDate != null ? (DateTime?)Convert.ToDateTime(ws.ProposalSubmittalDate) : null;
            newWorkspaceDTO.LineOfBusiness = ws.LineOfBusiness;
            newWorkspaceDTO.Segment = ws.Segment;
            newWorkspaceDTO.RFPNumber = ws.RFPNumber;
            newWorkspaceDTO.TrackingNumber = ws.TrackingNumber;
            newWorkspaceDTO.Shortname = ws.Shortname;
            newWorkspaceDTO.WorkspaceName = ws.WorkspaceName;
            newWorkspaceDTO.CreatedByUserID = mappedUserIds[ws.CreatedByUserID];
            newWorkspaceDTO.CostVolumeLeadPricerUserID = mappedUserIds[ws.CostVolumeLeadPricerUserID];
            newWorkspaceDTO.ContainsTemplate = ws.ContainsTemplate;
            newWorkspaceDTO.AllowSearch = ws.AllowSearch;
            newWorkspaceDTO.IsUsingEquivalentPerson = ws.IsUsingEquivalentPerson;
            newWorkspaceDTO.IsUsingTM = ws.IsUsingTM;
            newWorkspaceDTO.RteSizeLimit = ws.RteSizeLimit;

            if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
            {
                newWorkspaceDTO.ProposalTitle = ws.ProposalTitle;
                newWorkspaceDTO.ProposalClass = ws.ProposalClass;
                newWorkspaceDTO.RevisedSubmittalDate = ws.RevisedSubmittalDate;
            }
            else
            {
                // RMS
                newWorkspaceDTO.ProjectMapType = ws.ProjectMapType;
                newWorkspaceDTO.AllowGridEdit = ws.AllowGridEdit;
            }

            // supply an initial output template id (from the default set)
            newWorkspaceDTO.TemplateID = ws.TemplateID; 

            newWorkspaceDTO.ResourceDecimalPrecision = ws.ResourceDecimalPrecision;
            newWorkspaceDTO.CostDecimalPrecision = ws.CostDecimalPrecision;

            if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
            {
                newWorkspaceDTO.SelectedContractTypes = ws.SelectedContractTypes;               
                newWorkspaceDTO.Shortname = CreateTrackingNumberShortname(ws);
                newWorkspaceDTO.WorkspaceName = ws.WorkspaceName + " " + newWorkspaceDTO.Shortname;
            }
            else
            {
                newWorkspaceDTO.Shortname = ws.Shortname + "_01";
                newWorkspaceDTO.WorkspaceName = ws.WorkspaceName + " Duplicate";
            }

            newWorkspaceDTO.TrackingNumber = ws.TrackingNumber;

            if (newWorkspaceDTO.BOEExportSortByID == 0)
            {
                newWorkspaceDTO.BOEExportSortByID = (int)ExportSortBOEBy.WBS;
            }

            // if this workspace is being created by a copy, need to supply that workspace's perf org and resource list id so the entire list is copied
            // Copy the BOE Export Sort Order.
            
            newWorkspaceDTO.PerfOrgListID = ws.PerfOrgListID;
            newWorkspaceDTO.ResourceListID = ws.ResourceListID;
            newWorkspaceDTO.CustomFieldSorting = ws.CustomFieldSorting;
            newWorkspaceDTO.ResourceSorting = ws.ResourceSorting;
            newWorkspaceDTO.PerfOrgSorting = ws.PerfOrgSorting;
            newWorkspaceDTO.WorkspaceState = ws.WorkspaceState;
    
            newWorkspaceDTO.BOEExportSortByID = ws.BOEExportSortByID;
            
            // Set Project Map Workspaces to Working state - they can't be set to Initialization
            if (newWorkspaceDTO.IsProjectMapWorkspace)
            {
                newWorkspaceDTO.WorkspaceState = WorkspaceState.Working;
            }

            int newWorkspaceID = new WorkspaceDTODataLoader().SaveWorkspaceSettings(newWorkspaceDTO.CreatedByUserID, newWorkspaceDTO);
            FullWorkspace newWs = GetNewFactory().CreateFullWorkspace(newWorkspaceID);

            // Set the default rates/fees
            new RMSZoneTravelRatesFeesDataLoader(new EscalationRatesDTOLoader(), new MSTTravelNonzoneFeesAndCostsDTODataLoader()).CopySystemDefaultFees(newWorkspaceID);
            new RMSZoneTravelRatesFeesDataLoader(new EscalationRatesDTOLoader(), new MSTTravelNonzoneFeesAndCostsDTODataLoader()).CopySystemDefaultRates(newWorkspaceID);

            // Set the default Offload Rates
            if (newWorkspaceDTO.ProjectMapType != ProjectMapType.StandardWithoutOffload)
            {
                new OffloadRatesDTOLoader().CopySystemDefaultOffloadRates(newWorkspaceID);
            }

            return newWs;
        }

        /// <summary>
        /// Creates the tracking number shortname.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <returns></returns>
        /// <exception cref="GenValidationException">There are already 99 duplicates, please choose another Tracking Number.</exception>
        private static string CreateTrackingNumberShortname(FullWorkspace ws)
        {
            if (string.IsNullOrWhiteSpace(ws.TrackingNumber))
            {
                return ws.Shortname + "_01";
            }
            else
            {
                // Sanitize tracking number
                string workspaceNameText = ws.TrackingNumber.ToLower();
                char[] workspaceNameCharArray = workspaceNameText.ToCharArray();
                string whiteList = "abcdefghijklmnopqrstuvwxyz0123456789-";
                for (int index = 0; index < workspaceNameCharArray.Length; index++)
                {
                    if (whiteList.IndexOf(workspaceNameCharArray[index]) < 0)
                    {
                        workspaceNameCharArray[index] = '_';
                    }
                }

                string nextRevision = new string(workspaceNameCharArray);
                if (nextRevision.Length > 15)
                {
                    nextRevision = nextRevision.Substring(0, 15);
                }

                ICollection<WorkspaceDTO> trackingNameData = new WorkspaceDTODataLoader().GetAllWsNamesAndTrackingNumberInfo().Where(w => w.TrackingNumber == ws.TrackingNumber).ToList();
                if (trackingNameData.Any())
                {
                    int i = 0;
                    do
                    {
                        i++;

                        if (i > 99)
                        {
                            throw new GenValidationException("There are already 99 duplicates, please choose another Tracking Number.");
                        }
                    }
                    while (trackingNameData.Any(x => x.Shortname == (nextRevision + "_" + (i).ToString("00"))));

                    nextRevision = nextRevision + "_" + i.ToString("00");
                }

                return nextRevision;
            }
        }

        /// <summary>
        /// Gets the new factory.
        /// </summary>
        /// <returns></returns>
        private static IFullObjectFactory GetNewFactory()
        {
            IFullObjectFactory factory = GenBOEUnityContainer.Container.Resolve(typeof(IFullObjectFactory)) as IFullObjectFactory;

            return factory;
        }

        /// <summary>
        /// Changes the database.
        /// </summary>
        private static void ChangeDatabase()
        {
            GenBoeEntities.ChangeConnection("name=" + CONNECTION_NAME_COPY_TO);
            Constants.BOE_DB_CONTEXT_NAME = "name=" + CONNECTION_NAME_COPY_TO;
        }
    }
}
