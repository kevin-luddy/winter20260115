IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBOEFormPBOE]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBOEFormPBOE];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertBOEFormPBOE]
(
@PBOEFormID int,
@UpdateDT datetime2,
@WorkspaceID int,
@FormName varchar(200),
@Description varchar(max) = NULL,
@ProposalTitle varchar(200) = NULL,
@ProposalDate varchar(10) = NULL,
@Poc varchar(65)=  NULL,
@PocPhone varchar(60) = NULL,
@Approver varchar(65) = NULL,
@ApproverPhone varchar(60) = NULL,
@FormVersion int,
@CCoPD int,
@CCoPDOtherText varchar (100) = NULL,
@RFP varchar(50) = NULL,
@ProposalNumber varchar(50) = NULL,
@SupplierName varchar(50) = NULL,
@ValidityDate varchar(10) = NULL,
@ShouldCostEstimate int,
@ShouldCostEstimateDate date = NULL,
@RFPRelease int,
@RFPReleaseDate date = NULL,
@FirmSupplierReceipt int,
@FirmSupplierReceiptDate date = NULL,
@SourceSelection int,
@SourceSelectionDate date = NULL,
@CID int,
@CIDDate date = NULL,
@PriceAnalysis int,
@PriceAnalysisDate date = NULL,
@TechnicalEvaluation int,
@TechnicalEvaluationDate date = NULL,
@FactFinding int,
@FactFindingDate date = NULL,
@CostAnalysis int,
@CostAnalysisDate date = NULL,
@GovtPricing int,
@GovtPricingDate date = NULL,
@SupplierNegotiations int,
@SupplierNegotiationsDate date = NULL,
@MOU int,
@MOUDate date = NULL,
@PlannedDate_WrittenApproval date = NULL,
@PlannedDate_ApprovedSubmission date = NULL,
@ResourceIDs varchar (max),
@ClinContractTypes varchar(max),
@Revision int,
@CIDText varchar(50) = NULL,
@PriceAnalysisText varchar(50) = NULL,
@TechnicalEvaluationText varchar(50) = NULL,
@FactFindingText varchar(50) = NULL,
@CostAnalysisText varchar(50) = NULL,
@GovtPricingText varchar(50) = NULL,
@SupplierNegotiationsText varchar(50) = NULL,
@MOUText varchar(50) = NULL,
@ShouldCostEstimateText varchar(50) = NULL,
@RFPReleaseText varchar(50) = NULL,
@FirmSupplierReceiptText varchar(50) = NULL,
@SourceSelectionText varchar(50) = NULL,
@SupplierCCoPD int,
@SourceSelectionDescription VARCHAR(MAX) = NULL,
@CommercialityDescription VARCHAR(MAX) = NULL,
@TechnicalEvaluationDescription VARCHAR(MAX) = NULL,
@PriceAnalysisDescription VARCHAR(MAX) = NULL,
@CostAnalysisDescription VARCHAR(MAX) = NULL,
@RationaleValueSummary VARCHAR(MAX) = NULL,
@GovtPricingReceived int,
@GovtPricingReceivedDate date,
@GovtPricingReceivedText VARCHAR(50),
@CostAnalysisUnqual int,
@CostAnalysisUnqualDate date,
@CostAnalysisUnqualText VARCHAR(50),
@VendorId VARCHAR(20),
@SupplierProposedValue DECIMAL(13,2)
)
AS
/******************************************************************************
**		 
**		Name:	[upsertBOEFormPBOE]
**		Desc:	Insert/Update BOE Forms PBOE
**			
**		
**
**		Auth: Tim Wilson
**		Date: October 2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		10/27/16	twilson3			BOEJ-1531 Add BOE Forms CLIN xref table
**      11/14/16	twilson3			BOEJ-1541 INL Forms Text Entry Planned Fields
**		03/08/17	ranzalon			BOEJ-1944 Increase size of poc fields
**		10/27/17	twilson3			BOEJ-2578 Partial Save PBOE/IBOE
**		04/19/22	jquijano			IES-1014 Add new fields
**		05/02/22	jquijano			IES-1126 Add VendorID, SupplierProposedValue
*******************************************************************************/

SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

/*
Process Resource IDs
*/
IF RIGHT(@ResourceIDs, 1) <> ','
	SET @ResourceIDs = @ResourceIDs + ','

DECLARE @Resources TABLE (ResourceID INT)
WHILE (SELECT CHARINDEX (',', @ResourceIDs) ) > 1
	BEGIN	
		INSERT INTO @Resources (ResourceID)
		SELECT LEFT (@ResourceIDs, CHARINDEX (',', @ResourceIDs) -1)
		SET @ResourceIDs = RIGHT (@ResourceIDs, LEN (@ResourceIDs) - CHARINDEX (',', @ResourceIDs) )
	END

/*
Process Clin Contract Types
*/
IF RIGHT(@ClinContractTypes, 1) <> ','
	SET @ClinContractTypes = @ClinContractTypes + ','

DECLARE @Clins TABLE (ClinID INT, ContractType INT)
DECLARE @Breaker INT
WHILE (SELECT CHARINDEX (',', @ClinContractTypes) ) > 1
	BEGIN	
		SET @Breaker = CHARINDEX (':', @ClinContractTypes)
		INSERT INTO @Clins (ClinID, ContractType)
		SELECT LEFT (@ClinContractTypes, @Breaker -1), SUBSTRING (@ClinContractTypes, @Breaker + 1, CHARINDEX (',', @ClinContractTypes) - @Breaker - 1)
		SET @ClinContractTypes = RIGHT (@ClinContractTypes, LEN (@ClinContractTypes) - CHARINDEX (',', @ClinContractTypes) )
	END

DECLARE @InsertedPBOE AS Table (PBOEID int)

IF @PBOEFormID  < 0  /*Insert Record*/
	BEGIN	
		SET @UpdateDT = GETDATE()	
		INSERT INTO [dbo].[BOEFormPBOE]
			   (
				[UpdateDT]
			    ,[WorkspaceID]
				,[FormName]
				,[Description]
				,[ProposalTitle]
				,[ProposalDate]
				,[Poc]
				,[PocPhone]
				,[Approver]
				,[ApproverPhone]
				,[Revision]
				,[FormVersion]
				,[CCoPD] 
				,[CCoPDOtherText]
				,[RFP]
				,[ProposalNumber] 
				,[SupplierName]
				,[ValidityDate] 
				,[ShouldCostEstimate] 
				,[ShouldCostEstimateDate] 
				,[RFPRelease] 
				,[RFPReleaseDate] 
				,[FirmSupplierReceipt] 
				,[FirmSupplierReceiptDate] 
				,[SourceSelection] 
				,[SourceSelectionDate] 
				,[CID] 
				,[CIDDate] 
				,[PriceAnalysis] 
				,[PriceAnalysisDate] 
				,[TechnicalEvaluation] 
				,[TechnicalEvaluationDate] 
				,[FactFinding] 
				,[FactFindingDate] 
				,[CostAnalysis] 
				,[CostAnalysisDate] 
				,[GovtPricing] 
				,[GovtPricingDate] 
				,[SupplierNegotiations] 
				,[SupplierNegotiationsDate] 
				,[MOU] 
				,[MOUDate] 
				,[PlannedDate_WrittenApproval] 
				,[PlannedDate_ApprovedSubmission] 
				,[CIDText]
				,[PriceAnalysisText]
				,[TechnicalEvaluationText]
				,[FactFindingText]
				,[CostAnalysisText]
				,[GovtPricingText]
				,[SupplierNegotiationsText]
				,[MOUText]
				,ShouldCostEstimateText
				,RFPReleaseText
				,FirmSupplierReceiptText
				,SourceSelectionText
				,[SupplierCCoPD]
				,[SourceSelectionDescription]
				,[CommercialityDescription]
				,[TechnicalEvaluationDescription]
				,[PriceAnalysisDescription]
				,[CostAnalysisDescription]
				,[RationaleValueSummary]
				,[GovtPricingReceived]
				,[GovtPricingReceivedDate]
				,[GovtPricingReceivedText]
				,[CostAnalysisUnqual]
				,[CostAnalysisUnqualDate]
				,[CostAnalysisUnqualText]
				,[VendorId]
				,[SupplierProposedValue]
			   )
		OUTPUT inserted.PBOEFormID INTO @InsertedPBOE            
		VALUES
			   (
				@UpdateDT,
				@WorkspaceID,
				@FormName,
				@Description,
				@ProposalTitle,
				@ProposalDate,
				@Poc,
				@PocPhone,
				@Approver,
				@ApproverPhone,
				0, -- Revision always starts at 0
				@FormVersion,
				@CCoPD, 
				@CCoPDOtherText,
				@RFP,
				@ProposalNumber, 
				@SupplierName,
				@ValidityDate, 
				@ShouldCostEstimate, 
				@ShouldCostEstimateDate, 
				@RFPRelease, 
				@RFPReleaseDate, 
				@FirmSupplierReceipt, 
				@FirmSupplierReceiptDate, 
				@SourceSelection, 
				@SourceSelectionDate, 
				@CID, 
				@CIDDate, 
				@PriceAnalysis, 
				@PriceAnalysisDate, 
				@TechnicalEvaluation, 
				@TechnicalEvaluationDate, 
				@FactFinding, 
				@FactFindingDate, 
				@CostAnalysis, 
				@CostAnalysisDate, 
				@GovtPricing, 
				@GovtPricingDate, 
				@SupplierNegotiations, 
				@SupplierNegotiationsDate, 
				@MOU, 
				@MOUDate, 
				@PlannedDate_WrittenApproval, 
				@PlannedDate_ApprovedSubmission,
				@CIDText,
				@PriceAnalysisText,
				@TechnicalEvaluationText,
				@FactFindingText,
				@CostAnalysisText,
				@GovtPricingText,
				@SupplierNegotiationsText,
				@MOUText,
				@ShouldCostEstimateText,
				@RFPReleaseText,
				@FirmSupplierReceiptText,
				@SourceSelectionText,
				@SupplierCCoPD,
				@SourceSelectionDescription,
				@CommercialityDescription,
				@TechnicalEvaluationDescription,
				@PriceAnalysisDescription,
				@CostAnalysisDescription,
				@RationaleValueSummary,
				@GovtPricingReceived,
				@GovtPricingReceivedDate,
				@GovtPricingReceivedText,
				@CostAnalysisUnqual,
				@CostAnalysisUnqualDate,
				@CostAnalysisUnqualText,
				@VendorId,
				@SupplierProposedValue
			   )
		SELECT @PBOEFormID = PBOEID FROM @InsertedPBOE
	
		IF EXISTS (SELECT * FROM @Resources)
			BEGIN 
				INSERT INTO [dbo].[BOEFormPBOEResourcesXREF]
				   ([PBOEFormID]
				   ,[ResourceID]
				   )
				 SELECT
				   @PBOEFormID,
				   ResourceID
				   FROM @Resources
			END	

		IF EXISTS (SELECT * FROM @Clins)
			BEGIN 
				INSERT INTO [dbo].[BOEFormPBOECLINsXREF]
				   ([PBOEFormID]
				   ,[ClinID]
				   ,[ContractType]
				   )
				 SELECT
				   @PBOEFormID,
				   [ClinID],
				   [ContractType]
				   FROM @Clins
			END	
	END	
ELSE
	/*Update*/
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[BOEFormPBOE] WHERE [PBOEFormID] = @PBOEFormID) = @UpdateDT
			BEGIN			
				SET @UpdateDT = GETDATE()				
				UPDATE [dbo].[BOEFormPBOE]
					SET 
						[UpdateDT] = @UpdateDT,
						[FormName] = @FormName,
						[Description] = @Description,
						[ProposalTitle] = @ProposalTitle,
						[ProposalDate] = @ProposalDate,
						[Poc] = @Poc,
						[PocPhone] = @PocPhone,
						[Approver] =@Approver,
						[ApproverPhone] = @ApproverPhone,
						[Revision] = @Revision,
						[CCoPD] = @CCoPD, 
						[CCoPDOtherText] = @CCoPDOtherText,
						[RFP] = @RFP,
						[ProposalNumber] = @ProposalNumber, 
						[SupplierName] = @SupplierName,
						[ValidityDate] = @ValidityDate, 
						[ShouldCostEstimate] = @ShouldCostEstimate, 
						[ShouldCostEstimateDate] = @ShouldCostEstimateDate, 
						[RFPRelease] = @RFPRelease, 
						[RFPReleaseDate] = @RFPReleaseDate, 
						[FirmSupplierReceipt] = @FirmSupplierReceipt, 
						[FirmSupplierReceiptDate] = @FirmSupplierReceiptDate, 
						[SourceSelection] = @SourceSelection, 
						[SourceSelectionDate] = @SourceSelectionDate, 
						[CID] = @CID, 
						[CIDDate] = @CIDDate, 
						[PriceAnalysis] = @PriceAnalysis, 
						[PriceAnalysisDate] = @PriceAnalysisDate, 
						[TechnicalEvaluation] = @TechnicalEvaluation, 
						[TechnicalEvaluationDate] = @TechnicalEvaluationDate, 
						[FactFinding] = @FactFinding, 
						[FactFindingDate] = @FactFindingDate, 
						[CostAnalysis] = @CostAnalysis, 
						[CostAnalysisDate] = @CostAnalysisDate, 
						[GovtPricing] = @GovtPricing, 
						[GovtPricingDate] = @GovtPricingDate, 
						[SupplierNegotiations] = @SupplierNegotiations, 
						[SupplierNegotiationsDate] = @SupplierNegotiationsDate, 
						[MOU] = @MOU, 
						[MOUDate] = @MOUDate, 
						[PlannedDate_WrittenApproval] = @PlannedDate_WrittenApproval, 
						[PlannedDate_ApprovedSubmission] = @PlannedDate_ApprovedSubmission,
						[CIDText] = @CIDText,
						[PriceAnalysisText] = @PriceAnalysisText,
						[TechnicalEvaluationText] = @TechnicalEvaluationText,
						[FactFindingText] = @FactFindingText,
						[CostAnalysisText] = @CostAnalysisText,
						[GovtPricingText] = @GovtPricingText,
						[SupplierNegotiationsText] = @SupplierNegotiationsText,
						[MOUText] = @MOUText,
						ShouldCostEstimateText = @ShouldCostEstimateText,
						RFPReleaseText = @RFPReleaseText,
						FirmSupplierReceiptText = @FirmSupplierReceiptText,
						SourceSelectionText = @SourceSelectionText,
						[SupplierCCoPD] = @SupplierCCoPD,
						[SourceSelectionDescription] = @SourceSelectionDescription,
						[CommercialityDescription] = @CommercialityDescription,
						[TechnicalEvaluationDescription] = @TechnicalEvaluationDescription,
						[PriceAnalysisDescription] = @PriceAnalysisDescription,
						[CostAnalysisDescription] = @CostAnalysisDescription,
						[RationaleValueSummary] = @RationaleValueSummary,
						[GovtPricingReceived] = @GovtPricingReceived,
						[GovtPricingReceivedDate] = @GovtPricingReceivedDate,
						[GovtPricingReceivedText] = @GovtPricingReceivedText,
						[CostAnalysisUnqual] = @CostAnalysisUnqual,
						[CostAnalysisUnqualDate] = @CostAnalysisUnqualDate,
						[CostAnalysisUnqualText] = @CostAnalysisUnqualText,
						[VendorId] = @VendorId,
						[SupplierProposedValue] = @SupplierProposedValue
				WHERE
					[PBOEFormID] = @PBOEFormID

				DELETE FROM [dbo].[BOEFormPBOEResourcesXREF] WHERE [PBOEFormID] = @PBOEFormID
				DELETE FROM [dbo].[BOEFormPBOECLINsXREF] WHERE [PBOEFormID] = @PBOEFormID

				IF EXISTS (SELECT * FROM @Resources)
					BEGIN 
						INSERT INTO [dbo].[BOEFormPBOEResourcesXREF]
							([PBOEFormID]
							,[ResourceID]
							)
							SELECT
							@PBOEFormID,
							ResourceID
							FROM @Resources
					END	

				IF EXISTS (SELECT * FROM @Clins)
					BEGIN 
						INSERT INTO [dbo].[BOEFormPBOECLINsXREF]
						   ([PBOEFormID]
						   ,[ClinID]
						   ,[ContractType]
						   )
						 SELECT
						   @PBOEFormID,
						   [ClinID],
						   [ContractType]
						   FROM @Clins
					END	
			END		
			ELSE
				BEGIN
				
					SET @ErrorMessage =   'The PBOE has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
						@ErrorMessage, -- Message text.
						11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
					RETURN
				END	
	END

IF @@ERROR = 0
	SELECT	@PBOEFormID AS PBOEFormID
GO