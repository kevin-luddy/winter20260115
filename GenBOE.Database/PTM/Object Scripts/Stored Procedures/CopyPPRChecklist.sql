IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CopyPPRChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CopyPPRChecklist];
GO

CREATE PROCEDURE dbo.CopyPPRChecklist
(@newChecklistId AS INT) AS
/******************************************************************************
**		 
**		Name: CopyPPRChecklist
**		Desc: Copies PPR Checklist into a new Checklist version. This is executed manually.
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
	IF NOT EXISTS (SELECT 1 FROM ProposalPricingReview WHERE ProposalPricingReviewID = @newChecklistId)
	BEGIN
		UPDATE ProposalPricingReview SET IsCurrent = 0;

		SET IDENTITY_INSERT ProposalPricingReview ON;
		INSERT INTO ProposalPricingReview (ProposalPricingReviewID, ChecklistVersion, IsCurrent, ProposalChecklistTypeID)
			VALUES (@newChecklistId, @newChecklistId - 1, 1, 1);
		SET IDENTITY_INSERT ProposalPricingReview OFF;

		INSERT INTO PPRChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalPricingReviewID)
			SELECT ChecklistText, TextTypeID, SortOrder, ColumnOrder, @newChecklistId
				FROM PPRChecklistContent
				WHERE ProposalPricingReviewID = @newChecklistId - 1;
	END
END
GO