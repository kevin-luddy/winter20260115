EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.5';
GO

/*
                ## START ##

                2/19/24 [Scott] - PROPH-1558 - Update PPR Checklist
*/

-- Create the new SPs here because release scripts run before SP scripts
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

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CopyCannedResponsesPAR]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CopyCannedResponsesPAR];
GO

CREATE PROCEDURE dbo.CopyCannedResponsesPAR
/******************************************************************************
**		 
**		Name: CopyCannedResponsesPAR
**		Desc: Copies Canned Responses into a new Checklist version. This is executed manually.
**
**		Auth: Dusan
**		Date: 4/13/21
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
(@newChecklistId AS INT) AS
BEGIN
	IF NOT EXISTS (SELECT 1 FROM dbo.CannedResponsesPAR cR INNER JOIN PARChecklistContent pCC ON pCC.PARChecklistContentID = cR.QuestionId WHERE pCC.ProposalAdequacyReviewID = @newChecklistId)
	BEGIN
		DECLARE @oldChecklistId INT = @newChecklistId - 1;

		INSERT INTO dbo.CannedResponsesPAR
		SELECT cR.Text, new.PARChecklistContentID
			FROM dbo.CannedResponsesPAR cR 
				INNER JOIN PARChecklistContent old ON old.PARChecklistContentID = cR.QuestionId AND old.ProposalAdequacyReviewID = @oldChecklistId
				INNER JOIN PARChecklistContent new ON old.QuestionNumber = new.QuestionNumber AND new.ProposalAdequacyReviewID = @newChecklistId;

	END
END
GO

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
