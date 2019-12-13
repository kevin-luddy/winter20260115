/*
* Author: Jacob John
* Query to swap the Iscurrent flag for the latest checklist version and version 1. 
* Used for the Manual Checklist Version Test in US#16855
*/

USE [genTRAC]
GO

DECLARE @Version As INT
DECLARE @MaxChecklistVersion As INT

SET @Version = (SELECT [ChecklistVersion] FROM [dbo].[ProposalPricingReview] WHERE [IsCurrent] = 1)
SET @MaxChecklistVersion = (SELECT MAX([ChecklistVersion]) FROM [dbo].[ProposalPricingReview])

PRINT 'The current GenTRAC Proposal Checklist Version is ' + CAST(@Version AS VARCHAR(2))

IF (SELECT [IsCurrent] FROM [dbo].[ProposalAdequacyReview] WHERE [ChecklistVersion] = 1 AND [ProposalAdequacyReviewID] = 1) = 0
BEGIN	
	UPDATE [dbo].[ProposalAdequacyReview]
	   SET [IsCurrent] = 1
	 WHERE [ProposalAdequacyReviewID] = 1 AND [ChecklistVersion] = 1

	UPDATE [dbo].[ProposalAdequacyReview]
	   SET [IsCurrent] = 0
	 WHERE [ProposalAdequacyReviewID] = @MaxChecklistVersion AND [ChecklistVersion] = @MaxChecklistVersion
	 /* PRINT 'DEBUG: In PAR1' */
END
ELSE
BEGIN
	UPDATE [dbo].[ProposalAdequacyReview]
	   SET [IsCurrent] = 0
	 WHERE [ProposalAdequacyReviewID] = 1 AND [ChecklistVersion] = 1

	UPDATE [dbo].[ProposalAdequacyReview]
	   SET [IsCurrent] = 1
	 WHERE [ProposalAdequacyReviewID] = @MaxChecklistVersion AND [ChecklistVersion] = @MaxChecklistVersion
	 /* PRINT 'DEBUG: In PAR2' */
END

IF (SELECT [IsCurrent] FROM [dbo].[ProposalPricingReview] WHERE [ChecklistVersion] = 1 AND [ProposalPricingReviewID] = 1) = 0
BEGIN
	UPDATE [dbo].[ProposalPricingReview]
	   SET [IsCurrent] = 1
	 WHERE [ProposalPricingReviewID] = 1 AND [ChecklistVersion] = 1

	UPDATE [dbo].[ProposalPricingReview]
	   SET [IsCurrent] = 0
	 WHERE [ProposalPricingReviewID] = @MaxChecklistVersion AND [ChecklistVersion] = @MaxChecklistVersion
	 /* PRINT 'DEBUG: In PPR1' */
END
ELSE
BEGIN
	UPDATE [dbo].[ProposalPricingReview]
	   SET [IsCurrent] = 0
	 WHERE [ProposalPricingReviewID] = 1 AND [ChecklistVersion] = 1

	UPDATE [dbo].[ProposalPricingReview]
	   SET [IsCurrent] = 1
	 WHERE [ProposalPricingReviewID] = @MaxChecklistVersion AND [ChecklistVersion] = @MaxChecklistVersion
	 /* PRINT 'DEBUG: In PPR2' */
END

SET @Version = (SELECT [ChecklistVersion] FROM [dbo].[ProposalPricingReview] WHERE [IsCurrent] = 1)
PRINT CHAR(10) + 'The new GenTRAC Proposal Checklist Version is ' + CAST(@Version AS VARCHAR(2))

GO


