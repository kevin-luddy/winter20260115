/*
	## START ##
	
	9/7/2017 [twilson3] BOEJ-2459 Add File handling Tables
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AttachmentTypeLU]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[AttachmentTypeLU](
		[ID] [int] NOT NULL,
		[Name] [varchar](100) NOT NULL,
	 CONSTRAINT [PK_AttachmentType] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	INSERT INTO [AttachmentTypeLU] ([ID], [Name]) VALUES (1, 'Cost Kick-Off Package');
	INSERT INTO [AttachmentTypeLU] ([ID], [Name]) VALUES (2, 'Responsibility Assignments Matrix (RAM)');
	INSERT INTO [AttachmentTypeLU] ([ID], [Name]) VALUES (3, 'Executive Planning Panel (EPP)');

END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Attachment]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Attachment](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[UpdateDate] [datetime2](7) NOT NULL,
		[Name] [varchar](100) NULL,
		[Contents] [varbinary](max) NOT NULL,
		[UploadedBy] [varchar](1000) NOT NULL,
		[AttachmentType] [int] NOT NULL,
		[ProposalID] [int] NOT NULL,
	 CONSTRAINT [PK_Attachment] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[Attachment]  WITH CHECK ADD  CONSTRAINT [FK_Proposal_Attachment] FOREIGN KEY([ProposalID])
	REFERENCES [dbo].[Proposal] ([ProposalID])

	ALTER TABLE [dbo].[Attachment]  WITH CHECK ADD  CONSTRAINT [FK_AttachmentType] FOREIGN KEY([AttachmentType])
	REFERENCES [dbo].[AttachmentTypeLU] ([ID])

	CREATE NONCLUSTERED INDEX [IX_Attachment_ProposalID] ON [dbo].[Attachment] 
	(
		[ProposalID]   ASC
	) 
	ON [PRIMARY]
END
GO

/*
	9/7/2017 [twilson3] BOEJ-2459 Add File handling Tables

	## END ##
*/

/*
	## START ##

	9/21/2017 [Dusan] BOEJ-2558 Add new PAs into LOBs
*/

IF(NOT EXISTS(SELECT 1 FROM LineOfBusiness WHERE ProductLineId = 13 AND LineOfBusinessName = 'Space Protection'))
BEGIN
INSERT INTO LineOfBusiness (LineOfBusinessName, LineOfBusinessLongName, LineOfBusinessURL, ProductLineID, ForesightLineOfBusinessID, IsActive)
	VALUES
	('Space Protection', 'Space Protection', 'SP', 13, -1, 1),
	('Ops Sustainment & Logistics', 'Ops Sustainment & Logistics', 'OSL', 13, -1, 1),
	('New Business', 'New Business', 'NB_1',  9, -1, 1),
	('New Business', 'New Business', 'NB_2', 10, -1, 1),
	('New Business', 'New Business', 'NB_3', 11, -1, 1),
	('New Business', 'New Business', 'NB_4', 12, -1, 1),
	('New Business', 'New Business', 'NB_5', 13, -1, 1),
	('New Business', 'New Business', 'NB_6', 14, -1, 1),
	('New Business', 'New Business', 'NB_7', 15, -1, 1),
	('New Business', 'New Business', 'NB_8', 16, -1, 1),
	('New Business', 'New Business', 'NB_9', 17, -1, 1)
END
GO

/*
	9/21/2017 [Dusan] BOEJ-2558 Add new PAs into LOBs

	## END ##
*/

/*
	## START ##
	11/10/17 [pattoncr] - BOEJ-2107 Remove Proposal Log Report By Latest Version
*/

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateProposalLogReportByLatestVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreateProposalLogReportByLatestVersion];

GO

/*
	11/10/17 [pattoncr] - BOEJ-2107 Remove Proposal Log Report By Latest Version
	## END ##
*/

/*
	## START ##
	11/10/17 [twilson3] - BOEJ-2794 Fix PAR checklist typo
*/

UPDATE [dbo].[PARChecklistContent] SET [ChecklistText] = N'<p>Subcontractor Proposals (If S/C proposal >= $13.5M or if S/C proposal > $750K and 10% of the Prime Proposal price) Must be included with proposal or include statement how the subcontracts are submitted. </p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_16.docx'' target=''_blank''>Additional Instructions</a></p>' WHERE [ChecklistText] = N'<p>Subcontractor Proposals (If S/C proposal ? $13.5M or if S/C proposal ? $750K and 10% of the Prime Proposal price) Must be included with proposal or include statement how the subcontracts are submitted. </p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_16.docx'' target=''_blank''>Additional Instructions</a></p>'
	
/*
	11/10/17 [twilson3] - BOEJ-2794 Fix PAR checklist typo
	## END ##
*/	

/*
	## START ##
	11/29/17 [pattoncr] - BOEJ-2839 AD utils - NTID uniqueness - Step 2 (Remove Domain)
*/

-- Drop and recreate constraint (to remove NTDomain reference).
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[genTRACUser]') AND name = N'UK_genTRACUser')
ALTER TABLE [dbo].[genTRACUser] DROP CONSTRAINT [UK_genTRACUser]
GO

ALTER TABLE [dbo].[genTRACUser] ADD CONSTRAINT [UK_genTRACUser] UNIQUE NONCLUSTERED 
(
	[NTID] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

-- Drop column from genTRACUser.
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'NTDomain' AND Object_ID = Object_ID(N'[dbo].[genTRACUser]'))
ALTER TABLE dbo.[genTRACUser] DROP COLUMN NTDomain
GO

/*
	11/29/17 [pattoncr] - BOEJ-2839 AD utils - NTID uniqueness - Step 2 (Remove Domain)
	## END ##
*/