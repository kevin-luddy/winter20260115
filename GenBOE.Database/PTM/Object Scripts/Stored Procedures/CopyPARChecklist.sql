IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CopyPARChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CopyPARChecklist];
GO

CREATE PROCEDURE dbo.CopyPARChecklist
(@newChecklistId AS INT) AS
/******************************************************************************
**		 
**		Name: CopyPARChecklist
**		Desc: Copies PAR Checklist into a new Checklist version. This is executed manually.
**
**		Auth: RJ
**		Date: 4/13/23
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ProposalAdequacyReview WHERE ProposalAdequacyReviewID = @newChecklistId)
	BEGIN
		UPDATE ProposalAdequacyReview SET IsCurrent = 0;

		SET IDENTITY_INSERT ProposalAdequacyReview ON;
		INSERT INTO ProposalAdequacyReview (ProposalAdequacyReviewID, ChecklistVersion, IsCurrent, ProposalChecklistTypeID)
			VALUES (@newChecklistId, @newChecklistId - 1, 1, 1);
		SET IDENTITY_INSERT ProposalAdequacyReview OFF;

		INSERT INTO PARChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalAdequacyReviewID, QuestionNumber, Reference, SubmissionItem, YesOnly)
			SELECT ChecklistText, TextTypeID, SortOrder, ColumnOrder, @newChecklistId, QuestionNumber, Reference, SubmissionItem, YesOnly
				FROM PARChecklistContent
				WHERE ProposalAdequacyReviewId = @newChecklistId - 1;
	END
END
GO