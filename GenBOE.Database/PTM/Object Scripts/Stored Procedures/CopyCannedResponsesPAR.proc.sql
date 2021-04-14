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