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

IF OBJECT_ID('dbo.ProposalContractsData', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.ProposalContractsData (
		ProposalContractsDataId			INT				PRIMARY KEY		IDENTITY(1,1),
		UpdateDT						DATETIME2(7)	NOT NULL,
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
