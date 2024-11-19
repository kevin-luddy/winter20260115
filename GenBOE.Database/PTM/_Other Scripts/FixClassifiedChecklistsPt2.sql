/*   
	proph-2302 twilson3 PTM Cowan issues

	Classified install is missing checklist 15, so when checklist 16 was created there was nothing to copy from
	1.  Insert the missing checklist 15
	2.  Re-run checklist 16 creation scripts
	3.  Add checklists to Proposals missing them

*/

-- 2.  Rerun checklist 16


-- New Checklist version 16, ID 17 (only need to copy the content)
DECLARE @newChecklistId INT = 17;
-- set latest checklist to be current
UPDATE ProposalAdequacyReview SET IsCurrent = 0;
UPDATE ProposalPricingReview SET IsCurrent = 0;

INSERT INTO PARChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalAdequacyReviewID, QuestionNumber, Reference, SubmissionItem, YesOnly)
			SELECT ChecklistText, TextTypeID, SortOrder, ColumnOrder, @newChecklistId, QuestionNumber, Reference, SubmissionItem, YesOnly
				FROM PARChecklistContent
				WHERE ProposalAdequacyReviewId = @newChecklistId - 1;

EXEC CopyCannedResponsesPAR @newChecklistId;

INSERT INTO PPRChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalPricingReviewID)
			SELECT ChecklistText, TextTypeID, SortOrder, ColumnOrder, @newChecklistId
				FROM PPRChecklistContent
				WHERE ProposalPricingReviewID = @newChecklistId - 1;

UPDATE ProposalAdequacyReview SET IsCurrent = 1 WHERE ChecklistVersion = @newChecklistId - 1;
UPDATE ProposalPricingReview SET IsCurrent = 1 WHERE ChecklistVersion = @newChecklistId - 1;

-- Question 11
UPDATE [dbo].[PPRChecklistContent] SET SortOrder = 15 WHERE ProposalPricingReviewID = @newChecklistId AND SortOrder = 14
INSERT INTO [dbo].[PPRChecklistContent] (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalPricingReviewID) VALUES ('<p>11. Does the proposal include subcontractors of any dollar value or material supplier > CCoPD threshold with planned dates that go beyond proposal submittal? Click <a href="https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/SiteLinks/Proposal%20Tracking%20Module%20(PTM)/Proposal%20Pricing%20Review%20Question%20%2311%20-%20Help.docx">here</a> for additional information on applicability.</p>', 4, 14, 1, @newChecklistId);

GO
