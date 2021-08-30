// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IES.Common;
using GenBOE.Dtos;

namespace GenBOE.ActionLogic.ModelView.BOE
{

    public class BOECommentsModelView : PersistedDataModelView
    {
        public BOECommentsModelView()
        {
            this.Approver = null;
            this.Comments = new Collection<BOEComment>();
        }

        public BOECommentsModelView(int currentUserID, IReadOnlyCollection<BoeApproverResponseDTO> inBoeApprovers)
            : this()
        {
            if (inBoeApprovers != null)
            {
                var currentApprover = inBoeApprovers.FirstOrDefault(a => a.ETIUserID == currentUserID);

                if (currentApprover != null)
                {
                    this.Approver = new BOEApprover(currentApprover);
                }
            }
        }

        public BOEApprover Approver { get; set; }

        public Collection<BOEComment> Comments { get; set; }
    }

    public class BOEApprover : PersistedDataModelView
    {
        public BOEApprover()
        {
            this.BOEApprovalID = -1;
            this.ETIUserID = -1;
            this.ApproverResponse = ApproverReponseType.None;
            this.BoeID = -1;
            this.ApproverResponded = false;
        }

        public BOEApprover(BoeApproverResponseDTO approverInfo)
            : this()
        {
            if (approverInfo != null)
            {
                this.BOEApprovalID = approverInfo.Id;
                this.ETIUserID = approverInfo.ETIUserID;
                this.ApproverResponse = approverInfo.ApproverResponse;
                this.BoeID = approverInfo.BoeID;
                this.ApproverResponded = approverInfo.ApproverResponded;
                this.UpdateDate = approverInfo.UpdateDate;
            }
        }

        public int BOEApprovalID { get; set; }
        public int ETIUserID { get; set; }
        public ApproverReponseType ApproverResponse { get; set; }
        public int BoeID { get; set; }
        public bool? ApproverResponded { get; set; }
    }

    public class BOEComment
    {
        public BOEComment()
        {
            this.Deleted = false;
            this.ReviewerCommentID = -1;
            this.ReviewerComment = string.Empty;
            this.ReviewerID = -1;
            this.ReviewerName = string.Empty;
            this.ReviewerCommentUpdateDT = DateTime.MinValue;
            this.AuthorResponseID = -1;
            this.AuthorResponse = string.Empty;
            this.AuthorID = -1;
            this.AuthorName = string.Empty;
            this.AuthorResponseUpdateDT = DateTime.MinValue;
            this.CommentType = BOECommentType.Comment;
        }

        public BOEComment(BOECommentDTO inComment, UserDTO inCommenter)
            : this(inComment, inCommenter, null, null)
        {
        }

        public BOEComment(BOECommentDTO inComment, UserDTO inCommenter, BOECommentDTO inResponse, UserDTO inResponder) : this()
        {
            if (inComment != null)
            {
                this.ReviewerCommentID = inComment.Id;
                this.ReviewerID = inComment.BOECommentETIUserID;
                this.ReviewerComment = inComment.BOEComment;
                this.ReviewerCommentUpdateDT = inComment.UpdateDate;
            }

            if (inCommenter != null)
            {
                this.ReviewerName = inCommenter.DisplayName;
                this.ReviewerNtId = inCommenter.NTID;
            }

            if (inResponse != null)
            {
                this.AuthorResponseID = inResponse.Id;
                this.AuthorID = inResponse.BOECommentETIUserID;
                this.AuthorResponse = inResponse.BOEComment;
                this.AuthorResponseUpdateDT = inResponse.UpdateDate;
            }

            if (inResponder != null)
            {
                this.AuthorName = inResponder.DisplayName;
                this.AuthorNtId = inResponder.NTID;
            }
        }

        public bool Deleted { get; set; }

        public int ReviewerCommentID { get; set; }

        public string ReviewerComment { get; set; }

        public int ReviewerID { get; set; }

        public string ReviewerName { get; set; }

        public string ReviewerNtId { get; set; }

        // Display date as MM/DD/YYYY HH:MM AM/PM
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime ReviewerCommentUpdateDT { get; set; }

        public long ReviewerCommentUpdateDTLong
        {
            get
            {
                return this.ReviewerCommentUpdateDT.Ticks;
            }
            set
            {
                this.ReviewerCommentUpdateDT = new DateTime(value);
            }
        }

        public int AuthorResponseID { get; set; }

        public string AuthorResponse { get; set; }

        public int AuthorID { get; set; }

        public string AuthorName { get; set; }

        public string AuthorNtId { get; set; }

        // Display date as MM/DD/YYYY HH:MM AM/PM
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime AuthorResponseUpdateDT { get; set; }

        public long AuthorResponseUpdateDTLong
        {
            get
            {
                return this.AuthorResponseUpdateDT.Ticks;
            }
            set
            {
                this.AuthorResponseUpdateDT = new DateTime(value);
            }
        }
        
        public BOECommentType CommentType { get; set; }
    }

    public enum BOECommentType
    {
        Comment = 0,
        Approval = 1
    }
}
