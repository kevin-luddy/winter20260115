EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.5';
GO

/*
                ## START ##

                2/19/24 [Scott] - PROPH-1558 - Update PPR Checklist
*/

-- New Checklist version 16, ID 17
DECLARE @newChecklistId INT = 17;

IF NOT EXISTS (SELECT 1 FROM ProposalAdequacyReview WHERE ProposalAdequacyReviewID = @newChecklistId)
BEGIN
	EXEC CopyPARChecklist @newChecklistId;
	EXEC CopyCannedResponsesPAR @newChecklistId;
	EXEC CopyPPRChecklist @newChecklistId;

	-- Question 11
	UPDATE [dbo].[PPRChecklistContent] SET SortOrder = 15 WHERE ProposalPricingReviewID = @newChecklistId AND SortOrder = 14
	INSERT INTO [dbo].[PPRChecklistContent] (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalPricingReviewID) VALUES ('<p>11. Does the proposal include subcontractors of any dollar value or material supplier > CCoPD threshold with planned dates that go beyond proposal submittal? Click <a href="https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/SiteLinks/Proposal%20Tracking%20Module%20(PTM)/Proposal%20Pricing%20Review%20Question%20%2311%20-%20Help.docx">here</a> for additional information on applicability.</p>', 4, 14, 1, @newChecklistId);

END

/*
                ## END ##

                2/19/24 [Scott] - PROPH-1558 - Update PPR Checklist
*/
