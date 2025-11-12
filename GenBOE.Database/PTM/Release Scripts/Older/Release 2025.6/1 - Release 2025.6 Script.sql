EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.6';
GO

-- Author: RJ Anzalone (ranzalon)
-- PROPH-3306 - 10/13/2025
-- Add Questions 13 and 14 to PPR Checklist
DECLARE @newChecklistId INT = 20 -- Version 19

IF NOT EXISTS (SELECT 1 FROM ProposalAdequacyReview WHERE ProposalAdequacyReviewID = @newChecklistId)
BEGIN
	EXEC CopyPARChecklist @newChecklistId;
	EXEC CopyCannedResponsesPAR @newChecklistId;
	EXEC CopyPPRChecklist @newChecklistId;

	DECLARE @sortOrderIndex INT;
	
	-- Get the Sort Order of the last question
	SELECT @sortOrderIndex = MAX(SortOrder)
	FROM dbo.PPRChecklistContent
	WHERE ProposalPricingReviewID = @newChecklistId AND TextTypeID = 4;

	-- Question 13
	INSERT INTO dbo.PPRChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalPricingReviewID)
	VALUES ('<p>13. If you are pricing effort that includes <b><u>scope through 2028</u></b>, have you verified that the proposed costs align with the Heritage Space disclosed practices, rates and factors? (NA if no scope prior to 2029)</p>', 4, @sortOrderIndex + 1, 1, @newChecklistId);
	
	-- Question 14
	INSERT INTO dbo.PPRChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalPricingReviewID)
	VALUES ('<p>14. If you are pricing effort that includes <b><u>scope in 2029 and beyond</u></b>, have you verified that the proposed costs align with the 1LMX disclosed practices, rates and factors? (NA if no scope for 2029 and beyond)</p>', 4, @sortOrderIndex + 2, 1, @newChecklistId);

	-- Update Sort Order of Pricer Comment
	UPDATE dbo.PPRChecklistContent
	SET SortOrder = @sortOrderIndex + 3
	WHERE TextTypeID = 5 and ProposalPricingReviewID = @newChecklistId;
END