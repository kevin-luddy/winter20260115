IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreatePARChecklistReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreatePARChecklistReport];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreatePARChecklistReport]
(
	@ProposalID INT,
	@ProposalAdequacyReviewID INT
)	
AS
/******************************************************************************
**		 
**		Name: [CreatePARChecklistReport]
**		Desc: SSRS: Create PAR Checklist Report
**			
**		
**
**		Auth: Don Canuso
**		Date: 12/3/2014
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/22/19		ranzalon			BOEJ-4157 - Update for Canned Responses
*******************************************************************************/
SET NOCOUNT ON

SELECT DISTINCT 
	C.QuestionNumber,
	C.Reference,
	C.SubmissionItem,
	X.PageNumber,
	COALESCE(X.Comment, R.Text) as Comment,
	C.SortOrder,
	C.TextTypeID
FROM 
(
	SELECT 
		PARChecklistContentID,
		ProposalAdequacyReviewID,
		QuestionNumber,
		Reference,
		SubmissionItem,
		SortOrder,
		TextTypeID
		
	FROM [dbo].[PARChecklistContent]
	WHERE 
		ProposalAdequacyReviewID = @ProposalAdequacyReviewID AND
		ColumnOrder = 1 AND
		(
			QuestionNumber IS NOT NULL OR
			Reference IS NOT NULL OR
			SubmissionItem IS NOT NULL
		)
) C
	LEFT OUTER JOIN [dbo].[ProposalPARChecklistXREF] X ON 
			C.PARChecklistContentID = X.PARChecklistContentID
			AND X.ProposalID = @ProposalID
			AND X.ResponseTypeID = 1
	LEFT OUTER JOIN [dbo].[CannedResponsesPAR] R ON
			R.CannedResponseId = X.CannedResponseId
ORDER BY C.SortOrder

GO

GRANT EXECUTE ON OBJECT::dbo.CreatePARChecklistReport TO generationReporter;
GO