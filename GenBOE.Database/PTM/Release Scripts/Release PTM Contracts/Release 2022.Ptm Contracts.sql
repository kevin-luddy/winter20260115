EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2022.PtmContracts';
GO

/*
	## START ##

	10/27/2021 [Koovackal] - IES-442-DB-Work Part 1 and IES-181-DB-Work Part 2.
	                         Update Proposal Checklist table and implement Contracts tab table.
*/

IF COL_LENGTH ('dbo.ProposalChecklist', 'ProfitFee') IS NOT NULL
BEGIN
	EXEC sp_rename 'dbo.ProposalChecklist.ProfitFee', 'ProfitFeeWithCom', 'COLUMN';
END

IF COL_LENGTH ('dbo.ProposalChecklist', 'Profit') IS NULL
BEGIN
	ALTER TABLE dbo.ProposalChecklist ADD Profit BIGINT NULL;
END

IF COL_LENGTH ('dbo.ProposalChecklist', 'Com') IS NULL
BEGIN
	ALTER TABLE dbo.ProposalChecklist ADD Com BIGINT NULL;
END
GO

IF OBJECT_ID('dbo.EppDelegationAuthorityLU', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.EppDelegationAuthorityLU (
		Id		INT				PRIMARY KEY		IDENTITY(1,1),
		Text	VARCHAR(50)		NULL
	); 

	SET IDENTITY_INSERT dbo.EppDelegationAuthorityLU ON;
	INSERT INTO dbo.EppDelegationAuthorityLU (Id, Text)
		VALUES ('1', 'Program'), ('2', 'LOB'), ('3', 'Space'), ('4', 'Corporate');
	SET IDENTITY_INSERT dbo.EppDelegationAuthorityLU OFF;
END
GO

IF OBJECT_ID('dbo.ProposalContractsData', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.ProposalContractsData (
		ProposalContractsDataId			INT				PRIMARY KEY		IDENTITY(1,1),
		UpdateDT						DATETIME2(7)	NOT NULL,
		ProposalID						INT				NOT NULL		REFERENCES Proposal(ProposalID),
		PreviouslySubmittedROM			INT				NOT NULL		REFERENCES Proposal(ProposalId),
		CustomerSubmittalDate			DATE			NULL,
		ContractsCorrespondLogNumber	VARCHAR(20)		NOT NULL,
		FinalNegotiatedValue			BIGINT			NULL,
		FinalNegotiatedDate				DATE			NULL,
		EppDelegationAuthority			INT				NULL			FOREIGN KEY REFERENCES dbo.EppDelegationAuthorityLU(Id),
		ProgramEppDate					DATE			NULL,
		LobEppDate						DATE			NULL,
		PreSpaceEppDate					DATE			NULL,
		SpaceEppDate					DATE			NULL,
		PreCorporateEppDate				DATE			NULL,
		CorporateEppDate				DATE			NULL,
		EppRosDelegationNotes			VARCHAR(1000)	NULL,
		LmWon							BIT				NULL,
		ModCompletedDate				DATE			NULL
	); 
END
GO

/*
	10/27/2021 [Koovackal] - IES-442-DB-Work Part 1 and IES-181-DB-Work Part 2.
	                         Update Proposal Checklist table and implement Contracts tab table.

	## END ##
*/


/*
	## START ##

	1/17/2022 [Dusan] - IES-180: Create a new revision, related to PTM Contracts data
*/

-- New Checklist version 14, ID 15
DECLARE @newChecklistId INT = 15;

IF NOT EXISTS (SELECT 1 FROM ProposalAdequacyReview WHERE ProposalAdequacyReviewID = @newChecklistId)
BEGIN
	UPDATE ProposalAdequacyReview SET IsCurrent = 0;

    SET IDENTITY_INSERT ProposalAdequacyReview ON;
	INSERT INTO ProposalAdequacyReview (ProposalAdequacyReviewID, ChecklistVersion, IsCurrent, ProposalChecklistTypeID)
		VALUES (@newChecklistId, @newChecklistId - 1, 1, 1);
    SET IDENTITY_INSERT ProposalAdequacyReview OFF;

	INSERT INTO PARChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalAdequacyReviewID, QuestionNumber, Reference, SubmissionItem, YesOnly)
		SELECT ChecklistText, TextTypeID, SortOrder, ColumnOrder, @newChecklistId, QuestionNumber, Reference, SubmissionItem, YesOnly
			FROM PARChecklistContent
			WHERE ProposalAdequacyReviewId = @newChecklistId - 1;

	EXEC CopyCannedResponsesPAR @newChecklistId;
END

IF NOT EXISTS (SELECT 1 FROM ProposalPricingReview WHERE ProposalPricingReviewID = @newChecklistId)
BEGIN
	UPDATE ProposalPricingReview SET IsCurrent = 0;

    SET IDENTITY_INSERT ProposalPricingReview ON;
	INSERT INTO ProposalPricingReview (ProposalPricingReviewID, ChecklistVersion, IsCurrent, ProposalChecklistTypeID)
		VALUES (@newChecklistId, @newChecklistId - 1, 1, 1);
    SET IDENTITY_INSERT ProposalPricingReview OFF;

	INSERT INTO PPRChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalPricingReviewID)
		SELECT ChecklistText, TextTypeID, SortOrder, ColumnOrder, @newChecklistId
			FROM PPRChecklistContent
			WHERE ProposalPricingReviewID = @newChecklistId - 1;

	UPDATE PPRChecklistContent
		SET SortOrder = 14 
		WHERE SortOrder = 13 AND ProposalPricingReviewID = @newChecklistId;

	INSERT INTO PPRChecklistContent(ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalPricingReviewID)
		VALUES ('<p>10. Are closeout Costs Included in Price?</p>', 4, 13, 1, @newChecklistId);
END
GO

/*
	1/17/2022 [Dusan] - IES-180: Create a new revision, related to PTM Contracts data

	## END ##
*/


/*
	02/23/2022 [Koovackal] - IES-845 Rename "Submitted" Proposal status.

	## START ##
*/

IF EXISTS (SELECT ProposalStatus FROM dbo.ProposalStatusLU WHERE ProposalStatus = 'Submitted')
BEGIN
	UPDATE dbo.ProposalStatusLU
	SET
		ProposalStatus = 'Pending Certification'
	WHERE
		ProposalStatus = 'Submitted';
END
GO

/*
	02/23/2022 [Koovackal] - IES-845 Rename "Submitted" Proposal status.

	## END ##
*/


/*
	02/24/2022 [Koovackal] - IES-846 Create 2 new statuses

	## START ##
*/

SET IDENTITY_INSERT dbo.ProposalStatusLU ON;
IF NOT EXISTS (SELECT ProposalStatus FROM dbo.ProposalStatusLU WHERE ProposalStatusID = '9' AND ProposalStatus = 'Pending Contractual Award')
BEGIN
	INSERT INTO dbo.ProposalStatusLU (ProposalStatusID, ProposalStatus)
	VALUES ('9', 'Pending Contractual Award');
END
GO

IF NOT EXISTS (SELECT ProposalStatus FROM dbo.ProposalStatusLU WHERE ProposalStatusID = '10' AND ProposalStatus = 'Lost')
BEGIN
	INSERT INTO dbo.ProposalStatusLU (ProposalStatusID, ProposalStatus)
	VALUES ('10', 'Lost');
END
GO
SET IDENTITY_INSERT dbo.ProposalStatusLU OFF;

/*
	02/24/2022 [Koovackal] - IES-846 Create 2 new statuses

	## END ##
*/

/*
	03/10/2022 [Quijano] - IES-854 Create New Contracts Email

	## START ##
*/

IF COL_LENGTH ('dbo.Proposal','ModExecutedLastEmailed') IS NULL
BEGIN
	ALTER TABLE dbo.Proposal ADD ModExecutedLastEmailed datetime2(7) NULL;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[upsertProposal]
(
	  @ProposalID [int],
	  @UpdateDate [datetime2](7),
	  @ProposalTitle [varchar](100),
	  @OTISOpportunityID [varchar](25),
	  @ProposalStatusID [int],
	  @ProposalTypeID [int],
	  @IsIWTA [bit],
	  @ProgramName [varchar](50),
	  @Customer [varchar](50),
	  @CustomerTypeID [int],
	  @ISGSRoleID [int],
	  @RequestTypeID [int],
	  @RFPNumber [varchar](40) = NULL,
	  @LineOfBusinessID [int],
	  @ProgramAreaID [int],
	  @PricingToolID [int],
	  @BOEToolID [int],
	  @AnticipatedDeliveryDate [date],
	  @ProposalCostElementXREF [varchar] (50),
	  @EstimatedProposalValue [bigint],
	  @ProposalContractTypeXREF [varchar] (50),
	  @DateAssigned  [bit],
	  @CreatedByUserID [int],
	  @RFPIssuedDate [date],
	  @RFPReceivedDate [date],	  
	  @Comments  [varchar] (2500),
	  @ContractTypeGroupID [int],
	  @IsScheduleProposal [bit],
	  @ProposalLocationID [int],
	  @ProposalLocationName [varchar] (50),
	  @BOEToolName [varchar] (50),
	  @PricingToolName [varchar] (50),
	  @ProposalChecklistTypeID [int],
	  @ChangeChecklistFlag [bit],
	  @ProgramProposalStatusID [int],
	  @ProposalClassID int,
	  @WorkflowStatus int,
	  @WorkflowStatusLastUpdated datetime2 = NULL,
	  @LeadEstimatorSignedDT datetime2 = NULL,
	  @LeadEstimatorSignComment varchar (1000) = NULL,
	  @CoverSheetApproverSignedDT datetime2 = NULL,
	  @CoverSheetApproverSignComment varchar (1000) = NULL,
	  @PricingVerifierSignedDT datetime2 = NULL,
	  @PricingVerifierSignComment varchar (1000) = NULL,
	  @IndependentReviewerSignedDT datetime2 = NULL,
	  @IndependentReviewerSignComment varchar (1000) = NULL,
	  @LOBEstimatingLeadSignedDT datetime2 = NULL,
	  @LOBEstimatingLeadSignComment varchar (1000) = NULL,
	  @ApprovalEmailText varchar (1000) = NULL,
	  @RevisedSubmittalDate datetime2 = NULL,
	  @CCPDRequired [bit] = NULL,
	  @CostVolumeClassified [bit] = NULL,
	  @DocumentId int = NULL,
	  @ForecastedTrackingID varchar (13) = NULL,
	  @TrackingID varchar (13) = NULL,
	  @IsForecast [bit],
	  @AgreementDate datetime2 = NULL,
	  @CertificationDate datetime2 = NULL,
	  @CutOffDateUtilization int = NULL,
	  @CertificationTimelineCompleted datetime2 = NULL,
    @CertificationLastEmailed datetime2 = NULL,
	  @NoBidDate datetime2 = NULL,
	  @IsRevision bit,
	  @RevisionOfId int,
	  @ReasonCertificationNotRequired INT = 1,
	  @OtherReasonComment VARCHAR(1000) = NULL,
	  @SetupComments VARCHAR(MAX) = NULL,
		@ModExecutedLastEmailed datetime2 = NULL
)
AS
/******************************************************************************
**          
**          Name: [upsertProposal]
**          Desc: Insert/Update Proposal - Basic/General Information
**                
**          
**
**          Auth: Don Canuso
**          Date: 4/3/2013
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ---------------------------------------
**			1/ 7/17		Dusan					Removing Revisions & LMIS
**			1/12/2017	gbrunwo					BOEJ-1688 Update PTM SPs to not 
**												display technical details to the user
**			02/02/2017	tglick					added new field [Revised Submittal Date]
**			3/20/2017	twilson3				BOEJ-1957 Move CCPD from Checklist to Proposal
**			1/24/2017	twilson3				BOEJ-2808 Add RDSB link into PTM
**			3/06/2018	brunworg				BOEJ-3124 Add Forecasted Tracking Number into PTM
**			3/9/2018	twilson3				BOEJ-3128 Integration of Forecasted Tracking ID
**			3/14/2018	twilson3				BOEJ-3118 Email notification for Forecasted Proposal
**			6/06/2018	brunworg				BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
**			8/29/2018	twilson3				BOEJ-3756 Post Proposal redo
**			9/27/2018	ranzalon				BOEJ-3739 Classified Cost Volume
**			3/31/2020	ranzalon				BOEJ-4531 No Bid Date
**			6/17/2020	ranzalon				BOEJ-4535 IsRevision
**			6/23/2020	Dusan					BOEJ-4626 Add Certification Not Required
**			6/23/2020	ranzalon				BOEJ-4669 No new tracking number when IsRevision 
**			7/2/2020	ranzalon				BOEJ-4687 Link Revisions to Revised Proposal
**			7/31/2020	ranzalon				BOEJ-4648 Proposal Setup Comments
**          3/10/2022   jquijano                IES-854
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)
/*
Process Contract Types
*/
IF RIGHT(@ProposalContractTypeXREF, 1) <> ','
	  SET @ProposalContractTypeXREF = @ProposalContractTypeXREF + ','

DECLARE @ContractType TABLE (ContractTypeID INT)

WHILE (SELECT CHARINDEX (',', @ProposalContractTypeXREF) ) > 1
BEGIN
	  
	INSERT INTO @ContractType
	SELECT LEFT (@ProposalContractTypeXREF, CHARINDEX (',', @ProposalContractTypeXREF) -1)
	SET @ProposalContractTypeXREF = RIGHT (@ProposalContractTypeXREF, LEN (@ProposalContractTypeXREF) - CHARINDEX (',', @ProposalContractTypeXREF) )
	  
END
/*
Process Cost Element
*/
IF RIGHT(@ProposalCostElementXREF, 1) <> ','
	  SET @ProposalCostElementXREF = @ProposalCostElementXREF + ','

DECLARE @CostElement TABLE (CostElementID INT)

WHILE (SELECT CHARINDEX (',', @ProposalCostElementXREF) ) > 1
BEGIN
	  
	INSERT INTO @CostElement
	SELECT LEFT (@ProposalCostElementXREF, CHARINDEX (',', @ProposalCostElementXREF) -1)
	SET @ProposalCostElementXREF = RIGHT (@ProposalCostElementXREF, LEN (@ProposalCostElementXREF) - CHARINDEX (',', @ProposalCostElementXREF) )
	  
END

/* Process @IsForecast */
DECLARE @ProposalTrackingGeneratorID int
DECLARE @GeneratedTrackingID bit
SELECT @GeneratedTrackingID = 0
IF @IsForecast = 1
	BEGIN
		SET @TrackingID = NULL
	END
ELSE
	BEGIN
	/* Generate new Tracking ID if this is a new Non-Forecast proposal or editing a Non-Forecast proposal that used to be Forecast */
		IF (@ProposalID < 0 AND @IsRevision = 0) OR @TrackingID = '' OR @TrackingID is NULL
			BEGIN
				SELECT TOP 1 
						@TrackingID = CAST(ProposalYear AS varchar(4)) + '-' + CASE LEN(ProposalNumber) 
																								WHEN 1 THEN '0000' + CAST(ProposalNumber AS varchar(5))
																								WHEN 2 THEN '000' + CAST(ProposalNumber AS varchar(5))
																								WHEN 3 THEN '00' + CAST(ProposalNumber AS varchar(5))
																								WHEN 4 THEN '0' + CAST(ProposalNumber AS varchar(5))
																								WHEN 5 THEN CAST(ProposalNumber AS varchar(5))
																						END ,
						@ProposalTrackingGeneratorID = ProposalTrackingGeneratorID
				FROM dbo.ProposalTrackingGenerator
				WHERE ProposalYear = YEAR (GetDate()) AND UsedByProposalID IS NULL
			
			
				/*Removing first 2 characters in new proposals*/
				SET @TrackingID =	RIGHT(@TrackingID, (LEN(@TrackingID)-2))
				SET @GeneratedTrackingID = 1
			END
	END

/*
<ProposalID> +� � � + <Proposal Title>

Proposal ID is (<YYYY>-<5-digit sequence starting at 00001><Rev #) 
2013-10001Rnn
*/
IF @ProposalID  < 0  /*Insert Record*/
	BEGIN
		DECLARE @Inserted AS Table (ID int)
		SET @UpdateDate = GETDATE()

	INSERT INTO [dbo].[Proposal]
		([UpdateDate]
		,[ProposalTrackingID]
		,[ProposalTitle]
		,[OTISOpportunityID]
		,[ProposalStatusID]
		,[DateCreated]/*Current Date when created*/
		,[DateAssigned]/*Current Date when created*/
		,[ProposalTypeID]
		,[IsIWTA]
		,[ProgramName]
		,[Customer]
		,[CustomerTypeID]
		,[ISGSRoleID]
		,[RequestTypeID]
		,[RFPNumber]
		,[LineOfBusinessID]
		,[ProgramAreaID]
		,[PricingToolID]
		,[BOEToolID]
		,[AnticipatedDeliveryDate]
		,[EstimatedProposalValue]  
		,[IsActive]
		,[CreatedByUserID]
		,[RFPIssuedDate]		   
		,[RFPReceivedDate]		   
		,[Comments]
		,[ContractTypeGroupID]
		,[IsScheduleProposal]
		,[ProposalLocationID]
		,[ProposalLocationName]
		,[BOEToolName]
		,[PricingToolName]
		,[ProgramProposalStatusID]
		,[ProposalClassID]
		,[WorkflowStatus]
		,[WorkflowStatusLastUpdated]
		,[LeadEstimatorSignedDT]
		,[LeadEstimatorSignComment]
		,[CoverSheetApproverSignedDT]
		,[CoverSheetApproverSignComment]
		,[PricingVerifierSignedDT]
		,[PricingVerifierSignComment]
		,[IndependentReviewerSignedDT]
		,[IndependentReviewerSignComment]
		,[LOBEstimatingLeadSignedDT]
		,[LOBEstimatingLeadSignComment]
		,[ApprovalEmailText]
		,[RevisedSubmittalDate]
		,[CCPDRequired]
		,[CostVolumeClassified]
		,[DocumentId]
		,[ForecastedTrackingID]
		,[AgreementDate]
		,[CertificationDate]
		,[CutOffDateUtilization]
		,[CertificationTimelineCompleted]
		,[CertificationLastEmailed]
		,[NoBidDate]
		,[RevisionOfId]
		,[ReasonCertificationNotRequired]
		,[OtherReasonComment]
		,[SetupComments]
		,[ModExecutedLastEmailed]
		)
	OUTPUT inserted.ProposalID INTO @Inserted
	VALUES
		(@UpdateDate
		,@TrackingID
		,@ProposalTitle
		,@OTISOpportunityID
		,1 /*InProgress On Creation*/
		,@UpdateDate/*Current Date when created*/
		,@UpdateDate/*Current Date when created*/
		,@ProposalTypeID
		,@IsIWTA
		,@ProgramName
		,@Customer
		,@CustomerTypeID
		,@ISGSRoleID
		,@RequestTypeID
		,@RFPNumber
		,@LineOfBusinessID
		,@ProgramAreaID
		,@PricingToolID
		,@BOEToolID
		,@AnticipatedDeliveryDate
		,@EstimatedProposalValue
		,1 /*Active On Insert*/
		,@CreatedByUserID
		,@RFPIssuedDate
		,@RFPReceivedDate
		,@Comments
		,@ContractTypeGroupID
		,@IsScheduleProposal
		,@ProposalLocationID
		,@ProposalLocationName
		,@BOEToolName
		,@PricingToolName
		,@ProgramProposalStatusID
		,@ProposalClassID
		,@WorkflowStatus
		,@WorkflowStatusLastUpdated
		,@LeadEstimatorSignedDT
		,@LeadEstimatorSignComment
		,@CoverSheetApproverSignedDT
		,@CoverSheetApproverSignComment
		,@PricingVerifierSignedDT
		,@PricingVerifierSignComment
		,@IndependentReviewerSignedDT
		,@IndependentReviewerSignComment
		,@LOBEstimatingLeadSignedDT
		,@LOBEstimatingLeadSignComment
		,@ApprovalEmailText
		,@RevisedSubmittalDate
		,@CCPDRequired
		,@CostVolumeClassified
		,@DocumentId
		,@ForecastedTrackingID
		,@AgreementDate
		,@CertificationDate
		,@CutOffDateUtilization
		,@CertificationTimelineCompleted
		,@CertificationLastEmailed
		,@NoBidDate
		,@RevisionOfId
		,@ReasonCertificationNotRequired
		,@OtherReasonComment
		,@SetupComments
		,@ModExecutedLastEmailed
		)

		SELECT @ProposalID = ID FROM @Inserted
	  
		IF @GeneratedTrackingID = 1
		BEGIN
			UPDATE dbo.ProposalTrackingGenerator
			SET UsedByProposalID = @ProposalID  
			WHERE ProposalTrackingGeneratorID = @ProposalTrackingGeneratorID 
		END
  
		INSERT INTO [dbo].[ProposalContractTypeXREF]
			([ProposalID]
			,[ContractTypeID])
		SELECT DISTINCT @ProposalID, ContractTypeID           
		FROM @ContractType


		INSERT INTO [dbo].[ProposalCostElementXREF]
			([ProposalID]
			,[CostElementID])
		SELECT DISTINCT @ProposalID, CostElementID           
		FROM @CostElement

		/*CALL STORED PROCEDURE THAT MANAGES CHECKLIST*/
		EXECUTE [dbo].[upsertProposalChecklistTemplate] @ProposalID, @ProposalChecklistTypeID, @ChangeChecklistFlag
	END
ELSE
	/*Update*/
	BEGIN
		IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID) = @UpdateDate
			BEGIN
				SET @UpdateDate = GETDATE()
							  
				UPDATE [dbo].[Proposal]
					SET  [UpdateDate] = @UpdateDate
							,[ProposalTitle] = @ProposalTitle
							,[OTISOpportunityID] = @OTISOpportunityID
						,[ProposalStatusID] = @ProposalStatusID
						,[ProposalTypeID] = @ProposalTypeID
						,[IsIWTA] = @IsIWTA
						,[ProgramName] = @ProgramName
						,[Customer] = @Customer
						,[CustomerTypeID] = @CustomerTypeID
						,[ISGSRoleID] = @ISGSRoleID
						,[RequestTypeID] = @RequestTypeID
						,[RFPNumber] = @RFPNumber
						,[LineOfBusinessID] = @LineOfBusinessID
						,[ProgramAreaID] = @ProgramAreaID
						,[PricingToolID] = @PricingToolID
						,[BOEToolID] = @BOEToolID
						,[AnticipatedDeliveryDate] = @AnticipatedDeliveryDate
						,[EstimatedProposalValue] = @EstimatedProposalValue
						,[DateAssigned] = 
										CASE WHEN @DateAssigned = 1 THEN @UpdateDate
										ELSE [DateAssigned]
										END
						--,[CreatedByUserID] = @CreatedByUserID
						,[RFPIssuedDate] = @RFPIssuedDate
						,[RFPReceivedDate] = @RFPReceivedDate		   
						,[Comments] = @Comments
						,[ContractTypeGroupID] = @ContractTypeGroupID
						,[IsScheduleProposal] = @IsScheduleProposal
						,[ProposalLocationID] = @ProposalLocationID
						,[ProposalLocationName] = @ProposalLocationName
						,[BOEToolName] = @BOEToolName
						,[PricingToolName] = @PricingToolName
						,[ProgramProposalStatusID] = @ProgramProposalStatusID
						,[ProposalClassID] = @ProposalClassID
						,[WorkflowStatus] = @WorkflowStatus
						,[WorkflowStatusLastUpdated] = @WorkflowStatusLastUpdated
						,[LeadEstimatorSignedDT] = @LeadEstimatorSignedDT
						,[LeadEstimatorSignComment] = @LeadEstimatorSignComment
						,[CoverSheetApproverSignedDT] = @CoverSheetApproverSignedDT
						,[CoverSheetApproverSignComment] = @CoverSheetApproverSignComment
						,[PricingVerifierSignedDT] = @PricingVerifierSignedDT
						,[PricingVerifierSignComment] = @PricingVerifierSignComment
						,[IndependentReviewerSignedDT] = @IndependentReviewerSignedDT
						,[IndependentReviewerSignComment] = @IndependentReviewerSignComment
						,[LOBEstimatingLeadSignedDT] = @LOBEstimatingLeadSignedDT
						,[LOBEstimatingLeadSignComment] = @LOBEstimatingLeadSignComment
						,[ApprovalEmailText] = @ApprovalEmailText
						,[RevisedSubmittalDate] = @RevisedSubmittalDate
						,[CCPDRequired] = @CCPDRequired
						,[CostVolumeClassified] = @CostVolumeClassified
						,[DocumentId] = @DocumentId
						,[ForecastedTrackingId] = @ForecastedTrackingId
						,[ProposalTrackingID] = @TrackingID
						,[ForecastEmailSent] = 0
						,[AgreementDate] = @AgreementDate
						,[CertificationDate] = @CertificationDate
						,[CutOffDateUtilization] = @CutOffDateUtilization
						,[CertificationTimelineCompleted] = @CertificationTimelineCompleted
						,[CertificationLastEmailed] = @CertificationLastEmailed
						,[NoBidDate] = @NoBidDate
						,[RevisionOfId] = @RevisionOfId
						,[ReasonCertificationNotRequired] = @ReasonCertificationNotRequired
						,[OtherReasonComment] = @OtherReasonComment
						,[SetupComments] = @SetupComments
						,[ModExecutedLastEmailed] = @ModExecutedLastEmailed

						WHERE 
							ProposalID = @ProposalID;

				WITH [Target] AS 
					(
							SELECT 
								ProposalID,
								ContractTypeID
							FROM [dbo].[ProposalContractTypeXREF]
							WHERE
								ProposalID = @ProposalID
					)
				MERGE INTO [Target]
				USING (
							SELECT DISTINCT 
								@ProposalID AS ProposalID, 
								ContractTypeID           
							FROM @ContractType
							)  AS [Source] ON
							[Target].[ProposalID] = [Source].[ProposalID] AND
							[Target].[ContractTypeID] = [Source].[ContractTypeID]
				WHEN NOT MATCHED BY SOURCE 
				THEN 
				DELETE      

				WHEN NOT MATCHED BY TARGET THEN
				INSERT (ProposalID, ContractTypeID)
				VALUES ([Source].[ProposalID], [Source].[ContractTypeID]); 
				WITH [Target] AS 
					(
							SELECT 
								ProposalID,
								CostElementID
							FROM [dbo].[ProposalCostElementXREF]
							WHERE
								ProposalID = @ProposalID
					)
				MERGE INTO [Target]
				USING (
							SELECT DISTINCT 
								@ProposalID AS ProposalID, 
								CostElementID           
							FROM @CostElement
							)  AS [Source] ON
							[Target].[ProposalID] = [Source].[ProposalID] AND
							[Target].[CostElementID] = [Source].[CostElementID]
				WHEN NOT MATCHED BY SOURCE 
				THEN 
				DELETE      

				WHEN NOT MATCHED BY TARGET THEN
				INSERT (ProposalID, CostElementID)
				VALUES ([Source].[ProposalID], [Source].[CostElementID])
				;     

				IF @ChangeChecklistFlag = 1
				BEGIN
					/*
						Before updating the checklist, the Proposal needs to go back to 
						In Progress with all the rules of unlocking applied
					*/

					DECLARE @CurrentProposalDateTime [datetime2](7)
					SELECT @CurrentProposalDateTime = [UpdateDate] 
							FROM [dbo].[Proposal] 
							WHERE ProposalID = @ProposalID

					EXECUTE [dbo].[unlockProposal] @ProposalID, @UpdateDate, 3 /*@UnlockOptionID*/

					/*CALL STORED PROCEDURE THAT MANAGES CHECKLIST*/
					EXECUTE [dbo].[upsertProposalChecklistTemplate] @ProposalID, @ProposalChecklistTypeID, @ChangeChecklistFlag
				END

				IF @GeneratedTrackingID = 1
					BEGIN
						UPDATE dbo.ProposalTrackingGenerator
						SET UsedByProposalID = @ProposalID  
						WHERE ProposalTrackingGeneratorID = @ProposalTrackingGeneratorID 
					END
			END
		ELSE
			BEGIN
				SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
						@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
				RETURN
			END
	END

IF @@ERROR = 0
	  SELECT @ProposalID as ProposalID

GO

/*
	03/10/2022 [Quijano] - IES-854 Create New Contracts Email

	## END ##
*/

/*
	03/15/2022 [Quijano] - IES-854 Create New Contracts Email

	## START ##
*/
CREATE OR ALTER PROCEDURE [dbo].[deleteProposal]
(
@ProposalID int,
@UpdateDate datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteProposal]
**		Desc: Delete Proposal
**			
**
**		Auth: Don Canuso
**		Date: 04/3/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		9/7/2017	twilson3			BOEJ-2459 Add Attachment table
**		7/6/2020	Dusan				Attachment Proposals table
**		11/2/2021	Koovackal			Contracts Data and Offer tables
**		03/14/2022	Quijano				Remove lingering offers deletion
*******************************************************************************/
SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID ) = @UpdateDate
		BEGIN
			DECLARE @attachmentIds AS Table (Id int)
			INSERT INTO @attachmentIds SELECT AttachmentId FROM dbo.ProposalsAttachments WHERE ProposalId = @ProposalId AND IsRevisionReference = 0
			DELETE FROM dbo.ProposalsAttachments WHERE ProposalID = @ProposalID
			DELETE FROM dbo.Attachment WHERE Id IN (SELECT * FROM @attachmentIds)
			DELETE FROM dbo.ProposalUserRole WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalContractTypeXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalCostElementXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalPARChecklistXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalPPRChecklistXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalChecklist WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalChecklistComplete WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalContractsData WHERE ProposalID = @ProposalID
			DELETE FROM dbo.Proposal WHERE ProposalID = @ProposalID
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (@ErrorMessage, 11, 1)
			RETURN
		END
GO

/*
	03/15/2022 [Quijano] - IES-854 Create New Contracts Email

	## END ##
*/