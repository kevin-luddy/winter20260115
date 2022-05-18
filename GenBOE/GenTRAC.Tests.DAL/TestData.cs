// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using GenBOE.DataBridge.Common;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Models;
    using IES.Common;

    /// <summary>
    /// The data created during tests
    /// </summary>
    public class TestData
    {
        #region Class Information

        /// <summary>
        /// the current instance
        /// </summary>
        private static TestData currentInstance;

        /// <summary>
        /// Random Number Generator
        /// </summary>
        private static Random randomNumberGenerator = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// Can't create an object, must use GetInstance
        /// </summary>
        private TestData()
        {
        }

        /// <summary>
        /// Singleton pattern
        /// </summary>
        /// <returns>The test data instance</returns>
        public static TestData GetInstance()
        {
            if (currentInstance == null)
            {
                currentInstance = new TestData();
            }

            return currentInstance;
        }

        /// <summary>
        /// Initialize Test Data
        /// </summary>
        public void Initialize()
        {
            this.GetProposal();
        }

        /// <summary>
        /// Cleanup Test Data
        /// </summary>
        public void Cleanup()
        {
            // delete system permissions
            this.ResetPermissions();

            // delete proposal permissions
            this.ResetProposalPermissions();

            // delete proposals
            this.ResetProposals();

            // delete users
            this.ResetUsers();
        }

        /// <summary>
        /// Creates a random word
        /// </summary>
        /// <param name="size">size of the word</param>
        /// <param name="allowNumbers">whether or not to allow numbers</param>
        /// <param name="extraAllowedChars">any extra special characters allowed</param>
        /// <returns>the word</returns>
        public static string CreateRandomWord(int size, bool allowNumbers = false, string extraAllowedChars = "")
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
                word = string.Concat(word, viableChars[randomNumberGenerator.Next(viableChars.Length)]);
            }

            return word;
        }

        #endregion Class Information

        #region Users

        #region Setup

        /// <summary>
        /// User Loader
        /// </summary>
        private IUserLoader userLoader = new UserLoader();

        /// <summary>
        /// User Loader
        /// </summary>
        public IUserLoader UserLoader
        {
            get
            {
                return this.userLoader;
            }
        }

        /// <summary>
        /// Generic Pricer
        /// </summary>
        private UserDTO proposalPricer = null;

        /// <summary>
        /// Generic User
        /// </summary>
        private UserDTO genericUser = null;

        /// <summary>
        /// A collection of all users created through this class
        /// </summary>
        private List<UserDTO> userCollection = new List<UserDTO>();

        /// <summary>
        /// A generic user
        /// </summary>
        private UserDTO user1 = new UserDTO()
        {
            DisplayName = "Test User 1",
            Ntid = "testuser1",
            EmailAddress = "test@ptm.ssc.lmco.com",
            FirstName = "Test",
            LastName = "User 1",
            PhoneNumber = "555-1212",
            IsGroup = false,
            IsUsPerson = true,
            IsSubcontractor = false
        };

        #endregion Setup

        #region Get

        /// <summary>
        /// Gets a specific user
        /// </summary>
        /// <param name="userId">user Id to get</param>
        /// <returns>User Dto</returns>
        public UserDTO GetUser(int userId)
        {
            return this.userLoader.GetById(userId);
        }

        /// <summary>
        /// Deletes all of the test users out of the database.
        /// </summary>
        public void DeleteAllTestUsers()
        {
            ICollection<UserDTO> users = this.userLoader.GetAll().Where(u => u.EmailAddress == "test@ptm.ssc.lmco.com").ToList();
            ICollection<SystemPermissionDto> systemPermissions = this.systemPermissionsLoader.GetAllSystemPermissions();
            ICollection<ProposalPermissionDto> propPermissions = this.proposalPermissionsLoader.GetAllProposalPermissions();
            ICollection<ProposalDto> proposals = this.proposalLoader.GetAllSlim().Where(p => p.ProposalTitle.Contains("Mock")).ToList();
            foreach (UserDTO user in users)
            {
                ICollection<SystemPermissionDto> permissions = systemPermissions.Where(s => s.UserId == user.Id).ToList();
                ICollection<ProposalPermissionDto> props = propPermissions.Where(s => s.UserId == user.Id).ToList();
                using (TransactionScope scope = new TransactionScope())
                {
                    if (permissions.Any())
                    {
                        foreach (SystemPermissionDto perm in permissions)
                        {
                            perm.Updateable = UpdateType.Deleted;
                            this.systemPermissionsLoader.Save(perm);
                        }
                    }

                    if (props.Any())
                    {
                        foreach (ProposalPermissionDto perm in props)
                        {
                            perm.Updateable = UpdateType.Deleted;
                            this.proposalPermissionsLoader.Save(perm);
                        }
                    }

                    scope.Complete();
                }
            }

            if (proposals.Any())
            {
                foreach (ProposalDto prop in proposals)
                {
                    // reget the proposal
                    ProposalDto newProp = this.proposalLoader.GetById(prop.Id);
                    newProp.Updateable = UpdateType.Deleted;

                    using (TransactionScope scope = new TransactionScope())
                    {
                        this.proposalLoader.Save(newProp);
                        scope.Complete();
                    }
                }
            }

            foreach (UserDTO user in users)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    user.Updateable = UpdateType.Deleted;
                    this.userLoader.Save(user);
                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// Gets a user, creating a new one if 
        /// </summary>
        /// <param name="inCreateNew">Whether or not to create a new user</param>
        /// <param name="inUserDTO">The user to create</param>
        /// <param name="isGroup">determines if the user should be created as a group or not</param>
        /// <returns>the user</returns>
        public UserDTO GetUser(bool inCreateNew = false, UserDTO inUserDTO = null, bool isGroup = false)
        {
            UserDTO toReturn;

            if (inCreateNew)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    if (inUserDTO == null)
                    {
                        string userIdentifier = CreateRandomWord(5);
                        UserDTO testUser = new UserDTO()
                        {
                            DisplayName = "testUser_" + userIdentifier,
                            FirstName = "TestUser",
                            LastName = userIdentifier,
                            Ntid = "testNtid_" + userIdentifier,
                            EmailAddress = "test@ptm.ssc.lmco.com",
                            PhoneNumber = "867-5309",
                            Updateable = UpdateType.Upsert,
                            IsGroup = isGroup,
                            IsUsPerson = true,
                            IsSubcontractor = false
                        };

                        toReturn = this.userLoader.GetById(this.userLoader.Save(testUser).Value);
                        this.userCollection.Add(toReturn);
                    }
                    else
                    {
                        toReturn = this.userLoader.GetById(this.userLoader.Save(inUserDTO).Value);
                        this.userCollection.Add(toReturn);
                    }

                    scope.Complete();
                }
            }
            else
            {
                if (this.genericUser == null)
                {
                    this.genericUser = this.GetUser(true);
                }

                toReturn = this.genericUser;
            }

            return toReturn;
        }

        /// <summary>
        /// Creates the test user 1 if it's not already in the DB.
        /// </summary>
        /// <returns>The proposal manager</returns>
        public UserDTO GetProposalPricer()
        {
            if (this.proposalPricer == null)
            {
                int testUserID;
                if (!this.userLoader.UserExists("testuser1", out testUserID))
                {
                    UserDTO testUser = GenBOEUtilities.Clone<UserDTO>(this.user1);
                    testUser.Updateable = UpdateType.Upsert;

                    using (TransactionScope scope = new TransactionScope())
                    {
                        this.proposalPricer = this.userLoader.GetById(this.userLoader.Save(testUser).Value);
                        scope.Complete();
                    }

                    this.userCollection.Add(this.proposalPricer);
                }
                else
                {
                    this.proposalPricer = this.userLoader.GetById(testUserID);
                }
            }

            return this.proposalPricer;
        }

        #endregion Get

        #region Reset

        /// <summary>
        /// Resets the users so the next request creates new ones.
        /// </summary>
        private void ResetUsers()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                foreach (UserDTO user in this.userCollection)
                {
                    // get the current update time
                    var toDelete = this.userLoader.GetById(user.Id);

                    // delete
                    if (toDelete != null)
                    {
                        toDelete.Updateable = UpdateType.Deleted;
                        this.userLoader.Save(toDelete);
                    }
                }

                scope.Complete();
            }

            // Reset
            this.userCollection.Clear();
            this.proposalPricer = null;
            this.genericUser = null;
        }

        #endregion Reset

        #endregion Users

        #region Proposals

        #region Setup

        /// <summary>
        /// proposal Loader
        /// </summary>
        private IProposalLoader proposalLoader = new ProposalLoader();

        /// <summary>
        /// Generic proposal
        /// </summary>
        private ProposalDto genericProposal = null;

        /// <summary>
        /// A collection of all proposal created through this class
        /// </summary>
        private List<ProposalDto> proposalList = new List<ProposalDto>();

        /// <summary>
        /// PPR checklist content loader
        /// </summary>
        private IChecklistContentLoader pprContentLoader = new PPRChecklistContentLoader();

        /// <summary>
        /// PAR checklist content loader
        /// </summary>
        private IChecklistContentLoader parContentLoader = new PARChecklistContentLoader();

        /// <summary>
        /// Proposal checklist loader
        /// </summary>
        private IProposalChecklistLoader proposalChecklistLoader = new ProposalChecklistLoader();

        /// <summary>
        /// A generic proposal
        /// </summary>
        private ProposalDto proposal1 = new ProposalDto()
        {
            ProposalTitle = "MockProposal_" // plus identifier
        };

        /// <summary>
        /// Proposal Loader
        /// </summary>
        public IProposalLoader ProposalLoader
        {
            get 
            { 
                return this.proposalLoader; 
            }
        } 
         
        #endregion Setup

        #region Get

        /// <summary>
        /// Gets a proposal, will create the default one if none exist
        /// </summary>
        /// <param name="inCreateNew">Whether or not to create a new proposal</param>
        /// <param name="inProposalDto">The proposal to create</param>
        /// <param name="inAnticipatedDeliveryDate">The anticipated delivery date.</param>
        /// <param name="isForecasted">Boolean to tell whether to create a forecast proposal or a regular one.</param>
        /// <param name="revisionOfId">Optional proposal id, for revision testing</param>
        /// <returns>The proposal</returns>
        public ProposalDto GetProposal(bool inCreateNew = false, ProposalDto inProposalDto = null, DateTime? inAnticipatedDeliveryDate = null, bool isForecasted = false, int? revisionOfId = null)
        {
            ProposalDto toReturn = null;

            if (inCreateNew)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    int userId = this.GetUser(inCreateNew = true).Id;

                    if (inProposalDto == null)
                    {
                        string proposalIdentifier = "Mock" + CreateRandomWord(6);

                        ProposalDto newProposal = GenBOEUtilities.Clone<ProposalDto>(this.proposal1);
                        newProposal.ProposalTitle += proposalIdentifier;
                        newProposal.OTISOpportunityID = proposalIdentifier;
                        newProposal.ProposalStatus = ProposalStatus.InProgress;
                        newProposal.Id = -1;
                        newProposal.UpdateDate = DateTime.Now;
                        newProposal.Updateable = UpdateType.Upsert;
                        newProposal.ContractTypeGroup = 1; // ContractTypeGroup.CP;
                        newProposal.ContractTypeIds = new List<int>() { };
                        newProposal.BoeTool = BOETool.genBOE;
                        newProposal.CostElementTypeIds = new List<int> { (int)CostElementType.IWTA };
                        newProposal.Customer = proposalIdentifier;
                        newProposal.CustomerType = CustomerType.Commercial;
                        newProposal.DeliveryDate = inAnticipatedDeliveryDate.HasValue ? inAnticipatedDeliveryDate.Value : DateTime.Now;
                        newProposal.RFPIssuedDate = DateTime.Now;
                        newProposal.RFPReceivedDate = DateTime.Now;
                        newProposal.EstimatedProposalValue = 0;
                        newProposal.ISGSRole = ISGSRole.IWTA;
                        newProposal.ProgramAreaId = 46;
                        newProposal.ProposalLocation = ProposalLocation.ValleyForgePA;
                        newProposal.PricingTool = PricingTool.Excel;
                        newProposal.LineOfBusinessID = 10;
                        newProposal.ProgramName = proposalIdentifier;
                        newProposal.ProposalType = 3;
                        newProposal.Request = 1;
                        newProposal.ProposalClass = 1;
                        newProposal.RFPNumber = proposalIdentifier;
                        newProposal.ContractTypeIds = new List<int> { 1 }; // CostPlusAwardFee
                        newProposal.CostElementTypeIds = new List<int> { (int)CostElementType.Materials };
                        newProposal.CreatedByUserId = userId;
                        newProposal.ProgramProposalStatus = ProgramProposalStatus.UnderStrategicReviewISGS;
                        newProposal.IsCostVolumeClassified = false;
                        newProposal.RevisionOfId = revisionOfId;

                        if (isForecasted)
                        {
                            newProposal.ForecastedTrackingNumber = this.proposalLoader.GetNextForecastedTrackingNumber();
                            newProposal.IsForecastProposal = true;
                            using (genTRACEntities dbModel = new genTRACEntities())
                            {
                                newProposal.ProposalClass = dbModel.ProposalClassLUs.First(pc => pc.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED).ProposalClassID;
                            }
                        }

                        toReturn = this.proposalLoader.GetById(this.proposalLoader.Save(newProposal).Value);
                        this.proposalList.Add(toReturn);
                    }
                    else
                    {
                        inProposalDto.CreatedByUserId = userId;
                        toReturn = this.proposalLoader.GetById(this.proposalLoader.Save(inProposalDto).Value);
                        this.proposalList.Add(toReturn);
                    }

                    scope.Complete();
                }
            }
            else
            {
                if (this.genericProposal == null)
                {
                    this.genericProposal = this.GetProposal(true);
                }

                toReturn = this.genericProposal;
            }

            return toReturn;
        }

        /// <summary>
        /// Add given proposal to list, to be cleaned up at end of test run
        /// </summary>
        /// <param name="proposal">Proposal to be deleted</param>
        public void AddProposal(ProposalDto proposal)
        {
            this.proposalList.Add(proposal);
        }

        /// <summary>
        /// Update the non key fields of a proposal.  
        /// Use this when you'd like to update some values on an existing proposal but you don't want to:
        /// (1) add it to the proposalList since it already exists there
        /// (2) reassign the CreatedByUserId since the proposal since it's already been assigned
        /// Note: ProposalLoader Save() Upsert's this proposal so be careful not to update any key fields.
        /// </summary>
        /// <param name="inProposalDto">the proposal to update</param>
        /// <returns>the updated proposal after db Upsert</returns>
        public ProposalDto UpdateProposal(ProposalDto inProposalDto)
        {
            ProposalDto toReturn = null;

            if (inProposalDto == null)
            {
                throw new ArgumentNullException(nameof(inProposalDto));
            }

            inProposalDto.Updateable = UpdateType.Upsert;
            inProposalDto.UpdateDate = this.GetUpdateDate(inProposalDto);

            using (TransactionScope scope = new TransactionScope())
            {
                toReturn = this.proposalLoader.GetById(this.proposalLoader.Save(inProposalDto).Value);
                scope.Complete();
            }

            return toReturn;
        }

        /// <summary>
        /// Get the latest update date for the proposal from the database
        /// </summary>
        /// <param name="inProposalDto">the proposal to query</param>
        /// <returns>the latest update date for the proposal</returns>
        public DateTime GetUpdateDate(UpdateableDTO inProposalDto)
        {
            DateTime updateDate = DateTime.MinValue;
            if (inProposalDto == null)
            {
                throw new ArgumentNullException(nameof(inProposalDto));
            }

            using (TransactionScope scope = new TransactionScope())
            {
                UpdateableDTO proposal = this.proposalLoader.GetById(inProposalDto.Id);
                updateDate = proposal.UpdateDate;
                scope.Complete();
            }

            return updateDate;
        }

        /// <summary>
        /// Get PPR checklist for proposal
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>PPR checklist</returns>
        public ChecklistContentDto GetPPRChecklistForProposal(int proposalId)
        {
            return this.pprContentLoader.GetChecklistByProposalId(proposalId);
        }

        /// <summary>
        /// Get PAR checklist for proposal
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>PAR checklist</returns>
        public ChecklistContentDto GetPARChecklistForProposal(int proposalId)
        {
            return this.parContentLoader.GetChecklistByProposalId(proposalId);
        }

        /// <summary>
        /// Save a default checklist as Pricer for the given proposal
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="inProposalChecklistDto">The Proposal Checklist to create</param>
        /// <param name="isSubmit">True if submit</param>
        /// <returns>Checklist Dto</returns>
        public ProposalChecklistDto SaveChecklistAsPricer(int proposalId, ProposalChecklistDto inProposalChecklistDto = null, bool isSubmit = false)
        {
            ProposalChecklistDto toReturn = null;

            using (TransactionScope scope = new TransactionScope())
            {
                ProposalChecklistDto newChecklist = inProposalChecklistDto;
                if (inProposalChecklistDto == null)
                {
                    newChecklist = new ProposalChecklistDto();
                    newChecklist.IsSubmit = isSubmit;
                    newChecklist.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalPricingReview, new ProposalChecklistSaveInfo()
                    {
                        UserID = this.GetUser().Id
                    });
                    newChecklist.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalAdequacyReview, new ProposalChecklistSaveInfo()
                    {
                        UserID = this.GetUser().Id
                    });
                    newChecklist.UserSaveInfo[ChecklistResponseType.Peer].Add(ChecklistType.ProposalAdequacyReview, new ProposalChecklistSaveInfo()
                    {
                        UserID = this.GetUser().Id
                    });
                    newChecklist.ProfitFeeWithCom = 0;
                    newChecklist.Profit = 0;
                    newChecklist.Com = 0;
                    newChecklist.ProposalID = proposalId;
                    newChecklist.EstimatingSubmitsToContractsDate = DateTime.Now;
                    newChecklist.ROSPercentage = 0;
                    newChecklist.SubmittedValue = 0;
                    newChecklist.LMLaborHrs = 0;
                    newChecklist.LMLaborCost = 0;
                    newChecklist.SubcontractorCost = 0;
                    newChecklist.MaterialCost = 0;
                    newChecklist.IWTACost = 0;
                    newChecklist.TravelCost = 0;
                    newChecklist.OtherDirectCosts = 0;
                    newChecklist.ResponseType = ChecklistResponseType.Pricer;
                    newChecklist.UpdateDate = DateTime.Now;
                    newChecklist.Updateable = UpdateType.Upsert;
                }

                this.proposalChecklistLoader.Save(newChecklist);

                scope.Complete();
            }

            toReturn = this.proposalChecklistLoader.GetByProposalIds(new Collection<int>() { proposalId }).First();
            return toReturn;
        }

        /// <summary>
        /// Save a default checklist as Peer for the given proposal
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="inProposalChecklistDto">The Proposal Checklist to create</param>
        /// <param name="isSubmit">True if submit</param>
        /// <returns>Checklist Dto</returns>
        public ProposalChecklistDto SaveChecklistAsPeer(int proposalId, ProposalChecklistDto inProposalChecklistDto = null, bool isSubmit = false)
        {
            ProposalChecklistDto toReturn = null;

            using (TransactionScope scope = new TransactionScope())
            {
                ProposalChecklistDto newChecklist = inProposalChecklistDto;
                if (newChecklist == null)
                {
                    newChecklist = new ProposalChecklistDto();
                    newChecklist.IsSubmit = isSubmit;
                    newChecklist.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalPricingReview, new ProposalChecklistSaveInfo()
                    {
                        UserID = this.GetUser().Id
                    });
                    newChecklist.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalAdequacyReview, new ProposalChecklistSaveInfo()
                    {
                        UserID = this.GetUser().Id
                    });
                    newChecklist.UserSaveInfo[ChecklistResponseType.Peer].Add(ChecklistType.ProposalAdequacyReview, new ProposalChecklistSaveInfo()
                    {
                        UserID = this.GetUser().Id
                    });
                    newChecklist.ProposalID = proposalId;
                    newChecklist.ResponseType = ChecklistResponseType.Peer;
                    newChecklist.UpdateDate = DateTime.Now;
                    newChecklist.Updateable = UpdateType.Upsert;
                }

                this.proposalChecklistLoader.Save(newChecklist);

                scope.Complete();
            }

            toReturn = this.proposalChecklistLoader.GetByProposalIds(new Collection<int>() { proposalId }).First();
            return toReturn;
        }

        /// <summary>
        /// Set proposal status
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="newStatus">New status</param>
        public void SetProposalStatus(int proposalId, ProposalStatus newStatus)
        {
            ProposalDto proposal = this.proposalLoader.GetById(proposalId);
            this.proposalLoader.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, newStatus);
        }

        #endregion Get

        #region Set

        /// <summary>
        /// Set Anticipated delivery date for a given proposal id
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="deliveryDate">Anticipated delivery date</param>
        public void SetAnticipatedDeliveryDate(int proposalId, DateTime deliveryDate)
        {
            using (genTRACEntities dbModel = new genTRACEntities())
            {
                Proposal proposal = (from x in dbModel.Proposals
                                     where x.ProposalID == proposalId
                                     select x).FirstOrDefault();

                if (proposal != null)
                {
                    proposal.AnticipatedDeliveryDate = deliveryDate;
                    dbModel.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Sets the workflow status.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="statusLastUpdated">The status last updated.</param>
        public void SetWorkflowStatus(int proposalId, WorkflowStatus status, DateTime? statusLastUpdated = null)
        {
            using (genTRACEntities dbModel = new genTRACEntities())
            {
                Proposal proposal = (from x in dbModel.Proposals
                                     where x.ProposalID == proposalId
                                     select x).FirstOrDefault();

                if (proposal != null)
                {
                    proposal.WorkflowStatus = (int)status;
                    proposal.WorkflowStatusLastUpdated = statusLastUpdated;
                    dbModel.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Sets the Submit Date
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="submitDate">Submit Date</param>
        public void SetSubmitDate(int proposalId, DateTime submitDate)
        {
            using (genTRACEntities dbModel = new genTRACEntities())
            {
                ICollection<ProposalChecklistComplete> pccList = (from x in dbModel.ProposalChecklistCompletes
                           where x.ProposalID == proposalId
                           select x).ToCollection();

                foreach(ProposalChecklistComplete pCC in pccList)
                {
                    pCC.SubmitDate = submitDate;
                    dbModel.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Sets the Contracts Data Customer Submit Date
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="submitDate">Submit Date</param>
        public void SetCustomerSubmittalDate(int proposalId, DateTime submitDate)
        {
            using (genTRACEntities dbModel = new genTRACEntities())
            {
                ProposalContractsData contract = dbModel.ProposalContractsDatas.FirstOrDefault(x => x.ProposalID == proposalId);

                if (contract == null)
                {
                    contract = new ProposalContractsData
                    {
                        ProposalID = proposalId,
                        UpdateDT = DateTime.Now,
                        PreviouslySubmittedROM = this.ProposalLoader.GetAllSlim().Last().Id, // FK, must exist
                        ContractsCorrespondLogNumber = "abc123",
                        CustomerSubmittalDate = submitDate
                    };

                    dbModel.ProposalContractsDatas.Add(contract);
                }
                else
                {
                    contract.CustomerSubmittalDate = submitDate;
                }

                dbModel.SaveChanges();
            }
        }

        #endregion

        #region Reset

        /// <summary>
        /// Resets the proposals so the next request will create a new one.
        /// </summary>
        private void ResetProposals()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                foreach (ProposalDto proposal in this.proposalList.OrderByDescending(x => x.Id).ToList())
                {
                    // get the current update time
                    ProposalDto toDelete = this.proposalLoader.GetById(proposal.Id);

                    // delete
                    if (toDelete != null)
                    {
                        toDelete.Updateable = UpdateType.Deleted;
                        this.proposalLoader.Save(toDelete);
                    }
                }

                scope.Complete();
            }

            // Reset
            this.proposalList.Clear();
            this.genericProposal = null;
        }

        #endregion Reset

        #endregion Proposals

        #region System Permissions

        #region Setup

        /// <summary>
        /// System Permissions Loader
        /// </summary>
        private ISystemPermissionLoader systemPermissionsLoader = new SystemPermissionLoader();

        /// <summary>
        /// Generic Permission
        /// </summary>
        private SystemPermissionDto genericPermissions = null;

        /// <summary>
        /// A collection of all permissions created through this class
        /// </summary>
        private List<SystemPermissionDto> permissionsCollection = new List<SystemPermissionDto>();

        /// <summary>
        /// A generic permissions
        /// </summary>
        private SystemPermissionDto permissions1 = new SystemPermissionDto()
        {
            Role = PtmRole.Admin
        };

        #endregion Setup

        #region Get

        /// <summary>
        /// Gets a permission, will create the default one if none exist
        /// </summary>
        /// <param name="inCreateNew">Whether or not to create a new proposal</param>
        /// <param name="inPermissionsDto">permission</param>
        /// <returns>The proposal permissions</returns>
        public SystemPermissionDto GetPermission(bool inCreateNew = false, SystemPermissionDto inPermissionsDto = null)
        {
            SystemPermissionDto toReturn = null;

            if (inCreateNew)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    if (inPermissionsDto == null)
                    {
                        UserDTO testUser = this.GetUser();

                        SystemPermissionDto newPermissions = GenBOEUtilities.Clone<SystemPermissionDto>(this.permissions1);
                        newPermissions.UserId = testUser.Id;
                        newPermissions.Updateable = UpdateType.Upsert;

                        toReturn = this.systemPermissionsLoader.GetById(this.systemPermissionsLoader.Save(inPermissionsDto).Value);

                        if (toReturn != null)
                        {
                            this.permissionsCollection.Add(toReturn);
                        }
                    }
                    else
                    {
                        toReturn = this.systemPermissionsLoader.GetById(this.systemPermissionsLoader.Save(inPermissionsDto).Value);
                        this.permissionsCollection.Add(toReturn);
                    }

                    scope.Complete();
                }
            }
            else
            {
                if (this.genericPermissions == null)
                {
                    this.genericPermissions = this.GetPermission(true);
                }

                toReturn = this.genericPermissions;
            }

            return toReturn;
        }

        #endregion Get

        #region Reset

        /// <summary>
        /// Resets the Permissions so the next request creates new ones.
        /// </summary>
        private void ResetPermissions()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                foreach (SystemPermissionDto permission in this.permissionsCollection)
                {
                    // get the current update time
                    var toDelete = this.systemPermissionsLoader.GetById(permission.Id);

                    // delete
                    if (toDelete != null)
                    {
                        toDelete.Updateable = UpdateType.Deleted;
                        this.systemPermissionsLoader.Save(toDelete);
                    }
                }

                scope.Complete();
            }

            // Reset
            this.permissionsCollection.Clear();
            this.permissions1 = null;
            this.genericUser = null;
            this.genericPermissions = null;
        }

        #endregion Reset

        #endregion System Permissions

        #region Proposal Permissions

        #region Setup

        /// <summary>
        /// proposal Permissions Loader
        /// </summary>
        private IProposalPermissionLoader proposalPermissionsLoader = new ProposalPermissionLoader();

        /// <summary>
        /// Generic Proposal Permission
        /// </summary>
        private ProposalPermissionDto genericProposalPermissions = null;

        /// <summary>
        /// A collection of all permissions created through this class
        /// </summary>
        private List<ProposalPermissionDto> proposalPermissionsCollection = new List<ProposalPermissionDto>();

        /// <summary>
        /// A generic permissions
        /// </summary>
        private ProposalPermissionDto propsalPermissions1 = new ProposalPermissionDto()
        {
            Role = PtmRole.Pricer

            // user will be set to the test user
            // capture will be set to capture1
        };

        #endregion Setup

        #region Get

        /// <summary>
        /// Gets a proposal permission, will create the default one if none exist
        /// </summary>
        /// <param name="inCreateNew">Whether or not to create a new proposal</param>
        /// <param name="inPermissionsDto">permission</param>
        /// <returns>The proposal permissons</returns>
        public ProposalPermissionDto GetProposalPermission(bool inCreateNew = false, ProposalPermissionDto inPermissionsDto = null)
        {
            ProposalPermissionDto toReturn = null;

            if (inCreateNew)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    if (inPermissionsDto == null)
                    {
                        ProposalDto testProposal = this.GetProposal();
                        UserDTO testUser = this.GetUser();

                        ProposalPermissionDto newPermissions = GenBOEUtilities.Clone<ProposalPermissionDto>(this.propsalPermissions1);
                        newPermissions.UserId = testUser.Id;
                        newPermissions.Updateable = UpdateType.Upsert;
                        newPermissions.ProposalID = testProposal.Id;

                        toReturn = this.proposalPermissionsLoader.GetById(this.proposalPermissionsLoader.Save(newPermissions).Value);
                        if (toReturn != null)
                        {
                            this.proposalPermissionsCollection.Add(toReturn);
                        }
                    }
                    else
                    {
                        toReturn = this.proposalPermissionsLoader.GetById(this.proposalPermissionsLoader.Save(inPermissionsDto).Value);
                        this.proposalPermissionsCollection.Add(toReturn);
                    }

                    scope.Complete();
                }
            }
            else
            {
                if (this.genericProposalPermissions == null)
                {
                    this.genericProposalPermissions = this.GetProposalPermission(true);
                }

                toReturn = this.genericProposalPermissions;
            }

            return toReturn;
        }

        #endregion Get

        #region Reset

        /// <summary>
        /// Resets the Proposal Permissions so the next request creates new ones.
        /// </summary>
        private void ResetProposalPermissions()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                foreach (ProposalPermissionDto permission in this.proposalPermissionsCollection)
                {
                    // get the current update time
                    var toDelete = this.proposalPermissionsLoader.GetById(permission.Id);

                    // delete
                    if (toDelete != null)
                    {
                        toDelete.Updateable = UpdateType.Deleted;
                        this.proposalPermissionsLoader.Save(toDelete);
                    }
                }

                scope.Complete();
            }

            // Reset
            this.proposalPermissionsCollection.Clear();
            this.proposalPricer = null;
            this.genericUser = null;
            this.genericProposalPermissions = null;
        }

        #endregion Reset

        #endregion Proposal Permissions
    }
}
