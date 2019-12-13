using System;
using System.Collections.ObjectModel;
using System.Transactions;
using IES.Common;
using GenBOE.DataBridge.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class BOECommentDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_GetBOECommentDTO()
        {
            // set up
            var sut = new BOECommentDTODataLoader();
            Collection<BOECommentDTO> boeComments = new Collection<BOECommentDTO>();
            ICollection<int> boeCommentIDs = new Collection<int>();

            // create a string that we can use throughout the test
            string TempComment = Guid.NewGuid().ToString();

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

            using (TransactionScope scope = new TransactionScope())
            {
                // save the comment
                sut.Save(boeComments);
                scope.Complete();
            }

            boeComments = new Collection<BOECommentDTO>();

            boeCommentIDs = sut.GetByBoeId(this.Boe1.Id).Select(x => x.Id).ToList();

            BOECommentDTO updatedComment = new BOECommentDTO();
            for (int x = 0; x < boeCommentIDs.Count; x++)
            {
                // need to loop through the BOE
                BOECommentDTO bc = sut.GetById(boeCommentIDs.ToCollection()[x]);
                if (bc.BOEComment == TempComment)
                {
                    updatedComment = bc;
                    break;
                }
            }

            Assert.IsTrue(updatedComment.BoeID > 0, "BOE ID is negative");
            Assert.AreEqual(updatedComment.BOEComment, TempComment, "The Comment did not match");
            Assert.AreEqual(updatedComment.BOEResponseToCommentID, null, "This wasn't an approver's comment");
            this.ResetTestData();
        }


        [TestMethod]
        public void L_GetBOECommentIDs()
        {
            // set up
            var sut = new BOECommentDTODataLoader();
            Collection<BOECommentDTO> boeComments = new Collection<BOECommentDTO>();
            ICollection<int> boeCommentIDs = new Collection<int>();

            // create a string that we can use throughout the test
            string TempComment = Guid.NewGuid().ToString();

            // create a boe comment by an approver (not an author) to the BOE ID
            // so we at least have one comment to get
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

            using (TransactionScope scope = new TransactionScope())
            {
                // save the comment
                sut.Save(boeComments);
                scope.Complete();
            }

            boeCommentIDs = sut.GetByBoeId(this.Boe1.Id).Select(x => x.Id).ToCollection();

            // assert that boe comment IDs come back
            Assert.IsTrue(boeCommentIDs.Count > 0, "BOE Comment does not exist");
            this.ResetTestData();
        }


        [TestMethod]
        /// Changed this function to be static so the BOEHistoryDTODataLoaderTest could use it
        public void L_SaveBoeComment()
        {
            // set up
            var sut = new BOECommentDTODataLoader();
            Collection<BOECommentDTO> boeComments = new Collection<BOECommentDTO>();
            ICollection<int> boeCommentIDs = new Collection<int>();
            int beforeBOECommentCount = 0;
            int afterBOECommentCount = 0;

            // get the count before we save the boe comments below
            boeCommentIDs = sut.GetByBoeId(this.Boe1.Id).Select(x => x.Id).ToCollection();
            beforeBOECommentCount = boeCommentIDs.Count;

            // create a string that we can use throughout the test
            string TempComment = Guid.NewGuid().ToString();

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
            string TempComment2 = Guid.NewGuid().ToString();
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

            boeComments = new Collection<BOECommentDTO>();

            boeCommentIDs = sut.GetByBoeId(this.Boe1.Id).Select(x => x.Id).ToCollection();

            // get the count after we've saved the above comments
            afterBOECommentCount = boeCommentIDs.Count;
            Assert.AreEqual(beforeBOECommentCount + 2, afterBOECommentCount, "The comment counts are wrong");

            for (int x = 0; x < boeCommentIDs.Count; x++)
            {
                // need to loop through the BOE
                BOECommentDTO bc = sut.GetById(boeCommentIDs.ToCollection()[x]);

                // Since there can be a ton of comments, check for the one of the 2 we added and then break
                if (bc.BOEComment == TempComment)
                {
                    Assert.IsTrue(bc.Id > 0, "BOE Comment ID is negative");
                    Assert.AreEqual(bc.BOEComment, TempComment, "The Comment did not match");

                    break;
                }
                else if (bc.BOEComment == TempComment2)
                {
                    Assert.IsTrue(bc.Id > 0, "BOE Comment ID is negative");
                    Assert.AreEqual(bc.BOEComment, TempComment2, "The Comment did not match");
                    break;
                }

            }

            // Now let's set up an approver's response comment to an exisiting approver's comment
            boeComments.Clear();

            int approverCommentID = boeCommentIDs.ToCollection()[0];

            string TempComment3 = Guid.NewGuid().ToString();
            BOECommentDTO AuthorResponse = new BOECommentDTO();
            AuthorResponse.Id = -2;
            AuthorResponse.FieldID = (int)FieldType.AuthorResponse;
            AuthorResponse.BOEComment = "Author " + TempComment3;
            AuthorResponse.BOEResponseToCommentID = approverCommentID; // this is how the association between author and approver is done
            AuthorResponse.BOECommentETIUserID = this.Approver1.UserID;
            AuthorResponse.BoeID = this.Boe1.Id;
            AuthorResponse.Updateable = UpdateType.Upsert;
            AuthorResponse.UpdateDate = DateTime.Now;
            boeComments.Add(AuthorResponse);

            using (TransactionScope scope = new TransactionScope())
            {
                // save the new author's response with the reponse link
                sut.Save(boeComments);
                scope.Complete();
            }

            // let's verify that the author's comment was updated with the link to the author's comment
            boeCommentIDs = sut.GetByBoeId(this.Boe1.Id).Select(x => x.Id).ToCollection();
            for (int x = 0; x < boeCommentIDs.Count; x++)
            {
                BOECommentDTO authorComment = sut.GetById(boeCommentIDs.ToCollection()[x]);
                if (authorComment.BOEResponseToCommentID == approverCommentID)
                {
                    Assert.AreEqual(authorComment.BOEResponseToCommentID, approverCommentID, "The approver comment ID link did not save");

                    break;
                }

            }

            this.ResetTestData();
        }
    }
}
