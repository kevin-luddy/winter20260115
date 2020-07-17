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
    using IES.Common;
    using GenBOE.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.Dtos;

    [TestClass]
    public class BoeApproverResponseDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_GetBOEApprovalDTOByApprovalID()
        {
            // Arrange
            var sut = new BoeApproverResponseDTODataLoader();

            // set up BOE Approver
            int etiUserID = this._CreateNewUserSaved().UserID;

            int beforeApproverCount = sut.GetIdsByBoeID(this.Boe1.Id).Count;
            Collection<BoeApproverResponseDTO> boeApprovers = new Collection<BoeApproverResponseDTO>();
            BoeApproverResponseDTO boeApprover = new BoeApproverResponseDTO();
            boeApprover.Id = -1;
            boeApprover.BoeID = this.Boe1.Id;
            boeApprover.ETIUserID = etiUserID;
            boeApprover.Updateable = UpdateType.Upsert;
            boeApprover.CurrentUserETIUserID = etiUserID;
            boeApprovers.Add(boeApprover);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(boeApprovers);
                scope.Complete();
            }

            int afterApproverSaveCount = sut.GetIdsByBoeID(this.Boe1.Id).Count;
            Assert.AreEqual(beforeApproverCount + 1, afterApproverSaveCount, "The save didn't work");

            int boeApprovalID = sut.GetIdsByBoeID(this.Boe1.Id).First();
            // Act
            BoeApproverResponseDTO boeApproverReturn = sut.GetById(boeApprovalID);

            //Assert
            Assert.AreEqual(boeApproverReturn.BoeID, this.Boe1.Id, "The BOEs did not match");
            Assert.IsTrue(boeApproverReturn.Id > 0, "boe approval wasn't a positive value");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_SaveBoeApproval()
        {
            UserDTO newApprover1 = this._CreateNewUserSaved();
            UserDTO newApprover2 = this._CreateNewUserSaved();
            // Arrange
            var sut = new BoeApproverResponseDTODataLoader();
            int beforeSaveCount = sut.GetIdsByBoeID(this.Boe1.Id).Count;
            Collection<BoeApproverResponseDTO> approvers = new Collection<BoeApproverResponseDTO>();
            
            // save approver to boe
            BoeApproverResponseDTO boeApprover = new BoeApproverResponseDTO();
            int etiUser1 = newApprover1.UserID;
            boeApprover.Id = -1;
            boeApprover.BoeID = this.Boe1.Id;
            boeApprover.ETIUserID = etiUser1;
            boeApprover.ApproverResponse = ApproverReponseType.None;
            boeApprover.Updateable = UpdateType.Upsert;
            boeApprover.UpdateDate = DateTime.Now;
            boeApprover.CurrentUserETIUserID = etiUser1;

            approvers.Add(boeApprover);

            BoeApproverResponseDTO boeApprover2 = new BoeApproverResponseDTO();
            int etiUser2 = newApprover2.UserID;
            boeApprover2.Id = -2;
            boeApprover2.BoeID = this.Boe1.Id;
            boeApprover2.ETIUserID = etiUser2;
            boeApprover2.ApproverResponse = ApproverReponseType.None;
            boeApprover2.Updateable = UpdateType.Upsert;
            boeApprover2.UpdateDate = DateTime.Now;
            boeApprover2.CurrentUserETIUserID = etiUser2;

            approvers.Add(boeApprover2);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(approvers);
                scope.Complete();
            }

            int AfterSaveCount = sut.GetIdsByBoeID(this.Boe1.Id).Count;
            Assert.AreEqual(beforeSaveCount + 2, AfterSaveCount, "the save didn't work");

            // Now that we saved an approver, save an approver comment
            var allApprovers = sut.GetAll();
            approvers = new Collection<BoeApproverResponseDTO>((from a in allApprovers
                                                                where a.BoeID == this.Boe1.Id
                                                                select a).ToArray());

            Collection<BoeApproverResponseDTO> approversToApprove = new Collection<BoeApproverResponseDTO>();
            // set all Approvers to Accepted
            foreach (BoeApproverResponseDTO ba in approvers)
            {
                ba.ApproverResponse = ApproverReponseType.Approved;
                ba.Updateable = UpdateType.Upsert;
                approversToApprove.Add(ba);
            }

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(approversToApprove);
                scope.Complete();
            }

            //assert that the response is approved
            Assert.IsTrue(sut.GetById(sut.GetIdsByBoeID(this.Boe1.Id).First()).ApproverResponse == ApproverReponseType.Approved, "no reponse");
            Assert.IsTrue(sut.GetById(sut.GetIdsByBoeID(this.Boe1.Id).First()).ApproverResponded == true, "no reponse");


            //now set one of the respones to reject. by doing this it will change all the responses back to null
            BoeApproverResponseDTO rejectApprover = sut.GetById(sut.GetIdsByBoeID(this.Boe1.Id).First());
            rejectApprover.ApproverResponse = ApproverReponseType.Rejected;
            rejectApprover.Updateable = UpdateType.Upsert;
            
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(new Collection<BoeApproverResponseDTO> { rejectApprover });
                scope.Complete();
            }

            Assert.IsTrue(sut.GetById(sut.GetIdsByBoeID(this.Boe1.Id).First()).ApproverResponse == ApproverReponseType.None, "no reponse");
            Assert.IsTrue(sut.GetById(sut.GetIdsByBoeID(this.Boe1.Id).First()).ApproverResponded == null, "no reponse");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_DeleteBoeApprover()
        {
            UserDTO newApprover1 = this._CreateNewUserSaved();
            UserDTO newApprover2 = this._CreateNewUserSaved();
            // Arrange
            var sut = new BoeApproverResponseDTODataLoader();
            int beforeSaveCount = sut.GetIdsByBoeID(this.Boe1.Id).Count;
            Collection<BoeApproverResponseDTO> approvers = new Collection<BoeApproverResponseDTO>();
            // save approver to boe
            BoeApproverResponseDTO boeApprover = new BoeApproverResponseDTO();
            int etiUser1 = newApprover1.UserID;
            boeApprover.Id = -1;
            boeApprover.BoeID = this.Boe1.Id;
            boeApprover.ETIUserID = etiUser1;
            boeApprover.ApproverResponse = ApproverReponseType.None;
            boeApprover.CurrentUserETIUserID = etiUser1;
            boeApprover.Updateable = UpdateType.Upsert;
            boeApprover.UpdateDate = DateTime.Now;

            approvers.Add(boeApprover);

            BoeApproverResponseDTO boeApprover2 = new BoeApproverResponseDTO();
            int etiUser2 = newApprover2.UserID;
            boeApprover2.Id = -2;
            boeApprover2.BoeID = this.Boe1.Id;
            boeApprover2.ETIUserID = etiUser2;
            boeApprover2.ApproverResponse = ApproverReponseType.None;
            boeApprover2.CurrentUserETIUserID = etiUser2;
            boeApprover2.Updateable = UpdateType.Upsert;
            boeApprover2.UpdateDate = DateTime.Now;

            approvers.Add(boeApprover2);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(approvers);
                scope.Complete();
            }

            int AfterSaveCount = sut.GetIdsByBoeID(this.Boe1.Id).Count;
            Assert.AreEqual(beforeSaveCount + 2, AfterSaveCount, "the save didn't work");

            //now that we have approvers, delete one and add one at the same time
            BoeApproverResponseDTO deleteApprover = sut.GetById(sut.GetIdsByBoeID(this.Boe1.Id).First());
            deleteApprover.Updateable = UpdateType.Deleted;
            // set the current user
            deleteApprover.CurrentUserETIUserID = etiUser2;

            BoeApproverResponseDTO boeApprover3 = new BoeApproverResponseDTO();
            int etiUser3 = this._CreateNewUserSaved().UserID;
            boeApprover3.BoeID = this.Boe1.Id;
            boeApprover3.ETIUserID = etiUser3;
            boeApprover3.ApproverResponse = ApproverReponseType.None;
            boeApprover3.CurrentUserETIUserID = etiUser3;
            boeApprover3.Updateable = UpdateType.Upsert;
            boeApprover3.UpdateDate = DateTime.Now;

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(new Collection<BoeApproverResponseDTO> { deleteApprover, boeApprover3 });
                scope.Complete();
            }

            int AfterDeleteCount = sut.GetIdsByBoeID(this.Boe1.Id).Count;
            Assert.AreEqual(AfterDeleteCount, AfterSaveCount, "the save didn't work");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetBoeApprovalIdsByBoeID()
        {
            UserDTO newApprover = this._CreateNewUserSaved();

            var sut = new BoeApproverResponseDTODataLoader();
            int beforeSaveCount = sut.GetIdsByBoeID(this.Boe1.Id).Count;
            Collection<BoeApproverResponseDTO> approvers = new Collection<BoeApproverResponseDTO>();
            // save approver to boe
            BoeApproverResponseDTO boeApprover = new BoeApproverResponseDTO();
            int etiUser1 = newApprover.UserID;
            boeApprover.BoeID = this.Boe1.Id;
            boeApprover.ETIUserID = etiUser1;
            boeApprover.ApproverResponse = ApproverReponseType.None;
            boeApprover.Updateable = UpdateType.Upsert;
            boeApprover.UpdateDate = DateTime.Now;
            boeApprover.CurrentUserETIUserID = etiUser1;

            approvers.Add(boeApprover);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(approvers);
                scope.Complete();
            }

            int AfterSaveCount = sut.GetIdsByBoeID(this.Boe1.Id).Count;
            Assert.AreEqual(beforeSaveCount + 1, AfterSaveCount, "the save didn't work");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetAllBoeApprovals()
        {
            var sut = new BoeApproverResponseDTODataLoader();

            ICollection<BoeApproverResponseDTO> allApprovals_initial = sut.GetAll();

            Collection<BoeApproverResponseDTO> approvers = new Collection<BoeApproverResponseDTO>();
            // save approver to boe
            BoeApproverResponseDTO boeApprover = new BoeApproverResponseDTO();
            int etiUser1 = this._CreateNewUserSaved().UserID;
            boeApprover.BoeID = this.Boe1.Id;
            boeApprover.ETIUserID = etiUser1;
            boeApprover.ApproverResponse = ApproverReponseType.None;
            boeApprover.CurrentUserETIUserID = etiUser1;
            boeApprover.Updateable = UpdateType.Upsert;
            boeApprover.UpdateDate = DateTime.Now;

            approvers.Add(boeApprover);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(approvers);
                scope.Complete();
            }

            ICollection<BoeApproverResponseDTO> allApprovals_after = sut.GetAll();
            Assert.AreEqual(1, allApprovals_after.Count() - allApprovals_initial.Count(), "expected 1 approver to be added");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_SaveBoeApproverResponses()
        {
            // Arrange
            var sut = new BoeApproverResponseDTODataLoader();
            int beforeSaveCount = sut.GetIdsByBoeID(this.Boe1.Id).Count;
            Collection<BoeApproverResponseDTO> approvers = new Collection<BoeApproverResponseDTO>();
            // save approver to boe
            BoeApproverResponseDTO boeApprover = new BoeApproverResponseDTO();
            int etiUser1 = this._CreateNewUserSaved().UserID;
            boeApprover.BoeID = this.Boe1.Id;
            boeApprover.ETIUserID = etiUser1;
            boeApprover.ApproverResponse = ApproverReponseType.None;
            boeApprover.Updateable = UpdateType.Upsert;
            boeApprover.UpdateDate = DateTime.Now;
            boeApprover.CurrentUserETIUserID = etiUser1;

            approvers.Add(boeApprover);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(approvers);
                scope.Complete();
            }

            int AfterSaveCount = sut.GetIdsByBoeID(this.Boe1.Id).Count;
            Assert.AreEqual(beforeSaveCount + 1, AfterSaveCount, "the save didn't work");

            // Now that we saved an approver, save an approver comment
            var allApprovers = sut.GetAll();
            approvers = new Collection<BoeApproverResponseDTO>((from a in allApprovers
                                                                where a.BoeID == this.Boe1.Id
                                                                select a).ToArray());

            Collection<BoeApproverResponseDTO> approversToApprove = new Collection<BoeApproverResponseDTO>();
            // set all Approvers to Accepted
            foreach (BoeApproverResponseDTO ba in approvers)
            {
                ba.ApproverResponse = ApproverReponseType.Approved;
                ba.Updateable = UpdateType.Upsert;
                approversToApprove.Add(ba);
            }

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(approversToApprove);
                scope.Complete();
            }

            //assert that the response is approved
            Assert.IsTrue(sut.GetById(sut.GetIdsByBoeID(this.Boe1.Id).First()).ApproverResponse == ApproverReponseType.Approved, "no reponse");
            Assert.IsTrue(sut.GetById(sut.GetIdsByBoeID(this.Boe1.Id).First()).ApproverResponded == true, "no reponse");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_SaveBoeApproverResponseType()
        {
            // Arrange
            var sut = new BoeApproverResponseDTODataLoader();

            UserDTO newApprover = this._CreateNewUserSaved();

            int beforeSaveCount = sut.GetIdsByBoeID(this.Boe3.Id).Count;
            Collection<BoeApproverResponseDTO> approvers = new Collection<BoeApproverResponseDTO>();
            // save approver to boe
            BoeApproverResponseDTO boeApprover = new BoeApproverResponseDTO();
            int etiUser1 = newApprover.UserID;
            boeApprover.BoeID = this.Boe3.Id;
            boeApprover.ETIUserID = etiUser1;
            boeApprover.ApproverResponse = ApproverReponseType.None;
            boeApprover.Updateable = UpdateType.Upsert;
            boeApprover.UpdateDate = DateTime.Now;
            boeApprover.CurrentUserETIUserID = etiUser1;

            approvers.Add(boeApprover);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(approvers);
                scope.Complete();
            }

            int AfterSaveCount = sut.GetIdsByBoeID(this.Boe3.Id).Count;
            Assert.AreEqual(beforeSaveCount + 1, AfterSaveCount, "the save didn't work");

            // Now that we saved an approver, save an approver comment
            var allApprovers = sut.GetAll();
            approvers = new Collection<BoeApproverResponseDTO>((from a in allApprovers
                                                                where a.BoeID == this.Boe3.Id
                                                                select a).ToArray());

            Collection<BoeApproverResponseDTO> approversToApprove = new Collection<BoeApproverResponseDTO>();
            // set all Approvers to Accepted
            foreach (BoeApproverResponseDTO ba in approvers)
            {
                ba.ApproverResponse = ApproverReponseType.Approved;
                ba.Updateable = UpdateType.Upsert;
                approversToApprove.Add(ba);
            }

            using (TransactionScope scope = new TransactionScope())
            {
                foreach (BoeApproverResponseDTO response in approversToApprove)
                {
                    sut.Save(response);
                }
                scope.Complete();
            }

            allApprovers = sut.GetAll();
            approvers = new Collection<BoeApproverResponseDTO>((from a in allApprovers
                                                                where a.BoeID == this.Boe1.Id
                                                                select a).ToArray());
            Assert.AreEqual(approvers.Where(x => x.ApproverResponse == ApproverReponseType.Approved).Count(), 0, "there was an approval");
            Assert.AreEqual(approvers.Where(x => x.ApproverResponse == ApproverReponseType.Rejected).Count(), 0, "there was a rejection");
            int ApproversWithNoneResponse = approvers.Where(x => x.ApproverResponse == ApproverReponseType.None).Count();
            Assert.AreEqual(ApproversWithNoneResponse, approvers.Count, "there were approvers that weren't in none");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetApprovalUserIdsByWorkspaceId()
        {
            // Arrange
            var sut = new BoeApproverResponseDTODataLoader();

            // set up BOE Approver
            int etiUserID = this._CreateNewUserSaved().UserID;

            Collection<BoeApproverResponseDTO> boeApprovers = new Collection<BoeApproverResponseDTO>();
            BoeApproverResponseDTO boeApprover = new BoeApproverResponseDTO();
            boeApprover.Id = -1;
            boeApprover.BoeID = this.Boe1.Id;
            boeApprover.ETIUserID = etiUserID;
            boeApprover.Updateable = UpdateType.Upsert;
            boeApprover.CurrentUserETIUserID = etiUserID;
            boeApprovers.Add(boeApprover);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(boeApprovers);
                scope.Complete();
            }

            List<int> userIds = sut.GetByBoeIds(new List<int> { this.Boe1.Id, this.Boe2.Id, this.Boe3.Id }).Select<BoeApproverResponseDTO, int>(b => b.ETIUserID).Distinct().ToList();

            List<int> userIdsFromWs = sut.GetApprovalUserIdsByWorkspaceId(this.Boe1.WorkspaceID).ToList();

            //Assert
            Assert.AreEqual(userIds.Count, userIdsFromWs.Count);

            foreach (int id in userIds)
            {
                Assert.IsTrue(userIdsFromWs.Contains(id));
            }

            this.ResetTestData();
        }
    }
}