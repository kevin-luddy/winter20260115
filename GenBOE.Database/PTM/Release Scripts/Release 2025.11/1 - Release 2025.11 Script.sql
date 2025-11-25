EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.11';
GO

	IF COL_LENGTH('dbo.Proposal', 'DraftRfpIssuedDate') IS NULL
		BEGIN	

		ALTER TABLE [dbo].[Proposal]
		ADD DraftRfpIssuedDate datetime;
	
END