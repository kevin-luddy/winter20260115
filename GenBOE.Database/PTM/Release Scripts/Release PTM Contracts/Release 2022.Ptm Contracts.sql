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
	); 
END

IF OBJECT_ID('dbo.ProposalContractsOffers', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.ProposalContractsOffers (
		ProposalContractsOffersId	INT				PRIMARY KEY			IDENTITY(1,1),
		UpdateDT					DATETIME2(7)	NOT NULL,
		ContractsDataId 			INT 			NOT NULL			REFERENCES ProposalContractsData(ProposalContractsDataId),
		CustomerOfferAmount 		BIGINT			NOT NULL,
		CustomerOfferDate 			DATE			NOT NULL,
		LMCounterOfferDate 			DATE			NULL,
		LMCounterOfferCost			BIGINT			NULL,
		LMCounterOfferCOM			BIGINT			NULL,
		LMCounterOfferProfitFee		BIGINT			NULL
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