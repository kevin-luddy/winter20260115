DROP VIEW [dbo].[vwProposalLogReport];
GO

CREATE VIEW [dbo].[vwProposalLogReport] AS
/******************************************************************************
**		 
**		Name: [vwProposalLogReport]
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
**		01/04/17	Dusan				Removing Revisions & LMIS
**		01/05/17	pattoncr			Removing Segment
**      01/05/17	twilson3			BOEJ-1706 Remove ICE fields
**		01/19/17	n99040				added fields TechLead, IndependentReviewer, LeadEstimator, ProposalMgr, CoverSheetApprover, PricingVErification
**		02/02/17	tglick				added new field [Revised Submittal Date]
**      02/17/17	twilson3			BOEJ-1903 Add LOB Estimating Manager
**										BOEJ-1905 Remove IS&GS from Total Price column
**		3/22/2017	twilson3			BOEJ-1909 Remove Profit Tracking #
**		10/18/2017	Dusan				BOEJ-2652 Add Proposal Class field to the report
**		3/19/2018	twilson3			BOEJ-3126 Add Forecast Tracking # to SSRS
**		4/17/2018   Dusan				BOEJ-3388 Remove 2 IWTA columns and tweak how another one works
**		5/10/2018	Dusan				BOEJ-3402 Fixed labels for Workflow Submitted Date
**		5/30/2018	ranzalon			BOEJ-3405 - Adjusted naming of Actual Submittal Date
**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
**		8/29/2018	twilson3			BOEJ-3756 Post Proposal redo
**		8/30/2018	Dusan				BOEJ-3757 SSRS Updates w/ Post Proposal Changes
**		9/27/2018	ranzalon			BOEJ-3741 Classified Cost Volume
**		8/5/2019	twilson3			BOEJ-4274 Add LOB Manager Comments
**		4/22/2020	ranzalon			BOEJ-4535 Add Lead Estimator Approval Date
*******************************************************************************/
SELECT  
	
	P.ProposalID AS ProposalID,
	
	CAST (P.DateCreated AS DATE) AS DateCreated,
	YEAR(P.DateCreated) AS [Year],
	
	PA.ProgramAreaID as ProgramAreaID,
	LOB.LineOfBusinessID as LineOfBusinessID,
	LOB.LineOfBusinessName AS [Line Of Business],
	PA.ProgramAreaName AS [Program Area Name],
	
	P.ProposalTrackingID AS [Tracking #],
	P.ForecastedTrackingId AS [ForecastedTracking#],
	P.ProposalTitle AS [Proposal Title],
	
	CAST(P.DateCreated AS DATE) AS [Proposal Start Date],
	CAST (PC.UpdateDate AS Date) AS [Proposal End Date],
	
	LeadEstimator.UserID AS [LeadEstimatorUserID],
	
	[CostVolumeLead].[Cost Volume Lead] AS [Cost Volume Lead],
	[AdditionalPricingResource1].[Additional Pricing Resource 1] AS [Additional Pricing Resource 1],
	[AdditionalPricingResource2].[Additional Pricing Resource 2] AS [Additional Pricing Resource 2],
	
	P.EstimatedProposalValue AS [Estimated Value],
	CAST(P.AnticipatedDeliveryDate AS DATE) AS [Estimated Ship Date],
	CAST(P.RevisedSubmittalDate AS DATE) AS [Revised Submittal Date],
	P.ProgramName AS [Program Name],

	PC.ISGSTotalPrice AS [Submitted Value],
	CONVERT(varchar, CONVERT(datetime2(7), P.LeadEstimatorSignedDT), 100) AS [Lead Estimator Approval Date],
	CONVERT(varchar, CONVERT(datetime2(7), [Workflow Completed Date].MaxSubmitDate), 100) AS [Workflow Completed Date],
	IndependentReviewer.DisplayName AS [IndependentReviewerName],
	PricingVerification.DisplayName AS [PricingVerificationName],
	CoverSheetApprover.DisplayName AS [CoverSheetApproverName],
	LOBMgr.DisplayName AS [LOBMgrName],
	ProposalMgr.DisplayName AS [ProposalMgrName],
	TechLead.DisplayName AS [TechLeadName],
	LeadEstimator.DisplayName AS [LeadEstimatorName],

      PC.[LMLaborHours] AS [LMLaborHours],
      PC.[LMLaborCost] AS [LMLaborCost],
      PC.[SubcontractorCost] AS [SubcontractorCost],
      PC.[MaterialCost] AS [MaterialCost],
      PC.[IWTACost] AS [IWTACost],
      PC.[TravelCost] AS [TravelCost],
      PC.[OtherDirectCost] AS [OtherDirectCost],
	  PC.[AbsoluteValue] AS [AbsoluteValue],
	PC.[ProfitFee] AS [Profit/Fee + COM],	
	PC.[ISGSTotalPrice] AS [Total Price],

	PC.[ROSPercentage] AS [ROS %],
	

	CAST(PC.ProposalSubmittalDate AS DATE) AS [Actual Submittal Date],
	PT.ProposalType AS [Proposal Type],
	
	dbo.udfCreateCommaSeparatedList (P.ProposalID, 1) AS [Contract Type],
	
	I.ISGSRole AS [Prime or Sub],
	
	P.RFPNumber AS [RFP/Contract Modification Number],

	dbo.udfCreateCommaSeparatedList (P.ProposalID, 2) AS [Elements of Cost],
		
	[IS&GS Contracts POC].[IS&GS Contracts POC] AS [Contracts POC],
	P.Customer AS [Customer],
	CusType.CustomerType AS [Customer Type],
	CreatedBy.NTID AS [Created By],
	CAST (P.DateCreated AS DATE) AS [Created Date],
	
	PS.ProposalStatusID AS [ProposalStatusID],
	PS.ProposalStatus AS [Proposal Status],
	
	P.OTISOpportunityID AS [OTIS #],
	
	CASE
		WHEN P.PricingToolID = 3 AND P.PricingToolName IS NOT NULL THEN P.PricingToolName
		ELSE T.PricingTool
	END AS [Pricing Tool],


	CASE
		WHEN P.BOEToolID = 6 AND P.BOEToolName IS NOT NULL THEN P.BOEToolName
		ELSE B.BOETool
	END AS [BOE Tool]


	       ,CAST(P.RFPIssuedDate AS DATE) AS [RFP Issued Date]
		   ,CAST(P.RFPReceivedDate AS DATE) AS [RFP Received Date]
		   ,P.[Comments]
		   
		   ,CG.ContractTypeGroupID AS [ContractTypeGroupID] 
		   ,CG.ContractTypeGroup AS [ContractTypeGroup]
		   
		    
		   ,	CASE P.IsScheduleProposal
					WHEN 1 THEN 'Yes'
					WHEN 0 THEN 'No'
					ELSE 'N/A'
				END AS [Schedule Proposal],



	LeadEstimator.[NTID] AS [PricerNTID],
	[CostVolumeLead].[Cost Volume Lead NTID] AS [Cost Volume Lead NTID],	
	[AdditionalPricingResource1].[Additional Pricing Resource 1 NTID] AS [Additional Pricing Resource 1 NTID],	
	[AdditionalPricingResource2].[Additional Pricing Resource 2 NTID] AS [Additional Pricing Resource 2 NTID],	
	[IS&GS Contracts POC].[IS&GS Contracts POC NTID] AS [Contracts POC NTID],

	CASE
		WHEN P.ProposalLocationID = 9 AND P.ProposalLocationName IS NOT NULL THEN P.ProposalLocationName
		ELSE PLoc.ProposalLocation
	END AS [Proposal Location],
	
	CASE 
		WHEN P.ProposalTrackingID IS NULL OR P.ProposalTrackingID = '' THEN P.ForecastedTrackingId
		ELSE
			CASE LEN(LEFT (IsNULL(ProposalTrackingID, ForecastedTrackingId), CHARINDEX ('-',IsNULL(ProposalTrackingID, ForecastedTrackingId)) -1))
				WHEN 1 THEN P.ForecastedTrackingId
				WHEN 4 THEN LTRIM(RTRIM(LEFT (IsNULL(P.ProposalTrackingID, P.ForecastedTrackingId), 10))) 
				WHEN 2 THEN LTRIM(RTRIM(LEFT (IsNULL(P.ProposalTrackingID, P.ForecastedTrackingId), 8)))
			END
	END	AS MainProposalTrackingID
	,CCPDRequired
	,CostVolumeClassified
  ,ppsLU.ProgramProposalStatus 		
  ,pcLU.ProposalClass AS [Proposal Class]
  ,CAST(P.[AgreementDate] AS DATE) AS [Agreement Date]
  ,CAST(P.[CertificationDate] AS DATE) AS [Certification Date]
  ,CAST(P.[CertificationTimelineCompleted] AS DATE) AS [Certification Timeline Completed Date]
  ,CAST(P.[CertificationLastEmailed] AS DATE) AS [Certification Last Emailed Date]
  ,CASE P.[CutOffDateUtilization] 
					WHEN 0 THEN 'Yes'
					WHEN 1 THEN 'No, LM did not request'
					WHEN 2 THEN 'No, LM request denied'
					ELSE ''
			END AS [CutOff Date Utilization]
  ,CASE 
	WHEN T.[PricingTool] = 'ProPricer' AND B.[BOETool] = 'genBOE' THEN ''
	ELSE P.LOBEstimatingLeadSignComment
  END AS LOBMgrComment
  FROM [dbo].[Proposal] P
	INNER JOIN [dbo].[ProgramAreaLU] PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN [dbo].[LineOfBusinessLU] LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN dbo.ProposalTypeLU PT ON P.ProposalTypeID = PT.ProposalTypeID
	
	INNER JOIN dbo.PricingToolLU T ON P.PricingToolID = T.PricingToolID
	INNER JOIN dbo.BOEToolLU B ON P.BOEToolID = B.BOEToolID
	INNER JOIN dbo.ProposalLocationLU PLoc ON P.ProposalLocationID = PLoc.ProposalLocationID
	INNER JOIN dbo.ProposalStatusLU PS ON P.ProposalStatusID = PS.ProposalStatusID
	INNER JOIN dbo.CustomerTypeLU CusType ON P.CustomerTypeID = CusType.CustomerTypeID
	INNER JOIN dbo.ISGSRoleLU I ON P.ISGSRoleID = I.ISGSRoleID
	INNER JOIN dbo.ProposalClassLU pcLU ON P.ProposalClassID = pcLU.ProposalClassId

	LEFT OUTER JOIN dbo.ProposalChecklist PC ON P.ProposalID = PC.ProposalID
	LEFT OUTER JOIN dbo.ContractTypeGroupLU CG ON P.ContractTypeGroupID = CG.ContractTypeGroupID

	LEFT OUTER JOIN dbo.genTRACUser CreatedBy ON P.CreatedByUserID = CreatedBy.UserID

	LEFT OUTER JOIN
		(
			SELECT 
				ProposalID,
				MAX(SubmitDate) AS MaxSubmitDate
			FROM dbo.ProposalChecklistComplete
			GROUP BY ProposalID
		) [Workflow Completed Date] ON P.ProposalID = [Workflow Completed Date].ProposalID

	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName AS [Additional Pricing Resource 2],
				U.UserID AS [Additional Pricing Resource 2 UserID],
				U.NTID AS [Additional Pricing Resource 2 NTID]
				
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE RoleID = 5	/*Additional Pricing Resource 2*/
		) [AdditionalPricingResource2] ON P.ProposalID = [AdditionalPricingResource2].ProposalID
	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName AS [Additional Pricing Resource 1],
				U.UserID AS [Additional Pricing Resource 1 UserID],
				U.NTID AS [Additional Pricing Resource 1 NTID]
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE RoleID = 4	/*Additional Pricing Resource 1*/
		) [AdditionalPricingResource1] ON P.ProposalID = [AdditionalPricingResource1].ProposalID

	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName AS [Cost Volume Lead],
				U.UserID AS [Cost Volume Lead UserID],
				U.NTID AS [Cost Volume Lead NTID]
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE RoleID = 2 /*Cost Volume Lead*/
		) [CostVolumeLead] ON P.ProposalID = [CostVolumeLead].ProposalID



	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName AS [IS&GS Contracts POC],
				U.UserID AS [IS&GS Contracts POC UserID],
				U.NTID AS [IS&GS Contracts POC NTID]
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE RoleID = 8 /*IS&GS Contracts POC*/
		) [IS&GS Contracts POC] ON P.ProposalID = [IS&GS Contracts POC].ProposalID


		-- Independent Reviewer
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 11  /*11  Peer Reviewer*/
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

		--LOB Estimating Manager
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 20
		) LOBMgr ON P.ProposalID = LOBMgr.ProposalID

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

	LEFT JOIN dbo.ProgramProposalStatusLU ppsLU
	ON ppsLU.ProgramProposalStatusID = p.ProgramProposalStatusID

GO