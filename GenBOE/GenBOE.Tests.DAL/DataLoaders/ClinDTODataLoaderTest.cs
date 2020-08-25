// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ClinDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_SaveCLINDTO()
        {

            //Arrange
            var sut = new ClinDTODataLoader();

            //Act
            Collection<ClinDTO> createCLINS = new Collection<ClinDTO>();

            var clins = sut.GetByWorkspaceId(this.Workspace.Id);
            Collection<ClinDTO> getBeforeCLINS = new Collection<ClinDTO>(clins.ToArray());

            // add a new CLIN
            string title = Guid.NewGuid().ToString();
            var singleCLIN = new ClinDTO();
            singleCLIN.Id = -1;
            singleCLIN.ClinNumber = "Moq" + Math.Abs(MOQObject.randomNumberGenerator.Next()).ToString();
            singleCLIN.ClinTitle = title;
            singleCLIN.UpdateDate = DateTime.Now;
            singleCLIN.Updateable = UpdateType.Upsert;
            singleCLIN.StartDate = Convert.ToDateTime("08/01/2010");
            singleCLIN.EndDate = Convert.ToDateTime("04/01/2011");
            singleCLIN.ContractType = 1003;
            singleCLIN.WorkspaceID = this.Workspace.Id;
            createCLINS.Add(singleCLIN);

            //Assert

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(createCLINS);
                scope.Complete();
            }

            var afterClins = sut.GetByWorkspaceId(this.Workspace.Id);

            Collection<ClinDTO> getAfterCLINS = new Collection<ClinDTO>(afterClins.ToArray());

            Assert.AreEqual(getBeforeCLINS.Count + 1, getAfterCLINS.Count, "The clin save did not work");


            // Now, test for handling a collection of deletes
            //Act

            Collection<ClinDTO> deleteCLINS = new Collection<ClinDTO>();

            var afterDeleteClins = sut.GetByWorkspaceId(this.Workspace.Id);
            ClinDTO toDeleteClin = new ClinDTO();
            foreach (ClinDTO checkClin in afterDeleteClins)
            {
                // find the clin we just created
                if (checkClin.ClinTitle == title)
                {
                    toDeleteClin = checkClin;
                    break;
                }
            }

            // delete the CLIN we just saved
            toDeleteClin.Updateable = UpdateType.Deleted;
            deleteCLINS.Add(toDeleteClin);

            //Assert
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(deleteCLINS);
                scope.Complete();
            }

            int afterClinCount = sut.GetByWorkspaceId(this.Workspace.Id).Count;

            Assert.AreEqual(afterDeleteClins.Count - 1, afterClinCount);
            this.ResetTestData();
        }

        [TestMethod]
        public void L_UpdateClinDTO()
        {

            var sut = new ClinDTODataLoader();

            ClinDTO singleClin = new ClinDTO();
            Collection<ClinDTO> getBeforeClins = new Collection<ClinDTO>();
            Collection<ClinDTO> getAfterClins = new Collection<ClinDTO>();

            var beforeClins = sut.GetByWorkspaceId(this.Workspace.Id);
            getBeforeClins = new Collection<ClinDTO>(beforeClins.ToArray());

            //create a CLIN
            // in MOQ tests, you can't verify required or out of range values
            singleClin.Id = -1;
            singleClin.ClinNumber = "Moq" + Math.Abs(MOQObject.randomNumberGenerator.Next()).ToString();
            singleClin.ClinTitle = Guid.NewGuid().ToString();
            singleClin.UpdateDate = DateTime.Now;
            singleClin.Updateable = UpdateType.Upsert;
            singleClin.StartDate = Convert.ToDateTime("08/01/2010");
            singleClin.EndDate = Convert.ToDateTime("04/01/2011");
            singleClin.ContractType = 1008;
            singleClin.WorkspaceID = this.Workspace.Id;

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(singleClin);
                scope.Complete();
            }

            var afterCLins = sut.GetByWorkspaceId(this.Workspace.Id);

            getAfterClins = new Collection<ClinDTO>(afterCLins.ToArray());

            Assert.AreEqual(getBeforeClins.Count + 1, getAfterClins.Count);

            // edit clin
            singleClin = getAfterClins.ToArray()[Math.Abs(MOQObject.randomNumberGenerator.Next(getAfterClins.Count - 1))];
            singleClin.StartDate = Convert.ToDateTime("09/01/2010");
            singleClin.Updateable = UpdateType.Upsert;
            int saveClinID = singleClin.Id;
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(singleClin);
                scope.Complete();
            }

            var afterCLins2 = sut.GetByWorkspaceId(this.Workspace.Id);
            getAfterClins = new Collection<ClinDTO>(afterCLins2.ToArray());

            foreach (ClinDTO clin in getAfterClins)
            {
                if (clin.Id == saveClinID)
                {
                    // loader auto converts to 15th of month to avoid any datetime rollover issues since month/year is only thing that matters
                    Assert.AreEqual(Convert.ToDateTime("9/15/2010 12:00:00 PM"), clin.StartDate);
                    break;
                }
            }

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetBoeCountByCLINID()
        {
            var sut = new ClinDTODataLoader();

            // Get CLINs by workspace ID
            Collection<ClinDTO> results = sut.GetByWorkspaceId(this.Workspace.Id);

            int count = sut.GetBoeCountByClinID(results[0].Id);

            // it's valid for the CLIN not to be attached to a BOE yet
            Assert.IsTrue(count >= 0, "Something went wrong");

        }
        /// <summary>
        /// This is not a test method. It's stricly to create CLINS before the Delete CLIN
        /// test case is handled
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "to be used in test method")]
        static void InsertCLINSForDeleteSelectedTest(int inWorkspaceID)
        {

            //Arrange
            var sut = new ClinDTODataLoader();

            //Act
            Collection<ClinDTO> createCLINS = new Collection<ClinDTO>();

            int clinIDCount = -1;

            // create 3 new CLINS
            for (int x = 0; x < 3; x++)
            {
                var singleCLIN = new ClinDTO();
                singleCLIN.Id = clinIDCount;
                singleCLIN.ClinNumber = "Moq" + Math.Abs(MOQObject.randomNumberGenerator.Next()).ToString() + clinIDCount.ToString();
                singleCLIN.ClinTitle = Guid.NewGuid().ToString();
                singleCLIN.UpdateDate = DateTime.Now;
                singleCLIN.Updateable = UpdateType.Upsert;
                singleCLIN.StartDate = Convert.ToDateTime("08/01/2010");
                singleCLIN.EndDate = Convert.ToDateTime("04/01/2011");
                singleCLIN.ContractType = Constants.CONTRACT_TYPE_NOT_SET;
                singleCLIN.WorkspaceID = inWorkspaceID;

                createCLINS.Add(singleCLIN);

                clinIDCount--;

            }


            sut.Save(createCLINS);
        }

        //[TestMethod]
        public void L_DeleteSelectedClinDTO()
        {
            // TODOJIM
            //var sut = new ClinDTODataLoader();
            //ClinDTO singleClin = new ClinDTO();
            //Collection<ClinDTO> beforeclins = new Collection<ClinDTO>();
            //Collection<ClinDTO> afterclins = new Collection<ClinDTO>();

            //InsertCLINSForDeleteSelectedTest(GlobalTestCaseSetup.GlobalWorkspaceID); // add some CLINs so we'll never have an issue with deleting CLINS that don't exist

            //Collection<int> beforeClinIDs = sut.GetCLINIdsByWorkspaceID(GlobalTestCaseSetup.GlobalWorkspaceID);
            //var beforeCLins = from id in beforeClinIDs
            //                  select sut.GetCLINById(id);

            //beforeclins = new Collection<ClinDTO>(beforeCLins.ToArray());

            //// get a random clin to delete
            //singleClin = beforeclins.ToArray()[Math.Abs(MOQObject.randomNumberGenerator.Next(beforeclins.Count - 1))];


            //sut.DeleteSelectedCLIN(singleClin);

            //Collection<int> afterClinIDs = sut.GetCLINIdsByWorkspaceID(GlobalTestCaseSetup.GlobalWorkspaceID);
            //var afterCLins = from id in afterClinIDs
            //                 select sut.GetCLINById(id);

            //afterclins = new Collection<ClinDTO>(afterCLins.ToArray());
            //Assert.AreEqual(beforeclins.Count - 1, afterclins.Count);

            //this.ResetTestData();
        }

        [TestMethod]
        public void L_GetTaskVariableIDsByCLINID()
        {
            var sut = new ClinDTODataLoader();
            // Get task variables by clin id
            Collection<int> taskVarIDs = sut.GetTaskVariableIDsByClinID(this.Clin1.Id);

            Assert.IsTrue(taskVarIDs.Count > 0, "No Task variable IDs found associated with CLIN ID");
        }

        [TestMethod]
        public void L_GetWorkspaceVariableIDsByCLINID()
        {
            var sut = new ClinDTODataLoader();
            // Get task variables by clin id
            Collection<int> workspaceVarIDs = sut.GetWorkspaceVariableIDsByClinID(this.Clin1.Id);

            Assert.IsTrue(workspaceVarIDs.Count > 0, "No workspace variable IDs found associated with CLIN ID");
        }

        [TestMethod]
        public void L_PadCLINNumber()
        {
            string result;
            var sut = new ClinDTODataLoader();

            string singleClin = "..33abcdefg";
            result = sut.PadClinNumber(singleClin);
            Assert.AreEqual(result.ToString(), "00000000000000000000??.00000000000000000000??.00000000000000000033?abcdefg?");

            singleClin = "33abcdefg";
            result = sut.PadClinNumber(singleClin);
            Assert.AreEqual(result.ToString(), "00000000000000000033?abcdefg?");

            singleClin = "5";
            result = sut.PadClinNumber(singleClin);
            Assert.AreEqual(result.ToString(), "00000000000000000005??");

            singleClin = "1a";
            result = sut.PadClinNumber(singleClin);
            Assert.AreEqual(result.ToString(), "00000000000000000001?a?");

            singleClin = "2bcde.34abc";
            result = sut.PadClinNumber(singleClin);
            Assert.AreEqual(result.ToString(), "00000000000000000002?bcde?.00000000000000000034?abc?");

            singleClin = "1.2.3.4.5.6.7.8.9";
            result = sut.PadClinNumber(singleClin);
            Assert.AreEqual(result.ToString(), "00000000000000000001??.00000000000000000002??.00000000000000000003??.00000000000000000004??.00000000000000000005??.00000000000000000006??.00000000000000000007??.00000000000000000008??.00000000000000000009??");

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "newAvg"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "originalAvg")]
        //[TestMethod]
        public void TestOldVsNew()
        {
            //ClinDTODataLoader loader = new ClinDTODataLoader();

            //Stopwatch sw = new Stopwatch();
            //List<ClinDTO> originalData = new List<ClinDTO>();
            //List<ClinDTO> newData = new List<ClinDTO>();
            //List<long> originalTimes = new List<long>();
            //List<long> newTimes = new List<long>();

            //List<int> ids = new List<int>();

            //{
            //    int maxItems = 30;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.CLINs.OrderBy(x => new Guid()).Select(x => x.CLINID).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < 5; i++) { sw.Restart(); originalData.AddRange(loader.GetByIds_OLD(ids)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < 5; i++) { sw.Restart(); newData.AddRange(loader.GetByIds(ids)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<ClinDTO>(); newData = new List<ClinDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();

            //    int maxItems = 10;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.Workspaces.Where(x => x.CLINs.Any()).OrderBy(x => new Guid()).Select(x => x.WorkspaceID).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); originalData.AddRange(loader.GetByWorkspaceId_OLD(ids.ElementAt(i))); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); newData.AddRange(loader.GetByWorkspaceId(ids.ElementAt(i))); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}
        }

        public static void VerifyCollections(ICollection<ClinDTO> collection1, ICollection<ClinDTO> collection2, bool skipFieldsNotRestoredFromBackup = false)
        {
            Assert.AreEqual(collection1.Count, collection2.Count);
            for (int i = 0; i < collection1.Count; i++)
            {
                VerifyDtos(collection1.ElementAt(i), collection2.ElementAt(i), skipFieldsNotRestoredFromBackup);
            }
        }

        public static void VerifyDtos(ClinDTO dto1, ClinDTO dto2, bool skipFieldsNotRestoredFromBackup = false)
        {
            if(!skipFieldsNotRestoredFromBackup)
            {
                Assert.AreEqual(dto1.Id, dto2.Id);
                Assert.AreEqual(dto1.WorkspaceID, dto2.WorkspaceID);
            }

            Assert.AreEqual(dto1.ClinNumber, dto2.ClinNumber);
            Assert.AreEqual(dto1.ClinPaddedNumber, dto2.ClinPaddedNumber);
            Assert.AreEqual(dto1.ClinString, dto2.ClinString);
            Assert.AreEqual(dto1.ClinTitle, dto2.ClinTitle);
            Assert.AreEqual(dto1.EndDate, dto2.EndDate);
            Assert.AreEqual(dto1.ContractType, dto2.ContractType);
            Assert.AreEqual(dto1.InUse, dto2.InUse);
            Assert.AreEqual(dto1.StartDate, dto2.StartDate);
            Assert.AreEqual(dto1.Updateable, dto2.Updateable);
            Assert.AreEqual(dto1.UpdateDate, dto2.UpdateDate);
        }
    }
}
