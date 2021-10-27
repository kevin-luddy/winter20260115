EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2021.PtmContracts';
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

-- TODO DB Work Part 2

/*
	10/27/2021 [Koovackal] - IES-442-DB-Work Part 1 and IES-181-DB-Work Part 2.
	                         Update Proposal Checklist table and implement Contracts tab table.

	## END ##
*/
