EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2021.6';
GO

/*
	## START ##

	4/13/2021 [Dusan] - BOEJ-5211: Canned Responses not a part of the new revision;
*/
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CopyCannedResponsesPAR]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CopyCannedResponsesPAR];
GO

CREATE PROCEDURE dbo.CopyCannedResponsesPAR
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

EXEC dbo.CopyCannedResponsesPAR 14;
GO
/*
	4/13/2021 [Dusan] - BOEJ-5211: Canned Responses not a part of the new revision;

	## END ##
*/
