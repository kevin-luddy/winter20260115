IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vwDfarsChecklistResponseReport]') AND type in (N'V'))
	DROP VIEW [dbo].[vwDfarsChecklistResponseReport]
GO

CREATE VIEW [dbo].[vwDfarsChecklistResponseReport] AS
/******************************************************************************
**		 
**		Name: [vwDfarsChecklistResponseReport]
**		Desc: DFARS Checklist Response Report View
**			
**		
**
**		Auth: ranzalon
**		Date: 5/14/19
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/03/19	ranzalon			BOEJ-4217 - Updates based on feedback
*******************************************************************************/
SELECT  
	P.ProposalID AS ProposalID,	
	LOB.LineOfBusinessID as LineOfBusinessID,
	LOB.LineOfBusinessName AS [Line of Business],	
	PA.ProgramAreaID as ProgramAreaID,
	PA.ProgramAreaName as [Program Area Name],
	P.ProposalTrackingID AS [Tracking #],
	P.ProposalTitle AS [Proposal Title],
	CAST(PC.ProposalSubmittalDate AS DATE) AS [Actual Submittal Date],
	PCC.[SubmitDate] AS [Approval Workflow Completed Date],	
	P.[CertificationTimelineCompleted] AS [Certification Completed Date],
	LeadEstimator.UserID AS [LeadEstimatorUserID],
	LeadEstimator.DisplayName AS [LeadEstimatorName],		
	PS.ProposalStatusID AS [ProposalStatusID],
	PS.ProposalStatus AS [Proposal Status],
	ParCC.[QuestionNumber] AS [Question #],
	ParCC.[ChecklistText] AS [Question Text],
	RLU.[ResponseID] AS [ResponseID],
	RLU.[Response] AS [Response],
	PPCX.[CannedResponseId] AS [Selection],
	PPCX.[Comment] As [Comment]

  FROM [dbo].[Proposal] P
	INNER JOIN [dbo].[ProposalChecklistComplete] PCC ON PCC.ProposalID = P.ProposalID
	INNER JOIN [dbo].[LineOfBusinessLU] LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID	
	INNER JOIN [dbo].[ProgramAreaLU] PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN dbo.ProposalStatusLU PS ON P.ProposalStatusID = PS.ProposalStatusID
	LEFT OUTER JOIN dbo.ProposalChecklist PC ON P.ProposalID = PC.ProposalID
	LEFT OUTER JOIN
	(
		SELECT
			PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
		FROM ProposalUserRole PUR
			JOIN genTRACUser U ON PUR.UserID = U.UserID
		WHERE
			PUR.RoleID = 3 --Lead Estimator Role
	) LeadEstimator ON P.ProposalID = LeadEstimator.ProposalID
	LEFT OUTER JOIN dbo.ProposalPARChecklistXREF PPCX ON PPCX.ProposalID = P.ProposalID
	LEFT OUTER JOIN dbo.PARChecklistContent ParCC ON PPCX.PARChecklistContentID = ParCC.PARChecklistContentID
	LEFT OUTER JOIN dbo.ResponseLU RLU ON RLU.ResponseID = PPCX.ResponseID
	LEFT OUTER JOIN dbo.CannedResponsesPAR CR ON CR.CannedResponseId = PPCX.CannedResponseId
	LEFT OUTER JOIN dbo.ProposalAdequacyReview PAR ON PAR.ProposalAdequacyReviewID = ParCC.ProposalAdequacyReviewID
	WHERE RLU.[ResponseID] = 2 --'No' Response
		AND PAR.ChecklistVersion >= 12 --Earliest version with Canned Responses
		AND (PS.ProposalStatusID = 2 OR PS.ProposalStatusID = 6 OR PS.ProposalStatusID = 9) --'Completed', 'Pending Certification', 'Pending Contractual Award' Status
		AND PPCX.[CannedResponseId] is NULL --Other is Selected, so response ID is null
		AND PCC.[ChecklistTypeID] = 1 --Default Checklist Type

GO

