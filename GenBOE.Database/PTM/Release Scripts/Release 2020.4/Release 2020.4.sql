EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2020.4';
GO

/*
	## START ##

	6/15/2020 [ranzalon] - BOEJ-4634 - New Revision 
*/

IF NOT EXISTS (SELECT 1 
		FROM [dbo].[ProposalStatusLU]
		WHERE [ProposalStatus] = 'Revised')
BEGIN
SET IDENTITY_INSERT [dbo].[ProposalStatusLU] ON

INSERT INTO [dbo].[ProposalStatusLU]
(ProposalStatusID, ProposalStatus)
VALUES (8, 'Revised');

SET IDENTITY_INSERT [dbo].[ProposalStatusLU] OFF
END

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'dbo' AND 
	T.name = 'Proposal' AND C.name = 'IsRevision')
BEGIN 

ALTER TABLE [dbo].[Proposal]
ADD [IsRevision] bit NOT NULL
DEFAULT 0;

END

/*
	6/15/2020 [ranzalon] - BOEJ-4634 - New Revision 

	## END ##
*/

/*
	## START ##

	6/23/2020 [Dusan]	BOEJ-4626 Add Certification Not Required
*/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RateTypeLU]') AND type in (N'U'))
	BEGIN
		CREATE TABLE ReasonCertificationNotRequiredLU (
			Id		INT				PRIMARY KEY,
			Text	VARCHAR(50)		NOT NULL
		);
	END
GO

IF NOT EXISTS (SELECT 1 FROM ReasonCertificationNotRequiredLU)
	BEGIN
		INSERT INTO ReasonCertificationNotRequiredLU
			VALUES	(1, 'Lost / Not Awarded'),
					(2, 'Awarded Under Threshold'),
					(3, 'Other');
	END
GO

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'dbo' AND T.name = 'Proposal' AND C.name = 'OtherReasonComment')
	BEGIN
		ALTER TABLE Proposal ADD OtherReasonComment VARCHAR(1000);
	END
GO

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'dbo' AND T.name = 'Proposal' AND C.name = 'ReasonCertificationNotRequired')
	BEGIN
		ALTER TABLE Proposal ADD ReasonCertificationNotRequired INT REFERENCES ReasonCertificationNotRequiredLU(Id)
	END
GO
/*
	6/23/2020 [Dusan]	BOEJ-4626 Add Certification Not Required

	## END ##
*/