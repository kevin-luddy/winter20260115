IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[genBOE].[genTracData]') AND type in (N'V'))
	DROP VIEW [genBOE].[genTracData]
GO

CREATE VIEW [genBOE].[genTracData] AS 
/******************************************************************************
**		 
**		Name: [genBOE].[genTracData]
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
**		01/19/17	n99040				added alias LeadEstimatorName
**      2/15/17		twilson3			Fixed Independent Reviewer filter to use 'Peer Reviewer' as the text
**		2/15/17		Dusan				Removed RoleType
**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
**		11/19/19	twilson3			BOEJ-4312 Do not return Deleted Proposals
**		10/29/21	Dusan				IES-460: Use Revised Anticipated Delivery Date when Available
*******************************************************************************/
SELECT DISTINCT
	P.ProposalID AS [genTracProposalID],
	P.ProposalTrackingID AS [TrackingNumber],
	P.ProposalTitle AS [ProposalTitle],
	LOB.LineOfBusinessName AS [LineOfBusinessName],
	PA.ProgramAreaName AS [ProgramAreaName],
	PT.ProposalType AS [ProposalType],		
	PS.ProposalStatus AS [ProposalStatus],	
	
	P.EstimatedProposalValue AS [EstimatedValue],
	PC.ISGSTotalPrice AS [SubmittedValue],
	

	--per BOEJ-1546, replaced start/end dates with RFPReceivedDate & AnticpatedDeliveryDate

	CAST(P.RFPReceivedDate AS DATE) AS [ProposalStartDate],
	CASE WHEN p.RevisedSubmittalDate IS NOT NULL THEN CAST(p.RevisedSubmittalDate AS DATE) ELSE CAST(p.AnticipatedDeliveryDate AS DATE) END AS [ProposalEndDate],
	CAST (P.DateCreated AS DATE) AS [CreatedDate],
	CAST(PC.ProposalSubmittalDate AS DATE) AS [SubmittalDate],
	
	P.ProgramName AS [ProgramName],
		
	LEADESTIMATOR.DisplayName AS [LeadEstimator],		
	LEADESTIMATOR.DisplayName AS [LeadEstimatorName], -- report dataset field name consistency
	AER1.AdditionalEstimatingResource1 AS [AdditionalEstimatingResource1],
	AER2.AdditionalEstimatingResource2 AS [AdditionalEstimatingResource2],
	
	P.Customer AS [Customer],
	CT.CustomerType AS [CustomerType],
	
	CL.ContractsLead AS [ContractsLead],
		
	CVL.DisplayName AS [CostVolumeLead],
	
	Mngr.Manager AS Manager,
	
	PrcT.PricingTool AS [PricingTool],
	
	BT.BOETool AS [BOETool],
	
	dbo.udfCreateCommaSeparatedList (P.ProposalID, 1) AS [ContractType],
	dbo.udfCreateCommaSeparatedList (P.ProposalID, 2) AS [ElementsOfCost],
	
	P.IsIWTA AS [IWTA],
	
	CASE I.ISGSRole
		WHEN 'IWTA' THEN ''
		ELSE I.ISGSRole
	END AS [PrimeOrSub],
		
	P.RFPNumber AS [RFPNumber],
	PROPOSALMANAGER.DisplayName AS [ProposalManager],
	COVERSHEET.DisplayName AS [CoverSheetApprover],
	PRICINGVERIFIER.DisplayName AS [PricingVerifier],
	INDP_REVIEWER.DisplayName AS [IndependentReviewer],
	MATERIALLEAD.DisplayName AS [MaterialLead],
	SUBCONTRACTLEAD.DisplayName AS [SubcontractLead]
	
FROM [dbo].[Proposal] P
	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.RoleID = 3 /*LEADESTIMATOR*/  
		) LEADESTIMATOR ON P.ProposalID = LEADESTIMATOR.ProposalID

	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Proposal Mgr'
		) PROPOSALMANAGER ON P.ProposalID = PROPOSALMANAGER.ProposalID

	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Material Lead'
		) MATERIALLEAD ON P.ProposalID = MATERIALLEAD.ProposalID

	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Subcontract Lead'
		) SUBCONTRACTLEAD ON P.ProposalID = SUBCONTRACTLEAD.ProposalID

	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Cover Sheet Approver'
		) COVERSHEET ON P.ProposalID = COVERSHEET.ProposalID
		
	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName				
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Pricing Verification'
		) PRICINGVERIFIER ON P.ProposalID = PRICINGVERIFIER.ProposalID
	
	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Peer Reviewer' --Independent Reviewer name in RoleLU table
		) INDP_REVIEWER ON P.ProposalID = INDP_REVIEWER.ProposalID	

	INNER JOIN [dbo].[ProgramAreaLU] PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN [dbo].[LineOfBusinessLU] LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN [dbo].[ProposalTypeLU] PT ON P.ProposalTypeID = PT.ProposalTypeID

	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.RoleID = 2 /*Cost Volume Lead*/
		) CVL ON P.ProposalID = CVL.ProposalID
		
	LEFT OUTER JOIN [dbo].[ProposalChecklist] PC ON P.ProposalID = PC.ProposalID
	INNER JOIN [dbo].[BOEToolLU] BT ON BT.BOEToolID = P.BOEToolID
	INNER JOIN [dbo].[PricingToolLU] PrcT ON PrcT.PricingToolID = P.PricingToolID
	INNER JOIN [dbo].[ProposalStatusLU] PS ON PS.ProposalStatusID = P.ProposalStatusID
	INNER JOIN [dbo].[CustomerTypeLU] CT ON CT.CustomerTypeID = P.CustomerTypeID
	INNER JOIN [dbo].[ISGSRoleLU] I ON I.ISGSRoleID = P.ISGSRoleID
	
	LEFT OUTER JOIN
	(
		SELECT 
			ProposalID,
			MAX(SubmitDate) AS MaxSubmitDate
		FROM dbo.ProposalChecklistComplete
		GROUP BY ProposalID
	) PCE ON P.ProposalID = PCE.ProposalID
	
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS ContractsLead
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 8 /*Contracts Lead*/
	) CL ON P.ProposalID = CL.ProposalID
	
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS AdditionalEstimatingResource1
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 4	/*Additional Estimating Resource 1*/
	) AER1 ON P.ProposalID = AER1.ProposalID	
	
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS AdditionalEstimatingResource2
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 5	/*Additional Estimating Resource 2*/
	) AER2 ON P.ProposalID = AER2.ProposalID

	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS Manager			
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 1 /*Capture Manager*/
	) Mngr ON P.ProposalID = Mngr.ProposalID
WHERE PS.ProposalStatus != 'Deleted'

GO