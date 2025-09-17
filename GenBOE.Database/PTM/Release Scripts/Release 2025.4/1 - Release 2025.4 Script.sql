EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.4';
GO

-- Author: RJ Anzalone (ranzalon)
-- PROPH-3306 - 9/17/2025
-- Update to Question 16 of DFARS Checklist (Effective October 1, 2025)
DECLARE @newChecklistId INT = 19 -- Version 18

IF NOT EXISTS (SELECT 1 FROM ProposalAdequacyReview WHERE ProposalAdequacyReviewID = @newChecklistId)
BEGIN
	EXEC CopyPARChecklist @newChecklistId;
	EXEC CopyCannedResponsesPAR @newChecklistId;
	EXEC CopyPPRChecklist @newChecklistId;

	DECLARE @q16SortOrder INT;
	DECLARE @q16InstructionsId INT;

	SELECT @q16SortOrder = SortOrder 
	FROM dbo.PARChecklistContent 
	WHERE QuestionNumber = '16' AND ColumnOrder = 1 AND ProposalAdequacyReviewID = @newChecklistId;

	SELECT @q16InstructionsId = PARChecklistContentID 
	FROM dbo.PARChecklistContent 
	WHERE SortOrder = @q16SortOrder AND ColumnOrder = 2 AND ProposalAdequacyReviewID = @newChecklistId;

	UPDATE [dbo].[PARChecklistContent] 
	SET ChecklistText = '<p>Subcontractor Proposals (If S/C proposal >= $20M or if S/C proposal > CCoPD threshold and 10% of the Prime Proposal price) Must be included with proposal or include statement how the subcontracts are submitted.</p><br /><p>Note: This includes material suppliers as well; NA for actuals that have previously been definitized.</p><p><a href=''__BASE_URL__Instruction_16.docx'' target=''_blank''>Additional Instructions</a></p>' 
	WHERE PARChecklistContentId = @q16InstructionsId;
END