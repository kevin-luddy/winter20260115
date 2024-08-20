EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.9';
GO

-- Updated vwProposalLogReport.view for "Lost" to "Not Awarded"
UPDATE [dbo].[ProposalStatusLU]
SET ProposalStatus = 'Not Awarded'
WHERE ProposalStatus = 'Lost'
GO