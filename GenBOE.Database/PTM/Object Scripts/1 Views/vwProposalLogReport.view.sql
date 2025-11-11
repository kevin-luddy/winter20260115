IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vwProposalLogReport]') AND type in (N'V'))
	DROP VIEW [dbo].[vwProposalLogReport]
GO

CREATE VIEW [dbo].[vwProposalLogReport] AS
/******************************************************************************
**		 
**		Name: [vwProposalLogReport]
**		Desc: View that drives the Proposal Log Report.  This report is also used in Tableau
**			
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		6/30/2020	Dusan				BOEJ-4639 Add Revision Type
**										BOEJ-4590 Add Material POC and Subcontracts POC
**										BOEJ-4631 Add Reason Cert Not Required
**		7/30/2020	Dusan				BOEJ-4639 Add Latest Revision
**      10/13/2021  Koovackal           IES-181 DB Work -Added Profit, Com, ProfitFeeWithCon
**		1/16/2022   Dusan				IES-174 Add Contract Data to the report
**		3/7/2022	Dusan				IES-847 Modify Contracts Data data
**      3/8/2022	Koovackal			IES-849 Changes to "Revise" button. 
**										Removed contract offer code.
**		7/25/2022	Dusan / Thomas		IES-1499, IES-1510, IES-1511: Added new fields into the report: Cage Codes, Type of Contract Action, Cost thru COM
**		2/22/2023	ranzalon			Add CustomerDueDate
**		7/10/24		Dusan				PROPH-1563: Add AdditionalClassification field
**		7/14/24		Dusan				PROPH-1559: Added reason for CCOPD = No
**		7/18/24		Dusan				PROPH-1560: Added Include International Costs
**		8/19/24		Dusan				PROPH-2080: Added IsSupportDefinitizingUCA field
**      11/20/24	twilson3			proph-2357 Add Insurance fields
**		12/3/24		twilson3			proph-2544 Add SetupComments, Backup Estimator Name
**		02/27/25	Carlos				PROPH-2642 Add checklist question 11 to log report
**		05/13/25	ranzalon			PROPH-3038 Added Bid and Mission Segment EPP Dates
**		07/21/25	e378233				PROPH-2835/2836 Added MSAC POC and Program Manager
**		10/02/25	e403038				PROPH-2994 Additional EPP Dates (Original to Planned and add Scheduled)
**		10/13/25	ranzalon			PROPH-3375 PPR Questions 13 and 14
**		10/30/25	e403038				PROPH-3406 Label Modifications => Current Planned renamed to ScheduledActual and Current Scheduled renamed to Planned
**		11/4/25		ranzalon			PROPH-3420: Added Alternative Pricing Methodology
*******************************************************************************/
SELECT	
	P.ProposalID AS ProposalID,	
	p.AdditionalClassification,
	cNo.Text AS ReasonCcopdNo,
	P.ReasonCcopdNoOther,
	P.SetupComments,
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
	ProgramMgr.DisplayName AS [ProgramMgrName],
	MsacPoc.DisplayName AS [MSAC POC],
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
	PC.[Profit] AS [Profit/Fee],
	PC.[Com] AS [Com],
	PC.[ProfitFeeWithCom] AS [Profit/Fee + COM],
	PC.[ISGSTotalPrice] AS [Total Price],
	PC.[ROSPercentage] AS [ROS %],
	PC.IncludeInternationalCosts,
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
	CASE
		WHEN PS.ProposalStatusID = 10 THEN 'Not Awarded'
		ELSE PS.ProposalStatus
	END AS [Proposal Status],
	P.OTISOpportunityID AS [OTIS #],
	CASE
		WHEN P.PricingToolID = 3 AND P.PricingToolName IS NOT NULL THEN P.PricingToolName
		ELSE T.PricingTool
	END AS [Pricing Tool],
	CASE
		WHEN P.BOEToolID = 6 AND P.BOEToolName IS NOT NULL THEN P.BOEToolName
		ELSE B.BOETool
	END AS [BOE Tool],
	CASE
		WHEN P.CostVolumeToolID = 3 AND P.CostVolumeToolName IS NOT NULL THEN P.CostVolumeToolName
		ELSE CV.CostVolumeTool
	END AS [Cost Volume Tool],
	CAST(P.RFPIssuedDate AS DATE) AS [RFP Issued Date],
	CAST(P.RFPReceivedDate AS DATE) AS [RFP Received Date],
	P.[Comments],
	CG.ContractTypeGroupID AS [ContractTypeGroupID],
	CG.ContractTypeGroup AS [ContractTypeGroup],
	CASE P.IsScheduleProposal
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
			CASE LEN(LEFT (IsNULL(P.ProposalTrackingID, P.ForecastedTrackingId), CHARINDEX ('-',IsNULL(P.ProposalTrackingID, P.ForecastedTrackingId)) -1))
				WHEN 1 THEN P.ForecastedTrackingId
				WHEN 4 THEN LTRIM(RTRIM(LEFT (IsNULL(P.ProposalTrackingID, P.ForecastedTrackingId), 10))) 
				WHEN 2 THEN LTRIM(RTRIM(LEFT (IsNULL(P.ProposalTrackingID, P.ForecastedTrackingId), 8)))
			END
	END	AS MainProposalTrackingID,
	P.CCPDRequired,
	P.CostVolumeClassified,
	ppsLU.ProgramProposalStatus,
	pcLU.ProposalClass AS [Proposal Class],
	CAST(P.[AgreementDate] AS DATE) AS [Agreement Date],
	CAST(P.[CertificationDate] AS DATE) AS [Certification Date],
	CAST(P.[CertificationTimelineCompleted] AS DATE) AS [Certification Timeline Completed Date],
	CAST(P.[CertificationLastEmailed] AS DATE) AS [Certification Last Emailed Date],
	CASE P.[CutOffDateUtilization] 
		WHEN 0 THEN 'Yes'
		WHEN 1 THEN 'No, LM did not request'
		WHEN 2 THEN 'No, LM request denied'
		ELSE ''
	END AS [CutOff Date Utilization],
	CASE 
		WHEN T.[PricingTool] = 'ProPricer' AND B.[BOETool] = 'genBOE' THEN ''
		ELSE P.LOBEstimatingLeadSignComment
	END AS LOBMgrComment,
	CASE p.ReasonCertificationNotRequired
		WHEN 3 THEN p.OtherReasonComment -- Other
		ELSE rCNR.Text
	END AS ReasonCertificationNotRequired,
	CASE 
		WHEN p.RevisionOfId IS NULL THEN 'Original'
		ELSE 'Proposal Revision'
	END AS RevisionType,
	MaterialPOC.DisplayName AS MaterialPOC,
	SubcontractsPOC.DisplayName AS SubcontractsPOC,
	BackupEstimatorPOC.DisplayName AS BackupEstimatorPOC,
	CASE
		WHEN p.ProposalStatusID = 8 THEN 'No'
		ELSE 'Yes'
	END AS IsLatestVersion,
	-- Proposal Contract Data
	previousRomProposal.ProposalTrackingID AS ContractsPreviouslySubmittedRomTrackingNumber,
	previousRomChecklist.ProposalSubmittalDate AS ContractsPreviouslySubmittedRomDate,
	previousRomChecklist.ISGSTotalPrice AS ContractsPreviouslySubmittedRomValue,
	pCD.CustomerSubmittalDate AS ContractsCustomerSubmittalDate,
	pCD.ContractsCorrespondLogNumber AS ContractsCorrespondLogNumber,
	pCD.FinalNegotiatedValue AS ContractsFinalNegotiatedValue,
	pCD.FinalNegotiatedDate AS ContractsFinalNegotiatedDate,
	eppLU.Text AS ContractsEppDelegationAuthority,
	pCD.ScheduledActualProgramEppDate AS ContractsScheduledActualProgramEppDate,
	pCD.ScheduledActualLobEppDate AS ContractsScheduledActualLobEppDate,
	pCD.ScheduledActualPreSpaceEppDate AS ContractsScheduledActualPreSpaceEppDate,
	pCD.ScheduledActualSpaceEppDate AS ContractsScheduledActualSpaceEppDate,
	pCD.ScheduledActualPreCorporateEppDate AS ContractsScheduledActualPreCorporateEppDate,
	pCD.ScheduledActualCorporateEppDate AS ContractsScheduledActualCorporateEppDate,
	pCD.EppRosDelegationNotes AS ContractsEppRosDelegationNotes,
	pCD.CustomerDueDate AS CustomerDueDate,
	CASE
		WHEN pCD.LmWon = 1 THEN 'Yes'
		WHEN pCD.LmWon = 0 THEN 'No'
		ELSE NULL
	END AS ContractsLmWon,
	pCD.ModCompletedDate AS ContractsModCompletedDate,
	pCD.ScheduledActualBidEppDate AS ContractsScheduledActualBidEppDate,
	pCD.ScheduledActualMissionSegmentEppDate AS ContractsScheduledActualMissionSegmentEppDate,
	pCD.PlannedProgramEppDate AS ContractsPlannedProgramEppDate,
	pCD.PlannedLobEppDate AS ContractsPlannedLobEppDate,
	pCD.PlannedPreSpaceEppDate AS ContractsPlannedPreSpaceEppDate,
	pCD.PlannedSpaceEppDate AS ContractsPlannedSpaceEppDate,
	pCD.PlannedPreCorporateEppDate AS ContractsPlannedPreCorporateEppDate,
	pCD.PlannedCorporateEppDate AS ContractsPlannedCorporateEppDate,
	pCD.PlannedBidEppDate AS ContractsPlannedBidEppDate,
	pCD.PlannedMissionSegmentEppDate AS ContractsPlannedMissionSegmentEppDate,
	-- end of Proposal Contract Data
	pCD.CageCode,
	CASE
		WHEN p.ContractActionTypeOtherText IS NULL THEN aT.ContractActionType
		ELSE aT.ContractActionType + ': ' + p.ContractActionTypeOtherText
	END AS ContractActionType,
	PC.CostThroughCom,
	ppr.Response AS NlfResponse,
	ppr11.Response AS SupplierMilestoneDatesResponse,
	ppr13.Response AS ScopeVerifiedThru2028Response,
	ppr14.Response AS ScopeVerified2029BeyondResponse,
	P.IsSupportDefinitizingUCA,
	CASE
		WHEN pCD.IsInsuranceDirect = 1 THEN 'Yes'
		WHEN pCD.IsInsuranceDirect = 0 THEN 'N/A'
		WHEN pCD.IsInsuranceDirect = 2 THEN 'No'
		ELSE 'N/A'
	END AS IsInsuranceDirect,
	CASE
		WHEN pCD.IsInsuranceDirect = 1 THEN IT.[Text]
		WHEN pCD.IsInsuranceDirect = 0 THEN 'N/A'
		WHEN pCD.IsInsuranceDirect = 2 THEN 'N/A'
		ELSE 'N/A'
	END AS InsuranceType,
	pCD.ProposedInsurance,
	pCD.NegotiatedInsurance,
	P.SubjectToAlternativePricingMethodology,
	CASE
		WHEN P.AlternativePricingMethodology = null THEN ''
		WHEN P.AlternativePricingMethodology = 4 THEN P.AlternativePricingMethodologyOtherText
		ELSE apm.Text
	END AS AlternativePricingMethodology
  FROM [dbo].[Proposal] P
    INNER JOIN [dbo].[ProgramAreaLU] PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN [dbo].[LineOfBusinessLU] LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN dbo.ProposalTypeLU PT ON P.ProposalTypeID = PT.ProposalTypeID
	INNER JOIN dbo.PricingToolLU T ON P.PricingToolID = T.PricingToolID
	INNER JOIN dbo.BOEToolLU B ON P.BOEToolID = B.BOEToolID
	INNER JOIN dbo.CostVolumeToolLU CV on P.CostVolumeToolID = CV.CostVolumeToolID
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
		SELECT ProposalID, MAX(SubmitDate) AS MaxSubmitDate
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
		FROM dbo.ProposalUserRole PUR INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 5
		) [AdditionalPricingResource2] ON P.ProposalID = [AdditionalPricingResource2].ProposalID
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS [Additional Pricing Resource 1],
			U.UserID AS [Additional Pricing Resource 1 UserID],
			U.NTID AS [Additional Pricing Resource 1 NTID]
		FROM dbo.ProposalUserRole PUR INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 4	/*Additional Pricing Resource 1*/
	) [AdditionalPricingResource1] ON P.ProposalID = [AdditionalPricingResource1].ProposalID
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS [Cost Volume Lead],
			U.UserID AS [Cost Volume Lead UserID],
			U.NTID AS [Cost Volume Lead NTID]
		FROM dbo.ProposalUserRole PUR INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 2
	) [CostVolumeLead] ON P.ProposalID = [CostVolumeLead].ProposalID
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS [IS&GS Contracts POC],
			U.UserID AS [IS&GS Contracts POC UserID],
			U.NTID AS [IS&GS Contracts POC NTID]
		FROM dbo.ProposalUserRole PUR INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 8
	) [IS&GS Contracts POC] ON P.ProposalID = [IS&GS Contracts POC].ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
		FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
		WHERE PUR.RoleID = 11
	) IndependentReviewer ON P.ProposalID = IndependentReviewer.ProposalID
 	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 16
	) TechLead ON P.ProposalID = TechLead.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 25
	) ProgramMgr ON P.ProposalID = ProgramMgr.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 26
	) MsacPoc ON P.ProposalID = MsacPoc.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 17
	) ProposalMgr ON P.ProposalID = ProposalMgr.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 18
	) CoverSheetApprover ON P.ProposalID = CoverSheetApprover.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 19
	) PricingVerification ON P.ProposalID = PricingVerification.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 20
	) LOBMgr ON P.ProposalID = LOBMgr.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 3
	) LeadEstimator ON P.ProposalID = LeadEstimator.ProposalID
	LEFT JOIN dbo.ProgramProposalStatusLU ppsLU ON ppsLU.ProgramProposalStatusID = p.ProgramProposalStatusID
	LEFT JOIN dbo.ReasonCertificationNotRequiredLU rCNR ON rCNR.Id = p.ReasonCertificationNotRequired
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 6
	) MaterialPOC ON P.ProposalID = MaterialPOC.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID, U.DisplayName
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 7 
	) SubcontractsPOC ON P.ProposalID = SubcontractsPOC.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID, U.DisplayName
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 12 
	) BackupEstimatorPOC ON P.ProposalID = BackupEstimatorPOC.ProposalID

	-- Proposal Contract Data
	LEFT OUTER JOIN ProposalContractsData pCD ON pCD.ProposalID = p.ProposalID
	LEFT OUTER JOIN dbo.InsuranceTypeLU IT ON pCD.InsuranceType = IT.ID
	LEFT OUTER JOIN Proposal previousRomProposal ON pCD.PreviouslySubmittedROM = previousRomProposal.ProposalID
	LEFT OUTER JOIN ProposalChecklist previousRomChecklist ON pCD.PreviouslySubmittedROM = previousRomChecklist.ProposalID
	LEFT OUTER JOIN EppDelegationAuthorityLU eppLU ON eppLU.Id = pCD.EppDelegationAuthority
	-- end of Proposal Contract Data

	LEFT OUTER JOIN ContractActionTypeLU aT ON p.ContractActionType = aT.ID
	LEFT OUTER JOIN
			(SELECT xref.ProposalID, ppr.SortOrder, ppr.ChecklistText, r.Response
				FROM ProposalPPRChecklistXREF xref 
					INNER JOIN ResponseLU r ON r.ResponseID = xref.ResponseID
					INNER JOIN PPRChecklistContent ppr ON (xref.PPRChecklistContentID = ppr.PPRChecklistContentID AND ppr.ChecklistText LIKE '%NLF Forms%')) AS ppr
			ON ppr.ProposalId = p.ProposalId 
	LEFT OUTER JOIN
			(SELECT xref.ProposalID, ppr.SortOrder, ppr.ChecklistText, r.Response
				FROM ProposalPPRChecklistXREF xref 
					INNER JOIN ResponseLU r ON r.ResponseID = xref.ResponseID
					INNER JOIN PPRChecklistContent ppr ON (xref.PPRChecklistContentID = ppr.PPRChecklistContentID AND ppr.ChecklistText LIKE '%Does the proposal include subcontractors of any dollar value or material supplier > CCoPD threshold with planned dates that go beyond proposal submittal%')) AS ppr11
			ON ppr11.ProposalId = p.ProposalId 
	LEFT OUTER JOIN [CcopdReasonsNo] cNo ON P.ReasonCcopdNo = cNO.Id
	LEFT OUTER JOIN
			(SELECT xref.ProposalID, ppr.SortOrder, ppr.ChecklistText, r.Response
				FROM ProposalPPRChecklistXREF xref 
					INNER JOIN ResponseLU r ON r.ResponseID = xref.ResponseID
					INNER JOIN PPRChecklistContent ppr ON (xref.PPRChecklistContentID = ppr.PPRChecklistContentID AND ppr.ChecklistText LIKE '%scope through 2028%')) AS ppr13
			ON ppr13.ProposalId = p.ProposalId 
	LEFT OUTER JOIN
			(SELECT xref.ProposalID, ppr.SortOrder, ppr.ChecklistText, r.Response
				FROM ProposalPPRChecklistXREF xref 
					INNER JOIN ResponseLU r ON r.ResponseID = xref.ResponseID
					INNER JOIN PPRChecklistContent ppr ON (xref.PPRChecklistContentID = ppr.PPRChecklistContentID AND ppr.ChecklistText LIKE '%scope in 2029 and beyond%')) AS ppr14
			ON ppr14.ProposalId = p.ProposalId
	LEFT OUTER JOIN [AlternativePricingMethodology] apm on P.AlternativePricingMethodology = apm.Id
GO
