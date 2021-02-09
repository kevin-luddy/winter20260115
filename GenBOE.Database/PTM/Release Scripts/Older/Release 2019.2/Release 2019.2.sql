EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2019.2';
GO

-- Create version 12, with an Id = 13
DECLARE @newChecklistId INT = 13;

IF NOT EXISTS (SELECT 1 FROM ProposalAdequacyReview WHERE ProposalAdequacyReviewID = @newChecklistId)
BEGIN
	UPDATE ProposalAdequacyReview SET IsCurrent = 0;
    SET IDENTITY_INSERT ProposalAdequacyReview ON;
	INSERT INTO ProposalAdequacyReview (ProposalAdequacyReviewID, ChecklistVersion, IsCurrent, ProposalChecklistTypeID)
		VALUES (@newChecklistId, @newChecklistId - 1, 1, 1);
    SET IDENTITY_INSERT ProposalAdequacyReview OFF;

	INSERT INTO PARChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalAdequacyReviewID, QuestionNumber, Reference, SubmissionItem)
		SELECT ChecklistText, TextTypeID, SortOrder, ColumnOrder, @newChecklistId, QuestionNumber, Reference, SubmissionItem
			FROM PARChecklistContent
			WHERE ProposalAdequacyReviewId = @newChecklistId - 1;
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
END
GO

IF NOT EXISTS (
	SELECT 1 FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
		WHERE T.name = 'PARChecklistContent' AND C.name = 'YesOnly' AND S.name = 'dbo')
BEGIN
	ALTER TABLE PARChecklistContent
		ADD YesOnly BIT NOT NULL DEFAULT 0;

END
GO

DECLARE @newChecklistId INT = 13;

UPDATE PARChecklistContent 
	SET YesOnly = 1
	WHERE ProposalAdequacyReviewId = @newChecklistId
		AND QuestionNumber IN (1, 2, 3, 4, 5, 10, 13, 26, 30);

-- Check to see if table exists;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CannedResponsesPAR]') AND type in (N'U'))
BEGIN
	CREATE TABLE dbo.CannedResponsesPAR (
		CannedResponseId	INT				PRIMARY KEY		IDENTITY,
		Text				VARCHAR(500)	NOT NULL,
		QuestionId			INT				NOT NULL		REFERENCES PARChecklistContent(PARChecklistContentId)
	);
END
GO

IF NOT EXISTS (
	SELECT 1 FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
		WHERE T.name = 'ProposalParChecklistXREF' AND C.name = 'CannedResponseId' AND S.name = 'dbo')
BEGIN
	ALTER TABLE ProposalParChecklistXREF
		ADD CannedResponseId INT REFERENCES dbo.CannedResponsesPAR (CannedResponseId);
END
GO

DECLARE @newChecklistId INT = 13;

IF NOT EXISTS (SELECT 1 FROM dbo.CannedResponsesPAR)
BEGIN
INSERT INTO dbo.CannedResponsesPAR(Text, QuestionId)
	VALUES  ('NA - There are no Exceptions for CCOPD for Subcontracts/Suppliers over CCOPD Threshold for Commercial Item Determination (CID) (Q18) or Competition (Q20).', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 6 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no Judgmental factors included in the proposal.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 7 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no contingencies priced in the proposal.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 8 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no CERs or HEFs utilized in the proposal.  Service center factors are addressed with forward pricing rates.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 9 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are not multiple CLINs in the proposal.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 11 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no actual costs for work performed in the proposal.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 12 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no material or subcontract costs in the proposal.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 14 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no subcontracts that require field pricing analysis.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 15 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no subcontracts that require CCOPD.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 16 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no subcontracts/suppliers that require price/cost analysis to be submitted.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 17 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no Exceptions to Certified Cost or Pricing Data for Commercial Item Determination (CID).', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 18 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no subcontracts/suppliers over CCOPD Threshold that are based on competition.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 20 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no IWTAs proposed at Cost (IWTA-C) in this proposal.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 21 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no IWTAs proposed at Price (IWTA-P) in this proposal.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 22 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no Labor Hours estimated in this proposal.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 23 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - There are no Labor Hours estimated in this proposal.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 24 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - This proposal is not covered by service contract labor standards.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 25 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - This proposal does not contain Non-Labor Service Centers or other ODCs.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 27 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - Royalties are not proposed.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 28 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - FCCOM was not proposed.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 29 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - This proposal is not a modification or change order.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 31 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - This proposal is not a price revision or redetermination.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 32 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - This proposal is not incentive contract type.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 33 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - EPAs are not proposed.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 34 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - Performance Based Payments are not proposed.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 35 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - This is a change to an existing contract and the total procurement at the contract level does not exceed 70% with the addition of this change.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 36 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - Contract Clause 52.215-23 is not applicable on this contract.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 36 AND ProposalAdequacyReviewId = @newChecklistId)),
			('NA - Total procurement does not exceed 70% of the total cost.', (SELECT PARChecklistContentId FROM PARChecklistContent WHERE QuestionNumber = 36 AND ProposalAdequacyReviewId = @newChecklistId));
END
GO
