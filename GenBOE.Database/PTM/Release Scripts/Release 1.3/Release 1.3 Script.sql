/*
	## START ##
	
	3/7/2016 [Joe] - BOEJ-1818 Increase size of user's Display Name
*/

ALTER TABLE [genTRACUser] ALTER COLUMN [DisplayName] varchar(256);

/*
	3/7/2016 [Joe] - BOEJ-1818 Increase size of user's Display Name

	## END ##
*/

/*
	## START ##
	
	3/20/2017	twilson3				BOEJ-1909 Remove Profit Tracking #
*/
IF EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'Proposal' AND
					C.name = 'ProfitTrackingNumber' AND
					S.name = 'dbo'
				)
BEGIN
	ALTER TABLE [dbo].[Proposal] DROP COLUMN ProfitTrackingNumber
END
/*
	3/20/2017	twilson3				BOEJ-1909 Remove Profit Tracking #

	## END ##
*/

/*
	## START ##
	
	3/20/2017	twilson3				BOEJ-1957 Move CCPD from Checklist to Proposal
*/
IF NOT EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'Proposal' AND
					C.name = 'CCPDRequired' AND
					S.name = 'dbo'
				)
BEGIN
	ALTER TABLE [dbo].[Proposal] ADD [CCPDRequired] [bit] NULL
END
GO

-- Note: Because of the way the UPDATE works in SQL SERVER, it will still return an error when the IF EXISTS returns false.  
IF EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'ProposalChecklist' AND
					C.name = 'CCPDRequired' AND
					S.name = 'dbo'
				)
BEGIN
	UPDATE p SET p.[CCPDRequired] = c.[CCPDRequired]
	FROM [dbo].[Proposal] p
		INNER JOIN [dbo].[ProposalChecklist] c ON
			p.[ProposalID] = c.[ProposalID]
END
GO

IF EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'ProposalChecklist' AND
					C.name = 'CCPDRequired' AND
					S.name = 'dbo'
				)
BEGIN
	UPDATE [dbo].[Proposal] SET [CoverSheetApproverSignedDT] = NULL, [CoverSheetApproverSignComment] = NULL WHERE [CCPDRequired] = 0
END
GO

-- Update workflow status to ALL Approved if pricing verifier and indp. reviewer have signed (and workflow is started but we haven't gotten past the all approved status yet)
IF EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'ProposalChecklist' AND
					C.name = 'CCPDRequired' AND
					S.name = 'dbo'
				)
BEGIN
	UPDATE [dbo].[Proposal] SET [WorkflowStatus] = 60 WHERE [CCPDRequired] = 0 AND [WorkflowStatus] >= 10 AND [WorkflowStatus] < 60 AND [PricingVerifierSignedDT] IS NOT NULL AND [IndependentReviewerSignedDT] IS NOT NULL
END
GO

IF EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'ProposalChecklist' AND
					C.name = 'CCPDRequired' AND
					S.name = 'dbo'
				)
BEGIN
	ALTER TABLE [dbo].[ProposalChecklist] DROP COLUMN [CCPDRequired]
END
GO
/*
	3/20/2017	twilson3				BOEJ-1957 Move CCPD from Checklist to Proposal

	## END ##
*/

/*
	## START ##
	
	4/7/2017	twilson3				BOEJ-1990 Renaming a Program Area Selection
*/

UPDATE [dbo].[LineOfBusiness] SET [LineOfBusinessName] = 'AFSP', [LineOfBusinessLongName] = 'AFSP', [LineOfBusinessURL] = 'AFSP' WHERE [LineOfBusinessName] = 'Re-Entry'
GO

/*
	4/7/2017	twilson3				BOEJ-1990 Renaming a Program Area Selection

	## END ##
*/