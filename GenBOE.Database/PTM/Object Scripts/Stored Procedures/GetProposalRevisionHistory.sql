IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetProposalRevisionHistory]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[GetProposalRevisionHistory];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetProposalRevisionHistory] (@ProposalId INT)
AS
/******************************************************************************
**		 
**		Name: [GetProposalRevisionHistory]
**		Desc: Gets a revision history for a proposal. Any proposal id in the 
**				history tree will result in the entire history to be returned 
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		7/13/2020	Dusan				BOEJ-4694: Initial SP creation
*******************************************************************************/
	SET NOCOUNT ON;

	WITH ProposalRevisions AS
		(
		SELECT ProposalId, RevisionOfId, ProposalTrackingId, ProposalId AS ParentId
			FROM Proposal 
			WHERE RevisionOfId IS NULL AND ForecastedTrackingId IS NULL
		UNION ALL
		SELECT P.ProposalId, P.RevisionOfId, P.ProposalTrackingId, r.ParentId AS ParentId 
			FROM Proposal AS P
				INNER JOIN ProposalRevisions r ON p.RevisionOfId = r.ProposalID
			WHERE P.RevisionOfId IS NOT NULL AND ForecastedTrackingId IS NULL
		)
	SELECT ProposalId, RevisionOfId, ProposalTrackingId
		FROM ProposalRevisions
		WHERE ParentId = (SELECT ParentId FROM ProposalRevisions WHERE ProposalId = @proposalId);

GO
