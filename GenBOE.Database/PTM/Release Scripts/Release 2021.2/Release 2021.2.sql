EXEC dbo.[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2021.2';
GO

IF NOT EXISTS (SELECT * FROM [dbo].[ProposalLocationLU] WHERE [ProposalLocation] = 'Titusville, FL')
BEGIN
	INSERT INTO ProposalLocationLU VALUES ('Titusville, FL, ', 1);
END;