IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vwProposalActivityReport]') AND type in (N'V'))
	DROP VIEW [dbo].[vwProposalActivityReport]
GO

/****** Object:  View [dbo].[vwProposalActivityReport]    Script Date: 09/25/2013 07:38:34 ******/
CREATE VIEW [dbo].[vwProposalActivityReport] AS
/******************************************************************************
**		 
**		Name: [vwProposalActivityReport]
**		Desc: 
**			
**		
**
**		Auth: Unknown
**		Date: Unknown
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		01/19/17	n99040				added fields TechLead, IndependentReviewer, LeadEstimator, ProposalMgr, CoverSheetApprover, PricingVErification
**		02/02/17	tglick				added new field [Revised Submittal Date]
**		2/15/17		Dusan				Removed RoleType, Added Absolute Value
**		3/22/2017	twilson3			BOEJ-1909 Remove Profit Tracking #
**		3/19/2018	twilson3			BOEJ-3126 Add Forecast Tracking # to SSRS
**		5/30/2018	ranzalon			BOEJ-3405 - Adjusted naming of Actual Submittal Date
**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
*******************************************************************************/
SELECT  
	P.ProposalID AS [ProposalID],

	Pricer.[Pricer Name] AS [Pricer],
	Pricer.PricerNTID AS [PricerNTID],
	Pricer.PricerUserID AS [PricerUserID],

	IndependentReviewer.DisplayName AS [IndependentReviewerName],
	PricingVerification.DisplayName AS [PricingVerificationName],
	CoverSheetApprover.DisplayName AS [CoverSheetApproverName],
	ProposalMgr.DisplayName AS [ProposalMgrName],
	TechLead.DisplayName AS [TechLeadName],
	LeadEstimator.DisplayName AS [LeadEstimatorName],

	P.ProposalTrackingID AS [Tracking #],
	P.ForecastedTrackingId AS [ForecastedTracking#],
	P.ProposalTitle AS [Proposal Title],
	P.DateCreated AS DateCreated,
	
	PA.ProgramAreaID as ProgramAreaID,
	LOB.LineOfBusinessID as LineOfBusinessID,
	LOB.LineOfBusinessName as [Line Of Business],
	PA.ProgramAreaName as [Program Area Name],

	P.Customer AS [Customer],
	P.AnticipatedDeliveryDate AS [Estimated Ship Date],
--	'$' + REPLACE(CONVERT(varchar,CAST(P.EstimatedProposalValue AS MONEY),1), '.00','') AS [Estimated Value],
	P.EstimatedProposalValue AS [Estimated Value],
	P.RevisedSubmittalDate AS [Revised Submittal Date],
	P.DateAssigned AS [Date Assigned],
	CASE 
		WHEN P.ProposalStatusID = 2/*Completed*/
		THEN 
			--ChecklistCompleteDate.MaxUpdateDate_ChecklistComplete
			CONVERT(varchar, CONVERT(datetime2(7), ChecklistCompleteDate.MaxUpdateDate_ChecklistComplete), 100) 
		ELSE NULL--''
	END AS [Checklist Complete Date],

	PC.ProposalSubmittalDate AS [Actual Submittal Date],	

	PC.ISGSTotalPrice	 AS [Total Price],
/*
if IWTA is Yes, then use Submitted value (which is always IS&GS Total Price).  If Iwta is no, then blank 
*/
	CASE P.IsIWTA
		WHEN 1 THEN PC.ISGSTotalPrice
		WHEN 0 THEN NULL--''
		ELSE NULL--''
	END AS [IWTA Submitted Value],
	
	PC.AbsoluteValue,
	PS.ProposalStatusID AS [ProposalStatusID],
	PS.ProposalStatus AS [Proposal Status],
	
	CusType.CustomerTypeID AS CustomerTypeID,
	CusType.CustomerType  AS CustomerType

  FROM [dbo].[Proposal] P
	INNER JOIN [dbo].[ProgramAreaLU] PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN [dbo].[LineOfBusinessLU] LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN dbo.ProposalStatusLU PS ON P.ProposalStatusID = PS.ProposalStatusID
	INNER JOIN dbo.CustomerTypeLU CusType ON P.CustomerTypeID = CusType.CustomerTypeID
	LEFT OUTER JOIN dbo.ProposalChecklist PC ON P.ProposalID = PC.ProposalID
LEFT OUTER JOIN 
	(
		SELECT /*Should be Submit Date not Update Date*/
			MAX ([SubmitDate]) AS MaxUpdateDate_ChecklistComplete
			,[ProposalID]
		FROM [dbo].[ProposalChecklistComplete]
		GROUP BY ProposalID
	) [ChecklistCompleteDate] ON P.ProposalID = [ChecklistCompleteDate].ProposalID
	
	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName AS [Pricer Name],
				U.UserID AS [PricerUserID],
				U.NTID AS [PricerNTID]
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE RoleID = 3 /*Pricer*/
		) Pricer ON P.ProposalID = Pricer.ProposalID


		-- Independent Reviewer
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 11
		) IndependentReviewer ON P.ProposalID = IndependentReviewer.ProposalID

		--Tech Lead
	 	LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 16
		) TechLead ON P.ProposalID = TechLead.ProposalID

		--Proposal Mgr
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 17
		) ProposalMgr ON P.ProposalID = ProposalMgr.ProposalID

		--Cover Sheet Approver
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 18
		) CoverSheetApprover ON P.ProposalID = CoverSheetApprover.ProposalID

		--Pricing Verification
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 19
		) PricingVerification ON P.ProposalID = PricingVerification.ProposalID

		--Lead Estimator
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 3
		) LeadEstimator ON P.ProposalID = LeadEstimator.ProposalID

GO