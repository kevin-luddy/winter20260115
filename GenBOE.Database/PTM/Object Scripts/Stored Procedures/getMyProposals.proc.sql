IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getMyProposals]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getMyProposals];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getMyProposals]
(
	@ProposalStatusID int = NULL,
	@AssignedStart datetime2(7) = NULL,
	@AssignedEnd datetime2(7) = NULL,
	@Search varchar (250) = NULL,
	@NTID varchar (1000) = NULL,
	@ShowProposalsForMyOrganization bit = NULL,
	@UserAndGroupIDs XML = NULL, -- XML list of User and Group IDs (Only used when ShowProposalsForMyOrganization = 1)
	@ProposalClassFilterID int = NULL
)
AS
/******************************************************************************
**          
**          Name: [getMyProposals]
**          Desc: Mimics My Proposal Page
**                
**          
**
**          Auth: Don Canuso
**          Date: 4/11/14
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                -------------------------------
**          1/5/2017	twilson3				BOEJ-1693 Show proposals for LOB Viewer permissions for Users (does not include AD Groups)
**			01/19/2017	n99040					added fields TechLead, IndependentReviewer, LeadEstimator, ProposalMgr, CoverSheetApprover, PricingVErification
**			01/25/2017	tglick					BOEJ-1766 Lead Estimator is not populating correctly
**			02/02/2017	tglick					added new fields [Revised Submittal Date], [AbsoluteValue]
**			3/7/2017	twilson3				BOEJ-1942, BOEJ-1943 Fix permissions for who can see proposals on homepage
**			11/29/17	pattoncr				Remove domain.
**			1/24/2017	twilson3				BOEJ-2808 Add RDSB link into PTM
**			2/06/2018	brunworg				BOEJ-3049 - Add ShowProposalsForMyOrganization capability.
**			3/06/2018	brunworg				BOEJ-3124 Add Forecasted Tracking Number into PTM
**			3/9/2018	twilson3				BOEJ-3210 Retrieving Proposal Class Text to see if this is Forecast Proposal
**			3/13/2018	pattoncr				BOEJ-3122 - UI Home Page - Filter & Icons (Filter for Forecasted/NonForecasted)
**			3/14/2018	pattoncr				BOEJ-3122 - UI Home Page - Filter & Icons (Filter for Forecasted/NonForecasted) - update to make Proposal Class filter more efficient.
**			5/23/2018	ranzalon				BOEJ-3516 - Add CustomerTypeId to result
**			6/06/2018	brunworg				BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
**			8/31/18		twilson3				BOEJ-3761: New Submitted Proposal Status
**			6/5/20		Dusan					BOEJ-4657: Adding Forecasted proposals into field for search; cleaned up some formatting on text
******************************************************************************/
	SET NOCOUNT ON 

	IF @Search IS NOT NULL SET @Search = '%' + @Search + '%'
	DECLARE @genTracUserID INT = NULL;
	IF @genTracUserID IS NULL AND @NTID IS NOT NULL
	BEGIN
		SELECT @genTracUserID = UserID 
			FROM dbo.genTRACUser 
			WHERE NTID = @NTID
	END
	/*We want this employee and everyone under this employee*/
	DECLARE @EmployeeLMPeopleID varchar (6)

	SELECT @EmployeeLMPeopleID = empl_ser_no FROM DataMart.DataMartEmployee WHERE nt_account_nm = @NTID;
	WITH MyCTE
		AS ( SELECT empl_ser_no AS LMPeopleID, empl_first_nm as  FirstName, empl_last_nm as  LastName, rpt_to_ser_no as  FunctionalManagerLMPeopleID, nt_account_nm as  NTID
				FROM DataMart.DataMartEmployee
				WHERE empl_ser_no = @EmployeeLMPeopleID
			UNION ALL
			SELECT empl_ser_no AS LMPeopleID, empl_first_nm as  FirstName, empl_last_nm as  LastName, rpt_to_ser_no as  FunctionalManagerLMPeopleID, nt_account_nm as  NTID
				FROM DataMart.DataMartEmployee E
					INNER JOIN MyCTE  ON E.rpt_to_ser_no = MyCTE.LMPeopleID
				WHERE E.rpt_to_ser_no IS NOT NULL 
		  )

	SELECT DISTINCT
		P.ProposalID AS ProposalID,
		P.ProposalTrackingID AS [Tracking Number],
		P.ProposalTitle AS [Proposal Title],
		PA.ProgramAreaName AS [Program Area],
		P.Customer AS [Customer],
		P.CustomerTypeId As [CustomerTypeId],
		P.EstimatedProposalValue AS [Estimated Value],
		CaptureManager.DisplayName AS [Capture Manager],
		LeadEstimator.DisplayName AS [Pricer Name],
		PeerReviewer.DisplayName AS [Peer Reviewer],
		CostVolumeLead.[Cost Volume Lead] AS [Cost Volume Lead],
		IndependentReviewer.UserID AS IndependentReviewerUserId,
		IndependentReviewer.NTID AS IndependentReviewerNtId,
		IndependentReviewer.DisplayName AS IndependentReviewerName,
		PricingVerification.UserID AS PricingVerificationUserId,
		PricingVerification.NTID AS PricingVerificationNtId,
		PricingVerification.DisplayName AS PricingVerificationName,
		CoverSheetApprover.UserID AS CoverSheetApproverUserId,
		CoverSheetApprover.NTID AS CoverSheetApproverNtId,
		CoverSheetApprover.DisplayName AS CoverSheetApproverName,
		ProposalMgr.UserID AS ProposalMgrUserId,
		ProposalMgr.NTID AS ProposalMgrNtId,
		ProposalMgr.DisplayName AS ProposalMgrName,
		TechLead.UserID AS TechLeadUserId,
		TechLead.NTID  AS TechLeadNtId,
		TechLead.DisplayName AS TechLeadName,
		LeadEstimator.UserID AS LeadEstimatorUserId,
		LeadEstimator.NTID  AS LeadEstimatorNtId,
		LeadEstimator.DisplayName AS LeadEstimatorName,
		P.DateAssigned AS [Date Assigned],
		P.AnticipatedDeliveryDate AS [Estimated Ship Date (Due Date)],
		CAST(CC.ChecklistCompleteDate AS DATE) AS [ChecklistCompleteDate],
		CAST(PC.ProposalSubmittalDate AS DATE) AS [Proposal Submit Date],
		S.ProposalStatus AS [Proposal Status],
		P.RevisedSubmittalDate AS [Revised Submittal Date],
		P.DocumentId AS DocumentId,
		P.ForecastedTrackingID AS [Forecasted Tracking Number],
		PCLU.ProposalClass AS ProposalClass
	FROM dbo.Proposal P 
		INNER JOIN dbo.ProgramAreaLU PA ON P.ProgramAreaID = PA.ProgramAreaID
		INNER JOIN dbo.ProposalStatusLU S ON P.ProposalStatusID = S.ProposalStatusID
		INNER JOIN dbo.ProposalClassLU PCLU ON P.ProposalClassID = PCLU.ProposalClassID
		INNER JOIN dbo.ProposalUserRole PUR ON P.ProposalID = PUR.ProposalID
		LEFT OUTER JOIN (SELECT MAX(submitDate) AS ChecklistCompleteDate, ProposalID FROM ProposalChecklistComplete GROUP BY ProposalID) CC ON P.ProposalID = CC.ProposalID
		LEFT OUTER JOIN dbo.ProposalChecklist PC ON P.ProposalID = PC.ProposalID
		LEFT OUTER JOIN
			(SELECT PUR.ProposalID,U.DisplayName
				FROM dbo.ProposalUserRole PUR INNER JOIN dbo.RoleLU R ON PUR.RoleID = R.RoleID INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
				WHERE PUR.RoleID = 11 /*11  Peer Reviewer*/
			) PeerReviewer ON P.ProposalID = PeerReviewer.ProposalID
		LEFT OUTER JOIN
			(SELECT PUR.ProposalID,U.DisplayName
				FROM dbo.ProposalUserRole PUR INNER JOIN dbo.RoleLU R ON PUR.RoleID = R.RoleID INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
				WHERE PUR.RoleID = 1 /*1	Capture Manager*/
			) CaptureManager ON P.ProposalID = CaptureManager.ProposalID
		LEFT OUTER JOIN 
			(SELECT PUR.ProposalID, U.DisplayName AS [Cost Volume Lead], U.UserID AS [Cost Volume Lead UserID], U.NTID AS [Cost Volume Lead NTID]
				FROM dbo.ProposalUserRole PUR INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
				WHERE RoleID = 2 /*Cost Volume Lead*/
			) [CostVolumeLead] ON P.ProposalID = [CostVolumeLead].ProposalID
		LEFT OUTER JOIN
			(SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
				FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
				WHERE PUR.RoleID = 11
			) IndependentReviewer ON P.ProposalID = IndependentReviewer.ProposalID
 		LEFT OUTER JOIN
			(SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
				FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
				WHERE PUR.RoleID = 16
			) TechLead ON P.ProposalID = TechLead.ProposalID
		LEFT OUTER JOIN
			(SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
				FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
				WHERE PUR.RoleID = 17
			) ProposalMgr ON P.ProposalID = ProposalMgr.ProposalID
		LEFT OUTER JOIN
			(SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
				FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
				WHERE PUR.RoleID = 18
			) CoverSheetApprover ON P.ProposalID = CoverSheetApprover.ProposalID
		LEFT OUTER JOIN 
			(SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
				FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
				WHERE PUR.RoleID = 19
			) PricingVerification ON P.ProposalID = PricingVerification.ProposalID		
		LEFT JOIN 
			(SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
				FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
				WHERE PUR.RoleID = 3
			) LeadEstimator ON P.ProposalID = LeadEstimator.ProposalID
		LEFT OUTER JOIN
			(SELECT ProposalID, MAX(SubmitDate) AS MaxSubmitDate
				FROM dbo.ProposalChecklistComplete
				GROUP BY ProposalID
			) [ProposalReviewCompleteDate] ON P.ProposalID = [ProposalReviewCompleteDate].ProposalID
	WHERE ( 
		(
			(@ProposalStatusID IS NULL AND P.ProposalStatusID IN (1/*In Progress*/,2/*Completed*/,6/*Submitted*/)) 
			OR (@ProposalStatusID IS NOT NULL AND P.ProposalStatusID = @ProposalStatusID)
		) AND (
			(
				(@ProposalStatusID = 1/*In Progress*/ OR @ProposalStatusID = 6/*Submitted*/) 
				AND (@AssignedStart IS NULL OR CAST (P.DateAssigned AS Date) > = @AssignedStart)
				AND (@AssignedEnd IS NULL OR CAST (P.DateAssigned AS Date) < = @AssignedEnd) 
			) OR (
				P.ProposalStatusID = 2/*Completed*/ 
				AND (@AssignedStart IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) > = @AssignedStart) 
				AND (@AssignedEnd IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) < = @AssignedEnd) 
			) OR (
				@ProposalStatusID IS NULL 
				AND (
					(@AssignedStart IS NULL OR CAST (P.DateAssigned AS Date) > = @AssignedStart) 
					AND (@AssignedEnd IS NULL OR CAST (P.DateAssigned AS Date) < = @AssignedEnd) 
				) OR (
					(@AssignedStart IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) > = @AssignedStart) 
					AND (@AssignedEnd IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) < = @AssignedEnd) 
				) 
			)
		) AND (
			(isnull(@ProposalClassFilterID,0) < 1) /* All */
			OR (isnull(@ProposalClassFilterID,0) = 1 AND PCLU.ProposalClass = 'Forecasted') /* Forecasted */ 
			OR (isnull(@ProposalClassFilterID,0) = 2 AND PCLU.ProposalClass != 'Forecasted') /* Non-Forecasted */ 
		)
	) AND (/*Permissions*/
		(PUR.UserID = @genTracUserID)
		OR (
			PUR.RoleID IN (3 /*Pricer*/, 2 /*Cost Volume Lead*/, 18 /*Cover Sheet Approver*/,19 /*Pricing Verification*/,11 /*Independent Reviewer*/,20 /*LOB Estimating Lead/Mgr*/,4 /*Additional Pricing Resource 1*/,5 /*Additional Pricing Resource 2*/,12 /*Backup Lead Estimator*/) AND
			PUR.UserID IN (SELECT UserID FROM dbo.genTRACUser U INNER JOIN MyCTE C ON U.NTID = C.NTID)
		) OR (	
			@ShowProposalsForMyOrganization = 1 AND
			P.LineOfBusinessID in (SELECT XREF.LineOfBusinessID
										FROM dbo.LineOfBusinessRoleXREF XREF
											JOIN dbo.SystemUserRole SUR on SUR.SystemUserRoleID = XREF.SystemUserRoleID
											JOIN dbo.genTRACUser u on SUR.UserID = u.UserID
											JOIN (SELECT Node.Id.value('text()[1]', 'varchar(1000)') AS NTID FROM @UserAndGroupIDs.nodes('ROOT/id') AS Node(Id)) as UserAndGroupIDs on UserAndGroupIDs.NTID = u.NTID
										WHERE SUR.RoleID = 13 /*Viewer*/)
		)
	) AND (/*Search Capability*/
		@Search IS NULL
		OR (
			P.ProposalTrackingID LIKE @Search OR
			P.ForecastedTrackingId LIKE @Search OR
			P.ProposalTitle  LIKE @Search OR
			PA.ProgramAreaName  LIKE @Search OR
			P.Customer  LIKE @Search OR
			CAST (P.EstimatedProposalValue AS varchar (MAX))  LIKE @Search OR
			CaptureManager.DisplayName  LIKE @Search OR
			LeadEstimator.DisplayName  LIKE @Search OR
			CostVolumeLead.[Cost Volume Lead]  LIKE @Search OR
			CAST (P.DateAssigned AS varchar (100))  LIKE @Search OR
			CAST (P.AnticipatedDeliveryDate AS varchar (100)) LIKE @Search OR
			CAST (PC.ProposalSubmittalDate AS varchar (100))  LIKE @Search OR
			S.ProposalStatus LIKE @Search 
		)
	)
GO

GRANT EXECUTE ON OBJECT::dbo.getMyProposals TO generationReporter;
GO