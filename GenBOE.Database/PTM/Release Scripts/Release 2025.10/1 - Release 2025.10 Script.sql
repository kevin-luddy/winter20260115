EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.10';
GO

-- Author: RJ Anzalone (ranzalon)
-- PROPH-3306 - 11/13/2025
-- Remove Question 10 from PPR Checklist
DECLARE @newChecklistId INT = 21 -- Version 20

IF NOT EXISTS (SELECT 1 FROM ProposalAdequacyReview WHERE ProposalAdequacyReviewID = @newChecklistId)
BEGIN
	EXEC CopyPARChecklist @newChecklistId;
	EXEC CopyCannedResponsesPAR @newChecklistId;
	EXEC CopyPPRChecklist @newChecklistId;

	-- Remove Question 10 
	-- Sort Order is 3 greater than the question number (there are 3 rows prior to question 1), so use that to identify the questions
	DELETE FROM dbo.PPRChecklistContent
	WHERE ProposalPricingReviewID = @newChecklistId AND SortOrder = 13

	-- Update numbering and sort order of Questions 11-14 to be 1 less
	-- Question 11 -> Question 10
	UPDATE dbo.PPRChecklistContent
	SET ChecklistText = '<p>10. Does the proposal include subcontractors of any dollar value or material supplier > CCoPD threshold with planned dates that go beyond proposal submittal? Click <a href="https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/SiteLinks/Proposal%20Tracking%20Module%20(PTM)/Proposal%20Pricing%20Review%20Question%20%2311%20-%20Help.docx">here</a> for additional information on applicability.</p>', 
		SortOrder = 13
	WHERE ProposalPricingReviewID = @newChecklistId AND SortOrder = 14
	
	-- Question 12 -> Question 11
	UPDATE dbo.PPRChecklistContent
	SET ChecklistText = '<p>11. Has the NLF Forms tool been used to prepare the PBOE, IBOE, MPBOE, and Subcontractor Summary Table?</p>', 
		SortOrder = 14
	WHERE ProposalPricingReviewID = @newChecklistId AND SortOrder = 15
	
	-- Question 13 -> Question 12
	UPDATE dbo.PPRChecklistContent
	SET ChecklistText = '<p>12. If you are pricing effort that includes <b><u>scope through 2028</u></b>, have you verified that the proposed costs align with the Heritage Space disclosed practices, rates and factors? (NA if no scope prior to 2029)</p>', 
		SortOrder = 15
	WHERE ProposalPricingReviewID = @newChecklistId AND SortOrder = 16
		
	-- Question 14 -> Question 13
	UPDATE dbo.PPRChecklistContent
	SET ChecklistText = '<p>13. If you are pricing effort that includes <b><u>scope in 2029 and beyond</u></b>, have you verified that the proposed costs align with the 1LMX disclosed practices, rates and factors? (NA if no scope for 2029 and beyond)</p>', 
		SortOrder = 16
	WHERE ProposalPricingReviewID = @newChecklistId AND SortOrder = 17
	
	-- Update Sort Order of Pricer Comment to be one higher than the highest number question (Question 13 - Sort Order 16)
	UPDATE dbo.PPRChecklistContent
	SET SortOrder = 17
	WHERE TextTypeID = 5 and ProposalPricingReviewID = @newChecklistId;
END