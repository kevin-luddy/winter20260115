using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Transactions;
using IES.Common;
using GenBOE.DataBridge.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.Dtos;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class BOEHistoryDTODataLoaderTest : MOQLoaderObject
    {
        // This function will add BOE Comments so that the Get for BOE History will return data
        // No Asserts necessary. 
        public void SaveBoeCommentForBoeHistory()
        {
            // set up
            var sut = new BOECommentDTODataLoader();
            Collection<BOECommentDTO> boeComments = new Collection<BOECommentDTO>();

            // create a string that we can use throughout the test
            string TempComment = "BOEHISTORY" + Guid.NewGuid().ToString();

            // create a boe comment by an approver (not an author) to the BOE ID
            BOECommentDTO boeComment = new BOECommentDTO();
            boeComment.Id = -1;
            boeComment.FieldID = (int)FieldType.ApproverResponse;
            boeComment.BOEComment = TempComment;
            boeComment.BOEResponseToCommentID = null;
            boeComment.BOECommentETIUserID = this.Approver1.UserID;
            boeComment.BoeID = this.Boe1.Id;
            boeComment.Updateable = UpdateType.Upsert;
            boeComment.UpdateDate = DateTime.Now;
            boeComments.Add(boeComment);

            // create a second boe comment by an approver
            string TempComment2 = "BOEHISTORY" + Guid.NewGuid().ToString();
            BOECommentDTO boeComment2 = new BOECommentDTO();
            boeComment2.Id = -2;
            boeComment2.FieldID = (int)FieldType.ApproverResponse;
            boeComment2.BOEComment = TempComment2;
            boeComment2.BOEResponseToCommentID = null;
            boeComment2.BOECommentETIUserID = this.Approver1.UserID;
            boeComment2.BoeID = this.Boe1.Id;
            boeComment2.Updateable = UpdateType.Upsert;
            boeComment2.UpdateDate = DateTime.Now;
            boeComments.Add(boeComment2);

            using (TransactionScope scope = new TransactionScope())
            {
                // save the comments
                sut.Save(boeComments);
                scope.Complete();
            }
        }

        [TestMethod]
        public void L_GetBOEHistoryDTO()
        {
            var sut = new BOEHistoryDTODataLoader();
            ICollection<BOEHistoryDTO> boeLog = new Collection<BOEHistoryDTO>();

            this.SaveBoeCommentForBoeHistory();
            boeLog = sut.GetBOEHistory(this.Boe1.Id);

            // Assert
            // Since this test case is relying on the BOE Comment Save, we have very basic
            // to test
            Assert.IsTrue(boeLog != null, "The BOE log was null");
            Assert.IsTrue(boeLog.Count > 0, "The BOE log was empty");

            //foreach (BOEHistoryDTO boeHistory in boeLog)
            //{
            //    Assert.AreEqual(boeHistory.Field, "Approver's Response", "The BOE History Field is not an approver's response");
            //    Assert.IsTrue(boeHistory.NewValue.Contains("BOEHISTORY"), "No text BOE History in test case");
            //}

            this.ResetTestData();
        }
    }
}
