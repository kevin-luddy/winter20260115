EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.3';
GO

-- Author: Oyeyemi Oyetoro (e378233)
-- JIRA Story: PROPH-2835, PROPH-2836
-- Added Program Mgr and MSAC POC to Role table

IF NOT EXISTS (SELECT * FROM dbo.[RoleLU] WHERE RoleID IN (25))
BEGIN
	SET IDENTITY_INSERT dbo.[RoleLU] ON;

	INSERT INTO [RoleLU]
		(RoleID, [Role])
	VALUES
		(25, 'Program Mgr')
		
	SET IDENTITY_INSERT dbo.[RoleLU] OFF;
END
GO

IF NOT EXISTS (SELECT * FROM dbo.[RoleLU] WHERE RoleID IN (26))
BEGIN
	SET IDENTITY_INSERT dbo.[RoleLU] ON;

	INSERT INTO [RoleLU]
		(RoleID, [Role])
	VALUES
		(26, 'MSAC POC (Business Development)')
		
	SET IDENTITY_INSERT dbo.[RoleLU] OFF;
END
GO


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
	pCD.ProgramEppDate AS ContractsProgramEppDate,
	pCD.LobEppDate AS ContractsLobEppDate,
	pCD.PreSpaceEppDate AS ContractsPreSpaceEppDate,
	pCD.SpaceEppDate AS ContractsSpaceEppDate,
	pCD.PreCorporateEppDate AS ContractsPreCorporateEppDate,
	pCD.CorporateEppDate AS ContractsCorporateEppDate,
	pCD.EppRosDelegationNotes AS ContractsEppRosDelegationNotes,
	pCD.CustomerDueDate AS CustomerDueDate,
	CASE
		WHEN pCD.LmWon = 1 THEN 'Yes'
		WHEN pCD.LmWon = 0 THEN 'No'
		ELSE NULL
	END AS ContractsLmWon,
	pCD.ModCompletedDate AS ContractsModCompletedDate,
	pCD.BidEppDate AS ContractsBidEppDate,
	pCD.MissionSegmentEppDate AS ContractsMissionSegmentEppDate,
	-- end of Proposal Contract Data
	pCD.CageCode,
	CASE
		WHEN p.ContractActionTypeOtherText IS NULL THEN aT.ContractActionType
		ELSE aT.ContractActionType + ': ' + p.ContractActionTypeOtherText
	END AS ContractActionType,
	PC.CostThroughCom,
	ppr.Response AS NlfResponse,
	ppr11.Response AS SupplierMilestoneDatesResponse,
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
	pCD.NegotiatedInsurance
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
GO


IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateProposalLogReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreateProposalLogReport];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateProposalLogReport]
(
	@ProposalStatus varchar (8000),
	@AllProposals bit=NULL,
	@SpecificProposals bit=NULL,
	@Year varchar (8000)=NULL,
	@LOB varchar (8000)=NULL,
	@LeadEstimator varchar(8000)=NULL,
	@SubmitStartDate [date]=NULL,
	@SubmitEndDate [date]=NULL,
	@TrackingNumber varchar(10)=NULL,
	@ExecutionUserID varchar (8000)=NULL
)	
AS
/******************************************************************************
**		 
**		Name: [CreateProposalLogReport]
**		Desc: SSRS: Proposal Log Report
**
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/22/2020	ranzalon			BOEJ-4535 Add Lead Estimator Approval Date
**		6/30/2020	Dusan				BOEJ-4639 Add Revision Type
**										BOEJ-4590 Add Material POC and Subcontracts POC
**										BOEJ-4631 Add Reason Cert Not Required
**		7/30/2020	Dusan				BOEJ-4639 Add Latest Revision
**		1/16/2022   Dusan				IES-174 Add Contract Data to the report
**		3/7/2022	Dusan				IES-847 Modify Contracts Data data
**		7/25/2022	Dusan / Thomas		IES-1499, IES-1510, IES-1511: Added new fields into the report: Cage Codes, Type of Contract Action, Cost thru COM
**		7/28/2022	Dusan				IES-1578 Add [Profit/Fee] and [COM] individual fields to the report
**		10/13/2022	RJ					IES-1933 Cost Volume Tool
**		02/22/23	ranzalon			Add CustomerDueDate
**		7/10/23		Dusan				PROPH-1563: Add AdditionalClassification field
**		7/14/24		Dusan				PROPH-1559: Added reason for CCOPD = No
**		7/18/24		Dusan				PROPH-1560: Added Include International Costs
**		8/19/24		Dusan				PROPH-2080: Added IsSupportDefinitizingUCA field
**		11/21/24	twilson3			proph-2357 Add Insurance fields
**		02/27/25	Carlos				PROPH-2642 Added SupplierMilestoneDatesResponse field
**		05/13/25	ranzalon			PROPH-3038 Added Bid and Mission Segment EPP Dates
**		07/21/25	e378233				PROPH-2835/2836 Added MSAC POC and Program Manager
*******************************************************************************/

SET NOCOUNT ON

DECLARE @tblProposalStatus TABLE (ProposalStatusID int)
IF @ProposalStatus IS NULL OR @ProposalStatus = 'All'
	BEGIN
		INSERT INTO @tblProposalStatus
		SELECT ProposalStatusID FROM dbo.ProposalStatusLU
	END
ELSE	
	BEGIN
		IF RIGHT(@ProposalStatus, 1) <> ','
	      SET @ProposalStatus = @ProposalStatus + ','
	
		WHILE (SELECT CHARINDEX (',', @ProposalStatus) ) > 1
			BEGIN
			      
				  INSERT INTO @tblProposalStatus
				  SELECT LEFT (@ProposalStatus, CHARINDEX (',', @ProposalStatus) -1)
				  SET @ProposalStatus = RIGHT (@ProposalStatus, LEN (@ProposalStatus) - CHARINDEX (',', @ProposalStatus) )
			      
			END
	END

IF  @SpecificProposals = 1
BEGIN

/*Process Year*/
DECLARE @tblYear TABLE ([Year] varchar(100))
IF @Year IS NULL OR @Year = 'All'
	BEGIN
		INSERT INTO @tblYear
		SELECT DISTINCT YEAR(DateCreated) FROM dbo.Proposal
	END
ELSE	
	BEGIN
		IF RIGHT(@Year, 1) <> ','
	      SET @Year = @Year + ','
	
		WHILE (SELECT CHARINDEX (',', @Year) ) > 1
			BEGIN
			      
				  INSERT INTO @tblYear
				  SELECT LEFT (@Year, CHARINDEX (',', @Year) -1)
				  SET @Year = RIGHT (@Year, LEN (@Year) - CHARINDEX (',', @Year) )
			      
			END
	END




/*Line of Business*/
DECLARE @tblLineOfBusiness TABLE (LineOfBusinessID int)

IF @LOB IS NULL OR @LOB = 'All'
	BEGIN
		INSERT INTO @tblLineOfBusiness
		SELECT LineOfBusinessID FROM dbo.LineOfBusinessLU
	END
ELSE	
	BEGIN
		IF RIGHT(@LOB, 1) <> ','
	      SET @LOB = @LOB + ','
	
		WHILE (SELECT CHARINDEX (',', @LOB) ) > 1
			BEGIN
			      
				  INSERT INTO @tblLineOfBusiness
				  SELECT LEFT (@LOB, CHARINDEX (',', @LOB) -1)
				  SET @LOB = RIGHT (@LOB, LEN (@LOB) - CHARINDEX (',', @LOB) )
			      
			END
	END

/*Lead Estimator*/
DECLARE @tblLeadEstimator TABLE (LeadEstimatorID int)

IF @LeadEstimator IS NULL OR @LeadEstimator = 'All'
	BEGIN
		INSERT INTO @tblLeadEstimator
		SELECT  DISTINCT Pricer.UserID AS [UserID]
		FROM [dbo].[Proposal] P
			INNER JOIN 
				(
					SELECT 
						PUR.ProposalID,
						U.DisplayName AS [Pricer Name],
						U.UserID AS [UserID]
						
					FROM dbo.ProposalUserRole PUR
						INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
					WHERE	RoleID = 3 /*Lead Estimator*/
				) Pricer ON P.ProposalID = Pricer.ProposalID
	END
ELSE
BEGIN
	IF RIGHT(@LeadEstimator, 1) <> ','
		  SET @LeadEstimator = @LeadEstimator + ','

	WHILE (SELECT CHARINDEX (',', @LeadEstimator) ) > 1
	BEGIN
	      
		  INSERT INTO @tblLeadEstimator
		  SELECT LEFT (@LeadEstimator, CHARINDEX (',', @LeadEstimator) -1)
		  SET @LeadEstimator = RIGHT (@LeadEstimator, LEN (@LeadEstimator) - CHARINDEX (',', @LeadEstimator) )
	      
	END
END

END

/*Individual Running Report and the groups they belong to as @ExecutionUserID*/
DECLARE @tblExecutionUser TABLE (ExecutionUserID int)
IF @ExecutionUserID IS NULL
	RETURN 
ELSE
BEGIN
	IF RIGHT(@ExecutionUserID, 1) <> ','
		  SET @ExecutionUserID = @ExecutionUserID + ','

	WHILE (SELECT CHARINDEX (',', @ExecutionUserID) ) > 1
	BEGIN
	      
		  INSERT INTO @tblExecutionUser
		  SELECT LEFT (@ExecutionUserID, CHARINDEX (',', @ExecutionUserID) -1)
		  SET @ExecutionUserID = RIGHT (@ExecutionUserID, LEN (@ExecutionUserID) - CHARINDEX (',', @ExecutionUserID) )
	      
	END
END

DECLARE @MaxRev TABLE(MainProposalTrackingID char (10))
INSERT INTO @MaxRev
SELECT LEFT(IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), 10)
FROM dbo.Proposal
WHERE LEN(LEFT (IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), CHARINDEX ('-',IsNULL(NULLIF(ProposalTrackingID,''), '00-00000')) -1)) = 4/*To Support 4 Digit Dates*/
GROUP BY LEFT(IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), 10)
UNION
SELECT LEFT(IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), 8)
FROM dbo.Proposal
WHERE LEN(LEFT (IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), CHARINDEX ('-',IsNULL(NULLIF(ProposalTrackingID,''), '00-00000')) -1)) = 2/*To Support 2 Digit Dates*/
GROUP BY LEFT(IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), 8)

SELECT V.[ProposalID]
      ,V.[DateCreated]
      ,V.[Year]
      ,V.[ProgramAreaID]
      ,V.[LineOfBusinessID]
      ,V.[Line Of Business]
      ,V.[Program Area Name]
      ,V.[Tracking #]
      ,V.[ForecastedTracking#]
      ,V.[Proposal Title]
      ,V.[Proposal Start Date]
      ,V.[Proposal End Date]
      ,V.[LeadEstimatorUserID]
      ,V.IndependentReviewerName
	  ,V.PricingVerificationName
	  ,V.CoverSheetApproverName
	  ,V.LOBMgrName
	  ,V.ProposalMgrName
	  ,V.ProgramMgrName
	  ,V.TechLeadName
	  ,V.LeadEstimatorName
	  ,V.[MSAC POC]
      ,V.[Cost Volume Lead]
      ,V.[Additional Pricing Resource 1]
      ,V.[Additional Pricing Resource 2]
      ,V.[Estimated Value]
      ,V.[Estimated Ship Date]
      ,V.[Program Name]
      ,V.[Submitted Value]  
	  , CAST (
			CASE 
				WHEN LEN (DATEPART(MM, V.[Lead Estimator Approval Date])) = 1 
					THEN '0' + CAST (DATEPART(MM, V.[Lead Estimator Approval Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(MM, V.[Lead Estimator Approval Date]) AS CHAR(2))
			END  + '/' + 
			CASE 
				WHEN LEN (DATEPART(DD, V.[Lead Estimator Approval Date])) = 1 
					THEN '0' + CAST (DATEPART(DD, V.[Lead Estimator Approval Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(DD, V.[Lead Estimator Approval Date]) AS CHAR(2))
			END  + '/' + 
			CAST (DATEPART(YYYY, V.[Lead Estimator Approval Date]) AS CHAR(4))
      			AS varchar (10))	
    		 + ' ' +
			RIGHT (V.[Lead Estimator Approval Date], 7) 
		AS  [Lead Estimator Approval Date] 
      , CAST (
			CASE 
				WHEN LEN (DATEPART(MM, V.[Workflow Completed Date])) = 1 
					THEN '0' + CAST (DATEPART(MM, V.[Workflow Completed Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(MM, V.[Workflow Completed Date]) AS CHAR(2))
			END  + '/' + 
			CASE 
				WHEN LEN (DATEPART(DD, V.[Workflow Completed Date])) = 1 
					THEN '0' + CAST (DATEPART(DD, V.[Workflow Completed Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(DD, V.[Workflow Completed Date]) AS CHAR(2))
			END  + '/' + 
			CAST (DATEPART(YYYY, V.[Workflow Completed Date]) AS CHAR(4))
      			AS varchar (10))	
    		 + ' ' +
			RIGHT (V.[Workflow Completed Date], 7) 
		AS  [Workflow Completed Date]     
      ,V.[LMLaborHours]
      ,V.[LMLaborCost]
      ,V.[SubcontractorCost]
      ,V.[MaterialCost]
      ,V.[IWTACost]
      ,V.[TravelCost]
      ,V.[OtherDirectCost]
      ,V.[Profit/Fee + COM]
	  ,V.[Profit/Fee]
	  ,V.[COM]
      ,V.[Total Price]
      ,V.[ROS %]
      ,V.[Actual Submittal Date]
      ,V.[Proposal Type]
      ,V.[Contract Type]
      ,V.[Prime or Sub]
      ,V.[RFP/Contract Modification Number]
      ,V.[Elements of Cost]
      ,V.[Contracts POC]
      ,V.[Customer]
      ,V.[Customer Type]
      ,V.[Created By]
      ,V.[Created Date]
      ,V.[ProposalStatusID]
      ,V.[Proposal Status]
      ,V.[OTIS #]
      ,V.[Pricing Tool]
      ,V.[BOE Tool]
	  ,V.[Cost Volume Tool]
      ,V.[RFP Issued Date]
      ,V.[RFP Received Date]
      ,V.[Comments]
      ,V.[ContractTypeGroupID]
      ,V.[ContractTypeGroup]
      ,V.[Schedule Proposal]
      ,V.[Proposal Location]
      ,[CCPDRequired] = 
		CASE V.[CCPDRequired] 
			WHEN 1 THEN 'Yes'
			WHEN 0 THEN 'No'
			ELSE 'Unavailable for Record'
			END
	 ,[CostVolumeClassified] =
		CASE V.[CostVolumeClassified]
			WHEN 1 THEN 'Yes'
			WHEN 0 THEN 'No'
			ELSE 'Unavailable for Record'
			END
	 ,V.ProgramProposalStatus
	 ,V.[AbsoluteValue]
	 ,V.[Revised Submittal Date]
	 ,V.[Proposal Class]
	 ,V.[Agreement Date]
	 ,V.[Certification Date]
	 ,V.[CutOff Date Utilization]
	 ,V.[LOBMgrComment]
	 ,V.ReasonCertificationNotRequired
	 ,V.RevisionType
	 ,V.MaterialPOC
	 ,V.SubcontractsPOC
	 ,V.IsLatestVersion
	 -- Proposal Contract Data
	 ,V.ContractsPreviouslySubmittedRomTrackingNumber
	 ,V.ContractsPreviouslySubmittedRomDate
	 ,V.ContractsPreviouslySubmittedRomValue
	 ,V.ContractsCustomerSubmittalDate
	 ,V.ContractsCorrespondLogNumber
	 ,V.ContractsFinalNegotiatedValue
	 ,V.ContractsFinalNegotiatedDate
	 ,V.ContractsEppDelegationAuthority
 	 ,V.ContractsProgramEppDate
	 ,V.ContractsLobEppDate
	 ,V.ContractsPreSpaceEppDate
	 ,V.ContractsSpaceEppDate
	 ,V.ContractsPreCorporateEppDate
	 ,V.ContractsCorporateEppDate
	 ,V.ContractsEppRosDelegationNotes
	 ,V.ContractsLmWon
	 ,V.ContractsModCompletedDate
	 ,V.ContractsBidEppDate
	 ,V.ContractsMissionSegmentEppDate
	 -- end of Proposal Contract Data
	 ,V.CageCode
	 ,V.ContractActionType
	 ,V.CostThroughCom
	 ,V.CustomerDueDate
	 ,V.NlfResponse
	 ,V.SupplierMilestoneDatesResponse
	 ,AdditionalClassification =
		CASE V.[AdditionalClassification]
			WHEN 1 THEN 'Yes'
			WHEN 0 THEN 'No'
			ELSE NULL
			END
	,V.ReasonCcopdNo
	,V.ReasonCcopdNoOther
	,IncludeInternationalCosts =
		CASE V.IncludeInternationalCosts
			WHEN 1 THEN 'Yes'
			WHEN 0 THEN 'No'
			ELSE NULL
			END
	,IsSupportDefinitizingUCA =
		CASE V.IsSupportDefinitizingUCA
			WHEN 1 THEN 'Yes'
			WHEN 0 THEN 'No'
			ELSE NULL
			END
	,V.IsInsuranceDirect
	,V.InsuranceType
	,V.ProposedInsurance
	,V.NegotiatedInsurance
FROM [dbo].[vwProposalLogReport] V
	LEFT OUTER JOIN @MaxRev M ON 
		(
			LEFT(IsNULL(V.[Tracking #], '00-00000'),10) = M.MainProposalTrackingID AND
			LEN (M.MainProposalTrackingID) = 10
		) OR
		(
			LEFT(IsNULL(V.[Tracking #], '00-00000'),8) = M.MainProposalTrackingID AND
			LEN (M.MainProposalTrackingID) = 8
		)
WHERE
	/*Proposal Status is defined as mandatory*/
	(
		ProposalStatusID IN  
			(SELECT ProposalStatusID FROM @tblProposalStatus)
			
	) AND 
	
	(
		(
			@AllProposals = 1
		) OR	
		(
			@SpecificProposals = 1 AND
				(
					@Year = 'All' OR @Year IS NULL OR	[Year] IN (SELECT [Year] FROM @tblYear)
				) AND
				(
					@LOB = 'All' OR
					@LOB IS NULL OR 
					[LineOfBusinessID] IN (SELECT LineOfBusinessID FROM @tblLineOfBusiness) 
				) AND
				(
					@LeadEstimator = 'All' OR
					@LeadEstimator IS NULL OR
						(
							[LeadEstimatorUserID] IN (SELECT [LeadEstimatorID] FROM @tblLeadEstimator) 
						)
							
				)
		) OR		
		(
			@SubmitStartDate IS NOT NULL AND
			@SubmitEndDate IS NOT NULL AND
			CAST([Actual Submittal Date] AS DATE) > = @SubmitStartDate AND
			CAST([Actual Submittal Date] AS DATE) < = @SubmitEndDate
		)  OR		
		(
			(
				@TrackingNumber IS NOT NULL AND
				([Tracking #] LIKE @TrackingNumber + '%' OR [ForecastedTracking#] LIKE @TrackingNumber + '%')
			)			
		)	
	)

AND
	/*Now Define Role Access*/
	(
		/*If you are a System Admin, then you see everything*/
		EXISTS (
					SELECT S.SystemUserRoleID
					FROM dbo.SystemUserRole S
						INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
						INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
					WHERE 
						R.[Role] = 'Administrator'
				) OR
		/*System Pricer gets to see anything where they are defined as a Proposal User*/		
				(
					ProposalStatusID <> 4/*Deleted*/ AND
					EXISTS
						(
							SELECT S.SystemUserRoleID
							FROM dbo.SystemUserRole S
								INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
								INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
							WHERE 
								R.[Role] = 'System Pricer'
						) AND
					V.ProposalID IN
						(
							SELECT ProposalID
							FROM dbo.ProposalUserRole PUR
								INNER JOIN @tblExecutionUser U ON PUR.UserID = U.ExecutionUserID
						)								
							
			/*Viewers get to see where they are defined as Product Line Viewers*/							
				) OR

				(
					ProposalStatusID <> 4/*Deleted*/ AND

					EXISTS
						(
							SELECT S.SystemUserRoleID
							FROM dbo.SystemUserRole S
								INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
								INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
							WHERE 
								R.[Role] = 'Viewer'
						) AND
					LineOfBusinessID IN
						(
							SELECT LineOfBusinessID
							FROM dbo.LineOfBusinessRoleXREF X
								INNER JOIN dbo.SystemUserRole S ON X.SystemUserRoleID = S.SystemUserRoleID
								INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
						)								
							
							
				)
		
	)
GO