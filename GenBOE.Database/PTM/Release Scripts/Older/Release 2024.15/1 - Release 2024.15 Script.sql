EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.15';
GO

-- Author: Tim Wilson (twilson3)
-- JIRA Story: PROPH-1046
-- Additional Questions for Contracts Tab (Insurance)

IF OBJECT_ID('dbo.InsuranceTypeLU', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.InsuranceTypeLU (
		Id		INT				PRIMARY KEY		IDENTITY(1,1),
		[Text]	VARCHAR(50)		NULL
	); 

	SET IDENTITY_INSERT dbo.InsuranceTypeLU ON;
	INSERT INTO dbo.InsuranceTypeLU (Id, Text)
		VALUES ('1', 'Political Risk Insurance'), ('2', 'Space Insurance'), ('3', 'Launch In-Orbit Insurance'), ('4', 'Delivery In-Orbit Insurance'), ('5', 'Post Launch Milestone');
	SET IDENTITY_INSERT dbo.InsuranceTypeLU OFF;
END
GO

IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'ProposalContractsData' AND
		C.name = 'IsInsuranceDirect' AND
		S.name = 'dbo'
)
BEGIN
	ALTER TABLE ProposalContractsData ADD IsInsuranceDirect int NULL;
	ALTER TABLE ProposalContractsData ADD InsuranceType int NULL;
	ALTER TABLE ProposalContractsData ADD CONSTRAINT [FK_Contracts_InsuranceType] FOREIGN KEY ([InsuranceType]) REFERENCES dbo.[InsuranceTypeLU]([Id]);
	ALTER TABLE ProposalContractsData ADD ProposedInsurance bigint NULL;
	ALTER TABLE ProposalContractsData ADD NegotiatedInsurance bigint NULL;
END
GO