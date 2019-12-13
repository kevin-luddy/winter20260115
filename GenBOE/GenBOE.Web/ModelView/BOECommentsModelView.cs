using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GenBOE.ActionLogic.ModelView;
using GenBOE.Common;
using GenBOE.DataBridge.DTO;

namespace GenBOE.Web.ModelView
{

    public class BOECommentsModelView : PersistedDataModelView
    {
        public BOECommentsModelView()
            : base()
        {
            Approver = null;
            Comments = new Collection<BOEComment>();
        }

        public BOECommentsModelView(int currentUserID, Collection<BoeApproverResponseDTO> inBoeApprovers)
            : this()
        {
            if (inBoeApprovers != null)
            {
                var currentApprover = inBoeApprovers.Where(a => a.ETIUserID == currentUserID).FirstOrDefault();

                if (currentApprover != null)
                {
                    Approver = new BOEApprover(currentApprover);
                }
            }
        }

        public BOEApprover Approver { get; set; }

        public Collection<BOEComment> Comments { get; set; }
    }

    public class BOEApprover : PersistedDataModelView
    {
        public BOEApprover()
            : base()
        {
            BOEApprovalID = -1;
            ETIUserID = -1;
            ApproverResponse = ApproverReponseType.None;
            BoeID = -1;
            ApproverResponded = false;
        }

        public BOEApprover(BoeApproverResponseDTO approverInfo)
            : this()
        {
            if (approverInfo != null)
            {
                BOEApprovalID = approverInfo.Id;
                ETIUserID = approverInfo.ETIUserID;
                ApproverResponse = approverInfo.ApproverResponse;
                BoeID = approverInfo.BoeID;
                ApproverResponded = approverInfo.ApproverResponded;
                UpdateDate = approverInfo.UpdateDate;
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
            Deleted = false;
            ReviewerCommentID = -1;
            ReviewerComment = string.Empty;
            ReviewerID = -1;
            ReviewerName = string.Empty;
            ReviewerCommentUpdateDT = DateTime.MinValue;
            AuthorResponseID = -1;
            AuthorResponse = string.Empty;
            AuthorID = -1;
            AuthorName = string.Empty;
            AuthorResponseUpdateDT = DateTime.MinValue;
            CommentType = BOECommentType.Comment;
        }

        public BOEComment(BOECommentDTO inComment, UserDTO inCommenter)
            : this(inComment, inCommenter, null, null)
        {
        }

        public BOEComment(BOECommentDTO inComment, UserDTO inCommenter, BOECommentDTO inResponse, UserDTO inResponder) : this()
        {
            if (inComment != null)
            {
                ReviewerCommentID = inComment.Id;
                ReviewerID = inComment.BOECommentETIUserID;
                ReviewerComment = inComment.BOEComment;
                ReviewerCommentUpdateDT = inComment.UpdateDate;
            }

            if (inCommenter != null)
            {
                ReviewerName = inCommenter.DisplayName;
            }

            if (inResponse != null)
            {
                AuthorResponseID = inResponse.Id;
                AuthorID = inResponse.BOECommentETIUserID;
                AuthorResponse = inResponse.BOEComment;
                AuthorResponseUpdateDT = inResponse.UpdateDate;
            }

            if (inResponder != null)
            {
                AuthorName = inResponder.DisplayName;
            }
        }

        public bool Deleted { get; set; }

        public int ReviewerCommentID { get; set; }

        public string ReviewerComment { get; set; }

        public int ReviewerID { get; set; }

        public string ReviewerName { get; set; }

        // Display date as MM/DD/YYYY HH:MM AM/PM
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime ReviewerCommentUpdateDT { get; set; }

        public long ReviewerCommentUpdateDTLong
        {
            get
            {
                return ReviewerCommentUpdateDT.Ticks;
            }
            set
            {
                ReviewerCommentUpdateDT = new DateTime(value);
            }
        }

        public int AuthorResponseID { get; set; }

        public string AuthorResponse { get; set; }

        public int AuthorID { get; set; }

        public string AuthorName { get; set; }

        // Display date as MM/DD/YYYY HH:MM AM/PM
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime AuthorResponseUpdateDT { get; set; }

        public long AuthorResponseUpdateDTLong
        {
            get
            {
                return AuthorResponseUpdateDT.Ticks;
            }
            set
            {
                AuthorResponseUpdateDT = new DateTime(value);
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
