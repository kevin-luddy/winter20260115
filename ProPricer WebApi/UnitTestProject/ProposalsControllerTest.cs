namespace UnitTestProject
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using APTSPropricerApi;
    using APTSPropricerApi.Connection;
    using APTSPropricerApi.Controllers;
    using APTSPropricerApi.DTOs;
    using EBS.ProPricer.Data;
    using EBS.ProPricer.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProposalsControllerTest
    {
        [TestMethod]
        public void Create_A_Proposal()
        {
            using (IProPricerConnection _ppc = (IProPricerConnection)PoolManager.GetInstance(TestConstants.InstanceId).GetObjectsFromPool())
            { /// Arrange

                /// Act
                using (ProposalsController prop = new ProposalsController())
                {

                    // setup a new proposal
                    ProposalDto newProp = new ProposalDto
                    {
                        Name = "__AUTOMATED TEST__",
                        //newProp.name = "80 12-00103";
                        Version = "0",
                        Description = "Created from automated test project. It can safely be deleted.",
                        /// newProp.creator_Name = "Unit Test Project";
                        Manager = "Sombody Important",
                        BusinessUnit = "Winning proposals",
                        Rfq = "1234567890",
                        Notes = "some notes about our winning proposal",
                        ParentFolderName = "Test-Dusan",

                        StartDate = "2016/01",
                        EndDate = "2016-12",
                        DueDate = "2016/11/23",

                        GlobalProfitFactor = 0.123456.ToString(),

                        DirectRateTable = "RMS-171101",
                        BurdenRateTable = "RMS-171220-R1",
                        FactorRateTable = "CER-Global-171107",
                        TravelRateTable = "hSAC Travel 2017"
                    };

                    ReturnDto newid = prop.Post(TestConstants.InstanceId, newProp);

                    /// Assert
                    Proposal pr = null;
                    EBS.ProPricer.Data.EntityId pEntityId = new EBS.ProPricer.Data.EntityId(new Guid(newid.Retmsg));
                    if (_ppc.Workspace.Proposals.Find(pEntityId).HasValue)
                    {
                        pr = _ppc.Workspace.Proposals.Find(pEntityId).Value;
                    }

                    Assert.IsFalse(pr == null);

                    newProp.Id = newid.Retmsg;
                    newProp.Description = "Updated from automated test project. It can safely be deleted.";
                    /// newProp.creator_Name = "Unit Test Project";
                    newProp.Name = "11 - 11111 - Web API";
                    newProp.Rfq = "0987654321";
                    newProp.Notes = "some updated notes about our winning proposal";

                    newid = prop.Put(TestConstants.InstanceId, newProp);
                    Assert.IsTrue(newid.Retcode == "200");

                    // delete our test proposal
                    pr.Delete();
                    Assert.IsFalse(_ppc.Workspace.Proposals.Find(pEntityId).HasValue);
                }
            }
        }

        [TestMethod]
        public void Update_A_Proposal()
        {
            using (IProPricerConnection _ppc = (IProPricerConnection)PoolManager.GetInstance(TestConstants.InstanceId).GetObjectsFromPool())
            {/// Arrange

                /// Act
                using (ProposalsController prop = new ProposalsController())
                {

                    // setup a new proposal
                    ProposalDto newProp = new ProposalDto
                    {
                        Name = "__AUTOMATED TEST__",
                        Version = "0",
                        Description = "Created from automated test project. It can safely be deleted.",
                        Manager = "Sombody Important",
                        BusinessUnit = "Winning proposals",
                        Rfq = "1234567890",
                        Notes = "some notes about our winning proposal",
                        ParentFolderName = "Test-Dusan",

                        StartDate = "2016/01",
                        EndDate = "2016-12",
                        DueDate = "2016/11/23",

                        GlobalProfitFactor = 0.123456.ToString(),

                        DirectRateTable = "RMS-171101",
                        BurdenRateTable = "RMS-171220-R1",
                        FactorRateTable = "CER-Global-171107",
                        TravelRateTable = "hSAC Travel 2017"
                    };

                    ReturnDto newid = prop.Post(TestConstants.InstanceId, newProp);

                    /// Assert
                    EBS.ProPricer.Data.EntityId pEntityId = new EBS.ProPricer.Data.EntityId(new Guid(newid.Retmsg));

                    newProp.Name = "__ UPD Auto Test__";
                    newProp.Id = newid.Retmsg;

                    newid = prop.Put(TestConstants.InstanceId, newProp);
                    Assert.IsTrue(newid.Retcode == "200");

                    Proposal pr = null;
                    if (_ppc.Workspace.Proposals.Find(pEntityId).HasValue)
                    {
                        pr = _ppc.Workspace.Proposals.Find(pEntityId).Value;
                    }

                    // delete our test proposal
                    pr.Delete();
                }
            }
        }

        [TestMethod]
        public void Delete_A_Proposal()
        {
            using (IProPricerConnection _ppc = (IProPricerConnection)PoolManager.GetInstance(TestConstants.InstanceId).GetObjectsFromPool())
            {
                // setup a new proposal
                using (ProposalsController prop = new ProposalsController())
                {

                    ProposalDto newProp = new ProposalDto
                    {
                        Name = "__AUTOMATED TEST__",
                        //newProp.name = "80 12-00103";
                        Version = "0",
                        Description = "Created from automated test project. It can safely be deleted.",
                        /// newProp.creator_Name = "Unit Test Project";
                        Manager = "Sombody Important",
                        BusinessUnit = "Winning proposals",
                        Rfq = "1234567890",
                        Notes = "some notes about our winning proposal",
                        ParentFolderName = "Test-Dusan",

                        StartDate = "2016/01",
                        EndDate = "2016-12",
                        DueDate = "2016/11/23",

                        GlobalProfitFactor = 0.123456.ToString(),

                        DirectRateTable = "RMS-171101",
                        BurdenRateTable = "RMS-171220-R1",
                        FactorRateTable = "CER-Global-171107",
                        TravelRateTable = "hSAC Travel 2017"
                    };

                    ReturnDto newid = prop.Post(TestConstants.InstanceId, newProp);

                    // Assert
                    Proposal pr = null;
                    EBS.ProPricer.Data.EntityId pEntityId = new EBS.ProPricer.Data.EntityId(new Guid(newid.Retmsg));
                    if (_ppc.Workspace.Proposals.Find(pEntityId).HasValue)
                    {
                        pr = _ppc.Workspace.Proposals.Find(pEntityId).Value;
                    }

                    Assert.IsFalse(pr == null);

                    // delete our test proposal
                    pr.Delete();
                    Assert.IsFalse(_ppc.Workspace.Proposals.Find(pEntityId).HasValue);
                }
            }
        }

        [TestMethod]
        public void Get_Proposals()
        {
            using (ProposalsController controller = new ProposalsController())
            {

                /// Act
                ICollection<ProposalFolderInfo> data = controller.Get(TestConstants.InstanceId);

                Assert.IsTrue(data.Any());
            }
        }

        /// <summary>
        /// Test to demonstrate 2 ways to navigate the folder/proposal hierarchy:
        ///     1) Recursive hierarchy traversal, and
        ///     2) Find method
        /// </summary>
        [TestMethod]
        public void Get_Folder_Info()
        {
            using (IProPricerConnection _ppc = (IProPricerConnection)PoolManager.GetInstance(TestConstants.InstanceId).GetObjectsFromPool())
            {
                // demonstrate hierarchy traversal
                ProposalsController controller = new ProposalsController();
                FolderCollection topLevelFolders = _ppc.Workspace.GlobalLibrary.Folders;
                List<string> names = new List<string>();
                // only search the first 2 folders (so the test runs faster)
                Collection<Folder> folders = new Collection<Folder> {_ppc.Workspace.GlobalLibrary.Folders[0], _ppc.Workspace.GlobalLibrary.Folders[1] };
                getFolders(folders, string.Empty, names);
                Assert.IsTrue(names.Count > 0);

                // demonstrate Find method
                string ParentFolderName = "Test-Dusan";
                Folder tofolder = _ppc.Workspace.GlobalLibrary.Folders.Find(ParentFolderName, FolderCategory.Proposal, null).Value;
                Assert.IsNotNull(tofolder);
            }
        }

        /// <summary>
        /// Recursive method to traverse the folder/proposal hierarchy.
        /// </summary>
        /// <param name="folders">Set of folders to traverse</param>
        /// <param name="path">Path to folders</param>
        /// <param name="names">Collection of folder and proposal names gathered so far</param>
        private void getFolders(IEnumerable<Folder> folders, string path, List<string> names)
        {
            foreach (Folder folder in folders)
            {
                string fullPath = path + folder.Name;  // or, could use folder.GetPath();
                names.Add("Folder:   " + fullPath);
                foreach (var item in folder.GetItems())
                {
                    if (item is Proposal)
                    {
                        User actualOwner = ((Proposal)item).ActualOwner;
                        string actualOwnerLogin = actualOwner == null ? string.Empty : actualOwner.LoginName;
                        User creator = ((Proposal)item).Creator;
                        string creatorLogin = creator == null ? string.Empty : creator.LoginName;
                        names.Add("Proposal: " + fullPath + "//" + ((Proposal)item).Name +
                            " Actual Owner: " + actualOwnerLogin +
                            " Creator: " + creatorLogin);
                    }
                }

                getFolders(folder.GetSubFolders(), fullPath, names);
            }
        }
    }
}