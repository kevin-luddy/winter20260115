EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.8';
GO

/*
                ## START ##
                4/12/24 Dusan - PROPH-1843 - Update PPR Checklist
*/

-- New Checklist version 17, ID 18
DECLARE @newChecklistId INT = 18;

IF NOT EXISTS (SELECT 1 FROM ProposalAdequacyReview WHERE ProposalAdequacyReviewID = @newChecklistId)
BEGIN
	EXEC CopyPARChecklist @newChecklistId;
	EXEC CopyCannedResponsesPAR @newChecklistId;
	EXEC CopyPPRChecklist @newChecklistId;

	-- Question 11
	UPDATE [dbo].[PPRChecklistContent] SET SortOrder = 16 WHERE ProposalPricingReviewID = @newChecklistId AND SortOrder = 15
	INSERT INTO [dbo].[PPRChecklistContent] (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalPricingReviewID) 
		VALUES ('<p>12. Has the NLF Forms tool been used to prepare the PBOE, IBOE, MPBOE, and Subcontractor Summary Table?</p>', 4, 15, 1, @newChecklistId);

END

/*
                4/12/24 Dusan - PROPH-1843 - Update PPR Checklist
                ## END ##
*/
