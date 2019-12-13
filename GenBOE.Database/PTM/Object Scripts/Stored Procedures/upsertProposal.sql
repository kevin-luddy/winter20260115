IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposal];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[upsertProposal]
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
      @CertificationLastEmailed datetime2 = NULL
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
		IF @ProposalID < 0 OR @TrackingID = '' OR @TrackingID is NULL
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
<ProposalID> +” – “ + <Proposal Title>

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