EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2020.1';
GO

/*
		## START ##
		12/4/19		twilson3	BOEJ-4429 - Manage RTE Templates
*/

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RteTemplate]') AND type in (N'U'))
BEGIN
	--Create Workspace Rte Template tables
	CREATE TABLE [dbo].[RteTemplateSource](
		[RteTemplateSourceId] [int] IDENTITY(1,1) NOT NULL,
		[Description] varchar(50),
		[TaskOnly] bit NOT NULL
	CONSTRAINT [PK_RteTemplateSource] PRIMARY KEY CLUSTERED 
	(
		[RteTemplateSourceId] ASC
	)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
	) ON [PRIMARY]


	SET IDENTITY_INSERT [dbo].[RteTemplateSource] ON
	INSERT INTO [dbo].[RteTemplateSource] ([RteTemplateSourceId], [Description], [TaskOnly]) VALUES (1, 'BOE Description', 0)
	INSERT INTO [dbo].[RteTemplateSource] ([RteTemplateSourceId], [Description], [TaskOnly]) VALUES (2, 'BOE Sources', 0)
	INSERT INTO [dbo].[RteTemplateSource] ([RteTemplateSourceId], [Description], [TaskOnly]) VALUES (3, 'Task Description', 1)
	INSERT INTO [dbo].[RteTemplateSource] ([RteTemplateSourceId], [Description], [TaskOnly]) VALUES (4, 'Task MOQ Rationale', 1)

	SET IDENTITY_INSERT [dbo].[RteTemplateSource] OFF

	CREATE TABLE [dbo].[RteTemplate](
		[TemplateID] [int] IDENTITY(1,1) NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] [int] NOT NULL,
		[Description] [varchar](200) NOT NULL,
		[AuthorID] [int] NOT NULL,
		[CreatedOn] [datetime2](7) NOT NULL
	CONSTRAINT [PK_RteTemplate] PRIMARY KEY CLUSTERED 
	(
		[TemplateID] ASC
	)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[RteTemplate]  WITH NOCHECK ADD  CONSTRAINT [FK_RteTemplate_ETIuser] FOREIGN KEY([AuthorID])
	REFERENCES [dbo].[ETIuser] ([ETIUserID])

	ALTER TABLE [dbo].[RteTemplate]  WITH CHECK ADD  CONSTRAINT [FK_RteTemplate_Workspace] FOREIGN KEY([WorkspaceID])
	REFERENCES [dbo].[Workspace] ([WorkspaceID])

	CREATE NONCLUSTERED INDEX [IX_RteTemplate_WorkspaceID] ON [dbo].[RteTemplate] 
	(
		[WorkspaceID]   ASC
	) 
	ON [PRIMARY]

	CREATE TABLE [dbo].[RteTemplateAssigned](
		[TemplateID] [int] NOT NULL,
		[RteTemplateSourceId] [int] NOT NULL
	CONSTRAINT [PK_RteTemplateAssigned] PRIMARY KEY CLUSTERED 
	(
		[TemplateID] ASC,
		[RteTemplateSourceId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[RteTemplateAssigned]  WITH NOCHECK ADD  CONSTRAINT [FK_RteTemplateAssigned_TemplateID] FOREIGN KEY([TemplateID])
	REFERENCES [dbo].[RteTemplate] ([TemplateID])

	ALTER TABLE [dbo].[RteTemplateAssigned]  WITH NOCHECK ADD  CONSTRAINT [FK_RteTemplateAssigned_RteTemplateSourceId] FOREIGN KEY([RteTemplateSourceId])
	REFERENCES [dbo].[RteTemplateSource] ([RteTemplateSourceId])
	
	CREATE TABLE [dbo].[RteTemplateQuestion](
		[QuestionID] [int] IDENTITY(1,1) NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[TemplateID] [int] NOT NULL,
		[Text] [varchar](500) NOT NULL,
		[SortOrder] [int] NOT NULL,
		[Required] [bit] NOT NULL
		
	 CONSTRAINT [PK_RteTemplateQuestion] PRIMARY KEY CLUSTERED 
	(
		[QuestionID] ASC
	)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[RteTemplateQuestion]  WITH CHECK ADD  CONSTRAINT [FK_RteTemplateQuestion_TemplateID] FOREIGN KEY([TemplateID])
	REFERENCES [dbo].[RteTemplate] ([TemplateID])

	CREATE NONCLUSTERED INDEX [IX_RteTemplateQuestion_TemplateID] ON [dbo].[RteTemplateQuestion] 
	(
		[TemplateID]   ASC
	) 
	ON [PRIMARY]

	CREATE TABLE [dbo].[RteTemplateAnswer](
		[AnswerID] [int] IDENTITY(1,1) NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[QuestionID] [int] NOT NULL,
		[BOEID] [int] NOT NULL,
		[TaskID] [int] NULL,
		[Text] [varchar](max) NOT NULL,
		[RteTemplateSourceId] int NOT NULL
		
	 CONSTRAINT [PK_RteTemplateAnswer] PRIMARY KEY CLUSTERED 
	(
		[AnswerID] ASC
	)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[RteTemplateAnswer]  WITH CHECK ADD  CONSTRAINT [FK_RteTemplateAnswer_QuestionID] FOREIGN KEY([QuestionID])
	REFERENCES [dbo].[RteTemplateQuestion] ([QuestionID])

	ALTER TABLE [dbo].[RteTemplateAnswer]  WITH CHECK ADD  CONSTRAINT [FK_RteTemplateAnswer_BoeID] FOREIGN KEY([BoeID])
	REFERENCES [dbo].[BOE] ([BOEID])

	ALTER TABLE [dbo].[RteTemplateAnswer]  WITH CHECK ADD  CONSTRAINT [FK_RteTemplateAnswer_TaskID] FOREIGN KEY([TaskID])
	REFERENCES [dbo].[BOETaskElement] ([BOETaskElementID])

	CREATE NONCLUSTERED INDEX [IX_RteTemplateAnswer_BoeID] ON [dbo].[RteTemplateAnswer] 
	(
		[BoeID]   ASC
	) 
	ON [PRIMARY]

	CREATE TABLE [version].[RteTemplate](
		[TemplateID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] [int] NOT NULL,
		[Description] [varchar](200) NOT NULL,
		[AuthorID] [int] NOT NULL,
		[CreatedOn] [datetime2](7) NOT NULL,
		VersionID int not null,
	 )

	 CREATE TABLE [version].[RteTemplateAssigned](
		[TemplateID] [int] NOT NULL,
		[RteTemplateSourceId] [int] NOT NULL,
		VersionID int not null
	 )

	CREATE TABLE [version].[RteTemplateQuestion](
		[QuestionID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[TemplateID] [int] NOT NULL,
		[Text] [varchar](500) NOT NULL,
		[SortOrder] [int] NOT NULL,
		[Required] [bit] NOT NULL,
		VersionID int not null,
	)

	CREATE TABLE [version].[RteTemplateAnswer](
		[AnswerID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[QuestionID] [int] NOT NULL,
		[BOEID] [int] NOT NULL,
		[TaskID] [int] NULL,
		[Text] [varchar](max) NOT NULL,
		[RteTemplateSourceId] int NOT NULL, 
		VersionID int not null,
	)

END

GO

/*
       12/4/19		twilson3	BOEJ-4429 - Manage RTE Templates
       ## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2020.1';
GO

/*
		## START ##
		5/8/2020		Dusan		BOEJ-4479 - Add emails to RTE templates
*/

IF NOT EXISTS (SELECT 1 FROM EmailLU WHERE EmailId IN (49, 50, 51))
	BEGIN
		INSERT 
			INTO EmailLU
			(EmailId, "Subject", Body, "Trigger", DefaultOn, ForcedOn, Category, Recipient)
			VALUES 
			(49, 'genBOE: In-Use RTE Custom Template was unassigned', 'A Custom Template, assigned to one of the following: BOE Description, Source of Data, Task Description, or MOQ Rationale, has been unassigned (removed from use) in workspace {0}. Please review all of your BOEs / Tasks in the workspace.', 'In-use template was unassigned', 0, 0, 'RTE Custom Templates', 'BOE Author(s)'), 
			(50, 'genBOE: RTE Custom Template was assigned', 'A Custom Template, assigned to one of the following: BOE Description, Source of Data, Task Description, or MOQ Rationale, has been reassigned to another custom template in workspace {0}. Please review all of your BOEs / Tasks in the workspace.', 'Template was assigned when workspace was not in initialization', 0, 0, 'RTE Custom Templates', 'BOE Author(s)'), 
			(51, 'genBOE: In-Use RTE Custom Template''s prompt was deleted', 'A Custom Template, assigned to one of the following: BOE Description, Source of Data, Task Description, or MOQ Rationale, had one of its prompts deleted by a Workspace Administrator, in workspace {0}. Please review all of your BOEs / Tasks in the workspace.', 'In-use template''s prompt is deleted', 0, 0, 'RTE Custom Templates', 'BOE Author(s)');
	END
ELSE
	BEGIN
		UPDATE EmailLU SET Body = 'A Custom Template, assigned to one of the following: BOE Description, Source of Data, Task Description, or MOQ Rationale, has been unassigned (removed from use) in workspace {0}. Please review all of your BOEs / Tasks in the workspace.'
					WHERE EmailId = 49;
		UPDATE EmailLU SET Body = 'A Custom Template, assigned to one of the following: BOE Description, Source of Data, Task Description, or MOQ Rationale, has been reassigned to another custom template in workspace {0}. Please review all of your BOEs / Tasks in the workspace.'
					WHERE EmailId = 50;
		UPDATE EmailLU SET Body = 'A Custom Template, assigned to one of the following: BOE Description, Source of Data, Task Description, or MOQ Rationale, had one of its prompts deleted by a Workspace Administrator, in workspace {0}. Please review all of your BOEs / Tasks in the workspace.'
					WHERE EmailId = 51;
	END
GO
/*
		5/8/2020		Dusan		BOEJ-4479 - Add emails to RTE templates
		## END ##
*/

/*
		## START ##
		5/13/2020		Dusan		BOEJ-4612 - Fix a name of a template source
*/

UPDATE [RteTemplateSource] SET [Description] = 'BOE Sources of Data' WHERE Description = 'BOE Sources';
GO

/*
		5/13/2020		Dusan		BOEJ-4612 - Fix a name of a template source
		## END ##
*/


/*
		## START ##
		5/21/2020		Dusan		BOEJ-4620 - Update some text
*/
UPDATE [RteTemplateSource] SET [Description] = 'MOQ Rationale' WHERE Description = 'Task MOQ Rationale';
UPDATE [RteTemplateSource] SET [Description] = 'Sources of Data' WHERE Description = 'BOE Sources of Data';
GO
/*
		5/21/2020		Dusan		BOEJ-4620 - Update some text
		## END ##
*/
