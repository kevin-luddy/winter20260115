EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2021.5';
GO

/*
	## START ##

	3/24/2021 [RJ] - BOEJ-5051: Update PTM PAR Checklist Question 16 General Instructions
*/

-- New Checklist version 13, ID 14
DECLARE @newChecklistId INT = 14;

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

	UPDATE PARChecklistContent
	SET ChecklistText = '<p>Subcontractor Proposals (If S/C proposal >= $15M or if S/C proposal > CCoPD threshold and 10% of the Prime Proposal price) Must be included with proposal or include statement how the subcontracts are submitted. </p><p><a href=''__BASE_URL__Instruction_16.docx'' target=''_blank''>Additional Instructions</a></p>'
	WHERE SortOrder = 25 and ColumnOrder = 2 and ProposalAdequacyReviewID = @newChecklistId; --Question 16 General Instructions

	EXEC CopyCannedResponsesPAR @newChecklistId;
END

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


GO

/*
	3/24/2021 [RJ] - BOEJ-5051: Update PTM PAR Checklist Question 16 General Instructions

	## END ##
*/