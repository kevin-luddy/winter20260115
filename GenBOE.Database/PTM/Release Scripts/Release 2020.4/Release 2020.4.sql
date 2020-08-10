EXEC dbo.[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2020.4';
GO

/*
	## START ##

	6/15/2020 [ranzalon] - BOEJ-4634 - New Revision 
*/

IF NOT EXISTS (SELECT 1 
		FROM dbo.[ProposalStatusLU]
		WHERE [ProposalStatus] = 'Revised')
BEGIN
SET IDENTITY_INSERT dbo.[ProposalStatusLU] ON

INSERT INTO dbo.[ProposalStatusLU]
(ProposalStatusID, ProposalStatus)
VALUES (8, 'Revised');

SET IDENTITY_INSERT dbo.[ProposalStatusLU] OFF
END

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'dbo' AND 
	T.name = 'Proposal' AND C.name = 'RevisionOfId')
BEGIN 

ALTER TABLE dbo.[Proposal]
ADD [RevisionOfId] int NULL;

ALTER TABLE dbo.[Proposal]
ADD CONSTRAINT [FK_Proposal_Proposal]
FOREIGN KEY ([RevisionOfId]) REFERENCES dbo.[Proposal]([ProposalID]);

ALTER TABLE dbo.[Proposal]
CHECK CONSTRAINT [FK_Proposal_Proposal];

END

/*
	6/15/2020 [ranzalon] - BOEJ-4634 - New Revision 

	## END ##
*/

/*
	## START ##

	6/23/2020 [Dusan]	BOEJ-4626 Add Certification Not Required
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReasonCertificationNotRequiredLU]') AND type in (N'U'))
	BEGIN
		CREATE TABLE dbo.ReasonCertificationNotRequiredLU (
			Id		INT				PRIMARY KEY,
			Text	VARCHAR(50)		NOT NULL
		);
	END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ReasonCertificationNotRequiredLU)
	BEGIN
		INSERT INTO dbo.ReasonCertificationNotRequiredLU
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
		ALTER TABLE Proposal ADD ReasonCertificationNotRequired INT REFERENCES dbo.ReasonCertificationNotRequiredLU(Id)
	END
GO
/*
	6/23/2020 [Dusan]	BOEJ-4626 Add Certification Not Required

	## END ##
*/

/*
	## START ##

	7/1/2020 [Dusan]	BOEJ-4638 Post submittal attachments working with Revisioned Proposal
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProposalsAttachments]') AND type in (N'U'))
BEGIN
	CREATE TABLE dbo.ProposalsAttachments (
		ProposalId			INT		REFERENCES dbo.Proposal(ProposalId)		NOT NULL,
		AttachmentId		INT		REFERENCES dbo.Attachment(Id)			NOT NULL,
		AttachmentType		INT		REFERENCES dbo.AttachmentTypeLU(Id)		NOT NULL,
		IsRevisionReference	BIT		NOT NULL
	);
	ALTER TABLE dbo.ProposalsAttachments ADD CONSTRAINT PK_ProposalsAttachments PRIMARY KEY (ProposalId, AttachmentId);
END
GO

BEGIN TRY
	BEGIN TRAN AttachmentDataMigration
		INSERT INTO dbo.ProposalsAttachments(ProposalId, AttachmentId, AttachmentType, IsRevisionReference)
			SELECT ProposalId, Id, AttachmentType, 0 FROM dbo.Attachment;

		DROP INDEX Attachment.[IX_Attachment_ProposalID];
		ALTER TABLE dbo.Attachment DROP [FK_Proposal_Attachment];
		ALTER TABLE dbo.Attachment DROP [FK_AttachmentType];
		ALTER TABLE dbo.Attachment DROP COLUMN ProposalId;
		ALTER TABLE dbo.Attachment DROP COLUMN AttachmentType;
	COMMIT TRAN AttachmentDataMigration;
END TRY
BEGIN CATCH
	ROLLBACK TRAN AttachmentDataMigration;
	PRINT '!!! FAILED AttachmentDataMigration !!!';
END CATCH
GO

/*
	7/1/2020 [Dusan]	BOEJ-4638 Post submittal attachments working with Revisioned Proposal

	## END ##
*/

/*
	## START ##

	7/31/2020 [ranzalon] - BOEJ-4648 - Comments for Proposal Setup
*/

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'dbo' AND 
	T.name = 'Proposal' AND C.name = 'SetupComments')
BEGIN 

ALTER TABLE dbo.[Proposal]
ADD [SetupComments] VARCHAR(max) NULL;

END

/*
	7/31/2020 [ranzalon] - BOEJ-4648 - Comments for Proposal Setup

	## END ##
*/