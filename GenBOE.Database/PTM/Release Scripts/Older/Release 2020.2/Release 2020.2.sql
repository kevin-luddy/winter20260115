EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2020.2';
GO

/*
		## START ##
		03/05/2020		ranzalon	BOEJ-4490 - No Bid Proposal Status
*/

IF NOT EXISTS (SELECT 1 
		FROM [dbo].[ProposalStatusLU]
		WHERE [ProposalStatus] = 'No Bid')
BEGIN
SET IDENTITY_INSERT [dbo].[ProposalStatusLU] ON

INSERT INTO [dbo].[ProposalStatusLU]
(ProposalStatusID, ProposalStatus)
VALUES (7, 'No Bid');

SET IDENTITY_INSERT [dbo].[ProposalStatusLU] OFF
END

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'dbo' AND 
	T.name = 'Proposal' AND C.name = 'NoBidDate')
BEGIN 

ALTER TABLE [dbo].[Proposal]
ADD [NoBidDate] datetime2(7) NULL;

END

/*
       03/05/2020		ranzalon	BOEJ-4490 - No Bid Proposal Status
       ## END ##
*/