EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.11';
GO

-- Author: Yansen Tjandra (e402751)
-- PROPH-3280 - 12/03/2025
-- Add new column DraftRfpIssuedDate to Proposal table
	IF COL_LENGTH('dbo.Proposal', 'DraftRfpIssuedDate') IS NULL
		BEGIN	

		ALTER TABLE [dbo].[Proposal]
		ADD DraftRfpIssuedDate datetime;
	
END