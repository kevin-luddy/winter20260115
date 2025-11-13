EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.2';
GO

-- Author: ranzalon
-- JIRA Story: PROPH-3038
-- Contracts Tab Delegation of Authority Addition & Additional EPP Date Fields

IF NOT EXISTS (SELECT * FROM dbo.EppDelegationAuthorityLU WHERE Id IN (5))
BEGIN
	SET IDENTITY_INSERT dbo.EppDelegationAuthorityLU ON;

	INSERT INTO EppDelegationAuthorityLU
		(Id, Text)
	VALUES
		(5, 'Mission Segment')
		
	SET IDENTITY_INSERT dbo.EppDelegationAuthorityLU OFF;
END
GO

IF COL_LENGTH('dbo.ProposalContractsData', 'BidEppDate') IS NULL
BEGIN
	ALTER TABLE dbo.ProposalContractsData
	ADD BidEppDate DATE NULL
END
GO

IF COL_LENGTH('dbo.ProposalContractsData', 'MissionSegmentEppDate') IS NULL
BEGIN
	ALTER TABLE dbo.ProposalContractsData
	ADD MissionSegmentEppDate DATE NULL
END
GO